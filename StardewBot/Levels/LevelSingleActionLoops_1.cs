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
    public class LevelSingleActionLoops_1 : Level
    {
        public override string MapName => "Single-Action Loops";
        public override string EntryMessage => "Example Message";


        public LevelSingleActionLoops_1()
        {
            this.currentSublevel = new LevelSingleActionLoops_1Example();
            this.currentSublevel.addLast(new LevelSingleActionLoops_1Actual());
        }

        public class LevelSingleActionLoops_1Example : Sublevel {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelSingleActionLoops_1Example";
            public override string EntryMessage => "In this example level, open the code editor and examine the code. Make the indicate change and run the code to move on.";


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

        public class LevelSingleActionLoops_1Actual : Sublevel
        {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelSingleActionLoops_1Actual";
            public override string EntryMessage => "In this level, your goal is to reach the grassy location";

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
