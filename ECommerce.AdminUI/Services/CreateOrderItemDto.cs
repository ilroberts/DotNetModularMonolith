namespace ECommerce.AdminUI.Services
{
    /// <summary>
    /// DTO for creating an order item (used in POST request)
    /// </summary>
    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}

