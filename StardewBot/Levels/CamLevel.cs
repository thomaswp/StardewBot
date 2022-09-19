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
    public class CamLevel : Level
    {
        public override string MapName => "CamLevel";

        protected override Conditions levelConditions { get; set; }

        //private Conditions levelConditions = new Conditions();


        public override void setUpConditions()
        {
            levelConditions = new Conditions();
            levelConditions.addLocationCondition(botGoal);
            /*levelConditions.addLocation(botGoal);*/
        }

        public override Conditions getLevelConditions()
        {
            return levelConditions;
        }
    }
}
