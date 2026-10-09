using System;
using Volo.Abp.Application.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

[AllowAnonymous]
public class ProductAppService : ApplicationService, IProductAppService
{
    private readonly IRepository<Product, Guid> _productRepository;

    public async Task<ProductDto> GetAsync(Guid id)
    {
        var product = await _productRepository.GetAsync(id);
        
        return ObjectMapper.Map<Product, ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product(GuidGenerator.Create(), dto.Name, dto.Maker, dto.Barcode);
        await _productRepository.InsertAsync(product);
    
        return ObjectMapper.Map<Product,ProductDto>(product);
    }

    public ProductAppService(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }
    
}
