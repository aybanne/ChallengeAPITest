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
    public class PizzaTypesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public PizzaTypesController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/pizzatypes?limit=10
        [HttpGet]
        public async Task<IActionResult> GetPizzaTypes([FromQuery] int? limit)
        {
            try
            {
                const string cacheKey = "TopPizzaTypes";

                if (!_cache.TryGetValue(cacheKey, out List<PizzaType> pizzaTypes))
                {
                    var query = _context.PizzaTypes.AsQueryable();

                    if (limit.HasValue)
                        query = query.Take(limit.Value);

                    pizzaTypes = await query.ToListAsync();

                    // Cache for 5 minutes
                    _cache.Set(cacheKey, pizzaTypes, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
                }

                return Ok(pizzaTypes.Select(pt => new { pt.Id, pt.Name, pt.Category, pt.Ingredients }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch pizza types.", error = ex.Message });
            }
        }

        // GET: api/pizzatypes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPizzaType(string id)
        {
            try
            {
                var pizzaType = await _context.PizzaTypes.FirstOrDefaultAsync(pt => pt.Id == id);

                if (pizzaType == null)
                    return NotFound(new { message = $"PizzaType with ID '{id}' not found." });

                return Ok(pizzaType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to fetch pizza type with ID '{id}'.", error = ex.Message });
            }
        }
    }
}
