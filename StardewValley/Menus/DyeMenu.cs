using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class DyeMenu : MenuWithInventory
	{
		protected int _timeUntilCraft;

		public List<ClickableTextureComponent> dyePots;

		public ClickableTextureComponent dyeButton;

		public const int DYE_POT_ID_OFFSET = 5000;

		public Texture2D dyeTexture;

		protected Dictionary<Item, int> _highlightDictionary;

		protected Dictionary<string, Item> _lastValidEquippedItems;

		protected bool _shouldPrismaticDye;

		protected List<Vector2> _slotDrawPositions;

		protected int _hoveredPotIndex = -1;

		protected int[] _dyeDropAnimationFrames;

		public const int MILLISECONDS_PER_DROP_FRAME = 50;

		public const int TOTAL_DROP_FRAMES = 10;

		public string[][] validPotColors = new string[6][]
		{
			new string[4]
			{
				"color_red",
				"color_salmon",
				"color_dark_red",
				"color_pink"
			},
			new string[5]
			{
				"color_orange",
				"color_dark_orange",
				"color_dark_brown",
				"color_brown",
				"color_copper"
			},
			new string[4]
			{
				"color_yellow",
				"color_dark_yellow",
				"color_gold",
				"color_sand"
			},
			new string[5]
			{
				"color_green",
				"color_dark_green",
				"color_lime",
				"color_yellow_green",
				"color_jade"
			},
			new string[6]
			{
				"color_blue",
				"color_dark_blue",
				"color_dark_cyan",
				"color_light_cyan",
				"color_cyan",
				"color_aquamarine"
			},
			new string[6]
			{
				"color_purple",
				"color_dark_purple",
				"color_dark_pink",
				"color_pale_violet_red",
				"color_poppyseed",
				"color_iridium"
			}
		};

		protected bool _heldItemIsEquipped;

		protected string displayedDescription = "";

		public List<ClickableTextureComponent> dyedClothesDisplays;

		protected Vector2 _dyedClothesDisplayPosition;

		public DyeMenu()
			: base(null, okButton: true, trashCan: true, 12, 132)
		{
			if (yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				movePosition(0, -IClickableMenu.spaceToClearTopBorder);
			}
			Game1.playSound("bigSelect");
			inventory.highlightMethod = HighlightItems;
			dyeTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\dye_bench");
			dyedClothesDisplays = new List<ClickableTextureComponent>();
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
			GenerateHighlightDictionary();
			_UpdateDescriptionText();
		}

		protected void _CreateButtons()
		{
			_slotDrawPositions = inventory.GetSlotDrawPositions();
			Dictionary<int, Item> old_items = new Dictionary<int, Item>();
			if (dyePots != null)
			{
				for (int l = 0; l < dyePots.Count; l++)
				{
					old_items[l] = dyePots[l].item;
				}
			}
			dyePots = new List<ClickableTextureComponent>();
			for (int k = 0; k < validPotColors.Length; k++)
			{
				ClickableTextureComponent dye_pot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4 + 68 + 18 * k * 4, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 132, 64, 64), dyeTexture, new Rectangle(32 + 16 * k, 80, 16, 16), 4f)
				{
					myID = k + 5000,
					downNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					upNeighborID = -99998,
					item = (old_items.ContainsKey(k) ? old_items[k] : null)
				};
				dyePots.Add(dye_pot);
			}
			_dyeDropAnimationFrames = new int[dyePots.Count];
			for (int j = 0; j < _dyeDropAnimationFrames.Length; j++)
			{
				_dyeDropAnimationFrames[j] = -1;
			}
			dyeButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 448, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 200, 96, 96), dyeTexture, new Rectangle(0, 80, 24, 24), 4f)
			{
				myID = 1000,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				item = ((dyeButton != null) ? dyeButton.item : null)
			};
			if (inventory.inventory != null && inventory.inventory.Count >= 12)
			{
				for (int i = 0; i < 12; i++)
				{
					if (inventory.inventory[i] != null)
					{
						inventory.inventory[i].upNeighborID = -99998;
					}
				}
			}
			dyedClothesDisplays.Clear();
			_dyedClothesDisplayPosition = new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 692, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232);
			Vector2 dyed_items_position = _dyedClothesDisplayPosition;
			int drawn_items_count = 0;
			if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
			{
				drawn_items_count++;
			}
			if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
			{
				drawn_items_count++;
			}
			dyed_items_position.X -= drawn_items_count * 64 / 2;
			if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
			{
				ClickableTextureComponent component2 = new ClickableTextureComponent(new Rectangle((int)dyed_items_position.X, (int)dyed_items_position.Y, 64, 64), null, new Rectangle(0, 0, 64, 64), 4f);
				component2.item = Game1.player.shirtItem.Value;
				dyed_items_position.X += 64f;
				dyedClothesDisplays.Add(component2);
			}
			if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
			{
				ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle((int)dyed_items_position.X, (int)dyed_items_position.Y, 64, 64), null, new Rectangle(0, 0, 64, 64), 4f);
				component.item = Game1.player.pantsItem.Value;
				dyed_items_position.X += 64f;
				dyedClothesDisplays.Add(component);
			}
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
			if (i != null && !i.canBeTrashed())
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
			if (_hoveredPotIndex >= 0)
			{
				return _hoveredPotIndex == _highlightDictionary[i];
			}
			if (_highlightDictionary[i] >= 0)
			{
				return dyePots[_highlightDictionary[i]].item == null;
			}
			return false;
		}

		public void GenerateHighlightDictionary()
		{
			_highlightDictionary = new Dictionary<Item, int>();
			foreach (Item item in new List<Item>(inventory.actualInventory))
			{
				if (item != null)
				{
					_highlightDictionary[item] = GetPotIndex(item);
				}
			}
		}

		private void _DyePotClicked(ClickableTextureComponent dye_pot)
		{
			Item old_item = dye_pot.item;
			int index = dyePots.IndexOf(dye_pot);
			if (index < 0)
			{
				return;
			}
			if (heldItem == null || (heldItem.canBeTrashed() && GetPotIndex(heldItem) == index))
			{
				bool force_remove = false;
				if (dye_pot.item != null && heldItem != null && dye_pot.item.canStackWith(heldItem))
				{
					heldItem.Stack++;
					dye_pot.item = null;
					Game1.playSound("quickSlosh");
					return;
				}
				dye_pot.item = ((heldItem == null) ? null : heldItem.getOne());
				if (heldItem != null)
				{
					int old_stack = heldItem.Stack;
					heldItem.Stack--;
					if (old_stack == heldItem.Stack && old_stack == 1)
					{
						force_remove = true;
					}
				}
				if (heldItem != null && ((heldItem.Stack <= 0) | force_remove))
				{
					heldItem = old_item;
				}
				else if (heldItem != null && old_item != null)
				{
					Item j = Game1.player.addItemToInventory(heldItem);
					if (j != null)
					{
						Game1.createItemDebris(j, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
					}
					heldItem = old_item;
				}
				else if (old_item != null)
				{
					heldItem = old_item;
				}
				else if (heldItem != null && old_item == null && Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
				{
					Game1.player.addItemToInventory(heldItem);
					heldItem = null;
				}
				if (old_item != dye_pot.item)
				{
					_dyeDropAnimationFrames[index] = 0;
					Game1.playSound("quickSlosh");
					int count = 0;
					for (int i = 0; i < dyePots.Count; i++)
					{
						if (dyePots[i].item != null)
						{
							count++;
						}
					}
					if (count >= dyePots.Count)
					{
						DelayedAction.playSoundAfterDelay("newArtifact", 200);
					}
				}
				_highlightDictionary = null;
				GenerateHighlightDictionary();
			}
			_UpdateDescriptionText();
		}

		public Color GetColorForPot(int index)
		{
			switch (index)
			{
			case 0:
				return new Color(220, 0, 0);
			case 1:
				return new Color(255, 128, 0);
			case 2:
				return new Color(255, 230, 0);
			case 3:
				return new Color(10, 143, 0);
			case 4:
				return new Color(46, 105, 203);
			case 5:
				return new Color(115, 41, 181);
			default:
				return Color.Black;
			}
		}

		public int GetPotIndex(Item item)
		{
			for (int j = 0; j < validPotColors.Length; j++)
			{
				for (int i = 0; i < validPotColors[j].Length; i++)
				{
					if (item.HasContextTag(validPotColors[j][i]))
					{
						return j;
					}
				}
			}
			return -1;
		}

		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Delete)
			{
				if (heldItem != null && heldItem.canBeTrashed())
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

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item oldHeldItem = heldItem;
			Game1.player.IsEquippedItem(oldHeldItem);
			base.receiveLeftClick(x, y, heldItem != null || !Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift));
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && oldHeldItem != heldItem && heldItem != null)
			{
				foreach (ClickableTextureComponent pot2 in dyePots)
				{
					if (pot2.item == null)
					{
						_DyePotClicked(pot2);
					}
					if (heldItem == null)
					{
						return;
					}
				}
			}
			if (IsBusy())
			{
				return;
			}
			bool wasHeldItem = heldItem != null;
			foreach (ClickableTextureComponent pot in dyePots)
			{
				if (pot.containsPoint(x, y))
				{
					_DyePotClicked(pot);
					if (!wasHeldItem && heldItem != null && Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
					{
						heldItem = Game1.player.addItemToInventory(heldItem);
					}
					return;
				}
			}
			if (dyeButton.containsPoint(x, y))
			{
				if (heldItem == null && CanDye())
				{
					Game1.playSound("glug");
					for (int i = 0; i < dyePots.Count; i++)
					{
						if (dyePots[i].item != null)
						{
							dyePots[i].item.Stack--;
							if (dyePots[i].item.Stack <= 0)
							{
								dyePots[i].item = null;
							}
						}
					}
					Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.DyePots);
					_UpdateDescriptionText();
				}
				else
				{
					Game1.playSound("sell");
				}
			}
			if (heldItem != null && !isWithinBounds(x, y) && heldItem.canBeTrashed())
			{
				Game1.playSound("throwDownITem");
				Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				heldItem = null;
			}
		}

		public bool CanDye()
		{
			for (int i = 0; i < dyePots.Count; i++)
			{
				if (dyePots[i].item == null)
				{
					return false;
				}
			}
			return true;
		}

		protected virtual bool CheckHeldItem(Func<Item, bool> f = null)
		{
			return f?.Invoke(heldItem) ?? (heldItem != null);
		}

		public static bool IsWearingDyeable()
		{
			if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
			{
				return true;
			}
			if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
			{
				return true;
			}
			return false;
		}

		protected void _UpdateDescriptionText()
		{
			if (!IsWearingDyeable())
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_NoDyeable");
			}
			else if (CanDye())
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_CanDye");
			}
			else
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_Help");
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
			if (x <= dyePots[0].bounds.X || x >= dyePots.Last().bounds.Right || y <= dyePots[0].bounds.Y || y >= dyePots[0].bounds.Bottom)
			{
				_hoveredPotIndex = -1;
			}
			if (IsBusy())
			{
				return;
			}
			hoveredItem = null;
			base.performHoverAction(x, y);
			hoverText = "";
			foreach (ClickableTextureComponent component in dyedClothesDisplays)
			{
				if (component.containsPoint(x, y))
				{
					hoveredItem = component.item;
				}
			}
			for (int i = 0; i < dyePots.Count; i++)
			{
				if (dyePots[i].containsPoint(x, y))
				{
					dyePots[i].tryHover(x, y, 0f);
					_hoveredPotIndex = i;
				}
			}
			if (CanDye())
			{
				dyeButton.tryHover(x, y, 0.2f);
			}
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
			if (CanDye())
			{
				dyeButton.sourceRect.Y = 180;
				dyeButton.sourceRect.X = (int)(time.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 24;
			}
			else
			{
				dyeButton.sourceRect.Y = 80;
				dyeButton.sourceRect.X = 0;
			}
			for (int i = 0; i < dyePots.Count; i++)
			{
				if (_dyeDropAnimationFrames[i] >= 0)
				{
					_dyeDropAnimationFrames[i] += time.ElapsedGameTime.Milliseconds;
					if (_dyeDropAnimationFrames[i] >= 500)
					{
						_dyeDropAnimationFrames[i] = -1;
					}
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			base.draw(b, drawUpperPortion: true, drawDescriptionArea: true, 50, 160, 255);
			b.Draw(dyeTexture, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder), new Rectangle(0, 0, 142, 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			for (int j = 0; j < _slotDrawPositions.Count; j++)
			{
				if (j >= inventory.actualInventory.Count || inventory.actualInventory[j] == null || !_highlightDictionary.ContainsKey(inventory.actualInventory[j]))
				{
					continue;
				}
				int index = _highlightDictionary[inventory.actualInventory[j]];
				if (index >= 0)
				{
					Color color = GetColorForPot(index);
					if (_hoveredPotIndex == -1 && HighlightItems(inventory.actualInventory[j]))
					{
						b.Draw(dyeTexture, _slotDrawPositions[j], new Rectangle(32, 96, 32, 32), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					}
				}
			}
			dyeButton.draw(b, Color.White * (CanDye() ? 1f : 0.55f), 0.96f);
			dyeButton.drawItem(b, 16, 16);
			string make_result_text = Game1.content.LoadString("Strings\\UI:DyePot_WillDye");
			Vector2 dyed_items_position = _dyedClothesDisplayPosition;
			Utility.drawTextWithColoredShadow(position: new Vector2(dyed_items_position.X - Game1.smallFont.MeasureString(make_result_text).X / 2f, (float)(int)dyed_items_position.Y - Game1.smallFont.MeasureString(make_result_text).Y), b: b, text: make_result_text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f);
			foreach (ClickableTextureComponent dyedClothesDisplay in dyedClothesDisplays)
			{
				dyedClothesDisplay.drawItem(b);
			}
			for (int i = 0; i < dyePots.Count; i++)
			{
				dyePots[i].drawItem(b, 0, -16);
				if (_dyeDropAnimationFrames[i] >= 0)
				{
					Color color2 = GetColorForPot(i);
					b.Draw(dyeTexture, new Vector2(dyePots[i].bounds.X, dyePots[i].bounds.Y - 12), new Rectangle(_dyeDropAnimationFrames[i] / 50 * 16, 128, 16, 16), color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
				}
				dyePots[i].draw(b);
			}
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
			Utility.CollectOrDrop(heldItem);
			for (int i = 0; i < dyePots.Count; i++)
			{
				if (dyePots[i].item != null)
				{
					Utility.CollectOrDrop(dyePots[i].item);
				}
			}
			heldItem = null;
			dyeButton.item = null;
		}
	}
}
