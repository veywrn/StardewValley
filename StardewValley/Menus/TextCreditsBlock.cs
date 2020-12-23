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

		private bool renderNameInEnglish;

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
			if (SpriteText.IsMissingCharacters(rawtext))
			{
				renderNameInEnglish = true;
			}
		}

		public override void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
		{
			if (renderNameInEnglish)
			{
				int parenthesis_index = text.IndexOf('(');
				if (parenthesis_index != -1 && parenthesis_index > 0)
				{
					string name = text.Substring(0, parenthesis_index);
					string parenthesis_text = text.Substring(parenthesis_index);
					SpriteText.forceEnglishFont = true;
					int width_of_text = (int)((float)SpriteText.getWidthOfString(name) / SpriteText.fontPixelZoom * 3f);
					SpriteText.drawString(b, name, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, junimoText: false, -1, "", color);
					SpriteText.forceEnglishFont = false;
					SpriteText.drawString(b, parenthesis_text, topLeftX + width_of_text, topLeftY, 999999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", color);
				}
				else
				{
					SpriteText.forceEnglishFont = true;
					SpriteText.drawString(b, text, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, junimoText: false, -1, "", color);
					SpriteText.forceEnglishFont = false;
				}
			}
			else
			{
				SpriteText.drawString(b, text, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, junimoText: false, -1, "", color);
			}
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
