using System.Globalization;
using System.Text;
using BUS.Reporting;
using DTO.Statistics;

namespace GUI.Reports;

/// <summary>Hộp thoại xem trước tóm tắt báo cáo thống kê dự án và xuất ra tệp PDF [D7].</summary>
public sealed class ProjectReportPreviewDialog : Form
{
    private readonly ProjectStatisticsSummaryDto _summary;
    private readonly ProjectPdfReportService _pdf_service;

    public ProjectReportPreviewDialog(ProjectStatisticsSummaryDto summary, ProjectPdfReportService pdf_service)
    {
        _summary = summary;
        _pdf_service = pdf_service;

        Text = "Xem trước và xuất báo cáo dự án ra PDF";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(780, 560);
        MinimumSize = new Size(640, 440);
        Font = new Font("Segoe UI", 9.5f);

        var preview_box = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10f),
            Text = BuildPreviewText(_summary)
        };

        var bottom_panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 46,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8)
        };

        var close_button = new Button { Text = "Đóng", AutoSize = true };
        close_button.Click += (_, _) => Close();

        var export_button = new Button { Text = "Lưu tệp PDF...", AutoSize = true };
        export_button.Click += (_, _) => ExportPdf();

        bottom_panel.Controls.Add(close_button);
        bottom_panel.Controls.Add(export_button);

        Controls.Add(preview_box);
        Controls.Add(bottom_panel);
    }

    private void ExportPdf()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "PDF Document (*.pdf)|*.pdf",
            FileName = $"BaoCaoThongKeDuAn_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _pdf_service.ExportSummaryToPdf(dialog.FileName, _summary);
            MessageBox.Show(this, $"Đã xuất báo cáo PDF thành công:\n{dialog.FileName}", "Xuất PDF",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi xuất báo cáo PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string BuildPreviewText(ProjectStatisticsSummaryDto summary)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BÁO CÁO THỐNG KÊ DỰ ÁN THEO KỲ");
        sb.AppendLine($"Kỳ lọc: {summary.PeriodTitle}");
        sb.AppendLine($"Tiêu chí nhóm ngân sách: {summary.BudgetCriterionTitle}");
        sb.AppendLine($"Thời điểm lập: {summary.GeneratedAt:dd/MM/yyyy HH:mm}");
        sb.AppendLine(new string('=', 68));
        sb.AppendLine();

        AppendSection(sb, $"1. NGÂN SÁCH DỰ ÁN (theo {summary.BudgetCriterionTitle})", summary.BudgetRows, summary.TotalBudget);
        AppendSection(sb, "2. PHỤ CẤP DỰ ÁN (từ Chi tiết dự án)", summary.AllowanceRows, summary.TotalAllowance);
        AppendSection(sb, "3. THƯỞNG DỰ ÁN (từ Chi tiết dự án)", summary.BonusRows, summary.TotalBonus);
        return sb.ToString();
    }

    private static void AppendSection(StringBuilder sb, string title, IReadOnlyList<ProjectStatisticsRowDto> rows, decimal total)
    {
        sb.AppendLine(title);
        sb.AppendLine(new string('-', 68));
        if (rows.Count == 0)
        {
            sb.AppendLine("  (Không có dữ liệu)");
        }
        else
        {
            foreach (ProjectStatisticsRowDto row in rows)
                sb.AppendLine($"  - {row.GroupName,-36} | SL: {row.ItemCount,3} | {row.Amount.ToString("N0", CultureInfo.InvariantCulture),15} VNĐ");
        }
        sb.AppendLine($"  => TỔNG CỘNG: {total.ToString("N0", CultureInfo.InvariantCulture)} VNĐ");
        sb.AppendLine();
    }
}
