using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Products;

public class ProductDto : EntityDto<Guid> 
{
    public string Name { get; set; }
    public string Maker { get; set; }
    public string Barcode { get; set; }
}
