using ECommerce.Modules.Products.Domain;
using ECommerce.Modules.Products.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ECommerce.Modules.Products.Endpoints;

public static class ProductEndpoints
{
  public static void MapProductEndpoints(this WebApplication app)
  {
    var logger = app.Logger;

    app.MapPost("/products", async (IProductService productService,
      Product product, ClaimsPrincipal user) =>
    {
      logger.LogInformation("Creating product");
      var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
      var result = await productService.AddProductAsync(product, userId);

      if (!result.IsSuccess)
      {
        return Results.BadRequest(result.Error);
      }

      return Results.Created($"/products/{result.Value.Id}", result.Value);
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

    app.MapPut("/products/{id}", async (IProductService productService, Guid id, Product updatedProduct, ClaimsPrincipal user) =>
    {
      logger.LogInformation("Updating product with ID: {ProductId}", id);
      var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
      var result = await productService.UpdateProductAsync(id, updatedProduct, userId);

      if (!result.IsSuccess)
      {
        return result.Error == "Product not found." ?
          Results.NotFound(result.Error) :
          Results.BadRequest(result.Error);
      }

      return Results.Ok(result.Value);
    })
    .WithName("UpdateProduct")
    .WithTags("Products");
  }
}
