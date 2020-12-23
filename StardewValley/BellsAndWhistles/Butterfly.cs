using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class Butterfly : Critter
	{
		public const float maxSpeed = 3f;

		private int flapTimer;

		private int checkForLandingSpotTimer;

		private int landedTimer;

		private int flapSpeed = 50;

		private Vector2 motion;

		private float motionMultiplier = 1f;

		private bool summerButterfly;

		private bool islandButterfly;

		public bool stayInbounds;

		public Butterfly(Vector2 position, bool islandButterfly = false)
		{
			base.position = position * 64f;
			startingPosition = base.position;
			if (Game1.currentSeason.Equals("spring"))
			{
				baseFrame = ((Game1.random.NextDouble() < 0.5) ? (Game1.random.Next(3) * 3 + 160) : (Game1.random.Next(3) * 3 + 180));
			}
			else
			{
				baseFrame = ((Game1.random.NextDouble() < 0.5) ? (Game1.random.Next(3) * 4 + 128) : (Game1.random.Next(3) * 4 + 148));
				summerButterfly = true;
			}
			if (islandButterfly)
			{
				this.islandButterfly = islandButterfly;
				baseFrame = Game1.random.Next(4) * 4 + 364;
				summerButterfly = true;
			}
			motion = new Vector2((float)(Game1.random.NextDouble() + 0.25) * 3f * (float)((!(Game1.random.NextDouble() < 0.5)) ? 1 : (-1)) / 2f, (float)(Game1.random.NextDouble() + 0.5) * 3f * (float)((!(Game1.random.NextDouble() < 0.5)) ? 1 : (-1)) / 2f);
			flapSpeed = Game1.random.Next(45, 80);
			sprite = new AnimatedSprite(Critter.critterTexture, baseFrame, 16, 16);
			sprite.loop = false;
			startingPosition = position;
		}

		public void doneWithFlap(Farmer who)
		{
			flapTimer = 200 + Game1.random.Next(-5, 6);
		}

		public Butterfly setStayInbounds(bool stayInbounds)
		{
			this.stayInbounds = stayInbounds;
			return this;
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			flapTimer -= time.ElapsedGameTime.Milliseconds;
			if (flapTimer <= 0 && sprite.CurrentAnimation == null)
			{
				motionMultiplier = 1f;
				motion.X += (float)Game1.random.Next(-80, 81) / 100f;
				motion.Y = (float)(Game1.random.NextDouble() + 0.25) * -3f / 2f;
				if (Math.Abs(motion.X) > 1.5f)
				{
					motion.X = 3f * (float)Math.Sign(motion.X) / 2f;
				}
				if (Math.Abs(motion.Y) > 3f)
				{
					motion.Y = 3f * (float)Math.Sign(motion.Y);
				}
				if (stayInbounds)
				{
					if (position.X < 128f)
					{
						motion.X = 0.8f;
					}
					if (position.Y < 192f)
					{
						motion.Y /= 2f;
						flapTimer = 1000;
					}
					if (position.X > (float)(environment.map.DisplayWidth - 128))
					{
						motion.X = -0.8f;
					}
					if (position.Y > (float)(environment.map.DisplayHeight - 128))
					{
						motion.Y = -1f;
						flapTimer = 100;
					}
				}
				if (summerButterfly)
				{
					sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
						new FarmerSprite.AnimationFrame(baseFrame + 2, flapSpeed),
						new FarmerSprite.AnimationFrame(baseFrame + 3, flapSpeed),
						new FarmerSprite.AnimationFrame(baseFrame + 2, flapSpeed),
						new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
						new FarmerSprite.AnimationFrame(baseFrame, flapSpeed, secondaryArm: false, flip: false, doneWithFlap)
					});
				}
				else
				{
					sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
						new FarmerSprite.AnimationFrame(baseFrame + 2, flapSpeed),
						new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
						new FarmerSprite.AnimationFrame(baseFrame, flapSpeed, secondaryArm: false, flip: false, doneWithFlap)
					});
				}
			}
			position += motion * motionMultiplier;
			motion.Y += 0.005f * (float)time.ElapsedGameTime.Milliseconds;
			motionMultiplier -= 0.0005f * (float)time.ElapsedGameTime.Milliseconds;
			if (motionMultiplier <= 0f)
			{
				motionMultiplier = 0f;
			}
			return base.update(time, environment);
		}

		public override void draw(SpriteBatch b)
		{
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64f, -128f + yJumpOffset + yOffset)), position.Y / 10000f, 0, 0, Color.White, flip, 4f);
		}
	}
}
