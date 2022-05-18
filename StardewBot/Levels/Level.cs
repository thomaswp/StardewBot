using BlocklyBridge;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Layers;

namespace StardewBot.Levels
{
    public abstract class Level
    {
        public abstract string MapName { get; }
        public virtual string ID { get { return MapName; } }

        public const string FieldLocationName = "PracticeField";
        private const string MapPathRoot = "assets/maps/";
        private const string FieldMapPath = MapPathRoot + "field.tmx";

        protected GameLocation levelLocation;

        public static GameLocation FieldLocation;

        protected void LoadLocation(IModHelper helper)
        {
            string mapAssetKey = helper.ModContent.GetInternalAssetName(MapPathRoot + MapName + ".tmx").BaseName;
            levelLocation = new GameLocation(mapAssetKey, "temp_" + MapName) { IsOutdoors = true, IsFarm = false };
        }

        protected void Setup()
        {
            var entry = FieldLocation.GetMapPropertyPosition("Entry", 30, 30);

            foreach (var layer in levelLocation.Map.Layers)
            {
                Logger.Log("Layer: " + layer.Id);
                for (int col = 0; col < layer.TileWidth; col++)
                {
                    for (int row = 0; row < layer.TileHeight; row++)
                    {
                        var tile = layer.Tiles[col, row];
                        if (tile == null) continue;
                        Logger.Log($"({col},{row}): {tile.Id}, {tile.TileIndex}");
                    }
                }
            }
        }

        static Dictionary<string, Level> levelMap = new();
        static Level()
        {
            foreach (var level in new Level[]
            {
                new Loops1(),
            })
            {
                levelMap.Add(level.ID, level);
            }
        }

        public static void LoadAssets(IModHelper helper)
        {
            // get the internal asset key for the map file
            string mapAssetKey = helper.ModContent.GetInternalAssetName(FieldMapPath).BaseName;

            // add the location
            FieldLocation = new GameLocation(mapAssetKey, FieldLocationName) { IsOutdoors = true, IsFarm = false };
            Game1.locations.Add(FieldLocation);

            foreach (var level in levelMap.Values)
            {
                level.LoadLocation(helper);
            }
        }

        public static bool LoadLevel(string id)
        {
            if (!levelMap.TryGetValue(id, out var level)) return false;

            level.Setup();
            TeleportToField();
            return true;
        }

        private static void TeleportToField()
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
            };

            // Delayed action to be executed after a set time (here 0,1 seconds)
            // Teleporting without the delay may prove to be problematic
            DelayedAction.functionAfterDelay(TeleportFunction, 100);
        }
    }
}
