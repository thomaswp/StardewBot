using BlocklyBridge;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot
{
    public class BotFarmer : NPC
    {
		private readonly Farmer dummyFarmer;

		public readonly NetObjectList<Item> items = new NetObjectList<Item>();
		public readonly NetBool usingTool = new NetBool(value: false);

		private bool canReleaseTool;
		private readonly NetInt currentToolIndex = new NetInt(0);
		//private readonly NetEvent0 fireToolEvent = new NetEvent0(interpolate: true);
		//private readonly NetEvent0 beginUsingToolEvent = new NetEvent0(interpolate: true);
		//private readonly NetEvent0 endUsingToolEvent = new NetEvent0(interpolate: true);

		public int CurrentToolIndex
		{
			get
			{
				return currentToolIndex;
			}
			set
			{
				currentToolIndex.Set(value);
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast:Netcode types shouldn't be implicitly converted", Justification = "<Pending>")]
        public Item CurrentItem
		{
			get
			{
				if ((int)currentToolIndex >= items.Count)
				{
					return null;
				}
				return items[currentToolIndex];
			}
		}

		public Tool CurrentTool
		{
			get
			{
				if (CurrentItem != null && CurrentItem is Tool)
				{
					return (Tool)CurrentItem;
				}
				return null;
			}
			set
			{
				while (CurrentToolIndex >= items.Count)
				{
					items.Add(null);
				}
				items[CurrentToolIndex] = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast:Netcode types shouldn't be implicitly converted", Justification = "<Pending>")]
		public Object ActiveObject
		{
			get
			{
				if ((int)currentToolIndex < items.Count && items[currentToolIndex] != null && items[currentToolIndex] is Object)
				{
					return (Object)items[currentToolIndex];
				}
				return null;
			}
			set
			{
				if (value == null)
				{
					removeItemFromInventory(ActiveObject);
				}
				else
				{
					addItemToInventory(value, CurrentToolIndex);
				}
			}
		}

		public bool UsingTool
		{
			get
			{
				return usingTool;
			}
			set
			{
				usingTool.Set(value);
			}
		}

		public BotFarmer(string name)
            :base (Game1.getCharacterFromName("Robin").Sprite, Vector2.Zero, 0, name)
        {
			dummyFarmer = Game1.player.CreateFakeEventFarmer();
			foreach (Item item in Game1.player.Items)
            {
				if (item == null) continue;
				Logger.Log($"adding {item.Name}");
				items.Add(item);
            }
			CurrentToolIndex = 1;
			//Logger.Log($"Current tool: {CurrentTool?.Name}");
			//Logger.Log($"Current item: {items[currentToolIndex]?.Name}, is obj: {items[currentToolIndex] is Object}");
			//Logger.Log($"Carrying: {IsCarrying()}");
			//Logger.Log($"Active object: {ActiveObject?.Name}");
		}

		public bool IsCarrying()
		{
			if (ActiveObject == null || Game1.eventUp || Game1.killScreen)
			{
				return false;
			}
			if (ActiveObject is Furniture)
			{
				return false;
			}
			return true;
		}

		public void UseTool()
		{
			if (CurrentTool != null)
			{
				CurrentTool.DoFunction(currentLocation, (int)GetToolLocation().X, (int)GetToolLocation().Y, 1, dummyFarmer);
				// TODO: tool hold?
			}
		}

		public void BeginUsingTool()
		{
			performBeginUsingTool();
		}

		private void performBeginUsingTool()
		{
			if (CurrentTool != null)
			{
				//CanMove = false;
                canReleaseTool = true;
                UsingTool = true;
                CurrentTool.beginUsing(base.currentLocation, (int)lastClick.X, (int)lastClick.Y, dummyFarmer);
			}
		}

		public void EndUsingTool()
		{
			performEndUsingTool();
		}

		private void performEndUsingTool()
		{
			if (CurrentTool != null)
			{
				UsingTool = false;
				CurrentTool.endUsing(currentLocation, dummyFarmer);
			}
		}

		public Item addItemToInventory(Item item, int position)
		{
			if (position >= 0 && position < items.Count)
			{
				if (items[position] == null)
				{
					items[position] = item;
					return null;
				}
				if (item != null && items[position].maximumStackSize() != -1 && items[position].Name.Equals(item.Name) && items[position].ParentSheetIndex == item.ParentSheetIndex && (!(item is Object) || !(items[position] is Object) || (item as Object).quality == (items[position] as Object).quality))
				{
					int stackLeft = items[position].addToStack(item);
					if (stackLeft <= 0)
					{
						return null;
					}
					item.Stack = stackLeft;
					return item;
				}
				Item result = items[position];
				items[position] = item;
				return result;
			}
			return item;
		}

		public void removeItemFromInventory(Item which)
		{
			int i = items.IndexOf(which);
			if (i >= 0 && i < items.Count)
			{
				items[i].actionWhenStopBeingHeld(dummyFarmer);
				items[i] = null;
			}
		}

		private void UpdateDummyFarmer()
        {
			dummyFarmer.Position = Position;
			dummyFarmer.FacingDirection = FacingDirection;
			dummyFarmer.canReleaseTool = canReleaseTool;
			dummyFarmer.UsingTool = UsingTool;
			dummyFarmer.Items.Clear();
			foreach (var item in items) dummyFarmer.Items.Add(item);
			dummyFarmer.CurrentToolIndex = CurrentToolIndex;
        }

		public override void draw(SpriteBatch b, float alpha = 1)
        {
            base.draw(b, alpha);
            UpdateDummyFarmer();
			if (ActiveObject != null && IsCarrying())
			{
				Game1.drawPlayerHeldObject(dummyFarmer);
			}
			if (UsingTool && CurrentTool != null && (!CurrentTool.Name.Equals("Seeds")))
			{
				Game1.drawTool(dummyFarmer);
			}
			Logger.Log("draw");
			//dummyFarmer.draw(b, alpha);
		}

        public override void update(GameTime time, GameLocation location, long id, bool move)
        {
            base.update(time, location, id, move);
			for (int i = items.Count - 1; i >= 0; i--)
			{
				if (items[i] != null && items[i] is Tool)
				{
					((Tool)items[i]).tickUpdate(time, dummyFarmer);
				}
			}
			Logger.Log("update");
			dummyFarmer.update(time, location, id, move);
		}

        //public BotFarmer(string name)
        //    : base(new FarmerSprite("Characters\\Farmer\\farmer_base"), Vector2.Zero, Game1.player.speed, name, new(), true)
        //{
        //    var random = Game1.random;
        //    changeShirt(random.Next(40));
        //    changePants(new Color(random.Next(255), random.Next(255), random.Next(255)));
        //    changeHairStyle(random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
        //    if (random.NextDouble() < 0.5)
        //    {
        //        changeHat(random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
        //    }
        //    changeHairColor(new Color(random.Next(255), random.Next(255), random.Next(255)));
        //    changeSkinColor(random.Next(16));
        //    FarmerSprite.setOwner(Game1.player);

        //    Game1.otherFarmers.Add(random.Next(), this);
        //}

        //public void setTilePosition(Point p)
        //{
        //    setTilePosition(p.X, p.Y);
        //}

        //public void setTilePosition(int x, int y)
        //{
        //    Position = new Vector2(x * 64, y * 64);
        //}
    }
}
