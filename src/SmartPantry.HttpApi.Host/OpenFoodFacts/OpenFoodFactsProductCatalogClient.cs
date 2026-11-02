using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartPantry.ExternalProducts; 

namespace SmartPantry.OpenFoodFacts
{
    public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenFoodFactsProductCatalogClient> _logger;

        public OpenFoodFactsProductCatalogClient(HttpClient httpClient, ILogger<OpenFoodFactsProductCatalogClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ExternalProductDto> GetProductAsync(string barcode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v0/product/{barcode}.json");

                // Manejo de limite de peticiones
                if ((int)response.StatusCode == 429)
                {
                    return new ExternalProductDto { Status = ExternalProductStatus.RateLimitExceeded, Barcode = barcode };
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error consumiendo Open Food Facts. HTTP Status: {StatusCode}", response.StatusCode);
                    return new ExternalProductDto { Status = ExternalProductStatus.ServiceUnavailable, Barcode = barcode };
                }

                var offData = await response.Content.ReadFromJsonAsync<OpenFoodFactsResponse>();

                // Open Food Facts devuelve Status = 0 cuando no encuentra el codigo
                if (offData == null || offData.Status == 0)
                {
                    return new ExternalProductDto { Status = ExternalProductStatus.NotFound, Barcode = barcode };
                }

                // Mapeo exitoso al DTO del dominio
                return new ExternalProductDto
                {
                    Status = ExternalProductStatus.Found,
                    Barcode = offData.Code ?? barcode,
                    Name = offData.Product?.ProductName,
                    Brand = offData.Product?.Brands,
                    ImageUrl = offData.Product?.ImageUrl
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red conectando con Open Food Facts");
                return new ExternalProductDto { Status = ExternalProductStatus.ServiceUnavailable, Barcode = barcode };
            }
        }
    }
}