using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Monsters
{
	public class LavaLurk : Monster
	{
		public enum State
		{
			Submerged,
			Lurking,
			Emerged,
			Firing,
			Diving
		}

		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> submergedAnimation = new List<FarmerSprite.AnimationFrame>();

		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> lurkAnimation = new List<FarmerSprite.AnimationFrame>();

		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> emergeAnimation = new List<FarmerSprite.AnimationFrame>();

		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> diveAnimation = new List<FarmerSprite.AnimationFrame>();

		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> resubmergeAnimation = new List<FarmerSprite.AnimationFrame>();

		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> idleAnimation = new List<FarmerSprite.AnimationFrame>();

		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> fireAnimation = new List<FarmerSprite.AnimationFrame>();

		[XmlIgnore]
		public List<FarmerSprite.AnimationFrame> locallyPlayingAnimation;

		[XmlIgnore]
		public bool approachFarmer;

		[XmlIgnore]
		public Vector2 velocity = Vector2.Zero;

		[XmlIgnore]
		public int swimSpeed;

		[XmlIgnore]
		public Farmer targettedFarmer;

		[XmlIgnore]
		public NetEnum<State> currentState = new NetEnum<State>();

		[XmlIgnore]
		public float stateTimer;

		[XmlIgnore]
		public float fireTimer;

		public LavaLurk()
		{
			Initialize();
		}

		public LavaLurk(Vector2 position)
			: base("Lava Lurk", position)
		{
			Sprite.SpriteWidth = 16;
			Sprite.SpriteHeight = 16;
			Sprite.UpdateSourceRect();
			Initialize();
			ignoreDamageLOS.Value = true;
			SetRandomMovement();
			stateTimer = Utility.RandomFloat(3f, 5f);
		}

		public override void reloadSprite()
		{
			base.reloadSprite();
			Sprite.SpriteWidth = 16;
			Sprite.SpriteHeight = 16;
			Sprite.UpdateSourceRect();
		}

		public virtual void Initialize()
		{
			base.HideShadow = true;
			submergedAnimation.AddRange(new FarmerSprite.AnimationFrame[2]
			{
				new FarmerSprite.AnimationFrame(0, 750),
				new FarmerSprite.AnimationFrame(1, 1000)
			});
			lurkAnimation.AddRange(new FarmerSprite.AnimationFrame[2]
			{
				new FarmerSprite.AnimationFrame(2, 250),
				new FarmerSprite.AnimationFrame(3, 250)
			});
			resubmergeAnimation.AddRange(new FarmerSprite.AnimationFrame[3]
			{
				new FarmerSprite.AnimationFrame(3, 250),
				new FarmerSprite.AnimationFrame(2, 250),
				new FarmerSprite.AnimationFrame(1, 250, secondaryArm: false, flip: false, OnDiveAnimationEnd)
			});
			emergeAnimation.AddRange(new FarmerSprite.AnimationFrame[4]
			{
				new FarmerSprite.AnimationFrame(2, 150),
				new FarmerSprite.AnimationFrame(3, 150),
				new FarmerSprite.AnimationFrame(4, 150),
				new FarmerSprite.AnimationFrame(5, 150, secondaryArm: false, flip: false, OnEmergeAnimationEnd, behaviorAtEndOfFrame: true)
			});
			diveAnimation.AddRange(new FarmerSprite.AnimationFrame[4]
			{
				new FarmerSprite.AnimationFrame(5, 150),
				new FarmerSprite.AnimationFrame(4, 150),
				new FarmerSprite.AnimationFrame(3, 150),
				new FarmerSprite.AnimationFrame(2, 150, secondaryArm: false, flip: false, OnDiveAnimationEnd, behaviorAtEndOfFrame: true)
			});
			idleAnimation.AddRange(new FarmerSprite.AnimationFrame[2]
			{
				new FarmerSprite.AnimationFrame(5, 500),
				new FarmerSprite.AnimationFrame(6, 500)
			});
			fireAnimation.AddRange(new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(7, 500)
			});
		}

		public virtual void OnEmergeAnimationEnd(Farmer who)
		{
			PlayAnimation(idleAnimation, loop: true);
		}

		public virtual void OnDiveAnimationEnd(Farmer who)
		{
			PlayAnimation(submergedAnimation, loop: true);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(currentState);
		}

		protected override void sharedDeathAnimation()
		{
			base.currentLocation.playSound("skeletonDie");
			base.currentLocation.playSound("grunt");
			for (int i = 0; i < 16; i++)
			{
				Game1.createRadialDebris(base.currentLocation, "Characters\\Monsters\\Pepper Rex", new Rectangle(64, 128, 16, 16), 16, (int)Utility.Lerp(GetBoundingBox().Left, GetBoundingBox().Right, (float)Game1.random.NextDouble()), (int)Utility.Lerp(GetBoundingBox().Bottom, GetBoundingBox().Top, (float)Game1.random.NextDouble()), 1, (int)getTileLocation().Y, Color.White, 4f);
			}
		}

		protected override void updateAnimation(GameTime time)
		{
			base.updateAnimation(time);
			if (currentState.Value == State.Submerged)
			{
				PlayAnimation(submergedAnimation, loop: true);
			}
			else if (currentState.Value == State.Lurking)
			{
				if (PlayAnimation(lurkAnimation, loop: false) && base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
				{
					Game1.playSound("waterSlosh");
				}
			}
			else if (currentState.Value == State.Emerged)
			{
				if (locallyPlayingAnimation != emergeAnimation && locallyPlayingAnimation != idleAnimation)
				{
					if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
					{
						Game1.playSound("waterSlosh");
					}
					PlayAnimation(emergeAnimation, loop: false);
				}
			}
			else if (currentState.Value == State.Firing)
			{
				PlayAnimation(fireAnimation, loop: true);
			}
			else if (currentState.Value == State.Diving && locallyPlayingAnimation != diveAnimation && locallyPlayingAnimation != submergedAnimation && locallyPlayingAnimation != resubmergeAnimation)
			{
				if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
				{
					Game1.playSound("waterSlosh");
				}
				if (locallyPlayingAnimation == lurkAnimation)
				{
					PlayAnimation(resubmergeAnimation, loop: false);
				}
				else
				{
					PlayAnimation(diveAnimation, loop: false);
				}
			}
			Sprite.animateOnce(time);
		}

		public virtual bool PlayAnimation(List<FarmerSprite.AnimationFrame> animation_to_play, bool loop)
		{
			if (locallyPlayingAnimation != animation_to_play)
			{
				locallyPlayingAnimation = animation_to_play;
				Sprite.setCurrentAnimation(animation_to_play);
				Sprite.loop = loop;
				if (!loop)
				{
					Sprite.oldFrame = animation_to_play.Last().frame;
				}
				return true;
			}
			return false;
		}

		public virtual bool TargetInRange()
		{
			if (targettedFarmer == null)
			{
				return false;
			}
			if (Math.Abs(targettedFarmer.Position.X - base.Position.X) <= 640f && Math.Abs(targettedFarmer.Position.Y - base.Position.Y) <= 640f)
			{
				return true;
			}
			return false;
		}

		public virtual void SetRandomMovement()
		{
			velocity = new Vector2((Game1.random.Next(2) != 1) ? 1 : (-1), (Game1.random.Next(2) != 1) ? 1 : (-1));
		}

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			if (currentState.Value == State.Submerged)
			{
				return -1;
			}
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (targettedFarmer == null || targettedFarmer.currentLocation != base.currentLocation)
			{
				targettedFarmer = null;
				targettedFarmer = findPlayer();
			}
			if (stateTimer > 0f)
			{
				stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
				if (stateTimer <= 0f)
				{
					stateTimer = 0f;
				}
			}
			if (currentState.Value == State.Submerged)
			{
				swimSpeed = 2;
				if (stateTimer == 0f)
				{
					currentState.Value = State.Lurking;
					stateTimer = 1f;
				}
			}
			else if (currentState.Value == State.Lurking)
			{
				swimSpeed = 1;
				if (stateTimer == 0f)
				{
					if (TargetInRange())
					{
						currentState.Value = State.Emerged;
						stateTimer = 1f;
						swimSpeed = 0;
					}
					else
					{
						currentState.Value = State.Diving;
						stateTimer = 1f;
					}
				}
			}
			else if (currentState.Value == State.Emerged)
			{
				if (stateTimer == 0f)
				{
					currentState.Value = State.Firing;
					stateTimer = 1f;
					fireTimer = 0.25f;
				}
			}
			else if (currentState.Value == State.Firing)
			{
				if (stateTimer == 0f)
				{
					currentState.Value = State.Diving;
					stateTimer = 1f;
				}
				if (fireTimer > 0f)
				{
					fireTimer -= (float)time.ElapsedGameTime.TotalSeconds;
					if (fireTimer <= 0f)
					{
						fireTimer = 0.25f;
						if (targettedFarmer != null)
						{
							Vector2 shot_origin = base.Position + new Vector2(0f, -32f);
							Vector2 shot_velocity = targettedFarmer.Position - shot_origin;
							shot_velocity.Normalize();
							shot_velocity *= 7f;
							base.currentLocation.playSound("fireball");
							BasicProjectile projectile = new BasicProjectile(25, 10, 0, 3, (float)Math.PI / 16f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", explode: false, damagesMonsters: false, base.currentLocation, this);
							projectile.ignoreLocationCollision.Value = true;
							projectile.ignoreTravelGracePeriod.Value = true;
							projectile.maxTravelDistance.Value = 640;
							base.currentLocation.projectiles.Add(projectile);
						}
					}
				}
			}
			else if (currentState.Value == State.Diving && stateTimer == 0f)
			{
				currentState.Value = State.Submerged;
				stateTimer = Utility.RandomFloat(3f, 5f);
				approachFarmer = !approachFarmer;
				if (approachFarmer)
				{
					targettedFarmer = findPlayer();
				}
				SetRandomMovement();
			}
			if (targettedFarmer != null && approachFarmer)
			{
				if (getTileX() > targettedFarmer.getTileX())
				{
					velocity.X = -1f;
				}
				else if (getTileX() < targettedFarmer.getTileX())
				{
					velocity.X = 1f;
				}
				if (getTileY() > targettedFarmer.getTileY())
				{
					velocity.Y = -1f;
				}
				else if (getTileY() < targettedFarmer.getTileY())
				{
					velocity.Y = 1f;
				}
			}
			if (velocity.X != 0f || velocity.Y != 0f)
			{
				Rectangle next_bounds = GetBoundingBox();
				Vector2 next_position = base.Position;
				next_bounds.Inflate(48, 48);
				next_bounds.X += (int)velocity.X * swimSpeed;
				next_position.X += (int)velocity.X * swimSpeed;
				if (!CheckInWater(next_bounds))
				{
					velocity.X *= -1f;
					next_bounds.X += (int)velocity.X * swimSpeed;
					next_position.X += (int)velocity.X * swimSpeed;
				}
				next_bounds.Y += (int)velocity.Y * swimSpeed;
				next_position.Y += (int)velocity.Y * swimSpeed;
				if (!CheckInWater(next_bounds))
				{
					velocity.Y *= -1f;
					next_bounds.Y += (int)velocity.Y * swimSpeed;
					next_position.Y += (int)velocity.Y * swimSpeed;
				}
				if (base.Position != next_position)
				{
					base.Position = next_position;
				}
			}
		}

		public static bool IsLavaTile(GameLocation location, int x, int y)
		{
			return location.doesTileHaveProperty(x, y, "Water", "Back") != null;
		}

		public bool CheckInWater(Rectangle position)
		{
			for (int x = position.Left / 64; x <= position.Right / 64; x++)
			{
				for (int y = position.Top / 64; y <= position.Bottom / 64; y++)
				{
					if (!IsLavaTile(base.currentLocation, x, y))
					{
						return false;
					}
				}
			}
			return true;
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
		}

		public override Debris ModifyMonsterLoot(Debris debris)
		{
			if (debris != null)
			{
				debris.chunksMoveTowardPlayer = true;
			}
			return debris;
		}
	}
}
