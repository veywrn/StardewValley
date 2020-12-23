using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Menus
{
	public class MenuWithInventory : IClickableMenu
	{
		public const int region_okButton = 4857;

		public const int region_trashCan = 5948;

		public string descriptionText = "";

		public string hoverText = "";

		public string descriptionTitle = "";

		public InventoryMenu inventory;

		public Item heldItem;

		public Item hoveredItem;

		public int wiggleWordsTimer;

		public int hoverAmount;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent trashCan;

		public float trashCanLidRotation;

		public ClickableComponent dropItemInvisibleButton;

		public MenuWithInventory(InventoryMenu.highlightThisItem highlighterMethod = null, bool okButton = false, bool trashCan = false, int inventoryXOffset = 0, int inventoryYOffset = 0, int menuOffsetHack = 0)
			: base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 + menuOffsetHack, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2)
		{
			if (yPositionOnScreen < IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				yPositionOnScreen = IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
			}
			if (xPositionOnScreen < 0)
			{
				xPositionOnScreen = 0;
			}
			int yPositionForInventory = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + inventoryYOffset;
			inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + inventoryXOffset, yPositionForInventory, playerInventory: false, null, highlighterMethod);
			if (okButton)
			{
				this.okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
				{
					myID = 4857,
					upNeighborID = 5948,
					leftNeighborID = 12
				};
			}
			if (trashCan)
			{
				this.trashCan = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f)
				{
					myID = 5948,
					downNeighborID = 4857,
					leftNeighborID = 12,
					upNeighborID = 106
				};
			}
			dropItemInvisibleButton = new ClickableComponent(new Rectangle(xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, yPositionForInventory - 12, 64, 64), "")
			{
				myID = 107,
				rightNeighborID = 0
			};
		}

		public void movePosition(int dx, int dy)
		{
			xPositionOnScreen += dx;
			yPositionOnScreen += dy;
			inventory.movePosition(dx, dy);
			if (okButton != null)
			{
				okButton.bounds.X += dx;
				okButton.bounds.Y += dy;
			}
			if (trashCan != null)
			{
				trashCan.bounds.X += dx;
				trashCan.bounds.Y += dy;
			}
			if (dropItemInvisibleButton != null)
			{
				dropItemInvisibleButton.bounds.X += dx;
				dropItemInvisibleButton.bounds.Y += dy;
			}
		}

		public override bool readyToClose()
		{
			return heldItem == null;
		}

		public override bool isWithinBounds(int x, int y)
		{
			return base.isWithinBounds(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			heldItem = inventory.leftClick(x, y, heldItem, playSound);
			if (!isWithinBounds(x, y) && readyToClose() && trashCan != null)
			{
				trashCan.containsPoint(x, y);
			}
			if (okButton != null && okButton.containsPoint(x, y) && readyToClose())
			{
				exitThisMenu();
				if (Game1.currentLocation.currentEvent != null && Game1.currentLocation.currentEvent.CurrentCommand > 0)
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
				Game1.playSound("bigDeSelect");
			}
			if (trashCan != null && trashCan.containsPoint(x, y) && heldItem != null && heldItem.canBeTrashed())
			{
				Utility.trashItem(heldItem);
				heldItem = null;
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			heldItem = inventory.rightClick(x, y, heldItem, playSound);
		}

		public void receiveRightClickOnlyToolAttachments(int x, int y)
		{
			heldItem = inventory.rightClick(x, y, heldItem, playSound: true, onlyCheckToolAttachments: true);
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			descriptionTitle = "";
			hoveredItem = inventory.hover(x, y, heldItem);
			hoverText = inventory.hoverText;
			hoverAmount = 0;
			if (okButton != null)
			{
				if (okButton.containsPoint(x, y))
				{
					okButton.scale = Math.Min(1.1f, okButton.scale + 0.05f);
				}
				else
				{
					okButton.scale = Math.Max(1f, okButton.scale - 0.05f);
				}
			}
			if (trashCan == null)
			{
				return;
			}
			if (trashCan.containsPoint(x, y))
			{
				if (trashCanLidRotation <= 0f)
				{
					Game1.playSound("trashcanlid");
				}
				trashCanLidRotation = Math.Min(trashCanLidRotation + (float)Math.PI / 48f, (float)Math.PI / 2f);
				if (heldItem != null && Utility.getTrashReclamationPrice(heldItem, Game1.player) > 0)
				{
					hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
					hoverAmount = Utility.getTrashReclamationPrice(heldItem, Game1.player);
				}
			}
			else
			{
				trashCanLidRotation = Math.Max(trashCanLidRotation - (float)Math.PI / 48f, 0f);
			}
		}

		public override void update(GameTime time)
		{
			if (wiggleWordsTimer > 0)
			{
				wiggleWordsTimer -= time.ElapsedGameTime.Milliseconds;
			}
		}

		public virtual void draw(SpriteBatch b, bool drawUpperPortion = true, bool drawDescriptionArea = true, int red = -1, int green = -1, int blue = -1)
		{
			if (trashCan != null)
			{
				trashCan.draw(b);
				b.Draw(Game1.mouseCursors, new Vector2(trashCan.bounds.X + 60, trashCan.bounds.Y + 40), new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10), Color.White, trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
			}
			if (drawUpperPortion)
			{
				Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: false, red, green, blue);
				drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256, small: false, red, green, blue);
				if (drawDescriptionArea)
				{
					drawVerticalUpperIntersectingPartition(b, xPositionOnScreen + 576, 328, red, green, blue);
					if (!descriptionText.Equals(""))
					{
						int xPosition = xPositionOnScreen + 576 + 42 + ((wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
						int yPosition = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 32 + ((wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
						int max_height = 320;
						float scale = 0f;
						string parsed_text2 = "";
						do
						{
							scale = ((scale != 0f) ? (scale - 0.1f) : 1f);
							parsed_text2 = Game1.parseText(descriptionText, Game1.smallFont, (int)(224f / scale));
						}
						while (Game1.smallFont.MeasureString(parsed_text2).Y > (float)max_height / scale && scale > 0.5f);
						if (red == -1)
						{
							Utility.drawTextWithShadow(b, parsed_text2, Game1.smallFont, new Vector2(xPosition, yPosition), Game1.textColor * 0.75f, scale);
						}
						else
						{
							Utility.drawTextWithColoredShadow(b, parsed_text2, Game1.smallFont, new Vector2(xPosition, yPosition), Game1.textColor * 0.75f, Color.Black * 0.2f, scale);
						}
					}
				}
			}
			else
			{
				Game1.drawDialogueBox(xPositionOnScreen - IClickableMenu.borderWidth / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 64, width, height - (IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192), speaker: false, drawOnlyBox: true);
			}
			if (okButton != null)
			{
				okButton.draw(b);
			}
			inventory.draw(b, red, green, blue);
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			if (yPositionOnScreen < IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				yPositionOnScreen = IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
			}
			if (xPositionOnScreen < 0)
			{
				xPositionOnScreen = 0;
			}
			int yPositionForInventory = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16;
			string move_item_sound = inventory.moveItemSound;
			inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2, yPositionForInventory, playerInventory: false, null, inventory.highlightMethod);
			inventory.moveItemSound = move_item_sound;
			if (okButton != null)
			{
				okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
			}
			if (trashCan != null)
			{
				trashCan = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(669, 261, 16, 26), 4f);
			}
		}

		public override void draw(SpriteBatch b)
		{
			throw new NotImplementedException();
		}
	}
}
