using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Factories;
using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Persistence;
using Microsoft.EntityFrameworkCore;
using ECommerce.Contracts.Interfaces; // Add this using
using ECommerce.Common;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly CustomerDbContext _customerDbContext;
    private readonly IBusinessEventService _businessEventService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        CustomerDbContext customerDbContext,
        IBusinessEventService businessEventService,
        ILogger<CustomerService> logger)
    {
        _customerDbContext = customerDbContext;
        _businessEventService = businessEventService;
        _logger = logger;
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
    {
        return await _customerDbContext.Customers.FindAsync(customerId);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _customerDbContext.Customers.ToListAsync();
    }

    public async Task<Result<Customer, string>> AddCustomerAsync(Customer customer, string userId, string correlationId)
    {
        _logger.LogInformation("Adding customer with name {CustomerName}. CorrelationId: {CorrelationId}",
            customer.Name, correlationId);

        _customerDbContext.Customers.Add(customer);
        await _customerDbContext.SaveChangesAsync();

        var businessEvent = BusinessEventFactory.Create()
            .WithEntityType(nameof(Customer))
            .WithEntityId(customer.Id.ToString())
            .WithEventType(IBusinessEventService.EventType.Created)
            .WithActorId(userId)
            .WithActorType(IBusinessEventService.ActorType.User)
            .WithEntityData(customer)
            .WithCorrelationId(correlationId)  // Add the correlation ID to the business event
            .Build();

        var eventResult = await _businessEventService.TrackEventAsync(businessEvent);
        if (!eventResult.IsSuccess)
        {
            _customerDbContext.Entry(customer).State = EntityState.Detached;
            _logger.LogError("Customer creation failed. CorrelationId: {CorrelationId}, Error: {Error}",
                correlationId, eventResult.Error);
            return Result<Customer, string>.Failure($"Customer creation failed due to business event schema validation: {eventResult.Error}");
        }

        _logger.LogInformation("Customer created successfully with ID: {CustomerId}. CorrelationId: {CorrelationId}",
            customer.Id, correlationId);
        return Result<Customer, string>.Success(customer);
    }

    public async Task<Customer> UpdateCustomerAsync(Guid id, CustomerUpdateDto customerUpdateDto, string userId, string correlationId)
    {
        _logger.LogInformation("Updating customer with ID: {CustomerId}. CorrelationId: {CorrelationId}",
            id, correlationId);

        var existingCustomer = await _customerDbContext.Customers.FindAsync(id);
        if (existingCustomer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found. CorrelationId: {CorrelationId}",
                id, correlationId);
            throw new InvalidOperationException("Customer not found.");
        }

        existingCustomer.Name = customerUpdateDto.Name;
        existingCustomer.Email = customerUpdateDto.Email;

        await _customerDbContext.SaveChangesAsync();

        // Create a business event for the update operation
        var businessEvent = BusinessEventFactory.Create()
            .WithEntityType(nameof(Customer))
            .WithEntityId(existingCustomer.Id.ToString())
            .WithEventType(IBusinessEventService.EventType.Updated)
            .WithActorId(userId)
            .WithActorType(IBusinessEventService.ActorType.User)
            .WithEntityData(existingCustomer)
            .WithCorrelationId(correlationId)  // Add the correlation ID to the business event
            .Build();

        var eventResult = await _businessEventService.TrackEventAsync(businessEvent);
        if (!eventResult.IsSuccess)
        {
            _logger.LogWarning("Failed to track customer update event. CorrelationId: {CorrelationId}, Error: {Error}",
                correlationId, eventResult.Error);
        }

        _logger.LogInformation("Customer updated successfully. CorrelationId: {CorrelationId}", correlationId);
        return existingCustomer;
    }
}
