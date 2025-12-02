using ChallengeAPI.Data;
using ChallengeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChallengeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PizzaTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PizzaTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/pizzatypes
        [HttpGet]
        public async Task<IActionResult> GetPizzaTypes([FromQuery] int? limit)
        {
            var query = _context.PizzaTypes.AsQueryable();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            var pizzaTypes = await query.ToListAsync();
            return Ok(pizzaTypes);
        }

        // GET: api/pizzatypes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPizzaType(string id)
        {
            var pizzaType = await _context.PizzaTypes.FirstOrDefaultAsync(pt => pt.Id == id);

            if (pizzaType == null)
                return NotFound();

            return Ok(pizzaType);
        }
    }
}
