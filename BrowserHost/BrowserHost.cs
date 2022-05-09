
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
    public class BrowserHost : IBrowserUI, IContextMenuHandler
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
            browser.MenuHandler = this;
            
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

        private IBrowserHost Host { get { return browser.GetBrowser().GetHost(); } }

        static MouseButtonType ToMBType(int button)
        {
            switch ((MouseButton)button)
            {
                case MouseButton.Left: return MouseButtonType.Left;
                case MouseButton.Middle: return MouseButtonType.Middle;
                case MouseButton.Right: return MouseButtonType.Right;
            }
            return MouseButtonType.Left;
        }

        private CefEventFlags flags = CefEventFlags.None;

        public void MouseDown(int x, int y, int button)
        {
            switch ((MouseButton)button)
            {
                case MouseButton.Left: flags |= CefEventFlags.LeftMouseButton; break;
                case MouseButton.Middle: flags |= CefEventFlags.MiddleMouseButton; break;
                case MouseButton.Right: flags |= CefEventFlags.RightMouseButton; break;
            }
            Host.SendMouseClickEvent(new MouseEvent(x, y, flags), ToMBType(button), false, 1);
        }

        public void MouseUp(int x, int y, int button)
        {
            switch ((MouseButton)button)
            {
                case MouseButton.Left: flags &= ~CefEventFlags.LeftMouseButton; break;
                case MouseButton.Middle: flags &= ~CefEventFlags.MiddleMouseButton; break;
                case MouseButton.Right: flags &= ~CefEventFlags.RightMouseButton; break;
            }
            Host.SendMouseClickEvent(new MouseEvent(x, y, flags), ToMBType(button), true, 1);
        }

        public void MouseMove(int x, int y)
        {
            Host.SendMouseMoveEvent(new MouseEvent(x, y, flags), false);
        }

        public void KeyEvent(int type, int keyCode)
        {
            KeyEvent k = new KeyEvent();
            k.WindowsKeyCode = keyCode;
            k.Type = (KeyEventType)type;
            k.IsSystemKey = false;
            k.FocusOnEditableField = true;
            Host.SendKeyEvent(k);
        }

        #region ContextMenu Handlers
        public void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
        }

        public bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            return true;
        }

        public void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
        }

        public bool RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return true;
        }
        #endregion
    }
}
