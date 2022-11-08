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
    public class LevelConditional_1 : Level
    {
        public override string MapName => "Conditionals 1";
        public override string EntryMessage => "Example Message";

        public LevelConditional_1()
        {
            this.currentSublevel = new LevelConditional_1Example();
            this.currentSublevel.addLast(new LevelConditional_1Actual());
        }

        public class LevelConditional_1Example : Sublevel
        {
            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelConditional_1Example";
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


        public class LevelConditional_1Actual : Sublevel
        {
            public override Conditions levelConditions { get; set; }

            public override string MapName => "LevelConditional_1Actual";
            public override string EntryMessage => "In this level, your goal is to have the bot clear the resources and reach the grassy location";

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
