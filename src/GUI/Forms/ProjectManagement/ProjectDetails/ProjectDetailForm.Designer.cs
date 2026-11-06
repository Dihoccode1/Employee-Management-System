#nullable enable
namespace GUI.Forms.ProjectManagement.ProjectDetails;

partial class ProjectDetailForm
{
    private TextBox idTextBox = null!;
    private TextBox searchTextBox = null!;
    private ListView itemListView = null!;
    private ComboBox projectIdComboBox = null!;
    private NumericUpDown completionRateInput = null!;
    private ComboBox statusComboBox = null!;
    private NumericUpDown allowanceInput = null!;
    private NumericUpDown rewardInput = null!;

    private void initializeControls()
    {
        Text = "Chi tiết dự án";
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(1000, 650);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        SplitContainer mainSplit = new SplitContainer
        {
            Size = ClientSize, Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel2, SplitterDistance = 820,
            Panel1MinSize = 500, Panel2MinSize = 340
        };
        FlowLayoutPanel editor = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoScroll = true,
            FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(8)
        };
        idTextBox = new TextBox { ReadOnly = true };
        projectIdComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        completionRateInput = new NumericUpDown { DecimalPlaces = 2, Maximum = 100m, ThousandsSeparator = true };
        statusComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        allowanceInput = new NumericUpDown { DecimalPlaces = 2, Maximum = 9999999999999999.99m, ThousandsSeparator = true };
        rewardInput = new NumericUpDown { DecimalPlaces = 2, Maximum = 9999999999999999.99m, ThousandsSeparator = true };
        statusComboBox.Items.AddRange(new object[] { "Not started", "In progress", "Completed", "Cancelled" });
        ProjectFormControls.addField(editor, "Mã (tự động, không nhập)", idTextBox);
        ProjectFormControls.addField(editor, "Dự án", projectIdComboBox);
        ProjectFormControls.addField(editor, "Tiến độ (%)", completionRateInput);
        ProjectFormControls.addField(editor, "Trạng thái", statusComboBox);
        ProjectFormControls.addField(editor, "Phụ cấp", allowanceInput);
        ProjectFormControls.addField(editor, "Thưởng", rewardInput);

        FlowLayoutPanel searchPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(5), WrapContents = false };
        searchTextBox = new TextBox { Width = 230, PlaceholderText = "Nhập từ khóa hoặc mã..." };
        Button searchButton = new Button { Text = "Tìm kiếm", AutoSize = true };
        Button reloadButton = new Button { Text = "Tải lại", AutoSize = true };
        searchButton.Click += searchButtonClick;
        reloadButton.Click += reloadButtonClick;
        searchPanel.Controls.Add(searchTextBox);
        searchPanel.Controls.Add(searchButton);
        searchPanel.Controls.Add(reloadButton);

        FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(5) };
        Button newButton = new Button { Text = "Nhập mới", AutoSize = true };
        Button addButton = new Button { Text = "Thêm", AutoSize = true };
        Button updateButton = new Button { Text = "Sửa", AutoSize = true };
        Button deleteButton = new Button { Text = "Xóa", AutoSize = true };
        newButton.Click += newButtonClick;
        addButton.Click += addButtonClick;
        updateButton.Click += updateButtonClick;
        deleteButton.Click += deleteButtonClick;
        buttons.Controls.AddRange(new Control[] { newButton, addButton, updateButton, deleteButton });

        itemListView = new ListView
        {
            Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
            MultiSelect = false, HideSelection = false
        };
        itemListView.Columns.Add("Mã", 60);
        itemListView.Columns.Add("Dự án", 120);
        itemListView.Columns.Add("Tiến độ (%)", 120);
        itemListView.Columns.Add("Trạng thái", 120);
        itemListView.Columns.Add("Phụ cấp", 120);
        itemListView.Columns.Add("Thưởng", 120);
        itemListView.SelectedIndexChanged += itemSelectionChanged;

        mainSplit.Panel1.Controls.Add(itemListView);
        mainSplit.Panel1.Controls.Add(buttons);
        mainSplit.Panel1.Controls.Add(searchPanel);
        mainSplit.Panel2.Controls.Add(editor);
        Controls.Add(mainSplit);
        Shown += formShown;
    }
}
