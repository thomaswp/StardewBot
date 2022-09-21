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
                        tool.beginUsing(Bot.CurrentLocation, (int)Bot.TileLocation.X, (int)Bot.TileLocation.Y, Bot.getFarmer);
                        Bot.getFarmer.setTileLocation(Bot.TileLocation);
                        Bot.toolUseFrame = 1;
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
                if (botInventory[i].Category.Equals(StardewValley.Object.SeedsCategory))
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
                return DoInstantly(() => Bot.PlaceItem());
            }

            return null;
        }

    }
}
