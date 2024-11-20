using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

namespace ECommerce.ArchitectureTests;

public abstract class BaseArchitecture
{
    private const string BaseNamespace = "ECommerce";

    protected const string ModulesNamespace = $"{BaseNamespace}.Modules";
    protected const string CustomersModuleNamespace = $"{ModulesNamespace}.Customers";
    protected const string OrdersModuleNamespace = $"{ModulesNamespace}.Orders";
    protected const string ProductsModuleNamespace = $"{ModulesNamespace}.Products";

    protected readonly Architecture _architecture = new ArchLoader().LoadAssemblies(
        typeof(Contracts.Interfaces.ICustomerCatalogService).Assembly,
        typeof(Contracts.Interfaces.IProductCatalogService).Assembly,
        typeof(Common.Domain.Entity).Assembly,
        typeof(Modules.Orders.Services.OrderService).Assembly,
        typeof(Modules.Products.Services.ProductService).Assembly,
        typeof(Modules.Customers.Services.CustomerService).Assembly,
        typeof(Modules.Customers.Services.CustomerCatalogService).Assembly).Build();
}
