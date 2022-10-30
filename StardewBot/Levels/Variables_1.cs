using BlocklyBridge;
using Farmtronics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
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
    public class Variables_1 : Level
    {
        public override string MapName => "Variables";
        

        public Variables_1()
        {
            this.currentSublevel = new Variables_1Example();
            this.currentSublevel.addLast(new Variables_1Actual());
        }

        public class Variables_1Example : Sublevel {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "Variables_1Example";

            public override void setUpConditions()
            {
                levelConditions = new Conditions();
                levelConditions.AddLocationCondition(botGoal);
                /*levelConditions.addLocation(botGoal);*/
            }

            public override Conditions getLevelConditions()
            {
                return levelConditions;
            }
        }

        public class Variables_1Actual : Sublevel
        {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "Variables_1Actual";

            public override void setUpConditions()
            {
                levelConditions = new Conditions();
                levelConditions.AddInventorySpaceCondition(LevelLocation);
                /*levelConditions.addLocation(botGoal);*/
            }

            public override Conditions getLevelConditions()
            {
                return levelConditions;
            }
        }
    }
}
