
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
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] ProductQueryDto query)
        {

            // Fix invalid pagination values
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

            // Get all products with their categories by joining the Products and Categories tables
            var products = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Search Filter
            if(!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = $"%{query.Search}%";

                products = products.Where(p => 
                   EF.Functions.ILike(p.Name, search) ||
                   EF.Functions.ILike(p.Description, search));                
            }

            // Category Filter
            if(query.CategoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == query.CategoryId.Value);
            }

            //Min Price Filter
            if(query.MinPrice.HasValue)
            {
                products = products.Where(p => p.Price >= query.MinPrice.Value);
            }

            //Max Price Filter
            if(query.MaxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= query.MaxPrice.Value);
            }

            // Rating Filter
            if(query.Rating.HasValue)
            {
                products = products.Where(p => p.Rating >= query.Rating.Value);
            }

            // In Stock Filter
            if(query.InStock == true)
            {
                products = products.Where(p => p.Stock > 0);
            }

            // Sorting
            if(!string.IsNullOrWhiteSpace(query.Sort))
            {
                switch(query.Sort.ToLower())
                {
                    case "price-asc":
                        products = products.OrderBy(p => p.Price);
                        break;
                    case "price-desc":
                        products = products.OrderByDescending(p => p.Price);
                        break;
                    case "name-asc":
                        products = products.OrderBy(p => p.Name);
                        break;
                    case "name-desc":
                        products = products.OrderByDescending(p => p.Name);
                        break;
                    default:
                        products = products.OrderBy(p => p.Id);
                        break;
                }
            }
            else
            {
                products = products.OrderBy(p => p.Id);
            }

            var totalProducts = await products.CountAsync();


            // Pagination
            var items = await products
                .Skip((page - 1) * pageSize)
                .Take(query.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Slug = p.Slug,
                    Name = p.Name,
                    Price = p.Price,
                    Thumbnail = p.Thumbnail,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null ? p.Category.Name : string.Empty,
                    Rating = p.Rating,
                    Reviews = p.Reviews,
                    Stock = p.Stock
                })
                .ToListAsync();

            // Response
            return Ok(new
            {
                TotalProducts = totalProducts,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling( totalProducts / (double)pageSize),
                Products = items
            });
        }


        [HttpGet("{slug}")]
        public async Task<ActionResult<ProductDto>> GetProductBySlug(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if(product == null)
            {
                return NotFound();
            }

            return Ok(new ProductDto
            {
                Id = product.Id,
                Slug = product.Slug,
                Name = product.Name,
                Price = product.Price,
                Thumbnail = product.Thumbnail,
                CategoryId = product.CategoryId,
                Category = product.Category != null ? product.Category.Name : string.Empty,
                Rating = product.Rating,
                Reviews = product.Reviews,
                Stock = product.Stock
            });
        }


        [HttpGet("category/{slug}")]
        public async Task<ActionResult> GetProductsByCategory(
            string slug, [FromQuery] ProductQueryDto query)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Category != null && p.Category.Slug == slug)
                .AsQueryable();

            // Search
            if(!string.IsNullOrWhiteSpace(query.Search))
            {
                products = products.Where(p =>
                    p.Name.Contains(query.Search) ||
                    p.Description.Contains(query.Search));
            }

            // Min price
            if(query.MinPrice.HasValue)
            {
                products = products.Where(
                    p => p.Price >= query.MinPrice.Value);
            }

            // Max price
            if(query.MaxPrice.HasValue)
            {
                products = products.Where(
                    p => p.Price <= query.MaxPrice.Value);
            }

            // Rating filter            

            // Stock
            if(query.InStock == true)
            {
                products = products.Where(p => p.Stock > 0);
            }

            // Sorting
            switch(query.Sort)
            {
                case "price-asc":
                products = products.OrderBy(p => p.Price);
                break;

                case "price-desc":
                products = products.OrderByDescending(p => p.Price);
                break;

                case "name-asc":
                products = products.OrderBy(p => p.Name);
                break;

                case "name-desc":
                products = products.OrderByDescending(p => p.Name);
                break;

                default:
                products = products.OrderBy(p => p.Id);
                break;
            }

            var totalProducts = await products.CountAsync();

            var items = await products
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Slug = p.Slug,
                    Name = p.Name,
                    Price = p.Price,
                    Thumbnail = p.Thumbnail,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null ? p.Category.Name : string.Empty,
                    Rating = p.Rating,
                    Reviews = p.Reviews,
                    Stock = p.Stock
                })
                .ToListAsync();

            return Ok(new
            {
                TotalProducts = totalProducts,
                Page = query.Page,
                PageSize = query.PageSize,
                Products = items
            });
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


        [HttpGet("id/{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

            if(product == null)
            {
                return NotFound();
            }

            var dto = new ProductDto
            {
                Id = product.Id,
                Slug = product.Slug,
                Name = product.Name,
                Price = product.Price,
                Thumbnail = product.Thumbnail,
                Category = product.Category?.Name ?? string.Empty,
                Rating = product.Rating,
                Reviews = product.Reviews,
                Stock = product.Stock
            };

            return Ok(dto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);

            if(product == null)
            {
                return NotFound();
            }

            product.Slug = dto.Slug;
            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Thumbnail = dto.Thumbnail;
            product.Description = dto.Description;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if(product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
