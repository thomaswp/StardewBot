using BlocklyBridge;
using Farmtronics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace StardewBot.Levels
{
    public abstract class Level
    {
        public abstract string MapName { get; }
        public abstract string EntryMessage { get; }
        public virtual string ID { get { return MapName; } }

        //File Strings
        public const string FieldLocationName = "PracticeField";
        private const string MapPathRoot = "assets/maps/";
        private const string xmlCodeRoot = "assets/xml/";
        private const string FieldMapPath = MapPathRoot + "field.tmx";
        private const string OriginFieldMapPath = MapPathRoot + "originField.tmx";

        //Level State
        protected Map levelMap;
        protected Point botStart;
        protected Point levelOffset;
        protected static Point? botGoal;
        protected static List<Point?> botGoals;
        protected int startFacingDir;
        protected Bot bot;
        public GameLocation LevelLocation { get; private set; }
        public Sublevel currentSublevel;
        public bool isCompleted { get; set; }

        public Dictionary<String, Vector2> cropsLocations = new();
        public Dictionary<Vector2, String> objectLocations
        {
            get
            {
                Dictionary<Vector2, String> holder = new Dictionary<Vector2, string>();
                foreach (var obj in LevelLocation.objects.Pairs)
                {
                    if (LevelLocation.doesTileHaveProperty((int)obj.Key.X, (int)obj.Key.Y, "Collect", "Paths") != null)
                    {
                        holder.TryAdd(obj.Key, obj.Value.name);
                    }
                }
                return holder;
            }
        }

        //Static Members
        public static GameLocation OriginLocation { get; private set; }
        public static Level CurrentLevel { get; private set; }
        public static Dictionary<string, Level> levelsDict = new();
        internal int timesOpened;

        public static List<Level> GetLevels
        {
            get
            {
                List<Level> holder = new List<Level>();
                foreach (var level in levelsDict.Values)
                {
                    holder.Add(level);
                }
                return holder;
            }
        }


        static Level()
        {
            foreach (var level in new Level[]
            {
                //Must be listed in the opposite order as you would like displayed in the menu.
                //new Loops1(),
                //new CamLevel(),
                //new Ifs3(),
                new LevelConditional_1(),
                new LevelMultiActionLoops_2(),
                new LevelMultiActionLoops_1(),
                new LevelSingleActionLoops_1(),
                new LevelBasics()
                //new Variables_1(),
                //new Functions_1()
                
            })
            {
                level.isCompleted = false;
                levelsDict.Add(level.ID, level);
                level.timesOpened = 0;
            }
        }

        public static void resetLevelClearedDebug()
        {
            foreach (var level in levelsDict.Values)
            {
                level.isCompleted = false;
            }
        }


        //Static methods for loading levels and tutorial code
        internal static void LoadTutorialCode(ProgramState state, IModHelper helper)
        {
            foreach (var level in levelsDict.Values)
            {
                Sublevel holder = level.currentSublevel;
                while (holder != null)
                {
                    var botXML = holder.loadBotXML(helper);
                    if (botXML != null)
                    {
                        var program = state.GetProgram(botXML.Guid);
                        program.Code = botXML.Code;
                        program.VarMap = botXML.VarMap;

                        helper.Data.WriteGlobalData("programs", state);
                        //helper.Data.WriteJsonFile(ModEntry.PROGRAMS_FILE, state);
                    }

                    holder = holder.next;
                }
            }
        }

        public static void LoadAssets(IModHelper helper)
        {
            foreach (var level in levelsDict.Values)
            {
                Sublevel holder = level.currentSublevel;
                do
                {
                    if (holder == null) break;

                    holder.LoadLocation(helper);
                } while ((holder = holder.next) != null);

            }
        }

        protected void LoadLocation(IModHelper helper)
        {
            //string mapAssetKey = helper.ModContent.GetInternalAssetName(MapPathRoot + MapName + ".tmx").BaseName;
            //This needs to be changed! The map name is set to the last level initilized
            string mapAssetKey = ModEntry.helper.ModContent.GetInternalAssetName(FieldMapPath).BaseName;
            string originMapAssetKey = ModEntry.helper.ModContent.GetInternalAssetName(OriginFieldMapPath).BaseName;
            levelMap = helper.ModContent.Load<Map>(MapPathRoot + MapName + ".tmx");
            LevelLocation = new GameLocation(mapAssetKey, FieldLocationName + MapName) { IsOutdoors = true, IsFarm = true };
            OriginLocation = new GameLocation(originMapAssetKey, FieldLocationName + "Origin") { IsOutdoors = true, IsFarm = true };
            Game1.locations.Add(LevelLocation);
            Game1.locations.Add(OriginLocation);



            //Setup();

            //foreach (var tilesheet in FieldLocation.Map.TileSheets)
            //{
            //    levelLocation.Map.AddTileSheet(tilesheet);
            //}
        }

        public static bool LoadLevel(string id)
        {
            if (!levelsDict.TryGetValue(id, out var level)) return false;
            CurrentLevel = level;
            setUpLevel(level.currentSublevel);
            mapOverlayToggle();

            level.currentSublevel.timesOpened++;
            level.currentSublevel.timesRan = 0;

            var message = new JObject();
            message.Add("Level Name", id);
            message.Add("Sublevel Name", level.currentSublevel.MapName);
            message.Add("Sublevel Already Completed?", level.currentSublevel.isCompleted ? "Yes" : "No");
            message.Add("Times Sublevel Opened", level.currentSublevel.timesOpened);
            ModEntry.logger.Log(message, "Tutorial.Opened");

            /*level.currentSublevel.Setup();
            level.currentSublevel.setUpConditions();
            level.currentSublevel.TeleportToStart();*/
            return true;
        }

        public static bool LoadNextLevel()
        {
            setUpLevel(CurrentLevel.currentSublevel);
            /*CurrentLevel.currentSublevel.Setup();
            CurrentLevel.currentSublevel.setUpConditions();
            CurrentLevel.currentSublevel.TeleportToStart();*/
            return true;
        }

        private static bool setUpLevel(Sublevel level)
        {
            level.Setup();
            level.setUpConditions();

            level.TeleportToStart();
            ModEntry.Dispatcher.LoadTutorial(Level.CurrentLevel.MapName + " " + level.type, level.explanationText);
            //level.plantCrops();
            ModEntry.ModHelper.Events.Player.Warped += level.onWarp;
            return true;
        }

        public static void IDEReload()
        {
            if (Level.CurrentLevel == null || Level.CurrentLevel.currentSublevel == null) return;
            var sublevel = Level.CurrentLevel.currentSublevel;

            Level.CurrentLevel.currentSublevel.bot.SwitchTo();
            ModEntry.Dispatcher.LoadTutorial(Level.CurrentLevel.MapName + " " + sublevel.type, sublevel.explanationText);

        }

        public void onWarp(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            //plantCrops();

        }

        protected void Setup()
        {
            var entry = LevelLocation.GetMapPropertyPosition("Entry", 30, 30);

            Layer back = levelMap.GetLayer("Back");
            var anchor = FindTilePointWithProperty(back, "Anchor");
            var start = FindTilePointWithProperty(back, "BotStart");
            var goal = FindTilePointWithProperty(back, "BotGoal");
            /*var goals = FindTilePointsWithProperty(back, "BotGoal");*/

            ModEntry.logger.Log(anchor);
            ModEntry.logger.Log(start);
            ModEntry.logger.Log(goal);
            /*ModEntry.logger.Log(goals);*/

            if (anchor == null)
            {
                ModEntry.logger.Warn("No anchor for map: " + ID);
                return;
            }

            if (start == null)
            {
                ModEntry.logger.Warn("No anchor for map: " + ID);
                return;
            }

            Point offset = (Point)(entry - anchor);
            botStart = (Point)(start + offset);
            botGoal = goal + offset;

            /*foreach( var goal in goals )
            {
                botGoals.Add(goal + offset);
            }*/

            levelOffset = offset;
            ModEntry.logger.Log("Offset: " + offset);

            Tile baseSpringTile = LevelLocation.Map.GetLayer("Back").Tiles[0, 0];
            Tile basePathsTile = LevelLocation.Map.GetLayer("Paths").Tiles[0, 0];

            Tile OriginBaseSpringTile = OriginLocation.map.GetLayer("Back").Tiles[0, 0];
            Tile OriginBasePathsTile = OriginLocation.Map.GetLayer("Paths").Tiles[0, 0];

            string facingStr = back.Tiles[start.Value.X, start.Value.Y].Properties["Facing"];
            if (int.TryParse(facingStr, out int dir)) startFacingDir = dir;

            foreach (var layer in OriginLocation.map.Layers)
            {
                //if (layer.Id == "Paths") continue;

                var fieldLayer = LevelLocation.Map.GetLayer(layer.Id);
                if (fieldLayer == null)
                {
                    ModEntry.logger.Warn("Missing layer: " + layer.Id);
                    continue;
                }

                var baseTile = layer.Id == "Paths" ? basePathsTile : baseSpringTile;

                

                //ModEntry.logger.Log("Layer: " + layer.Id);
                for (int x = 0; x < layer.TileWidth; x++)
                {
                    for (int y = 0; y < layer.TileHeight; y++)
                    {
                        var tile = layer.Tiles[y, x];
                        if (tile == null)
                        {
                            fieldLayer.Tiles[y, x] = null;
                            continue;
                        }

                        Tile newTile = baseTile.Clone(fieldLayer);
                        newTile.TileIndex = tile.TileIndex;
                        newTile.TileIndexProperties.Clear();
                        foreach (var prop in tile.TileIndexProperties)
                        {
                            newTile.TileIndexProperties.Add(prop);
                        }

                        fieldLayer.Tiles[y, x] = newTile;
                    }
                }
            }

            foreach (var layer in levelMap.Layers)
            {
                //if (layer.Id == "Paths") continue;

                var fieldLayer = LevelLocation.Map.GetLayer(layer.Id);
                if (fieldLayer == null)
                {
                    ModEntry.logger.Warn("Missing layer: " + layer.Id);
                    continue;
                }

                var baseTile = layer.Id == "Paths" ? basePathsTile : baseSpringTile;

                //ModEntry.logger.Log("Layer: " + layer.Id);
                for (int x = 0; x < layer.TileWidth; x++)
                {
                    for (int y = 0; y < layer.TileHeight; y++)
                    {
                        var tile = layer.Tiles[y, x];
                        if (tile == null) continue;

                        Tile newTile = baseTile.Clone(fieldLayer);
                        newTile.TileIndex = tile.TileIndex;
                        newTile.TileIndexProperties.Clear();
                        foreach (var prop in tile.TileIndexProperties)
                        {
                            newTile.TileIndexProperties.Add(prop);
                        }

                        newTile.Properties.Clear();
                        foreach (var prop in tile.Properties)
                        {
                            newTile.Properties.Add(prop);
                        }

                        fieldLayer.Tiles[y + offset.X, x + offset.Y] = newTile;
                    }
                }
            }
        }

        private void TeleportToStart()
        {
            GameLocation location = LevelLocation;

            // of creating a function here, an existing function can be used, e.g. if the same function is used multiple times
            DelayedAction.delayedBehavior TeleportFunction = () =>
            {
                //Insert here the coordinates you want to teleport to
                Layer layer = location.map.GetLayer("Back");
                Layer tileData = levelMap.GetLayer("Back");

                var anchor = FindTilePointWithProperty(tileData, "Anchor");
                Point? entry = FindTilePointWithProperty(tileData, "Entry") + levelOffset ?? location.GetMapPropertyPosition("Entry", 30, 30);
                // ?? location.GetMapPropertyPosition("Entry", 30, 30)
                int dir = (int)Direction.Right;

                LevelLocation.resetForPlayerEntry();

                //The teleport command itself
                Game1.warpFarmer(new LocationRequest(location.NameOrUniqueName, location.uniqueName.Value != null, location), entry.Value.X, entry.Value.Y, dir);

                if (this.bot != null)
                {
                    bot.SwitchTo();
                }

                ResetBot();
                //DelayedAction.functionAfterDelay(() => plantCrops(), 1000);

                //Provide Entry Message for this Level
                if (!String.IsNullOrEmpty(EntryMessage)) Game1.drawObjectDialogue(EntryMessage);
            };
            // Delayed action to be executed after a set time (here 0,1 seconds)
            // Teleporting without the delay may prove to be problematic
            DelayedAction.functionAfterDelay(TeleportFunction, 100);



        }



        private void plantCrops()
        {
            var location = LevelLocation;
            var back = levelMap.GetLayer("Back");
            var tilePoints = FindTilePointsWithProperty(back, "PlantCrop");


            foreach (var tileLocation in tilePoints)
            {
                var offsetPoint = tileLocation + levelOffset;
                var x = offsetPoint.Value.X;
                var y = offsetPoint.Value.Y;

                var tile = back.Tiles[tileLocation.Value.X, tileLocation.Value.Y];

                int index;
                int harvest;
                int water;
                int goal;

                try
                {
                    int.TryParse(tile.Properties["Index"], out var idx);
                    index = idx;
                }
                catch { index = -1; }

                try
                {
                    int.TryParse(tile.Properties["Harvestable"], out var harv);
                    harvest = harv;
                }
                catch { harvest = -1; }

                try
                {
                    int.TryParse(tile.Properties["Watered"], out var wat);
                    water = wat;
                }
                catch { water = -1; }

                try
                {
                    int.TryParse(tile.Properties["Goal"], out var g);
                    goal = g;
                }
                catch { goal = -1; }

                try
                {
                    location.makeHoeDirt(new Vector2(x, y), true);
                }
                catch
                {

                }

                var seed = new StardewValley.Object(index, 1);
                bot.inventory.Add(seed);
                bot.getFarmer.ActiveObject = seed;

                HoeDirt feature = (HoeDirt)location.GetTerrainFeature(new Vector2(x, y));
                if (feature.crop != null)
                {
                    feature.destroyCrop(new Vector2(x, y), false, location);
                }

                //ModUtils.placementAction(location, x, y, seed, bot.getFarmer);
                seed.placementAction(location, x * 64, y * 64, bot.getFarmer);

                if (location.isTileHoeDirt(new Vector2(x, y)) && harvest > 0)
                {
                    feature.crop.growCompletely();
                }
                if (location.isTileHoeDirt(new Vector2(x, y)) && water > 0)
                {
                    feature.performToolAction(new WateringCan(), 0, new Vector2(x, y), location);
                }

                switch (goal)
                {
                    case 0:
                        cropsLocations.Add("Water", new Vector2(x, y));
                        break;
                    default:
                        cropsLocations.Add("Harvest", new Vector2(x, y));
                        break;
                }



                //else
                //{
                //    var message = new JObject();
                //    message.Add("Error", "Didn't Plant.");ZZ
                //    ModEntry.logger.Log(message, action: "Level.Error");
                //}

            }
        }

        public async static void levelCleared()
        {
            ModEntry.logger.Log("You cleared the level!");
            Game1.drawObjectDialogue("You cleared the level!");
            await Task.Delay(4000);



            /*CurrentLevel.currentSublevel.bot = null;*/

            if (CurrentLevel.currentSublevel.next != null)
            {
                cleanLevel();
                CurrentLevel.currentSublevel.isCompleted = true;
                CurrentLevel.currentSublevel = CurrentLevel.currentSublevel.next;
                LoadNextLevel();
            }
            else
            {

                CurrentLevel.isCompleted = true;
                var level = CurrentLevel;
                leaveLevel();
                level.currentSublevel = level.currentSublevel.prev;

            }
        }

        public static void levelExit()
        {
            cleanLevel();
            leaveLevel();
        }

        public static void leaveLevel()
        {
            ModEntry.Dispatcher.EndTutorial();
            cleanLevel();
            CurrentLevel = null;
            Game1.warpFarmer("Farm", 64, 15, flip: false);
            foreach (var bot in Bot.instances)
            {
                if (bot.CurrentLocation == Game1.getLocationFromName("Farm"))
                {
                    bot.SwitchTo();
                    break;
                }
            }
            mapOverlayToggle();
        }

        private static void mapOverlayToggle()
        {
            if (Game1.onScreenMenus.Contains(ModEntry.options))
            {
                Game1.onScreenMenus.Remove(ModEntry.options);
            }
            else
            {
                Game1.onScreenMenus.Add(ModEntry.options);
            }
        }

        private static void cleanLevel()
        {
            var message = new JObject();
            message.Add("Level Name", CurrentLevel.MapName);
            message.Add("Sublevel Name", CurrentLevel.currentSublevel.MapName);
            message.Add("Sublevel Completed?", CurrentLevel.currentSublevel.isCompleted ? "Yes" : "No");
            message.Add("Times Code Run", CurrentLevel.currentSublevel.timesRan);

            ModEntry.logger.Log(message, "Tutorial.End");

            CurrentLevel.currentSublevel.saveBotXML();

            CurrentLevel.currentSublevel.objectLocations.Clear();
            CurrentLevel.currentSublevel.LevelLocation.terrainFeatures.Clear();
            CurrentLevel.currentSublevel.DestroyBot();
        }

        public static void OnTesting(BotController botController)
        {
            if (CurrentLevel == null) return;
            if (CurrentLevel.currentSublevel == null) return;
            if (CurrentLevel.currentSublevel.bot != botController.Bot) return;
            resetLevel();
        }

        public static void resetLevel()
        {
            //var sublevel = Level.CurrentLevel.currentSublevel;
            //ModEntry.Dispatcher.LoadTutorial(Level.CurrentLevel.MapName + " " + sublevel.type, sublevel.EntryMessage, sublevel.explanationText);
            CurrentLevel.currentSublevel.ResetBot();

        }

        protected void DestroyBot()
        {
            //DelayedAction.functionAfterDelay(() => { bot.DestroyBot(LevelLocation); this.bot = null; }, 1);
        }

        protected void ResetBot()
        {
            ModEntry.logger.Log("Bot Reset: " + botStart);

            var placementTile = new Vector2(botStart.X, botStart.Y);
            if (bot == null)
            {
                bot = new Bot(ID, placementTile, LevelLocation);
                bot.shakeTimer = 50;
            }
            else
            {
                bot.MoveInstantlyToTile(placementTile);
            }
            LevelLocation.overlayObjects[placementTile] = bot;
            bot.FacingDirection = startFacingDir;

            AddObjects();
            cropsLocations.Clear();
            
            plantCrops();
        }

        protected void AddObjects()
        {
            LevelLocation.loadWeeds();
            //var layer = levelMap.GetLayer("Paths");
            //if (layer == null) return;

            //for (int x = 0; x < layer.TileWidth; x++)
            //{
            //    for (int y = 0; y < layer.TileHeight; y++)
            //    {
            //        var t = layer.Tiles[y, x];
            //        if (t == null) continue;

            //        switch (t.TileIndex)
            //        {
            //            case 13:
            //            case 14:
            //            case 15:
            //                if (FieldLocation.CanLoadPathObjectHere(t))
            //                {
            //                    FieldLocation.objects.Add(t, new Object(t, FieldLocation.getWeedForSeason(Game1.random, FieldLocation.GetSeasonForLocation()), 1));
            //                }
            //                break;
            //            case 16:
            //                if (FieldLocation.CanLoadPathObjectHere(t))
            //                {
            //                    FieldLocation.objects.Add(t, new Object(t, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
            //                }
            //                break;
            //            case 17:
            //                if (FieldLocation.CanLoadPathObjectHere(t))
            //                {
            //                    FieldLocation.objects.Add(t, new Object(t, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
            //                }
            //                break;
            //            case 18:
            //                if (CanLoadPathObjectHere(t))
            //                {
            //                    FieldLocation.objects.Add(t, new Object(t, (Game1.random.NextDouble() < 0.5) ? 294 : 295, 1));
            //                }
            //                break;
            //        }
            //    }
            //}            
        }

        private static Point? FindTilePointWithProperty(Layer layer, string property)
        {
            for (int x = 0; x < layer.TileWidth; x++)
            {
                for (int y = 0; y < layer.TileHeight; y++)
                {
                    var tile = layer.Tiles[x, y];
                    if (tile == null) continue;
                    tile.Properties.TryGetValue(property, out var value);
                    if (value != null) return new Point(x, y);
                }
            }
            return null;
        }

        private static List<Point?> FindTilePointsWithProperty(Layer layer, string property)
        {
            List<Point?> tiles = new List<Point?>();
            for (int x = 0; x < layer.TileWidth; x++)
            {
                for (int y = 0; y < layer.TileHeight; y++)
                {
                    var tile = layer.Tiles[x, y];
                    if (tile == null) continue;
                    tile.Properties.TryGetValue(property, out var value);
                    if (value != null) if (value != null) tiles.Add(new Point(x, y));
                }
            }
            return tiles;
        }

        private static Tile FindTileWithProperty(Layer layer, string property)
        {
            var point = FindTilePointWithProperty(layer, property);
            if (point == null) return null;
            return layer.Tiles[point.Value.X, point.Value.Y];
        }

        private static List<Tile> FindTilesWithProperty(Layer layer, string property)
        {
            List<Tile> tiles = new List<Tile>();
            for (int x = 0; x < layer.TileWidth; x++)
            {
                for (int y = 0; y < layer.TileHeight; y++)
                {
                    var tile = layer.Tiles[x, y];
                    if (tile == null) continue;
                    tile.Properties.TryGetValue(property, out var value);
                    if (value != null) if (value != null) tiles.Add(tile);
                }
            }
            return tiles;
        }

        public abstract class Sublevel : Level
        {
            public Sublevel next;
            public Sublevel prev;
            public List<String> explanationText;
            public abstract String type { get; }
            public int timesRan;

            public Sublevel()
            {
                next = null;
                prev = null;
                levelConditions = new Conditions();
                explanationText = new List<string>();
                timesOpened = 0;
                timesRan = 0;

            }

            public void CodeRun()
            {
                timesRan++;
            }

            public void addLast(Sublevel next)
            {
                Sublevel current = this;

                while (current.next != null)
                {
                    current = current.next;
                }

                next.prev = current;
                current.next = next;
            }

            public abstract Conditions levelConditions
            {
                get; set;
            }

            public abstract Conditions getLevelConditions();
            public abstract void setUpConditions();

            internal BlocklyBridge.Program loadBotXML(IModHelper helper)
            {
                /*using (StreamReader file = File.OpenText("Levels/xml/CamExample.json"));
                using (JsonTextReader reader = new JsonTextReader(file));*/
                //IDictionary<string, string> data;

                ProgramState trialData;

                try
                {
                    //data = helper.ModContent.Load<Dictionary<string, string>>(xmlCodeRoot + MapName + ".json");
                    //if (data == null) return null;
                    trialData = helper.Data.ReadJsonFile<ProgramState>(xmlCodeRoot + "LevelPresets.json");
                    return trialData.Programs.Where(x => x.Guid == MapName).FirstOrDefault();
                }
                catch
                {
                    return null;
                }

                //JObject obj = new JObject();

                //foreach (var entry in data)
                //{
                //    obj.Add(entry.Key, entry.Value);
                //}

                
            }

            internal void saveBotXML()
            {
                var prog = ModEntry.Dispatcher.State.GetProgram(this.MapName);
                var presets = ModEntry.helper.Data.ReadJsonFile<ProgramState>(xmlCodeRoot + "LevelPresets.json");
                var saveprog = presets.GetProgram(prog.Guid);

                saveprog.Code = prog.Code;
                saveprog.VarMap = prog.VarMap;

                ModEntry.helper.Data.WriteJsonFile(xmlCodeRoot + "LevelPresets.json", presets);
            }
        }
    }
}
