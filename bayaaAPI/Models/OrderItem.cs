namespace bayaaAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        // Price at the time of purchase
        public decimal UnitPrice { get; set; }

        // Snapshot of product name
        public string ProductName { get; set; } = string.Empty;
    }
}
