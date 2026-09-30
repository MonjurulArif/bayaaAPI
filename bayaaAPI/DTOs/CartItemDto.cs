namespace bayaaAPI.DTOs
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Thumbnail { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Stock { get; set; }
    }
}
