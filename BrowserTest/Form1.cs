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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrowserTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private GraphicsReader reader;
        private IOBridge bridge;

        private void Form1_Load(object sender, EventArgs e)
        {
            reader =  new GraphicsReader();
            bridge = new IOBridge();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (reader.HasNewBitmap)
            {
                byte[] bytes = reader.ReadBitmap();
                GCHandle pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                Bitmap bmp = new Bitmap(BrowserSettings.MAX_WIDTH, BrowserSettings.MAX_HEIGHT, 
                    BrowserSettings.MAX_WIDTH * 4, PixelFormat.Format32bppRgb, pointer);
                pictureBox1.Image = bmp;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            reader.Dispose();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            bridge.MouseDown(e.X, e.Y);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            bridge.MouseMove(e.X, e.Y);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            bridge.MouseUp(e.X, e.Y);
        }
    }
}
