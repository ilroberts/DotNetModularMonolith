using ECommerce.Common;
using ECommerce.Contracts.DTOs;
using ECommerce.Modules.Customers.Domain;

namespace ECommerce.Modules.Customers.Services;

public interface ICustomerService
{
    Task<Customer?> GetCustomerByIdAsync(Guid customerId);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Result<Customer, string>> AddCustomerAsync(Customer customer, string userId, string correlationId);
    Task<Customer> UpdateCustomerAsync(Guid id, CustomerUpdateDto customerUpdateDto, string userId, string correlationId);
}
