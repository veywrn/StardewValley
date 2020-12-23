using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Tools;
using System;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	public class ResourceClump : TerrainFeature
	{
		public const int stumpIndex = 600;

		public const int hollowLogIndex = 602;

		public const int meteoriteIndex = 622;

		public const int boulderIndex = 672;

		public const int mineRock1Index = 752;

		public const int mineRock2Index = 754;

		public const int mineRock3Index = 756;

		public const int mineRock4Index = 758;

		[XmlElement("width")]
		public readonly NetInt width = new NetInt();

		[XmlElement("height")]
		public readonly NetInt height = new NetInt();

		[XmlElement("parentSheetIndex")]
		public readonly NetInt parentSheetIndex = new NetInt();

		[XmlElement("health")]
		public readonly NetFloat health = new NetFloat();

		[XmlElement("tile")]
		public readonly NetVector2 tile = new NetVector2();

		protected float shakeTimer;

		public ResourceClump()
			: base(needsTick: true)
		{
			base.NetFields.AddFields(width, height, parentSheetIndex, health, tile);
		}

		public ResourceClump(int parentSheetIndex, int width, int height, Vector2 tile)
			: this()
		{
			this.width.Value = width;
			this.height.Value = height;
			this.parentSheetIndex.Value = parentSheetIndex;
			this.tile.Value = tile;
			switch (parentSheetIndex)
			{
			case 600:
				health.Value = 10f;
				break;
			case 602:
				health.Value = 20f;
				break;
			case 622:
				health.Value = 20f;
				break;
			case 752:
			case 754:
			case 756:
			case 758:
				health.Value = 8f;
				break;
			case 672:
				health.Value = 10f;
				break;
			}
		}

		public override bool isPassable(Character c = null)
		{
			return false;
		}

		public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
		{
			if (t == null)
			{
				return false;
			}
			int radialDebris = 12;
			switch ((int)parentSheetIndex)
			{
			case 600:
				if (t is Axe && (int)t.upgradeLevel < 1)
				{
					location.playSound("axe");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13945"));
					Game1.player.jitterStrength = 1f;
					return false;
				}
				if (!(t is Axe))
				{
					return false;
				}
				location.playSound("axchop");
				break;
			case 602:
				if (t is Axe && (int)t.upgradeLevel < 2)
				{
					location.playSound("axe");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13948"));
					Game1.player.jitterStrength = 1f;
					return false;
				}
				if (!(t is Axe))
				{
					return false;
				}
				location.playSound("axchop");
				break;
			case 622:
				if (t is Pickaxe && (int)t.upgradeLevel < 3)
				{
					location.playSound("clubhit");
					location.playSound("clank");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13952"));
					Game1.player.jitterStrength = 1f;
					return false;
				}
				if (!(t is Pickaxe))
				{
					return false;
				}
				location.playSound("hammer");
				radialDebris = 14;
				break;
			case 672:
				if (t is Pickaxe && (int)t.upgradeLevel < 2)
				{
					location.playSound("clubhit");
					location.playSound("clank");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13956"));
					Game1.player.jitterStrength = 1f;
					return false;
				}
				if (!(t is Pickaxe))
				{
					return false;
				}
				location.playSound("hammer");
				radialDebris = 14;
				break;
			case 752:
			case 754:
			case 756:
			case 758:
				if (!(t is Pickaxe))
				{
					return false;
				}
				location.playSound("hammer");
				radialDebris = 14;
				shakeTimer = 500f;
				base.NeedsUpdate = true;
				break;
			}
			float power = Math.Max(1f, (float)((int)t.upgradeLevel + 1) * 0.75f);
			health.Value -= power;
			if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(power / 12f) && ((int)parentSheetIndex == 602 || (int)parentSheetIndex == 600))
			{
				Debris d = new Debris(709, new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()));
				d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
				d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
				location.debris.Add(d);
			}
			Game1.createRadialDebris(Game1.currentLocation, radialDebris, (int)tileLocation.X + Game1.random.Next((int)width / 2 + 1), (int)tileLocation.Y + Game1.random.Next((int)height / 2 + 1), Game1.random.Next(4, 9), resource: false);
			if ((float)health <= 0f)
			{
				if (t != null && location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.05)
				{
					Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
					if (o != null)
					{
						Game1.createItemDebris(o, tileLocation * 64f, -1, location);
					}
				}
				if (Game1.IsMultiplayer)
				{
					_ = Game1.recentMultiplayerRandom;
				}
				else
				{
					new Random((int)((float)(double)Game1.uniqueIDForThisGame + tileLocation.X * 7f + tileLocation.Y * 11f + (float)(double)Game1.stats.DaysPlayed + (float)health));
				}
				switch ((int)parentSheetIndex)
				{
				case 600:
				case 602:
				{
					if (t.getLastFarmerToUse() == Game1.player)
					{
						Game1.stats.StumpsChopped++;
					}
					t.getLastFarmerToUse().gainExperience(2, 25);
					int numChunks = ((int)parentSheetIndex == 602) ? 8 : 2;
					Random r2;
					if (Game1.IsMultiplayer)
					{
						Game1.recentMultiplayerRandom = new Random((int)tileLocation.X * 1000 + (int)tileLocation.Y);
						r2 = Game1.recentMultiplayerRandom;
					}
					else
					{
						r2 = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
					}
					if (t.getLastFarmerToUse().professions.Contains(12))
					{
						if (numChunks == 8)
						{
							numChunks = 10;
						}
						else if (r2.NextDouble() < 0.5)
						{
							numChunks++;
						}
					}
					if (Game1.IsMultiplayer)
					{
						Game1.createMultipleObjectDebris(709, (int)tileLocation.X, (int)tileLocation.Y, numChunks, t.getLastFarmerToUse().UniqueMultiplayerID);
					}
					else
					{
						Game1.createMultipleObjectDebris(709, (int)tileLocation.X, (int)tileLocation.Y, numChunks);
					}
					location.playSound("stumpCrack");
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(23, tileLocation * 64f, Color.White, 4, flipped: false, 140f, 0, 128, -1f, 128));
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(385, 1522, 127, 79), 2000f, 1, 1, tileLocation * 64f + new Vector2(0f, 49f), flicker: false, flipped: false, 1E-05f, 0.016f, Color.White, 1f, 0f, 0f, 0f));
					Game1.createRadialDebris(Game1.currentLocation, 34, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 9), resource: false);
					if (r2.NextDouble() < 0.1)
					{
						Game1.createMultipleObjectDebris(292, (int)tileLocation.X, (int)tileLocation.Y, 1);
					}
					if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
					{
						Game1.createObjectDebris(890, (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
					}
					return true;
				}
				case 672:
				case 752:
				case 754:
				case 756:
				case 758:
				{
					int numChunks = ((int)parentSheetIndex == 672) ? 15 : 10;
					if (Game1.IsMultiplayer)
					{
						Game1.recentMultiplayerRandom = new Random((int)tileLocation.X * 1000 + (int)tileLocation.Y);
						Random r2 = Game1.recentMultiplayerRandom;
					}
					else
					{
						Random r2 = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
					}
					if (Game1.IsMultiplayer)
					{
						Game1.createMultipleObjectDebris(390, (int)tileLocation.X, (int)tileLocation.Y, numChunks, t.getLastFarmerToUse().UniqueMultiplayerID);
					}
					else
					{
						Game1.createRadialDebris(Game1.currentLocation, 390, (int)tileLocation.X, (int)tileLocation.Y, numChunks, resource: false, -1, item: true);
					}
					location.playSound("boulderBreak");
					Game1.createRadialDebris(Game1.currentLocation, 32, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 12), resource: false);
					Color c = Color.White;
					switch ((int)parentSheetIndex)
					{
					case 752:
						c = new Color(188, 119, 98);
						break;
					case 754:
						c = new Color(168, 120, 95);
						break;
					case 756:
					case 758:
						c = new Color(67, 189, 238);
						break;
					}
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(48, tileLocation * 64f, c, 5, flipped: false, 180f, 0, 128, -1f, 128)
					{
						alphaFade = 0.01f
					});
					return true;
				}
				case 622:
				{
					int numChunks = 6;
					if (Game1.IsMultiplayer)
					{
						Game1.recentMultiplayerRandom = new Random((int)tileLocation.X * 1000 + (int)tileLocation.Y);
						Random r2 = Game1.recentMultiplayerRandom;
					}
					else
					{
						Random r2 = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
					}
					if (Game1.IsMultiplayer)
					{
						Game1.createMultipleObjectDebris(386, (int)tileLocation.X, (int)tileLocation.Y, numChunks, t.getLastFarmerToUse().UniqueMultiplayerID);
						Game1.createMultipleObjectDebris(390, (int)tileLocation.X, (int)tileLocation.Y, numChunks, t.getLastFarmerToUse().UniqueMultiplayerID);
						Game1.createMultipleObjectDebris(535, (int)tileLocation.X, (int)tileLocation.Y, 2, t.getLastFarmerToUse().UniqueMultiplayerID);
					}
					else
					{
						Game1.createMultipleObjectDebris(386, (int)tileLocation.X, (int)tileLocation.Y, numChunks);
						Game1.createMultipleObjectDebris(390, (int)tileLocation.X, (int)tileLocation.Y, numChunks);
						Game1.createMultipleObjectDebris(535, (int)tileLocation.X, (int)tileLocation.Y, 2);
					}
					location.playSound("boulderBreak");
					Game1.createRadialDebris(Game1.currentLocation, 32, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 12), resource: false);
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, tileLocation * 64f, Color.White));
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 0f)) * 64f, Color.White, 8, flipped: false, 110f));
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 1f)) * 64f, Color.White, 8, flipped: true, 80f));
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(0f, 1f)) * 64f, Color.White, 8, flipped: false, 90f));
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, tileLocation * 64f + new Vector2(32f, 32f), Color.White, 8, flipped: false, 70f));
					return true;
				}
				}
			}
			else
			{
				shakeTimer = 100f;
				base.NeedsUpdate = true;
			}
			return false;
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, (int)width * 64, (int)height * 64);
		}

		public bool occupiesTile(int x, int y)
		{
			if ((float)x >= tile.X && (float)x - tile.X < (float)(int)width && (float)y >= tile.Y)
			{
				return (float)y - tile.Y < (float)(int)height;
			}
			return false;
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16);
			sourceRect.Width = (int)width * 16;
			sourceRect.Height = (int)height * 16;
			Vector2 position = tile.Value * 64f;
			if (shakeTimer > 0f)
			{
				position.X += (float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 4f;
			}
			spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, position), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tile.Y + 1f) * 64f / 10000f + tile.X / 100000f);
		}

		public override void loadSprite()
		{
		}

		public override bool performUseAction(Vector2 tileLocation, GameLocation location)
		{
			if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
			{
				Game1.haltAfterCheck = false;
				return false;
			}
			switch ((int)parentSheetIndex)
			{
			case 602:
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13962")));
				return true;
			case 672:
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13963")));
				return true;
			case 622:
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13964")));
				return true;
			default:
				return false;
			}
		}

		public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
		{
			if (shakeTimer > 0f)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				base.NeedsUpdate = false;
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
	}
}
