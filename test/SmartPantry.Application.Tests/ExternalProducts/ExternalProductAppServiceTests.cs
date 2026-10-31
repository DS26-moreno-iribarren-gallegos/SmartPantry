using System.Threading.Tasks;
using NSubstitute;
using Shouldly; // Usado comúnmente en ABP para aserciones, o puedes usar Assert de xUnit
using Xunit;
using SmartPantry.ExternalProducts;

namespace SmartPantry.ExternalProducts.Tests
{
    public class ExternalProductAppServiceTests
    {
        private readonly IExternalProductCatalogClient _mockClient;
        private readonly ExternalProductAppService _appService;

        public ExternalProductAppServiceTests()
        {
            // 1. Configuramos el mock con NSubstitute
            _mockClient = Substitute.For<IExternalProductCatalogClient>();
            
            // 2. Inyectamos el mock al servicio
            _appService = new ExternalProductAppService(_mockClient);
        }

        [Fact]
        public async Task Should_Return_Found_When_Product_Exists()
        {
            // Arrange
            var barcode = "5449000000996";
            _mockClient.GetProductAsync(barcode).Returns(Task.FromResult(new ExternalProductDto 
            { 
                Status = ExternalProductStatus.Found, 
                Barcode = barcode, 
                Name = "Coca-Cola" 
            }));

            // Act
            var result = await _appService.GetProductByBarcodeAsync(new GetExternalProductInputDto { Barcode = barcode });

            // Assert
            result.Status.ShouldBe(ExternalProductStatus.Found);
            result.Name.ShouldBe("Coca-Cola");
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
        {
            // Arrange
            var barcode = "0000000000000";
            _mockClient.GetProductAsync(barcode).Returns(Task.FromResult(new ExternalProductDto 
            { 
                Status = ExternalProductStatus.NotFound, 
                Barcode = barcode 
            }));

            // Act
            var result = await _appService.GetProductByBarcodeAsync(new GetExternalProductInputDto { Barcode = barcode });

            // Assert
            result.Status.ShouldBe(ExternalProductStatus.NotFound);
            result.Name.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Handle_Missing_Data_Gracefully()
        {
            // Arrange: producto encontrado pero con datos incompletos (null)
            var barcode = "1234567890123";
            _mockClient.GetProductAsync(barcode).Returns(Task.FromResult(new ExternalProductDto 
            { 
                Status = ExternalProductStatus.Found, 
                Barcode = barcode,
                Name = null, // Dato ausente
                Brand = null
            }));

            // Act
            var result = await _appService.GetProductByBarcodeAsync(new GetExternalProductInputDto { Barcode = barcode });

            // Assert
            result.Status.ShouldBe(ExternalProductStatus.Found);
            result.Name.ShouldBeNull();
        }

        [Fact]
        public async Task Should_Return_RateLimitExceeded()
        {
            // Arrange
            var barcode = "5449000000996";
            _mockClient.GetProductAsync(barcode).Returns(Task.FromResult(new ExternalProductDto 
            { 
                Status = ExternalProductStatus.RateLimitExceeded 
            }));

            // Act
            var result = await _appService.GetProductByBarcodeAsync(new GetExternalProductInputDto { Barcode = barcode });

            // Assert
            result.Status.ShouldBe(ExternalProductStatus.RateLimitExceeded);
        }

        [Fact]
        public async Task Should_Return_ServiceUnavailable_When_Provider_Fails()
        {
            // Arrange
            var barcode = "5449000000996";
            _mockClient.GetProductAsync(barcode).Returns(Task.FromResult(new ExternalProductDto 
            { 
                Status = ExternalProductStatus.ServiceUnavailable 
            }));

            // Act
            var result = await _appService.GetProductByBarcodeAsync(new GetExternalProductInputDto { Barcode = barcode });

            // Assert
            result.Status.ShouldBe(ExternalProductStatus.ServiceUnavailable);
        }
    }
}