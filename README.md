# Ứng dụng quản lý nhân sự

Bộ khung C# WinForms + ASP.NET Core Web API + SQL Server, OOP và kiến trúc 3 lớp. Hai giao diện dùng chung nghiệp vụ: **GUI → BUS → DAO** và **Web → API → BUS → DAO**. DTO là thư viện truyền dữ liệu dùng chung. Hiện có luồng tải danh sách nhân viên mẫu trên WinForms và web; các thư mục mở rộng mới chỉ là khung, chưa triển khai CRUD, tìm kiếm, chấm công, tính lương, thống kê hay nhập/xuất file.

## Cấu trúc

```text
HumanResourceManagement.sln
Directory.Build.props
src/
  API/                             # ASP.NET Core Web API + giao diện web tĩnh
    Controllers/                   # HumanResources, ProjectManagement, SalaryInformation
    Configuration/                 # Cấu hình kết nối database
    Extensions/                    # Đăng ký service BUS qua DI
    Properties/launchSettings.json
    wwwroot/                       # HTML/CSS/JavaScript
      index.html
      css/
      js/services/                 # Gọi HTTP API
      pages/                       # Ba nhóm nghiệp vụ, thư mục cho từng module
    Program.cs
    appsettings.json
    API.http                       # Request mẫu
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
  web-api.md                       # Cấu trúc và cách phát triển phần web
  report/
  screenshots/
```

Ba nhóm màu có cùng cách chia module trong `GUI/Forms`, `BUS`, `DAO`, `DTO`, `API/Controllers` và `API/wwwroot/pages`:

| Màu trên ERD | Thư mục nhóm | Các module con |
|---|---|---|
| Xanh lá | `HumanResources` | `Departments`, `Positions`, `Employees`, `Contracts` |
| Đỏ | `ProjectManagement` | `Projects`, `ProjectDetails`, `Tasks`, `Assignments` |
| Xanh lam | `SalaryInformation` | `MonthlyAttendance`, `Attendance`, `AttendanceStatuses`, `Payroll`, `SeniorityAllowanceRules`, `EmployeeSeniorityAllowances`, `EmployeeRewards`, `RewardRules` |

Các phần hỗ trợ như `Lookups`, `Statistics`, `Settings`, nhập/xuất file và kết nối database được dùng chung. Thư mục chưa triển khai có `.gitkeep` để giữ trong Git. Việc chia nhóm chưa thay đổi schema SQL hoặc triển khai chức năng mới.

## Sơ đồ cơ sở dữ liệu (ERD)

[![Sơ đồ ERD quản lý nhân sự](docs/diagrams/ERD_quan_ly_nhan_su.png)](docs/diagrams/ERD_quan_ly_nhan_su.png)

Nhấn vào hình để xem ảnh lớn.

[ERD quản lý nhân sự — file draw.io](docs/diagrams/ERD_QLNS.drawio)

Tải file và mở bằng draw.io/diagrams.net để xem hoặc chỉnh sửa. Sơ đồ chia thành ba nhóm màu tương ứng với cấu trúc module ở trên. Các script SQL hiện vẫn là schema khởi đầu, chưa đồng bộ đầy đủ với ERD này.

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

## Chạy web và API

Sau khi tạo database và đặt `HRM_CONNECTION_STRING` như trên:

```powershell
dotnet run --project src/API
```

Mở `http://localhost:5080` để xem trang web khởi đầu. Trang này có nút tải danh sách nhân viên từ `GET /api/employees`. `GET /health` kiểm tra API đang chạy, không kiểm tra database. Có thể dùng file `src/API/API.http` để gửi request.

Web vẫn mở được khi chưa cấu hình database; API nhân viên trả HTTP 503 kèm thông báo cấu hình. Chưa có trang CRUD cho toàn bộ đối tượng. Xem [hướng dẫn web/API](docs/web-api.md) để phát triển tiếp.

## Quy ước

- GUI gọi BUS, không viết SQL trong Form.
- JavaScript gọi API qua HTTP; controller gọi BUS. API và GUI là hai phần giao diện của cùng hệ thống, không tham chiếu nhau.
- BUS kiểm tra dữ liệu và áp dụng quy tắc nghiệp vụ.
- DAO truy cập SQL Server; dùng tham số SQL khi thêm dữ liệu đầu vào.
- DTO chỉ chứa dữ liệu, không phụ thuộc UI hay database.
- Thêm chức năng theo thứ tự DTO → DAO → BUS → GUI.
- Theo yêu cầu môn học, tìm kiếm trên `ArrayList` sẽ nằm trong BUS; biến `static` phục vụ chọn dữ liệu giữa các Form sẽ nằm trong GUI.
- Xem [kiến trúc](docs/architecture.md) và [đối chiếu yêu cầu đồ án](docs/project-requirements.md) trước khi triển khai.
