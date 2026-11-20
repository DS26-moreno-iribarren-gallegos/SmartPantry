using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.Products;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Security.Claims;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Pantry;

public abstract class PantryItemAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : Volo.Abp.Modularity.IAbpModule
{
    private readonly IPantryItemAppService _pantryItemAppService;
    private readonly IProductAppService _productAppService;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    protected PantryItemAppService_Tests()
    {
        _pantryItemAppService = GetRequiredService<IPantryItemAppService>();
        _productAppService = GetRequiredService<IProductAppService>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable LoginAs(Guid userId)
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(AbpClaimTypes.UserId, userId.ToString()) },
                "TestAuth"
            )
        );

        return _currentPrincipalAccessor.Change(principal);
    }

    [Fact]
    public async Task Should_Manage_Own_Pantry_Items_And_Resolve_Product_Name()
    {
        var userAId = Guid.NewGuid();
        var expiration = DateTime.UtcNow.Date.AddDays(10);

        using (LoginAs(userAId))
        {
            // Registramos un producto en el catálogo para comprobar el cruce por barcode
            await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                Name = "Leche Entera 1L",
                Maker = "La Serenísima",
                Barcode = "7790742000105"
            });

            // 1. Crear ítem con producto conocido y otro sin producto en catálogo (RF-09)
            var createdKnown = await _pantryItemAppService.CreateAsync(new CreatePantryItemDto
            {
                ProductBarcode = "7790742000105",
                Quantity = 2,
                Unit = "litros",
                ExpirationDate = expiration
            });

            var createdUnknown = await _pantryItemAppService.CreateAsync(new CreatePantryItemDto
            {
                ProductBarcode = "9999999999999"
            });

            // 2. Listar ítems propios
            var list = await _pantryItemAppService.GetListAsync(new PagedAndSortedResultRequestDto());
            list.TotalCount.ShouldBe(2);

            var knownItem = list.Items.FirstOrDefault(i => i.Id == createdKnown.Id);
            knownItem.ShouldNotBeNull();
            knownItem.ProductName.ShouldBe("Leche Entera 1L");
            knownItem.Quantity.ShouldBe(2);
            knownItem.Unit.ShouldBe("litros");

            var unknownItem = list.Items.FirstOrDefault(i => i.Id == createdUnknown.Id);
            unknownItem.ShouldNotBeNull();
            unknownItem.ProductName.ShouldBeNull();
            unknownItem.Quantity.ShouldBeNull();
            unknownItem.Unit.ShouldBeNull();

            // 3. Actualizar cantidad, unidad y vencimiento
            var newExpiration = expiration.AddDays(5);
            var updated = await _pantryItemAppService.UpdateAsync(createdKnown.Id, new UpdatePantryItemDto
            {
                Quantity = 1.5m,
                Unit = "L",
                ExpirationDate = newExpiration
            });
            updated.Quantity.ShouldBe(1.5m);
            updated.Unit.ShouldBe("L");
            updated.ExpirationDate.ShouldBe(newExpiration);

            // 4. Marcar como consumido
            var consumed = await _pantryItemAppService.MarkAsConsumedAsync(createdKnown.Id);
            consumed.Status.ShouldBe(PantryItemStatus.Consumed);

            // 5. Eliminar ítem
            await _pantryItemAppService.DeleteAsync(createdUnknown.Id);
            await Assert.ThrowsAnyAsync<EntityNotFoundException>(
                async () => await _pantryItemAppService.GetAsync(createdUnknown.Id)
            );
        }
    }

    [Fact]
    public async Task Should_Reject_Negative_Quantity_On_Update()
    {
        var userAId = Guid.NewGuid();

        using (LoginAs(userAId))
        {
            var created = await _pantryItemAppService.CreateAsync(new CreatePantryItemDto
            {
                ProductBarcode = "7790742000105",
                Quantity = 1,
                Unit = "unidad"
            });

            await Assert.ThrowsAsync<AbpValidationException>(async () =>
                await _pantryItemAppService.UpdateAsync(created.Id, new UpdatePantryItemDto
                {
                    Quantity = -3,
                    Unit = "unidad"
                })
            );
        }
    }

    [Fact]
    public async Task Should_Isolate_Items_Between_Different_Users()
    {
        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();
        Guid userAItemId;

        using (LoginAs(userAId))
        {
            var created = await _pantryItemAppService.CreateAsync(new CreatePantryItemDto
            {
                ProductBarcode = "7790742000105",
                Quantity = 3,
                Unit = "paquetes"
            });
            userAItemId = created.Id;
        }

        // Usuario B intenta listar, consultar, editar, consumir o eliminar el ítem de Usuario A
        using (LoginAs(userBId))
        {
            var userBList = await _pantryItemAppService.GetListAsync(new PagedAndSortedResultRequestDto());
            userBList.Items.ShouldNotContain(i => i.Id == userAItemId);

            await Assert.ThrowsAsync<AbpAuthorizationException>(
                async () => await _pantryItemAppService.GetAsync(userAItemId)
            );

            await Assert.ThrowsAsync<AbpAuthorizationException>(
                async () => await _pantryItemAppService.UpdateAsync(userAItemId, new UpdatePantryItemDto { Quantity = 10 })
            );

            await Assert.ThrowsAsync<AbpAuthorizationException>(
                async () => await _pantryItemAppService.MarkAsConsumedAsync(userAItemId)
            );

            await Assert.ThrowsAsync<AbpAuthorizationException>(
                async () => await _pantryItemAppService.DeleteAsync(userAItemId)
            );
        }
    }
}