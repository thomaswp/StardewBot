using System;
using SharedMemory;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlocklyBridge;

namespace BrowserHost
{
    public class Interchange : IDisposable
    {
        public const int NODES = 500;

        private readonly CircularBuffer buffer;

        public Interchange()
        {
            buffer = new CircularBuffer(BrowserSettings.MEMORY_NAME, NODES, BrowserSettings.NODE_SIZE);

        }
    
        public void Write(byte[] bytes)
        {
            if (bytes.Length > BrowserSettings.NODE_SIZE)
            {
                throw new Exception("Too much data!");
            }
            Console.WriteLine($"Writng {bytes.Length} bytes");
            buffer.Write(bytes);
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
