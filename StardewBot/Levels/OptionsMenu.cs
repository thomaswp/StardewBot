using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using Newtonsoft.Json.Linq;

namespace StardewBot.Levels
{
    public class OptionsButtonMenu : IClickableMenu
    {
        private Vector2 position;
        private ClickableComponent ResetButton;
        private ClickableComponent ExitButton;
        static int buttonWidth = 200;
        static int buttonHeight = 60;
        private int clearTop;


        public OptionsButtonMenu()
        : base(20, 20, 300, 300, showUpperRightCloseButton: false)
        {
            clearTop = IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearTopBorder;

            this.position = new Vector2(this.xPositionOnScreen + 30, this.yPositionOnScreen + 20);
            this.ResetButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 30, this.yPositionOnScreen + 20, buttonWidth, buttonHeight), "Reset");
            this.ExitButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 30, this.yPositionOnScreen + 2 * 20 + buttonHeight, buttonWidth, buttonHeight), "Exit");
            base.populateClickableComponentList();
        }


        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.ResetButton.containsPoint(x, y) && Level.CurrentLevel != null)
            {
                Level.resetLevel();
                return;
            }

            if (this.ExitButton.containsPoint(x, y) && Level.CurrentLevel != null)
            {
                Level.leaveLevel();
                return;
            }

            Game1.playSound("bigDeSelect");
            return;
        }

        public override void update(GameTime time)
        {
            base.update(time);
        }
        public override void draw(SpriteBatch b)
        {
            updatePosition();

            //Draws backgorund box with vertical partition.
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, speaker: true, drawOnlyBox: true);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.ResetButton.bounds.X, this.ResetButton.bounds.Y, this.ResetButton.bounds.Width, this.ResetButton.bounds.Height, this.ResetButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
            Utility.drawTextWithShadow(b, ResetButton.name , Game1.dialogueFont, new Vector2(this.ResetButton.bounds.X + 36 + 4, this.ResetButton.bounds.Y + 8), Game1.textColor, 1f, 0.1f);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.ExitButton.bounds.X, this.ExitButton.bounds.Y, this.ExitButton.bounds.Width, this.ExitButton.bounds.Height, this.ExitButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
            Utility.drawTextWithShadow(b, ExitButton.name , Game1.dialogueFont, new Vector2(this.ExitButton.bounds.X + 36 + 4, this.ExitButton.bounds.Y + 8), Game1.textColor, 1f, 0.1f);

            if (!Game1.options.SnappyMenus || !Game1.options.hardwareCursor)
            {
                base.drawMouse(b, ignore_transparency: true);
            }

           /* Game1.drawDialogueBox(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 10, this.yPositionOnScreen + spaceToClearTopBorder + 200, this.leftWidth - 10, 300, message: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", speaker: true, drawOnlyBox: true);*/
        }

        protected override void cleanupBeforeExit()
        {
            //ModEntry.logger.Log(message.ToString(), action: "Menu.Closed");
            base.cleanupBeforeExit();
        }

        private void updatePosition()
        {
            //base.xPositionOnScreen = (int)this.position.X;
            //base.yPositionOnScreen = (int)this.position.Y;
            this.ResetButton.bounds = new Rectangle(this.xPositionOnScreen + 30 + 20, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 20, buttonWidth, buttonHeight);
            this.ExitButton.bounds = new Rectangle(this.xPositionOnScreen + 30 + 20, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 2 * 20 + buttonHeight, buttonWidth, buttonHeight);
        }
    }
}
