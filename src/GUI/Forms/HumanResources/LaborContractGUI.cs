using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS.HumanResources;
using HRM.DTO.HumanResources;

namespace HRM.GUI.Forms.HumanResources
{
    public partial class LaborContractGUI : Form
    {
        private TextBox txtContractId, txtEmployeeId, txtBaseSalary, txtSearch;
        private ComboBox cboContractType, cboStatus;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private Button btnReadList, btnAdd, btnUpdate, btnDelete, btnSearch;
        private ListView lvContracts;

        public LaborContractGUI()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Labor Contract Management";
            this.Size = new Size(800, 580);

            txtContractId = new TextBox { Location = new Point(150, 20), Width = 200 };
            txtEmployeeId = new TextBox { Location = new Point(150, 55), Width = 200 };
            cboContractType = new ComboBox { Location = new Point(150, 90), Width = 200 };
            cboContractType.Items.AddRange(new object[] { "Full-time", "Part-time", "Probation" });
            dtpStartDate = new DateTimePicker { Location = new Point(150, 125), Width = 200 };
            dtpEndDate = new DateTimePicker { Location = new Point(150, 160), Width = 200 };
            txtBaseSalary = new TextBox { Location = new Point(150, 195), Width = 200 };
            cboStatus = new ComboBox { Location = new Point(150, 230), Width = 200 };
            cboStatus.Items.AddRange(new object[] { "Active", "Expired", "Terminated" });
            txtSearch = new TextBox { Location = new Point(150, 265), Width = 200 };

            this.Controls.Add(new Label { Text = "Contract ID:", Location = new Point(20, 20) });
            this.Controls.Add(txtContractId);
            this.Controls.Add(new Label { Text = "Employee ID:", Location = new Point(20, 55) });
            this.Controls.Add(txtEmployeeId);
            this.Controls.Add(new Label { Text = "Contract Type:", Location = new Point(20, 90) });
            this.Controls.Add(cboContractType);
            this.Controls.Add(new Label { Text = "Start Date:", Location = new Point(20, 125) });
            this.Controls.Add(dtpStartDate);
            this.Controls.Add(new Label { Text = "End Date:", Location = new Point(20, 160) });
            this.Controls.Add(dtpEndDate);
            this.Controls.Add(new Label { Text = "Base Salary:", Location = new Point(20, 195) });
            this.Controls.Add(txtBaseSalary);
            this.Controls.Add(new Label { Text = "Status:", Location = new Point(20, 230) });
            this.Controls.Add(cboStatus);
            this.Controls.Add(new Label { Text = "Search Emp ID:", Location = new Point(20, 265) });
            this.Controls.Add(txtSearch);

            btnReadList = new Button { Text = "Load List", Location = new Point(400, 20), Width = 95 };
            btnAdd = new Button { Text = "Add", Location = new Point(510, 20), Width = 95 };
            btnUpdate = new Button { Text = "Update", Location = new Point(400, 60), Width = 95 };
            btnDelete = new Button { Text = "Delete", Location = new Point(510, 60), Width = 95 };
            btnSearch = new Button { Text = "Search", Location = new Point(400, 263), Width = 95 };

            btnReadList.Click += btnReadList_Click;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += btnSearch_Click;
            this.Controls.AddRange(new Control[] { btnReadList, btnAdd, btnUpdate, btnDelete, btnSearch });

            lvContracts = new ListView
            {
                Location = new Point(20, 305),
                Size = new Size(740, 220),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvContracts.Columns.Add("Contract ID", 100);
            lvContracts.Columns.Add("Employee ID", 100);
            lvContracts.Columns.Add("Contract Type", 110);
            lvContracts.Columns.Add("Start Date", 100);
            lvContracts.Columns.Add("End Date", 100);
            lvContracts.Columns.Add("Base Salary", 110);
            lvContracts.Columns.Add("Status", 100);
            lvContracts.SelectedIndexChanged += lvContracts_SelectedIndexChanged;
            this.Controls.Add(lvContracts);
        }

        private void appendContractToListView(LaborContractDTO contract)
        {
            int n = lvContracts.Items.Count;
            lvContracts.Items.Add(contract.contractId);
            lvContracts.Items[n].SubItems.Add(contract.employeeId);
            lvContracts.Items[n].SubItems.Add(contract.contractType);
            lvContracts.Items[n].SubItems.Add(contract.startDate.ToString("yyyy-MM-dd"));
            lvContracts.Items[n].SubItems.Add(contract.endDate.ToString("yyyy-MM-dd"));
            lvContracts.Items[n].SubItems.Add(contract.baseSalary.ToString());
            lvContracts.Items[n].SubItems.Add(contract.status);
        }

        private void btnReadList_Click(object sender, EventArgs e)
        {
            lvContracts.Items.Clear();
            LaborContractBUS bus = new LaborContractBUS();
            List<LaborContractDTO> list = bus.readContractList();
            foreach (LaborContractDTO contract in list)
            {
                appendContractToListView(contract);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            decimal.TryParse(txtBaseSalary.Text, out decimal salary);
            LaborContractDTO contract = new LaborContractDTO(
                txtContractId.Text, txtEmployeeId.Text, cboContractType.Text,
                dtpStartDate.Value, dtpEndDate.Value, salary, cboStatus.Text
            );
            LaborContractBUS bus = new LaborContractBUS();
            bus.add(contract);
            appendContractToListView(contract);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            decimal.TryParse(txtBaseSalary.Text, out decimal salary);
            LaborContractDTO contract = new LaborContractDTO(
                txtContractId.Text, txtEmployeeId.Text, cboContractType.Text,
                dtpStartDate.Value, dtpEndDate.Value, salary, cboStatus.Text
            );
            LaborContractBUS bus = new LaborContractBUS();
            bus.update(contract);
            btnReadList_Click(sender, e);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            LaborContractBUS bus = new LaborContractBUS();
            bus.delete(txtContractId.Text);
            btnReadList_Click(sender, e);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            lvContracts.Items.Clear();
            LaborContractBUS bus = new LaborContractBUS();
            List<LaborContractDTO> list = bus.searchByEmployeeId(txtSearch.Text);
            foreach (LaborContractDTO contract in list)
            {
                appendContractToListView(contract);
            }
        }

        private void lvContracts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvContracts.SelectedItems.Count > 0)
            {
                ListViewItem item = lvContracts.SelectedItems[0];
                txtContractId.Text = item.SubItems[0].Text;
                txtEmployeeId.Text = item.SubItems[1].Text;
                cboContractType.Text = item.SubItems[2].Text;
                if (DateTime.TryParse(item.SubItems[3].Text, out DateTime start)) dtpStartDate.Value = start;
                if (DateTime.TryParse(item.SubItems[4].Text, out DateTime end)) dtpEndDate.Value = end;
                txtBaseSalary.Text = item.SubItems[5].Text;
                cboStatus.Text = item.SubItems[6].Text;
            }
        }
    }
}
