using ECommerce.Contracts.DTOs;
using ECommerce.Modules.Customers.Domain;

namespace ECommerce.Modules.Customers.Services;

public interface ICustomerService
{
    Task<Customer?> GetCustomerByIdAsync(Guid customerId);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Customer> AddCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Guid id, CustomerUpdateDto customerUpdateDto);
}
