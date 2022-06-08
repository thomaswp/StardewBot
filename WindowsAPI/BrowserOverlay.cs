using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HWND = System.IntPtr;
using UINT = System.UInt32;

namespace WindowsAPI
{

    public class BrowserOverlay : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetWindowRect(IntPtr hwnd, out Rect rect);

        [DllImport("USER32.DLL")]
        private static extern IntPtr ShowWindow(IntPtr hwnd, int flags);

        [DllImport("USER32.DLL")]
        private static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int width, int height, UINT uFlags);


        [DllImport("USER32.DLL")]
        private static extern IntPtr SetWindowLongA(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("USER32.DLL")]
        private static extern HWND SetParent(HWND hWndChild, HWND hWndNewParent);

        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //Sets window attributes
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //Gets window attributes
        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        static extern IntPtr GetMenu(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll")]
        static extern bool DrawMenuBar(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        //assorted constants needed
        public static uint MF_BYPOSITION = 0x400;
        public static uint MF_REMOVE = 0x1000;
        public static int GWL_STYLE = -16;
        public static int WS_CHILD = 0x40000000; //child window
        public static int WS_BORDER = 0x00800000; //window with border
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar 
        public static int WS_SYSMENU = 0x00080000; //window menu  
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_SIZEBOX = 0x00040000;


        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOACTIVATE = 0x0010;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private Process browserProcess;


        public bool Initialize()
        {
            // TODO: Only works if there are no existing chrome processes...
            // TODO: Get default or available browser; use specific file location

            string[] browserPaths =
            {
                Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Google\Chrome\Application\chrome.exe",
                Environment.GetEnvironmentVariable("ProgramFiles") + @"\Google\Chrome\Application\chrome.exe",
            };

            foreach (string path in browserPaths)
            {
                try
                {
                    var browserProcess = new Process();
                    browserProcess.StartInfo.FileName = path;
                    browserProcess.StartInfo.Arguments = "file:///C:/xampp/htdocs/farmbot-blockly/step-execution.html" + " --new-window";
                    browserProcess.Start();
                    this.browserProcess = browserProcess;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            if (browserProcess == null)
            {
                Debug.WriteLine("Could not find browser");
                return false;
            }


            // TODO: Actually wait until the process has a window handle
            Thread.Sleep(1000);

            //var owner = parentHandle;
            var owner = Process.GetCurrentProcess().MainWindowHandle;
            var owned = browserProcess.MainWindowHandle;
            var result = SetParent(owned, owner);
            //var result2 = SetWindowLongA(owned, -16, 0x40000000L);
            Debug.WriteLine($"{owner} owns {owned}: {result}");

            IntPtr pFoundWindow = owned;
            int style = GetWindowLong(pFoundWindow, GWL_STYLE);

            //get menu
            //IntPtr HMENU = GetMenu(pFoundWindow);
            ////get item count
            //int count = GetMenuItemCount(HMENU);
            ////loop & remove
            //for (int i = 0; i < count; i++)
            //    RemoveMenu(HMENU, 0, (MF_BYPOSITION | MF_REMOVE));

            //force a redraw
            //DrawMenuBar(proc.MainWindowHandle);
            //SetWindowLong(pFoundWindow, GWL_STYLE, (style & ~WS_SYSMENU));
            //SetWindowLong(pFoundWindow, GWL_STYLE, (style & ~WS_CAPTION));
            //SetWindowLong(pFoundWindow, GWL_STYLE, (style & ~WS_MAXIMIZEBOX));
            //SetWindowLong(pFoundWindow, GWL_STYLE, (style & ~WS_MINIMIZEBOX));
            //SetWindowLong(pFoundWindow, GWL_STYLE, (style & ~WS_SIZEBOX));
            //SetWindowLong(pFoundWindow, GWL_STYLE, (style | WS_CHILD));

            //browserProcess = new Process();
            //browserProcess.StartInfo.FileName = @"file:///C:/xampp/htdocs/farmbot-blockly/step-execution.html";
            //browserProcess.StartInfo.UseShellExecute = true;
            //browserProcess.Start();


            SetTopmost(true);
            return true;
        }

        public enum WindowState
        {
            Normal, Minimized, Maximized,
        }

        public static WindowState GetWindowState(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return WindowState.Normal;
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            GetWindowPlacement(windowHandle, ref placement);
            switch (placement.showCmd)
            {
                case 1:
                    return WindowState.Normal;
                case 2:
                    return WindowState.Minimized;
                case 3:
                    return WindowState.Maximized;
            }
            return WindowState.Normal;
        }

        public void SetTopmost(bool isTopmost)
        {
            //if (browserProcess == null) return;
            //IntPtr handle = browserProcess.MainWindowHandle;
            //if (isTopmost)
            //{
            //    EnsureNormalWindowState();
            //    SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            //}
            //else
            //{
            //    SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, TOPMOST_FLAGS);
            //}
        }

        private void EnsureNormalWindowState()
        {
            if (browserProcess == null) return;
            IntPtr handle = browserProcess.MainWindowHandle;
            if (GetWindowState(handle) == WindowState.Normal) return;
            ShowWindow(handle, 1); // Ask for normal
        }

        public void RepositionBrowser(int x, int y, int width, int height)
        {
            if (browserProcess == null) return;
            //var owner = Process.GetCurrentProcess().MainWindowHandle;
            //Debug.WriteLine(owner);
            EnsureNormalWindowState();
            SetWindowPos(browserProcess.MainWindowHandle, IntPtr.Zero, x, y, width, height, SWP_NOACTIVATE);
            // SetWindowPos(browserProcess.MainWindowHandle, HWND_TOPMOST, x, y, width, height, SWP_NOACTIVATE);
        }

        public void OnParentWindowFocused()
        {
            SetTopmost(true);
        }

        public void OnParentWindowBlurred()
        {
            SetTopmost(false);
        }

        public static HWND? GetChrome()
        {
            foreach (var process in Process.GetProcesses())
            {
                bool isVisible = IsWindowVisible(process.MainWindowHandle);
                //Debug.WriteLine($"{process.ProcessName}: {isVisible}");
                if (process.ProcessName == "chrome" && isVisible)
                {
                    //Bitmap bmp = PrintWindow(process.MainWindowHandle);
                    //bmps.Add(bmp);
                    Debug.WriteLine("found chrome!");
                    return process.MainWindowHandle;
                }
            }
            return null;
        }

        public void Dispose()
        {
            if (browserProcess == null) return;
            browserProcess.CloseMainWindow();
        }
    }
}
