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
        public Farming(BotController controller) : base(controller)
        { }

        [ScriptableMethod]
        public AsyncMethod Harvest()
        {
            //return DoUntilFinished(() => Bot.UseTool(tool.ToString().Replace("_", " ")));

            return DoInstantly(() => Bot.Harvest()); 
        }

        [ScriptableMethod]
        public AsyncFunction<bool> IsTillable()
        {
            return Result(Bot.CurrentLocation.DoesTileHaveProperty("Diggable", Bot.TileLocation));
           /* return Result(Location.isTileHoeDirt(Bot.FacingTile) && Bot.IsFacingClear);*/
        }

        [ScriptableMethod]
        public AsyncMethod TillUnderneath()
        {
            for (int i = 0; i < Bot.inventory.Count; i++)
            {
                Tool tool = Bot.inventory[i] as Tool;
                if (tool == null) continue;
                if (tool.BaseName == "Hoe")
                {
                    
                    Bot.CurrentToolIndex = i;

                    return DoUntilFinished(() => {
                        /*tool.beginUsing(Bot.CurrentLocation, (int)Bot.getPosition.X, (int)Bot.getPosition.Y, Bot.getFarmer);*/
                        tool.DoFunction(Bot.CurrentLocation, (int)Bot.getPosition.X, (int)Bot.getPosition.Y, (int)((Game1.toolHold + 20f) / 600f) + 1, Bot.getFarmer);
                        /*ModUtils.beginUsing(Bot.CurrentLocation, (int)Bot.getPosition.X - 64 * Bot.DxForDirection(Bot.getFarmer.FacingDirection), (int)Bot.getPosition.Y - 64 * Bot.DyForDirection(Bot.getFarmer.FacingDirection), Bot.getFarmer, tool);*/

                        Bot.getFarmer.setTileLocation(Bot.TileLocation);
                       /* Farmer.showToolSwipeEffect(Bot.getFarmer);

                        // Count how many frames into the swipe effect we are.
                        // We'll actually apply the tool effect later, in Update.
                        Bot.toolUseFrame = 1;*/
                    });
                }
            }
            return null;
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
                return DoInstantly(() => Bot.PlaceItem((int)Bot.getPosition.X, (int)Bot.getPosition.Y));
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
        public AsyncFunction<int> InventoryItems()
        {
            return Result(Bot.inventory.Count);
        }
    }
}
