using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Menus
{
	public class SliderBar
	{
		public static int defaultWidth = 128;

		public const int defaultHeight = 20;

		public int value;

		public Rectangle bounds;

		public SliderBar(int x, int y, int initialValue)
		{
			bounds = new Rectangle(x, y, defaultWidth, 20);
			value = initialValue;
		}

		public int click(int x, int y)
		{
			if (bounds.Contains(x, y))
			{
				x -= bounds.X;
				value = (int)((float)x / (float)bounds.Width * 100f);
			}
			return value;
		}

		public void changeValueBy(int amount)
		{
			value += amount;
			value = Math.Max(0, Math.Min(100, value));
		}

		public void release(int x, int y)
		{
		}

		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(bounds.X, bounds.Center.Y - 2, bounds.Width, 4), Color.DarkGray);
			b.Draw(Game1.mouseCursors, new Vector2(bounds.X + (int)((float)value / 100f * (float)bounds.Width) + 4, bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
		}
	}
}
