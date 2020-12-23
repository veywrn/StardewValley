using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using xTile.Dimensions;

namespace StardewValley.BellsAndWhistles
{
	public class CrabCritter : Critter
	{
		public Microsoft.Xna.Framework.Rectangle movementRectangle;

		public float nextCharacterCheck = 2f;

		public float nextFrameChange;

		public float nextMovementChange;

		public bool moving;

		public bool diving;

		public bool skittering;

		protected float skitterTime = 5f;

		protected Microsoft.Xna.Framework.Rectangle _baseSourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 272, 18, 18);

		protected int _currentFrame;

		protected int _crabVariant;

		protected Vector2 movementDirection = Vector2.Zero;

		public Microsoft.Xna.Framework.Rectangle movementBounds;

		public CrabCritter()
		{
			sprite = new AnimatedSprite(Critter.critterTexture, 0, 18, 18);
			sprite.SourceRect = _baseSourceRectangle;
			sprite.ignoreSourceRectUpdates = true;
			_crabVariant = 1;
			UpdateSpriteRectangle();
		}

		public CrabCritter(Vector2 start_position)
			: this()
		{
			position = start_position;
			float movement_rectangle_width = 256f;
			movementBounds = new Microsoft.Xna.Framework.Rectangle((int)(start_position.X - movement_rectangle_width / 2f), (int)start_position.Y, (int)movement_rectangle_width, 0);
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			nextFrameChange -= (float)time.ElapsedGameTime.TotalSeconds;
			if (skittering)
			{
				skitterTime -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (nextFrameChange <= 0f && (moving || skittering))
			{
				_currentFrame++;
				if (_currentFrame >= 4)
				{
					_currentFrame = 0;
				}
				if (skittering)
				{
					nextFrameChange = Utility.RandomFloat(0.025f, 0.05f);
				}
				else
				{
					nextFrameChange = Utility.RandomFloat(0.05f, 0.15f);
				}
			}
			if (skittering)
			{
				if (yJumpOffset >= 0f)
				{
					if (!diving)
					{
						if (Game1.random.Next(0, 4) == 0)
						{
							gravityAffectedDY = -4f;
						}
						else
						{
							gravityAffectedDY = -2f;
						}
					}
					else
					{
						if (environment.doesTileHaveProperty((int)position.X / 64, (int)position.Y / 64, "Water", "Back") != null)
						{
							environment.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 50f, 2, 1, position, flicker: false, flipped: false));
							Game1.playSound("dropItemInWater");
							return true;
						}
						gravityAffectedDY = -4f;
					}
				}
			}
			else
			{
				nextCharacterCheck -= (float)time.ElapsedGameTime.TotalSeconds;
				if (nextCharacterCheck <= 0f)
				{
					Character f = Utility.isThereAFarmerOrCharacterWithinDistance(position / 64f, 7, environment);
					if (f != null)
					{
						_crabVariant = 0;
						skittering = true;
						if (f.position.X > position.X)
						{
							movementDirection.X = -3f;
						}
						else
						{
							movementDirection.X = 3f;
						}
					}
					nextCharacterCheck = 0.25f;
				}
				if (!skittering)
				{
					if (moving && yJumpOffset >= 0f)
					{
						gravityAffectedDY = -1f;
					}
					nextMovementChange -= (float)time.ElapsedGameTime.TotalSeconds;
					if (nextMovementChange <= 0f)
					{
						moving = !moving;
						if (moving)
						{
							if (Game1.random.NextDouble() > 0.5)
							{
								movementDirection.X = 1f;
							}
							else
							{
								movementDirection.X = -1f;
							}
						}
						else
						{
							movementDirection = Vector2.Zero;
						}
						if (moving)
						{
							nextMovementChange = Utility.RandomFloat(0.15f, 0.5f);
						}
						else
						{
							nextMovementChange = Utility.RandomFloat(0.2f, 1f);
						}
					}
				}
			}
			position += movementDirection;
			if (!diving && !environment.isTilePassable(new Location((int)(position.X / 64f), (int)(position.Y / 64f)), Game1.viewport))
			{
				position -= movementDirection;
				movementDirection *= -1f;
			}
			if (!skittering)
			{
				if (position.X < (float)movementBounds.Left)
				{
					position.X = movementBounds.Left;
					movementDirection *= -1f;
				}
				if (position.X > (float)movementBounds.Right)
				{
					position.X = movementBounds.Right;
					movementDirection *= -1f;
				}
			}
			else if (!diving && environment.doesTileHaveProperty((int)(position.X / 64f + (float)Math.Sign(movementDirection.X) * 1f), (int)position.Y / 64, "Water", "Back") != null)
			{
				if (yJumpOffset >= 0f)
				{
					gravityAffectedDY = -7f;
				}
				diving = true;
			}
			UpdateSpriteRectangle();
			if (skitterTime <= 0f)
			{
				return true;
			}
			return base.update(time, environment);
		}

		public virtual void UpdateSpriteRectangle()
		{
			Microsoft.Xna.Framework.Rectangle source_rectangle = _baseSourceRectangle;
			source_rectangle.Y += _crabVariant * 18;
			int drawn_frame = _currentFrame;
			if (drawn_frame == 3)
			{
				drawn_frame = 1;
			}
			source_rectangle.X += drawn_frame * 18;
			sprite.SourceRect = source_rectangle;
		}

		public override void draw(SpriteBatch b)
		{
			float alpha = skitterTime;
			if (alpha > 1f)
			{
				alpha = 1f;
			}
			if (alpha < 0f)
			{
				alpha = 0f;
			}
			sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(position + new Vector2(0f, -20f + yJumpOffset + yOffset))), (position.Y + 64f - 32f) / 10000f, 0, 0, Color.White * alpha, flip, 4f);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32f, 40f)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (yJumpOffset + yOffset) / 16f), SpriteEffects.None, (position.Y - 1f) / 10000f);
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
		}
	}
}
