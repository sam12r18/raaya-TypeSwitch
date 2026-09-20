using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RaayaTypeSwitch;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly TypingEngine _engine = new();
    private readonly KeyboardHook _hook;
    private readonly NotifyIcon _tray;
    private readonly string _logPath;

    public TrayApplicationContext(string logPath)
    {
        _logPath = logPath;
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

            if (_tray is not null)
            {
                _tray.Text = enabled.Checked
                    ? "Raaya TypeSwitch - فعال"
                    : "Raaya TypeSwitch - غیرفعال";
            }
        };

        var openLog = new ToolStripMenuItem("باز کردن لاگ");
        openLog.Click += (_, _) =>
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"باز کردن فایل لاگ ممکن نشد:\n{ex.Message}\n\n{_logPath}",
                    "Raaya TypeSwitch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        };

        var about = new ToolStripMenuItem("درباره");
        about.Click += (_, _) => MessageBox.Show(
            "Raaya TypeSwitch MVP\n" +
            "اصلاح خودکار تایپ فارسی ↔ انگلیسی با Layout اشتباه.\n\n" +
            "این برنامه پنجره اصلی ندارد و از System Tray اجرا می‌شود.",
            "Raaya TypeSwitch",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        var exit = new ToolStripMenuItem("خروج");
        exit.Click += (_, _) => ExitThread();

        menu.Items.Add(enabled);
        menu.Items.Add(openLog);
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
            File.AppendAllText(
                _logPath,
                $"[{DateTimeOffset.Now:O}] Creating keyboard hook...{Environment.NewLine}",
                Encoding.UTF8);

            _hook.Start();

            File.AppendAllText(
                _logPath,
                $"[{DateTimeOffset.Now:O}] Keyboard hook started successfully.{Environment.NewLine}",
                Encoding.UTF8);

            _tray.BalloonTipTitle = "Raaya TypeSwitch";
            _tray.BalloonTipText = "برنامه اجرا شد و در System Tray فعال است.";
            _tray.ShowBalloonTip(2500);
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(
                    _logPath,
                    $"[{DateTimeOffset.Now:O}] Keyboard hook failed:{Environment.NewLine}{ex}{Environment.NewLine}",
                    Encoding.UTF8);
            }
            catch
            {
            }

            MessageBox.Show(
                $"Keyboard hook اجرا نشد:\n{ex.Message}\n\nLog:\n{_logPath}",
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
