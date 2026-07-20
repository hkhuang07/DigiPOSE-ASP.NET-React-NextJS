<div align="center">

# 🖥️ DigiPOSE

### Digital Point of Sale Enterprise

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=nuget&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**Hệ thống Quản trị Nguồn lực & Điểm Bán hàng Tập trung (ERP/PoS) cho chuỗi bán lẻ đa chi nhánh.**

[English](README.md) · [Tiếng Việt](README_VI.md)

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
│     Cookie Auth            │     React.js / Next.js Ready  │
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
| **ORM** | Entity Framework Core 10 |
| **Database** | Microsoft SQL Server |
| **Auth** | Cookie Authentication + JWT Bearer |
| **Password** | BCrypt Hashing |
| **UI** | Cyber-Cinematic HUD (Custom CSS + Font Awesome) |

</div>

## 📂 Cấu trúc thư mục

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

## 🧩 Chức năng chính

### 1. Quản lý Hàng hóa & Tồn kho
- CRUD sản phẩm, danh mục, đơn vị tính, nhà sản xuất
- Quản lý tồn kho đa chi nhánh (`ProductInventories`)
- Nhập/Xuất/Trả hàng qua chứng từ kho (`StockVouchers`)
- Quét mã vạch SKU với tốc độ O(1) lookup

### 2. Quản lý Ca làm việc & Quầy thu ngân
- Định danh quầy PoS vật lý theo chi nhánh
- Mở/Đóng ca với kiểm soát dòng tiền (`StartCash` / `EndCash`)
- Kết toán doanh thu cuối ca tự động

### 3. Bán hàng tại quầy (Point of Sale)
- Tạo đơn nháp (Draft Order) — không dùng Session
- Checkout với DB Transaction nguyên khối (ACID)
- Tích lũy điểm thưởng CRM cho khách hàng
- Phát hành hóa đơn điện tử qua Email (MailKit SMTP)

### 4. Quản trị Đối tác
- Quản lý khách hàng (phân loại VIP, doanh nghiệp)
- Quản lý nhà cung cấp & công nợ (`DebtBalance`)

### 5. Báo cáo & Thống kê
- Doanh thu theo chi nhánh / nhân viên / thời gian
- Cảnh báo tồn kho thấp (`StockQuantity ≤ MinStockLevel`)
- Tra soát chứng từ kho đa chiều

## 🔐 Bảo mật

- **Mật khẩu**: BCrypt hashing (không lưu plaintext)
- **Admin Auth**: Cookie Authentication (phiên trượt 8 tiếng)
- **PoS Auth**: JWT Bearer Token (stateless)
- **Concurrency**: Optimistic Concurrency via `[Timestamp] RowVersion`
- **Cascade Delete**: Vô hiệu hóa toàn bộ (`DeleteBehavior.Restrict`)
- **ACID Transaction**: Checkout được gói trong `IDbContextTransaction`

## 🚀 Hướng dẫn cài đặt & chạy

### Yêu cầu hệ thống

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) hoặc SQL Server LocalDB
- [Git](https://git-scm.com/)

### Bước 1: Clone repository

```bash
git clone <repository-url>
cd digipose
```

### Bước 2: Cấu hình Connection String

Mở file `source/DigiPOSE/appsettings.json` và cập nhật connection string phù hợp:

```json
{
  "ConnectionStrings": {
    "DigiPoseConnection": "Server=.;Database=DigiPOSE_Db;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  }
}
```

### Bước 3: Khởi tạo Database

```bash
cd source/DigiPOSE
dotnet ef database update
```

### Bước 4: Chạy ứng dụng

```bash
dotnet watch run
```

Truy cập: **https://localhost:5001** hoặc **http://localhost:5000**

### Các lệnh hữu ích khác

```bash
# Build project
dotnet build

# Tạo migration mới
dotnet ef migrations add <MigrationName>

# Restore packages
dotnet restore

# Publish production
dotnet publish -c Release -o ./publish
```

## 📊 Cơ sở dữ liệu (26 Tables — 3NF)

<div align="center">

| Phân hệ | Bảng dữ liệu |
|:---:|:---|
| **IAM & Org** | Branches · Roles · Users · Counters · Shifts · ShiftStatuses |
| **Partners** | CustomerTypes · Customers · Suppliers |
| **Catalog & Inventory** | Categories · Units · Manufacturers · TaxTypes · ProductTypes · ItemNatures · Products · ProductInventories · StockVouchers · StockVoucherDetails |
| **Sales Core** | OrderStatuses · PaymentMethods · Orders · OrderDetails |
| **Invoicing** | Invoices · InvoiceStatuses · InvoiceTypes |

</div>

## 📝 Tài liệu kỹ thuật

Chi tiết các giai đoạn phát triển được lưu tại thư mục [`docs/`](docs/):

<div align="center">

| File | Nội dung |
|:---:|:---:|
| `master-docs.md` | Blueprint kiến trúc tổng thể |
| `pharse01.md` → `pharse15.md` | Chi tiết từng giai đoạn phát triển |

</div>

---

<div align="center">

**DigiPOSE** — Built with ASP.NET Core MVC, React.js, Next.js

*Huỳnh Quốc Huy*
huykyunh.k@gmail.com

</div>
