using BUS;
using GUI.Forms;
namespace GUI;
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var connectionString = Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            MessageBox.Show("Vui lòng cấu hình HRM_CONNECTION_STRING theo README.", "Cấu hình SQL Server");
            return;
        }
        Application.Run(new MainForm(ServiceFactory.CreateEmployeeService(connectionString), connectionString));
    }
}
