using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using System;
using System.Linq;

namespace StardewValley.Menus
{
	internal class TextCreditsBlock : ICreditsBlock
	{
		private string text;

		private int color;

		public TextCreditsBlock(string rawtext)
		{
			string[] split = rawtext.Split(']');
			if (split.Count() > 1)
			{
				text = split[1];
				color = Convert.ToInt32(split[0].Substring(1));
			}
			else
			{
				text = split[0];
				color = 4;
			}
		}

		public override void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
		{
			SpriteText.drawString(b, text, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, junimoText: false, -1, "", color);
		}

		public override int getHeight(int maxWidth)
		{
			if (!(text == ""))
			{
				return SpriteText.getHeightOfString(text, maxWidth);
			}
			return 64;
		}
	}
}
