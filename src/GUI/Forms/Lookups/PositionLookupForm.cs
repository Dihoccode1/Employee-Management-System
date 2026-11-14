using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BUS.HumanResources.Positions;
using DTO.HumanResources.Positions;

namespace GUI.Forms.Lookups;

/// <summary>Form tra cứu Chức vụ: lọc trên ArrayList ở BUS và trả kết quả qua biến static dùng một lần.</summary>
internal sealed class PositionLookupForm : Form
{
    private static PositionDto? _selected_position;
    private readonly PositionService _position_service;
    private readonly ArrayList _positions;
    private readonly TextBox _search_box = new() { Dock = DockStyle.Top, PlaceholderText = "Tìm theo mã, tên hoặc mô tả chức vụ..." };
    private readonly ListView _position_list = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false
    };

    public PositionLookupForm(PositionService position_service, List<PositionDto> positions)
    {
        _position_service = position_service;
        _positions = new ArrayList(positions);
        // Xóa lựa chọn cũ mỗi lần mở lại form tra cứu.
        _selected_position = null;

        Text = "Tra cứu chức vụ";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(700, 420);
        Font = new Font("Segoe UI", 9.5f);

        _position_list.Columns.Add("Mã chức vụ", 120);
        _position_list.Columns.Add("Tên chức vụ", 180);
        _position_list.Columns.Add("Phụ cấp chức vụ", 130);
        _position_list.Columns.Add("Mô tả", 240);

        var select_button = new Button { Text = "Chọn chức vụ", Dock = DockStyle.Bottom, Height = 36 };
        select_button.Click += (_, _) => ConfirmSelection();
        _position_list.DoubleClick += (_, _) => ConfirmSelection();
        _search_box.TextChanged += (_, _) => RenderPositions();
        AcceptButton = select_button;

        Controls.Add(_position_list);
        Controls.Add(_search_box);
        Controls.Add(select_button);
        RenderPositions();
    }

    /// <summary>Form gọi lấy đối tượng chức vụ đã chọn một lần rồi xóa biến static.</summary>
    public static PositionDto? TakeSelection()
    {
        PositionDto? position = _selected_position;
        _selected_position = null;
        return position;
    }

    private void ConfirmSelection()
    {
        if (_position_list.SelectedItems.Count != 1)
            return;

        _selected_position = (PositionDto)_position_list.SelectedItems[0].Tag!;
        DialogResult = DialogResult.OK;
    }

    private void RenderPositions()
    {
        _position_list.BeginUpdate();
        try
        {
            _position_list.Items.Clear();
            foreach (PositionDto position in _position_service.Search(_positions, _search_box.Text))
            {
                _position_list.Items.Add(new ListViewItem([
                    position.Code,
                    position.Name,
                    position.AllowanceAmount.ToString("N0"),
                    position.Description
                ])
                {
                    Tag = position
                });
            }
        }
        finally
        {
            _position_list.EndUpdate();
        }
    }
}
