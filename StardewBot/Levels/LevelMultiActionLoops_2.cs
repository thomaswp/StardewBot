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
            public override string type => "Example";
            public override Conditions levelConditions { get; set; }
            public override string MapName => "LevelMultiActionLoops_2Example";
            public override string EntryMessage => "In this example level, open the code editor and examine the code. Make the indicate change and run the code to move on.";

            public override void setUpConditions()
            {
                explanationText.Add("In this tutorial, we will continue focussing on Loops with multiple actions inside.");
                explanationText.Add("This level is similar to the prior tutorial, but includes additional movement to get all of the resource blocks.");
                explanationText.Add("The objective of this level is to gather all of the wood and stone, and make it to the grassy spot.");

                levelConditions = new Conditions();
                levelConditions.AddLocationCondition(botGoal);
                levelConditions.AddResourceCondition();
            }

            public override Conditions getLevelConditions()
            {
                return levelConditions;
            }
        }


        public class LevelMultiActionLoops_2Actual : Sublevel
        {
            public override string type => "Actual";
            public override Conditions levelConditions { get; set; }
            public override string MapName => "LevelMultiActionLoops_2Actual";
            public override string EntryMessage => "In this level, your goal is to reach the grassy location";

            public override void setUpConditions()
            {

                explanationText.Add("Good Job! This next level is a little more involved that the prior examples.");
                explanationText.Add("So far, a function with a loop inside has been provided for you. A function is a block of code that can be called over and over again when needed. This way, you won't need to have many duplicate blocks.");
                explanationText.Add("Inside of the current function is a loop that you should use to go through a single line of the square. The function is called four times, so this loop will happen four times.");
                explanationText.Add("The objective of this level is to clear out all of the glass blocks on this level, and return the bot to its starting position.");


                levelConditions = new Conditions();
                levelConditions.AddLocationCondition(botGoal);
                levelConditions.AddResourceCondition();
            }

            public override Conditions getLevelConditions()
            {
                return levelConditions;
            }
        }
    }
}
