// Интерфейс Windows-версии: значок у часов, панель над ним, окно настроек.
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace ProxyTunnel;

static class Palette
{
    public static readonly Color Green = Color.FromArgb(65, 229, 177);
    public static readonly Color Red = Color.FromArgb(255, 138, 148);
    public static readonly Color Orange = Color.FromArgb(255, 191, 117);
    public static readonly Color Brand = Color.FromArgb(94, 240, 195);
    public static readonly Color Gray = Color.DarkGray;
    public static readonly Color Accent = Color.Gainsboro;
    public static readonly Color Ink = Color.WhiteSmoke;
    public static readonly Color Sub = Color.Silver;
    public static readonly Color Canvas = Color.FromArgb(36, 36, 36);
    public static readonly Color Night = Color.FromArgb(32, 32, 32);
    public static readonly Color Raised = Color.FromArgb(49, 49, 49);
    public static readonly Color Line = Color.FromArgb(69, 69, 69);
    public static readonly Color Mist = Color.FromArgb(207, 207, 207);
    public static readonly Color Selected = Color.FromArgb(65, 65, 65);
    public static readonly Color Field = Color.FromArgb(41, 41, 41);
    public static readonly Color StrongLine = Color.DimGray;
    public static readonly Font Text = new("Segoe UI", 10f);
    public static readonly Font Small = new("Segoe UI", 9.5f);
    public static readonly Font Bold = new("Segoe UI Semibold", 10f);
    public static readonly Font Header = new("Segoe UI Semibold", 13f);
    public static readonly Font Title = new("Segoe UI Semibold", 19f);
}

static class Ui
{
    public static Label L(string text, Font? font = null, Color? color = null, int width = 480) => new()
    {
        Text = text, BackColor = Color.Transparent, AutoSize = true, MaximumSize = new Size(width, 0), Font = font ?? Palette.Text,
        ForeColor = color ?? Palette.Ink, Margin = new Padding(0, 2, 0, 2)
    };

    public static FlowLayoutPanel Row(params Control[] cs)
    {
        var p = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 2, 0, 2)
        };
        foreach (var control in cs)
            p.Controls.Add(control is TextBox or NumericUpDown ? new InputFrame(control) : control);
        return p;
    }

    public static Label Caption(string text) => new()
    {
        Text = text, AutoSize = false, Width = 226, Height = 36, TextAlign = ContentAlignment.MiddleLeft,
        Font = Palette.Text
    };

    public static FlowLayoutPanel Column(int width) => new()
    {
        FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false,
        AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(width, 0),
        Margin = new Padding(0)
    };

    public static void StyleButton(Button button, bool primary = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = primary ? Palette.StrongLine : Palette.Line;
        button.FlatAppearance.MouseOverBackColor = primary ? Color.DimGray : Palette.Selected;
        button.FlatAppearance.MouseDownBackColor = button.FlatAppearance.MouseOverBackColor;
        button.BackColor = primary ? Palette.Selected : Palette.Raised;
        button.ForeColor = Palette.Ink;
        button.MinimumSize = new Size(0, 32);
        button.Padding = new Padding(7, 0, 7, 0);
        button.Cursor = Cursors.Hand;
    }

    public static FlowLayoutPanel Card(string title, int width)
    {
        var card = new SurfaceCard
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(width + 28, 0), FlowDirection = FlowDirection.TopDown,
            WrapContents = false, BackColor = Palette.Raised
        };
        card.Padding = new Padding(14, 8, 14, 9);
        card.Margin = new Padding(0, 0, 0, 7);
        if (title.Length > 0)
        {
            var label = L(title, Palette.Header, width: width);
            label.Margin = new Padding(0, 0, 0, 3);
            card.Controls.Add(label);
        }
        return card;
    }

    public static FlowLayoutPanel Brand()
    {
        var mark = new BrandMark { Margin = new Padding(0, 0, 10, 0) };
        var name = L("Proxy Tunnel", Palette.Header, Color.White, 184);
        name.Margin = new Padding(0, 4, 0, 0);
        return Row(mark, name);
    }

    public static LinkLabel Link(string title) => new()
    {
        Text = title, AutoSize = true, Font = Palette.Bold, LinkColor = Palette.Accent,
        ActiveLinkColor = Palette.Ink, VisitedLinkColor = Palette.Accent, LinkBehavior = LinkBehavior.HoverUnderline,
        Margin = new Padding(0, 5, 18, 5), Cursor = Cursors.Hand
    };

    public static DialogResult Prompt(Form? owner, string message, string title, string? confirm = null)
    {
        using var dialog = new TunnelWindow
        {
            Text = title, BackColor = Palette.Canvas, Font = Palette.Text,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
            StartPosition = owner == null ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent,
            AutoScaleDimensions = new SizeF(96, 96), AutoScaleMode = AutoScaleMode.Dpi,
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        var content = Column(400);
        content.Padding = new Padding(18);
        var description = L(message, width: 364);
        description.Margin = new Padding(0, 0, 0, 14);
        content.Controls.Add(description);
        var ok = new ActionButton { Text = confirm ?? "Закрыть", AutoSize = true, DialogResult = DialogResult.OK };
        StyleButton(ok, primary: true);
        var actions = Row();
        if (confirm != null)
        {
            var cancel = new ActionButton { Text = "Отмена", AutoSize = true, DialogResult = DialogResult.Cancel };
            StyleButton(cancel);
            actions.Controls.Add(cancel);
            dialog.CancelButton = cancel;
            dialog.AcceptButton = cancel;
        }
        else
        {
            dialog.AcceptButton = ok;
            dialog.CancelButton = ok;
        }
        actions.Controls.Add(ok);
        content.Controls.Add(actions);
        dialog.Controls.Add(content);
        return dialog.ShowDialog(owner);
    }

    public static void Open(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
    }

    public static string Mark(Step s) => s.Running ? "…" : s.Ok == true ? "✓" : s.Ok == false ? "✗" : "○";
    public static Color MarkColor(Step s) => s.Running ? Palette.Accent : s.Ok == true ? Palette.Green : s.Ok == false ? Palette.Red : Palette.Gray;

    /// Список шагов: значок, название, пояснение под ним.
    public static void FillSteps(FlowLayoutPanel panel, IEnumerable<Step> steps, int width)
    {
        panel.SuspendLayout();
        while (panel.Controls.Count > 0) panel.Controls[0].Dispose();
        foreach (var s in steps)
        {
            var title = L($"{Mark(s)}  {s.Title}", Palette.Bold, MarkColor(s), width);
            title.Margin = new Padding(0, 4, 0, 0);
            panel.Controls.Add(title);
            if (s.Detail.Length > 0)
            {
                var d = L(s.Detail, Palette.Small, Palette.Sub, width - 18);
                d.Margin = new Padding(18, 0, 0, 2);
                panel.Controls.Add(d);
            }
        }
        ScaleNewChildren(panel);
        panel.ResumeLayout();
    }

    public static void ScaleNewChildren(Control parent)
    {
        if (!parent.IsHandleCreated || parent.DeviceDpi == 96) return;
        var scale = parent.DeviceDpi / 96f;
        foreach (Control child in parent.Controls)
            child.Scale(new SizeF(scale, scale));
    }
}

sealed class BrandMark : Control
{
    public BrandMark()
    {
        Size = new Size(32, 32);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.ScaleTransform(Width / 32f, Height / 32f);
        using var route = new Pen(Palette.Brand, 2.3f) { LineJoin = LineJoin.Round };
        for (int i = 0; i < 3; i++)
            g.DrawEllipse(route, 2 + 6 * i, 10 - 3 * i, 16, 18);
    }
}

sealed class AboutForm : TunnelWindow
{
    static AboutForm? shown;

    public static void ShowOnce()
    {
        if (shown == null || shown.IsDisposed) shown = new AboutForm();
        shown.Show();
        shown.Activate();
    }

    AboutForm()
    {
        const int W = 320;
        Text = "О программе Proxy Tunnel";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Palette.Canvas;
        Padding = new Padding(18);
        Font = Palette.Text;

        var col = Ui.Column(W);
        var brand = Ui.Brand();
        brand.Margin = new Padding(0, 0, 0, 10);
        col.Controls.Add(brand);
        col.Location = new Point(Padding.Left, Padding.Top);
        col.Controls.Add(Ui.L("Отправляет Claude и выбранные сайты через ваш сервер за границей. Всё остальное работает напрямую, как без программы.", width: W));
        col.Controls.Add(Ui.L("1. Настройки → адрес сервера → «Настроить сервер». Это один раз.", width: W));
        col.Controls.Add(Ui.L("2. На панели нажмите «Подключить».", width: W));
        col.Controls.Add(Ui.L("3. Заново откройте приложения и терминалы, которые будут работать через туннель.", width: W));
        col.Controls.Add(Ui.L("Список сайтов находится в разделе «Маршрутизация». Если что-то не так — загляните в журнал.", Palette.Small, Palette.Sub, W));
        Controls.Add(col);
    }
}

/// Панель над значком: состояние туннеля и команды.
sealed class StatusForm : TunnelWindow
{
    readonly Core core;
    readonly TrayApp app;
    const int W = 340;
    bool showDiagnostics;
    readonly FlowLayoutPanel body = Ui.Column(W);

    public StatusForm(Core core, TrayApp app)
    {
        this.core = core;
        this.app = app;
        Text = "Proxy Tunnel";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Palette.Canvas;
        DoubleBuffered = true;
        ResizeRedraw = true;
        Padding = new Padding(6);
        Font = Palette.Text;
        body.Location = new Point(Padding.Left, Padding.Top);
        Controls.Add(body);
        Deactivate += (_, _) => Hide();
        core.Changed += Rebuild;
        Rebuild();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (Width < 2 || Height < 2) return;
        using var outline = Surface.Round(ClientRectangle, 9 * DeviceDpi / 96f);
        Region = new Region(outline);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width < 2 || Height < 2) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var outline = Surface.Round(new RectangleF(.5f, .5f, Width - 1, Height - 1), 9 * DeviceDpi / 96f);
        using var stroke = new Pen(Color.FromArgb(76, 76, 76));
        e.Graphics.DrawPath(stroke, outline);
    }

    public void ShowNearTray()
    {
        showDiagnostics = false;
        Rebuild();
        Show();
        var wa = Screen.PrimaryScreen!.WorkingArea;
        Location = new Point(wa.Right - Width - 12, wa.Bottom - Height - 12);
        Activate();
    }

    void Rebuild()
    {
        if (IsDisposed) return;
        body.SuspendLayout();
        while (body.Controls.Count > 0) body.Controls[0].Dispose();

        var state = GetStatus();
        var mark = new BrandMark { Size = new Size(25, 25), BackColor = Palette.Canvas, Margin = new Padding(0, 2, 7, 0) };
        var name = Ui.L("Proxy Tunnel", Palette.Bold, Color.White);
        name.Margin = new Padding(0, 5, 0, 0);
        var heading = Ui.Row(mark, name);
        heading.Anchor = AnchorStyles.Left;
        var action = new ActionButton
        {
            Text = !core.Config.IsComplete ? "Настроить сервер…" : core.Enabled ? "Отключить" : "Подключить",
            AutoSize = true, Enabled = !core.Busy, Font = Palette.Small, Anchor = AnchorStyles.Right
        };
        Ui.StyleButton(action);
        action.BackColor = Palette.Raised;
        action.FlatAppearance.BorderColor = Palette.Line;
        action.FlatAppearance.MouseOverBackColor = Color.FromArgb(61, 61, 61);
        action.Click += async (_, _) =>
        {
            if (core.Config.IsComplete) await core.Toggle();
            else { Hide(); app.ShowSettings(); }
        };
        var header = new TableLayoutPanel
        {
            Width = W, Height = 44, ColumnCount = 2, RowCount = 1, Padding = new Padding(7, 0, 4, 0),
            Margin = new Padding(0, 0, 0, 4)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.Controls.Add(heading, 0, 0);
        header.Controls.Add(action, 1, 0);
        body.Controls.Add(header);

        var status = new TableLayoutPanel
        {
            ColumnCount = 2, RowCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(W, 52), BackColor = Palette.Raised, Padding = new Padding(0, 6, 6, 6),
            Margin = new Padding(0, 0, 0, 3)
        };
        var marker = new Panel { Width = 3, Height = 20, Anchor = AnchorStyles.Left, BackColor = state.Color,
            Margin = new Padding(1, 0, 9, 0) };
        var summary = Ui.Column(W - 24);
        summary.Controls.Add(Ui.L(state.Title, Palette.Bold, Color.White, W - 24));
        if (state.Detail.Length > 0)
        {
            var detail = Ui.L(state.Detail, Palette.Small, Palette.Mist, W - 24);
            detail.MaximumSize = new Size(W - 24, detail.Font.Height * 2 + 3);
            detail.AutoEllipsis = true;
            summary.Controls.Add(detail);
        }
        status.Controls.Add(marker, 0, 0);
        status.Controls.Add(summary, 1, 0);
        body.Controls.Add(status);

        if (!core.Enabled) showDiagnostics = false;
        if (showDiagnostics && core.Steps.Count > 0)
        {
            var diagnostics = Ui.Column(W);
            diagnostics.Padding = new Padding(8, 4, 8, 4);
            int listWidth = W - 16 - SystemInformation.VerticalScrollBarWidth;
            var steps = Ui.Column(listWidth);
            Ui.FillSteps(steps, core.Steps, listWidth);
            var scroll = new Panel { AutoScroll = true, Width = W - 16, Margin = Padding.Empty };
            scroll.Height = Math.Min(158, steps.GetPreferredSize(new Size(listWidth, 0)).Height);
            ThemeScrollbars(scroll);
            scroll.Controls.Add(steps);
            diagnostics.Controls.Add(scroll);
            if (core.CheckedAt is { } time && !core.Checking)
                diagnostics.Controls.Add(Ui.L($"Проверено в {time:HH:mm}", Palette.Small, Palette.Sub, W));
            body.Controls.Add(diagnostics);
        }

        if (core.Message.Length > 0)
            body.Controls.Add(Ui.L(core.Message, Palette.Small, Palette.Mist, W - 12));

        var tools = Ui.Column(W);
        tools.Padding = new Padding(4, 0, 4, 2);
        if (core.Config.IsComplete && core.Enabled)
        {
            var reconnect = PanelLink("Переподключить");
            reconnect.LinkClicked += async (_, _) => await core.Reconnect();
            var check = PanelLink("Проверить");
            check.LinkClicked += async (_, _) => await core.Refresh();
            reconnect.Enabled = check.Enabled = !core.Busy;
            var commands = Ui.Row(reconnect, check);
            if (core.Steps.Count > 0)
            {
                var toggle = PanelLink(showDiagnostics ? "Свернуть" : "Диагностика");
                toggle.LinkClicked += (_, _) => { showDiagnostics = !showDiagnostics; Rebuild(); };
                commands.Controls.Add(toggle);
            }
            tools.Controls.Add(commands);
        }

        var settings = PanelLink("Настройки");
        settings.LinkClicked += (_, _) => { Hide(); app.ShowSettings(); };
        var log = PanelLink("Журнал");
        log.LinkClicked += (_, _) => app.OpenLog();
        var help = PanelLink("О программе");
        help.LinkClicked += (_, _) => { Hide(); AboutForm.ShowOnce(); };
        var footer = Ui.Row(settings, log, help);
        footer.Padding = new Padding(3, 4, 0, 2);
        tools.Controls.Add(new Panel { Height = 1, Width = W - 9, BackColor = Color.FromArgb(57, 57, 57), Margin = new Padding(0, 3, 0, 2) });
        tools.Controls.Add(footer);
        body.Controls.Add(tools);
        Ui.ScaleNewChildren(body);
        body.ResumeLayout();
        if (Visible)
        {
            var wa = Screen.PrimaryScreen!.WorkingArea;
            Location = new Point(wa.Right - Width - 12, wa.Bottom - Height - 12);
        }
    }

    static LinkLabel PanelLink(string title)
    {
        var link = Ui.Link(title);
        link.Font = Palette.Small;
        link.Margin = new Padding(0, 2, 9, 2);
        link.LinkColor = link.VisitedLinkColor = Palette.Mist;
        link.ActiveLinkColor = Color.White;
        link.DisabledLinkColor = Palette.Mist;
        return link;
    }

    (string Title, string Detail, Color Color) GetStatus()
    {
        if (!core.Config.IsComplete)
            return ("Туннель не настроен", "Укажите адрес своего сервера, чтобы начать.", Color.White);
        if (!core.Enabled)
            return ("Туннель выключен", "Выбранные приложения и сайты подключаются напрямую.", Color.White);
        if (!core.Tunnel.Running && core.Tunnel.LastError.Length > 0)
            return ("Соединение прервано", core.Tunnel.LastError, Palette.Red);
        if (core.Checking)
            return ("Проверка подключения…", "", Palette.Accent);
        if (core.VerdictOK)
        {
            var exit = core.Steps.Count > 1 ? core.Steps[1].Detail : "";
            return ("Туннель подключён", exit.Length > 0 ? $"Выход: {exit}" : "", Palette.Green);
        }
        if (core.Steps.Count == 0)
            return ("Подключение…", "", Palette.Accent);
        if (core.Steps.Count > 1 && core.Steps[^1].Ok == false && core.Steps.Take(core.Steps.Count - 1).All(s => s.Ok == true))
            return ("Туннель подключён частично", "Проверьте маршрутизацию сайтов в браузере.", Color.PeachPuff);
        var failure = core.Steps.FirstOrDefault(s => s.Ok == false);
        string detail = failure == null ? "Проверьте результаты диагностики." :
            string.IsNullOrEmpty(failure.Detail) ? failure.Title : $"{failure.Title}: {failure.Detail}";
        return ("Соединение не готово", detail, Color.MistyRose);
    }
}

/// Окно настроек: сервер, настройка сервера, доступ к серверу, список сайтов, дополнительно.
sealed class SettingsForm : TunnelWindow
{
    readonly Core core;
    readonly TrayApp app;
    const int W = 614;
    readonly Panel viewport = new() { Dock = DockStyle.Fill, AutoScroll = true };
    readonly FlowLayoutPanel navigation = Ui.Row();
    readonly List<(Button Button, FlowLayoutPanel Page)> pages = new();
    int selectedPage = -1;

    readonly TextBox host = new() { Width = 459 };
    readonly NumericUpDown sshPort = Num(22);
    readonly Label serverHint = Ui.L("Нет сервера? Подойдёт любой зарубежный VPS на Ubuntu или Debian.", Palette.Small, Palette.Sub, W);

    readonly ModeChip autoMode = new() { Text = "Автоматически", AutoSize = true, Checked = true };
    readonly ModeChip manualMode = new() { Text = "Вручную", AutoSize = true };
    readonly FlowLayoutPanel autoPanel = Ui.Column(W);
    readonly FlowLayoutPanel manualPanel = Ui.Column(W);
    readonly Label access = Ui.L("", Palette.Text, null, W);
    readonly FlowLayoutPanel pwPanel = Ui.Column(W);
    readonly TextBox login = new() { Width = 200, Text = "root" };
    readonly TextBox password = new() { Width = 200, UseSystemPasswordChar = true };
    readonly RouteSwitch makeKey = new() { Text = "Создать ключ для входа и добавить его на сервер", AutoSize = true, Checked = true };
    readonly ActionButton setupButton = new() { AutoSize = true, Font = Palette.Bold };
    readonly FlowLayoutPanel setupSteps = Ui.Column(W);
    readonly Label setupDone = Ui.L("", Palette.Text, Palette.Green, W);
    readonly ActionButton createTunnelKey = new() { Text = "Создать ключ туннеля", AutoSize = true };

    readonly FlowLayoutPanel accessSection = Ui.Card("Способы входа", W);
    readonly FlowLayoutPanel accessEmpty = Ui.Card("", W);
    readonly Label keyState = Ui.L("");
    readonly Label pwState = Ui.L("");
    readonly ActionButton checkConn = new() { Text = "Проверить соединение", AutoSize = true };
    readonly ActionButton disablePw = new() { Text = "Выключить вход по паролю", AutoSize = true };
    readonly FlowLayoutPanel addKeyRow = Ui.Column(W);
    readonly TextBox accessPassword = new() { Width = 200, UseSystemPasswordChar = true };
    readonly Label accessNote = Ui.L("", Palette.Small, Palette.Sub, W);

    readonly TextBox domains = new() { Multiline = true, Width = W, Height = 100, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9.5f) };
    readonly TextBox tunnelUser = new() { Width = 160 };
    readonly NumericUpDown localPort = Num(8888), remotePort = Num(8888), pacPort = Num(8889);
    readonly RouteSwitch autostart = new() { Text = "Запускать при входе в Windows", AutoSize = true };
    readonly RouteSwitch claudeCode = new() { Text = "Claude Code — через туннель", AutoSize = true };
    readonly RouteSwitch codexCli = new() { Text = "Codex в терминале — через туннель", AutoSize = true };
    readonly RouteSwitch geminiCli = new() { Text = "Gemini в терминале — через туннель", AutoSize = true };
    readonly Label note = new()
    {
        Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true,
        Font = Palette.Small, ForeColor = Palette.Sub, Margin = new Padding(0, 0, 12, 0)
    };
    readonly System.Windows.Forms.Timer probeTimer = new() { Interval = 900 };
    bool loaded;

    static NumericUpDown Num(int v)
    {
        var number = new NumericUpDown { Minimum = 1, Maximum = 65535, Value = v, Width = 118, BorderStyle = BorderStyle.None };
        foreach (Control child in number.Controls)
        {
            if (child is TextBox) continue;
            child.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.Clear(Palette.Field);
                using var pen = new Pen(Palette.Sub, 1.4f);
                int x = child.Width / 2, y = child.Height / 4;
                g.DrawLines(pen, new Point[] { new(x - 3, y + 1), new(x, y - 2), new(x + 3, y + 1) });
                y += child.Height / 2;
                g.DrawLines(pen, new Point[] { new(x - 3, y - 1), new(x, y + 2), new(x + 3, y - 1) });
            };
        }
        return number;
    }

    public SettingsForm(Core core, TrayApp app)
    {
        this.core = core;
        this.app = app;
        Text = "Настройки Proxy Tunnel";
        Font = Palette.Text;
        StartPosition = FormStartPosition.CenterScreen;
        var workArea = Screen.GetWorkingArea(this);
        ClientSize = new Size(744, Math.Clamp(workArea.Height * 96 / DeviceDpi - 64, 325, 568));
        MinimumSize = new Size(692, 360);
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Palette.Canvas;
        ForeColor = Palette.Ink;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 49));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 49));
        Controls.Add(layout);

        var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
            BackColor = Palette.Night, Margin = Padding.Empty, Padding = new Padding(12, 2, 8, 2) };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var brand = Ui.Brand();
        brand.Anchor = AnchorStyles.Left;
        navigation.Anchor = AnchorStyles.Right;
        top.Controls.Add(brand, 0, 0);
        top.Controls.Add(navigation, 1, 0);
        layout.Controls.Add(top, 0, 0);
        ThemeScrollbars(viewport);
        ThemeScrollbars(domains);
        viewport.Margin = Padding.Empty;
        layout.Controls.Add(viewport, 0, 1);
        viewport.SizeChanged += (_, _) => CenterPage();

        var server = AddPage("Сервер", "Подключение к серверу", "Укажите адрес и настройте прокси на своём сервере.");
        var endpoint = Ui.Card("Адрес и порт", W);
        var address = Ui.Column(host.Width);
        address.Controls.Add(Ui.L("Адрес сервера", Palette.Small, Palette.Sub));
        address.Controls.Add(new InputFrame(host));
        var port = Ui.Column(sshPort.Width);
        port.Controls.Add(Ui.L("SSH-порт", Palette.Small, Palette.Sub));
        port.Controls.Add(new InputFrame(sshPort));
        port.Margin = new Padding(8, 0, 0, 0);
        endpoint.Controls.Add(Ui.Row(address, port));
        endpoint.Controls.Add(serverHint);
        server.Controls.Add(endpoint);

        var provisioning = Ui.Card("Настройка сервера", W);
        provisioning.Controls.Add(Ui.Row(autoMode, manualMode));
        autoMode.CheckedChanged += (_, _) => Sync();

        autoPanel.Controls.Add(Ui.L("Программа сама зайдёт на сервер (Ubuntu или Debian), поставит прокси и заведёт пользователя, которому разрешён только туннель.", Palette.Text, null, W));
        autoPanel.Controls.Add(access);
        pwPanel.Controls.Add(Ui.Row(Ui.Caption("Логин"), login));
        pwPanel.Controls.Add(Ui.Row(Ui.Caption("Пароль"), password));
        pwPanel.Controls.Add(makeKey);
        pwPanel.Controls.Add(Ui.L("Тогда в следующий раз пароль не понадобится, а вход по паролю можно будет выключить. Пароль нигде не сохраняется.", Palette.Small, Palette.Sub, W));
        autoPanel.Controls.Add(pwPanel);
        setupButton.Click += async (_, _) => await SetupClicked();
        autoPanel.Controls.Add(setupButton);
        autoPanel.Controls.Add(setupSteps);
        autoPanel.Controls.Add(setupDone);
        provisioning.Controls.Add(autoPanel);

        manualPanel.Controls.Add(Ui.L("Если программе на сервер не зайти (вход только через консоль хостинга): скопируйте команду, вставьте её в консоль сервера под root или пользователем с sudo и дождитесь строки «сервер готов». Затем «Сохранить» внизу.", Palette.Text, null, W));
        var copyCmd = new ActionButton { Text = "Скопировать команду для сервера", AutoSize = true };
        copyCmd.Click += (_, _) => CopyCommand();
        createTunnelKey.Click += (_, _) => { var e = Keys.Ensure(Paths.TunnelKey, "proxy-tunnel"); note.Text = e == null ? "Ключ создан. Теперь скопируйте команду для сервера." : "Ключ не создан: " + e; Sync(); };
        manualPanel.Controls.Add(Ui.Row(createTunnelKey, copyCmd));
        provisioning.Controls.Add(manualPanel);
        server.Controls.Add(provisioning);

        // Доступ к серверу
        var security = AddPage("Доступ", "Доступ к серверу", "Проверьте вход по SSH и управляйте доступом.");
        accessSection.Controls.Add(Ui.Row(Ui.Caption("Вход по ключу"), keyState));
        accessSection.Controls.Add(Ui.Row(Ui.Caption("Вход по паролю"), pwState));
        checkConn.Click += async (_, _) => await core.CheckConnection();
        disablePw.Click += async (_, _) => await core.DisablePasswordLogin();
        accessSection.Controls.Add(Ui.Row(checkConn, disablePw));
        addKeyRow.Controls.Add(Ui.L("Входа по ключу нет — его можно создать: программа сделает ключ и добавит его на сервер по паролю.", Palette.Small, null, W));
        var addKey = new ActionButton { Text = "Создать ключ и добавить", AutoSize = true };
        addKey.Click += async (_, _) => { var pw = accessPassword.Text; accessPassword.Text = ""; if (pw.Length > 0) await core.AddAdminKeyNow(pw); };
        addKeyRow.Controls.Add(Ui.Row(Ui.Caption("Пароль"), accessPassword));
        addKeyRow.Controls.Add(Ui.Row(addKey));
        accessSection.Controls.Add(addKeyRow);
        accessSection.Controls.Add(accessNote);
        accessSection.Controls.Add(Ui.L("Выключить вход по паролю можно только когда вход по ключу подтверждён — иначе легко потерять доступ к серверу.", Palette.Small, Palette.Sub, W));
        accessEmpty.Controls.Add(Ui.L("Сначала настройте подключение в разделе «Сервер».", width: W));
        var serverLink = Ui.Link("Открыть настройки сервера");
        serverLink.LinkClicked += (_, _) => SelectPage(0);
        accessEmpty.Controls.Add(serverLink);
        security.Controls.Add(accessSection);
        security.Controls.Add(accessEmpty);

        // Через туннель
        var routing = AddPage("Маршрутизация", "Через туннель", "Выберите приложения и сайты, которые используют прокси.");
        var apps = Ui.Card("Терминал и редакторы", W);
        // Галочки действуют сразу, туннель не переподключают.
        claudeCode.CheckedChanged += async (_, _) => { if (loaded) await core.SetToggle(c => c.ClaudeCode = claudeCode.Checked); };
        codexCli.CheckedChanged += async (_, _) => { if (loaded) await core.SetToggle(c => c.CodexCLI = codexCli.Checked); };
        geminiCli.CheckedChanged += async (_, _) => { if (loaded) await core.SetToggle(c => c.GeminiCLI = geminiCli.Checked); };
        apps.Controls.Add(Ui.L(Core.RestartHint, Palette.Small, Palette.Sub, W));
        apps.Controls.Add(claudeCode);
        apps.Controls.Add(Ui.L("Действует в терминале и VS Code. Российские сайты открываются напрямую.", Palette.Small, Palette.Sub, W));
        apps.Controls.Add(codexCli);
        apps.Controls.Add(Ui.L("Действует в cmd, PowerShell, Git Bash и терминале VS Code, открытых после включения. Расширения VS Code подключаются напрямую.", Palette.Small, Palette.Sub, W));
        apps.Controls.Add(geminiCli);
        apps.Controls.Add(Ui.L("Для Gemini действуют те же правила, что и для Codex.", Palette.Small, Palette.Sub, W));
        routing.Controls.Add(apps);
        var sites = Ui.Card("Сайты", W);
        sites.Controls.Add(Ui.L("Введите по одному домену на строку. Поддомены включаются автоматически. Список применяется после сохранения.", Palette.Small, Palette.Sub, W));
        domains.AccessibleName = "Домены для туннеля";
        domains.Margin = new Padding(0, 5, 0, 0);
        sites.Controls.Add(new InputFrame(domains));
        routing.Controls.Add(sites);

        // Дополнительно
        var advanced = AddPage("Дополнительно", "Дополнительно", "Порты, автозапуск и параметры приложения.");
        var ports = Ui.Card("Параметры туннеля", W);
        ports.Controls.Add(Ui.Row(Ui.Caption("Пользователь туннеля"), tunnelUser));
        ports.Controls.Add(Ui.Row(Ui.Caption("Порт туннеля на компьютере"), localPort));
        ports.Controls.Add(Ui.Row(Ui.Caption("Порт прокси на сервере"), remotePort));
        ports.Controls.Add(Ui.Row(Ui.Caption("Порт PAC"), pacPort));
        var forget = new ActionButton { Text = "Забыть ключ сервера…", AutoSize = true };
        forget.Click += (_, _) =>
        {
            if (Ui.Prompt(this,
                    "Это нужно, только если сервер переустановили и программа пишет, что ключ сервера изменился. При следующем подключении программа примет ключ сервера заново — если в этот момент кто-то подменит сервер, подмену она не заметит.",
                    "Забыть ключ сервера?", "Забыть ключ") == DialogResult.OK)
            {
                core.ForgetHostKey();
                note.Text = core.Message;
            }
        };
        ports.Controls.Add(forget);
        advanced.Controls.Add(ports);

        var appCard = Ui.Card("Приложение", W);
        autostart.CheckedChanged += (_, _) => { if (!loaded) return; try { Autostart.On = autostart.Checked; } catch (Exception e) { note.Text = "Автозапуск: " + e.Message; } };
        appCard.Controls.Add(autostart);

        var save = new ActionButton { Text = "Сохранить", AutoSize = true, Font = Palette.Bold };
        save.Click += (_, _) => Save();
        var cancel = new ActionButton { Text = "Отменить", AutoSize = true };
        cancel.Click += (_, _) => LoadFromConfig();
        foreach (var button in new[] { copyCmd, createTunnelKey, checkConn, disablePw, addKey, forget, cancel })
            Ui.StyleButton(button);
        Ui.StyleButton(setupButton, primary: true);
        Ui.StyleButton(save, primary: true);
        host.PlaceholderText = "IP-адрес или домен";
        foreach (var field in new[] { host, login, password, accessPassword, tunnelUser, domains })
            field.BorderStyle = BorderStyle.None;
        host.AccessibleName = "Адрес сервера";
        sshPort.AccessibleName = "SSH-порт";
        login.AccessibleName = "Логин";
        password.AccessibleName = accessPassword.AccessibleName = "Пароль";
        tunnelUser.AccessibleName = "Пользователь туннеля";
        localPort.AccessibleName = "Порт туннеля на компьютере";
        remotePort.AccessibleName = "Порт прокси на сервере";
        pacPort.AccessibleName = "Порт PAC";
        var quit = Ui.Link("Выйти из Proxy Tunnel");
        quit.LinkColor = quit.VisitedLinkColor = Palette.Red;
        quit.LinkClicked += (_, _) => app.Quit();
        appCard.Controls.Add(Ui.Row(quit));
        appCard.Controls.Add(Ui.L("После выхода трафик пойдёт напрямую.", Palette.Small, Palette.Sub, W));
        advanced.Controls.Add(appCard);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Palette.Night, ColumnCount = 2,
            RowCount = 1, Padding = new Padding(14, 2, 12, 2), Margin = new Padding(0)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        footer.Controls.Add(note, 0, 0);
        footer.Controls.Add(Ui.Row(cancel, save), 1, 0);
        layout.Controls.Add(footer, 0, 2);
        SelectPage(0);

        // фоновая проверка входа по ключу — через секунду после того, как адрес или порт перестали меняться
        host.TextChanged += (_, _) => { if (loaded) { probeTimer.Stop(); probeTimer.Start(); } Sync(); };
        password.TextChanged += (_, _) => Sync();
        login.TextChanged += (_, _) => Sync();
        sshPort.ValueChanged += (_, _) => { if (loaded) { probeTimer.Stop(); probeTimer.Start(); } };
        probeTimer.Tick += async (_, _) => { probeTimer.Stop(); await core.ProbeAccess(Draft()); };

        FormClosing += (_, e) => { if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); } };
        core.Changed += Sync;
        LoadFromConfig();
    }

    FlowLayoutPanel AddPage(string label, string title, string description)
    {
        var page = Ui.Column(W + 28);
        page.Location = new Point(0, 0);
        page.Padding = new Padding(0, 8, 0, 2);
        page.Visible = false;
        var heading = Ui.L(title, Palette.Title, width: W);
        heading.Margin = new Padding(2, 6, 0, 1);
        var subtitle = Ui.L(description, Palette.Small, Palette.Mist, W);
        subtitle.Margin = new Padding(4, 0, 0, 13);
        page.Controls.Add(heading);
        page.Controls.Add(subtitle);
        viewport.Controls.Add(page);

        var button = new ActionButton
        {
            Text = label, TextAlign = ContentAlignment.MiddleLeft, AutoSize = true, Height = 40,
            Margin = new Padding(2, 0, 2, 0), Font = Palette.Bold
        };
        Ui.StyleButton(button);
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Palette.Raised;
        button.FlatAppearance.MouseDownBackColor = Palette.Raised;
        button.BackColor = Palette.Night;
        button.ForeColor = Palette.Mist;
        var index = pages.Count;
        button.Click += (_, _) => SelectPage(index);
        navigation.Controls.Add(button);
        pages.Add((button, page));
        return page;
    }

    void SelectPage(int index)
    {
        if (selectedPage == index) return;
        viewport.SuspendLayout();
        if (selectedPage >= 0)
        {
            var previous = pages[selectedPage];
            previous.Page.Visible = false;
            previous.Button.BackColor = Palette.Night;
            previous.Button.ForeColor = Palette.Mist;
            previous.Button.FlatAppearance.MouseOverBackColor = Palette.Raised;
            previous.Button.AccessibleDescription = null;
        }
        viewport.AutoScrollPosition = Point.Empty;
        var current = pages[index];
        current.Page.Location = Point.Empty;
        current.Page.Visible = true;
        current.Button.BackColor = Palette.Selected;
        current.Button.ForeColor = Palette.Ink;
        current.Button.FlatAppearance.MouseOverBackColor = Palette.Selected;
        current.Button.AccessibleDescription = "Открытый раздел";
        selectedPage = index;
        viewport.ResumeLayout();
        CenterPage();
    }

    void CenterPage()
    {
        if (selectedPage < 0) return;
        var page = pages[selectedPage].Page;
        int x = Math.Max(0, (viewport.ClientSize.Width - page.Width) / 2);
        if (page.Left != x) page.Left = x;
    }

    /// Показ окна: сохранённые настройки подставляются, только если в окне нет несохранённых правок.
    public void ShowWindow()
    {
        if (!Edited()) LoadFromConfig();
        Show();
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
        if (Draft().HostT.Length > 0) _ = core.ProbeAccess(Draft());
    }

    bool Edited() =>
        System.Text.Json.JsonSerializer.Serialize(Draft()) != System.Text.Json.JsonSerializer.Serialize(Clean(core.Config));

    static Config Clean(Config c)
    {
        var x = c.Clone();
        x.Host = x.HostT;
        x.Domains = x.CleanDomains();
        return x;
    }

    Config Draft()
    {
        var c = core.Config.Clone();
        c.Host = host.Text.Trim();
        c.SshPort = (int)sshPort.Value;
        c.User = tunnelUser.Text.Trim().Length > 0 ? tunnelUser.Text.Trim() : "tunnel";
        c.AdminUser = login.Text.Trim().Length > 0 ? login.Text.Trim() : c.AdminUser;
        c.LocalPort = (int)localPort.Value;
        c.RemotePort = (int)remotePort.Value;
        c.PacPort = (int)pacPort.Value;
        c.Domains = domains.Lines.ToList();
        c.Domains = c.CleanDomains();
        return c;
    }

    void LoadFromConfig()
    {
        loaded = false;
        var c = core.Config;
        host.Text = c.Host;
        sshPort.Value = Math.Clamp(c.SshPort, 1, 65535);
        login.Text = c.AdminT;
        tunnelUser.Text = c.User;
        localPort.Value = Math.Clamp(c.LocalPort, 1, 65535);
        remotePort.Value = Math.Clamp(c.RemotePort, 1, 65535);
        pacPort.Value = Math.Clamp(c.PacPort, 1, 65535);
        domains.Lines = c.Domains.ToArray();
        try { autostart.Checked = Autostart.On; } catch { }
        claudeCode.Checked = c.ClaudeCode;
        codexCli.Checked = c.CodexCLI;
        geminiCli.Checked = c.GeminiCLI;
        note.Text = "";
        loaded = true;
        Sync();
    }

    void Save()
    {
        var c = Draft();
        core.Apply(c);
        domains.Lines = c.Domains.ToArray();
        note.Text = c.IsComplete ? "Сохранено. Туннель переподключается." : "Сохранено. Для подключения нужен настроенный сервер.";
    }

    bool AlreadySetUp => core.Config.IsComplete && host.Text.Trim() == core.Config.HostT;

    async Task SetupClicked()
    {
        if (host.Text.Trim().Length == 0)
        {
            // без адреса — коротко подсветить поле адреса и поставить в него курсор
            host.Focus();
            host.BackColor = Color.FromArgb(92, 36, 46);
            await Task.Delay(900);
            host.BackColor = Palette.Field;
            return;
        }
        if (AlreadySetUp && Ui.Prompt(this,
                "Прокси на сервере поставится заново, его настройки перезапишутся. Ключи и вход на сервер не пропадут, но туннель прервётся на несколько секунд. Нужно, если туннель перестал работать или сервер переустановили.",
                "Сервер уже настроен", "Настроить заново") != DialogResult.OK) return;
        var pw = pwPanel.Visible ? password.Text : "";
        note.Text = "";
        await core.SetupServer(Draft(), pw, makeKey.Checked);
        if (core.SetupDone) password.Text = "";
        LoadFromConfig();
    }

    void CopyCommand()
    {
        var pub = Keys.Public(Paths.TunnelKey);
        if (pub == null) { note.Text = "Сначала создайте ключ туннеля."; return; }
        try
        {
            Clipboard.SetText(Ssh.ManualCommand(Draft(), pub).Replace("\n", "\r\n"));
            note.Text = "Команда скопирована. Вставьте её в консоль сервера, дождитесь «сервер готов», затем «Сохранить».";
        }
        catch (Exception e) { note.Text = "Не скопировалось: " + e.Message; }
    }

    /// Обновить видимость и надписи по состоянию программы.
    void Sync()
    {
        if (IsDisposed) return;
        SuspendLayout();
        serverHint.Visible = core.Config.HostT.Length == 0;
        autoPanel.Visible = autoMode.Checked;
        manualPanel.Visible = manualMode.Checked;
        createTunnelKey.Visible = !File.Exists(Paths.TunnelKey);

        var hostSet = host.Text.Trim().Length > 0;
        (access.Text, access.ForeColor) = !hostSet ? ("", Palette.Sub) : core.Access switch
        {
            AccessState.Checking => ("Проверяю вход по ключу…", Palette.Sub),
            AccessState.Key => ("✓ " + core.AccessText, Palette.Green),
            AccessState.NeedPassword => ("🔑 " + core.AccessText, Palette.Orange),
            AccessState.Failed => ("✗ " + core.AccessText, Palette.Red),
            _ => ("", Palette.Sub)
        };
        access.Visible = access.Text.Length > 0;
        pwPanel.Visible = core.Access == AccessState.NeedPassword;
        setupButton.Text = core.SetupBusy ? "Настраиваю…" : AlreadySetUp ? "Настроить заново…" : "Настроить сервер";
        // Кнопка доступна всегда, кроме самой настройки: без адреса она подсветит поле, а ключ или пароль
        // выберет сама настройка (сначала ключ, затем пароль, если он введён).
        setupButton.Enabled = !core.SetupBusy;
        Ui.FillSteps(setupSteps, core.SetupSteps, W);
        setupSteps.Visible = core.SetupSteps.Count > 0;
        setupDone.Text = core.SetupDone ? "Туннель подключён. " + Core.RestartHint : "";
        setupDone.Visible = core.SetupDone;

        accessSection.Visible = core.Config.IsComplete;
        accessEmpty.Visible = !core.Config.IsComplete;
        (keyState.Text, keyState.ForeColor) = core.KeyStable switch
        {
            null => ("не проверен", Palette.Sub),
            true => (core.KeyCheckedHow.Length == 0 ? "стабилен, 3 из 3" : $"работает ({core.KeyCheckedHow})", Palette.Green),
            false => ("не прошёл", Palette.Red)
        };
        (pwState.Text, pwState.ForeColor) = core.PasswordLogin switch
        {
            null => ("неизвестно", Palette.Sub),
            true => ("включён", Palette.Orange),
            false => ("выключен", Palette.Green)
        };
        checkConn.Enabled = !core.AccessBusy;
        disablePw.Enabled = !core.AccessBusy && core.KeyStable == true && core.PasswordLogin != false;
        addKeyRow.Visible = core.KeyDenied || (core.Access == AccessState.NeedPassword && !core.SetupBusy && core.Config.IsComplete);
        accessNote.Text = core.AccessNote;
        accessNote.Visible = core.AccessNote.Length > 0;
        ResumeLayout();
    }
}

/// Значок у часов: левый щелчок — панель, правый — меню.
public sealed class TrayApp : ApplicationContext
{
    readonly Core core;
    readonly NotifyIcon icon;
    readonly StatusForm status;
    SettingsForm? settings;
    readonly Dictionary<string, Icon> icons = new();
    bool quitting;

    public TrayApp()
    {
        core = new Core();
        status = new StatusForm(core, this);
        var menu = new ContextMenuStrip { BackColor = Palette.Night, ForeColor = Palette.Ink, ShowImageMargin = false, Renderer = new TrayMenuRenderer() };
        menu.Items.Add("Открыть", null, (_, _) => status.ShowNearTray());
        menu.Items.Add("Настройки…", null, (_, _) => ShowSettings());
        menu.Items.Add("Журнал", null, (_, _) => OpenLog());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Выйти", null, (_, _) => Quit());
        icon = new NotifyIcon { Visible = true, Text = "Proxy Tunnel", ContextMenuStrip = menu };
        icon.MouseClick += (_, e) =>
        {
            if (e.Button != MouseButtons.Left) return;
            if (status.Visible) status.Hide(); else status.ShowNearTray();
        };
        core.Changed += UpdateIcon;
        UpdateIcon();
        Application.ApplicationExit += (_, _) => { if (!quitting) core.Shutdown(); };
        Microsoft.Win32.SystemEvents.SessionEnding += (_, _) => core.Shutdown();

        _ = core.StartServices();   // туннель при запуске выключен — снимет настройки, оставшиеся после аварийного закрытия
        if (core.Config.IsComplete) status.ShowNearTray();   // сразу показать панель: включают по карточке
        else ShowSettings();
    }

    public void ShowSettings()
    {
        settings ??= new SettingsForm(core, this);
        settings.ShowWindow();
    }

    public void OpenLog()
    {
        if (!File.Exists(Paths.LogFile)) Log.Append("журнал создан");
        Ui.Open(Paths.LogFile);
    }

    public void Quit()
    {
        quitting = true;
        core.Shutdown();
        icon.Visible = false;
        ExitThread();
    }

    void UpdateIcon()
    {
        string state;
        if (!core.Config.IsComplete) state = "setup";
        else if (!core.Enabled) state = "off";
        else if (!core.Tunnel.Running && core.Tunnel.LastError.Length > 0) state = "fail";
        else if (core.Checking || core.Steps.Count == 0) state = "connecting";
        else if (core.VerdictOK) state = "ok";
        else if (core.Steps.Count > 1 && core.Steps[^1].Ok == false && core.Steps.Take(core.Steps.Count - 1).All(s => s.Ok == true)) state = "partial";
        else state = "fail";
        icon.Icon = Make(state);
        icon.Text = state switch
        {
            "ok" => "Proxy Tunnel — подключён",
            "connecting" => "Proxy Tunnel — проверка соединения",
            "off" => "Proxy Tunnel — выключен",
            "setup" => "Proxy Tunnel — не настроен",
            "partial" => "Proxy Tunnel — работает, кроме сайтов в браузере",
            _ => "Proxy Tunnel — соединение не готово"
        };
    }

    Icon Make(string state)
    {
        if (!icons.TryGetValue(state, out var result))
        {
            result = new Icon(typeof(TrayApp), $"Icons.{state}.ico");
            icons.Add(state, result);
        }
        return result;
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var mutex = new Mutex(true, "ProxyTunnel-single-instance", out var first);
        if (!first)
        {
            Ui.Prompt(null, "Proxy Tunnel уже запущен — его значок возле часов.", "Proxy Tunnel");
            return;
        }
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
        Application.Run(new TrayApp());
    }
}
