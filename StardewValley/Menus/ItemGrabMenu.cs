using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class ItemGrabMenu : MenuWithInventory
	{
		public delegate void behaviorOnItemSelect(Item item, Farmer who);

		public class TransferredItemSprite
		{
			public Item item;

			public Vector2 position;

			public float age;

			public float alpha = 1f;

			public TransferredItemSprite(Item transferred_item, int start_x, int start_y)
			{
				item = transferred_item;
				position.X = start_x;
				position.Y = start_y;
			}

			public bool Update(GameTime time)
			{
				float life_time = 0.15f;
				position.Y -= (float)time.ElapsedGameTime.TotalSeconds * 128f;
				age += (float)time.ElapsedGameTime.TotalSeconds;
				alpha = 1f - age / life_time;
				if (age >= life_time)
				{
					return true;
				}
				return false;
			}

			public void Draw(SpriteBatch b)
			{
				item.drawInMenu(b, position, 1f, alpha, 0.9f, StackDrawType.Hide, Color.White, drawShadow: false);
			}
		}

		public const int region_organizationButtons = 15923;

		public const int region_itemsToGrabMenuModifier = 53910;

		public const int region_fillStacksButton = 12952;

		public const int region_organizeButton = 106;

		public const int region_colorPickToggle = 27346;

		public const int region_specialButton = 12485;

		public const int region_lastShippedHolder = 12598;

		public const int source_none = 0;

		public const int source_chest = 1;

		public const int source_gift = 2;

		public const int source_fishingChest = 3;

		public const int specialButton_junimotoggle = 1;

		public InventoryMenu ItemsToGrabMenu;

		private TemporaryAnimatedSprite poof;

		public bool reverseGrab;

		public bool showReceivingMenu = true;

		public bool drawBG = true;

		public bool destroyItemOnClick;

		public bool canExitOnKey;

		public bool playRightClickSound;

		public bool allowRightClick;

		public bool shippingBin;

		private string message;

		private behaviorOnItemSelect behaviorFunction;

		public behaviorOnItemSelect behaviorOnItemGrab;

		private Item hoverItem;

		private Item sourceItem;

		public ClickableTextureComponent fillStacksButton;

		public ClickableTextureComponent organizeButton;

		public ClickableTextureComponent colorPickerToggleButton;

		public ClickableTextureComponent specialButton;

		public ClickableTextureComponent lastShippedHolder;

		public List<ClickableComponent> discreteColorPickerCC;

		public int source;

		public int whichSpecialButton;

		public object context;

		private bool snappedtoBottom;

		public DiscreteColorPicker chestColorPicker;

		private bool essential;

		protected List<TransferredItemSprite> _transferredItemSprites = new List<TransferredItemSprite>();

		protected bool _sourceItemInCurrentLocation;

		public ClickableTextureComponent junimoNoteIcon;

		private int junimoNotePulser;

		public ItemGrabMenu(IList<Item> inventory, object context = null)
			: base(null, okButton: true, trashCan: true)
		{
			this.context = context;
			ItemsToGrabMenu = new InventoryMenu(xPositionOnScreen + 32, yPositionOnScreen, playerInventory: false, inventory);
			trashCan.myID = 106;
			ItemsToGrabMenu.populateClickableComponentList();
			for (int k = 0; k < ItemsToGrabMenu.inventory.Count; k++)
			{
				if (ItemsToGrabMenu.inventory[k] != null)
				{
					ItemsToGrabMenu.inventory[k].myID += 53910;
					ItemsToGrabMenu.inventory[k].upNeighborID += 53910;
					ItemsToGrabMenu.inventory[k].rightNeighborID += 53910;
					ItemsToGrabMenu.inventory[k].downNeighborID = -7777;
					ItemsToGrabMenu.inventory[k].leftNeighborID += 53910;
					ItemsToGrabMenu.inventory[k].fullyImmutable = true;
					if (k % (ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows) == 0)
					{
						ItemsToGrabMenu.inventory[k].leftNeighborID = dropItemInvisibleButton.myID;
					}
					if (k % (ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows) == ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows - 1)
					{
						ItemsToGrabMenu.inventory[k].rightNeighborID = trashCan.myID;
					}
				}
			}
			for (int j = 0; j < GetColumnCount(); j++)
			{
				if (base.inventory != null && base.inventory.inventory != null && base.inventory.inventory.Count >= GetColumnCount())
				{
					base.inventory.inventory[j].upNeighborID = (shippingBin ? 12598 : (-7777));
				}
			}
			if (!shippingBin)
			{
				for (int i = 0; i < GetColumnCount() * 3; i++)
				{
					if (base.inventory != null && base.inventory.inventory != null && base.inventory.inventory.Count > i)
					{
						base.inventory.inventory[i].upNeighborID = -7777;
						base.inventory.inventory[i].upNeighborImmutable = true;
					}
				}
			}
			if (trashCan != null)
			{
				trashCan.leftNeighborID = 11;
			}
			if (okButton != null)
			{
				okButton.leftNeighborID = 11;
			}
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
			base.inventory.showGrayedOutSlots = true;
			SetupBorderNeighbors();
		}

		public virtual void DropRemainingItems()
		{
			if (ItemsToGrabMenu != null && ItemsToGrabMenu.actualInventory != null)
			{
				foreach (Item item in ItemsToGrabMenu.actualInventory)
				{
					if (item != null)
					{
						Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
					}
				}
				ItemsToGrabMenu.actualInventory.Clear();
			}
		}

		public override bool readyToClose()
		{
			return base.readyToClose();
		}

		public ItemGrabMenu(IList<Item> inventory, bool reverseGrab, bool showReceivingMenu, InventoryMenu.highlightThisItem highlightFunction, behaviorOnItemSelect behaviorOnItemSelectFunction, string message, behaviorOnItemSelect behaviorOnItemGrab = null, bool snapToBottom = false, bool canBeExitedWithKey = false, bool playRightClickSound = true, bool allowRightClick = true, bool showOrganizeButton = false, int source = 0, Item sourceItem = null, int whichSpecialButton = -1, object context = null)
			: base(highlightFunction, okButton: true, trashCan: true, 0, 0, 64)
		{
			this.source = source;
			this.message = message;
			this.reverseGrab = reverseGrab;
			this.showReceivingMenu = showReceivingMenu;
			this.playRightClickSound = playRightClickSound;
			this.allowRightClick = allowRightClick;
			base.inventory.showGrayedOutSlots = true;
			this.sourceItem = sourceItem;
			if (sourceItem != null && Game1.currentLocation.objects.Values.Contains(sourceItem))
			{
				_sourceItemInCurrentLocation = true;
			}
			else
			{
				_sourceItemInCurrentLocation = false;
			}
			if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).SpecialChestType == Chest.SpecialChestTypes.None)
			{
				chestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, 0, new Chest(playerChest: true, sourceItem.ParentSheetIndex));
				chestColorPicker.colorSelection = chestColorPicker.getSelectionFromColor((sourceItem as Chest).playerChoiceColor);
				(chestColorPicker.itemToDrawColored as Chest).playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
				colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f)
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"),
					myID = 27346,
					downNeighborID = -99998,
					leftNeighborID = 53921,
					region = 15923
				};
				if (InventoryPage.ShouldShowJunimoNoteIcon())
				{
					junimoNoteIcon = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 + -216, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f)
					{
						myID = 898,
						leftNeighborID = 11,
						downNeighborID = 106
					};
				}
			}
			this.whichSpecialButton = whichSpecialButton;
			this.context = context;
			if (whichSpecialButton == 1)
			{
				specialButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(108, 491, 16, 16), 4f)
				{
					myID = 12485,
					downNeighborID = (showOrganizeButton ? 12952 : 5948),
					region = 15923,
					leftNeighborID = 53921
				};
				if (context != null && context is JunimoHut)
				{
					specialButton.sourceRect.X = ((context as JunimoHut).noHarvest ? 124 : 108);
				}
			}
			if (snapToBottom)
			{
				movePosition(0, Game1.uiViewport.Height - (yPositionOnScreen + height - IClickableMenu.spaceToClearTopBorder));
				snappedtoBottom = true;
			}
			if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).GetActualCapacity() != 36)
			{
				int capacity = (sourceItem as Chest).GetActualCapacity();
				int rows = 3;
				if (capacity < 9)
				{
					rows = 1;
				}
				int containerWidth = 64 * (capacity / rows);
				ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, yPositionOnScreen + 64, playerInventory: false, inventory, highlightFunction, capacity, rows);
				if ((sourceItem as Chest).SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
				{
					base.inventory.moveItemSound = "Ship";
				}
			}
			else
			{
				ItemsToGrabMenu = new InventoryMenu(xPositionOnScreen + 32, yPositionOnScreen, playerInventory: false, inventory, highlightFunction);
			}
			ItemsToGrabMenu.populateClickableComponentList();
			for (int j = 0; j < ItemsToGrabMenu.inventory.Count; j++)
			{
				if (ItemsToGrabMenu.inventory[j] != null)
				{
					ItemsToGrabMenu.inventory[j].myID += 53910;
					ItemsToGrabMenu.inventory[j].upNeighborID += 53910;
					ItemsToGrabMenu.inventory[j].rightNeighborID += 53910;
					ItemsToGrabMenu.inventory[j].downNeighborID = -7777;
					ItemsToGrabMenu.inventory[j].leftNeighborID += 53910;
					ItemsToGrabMenu.inventory[j].fullyImmutable = true;
				}
			}
			behaviorFunction = behaviorOnItemSelectFunction;
			this.behaviorOnItemGrab = behaviorOnItemGrab;
			canExitOnKey = canBeExitedWithKey;
			if (showOrganizeButton)
			{
				fillStacksButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 - 64 - 16, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"), Game1.mouseCursors, new Rectangle(103, 469, 16, 16), 4f)
				{
					myID = 12952,
					upNeighborID = ((colorPickerToggleButton != null) ? 27346 : ((specialButton != null) ? 12485 : (-500))),
					downNeighborID = 106,
					leftNeighborID = 53921,
					region = 15923
				};
				organizeButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f)
				{
					myID = 106,
					upNeighborID = 12952,
					downNeighborID = 5948,
					leftNeighborID = 53921,
					region = 15923
				};
			}
			RepositionSideButtons();
			if (chestColorPicker != null)
			{
				discreteColorPickerCC = new List<ClickableComponent>();
				for (int i = 0; i < chestColorPicker.totalColors; i++)
				{
					discreteColorPickerCC.Add(new ClickableComponent(new Rectangle(chestColorPicker.xPositionOnScreen + IClickableMenu.borderWidth / 2 + i * 9 * 4, chestColorPicker.yPositionOnScreen + IClickableMenu.borderWidth / 2, 36, 28), "")
					{
						myID = i + 4343,
						rightNeighborID = ((i < chestColorPicker.totalColors - 1) ? (i + 4343 + 1) : (-1)),
						leftNeighborID = ((i > 0) ? (i + 4343 - 1) : (-1)),
						downNeighborID = ((ItemsToGrabMenu != null && ItemsToGrabMenu.inventory.Count > 0) ? 53910 : 0)
					});
				}
			}
			if (organizeButton != null)
			{
				foreach (ClickableComponent item in ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right))
				{
					item.rightNeighborID = organizeButton.myID;
				}
			}
			if (trashCan != null && base.inventory.inventory.Count >= 12 && base.inventory.inventory[11] != null)
			{
				base.inventory.inventory[11].rightNeighborID = 5948;
			}
			if (trashCan != null)
			{
				trashCan.leftNeighborID = 11;
			}
			if (okButton != null)
			{
				okButton.leftNeighborID = 11;
			}
			ClickableComponent top_right = ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right).FirstOrDefault();
			if (top_right != null)
			{
				if (organizeButton != null)
				{
					organizeButton.leftNeighborID = top_right.myID;
				}
				if (specialButton != null)
				{
					specialButton.leftNeighborID = top_right.myID;
				}
				if (fillStacksButton != null)
				{
					fillStacksButton.leftNeighborID = top_right.myID;
				}
				if (junimoNoteIcon != null)
				{
					junimoNoteIcon.leftNeighborID = top_right.myID;
				}
			}
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
			SetupBorderNeighbors();
		}

		public virtual void RepositionSideButtons()
		{
			List<ClickableComponent> side_buttons = new List<ClickableComponent>();
			if (organizeButton != null)
			{
				side_buttons.Add(organizeButton);
			}
			if (fillStacksButton != null)
			{
				side_buttons.Add(fillStacksButton);
			}
			if (colorPickerToggleButton != null)
			{
				side_buttons.Add(colorPickerToggleButton);
			}
			if (specialButton != null)
			{
				side_buttons.Add(specialButton);
			}
			if (junimoNoteIcon != null)
			{
				side_buttons.Add(junimoNoteIcon);
			}
			int step_size = 80;
			if (side_buttons.Count >= 4)
			{
				step_size = 72;
			}
			for (int i = 0; i < side_buttons.Count; i++)
			{
				ClickableComponent button = side_buttons[i];
				if (i > 0 && side_buttons.Count > 1)
				{
					button.downNeighborID = side_buttons[i - 1].myID;
				}
				if (i < side_buttons.Count - 1 && side_buttons.Count > 1)
				{
					button.upNeighborID = side_buttons[i + 1].myID;
				}
				button.bounds.X = xPositionOnScreen + width;
				button.bounds.Y = yPositionOnScreen + height / 3 - 64 - step_size * i;
			}
		}

		public void SetupBorderNeighbors()
		{
			List<ClickableComponent> border2 = inventory.GetBorder(InventoryMenu.BorderSide.Right);
			foreach (ClickableComponent item in border2)
			{
				item.rightNeighborID = -99998;
				item.rightNeighborImmutable = true;
			}
			border2 = ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right);
			bool has_organizational_buttons = false;
			foreach (ClickableComponent allClickableComponent in allClickableComponents)
			{
				if (allClickableComponent.region == 15923)
				{
					has_organizational_buttons = true;
					break;
				}
			}
			foreach (ClickableComponent slot in border2)
			{
				if (has_organizational_buttons)
				{
					slot.rightNeighborID = -99998;
					slot.rightNeighborImmutable = true;
				}
				else
				{
					slot.rightNeighborID = -1;
				}
			}
			for (int j = 0; j < GetColumnCount(); j++)
			{
				if (inventory != null && inventory.inventory != null && inventory.inventory.Count >= 12)
				{
					inventory.inventory[j].upNeighborID = (shippingBin ? 12598 : ((discreteColorPickerCC != null && ItemsToGrabMenu != null && ItemsToGrabMenu.inventory.Count <= j && Game1.player.showChestColorPicker) ? 4343 : ((ItemsToGrabMenu.inventory.Count > j) ? (53910 + j) : 53910)));
				}
				if (discreteColorPickerCC != null && ItemsToGrabMenu != null && ItemsToGrabMenu.inventory.Count > j && Game1.player.showChestColorPicker)
				{
					ItemsToGrabMenu.inventory[j].upNeighborID = 4343;
				}
				else
				{
					ItemsToGrabMenu.inventory[j].upNeighborID = -1;
				}
			}
			if (shippingBin)
			{
				return;
			}
			for (int i = 0; i < 36; i++)
			{
				if (inventory != null && inventory.inventory != null && inventory.inventory.Count > i)
				{
					inventory.inventory[i].upNeighborID = -7777;
					inventory.inventory[i].upNeighborImmutable = true;
				}
			}
		}

		public virtual int GetColumnCount()
		{
			return ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows;
		}

		public ItemGrabMenu setEssential(bool essential)
		{
			this.essential = essential;
			return this;
		}

		public void initializeShippingBin()
		{
			shippingBin = true;
			lastShippedHolder = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height / 2 - 80 - 64, 96, 96), "", Game1.content.LoadString("Strings\\UI:ShippingBin_LastItem"), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), 4f)
			{
				myID = 12598,
				region = 12598
			};
			for (int i = 0; i < GetColumnCount(); i++)
			{
				if (inventory != null && inventory.inventory != null && inventory.inventory.Count >= GetColumnCount())
				{
					inventory.inventory[i].upNeighborID = -7777;
					if (i == 11)
					{
						inventory.inventory[i].rightNeighborID = 5948;
					}
				}
			}
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			switch (direction)
			{
			case 2:
			{
				for (int j = 0; j < 12; j++)
				{
					if (inventory != null && inventory.inventory != null && inventory.inventory.Count >= GetColumnCount() && shippingBin)
					{
						inventory.inventory[j].upNeighborID = (shippingBin ? 12598 : (Math.Min(j, ItemsToGrabMenu.inventory.Count - 1) + 53910));
					}
				}
				if (!shippingBin && oldID >= 53910)
				{
					int index = oldID - 53910;
					if (index + GetColumnCount() <= ItemsToGrabMenu.inventory.Count - 1)
					{
						currentlySnappedComponent = getComponentWithID(index + GetColumnCount() + 53910);
						snapCursorToCurrentSnappedComponent();
						break;
					}
				}
				currentlySnappedComponent = getComponentWithID((oldRegion != 12598) ? ((oldID - 53910) % GetColumnCount()) : 0);
				snapCursorToCurrentSnappedComponent();
				break;
			}
			case 0:
			{
				if (shippingBin && Game1.getFarm().lastItemShipped != null && oldID < 12)
				{
					currentlySnappedComponent = getComponentWithID(12598);
					currentlySnappedComponent.downNeighborID = oldID;
					snapCursorToCurrentSnappedComponent();
					break;
				}
				if (oldID < 53910 && oldID >= 12)
				{
					currentlySnappedComponent = getComponentWithID(oldID - 12);
					break;
				}
				int id = oldID + GetColumnCount() * 2;
				for (int i = 0; i < 3; i++)
				{
					if (ItemsToGrabMenu.inventory.Count > id)
					{
						break;
					}
					id -= GetColumnCount();
				}
				if (showReceivingMenu)
				{
					if (id < 0)
					{
						if (ItemsToGrabMenu.inventory.Count > 0)
						{
							currentlySnappedComponent = getComponentWithID(53910 + ItemsToGrabMenu.inventory.Count - 1);
						}
						else if (discreteColorPickerCC != null)
						{
							currentlySnappedComponent = getComponentWithID(4343);
						}
					}
					else
					{
						currentlySnappedComponent = getComponentWithID(id + 53910);
						if (currentlySnappedComponent == null)
						{
							currentlySnappedComponent = getComponentWithID(53910);
						}
					}
				}
				snapCursorToCurrentSnappedComponent();
				break;
			}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (shippingBin)
			{
				currentlySnappedComponent = getComponentWithID(0);
			}
			else if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
			{
				currentlySnappedComponent = getComponentWithID(0);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID((ItemsToGrabMenu.inventory.Count > 0 && showReceivingMenu) ? 53910 : 0);
			}
			snapCursorToCurrentSnappedComponent();
		}

		public void setSourceItem(Item item)
		{
			sourceItem = item;
			chestColorPicker = null;
			colorPickerToggleButton = null;
			if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).SpecialChestType == Chest.SpecialChestTypes.None)
			{
				chestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, 0, new Chest(playerChest: true, sourceItem.ParentSheetIndex));
				chestColorPicker.colorSelection = chestColorPicker.getSelectionFromColor((sourceItem as Chest).playerChoiceColor);
				(chestColorPicker.itemToDrawColored as Chest).playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
				colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f)
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker")
				};
			}
			RepositionSideButtons();
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (direction == 1 && ItemsToGrabMenu.inventory.Contains(a) && inventory.inventory.Contains(b))
			{
				return false;
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		public void setBackgroundTransparency(bool b)
		{
			drawBG = b;
		}

		public void setDestroyItemOnClick(bool b)
		{
			destroyItemOnClick = b;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!allowRightClick)
			{
				receiveRightClickOnlyToolAttachments(x, y);
				return;
			}
			base.receiveRightClick(x, y, playSound && playRightClickSound);
			if (heldItem == null && showReceivingMenu)
			{
				heldItem = ItemsToGrabMenu.rightClick(x, y, heldItem, playSound: false);
				if (heldItem != null && behaviorOnItemGrab != null)
				{
					behaviorOnItemGrab(heldItem, Game1.player);
					if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ItemGrabMenu)
					{
						(Game1.activeClickableMenu as ItemGrabMenu).setSourceItem(sourceItem);
					}
					if (Game1.options.SnappyMenus)
					{
						(Game1.activeClickableMenu as ItemGrabMenu).currentlySnappedComponent = currentlySnappedComponent;
						(Game1.activeClickableMenu as ItemGrabMenu).snapCursorToCurrentSnappedComponent();
					}
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(heldItem, 326))
				{
					heldItem = null;
					Game1.player.canUnderstandDwarves = true;
					poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
					Game1.playSound("fireball");
				}
				else if (heldItem is Object && Utility.IsNormalObjectAtParentSheetIndex(heldItem, 434))
				{
					Object held_item = heldItem as Object;
					heldItem = null;
					exitThisMenu(playSound: false);
					Game1.player.eatObject(held_item, overrideFullness: true);
				}
				else if (heldItem is Object && (bool)(heldItem as Object).isRecipe)
				{
					string recipeName = heldItem.Name.Substring(0, heldItem.Name.IndexOf("Recipe") - 1);
					try
					{
						if ((heldItem as Object).Category == -7)
						{
							Game1.player.cookingRecipes.Add(recipeName, 0);
						}
						else
						{
							Game1.player.craftingRecipes.Add(recipeName, 0);
						}
						poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
						Game1.playSound("newRecipe");
					}
					catch (Exception)
					{
					}
					heldItem = null;
				}
				else if (Game1.player.addItemToInventoryBool(heldItem))
				{
					heldItem = null;
					Game1.playSound("coin");
				}
			}
			else if (reverseGrab || behaviorFunction != null)
			{
				behaviorFunction(heldItem, Game1.player);
				if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ItemGrabMenu)
				{
					(Game1.activeClickableMenu as ItemGrabMenu).setSourceItem(sourceItem);
				}
				if (destroyItemOnClick)
				{
					heldItem = null;
				}
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			if (snappedtoBottom)
			{
				movePosition((newBounds.Width - oldBounds.Width) / 2, Game1.uiViewport.Height - (yPositionOnScreen + height - IClickableMenu.spaceToClearTopBorder));
			}
			else
			{
				movePosition((newBounds.Width - oldBounds.Width) / 2, (newBounds.Height - oldBounds.Height) / 2);
			}
			if (ItemsToGrabMenu != null)
			{
				ItemsToGrabMenu.gameWindowSizeChanged(oldBounds, newBounds);
			}
			RepositionSideButtons();
			if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).SpecialChestType == Chest.SpecialChestTypes.None)
			{
				chestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, 0, new Chest(playerChest: true, sourceItem.ParentSheetIndex));
				chestColorPicker.colorSelection = chestColorPicker.getSelectionFromColor((sourceItem as Chest).playerChoiceColor);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, (!destroyItemOnClick) ? true : false);
			if (shippingBin && lastShippedHolder.containsPoint(x, y))
			{
				if (Game1.getFarm().lastItemShipped == null)
				{
					return;
				}
				Game1.getFarm().getShippingBin(Game1.player).Remove(Game1.getFarm().lastItemShipped);
				if (Game1.player.addItemToInventoryBool(Game1.getFarm().lastItemShipped))
				{
					Game1.playSound("coin");
					Game1.getFarm().lastItemShipped = null;
					if (Game1.player.ActiveObject != null)
					{
						Game1.player.showCarrying();
						Game1.player.Halt();
					}
				}
				else
				{
					Game1.getFarm().getShippingBin(Game1.player).Add(Game1.getFarm().lastItemShipped);
				}
				return;
			}
			if (chestColorPicker != null)
			{
				chestColorPicker.receiveLeftClick(x, y);
				if (sourceItem != null && sourceItem is Chest)
				{
					(sourceItem as Chest).playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
				}
			}
			if (colorPickerToggleButton != null && colorPickerToggleButton.containsPoint(x, y))
			{
				Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
				chestColorPicker.visible = Game1.player.showChestColorPicker;
				try
				{
					Game1.playSound("drumkit6");
				}
				catch (Exception)
				{
				}
				SetupBorderNeighbors();
				return;
			}
			if (whichSpecialButton != -1 && specialButton != null && specialButton.containsPoint(x, y))
			{
				Game1.playSound("drumkit6");
				int num = whichSpecialButton;
				if (num == 1 && context != null && context is JunimoHut)
				{
					(context as JunimoHut).noHarvest.Value = !(context as JunimoHut).noHarvest;
					specialButton.sourceRect.X = ((context as JunimoHut).noHarvest ? 124 : 108);
				}
				return;
			}
			if (heldItem == null && showReceivingMenu)
			{
				heldItem = ItemsToGrabMenu.leftClick(x, y, heldItem, playSound: false);
				if (heldItem != null && behaviorOnItemGrab != null)
				{
					behaviorOnItemGrab(heldItem, Game1.player);
					if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ItemGrabMenu)
					{
						(Game1.activeClickableMenu as ItemGrabMenu).setSourceItem(sourceItem);
						if (Game1.options.SnappyMenus)
						{
							(Game1.activeClickableMenu as ItemGrabMenu).currentlySnappedComponent = currentlySnappedComponent;
							(Game1.activeClickableMenu as ItemGrabMenu).snapCursorToCurrentSnappedComponent();
						}
					}
				}
				if (heldItem is Object && (int)(heldItem as Object).parentSheetIndex == 326)
				{
					heldItem = null;
					Game1.player.canUnderstandDwarves = true;
					poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
					Game1.playSound("fireball");
				}
				else if (heldItem is Object && (int)(heldItem as Object).parentSheetIndex == 102)
				{
					heldItem = null;
					Game1.player.foundArtifact(102, 1);
					poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
					Game1.playSound("fireball");
				}
				if (heldItem is Object && Utility.IsNormalObjectAtParentSheetIndex(heldItem, 434))
				{
					Object held_item2 = heldItem as Object;
					heldItem = null;
					exitThisMenu(playSound: false);
					Game1.player.eatObject(held_item2, overrideFullness: true);
				}
				else if (heldItem is Object && (bool)(heldItem as Object).isRecipe)
				{
					string recipeName = heldItem.Name.Substring(0, heldItem.Name.IndexOf("Recipe") - 1);
					try
					{
						if ((heldItem as Object).Category == -7)
						{
							Game1.player.cookingRecipes.Add(recipeName, 0);
						}
						else
						{
							Game1.player.craftingRecipes.Add(recipeName, 0);
						}
						poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
						Game1.playSound("newRecipe");
					}
					catch (Exception)
					{
					}
					heldItem = null;
				}
				else if (Game1.player.addItemToInventoryBool(heldItem))
				{
					heldItem = null;
					Game1.playSound("coin");
				}
			}
			else if ((reverseGrab || behaviorFunction != null) && isWithinBounds(x, y))
			{
				behaviorFunction(heldItem, Game1.player);
				if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ItemGrabMenu)
				{
					(Game1.activeClickableMenu as ItemGrabMenu).setSourceItem(sourceItem);
					if (Game1.options.SnappyMenus)
					{
						(Game1.activeClickableMenu as ItemGrabMenu).currentlySnappedComponent = currentlySnappedComponent;
						(Game1.activeClickableMenu as ItemGrabMenu).snapCursorToCurrentSnappedComponent();
					}
				}
				if (destroyItemOnClick)
				{
					heldItem = null;
					return;
				}
			}
			if (organizeButton != null && organizeButton.containsPoint(x, y))
			{
				ClickableComponent last_snapped_component = currentlySnappedComponent;
				organizeItemsInList(ItemsToGrabMenu.actualInventory);
				Item held_item = heldItem;
				heldItem = null;
				Game1.activeClickableMenu = new ItemGrabMenu(ItemsToGrabMenu.actualInventory, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, behaviorFunction, null, behaviorOnItemGrab, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, source, sourceItem, whichSpecialButton, context).setEssential(essential);
				if (last_snapped_component != null)
				{
					Game1.activeClickableMenu.setCurrentlySnappedComponentTo(last_snapped_component.myID);
					if (Game1.options.SnappyMenus)
					{
						snapCursorToCurrentSnappedComponent();
					}
				}
				(Game1.activeClickableMenu as ItemGrabMenu).heldItem = held_item;
				Game1.playSound("Ship");
			}
			else if (fillStacksButton != null && fillStacksButton.containsPoint(x, y))
			{
				FillOutStacks();
				Game1.playSound("Ship");
			}
			else if (junimoNoteIcon != null && junimoNoteIcon.containsPoint(x, y))
			{
				if (readyToClose())
				{
					Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true);
				}
			}
			else if (heldItem != null && !isWithinBounds(x, y) && heldItem.canBeTrashed())
			{
				DropHeldItem();
			}
		}

		public virtual void DropHeldItem()
		{
			if (heldItem != null)
			{
				Game1.playSound("throwDownITem");
				Console.WriteLine("Dropping " + heldItem.Name);
				int drop_direction = Game1.player.facingDirection;
				if (context is LibraryMuseum)
				{
					drop_direction = 2;
				}
				Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), drop_direction);
				if (inventory.onAddItem != null)
				{
					inventory.onAddItem(heldItem, Game1.player);
				}
				heldItem = null;
			}
		}

		public void FillOutStacks()
		{
			for (int l = 0; l < ItemsToGrabMenu.actualInventory.Count; l++)
			{
				Item chest_item = ItemsToGrabMenu.actualInventory[l];
				if (chest_item == null || chest_item.maximumStackSize() <= 1)
				{
					continue;
				}
				for (int k = 0; k < inventory.actualInventory.Count; k++)
				{
					Item inventory_item = inventory.actualInventory[k];
					if (inventory_item == null || !chest_item.canStackWith(inventory_item))
					{
						continue;
					}
					TransferredItemSprite item_sprite = new TransferredItemSprite(inventory_item.getOne(), inventory.inventory[k].bounds.X, inventory.inventory[k].bounds.Y);
					_transferredItemSprites.Add(item_sprite);
					int stack_count2 = inventory_item.Stack;
					if (chest_item.getRemainingStackSpace() > 0)
					{
						stack_count2 = chest_item.addToStack(inventory_item);
						ItemsToGrabMenu.ShakeItem(chest_item);
					}
					inventory_item.Stack = stack_count2;
					while (inventory_item.Stack > 0)
					{
						Item overflow_stack = null;
						if (!Utility.canItemBeAddedToThisInventoryList(chest_item.getOne(), ItemsToGrabMenu.actualInventory, ItemsToGrabMenu.capacity))
						{
							break;
						}
						if (overflow_stack == null)
						{
							for (int j = 0; j < ItemsToGrabMenu.actualInventory.Count; j++)
							{
								if (ItemsToGrabMenu.actualInventory[j] != null && ItemsToGrabMenu.actualInventory[j].canStackWith(chest_item) && ItemsToGrabMenu.actualInventory[j].getRemainingStackSpace() > 0)
								{
									overflow_stack = ItemsToGrabMenu.actualInventory[j];
									break;
								}
							}
						}
						if (overflow_stack == null)
						{
							for (int i = 0; i < ItemsToGrabMenu.actualInventory.Count; i++)
							{
								if (ItemsToGrabMenu.actualInventory[i] == null)
								{
									Item item = ItemsToGrabMenu.actualInventory[i] = chest_item.getOne();
									overflow_stack = item;
									overflow_stack.Stack = 0;
									break;
								}
							}
						}
						if (overflow_stack == null && ItemsToGrabMenu.actualInventory.Count < ItemsToGrabMenu.capacity)
						{
							overflow_stack = chest_item.getOne();
							overflow_stack.Stack = 0;
							ItemsToGrabMenu.actualInventory.Add(overflow_stack);
						}
						if (overflow_stack == null)
						{
							break;
						}
						stack_count2 = overflow_stack.addToStack(inventory_item);
						ItemsToGrabMenu.ShakeItem(overflow_stack);
						inventory_item.Stack = stack_count2;
					}
					if (inventory_item.Stack == 0)
					{
						inventory.actualInventory[k] = null;
					}
				}
			}
		}

		public static void organizeItemsInList(IList<Item> items)
		{
			List<Item> copy = new List<Item>(items);
			List<Item> tools = new List<Item>();
			for (int m = 0; m < copy.Count; m++)
			{
				if (copy[m] == null)
				{
					copy.RemoveAt(m);
					m--;
				}
				else if (copy[m] is Tool)
				{
					tools.Add(copy[m]);
					copy.RemoveAt(m);
					m--;
				}
			}
			for (int l = 0; l < copy.Count; l++)
			{
				Item current_item = copy[l];
				if (current_item.getRemainingStackSpace() <= 0)
				{
					continue;
				}
				for (int i = l + 1; i < copy.Count; i++)
				{
					Item other_item = copy[i];
					if (current_item.canStackWith(other_item))
					{
						other_item.Stack = current_item.addToStack(other_item);
						if (other_item.Stack == 0)
						{
							copy.RemoveAt(i);
							i--;
						}
					}
				}
			}
			copy.Sort();
			copy.InsertRange(0, tools);
			for (int k = 0; k < items.Count; k++)
			{
				items[k] = null;
			}
			for (int j = 0; j < copy.Count; j++)
			{
				items[j] = copy[j];
			}
		}

		public bool areAllItemsTaken()
		{
			for (int i = 0; i < ItemsToGrabMenu.actualInventory.Count; i++)
			{
				if (ItemsToGrabMenu.actualInventory[i] != null)
				{
					return false;
				}
			}
			return true;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (b == Buttons.Back && organizeButton != null)
			{
				organizeItemsInList(Game1.player.items);
				Game1.playSound("Ship");
			}
			if (b == Buttons.RightShoulder)
			{
				ClickableComponent fill_stacks_component = getComponentWithID(12952);
				if (fill_stacks_component != null)
				{
					setCurrentlySnappedComponentTo(fill_stacks_component.myID);
					snapCursorToCurrentSnappedComponent();
				}
				else
				{
					int highest_y = -1;
					ClickableComponent highest_component = null;
					foreach (ClickableComponent component3 in allClickableComponents)
					{
						if (component3.region == 15923 && (highest_y == -1 || component3.bounds.Y < highest_y))
						{
							highest_y = component3.bounds.Y;
							highest_component = component3;
						}
					}
					if (highest_component != null)
					{
						setCurrentlySnappedComponentTo(highest_component.myID);
						snapCursorToCurrentSnappedComponent();
					}
				}
			}
			if (shippingBin || b != Buttons.LeftShoulder)
			{
				return;
			}
			ClickableComponent component2 = getComponentWithID(53910);
			if (component2 != null)
			{
				setCurrentlySnappedComponentTo(component2.myID);
				snapCursorToCurrentSnappedComponent();
				return;
			}
			component2 = getComponentWithID(0);
			if (component2 != null)
			{
				setCurrentlySnappedComponentTo(0);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				applyMovementKey(key);
			}
			if ((canExitOnKey || areAllItemsTaken()) && Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
			{
				exitThisMenu();
				if (Game1.currentLocation.currentEvent != null && Game1.currentLocation.currentEvent.CurrentCommand > 0)
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && heldItem != null)
			{
				Game1.setMousePosition(trashCan.bounds.Center);
			}
			if (key == Keys.Delete && heldItem != null && heldItem.canBeTrashed())
			{
				Utility.trashItem(heldItem);
				heldItem = null;
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (poof != null && poof.update(time))
			{
				poof = null;
			}
			if (chestColorPicker != null)
			{
				chestColorPicker.update(time);
			}
			if (sourceItem != null && sourceItem is Chest && _sourceItemInCurrentLocation)
			{
				Vector2 tileLocation = (sourceItem as Object).tileLocation;
				if (tileLocation != Vector2.Zero && !Game1.currentLocation.objects.ContainsKey(tileLocation))
				{
					if (Game1.activeClickableMenu != null)
					{
						Game1.activeClickableMenu.emergencyShutDown();
					}
					Game1.exitActiveMenu();
				}
			}
			for (int i = 0; i < _transferredItemSprites.Count; i++)
			{
				if (_transferredItemSprites[i].Update(time))
				{
					_transferredItemSprites.RemoveAt(i);
					i--;
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			hoveredItem = null;
			hoverText = "";
			base.performHoverAction(x, y);
			if (colorPickerToggleButton != null)
			{
				colorPickerToggleButton.tryHover(x, y, 0.25f);
				if (colorPickerToggleButton.containsPoint(x, y))
				{
					hoverText = colorPickerToggleButton.hoverText;
				}
			}
			if (organizeButton != null)
			{
				organizeButton.tryHover(x, y, 0.25f);
				if (organizeButton.containsPoint(x, y))
				{
					hoverText = organizeButton.hoverText;
				}
			}
			if (fillStacksButton != null)
			{
				fillStacksButton.tryHover(x, y, 0.25f);
				if (fillStacksButton.containsPoint(x, y))
				{
					hoverText = fillStacksButton.hoverText;
				}
			}
			if (specialButton != null)
			{
				specialButton.tryHover(x, y, 0.25f);
			}
			if (showReceivingMenu)
			{
				Item item_grab_hovered_item = ItemsToGrabMenu.hover(x, y, heldItem);
				if (item_grab_hovered_item != null)
				{
					hoveredItem = item_grab_hovered_item;
				}
			}
			if (junimoNoteIcon != null)
			{
				junimoNoteIcon.tryHover(x, y);
				if (junimoNoteIcon.containsPoint(x, y))
				{
					hoverText = junimoNoteIcon.hoverText;
				}
				if (GameMenu.bundleItemHovered)
				{
					junimoNoteIcon.scale = junimoNoteIcon.baseScale + (float)Math.Sin((float)junimoNotePulser / 100f) / 4f;
					junimoNotePulser += (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				}
				else
				{
					junimoNotePulser = 0;
					junimoNoteIcon.scale = junimoNoteIcon.baseScale;
				}
			}
			if (hoverText != null)
			{
				return;
			}
			if (organizeButton != null)
			{
				hoverText = null;
				organizeButton.tryHover(x, y);
				if (organizeButton.containsPoint(x, y))
				{
					hoverText = organizeButton.hoverText;
				}
			}
			if (shippingBin)
			{
				hoverText = null;
				if (lastShippedHolder.containsPoint(x, y) && Game1.getFarm().lastItemShipped != null)
				{
					hoverText = lastShippedHolder.hoverText;
				}
			}
			if (chestColorPicker != null)
			{
				chestColorPicker.performHoverAction(x, y);
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (drawBG)
			{
				b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			}
			base.draw(b, drawUpperPortion: false, drawDescriptionArea: false);
			if (showReceivingMenu)
			{
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 64, yPositionOnScreen + height / 2 + 64 + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 64, yPositionOnScreen + height / 2 + 64 - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 40, yPositionOnScreen + height / 2 + 64 - 44), new Rectangle(4, 372, 8, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				if ((source != 1 || sourceItem == null || !(sourceItem is Chest) || ((sourceItem as Chest).SpecialChestType != Chest.SpecialChestTypes.MiniShippingBin && (sourceItem as Chest).SpecialChestType != Chest.SpecialChestTypes.JunimoChest && (sourceItem as Chest).SpecialChestType != Chest.SpecialChestTypes.Enricher)) && source != 0)
				{
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 72, yPositionOnScreen + 64 + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 72, yPositionOnScreen + 64 - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Rectangle sourceRect = new Rectangle(127, 412, 10, 11);
					switch (source)
					{
					case 3:
						sourceRect.X += 10;
						break;
					case 2:
						sourceRect.X += 20;
						break;
					}
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 52, yPositionOnScreen + 64 - 44), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				Game1.drawDialogueBox(ItemsToGrabMenu.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, ItemsToGrabMenu.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder, ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearSideBorder * 2, ItemsToGrabMenu.height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2, speaker: false, drawOnlyBox: true);
				ItemsToGrabMenu.draw(b);
			}
			else if (message != null)
			{
				Game1.drawDialogueBox(Game1.uiViewport.Width / 2, ItemsToGrabMenu.yPositionOnScreen + ItemsToGrabMenu.height / 2, speaker: false, drawOnlyBox: false, message);
			}
			if (poof != null)
			{
				poof.draw(b, localPosition: true);
			}
			foreach (TransferredItemSprite transferredItemSprite in _transferredItemSprites)
			{
				transferredItemSprite.Draw(b);
			}
			if (shippingBin && Game1.getFarm().lastItemShipped != null)
			{
				lastShippedHolder.draw(b);
				Game1.getFarm().lastItemShipped.drawInMenu(b, new Vector2(lastShippedHolder.bounds.X + 16, lastShippedHolder.bounds.Y + 16), 1f);
				b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + -8, lastShippedHolder.bounds.Bottom - 100), new Rectangle(325, 448, 5, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + 84, lastShippedHolder.bounds.Bottom - 100), new Rectangle(325, 448, 5, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + -8, lastShippedHolder.bounds.Bottom - 44), new Rectangle(325, 452, 5, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + 84, lastShippedHolder.bounds.Bottom - 44), new Rectangle(325, 452, 5, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (colorPickerToggleButton != null)
			{
				colorPickerToggleButton.draw(b);
			}
			else if (specialButton != null)
			{
				specialButton.draw(b);
			}
			if (chestColorPicker != null)
			{
				chestColorPicker.draw(b);
			}
			if (organizeButton != null)
			{
				organizeButton.draw(b);
			}
			if (fillStacksButton != null)
			{
				fillStacksButton.draw(b);
			}
			if (junimoNoteIcon != null)
			{
				junimoNoteIcon.draw(b);
			}
			if (hoverText != null && (hoveredItem == null || hoveredItem == null || ItemsToGrabMenu == null))
			{
				if (hoverAmount > 0)
				{
					IClickableMenu.drawToolTip(b, hoverText, "", null, heldItem: true, -1, 0, -1, -1, null, hoverAmount);
				}
				else
				{
					IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
				}
			}
			if (hoveredItem != null)
			{
				IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, heldItem != null);
			}
			else if (hoveredItem != null && ItemsToGrabMenu != null)
			{
				IClickableMenu.drawToolTip(b, ItemsToGrabMenu.descriptionText, ItemsToGrabMenu.descriptionTitle, hoveredItem, heldItem != null);
			}
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
			}
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			Console.WriteLine("ItemGrabMenu.emergencyShutDown");
			if (heldItem != null)
			{
				Console.WriteLine("Taking " + heldItem.Name);
				heldItem = Game1.player.addItemToInventory(heldItem);
			}
			if (heldItem != null)
			{
				DropHeldItem();
			}
			if (essential)
			{
				Console.WriteLine("essential");
				foreach (Item item in ItemsToGrabMenu.actualInventory)
				{
					if (item != null)
					{
						Console.WriteLine("Taking " + item.Name);
						Item leftOver = Game1.player.addItemToInventory(item);
						if (leftOver != null)
						{
							Console.WriteLine("Dropping " + leftOver.Name);
							Game1.createItemDebris(leftOver, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
						}
					}
				}
			}
			else
			{
				Console.WriteLine("essential");
			}
		}
	}
}
