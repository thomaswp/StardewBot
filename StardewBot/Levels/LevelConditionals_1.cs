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
            public override string type => "Example";
            public override Conditions levelConditions { get; set; }
            public override string MapName => "LevelConditionals_1Example";
            public override string EntryMessage => "In this example level, open the code editor and examine the code. Make the indicate change and run the code to move on.";

            public override void setUpConditions()
            {
                explanationText.Add("In this tutorial, we will learn more about conditional logic.");
                explanationText.Add("Conditional Logic is based on if a condition is met. For example, *If* there is a crop that is not watered, *then* water it. Or *Else* you do nothing and move on.");
                explanationText.Add("You have been given code that uses Sensing category blocks to determine what is IN FRONT of the bot. You objective is to get the bot to the grassy patch, and collect all of the wood and stone.");
                levelConditions = new Conditions();
                levelConditions.AddLocationCondition(botGoal);
                levelConditions.AddResourceCondition();
            }

            public override Conditions getLevelConditions()
            {
                return levelConditions;
            }
        }


        public class LevelConditional_1Actual : Sublevel
        {
            public override string type => "Actual";
            public override Conditions levelConditions { get; set; }
            public override string MapName => "LevelConditionals_1Actual";
            public override string EntryMessage => "In this level, your goal is to have the bot clear the resources and reach the grassy location";

            public override void setUpConditions()
            {
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
