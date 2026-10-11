using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Products;

public class CreateProductDto
{
    [Required] 
    [StringLength(ProductConsts.NameMaxLength)]
    public string Name{ get; set; } = string.Empty;
    
    [Required]
    [StringLength(ProductConsts.BarcodeMaxLength)]
    public string Barcode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(ProductConsts.MakerMaxLength)]
    public string Maker { get; set; } = string.Empty;
}
