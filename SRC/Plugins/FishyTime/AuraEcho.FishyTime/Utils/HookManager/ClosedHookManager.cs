using System;
using AuraEcho.FishyTime.Contracts;
using AuraEcho.FishyTime.Models;

namespace AuraEcho.FishyTime.Utils.HookManager
{
    public class ClosedHookManager : IHookManager, IDisposable
    {
        private WinEventSafeHandle _hookHandle;
        private readonly WinEventDelegate _winEventDelegate;
        public Win32Window Win32Window { get; private set; }
        private event Action<Win32Window> _closed;
        public event Action<Win32Window> Closed
        {
            add
            {
                bool wasEmpty = _closed == null;
                _closed += value;
                if (wasEmpty) StartHook();
            }
            remove
            {
                _closed -= value;
                if (_closed == null) StopHook();
            }
        }
        public ClosedHookManager(Win32Window win32Window)
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
            if (eventType != Win32Helper.EVENT_OBJECT_DESTROY) return;
            if (idObject != 0 || idChild != 0) return;
            OnClosed();
        }
        public void OnClosed()
        {
            _closed?.Invoke(Win32Window);
        }
        public void StartHook()
        {
            if (_hookHandle is { IsInvalid: false }) return;
            uint threadId = Win32Helper.GetWindowThreadProcessId(Win32Window.Handle, out uint processId);
            _hookHandle = Win32Helper.SetWinEventHook(
                Win32Helper.EVENT_OBJECT_DESTROY,
                Win32Helper.EVENT_OBJECT_DESTROY,
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
            _closed = null;
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
