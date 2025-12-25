using Microsoft.EntityFrameworkCore;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Data;
using PowerLab.Core.Repositories;
using PowerLab.Core.Services;
using PowerLab.Core.Tools;
using PowerLab.Core.Tools.HttpClientPipelines;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.UpdaterService;

var logDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "PowerLab",
    "UpdaterService",
    "Logs");

Directory.CreateDirectory(logDir);

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "PowerLab Updater Service";
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();

        services.AddDbContext<PowerLabDbContext>(options => options.UseSqlite($"Data Source={ApplicationPaths.HostDataBase}"));
        services.AddSingleton<IAppLogger>(new Serilogger(logDir));
        services.AddSingleton(c =>
        {
            var logger = c.GetRequiredService<IAppLogger>();
            var logHandler = new LoggingHandler(logger)
            {
                InnerHandler = new HttpClientHandler()
            };
            return new HttpClient(logHandler);
        });
        services.AddScoped(c =>
        {
            var client = c.GetRequiredService<HttpClient>();
            return new HttpHelper(client);
        });
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IAppPackageRepository, AppPackageRepository>();
        services.AddScoped<ILocalPluginRepository, LocalPluginRepository>();
        services.AddScoped<IRemotePluginRepository, RemotePluginRepository>();
    })
    .Build();
host.Run();
