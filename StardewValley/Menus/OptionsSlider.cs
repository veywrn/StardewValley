using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace StardewValley.Menus
{
	public class OptionsSlider : OptionsElement
	{
		public const int pixelsWide = 48;

		public const int pixelsHigh = 6;

		public const int sliderButtonWidth = 10;

		public const int sliderMaxValue = 100;

		public int value;

		public static Rectangle sliderBGSource = new Rectangle(403, 383, 6, 6);

		public static Rectangle sliderButtonRect = new Rectangle(420, 441, 10, 6);

		public OptionsSlider(string label, int whichOption, int x = -1, int y = -1)
			: base(label, x, y, 192, 24, whichOption)
		{
			Game1.options.setSliderToProperValue(this);
		}

		public override void leftClickHeld(int x, int y)
		{
			if (!greyedOut)
			{
				base.leftClickHeld(x, y);
				if (x < bounds.X)
				{
					value = 0;
				}
				else if (x > bounds.Right - 40)
				{
					value = 100;
				}
				else
				{
					value = (int)((float)(x - bounds.X) / (float)(bounds.Width - 40) * 100f);
				}
				Game1.options.changeSliderOption(whichOption, value);
			}
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (!greyedOut)
			{
				base.receiveLeftClick(x, y);
				leftClickHeld(x, y);
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && !greyedOut)
			{
				if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
				{
					value = Math.Min(value + 10, 100);
					Game1.options.changeSliderOption(whichOption, value);
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
				{
					value = Math.Max(value - 10, 0);
					Game1.options.changeSliderOption(whichOption, value);
				}
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY)
		{
			base.draw(b, slotX, slotY);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, sliderBGSource, slotX + bounds.X, slotY + bounds.Y, bounds.Width, bounds.Height, Color.White, 4f, drawShadow: false);
			b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + bounds.X) + (float)(bounds.Width - 40) * ((float)value / 100f), slotY + bounds.Y), sliderButtonRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
		}
	}
}
