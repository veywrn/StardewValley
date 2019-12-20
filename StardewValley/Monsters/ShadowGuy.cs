using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using xTile.Dimensions;

namespace StardewValley.Monsters
{
	public class ShadowGuy : Monster
	{
		public const int visionDistance = 8;

		public const int spellCooldown = 1500;

		private bool spottedPlayer;

		private bool casting;

		private bool teleporting;

		private int coolDown = 1500;

		private IEnumerator<Point> teleportationPath;

		private float rotationTimer;

		public ShadowGuy()
		{
		}

		public ShadowGuy(Vector2 position)
			: base("Shadow Guy", position)
		{
			if (Game1.MasterPlayer.friendshipData.ContainsKey("???") && Game1.MasterPlayer.friendshipData["???"].Points >= 1250)
			{
				base.DamageToFarmer = 0;
			}
			Halt();
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow " + ((base.Position.X % 4f == 0f) ? "Girl" : "Guy"));
		}

		public override void draw(SpriteBatch b)
		{
			if (!casting)
			{
				base.draw(b);
				return;
			}
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-8, 9), 64 + Game1.random.Next(-8, 9)), Sprite.SourceRect, Color.White * 0.5f, rotation, new Vector2(32f, 64f), Math.Max(0.2f, scale), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-8, 9), 64 + Game1.random.Next(-8, 9)), Sprite.SourceRect, Color.White * 0.5f, rotation, new Vector2(32f, 64f), Math.Max(0.2f, scale), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 1) / 10000f)));
			for (int i = 0; i < 8; i++)
			{
				b.Draw(Projectile.projectileSheet, Game1.GlobalToLocal(Game1.viewport, getStandingPosition()), new Microsoft.Xna.Framework.Rectangle(212, 20, 24, 24), Color.White * 0.7f, rotationTimer + (float)i * (float)Math.PI / 4f, new Vector2(32f, 256f), 1.5f, SpriteEffects.None, 0.95f);
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
				if (who.CurrentTool.Name.Equals("Holy Sword") && !isBomb)
				{
					base.Health -= damage * 3 / 4;
					base.currentLocation.debris.Add(new Debris(string.Concat(damage * 3 / 4), 1, new Vector2(getStandingX(), getStandingY()), Color.LightBlue, 1f, 0f));
				}
				base.Health -= actualDamage;
				if (casting && Game1.random.NextDouble() < 0.5)
				{
					coolDown += 200;
				}
				else if (Game1.random.NextDouble() < 0.4 + 1.0 / (double)base.Health && !base.currentLocation.IsFarm)
				{
					castTeleport();
					if (base.Health <= 10)
					{
						base.speed = Math.Min(3, base.speed + 1);
					}
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

		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10));
		}

		protected override void sharedDeathAnimation()
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, 64, 21), 64, getStandingX(), getStandingY() - 32, 1, getStandingY() / 64, Color.White);
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(Sprite.SourceRect.X + 10, Sprite.SourceRect.Y + 21, 64, 21), 42, getStandingX(), getStandingY() - 32, 1, getStandingY() / 64, Color.White);
		}

		public void castTeleport()
		{
			int tries = 0;
			Vector2 possiblePoint = new Vector2(getTileLocation().X + (float)((Game1.random.NextDouble() < 0.5) ? Game1.random.Next(-5, -1) : Game1.random.Next(2, 6)), getTileLocation().Y + (float)((Game1.random.NextDouble() < 0.5) ? Game1.random.Next(-5, -1) : Game1.random.Next(2, 6)));
			for (; tries < 6; tries++)
			{
				if (base.currentLocation.isTileOnMap(possiblePoint) && base.currentLocation.isTileLocationOpen(new Location((int)possiblePoint.X, (int)possiblePoint.Y)) && !base.currentLocation.isTileOccupiedForPlacement(possiblePoint))
				{
					break;
				}
				possiblePoint = new Vector2(getTileLocation().X + (float)((Game1.random.NextDouble() < 0.5) ? Game1.random.Next(-5, -1) : Game1.random.Next(2, 6)), getTileLocation().Y + (float)((Game1.random.NextDouble() < 0.5) ? Game1.random.Next(-5, -1) : Game1.random.Next(2, 6)));
			}
			if (tries < 6)
			{
				teleporting = true;
				teleportationPath = Utility.GetPointsOnLine((int)getTileLocation().X, (int)getTileLocation().Y, (int)possiblePoint.X, (int)possiblePoint.Y, ignoreSwap: true).GetEnumerator();
				coolDown = 20;
			}
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (timeBeforeAIMovementAgain <= 0f)
			{
				base.IsInvisible = false;
			}
			if (teleporting)
			{
				coolDown -= time.ElapsedGameTime.Milliseconds;
				if (coolDown <= 0)
				{
					if (teleportationPath.MoveNext())
					{
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(Sprite.textureName, Sprite.SourceRect, base.Position, flipped: false, 0.04f, Color.White));
						base.Position = new Vector2(teleportationPath.Current.X * 64 + 4, teleportationPath.Current.Y * 64 - 32 - 4);
						coolDown = 20;
					}
					else
					{
						teleporting = false;
						coolDown = 500;
					}
				}
			}
			else if (!spottedPlayer && Utility.couldSeePlayerInPeripheralVision(base.Player, this) && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, getTileLocation(), base.Player.getTileLocation(), 8))
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
			else if (casting)
			{
				Halt();
				base.IsWalkingTowardPlayer = false;
				rotationTimer = (float)((double)((float)time.TotalGameTime.Milliseconds * ((float)Math.PI / 128f) / 24f) % (Math.PI * 1024.0));
				coolDown -= time.ElapsedGameTime.Milliseconds;
				if (coolDown <= 0)
				{
					base.Scale = 1f;
					Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(GetBoundingBox().Center, 15f, base.Player);
					if (base.Player.attack >= 0 && Game1.random.NextDouble() < 0.6)
					{
						base.currentLocation.projectiles.Add(new DebuffingProjectile(18, 2, 4, 4, (float)Math.PI / 16f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2(GetBoundingBox().X, GetBoundingBox().Y)));
					}
					else
					{
						base.currentLocation.playSound("fireball");
						base.currentLocation.projectiles.Add(new BasicProjectile(10, 3, 0, 3, 0f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2(GetBoundingBox().X, GetBoundingBox().Y)));
					}
					casting = false;
					coolDown = 1500;
					base.IsWalkingTowardPlayer = true;
				}
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
					casting = true;
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
