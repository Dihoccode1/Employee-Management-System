#nullable enable
namespace GUI.Forms.ProjectManagement.Assignments;

partial class AssignmentForm
{
    private TextBox idTextBox = null!;
    private TextBox searchTextBox = null!;
    private ListView itemListView = null!;
    private ComboBox taskIdComboBox = null!;
    private ComboBox projectIdComboBox = null!;
    private ComboBox employeeIdComboBox = null!;
    private DateTimePicker assignedDatePicker = null!;
    private DateTimePicker startDatePicker = null!;
    private DateTimePicker deadlinePicker = null!;
    private DateTimePicker completedDatePicker = null!;
    private NumericUpDown completionRateInput = null!;
    private NumericUpDown contributionScoreInput = null!;
    private ComboBox statusComboBox = null!;
    private TextBox notesTextBox = null!;

    private void initializeControls()
    {
        Text = "Phân công";
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
        taskIdComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        projectIdComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        employeeIdComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        assignedDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
        startDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
        deadlinePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
        completedDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
        completionRateInput = new NumericUpDown { DecimalPlaces = 2, Maximum = 100m, ThousandsSeparator = true };
        contributionScoreInput = new NumericUpDown { DecimalPlaces = 2, Maximum = 99999999.99m, ThousandsSeparator = true };
        statusComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        notesTextBox = new TextBox { MaxLength = 1000 };
        statusComboBox.Items.AddRange(new object[] { "Not started", "In progress", "Completed", "Cancelled" });
        ProjectFormControls.addField(editor, "Mã (tự động, không nhập)", idTextBox);
        ProjectFormControls.addField(editor, "Dự án", projectIdComboBox);
        ProjectFormControls.addField(editor, "Công việc", taskIdComboBox);
        ProjectFormControls.addField(editor, "Nhân viên", employeeIdComboBox);
        ProjectFormControls.addField(editor, "Ngày phân công", assignedDatePicker);
        ProjectFormControls.addField(editor, "Ngày bắt đầu", startDatePicker);
        ProjectFormControls.addField(editor, "Hạn hoàn thành", deadlinePicker);
        ProjectFormControls.addField(editor, "Ngày hoàn thành", completedDatePicker);
        ProjectFormControls.addField(editor, "Hoàn thành (%)", completionRateInput);
        ProjectFormControls.addField(editor, "Điểm đóng góp", contributionScoreInput);
        ProjectFormControls.addField(editor, "Trạng thái", statusComboBox);
        ProjectFormControls.addField(editor, "Ghi chú", notesTextBox);

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
        itemListView.Columns.Add("Công việc", 120);
        itemListView.Columns.Add("Dự án", 120);
        itemListView.Columns.Add("Nhân viên", 120);
        itemListView.Columns.Add("Ngày phân công", 120);
        itemListView.Columns.Add("Ngày bắt đầu", 120);
        itemListView.Columns.Add("Hạn hoàn thành", 120);
        itemListView.Columns.Add("Ngày hoàn thành", 120);
        itemListView.Columns.Add("Hoàn thành (%)", 120);
        itemListView.Columns.Add("Điểm đóng góp", 120);
        itemListView.Columns.Add("Trạng thái", 120);
        itemListView.Columns.Add("Ghi chú", 120);
        itemListView.SelectedIndexChanged += itemSelectionChanged;
        projectIdComboBox.SelectedIndexChanged += projectSelectionChanged;

        mainSplit.Panel1.Controls.Add(itemListView);
        mainSplit.Panel1.Controls.Add(buttons);
        mainSplit.Panel1.Controls.Add(searchPanel);
        mainSplit.Panel2.Controls.Add(editor);
        Controls.Add(mainSplit);
        Shown += formShown;
    }
}
