using ChallengeAPI.Services;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("import-orders")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportOrders([FromForm] FileUploadRequest request)
        {
            if (request.File == null) return BadRequest("Orders file required.");
            await using var stream = await GetDecompressedStream(request.File);
            await _csvService.ImportOrdersCsvAsync(stream);
            return Ok("Orders imported successfully.");
        }

        [HttpPost("import-orderdetails")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportOrderDetails([FromForm] FileUploadRequest request)
        {
            if (request.File == null) return BadRequest("OrderDetails file required.");
            await using var stream = await GetDecompressedStream(request.File);
            await _csvService.ImportOrderDetailsCsvAsync(stream);
            return Ok("OrderDetails imported successfully.");
        }

        [HttpPost("import-pizzatypes")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportPizzaTypes([FromForm] FileUploadRequest request)
        {
            if (request.File == null) return BadRequest("PizzaTypes file required.");
            await using var stream = await GetDecompressedStream(request.File);
            await _csvService.ImportPizzaTypesCsvAsync(stream);
            return Ok("PizzaTypes imported successfully.");
        }

        [HttpPost("import-pizzas")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportPizzas([FromForm] FileUploadRequest request)
        {
            if (request.File == null) return BadRequest("Pizzas file required.");
            await using var stream = await GetDecompressedStream(request.File);
            await _csvService.ImportPizzasCsvAsync(stream);
            return Ok("Pizzas imported successfully.");
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
