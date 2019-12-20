using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley.Monsters
{
	public class Grub : Monster
	{
		public const int healthToRunAway = 8;

		private readonly NetBool leftDrift = new NetBool();

		private readonly NetBool pupating = new NetBool();

		[XmlElement("hard")]
		public readonly NetBool hard = new NetBool();

		private int metamorphCounter = 2000;

		private readonly NetFloat targetRotation = new NetFloat();

		public Grub()
		{
		}

		public Grub(Vector2 position)
			: this(position, hard: false)
		{
		}

		public Grub(Vector2 position, bool hard)
			: base("Grub", position)
		{
			if (Game1.random.NextDouble() < 0.5)
			{
				leftDrift.Value = true;
			}
			base.FacingDirection = Game1.random.Next(4);
			targetRotation.Value = (rotation = (float)Game1.random.Next(4) / (float)Math.PI);
			this.hard.Value = hard;
			if (hard)
			{
				base.DamageToFarmer *= 3;
				base.Health *= 5;
				base.MaxHealth = base.Health;
				base.ExperienceGained *= 3;
				if (Game1.random.NextDouble() < 0.1)
				{
					objectsToDrop.Add(456);
				}
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(leftDrift, pupating, hard, targetRotation);
			position.Field.AxisAlignedMovement = true;
		}

		public override void reloadSprite()
		{
			base.reloadSprite();
			Sprite.SpriteHeight = 24;
			Sprite.UpdateSourceRect();
		}

		public void setHard()
		{
			hard.Value = true;
			if ((bool)hard)
			{
				base.DamageToFarmer = 12;
				base.Health = 100;
				base.MaxHealth = base.Health;
				base.ExperienceGained = 10;
				if (Game1.random.NextDouble() < 0.1)
				{
					objectsToDrop.Add(456);
				}
			}
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
				base.currentLocation.playSound("slimeHit");
				if ((bool)pupating)
				{
					base.currentLocation.playSound("crafting");
					setTrajectory(xTrajectory / 2, yTrajectory / 2);
					return 0;
				}
				base.Slipperiness = 4;
				base.Health -= actualDamage;
				setTrajectory(xTrajectory, yTrajectory);
				if (base.Health <= 0)
				{
					base.currentLocation.playSound("slimedead");
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.Orange, 10)
					{
						holdLastFrame = true,
						alphaFade = 0.01f,
						interval = 50f
					}, base.currentLocation);
				}
			}
			return actualDamage;
		}

		public override void defaultMovementBehavior(GameTime time)
		{
			base.Scale = 1f + (float)(0.125 * Math.Sin(time.TotalGameTime.TotalMilliseconds / (double)(500f + base.Position.X / 100f)));
		}

		public override void update(GameTime time, GameLocation location)
		{
			if ((base.Health > 8 || ((bool)hard && base.Health >= base.MaxHealth)) && !pupating)
			{
				base.update(time, location);
				return;
			}
			if (invincibleCountdown > 0)
			{
				invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (invincibleCountdown <= 0)
				{
					stopGlowing();
				}
			}
			if (Game1.IsMasterGame)
			{
				behaviorAtGameTick(time);
			}
			updateAnimation(time);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Sprite.SpriteWidth * 4 / 2, GetBoundingBox().Height / 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, hard ? Color.Lime : Color.White, rotation, new Vector2(Sprite.SpriteWidth / 2, (float)Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, scale) * 4f, (flip || (Sprite.CurrentAnimation != null && Sprite.CurrentAnimation[Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
		}

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if ((bool)pupating)
			{
				base.Scale = 1f + (float)Math.Sin((float)time.TotalGameTime.Milliseconds * ((float)Math.PI / 8f)) / 12f;
				metamorphCounter -= time.ElapsedGameTime.Milliseconds;
			}
			else if (base.Health <= 8 || ((bool)hard && base.Health < base.MaxHealth))
			{
				metamorphCounter -= time.ElapsedGameTime.Milliseconds;
				if (metamorphCounter <= 0)
				{
					Sprite.Animate(time, 16, 4, 125f);
					if (Sprite.currentFrame == 19)
					{
						metamorphCounter = 4500;
					}
				}
			}
			else if (isMoving())
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
				rotation = 0f;
				base.Scale = 1f;
			}
			else if (!withinPlayerThreshold())
			{
				Halt();
				rotation = targetRotation;
			}
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if ((bool)pupating)
			{
				base.Scale = 1f + (float)Math.Sin((float)time.TotalGameTime.Milliseconds * ((float)Math.PI / 8f)) / 12f;
				metamorphCounter -= time.ElapsedGameTime.Milliseconds;
				if (metamorphCounter <= 0)
				{
					base.Health = -500;
					Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(208, 424, 32, 40), 4, getStandingX(), getStandingY(), 25, (int)getTileLocation().Y);
					Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(208, 424, 32, 40), 8, getStandingX(), getStandingY(), 15, (int)getTileLocation().Y);
					base.currentLocation.characters.Add(new Fly(base.Position, hard)
					{
						currentLocation = base.currentLocation
					});
				}
			}
			else if (base.Health <= 8 || ((bool)hard && base.Health < base.MaxHealth))
			{
				metamorphCounter -= time.ElapsedGameTime.Milliseconds;
				if (metamorphCounter <= 0)
				{
					Sprite.Animate(time, 16, 4, 125f);
					if (Sprite.currentFrame == 19)
					{
						pupating.Value = true;
						metamorphCounter = 4500;
					}
					return;
				}
				if (Math.Abs(base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y) > 128)
				{
					if (base.Player.GetBoundingBox().Center.X > GetBoundingBox().Center.X)
					{
						SetMovingLeft(b: true);
					}
					else
					{
						SetMovingRight(b: true);
					}
				}
				else if (Math.Abs(base.Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X) > 128)
				{
					if (base.Player.GetBoundingBox().Center.Y > GetBoundingBox().Center.Y)
					{
						SetMovingUp(b: true);
					}
					else
					{
						SetMovingDown(b: true);
					}
				}
				MovePosition(time, Game1.viewport, base.currentLocation);
			}
			else if (withinPlayerThreshold())
			{
				base.Scale = 1f;
				rotation = 0f;
			}
			else if (isMoving())
			{
				Halt();
				faceDirection(Game1.random.Next(4));
				targetRotation.Value = (rotation = (float)Game1.random.Next(4) / (float)Math.PI);
			}
		}
	}
}
