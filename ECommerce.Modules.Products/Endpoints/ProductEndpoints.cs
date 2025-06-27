using System.Security.Claims;
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
            Product product, HttpContext httpContext) =>
        {
            // Get correlation ID from request headers
            string correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? "not-available";
            logger.LogInformation("Creating product. CorrelationId: {CorrelationId}", correlationId);

            var user = httpContext.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            // Pass correlation ID to service method
            var result = await productService.AddProductAsync(product, userId, correlationId);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(result.Error);
            }

            return Results.Created($"/products/{result.Value.Id}", result.Value);
        })
        .WithName("CreateProduct")
        .WithTags("Products")
        .RequireAuthorization();

    app.MapGet("/products", async (IProductService productService) =>
        {
            var products = await productService.GetAllProductsAsync();
            logger.LogInformation($"Number of products to be returned: {products.Count()}");
            return Results.Ok(products);
        })
        .WithName("GetAllProducts")
        .WithTags("Products")
        .RequireAuthorization();

    app.MapGet("/products/{id}", async (IProductService productService, Guid id) =>
        {
            var product = await productService.GetProductByIdAsync(id);
            return product != null ? Results.Ok(product) : Results.NotFound();
        })
        .WithName("GetProductById")
        .WithTags("Products")
        .RequireAuthorization();

    app.MapPut("/products/{id}", async (IProductService productService,
            Guid id, Product updatedProduct, HttpContext httpContext) =>
        {
            // Get correlation ID from request headers
            string correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? "not-available";
            logger.LogInformation("Updating product with ID: {ProductId}. CorrelationId: {CorrelationId}", id, correlationId);

            var user = httpContext.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            // Pass correlation ID to service method
            var result = await productService.UpdateProductAsync(id, updatedProduct, userId, correlationId);

            if (!result.IsSuccess)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(result.Value);
        })
        .WithName("UpdateProduct")
        .WithTags("Products")
        .RequireAuthorization();
  }
}
