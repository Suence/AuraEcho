namespace AuraEcho.LauncherService

open System
open System.IO
open System.IO.Pipes
open System.Security.AccessControl
open System.Security.Principal
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System.Threading
open System.Linq

type LauncherWorker(logger : ILogger<LauncherWorker>) =
    inherit BackgroundService()

    [<Literal>]
    let PIPE_NAME = "AURAECHO_LAUNCHER_SERVICE_PIPE"

    let resolveCommandLine (commandLine:string) =
        let fileName = Path.GetFileName(commandLine).Split(" ").First()
        match fileName with
        | "" -> None
        | _ -> 
            let fileFullPath = Path.Combine(Path.GetDirectoryName(commandLine), fileName)

            let args = Path.GetFileName(commandLine).Split(" ").Skip(1)

            match args.Count() with
            | 0 -> Some (commandLine, "")
            | _ -> Some (fileFullPath, String.Join(" ", args))
    
    let createProcessInUserSession commandLine =
        match resolveCommandLine commandLine with
        | None -> logger.LogWarning("invalid commandline")
        | Some (exePath, args) ->
            match File.Exists(exePath) with
            | true -> 
                (exePath, args) 
                |> UserSessionProcessLauncher.launch 
                |> (fun launchResult -> logger.LogWarning("launched file: {ExePath}, args: {args}, result: {launchResult}", exePath, args, launchResult))
            | false -> logger.LogWarning("Executable path {ExePath} does not exist.", exePath)

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
       logger.LogInformation("AuraEchoLauncherService is started.")
       ct |> pipeLoop |> ignore
    }

    override _.StopAsync (cancellationToken: CancellationToken): Tasks.Task = 
        logger.LogInformation "AuraEchoLauncherService is stopped."
        base.StopAsync(cancellationToken: CancellationToken)
        