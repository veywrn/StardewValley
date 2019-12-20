using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace StardewValley.Minigames
{
	public class PlaneFlyBy : IMinigame
	{
		public const float backGroundSpeed = 0.25f;

		public const float robotSpeed = 1f;

		public const int skyLength = 2560;

		public int millisecondsSinceStart;

		public int backgroundPosition = -2560 + Game1.graphics.GraphicsDevice.Viewport.Height;

		public int smokeTimer = 500;

		public Vector2 robotPosition = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height / 2) * 1f / Game1.options.zoomLevel;

		public List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool tick(GameTime time)
		{
			millisecondsSinceStart += time.ElapsedGameTime.Milliseconds;
			robotPosition.X -= 1f * (float)time.ElapsedGameTime.Milliseconds / 4f;
			smokeTimer -= time.ElapsedGameTime.Milliseconds;
			if (smokeTimer <= 0)
			{
				smokeTimer = 100;
				tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(173, 1828, 15, 20), 1500f, 2, 0, robotPosition + new Vector2(68f, -24f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, 0.1f),
					scale = 4f,
					scaleChange = 0.002f,
					alphaFade = 0.0025f,
					rotation = -(float)Math.PI / 2f
				});
			}
			for (int i = tempSprites.Count - 1; i >= 0; i--)
			{
				if (tempSprites[i].update(time))
				{
					tempSprites.RemoveAt(i);
				}
			}
			if (robotPosition.X < -128f && !Game1.globalFade)
			{
				Game1.globalFadeToBlack(afterFade, 0.006f);
			}
			return false;
		}

		public void afterFade()
		{
			Game1.currentMinigame = null;
			Game1.globalFadeToClear();
			if (Game1.currentLocation.currentEvent != null)
			{
				Game1.currentLocation.currentEvent.CurrentCommand++;
				Game1.currentLocation.temporarySprites.Clear();
			}
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void releaseLeftClick(int x, int y)
		{
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void receiveKeyPress(Keys k)
		{
			if (k == Keys.Escape)
			{
				robotPosition.X = -1000f;
				tempSprites.Clear();
			}
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			b.Draw(Game1.mouseCursors, new Rectangle(0, backgroundPosition, Game1.graphics.GraphicsDevice.Viewport.Width, 2560), new Rectangle(264, 1858, 1, 84), Color.White);
			b.Draw(Game1.mouseCursors, new Vector2(0f, backgroundPosition), new Rectangle(0, 1454, 639, 188), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, backgroundPosition - 752), new Rectangle(0, 1454, 639, 188), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, backgroundPosition - 1504), new Rectangle(0, 1454, 639, 188), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, backgroundPosition - 2256), new Rectangle(0, 1454, 639, 188), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, robotPosition, new Rectangle(222 + millisecondsSinceStart / 50 % 2 * 20, 1890, 20, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
			{
				tempSprite.draw(b, localPosition: true);
			}
			b.End();
		}

		public void changeScreenSize()
		{
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			backgroundPosition = 2560 - (int)((float)Game1.graphics.GraphicsDevice.Viewport.Height * pixel_zoom_adjustment);
			robotPosition = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height) * pixel_zoom_adjustment;
		}

		public void unload()
		{
		}

		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		public string minigameId()
		{
			return null;
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public bool forceQuit()
		{
			return false;
		}
	}
}
