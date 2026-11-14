USE HumanResourceManagement;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @DepartmentId INT = (SELECT TOP (1) Id FROM dbo.Departments ORDER BY Id);
DECLARE @EmployeeId INT = (SELECT TOP (1) Id FROM dbo.Employees ORDER BY Id);
IF @DepartmentId IS NULL OR @EmployeeId IS NULL
    THROW 50001, N'Cần có phòng ban và nhân viên trước khi tạo dữ liệu dự án mẫu.', 1;

-- Dữ liệu minh họa, chỉ thêm khi chưa có dự án mẫu này.
IF NOT EXISTS (SELECT 1 FROM dbo.Projects WHERE Name = N'Dự án mẫu - Quản lý nhân sự')
BEGIN
    INSERT INTO dbo.Projects
        (Name, StartDate, EndDate, Deadline, Description, Budget, DepartmentId, ManagerId, Status)
    VALUES
        (N'Dự án mẫu - Quản lý nhân sự', '2026-11-01', NULL, '2026-12-31',
         N'Dữ liệu minh họa cho bốn màn hình quản lý dự án.', 10000000, @DepartmentId, @EmployeeId, N'Đang thực hiện');
    DECLARE @ProjectId INT = CONVERT(INT, SCOPE_IDENTITY());

    INSERT INTO dbo.ProjectDetails (ProjectId, CompletionPercentage, Status, Allowance, Bonus)
    VALUES (@ProjectId, 20, N'Đang thực hiện', 500000, 1000000);
    DECLARE @DetailId INT = CONVERT(INT, SCOPE_IDENTITY());

    INSERT INTO dbo.ProjectTasks
        (ProjectDetailId, Name, Description, Type, Priority, StartDate, Deadline, CompletedDate,
         EstimatedHours, CompletionPercentage, Status, Notes)
    VALUES
        (@DetailId, N'Xây dựng màn hình dự án', N'Thêm, sửa, xóa và tìm kiếm.',
         N'Lập trình', N'Cao', '2026-11-01', '2026-11-30', NULL, 16, 25, N'Đang thực hiện', N'');
    DECLARE @TaskId INT = CONVERT(INT, SCOPE_IDENTITY());

    INSERT INTO dbo.Assignments
        (TaskId, EmployeeId, AssignedDate, StartDate, Deadline, CompletedDate, CompletionPercentage, Status, Notes)
    VALUES
        (@TaskId, @EmployeeId, '2026-11-01', '2026-11-01', '2026-11-30', NULL, 30, N'Đang thực hiện', N'Phân công minh họa.');
END;
COMMIT TRANSACTION;
GO
