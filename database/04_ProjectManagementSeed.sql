USE HumanResourceManagement;
GO
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @departmentId INT = (SELECT TOP (1) Id FROM dbo.Departments ORDER BY Id);
    DECLARE @employeeId INT = (SELECT TOP (1) Id FROM dbo.Employees ORDER BY Id);
    IF @departmentId IS NULL OR @employeeId IS NULL
        THROW 51003, N'Cần có phòng ban và nhân viên. Hãy chạy 02_SeedData.sql trước.', 1;

    DECLARE @projectId INT = (SELECT TOP (1) id FROM dbo.Projects WHERE name = N'Dự án quản lý nhân sự mẫu' ORDER BY id);
    IF @projectId IS NULL
    BEGIN
        INSERT dbo.Projects (name, startDate, endDate, deadline, description, budget, departmentId, managerId, status)
        VALUES (N'Dự án quản lý nhân sự mẫu', '20261101', NULL, '20261231',
                N'Dữ liệu mẫu để thực hành 3 lớp.', 10000000, @departmentId, @employeeId, N'Not started');
        SET @projectId = CONVERT(INT, SCOPE_IDENTITY());
    END;

    DECLARE @detailId INT = (SELECT id FROM dbo.ProjectDetails WHERE projectId = @projectId);
    IF @detailId IS NULL
    BEGIN
        INSERT dbo.ProjectDetails (projectId, completionRate, status, allowance, reward)
        VALUES (@projectId, 0, N'Not started', 500000, 1000000);
        SET @detailId = CONVERT(INT, SCOPE_IDENTITY());
    END;

    DECLARE @taskId INT = (SELECT TOP (1) id FROM dbo.ProjectTasks WHERE projectDetailId = @detailId AND name = N'Thiết kế giao diện' ORDER BY id);
    IF @taskId IS NULL
    BEGIN
        INSERT dbo.ProjectTasks (projectDetailId, name, description, taskType, priority, startDate,
            deadline, completedDate, estimatedHours, completionRate, status, notes)
        VALUES (@detailId, N'Thiết kế giao diện', N'Làm form WinForms đơn giản.', N'GUI', N'Normal',
            '20261102', '20261130', NULL, 24, 0, N'Not started', N'');
        SET @taskId = CONVERT(INT, SCOPE_IDENTITY());
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Assignments WHERE taskId = @taskId AND employeeId = @employeeId)
        INSERT dbo.Assignments (taskId, projectId, employeeId, assignedDate, startDate,
            deadline, completedDate, completionRate, contributionScore, status, notes)
        VALUES (@taskId, @projectId, @employeeId, '20261101', '20261102',
            '20261130', NULL, 0, 0, N'Not started', N'Phân công mẫu.');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
