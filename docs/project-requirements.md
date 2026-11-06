# Đối chiếu yêu cầu đồ án

Đối chiếu tài liệu `C-w7-YeuCauDoAn (1).pdf` do người dùng cung cấp. Đây là kế hoạch triển khai cho đề tài quản lý nhân sự; thư mục có `.gitkeep` không có nghĩa là đã hoàn thành chức năng.

| Mục trong PDF | Phần phụ trách | Tình trạng / việc tiếp theo |
|---|---|---|
| 1. 3-Layer ver2, class MyConnection | GUI, BUS, DAO; DAO/Infrastructure/MyConnection.cs | Đã chia project và có lớp tạo kết nối; cần đối chiếu thêm mẫu ver2 học trên lớp nếu có yêu cầu chi tiết |
| 2. Tìm kiếm trên ArrayList trong BUS | BUS/ProjectManagement và các service liên quan | Đã có tìm kiếm cơ bản trên ArrayList cho cả bốn module ProjectManagement; phần nhân viên chưa có tìm kiếm |
| 3. Form main–sub, hai ListView, quan hệ 1–n | GUI/Forms/ProjectManagement/Projects | Đã có ProjectForm: chọn dự án để xem công việc trong ListView phía dưới |
| 4. Nút ... tìm kiếm, chọn dữ liệu điền form qua static | GUI/Forms/Lookups, GUI/Selection | Đã có thư mục; chưa có form/lớp chọn dữ liệu |
| 5. Tính duy nhất của dữ liệu | BUS và ràng buộc SQL Server | ProjectManagement có mã tự tăng, mỗi dự án tối đa một chi tiết, mỗi cặp công việc–nhân viên một phân công; BUS và SQL cùng kiểm tra. Các module khác cần phát triển tiếp |
| 6. Dữ liệu thay đổi khi phát sinh nghiệp vụ | BUS/HumanResources/Contracts, Attendance, Payroll; DAO tương ứng | Cần thống nhất nghiệp vụ và transaction cho các thao tác liên quan nhiều bảng |
| 7. Trường không cho nhập | GUI và BUS | Mã ProjectManagement do SQL tự tăng và chỉ đọc trên form; lương thực nhận vẫn là phần dự kiến |
| 8. Tìm kiếm nâng cao | Form của module và BUS tương ứng | Cần AND, OR, NOT, >, >=, <, <=, <> trên một/nhiều trường; xử lý ArrayList trong BUS |
| 9. Table tham số | Settings trong GUI/BUS/DAO/DTO; database | Đã có thư mục; cần thiết kế bảng và quy tắc, ví dụ số giờ làm chuẩn hoặc mức phụ cấp |
| 10. Thống kê tiền theo ngày/tháng/quý/năm | Statistics trong GUI/BUS/DAO/DTO | Chưa triển khai; cần thống nhất với giảng viên cách áp dụng ví dụ thu/chi/lợi nhuận của tài liệu sang đề tài nhân sự |
| 11. Import/export xls (1 điểm) | BUS/ImportExport, GUI của module | Chưa triển khai/chọn thư viện; phải hỗ trợ .xls, không chỉ .xlsx |
| 12. Report → PDF (1 điểm) | BUS/Reporting, GUI/Reports | Chưa triển khai; dự kiến phiếu lương hoặc bảng lương |
| 13. Màn hình desktop, menu trái, form con | GUI/Forms/MainForm.cs và các form module | Đã có bảng nhân viên và bốn nút mở các form ProjectManagement dạng hộp thoại; chưa có menu trái/form con nhúng |
| 14. Báo cáo cuối kỳ | docs/report, docs/screenshots | Đã có thư mục; chưa có báo cáo |

## Báo cáo cuối kỳ

- Tối đa 20 trang, Times New Roman cỡ 13, không có trang mục lục.
- Bìa: học phần Lập trình C#, tên đề tài, thành viên/lớp, giảng viên hướng dẫn và dòng tháng 12/2026 theo tài liệu.
- Nội dung: giới thiệu, thiết kế CSDL kèm hình rõ nét, các chức năng chính và chức năng con.
- Mỗi chức năng cần giới thiệu, ảnh giao diện có chú thích, công thức tính và đầu vào/đầu ra/xử lý của hàm.
- Hướng dẫn cài đặt: ảnh cây thư mục thể hiện GUI–BUS–DAO, cách kết nối CSDL, công cụ/thư viện và liên kết tải.
- Sản phẩm nộp: liên kết source và báo cáo DOC/PDF.

## Phạm vi cần thống nhất

Các ví dụ bán hàng, hóa đơn, nhập kho trong tài liệu là cơ sở để đối chiếu yêu cầu. Cần thống nhất với giảng viên cách áp dụng thống kê tiền và nghiệp vụ cập nhật dữ liệu cho quản lý nhân sự trước khi chốt công thức và database.

Đăng nhập/phân quyền chưa được triển khai. Đây là phần có thể bổ sung theo phạm vi đề tài, không được tính là đã hoàn thành chỉ vì có ví dụ minh họa trong tài liệu.
