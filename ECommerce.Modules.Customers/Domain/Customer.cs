// ECommerce.Modules.Customers/Domain/Customer.cs
using ECommerce.Common.Domain;

namespace ECommerce.Modules.Customers.Domain;

public class Customer : Entity
{
    public string Name { get; set; }
    public string Email { get; set; }

    public Customer(string name, string email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
    }
}
