using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Documents;
using PowerLab.Core.Extensions;
using PowerLab.FishyTime.Models;

namespace PowerLab.FishyTime.Utils
{
    public class WindowStatusWatcher : IDisposable
    {
        public nint WindowHandle { get; private set; }

        public bool IsWatching { get; private set; }

        private nint _locationHook;
        private nint _hideHook;
        private nint _showHook;
        private nint _minWindowHook;
        private nint _minimizeendHook;
        private nint _destroyHook;
        private nint _mouseLeaveHook;
        private WinEventDelegate _winEventDelegate;

        private bool _lastTopmostState;

        private DateTime _lastRectEventTime = DateTime.MinValue;
        private readonly TimeSpan _rectEventThrottleInterval = TimeSpan.FromMilliseconds(100);
        private Rect _lastRect = Rect.Empty;

        public event Action<Rect> RectChanged;
        public event Action<bool> TopmostChanged;
        public event Action<ShowWindowCommands> WindowStateChanged;
        public event Action MouseEnter;
        public event Action MouseLeave;
        public event Action<bool> VisibilityChanged;
        public event Action Closed;

        public WindowStatusWatcher()
        {
            _winEventDelegate = WinEventProc;
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd != WindowHandle) return;

            if (eventType == Win32Helper.EVENT_OBJECT_LOCATIONCHANGE)
            {
                var now = DateTime.Now;
                if ((now - _lastRectEventTime) < _rectEventThrottleInterval)
                    return;

                _lastRectEventTime = now;
            }

            switch (eventType)
            {
                case Win32Helper.EVENT_OBJECT_DESTROY:
                    if (idObject == 0 && idChild == 0)
                    {
                        Closed?.Invoke();
                    }
                    break;
                case Win32Helper.WM_MOUSELEAVE:
                    Win32Helper.TrackMouseLeave(hwnd);
                    break;
                case Win32Helper.SW_SHOW:
                    break;
                case Win32Helper.SW_HIDE:
                    break;
                case Win32Helper.EVENT_SYSTEM_MINIMIZESTART:
                case Win32Helper.EVENT_SYSTEM_MINIMIZEEND:
                case Win32Helper.EVENT_SYSTEM_FOREGROUND:
                    WindowStateChanged?.Invoke(Win32Helper.GetWindowState(WindowHandle));
                    break;
                case Win32Helper.EVENT_OBJECT_LOCATIONCHANGE:

                    var windowRect = Win32Helper.GetWindowRect(WindowHandle);
                    if (windowRect == Rect.Empty) return;

                    var newRect = new Rect(
                        new Point(windowRect.Left, windowRect.Top),
                        new Size(windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top));

                    if (newRect == _lastRect) break;
                    _lastRect = newRect;

                    RectChanged?.Invoke(newRect);
                    break;
            }

            bool isTopmost = Win32Helper.IsWindowTopmost(WindowHandle);
            if (isTopmost != _lastTopmostState)
            {
                _lastTopmostState = isTopmost;
                TopmostChanged?.Invoke(isTopmost);
            }
        }

        public void StartWatch(IntPtr hwnd)
        {
            if (IsWatching) return;

            if (hwnd == IntPtr.Zero)
                throw new ArgumentNullException(nameof(hwnd), "Window handle cannot be null.");

            WindowHandle = hwnd;

            uint threadId = Win32Helper.GetWindowThreadProcessId(WindowHandle, out uint processId);

            _locationHook = Win32Helper.SetWinEventHook(
                Win32Helper.EVENT_OBJECT_LOCATIONCHANGE,
                Win32Helper.EVENT_OBJECT_LOCATIONCHANGE,
                IntPtr.Zero,
                _winEventDelegate,
                processId,
                threadId,
                Win32Helper.WINEVENT_OUTOFCONTEXT);

            _hideHook = Win32Helper.SetWinEventHook(
                Win32Helper.SW_HIDE,
                Win32Helper.SW_HIDE,
                IntPtr.Zero,
                _winEventDelegate,
                processId,
                threadId,
                Win32Helper.WINEVENT_OUTOFCONTEXT);

            _showHook = Win32Helper.SetWinEventHook(
                Win32Helper.SW_SHOW,
                Win32Helper.SW_SHOW,
                IntPtr.Zero,
                _winEventDelegate,
                processId,
                threadId,
                Win32Helper.WINEVENT_OUTOFCONTEXT);

            _minWindowHook = Win32Helper.SetWinEventHook(
                Win32Helper.EVENT_OBJECT_LOCATIONCHANGE,
                Win32Helper.EVENT_OBJECT_LOCATIONCHANGE,
                IntPtr.Zero,
                _winEventDelegate,
                processId,
                threadId,
                Win32Helper.WINEVENT_OUTOFCONTEXT);

            _minimizeendHook = Win32Helper.SetWinEventHook(
                Win32Helper.EVENT_SYSTEM_MINIMIZESTART,
                Win32Helper.EVENT_SYSTEM_MINIMIZESTART,
                IntPtr.Zero,
                _winEventDelegate,
                processId,
                threadId,
                Win32Helper.WINEVENT_OUTOFCONTEXT);

            _destroyHook = Win32Helper.SetWinEventHook(
                Win32Helper.EVENT_OBJECT_DESTROY,
                Win32Helper.EVENT_OBJECT_DESTROY,
                IntPtr.Zero,
                _winEventDelegate,
                processId,
                threadId,
                Win32Helper.WINEVENT_OUTOFCONTEXT);

            _mouseLeaveHook = Win32Helper.SetWinEventHook(
                Win32Helper.WM_MOUSELEAVE,
                Win32Helper.WM_MOUSELEAVE,
                IntPtr.Zero,
                _winEventDelegate,
                processId,
                threadId,
                Win32Helper.WINEVENT_OUTOFCONTEXT);

            IsWatching = true;
        }

        public void StopWatch()
        {
            if (!IsWatching) return;

            var hooks = new[] {
                _locationHook,
                _hideHook,
                _showHook,
                _minWindowHook,
                _minimizeendHook,
                _destroyHook,
                _mouseLeaveHook
            };
            hooks.ForEach(item =>
            {
                if (item == IntPtr.Zero) return;
                Win32Helper.UnhookWinEvent(item);
                item = IntPtr.Zero;
            });

            IsWatching = false;
        }

        public void RestartWatch(IntPtr hwnd)
        {
            StopWatch();
            StartWatch(hwnd);
        }

        public void Dispose()
        {
            StopWatch();

            RectChanged = null;
            TopmostChanged = null;
            WindowStateChanged = null;
            MouseEnter = null;
            MouseLeave = null;
            VisibilityChanged = null;
            Closed = null;
        }
    }
}
