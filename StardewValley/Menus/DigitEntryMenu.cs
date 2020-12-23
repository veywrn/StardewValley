using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	internal class DigitEntryMenu : NumberSelectionMenu
	{
		public List<ClickableComponent> digits = new List<ClickableComponent>();

		private int calculatorX;

		private int calculatorY;

		private int calculatorWidth;

		private int calculatorHeight;

		private static string clear = "c";

		protected override Vector2 centerPosition => new Vector2(Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2 + 128);

		public DigitEntryMenu(string message, behaviorOnNumberSelect behaviorOnSelection, int price = -1, int minValue = 0, int maxValue = 99, int defaultNumber = 0)
			: base(message, behaviorOnSelection, price, minValue, maxValue, defaultNumber)
		{
			Game1.dialogueFont.MeasureString(message);
			_ = IClickableMenu.borderWidth;
			int buttonsPerRow = 3;
			int buttonWidth = 44;
			int buttonHeight = buttonWidth;
			int bufferX = 8;
			int bufferY = bufferX;
			int rowWidth = buttonsPerRow * buttonWidth + (buttonsPerRow - 1) * bufferX;
			calculatorWidth = buttonWidth * buttonsPerRow + bufferX * (buttonsPerRow - 1) + IClickableMenu.spaceToClearSideBorder * 2 + 128;
			calculatorHeight = buttonHeight * 4 + bufferY * 3 + IClickableMenu.spaceToClearTopBorder * 2;
			calculatorX = Game1.uiViewport.Width / 2 - calculatorWidth / 2;
			calculatorY = Game1.uiViewport.Height / 2 - calculatorHeight;
			int buttonX = Game1.uiViewport.Width / 2;
			int buttonY = Game1.uiViewport.Height / 2 - 384 + 24 + IClickableMenu.spaceToClearTopBorder;
			for (int i = 0; i < 11; i++)
			{
				string digit = (i + 1).ToString();
				switch (i)
				{
				case 9:
					digit = clear;
					break;
				case 10:
					digit = "0";
					break;
				}
				digits.Add(new ClickableComponent(new Rectangle(buttonX - rowWidth / 2 + i % buttonsPerRow * (bufferX + buttonWidth), buttonY + i / buttonsPerRow * (bufferY + buttonHeight), buttonWidth, buttonHeight), digit)
				{
					myID = i,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998,
					upNeighborID = -99998
				});
			}
			populateClickableComponentList();
		}

		private void onDigitPressed(string digit)
		{
			if (digit == clear)
			{
				currentValue = 0;
				numberSelectedBox.Text = currentValue.ToString();
			}
			else
			{
				string currentStr = currentValue.ToString();
				currentValue = Math.Min(val2: Convert.ToInt32((!(currentStr == "0")) ? (currentStr + digit) : digit.ToString()), val1: maxValue);
				numberSelectedBox.Text = currentValue.ToString();
			}
		}

		public override bool isWithinBounds(int x, int y)
		{
			if (!base.isWithinBounds(x, y))
			{
				if (x - calculatorX < calculatorWidth && x - calculatorX >= 0 && y - calculatorY < calculatorHeight)
				{
					return y - calculatorY >= 0;
				}
				return false;
			}
			return true;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableComponent c in digits)
			{
				if (c.containsPoint(x, y))
				{
					Game1.playSound("smallSelect");
					onDigitPressed(c.name);
				}
			}
			base.receiveLeftClick(x, y, playSound: true);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableComponent c in digits)
			{
				if (c.containsPoint(x, y))
				{
					c.scale = 2f;
				}
				else
				{
					c.scale = 1f;
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Game1.drawDialogueBox(calculatorX, calculatorY, calculatorWidth, calculatorHeight, speaker: false, drawOnlyBox: true);
			foreach (ClickableComponent c in digits)
			{
				if (c.name == clear)
				{
					b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X - 4, c.bounds.Y + 4), new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10), Color.Black * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X, c.bounds.Y), new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10), Color.White * 0.6f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
					Vector2 textPosition = new Vector2(c.bounds.X + c.bounds.Width / 2 - SpriteText.getWidthOfString(c.name) / 2, c.bounds.Y + c.bounds.Height / 2 - SpriteText.getHeightOfString(c.name) / 2 - 4);
					SpriteText.drawString(b, c.name, (int)textPosition.X, (int)textPosition.Y);
				}
				else
				{
					b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X - 4, c.bounds.Y + 4), new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10), Color.Black * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X, c.bounds.Y), new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
					NumberSprite.draw(position: new Vector2(c.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(c.name)) * 6, c.bounds.Y + 24 - NumberSprite.getHeight() / 4), number: Convert.ToInt32(c.name), b: b, c: Color.Gold, scale: 0.5f, layerDepth: 0.86f, alpha: 1f, secondDigitOffset: 0);
				}
			}
			drawMouse(b);
		}
	}
}
