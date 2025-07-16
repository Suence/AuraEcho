using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using PowerLab.Core.Attributes;
using PowerLab.Core.Constants;
using PowerLab.FishyTime.Events;
using PowerLab.FishyTime.Models;
using PowerLab.FishyTime.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using WinForms = System.Windows.Forms;

namespace PowerLab.FishyTime.ViewModels
{
    public class FishyTimeHomeViewModel : BindableBase
    {
        #region private members
        private readonly IContainerProvider _containerProvider;
        private readonly IEventAggregator _eventAggregator;
        private ManagedWindowInfo _managedWindowInfo;
        private WindowStatusWatcher _windowRectWatcher;
        private MouseMonitor _mouseMonitor;
        private double _targetScreenWidth;
        private double _targetScreenHeight;
        private bool _userUpdating = false;
        private int _screenCount = 1;
        private FishyTimeConfig _fishyTimeConfig;
        private const string FishyTimeConfigFileName = "FishyTimeConfig.json";
        private ObservableCollection<WindowMaskMode> _windowMaskModes =
        [
            WindowMaskMode.MouseLeave,
            WindowMaskMode.HotZone,
            WindowMaskMode.Spotlight
        ];
        #endregion

        public ObservableCollection<WindowMaskMode> WindowMaskModes
        {
            get => _windowMaskModes;
            set => SetProperty(ref _windowMaskModes, value);
        }
        public ManagedWindowInfo ManagedWindowInfo
        {
            get => _managedWindowInfo;
            set => SetProperty(ref _managedWindowInfo, value);
        }

        public FishyTimeConfig FishyTimeConfig
        {
            get => _fishyTimeConfig;
            set => SetProperty(ref _fishyTimeConfig, value);
        }

        public double TargetScreenWidth
        {
            get => _targetScreenWidth;
            set => SetProperty(ref _targetScreenWidth, value);
        }

        public double TargetScreenHeight
        {
            get => _targetScreenHeight;
            set => SetProperty(ref _targetScreenHeight, value);
        }

        public DelegateCommand<IntPtr?> SetManagedWindowInfoCommand { get; }
        private void SetManagedWindowInfo(IntPtr? handle)
        {
            if (handle is null or 0) return;
            if (handle == ManagedWindowInfo?.Handle) return;

            _eventAggregator.GetEvent<MaskWindowCloseRequestedEvent>().Publish();
            _eventAggregator.GetEvent<SpotlightWindowCloseRequestedEvent>().Publish();

            UpdateScreenSize(handle.Value);

            ManagedWindowInfo = GetManagedWindowInfo(handle.Value);
            if (FishyTimeConfig.WindowMaskMode == WindowMaskMode.Spotlight && FishyTimeConfig.IsWindowMaskEnabled)
            {
                _eventAggregator
                .GetEvent<MaskWindowRequestedEvent>()
                .Publish(new MaskWindowEventArgs(ManagedWindowInfo.Bounds, ManagedWindowInfo, FishyTimeConfig.WindowMaskMode));
            }

            _windowRectWatcher.RestartWatch(ManagedWindowInfo.Handle);
            RestartMouseMonitor();
        }

        private void RestartMouseMonitor()
        {
            switch (FishyTimeConfig.WindowMaskMode)
            {
                case WindowMaskMode.MouseLeave:
                    _mouseMonitor.Restart(ManagedWindowInfo.Handle);
                    break;
                case WindowMaskMode.HotZone:
                    _mouseMonitor.Restart(BuildHotZones(Win32Helper.GetWindowScreen(ManagedWindowInfo.Handle)));
                    break;
                case WindowMaskMode.Spotlight:
                    _mouseMonitor.Restart(ManagedWindowInfo.Handle);
                    break;
                default: throw new Exception($"意外的枚举值：{FishyTimeConfig.WindowMaskMode}");
            }
        }

        private void InitWatcher()
        {
            _windowRectWatcher = new WindowStatusWatcher();
            _windowRectWatcher.RectChanged += ManagedWindowRectChanged;
            _windowRectWatcher.TopmostChanged += ManagedWindowTopmostChanged;
            _windowRectWatcher.WindowStateChanged += ManagedWindowStateChanged;
            _windowRectWatcher.VisibilityChanged += ManagedWindowVisibilityChanged;
            _windowRectWatcher.Closed += ManagedWindowClosed;

            _mouseMonitor = _containerProvider.Resolve<MouseMonitor>();
            _mouseMonitor.MouseEnter += MouseMonitor_MouseEnter;
            _mouseMonitor.MouseLeave += MouseMonitor_MouseLeave;
            _mouseMonitor.MouseMove += MouseMonitor_MouseMove;
        }

        private void MouseMonitor_MouseMove(Point point)
        {
            if (FishyTimeConfig.WindowMaskMode != WindowMaskMode.Spotlight) return;


        }

        private void MouseMonitor_MouseLeave()
        {
            if (FishyTimeConfig.WindowMaskMode != WindowMaskMode.MouseLeave) return;

            if (!FishyTimeConfig.IsWindowMaskEnabled) return;

            if (ManagedWindowInfo is null || ManagedWindowInfo.IsMasked) return;

            ManagedWindowInfo.IsMouseOver = false;

            Win32Helper.HideWindow(ManagedWindowInfo.Handle);

            ManagedWindowInfo.IsMasked = true;
            _eventAggregator.GetEvent<MaskWindowRequestedEvent>()
                            .Publish(new MaskWindowEventArgs(ManagedWindowInfo.Bounds, ManagedWindowInfo, FishyTimeConfig.WindowMaskMode));
        }

        private void MouseMonitor_MouseEnter()
        {
            if (FishyTimeConfig.WindowMaskMode != WindowMaskMode.HotZone) return;

            if (!FishyTimeConfig.IsWindowMaskEnabled) return;

            if (ManagedWindowInfo is null || ManagedWindowInfo.IsMasked) return;

            Win32Helper.HideWindow(ManagedWindowInfo.Handle);

            ManagedWindowInfo.IsMasked = true;
            _eventAggregator.GetEvent<MaskWindowRequestedEvent>()
                            .Publish(new MaskWindowEventArgs(ManagedWindowInfo.Bounds, ManagedWindowInfo, FishyTimeConfig.WindowMaskMode));
        }

        private void ManagedWindowClosed()
        {
            _windowRectWatcher?.StopWatch();
            _mouseMonitor?.Stop();

            ManagedWindowInfo = null;

            _eventAggregator.GetEvent<MaskWindowCloseRequestedEvent>().Publish();
        }

        private void ManagedWindowVisibilityChanged(bool isVisible)
        {
            if (ManagedWindowInfo is null) return;

            ManagedWindowInfo.IsVisible = isVisible;
        }

        private void ManagedWindowStateChanged(ShowWindowCommands windowState)
        {
            _userUpdating = true;
            ManagedWindowInfo.WindowState = windowState;
            _userUpdating = false;
        }

        private void ManagedWindowRectChanged(Rect rect)
        {
            _userUpdating = true;

            ManagedWindowInfo.Position = rect.Location;
            ManagedWindowInfo.Height = rect.Height;
            ManagedWindowInfo.Width = rect.Width;

            //if (_screenCount > 1)
            //    UpdateScreenSize(ManagedWindowInfo.Handle);

            _userUpdating = false;
        }

        private void ManagedWindowTopmostChanged(bool isTopmost)
        {
            _userUpdating = true;
            ManagedWindowInfo.IsTopmost = isTopmost;
            _userUpdating = false;
        }

        public DelegateCommand<string> ApplyCommand { get; }
        private void Apply(string propertyName)
        {
            Apply(ManagedWindowInfo, propertyName);
        }

        private async void Apply(ManagedWindowInfo managedWindowInfo, string propertyName)
        {
            if (_userUpdating || managedWindowInfo is null) return;

            if (propertyName == "Rect")
            {
                await Win32Helper.SetWindowPosAsync(
                    managedWindowInfo.Handle,
                    IntPtr.Zero,
                    (int)managedWindowInfo.Position.X,
                    (int)managedWindowInfo.Position.Y,
                    (int)managedWindowInfo.Width,
                    (int)managedWindowInfo.Height,
                    Win32Helper.SWP_NOZORDER | Win32Helper.SWP_NOACTIVATE);
                return;
            }

            if (propertyName == "Topmost")
            {
                if (managedWindowInfo.IsMasked && managedWindowInfo.IsTopmost)
                {
                    Win32Helper.SetWindowTopmoastWithoutShow(managedWindowInfo.Handle, managedWindowInfo.IsTopmost);
                    return;
                }
                Win32Helper.SetWindowTopmost(managedWindowInfo.Handle, managedWindowInfo.IsTopmost);
                return;
            }

            if (propertyName == "Opacity")
            {
                await Win32Helper.SetLayeredWindowAttributesAsync(managedWindowInfo.Handle, 0, (byte)(managedWindowInfo.Opacity * 255), Win32Helper.LWA_ALPHA);
                return;
            }
        }
        public DelegateCommand ShowOrHideCommand { get; }
        private void ShowOrHide()
        {
            // TODO: 状态有 200ms 延迟
            if (ManagedWindowInfo.IsVisible)
            {
                Win32Helper.HideWindow(ManagedWindowInfo.Handle);
                return;
            }
            Win32Helper.ShowWindow(ManagedWindowInfo.Handle);
        }

        public DelegateCommand LoadDataCommand { get; }
        private void LoadData()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FishyTimeConfigFilePath));

            if (!File.Exists(FishyTimeConfigFilePath))
            {
                FishyTimeConfig = new FishyTimeConfig
                {
                    IsWindowMaskEnabled = false
                };
                return;
            }

            string configJson = File.ReadAllText(FishyTimeConfigFilePath);
            var fishyTimeConfig = JsonSerializer.Deserialize<FishyTimeConfig>(configJson);
            if (fishyTimeConfig is null)
            {
                FishyTimeConfig = new FishyTimeConfig
                {
                    IsWindowMaskEnabled = false
                };
                return;
            }

            FishyTimeConfig = fishyTimeConfig;
        }

        private static string FishyTimeConfigFilePath
            => Path.Combine(ApplicationPaths.Data, "FishyTime", FishyTimeConfigFileName);

        public DelegateCommand SaveDataCommand { get; }
        private void SaveData()
        {
            if (FishyTimeConfig is null) return;

            var configJson = JsonSerializer.Serialize(FishyTimeConfig);
            File.WriteAllText(FishyTimeConfigFilePath, configJson);

            if (!FishyTimeConfig.IsWindowMaskEnabled)
            {
                _eventAggregator.GetEvent<WindowMaskDisabledEvent>().Publish();
                return;
            }

            if (FishyTimeConfig.WindowMaskMode != WindowMaskMode.Spotlight)
                return;

            _eventAggregator
            .GetEvent<MaskWindowRequestedEvent>()
            .Publish(new MaskWindowEventArgs(ManagedWindowInfo.Bounds, ManagedWindowInfo, FishyTimeConfig.WindowMaskMode));
        }

        public DelegateCommand<WindowMaskMode?> SwtichWindowMaskModeCommand { get; }
        private void SwitchWindowMaskMode(WindowMaskMode? mode)
        {
            if (mode is null) return;

            FishyTimeConfig.WindowMaskMode = mode.Value;

            if (FishyTimeConfig.WindowMaskMode == WindowMaskMode.Spotlight)
            {
                _eventAggregator.GetEvent<MaskWindowCloseRequestedEvent>().Publish();
                _eventAggregator.GetEvent<SpotlightWindowCloseRequestedEvent>().Publish();

                _eventAggregator
                .GetEvent<MaskWindowRequestedEvent>()
                .Publish(new MaskWindowEventArgs(ManagedWindowInfo.Bounds, ManagedWindowInfo, FishyTimeConfig.WindowMaskMode));
            }
            else
            {
                _eventAggregator.GetEvent<SpotlightWindowCloseRequestedEvent>().Publish();
            }

            RestartMouseMonitor();

            SaveData();
        }
        [Logging]
        public FishyTimeHomeViewModel(IEventAggregator eventAggregator, IContainerProvider containerProvider)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _containerProvider = containerProvider ?? throw new ArgumentNullException(nameof(containerProvider));

            ApplyCommand = new DelegateCommand<string>(Apply);
            SetManagedWindowInfoCommand = new DelegateCommand<nint?>(SetManagedWindowInfo);
            ShowOrHideCommand = new DelegateCommand(ShowOrHide);
            LoadDataCommand = new DelegateCommand(LoadData);
            SaveDataCommand = new DelegateCommand(SaveData);
            SwtichWindowMaskModeCommand = new DelegateCommand<WindowMaskMode?>(SwitchWindowMaskMode);

            _eventAggregator.GetEvent<MaskWindowClosedEvent>().Subscribe(MaskWindowClosed);

            //ListeningToMonitorDeviceChanges();
            //Task.Run(() => _screenCount = WMIHelper.GetScreenCount());
            InitWatcher();
        }

        private void MaskWindowClosed(MaskWindowEventArgs args)
        {
            args.WindowInfo.IsMasked = false;

            if (args.WindowInfo is null) return;

            args.WindowInfo.Position = args.MaskedArea.Location;
            //ManagedWindowInfo.Width = bouds.Width;
            //ManagedWindowInfo.Height = bouds.Height;
            Apply(args.WindowInfo, "Rect");

            Win32Helper.ShowWindow(args.WindowInfo.Handle);
        }

        /// <summary>
        /// 监听显示器设备变化
        /// </summary>
        private void ListeningToMonitorDeviceChanges()
        {
            string query = "SELECT * FROM __InstanceOperationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.PNPClass = 'Monitor'";
            ManagementEventWatcher watcher = new ManagementEventWatcher(new WqlEventQuery(query));
            watcher.EventArrived += new EventArrivedEventHandler(DeviceEventArrived);

            Task.Run(watcher.Start);
        }

        /// <summary>
        /// 显示器设备变化事件处理程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeviceEventArrived(object sender, EventArrivedEventArgs e)
        {
            _screenCount = await WMIHelper.GetScreenCountAsync();

            if (ManagedWindowInfo is null) return;

            UpdateScreenSize(ManagedWindowInfo.Handle);
        }
        private static ManagedWindowInfo GetManagedWindowInfo(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return null;

            double opacity = Win32Helper.GetWindowOpacity(handle);
            ManagedWindowInfo managedWindowInfo = new()
            {
                Handle = handle,
                Name = Win32Helper.GetWindowTitle(handle),
                Opacity = opacity,
                IsTopmost = Win32Helper.IsWindowTopmost(handle),
                CanSetOpacity = Win32Helper.TrySetWindowOpacity(handle, (byte)(opacity * 255)),
                WindowState = Win32Helper.GetWindowState(handle),
                Icon = Win32Helper.GetWindowIcon(handle)
            };

            var windowRect = Win32Helper.GetWindowRect(handle);
            if (windowRect == Rect.Empty)
                return managedWindowInfo;

            managedWindowInfo.Position = windowRect.Location;
            managedWindowInfo.Height = windowRect.Height;
            managedWindowInfo.Width = windowRect.Width;
            return managedWindowInfo;
        }

        private void UpdateScreenSize(IntPtr managedWindowHandle)
        {
            WinForms::Screen targetScreen = Win32Helper.GetWindowScreen(managedWindowHandle);
            TargetScreenWidth = targetScreen is null ? 1920 : targetScreen.Bounds.Width;
            TargetScreenHeight = targetScreen is null ? 1080 : targetScreen.Bounds.Height;
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

    }
}
