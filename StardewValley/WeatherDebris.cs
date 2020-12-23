using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	[InstanceStatics]
	public class WeatherDebris
	{
		public const int pinkPetals = 0;

		public const int greenLeaves = 1;

		public const int fallLeaves = 2;

		public const int snow = 3;

		public const int animationInterval = 100;

		public const float gravity = -0.5f;

		public Vector2 position;

		public Rectangle sourceRect;

		public int which;

		public int animationIndex;

		public int animationTimer = 100;

		public int animationDirection = 1;

		public int animationIntervalOffset;

		public float dx;

		public float dy;

		public static float globalWind = -0.25f;

		private bool blowing;

		public WeatherDebris(Vector2 position, int which, float rotationVelocity, float dx, float dy)
		{
			this.position = position;
			this.which = which;
			this.dx = dx;
			this.dy = dy;
			switch (which)
			{
			case 1:
				sourceRect = new Rectangle(352, 1200, 16, 16);
				animationIntervalOffset = (Game1.random.Next(25) - 12) * 2;
				break;
			case 0:
				sourceRect = new Rectangle(352, 1184, 16, 16);
				animationIntervalOffset = (Game1.random.Next(25) - 12) * 2;
				break;
			case 2:
				sourceRect = new Rectangle(352, 1216, 16, 16);
				animationIntervalOffset = (Game1.random.Next(25) - 12) * 2;
				break;
			case 3:
				sourceRect = new Rectangle(391 + 4 * Game1.random.Next(5), 1236, 4, 4);
				break;
			}
		}

		public void update()
		{
			update(slow: false);
		}

		public void update(bool slow)
		{
			position.X += dx + (slow ? 0f : globalWind);
			position.Y += dy - (slow ? 0f : (-0.5f));
			if (dy < 0f && !blowing)
			{
				dy += 0.01f;
			}
			if (!Game1.fadeToBlack && Game1.fadeToBlackAlpha <= 0f)
			{
				if (position.X < -80f)
				{
					position.X = Game1.viewport.Width;
					position.Y = Game1.random.Next(0, Game1.viewport.Height - 64);
				}
				if (position.Y > (float)(Game1.viewport.Height + 16))
				{
					position.X = Game1.random.Next(0, Game1.viewport.Width);
					position.Y = -64f;
					dy = (float)Game1.random.Next(-15, 10) / ((!slow) ? 50f : ((Game1.random.NextDouble() < 0.1) ? 5f : 200f));
					dx = (float)Game1.random.Next(-10, 0) / (slow ? 200f : 50f);
				}
				else if (position.Y < -64f)
				{
					position.Y = Game1.viewport.Height;
					position.X = Game1.random.Next(0, Game1.viewport.Width);
				}
			}
			if (blowing)
			{
				dy -= 0.01f;
				if (Game1.random.NextDouble() < 0.006 || dy < -2f)
				{
					blowing = false;
				}
			}
			else if (!slow && Game1.random.NextDouble() < 0.001 && Game1.currentSeason != null && (Game1.currentSeason.Equals("spring") || Game1.currentSeason.Equals("summer")))
			{
				blowing = true;
			}
			int num = which;
			if ((uint)num > 3u)
			{
				return;
			}
			animationTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			if (animationTimer > 0)
			{
				return;
			}
			animationTimer = 100 + animationIntervalOffset;
			animationIndex += animationDirection;
			if (animationDirection == 0)
			{
				if (animationIndex >= 9)
				{
					animationDirection = -1;
				}
				else
				{
					animationDirection = 1;
				}
			}
			if (animationIndex > 10)
			{
				if (Game1.random.NextDouble() < 0.82)
				{
					animationIndex--;
					animationDirection = 0;
					dx += 0.1f;
					dy -= 0.2f;
				}
				else
				{
					animationIndex = 0;
				}
			}
			else if (animationIndex == 4 && animationDirection == -1)
			{
				animationIndex++;
				animationDirection = 0;
				dx -= 0.1f;
				dy -= 0.1f;
			}
			if (animationIndex == 7 && animationDirection == -1)
			{
				dy -= 0.2f;
			}
			if (which != 3)
			{
				sourceRect.X = 352 + animationIndex * 16;
			}
		}

		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, position, sourceRect, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1E-06f);
		}
	}
}
