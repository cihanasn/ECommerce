using ECommerce.Entities;
using ECommerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("orders/create")]
        public async Task<ActionResult> CreateOrder([FromBody] OrderModel model)
        {
            var cart = await _context.Carts.Include(c => c.CartLines)
                                            .ThenInclude(ci => ci.Product)
                                            .FirstOrDefaultAsync(c => c.UserId == model.UserId);

            if (cart == null)
            {
                return NotFound();
            }

            var order = new Order { UserId = model.UserId, OrderDate = DateTime.UtcNow, OrderLines = new List<OrderLine>() };

            foreach (var item in cart.CartLines)
            {
                var orderLine = new OrderLine
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };

                order.OrderLines.Add(orderLine);
            }

            order.ShipAddress = model.ShipAddress;
            order.ShipCity = model.ShipCity;
            order.ShipCountry = model.ShipCountry;
            order.ShipZip = model.ShipZip;

            order.Total = cart.ComputeTotalValue();  // Sepetin toplam fiyatı hesaplanarak Total alanına atama yapılmıştır.
            
            order.IsPaid = false;

            var payment = new Payment { PaymentDate = DateTime.UtcNow, Amount = order.Total };
            _context.Payments.Add(payment);
            order.Payment = payment;

            _context.Orders.Add(order);
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("orders/{userId}")]
        public async Task<ActionResult<List<Order>>> GetOrders(string userId)
        {
            var orders = await _context.Orders.Include(o => o.OrderLines)
                                               .ThenInclude(oi => oi.Product)
                                               .Where(o => o.UserId == userId)
                                               .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound();
            }

            return Ok(orders);
        }

        [HttpGet("users/{userId}/orders")]
        public async Task<ActionResult<List<Order>>> GetOrderHistory(string userId)
        {
            var orders = await _context.Orders.Include(o => o.OrderLines)
                                               .ThenInclude(oi => oi.Product)
                                               .Where(o => o.UserId == userId)
                                               .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound();
            }

            return Ok(orders);
        }

        [HttpGet("users/{userId}/orders/{orderId}")]
        public async Task<ActionResult<Order>> GetOrder(string userId, int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderLines)
                                              .ThenInclude(oi => oi.Product)
                                              .FirstOrDefaultAsync(o => o.UserId == userId && o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }


    }
}
