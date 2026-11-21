// Общие поверхности, поля и элементы управления настольного клиента.
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace ProxyTunnel;

class TunnelWindow : Form
{
    static readonly Icon AppIcon = new(typeof(TunnelWindow), "Icons.App.ico");

    public TunnelWindow() { Icon = AppIcon; }

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr handle, int attribute, ref int value, int length);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    static extern int SetWindowTheme(IntPtr window, string appName, string? classList);

    public static void ThemeScrollbars(Control control)
    {
        control.HandleCreated += (_, _) => SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        int on = 1, caption = ColorTranslator.ToWin32(Palette.Night);
        DwmSetWindowAttribute(Handle, 20, ref on, sizeof(int));
        DwmSetWindowAttribute(Handle, 35, ref caption, sizeof(int));
    }
}

static class Surface
{
    public static GraphicsPath Round(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();
        float d = Math.Max(1, Math.Min(2 * radius, Math.Min(bounds.Width, bounds.Height)));
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void Paint(Control control, Graphics g, Color fill, Color border, int radius)
    {
        if (control.Width < 2 || control.Height < 2) return;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(control.Parent?.BackColor ?? Palette.Canvas);
        using var path = Round(new RectangleF(0.5f, 0.5f, control.Width - 1, control.Height - 1), radius * control.DeviceDpi / 96f);
        using var brush = new SolidBrush(fill);
        using var pen = new Pen(border);
        g.FillPath(brush, path);
        g.DrawPath(pen, path);
    }
}

sealed class SurfaceCard : FlowLayoutPanel
{
    public SurfaceCard() { DoubleBuffered = true; ResizeRedraw = true; }
    protected override void OnPaintBackground(PaintEventArgs e) =>
        Surface.Paint(this, e.Graphics, BackColor, Palette.Line, 8);
}

sealed class ActionButton : Button
{
    public ActionButton() { DoubleBuffered = true; }
    protected override void OnPaint(PaintEventArgs e)
    {
        var hover = ClientRectangle.Contains(PointToClient(Cursor.Position));
        var fill = !Enabled ? Palette.Raised : hover ? FlatAppearance.MouseOverBackColor : BackColor;
        var border = Focused && ShowFocusCues ? Palette.Accent :
            FlatAppearance.BorderSize == 0 ? fill : FlatAppearance.BorderColor;
        Surface.Paint(this, e.Graphics, fill, border, 6);
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Enabled ? ForeColor : Palette.Sub,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}

sealed class RouteSwitch : CheckBox
{
    public RouteSwitch()
    {
        Padding = new Padding(26, 4, 0, 4);
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        float scale = DeviceDpi / 96f;
        float w = 34 * scale, h = 19 * scale, y = (Height - h) / 2;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var track = Surface.Round(new RectangleF(1, y, w, h), h / 2);
        using var fill = new SolidBrush(Checked ? Palette.Green : Palette.Line);
        using var thumb = new SolidBrush(Checked ? Palette.Night : Palette.Sub);
        g.FillPath(fill, track);
        float knob = h - 6 * scale;
        g.FillEllipse(thumb, Checked ? w - knob - 2 * scale : 4 * scale, y + 3 * scale, knob, knob);
        int left = (int)(w + 10 * scale);
        TextRenderer.DrawText(g, Text, Font, new Rectangle(left, 0, Width - left, Height), ForeColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, ClientRectangle, ForeColor, BackColor);
    }
}

sealed class ModeChip : RadioButton
{
    public ModeChip()
    {
        Appearance = Appearance.Button;
        Padding = new Padding(9, 3, 9, 3);
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Surface.Paint(this, e.Graphics, Checked ? Palette.Selected : Palette.Field,
            Focused && ShowFocusCues ? Palette.Accent : Checked ? Palette.StrongLine : Palette.Line, 8);
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Checked ? Palette.Accent : Palette.Sub,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

sealed class InputFrame : Panel
{
    public InputFrame(Control editor)
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = editor.BackColor = Palette.Field;
        editor.ForeColor = Palette.Ink;
        Padding = new Padding(9, 5, 9, 5);
        Size = new Size(editor.Width, editor.Height + Padding.Vertical);
        editor.Dock = DockStyle.Fill;
        editor.Margin = Padding.Empty;
        Controls.Add(editor);
        Enter += (_, _) => Invalidate();
        Leave += (_, _) => Invalidate();
        Click += (_, _) => editor.Focus();
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        Surface.Paint(this, e.Graphics, BackColor, ContainsFocus ? Palette.Accent : Palette.Line, 8);
}

sealed class TrayMenuRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        using var pen = new Pen(Palette.Line);
        e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        using var pen = new Pen(Palette.Line);
        int y = e.Item.Height / 2;
        e.Graphics.DrawLine(pen, 6, y, e.Item.Width - 6, y);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        using var fill = new SolidBrush(e.Item.Selected ? Palette.Selected : Palette.Night);
        e.Graphics.FillRectangle(fill, new Rectangle(Point.Empty, e.Item.Size));
    }
}
