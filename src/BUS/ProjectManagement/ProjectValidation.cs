namespace BUS.ProjectManagement;

// Các kiểm tra chung; không tự tính tiền, tiến độ hoặc trạng thái.
internal static class ProjectValidation
{
    public const decimal MaximumMoney = 9999999999999999.99m;

    public static string RequiredText(string? value, string label, int maximumLength)
    {
        string text = (value ?? "").Trim();
        if (text.Length == 0)
            throw new ArgumentException($"Vui lòng nhập {label}.");
        if (text.Length > maximumLength)
            throw new ArgumentException($"{label} không được quá {maximumLength} ký tự.");
        return text;
    }

    public static string OptionalText(string? value, string label, int maximumLength)
    {
        string text = (value ?? "").Trim();
        if (text.Length > maximumLength)
            throw new ArgumentException($"{label} không được quá {maximumLength} ký tự.");
        return text;
    }

    public static void PositiveId(int id, string label)
    {
        if (id <= 0)
            throw new ArgumentException($"Vui lòng chọn {label}.");
    }

    public static void RecordId(int id)
    {
        if (id < 0)
            throw new ArgumentException("Mã bản ghi không hợp lệ.");
    }

    public static void Number(decimal value, string label, decimal maximum)
    {
        if (value < 0 || value > maximum)
            throw new ArgumentException($"{label} phải từ 0 đến {maximum:N2}.");
        if (decimal.Round(value, 2) != value)
            throw new ArgumentException($"{label} chỉ được có tối đa 2 chữ số thập phân.");
    }

    public static void Dates(DateTime startDate, DateTime deadline, DateTime? completedDate)
    {
        if (deadline.Date < startDate.Date)
            throw new ArgumentException("Hạn hoàn thành không được trước ngày bắt đầu.");
        if (completedDate.HasValue && completedDate.Value.Date < startDate.Date)
            throw new ArgumentException("Ngày hoàn thành không được trước ngày bắt đầu.");
        // Cho phép hoàn thành sau hạn để lưu đúng thực tế.
    }
}
