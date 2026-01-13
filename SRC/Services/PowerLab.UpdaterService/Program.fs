namespace PowerLab.UpdaterService

open System.IO
open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open PowerLab.Core.Data
open Microsoft.EntityFrameworkCore
open PowerLab.PluginContracts.Interfaces
open PowerLab.Core.Services
open PowerLab.Core.Constants
open PowerLab.Core.Tools.HttpClientPipelines
open System.Net.Http
open PowerLab.Core.Contracts
open PowerLab.Core.Repositories
open PowerLab.Core.Tools

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
                new HttpHelper(client))
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