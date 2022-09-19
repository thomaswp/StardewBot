using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot.Levels
{
    public class Ifs3 : Level
    {
        public override string MapName => "ifs-3-types";

        protected override Conditions levelConditions { get; set; }

        public Ifs3()
        {
            levelConditions = new Conditions();
        }
        public override void setUpConditions()
        {
            levelConditions = new Conditions();
            levelConditions.addLocationCondition(botGoal);
        }

        public override Conditions getLevelConditions()
        {
            return levelConditions;
        }

        
    }
}
