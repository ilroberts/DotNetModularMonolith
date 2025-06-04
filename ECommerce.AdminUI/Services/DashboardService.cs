using System.Net.Http.Json;

namespace ECommerce.AdminUI.Services;

public class DashboardService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DashboardService(IHttpClientFactory httpClientFactory, 
        ILogger<DashboardService> logger, 
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient("ModularMonolith");
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
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
            
            // Get business events
            var eventsResponse = await _httpClient.GetAsync("events");
            if (eventsResponse.IsSuccessStatusCode)
            {
                var events = await eventsResponse.Content.ReadFromJsonAsync<List<BusinessEventDto>>() ?? new List<BusinessEventDto>();
                stats.RecentEvents = events.OrderByDescending(e => e.EventTimestamp).Take(5).ToList();
            }
            
            // Additional data could be fetched here as needed
            // For example, product count, order count, etc.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics");
        }
        
        return stats;
    }
}

public class DashboardStats
{
    public int CustomerCount { get; set; }
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
    public List<BusinessEventDto> RecentEvents { get; set; } = new();
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
