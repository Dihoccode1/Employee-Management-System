using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS.HumanResources;
using HRM.DTO.HumanResources;

namespace HRM.GUI.Forms.HumanResources
{
    public partial class DepartmentGUI : Form
    {
        private TextBox txtDepartmentId, txtDepartmentName, txtDescription, txtSearch;
        private Button btnReadList, btnAdd, btnUpdate, btnDelete, btnSearch;
        private ListView lvDepartments;

        public DepartmentGUI()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Department Management";
            this.Size = new Size(720, 500);

            txtDepartmentId = new TextBox { Location = new Point(150, 20), Width = 200 };
            txtDepartmentName = new TextBox { Location = new Point(150, 55), Width = 200 };
            txtDescription = new TextBox { Location = new Point(150, 90), Width = 200 };
            txtSearch = new TextBox { Location = new Point(150, 125), Width = 200 };

            this.Controls.Add(new Label { Text = "Department ID:", Location = new Point(20, 20) });
            this.Controls.Add(txtDepartmentId);
            this.Controls.Add(new Label { Text = "Department Name:", Location = new Point(20, 55) });
            this.Controls.Add(txtDepartmentName);
            this.Controls.Add(new Label { Text = "Description:", Location = new Point(20, 90) });
            this.Controls.Add(txtDescription);
            this.Controls.Add(new Label { Text = "Search Name:", Location = new Point(20, 125) });
            this.Controls.Add(txtSearch);

            btnReadList = new Button { Text = "Load List", Location = new Point(380, 20), Width = 90 };
            btnAdd = new Button { Text = "Add", Location = new Point(480, 20), Width = 90 };
            btnUpdate = new Button { Text = "Update", Location = new Point(380, 60), Width = 90 };
            btnDelete = new Button { Text = "Delete", Location = new Point(480, 60), Width = 90 };
            btnSearch = new Button { Text = "Search", Location = new Point(380, 123), Width = 90 };

            btnReadList.Click += btnReadList_Click;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += btnSearch_Click;

            this.Controls.AddRange(new Control[] { btnReadList, btnAdd, btnUpdate, btnDelete, btnSearch });

            lvDepartments = new ListView
            {
                Location = new Point(20, 170),
                Size = new Size(660, 270),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvDepartments.Columns.Add("Department ID", 150);
            lvDepartments.Columns.Add("Department Name", 220);
            lvDepartments.Columns.Add("Description", 270);
            lvDepartments.SelectedIndexChanged += lvDepartments_SelectedIndexChanged;
            this.Controls.Add(lvDepartments);
        }

        private void btnReadList_Click(object sender, EventArgs e)
        {
            lvDepartments.Items.Clear();
            DepartmentBUS bus = new DepartmentBUS();
            List<DepartmentDTO> list = bus.readDepartmentList();
            foreach (DepartmentDTO dept in list)
            {
                int n = lvDepartments.Items.Count;
                lvDepartments.Items.Add(dept.departmentId);
                lvDepartments.Items[n].SubItems.Add(dept.departmentName);
                lvDepartments.Items[n].SubItems.Add(dept.description);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string id = txtDepartmentId.Text;
            string name = txtDepartmentName.Text;
            string desc = txtDescription.Text;

            DepartmentDTO dept = new DepartmentDTO(id, name, desc);
            DepartmentBUS bus = new DepartmentBUS();
            bus.add(dept);

            int n = lvDepartments.Items.Count;
            lvDepartments.Items.Add(dept.departmentId);
            lvDepartments.Items[n].SubItems.Add(dept.departmentName);
            lvDepartments.Items[n].SubItems.Add(dept.description);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DepartmentDTO dept = new DepartmentDTO(txtDepartmentId.Text, txtDepartmentName.Text, txtDescription.Text);
            DepartmentBUS bus = new DepartmentBUS();
            bus.update(dept);
            btnReadList_Click(sender, e);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DepartmentBUS bus = new DepartmentBUS();
            bus.delete(txtDepartmentId.Text);
            btnReadList_Click(sender, e);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            lvDepartments.Items.Clear();
            DepartmentBUS bus = new DepartmentBUS();
            List<DepartmentDTO> list = bus.searchByName(txtSearch.Text);
            foreach (DepartmentDTO dept in list)
            {
                int n = lvDepartments.Items.Count;
                lvDepartments.Items.Add(dept.departmentId);
                lvDepartments.Items[n].SubItems.Add(dept.departmentName);
                lvDepartments.Items[n].SubItems.Add(dept.description);
            }
        }

        private void lvDepartments_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvDepartments.SelectedItems.Count > 0)
            {
                ListViewItem item = lvDepartments.SelectedItems[0];
                txtDepartmentId.Text = item.SubItems[0].Text;
                txtDepartmentName.Text = item.SubItems[1].Text;
                txtDescription.Text = item.SubItems[2].Text;
            }
        }
    }
}
