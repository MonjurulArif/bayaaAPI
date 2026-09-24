namespace bayaaAPI.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Slug { get; set; } = "";

        public string Name { get; set; } = "";

        public decimal Price { get; set; }

        public string Thumbnail { get; set; } = "";

        public int? CategoryId { get; set; }

        public string Category { get; set; } = "";

        public string Description { get; set; } = "";

        public decimal? Rating { get; set; }

        public int Reviews { get; set; }

        public int Stock { get; set; }
    }
}
