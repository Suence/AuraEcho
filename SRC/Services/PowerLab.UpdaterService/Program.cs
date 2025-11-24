using System.Diagnostics;
using PowerLab.UpdaterService;
using PowerLab.UpdaterService.Constants;
using PowerLab.UpdaterService.Contracts;
using PowerLab.UpdaterService.Services;
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
        services.AddSingleton<IFileRespository, FileRespository>();
        services.AddSingleton<IPackageRespository, PackageRespository>();
    })
    .UseSerilog()
    .Build();

host.Run();
