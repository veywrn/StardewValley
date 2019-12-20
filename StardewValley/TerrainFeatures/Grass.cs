using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Tools;
using System;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	public class Grass : TerrainFeature
	{
		public const float defaultShakeRate = (float)Math.PI / 80f;

		public const float maximumShake = (float)Math.PI / 8f;

		public const float shakeDecayRate = (float)Math.PI / 350f;

		public const byte springGrass = 1;

		public const byte caveGrass = 2;

		public const byte frostGrass = 3;

		public const byte lavaGrass = 4;

		public static ICue grassSound;

		[XmlElement("grassType")]
		public readonly NetByte grassType = new NetByte();

		private bool shakeLeft;

		protected float shakeRotation;

		protected float maxShake;

		protected float shakeRate;

		[XmlElement("numberOfWeeds")]
		public readonly NetInt numberOfWeeds = new NetInt();

		[XmlElement("grassSourceOffset")]
		public readonly NetInt grassSourceOffset = new NetInt();

		protected Lazy<Texture2D> texture;

		private int[] whichWeed = new int[4];

		private int[] offset1 = new int[4];

		private int[] offset2 = new int[4];

		private int[] offset3 = new int[4];

		private int[] offset4 = new int[4];

		private bool[] flip = new bool[4];

		private double[] shakeRandom = new double[4];

		public Grass()
			: base(needsTick: true)
		{
			texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>(textureName()));
			base.NetFields.AddFields(grassType, numberOfWeeds, grassSourceOffset);
		}

		public Grass(int which, int numberOfWeeds)
			: this()
		{
			grassType.Value = (byte)which;
			loadSprite();
			this.numberOfWeeds.Value = numberOfWeeds;
		}

		protected virtual string textureName()
		{
			return "TerrainFeatures\\grass";
		}

		public override bool isPassable(Character c = null)
		{
			return true;
		}

		public override void loadSprite()
		{
			try
			{
				if (Game1.soundBank != null)
				{
					grassSound = Game1.soundBank.GetCue("grassyStep");
				}
				if ((byte)grassType == 1)
				{
					string currentSeason = Game1.currentSeason;
					if (!(currentSeason == "spring"))
					{
						if (!(currentSeason == "summer"))
						{
							if (currentSeason == "fall")
							{
								grassSourceOffset.Value = 40;
							}
						}
						else
						{
							grassSourceOffset.Value = 20;
						}
					}
					else
					{
						grassSourceOffset.Value = 0;
					}
				}
				else if ((byte)grassType == 2)
				{
					grassSourceOffset.Value = 60;
				}
				else if ((byte)grassType == 3)
				{
					grassSourceOffset.Value = 80;
				}
				else if ((byte)grassType == 4)
				{
					grassSourceOffset.Value = 100;
				}
			}
			catch (Exception)
			{
			}
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64);
		}

		public override Rectangle getRenderBounds(Vector2 tileLocation)
		{
			return new Rectangle((int)(tileLocation.X * 64f) - 32, (int)(tileLocation.Y * 64f) - 32, 128, 112);
		}

		public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who, GameLocation location)
		{
			if (location != Game1.currentLocation)
			{
				return;
			}
			if (speedOfCollision > 0 && maxShake == 0f && positionOfCollider.Intersects(getBoundingBox(tileLocation)))
			{
				if ((who == null || !(who is FarmAnimal)) && grassSound != null && !grassSound.IsPlaying && Utility.isOnScreen(new Point((int)tileLocation.X, (int)tileLocation.Y), 2, location) && Game1.soundBank != null)
				{
					grassSound = Game1.soundBank.GetCue("grassyStep");
					grassSound.Play();
				}
				shake((float)Math.PI / 8f / (float)((5 + Game1.player.addedSpeed) / speedOfCollision), (float)Math.PI / 80f / (float)((5 + Game1.player.addedSpeed) / speedOfCollision), (float)positionOfCollider.Center.X > tileLocation.X * 64f + 32f);
			}
			if (who is Farmer && Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon && ((MeleeWeapon)Game1.player.CurrentTool).isOnSpecial && (int)((MeleeWeapon)Game1.player.CurrentTool).type == 0 && Math.Abs(shakeRotation) < 0.001f && performToolAction(Game1.player.CurrentTool, -1, tileLocation, location))
			{
				Game1.currentLocation.terrainFeatures.Remove(tileLocation);
			}
			if (who is Farmer)
			{
				(who as Farmer).temporarySpeedBuff = -1f;
			}
		}

		public bool reduceBy(int number, Vector2 tileLocation, bool showDebris)
		{
			numberOfWeeds.Value -= number;
			if (showDebris)
			{
				Game1.createRadialDebris(Game1.currentLocation, textureName(), new Rectangle(2, 8, 8, 8), 1, ((int)tileLocation.X + 1) * 64, ((int)tileLocation.Y + 1) * 64, Game1.random.Next(6, 14), (int)tileLocation.Y + 1, Color.White, 4f);
			}
			return (int)numberOfWeeds <= 0;
		}

		protected void shake(float shake, float rate, bool left)
		{
			maxShake = shake;
			shakeRate = rate;
			shakeRotation = 0f;
			shakeLeft = left;
			base.NeedsUpdate = true;
		}

		public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
		{
			if (shakeRandom[0] == 0.0)
			{
				setUpRandom(tileLocation);
			}
			if (maxShake > 0f)
			{
				if (shakeLeft)
				{
					shakeRotation -= shakeRate;
					if (Math.Abs(shakeRotation) >= maxShake)
					{
						shakeLeft = false;
					}
				}
				else
				{
					shakeRotation += shakeRate;
					if (shakeRotation >= maxShake)
					{
						shakeLeft = true;
						shakeRotation -= shakeRate;
					}
				}
				maxShake = Math.Max(0f, maxShake - (float)Math.PI / 350f);
			}
			else
			{
				shakeRotation /= 2f;
				if (shakeRotation <= 0.01f)
				{
					base.NeedsUpdate = false;
					shakeRotation = 0f;
				}
			}
			return false;
		}

		public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
		{
			if ((byte)grassType == 1 && !Game1.currentSeason.Equals("winter") && (int)numberOfWeeds < 4)
			{
				numberOfWeeds.Value = Utility.Clamp(numberOfWeeds.Value + Game1.random.Next(1, 4), 0, 4);
			}
			setUpRandom(tileLocation);
		}

		public void setUpRandom(Vector2 tileLocation)
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed / 28 + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
			for (int i = 0; i < 4; i++)
			{
				whichWeed[i] = r.Next(3);
				offset1[i] = r.Next(-2, 3);
				offset2[i] = r.Next(-2, 3);
				offset3[i] = r.Next(-2, 3);
				offset4[i] = r.Next(-2, 3);
				flip[i] = (r.NextDouble() < 0.5);
				shakeRandom[i] = r.NextDouble();
			}
		}

		public override bool seasonUpdate(bool onLoad)
		{
			if ((byte)grassType == 1 && Game1.currentSeason.Equals("winter"))
			{
				return true;
			}
			if ((byte)grassType == 1)
			{
				loadSprite();
			}
			return false;
		}

		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			if ((t != null && t is MeleeWeapon && (int)((MeleeWeapon)t).type != 2) || explosion > 0)
			{
				if (t != null && (int)(t as MeleeWeapon).type != 1)
				{
					DelayedAction.playSoundAfterDelay("daggerswipe", 50);
				}
				else
				{
					location.playSound("swordswipe");
				}
				shake((float)Math.PI * 3f / 32f, (float)Math.PI / 40f, Game1.random.NextDouble() < 0.5);
				int numberOfWeedsToDestroy2 = 0;
				numberOfWeedsToDestroy2 = ((explosion <= 0) ? 1 : Math.Max(1, explosion + 2 - Game1.recentMultiplayerRandom.Next(2)));
				if (t is MeleeWeapon && t.InitialParentTileIndex == 53)
				{
					numberOfWeedsToDestroy2 = 2;
				}
				numberOfWeeds.Value = (int)numberOfWeeds - numberOfWeedsToDestroy2;
				Color c = Color.Green;
				switch ((byte)grassType)
				{
				case 1:
					switch (Game1.currentSeason)
					{
					case "spring":
						c = new Color(60, 180, 58);
						break;
					case "summer":
						c = new Color(110, 190, 24);
						break;
					case "fall":
						c = new Color(219, 102, 58);
						break;
					}
					break;
				case 2:
					c = new Color(148, 146, 71);
					break;
				case 3:
					c = new Color(216, 240, 255);
					break;
				case 4:
					c = new Color(165, 93, 58);
					break;
				}
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(28, tileLocation * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), c, 8, Game1.random.NextDouble() < 0.5, Game1.random.Next(60, 100)));
				if ((int)numberOfWeeds <= 0)
				{
					if ((byte)grassType != 1)
					{
						Random grassRandom = Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : new Random((int)((float)(double)Game1.uniqueIDForThisGame + tileLocation.X * 1000f + tileLocation.Y * 11f + (float)Game1.CurrentMineLevel + (float)Game1.player.timesReachedMineBottom));
						if (grassRandom.NextDouble() < 0.005)
						{
							Game1.createObjectDebris(114, (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, location);
						}
						else if (grassRandom.NextDouble() < 0.01)
						{
							Game1.createDebris(4, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(1, 2), location);
						}
						else if (grassRandom.NextDouble() < 0.02)
						{
							Game1.createDebris(92, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(2, 4), location);
						}
					}
					else if (t is MeleeWeapon && (t.Name.Contains("Scythe") || (t as MeleeWeapon).isScythe()))
					{
						Random obj = Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : new Random((int)((float)(double)Game1.uniqueIDForThisGame + tileLocation.X * 1000f + tileLocation.Y * 11f));
						double chance = (t.InitialParentTileIndex == 53) ? 0.75 : 0.5;
						if (obj.NextDouble() < chance && (Game1.getLocationFromName("Farm") as Farm).tryToAddHay(1) == 0)
						{
							TemporaryAnimatedSprite tmpSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, t.getLastFarmerToUse().Position - new Vector2(0f, 128f), flicker: false, flipped: false, t.getLastFarmerToUse().Position.Y / 10000f, 0.005f, Color.White, 4f, -0.005f, 0f, 0f);
							tmpSprite.motion.Y = -1f;
							tmpSprite.layerDepth = 1f - (float)Game1.random.Next(100) / 10000f;
							tmpSprite.delayBeforeAnimationStart = Game1.random.Next(350);
							Game1.multiplayer.broadcastSprites(t.getLastFarmerToUse().currentLocation, tmpSprite);
							Game1.addHUDMessage(new HUDMessage("Hay", 1, add: true, Color.LightGoldenrodYellow, new Object(178, 1)));
						}
					}
					return true;
				}
			}
			return false;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed / 28 + (int)positionOnScreen.X * 7 + (int)positionOnScreen.Y * 11);
			for (int i = 0; i < (int)numberOfWeeds; i++)
			{
				int whichWeed = r.Next(3);
				spriteBatch.Draw(position: (i != 4) ? (tileLocation * 64f + new Vector2((float)(i % 2 * 64 / 2 + r.Next(-2, 2) * 4 - 4) + 30f, i / 2 * 64 / 2 + r.Next(-2, 2) * 4 + 40)) : (tileLocation * 64f + new Vector2((float)(16 + r.Next(-2, 2) * 4 - 4) + 30f, 16 + r.Next(-2, 2) * 4 + 40)), texture: texture.Value, sourceRectangle: new Rectangle(whichWeed * 15, grassSourceOffset, 15, 20), color: Color.White, rotation: shakeRotation / (float)(r.NextDouble() + 1.0), origin: Vector2.Zero, scale: scale, effects: SpriteEffects.None, layerDepth: layerDepth + (32f * scale + 300f) / 20000f);
			}
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			for (int i = 0; i < (int)numberOfWeeds; i++)
			{
				Vector2 pos = (i != 4) ? (tileLocation * 64f + new Vector2((float)(i % 2 * 64 / 2 + offset3[i] * 4 - 4) + 30f, i / 2 * 64 / 2 + offset4[i] * 4 + 40)) : (tileLocation * 64f + new Vector2((float)(16 + offset1[i] * 4 - 4) + 30f, 16 + offset2[i] * 4 + 40));
				spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, pos), new Rectangle(whichWeed[i] * 15, grassSourceOffset, 15, 20), Color.White, shakeRotation / (float)(shakeRandom[i] + 1.0), new Vector2(7.5f, 17.5f), 4f, flip[i] ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (pos.Y + 16f - 20f) / 10000f + pos.X / 1E+07f);
			}
		}
	}
}
