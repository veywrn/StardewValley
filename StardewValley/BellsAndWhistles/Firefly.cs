using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles
{
	public class Firefly : Critter
	{
		private bool glowing;

		private int glowTimer;

		private int id;

		private Vector2 motion;

		private LightSource light;

		public Firefly()
		{
		}

		public Firefly(Vector2 position)
		{
			baseFrame = -1;
			base.position = position * 64f;
			startingPosition = position * 64f;
			motion = new Vector2((float)Game1.random.Next(-10, 11) * 0.1f, (float)Game1.random.Next(-10, 11) * 0.1f);
			id = (int)(position.X * 10099f + position.Y * 77f + (float)Game1.random.Next(99999));
			light = new LightSource(4, position, (float)Game1.random.Next(4, 6) * 0.1f, Color.Purple * 0.8f, id, LightSource.LightContext.None, 0L);
			glowing = true;
			Game1.currentLightSources.Add(light);
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			position += motion;
			motion.X += (float)Game1.random.Next(-1, 2) * 0.1f;
			motion.Y += (float)Game1.random.Next(-1, 2) * 0.1f;
			if (motion.X < -1f)
			{
				motion.X = -1f;
			}
			if (motion.X > 1f)
			{
				motion.X = 1f;
			}
			if (motion.Y < -1f)
			{
				motion.Y = -1f;
			}
			if (motion.Y > 1f)
			{
				motion.Y = 1f;
			}
			if (glowing)
			{
				light.position.Value = position;
			}
			if (position.X < -128f || position.Y < -128f || position.X > (float)environment.map.DisplayWidth || position.Y > (float)environment.map.DisplayHeight)
			{
				return true;
			}
			return false;
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, Game1.GlobalToLocal(position), Game1.staminaRect.Bounds, glowing ? Color.White : Color.Brown, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}
	}
}
