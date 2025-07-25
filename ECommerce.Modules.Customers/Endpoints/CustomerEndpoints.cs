using System.Security.Claims;
using ECommerce.Contracts.DTOs;
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
          Customer customer, HttpContext httpContext) =>
        {
            var user = httpContext.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            // Get correlation ID from request headers
            string correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? "not-available";
            logger.LogInformation("Creating customer. CorrelationId: {CorrelationId}", correlationId);

            // Pass correlation ID to the service method
            var result = await customerService.AddCustomerAsync(customer, userId, correlationId);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Created($"/customers/{result.Value!.Id}", result.Value);
        })
        .WithName("CreateCustomer")
        .WithTags("Customers")
        .Produces<Customer>(StatusCodes.Status201Created)
        .Produces<string>(StatusCodes.Status400BadRequest)
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

        app.MapPut("/customers/{id}", async (ICustomerService customerService, Guid id,
                CustomerUpdateDto customerUpdateDto, HttpContext httpContext) =>
        {
            var user = httpContext.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            // Get correlation ID from request headers
            string correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? "not-available";
            logger.LogInformation("Updating customer with ID: {CustomerId}. CorrelationId: {CorrelationId}", id, correlationId);

            try
            {
                // Pass correlation ID to the service method
                var updatedCustomer = await customerService.UpdateCustomerAsync(id, customerUpdateDto, userId, correlationId);
                return Results.Ok(updatedCustomer);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateCustomer")
        .WithTags("Customers")
        .RequireAuthorization();
    }
}
