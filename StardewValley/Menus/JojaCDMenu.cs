using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class JojaCDMenu : IClickableMenu
	{
		public new const int width = 1280;

		public new const int height = 576;

		public const int buttonWidth = 147;

		public const int buttonHeight = 30;

		private Texture2D noteTexture;

		public List<ClickableComponent> checkboxes = new List<ClickableComponent>();

		private string hoverText;

		private bool boughtSomething;

		private int exitTimer = -1;

		public JojaCDMenu(Texture2D noteTexture)
			: base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 288, 1280, 576, showUpperRightCloseButton: true)
		{
			Game1.player.forceCanMove();
			this.noteTexture = noteTexture;
			int x = xPositionOnScreen + 4;
			int y = yPositionOnScreen + 208;
			for (int i = 0; i < 5; i++)
			{
				checkboxes.Add(new ClickableComponent(new Rectangle(x, y, 588, 120), string.Concat(i))
				{
					myID = i,
					rightNeighborID = ((i % 2 != 0 || i == 4) ? (-1) : (i + 1)),
					leftNeighborID = ((i % 2 == 0) ? (-1) : (i - 1)),
					downNeighborID = i + 2,
					upNeighborID = i - 2
				});
				x += 592;
				if (x > xPositionOnScreen + 1184)
				{
					x = xPositionOnScreen + 4;
					y += 120;
				}
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccVault"))
			{
				checkboxes[0].name = "complete";
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccBoilerRoom"))
			{
				checkboxes[1].name = "complete";
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccCraftsRoom"))
			{
				checkboxes[2].name = "complete";
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccPantry"))
			{
				checkboxes[3].name = "complete";
			}
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccFishTank"))
			{
				checkboxes[4].name = "complete";
			}
			exitFunction = onExitFunction;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
				Game1.mouseCursorTransparency = 1f;
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		private void onExitFunction()
		{
			if (boughtSomething)
			{
				JojaMart.Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_JojaCDConfirm"));
				Game1.drawDialogue(JojaMart.Morris);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (exitTimer < 0)
			{
				base.receiveLeftClick(x, y);
				foreach (ClickableComponent b in checkboxes)
				{
					if (b.containsPoint(x, y) && !b.name.Equals("complete"))
					{
						int buttonNumber = Convert.ToInt32(b.name);
						int price = getPriceFromButtonNumber(buttonNumber);
						if (Game1.player.Money >= price)
						{
							Game1.player.Money -= price;
							Game1.playSound("reward");
							b.name = "complete";
							boughtSomething = true;
							switch (buttonNumber)
							{
							case 0:
								Game1.addMailForTomorrow("jojaVault", noLetter: true, sendToEveryone: true);
								Game1.addMailForTomorrow("ccVault", noLetter: true, sendToEveryone: true);
								break;
							case 1:
								Game1.addMailForTomorrow("jojaBoilerRoom", noLetter: true, sendToEveryone: true);
								Game1.addMailForTomorrow("ccBoilerRoom", noLetter: true, sendToEveryone: true);
								break;
							case 2:
								Game1.addMailForTomorrow("jojaCraftsRoom", noLetter: true, sendToEveryone: true);
								Game1.addMailForTomorrow("ccCraftsRoom", noLetter: true, sendToEveryone: true);
								break;
							case 3:
								Game1.addMailForTomorrow("jojaPantry", noLetter: true, sendToEveryone: true);
								Game1.addMailForTomorrow("ccPantry", noLetter: true, sendToEveryone: true);
								break;
							case 4:
								Game1.addMailForTomorrow("jojaFishTank", noLetter: true, sendToEveryone: true);
								Game1.addMailForTomorrow("ccFishTank", noLetter: true, sendToEveryone: true);
								break;
							}
							exitTimer = 1000;
						}
						else
						{
							Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
						}
					}
				}
			}
		}

		public override bool readyToClose()
		{
			return true;
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (exitTimer >= 0)
			{
				exitTimer -= time.ElapsedGameTime.Milliseconds;
				if (exitTimer <= 0)
				{
					exitThisMenu();
				}
			}
			Game1.mouseCursorTransparency = 1f;
		}

		public int getPriceFromButtonNumber(int buttonNumber)
		{
			switch (buttonNumber)
			{
			case 0:
				return 40000;
			case 1:
				return 15000;
			case 2:
				return 25000;
			case 3:
				return 35000;
			case 4:
				return 20000;
			default:
				return -1;
			}
		}

		public string getDescriptionFromButtonNumber(int buttonNumber)
		{
			return Game1.content.LoadString("Strings\\UI:JojaCDMenu_Hover" + buttonNumber);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			hoverText = "";
			foreach (ClickableComponent b in checkboxes)
			{
				if (b.containsPoint(x, y))
				{
					hoverText = (b.name.Equals("complete") ? "" : Game1.parseText(getDescriptionFromButtonNumber(Convert.ToInt32(b.name)), Game1.dialogueFont, 384));
				}
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			xPositionOnScreen = Game1.uiViewport.Width / 2 - 640;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - 288;
			int x = xPositionOnScreen + 4;
			int y = yPositionOnScreen + 208;
			checkboxes.Clear();
			for (int i = 0; i < 5; i++)
			{
				checkboxes.Add(new ClickableComponent(new Rectangle(x, y, 588, 120), string.Concat(i)));
				x += 592;
				if (x > xPositionOnScreen + 1184)
				{
					x = xPositionOnScreen + 4;
					y += 120;
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			b.Draw(noteTexture, Utility.getTopLeftPositionForCenteringOnScreen(1280, 576), new Rectangle(0, 0, 320, 144), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.79f);
			base.draw(b);
			foreach (ClickableComponent c in checkboxes)
			{
				if (c.name.Equals("complete"))
				{
					b.Draw(noteTexture, new Vector2(c.bounds.Left + 16, c.bounds.Y + 16), new Rectangle(0, 144, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
				}
			}
			Game1.dayTimeMoneyBox.drawMoneyBox(b, Game1.uiViewport.Width - 300 - IClickableMenu.spaceToClearSideBorder * 2, 4);
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
			if (hoverText != null && !hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
