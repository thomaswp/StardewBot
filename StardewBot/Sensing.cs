﻿using BlocklyBridge;
using Farmtronics;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot
{
    [ScriptableBehavior("Sensing", 160)]
    public class Sensing : ActionCategory
    {

        //enum TileProperty
        //{
        //    Diggable,
        //    Water,

        //}


        private static Dictionary<int, string> ClumpMap = new Dictionary<int, string>();
        private const string WOOD = "wood";
        private const string STONE = "stone";
        static Sensing()
        {
            ClumpMap.Add(ResourceClump.hollowLogIndex, WOOD);
            ClumpMap.Add(ResourceClump.stumpIndex, WOOD);
            ClumpMap.Add(ResourceClump.mineRock1Index, STONE);
            ClumpMap.Add(ResourceClump.mineRock2Index, STONE);
            ClumpMap.Add(ResourceClump.mineRock3Index, STONE);
            ClumpMap.Add(ResourceClump.mineRock4Index, STONE);
            ClumpMap.Add(ResourceClump.boulderIndex, STONE);
        }

        public Sensing(BotController controller) : base(controller)
        {

        }

        //private bool IsPassable(Vector2 tile)
        //{
        //    // TODO: This doesn't work with grass - need to see how Farmer does it
        //    // Probably need to use isCollidingPosition
        //    return Location.isTileLocationTotallyClearAndPlaceable(tile);
        //}

        [ScriptableMethod]
        public AsyncFunction<bool> IsForwardClear()
        {
            return Result(Bot.IsFacingClear);
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsForwardWater()
        {
            return Result(Bot.IsFacingClear && Location.IsTileWater(Bot.FacingTile));
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsForwardDiggable()
        {
            return Result(Bot.IsFacingClear && Location.DoesTileHaveProperty("Diggable", Bot.FacingTile));
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsForwardWood()
        {
            return Result(IsWood(Bot.FacingTile));
        }

        private bool IsWood(Vector2 tile)
        {
            var feature = Location.GetTerrainFeature(tile);
            if (feature is Tree || feature is GiantCrop || feature is Bush) return true;
            return IsClumpType(tile, WOOD);
        }

        private bool IsClumpType(Vector2 tile, string type)
        {
            var clumps = Location.GetResourceClumps(tile);
            return clumps.Any(
                c => ClumpMap.TryGetValue(c.parentSheetIndex.Value, out string t) &&
                t == type
            );
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsForwardStone()
        {
            return Result(IsStone(Bot.FacingTile));
        }

        private bool IsStone(Vector2 tile)
        {
            //var feature = Location.GetTerrainFeature(tile);
            // TODO: This doesn't work :(
            return IsClumpType(tile, STONE);
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsForwardGrass()
        {
            return Result(IsGrass(Bot.FacingTile));
        }

        private bool IsGrass(Vector2 tile)
        {
            var feature = Location.GetTerrainFeature(tile);
            if (feature is Grass) return true;
            if (feature is HoeDirt)
            {
                var crop = (feature as HoeDirt).crop;
                if (crop != null && crop.dead.Value) return true;
            }
            return false;
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsForwardCrop()
        {
            return Result(Bot.IsFacingClear && Location.IsTileACrop(Bot.FacingTile));
        }
    }
}
