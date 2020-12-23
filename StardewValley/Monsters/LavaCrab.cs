using Microsoft.Xna.Framework;
using System;

namespace StardewValley.Monsters
{
	public class LavaCrab : Monster
	{
		public LavaCrab()
		{
		}

		public LavaCrab(Vector2 position)
			: base("Lava Crab", position)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			position.Field.AxisAlignedMovement = true;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - (int)resilience);
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				actualDamage = -1;
			}
			else if (Sprite.currentFrame % 4 == 0)
			{
				actualDamage = 0;
				base.currentLocation.playSound("crafting");
			}
			else
			{
				base.Health -= actualDamage;
				base.currentLocation.playSound("hitEnemy");
				setTrajectory(xTrajectory, yTrajectory);
				if (base.Health <= 0)
				{
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position, Color.Purple, 10));
					base.currentLocation.playSound("monsterdead");
				}
			}
			return actualDamage;
		}

		protected override void updateAnimation(GameTime time)
		{
			if (isMoving() || withinPlayerThreshold())
			{
				if (FacingDirection == 0)
				{
					Sprite.AnimateUp(time);
				}
				else if (FacingDirection == 3)
				{
					Sprite.AnimateLeft(time);
				}
				else if (FacingDirection == 1)
				{
					Sprite.AnimateRight(time);
				}
				else if (FacingDirection == 2)
				{
					Sprite.AnimateDown(time);
				}
				if (Sprite.currentFrame % 4 == 0)
				{
					Sprite.currentFrame++;
					Sprite.UpdateSourceRect();
				}
			}
			else
			{
				Sprite.StopAnimation();
			}
			resetAnimationSpeed();
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (!withinPlayerThreshold())
			{
				Halt();
			}
		}
	}
}
