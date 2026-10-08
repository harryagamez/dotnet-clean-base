using System.Net;
using System.Net.Http.Json;
using CleanBase.Application.Products;
using Microsoft.AspNetCore.Http;

namespace CleanBase.Api.IntegrationTests.Products;

/// <summary>
/// Exercises the full stack over HTTP: routing, validation, handlers, EF Core and SQL Server.
/// Every test creates its own product with a unique SKU, so tests don't depend on each other.
/// </summary>
public sealed class ProductEndpointsTests(ApiFactory factory)
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Create_ThenGetById_ReturnsTheNormalizedProduct()
    {
        using HttpClient client = factory.CreateClient();
        string sku = NewSku();

        HttpResponseMessage createResponse = await client.PostAsJsonAsync(
            "/products",
            new { name = "  Keyboard ", sku = sku.ToLowerInvariant(), price = 89.90m },
            CancellationToken);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        Guid id = await createResponse.Content.ReadFromJsonAsync<Guid>(CancellationToken);
        createResponse.Headers.Location!.AbsolutePath.ShouldBe($"/products/{id}");

        ProductResponse? product = await client.GetFromJsonAsync<ProductResponse>($"/products/{id}", CancellationToken);

        product.ShouldBe(new ProductResponse(id, "Keyboard", sku, 89.90m));
    }

    [Fact]
    public async Task Create_WithInvalidData_Returns400WithOneEntryPerField()
    {
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/products",
            new { name = string.Empty, sku = string.Empty, price = -1m },
            CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        HttpValidationProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(CancellationToken);
        problem!.Errors.Keys.ShouldBe(["Name", "Sku", "Price"], ignoreOrder: true);
    }

    [Fact]
    public async Task Create_WithMalformedJson_Returns400()
    {
        using HttpClient client = factory.CreateClient();
        using var content = new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/products", content, CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithDuplicateSku_Returns409()
    {
        using HttpClient client = factory.CreateClient();
        string sku = NewSku();
        await CreateProductAsync(client, sku);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/products",
            new { name = "Another", sku, price = 1m },
            CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetById_WhenProductDoesNotExist_Returns404()
    {
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync($"/products/{Guid.NewGuid()}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task List_IncludesCreatedProduct()
    {
        using HttpClient client = factory.CreateClient();
        Guid id = await CreateProductAsync(client, NewSku());

        List<ProductResponse>? products =
            await client.GetFromJsonAsync<List<ProductResponse>>("/products", CancellationToken);

        products!.ShouldContain(product => product.Id == id);
    }

    [Fact]
    public async Task Update_ChangesNameAndPrice()
    {
        using HttpClient client = factory.CreateClient();
        Guid id = await CreateProductAsync(client, NewSku());

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/products/{id}",
            new { name = "Mouse", price = 20m },
            CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        ProductResponse? product = await client.GetFromJsonAsync<ProductResponse>($"/products/{id}", CancellationToken);
        product!.Name.ShouldBe("Mouse");
        product.Price.ShouldBe(20m);
    }

    [Fact]
    public async Task Delete_RemovesTheProduct()
    {
        using HttpClient client = factory.CreateClient();
        Guid id = await CreateProductAsync(client, NewSku());

        HttpResponseMessage deleteResponse = await client.DeleteAsync($"/products/{id}", CancellationToken);
        HttpResponseMessage getResponse = await client.GetAsync($"/products/{id}", CancellationToken);

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static string NewSku() => $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();

    private static async Task<Guid> CreateProductAsync(HttpClient client, string sku)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/products",
            new { name = "Keyboard", sku, price = 10m },
            CancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(CancellationToken);
    }
}
