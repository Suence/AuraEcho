namespace PowerLab.LauncherService

open System
open System.IO
open System.IO.Pipes
open System.Security.AccessControl
open System.Security.Principal
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System.Threading

type LauncherWorker(logger : ILogger<LauncherWorker>) =
    inherit BackgroundService()

    [<Literal>]
    let PIPE_NAME = "POWERLAB_LAUNCHER_SERVICE_PIPE"
    
    let createProcessInUserSession exePath =
        match File.Exists(exePath) with
        | true -> exePath |> UserSessionProcessLauncher.launch |> ignore
        | false -> 
            logger.LogWarning("Executable path {ExePath} does not exist.", exePath)

    let createPipeSecurity = 
        let ps = PipeSecurity()
        ps.AddAccessRule(
            PipeAccessRule(
                SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow))
        ps

    let createNamedPipeServer pipeSecurity =
        NamedPipeServerStreamAcl.Create(
            PIPE_NAME,
            PipeDirection.In,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            0,
            0,
            pipeSecurity)

    let pipeLoop (ct: CancellationToken) = task {
        logger.LogInformation("Pipe server loop started.")
        
        while not ct.IsCancellationRequested do
            try
                use server = createNamedPipeServer createPipeSecurity
                
                do! server.WaitForConnectionAsync(ct) |> Async.AwaitTask
                logger.LogInformation("Pipe server connected.")
                use reader = new StreamReader(server)
                let! cmd = reader.ReadLineAsync() |> Async.AwaitTask
                
                match cmd |> Option.ofObj with
                | Some c -> createProcessInUserSession c
                | None -> ()
                
                logger.LogDebug("Message processed, restarting server instance.")
            with
            | :? OperationCanceledException -> 
                logger.LogInformation("Pipe server shutting down...")
            | ex -> 
                logger.LogError(ex, "Error in pipe loop")
                do! Async.Sleep 1000
    }
    
    override _.ExecuteAsync(ct : CancellationToken) = task {
       logger.LogInformation("PowerLabLauncherService is started.")
       ct |> pipeLoop |> ignore
    }

    override _.StopAsync (cancellationToken: CancellationToken): Tasks.Task = 
        logger.LogInformation "PowerLabLauncherService is stopped."
        base.StopAsync(cancellationToken: CancellationToken)
        