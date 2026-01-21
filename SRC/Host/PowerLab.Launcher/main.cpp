#include <windows.h>
#include <gdiplus.h>
#include <string>
#include <dwmapi.h>
#include <vector>
#include <tlhelp32.h>
#include <thread>
#include <filesystem>

#include "resource.h"
#pragma comment(lib, "gdiplus.lib")
#pragma comment(lib, "dwmapi.lib")

using namespace Gdiplus;
namespace fs = std::filesystem;

const wchar_t* PIPE_NAME = L"\\\\.\\pipe\\POWERLAB_LAUNCHER_SERVICE_PIPE";
const std::string APPNAME = "PowerLab.exe";

Image* LoadImageFromResource(HMODULE hMod, int resId, const wchar_t* resType) {
    HRSRC hRes = FindResource(hMod, MAKEINTRESOURCE(resId), resType);
    if (!hRes) return nullptr;

    DWORD resSize = SizeofResource(hMod, hRes);
    HGLOBAL hResData = LoadResource(hMod, hRes);
    if (!hResData) return nullptr;

    void* pRes = LockResource(hResData);

    // 将资源数据拷贝到流中
    IStream* pStream = nullptr;
    HGLOBAL hMem = GlobalAlloc(GMEM_MOVEABLE, resSize);
    if (hMem) {
        void* pLockedMem = GlobalLock(hMem);
        memcpy(pLockedMem, pRes, resSize);
        GlobalUnlock(hMem);
        CreateStreamOnHGlobal(hMem, TRUE, &pStream);
    }

    if (pStream) {
        Image* pImage = Image::FromStream(pStream);
        pStream->Release();
        return pImage;
    }
    return nullptr;
}

std::string GetAppInstallPath() {
    std::string result = "";
    HKEY hKey;
    const wchar_t* subKey = L"SOFTWARE\\PowerLab";
    const wchar_t* valueName = L"InstallPath";

    LSTATUS status = RegOpenKeyExW(HKEY_LOCAL_MACHINE, subKey, 0, KEY_READ, &hKey);

    if (status == ERROR_SUCCESS) {
        DWORD bufferSize = 0;

        // 获取数据缓冲区大小
        status = RegQueryValueExW(hKey, valueName, NULL, NULL, NULL, &bufferSize);

        if (status == ERROR_SUCCESS && bufferSize > 0) {
            std::vector<wchar_t> buffer(bufferSize / sizeof(wchar_t));

            // 获取实际数据
            status = RegQueryValueExW(hKey, valueName, NULL, NULL, (LPBYTE)buffer.data(), &bufferSize);

            if (status == ERROR_SUCCESS) {
                std::wstring wPath(buffer.data());

                // 将 wstring 转换为 string (假设路径是 ANSI/UTF-8 编码)
                int size_needed = WideCharToMultiByte(CP_UTF8, 0, wPath.c_str(), (int)wPath.length(), NULL, 0, NULL, NULL);
                result.resize(size_needed);
                WideCharToMultiByte(CP_UTF8, 0, wPath.c_str(), (int)wPath.length(), &result[0], size_needed, NULL, NULL);

                fs::path originalPath = result;
                fs::path newPath = originalPath;
                newPath.replace_filename(APPNAME);
                result = newPath.string();
            }
        }
        RegCloseKey(hKey);
    }

    return result;
}

bool SendPipeMessage(const std::string& message) {
    HANDLE hPipe = CreateFile(
        PIPE_NAME,
        GENERIC_WRITE,
        0,
        NULL,
        OPEN_EXISTING,
        0,
        NULL
    );

    if (hPipe != INVALID_HANDLE_VALUE) {
        DWORD bytesWritten;
        WriteFile(hPipe, message.c_str(), (DWORD)message.length(), &bytesWritten, NULL);
        CloseHandle(hPipe);
        return true;
    }
    else {
        OutputDebugString(L"无法连接到命名管道服务端\n");
        return false;
    }
}
void EnableWin11RoundedCorner(HWND hwnd)
{
    DWORD preference = DWMWCP_ROUND;
    DwmSetWindowAttribute(
        hwnd,
        DWMWA_WINDOW_CORNER_PREFERENCE,
        &preference,
        sizeof(preference)
    );
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    static Image* pImage = nullptr;
    switch (uMsg) {
    case WM_CREATE: {
        pImage = LoadImageFromResource(GetModuleHandle(nullptr), IDB_PNG1, L"PNG");
        EnableWin11RoundedCorner(hwnd);
        return 0;
    }
    
    case WM_NCHITTEST: {
        return HTCAPTION;
    }

    case WM_PAINT: {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hwnd, &ps);
        Graphics graphics(hdc);

        if (pImage && pImage->GetLastStatus() == Ok) {
            RECT rc;
            GetClientRect(hwnd, &rc);
            graphics.DrawImage(pImage, 0, 0, rc.right, rc.bottom);
        }
        else {
            SolidBrush brush(Color(255, 200, 200, 200));
            graphics.FillRectangle(&brush, 0, 0, 512, 315);
        }

        EndPaint(hwnd, &ps);
        return 0;
    }
    case WM_DESTROY:
        delete pImage;
        PostQuitMessage(0);
        return 0;
    }
    return DefWindowProc(hwnd, uMsg, wParam, lParam);
}

void CenterWindow(HWND hwnd) {
    RECT rc;
    GetWindowRect(hwnd, &rc);
    int xPos = (GetSystemMetrics(SM_CXSCREEN) - (rc.right - rc.left)) / 2;
    int yPos = (GetSystemMetrics(SM_CYSCREEN) - (rc.bottom - rc.top)) / 2;
    SetWindowPos(hwnd, 0, xPos, yPos, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
}

bool IsProcessRunning(const std::wstring& processName) {
    bool exists = false;
    // 创建系统进程快照
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot != INVALID_HANDLE_VALUE) {
        PROCESSENTRY32W pe;
        pe.dwSize = sizeof(PROCESSENTRY32W);
        // 遍历进程列表
        if (Process32FirstW(hSnapshot, &pe)) {
            do {
                std::wstring currentProc(pe.szExeFile);
                OutputDebugStringW((L"Scanning: " + currentProc + L"\n").c_str());
                if (processName == pe.szExeFile) {
                    
                    exists = true;
                    break;
                }
            } while (Process32NextW(hSnapshot, &pe));
        }
        CloseHandle(hSnapshot);
    }
    return exists;
}
void StartApp(HWND hwndTarget) {
    const wchar_t* TARGET_WINDOW_TITLE = L"PowerLab";

    std::string message = GetAppInstallPath();
    bool sendResult = SendPipeMessage(message);

    if (!sendResult) return;

    int attempts = 0;
    while (attempts < 100) {
        HWND targetHwnd = FindWindow(NULL, TARGET_WINDOW_TITLE);

        if (targetHwnd != NULL && IsWindowVisible(targetHwnd)) {
            OutputDebugString(L"Target window found! Closing launcher...\n");

            PostMessage(hwndTarget, WM_CLOSE, 0, 0);
            return;
        }

        Sleep(200);
        attempts++;
    }

    PostMessage(hwndTarget, WM_CLOSE, 0, 0);
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR pCmdLine, int nCmdShow) {
    std::string cmdLine(pCmdLine);
    if (cmdLine.find("-hide") != std::string::npos) {
        std::string message = GetAppInstallPath() += " -hide";
        SendPipeMessage(message);
        return 0;
    }

    GdiplusStartupInput gdiplusStartupInput;
    ULONG_PTR gdiplusToken;
    GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);

    const wchar_t CLASS_NAME[] = L"PowerLabLauncherMainWindowClass";
    WNDCLASS wc = { };
    wc.lpfnWndProc = WindowProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = CLASS_NAME;
    wc.hCursor = LoadCursor(NULL, IDC_ARROW); 
    wc.hbrBackground = (HBRUSH)(COLOR_BTNFACE + 1);

    RegisterClass(&wc);

    HWND hwnd = CreateWindowEx(
        WS_EX_TOOLWINDOW,
        CLASS_NAME,
        L"PowerLabLauncher",
        WS_POPUP,
        CW_USEDEFAULT, CW_USEDEFAULT, 512, 315,
        NULL, NULL, hInstance, NULL
    );

    CenterWindow(hwnd);
    ShowWindow(hwnd, nCmdShow);

    std::thread(StartApp, hwnd).detach();

    MSG msg = { };
    while (GetMessage(&msg, NULL, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    GdiplusShutdown(gdiplusToken);
    return 0;
}