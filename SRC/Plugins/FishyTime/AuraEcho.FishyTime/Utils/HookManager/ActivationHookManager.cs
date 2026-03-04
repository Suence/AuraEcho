using System;
using AuraEcho.FishyTime.Contracts;
using AuraEcho.FishyTime.Models;

namespace AuraEcho.FishyTime.Utils.HookManager
{
    public class ActivationHookManager : IHookManager, IDisposable
    {
        private WinEventSafeHandle _hookHandle;
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
            if (_hookHandle is { IsInvalid: false }) return;

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
            _hookHandle?.Dispose();
            _hookHandle = null;
        }
        public void ClearEventSubscribers()
        {
            _activated = null;
            _deactivated = null;
        }
        #region IDisposable Implementation
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            StopHook();
            ClearEventSubscribers();

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
