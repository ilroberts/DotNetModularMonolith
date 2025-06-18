using System.Text.Json.Serialization;

namespace ECommerce.AdminUI.Services
{
    /// <summary>
    /// Represents the order structure as returned by the API
    /// </summary>
    public class ApiOrderDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ApiOrderItemDto> Items { get; set; } = new List<ApiOrderItemDto>();
    }

    public class ApiOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
