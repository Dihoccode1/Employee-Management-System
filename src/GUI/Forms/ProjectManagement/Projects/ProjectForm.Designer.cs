#nullable enable
namespace GUI.Forms.ProjectManagement.Projects;

partial class ProjectForm
{
    private TextBox idTextBox = null!;
    private TextBox searchTextBox = null!;
    private ListView itemListView = null!;
    private ListView taskListView = null!;
    private TextBox nameTextBox = null!;
    private DateTimePicker startDatePicker = null!;
    private DateTimePicker endDatePicker = null!;
    private DateTimePicker deadlinePicker = null!;
    private TextBox descriptionTextBox = null!;
    private NumericUpDown budgetInput = null!;
    private ComboBox departmentIdComboBox = null!;
    private ComboBox managerIdComboBox = null!;
    private ComboBox statusComboBox = null!;

    private void initializeControls()
    {
        Text = "Dự án";
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
        nameTextBox = new TextBox { MaxLength = 150 };
        startDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
        endDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
        deadlinePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
        descriptionTextBox = new TextBox { MaxLength = 1000 };
        budgetInput = new NumericUpDown { DecimalPlaces = 2, Maximum = 9999999999999999.99m, ThousandsSeparator = true };
        departmentIdComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        managerIdComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        statusComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        statusComboBox.Items.AddRange(new object[] { "Not started", "In progress", "Completed", "Cancelled" });
        ProjectFormControls.addField(editor, "Mã (tự động, không nhập)", idTextBox);
        ProjectFormControls.addField(editor, "Tên dự án", nameTextBox);
        ProjectFormControls.addField(editor, "Ngày bắt đầu", startDatePicker);
        ProjectFormControls.addField(editor, "Ngày kết thúc", endDatePicker);
        ProjectFormControls.addField(editor, "Hạn hoàn thành", deadlinePicker);
        ProjectFormControls.addField(editor, "Mô tả", descriptionTextBox);
        ProjectFormControls.addField(editor, "Ngân sách", budgetInput);
        ProjectFormControls.addField(editor, "Phòng ban", departmentIdComboBox);
        ProjectFormControls.addField(editor, "Người quản lý", managerIdComboBox);
        ProjectFormControls.addField(editor, "Trạng thái", statusComboBox);

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
        itemListView.Columns.Add("Tên dự án", 180);
        itemListView.Columns.Add("Ngày bắt đầu", 120);
        itemListView.Columns.Add("Ngày kết thúc", 120);
        itemListView.Columns.Add("Hạn hoàn thành", 120);
        itemListView.Columns.Add("Mô tả", 120);
        itemListView.Columns.Add("Ngân sách", 120);
        itemListView.Columns.Add("Phòng ban", 120);
        itemListView.Columns.Add("Người quản lý", 120);
        itemListView.Columns.Add("Trạng thái", 120);
        itemListView.SelectedIndexChanged += itemSelectionChanged;

        SplitContainer listSplit = new SplitContainer
        {
            Size = new Size(820, 650), Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal, SplitterDistance = 370
        };
        taskListView = new ListView
        {
            Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true
        };
        taskListView.Columns.Add("Mã công việc", 100);
        taskListView.Columns.Add("Tên công việc", 250);
        taskListView.Columns.Add("Hoàn thành (%)", 120);
        taskListView.Columns.Add("Trạng thái", 130);
        GroupBox taskGroup = new GroupBox { Text = "Công việc của dự án đang chọn", Dock = DockStyle.Fill };
        taskGroup.Controls.Add(taskListView);
        listSplit.Panel1.Controls.Add(itemListView);
        listSplit.Panel2.Controls.Add(taskGroup);
        mainSplit.Panel1.Controls.Add(listSplit);
        mainSplit.Panel1.Controls.Add(buttons);
        mainSplit.Panel1.Controls.Add(searchPanel);
        mainSplit.Panel2.Controls.Add(editor);
        Controls.Add(mainSplit);
        Shown += formShown;
    }
}
