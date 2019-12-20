using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Crafting;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class TailoringMenu : MenuWithInventory
	{
		public enum CraftState
		{
			MissingIngredients,
			Valid,
			InvalidRecipe,
			NotDyeable
		}

		protected int _timeUntilCraft;

		public const int region_leftIngredient = 998;

		public const int region_rightIngredient = 997;

		public const int region_startButton = 996;

		public const int region_resultItem = 995;

		public ClickableTextureComponent needleSprite;

		public ClickableTextureComponent presserSprite;

		public ClickableTextureComponent craftResultDisplay;

		public Vector2 needlePosition;

		public Vector2 presserPosition;

		public Vector2 leftIngredientStartSpot;

		public Vector2 leftIngredientEndSpot;

		protected float _rightItemOffset;

		public ClickableTextureComponent leftIngredientSpot;

		public ClickableTextureComponent rightIngredientSpot;

		public ClickableTextureComponent blankLeftIngredientSpot;

		public ClickableTextureComponent blankRightIngredientSpot;

		public ClickableTextureComponent startTailoringButton;

		public const int region_shirt = 108;

		public const int region_pants = 109;

		public const int region_hat = 101;

		public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

		public const int CRAFT_TIME = 1500;

		public Texture2D tailoringTextures;

		public List<TailorItemRecipe> _tailoringRecipes;

		private ICue _sewingSound;

		protected Dictionary<Item, bool> _highlightDictionary;

		protected Dictionary<string, Item> _lastValidEquippedItems;

		protected bool _shouldPrismaticDye;

		protected bool _heldItemIsEquipped;

		protected bool _isDyeCraft;

		protected bool _isMultipleResultCraft;

		protected string displayedDescription = "";

		protected CraftState _craftState;

		public Vector2 questionMarkOffset;

		public TailoringMenu()
			: base(null, okButton: true, trashCan: true, 12, 132)
		{
			Game1.playSound("bigSelect");
			if (yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				movePosition(0, -IClickableMenu.spaceToClearTopBorder);
			}
			inventory.highlightMethod = HighlightItems;
			tailoringTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\tailoring");
			_tailoringRecipes = Game1.temporaryContent.Load<List<TailorItemRecipe>>("Data\\TailoringRecipes");
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
			leftIngredientSpot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 192, 96, 96), tailoringTextures, new Rectangle(0, 156, 24, 24), 4f)
			{
				myID = 998,
				downNeighborID = -99998,
				leftNeighborID = 109,
				rightNeighborID = 996,
				upNeighborID = 997,
				item = ((leftIngredientSpot != null) ? leftIngredientSpot.item : null)
			};
			leftIngredientStartSpot = new Vector2(leftIngredientSpot.bounds.X, leftIngredientSpot.bounds.Y);
			leftIngredientEndSpot = leftIngredientStartSpot + new Vector2(256f, 0f);
			needleSprite = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 116, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 128, 96, 96), tailoringTextures, new Rectangle(64, 80, 16, 32), 4f);
			presserSprite = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 116, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 128, 96, 96), tailoringTextures, new Rectangle(48, 80, 16, 32), 4f);
			needlePosition = new Vector2(needleSprite.bounds.X, needleSprite.bounds.Y);
			presserPosition = new Vector2(presserSprite.bounds.X, presserSprite.bounds.Y);
			rightIngredientSpot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 400, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8, 96, 96), tailoringTextures, new Rectangle(0, 180, 24, 24), 4f)
			{
				myID = 997,
				downNeighborID = 996,
				leftNeighborID = 998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				item = ((rightIngredientSpot != null) ? rightIngredientSpot.item : null),
				fullyImmutable = true
			};
			blankRightIngredientSpot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 400, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8, 96, 96), tailoringTextures, new Rectangle(0, 128, 24, 24), 4f);
			blankLeftIngredientSpot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 192, 96, 96), tailoringTextures, new Rectangle(0, 128, 24, 24), 4f);
			startTailoringButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 448, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 128, 96, 96), tailoringTextures, new Rectangle(24, 80, 24, 24), 4f)
			{
				myID = 996,
				downNeighborID = -99998,
				leftNeighborID = 998,
				rightNeighborID = 995,
				upNeighborID = 997,
				item = ((startTailoringButton != null) ? startTailoringButton.item : null),
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
			equipmentIcons = new List<ClickableComponent>();
			equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, 64, 64), "Hat")
			{
				myID = 101,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborID = -99998
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, 64, 64), "Shirt")
			{
				myID = 108,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, 64, 64), "Pants")
			{
				myID = 109,
				upNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998
			});
			for (int i = 0; i < equipmentIcons.Count; i++)
			{
				equipmentIcons[i].bounds.X = xPositionOnScreen - 64 + 9;
				equipmentIcons[i].bounds.Y = yPositionOnScreen + 192 + i * 64;
			}
			craftResultDisplay = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 660, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232, 64, 64), tailoringTextures, new Rectangle(0, 208, 16, 16), 4f)
			{
				myID = 995,
				downNeighborID = -99998,
				leftNeighborID = 996,
				upNeighborID = 997,
				item = ((craftResultDisplay != null) ? craftResultDisplay.item : null)
			};
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public bool IsBusy()
		{
			return _timeUntilCraft > 0;
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
			if (Game1.player.pantsItem.Value != null)
			{
				item_list.Add(Game1.player.pantsItem.Value);
			}
			if (Game1.player.shirtItem.Value != null)
			{
				item_list.Add(Game1.player.shirtItem.Value);
			}
			if (Game1.player.hat.Value != null)
			{
				item_list.Add(Game1.player.hat.Value);
			}
			foreach (Item item in item_list)
			{
				if (item != null)
				{
					if (leftIngredientSpot.item == null && rightIngredientSpot.item == null)
					{
						_highlightDictionary[item] = true;
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
			if (heldItem == null || IsValidCraftIngredient(heldItem))
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
			if (item.HasContextTag("item_lucky_purple_shorts"))
			{
				return true;
			}
			if (!item.canBeTrashed())
			{
				return false;
			}
			return true;
		}

		private void _rightIngredientSpotClicked()
		{
			Item old_item = rightIngredientSpot.item;
			if (heldItem == null || IsValidCraftIngredient(heldItem))
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
			Item oldHeldItem = heldItem;
			bool num = Game1.player.IsEquippedItem(oldHeldItem);
			base.receiveLeftClick(x, y, playSound: true);
			if (num && heldItem != oldHeldItem)
			{
				if (oldHeldItem == Game1.player.hat.Value)
				{
					Game1.player.hat.Value = null;
					_highlightDictionary = null;
				}
				else if (oldHeldItem == Game1.player.shirtItem.Value)
				{
					Game1.player.shirtItem.Value = null;
					_highlightDictionary = null;
				}
				else if (oldHeldItem == Game1.player.pantsItem.Value)
				{
					Game1.player.pantsItem.Value = null;
					_highlightDictionary = null;
				}
			}
			foreach (ClickableComponent c in equipmentIcons)
			{
				if (c.containsPoint(x, y))
				{
					string name = c.name;
					if (!(name == "Hat"))
					{
						if (!(name == "Shirt"))
						{
							if (name == "Pants")
							{
								Item item_to_place3 = Utility.PerformSpecialItemPlaceReplacement(heldItem);
								if (heldItem == null)
								{
									if (HighlightItems((Clothing)Game1.player.pantsItem))
									{
										heldItem = Utility.PerformSpecialItemGrabReplacement((Clothing)Game1.player.pantsItem);
										if (!(heldItem is Clothing))
										{
											Game1.player.pantsItem.Value = null;
										}
										Game1.playSound("dwop");
										_highlightDictionary = null;
										_ValidateCraft();
									}
								}
								else if (item_to_place3 is Clothing && (item_to_place3 as Clothing).clothesType.Value == 1)
								{
									Item old_item6 = Game1.player.pantsItem.Value;
									old_item6 = Utility.PerformSpecialItemGrabReplacement(old_item6);
									if (old_item6 == heldItem)
									{
										old_item6 = null;
									}
									Game1.player.pantsItem.Value = (item_to_place3 as Clothing);
									heldItem = old_item6;
									Game1.playSound("sandyStep");
									_highlightDictionary = null;
									_ValidateCraft();
								}
							}
						}
						else
						{
							Item item_to_place2 = Utility.PerformSpecialItemPlaceReplacement(heldItem);
							if (heldItem == null)
							{
								if (HighlightItems((Clothing)Game1.player.shirtItem))
								{
									heldItem = Utility.PerformSpecialItemGrabReplacement((Clothing)Game1.player.shirtItem);
									Game1.playSound("dwop");
									if (!(heldItem is Clothing))
									{
										Game1.player.shirtItem.Value = null;
									}
									_highlightDictionary = null;
									_ValidateCraft();
								}
							}
							else if (heldItem is Clothing && (heldItem as Clothing).clothesType.Value == 0)
							{
								Item old_item4 = (Clothing)Game1.player.shirtItem;
								old_item4 = Utility.PerformSpecialItemGrabReplacement(old_item4);
								if (old_item4 == heldItem)
								{
									old_item4 = null;
								}
								Game1.player.shirtItem.Value = (item_to_place2 as Clothing);
								heldItem = old_item4;
								Game1.playSound("sandyStep");
								_highlightDictionary = null;
								_ValidateCraft();
							}
						}
					}
					else
					{
						Item item_to_place = Utility.PerformSpecialItemPlaceReplacement(heldItem);
						if (heldItem == null)
						{
							if (HighlightItems((Hat)Game1.player.hat))
							{
								heldItem = Utility.PerformSpecialItemGrabReplacement((Hat)Game1.player.hat);
								Game1.playSound("dwop");
								if (!(heldItem is Hat))
								{
									Game1.player.hat.Value = null;
								}
								_highlightDictionary = null;
								_ValidateCraft();
							}
						}
						else if (item_to_place is Hat)
						{
							Item old_item2 = Game1.player.hat.Value;
							old_item2 = Utility.PerformSpecialItemGrabReplacement(old_item2);
							if (old_item2 == heldItem)
							{
								old_item2 = null;
							}
							Game1.player.hat.Value = (item_to_place as Hat);
							heldItem = old_item2;
							Game1.playSound("grassyStep");
							_highlightDictionary = null;
							_ValidateCraft();
						}
					}
					return;
				}
			}
			if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && oldHeldItem != heldItem && heldItem != null)
			{
				if (heldItem.Name == "Cloth" || (heldItem is Clothing && (bool)(heldItem as Clothing).dyeable))
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
				if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && heldItem != null)
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
				if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && heldItem != null)
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
					if (!fail && IsValidCraft(leftIngredientSpot.item, rightIngredientSpot.item))
					{
						Game1.playSound("bigSelect");
						_sewingSound = Game1.soundBank.GetCue("sewing_loop");
						_sewingSound.Play();
						startTailoringButton.scale = startTailoringButton.baseScale;
						_timeUntilCraft = 1500;
						_UpdateDescriptionText();
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

		protected void _ValidateCraft()
		{
			Item left_item = leftIngredientSpot.item;
			Item right_item = rightIngredientSpot.item;
			if (left_item == null || right_item == null)
			{
				_craftState = CraftState.MissingIngredients;
			}
			else if (left_item is Clothing && !(left_item as Clothing).dyeable)
			{
				_craftState = CraftState.NotDyeable;
			}
			else if (IsValidCraft(left_item, right_item))
			{
				_craftState = CraftState.Valid;
				bool should_prismatic_dye = _shouldPrismaticDye;
				Item left_item_clone = left_item.getOne();
				if (IsMultipleResultCraft(left_item, right_item))
				{
					_isMultipleResultCraft = true;
				}
				else
				{
					_isMultipleResultCraft = false;
				}
				craftResultDisplay.item = CraftItem(left_item_clone, right_item.getOne());
				if (craftResultDisplay.item == left_item_clone)
				{
					_isDyeCraft = true;
				}
				else
				{
					_isDyeCraft = false;
				}
				_shouldPrismaticDye = should_prismatic_dye;
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
				displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_Busy");
			}
			else if (_craftState == CraftState.NotDyeable)
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_NotDyeable");
			}
			else if (_craftState == CraftState.MissingIngredients)
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_MissingIngredients");
			}
			else if (_craftState == CraftState.Valid)
			{
				if (!CanFitCraftedItem())
				{
					displayedDescription = Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588");
				}
				else
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_Valid");
				}
			}
			else if (_craftState == CraftState.InvalidRecipe)
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_InvalidRecipe");
			}
			else
			{
				displayedDescription = "";
			}
		}

		public static Color? GetDyeColor(Item dye_object)
		{
			if (dye_object != null)
			{
				if (dye_object.Name == "Prismatic Shard")
				{
					return Color.White;
				}
				if (dye_object is ColoredObject)
				{
					return (dye_object as ColoredObject).color;
				}
				Dictionary<string, Color> color_dictionary = new Dictionary<string, Color>();
				color_dictionary["black"] = new Color(45, 45, 45);
				color_dictionary["gray"] = Color.Gray;
				color_dictionary["white"] = Color.White;
				color_dictionary["pink"] = new Color(255, 163, 186);
				color_dictionary["red"] = new Color(220, 0, 0);
				color_dictionary["orange"] = new Color(255, 128, 0);
				color_dictionary["yellow"] = new Color(255, 230, 0);
				color_dictionary["green"] = new Color(10, 143, 0);
				color_dictionary["blue"] = new Color(46, 85, 183);
				color_dictionary["purple"] = new Color(115, 41, 181);
				color_dictionary["brown"] = new Color(130, 73, 37);
				color_dictionary["light_cyan"] = new Color(180, 255, 255);
				color_dictionary["cyan"] = Color.Cyan;
				color_dictionary["aquamarine"] = Color.Aquamarine;
				color_dictionary["sea_green"] = Color.SeaGreen;
				color_dictionary["lime"] = Color.Lime;
				color_dictionary["yellow_green"] = Color.GreenYellow;
				color_dictionary["pale_violet_red"] = Color.PaleVioletRed;
				color_dictionary["salmon"] = new Color(255, 85, 95);
				color_dictionary["jade"] = new Color(130, 158, 93);
				color_dictionary["sand"] = Color.NavajoWhite;
				color_dictionary["poppyseed"] = new Color(82, 47, 153);
				color_dictionary["dark_red"] = Color.DarkRed;
				color_dictionary["dark_orange"] = Color.DarkOrange;
				color_dictionary["dark_yellow"] = Color.DarkGoldenrod;
				color_dictionary["dark_green"] = Color.DarkGreen;
				color_dictionary["dark_blue"] = Color.DarkBlue;
				color_dictionary["dark_purple"] = Color.DarkViolet;
				color_dictionary["dark_pink"] = Color.DeepPink;
				color_dictionary["dark_cyan"] = Color.DarkCyan;
				color_dictionary["dark_gray"] = Color.DarkGray;
				color_dictionary["dark_brown"] = Color.SaddleBrown;
				color_dictionary["gold"] = Color.Gold;
				color_dictionary["copper"] = new Color(179, 85, 0);
				color_dictionary["iron"] = new Color(197, 213, 224);
				color_dictionary["iridium"] = new Color(105, 15, 255);
				foreach (string key in color_dictionary.Keys)
				{
					if (dye_object.HasContextTag("color_" + key))
					{
						return color_dictionary[key];
					}
				}
			}
			return null;
		}

		public bool DyeItems(Clothing clothing, Item dye_object, float dye_strength_override = -1f)
		{
			if (dye_object.Name == "Prismatic Shard")
			{
				clothing.Dye(Color.White, 1f);
				clothing.isPrismatic.Set(newValue: true);
				return true;
			}
			Color? dye_color = GetDyeColor(dye_object);
			if (dye_color.HasValue)
			{
				float dye_strength = 0.25f;
				if (dye_object.HasContextTag("dye_medium"))
				{
					dye_strength = 0.5f;
				}
				if (dye_object.HasContextTag("dye_strong"))
				{
					dye_strength = 1f;
				}
				if (dye_strength_override >= 0f)
				{
					dye_strength = dye_strength_override;
				}
				clothing.Dye(dye_color.Value, dye_strength);
				if (clothing == Game1.player.shirtItem.Value || clothing == Game1.player.pantsItem.Value)
				{
					Game1.player.FarmerRenderer.MarkSpriteDirty();
				}
				return true;
			}
			return false;
		}

		public TailorItemRecipe GetRecipeForItems(Item left_item, Item right_item)
		{
			foreach (TailorItemRecipe recipe in _tailoringRecipes)
			{
				bool fail = false;
				if (recipe.FirstItemTags != null && recipe.FirstItemTags.Count > 0)
				{
					if (left_item == null)
					{
						continue;
					}
					foreach (string required_tag2 in recipe.FirstItemTags)
					{
						if (!left_item.HasContextTag(required_tag2))
						{
							fail = true;
							break;
						}
					}
				}
				if (!fail)
				{
					if (recipe.SecondItemTags != null && recipe.SecondItemTags.Count > 0)
					{
						if (right_item == null)
						{
							continue;
						}
						foreach (string required_tag in recipe.SecondItemTags)
						{
							if (!right_item.HasContextTag(required_tag))
							{
								fail = true;
								break;
							}
						}
					}
					if (!fail)
					{
						return recipe;
					}
				}
			}
			return null;
		}

		public bool IsValidCraft(Item left_item, Item right_item)
		{
			if (left_item == null || right_item == null)
			{
				return false;
			}
			if (left_item is Boots && right_item is Boots)
			{
				return true;
			}
			if (left_item is Clothing && (left_item as Clothing).dyeable.Value)
			{
				if (right_item.HasContextTag("color_prismatic"))
				{
					return true;
				}
				if (GetDyeColor(right_item).HasValue)
				{
					return true;
				}
			}
			if (GetRecipeForItems(left_item, right_item) != null)
			{
				return true;
			}
			return false;
		}

		public bool IsMultipleResultCraft(Item left_item, Item right_item)
		{
			TailorItemRecipe recipe = GetRecipeForItems(left_item, right_item);
			if (recipe != null && recipe.CraftedItemIDs != null && recipe.CraftedItemIDs.Count > 0)
			{
				return true;
			}
			return false;
		}

		public Item CraftItem(Item left_item, Item right_item)
		{
			if (left_item == null || right_item == null)
			{
				return null;
			}
			if (left_item is Boots && left_item is Boots)
			{
				(left_item as Boots).applyStats(right_item as Boots);
				return left_item;
			}
			if (left_item is Clothing && (left_item as Clothing).dyeable.Value)
			{
				if (right_item.HasContextTag("color_prismatic"))
				{
					_shouldPrismaticDye = true;
					return left_item;
				}
				if (DyeItems(left_item as Clothing, right_item))
				{
					return left_item;
				}
			}
			TailorItemRecipe recipe = GetRecipeForItems(left_item, right_item);
			if (recipe != null)
			{
				int crafted_item_id = recipe.CraftedItemID;
				if (recipe != null && recipe.CraftedItemIDs != null && recipe.CraftedItemIDs.Count > 0)
				{
					crafted_item_id = int.Parse(Utility.GetRandom(recipe.CraftedItemIDs));
				}
				Item crafted_item2 = null;
				crafted_item2 = ((crafted_item_id < 0) ? new Object(-crafted_item_id, 1) : ((crafted_item_id < 2000 || crafted_item_id >= 3000) ? ((Item)new Clothing(crafted_item_id)) : ((Item)new Hat(crafted_item_id - 2000))));
				if (crafted_item2 != null && crafted_item2 is Clothing)
				{
					DyeItems(crafted_item2 as Clothing, right_item, 1f);
				}
				Object crafted_object;
				Object left_object;
				Object right_object;
				if ((crafted_object = (crafted_item2 as Object)) != null && (((left_object = (left_item as Object)) != null && left_object.questItem.Value) || ((right_object = (right_item as Object)) != null && right_object.questItem.Value)))
				{
					crafted_object.questItem.Value = true;
				}
				return crafted_item2;
			}
			return null;
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
				if (leftIngredientSpot.item.Stack <= 0)
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
					if (equipmentIcons[i].name == "Shirt")
					{
						hoveredItem = Game1.player.shirtItem.Value;
					}
					else if (equipmentIcons[i].name == "Hat")
					{
						hoveredItem = Game1.player.hat.Value;
					}
					else if (equipmentIcons[i].name == "Pants")
					{
						hoveredItem = Game1.player.pantsItem.Value;
					}
				}
			}
			if (craftResultDisplay.visible && craftResultDisplay.containsPoint(x, y) && craftResultDisplay.item != null)
			{
				if (_isDyeCraft || Game1.player.HasTailoredThisItem(craftResultDisplay.item))
				{
					hoveredItem = craftResultDisplay.item;
				}
				else
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Tailor_MakeResultUnknown");
				}
			}
			if (leftIngredientSpot.containsPoint(x, y))
			{
				if (leftIngredientSpot.item != null)
				{
					hoveredItem = leftIngredientSpot.item;
				}
				else
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Tailor_Feed");
				}
			}
			if (rightIngredientSpot.containsPoint(x, y) && rightIngredientSpot.item == null)
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Tailor_Spool");
			}
			rightIngredientSpot.tryHover(x, y);
			leftIngredientSpot.tryHover(x, y);
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
			descriptionText = displayedDescription;
			questionMarkOffset.X = (float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.5) * 4f;
			questionMarkOffset.Y = (float)Math.Cos(time.TotalGameTime.TotalSeconds * 5.0) * -4f;
			bool can_fit_crafted_item = CanFitCraftedItem();
			if (_craftState == CraftState.Valid && can_fit_crafted_item)
			{
				startTailoringButton.sourceRect.Y = 104;
			}
			else
			{
				startTailoringButton.sourceRect.Y = 80;
			}
			if ((_craftState == CraftState.Valid && !IsBusy()) & can_fit_crafted_item)
			{
				craftResultDisplay.visible = true;
			}
			else
			{
				craftResultDisplay.visible = false;
			}
			if (_timeUntilCraft > 0)
			{
				startTailoringButton.tryHover(startTailoringButton.bounds.Center.X, startTailoringButton.bounds.Center.Y, 0.33f);
				Vector2 lerped_position = new Vector2(0f, 0f);
				lerped_position.X = Utility.Lerp(leftIngredientEndSpot.X, leftIngredientStartSpot.X, (float)_timeUntilCraft / 1500f);
				lerped_position.Y = Utility.Lerp(leftIngredientEndSpot.Y, leftIngredientStartSpot.Y, (float)_timeUntilCraft / 1500f);
				leftIngredientSpot.bounds.X = (int)lerped_position.X;
				leftIngredientSpot.bounds.Y = (int)lerped_position.Y;
				_timeUntilCraft -= time.ElapsedGameTime.Milliseconds;
				needleSprite.bounds.Location = new Point((int)needlePosition.X, (int)(needlePosition.Y - 2f * ((float)_timeUntilCraft % 25f) / 25f * 4f));
				presserSprite.bounds.Location = new Point((int)presserPosition.X, (int)(presserPosition.Y - 1f * ((float)_timeUntilCraft % 50f) / 50f * 4f));
				_rightItemOffset = (float)Math.Sin(time.TotalGameTime.TotalMilliseconds * 2.0 * Math.PI / 180.0) * 2f;
				if (_timeUntilCraft > 0)
				{
					return;
				}
				TailorItemRecipe recipe = GetRecipeForItems(leftIngredientSpot.item, rightIngredientSpot.item);
				_shouldPrismaticDye = false;
				Item crafted_item = CraftItem(leftIngredientSpot.item, rightIngredientSpot.item);
				if (_sewingSound != null && _sewingSound.IsPlaying)
				{
					_sewingSound.Stop(AudioStopOptions.Immediate);
				}
				if (!Utility.canItemBeAddedToThisInventoryList(crafted_item, inventory.actualInventory))
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
				if ((recipe == null || recipe.SpendRightItem) && (readyToClose() || !_shouldPrismaticDye))
				{
					SpendRightItem();
				}
				if (recipe != null)
				{
					Game1.player.MarkItemAsTailored(crafted_item);
				}
				Game1.playSound("coin");
				heldItem = crafted_item;
				_timeUntilCraft = 0;
				_ValidateCraft();
				if (_shouldPrismaticDye)
				{
					Item old_held_item = heldItem;
					heldItem = null;
					if (readyToClose())
					{
						exitThisMenuNoSound();
						Game1.activeClickableMenu = new CharacterCustomization(crafted_item as Clothing);
						return;
					}
					heldItem = old_held_item;
				}
			}
			_rightItemOffset = 0f;
			leftIngredientSpot.bounds.X = (int)leftIngredientStartSpot.X;
			leftIngredientSpot.bounds.Y = (int)leftIngredientStartSpot.Y;
			needleSprite.bounds.Location = new Point((int)needlePosition.X, (int)needlePosition.Y);
			presserSprite.bounds.Location = new Point((int)presserPosition.X, (int)presserPosition.Y);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			b.Draw(tailoringTextures, new Vector2((float)xPositionOnScreen + 96f, yPositionOnScreen - 64), new Rectangle(101, 80, 41, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.87f);
			b.Draw(tailoringTextures, new Vector2((float)xPositionOnScreen + 352f, yPositionOnScreen - 64), new Rectangle(101, 80, 41, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(tailoringTextures, new Vector2((float)xPositionOnScreen + 608f, yPositionOnScreen - 64), new Rectangle(101, 80, 41, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(tailoringTextures, new Vector2((float)xPositionOnScreen + 256f, yPositionOnScreen), new Rectangle(79, 97, 22, 20), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(tailoringTextures, new Vector2((float)xPositionOnScreen + 512f, yPositionOnScreen), new Rectangle(79, 97, 22, 20), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(tailoringTextures, new Vector2((float)xPositionOnScreen + 32f, yPositionOnScreen + 44), new Rectangle(81, 81, 16, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(tailoringTextures, new Vector2((float)xPositionOnScreen + 768f, yPositionOnScreen + 44), new Rectangle(81, 81, 16, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			Game1.DrawBox(xPositionOnScreen - 64, yPositionOnScreen + 128, 128, 265, new Color(50, 160, 255));
			Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2((float)(xPositionOnScreen - 64) + 9.6f, yPositionOnScreen + 128), 0.87f, 4f, 2, Game1.player);
			base.draw(b, drawUpperPortion: true, drawDescriptionArea: true, 50, 160, 255);
			b.Draw(tailoringTextures, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder), new Rectangle(0, 0, 142, 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			startTailoringButton.draw(b, Color.White, 0.96f);
			startTailoringButton.drawItem(b, 16, 16);
			presserSprite.draw(b, Color.White, 0.99f);
			needleSprite.draw(b, Color.White, 0.97f);
			Point random_shaking = new Point(0, 0);
			if (!IsBusy())
			{
				if (leftIngredientSpot.item != null)
				{
					blankLeftIngredientSpot.draw(b);
				}
				else
				{
					leftIngredientSpot.draw(b, Color.White, 0.87f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000 / 200);
				}
			}
			else
			{
				random_shaking.X = Game1.random.Next(-1, 2);
				random_shaking.Y = Game1.random.Next(-1, 2);
			}
			leftIngredientSpot.drawItem(b, (4 + random_shaking.X) * 4, (4 + random_shaking.Y) * 4);
			if (craftResultDisplay.visible)
			{
				string make_result_text = Game1.content.LoadString("Strings\\UI:Tailor_MakeResult");
				Utility.drawTextWithColoredShadow(position: new Vector2((float)craftResultDisplay.bounds.Center.X - Game1.smallFont.MeasureString(make_result_text).X / 2f, (float)craftResultDisplay.bounds.Top - Game1.smallFont.MeasureString(make_result_text).Y), b: b, text: make_result_text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f);
				craftResultDisplay.draw(b);
				if (craftResultDisplay.item != null)
				{
					if (_isMultipleResultCraft)
					{
						Rectangle question_mark_bounds = craftResultDisplay.bounds;
						question_mark_bounds.X += 6;
						question_mark_bounds.Y -= 8 + (int)questionMarkOffset.Y;
						b.Draw(tailoringTextures, question_mark_bounds, new Rectangle(112, 208, 16, 16), Color.White);
					}
					else if (_isDyeCraft || Game1.player.HasTailoredThisItem(craftResultDisplay.item))
					{
						craftResultDisplay.drawItem(b);
					}
					else
					{
						Object crafted_object;
						if (craftResultDisplay.item is Hat)
						{
							b.Draw(tailoringTextures, craftResultDisplay.bounds, new Rectangle(96, 208, 16, 16), Color.White);
						}
						else if (craftResultDisplay.item is Clothing)
						{
							if ((craftResultDisplay.item as Clothing).clothesType.Value == 1)
							{
								b.Draw(tailoringTextures, craftResultDisplay.bounds, new Rectangle(64, 208, 16, 16), Color.White);
							}
							else if ((craftResultDisplay.item as Clothing).clothesType.Value == 0)
							{
								b.Draw(tailoringTextures, craftResultDisplay.bounds, new Rectangle(80, 208, 16, 16), Color.White);
							}
						}
						else if ((crafted_object = (craftResultDisplay.item as Object)) != null && Utility.IsNormalObjectAtParentSheetIndex(crafted_object, 71))
						{
							b.Draw(tailoringTextures, craftResultDisplay.bounds, new Rectangle(64, 208, 16, 16), Color.White);
						}
						Rectangle question_mark_bounds2 = craftResultDisplay.bounds;
						question_mark_bounds2.X += 24;
						question_mark_bounds2.Y += 12 + (int)questionMarkOffset.Y;
						b.Draw(tailoringTextures, question_mark_bounds2, new Rectangle(112, 208, 16, 16), Color.White);
					}
				}
			}
			foreach (ClickableComponent c in equipmentIcons)
			{
				switch (c.name)
				{
				case "Hat":
					if (Game1.player.hat.Value != null)
					{
						b.Draw(tailoringTextures, c.bounds, new Rectangle(0, 208, 16, 16), Color.White);
						float transparency = 1f;
						if (!HighlightItems((Hat)Game1.player.hat))
						{
							transparency = 0.5f;
						}
						if (Game1.player.hat.Value == heldItem)
						{
							transparency = 0.5f;
						}
						Game1.player.hat.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency, 0.866f, StackDrawType.Hide);
					}
					else
					{
						b.Draw(tailoringTextures, c.bounds, new Rectangle(48, 208, 16, 16), Color.White);
					}
					break;
				case "Shirt":
					if (Game1.player.shirtItem.Value != null)
					{
						b.Draw(tailoringTextures, c.bounds, new Rectangle(0, 208, 16, 16), Color.White);
						float transparency2 = 1f;
						if (!HighlightItems((Clothing)Game1.player.shirtItem))
						{
							transparency2 = 0.5f;
						}
						if (Game1.player.shirtItem.Value == heldItem)
						{
							transparency2 = 0.5f;
						}
						Game1.player.shirtItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency2, 0.866f);
					}
					else
					{
						b.Draw(tailoringTextures, c.bounds, new Rectangle(32, 208, 16, 16), Color.White);
					}
					break;
				case "Pants":
					if (Game1.player.pantsItem.Value != null)
					{
						b.Draw(tailoringTextures, c.bounds, new Rectangle(0, 208, 16, 16), Color.White);
						float transparency3 = 1f;
						if (!HighlightItems((Clothing)Game1.player.pantsItem))
						{
							transparency3 = 0.5f;
						}
						if (Game1.player.pantsItem.Value == heldItem)
						{
							transparency3 = 0.5f;
						}
						Game1.player.pantsItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency3, 0.866f);
					}
					else
					{
						b.Draw(tailoringTextures, c.bounds, new Rectangle(16, 208, 16, 16), Color.White);
					}
					break;
				}
			}
			if (!IsBusy())
			{
				if (rightIngredientSpot.item != null)
				{
					blankRightIngredientSpot.draw(b);
				}
				else
				{
					rightIngredientSpot.draw(b, Color.White, 0.87f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000 / 200);
				}
			}
			rightIngredientSpot.drawItem(b, 16, (4 + (int)_rightItemOffset) * 4);
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, (heldItem != null) ? 32 : 0, (heldItem != null) ? 32 : 0);
			}
			else if (hoveredItem != null)
			{
				IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, heldItem != null);
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
