namespace ECommerce.AdminUI.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
        Task<bool> CreateCustomerAsync(CustomerDto customer);
        Task<bool> UpdateCustomerAsync(Guid id, CustomerDto customer);
        Task<bool> DeleteCustomerAsync(Guid id);
    }
}

