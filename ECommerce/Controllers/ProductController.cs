using ECommerce.Entities;
using ECommerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }


        [HttpPost("Add")]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("{category}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(string category)
        {
            var products = await _context.Products.Where(p => p.Categories.Any(c => c.Name == category)).ToListAsync();
            return Ok(products);
        }


        [HttpGet("{minPrice}/{maxPrice}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByPrice(decimal minPrice, decimal maxPrice)
        {
            var products = await _context.Products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToListAsync();
            return Ok(products);
        }

        [HttpGet("search/{keyword}")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(string keyword)
        {
            var products = await _context.Products.Where(p => p.Name.Contains(keyword)).ToListAsync();
            return Ok(products);
        }

        
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Product>>> FilterProducts([FromQuery] ProductFilter productFilter)
        {
            var query = _context.Products.AsQueryable();

            if (productFilter.MinPrice != null)
            {
                query = query.Where(p => p.Price >= productFilter.MinPrice);
            }

            if (productFilter.MaxPrice != null)
            {
                query = query.Where(p => p.Price <= productFilter.MaxPrice);
            }

            if (productFilter.Category != null)
            {
                query = query.Where(p => p.Categories.Any(c => c.Name == productFilter.Category));
            }

            var products = await query.ToListAsync();

            return Ok(products);
        }




    }
}
