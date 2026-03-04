using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AuraEcho.FishyTime.Utils;

public sealed class WinEventSafeHandle : SafeHandle
{
    public WinEventSafeHandle() : base(IntPtr.Zero, ownsHandle: true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        bool ok = Win32Helper.UnhookWinEvent(handle);
#if DEBUG
        Debug.WriteLine($"WinEventSafeHandle.ReleaseHandle: handle=0x{handle.ToInt64():X}, ok={ok}");
#endif
        return ok;
    }

    public bool IsClosedOrInvalid => IsClosed || IsInvalid;

    public override string ToString() => $"WinEventSafeHandle: 0x{handle.ToInt64():X}";
}
