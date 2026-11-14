using System.Globalization;
using BUS.Reporting;
using BUS.Statistics;
using DTO.Statistics;
using GUI.Reports;

namespace GUI.Forms.Statistics;

/// <summary>
/// Màn hình thống kê Ngân sách dự án [D5], Phụ cấp & Thưởng dự án theo kỳ [D6] và xuất báo cáo PDF [D7].
/// </summary>
public sealed class ProjectStatisticsForm : Form
{
    private readonly ProjectStatisticsService _statistics_service;
    private readonly ProjectPdfReportService _pdf_service;
    private ProjectStatisticsSummaryDto? _current_summary;

    private readonly ComboBox _period_type_box = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
    private readonly DateTimePicker _from_date_box = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 115 };
    private readonly DateTimePicker _to_date_box = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 115 };
    private readonly NumericUpDown _month_box = new() { Minimum = 1, Maximum = 12, Width = 58 };
    private readonly NumericUpDown _quarter_box = new() { Minimum = 1, Maximum = 4, Width = 58 };
    private readonly NumericUpDown _year_box = new() { Minimum = 1900, Maximum = 2200, Width = 72 };
    private readonly ComboBox _group_by_box = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 145 };

    private readonly ListView _budget_list = CreateGrid();
    private readonly ListView _allowance_list = CreateGrid();
    private readonly ListView _bonus_list = CreateGrid();
    private readonly Label _status_label = new() { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 0, 0) };

    public ProjectStatisticsForm(ProjectStatisticsService statistics_service, ProjectPdfReportService pdf_service)
    {
        _statistics_service = statistics_service;
        _pdf_service = pdf_service;

        Text = "Thống kê dự án theo kỳ & Xuất báo cáo PDF";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(960, 640);
        Font = new Font("Segoe UI", 9.5f);

        InitializeFilterDefaults();
        BuildLayout();
        Shown += async (_, _) => await LoadStatisticsAsync();
    }

    private void InitializeFilterDefaults()
    {
        _period_type_box.DisplayMember = nameof(OptionItem.Label);
        _period_type_box.ValueMember = nameof(OptionItem.Value);
        _period_type_box.DataSource = new List<OptionItem>
        {
            new("ALL", "Tất cả thời gian"),
            new("DATE_RANGE", "Từ ngày – Đến ngày"),
            new("MONTH", "Theo tháng"),
            new("QUARTER", "Theo quý"),
            new("YEAR", "Theo năm")
        };
        _period_type_box.SelectedIndexChanged += (_, _) => UpdateFilterInputStates();

        _group_by_box.DisplayMember = nameof(OptionItem.Label);
        _group_by_box.ValueMember = nameof(OptionItem.Value);
        _group_by_box.DataSource = new List<OptionItem>
        {
            new("DEPARTMENT", "Phòng ban"),
            new("MANAGER", "Người quản lý")
        };

        _from_date_box.Value = new DateTime(DateTime.Today.Year, 1, 1);
        _to_date_box.Value = DateTime.Today;
        _month_box.Value = DateTime.Today.Month;
        _quarter_box.Value = ((DateTime.Today.Month - 1) / 3) + 1;
        _year_box.Value = DateTime.Today.Year;
        UpdateFilterInputStates();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        // Khung bộ lọc kỳ và nút chức năng
        var filter_flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            WrapContents = true,
            AutoScroll = true
        };
        filter_flow.Controls.Add(new Label { Text = "Kỳ lọc:", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        filter_flow.Controls.Add(_period_type_box);
        filter_flow.Controls.Add(new Label { Text = "Từ ngày:", AutoSize = true, Padding = new Padding(8, 6, 4, 0) });
        filter_flow.Controls.Add(_from_date_box);
        filter_flow.Controls.Add(new Label { Text = "Đến ngày:", AutoSize = true, Padding = new Padding(8, 6, 4, 0) });
        filter_flow.Controls.Add(_to_date_box);
        filter_flow.Controls.Add(new Label { Text = "Tháng:", AutoSize = true, Padding = new Padding(8, 6, 4, 0) });
        filter_flow.Controls.Add(_month_box);
        filter_flow.Controls.Add(new Label { Text = "Quý:", AutoSize = true, Padding = new Padding(8, 6, 4, 0) });
        filter_flow.Controls.Add(_quarter_box);
        filter_flow.Controls.Add(new Label { Text = "Năm:", AutoSize = true, Padding = new Padding(8, 6, 4, 0) });
        filter_flow.Controls.Add(_year_box);
        filter_flow.Controls.Add(new Label { Text = "Nhóm ngân sách theo:", AutoSize = true, Padding = new Padding(10, 6, 4, 0) });
        filter_flow.Controls.Add(_group_by_box);

        var run_button = new Button { Text = "Thống kê", AutoSize = true, Margin = new Padding(10, 2, 4, 2) };
        run_button.Click += async (_, _) => await LoadStatisticsAsync();

        var export_button = new Button { Text = "Xuất báo cáo PDF", AutoSize = true, Margin = new Padding(4, 2, 4, 2) };
        export_button.Click += (_, _) => OpenPdfExportDialog();

        filter_flow.Controls.Add(run_button);
        filter_flow.Controls.Add(export_button);

        // Khu vực 3 bảng thống kê (D5 + D6)
        var tables_panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8, 0, 8, 4) };
        tables_panel.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
        tables_panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        tables_panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33));

        ConfigureColumns(_budget_list, "Kỳ lọc", "Tiêu chí nhóm", "Số dự án", "Tổng ngân sách (VNĐ)");
        ConfigureColumns(_allowance_list, "Kỳ lọc", "Dự án", "Số chi tiết", "Tổng phụ cấp (VNĐ)");
        ConfigureColumns(_bonus_list, "Kỳ lọc", "Dự án", "Số chi tiết", "Tổng tiền thưởng (VNĐ)");

        tables_panel.Controls.Add(WrapGroup("1. Thống kê tổng ngân sách dự án theo kỳ [D5]", _budget_list), 0, 0);
        tables_panel.Controls.Add(WrapGroup("2. Thống kê phụ cấp dự án từ Chi tiết dự án [D6]", _allowance_list), 0, 1);
        tables_panel.Controls.Add(WrapGroup("3. Thống kê tiền thưởng dự án từ Chi tiết dự án [D6]", _bonus_list), 0, 2);

        root.Controls.Add(filter_flow, 0, 0);
        root.Controls.Add(tables_panel, 0, 1);
        root.Controls.Add(_status_label, 0, 2);
        Controls.Add(root);
    }

    private void UpdateFilterInputStates()
    {
        string period = _period_type_box.SelectedValue?.ToString() ?? "ALL";
        _from_date_box.Enabled = period == "DATE_RANGE";
        _to_date_box.Enabled = period == "DATE_RANGE";
        _month_box.Enabled = period == "MONTH";
        _quarter_box.Enabled = period == "QUARTER";
        _year_box.Enabled = period is "MONTH" or "QUARTER" or "YEAR";
    }

    private async Task LoadStatisticsAsync()
    {
        UseWaitCursor = true;
        try
        {
            var filter = new ProjectStatisticsFilterDto
            {
                PeriodType = _period_type_box.SelectedValue?.ToString() ?? "ALL",
                FromDate = _from_date_box.Value.Date,
                ToDate = _to_date_box.Value.Date,
                Month = (int)_month_box.Value,
                Quarter = (int)_quarter_box.Value,
                Year = (int)_year_box.Value,
                BudgetGroupBy = _group_by_box.SelectedValue?.ToString() ?? "DEPARTMENT"
            };

            ProjectStatisticsSummaryDto summary = await _statistics_service.GetSummaryAsync(filter);
            _current_summary = summary;

            BindTable(_budget_list, summary.BudgetRows, summary.TotalBudget, summary.BudgetCriterionTitle);
            BindTable(_allowance_list, summary.AllowanceRows, summary.TotalAllowance, "Dự án");
            BindTable(_bonus_list, summary.BonusRows, summary.TotalBonus, "Dự án");

            _status_label.Text = $"Kỳ: {summary.PeriodTitle} | Nhóm ngân sách: {summary.BudgetCriterionTitle} | " +
                $"Tổng ngân sách: {summary.TotalBudget:N0} VNĐ | Phụ cấp: {summary.TotalAllowance:N0} VNĐ | Thưởng: {summary.TotalBonus:N0} VNĐ";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Kiểm tra kỳ lọc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi thống kê dự án", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void OpenPdfExportDialog()
    {
        if (_current_summary is null)
        {
            MessageBox.Show(this, "Vui lòng nhấn Thống kê để nạp số liệu trước khi xuất PDF.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var preview = new ProjectReportPreviewDialog(_current_summary, _pdf_service);
        preview.ShowDialog(this);
    }

    private static void BindTable(ListView list_view, IReadOnlyList<ProjectStatisticsRowDto> rows, decimal total_amount, string criterion_header)
    {
        list_view.BeginUpdate();
        try
        {
            list_view.Columns[1].Text = criterion_header;
            list_view.Items.Clear();

            int total_count = 0;
            foreach (ProjectStatisticsRowDto row in rows)
            {
                total_count += row.ItemCount;
                list_view.Items.Add(new ListViewItem([
                    row.PeriodName,
                    row.GroupName,
                    row.ItemCount.ToString("N0", CultureInfo.InvariantCulture),
                    row.Amount.ToString("N0", CultureInfo.InvariantCulture)
                ]));
            }

            // Dòng tổng cộng ở cuối bảng khớp 100% với báo cáo PDF.
            var total_row = new ListViewItem([
                rows.Count > 0 ? rows[0].PeriodName : "",
                "TỔNG CỘNG",
                total_count.ToString("N0", CultureInfo.InvariantCulture),
                total_amount.ToString("N0", CultureInfo.InvariantCulture)
            ])
            {
                Font = new Font(list_view.Font, FontStyle.Bold),
                BackColor = Color.FromArgb(236, 243, 248)
            };
            list_view.Items.Add(total_row);
        }
        finally
        {
            list_view.EndUpdate();
        }
    }

    private static ListView CreateGrid() => new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        GridLines = true
    };

    private static void ConfigureColumns(ListView grid, string col1, string col2, string col3, string col4)
    {
        grid.Columns.Add(col1, 220);
        grid.Columns.Add(col2, 430);
        grid.Columns.Add(col3, 120, HorizontalAlignment.Right);
        grid.Columns.Add(col4, 240, HorizontalAlignment.Right);
    }

    private static GroupBox WrapGroup(string title, Control child)
    {
        var box = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(6) };
        box.Controls.Add(child);
        return box;
    }

    private sealed record OptionItem(string Value, string Label);
}
