<div align="center">

# 🖥️ DigiPOSE

### Digital Point of Sale Enterprise

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=nuget&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**Centralized ERP/PoS Resource Management & Point of Sale System for multi-branch retail chains.**

[English](README.md) · [Tiếng Việt](README_VI.md)

</div>

---

## 📋 About

> DigiPOSE (Digital Point of Sale Enterprise) is a B2B ERP/PoS management system built on ASP.NET Core MVC. It handles end-to-end operations for multi-branch retail chains: inventory management, shift & counter control, point-of-sale transactions, customer CRM, and real-time revenue reporting.

## 🏗️ System Architecture

```
┌───────────────────────────────────────────────────────────┐
│                     DigiPOSE System                       │
├────────────────────────────┬──────────────────────────────┤
│     Admin CMS (SSR)        │     PoS Frontend (API)       │
│     ASP.NET Core MVC       │     RESTful Web API (JSON)    │
│     Razor Views            │     JWT Bearer Auth           │
│     Cookie Auth            │     React.js / Next.js Ready  │
├────────────────────────────┴──────────────────────────────┤
│                Entity Framework Core 10                    │
│                SQL Server (3NF — 26 Tables)                │
└───────────────────────────────────────────────────────────┘
```

## ⚙️ Tech Stack

<div align="center">

| Layer | Technology |
|:---:|:---:|
| **Runtime** | .NET 10.0 |
| **Framework** | ASP.NET Core MVC (Razor Views) |
| **ORM** | Entity Framework Core 10 |
| **Database** | Microsoft SQL Server |
| **Auth** | Cookie Authentication + JWT Bearer |
| **Password** | BCrypt Hashing |
| **UI** | Cyber-Cinematic HUD (Custom CSS + Font Awesome) |

</div>

## 📂 Project Structure

```
digipose/
├── source/
│   └── DigiPOSE/
│       ├── Controllers/        # MVC Controllers (27 modules)
│       ├── Models/             # Entity Models & DbContext (26 tables)
│       ├── Helpers/            # Utility classes (SlugHelper, ...)
│       ├── Migrations/         # EF Core Database Migrations
│       ├── Views/              # Razor Views (CRUD per entity)
│       ├── wwwroot/            # Static assets (CSS, JS, images)
│       ├── Program.cs          # Application entry point
│       └── appsettings.json    # Configuration & connection strings
├── docs/                       # Technical documentation (15 phases)
├── asset/                      # Project assets
├── .gitignore
├── README.md                   # English documentation
└── README_VI.md                # Vietnamese documentation
```

## 🧩 Key Features

### 1. Product & Inventory Management
- Full CRUD for products, categories, units, manufacturers
- Multi-branch inventory tracking (`ProductInventories`)
- Import/Export/Return via stock vouchers (`StockVouchers`)
- Barcode SKU scanning with O(1) lookup speed

### 2. Shift & Counter Management
- Physical PoS counter identification per branch
- Open/Close shift with cash flow control (`StartCash` / `EndCash`)
- Automatic end-of-shift revenue reconciliation

### 3. Point of Sale Operations
- Draft Order creation — no Session dependency
- ACID-compliant checkout via DB Transaction
- CRM reward points accumulation for customers
- Electronic invoice delivery via Email (MailKit SMTP)

### 4. Partner Management
- Customer management (VIP, corporate classification)
- Supplier management with debt tracking (`DebtBalance`)

### 5. Reporting & Analytics
- Revenue by branch / employee / time period
- Low stock alerts (`StockQuantity ≤ MinStockLevel`)
- Multi-dimensional stock voucher auditing

## 🔐 Security

- **Passwords**: BCrypt hashing (no plaintext storage)
- **Admin Auth**: Cookie Authentication (8-hour sliding session)
- **PoS Auth**: JWT Bearer Token (stateless)
- **Concurrency**: Optimistic Concurrency via `[Timestamp] RowVersion`
- **Cascade Delete**: Disabled globally (`DeleteBehavior.Restrict`)
- **ACID Transaction**: Checkout wrapped in `IDbContextTransaction`

## 🚀 Getting Started

### Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) or SQL Server LocalDB
- [Git](https://git-scm.com/)

### Step 1: Clone the repository

```bash
git clone <repository-url>
cd digipose
```

### Step 2: Configure Connection String

Edit `source/DigiPOSE/appsettings.json` with your SQL Server connection:

```json
{
  "ConnectionStrings": {
    "DigiPoseConnection": "Server=.;Database=DigiPOSE_Db;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  }
}
```

### Step 3: Initialize Database

```bash
cd source/DigiPOSE
dotnet ef database update
```

### Step 4: Run the application

```bash
dotnet watch run
```

Access: **https://localhost:5001** or **http://localhost:5000**

### Useful Commands

```bash
# Build project
dotnet build

# Create new migration
dotnet ef migrations add <MigrationName>

# Restore packages
dotnet restore

# Publish for production
dotnet publish -c Release -o ./publish
```

## 📊 Database Schema (26 Tables — 3NF)

<div align="center">

| Domain | Tables |
|:---:|:---|
| **IAM & Org** | Branches · Roles · Users · Counters · Shifts · ShiftStatuses |
| **Partners** | CustomerTypes · Customers · Suppliers |
| **Catalog & Inventory** | Categories · Units · Manufacturers · TaxTypes · ProductTypes · ItemNatures · Products · ProductInventories · StockVouchers · StockVoucherDetails |
| **Sales Core** | OrderStatuses · PaymentMethods · Orders · OrderDetails |
| **Invoicing** | Invoices · InvoiceStatuses · InvoiceTypes |

</div>

## 📝 Documentation

Detailed development phases are available in the [`docs/`](docs/) directory:

<div align="center">

| File | Content |
|:---:|:---:|
| `master-docs.md` | Master architecture blueprint |
| `pharse01.md` → `pharse15.md` | Step-by-step development phases |

</div>

---

<div align="center">

**DigiPOSE** — Built with ASP.NET Core MVC, React.js, Next.js

*Huỳnh Quốc Huy*
huykyunh.k@gmail.com

</div>
