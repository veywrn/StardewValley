using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class ForgeMenu : MenuWithInventory
	{
		public enum CraftState
		{
			MissingIngredients,
			MissingShards,
			Valid,
			InvalidRecipe
		}

		protected int _timeUntilCraft;

		protected int _clankEffectTimer;

		protected int _sparklingTimer;

		public const int region_leftIngredient = 998;

		public const int region_rightIngredient = 997;

		public const int region_startButton = 996;

		public const int region_resultItem = 995;

		public const int region_unforgeButton = 994;

		public ClickableTextureComponent craftResultDisplay;

		public ClickableTextureComponent leftIngredientSpot;

		public ClickableTextureComponent rightIngredientSpot;

		public ClickableTextureComponent startTailoringButton;

		public ClickableComponent unforgeButton;

		public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

		public const int region_ring_1 = 110;

		public const int region_ring_2 = 111;

		public const int CRAFT_TIME = 1600;

		public Texture2D forgeTextures;

		protected Dictionary<Item, bool> _highlightDictionary;

		protected Dictionary<string, Item> _lastValidEquippedItems;

		protected List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();

		private bool unforging;

		protected string displayedDescription = "";

		protected CraftState _craftState;

		public Vector2 questionMarkOffset;

		public ForgeMenu()
			: base(null, okButton: true, trashCan: true, 12, 132)
		{
			Game1.playSound("bigSelect");
			if (yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				movePosition(0, -IClickableMenu.spaceToClearTopBorder);
			}
			inventory.highlightMethod = HighlightItems;
			forgeTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\ForgeMenu");
			_CreateButtons();
			if (trashCan != null)
			{
				trashCan.myID = 106;
			}
			if (okButton != null)
			{
				okButton.leftNeighborID = 11;
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
			_ValidateCraft();
		}

		protected void _CreateButtons()
		{
			leftIngredientSpot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 204, yPositionOnScreen + 212, 64, 64), forgeTextures, new Rectangle(142, 0, 16, 16), 4f)
			{
				myID = 998,
				downNeighborID = -99998,
				leftNeighborID = 110,
				rightNeighborID = 997,
				item = ((leftIngredientSpot != null) ? leftIngredientSpot.item : null),
				fullyImmutable = true
			};
			rightIngredientSpot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 348, yPositionOnScreen + 212, 64, 64), forgeTextures, new Rectangle(142, 0, 16, 16), 4f)
			{
				myID = 997,
				downNeighborID = 996,
				leftNeighborID = 998,
				rightNeighborID = 994,
				item = ((rightIngredientSpot != null) ? rightIngredientSpot.item : null),
				fullyImmutable = true
			};
			startTailoringButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 204, yPositionOnScreen + 308, 52, 56), forgeTextures, new Rectangle(0, 80, 13, 14), 4f)
			{
				myID = 996,
				downNeighborID = -99998,
				leftNeighborID = 111,
				rightNeighborID = 994,
				upNeighborID = 998,
				item = ((startTailoringButton != null) ? startTailoringButton.item : null),
				fullyImmutable = true
			};
			unforgeButton = new ClickableComponent(new Rectangle(xPositionOnScreen + 484, yPositionOnScreen + 312, 40, 44), "Unforge")
			{
				myID = 994,
				downNeighborID = -99998,
				leftNeighborID = 996,
				rightNeighborID = 995,
				upNeighborID = 997,
				fullyImmutable = true
			};
			if (inventory.inventory != null && inventory.inventory.Count >= 12)
			{
				for (int j = 0; j < 12; j++)
				{
					if (inventory.inventory[j] != null)
					{
						inventory.inventory[j].upNeighborID = -99998;
					}
				}
			}
			craftResultDisplay = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 660, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232, 64, 64), forgeTextures, new Rectangle(0, 208, 16, 16), 4f)
			{
				myID = 995,
				downNeighborID = -99998,
				leftNeighborID = 996,
				upNeighborID = 997,
				item = ((craftResultDisplay != null) ? craftResultDisplay.item : null)
			};
			equipmentIcons = new List<ClickableComponent>();
			equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, 64, 64), "Ring1")
			{
				myID = 110,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborID = -99998
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, 64, 64), "Ring2")
			{
				myID = 111,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998
			});
			for (int i = 0; i < equipmentIcons.Count; i++)
			{
				equipmentIcons[i].bounds.X = xPositionOnScreen - 64 + 9;
				equipmentIcons[i].bounds.Y = yPositionOnScreen + 192 + i * 64;
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public bool IsBusy()
		{
			if (_timeUntilCraft <= 0)
			{
				return _sparklingTimer > 0;
			}
			return true;
		}

		public override bool readyToClose()
		{
			if (base.readyToClose() && heldItem == null)
			{
				return !IsBusy();
			}
			return false;
		}

		public bool HighlightItems(Item i)
		{
			if (i == null)
			{
				return false;
			}
			if (i != null && !IsValidCraftIngredient(i))
			{
				return false;
			}
			if (_highlightDictionary == null)
			{
				GenerateHighlightDictionary();
			}
			if (!_highlightDictionary.ContainsKey(i))
			{
				_highlightDictionary = null;
				GenerateHighlightDictionary();
			}
			return _highlightDictionary[i];
		}

		public void GenerateHighlightDictionary()
		{
			_highlightDictionary = new Dictionary<Item, bool>();
			List<Item> item_list = new List<Item>(inventory.actualInventory);
			if (Game1.player.leftRing.Value != null)
			{
				item_list.Add(Game1.player.leftRing.Value);
			}
			if (Game1.player.rightRing.Value != null)
			{
				item_list.Add(Game1.player.rightRing.Value);
			}
			foreach (Item item in item_list)
			{
				if (item != null)
				{
					if (Utility.IsNormalObjectAtParentSheetIndex(item, 848))
					{
						_highlightDictionary[item] = true;
					}
					else if (leftIngredientSpot.item == null && rightIngredientSpot.item == null)
					{
						bool valid = false;
						if (item is Ring)
						{
							valid = true;
						}
						if (item is Tool && BaseEnchantment.GetAvailableEnchantmentsForItem(item as Tool).Count > 0)
						{
							valid = true;
						}
						if (BaseEnchantment.GetEnchantmentFromItem(null, item) != null)
						{
							valid = true;
						}
						_highlightDictionary[item] = valid;
					}
					else if (leftIngredientSpot.item != null && rightIngredientSpot.item != null)
					{
						_highlightDictionary[item] = false;
					}
					else if (leftIngredientSpot.item != null)
					{
						_highlightDictionary[item] = IsValidCraft(leftIngredientSpot.item, item);
					}
					else
					{
						_highlightDictionary[item] = IsValidCraft(item, rightIngredientSpot.item);
					}
				}
			}
		}

		private void _leftIngredientSpotClicked()
		{
			Item old_item = leftIngredientSpot.item;
			if ((heldItem == null || IsValidCraftIngredient(heldItem)) && (heldItem == null || heldItem is Tool || heldItem is Ring))
			{
				Game1.playSound("stoneStep");
				leftIngredientSpot.item = heldItem;
				heldItem = old_item;
				_highlightDictionary = null;
				_ValidateCraft();
			}
		}

		public bool IsValidCraftIngredient(Item item)
		{
			if (!item.canBeTrashed() && (!(item is Tool) || BaseEnchantment.GetAvailableEnchantmentsForItem(item as Tool).Count <= 0))
			{
				return false;
			}
			return true;
		}

		private void _rightIngredientSpotClicked()
		{
			Item old_item = rightIngredientSpot.item;
			if ((heldItem == null || IsValidCraftIngredient(heldItem)) && (heldItem == null || (int)heldItem.parentSheetIndex != 848))
			{
				Game1.playSound("stoneStep");
				rightIngredientSpot.item = heldItem;
				heldItem = old_item;
				_highlightDictionary = null;
				_ValidateCraft();
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Delete)
			{
				if (heldItem != null && IsValidCraftIngredient(heldItem))
				{
					Utility.trashItem(heldItem);
					heldItem = null;
				}
			}
			else
			{
				base.receiveKeyPress(key);
			}
		}

		public bool IsHoldingEquippedItem()
		{
			if (heldItem == null)
			{
				return false;
			}
			if (!Game1.player.IsEquippedItem(heldItem))
			{
				return Game1.player.IsEquippedItem(Utility.PerformSpecialItemGrabReplacement(heldItem));
			}
			return true;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item old_held_item = heldItem;
			Game1.player.IsEquippedItem(old_held_item);
			base.receiveLeftClick(x, y, playSound: true);
			foreach (ClickableComponent c in equipmentIcons)
			{
				if (c.containsPoint(x, y))
				{
					string name = c.name;
					if (!(name == "Ring1"))
					{
						if (name == "Ring2" && (HighlightItems(Game1.player.rightRing.Value) || Game1.player.rightRing.Value == null))
						{
							Item item_to_place2 = heldItem;
							Item old_item2 = Game1.player.rightRing.Value;
							if (old_item2 != heldItem && (item_to_place2 == null || item_to_place2 is Ring))
							{
								if (Game1.player.rightRing.Value != null)
								{
									Game1.player.rightRing.Value.onUnequip(Game1.player, Game1.currentLocation);
								}
								Game1.player.rightRing.Value = (item_to_place2 as Ring);
								heldItem = old_item2;
								if (Game1.player.rightRing.Value != null)
								{
									Game1.player.rightRing.Value.onEquip(Game1.player, Game1.currentLocation);
									Game1.playSound("crit");
								}
								else if (heldItem != null)
								{
									Game1.playSound("dwop");
								}
								_highlightDictionary = null;
								_ValidateCraft();
							}
						}
					}
					else if (HighlightItems(Game1.player.leftRing.Value) || Game1.player.leftRing.Value == null)
					{
						Item item_to_place = heldItem;
						Item old_item = Game1.player.leftRing.Value;
						if (old_item != heldItem && (item_to_place == null || item_to_place is Ring))
						{
							if (Game1.player.leftRing.Value != null)
							{
								Game1.player.leftRing.Value.onUnequip(Game1.player, Game1.currentLocation);
							}
							Game1.player.leftRing.Value = (item_to_place as Ring);
							heldItem = old_item;
							if (Game1.player.leftRing.Value != null)
							{
								Game1.player.leftRing.Value.onEquip(Game1.player, Game1.currentLocation);
								Game1.playSound("crit");
							}
							else if (heldItem != null)
							{
								Game1.playSound("dwop");
							}
							_highlightDictionary = null;
							_ValidateCraft();
						}
					}
					return;
				}
			}
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && old_held_item != heldItem && heldItem != null)
			{
				if (heldItem is Tool || (heldItem is Ring && leftIngredientSpot.item == null))
				{
					_leftIngredientSpotClicked();
				}
				else
				{
					_rightIngredientSpotClicked();
				}
			}
			if (IsBusy())
			{
				return;
			}
			if (leftIngredientSpot.containsPoint(x, y))
			{
				_leftIngredientSpotClicked();
				if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && heldItem != null)
				{
					if (Game1.player.IsEquippedItem(heldItem))
					{
						heldItem = null;
					}
					else
					{
						heldItem = inventory.tryToAddItem(heldItem, "");
					}
				}
			}
			else if (rightIngredientSpot.containsPoint(x, y))
			{
				_rightIngredientSpotClicked();
				if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && heldItem != null)
				{
					if (Game1.player.IsEquippedItem(heldItem))
					{
						heldItem = null;
					}
					else
					{
						heldItem = inventory.tryToAddItem(heldItem, "");
					}
				}
			}
			else if (startTailoringButton.containsPoint(x, y))
			{
				if (heldItem == null)
				{
					bool fail = false;
					if (!CanFitCraftedItem())
					{
						Game1.playSound("cancel");
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
						_timeUntilCraft = 0;
						fail = true;
					}
					if (!fail && IsValidCraft(leftIngredientSpot.item, rightIngredientSpot.item) && Game1.player.hasItemInInventory(848, GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item)))
					{
						Game1.playSound("bigSelect");
						startTailoringButton.scale = startTailoringButton.baseScale;
						_timeUntilCraft = 1600;
						_clankEffectTimer = 300;
						_UpdateDescriptionText();
						int crystals2 = GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item);
						for (int k = 0; k < crystals2; k++)
						{
							tempSprites.Add(new TemporaryAnimatedSprite("", new Rectangle(143, 17, 14, 15), new Vector2(xPositionOnScreen + 276, yPositionOnScreen + 300), flipped: false, 0.1f, Color.White)
							{
								texture = forgeTextures,
								motion = new Vector2(-4f, -4f),
								scale = 4f,
								layerDepth = 1f,
								startSound = "boulderCrack",
								delayBeforeAnimationStart = 1400 / crystals2 * k
							});
						}
						if (rightIngredientSpot.item != null && (int)rightIngredientSpot.item.parentSheetIndex == 74)
						{
							_sparklingTimer = 900;
							Rectangle r2 = leftIngredientSpot.bounds;
							r2.Offset(-32, -32);
							List<TemporaryAnimatedSprite> sparkles = Utility.sparkleWithinArea(r2, 6, Color.White, 80, 1600);
							sparkles.First().startSound = "discoverMineral";
							tempSprites.AddRange(sparkles);
							r2 = rightIngredientSpot.bounds;
							r2.Inflate(-16, -16);
							Vector2 position2 = Utility.getRandomPositionInThisRectangle(r2, Game1.random);
							int num = 30;
							for (int j = 0; j < num; j++)
							{
								position2 = Utility.getRandomPositionInThisRectangle(r2, Game1.random);
								tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 48, 2, 2), position2, flipped: false, 0f, Color.White)
								{
									motion = new Vector2(-4f, 0f),
									yPeriodic = true,
									yPeriodicRange = 16f,
									yPeriodicLoopTime = 1200f,
									scale = 4f,
									layerDepth = 1f,
									animationLength = 12,
									interval = Game1.random.Next(20, 40),
									totalNumberOfLoops = 1,
									delayBeforeAnimationStart = _clankEffectTimer / num * j
								});
							}
						}
					}
					else
					{
						Game1.playSound("sell");
					}
				}
				else
				{
					Game1.playSound("sell");
				}
			}
			else if (unforgeButton.containsPoint(x, y))
			{
				if (rightIngredientSpot.item == null)
				{
					if (IsValidUnforge())
					{
						if (leftIngredientSpot.item is MeleeWeapon && !Game1.player.couldInventoryAcceptThisObject(848, (leftIngredientSpot.item as MeleeWeapon).GetTotalForgeLevels() * 5 + ((leftIngredientSpot.item as MeleeWeapon).GetTotalForgeLevels() - 1) * 2))
						{
							displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_noroom");
							Game1.playSound("cancel");
						}
						else if (leftIngredientSpot.item is CombinedRing && Game1.player.freeSpotsInInventory() < 2)
						{
							displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_noroom");
							Game1.playSound("cancel");
						}
						else
						{
							unforging = true;
							_timeUntilCraft = 1600;
							int crystals = GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item) / 2;
							for (int i = 0; i < crystals; i++)
							{
								Vector2 motion = new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-4, 5));
								if (motion.X == 0f && motion.Y == 0f)
								{
									motion = new Vector2(-4f, -4f);
								}
								tempSprites.Add(new TemporaryAnimatedSprite("", new Rectangle(143, 17, 14, 15), new Vector2(leftIngredientSpot.bounds.X, leftIngredientSpot.bounds.Y), flipped: false, 0.1f, Color.White)
								{
									alpha = 0.01f,
									alphaFade = -0.1f,
									alphaFadeFade = -0.005f,
									texture = forgeTextures,
									motion = motion,
									scale = 4f,
									layerDepth = 1f,
									startSound = "boulderCrack",
									delayBeforeAnimationStart = 1100 / crystals * i
								});
							}
							Game1.playSound("debuffHit");
						}
					}
					else
					{
						displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_invalid");
						Game1.playSound("cancel");
					}
				}
				else
				{
					if (IsValidUnforge(ignore_right_slot_occupancy: true))
					{
						displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_right_slot");
					}
					else
					{
						displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_invalid");
					}
					Game1.playSound("cancel");
				}
			}
			if (heldItem == null || isWithinBounds(x, y) || !heldItem.canBeTrashed())
			{
				return;
			}
			if (Game1.player.IsEquippedItem(heldItem))
			{
				if (heldItem == Game1.player.hat.Value)
				{
					Game1.player.hat.Value = null;
				}
				else if (heldItem == Game1.player.shirtItem.Value)
				{
					Game1.player.shirtItem.Value = null;
				}
				else if (heldItem == Game1.player.pantsItem.Value)
				{
					Game1.player.pantsItem.Value = null;
				}
			}
			Game1.playSound("throwDownITem");
			Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
			heldItem = null;
		}

		protected virtual bool CheckHeldItem(Func<Item, bool> f = null)
		{
			return f?.Invoke(heldItem) ?? (heldItem != null);
		}

		public virtual int GetForgeCostAtLevel(int level)
		{
			return 10 + level * 5;
		}

		public virtual int GetForgeCost(Item left_item, Item right_item)
		{
			if (right_item != null && (int)right_item.parentSheetIndex == 896)
			{
				return 20;
			}
			if (right_item != null && (int)right_item.parentSheetIndex == 74)
			{
				return 20;
			}
			if (right_item != null && (int)right_item.parentSheetIndex == 72)
			{
				return 10;
			}
			if (left_item is MeleeWeapon && right_item is MeleeWeapon)
			{
				return 10;
			}
			if (left_item != null && left_item is Tool)
			{
				return GetForgeCostAtLevel((left_item as Tool).GetTotalForgeLevels());
			}
			if (left_item != null && left_item is Ring && right_item != null && right_item is Ring)
			{
				return 20;
			}
			return 1;
		}

		protected void _ValidateCraft()
		{
			Item left_item = leftIngredientSpot.item;
			Item right_item = rightIngredientSpot.item;
			if (left_item == null || right_item == null)
			{
				_craftState = CraftState.MissingIngredients;
			}
			else if (IsValidCraft(left_item, right_item))
			{
				_craftState = CraftState.Valid;
				Item left_item_clone = left_item.getOne();
				if (right_item != null && Utility.IsNormalObjectAtParentSheetIndex(right_item, 72))
				{
					(left_item_clone as Tool).AddEnchantment(new DiamondEnchantment());
					craftResultDisplay.item = left_item_clone;
				}
				else
				{
					craftResultDisplay.item = CraftItem(left_item_clone, right_item.getOne());
				}
			}
			else
			{
				_craftState = CraftState.InvalidRecipe;
			}
			_UpdateDescriptionText();
		}

		protected void _UpdateDescriptionText()
		{
			if (IsBusy())
			{
				if (rightIngredientSpot.item != null && (int)rightIngredientSpot.item.parentSheetIndex == 74)
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_enchanting");
				}
				else
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_forging");
				}
			}
			else if (_craftState == CraftState.MissingIngredients)
			{
				displayedDescription = (displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_description1") + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Forge_description2"));
			}
			else if (_craftState == CraftState.MissingShards)
			{
				if (heldItem != null && heldItem.ParentSheetIndex == 848)
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_shards");
				}
				else
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_notenoughshards");
				}
			}
			else if (_craftState == CraftState.Valid)
			{
				if (!CanFitCraftedItem())
				{
					displayedDescription = Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588");
				}
				else
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_valid");
				}
			}
			else if (_craftState == CraftState.InvalidRecipe)
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_wrongorder");
			}
			else
			{
				displayedDescription = "";
			}
		}

		public bool IsValidCraft(Item left_item, Item right_item)
		{
			if (left_item == null || right_item == null)
			{
				return false;
			}
			if (left_item is Tool && (left_item as Tool).CanForge(right_item))
			{
				return true;
			}
			if (left_item is Ring && right_item is Ring && (left_item as Ring).CanCombine(right_item as Ring))
			{
				return true;
			}
			return false;
		}

		public Item CraftItem(Item left_item, Item right_item, bool forReal = false)
		{
			if (left_item == null || right_item == null)
			{
				return null;
			}
			if (left_item is Tool && !(left_item as Tool).Forge(right_item, forReal))
			{
				return null;
			}
			if (left_item is Ring && right_item is Ring)
			{
				left_item = (left_item as Ring).Combine(right_item as Ring);
			}
			return left_item;
		}

		public void SpendRightItem()
		{
			if (rightIngredientSpot.item != null)
			{
				rightIngredientSpot.item.Stack--;
				if (rightIngredientSpot.item.Stack <= 0 || rightIngredientSpot.item.maximumStackSize() == 1)
				{
					rightIngredientSpot.item = null;
				}
			}
		}

		public void SpendLeftItem()
		{
			if (leftIngredientSpot.item != null)
			{
				leftIngredientSpot.item.Stack--;
				if (leftIngredientSpot.item.Stack <= 0 || leftIngredientSpot.item.maximumStackSize() == 1)
				{
					leftIngredientSpot.item = null;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!IsBusy())
			{
				base.receiveRightClick(x, y, playSound: true);
			}
		}

		public override void performHoverAction(int x, int y)
		{
			if (IsBusy())
			{
				return;
			}
			hoveredItem = null;
			base.performHoverAction(x, y);
			hoverText = "";
			for (int i = 0; i < equipmentIcons.Count; i++)
			{
				if (equipmentIcons[i].containsPoint(x, y))
				{
					if (equipmentIcons[i].name == "Ring1")
					{
						hoveredItem = Game1.player.leftRing.Value;
					}
					else if (equipmentIcons[i].name == "Ring2")
					{
						hoveredItem = Game1.player.rightRing.Value;
					}
				}
			}
			if (craftResultDisplay.visible && craftResultDisplay.containsPoint(x, y) && craftResultDisplay.item != null)
			{
				hoveredItem = craftResultDisplay.item;
			}
			if (leftIngredientSpot.containsPoint(x, y) && leftIngredientSpot.item != null)
			{
				hoveredItem = leftIngredientSpot.item;
			}
			if (rightIngredientSpot.containsPoint(x, y) && rightIngredientSpot.item != null)
			{
				hoveredItem = rightIngredientSpot.item;
			}
			if (unforgeButton.containsPoint(x, y))
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Forge_Unforge");
			}
			if (_craftState == CraftState.Valid && CanFitCraftedItem())
			{
				startTailoringButton.tryHover(x, y, 0.33f);
			}
			else
			{
				startTailoringButton.tryHover(-999, -999);
			}
		}

		public bool CanFitCraftedItem()
		{
			if (craftResultDisplay.item != null && !Utility.canItemBeAddedToThisInventoryList(craftResultDisplay.item, inventory.actualInventory))
			{
				return false;
			}
			return true;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			int yPositionForInventory = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
			inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, playerInventory: false, null, inventory.highlightMethod);
			_CreateButtons();
		}

		public override void emergencyShutDown()
		{
			_OnCloseMenu();
			base.emergencyShutDown();
		}

		public override void update(GameTime time)
		{
			base.update(time);
			for (int l = tempSprites.Count - 1; l >= 0; l--)
			{
				if (tempSprites[l].update(time))
				{
					tempSprites.RemoveAt(l);
				}
			}
			if (leftIngredientSpot.item != null && rightIngredientSpot.item != null && !Game1.player.hasItemInInventory(848, GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item)))
			{
				if (_craftState != CraftState.MissingShards)
				{
					_craftState = CraftState.MissingShards;
					craftResultDisplay.item = null;
					_UpdateDescriptionText();
				}
			}
			else if (_craftState == CraftState.MissingShards)
			{
				_ValidateCraft();
			}
			descriptionText = displayedDescription;
			questionMarkOffset.X = (float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.5) * 4f;
			questionMarkOffset.Y = (float)Math.Cos(time.TotalGameTime.TotalSeconds * 5.0) * -4f;
			bool can_fit_crafted_item = CanFitCraftedItem();
			if ((_craftState == CraftState.Valid && !IsBusy()) & can_fit_crafted_item)
			{
				craftResultDisplay.visible = true;
			}
			else
			{
				craftResultDisplay.visible = false;
			}
			if (_timeUntilCraft <= 0 && _sparklingTimer <= 0)
			{
				return;
			}
			startTailoringButton.tryHover(startTailoringButton.bounds.Center.X, startTailoringButton.bounds.Center.Y, 0.33f);
			_timeUntilCraft -= (int)time.ElapsedGameTime.TotalMilliseconds;
			_clankEffectTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (_timeUntilCraft <= 0 && _sparklingTimer > 0)
			{
				_sparklingTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			else if (_clankEffectTimer <= 0 && !unforging)
			{
				_clankEffectTimer = 450;
				if (rightIngredientSpot.item != null && (int)rightIngredientSpot.item.parentSheetIndex == 74)
				{
					Rectangle r2 = rightIngredientSpot.bounds;
					r2.Inflate(-16, -16);
					Vector2 position3 = Utility.getRandomPositionInThisRectangle(r2, Game1.random);
					int num = 30;
					for (int k = 0; k < num; k++)
					{
						position3 = Utility.getRandomPositionInThisRectangle(r2, Game1.random);
						tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 48, 2, 2), position3, flipped: false, 0f, Color.White)
						{
							motion = new Vector2(-4f, 0f),
							yPeriodic = true,
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 1200f,
							scale = 4f,
							layerDepth = 1f,
							animationLength = 12,
							interval = Game1.random.Next(20, 40),
							totalNumberOfLoops = 1,
							delayBeforeAnimationStart = _clankEffectTimer / num * k
						});
					}
				}
				else
				{
					Game1.playSound("crafting");
					Game1.playSound("clank");
					Rectangle r = leftIngredientSpot.bounds;
					r.Inflate(-21, -21);
					Vector2 position = Utility.getRandomPositionInThisRectangle(r, Game1.random);
					tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
					{
						motion = new Vector2(-1f, -10f),
						acceleration = new Vector2(0f, 0.6f),
						scale = 4f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
					tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
					{
						motion = new Vector2(0f, -8f),
						acceleration = new Vector2(0f, 0.48f),
						scale = 4f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
					tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
					{
						motion = new Vector2(1f, -10f),
						acceleration = new Vector2(0f, 0.6f),
						scale = 4f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
					tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
					{
						motion = new Vector2(-2f, -8f),
						acceleration = new Vector2(0f, 0.6f),
						scale = 2f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
					tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
					{
						motion = new Vector2(2f, -8f),
						acceleration = new Vector2(0f, 0.6f),
						scale = 2f,
						layerDepth = 1f,
						animationLength = 12,
						interval = 30f,
						totalNumberOfLoops = 1
					});
				}
			}
			if (_timeUntilCraft > 0 || _sparklingTimer > 0)
			{
				return;
			}
			if (unforging)
			{
				if (leftIngredientSpot.item is MeleeWeapon)
				{
					MeleeWeapon weapon = leftIngredientSpot.item as MeleeWeapon;
					int cost = 0;
					if (weapon != null)
					{
						int weapon_forge_levels = weapon.GetTotalForgeLevels(for_unforge: true);
						for (int j = 0; j < weapon_forge_levels; j++)
						{
							cost += GetForgeCostAtLevel(j);
						}
						if (weapon.hasEnchantmentOfType<DiamondEnchantment>())
						{
							cost += GetForgeCost(leftIngredientSpot.item, new Object(72, 1));
						}
						for (int i = weapon.enchantments.Count - 1; i >= 0; i--)
						{
							if (weapon.enchantments[i].IsForge())
							{
								weapon.RemoveEnchantment(weapon.enchantments[i]);
							}
						}
						if (weapon.appearance.Value >= 0)
						{
							weapon.appearance.Value = -1;
							weapon.IndexOfMenuItemView = weapon.getDrawnItemIndex();
							cost += 10;
						}
						leftIngredientSpot.item = null;
						Game1.playSound("coin");
						heldItem = weapon;
					}
					Utility.CollectOrDrop(new Object(848, cost / 2));
				}
				else if (leftIngredientSpot.item is CombinedRing)
				{
					CombinedRing ring = leftIngredientSpot.item as CombinedRing;
					if (ring != null)
					{
						List<Ring> rings = new List<Ring>(ring.combinedRings);
						ring.combinedRings.Clear();
						foreach (Ring item in rings)
						{
							Utility.CollectOrDrop(item);
						}
						leftIngredientSpot.item = null;
						Game1.playSound("coin");
					}
					Utility.CollectOrDrop(new Object(848, 10));
				}
				unforging = false;
				_timeUntilCraft = 0;
				_ValidateCraft();
				return;
			}
			Game1.player.removeItemsFromInventory(848, GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item));
			Item crafted_item = CraftItem(leftIngredientSpot.item, rightIngredientSpot.item, forReal: true);
			if (crafted_item != null && !Utility.canItemBeAddedToThisInventoryList(crafted_item, inventory.actualInventory))
			{
				Game1.playSound("cancel");
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
				_timeUntilCraft = 0;
				return;
			}
			if (leftIngredientSpot.item == crafted_item)
			{
				leftIngredientSpot.item = null;
			}
			else
			{
				SpendLeftItem();
			}
			SpendRightItem();
			Game1.playSound("coin");
			heldItem = crafted_item;
			_timeUntilCraft = 0;
			_ValidateCraft();
		}

		public virtual bool IsValidUnforge(bool ignore_right_slot_occupancy = false)
		{
			if (!ignore_right_slot_occupancy && rightIngredientSpot.item != null)
			{
				return false;
			}
			if (leftIngredientSpot.item != null && leftIngredientSpot.item is MeleeWeapon && ((leftIngredientSpot.item as MeleeWeapon).GetTotalForgeLevels() > 0 || (leftIngredientSpot.item as MeleeWeapon).appearance.Value >= 0))
			{
				return true;
			}
			if (leftIngredientSpot.item != null && leftIngredientSpot.item is CombinedRing)
			{
				return true;
			}
			return false;
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			Game1.DrawBox(xPositionOnScreen - 64, yPositionOnScreen + 128, 128, 201, new Color(116, 11, 3));
			Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2((float)(xPositionOnScreen - 64) + 9.6f, yPositionOnScreen + 128), 0.87f, 4f, 2, Game1.player);
			base.draw(b, drawUpperPortion: true, drawDescriptionArea: true, 116, 11, 3);
			b.Draw(forgeTextures, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder), new Rectangle(0, 0, 142, 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			Color draw_color = Color.White;
			if (_craftState == CraftState.MissingShards)
			{
				draw_color = Color.Gray * 0.75f;
			}
			b.Draw(forgeTextures, new Vector2(xPositionOnScreen + 276, yPositionOnScreen + 300), new Rectangle(142, 16, 17, 17), draw_color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			if (leftIngredientSpot.item != null && rightIngredientSpot.item != null && IsValidCraft(leftIngredientSpot.item, rightIngredientSpot.item))
			{
				int source_offset = (GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item) - 10) / 5;
				if (source_offset >= 0 && source_offset <= 2)
				{
					b.Draw(forgeTextures, new Vector2(xPositionOnScreen + 344, yPositionOnScreen + 320), new Rectangle(142, 38 + source_offset * 10, 17, 10), Color.White * ((_craftState == CraftState.MissingShards) ? 0.5f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				}
			}
			if (IsValidUnforge())
			{
				b.Draw(forgeTextures, new Vector2(unforgeButton.bounds.X, unforgeButton.bounds.Y), new Rectangle(143, 69, 11, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
			if (_craftState == CraftState.Valid)
			{
				startTailoringButton.draw(b, Color.White, 0.96f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 200 % 12);
				startTailoringButton.drawItem(b, 16, 16);
			}
			Point random_shaking = new Point(0, 0);
			bool left_slot_accepts_this_item = false;
			bool right_slot_accepts_this_item = false;
			Item highlight_item = hoveredItem;
			if (heldItem != null)
			{
				highlight_item = heldItem;
			}
			if (highlight_item != null && highlight_item != leftIngredientSpot.item && highlight_item != rightIngredientSpot.item && highlight_item != craftResultDisplay.item)
			{
				if (highlight_item is Tool)
				{
					if (leftIngredientSpot.item is Tool)
					{
						right_slot_accepts_this_item = true;
					}
					else
					{
						left_slot_accepts_this_item = true;
					}
				}
				if (BaseEnchantment.GetEnchantmentFromItem(leftIngredientSpot.item, highlight_item) != null)
				{
					right_slot_accepts_this_item = true;
				}
				if (highlight_item is Ring && !(highlight_item is CombinedRing) && (leftIngredientSpot.item == null || leftIngredientSpot.item is Ring) && (rightIngredientSpot.item == null || rightIngredientSpot.item is Ring))
				{
					left_slot_accepts_this_item = true;
					right_slot_accepts_this_item = true;
				}
			}
			foreach (ClickableComponent c in equipmentIcons)
			{
				string name = c.name;
				if (!(name == "Ring1"))
				{
					if (name == "Ring2")
					{
						if (Game1.player.rightRing.Value != null)
						{
							b.Draw(forgeTextures, c.bounds, new Rectangle(0, 96, 16, 16), Color.White);
							float transparency2 = 1f;
							if (!HighlightItems((Ring)Game1.player.rightRing))
							{
								transparency2 = 0.5f;
							}
							if (Game1.player.rightRing.Value == heldItem)
							{
								transparency2 = 0.5f;
							}
							Game1.player.rightRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency2, 0.866f, StackDrawType.Hide);
						}
						else
						{
							b.Draw(forgeTextures, c.bounds, new Rectangle(16, 96, 16, 16), Color.White);
						}
					}
				}
				else if (Game1.player.leftRing.Value != null)
				{
					b.Draw(forgeTextures, c.bounds, new Rectangle(0, 96, 16, 16), Color.White);
					float transparency = 1f;
					if (!HighlightItems((Ring)Game1.player.leftRing))
					{
						transparency = 0.5f;
					}
					if (Game1.player.leftRing.Value == heldItem)
					{
						transparency = 0.5f;
					}
					Game1.player.leftRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency, 0.866f, StackDrawType.Hide);
				}
				else
				{
					b.Draw(forgeTextures, c.bounds, new Rectangle(16, 96, 16, 16), Color.White);
				}
			}
			if (!IsBusy())
			{
				if (left_slot_accepts_this_item)
				{
					leftIngredientSpot.draw(b, Color.White, 0.87f);
				}
			}
			else if (_clankEffectTimer > 300 || (_timeUntilCraft > 0 && unforging))
			{
				random_shaking.X = Game1.random.Next(-1, 2);
				random_shaking.Y = Game1.random.Next(-1, 2);
			}
			leftIngredientSpot.drawItem(b, random_shaking.X * 4, random_shaking.Y * 4);
			if (craftResultDisplay.visible)
			{
				string make_result_text = Game1.content.LoadString("Strings\\UI:Tailor_MakeResult");
				Utility.drawTextWithColoredShadow(position: new Vector2((float)craftResultDisplay.bounds.Center.X - Game1.smallFont.MeasureString(make_result_text).X / 2f, (float)craftResultDisplay.bounds.Top - Game1.smallFont.MeasureString(make_result_text).Y), b: b, text: make_result_text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f);
				if (craftResultDisplay.item != null)
				{
					craftResultDisplay.drawItem(b);
				}
			}
			if (!IsBusy() && right_slot_accepts_this_item)
			{
				rightIngredientSpot.draw(b, Color.White, 0.87f);
			}
			rightIngredientSpot.drawItem(b);
			foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
			{
				tempSprite.draw(b, localPosition: true);
			}
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, (heldItem != null) ? 32 : 0, (heldItem != null) ? 32 : 0);
			}
			else if (hoveredItem != null)
			{
				if (hoveredItem == craftResultDisplay.item && Utility.IsNormalObjectAtParentSheetIndex(rightIngredientSpot.item, 74))
				{
					BaseEnchantment.hideEnchantmentName = true;
				}
				IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, heldItem != null);
				BaseEnchantment.hideEnchantmentName = false;
			}
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
			}
			if (!Game1.options.hardwareCursor)
			{
				drawMouse(b);
			}
		}

		protected override void cleanupBeforeExit()
		{
			_OnCloseMenu();
		}

		protected void _OnCloseMenu()
		{
			if (!Game1.player.IsEquippedItem(heldItem))
			{
				Utility.CollectOrDrop(heldItem);
			}
			if (!Game1.player.IsEquippedItem(leftIngredientSpot.item))
			{
				Utility.CollectOrDrop(leftIngredientSpot.item);
			}
			if (!Game1.player.IsEquippedItem(rightIngredientSpot.item))
			{
				Utility.CollectOrDrop(rightIngredientSpot.item);
			}
			if (!Game1.player.IsEquippedItem(startTailoringButton.item))
			{
				Utility.CollectOrDrop(startTailoringButton.item);
			}
			heldItem = null;
			leftIngredientSpot.item = null;
			rightIngredientSpot.item = null;
			startTailoringButton.item = null;
		}
	}
}
