using System;
using System.Threading.Tasks;
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
        public async Task Should_Create_Warning_And_Be_Idempotent()
        {
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var referenceDate = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);
            var expirationDate = referenceDate.AddDays(2);

            // Usamos una única Unidad de Trabajo que abarque toda la prueba para evitar que el DbContext muera
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                // Arrange
                var item = new PantryItem(itemId, userId, "123456", PantryItemStatus.Available, expirationDate);
                await _itemRepository.InsertAsync(item, autoSave: true);

                // Act 1: Primera ejecución
                await _manager.ProcessExpirationsAsync(referenceDate);

                // Assert 1
                var warnings = await _warningRepository.GetListAsync();
                warnings.Count.ShouldBe(1);
                warnings[0].PantryItemId.ShouldBe(itemId);
                warnings[0].IsActive.ShouldBeTrue();

                // Act 2: Idempotencia (Segunda ejecución sobre los mismos datos)
                await _manager.ProcessExpirationsAsync(referenceDate);

                // Assert 2: No debe duplicarse
                warnings = await _warningRepository.GetListAsync();
                warnings.Count.ShouldBe(1);

                await uow.CompleteAsync();
            }
        }

        [Fact]
        public async Task Should_Deactivate_Warning_When_Item_Is_Consumed()
        {
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var referenceDate = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc);

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                // Arrange
                var item = new PantryItem(itemId, userId, "123456", PantryItemStatus.Consumed, referenceDate.AddDays(-1));
                await _itemRepository.InsertAsync(item, autoSave: true);

                var warning = new ExpirationWarning(Guid.NewGuid(), itemId, item.ExpirationDate.Value, true);
                await _warningRepository.InsertAsync(warning, autoSave: true);

                // Act
                await _manager.ProcessExpirationsAsync(referenceDate);

                // Assert
                var updatedWarning = await _warningRepository.FirstOrDefaultAsync(w => w.PantryItemId == itemId);
                updatedWarning.ShouldNotBeNull();
                updatedWarning.IsActive.ShouldBeFalse();

                await uow.CompleteAsync();
            }
        }
    }
}