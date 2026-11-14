using BUS;
using BUS.HumanResources;
using BUS.HumanResources.Employees;
using GUI.Controls;
using GUI.Forms.HumanResources.Contracts;
using GUI.Forms.HumanResources.Departments;
using GUI.Forms.HumanResources.Employees;
using GUI.Forms.HumanResources.Positions;
using GUI.Forms.ProjectManagement.Assignments;
using GUI.Forms.ProjectManagement.ProjectDetails;
using GUI.Forms.ProjectManagement.Projects;
using GUI.Forms.ProjectManagement.Tasks;
using GUI.Forms.SalaryInformation.Attendance;
using GUI.Forms.Settings;
using GUI.Forms.Statistics;

namespace GUI.Forms;

public partial class MainForm : Form
{
    private readonly Panel _dashboard_panel = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(36, 54, 71) };
    private readonly Panel _workspace_panel = new() { Dock = DockStyle.Fill, Visible = false };
    private readonly FlowLayoutPanel _left_menu_panel = new()
    {
        Dock = DockStyle.Left,
        Width = 235,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        BackColor = Color.FromArgb(32, 48, 64),
        Padding = new Padding(10)
    };
    private readonly Label _active_screen_label = new()
    {
        Dock = DockStyle.Fill,
        Font = new Font("Segoe UI", 11f, FontStyle.Bold),
        ForeColor = Color.White,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(14, 0, 0, 0)
    };
    private readonly Label _active_status_badge = new()
    {
        Dock = DockStyle.Right,
        Width = 240,
        Font = new Font("Segoe UI", 9.5f),
        ForeColor = Color.FromArgb(196, 224, 255),
        TextAlign = ContentAlignment.MiddleRight,
        Padding = new Padding(0, 0, 14, 0)
    };
    private readonly Panel _child_host_panel = new() { Dock = DockStyle.Fill, BackColor = SystemColors.Control };
    private readonly List<Button> _nav_buttons = [];
    private Form? _active_child_form;

    public MainForm(EmployeeService employee_service, string connection_string)
    {
        InitializeComponent();
        BuildShell(employee_service, connection_string);
    }

    private void BuildShell(EmployeeService employee_service, string connection_string)
    {
        List<ScreenModule> modules =
        [
            new("Danh sách nhân viên", "Danh sách\nnhân viên", "\uE8FD", Color.FromArgb(98, 202, 233),
                () => new EmployeeListForm(employee_service)),
            new("Hồ sơ nhân viên", "Hồ sơ\nnhân viên", "\uE77B", Color.FromArgb(113, 221, 171),
                () => new EmployeeForm(connection_string)),
            new("Phòng ban", "Phòng ban", "\uEC27", Color.FromArgb(148, 178, 255),
                () => new DepartmentForm(connection_string)),
            new("Chức vụ", "Chức vụ", "\uE8F9", Color.FromArgb(249, 199, 102),
                () => new PositionForm(connection_string)),
            new("Hợp đồng (Main–Sub)", "Hợp đồng\n(Main–Sub)", "\uE8A5", Color.FromArgb(208, 168, 247),
                () => new ContractForm(connection_string)),
            new("Tham số nghiệp vụ", "Tham số\nnghiệp vụ", "\uE713", Color.FromArgb(170, 214, 154),
                () => new SystemParameterForm(ServiceFactory.CreateSystemParameterService(connection_string))),
            new("Quản lý dự án", "Dự án", "\uE8B7", Color.FromArgb(246, 164, 116),
                () => new ProjectForm(
                    ServiceFactory.CreateProjectService(connection_string),
                    ServiceFactory.CreateProjectLookupService(connection_string),
                    ServiceFactory.CreateDepartmentService(connection_string),
                    employee_service)),
            new("Chi tiết dự án (Main–Sub)", "Chi tiết\ndự án", "\uE9F9", Color.FromArgb(116, 206, 238),
                () => new ProjectDetailForm(
                    ServiceFactory.CreateProjectDetailService(connection_string),
                    ServiceFactory.CreateProjectService(connection_string))),
            new("Công việc dự án", "Công việc", "\uE73A", Color.FromArgb(113, 221, 171),
                () => new ProjectTaskForm(
                    ServiceFactory.CreateProjectTaskService(connection_string),
                    ServiceFactory.CreateProjectDetailService(connection_string))),
            new("Phân công dự án", "Phân công", "\uE716", Color.FromArgb(240, 161, 209),
                () => new AssignmentForm(
                    ServiceFactory.CreateAssignmentService(connection_string),
                    ServiceFactory.CreateProjectTaskService(connection_string),
                    ServiceFactory.CreateProjectLookupService(connection_string),
                    employee_service)),
            new("Thống kê dự án & PDF", "Thống kê\n& Xuất PDF", "\uE9D2", Color.FromArgb(255, 193, 120),
                () => new ProjectStatisticsForm(
                    ServiceFactory.CreateProjectStatisticsService(connection_string),
                    ServiceFactory.CreateProjectPdfReportService())),
            new("Chấm công nhân sự", "Chấm công", "\uE823", Color.FromArgb(102, 224, 207),
                () => new AttendanceForm(
                    ServiceFactory.CreateAttendanceService(connection_string),
                    employee_service,
                    ServiceFactory.CreateSystemParameterService(connection_string)))
        ];

        // 1. Màn hình mở đầu dạng ô (Dashboard Tiles)
        var dash_layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.FromArgb(36, 54, 71)
        };
        dash_layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        dash_layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var tiles = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 2,
            Padding = new Padding(22, 24, 22, 24),
            Margin = Padding.Empty
        };
        for (int column = 0; column < tiles.ColumnCount; column++)
            tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / tiles.ColumnCount));
        for (int row = 0; row < tiles.RowCount; row++)
            tiles.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        for (int i = 0; i < modules.Count; i++)
        {
            ScreenModule mod = modules[i];
            int col = i % 6;
            int row = i / 6;
            var tile = new ManagementMenuTile(mod.TileLabel, mod.Glyph, mod.Accent);
            tile.Click += (_, _) => OpenModuleInWorkspace(mod, null);
            tiles.Controls.Add(tile, col, row);
        }

        dash_layout.Controls.Add(new ManagementMenuHeader(), 0, 0);
        dash_layout.Controls.Add(tiles, 0, 1);
        _dashboard_panel.Controls.Add(dash_layout);

        // 2. Màn hình quản lý với Menu trái + Vùng hiển thị form con [D8]
        var back_btn = CreateNavButton("← Màn hình chính", is_primary: true);
        back_btn.Click += (_, _) => ShowDashboard();
        _left_menu_panel.Controls.Add(back_btn);
        _left_menu_panel.Controls.Add(new Label
        {
            Text = "DANH MỤC CHỨC NĂNG",
            ForeColor = Color.FromArgb(148, 175, 199),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            AutoSize = false,
            Width = 205,
            Height = 26,
            TextAlign = ContentAlignment.BottomLeft,
            Margin = new Padding(4, 10, 0, 4)
        });

        foreach (ScreenModule mod in modules)
        {
            var nav_btn = CreateNavButton(mod.Title, is_primary: false);
            nav_btn.Tag = mod;
            nav_btn.Click += (_, _) => OpenModuleInWorkspace(mod, nav_btn);
            _nav_buttons.Add(nav_btn);
            _left_menu_panel.Controls.Add(nav_btn);
        }

        var banner_panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = Color.FromArgb(44, 68, 92)
        };
        banner_panel.Controls.Add(_active_screen_label);
        banner_panel.Controls.Add(_active_status_badge);

        var right_area = new Panel { Dock = DockStyle.Fill };
        right_area.Controls.Add(_child_host_panel);
        right_area.Controls.Add(banner_panel);

        _workspace_panel.Controls.Add(right_area);
        _workspace_panel.Controls.Add(_left_menu_panel);

        Controls.Add(_workspace_panel);
        Controls.Add(_dashboard_panel);
    }

    private void ShowDashboard()
    {
        DisposeActiveChildForm();
        _workspace_panel.Visible = false;
        _dashboard_panel.Visible = true;
    }

    private void OpenModuleInWorkspace(ScreenModule module, Button? clicked_button)
    {
        Button? target_btn = clicked_button ?? _nav_buttons.FirstOrDefault(b => ReferenceEquals(b.Tag, module));

        DisposeActiveChildForm();
        _dashboard_panel.Visible = false;
        _workspace_panel.Visible = true;

        Form child = module.CreateForm();
        child.TopLevel = false;
        child.FormBorderStyle = FormBorderStyle.None;
        child.Dock = DockStyle.Fill;

        _active_child_form = child;
        _child_host_panel.Controls.Add(child);
        child.Show();

        _active_screen_label.Text = module.Title;
        _active_status_badge.Text = $"▶ Đang mở: {module.Title}";
        HighlightNavButton(target_btn);
    }

    private void DisposeActiveChildForm()
    {
        if (_active_child_form is null)
            return;

        _child_host_panel.Controls.Clear();
        _active_child_form.Close();
        _active_child_form.Dispose();
        _active_child_form = null;
    }

    private void HighlightNavButton(Button? active_btn)
    {
        foreach (Button btn in _nav_buttons)
        {
            bool selected = ReferenceEquals(btn, active_btn);
            btn.BackColor = selected ? Color.FromArgb(62, 114, 164) : Color.FromArgb(42, 62, 82);
            btn.Font = new Font("Segoe UI", 9.5f, selected ? FontStyle.Bold : FontStyle.Regular);
        }
    }

    private static Button CreateNavButton(string text, bool is_primary)
    {
        return new Button
        {
            Text = text,
            Width = 208,
            Height = 36,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = is_primary ? Color.FromArgb(56, 128, 108) : Color.FromArgb(42, 62, 82),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 6, 0),
            Margin = new Padding(0, 2, 0, 2),
            Font = new Font("Segoe UI", 9.5f, is_primary ? FontStyle.Bold : FontStyle.Regular),
            Cursor = Cursors.Hand
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DisposeActiveChildForm();
        base.Dispose(disposing);
    }

    private sealed record ScreenModule(
        string Title,
        string TileLabel,
        string Glyph,
        Color Accent,
        Func<Form> CreateForm);
}
