USE HumanResourceManagement;
GO
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.Projects', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Projects (
            id INT IDENTITY(1,1) CONSTRAINT PK_Projects PRIMARY KEY,
            name NVARCHAR(150) NOT NULL,
            startDate DATE NOT NULL,
            endDate DATE NULL,
            deadline DATE NOT NULL,
            description NVARCHAR(1000) NOT NULL,
            budget DECIMAL(18,2) NOT NULL,
            departmentId INT NOT NULL,
            managerId INT NOT NULL,
            status NVARCHAR(30) NOT NULL,
            CONSTRAINT FK_Projects_Departments FOREIGN KEY (departmentId) REFERENCES dbo.Departments(Id),
            CONSTRAINT FK_Projects_Managers FOREIGN KEY (managerId) REFERENCES dbo.Employees(Id),
            CONSTRAINT CK_Projects_Dates CHECK (deadline >= startDate AND (endDate IS NULL OR endDate >= startDate)),
            CONSTRAINT CK_Projects_Budget CHECK (budget >= 0),
            CONSTRAINT CK_Projects_Status CHECK (status IN (N'Not started', N'In progress', N'Completed', N'Cancelled'))
        );
    END;
    IF OBJECT_ID(N'dbo.ProjectDetails', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProjectDetails (
            id INT IDENTITY(1,1) CONSTRAINT PK_ProjectDetails PRIMARY KEY,
            projectId INT NOT NULL,
            completionRate DECIMAL(5,2) NOT NULL,
            status NVARCHAR(30) NOT NULL,
            allowance DECIMAL(18,2) NOT NULL,
            reward DECIMAL(18,2) NOT NULL,
            CONSTRAINT UQ_ProjectDetails_Project UNIQUE (projectId),
            CONSTRAINT FK_ProjectDetails_Projects FOREIGN KEY (projectId) REFERENCES dbo.Projects(id),
            CONSTRAINT CK_ProjectDetails_Rate CHECK (completionRate BETWEEN 0 AND 100),
            CONSTRAINT CK_ProjectDetails_Money CHECK (allowance >= 0 AND reward >= 0),
            CONSTRAINT CK_ProjectDetails_Status CHECK (status IN (N'Not started', N'In progress', N'Completed', N'Cancelled'))
        );
    END;
    IF OBJECT_ID(N'dbo.ProjectTasks', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProjectTasks (
            id INT IDENTITY(1,1) CONSTRAINT PK_ProjectTasks PRIMARY KEY,
            projectDetailId INT NOT NULL,
            name NVARCHAR(150) NOT NULL,
            description NVARCHAR(1000) NOT NULL,
            taskType NVARCHAR(100) NOT NULL,
            priority NVARCHAR(20) NOT NULL,
            startDate DATE NOT NULL,
            deadline DATE NOT NULL,
            completedDate DATE NULL,
            estimatedHours DECIMAL(10,2) NOT NULL,
            completionRate DECIMAL(5,2) NOT NULL,
            status NVARCHAR(30) NOT NULL,
            notes NVARCHAR(1000) NOT NULL,
            CONSTRAINT FK_ProjectTasks_ProjectDetails FOREIGN KEY (projectDetailId) REFERENCES dbo.ProjectDetails(id),
            CONSTRAINT CK_ProjectTasks_Dates CHECK (deadline >= startDate AND (completedDate IS NULL OR completedDate >= startDate)),
            CONSTRAINT CK_ProjectTasks_Rate CHECK (completionRate BETWEEN 0 AND 100),
            CONSTRAINT CK_ProjectTasks_Hours CHECK (estimatedHours >= 0),
            CONSTRAINT CK_ProjectTasks_Priority CHECK (priority IN (N'Low', N'Normal', N'High')),
            CONSTRAINT CK_ProjectTasks_Status CHECK (status IN (N'Not started', N'In progress', N'Completed', N'Cancelled'))
        );
    END;
    IF OBJECT_ID(N'dbo.Assignments', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Assignments (
            id INT IDENTITY(1,1) CONSTRAINT PK_Assignments PRIMARY KEY,
            taskId INT NOT NULL,
            projectId INT NOT NULL,
            employeeId INT NOT NULL,
            assignedDate DATE NOT NULL,
            startDate DATE NOT NULL,
            deadline DATE NOT NULL,
            completedDate DATE NULL,
            completionRate DECIMAL(5,2) NOT NULL,
            contributionScore DECIMAL(10,2) NOT NULL,
            status NVARCHAR(30) NOT NULL,
            notes NVARCHAR(1000) NOT NULL,
            CONSTRAINT FK_Assignments_Tasks FOREIGN KEY (taskId) REFERENCES dbo.ProjectTasks(id),
            CONSTRAINT FK_Assignments_Projects FOREIGN KEY (projectId) REFERENCES dbo.Projects(id),
            CONSTRAINT FK_Assignments_Employees FOREIGN KEY (employeeId) REFERENCES dbo.Employees(Id),
            CONSTRAINT UQ_Assignments_TaskEmployee UNIQUE (taskId, employeeId),
            CONSTRAINT CK_Assignments_Dates CHECK (assignedDate <= startDate AND deadline >= startDate AND (completedDate IS NULL OR completedDate >= startDate)),
            CONSTRAINT CK_Assignments_Rate CHECK (completionRate BETWEEN 0 AND 100),
            CONSTRAINT CK_Assignments_Score CHECK (contributionScore >= 0),
            CONSTRAINT CK_Assignments_Status CHECK (status IN (N'Not started', N'In progress', N'Completed', N'Cancelled'))
        );
    END;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- projectId in Assignments must match the project of its task.
-- These checks also protect data written outside the application.
CREATE OR ALTER TRIGGER dbo.TR_Assignments_CheckProject
ON dbo.Assignments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted a
        JOIN dbo.ProjectTasks t ON t.id = a.taskId
        JOIN dbo.ProjectDetails d ON d.id = t.projectDetailId
        WHERE a.projectId <> d.projectId
    )
        THROW 51000, N'Công việc không thuộc dự án đã chọn.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_ProjectTasks_CheckAssignments
ON dbo.ProjectTasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted t
        JOIN dbo.ProjectDetails d ON d.id = t.projectDetailId
        JOIN dbo.Assignments a ON a.taskId = t.id
        WHERE a.projectId <> d.projectId
    )
        THROW 51001, N'Không thể chuyển công việc sang dự án khác khi đã có phân công. Hãy xóa phân công liên quan trước.', 1;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_ProjectDetails_CheckAssignments
ON dbo.ProjectDetails
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted d
        JOIN dbo.ProjectTasks t ON t.projectDetailId = d.id
        JOIN dbo.Assignments a ON a.taskId = t.id
        WHERE a.projectId <> d.projectId
    )
        THROW 51002, N'Không thể chuyển chi tiết sang dự án khác khi đã có phân công. Hãy xóa phân công liên quan trước.', 1;
END;
GO
