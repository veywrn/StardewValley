using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Projectiles;
using System;
using System.Xml.Serialization;

namespace StardewValley.Monsters
{
	public class Shooter : Monster
	{
		public NetBool shooting = new NetBool();

		public int shotsLeft;

		public float nextShot;

		public int projectileSpeed = 12;

		public int projectileDebuff = 26;

		public int numberOfShotsPerFire = 1;

		public float aimTime = 0.25f;

		public float burstTime = 0.25f;

		public float aimEndTime = 1f;

		public int firedProjectile = 12;

		public string damageSound = "shadowHit";

		public string fireSound = "Cowboy_gunshot";

		public int projectileRange = 10;

		public int desiredDistance = 5;

		public int fireRange = 8;

		[XmlIgnore]
		public NetEvent0 fireEvent = new NetEvent0();

		public Shooter()
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(shooting, fireEvent);
			fireEvent.onEvent += OnFire;
		}

		public override int GetBaseDifficultyLevel()
		{
			return 1;
		}

		public virtual void OnFire()
		{
			shakeTimer = 250;
		}

		public override bool ShouldActuallyMoveAwayFromPlayer()
		{
			if (base.Player != null && Math.Abs(base.Player.getTileX() - getTileX()) < desiredDistance && Math.Abs(base.Player.getTileY() - getTileY()) < desiredDistance)
			{
				return true;
			}
			return base.ShouldActuallyMoveAwayFromPlayer();
		}

		public override Rectangle GetBoundingBox()
		{
			return base.GetBoundingBox();
		}

		public Shooter(Vector2 position)
			: base("Shadow Sniper", position)
		{
			Sprite.SpriteHeight = 32;
			Sprite.SpriteWidth = 32;
			forceOneTileWide.Value = true;
			Sprite.UpdateSourceRect();
			InitializeVariant();
		}

		public Shooter(Vector2 position, string monster_name)
			: base(monster_name, position)
		{
			Sprite.SpriteHeight = 32;
			Sprite.SpriteWidth = 32;
			forceOneTileWide.Value = true;
			Sprite.UpdateSourceRect();
			InitializeVariant();
		}

		public virtual void InitializeVariant()
		{
			if (!(base.Name == "Shadow Sniper"))
			{
				_ = (base.Name == "Skeleton Gunner");
			}
			nextShot = 1f;
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name);
			Sprite.SpriteHeight = 32;
			Sprite.UpdateSourceRect();
		}

		protected override void updateAnimation(GameTime time)
		{
			if (shooting.Value)
			{
				if (FacingDirection == 2)
				{
					Sprite.CurrentFrame = 16;
				}
				else if (FacingDirection == 1)
				{
					Sprite.CurrentFrame = 17;
				}
				else if (FacingDirection == 0)
				{
					Sprite.CurrentFrame = 18;
				}
				else if (FacingDirection == 3)
				{
					Sprite.CurrentFrame = 19;
				}
			}
			if (!Game1.IsMasterGame && isMoving())
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
			}
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (!shooting.Value)
			{
				if (nextShot > 0f)
				{
					nextShot -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else if (base.Player != null)
				{
					int player_x = base.Player.getTileX();
					int player_y = base.Player.getTileY();
					int x = getTileX();
					int y = getTileY();
					if (Math.Abs(player_x - x) <= fireRange && Math.Abs(player_y - y) <= fireRange && (Math.Abs(player_x - x) < 2 || Math.Abs(player_y - y) < 2))
					{
						Halt();
						faceGeneralDirection(base.Player.getStandingPosition());
						shooting.Value = true;
						nextShot = aimTime;
						shotsLeft = numberOfShotsPerFire;
					}
				}
			}
			else
			{
				xVelocity = 0f;
				yVelocity = 0f;
				if (shotsLeft > 0)
				{
					if (nextShot > 0f)
					{
						nextShot -= (float)time.ElapsedGameTime.TotalSeconds;
						if (nextShot <= 0f)
						{
							Vector2 shot_velocity = Vector2.Zero;
							float starting_rotation = 0f;
							if ((int)facingDirection == 0)
							{
								shot_velocity = new Vector2(0f, -1f);
								starting_rotation = 0f;
							}
							if ((int)facingDirection == 3)
							{
								shot_velocity = new Vector2(-1f, 0f);
								starting_rotation = -(float)Math.PI / 2f;
							}
							if ((int)facingDirection == 1)
							{
								shot_velocity = new Vector2(1f, 0f);
								starting_rotation = (float)Math.PI / 2f;
							}
							if ((int)facingDirection == 2)
							{
								shot_velocity = new Vector2(0f, 1f);
								starting_rotation = (float)Math.PI;
							}
							shot_velocity *= (float)projectileSpeed;
							fireEvent.Fire();
							base.currentLocation.playSound(fireSound);
							BasicProjectile projectile = new BasicProjectile(base.DamageToFarmer, firedProjectile, 0, 0, 0f, shot_velocity.X, shot_velocity.Y, base.Position, "", "", explode: false, damagesMonsters: false, base.currentLocation, this);
							projectile.startingRotation.Value = starting_rotation;
							projectile.height.Value = 24f;
							projectile.debuff.Value = projectileDebuff;
							projectile.ignoreTravelGracePeriod.Value = true;
							projectile.IgnoreLocationCollision = true;
							projectile.maxTravelDistance.Value = 64 * projectileRange;
							base.currentLocation.projectiles.Add(projectile);
							shotsLeft--;
							if (shotsLeft == 0)
							{
								nextShot = aimEndTime;
							}
							else
							{
								nextShot = burstTime;
							}
						}
					}
				}
				else if (nextShot > 0f)
				{
					nextShot -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else
				{
					shooting.Value = false;
					nextShot = 2f;
				}
			}
			base.behaviorAtGameTick(time);
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
			if (shooting.Value)
			{
				MovePosition(time, Game1.viewport, location);
			}
			else
			{
				base.updateMovement(location, time);
			}
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			shooting.Value = false;
			shotsLeft = 0;
			nextShot = Math.Max(0.5f, nextShot);
			base.currentLocation.playSound(damageSound);
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}

		protected override void localDeathAnimation()
		{
			if (base.Name == "Shadow Sniper")
			{
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10), base.currentLocation);
				for (int i = 1; i < 3; i++)
				{
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(0f, 1f) * 64f * i, Color.Gray * 0.75f, 10)
					{
						delayBeforeAnimationStart = i * 159
					});
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(0f, -1f) * 64f * i, Color.Gray * 0.75f, 10)
					{
						delayBeforeAnimationStart = i * 159
					});
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, 0f) * 64f * i, Color.Gray * 0.75f, 10)
					{
						delayBeforeAnimationStart = i * 159
					});
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, 0f) * 64f * i, Color.Gray * 0.75f, 10)
					{
						delayBeforeAnimationStart = i * 159
					});
				}
				base.currentLocation.localSound("shadowDie");
			}
		}

		protected override void sharedDeathAnimation()
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, 16, 5), 16, getStandingX(), getStandingY() - 32, 1, getStandingY() / 64, Color.White, 4f);
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X + 2, Sprite.SourceRect.Y + 5, 16, 5), 10, getStandingX(), getStandingY() - 32, 1, getStandingY() / 64, Color.White, 4f);
		}

		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			fireEvent.Poll();
		}
	}
}
