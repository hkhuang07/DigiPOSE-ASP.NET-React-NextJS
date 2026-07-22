<div align="center">

# 🖥️ DigiPOSE

### Digital Point of Sale Enterprise

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=nuget&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![DataTables](https://img.shields.io/badge/DataTables-Server--Side-00E5FF?logo=datatables&logoColor=black)](https://datatables.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**Centralized ERP/PoS Resource Management & Point of Sale System for multi-branch retail chains.**

[English](README.md) · [Tiếng Việt](README_VI.md) · [GitHub Repository](https://github.com/hkhuang07/DigiPOSE-ASP.NET-React-NextJS)

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
│     DataTables Server-Side │     React.js / Next.js Ready  │
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
| **ORM & Linq** | Entity Framework Core 10, System.Linq.Dynamic.Core |
| **Database** | Microsoft SQL Server (26 Normalized 3NF Tables) |
| **Data Grid** | DataTables 1.13.8 (100% Server-Side Pagination & Dynamic Filtering across 26 Controllers) |
| **Auth** | Cookie Authentication + JWT Bearer |
| **UI/UX Vibe** | Cyber-Cinematic Military HUD (Pure Black `#000000`, Cyan Glow `#00E5FF`, Glassmorphism) |

</div>

## 📂 Project Structure

```
digipose/
├── source/
│   └── DigiPOSE/
│       ├── Controllers/        # MVC Controllers (26 DataTables Server-Side modules)
│       ├── Models/             # Entity Models & DbContext (26 tables)
│       ├── Helpers/            # Utility classes (SlugHelper, ...)
│       ├── Migrations/         # EF Core Database Migrations
│       ├── Views/              # Cyber HUD Razor Views (Modal CRUD per entity)
│       ├── wwwroot/            # Static assets & Localized LibMan Libraries (DataTables, FontAwesome, CKEditor)
│       ├── libman.json         # Client-side library configuration
│       ├── Program.cs          # Application entry point
│       └── appsettings.json    # Configuration & connection strings
├── docs/                       # Technical documentation (15 phases)
├── asset/                      # Project assets & screenshots
├── .gitignore
├── README.md                   # English documentation
└── README_VI.md                # Vietnamese documentation
```

## 🧩 Key Features & Enterprise Implementation

### 1. DataTables Server-Side Architecture (26/26 Modules)
- **Zero Client Overload**: Complete Server-Side pagination, sorting, and global multi-field filtering implemented across all 26 controllers via `System.Linq.Dynamic.Core`.
- **Global Modal CRUD (`#globalModal`)**: Dynamic partial view loading for `Create`, `Edit`, `Details`, and `Delete` actions without full page reloads.

### 2. Product & Inventory Management
- Full CRUD for products, categories, units, manufacturers, product types, and item natures.
- Multi-branch inventory tracking (`ProductInventories`).
- Import/Export/Return via stock vouchers (`StockVouchers` & `StockVoucherDetails`).
- Barcode SKU scanning with $O(1)$ lookup speed.

### 3. Shift & Counter Management
- Physical PoS counter identification per branch (`Counters`).
- Open/Close shift with cash flow control (`StartCash` / `EndCash`).
- Automatic end-of-shift revenue reconciliation (`Shifts` & `ShiftStatuses`).

### 4. Point of Sale Operations & Financial Invoicing
- Draft Order creation — no Session dependency.
- ACID-compliant checkout via DB Transactions.
- Invoicing workflow (`Invoices`, `InvoiceTypes`, `InvoiceStatuses`).

## 🚀 Getting Started & CLI Commands

### Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) or SQL Server LocalDB
- [Git](https://git-scm.com/)
- [Microsoft Library Manager CLI (LibMan)](https://learn.microsoft.com/en-us/aspnet/core/client-side/libman/)

### Step 1: Clone the repository

```bash
git clone https://github.com/hkhuang07/DigiPOSE-ASP.NET-React-NextJS.git
cd DigiPOSE-ASP.NET-React-NextJS
```

### Step 2: Install NuGet & LibMan Packages via Terminal

Navigate to the source directory:
```bash
cd source/DigiPOSE
```

Restore Backend Dependencies (NuGet):
```bash
dotnet add package System.Linq.Dynamic.Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet restore
```

Install & Restore Client-Side Libraries via LibMan CLI:
```bash
# Install LibMan tool globally (if not already installed)
dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Install DataTables Core & Bootstrap 5 Local Packages
libman install datatables.net@1.13.8 --provider cdnjs --destination wwwroot/lib/datatables --files jquery.dataTables.min.js
libman install datatables.net-bs5@1.13.8 --provider cdnjs --destination wwwroot/lib/datatables --files dataTables.bootstrap5.min.css --files dataTables.bootstrap5.min.js

# Install DataTables Buttons & Export Plugins
libman install datatables.net-buttons@2.4.2 --provider cdnjs --destination wwwroot/lib/datatables --files js/dataTables.buttons.min.js --files js/buttons.html5.min.js
libman install datatables.net-buttons-bs5@2.4.2 --provider cdnjs --destination wwwroot/lib/datatables --files buttons.bootstrap5.min.css --files buttons.bootstrap5.min.js
libman install jszip@3.10.1 --provider cdnjs --destination wwwroot/lib/datatables --files jszip.min.js

# Restore all libraries defined in libman.json
libman restore
```

### Step 3: Configure Connection String

Edit `source/DigiPOSE/appsettings.json` with your SQL Server connection:

```json
{
  "ConnectionStrings": {
    "DigiPoseConnection": "Server=.;Database=DigiPOSE_Db;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  }
}
```

### Step 4: Initialize Database & Run

```bash
# Update Database Schema
dotnet ef database update

# Run application with Hot Reload
dotnet watch run
```

Access application at: **https://localhost:5001** or **http://localhost:5000**

---

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

---

<div align="center">

**DigiPOSE** — Built with ASP.NET Core MVC, React.js, Next.js  
GitHub Repository: [hkhuang07/DigiPOSE-ASP.NET-React-NextJS](https://github.com/hkhuang07/DigiPOSE-ASP.NET-React-NextJS)

*Huỳnh Quốc Huy* — huykyunh.k@gmail.com

</div>
