using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using DryIoc;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.EntityFrameworkCore;
using PowerLab.Constants;
using PowerLab.Core.Attributes;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Data;
using PowerLab.Core.Events;
using PowerLab.Core.Native.Win32;
using PowerLab.Core.Repositories;
using PowerLab.Core.Services;
using PowerLab.Core.Tools;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.PluginContracts.Models;
using PowerLab.Services;
using PowerLab.UIToolkit.RegionDialog;
using PowerLab.ViewModels;
using PowerLab.Views;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PowerLab;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private const string PIPE_NAME = "PowerLab_SingleInstance_Pipe";

    private string[] _startupArgs;
    private TaskbarIcon _notifyIcon;
    protected override Window CreateShell()
    {
        LoggingAttribute.Logger = Container.Resolve<ILogger>();
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<ILogger, SerilogService>();
        containerRegistry.RegisterSingleton<IPathProvider, PathProvider>();
        containerRegistry.RegisterSingleton<IFileDialogService, FileDialogService>();
        containerRegistry.RegisterSingleton<IPluginManager, PluginManager>();
        containerRegistry.RegisterSingleton<IThemeManager, ThemeManager>();
        containerRegistry.RegisterSingleton<IHostSettingsProvider, HostSettingsProvider>();
        containerRegistry.RegisterSingleton<ILocalPluginRepository, LocalPluginRepository>();
        containerRegistry.RegisterSingleton<IRegionDialogService, RegionDialogService>();
        containerRegistry.RegisterSingleton<INavigationService, NavigationService>();

        containerRegistry.RegisterSingleton<IFileRepository, FileRepository>();
        containerRegistry.RegisterSingleton<IAppPackageRepository, AppPackageRepository>();
        containerRegistry.RegisterSingleton<IRemotePluginRepository, RemotePluginRepository>();

        containerRegistry.RegisterForNavigation<Homepage>();
        containerRegistry.RegisterForNavigation<PluginsDashboard>();
        containerRegistry.RegisterForNavigation<Settings>();
        containerRegistry.RegisterForNavigation<GeneralSettings>();
        containerRegistry.RegisterForNavigation<ConfirmDialog>();
        containerRegistry.RegisterForNavigation<PluginsMarketplace>();
        containerRegistry.RegisterForNavigation<MarketplacePluginDetails>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {

    }

    protected override void OnInitialized()
    {
        if (!_startupArgs.Contains("--boot"))
        {
            base.OnInitialized();
        }

        //var regionManager = Container.Resolve<IRegionManager>();
        //regionManager.RegisterViewWithRegion(HostRegionNames.MainRegion, typeof(Homepage));
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        _startupArgs = e.Args;
        base.OnStartup(e);

        StartPipeServer();

        RegisterEvents();
        LoadConfig();

        _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        _notifyIcon.DataContext = Container.Resolve<NotifyIconViewModel>();

        string dbPath = Path.Combine(ApplicationPaths.Data, "powerlab.db");
        if (!File.Exists(dbPath))
        {
            using var pluginDbContext = Container.Resolve<PowerLabDbContext>();

            var logger = Container.Resolve<ILogger>();
            logger.Information("Begin Migrate");
            pluginDbContext.Database.Migrate();
            logger.Information("End Migrate");
        }
    }

    private void LoadConfig()
    {
        var hostSettingsProvider = Container.Resolve<IHostSettingsProvider>();
        var hostSettings = hostSettingsProvider.LoadHostSettings();

        Container.Resolve<IThemeManager>().CurrentTheme = hostSettings.AppTheme;
        var targetCultureInfo = hostSettings.AppLanguage switch
        {
            AppLanguage.ChineseSimplified => new CultureInfo("zh-CN"),
            AppLanguage.English => new CultureInfo("en-US"),
            _ => CultureInfo.CurrentCulture
        };
        ApplicationResources.ChangeCulture(targetCultureInfo);

        RenderOptions.ProcessRenderMode =
            hostSettings.HardwareAcceleration
            ? RenderMode.Default
            : RenderMode.SoftwareOnly;
    }

    /// <summary>
    /// 程序入口函数
    /// </summary>
    [STAThread]
    static void Main()
    {
        var logger = new SerilogService();
        logger.Debug("程序已启动");

        if (Mutex.TryOpenExisting(MutexNames.INSTALLER_MUTEX_ID, out var _))
        {
            logger.Debug("检测到安装程序正在运行，正在退出程序。");
            return;
        }

        using var mutex = new Mutex(true, MutexNames.POWERLAB_MUTEX_ID);
        if (!mutex.WaitOne(TimeSpan.Zero, true))
        {
            using var client = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.Out);
            client.Connect(200);
            using var writer = new StreamWriter(client);
            writer.WriteLine(NamedPipeMessages.ShowWindow);
            writer.Flush();

            logger.Debug("已有实例正在运行，正在退出程序。");
            return;
        }
        logger = null;

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

    private static void StartPipeServer()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                using var server = new NamedPipeServerStream(
                    PIPE_NAME,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync();

                using var reader = new StreamReader(server);
                string? cmd = await reader.ReadLineAsync();

                if (cmd == NamedPipeMessages.ShowWindow)
                {
                    IEventAggregator eventAggregator = (Current as App)!.Container.Resolve<IEventAggregator>();
                    eventAggregator.GetEvent<RequestShowAppEvent>().Publish();
                }
            }
        });
    }

    /// <summary>
    /// 订阅全局异常处理事件
    /// </summary>
    private void RegisterEvents()
    {
        //Task线程内未捕获异常处理事件
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        //UI线程未捕获异常处理事件（UI主线程）
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }
    /// <summary>
    /// Task 线程异常处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            if (e.Exception is Exception exception)
            {
                HandleException(exception);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        finally
        {
            e.SetObserved();
        }
    }

    /// <summary>
    /// UI 线程异常处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            HandleException(e.Exception);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        finally
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// 非 UI 线程异常处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            if (e.ExceptionObject is Exception exception)
            {
                HandleException(exception);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        finally
        {

        }
    }

    /// <summary>
    /// 全局异常处理逻辑
    /// </summary>
    /// <param name="exception"></param>
    private void HandleException(Exception exception)
    {
        Debug.WriteLine(exception);
        LoggingAttribute.Logger.Debug(exception.ToString());
    }
}
