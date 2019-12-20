using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace StardewValley.Monsters
{
	public class RockGolem : Monster
	{
		private readonly NetBool seenPlayer = new NetBool();

		public RockGolem()
		{
		}

		public RockGolem(Vector2 position)
			: base("Stone Golem", position)
		{
			base.IsWalkingTowardPlayer = false;
			base.Slipperiness = 2;
			jitteriness.Value = 0.0;
			base.HideShadow = true;
		}

		public RockGolem(Vector2 position, MineShaft mineArea)
			: this(position)
		{
			int mineLevel = mineArea.mineLevel;
			if (mineLevel > 80)
			{
				base.DamageToFarmer *= 2;
				base.Health = (int)((float)base.Health * 2.5f);
			}
			else if (mineLevel > 40)
			{
				base.DamageToFarmer = (int)((float)base.DamageToFarmer * 1.5f);
				base.Health = (int)((float)base.Health * 1.75f);
			}
		}

		public RockGolem(Vector2 position, int difficultyMod)
			: base("Wilderness Golem", position)
		{
			base.IsWalkingTowardPlayer = false;
			base.Slipperiness = 3;
			base.HideShadow = true;
			jitteriness.Value = 0.0;
			base.DamageToFarmer += difficultyMod;
			base.Health += (int)((float)(difficultyMod * difficultyMod) * 2f);
			base.ExperienceGained += difficultyMod;
			if (difficultyMod >= 5 && Game1.random.NextDouble() < 0.05)
			{
				objectsToDrop.Add(749);
			}
			if (difficultyMod >= 5 && Game1.random.NextDouble() < 0.2)
			{
				objectsToDrop.Add(770);
			}
			if (difficultyMod >= 10 && Game1.random.NextDouble() < 0.01)
			{
				objectsToDrop.Add(386);
			}
			if (difficultyMod >= 10 && Game1.random.NextDouble() < 0.01)
			{
				objectsToDrop.Add(386);
			}
			if (difficultyMod >= 10 && Game1.random.NextDouble() < 0.001)
			{
				objectsToDrop.Add(74);
			}
		}

		public RockGolem(Vector2 position, bool alreadySpawned)
			: base("Stone Golem", position)
		{
			if (alreadySpawned)
			{
				base.IsWalkingTowardPlayer = true;
				seenPlayer.Value = true;
				moveTowardPlayerThreshold.Value = 16;
			}
			else
			{
				base.IsWalkingTowardPlayer = false;
			}
			Sprite.loop = false;
			base.Slipperiness = 2;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(seenPlayer);
			position.Field.AxisAlignedMovement = true;
		}

		public override List<Item> getExtraDropItems()
		{
			if (name.Equals("Wilderness Golem"))
			{
				if (Game1.random.NextDouble() <= 0.0001)
				{
					return new List<Item>
					{
						new Hat(40)
					};
				}
				if (Game1.currentSeason.Equals("spring") && Game1.random.NextDouble() < 0.0625)
				{
					List<Item> shoots = new List<Item>();
					int num = Game1.random.Next(2, 6);
					for (int i = 0; i < num; i++)
					{
						shoots.Add(new Object(273, 1));
					}
					return shoots;
				}
			}
			return base.getExtraDropItems();
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - (int)resilience);
			base.focusedOnFarmers = true;
			base.IsWalkingTowardPlayer = true;
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				setTrajectory(xTrajectory, yTrajectory);
				if (base.Health <= 0)
				{
					deathAnimation();
				}
				else
				{
					base.currentLocation.playSound("rockGolemHit");
				}
				base.currentLocation.playSound("hitEnemy");
			}
			return actualDamage;
		}

		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position, Color.DarkGray, 10));
			base.currentLocation.localSound("rockGolemDie");
		}

		protected override void sharedDeathAnimation()
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 576, 64, 64), 32, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, Game1.random.Next(4, 9), (int)getTileLocation().Y);
		}

		public override void noMovementProgressNearPlayerBehavior()
		{
			if (base.IsWalkingTowardPlayer)
			{
				Halt();
				faceGeneralDirection(base.Player.getStandingPosition());
			}
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (base.IsWalkingTowardPlayer)
			{
				base.behaviorAtGameTick(time);
			}
			if (!seenPlayer)
			{
				if (withinPlayerThreshold())
				{
					base.currentLocation.playSound("rockGolemSpawn");
					seenPlayer.Value = true;
				}
				else
				{
					Sprite.currentFrame = 16;
					Sprite.loop = false;
					Sprite.UpdateSourceRect();
				}
			}
			else if (Sprite.currentFrame >= 16)
			{
				Sprite.Animate(time, 16, 8, 75f);
				if (Sprite.currentFrame >= 24)
				{
					Sprite.loop = true;
					Sprite.currentFrame = 0;
					moveTowardPlayerThreshold.Value = 16;
					base.IsWalkingTowardPlayer = true;
					jitteriness.Value = 0.01;
					base.HideShadow = false;
				}
			}
			else if (base.IsWalkingTowardPlayer && Game1.random.NextDouble() < 0.001 && Utility.isOnScreen(getStandingPosition(), 0))
			{
				controller = new PathFindController(this, base.currentLocation, new Point((int)base.Player.getTileLocation().X, (int)base.Player.getTileLocation().Y), -1, null, 200);
			}
		}

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (base.IsWalkingTowardPlayer)
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
			if (!seenPlayer)
			{
				Sprite.currentFrame = 16;
				Sprite.loop = false;
				Sprite.UpdateSourceRect();
			}
			else if (Sprite.currentFrame >= 16)
			{
				Sprite.Animate(time, 16, 8, 75f);
				if (Sprite.currentFrame >= 24)
				{
					Sprite.loop = true;
					Sprite.currentFrame = 0;
					Sprite.UpdateSourceRect();
				}
			}
		}
	}
}
