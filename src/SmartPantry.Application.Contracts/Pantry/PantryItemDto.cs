using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Pantry;

public class PantryItemDto : EntityDto<Guid>
{
    public string ProductBarcode { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public PantryItemStatus Status { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
}