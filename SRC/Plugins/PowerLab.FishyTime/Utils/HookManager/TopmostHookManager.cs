using System;
using PowerLab.FishyTime.Contracts;
using PowerLab.FishyTime.Models;

namespace PowerLab.FishyTime.Utils.HookManager
{
    public class TopmostHookManager : IHookManager
    {
        private nint _hookHandle = IntPtr.Zero;
        private readonly WinEventDelegate _winEventDelegate;
        public Win32Window Win32Window { get; private set; }
        private event Action<bool> _topmostChanged;
        public event Action<bool> TopmostChanged
        {
            add
            {
                bool wasEmpty = _topmostChanged == null;
                _topmostChanged += value;
                if (wasEmpty) StartHook();
            }
            remove
            {
                _topmostChanged -= value;
                if (_topmostChanged == null) StopHook();
            }
        }
        public TopmostHookManager(Win32Window win32Window)
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
            if (eventType != Win32Helper.EVENT_OBJECT_LOCATIONCHANGE) return;

            bool isTopmost = Win32Helper.IsWindowTopmost(Win32Window.Handle);
            if (isTopmost != Win32Window.IsTopmost)
            {
                OnTopmostChanged(isTopmost);
            }
        }

        public void OnTopmostChanged(bool isTopmost)
        {
            Win32Window.IsTopmost = isTopmost;
            _topmostChanged?.Invoke(isTopmost);
        }
        public void StartHook()
        {
            if (_hookHandle != IntPtr.Zero) return;
            uint threadId = Win32Helper.GetWindowThreadProcessId(Win32Window.Handle, out uint processId);
            _hookHandle = Win32Helper.SetWinEventHook(
                Win32Helper.EVENT_OBJECT_LOCATIONCHANGE,
                Win32Helper.EVENT_OBJECT_LOCATIONCHANGE,
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
            _topmostChanged = null;
        }
        public void Dispose()
        {
            StopHook();
            ClearEventSubscribers();
        }
    }
}
