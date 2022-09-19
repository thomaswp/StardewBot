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
        public event EventHandler singleConditionMet;
        public EventHandler singleConditionHandler;

        //Movement-Based Event and Conditions
        public event EventHandler<MovedArgs> botMoved;
        public EventHandler<MovedArgs> moveHandler;
        public Point? locationGoal;
        /*private Dictionary<Point?, bool> locationGoals;*/

        /**
         * Property to see if all of the ocnditions are met.
         */
        /*public bool allLocationsVisited
        {
            get
            {
                if (locationGoals.Count == 0) return false;
                foreach (var locations in locationGoals)
                {
                    if (locations.Value == false) return false;
                }
                return true;
            }
        }*/

        //Keeps track of all current conditions
        //TODO Change how Dictionary holds conditions so that multiple of the same type of items
        private Dictionary<conditionType, bool> conditions;

        /**
         * Property to see if all of the ocnditions are met.
         */
        public bool allConditionsMet
        {
            get
            {
                if (conditions.Count == 0) return false;
                foreach(var condition in conditions)
                {
                    if (condition.Value == false) return false;
                }
                return true;
            }
        }

        /**
         * Defines what conditions are available to use
         */
        public enum conditionType
        {
            Test,
            reachedLocation, 
            HarvestedCrops,
            ItemsPlaced,
            TilesTiled
        }

        /**
         * Sets up the Conditions class with a list of conditions that a level can have and 
         * a list of points where goal locations can be added. Also initializes the 
         * singleConditionHandler so that every time a condition is met, it checks to see if
         * all conditions have been met. 
         */
        public Conditions()
        {
            conditions = new Dictionary<conditionType, bool>();
            /*locationGoals = new Dictionary<Point?, bool>();*/
            singleConditionHandler = singleConditionMet;
            singleConditionHandler += checkAllConditions;
        }

        /**
         * Checks to see if all of the conditions that were set in the level are met. If so, the
         * level ends.
         */
        private void checkAllConditions(object sender, EventArgs e)
        {
            if (allConditionsMet)
            {
                Level.levelCleared();
            }
        }




        /**
         * Adds a location condition based on a single location. Can be expanded to multiple locations later. 
         */
        public void addLocationCondition(Point? newLocation)
        {
            if (newLocation != null) locationGoal = newLocation;
            moveHandler = botMoved;
            moveHandler += checkLocation;
            conditions.Add(conditionType.reachedLocation, false);
            /*if (newLocation != null) locationGoals.Add(newLocation, false);*/
        }
        /**
         * Checks to see if the location condition is being used. If so, it checks to see if the bots location
         * is the same as one of the goal locations. If so
         */
        private void checkLocation(object sender, MovedArgs e)
        {
            if (locationGoal.Equals(e.botLocation))
            {
                conditions[conditionType.reachedLocation] = true;
                singleConditionHandler.Invoke(sender, e);
            }
            else
            {
                conditions[conditionType.reachedLocation] = false;
            }  
            /*if(conditions.ContainsKey(conditionType.reachedLocation))
            {
                List<Point?> reached = new List<Point?>();
                List<Point?> notReached = new List<Point?>();
                foreach (var location in locationGoals)
                {
                    if(location.Key.Equals(e.botLocation))
                    {
                        reached.Add(location.Key);
                    } else
                    {
                        notReached.Add(location.Key);
                    }
                }

                foreach (var location in reached)
                {
                    locationGoals[location] = true;
                }

                foreach (var location in notReached)
                {
                    locationGoals[location] = false;
                }

                if (allLocationsVisited)
                {
                    singleConditionHandler.Invoke(sender, e);
                    conditions[conditionType.reachedLocation] = true;
                }
                else
                {
                    conditions[conditionType.reachedLocation] = false;
                }
            }*/

            /*if (locationGoals.ContainsKey(e.botLocation))
            {
                locationGoals[e.botLocation] = true;

                if (allLocationsVisited)
                {
                    singleConditionHandler.Invoke(sender, e);
                    conditions[conditionType.reachedLocation] = true;
                }
                else
                {
                    conditions[conditionType.reachedLocation] = false;
                }
            }*/
        }

        /**
         * Template for function to add a condition.
         */
        /*public void addTestFunc()
        {

        
        }*/

        /**
        * Template for function to check whether condition has been met.
        */

        /*private bool checkTestFunc()
        {
            return true;
        }*/
    }
}
