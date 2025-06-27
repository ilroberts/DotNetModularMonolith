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

  public async Task<Result<Product, string>> AddProductAsync(Product product, string userId)
  {
    productDbContext.Products.Add(product);
    await productDbContext.SaveChangesAsync();

    var businessEvent = BusinessEventFactory.Create()
      .WithEntityType(nameof(Product))
      .WithEntityId(product.Id.ToString())
      .WithEventType(IBusinessEventService.EventType.Created)
      .WithActorId(userId)
      .WithActorType(IBusinessEventService.ActorType.User)
      .WithEntityData(product)
      .Build();

    var eventResult = await businessEventService.TrackEventAsync(businessEvent);
    if (!eventResult.IsSuccess)
    {
      productDbContext.Entry(product).State = EntityState.Detached;
      return Result<Product, string>.Failure($"Product creation failed due to business event schema validation: {eventResult.Error}");
    }

    return Result<Product, string>.Success(product);
  }

  public async Task<Result<Product, string>> UpdateProductAsync(Guid id, Product updatedProduct, string userId)
  {
    var existingProduct = await productDbContext.Products.FindAsync(id);
    if (existingProduct == null)
    {
      return Result<Product, string>.Failure("Product not found.");
    }

    existingProduct.Name = updatedProduct.Name;
    existingProduct.Price = updatedProduct.Price;

    await productDbContext.SaveChangesAsync();

    var businessEvent = BusinessEventFactory.Create()
      .WithEntityType(nameof(Product))
      .WithEntityId(id.ToString())
      .WithEventType(IBusinessEventService.EventType.Updated)
      .WithActorId(userId)
      .WithActorType(IBusinessEventService.ActorType.User)
      .WithEntityData(existingProduct)
      .Build();

    logger.LogInformation("Product update event correlation ID: {CorrelationId}", businessEvent.CorrelationId);

    var eventResult = await businessEventService.TrackEventAsync(businessEvent);
    return !eventResult.IsSuccess ? Result<Product, string>.Failure($"Product update event tracking failed: {eventResult.Error}")
        : Result<Product, string>.Success(existingProduct);
  }
}
