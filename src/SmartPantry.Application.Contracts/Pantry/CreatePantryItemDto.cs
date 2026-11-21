using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Pantry;

public class CreatePantryItemDto
{
    [Required]
    [StringLength(128)]
    public string ProductBarcode { get; set; } = string.Empty;

    public DateTime? ExpirationDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Quantity { get; set; }

    [StringLength(PantryItemConsts.UnitMaxLength)]
    public string? Unit { get; set; }
}