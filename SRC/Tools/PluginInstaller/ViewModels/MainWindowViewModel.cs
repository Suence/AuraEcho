using PluginInstaller.Constants;
using PluginInstaller.Tools;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.IO;

namespace PluginInstaller.ViewModels;

public class MainWindowViewModel : BindableBase
{
    #region private members
    private string _title = "PowerLab 扩展安装向导";
    private readonly IRegionManager _regionManager;
    #endregion

    /// <summary>
    /// 窗口标题
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public DelegateCommand NavigationToInitPageCommand { get; }
    private void NavigationToInitPage()
    {
        string? filePath = GlobalObjectHolder.StartupArgs?.FirstOrDefault();
        if (filePath is null || !File.Exists(filePath))
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, ViewNames.PickPluginInstallFile);
            return;
        }

        _regionManager.RequestNavigate(
            RegionNames.MainRegion,
            ViewNames.InstallPreparation,
            new NavigationParameters
            {
                { "PluginInstallFilePath", filePath }
            });
    }

    public MainWindowViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;

        NavigationToInitPageCommand = new DelegateCommand(NavigationToInitPage);
    }
}
