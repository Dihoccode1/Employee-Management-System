#nullable enable
namespace GUI.Forms;
partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private Button loadButton = null!;
    private DataGridView employeesGrid = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        loadButton = new Button();
        employeesGrid = new DataGridView();
        ((System.ComponentModel.ISupportInitialize)employeesGrid).BeginInit();
        SuspendLayout();
        loadButton.Text = "Tải danh sách nhân viên";
        loadButton.Dock = DockStyle.Top;
        loadButton.Height = 48;
        loadButton.Click += LoadEmployees_Click;
        employeesGrid.Dock = DockStyle.Fill;
        employeesGrid.ReadOnly = true;
        employeesGrid.AllowUserToAddRows = false;
        employeesGrid.AllowUserToDeleteRows = false;
        employeesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        Controls.Add(employeesGrid);
        Controls.Add(loadButton);
        ClientSize = new Size(1100, 650);
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Ứng dụng quản lý nhân sự";
        ((System.ComponentModel.ISupportInitialize)employeesGrid).EndInit();
        ResumeLayout(false);
    }
}