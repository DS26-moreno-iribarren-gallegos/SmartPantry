namespace SmartPantry.ExternalProducts
{
    public class ExternalProductDto
    {
        public ExternalProductStatus Status { get; set; }
        public string? Barcode { get; set; }
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
    }
}

/* Las propiedades son anulables (string?) para cumplir con el requisito de no inventar valores si Open Food Facts no los informa */