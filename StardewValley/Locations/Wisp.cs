using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Locations
{
	public class Wisp
	{
		public Vector2 position;

		public Vector2 drawPosition;

		public Vector2[] oldPositions = new Vector2[16];

		public int oldPositionIndex;

		public int index;

		public int tailUpdateTimer;

		public float rotationSpeed;

		public float rotationOffset;

		public float rotationRadius = 16f;

		public float age;

		public float lifeTime = 1f;

		public Color baseColor;

		public Wisp(int index)
		{
			Reinitialize();
		}

		public virtual void Reinitialize()
		{
			baseColor = Color.White * Utility.RandomFloat(0.25f, 0.75f);
			rotationOffset = Utility.RandomFloat(0f, 360f);
			rotationSpeed = Utility.RandomFloat(0.5f, 2f);
			rotationRadius = Utility.RandomFloat(8f, 32f);
			lifeTime = Utility.RandomFloat(6f, 12f);
			age = 0f;
			position = new Vector2(Game1.random.Next(0, Game1.currentLocation.map.DisplayWidth), Game1.random.Next(0, Game1.currentLocation.map.DisplayHeight));
			drawPosition = Vector2.Zero;
			for (int i = 0; i < oldPositions.Length; i++)
			{
				oldPositions[i] = Vector2.Zero;
			}
		}

		public virtual void Update(GameTime time)
		{
			age += (float)time.ElapsedGameTime.TotalSeconds;
			position.X -= Math.Max(0.4f, Math.Min(1f, (float)index * 0.01f)) - (float)((double)((float)index * 0.01f) * Math.Sin(Math.PI * 2.0 * (double)time.TotalGameTime.Milliseconds / 8000.0));
			position.Y += Math.Max(0.5f, Math.Min(1.2f, (float)index * 0.02f));
			if (age >= lifeTime)
			{
				Reinitialize();
			}
			else if (position.Y > (float)Game1.currentLocation.map.DisplayHeight)
			{
				Reinitialize();
			}
			else if (position.X < 0f)
			{
				Reinitialize();
			}
			drawPosition = position + new Vector2((float)Math.Sin(age * rotationSpeed + rotationOffset), (float)Math.Sin(age * rotationSpeed + rotationOffset)) * rotationRadius;
			tailUpdateTimer--;
			if (tailUpdateTimer <= 0)
			{
				tailUpdateTimer = 6;
				oldPositionIndex = (oldPositionIndex + 1) % oldPositions.Length;
				oldPositions[oldPositionIndex] = drawPosition;
			}
		}

		public virtual void Draw(SpriteBatch b)
		{
			Color draw_color = baseColor;
			draw_color *= Utility.Lerp(0f, 1f, (float)Math.Sin((double)(age / lifeTime) * Math.PI));
			float rotation = age * rotationSpeed * 2f + rotationOffset * (float)index;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, drawPosition), new Rectangle(346 + (int)(age / 0.25f + rotationOffset) % 4 * 5, 1971, 5, 5), draw_color, rotation, new Vector2(2.5f, 2.5f), 4f, SpriteEffects.None, 1f);
			int tail_index = oldPositionIndex;
			for (int i = 0; i < oldPositions.Length; i++)
			{
				tail_index++;
				if (tail_index >= oldPositions.Length)
				{
					tail_index = 0;
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, oldPositions[tail_index]), new Rectangle(356, 1971, 5, 5), draw_color * ((float)i / (float)oldPositions.Length), rotation - (float)i, new Vector2(2.5f, 2.5f), 2f, SpriteEffects.None, 1f);
			}
		}
	}
}
