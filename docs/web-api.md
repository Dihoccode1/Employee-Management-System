# ASP.NET Core Web API và giao diện web

Project `API` dùng SDK Web và target .NET 10 từ `Directory.Build.props`. Project tham chiếu BUS và DTO, tái sử dụng DAO và SQL Server thông qua BUS. Không tạo thêm lớp nghiệp vụ hoặc database riêng cho web.

## Cấu trúc

```text
src/API/
  API.csproj
  Program.cs
  appsettings.json
  API.http
  Properties/
    launchSettings.json
  Configuration/
    DatabaseSettings.cs
  Extensions/
    ServiceCollectionExtensions.cs
  Controllers/
    HumanResources/
      Employees/EmployeesController.cs
      Departments/
      Positions/
      Contracts/
    ProjectManagement/
      Projects/
      ProjectDetails/
      Tasks/
      Assignments/
    SalaryInformation/
      MonthlyAttendance/
      Attendance/
      AttendanceStatuses/
      Payroll/
      SeniorityAllowanceRules/
      EmployeeSeniorityAllowances/
      EmployeeRewards/
      RewardRules/
    Statistics/
    Settings/
    ImportExport/
    Reporting/
  wwwroot/
    index.html
    css/site.css
    js/
      app.js
      services/employees-api.js
    pages/
      HumanResources/
      ProjectManagement/
      SalaryInformation/
```

Mỗi nhóm trong `wwwroot/pages` có sẵn các thư mục module giống nhóm controller tương ứng. Các thư mục có `.gitkeep` chỉ là nơi viết code tiếp; chưa có controller/trang CRUD của module đó.

## Luồng mẫu đã có

1. Mở trang `/`, bấm **Tải danh sách**.
2. `employees-api.js` gọi `GET /api/employees`.
3. `EmployeesController` gọi `EmployeeService` được inject qua constructor.
4. BUS gọi repository trong DAO, trả DTO về controller và trình duyệt dưới dạng JSON.

Controller nhận request, mã HTTP và trả dữ liệu; quy tắc nghiệp vụ tiếp tục viết trong BUS, truy vấn SQL trong DAO. JavaScript không kết nối trực tiếp database.

## Cấu hình và chạy

Chạy script SQL của dự án trước khi tải nhân viên. Tại thư mục solution:

```powershell
$env:HRM_CONNECTION_STRING = 'Server=localhost;Database=HumanResourceManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
dotnet run --project src/API
```

Mở `http://localhost:5080`. Thay tên server bằng instance SQL Server đang dùng. Cấu hình chứng chỉ trong ví dụ dành cho phát triển cục bộ.

API cũng nhận `ConnectionStrings__HumanResourceManagement` theo cấu hình ASP.NET Core. Nếu có cả hai, `HRM_CONNECTION_STRING` được ưu tiên. File `appsettings.json` để trống chuỗi kết nối; không commit mật khẩu vào source.

Chạy bằng Visual Studio: chọn API làm Startup Project, profile API. Chạy GUI cho WinForms; có thể chạy hai project đồng thời. Giao diện HTML và API dùng cùng địa chỉ nên chưa cần cấu hình CORS.

| Đường dẫn | Kết quả |
|---|---|
| `GET /` | Trang web khởi đầu |
| `GET /health` | HTTP 200, trạng thái tiến trình; không kiểm tra SQL Server |
| `GET /api/employees` | HTTP 200 và mảng nhân viên khi cấu hình/database hợp lệ |
| `GET /api/employees` khi thiếu cấu hình | HTTP 503 dạng ProblemDetails |

Lỗi ngoài dự kiến được ghi log phía server và trả ProblemDetails tổng quát. Chưa có endpoint thêm/sửa/xóa, tìm kiếm, xác thực/phân quyền hoặc Swagger/OpenAPI UI.

## Thêm chức năng quản lý đối tượng

Ví dụ phòng ban: viết DTO trong `DTO/HumanResources/Departments`, truy cập dữ liệu trong DAO, nghiệp vụ trong BUS, đăng ký service trong `API/Extensions/ServiceCollectionExtensions.cs`, rồi tạo `DepartmentsController` trong `API/Controllers/HumanResources/Departments`.

Sau đó tạo trang ở `wwwroot/pages/HumanResources/Departments`, thêm hàm gọi API trong `wwwroot/js/services`, và nối đường dẫn từ trang chính. Dùng GET/POST/PUT/DELETE khi triển khai xem/thêm/sửa/xóa. Các yêu cầu đặc thù WinForms như biến static để chọn dữ liệu vẫn thuộc GUI.
