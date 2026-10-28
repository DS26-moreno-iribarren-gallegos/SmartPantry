using System;
using Shouldly;
using Xunit;

namespace SmartPantry.Products;

public class Product_Domain_Tests
{
    [Fact]
    public void Update_Should_Normalize_And_Keep_State_When_Valid()
    {
        // Arrange
        var product = new Product(Guid.NewGuid(), "Original", "Maker", "123");

        // Act - Prueba de modificación válida con espacios extra (normalización)
        product.Update("  Nuevo Nombre  ", " Nuevo Maker ", " 321 ");

        // Assert
        product.Name.ShouldBe("Nuevo Nombre");
        product.Maker.ShouldBe("Nuevo Maker");
        product.Barcode.ShouldBe("321");
    }

    [Fact]
    public void Update_Should_Reject_Invalid_Modification_And_Not_Leave_Partial_State()
    {
        // Arrange
        var product = new Product(Guid.NewGuid(), "Original", "Maker", "123");

        // Act & Assert - Prueba de rechazo
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            product.Update("", "Nuevo Maker", "321"); // Nombre vacío es inválido
        });

        // Assert - Verifica que mantiene el estado anterior
        product.Name.ShouldBe("Original");
        product.Maker.ShouldBe("Maker");
        product.Barcode.ShouldBe("123");
    }
}