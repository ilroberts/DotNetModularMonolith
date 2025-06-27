using ECommerce.Modules.Products.Domain;
using ECommerce.Modules.Products.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ECommerce.Contracts.Interfaces;
using ECommerce.Contracts.Factories;
using ECommerce.Common;

namespace ECommerce.Modules.Products.Services;

public class ProductService(
    ProductDbContext productDbContext,
    ILogger<ProductService> logger,
    IBusinessEventService businessEventService)
    : IProductService
{
    public async Task<Product> GetProductByIdAsync(Guid productId)
  {
    return await productDbContext.Products.FindAsync(productId);
  }

  public async Task<IEnumerable<Product>> GetAllProductsAsync()
  {
    var products = await productDbContext.Products.ToListAsync();
    logger.LogInformation("Number of products to be returned: {Count}", products.Count());
    return products;
  }

  public async Task<Result<Product, string>> AddProductAsync(Product product, string userId, string correlationId)
  {
    logger.LogInformation("Adding product with name {ProductName}. CorrelationId: {CorrelationId}",
        product.Name, correlationId);

    productDbContext.Products.Add(product);
    await productDbContext.SaveChangesAsync();

    var businessEvent = BusinessEventFactory.Create()
      .WithEntityType(nameof(Product))
      .WithEntityId(product.Id.ToString())
      .WithEventType(IBusinessEventService.EventType.Created)
      .WithActorId(userId)
      .WithActorType(IBusinessEventService.ActorType.User)
      .WithEntityData(product)
      .WithCorrelationId(correlationId)  // Add correlation ID to the business event
      .Build();

    var eventResult = await businessEventService.TrackEventAsync(businessEvent);
    if (!eventResult.IsSuccess)
    {
      productDbContext.Entry(product).State = EntityState.Detached;
      logger.LogError("Product creation failed. CorrelationId: {CorrelationId}, Error: {Error}",
        correlationId, eventResult.Error);
      return Result<Product, string>.Failure($"Product creation failed due to business event schema validation: {eventResult.Error}");
    }

    logger.LogInformation("Product created successfully with ID: {ProductId}. CorrelationId: {CorrelationId}",
        product.Id, correlationId);
    return Result<Product, string>.Success(product);
  }

  public async Task<Result<Product, string>> UpdateProductAsync(Guid id, Product updatedProduct, string userId, string correlationId)
  {
    logger.LogInformation("Updating product with ID: {ProductId}. CorrelationId: {CorrelationId}",
        id, correlationId);

    var existingProduct = await productDbContext.Products.FindAsync(id);
    if (existingProduct == null)
    {
      logger.LogWarning("Product with ID {ProductId} not found. CorrelationId: {CorrelationId}",
          id, correlationId);
      return Result<Product, string>.Failure("Product not found.");
    }

    existingProduct.Name = updatedProduct.Name;
    existingProduct.Price = updatedProduct.Price;

    await productDbContext.SaveChangesAsync();

    var businessEvent = BusinessEventFactory.Create()
      .WithEntityType(nameof(Product))
      .WithEntityId(existingProduct.Id.ToString())
      .WithEventType(IBusinessEventService.EventType.Updated)
      .WithActorId(userId)
      .WithActorType(IBusinessEventService.ActorType.User)
      .WithEntityData(existingProduct)
      .WithCorrelationId(correlationId)  // Add correlation ID to the business event
      .Build();

    var eventResult = await businessEventService.TrackEventAsync(businessEvent);
    if (!eventResult.IsSuccess)
    {
      logger.LogWarning("Failed to track product update event. CorrelationId: {CorrelationId}, Error: {Error}",
          correlationId, eventResult.Error);
    }

    logger.LogInformation("Product updated successfully. CorrelationId: {CorrelationId}", correlationId);
    return Result<Product, string>.Success(existingProduct);
  }
}
