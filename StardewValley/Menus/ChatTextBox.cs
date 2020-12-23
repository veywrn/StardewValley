using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class ChatTextBox : TextBox
	{
		public IClickableMenu parentMenu;

		public List<ChatSnippet> finalText = new List<ChatSnippet>();

		public float currentWidth;

		public ChatTextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor)
			: base(textBoxTexture, caretTexture, font, textColor)
		{
		}

		public void reset()
		{
			currentWidth = 0f;
			finalText.Clear();
		}

		public void setText(string text)
		{
			reset();
			RecieveTextInput(text);
		}

		public override void RecieveTextInput(string text)
		{
			if (finalText.Count == 0)
			{
				finalText.Add(new ChatSnippet("", LocalizedContentManager.CurrentLanguageCode));
			}
			if (!(currentWidth + ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode).MeasureString(text).X >= (float)(base.Width - 16)))
			{
				if (finalText.Last().message != null)
				{
					finalText.Last().message += text;
				}
				else
				{
					finalText.Add(new ChatSnippet(text, LocalizedContentManager.CurrentLanguageCode));
				}
				updateWidth();
			}
		}

		public override void RecieveTextInput(char inputChar)
		{
			RecieveTextInput(inputChar.ToString() ?? "");
		}

		public override void RecieveCommandInput(char command)
		{
			if (base.Selected && command == '\b')
			{
				backspace();
			}
			else
			{
				base.RecieveCommandInput(command);
			}
		}

		public void backspace()
		{
			if (finalText.Count > 0)
			{
				if (finalText.Last().message != null)
				{
					if (finalText.Last().message.Length > 1)
					{
						finalText.Last().message = finalText.Last().message.Remove(finalText.Last().message.Length - 1);
					}
					else
					{
						finalText.RemoveAt(finalText.Count - 1);
					}
				}
				else if (finalText.Last().emojiIndex != -1)
				{
					finalText.RemoveAt(finalText.Count - 1);
				}
			}
			updateWidth();
		}

		public void receiveEmoji(int emoji)
		{
			if (!(currentWidth + 40f > (float)(base.Width - 16)))
			{
				finalText.Add(new ChatSnippet(emoji));
				updateWidth();
			}
		}

		public void updateWidth()
		{
			currentWidth = 0f;
			foreach (ChatSnippet cs in finalText)
			{
				if (cs.message != null)
				{
					cs.myLength = ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode).MeasureString(cs.message).X;
				}
				currentWidth += cs.myLength;
			}
		}

		public override void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
		{
			bool caretVisible2 = true;
			caretVisible2 = ((!(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 < 500.0)) ? true : false);
			if (_textBoxTexture != null)
			{
				spriteBatch.Draw(_textBoxTexture, new Rectangle(base.X, base.Y, 16, base.Height), new Rectangle(0, 0, 16, base.Height), Color.White);
				spriteBatch.Draw(_textBoxTexture, new Rectangle(base.X + 16, base.Y, base.Width - 32, base.Height), new Rectangle(16, 0, 4, base.Height), Color.White);
				spriteBatch.Draw(_textBoxTexture, new Rectangle(base.X + base.Width - 16, base.Y, 16, base.Height), new Rectangle(_textBoxTexture.Bounds.Width - 16, 0, 16, base.Height), Color.White);
			}
			else
			{
				Game1.drawDialogueBox(base.X - 32, base.Y - 112 + 10, base.Width + 80, base.Height, speaker: false, drawOnlyBox: true);
			}
			if (caretVisible2 && base.Selected)
			{
				spriteBatch.Draw(Game1.staminaRect, new Rectangle(base.X + 16 + (int)currentWidth - 2, base.Y + 8, 4, 32), _textColor);
			}
			float xPositionSoFar = 0f;
			for (int i = 0; i < finalText.Count; i++)
			{
				if (finalText[i].emojiIndex != -1)
				{
					spriteBatch.Draw(ChatBox.emojiTexture, new Vector2((float)base.X + xPositionSoFar + 12f, base.Y + 12), new Rectangle(finalText[i].emojiIndex * 9 % ChatBox.emojiTexture.Width, finalText[i].emojiIndex * 9 / ChatBox.emojiTexture.Width * 9, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				}
				else if (finalText[i].message != null)
				{
					spriteBatch.DrawString(ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode), finalText[i].message, new Vector2((float)base.X + xPositionSoFar + 12f, base.Y + 12), ChatMessage.getColorFromName(Game1.player.defaultChatColor), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
				}
				xPositionSoFar += finalText[i].myLength;
			}
		}
	}
}
