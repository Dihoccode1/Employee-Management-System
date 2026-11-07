# Kiến trúc

Luồng xử lý: GUI → BUS → DAO → SQL Server. DTO dùng chung để trao đổi dữ liệu, không phải lớp xử lý thứ tư.

Tham chiếu project: GUI → BUS, DTO; BUS → DAO, DTO; DAO → DTO. ServiceFactory khởi tạo các dependency, giữ GUI không tham chiếu trực tiếp DAO.

OOP: repository đóng gói truy cập dữ liệu; IEmployeeRepository tách hợp đồng khỏi triển khai; constructor nhận dependency để có thể thay repository khi kiểm thử.

## Nhóm nghiệp vụ theo màu ERD

Ba thư mục nhóm được đặt trong mỗi project BUS/DAO/DTO và trong GUI/Forms. Namespace của code đã có được cập nhật theo thư mục, ví dụ `BUS.HumanResources.Employees`.

| Màu | Nhóm | Thư mục con | Bảng/chức năng |
|---|---|---|---|
| Xanh lá | HumanResources | Departments | Phòng ban |
| Xanh lá | HumanResources | Positions | Chức vụ |
| Xanh lá | HumanResources | Employees | Nhân viên |
| Xanh lá | HumanResources | Contracts | Hợp đồng lao động |
| Đỏ | ProjectManagement | Projects | Dự án |
| Đỏ | ProjectManagement | ProjectDetails | Chi tiết dự án |
| Đỏ | ProjectManagement | Tasks | Công việc |
| Đỏ | ProjectManagement | Assignments | Phân công |
| Xanh lam | SalaryInformation | MonthlyAttendance | Thông tin chấm công tháng |
| Xanh lam | SalaryInformation | Attendance | Bảng chấm công |
| Xanh lam | SalaryInformation | AttendanceStatuses | Bảng trạng thái chấm công |
| Xanh lam | SalaryInformation | Payroll | Bảng lương |
| Xanh lam | SalaryInformation | SeniorityAllowanceRules | Quy định phụ cấp thâm niên |
| Xanh lam | SalaryInformation | EmployeeSeniorityAllowances | Nhân viên phụ cấp thâm niên |
| Xanh lam | SalaryInformation | EmployeeRewards | Nhân viên khen thưởng |
| Xanh lam | SalaryInformation | RewardRules | Quy định thưởng chung |

Các phần dùng chung (kết nối, thống kê, tham số, tra cứu, Excel/PDF) nằm ngoài ba nhóm. Đây là cách tổ chức module trong kiến trúc 3 lớp, không tạo thêm project hoặc lớp kiến trúc.

ERD gốc: [Sơ đồ quản lý nhân sự](diagrams/ERD_quan_ly_nhan_su_chu_lon.drawio). ProjectManagement đã triển khai với quan hệ Projects → ProjectDetails → ProjectTasks → Assignments. Mỗi bảng con giữ khóa ngoại đến bảng cha; không có khóa ngoại ngược. Các khác biệt với ERD và quy tắc nhập tay được giải thích trong [ProjectManagement](project-management.md). Các module còn lại chưa đồng bộ đầy đủ với ERD.

## Các phần hỗ trợ

- `GUI/Forms/Lookups`: form hỗ trợ tìm kiếm/chọn nhân viên, phòng ban... mở từ nút `...`.
- `GUI/Selection`: dự kiến chứa lớp giữ kết quả chọn bằng biến `static` theo yêu cầu môn học; không lưu dữ liệu chọn giao diện vào BUS/DAO.
- `GUI/Reports`: bố cục hoặc mẫu báo cáo; `BUS/Reporting` xử lý dữ liệu và xuất PDF.
- `BUS/ImportExport`: xử lý nhập/xuất `.xls`; GUI chỉ mở hộp thoại chọn file và hiển thị kết quả. BUS gọi DAO khi cần đọc/ghi SQL Server.
- `Statistics` và `Settings` có thư mục trong GUI/BUS/DAO/DTO để phát triển xuyên suốt các lớp.
- `docs/report` và `docs/screenshots`: báo cáo cuối kỳ và hình minh họa.

Các thư mục hỗ trợ hiện chỉ có `.gitkeep`, chưa có triển khai hoặc package Excel/PDF. Database chưa có bảng tham số.

## Quy ước theo môn học

- Tên lớp truy cập dữ liệu là DAO; các class repository hiện tại thuộc DAO, không tạo thêm một project DAL song song.
- `DAO/Infrastructure/MyConnection.cs` tạo kết nối SQL Server, được truyền vào repository qua constructor. Mỗi truy vấn tự mở và giải phóng kết nối.
- ProjectManagement tìm kiếm trên `ArrayList` trong BUS; GUI giữ danh sách đã tải, BUS trả kết quả theo mã/tên/trạng thái và các trường liên quan. Nhân viên vẫn chỉ đọc danh sách bằng `IReadOnlyList<EmployeeDto>`.
- Form main–sub dùng hai `ListView` cho quan hệ một–nhiều, chẳng hạn chọn nhân viên để hiện danh sách hợp đồng. `MainForm` hiện chưa có bố cục menu trái/form con hoặc phần main–sub này.

ProjectManagement kiểm tra dữ liệu trong BUS; DAO dùng tham số SQL và mỗi thao tác ghi một bảng. SQL Server chặn xóa khi còn dữ liệu con bằng khóa ngoại không cascade. Script 03 bổ sung bảng vào database cũ; không cần xóa dữ liệu nhân sự. Các module chưa triển khai vẫn cần bổ sung validation, transaction nếu ghi nhiều bảng và migration khi thay đổi schema. Nếu đề tài cần đăng nhập, thiết kế thêm xác thực và phân quyền.
