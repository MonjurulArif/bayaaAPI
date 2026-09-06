namespace bayaaAPI.DTOs
{
    public class CreateOrderDto
    {
        public List<CreateOrderItemDto> Items { get; set; } = new();

        public string CustomerName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }

        public string Division { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "COD";
    }
}
