IF DB_ID(N'HumanResourceManagement') IS NULL
    CREATE DATABASE HumanResourceManagement;
GO
USE HumanResourceManagement;
GO
CREATE TABLE dbo.Departments (
    Id INT IDENTITY PRIMARY KEY,
    Code VARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL
);
CREATE TABLE dbo.Positions (
    Id INT IDENTITY PRIMARY KEY,
    Code VARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL
);
CREATE TABLE dbo.Employees (
    Id INT IDENTITY PRIMARY KEY,
    EmployeeCode VARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(254) NULL,
    DepartmentId INT NULL REFERENCES dbo.Departments(Id),
    PositionId INT NULL REFERENCES dbo.Positions(Id),
    HireDate DATE NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
CREATE TABLE dbo.Contracts (
    Id INT IDENTITY PRIMARY KEY,
    EmployeeId INT NOT NULL REFERENCES dbo.Employees(Id),
    ContractCode VARCHAR(30) NOT NULL UNIQUE,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    BaseSalary DECIMAL(18,2) NOT NULL CHECK (BaseSalary >= 0),
    CHECK (EndDate IS NULL OR EndDate >= StartDate)
);
GO
