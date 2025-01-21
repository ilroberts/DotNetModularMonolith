using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Customers.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var logger = app.Logger;

        app.MapPost("/customers", async (ICustomerService customerService,
          Customer customer) =>
        {
            logger.LogInformation("Creating customer");
            await customerService.AddCustomerAsync(customer);
            return Results.Created($"/customers/{customer.Id}", customer);
        })
        .WithName("CreateCustomer")
        .WithTags("Customers")
        .RequireAuthorization();

        app.MapGet("/customers", async (ICustomerService customerService) =>
        {
            var customers = await customerService.GetAllCustomersAsync();
            logger.LogInformation($"Number of customers to be returned: {customers.Count()}");
            return Results.Ok(customers);
        })
        .WithName("GetAllCustomers")
        .WithTags("Customers")
        .RequireAuthorization();

        app.MapGet("/customers/{id}", async (ICustomerService customerService, Guid id) =>
        {
            var customer = await customerService.GetCustomerByIdAsync(id);
            return customer is not null ? Results.Ok(customer) : Results.NotFound();
        })
        .WithName("GetCustomerById")
        .WithTags("Customers")
        .RequireAuthorization();
    }
}
