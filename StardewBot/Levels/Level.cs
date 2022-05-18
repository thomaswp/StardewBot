using BlocklyBridge;
using Farmtronics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private const string FieldMapPath = MapPathRoot + "field.tmx";

        protected Map levelMap;
        protected Point botStart;
        protected Point levelOffset;
        protected Point? botGoal;
        protected int startFacingDir;
        protected Bot bot;

        public static GameLocation FieldLocation;
        static Dictionary<string, Level> levelsDict = new();

        protected void LoadLocation(IModHelper helper)
        {
            //string mapAssetKey = helper.ModContent.GetInternalAssetName(MapPathRoot + MapName + ".tmx").BaseName;
            levelMap = helper.ModContent.Load<Map>(MapPathRoot + MapName + ".tmx");

            //foreach (var tilesheet in FieldLocation.Map.TileSheets)
            //{
            //    levelLocation.Map.AddTileSheet(tilesheet);
            //}
        }

        protected void Setup()
        {
            var entry = FieldLocation.GetMapPropertyPosition("Entry", 30, 30);

            Layer back = levelMap.GetLayer("Back");
            var anchor = FindTilePointWithProperty(back, "Anchor");
            var start = FindTilePointWithProperty(back, "BotStart");
            var goal = FindTilePointWithProperty(back, "BotGoal");
            Logger.Log(anchor);
            Logger.Log(start);
            Logger.Log(goal);

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
            levelOffset = offset;
            Logger.Log("Offset: " + offset);

            string facingStr = back.Tiles[start.Value.X, start.Value.Y].Properties["Facing"];
            if (int.TryParse(facingStr, out int dir)) startFacingDir = dir;

            Tile baseSpringTile = FieldLocation.Map.GetLayer("Back").Tiles[0, 0];
            Tile basePathsTile = FieldLocation.Map.GetLayer("Paths").Tiles[0, 0];

            foreach (var layer in levelMap.Layers)
            {
                //if (layer.Id == "Paths") continue;

                var fieldLayer = FieldLocation.Map.GetLayer(layer.Id);
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

        protected void ResetBot()
        {
            Logger.Log("Bot Reset: " + botStart);

            var placementTile = new Vector2(botStart.X, botStart.Y);
            if (bot == null)
            {
                bot = new Bot(placementTile, FieldLocation);
                bot.shakeTimer = 50;
            }
            else
            {
                // Remove from former tile?
                //FieldLocation.overlayObjects[bot.] = bot;
            }
            FieldLocation.overlayObjects[placementTile] = bot;
            bot.FacingDirection = startFacingDir;

            AddObjects();
        }

        protected void AddObjects()
        {
            FieldLocation.loadWeeds();
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

        private static Tile FindTileWithProperty(Layer layer, string property)
        {
            var point = FindTilePointWithProperty(layer, property);
            if (point == null) return null;
            return layer.Tiles[point.Value.X, point.Value.Y];
        }

        static Level()
        {
            foreach (var level in new Level[]
            {
                new Loops1(),
            })
            {
                levelsDict.Add(level.ID, level);
            }
        }

        public static void LoadAssets(IModHelper helper)
        {
            // get the internal asset key for the map file
            string mapAssetKey = helper.ModContent.GetInternalAssetName(FieldMapPath).BaseName;

            // add the location
            FieldLocation = new GameLocation(mapAssetKey, FieldLocationName) { IsOutdoors = true, IsFarm = false };
            Game1.locations.Add(FieldLocation);

            foreach (var level in levelsDict.Values)
            {
                level.LoadLocation(helper);
            }
        }

        public static bool LoadLevel(string id)
        {
            if (!levelsDict.TryGetValue(id, out var level)) return false;

            level.Setup();
            level.TeleportToStart();
            return true;
        }

        private void TeleportToStart()
        {
            GameLocation location = FieldLocation;

            // of creating a function here, an existing function can be used, e.g. if the same function is used multiple times
            DelayedAction.delayedBehavior TeleportFunction = () => {
                //Insert here the coordinates you want to teleport to
                Layer layer = location.map.GetLayer("Back");
                Point entry = location.GetMapPropertyPosition("Entry", 30, 30);
                int dir = (int)Direction.Right;

                //The teleport command itself
                Game1.warpFarmer(new LocationRequest(location.NameOrUniqueName, location.uniqueName.Value != null, location), entry.X, entry.Y, dir);

                DelayedAction.functionAfterDelay(() => ResetBot(), 1);
            };

            // Delayed action to be executed after a set time (here 0,1 seconds)
            // Teleporting without the delay may prove to be problematic
            DelayedAction.functionAfterDelay(TeleportFunction, 100);
        }
    }
}
