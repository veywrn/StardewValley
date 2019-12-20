using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;

namespace StardewValley.Monsters
{
	public class ShadowShaman : Monster
	{
		public const int visionDistance = 8;

		public const int spellCooldown = 1500;

		private bool spottedPlayer;

		private readonly NetBool casting = new NetBool();

		private int coolDown = 1500;

		private float rotationTimer;

		public ShadowShaman()
		{
		}

		public ShadowShaman(Vector2 position)
			: base("Shadow Shaman", position)
		{
			if (Game1.MasterPlayer.friendshipData.ContainsKey("???") && Game1.MasterPlayer.friendshipData["???"].Points >= 1250)
			{
				base.DamageToFarmer = 0;
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(casting);
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow Shaman");
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if ((bool)casting)
			{
				for (int i = 0; i < 8; i++)
				{
					b.Draw(Projectile.projectileSheet, Game1.GlobalToLocal(Game1.viewport, getStandingPosition()), new Rectangle(119, 6, 3, 3), Color.White * 0.7f, rotationTimer + (float)i * (float)Math.PI / 4f, new Vector2(8f, 48f), 6f, SpriteEffects.None, 0.95f);
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
				if (who.CurrentTool != null && who.CurrentTool.Name != null && who.CurrentTool.Name.Equals("Holy Sword") && !isBomb)
				{
					base.Health -= damage * 3 / 4;
					base.currentLocation.debris.Add(new Debris(string.Concat(damage * 3 / 4), 1, new Vector2(getStandingX(), getStandingY()), Color.LightBlue, 1f, 0f));
				}
				base.Health -= actualDamage;
				if ((bool)casting && Game1.random.NextDouble() < 0.5)
				{
					coolDown += 200;
				}
				else
				{
					setTrajectory(xTrajectory, yTrajectory);
					base.currentLocation.playSound("shadowHit");
				}
				if (base.Health <= 0)
				{
					base.currentLocation.playSound("shadowDie");
					deathAnimation();
				}
			}
			return actualDamage;
		}

		protected override void sharedDeathAnimation()
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, 16, 5), 16, getStandingX(), getStandingY() - 32, 1, getStandingY() / 64, Color.White);
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X + 2, Sprite.SourceRect.Y + 5, 16, 5), 10, getStandingX(), getStandingY() - 32, 1, getStandingY() / 64, Color.White);
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 10, 16, 5), 16, getStandingX(), getStandingY() - 32, 1, getStandingY() / 64, Color.White);
		}

		protected override void localDeathAnimation()
		{
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10), base.currentLocation);
			for (int i = 1; i < 3; i++)
			{
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, 1f) * 64f * i, Color.Gray * 0.75f, 10)
				{
					delayBeforeAnimationStart = i * 159
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, -1f) * 64f * i, Color.Gray * 0.75f, 10)
				{
					delayBeforeAnimationStart = i * 159
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, 1f) * 64f * i, Color.Gray * 0.75f, 10)
				{
					delayBeforeAnimationStart = i * 159
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, -1f) * 64f * i, Color.Gray * 0.75f, 10)
				{
					delayBeforeAnimationStart = i * 159
				});
			}
		}

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if ((bool)casting)
			{
				Sprite.Animate(time, 16, 4, 200f);
				rotationTimer = (float)((double)((float)time.TotalGameTime.Milliseconds * ((float)Math.PI / 128f) / 24f) % (Math.PI * 1024.0));
			}
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

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (timeBeforeAIMovementAgain <= 0f)
			{
				base.IsInvisible = false;
			}
			if (!spottedPlayer && Utility.couldSeePlayerInPeripheralVision(base.Player, this) && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, getTileLocation(), base.Player.getTileLocation(), 8))
			{
				controller = null;
				spottedPlayer = true;
				Halt();
				facePlayer(base.Player);
				if (Game1.random.NextDouble() < 0.3)
				{
					base.currentLocation.playSound("shadowpeep");
				}
			}
			else if ((bool)casting)
			{
				base.IsWalkingTowardPlayer = false;
				Sprite.Animate(time, 16, 4, 200f);
				rotationTimer = (float)((double)((float)time.TotalGameTime.Milliseconds * ((float)Math.PI / 128f) / 24f) % (Math.PI * 1024.0));
				coolDown -= time.ElapsedGameTime.Milliseconds;
				if (coolDown > 0)
				{
					return;
				}
				base.Scale = 1f;
				Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(GetBoundingBox().Center, 15f, base.Player);
				if (base.Player.attack >= 0 && Game1.random.NextDouble() < 0.6)
				{
					base.currentLocation.projectiles.Add(new DebuffingProjectile(14, 7, 4, 4, (float)Math.PI / 16f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2(GetBoundingBox().X, GetBoundingBox().Y), base.currentLocation, this));
				}
				else
				{
					List<Monster> monstersNearPlayer = new List<Monster>();
					foreach (NPC i in base.currentLocation.characters)
					{
						if (i is Monster && (i as Monster).withinPlayerThreshold(6))
						{
							monstersNearPlayer.Add((Monster)i);
						}
					}
					Monster lowestHealthMonster = null;
					double lowestHealth = 1.0;
					foreach (Monster j in monstersNearPlayer)
					{
						if ((double)j.Health / (double)j.MaxHealth <= lowestHealth)
						{
							lowestHealthMonster = j;
							lowestHealth = (double)j.Health / (double)j.MaxHealth;
						}
					}
					if (lowestHealthMonster != null)
					{
						int amountToHeal = 60;
						lowestHealthMonster.Health = Math.Min(lowestHealthMonster.MaxHealth, lowestHealthMonster.Health + amountToHeal);
						base.currentLocation.playSound("healSound");
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 256, 64, 64), 40f, 8, 0, lowestHealthMonster.Position + new Vector2(32f, 64f), flicker: false, flipped: false));
						base.currentLocation.debris.Add(new Debris(amountToHeal, new Vector2(lowestHealthMonster.GetBoundingBox().Center.X, lowestHealthMonster.GetBoundingBox().Center.Y), Color.Green, 1f, lowestHealthMonster));
					}
				}
				casting.Value = false;
				coolDown = 1500;
				base.IsWalkingTowardPlayer = true;
			}
			else if (spottedPlayer && withinPlayerThreshold(8))
			{
				if (base.Health < 30)
				{
					if (Math.Abs(base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y) > 192)
					{
						if (base.Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X > 0)
						{
							SetMovingLeft(b: true);
						}
						else
						{
							SetMovingRight(b: true);
						}
					}
					else if (base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y > 0)
					{
						SetMovingUp(b: true);
					}
					else
					{
						SetMovingDown(b: true);
					}
				}
				else if (controller == null && !Utility.doesPointHaveLineOfSightInMine(base.currentLocation, getTileLocation(), base.Player.getTileLocation(), 8))
				{
					controller = new PathFindController(this, base.currentLocation, new Point((int)base.Player.getTileLocation().X, (int)base.Player.getTileLocation().Y), -1, null, 300);
					if (controller == null || controller.pathToEndPoint == null || controller.pathToEndPoint.Count == 0)
					{
						spottedPlayer = false;
						Halt();
						controller = null;
						base.addedSpeed = 0;
					}
				}
				else if (coolDown <= 0 && Game1.random.NextDouble() < 0.02)
				{
					casting.Value = true;
					Halt();
					coolDown = 500;
				}
				coolDown -= time.ElapsedGameTime.Milliseconds;
			}
			else if (spottedPlayer)
			{
				base.IsWalkingTowardPlayer = false;
				spottedPlayer = false;
				controller = null;
				base.addedSpeed = 0;
			}
			else
			{
				defaultMovementBehavior(time);
			}
		}
	}
}
