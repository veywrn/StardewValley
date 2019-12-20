using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Network;
using System;

namespace StardewValley.Monsters
{
	public class Serpent : Monster
	{
		public const float rotationIncrement = (float)Math.PI / 64f;

		private int wasHitCounter;

		private float targetRotation;

		private bool turningRight;

		private readonly NetFarmerRef killer = new NetFarmerRef().Delayed(interpolationWait: false);

		public Serpent()
		{
		}

		public Serpent(Vector2 position)
			: base("Serpent", position)
		{
			base.Slipperiness = 24 + Game1.random.Next(10);
			Halt();
			base.IsWalkingTowardPlayer = false;
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			base.Scale = 0.75f;
			base.HideShadow = true;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(killer.NetFields);
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Serpent");
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			base.Scale = 0.75f;
			base.HideShadow = true;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - (int)resilience);
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				setTrajectory(xTrajectory / 3, yTrajectory / 3);
				wasHitCounter = 500;
				base.currentLocation.playSound("serpentHit");
				if (base.Health <= 0)
				{
					killer.Value = who;
					deathAnimation();
				}
			}
			base.addedSpeed = Game1.random.Next(-1, 1);
			return actualDamage;
		}

		protected override void sharedDeathAnimation()
		{
		}

		protected override void localDeathAnimation()
		{
			if (killer.Value != null)
			{
				Rectangle bb = GetBoundingBox();
				bb.Inflate(-bb.Width / 2 + 1, -bb.Height / 2 + 1);
				Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(bb.Center, 4f, killer.Value);
				int xTrajectory = -(int)velocityTowardPlayer.X;
				int yTrajectory = -(int)velocityTowardPlayer.Y;
				base.currentLocation.localSound("serpentDie");
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Sprite.textureName, new Rectangle(0, 64, 32, 32), 200f, 4, 0, base.Position, flicker: false, flipped: false, 0.9f, 0.001f, Color.White, 4f * (float)scale, 0.01f, rotation + (float)Math.PI, (float)((double)Game1.random.Next(3, 5) * Math.PI / 64.0))
				{
					motion = new Vector2(xTrajectory, yTrajectory),
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(-32f, 0f), Color.LightGreen * 0.9f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 50,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory),
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(32f, 0f), Color.LightGreen * 0.8f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 100,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.8f,
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(0f, -32f), Color.LightGreen * 0.7f, 10)
				{
					delayBeforeAnimationStart = 150,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.6f,
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center), Color.LightGreen * 0.6f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 200,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.4f,
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(0f, 32f), Color.LightGreen * 0.5f, 10)
				{
					delayBeforeAnimationStart = 250,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.2f,
					layerDepth = 1f
				});
			}
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (Utility.isOnScreen(base.Position, 128))
			{
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(64f, GetBoundingBox().Height), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(64f, GetBoundingBox().Height / 2), Sprite.SourceRect, Color.White, rotation, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f)));
				if (isGlowing)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(64f, GetBoundingBox().Height / 2), Sprite.SourceRect, glowingColor * glowingTransparency, rotation, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f + 0.0001f)));
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			return new Rectangle((int)base.Position.X + 8, (int)base.Position.Y, Sprite.SpriteWidth * 4 * 3 / 4, 96);
		}

		protected override void updateAnimation(GameTime time)
		{
			base.updateAnimation(time);
			if (wasHitCounter >= 0)
			{
				wasHitCounter -= time.ElapsedGameTime.Milliseconds;
			}
			Sprite.Animate(time, 0, 9, 40f);
			if (withinPlayerThreshold() && invincibleCountdown <= 0)
			{
				float xSlope3 = -(base.Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X);
				float ySlope3 = base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y;
				float t = Math.Max(1f, Math.Abs(xSlope3) + Math.Abs(ySlope3));
				if (t < 64f)
				{
					xVelocity = Math.Max(-7f, Math.Min(7f, xVelocity * 1.1f));
					yVelocity = Math.Max(-7f, Math.Min(7f, yVelocity * 1.1f));
				}
				xSlope3 /= t;
				ySlope3 /= t;
				if (wasHitCounter <= 0)
				{
					targetRotation = (float)Math.Atan2(0f - ySlope3, xSlope3) - (float)Math.PI / 2f;
					if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) > Math.PI * 7.0 / 8.0 && Game1.random.NextDouble() < 0.5)
					{
						turningRight = true;
					}
					else if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) < Math.PI / 8.0)
					{
						turningRight = false;
					}
					if (turningRight)
					{
						rotation -= (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
					}
					else
					{
						rotation += (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
					}
					rotation %= (float)Math.PI * 2f;
					wasHitCounter = 5 + Game1.random.Next(-1, 2);
				}
				float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f));
				xSlope3 = (float)Math.Cos((double)rotation + Math.PI / 2.0);
				ySlope3 = 0f - (float)Math.Sin((double)rotation + Math.PI / 2.0);
				xVelocity += (0f - xSlope3) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				yVelocity += (0f - ySlope3) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(xVelocity) > Math.Abs((0f - xSlope3) * 7f))
				{
					xVelocity -= (0f - xSlope3) * maxAccel / 6f;
				}
				if (Math.Abs(yVelocity) > Math.Abs((0f - ySlope3) * 7f))
				{
					yVelocity -= (0f - ySlope3) * maxAccel / 6f;
				}
			}
			resetAnimationSpeed();
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity))
			{
				base.Health = -500;
			}
			if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
			{
				base.Health = -500;
			}
			if (withinPlayerThreshold() && invincibleCountdown <= 0)
			{
				faceDirection(2);
			}
		}
	}
}
