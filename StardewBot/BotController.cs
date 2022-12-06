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

namespace StardewBot
{
    public class BotController : IProgrammable
    {
        public readonly string Guid;
        public readonly string Name;
        public readonly Bot Bot;

        private Dictionary<String, IBehavior> behaviors = new Dictionary<String, IBehavior>();
        private MethodQueue queue = new BlocklyBridge.MethodQueue();

        public BotController(Bot bot, string guid)
        {
            Bot = bot;
            Name = bot.Name;
            Guid = guid;

            ModEntry.Dispatcher.Register(this);

            foreach (IBehavior behavior in new IBehavior[] {
                new Events(this),
                new Movement(this),
                new Sensing(this),
                new Farming(this)
            })
            {
                behaviors.Add(behavior.GetType().Name, behavior);
            }
        }

        public void OnClick()
        {
            if (behaviors.TryGetValue("Events", out var eve))
            {
                ((Events)eve).OnClick();
            }
        }

        public void FlagFinished()
        {
            Bot.FlagFinished();
        }

        public void EnqueueMethod(AsyncMethod method)
        {
            queue.Enqueue(method);
        }

        public string GetGuid()
        {
            return Guid;
        }

        public string GetName()
        {
            return Name;
        }

        public object GetObjectForType(Type declaringType)
        {
            if (behaviors.TryGetValue(declaringType.Name, out var obj)) return obj;
            throw new NotImplementedException("Unknown type: " + declaringType.Name);
        }

        public bool TryTestCode()
        {
            Levels.Level.OnTesting(this);
            var events = behaviors.Values.Where(b => b is Events).First() as Events;
            events.OnCodeTested();
            if (ModEntry.Overlay.Showing) ModEntry.Overlay.Hide();
            return true;
        }

        public void Update(GameTime time)
        {
            foreach (var behavior in behaviors.Values) behavior.Update(time);
            queue.Update();
        }

        public void Dispose()
        {
            behaviors.Clear();
            
        }
    }

    public interface IBehavior
    {
        void Update(GameTime time);
    }
}
