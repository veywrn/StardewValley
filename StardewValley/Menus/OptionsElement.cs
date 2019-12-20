using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class OptionsElement
	{
		public const int defaultX = 8;

		public const int defaultY = 4;

		public const int defaultPixelWidth = 9;

		public Rectangle bounds;

		public string label;

		public int whichOption;

		public bool greyedOut;

		public Vector2 labelOffset = Vector2.Zero;

		public OptionsElement(string label)
		{
			this.label = label;
			bounds = new Rectangle(32, 16, 36, 36);
			whichOption = -1;
		}

		public OptionsElement(string label, int x, int y, int width, int height, int whichOption = -1)
		{
			if (x == -1)
			{
				x = 32;
			}
			if (y == -1)
			{
				y = 16;
			}
			bounds = new Rectangle(x, y, width, height);
			this.label = label;
			this.whichOption = whichOption;
		}

		public OptionsElement(string label, Rectangle bounds, int whichOption)
		{
			this.whichOption = whichOption;
			this.label = label;
			this.bounds = bounds;
		}

		public virtual void receiveLeftClick(int x, int y)
		{
		}

		public virtual void leftClickHeld(int x, int y)
		{
		}

		public virtual void leftClickReleased(int x, int y)
		{
		}

		public virtual void receiveKeyPress(Keys key)
		{
		}

		public virtual void draw(SpriteBatch b, int slotX, int slotY)
		{
			if (whichOption == -1)
			{
				SpriteText.drawString(b, label, slotX + bounds.X + (int)labelOffset.X, slotY + bounds.Y + (int)labelOffset.Y + 12, 999, -1, 999, 1f, 0.1f);
			}
			else
			{
				Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(slotX + bounds.X + bounds.Width + 8 + (int)labelOffset.X, slotY + bounds.Y + (int)labelOffset.Y), greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
			}
		}
	}
}
