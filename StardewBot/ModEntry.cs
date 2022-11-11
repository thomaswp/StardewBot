using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BlocklyBridge;
using Farmtronics;
using MailFrameworkMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewBot.API;
using StardewBot.Levels;
using StardewBot.Overlays;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using WindowsAPI;
using xTile.Layers;
using static StardewValley.GameLocation;

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

        public static IProgrammingOverlay Overlay { get; private set; }

        internal static ModEntry instance;
        public static IModHelper helper => ModHelper;


        // May not actually need to save this...
        public static ProgramState State { get; private set; }
        public const string PROGRAMS_FILE = "programs.json";

        public static StardewLogger logger;

        public static OptionsButtonMenu options;

        DGAAPI dga;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            

            instance = this;
            ModHelper = helper;
            logger = new StardewLogger(Monitor);

            logger.LogSessionStart();

            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Display.Rendered += Display_Rendered;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;

            // From Farmtronics
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.Saved += OnSaved;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            helper.Events.Display.WindowResized += Display_WindowResized;

            State = helper.Data.ReadJsonFile<ProgramState>(PROGRAMS_FILE) ?? new ProgramState();


            //var test = new JObject();
            //test.Add("Test", "testValue");
            //helper.Data.WriteJsonFile<JObject>("Levels/xml/test.json", test);

            Dispatcher = new Dispatcher("127.0.0.1", 8000, action => queuedActions.Enqueue(action));

            Assembly assembly = Assembly.GetExecutingAssembly();

            Level.LoadTutorialCode(State, helper);
            /*Dispatcher.State = State;*/

            Dispatcher.Start(assembly.GetTypes(), () =>
            {
                logger.Log("Connected!!!");
                Dispatcher.State = State;
                logger.Log("Loading: " + State.ToJSON());
                if (Bot.instances.Count > 0)
                {
                    Dispatcher.SetTarget(Bot.instances[0].Controller);
                }   
            });
            Logger.OnLog += Logger_OnLog;
            Logger.OnWarn += Logger_OnWarn;
            Dispatcher.OnSave += Dispatcher_OnSave;
            Dispatcher.OnReceiveMinimize += Dispatcher_OnReceiveMinimize;

            //var bounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
            //// GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //int width = bounds.Width;
            //int height = bounds.Height;
            //logger.Log($"Width: {width}; Height: {height}");
            //Overlay = new WebOverlay(helper, width, height);

            // TODO: What about resize..?
            Overlay = new BrowserProgrammingOverlay();

            options = new OptionsButtonMenu();
        }

        private void Dispatcher_OnReceiveMinimize()
        {
            Overlay.Hide();
        }

        private void Display_WindowResized(object sender, WindowResizedEventArgs e)
        {
            logger.Log("Resizing to: " + e.NewSize);
            Dispatcher.ResizeBlockly(e.NewSize.X, e.NewSize.Y);
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //if (e.Button == SButton.X)
            //{
            //    logger.Log("Toggling Overlay");
            //    if (Overlay.Showing) Overlay.Hide();
            //    else Overlay.Show();
            //}

            if (!Context.IsWorldReady)
                return;

            if (e.Button == SButton.H)
            {
                toggleMenu();
            }

            if (e.Button == SButton.P)
            {
                Game1.activeClickableMenu = new OptionsButtonMenu();
                //Level.resetLevelClearedDebug();
            }

            if(e.Button == SButton.MouseRight)
            {
                if(Game1.player.ActiveObject != null && Game1.player.ActiveObject.Name == "Bot Phone" && Game1.activeClickableMenu is null)
                {
                    //logger.Log("Toggling Overlay");
                    if (Overlay.Showing) Overlay.Hide();
                    else Overlay.Show();
                    //toggleMenu();
                }
            }
        }

        private void Dispatcher_OnSave(String target, String code, ProgramState obj)
        {
            State = obj;
            var message = new JObject();
            message.Add("Bot", target);
            logger.Log(message.ToString(), action: "IDE.CodeChanged", code: code);
            helper.Data.WriteJsonFile(PROGRAMS_FILE, obj);
        }

        private void Logger_OnLog(String message, String code)
        {
            var jMessage = new JObject();
            jMessage.Add("Type", message);
            logger.Log(jMessage, code: code, action: "IDE.OnChanged");
        }

        private void Logger_OnWarn(String message)
        {
            logger.Warn(message);
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Note: Does not work curretly
            GameRunner.instance.Exiting += Instance_Exiting;

            dga = helper.ModRegistry.GetApi<DGAAPI>("spacechase0.DynamicGameAssets");
            if (dga is not null) dga.AddEmbeddedPack(ModManifest, Path.Combine(helper.DirectoryPath, "assets", "dga"));

            var bounds = Game1.game1.Window.ClientBounds;
            Dispatcher.ResizeBlockly(bounds.Width, bounds.Height);

            try
            {
                var config = Helper.ModContent.Load<Dictionary<string, string>>("assets/config.json");
                string blocklyPath = config["blocklyPath"];
                string browserPath = Path.Combine(helper.DirectoryPath, "assets", "Farmbot-Blockly", "step-execution.html"); 
                browserPath = File.Exists(browserPath) ? browserPath : config["browserPath"];
                // logger.Log("Blockly path: " + blocklyPath);
                // TODO: Create smart default instead of crashing

                Overlay.Initialize(blocklyPath, browserPath); 
            }
            catch
            {
                throw new Exception("Please create assets/config.json and specify a 'blocklyPath'");
            }

            //Game1.onScreenMenus.Add(new OptionsButtonMenu());
        }

        private void Instance_Exiting(object sender, EventArgs e)
        {
            // TODO: This is never called - not sure why
            Overlay.Dispose();
        }

        private void Display_Rendered(object sender, RenderedEventArgs e)
        {
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Bot.ClearAll();
        }

        uint prevTicks;
        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {

            while (queuedActions.TryDequeue(out Action result))
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
            //Farm farm = Game1.getFarm();
            //NPC robin = Game1.getCharacterFromName("Robin");
            //NPC bot = new NPC(robin.Sprite, Vector2.Zero, 0, "Robot");

            // TODO: Remove
            Game1.options.musicVolumeLevel = 0;

            //var player = Game1.player;
            //player.addItemToInventory(new Bot("1234", null));

            //Level.LoadLevel("loops-line");
            //Level.LoadLevel("CamLevel");

            //BotFarmer bot = new BotFarmer("Robot");

            //farm.addCharacter(bot);
            //bot.currentLocation = farm;
            //bot.setTilePosition(farm.GetMainFarmHouseEntry() + new Point(1, 5));
            ////logger.Log(robin.currentLocation.Name);

            //robot = new BotController(bot, "ROBOT1");
            //Dispatcher.SetTarget(robot);

            // print button presses to the console window
            //Monitor.Log($"Spawned robot!!: {bot.position}", LogLevel.Debug);
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            var eodcrops = Game1.player.displayedShippedItems.ToString() ?? Game1.getFarm().getShippingBin(Game1.player).ToString() ?? "No Crops Sold";
            var jojamember = Game1.player.mailReceived.Contains("JojaMember") ? "Yes" : "No";
            List<String> bundlesCompleted = new List<string>();

            if (Game1.player.mailReceived.Contains("ccCraftsRoom")) bundlesCompleted.Add("ccCraftsRoom");
            if (Game1.player.mailReceived.Contains("ccVault")) bundlesCompleted.Add("ccVault");
            if (Game1.player.mailReceived.Contains("ccFishTank")) bundlesCompleted.Add("ccFishTank");
            if (Game1.player.mailReceived.Contains("ccPantry")) bundlesCompleted.Add("ccPantry");
            if (Game1.player.mailReceived.Contains("ccBulletin")) bundlesCompleted.Add("ccBulletin");
            if (Game1.player.mailReceived.Contains("ccBoilerRoom")) bundlesCompleted.Add("ccBoilerRoom");
            if (Game1.player.mailReceived.Contains("ccPantry")) bundlesCompleted.Add("ccPantry");
            if (Game1.player.mailReceived.Contains("ccBulletin")) bundlesCompleted.Add("ccBulletin");
            if (Game1.player.mailReceived.Contains("jojaCraftsRoom")) bundlesCompleted.Add("jojaCraftsRoom");
            if (Game1.player.mailReceived.Contains("jojaVault")) bundlesCompleted.Add("jojaVault");
            if (Game1.player.mailReceived.Contains("jojaFishTank")) bundlesCompleted.Add("jojaFishTank");
            if (Game1.player.mailReceived.Contains("jojaBoilerRoom")) bundlesCompleted.Add("jojaBoilerRoom");
            if (Game1.player.mailReceived.Contains("jojaPantry")) bundlesCompleted.Add("jojaPantry");

            logger.Log($"Day Ended. Crops sold: {eodcrops}. Joja Member: {jojamember}. Bundles Complete: {bundlesCompleted}.", action: "Game.DayEnded");
        }

        /*********
        ** Private methods
        *********/

        private void toggleMenu()
        {
            if (Game1.activeClickableMenu is Levels.TutorialMenu menu)
            {
                
                menu.exitThisMenu();
            } else
            {
                Game1.activeClickableMenu = new Levels.TutorialMenu();
            }

        }




        public void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            logger.Log($"Menu opened: {e.NewMenu}");
            //if (e.NewMenu is ShopMenu shop)
            //{
            //    if (shop.portraitPerson != Game1.getCharacterFromName("Pierre")) return;
            //    if (Game1.player.mailReceived.Contains("FirstBotLetter"))
            //    {
            //        logger.Log("Adding item to Pierres Shop!");
            //        // Add a bot to the store inventory.
            //        // Let's insert it after Flooring but before Catalogue.
            //        int index = 0;
            //        for (; index < shop.forSale.Count; index++)
            //        {
            //            var item = shop.forSale[index];
            //            logger.Log($"Shop item {index}: {item} with {item.Name}");
            //            if (item.Name == "Catalogue" || (index > 0 && shop.forSale[index - 1].Name == "Flooring")) break;
            //        }
            //        var botForSale = new Bot(null, farmer: null);
            //        shop.forSale.Insert(index, botForSale);
            //        shop.itemPriceAndStock.Add(botForSale, new int[2] { 2500, int.MaxValue });  // sale price and available stock
            //    }
            //}

            TVOverride.OverrideTVMenu(e);
        }

       

        public void OnSaving(object sender, SavingEventArgs args)
        {
            if (Context.IsMainPlayer)
            {
                Bot.ConvertBotsToChests();

                LevelData levelSave = new LevelData();
                foreach(var level in Level.levelsDict.Values)
                {
                    levelSave.LevelsCompleted[level.MapName] = level.isCompleted;
                }


                helper.Data.WriteSaveData<LevelData>("level-save", levelSave);
            }

            
        }

        public void OnSaved(object sender, SavedEventArgs args)
        {
            if (Context.IsMainPlayer) Bot.ConvertChestsToBots();
        }

        public void OnSaveLoaded(object sender, SaveLoadedEventArgs args)
        {
            if (Context.IsMainPlayer) Bot.ConvertChestsToBots();
            Level.LoadAssets(Helper);

            var levelSave = helper.Data.ReadSaveData<LevelData>("level-save");

            if (levelSave != null)
            {
                foreach (var level in Level.levelsDict.Values)
                {
                    if (levelSave.LevelsCompleted.ContainsKey(level.MapName))
                    {
                        level.isCompleted = true;
                    }
                }
            }

            var stg = dga.ListContentPacks();
            var test = dga.GetItemsByPack("StardewBot");
            StardewValley.Object phone;

            if (Array.IndexOf<String>(stg, "StardewBot") > -1)
            {
                phone = (StardewValley.Object)dga.SpawnDGAItem(test[0]);

                MailDao.SaveLetter(
                   new Letter(
                       "FirstBotLetter"
                       , "Here is you first bot. Don't break it. ^~Some Body"
                       , new List<Item> { new Bot("0001", farmer: null), phone }
                       , (l) => !Game1.player.mailReceived.Contains(l.Id)
                       , (l) => Game1.player.mailReceived.Add(l.Id)
                   )
               );
            }
        }

        class LevelData
        {
            public Dictionary<String, bool> LevelsCompleted { get; set; }

            public LevelData()
            {
                LevelsCompleted = new Dictionary<string, bool>();
            }
        }
    }
}