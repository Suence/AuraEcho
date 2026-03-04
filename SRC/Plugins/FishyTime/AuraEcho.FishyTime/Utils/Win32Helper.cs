using AuraEcho.FishyTime.Models;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Drawing = System.Drawing;

namespace AuraEcho.FishyTime.Utils;

public static class Win32Helper
{
    public const int SWP_NOZORDER = 0x0004;
    public const int SWP_NOACTIVATE = 0x0010;
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_LAYERED = 0x80000;
    public const int LWA_ALPHA = 0x2;
    public const int SW_SHOW = 5;
    public const int SW_HIDE = 0;
    public const int SW_SHOWNOACTIVATE = 4;
    private const uint GA_ROOT = 2;
    public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;
    public const uint WINEVENT_OUTOFCONTEXT = 0;
    public const uint EVENT_OBJECT_DESTROY = 0x8001;

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);
    private const int WS_EX_TOPMOST = 0x00000008;

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;
    public const int GWLP_HWNDPARENT = -8;

    public const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
    public const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;
    public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

    private const int WM_GETICON = 0x007F;
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;
    private const int ICON_SMALL2 = 2;
    private const int GCL_HICON = -14;

    public const int WM_MOUSELEAVE = 0x02A3;
    private const int TME_LEAVE = 0x00000002;
    public const int WH_MOUSE_LL = 14;
    public const int WM_MOUSEMOVE = 0x0200;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    public static Task<bool> SetWindowPosAsync(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags)
        => Task.Run(() => SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags));

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool GetLayeredWindowAttributes(IntPtr hwnd, out uint pcrKey, out byte pbAlpha, out uint pdwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    public static Task<bool> SetLayeredWindowAttributesAsync(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags)
        => Task.Run(() => SetLayeredWindowAttributes(hwnd, crKey, bAlpha, dwFlags));

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern IntPtr WindowFromPoint(POINT Point);

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern WinEventSafeHandle SetWinEventHook(uint eventMin, uint eventMax, IntPtr
        hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
        uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern MouseHookSafeHandle SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
    
    [DllImport("user32.dll")]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    [DllImport("user32.dll")]
    private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    [DllImport("user32.dll", EntryPoint = "GetClassLongPtr", SetLastError = true)]
    private static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetClassLong", SetLastError = true)]
    private static extern uint GetClassLong32(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll")]
    private static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

    private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8)
            return GetClassLongPtr64(hWnd, nIndex);
        else
            return new IntPtr((long)GetClassLong32(hWnd, nIndex));
    }

    public static Drawing::Icon GetWindowIcon(IntPtr hwnd)
    {
        // 优先尝试获取运行时设置的图标
        IntPtr hIcon = SendMessage(hwnd, WM_GETICON, ICON_BIG, 0);

        // 如果没有，尝试类图标
        if (hIcon == IntPtr.Zero)
            hIcon = GetClassLongPtr(hwnd, GCL_HICON);

        if (hIcon != IntPtr.Zero)
            return Drawing::Icon.FromHandle(hIcon);

        return Drawing::SystemIcons.Application;
    }
    public static Task<Drawing::Icon> GetWindowIconAsync(IntPtr hwnd)
        => Task.Run(() => GetWindowIcon(hwnd));

    public static void SetWindowTopmost(IntPtr hwnd, bool topmost)
    {
        SetWindowPos(hwnd,
            topmost ? HWND_TOPMOST : HWND_NOTOPMOST,
            0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }
    public static Task SetWindowTopmostAsync(IntPtr hwnd, bool topmost)
        => Task.Run(() => SetWindowTopmost(hwnd, topmost));

    public static void SetWindowTopmoastWithoutShow(nint hwnd, bool topmost)
    {
        SetWindowPos(hwnd,
            topmost ? HWND_TOPMOST : HWND_NOTOPMOST,
            0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE);
    }
    public static Task SetWindowTopmoastWithoutShowAsync(nint hwnd, bool topmost)
        => Task.Run(() => SetWindowTopmoastWithoutShow(hwnd, topmost));

    public static bool IsWindowTopmost(IntPtr hwnd)
    {
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        return (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;
    }
    public static Task<bool> IsWindowTopmostAsync(IntPtr hwnd)
        => Task.Run(() => IsWindowTopmost(hwnd));


    public static Rect GetWindowRect(IntPtr hwnd)
    {
        bool isSucceded = GetWindowRect(hwnd, out RECT winRect);
        if (!isSucceded) return Rect.Empty;

        return new Rect(
           new Point(winRect.Left, winRect.Top),
           new Size(winRect.Right - winRect.Left, winRect.Bottom - winRect.Top));
    }
    public static Task<Rect> GetWindowRectAsync(IntPtr hwnd)
        => Task.Run(() => GetWindowRect(hwnd));

    public static IntPtr GetWindowUnderMouse()
    {
        if (GetCursorPos(out POINT point))
        {
            return WindowFromPoint(point);
        }
        return IntPtr.Zero;
    }

    public static double GetWindowOpacity(IntPtr hwnd)
    {
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        if ((exStyle & WS_EX_LAYERED) == 0) return 1;

        if (!GetLayeredWindowAttributes(hwnd, out _, out byte alpha, out uint flags)) return 1;

        if ((flags & LWA_ALPHA) == 0) return 1;

        return alpha / 255.0;
    }
    public static Task<double> GetWindowOpacityAsync(IntPtr hwnd)
        => Task.Run(() => GetWindowOpacity(hwnd));

    public static IntPtr GetTopLevelWindowUnderMouse()
    {
        var hwnd = GetWindowUnderMouse();
        if (hwnd != IntPtr.Zero)
        {
            return GetAncestor(hwnd, GA_ROOT);
        }
        return IntPtr.Zero;
    }

    public static string GetWindowTitle(IntPtr hwnd)
    {
        var buffer = new StringBuilder(256);
        if (GetWindowText(hwnd, buffer, buffer.Capacity) > 0)
        {
            return buffer.ToString();
        }
        return string.Empty;
    }
    public static Task<string> GetWindowTitleAsync(IntPtr hwnd)
        => Task.Run(() => GetWindowTitle(hwnd));

    public static string GetWindowClassName(IntPtr hwnd)
    {
        var buffer = new StringBuilder(256);
        if (GetClassName(hwnd, buffer, buffer.Capacity) > 0)
        {
            return buffer.ToString();
        }
        return string.Empty;
    }

    public static bool TrySetWindowOpacity(IntPtr hwnd, double opacity)
    {
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        if ((exStyle & WS_EX_LAYERED) == 0)
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
        }

        bool result = SetLayeredWindowAttributes(hwnd, 0, (byte)(opacity * 255), LWA_ALPHA);
        return result;
    }
    public static Task<bool> TrySetWindowOpacityAsync(IntPtr hwnd, double opacity)
        => Task.Run(() => TrySetWindowOpacity(hwnd, opacity));

    public static bool IsFullScreen(IntPtr hwnd)
    {
        if (!GetWindowRect(hwnd, out RECT rect))
            return false;

        var windowRect = new System.Drawing.Rectangle(
            rect.Left, rect.Top,
            rect.Right - rect.Left,
            rect.Bottom - rect.Top);

        // 获取窗口所在屏幕
        var screen = Screen.FromHandle(hwnd);
        var screenBounds = screen.WorkingArea;

        return windowRect.Left <= screenBounds.Left &&
               windowRect.Top <= screenBounds.Top &&
               windowRect.Right >= screenBounds.Right &&
               windowRect.Bottom >= screenBounds.Bottom;
    }

    public static ShowWindowCommands GetWindowState(IntPtr hwnd)
    {
        if (!IsWindowVisible(hwnd))
            return ShowWindowCommands.Hide;

        WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
        placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
        GetWindowPlacement(hwnd, ref placement);

        return placement.showCmd;
    }
    public static Task<ShowWindowCommands> GetWindowStateAsync(IntPtr hwnd)
        => Task.Run(() => GetWindowState(hwnd));

    public static void TrackMouseLeave(IntPtr hwnd)
    {
        TRACKMOUSEEVENT tme = new TRACKMOUSEEVENT();
        tme.cbSize = Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
        tme.dwFlags = TME_LEAVE;
        tme.hwndTrack = hwnd;
        TrackMouseEvent(ref tme);
    }

    public static void HideWindow(IntPtr hwnd)
    {
        ShowWindow(hwnd, SW_HIDE);
    }

    public static void ShowWindow(IntPtr hwnd)
    {
        ShowWindow(hwnd, SW_SHOW);
    }

    public static void ShowWindowNoActivate(IntPtr hwnd)
    {
        ShowWindow(hwnd, SW_SHOWNOACTIVATE);
    }

    public static Screen GetWindowScreen(IntPtr hwnd)
    {
        if (!GetWindowRect(hwnd, out RECT rect))
            return null;

        var windowRectangle = new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        return Screen.AllScreens.FirstOrDefault(s => s.Bounds.IntersectsWith(windowRectangle));
    }
    public static Task<Screen> GetWindowScreenAsync(IntPtr hwnd)
        => Task.Run(() => GetWindowScreen(hwnd));


    public static void SetWindowOwner(nint childrenHandle, nint parentHandle)
    {
        SetWindowLongPtr(childrenHandle, GWLP_HWNDPARENT, parentHandle);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

[StructLayout(LayoutKind.Sequential)]
public struct WINDOWPLACEMENT
{
    public int length;
    public int flags;
    public ShowWindowCommands showCmd;
    public Drawing::Point ptMinPosition;
    public Drawing::Point ptMaxPosition;
    public Drawing::Rectangle rcNormalPosition;
}

[StructLayout(LayoutKind.Sequential)]
public struct TRACKMOUSEEVENT
{
    public int cbSize;
    public int dwFlags;
    public IntPtr hwndTrack;
    public int dwHoverTime;
}

[StructLayout(LayoutKind.Sequential)]
public struct MSLLHOOKSTRUCT
{
    public POINT pt;
    public int mouseData, flags, time;
    public IntPtr dwExtraInfo;
}

public delegate void WinEventDelegate(
    IntPtr hWinEventHook,
    uint eventType,
    IntPtr hwnd,
    int idObject,
    int idChild,
    uint dwEventThread,
    uint dwmsEventTime);

public delegate nint LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
