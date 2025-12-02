using ChallengeAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChallengeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PizzasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PizzasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/pizzas
        [HttpGet]
        public async Task<IActionResult> GetPizzas([FromQuery] int? limit)
        {
            var query = _context.Pizzas.AsQueryable();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            var pizzas = await query.ToListAsync();
            return Ok(pizzas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPizza(string id)
        {
            var pizza = await _context.Pizzas.FirstOrDefaultAsync(p => p.Id == id);

            if (pizza == null)
                return NotFound();

            return Ok(pizza);
        }
    }
}
