using ChallengeAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;

namespace ChallengeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly CsvService _csvService;

        public ImportController(CsvService csvService)
        {
            _csvService = csvService;
        }

        // DTO for file uploads
        public class FileUploadRequest
        {
            public IFormFile File { get; set; }
        }

        // Thread-safe dictionary to store metrics per file type
        private static readonly ConcurrentDictionary<string, ImportMetrics> _metrics =
            new ConcurrentDictionary<string, ImportMetrics>();

        public class ImportMetrics
        {
            public int RowsProcessed { get; set; }
            public int RowsImported { get; set; }
            public int RowsSkipped { get; set; }
            public List<string> SkippedIds { get; set; } = new();
            public long DurationMs { get; set; } // duration in milliseconds
        }

        // ===========================
        // Metrics endpoint
        // GET: api/import/metrics/{type}
        // ===========================
        [HttpGet("metrics/{type}")]
        public IActionResult GetMetrics(string type)
        {
            if (_metrics.TryGetValue(type.ToLower(), out var metrics))
                return Ok(metrics);

            return NotFound(new { message = $"No metrics available for '{type}'" });
        }

        // Helper: update metrics from CsvService after import
        internal static void SetMetrics(string type, int processed, int imported, int skipped, List<string> skippedIds, long durationMs)
        {
            _metrics[type.ToLower()] = new ImportMetrics
            {
                RowsProcessed = processed,
                RowsImported = imported,
                RowsSkipped = skipped,
                SkippedIds = skippedIds,
                DurationMs = durationMs
            };
        }

        // ===========================
        // Import endpoints
        // ===========================

        [HttpPost("orders")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportOrders([FromForm] FileUploadRequest request)
        {
            if (request.File == null)
                return BadRequest(new { message = "Orders file is required." });

            try
            {
                await using var stream = await GetDecompressedStream(request.File);

                var stopwatch = Stopwatch.StartNew();
                var result = await _csvService.ImportOrdersCsvAsync(stream);
                stopwatch.Stop();

                SetMetrics("orders", result.Total, result.Imported, result.Skipped, result.SkippedIds, stopwatch.ElapsedMilliseconds);

                return Ok(new
                {
                    message = "Orders imported successfully.",
                    metrics = result,
                    durationMs = stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to import orders.", error = ex.Message });
            }
        }

        [HttpPost("orderdetails")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportOrderDetails([FromForm] FileUploadRequest request)
        {
            if (request.File == null)
                return BadRequest(new { message = "OrderDetails file is required." });

            try
            {
                await using var stream = await GetDecompressedStream(request.File);

                var stopwatch = Stopwatch.StartNew();
                var result = await _csvService.ImportOrderDetailsCsvAsync(stream);
                stopwatch.Stop();

                SetMetrics("orderdetails", result.Total, result.Imported, result.Skipped, result.SkippedIds, stopwatch.ElapsedMilliseconds);

                return Ok(new
                {
                    message = "OrderDetails imported successfully.",
                    metrics = result,
                    durationMs = stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to import order details.", error = ex.Message });
            }
        }

        [HttpPost("pizzatypes")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportPizzaTypes([FromForm] FileUploadRequest request)
        {
            if (request.File == null)
                return BadRequest(new { message = "PizzaTypes file is required." });

            try
            {
                await using var stream = await GetDecompressedStream(request.File);

                var stopwatch = Stopwatch.StartNew();
                var result = await _csvService.ImportPizzaTypesCsvAsync(stream);
                stopwatch.Stop();

                SetMetrics("pizzatypes", result.Total, result.Imported, result.Skipped, result.SkippedIds, stopwatch.ElapsedMilliseconds);

                return Ok(new
                {
                    message = "PizzaTypes imported successfully.",
                    metrics = result,
                    durationMs = stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to import pizza types.", error = ex.Message });
            }
        }

        [HttpPost("pizzas")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportPizzas([FromForm] FileUploadRequest request)
        {
            if (request.File == null)
                return BadRequest(new { message = "Pizzas file is required." });

            try
            {
                await using var stream = await GetDecompressedStream(request.File);

                var stopwatch = Stopwatch.StartNew();
                var result = await _csvService.ImportPizzasCsvAsync(stream);
                stopwatch.Stop();

                SetMetrics("pizzas", result.Total, result.Imported, result.Skipped, result.SkippedIds, stopwatch.ElapsedMilliseconds);

                return Ok(new
                {
                    message = "Pizzas imported successfully.",
                    metrics = result,
                    durationMs = stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to import pizzas.", error = ex.Message });
            }
        }

        // Helper: decompress GZip file if needed
        private async Task<Stream> GetDecompressedStream(IFormFile file)
        {
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Check if file is GZip (starts with 1F 8B)
            if (memoryStream.Length >= 2)
            {
                memoryStream.Position = 0;
                var signature = new byte[2];
                memoryStream.Read(signature, 0, 2);
                memoryStream.Position = 0;
                if (signature[0] == 0x1F && signature[1] == 0x8B)
                {
                    return new GZipStream(memoryStream, CompressionMode.Decompress);
                }
            }

            return memoryStream; // normal CSV
        }
    }
}
