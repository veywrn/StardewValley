using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class Crow : Critter
	{
		public const int flyingSpeed = 6;

		public const int pecking = 0;

		public const int flyingAway = 1;

		public const int sleeping = 2;

		public const int stopped = 3;

		private int state;

		public Crow(int tileX, int tileY)
			: base(14, new Vector2(tileX * 64, tileY * 64))
		{
			flip = (Game1.random.NextDouble() < 0.5);
			position.X += 32f;
			position.Y += 32f;
			startingPosition = position;
			state = 0;
		}

		public void hop(Farmer who)
		{
			gravityAffectedDY = -4f;
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
			Farmer f = Utility.isThereAFarmerWithinDistance(position / 64f, 4, environment);
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
			if (f != null && state != 1)
			{
				if (Game1.random.NextDouble() < 0.85)
				{
					Game1.playSound("crow");
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
					new FarmerSprite.AnimationFrame((short)(baseFrame + 6), 40),
					new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 40),
					new FarmerSprite.AnimationFrame((short)(baseFrame + 8), 40),
					new FarmerSprite.AnimationFrame((short)(baseFrame + 9), 40),
					new FarmerSprite.AnimationFrame((short)(baseFrame + 10), 40, secondaryArm: false, flip, playFlap),
					new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 40),
					new FarmerSprite.AnimationFrame((short)(baseFrame + 9), 40),
					new FarmerSprite.AnimationFrame((short)(baseFrame + 8), 40),
					new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 40)
				});
				sprite.loop = true;
			}
			switch (state)
			{
			case 0:
				if (sprite.CurrentAnimation == null)
				{
					List<FarmerSprite.AnimationFrame> peckAnim = new List<FarmerSprite.AnimationFrame>();
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)baseFrame, 480));
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 1), 170, secondaryArm: false, flip));
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 2), 170, secondaryArm: false, flip));
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
				yOffset -= 2f;
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
			case 3:
				if (Game1.random.NextDouble() < 0.008 && sprite.CurrentAnimation == null && yJumpOffset >= 0f)
				{
					switch (Game1.random.Next(5))
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
						state = 1;
						sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame((short)(baseFrame + 6), 50),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 50),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 8), 50),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 9), 50),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 10), 50, secondaryArm: false, flip, playFlap),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 50),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 9), 50),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 8), 50),
							new FarmerSprite.AnimationFrame((short)(baseFrame + 7), 50)
						});
						sprite.loop = true;
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
