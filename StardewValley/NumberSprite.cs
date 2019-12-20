using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley
{
	public class NumberSprite
	{
		public const int textureX = 512;

		public const int textureY = 128;

		public const int digitWidth = 8;

		public const int digitHeight = 8;

		public const int groupWidth = 48;

		public static void draw(int number, SpriteBatch b, Vector2 position, Color c, float scale, float layerDepth, float alpha, int secondDigitOffset, int spaceBetweenDigits = 0)
		{
			int digit = 1;
			secondDigitOffset = Math.Min(0, secondDigitOffset);
			do
			{
				int currentDigit = number % 10;
				number /= 10;
				int textX = 512 + currentDigit * 8 % 48;
				int textY = 128 + currentDigit * 8 / 48 * 8;
				b.Draw(Game1.mouseCursors, position + new Vector2(0f, (digit == 2) ? secondDigitOffset : 0), new Rectangle(textX, textY, 8, 8), c * alpha, 0f, new Vector2(4f, 4f), 4f * scale, SpriteEffects.None, layerDepth);
				position.X -= 8f * scale * 4f - 4f - (float)spaceBetweenDigits;
				digit++;
			}
			while (number > 0);
		}

		public static int getHeight()
		{
			return 8;
		}

		public static int getWidth(string number)
		{
			return getWidth(Convert.ToInt32(number));
		}

		public static int getWidth(int number)
		{
			int width = 8;
			number /= 10;
			while (number != 0)
			{
				number /= 10;
				width += 8;
			}
			return width;
		}

		public static int numberOfDigits(int number)
		{
			int num = 1;
			number /= 10;
			while (number != 0)
			{
				number /= 10;
				num++;
			}
			return num;
		}
	}
}
