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

        public event EventHandler FinishedAction;
        public EventHandler FinishedActionHandler;


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
            TilesTiled,
            InventorySpace
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
            FinishedActionHandler = FinishedAction;
            
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
         * Adds a inventory space condition based on a single location.  
         * </summary>
         */
        public void AddInventorySpaceCondition(GameLocation location)
        {
            inventorySpaceCondition holder = null;

            if (!isConditionInEvent("CheckInventorySpace"))
            {
                FinishedActionHandler += CheckInventorySpace;
            }

            holder = new inventorySpaceCondition(conditionType.InventorySpace, location);
            conditionss.Add(holder);
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

            if(!isConditionInEvent("checkLocation"))
            {
                FinishedActionHandler += checkLocation;
            }

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
        private void CheckInventorySpace(object sender, EventArgs e)
        {
            foreach (var condition in conditionss)
            {
                if (condition.type.Equals(conditionType.InventorySpace))
                {
                    inventorySpaceCondition con = (inventorySpaceCondition)condition;
                    Bot bot = (Bot)sender;
                    con.bot = bot;
                    if (con.isMet)
                    {
                        singleConditionHandler.Invoke(sender, e);
                    }
                }
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
        private void checkLocation(object sender, EventArgs e)
        {
            foreach(var condition in conditionss)
            {
                if(condition.type.Equals(conditionType.ReachedLocation)) {
                    locationCondition con = (locationCondition)condition;
                    Bot bot = (Bot)sender;
                    con.bot = bot;
                    if(con.isMet)
                    {
                        singleConditionHandler.Invoke(sender, e);
                    }
                }
            }

        }

        private bool isConditionInEvent(String name)
        {
            if (FinishedActionHandler != null && FinishedActionHandler.GetInvocationList().Any())
            {
                foreach (var methodDelegate in FinishedActionHandler.GetInvocationList())
                {
                    if (methodDelegate.Method.Name.Equals(name))
                    {
                        return true;
                    }
                }
            }

            return false;
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
            public Bot bot { get; set; }
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
            public override bool isMet
            {
                get
                {
                    if (bot.CurrentLocation.getObjectAtTile(goal.Value.X, goal.Value.Y) is Bot) return true;
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
            }

            
        }

        /**
        * <summary>
        * Specific condition for location based goals. 
        * </summary>
        */
        private class inventorySpaceCondition : ConditionInformation
        {
            public override bool isMet
            {
                get
                {
                    if (bot.getFarmer.isInventoryFull()) {
                        if (location.debris.Count > 0) return false;
                        return true;
                    }
                    return false;
                }
            }

            protected GameLocation location { get; private set; }

            /**
             * <param name="goal"> The point which the bot needs to reach. </param>
             * <param name="type"> The type of which the condition is. </param>
             */
            public inventorySpaceCondition(conditionType type, GameLocation location) : base(type)
            {
                this.location = location;
            }


        }
    }
}
