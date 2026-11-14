using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DAO.Infrastructure;
using DTO.HumanResources.Employees;
using Microsoft.Data.SqlClient;

namespace DAO.HumanResources.Employees;

public sealed class EmployeeDao(MyConnection connection_factory) : IEmployeeDao
{
    public async Task<List<EmployeeDto>> GetAllAsync(CancellationToken cancellation_token = default)
    {
        var items = new List<EmployeeDto>();
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT e.Id,
                   e.EmployeeCode,
                   e.LastName,
                   e.FirstName,
                   e.FullName,
                   e.DateOfBirth,
                   e.Gender,
                   e.PhoneNumber,
                   e.Email,
                   e.DepartmentId,
                   ISNULL(d.Code, '') AS DepartmentCode,
                   ISNULL(d.Name, N'') AS DepartmentName,
                   e.PositionId,
                   ISNULL(p.Code, '') AS PositionCode,
                   ISNULL(p.Name, N'') AS PositionName,
                   e.HireDate,
                   e.Status,
                   e.IsActive,
                   e.CurrentBaseSalary,
                   e.Degree
            FROM dbo.Employees AS e
            LEFT JOIN dbo.Departments AS d ON d.Id = e.DepartmentId
            LEFT JOIN dbo.Positions AS p ON p.Id = e.PositionId
            ORDER BY e.EmployeeCode ASC;
            """;

        using var reader = await command.ExecuteReaderAsync(cancellation_token);
        int id_ordinal = reader.GetOrdinal("Id");
        int code_ordinal = reader.GetOrdinal("EmployeeCode");
        int last_name_ordinal = reader.GetOrdinal("LastName");
        int first_name_ordinal = reader.GetOrdinal("FirstName");
        int full_name_ordinal = reader.GetOrdinal("FullName");
        int dob_ordinal = reader.GetOrdinal("DateOfBirth");
        int gender_ordinal = reader.GetOrdinal("Gender");
        int phone_ordinal = reader.GetOrdinal("PhoneNumber");
        int email_ordinal = reader.GetOrdinal("Email");
        int dept_id_ordinal = reader.GetOrdinal("DepartmentId");
        int dept_code_ordinal = reader.GetOrdinal("DepartmentCode");
        int dept_name_ordinal = reader.GetOrdinal("DepartmentName");
        int pos_id_ordinal = reader.GetOrdinal("PositionId");
        int pos_code_ordinal = reader.GetOrdinal("PositionCode");
        int pos_name_ordinal = reader.GetOrdinal("PositionName");
        int hire_date_ordinal = reader.GetOrdinal("HireDate");
        int status_ordinal = reader.GetOrdinal("Status");
        int active_ordinal = reader.GetOrdinal("IsActive");
        int salary_ordinal = reader.GetOrdinal("CurrentBaseSalary");
        int degree_ordinal = reader.GetOrdinal("Degree");

        while (await reader.ReadAsync(cancellation_token))
        {
            var employee = new EmployeeDto
            {
                Id = reader.GetInt32(id_ordinal),
                EmployeeCode = reader.GetString(code_ordinal),
                LastName = reader.IsDBNull(last_name_ordinal) ? string.Empty : reader.GetString(last_name_ordinal),
                FirstName = reader.IsDBNull(first_name_ordinal) ? string.Empty : reader.GetString(first_name_ordinal),
                FullName = reader.GetString(full_name_ordinal),
                DateOfBirth = reader.IsDBNull(dob_ordinal) ? new DateTime(2000, 1, 1) : reader.GetDateTime(dob_ordinal),
                Gender = reader.IsDBNull(gender_ordinal) ? "Nam" : reader.GetString(gender_ordinal),
                PhoneNumber = reader.IsDBNull(phone_ordinal) ? string.Empty : reader.GetString(phone_ordinal),
                Email = reader.IsDBNull(email_ordinal) ? null : reader.GetString(email_ordinal),
                DepartmentId = reader.IsDBNull(dept_id_ordinal) ? null : reader.GetInt32(dept_id_ordinal),
                DepartmentCode = reader.GetString(dept_code_ordinal),
                DepartmentName = reader.GetString(dept_name_ordinal),
                PositionId = reader.IsDBNull(pos_id_ordinal) ? null : reader.GetInt32(pos_id_ordinal),
                PositionCode = reader.GetString(pos_code_ordinal),
                PositionName = reader.GetString(pos_name_ordinal),
                HireDate = reader.GetDateTime(hire_date_ordinal),
                Status = reader.IsDBNull(status_ordinal) ? "Đang làm việc" : reader.GetString(status_ordinal),
                IsActive = reader.GetBoolean(active_ordinal),
                CurrentBaseSalary = reader.IsDBNull(salary_ordinal) ? 0m : reader.GetDecimal(salary_ordinal),
                Degree = reader.IsDBNull(degree_ordinal) ? null : reader.GetString(degree_ordinal)
            };

            // Tách Họ/Tên tự động nếu bản ghi cũ trong DB mới chỉ lưu FullName.
            employee.SynchronizeNameFields();
            items.Add(employee);
        }

        return items;
    }

    public async Task<bool> ExistsAsync(string employee_code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM dbo.Employees WHERE EmployeeCode = @EmployeeCode;";
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 20).Value = employee_code;
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellation_token)) > 0;
    }

    public async Task<int> InsertAsync(EmployeeDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            -- Quy đổi từ mã nghiệp vụ sang khóa Id để giữ liên kết khóa ngoại ổn định.
            DECLARE @DeptId INT = NULL;
            IF @DepartmentId IS NOT NULL AND @DepartmentId > 0
                SELECT TOP (1) @DeptId = Id FROM dbo.Departments WHERE Id = @DepartmentId;
            ELSE IF LTRIM(RTRIM(@DepartmentCode)) <> ''
                SELECT TOP (1) @DeptId = Id FROM dbo.Departments WHERE Code = @DepartmentCode;

            IF (@DepartmentId IS NOT NULL AND @DepartmentId > 0 AND @DeptId IS NULL)
               OR (LTRIM(RTRIM(@DepartmentCode)) <> '' AND @DeptId IS NULL)
                THROW 50011, N'Mã phòng ban không tồn tại trong hệ thống.', 1;

            DECLARE @PosId INT = NULL;
            IF @PositionId IS NOT NULL AND @PositionId > 0
                SELECT TOP (1) @PosId = Id FROM dbo.Positions WHERE Id = @PositionId;
            ELSE IF LTRIM(RTRIM(@PositionCode)) <> ''
                SELECT TOP (1) @PosId = Id FROM dbo.Positions WHERE Code = @PositionCode;

            IF (@PositionId IS NOT NULL AND @PositionId > 0 AND @PosId IS NULL)
               OR (LTRIM(RTRIM(@PositionCode)) <> '' AND @PosId IS NULL)
                THROW 50012, N'Mã chức vụ không tồn tại trong hệ thống.', 1;

            INSERT INTO dbo.Employees
                (EmployeeCode, LastName, FirstName, FullName, DateOfBirth, Gender, PhoneNumber, Email,
                 DepartmentId, PositionId, HireDate, Status, IsActive, CurrentBaseSalary, Degree)
            OUTPUT INSERTED.Id
            VALUES
                (@EmployeeCode, @LastName, @FirstName, @FullName, @DateOfBirth, @Gender, @PhoneNumber, @Email,
                 @DeptId, @PosId, @HireDate, @Status, @IsActive, @CurrentBaseSalary, @Degree);
            """;
        AddParameters(command, item);

        try
        {
            int inserted_id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellation_token));
            item.Id = inserted_id;
            return inserted_id;
        }
        catch (SqlException exception) when (exception.Number is 50011 or 50012)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
        catch (SqlException exception)
        {
            throw HrSqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task UpdateAsync(EmployeeDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @DeptId INT = NULL;
            IF @DepartmentId IS NOT NULL AND @DepartmentId > 0
                SELECT TOP (1) @DeptId = Id FROM dbo.Departments WHERE Id = @DepartmentId;
            ELSE IF LTRIM(RTRIM(@DepartmentCode)) <> ''
                SELECT TOP (1) @DeptId = Id FROM dbo.Departments WHERE Code = @DepartmentCode;

            IF (@DepartmentId IS NOT NULL AND @DepartmentId > 0 AND @DeptId IS NULL)
               OR (LTRIM(RTRIM(@DepartmentCode)) <> '' AND @DeptId IS NULL)
                THROW 50011, N'Mã phòng ban không tồn tại trong hệ thống.', 1;

            DECLARE @PosId INT = NULL;
            IF @PositionId IS NOT NULL AND @PositionId > 0
                SELECT TOP (1) @PosId = Id FROM dbo.Positions WHERE Id = @PositionId;
            ELSE IF LTRIM(RTRIM(@PositionCode)) <> ''
                SELECT TOP (1) @PosId = Id FROM dbo.Positions WHERE Code = @PositionCode;

            IF (@PositionId IS NOT NULL AND @PositionId > 0 AND @PosId IS NULL)
               OR (LTRIM(RTRIM(@PositionCode)) <> '' AND @PosId IS NULL)
                THROW 50012, N'Mã chức vụ không tồn tại trong hệ thống.', 1;

            UPDATE dbo.Employees
            SET LastName = @LastName,
                FirstName = @FirstName,
                FullName = @FullName,
                DateOfBirth = @DateOfBirth,
                Gender = @Gender,
                PhoneNumber = @PhoneNumber,
                Email = @Email,
                DepartmentId = @DeptId,
                PositionId = @PosId,
                HireDate = @HireDate,
                Status = @Status,
                IsActive = @IsActive,
                CurrentBaseSalary = @CurrentBaseSalary,
                Degree = @Degree
            WHERE (@Id > 0 AND Id = @Id) OR EmployeeCode = @EmployeeCode;
            """;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
        AddParameters(command, item);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellation_token) == 0)
                throw new InvalidOperationException("Không tìm thấy hồ sơ nhân viên cần cập nhật.");
        }
        catch (SqlException exception) when (exception.Number is 50011 or 50012)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
        catch (SqlException exception)
        {
            throw HrSqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task DeleteAsync(string employee_code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Employees WHERE EmployeeCode = @EmployeeCode;";
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 20).Value = employee_code;

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellation_token) == 0)
                throw new InvalidOperationException("Mã nhân viên không tồn tại.");
        }
        catch (SqlException exception) when (exception.Number == 547)
        {
            throw new InvalidOperationException(
                "Không thể xóa nhân viên vì đang có dữ liệu Hợp đồng, Dự án, Phân công hoặc Chấm công liên quan.",
                exception);
        }
    }

    private static void AddParameters(SqlCommand command, EmployeeDto item)
    {
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 20).Value = item.EmployeeCode;
        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = item.LastName;
        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = item.FirstName;
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = item.FullName;
        command.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value = item.DateOfBirth.Date;
        command.Parameters.Add("@Gender", SqlDbType.NVarChar, 20).Value = item.Gender;
        command.Parameters.Add("@PhoneNumber", SqlDbType.VarChar, 20).Value = item.PhoneNumber;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = (object?)item.Email ?? DBNull.Value;
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = (object?)item.DepartmentId ?? DBNull.Value;
        command.Parameters.Add("@DepartmentCode", SqlDbType.VarChar, 20).Value = item.DepartmentCode ?? string.Empty;
        command.Parameters.Add("@PositionId", SqlDbType.Int).Value = (object?)item.PositionId ?? DBNull.Value;
        command.Parameters.Add("@PositionCode", SqlDbType.VarChar, 20).Value = item.PositionCode ?? string.Empty;
        command.Parameters.Add("@HireDate", SqlDbType.Date).Value = item.HireDate.Date;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = item.Status;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
        HrSqlParameterHelper.AddDecimal(command, "@CurrentBaseSalary", item.CurrentBaseSalary);
        command.Parameters.Add("@Degree", SqlDbType.NVarChar, 100).Value = (object?)item.Degree ?? DBNull.Value;
    }
}
