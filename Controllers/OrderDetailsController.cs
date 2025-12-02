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
    public class OrderDetailsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public OrderDetailsController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/orderdetails?limit=10
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails([FromQuery] int? limit)
        {
            try
            {
                const string cacheKey = "TopOrderDetails";

                if (!_cache.TryGetValue(cacheKey, out List<OrderDetail> orderDetails))
                {
                    var query = _context.OrderDetails.AsQueryable();

                    if (limit.HasValue)
                        query = query.Take(limit.Value);

                    orderDetails = await query.ToListAsync();

                    // Cache the result for 5 minutes
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    };
                    _cache.Set(cacheKey, orderDetails, cacheOptions);
                }

                return Ok(orderDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch order details.", error = ex.Message });
            }
        }

        // GET: api/orderdetails/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            try
            {
                var orderDetail = await _context.OrderDetails
                    .FirstOrDefaultAsync(od => od.Id == id);

                if (orderDetail == null)
                    return NotFound(new { message = $"OrderDetail with ID {id} not found." });

                return Ok(orderDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to fetch order detail with ID {id}.", error = ex.Message });
            }
        }
    }
}
