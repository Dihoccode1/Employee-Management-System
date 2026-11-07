USE HumanResourceManagement;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Mã tự tăng. Các quan hệ chỉ đi từ bảng con đến bảng cha.
IF OBJECT_ID(N'dbo.Projects', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Projects (
        Id INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        Deadline DATE NOT NULL,
        Description NVARCHAR(2000) NOT NULL,
        Budget DECIMAL(18,2) NOT NULL CHECK (Budget >= 0),
        DepartmentId INT NOT NULL REFERENCES dbo.Departments(Id),
        ManagerId INT NOT NULL REFERENCES dbo.Employees(Id),
        Status NVARCHAR(50) NOT NULL CHECK (LEN(LTRIM(RTRIM(Status))) > 0),
        CHECK (Deadline >= StartDate),
        CHECK (EndDate IS NULL OR EndDate >= StartDate)
    );
    CREATE INDEX IX_Projects_DepartmentId ON dbo.Projects(DepartmentId);
    CREATE INDEX IX_Projects_ManagerId ON dbo.Projects(ManagerId);
END;

IF OBJECT_ID(N'dbo.ProjectDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectDetails (
        Id INT IDENTITY PRIMARY KEY,
        ProjectId INT NOT NULL REFERENCES dbo.Projects(Id),
        CompletionPercentage DECIMAL(5,2) NOT NULL CHECK (CompletionPercentage BETWEEN 0 AND 100),
        Status NVARCHAR(50) NOT NULL CHECK (LEN(LTRIM(RTRIM(Status))) > 0),
        Allowance DECIMAL(18,2) NOT NULL CHECK (Allowance >= 0),
        Bonus DECIMAL(18,2) NOT NULL CHECK (Bonus >= 0)
    );
    CREATE INDEX IX_ProjectDetails_ProjectId ON dbo.ProjectDetails(ProjectId);
END;

IF OBJECT_ID(N'dbo.ProjectTasks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectTasks (
        Id INT IDENTITY PRIMARY KEY,
        ProjectDetailId INT NOT NULL REFERENCES dbo.ProjectDetails(Id),
        Name NVARCHAR(200) NOT NULL CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
        Description NVARCHAR(2000) NOT NULL,
        Type NVARCHAR(100) NOT NULL,
        Priority NVARCHAR(50) NOT NULL,
        StartDate DATE NOT NULL,
        Deadline DATE NOT NULL,
        CompletedDate DATE NULL,
        EstimatedHours DECIMAL(10,2) NOT NULL CHECK (EstimatedHours >= 0),
        CompletionPercentage DECIMAL(5,2) NOT NULL CHECK (CompletionPercentage BETWEEN 0 AND 100),
        Status NVARCHAR(50) NOT NULL CHECK (LEN(LTRIM(RTRIM(Status))) > 0),
        Notes NVARCHAR(2000) NOT NULL,
        CHECK (Deadline >= StartDate),
        CHECK (CompletedDate IS NULL OR CompletedDate >= StartDate)
    );
    CREATE INDEX IX_ProjectTasks_ProjectDetailId ON dbo.ProjectTasks(ProjectDetailId);
END;

IF OBJECT_ID(N'dbo.Assignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Assignments (
        Id INT IDENTITY PRIMARY KEY,
        TaskId INT NOT NULL REFERENCES dbo.ProjectTasks(Id),
        EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
        AssignedDate DATE NOT NULL,
        StartDate DATE NOT NULL,
        Deadline DATE NOT NULL,
        CompletedDate DATE NULL,
        CompletionPercentage DECIMAL(5,2) NOT NULL CHECK (CompletionPercentage BETWEEN 0 AND 100),
        Status NVARCHAR(50) NOT NULL CHECK (LEN(LTRIM(RTRIM(Status))) > 0),
        Notes NVARCHAR(2000) NOT NULL,
        CHECK (Deadline >= StartDate),
        CHECK (CompletedDate IS NULL OR CompletedDate >= StartDate)
    );
    CREATE INDEX IX_Assignments_TaskId ON dbo.Assignments(TaskId);
    CREATE INDEX IX_Assignments_EmployeeId ON dbo.Assignments(EmployeeId);
END;

-- Không dùng ON DELETE CASCADE: còn dữ liệu con thì SQL Server chặn xóa.
-- Tiến độ, trạng thái, phụ cấp và thưởng đều do người dùng nhập.
-- Không có cột điểm đánh giá đóng góp, không tự cập nhật bảng lương.
COMMIT TRANSACTION;
GO
