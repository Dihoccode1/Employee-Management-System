using System;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    public partial class EmployeeForm : Form
    {
        private readonly EmployeeBUS employeeBUS = new EmployeeBUS();
        private TextBox txtEmployeeId, txtDepartmentId, txtPositionId, txtLastName, txtFirstName;
        private TextBox txtPhoneNumber, txtEmail, txtCurrentBaseSalary, txtDegree;
        private ComboBox cboGender, cboStatus;
        private DateTimePicker dtpDateOfBirth, dtpHireDate;
        private Button btnAdd, btnUpdate, btnDelete;
        private DataGridView dgvEmployees;

        public EmployeeForm()
        {
            InitializeComponent();
            loadEmployeeData();
        }

        private void InitializeComponent()
        {
            this.Text = "Employee Management";
            this.Size = new Size(950, 650);

            txtEmployeeId = new TextBox { Location = new Point(150, 20), Width = 180 };
            txtDepartmentId = new TextBox { Location = new Point(150, 55), Width = 180 };
            txtPositionId = new TextBox { Location = new Point(150, 90), Width = 180 };
            txtLastName = new TextBox { Location = new Point(150, 125), Width = 180 };
            txtFirstName = new TextBox { Location = new Point(150, 160), Width = 180 };
            dtpDateOfBirth = new DateTimePicker { Location = new Point(150, 195), Width = 180 };
            cboGender = new ComboBox { Location = new Point(150, 230), Width = 180 };
            cboGender.Items.AddRange(new object[] { "Nam", "Nu", "Khac" });

            txtPhoneNumber = new TextBox { Location = new Point(500, 20), Width = 180 };
            txtEmail = new TextBox { Location = new Point(500, 55), Width = 180 };
            dtpHireDate = new DateTimePicker { Location = new Point(500, 90), Width = 180 };
            cboStatus = new ComboBox { Location = new Point(500, 125), Width = 180 };
            cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            txtCurrentBaseSalary = new TextBox { Location = new Point(500, 160), Width = 180 };
            txtDegree = new TextBox { Location = new Point(500, 195), Width = 180 };

            this.Controls.Add(new Label { Text = "Employee ID:", Location = new Point(20, 20) });
            this.Controls.Add(txtEmployeeId);
            this.Controls.Add(new Label { Text = "Department ID:", Location = new Point(20, 55) });
            this.Controls.Add(txtDepartmentId);
            this.Controls.Add(new Label { Text = "Position ID:", Location = new Point(20, 90) });
            this.Controls.Add(txtPositionId);
            this.Controls.Add(new Label { Text = "Last Name:", Location = new Point(20, 125) });
            this.Controls.Add(txtLastName);
            this.Controls.Add(new Label { Text = "First Name:", Location = new Point(20, 160) });
            this.Controls.Add(txtFirstName);
            this.Controls.Add(new Label { Text = "Date of Birth:", Location = new Point(20, 195) });
            this.Controls.Add(dtpDateOfBirth);
            this.Controls.Add(new Label { Text = "Gender:", Location = new Point(20, 230) });
            this.Controls.Add(cboGender);

            this.Controls.Add(new Label { Text = "Phone:", Location = new Point(370, 20) });
            this.Controls.Add(txtPhoneNumber);
            this.Controls.Add(new Label { Text = "Email:", Location = new Point(370, 55) });
            this.Controls.Add(txtEmail);
            this.Controls.Add(new Label { Text = "Hire Date:", Location = new Point(370, 90) });
            this.Controls.Add(dtpHireDate);
            this.Controls.Add(new Label { Text = "Status:", Location = new Point(370, 125) });
            this.Controls.Add(cboStatus);
            this.Controls.Add(new Label { Text = "Base Salary:", Location = new Point(370, 160) });
            this.Controls.Add(txtCurrentBaseSalary);
            this.Controls.Add(new Label { Text = "Degree:", Location = new Point(370, 195) });
            this.Controls.Add(txtDegree);

            btnAdd = new Button { Text = "Add", Location = new Point(730, 20), Width = 90 };
            btnUpdate = new Button { Text = "Update", Location = new Point(730, 60), Width = 90 };
            btnDelete = new Button { Text = "Delete", Location = new Point(730, 100), Width = 90 };

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            this.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete });

            dgvEmployees = new DataGridView
            {
                Location = new Point(20, 275),
                Size = new Size(890, 310),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            dgvEmployees.CellClick += dgvEmployees_CellClick;
            this.Controls.Add(dgvEmployees);
        }

        private void loadEmployeeData()
        {
            dgvEmployees.DataSource = employeeBUS.getAllEmployees();
        }

        private EmployeeDTO getFormInput()
        {
            decimal.TryParse(txtCurrentBaseSalary.Text, out decimal salary);
            return new EmployeeDTO(
                txtEmployeeId.Text.Trim(),
                txtDepartmentId.Text.Trim(),
                txtPositionId.Text.Trim(),
                txtLastName.Text.Trim(),
                txtFirstName.Text.Trim(),
                dtpDateOfBirth.Value,
                cboGender.Text,
                txtPhoneNumber.Text.Trim(),
                txtEmail.Text.Trim(),
                dtpHireDate.Value,
                cboStatus.Text,
                salary,
                string.IsNullOrWhiteSpace(txtDegree.Text) ? null : txtDegree.Text.Trim()
            );
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (employeeBUS.addEmployee(getFormInput()))
            {
                MessageBox.Show("Added employee successfully!");
                loadEmployeeData();
            }
            else MessageBox.Show("Failed to add employee.");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (employeeBUS.updateEmployee(getFormInput()))
            {
                MessageBox.Show("Updated employee successfully!");
                loadEmployeeData();
            }
            else MessageBox.Show("Failed to update employee.");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (employeeBUS.deleteEmployee(txtEmployeeId.Text.Trim()))
            {
                MessageBox.Show("Deleted employee successfully!");
                loadEmployeeData();
            }
            else MessageBox.Show("Failed to delete employee.");
        }

        private void dgvEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvEmployees.Rows[e.RowIndex];
                txtEmployeeId.Text = row.Cells["employeeId"].Value?.ToString();
                txtDepartmentId.Text = row.Cells["departmentId"].Value?.ToString();
                txtPositionId.Text = row.Cells["positionId"].Value?.ToString();
                txtLastName.Text = row.Cells["lastName"].Value?.ToString();
                txtFirstName.Text = row.Cells["firstName"].Value?.ToString();
                dtpDateOfBirth.Value = Convert.ToDateTime(row.Cells["dateOfBirth"].Value);
                cboGender.Text = row.Cells["gender"].Value?.ToString();
                txtPhoneNumber.Text = row.Cells["phoneNumber"].Value?.ToString();
                txtEmail.Text = row.Cells["email"].Value?.ToString();
                dtpHireDate.Value = Convert.ToDateTime(row.Cells["hireDate"].Value);
                cboStatus.Text = row.Cells["status"].Value?.ToString();
                txtCurrentBaseSalary.Text = row.Cells["currentBaseSalary"].Value?.ToString();
                txtDegree.Text = row.Cells["degree"].Value?.ToString();
            }
        }
    }
}
