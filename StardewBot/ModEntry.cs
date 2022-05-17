using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using BlocklyBridge;
using Farmtronics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace StardewBot
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private ConcurrentQueue<Action> queuedActions = new ConcurrentQueue<Action>();

        //private BotController robot;


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

        internal static ModEntry instance;
        public static IModHelper helper => ModHelper;
        
        // May not actually need to save this...
        public static ProgramState State { get; private set; }
        const string PROGRAMS_FILE = "programs.json";

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            ModHelper = helper;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Display.Rendered += Display_Rendered;

            // From Farmtronics
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.Saved += OnSaved;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            Logger.Implementation = new StardewLogger(Monitor);

            State = helper.Data.ReadJsonFile<ProgramState>(PROGRAMS_FILE) ?? new ProgramState();

            Dispatcher = new Dispatcher("127.0.0.1", 8000, action => queuedActions.Enqueue(action));

            Assembly assembly = Assembly.GetExecutingAssembly();
            Dispatcher.Start(assembly.GetTypes(), () =>
            {
                Logger.Log("Connected!!!");
                Dispatcher.State = State;
                Logger.Log("Loading: " + State.ToJSON());
                if (Bot.instances.Count > 0)
                {
                    Dispatcher.SetTarget(Bot.instances[0].Controller);
                }
            });
            Dispatcher.OnSave += Dispatcher_OnSave;

            // What about resize..?
            int width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Overlay = new WebOverlay(helper, width, height);
        }

        private void Dispatcher_OnSave(ProgramState obj)
        {
            State = obj;
            helper.Data.WriteJsonFile(PROGRAMS_FILE, obj);
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            GameRunner.instance.Exiting += Instance_Exiting;
        }

        private void Instance_Exiting(object sender, EventArgs e)
        {
            // TODO: This is never called - not sure why
            Overlay.Dispose();
        }

        private void Display_Rendered(object sender, RenderedEventArgs e)
        {
            //Overlay.Draw(e.SpriteBatch);
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Bot.ClearAll();
        }

        uint prevTicks;
        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            while(queuedActions.TryDequeue(out Action result))
            {
                result();
            }
            //if (robot != null) robot.Update();
            Overlay.Update();

            uint dTicks = e.Ticks - prevTicks;
            var gameTime = new GameTime(new TimeSpan(e.Ticks * 10000000 / 60), new TimeSpan(dTicks * 10000000 / 60));
            Bot.UpdateAll(gameTime);
            prevTicks = e.Ticks;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Farm farm = Game1.getFarm();
            //NPC robin = Game1.getCharacterFromName("Robin");
            //NPC bot = new NPC(robin.Sprite, Vector2.Zero, 0, "Robot");

            var player = Game1.player;
            player.addItemToInventory(new Bot(null));



            //BotFarmer bot = new BotFarmer("Robot");

            //farm.addCharacter(bot);
            //bot.currentLocation = farm;
            //bot.setTilePosition(farm.GetMainFarmHouseEntry() + new Point(1, 5));
            ////Logger.Log(robin.currentLocation.Name);

            //robot = new BotController(bot, "ROBOT1");
            //Dispatcher.SetTarget(robot);

            // print button presses to the console window
            //Monitor.Log($"Spawned robot!!: {bot.position}", LogLevel.Debug);
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




        public void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            Logger.Log($"Menu opened: {e.NewMenu}");
            if (e.NewMenu is ShopMenu shop)
            {
                if (shop.portraitPerson != Game1.getCharacterFromName("Pierre")) return;
                if (Game1.player.mailReceived.Contains("FarmtronicsFirstBotMail"))
                {
                    // Add a bot to the store inventory.
                    // Let's insert it after Flooring but before Catalogue.
                    int index = 0;
                    for (; index < shop.forSale.Count; index++)
                    {
                        var item = shop.forSale[index];
                        Logger.Log($"Shop item {index}: {item} with {item.Name}");
                        if (item.Name == "Catalogue" || (index > 0 && shop.forSale[index - 1].Name == "Flooring")) break;
                    }
                    var botForSale = new Bot(null);
                    shop.forSale.Insert(index, botForSale);
                    shop.itemPriceAndStock.Add(botForSale, new int[2] { 2500, int.MaxValue });  // sale price and available stock
                }
            }
        }

        public void OnSaving(object sender, SavingEventArgs args)
        {
            if (Context.IsMainPlayer) Bot.ConvertBotsToChests();
        }

        public void OnSaved(object sender, SavedEventArgs args)
        {
            if (Context.IsMainPlayer) Bot.ConvertChestsToBots();
        }

        public void OnSaveLoaded(object sender, SaveLoadedEventArgs args)
        {
            if (Context.IsMainPlayer) Bot.ConvertChestsToBots();
        }
    }
}