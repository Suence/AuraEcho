namespace AuraEcho.LauncherService

open System
open System.Runtime.InteropServices

module NativeMethods = 

    // 没有任何用户登录时返回的 Session 值
    [<Literal>] 
    let INVALID_SESSION_ID = 0xFFFFFFFF
    
    // Windows 允许的最大 token 权限
    [<Literal>] 
    let MAXIMUM_ALLOWED = 0x02000000u
    
    // Token 类型定义
    [<Literal>] 
    let SECURITY_IDENTIFICATION = 1
    [<Literal>] 
    let TOKEN_PRIMARYTOKEN_PRIMARY = 1
    
    // CreateProcess flags
    [<Literal>] 
    let CREATE_NEW_CONSOLE = 0x00000010u
    [<Literal>] 
    let CREATE_UNICODE_ENVIRONMENT = 0x00000400u
    
    // 用户真实桌面
    [<Literal>] 
    let INTERACTIVE_DESKTOP = @"winsta0\default"

    [<StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)>]
    type STARTUPINFO =
        struct
            val mutable cb : int
            val mutable lpReserved : string
            val mutable lpDesktop : string
            val mutable lpTitle : string
            val mutable dwX : int
            val mutable dwY : int
            val mutable dwXSize : int
            val mutable dwYSize : int
            val mutable dwXCountChars : int
            val mutable dwYCountChars : int
            val mutable dwFillAttribute : int
            val mutable dwFlags : int
            val mutable wShowWindow : int16
            val mutable cbReserved2 : int16
            val mutable lpReserved2 : IntPtr
            val mutable hStdInput : IntPtr
            val mutable hStdOutput : IntPtr
            val mutable hStdError : IntPtr
        end

    [<StructLayout(LayoutKind.Sequential)>]
    type PROCESS_INFORMATION =
        struct
            val mutable hProcess : IntPtr
            val mutable hThread : IntPtr
            val mutable dwProcessId : int
            val mutable dwThreadId : int
        end

    [<DllImport("kernel32.dll")>]
    extern uint32 WTSGetActiveConsoleSessionId()

    [<DllImport("wtsapi32.dll", SetLastError=true)>]
    extern bool WTSQueryUserToken(uint32 sessionId, IntPtr& token)

    [<DllImport("advapi32.dll", SetLastError=true)>]
    extern bool DuplicateTokenEx(
        IntPtr hExistingToken,
        uint32 desiredAccess,
        IntPtr tokenAttributes,
        int impersonationLevel,
        int tokenType,
        IntPtr& newToken)

    [<DllImport("advapi32.dll", SetLastError=true, CharSet=CharSet.Unicode)>]
    extern bool CreateProcessAsUser(
        IntPtr token,
        string app,
        string cmdLine,
        IntPtr pa,
        IntPtr ta,
        bool inheritHandles,
        uint32 flags,
        IntPtr env,
        string cwd,
        STARTUPINFO& si,
        PROCESS_INFORMATION& pi)

    [<DllImport("userenv.dll", SetLastError=true)>]
    extern bool CreateEnvironmentBlock(IntPtr& env, IntPtr token, bool inheritA)

    [<DllImport("userenv.dll", SetLastError=true)>]
    extern bool DestroyEnvironmentBlock(IntPtr env)

    [<DllImport("kernel32.dll", SetLastError=true)>]
    extern bool CloseHandle(IntPtr h)

    [<DllImport("advapi32.dll", SetLastError=true)>]
    extern bool GetTokenInformation(
        IntPtr token,
        int infoClass,
        IntPtr buffer,
        int length,
        int& returnLength)

    [<DllImport("Wtsapi32.dll")>]
    extern bool WTSRegisterSessionNotification(IntPtr hWnd, uint32 dwFlags)

    [<DllImport("Wtsapi32.dll")>]
    extern bool WTSUnRegisterSessionNotification(IntPtr hWnd)
