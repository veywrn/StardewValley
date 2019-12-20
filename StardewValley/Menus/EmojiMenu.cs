using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class EmojiMenu : IClickableMenu
	{
		public const int EMOJI_SIZE = 9;

		private Texture2D chatBoxTexture;

		private Texture2D emojiTexture;

		private ChatBox chatBox;

		private List<ClickableComponent> emojiSelectionButtons = new List<ClickableComponent>();

		private int pageStartIndex;

		private ClickableComponent upArrow;

		private ClickableComponent downArrow;

		private ClickableComponent sendArrow;

		public static int totalEmojis;

		public EmojiMenu(ChatBox chatBox, Texture2D emojiTexture, Texture2D chatBoxTexture)
		{
			this.chatBox = chatBox;
			this.chatBoxTexture = chatBoxTexture;
			this.emojiTexture = emojiTexture;
			width = 300;
			height = 248;
			for (int y = 0; y < 5; y++)
			{
				for (int x = 0; x < 6; x++)
				{
					emojiSelectionButtons.Add(new ClickableComponent(new Rectangle(16 + x * 10 * 4, 16 + y * 10 * 4, 36, 36), string.Concat(x + y * 6)));
				}
			}
			upArrow = new ClickableComponent(new Rectangle(256, 16, 32, 20), "");
			downArrow = new ClickableComponent(new Rectangle(256, 156, 32, 20), "");
			sendArrow = new ClickableComponent(new Rectangle(256, 188, 32, 32), "");
			totalEmojis = ChatBox.emojiTexture.Width / 9 * (ChatBox.emojiTexture.Height / 9);
		}

		public void leftClick(int x, int y, ChatBox cb)
		{
			if (isWithinBounds(x, y))
			{
				int relativeX = x - xPositionOnScreen;
				int relativeY = y - yPositionOnScreen;
				if (upArrow.containsPoint(relativeX, relativeY))
				{
					upArrowPressed();
				}
				else if (downArrow.containsPoint(relativeX, relativeY))
				{
					downArrowPressed();
				}
				else if (sendArrow.containsPoint(relativeX, relativeY) && cb.chatBox.currentWidth > 0f)
				{
					cb.textBoxEnter(cb.chatBox);
					sendArrow.scale = 0.5f;
					Game1.playSound("shwip");
				}
				foreach (ClickableComponent c in emojiSelectionButtons)
				{
					if (c.containsPoint(relativeX, relativeY))
					{
						int index = pageStartIndex + int.Parse(c.name);
						cb.chatBox.receiveEmoji(index);
						Game1.playSound("coin");
						break;
					}
				}
			}
		}

		private void upArrowPressed(int amountToScroll = 30)
		{
			if (pageStartIndex != 0)
			{
				Game1.playSound("Cowboy_Footstep");
			}
			pageStartIndex = Math.Max(0, pageStartIndex - amountToScroll);
			upArrow.scale = 0.75f;
		}

		private void downArrowPressed(int amountToScroll = 30)
		{
			if (pageStartIndex != totalEmojis - 30)
			{
				Game1.playSound("Cowboy_Footstep");
			}
			pageStartIndex = Math.Min(totalEmojis - 30, pageStartIndex + amountToScroll);
			downArrow.scale = 0.75f;
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (direction < 0)
			{
				downArrowPressed(6);
			}
			else if (direction > 0)
			{
				upArrowPressed(6);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(chatBoxTexture, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), new Rectangle(0, 56, 300, 244), Color.White);
			for (int i = 0; i < emojiSelectionButtons.Count; i++)
			{
				b.Draw(emojiTexture, new Vector2(emojiSelectionButtons[i].bounds.X + xPositionOnScreen, emojiSelectionButtons[i].bounds.Y + yPositionOnScreen), new Rectangle((pageStartIndex + i) * 9 % emojiTexture.Width, (pageStartIndex + i) * 9 / emojiTexture.Width * 9, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			}
			if (upArrow.scale < 1f)
			{
				upArrow.scale += 0.05f;
			}
			if (downArrow.scale < 1f)
			{
				downArrow.scale += 0.05f;
			}
			if (sendArrow.scale < 1f)
			{
				sendArrow.scale += 0.05f;
			}
			b.Draw(chatBoxTexture, new Vector2(upArrow.bounds.X + xPositionOnScreen + 16, upArrow.bounds.Y + yPositionOnScreen + 10), new Rectangle(156, 300, 32, 20), Color.White * ((pageStartIndex == 0) ? 0.25f : 1f), 0f, new Vector2(16f, 10f), upArrow.scale, SpriteEffects.None, 0.9f);
			b.Draw(chatBoxTexture, new Vector2(downArrow.bounds.X + xPositionOnScreen + 16, downArrow.bounds.Y + yPositionOnScreen + 10), new Rectangle(192, 300, 32, 20), Color.White * ((pageStartIndex == totalEmojis - 30) ? 0.25f : 1f), 0f, new Vector2(16f, 10f), downArrow.scale, SpriteEffects.None, 0.9f);
			b.Draw(chatBoxTexture, new Vector2(sendArrow.bounds.X + xPositionOnScreen + 16, sendArrow.bounds.Y + yPositionOnScreen + 10), new Rectangle(116, 304, 28, 28), Color.White * ((chatBox.chatBox.currentWidth > 0f) ? 1f : 0.4f), 0f, new Vector2(14f, 16f), sendArrow.scale, SpriteEffects.None, 0.9f);
		}
	}
}
