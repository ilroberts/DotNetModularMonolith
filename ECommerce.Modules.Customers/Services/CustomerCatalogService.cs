using ECommerce.Contracts.Interfaces;
using ECommerce.Contracts.DTOs;

namespace ECommerce.Modules.Customers.Services;

internal class CustomerCatalogService(ICustomerService customerService) : ICustomerCatalogService
{
    private readonly ICustomerService _customerService = customerService;

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        return customer == null ? null : new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email
        };
    }

    public async Task<bool> CustomerExistsAsync(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        return customer != null;
    }
}
