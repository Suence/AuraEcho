namespace PowerLab.LauncherService


open System
open System.IO
open PowerLab.LauncherService.NativeMethods
open PowerLab.LauncherService.TokenHelper
open System.Runtime.InteropServices


module UserSessionProcessLauncher =
    let check name ok =
        if ok then Ok ()
        else Error name

    let launch exePath =
        let session = WTSGetActiveConsoleSessionId()
        if session = 0xFFFFFFFFu then false else

        let mutable userToken = IntPtr.Zero
        let mutable primary = IntPtr.Zero
        let mutable env = IntPtr.Zero

        let result =
            check "WTSQueryUserToken" (WTSQueryUserToken(session, &userToken))
            |> Result.bind (fun _ ->
                let elevated = getElevatedToken userToken
                let baseToken = if elevated = IntPtr.Zero then userToken else elevated
                check "DuplicateTokenEx" (DuplicateTokenEx(baseToken, MAXIMUM_ALLOWED, IntPtr.Zero, SECURITY_IDENTIFICATION, TOKEN_PRIMARYTOKEN_PRIMARY, &primary)))
            |> Result.bind (fun _ ->
                check "CreateEnvironmentBlock" (CreateEnvironmentBlock(&env, primary, false)))
            |> Result.bind (fun _ ->
                let mutable si = STARTUPINFO()
                si.cb <- Marshal.SizeOf<STARTUPINFO>()
                si.lpDesktop <- INTERACTIVE_DESKTOP

                let mutable pi = PROCESS_INFORMATION()

                check "CreateProcessAsUser" (
                    CreateProcessAsUser(
                        primary,
                        exePath,
                        null,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        false,
                        CREATE_NEW_CONSOLE ||| CREATE_UNICODE_ENVIRONMENT,
                        env,
                        Path.GetDirectoryName exePath,
                        &si,
                        &pi)))

        if env <> IntPtr.Zero then DestroyEnvironmentBlock(env) |> ignore
        if primary <> IntPtr.Zero then CloseHandle(primary) |> ignore
        if userToken <> IntPtr.Zero then CloseHandle(userToken) |> ignore

        result.IsOk



