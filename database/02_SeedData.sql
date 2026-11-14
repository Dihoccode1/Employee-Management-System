USE HumanResourceManagement;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Code = 'HR')
    INSERT INTO dbo.Departments (Code, Name, Description)
    VALUES ('HR', N'Nhân sự', N'Quản lý hồ sơ nhân sự, tuyển dụng và hợp đồng lao động');

IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Code = 'IT')
    INSERT INTO dbo.Departments (Code, Name, Description)
    VALUES ('IT', N'Công nghệ thông tin', N'Phát triển phần mềm và vận hành hạ tầng hệ thống');

IF NOT EXISTS (SELECT 1 FROM dbo.Positions WHERE Code = 'STAFF')
    INSERT INTO dbo.Positions (Code, Name, Description, AllowanceAmount)
    VALUES ('STAFF', N'Nhân viên', N'Nhân viên chuyên môn thực hiện nghiệp vụ', 500000);

IF NOT EXISTS (SELECT 1 FROM dbo.Positions WHERE Code = 'MANAGER')
    INSERT INTO dbo.Positions (Code, Name, Description, AllowanceAmount)
    VALUES ('MANAGER', N'Trưởng phòng', N'Quản lý và điều hành hoạt động phòng ban', 3000000);

IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE EmployeeCode = 'NV001')
BEGIN
    INSERT INTO dbo.Employees
        (EmployeeCode, LastName, FirstName, FullName, DateOfBirth, Gender, PhoneNumber, Email,
         DepartmentId, PositionId, HireDate, Status, IsActive, CurrentBaseSalary, Degree)
    SELECT 'NV001', N'Nguyễn Văn', N'An', N'Nguyễn Văn An', '1998-05-12', N'Nam', '0901234567',
           'nv001@company.example', d.Id, p.Id, '2026-01-01', N'Đang làm việc', 1, 12000000, N'Đại học'
    FROM dbo.Departments d
    CROSS JOIN dbo.Positions p
    WHERE d.Code = 'HR' AND p.Code = 'STAFF';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Contracts WHERE ContractCode = 'HD001')
BEGIN
    INSERT INTO dbo.Contracts (EmployeeId, ContractCode, ContractType, StartDate, EndDate, BaseSalary, Status)
    SELECT e.Id, 'HD001', N'Xác định thời hạn', '2026-01-01', '2027-01-01', 12000000, N'Còn hiệu lực'
    FROM dbo.Employees e
    WHERE e.EmployeeCode = 'NV001';
END;

-- Khởi tạo các tham số nghiệp vụ mặc định nếu chưa tồn tại.
IF NOT EXISTS (SELECT 1 FROM dbo.SystemParameters WHERE Code = 'STANDARD_WORK_DAYS')
    INSERT INTO dbo.SystemParameters (Code, Name, Value, ValueType, MinValue, MaxValue, Description)
    VALUES ('STANDARD_WORK_DAYS', N'Số ngày công chuẩn trong tháng', '26', 'INT', 1, 31, N'Số ngày công chuẩn dùng tính chấm công và lương tháng');

IF NOT EXISTS (SELECT 1 FROM dbo.SystemParameters WHERE Code = 'STANDARD_WORK_HOURS')
    INSERT INTO dbo.SystemParameters (Code, Name, Value, ValueType, MinValue, MaxValue, Description)
    VALUES ('STANDARD_WORK_HOURS', N'Số giờ làm việc chuẩn mỗi ngày', '8', 'DECIMAL', 1, 24, N'Số giờ làm việc tiêu chuẩn trong một ngày');

IF NOT EXISTS (SELECT 1 FROM dbo.SystemParameters WHERE Code = 'MIN_BASE_SALARY')
    INSERT INTO dbo.SystemParameters (Code, Name, Value, ValueType, MinValue, MaxValue, Description)
    VALUES ('MIN_BASE_SALARY', N'Lương cơ bản tối thiểu', '4960000', 'DECIMAL', 0, 1000000000, N'Mức lương cơ bản tối thiểu áp dụng khi lập hồ sơ nhân viên và hợp đồng');

IF NOT EXISTS (SELECT 1 FROM dbo.SystemParameters WHERE Code = 'MAX_OVERTIME_HOURS_MONTH')
    INSERT INTO dbo.SystemParameters (Code, Name, Value, ValueType, MinValue, MaxValue, Description)
    VALUES ('MAX_OVERTIME_HOURS_MONTH', N'Số giờ tăng ca tối đa/tháng', '40', 'DECIMAL', 0, 200, N'Giới hạn giờ làm thêm hợp lệ trong một tháng');

IF NOT EXISTS (SELECT 1 FROM dbo.SystemParameters WHERE Code = 'LATE_THRESHOLD_MINUTES')
    INSERT INTO dbo.SystemParameters (Code, Name, Value, ValueType, MinValue, MaxValue, Description)
    VALUES ('LATE_THRESHOLD_MINUTES', N'Số phút trễ cho phép', '15', 'INT', 0, 120, N'Số phút đi trễ tối đa trước khi ghi nhận đi muộn');
GO
