using DTO.ProjectManagement;

namespace GUI.Forms.ProjectManagement;

// Small layout helpers shared by the four ordinary WinForms screens.
internal static class ProjectFormControls
{
    public static void addField(FlowLayoutPanel editor, string caption, Control input)
    {
        Panel field = new Panel { Width = 320, Height = 62 };
        Label label = new Label { Text = caption, AutoSize = true, Location = new Point(0, 2) };
        input.Location = new Point(0, 25);
        input.Width = 300;
        field.Controls.Add(label);
        field.Controls.Add(input);
        editor.Controls.Add(field);
    }

    public static void bindOptions(ComboBox combo, List<LookupItemDto> options)
    {
        combo.Items.Clear();
        combo.DisplayMember = "name";
        foreach (LookupItemDto item in options)
            combo.Items.Add(item);
        combo.SelectedIndex = -1;
    }

    public static void selectId(ComboBox combo, int id)
    {
        combo.SelectedIndex = -1;
        for (int index = 0; index < combo.Items.Count; index++)
        {
            LookupItemDto? item = combo.Items[index] as LookupItemDto;
            if (item != null && item.id == id)
            {
                combo.SelectedIndex = index;
                break;
            }
        }
    }

    public static int selectedId(ComboBox combo)
    {
        LookupItemDto? item = combo.SelectedItem as LookupItemDto;
        return item == null ? 0 : item.id;
    }

    public static void showError(Exception exception)
    {
        string message = "Không thể thực hiện. Kiểm tra kết nối SQL Server và chạy script 03_ProjectManagement.sql theo README.";
        if (exception is ArgumentException || exception is InvalidOperationException)
            message = exception.Message;
        MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
