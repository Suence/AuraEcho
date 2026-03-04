namespace AuraEcho.UpdaterService

open System
open System.IO
open System.Net.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open AuraEcho.Core.Constants
open AuraEcho.Core.Contracts
open AuraEcho.Core.Data
open AuraEcho.Core.Repositories
open AuraEcho.Core.Services
open AuraEcho.Core.Tools
open AuraEcho.Core.Tools.HttpClientPipelines
open AuraEcho.PluginContracts.Interfaces


module Program =

    let logDir = 
        Path.Combine(
            Environment.GetFolderPath Environment.SpecialFolder.CommonApplicationData,
            "AuraEcho",
            "UpdaterService",
            "Logs");

    let configureServices (services: IServiceCollection) =
        services
            .AddHostedService<Worker>()
            .AddDbContext<AuraEchoDbContext>(fun options ->
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
                .UseWindowsService(fun options -> options.ServiceName <- "AuraEcho Updater Service")
                .ConfigureServices(configureServices)
                .Build()
                .Run()

        0