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

    [ScriptableBehavior("Variables", 180)]
    public class Variables : ActionCategory
    {
        public Variables(BotController controller) : base(controller)
        { }

        [ScriptableMethod]
        public AsyncFunction<int> NumberOFInventoryItems()
        {
            return Result(Bot.inventory.Count);
        }

        public AsyncFunction<bool> IsInventoryFull()
        {
            return Result(Bot.getFarmer.isInventoryFull());
        }

    }
}
