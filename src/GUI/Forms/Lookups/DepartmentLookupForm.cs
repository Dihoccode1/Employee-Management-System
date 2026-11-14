using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BUS.HumanResources.Departments;
using DTO.HumanResources.Departments;
using GUI.Selection;

namespace GUI.Forms.Lookups;

/// <summary>Form tra cứu Phòng ban: lọc trên ArrayList ở BUS và trả kết quả qua biến static dùng một lần.</summary>
public sealed class DepartmentLookupForm : Form
{
    private readonly DepartmentService _department_service;
    private readonly ArrayList _departments;
    private readonly TextBox _search_box = new() { Dock = DockStyle.Top, PlaceholderText = "Tìm theo mã, tên hoặc mô tả phòng ban..." };
    private readonly ListView _department_list = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false
    };

    public DepartmentLookupForm(DepartmentService department_service, List<DepartmentDto> departments)
    {
        _department_service = department_service;
        _departments = new ArrayList(departments);
        // Xóa lựa chọn cũ ngay khi mở lại form tra cứu để tránh dùng nhầm kết quả trước đó.
        LookupSelectionStore.SetDepartment(null);

        Text = "Tra cứu phòng ban";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(680, 420);
        Font = new Font("Segoe UI", 9.5f);

        _department_list.Columns.Add("Mã phòng ban", 120);
        _department_list.Columns.Add("Tên phòng ban", 220);
        _department_list.Columns.Add("Mô tả", 310);

        var select_button = new Button { Text = "Chọn phòng ban", Dock = DockStyle.Bottom, Height = 36 };
        select_button.Click += (_, _) => ConfirmSelection();
        _department_list.DoubleClick += (_, _) => ConfirmSelection();
        _search_box.TextChanged += (_, _) => RenderDepartments();
        AcceptButton = select_button;

        Controls.Add(_department_list);
        Controls.Add(_search_box);
        Controls.Add(select_button);
        RenderDepartments();
    }

    /// <summary>Form gọi lấy đối tượng phòng ban đã chọn một lần rồi xóa biến static.</summary>
    public static DepartmentDto? TakeSelection() => LookupSelectionStore.TakeDepartment();

    private void ConfirmSelection()
    {
        if (_department_list.SelectedItems.Count != 1)
            return;

        LookupSelectionStore.SetDepartment((DepartmentDto)_department_list.SelectedItems[0].Tag!);
        DialogResult = DialogResult.OK;
    }

    private void RenderDepartments()
    {
        _department_list.BeginUpdate();
        try
        {
            _department_list.Items.Clear();
            foreach (DepartmentDto department in _department_service.Search(_departments, _search_box.Text))
            {
                _department_list.Items.Add(new ListViewItem([
                    department.Code,
                    department.Name,
                    department.Description
                ])
                {
                    Tag = department
                });
            }
        }
        finally
        {
            _department_list.EndUpdate();
        }
    }
}
