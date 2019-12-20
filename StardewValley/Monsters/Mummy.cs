using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Collections.Generic;

namespace StardewValley.Monsters
{
	public class Mummy : Monster
	{
		public NetInt reviveTimer = new NetInt(0);

		public const int revivalTime = 10000;

		protected int _damageToFarmer;

		private readonly NetEvent1Field<bool, NetBool> crumbleEvent = new NetEvent1Field<bool, NetBool>();

		public Mummy()
		{
		}

		public Mummy(Vector2 position)
			: base("Mummy", position)
		{
			Sprite.SpriteHeight = 32;
			Sprite.ignoreStopAnimation = true;
			Sprite.UpdateSourceRect();
			_damageToFarmer = damageToFarmer.Value;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(crumbleEvent, reviveTimer);
			crumbleEvent.onEvent += performCrumble;
			position.Field.AxisAlignedMovement = true;
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Mummy");
			Sprite.SpriteHeight = 32;
			Sprite.UpdateSourceRect();
			Sprite.ignoreStopAnimation = true;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - (int)resilience);
			if ((int)reviveTimer > 0)
			{
				if (isBomb)
				{
					base.Health = 0;
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.BlueViolet, 10)
					{
						holdLastFrame = true,
						alphaFade = 0.01f,
						interval = 70f
					}, base.currentLocation);
					base.currentLocation.playSound("ghost");
					return 999;
				}
				return -1;
			}
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Slipperiness = 2;
				base.Health -= actualDamage;
				setTrajectory(xTrajectory, yTrajectory);
				base.currentLocation.playSound("shadowHit");
				base.currentLocation.playSound("skeletonStep");
				base.IsWalkingTowardPlayer = true;
				if (base.Health <= 0)
				{
					reviveTimer.Value = 10000;
					base.Health = base.MaxHealth;
					deathAnimation();
				}
			}
			return actualDamage;
		}

		public override void defaultMovementBehavior(GameTime time)
		{
			if ((int)reviveTimer <= 0)
			{
				base.defaultMovementBehavior(time);
			}
		}

		protected override void sharedDeathAnimation()
		{
			Halt();
			crumble();
			collidesWithOtherCharacters.Value = false;
			base.IsWalkingTowardPlayer = false;
			moveTowardPlayerThreshold.Value = -1;
		}

		protected override void localDeathAnimation()
		{
		}

		public override void update(GameTime time, GameLocation location)
		{
			crumbleEvent.Poll();
			if ((int)reviveTimer > 0 && Sprite.CurrentAnimation == null && Sprite.currentFrame != 19)
			{
				Sprite.currentFrame = 19;
			}
			base.update(time, location);
		}

		private void crumble(bool reverse = false)
		{
			crumbleEvent.Fire(reverse);
		}

		private void performCrumble(bool reverse)
		{
			Sprite.setCurrentAnimation(getCrumbleAnimation(reverse));
			if (!reverse)
			{
				if (Game1.IsMasterGame)
				{
					damageToFarmer.Value = 0;
				}
				reviveTimer.Value = 10000;
				base.currentLocation.localSound("monsterdead");
			}
			else
			{
				if (Game1.IsMasterGame)
				{
					damageToFarmer.Value = _damageToFarmer;
				}
				reviveTimer.Value = 0;
				base.currentLocation.localSound("skeletonDie");
			}
		}

		private List<FarmerSprite.AnimationFrame> getCrumbleAnimation(bool reverse = false)
		{
			List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
			if (!reverse)
			{
				animation.Add(new FarmerSprite.AnimationFrame(16, 100, 0, secondaryArm: false, flip: false));
			}
			else
			{
				animation.Add(new FarmerSprite.AnimationFrame(16, 100, 0, secondaryArm: false, flip: false, behaviorAfterRevival, behaviorAtEndOfFrame: true));
			}
			animation.Add(new FarmerSprite.AnimationFrame(17, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(18, 100, 0, secondaryArm: false, flip: false));
			if (!reverse)
			{
				animation.Add(new FarmerSprite.AnimationFrame(19, 100, 0, secondaryArm: false, flip: false, behaviorAfterCrumble));
			}
			else
			{
				animation.Add(new FarmerSprite.AnimationFrame(19, 100, 0, secondaryArm: false, flip: false));
			}
			if (reverse)
			{
				animation.Reverse();
			}
			return animation;
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if ((int)reviveTimer <= 0 && withinPlayerThreshold())
			{
				base.IsWalkingTowardPlayer = true;
			}
			base.behaviorAtGameTick(time);
		}

		protected override void updateAnimation(GameTime time)
		{
			if (Sprite.CurrentAnimation != null)
			{
				if (Sprite.animateOnce(time))
				{
					Sprite.CurrentAnimation = null;
				}
			}
			else if ((int)reviveTimer > 0)
			{
				reviveTimer.Value -= time.ElapsedGameTime.Milliseconds;
				if ((int)reviveTimer < 2000)
				{
					shake(reviveTimer);
				}
				if ((int)reviveTimer <= 0)
				{
					if (Game1.IsMasterGame)
					{
						crumble(reverse: true);
						base.IsWalkingTowardPlayer = true;
					}
					else
					{
						reviveTimer.Value = 1;
					}
				}
			}
			else if (!Game1.IsMasterGame)
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
				else
				{
					Sprite.StopAnimation();
				}
			}
			resetAnimationSpeed();
		}

		private void behaviorAfterCrumble(Farmer who)
		{
			Halt();
			Sprite.currentFrame = 19;
			Sprite.CurrentAnimation = null;
		}

		private void behaviorAfterRevival(Farmer who)
		{
			base.IsWalkingTowardPlayer = true;
			collidesWithOtherCharacters.Value = true;
			Sprite.currentFrame = 0;
			Sprite.oldFrame = 0;
			moveTowardPlayerThreshold.Value = 8;
			Sprite.CurrentAnimation = null;
		}
	}
}
