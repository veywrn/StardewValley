using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.BellsAndWhistles
{
	public class Cloud : Critter
	{
		public const int width = 147;

		public const int height = 100;

		public int zoom = 5;

		private bool verticalFlip;

		private bool horizontalFlip;

		public Cloud()
		{
		}

		public Cloud(Vector2 position)
		{
			base.position = position * 64f;
			startingPosition = position;
			verticalFlip = (Game1.random.NextDouble() < 0.5);
			horizontalFlip = (Game1.random.NextDouble() < 0.5);
			zoom = Game1.random.Next(4, 7);
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			position.Y -= (float)time.ElapsedGameTime.TotalMilliseconds * 0.02f;
			position.X -= (float)time.ElapsedGameTime.TotalMilliseconds * 0.02f;
			if (position.X < (float)(-147 * zoom) || position.Y < (float)(-100 * zoom))
			{
				return true;
			}
			return false;
		}

		public override Rectangle getBoundingBox(int xOffset, int yOffset)
		{
			return new Rectangle((int)position.X, (int)position.Y, 147 * zoom, 100 * zoom);
		}

		public override void draw(SpriteBatch b)
		{
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(position), new Rectangle(128, 0, 146, 99), Color.White, (verticalFlip && horizontalFlip) ? ((float)Math.PI) : 0f, Vector2.Zero, zoom, (verticalFlip && !horizontalFlip) ? SpriteEffects.FlipVertically : ((horizontalFlip && !verticalFlip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 1f);
		}
	}
}
