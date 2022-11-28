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
    public class LevelBasics : Level
    {
        public override string MapName => "The Basics";
        public override string EntryMessage => "Example Message";

        public LevelBasics()
        {
            this.currentSublevel = new LevelBasicsExample();
            this.currentSublevel.addLast(new LevelBasicsActual());
        }

        public class LevelBasicsExample : Sublevel
        {
            public override string type => "Example";
            public override Conditions levelConditions { get; set; }
            public override string MapName => "LevelBasicsExample";
            public override string EntryMessage => "In this example level, examine the code blocks given and move your bot to the objective.";

            public override void setUpConditions()
            {
                explanationText.Add("Welcome to the Code Editor! This will be where you will edit your bot's code to get it to do whatever you'd like.");
                explanationText.Add("To add new block, click on any category in the toolbar, and drag out a block. To delete a block, drag it to the trash can, or back to the toolbar.");
                explanationText.Add("You will be exploring a lot of bot funcitonalities in future tutorials, but we will focus on Stardew specific blocks.");
                explanationText.Add("Events are blocks that will start your code. In order for any code to run, you will need to start with an event block. You can choose between when you click on your bot, when you hit the 'Test My Code' button, or when the day starts.");
                explanationText.Add("Movement blocks allow you to move your bot in any direction, as well as turning your robot.");
                explanationText.Add("Sensing blocks allow your bot to sense what is in front of it, whether that is a piece of wood, a rock, or grass. There are other sensing blocks available, so make sure to check them all out.");
                explanationText.Add("The objective of this tutorial is to get you started with creating code. Move your bot onto the patch of grass. (You can duplicate a block with right-click, then selecting Duplicate)");

                levelConditions = new Conditions();
                levelConditions.AddLocationCondition(botGoal); 
            }

            public override Conditions getLevelConditions()
            {
                return levelConditions;
            }
        }


        public class LevelBasicsActual : Sublevel
        {
            public override string type => "Actual";
            public override Conditions levelConditions { get; set; }
            public override string MapName => "LevelBasicsActual";
            public override string EntryMessage => "In this level, your goal is to have the bot clear the resources and reach the grassy location";

            public override void setUpConditions()
            {
                explanationText.Add("Good Job! The next set of blocks you have available to you are the Farming blocks.");
                explanationText.Add("Farming blocks allow your bot to do numerous farming related tasks, such as planting seeds, watering crops, and harvesting. There is also a handy function to display a message at the bottom of your screen. The text block can be found in the Text category.");
                explanationText.Add("The objective of this level is to harvest the crop in front of the bot. ");

                levelConditions = new Conditions();
                levelConditions.AddCropCondition();
            }

            public override Conditions getLevelConditions()
            {
                return levelConditions;
            }
        }
    }
}
