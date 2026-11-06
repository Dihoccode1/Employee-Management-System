using System;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    public partial class PositionForm : Form
    {
        private readonly PositionBUS positionBUS = new PositionBUS();
        private TextBox txtPositionId, txtPositionName, txtDescription, txtAllowance;
        private Button btnAdd, btnUpdate, btnDelete;
        private DataGridView dgvPositions;

        public PositionForm()
        {
            InitializeComponent();
            loadPositionData();
        }

        private void InitializeComponent()
        {
            this.Text = "Position Management";
            this.Size = new Size(700, 520);

            txtPositionId = new TextBox { Location = new Point(150, 20), Width = 200 };
            txtPositionName = new TextBox { Location = new Point(150, 60), Width = 200 };
            txtDescription = new TextBox { Location = new Point(150, 100), Width = 200 };
            txtAllowance = new TextBox { Location = new Point(150, 140), Width = 200 };

            this.Controls.Add(new Label { Text = "Position ID:", Location = new Point(20, 20) });
            this.Controls.Add(txtPositionId);
            this.Controls.Add(new Label { Text = "Position Name:", Location = new Point(20, 60) });
            this.Controls.Add(txtPositionName);
            this.Controls.Add(new Label { Text = "Description:", Location = new Point(20, 100) });
            this.Controls.Add(txtDescription);
            this.Controls.Add(new Label { Text = "Allowance:", Location = new Point(20, 140) });
            this.Controls.Add(txtAllowance);

            btnAdd = new Button { Text = "Add", Location = new Point(400, 20), Width = 80 };
            btnUpdate = new Button { Text = "Update", Location = new Point(400, 60), Width = 80 };
            btnDelete = new Button { Text = "Delete", Location = new Point(400, 100), Width = 80 };

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;

            this.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete });

            dgvPositions = new DataGridView
            {
                Location = new Point(20, 180),
                Size = new Size(640, 280),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            dgvPositions.CellClick += dgvPositions_CellClick;
            this.Controls.Add(dgvPositions);
        }

        private void loadPositionData()
        {
            dgvPositions.DataSource = positionBUS.getAllPositions();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            decimal.TryParse(txtAllowance.Text, out decimal allowance);
            var dto = new PositionDTO(txtPositionId.Text.Trim(), txtPositionName.Text.Trim(), txtDescription.Text.Trim(), allowance);
            if (positionBUS.addPosition(dto))
            {
                MessageBox.Show("Added successfully!");
                loadPositionData();
            }
            else MessageBox.Show("Failed to add position.");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            decimal.TryParse(txtAllowance.Text, out decimal allowance);
            var dto = new PositionDTO(txtPositionId.Text.Trim(), txtPositionName.Text.Trim(), txtDescription.Text.Trim(), allowance);
            if (positionBUS.updatePosition(dto))
            {
                MessageBox.Show("Updated successfully!");
                loadPositionData();
            }
            else MessageBox.Show("Failed to update position.");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (positionBUS.deletePosition(txtPositionId.Text.Trim()))
            {
                MessageBox.Show("Deleted successfully!");
                loadPositionData();
            }
            else MessageBox.Show("Failed to delete position.");
        }

        private void dgvPositions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPositions.Rows[e.RowIndex];
                txtPositionId.Text = row.Cells["positionId"].Value?.ToString();
                txtPositionName.Text = row.Cells["positionName"].Value?.ToString();
                txtDescription.Text = row.Cells["description"].Value?.ToString();
                txtAllowance.Text = row.Cells["positionAllowance"].Value?.ToString();
            }
        }
    }
}
