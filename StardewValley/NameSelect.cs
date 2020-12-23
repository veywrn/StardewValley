using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	[InstanceStatics]
	public class NameSelect
	{
		public const int maxNameLength = 9;

		public const int charactersPerRow = 15;

		public static string name = "";

		private static int selection = 0;

		private static List<char> namingCharacters;

		public static void load()
		{
			namingCharacters = new List<char>();
			for (int l = 0; l < 25; l += 5)
			{
				for (int k = 0; k < 5; k++)
				{
					namingCharacters.Add((char)(97 + l + k));
				}
				for (int j = 0; j < 5; j++)
				{
					namingCharacters.Add((char)(65 + l + j));
				}
				if (l < 10)
				{
					for (int i = 0; i < 5; i++)
					{
						namingCharacters.Add((char)(48 + l + i));
					}
				}
				else if (l < 15)
				{
					namingCharacters.Add('?');
					namingCharacters.Add('$');
					namingCharacters.Add('\'');
					namingCharacters.Add('#');
					namingCharacters.Add('[');
				}
				else if (l < 20)
				{
					namingCharacters.Add('-');
					namingCharacters.Add('=');
					namingCharacters.Add('~');
					namingCharacters.Add('&');
					namingCharacters.Add('!');
				}
				else
				{
					namingCharacters.Add('Z');
					namingCharacters.Add('z');
					namingCharacters.Add('<');
					namingCharacters.Add('"');
					namingCharacters.Add(']');
				}
			}
		}

		public static void draw()
		{
			int width = Math.Min(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width % 64, Game1.graphics.GraphicsDevice.Viewport.Width - Game1.graphics.GraphicsDevice.Viewport.Width % 64 - 128);
			int height = Math.Min(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Height - Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Height % 64, Game1.graphics.GraphicsDevice.Viewport.Height - Game1.graphics.GraphicsDevice.Viewport.Height % 64 - 64);
			int xLocation = Game1.graphics.GraphicsDevice.Viewport.Width / 2 - width / 2;
			int yLocation = Game1.graphics.GraphicsDevice.Viewport.Height / 2 - height / 2;
			int widthBetweenCharacters = (width - 128) / 15;
			int heightBetweenCharacters = (height - 256) / 5;
			Game1.drawDialogueBox(xLocation, yLocation, width, height, speaker: false, drawOnlyBox: true);
			string titleMessage = "";
			switch (Game1.nameSelectType)
			{
			case "samBand":
				titleMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:NameSelect.cs.3856");
				break;
			case "Animal":
			case "playerName":
			case "coopDwellerBorn":
				titleMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:NameSelect.cs.3860");
				break;
			}
			Game1.spriteBatch.DrawString(Game1.dialogueFont, titleMessage, new Vector2(xLocation + 128, yLocation + 128), Game1.textColor);
			int titleMessageWidth = (int)Game1.dialogueFont.MeasureString(titleMessage).X;
			string underline = "";
			for (int k = 0; k < 9; k++)
			{
				if (name.Length > k)
				{
					Game1.spriteBatch.DrawString(Game1.dialogueFont, name[k].ToString() ?? "", new Vector2((float)(xLocation + 128 + titleMessageWidth) + Game1.dialogueFont.MeasureString(underline).X + (Game1.dialogueFont.MeasureString("_").X - Game1.dialogueFont.MeasureString(name[k].ToString() ?? "").X) / 2f - 2f, yLocation + 128 - 6), Game1.textColor);
				}
				underline += "_ ";
			}
			Game1.spriteBatch.DrawString(Game1.dialogueFont, "_ _ _ _ _ _ _ _ _", new Vector2(xLocation + 128 + titleMessageWidth, yLocation + 128), Game1.textColor);
			Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:NameSelect.cs.3864"), new Vector2(xLocation + width - 192, yLocation + height - 96), Game1.textColor);
			for (int j = 0; j < 5; j++)
			{
				int sectionBuffer = 0;
				for (int i = 0; i < 15; i++)
				{
					if (i != 0 && i % 5 == 0)
					{
						sectionBuffer += widthBetweenCharacters / 3;
					}
					Game1.spriteBatch.DrawString(Game1.dialogueFont, namingCharacters[j * 15 + i].ToString() ?? "", new Vector2(sectionBuffer + xLocation + 64 + widthBetweenCharacters * i, yLocation + 192 + heightBetweenCharacters * j), Game1.textColor);
					if (selection == j * 15 + i)
					{
						Game1.spriteBatch.Draw(Game1.objectSpriteSheet, new Vector2(sectionBuffer + xLocation + widthBetweenCharacters * i - 6, yLocation + 192 + heightBetweenCharacters * j - 8), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 26), Color.White);
					}
				}
			}
			if (selection == -1)
			{
				Game1.spriteBatch.Draw(Game1.objectSpriteSheet, new Vector2(xLocation + width - 192 - 64 - 6, yLocation + height - 96 - 8), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 26), Color.White);
			}
		}

		public static bool select()
		{
			if (selection == -1)
			{
				if (name.Length > 0)
				{
					return true;
				}
			}
			else if (name.Length < 9)
			{
				name += namingCharacters[selection].ToString();
				Game1.playSound("smallSelect");
			}
			return false;
		}

		public static void startButton()
		{
			if (name.Length > 0)
			{
				selection = -1;
				Game1.playSound("smallSelect");
			}
		}

		public static bool isOnDone()
		{
			return selection == -1;
		}

		public static void backspace()
		{
			if (name.Length > 0)
			{
				name = name.Remove(name.Length - 1);
				Game1.playSound("toolSwap");
			}
		}

		public static bool cancel()
		{
			if ((Game1.nameSelectType.Equals("samBand") || Game1.nameSelectType.Equals("coopDwellerBorn")) && name.Length > 0)
			{
				Game1.playSound("toolSwap");
				name = name.Remove(name.Length - 1);
				return false;
			}
			selection = 0;
			name = "";
			return true;
		}

		public static void moveSelection(int direction)
		{
			Game1.playSound("toolSwap");
			if (direction.Equals(0))
			{
				if (selection == -1)
				{
					selection = namingCharacters.Count - 2;
				}
				else if (selection - 15 < 0)
				{
					selection = namingCharacters.Count - 15 + selection;
				}
				else
				{
					selection -= 15;
				}
			}
			else if (direction.Equals(1))
			{
				selection++;
				if (selection % 15 == 0)
				{
					selection -= 15;
				}
			}
			else if (direction.Equals(2))
			{
				if (selection >= namingCharacters.Count - 2)
				{
					selection = -1;
					return;
				}
				selection += 15;
				if (selection >= namingCharacters.Count)
				{
					selection -= namingCharacters.Count;
				}
			}
			else if (direction.Equals(3))
			{
				if (selection % 15 == 0)
				{
					selection += 14;
				}
				else
				{
					selection--;
				}
			}
		}
	}
}
