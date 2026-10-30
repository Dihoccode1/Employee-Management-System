# Kiến trúc

Luồng xử lý: GUI → BUS → DAL → SQL Server. DTO dùng chung để trao đổi dữ liệu, không phải lớp xử lý thứ tư.

Tham chiếu project: GUI → BUS, DTO; BUS → DAL, DTO; DAL → DTO. ServiceFactory khởi tạo các dependency, giữ GUI không tham chiếu trực tiếp DAL.

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

Đây là schema và bộ khung ban đầu. Khi phát triển cần bổ sung validation trong BUS, transaction cho thao tác ghi nhiều bảng và migration cho thay đổi database. Nếu đề tài cần đăng nhập, thiết kế thêm xác thực và phân quyền.
