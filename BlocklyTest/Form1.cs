using BlocklyBridge;
using WindowsAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlocklyTest
{
    public partial class Form1 : Form, IProgrammable, ILogger
    {
        public static Dispatcher Dispatcher
        {
            get;
            private set;
        }

        private BrowserOverlay browserOverlay;

        public TextBox Output => textBox1;

        private MethodQueue queue = new MethodQueue();
        private Functions functions;

        public Form1()
        {
            InitializeComponent();
            timer1.Enabled = true;
            functions = new Functions(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Dispatcher = new Dispatcher("127.0.0.1", 8000);

            Assembly assembly = Assembly.GetExecutingAssembly();
            Dispatcher.Start(assembly.GetTypes(), () =>
            {
                Logger.Log("Connected!!!");
                try
                {
                    Dispatcher.State = ProgramState.FromJSON(Properties.Settings.Default.Code);
                }
                catch 
                {
                    Logger.Warn("Failed to load saved code: " + Properties.Settings.Default.Code);
                }
                Dispatcher.SetTarget(this);
                BlocklyGenerator.SendEvent(this, "OnStart");
            });

            Dispatcher.Register(this);
            Dispatcher.OnSave += Dispatcher_OnSave;

            Logger.Implementation = this;

            browserOverlay = new BrowserOverlay();
            browserOverlay.Initialize();
            UpdatePosition();
        }

        private void Dispatcher_OnSave(ProgramState obj)
        {
            Properties.Settings.Default.Code = obj.ToJSON();
            Logger.Log(Properties.Settings.Default.Code);
            Properties.Settings.Default.Save();
        }

        public string GetGuid()
        {
            return "1234";
        }

        public string GetName()
        {
            return "Form";
        }

        public object GetObjectForType(Type declaringType)
        {
            return functions;
        }

        public void EnqueueMethod(AsyncMethod method)
        {
            queue.Enqueue(method);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            queue.Update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BlocklyGenerator.SendEvent(this, "OnButtonClicked");
        }

        public void Log(object message)
        {
            Debug.WriteLine(message);
        }

        public void Warn(object message)
        {
            Debug.WriteLine(message);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispatcher.Dispose();
            browserOverlay.Dispose();
        }

        public bool TryTestCode()
        {
            BlocklyGenerator.SendEvent(this, "OnTest");
            return true;
        }

        private void Form1_Layout(object sender, LayoutEventArgs e)
        {
            UpdatePosition();
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (browserOverlay == null) return;
            //Rectangle dBounds = DesktopBounds;
            Rectangle bounds = panel1.Bounds;
            Point topLeft = panel1.PointToScreen(Point.Empty);
            bounds.X = topLeft.X;
            bounds.Y = topLeft.Y;
            browserOverlay.RepositionBrowser(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
        }
    }
}
