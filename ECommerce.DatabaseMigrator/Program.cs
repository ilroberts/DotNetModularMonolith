using Microsoft.EntityFrameworkCore;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Modules.Products.Persistence;
using ECommerce.BusinessEvents.Persistence;

string GetRequiredEnv(string key) =>
    Environment.GetEnvironmentVariable(key) ?? throw new InvalidOperationException($"Environment variable '{key}' is not set.");

var connectionStrings = new Dictionary<string, string>
{
    ["Customers"] = GetRequiredEnv("CUSTOMER_DB_CONNECTION"),
    ["Orders"] = GetRequiredEnv("ORDER_DB_CONNECTION"),
    ["Products"] = GetRequiredEnv("PRODUCT_DB_CONNECTION"),
    ["BusinessEvents"] = GetRequiredEnv("BUSINESSEVENT_DB_CONNECTION"),
};

var optionsCustomers = new DbContextOptionsBuilder<CustomerDbContext>()
    .UseNpgsql(connectionStrings["Customers"])
    .Options;
using (var context = new CustomerDbContext(optionsCustomers))
{
    Console.WriteLine("Migrating Customers DB...");
    context.Database.Migrate();
}

var optionsOrders = new DbContextOptionsBuilder<OrderDbContext>()
    .UseNpgsql(connectionStrings["Orders"])
    .Options;
using (var context = new OrderDbContext(optionsOrders))
{
    Console.WriteLine("Migrating Orders DB...");
    context.Database.Migrate();
}

var optionsProducts = new DbContextOptionsBuilder<ProductDbContext>()
    .UseNpgsql(connectionStrings["Products"])
    .Options;
using (var context = new ProductDbContext(optionsProducts))
{
    Console.WriteLine("Migrating Products DB...");
    context.Database.Migrate();
}

var optionsBusinessEvents = new DbContextOptionsBuilder<BusinessEventDbContext>()
    .UseNpgsql(connectionStrings["BusinessEvents"])
    .Options;
using (var context = new BusinessEventDbContext(optionsBusinessEvents))
{
    Console.WriteLine("Migrating BusinessEvents DB...");
    context.Database.Migrate();
}

Console.WriteLine("All migrations applied.");
