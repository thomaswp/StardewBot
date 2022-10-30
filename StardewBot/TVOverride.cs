using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using static StardewValley.GameLocation;

namespace StardewBot
{
    class TVOverride
    {
        private static StardewValley.Objects.TV tv;

        public static void OverrideTVMenu(MenuChangedEventArgs e)
        {
            var farmhouse = Game1.getLocationFromName("FarmHouse");

            if (e.NewMenu is DialogueBox && Game1.currentLocation.Equals(farmhouse))
            {

                var question = ModEntry.helper.Reflection.GetField<afterQuestionBehavior>(farmhouse, "afterQuestion");
                if (question != null && question.GetValue() != null && question.GetValue().Target is StardewValley.Objects.TV)
                {
                    tv = (StardewValley.Objects.TV)question.GetValue().Target;
                    var menu = (DialogueBox)e.NewMenu;

                    question.SetValue(CustomSelectChannel);

                    List<Response> channels = new List<Response>();
                    channels.Add(new Response("Weather", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13105")));
                    channels.Add(new Response("Fortune", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13107")));
                    string day = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
                    if (day.Equals("Mon") || day.Equals("Thu"))
                    {
                        channels.Add(new Response("Livin'", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13111")));
                    }
                    if (day.Equals("Sun"))
                    {
                        channels.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13114")));
                    }
                    if (day.Equals("Wed") && Game1.stats.DaysPlayed > 7)
                    {
                        channels.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13117")));
                    }
                    if (Game1.Date.Season == "fall" && Game1.Date.DayOfMonth == 26 && Game1.stats.getStat("childrenTurnedToDoves") != 0 && !Game1.player.mailReceived.Contains("cursed_doll"))
                    {
                        channels.Add(new Response("???", "???"));
                    }
                    if (Game1.player.mailReceived.Contains("pamNewChannel"))
                    {
                        channels.Add(new Response("Fishing", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel")));
                    }
                    channels.Add(new Response("Bot", "Bot Tutorials"));
                    channels.Add(new Response("(Leave)", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13118")));


                    Game1.activeClickableMenu = new DialogueBox(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13120"), channels);
                    Game1.dialogueUp = true;
                    Game1.player.CanMove = false;
                }
            }
        }


        public static void CustomSelectChannel(Farmer who, string answer)
        {
            if (answer.Split(' ')[0].Equals("Bot"))
            {
                Game1.activeClickableMenu = new Levels.TutorialMenu();
            }
            else
            {
                try
                {
                    ModEntry.helper.Reflection.GetMethod(tv, "selectChannel").Invoke(who, answer);
                }
                catch { }
            }

        }
    }
}
