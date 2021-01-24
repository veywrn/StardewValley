using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using System.Collections.Generic;

namespace StardewValley.Minigames
{
	public class MaruComet : IMinigame
	{
		private const int telescopeCircleWidth = 143;

		private const int flybyRepeater = 200;

		private const float flybySpeed = 0.8f;

		private LocalizedContentManager content;

		private Vector2 centerOfScreen;

		private Vector2 cometColorOrigin;

		private Texture2D cometTexture;

		private List<Vector2> flybys = new List<Vector2>();

		private List<Vector2> flybysClose = new List<Vector2>();

		private List<Vector2> flybysFar = new List<Vector2>();

		private string currentString = "";

		private int zoom;

		private int flybyTimer;

		private int totalTimer;

		private int currentStringCharacter;

		private int characterAdvanceTimer;

		private float fade = 1f;

		public MaruComet()
		{
			zoom = 4;
			content = Game1.content.CreateTemporary();
			cometTexture = content.Load<Texture2D>("Minigames\\MaruComet");
			changeScreenSize();
		}

		public void changeScreenSize()
		{
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			centerOfScreen = pixel_zoom_adjustment * new Vector2(Game1.game1.localMultiplayerWindow.Width / 2, Game1.game1.localMultiplayerWindow.Height / 2);
			centerOfScreen.X = (int)centerOfScreen.X;
			centerOfScreen.Y = (int)centerOfScreen.Y;
			cometColorOrigin = centerOfScreen + pixel_zoom_adjustment * new Vector2(-71 * zoom, 71 * zoom);
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public bool tick(GameTime time)
		{
			flybyTimer -= time.ElapsedGameTime.Milliseconds;
			if (fade > 0f)
			{
				fade -= (float)time.ElapsedGameTime.Milliseconds * 0.001f;
			}
			if (flybyTimer <= 0)
			{
				flybyTimer = 200;
				bool bottom = Game1.random.NextDouble() < 0.5;
				flybys.Add(new Vector2(bottom ? Game1.random.Next(143 * zoom) : (-8 * zoom), bottom ? (8 * zoom) : (-Game1.random.Next(143 * zoom))));
				flybysClose.Add(new Vector2(bottom ? Game1.random.Next(143 * zoom) : (-8 * zoom), bottom ? (8 * zoom) : (-Game1.random.Next(143 * zoom))));
				flybysFar.Add(new Vector2(bottom ? Game1.random.Next(143 * zoom) : (-8 * zoom), bottom ? (8 * zoom) : (-Game1.random.Next(143 * zoom))));
			}
			for (int i = flybys.Count - 1; i >= 0; i--)
			{
				flybys[i] = new Vector2(flybys[i].X + 0.8f * (float)time.ElapsedGameTime.Milliseconds, flybys[i].Y - 0.8f * (float)time.ElapsedGameTime.Milliseconds);
				if (cometColorOrigin.Y + flybys[i].Y < centerOfScreen.Y - (float)(143 * zoom / 2))
				{
					flybys.RemoveAt(i);
				}
			}
			for (int j = flybysClose.Count - 1; j >= 0; j--)
			{
				flybysClose[j] = new Vector2(flybysClose[j].X + 0.8f * (float)time.ElapsedGameTime.Milliseconds * 1.5f, flybysClose[j].Y - 0.8f * (float)time.ElapsedGameTime.Milliseconds * 1.5f);
				if (cometColorOrigin.Y + flybysClose[j].Y < centerOfScreen.Y - (float)(143 * zoom / 2))
				{
					flybysClose.RemoveAt(j);
				}
			}
			for (int k = flybysFar.Count - 1; k >= 0; k--)
			{
				flybysFar[k] = new Vector2(flybysFar[k].X + 0.8f * (float)time.ElapsedGameTime.Milliseconds * 0.5f, flybysFar[k].Y - 0.8f * (float)time.ElapsedGameTime.Milliseconds * 0.5f);
				if (cometColorOrigin.Y + flybysFar[k].Y < centerOfScreen.Y - (float)(143 * zoom / 2))
				{
					flybysFar.RemoveAt(k);
				}
			}
			totalTimer += time.ElapsedGameTime.Milliseconds;
			if (totalTimer >= 28000)
			{
				if (!currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet5")))
				{
					currentStringCharacter = 0;
					currentString = Game1.content.LoadString("Strings\\Events:Maru_comet5");
				}
			}
			else if (totalTimer >= 25000)
			{
				if (!currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet4")))
				{
					currentStringCharacter = 0;
					currentString = Game1.content.LoadString("Strings\\Events:Maru_comet4");
				}
			}
			else if (totalTimer >= 20000)
			{
				if (!currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet3")))
				{
					currentStringCharacter = 0;
					currentString = Game1.content.LoadString("Strings\\Events:Maru_comet3");
				}
			}
			else if (totalTimer >= 16000)
			{
				if (!currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet2")))
				{
					currentStringCharacter = 0;
					currentString = Game1.content.LoadString("Strings\\Events:Maru_comet2");
				}
			}
			else if (totalTimer >= 10000 && !currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet1")))
			{
				currentStringCharacter = 0;
				currentString = Game1.content.LoadString("Strings\\Events:Maru_comet1");
			}
			characterAdvanceTimer += time.ElapsedGameTime.Milliseconds;
			if (characterAdvanceTimer > 30)
			{
				currentStringCharacter++;
				characterAdvanceTimer = 0;
			}
			if (totalTimer >= 35000)
			{
				fade += (float)time.ElapsedGameTime.Milliseconds * 0.002f;
				if (fade >= 1f)
				{
					if (Game1.currentLocation.currentEvent != null)
					{
						Game1.currentLocation.currentEvent.CurrentCommand++;
					}
					return true;
				}
			}
			return false;
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, null, null);
			b.Draw(cometTexture, cometColorOrigin + new Vector2((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0), -(int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0)), new Rectangle(247, 0, 265, 240), Color.White, 0f, new Vector2(265f, 0f), zoom, SpriteEffects.None, 0.1f);
			b.Draw(cometTexture, cometColorOrigin + new Vector2((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0) + 808, -(int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0) - 808), new Rectangle(247, 0, 265, 240), Color.White, 0f, new Vector2(265f, 0f), zoom, SpriteEffects.None, 0.1f);
			b.Draw(cometTexture, centerOfScreen + new Vector2(-71f, -71f) * zoom, new Rectangle((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 300.0 / 100.0) * 143, 240, 143, 143), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.2f);
			foreach (Vector2 v in flybys)
			{
				b.Draw(cometTexture, cometColorOrigin + v, new Rectangle(0, 0, 8, 8), Color.White * 0.4f, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.24f);
			}
			foreach (Vector2 v2 in flybysClose)
			{
				b.Draw(cometTexture, cometColorOrigin + v2, new Rectangle(0, 0, 8, 8), Color.White * 0.4f, 0f, Vector2.Zero, zoom + 1, SpriteEffects.None, 0.24f);
			}
			foreach (Vector2 v3 in flybysFar)
			{
				b.Draw(cometTexture, cometColorOrigin + v3, new Rectangle(0, 0, 8, 8), Color.White * 0.4f, 0f, Vector2.Zero, zoom - 1, SpriteEffects.None, 0.24f);
			}
			b.Draw(cometTexture, centerOfScreen + new Vector2(-71f, -71f) * zoom, new Rectangle(0, 97, 143, 143), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.3f);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, (int)centerOfScreen.X - 71 * zoom, Game1.graphics.GraphicsDevice.Viewport.Height), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, (int)centerOfScreen.Y - 71 * zoom), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
			b.Draw(Game1.staminaRect, new Rectangle((int)centerOfScreen.X + 71 * zoom, 0, Game1.graphics.GraphicsDevice.Viewport.Width - ((int)centerOfScreen.X + 71 * zoom), Game1.graphics.GraphicsDevice.Viewport.Height), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
			b.Draw(Game1.staminaRect, new Rectangle((int)centerOfScreen.X - 71 * zoom, (int)centerOfScreen.Y + 71 * zoom, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height - ((int)centerOfScreen.Y + 71 * zoom)), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
			float height = SpriteText.getHeightOfString(currentString, Game1.game1.localMultiplayerWindow.Width);
			float text_draw_y = (int)centerOfScreen.Y + 79 * zoom;
			if (text_draw_y + height > (float)Game1.viewport.Height)
			{
				text_draw_y += (float)Game1.viewport.Height - (text_draw_y + height);
			}
			SpriteText.drawStringHorizontallyCenteredAt(b, currentString, (int)centerOfScreen.X, (int)text_draw_y, currentStringCharacter, -1, 99999, 1f, 0.99f, junimoText: false, 3, Game1.game1.localMultiplayerWindow.Width);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Game1.staminaRect.Bounds, Color.Black * fade, 0f, Vector2.Zero, SpriteEffects.None, 1f);
			b.End();
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public string minigameId()
		{
			return null;
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public void receiveEventPoke(int data)
		{
		}

		public void receiveKeyPress(Keys k)
		{
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
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

		public void unload()
		{
			content.Unload();
		}

		public bool forceQuit()
		{
			return false;
		}
	}
}
