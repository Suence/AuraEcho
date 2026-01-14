namespace PowerLab.UpdaterService

open System
open System.IO
open System.Net.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open PowerLab.Core.Constants
open PowerLab.Core.Contracts
open PowerLab.Core.Data
open PowerLab.Core.Repositories
open PowerLab.Core.Services
open PowerLab.Core.Tools
open PowerLab.Core.Tools.HttpClientPipelines
open PowerLab.PluginContracts.Interfaces


module Program =

    let logDir = 
        Path.Combine(
            Environment.GetFolderPath Environment.SpecialFolder.CommonApplicationData,
            "PowerLab",
            "UpdaterService",
            "Logs");

    let configureServices (services: IServiceCollection) =
        services
            .AddHostedService<Worker>()
            .AddDbContext<PowerLabDbContext>(fun options -> 
                options.UseSqlite $"Data Source={ApplicationPaths.HostDataBase}" |> ignore)
            .AddSingleton<IAppLogger>(new Serilogger(logDir))
            .AddSingleton<HttpClient>(fun sp ->
                let logger = sp.GetRequiredService<IAppLogger>()
                let logHandler = new LoggingHandler(logger, InnerHandler = new HttpClientHandler())
                new HttpClient(logHandler))
            .AddScoped<HttpHelper>(fun sp ->
                let client = sp.GetRequiredService<HttpClient>()
                HttpHelper(client))
            .AddScoped<IFileRepository, FileRepository>()
            .AddScoped<IAppPackageRepository, AppPackageRepository>()
            .AddScoped<ILocalPluginRepository, LocalPluginRepository>()
            .AddScoped<IRemotePluginRepository, RemotePluginRepository>()
        |> ignore

    [<EntryPoint>]
    let main args =

        Directory.CreateDirectory logDir |> ignore;

        let builder = 
            Host.CreateDefaultBuilder(args)
                .UseWindowsService(fun options -> options.ServiceName <- "PowerLab Updater Service")
                .ConfigureServices(configureServices)
                .Build()
                .Run()

        0