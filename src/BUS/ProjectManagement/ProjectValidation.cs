namespace BUS.ProjectManagement;

internal static class ProjectValidation
{
    public static string requiredText(string? value, string label, int maximumLength)
    {
        string result = (value ?? "").Trim();
        if (result.Length == 0)
            throw new ArgumentException(label + " không được để trống.");
        if (result.Length > maximumLength)
            throw new ArgumentException(label + " tối đa " + maximumLength + " ký tự.");
        return result;
    }

    public static string optionalText(string? value, string label, int maximumLength)
    {
        string result = (value ?? "").Trim();
        if (result.Length > maximumLength)
            throw new ArgumentException(label + " tối đa " + maximumLength + " ký tự.");
        return result;
    }

    public static void validId(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Hãy chọn một bản ghi trong danh sách.");
    }

    public static void dateRange(DateTime startDate, DateTime deadline, DateTime? completedDate)
    {
        if (deadline.Date < startDate.Date)
            throw new ArgumentException("Hạn hoàn thành phải từ ngày bắt đầu trở đi.");
        if (completedDate.HasValue && completedDate.Value.Date < startDate.Date)
            throw new ArgumentException("Ngày kết thúc/hoàn thành phải từ ngày bắt đầu trở đi.");
    }

    public static void amount(decimal value, decimal maximum, string label)
    {
        if (value < 0 || value > maximum)
            throw new ArgumentException(label + " phải từ 0 đến " + maximum + ".");
        if (decimal.Round(value, 2) != value)
            throw new ArgumentException(label + " chỉ được có tối đa 2 chữ số thập phân.");
    }

    public static void validStatus(string status)
    {
        if (status != "Not started" && status != "In progress" &&
            status != "Completed" && status != "Cancelled")
            throw new ArgumentException("Trạng thái không hợp lệ.");
    }
}
