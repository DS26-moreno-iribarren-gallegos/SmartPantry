using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Xunit;

namespace SmartPantry.Products;

public abstract class ProductAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : Volo.Abp.Modularity.IAbpModule
{
    private readonly IProductAppService _productAppService;

    protected ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
    }

    [Fact]
    public async Task Should_Perform_Complete_CRUD_Operations()
    {
        // 1. Registrar
        var createDto = new CreateUpdateProductDto { Name = "Test", Maker = "M", Barcode = "1" };
        var created = await _productAppService.CreateAsync(createDto);
        created.Id.ShouldNotBe(Guid.Empty);

        // 2. Listar
        var list = await _productAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        list.Items.ShouldContain(p => p.Id == created.Id);

        // 3. Consultar individual
        var fetched = await _productAppService.GetAsync(created.Id);
        fetched.Name.ShouldBe("Test");

        // 4. Modificar
        var updateDto = new CreateUpdateProductDto { Name = "Updated", Maker = "M", Barcode = "1" };
        var updated = await _productAppService.UpdateAsync(created.Id, updateDto);
        updated.Name.ShouldBe("Updated");

        // 5. Eliminar
        await _productAppService.DeleteAsync(created.Id);

        await Assert.ThrowsAnyAsync<EntityNotFoundException>(
            async () => await _productAppService.GetAsync(created.Id)
        );
    }
}