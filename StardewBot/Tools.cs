using BlocklyBridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot
{
    public enum ToolType
    {
        Hoe,
        Axe,
        Pickaxe,
        Watering_Can,
        Scythe,
    }

    [ScriptableBehavior("Tools", 140)]
    public class Tools : ActionCategory
    {
        public Tools(BotController controller) : base(controller)
        { }

        [ScriptableMethod]
        public AsyncMethod UseTool(ToolType tool)
        {
            return DoUntilFinished(() => Bot.UseTool(tool.ToString().Replace("_", " ")));
        }

    }
}
