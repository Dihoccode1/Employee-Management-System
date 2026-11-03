# Kiến trúc

Luồng xử lý: GUI → BUS → DAO → SQL Server. DTO dùng chung để trao đổi dữ liệu, không phải lớp xử lý thứ tư.

Tham chiếu project: GUI → BUS, DTO; BUS → DAO, DTO; DAO → DTO. ServiceFactory khởi tạo các dependency, giữ GUI không tham chiếu trực tiếp DAO.

OOP: repository đóng gói truy cập dữ liệu; IEmployeeRepository tách hợp đồng khỏi triển khai; constructor nhận dependency để có thể thay repository khi kiểm thử.

## Module dự kiến

| Thư mục | Chức năng |
|---|---|
| Employees | Hồ sơ nhân viên, thêm/sửa/ngừng làm việc, tìm kiếm |
| Departments | Phòng ban |
| Positions | Chức vụ |
| Contracts | Hợp đồng, thời hạn và lương cơ bản |
| Attendance | Chấm công |
| Payroll | Tính và lưu bảng lương |
| Statistics | Tổng hợp dữ liệu theo khoảng ngày, tháng, quý, năm |
| Settings | Đọc và cập nhật tham số nghiệp vụ |

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
- Khi phát triển tìm kiếm, xử lý trên `ArrayList` trong BUS theo đề bài; mã hiện tại chỉ đọc danh sách nhân viên bằng `IReadOnlyList<EmployeeDto>`.
- Form main–sub dùng hai `ListView` cho quan hệ một–nhiều, chẳng hạn chọn nhân viên để hiện danh sách hợp đồng. `MainForm` hiện chưa có bố cục menu trái/form con hoặc phần main–sub này.

Đây là schema và bộ khung ban đầu. Khi phát triển cần bổ sung validation trong BUS, transaction cho thao tác ghi nhiều bảng và migration cho thay đổi database. Nếu đề tài cần đăng nhập, thiết kế thêm xác thực và phân quyền.
