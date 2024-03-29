﻿using Browser.Common;
using SharedMemory;
using System;
using System.Threading;

namespace Browser.Common
{
    public class GraphicsReader : IDisposable
    {
        private readonly CircularBuffer buffer;

        private byte[] lastRead;
        private Thread thread;

        public bool HasNewBitmap { get;  private set; }
        public bool Failed { get; private set; }

        private int width, height;
        private int Size { get { return width * height * 4; } }

        public GraphicsReader(int width, int height)
        {
            this.width = width;
            this.height = height;
            try
            {
                buffer = new CircularBuffer(Settings.MEMORY_NAME);
            } catch (Exception e)
            {
                Failed = true;
                Console.WriteLine("Failed to initialize GraphicsReader: " + e.Message);
                return;
            }
            Action a = new Action(() =>
            {

                while (!buffer.ShuttingDown)
                {
                    byte[] bytes = new byte[Size];
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
            lock(this)
            { 
                buffer.Dispose();
                thread.Join();
            }
        }
    }
}
