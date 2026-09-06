namespace bayaaAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Customer
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Customer-facing order number
        public string OrderNumber { get; set; } = string.Empty;

        // Delivery information snapshot
        public string CustomerName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }

        public string Division { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Payment
        public string PaymentMethod { get; set; } = "COD";
        public string PaymentStatus { get; set; } = "Pending";

        // Pricing
        public decimal Subtotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal TotalAmount { get; set; }

        // Order status
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; }
            = new List<OrderItem>();
    }
}
