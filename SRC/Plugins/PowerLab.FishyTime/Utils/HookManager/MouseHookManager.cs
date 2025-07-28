using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using PowerLab.FishyTime.Contracts;

namespace PowerLab.FishyTime.Utils.HookManager
{
    public class MouseHookManager : IHookManager, IDisposable
    {
        private Channel<Guid> _mousePointChannel;
        public static MouseHookManager Instance { get; } = new MouseHookManager();

        private MouseHookSafeHandle _hookHandle;
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
            if (nCode >= 0 && (int)wParam == Win32Helper.WM_MOUSEMOVE && !IsDisposed)
            {
                _mousePointChannel.Writer.TryWrite(Guid.NewGuid());
            }

            return Win32Helper.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public void OnMouseMove(Point point)
        {
            _mouseMove?.Invoke(point);
        }

        public void StartHook()
        {
            if (_hookHandle != null && !_hookHandle.IsClosedOrInvalid)
                return;

            _mousePointChannel = Channel.CreateBounded<Guid>(new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });
            Task.Run(MousePointChangedHandler);

            _hookHandle = Win32Helper.SetWindowsHookEx(
                Win32Helper.WH_MOUSE_LL,
                _mouseEventDelegate,
                IntPtr.Zero,
                0);

            if (_hookHandle == null || _hookHandle.IsInvalid)
            {
                int error = Marshal.GetLastWin32Error();
                _hookHandle?.Dispose();
                _hookHandle = null;
                throw new InvalidOperationException($"鼠标 Hook 安装失败，Win32Error={error}");
            }
        }

        private async Task MousePointChangedHandler()
        {
            await foreach (var _ in _mousePointChannel.Reader.ReadAllAsync())
            {
                if (Win32Helper.GetCursorPos(out POINT pt))
                {
                    OnMouseMove(new Point(pt.X, pt.Y));
                }
            }
        }

        public void StopHook()
        {
            _hookHandle?.Dispose();
            _hookHandle = null;

            _mousePointChannel.Writer.TryComplete();
        }

        public void ClearEventSubscribers()
        {
            _mouseMove = null;
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
