using BlocklyBridge;
using Farmtronics;
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

        private Dictionary<Type, IBehavior> behaviors = new Dictionary<Type, IBehavior>();
        private MethodQueue queue = new BlocklyBridge.MethodQueue();

        public BotController(Bot bot, string guid)
        {
            Bot = bot;
            Name = bot.Name;
            Guid = guid;

            ModEntry.Dispatcher.Register(this);

            foreach (IBehavior behavior in new IBehavior[] {
                new Movement(this),
            })
            {
                behaviors.Add(behavior.GetType(), behavior);
            }
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
            if (behaviors.TryGetValue(declaringType, out var obj)) return obj;
            throw new NotImplementedException("Unknown type: " + declaringType.Name);
        }

        public void Update()
        {
            foreach (var behavior in behaviors.Values) behavior.Update();
            queue.Update();
        }
    }

    public interface IBehavior
    {
        void Update();
    }
}
