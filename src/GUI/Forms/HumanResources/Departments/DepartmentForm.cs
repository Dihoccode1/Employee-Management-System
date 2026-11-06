using System;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    public partial class DepartmentForm : Form
    {
        private readonly DepartmentBUS departmentBUS = new DepartmentBUS();
        private TextBox txtDepartmentId, txtDepartmentName, txtDescription;
        private Button btnAdd, btnUpdate, btnDelete;
        private DataGridView dgvDepartments;

        public DepartmentForm()
        {
            InitializeComponent();
            loadDepartmentData();
        }

        private void InitializeComponent()
        {
            this.Text = "Department Management";
            this.Size = new Size(700, 500);

            txtDepartmentId = new TextBox { Location = new Point(150, 20), Width = 200 };
            txtDepartmentName = new TextBox { Location = new Point(150, 60), Width = 200 };
            txtDescription = new TextBox { Location = new Point(150, 100), Width = 200 };

            this.Controls.Add(new Label { Text = "Department ID:", Location = new Point(20, 20) });
            this.Controls.Add(txtDepartmentId);
            this.Controls.Add(new Label { Text = "Department Name:", Location = new Point(20, 60) });
            this.Controls.Add(txtDepartmentName);
            this.Controls.Add(new Label { Text = "Description:", Location = new Point(20, 100) });
            this.Controls.Add(txtDescription);

            btnAdd = new Button { Text = "Add", Location = new Point(400, 20), Width = 80 };
            btnUpdate = new Button { Text = "Update", Location = new Point(400, 60), Width = 80 };
            btnDelete = new Button { Text = "Delete", Location = new Point(400, 100), Width = 80 };

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;

            this.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete });

            dgvDepartments = new DataGridView
            {
                Location = new Point(20, 150),
                Size = new Size(640, 280),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            dgvDepartments.CellClick += dgvDepartments_CellClick;
            this.Controls.Add(dgvDepartments);
        }

        private void loadDepartmentData()
        {
            dgvDepartments.DataSource = departmentBUS.getAllDepartments();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var dto = new DepartmentDTO(txtDepartmentId.Text.Trim(), txtDepartmentName.Text.Trim(), txtDescription.Text.Trim());
            if (departmentBUS.addDepartment(dto))
            {
                MessageBox.Show("Added successfully!");
                loadDepartmentData();
            }
            else MessageBox.Show("Failed to add department.");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            var dto = new DepartmentDTO(txtDepartmentId.Text.Trim(), txtDepartmentName.Text.Trim(), txtDescription.Text.Trim());
            if (departmentBUS.updateDepartment(dto))
            {
                MessageBox.Show("Updated successfully!");
                loadDepartmentData();
            }
            else MessageBox.Show("Failed to update department.");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (departmentBUS.deleteDepartment(txtDepartmentId.Text.Trim()))
            {
                MessageBox.Show("Deleted successfully!");
                loadDepartmentData();
            }
            else MessageBox.Show("Failed to delete department.");
        }

        private void dgvDepartments_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDepartments.Rows[e.RowIndex];
                txtDepartmentId.Text = row.Cells["departmentId"].Value?.ToString();
                txtDepartmentName.Text = row.Cells["departmentName"].Value?.ToString();
                txtDescription.Text = row.Cells["description"].Value?.ToString();
            }
        }
    }
}
