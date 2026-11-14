using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DAO.Infrastructure;
using DTO.HumanResources.Positions;
using Microsoft.Data.SqlClient;

namespace DAO.HumanResources.Positions;

public sealed class PositionDao(MyConnection connection_factory) : IPositionDao
{
    public async Task<List<PositionDto>> GetAllAsync(CancellationToken cancellation_token = default)
    {
        var items = new List<PositionDto>();
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Code, Name, Description, AllowanceAmount
            FROM dbo.Positions
            ORDER BY Code ASC;
            """;

        using var reader = await command.ExecuteReaderAsync(cancellation_token);
        int id_ordinal = reader.GetOrdinal("Id");
        int code_ordinal = reader.GetOrdinal("Code");
        int name_ordinal = reader.GetOrdinal("Name");
        int description_ordinal = reader.GetOrdinal("Description");
        int allowance_ordinal = reader.GetOrdinal("AllowanceAmount");

        while (await reader.ReadAsync(cancellation_token))
        {
            items.Add(new PositionDto
            {
                Id = reader.GetInt32(id_ordinal),
                Code = reader.GetString(code_ordinal),
                Name = reader.GetString(name_ordinal),
                Description = reader.IsDBNull(description_ordinal) ? string.Empty : reader.GetString(description_ordinal),
                AllowanceAmount = reader.IsDBNull(allowance_ordinal) ? 0m : reader.GetDecimal(allowance_ordinal)
            });
        }

        return items;
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM dbo.Positions WHERE Code = @Code;";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = code;
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellation_token)) > 0;
    }

    public async Task<int> InsertAsync(PositionDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.Positions (Code, Name, Description, AllowanceAmount)
            OUTPUT INSERTED.Id
            VALUES (@Code, @Name, @Description, @AllowanceAmount);
            """;
        AddParameters(command, item);

        try
        {
            int inserted_id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellation_token));
            item.Id = inserted_id;
            return inserted_id;
        }
        catch (SqlException exception)
        {
            throw HrSqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task UpdateAsync(PositionDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.Positions
            SET Name = @Name, Description = @Description, AllowanceAmount = @AllowanceAmount
            WHERE Code = @Code OR (@Id > 0 AND Id = @Id);
            """;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
        AddParameters(command, item);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellation_token) == 0)
                throw new InvalidOperationException("Mã chức vụ không tồn tại.");
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
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Positions WHERE Code = @Code;";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = code;

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellation_token) == 0)
                throw new InvalidOperationException("Mã chức vụ không tồn tại.");
        }
        catch (SqlException exception) when (exception.Number == 547)
        {
            throw new InvalidOperationException("Không thể xóa vì chức vụ đang được gán cho nhân viên.", exception);
        }
    }

    private static void AddParameters(SqlCommand command, PositionDto item)
    {
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = item.Code;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = item.Name;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = item.Description;
        HrSqlParameterHelper.AddDecimal(command, "@AllowanceAmount", item.AllowanceAmount);
    }
}
