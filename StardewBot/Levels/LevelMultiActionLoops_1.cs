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
    public class LevelMultiActionLoops_1 : Level
    {
        public override string MapName => "Multi-Action Loops 1";

        public LevelMultiActionLoops_1()
        {
            this.currentSublevel = new LevelMultiActionLoops_1Example();
            this.currentSublevel.addLast(new LevelMultiActionLoops_1Actual());
        }

        public class LevelMultiActionLoops_1Example : Sublevel
        {
            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelMultiActionLoops_1Example";

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


        public class LevelMultiActionLoops_1Actual : Sublevel
        {
            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelMultiActionLoops_1Actual";

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
