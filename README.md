# Ứng dụng quản lý nhân sự

Bộ khung C# WinForms + SQL Server, OOP và kiến trúc 3 lớp. Có luồng tải danh sách nhân viên mẫu; chưa triển khai CRUD, đăng nhập, chấm công và tính lương.

## Cấu trúc

```text
HumanResourceManagement.sln
Directory.Build.props
src/
  GUI/           # Presentation: giao diện WinForms
    Forms/           # MainForm.cs + MainForm.Designer.cs
    Controls/
    Resources/
  BUS/           # Business: service, quy tắc nghiệp vụ
    ServiceFactory.cs
  DAL/           # Data Access: repository, truy vấn SQL
    Infrastructure/  # SqlConnectionFactory
  DTO/           # Đối tượng truyền dữ liệu dùng chung
database/
  01_CreateDatabase.sql
  02_SeedData.sql
docs/
  architecture.md
```

BUS, DAL và DTO có các thư mục chức năng: Employees, Departments, Positions, Contracts, Attendance, Payroll. Thư mục chưa triển khai có .gitkeep để giữ trong Git.

## Chạy ứng dụng

1. Cài .NET 10 SDK, SQL Server và IDE hỗ trợ .NET 10/WinForms.
2. Dùng SQL Server Management Studio chạy lần lượt hai script trong database. Script tạo bảng và dữ liệu mẫu chỉ chạy một lần trên database mới.
3. Mở PowerShell tại thư mục solution:

```powershell
$env:HRM_CONNECTION_STRING = 'Server=localhost;Database=HumanResourceManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
dotnet restore HumanResourceManagement.sln
dotnet build HumanResourceManagement.sln --no-restore
dotnet run --project src/GUI
```

Thay localhost bằng tên SQL Server thực tế, ví dụ .\SQLEXPRESS. TrustServerCertificate=True chỉ dành cho phát triển cục bộ; khi triển khai dùng chứng chỉ hợp lệ. Không lưu mật khẩu vào Git.

Hoặc mở solution trong Visual Studio, đặt GUI làm Startup Project. Cần thiết lập biến môi trường trước khi mở IDE. Bấm **Tải danh sách nhân viên** để kiểm tra kết nối.

## Quy ước

- GUI gọi BUS, không viết SQL trong Form.
- BUS kiểm tra dữ liệu và áp dụng quy tắc nghiệp vụ.
- DAL truy cập SQL Server; dùng tham số SQL khi thêm dữ liệu đầu vào.
- DTO chỉ chứa dữ liệu, không phụ thuộc UI hay database.
- Thêm chức năng theo thứ tự DTO → DAL → BUS → GUI.
