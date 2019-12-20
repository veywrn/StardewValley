using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StardewValley.Menus
{
	public class ChatMessage
	{
		public List<ChatSnippet> message = new List<ChatSnippet>();

		public int timeLeftToDisplay;

		public int verticalSize;

		public float alpha = 1f;

		public Color color;

		public LocalizedContentManager.LanguageCode language;

		public void parseMessageForEmoji(string messagePlaintext)
		{
			if (messagePlaintext == null)
			{
				return;
			}
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < messagePlaintext.Count(); i++)
			{
				if (messagePlaintext[i] == '[')
				{
					if (sb.Length > 0)
					{
						breakNewLines(sb);
					}
					sb.Clear();
					int tag_close_index = messagePlaintext.IndexOf(']', i);
					int next_open_index = -1;
					if (i + 1 < messagePlaintext.Count())
					{
						next_open_index = messagePlaintext.IndexOf('[', i + 1);
					}
					if (tag_close_index != -1 && (next_open_index == -1 || next_open_index > tag_close_index))
					{
						string sub = messagePlaintext.Substring(i + 1, tag_close_index - i - 1);
						int emojiIndex = -1;
						if (int.TryParse(sub, out emojiIndex))
						{
							if (emojiIndex < EmojiMenu.totalEmojis)
							{
								message.Add(new ChatSnippet(emojiIndex));
							}
						}
						else
						{
							switch (sub)
							{
							case "aqua":
							case "red":
							case "jungle":
							case "blue":
							case "green":
							case "yellowgreen":
							case "pink":
							case "yellow":
							case "orange":
							case "purple":
							case "gray":
							case "cream":
							case "peach":
							case "plum":
							case "jade":
							case "salmon":
							case "brown":
								if (color.Equals(Color.White))
								{
									color = getColorFromName(sub);
								}
								break;
							default:
								sb.Append("[");
								sb.Append(sub);
								sb.Append("]");
								break;
							}
						}
						i = tag_close_index;
					}
					else
					{
						sb.Append("[");
					}
				}
				else
				{
					sb.Append(messagePlaintext[i]);
				}
			}
			if (sb.Length > 0)
			{
				breakNewLines(sb);
			}
		}

		public static Color getColorFromName(string name)
		{
			switch (name)
			{
			case "aqua":
				return Color.MediumTurquoise;
			case "jungle":
				return Color.SeaGreen;
			case "red":
				return new Color(220, 20, 20);
			case "blue":
				return Color.DodgerBlue;
			case "jade":
				return new Color(50, 230, 150);
			case "green":
				return new Color(0, 180, 10);
			case "yellowgreen":
				return new Color(182, 214, 0);
			case "pink":
				return Color.HotPink;
			case "yellow":
				return new Color(240, 200, 0);
			case "orange":
				return new Color(255, 100, 0);
			case "purple":
				return new Color(138, 43, 250);
			case "gray":
				return Color.Gray;
			case "cream":
				return new Color(255, 255, 180);
			case "peach":
				return new Color(255, 180, 120);
			case "brown":
				return new Color(160, 80, 30);
			case "salmon":
				return Color.Salmon;
			case "plum":
				return new Color(190, 0, 190);
			default:
				return Color.White;
			}
		}

		private void breakNewLines(StringBuilder sb)
		{
			string[] split = sb.ToString().Split(new string[1]
			{
				Environment.NewLine
			}, StringSplitOptions.None);
			for (int i = 0; i < split.Length; i++)
			{
				message.Add(new ChatSnippet(split[i], language));
				if (i != split.Length - 1)
				{
					message.Add(new ChatSnippet(Environment.NewLine, language));
				}
			}
		}

		public static string makeMessagePlaintext(List<ChatSnippet> message, bool include_color_information)
		{
			StringBuilder sb = new StringBuilder();
			foreach (ChatSnippet cs in message)
			{
				if (cs.message != null)
				{
					sb.Append(cs.message);
				}
				else if (cs.emojiIndex != -1)
				{
					sb.Append("[" + cs.emojiIndex + "]");
				}
			}
			if (include_color_information && Game1.player.defaultChatColor != null && !getColorFromName(Game1.player.defaultChatColor).Equals(Color.White))
			{
				sb.Append(" [");
				sb.Append(Game1.player.defaultChatColor);
				sb.Append("]");
			}
			return sb.ToString();
		}

		public void draw(SpriteBatch b, int x, int y)
		{
			float xPositionSoFar = 0f;
			float yPositionSoFar = 0f;
			for (int i = 0; i < message.Count; i++)
			{
				if (message[i].emojiIndex != -1)
				{
					b.Draw(ChatBox.emojiTexture, new Vector2((float)x + xPositionSoFar + 1f, (float)y + yPositionSoFar - 4f), new Rectangle(message[i].emojiIndex * 9 % ChatBox.emojiTexture.Width, message[i].emojiIndex * 9 / ChatBox.emojiTexture.Width * 9, 9, 9), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				}
				else if (message[i].message != null)
				{
					if (message[i].message.Equals(Environment.NewLine))
					{
						xPositionSoFar = 0f;
						yPositionSoFar += ChatBox.messageFont(language).MeasureString("(").Y;
					}
					else
					{
						b.DrawString(ChatBox.messageFont(language), message[i].message, new Vector2((float)x + xPositionSoFar, (float)y + yPositionSoFar), color * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
					}
				}
				xPositionSoFar += message[i].myLength;
				if (xPositionSoFar >= 888f)
				{
					xPositionSoFar = 0f;
					yPositionSoFar += ChatBox.messageFont(language).MeasureString("(").Y;
					if (message.Count > i + 1 && message[i + 1].message != null && message[i + 1].message.Equals(Environment.NewLine))
					{
						i++;
					}
				}
			}
		}
	}
}
