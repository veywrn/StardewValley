using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;

namespace StardewValley.Monsters
{
	public class DinoMonster : Monster
	{
		public enum AttackState
		{
			None,
			Fireball,
			Charge
		}

		public int timeUntilNextAttack;

		protected bool _hasPlayedFireSound;

		public readonly NetBool firing = new NetBool(value: false);

		public NetInt attackState = new NetInt();

		public int nextFireTime;

		public int totalFireTime;

		public int nextChangeDirectionTime;

		public int nextWanderTime;

		public bool wanderState;

		public DinoMonster()
		{
		}

		public DinoMonster(Vector2 position)
			: base("Pepper Rex", position)
		{
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			Sprite.UpdateSourceRect();
			timeUntilNextAttack = 2000;
			nextChangeDirectionTime = Game1.random.Next(1000, 3000);
			nextWanderTime = Game1.random.Next(1000, 2000);
		}

		protected override void initNetFields()
		{
			base.NetFields.AddFields(attackState, firing);
			base.initNetFields();
		}

		public override void reloadSprite()
		{
			base.reloadSprite();
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			Sprite.UpdateSourceRect();
		}

		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + yJumpOffset), Sprite.SourceRect, Color.White, rotation, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
				if (isGlowing)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + yJumpOffset), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, new Vector2(16f, 16f), 4f * Math.Max(0.2f, scale), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f + 0.001f)));
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			Vector2 position = base.Position;
			return new Rectangle((int)position.X + 8, (int)position.Y, Sprite.SpriteWidth * 4 * 3 / 4, 64);
		}

		public override List<Item> getExtraDropItems()
		{
			List<Item> extra_items = new List<Item>();
			if (Game1.random.NextDouble() < 0.10000000149011612)
			{
				extra_items.Add(new Object(107, 1));
			}
			else
			{
				List<Item> non_egg_items = new List<Item>();
				non_egg_items.Add(new Object(580, 1));
				non_egg_items.Add(new Object(583, 1));
				non_egg_items.Add(new Object(584, 1));
				extra_items.Add(Utility.GetRandom(non_egg_items));
			}
			return extra_items;
		}

		protected override void sharedDeathAnimation()
		{
			base.currentLocation.playSound("skeletonDie");
			base.currentLocation.playSound("grunt");
			for (int i = 0; i < 16; i++)
			{
				Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(64, 128, 16, 16), 16, (int)Utility.Lerp(GetBoundingBox().Left, GetBoundingBox().Right, (float)Game1.random.NextDouble()), (int)Utility.Lerp(GetBoundingBox().Bottom, GetBoundingBox().Top, (float)Game1.random.NextDouble()), 1, (int)getTileLocation().Y, Color.White, 4f);
			}
		}

		protected override void localDeathAnimation()
		{
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.HotPink, 10)
			{
				holdLastFrame = true,
				alphaFade = 0.01f,
				interval = 70f
			}, base.currentLocation, 8, 96);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (attackState.Value == 1)
			{
				base.IsWalkingTowardPlayer = false;
				Halt();
			}
			else if (withinPlayerThreshold())
			{
				base.IsWalkingTowardPlayer = true;
			}
			else
			{
				base.IsWalkingTowardPlayer = false;
				nextChangeDirectionTime -= time.ElapsedGameTime.Milliseconds;
				nextWanderTime -= time.ElapsedGameTime.Milliseconds;
				if (nextChangeDirectionTime < 0)
				{
					nextChangeDirectionTime = Game1.random.Next(500, 1000);
					_ = FacingDirection;
					facingDirection.Value = (facingDirection.Value + (Game1.random.Next(0, 3) - 1) + 4) % 4;
				}
				if (nextWanderTime < 0)
				{
					if (wanderState)
					{
						nextWanderTime = Game1.random.Next(1000, 2000);
					}
					else
					{
						nextWanderTime = Game1.random.Next(1000, 3000);
					}
					wanderState = !wanderState;
				}
				if (wanderState)
				{
					moveLeft = (moveUp = (moveRight = (moveDown = false)));
					tryToMoveInDirection(facingDirection.Value, isFarmer: false, base.DamageToFarmer, isGlider);
				}
			}
			timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
			if (attackState.Value == 0 && withinPlayerThreshold(2))
			{
				firing.Set(newValue: false);
				if (timeUntilNextAttack < 0)
				{
					timeUntilNextAttack = 0;
					attackState.Set(1);
					nextFireTime = 500;
					totalFireTime = 3000;
					base.currentLocation.playSound("croak");
				}
			}
			else
			{
				if (totalFireTime <= 0)
				{
					return;
				}
				if (!firing)
				{
					Farmer player = base.Player;
					if (player != null)
					{
						faceGeneralDirection(player.Position);
					}
				}
				totalFireTime -= time.ElapsedGameTime.Milliseconds;
				if (nextFireTime > 0)
				{
					nextFireTime -= time.ElapsedGameTime.Milliseconds;
					if (nextFireTime <= 0)
					{
						if (!firing.Value)
						{
							firing.Set(newValue: true);
							base.currentLocation.playSound("furnace");
						}
						float fire_angle2 = 0f;
						Vector2 shot_origin = new Vector2((float)GetBoundingBox().Center.X - 32f, (float)GetBoundingBox().Center.Y - 32f);
						switch (facingDirection.Value)
						{
						case 0:
							yVelocity = -1f;
							shot_origin.Y -= 64f;
							fire_angle2 = 90f;
							break;
						case 1:
							xVelocity = -1f;
							shot_origin.X += 64f;
							fire_angle2 = 0f;
							break;
						case 3:
							xVelocity = 1f;
							shot_origin.X -= 64f;
							fire_angle2 = 180f;
							break;
						case 2:
							yVelocity = 1f;
							fire_angle2 = 270f;
							break;
						}
						fire_angle2 += (float)Math.Sin((double)((float)totalFireTime / 1000f * 180f) * Math.PI / 180.0) * 25f;
						Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle2 * Math.PI / 180.0), 0f - (float)Math.Sin((double)fire_angle2 * Math.PI / 180.0));
						shot_velocity *= 10f;
						BasicProjectile projectile = new BasicProjectile(25, 10, 0, 1, (float)Math.PI / 16f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", explode: false, damagesMonsters: false, base.currentLocation, this);
						projectile.ignoreTravelGracePeriod.Value = true;
						projectile.maxTravelDistance.Value = 256;
						base.currentLocation.projectiles.Add(projectile);
						nextFireTime = 50;
					}
				}
				if (totalFireTime <= 0)
				{
					totalFireTime = 0;
					nextFireTime = 0;
					attackState.Set(0);
					timeUntilNextAttack = Game1.random.Next(1000, 2000);
				}
			}
		}

		protected override void updateAnimation(GameTime time)
		{
			int direction_offset = 0;
			if (FacingDirection == 2)
			{
				direction_offset = 0;
			}
			else if (FacingDirection == 1)
			{
				direction_offset = 4;
			}
			else if (FacingDirection == 0)
			{
				direction_offset = 8;
			}
			else if (FacingDirection == 3)
			{
				direction_offset = 12;
			}
			if (attackState.Value == 1)
			{
				if (firing.Value)
				{
					Sprite.CurrentFrame = 16 + direction_offset;
				}
				else
				{
					Sprite.CurrentFrame = 17 + direction_offset;
				}
				return;
			}
			if (isMoving() || wanderState)
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
				return;
			}
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
			Sprite.StopAnimation();
		}

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			int direction_offset = 0;
			if (FacingDirection == 2)
			{
				direction_offset = 0;
			}
			else if (FacingDirection == 1)
			{
				direction_offset = 4;
			}
			else if (FacingDirection == 0)
			{
				direction_offset = 8;
			}
			else if (FacingDirection == 3)
			{
				direction_offset = 12;
			}
			if (attackState.Value == 1)
			{
				if (firing.Value)
				{
					Sprite.CurrentFrame = 16 + direction_offset;
				}
				else
				{
					Sprite.CurrentFrame = 17 + direction_offset;
				}
			}
			else if (isMoving())
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
			else
			{
				Sprite.StopAnimation();
			}
		}
	}
}
