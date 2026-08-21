
using bayaaAPI.Data;
using bayaaAPI.DTOs;
using bayaaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bayaaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products.Select(p => new ProductDto
            {
                Id = p.Id,
                Slug = p.Slug,
                Name = p.Name,
                Price = p.Price,
                Thumbnail = p.Thumbnail,
                Category = p.Category != null ? p.Category.Name : string.Empty,
                Rating = p.Rating,
                Reviews = p.Reviews,
                Stock = p.Stock
            }).ToListAsync();
            return Ok(products);
        }


        [HttpPost]
        public async Task<ActionResult> CreateProduct(CreateProductDto dto)
        {
            var product = new Product
            {
                Slug = dto.Slug,
                Name = dto.Name,
                Price = dto.Price,
                Thumbnail = dto.Thumbnail,
                Description = dto.Description,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }
    }
}
