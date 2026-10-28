using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

[AllowAnonymous]
public class ProductAppService :
    CrudAppService<
        Product,
        ProductDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateProductDto>,
    IProductAppService
{
    // 1. Usamos el constructor base de CrudAppService
    public ProductAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
    }

    // 2. Sobrescribimos CreateAsync para usar tu constructor de Dominio
    public override async Task<ProductDto> CreateAsync(CreateUpdateProductDto input)
    {
        var product = new Product(GuidGenerator.Create(), input.Name, input.Maker, input.Barcode);
        await Repository.InsertAsync(product);
        return ObjectMapper.Map<Product, ProductDto>(product);
    }

    // 3. Sobrescribimos UpdateAsync para usar tu método Update de Dominio (Evita estado parcial)
    public override async Task<ProductDto> UpdateAsync(Guid id, CreateUpdateProductDto input)
    {
        var product = await Repository.GetAsync(id);

        product.Update(input.Name, input.Maker, input.Barcode);

        await Repository.UpdateAsync(product);
        return ObjectMapper.Map<Product, ProductDto>(product);
    }
}