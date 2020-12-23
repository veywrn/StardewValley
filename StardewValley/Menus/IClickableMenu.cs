using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StardewValley.Menus
{
	[InstanceStatics]
	public abstract class IClickableMenu
	{
		public delegate void onExit();

		protected IClickableMenu _childMenu;

		protected IClickableMenu _parentMenu;

		public const int currency_g = 0;

		public const int currency_starTokens = 1;

		public const int currency_qiCoins = 2;

		public const int currency_qiGems = 4;

		public const int greyedOutSpotIndex = 57;

		public const int outerBorderWithUpArrow = 61;

		public const int lvlMarkerRedIndex = 54;

		public const int lvlMarkerGreyIndex = 55;

		public const int borderWithDownArrowIndex = 46;

		public const int borderWithUpArrowIndex = 47;

		public const int littleHeartIndex = 49;

		public const int uncheckedBoxIndex = 50;

		public const int checkedBoxIndex = 51;

		public const int presentIconIndex = 58;

		public const int itemSpotIndex = 10;

		public static int borderWidth = 40;

		public static int tabYPositionRelativeToMenuY = -48;

		public static int spaceToClearTopBorder = 96;

		public static int spaceToClearSideBorder = 16;

		public const int spaceBetweenTabs = 4;

		public int width;

		public int height;

		public int xPositionOnScreen;

		public int yPositionOnScreen;

		public int currentRegion;

		public Action<IClickableMenu> behaviorBeforeCleanup;

		public onExit exitFunction;

		public ClickableTextureComponent upperRightCloseButton;

		public bool destroy;

		public bool gamePadControlsImplemented;

		protected int _dependencies;

		public List<ClickableComponent> allClickableComponents;

		public ClickableComponent currentlySnappedComponent;

		public IClickableMenu()
		{
		}

		public IClickableMenu(int x, int y, int width, int height, bool showUpperRightCloseButton = false)
		{
			Game1.mouseCursorTransparency = 1f;
			initialize(x, y, width, height, showUpperRightCloseButton);
			if (Game1.gameMode == 3 && Game1.player != null && !Game1.eventUp)
			{
				Game1.player.Halt();
			}
		}

		public void initialize(int x, int y, int width, int height, bool showUpperRightCloseButton = false)
		{
			if (Game1.player != null && !Game1.player.UsingTool && !Game1.eventUp)
			{
				Game1.player.forceCanMove();
			}
			xPositionOnScreen = x;
			yPositionOnScreen = y;
			this.width = width;
			this.height = height;
			if (showUpperRightCloseButton)
			{
				upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
			}
			for (int i = 0; i < 4; i++)
			{
				Game1.directionKeyPolling[i] = 250;
			}
		}

		public virtual bool HasFocus()
		{
			return Game1.activeClickableMenu == this;
		}

		public IClickableMenu GetChildMenu()
		{
			return _childMenu;
		}

		public IClickableMenu GetParentMenu()
		{
			return _parentMenu;
		}

		public void SetChildMenu(IClickableMenu menu)
		{
			_childMenu = menu;
			if (_childMenu != null)
			{
				_childMenu._parentMenu = this;
			}
		}

		public void AddDependency()
		{
			_dependencies++;
		}

		public void RemoveDependency()
		{
			_dependencies--;
			if (_dependencies <= 0 && Game1.activeClickableMenu != this && TitleMenu.subMenu != this && this is IDisposable)
			{
				(this as IDisposable).Dispose();
			}
		}

		public bool HasDependencies()
		{
			return _dependencies > 0;
		}

		public virtual bool areGamePadControlsImplemented()
		{
			return false;
		}

		public ClickableComponent getLastClickableComponentInThisListThatContainsThisXCoord(List<ClickableComponent> ccList, int xCoord)
		{
			for (int i = ccList.Count - 1; i >= 0; i--)
			{
				if (ccList[i].bounds.Contains(xCoord, ccList[i].bounds.Center.Y))
				{
					return ccList[i];
				}
			}
			return null;
		}

		public ClickableComponent getFirstClickableComponentInThisListThatContainsThisXCoord(List<ClickableComponent> ccList, int xCoord)
		{
			for (int i = 0; i < ccList.Count; i++)
			{
				if (ccList[i].bounds.Contains(xCoord, ccList[i].bounds.Center.Y))
				{
					return ccList[i];
				}
			}
			return null;
		}

		public ClickableComponent getLastClickableComponentInThisListThatContainsThisYCoord(List<ClickableComponent> ccList, int yCoord)
		{
			for (int i = ccList.Count - 1; i >= 0; i--)
			{
				if (ccList[i].bounds.Contains(ccList[i].bounds.Center.X, yCoord))
				{
					return ccList[i];
				}
			}
			return null;
		}

		public ClickableComponent getFirstClickableComponentInThisListThatContainsThisYCoord(List<ClickableComponent> ccList, int yCoord)
		{
			for (int i = 0; i < ccList.Count; i++)
			{
				if (ccList[i].bounds.Contains(ccList[i].bounds.Center.X, yCoord))
				{
					return ccList[i];
				}
			}
			return null;
		}

		public virtual void receiveGamePadButton(Buttons b)
		{
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && b != Buttons.A)
			{
				_ = 16384;
			}
		}

		public void drawMouse(SpriteBatch b, bool ignore_transparency = false, int cursor = -1)
		{
			if (!Game1.options.hardwareCursor)
			{
				float transparency = Game1.mouseCursorTransparency;
				if (ignore_transparency)
				{
					transparency = 1f;
				}
				if (cursor < 0)
				{
					cursor = ((Game1.options.snappyMenus && Game1.options.gamepadControls) ? 44 : 0);
				}
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, cursor, 16, 16), Color.White * transparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
		}

		public void populateClickableComponentList()
		{
			allClickableComponents = new List<ClickableComponent>();
			FieldInfo[] fields = GetType().GetFields();
			foreach (FieldInfo f in fields)
			{
				if (f.GetCustomAttributes(typeof(SkipForClickableAggregation), inherit: true).Length != 0 || f.DeclaringType == typeof(IClickableMenu))
				{
					continue;
				}
				if (f.FieldType.IsSubclassOf(typeof(ClickableComponent)) || f.FieldType == typeof(ClickableComponent))
				{
					if (f.GetValue(this) != null)
					{
						allClickableComponents.Add((ClickableComponent)f.GetValue(this));
					}
				}
				else if (f.FieldType == typeof(List<ClickableComponent>))
				{
					List<ClickableComponent> l4 = (List<ClickableComponent>)f.GetValue(this);
					if (l4 == null)
					{
						continue;
					}
					for (int i3 = l4.Count - 1; i3 >= 0; i3--)
					{
						if (l4[i3] != null)
						{
							allClickableComponents.Add(l4[i3]);
						}
					}
				}
				else if (f.FieldType == typeof(List<ClickableTextureComponent>))
				{
					List<ClickableTextureComponent> l3 = (List<ClickableTextureComponent>)f.GetValue(this);
					if (l3 == null)
					{
						continue;
					}
					for (int i2 = l3.Count - 1; i2 >= 0; i2--)
					{
						if (l3[i2] != null)
						{
							allClickableComponents.Add(l3[i2]);
						}
					}
				}
				else if (f.FieldType == typeof(List<ClickableAnimatedComponent>))
				{
					List<ClickableAnimatedComponent> n = (List<ClickableAnimatedComponent>)f.GetValue(this);
					for (int m = n.Count - 1; m >= 0; m--)
					{
						if (n[m] != null)
						{
							allClickableComponents.Add(n[m]);
						}
					}
				}
				else if (f.FieldType == typeof(List<Bundle>))
				{
					List<Bundle> k = (List<Bundle>)f.GetValue(this);
					for (int j = k.Count - 1; j >= 0; j--)
					{
						if (k[j] != null)
						{
							allClickableComponents.Add(k[j]);
						}
					}
				}
				else if (f.FieldType == typeof(InventoryMenu))
				{
					allClickableComponents.AddRange(((InventoryMenu)f.GetValue(this)).inventory);
					allClickableComponents.Add(((InventoryMenu)f.GetValue(this)).dropItemInvisibleButton);
				}
				else if (f.FieldType == typeof(List<Dictionary<ClickableTextureComponent, CraftingRecipe>>))
				{
					foreach (Dictionary<ClickableTextureComponent, CraftingRecipe> d in (List<Dictionary<ClickableTextureComponent, CraftingRecipe>>)f.GetValue(this))
					{
						allClickableComponents.AddRange(d.Keys);
					}
				}
				else if (f.FieldType == typeof(Dictionary<int, List<List<ClickableTextureComponent>>>))
				{
					foreach (List<List<ClickableTextureComponent>> value in ((Dictionary<int, List<List<ClickableTextureComponent>>>)f.GetValue(this)).Values)
					{
						foreach (List<ClickableTextureComponent> l2 in value)
						{
							allClickableComponents.AddRange(l2);
						}
					}
				}
				else if (f.FieldType == typeof(Dictionary<int, ClickableTextureComponent>))
				{
					foreach (ClickableTextureComponent i in ((Dictionary<int, ClickableTextureComponent>)f.GetValue(this)).Values)
					{
						allClickableComponents.Add(i);
					}
				}
			}
			GameMenu game_menu;
			if ((game_menu = (Game1.activeClickableMenu as GameMenu)) != null && this == game_menu.GetCurrentPage())
			{
				game_menu.AddTabsToClickableComponents(this);
			}
			if (upperRightCloseButton != null)
			{
				allClickableComponents.Add(upperRightCloseButton);
			}
		}

		public virtual void applyMovementKey(int direction)
		{
			if (allClickableComponents == null)
			{
				populateClickableComponentList();
			}
			moveCursorInDirection(direction);
		}

		public virtual void snapToDefaultClickableComponent()
		{
		}

		public void applyMovementKey(Keys key)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
			{
				applyMovementKey(0);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
			{
				applyMovementKey(1);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
			{
				applyMovementKey(2);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				applyMovementKey(3);
			}
		}

		public virtual void setCurrentlySnappedComponentTo(int id)
		{
			currentlySnappedComponent = getComponentWithID(id);
		}

		public void moveCursorInDirection(int direction)
		{
			if (currentlySnappedComponent == null && allClickableComponents != null && allClickableComponents.Count() > 0)
			{
				snapToDefaultClickableComponent();
				if (currentlySnappedComponent == null)
				{
					currentlySnappedComponent = allClickableComponents.First();
				}
			}
			if (currentlySnappedComponent == null)
			{
				return;
			}
			ClickableComponent old = currentlySnappedComponent;
			switch (direction)
			{
			case 0:
				if (currentlySnappedComponent.upNeighborID == -99999)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent.upNeighborID == -99998)
				{
					automaticSnapBehavior(0, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else if (currentlySnappedComponent.upNeighborID == -7777)
				{
					customSnapBehavior(0, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else
				{
					currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.upNeighborID);
				}
				if (currentlySnappedComponent != null && (old == null || (old.upNeighborID != -7777 && old.upNeighborID != -99998)) && !currentlySnappedComponent.downNeighborImmutable && !currentlySnappedComponent.fullyImmutable)
				{
					currentlySnappedComponent.downNeighborID = old.myID;
				}
				if (currentlySnappedComponent == null)
				{
					noSnappedComponentFound(0, old.region, old.myID);
				}
				break;
			case 1:
				if (currentlySnappedComponent.rightNeighborID == -99999)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent.rightNeighborID == -99998)
				{
					automaticSnapBehavior(1, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else if (currentlySnappedComponent.rightNeighborID == -7777)
				{
					customSnapBehavior(1, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else
				{
					currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.rightNeighborID);
				}
				if (currentlySnappedComponent != null && (old == null || (old.rightNeighborID != -7777 && old.rightNeighborID != -99998)) && !currentlySnappedComponent.leftNeighborImmutable && !currentlySnappedComponent.fullyImmutable)
				{
					currentlySnappedComponent.leftNeighborID = old.myID;
				}
				if (currentlySnappedComponent == null && old.tryDefaultIfNoRightNeighborExists)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent == null)
				{
					noSnappedComponentFound(1, old.region, old.myID);
				}
				break;
			case 2:
				if (currentlySnappedComponent.downNeighborID == -99999)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent.downNeighborID == -99998)
				{
					automaticSnapBehavior(2, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else if (currentlySnappedComponent.downNeighborID == -7777)
				{
					customSnapBehavior(2, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else
				{
					currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.downNeighborID);
				}
				if (currentlySnappedComponent != null && (old == null || (old.downNeighborID != -7777 && old.downNeighborID != -99998)) && !currentlySnappedComponent.upNeighborImmutable && !currentlySnappedComponent.fullyImmutable)
				{
					currentlySnappedComponent.upNeighborID = old.myID;
				}
				if (currentlySnappedComponent == null && old.tryDefaultIfNoDownNeighborExists)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent == null)
				{
					noSnappedComponentFound(2, old.region, old.myID);
				}
				break;
			case 3:
				if (currentlySnappedComponent.leftNeighborID == -99999)
				{
					snapToDefaultClickableComponent();
				}
				else if (currentlySnappedComponent.leftNeighborID == -99998)
				{
					automaticSnapBehavior(3, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else if (currentlySnappedComponent.leftNeighborID == -7777)
				{
					customSnapBehavior(3, currentlySnappedComponent.region, currentlySnappedComponent.myID);
				}
				else
				{
					currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.leftNeighborID);
				}
				if (currentlySnappedComponent != null && (old == null || (old.leftNeighborID != -7777 && old.leftNeighborID != -99998)) && !currentlySnappedComponent.rightNeighborImmutable && !currentlySnappedComponent.fullyImmutable)
				{
					currentlySnappedComponent.rightNeighborID = old.myID;
				}
				if (currentlySnappedComponent == null)
				{
					noSnappedComponentFound(3, old.region, old.myID);
				}
				break;
			}
			if (currentlySnappedComponent != null && old != null && currentlySnappedComponent.region != old.region)
			{
				actionOnRegionChange(old.region, currentlySnappedComponent.region);
			}
			if (currentlySnappedComponent == null)
			{
				currentlySnappedComponent = old;
			}
			snapCursorToCurrentSnappedComponent();
			if (currentlySnappedComponent != old)
			{
				Game1.playSound("shiny4");
			}
		}

		public virtual void snapCursorToCurrentSnappedComponent()
		{
			if (currentlySnappedComponent != null)
			{
				Game1.setMousePosition(currentlySnappedComponent.bounds.Right - currentlySnappedComponent.bounds.Width / 4, currentlySnappedComponent.bounds.Bottom - currentlySnappedComponent.bounds.Height / 4, ui_scale: true);
			}
		}

		protected virtual void noSnappedComponentFound(int direction, int oldRegion, int oldID)
		{
		}

		protected virtual void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
		}

		public virtual bool IsActive()
		{
			if (_parentMenu == null)
			{
				return this == Game1.activeClickableMenu;
			}
			IClickableMenu root = _parentMenu;
			while (root != null && root._parentMenu != null)
			{
				root = root._parentMenu;
			}
			return root == Game1.activeClickableMenu;
		}

		public virtual void automaticSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (currentlySnappedComponent == null)
			{
				snapToDefaultClickableComponent();
				return;
			}
			Vector2 snap_direction = Vector2.Zero;
			switch (direction)
			{
			case 3:
				snap_direction.X = -1f;
				snap_direction.Y = 0f;
				break;
			case 1:
				snap_direction.X = 1f;
				snap_direction.Y = 0f;
				break;
			case 0:
				snap_direction.X = 0f;
				snap_direction.Y = -1f;
				break;
			case 2:
				snap_direction.X = 0f;
				snap_direction.Y = 1f;
				break;
			}
			float closest_distance = -1f;
			ClickableComponent closest_component_in_direction = null;
			for (int i = 0; i < allClickableComponents.Count; i++)
			{
				ClickableComponent other_component = allClickableComponents[i];
				if ((other_component.leftNeighborID == -1 && other_component.rightNeighborID == -1 && other_component.upNeighborID == -1 && other_component.downNeighborID == -1) || other_component.myID == -500 || !IsAutomaticSnapValid(direction, currentlySnappedComponent, other_component) || !other_component.visible || other_component == upperRightCloseButton || other_component == currentlySnappedComponent)
				{
					continue;
				}
				Vector2 offset = new Vector2(other_component.bounds.Center.X - currentlySnappedComponent.bounds.Center.X, other_component.bounds.Center.Y - currentlySnappedComponent.bounds.Center.Y);
				Vector2 normalized_offset = new Vector2(offset.X, offset.Y);
				normalized_offset.Normalize();
				float dot = Vector2.Dot(snap_direction, normalized_offset);
				if (!(dot > 0.01f))
				{
					continue;
				}
				float score = Vector2.DistanceSquared(Vector2.Zero, offset);
				bool close_enough = false;
				switch (direction)
				{
				case 0:
				case 2:
					if (Math.Abs(offset.X) < 32f)
					{
						close_enough = true;
					}
					break;
				case 1:
				case 3:
					if (Math.Abs(offset.Y) < 32f)
					{
						close_enough = true;
					}
					break;
				}
				if (_ShouldAutoSnapPrioritizeAlignedElements() && ((dot > 0.99999f) | close_enough))
				{
					score *= 0.01f;
				}
				if (closest_distance == -1f || score < closest_distance)
				{
					closest_distance = score;
					closest_component_in_direction = other_component;
				}
			}
			if (closest_component_in_direction != null)
			{
				currentlySnappedComponent = closest_component_in_direction;
			}
		}

		protected virtual bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			return true;
		}

		public virtual bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return true;
		}

		protected virtual void actionOnRegionChange(int oldRegion, int newRegion)
		{
		}

		public ClickableComponent getComponentWithID(int id)
		{
			if (id == -500)
			{
				return null;
			}
			if (allClickableComponents != null)
			{
				for (int j = 0; j < allClickableComponents.Count; j++)
				{
					if (allClickableComponents[j] != null && allClickableComponents[j].myID == id && allClickableComponents[j].visible)
					{
						return allClickableComponents[j];
					}
				}
				for (int i = 0; i < allClickableComponents.Count; i++)
				{
					if (allClickableComponents[i] != null && allClickableComponents[i].myAlternateID == id && allClickableComponents[i].visible)
					{
						return allClickableComponents[i];
					}
				}
			}
			return null;
		}

		public void initializeUpperRightCloseButton()
		{
			upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
		}

		public virtual void drawBackground(SpriteBatch b)
		{
			if (this is ShopMenu)
			{
				for (int x = 0; x < Game1.uiViewport.Width; x += 400)
				{
					for (int y = 0; y < Game1.uiViewport.Height; y += 384)
					{
						b.Draw(Game1.mouseCursors, new Vector2(x, y), new Rectangle(527, 0, 100, 96), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
					}
				}
				return;
			}
			if (Game1.isDarkOut())
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle(639, 858, 1, 144), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			else if (Game1.IsRainingHere())
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle(640, 858, 1, 184), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle(639 + Utility.getSeasonNumber(Game1.currentSeason), 1051, 1, 400), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			b.Draw(Game1.mouseCursors, new Vector2(-120f, Game1.uiViewport.Height - 592), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1035 : ((Game1.isRaining || Game1.isDarkOut()) ? 886 : 737), 639, 148), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
			b.Draw(Game1.mouseCursors, new Vector2(2436f, Game1.uiViewport.Height - 592), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1035 : ((Game1.isRaining || Game1.isDarkOut()) ? 886 : 737), 639, 148), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
			if (Game1.isRaining)
			{
				b.Draw(Game1.staminaRect, Utility.xTileToMicrosoftRectangle(Game1.uiViewport), Color.Blue * 0.2f);
			}
		}

		public virtual bool showWithoutTransparencyIfOptionIsSet()
		{
			if (this is GameMenu || this is ShopMenu || this is WheelSpinGame || this is ItemGrabMenu)
			{
				return true;
			}
			return false;
		}

		public virtual void clickAway()
		{
		}

		public virtual void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			xPositionOnScreen = (int)((float)(newBounds.Width - width) * ((float)xPositionOnScreen / (float)(oldBounds.Width - width)));
			yPositionOnScreen = (int)((float)(newBounds.Height - height) * ((float)yPositionOnScreen / (float)(oldBounds.Height - height)));
		}

		public virtual void setUpForGamePadMode()
		{
		}

		public virtual bool shouldClampGamePadCursor()
		{
			return false;
		}

		public virtual void releaseLeftClick(int x, int y)
		{
		}

		public virtual void leftClickHeld(int x, int y)
		{
		}

		public virtual void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
			{
				if (playSound)
				{
					Game1.playSound("bigDeSelect");
				}
				exitThisMenu();
			}
		}

		public virtual bool overrideSnappyMenuCursorMovementBan()
		{
			return false;
		}

		public virtual void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public virtual void receiveKeyPress(Keys key)
		{
			if (key != 0)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
				{
					exitThisMenu();
				}
				else if (Game1.options.snappyMenus && Game1.options.gamepadControls && !overrideSnappyMenuCursorMovementBan())
				{
					applyMovementKey(key);
				}
			}
		}

		public virtual void gamePadButtonHeld(Buttons b)
		{
		}

		public virtual ClickableComponent getCurrentlySnappedComponent()
		{
			return currentlySnappedComponent;
		}

		public virtual void receiveScrollWheelAction(int direction)
		{
		}

		public virtual void performHoverAction(int x, int y)
		{
			if (upperRightCloseButton != null)
			{
				upperRightCloseButton.tryHover(x, y, 0.5f);
			}
		}

		public virtual void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
		{
			if (upperRightCloseButton != null && shouldDrawCloseButton())
			{
				upperRightCloseButton.draw(b);
			}
		}

		public virtual void draw(SpriteBatch b)
		{
			if (upperRightCloseButton != null && shouldDrawCloseButton())
			{
				upperRightCloseButton.draw(b);
			}
		}

		public virtual bool isWithinBounds(int x, int y)
		{
			if (x - xPositionOnScreen < width && x - xPositionOnScreen >= 0 && y - yPositionOnScreen < height)
			{
				return y - yPositionOnScreen >= 0;
			}
			return false;
		}

		public virtual void update(GameTime time)
		{
		}

		protected virtual void cleanupBeforeExit()
		{
		}

		public virtual bool shouldDrawCloseButton()
		{
			return true;
		}

		public void exitThisMenuNoSound()
		{
			exitThisMenu(playSound: false);
		}

		public void exitThisMenu(bool playSound = true)
		{
			if (behaviorBeforeCleanup != null)
			{
				behaviorBeforeCleanup(this);
			}
			cleanupBeforeExit();
			if (playSound)
			{
				Game1.playSound("bigDeSelect");
			}
			if (this == Game1.activeClickableMenu)
			{
				Game1.exitActiveMenu();
			}
			else if (Game1.activeClickableMenu is GameMenu && (Game1.activeClickableMenu as GameMenu).GetCurrentPage() == this)
			{
				Game1.exitActiveMenu();
			}
			if (_parentMenu != null)
			{
				IClickableMenu parentMenu = _parentMenu;
				_parentMenu = null;
				parentMenu.SetChildMenu(null);
			}
			if (exitFunction != null)
			{
				onExit onExit = exitFunction;
				exitFunction = null;
				onExit();
			}
		}

		public virtual bool autoCenterMouseCursorForGamepad()
		{
			return true;
		}

		public virtual void emergencyShutDown()
		{
		}

		public virtual bool readyToClose()
		{
			return true;
		}

		protected void drawHorizontalPartition(SpriteBatch b, int yPosition, bool small = false, int red = -1, int green = -1, int blue = -1)
		{
			Color tint = (red == -1) ? Color.White : new Color(red, green, blue);
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			if (small)
			{
				b.Draw(texture, new Rectangle(xPositionOnScreen + 32, yPosition, width - 64, 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 25), tint);
				return;
			}
			b.Draw(texture, new Vector2(xPositionOnScreen, yPosition), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 4), tint);
			b.Draw(texture, new Rectangle(xPositionOnScreen + 64, yPosition, width - 128, 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 6), tint);
			b.Draw(texture, new Vector2(xPositionOnScreen + width - 64, yPosition), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 7), tint);
		}

		protected void drawVerticalPartition(SpriteBatch b, int xPosition, bool small = false, int red = -1, int green = -1, int blue = -1)
		{
			Color tint = (red == -1) ? Color.White : new Color(red, green, blue);
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			if (small)
			{
				b.Draw(texture, new Rectangle(xPosition, yPositionOnScreen + 64 + 32, 64, height - 128), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 26), tint);
				return;
			}
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 1), tint);
			b.Draw(texture, new Rectangle(xPosition, yPositionOnScreen + 128, 64, height - 192), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 5), tint);
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + height - 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 13), tint);
		}

		protected void drawVerticalIntersectingPartition(SpriteBatch b, int xPosition, int yPosition, int red = -1, int green = -1, int blue = -1)
		{
			Color tint = (red == -1) ? Color.White : new Color(red, green, blue);
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			b.Draw(texture, new Vector2(xPosition, yPosition), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 59), tint);
			b.Draw(texture, new Rectangle(xPosition, yPosition + 64, 64, yPositionOnScreen + height - 64 - yPosition - 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 63), tint);
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + height - 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 62), tint);
		}

		protected void drawVerticalUpperIntersectingPartition(SpriteBatch b, int xPosition, int partitionHeight, int red = -1, int green = -1, int blue = -1)
		{
			Color tint = (red == -1) ? Color.White : new Color(red, green, blue);
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 44), tint);
			b.Draw(texture, new Rectangle(xPosition, yPositionOnScreen + 128, 64, partitionHeight - 32), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 63), tint);
			b.Draw(texture, new Vector2(xPosition, yPositionOnScreen + partitionHeight + 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 39), tint);
		}

		public static void drawTextureBox(SpriteBatch b, int x, int y, int width, int height, Color color)
		{
			drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, color);
		}

		public static void drawTextureBox(SpriteBatch b, Texture2D texture, Rectangle sourceRect, int x, int y, int width, int height, Color color, float scale = 1f, bool drawShadow = true, float draw_layer = -1f)
		{
			int cornerSize = sourceRect.Width / 3;
			float shadow_layer = draw_layer - 0.03f;
			if (draw_layer < 0f)
			{
				draw_layer = 0.8f - (float)y * 1E-06f;
				shadow_layer = 0.77f;
			}
			if (drawShadow)
			{
				b.Draw(texture, new Vector2(x + width - (int)((float)cornerSize * scale) - 8, y + 8), new Rectangle(sourceRect.X + cornerSize * 2, sourceRect.Y, cornerSize, cornerSize), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Vector2(x - 8, y + height - (int)((float)cornerSize * scale) + 8), new Rectangle(sourceRect.X, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Vector2(x + width - (int)((float)cornerSize * scale) - 8, y + height - (int)((float)cornerSize * scale) + 8), new Rectangle(sourceRect.X + cornerSize * 2, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle(x + (int)((float)cornerSize * scale) - 8, y + 8, width - (int)((float)cornerSize * scale) * 2, (int)((float)cornerSize * scale)), new Rectangle(sourceRect.X + cornerSize, sourceRect.Y, cornerSize, cornerSize), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle(x + (int)((float)cornerSize * scale) - 8, y + height - (int)((float)cornerSize * scale) + 8, width - (int)((float)cornerSize * scale) * 2, (int)((float)cornerSize * scale)), new Rectangle(sourceRect.X + cornerSize, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle(x - 8, y + (int)((float)cornerSize * scale) + 8, (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale) * 2), new Rectangle(sourceRect.X, cornerSize + sourceRect.Y, cornerSize, cornerSize), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle(x + width - (int)((float)cornerSize * scale) - 8, y + (int)((float)cornerSize * scale) + 8, (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale) * 2), new Rectangle(sourceRect.X + cornerSize * 2, cornerSize + sourceRect.Y, cornerSize, cornerSize), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle((int)((float)cornerSize * scale / 2f) + x - 8, (int)((float)cornerSize * scale / 2f) + y + 8, width - (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale)), new Rectangle(cornerSize + sourceRect.X, cornerSize + sourceRect.Y, cornerSize, cornerSize), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
			}
			b.Draw(texture, new Rectangle((int)((float)cornerSize * scale) + x, (int)((float)cornerSize * scale) + y, width - (int)((float)cornerSize * scale * 2f), height - (int)((float)cornerSize * scale * 2f)), new Rectangle(cornerSize + sourceRect.X, cornerSize + sourceRect.Y, cornerSize, cornerSize), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Vector2(x, y), new Rectangle(sourceRect.X, sourceRect.Y, cornerSize, cornerSize), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Vector2(x + width - (int)((float)cornerSize * scale), y), new Rectangle(sourceRect.X + cornerSize * 2, sourceRect.Y, cornerSize, cornerSize), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Vector2(x, y + height - (int)((float)cornerSize * scale)), new Rectangle(sourceRect.X, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Vector2(x + width - (int)((float)cornerSize * scale), y + height - (int)((float)cornerSize * scale)), new Rectangle(sourceRect.X + cornerSize * 2, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Rectangle(x + (int)((float)cornerSize * scale), y, width - (int)((float)cornerSize * scale) * 2, (int)((float)cornerSize * scale)), new Rectangle(sourceRect.X + cornerSize, sourceRect.Y, cornerSize, cornerSize), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Rectangle(x + (int)((float)cornerSize * scale), y + height - (int)((float)cornerSize * scale), width - (int)((float)cornerSize * scale) * 2, (int)((float)cornerSize * scale)), new Rectangle(sourceRect.X + cornerSize, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Rectangle(x, y + (int)((float)cornerSize * scale), (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale) * 2), new Rectangle(sourceRect.X, cornerSize + sourceRect.Y, cornerSize, cornerSize), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Rectangle(x + width - (int)((float)cornerSize * scale), y + (int)((float)cornerSize * scale), (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale) * 2), new Rectangle(sourceRect.X + cornerSize * 2, cornerSize + sourceRect.Y, cornerSize, cornerSize), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
		}

		public void drawBorderLabel(SpriteBatch b, string text, SpriteFont font, int x, int y)
		{
			int width = (int)font.MeasureString(text).X;
			y += 52;
			b.Draw(Game1.mouseCursors, new Vector2(x, y), new Rectangle(256, 267, 6, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(x + 24, y), new Rectangle(262, 267, 1, 16), Color.White, 0f, Vector2.Zero, new Vector2(width, 4f), SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(x + 24 + width, y), new Rectangle(263, 267, 6, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			Utility.drawTextWithShadow(b, text, font, new Vector2(x + 24, y + 20), Game1.textColor);
		}

		public static void drawToolTip(SpriteBatch b, string hoverText, string hoverTitle, Item hoveredItem, bool heldItem = false, int healAmountToDisplay = -1, int currencySymbol = 0, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, CraftingRecipe craftingIngredients = null, int moneyAmountToShowAtBottom = -1)
		{
			bool edibleItem = hoveredItem != null && hoveredItem is Object && (int)(hoveredItem as Object).edibility != -300;
			int width2 = Math.Max((healAmountToDisplay != -1) ? ((int)Game1.smallFont.MeasureString(healAmountToDisplay + "+ Energy").X + 32) : 0, Math.Max((int)Game1.smallFont.MeasureString(hoverText).X, (hoverTitle != null) ? ((int)Game1.dialogueFont.MeasureString(hoverTitle).X) : 0)) + 32;
			if (edibleItem)
			{
				int maxStat = 9999;
				int buffer = 92;
				width2 = (int)Math.Max(width2, Math.Max(Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Energy", maxStat)).X + (float)buffer, Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Health", maxStat)).X + (float)buffer));
			}
			drawHoverText(b, hoverText, Game1.smallFont, heldItem ? 40 : 0, heldItem ? 40 : 0, moneyAmountToShowAtBottom, hoverTitle, edibleItem ? ((int)(hoveredItem as Object).edibility) : (-1), (edibleItem && Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/').Length > 7) ? hoveredItem.ModifyItemBuffs(Game1.objectInformation[(hoveredItem as Object).parentSheetIndex].Split('/')[7].Split(' ')) : null, hoveredItem, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, -1, -1, 1f, craftingIngredients);
		}

		public static void drawHoverText(SpriteBatch b, string text, SpriteFont font, int xOffset = 0, int yOffset = 0, int moneyAmountToDisplayAtBottom = -1, string boldTitleText = null, int healAmountToDisplay = -1, string[] buffIconsToDisplay = null, Item hoveredItem = null, int currencySymbol = 0, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, int overrideX = -1, int overrideY = -1, float alpha = 1f, CraftingRecipe craftingIngredients = null, IList<Item> additional_craft_materials = null)
		{
			StringBuilder sb = null;
			if (text != null)
			{
				sb = new StringBuilder(text);
			}
			drawHoverText(b, sb, font, xOffset, yOffset, moneyAmountToDisplayAtBottom, boldTitleText, healAmountToDisplay, buffIconsToDisplay, hoveredItem, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, overrideX, overrideY, alpha, craftingIngredients, additional_craft_materials);
		}

		public static void drawHoverText(SpriteBatch b, StringBuilder text, SpriteFont font, int xOffset = 0, int yOffset = 0, int moneyAmountToDisplayAtBottom = -1, string boldTitleText = null, int healAmountToDisplay = -1, string[] buffIconsToDisplay = null, Item hoveredItem = null, int currencySymbol = 0, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, int overrideX = -1, int overrideY = -1, float alpha = 1f, CraftingRecipe craftingIngredients = null, IList<Item> additional_craft_materials = null)
		{
			if (text == null || text.Length == 0)
			{
				return;
			}
			string bold_title_subtext = null;
			if (boldTitleText != null && boldTitleText.Length == 0)
			{
				boldTitleText = null;
			}
			int width = Math.Max((healAmountToDisplay != -1) ? ((int)font.MeasureString(healAmountToDisplay + "+ Energy" + 32).X) : 0, Math.Max((int)font.MeasureString(text).X, (boldTitleText != null) ? ((int)Game1.dialogueFont.MeasureString(boldTitleText).X) : 0)) + 32;
			int height2 = Math.Max(20 * 3, (int)font.MeasureString(text).Y + 32 + (int)((moneyAmountToDisplayAtBottom > -1) ? (font.MeasureString(string.Concat(moneyAmountToDisplayAtBottom)).Y + 4f) : 8f) + (int)((boldTitleText != null) ? (Game1.dialogueFont.MeasureString(boldTitleText).Y + 16f) : 0f));
			if (extraItemToShowIndex != -1)
			{
				string[] split = Game1.objectInformation[extraItemToShowIndex].Split('/');
				string objName = split[0];
				if (LocalizedContentManager.CurrentLanguageCode != 0)
				{
					objName = split[4];
				}
				string requirement2 = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", extraItemToShowAmount, (extraItemToShowAmount > 1) ? Lexicon.makePlural(objName) : objName);
				int spriteWidth = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16).Width * 2 * 4;
				width = Math.Max(width, spriteWidth + (int)font.MeasureString(requirement2).X);
			}
			if (buffIconsToDisplay != null)
			{
				for (int k = 0; k < buffIconsToDisplay.Length; k++)
				{
					if (!buffIconsToDisplay[k].Equals("0"))
					{
						height2 += 34;
					}
				}
				height2 += 4;
			}
			if (craftingIngredients != null && Game1.options.showAdvancedCraftingInformation && craftingIngredients.getCraftCountText() != null)
			{
				height2 += (int)font.MeasureString("T").Y;
			}
			string categoryName = null;
			if (hoveredItem != null)
			{
				height2 += 68 * hoveredItem.attachmentSlots();
				categoryName = hoveredItem.getCategoryName();
				if (categoryName.Length > 0)
				{
					width = Math.Max(width, (int)font.MeasureString(categoryName).X + 32);
					height2 += (int)font.MeasureString("T").Y;
				}
				int maxStat = 9999;
				int buffer = 92;
				Point p = hoveredItem.getExtraSpaceNeededForTooltipSpecialIcons(font, width, buffer, height2, text, boldTitleText, moneyAmountToDisplayAtBottom);
				width = ((p.X != 0) ? p.X : width);
				height2 = ((p.Y != 0) ? p.Y : height2);
				if (hoveredItem is MeleeWeapon && (hoveredItem as MeleeWeapon).GetTotalForgeLevels() > 0)
				{
					height2 += (int)font.MeasureString("T").Y;
				}
				if (hoveredItem is MeleeWeapon && (hoveredItem as MeleeWeapon).GetEnchantmentLevel<GalaxySoulEnchantment>() > 0)
				{
					height2 += (int)font.MeasureString("T").Y;
				}
				if (hoveredItem is Object && (int)(hoveredItem as Object).edibility != -300)
				{
					height2 = ((healAmountToDisplay == -1) ? (height2 + 40) : (height2 + 40 * ((healAmountToDisplay <= 0) ? 1 : 2)));
					healAmountToDisplay = (hoveredItem as Object).staminaRecoveredOnConsumption();
					width = (int)Math.Max(width, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Energy", maxStat)).X + (float)buffer, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Health", maxStat)).X + (float)buffer));
				}
				if (buffIconsToDisplay != null)
				{
					for (int j = 0; j < buffIconsToDisplay.Length; j++)
					{
						if (!buffIconsToDisplay[j].Equals("0") && j <= 11)
						{
							width = (int)Math.Max(width, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + j, maxStat)).X + (float)buffer);
						}
					}
				}
			}
			Vector2 small_text_size = Vector2.Zero;
			if (craftingIngredients != null)
			{
				if (Game1.options.showAdvancedCraftingInformation)
				{
					int craftable_count = craftingIngredients.getCraftableCount(additional_craft_materials);
					if (craftable_count > 1)
					{
						bold_title_subtext = " (" + craftable_count + ")";
						small_text_size = Game1.smallFont.MeasureString(bold_title_subtext);
					}
				}
				width = (int)Math.Max(Game1.dialogueFont.MeasureString(boldTitleText).X + small_text_size.X + 12f, 384f);
				height2 += craftingIngredients.getDescriptionHeight(width - 8) + ((healAmountToDisplay == -1) ? (-32) : 0);
			}
			else if (bold_title_subtext != null && boldTitleText != null)
			{
				small_text_size = Game1.smallFont.MeasureString(bold_title_subtext);
				width = (int)Math.Max(width, Game1.dialogueFont.MeasureString(boldTitleText).X + small_text_size.X + 12f);
			}
			int x = Game1.getOldMouseX() + 32 + xOffset;
			int y4 = Game1.getOldMouseY() + 32 + yOffset;
			if (overrideX != -1)
			{
				x = overrideX;
			}
			if (overrideY != -1)
			{
				y4 = overrideY;
			}
			if (x + width > Utility.getSafeArea().Right)
			{
				x = Utility.getSafeArea().Right - width;
				y4 += 16;
			}
			if (y4 + height2 > Utility.getSafeArea().Bottom)
			{
				x += 16;
				if (x + width > Utility.getSafeArea().Right)
				{
					x = Utility.getSafeArea().Right - width;
				}
				y4 = Utility.getSafeArea().Bottom - height2;
			}
			drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y4, width + ((craftingIngredients != null) ? 21 : 0), height2, Color.White * alpha);
			if (boldTitleText != null)
			{
				Vector2 bold_text_size = Game1.dialogueFont.MeasureString(boldTitleText);
				drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y4, width + ((craftingIngredients != null) ? 21 : 0), (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)((hoveredItem != null && categoryName.Length > 0) ? font.MeasureString("asd").Y : 0f) - 4, Color.White * alpha, 1f, drawShadow: false);
				b.Draw(Game1.menuTexture, new Rectangle(x + 12, y4 + (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)((hoveredItem != null && categoryName.Length > 0) ? font.MeasureString("asd").Y : 0f) - 4, width - 4 * ((craftingIngredients != null) ? 1 : 6), 4), new Rectangle(44, 300, 4, 4), Color.White);
				b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, y4 + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, y4 + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, y4 + 16 + 4), Game1.textColor);
				if (bold_title_subtext != null)
				{
					Utility.drawTextWithShadow(b, bold_title_subtext, Game1.smallFont, new Vector2((float)(x + 16) + bold_text_size.X, (int)((float)(y4 + 16 + 4) + bold_text_size.Y / 2f - small_text_size.Y / 2f)), Game1.textColor);
				}
				y4 += (int)Game1.dialogueFont.MeasureString(boldTitleText).Y;
			}
			if (hoveredItem != null && categoryName.Length > 0)
			{
				y4 -= 4;
				Utility.drawTextWithShadow(b, categoryName, font, new Vector2(x + 16, y4 + 16 + 4), hoveredItem.getCategoryColor(), 1f, -1f, 2, 2);
				y4 += (int)font.MeasureString("T").Y + ((boldTitleText != null) ? 16 : 0) + 4;
				if (hoveredItem is Tool && (hoveredItem as Tool).GetTotalForgeLevels() > 0)
				{
					string forged_string2 = Game1.content.LoadString("Strings\\UI:Item_Tooltip_Forged");
					Utility.drawTextWithShadow(b, forged_string2, font, new Vector2(x + 16, y4 + 16 + 4), Color.DarkRed, 1f, -1f, 2, 2);
					int forges = (hoveredItem as Tool).GetTotalForgeLevels();
					if (forges < (hoveredItem as Tool).GetMaxForges() && !(hoveredItem as Tool).hasEnchantmentOfType<DiamondEnchantment>())
					{
						Utility.drawTextWithShadow(b, " (" + forges + "/" + (hoveredItem as Tool).GetMaxForges() + ")", font, new Vector2((float)(x + 16) + font.MeasureString(forged_string2).X, y4 + 16 + 4), Color.DimGray, 1f, -1f, 2, 2);
					}
					y4 += (int)font.MeasureString("T").Y;
				}
				if (hoveredItem is MeleeWeapon && (hoveredItem as MeleeWeapon).GetEnchantmentLevel<GalaxySoulEnchantment>() > 0)
				{
					GalaxySoulEnchantment enchantment = (hoveredItem as MeleeWeapon).GetEnchantmentOfType<GalaxySoulEnchantment>();
					string forged_string = Game1.content.LoadString("Strings\\UI:Item_Tooltip_GalaxyForged");
					Utility.drawTextWithShadow(b, forged_string, font, new Vector2(x + 16, y4 + 16 + 4), Color.DarkRed, 1f, -1f, 2, 2);
					int level = enchantment.GetLevel();
					if (level < enchantment.GetMaximumLevel())
					{
						Utility.drawTextWithShadow(b, " (" + level + "/" + enchantment.GetMaximumLevel() + ")", font, new Vector2((float)(x + 16) + font.MeasureString(forged_string).X, y4 + 16 + 4), Color.DimGray, 1f, -1f, 2, 2);
					}
					y4 += (int)font.MeasureString("T").Y;
				}
			}
			else
			{
				y4 += ((boldTitleText != null) ? 16 : 0);
			}
			if (hoveredItem != null && craftingIngredients == null)
			{
				hoveredItem.drawTooltip(b, ref x, ref y4, font, alpha, text);
			}
			else if (text != null && text.Length != 0 && (text.Length != 1 || text[0] != ' '))
			{
				b.DrawString(font, text, new Vector2(x + 16, y4 + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor * alpha);
				b.DrawString(font, text, new Vector2(x + 16, y4 + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor * alpha);
				b.DrawString(font, text, new Vector2(x + 16, y4 + 16 + 4) + new Vector2(2f, 0f), Game1.textShadowColor * alpha);
				b.DrawString(font, text, new Vector2(x + 16, y4 + 16 + 4), Game1.textColor * 0.9f * alpha);
				y4 += (int)font.MeasureString(text).Y + 4;
			}
			if (craftingIngredients != null)
			{
				craftingIngredients.drawRecipeDescription(b, new Vector2(x + 16, y4 - 8), width, additional_craft_materials);
				y4 += craftingIngredients.getDescriptionHeight(width - 8);
			}
			if (healAmountToDisplay != -1)
			{
				int stamina_recovery = (hoveredItem as Object).staminaRecoveredOnConsumption();
				if (stamina_recovery >= 0)
				{
					int health_recovery = (hoveredItem as Object).healthRecoveredOnConsumption();
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, y4 + 16), new Rectangle((stamina_recovery < 0) ? 140 : 0, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", ((stamina_recovery > 0) ? "+" : "") + stamina_recovery), font, new Vector2(x + 16 + 34 + 4, y4 + 16), Game1.textColor);
					y4 += 34;
					if (health_recovery > 0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, y4 + 16), new Rectangle(0, 438, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Health", ((health_recovery > 0) ? "+" : "") + health_recovery), font, new Vector2(x + 16 + 34 + 4, y4 + 16), Game1.textColor);
						y4 += 34;
					}
				}
				else if (stamina_recovery != -300)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, y4 + 16), new Rectangle(140, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", string.Concat(stamina_recovery)), font, new Vector2(x + 16 + 34 + 4, y4 + 16), Game1.textColor);
					y4 += 34;
				}
			}
			if (buffIconsToDisplay != null)
			{
				for (int i = 0; i < buffIconsToDisplay.Length; i++)
				{
					if (!buffIconsToDisplay[i].Equals("0"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, y4 + 16), new Rectangle(10 + i * 10, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
						string buffName = ((Convert.ToInt32(buffIconsToDisplay[i]) > 0) ? "+" : "") + buffIconsToDisplay[i] + " ";
						if (i <= 11)
						{
							buffName = Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + i, buffName);
						}
						Utility.drawTextWithShadow(b, buffName, font, new Vector2(x + 16 + 34 + 4, y4 + 16), Game1.textColor);
						y4 += 34;
					}
				}
			}
			if (hoveredItem != null && hoveredItem.attachmentSlots() > 0)
			{
				hoveredItem.drawAttachments(b, x + 16, y4 + 16);
				if (moneyAmountToDisplayAtBottom > -1)
				{
					y4 += 68 * hoveredItem.attachmentSlots();
				}
			}
			if (moneyAmountToDisplayAtBottom > -1)
			{
				b.DrawString(font, string.Concat(moneyAmountToDisplayAtBottom), new Vector2(x + 16, y4 + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(font, string.Concat(moneyAmountToDisplayAtBottom), new Vector2(x + 16, y4 + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(font, string.Concat(moneyAmountToDisplayAtBottom), new Vector2(x + 16, y4 + 16 + 4) + new Vector2(2f, 0f), Game1.textShadowColor);
				b.DrawString(font, string.Concat(moneyAmountToDisplayAtBottom), new Vector2(x + 16, y4 + 16 + 4), Game1.textColor);
				switch (currencySymbol)
				{
				case 0:
					b.Draw(Game1.debrisSpriteSheet, new Vector2((float)(x + 16) + font.MeasureString(string.Concat(moneyAmountToDisplayAtBottom)).X + 20f, y4 + 16 + 16), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 0.95f);
					break;
				case 1:
					b.Draw(Game1.mouseCursors, new Vector2((float)(x + 8) + font.MeasureString(string.Concat(moneyAmountToDisplayAtBottom)).X + 20f, y4 + 16 - 5), new Rectangle(338, 400, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					break;
				case 2:
					b.Draw(Game1.mouseCursors, new Vector2((float)(x + 8) + font.MeasureString(string.Concat(moneyAmountToDisplayAtBottom)).X + 20f, y4 + 16 - 7), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					break;
				case 4:
					b.Draw(Game1.objectSpriteSheet, new Vector2((float)(x + 8) + font.MeasureString(string.Concat(moneyAmountToDisplayAtBottom)).X + 20f, y4 + 16 - 7), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					break;
				}
				y4 += 48;
			}
			if (extraItemToShowIndex != -1)
			{
				if (moneyAmountToDisplayAtBottom == -1)
				{
					y4 += 8;
				}
				string displayName = Game1.objectInformation[extraItemToShowIndex].Split('/')[4];
				string requirement = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", extraItemToShowAmount, displayName);
				float minimum_box_height = Math.Max(font.MeasureString(requirement).Y + 21f, 96f);
				drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y4 + 4, width + ((craftingIngredients != null) ? 21 : 0), (int)minimum_box_height, Color.White);
				y4 += 20;
				b.DrawString(font, requirement, new Vector2(x + 16, y4 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(font, requirement, new Vector2(x + 16, y4 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(font, requirement, new Vector2(x + 16, y4 + 4) + new Vector2(2f, 0f), Game1.textShadowColor);
				b.DrawString(Game1.smallFont, requirement, new Vector2(x + 16, y4 + 4), Game1.textColor);
				b.Draw(Game1.objectSpriteSheet, new Vector2(x + 16 + (int)font.MeasureString(requirement).X + 21, y4), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (craftingIngredients != null && Game1.options.showAdvancedCraftingInformation)
			{
				Utility.drawTextWithShadow(b, craftingIngredients.getCraftCountText(), font, new Vector2(x + 16, y4 + 16 + 4), Game1.textColor, 1f, -1f, 2, 2);
				y4 += (int)font.MeasureString("T").Y + 4;
			}
		}
	}
}
