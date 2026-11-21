using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Pantry;

public class UpdatePantryItemDto
{
    [Range(0, double.MaxValue)]
    public decimal? Quantity { get; set; }

    [StringLength(PantryItemConsts.UnitMaxLength)]
    public string? Unit { get; set; }

    public DateTime? ExpirationDate { get; set; }
}