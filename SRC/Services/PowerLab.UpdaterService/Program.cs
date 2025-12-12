using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Data;
using PowerLab.Core.Repositories;
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

        services.AddDbContext<PowerLabDbContext>(options => options.UseSqlite($"Data Source={ApplicationPaths.HostDataBase}"));

        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IAppPackageRepository, AppPackageRepository>();
        services.AddScoped<ILocalPluginRepository, LocalPluginRepository>();
        services.AddScoped<IRemotePluginRepository, RemotePluginRepository>();
    })
    .UseSerilog()
    .Build();
host.Run();
