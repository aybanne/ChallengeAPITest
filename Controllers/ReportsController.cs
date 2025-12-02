using ChallengeAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChallengeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // ==============================
        // 1. Sales summary
        // ==============================
        // GET: api/reports/sales-summary
        [HttpGet("sales-summary")]
        public async Task<IActionResult> GetSalesSummary()
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

            var report = new
            {
                TotalOrders = totalOrders,
                TotalPizzasSold = totalPizzasSold,
                TotalSales = totalSales,
                AverageOrderValue = avgOrderValue,
                TopPizzaTypes = topPizzaTypes
            };

            return Ok(report);
        }

        // ==============================
        // 2. Daily sales
        // ==============================
        // GET: api/reports/daily-sales
        [HttpGet("daily-sales")]
        public async Task<IActionResult> GetDailySales()
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

        // ==============================
        // 3. Top pizzas
        // ==============================
        // GET: api/reports/top-pizzas
        [HttpGet("top-pizzas")]
        public async Task<IActionResult> GetTopPizzas()
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

        // ==============================
        // 4. Pizza size popularity
        // ==============================
        // GET: api/reports/pizza-sizes
        [HttpGet("pizza-sizes")]
        public async Task<IActionResult> GetPizzaSizePopularity()
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

        // ==============================
        // 5. Top pizza types by revenue
        // ==============================
        // GET: api/reports/top-pizza-types
        [HttpGet("top-pizza-types")]
        public async Task<IActionResult> GetTopPizzaTypesByRevenue()
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
    }
}
