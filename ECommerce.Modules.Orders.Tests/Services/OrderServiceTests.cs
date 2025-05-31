using ECommerce.Modules.Orders.Services;
using ECommerce.Modules.Orders.Domain;
using Moq;
using Microsoft.Extensions.Logging;
using ECommerce.Contracts.Interfaces;
using ECommerce.Contracts.DTOs;
using ECommerce.Modules.Orders.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Tests.Services;

public class OrderServiceTests
{
  private readonly Mock<ILogger<OrderService>> _loggerMock;
  private readonly Mock<IProductCatalogService> _productCatalogServiceMock;
  private readonly Mock<ICustomerCatalogService> _customerCatalogServiceMock;
  private readonly OrderService _orderService;
  private readonly OrderDbContext _orderDbContext;


  public OrderServiceTests()
  {
    _loggerMock = new Mock<ILogger<OrderService>>();
    _productCatalogServiceMock = new Mock<IProductCatalogService>();
    _customerCatalogServiceMock = new Mock<ICustomerCatalogService>();

    var options = new DbContextOptionsBuilder<OrderDbContext>()
      .UseInMemoryDatabase(databaseName: "OrderDatabase")
      .Options;

    _orderDbContext = new OrderDbContext(options);
    _orderService = new OrderService(_orderDbContext, _loggerMock.Object,
      _productCatalogServiceMock.Object,
      _customerCatalogServiceMock.Object);
  }

  private static Order CreateTestOrder()
  {
    var productId = Guid.NewGuid();
    var orderItem = new OrderItem()
    {
      ProductId = productId,
      Quantity = 1,
      Price = 100
    };
    var customerId = Guid.NewGuid();
    var order = new Order(customerId, new List<OrderItem> { orderItem });
    return order;
  }

  [Fact]
  public async Task CreateOrder_ShouldReturnOrder_WhenOrderIsValid()
  {
    // Arrange
    var order = CreateTestOrder();
    var customerId = order.CustomerId;
    var productId = order.Items.First().ProductId;

    using (var context = new OrderDbContext(new DbContextOptionsBuilder<OrderDbContext>()
      .UseInMemoryDatabase(databaseName: "OrderDatabase")
      .Options))
    {
      context.Add(order);
      context.SaveChanges();

      var customer = new CustomerDto() { Id = customerId, Name = "John Doe" };
      var product = new ProductDto() { Id = productId, Name = "Product 1", Price = 100 };

      _customerCatalogServiceMock.Setup(service => service.GetCustomerByIdAsync(It.IsAny<Guid>())).ReturnsAsync(customer);
      _customerCatalogServiceMock.Setup(service => service.CustomerExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
      _productCatalogServiceMock.Setup(service => service.GetProductByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);

      // Act
      var orderResult = await _orderService.CreateOrderAsync(customerId, [order.Items.First()]);

      // Assert
      Assert.NotNull(orderResult);
      Assert.True(orderResult.Success);
    }
  }

  [Fact]
  public async Task CreateOrder_ShouldReturnError_WhenProductIsInvalid()
  {
    // Arrange
    var productId = Guid.NewGuid();
    var orderItem = new OrderItem() {
      ProductId = productId,
      Quantity = 1,
      Price = 100
    };
    var customerId = Guid.NewGuid();
    var product = new ProductDto() { Id = productId, Name = "Product 1", Price = 100 };
    var expectedErrorMessage = "One or more products not found";

    _productCatalogServiceMock.Setup(service => service.GetProductByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductDto?)null);

    // Act
    var orderResult = await _orderService.CreateOrderAsync(customerId, [orderItem]);

    // Assert
    Assert.NotNull(orderResult);
    Assert.False(orderResult.Success);
    Assert.Equal(expectedErrorMessage, orderResult.Message);
  }

  [Fact]
  public async Task CreateOrder_ShouldReturnError_WhenCustomerIsInvalid()
  {
    // Arrange
    var productId = Guid.NewGuid();
    var orderItem = new OrderItem() {
      ProductId = productId,
      Quantity = 1,
      Price = 100
    };
    var customerId = Guid.NewGuid();
    var customer = new CustomerDto() { Id = customerId, Name = "John Doe" };
    var product = new ProductDto() { Id = productId, Name = "Product 1", Price = 100 };
    var expectedErrorMessage = $"Customer with id {customerId} not found";

    _productCatalogServiceMock.Setup(service => service.GetProductByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);
    _customerCatalogServiceMock.Setup(service => service.GetCustomerByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CustomerDto?)null);

    // Act
    var orderResult = await _orderService.CreateOrderAsync(customerId, [orderItem]);

    // Assert
    Assert.NotNull(orderResult);
    Assert.False(orderResult.Success);
    Assert.Equal(expectedErrorMessage, orderResult.Message);
  }
}
