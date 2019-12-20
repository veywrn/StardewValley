using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Objects;
using System.Collections.Generic;

namespace StardewValley.Minigames
{
	public class Test : IMinigame
	{
		public List<Wallpaper> wallpaper = new List<Wallpaper>();

		public Test()
		{
			for (int i = 0; i < 40; i++)
			{
				wallpaper.Add(new Wallpaper(i, isFloor: true));
			}
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool tick(GameTime time)
		{
			return false;
		}

		public void afterFade()
		{
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Game1.currentMinigame = null;
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
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, 2000, 2000), Color.White);
			Vector2 draw = new Vector2(16f, 16f);
			for (int i = 0; i < wallpaper.Count; i++)
			{
				wallpaper[i].drawInMenu(b, draw, 1f);
				draw.X += 128f;
				if (draw.X >= (float)(Game1.graphics.GraphicsDevice.Viewport.Width - 128))
				{
					draw.X = 16f;
					draw.Y += 128f;
				}
			}
			b.End();
		}

		public void changeScreenSize()
		{
		}

		public void unload()
		{
		}

		public void receiveEventPoke(int data)
		{
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
			return true;
		}
	}
}
