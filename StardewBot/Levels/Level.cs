using BlocklyBridge;
using Farmtronics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
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
        public virtual string ID { get { return MapName; } }

        public const string FieldLocationName = "PracticeField";
        private const string MapPathRoot = "assets/maps/";
        private const string xmlCodeRoot = "assets/xml/";
        private const string FieldMapPath = MapPathRoot + "field.tmx";
        private const string OriginFieldMapPath = MapPathRoot + "originField.tmx";

        protected Map levelMap;
        protected Point botStart;
        protected Point levelOffset;
        protected static Point? botGoal;
        protected static List<Point?> botGoals;
        protected int startFacingDir;
        protected Bot bot;

        public static GameLocation OriginLocation { get; private set; }
        public GameLocation LevelLocation { get; private set; }
        public static Level CurrentLevel { get; private set; }

        public static Dictionary<string, Level> levelsDict = new();
        public static List<Level> GetLevels
        {
            get
            {
                List<Level> holder = new List<Level>();
                foreach(var level in levelsDict.Values)
                {
                    holder.Add(level);
                }
                return holder;
            }
        }

        public Sublevel currentSublevel;


        static Level()
        {
            foreach (var level in new Level[]
            {
                //This needs to be changed! The map name is set to the last level initilized
                new Loops1(),
                new CamLevel(),
                new Ifs3(),
                new Level1_1(),
                new LevelMultiActionLoops_1()
                
            })
            {
                levelsDict.Add(level.ID, level);
            }
        }

        internal static void LoadTutorialCode(ProgramState state, IModHelper helper)
        {
            foreach (var level in levelsDict.Values)
            {
                Sublevel holder = level.currentSublevel;
                if (holder != null)
                {
                    var botXML = holder.loadBotXML(helper);
                    if (botXML != null)
                    {
                        var program = state.GetProgram((String)botXML["Guid"]);
                        program.Code = (String)botXML["Code"];
                        program.VarMap = (String)botXML["VarMap"];

                        helper.Data.WriteJsonFile(ModEntry.PROGRAMS_FILE, state);
                    }
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
            return true;
        }

        

        protected void Setup()
        {
            var entry = LevelLocation.GetMapPropertyPosition("Entry", 30, 30); 

            Layer back = levelMap.GetLayer("Back");
            var anchor = FindTilePointWithProperty(back, "Anchor");
            var start = FindTilePointWithProperty(back, "BotStart");
            var goal = FindTilePointWithProperty(back, "BotGoal"); 
            /*var goals = FindTilePointsWithProperty(back, "BotGoal");*/
            
            Logger.Log(anchor);
            Logger.Log(start);
            Logger.Log(goal); 
            /*Logger.Log(goals);*/

            if (anchor == null)
            {
                Logger.Warn("No anchor for map: " + ID);
                return;
            }

            if (start == null)
            {
                Logger.Warn("No anchor for map: " + ID);
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
            Logger.Log("Offset: " + offset);

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
                    Logger.Warn("Missing layer: " + layer.Id);
                    continue;
                }

                var baseTile = layer.Id == "Paths" ? basePathsTile : baseSpringTile;

                //Logger.Log("Layer: " + layer.Id);
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
                    Logger.Warn("Missing layer: " + layer.Id);
                    continue;
                }

                var baseTile = layer.Id == "Paths" ? basePathsTile : baseSpringTile;

                //Logger.Log("Layer: " + layer.Id);
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

                        fieldLayer.Tiles[y + offset.X, x + offset.Y] = newTile;
                    }
                }
            }
        }

        private void TeleportToStart()
        {
            GameLocation location = LevelLocation;

            // of creating a function here, an existing function can be used, e.g. if the same function is used multiple times
            DelayedAction.delayedBehavior TeleportFunction = () => {
                //Insert here the coordinates you want to teleport to
                Layer layer = location.map.GetLayer("Back");
                Point entry = location.GetMapPropertyPosition("Entry", 30, 30);
                int dir = (int)Direction.Right;

                LevelLocation.resetForPlayerEntry();

                //The teleport command itself
                Game1.warpFarmer(new LocationRequest(location.NameOrUniqueName, location.uniqueName.Value != null, location), entry.X, entry.Y, dir);

                if (this.bot != null)
                {
                    bot.SwitchTo();
                }

                DelayedAction.functionAfterDelay(() => ResetBot(), 1);
            };

            // Delayed action to be executed after a set time (here 0,1 seconds)
            // Teleporting without the delay may prove to be problematic
            DelayedAction.functionAfterDelay(TeleportFunction, 100);
        }

        public async static void levelCleared()
        {
            Logger.Log("You cleared the level!");
            Game1.drawObjectDialogue("You cleared the level!");
            await Task.Delay(4000);

            CurrentLevel.currentSublevel.DestroyBot();
            /*CurrentLevel.currentSublevel.bot = null;*/

            if (CurrentLevel.currentSublevel.next != null)
            {
                CurrentLevel.currentSublevel = CurrentLevel.currentSublevel.next;
                LoadNextLevel();
            } else
            {
                CurrentLevel = null;
                Game1.warpFarmer("Farm", 64, 15, flip: false);
            }
        }

        public static void OnTesting(BotController botController)
        {
            if (CurrentLevel == null) return;
            if (CurrentLevel.currentSublevel == null) return;
            if (CurrentLevel.currentSublevel.bot != botController.Bot) return;
            CurrentLevel.currentSublevel.ResetBot();
        }

        protected void DestroyBot()
        {
            if(bot != null)
            {
                DelayedAction.functionAfterDelay(() => bot.DestroyBot(LevelLocation), 1);
            }
        }

        protected void ResetBot()
        {
            Logger.Log("Bot Reset: " + botStart);

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

        public abstract class Sublevel : Level
        {
            public Sublevel next;

            public Sublevel()
            {
                next = null;
                levelConditions = new Conditions();
            }

            public void addLast(Sublevel next)
            {
                Sublevel current = this;

                while(current.next != null)
                {
                    current = current.next;
                }

                current.next = next;
            }

            public abstract Conditions levelConditions
            {
                get; set;
            }

            public abstract Conditions getLevelConditions();
            public abstract void setUpConditions();

            internal JObject loadBotXML(IModHelper helper)
            {
                /*using (StreamReader file = File.OpenText("Levels/xml/CamExample.json"));
                using (JsonTextReader reader = new JsonTextReader(file));*/
                IDictionary<string, string> data;

                try
                {
                    data = helper.ModContent.Load<Dictionary<string, string>>(xmlCodeRoot + MapName + ".json");
                    if (data == null) return null;
                } catch
                {
                    return null;
                }

                JObject obj = new JObject();

                foreach (var entry in data)
                {
                    obj.Add(entry.Key, entry.Value);
                }

                return obj;
            }
        }
    }
}
