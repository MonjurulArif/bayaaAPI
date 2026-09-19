using bayaaAPI.Data;
using bayaaAPI.DTOs;
using bayaaAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bayaaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {

            if(dto.Items == null || dto.Items.Count == 0)
            {
                return BadRequest("Cart is empty");
            }

            if(string.IsNullOrWhiteSpace(dto.CustomerName) ||
                string.IsNullOrWhiteSpace(dto.Mobile) ||
                string.IsNullOrWhiteSpace(dto.Division) ||
                string.IsNullOrWhiteSpace(dto.District) ||
                string.IsNullOrWhiteSpace(dto.Area) ||
                string.IsNullOrWhiteSpace(dto.Address))
            {
                return BadRequest("Delivery information is incomplete.");
            }

            if(string.IsNullOrWhiteSpace(dto.PaymentMethod))
            {
                return BadRequest("Payment method is required.");
            }

            var allowedPaymentMethods = new[]
            {
                "COD", "Bkash", "Nagad", "Rocket", "Card"
            };

            if(!allowedPaymentMethods.Contains(
                    dto.PaymentMethod,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid payment method.");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Combine duplicate products in the cart
            var items = dto.Items
                .GroupBy(x => x.ProductId)
                .Select(g => new CreateOrderItemDto
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var productIds = items.Select(x => x.ProductId).Distinct().ToList();

                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                if(products.Count != productIds.Count)
                {
                    var missingIds = productIds.Except(products.Keys);
                    return BadRequest($"Products not found: {string.Join(", ", missingIds)}");
                }

                decimal subtotal = 0;

                var orderItems = new List<OrderItem>();

                foreach(var item in items)
                {
                    if(item.Quantity <= 0)
                    {
                        return BadRequest($"Invalid quantity for product ID {item.ProductId}.");
                    }

                    var product = products[item.ProductId];

                    if(product.Stock < item.Quantity)
                    {
                        return BadRequest(
                            $"Insufficient stock for product {product.Name}. " +
                            $"Available: {product.Stock}, " +
                            $"Requested: {item.Quantity}");
                    }

                    var itemTotal = product.Price * item.Quantity;

                    subtotal += itemTotal;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });

                    product.Stock -= item.Quantity;
                }

                // Backend logic for calculating delivery charge based on district or other criteria
                decimal deliveryCharge = 70; // Default

                decimal totalAmount = subtotal + deliveryCharge;

                var randomPart = Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 8)
                    .ToUpper();

                var orderNumber =
                    $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{randomPart}";

                var order = new Order
                {
                    UserId = userId,

                    OrderNumber = orderNumber,

                    CustomerName = dto.CustomerName.Trim(),
                    Mobile = dto.Mobile.Trim(),
                    Email = dto.Email?.Trim(),

                    Division = dto.Division.Trim(),
                    District = dto.District.Trim(),
                    Area = dto.Area.Trim(),
                    Address = dto.Address.Trim(),

                    PaymentMethod = dto.PaymentMethod.Trim(),
                    PaymentStatus = "Pending",

                    Subtotal = subtotal,
                    DeliveryCharge = deliveryCharge,
                    TotalAmount = totalAmount,

                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,

                    Items = orderItems
                };

                _context.Orders.Add(order);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    id = order.Id,
                    orderNumber = order.OrderNumber,
                    subtotal = order.Subtotal,
                    deliveryCharge = order.DeliveryCharge,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                });
            }

            catch(DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                return Conflict("Product stock changed while placing the order. Please review your cart and try again.");
            }

            catch(Exception)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, "An error occurred while creating the order.");
            }

        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,

                    CustomerName = o.CustomerName,
                    Mobile = o.Mobile,
                    Email = o.Email,

                    Division = o.Division,
                    District = o.District,
                    Area = o.Area,
                    Address = o.Address,

                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,

                    Subtotal = o.Subtotal,
                    DeliveryCharge = o.DeliveryCharge,
                    TotalAmount = o.TotalAmount,

                    Status = o.Status,
                    CreatedAt = o.CreatedAt,

                    Items = o.Items
                    .Select(i => new OrderItemResponseDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                    })
                    .ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Include(o => o.Items)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,

                    CustomerName = o.CustomerName,
                    Mobile = o.Mobile,
                    Email = o.Email,

                    Division = o.Division,
                    District = o.District,
                    Area = o.Area,
                    Address = o.Address,

                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,

                    Subtotal = o.Subtotal,
                    DeliveryCharge = o.DeliveryCharge,
                    TotalAmount = o.TotalAmount,

                    Status = o.Status,
                    CreatedAt = o.CreatedAt,

                    Items = o.Items
                        .Select(i => new OrderItemResponseDto
                        {
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            UnitPrice = i.UnitPrice,
                            Quantity = i.Quantity,
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if(order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order);
        }


        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if(order == null)
            {
                return NotFound("Order not found");
            }

            var allowedStatuses = new[]
            {
                "Pending",
                "Processing",
                "Shipped",
                "Delivered",
                "Cancelled"
            };

            if(!allowedStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid order status.");
            }

            order.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                order.Id,
                order.OrderNumber,
                order.Status
            });

        }


        [HttpGet("admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.CustomerName,
                    o.Mobile,
                    o.TotalAmount,
                    o.Status,
                    o.CreatedAt
                })
                .ToListAsync();

            return Ok(orders);


        }
    }
}
