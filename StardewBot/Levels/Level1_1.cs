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
    public class Level1_1 : Level
    {
        public override string MapName => "Level1_1";
        

        public Level1_1()
        {
            this.currentSublevel = new Level1_1Example();
            this.currentSublevel.addLast(new Level1_1Actual());
        }

        public class Level1_1Example : Sublevel {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "Level1_1Example";

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

        public class Level1_1Actual : Sublevel
        {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "Level1_1Actual";

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
    }
}
