using PowerLab.FishyTime.Contracts;
using PowerLab.FishyTime.Utils;
using PowerLab.FishyTime.Utils.HookManager;
using PowerLab.FishyTime.Views;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;
namespace PowerLab.FishyTime.Models;

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
    private bool _isActivated;
    private bool _isClosed;
    private bool _isDisposed;
    private bool _isEnabledMask;
    private WindowMaskMode _maskMode;
    private List<Rect> _hotZoneRegions;
    private List<IHookManager> _hookManagers = [];
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
    public event Action<Win32Window> Closed
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

    public event Action<Point> MouseMove
    {
        add => MouseHookManager.Instance.MouseMove += value;
        remove => MouseHookManager.Instance.MouseMove -= value;
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

    public bool IsActivated
    {
        get => _isActivated;
        private set => SetProperty(ref _isActivated, value);
    }

    public bool IsClosed
    {
        get => _isClosed;
        private set => SetProperty(ref _isClosed, value);
    }

    public bool IsDisposed
    {
        get => _isDisposed;
        private set => SetProperty(ref _isDisposed, value);
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
        set => SetProperty(ref _isMouseOver, value);
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

    private Win32Window(nint handle) => Handle = handle;

    public static Task<Win32Window> AttachAsync(nint handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Handle cannot be zero.", nameof(handle));

        Win32Window win32Window = new(handle);
        return win32Window.LoadAsync().ContinueWith(_ => win32Window);
    }

    public void Deattach()
    {
        if (!IsClosed)
        {
            if (!Win32Helper.IsWindowVisible(Handle))
                Win32Helper.ShowWindowNoActivate(Handle);

            Win32Helper.TrySetWindowOpacity(Handle, 1);
        }
        _windowMask?.Close();
        Dispose();
    }

    private async Task LoadAsync()
    {
        Name = await Win32Helper.GetWindowTitleAsync(Handle);
        IsTopmost = await Win32Helper.IsWindowTopmostAsync(Handle);
        WindowState = await Win32Helper.GetWindowStateAsync(Handle);
        Icon = await Win32Helper.GetWindowIconAsync(Handle);
        Opacity = await Win32Helper.GetWindowOpacityAsync(Handle);
        CanSetOpacity = await Win32Helper.TrySetWindowOpacityAsync(Handle, Opacity);
        Bounds = await Win32Helper.GetWindowRectAsync(Handle);

        WinForms::Screen targetScreen = await Win32Helper.GetWindowScreenAsync(Handle);
        MaxWidth = targetScreen is null ? 1920 : targetScreen.Bounds.Width;
        MaxHeight = targetScreen is null ? 1080 : targetScreen.Bounds.Height;

        _hotZoneRegions = BuildHotZones(targetScreen);

        _hookManagers.AddRange(
        [
            _rectHookManager = new RectHookManager(this),
            _topmostHookManager = new TopmostHookManager(this),
            _activationHookManager = new ActivationHookManager(this),
            _closedHookManager = new ClosedHookManager(this),
            _windowStateHookManager = new WindowStateHookManager(this)
        ]);

        RectChanged += OnBoundsChanged;
        Activated += OnActivated;
        Deactivated += OnDeactivated;
        Closed += OnClosed;
        WindowStateChanged += OnWindowStateChanged;

        IsLoaded = true;
    }

    private void OnDeactivated()
        => _isActivated = false;

    private void OnActivated()
        => _isActivated = true;

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
        await Win32Helper.TrySetWindowOpacityAsync(Handle, Opacity);
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
        if (MaskMode != WindowMaskMode.HotZone) return;

        if (IsMasked) return;

        Application.Current.Dispatcher.BeginInvoke(ShowMaskWindow);
    }

    private void OnMouseLeave()
    {
        if (MaskMode != WindowMaskMode.MouseLeave) return;

        if (IsMasked || WindowState == ShowWindowCommands.Minimized) return;

        Application.Current.Dispatcher.BeginInvoke(ShowMaskWindow);
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

    private void OnClosed(Win32Window _)
    {
        IsClosed = true;
        Debug.WriteLine($"{Name} is closed.");
        _windowMask?.Close();
        Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        _hookManagers.ForEach(hook => hook?.Dispose());

        MouseMove -= OnMouseMove;
        MouseEnter = null;
        MouseLeave = null;
        Loaded = null;
        IsDisposed = true;

        Debug.WriteLine($"{Name} is disposed.");

        GC.SuppressFinalize(this);
    }
}
