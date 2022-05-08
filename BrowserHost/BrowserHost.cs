
using Browser.Common;
using CefSharp;
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

        public BrowserHost(Interchange interchange)
        {
            this.interchange = interchange;
            //CefSettings settings = new CefSettings();
            browser = new ChromiumWebBrowser();
            browser.BrowserInitialized += Browser_BrowserInitialized;
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.Paint += Browser_Paint;
            
            bridge = new IOBridge(this);
        }

        private void Browser_BrowserInitialized(object sender, EventArgs e)
        {
            Console.WriteLine("Browser Initialized");
            //string url = "https://www.google.com";
            //string url = "https://blockly-demo.appspot.com/static/demos/code/index.html";
            string url = @"C:\xampp\htdocs\farmbot-blockly\step-execution.html";
            browser.Size = new System.Drawing.Size(Common.BrowserSettings.MAX_WIDTH, Common.BrowserSettings.MAX_HEIGHT);
            browser.Load(url);
        }

        private void Browser_Paint(object sender, OnPaintEventArgs e)
        {
            //Console.WriteLine("Paint!!");
            int size = e.Height * e.Width * 4;
            // Console.WriteLine("Painted");
            //Bitmap bmp = new Bitmap(e.Width, e.Height, e.Width * 4, PixelFormat.Format32bppRgb, e.BufferHandle);
            //lock (this)
            //{
            //    lastImage = image;
            //    image = new Bitmap(bmp);

            //    if (lastImage != null) lastImage.Dispose();
            //    //pictureBox1.Image = image;
            //}
            //Console.WriteLine("Paint!!");

            byte[] managedArray = new byte[size];
            Marshal.Copy(e.BufferHandle, managedArray, 0, size);
            interchange.Write(managedArray);

            //using (var ms = new MemoryStream(managedArray))
            //{

            //}
            //Bitmap bmp = new Bitmap(e.Width, e.Height);
            //var data = bmp.LockBits(new Rectangle(0, 0, e.Width, e.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            //data.

        }

        private void Browser_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            Console.WriteLine("Loaded: " + e.IsLoading);
            //var b = browser.GetBrowser();
            //b.MainFrame.GetSourceAsync().ContinueWith(t => Console.WriteLine("Loading: " + t.Result));
        }

        public void MouseDown(int x, int y)
        {
            //Console.WriteLine($"Sending down {x} {y}");
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

        //        //private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        //        //{
        //        //    browser.GetBrowser().GetHost().SendMouseClickEvent(new MouseEvent(e.X, e.Y, CefEventFlags.None), MouseButtonType.Left, false, 1);
        //        //}

        //        //private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        //        //{
        //        //    browser.GetBrowser().GetHost().SendMouseClickEvent(new MouseEvent(e.X, e.Y, CefEventFlags.None), MouseButtonType.Left, true, 1);
        //        //}

        //        //private void pictureBox1_MouseHover(object sender, EventArgs e)
        //        //{
        //        //}

        //        //private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        //        //{

        //        //    browser.GetBrowser().GetHost().SendMouseMoveEvent(new MouseEvent(e.X, e.Y, CefEventFlags.None), false);
        //        //}

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
