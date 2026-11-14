using System;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DAO.HumanResources;

/// <summary>Hàm hỗ trợ tham số SQL, ánh xạ lỗi ràng buộc và bảo đảm cột ERD tương thích trên CSDL.</summary>
internal static class HrSqlParameterHelper
{
    private static readonly ConcurrentDictionary<string, bool> _verified_connections = new(StringComparer.Ordinal);

    public static void AddDecimal(SqlCommand command, string parameter_name, decimal value, byte precision = 18, byte scale = 2)
    {
        SqlParameter parameter = command.Parameters.Add(parameter_name, SqlDbType.Decimal);
        parameter.Precision = precision;
        parameter.Scale = scale;
        parameter.Value = value;
    }

    public static void AddNullableDecimal(SqlCommand command, string parameter_name, decimal? value, byte precision = 18, byte scale = 2)
    {
        SqlParameter parameter = command.Parameters.Add(parameter_name, SqlDbType.Decimal);
        parameter.Precision = precision;
        parameter.Scale = scale;
        parameter.Value = (object?)value ?? DBNull.Value;
    }

    public static Exception CreateWriteException(SqlException exception) => exception.Number switch
    {
        2601 or 2627 => new InvalidOperationException("Mã bản ghi đã tồn tại trong hệ thống. Vui lòng nhập mã khác.", exception),
        547 => new InvalidOperationException("Dữ liệu nhập vi phạm ràng buộc dữ liệu hoặc tham chiếu không tồn tại.", exception),
        _ => new InvalidOperationException("Không thể lưu dữ liệu vào cơ sở dữ liệu.", exception)
    };

    /// <summary>Đảm bảo các cột ERD nhóm Nhân sự tồn tại nếu CSDL đang dùng schema 01 cũ.</summary>
    public static async Task EnsureSchemaAsync(SqlConnection connection, CancellationToken cancellation_token = default)
    {
        string key = connection.ConnectionString;
        if (_verified_connections.ContainsKey(key))
            return;

        using var check_command = connection.CreateCommand();
        check_command.CommandText = """
            SELECT CASE
                WHEN COL_LENGTH(N'dbo.Departments', N'Description') IS NOT NULL
                 AND COL_LENGTH(N'dbo.Positions', N'AllowanceAmount') IS NOT NULL
                 AND COL_LENGTH(N'dbo.Employees', N'CurrentBaseSalary') IS NOT NULL
                 AND COL_LENGTH(N'dbo.Employees', N'Degree') IS NOT NULL
                 AND COL_LENGTH(N'dbo.Contracts', N'ContractType') IS NOT NULL
                 AND COL_LENGTH(N'dbo.Contracts', N'Status') IS NOT NULL
                THEN 1 ELSE 0
            END;
            """;

        int is_ready = Convert.ToInt32(await check_command.ExecuteScalarAsync(cancellation_token));
        if (is_ready == 0)
        {
            // Bổ sung từng cột còn thiếu trước khi thực hiện UPDATE đồng bộ dữ liệu cũ.
            using var alter_command = connection.CreateCommand();
            alter_command.CommandText = """
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
                """;
            await alter_command.ExecuteNonQueryAsync(cancellation_token);
        }

        _verified_connections[key] = true;
    }
}
