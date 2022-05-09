
using Browser.Common;
using CefSharp;
using CefSharp.Core;
using CefSharp.OffScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Browser.Host
{
    public class BrowserHost : IBrowserUI
    {

        private ChromiumWebBrowser browser;
        private readonly Interchange interchange;
        private IOBridge bridge;

        public bool IsDisposed { get; internal set; }

        public BrowserHost(Interchange interchange)
        {
            this.interchange = interchange;
            //CefSettings settings = new CefSettings();
            //settings.CefCommandLineArgs.Add("--off-screen-frame-rate", "60");
            //Cef.Initialize(settings);
            var settings = new CefSharp.Core.BrowserSettings();
            settings.WindowlessFrameRate = 60;
            browser = new ChromiumWebBrowser("", settings);
            //browser = new ChromiumWebBrowser();
            browser.BrowserInitialized += Browser_BrowserInitialized;
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.Paint += Browser_Paint;
            
            bridge = new IOBridge(this);
        }


        public void StartBrowser(int width, int height, string url)
        {
            browser.Size = new System.Drawing.Size(width, height);
            browser.Load(url);
        }

        public void Shutdown()
        {
            interchange.Dispose();
            bridge.Dispose();
            browser.Dispose();
            IsDisposed = true;
        }

        private void Browser_BrowserInitialized(object sender, EventArgs e)
        {
            Console.WriteLine("Browser Initialized");
            //string url = "https://www.google.com";
            //string url = "https://blockly-demo.appspot.com/static/demos/code/index.html";
            //string url = @"C:\xampp\htdocs\farmbot-blockly\step-execution.html";
        }

        private void Browser_Paint(object sender, OnPaintEventArgs e)
        {
            int size = e.Height * e.Width * 4;
            byte[] managedArray = new byte[size];
            Marshal.Copy(e.BufferHandle, managedArray, 0, size);
            interchange.Write(managedArray);

        }

        private void Browser_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            Console.WriteLine("Loaded: " + e.IsLoading);
        }

        public void MouseDown(int x, int y)
        {
            browser.GetBrowser().GetHost().SendMouseClickEvent(new MouseEvent(x, y, CefEventFlags.None), MouseButtonType.Left, false, 1);
        }

        public void MouseUp(int x, int y)
        {
            browser.GetBrowser().GetHost().SendMouseClickEvent(new MouseEvent(x, y, CefEventFlags.None), MouseButtonType.Left, true, 1);
        }

        public void MouseMove(int x, int y)
        {
            browser.GetBrowser().GetHost().SendMouseMoveEvent(new MouseEvent(x, y, CefEventFlags.None), false);
        }

        //        //private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        //        //{
        //        //    KeyEvent k = new KeyEvent();
        //        //    k.WindowsKeyCode = e.KeyChar;
        //        //    k.Type = KeyEventType.Char;
        //        //    k.IsSystemKey = false;
        //        //    k.FocusOnEditableField = true;
        //        //    browser.GetBrowser().GetHost().SendKeyEvent(k);
        //        //}
    }
}
