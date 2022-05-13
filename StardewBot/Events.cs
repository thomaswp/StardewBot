using BlocklyBridge;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot
{
    [ScriptableBehavior("Events", 100)]
    public class Events : ActionCategory
    {
        public Events(BotController controller) : base(controller)
        {
            ModEntry.ModHelper.Events.Input.ButtonPressed += Input_ButtonPressed;
            ModEntry.ModHelper.Events.GameLoop.DayStarted += (sender, e) => OnDayStarted();
            //ModEntry.ModHelper.Events.GameLoop.OneSecondUpdateTicked += (sender, e) => EachSecond();
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseRight || e.Button == SButton.MouseLeft)
            {
                //Vector2 tile = e.Cursor.Tile;
                //var location = Game1.player.currentLocation;
                //var loc = new xTile.Dimensions.Location((int)tile.X, (int)tile.Y);
                //var rect = new Rectangle(loc.X, loc.Y, 1, 1);
                //Logger.Log($"tile {tile} passable {location.isTilePassable(loc, Game1.viewport)}");
                //Logger.Log($"tile {tile} colliding {location.isCollidingPosition(rect, Game1.viewport, true)}");
                //Logger.Log($"tile {tile} placable {location.isTileLocationTotallyClearAndPlaceable(tile)} in {Location.Name}");
                if (e.Cursor.Tile == Bot.TileLocation)
                {
                    OnClick();
                }
            }
        }

        [ScriptableEvent()]
        public void OnClick()
        {
            BlocklyGenerator.SendEvent(Controller, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        [ScriptableEvent()]
        public void OnDayStarted()
        {
            BlocklyGenerator.SendEvent(Controller, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        // Currently disabled to minimize logging...
        [ScriptableEvent()]
        public void EachSecond()
        {
            BlocklyGenerator.SendEvent(Controller, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
    }
}
