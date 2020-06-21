using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class InventoryPage : IClickableMenu
	{
		public const int region_inventory = 100;

		public const int region_hat = 101;

		public const int region_ring1 = 102;

		public const int region_ring2 = 103;

		public const int region_boots = 104;

		public const int region_trashCan = 105;

		public const int region_organizeButton = 106;

		public const int region_accessory = 107;

		public const int region_shirt = 108;

		public const int region_pants = 109;

		public const int region_shoes = 110;

		public InventoryMenu inventory;

		private string descriptionText = "";

		private string hoverText = "";

		private string descriptionTitle = "";

		private string hoverTitle = "";

		private int hoverAmount;

		private Item hoveredItem;

		private Item itemToHold;

		public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

		public ClickableComponent portrait;

		public ClickableTextureComponent trashCan;

		public ClickableTextureComponent organizeButton;

		private float trashCanLidRotation;

		public ClickableTextureComponent junimoNoteIcon;

		private int junimoNotePulser;

		public InventoryPage(int x, int y, int width, int height)
			: base(x, y, width, height)
		{
			inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth, playerInventory: true);
			equipmentIcons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 48, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 256 - 12, 64, 64), "Left Ring")
			{
				myID = 102,
				downNeighborID = 103,
				upNeighborID = Game1.player.MaxItems - 12,
				rightNeighborID = 101,
				fullyImmutable = true
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 48, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 320 - 12, 64, 64), "Right Ring")
			{
				myID = 103,
				upNeighborID = 102,
				downNeighborID = 104,
				rightNeighborID = 108,
				fullyImmutable = true
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 48, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 384 - 12, 64, 64), "Boots")
			{
				myID = 104,
				upNeighborID = 103,
				rightNeighborID = 109,
				fullyImmutable = true
			});
			portrait = new ClickableComponent(new Rectangle(xPositionOnScreen + 192 - 8 - 64 + 32, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8 + 64, 64, 96), "32");
			trashCan = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 3 + 576 + 32, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192 + 64, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f)
			{
				myID = 105,
				upNeighborID = 106,
				leftNeighborID = 101
			};
			organizeButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 + 8, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f)
			{
				myID = 106,
				downNeighborID = 105,
				leftNeighborID = 11,
				upNeighborID = 898
			};
			equipmentIcons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 48 + 208, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 256 - 12, 64, 64), "Hat")
			{
				myID = 101,
				leftNeighborID = 102,
				downNeighborID = 108,
				upNeighborID = Game1.player.MaxItems - 12,
				rightNeighborID = 105,
				fullyImmutable = true
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 48 + 208, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 320 - 12, 64, 64), "Shirt")
			{
				myID = 108,
				upNeighborID = 101,
				downNeighborID = 109,
				rightNeighborID = 105,
				leftNeighborID = 103,
				fullyImmutable = true
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 48 + 208, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 384 - 12, 64, 64), "Pants")
			{
				myID = 109,
				upNeighborID = 108,
				rightNeighborID = 105,
				leftNeighborID = 104,
				fullyImmutable = true
			});
			if (Game1.player.hasOrWillReceiveMail("canReadJunimoText") && !Game1.player.hasOrWillReceiveMail("JojaMember") && (!Game1.MasterPlayer.hasCompletedCommunityCenter() || (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("hasSeenAbandonedJunimoNote") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))))
			{
				junimoNoteIcon = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + 96, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f)
				{
					myID = 898,
					leftNeighborID = 11,
					downNeighborID = 106
				};
			}
		}

		protected virtual bool checkHeldItem(Func<Item, bool> f = null)
		{
			return f?.Invoke(Game1.player.CursorSlotItem) ?? (Game1.player.CursorSlotItem != null);
		}

		protected virtual Item takeHeldItem()
		{
			Item cursorSlotItem = Game1.player.CursorSlotItem;
			Game1.player.CursorSlotItem = null;
			return cursorSlotItem;
		}

		protected virtual void setHeldItem(Item item)
		{
			if (item != null)
			{
				item.NetFields.Parent = null;
			}
			Game1.player.CursorSlotItem = item;
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.isAnyGamePadButtonBeingPressed() && Game1.options.doesInputListContain(Game1.options.menuButton, key) && checkHeldItem())
			{
				Game1.setMousePosition(trashCan.bounds.Center);
			}
			if (key.Equals(Keys.Delete) && checkHeldItem((Item i) => i?.canBeTrashed() ?? false))
			{
				Utility.trashItem(takeHeldItem());
			}
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot1, key))
			{
				Game1.player.CurrentToolIndex = 0;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot2, key))
			{
				Game1.player.CurrentToolIndex = 1;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot3, key))
			{
				Game1.player.CurrentToolIndex = 2;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot4, key))
			{
				Game1.player.CurrentToolIndex = 3;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot5, key))
			{
				Game1.player.CurrentToolIndex = 4;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot6, key))
			{
				Game1.player.CurrentToolIndex = 5;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot7, key))
			{
				Game1.player.CurrentToolIndex = 6;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot8, key))
			{
				Game1.player.CurrentToolIndex = 7;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot9, key))
			{
				Game1.player.CurrentToolIndex = 8;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot10, key))
			{
				Game1.player.CurrentToolIndex = 9;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot11, key))
			{
				Game1.player.CurrentToolIndex = 10;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot12, key))
			{
				Game1.player.CurrentToolIndex = 11;
				Game1.playSound("toolSwap");
			}
		}

		public override void setUpForGamePadMode()
		{
			base.setUpForGamePadMode();
			if (inventory != null)
			{
				inventory.setUpForGamePadMode();
			}
			currentRegion = 100;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableComponent c in equipmentIcons)
			{
				if (c.containsPoint(x, y))
				{
					bool heldItemWasNull = !checkHeldItem();
					switch (c.name)
					{
					case "Hat":
						if (checkHeldItem((Item i) => i == null || i is Hat || i is Pan))
						{
							Hat tmp = (Game1.player.CursorSlotItem is Pan) ? new Hat(71) : ((Hat)takeHeldItem());
							Item heldItem2 = (Hat)Game1.player.hat;
							heldItem2 = Utility.PerformSpecialItemGrabReplacement(heldItem2);
							setHeldItem(heldItem2);
							Game1.player.hat.Value = tmp;
							if (Game1.player.hat.Value != null)
							{
								Game1.playSound("grassyStep");
							}
							else if (checkHeldItem())
							{
								Game1.playSound("dwop");
							}
						}
						break;
					case "Left Ring":
						if (checkHeldItem((Item i) => i == null || i is Ring))
						{
							Ring tmp2 = (Ring)takeHeldItem();
							if (Game1.player.leftRing.Value != null)
							{
								Game1.player.leftRing.Value.onUnequip(Game1.player, Game1.currentLocation);
							}
							setHeldItem((Ring)Game1.player.leftRing);
							Game1.player.leftRing.Value = tmp2;
							if (Game1.player.leftRing.Value != null)
							{
								Game1.player.leftRing.Value.onEquip(Game1.player, Game1.currentLocation);
								Game1.playSound("crit");
							}
							else if (checkHeldItem())
							{
								Game1.playSound("dwop");
							}
						}
						break;
					case "Right Ring":
						if (checkHeldItem((Item i) => i == null || i is Ring))
						{
							Ring tmp3 = (Ring)takeHeldItem();
							if (Game1.player.rightRing.Value != null)
							{
								Game1.player.rightRing.Value.onUnequip(Game1.player, Game1.currentLocation);
							}
							setHeldItem((Ring)Game1.player.rightRing);
							Game1.player.rightRing.Value = tmp3;
							if (Game1.player.rightRing.Value != null)
							{
								Game1.player.rightRing.Value.onEquip(Game1.player, Game1.currentLocation);
								Game1.playSound("crit");
							}
							else if (checkHeldItem())
							{
								Game1.playSound("dwop");
							}
						}
						break;
					case "Boots":
						if (checkHeldItem((Item i) => i == null || i is Boots))
						{
							Boots tmp4 = (Boots)takeHeldItem();
							if (Game1.player.boots.Value != null)
							{
								Game1.player.boots.Value.onUnequip();
							}
							setHeldItem((Boots)Game1.player.boots);
							Game1.player.boots.Value = tmp4;
							if (Game1.player.boots.Value != null)
							{
								Game1.player.boots.Value.onEquip();
								Game1.playSound("sandyStep");
								DelayedAction.playSoundAfterDelay("sandyStep", 150);
							}
							else if (checkHeldItem())
							{
								Game1.playSound("dwop");
							}
						}
						break;
					case "Shirt":
						if (checkHeldItem((Item i) => i == null || (i is Clothing && (i as Clothing).clothesType.Value == 0)))
						{
							Clothing tmp5 = (Clothing)takeHeldItem();
							Item heldItem4 = (Clothing)Game1.player.shirtItem;
							heldItem4 = Utility.PerformSpecialItemGrabReplacement(heldItem4);
							setHeldItem(heldItem4);
							Game1.player.shirtItem.Value = tmp5;
							if (Game1.player.shirtItem.Value != null)
							{
								Game1.playSound("sandyStep");
							}
							else if (checkHeldItem())
							{
								Game1.playSound("dwop");
							}
						}
						break;
					case "Pants":
						if (checkHeldItem((Item i) => i == null || (i is Clothing && (i as Clothing).clothesType.Value == 1) || (i is Object && (int)i.parentSheetIndex == 71)))
						{
							Clothing tmp6 = (Game1.player.CursorSlotItem is Object && (int)Game1.player.CursorSlotItem.parentSheetIndex == 71) ? new Clothing(15) : ((Clothing)takeHeldItem());
							Item heldItem6 = (Clothing)Game1.player.pantsItem;
							heldItem6 = Utility.PerformSpecialItemGrabReplacement(heldItem6);
							setHeldItem(heldItem6);
							Game1.player.pantsItem.Value = tmp6;
							if (Game1.player.pantsItem.Value != null)
							{
								Game1.playSound("sandyStep");
							}
							else if (checkHeldItem())
							{
								Game1.playSound("dwop");
							}
						}
						break;
					}
					if (heldItemWasNull && checkHeldItem() && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
					{
						int l;
						for (l = 0; l < Game1.player.items.Count; l++)
						{
							if (Game1.player.items[l] == null || checkHeldItem((Item item) => Game1.player.items[l].canStackWith(item)))
							{
								if (Game1.player.CurrentToolIndex == l && checkHeldItem())
								{
									Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
								}
								setHeldItem(Utility.addItemToInventory(takeHeldItem(), l, inventory.actualInventory));
								if (Game1.player.CurrentToolIndex == l && checkHeldItem())
								{
									Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
								}
								Game1.playSound("stoneStep");
								return;
							}
						}
					}
				}
			}
			setHeldItem(inventory.leftClick(x, y, takeHeldItem(), !Game1.oldKBState.IsKeyDown(Keys.LeftShift)));
			if (checkHeldItem((Item i) => i != null && Utility.IsNormalObjectAtParentSheetIndex(i, 434)))
			{
				Game1.playSound("smallSelect");
				Game1.player.eatObject(takeHeldItem() as Object, overrideFullness: true);
				Game1.exitActiveMenu();
			}
			else if (checkHeldItem() && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
			{
				if (checkHeldItem((Item i) => i is Ring))
				{
					if (Game1.player.leftRing.Value == null)
					{
						Game1.player.leftRing.Value = (takeHeldItem() as Ring);
						Game1.player.leftRing.Value.onEquip(Game1.player, Game1.currentLocation);
						Game1.playSound("crit");
						return;
					}
					if (Game1.player.rightRing.Value == null)
					{
						Game1.player.rightRing.Value = (takeHeldItem() as Ring);
						Game1.player.rightRing.Value.onEquip(Game1.player, Game1.currentLocation);
						Game1.playSound("crit");
						return;
					}
				}
				else if (checkHeldItem((Item i) => i is Hat))
				{
					if (Game1.player.hat.Value == null)
					{
						Game1.player.hat.Value = (takeHeldItem() as Hat);
						Game1.playSound("grassyStep");
						return;
					}
				}
				else if (checkHeldItem((Item i) => i is Boots))
				{
					if (Game1.player.boots.Value == null)
					{
						Game1.player.boots.Value = (takeHeldItem() as Boots);
						Game1.player.boots.Value.onEquip();
						Game1.playSound("sandyStep");
						DelayedAction.playSoundAfterDelay("sandyStep", 150);
						return;
					}
				}
				else if (checkHeldItem((Item i) => i is Clothing && (i as Clothing).clothesType.Value == 0))
				{
					if (Game1.player.shirtItem.Value == null)
					{
						Game1.player.shirtItem.Value = (takeHeldItem() as Clothing);
						Game1.playSound("sandyStep");
						DelayedAction.playSoundAfterDelay("sandyStep", 150);
						return;
					}
				}
				else if (checkHeldItem((Item i) => i is Clothing && (i as Clothing).clothesType.Value == 1) && Game1.player.pantsItem.Value == null)
				{
					Game1.player.pantsItem.Value = (takeHeldItem() as Clothing);
					Game1.playSound("sandyStep");
					DelayedAction.playSoundAfterDelay("sandyStep", 150);
					return;
				}
				if (inventory.getInventoryPositionOfClick(x, y) >= 12)
				{
					int k;
					for (k = 0; k < 12; k++)
					{
						if (Game1.player.items[k] == null || checkHeldItem((Item item) => Game1.player.items[k].canStackWith(item)))
						{
							if (Game1.player.CurrentToolIndex == k && checkHeldItem())
							{
								Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
							}
							setHeldItem(Utility.addItemToInventory(takeHeldItem(), k, inventory.actualInventory));
							if (checkHeldItem())
							{
								Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
							}
							Game1.playSound("stoneStep");
							return;
						}
					}
				}
				else if (inventory.getInventoryPositionOfClick(x, y) < 12)
				{
					int j;
					for (j = 12; j < Game1.player.items.Count; j++)
					{
						if (Game1.player.items[j] == null || checkHeldItem((Item item) => Game1.player.items[j].canStackWith(item)))
						{
							if (Game1.player.CurrentToolIndex == j && checkHeldItem())
							{
								Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
							}
							setHeldItem(Utility.addItemToInventory(takeHeldItem(), j, inventory.actualInventory));
							if (checkHeldItem())
							{
								Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
							}
							Game1.playSound("stoneStep");
							return;
						}
					}
				}
			}
			if (portrait.containsPoint(x, y))
			{
				portrait.name = (portrait.name.Equals("32") ? "8" : "32");
			}
			if (trashCan.containsPoint(x, y) && checkHeldItem((Item i) => i?.canBeTrashed() ?? false))
			{
				Utility.trashItem(takeHeldItem());
				if (Game1.options.SnappyMenus)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
			else if (!isWithinBounds(x, y) && checkHeldItem((Item i) => i?.canBeTrashed() ?? false))
			{
				Game1.playSound("throwDownITem");
				Game1.createItemDebris(takeHeldItem(), Game1.player.getStandingPosition(), Game1.player.FacingDirection).DroppedByPlayerID.Value = Game1.player.UniqueMultiplayerID;
			}
			if (organizeButton != null && organizeButton.containsPoint(x, y))
			{
				ItemGrabMenu.organizeItemsInList(Game1.player.items);
				Game1.playSound("Ship");
			}
			if (junimoNoteIcon != null && junimoNoteIcon.containsPoint(x, y) && readyToClose())
			{
				Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true);
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.Back && organizeButton != null)
			{
				ItemGrabMenu.organizeItemsInList(Game1.player.items);
				Game1.playSound("Ship");
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			setHeldItem(inventory.rightClick(x, y, takeHeldItem()));
		}

		public override void performHoverAction(int x, int y)
		{
			hoverAmount = -1;
			descriptionText = "";
			descriptionTitle = "";
			hoveredItem = inventory.hover(x, y, Game1.player.CursorSlotItem);
			hoverText = inventory.hoverText;
			hoverTitle = inventory.hoverTitle;
			foreach (ClickableComponent c in equipmentIcons)
			{
				if (c.containsPoint(x, y))
				{
					switch (c.name)
					{
					case "Hat":
						if (Game1.player.hat.Value != null)
						{
							hoveredItem = (Hat)Game1.player.hat;
							hoverText = Game1.player.hat.Value.getDescription();
							hoverTitle = Game1.player.hat.Value.DisplayName;
						}
						break;
					case "Right Ring":
						if (Game1.player.rightRing.Value != null)
						{
							hoveredItem = (Ring)Game1.player.rightRing;
							hoverText = Game1.player.rightRing.Value.getDescription();
							hoverTitle = Game1.player.rightRing.Value.DisplayName;
						}
						break;
					case "Left Ring":
						if (Game1.player.leftRing.Value != null)
						{
							hoveredItem = (Ring)Game1.player.leftRing;
							hoverText = Game1.player.leftRing.Value.getDescription();
							hoverTitle = Game1.player.leftRing.Value.DisplayName;
						}
						break;
					case "Boots":
						if (Game1.player.boots.Value != null)
						{
							hoveredItem = (Boots)Game1.player.boots;
							hoverText = Game1.player.boots.Value.getDescription();
							hoverTitle = Game1.player.boots.Value.DisplayName;
						}
						break;
					case "Shirt":
						if (Game1.player.shirtItem.Value != null)
						{
							hoveredItem = (Clothing)Game1.player.shirtItem;
							hoverText = Game1.player.shirtItem.Value.getDescription();
							hoverTitle = Game1.player.shirtItem.Value.DisplayName;
						}
						break;
					case "Pants":
						if (Game1.player.pantsItem.Value != null)
						{
							hoveredItem = (Clothing)Game1.player.pantsItem;
							hoverText = Game1.player.pantsItem.Value.getDescription();
							hoverTitle = Game1.player.pantsItem.Value.DisplayName;
						}
						break;
					}
					c.scale = Math.Min(c.scale + 0.05f, 1.1f);
				}
				c.scale = Math.Max(1f, c.scale - 0.025f);
			}
			if (portrait.containsPoint(x, y))
			{
				portrait.scale += 0.2f;
				hoverText = Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", Game1.player.Level) + Environment.NewLine + Game1.player.getTitle();
			}
			else
			{
				portrait.scale = 0f;
			}
			if (trashCan.containsPoint(x, y))
			{
				if (trashCanLidRotation <= 0f)
				{
					Game1.playSound("trashcanlid");
				}
				trashCanLidRotation = Math.Min(trashCanLidRotation + (float)Math.PI / 48f, (float)Math.PI / 2f);
				if (checkHeldItem() && Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player) > 0)
				{
					hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
					hoverAmount = Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player);
				}
			}
			else if (trashCanLidRotation != 0f)
			{
				trashCanLidRotation = Math.Max(trashCanLidRotation - (float)Math.PI / 24f, 0f);
				if (trashCanLidRotation == 0f)
				{
					Game1.playSound("thudStep");
				}
			}
			if (organizeButton != null)
			{
				organizeButton.tryHover(x, y);
				if (organizeButton.containsPoint(x, y))
				{
					hoverText = organizeButton.hoverText;
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
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override bool readyToClose()
		{
			return !checkHeldItem();
		}

		public override void draw(SpriteBatch b)
		{
			drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192);
			inventory.draw(b);
			foreach (ClickableComponent c in equipmentIcons)
			{
				switch (c.name)
				{
				case "Hat":
					if (Game1.player.hat.Value != null)
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.hat.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, 1f, 0.866f, StackDrawType.Hide);
					}
					else
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 42), Color.White);
					}
					break;
				case "Right Ring":
					if (Game1.player.rightRing.Value != null)
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.rightRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
					}
					else
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
					}
					break;
				case "Left Ring":
					if (Game1.player.leftRing.Value != null)
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.leftRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
					}
					else
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
					}
					break;
				case "Boots":
					if (Game1.player.boots.Value != null)
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.boots.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
					}
					else
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 40), Color.White);
					}
					break;
				case "Shirt":
					if (Game1.player.shirtItem.Value != null)
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.shirtItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
					}
					else
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 69), Color.White);
					}
					break;
				case "Pants":
					if (Game1.player.pantsItem.Value != null)
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.pantsItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
					}
					else
					{
						b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 68), Color.White);
					}
					break;
				}
			}
			b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, new Vector2(xPositionOnScreen + 192 - 64 - 8, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8), Color.White);
			FarmerRenderer.isDrawingForUI = true;
			Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes ? 108 : 0, secondaryArm: false, flip: false), Game1.player.bathingClothes ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes ? 576 : 0, 16, 32), new Vector2(xPositionOnScreen + 192 - 8 - 32, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 320 - 32 - 8), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, Game1.player);
			if (Game1.timeOfDay >= 1900)
			{
				Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes ? 108 : 0, secondaryArm: false, flip: false), Game1.player.bathingClothes ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes ? 576 : 0, 16, 32), new Vector2(xPositionOnScreen + 192 - 8 - 32, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 320 - 32 - 8), Vector2.Zero, 0.8f, 2, Color.DarkBlue * 0.3f, 0f, 1f, Game1.player);
			}
			FarmerRenderer.isDrawingForUI = false;
			Utility.drawTextWithShadow(b, Game1.player.Name, Game1.dialogueFont, new Vector2((float)(xPositionOnScreen + 192 - 8) - Game1.dialogueFont.MeasureString(Game1.player.Name).X / 2f, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 448 + 8), Game1.textColor);
			float offset = 32f;
			string farmName = Game1.content.LoadString("Strings\\UI:Inventory_FarmName", Game1.player.farmName);
			Utility.drawTextWithShadow(b, farmName, Game1.dialogueFont, new Vector2((float)xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(farmName).X / 2f, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 + 4), Game1.textColor);
			string currentFunds = Game1.content.LoadString("Strings\\UI:Inventory_CurrentFunds" + (Game1.player.useSeparateWallets ? "_Separate" : ""), Utility.getNumberWithCommas(Game1.player.Money));
			Utility.drawTextWithShadow(b, currentFunds, Game1.dialogueFont, new Vector2((float)xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(currentFunds).X / 2f, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 320), Game1.textColor);
			string totalEarnings = Game1.content.LoadString("Strings\\UI:Inventory_TotalEarnings" + (Game1.player.useSeparateWallets ? "_Separate" : ""), Utility.getNumberWithCommas((int)Game1.player.totalMoneyEarned));
			Utility.drawTextWithShadow(b, totalEarnings, Game1.dialogueFont, new Vector2((float)xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(totalEarnings).X / 2f, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 384 - 4), Game1.textColor);
			Pet pet_character = Game1.MasterPlayer.getPet();
			string pet_name = Game1.MasterPlayer.getPetDisplayName();
			if (pet_character != null)
			{
				Utility.drawTextWithShadow(b, pet_name, Game1.dialogueFont, new Vector2((float)xPositionOnScreen + offset + 320f + Math.Max(64f, Game1.dialogueFont.MeasureString(Game1.player.Name).X / 2f), yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 448 + 8), Game1.textColor);
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)xPositionOnScreen + offset + 256f + Math.Max(64f, Game1.dialogueFont.MeasureString(Game1.player.Name).X / 2f), yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 448 - 4), new Rectangle(160 + ((!Game1.MasterPlayer.catPerson) ? 48 : 0) + pet_character.whichBreed.Value * 16, 208, 16, 16), Color.White, 0f, Vector2.Zero, 4f);
			}
			if (Game1.player.horseName.Value != null && Game1.player.horseName.Value != "")
			{
				Utility.drawTextWithShadow(b, Game1.player.horseName.Value, Game1.dialogueFont, new Vector2((float)xPositionOnScreen + offset + 384f + Math.Max(64f, Game1.dialogueFont.MeasureString(Game1.player.Name).X / 2f) + ((pet_name != null) ? Math.Max(64f, Game1.dialogueFont.MeasureString(pet_name).X) : 0f), yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 448 + 8), Game1.textColor);
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)xPositionOnScreen + offset + 320f + 8f + Math.Max(64f, Game1.dialogueFont.MeasureString(Game1.player.Name).X / 2f) + ((pet_name != null) ? Math.Max(64f, Game1.dialogueFont.MeasureString(pet_name).X) : 0f), yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 448 - 4), new Rectangle(193, 192, 16, 16), Color.White, 0f, Vector2.Zero, 4f);
			}
			_ = xPositionOnScreen;
			_ = width / 3;
			if (organizeButton != null)
			{
				organizeButton.draw(b);
			}
			trashCan.draw(b);
			b.Draw(Game1.mouseCursors, new Vector2(trashCan.bounds.X + 60, trashCan.bounds.Y + 40), new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10), Color.White, trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
			if (checkHeldItem())
			{
				Game1.player.CursorSlotItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
			}
			if (hoverText != null && !hoverText.Equals(""))
			{
				if (hoverAmount > 0)
				{
					IClickableMenu.drawToolTip(b, hoverText, hoverTitle, null, heldItem: true, -1, 0, -1, -1, null, hoverAmount);
				}
				else
				{
					IClickableMenu.drawToolTip(b, hoverText, hoverTitle, hoveredItem, checkHeldItem());
				}
			}
			if (junimoNoteIcon != null)
			{
				junimoNoteIcon.draw(b);
			}
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			setHeldItem(Game1.player.addItemToInventory(takeHeldItem()));
			if (checkHeldItem())
			{
				Game1.playSound("throwDownITem");
				Game1.createItemDebris(takeHeldItem(), Game1.player.getStandingPosition(), Game1.player.FacingDirection);
			}
		}
	}
}
