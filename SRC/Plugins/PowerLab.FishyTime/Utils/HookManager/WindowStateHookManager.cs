using System;
using PowerLab.FishyTime.Contracts;
using PowerLab.FishyTime.Models;

namespace PowerLab.FishyTime.Utils.HookManager
{
    public class WindowStateHookManager : IHookManager
    {
        private nint _hookHandle = IntPtr.Zero;
        private readonly WinEventDelegate _winEventDelegate;
        public Win32Window Win32Window { get; private set; }
        private event Action<ShowWindowCommands> _windowStateChanged;
        public event Action<ShowWindowCommands> WindowStateChanged
        {
            add
            {
                bool wasEmpty = _windowStateChanged == null;
                _windowStateChanged += value;
                if (wasEmpty) StartHook();
            }
            remove
            {
                _windowStateChanged -= value;
                if (_windowStateChanged == null) StopHook();
            }
        }
        public WindowStateHookManager(Win32Window win32Window)
        {
            if (win32Window is null)
                throw new ArgumentNullException(nameof(win32Window), "win32Window cannot be null.");

            _winEventDelegate = WinEventProc;
            Win32Window = win32Window;
        }
        private void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero || hwnd != Win32Window.Handle) return;
            if (eventType != Win32Helper.EVENT_SYSTEM_MINIMIZESTART &&
                eventType != Win32Helper.EVENT_SYSTEM_MINIMIZEEND) return;

            OnWindowStateChanged(Win32Helper.GetWindowState(Win32Window.Handle));
        }

        public void OnWindowStateChanged(ShowWindowCommands state)
        {
            Win32Window.WindowState = state;
            _windowStateChanged?.Invoke(state);
        }

        public void StartHook()
        {
            if (_hookHandle != IntPtr.Zero) return;
            uint threadId = Win32Helper.GetWindowThreadProcessId(Win32Window.Handle, out uint processId);
            _hookHandle = Win32Helper.SetWinEventHook(
                Win32Helper.EVENT_SYSTEM_MINIMIZESTART,
                Win32Helper.EVENT_SYSTEM_MINIMIZEEND,
                IntPtr.Zero,
                _winEventDelegate,
                processId,
                threadId,
                Win32Helper.WINEVENT_OUTOFCONTEXT);
        }
        public void StopHook()
        {
            if (_hookHandle == IntPtr.Zero) return;
            Win32Helper.UnhookWinEvent(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
        public void ClearEventSubscribers()
        {
            _windowStateChanged = null;
        }
        public void Dispose()
        {
            StopHook();
            ClearEventSubscribers();
        }
    }
}
