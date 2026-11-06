using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS.HumanResources;
using HRM.DTO.HumanResources;

namespace HRM.GUI.Forms.HumanResources
{
    public partial class PositionGUI : Form
    {
        private TextBox txtPositionId, txtPositionName, txtDescription, txtAllowance, txtSearch;
        private Button btnReadList, btnAdd, btnUpdate, btnDelete, btnSearch;
        private ListView lvPositions;

        public PositionGUI()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Position Management";
            this.Size = new Size(740, 530);

            txtPositionId = new TextBox { Location = new Point(150, 20), Width = 200 };
            txtPositionName = new TextBox { Location = new Point(150, 55), Width = 200 };
            txtDescription = new TextBox { Location = new Point(150, 90), Width = 200 };
            txtAllowance = new TextBox { Location = new Point(150, 125), Width = 200 };
            txtSearch = new TextBox { Location = new Point(150, 160), Width = 200 };

            this.Controls.Add(new Label { Text = "Position ID:", Location = new Point(20, 20) });
            this.Controls.Add(txtPositionId);
            this.Controls.Add(new Label { Text = "Position Name:", Location = new Point(20, 55) });
            this.Controls.Add(txtPositionName);
            this.Controls.Add(new Label { Text = "Description:", Location = new Point(20, 90) });
            this.Controls.Add(txtDescription);
            this.Controls.Add(new Label { Text = "Allowance:", Location = new Point(20, 125) });
            this.Controls.Add(txtAllowance);
            this.Controls.Add(new Label { Text = "Search Name:", Location = new Point(20, 160) });
            this.Controls.Add(txtSearch);

            btnReadList = new Button { Text = "Load List", Location = new Point(380, 20), Width = 90 };
            btnAdd = new Button { Text = "Add", Location = new Point(480, 20), Width = 90 };
            btnUpdate = new Button { Text = "Update", Location = new Point(380, 60), Width = 90 };
            btnDelete = new Button { Text = "Delete", Location = new Point(480, 60), Width = 90 };
            btnSearch = new Button { Text = "Search", Location = new Point(380, 158), Width = 90 };

            btnReadList.Click += btnReadList_Click;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += btnSearch_Click;

            this.Controls.AddRange(new Control[] { btnReadList, btnAdd, btnUpdate, btnDelete, btnSearch });

            lvPositions = new ListView
            {
                Location = new Point(20, 200),
                Size = new Size(680, 270),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvPositions.Columns.Add("Position ID", 130);
            lvPositions.Columns.Add("Position Name", 180);
            lvPositions.Columns.Add("Description", 200);
            lvPositions.Columns.Add("Allowance", 150);
            lvPositions.SelectedIndexChanged += lvPositions_SelectedIndexChanged;
            this.Controls.Add(lvPositions);
        }

        private void btnReadList_Click(object sender, EventArgs e)
        {
            lvPositions.Items.Clear();
            PositionBUS bus = new PositionBUS();
            List<PositionDTO> list = bus.readPositionList();
            foreach (PositionDTO pos in list)
            {
                int n = lvPositions.Items.Count;
                lvPositions.Items.Add(pos.positionId);
                lvPositions.Items[n].SubItems.Add(pos.positionName);
                lvPositions.Items[n].SubItems.Add(pos.description);
                lvPositions.Items[n].SubItems.Add(pos.positionAllowance.ToString());
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string id = txtPositionId.Text;
            string name = txtPositionName.Text;
            string desc = txtDescription.Text;
            decimal.TryParse(txtAllowance.Text, out decimal allowance);

            PositionDTO pos = new PositionDTO(id, name, desc, allowance);
            PositionBUS bus = new PositionBUS();
            bus.add(pos);

            int n = lvPositions.Items.Count;
            lvPositions.Items.Add(pos.positionId);
            lvPositions.Items[n].SubItems.Add(pos.positionName);
            lvPositions.Items[n].SubItems.Add(pos.description);
            lvPositions.Items[n].SubItems.Add(pos.positionAllowance.ToString());
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            decimal.TryParse(txtAllowance.Text, out decimal allowance);
            PositionDTO pos = new PositionDTO(txtPositionId.Text, txtPositionName.Text, txtDescription.Text, allowance);
            PositionBUS bus = new PositionBUS();
            bus.update(pos);
            btnReadList_Click(sender, e);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            PositionBUS bus = new PositionBUS();
            bus.delete(txtPositionId.Text);
            btnReadList_Click(sender, e);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            lvPositions.Items.Clear();
            PositionBUS bus = new PositionBUS();
            List<PositionDTO> list = bus.searchByName(txtSearch.Text);
            foreach (PositionDTO pos in list)
            {
                int n = lvPositions.Items.Count;
                lvPositions.Items.Add(pos.positionId);
                lvPositions.Items[n].SubItems.Add(pos.positionName);
                lvPositions.Items[n].SubItems.Add(pos.description);
                lvPositions.Items[n].SubItems.Add(pos.positionAllowance.ToString());
            }
        }

        private void lvPositions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvPositions.SelectedItems.Count > 0)
            {
                ListViewItem item = lvPositions.SelectedItems[0];
                txtPositionId.Text = item.SubItems[0].Text;
                txtPositionName.Text = item.SubItems[1].Text;
                txtDescription.Text = item.SubItems[2].Text;
                txtAllowance.Text = item.SubItems[3].Text;
            }
        }
    }
}
