USE HumanResourceManagement;
GO
SET XACT_ABORT ON;

IF DB_NAME() <> N'HumanResourceManagement'
    THROW 50000, N'Cần chọn database HumanResourceManagement.', 1;

IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL OR OBJECT_ID(N'dbo.Projects', N'U') IS NULL
    THROW 50001, N'Cần chạy 01_CreateDatabase.sql và 03_ProjectManagement.sql trước.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Nhận diện hai bảng khung từ script 01 cũ. Dừng nếu chúng đã có dữ liệu.
    IF COL_LENGTH(N'dbo.Payroll', N'Deduction') IS NOT NULL
        EXEC(N'
            IF EXISTS (SELECT 1 FROM dbo.Payroll WITH (TABLOCKX, HOLDLOCK))
                THROW 50002, N''Bảng Payroll cũ có dữ liệu. Cần chuyển dữ liệu trước khi nâng cấp.'', 1;
            DROP TABLE dbo.Payroll;
        ');

    IF COL_LENGTH(N'dbo.Attendance', N'WorkHours') IS NOT NULL
        EXEC(N'
            IF EXISTS (SELECT 1 FROM dbo.Attendance WITH (TABLOCKX, HOLDLOCK))
                THROW 50003, N''Bảng Attendance cũ có dữ liệu. Cần chuyển dữ liệu trước khi nâng cấp.'', 1;
            DROP TABLE dbo.Attendance;
        ');

    -- Bảng trạng thái chấm công.
    IF OBJECT_ID(N'dbo.AttendanceStatuses', N'U') IS NULL
        CREATE TABLE dbo.AttendanceStatuses (
            Id INT IDENTITY PRIMARY KEY,
            AttendanceStatus NVARCHAR(50) NOT NULL,
            Description NVARCHAR(500) NOT NULL
        );

    -- Bảng chấm công.
    IF OBJECT_ID(N'dbo.Attendance', N'U') IS NULL
        CREATE TABLE dbo.Attendance (
            Id INT IDENTITY PRIMARY KEY,
            EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
            AttendanceStatusId INT NOT NULL REFERENCES dbo.AttendanceStatuses(Id),
            WorkDate DATE NOT NULL,
            CheckIn TIME(0) NOT NULL,
            CheckOut TIME(0) NOT NULL,
            AttendanceStatus NVARCHAR(50) NOT NULL,
            UNIQUE (EmployeeId, WorkDate)
        );

    -- Thông tin chấm công tháng.
    IF OBJECT_ID(N'dbo.MonthlyAttendance', N'U') IS NULL
        CREATE TABLE dbo.MonthlyAttendance (
            Id INT IDENTITY PRIMARY KEY,
            EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
            [Month] INT NOT NULL CHECK ([Month] BETWEEN 1 AND 12),
            [Year] INT NOT NULL,
            StandardWorkDays DECIMAL(5,2) NOT NULL,
            ActualWorkDays DECIMAL(5,2) NOT NULL,
            PaidLeaveDays DECIMAL(5,2) NOT NULL,
            UnpaidLeaveDays DECIMAL(5,2) NOT NULL,
            ApprovedLeaveDays DECIMAL(5,2) NOT NULL,
            UnapprovedLeaveDays DECIMAL(5,2) NOT NULL,
            OvertimeHours DECIMAL(7,2) NOT NULL,
            PayrollWorkDays DECIMAL(5,2) NOT NULL,
            UNIQUE (EmployeeId, [Month], [Year])
        );

    -- Quy định phụ cấp thâm niên.
    IF OBJECT_ID(N'dbo.SeniorityAllowanceRules', N'U') IS NULL
        CREATE TABLE dbo.SeniorityAllowanceRules (
            Id INT IDENTITY PRIMARY KEY,
            Name NVARCHAR(100) NOT NULL,
            Description NVARCHAR(500) NOT NULL,
            MinimumWorkDays DECIMAL(5,2) NOT NULL,
            EligibilityConditions NVARCHAR(500) NOT NULL,
            AllowanceAmount DECIMAL(18,2) NOT NULL
        );

    -- Nhân viên phụ cấp thâm niên.
    IF OBJECT_ID(N'dbo.EmployeeSeniorityAllowances', N'U') IS NULL
        CREATE TABLE dbo.EmployeeSeniorityAllowances (
            Id INT IDENTITY PRIMARY KEY,
            EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
            AllowanceRuleId INT NOT NULL REFERENCES dbo.SeniorityAllowanceRules(Id),
            ProjectId INT NULL REFERENCES dbo.Projects(Id),
            AllowanceAmount DECIMAL(18,2) NOT NULL,
            StartDate DATE NOT NULL,
            EndDate DATE NULL,
            Reason NVARCHAR(500) NOT NULL,
            PaymentStatus NVARCHAR(50) NOT NULL
        );

    -- Quy định thưởng chung.
    IF OBJECT_ID(N'dbo.RewardRules', N'U') IS NULL
        CREATE TABLE dbo.RewardRules (
            Id INT IDENTITY PRIMARY KEY,
            Name NVARCHAR(100) NOT NULL,
            Description NVARCHAR(500) NOT NULL,
            MinimumWorkDays DECIMAL(5,2) NOT NULL,
            RewardConditions NVARCHAR(500) NOT NULL,
            RewardAmount DECIMAL(18,2) NOT NULL,
            EffectiveDate DATE NOT NULL
        );

    -- Nhân viên khen thưởng.
    IF OBJECT_ID(N'dbo.EmployeeRewards', N'U') IS NULL
        CREATE TABLE dbo.EmployeeRewards (
            Id INT IDENTITY PRIMARY KEY,
            EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
            RewardRuleId INT NOT NULL REFERENCES dbo.RewardRules(Id),
            ProjectId INT NULL REFERENCES dbo.Projects(Id),
            RewardDate DATE NOT NULL,
            Reason NVARCHAR(500) NOT NULL,
            RewardAmount DECIMAL(18,2) NOT NULL,
            PaymentStatus NVARCHAR(50) NOT NULL
        );

    -- Bảng lương. ma_phu_cap xuất hiện hai lần trên sơ đồ, dùng một AllowanceId.
    -- ma_cham_cong nối tới Thông tin chấm công tháng theo đường nối 1:1 trên ERD.
    IF OBJECT_ID(N'dbo.Payroll', N'U') IS NULL
        CREATE TABLE dbo.Payroll (
            Id INT IDENTITY PRIMARY KEY,
            EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
            AllowanceId INT NOT NULL REFERENCES dbo.EmployeeSeniorityAllowances(Id),
            MonthlyAttendanceId INT NOT NULL UNIQUE REFERENCES dbo.MonthlyAttendance(Id),
            SalaryMonth INT NOT NULL CHECK (SalaryMonth BETWEEN 1 AND 12),
            SalaryYear INT NOT NULL,
            SalaryQuarter INT NOT NULL CHECK (SalaryQuarter BETWEEN 1 AND 4),
            WorkDays DECIMAL(5,2) NOT NULL,
            HolidayBonus DECIMAL(18,2) NOT NULL,
            ProjectBonus DECIMAL(18,2) NOT NULL,
            PositionAllowance DECIMAL(18,2) NOT NULL,
            SeniorityAllowance DECIMAL(18,2) NOT NULL,
            ProjectAllowance DECIMAL(18,2) NOT NULL,
            Penalty DECIMAL(18,2) NOT NULL,
            BaseSalary DECIMAL(18,2) NOT NULL,
            NetSalary DECIMAL(18,2) NOT NULL,
            Status NVARCHAR(50) NOT NULL,
            UNIQUE (EmployeeId, SalaryMonth, SalaryYear)
        );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
