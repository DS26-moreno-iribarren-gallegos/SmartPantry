using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;
using SmartPantry.Pantry;

namespace SmartPantry.EntityFrameworkCore.Pantry
{
    public class PantryExpirationManager_Tests : SmartPantryEntityFrameworkCoreTestBase
    {
        private readonly PantryExpirationManager _manager;
        private readonly IRepository<PantryItem, Guid> _itemRepository;
        private readonly IRepository<ExpirationWarning, Guid> _warningRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public PantryExpirationManager_Tests()
        {
            _manager = GetRequiredService<PantryExpirationManager>();
            _itemRepository = GetRequiredService<IRepository<PantryItem, Guid>>();
            _warningRepository = GetRequiredService<IRepository<ExpirationWarning, Guid>>();
            _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
        }

        // 1. REQUERIMIENTO: Prueba explícita para un producto consumido
        [Fact]
        public async Task Should_Deactivate_Warning_When_Item_Is_Consumed()
        {
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var referenceDate = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                // Arrange: Producto en estado Consumed con una advertencia previa activa
                var item = new PantryItem(itemId, userId, "123456", PantryItemStatus.Consumed, referenceDate.AddDays(-1));
                await _itemRepository.InsertAsync(item, autoSave: true);

                var warning = new ExpirationWarning(Guid.NewGuid(), itemId, item.ExpirationDate.Value, true);
                await _warningRepository.InsertAsync(warning, autoSave: true);

                // Act: Procesar advertencias con la fecha de referencia
                await _manager.ProcessExpirationsAsync(referenceDate);

                // Assert: La advertencia debe desactivarse (IsActive = false) al dejar de estar Disponible
                var updatedWarning = await _warningRepository.FirstOrDefaultAsync(w => w.PantryItemId == itemId);
                updatedWarning.ShouldNotBeNull();
                updatedWarning.Id.ShouldBe(warning.Id);
                updatedWarning.IsActive.ShouldBeFalse();

                await uow.CompleteAsync();
            }
        }

        [Fact]
        public async Task Should_Not_Create_Warning_When_Item_Is_Consumed()
        {
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var referenceDate = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                // Un producto consumido no debe generar advertencias, aunque esté vencido.
                var item = new PantryItem(itemId, userId, "CONSUMED-01", PantryItemStatus.Consumed, referenceDate.AddDays(-1));
                await _itemRepository.InsertAsync(item, autoSave: true);

                await _manager.ProcessExpirationsAsync(referenceDate);

                var warnings = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
                warnings.ShouldBeEmpty();

                await uow.CompleteAsync();
            }
        }

        // 2. REQUERIMIENTO: Actualización, desactivación y reactivación sobre una misma advertencia sin duplicados
        [Fact]
        public async Task Should_Update_Deactivate_And_Reactivate_Same_Warning_Without_Duplicates()
        {
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var referenceDate = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                // Paso 1: Ítem vence en 2 días (dentro del umbral) -> Se crea la advertencia inicial
                var item = new PantryItem(itemId, userId, "ITEM-01", PantryItemStatus.Available, referenceDate.AddDays(2));
                await _itemRepository.InsertAsync(item, autoSave: true);

                await _manager.ProcessExpirationsAsync(referenceDate);

                var warnings = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
                warnings.Count.ShouldBe(1);
                var originalWarningId = warnings[0].Id;
                warnings[0].IsActive.ShouldBeTrue();
                warnings[0].TargetExpirationDate.ShouldBe(referenceDate.AddDays(2));

                // Paso 2: Cambiar la fecha a 1 día -> Se actualiza la fecha en el registro existente
                item.ExpirationDate = referenceDate.AddDays(1);
                await _itemRepository.UpdateAsync(item, autoSave: true);

                await _manager.ProcessExpirationsAsync(referenceDate);

                warnings = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
                warnings.Count.ShouldBe(1);
                warnings[0].Id.ShouldBe(originalWarningId);
                warnings[0].IsActive.ShouldBeTrue();
                warnings[0].TargetExpirationDate.ShouldBe(referenceDate.AddDays(1));

                // Paso 3: Cambiar la fecha fuera del umbral (10 días) -> Se desactiva la advertencia
                item.ExpirationDate = referenceDate.AddDays(10);
                await _itemRepository.UpdateAsync(item, autoSave: true);

                await _manager.ProcessExpirationsAsync(referenceDate);

                warnings = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
                warnings.Count.ShouldBe(1);
                warnings[0].Id.ShouldBe(originalWarningId);
                warnings[0].IsActive.ShouldBeFalse();

                // Paso 4: Reactivación -> Vuelve a fecha dentro del umbral (2 días), reactiva la misma advertencia sin duplicar
                item.ExpirationDate = referenceDate.AddDays(2);
                await _itemRepository.UpdateAsync(item, autoSave: true);

                await _manager.ProcessExpirationsAsync(referenceDate);

                warnings = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
                warnings.Count.ShouldBe(1);
                warnings[0].Id.ShouldBe(originalWarningId);
                warnings[0].IsActive.ShouldBeTrue();
                warnings[0].TargetExpirationDate.ShouldBe(referenceDate.AddDays(2));

                await uow.CompleteAsync();
            }
        }

        // 3. COBERCTURA DE LÍMITES: Día límite (3 días), fuera de umbral, ya vencido y sin fecha (null)
        [Fact]
        public async Task Should_Handle_Boundary_Dates_And_Null_Expirations()
        {
            var userId = Guid.NewGuid();
            var referenceDate = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);

            var idExactLimit = Guid.NewGuid();       // Exactamente 3 días (límite del umbral de 3 días)
            var idOutsideThreshold = Guid.NewGuid(); // 5 días (fuera del umbral)
            var idAlreadyExpired = Guid.NewGuid();   // Vencido hace 2 días
            var idNoDate = Guid.NewGuid();           // Sin fecha de vencimiento (null)

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await _itemRepository.InsertAsync(new PantryItem(idExactLimit, userId, "222", PantryItemStatus.Available, referenceDate.AddDays(3)), autoSave: true);
                await _itemRepository.InsertAsync(new PantryItem(idOutsideThreshold, userId, "333", PantryItemStatus.Available, referenceDate.AddDays(5)), autoSave: true);
                await _itemRepository.InsertAsync(new PantryItem(idAlreadyExpired, userId, "444", PantryItemStatus.Available, referenceDate.AddDays(-2)), autoSave: true);
                await _itemRepository.InsertAsync(new PantryItem(idNoDate, userId, "555", PantryItemStatus.Available, null), autoSave: true);

                await _manager.ProcessExpirationsAsync(referenceDate);

                var warnings = await _warningRepository.GetListAsync();

                // Assert: Genera/mantiene advertencia activa para día límite y ya vencido; ignora fuera de umbral y sin fecha
                warnings.Any(w => w.PantryItemId == idExactLimit && w.IsActive).ShouldBeTrue();
                warnings.Any(w => w.PantryItemId == idAlreadyExpired && w.IsActive).ShouldBeTrue();
                warnings.Any(w => w.PantryItemId == idOutsideThreshold && w.IsActive).ShouldBeFalse();
                warnings.Any(w => w.PantryItemId == idNoDate && w.IsActive).ShouldBeFalse();

                await uow.CompleteAsync();
            }
        }

        // 4. CONCURRENCIA: Prueba de ejecuciones simultáneas e idempotencia concurrente
        [Fact]
        public async Task Should_Be_Idempotent_Under_Concurrent_Simultaneous_Executions()
        {
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var referenceDate = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                var item = new PantryItem(itemId, userId, "999", PantryItemStatus.Available, referenceDate.AddDays(1));
                await _itemRepository.InsertAsync(item, autoSave: true);
                await uow.CompleteAsync();
            }

            // Cada ejecución paralela asigna su propio IServiceScope para simular workers concurrentes reales
            var tasks = Enumerable.Range(0, 3).Select(async _ =>
            {
                using var scope = ServiceProvider.CreateScope();
                var manager = scope.ServiceProvider.GetRequiredService<PantryExpirationManager>();
                var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

                using var uow = uowManager.Begin(requiresNew: true);
                await manager.ProcessExpirationsAsync(referenceDate);
                await uow.CompleteAsync();
            });

            await Task.WhenAll(tasks);

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                // El índice único físico en BD y la lógica del manager aseguran exactamente 1 advertencia activa
                var warnings = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
                warnings.Count.ShouldBe(1);
                warnings[0].IsActive.ShouldBeTrue();

                await uow.CompleteAsync();
            }
        }
    }
}
