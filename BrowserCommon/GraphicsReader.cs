using Browser.Common;
using SharedMemory;
using System;
using System.Threading;

namespace BrowserCommon
{
    public class GraphicsReader : IDisposable
    {
        private readonly CircularBuffer buffer;

        private byte[] lastRead;
        private Thread thread;

        public bool HasNewBitmap { get;  private set; }

        public GraphicsReader()
        {
            buffer = new CircularBuffer(BrowserSettings.MEMORY_NAME);
            Action a = new Action(() =>
            {

                while (!buffer.ShuttingDown)
                {
                    byte[] bytes = new byte[BrowserSettings.NODE_SIZE];
                    int read = buffer.Read(bytes);
                    if (read == 0) continue;
                    Console.WriteLine("Read texture");
                    lock (this)
                    {
                        lastRead = bytes;
                        HasNewBitmap = true;
                    }
                }
            });
            thread = new Thread(new ThreadStart(a));
            thread.Start();
        }

        public byte[] ReadBitmap()
        {
            lock(this)
            {
                HasNewBitmap = false;
                return lastRead;
            }
        }

        public void Dispose()
        {
            buffer.Dispose();
            thread.Join();
        }
    }
}
