using ECommerce.Modules.Orders.Domain;
using ECommerce.Modules.Orders.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Orders.Endpoints
{
  public static class OrderEndpoints
  {
    public static void MapOrderEndpoints(this WebApplication app)
    {
      // Define POST endpoint to create an order
      app.MapPost("/orders", async (IOrderService orderService,
              List<OrderItem> items, Guid customerId) =>
          {
              var logger = app.Logger;
              logger.LogInformation("Creating order for customer {CustomerId}", customerId);

              var result = await orderService.CreateOrderAsync(customerId, items);

              logger.LogInformation("Order created for customer {CustomerId}", customerId);

              if (!result.Success)
              {
                  logger.LogError("Error creating order for customer {CustomerId}: {Message}", customerId,
                      result.Message);
                  return Results.BadRequest(result.Message);
              }

              return Results.Ok("Order created!");
          })
          .WithName("CreateOrder")
          .WithTags("Orders")
          .RequireAuthorization();

      // Define GET endpoint to retrieve all orders
      app.MapGet("/orders", async (IOrderService orderService) =>
          {
              var orders = await orderService.GetAllOrdersAsync();
              return Results.Ok(orders);
          })
          .WithName("GetAllOrders")
          .WithTags("Orders")
          .RequireAuthorization();

      // Define GET endpoint to retrieve a specific order by ID
      app.MapGet("/orders/{id}", async (IOrderService orderService, Guid id) =>
          {
              var order = await orderService.GetOrderByIdAsync(id);
              return order is not null ? Results.Ok(order) : Results.NotFound();
          })
          .WithName("GetOrderById")
          .WithTags("Orders")
          .RequireAuthorization();
    }
  }
}
