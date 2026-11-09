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

        [Fact]
        public async Task Should_Handle_All_Expiration_Scenarios_And_Reactivation()
        {
            var userId = Guid.NewGuid();
            var referenceDate = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);

            var idWithinThreshold = Guid.NewGuid();  // Vence en 2 días (dentro del umbral)
            var idExactLimit = Guid.NewGuid();       // Vence en exactamente 3 días (límite)
            var idOutsideThreshold = Guid.NewGuid(); // Vence en 5 días (fuera del umbral)
            var idAlreadyExpired = Guid.NewGuid();   // Vencido hace 2 días
            var idNoDate = Guid.NewGuid();           // Sin fecha (null)

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await _itemRepository.InsertAsync(new PantryItem(idWithinThreshold, userId, "111", PantryItemStatus.Available, referenceDate.AddDays(2)), autoSave: true);
                await _itemRepository.InsertAsync(new PantryItem(idExactLimit, userId, "222", PantryItemStatus.Available, referenceDate.AddDays(3)), autoSave: true);
                await _itemRepository.InsertAsync(new PantryItem(idOutsideThreshold, userId, "333", PantryItemStatus.Available, referenceDate.AddDays(5)), autoSave: true);
                await _itemRepository.InsertAsync(new PantryItem(idAlreadyExpired, userId, "444", PantryItemStatus.Available, referenceDate.AddDays(-2)), autoSave: true);
                await _itemRepository.InsertAsync(new PantryItem(idNoDate, userId, "555", PantryItemStatus.Available, null), autoSave: true);

                // Act 1: Procesar vencimientos
                await _manager.ProcessExpirationsAsync(referenceDate);

                var warnings = await _warningRepository.GetListAsync();

                // Assert 1: Solo 3 activos (dentro del umbral, día límite y ya vencido)
                warnings.Count(w => w.IsActive).ShouldBe(3);
                warnings.Any(w => w.PantryItemId == idWithinThreshold && w.IsActive).ShouldBeTrue();
                warnings.Any(w => w.PantryItemId == idExactLimit && w.IsActive).ShouldBeTrue();
                warnings.Any(w => w.PantryItemId == idAlreadyExpired && w.IsActive).ShouldBeTrue();
                warnings.Any(w => w.PantryItemId == idOutsideThreshold).ShouldBeFalse();
                warnings.Any(w => w.PantryItemId == idNoDate).ShouldBeFalse();

                // Act 2: Probar cambio de fecha, desactivación y reactivación
                // El ítem fuera del umbral ahora pasa a vencer mañana (reactiva/crea advertencia)
                var outsideItem = await _itemRepository.GetAsync(idOutsideThreshold);
                outsideItem.ExpirationDate = referenceDate.AddDays(1);
                await _itemRepository.UpdateAsync(outsideItem, autoSave: true);

                // El ítem dentro del umbral pasa a vencer a 10 días (se desactiva)
                var withinItem = await _itemRepository.GetAsync(idWithinThreshold);
                withinItem.ExpirationDate = referenceDate.AddDays(10);
                await _itemRepository.UpdateAsync(withinItem, autoSave: true);

                await _manager.ProcessExpirationsAsync(referenceDate);

                var updatedWarnings = await _warningRepository.GetListAsync();

                // Assert 2
                updatedWarnings.Single(w => w.PantryItemId == idOutsideThreshold).IsActive.ShouldBeTrue();
                updatedWarnings.Single(w => w.PantryItemId == idWithinThreshold).IsActive.ShouldBeFalse();

                await uow.CompleteAsync();
            }
        }

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

            // Cada ejecución paralela usa su propio scope aislado para simular workers concurrentes
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
                var warnings = await _warningRepository.GetListAsync(w => w.PantryItemId == itemId);
                warnings.Count.ShouldBe(1);
                warnings[0].IsActive.ShouldBeTrue();

                await uow.CompleteAsync();
            }
        }
    }
}