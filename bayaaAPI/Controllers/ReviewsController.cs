using bayaaAPI.Data;
using bayaaAPI.DTOs;
using bayaaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bayaaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.FirstName + " " + r.User.LastName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview(int productId, CreateReviewDto dto)
        {
            if(dto.Rating < 1 || dto.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if(product == null)
            {
                return NotFound("Product not found");
            }

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ProductId == productId && r.UserId == userId);

            if(alreadyReviewed)
            {
                return BadRequest("You have already reviewed this product");
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim() ?? "",
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);

            await _context.SaveChangesAsync();

            var averageRating = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (double)r.Rating);

            var reviewCount = await _context.Reviews
                .CountAsync(r => r.ProductId == productId);

            product.Rating = (decimal)Math.Round(averageRating, 1);
            product.Reviews = reviewCount;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Review added successfully."
            });
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview( int id, UpdateReviewDto dto)
        {
            if(dto.Rating < 1 || dto.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if(review == null)
            {
                return NotFound("Review not found.");
            }

            if(review.UserId != userId)
            {
                return Forbid();
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment?.Trim() ?? "";
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await UpdateProductRating(review.ProductId);

            return Ok(new
            {
                message = "Review updated successfully."
            });
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if(review == null)
            {
                return NotFound("Review not found.");
            }

            if(review.UserId != userId)
            {
                return Forbid();
            }

            var productId = review.ProductId;

            _context.Reviews.Remove(review);

            await _context.SaveChangesAsync();

            await UpdateProductRating(productId);

            return Ok(new
            {
                message = "Review deleted successfully."
            });
        }

        private async Task UpdateProductRating(int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if(product == null)
            {
                return;
            }

            var reviewCount = await _context.Reviews
                .CountAsync(r => r.ProductId == productId);
                
            product.Reviews = reviewCount;        

            product.Rating = reviewCount == 0 ? 0
                : (decimal)Math.Round(
                    await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .AverageAsync(r => r.Rating), 1);

            await _context.SaveChangesAsync();
        }



    }
}
