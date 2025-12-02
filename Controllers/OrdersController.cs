using ChallengeAPI.Data;
using ChallengeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ChallengeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public OrdersController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/orders?limit=10
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int? limit)
        {
            try
            {
                const string cacheKey = "TopOrders";

                if (!_cache.TryGetValue(cacheKey, out List<Order> orders))
                {
                    var query = _context.Orders.AsQueryable();

                    if (limit.HasValue)
                        query = query.Take(limit.Value);

                    orders = await query.ToListAsync();

                    // Cache the result for 5 minutes
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    };
                    _cache.Set(cacheKey, orders, cacheOptions);
                }

                return Ok(orders.Select(o => new { o.Id, o.OrderDate }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch orders.", error = ex.Message });
            }
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound(new { message = $"Order with ID {id} not found." });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to fetch order with ID {id}.", error = ex.Message });
            }
        }
    }
}
