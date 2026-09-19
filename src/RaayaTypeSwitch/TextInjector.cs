using System.Runtime.InteropServices;

namespace RaayaTypeSwitch;

internal sealed class TextInjector
{
    private const nuint InjectionMarker = 0xA170A170;

    public void ReplaceLastWord(int originalCharacterCount, string replacement, int delimiterVk)
    {
        var inputs = new List<NativeMethods.INPUT>();

        for (var i = 0; i < originalCharacterCount; i++)
        {
            inputs.Add(KeyDown((ushort)NativeMethods.VK_BACK));
            inputs.Add(KeyUp((ushort)NativeMethods.VK_BACK));
        }

        foreach (var ch in replacement)
        {
            inputs.Add(UnicodeDown(ch));
            inputs.Add(UnicodeUp(ch));
        }

        if (delimiterVk != 0)
        {
            inputs.Add(KeyDown((ushort)delimiterVk));
            inputs.Add(KeyUp((ushort)delimiterVk));
        }

        if (inputs.Count > 0)
        {
            NativeMethods.SendInput(
                (uint)inputs.Count,
                inputs.ToArray(),
                Marshal.SizeOf<NativeMethods.INPUT>());
        }
    }

    public void ReplaceLastWordWithoutDelimiter(int originalCharacterCount, string replacement) =>
        ReplaceLastWord(originalCharacterCount, replacement, 0);

    private static NativeMethods.INPUT KeyDown(ushort vk) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wVk = vk,
                dwExtraInfo = InjectionMarker
            }
        }
    };

    private static NativeMethods.INPUT KeyUp(ushort vk) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wVk = vk,
                dwFlags = NativeMethods.KEYEVENTF_KEYUP,
                dwExtraInfo = InjectionMarker
            }
        }
    };

    private static NativeMethods.INPUT UnicodeDown(char ch) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wScan = ch,
                dwFlags = NativeMethods.KEYEVENTF_UNICODE,
                dwExtraInfo = InjectionMarker
            }
        }
    };

    private static NativeMethods.INPUT UnicodeUp(char ch) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wScan = ch,
                dwFlags = NativeMethods.KEYEVENTF_UNICODE | NativeMethods.KEYEVENTF_KEYUP,
                dwExtraInfo = InjectionMarker
            }
        }
    };
}
