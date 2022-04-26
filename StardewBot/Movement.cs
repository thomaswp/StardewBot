using BlocklyBridge;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot
{
    [ScriptableBehavior("Movement", 60)]
    public class Movement : IBehavior
    {
        public readonly Bot Bot;
        public NPC NPC { get { return Bot.NPC; } }

        private bool inDialog = false;

        public Movement(Bot bot)
        {
            Bot = bot;
        }

        [ScriptableEvent(false)]
        public void OnClick()
        {
            BlocklyGenerator.SendEvent(Bot, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        [ScriptableEvent(false)]
        public void OnDialogStart()
        {
            BlocklyGenerator.SendEvent(Bot, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        [ScriptableEvent(false)]
        public void OnDialogEnd()
        {
            BlocklyGenerator.SendEvent(Bot, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        [ScriptableMethod]
        public AsyncMethod MoveForward()
        {
            // TODO
            return new AsyncMethod().Do(() => Logger.Log("Forward..."));
        }

        [ScriptableMethod]
        public AsyncMethod Jump()
        {
            return new AsyncMethod()
                .Do(() => NPC.jump())
                .UpdateUntil(() => NPC.yJumpOffset > 0)
                .UpdateUntil(() => NPC.yJumpOffset == 0);
        }

        public void Update()
        {
            bool inDialogNow = NPC.CurrentDialogue.Count == 0;
            if (inDialog != inDialogNow)
            {
                inDialog = inDialogNow;
                if (inDialog)
                {
                    OnDialogStart();
                    Logger.Log("Dialog starting!");
                }
                else
                {
                    OnDialogEnd();
                    Logger.Log("Dialog ended!");
                }
            }
        }
    }
}
