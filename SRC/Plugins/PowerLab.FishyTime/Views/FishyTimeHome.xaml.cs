using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using PowerLab.Core.Attributes;
using PowerLab.Core.Contracts;
using PowerLab.FishyTime.Events;
using PowerLab.FishyTime.Models;
using PowerLab.FishyTime.Utils;
using PowerLab.FishyTime.ViewModels;
using Prism.Events;
using Prism.Ioc;
using WinForms = System.Windows.Forms;
namespace PowerLab.FishyTime.Views
{
    /// <summary>
    /// Interaction logic for FishyTimeHome
    /// </summary>
    public partial class FishyTimeHome : UserControl
    {
        private readonly List<Window> _blackWindows = [];
        private Window _ownerWindow;
        private readonly IEventAggregator _eventAggregator;
        private readonly IContainerProvider _containerProvider;
        private nint _maskWindowHandle;

        [Logging]
        public FishyTimeHome(ILogger logger, IEventAggregator eventAggregator, IContainerProvider container)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _containerProvider = container ?? throw new ArgumentNullException(nameof(container));

            _eventAggregator.GetEvent<MaskWindowRequestedEvent>().Subscribe(OpenMaskWindow, ThreadOption.UIThread);

            InitializeComponent();
        }


        private void OpenMaskWindow(MaskWindowEventArgs args)
        {
            if (args.WindowMaskMode == WindowMaskMode.Spotlight)
            {
                var spotlightWindow = _containerProvider.Resolve<SpotlightWindow>();
                spotlightWindow.Left = args.WindowInfo.Bounds.Left;
                spotlightWindow.Top = args.WindowInfo.Bounds.Top;
                spotlightWindow.Width = args.WindowInfo.Width;
                spotlightWindow.Height = args.WindowInfo.Height;
                spotlightWindow.Topmost = true;
                spotlightWindow.Show();
                return;
            }

            var maskWindow = _containerProvider.Resolve<MaskWindow>();
            maskWindow.Left = args.WindowInfo.Bounds.Left;
            maskWindow.Top = args.WindowInfo.Bounds.Top;
            maskWindow.Width = args.WindowInfo.Width;
            maskWindow.Height = args.WindowInfo.Height;
            maskWindow.Closing += (s, e) => 
                _eventAggregator.GetEvent<MaskWindowClosedEvent>()
                                .Publish(new MaskWindowEventArgs(maskWindow.RestoreBounds, args.WindowInfo, WindowMaskMode.MouseLeave));

            maskWindow.Show();
            _maskWindowHandle = new WindowInteropHelper(maskWindow).Handle;
        }

        private void OpenWindowsUpdateWindowButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllFullScreenWindows();

            foreach (var screen in WinForms::Screen.AllScreens)
            {
                Window fullScreenWindow =
                    screen.Primary
                    ? new WindowsUpdateWindow()
                    : new Window();
                fullScreenWindow.WindowStyle = WindowStyle.None;
                fullScreenWindow.ResizeMode = ResizeMode.NoResize;
                fullScreenWindow.Left = screen.Bounds.Left;
                fullScreenWindow.Top = screen.Bounds.Top;
                fullScreenWindow.Width = screen.Bounds.Width;
                fullScreenWindow.Height = screen.Bounds.Height;
                fullScreenWindow.Topmost = true;
                fullScreenWindow.Cursor = Cursors.None;
                fullScreenWindow.Background = Brushes.Black;
                fullScreenWindow.ShowInTaskbar = false;

                fullScreenWindow.InputBindings.Add(new InputBinding(ApplicationCommands.Close, new KeyGesture(Key.Escape)));
                fullScreenWindow.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, FullScreenWindowEsc_Click));
                fullScreenWindow.Closed += FullScreenWindowClosed;
                _blackWindows.Add(fullScreenWindow);
                fullScreenWindow.Show();
            }
        }

        private void FullScreenWindowEsc_Click(object sender, ExecutedRoutedEventArgs e) => CloseAllFullScreenWindows();
        private void FullScreenWindowClosed(object sender, EventArgs e)
            => CloseAllFullScreenWindows();
        private void CloseAllFullScreenWindows()
        {
            if (_blackWindows.Count == 0) return;

            _blackWindows.ToList().ForEach(window => window.Close());
            _blackWindows.Clear();
        }


        private void PickButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture((IInputElement)sender);
            Cursor = Cursors.Cross;
        }

        private void PickButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            Cursor = Cursors.Arrow;

            // 获取鼠标所在窗口句柄
            var hwnd = Win32Helper.GetTopLevelWindowUnderMouse();
            if (hwnd == IntPtr.Zero) return;

            var ownerWindowHandle = new WindowInteropHelper(_ownerWindow).Handle;
            if (hwnd == ownerWindowHandle || hwnd == _maskWindowHandle) return;

            (DataContext as FishyTimeHomeViewModel).SetManagedWindowInfoCommand.Execute(hwnd);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _ownerWindow ??= Window.GetWindow(this);
        }
    }
}
