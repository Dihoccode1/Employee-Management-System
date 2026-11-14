USE HumanResourceManagement;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'HumanResourceManagement'
    THROW 50020, N'Cần chọn database HumanResourceManagement.', 1;

IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL OR OBJECT_ID(N'dbo.Projects', N'U') IS NULL
    THROW 50021, N'Cần chạy 01_CreateDatabase.sql và 03_ProjectManagement.sql trước.', 1;

DECLARE @departments TABLE (SortOrder INT PRIMARY KEY, Code VARCHAR(20), Name NVARCHAR(100), Description NVARCHAR(500));
INSERT @departments VALUES
    (1, 'HR', N'Nhân sự', N'Quản lý hồ sơ nhân sự, tuyển dụng và phúc lợi'),
    (2, 'IT', N'Công nghệ thông tin', N'Phát triển phần mềm và hạ tầng kỹ thuật'),
    (3, 'FIN', N'Tài chính', N'Kế toán, ngân sách và kiểm soát chi phí'),
    (4, 'OPS', N'Vận hành', N'Điều phối vận hành và tài sản doanh nghiệp'),
    (5, 'SALES', N'Kinh doanh', N'Phát triển thị trường và chăm sóc khách hàng'),
    (6, 'MKT', N'Tiếp thị', N'Truyền thông, thương hiệu và khảo sát thị trường');

DECLARE @positions TABLE (Code VARCHAR(20) PRIMARY KEY, Name NVARCHAR(100), Description NVARCHAR(500), AllowanceAmount DECIMAL(18,2));
INSERT @positions VALUES
    ('STAFF', N'Nhân viên', N'Nhân viên thực hiện nghiệp vụ chuyên môn', 500000),
    ('ANALYST', N'Chuyên viên', N'Chuyên viên phân tích và tổng hợp nghiệp vụ', 1200000),
    ('LEAD', N'Trưởng nhóm', N'Điều phối công việc nhóm chuyên môn', 2000000),
    ('MANAGER', N'Trưởng phòng', N'Quản lý và điều hành phòng ban', 3500000);

DECLARE @project_samples TABLE (
    SortOrder INT PRIMARY KEY,
    Name NVARCHAR(200),
    DepartmentCode VARCHAR(20),
    StartDate DATE,
    Deadline DATE,
    Budget DECIMAL(18,2),
    Status NVARCHAR(50),
    Description NVARCHAR(2000)
);

INSERT @project_samples VALUES
    (1, N'Số hóa hồ sơ nhân sự', 'HR', '2026-09-01', '2026-11-30', 75000000, N'Đang thực hiện', N'Chuẩn hóa và lưu trữ hồ sơ nhân viên.'),
    (2, N'Cổng đăng ký nghỉ phép', 'IT', '2026-09-15', '2026-12-15', 120000000, N'Đang thực hiện', N'Xây dựng biểu mẫu gửi và duyệt đơn nghỉ.'),
    (3, N'Đối soát chi phí nội bộ', 'FIN', '2026-08-01', '2026-10-31', 48000000, N'Hoàn thành', N'Rà soát và đối chiếu chứng từ theo phòng ban.'),
    (4, N'Tối ưu quy trình bàn giao', 'OPS', '2026-10-01', '2026-12-10', 62000000, N'Đang thực hiện', N'Chuẩn hóa danh mục việc cần bàn giao.'),
    (5, N'Quản lý khách hàng doanh nghiệp', 'SALES', '2026-10-15', '2027-01-15', 160000000, N'Đang thực hiện', N'Tập trung thông tin khách hàng và lịch chăm sóc.'),
    (6, N'Chiến dịch nhận diện thương hiệu', 'MKT', '2026-09-05', '2026-11-05', 95000000, N'Hoàn thành', N'Cập nhật tài liệu giới thiệu và bộ nhận diện.'),
    (7, N'Chương trình hội nhập nhân viên mới', 'HR', '2026-11-15', '2027-01-31', 44000000, N'Kế hoạch', N'Xây dựng tài liệu hướng dẫn và lịch đào tạo.'),
    (8, N'Nâng cấp hệ thống chấm công', 'IT', '2026-10-20', '2026-12-20', 135000000, N'Đang thực hiện', N'Tích hợp dữ liệu giờ vào, giờ ra và bảng tổng hợp.'),
    (9, N'Lập ngân sách năm 2027', 'FIN', '2026-11-20', '2026-12-31', 32000000, N'Kế hoạch', N'Tổng hợp dự toán và kế hoạch chi phí năm tới.'),
    (10, N'Kiểm kê thiết bị văn phòng', 'OPS', '2026-09-10', '2026-10-20', 26000000, N'Hoàn thành', N'Đối chiếu thiết bị và cập nhật người phụ trách.'),
    (11, N'Mở rộng kênh bán hàng', 'SALES', '2026-11-01', '2027-02-15', 145000000, N'Đang thực hiện', N'Khảo sát và thử nghiệm các kênh phân phối mới.'),
    (12, N'Khảo sát mức độ hài lòng', 'MKT', '2026-11-18', '2027-01-10', 38000000, N'Kế hoạch', N'Thu thập phản hồi và tổng hợp kết quả khảo sát.');

BEGIN TRY
    BEGIN TRANSACTION;

    -- Khóa trong giao dịch để hai lần nạp đồng thời không vượt số lượng mục tiêu.
    DECLARE @employee_count INT = (SELECT COUNT(*) FROM dbo.Employees WITH (TABLOCKX, HOLDLOCK));
    DECLARE @project_count INT = (SELECT COUNT(*) FROM dbo.Projects WITH (TABLOCKX, HOLDLOCK));
    DECLARE @employees_to_add INT = CASE WHEN @employee_count < 55 THEN 55 - @employee_count ELSE 0 END;
    DECLARE @projects_to_add INT = CASE WHEN @project_count < 12 THEN 12 - @project_count ELSE 0 END;

    IF @employees_to_add > 0 OR @projects_to_add > 0
    BEGIN
        INSERT dbo.Departments (Code, Name, Description)
        SELECT sample.Code, sample.Name, sample.Description FROM @departments AS sample
        WHERE NOT EXISTS (SELECT 1 FROM dbo.Departments AS existing WHERE existing.Code = sample.Code);
    END;

    IF @employees_to_add > 0
    BEGIN
        INSERT dbo.Positions (Code, Name, Description, AllowanceAmount)
        SELECT sample.Code, sample.Name, sample.Description, sample.AllowanceAmount FROM @positions AS sample
        WHERE NOT EXISTS (SELECT 1 FROM dbo.Positions AS existing WHERE existing.Code = sample.Code);

        -- Ghép 5 họ/tên đệm với 11 tên để tạo danh sách 55 nhân viên giả lập đầy đủ thuộc tính ERD.
        WITH employee_samples AS (
            SELECT (family.SortOrder - 1) * 11 + given.SortOrder AS SortOrder,
                   family.Name AS LastName,
                   given.Name AS FirstName,
                   CONCAT(family.Name, N' ', given.Name) AS FullName
            FROM (VALUES (1, N'Nguyễn Minh'), (2, N'Trần Thanh'), (3, N'Lê Hoài'),
                         (4, N'Phạm Gia'), (5, N'Võ Bảo')) AS family(SortOrder, Name)
            CROSS JOIN (VALUES (1, N'An'), (2, N'Anh'), (3, N'Bình'), (4, N'Châu'),
                               (5, N'Duy'), (6, N'Hà'), (7, N'Hân'), (8, N'Khang'),
                               (9, N'Linh'), (10, N'Phúc'), (11, N'Trang')) AS given(SortOrder, Name)
        )
        INSERT dbo.Employees (
            EmployeeCode, LastName, FirstName, FullName, DateOfBirth, Gender, PhoneNumber, Email,
            DepartmentId, PositionId, HireDate, Status, IsActive, CurrentBaseSalary, Degree)
        SELECT TOP (@employees_to_add)
               code.EmployeeCode,
               sample.LastName,
               sample.FirstName,
               sample.FullName,
               DATEADD(YEAR, -(23 + (sample.SortOrder % 15)), CONVERT(DATE, '2026-01-01')),
               CASE WHEN sample.SortOrder % 2 = 0 THEN N'Nữ' ELSE N'Nam' END,
               CONCAT('09', RIGHT(CONCAT('00000000', 10000000 + sample.SortOrder * 137), 8)),
               CONCAT(LOWER(code.EmployeeCode), '@nnt-demo.example'),
               department.Id,
               position.Id,
               DATEADD(DAY, sample.SortOrder * 21, CONVERT(DATE, '2022-01-03')),
               CASE WHEN sample.SortOrder > 52 THEN N'Đã nghỉ việc' ELSE N'Đang làm việc' END,
               CASE WHEN sample.SortOrder > 52 THEN 0 ELSE 1 END,
               CASE
                   WHEN sample.SortOrder <= 6 THEN 25000000
                   WHEN sample.SortOrder <= 12 THEN 18000000
                   WHEN sample.SortOrder % 3 = 0 THEN 14000000
                   ELSE 10000000
               END,
               CASE
                   WHEN sample.SortOrder <= 12 THEN N'Thạc sĩ'
                   WHEN sample.SortOrder % 4 = 0 THEN N'Cao đẳng'
                   ELSE N'Đại học'
               END
        FROM employee_samples AS sample
        CROSS APPLY (VALUES (CONCAT('DEMO', RIGHT(CONCAT('000', sample.SortOrder), 3)))) AS code(EmployeeCode)
        JOIN @departments AS department_sample ON department_sample.SortOrder = (sample.SortOrder - 1) % 6 + 1
        JOIN dbo.Departments AS department ON department.Code = department_sample.Code
        JOIN dbo.Positions AS position ON position.Code = CASE
            WHEN sample.SortOrder <= 6 THEN 'MANAGER'
            WHEN sample.SortOrder <= 12 THEN 'LEAD'
            WHEN sample.SortOrder % 3 = 0 THEN 'ANALYST'
            ELSE 'STAFF'
        END
        WHERE NOT EXISTS (SELECT 1 FROM dbo.Employees AS existing WHERE existing.EmployeeCode = code.EmployeeCode)
        ORDER BY sample.SortOrder;
    END;

    -- Bổ sung hợp đồng mẫu cho các nhân viên chưa có hợp đồng để test màn hình Main-Sub.
    INSERT dbo.Contracts (EmployeeId, ContractCode, ContractType, StartDate, EndDate, BaseSalary, Status)
    SELECT e.Id,
           CONCAT('HD-', e.EmployeeCode),
           CASE WHEN e.IsActive = 1 THEN N'Xác định thời hạn' ELSE N'Thử việc' END,
           e.HireDate,
           CASE WHEN e.IsActive = 1 THEN DATEADD(YEAR, 2, e.HireDate) ELSE DATEADD(MONTH, 3, e.HireDate) END,
           CASE WHEN e.CurrentBaseSalary > 0 THEN e.CurrentBaseSalary ELSE 10000000 END,
           CASE WHEN e.IsActive = 1 THEN N'Còn hiệu lực' ELSE N'Hết hiệu lực' END
    FROM dbo.Employees AS e
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Contracts AS c WHERE c.EmployeeId = e.Id);

    -- Ưu tiên nhân viên đang làm việc cùng phòng ban khi chọn quản lý dự án.
    INSERT dbo.Projects (Name, StartDate, EndDate, Deadline, Description, Budget, DepartmentId, ManagerId, Status)
    SELECT TOP (@projects_to_add) sample.Name, sample.StartDate,
           CASE WHEN sample.Status = N'Hoàn thành' THEN sample.Deadline END,
           sample.Deadline, sample.Description, sample.Budget, department.Id, manager.Id, sample.Status
    FROM @project_samples AS sample
    JOIN dbo.Departments AS department ON department.Code = sample.DepartmentCode
    CROSS APPLY (
        SELECT TOP (1) employee.Id
        FROM dbo.Employees AS employee
        LEFT JOIN dbo.Positions AS position ON position.Id = employee.PositionId
        ORDER BY employee.IsActive DESC,
                 CASE WHEN employee.DepartmentId = department.Id THEN 0 ELSE 1 END,
                 CASE WHEN position.Code = 'MANAGER' THEN 0 ELSE 1 END,
                 employee.Id
    ) AS manager
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Projects AS existing WHERE existing.Name = sample.Name)
    ORDER BY sample.SortOrder;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT (SELECT COUNT(*) FROM dbo.Employees) AS EmployeeCount,
       (SELECT COUNT(*) FROM dbo.Contracts) AS ContractCount,
       (SELECT COUNT(*) FROM dbo.Projects) AS ProjectCount;
GO
