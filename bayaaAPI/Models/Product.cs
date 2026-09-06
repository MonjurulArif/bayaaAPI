using System.ComponentModel.DataAnnotations;

namespace bayaaAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Thumbnail { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Rating { get; set; }

        public int Reviews { get; set; }

        public int Stock { get; set; }

        public uint Version { get; set; }

        public int? CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
