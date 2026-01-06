namespace PowerLab.LauncherService

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

module Program =

    [<EntryPoint>]
    let main args =
        Host.CreateDefaultBuilder(args)
            .UseWindowsService(fun opt ->
                opt.ServiceName <- "PowerLabLauncherService")
            .ConfigureServices(fun services ->
                services.AddHostedService<LauncherWorker>() |> ignore)
            .Build()
            .Run()
        0