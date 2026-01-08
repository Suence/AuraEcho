namespace PowerLab.LauncherService

open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System.IO
open Serilog

module Program =
    let logDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "PowerLab",
        "LauncherService",
        "Logs")

    [<EntryPoint>]
    let main args =

        Log.Logger <-
            LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(logDir, "LauncherService-.log"),
                    rollingInterval = RollingInterval.Day,
                    retainedFileCountLimit = Nullable 7)
                .CreateLogger()

        Host.CreateDefaultBuilder(args)
            .UseWindowsService(fun opt -> opt.ServiceName <- "PowerLabLauncherService")
            .ConfigureServices(fun services -> services.AddHostedService<LauncherWorker>() |> ignore)
            .UseSerilog()
            .Build()
            .Run()
        0