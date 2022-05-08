using BlocklyBridge;
using Browser.Common;
using Microsoft.Xna.Framework.Graphics;
using SharedMemory;
using System;
using System.Threading;

namespace StardewBot
{
    public class WebOverlay
    {
        private readonly CircularBuffer buffer;

        private byte[] lastRead;
        Texture2D cachedTexture;

        public WebOverlay()
        {
            buffer = new CircularBuffer(BrowserSettings.MEMORY_NAME);
            Action a = new Action(() =>
            {

                while (true)
                {
                    byte[] bytes = new byte[BrowserSettings.NODE_SIZE];
                    int read = buffer.Read(bytes);
                    if (read == 0) continue;
                    Logger.Log("Read texture");
                    lock (this)
                    {
                        lastRead = bytes;
                        cachedTexture = null;
                    }
                }
            });
            Thread t = new Thread(new ThreadStart(a));
            t.Start();
        }

        public Texture2D ReadTexture(GraphicsDevice graphicsDevice)
        {
            lock (this)
            {
                if (cachedTexture != null) return cachedTexture;
                if (lastRead == null) return null;
                // Switch bgra -> rgba
                for (int i = 0; i < lastRead.Length; i += 4)
                {
                    byte temp = lastRead[i + 0];
                    lastRead[i + 0] = lastRead[i + 2];
                    lastRead[i + 2] = temp;
                }
                Texture2D texture = new Texture2D(graphicsDevice, BrowserSettings.MAX_WIDTH, BrowserSettings.MAX_HEIGHT);
                texture.SetData(lastRead);
                Console.WriteLine("Texture");
                cachedTexture = texture;
                return cachedTexture;
            }
        }
    }
}
