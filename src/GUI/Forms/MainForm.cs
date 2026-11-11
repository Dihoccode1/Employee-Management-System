using BUS;
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

namespace GUI.Forms;

public partial class MainForm : Form
{
    public MainForm(EmployeeService employee_service, string connection_string)
    {
        InitializeComponent();
        BuildMenu(employee_service, connection_string);
    }

    private void BuildMenu(EmployeeService employee_service, string connection_string)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.FromArgb(36, 54, 71)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var tiles = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 2,
            Padding = new Padding(26, 24, 26, 24),
            Margin = Padding.Empty
        };
        for (int column = 0; column < tiles.ColumnCount; column++)
            tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        for (int row = 0; row < tiles.RowCount; row++)
            tiles.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        AddTile(tiles, 0, 0, "Danh sách\nnhân viên", "\uE8FD", Color.FromArgb(98, 202, 233),
            () => new EmployeeListForm(employee_service));
        AddTile(tiles, 1, 0, "Hồ sơ\nnhân viên", "\uE77B", Color.FromArgb(113, 221, 171),
            () => new EmployeeForm(connection_string));
        AddTile(tiles, 2, 0, "Phòng ban", "\uEC27", Color.FromArgb(148, 178, 255),
            () => new DepartmentForm(connection_string));
        AddTile(tiles, 3, 0, "Chức vụ", "\uE8F9", Color.FromArgb(249, 199, 102),
            () => new PositionForm(connection_string));
        AddTile(tiles, 4, 0, "Hợp đồng", "\uE8A5", Color.FromArgb(208, 168, 247),
            () => new ContractForm(connection_string));
        AddTile(tiles, 0, 1, "Dự án", "\uE8B7", Color.FromArgb(246, 164, 116),
            () => new ProjectForm(ServiceFactory.CreateProjectService(connection_string),
                ServiceFactory.CreateProjectLookupService(connection_string)));
        AddTile(tiles, 1, 1, "Chi tiết\ndự án", "\uE9F9", Color.FromArgb(116, 206, 238),
            () => new ProjectDetailForm(ServiceFactory.CreateProjectDetailService(connection_string),
                ServiceFactory.CreateProjectService(connection_string)));
        AddTile(tiles, 2, 1, "Công việc", "\uE73A", Color.FromArgb(113, 221, 171),
            () => new ProjectTaskForm(ServiceFactory.CreateProjectTaskService(connection_string),
                ServiceFactory.CreateProjectDetailService(connection_string)));
        AddTile(tiles, 3, 1, "Phân công", "\uE716", Color.FromArgb(240, 161, 209),
            () => new AssignmentForm(ServiceFactory.CreateAssignmentService(connection_string),
                ServiceFactory.CreateProjectTaskService(connection_string),
                ServiceFactory.CreateProjectLookupService(connection_string)));
        AddTile(tiles, 4, 1, "Chấm công", "\uE823", Color.FromArgb(102, 224, 207),
            () => new AttendanceForm(ServiceFactory.CreateAttendanceService(connection_string), employee_service));

        layout.Controls.Add(new ManagementMenuHeader(), 0, 0);
        layout.Controls.Add(tiles, 0, 1);
        Controls.Add(layout);
    }

    private void AddTile(TableLayoutPanel menu, int column, int row, string label,
        string glyph, Color accent, Func<Form> create_form)
    {
        var tile = new ManagementMenuTile(label, glyph, accent);
        tile.Click += (_, _) =>
        {
            using var form = create_form();
            form.ShowDialog(this);
        };
        menu.Controls.Add(tile, column, row);
    }
}
