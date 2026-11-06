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
    private readonly string connectionString;

    public MainForm(EmployeeService employeeService, string connectionString)
    {
        _employeeService = employeeService;
        this.connectionString = connectionString;
        InitializeComponent();
    }

    private FlowLayoutPanel createProjectNavigation()
    {
        FlowLayoutPanel navigation = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(5) };
        Button projectButton = new Button { Text = "Dự án", AutoSize = true };
        Button detailButton = new Button { Text = "Chi tiết dự án", AutoSize = true };
        Button taskButton = new Button { Text = "Công việc", AutoSize = true };
        Button assignmentButton = new Button { Text = "Phân công", AutoSize = true };
        projectButton.Click += openProjects;
        detailButton.Click += openProjectDetails;
        taskButton.Click += openTasks;
        assignmentButton.Click += openAssignments;
        navigation.Controls.AddRange(new Control[] { projectButton, detailButton, taskButton, assignmentButton });
        return navigation;
    }

    private void openProjects(object? sender, EventArgs e)
    {
        using ProjectForm form = new ProjectForm(
            ServiceFactory.createProjectBus(connectionString),
            ServiceFactory.createProjectDetailBus(connectionString),
            ServiceFactory.createProjectTaskBus(connectionString),
            ServiceFactory.createProjectLookupBus(connectionString));
        form.ShowDialog(this);
    }

    private void openProjectDetails(object? sender, EventArgs e)
    {
        using ProjectDetailForm form = new ProjectDetailForm(
            ServiceFactory.createProjectDetailBus(connectionString),
            ServiceFactory.createProjectBus(connectionString));
        form.ShowDialog(this);
    }

    private void openTasks(object? sender, EventArgs e)
    {
        using ProjectTaskForm form = new ProjectTaskForm(
            ServiceFactory.createProjectTaskBus(connectionString),
            ServiceFactory.createProjectDetailBus(connectionString));
        form.ShowDialog(this);
    }

    private void openAssignments(object? sender, EventArgs e)
    {
        using AssignmentForm form = new AssignmentForm(
            ServiceFactory.createAssignmentBus(connectionString),
            ServiceFactory.createProjectBus(connectionString),
            ServiceFactory.createProjectDetailBus(connectionString),
            ServiceFactory.createProjectTaskBus(connectionString),
            ServiceFactory.createProjectLookupBus(connectionString));
        form.ShowDialog(this);
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
