using ChallengeAPI.Data;
using ChallengeAPI.Models;
using CsvHelper;
using CsvHelper.Configuration;
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

        // -----------------------
        // Import PizzaTypes CSV
        // -----------------------
        public async Task ImportPizzaTypesCsvAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<PizzaTypeCsvMap>();

            var batch = new List<PizzaType>();

            await foreach (var record in csv.GetRecordsAsync<PizzaTypeCsv>())
            {
                if (!_context.PizzaTypes.Any(pt => pt.Id == record.PizzaTypeId))
                {
                    batch.Add(new PizzaType
                    {
                        Id = record.PizzaTypeId,
                        Name = record.Name,
                        Category = record.Category,
                        Ingredients = record.Ingredients
                    });

                    if (batch.Count >= BatchSize)
                    {
                        await _context.AddRangeAsync(batch);
                        await _context.SaveChangesAsync();
                        batch.Clear();
                    }
                }
            }

            if (batch.Any())
            {
                await _context.AddRangeAsync(batch);
                await _context.SaveChangesAsync();
            }
        }

        // -----------------------
        // Import Pizzas CSV
        // -----------------------
        public async Task ImportPizzasCsvAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<PizzaCsvMap>();

            var batch = new List<Pizza>();

            await foreach (var record in csv.GetRecordsAsync<PizzaCsv>())
            {
                if (!_context.Pizzas.Any(p => p.Id == record.PizzaId))
                {
                    batch.Add(new Pizza
                    {
                        Id = record.PizzaId,
                        PizzaTypeId = record.PizzaTypeId,
                        Size = record.Size,
                        Price = record.Price
                    });

                    if (batch.Count >= BatchSize)
                    {
                        await _context.AddRangeAsync(batch);
                        await _context.SaveChangesAsync();
                        batch.Clear();
                    }
                }
            }

            if (batch.Any())
            {
                await _context.AddRangeAsync(batch);
                await _context.SaveChangesAsync();
            }
        }

        // -----------------------
        // Import Orders CSV
        // -----------------------
        public async Task ImportOrdersCsvAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<OrderCsvMap>();

            var batch = new List<Order>();

            await foreach (var record in csv.GetRecordsAsync<OrderCsv>())
            {
                if (!_context.Orders.Any(o => o.Id == record.OrderId))
                {
                    batch.Add(new Order
                    {
                        Id = record.OrderId,
                        OrderDate = DateTime.SpecifyKind(record.Date.Date + record.Time, DateTimeKind.Utc)
                    });

                    if (batch.Count >= BatchSize)
                    {
                        await _context.AddRangeAsync(batch);
                        await _context.SaveChangesAsync();
                        batch.Clear();
                    }
                }
            }

            if (batch.Any())
            {
                await _context.AddRangeAsync(batch);
                await _context.SaveChangesAsync();
            }
        }

        // -----------------------
        // Import OrderDetails CSV
        // -----------------------
        public async Task ImportOrderDetailsCsvAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<OrderDetailCsvMap>();

            var batch = new List<OrderDetail>();
            var skippedRows = new List<OrderDetailCsv>();

            await foreach (var record in csv.GetRecordsAsync<OrderDetailCsv>())
            {
                var pizzaExists = _context.Pizzas.Any(p => p.Id == record.PizzaId);
                var orderExists = _context.Orders.Any(o => o.Id == record.OrderId);

                if (!_context.OrderDetails.Any(od => od.Id == record.OrderDetailsId) && pizzaExists && orderExists)
                {
                    batch.Add(new OrderDetail
                    {
                        Id = record.OrderDetailsId,
                        OrderId = record.OrderId,
                        PizzaId = record.PizzaId,
                        Quantity = record.Quantity
                    });

                    if (batch.Count >= BatchSize)
                    {
                        await _context.AddRangeAsync(batch);
                        await _context.SaveChangesAsync();
                        batch.Clear();
                    }
                }
                else
                {
                    skippedRows.Add(record);
                }
            }

            if (batch.Any())
            {
                await _context.AddRangeAsync(batch);
                await _context.SaveChangesAsync();
            }
        }

        // -----------------------
        // CSV helper classes
        // -----------------------
        public class PizzaTypeCsv { public string PizzaTypeId; public string Name; public string Category; public string Ingredients; }
        public class PizzaCsv { public string PizzaId; public string PizzaTypeId; public string Size; public decimal Price; }
        public class OrderCsv { public int OrderId; public DateTime Date; public TimeSpan Time; }
        public class OrderDetailCsv { public int OrderDetailsId; public int OrderId; public string PizzaId; public int Quantity; }

        // -----------------------
        // CSV ClassMaps
        // -----------------------
        public sealed class PizzaTypeCsvMap : ClassMap<PizzaTypeCsv>
        {
            public PizzaTypeCsvMap() { Map(m => m.PizzaTypeId).Name("pizza_type_id"); Map(m => m.Name).Name("name"); Map(m => m.Category).Name("category"); Map(m => m.Ingredients).Name("ingredients"); }
        }
        public sealed class PizzaCsvMap : ClassMap<PizzaCsv>
        {
            public PizzaCsvMap() { Map(m => m.PizzaId).Name("pizza_id"); Map(m => m.PizzaTypeId).Name("pizza_type_id"); Map(m => m.Size).Name("size"); Map(m => m.Price).Name("price"); }
        }
        public sealed class OrderCsvMap : ClassMap<OrderCsv> { public OrderCsvMap() { Map(m => m.OrderId).Name("order_id"); Map(m => m.Date).Name("date"); Map(m => m.Time).Name("time"); } }
        public sealed class OrderDetailCsvMap : ClassMap<OrderDetailCsv> { public OrderDetailCsvMap() { Map(m => m.OrderDetailsId).Name("order_details_id"); Map(m => m.OrderId).Name("order_id"); Map(m => m.PizzaId).Name("pizza_id"); Map(m => m.Quantity).Name("quantity"); } }

        // -----------------------
        // Optional: Compress Stream for Large Files
        // -----------------------
        public Stream CompressCsv(Stream inputStream)
        {
            var outputStream = new MemoryStream();
            using (var gzip = new GZipStream(outputStream, CompressionMode.Compress, true)) inputStream.CopyTo(gzip);
            outputStream.Position = 0;
            return outputStream;
        }
    }
}
