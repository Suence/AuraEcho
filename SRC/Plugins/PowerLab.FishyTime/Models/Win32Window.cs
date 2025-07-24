using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using PowerLab.Core.Extensions;
using PowerLab.FishyTime.Contracts;
using PowerLab.FishyTime.Utils;
using PowerLab.FishyTime.Utils.HookManager;
using PowerLab.FishyTime.Views;
using Prism.Ioc;
using Prism.Mvvm;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;
namespace PowerLab.FishyTime.Models
{
    public class Win32Window : BindableBase, IDisposable
    {
        #region private members
        private string _name;
        private double _width;
        private double _height;
        private double _maxWidth;
        private double _maxHeight;
        private Point _position;
        private nint _handle;
        private double _opacity;
        private bool _isTopmost;
        private ShowWindowCommands _windowState;
        private bool _isMouseOver;
        private bool _isVisible = true;
        private Drawing::Icon _icon;
        private bool _isMasked;
        private bool _isLoaded;

        private bool _isEnabledMask;
        private WindowMaskMode _maskMode;
        private List<Rect> _hotZoneRegions;

        private IWindowMask _windowMask;
        #endregion

        #region events
        private RectHookManager _rectHookManager;
        public event Action<Rect> RectChanged
        {
            add => _rectHookManager.RectChanged += value;
            remove => _rectHookManager.RectChanged -= value;
        }

        private TopmostHookManager _topmostHookManager;
        public event Action<bool> TopmostChanged
        {
            add => _topmostHookManager.TopmostChanged += value;
            remove => _topmostHookManager.TopmostChanged -= value;
        }

        private WindowStateHookManager _windowStateHookManager;
        public event Action<ShowWindowCommands> WindowStateChanged
        {
            add => _windowStateHookManager.WindowStateChanged += value;
            remove => _windowStateHookManager.WindowStateChanged -= value;
        }

        public ClosedHookManager _closedHookManager;
        public event Action Closed
        {
            add => _closedHookManager.Closed += value;
            remove => _closedHookManager.Closed -= value;
        }

        public ActivationHookManager _activationHookManager;
        public event Action Activated
        {
            add => _activationHookManager.Activated += value;
            remove => _activationHookManager.Activated -= value;
        }
        public event Action Deactivated
        {
            add => _activationHookManager.Deactivated += value;
            remove => _activationHookManager.Deactivated -= value;
        }

        public event Action Loaded;
        public event Action MouseEnter;
        public event Action MouseLeave;

        private MouseHookManager _mouseHookManager;
        public event Action<Point> MouseMove
        {
            add => _mouseHookManager.MouseMove += value;
            remove => _mouseHookManager.MouseMove -= value;
        }

        #endregion

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Drawing::Icon Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public double Width
        {
            get => _width;
            set
            {
                if (!SetProperty(ref _width, value)) return;

                ApplyRectToSourceWindow();
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                if (!SetProperty(ref _height, value)) return;

                ApplyRectToSourceWindow();
            }
        }

        public double MaxWidth
        {
            get => _maxWidth;
            set => SetProperty(ref _maxWidth, value);
        }

        public double MaxHeight
        {
            get => _maxHeight;
            set => SetProperty(ref _maxHeight, value);
        }

        public Point Position
        {
            get => _position;
            set
            {
                if (!SetProperty(ref _position, value)) return;

                ApplyRectToSourceWindow();
            }
        }

        public bool IsEnabledMask
        {
            get => _isEnabledMask;
            set
            {
                if (!SetProperty(ref _isEnabledMask, value)) return;

                IsEnabledMaskChanged(value);
            }
        }

        public WindowMaskMode MaskMode
        {
            get => _maskMode;
            set
            {
                if (!SetProperty(ref _maskMode, value)) return;

                MaskModeChanged(value);
            }
        }

        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                if (!SetProperty(ref _isLoaded, value)) return;

                if (value) Loaded?.Invoke();
            }
        }

        public bool IsMasked
        {
            get => _isMasked;
            internal set => SetProperty(ref _isMasked, value);
        }

        public nint Handle
        {
            get => _handle;
            private set => SetProperty(ref _handle, value);
        }

        public nint MaskHandle => _windowMask?.Handle ?? IntPtr.Zero;

        public double Opacity
        {
            get => _opacity;
            set
            {
                if (!SetProperty(ref _opacity, value)) return;

                ApplyOpacityToSourceWindow();
            }
        }

        public bool IsTopmost
        {
            get => _isTopmost;
            set
            {
                if (!SetProperty(ref _isTopmost, value)) return;

                ApplyTopmostToSourceWindow();
            }
        }

        public bool IsMouseOver
        {
            get => _isMouseOver;
            set
            {
                SetProperty(ref _isMouseOver, value);
                Debug.WriteLine(value);
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public ShowWindowCommands WindowState
        {
            get => _windowState;
            set => SetProperty(ref _windowState, value);
        }

        public Rect Bounds
        {
            get => new(Position, new Size(Width, Height));
            set
            {
                if (value == Rect.Empty) return;

                SetProperty(ref _position, value.Location, nameof(Position));
                SetProperty(ref _width, value.Width, nameof(Width));
                SetProperty(ref _height, value.Height, nameof(Height));

                RaisePropertyChanged(nameof(Bounds));
            }
        }

        public bool CanSetOpacity { get; set; }

        public Win32Window(nint handle) => Handle = handle;

        public async Task LoadAsync()
        {
            Name = await Win32Helper.GetWindowTitleAsync(Handle);
            IsTopmost = await Win32Helper.IsWindowTopmostAsync(Handle);
            WindowState = await Win32Helper.GetWindowStateAsync(Handle);
            Icon = await Win32Helper.GetWindowIconAsync(Handle);
            Opacity = await Win32Helper.GetWindowOpacityAsync(Handle);
            CanSetOpacity = await Win32Helper.TrySetWindowOpacityAsync(Handle, (byte)(Opacity * 255));
            Bounds = await Win32Helper.GetWindowRectAsync(Handle);

            WinForms::Screen targetScreen = await Win32Helper.GetWindowScreenAsync(Handle);
            MaxWidth = targetScreen is null ? 1920 : targetScreen.Bounds.Width;
            MaxHeight = targetScreen is null ? 1080 : targetScreen.Bounds.Height;

            _hotZoneRegions = BuildHotZones(targetScreen);

            _rectHookManager = new RectHookManager(this);
            RectChanged += OnBoundsChanged;

            _topmostHookManager = new TopmostHookManager(this);
            _activationHookManager = new ActivationHookManager(this);

            _closedHookManager = new ClosedHookManager(this);
            Closed += Dispose;

            _windowStateHookManager = new WindowStateHookManager(this);
            WindowStateChanged += OnWindowStateChanged;

            _mouseHookManager = MouseHookManager.Instance;

            IsLoaded = true;
        }

        private static List<Rect> BuildHotZones(WinForms::Screen screen)
        {
            List<Rect> hotZones =
            [
              new Rect(new Point(screen.Bounds.X, screen.Bounds.Y), new Size(200, 200)),
              new Rect(new Point(screen.Bounds.X + screen.Bounds.Width - 200, screen.Bounds.Y), new Size(200, 200)),
              new Rect(new Point(screen.Bounds.X + screen.Bounds.Width - 200, screen.Bounds.Y + screen.Bounds.Height - 200), new Size(200, 200)),
              new Rect(new Point(screen.Bounds.X, screen.Bounds.Y + screen.Bounds.Height - 200), new Size(200, 200))
            ];
            return hotZones;
        }

        private void IsEnabledMaskChanged(bool isEnabledMask)
        {
            if (isEnabledMask)
            {
                EnableMask();
                return;
            }
            DisableMask();
        }

        private void EnableMask()
        {
            MouseMove += OnMouseMove;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;

            if (MaskMode == WindowMaskMode.Spotlight)
            {
                ShowSpotlightWindow();
                return;
            }
        }

        private void DisableMask()
        {
            MouseMove -= OnMouseMove;
            MouseEnter = null;
            MouseLeave = null;

            _windowMask?.Close();
        }

        private void MaskModeChanged(WindowMaskMode mode)
        {
            _windowMask?.Close();

            if (mode == WindowMaskMode.Spotlight)
            {
                ShowSpotlightWindow();
                return;
            }
        }

        private async void ApplyRectToSourceWindow()
        {
            await Win32Helper.SetWindowPosAsync(
                Handle,
                IntPtr.Zero,
                (int)Position.X,
                (int)Position.Y,
                (int)Width,
                (int)Height,
                Win32Helper.SWP_NOZORDER | Win32Helper.SWP_NOACTIVATE);
        }

        private async void ApplyTopmostToSourceWindow()
        {
            if (IsMasked && IsTopmost)
            {
                await Win32Helper.SetWindowTopmoastWithoutShowAsync(Handle, IsTopmost);
                return;
            }
            await Win32Helper.SetWindowTopmostAsync(Handle, IsTopmost);
        }

        private async void ApplyOpacityToSourceWindow()
        {
            await Win32Helper.SetLayeredWindowAttributesAsync(Handle, 0, (byte)(Opacity * 255), Win32Helper.LWA_ALPHA);
        }

        private void OnMouseMove(Point point)
        {
            if (CheckBounds(point))
            {
                if (IsMouseOver) return;

                IsMouseOver = true;
                MouseEnter?.Invoke();
                return;
            }

            if (!IsMouseOver) return;

            IsMouseOver = false;
            MouseLeave?.Invoke();
        }

        private void OnMouseEnter()
        {
            Debug.WriteLine("MouseEnter");

            if (MaskMode != WindowMaskMode.HotZone) return;

            if (IsMasked) return;

            ShowMaskWindow();
        }

        private void OnMouseLeave()
        {
            Debug.WriteLine("MouseLeave");

            if (MaskMode != WindowMaskMode.MouseLeave) return;

            if (IsMasked || WindowState == ShowWindowCommands.Minimized) return;

            ShowMaskWindow();
        }

        private bool CheckBounds(Point point)
        {
            if (MaskMode == WindowMaskMode.HotZone)
                return _hotZoneRegions.Exists(region => region.Contains(point));

            return Win32Helper.GetTopLevelWindowUnderMouse() == Handle;
        }

        private void OnBoundsChanged(Rect rect)
        {
            if (rect == Rect.Empty) return;

            Bounds = rect;
        }

        private void OnWindowStateChanged(ShowWindowCommands state)
        {
            WindowState = state;

            Debug.WriteLine("WindowStateChanged");
        }

        #region MaskWindow
        public void ShowMaskWindow()
        {
            _windowMask = new MaskWindow(this);
            _windowMask.MaskClosed += OnMaskWindowClosed;

            _windowMask.Show();
            IsMasked = true;
        }

        private void OnMaskWindowClosed()
        {
            IsMasked = false;
        }

        public void ShowSpotlightWindow()
        {
            _windowMask = new SpotlightWindow(this);
            _windowMask.MaskClosed += OnMaskWindowClosed;
            _windowMask.Show();
            IsMasked = true;
        }
        #endregion
        public void Show()
        {
            Win32Helper.ShowWindow(Handle);
        }

        public void Hide()
        {
            Win32Helper.HideWindow(Handle);
        }

        public void Dispose()
        {
            new IHookManager[]
            {
                _rectHookManager,
                _topmostHookManager,
                _activationHookManager,
                _closedHookManager,
                _windowStateHookManager
            }.ForEach(hook => hook?.Dispose());

            MouseMove -= OnMouseMove;
            MouseEnter = null;
            MouseLeave = null;
            Loaded = null;
        }
    }
}
