using PowerLab.Constants;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Data.Entities;
using PowerLab.Core.Events;
using PowerLab.Core.Models;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PowerLab.ViewModels;

public class MarketplacePluginDetailsViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
{
    private readonly IRemotePluginRepository _pluginRepository;
    private readonly IPluginInstallService _pluginInstallService;
    private readonly INavigationService _navigationService;
    private readonly IEventAggregator _eventAggregator;
    private readonly IPluginManager _pluginManager;

    public AppPlugin Plugin
    {
        get => field;
        set => SetProperty(ref field, value);
    }
    public bool IsInstalling
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public double InstallProgress
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public string InstallingMessage
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public PluginPackage LatestVersionInfo
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public DelegateCommand InstallCommand { get; }
    private async void Install()
    {
        IsInstalling = true;
        InstallProgress = 0;
        InstallingMessage = String.Empty;

        InstallingMessage = "Downloading...";
        Progress<double> progress = new Progress<double>(p => InstallProgress = p);
        var pluginInstallerFilePath = Path.Combine(ApplicationPaths.Temp, $"{Plugin.Id}.plix");
        var result = await _pluginRepository.DownloadLatestAsync(Plugin.Id, "stable", pluginInstallerFilePath, progress);
        if (!result)
        {
            IsInstalling = false;
            throw new Exception();
        }

        InstallingMessage = "installing...";
        var pluginRegistry = await _pluginInstallService.InstallAsync(pluginInstallerFilePath);
        File.Delete(pluginInstallerFilePath);
        if (pluginRegistry is null)
        {
            IsInstalling = false;
            throw new Exception();
        }
        var installResult = await _pluginManager.LoadPluginAsync(pluginRegistry);
        if (!installResult)
        {
            IsInstalling = false;
            throw new Exception();
        }
        Plugin.IsInstalled = true;
        _eventAggregator.GetEvent<PluginInstalledEvent>().Publish(pluginRegistry);
        IsInstalling = false;
    }

    public DelegateCommand OpenPluginCommand { get; }
    private void OpenPlugin()
    {
        PluginRegistryModel? targetRegistry = 
            _pluginManager.Plugins.FirstOrDefault(p => p.Manifest.Id == Plugin.Id) 
            ?? throw new Exception();

        _navigationService.RequestNavigate(
            HostRegionNames.MainRegion,
            targetRegistry.Manifest.DefaultViewName);
    }

    private async Task LoadPluginDetails()
    {
        var result = await _pluginRepository.GetLatestAsync(Plugin.Id);
        if (result is null) return;

        LatestVersionInfo = result;
    }
    public MarketplacePluginDetailsViewModel(
        IRemotePluginRepository pluginRepository,
        INavigationService navigationService,
        IEventAggregator eventAggregator,
        IPluginInstallService pluginInstallService, 
        IPluginManager pluginManager)
    {
        _pluginRepository = pluginRepository;
        _pluginInstallService = pluginInstallService;
        _navigationService = navigationService;
        _pluginManager = pluginManager;
        _eventAggregator = eventAggregator;

        OpenPluginCommand = new DelegateCommand(OpenPlugin);
        InstallCommand = new DelegateCommand(Install);
    }

    public bool KeepAlive => false;

    public bool IsNavigationTarget(NavigationContext navigationContext)
        => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        Plugin = navigationContext.Parameters["Plugin"] as AppPlugin;
        _ = LoadPluginDetails();
    }
}
