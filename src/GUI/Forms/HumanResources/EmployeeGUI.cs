using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS.HumanResources;
using HRM.DTO.HumanResources;

namespace HRM.GUI.Forms.HumanResources
{
    public partial class EmployeeGUI : Form
    {
        private TextBox txtEmployeeId, txtDepartmentId, txtPositionId, txtLastName, txtFirstName;
        private TextBox txtPhoneNumber, txtEmail, txtCurrentBaseSalary, txtDegree, txtSearch;
        private ComboBox cboGender, cboStatus;
        private DateTimePicker dtpDateOfBirth, dtpHireDate;
        private Button btnReadList, btnAdd, btnUpdate, btnDelete, btnSearch;
        private ListView lvEmployees;

        public EmployeeGUI()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Employee Management";
            this.Size = new Size(980, 650);

            txtEmployeeId = new TextBox { Location = new Point(150, 20), Width = 180 };
            txtDepartmentId = new TextBox { Location = new Point(150, 55), Width = 180 };
            txtPositionId = new TextBox { Location = new Point(150, 90), Width = 180 };
            txtLastName = new TextBox { Location = new Point(150, 125), Width = 180 };
            txtFirstName = new TextBox { Location = new Point(150, 160), Width = 180 };
            dtpDateOfBirth = new DateTimePicker { Location = new Point(150, 195), Width = 180 };
            cboGender = new ComboBox { Location = new Point(150, 230), Width = 180 };
            cboGender.Items.AddRange(new object[] { "Nam", "Nu" });

            txtPhoneNumber = new TextBox { Location = new Point(500, 20), Width = 180 };
            txtEmail = new TextBox { Location = new Point(500, 55), Width = 180 };
            dtpHireDate = new DateTimePicker { Location = new Point(500, 90), Width = 180 };
            cboStatus = new ComboBox { Location = new Point(500, 125), Width = 180 };
            cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            txtCurrentBaseSalary = new TextBox { Location = new Point(500, 160), Width = 180 };
            txtDegree = new TextBox { Location = new Point(500, 195), Width = 180 };
            txtSearch = new TextBox { Location = new Point(500, 230), Width = 180 };

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
            this.Controls.Add(new Label { Text = "Search LastName:", Location = new Point(370, 230) });
            this.Controls.Add(txtSearch);

            btnReadList = new Button { Text = "Load List", Location = new Point(720, 20), Width = 100 };
            btnAdd = new Button { Text = "Add", Location = new Point(720, 60), Width = 100 };
            btnUpdate = new Button { Text = "Update", Location = new Point(720, 100), Width = 100 };
            btnDelete = new Button { Text = "Delete", Location = new Point(720, 140), Width = 100 };
            btnSearch = new Button { Text = "Search", Location = new Point(720, 228), Width = 100 };

            btnReadList.Click += btnReadList_Click;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += btnSearch_Click;
            this.Controls.AddRange(new Control[] { btnReadList, btnAdd, btnUpdate, btnDelete, btnSearch });

            lvEmployees = new ListView
            {
                Location = new Point(20, 275),
                Size = new Size(920, 310),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvEmployees.Columns.Add("Emp ID", 70);
            lvEmployees.Columns.Add("Dept ID", 70);
            lvEmployees.Columns.Add("Pos ID", 70);
            lvEmployees.Columns.Add("Last Name", 100);
            lvEmployees.Columns.Add("First Name", 80);
            lvEmployees.Columns.Add("Birth Date", 85);
            lvEmployees.Columns.Add("Gender", 60);
            lvEmployees.Columns.Add("Phone", 90);
            lvEmployees.Columns.Add("Email", 110);
            lvEmployees.Columns.Add("Hire Date", 85);
            lvEmployees.Columns.Add("Status", 70);
            lvEmployees.Columns.Add("Salary", 85);
            lvEmployees.Columns.Add("Degree", 80);
            lvEmployees.SelectedIndexChanged += lvEmployees_SelectedIndexChanged;
            this.Controls.Add(lvEmployees);
        }

        private void appendEmployeeToListView(EmployeeDTO emp)
        {
            int n = lvEmployees.Items.Count;
            lvEmployees.Items.Add(emp.employeeId);
            lvEmployees.Items[n].SubItems.Add(emp.departmentId);
            lvEmployees.Items[n].SubItems.Add(emp.positionId);
            lvEmployees.Items[n].SubItems.Add(emp.lastName);
            lvEmployees.Items[n].SubItems.Add(emp.firstName);
            lvEmployees.Items[n].SubItems.Add(emp.dateOfBirth.ToString("yyyy-MM-dd"));
            lvEmployees.Items[n].SubItems.Add(emp.gender);
            lvEmployees.Items[n].SubItems.Add(emp.phoneNumber);
            lvEmployees.Items[n].SubItems.Add(emp.email);
            lvEmployees.Items[n].SubItems.Add(emp.hireDate.ToString("yyyy-MM-dd"));
            lvEmployees.Items[n].SubItems.Add(emp.status);
            lvEmployees.Items[n].SubItems.Add(emp.currentBaseSalary.ToString());
            lvEmployees.Items[n].SubItems.Add(emp.degree);
        }

        private void btnReadList_Click(object sender, EventArgs e)
        {
            lvEmployees.Items.Clear();
            EmployeeBUS bus = new EmployeeBUS();
            List<EmployeeDTO> list = bus.readEmployeeList();
            foreach (EmployeeDTO emp in list)
            {
                appendEmployeeToListView(emp);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            decimal.TryParse(txtCurrentBaseSalary.Text, out decimal salary);
            EmployeeDTO emp = new EmployeeDTO(
                txtEmployeeId.Text, txtDepartmentId.Text, txtPositionId.Text,
                txtLastName.Text, txtFirstName.Text, dtpDateOfBirth.Value,
                cboGender.Text, txtPhoneNumber.Text, txtEmail.Text,
                dtpHireDate.Value, cboStatus.Text, salary, txtDegree.Text
            );
            EmployeeBUS bus = new EmployeeBUS();
            bus.add(emp);
            appendEmployeeToListView(emp);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            decimal.TryParse(txtCurrentBaseSalary.Text, out decimal salary);
            EmployeeDTO emp = new EmployeeDTO(
                txtEmployeeId.Text, txtDepartmentId.Text, txtPositionId.Text,
                txtLastName.Text, txtFirstName.Text, dtpDateOfBirth.Value,
                cboGender.Text, txtPhoneNumber.Text, txtEmail.Text,
                dtpHireDate.Value, cboStatus.Text, salary, txtDegree.Text
            );
            EmployeeBUS bus = new EmployeeBUS();
            bus.update(emp);
            btnReadList_Click(sender, e);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            EmployeeBUS bus = new EmployeeBUS();
            bus.delete(txtEmployeeId.Text);
            btnReadList_Click(sender, e);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            lvEmployees.Items.Clear();
            EmployeeBUS bus = new EmployeeBUS();
            List<EmployeeDTO> list = bus.searchByLastName(txtSearch.Text);
            foreach (EmployeeDTO emp in list)
            {
                appendEmployeeToListView(emp);
            }
        }

        private void lvEmployees_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvEmployees.SelectedItems.Count > 0)
            {
                ListViewItem item = lvEmployees.SelectedItems[0];
                txtEmployeeId.Text = item.SubItems[0].Text;
                txtDepartmentId.Text = item.SubItems[1].Text;
                txtPositionId.Text = item.SubItems[2].Text;
                txtLastName.Text = item.SubItems[3].Text;
                txtFirstName.Text = item.SubItems[4].Text;
                if (DateTime.TryParse(item.SubItems[5].Text, out DateTime dob)) dtpDateOfBirth.Value = dob;
                cboGender.Text = item.SubItems[6].Text;
                txtPhoneNumber.Text = item.SubItems[7].Text;
                txtEmail.Text = item.SubItems[8].Text;
                if (DateTime.TryParse(item.SubItems[9].Text, out DateTime hire)) dtpHireDate.Value = hire;
                cboStatus.Text = item.SubItems[10].Text;
                txtCurrentBaseSalary.Text = item.SubItems[11].Text;
                txtDegree.Text = item.SubItems[12].Text;
            }
        }
    }
}
