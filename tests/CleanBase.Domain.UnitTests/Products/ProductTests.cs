using CleanBase.Domain.Common;
using CleanBase.Domain.Products;

namespace CleanBase.Domain.UnitTests.Products;

public sealed class ProductTests
{
    [Fact]
    public void Create_WithValidData_ReturnsNormalizedProduct()
    {
        Result<Product> result = Product.Create("  Keyboard  ", " kb-001 ", 89.90m);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.Name.ShouldBe("Keyboard");
        result.Value.Sku.ShouldBe("KB-001");
        result.Value.Price.ShouldBe(89.90m);
    }

    [Fact]
    public void Create_WithZeroPrice_Succeeds()
    {
        Result<Product> result = Product.Create("Free sample", "FREE-1", 0m);

        result.IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ReturnsNameRequired(string name)
    {
        Result<Product> result = Product.Create(name, "SKU-1", 10m);

        result.Error.ShouldBe(ProductErrors.NameRequired);
    }

    [Fact]
    public void Create_WithTooLongName_ReturnsNameTooLong()
    {
        string name = new('a', Product.NameMaxLength + 1);

        Result<Product> result = Product.Create(name, "SKU-1", 10m);

        result.Error.ShouldBe(ProductErrors.NameTooLong);
    }

    [Fact]
    public void Create_WithBlankSku_ReturnsSkuRequired()
    {
        Result<Product> result = Product.Create("Keyboard", " ", 10m);

        result.Error.ShouldBe(ProductErrors.SkuRequired);
    }

    [Fact]
    public void Create_WithTooLongSku_ReturnsSkuTooLong()
    {
        string sku = new('S', Product.SkuMaxLength + 1);

        Result<Product> result = Product.Create("Keyboard", sku, 10m);

        result.Error.ShouldBe(ProductErrors.SkuTooLong);
    }

    [Fact]
    public void Create_WithNegativePrice_ReturnsNegativePrice()
    {
        Result<Product> result = Product.Create("Keyboard", "SKU-1", -0.01m);

        result.Error.ShouldBe(ProductErrors.NegativePrice);
    }

    [Fact]
    public void Update_WithValidData_ChangesNameAndPrice()
    {
        Product product = Product.Create("Keyboard", "SKU-1", 10m).Value;

        Result result = product.Update("  Mouse ", 20m);

        result.IsSuccess.ShouldBeTrue();
        product.Name.ShouldBe("Mouse");
        product.Price.ShouldBe(20m);
    }

    [Fact]
    public void Update_WithInvalidData_ReturnsErrorAndKeepsState()
    {
        Product product = Product.Create("Keyboard", "SKU-1", 10m).Value;

        Result result = product.Update("Mouse", -1m);

        result.Error.ShouldBe(ProductErrors.NegativePrice);
        product.Name.ShouldBe("Keyboard");
        product.Price.ShouldBe(10m);
    }
}
