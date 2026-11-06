using Microsoft.Data.SqlClient;

namespace DAO.ProjectManagement;

internal static class ProjectDatabaseErrors
{
    public static InvalidOperationException createWriteError(SqlException exception)
    {
        string message = "Không thể lưu dữ liệu. Kiểm tra kết nối và script ProjectManagement.";
        if (exception.Number == 2601 || exception.Number == 2627)
            message = "Dữ liệu bị trùng: mỗi dự án chỉ có một chi tiết và mỗi nhân viên chỉ nhận một phân công cho cùng công việc.";
        else if (exception.Number == 547)
            message = "Dữ liệu liên kết không hợp lệ hoặc đang được sử dụng. Hãy kiểm tra dữ liệu con trước khi xóa.";
        else if (exception.Number >= 51000 && exception.Number <= 51002)
            message = exception.Message;

        return new InvalidOperationException(message, exception);
    }
}
