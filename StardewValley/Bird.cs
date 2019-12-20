using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.Network;
using System;

namespace StardewValley
{
	public class Bird
	{
		public enum BirdState
		{
			Idle,
			Flying
		}

		public Vector2 position;

		public Point startPosition;

		public Point endPosition;

		public float pathPosition;

		public float velocity;

		public int framesUntilNextMove;

		public BirdState birdState;

		public MovieTheater theater;

		public int peckFrames;

		public int nextPeck;

		public int peckDirection;

		public int birdType;

		public float flyArcHeight;

		public Bird()
		{
			position = new Vector2(0f, 0f);
			startPosition = new Point(0, 0);
			endPosition = new Point(0, 0);
			birdType = Game1.random.Next(0, 4);
		}

		public Bird(Point point, MovieTheater location, int bird_type = 0)
		{
			startPosition.X = (endPosition.X = point.X);
			startPosition.Y = (endPosition.Y = point.Y);
			position.X = ((float)startPosition.X + 0.5f) * 64f;
			position.Y = ((float)startPosition.Y + 0.5f) * 64f;
			theater = location;
			birdType = bird_type;
			framesUntilNextMove = Game1.random.Next(100, 300);
			peckDirection = Game1.random.Next(0, 2);
		}

		public void Draw(SpriteBatch b)
		{
			Vector2 offset_position = new Vector2(position.X, position.Y);
			offset_position.X += (float)Math.Sin((float)Game1.currentGameTime.TotalGameTime.Milliseconds * 0.0025f) * velocity * 2f;
			offset_position.Y += (float)Math.Sin((float)Game1.currentGameTime.TotalGameTime.Milliseconds * 0.006f) * velocity * 2f;
			offset_position.Y += (float)Math.Sin((double)pathPosition * Math.PI) * (0f - flyArcHeight);
			int frame = 0;
			SpriteEffects effect = SpriteEffects.None;
			if (birdState == BirdState.Idle)
			{
				if (peckDirection == 1)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
				frame = ((!theater.ShouldBirdsRoost()) ? ((peckFrames > 0) ? 1 : 0) : ((peckFrames <= 0) ? 8 : 9));
			}
			else
			{
				Vector2 offset = new Vector2(endPosition.X - startPosition.X, endPosition.Y - startPosition.Y);
				offset.Normalize();
				if (!(Math.Abs(offset.X) > Math.Abs(offset.Y)))
				{
					frame = ((!(offset.Y > 0f)) ? 6 : 4);
				}
				else
				{
					frame = 2;
					if (offset.X > 0f)
					{
						effect = SpriteEffects.FlipHorizontally;
					}
				}
				if (pathPosition > 0.95f)
				{
					frame += Game1.currentGameTime.TotalGameTime.Milliseconds / 50 % 2;
				}
				else if (!(pathPosition > 0.75f))
				{
					frame += Game1.currentGameTime.TotalGameTime.Milliseconds / 100 % 2;
				}
			}
			Rectangle source = new Rectangle(16 * frame, 16 * birdType, 16, 16);
			Rectangle draw_position = Game1.GlobalToLocal(Game1.viewport, new Rectangle((int)offset_position.X, (int)offset_position.Y, 64, 64));
			b.Draw(Game1.birdsSpriteSheet, draw_position, source, Color.White, 0f, new Vector2(8f, 14f), effect, 0.01f - Math.Min(0.01f, position.Y * 0.001f));
		}

		public void FlyToNewPoint()
		{
			Point point = theater.GetFreeBirdPoint(this, 500);
			if (point != default(Point))
			{
				theater.ReserveBirdPoint(this, point);
				startPosition = endPosition;
				endPosition = point;
				pathPosition = 0f;
				velocity = 0f;
				if (theater.ShouldBirdsRoost())
				{
					birdState = BirdState.Idle;
				}
				else
				{
					birdState = BirdState.Flying;
				}
				float tile_distance = Utility.distance(startPosition.X, endPosition.X, startPosition.Y, endPosition.Y);
				if (tile_distance >= 7f)
				{
					flyArcHeight = 200f;
				}
				else if (tile_distance >= 5f)
				{
					flyArcHeight = 150f;
				}
				else
				{
					flyArcHeight = 20f;
				}
			}
			else
			{
				framesUntilNextMove = Game1.random.Next(800, 1200);
			}
		}

		public void Update(GameTime time)
		{
			if (peckFrames > 0)
			{
				peckFrames--;
			}
			else
			{
				nextPeck--;
				if (nextPeck <= 0)
				{
					if (theater.ShouldBirdsRoost())
					{
						peckFrames = 50;
					}
					else
					{
						peckFrames = 5;
					}
					nextPeck = Game1.random.Next(10, 30);
					if (Game1.random.NextDouble() <= 0.75)
					{
						nextPeck += Game1.random.Next(50, 100);
						if (!theater.ShouldBirdsRoost())
						{
							peckDirection = Game1.random.Next(0, 2);
						}
					}
				}
			}
			if (birdState == BirdState.Idle)
			{
				if (!theater.ShouldBirdsRoost())
				{
					using (FarmerCollection.Enumerator enumerator = theater.farmers.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							Farmer farmer = enumerator.Current;
							float num = Utility.distance(farmer.position.X, position.X, farmer.position.Y, position.Y);
							framesUntilNextMove--;
							if (num < 200f || framesUntilNextMove <= 0)
							{
								FlyToNewPoint();
							}
						}
					}
				}
			}
			else
			{
				if (birdState != BirdState.Flying)
				{
					return;
				}
				float distance = Utility.distance((float)(endPosition.X * 64) + 32f, position.X, (float)(endPosition.Y * 64) + 32f, position.Y);
				int max_velocity = 5;
				float slow_down_multiplier = 0.25f;
				if (distance > (float)max_velocity / slow_down_multiplier)
				{
					velocity = Utility.MoveTowards(velocity, max_velocity, 0.5f);
				}
				else
				{
					velocity = Math.Max(Math.Min(distance * slow_down_multiplier, velocity), 1f);
				}
				float path_distance = Utility.distance((float)endPosition.X + 32f, (float)startPosition.X + 32f, (float)endPosition.Y + 32f, (float)startPosition.Y + 32f) * 64f;
				if (path_distance <= 0.0001f)
				{
					path_distance = 0.0001f;
				}
				float delta = velocity / path_distance;
				pathPosition += delta;
				position = new Vector2(Utility.Lerp((float)(startPosition.X * 64) + 32f, (float)(endPosition.X * 64) + 32f, pathPosition), Utility.Lerp((float)(startPosition.Y * 64) + 32f, (float)(endPosition.Y * 64) + 32f, pathPosition));
				if (pathPosition >= 1f)
				{
					position = new Vector2((float)(endPosition.X * 64) + 32f, (float)(endPosition.Y * 64) + 32f);
					birdState = BirdState.Idle;
					velocity = 0f;
					framesUntilNextMove = Game1.random.Next(350, 500);
					if (Game1.random.NextDouble() < 0.75)
					{
						framesUntilNextMove += Game1.random.Next(200, 300);
					}
				}
			}
		}
	}
}
