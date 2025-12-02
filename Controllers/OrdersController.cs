using ChallengeAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChallengeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/orders?limit=10
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int? limit)
        {
            var query = _context.Orders
                .AsQueryable();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            var orders = await query.ToListAsync();

            return Ok(orders); // plain JSON, only Id and OrderDate
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }
}
