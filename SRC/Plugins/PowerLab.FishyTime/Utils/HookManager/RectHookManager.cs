using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using PowerLab.FishyTime.Contracts;
using PowerLab.FishyTime.Models;

namespace PowerLab.FishyTime.Utils.HookManager
{
    public sealed class RectHookManager : IHookManager, IDisposable
    {
        private WinEventSafeHandle _hookHandle;
        private readonly WinEventDelegate _winEventDelegate;
        private Channel<Guid> _rectChangedChannel;

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
            if (IsDisposed) return;

            if (hwnd == IntPtr.Zero || hwnd != Win32Window.Handle) return;
            if (eventType != Win32Helper.EVENT_OBJECT_LOCATIONCHANGE) return;

            _rectChangedChannel.Writer.TryWrite(Guid.NewGuid());
        }

        private async Task RectChangedHandler()
        {
            await foreach (var _ in _rectChangedChannel.Reader.ReadAllAsync())
            {
                var windowRect = Win32Helper.GetWindowRect(Win32Window.Handle);
                if (windowRect == Rect.Empty) continue;

                var newRect = new Rect(
                    new Point(windowRect.Left, windowRect.Top),
                    new Size(windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top));

                if (newRect == Win32Window.Bounds) continue;

                OnRectChanged(newRect);
            }
        }

        private void OnRectChanged(Rect rect) => _rectChanged?.Invoke(rect);

        public void StartHook()
        {
            if (_hookHandle is { IsInvalid: false }) return;

            _rectChangedChannel = Channel.CreateBounded<Guid>(new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });


            Task.Run(RectChangedHandler);

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
            _hookHandle?.Dispose();
            _hookHandle = null;

            _rectChangedChannel.Writer.TryComplete();
        }

        public void ClearEventSubscribers() => _rectChanged = null;

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
