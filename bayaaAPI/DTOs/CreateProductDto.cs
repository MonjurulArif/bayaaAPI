namespace bayaaAPI.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int Stock { get; set; }
    }
}
