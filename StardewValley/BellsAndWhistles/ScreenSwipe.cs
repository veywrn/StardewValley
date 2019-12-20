using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class ScreenSwipe
	{
		public const int swipe_bundleComplete = 0;

		public const int borderPixelWidth = 7;

		private Rectangle bgSource;

		private Rectangle flairSource;

		private Rectangle messageSource;

		private Rectangle movingFlairSource;

		private Rectangle bgDest;

		private int yPosition;

		private int durationAfterSwipe;

		private int originalBGSourceXLimit;

		private List<Vector2> flairPositions = new List<Vector2>();

		private Vector2 messagePosition;

		private Vector2 movingFlairPosition;

		private Vector2 movingFlairMotion;

		private float swipeVelocity;

		public ScreenSwipe(int which, float swipeVelocity = -1f, int durationAfterSwipe = -1)
		{
			Game1.playSound("throw");
			if (swipeVelocity == -1f)
			{
				swipeVelocity = 5f;
			}
			if (durationAfterSwipe == -1)
			{
				durationAfterSwipe = 2700;
			}
			this.swipeVelocity = swipeVelocity;
			this.durationAfterSwipe = durationAfterSwipe;
			Vector2 screenCenter = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2);
			if (which == 0)
			{
				messageSource = new Rectangle(128, 1367, 150, 14);
			}
			if (which == 0)
			{
				bgSource = new Rectangle(128, 1296, 1, 71);
				flairSource = new Rectangle(144, 1303, 144, 58);
				movingFlairSource = new Rectangle(643, 768, 8, 13);
				originalBGSourceXLimit = bgSource.X + bgSource.Width;
				yPosition = (int)screenCenter.Y - bgSource.Height * 4 / 2;
				messagePosition = new Vector2(screenCenter.X - (float)(messageSource.Width * 4 / 2), screenCenter.Y - (float)(messageSource.Height * 4 / 2));
				flairPositions.Add(new Vector2(messagePosition.X - (float)(flairSource.Width * 4) - 64f, yPosition + 28));
				flairPositions.Add(new Vector2(messagePosition.X + (float)(messageSource.Width * 4) + 64f, yPosition + 28));
				movingFlairPosition = new Vector2(messagePosition.X + (float)(messageSource.Width * 4) + 192f, screenCenter.Y + 32f);
				movingFlairMotion = new Vector2(0f, -0.5f);
			}
			bgDest = new Rectangle(0, yPosition, bgSource.Width * 4, bgSource.Height * 4);
		}

		public bool update(GameTime time)
		{
			if (durationAfterSwipe > 0 && bgDest.Width <= Game1.viewport.Width)
			{
				bgDest.Width += (int)((double)swipeVelocity * time.ElapsedGameTime.TotalMilliseconds);
				if (bgDest.Width > Game1.viewport.Width)
				{
					Game1.playSound("newRecord");
				}
			}
			else if (durationAfterSwipe <= 0)
			{
				bgDest.X += (int)((double)swipeVelocity * time.ElapsedGameTime.TotalMilliseconds);
				for (int i = 0; i < flairPositions.Count; i++)
				{
					if ((float)bgDest.X > flairPositions[i].X)
					{
						flairPositions[i] = new Vector2(bgDest.X, flairPositions[i].Y);
					}
				}
				if ((float)bgDest.X > messagePosition.X)
				{
					messagePosition = new Vector2(bgDest.X, messagePosition.Y);
				}
				if ((float)bgDest.X > movingFlairPosition.X)
				{
					movingFlairPosition = new Vector2(bgDest.X, movingFlairPosition.Y);
				}
			}
			if (bgDest.Width > Game1.viewport.Width && durationAfterSwipe > 0)
			{
				if (Game1.oldMouseState.LeftButton == ButtonState.Pressed)
				{
					durationAfterSwipe = 0;
				}
				durationAfterSwipe -= (int)time.ElapsedGameTime.TotalMilliseconds;
				if (durationAfterSwipe <= 0)
				{
					Game1.playSound("tinyWhip");
				}
			}
			movingFlairPosition += movingFlairMotion;
			return bgDest.X > Game1.viewport.Width;
		}

		public Rectangle getAdjustedSourceRect(Rectangle sourceRect, float xStartPosition)
		{
			if (xStartPosition > (float)bgDest.Width || xStartPosition + (float)(sourceRect.Width * 4) < (float)bgDest.X)
			{
				return Rectangle.Empty;
			}
			int x = (int)Math.Max(sourceRect.X, (float)sourceRect.X + ((float)bgDest.X - xStartPosition) / 4f);
			return new Rectangle(x, sourceRect.Y, (int)Math.Min(sourceRect.Width - (x - sourceRect.X) / 4, ((float)bgDest.Width - xStartPosition) / 4f), sourceRect.Height);
		}

		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, bgDest, bgSource, Color.White);
			foreach (Vector2 v in flairPositions)
			{
				Rectangle r = getAdjustedSourceRect(flairSource, v.X);
				if (r.Right >= originalBGSourceXLimit)
				{
					r.Width = originalBGSourceXLimit - r.X;
				}
				b.Draw(Game1.mouseCursors, v, r, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			b.Draw(Game1.mouseCursors, movingFlairPosition, getAdjustedSourceRect(movingFlairSource, movingFlairPosition.X), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, messagePosition, getAdjustedSourceRect(messageSource, messagePosition.X), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}
	}
}
