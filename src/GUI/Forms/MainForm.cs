using BUS.HumanResources.Employees;
using BUS;
using GUI.Forms.ProjectManagement.Projects;
using GUI.Forms.ProjectManagement.ProjectDetails;
using GUI.Forms.ProjectManagement.Tasks;
using GUI.Forms.ProjectManagement.Assignments;
namespace GUI.Forms;
public partial class MainForm : Form
{
    private readonly EmployeeService _employeeService;
    public MainForm(EmployeeService employeeService, string connectionString)
    {
        _employeeService = employeeService;
        InitializeComponent();
        AddProjectManagementMenu(connectionString);
    }

    private void AddProjectManagementMenu(string connectionString)
    {
        var menu = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(6) };
        AddMenuButton(menu, "Dự án", () => new ProjectForm(
            ServiceFactory.CreateProjectService(connectionString),
            ServiceFactory.CreateProjectLookupService(connectionString)));
        AddMenuButton(menu, "Chi tiết dự án", () => new ProjectDetailForm(
            ServiceFactory.CreateProjectDetailService(connectionString),
            ServiceFactory.CreateProjectService(connectionString)));
        AddMenuButton(menu, "Công việc", () => new ProjectTaskForm(
            ServiceFactory.CreateProjectTaskService(connectionString),
            ServiceFactory.CreateProjectDetailService(connectionString)));
        AddMenuButton(menu, "Phân công", () => new AssignmentForm(
            ServiceFactory.CreateAssignmentService(connectionString),
            ServiceFactory.CreateProjectTaskService(connectionString),
            ServiceFactory.CreateProjectLookupService(connectionString)));
        Controls.Add(menu);
    }

    private void AddMenuButton(FlowLayoutPanel menu, string text, Func<Form> createForm)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += (_, _) =>
        {
            using var form = createForm();
            form.ShowDialog(this);
        };
        menu.Controls.Add(button);
    }
    private async void LoadEmployees_Click(object? sender, EventArgs e)
    {
        loadButton.Enabled = false;
        try { employeesGrid.DataSource = (await _employeeService.GetAllAsync()).ToList(); }
        catch (Exception)
        {
            MessageBox.Show("Không thể tải dữ liệu. Kiểm tra kết nối và script SQL Server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { loadButton.Enabled = true; }
    }
}
