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
    public class Functions_1 : Level
    {
        public override string MapName => "Functions";
        public override string EntryMessage => "Example Message";


        public Functions_1()
        {
            this.currentSublevel = new Functions_1Example();
            this.currentSublevel.addLast(new Functions_1Actual());
        }

        public class Functions_1Example : Sublevel {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "Functions_1Example";
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

        public class Functions_1Actual : Sublevel
        {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "Functions_1Actual";
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
