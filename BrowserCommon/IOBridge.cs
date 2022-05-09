using SharedMemory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Browser.Common
{
    public class IOBridge : IDisposable, IBrowserUI
    {

        const string NAME = "IOBridge";

        private readonly RpcBuffer buffer;

        private readonly IBrowserUI ui;
        public bool IsHost { get { return ui != null; } }

        private static Dictionary<byte, string> IDNameMap = new();
        private static Dictionary<string, byte> NameIDMap = new();

        public IOBridge(IBrowserUI ui = null)
        {
            this.ui = ui;
            if (IsHost)
            {
                buffer = new RpcBuffer(NAME, (id, payload) =>
                {
                    HandleCall(id, payload);
                });
            }
            else
            {
                buffer = new RpcBuffer(NAME);
            }
        }

        private static byte actionIndex = 0;
        static IOBridge()
        {    
            foreach (var method in typeof (IBrowserUI).GetMethods())
            {
                //if (method.GetCustomAttributes(typeof(Callable), true).Length > 0)
                //{
                IDNameMap[actionIndex] = method.Name;
                NameIDMap[method.Name] = actionIndex;
                actionIndex++;
                //}
            }
        }

        private void HandleCall(ulong id, byte[] payload)
        {
            //Console.WriteLine($"receiving call {id}, {string.Join(',', payload)}");
            new Params(payload).Call(ui);
        }

        public void Dispose()
        {
            if (!IsHost) Shutdown();
            buffer.Dispose();
        }

        public void StartBrowser(int width, int height, string url)
        {
            var args = new Args("StartBrowser").Write(width).Write(height).Write(url);
            buffer.RemoteRequestAsync(args.ToBytes());
        }

        public void Shutdown()
        {
            buffer.RemoteRequest(new Args("Shutdown").ToBytes());
        }

        public void MouseDown(int x, int y, int button)
        {
            var args = new Args("MouseDown").Write(x).Write(y).Write(button);
            buffer.RemoteRequestAsync(args.ToBytes());
        }

        public void MouseUp(int x, int y, int button)
        {
            var args = new Args("MouseUp").Write(x).Write(y).Write(button);
            buffer.RemoteRequestAsync(args.ToBytes());
        }

        public void MouseButtonEvent(int x, int y, int button, bool down)
        {
            var args = new Args("MouseButtonEvent").Write(x).Write(y).Write(button).Write(down);
            buffer.RemoteRequestAsync(args.ToBytes());
        }

        public void MouseMove(int x, int y)
        {
            var args = new Args("MouseMove").Write(x).Write(y);
            buffer.RemoteRequestAsync(args.ToBytes());
        }

        public void KeyEvent(int type, int keyCode)
        {
            var args = new Args("KeyEvent").Write(type).Write(keyCode);
            buffer.RemoteRequestAsync(args.ToBytes());
        }

        private class Args
        {
            private MemoryStream ms = new MemoryStream();
            private BinaryWriter writer;

            public Args(string name)
            {
                writer = new BinaryWriter(ms);
                writer.Write(NameIDMap[name]);
            }

            internal byte[] ToBytes()
            {
                return ms.ToArray();
            }

            internal Args Write(int x)
            {
                writer.Write(x);
                return this;
            }

            internal Args Write(string x)
            {
                writer.Write(x);
                return this;
            }

            internal Args Write(bool x)
            {
                writer.Write(x);
                return this;
            }
        }

        private class Params
        {

            public MethodInfo Method { get; internal set; }
            public object[] Parameters { get; internal set; }

            public Params(byte[] payload)
            {
                BinaryReader reader = new BinaryReader(new MemoryStream(payload));
                string methodName = IDNameMap[reader.ReadByte()];
                Method = typeof(IBrowserUI).GetMethod(methodName);
                Parameters = new object[Method.GetParameters().Length];
                int index = 0;
                foreach (var param in Method.GetParameters())
                {
                    object value = null;
                    if (param.ParameterType == typeof(int))
                    {
                        value = reader.ReadInt32();
                        //Console.WriteLine($"Setting {param.Name} to {value}");
                    }
                    else if (param.ParameterType == typeof(string))
                    {
                        value = reader.ReadString();
                        //Console.WriteLine($"Setting {param.Name} to {value}");
                    }
                    else if (param.ParameterType == typeof(bool))
                    {
                        value = reader.ReadBoolean();
                        //Console.WriteLine($"Setting {param.Name} to {value}");
                    }
                    else
                    {
                        throw new Exception("Unknow param type");
                    }
                    Parameters[index++] = value;
                }

            }

            public void Call(IBrowserUI bridge)
            {
                Method.Invoke(bridge, Parameters);
            }
        }

    }

    public enum MouseButton
    {
        Left, Middle, Right
    }

    public interface IBrowserUI
    {
        void StartBrowser(int width, int height, string url);

        void Shutdown();

        void MouseDown(int x, int y, int button);

        void MouseUp(int x, int y, int button);

        void MouseButtonEvent(int x, int y, int button, bool down)
        {
            if (down) MouseDown(x, y, button);
            else MouseUp(x, y, button);
        }

        void MouseMove(int x, int y);

        void KeyEvent(int type, int keyCode);
    }
}
