using System.Net.Http.Json;

namespace ECommerce.AdminUI.Services;

public class DashboardService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly OrderService _orderService;

    public DashboardService(
        IHttpClientFactory httpClientFactory,
        ILogger<DashboardService> logger,
        IHttpContextAccessor httpContextAccessor,
        OrderService orderService)
    {
        _httpClient = httpClientFactory.CreateClient("ModularMonolith");
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _orderService = orderService;
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

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var stats = new DashboardStats();

        try
        {
            AddAuthorizationHeader();

            // Get customer count
            var customerResponse = await _httpClient.GetAsync("customers");
            if (customerResponse.IsSuccessStatusCode)
            {
                var customers = await customerResponse.Content.ReadFromJsonAsync<List<CustomerDto>>() ?? new List<CustomerDto>();
                stats.CustomerCount = customers.Count;
            }

            // Get product count
            var productResponse = await _httpClient.GetAsync("products");
            if (productResponse.IsSuccessStatusCode)
            {
                var products = await productResponse.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new List<ProductDto>();
                stats.ProductCount = products.Count;
            }

            // Get orders
            var orders = await _orderService.GetAllOrdersAsync();
            stats.OrderCount = orders.Count;
            stats.TotalSales = orders.Sum(o => o.TotalPrice);

            // Get business events
            var eventsResponse = await _httpClient.GetAsync("events");
            if (eventsResponse.IsSuccessStatusCode)
            {
                var events = await eventsResponse.Content.ReadFromJsonAsync<List<BusinessEventDto>>() ?? new List<BusinessEventDto>();
                stats.RecentEvents = events.OrderByDescending(e => e.EventTimestamp).Take(5).ToList();
            }

            // Generate order statistics for chart
            stats.OrderStatistics = GenerateOrderStatistics(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics");
        }

        return stats;
    }

    private List<OrderStatistic> GenerateOrderStatistics(List<OrderDto> orders)
    {
        // For demo/initial implementation purpose, if no orders exist, create some sample data
        if (orders == null || !orders.Any())
        {
            return GenerateSampleOrderStatistics();
        }

        // Group orders by day for the last 30 days
        var startDate = DateTime.UtcNow.AddDays(-30);
        var ordersByDay = orders
            .Where(o => o.CreatedAt >= startDate)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new OrderStatistic
            {
                Date = g.Key,
                OrderCount = g.Count(),
                TotalSales = g.Sum(o => o.TotalPrice)
            })
            .OrderBy(x => x.Date)
            .ToList();

        // Fill in missing dates
        var result = new List<OrderStatistic>();
        for (var day = startDate.Date; day <= DateTime.UtcNow.Date; day = day.AddDays(1))
        {
            var existingStat = ordersByDay.FirstOrDefault(s => s.Date == day);
            if (existingStat != null)
            {
                result.Add(existingStat);
            }
            else
            {
                result.Add(new OrderStatistic { Date = day, OrderCount = 0, TotalSales = 0 });
            }
        }

        return result;
    }

    private List<OrderStatistic> GenerateSampleOrderStatistics()
    {
        var random = new Random();
        var result = new List<OrderStatistic>();

        // Generate 30 days of sample data
        for (int i = 30; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var orderCount = random.Next(1, 10); // Random number of orders between 1-10
            var totalSales = orderCount * random.Next(50, 200); // Random sales amount

            result.Add(new OrderStatistic
            {
                Date = date,
                OrderCount = orderCount,
                TotalSales = totalSales
            });
        }

        return result;
    }
}

public class DashboardStats
{
    public int CustomerCount { get; set; }
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
    public List<BusinessEventDto> RecentEvents { get; set; } = new();
    public List<OrderStatistic> OrderStatistics { get; set; } = new();
}

public class OrderStatistic
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
}

public class BusinessEventDto
{
    public Guid EventId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset EventTimestamp { get; set; }
    public string ActorId { get; set; } = string.Empty;
    public string ActorType { get; set; } = string.Empty;
}
