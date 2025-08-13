using System.Windows;
using DryIoc;
using PluginInstaller.Constants;
using PluginInstaller.Tools;
using PluginInstaller.Views;
using PowerLab.Core.Attributes;
using PowerLab.Core.Contracts;
using PowerLab.Core.Native.Win32;
using PowerLab.Core.Services;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PluginInstaller
{
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
            containerRegistry.RegisterForNavigation<InstallPreparation>();
            containerRegistry.RegisterForNavigation<Installing>();
            containerRegistry.RegisterForNavigation<InstallCompleted>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
        }
        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate(RegionNames.MainRegion, nameof(InstallPreparation));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            GlobalObjectHolder.StartupArgs = e.Args;
        }

        /// <summary>
        /// 程序入口函数
        /// </summary>
        [STAThread]
        static void Main()
        {
            var logger = new SerilogService();

            logger.Debug("程序已启动");

            using var mutex = new Mutex(true, "7DE0BAA8-9D9A-46E1-82C1-947DBAABEE78");
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

}
