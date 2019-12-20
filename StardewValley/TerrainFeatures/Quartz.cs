using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	public class Quartz : TerrainFeature
	{
		public const float shakeRate = (float)Math.PI / 200f;

		public const float shakeDecayRate = 0.00306796166f;

		public const double chanceForDiamond = 0.02;

		public const double chanceForPrismaticShard = 0.005;

		public const double chanceForIridium = 0.007;

		public const double chanceForLevelUnique = 0.03;

		public const double chanceForRefinedQuartz = 0.04;

		public const int startingHealth = 10;

		public const int large = 3;

		public const int medium = 2;

		public const int small = 1;

		public const int tiny = 0;

		public const int pointingLeft = 0;

		public const int pointingUp = 1;

		public const int pointingRight = 2;

		private Texture2D texture;

		[XmlElement("health")]
		public readonly NetFloat health = new NetFloat();

		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		private bool shakeLeft;

		[XmlElement("falling")]
		private readonly NetBool falling = new NetBool();

		private float shakeRotation;

		private float maxShake;

		[XmlElement("glow")]
		private readonly NetFloat glow = new NetFloat(0f);

		[XmlElement("bigness")]
		public readonly NetInt bigness = new NetInt();

		private int identifier;

		[XmlElement("color")]
		private readonly NetColor color = new NetColor();

		public Quartz()
			: base(needsTick: true)
		{
			base.NetFields.AddFields(health, flipped, falling, glow, bigness, color);
			loadSprite();
		}

		public Quartz(int bigness, Color color)
			: this()
		{
			if (bigness > 3)
			{
				this.bigness.Value = 2;
			}
			health.Value = 10 - (3 - bigness) * 2;
			this.bigness.Value = bigness;
			this.color.Value = color;
		}

		public override void loadSprite()
		{
			try
			{
				texture = Game1.content.Load<Texture2D>("TerrainFeatures\\Quartz");
			}
			catch (Exception)
			{
			}
			identifier = Game1.random.Next(-999999, 999999);
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			int num = bigness;
			if (num == 3)
			{
				return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 128, 128);
			}
			return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64);
		}

		public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
		{
			if ((float)glow > 0f)
			{
				glow.Value -= 0.01f;
				LightSource ls = Utility.getLightSource((int)(tileLocation.X * 1000f + tileLocation.Y));
				if (ls != null)
				{
					ls.color.Value = new Color(255 - color.R, 255 - color.G, 255 - color.B, (int)(255f * (float)glow));
				}
				if (glow.Value <= 0f)
				{
					Utility.removeLightSource((int)(tileLocation.X * 1000f + tileLocation.Y));
				}
			}
			if (maxShake > 0f)
			{
				if (shakeLeft)
				{
					shakeRotation -= (float)Math.PI / 200f;
					if (shakeRotation <= 0f - maxShake)
					{
						shakeLeft = false;
					}
				}
				else
				{
					shakeRotation += (float)Math.PI / 200f;
					if (shakeRotation >= maxShake)
					{
						shakeLeft = true;
					}
				}
			}
			if (maxShake > 0f)
			{
				maxShake = Math.Max(0f, maxShake - 0.00306796166f);
			}
			if ((float)health <= 0f)
			{
				return true;
			}
			return false;
		}

		public override void performPlayerEntryAction(Vector2 tileLocation)
		{
		}

		private void shake(Vector2 tileLocation)
		{
			if (maxShake == 0f)
			{
				shakeLeft = (Game1.player.getTileLocation().X > tileLocation.X || ((Game1.player.getTileLocation().X == tileLocation.X && Game1.random.NextDouble() < 0.5) ? true : false));
				maxShake = (float)Math.PI / 128f;
			}
		}

		public override bool performUseAction(Vector2 tileLocation, GameLocation location)
		{
			if (Game1.soundBank != null)
			{
				Random random = new Random((int)((float)(double)Game1.uniqueIDForThisGame + tileLocation.X * 7f + tileLocation.Y * 11f + (float)Game1.CurrentMineLevel));
				ICue c = Game1.soundBank.GetCue("crystal");
				int pitch2 = random.Next(2400);
				pitch2 -= pitch2 % 100;
				c.SetVariable("Pitch", pitch2);
				c.Play();
			}
			glow.Value = 0.7f;
			Color glowColor = (glow.Value > 0f) ? new Color(color.R + (int)(glow.Value * 50f), color.G + (int)(glow.Value * 50f), color.B + (int)(glow.Value * 50f)) : ((Color)color);
			glowColor *= 0.3f + glow.Value;
			Utility.removeLightSource((int)(tileLocation.X * 1000f + tileLocation.Y));
			if ((int)bigness < 2)
			{
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), 1f, Utility.getOppositeColor(glowColor), (int)(tileLocation.X * 1000f + tileLocation.Y), LightSource.LightContext.None, 0L));
			}
			else if ((int)bigness >= 2)
			{
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), 1f, Utility.getOppositeColor(glowColor), (int)(tileLocation.X * 1000f + tileLocation.Y), LightSource.LightContext.None, 0L));
			}
			return false;
		}

		public override bool isPassable(Character c = null)
		{
			if ((float)health <= 0f)
			{
				return true;
			}
			return false;
		}

		public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
		{
		}

		public override bool seasonUpdate(bool onLoad)
		{
			return false;
		}

		private Rectangle getSourceRect(int size)
		{
			switch (size)
			{
			case 3:
				return new Rectangle((int)((10f - (float)health) / 3f) * 16 * 2, 32, 32, 48);
			case 2:
				return new Rectangle((int)((8f - (float)health) / 2f) * 16, 0, 16, 32);
			case 1:
				return new Rectangle(64 + (((float)health <= 3f) ? 16 : 0), 16, 16, 16);
			case 0:
				return new Rectangle(16, 0, 16, 16);
			default:
				return Rectangle.Empty;
			}
		}

		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location = null)
		{
			if ((float)health > 0f)
			{
				float damage = 0f;
				if (t == null && explosion > 0)
				{
					damage = explosion;
				}
				else if (t.BaseName.Contains("Pickaxe"))
				{
					switch ((int)t.upgradeLevel)
					{
					case 0:
						damage = 2f;
						break;
					case 1:
						damage = 2.5f;
						break;
					case 2:
						damage = 3.34f;
						break;
					case 3:
						damage = 5f;
						break;
					case 4:
						damage = 10f;
						break;
					}
					Game1.playSound("hammer");
				}
				if (damage > 0f)
				{
					glow.Value = 0.7f;
					shake(tileLocation);
					health.Value -= damage;
					glow.Value = 0.25f;
					Color glowColor = new Color(255 - color.R, 255 - color.G, 255 - color.B, (int)(255f * (float)glow));
					Utility.removeLightSource((int)(tileLocation.X * 1000f + tileLocation.Y));
					if ((int)bigness < 2)
					{
						Game1.currentLightSources.Add(new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), 1f, Utility.getOppositeColor(glowColor), (int)(tileLocation.X * 1000f + tileLocation.Y), LightSource.LightContext.None, 0L));
					}
					else if ((int)bigness == 2)
					{
						Game1.currentLightSources.Add(new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), 1f, Utility.getOppositeColor(glowColor), (int)(tileLocation.X * 1000f + tileLocation.Y), LightSource.LightContext.None, 0L));
					}
					if ((float)health <= 0f)
					{
						Random crystalRandom = new Random((int)((float)(double)Game1.uniqueIDForThisGame + tileLocation.X * 7f + tileLocation.Y * 11f + (float)Game1.CurrentMineLevel + (float)Game1.player.timesReachedMineBottom));
						double luckModifier = 1.0 + Game1.player.team.AverageDailyLuck(Game1.currentLocation) + Game1.player.team.AverageLuckLevel(Game1.currentLocation) / 100.0 + (double)(int)Game1.player.miningLevel / 50.0;
						if (crystalRandom.NextDouble() < 0.005 * luckModifier)
						{
							Game1.createObjectDebris(74, (int)tileLocation.X, (int)tileLocation.Y, location);
						}
						else if (crystalRandom.NextDouble() < 0.007 * luckModifier)
						{
							Game1.createDebris(10, (int)tileLocation.X, (int)tileLocation.Y, 2, location);
						}
						else if (crystalRandom.NextDouble() < 0.02 * luckModifier)
						{
							Game1.createObjectDebris(72, (int)tileLocation.X, (int)tileLocation.Y, location);
						}
						else if (crystalRandom.NextDouble() < 0.03 * luckModifier)
						{
							Game1.createObjectDebris((Game1.CurrentMineLevel < 40) ? 86 : ((Game1.CurrentMineLevel < 80) ? 84 : 82), (int)tileLocation.X, (int)tileLocation.Y, location);
						}
						else if (crystalRandom.NextDouble() < 0.04 * luckModifier)
						{
							Game1.createObjectDebris(338, (int)tileLocation.X, (int)tileLocation.Y, location);
						}
						for (int i = 0; i < (int)bigness * 3; i++)
						{
							int xPos = Game1.random.Next(getBoundingBox(tileLocation).X, getBoundingBox(tileLocation).Right);
							int yPos = Game1.random.Next(getBoundingBox(tileLocation).Y, getBoundingBox(tileLocation).Bottom);
							Game1.currentLocation.TemporarySprites.Add(new CosmeticDebris(texture, new Vector2(xPos, yPos), (float)Game1.random.Next(-25, 25) / 100f, (float)(xPos - getBoundingBox(tileLocation).Center.X) / 30f, (float)Game1.random.Next(-800, -100) / 100f, (int)tileLocation.Y * 64 + 64, new Rectangle(Game1.random.Next(4, 8) * 16, 0, 16, 16), color, (Game1.soundBank != null) ? Game1.soundBank.GetCue("boulderCrack") : null, new LightSource(4, Vector2.Zero, 0.25f, glowColor, LightSource.LightContext.None, 0L), 24, 1000));
						}
						Utility.removeLightSource((int)(tileLocation.X * 1000f + tileLocation.Y));
					}
				}
			}
			return false;
		}

		private Vector2 getPivot()
		{
			switch ((int)bigness)
			{
			case 3:
				return new Vector2(16f, 48f);
			case 2:
				return new Vector2(8f, 32f);
			case 1:
				return new Vector2(8f, 16f);
			default:
				return Vector2.Zero;
			}
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			if ((float)health > 0f)
			{
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(getBoundingBox(tileLocation).Center.X, getBoundingBox(tileLocation).Bottom)), getSourceRect(bigness), color, shakeRotation, getPivot(), 4f, SpriteEffects.None, (tileLocation.Y * 64f + 64f) / 10000f);
			}
		}
	}
}
