using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using PowerLab.Core.Attributes;
using PowerLab.Core.Constants;
using PowerLab.FishyTime.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace PowerLab.FishyTime.ViewModels
{
    public class FishyTimeHomeViewModel : BindableBase
    {
        #region private members
        private FishyTimeConfig _fishyTimeConfig;
        private const string FishyTimeConfigFileName = "FishyTimeConfig.json";
        private ObservableCollection<Win32Window> _win32Windows;
        private ObservableCollection<WindowMaskMode> _windowMaskModes =
        [
            WindowMaskMode.MouseLeave,
            WindowMaskMode.HotZone,
            WindowMaskMode.Spotlight
        ];
        #endregion

        public ObservableCollection<Win32Window> Win32Windows
        {
            get => _win32Windows;
            set => SetProperty(ref _win32Windows, value);
        }

        public ObservableCollection<WindowMaskMode> WindowMaskModes
        {
            get => _windowMaskModes;
            set => SetProperty(ref _windowMaskModes, value);
        }

        public FishyTimeConfig FishyTimeConfig
        {
            get => _fishyTimeConfig;
            set => SetProperty(ref _fishyTimeConfig, value);
        }

        public DelegateCommand<IntPtr?> AddWin32WindowCommand { get; }
        private async void AddWin32Window(IntPtr? handle)
        {
            if (handle is null or 0) return;

            var isExist = Win32Windows.Any(w => w.Handle == handle || w.MaskHandle == handle);
            if (isExist) return;

            var win32Window = new Win32Window(handle.Value);
            await win32Window.LoadAsync();
            win32Window.Closed += Win32WindowClosed;
            
            Win32Windows.Add(win32Window);
        }

        private void Win32WindowClosed(Win32Window win32Window)
        {
            if (win32Window is null) return;

            RemoveWin32Window(win32Window);
        }

        public DelegateCommand<Win32Window> RemoveWin32WindowCommand { get; }
        private void RemoveWin32Window(Win32Window win32Window)
        {
            if (win32Window is null) return;

            if (Win32Windows.Remove(win32Window))
            {
                win32Window.Dispose();
            }
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
        }
        [Logging]
        public FishyTimeHomeViewModel()
        {
            Win32Windows = new ObservableCollection<Win32Window>();
            AddWin32WindowCommand = new DelegateCommand<nint?>(AddWin32Window);
            RemoveWin32WindowCommand = new DelegateCommand<Win32Window>(RemoveWin32Window);

            LoadDataCommand = new DelegateCommand(LoadData);
            SaveDataCommand = new DelegateCommand(SaveData);
        }

    }
}
