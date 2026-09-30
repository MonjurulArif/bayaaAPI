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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if(cart == null)
            {
                return Ok(new List<CartItemDto>());
            }

            var items = cart.Items
                .Select(i => new CartItemDto
                {
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    Slug = i.Product.Slug,
                    Price = i.Product.Price,
                    Thumbnail = i.Product.Thumbnail,
                    Quantity = i.Quantity,
                    Stock = i.Product.Stock
                })
                .ToList();

            return Ok(items);
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if(quantity < 1)
            {
                return BadRequest("Quantity must be at least 1");
            }

            var userId = GetUserId();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if(product == null)
            {
                return NotFound("Product not found.");
            }

            if(product.Stock <= 0)
            {
                return BadRequest("Product is out of stock.");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if(cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);

                await _context.SaveChangesAsync();
            }

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if(item == null)
            {
                if(quantity > product.Stock)
                {
                    return BadRequest($"Cannot add {quantity} items to cart. Only {product.Stock} in stock.");
                }

                item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                };

                _context.CartItems.Add(item);
            }
            else
            {
                var newQuantity = item.Quantity + quantity;

                if(newQuantity > product.Stock)
                {
                    return BadRequest($"Cannot add {quantity} items to cart. Only {product.Stock - item.Quantity} more can be added.");
                }

                item.Quantity = newQuantity;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product added to cart successfully."
            });

        }


        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateCartItem(int productId, int quantity)
        {
            if(quantity < 1)
            {
                return BadRequest("Quantity must be at least 1");
            }

            var userId = GetUserId();

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i =>
                    i.ProductId == productId &&
                    i.Cart.UserId == userId);

            if(item == null)
            {
                return NotFound("Cart item not found.");
            }

            if(quantity > item.Product.Stock)
            {
                return BadRequest($"Cannot set quantity to {quantity}. Only {item.Product.Stock} in stock.");
            }

            item.Quantity = quantity;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cart item updated successfully."
            });
        }


        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveCartItem(int productId)
        {
            var userId = GetUserId();

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i =>
                    i.ProductId == productId &&
                    i.Cart.UserId == userId);

            if(item == null)
            {
                return NotFound("Cart item not found.");
            }

            _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product removed from cart."
            });

        }


        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if(cart == null || !cart.Items.Any())
            {
                return Ok(new
                {
                    message = "Cart is already empty."
                });
            }

            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cart cleared successfully."
            });
        }


    }
}
