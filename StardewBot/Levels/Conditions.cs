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
    /**
     * Responsible for setting up win conditions for each level. 
     */
    public class Conditions
    {
        /*
         * Event if a condition is met. If a condition is met, then the program checks
         * to see if all of the conditions are met.
         */
        //TODO Evaluate if this is neccessary.
        public event EventHandler singleConditionMet;
        public EventHandler singleConditionHandler;

        //Movement-Based Event and Conditions
        public event EventHandler<MovedArgs> botMoved;
        public EventHandler<MovedArgs> moveHandler;
        public Point? locationGoal;

        //Keeps track of all current conditions
        private List<ConditionInformation> conditionss;

        /**
         * <summary>         
         * Property to see if all of the conditions are met.
         * </summary>
         */
        public bool allConditionsMet
        {
            get
            {
                if (conditionss.Count == 0) return false;
                foreach(var condition in conditionss)
                {
                    if (condition.isMet == false) return false;
                }
                return true;
            }
        }

        /**
         * <summary>
         * Defines what level conditions are available to use.
         * </summary>
         */
        public enum conditionType
        {
            ReachedLocation, 
            HarvestedCrops,
            ItemsPlaced,
            TilesTiled
        }

        /**
         * <summary>
         * Sets up the Conditions class with a list of conditions.
         * This list can have different types of conditions added to it. 
         * Also initializes the singleConditionHandler so that every time a condition is met, 
         * it checks to see if all conditions have been met. 
         * </summary>
         * 
         */
        public Conditions()
        {
            conditionss = new List<ConditionInformation>();
            singleConditionHandler = singleConditionMet;
            singleConditionHandler += checkAllConditions;
            moveHandler = botMoved;
            moveHandler += checkLocation;
        }

        /**
         * <summary>
         * Checks to see if all of the conditions that were set in the level are met. If so, the
         * level ends.
         * </summary>
         * <param name="sender"> The object that made the call. </param>
         * <param name="e"> Arguments passed when the event is called. </param>
         */
        private void checkAllConditions(object sender, EventArgs e)
        {
            if (allConditionsMet)
            {
                Level.levelCleared();
            }
        }

        /**
         * <summary>
         * Adds a location condition based on a single location.  
         * </summary>
         * <param name="newLocation"> The location where the bot needs to reach. </param> 
         */
        public void AddLocationCondition(Point? newLocation)
        {
            locationCondition holder = null;
            if (newLocation != null)
            {
                holder = new locationCondition(conditionType.ReachedLocation, newLocation);
                conditionss.Add(holder);
            }
        }

        /**
         * <summary>
         * Checks to see if the location condition is being used. If so, it checks to see if the bots location
         * is the same as one of the goal locations. If so, then the program checks if all other conditions are met.
         * </summary>
         * <param name="sender"> The object that made the call. </param>
         * <param name="e"> Arguments passed when the event is called. </param>
         */
        private void checkLocation(object sender, MovedArgs e)
        {
            foreach(var condition in conditionss)
            {
                if(condition.type.Equals(conditionType.ReachedLocation)) {
                    locationCondition con = (locationCondition)condition;
                    con.botLocation = e.botLocation;
                    if(con.isMet)
                    {
                        singleConditionHandler.Invoke(sender, e);
                    }
                }
            }
        }

        /**
         * <summary>
         * Abstract class that defines what a condition must consist of. The condition must
         * have a type and a boolean for whether it has been met. 
         * </summary>
         */
        private abstract class ConditionInformation
        {
            public conditionType type { get; private set; }
            public abstract bool isMet { get; }

            /**
             * <param name="type"> The type of which the condition is. </param>
             */
            public ConditionInformation (conditionType type)
            {
                this.type = type;
            }
        }

        /**
         * <summary>
         * Specific condition for location based goals. 
         * </summary>
         */
        private class locationCondition : ConditionInformation
        {

            public Point? botLocation { get; set; }
            public override bool isMet
            {
                get
                {
                    if (botLocation != null && goal.Equals(botLocation)) return true;
                    return false;
                }
            }
            
            protected Point? goal { get; private set; }

            /**
             * <param name="goal"> The point which the bot needs to reach. </param>
             * <param name="type"> The type of which the condition is. </param>
             */
            public locationCondition(conditionType type, Point? goal) : base(type) {
                this.goal = goal;
                botLocation = null;
            }

            
        }
    }
}
