namespace bayaaAPI.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Thumbnail { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int Reviews { get; set; }
        public int Stock { get; set; }
    }
}
