# ProjectManagement

Module gồm bốn phần: Projects (Dự án), ProjectDetails (Chi tiết dự án), Tasks (Công việc), Assignments (Phân công).

## Quy tắc đã thống nhất

- Một dự án có nhiều chi tiết; một chi tiết có nhiều công việc; một công việc có nhiều phân công. Mỗi phân công gắn một nhân viên.
- Thưởng và phụ cấp nhập tay, lưu trong Chi tiết dự án. Không chia tiền tự động cho nhân viên hoặc chuyển sang bảng lương.
- Tiến độ của chi tiết, công việc và phân công nhập riêng trong khoảng 0–100. Trạng thái của cả bốn phần nhập bằng chữ, tối đa 50 ký tự. Không tự tổng hợp giữa các cấp.
- Bỏ trường điểm đánh giá đóng góp.
- Chặn xóa khi còn dữ liệu con. Xóa từ Phân công → Công việc → Chi tiết → Dự án.
- Mã do SQL Server tự tăng; khi thêm mới không cần nhập mã.

Các khóa ngoại ngược trong ERD gốc được bỏ: Projects không giữ ProjectDetailId; ProjectDetails không giữ TaskId. Assignments chỉ giữ TaskId, không lưu thêm ProjectId vì có thể lấy dự án qua công việc và chi tiết. File ERD gốc là tài liệu tham khảo, script 03 thể hiện schema đã điều chỉnh.

## Cách chạy

1. Trên database mới, chạy 01_CreateDatabase.sql và 02_SeedData.sql. Nếu đã có database thì không chạy lại hai script này.
2. Chạy 03_ProjectManagement.sql. Có thể chạy thêm 04_ProjectManagementSeed.sql để có dữ liệu minh họa.
3. Cấu hình HRM_CONNECTION_STRING như README, rồi chạy GUI.
4. Tạo dự án trước; tiếp theo tạo chi tiết, công việc và phân công. Cần có sẵn phòng ban và nhân viên để chọn.

Mỗi màn hình có Tải lại, Thêm mới, Lưu, Xóa và ô tìm kiếm. Tìm trên danh sách đang tải, không truy vấn SQL theo từng ký tự. Chọn dòng trong bảng để sửa; bấm Thêm mới để bỏ lựa chọn và nhập bản ghi mới. Sau khi lưu/xóa, dữ liệu được tải lại.

Ngày kết thúc thực tế của dự án và ngày hoàn thành của công việc/phân công được bỏ trống bằng cách bỏ dấu chọn ở ô ngày. Hạn hoàn thành phải từ ngày bắt đầu trở đi. Ngày hoàn thành thực tế có thể sau hạn để ghi nhận trễ hạn. Không tự thay đổi trạng thái khi nhập ngày hoặc tiến độ.

Ngân sách, phụ cấp, thưởng không âm và tối đa 2 chữ số thập phân. Thời gian dự kiến của công việc có đơn vị giờ. Các ô số không nhận giá trị âm. BUS cũng kiểm tra lại để các lệnh gọi ngoài GUI vẫn phải tuân thủ quy tắc.

## Đọc code theo thứ tự

Ví dụ với Chi tiết dự án:

1. DTO/ProjectManagement/ProjectDetails/ProjectDetailDto.cs: các thuộc tính chứa dữ liệu. Allowance là phụ cấp, Bonus là thưởng, CompletionPercentage là tiến độ.
2. DAO/ProjectManagement/ProjectDetails/ProjectDetailDao.cs: đọc/ghi bảng ProjectDetails bằng SQL có tham số. IProjectDetailDao mô tả các hàm DAO phải có.
3. BUS/ProjectManagement/ProjectDetails/ProjectDetailService.cs: kiểm tra dữ liệu rồi gọi DAO. SaveAsync thêm mới khi Id = 0, cập nhật khi Id > 0.
4. GUI/Forms/ProjectManagement/ProjectDetails/ProjectDetailForm.cs: tạo ô nhập, lấy dữ liệu từ các ô và gọi BUS.

Luồng khi bấm Lưu:

```text
ProjectDetailForm
    → tạo ProjectDetailDto từ ô nhập
    → ProjectDetailService.SaveAsync kiểm tra
    → ProjectDetailDao.InsertAsync hoặc UpdateAsync
    → SQL Server lưu dữ liệu
    → form tải lại danh sách
```

DTO chỉ chứa dữ liệu. GUI không viết SQL. BUS không gọi MessageBox. DAO không quyết định tiền thưởng hoặc tiến độ.

ProjectValidation chứa các kiểm tra dùng chung. ManagementFormBase chứa bố cục, nút bấm và xử lý lỗi dùng chung cho bốn form. Mỗi form con khai báo rõ từng ô nhập và cách chuyển sang DTO; không dùng reflection hoặc tự động sinh biểu mẫu lúc chạy.

ServiceFactory tạo BUS và DAO để GUI chỉ cần tham chiếu BUS/DTO. MyConnection tạo kết nối; mỗi lần đọc/ghi đều mở và giải phóng kết nối bằng using. async/await cho phép đợi SQL mà không làm đứng giao diện.

Tên bảng tiếng Anh:

| Bảng | Nội dung | Khóa ngoại chính |
|---|---|---|
| Projects | Tên, ngày bắt đầu/kết thúc, hạn, mô tả, ngân sách, phòng ban, người quản lý, trạng thái | DepartmentId, ManagerId |
| ProjectDetails | Tiến độ, trạng thái, phụ cấp, thưởng | ProjectId |
| ProjectTasks | Tên, mô tả, loại, ưu tiên, ngày, số giờ dự kiến, tiến độ, trạng thái, ghi chú | ProjectDetailId |
| Assignments | Nhân viên, ngày phân công, ngày thực hiện, tiến độ, trạng thái, ghi chú | TaskId, EmployeeId |

## Kiểm thử

Kiểm thử BUS dùng DAO giả, không cần cài thêm thư viện test:

```powershell
dotnet run --project tests/ProjectManagement.Tests
```

Kiểm thử tích hợp trên SQL Server LocalDB riêng:

```powershell
SqlLocalDB create CodexEMSProjectTests -s
dotnet run --project tests/ProjectManagement.Tests -- --sql
```

Nếu instance đã có, dùng SqlLocalDB start CodexEMSProjectTests. Bộ test tạo database với tên EMS_ProjectTests_ kèm GUID và tự xóa khi xong; không đọc HRM_CONNECTION_STRING của ứng dụng.

Test SQL chạy cả bốn script, kiểm tra khả năng chạy lại script 03/04, thêm/sửa/xóa cả bốn bảng, dữ liệu tiếng Việt có dấu nháy, ngày có thể trống, tiền có phần lẻ, khóa ngoại, chặn xóa cha, bản ghi đã bị xóa, ràng buộc SQL và việc không tự tính lại giá trị ở cấp cha.

Phạm vi này chưa gồm tìm kiếm nâng cao AND/OR/NOT, hai ListView main–sub, nhập/xuất Excel/PDF hoặc tính lương. Các chức năng đó vẫn được theo dõi riêng trong project-requirements.md.
