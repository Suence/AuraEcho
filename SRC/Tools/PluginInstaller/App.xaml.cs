using DryIoc;
using Microsoft.EntityFrameworkCore;
using PluginInstaller.Tools;
using PluginInstaller.Views;
using PowerLab.Core.Attributes;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Data;
using PowerLab.Core.Native.Win32;
using PowerLab.Core.Repositories;
using PowerLab.Core.Services;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.UIToolkit.RegionDialog;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using System.IO;
using System.Windows;

namespace PluginInstaller;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
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
        containerRegistry.RegisterSingleton<IRegionDialogService, RegionDialogService>();

        containerRegistry.RegisterForNavigation<InstallPreparation>();
        containerRegistry.RegisterForNavigation<Installing>();
        containerRegistry.RegisterForNavigation<InstallCompleted>();
        containerRegistry.RegisterForNavigation<PickPluginInstallFile>();
        containerRegistry.RegisterForNavigation<ConfirmDialog>();
        //containerRegistry.Register<PluginDbContext>(provider =>
        //{
        //    var dbPath = Path.Combine(ApplicationPaths.Data, "powerlab.db");
        //    var options = new DbContextOptionsBuilder<PluginDbContext>()
        //        .UseSqlite($"Data Source={dbPath}")
        //        .Options;
        //    return new PluginDbContext(options);
        //});
        containerRegistry.Register<ILocalPluginRepository, LocalPluginRepository>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        GlobalObjectHolder.StartupArgs = e.Args;
        base.OnStartup(e);

        var dbPath = Path.Combine(ApplicationPaths.Data, "powerlab.db");
        if (!File.Exists(dbPath))
        {
            using var pluginDbContext = Container.Resolve<PowerLabDbContext>();
            pluginDbContext.Database.Migrate();
        }
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

        using var mutex = new Mutex(true, MutexNames.PLUGIN_INSTALLER_MUTEX_ID);
        if (!mutex.WaitOne(TimeSpan.Zero, true))
        {
            IntPtr mainWindowHandle = Win32Helper.FindWindow(null, "PlixInstaller");
            if (mainWindowHandle != IntPtr.Zero)
                Win32Helper.SetForegroundWindow(mainWindowHandle);

            logger.Debug("已有实例正在运行，正在退出程序。");
            return;
        }
        logger = null;

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
