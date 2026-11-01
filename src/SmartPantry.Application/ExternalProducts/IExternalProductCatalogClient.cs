using System.Threading.Tasks;

namespace SmartPantry.ExternalProducts
{
    public interface IExternalProductCatalogClient
    {
        Task<ExternalProductDto> GetProductAsync(string barcode);
    }
}