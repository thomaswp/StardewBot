
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
    public class BrowserHost : IBrowserUI, IContextMenuHandler, ILifeSpanHandler
    {

        private ChromiumWebBrowser browser;
        private readonly Interchange interchange;
        private IOBridge bridge;

        public bool IsDisposed { get; internal set; }
        private IBrowserHost Host { get { return browser.GetBrowser().GetHost(); } }

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
            browser.LifeSpanHandler = this;
            browser.MenuHandler = this;
            // Better idea: just don't use JS dialogs
            //browser.JsDialogHandler = this;
            
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

        public void Refresh()
        {
            browser.GetBrowser().Reload();
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

        #region ContextMenu and Lifespan Handlers
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

        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = chromiumWebBrowser;
            return true;
        }

        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            
        }

        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            return true;
        }

        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            
        }

        //public bool OnJSDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
        //{
        //    throw new NotImplementedException();
        //}

        //public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
        //{
        //    throw new NotImplementedException();
        //}

        //public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
        //{
        //    throw new NotImplementedException();
        //}
        #endregion
    }
}
