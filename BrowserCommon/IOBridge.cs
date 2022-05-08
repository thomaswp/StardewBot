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
            buffer.Dispose();
        }

        public void MouseDown(int x, int y)
        {
            var args = new Args("MouseDown").Write(x).Write(y);
            buffer.RemoteRequestAsync(args.ToBytes());
        }

        public void MouseUp(int x, int y)
        {
            var args = new Args("MouseUp").Write(x).Write(y);
            buffer.RemoteRequestAsync(args.ToBytes());
        }

        public void MouseMove(int x, int y)
        {
            var args = new Args("MouseMove").Write(x).Write(y);
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
    //class Callable : Attribute
    //{

    //}

    public interface IBrowserUI
    {
        //[Callable]
        void MouseDown(int x, int y);

        //[Callable]
        void MouseUp(int x, int y);

        //[Callable]
        void MouseMove(int x, int y);
    }
}
