using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Monsters
{
	public class DwarvishSentry : Monster
	{
		private float lastFireball;

		private new int yOffset;

		private float pauseTimer;

		public DwarvishSentry()
		{
		}

		public DwarvishSentry(Vector2 position)
			: base("Dwarvish Sentry", position)
		{
			Sprite.SpriteHeight = 16;
			base.IsWalkingTowardPlayer = false;
			Sprite.UpdateSourceRect();
			base.HideShadow = true;
			isGlider.Value = true;
			base.Slipperiness = 1;
			pauseTimer = 10000f;
			DelayedAction.playSoundAfterDelay("DwarvishSentry", 500);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Dwarvish Sentry");
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
				base.currentLocation?.playSound("clank");
				if (base.Health <= 0)
				{
					deathAnimation();
				}
			}
			return actualDamage;
		}

		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Sprite.textureName, new Rectangle(0, 64, 16, 16), 70f, 7, 0, base.Position + new Vector2(0f, -32f), flicker: false, flipped: false)
			{
				scale = 4f
			});
			base.currentLocation.localSound("fireball");
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
			{
				delayBeforeAnimationStart = 100
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
			{
				delayBeforeAnimationStart = 200
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
			{
				delayBeforeAnimationStart = 300
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
			{
				delayBeforeAnimationStart = 400
			});
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, getLocalPosition(Game1.viewport) + new Vector2(50f, 80 + yOffset), new Rectangle(536 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 350.0 / 70.0) * 8, 1945, 8, 8), Color.White * 0.75f, 0f, new Vector2(8f, 16f), 4f, SpriteEffects.FlipVertically, 0.99f - position.X / 10000f);
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + yOffset), Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f - position.X / 10000f);
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)yOffset / 20f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f);
		}

		protected override void updateAnimation(GameTime time)
		{
			base.updateAnimation(time);
			yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 2000f) * (Math.PI * 2.0)) * 7.0);
			if (Sprite.currentFrame % 4 != 0 && Game1.random.NextDouble() < 0.1)
			{
				Sprite.currentFrame -= Sprite.currentFrame % 4;
			}
			if (Game1.random.NextDouble() < 0.01)
			{
				Sprite.currentFrame++;
			}
			resetAnimationSpeed();
		}

		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			faceGeneralDirection(base.Player.Position);
			pauseTimer += (int)time.ElapsedGameTime.TotalMilliseconds;
			if (pauseTimer < 10000f)
			{
				setTrajectory(Vector2.Normalize(base.Player.Position - base.Position) * new Vector2(1f, -1f));
			}
			else if (Game1.random.NextDouble() < 0.01)
			{
				pauseTimer = Game1.random.Next(5000);
			}
		}
	}
}
