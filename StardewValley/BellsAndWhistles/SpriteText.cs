using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.BellsAndWhistles
{
	public class SpriteText
	{
		public enum ScrollTextAlignment
		{
			Left,
			Center,
			Right
		}

		public const int scrollStyle_scroll = 0;

		public const int scrollStyle_speechBubble = 1;

		public const int scrollStyle_darkMetal = 2;

		public const int maxCharacter = 999999;

		public const int maxHeight = 999999;

		public const int characterWidth = 8;

		public const int characterHeight = 16;

		public const int horizontalSpaceBetweenCharacters = 0;

		public const int verticalSpaceBetweenCharacters = 2;

		public const char newLine = '^';

		public static float fontPixelZoom = 3f;

		public static float shadowAlpha = 0.15f;

		private static Dictionary<char, FontChar> _characterMap;

		private static FontFile FontFile = null;

		private static List<Texture2D> fontPages = null;

		public static Texture2D spriteTexture;

		public static Texture2D coloredTexture;

		public const int color_Black = 0;

		public const int color_Blue = 1;

		public const int color_Red = 2;

		public const int color_Purple = 3;

		public const int color_White = 4;

		public const int color_Orange = 5;

		public const int color_Green = 6;

		public const int color_Cyan = 7;

		public const int color_Gray = 8;

		public static void drawStringHorizontallyCenteredAt(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int color = -1, int maxWidth = 99999)
		{
			drawString(b, s, x - getWidthOfString(s, maxWidth) / 2, y, characterPosition, width, height, alpha, layerDepth, junimoText, -1, "", color);
		}

		public static int getWidthOfString(string s, int widthConstraint = 999999)
		{
			setUpCharacterMap();
			int width = 0;
			int maxWidth = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (!LocalizedContentManager.CurrentLanguageLatin)
				{
					if (_characterMap.TryGetValue(s[i], out FontChar c))
					{
						width += c.XAdvance;
					}
					maxWidth = Math.Max(width, maxWidth);
					if (s[i] == '^' || (float)width * fontPixelZoom > (float)widthConstraint)
					{
						width = 0;
					}
					continue;
				}
				width += 8 + getWidthOffsetForChar(s[i]);
				if (i > 0)
				{
					width += getWidthOffsetForChar(s[Math.Max(0, i - 1)]);
				}
				maxWidth = Math.Max(width, maxWidth);
				float pos = positionOfNextSpace(s, i, (int)((float)width * fontPixelZoom), 0);
				if (s[i] == '^' || (float)width * fontPixelZoom >= (float)widthConstraint || pos >= (float)widthConstraint)
				{
					width = 0;
				}
			}
			return (int)((float)maxWidth * fontPixelZoom);
		}

		public static int getHeightOfString(string s, int widthConstraint = 999999)
		{
			if (s.Length == 0)
			{
				return 0;
			}
			Vector2 position = default(Vector2);
			int accumulatedHorizontalSpaceBetweenCharacters = 0;
			s = s.Replace(Environment.NewLine, "");
			setUpCharacterMap();
			if (!LocalizedContentManager.CurrentLanguageLatin)
			{
				for (int j = 0; j < s.Length; j++)
				{
					if (s[j] == '^')
					{
						position.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						position.X = 0f;
						continue;
					}
					if (positionOfNextSpace(s, j, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= widthConstraint)
					{
						position.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						position.X = 0f;
					}
					if (_characterMap.TryGetValue(s[j], out FontChar c))
					{
						position.X += (float)c.XAdvance * fontPixelZoom;
					}
				}
				return (int)(position.Y + (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom);
			}
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '^')
				{
					position.Y += 18f * fontPixelZoom;
					position.X = 0f;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					continue;
				}
				if (positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= widthConstraint)
				{
					position.Y += 18f * fontPixelZoom;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					position.X = 0f;
				}
				position.X += 8f * fontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)getWidthOffsetForChar(s[i]) * fontPixelZoom;
				if (i > 0)
				{
					position.X += (float)getWidthOffsetForChar(s[i - 1]) * fontPixelZoom;
				}
				accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * fontPixelZoom);
			}
			return (int)(position.Y + 16f * fontPixelZoom);
		}

		public static Color getColorFromIndex(int index)
		{
			switch (index)
			{
			case 1:
				return Color.SkyBlue;
			case 2:
				return Color.Red;
			case 3:
				return new Color(110, 43, 255);
			case -1:
				if (LocalizedContentManager.CurrentLanguageLatin)
				{
					return Color.White;
				}
				return new Color(86, 22, 12);
			case 4:
				return Color.White;
			case 5:
				return Color.OrangeRed;
			case 6:
				return Color.LimeGreen;
			case 7:
				return Color.Cyan;
			case 8:
				return new Color(60, 60, 60);
			default:
				return Color.Black;
			}
		}

		public static string getSubstringBeyondHeight(string s, int width, int height)
		{
			Vector2 position = default(Vector2);
			int accumulatedHorizontalSpaceBetweenCharacters = 0;
			s = s.Replace(Environment.NewLine, "");
			setUpCharacterMap();
			if (!LocalizedContentManager.CurrentLanguageLatin)
			{
				for (int j = 0; j < s.Length; j++)
				{
					if (s[j] == '^')
					{
						position.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						position.X = 0f;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						continue;
					}
					if (_characterMap.TryGetValue(s[j], out FontChar c))
					{
						if (j > 0)
						{
							position.X += (float)c.XAdvance * fontPixelZoom;
						}
						if (positionOfNextSpace(s, j, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
						{
							position.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
							accumulatedHorizontalSpaceBetweenCharacters = 0;
							position.X = 0f;
						}
					}
					if (position.Y >= (float)height - (float)FontFile.Common.LineHeight * fontPixelZoom * 2f)
					{
						return s.Substring(getLastSpace(s, j));
					}
				}
				return "";
			}
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '^')
				{
					position.Y += 18f * fontPixelZoom;
					position.X = 0f;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					continue;
				}
				if (i > 0)
				{
					position.X += 8f * fontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(getWidthOffsetForChar(s[i]) + getWidthOffsetForChar(s[i - 1])) * fontPixelZoom;
				}
				accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * fontPixelZoom);
				if (positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
				{
					position.Y += 18f * fontPixelZoom;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					position.X = 0f;
				}
				if (position.Y >= (float)height - 16f * fontPixelZoom * 2f)
				{
					return s.Substring(getLastSpace(s, i));
				}
			}
			return "";
		}

		public static int getIndexOfSubstringBeyondHeight(string s, int width, int height)
		{
			Vector2 position = default(Vector2);
			int accumulatedHorizontalSpaceBetweenCharacters = 0;
			s = s.Replace(Environment.NewLine, "");
			setUpCharacterMap();
			if (!LocalizedContentManager.CurrentLanguageLatin)
			{
				for (int j = 0; j < s.Length; j++)
				{
					if (s[j] == '^')
					{
						position.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						position.X = 0f;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						continue;
					}
					if (_characterMap.TryGetValue(s[j], out FontChar c))
					{
						if (j > 0)
						{
							position.X += (float)c.XAdvance * fontPixelZoom;
						}
						if (positionOfNextSpace(s, j, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
						{
							position.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
							accumulatedHorizontalSpaceBetweenCharacters = 0;
							position.X = 0f;
						}
					}
					if (position.Y >= (float)height - (float)FontFile.Common.LineHeight * fontPixelZoom * 2f)
					{
						return j - 1;
					}
				}
				return s.Length - 1;
			}
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '^')
				{
					position.Y += 18f * fontPixelZoom;
					position.X = 0f;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					continue;
				}
				if (i > 0)
				{
					position.X += 8f * fontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(getWidthOffsetForChar(s[i]) + getWidthOffsetForChar(s[i - 1])) * fontPixelZoom;
				}
				accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * fontPixelZoom);
				if (positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
				{
					position.Y += 18f * fontPixelZoom;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					position.X = 0f;
				}
				if (position.Y >= (float)height - 16f * fontPixelZoom)
				{
					return i - 1;
				}
			}
			return s.Length - 1;
		}

		public static List<string> getStringBrokenIntoSectionsOfHeight(string s, int width, int height)
		{
			List<string> brokenUp = new List<string>();
			while (s.Length > 0)
			{
				string tmp = getStringPreviousToThisHeightCutoff(s, width, height);
				if (tmp.Length <= 0)
				{
					break;
				}
				brokenUp.Add(tmp);
				s = s.Substring(brokenUp.Last().Length);
			}
			return brokenUp;
		}

		public static string getStringPreviousToThisHeightCutoff(string s, int width, int height)
		{
			return s.Substring(0, getIndexOfSubstringBeyondHeight(s, width, height) + 1);
		}

		private static int getLastSpace(string s, int startIndex)
		{
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
			{
				return startIndex;
			}
			for (int i = startIndex; i >= 0; i--)
			{
				if (s[i] == ' ')
				{
					return i;
				}
			}
			return startIndex;
		}

		public static int getWidthOffsetForChar(char c)
		{
			switch (c)
			{
			case ',':
			case '.':
				return -2;
			case '!':
			case 'j':
			case 'l':
			case '¡':
				return -1;
			case 'i':
			case 'ì':
			case 'í':
			case 'î':
			case 'ï':
			case 'ı':
				return -1;
			case '^':
				return -8;
			case '$':
				return 1;
			default:
				return 0;
			}
		}

		public static void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, int width, float alpha = 1f, int color = -1, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
		{
			drawString(b, s, x - width / 2, y, 999999, width, 999999, alpha, layerDepth, junimoText, scrollType, "", color, ScrollTextAlignment.Center);
		}

		public static void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, int color = -1, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
		{
			drawString(b, s, x - getWidthOfString((placeHolderWidthText.Length > 0) ? placeHolderWidthText : s) / 2, y, 999999, -1, 999999, alpha, layerDepth, junimoText, scrollType, placeHolderWidthText, color, ScrollTextAlignment.Center);
		}

		public static void drawStringWithScrollBackground(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, int color = -1, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
		{
			drawString(b, s, x, y, 999999, -1, 999999, alpha, 0.88f, junimoText: false, 0, placeHolderWidthText, color, scroll_text_alignment);
		}

		private static FontFile loadFont(string assetName)
		{
			return FontLoader.Parse(Game1.content.Load<XmlSource>(assetName).Source);
		}

		private static void setUpCharacterMap()
		{
			if (!LocalizedContentManager.CurrentLanguageLatin && _characterMap == null)
			{
				_characterMap = new Dictionary<char, FontChar>();
				fontPages = new List<Texture2D>();
				switch (LocalizedContentManager.CurrentLanguageCode)
				{
				case LocalizedContentManager.LanguageCode.ja:
					FontFile = loadFont("Fonts\\Japanese");
					fontPixelZoom = 1.75f;
					break;
				case LocalizedContentManager.LanguageCode.zh:
					FontFile = loadFont("Fonts\\Chinese");
					fontPixelZoom = 1.5f;
					break;
				case LocalizedContentManager.LanguageCode.ru:
					FontFile = loadFont("Fonts\\Russian");
					fontPixelZoom = 3f;
					break;
				case LocalizedContentManager.LanguageCode.th:
					FontFile = loadFont("Fonts\\Thai");
					fontPixelZoom = 1.5f;
					break;
				case LocalizedContentManager.LanguageCode.ko:
					FontFile = loadFont("Fonts\\Korean");
					fontPixelZoom = 1.5f;
					break;
				}
				foreach (FontChar fontCharacter in FontFile.Chars)
				{
					char c = (char)fontCharacter.ID;
					_characterMap.Add(c, fontCharacter);
				}
				foreach (FontPage fontPage in FontFile.Pages)
				{
					fontPages.Add(Game1.content.Load<Texture2D>("Fonts\\" + fontPage.File));
				}
				LocalizedContentManager.OnLanguageChange += OnLanguageChange;
			}
			else if (LocalizedContentManager.CurrentLanguageLatin && fontPixelZoom < 3f)
			{
				fontPixelZoom = 3f;
			}
		}

		public static void drawString(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int drawBGScroll = -1, string placeHolderScrollWidthText = "", int color = -1, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
		{
			setUpCharacterMap();
			bool width_specified = true;
			if (width == -1)
			{
				width_specified = false;
				width = Game1.graphics.GraphicsDevice.Viewport.Width - x;
				if (drawBGScroll == 1)
				{
					width = getWidthOfString(s) * 2;
				}
			}
			if (fontPixelZoom < 4f && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko)
			{
				y += (int)((4f - fontPixelZoom) * 4f);
			}
			Vector2 position = new Vector2(x, y);
			int accumulatedHorizontalSpaceBetweenCharacters = 0;
			if (drawBGScroll != 1)
			{
				if (position.X + (float)width > (float)(Game1.graphics.GraphicsDevice.Viewport.Width - 4))
				{
					position.X = Game1.graphics.GraphicsDevice.Viewport.Width - width - 4;
				}
				if (position.X < 0f)
				{
					position.X = 0f;
				}
			}
			switch (drawBGScroll)
			{
			case 0:
			case 2:
			{
				int scroll_width = getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
				if (width_specified)
				{
					scroll_width = width;
				}
				switch (drawBGScroll)
				{
				case 0:
					b.Draw(Game1.mouseCursors, position + new Vector2(-12f, -3f) * 4f, new Rectangle(325, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(337, 318, 1, 18), Color.White * alpha, 0f, Vector2.Zero, new Vector2(scroll_width, 4f), SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors, position + new Vector2(scroll_width, -12f), new Rectangle(338, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					break;
				case 2:
					b.Draw(Game1.mouseCursors, position + new Vector2(-3f, -3f) * 4f, new Rectangle(327, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(330, 281, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(scroll_width + 4, 4f), SpriteEffects.None, layerDepth - 0.001f);
					b.Draw(Game1.mouseCursors, position + new Vector2(scroll_width + 4, -12f), new Rectangle(333, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					break;
				}
				switch (scroll_text_alignment)
				{
				case ScrollTextAlignment.Center:
					x += (scroll_width - getWidthOfString(s)) / 2;
					position.X = x;
					break;
				case ScrollTextAlignment.Right:
					x += scroll_width - getWidthOfString(s);
					position.X = x;
					break;
				}
				position.Y += (4f - fontPixelZoom) * 4f;
				break;
			}
			case 1:
			{
				int text_width = getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
				Vector2 speech_position = position;
				if (Game1.currentLocation != null && Game1.currentLocation.map != null && Game1.currentLocation.map.Layers[0] != null)
				{
					int left_edge = -Game1.viewport.X + 28;
					int right_edge = -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 28;
					if (position.X < (float)left_edge)
					{
						position.X = left_edge;
					}
					if (position.X + (float)text_width > (float)right_edge)
					{
						position.X = right_edge - text_width;
					}
					speech_position.X += text_width / 2;
					if (speech_position.X < position.X)
					{
						position.X += speech_position.X - position.X;
					}
					if (speech_position.X > position.X + (float)text_width - 24f)
					{
						position.X += speech_position.X - (position.X + (float)text_width - 24f);
					}
					speech_position.X = Utility.Clamp(speech_position.X, position.X, position.X + (float)text_width - 24f);
				}
				b.Draw(Game1.mouseCursors, position + new Vector2(-7f, -3f) * 4f, new Rectangle(324, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(331, 299, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s), 4f), SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, position + new Vector2(text_width, -12f), new Rectangle(332, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, speech_position + new Vector2(0f, 52f), new Rectangle(341, 308, 6, 5), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.0001f);
				x = (int)position.X;
				if (placeHolderScrollWidthText.Length > 0)
				{
					x += getWidthOfString(placeHolderScrollWidthText) / 2 - getWidthOfString(s) / 2;
					position.X = x;
				}
				position.Y += (4f - fontPixelZoom) * 4f;
				break;
			}
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				position.Y -= 8f;
			}
			s = s.Replace(Environment.NewLine, "");
			if (!junimoText && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th))
			{
				position.Y -= (4f - fontPixelZoom) * 4f;
			}
			s = s.Replace('♡', '<');
			for (int i = 0; i < Math.Min(s.Length, characterPosition); i++)
			{
				if ((LocalizedContentManager.CurrentLanguageLatin || IsSpecialCharacter(s[i])) | junimoText)
				{
					float tempzoom = fontPixelZoom;
					if (IsSpecialCharacter(s[i]) | junimoText)
					{
						fontPixelZoom = 3f;
					}
					if (s[i] == '^')
					{
						position.Y += 18f * fontPixelZoom;
						position.X = x;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						continue;
					}
					accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * fontPixelZoom);
					bool upper = char.IsUpper(s[i]) || s[i] == 'ß';
					Vector2 spriteFontOffset = new Vector2(0f, -1 + ((!junimoText && upper) ? (-3) : 0));
					if (s[i] == 'Ç')
					{
						spriteFontOffset.Y += 2f;
					}
					if (positionOfNextSpace(s, i, (int)position.X - x, accumulatedHorizontalSpaceBetweenCharacters) >= width)
					{
						position.Y += 18f * fontPixelZoom;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						position.X = x;
						if (s[i] == ' ')
						{
							continue;
						}
					}
					b.Draw((color != -1) ? coloredTexture : spriteTexture, position + spriteFontOffset * fontPixelZoom, getSourceRectForChar(s[i], junimoText), ((IsSpecialCharacter(s[i]) | junimoText) ? Color.White : getColorFromIndex(color)) * alpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
					fontPixelZoom = tempzoom;
					if (i < s.Length - 1)
					{
						position.X += 8f * fontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)getWidthOffsetForChar(s[i + 1]) * fontPixelZoom;
					}
					if (s[i] != '^')
					{
						position.X += (float)getWidthOffsetForChar(s[i]) * fontPixelZoom;
					}
					continue;
				}
				if (s[i] == '^')
				{
					position.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
					position.X = x;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					continue;
				}
				if (i > 0 && IsSpecialCharacter(s[i - 1]))
				{
					position.X += 24f;
				}
				if (_characterMap.TryGetValue(s[i], out FontChar fc))
				{
					Rectangle sourcerect = new Rectangle(fc.X, fc.Y, fc.Width, fc.Height);
					Texture2D _texture = fontPages[fc.Page];
					if (positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= x + width - 4)
					{
						position.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						position.X = x;
					}
					Vector2 position2 = new Vector2(position.X + (float)fc.XOffset * fontPixelZoom, position.Y + (float)fc.YOffset * fontPixelZoom);
					if (drawBGScroll != -1 && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
					{
						position2.Y -= 8f;
					}
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
					{
						Vector2 offset = new Vector2(-1f, 1f) * fontPixelZoom;
						b.Draw(_texture, position2 + offset, sourcerect, getColorFromIndex(color) * alpha * shadowAlpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
						b.Draw(_texture, position2 + new Vector2(0f, offset.Y), sourcerect, getColorFromIndex(color) * alpha * shadowAlpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
						b.Draw(_texture, position2 + new Vector2(offset.X, 0f), sourcerect, getColorFromIndex(color) * alpha * shadowAlpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
					}
					b.Draw(_texture, position2, sourcerect, getColorFromIndex(color) * alpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
					position.X += (float)fc.XAdvance * fontPixelZoom;
				}
			}
		}

		private static bool IsSpecialCharacter(char c)
		{
			if (!c.Equals('<') && !c.Equals('=') && !c.Equals('>') && !c.Equals('@') && !c.Equals('$') && !c.Equals('`'))
			{
				return c.Equals('+');
			}
			return true;
		}

		private static void OnLanguageChange(LocalizedContentManager.LanguageCode code)
		{
			if (_characterMap != null)
			{
				_characterMap.Clear();
			}
			else
			{
				_characterMap = new Dictionary<char, FontChar>();
			}
			if (fontPages != null)
			{
				fontPages.Clear();
			}
			else
			{
				fontPages = new List<Texture2D>();
			}
			switch (code)
			{
			case LocalizedContentManager.LanguageCode.ja:
				FontFile = loadFont("Fonts\\Japanese");
				fontPixelZoom = 1.75f;
				break;
			case LocalizedContentManager.LanguageCode.zh:
				FontFile = loadFont("Fonts\\Chinese");
				fontPixelZoom = 1.5f;
				break;
			case LocalizedContentManager.LanguageCode.ru:
				FontFile = loadFont("Fonts\\Russian");
				fontPixelZoom = 3f;
				break;
			case LocalizedContentManager.LanguageCode.th:
				FontFile = loadFont("Fonts\\Thai");
				fontPixelZoom = 1.5f;
				break;
			case LocalizedContentManager.LanguageCode.ko:
				FontFile = loadFont("Fonts\\Korean");
				fontPixelZoom = 1.5f;
				break;
			}
			foreach (FontChar fontCharacter in FontFile.Chars)
			{
				char c = (char)fontCharacter.ID;
				_characterMap.Add(c, fontCharacter);
			}
			foreach (FontPage fontPage in FontFile.Pages)
			{
				fontPages.Add(Game1.content.Load<Texture2D>("Fonts\\" + fontPage.File));
			}
		}

		public static int positionOfNextSpace(string s, int index, int currentXPosition, int accumulatedHorizontalSpaceBetweenCharacters)
		{
			setUpCharacterMap();
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
			{
				if (_characterMap.TryGetValue(s[index], out FontChar fc))
				{
					return currentXPosition + (int)((float)fc.XAdvance * fontPixelZoom);
				}
				return currentXPosition + (int)((float)FontFile.Common.LineHeight * fontPixelZoom);
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				if (_characterMap.TryGetValue(s[index], out FontChar fc3))
				{
					return currentXPosition + (int)((float)fc3.XAdvance * fontPixelZoom);
				}
				return currentXPosition + (int)((float)FontFile.Common.LineHeight * fontPixelZoom);
			}
			for (int i = index; i < s.Length; i++)
			{
				if (!LocalizedContentManager.CurrentLanguageLatin)
				{
					if (s[i] == ' ' || s[i] == '^')
					{
						return currentXPosition;
					}
					currentXPosition = ((!_characterMap.TryGetValue(s[i], out FontChar fc2)) ? (currentXPosition + (int)((float)FontFile.Common.LineHeight * fontPixelZoom)) : (currentXPosition + (int)((float)fc2.XAdvance * fontPixelZoom)));
					continue;
				}
				if (s[i] == ' ' || s[i] == '^')
				{
					return currentXPosition;
				}
				currentXPosition += (int)(8f * fontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(getWidthOffsetForChar(s[i]) + getWidthOffsetForChar(s[Math.Max(0, i - 1)])) * fontPixelZoom);
				accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * fontPixelZoom);
			}
			return currentXPosition;
		}

		private static Rectangle getSourceRectForChar(char c, bool junimoText)
		{
			int i = c - 32;
			switch (c)
			{
			case 'Œ':
				i = 96;
				break;
			case 'œ':
				i = 97;
				break;
			case 'Ğ':
				i = 102;
				break;
			case 'ğ':
				i = 103;
				break;
			case 'İ':
				i = 98;
				break;
			case 'ı':
				i = 99;
				break;
			case 'Ş':
				i = 100;
				break;
			case 'ş':
				i = 101;
				break;
			case '’':
				i = 104;
				break;
			case 'Ő':
				i = 105;
				break;
			case 'ő':
				i = 106;
				break;
			case 'Ű':
				i = 107;
				break;
			case 'ű':
				i = 108;
				break;
			}
			return new Rectangle(i * 8 % spriteTexture.Width, i * 8 / spriteTexture.Width * 16 + (junimoText ? 224 : 0), 8, 16);
		}
	}
}
