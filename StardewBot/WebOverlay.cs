using BlocklyBridge;
using Browser.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharedMemory;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Threading;

namespace StardewBot
{


    public class WebOverlay : IDisposable
    {

        class Menu : IClickableMenu
        {
            WebOverlay overlay;

            public Menu(WebOverlay overlay, int width, int height)
                : base(0, 0, width, height)
            {
                this.overlay = overlay;
            }

            public override void update(GameTime time)
            {
                base.update(time);
            }

            public override void draw(SpriteBatch b)
            {
                base.draw(b);
                overlay.Draw(b);
                drawMouse(b);
            }
        }

        Texture2D cachedTexture;

        private GraphicsReader reader;
        private IOBridge bridge;
        private int width, height;
        private IModHelper helper;

        private Menu menu;

        public bool Showing { get; private set; }

        public WebOverlay(IModHelper helper, int width, int height)
            
        {
            this.helper = helper;
            this.width = width;
            this.height = height;
            bridge = new IOBridge();
            //string url = "https://www.google.com";
            //string url = "https://blockly-demo.appspot.com/static/demos/code/index.html";
            string url = @"C:\xampp\htdocs\farmbot-blockly\step-execution.html";
            bridge.StartBrowser(width, height, url);
            reader = new GraphicsReader(width, height);

            helper.Events.Input.CursorMoved += Input_CursorMoved;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonReleased += Input_ButtonReleased;
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            //helper.Input.

            menu = new Menu(this, width, height);
        }

        public void ToggleShowing()
        {
            Showing = !Showing;
            Game1.activeClickableMenu = Showing ? menu : null;
        }

        private void Input_MouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            //bridge.KeyEvent()
        }

        private void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            HandleButtonEvent(e.Button, e.Cursor, false);
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //Logger.Log(e.Button);
            //if (e.Button == SButton.X) ToggleShowing();
            HandleButtonEvent(e.Button, e.Cursor, true);
        }

        private void HandleButtonEvent(SButton button, ICursorPosition cursor, bool down)
        {
            if (!Showing) return;
            var pos = cursor.ScreenPixels;
            int x = (int)pos.X, y = (int)pos.Y;
            //Logger.Log($"({x}, {y})");
            if (button == SButton.MouseLeft) bridge.MouseButtonEvent(x, y, (int)MouseButton.Left, down);
            else if (button == SButton.MouseMiddle) bridge.MouseButtonEvent(x, y, (int)MouseButton.Middle, down);
            else if (button == SButton.MouseRight) bridge.MouseButtonEvent(x, y, (int)MouseButton.Right, down);

        }

        private void Input_CursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (!Showing) return;
            var position = e.NewPosition.ScreenPixels;
            bridge.MouseMove((int)position.X, (int)position.Y);
        }

        public void Dispose()
        {
            reader.Dispose();
            bridge.Dispose();
        }

        public Texture2D ReadTexture(GraphicsDevice graphicsDevice)
        {
            if (!reader.HasNewBitmap) return cachedTexture;
            byte[] lastRead = reader.ReadBitmap();
            if (lastRead == null) return null;
                
            // Switch bgra -> rgba
            for (int i = 0; i < lastRead.Length; i += 4)
            {
                byte temp = lastRead[i + 0];
                lastRead[i + 0] = lastRead[i + 2];
                lastRead[i + 2] = temp;
            }
            Texture2D texture = new Texture2D(graphicsDevice, Settings.MAX_WIDTH, Settings.MAX_HEIGHT);
            texture.SetData(lastRead);
            Console.WriteLine("Texture");
            cachedTexture = texture;
            return cachedTexture;
        }

        public void Update()
        {
            //Logger.Log(helper.Input.GetState(SButton.X));
            //Microsoft.Xna.Framework.Input.Keyboard.GetState().GetPressedKeys
            if (helper.Input.GetState(SButton.X) == SButtonState.Pressed)
            {
                ToggleShowing();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Showing) return;
            var texture = ReadTexture(spriteBatch.GraphicsDevice);
            if (texture == null) return;
            spriteBatch.Draw(texture, new Vector2(0, 0), new Color(255, 255, 255, 255));
        }
    }
}
