using System;
using System.Collections.ObjectModel;
using System.IO;
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
        private Win32Window _managedWindowInfo;
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
        public Win32Window ManagedWindowInfo
        {
            get => _managedWindowInfo;
            set => SetProperty(ref _managedWindowInfo, value);
        }

        public FishyTimeConfig FishyTimeConfig
        {
            get => _fishyTimeConfig;
            set => SetProperty(ref _fishyTimeConfig, value);
        }

        public DelegateCommand<IntPtr?> SetManagedWindowInfoCommand { get; }
        private async void SetManagedWindowInfo(IntPtr? handle)
        {
            if (handle is null or 0) return;
            if (handle == ManagedWindowInfo?.Handle || handle == ManagedWindowInfo?.MaskHandle) return;

            ManagedWindowInfo = new Win32Window(handle.Value);
            await ManagedWindowInfo.LoadAsync();
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
            SetManagedWindowInfoCommand = new DelegateCommand<nint?>(SetManagedWindowInfo);

            LoadDataCommand = new DelegateCommand(LoadData);
            SaveDataCommand = new DelegateCommand(SaveData);
        }

    }
}
