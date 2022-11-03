/*
This class is a stardew valley Object subclass that represents a Bot.

*/

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using StardewBot;
using StardewBot.Levels;
using BlocklyBridge;
using static StardewValley.Debris;

namespace Farmtronics {
	public class Terminal : StardewValley.Object {

		public Color screenColor = Color.Transparent;
		public Color statusColor = Color.Yellow;
		public static List<Bot> instances { 
			get
            {
				return Bot.instances;
            } 
		}

		Vector2 position;

		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		static Texture2D TerminalSprite;


		public Terminal(Vector2 tileLocation, GameLocation location) {
			//Debug.Log($"Creating Bot({farmer?.Name}):\n{Environment.StackTrace}");
			if (TerminalSprite == null) {
				TerminalSprite = ModEntry.helper.ModContent.Load<Texture2D>("assets/BotSprites.png");
			}

			Name = "Terminal";
			type.Value = "Crafting";
			bigCraftable.Value = true;
			canBeSetDown.Value = true;

			position = TileLocation * 64f;
		}


		//----------------------------------------------------------------------

		public static void UpdateAll(GameTime gameTime) {
			bool debug = false;//ModEntry.instance.Helper.Input.IsDown(SButton.RightShift);
			for (int i=instances.Count - 1; i >= 0; i--) {
				if (debug) ModEntry.logger.Log($"Updating {i}/{instances.Count}: {instances[i].Name}");
				instances[i].Update(gameTime);
			}
		}

		public static void ClearAll() {
			instances.Clear();
		}

		/// <summary>
		/// Initializes each bot instance.
		/// Does nothing if the bot instance has already been initialized.
		/// Effectively starts up the bots.
		/// </summary>
		public static void InitShellAll() {
			foreach(var instance in instances) {
				instance.InitShell();
			}
		}

		public override void dropItem(GameLocation location, Vector2 origin, Vector2 destination) {
			//Debug.Log($"Bot.dropItem({location}, {origin}, {destination}");
			base.dropItem(location, origin, destination);
		}

		public override bool performDropDownAction(Farmer who) {
			//Debug.Log($"Bot.performDropDownAction({who.Name})");
			base.performDropDownAction(who);

			return false;	// OK to set down (add to Objects list in the tile)
		}

		/// <summary>
		/// placementAction is called when the player, who is carring a Bot item, indicates
		/// that they want to place it down.  The item is going to be destroyed; we have
		/// to create a new Bot instance that matches its data.
		/// </summary>
		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null) {
			//Debug.Log($"Bot.placementAction({location}, {x}, {y}, {who.Name})");
			Vector2 placementTile = new Vector2(x / 64, y / 64);
			// Create a new bot, copying the farmer (including inventory) from this one.
			var terminal = new Terminal(placementTile, location);
			Game1.player.currentLocation.overlayObjects[placementTile] = terminal;
			terminal.shakeTimer = 50;

			location.playSound("hammer");
			return true;
		}

		
		

		

		public void Update(GameTime gameTime) {
			bool debug = false;//ModEntry.instance.Helper.Input.IsDown(SButton.RightShift);

        }

		public override string getDescription() {
			return "A programmable mechanical wonder.";
		}

		protected override string loadDisplayName() {
			return name;
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false) {
			//Debug.Log($"Bot.checkForAction({who.Name}, {justCheckingForActivity}), tool {who.CurrentTool}");
			if (justCheckingForActivity) return true;
			// all this overriding... just to change the open sound.
			if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true)) {
				//Debug.Log($"Bailing because didPlayerJustRightClick is false");
				return false;
			}

			// ToDo: use mutex to ensure only one player can open a bot at a time.
			// (Tried, but couldn't get to work:
			//Debug.Log($"Requesting mutex lock: {mutex}, IsLocked={mutex.IsLocked()}, IsLockHeld={mutex.IsLockHeld()}");
			//mutex.RequestLock(delegate {
			//	Game1.playSound("bigSelect");
			//	Game1.player.Halt();
			//	Game1.player.freezePause = 1000;
			//	ShowMenu();
			//}, delegate {
			//	Debug.Log("Failed to get mutex lock :(");
			//});

			// For now, just dewit:
			Game1.playSound("bigSelect");
			Game1.player.Halt();
			Game1.player.freezePause = 1000;
			ShowMenu();
		
			return true;
		}

		
		public override bool performToolAction(Tool t, GameLocation location) {
			//Debug.Log($"{name} Bot.performToolAction({t}, {location})");

           if (t is Pickaxe or Axe or Hoe) {
				//Debug.Log("{name} Bot.performToolAction: creating custom debris");
				var who = t.getLastFarmerToUse();
                this.performRemoveAction(this.TileLocation, location);
				Debris deb = new Debris(this.getOne(), who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y));
                Game1.currentLocation.debris.Add(deb);
				//Debug.Log($"{name} Created debris with item {deb.item} and energy {energy}");
				// Remove, stop, and destroy this bot
                Game1.currentLocation.overlayObjects.Remove(this.TileLocation);

                return false;
            }

			// previous code, that called the base... this sometimes resulted
			// in picking up a chest, while leaving a ghost bot behind:
			//bool result = base.performToolAction(t, location);
			//Debug.Log($"{name} Bot.performToolAction: My TileLocation is now {this.TileLocation}");
			//return result;
			// I'm not aware of any use case for doing the default tool action on a bot.
			// So now we're going to avoid that whole issue by always doing:
			return false;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1) {
			//ModEntry.instance.print($"draw 1 at {x},{y}, {alpha}");

			if (alpha < 0.9f) {
				// Drawing with alpha=0.5 is done when the player is placing the bot down
				// in the world.  In this case, our internal position doesn't matter;
				// we want to update that to match the given tile position.
				position.X = x * 64;
				position.Y = y * 64;
			}

			// draw shadow
			spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport,
				new Vector2(position.X + 32, position.Y + 51 + 4)),
				Game1.shadowTexture.Bounds, Color.White * alpha, 0f,
				new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f,
				SpriteEffects.None, (float)getBoundingBox(new Vector2(x, y)).Bottom / 15000f);

			// draw sprite
			if (TerminalSprite == null) {
				ModEntry.logger.Log("Bot.draw: botSprites is null; bailing out");
				return;
			}

			Vector2 position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2(
				position.X + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0),
				position.Y + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
			// Note: FacingDirection 0-3 is Up, Right, Down, Left
			int facing = 2;
			Rectangle srcRect = new Rectangle(16 * facing, 0, 16, 24);
			Vector2 origin2 = new Vector2(8f, 8f);
			float scale = (this.scale.Y > 1f) ? getScale().Y : 4f;
			float z = (float)(getBoundingBox(new Vector2(x, y)).Bottom) / 10000f;
			// base sprite
			spriteBatch.Draw(TerminalSprite, position3, srcRect, Color.White * alpha, 0f,
				origin2, scale, SpriteEffects.None, z);
			// screen color (if not black or clear)
			if (screenColor.A > 0 && (screenColor.R > 0 || screenColor.G > 0 || screenColor.B > 0)) {
				srcRect.Y = 24;
				spriteBatch.Draw(TerminalSprite, position3, srcRect, screenColor * alpha, 0f,
					origin2, scale, SpriteEffects.None, z + 0.001f);
			}
			// screen shine overlay
			srcRect.Y = 48;
			spriteBatch.Draw(TerminalSprite, position3, srcRect, Color.White * alpha, 0f,
				origin2, scale, SpriteEffects.None, z + 0.002f);
			// status light color (if not black or clear)
			if (statusColor.A > 0 && (statusColor.R > 0 || statusColor.G > 0 || statusColor.B > 0)) {
				srcRect.Y = 72;
				spriteBatch.Draw(TerminalSprite, position3, srcRect, statusColor * alpha, 0f,
					origin2, scale, SpriteEffects.None, z + 0.002f);
			}

		}

		public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1) {
			//ModEntry.instance.print($"draw 2 at {xNonTile},{yNonTile}, {layerDepth}, {alpha}");
			base.draw(spriteBatch, xNonTile, yNonTile, layerDepth, alpha);
		}

		/// <summary>
		/// Draw the bot as it should appear above the player's head when held.
		/// </summary>
        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f) {
			//Debug.Log($"Bot.drawWhenHeld");
			if (TerminalSprite == null) {
				ModEntry.logger.Log("Bot.drawWhenHeld: botSprites is null; bailing out");
				return;
			}
			Rectangle srcRect = new Rectangle(16 * f.facingDirection, 0, 16, 24);
            spriteBatch.Draw(TerminalSprite, objectPosition, srcRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow) {
			if (TerminalSprite == null) {
				ModEntry.logger.Log("Bot.drawInMenu: botSprites is null; bailing out");
				return;
			}
 			//Debug.Log($"Bot.drawInMenu with scaleSize {scaleSize}");
            if ((bool)this.IsRecipe) {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1)
				|| drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

			Rectangle srcRect = new Rectangle(0, 112, 16, 16);
			spriteBatch.Draw(TerminalSprite, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), srcRect, color * transparency, 0f,
				new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);

            if (shouldDrawStackNumber) {
				var loc = location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.Stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f);
                Utility.drawTinyDigits(this.Stack, spriteBatch, loc, 3f * scaleSize, 1f, color);
			}
        }

        public override void drawAsProp(SpriteBatch b) {
  			//Debug.Log($"Bot.drawAsProp");
 			if (TerminalSprite == null) {
				ModEntry.logger.Log("Terminal.drawAsProp: TerminalSprite is null; bailing out");
				return;
			}
			if (this.isTemporarilyInvisible) return;
            int x = (int)this.TileLocation.X;
            int y = (int)this.TileLocation.Y;

            Vector2 scaleFactor = Vector2.One; // this.PulseIfWorking ? this.getScale() : Vector2.One;
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle srcRect = new Rectangle(16 * 2, 0, 16, 24);
            b.Draw(destinationRectangle: new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f),
				(int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f)),
				texture: TerminalSprite,
				sourceRectangle: srcRect,
				color: Color.White,
				rotation: 0f,
				origin: Vector2.Zero,
				effects: SpriteEffects.None,
				layerDepth: Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
        }

		public override bool canStackWith(ISalable other) {
			// Bots don't allow stacking.  Too hard to deal with individual bot
			// names, energy, inventory, etc.
			return false;
		}

	
		public int GetActualCapacity() {
			return 12;
		}

		/// <summary>
		/// Initializes this bot instance.
		/// Does nothing if the bot instance has already been initialized.
		/// Effectively starts up the bot.
		/// </summary>
		public void InitShell() {
			// TODO: Reset
			//if(shell == null) {
			//	shell = new Shell();
			//	shell.Init(this);
			//}
		}

		public void ShowMenu() {
			ModEntry.logger.Log($"{Name} ShowMenu()");

			// Make sure the bot is booted up when showing the menu.
			InitShell();
			/*Game1.activeClickableMenu = new BotUIMenu();*/
		}
    }
}
