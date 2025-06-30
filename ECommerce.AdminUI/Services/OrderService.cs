using System.Text;
using System.Text.Json;

namespace ECommerce.AdminUI.Services
{
    public class OrderService(
        IHttpClientFactory httpClientFactory,
        ILogger<OrderService> logger,
        IHttpContextAccessor httpContextAccessor,
        ICustomerService customerService,
        IProductService productService,
        IAuthService authService)
        : BaseService(httpContextAccessor, authService, logger), IOrderService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("ModularMonolith");

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var token = GetTokenFromSession();
            var username = GetUsernameFromSession();

            if (string.IsNullOrEmpty(token))
            {
                Logger.LogWarning("No auth token available for order list request");
                return new List<OrderDto>();
            }

            try
            {
                // Define the API call as a function that takes a token
                async Task<HttpResponseMessage> apiCall(string tkn)
                {
                    AddAuthorizationHeader(_httpClient, tkn);
                    return await _httpClient.GetAsync("orders");
                }

                // Execute with automatic token refresh
                var httpContext = HttpContextAccessor.HttpContext!;
                var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                    apiCall, token, username ?? string.Empty, httpContext);

                if (!success || response == null)
                {
                    Logger.LogWarning("Failed to retrieve orders due to authentication issues");
                    return new List<OrderDto>();
                }

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
                            CreatedAt = apiOrder.CreatedAt, // Use actual creation date from API
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
                Logger.LogError(ex, "Error retrieving orders");
                return new List<OrderDto>();
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var token = GetTokenFromSession();
            var username = GetUsernameFromSession();

            if (string.IsNullOrEmpty(token))
            {
                Logger.LogWarning("No auth token available for order detail request");
                return null;
            }

            try
            {
                // Define the API call as a function that takes a token
                async Task<HttpResponseMessage> apiCall(string tkn)
                {
                    AddAuthorizationHeader(_httpClient, tkn);
                    return await _httpClient.GetAsync($"orders/{id}");
                }

                // Execute with automatic token refresh
                var httpContext = HttpContextAccessor.HttpContext!;
                var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                    apiCall, token, username ?? string.Empty, httpContext);

                if (!success || response == null)
                {
                    Logger.LogWarning("Failed to retrieve order {OrderId} due to authentication issues", id);
                    return null;
                }

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
                    CreatedAt = apiOrder.CreatedAt, // Use actual creation date from API
                    Status = "Completed" // Default status as API doesn't provide it yet
                };

                // Enrich order with customer and product details
                await EnrichOrderWithDetailsAsync(order);

                return order;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return null;
            }
        }

        public async Task<bool> CreateOrderAsync(OrderDto order)
        {
            var token = GetTokenFromSession();
            var username = GetUsernameFromSession();

            if (string.IsNullOrEmpty(token))
            {
                Logger.LogWarning("No auth token available for create order request");
                return false;
            }

            try
            {
                // Convert our OrderDto to the API format
                var apiOrder = new ApiOrderDto
                {
                    CustomerId = order.CustomerId,
                    Items =
                    [
                        new ApiOrderItemDto { ProductId = order.ProductId,
                            Quantity = order.Quantity,
                            Price = order.ProductPrice }
                    ]
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(apiOrder),
                    Encoding.UTF8,
                    "application/json");

                // Define the API call as a function that takes a token
                async Task<HttpResponseMessage> apiCall(string tkn)
                {
                    AddAuthorizationHeader(_httpClient, tkn);
                    return await _httpClient.PostAsync("orders", content);
                }

                // Execute with automatic token refresh
                var httpContext = HttpContextAccessor.HttpContext!;
                var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                    apiCall, token, username ?? string.Empty, httpContext);

                if (success && response != null)
                {
                    Logger.LogInformation("Successfully created order");
                    return true;
                }

                Logger.LogWarning("Failed to create order due to authentication issues");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating order");
                return false;
            }
        }

        public async Task<bool> UpdateOrderAsync(Guid id, OrderDto order)
        {
            var token = GetTokenFromSession();
            var username = GetUsernameFromSession();

            if (string.IsNullOrEmpty(token))
            {
                Logger.LogWarning("No auth token available for update order request");
                return false;
            }

            try
            {
                // Convert our OrderDto to the API format
                var apiOrder = new ApiOrderDto
                {
                    Id = id,
                    CustomerId = order.CustomerId,
                    Items = new List<ApiOrderItemDto>
                    {
                        new()
                        {
                            ProductId = order.ProductId,
                            Quantity = order.Quantity,
                            Price = order.ProductPrice
                        }
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(apiOrder),
                    Encoding.UTF8,
                    "application/json");

                // Define the API call as a function that takes a token
                async Task<HttpResponseMessage> apiCall(string tkn)
                {
                    AddAuthorizationHeader(_httpClient, tkn);
                    return await _httpClient.PutAsync($"orders/{id}", content);
                }

                // Execute with automatic token refresh
                var httpContext = HttpContextAccessor.HttpContext!;
                var (success, response) = await AuthService.ExecuteWithTokenRefreshAsync(
                    apiCall, token, username ?? string.Empty, httpContext);

                if (success && response != null)
                {
                    Logger.LogInformation("Successfully updated order {OrderId}", id);
                    return true;
                }

                Logger.LogWarning("Failed to update order {OrderId} due to authentication issues", id);
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating order {OrderId}", id);
                return false;
            }
        }

        /// <summary>
        /// Placeholder method for deleting an order.
        /// Note: The backend API doesn't support deletion yet.
        /// </summary>
        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            Logger.LogWarning("DeleteOrderAsync was called, but the backend API doesn't support deletion yet. Order ID: {OrderId}", id);
            await Task.CompletedTask;
            return false;
        }

        private async Task EnrichOrderWithDetailsAsync(OrderDto order)
        {
            // Get customer details
            var customer = await customerService.GetCustomerByIdAsync(order.CustomerId);
            if (customer != null)
            {
                order.CustomerName = customer.Name; // Using the Name property which exists on CustomerDto
            }

            // Get product details
            var product = await productService.GetProductByIdAsync(order.ProductId);
            if (product != null)
            {
                order.ProductName = product.Name;
            }
        }
    }
}
