

### 1. Clone repository

```bash
git clone https://github.com/ClowderDev/Framework_Project.git
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
