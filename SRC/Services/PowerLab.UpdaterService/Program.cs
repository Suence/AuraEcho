using PowerLab.UpdaterService;
using Serilog;

var logDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "PowerLab",
    "UpdaterService",
    "Logs");

Directory.CreateDirectory(logDir);

Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(logDir, "updater.log"), rollingInterval: RollingInterval.Day)
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
