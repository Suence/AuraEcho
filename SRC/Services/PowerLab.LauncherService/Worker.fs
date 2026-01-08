namespace PowerLab.LauncherService

open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System.Diagnostics
open System.Threading
open Microsoft.Win32

type LauncherWorker(logger : ILogger<LauncherWorker>) =
    inherit BackgroundService()

    let isRunning() =
        Process.GetProcessesByName("PowerLab").Length > 0

    let getInstallPath() = 
        let keyPath = @"Software\PowerLab"
        use key = Registry.LocalMachine.OpenSubKey(keyPath)
        if key = null then None
        else
            match key.GetValue("InstallPath") with
            | null -> None
            | value -> Some (value.ToString())

    let launchPowerLab() =
        logger.LogInformation("User logged in/unlocked session, launching PowerLab")
        getInstallPath() 
        |> Option.bind (fun path -> Some (path |> UserSessionProcessLauncher.launch))
        |> (fun launchResult -> launchResult.Value |> sprintf "launch result %A" |> logger.LogInformation)

    override _.ExecuteAsync(ct : CancellationToken) =
        task {
            //Debugger.Launch() |> ignore
            
            logger.LogInformation("PowerLabLauncherService is started.")
            launchPowerLab()
            //SystemEvents.UserPreferenceChanged.Add(fun args ->
            //    args.Category |> sprintf "UserPreferenceChanged %A" |> logger.LogInformation)

            // //订阅会话变化事件
            //SystemEvents.SessionSwitch.Add(fun args ->
            //    args.Reason |> sprintf "SessionSwitch %A" |> logger.LogInformation
            //    match args.Reason with
            //    | SessionSwitchReason.SessionLogon -> launchPowerLab()
            //    | _ -> ()
            //)
        }
    override _.StopAsync (cancellationToken: CancellationToken): Tasks.Task = 
        logger.LogInformation "PowerLabLauncherService is stopped."
        base.StopAsync(cancellationToken: CancellationToken)
        