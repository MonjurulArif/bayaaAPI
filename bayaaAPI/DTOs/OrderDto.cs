namespace bayaaAPI.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }

        public string Division { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public List<OrderItemResponseDto> Items { get; set; } = new();
    }
}
