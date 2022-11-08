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
    public class CamLevel : Level
    {
        public override string MapName => "CamLevel";
        public override string EntryMessage => "Example Message";
        

        public CamLevel()
        {
            this.currentSublevel = new CamExample();
            this.currentSublevel.addLast(new CamActual());
        }

        public class CamExample : Sublevel {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "CamExample";
            public override string EntryMessage => "Example Message";

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

        public class CamActual : Sublevel
        {

            public override Conditions levelConditions { get; set; }

            public override string MapName => "CamActual";
            public override string EntryMessage => "Example Message";

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
