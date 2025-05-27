using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Factories;
using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Customers.Util;
using Microsoft.EntityFrameworkCore;
using ECommerce.Contracts.Interfaces; // Add this using

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

    public async Task<Customer> AddCustomerAsync(Customer customer,  string userId)
    {
        // get valid suspensions
        var suspensionTypes = SuspensionTypeCode.GetAllSuspensionTypes();

        customerDbContext.Customers.Add(customer);
        await customerDbContext.SaveChangesAsync();

        var businessEvent = BusinessEventFactory.Create()
            .WithEntityType("Customer")
            .WithEntityId(customer.Id.ToString())
            .WithEventType(IBusinessEventService.EventType.Created)
            .WithSchemaVersion(1)
            .WithActorId(userId)
            .WithActorType(IBusinessEventService.ActorType.User)
            .WithEntityData(customer)
            .Build();

        await businessEventService.TrackEventAsync(businessEvent);

        return customer;
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
            .WithEntityType("Customer")
            .WithEntityId(id.ToString())
            .WithSchemaVersion(1)
            .WithEventType(IBusinessEventService.EventType.Updated)
            .WithActorId(userId)
            .WithActorType(IBusinessEventService.ActorType.User)
            .WithEntityData(existingCustomer)
            .Build();

        await businessEventService.TrackEventAsync(businessEvent);

        return existingCustomer;
    }
}
