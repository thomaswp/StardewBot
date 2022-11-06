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
    public class LevelConditionals_3 : Level
    {
        public override string MapName => "Conditionals 3";

        public LevelConditionals_3()
        {
            this.currentSublevel = new LevelConditionals_3Example();
            this.currentSublevel.addLast(new LevelConditionals_3Actual());
        }

        public class LevelConditionals_3Example : Sublevel
        {
            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelConditionals_3Example";

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


        public class LevelConditionals_3Actual : Sublevel
        {
            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelConditionals_3Actual";

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
