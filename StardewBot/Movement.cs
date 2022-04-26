﻿using BlocklyBridge;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using Microsoft.Xna.Framework;

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

            ModEntry.ModHelper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseRight || e.Button == SButton.MouseLeft)
            {
                if (e.Cursor.Tile == NPC.getTileLocation())
                {
                    OnClick();
                }
            }
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
            Vector2 startTile = NPC.getTileLocation();
            return new AsyncMethod()
                .Do(() => {
                //NPC.tryToMoveInDirection(NPC.FacingDirection, false, 0, false);
                NPC.setMovingInFacingDirection();
                //NPC.facePlayer(Game1.player);
            })
            .UpdateUntil(() => NPC.getTileLocation() != startTile)
            .Do(() => NPC.Halt());
        }

        [ScriptableMethod]
        public AsyncMethod Jump()
        {
            Logger.Log(NPC.yJumpOffset);
            return new AsyncMethod()
                .Do(() => NPC.jump())
                .UpdateUntil(() => NPC.yJumpOffset != 0)
                .UpdateUntil(() => NPC.yJumpOffset == 0);
        }

        public void Update()
        {
            //NPC.updateMovement(NPC.currentLocation, Game1.currentGameTime);
            NPC.MovePosition(Game1.currentGameTime, Game1.viewport, NPC.currentLocation);
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
