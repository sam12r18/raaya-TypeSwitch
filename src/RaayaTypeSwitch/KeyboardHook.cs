using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaayaTypeSwitch;

internal sealed class KeyboardHook : IDisposable
{
    private readonly TypingEngine _engine;
    private readonly NativeMethods.LowLevelKeyboardProc _proc;
    private IntPtr _hookId;

    public KeyboardHook(TypingEngine engine)
    {
        _engine = engine;
        _proc = HookCallback;
    }

    public void Start()
    {
        if (_hookId != IntPtr.Zero)
            return;

        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;

        var moduleHandle = NativeMethods.GetModuleHandle(module?.ModuleName);
        _hookId = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_KEYBOARD_LL,
            _proc,
            moduleHandle,
            0);

        if (_hookId == IntPtr.Zero)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 &&
            (wParam == (IntPtr)NativeMethods.WM_KEYDOWN ||
             wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN))
        {
            var info = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);

            if ((info.flags & NativeMethods.LLKHF_INJECTED) == 0)
            {
                var suppress = _engine.OnKeyDown(info.vkCode, info.scanCode);
                if (suppress)
                    return (IntPtr)1;
            }
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hookId != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        GC.SuppressFinalize(this);
    }
}
