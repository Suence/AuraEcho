using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PowerLab.FishyTime.Utils
{
    public sealed class MouseHookSafeHandle : SafeHandle
    {
        public MouseHookSafeHandle() : base(IntPtr.Zero, ownsHandle: true) { }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            bool ok = Win32Helper.UnhookWindowsHookEx(handle);
#if DEBUG
            Debug.WriteLine($"SafeMouseHookHandle.ReleaseHandle: handle=0x{handle.ToInt64():X}, ok={ok}");
#endif
            return ok;
        }

        public bool IsClosedOrInvalid => IsClosed || IsInvalid;

        public override string ToString() => $"SafeMouseHookHandle: 0x{handle.ToInt64():X}";
    }
}
