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
    public class LevelMultiActionLoops_2 : Level
    {
        public override string MapName => "Multi-Action Loops 2";
        public override string EntryMessage => "Example Message";

        public LevelMultiActionLoops_2()
        {
            this.currentSublevel = new LevelMultiActionLoops_2Example();
            this.currentSublevel.addLast(new LevelMultiActionLoops_2Actual());
        }

        public class LevelMultiActionLoops_2Example : Sublevel
        {
            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelMultiActionLoops_2Example";
            public override string EntryMessage => "In this example level, open the code editor and examine the code. Make the indicate change and run the code to move on.";

            public override void setUpConditions()
            {
                levelConditions = new Conditions();
                levelConditions.AddLocationCondition(botGoal); // 
            }

            public override Conditions getLevelConditions()
            {
                return levelConditions;
            }
        }


        public class LevelMultiActionLoops_2Actual : Sublevel
        {
            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelMultiActionLoops_2Actual";
            public override string EntryMessage => "In this level, your goal is to reach the grassy location";

            public override void setUpConditions()
            {
                levelConditions = new Conditions();
                levelConditions.AddLocationCondition(botGoal);
            }

            public override Conditions getLevelConditions()
            {
                return levelConditions;
            }
        }
    }
}
