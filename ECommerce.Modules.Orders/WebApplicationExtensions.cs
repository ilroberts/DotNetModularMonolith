using ECommerce.Modules.Orders.Endpoints;
using Microsoft.AspNetCore.Builder;

namespace ECommerce.Modules.Orders;

public static class WebApplicationExtensions
{
  public static WebApplication AddOrderEndpooints(this WebApplication app)
  {
    app.MapOrderEndpoints();
    return app;
  }
}