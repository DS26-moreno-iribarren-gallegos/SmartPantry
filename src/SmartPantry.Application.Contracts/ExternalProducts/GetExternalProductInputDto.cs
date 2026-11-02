using System.ComponentModel.DataAnnotations;

namespace SmartPantry.ExternalProducts
{
    public class GetExternalProductInputDto
    {
        [Required]
        [StringLength(13, MinimumLength = 8)] // En este apartado podemos configurar las longitudes
        public string Barcode { get; set; } = string.Empty;
    }
}