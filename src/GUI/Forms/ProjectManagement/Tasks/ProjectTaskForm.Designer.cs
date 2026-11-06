#nullable enable
namespace GUI.Forms.ProjectManagement.Tasks;

partial class ProjectTaskForm
{
    private TextBox idTextBox = null!;
    private TextBox searchTextBox = null!;
    private ListView itemListView = null!;
    private ComboBox projectDetailIdComboBox = null!;
    private TextBox nameTextBox = null!;
    private TextBox descriptionTextBox = null!;
    private TextBox taskTypeTextBox = null!;
    private ComboBox priorityComboBox = null!;
    private DateTimePicker startDatePicker = null!;
    private DateTimePicker deadlinePicker = null!;
    private DateTimePicker completedDatePicker = null!;
    private NumericUpDown estimatedHoursInput = null!;
    private NumericUpDown completionRateInput = null!;
    private ComboBox statusComboBox = null!;
    private TextBox notesTextBox = null!;

    private void initializeControls()
    {
        Text = "Công việc";
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
        projectDetailIdComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        nameTextBox = new TextBox { MaxLength = 150 };
        descriptionTextBox = new TextBox { MaxLength = 1000 };
        taskTypeTextBox = new TextBox { MaxLength = 100 };
        priorityComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        startDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
        deadlinePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
        completedDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
        estimatedHoursInput = new NumericUpDown { DecimalPlaces = 2, Maximum = 99999999.99m, ThousandsSeparator = true };
        completionRateInput = new NumericUpDown { DecimalPlaces = 2, Maximum = 100m, ThousandsSeparator = true };
        statusComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        notesTextBox = new TextBox { MaxLength = 1000 };
        priorityComboBox.Items.AddRange(new object[] { "Low", "Normal", "High" });
        statusComboBox.Items.AddRange(new object[] { "Not started", "In progress", "Completed", "Cancelled" });
        ProjectFormControls.addField(editor, "Mã (tự động, không nhập)", idTextBox);
        ProjectFormControls.addField(editor, "Chi tiết dự án", projectDetailIdComboBox);
        ProjectFormControls.addField(editor, "Tên công việc", nameTextBox);
        ProjectFormControls.addField(editor, "Mô tả", descriptionTextBox);
        ProjectFormControls.addField(editor, "Loại công việc", taskTypeTextBox);
        ProjectFormControls.addField(editor, "Ưu tiên", priorityComboBox);
        ProjectFormControls.addField(editor, "Ngày bắt đầu", startDatePicker);
        ProjectFormControls.addField(editor, "Hạn hoàn thành", deadlinePicker);
        ProjectFormControls.addField(editor, "Ngày hoàn thành", completedDatePicker);
        ProjectFormControls.addField(editor, "Số giờ dự kiến", estimatedHoursInput);
        ProjectFormControls.addField(editor, "Hoàn thành (%)", completionRateInput);
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
        itemListView.Columns.Add("Chi tiết dự án", 120);
        itemListView.Columns.Add("Tên công việc", 120);
        itemListView.Columns.Add("Mô tả", 120);
        itemListView.Columns.Add("Loại công việc", 120);
        itemListView.Columns.Add("Ưu tiên", 120);
        itemListView.Columns.Add("Ngày bắt đầu", 120);
        itemListView.Columns.Add("Hạn hoàn thành", 120);
        itemListView.Columns.Add("Ngày hoàn thành", 120);
        itemListView.Columns.Add("Số giờ dự kiến", 120);
        itemListView.Columns.Add("Hoàn thành (%)", 120);
        itemListView.Columns.Add("Trạng thái", 120);
        itemListView.Columns.Add("Ghi chú", 120);
        itemListView.SelectedIndexChanged += itemSelectionChanged;

        mainSplit.Panel1.Controls.Add(itemListView);
        mainSplit.Panel1.Controls.Add(buttons);
        mainSplit.Panel1.Controls.Add(searchPanel);
        mainSplit.Panel2.Controls.Add(editor);
        Controls.Add(mainSplit);
        Shown += formShown;
    }
}
