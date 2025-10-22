using PowerLab.UpdaterService;
using Serilog;

Log.Logger = new LoggerConfiguration()
            .WriteTo.File(@"D:\Workspace\Personal\DNFPowerLabUpdaterLog\updater.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "PowerLab Updater Service";
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .UseSerilog()
    .Build();
host.Run();
