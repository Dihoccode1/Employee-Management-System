USE HumanResourceManagement;
GO
INSERT INTO dbo.Departments (Code, Name) VALUES ('HR', N'Nhân sự');
INSERT INTO dbo.Positions (Code, Name) VALUES ('STAFF', N'Nhân viên');
INSERT INTO dbo.Employees (EmployeeCode, FullName, DepartmentId, PositionId, HireDate)
SELECT 'NV001', N'Nguyễn Văn An', d.Id, p.Id, '2026-01-01'
FROM dbo.Departments d CROSS JOIN dbo.Positions p
WHERE d.Code = 'HR' AND p.Code = 'STAFF';
GO