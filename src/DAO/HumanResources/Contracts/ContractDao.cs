using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DAO.Infrastructure;
using DTO.HumanResources.Contracts;
using Microsoft.Data.SqlClient;

namespace DAO.HumanResources.Contracts;

public sealed class ContractDao(MyConnection connection_factory) : IContractDao
{
    public async Task<List<ContractDto>> GetAllAsync(CancellationToken cancellation_token = default)
    {
        var items = new List<ContractDto>();
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT c.Id,
                   c.ContractCode,
                   c.EmployeeId,
                   e.EmployeeCode,
                   e.FullName AS EmployeeName,
                   c.ContractType,
                   c.StartDate,
                   c.EndDate,
                   c.BaseSalary,
                   c.Status
            FROM dbo.Contracts AS c
            JOIN dbo.Employees AS e ON e.Id = c.EmployeeId
            ORDER BY c.ContractCode ASC;
            """;

        using var reader = await command.ExecuteReaderAsync(cancellation_token);
        int id_ordinal = reader.GetOrdinal("Id");
        int code_ordinal = reader.GetOrdinal("ContractCode");
        int emp_id_ordinal = reader.GetOrdinal("EmployeeId");
        int emp_code_ordinal = reader.GetOrdinal("EmployeeCode");
        int emp_name_ordinal = reader.GetOrdinal("EmployeeName");
        int type_ordinal = reader.GetOrdinal("ContractType");
        int start_ordinal = reader.GetOrdinal("StartDate");
        int end_ordinal = reader.GetOrdinal("EndDate");
        int salary_ordinal = reader.GetOrdinal("BaseSalary");
        int status_ordinal = reader.GetOrdinal("Status");

        while (await reader.ReadAsync(cancellation_token))
        {
            items.Add(new ContractDto
            {
                Id = reader.GetInt32(id_ordinal),
                ContractCode = reader.GetString(code_ordinal),
                EmployeeId = reader.GetInt32(emp_id_ordinal),
                EmployeeCode = reader.GetString(emp_code_ordinal),
                EmployeeName = reader.GetString(emp_name_ordinal),
                ContractType = reader.IsDBNull(type_ordinal) ? "Xác định thời hạn" : reader.GetString(type_ordinal),
                StartDate = reader.GetDateTime(start_ordinal),
                EndDate = reader.IsDBNull(end_ordinal) ? null : reader.GetDateTime(end_ordinal),
                BaseSalary = reader.GetDecimal(salary_ordinal),
                Status = reader.IsDBNull(status_ordinal) ? "Còn hiệu lực" : reader.GetString(status_ordinal)
            });
        }

        return items;
    }

    public async Task<bool> ExistsAsync(string contract_code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM dbo.Contracts WHERE ContractCode = @ContractCode;";
        command.Parameters.Add("@ContractCode", SqlDbType.VarChar, 30).Value = contract_code;
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellation_token)) > 0;
    }

    public async Task<int> InsertAsync(ContractDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @EmpId INT = NULL;
            IF @EmployeeId > 0
                SELECT TOP (1) @EmpId = Id FROM dbo.Employees WHERE Id = @EmployeeId;
            ELSE
                SELECT TOP (1) @EmpId = Id FROM dbo.Employees WHERE EmployeeCode = @EmployeeCode;

            IF @EmpId IS NULL
                THROW 50001, N'Nhân viên không tồn tại trong hệ thống.', 1;

            INSERT INTO dbo.Contracts (EmployeeId, ContractCode, ContractType, StartDate, EndDate, BaseSalary, Status)
            OUTPUT INSERTED.Id
            VALUES (@EmpId, @ContractCode, @ContractType, @StartDate, @EndDate, @BaseSalary, @Status);

            -- Đồng bộ lương cơ bản hiện hành trên hồ sơ nhân viên khi hợp đồng còn hiệu lực.
            IF @Status = N'Còn hiệu lực'
                UPDATE dbo.Employees SET CurrentBaseSalary = @BaseSalary WHERE Id = @EmpId;
            """;
        AddParameters(command, item);

        try
        {
            int inserted_id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellation_token));
            item.Id = inserted_id;
            return inserted_id;
        }
        catch (SqlException exception) when (exception.Number == 50001)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
        catch (SqlException exception)
        {
            throw HrSqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task UpdateAsync(ContractDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @EmpId INT = NULL;
            IF @EmployeeId > 0
                SELECT TOP (1) @EmpId = Id FROM dbo.Employees WHERE Id = @EmployeeId;
            ELSE
                SELECT TOP (1) @EmpId = Id FROM dbo.Employees WHERE EmployeeCode = @EmployeeCode;

            IF @EmpId IS NULL
                THROW 50001, N'Nhân viên không tồn tại trong hệ thống.', 1;

            UPDATE dbo.Contracts
            SET EmployeeId = @EmpId,
                ContractType = @ContractType,
                StartDate = @StartDate,
                EndDate = @EndDate,
                BaseSalary = @BaseSalary,
                Status = @Status
            WHERE (@Id > 0 AND Id = @Id) OR ContractCode = @ContractCode;

            IF @@ROWCOUNT = 0
                THROW 50002, N'Mã hợp đồng không tồn tại.', 1;

            IF @Status = N'Còn hiệu lực'
                UPDATE dbo.Employees SET CurrentBaseSalary = @BaseSalary WHERE Id = @EmpId;
            """;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
        AddParameters(command, item);

        try
        {
            await command.ExecuteNonQueryAsync(cancellation_token);
        }
        catch (SqlException exception) when (exception.Number is 50001 or 50002)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
        catch (SqlException exception)
        {
            throw HrSqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task DeleteAsync(string contract_code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Contracts WHERE ContractCode = @ContractCode;";
        command.Parameters.Add("@ContractCode", SqlDbType.VarChar, 30).Value = contract_code;

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellation_token) == 0)
                throw new InvalidOperationException("Mã hợp đồng không tồn tại.");
        }
        catch (SqlException exception) when (exception.Number == 547)
        {
            throw new InvalidOperationException("Không thể xóa hợp đồng vì còn dữ liệu liên quan.", exception);
        }
    }

    private static void AddParameters(SqlCommand command, ContractDto item)
    {
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = item.EmployeeId;
        command.Parameters.Add("@ContractCode", SqlDbType.VarChar, 30).Value = item.ContractCode;
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 20).Value = item.EmployeeCode;
        command.Parameters.Add("@ContractType", SqlDbType.NVarChar, 100).Value = item.ContractType;
        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = item.StartDate.Date;
        command.Parameters.Add("@EndDate", SqlDbType.Date).Value = (object?)item.EndDate?.Date ?? DBNull.Value;
        HrSqlParameterHelper.AddDecimal(command, "@BaseSalary", item.BaseSalary);
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = item.Status;
    }
}
