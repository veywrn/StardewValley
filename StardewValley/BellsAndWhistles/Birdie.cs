using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class Birdie : Critter
	{
		public const int brownBird = 25;

		public const int blueBird = 45;

		public const int flyingSpeed = 6;

		public const int walkingSpeed = 1;

		public const int pecking = 0;

		public const int flyingAway = 1;

		public const int sleeping = 2;

		public const int stopped = 3;

		public const int walking = 4;

		private int state;

		private float flightOffset;

		private int characterCheckTimer = 200;

		private int walkTimer;

		public Birdie(int tileX, int tileY, int startingIndex = 25)
			: base(startingIndex, new Vector2(tileX * 64, tileY * 64))
		{
			flip = (Game1.random.NextDouble() < 0.5);
			position.X += 32f;
			position.Y += 32f;
			startingPosition = position;
			flightOffset = (float)Game1.random.NextDouble() - 0.5f;
			state = 0;
		}

		public void hop(Farmer who)
		{
			gravityAffectedDY = -2f;
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			if (state == 1)
			{
				base.draw(b);
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (state != 1)
			{
				base.draw(b);
			}
		}

		private void donePecking(Farmer who)
		{
			state = ((!(Game1.random.NextDouble() < 0.5)) ? 3 : 0);
		}

		private void playFlap(Farmer who)
		{
			if (Utility.isOnScreen(position, 64))
			{
				Game1.playSound("batFlap");
			}
		}

		private void playPeck(Farmer who)
		{
			if (Utility.isOnScreen(position, 64))
			{
				Game1.playSound("shiny4");
			}
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			if (yJumpOffset < 0f && state != 1)
			{
				if (!flip && !environment.isCollidingPosition(getBoundingBox(-2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
				{
					position.X -= 2f;
				}
				else if (!environment.isCollidingPosition(getBoundingBox(2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
				{
					position.X += 2f;
				}
			}
			characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (characterCheckTimer < 0)
			{
				Character f = Utility.isThereAFarmerOrCharacterWithinDistance(position / 64f, 4, environment);
				characterCheckTimer = 200;
				if (f != null && state != 1)
				{
					if (Game1.random.NextDouble() < 0.85)
					{
						Game1.playSound("SpringBirds");
					}
					state = 1;
					if (f.Position.X > position.X)
					{
						flip = false;
					}
					else
					{
						flip = true;
					}
					sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((short)(baseFrame + 6), 70),
						new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 60, secondaryArm: false, flip, playFlap),
						new FarmerSprite.AnimationFrame((short)(baseFrame + 8), 70),
						new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 60)
					});
					sprite.loop = true;
				}
			}
			switch (state)
			{
			case 0:
				if (sprite.CurrentAnimation == null)
				{
					List<FarmerSprite.AnimationFrame> peckAnim = new List<FarmerSprite.AnimationFrame>();
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 2), 480));
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 3), 170, secondaryArm: false, flip));
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 4), 170, secondaryArm: false, flip));
					int pecks = Game1.random.Next(1, 5);
					for (int i = 0; i < pecks; i++)
					{
						peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 3), 70));
						peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 4), 100, secondaryArm: false, flip, playPeck));
					}
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 3), 100));
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 2), 70, secondaryArm: false, flip));
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 1), 70, secondaryArm: false, flip));
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)baseFrame, 500, secondaryArm: false, flip, donePecking));
					sprite.loop = false;
					sprite.setCurrentAnimation(peckAnim);
				}
				break;
			case 1:
				if (!flip)
				{
					position.X -= 6f;
				}
				else
				{
					position.X += 6f;
				}
				yOffset -= 2f + flightOffset;
				break;
			case 2:
				if (sprite.CurrentAnimation == null)
				{
					sprite.currentFrame = baseFrame + 5;
				}
				if (Game1.random.NextDouble() < 0.003 && sprite.CurrentAnimation == null)
				{
					state = 3;
				}
				break;
			case 4:
				if (!flip && !environment.isCollidingPosition(getBoundingBox(-1, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
				{
					position.X -= 1f;
				}
				else if (flip && !environment.isCollidingPosition(getBoundingBox(1, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
				{
					position.X += 1f;
				}
				walkTimer -= time.ElapsedGameTime.Milliseconds;
				if (walkTimer < 0)
				{
					state = 3;
					sprite.loop = false;
					sprite.CurrentAnimation = null;
					sprite.currentFrame = baseFrame;
				}
				break;
			case 3:
				if (Game1.random.NextDouble() < 0.008 && sprite.CurrentAnimation == null && yJumpOffset >= 0f)
				{
					switch (Game1.random.Next(6))
					{
					case 0:
						state = 2;
						break;
					case 1:
						state = 0;
						break;
					case 2:
						hop(null);
						break;
					case 3:
						flip = !flip;
						hop(null);
						break;
					case 4:
					case 5:
						state = 4;
						sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((short)baseFrame, 100),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 1), 100)
						});
						sprite.loop = true;
						if (position.X >= startingPosition.X)
						{
							flip = false;
						}
						else
						{
							flip = true;
						}
						walkTimer = Game1.random.Next(5, 15) * 100;
						break;
					}
				}
				else if (sprite.CurrentAnimation == null)
				{
					sprite.currentFrame = baseFrame;
				}
				break;
			}
			return base.update(time, environment);
		}
	}
}
