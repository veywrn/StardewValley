using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Projectiles;
using System;

namespace StardewValley.Monsters
{
	public class Skeleton : Monster
	{
		private bool spottedPlayer;

		private readonly NetBool throwing = new NetBool();

		private int controllerAttemptTimer;

		public Skeleton()
		{
		}

		public Skeleton(Vector2 position)
			: base("Skeleton", position, Game1.random.Next(4))
		{
			Sprite.SpriteHeight = 32;
			Sprite.UpdateSourceRect();
			base.IsWalkingTowardPlayer = false;
			jitteriness.Value = 0.0;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(throwing);
			position.Field.AxisAlignedMovement = true;
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Skeleton");
			Sprite.SpriteHeight = 32;
			Sprite.UpdateSourceRect();
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			base.currentLocation.playSound("skeletonHit");
			base.Slipperiness = 3;
			if ((bool)throwing)
			{
				throwing.Value = false;
				Halt();
			}
			if (base.Health - damage <= 0)
			{
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(46, base.Position, Color.White, 10, flipped: false, 70f));
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(46, base.Position + new Vector2(-16f, 0f), Color.White, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 100
				});
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(46, base.Position + new Vector2(16f, 0f), Color.White, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 200
				});
			}
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}

		public override void shedChunks(int number)
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 128, 16, 16), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, Color.White, 4f);
		}

		protected override void sharedDeathAnimation()
		{
			base.currentLocation.playSound("skeletonDie");
			Game1.random.Next(5, 13);
			shedChunks(20);
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(3, (Game1.random.NextDouble() < 0.5) ? 3 : 35, 10, 10), 11, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, 1, (int)getTileLocation().Y, Color.White, 4f);
		}

		public override void update(GameTime time, GameLocation location)
		{
			if (!throwing)
			{
				base.update(time, location);
				return;
			}
			if (Game1.IsMasterGame)
			{
				behaviorAtGameTick(time);
			}
			updateAnimation(time);
		}

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if ((bool)throwing)
			{
				if (invincibleCountdown > 0)
				{
					invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
					if (invincibleCountdown <= 0)
					{
						stopGlowing();
					}
				}
				Sprite.Animate(time, 20, 5, 150f);
				if (Sprite.currentFrame == 24)
				{
					Sprite.currentFrame = 23;
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
			}
			else
			{
				Sprite.StopAnimation();
			}
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (!throwing)
			{
				base.behaviorAtGameTick(time);
			}
			if (!spottedPlayer && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, getTileLocation(), base.Player.getTileLocation(), 8))
			{
				controller = new PathFindController(this, base.currentLocation, new Point(base.Player.getStandingX() / 64, base.Player.getStandingY() / 64), Game1.random.Next(4), null, 200);
				spottedPlayer = true;
				Halt();
				facePlayer(base.Player);
				base.currentLocation.playSound("skeletonStep");
				base.IsWalkingTowardPlayer = true;
			}
			else if ((bool)throwing)
			{
				if (invincibleCountdown > 0)
				{
					invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
					if (invincibleCountdown <= 0)
					{
						stopGlowing();
					}
				}
				Sprite.Animate(time, 20, 5, 150f);
				if (Sprite.currentFrame == 24)
				{
					throwing.Value = false;
					Sprite.currentFrame = 0;
					faceDirection(2);
					Vector2 v = Utility.getVelocityTowardPlayer(new Point((int)base.Position.X, (int)base.Position.Y), 8f, base.Player);
					base.currentLocation.projectiles.Add(new BasicProjectile(base.DamageToFarmer, 4, 0, 0, (float)Math.PI / 16f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "skeletonHit", "skeletonStep", explode: false, damagesMonsters: false, base.currentLocation, this));
				}
			}
			else if (spottedPlayer && controller == null && Game1.random.NextDouble() < 0.002 && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, getTileLocation(), base.Player.getTileLocation(), 8))
			{
				throwing.Value = true;
				Halt();
				Sprite.currentFrame = 20;
				shake(750);
			}
			else if (withinPlayerThreshold(2))
			{
				controller = null;
			}
			else if (spottedPlayer && controller == null && controllerAttemptTimer <= 0)
			{
				controller = new PathFindController(this, base.currentLocation, new Point(base.Player.getStandingX() / 64, base.Player.getStandingY() / 64), Game1.random.Next(4), null, 200);
				Halt();
				facePlayer(base.Player);
				controllerAttemptTimer = (base.wildernessFarmMonster ? 2000 : 1000);
			}
			else if (base.wildernessFarmMonster)
			{
				spottedPlayer = true;
				base.IsWalkingTowardPlayer = true;
			}
			controllerAttemptTimer -= time.ElapsedGameTime.Milliseconds;
		}
	}
}
