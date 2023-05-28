using ECommerce.Entities;
using ECommerce.Models;
using ECommerce.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("cart/add")]
        public async Task<ActionResult<CartResponse>> AddToCart([FromBody] AddToCartModel model)
        {
            var product = await _context.Products.FindAsync(model.ProductId);

            if (product == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.Include(c => c.CartLines)
                                            .FirstOrDefaultAsync(c => c.UserId == model.UserId);

            if (cart == null)
            {
                cart = new Cart { UserId = model.UserId };
                _context.Carts.Add(cart);
            }

            var cartLine = cart.CartLines?.FirstOrDefault(ci => ci.ProductId == model.ProductId);

            if (cartLine != null)
            {
                cartLine.Quantity += model.Quantity;
            }
            else
            {
                cartLine = new CartLine { ProductId = model.ProductId, Quantity = model.Quantity, Product = product, UserId = model.UserId };
                if(cart.CartLines == null)
                {
                    cart.CartLines = new List<CartLine>();
                }
                cart.CartLines.Add(cartLine);
            }

            await _context.SaveChangesAsync();

            var cartResponse = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartLines = cart.CartLines.Select(cl => new CartLineResponse
                {
                    Id = cl.Id,
                    UserId = cl.UserId,
                    Quantity = cl.Quantity,
                    ProductId = cl.ProductId,
                    TotalPrice = cl.TotalPrice,
                    Product = new ProductResponse
                    {
                        Id = cl.Product.Id,
                        Name = cl.Product.Name,
                        Description = cl.Product.Description,
                        Price = cl.Product.Price,
                        ImageUrl = cl.Product.ImageUrl
                    }
                }).ToList()
            };

            return Ok(cartResponse);
        }


        [HttpGet("cart/{userId}")]
        public async Task<ActionResult<CartResponse>> GetCart(string userId)
        {
            var cart = await _context.Carts.Include(c => c.CartLines)
                                            .ThenInclude(ci => ci.Product)
                                            .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound();
            }

            var cartResponse = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartLines = cart.CartLines.Select(cl => new CartLineResponse
                {
                    Id = cl.Id,
                    UserId = cl.UserId,
                    Quantity = cl.Quantity,
                    ProductId = cl.ProductId,
                    TotalPrice = cl.TotalPrice,
                    Product = new ProductResponse
                    {
                        Id = cl.Product.Id,
                        Name = cl.Product.Name,
                        Description = cl.Product.Description,
                        Price = cl.Product.Price,
                        ImageUrl = cl.Product.ImageUrl
                    }
                }).ToList()
            };

            return Ok(cartResponse);
        }

        [HttpDelete("cart/remove")]
        public async Task<ActionResult> RemoveFromCart([FromBody] CartLine item)
        {
            var cart = await _context.Carts.Include(c => c.CartLines)
                                            .FirstOrDefaultAsync(c => c.UserId == item.UserId);

            if (cart == null)
            {
                return NotFound();
            }

            var cartLine = cart.CartLines.FirstOrDefault(ci => ci.ProductId == item.ProductId);

            if (cartLine == null)
            {
                return NotFound();
            }

            if (cartLine.Quantity > item.Quantity)
            {
                cartLine.Quantity -= item.Quantity;
            }
            else
            {
                cart.CartLines.Remove(cartLine);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("cart/total/{userId}")]
        public async Task<ActionResult<decimal>> GetTotalValue(string userId)
        {
            var cart = await _context.Carts.Include(c => c.CartLines)
                                            .ThenInclude(ci => ci.Product)
                                            .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound();
            }

            var totalValue = cart.ComputeTotalValue();

            return Ok(totalValue);
        }



    }
}
