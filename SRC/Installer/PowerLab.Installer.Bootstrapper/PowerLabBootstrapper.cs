using System.Diagnostics;
using WixToolset.BootstrapperApplicationApi;

namespace PowerLab.Installer.Bootstrapper
{
    public sealed class PowerLabBootstrapper : BootstrapperApplication
    {
        public IEngine Engine { get; private set; }
        public IBootstrapperCommand Command { get; private set; }

        protected override void OnCreate(CreateEventArgs args)
        {
            base.OnCreate(args);
            Engine = args.Engine;
            Command = args.Command;
        }

        protected override void Run()
        {
            if (Environment.GetCommandLineArgs().Contains("-debug", StringComparer.OrdinalIgnoreCase))
                Debugger.Launch();

            // 稍后要在这里添加安装流程控制。
            Engine.Log(LogLevel.Standard, "Running the Wix3Demo.InstallerUI.");
            try
            {
                LaunchUI();
                Engine.Log(LogLevel.Standard, "Exiting the Wix3Demo.InstallerUI.");
                Engine.Quit(0);
            }
            catch (Exception ex)
            {
                Engine.Log(LogLevel.Error, $"The Wix3Demo.InstallerUI is failed: {ex}");
                Engine.Quit(-1);
            }
            finally
            {
                Engine.Log(LogLevel.Standard, "The Wix3Demo.InstallerUI has exited.");
            }
        }

        private void LaunchUI()
        {
            //GlobalObjectHolder.BAInstance = this;

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
