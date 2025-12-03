# ChallengeAPI

**C# .NET 10 Web API for Pizza Orders CSV Import & Reporting**

---

## Table of Contents

1. Overview  
2. Setup & Installation  
3. Project Structure  
4. Database Schema  
5. CSV Import Instructions  
6. Metrics & Reporting  
7. API Endpoints  
8. Swagger UI  
9. Batch & Skipped Rows Behavior  
10. Notes / Repository Organization  

---

## Overview

This API handles:

- Importing CSV files for **Orders**, **OrderDetails**, **PizzaTypes**, and **Pizzas**  
- Metrics collection for rows processed, imported, skipped, and errors  
- Reporting endpoints for **sales summary**, **daily sales**, **top pizzas**, and **pizza size popularity**  

It uses **.NET 10**, **EF Core 10**, and **PostgreSQL**.  

---

## Setup & Installation

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)  
- PostgreSQL database  
- Optional: **Visual Studio 2022/2023**

### Steps

Clone repository:

```
git clone https://github.com/aybanne/ChallengeAPITest

```

Update your `appsettings.json` with PostgreSQL connection:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=challengeapi;Username=yourusername;Password=yourpassword"
  }
}
```

Apply EF Core migrations:

```
dotnet ef database update
```

Run the API:

```
dotnet run
```

Access Swagger UI at:

```
https://localhost:<port>/swagger/index.html
```
Access Optional Blazor UI at:

```
https://localhost:<port>/csv-upload
```
---

## Project Structure

```
ChallengeAPI/
├── Controllers/        # API controllers
├── Services/           # Business logic (CSV import, metrics)
├── Data/               # DbContext and EF models
├── Models/             # Entity definitions
├── wwwroot/            # Static files
└── Program.cs          # .NET 10 minimal hosting configuration
```

---

## Database Schema

| Table        | Columns                               | Relations                                |
|--------------|---------------------------------------|------------------------------------------|
| PizzaTypes   | Id, Name, Category, Ingredients       | One-to-many with Pizzas                  |
| Pizzas       | Id, PizzaTypeId, Size, Price          | Many-to-one with PizzaTypes              |
| Orders       | Id, OrderDate                         | One-to-many with OrderDetails            |
| OrderDetails | Id, OrderId, PizzaId, Quantity        | Many-to-one with Orders & Pizzas         |

---

## CSV Import Instructions

- Supported CSVs: Orders, OrderDetails, PizzaTypes, Pizzas  
- Optional GZip compression supported  
- Files can be uploaded via Swagger UI or frontend  
- Each CSV processed in batches of 1000 rows  
- Duplicate entries (by Id) automatically skipped  
- Metrics recorded per import: processed, imported, skipped, failed, time elapsed

---

## Metrics & Reporting

**Metrics summary endpoint**: returns statistics for the last import including:  
- Rows processed  
- Rows imported  
- Rows skipped  
- Errors  

**Reporting endpoints:**  
- `/api/reports/sales-summary` – Total orders, total pizzas, revenue, top pizza types  
- `/api/reports/daily-sales` – Orders and sales grouped by day  
- `/api/reports/top-pizzas` – Quantity sold and total sales per pizza  
- `/api/reports/pizza-sizes` – Popularity and sales per size  

---

## API Endpoints

| Endpoint                        | Method | Description                          |
|---------------------------------|--------|--------------------------------------|
| /api/import/orders              | POST   | Import Orders CSV                    |
| /api/import/orderdetails        | POST   | Import OrderDetails CSV              |
| /api/import/pizzatypes          | POST   | Import PizzaTypes CSV                |
| /api/import/pizzas              | POST   | Import Pizzas CSV                    |
| /api/reports/sales-summary      | GET    | Returns total sales metrics          |
| /api/reports/daily-sales        | GET    | Returns daily sales                  |
| /api/reports/top-pizzas         | GET    | Returns top selling pizzas           |
| /api/reports/pizza-sizes        | GET    | Returns pizza size popularity        |
| /api/reports/top-pizza-types    | GET    | Returns top pizza types by revenue   |

---

## Swagger UI

Available at:  
```
https://localhost:<port>/swagger/index.html
```

- Allows testing all import and report endpoints  
- Supports file uploads with multipart/form-data and optional GZip files  

---

## Batch & Skipped Rows Behavior

- Rows processed in batches of 1000 to avoid large transaction overhead  
- Duplicate Id rows skipped and logged internally  
- Rows failing import (FK/validation issues) skipped  
- Metrics endpoint shows counts of processed, imported, skipped, failed  

---

## Notes / Repository Organization

- README includes setup, CSV instructions, metrics, and reporting guidance  
- CSV import handles compressed files and duplicates  
- Each service method is fault-tolerant and logs skipped rows  
- Project is modular: controllers handle HTTP, services handle logic, DbContext manages database  
---

