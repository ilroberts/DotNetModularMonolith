using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ECommerce.AdminUI.Services
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CustomerService _customerService;
        private readonly ProductService _productService;

        public OrderService(
            IHttpClientFactory httpClientFactory,
            ILogger<OrderService> logger,
            IHttpContextAccessor httpContextAccessor,
            CustomerService customerService,
            ProductService productService)
        {
            _httpClient = httpClientFactory.CreateClient("ModularMonolith");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _customerService = customerService;
            _productService = productService;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("orders");
                response.EnsureSuccessStatusCode();

                // Deserialize to API format first
                var apiOrders = await response.Content.ReadFromJsonAsync<List<ApiOrderDto>>() ?? new List<ApiOrderDto>();
                var orders = new List<OrderDto>();

                // Convert API order format to application OrderDto format
                foreach (var apiOrder in apiOrders)
                {
                    // Take the first item for now (our UI currently only supports single item orders)
                    var item = apiOrder.Items.FirstOrDefault();
                    if (item != null)
                    {
                        var order = new OrderDto
                        {
                            Id = apiOrder.Id,
                            CustomerId = apiOrder.CustomerId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            ProductPrice = item.Price,
                            TotalPrice = item.Price * item.Quantity,
                            CreatedAt = DateTime.UtcNow, // API doesn't provide creation date yet
                            Status = "Completed" // Default status as API doesn't provide it yet
                        };

                        orders.Add(order);
                    }
                }

                // Enrich orders with customer and product details
                foreach (var order in orders)
                {
                    await EnrichOrderWithDetailsAsync(order);
                }

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return new List<OrderDto>();
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"orders/{id}");
                response.EnsureSuccessStatusCode();

                // Deserialize to API format first
                var apiOrder = await response.Content.ReadFromJsonAsync<ApiOrderDto>();
                if (apiOrder == null || !apiOrder.Items.Any())
                {
                    return null;
                }

                // Take the first item for now (our UI currently only supports single item orders)
                var item = apiOrder.Items.First();
                var order = new OrderDto
                {
                    Id = apiOrder.Id,
                    CustomerId = apiOrder.CustomerId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ProductPrice = item.Price,
                    TotalPrice = item.Price * item.Quantity,
                    CreatedAt = DateTime.UtcNow, // API doesn't provide creation date yet
                    Status = "Completed" // Default status as API doesn't provide it yet
                };

                // Enrich with customer and product details
                await EnrichOrderWithDetailsAsync(order);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return null;
            }
        }

        private async Task EnrichOrderWithDetailsAsync(OrderDto order)
        {
            // Fetch customer details
            if (order.CustomerId != Guid.Empty)
            {
                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                if (customer != null)
                {
                    order.CustomerName = customer.Name;
                    order.CustomerEmail = customer.Email;
                }
            }

            // Fetch product details
            if (order.ProductId != Guid.Empty)
            {
                var product = await _productService.GetProductByIdAsync(order.ProductId);
                if (product != null)
                {
                    order.ProductName = product.Name;
                    order.ProductPrice = product.Price;

                    // Recalculate total price if necessary
                    if (order.TotalPrice == 0 && order.Quantity > 0)
                    {
                        order.TotalPrice = order.ProductPrice * order.Quantity;
                    }
                }
            }
        }

        public async Task<bool> CreateOrderAsync(OrderDto order)
        {
            try
            {
                // Enrich the order with customer and product details
                await EnrichOrderWithDetailsAsync(order);

                AddAuthorizationHeader();

                // Create the order items array as expected by the API
                var orderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        Id = Guid.NewGuid(),
                        ProductId = order.ProductId,
                        Quantity = order.Quantity,
                        Price = order.ProductPrice
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(orderItems),
                    Encoding.UTF8,
                    "application/json");

                // Add customerId as a query parameter
                var url = $"orders?customerId={order.CustomerId}";
                _logger.LogInformation("Creating order at URL: {Url} with data: {OrderData}",
                    _httpClient.BaseAddress + url, JsonSerializer.Serialize(orderItems));

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for customer {CustomerId}", order.CustomerId);
                return false;
            }
        }

        public async Task<bool> UpdateOrderAsync(Guid id, OrderDto order)
        {
            // For now, updates aren't fully implemented as we'd need to map back to API format
            try
            {
                // Ensure order has all required details
                await EnrichOrderWithDetailsAsync(order);

                AddAuthorizationHeader();
                var content = new StringContent(
                    JsonSerializer.Serialize(order),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"orders/{id}", content);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"orders/{id}");
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderId}", id);
                return false;
            }
        }
    }
}
