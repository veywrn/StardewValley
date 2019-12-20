using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class ItemListMenu : IClickableMenu
	{
		public const int region_okbutton = 101;

		public const int region_forwardButton = 102;

		public const int region_backButton = 103;

		public int itemsPerCategoryPage = 8;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent backButton;

		private List<Item> itemsToList;

		private string title;

		private int currentTab;

		private int totalValueOfItems;

		public ItemListMenu(string menuTitle, List<Item> itemList)
		{
			title = menuTitle;
			itemsToList = itemList;
			foreach (Item i in itemList)
			{
				totalValueOfItems += Utility.getSellToStorePriceOfItem(i);
			}
			itemsToList.Add(null);
			_ = 40;
			int itemSlotWidth = 96;
			int centerX = Game1.viewport.Width / 2;
			int centerY = Game1.viewport.Height / 2;
			width = Math.Min(800, Game1.viewport.Width - 128);
			height = Math.Min(720, Game1.viewport.Height - 128);
			if (height <= 720)
			{
				itemsPerCategoryPage = 7;
			}
			xPositionOnScreen = centerX - width / 2;
			yPositionOnScreen = centerY - height / 2;
			Rectangle okRect = new Rectangle(centerX + width / 2 + 4, centerY + height / 2 - 96, 64, 64);
			okButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), okRect, null, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f)
			{
				myID = 101,
				leftNeighborID = -7777
			};
			if (Game1.options.gamepadControls)
			{
				Mouse.SetPosition(okRect.Center.X, okRect.Center.Y);
				Game1.InvalidateOldMouseMovement();
				Game1.lastCursorMotionWasMouse = false;
			}
			backButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen - 64, yPositionOnScreen + height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 103,
				rightNeighborID = -7777
			};
			forwardButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				leftNeighborID = 103,
				rightNeighborID = 101
			};
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(101);
			snapCursorToCurrentSnappedComponent();
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID == 103 && direction == 1)
			{
				if (showForwardButton())
				{
					currentlySnappedComponent = getComponentWithID(102);
					snapCursorToCurrentSnappedComponent();
				}
				else
				{
					snapToDefaultClickableComponent();
				}
			}
			else if (oldID == 101 && direction == 3)
			{
				if (showForwardButton())
				{
					currentlySnappedComponent = getComponentWithID(102);
					snapCursorToCurrentSnappedComponent();
				}
				else if (showBackButton())
				{
					currentlySnappedComponent = getComponentWithID(103);
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			switch (b)
			{
			case Buttons.LeftTrigger:
				if (showBackButton())
				{
					currentTab--;
					Game1.playSound("shwip");
				}
				break;
			case Buttons.RightTrigger:
				if (showForwardButton())
				{
					currentTab++;
					Game1.playSound("shwip");
				}
				break;
			case Buttons.B:
				okClicked();
				break;
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			okButton.tryHover(x, y);
			backButton.tryHover(x, y);
			forwardButton.tryHover(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (okButton.containsPoint(x, y))
			{
				okClicked();
			}
			if (backButton.containsPoint(x, y))
			{
				if (currentTab != 0)
				{
					currentTab--;
				}
				Game1.playSound("shwip");
			}
			else if (showForwardButton() && forwardButton.containsPoint(x, y))
			{
				currentTab++;
				Game1.playSound("shwip");
			}
		}

		private void okClicked()
		{
			Game1.activeClickableMenu = null;
			if (Game1.CurrentEvent != null)
			{
				Game1.CurrentEvent.CurrentCommand++;
			}
			Game1.playSound("bigDeSelect");
		}

		public override void draw(SpriteBatch b)
		{
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
			SpriteText.drawStringHorizontallyCenteredAt(b, title, xPositionOnScreen + width / 2, yPositionOnScreen + 32 + 12);
			Vector2 position = new Vector2(xPositionOnScreen + 32, yPositionOnScreen + 96 + 4);
			for (int i = currentTab * itemsPerCategoryPage; i < currentTab * itemsPerCategoryPage + itemsPerCategoryPage; i++)
			{
				if (itemsToList.Count > i)
				{
					if (itemsToList[i] == null)
					{
						SpriteText.drawString(b, Game1.content.LoadString("Strings\\UI:ItemList_ItemsLostValue", totalValueOfItems), (int)position.X + 64 + 12, (int)position.Y + 12);
						continue;
					}
					itemsToList[i].drawInMenu(b, position, 1f, 1f, 1f, StackDrawType.Draw_OneInclusive);
					SpriteText.drawString(b, itemsToList[i].DisplayName, (int)position.X + 64 + 12, (int)position.Y + 12);
					position.Y += 68f;
				}
			}
			if (showBackButton())
			{
				backButton.draw(b);
			}
			if (showForwardButton())
			{
				forwardButton.draw(b);
			}
			okButton.draw(b);
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
		}

		public bool showBackButton()
		{
			return currentTab > 0;
		}

		public bool showForwardButton()
		{
			return itemsToList.Count > itemsPerCategoryPage * (currentTab + 1);
		}
	}
}
