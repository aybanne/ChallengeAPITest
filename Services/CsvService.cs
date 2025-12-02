using ChallengeAPI.Data;
using ChallengeAPI.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO.Compression;

namespace ChallengeAPI.Services
{
    public class CsvService
    {
        private readonly AppDbContext _context;
        private const int BatchSize = 1000;

        public CsvService(AppDbContext context)
        {
            _context = context;
        }

        // ========================
        // Import Result DTO
        // ========================
        public class ImportResult
        {
            public int Total { get; set; }
            public int Imported { get; set; }
            public int Skipped { get; set; }
            public List<string> SkippedIds { get; set; } = new();
        }

        // ========================
        // Import PizzaTypes CSV
        // ========================
        public async Task<ImportResult> ImportPizzaTypesCsvAsync(Stream csvStream)
        {
            var result = new ImportResult();
            var batch = new List<PizzaType>();

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<PizzaTypeCsvMap>();

            var existingIds = await _context.PizzaTypes.Select(pt => pt.Id).ToListAsync();

            await foreach (var record in csv.GetRecordsAsync<PizzaTypeCsv>())
            {
                result.Total++;
                if (!existingIds.Contains(record.PizzaTypeId))
                {
                    batch.Add(new PizzaType
                    {
                        Id = record.PizzaTypeId,
                        Name = record.Name,
                        Category = record.Category,
                        Ingredients = record.Ingredients
                    });

                    if (batch.Count >= BatchSize)
                        await SaveBatchAsync(batch, result);
                }
                else
                {
                    result.Skipped++;
                    result.SkippedIds.Add(record.PizzaTypeId);
                }
            }

            if (batch.Any())
                await SaveBatchAsync(batch, result);

            return result;
        }

        // ========================
        // Import Pizzas CSV
        // ========================
        public async Task<ImportResult> ImportPizzasCsvAsync(Stream csvStream)
        {
            var result = new ImportResult();
            var batch = new List<Pizza>();

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<PizzaCsvMap>();

            var existingIds = await _context.Pizzas.Select(p => p.Id).ToListAsync();

            await foreach (var record in csv.GetRecordsAsync<PizzaCsv>())
            {
                result.Total++;
                if (!existingIds.Contains(record.PizzaId))
                {
                    batch.Add(new Pizza
                    {
                        Id = record.PizzaId,
                        PizzaTypeId = record.PizzaTypeId,
                        Size = record.Size,
                        Price = record.Price
                    });

                    if (batch.Count >= BatchSize)
                        await SaveBatchAsync(batch, result);
                }
                else
                {
                    result.Skipped++;
                    result.SkippedIds.Add(record.PizzaId);
                }
            }

            if (batch.Any())
                await SaveBatchAsync(batch, result);

            return result;
        }

        // ========================
        // Import Orders CSV
        // ========================
        public async Task<ImportResult> ImportOrdersCsvAsync(Stream csvStream)
        {
            var result = new ImportResult();
            var batch = new List<Order>();

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<OrderCsvMap>();

            var existingIds = await _context.Orders.Select(o => o.Id).ToListAsync();

            await foreach (var record in csv.GetRecordsAsync<OrderCsv>())
            {
                result.Total++;
                if (!existingIds.Contains(record.OrderId))
                {
                    batch.Add(new Order
                    {
                        Id = record.OrderId,
                        OrderDate = DateTime.SpecifyKind(record.Date.Date + record.Time, DateTimeKind.Utc)
                    });

                    if (batch.Count >= BatchSize)
                        await SaveBatchAsync(batch, result);
                }
                else
                {
                    result.Skipped++;
                    result.SkippedIds.Add(record.OrderId.ToString());
                }
            }

            if (batch.Any())
                await SaveBatchAsync(batch, result);

            return result;
        }

        // ========================
        // Import OrderDetails CSV
        // ========================
        public async Task<ImportResult> ImportOrderDetailsCsvAsync(Stream csvStream)
        {
            var result = new ImportResult();
            var batch = new List<OrderDetail>();
            var skippedRows = new List<OrderDetailCsv>();

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<OrderDetailCsvMap>();

            var existingOrderDetailIds = await _context.OrderDetails.Select(od => od.Id).ToListAsync();
            var existingPizzaIds = await _context.Pizzas.Select(p => p.Id).ToListAsync();
            var existingOrderIds = await _context.Orders.Select(o => o.Id).ToListAsync();

            await foreach (var record in csv.GetRecordsAsync<OrderDetailCsv>())
            {
                result.Total++;
                if (!existingOrderDetailIds.Contains(record.OrderDetailsId) &&
                    existingPizzaIds.Contains(record.PizzaId) &&
                    existingOrderIds.Contains(record.OrderId))
                {
                    batch.Add(new OrderDetail
                    {
                        Id = record.OrderDetailsId,
                        OrderId = record.OrderId,
                        PizzaId = record.PizzaId,
                        Quantity = record.Quantity
                    });

                    if (batch.Count >= BatchSize)
                        await SaveBatchAsync(batch, result);
                }
                else
                {
                    result.Skipped++;
                    result.SkippedIds.Add(record.OrderDetailsId.ToString());
                    skippedRows.Add(record);
                }
            }

            if (batch.Any())
                await SaveBatchAsync(batch, result);

            return result;
        }

        // ========================
        // Helper:
        // ========================
        private async Task SaveBatchAsync<T>(List<T> batch, ImportResult result) where T : class
        {
            try
            {
                await _context.AddRangeAsync(batch);
                await _context.SaveChangesAsync();
                result.Imported += batch.Count;
                batch.Clear();
            }
            catch (Exception ex)
            {
                // Log failed rows here
                Console.WriteLine($"Error saving batch: {ex.Message}");
                batch.Clear(); 
            }
        }

        // ========================
        // CSV Helper Classes
        // ========================
        public class PizzaTypeCsv { public string PizzaTypeId; public string Name; public string Category; public string Ingredients; }
        public class PizzaCsv { public string PizzaId; public string PizzaTypeId; public string Size; public decimal Price; }
        public class OrderCsv { public int OrderId; public DateTime Date; public TimeSpan Time; }
        public class OrderDetailCsv { public int OrderDetailsId; public int OrderId; public string PizzaId; public int Quantity; }

        // ========================
        // CSV ClassMaps
        // ========================
        public sealed class PizzaTypeCsvMap : ClassMap<PizzaTypeCsv>
        {
            public PizzaTypeCsvMap()
            {
                Map(m => m.PizzaTypeId).Name("pizza_type_id");
                Map(m => m.Name).Name("name");
                Map(m => m.Category).Name("category");
                Map(m => m.Ingredients).Name("ingredients");
            }
        }
        public sealed class PizzaCsvMap : ClassMap<PizzaCsv>
        {
            public PizzaCsvMap()
            {
                Map(m => m.PizzaId).Name("pizza_id");
                Map(m => m.PizzaTypeId).Name("pizza_type_id");
                Map(m => m.Size).Name("size");
                Map(m => m.Price).Name("price");
            }
        }
        public sealed class OrderCsvMap : ClassMap<OrderCsv>
        {
            public OrderCsvMap()
            {
                Map(m => m.OrderId).Name("order_id");
                Map(m => m.Date).Name("date");
                Map(m => m.Time).Name("time");
            }
        }
        public sealed class OrderDetailCsvMap : ClassMap<OrderDetailCsv>
        {
            public OrderDetailCsvMap()
            {
                Map(m => m.OrderDetailsId).Name("order_details_id");
                Map(m => m.OrderId).Name("order_id");
                Map(m => m.PizzaId).Name("pizza_id");
                Map(m => m.Quantity).Name("quantity");
            }
        }

        // ========================
        // Optional: Compress Stream for Large Files
        // ========================
        public Stream CompressCsv(Stream inputStream)
        {
            var outputStream = new MemoryStream();
            using (var gzip = new GZipStream(outputStream, CompressionMode.Compress, true))
                inputStream.CopyTo(gzip);
            outputStream.Position = 0;
            return outputStream;
        }
    }
}
