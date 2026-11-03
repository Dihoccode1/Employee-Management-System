# Ứng dụng quản lý nhân sự

Bộ khung C# WinForms + SQL Server, OOP và kiến trúc 3 lớp **GUI → BUS → DAO**. DTO là thư viện truyền dữ liệu dùng chung. Hiện có luồng tải danh sách nhân viên mẫu; các thư mục mở rộng mới chỉ là khung, chưa triển khai CRUD, tìm kiếm, chấm công, tính lương, thống kê hay nhập/xuất file.

## Cấu trúc

```text
HumanResourceManagement.sln
Directory.Build.props
src/
  GUI/                     # Giao diện WinForms
    Forms/
      MainForm.cs          # Kèm MainForm.Designer.cs
      Employees/
      Departments/
      Positions/
      Contracts/
      Attendance/
      Payroll/
      Lookups/             # Form tra cứu/chọn dữ liệu bằng nút ...
      Statistics/          # Giao diện thống kê
      Settings/            # Giao diện tham số
    Selection/             # Dự kiến giữ kết quả chọn qua biến static
    Reports/               # Mẫu/bố cục báo cáo
    Controls/
    Resources/
  BUS/                     # Nghiệp vụ, kiểm tra dữ liệu, tìm kiếm
    ServiceFactory.cs
    Employees/             # EmployeeService.cs
    Departments/, Positions/, Contracts/, Attendance/, Payroll/
    Statistics/
    Settings/
    ImportExport/          # Dự kiến xử lý nhập/xuất .xls
    Reporting/             # Dự kiến xử lý báo cáo PDF
  DAO/                     # Kết nối, repository, truy vấn SQL Server
    Infrastructure/
      MyConnection.cs
    Employees/             # IEmployeeRepository + EmployeeRepository
    Departments/, Positions/, Contracts/, Attendance/, Payroll/
    Statistics/
    Settings/
  DTO/                     # Đối tượng truyền dữ liệu
    Employees/             # EmployeeDto.cs
    Departments/, Positions/, Contracts/, Attendance/, Payroll/
    Statistics/
    Settings/
database/
  01_CreateDatabase.sql
  02_SeedData.sql
docs/
  architecture.md
  project-requirements.md  # Đối chiếu yêu cầu đồ án và việc cần làm
  report/                  # Báo cáo cuối kỳ DOC/PDF
  screenshots/             # Ảnh CSDL, giao diện, cây thư mục
```

Các tên thư mục viết cùng một dòng trong cây trên là những thư mục riêng. Thư mục chưa triển khai có `.gitkeep` để giữ trong Git. Chưa có bảng tham số trong SQL và chưa có thư viện nhập/xuất Excel hoặc PDF.

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
- DAO truy cập SQL Server; dùng tham số SQL khi thêm dữ liệu đầu vào.
- DTO chỉ chứa dữ liệu, không phụ thuộc UI hay database.
- Thêm chức năng theo thứ tự DTO → DAO → BUS → GUI.
- Theo yêu cầu môn học, tìm kiếm trên `ArrayList` sẽ nằm trong BUS; biến `static` phục vụ chọn dữ liệu giữa các Form sẽ nằm trong GUI.
- Xem [kiến trúc](docs/architecture.md) và [đối chiếu yêu cầu đồ án](docs/project-requirements.md) trước khi triển khai.
