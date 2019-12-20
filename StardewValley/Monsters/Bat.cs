using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace StardewValley.Monsters
{
	public class Bat : Monster
	{
		public const float rotationIncrement = (float)Math.PI / 64f;

		private readonly NetInt wasHitCounter = new NetInt(0);

		private float targetRotation;

		private readonly NetBool turningRight = new NetBool();

		private readonly NetBool seenPlayer = new NetBool();

		private readonly NetBool cursedDoll = new NetBool();

		private readonly NetBool hauntedSkull = new NetBool();

		private ICue batFlap;

		private float extraVelocity;

		private float maxSpeed = 5f;

		private List<Vector2> previousPositions = new List<Vector2>();

		public Bat()
		{
		}

		public Bat(Vector2 position)
			: base("Bat", position)
		{
			base.Slipperiness = 24 + Game1.random.Next(-10, 11);
			Halt();
			base.IsWalkingTowardPlayer = false;
			base.HideShadow = true;
		}

		public Bat(Vector2 position, int mineLevel)
			: base("Bat", position)
		{
			base.Slipperiness = 20 + Game1.random.Next(-5, 6);
			switch (mineLevel)
			{
			case 77377:
				parseMonsterInfo("Lava Bat");
				base.Name = "Haunted Skull";
				reloadSprite();
				extraVelocity = 1f;
				extraVelocity = 3f;
				maxSpeed = 8f;
				shakeTimer = 100;
				cursedDoll.Value = true;
				hauntedSkull.Value = true;
				objectsToDrop.Clear();
				break;
			case -666:
				parseMonsterInfo("Iridium Bat");
				reloadSprite();
				extraVelocity = 1f;
				extraVelocity = 3f;
				maxSpeed = 8f;
				base.Health *= 2;
				shakeTimer = 100;
				cursedDoll.Value = true;
				objectsToDrop.Clear();
				break;
			case 40:
			case 41:
			case 42:
			case 43:
			case 44:
			case 45:
			case 46:
			case 47:
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
			case 58:
			case 59:
			case 60:
			case 61:
			case 62:
			case 63:
			case 64:
			case 65:
			case 66:
			case 67:
			case 68:
			case 69:
			case 70:
			case 71:
			case 72:
			case 73:
			case 74:
			case 75:
			case 76:
			case 77:
			case 78:
			case 79:
				base.Name = "Frost Bat";
				parseMonsterInfo("Frost Bat");
				reloadSprite();
				break;
			default:
				if (mineLevel >= 80 && mineLevel < 171)
				{
					base.Name = "Lava Bat";
					parseMonsterInfo("Lava Bat");
					reloadSprite();
				}
				else if (mineLevel >= 171)
				{
					base.Name = "Iridium Bat";
					parseMonsterInfo("Iridium Bat");
					reloadSprite();
					extraVelocity = 1f;
				}
				break;
			}
			if (mineLevel > 999)
			{
				extraVelocity = 3f;
				maxSpeed = 8f;
				base.Health *= 2;
				shakeTimer = 999999;
			}
			Halt();
			base.IsWalkingTowardPlayer = false;
			base.HideShadow = true;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(wasHitCounter, turningRight, seenPlayer, cursedDoll, hauntedSkull);
		}

		public override void reloadSprite()
		{
			if (Sprite == null)
			{
				Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name);
			}
			else
			{
				Sprite.textureName.Value = "Characters\\Monsters\\" + base.Name;
			}
			base.HideShadow = true;
		}

		public override List<Item> getExtraDropItems()
		{
			List<Item> extraDrops = new List<Item>();
			if ((bool)cursedDoll && Game1.random.NextDouble() < 0.1429 && (bool)hauntedSkull)
			{
				switch (Game1.random.Next(11))
				{
				case 0:
					switch (Game1.random.Next(6))
					{
					case 0:
					{
						Clothing v2 = new Clothing(10);
						v2.clothesColor.Value = Color.DimGray;
						extraDrops.Add(v2);
						break;
					}
					case 1:
						extraDrops.Add(new Clothing(1004));
						break;
					case 2:
						extraDrops.Add(new Clothing(1014));
						break;
					case 3:
						extraDrops.Add(new Clothing(1263));
						break;
					case 4:
						extraDrops.Add(new Clothing(1262));
						break;
					case 5:
					{
						Clothing v2 = new Clothing(12);
						v2.clothesColor.Value = Color.DimGray;
						extraDrops.Add(v2);
						break;
					}
					}
					break;
				case 1:
					extraDrops.Add(new MeleeWeapon(2));
					break;
				case 2:
					extraDrops.Add(new Object(288, 1));
					break;
				case 3:
					extraDrops.Add(new Ring(534));
					break;
				case 4:
					extraDrops.Add(new Ring(531));
					break;
				case 5:
					do
					{
						extraDrops.Add(new Object(768, 1));
						extraDrops.Add(new Object(769, 1));
					}
					while (Game1.random.NextDouble() < 0.33);
					break;
				case 6:
					extraDrops.Add(new Object(581, 1));
					break;
				case 7:
					extraDrops.Add(new Object(582, 1));
					break;
				case 8:
					extraDrops.Add(new Object(725, 1));
					break;
				case 9:
					extraDrops.Add(new Object(86, 1));
					break;
				case 10:
					if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccVault"))
					{
						extraDrops.Add(new Object(275, 1));
					}
					else
					{
						extraDrops.Add(new Object(749, 1));
					}
					break;
				}
				return extraDrops;
			}
			if ((bool)hauntedSkull && Game1.random.NextDouble() < 0.25 && Game1.currentSeason == "winter")
			{
				do
				{
					extraDrops.Add(new Object(273, 1));
				}
				while (Game1.random.NextDouble() < 0.4);
			}
			if ((bool)hauntedSkull && Game1.random.NextDouble() < 0.001502)
			{
				extraDrops.Add(new Object(279, 1));
			}
			if (extraDrops.Count > 0)
			{
				return extraDrops;
			}
			return base.getExtraDropItems();
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - (int)resilience);
			seenPlayer.Value = true;
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				setTrajectory(xTrajectory / 3, yTrajectory / 3);
				wasHitCounter.Value = 500;
				base.currentLocation.playSound("hitEnemy");
				if (base.Health <= 0)
				{
					deathAnimation();
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position, Color.DarkMagenta, 10));
					if ((bool)cursedDoll)
					{
						base.currentLocation.playSound("rockGolemHit");
						if ((bool)hauntedSkull)
						{
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(Sprite.textureName, new Rectangle(0, 32, 16, 16), 2000f, 1, 9999, position, flicker: false, flipped: false, 1f, 0.02f, Color.White, 4f, 0f, 0f, 0f)
							{
								motion = new Vector2(0f - xVelocity, Game1.random.Next(-12, -7)),
								acceleration = new Vector2(0f, 0.4f),
								rotationChange = (float)Game1.random.Next(-200, 200) / 1000f
							});
						}
						else if (who != null)
						{
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(388, 1894, 24, 22), 40f, 6, 9999, position, flicker: false, flipped: true, 1f, 0f, Color.Black * 0.67f, 4f, 0f, 0f, 0f)
							{
								motion = new Vector2(8f, -4f)
							});
						}
					}
					else
					{
						base.currentLocation.playSound("batScreech");
					}
				}
			}
			base.addedSpeed = Game1.random.Next(-1, 1);
			return actualDamage;
		}

		public override void shedChunks(int number, float scale)
		{
			if ((bool)cursedDoll && (bool)hauntedSkull)
			{
				Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 64, 16, 16), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, Color.White, 4f);
			}
			else
			{
				Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 384, 64, 64), 32, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, Color.White, scale);
			}
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (!Utility.isOnScreen(base.Position, 128))
			{
				return;
			}
			if ((bool)cursedDoll)
			{
				if ((bool)hauntedSkull)
				{
					Vector2 pos_offset = Vector2.Zero;
					if (previousPositions.Count > 2)
					{
						pos_offset = base.Position - previousPositions[1];
					}
					int direction = (!(Math.Abs(pos_offset.X) > Math.Abs(pos_offset.Y))) ? ((!(pos_offset.Y < 0f)) ? 2 : 0) : ((pos_offset.X > 0f) ? 1 : 3);
					if (direction == -1)
					{
						direction = 2;
					}
					Vector2 offset = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (Math.PI * 60.0)));
					b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + offset.Y / 20f, SpriteEffects.None, 0.0001f);
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + (((bool)seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16), Color.Red * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - 1f) / 10000f);
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + (((bool)seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16), Color.Yellow * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f) / 10000f);
					for (int i = previousPositions.Count - 1; i >= 0; i -= 2)
					{
						b.Draw(Sprite.Texture, new Vector2(previousPositions[i].X - (float)Game1.viewport.X, previousPositions[i].Y - (float)Game1.viewport.Y + (float)yJumpOffset) + drawOffset + new Vector2(32f, 32f) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + (((bool)seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16), Color.White * (0f + 0.125f * (float)i), 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - (float)i) / 10000f);
					}
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + (((bool)seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f + 1f) / 10000f);
				}
				else
				{
					Vector2 offset2 = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (Math.PI * 60.0)));
					b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + offset2.Y / 20f, SpriteEffects.None, 0.0001f);
					b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset2, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16), Color.Violet * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - 1f) / 10000f);
					b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset2, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16), Color.Lime * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f) / 10000f);
					b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset2, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16), new Color(255, 50, 50), 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f + 1f) / 10000f);
				}
			}
			else
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), Sprite.SourceRect, (shakeTimer > 0) ? Color.Red : Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.92f);
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, base.wildernessFarmMonster ? 0.0001f : ((float)(getStandingY() - 1) / 10000f));
				if (isGlowing)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)getStandingY() / 10000f + 0.001f)));
				}
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if ((int)wasHitCounter >= 0)
			{
				wasHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
			}
			if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity) || base.Position.X < -2000f || base.Position.Y < -2000f)
			{
				base.Health = -500;
			}
			if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
			{
				base.Health = -500;
			}
			if (!base.focusedOnFarmers && !withinPlayerThreshold(6) && !seenPlayer)
			{
				return;
			}
			seenPlayer.Value = true;
			if (invincibleCountdown > 0)
			{
				if (base.Name.Equals("Lava Bat"))
				{
					glowingColor = Color.Cyan;
				}
				return;
			}
			float xSlope3 = -(base.Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X);
			float ySlope3 = base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y;
			float t = Math.Max(1f, Math.Abs(xSlope3) + Math.Abs(ySlope3));
			if (t < (float)((extraVelocity > 0f) ? 192 : 64))
			{
				xVelocity = Math.Max(0f - maxSpeed, Math.Min(maxSpeed, xVelocity * 1.05f));
				yVelocity = Math.Max(0f - maxSpeed, Math.Min(maxSpeed, yVelocity * 1.05f));
			}
			xSlope3 /= t;
			ySlope3 /= t;
			if ((int)wasHitCounter <= 0)
			{
				targetRotation = (float)Math.Atan2(0f - ySlope3, xSlope3) - (float)Math.PI / 2f;
				if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) > Math.PI * 7.0 / 8.0 && Game1.random.NextDouble() < 0.5)
				{
					turningRight.Value = true;
				}
				else if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) < Math.PI / 8.0)
				{
					turningRight.Value = false;
				}
				if ((bool)turningRight)
				{
					rotation -= (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
				}
				else
				{
					rotation += (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
				}
				rotation %= (float)Math.PI * 2f;
				wasHitCounter.Value = 0;
			}
			float maxAccel = Math.Min(5f, Math.Max(1f, 5f - t / 64f / 2f)) + extraVelocity;
			xSlope3 = (float)Math.Cos((double)rotation + Math.PI / 2.0);
			ySlope3 = 0f - (float)Math.Sin((double)rotation + Math.PI / 2.0);
			xVelocity += (0f - xSlope3) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			yVelocity += (0f - ySlope3) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			if (Math.Abs(xVelocity) > Math.Abs((0f - xSlope3) * maxSpeed))
			{
				xVelocity -= (0f - xSlope3) * maxAccel / 6f;
			}
			if (Math.Abs(yVelocity) > Math.Abs((0f - ySlope3) * maxSpeed))
			{
				yVelocity -= (0f - ySlope3) * maxAccel / 6f;
			}
		}

		protected override void updateAnimation(GameTime time)
		{
			if (base.focusedOnFarmers || withinPlayerThreshold(6) || (bool)seenPlayer)
			{
				Sprite.Animate(time, 0, 4, 80f);
				if (Sprite.currentFrame % 3 == 0 && Utility.isOnScreen(base.Position, 512) && (batFlap == null || !batFlap.IsPlaying) && Game1.soundBank != null && base.currentLocation == Game1.currentLocation && !cursedDoll)
				{
					batFlap = Game1.soundBank.GetCue("batFlap");
					batFlap.Play();
				}
				if (cursedDoll.Value)
				{
					shakeTimer -= time.ElapsedGameTime.Milliseconds;
					if (shakeTimer < 0)
					{
						if (!hauntedSkull.Value)
						{
							base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16), position + new Vector2(0f, -32f), flipped: false, 0.1f, new Color(255, 50, 255) * 0.8f)
							{
								scale = 4f
							});
						}
						shakeTimer = 50;
					}
					previousPositions.Add(base.Position);
					if (previousPositions.Count > 8)
					{
						previousPositions.RemoveAt(0);
					}
				}
			}
			else
			{
				Sprite.currentFrame = 4;
				Halt();
			}
			resetAnimationSpeed();
		}
	}
}
