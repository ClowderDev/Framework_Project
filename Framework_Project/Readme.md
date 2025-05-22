# Framework_Project

## Giới thiệu

Đây là project website bán hàng sử dụng ASP.NET Core MVC, hỗ trợ quản lý đơn hàng, sản phẩm, người dùng, thanh toán qua MoMo, VNPay, COD, v.v.

## Yêu cầu hệ thống

- .NET 8.0 SDK trở lên
- SQL Server (hoặc SQL Express)
- Visual Studio 2022 (khuyến nghị) hoặc VS Code
- Git

## Cài đặt

### 1. Clone repository

```bash
git clone https://github.com/your-username/Framework_Project.git
cd Framework_Project/Framework_Project
```

### 2. Cấu hình Database

- Tạo một database mới trên SQL Server.
- Mở file `appsettings.json` và cập nhật chuỗi kết nối tại mục `ConnectionStrings` cho phù hợp với database của bạn.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

### 3. Chạy migration (nếu có)

```bash
dotnet ef database update
```

> Nếu chưa cài đặt EF Core Tools, chạy:  
> `dotnet tool install --global dotnet-ef`

### 4. Cài đặt các package phụ thuộc

Thông thường, các package sẽ được tự động restore khi build project:

```bash
dotnet restore
```

## Chạy project

```bash
dotnet run
```

## Nếu gặp lỗi khi thanh toán Momo chạy đoạn code sau:

```bash
dotnet dev-certs https --trust
```
