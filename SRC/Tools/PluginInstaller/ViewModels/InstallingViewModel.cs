using PluginInstaller.Constants;
using PluginInstaller.Tools;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Tools;
using PowerLab.PluginContracts.Attributes;
using PowerLab.PluginContracts.Interfaces;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System.IO;
using System.Reflection;

namespace PluginInstaller.ViewModels;

/// <summary>
/// 正在安装
/// </summary>
public class InstallingViewModel : BindableBase, INavigationAware
{
    #region private members
    private readonly IRegionManager _regionManager;
    private readonly IPluginInstallService _pluginInstallService;
    private string _installFilePath;
    #endregion


    public DelegateCommand InstallPluginCommand { get; }
    /// <summary>
    /// 安装模块
    /// </summary>
    private async void InstallPlugin()
    {
        await _pluginInstallService.InstallAsync(_installFilePath);
        
        _regionManager.RequestNavigate(
            RegionNames.MainRegion,
            ViewNames.InstallCompleted);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="regionManager"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public InstallingViewModel(IRegionManager regionManager, IPluginInstallService pluginInstallService)
    {
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        _pluginInstallService = pluginInstallService ?? throw new ArgumentNullException(nameof(regionManager));

        InstallPluginCommand = new DelegateCommand(InstallPlugin);
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
        => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _installFilePath = navigationContext.Parameters.GetValue<string>("FilePath");
    }
}
