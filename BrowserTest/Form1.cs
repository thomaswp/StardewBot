using Browser.Common;
using BrowserCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrowserTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Thread.Sleep(1000);
            InitializeComponent();
        }

        private GraphicsReader reader;
        private IOBridge bridge;
        private int frames;
        private int width, height;

        private void Form1_Load(object sender, EventArgs e)
        {
            width = pictureBox1.Width;
            height = pictureBox1.Height;
            bridge = new IOBridge();
            string url = "https://www.google.com";
            //string url = "https://blockly-demo.appspot.com/static/demos/code/index.html";
            //string url = @"C:\xampp\htdocs\farmbot-blockly\step-execution.html";
            bridge.StartBrowser(width, height, url);
            reader =  new GraphicsReader(width, height);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (reader.HasNewBitmap)
            {
                byte[] bytes = reader.ReadBitmap();
                GCHandle pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                Bitmap bmp = new Bitmap(width, height, width * 4, PixelFormat.Format32bppRgb, pointer);
                pictureBox1.Image = bmp;
                frames++;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Text = frames + " FPS";
            frames = 0;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            reader.Dispose();
            bridge.Dispose();
        }

        static int FromMButton(MouseButtons button)
        {
            if (button == MouseButtons.Right) return (int)MouseButton.Right;
            if (button == MouseButtons.Middle) return (int)MouseButton.Middle;
            return (int) MouseButton.Left;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            bridge.MouseDown(e.X, e.Y, FromMButton(e.Button));
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            bridge.MouseMove(e.X, e.Y);
        }

        // TODO: Add Enum
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            bridge.KeyEvent(1, (int)e.KeyCode);
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            bridge.KeyEvent(3, e.KeyChar);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            bridge.KeyEvent(2, (int)e.KeyCode);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            bridge.MouseUp(e.X, e.Y, FromMButton(e.Button));
        }
    }
}
