using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace GUI.Controls;

internal sealed class ManagementMenuTile : Button
{
    private readonly Bitmap _icon;
    private readonly Font _label_font = new("Segoe UI", 11);

    public ManagementMenuTile(string label, string glyph, Color accent)
    {
        _icon = CreateIcon(glyph, accent);
        Text = label;
        Font = _label_font;
        AccessibleName = label.Replace('\n', ' ');
        Image = _icon;
        ImageAlign = ContentAlignment.MiddleCenter;
        TextAlign = ContentAlignment.MiddleCenter;
        TextImageRelation = TextImageRelation.ImageAboveText;
        UseVisualStyleBackColor = false;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 1;
        FlatAppearance.BorderColor = Color.FromArgb(65, 88, 107);
        FlatAppearance.MouseOverBackColor = Color.FromArgb(58, 82, 103);
        FlatAppearance.MouseDownBackColor = Color.FromArgb(49, 71, 90);
        BackColor = Color.FromArgb(44, 65, 83);
        ForeColor = Color.White;
        Cursor = Cursors.Hand;
        Dock = DockStyle.Fill;
        Margin = new Padding(8);
        Padding = new Padding(10);
    }

    private static Bitmap CreateIcon(string glyph, Color accent)
    {
        var icon = new Bitmap(72, 80);
        using var graphics = Graphics.FromImage(icon);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        using var background = new SolidBrush(Color.FromArgb(38, accent));
        graphics.FillEllipse(background, 3, 1, 66, 66);
        using var foreground = new SolidBrush(accent);
        using var font = new Font("Segoe MDL2 Assets", 32, GraphicsUnit.Pixel);
        using var alignment = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        graphics.DrawString(glyph, font, foreground, new RectangleF(3, 1, 66, 66), alignment);
        return icon;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _icon.Dispose();
            _label_font.Dispose();
        }
        base.Dispose(disposing);
    }
}
