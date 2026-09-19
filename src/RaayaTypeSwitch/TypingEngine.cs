namespace RaayaTypeSwitch;

internal sealed class TypingEngine
{
    private readonly KeyboardLayoutService _layouts = new();
    private readonly LanguageDetector _detector = new();
    private readonly TextInjector _injector = new();
    private readonly List<KeyStroke> _buffer = new();

    public bool Enabled { get; set; } = true;

    public event Action<string>? StatusChanged;

    public bool OnKeyDown(uint vkCode, uint scanCode)
    {
        if (!Enabled)
            return false;

        if (IsModifierDown())
        {
            Clear();
            return false;
        }

        if (vkCode == NativeMethods.VK_BACK)
        {
            if (_buffer.Count > 0)
                _buffer.RemoveAt(_buffer.Count - 1);

            return false;
        }

        if (IsNavigationOrResetKey(vkCode))
        {
            Clear();
            return false;
        }

        if (IsDelimiter(vkCode))
        {
            if (_buffer.Count == 0)
                return false;

            var corrected = TryCorrect((int)vkCode);
            Clear();
            return corrected;
        }

        if (IsPotentialPrintable(vkCode))
        {
            var shift = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0;
            _buffer.Add(new KeyStroke(vkCode, scanCode, shift));

            if (_buffer.Count > 40)
                Clear();
        }

        return false;
    }

    private bool TryCorrect(int delimiterVk)
    {
        var currentLanguage = _layouts.GetForegroundLanguage();
        if (currentLanguage is not (LanguageKind.Persian or LanguageKind.English))
            return false;

        var alternateLanguage = currentLanguage == LanguageKind.Persian
            ? LanguageKind.English
            : LanguageKind.Persian;

        var current = _layouts.Translate(_buffer, currentLanguage);
        var alternate = _layouts.Translate(_buffer, alternateLanguage);

        var result = _detector.Detect(current, alternate, currentLanguage);
        if (!result.ShouldCorrect)
            return false;

        _injector.ReplaceLastWord(current.Length, alternate, delimiterVk);
        _layouts.SwitchForeground(result.TargetLanguage);

        StatusChanged?.Invoke(
            $"{current} → {alternate} ({result.CurrentScore:0.0}/{result.AlternateScore:0.0})");

        return true;
    }

    private void Clear() => _buffer.Clear();

    private static bool IsDelimiter(uint vk) =>
        vk is NativeMethods.VK_SPACE or NativeMethods.VK_RETURN or NativeMethods.VK_TAB;

    private static bool IsNavigationOrResetKey(uint vk) =>
        vk is NativeMethods.VK_ESCAPE
            or NativeMethods.VK_LEFT
            or NativeMethods.VK_RIGHT
            or NativeMethods.VK_UP
            or NativeMethods.VK_DOWN
            or NativeMethods.VK_DELETE;

    private static bool IsPotentialPrintable(uint vk) =>
        (vk >= 0x30 && vk <= 0x5A) ||
        (vk >= 0xBA && vk <= 0xE2);

    private static bool IsModifierDown()
    {
        static bool Down(int vk) => (NativeMethods.GetAsyncKeyState(vk) & 0x8000) != 0;

        return Down(NativeMethods.VK_CONTROL)
            || Down(NativeMethods.VK_MENU)
            || Down(NativeMethods.VK_LWIN)
            || Down(NativeMethods.VK_RWIN);
    }
}
