// ECommerce.Modules.Products/Domain/Product.cs
using ECommerce.Common.Domain;

namespace ECommerce.Modules.Products.Domain;

public class Product : Entity
{
  public string Name { get; set; }
  public decimal Price { get; set; }

  public Product(string name, decimal price)
  {
    Id = Guid.NewGuid();
    Name = name;
    Price = price;
  }
}
