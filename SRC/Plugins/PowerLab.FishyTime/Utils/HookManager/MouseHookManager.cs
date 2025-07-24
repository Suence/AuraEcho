using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PowerLab.FishyTime.Contracts;

namespace PowerLab.FishyTime.Utils.HookManager
{
    public class MouseHookManager : IHookManager
    {
        public static MouseHookManager Instance { get; } = new MouseHookManager();

        private nint _hookHandle = IntPtr.Zero;
        private readonly LowLevelMouseProc _mouseEventDelegate;

        private MouseHookManager()
        {
            _mouseEventDelegate = HookCallback;
        }

        private event Action<Point> _mouseMove;
        public event Action<Point> MouseMove
        {
            add
            {
                bool wasEmpty = _mouseMove == null;
                _mouseMove += value;
                if (wasEmpty) StartHook();
            }
            remove
            {
                _mouseMove -= value;
                if (_mouseMove == null) StopHook();
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || wParam != Win32Helper.WM_MOUSEMOVE)
                return Win32Helper.CallNextHookEx(_hookHandle, nCode, wParam, lParam);

            Win32Helper.GetCursorPos(out POINT pt);

            OnMouseMove(new Point(pt.X, pt.Y));

            return Win32Helper.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        public void OnMouseMove(Point point)
        {
            _mouseMove?.Invoke(point);
        }

        public void StartHook()
        {
            if (_hookHandle != IntPtr.Zero) return;

            _hookHandle = Win32Helper.SetWindowsHookEx(
                Win32Helper.WH_MOUSE_LL,
                _mouseEventDelegate,
                IntPtr.Zero,
                0);

            if (_hookHandle == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Hook 安装失败，错误码: {error}");
            }
        }
        public void StopHook()
        {
            if (_hookHandle == IntPtr.Zero) return;
            Win32Helper.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }

        public void ClearEventSubscribers()
        {
            _mouseMove = null;
        }

        public void Dispose()
        {
            StopHook();
            ClearEventSubscribers();
        }
    }
}
