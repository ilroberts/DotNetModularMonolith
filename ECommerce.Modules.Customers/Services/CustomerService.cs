using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Customers.Util;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Services;

public class CustomerService(CustomerDbContext customerDbContext) : ICustomerService
{
    public async Task<Customer> GetCustomerByIdAsync(Guid customerId)
    {
        return await customerDbContext.Customers.FindAsync(customerId);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await customerDbContext.Customers.ToListAsync();
    }

    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        // get valid suspensions
        var suspensionTypes = SuspensionTypeCode.GetAllSuspensionTypes();

        customerDbContext.Customers.Add(customer);
        await customerDbContext.SaveChangesAsync();
        return customer;
    }
}
