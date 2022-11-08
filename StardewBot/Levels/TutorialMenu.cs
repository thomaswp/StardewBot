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
    class TutorialMenu : IClickableMenu
    {
        public InventoryPage inventoryPage;

        public InventoryMenu inventory;

        private string descriptionText = "";

        private string hoverText = "";

        private string descriptionTitle = "";

        private string hoverTitle = "";

        private int hoverAmount;

        private Item hoveredItem;

        private List<ClickableComponent> TutorialButtons;

        private List<List<Level>> pages;

        private int currentPage;

        public ClickableTextureComponent backButton;
        public ClickableTextureComponent previousButton;
        public ClickableTextureComponent nextButton;

        private int leftWidth;

        private int clearTop;

        private int buttonTotalHeight;

        JObject message;

        public TutorialMenu()
        : base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            message = new JObject();
            message.Add("Menu", "Tutorial Menu");
            ModEntry.logger.Log(message.ToString(), action: "Menu.Opened");
            leftWidth = 230;
            clearTop = IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearTopBorder;
            buttonTotalHeight = 460;

            Game1.currentSpeaker = Game1.getCharacterFromName("Sebastion");

            this.paginateLevels();

            this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth, playerInventory: true);
            /*this.inventoryPage = new InventoryPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height);*/
            /*base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + this.width - 20, base.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);*/

            this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, 36, 33), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 3f)
            {
                myID = 102,
                rightNeighborID = 0
            };

            this.previousButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2 + this.leftWidth + 20, this.yPositionOnScreen + this.height - (IClickableMenu.spaceToClearTopBorder + 10), 36, 33), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 3f)
            {
                myID = 103,
                rightNeighborID = 104
            };

            this.nextButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - (IClickableMenu.spaceToClearSideBorder + 20), this.yPositionOnScreen + this.height - (IClickableMenu.spaceToClearTopBorder + 10), 36, 33), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 3f)
            {
                myID = 104,
                leftNeighborID = 103
            };

            /* base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 20, base.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);*/

            this.TutorialButtons = new List<ClickableComponent>();
            for (int i = 0; i < 6; i++)
            {
                this.TutorialButtons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 7 + IClickableMenu.borderWidth + 140, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + i * (buttonTotalHeight / 6), this.width - 32 * 7 - 16 - 140 , buttonTotalHeight / 6), i.ToString() ?? "")
                {
                    myID = i,
                    downNeighborID = -7777,
                    upNeighborID = ((i > 0) ? (i - 1) : (-1)),
                    rightNeighborID = -7777,
                    leftNeighborID = -7777,
                    fullyImmutable = true
                });
            }

            base.populateClickableComponentList();
            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();
            }

            base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 20, base.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
        }

        private void paginateLevels()
        {
            this.pages = new List<List<Level>>();
            for (int j = Level.levelsDict.Count - 1; j >= 0; j--)
            {
                int which = j;
                while (this.pages.Count <= which / 6)
                {
                    this.pages.Add(new List<Level>());
                }
            }
            for (int i = Level.levelsDict.Count - 1; i >= 0; i--)
            {
                    int which2 = Level.levelsDict.Count - 1 - i;
                    this.pages[which2 / 6].Add(Level.GetLevels[i]);
                
            }
            if (this.pages.Count == 0)
            {
                this.pages.Add(new List<Level>());
            }
            this.currentPage = Math.Min(Math.Max(this.currentPage, 0), this.pages.Count - 1);
        }

        private void nonQuestPageForwardButton()
        {
            this.currentPage++;
            Game1.playSound("shwip");
            if (Game1.options.SnappyMenus && this.currentPage == this.pages.Count - 1)
            {
                base.currentlySnappedComponent = base.getComponentWithID(0);
                this.snapCursorToCurrentSnappedComponent();
            }
        }

        private void nonQuestPageBackButton()
        {
            this.currentPage--;
            Game1.playSound("shwip");
            if (Game1.options.SnappyMenus && this.currentPage == 0)
            {
                base.currentlySnappedComponent = base.getComponentWithID(0);
                this.snapCursorToCurrentSnappedComponent();
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            for (int i = 0; i < this.TutorialButtons.Count; i++)
            {
                if (this.pages.Count > 0 && this.pages[this.currentPage].Count > i && this.TutorialButtons[i].containsPoint(x, y))
                {
                    Game1.playSound("smallSelect");
                    try
                    {
                        Level.LoadLevel(this.pages[this.currentPage][i].MapName);
                    } catch
                    {
                        Game1.drawObjectDialogue("Could not load level " + this.pages[this.currentPage][i].MapName + ".");
                    }

                    this.exitThisMenu(true);

                    return;
                }
            }
            if (this.currentPage < this.pages.Count - 1 && this.nextButton.containsPoint(x, y))
            {
                this.nonQuestPageForwardButton();
                return;
            }
            if (this.currentPage > 0 && this.previousButton.containsPoint(x, y))
            {
                this.nonQuestPageBackButton();
                return;
            }
            if (this.backButton.containsPoint(x, y))
            {
                base.exitThisMenu();
                return;
            }

            Game1.playSound("bigDeSelect");
            return;
        }

        public override void update(GameTime time)
        {
            base.update(time);
        }

        private void drawPortrait(int x, int y, SpriteBatch b)
        {

        }

        public override void draw(SpriteBatch b)
        {
            int topPadding = 5;
            //Draws darkness over background
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            //Draws backgorund box with vertical partition.
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, speaker: true, drawOnlyBox: true);
            base.drawVerticalPartition(b, this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.leftWidth);

            

            SpriteText.drawString(b, "Tutorials", (this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.leftWidth) + ((this.width - (IClickableMenu.spaceToClearSideBorder * 2)) - this.leftWidth) / 3 + (15), this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + topPadding);

            int boxX;
            int boxY;

            for (int i = 0; i < this.TutorialButtons.Count; i++)
            {
                if (this.pages.Count() > 0 && this.pages[this.currentPage].Count() > i)
                {
                    this.TutorialButtons[i].bounds.X = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 4 + this.leftWidth + 23;
                    this.TutorialButtons[i].bounds.Y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + topPadding * 2 + i * (buttonTotalHeight / 6); ;
                    //Text Box
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.TutorialButtons[i].bounds.X, this.TutorialButtons[i].bounds.Y, this.TutorialButtons[i].bounds.Width, this.TutorialButtons[i].bounds.Height, this.pages[this.currentPage][i].isCompleted ? new Color(224, 191, 128) : this.TutorialButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())  ? Color.Wheat : Color.White, 4f, drawShadow: false);
                    
                    //Level Text
                    //SpriteText.drawString(b, this.pages[this.currentPage][i].MapName, this.TutorialButtons[i].bounds.X + 48 + 4, this.TutorialButtons[i].bounds.Y + 15);
                    Utility.drawTextWithShadow(b, this.pages[this.currentPage][i].MapName, Game1.dialogueFont, new Vector2(this.TutorialButtons[i].bounds.X + 36 + 4, this.TutorialButtons[i].bounds.Y + 15), this.pages[this.currentPage][i].isCompleted ? new Color(36, 26, 36) : Game1.textColor, 1f, 0.1f);
                    //Checkbox
                    b.Draw(Game1.mouseCursors, new Vector2(this.TutorialButtons[i].bounds.X + this.TutorialButtons[i].bounds.Width - 64, this.TutorialButtons[i].bounds.Y + 20), this.pages[this.currentPage][i].isCompleted ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked, Color.White * 1f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
                }

            }
            if(this.xPositionOnScreen != backButton.bounds.X || this.yPositionOnScreen != backButton.bounds.Y)
            {

                backButton.bounds = new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 25, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 10, 36, 33);
            }

            this.backButton.draw(b);

            if (this.xPositionOnScreen != previousButton.bounds.X || this.yPositionOnScreen != previousButton.bounds.Y)
            {

                previousButton.bounds = new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2 + this.leftWidth + 30, this.yPositionOnScreen + this.height - (IClickableMenu.borderWidth + 30), 36, 33);
            }

            if (this.currentPage > 0)
            {
                this.previousButton.draw(b);
            }

            if (this.xPositionOnScreen != nextButton.bounds.X || this.yPositionOnScreen != nextButton.bounds.Y)
            {

                nextButton.bounds = new Rectangle(this.xPositionOnScreen + this.width - (IClickableMenu.spaceToClearSideBorder * 2 + 50), this.yPositionOnScreen + this.height - (IClickableMenu.borderWidth + 30), 36, 33);
            }

            if (this.currentPage < this.pages.Count - 1)
            {
                this.nextButton.draw(b);
            }

            if (!Game1.options.SnappyMenus || !Game1.options.hardwareCursor)
            {
                base.drawMouse(b, ignore_transparency: true);
            }

           /* Game1.drawDialogueBox(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 10, this.yPositionOnScreen + spaceToClearTopBorder + 200, this.leftWidth - 10, 300, message: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", speaker: true, drawOnlyBox: true);*/
        }

        protected override void cleanupBeforeExit()
        {
            ModEntry.logger.Log(message.ToString(), action: "Menu.Closed");
            Game1.currentSpeaker = null;
            base.cleanupBeforeExit();
        }
        public override void performHoverAction(int x, int y)
        {
            this.hoverAmount = -1;
            this.descriptionText = "";
            this.descriptionTitle = "";
            this.hoveredItem = this.inventory.hover(x, y, Game1.player.CursorSlotItem);
            this.hoverText = this.inventory.hoverText;
            this.hoverTitle = this.inventory.hoverTitle;

            for (int i = 0; i < this.TutorialButtons.Count; i++)
            {
                if (this.TutorialButtons[i].containsPoint(x, y) && !this.TutorialButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
                {
                    Game1.playSound("Cowboy_gunshot");
                }
            }

            this.backButton.tryHover(x, y, 0.2f);
            this.previousButton.tryHover(x, y, 0.2f);
            this.nextButton.tryHover(x, y, 0.2f);
        }

        /*********
       ** Protected methods
       *********/
        /// <summary>Get the viewport size adjusted for compatibility.</summary>
        protected Point GetViewportSize()
        {
            Point viewport = new Point(Game1.uiViewport.Width, Game1.uiViewport.Height);
                viewport = new Point(
                    x: Math.Min(viewport.X, Game1.graphics.GraphicsDevice.Viewport.Width),
                    y: Math.Min(viewport.Y, Game1.graphics.GraphicsDevice.Viewport.Height)
                );
            

            return viewport;
        }
    }
}
