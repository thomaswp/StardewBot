using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using BlocklyBridge;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace StardewBot
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private ConcurrentQueue<Action> queuedActions = new ConcurrentQueue<Action>();

        private BotController robot;


        public static Dispatcher Dispatcher
        {
            get;
            private set;
        }
        public static IModHelper ModHelper
        {
            get;
            private set;
        }

        public WebOverlay Overlay;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Display.Rendered += Display_Rendered;

            Logger.Implementation = new StardewLogger(Monitor);

            Dispatcher = new Dispatcher("127.0.0.1", 8000, action => queuedActions.Enqueue(action));

            Assembly assembly = Assembly.GetExecutingAssembly();
            Dispatcher.Start(assembly.GetTypes(), () =>
            {
                Logger.Log("Connected!!!");
                if (robot != null) Dispatcher.SetTarget(robot);
            });

            // What about resize..?
            int width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Overlay = new WebOverlay(helper, width, height);
        }

        private void Display_Rendered(object sender, RenderedEventArgs e)
        {
            Overlay.Draw(e.SpriteBatch);
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            while(queuedActions.TryDequeue(out Action result))
            {
                result();
            }
            if (robot != null) robot.Update();
            Overlay.Update();
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Farm farm = Game1.getFarm();
            //NPC robin = Game1.getCharacterFromName("Robin");
            //NPC bot = new NPC(robin.Sprite, Vector2.Zero, 0, "Robot");

            var player = Game1.player;
            BotFarmer bot = new BotFarmer("Robot");

            farm.addCharacter(bot);
            bot.currentLocation = farm;
            bot.setTilePosition(farm.GetMainFarmHouseEntry() + new Point(1, 5));
            //Logger.Log(robin.currentLocation.Name);

            robot = new BotController(bot, "ROBOT1");
            Dispatcher.SetTarget(robot);

            // print button presses to the console window
            Monitor.Log($"Spawned robot!!: {bot.position}", LogLevel.Debug);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            //if (robot != null && e.Button == SButton.O)
            //{
            //    //robot.tryToMoveInDirection(0, false, 0, false);
            //    robot.jump();
            //}

        }
    }
}