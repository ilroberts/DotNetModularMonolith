using ECommerce.Contracts.DTOs;

namespace ECommerce.Contracts.Interfaces;

public interface ICustomerCatalogService
{
  Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
  Task<bool> CustomerExistsAsync(Guid id);
}
