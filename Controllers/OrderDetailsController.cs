using ChallengeAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChallengeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrderDetailsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/orderdetails?limit=10
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails([FromQuery] int? limit)
        {
            var query = _context.OrderDetails.AsQueryable();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            var orderDetails = await query.ToListAsync();

            return Ok(orderDetails); // plain JSON
        }

        // GET: api/orderdetails/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(od => od.Id == id);

            if (orderDetail == null)
                return NotFound();

            return Ok(orderDetail);
        }
    }
}
