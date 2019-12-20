using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using System;

namespace StardewValley.Menus
{
	public class DiscreteColorPicker : IClickableMenu
	{
		public const int sizeOfEachSwatch = 7;

		public Item itemToDrawColored;

		public bool showExample;

		public bool visible = true;

		public int colorSelection;

		public int totalColors;

		public DiscreteColorPicker(int xPosition, int yPosition, int startingColor = 0, Item itemToDrawColored = null)
		{
			totalColors = 21;
			xPositionOnScreen = xPosition;
			yPositionOnScreen = yPosition;
			width = totalColors * 9 * 4 + IClickableMenu.borderWidth;
			height = 28 + IClickableMenu.borderWidth;
			this.itemToDrawColored = itemToDrawColored;
			if (this.itemToDrawColored is Chest)
			{
				(itemToDrawColored as Chest).resetLidFrame();
			}
			visible = Game1.player.showChestColorPicker;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public int getSelectionFromColor(Color c)
		{
			for (int i = 0; i < totalColors; i++)
			{
				if (getColorFromSelection(i).Equals(c))
				{
					return i;
				}
			}
			return -1;
		}

		public Color getCurrentColor()
		{
			return getColorFromSelection(colorSelection);
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!visible)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			Rectangle area = new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth / 2, yPositionOnScreen + IClickableMenu.borderWidth / 2, 36 * totalColors, 28);
			if (area.Contains(x, y))
			{
				colorSelection = (x - area.X) / 36;
				try
				{
					Game1.playSound("coin");
				}
				catch (Exception)
				{
				}
				if (itemToDrawColored is Chest)
				{
					(itemToDrawColored as Chest).playerChoiceColor.Value = getColorFromSelection(colorSelection);
					(itemToDrawColored as Chest).resetLidFrame();
				}
			}
		}

		public Color getColorFromSelection(int selection)
		{
			switch (selection)
			{
			case 2:
				return new Color(119, 191, 255);
			case 1:
				return new Color(85, 85, 255);
			case 3:
				return new Color(0, 170, 170);
			case 4:
				return new Color(0, 234, 175);
			case 5:
				return new Color(0, 170, 0);
			case 6:
				return new Color(159, 236, 0);
			case 7:
				return new Color(255, 234, 18);
			case 8:
				return new Color(255, 167, 18);
			case 9:
				return new Color(255, 105, 18);
			case 10:
				return new Color(255, 0, 0);
			case 11:
				return new Color(135, 0, 35);
			case 12:
				return new Color(255, 173, 199);
			case 13:
				return new Color(255, 117, 195);
			case 14:
				return new Color(172, 0, 198);
			case 15:
				return new Color(143, 0, 255);
			case 16:
				return new Color(89, 11, 142);
			case 17:
				return new Color(64, 64, 64);
			case 18:
				return new Color(100, 100, 100);
			case 19:
				return new Color(200, 200, 200);
			case 20:
				return new Color(254, 254, 254);
			default:
				return Color.Black;
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!visible)
			{
				return;
			}
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.LightGray);
			for (int i = 0; i < totalColors; i++)
			{
				if (i == 0)
				{
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth / 2, yPositionOnScreen + IClickableMenu.borderWidth / 2), new Rectangle(295, 503, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
				else
				{
					b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth / 2 + i * 9 * 4, yPositionOnScreen + IClickableMenu.borderWidth / 2, 28, 28), getColorFromSelection(i));
				}
				if (i == colorSelection)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), xPositionOnScreen + IClickableMenu.borderWidth / 2 - 4 + i * 9 * 4, yPositionOnScreen + IClickableMenu.borderWidth / 2 - 4, 36, 36, Color.Black, 4f, drawShadow: false);
				}
			}
			if (itemToDrawColored != null && itemToDrawColored is Chest)
			{
				(itemToDrawColored as Chest).draw(b, xPositionOnScreen + width + IClickableMenu.borderWidth / 2, yPositionOnScreen + 16, 1f, local: true);
			}
		}
	}
}
