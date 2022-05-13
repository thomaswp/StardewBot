using BlocklyBridge;
using Farmtronics;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot
{
    public abstract class ActionCategory : IBehavior
    {
        public readonly BotController Controller;

        public Bot Bot { get { return Controller.Bot; } }

        public const int INSTANT_ACTION_DURATION = 5;

        protected readonly static AsyncMethod delay = new AsyncMethod().Wait(INSTANT_ACTION_DURATION);

        protected AsyncMethod finishAction => new AsyncMethod().UpdateUntil(() => !Bot.IsPerformingAction);

        public GameLocation Location { get { return Bot.CurrentLocation; } }

        public ActionCategory(BotController controller)
        {
            Controller = controller;
        }

        public virtual void Update(GameTime time)
        {
        }

        protected AsyncMethod DoInstantly(Action action)
        {
            return new AsyncMethod().Do(action).Do(delay);
        }

        protected AsyncMethod DoUntilFinished(Action action)
        {
            return new AsyncMethod().Do(action).Do(finishAction);
        }

        protected AsyncFunction<T> Result<T>(T value)
        {
            var func = new AsyncFunction<T>();
            //func.Do(delay);
            func.Return(() => value);
            return func;
        }
    }
}
