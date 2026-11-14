IF DB_ID(N'HumanResourceManagement') IS NULL
    CREATE DATABASE HumanResourceManagement;
GO
USE HumanResourceManagement;
GO

-- Tạo các bảng nhóm Nhân sự với khóa tự tăng Id (PK) và mã nghiệp vụ UNIQUE
-- để giữ toàn vẹn khóa ngoại cho Hợp đồng, Dự án, Phân công và Chấm công.
IF OBJECT_ID(N'dbo.Departments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Departments (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Code VARCHAR(20) NOT NULL CONSTRAINT UQ_Departments_Code UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL CONSTRAINT DF_Departments_Description DEFAULT (N'')
    );
END;

IF OBJECT_ID(N'dbo.Positions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Positions (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Code VARCHAR(20) NOT NULL CONSTRAINT UQ_Positions_Code UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL CONSTRAINT DF_Positions_Description DEFAULT (N''),
        AllowanceAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Positions_AllowanceAmount DEFAULT (0)
            CONSTRAINT CK_Positions_AllowanceAmount CHECK (AllowanceAmount >= 0)
    );
END;

IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmployeeCode VARCHAR(20) NOT NULL CONSTRAINT UQ_Employees_EmployeeCode UNIQUE,
        LastName NVARCHAR(100) NOT NULL CONSTRAINT DF_Employees_LastName DEFAULT (N''),
        FirstName NVARCHAR(50) NOT NULL CONSTRAINT DF_Employees_FirstName DEFAULT (N''),
        FullName NVARCHAR(150) NOT NULL,
        DateOfBirth DATE NOT NULL CONSTRAINT DF_Employees_DateOfBirth DEFAULT ('2000-01-01'),
        Gender NVARCHAR(20) NOT NULL CONSTRAINT DF_Employees_Gender DEFAULT (N'Nam'),
        PhoneNumber VARCHAR(20) NOT NULL CONSTRAINT DF_Employees_PhoneNumber DEFAULT (''),
        Email NVARCHAR(254) NULL,
        DepartmentId INT NULL REFERENCES dbo.Departments(Id),
        PositionId INT NULL REFERENCES dbo.Positions(Id),
        HireDate DATE NOT NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Employees_Status DEFAULT (N'Đang làm việc'),
        IsActive BIT NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT (1),
        CurrentBaseSalary DECIMAL(18,2) NOT NULL CONSTRAINT DF_Employees_CurrentBaseSalary DEFAULT (0)
            CONSTRAINT CK_Employees_CurrentBaseSalary CHECK (CurrentBaseSalary >= 0),
        Degree NVARCHAR(100) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END;

IF OBJECT_ID(N'dbo.Contracts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Contracts (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
        ContractCode VARCHAR(30) NOT NULL CONSTRAINT UQ_Contracts_ContractCode UNIQUE,
        ContractType NVARCHAR(100) NOT NULL CONSTRAINT DF_Contracts_ContractType DEFAULT (N'Xác định thời hạn'),
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        BaseSalary DECIMAL(18,2) NOT NULL CONSTRAINT CK_Contracts_BaseSalary CHECK (BaseSalary >= 0),
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Contracts_Status DEFAULT (N'Còn hiệu lực'),
        CONSTRAINT CK_Contracts_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate)
    );
END;

-- Bảng tham số nghiệp vụ dùng chung toàn hệ thống.
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

-- Nâng cấp tự động các cột ERD nếu CSDL đã được tạo từ phiên bản script 01 cũ.
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
GO

-- Đồng bộ dữ liệu chuyển đổi cho các bản ghi đã tồn tại (tách họ/tên từ FullName, trạng thái từ IsActive).
UPDATE dbo.Employees
SET
    LastName = CASE
        WHEN LTRIM(RTRIM(LastName)) = N'' AND CHARINDEX(N' ', REVERSE(LTRIM(RTRIM(FullName)))) > 0
            THEN LTRIM(RTRIM(LEFT(LTRIM(RTRIM(FullName)), LEN(LTRIM(RTRIM(FullName))) - CHARINDEX(N' ', REVERSE(LTRIM(RTRIM(FullName)))))))
        ELSE LastName
    END,
    FirstName = CASE
        WHEN LTRIM(RTRIM(FirstName)) = N'' AND CHARINDEX(N' ', REVERSE(LTRIM(RTRIM(FullName)))) > 0
            THEN RIGHT(LTRIM(RTRIM(FullName)), CHARINDEX(N' ', REVERSE(LTRIM(RTRIM(FullName)))) - 1)
        WHEN LTRIM(RTRIM(FirstName)) = N''
            THEN LTRIM(RTRIM(FullName))
        ELSE FirstName
    END,
    Status = CASE
        WHEN LTRIM(RTRIM(Status)) = N'' AND IsActive = 0 THEN N'Đã nghỉ việc'
        WHEN LTRIM(RTRIM(Status)) = N'' THEN N'Đang làm việc'
        ELSE Status
    END
WHERE LTRIM(RTRIM(FullName)) <> N'';
GO
