using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerce.AdminUI.Services;

public class DashboardService(
    IHttpClientFactory httpClientFactory,
    ILogger<DashboardService> logger,
    IHttpContextAccessor httpContextAccessor,
    OrderService orderService)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("ModularMonolith");

    private void AddAuthorizationHeader()
    {
        var token = httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
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
            var orders = await orderService.GetAllOrdersAsync();
            stats.OrderCount = orders.Count;
            stats.TotalSales = orders.Sum(o => o.TotalPrice);

            logger.LogInformation("Retrieved {OrderCount} orders with total sales: ${TotalSales}",
                orders.Count, stats.TotalSales);

            // Log order dates to diagnose the issue
            foreach (var order in orders)
            {
                logger.LogInformation("Order ID: {OrderId}, CreatedAt: {CreatedAt}, Price: ${Price}",
                    order.Id, order.CreatedAt, order.TotalPrice);
            }

            // Get business events
            var eventsResponse = await _httpClient.GetAsync("events");
            if (eventsResponse.IsSuccessStatusCode)
            {
                var events = await eventsResponse.Content.ReadFromJsonAsync<List<BusinessEventDto>>() ?? new List<BusinessEventDto>();
                stats.RecentEvents = events.OrderByDescending(e => e.EventTimestamp).Take(5).ToList();
            }

            // Generate order statistics for chart
            stats.OrderStatistics = GenerateOrderStatistics(orders);

            // Log the generated statistics
            logger.LogInformation("Generated {StatCount} order statistics entries:", stats.OrderStatistics.Count);
            foreach (var stat in stats.OrderStatistics)
            {
                logger.LogInformation("Date: {Date}, OrderCount: {Count}, TotalSales: ${Sales}",
                    stat.Date.ToString("yyyy-MM-dd"), stat.OrderCount, stat.TotalSales);
            }

            // Log the JSON that will be sent to the chart
            logger.LogInformation("Order statistics JSON: {OrderStatsJson}",
                JsonSerializer.Serialize(stats.OrderStatistics));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving dashboard statistics");
        }

        return stats;
    }

    private List<OrderStatistic> GenerateOrderStatistics(List<OrderDto> orders)
    {
        // For demo/initial implementation purpose, if no orders exist, create some sample data
        if (orders == null || !orders.Any())
        {
            logger.LogInformation("No orders found, generating sample statistics");
            return GenerateSampleOrderStatistics();
        }

        // Calculate a date range for the past 7 days
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-6); // This gives us 7 days total (today + 6 previous days)

        logger.LogInformation("Generating statistics from {StartDate} to {EndDate}",
            startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        // Log the orders being considered for the chart
        logger.LogInformation("Orders in date range: {Count}",
            orders.Count(o => o.CreatedAt.Date >= startDate && o.CreatedAt.Date <= endDate));

        // Group orders by day for the last 7 days
        var ordersByDay = orders
            .Where(o => o.CreatedAt.Date >= startDate && o.CreatedAt.Date <= endDate)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new OrderStatistic
            {
                Date = g.Key,
                OrderCount = g.Count(),
                TotalSales = g.Sum(o => o.TotalPrice)
            })
            .ToDictionary(x => x.Date, x => x); // Convert to dictionary for easier lookup

        // Log the unique dates found in orders
        logger.LogInformation("Found orders on these dates: {Dates}",
            string.Join(", ", ordersByDay.Keys.Select(d => d.ToString("yyyy-MM-dd"))));

        // Fill in missing dates with zero values
        var result = new List<OrderStatistic>();
        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            if (ordersByDay.TryGetValue(day, out var existingStat))
            {
                logger.LogInformation("Adding existing stat for {Date}: {Count} orders, ${Sales}",
                    day.ToString("yyyy-MM-dd"), existingStat.OrderCount, existingStat.TotalSales);
                result.Add(existingStat);
            }
            else
            {
                logger.LogInformation("Adding zero stat for {Date} (no orders)", day.ToString("yyyy-MM-dd"));
                result.Add(new OrderStatistic { Date = day, OrderCount = 0, TotalSales = 0 });
            }
        }

        return result;
    }

    private List<OrderStatistic> GenerateSampleOrderStatistics()
    {
        var random = new Random();
        var result = new List<OrderStatistic>();

        // Generate 7 days of sample data instead of 30
        for (int i = 7; i >= 0; i--)
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
