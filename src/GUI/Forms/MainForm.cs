using BUS.Employees;
namespace GUI.Forms;
public partial class MainForm : Form
{
    private readonly EmployeeService _employeeService;
    public MainForm(EmployeeService employeeService)
    {
        _employeeService = employeeService;
        InitializeComponent();
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