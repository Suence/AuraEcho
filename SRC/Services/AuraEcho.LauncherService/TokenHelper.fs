namespace AuraEcho.LauncherService

open System
open System.Runtime.InteropServices
open AuraEcho.LauncherService.NativeMethods

module TokenHelper = 
    let TokenLinkedToken = 19

    let getElevatedToken (userToken : IntPtr) =
        let mutable size = 0
        GetTokenInformation(userToken, TokenLinkedToken, IntPtr.Zero, 0, &size) |> ignore

        let buffer = Marshal.AllocHGlobal(size)
        try
            if not (GetTokenInformation(userToken, TokenLinkedToken, buffer, size, &size)) then
                IntPtr.Zero
            else
                Marshal.ReadIntPtr(buffer)
        finally
            Marshal.FreeHGlobal(buffer)
