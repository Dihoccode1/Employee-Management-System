# Ứng dụng quản lý nhân sự

Ứng dụng C# WinForms + SQL Server, OOP và kiến trúc 3 lớp **GUI → BUS → DAO**. DTO là thư viện truyền dữ liệu dùng chung. Đã có tải danh sách nhân viên và CRUD/tìm kiếm cho cả bốn phần ProjectManagement: Dự án, Chi tiết dự án, Công việc, Phân công. Các phần chấm công, tính lương, thống kê và nhập/xuất file vẫn là khung.

## Cấu trúc

```text
HumanResourceManagement.sln
Directory.Build.props
src/
  GUI/
    Forms/
      MainForm.cs                  # Kèm MainForm.Designer.cs
      HumanResources/              # Xanh lá: nhân sự
      ProjectManagement/           # Đỏ: dự án và công việc
      SalaryInformation/          # Xanh lam: chấm công, lương thưởng
      Lookups/
      Statistics/
      Settings/
    Selection/
    Reports/
    Controls/
    Resources/
  BUS/
    HumanResources/
    ProjectManagement/
    SalaryInformation/
    ServiceFactory.cs
    Statistics/
    Settings/
    ImportExport/
    Reporting/
  DAO/
    HumanResources/
    ProjectManagement/
    SalaryInformation/
    Infrastructure/MyConnection.cs
    Statistics/
    Settings/
  DTO/
    HumanResources/
    ProjectManagement/
    SalaryInformation/
    Statistics/
    Settings/
database/
  01_CreateDatabase.sql
  02_SeedData.sql
docs/
  diagrams/                        # Ảnh ERD và file draw.io gốc
  architecture.md
  project-requirements.md
  report/
  screenshots/
```

Ba nhóm màu có cùng cách chia module trong `GUI/Forms`, `BUS`, `DAO` và `DTO`:

| Màu trên ERD | Thư mục nhóm | Các module con |
|---|---|---|
| Xanh lá | `HumanResources` | `Departments`, `Positions`, `Employees`, `Contracts` |
| Đỏ | `ProjectManagement` | `Projects`, `ProjectDetails`, `Tasks`, `Assignments` |
| Xanh lam | `SalaryInformation` | `MonthlyAttendance`, `Attendance`, `AttendanceStatuses`, `Payroll`, `SeniorityAllowanceRules`, `EmployeeSeniorityAllowances`, `EmployeeRewards`, `RewardRules` |

Các phần hỗ trợ như `Lookups`, `Statistics`, `Settings`, nhập/xuất file và kết nối database được dùng chung. Thư mục chưa triển khai có `.gitkeep` để giữ trong Git. Riêng ProjectManagement đã có DTO, DAO, BUS, GUI và script SQL bổ sung; xem [hướng dẫn đọc code và nghiệp vụ](docs/project-management.md).

## Sơ đồ cơ sở dữ liệu (ERD)

[![Sơ đồ ERD quản lý nhân sự](docs/diagrams/ERD_quan_ly_nhan_su.png)](docs/diagrams/ERD_quan_ly_nhan_su.png)

Nhấn vào hình để xem ảnh lớn.

[ERD quản lý nhân sự — file draw.io](docs/diagrams/ERD_quan_ly_nhan_su_chu_lon.drawio)

Tải file và mở bằng draw.io/diagrams.net để xem hoặc chỉnh sửa. Đây là ERD tham khảo ban đầu. Quan hệ ProjectManagement đã được điều chỉnh theo thống nhất với người dùng để tránh khóa ngoại trỏ vòng: Dự án–Chi tiết 1–1, Chi tiết–Công việc 1–N. Xem quan hệ đang triển khai trong [tài liệu ProjectManagement](docs/project-management.md); ảnh/file draw.io cũ chưa cập nhật. Các module còn lại chưa đồng bộ đầy đủ với ERD.

## Chạy ứng dụng

1. Cài .NET 10 SDK, SQL Server và IDE hỗ trợ .NET 10/WinForms.
2. Dùng SQL Server Management Studio chạy `database/01_CreateDatabase.sql`, `02_SeedData.sql`, `03_ProjectManagement.sql` theo thứ tự. Có thể chạy thêm `04_ProjectManagementSeed.sql` để có mẫu dự án/công việc/phân công. Script 01/02 chỉ chạy một lần trên database mới; database đã có schema nền chỉ cần bổ sung 03 và tùy chọn 04. Script 03/04 có thể chạy lại trên schema này.
3. Mở PowerShell tại thư mục solution:

```powershell
$env:HRM_CONNECTION_STRING = 'Server=localhost;Database=HumanResourceManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
dotnet restore HumanResourceManagement.sln
dotnet build HumanResourceManagement.sln --no-restore
dotnet run --project src/GUI
```

Thay localhost bằng tên SQL Server thực tế, ví dụ .\SQLEXPRESS. TrustServerCertificate=True chỉ dành cho phát triển cục bộ; khi triển khai dùng chứng chỉ hợp lệ. Không lưu mật khẩu vào Git.

Hoặc mở solution trong Visual Studio, đặt GUI làm Startup Project. Cần thiết lập biến môi trường trước khi mở IDE. Bấm **Tải danh sách nhân viên** để kiểm tra kết nối; dùng bốn nút **Dự án**, **Chi tiết dự án**, **Công việc**, **Phân công** để mở form tương ứng.

ProjectManagement đã có bộ kiểm thử SQL Server và form tại `tests/ProjectManagement.SmokeTests`. Xem cách chạy và luồng xử lý đơn giản trong [hướng dẫn ProjectManagement](docs/project-management.md).

## Quy ước

- GUI gọi BUS, không viết SQL trong Form.
- BUS kiểm tra dữ liệu và áp dụng quy tắc nghiệp vụ.
- DAO truy cập SQL Server; dùng tham số SQL khi thêm dữ liệu đầu vào.
- DTO chỉ chứa dữ liệu, không phụ thuộc UI hay database.
- Thêm chức năng theo thứ tự DTO → DAO → BUS → GUI.
- Theo yêu cầu môn học, tìm kiếm trên `ArrayList` sẽ nằm trong BUS; biến `static` phục vụ chọn dữ liệu giữa các Form sẽ nằm trong GUI.
- Xem [kiến trúc](docs/architecture.md) và [đối chiếu yêu cầu đồ án](docs/project-requirements.md) trước khi triển khai.
