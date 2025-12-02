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
    public class PizzasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public PizzasController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/pizzas?limit=10
        [HttpGet]
        public async Task<IActionResult> GetPizzas([FromQuery] int? limit)
        {
            try
            {
                const string cacheKey = "TopPizzas";

                if (!_cache.TryGetValue(cacheKey, out List<Pizza> pizzas))
                {
                    var query = _context.Pizzas.AsQueryable();

                    if (limit.HasValue)
                        query = query.Take(limit.Value);

                    pizzas = await query.ToListAsync();

                    // Cache for 5 minutes
                    _cache.Set(cacheKey, pizzas, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
                }

                return Ok(pizzas.Select(p => new { p.Id, p.PizzaTypeId, p.Size, p.Price }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch pizzas.", error = ex.Message });
            }
        }

        // GET: api/pizzas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPizza(string id)
        {
            try
            {
                var pizza = await _context.Pizzas.FirstOrDefaultAsync(p => p.Id == id);

                if (pizza == null)
                    return NotFound(new { message = $"Pizza with ID '{id}' not found." });

                return Ok(pizza);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to fetch pizza with ID '{id}'.", error = ex.Message });
            }
        }
    }
}
