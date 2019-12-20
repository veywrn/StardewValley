using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class Frog : Critter
	{
		private bool waterLeaper;

		private bool leapingIntoWater;

		private bool splash;

		private int characterCheckTimer = 200;

		private int beforeFadeTimer;

		private float alpha = 1f;

		public Frog(Vector2 position, bool waterLeaper = false, bool forceFlip = false)
		{
			this.waterLeaper = waterLeaper;
			base.position = position * 64f;
			sprite = new AnimatedSprite(Critter.critterTexture, waterLeaper ? 300 : 280, 16, 16);
			sprite.loop = true;
			if (!flip && forceFlip)
			{
				flip = true;
			}
			if (waterLeaper)
			{
				sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(300, 600),
					new FarmerSprite.AnimationFrame(304, 100),
					new FarmerSprite.AnimationFrame(305, 100),
					new FarmerSprite.AnimationFrame(306, 300),
					new FarmerSprite.AnimationFrame(305, 100),
					new FarmerSprite.AnimationFrame(304, 100)
				});
			}
			else
			{
				sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(280, 60),
					new FarmerSprite.AnimationFrame(281, 70),
					new FarmerSprite.AnimationFrame(282, 140),
					new FarmerSprite.AnimationFrame(283, 90)
				});
				beforeFadeTimer = 1000;
				flip = (base.position.X + 4f < Game1.player.Position.X);
			}
			startingPosition = position;
		}

		public void startSplash(Farmer who)
		{
			splash = true;
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			if (waterLeaper)
			{
				if (!leapingIntoWater)
				{
					characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
					if (characterCheckTimer <= 0)
					{
						if (Utility.isThereAFarmerOrCharacterWithinDistance(position / 64f, 6, environment) != null)
						{
							leapingIntoWater = true;
							sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
							{
								new FarmerSprite.AnimationFrame(300, 100),
								new FarmerSprite.AnimationFrame(301, 100),
								new FarmerSprite.AnimationFrame(302, 100),
								new FarmerSprite.AnimationFrame(303, 1500, secondaryArm: false, flip: false, startSplash, behaviorAtEndOfFrame: true)
							});
							sprite.loop = false;
							sprite.oldFrame = 303;
							gravityAffectedDY = -6f;
						}
						else if (Game1.random.NextDouble() < 0.01)
						{
							Game1.playSound("croak");
						}
						characterCheckTimer = 200;
					}
				}
				else
				{
					position.X += (flip ? (-4) : 4);
					if (gravityAffectedDY >= 0f && yJumpOffset >= 0f)
					{
						sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(300, 100),
							new FarmerSprite.AnimationFrame(301, 100),
							new FarmerSprite.AnimationFrame(302, 100),
							new FarmerSprite.AnimationFrame(303, 1500, secondaryArm: false, flip: false, startSplash, behaviorAtEndOfFrame: true)
						});
						sprite.loop = false;
						sprite.oldFrame = 303;
						gravityAffectedDY = -6f;
						yJumpOffset = 0f;
						if (environment.doesTileHaveProperty((int)position.X / 64, (int)position.Y / 64, "Water", "Back") != null)
						{
							splash = true;
						}
					}
				}
			}
			else
			{
				position.X += (flip ? (-3) : 3);
				beforeFadeTimer -= time.ElapsedGameTime.Milliseconds;
				if (beforeFadeTimer <= 0)
				{
					alpha -= 0.001f * (float)time.ElapsedGameTime.Milliseconds;
					if (alpha <= 0f)
					{
						return true;
					}
				}
				if (environment.doesTileHaveProperty((int)position.X / 64, (int)position.Y / 64, "Water", "Back") != null)
				{
					splash = true;
				}
			}
			if (splash)
			{
				environment.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 50f, 2, 1, position, flicker: false, flipped: false));
				Game1.playSound("dropItemInWater");
				return true;
			}
			return base.update(time, environment);
		}

		public override void draw(SpriteBatch b)
		{
			sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(position + new Vector2(0f, -20f + yJumpOffset + yOffset))), (position.Y + 64f) / 10000f, 0, 0, Color.White * alpha, flip, 4f);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32f, 40f)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (yJumpOffset + yOffset) / 16f), SpriteEffects.None, (position.Y - 1f) / 10000f);
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
		}
	}
}
