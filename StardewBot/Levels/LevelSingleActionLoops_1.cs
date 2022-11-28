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
            public override string type => "Example";
            public override Conditions levelConditions { get; set; }
            public override string MapName => "LevelSingleActionLoops_1Example";
            public override string EntryMessage => "In this example level, open the code editor and examine the code. Make the indicate change and run the code to move on.";


            public override void setUpConditions()
            {
                explanationText.Add("In this level, we will be focusing on Loops! We will start by using a loop to get your bot to do a single action over and over again.");
                explanationText.Add("We will start by having a loop repeat movement multiple times. Inside of the loops category, you will be able to find numerous blocks to repeat actions.");
                explanationText.Add("The objective of this level is to change number of times the bot moves, in order to get it to land on the grassy patch.");

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
            public override string type => "Actual";
            public override Conditions levelConditions { get; set; }
            public override string MapName => "LevelSingleActionLoops_1Actual";
            public override string EntryMessage => "In this level, your goal is to reach the grassy location";

            public override void setUpConditions()
            {
                explanationText.Add("Good Job! The next step is applying what you've seen in the last level. Your objective is to land on the grassy patch on the level. Try using two loops to accomplish this task.");

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
