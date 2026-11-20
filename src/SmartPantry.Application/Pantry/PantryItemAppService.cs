using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SmartPantry.Products;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace SmartPantry.Pantry;

[Authorize]
public class PantryItemAppService : ApplicationService, IPantryItemAppService
{
    private readonly IRepository<PantryItem, Guid> _pantryItemRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public PantryItemAppService(
        IRepository<PantryItem, Guid> pantryItemRepository,
        IRepository<Product, Guid> productRepository)
    {
        _pantryItemRepository = pantryItemRepository;
        _productRepository = productRepository;
    }

    public async Task<PagedResultDto<PantryItemDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var userId = CurrentUser.GetId();
        var queryable = await _pantryItemRepository.GetQueryableAsync();

        var userQuery = queryable.Where(x => x.UserId == userId);
        var totalCount = await AsyncExecuter.CountAsync(userQuery);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ||
                      input.Sorting.StartsWith("productName", StringComparison.OrdinalIgnoreCase)
            ? nameof(PantryItem.ExpirationDate)
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            userQuery
                .OrderBy(sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var barcodes = items
            .Select(x => x.ProductBarcode)
            .Distinct()
            .ToList();

        var productQueryable = await _productRepository.GetQueryableAsync();
        var products = await AsyncExecuter.ToListAsync(
            productQueryable.Where(p => barcodes.Contains(p.Barcode))
        );

        var productNamesByBarcode = products
            .GroupBy(p => p.Barcode)
            .ToDictionary(g => g.Key, g => g.First().Name);

        var dtos = items
            .Select(item => MapToDto(
                item,
                productNamesByBarcode.GetValueOrDefault(item.ProductBarcode)))
            .ToList();

        return new PagedResultDto<PantryItemDto>(totalCount, dtos);
    }

    public async Task<PantryItemDto> GetAsync(Guid id)
    {
        var item = await GetOwnedItemAsync(id);
        var productName = await FindProductNameAsync(item.ProductBarcode);
        return MapToDto(item, productName);
    }

    public async Task<PantryItemDto> CreateAsync(CreatePantryItemDto input)
    {
        var userId = CurrentUser.GetId();
        var item = new PantryItem(
            GuidGenerator.Create(),
            userId,
            input.ProductBarcode.Trim(),
            PantryItemStatus.Available,
            input.ExpirationDate,
            input.Quantity,
            input.Unit);

        await _pantryItemRepository.InsertAsync(item, autoSave: true);
        var productName = await FindProductNameAsync(item.ProductBarcode);
        return MapToDto(item, productName);
    }

    public async Task<PantryItemDto> UpdateAsync(Guid id, UpdatePantryItemDto input)
    {
        var item = await GetOwnedItemAsync(id);
        item.UpdateDetails(input.Quantity, input.Unit, input.ExpirationDate);
        await _pantryItemRepository.UpdateAsync(item, autoSave: true);

        var productName = await FindProductNameAsync(item.ProductBarcode);
        return MapToDto(item, productName);
    }

    public async Task<PantryItemDto> MarkAsConsumedAsync(Guid id)
    {
        var item = await GetOwnedItemAsync(id);
        item.MarkAsConsumed();
        await _pantryItemRepository.UpdateAsync(item, autoSave: true);

        var productName = await FindProductNameAsync(item.ProductBarcode);
        return MapToDto(item, productName);
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await GetOwnedItemAsync(id);
        await _pantryItemRepository.DeleteAsync(item, autoSave: true);
    }

    private async Task<PantryItem> GetOwnedItemAsync(Guid id)
    {
        var item = await _pantryItemRepository.FindAsync(id);
        if (item == null)
        {
            throw new EntityNotFoundException(typeof(PantryItem), id);
        }

        if (item.UserId != CurrentUser.GetId())
        {
            throw new AbpAuthorizationException("No tiene permisos para acceder a este ítem de despensa.");
        }

        return item;
    }

    private async Task<string?> FindProductNameAsync(string barcode)
    {
        var product = await _productRepository.FirstOrDefaultAsync(p => p.Barcode == barcode);
        return product?.Name;
    }

    private static PantryItemDto MapToDto(PantryItem item, string? productName)
    {
        return new PantryItemDto
        {
            Id = item.Id,
            ProductBarcode = item.ProductBarcode,
            ProductName = productName,
            Status = item.Status,
            ExpirationDate = item.ExpirationDate,
            Quantity = item.Quantity,
            Unit = item.Unit
        };
    }
}