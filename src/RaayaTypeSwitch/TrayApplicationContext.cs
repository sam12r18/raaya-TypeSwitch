using System.Drawing;
using System.Windows.Forms;

namespace RaayaTypeSwitch;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly TypingEngine _engine = new();
    private readonly KeyboardHook _hook;
    private readonly NotifyIcon _tray;

    public TrayApplicationContext()
    {
        _hook = new KeyboardHook(_engine);

        var menu = new ContextMenuStrip();

        var enabled = new ToolStripMenuItem("فعال")
        {
            Checked = true,
            CheckOnClick = true
        };

        enabled.CheckedChanged += (_, _) =>
        {
            _engine.Enabled = enabled.Checked;
            _tray.Text = enabled.Checked
                ? "Raaya TypeSwitch - فعال"
                : "Raaya TypeSwitch - غیرفعال";
        };

        var about = new ToolStripMenuItem("درباره");
        about.Click += (_, _) => MessageBox.Show(
            "Raaya TypeSwitch MVP\n" +
            "اصلاح خودکار تایپ فارسی ↔ انگلیسی با Layout اشتباه.\n\n" +
            "نسخه فعلی آزمایشی است و اصلاح خودکار را در پایان کلمه انجام می‌دهد.",
            "Raaya TypeSwitch",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        var exit = new ToolStripMenuItem("خروج");
        exit.Click += (_, _) => ExitThread();

        menu.Items.Add(enabled);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(about);
        menu.Items.Add(exit);

        _tray = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "Raaya TypeSwitch - فعال",
            ContextMenuStrip = menu
        };

        _tray.DoubleClick += (_, _) => enabled.Checked = !enabled.Checked;

        _engine.StatusChanged += status =>
        {
            try
            {
                _tray.BalloonTipTitle = "Raaya TypeSwitch";
                _tray.BalloonTipText = status;
                _tray.ShowBalloonTip(900);
            }
            catch
            {
                // Notification failure must never interrupt typing.
            }
        };

        try
        {
            _hook.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Keyboard hook اجرا نشد:\n{ex.Message}",
                "Raaya TypeSwitch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            ExitThread();
        }
    }

    protected override void ExitThreadCore()
    {
        _hook.Dispose();
        _tray.Visible = false;
        _tray.Dispose();
        base.ExitThreadCore();
    }
}
