﻿using BlocklyBridge;
using Browser.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharedMemory;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Threading;

namespace StardewBot.Overlays
{
    public class SharedMemoryOverlay : IProgrammingOverlay, IKeyboardSubscriber
    {

        class Menu : IClickableMenu
        {
            SharedMemoryOverlay overlay;

            public Menu(SharedMemoryOverlay overlay, int width, int height)
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
        private bool externalBrowser;

        private Menu menu;

        public bool Showing { get; private set; }
        public bool Selected { get => Showing; set { } }

        public SharedMemoryOverlay(IModHelper helper, int width, int height)
            
        {
            this.helper = helper;
            this.width = width;
            this.height = height;

            helper.Events.Input.CursorMoved += Input_CursorMoved;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonReleased += Input_ButtonReleased;
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;

        }

        public void Initialize(string blocklyPath)
        {
            bridge = new IOBridge();
            //string url = "https://www.google.com";
            //string url = "https://blockly-demo.appspot.com/static/demos/code/index.html";
            string url = blocklyPath;
            bridge.StartBrowser(width, height, url);
            reader = new GraphicsReader(width, height);
            if (reader.Failed)
            {
                externalBrowser = true;
                return;
            }
            menu = new Menu(this, width, height);
        }

        private void KeyboardInput_CharEntered(object sender, CharacterEventArgs e)
        {
            Logger.Log("Keyboard: " + e.Character);
            if (!Showing) return; 
            bridge.KeyEvent(3, e.Character);
        }

        public void Show()
        {
            if (!Showing) ToggleShowing();
        }

        public void Hide()
        {
            if (Showing) ToggleShowing();
        }

        public void ToggleShowing()
        {
            Showing = !Showing;
            Game1.activeClickableMenu = Showing ? menu : null;
            if (Showing)
            {
                Game1.keyboardDispatcher.Subscriber = this;
            }
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
            if (Showing && e.Button == SButton.R)
            {
                bridge.Refresh();
            }
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
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            texture.SetData(lastRead);
            Console.WriteLine("Texture");
            cachedTexture = texture;
            return cachedTexture;
        }

        public void Update()
        {
            if (externalBrowser) return;
            //Logger.Log(helper.Input.GetState(SButton.X));
            //Microsoft.Xna.Framework.Input.Keyboard.GetState().GetPressedKeys
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (externalBrowser || !Showing) return;
            var texture = ReadTexture(spriteBatch.GraphicsDevice);
            if (texture == null) return;
            spriteBatch.Draw(texture, new Vector2(0, 0), new Color(255, 255, 255, 255));
        }

        public void RecieveTextInput(char inputChar)
        {
            bridge.KeyEvent(3, inputChar);
        }

        public void RecieveTextInput(string text)
        {
            Logger.Log("Receiving: " + text);
        }

        public void RecieveCommandInput(char command)
        {
            bridge.KeyEvent(3, command);
        }

        public void RecieveSpecialInput(Keys key)
        {
            Logger.Log("Receiving: " + key);
        }
    }
}
