using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace StardewValley.Minigames
{
	public class HaleyCowPictures : IMinigame
	{
		private const int pictureWidth = 416;

		private const int pictureHeight = 496;

		private const int sourceWidth = 104;

		private const int sourceHeight = 124;

		private int numberOfPhotosSoFar;

		private int betweenPhotoTimer = 1000;

		private LocalizedContentManager content;

		private Vector2 centerOfScreen;

		private Texture2D pictures;

		private float fadeAlpha;

		public HaleyCowPictures()
		{
			content = Game1.content.CreateTemporary();
			pictures = (Game1.currentSeason.Equals("winter") ? content.Load<Texture2D>("LooseSprites\\cowPhotosWinter") : content.Load<Texture2D>("LooseSprites\\cowPhotos"));
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			centerOfScreen = new Vector2(Game1.game1.localMultiplayerWindow.Width / 2, Game1.game1.localMultiplayerWindow.Height / 2) * pixel_zoom_adjustment;
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool tick(GameTime time)
		{
			betweenPhotoTimer -= time.ElapsedGameTime.Milliseconds;
			if (betweenPhotoTimer <= 0)
			{
				betweenPhotoTimer = 5000;
				numberOfPhotosSoFar++;
				if (numberOfPhotosSoFar < 5)
				{
					Game1.playSound("cameraNoise");
				}
				if (numberOfPhotosSoFar >= 6)
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
					return true;
				}
			}
			if (numberOfPhotosSoFar >= 5)
			{
				fadeAlpha = Math.Min(1f, fadeAlpha += 0.007f);
			}
			if (numberOfPhotosSoFar > 0)
			{
				Game1.player.blinkTimer = 0;
				Game1.player.currentEyes = 0;
			}
			return false;
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
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, null, null);
			if (numberOfPhotosSoFar > 0)
			{
				b.Draw(pictures, centerOfScreen + new Vector2(-208f, -248f), new Rectangle(0, 0, 104, 124), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				Game1.player.faceDirection(2);
				Game1.player.FarmerRenderer.draw(b, Game1.player, 0, centerOfScreen + new Vector2(-208f, -248f) + new Vector2(70f, 66f) * 4f, 0.01f);
				b.Draw(Game1.shadowTexture, centerOfScreen + new Vector2(-208f, -248f) + new Vector2(70f, 66f) * 4f + new Vector2(32f, 120f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.005f);
			}
			if (numberOfPhotosSoFar > 1)
			{
				Game1.player.faceDirection(3);
				b.Draw(pictures, centerOfScreen + new Vector2(-208f, -248f) + new Vector2(16f, 16f), new Rectangle(104, 0, 104, 124), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				Game1.player.FarmerRenderer.draw(b, Game1.player, 6, centerOfScreen + new Vector2(-208f, -248f) + new Vector2(16f, 16f) + new Vector2(64f, 66f) * 4f, 0.11f, flip: true);
				b.Draw(Game1.shadowTexture, centerOfScreen + new Vector2(-208f, -248f) + new Vector2(16f, 16f) + new Vector2(64f, 66f) * 4f + new Vector2(32f, 120f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.105f);
			}
			if (numberOfPhotosSoFar > 2)
			{
				Game1.player.faceDirection(3);
				b.Draw(pictures, centerOfScreen + new Vector2(-208f, -248f) - new Vector2(24f, 8f), new Rectangle(0, 124, 104, 124), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
				Game1.player.FarmerRenderer.draw(b, Game1.player, 89, centerOfScreen + new Vector2(-208f, -248f) - new Vector2(24f, 8f) + new Vector2(55f, 66f) * 4f, 0.21f, flip: true);
				b.Draw(Game1.shadowTexture, centerOfScreen + new Vector2(-208f, -248f) - new Vector2(24f, 8f) + new Vector2(55f, 66f) * 4f + new Vector2(32f, 120f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.205f);
			}
			if (numberOfPhotosSoFar > 3)
			{
				Game1.player.faceDirection(2);
				b.Draw(pictures, centerOfScreen + new Vector2(-208f, -248f), new Rectangle(104, 124, 104, 124), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.3f);
				Game1.player.FarmerRenderer.draw(b, Game1.player, 94, centerOfScreen + new Vector2(-208f, -248f) + new Vector2(70f, 66f) * 4f, 0.31f);
				b.Draw(Game1.shadowTexture, centerOfScreen + new Vector2(-208f, -248f) + new Vector2(70f, 66f) * 4f + new Vector2(32f, 120f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.305f);
			}
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Game1.staminaRect.Bounds, Color.Black * fadeAlpha, 0f, Vector2.Zero, SpriteEffects.None, 1f);
			b.End();
		}

		public void changeScreenSize()
		{
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			centerOfScreen = new Vector2(Game1.game1.localMultiplayerWindow.Width / 2, Game1.game1.localMultiplayerWindow.Height / 2) * pixel_zoom_adjustment;
		}

		public void unload()
		{
			content.Unload();
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
