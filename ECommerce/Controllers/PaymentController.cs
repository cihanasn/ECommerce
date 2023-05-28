using ECommerce.Entities;
using ECommerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("payments")]
        public async Task<ActionResult<Payment>> CreatePayment([FromBody] Payment payment)
        {
            // Ödeme işlemini gerçekleştiren servisi burada çağırabilirsiniz.
            // Servisten geri dönen sonucu Payment nesnesine doldurarak kaydedebilirsiniz.

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }

        [HttpGet("payments/{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpPost("orders/{orderId}/pay")]
        public async Task<ActionResult> PayOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Ödeme işlemini gerçekleştiren servisi burada çağırabilirsiniz.
            // Servisten geri dönen sonucu Order nesnesine doldurarak kaydedebilirsiniz.

            order.IsPaid = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}
