using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PowerLab.FishyTime.Contracts;
using PowerLab.FishyTime.Models;

namespace PowerLab.FishyTime.Utils.HookManager
{
    public class RectHookManager : IHookManager
    {
        private nint _hookHandle = IntPtr.Zero;
        private readonly WinEventDelegate _winEventDelegate;
        private DateTime _lastRectEventTime = DateTime.MinValue;
        private readonly TimeSpan _rectEventThrottleInterval = TimeSpan.FromMilliseconds(10);

        public Win32Window Win32Window { get; }

        public RectHookManager(Win32Window win32Window)
        {
            if (win32Window is null)
                throw new ArgumentNullException(nameof(win32Window), "win32Window cannot be null.");

            _winEventDelegate = WinEventProc;
            Win32Window = win32Window;
        }

        private event Action<Rect> _rectChanged;
        public event Action<Rect> RectChanged
        {
            add
            {
                bool wasEmpty = _rectChanged == null;
                _rectChanged += value;
                if (wasEmpty) StartHook();
            }
            remove
            {
                _rectChanged -= value;
                if (_rectChanged == null) StopHook();
            }
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero || hwnd != Win32Window.Handle) return;
            if (eventType != Win32Helper.EVENT_OBJECT_LOCATIONCHANGE) return;
            var now = DateTime.Now;
            if ((now - _lastRectEventTime) < _rectEventThrottleInterval)
                return;

            _lastRectEventTime = now;

            Task.Run(() =>
            {
                var windowRect = Win32Helper.GetWindowRect(Win32Window.Handle);
                if (windowRect == Rect.Empty) return;

                var newRect = new Rect(
                    new Point(windowRect.Left, windowRect.Top),
                    new Size(windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top));

                if (newRect == Win32Window.Bounds) return;

                OnRectChanged(newRect);
            });
        }

        public void OnRectChanged(Rect rect)
        {
            _rectChanged?.Invoke(rect);
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
            _rectChanged = null;
        }

        public void Dispose()
        {
            StopHook();
            ClearEventSubscribers();
        }
    }
}
