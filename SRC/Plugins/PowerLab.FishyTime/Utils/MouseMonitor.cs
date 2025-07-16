using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Prism.Events;

namespace PowerLab.FishyTime.Utils
{
    public class MouseMonitor : IDisposable
    {
        public nint WindowHandle { get; private set; }
        private IEnumerable<Rect> _regions;
        private readonly IEventAggregator _eventAggregator;
        private nint _mouseHook;
        private bool _lastMouseOverState = false;
        private readonly LowLevelMouseProc _mouseEventDelegate;
        public event Action MouseEnter;
        public event Action MouseLeave;
        public event Action<Point> MouseMove;
        public bool IsMonitoring { get; private set; }

        public MouseMonitor(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _mouseEventDelegate = HookCallback;
        }

        private void SetRegion(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                throw new ArgumentNullException(nameof(hwnd), "Window handle cannot be null.");

            WindowHandle = hwnd;

            _regions = null;
        }

        private void SetRegion(IEnumerable<Rect> regions)
        {
            _regions = regions ?? throw new ArgumentNullException(nameof(regions));
            WindowHandle = IntPtr.Zero;
        }

        public void Start(IntPtr hwnd)
        {
            SetRegion(hwnd);
            StartCore();
        }

        public void Start(IEnumerable<Rect> regions)
        {
            SetRegion(regions);
            StartCore();
        }

        public void Restart(IntPtr hwnd)
        {
            Stop();
            Start(hwnd);
        }

        public void Restart(IEnumerable<Rect> regions)
        {
            Stop();
            Start(regions);
        }

        private void StartCore()
        {
            if (IsMonitoring) return;

            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;

            //Win32Helper.GetModuleHandle(curModule.ModuleName!),
            _mouseHook = Win32Helper.SetWindowsHookEx(
                Win32Helper.WH_MOUSE_LL,
                _mouseEventDelegate,
                IntPtr.Zero,
                0);

            if (_mouseHook == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Hook 安装失败，错误码: {error}");
            }

            IsMonitoring = true;
        }

        public void Stop()
        {
            if (!IsMonitoring) return;

            Win32Helper.UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;

            IsMonitoring = false;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || wParam != Win32Helper.WM_MOUSEMOVE)
                return Win32Helper.CallNextHookEx(_mouseHook, nCode, wParam, lParam);

            Win32Helper.GetCursorPos(out POINT pt);
            //POINT pt = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam).pt;

            MouseMove?.Invoke(new Point(pt.X, pt.Y));
            _eventAggregator.GetEvent<Events.MouseMoveEvent>().Publish(new Point(pt.X, pt.Y));
            if (CheckBounds(new Point(pt.X, pt.Y)))
            {
                if (_lastMouseOverState)
                    return Win32Helper.CallNextHookEx(_mouseHook, nCode, wParam, lParam);

                Debug.WriteLine("MouseEnter");
                MouseEnter?.Invoke();
                _lastMouseOverState = true;
                return Win32Helper.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
            }

            if (!_lastMouseOverState)
                return Win32Helper.CallNextHookEx(_mouseHook, nCode, wParam, lParam);

            Debug.WriteLine("MouseLeave");
            MouseLeave?.Invoke();
            _lastMouseOverState = false;

            return Win32Helper.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }

        private bool CheckBounds(Point point)
        {
            if (WindowHandle == IntPtr.Zero)
                return _regions.Any(region => region.Contains(point));

            return Win32Helper.GetTopLevelWindowUnderMouse() == WindowHandle;
        }

        public void Dispose()
        {
            if (_mouseHook == IntPtr.Zero) return;

            Win32Helper.UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;

            MouseEnter = null;
            MouseLeave = null;
        }
    }
}
