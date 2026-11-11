using System.Drawing.Drawing2D;

namespace GUI.Controls;

internal sealed class ManagementMenuHeader : Panel
{
    public ManagementMenuHeader()
    {
        BackColor = Color.FromArgb(247, 249, 251);
        Dock = DockStyle.Fill;
        Margin = Padding.Empty;
        DoubleBuffered = true;
        AccessibleName = "Hệ thống quản lý nhân sự";
    }

    protected override void OnPaint(PaintEventArgs args)
    {
        base.OnPaint(args);
        int height = ClientSize.Height;
        int brand_width = ClientSize.Width / 4;
        var graphics = args.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Dải cong nối phần thương hiệu với nền xanh của menu.
        using var path = new GraphicsPath();
        path.AddLine(0, 0, 44, 0);
        path.AddBezier(44, 0, 92, height - 12, brand_width - 62, height - 60, brand_width, height);
        path.AddLine(brand_width, height, 0, height);
        path.CloseFigure();

        using var navy = new SolidBrush(Color.FromArgb(36, 54, 71));
        graphics.FillPath(navy, path);
        using var accent = new SolidBrush(Color.FromArgb(102, 224, 207));
        graphics.FillEllipse(accent, 25, height - 48, 14, 14);

        using var brand_font = new Font("Segoe UI", 19, FontStyle.Bold);
        TextRenderer.DrawText(graphics, "QLNS", brand_font,
            new Rectangle(48, height - 63, 120, 44), Color.White,
            TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        using var title_font = new Font("Segoe UI Light", 29);
        TextRenderer.DrawText(graphics, "Hệ thống quản lý nhân sự", title_font,
            new Rectangle(brand_width + 10, 0, Math.Max(0, ClientSize.Width - brand_width - 36), height),
            Color.FromArgb(36, 54, 71), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
    }
}
