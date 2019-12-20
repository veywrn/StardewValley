using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class Seagull : Critter
	{
		public const int walkingSpeed = 2;

		public const int flyingSpeed = 4;

		public const int walking = 0;

		public const int flyingAway = 1;

		public const int flyingToLand = 4;

		public const int swimming = 2;

		public const int stopped = 3;

		private int state;

		private int characterCheckTimer = 200;

		private bool moveLeft;

		public Seagull(Vector2 position, int startingState)
			: base(0, position)
		{
			moveLeft = (Game1.random.NextDouble() < 0.5);
			startingPosition = position;
			state = startingState;
		}

		public void hop(Farmer who)
		{
			gravityAffectedDY = -4f;
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (characterCheckTimer < 0)
			{
				Character f = Utility.isThereAFarmerOrCharacterWithinDistance(position / 64f, 4, environment);
				characterCheckTimer = 200;
				if (f != null && state != 1)
				{
					if (Game1.random.NextDouble() < 0.25)
					{
						Game1.playSound("seagulls");
					}
					state = 1;
					if (f.Position.X > position.X)
					{
						moveLeft = true;
					}
					else
					{
						moveLeft = false;
					}
					sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((short)(baseFrame + 10), 80),
						new FarmerSprite.AnimationFrame((short)(baseFrame + 11), 80),
						new FarmerSprite.AnimationFrame((short)(baseFrame + 12), 80),
						new FarmerSprite.AnimationFrame((short)(baseFrame + 13), 100)
					});
					sprite.loop = true;
				}
			}
			switch (state)
			{
			case 0:
				if (moveLeft && !environment.isCollidingPosition(getBoundingBox(-2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
				{
					position.X -= 2f;
				}
				else if (!moveLeft && !environment.isCollidingPosition(getBoundingBox(2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
				{
					position.X += 2f;
				}
				if (Game1.random.NextDouble() < 0.005)
				{
					state = 3;
					sprite.loop = false;
					sprite.CurrentAnimation = null;
					sprite.currentFrame = 0;
				}
				break;
			case 2:
			{
				sprite.currentFrame = baseFrame + 9;
				float tmpY = yOffset;
				if ((time.TotalGameTime.TotalMilliseconds + (double)((int)position.X * 4)) % 2000.0 < 1000.0)
				{
					yOffset = 2f;
				}
				else
				{
					yOffset = 0f;
				}
				if (yOffset > tmpY)
				{
					environment.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, new Vector2(position.X - 32f, position.Y - 32f), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
				}
				break;
			}
			case 1:
				if (moveLeft)
				{
					position.X -= 4f;
				}
				else
				{
					position.X += 4f;
				}
				yOffset -= 2f;
				break;
			case 3:
				if (Game1.random.NextDouble() < 0.003 && sprite.CurrentAnimation == null)
				{
					sprite.loop = false;
					switch (Game1.random.Next(4))
					{
					case 0:
					{
						List<FarmerSprite.AnimationFrame> frames2 = new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((short)(baseFrame + 2), 100),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 3), 100),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 4), 200),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 5), 200)
						};
						int extra2 = Game1.random.Next(5);
						for (int i = 0; i < extra2; i++)
						{
							frames2.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 4), 200));
							frames2.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 5), 200));
						}
						sprite.setCurrentAnimation(frames2);
						break;
					}
					case 1:
						sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(6, (short)Game1.random.Next(500, 4000))
						});
						break;
					case 2:
					{
						List<FarmerSprite.AnimationFrame> frames2 = new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((short)(baseFrame + 6), 500),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 100, secondaryArm: false, flip: false, hop),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 8), 100)
						};
						int extra2 = Game1.random.Next(3);
						for (int j = 0; j < extra2; j++)
						{
							frames2.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 100));
							frames2.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 8), 100));
						}
						sprite.setCurrentAnimation(frames2);
						break;
					}
					case 3:
						state = 0;
						sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((short)baseFrame, 200),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 1), 200)
						});
						sprite.loop = true;
						moveLeft = (Game1.random.NextDouble() < 0.5);
						if (Game1.random.NextDouble() < 0.33)
						{
							if (position.X > startingPosition.X)
							{
								moveLeft = true;
							}
							else
							{
								moveLeft = false;
							}
						}
						break;
					}
				}
				else if (sprite.CurrentAnimation == null)
				{
					sprite.currentFrame = baseFrame;
				}
				break;
			}
			flip = !moveLeft;
			return base.update(time, environment);
		}
	}
}
