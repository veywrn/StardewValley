using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Projectiles;
using System;

namespace StardewValley.Monsters
{
	public class SquidKid : Monster
	{
		private float lastFireball;

		private new int yOffset;

		private readonly NetEvent0 fireballEvent = new NetEvent0();

		private readonly NetEvent0 hurtAnimationEvent = new NetEvent0();

		public SquidKid()
		{
		}

		public SquidKid(Vector2 position)
			: base("Squid Kid", position)
		{
			Sprite.SpriteHeight = 16;
			base.IsWalkingTowardPlayer = false;
			Sprite.UpdateSourceRect();
			base.HideShadow = true;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(fireballEvent, hurtAnimationEvent);
			fireballEvent.onEvent += delegate
			{
				if (!Game1.IsMasterGame)
				{
					fireballFired();
				}
			};
			hurtAnimationEvent.onEvent += delegate
			{
				Sprite.currentFrame = Sprite.currentFrame - Sprite.currentFrame % 4 + 3;
			};
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Squid Kid");
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
				setTrajectory(xTrajectory, yTrajectory);
				base.currentLocation.playSound("hitEnemy");
				hurtAnimationEvent.Fire();
				if (base.Health <= 0)
				{
					deathAnimation();
				}
			}
			return actualDamage;
		}

		protected override void sharedDeathAnimation()
		{
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
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + yOffset), Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)yOffset / 20f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f);
		}

		protected override void updateAnimation(GameTime time)
		{
			base.updateAnimation(time);
			yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 2000f) * (Math.PI * 2.0)) * 15.0);
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

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (isMoving())
			{
				if (base.FacingDirection == 0)
				{
					Sprite.AnimateUp(time);
				}
				else if (base.FacingDirection == 3)
				{
					Sprite.AnimateLeft(time);
				}
				else if (base.FacingDirection == 1)
				{
					Sprite.AnimateRight(time);
				}
				else if (base.FacingDirection == 2)
				{
					Sprite.AnimateDown(time);
				}
			}
			faceGeneralDirection(base.Player.Position);
		}

		private Vector2 fireballFired()
		{
			switch (base.FacingDirection)
			{
			case 0:
				Sprite.currentFrame = 3;
				return Vector2.Zero;
			case 1:
				Sprite.currentFrame = 7;
				return new Vector2(64f, 0f);
			case 2:
				Sprite.currentFrame = 11;
				return new Vector2(0f, 32f);
			case 3:
				Sprite.currentFrame = 15;
				return new Vector2(-32f, 0f);
			default:
				return Vector2.Zero;
			}
		}

		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			fireballEvent.Poll();
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			faceGeneralDirection(base.Player.Position);
			lastFireball = Math.Max(0f, lastFireball - (float)time.ElapsedGameTime.Milliseconds);
			if (withinPlayerThreshold() && lastFireball == 0f && Game1.random.NextDouble() < 0.01)
			{
				base.IsWalkingTowardPlayer = false;
				Vector2 value = new Vector2(base.Position.X, base.Position.Y + 64f);
				Halt();
				fireballEvent.Fire();
				_ = value + fireballFired();
				Sprite.UpdateSourceRect();
				Vector2 trajectory = Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(getStandingPosition()), 8f, base.Player);
				base.currentLocation.projectiles.Add(new BasicProjectile(15, 10, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", explode: true, damagesMonsters: false, base.currentLocation, this));
				base.currentLocation.playSound("fireball");
				lastFireball = Game1.random.Next(1200, 3500);
			}
			else if (lastFireball != 0f && Game1.random.NextDouble() < 0.02)
			{
				Halt();
				if (withinPlayerThreshold())
				{
					base.Slipperiness = 8;
					setTrajectory((int)Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(getStandingPosition()), 8f, base.Player).X, (int)(0f - Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(getStandingPosition()), 8f, base.Player).Y));
				}
			}
		}
	}
}
