using bayaaAPI.Data;
using bayaaAPI.DTOs;
using bayaaAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bayaaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WishlistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist( AddWishlistDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            
            var existItem = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == dto.ProductId);

            if (existItem)
            {
                return Ok();
            }
            
            _context.WishlistItems.Add(
                new WishlistItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    CreatedAt = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product added to wishlist." });
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var wishlistItem = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem == null)
            {
                return NotFound(new { message = "Product not found in wishlist." });
            }

            _context.WishlistItems.Remove(wishlistItem);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product removed from wishlist." });
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var wishlistItems = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                .Select(w => new
                {
                    w.Product.Id,
                    w.Product.Name,
                    w.Product.Slug,
                    w.Product.Price,
                    w.Product.Thumbnail,
                    w.Product.Rating
                })
                .ToListAsync();

            return Ok(wishlistItems);
        }

    }
}
