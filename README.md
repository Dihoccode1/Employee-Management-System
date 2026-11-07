# Ứng dụng quản lý nhân sự

Ứng dụng C# WinForms + SQL Server theo kiến trúc 3 lớp **GUI → BUS → DAO**. DTO là thư viện truyền dữ liệu dùng chung. Đã có chức năng tải nhân viên mẫu và module **ProjectManagement** với bốn màn hình Dự án, Chi tiết dự án, Công việc, Phân công: thêm, sửa, xóa, tìm kiếm. Các module chấm công, tính lương, thống kê và nhập/xuất file vẫn là khung.

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

Các phần hỗ trợ như `Lookups`, `Statistics`, `Settings`, nhập/xuất file và kết nối database được dùng chung. Thư mục chưa triển khai có `.gitkeep` để giữ trong Git. ProjectManagement đã có code và script SQL riêng; các nhóm còn lại giữ phạm vi hiện có.

## Sơ đồ cơ sở dữ liệu (ERD)

[![Sơ đồ ERD quản lý nhân sự](docs/diagrams/ERD_quan_ly_nhan_su.png)](docs/diagrams/ERD_quan_ly_nhan_su.png)

Nhấn vào hình để xem ảnh lớn.

[ERD quản lý nhân sự — file draw.io](docs/diagrams/ERD_quan_ly_nhan_su_chu_lon.drawio)

Tải file và mở bằng draw.io/diagrams.net để xem hoặc chỉnh sửa. Sơ đồ gốc được giữ làm tài liệu tham khảo. ProjectManagement triển khai theo các điều chỉnh đã thống nhất: quan hệ một–nhiều từ Dự án đến Phân công, bỏ khóa ngoại ngược và bỏ điểm đánh giá đóng góp. Xem [hướng dẫn ProjectManagement](docs/project-management.md).

## Chạy ứng dụng

1. Cài .NET 10 SDK, SQL Server và IDE hỗ trợ .NET 10/WinForms.
2. Dùng SQL Server Management Studio chạy `database/01_CreateDatabase.sql`, rồi `database/02_SeedData.sql` trên database mới. Nếu đã có database thì bỏ qua hai script này. Tiếp tục chạy `database/03_ProjectManagement.sql` để tạo bốn bảng dự án; có thể chạy `database/04_ProjectManagementSeed.sql` nếu muốn thêm dữ liệu minh họa. Script 03 và 04 có thể chạy lại trên schema được tạo bởi chính các script này.
3. Mở PowerShell tại thư mục solution:

```powershell
$env:HRM_CONNECTION_STRING = 'Server=localhost;Database=HumanResourceManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
dotnet restore HumanResourceManagement.sln
dotnet build HumanResourceManagement.sln --no-restore
dotnet run --project src/GUI
```

Thay localhost bằng tên SQL Server thực tế, ví dụ .\SQLEXPRESS. TrustServerCertificate=True chỉ dành cho phát triển cục bộ; khi triển khai dùng chứng chỉ hợp lệ. Không lưu mật khẩu vào Git.

Hoặc mở solution trong Visual Studio, đặt GUI làm Startup Project. Cần thiết lập biến môi trường trước khi mở IDE. Bấm **Tải danh sách nhân viên** để kiểm tra kết nối. Bốn nút **Dự án / Chi tiết dự án / Công việc / Phân công** mở các màn hình quản lý tương ứng.

Để thêm dữ liệu, đi theo thứ tự Dự án → Chi tiết dự án → Công việc → Phân công. Chọn một dòng để sửa, bấm **Thêm mới** để nhập bản ghi mới, rồi **Lưu**. Chỉ xóa được khi không còn dữ liệu con. Trạng thái nhập bằng chữ; tiến độ, thưởng và phụ cấp đều nhập tay. Với ngày hoàn thành chưa có, bỏ dấu chọn ở ô ngày.

Kiểm thử BUS, không cần SQL Server:

```powershell
dotnet run --project tests/ProjectManagement.Tests
```

Hướng dẫn đọc code và kiểm thử SQL nằm trong [docs/project-management.md](docs/project-management.md).

## Quy ước

- GUI gọi BUS, không viết SQL trong Form.
- BUS kiểm tra dữ liệu và áp dụng quy tắc nghiệp vụ.
- DAO truy cập SQL Server; dùng tham số SQL khi thêm dữ liệu đầu vào.
- DTO chỉ chứa dữ liệu, không phụ thuộc UI hay database.
- Thêm chức năng theo thứ tự DTO → DAO → BUS → GUI.
- Theo yêu cầu môn học, tìm kiếm trên `ArrayList` sẽ nằm trong BUS; biến `static` phục vụ chọn dữ liệu giữa các Form sẽ nằm trong GUI.
- Xem [kiến trúc](docs/architecture.md) và [đối chiếu yêu cầu đồ án](docs/project-requirements.md) trước khi triển khai.
