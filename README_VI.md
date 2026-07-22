<div align="center">

# 🖥️ DigiPOSE

### Digital Point of Sale Enterprise

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=nuget&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![DataTables](https://img.shields.io/badge/DataTables-Server--Side-00E5FF?logo=datatables&logoColor=black)](https://datatables.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**Hệ thống Quản trị Nguồn lực & Điểm Bán hàng Tập trung (ERP/PoS) cho chuỗi bán lẻ đa chi nhánh.**

[English](README.md) · [Tiếng Việt](README_VI.md) · [GitHub Repository](https://github.com/hkhuang07/DigiPOSE-ASP.NET-React-NextJS)

</div>

---

## 📋 Mô tả dự án

> DigiPOSE (Digital Point of Sale Enterprise) là phần mềm quản trị bán hàng ERP/PoS (B2B) xây dựng trên ASP.NET Core MVC. Hệ thống xử lý toàn bộ vận hành chuỗi bán lẻ đa chi nhánh: quản lý kho, ca làm việc, giao dịch tại quầy, CRM khách hàng và báo cáo doanh thu thời gian thực.

## 🏗️ Kiến trúc hệ thống

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

## ⚙️ Công nghệ sử dụng

<div align="center">

| Layer | Công nghệ |
|:---:|:---:|
| **Runtime** | .NET 10.0 |
| **Framework** | ASP.NET Core MVC (Razor Views) |
| **ORM & Linq** | Entity Framework Core 10, System.Linq.Dynamic.Core |
| **Database** | Microsoft SQL Server (26 Bảng chuẩn hóa 3NF) |
| **Data Grid** | DataTables 1.13.8 (100% Server-Side Pagination & Dynamic Filtering trên toàn bộ 26 Controllers) |
| **Auth** | Cookie Authentication + JWT Bearer |
| **Giao diện (UI)** | Cyber-Cinematic Military HUD (Chủ đạo Pure Black `#000000`, Hologram Cyan `#00E5FF`, Glassmorphism) |

</div>

## 📂 Cấu trúc thư mục

```
digipose/
├── source/
│   └── DigiPOSE/
│       ├── Controllers/        # MVC Controllers (26 module DataTables Server-Side)
│       ├── Models/             # Entity Models & DbContext (26 tables)
│       ├── Helpers/            # Utility classes (SlugHelper, ...)
│       ├── Migrations/         # EF Core Database Migrations
│       ├── Views/              # Cyber HUD Razor Views (Modal CRUD per entity)
│       ├── wwwroot/            # Static assets & Các gói LibMan cục bộ (DataTables, FontAwesome, CKEditor)
│       ├── libman.json         # File cấu hình quản lý thư viện client-side
│       ├── Program.cs          # Application entry point
│       └── appsettings.json    # Configuration & connection strings
├── docs/                       # Technical documentation (15 phases)
├── asset/                      # Project assets & screenshots
├── .gitignore
├── README.md                   # English documentation
└── README_VI.md                # Vietnamese documentation
```

## 🧩 Chức năng chính & Chuẩn Kiến trúc Doanh nghiệp

### 1. Phân trang Server-Side DataTables Đồng bộ (26/26 Modules)
- **Tối ưu hóa hiệu năng**: Phân trang, sắp xếp và tìm kiếm động toàn bộ dữ liệu trực tiếp trên SQL Server thông qua `System.Linq.Dynamic.Core` giúp giảm 100% tải phía Client.
- **Hệ thống Modal CRUD dùng chung (`#globalModal`)**: Gọi Partial View động cho các thao tác `Create`, `Edit`, `Details`, `Delete` không cần reload trang.

### 2. Quản lý Hàng hóa & Tồn kho
- CRUD hàng hóa, danh mục, đơn vị tính, nhà sản xuất, loại sản phẩm, tính chất mặt hàng.
- Quản lý tồn kho đa chi nhánh (`ProductInventories`).
- Nhập/Xuất/Trả hàng qua chứng từ kho (`StockVouchers` & `StockVoucherDetails`).
- Quét mã vạch SKU với tốc độ tra cứu $O(1)$.

### 3. Quản lý Ca làm việc & Quầy thu ngân
- Định danh quầy PoS vật lý theo chi nhánh (`Counters`).
- Mở/Đóng ca với kiểm soát dòng tiền (`StartCash` / `EndCash`).
- Kết toán doanh thu cuối ca tự động (`Shifts` & `ShiftStatuses`).

### 4. Bán hàng tại quầy & Hóa đơn Tài chính
- Tạo đơn nháp (Draft Order) không phụ thuộc Session.
- Checkout với DB Transaction nguyên khối đảm bảo tính toàn vẹn (ACID).
- Quản lý hóa đơn tài chính (`Invoices`, `InvoiceTypes`, `InvoiceStatuses`).

## 🚀 Hướng dẫn cài đặt & Lệnh Terminal (CLI)

### Yêu cầu hệ thống

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) hoặc SQL Server LocalDB
- [Git](https://git-scm.com/)
- [Microsoft Library Manager CLI (LibMan)](https://learn.microsoft.com/en-us/aspnet/core/client-side/libman/)

### Bước 1: Clone repository

```bash
git clone https://github.com/hkhuang07/DigiPOSE-ASP.NET-React-NextJS.git
cd DigiPOSE-ASP.NET-React-NextJS
```

### Bước 2: Cài đặt gói NuGet & LibMan bằng Terminal

Di chuyển vào thư mục mã nguồn:
```bash
cd source/DigiPOSE
```

Khôi phục & cài đặt các gói Backend (NuGet):
```bash
dotnet add package System.Linq.Dynamic.Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet restore
```

Cài đặt các gói Frontend Cục bộ qua CLI LibMan:
```bash
# Cài đặt công cụ LibMan toàn cục (nếu chưa có)
dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Cài đặt DataTables Core & Bootstrap 5 Local
libman install datatables.net@1.13.8 --provider cdnjs --destination wwwroot/lib/datatables --files jquery.dataTables.min.js
libman install datatables.net-bs5@1.13.8 --provider cdnjs --destination wwwroot/lib/datatables --files dataTables.bootstrap5.min.css --files dataTables.bootstrap5.min.js

# Cài đặt DataTables Buttons & Export Plugins (Excel/PDF)
libman install datatables.net-buttons@2.4.2 --provider cdnjs --destination wwwroot/lib/datatables --files js/dataTables.buttons.min.js --files js/buttons.html5.min.js
libman install datatables.net-buttons-bs5@2.4.2 --provider cdnjs --destination wwwroot/lib/datatables --files buttons.bootstrap5.min.css --files buttons.bootstrap5.min.js
libman install jszip@3.10.1 --provider cdnjs --destination wwwroot/lib/datatables --files jszip.min.js

# Restore toàn bộ thư viện khai báo trong libman.json
libman restore
```

### Bước 3: Cấu hình Connection String

Mở file `source/DigiPOSE/appsettings.json` và cập nhật connection string phù hợp:

```json
{
  "ConnectionStrings": {
    "DigiPoseConnection": "Server=.;Database=DigiPOSE_Db;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  }
}
```

### Bước 4: Khởi tạo Database & Chạy dự án

```bash
# Cập nhật Schema Database
dotnet ef database update

# Chạy ứng dụng với Hot Reload
dotnet watch run
```

Truy cập hệ thống tại: **https://localhost:5001** hoặc **http://localhost:5000**

---

## 📊 Cơ sở dữ liệu (26 Bảng — 3NF)

<div align="center">

| Phân hệ | Bảng dữ liệu |
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
