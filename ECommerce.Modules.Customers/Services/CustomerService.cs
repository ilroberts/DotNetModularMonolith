using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Factories;
using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Persistence;
using Microsoft.EntityFrameworkCore;
using ECommerce.Contracts.Interfaces; // Add this using
using ECommerce.Contracts.Exceptions;
using ECommerce.Common;

namespace ECommerce.Modules.Customers.Services;

public class CustomerService(
    CustomerDbContext customerDbContext,
    IBusinessEventService businessEventService // Inject the interface
) : ICustomerService
{
    public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
    {
        return await customerDbContext.Customers.FindAsync(customerId);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await customerDbContext.Customers.ToListAsync();
    }

    public async Task<Result<Customer, string>> AddCustomerAsync(Customer customer, string userId)
    {
        customerDbContext.Customers.Add(customer);
        await customerDbContext.SaveChangesAsync();

        var businessEvent = BusinessEventFactory.Create()
            .WithEntityType(nameof(Customer))
            .WithEntityId(customer.Id.ToString())
            .WithEventType(IBusinessEventService.EventType.Created)
            .WithActorId(userId)
            .WithActorType(IBusinessEventService.ActorType.User)
            .WithEntityData(customer)
            .Build();

        var eventResult = await businessEventService.TrackEventAsync(businessEvent);
        if (!eventResult.IsSuccess)
        {
            customerDbContext.Entry(customer).State = EntityState.Detached;
            return Result<Customer, string>.Failure($"Customer creation failed due to business event schema validation: {eventResult.Error}");
        }
        return Result<Customer, string>.Success(customer);
    }

    public async Task<Customer> UpdateCustomerAsync(Guid id, CustomerUpdateDto customerUpdateDto, string userId)
    {
        var existingCustomer = await customerDbContext.Customers.FindAsync(id);
        if (existingCustomer == null)
        {
            throw new InvalidOperationException("Customer not found.");
        }

        existingCustomer.Name = customerUpdateDto.Name;
        existingCustomer.Email = customerUpdateDto.Email;

        await customerDbContext.SaveChangesAsync();

        var businessEvent = BusinessEventFactory.Create()
            .WithEntityType(nameof(Customer))
            .WithEntityId(id.ToString())
            .WithEventType(IBusinessEventService.EventType.Updated)
            .WithActorId(userId)
            .WithActorType(IBusinessEventService.ActorType.User)
            .WithEntityData(existingCustomer)
            .Build();

        await businessEventService.TrackEventAsync(businessEvent);
        return existingCustomer;
    }
}
