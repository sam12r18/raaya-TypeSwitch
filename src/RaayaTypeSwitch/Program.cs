using System.Text;
using System.Windows.Forms;

namespace RaayaTypeSwitch;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RaayaTypeSwitch",
            "startup.log");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(
                logPath,
                $"[{DateTimeOffset.Now:O}] Starting Raaya TypeSwitch{Environment.NewLine}",
                Encoding.UTF8);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => LogFatal(logPath, "UI thread exception", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    LogFatal(logPath, "Unhandled exception", ex);
            };

            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext(logPath));
        }
        catch (Exception ex)
        {
            LogFatal(logPath, "Startup failure", ex);

            MessageBox.Show(
                $"Raaya TypeSwitch نتوانست اجرا شود.\n\n{ex.Message}\n\nLog:\n{logPath}",
                "Raaya TypeSwitch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static void LogFatal(string logPath, string title, Exception ex)
    {
        try
        {
            File.AppendAllText(
                logPath,
                $"[{DateTimeOffset.Now:O}] {title}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}",
                Encoding.UTF8);
        }
        catch
        {
            // Logging must not cause another startup failure.
        }
    }
}
