USE HumanResourceManagement;
GO
SET XACT_ABORT ON;

-- Script nâng cấp an toàn cho CSDL đã chạy 01_CreateDatabase.sql trước đó:
-- bổ sung thuộc tính ERD cho Nhân sự, giữ nguyên khóa Id làm FK cho Hợp đồng/Dự án/Chấm công,
-- đồng thời tạo bảng tham số nghiệp vụ dbo.SystemParameters và seed tham số mặc định.

IF COL_LENGTH(N'dbo.Departments', N'Description') IS NULL
    ALTER TABLE dbo.Departments ADD Description NVARCHAR(500) NOT NULL CONSTRAINT DF_Departments_Description DEFAULT (N'');

IF COL_LENGTH(N'dbo.Positions', N'Description') IS NULL
    ALTER TABLE dbo.Positions ADD Description NVARCHAR(500) NOT NULL CONSTRAINT DF_Positions_Description DEFAULT (N'');
IF COL_LENGTH(N'dbo.Positions', N'AllowanceAmount') IS NULL
    ALTER TABLE dbo.Positions ADD AllowanceAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Positions_AllowanceAmount DEFAULT (0)
        CONSTRAINT CK_Positions_AllowanceAmount CHECK (AllowanceAmount >= 0);

IF COL_LENGTH(N'dbo.Employees', N'LastName') IS NULL
    ALTER TABLE dbo.Employees ADD LastName NVARCHAR(100) NOT NULL CONSTRAINT DF_Employees_LastName DEFAULT (N'');
IF COL_LENGTH(N'dbo.Employees', N'FirstName') IS NULL
    ALTER TABLE dbo.Employees ADD FirstName NVARCHAR(50) NOT NULL CONSTRAINT DF_Employees_FirstName DEFAULT (N'');
IF COL_LENGTH(N'dbo.Employees', N'DateOfBirth') IS NULL
    ALTER TABLE dbo.Employees ADD DateOfBirth DATE NOT NULL CONSTRAINT DF_Employees_DateOfBirth DEFAULT ('2000-01-01');
IF COL_LENGTH(N'dbo.Employees', N'Gender') IS NULL
    ALTER TABLE dbo.Employees ADD Gender NVARCHAR(20) NOT NULL CONSTRAINT DF_Employees_Gender DEFAULT (N'Nam');
IF COL_LENGTH(N'dbo.Employees', N'PhoneNumber') IS NULL
    ALTER TABLE dbo.Employees ADD PhoneNumber VARCHAR(20) NOT NULL CONSTRAINT DF_Employees_PhoneNumber DEFAULT ('');
IF COL_LENGTH(N'dbo.Employees', N'Status') IS NULL
    ALTER TABLE dbo.Employees ADD Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Employees_Status DEFAULT (N'Đang làm việc');
IF COL_LENGTH(N'dbo.Employees', N'CurrentBaseSalary') IS NULL
    ALTER TABLE dbo.Employees ADD CurrentBaseSalary DECIMAL(18,2) NOT NULL CONSTRAINT DF_Employees_CurrentBaseSalary DEFAULT (0)
        CONSTRAINT CK_Employees_CurrentBaseSalary CHECK (CurrentBaseSalary >= 0);
IF COL_LENGTH(N'dbo.Employees', N'Degree') IS NULL
    ALTER TABLE dbo.Employees ADD Degree NVARCHAR(100) NULL;
IF COL_LENGTH(N'dbo.Employees', N'CreatedAt') IS NULL
    ALTER TABLE dbo.Employees ADD CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT (SYSUTCDATETIME());

IF COL_LENGTH(N'dbo.Contracts', N'ContractType') IS NULL
    ALTER TABLE dbo.Contracts ADD ContractType NVARCHAR(100) NOT NULL CONSTRAINT DF_Contracts_ContractType DEFAULT (N'Xác định thời hạn');
IF COL_LENGTH(N'dbo.Contracts', N'Status') IS NULL
    ALTER TABLE dbo.Contracts ADD Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Contracts_Status DEFAULT (N'Còn hiệu lực');

IF OBJECT_ID(N'dbo.Assignments', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Assignments', N'ContributionScore') IS NULL
    ALTER TABLE dbo.Assignments ADD ContributionScore DECIMAL(5,2) NOT NULL
        CONSTRAINT DF_Assignments_ContributionScore DEFAULT (0)
        CONSTRAINT CK_Assignments_ContributionScore CHECK (ContributionScore BETWEEN 0 AND 100);

IF OBJECT_ID(N'dbo.SystemParameters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemParameters (
        Code VARCHAR(50) NOT NULL PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        Value NVARCHAR(200) NOT NULL,
        ValueType VARCHAR(20) NOT NULL CONSTRAINT DF_SystemParameters_ValueType DEFAULT ('DECIMAL'),
        MinValue DECIMAL(18,2) NULL,
        MaxValue DECIMAL(18,2) NULL,
        Description NVARCHAR(500) NOT NULL CONSTRAINT DF_SystemParameters_Description DEFAULT (N''),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_SystemParameters_UpdatedAt DEFAULT (SYSUTCDATETIME())
    );
END;
GO

-- Chuyển đổi dữ liệu nhân viên hiện có sang các cột mới (tách LastName/FirstName, đồng bộ CurrentBaseSalary từ hợp đồng gần nhất).
UPDATE e
SET
    LastName = CASE
        WHEN LTRIM(RTRIM(e.LastName)) = N'' AND CHARINDEX(N' ', REVERSE(LTRIM(RTRIM(e.FullName)))) > 0
            THEN LTRIM(RTRIM(LEFT(LTRIM(RTRIM(e.FullName)), LEN(LTRIM(RTRIM(e.FullName))) - CHARINDEX(N' ', REVERSE(LTRIM(RTRIM(e.FullName)))))))
        ELSE e.LastName
    END,
    FirstName = CASE
        WHEN LTRIM(RTRIM(e.FirstName)) = N'' AND CHARINDEX(N' ', REVERSE(LTRIM(RTRIM(e.FullName)))) > 0
            THEN RIGHT(LTRIM(RTRIM(e.FullName)), CHARINDEX(N' ', REVERSE(LTRIM(RTRIM(e.FullName)))) - 1)
        WHEN LTRIM(RTRIM(e.FirstName)) = N''
            THEN LTRIM(RTRIM(e.FullName))
        ELSE e.FirstName
    END,
    Status = CASE
        WHEN LTRIM(RTRIM(e.Status)) = N'' AND e.IsActive = 0 THEN N'Đã nghỉ việc'
        WHEN LTRIM(RTRIM(e.Status)) = N'' THEN N'Đang làm việc'
        ELSE e.Status
    END,
    CurrentBaseSalary = CASE
        WHEN e.CurrentBaseSalary = 0 THEN ISNULL((
            SELECT TOP (1) c.BaseSalary
            FROM dbo.Contracts AS c
            WHERE c.EmployeeId = e.Id
            ORDER BY c.StartDate DESC, c.Id DESC
        ), 0)
        ELSE e.CurrentBaseSalary
    END
FROM dbo.Employees AS e;

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
