using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly CustomerDbContext _customerDbContext;

    public CustomerService(CustomerDbContext customerDbContext)
    {
        _customerDbContext = customerDbContext;
    }

    public async Task<Customer> GetCustomerByIdAsync(Guid customerId)
    {
        return await _customerDbContext.Customers.FindAsync(customerId);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _customerDbContext.Customers.ToListAsync();
    }

    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        // get valid suspensions
        var suspensionTypes = SuspensionTypeCode.GetAllSuspensionTypes();

        _customerDbContext.Customers.Add(customer);
        await _customerDbContext.SaveChangesAsync();
        return customer;
    }
}
