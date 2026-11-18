using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;

namespace SmartPantry.ExternalProducts
{
    [Authorize]
    public class ExternalProductAppService : ApplicationService, IExternalProductAppService
    {
        private readonly IExternalProductCatalogClient _productCatalogClient;

        public ExternalProductAppService(IExternalProductCatalogClient productCatalogClient)
        {
            _productCatalogClient = productCatalogClient;
        }

        public async Task<ExternalProductDto> GetProductByBarcodeAsync(GetExternalProductInputDto input)
        {
            // La logica de negocio o validaciones adicionales van aca antes o despues de la llamada
            // Por ahora delegamos la responsabilidad de la consulta al cliente
            return await _productCatalogClient.GetProductAsync(input.Barcode);
        }
    }
}