using ChallengeAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ChallengeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public ReportsController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // ==============================
        // 1. Sales summary
        // ==============================
        [HttpGet("sales-summary")]
        public async Task<IActionResult> GetSalesSummary()
        {
            try
            {
                const string cacheKey = "SalesSummary";

                if (!_cache.TryGetValue(cacheKey, out object report))
                {
                    var totalOrders = await _context.Orders.CountAsync();
                    var totalPizzasSold = await _context.OrderDetails.SumAsync(od => od.Quantity);
                    var totalSales = await _context.OrderDetails
                        .Include(od => od.Pizza)
                        .SumAsync(od => od.Quantity * od.Pizza.Price);
                    var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

                    var topPizzaTypes = await _context.OrderDetails
                        .Include(od => od.Pizza)
                        .ThenInclude(p => p.PizzaType)
                        .GroupBy(od => od.Pizza.PizzaType.Name)
                        .Select(g => new
                        {
                            PizzaType = g.Key,
                            QuantitySold = g.Sum(x => x.Quantity)
                        })
                        .OrderByDescending(x => x.QuantitySold)
                        .Take(5)
                        .ToListAsync();

                    report = new
                    {
                        TotalOrders = totalOrders,
                        TotalPizzasSold = totalPizzasSold,
                        TotalSales = totalSales,
                        AverageOrderValue = avgOrderValue,
                        TopPizzaTypes = topPizzaTypes
                    };

                    _cache.Set(cacheKey, report, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to generate sales summary.", error = ex.Message });
            }
        }

        // ==============================
        // 2. Daily sales
        // ==============================
        [HttpGet("daily-sales")]
        public async Task<IActionResult> GetDailySales()
        {
            try
            {
                var dailySales = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Pizza)
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalOrders = g.Count(),
                        TotalPizzasSold = g.Sum(o => o.OrderDetails.Sum(od => od.Quantity)),
                        TotalSales = g.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Pizza.Price))
                    })
                    .OrderByDescending(x => x.Date)
                    .ToListAsync();

                return Ok(dailySales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch daily sales.", error = ex.Message });
            }
        }

        // ==============================
        // 3. Top pizzas
        // ==============================
        [HttpGet("top-pizzas")]
        public async Task<IActionResult> GetTopPizzas()
        {
            try
            {
                var topPizzas = await _context.OrderDetails
                    .Include(od => od.Pizza)
                    .ThenInclude(p => p.PizzaType)
                    .GroupBy(od => od.Pizza.PizzaType.Name)
                    .Select(g => new
                    {
                        PizzaName = g.Key,
                        QuantitySold = g.Sum(od => od.Quantity),
                        TotalSales = g.Sum(od => od.Quantity * od.Pizza.Price)
                    })
                    .OrderByDescending(x => x.QuantitySold)
                    .Take(10)
                    .ToListAsync();

                return Ok(topPizzas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch top pizzas.", error = ex.Message });
            }
        }

        // ==============================
        // 4. Pizza size popularity
        // ==============================
        [HttpGet("pizza-sizes")]
        public async Task<IActionResult> GetPizzaSizePopularity()
        {
            try
            {
                var sizeStats = await _context.OrderDetails
                    .Include(od => od.Pizza)
                    .GroupBy(od => od.Pizza.Size)
                    .Select(g => new
                    {
                        Size = g.Key,
                        QuantitySold = g.Sum(od => od.Quantity),
                        TotalSales = g.Sum(od => od.Quantity * od.Pizza.Price)
                    })
                    .OrderByDescending(x => x.QuantitySold)
                    .ToListAsync();

                return Ok(sizeStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch pizza size stats.", error = ex.Message });
            }
        }

        // ==============================
        // 5. Top pizza types by revenue
        // ==============================
        [HttpGet("top-pizza-types")]
        public async Task<IActionResult> GetTopPizzaTypesByRevenue()
        {
            try
            {
                var topRevenueTypes = await _context.OrderDetails
                    .Include(od => od.Pizza)
                    .ThenInclude(p => p.PizzaType)
                    .GroupBy(od => od.Pizza.PizzaType.Name)
                    .Select(g => new
                    {
                        PizzaType = g.Key,
                        QuantitySold = g.Sum(od => od.Quantity),
                        TotalRevenue = g.Sum(od => od.Quantity * od.Pizza.Price)
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .Take(5)
                    .ToListAsync();

                return Ok(topRevenueTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch top pizza types by revenue.", error = ex.Message });
            }
        }
    }
}
