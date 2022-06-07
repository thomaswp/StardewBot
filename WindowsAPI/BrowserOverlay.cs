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

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hwnd, out Rect rect);

        [DllImport("USER32.DLL")]
        private static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int width, int height, UINT uFlags);

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
            browserProcess = new Process();
            browserProcess.StartInfo.FileName = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            browserProcess.StartInfo.Arguments = "file:///C:/xampp/htdocs/farmbot-blockly/step-execution.html" + " --new-window";
            browserProcess.Start();

            //browserProcess = new Process();
            //browserProcess.StartInfo.FileName = @"file:///C:/xampp/htdocs/farmbot-blockly/step-execution.html";
            //browserProcess.StartInfo.UseShellExecute = true;
            //browserProcess.Start();

            SetTopmost();
            return true;
        }

        public void SetTopmost()
        {
            if (browserProcess == null) return;
            SetWindowPos(browserProcess.MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }

        public void RepositionBrowser(int x, int y, int width, int height)
        {
            if (browserProcess == null) return;
            SetWindowPos(browserProcess.MainWindowHandle, HWND_TOPMOST, x, y, width, height, SWP_NOACTIVATE);
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
