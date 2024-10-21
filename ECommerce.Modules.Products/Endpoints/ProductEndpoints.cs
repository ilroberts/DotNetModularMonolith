using ECommerce.Modules.Products.Domain;
using ECommerce.Modules.Products.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Products.Endpoints;

public static class ProductEndpoints
{
  public static void MapProductEndpoints(this WebApplication app)
  {
    var logger = app.Logger;

    app.MapPost("/products", async (IProductService productService,
      Product product) =>
    {
      logger.LogInformation("Creating products");
      await productService.AddProductAsync(product);

      return Results.Created($"/products/{product.Id}", product);
    })
    .WithName("CreateProduct")
    .WithTags("Products");

    app.MapGet("/products", async (IProductService productService) =>
    {
      var products = await productService.GetAllProductsAsync();
      logger.LogInformation($"Number of products to be returned: {products.Count()}");
      return Results.Ok(products);
    })
    .WithName("GetAllProducts")
    .WithTags("Products");

    app.MapGet("/products/{id}", async (IProductService productService, Guid id) =>
    {
      var product = await productService.GetProductByIdAsync(id);
      return product is not null ? Results.Ok(product) : Results.NotFound();
    })
    .WithName("GetProductById")
    .WithTags("Products");
  }
}
