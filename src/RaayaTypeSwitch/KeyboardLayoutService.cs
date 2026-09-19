using System.Text;

namespace RaayaTypeSwitch;

internal sealed class KeyboardLayoutService
{
    private const string EnglishUsKlid = "00000409";
    private const string PersianKlid = "00000429";

    public IntPtr EnglishLayout { get; } = NativeMethods.LoadKeyboardLayout(EnglishUsKlid, 0);
    public IntPtr PersianLayout { get; } = NativeMethods.LoadKeyboardLayout(PersianKlid, 0);

    public IntPtr GetForegroundLayout()
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return NativeMethods.GetKeyboardLayout(0);

        var threadId = NativeMethods.GetWindowThreadProcessId(hwnd, out _);
        return NativeMethods.GetKeyboardLayout(threadId);
    }

    public LanguageKind GetForegroundLanguage()
    {
        var hkl = GetForegroundLayout();
        var langId = unchecked((ushort)((long)hkl & 0xFFFF));

        return langId switch
        {
            0x0429 => LanguageKind.Persian,
            0x0409 => LanguageKind.English,
            _ => LanguageKind.Other
        };
    }

    public string Translate(IEnumerable<KeyStroke> keys, LanguageKind target)
    {
        var hkl = target switch
        {
            LanguageKind.Persian => PersianLayout,
            LanguageKind.English => EnglishLayout,
            _ => GetForegroundLayout()
        };

        var result = new StringBuilder();

        foreach (var key in keys)
        {
            var state = new byte[256];
            if (key.Shift)
                state[NativeMethods.VK_SHIFT] = 0x80;

            var buffer = new StringBuilder(8);
            var count = NativeMethods.ToUnicodeEx(
                key.VirtualKey,
                key.ScanCode,
                state,
                buffer,
                buffer.Capacity,
                0,
                hkl);

            if (count > 0)
                result.Append(buffer.ToString(0, Math.Min(count, buffer.Length)));
        }

        return result.ToString();
    }

    public void SwitchForeground(LanguageKind target)
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return;

        var hkl = target switch
        {
            LanguageKind.Persian => PersianLayout,
            LanguageKind.English => EnglishLayout,
            _ => IntPtr.Zero
        };

        if (hkl != IntPtr.Zero)
        {
            NativeMethods.PostMessage(
                hwnd,
                NativeMethods.WM_INPUTLANGCHANGEREQUEST,
                IntPtr.Zero,
                hkl);
        }
    }
}

internal enum LanguageKind
{
    Other = 0,
    Persian = 1,
    English = 2
}
