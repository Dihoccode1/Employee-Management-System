using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DAO.HumanResources;
using DAO.Infrastructure;
using DTO.Settings;
using Microsoft.Data.SqlClient;

namespace DAO.Settings;

/// <summary>Truy xuất bảng tham số nghiệp vụ dbo.SystemParameters qua MyConnection và SqlParameter.</summary>
public sealed class SystemParameterDao(MyConnection connection_factory) : ISystemParameterDao
{
    private static readonly ConcurrentDictionary<string, bool> _initialized_connections = new(StringComparer.Ordinal);

    public async Task<List<SystemParameterDto>> GetAllAsync(CancellationToken cancellation_token = default)
    {
        var items = new List<SystemParameterDto>();
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await EnsureTableAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Code, Name, Value, ValueType, MinValue, MaxValue, Description, UpdatedAt
            FROM dbo.SystemParameters
            ORDER BY Code ASC;
            """;

        using var reader = await command.ExecuteReaderAsync(cancellation_token);
        while (await reader.ReadAsync(cancellation_token))
            items.Add(MapReader(reader));

        return items;
    }

    public async Task<SystemParameterDto?> GetByCodeAsync(string code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await EnsureTableAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Code, Name, Value, ValueType, MinValue, MaxValue, Description, UpdatedAt
            FROM dbo.SystemParameters
            WHERE Code = @Code;
            """;
        command.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = code;

        using var reader = await command.ExecuteReaderAsync(cancellation_token);
        if (await reader.ReadAsync(cancellation_token))
            return MapReader(reader);

        return null;
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await EnsureTableAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM dbo.SystemParameters WHERE Code = @Code;";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = code;
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellation_token)) > 0;
    }

    public async Task InsertAsync(SystemParameterDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await EnsureTableAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.SystemParameters (Code, Name, Value, ValueType, MinValue, MaxValue, Description, UpdatedAt)
            VALUES (@Code, @Name, @Value, @ValueType, @MinValue, @MaxValue, @Description, SYSUTCDATETIME());
            """;
        AddParameters(command, item);

        try
        {
            await command.ExecuteNonQueryAsync(cancellation_token);
        }
        catch (SqlException exception)
        {
            throw HrSqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task UpdateAsync(SystemParameterDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await EnsureTableAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.SystemParameters
            SET Name = @Name,
                Value = @Value,
                ValueType = @ValueType,
                MinValue = @MinValue,
                MaxValue = @MaxValue,
                Description = @Description,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Code = @Code;
            """;
        AddParameters(command, item);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellation_token) == 0)
                throw new InvalidOperationException("Mã tham số nghiệp vụ không tồn tại.");
        }
        catch (SqlException exception)
        {
            throw HrSqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task DeleteAsync(string code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await EnsureTableAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.SystemParameters WHERE Code = @Code;";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = code;

        if (await command.ExecuteNonQueryAsync(cancellation_token) == 0)
            throw new InvalidOperationException("Mã tham số nghiệp vụ không tồn tại.");
    }

    private static SystemParameterDto MapReader(SqlDataReader reader) => new()
    {
        Code = reader.GetString(reader.GetOrdinal("Code")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Value = reader.GetString(reader.GetOrdinal("Value")),
        ValueType = reader.GetString(reader.GetOrdinal("ValueType")),
        MinValue = reader.IsDBNull(reader.GetOrdinal("MinValue")) ? null : reader.GetDecimal(reader.GetOrdinal("MinValue")),
        MaxValue = reader.IsDBNull(reader.GetOrdinal("MaxValue")) ? null : reader.GetDecimal(reader.GetOrdinal("MaxValue")),
        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? string.Empty : reader.GetString(reader.GetOrdinal("Description")),
        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
    };

    private static void AddParameters(SqlCommand command, SystemParameterDto item)
    {
        command.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = item.Code;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = item.Name;
        command.Parameters.Add("@Value", SqlDbType.NVarChar, 200).Value = item.Value;
        command.Parameters.Add("@ValueType", SqlDbType.VarChar, 20).Value = item.ValueType;
        HrSqlParameterHelper.AddNullableDecimal(command, "@MinValue", item.MinValue);
        HrSqlParameterHelper.AddNullableDecimal(command, "@MaxValue", item.MaxValue);
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = item.Description;
    }

    private static async Task EnsureTableAsync(SqlConnection connection, CancellationToken cancellation_token)
    {
        string key = connection.ConnectionString;
        if (_initialized_connections.ContainsKey(key))
            return;

        using var command = connection.CreateCommand();
        command.CommandText = """
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
            """;
        await command.ExecuteNonQueryAsync(cancellation_token);
        _initialized_connections[key] = true;
    }
}
