using Microsoft.Data.SqlClient;
using System.Data;

namespace DAO.Infrastructure;

internal static class SqlParameterHelper
{
    public static void AddDecimal(SqlCommand command, string name, decimal value, byte precision)
    {
        var parameter = command.Parameters.Add(name, SqlDbType.Decimal);
        parameter.Precision = precision;
        parameter.Scale = 2;
        parameter.Value = value;
    }

    public static InvalidOperationException CreateWriteException(SqlException exception)
    {
        string message = exception.Number switch
        {
            547 => "Dữ liệu liên quan không còn tồn tại hoặc dữ liệu nhập vi phạm ràng buộc. Vui lòng tải lại và kiểm tra.",
            2601 or 2627 => "Dữ liệu bị trùng. Vui lòng kiểm tra lại.",
            _ => "Không thể lưu dữ liệu. Vui lòng kiểm tra kết nối SQL Server."
        };
        return new InvalidOperationException(message, exception);
    }
}
