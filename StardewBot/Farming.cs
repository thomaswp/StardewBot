using BlocklyBridge;
using Farmtronics;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;

namespace StardewBot
{

    [ScriptableBehavior("Farming", 180)]
    public class Farming : ActionCategory
    {
        public enum ToolType
        {
            Hoe,
            Axe,
            Pickaxe,
            Watering_Can,
            Scythe,
        }

        public Farming(BotController controller) : base(controller)
        { }

        [ScriptableMethod]
        public AsyncMethod UseTool(ToolType tool)
        {
            return DoUntilFinished(() => Bot.UseTool(tool.ToString().Replace("_", " ")));
        }

        [ScriptableMethod]
        public AsyncMethod Harvest()
        {
            //return DoUntilFinished(() => Bot.UseTool(tool.ToString().Replace("_", " ")));

            return DoInstantly(() => Bot.Harvest()); 
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsTillable()
        {
            return Result(Bot.CurrentLocation.DoesTileHaveProperty("Diggable", Bot.FacingTile));
           /* return Result(Location.isTileHoeDirt(Bot.FacingTile) && Bot.IsFacingClear);*/
        }

        [ScriptableMethod]
        public AsyncMethod DisplayPrompt(String message)
        {
            return DoInstantly(() => Game1.drawObjectDialogue(message));
        }



        //TODO Add lists to Blockly Parameter Generation 
        public IList<Item> inventorySeeds
        {
            get
            {
                IList<Item> seeds = new List<Item>();

                foreach (var item in Bot.inventory)
                {
                    if (item.Category.Equals(StardewValley.Object.SeedsCategory))
                    {
                        seeds.Add(item);
                    }
                }

                return seeds;
            }
        }

        [ScriptableMethod]
        public AsyncMethod plantSeed()
        {
            int seed = -1;

            var botInventory = Bot.inventory;

            for (int i = 0; i < botInventory.Count; i++)
            {
                if (botInventory[i] != null && botInventory[i].Category.Equals(StardewValley.Object.SeedsCategory))
                {
                    seed = i;
                    break;
                }
            }

            //foreach(var item in botInventory)
            //{
            //    if (item.Category.Equals(StardewValley.Object.SeedsCategory))
            //    {
            //        seed = item;
            //        break;
            //    }
            //} 

            if (seed >= 0)
            {
                Bot.CurrentToolIndex = seed;
                return DoInstantly(() => Bot.PlaceItem((int)Bot.FacingTile.X * 64, (int)Bot.FacingTile.Y * 64));
            }

            return null;
        }

        /**
         * Must be forward because bot can't go over crops such as grapes/on vines
         */
        [ScriptableMethod]
        public AsyncFunction<bool> IsCropForwardHandHarvestable()
        {
            int tileX = (int)Bot.FacingTile.X;
            int tileY = (int)Bot.FacingTile.Y;
            Vector2 v = new Vector2(tileX, tileY);
            if (Bot.CurrentLocation.terrainFeatures.ContainsKey(v) && Bot.CurrentLocation.terrainFeatures[v] is HoeDirt)
            {
                return Result(Location.isCropAtTile(tileX, tileY) && ((HoeDirt)Bot.CurrentLocation.terrainFeatures[v]).crop.harvestMethod == 0);
            }

            return Result(false);
            /* return Result(Location.isTileHoeDirt(Bot.FacingTile) && Bot.IsFacingClear);*/
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsCropFullyGrown()
        {
            int tileX = (int)Bot.FacingTile.X;
            int tileY = (int)Bot.FacingTile.Y;
            Vector2 v = new Vector2(tileX, tileY);
            if (Bot.CurrentLocation.terrainFeatures.ContainsKey(v) && Bot.CurrentLocation.terrainFeatures[v] is HoeDirt)
            {
                return Result(Location.isCropAtTile(tileX, tileY) && ((HoeDirt)Bot.CurrentLocation.terrainFeatures[v]).readyForHarvest());
            }

            return Result(false);
        }

        [ScriptableMethod]
        public AsyncFunction<int> NumberOfInventoryItems()
        {
            return Result(Bot.inventory.Count);
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsInventoryFull()
        {
            return Result(Bot.getFarmer.isInventoryFull());
        }
    }
}
