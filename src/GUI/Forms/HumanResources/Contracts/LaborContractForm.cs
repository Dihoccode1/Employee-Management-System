using System;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    public partial class LaborContractForm : Form
    {
        private readonly LaborContractBUS contractBUS = new LaborContractBUS();
        private TextBox txtContractId, txtEmployeeId, txtBaseSalary;
        private ComboBox cboContractType, cboStatus;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private Button btnAdd, btnUpdate, btnDelete;
        private DataGridView dgvContracts;

        public LaborContractForm()
        {
            InitializeComponent();
            loadContractData();
        }

        private void InitializeComponent()
        {
            this.Text = "Labor Contract Management";
            this.Size = new Size(780, 560);

            txtContractId = new TextBox { Location = new Point(150, 20), Width = 200 };
            txtEmployeeId = new TextBox { Location = new Point(150, 55), Width = 200 };
            cboContractType = new ComboBox { Location = new Point(150, 90), Width = 200 };
            cboContractType.Items.AddRange(new object[] { "Full-time", "Part-time", "Probation" });
            dtpStartDate = new DateTimePicker { Location = new Point(150, 125), Width = 200 };
            dtpEndDate = new DateTimePicker { Location = new Point(150, 160), Width = 200 };
            txtBaseSalary = new TextBox { Location = new Point(150, 195), Width = 200 };
            cboStatus = new ComboBox { Location = new Point(150, 230), Width = 200 };
            cboStatus.Items.AddRange(new object[] { "Active", "Expired", "Terminated" });

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

            btnAdd = new Button { Text = "Add", Location = new Point(420, 20), Width = 90 };
            btnUpdate = new Button { Text = "Update", Location = new Point(420, 60), Width = 90 };
            btnDelete = new Button { Text = "Delete", Location = new Point(420, 100), Width = 90 };

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            this.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete });

            dgvContracts = new DataGridView
            {
                Location = new Point(20, 275),
                Size = new Size(720, 220),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            dgvContracts.CellClick += dgvContracts_CellClick;
            this.Controls.Add(dgvContracts);
        }

        private void loadContractData()
        {
            dgvContracts.DataSource = contractBUS.getAllContracts();
        }

        private LaborContractDTO getContractInput()
        {
            decimal.TryParse(txtBaseSalary.Text, out decimal salary);
            return new LaborContractDTO(
                txtContractId.Text.Trim(),
                txtEmployeeId.Text.Trim(),
                cboContractType.Text,
                dtpStartDate.Value,
                dtpEndDate.Value,
                salary,
                cboStatus.Text
            );
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (contractBUS.addContract(getContractInput()))
            {
                MessageBox.Show("Added contract successfully!");
                loadContractData();
            }
            else MessageBox.Show("Failed to add contract.");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (contractBUS.updateContract(getContractInput()))
            {
                MessageBox.Show("Updated contract successfully!");
                loadContractData();
            }
            else MessageBox.Show("Failed to update contract.");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (contractBUS.deleteContract(txtContractId.Text.Trim()))
            {
                MessageBox.Show("Deleted contract successfully!");
                loadContractData();
            }
            else MessageBox.Show("Failed to delete contract.");
        }

        private void dgvContracts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvContracts.Rows[e.RowIndex];
                txtContractId.Text = row.Cells["contractId"].Value?.ToString();
                txtEmployeeId.Text = row.Cells["employeeId"].Value?.ToString();
                cboContractType.Text = row.Cells["contractType"].Value?.ToString();
                dtpStartDate.Value = Convert.ToDateTime(row.Cells["startDate"].Value);
                dtpEndDate.Value = Convert.ToDateTime(row.Cells["endDate"].Value);
                txtBaseSalary.Text = row.Cells["baseSalary"].Value?.ToString();
                cboStatus.Text = row.Cells["status"].Value?.ToString();
            }
        }
    }
}
