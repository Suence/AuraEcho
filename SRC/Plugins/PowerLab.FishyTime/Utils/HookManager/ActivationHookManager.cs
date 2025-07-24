using System;
using PowerLab.FishyTime.Contracts;
using PowerLab.FishyTime.Models;

namespace PowerLab.FishyTime.Utils.HookManager
{
    public class ActivationHookManager : IHookManager
    {
        private nint _hookHandle = IntPtr.Zero;
        private readonly WinEventDelegate _winEventDelegate;
        public Win32Window Win32Window { get; private set; }
        private nint _lastForegroundHwnd = IntPtr.Zero;
        private event Action _activated;
        public event Action Activated
        {
            add
            {
                bool wasEmpty = _activated is null && _deactivated is null;
                _activated += value;
                if (wasEmpty) StartHook();
            }
            remove
            {
                _activated -= value;
                if (_activated is null && _deactivated is null) StopHook();
            }
        }
        private event Action _deactivated;
        public event Action Deactivated
        {
            add
            {
                bool wasEmpty = _activated is null && _deactivated is null;
                _activated += value;
                if (wasEmpty) StartHook();
            }
            remove
            {
                _activated -= value;
                if (_activated is null && _deactivated is null) StopHook();
            }
        }
        public ActivationHookManager(Win32Window win32Window)
        {
            if (win32Window is null)
                throw new ArgumentNullException(nameof(win32Window), "win32Window cannot be null.");

            _winEventDelegate = WinEventProc;
            Win32Window = win32Window;
        }
        private void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == Win32Window.Handle)
            {
                OnActivated();
            }
            else if (_lastForegroundHwnd == Win32Window.Handle)
            {
                OnDeactivated();
            }

            _lastForegroundHwnd = hwnd;
        }

        public void OnActivated()
        {
            _activated?.Invoke();
        }

        public void OnDeactivated()
        {
            _deactivated?.Invoke();
        }

        public void StartHook()
        {
            if (_hookHandle != IntPtr.Zero) return;
            uint threadId = Win32Helper.GetWindowThreadProcessId(Win32Window.Handle, out uint processId);
            _hookHandle = Win32Helper.SetWinEventHook(
                Win32Helper.EVENT_SYSTEM_FOREGROUND,
                Win32Helper.EVENT_SYSTEM_FOREGROUND,
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
            _activated = null;
            _deactivated = null;
        }
        public void Dispose()
        {
            StopHook();
            ClearEventSubscribers();
        }
    }
}
