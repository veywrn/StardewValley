using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	public class Bush : LargeTerrainFeature
	{
		public const float shakeRate = (float)Math.PI / 200f;

		public const float shakeDecayRate = 0.00306796166f;

		public const int smallBush = 0;

		public const int mediumBush = 1;

		public const int largeBush = 2;

		public const int greenTeaBush = 3;

		public const int walnutBush = 4;

		public const int daysToMatureGreenTeaBush = 20;

		[XmlElement("size")]
		public readonly NetInt size = new NetInt();

		[XmlElement("datePlanted")]
		public readonly NetInt datePlanted = new NetInt();

		[XmlElement("tileSheetOffset")]
		public readonly NetInt tileSheetOffset = new NetInt();

		[XmlElement("overrideSeason")]
		public readonly NetInt overrideSeason = new NetInt(-1);

		public float health;

		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		[XmlElement("townBush")]
		public readonly NetBool townBush = new NetBool();

		[XmlElement("greenhouseBush")]
		public readonly NetBool greenhouseBush = new NetBool();

		[XmlElement("drawShadow")]
		public readonly NetBool drawShadow = new NetBool(value: true);

		private bool shakeLeft;

		private float shakeRotation;

		private float maxShake;

		private float alpha = 1f;

		private long lastPlayerToHit;

		private float shakeTimer;

		[XmlElement("sourceRect")]
		private readonly NetRectangle sourceRect = new NetRectangle();

		[XmlIgnore]
		public NetMutex uniqueSpawnMutex = new NetMutex();

		public static Lazy<Texture2D> texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>("TileSheets\\bushes"));

		public static Rectangle treeTopSourceRect = new Rectangle(0, 0, 48, 96);

		public static Rectangle stumpSourceRect = new Rectangle(32, 96, 16, 32);

		public static Rectangle shadowSourceRect = new Rectangle(663, 1011, 41, 30);

		private float yDrawOffset;

		public Bush()
			: base(needsTick: true)
		{
			base.NetFields.AddFields(size, tileSheetOffset, flipped, townBush, drawShadow, sourceRect, datePlanted, greenhouseBush, overrideSeason, uniqueSpawnMutex.NetFields);
		}

		public Bush(Vector2 tileLocation, int size, GameLocation location, int datePlantedOverride = -1)
			: this()
		{
			tilePosition.Value = tileLocation;
			this.size.Value = size;
			if (location is Town && tileLocation.X % 5f != 0f)
			{
				townBush.Value = true;
			}
			if (location.map.GetLayer("Front").Tiles[(int)tileLocation.X, (int)tileLocation.Y] != null)
			{
				drawShadow.Value = false;
			}
			datePlanted.Value = ((datePlantedOverride == -1) ? ((int)Game1.stats.DaysPlayed) : datePlantedOverride);
			if (size == 3)
			{
				drawShadow.Value = false;
			}
			if (location.IsGreenhouse)
			{
				greenhouseBush.Value = true;
			}
			if (size == 4)
			{
				tileSheetOffset.Value = 1;
				overrideSeason.Value = 1;
			}
			GameLocation old_location = currentLocation;
			currentLocation = location;
			loadSprite();
			currentLocation = old_location;
			flipped.Value = (Game1.random.NextDouble() < 0.5);
		}

		public int getAge()
		{
			return (int)Game1.stats.DaysPlayed - datePlanted.Value;
		}

		public void setUpSourceRect()
		{
			int seasonNumber = ((int)overrideSeason == -1) ? Utility.getSeasonNumber(Game1.GetSeasonForLocation(currentLocation)) : ((int)overrideSeason);
			if (greenhouseBush.Value)
			{
				seasonNumber = 0;
			}
			if ((int)size == 0)
			{
				sourceRect.Value = new Rectangle(seasonNumber * 16 * 2 + (int)tileSheetOffset * 16, 224, 16, 32);
			}
			else if ((int)size == 1)
			{
				if ((bool)townBush)
				{
					sourceRect.Value = new Rectangle(seasonNumber * 16 * 2, 96, 32, 32);
				}
				else
				{
					sourceRect.Value = new Rectangle((seasonNumber * 16 * 4 + (int)tileSheetOffset * 16 * 2) % texture.Value.Bounds.Width, (seasonNumber * 16 * 4 + (int)tileSheetOffset * 16 * 2) / texture.Value.Bounds.Width * 3 * 16, 32, 48);
				}
			}
			else if ((int)size == 2)
			{
				if ((bool)townBush && (seasonNumber == 0 || seasonNumber == 1))
				{
					sourceRect.Value = new Rectangle(48, 176, 48, 48);
					return;
				}
				switch (seasonNumber)
				{
				case 0:
				case 1:
					sourceRect.Value = new Rectangle(0, 128, 48, 48);
					break;
				case 2:
					sourceRect.Value = new Rectangle(48, 128, 48, 48);
					break;
				case 3:
					sourceRect.Value = new Rectangle(0, 176, 48, 48);
					break;
				}
			}
			else if ((int)size == 3)
			{
				int age = getAge();
				switch (seasonNumber)
				{
				case 0:
					sourceRect.Value = new Rectangle(Math.Min(2, age / 10) * 16 + (int)tileSheetOffset * 16, 256, 16, 32);
					break;
				case 1:
					sourceRect.Value = new Rectangle(64 + Math.Min(2, age / 10) * 16 + (int)tileSheetOffset * 16, 256, 16, 32);
					break;
				case 2:
					sourceRect.Value = new Rectangle(Math.Min(2, age / 10) * 16 + (int)tileSheetOffset * 16, 288, 16, 32);
					break;
				case 3:
					sourceRect.Value = new Rectangle(64 + Math.Min(2, age / 10) * 16 + (int)tileSheetOffset * 16, 288, 16, 32);
					break;
				}
			}
			else if ((int)size == 4)
			{
				sourceRect.Value = new Rectangle(tileSheetOffset.Value * 32, 320, 32, 32);
			}
		}

		public bool inBloom(string season, int dayOfMonth)
		{
			if ((int)size == 4)
			{
				return tileSheetOffset.Value == 1;
			}
			if ((int)overrideSeason != -1)
			{
				season = Utility.getSeasonNameFromNumber(overrideSeason);
			}
			if ((int)size == 3)
			{
				if (getAge() >= 20 && dayOfMonth >= 22 && (!season.Equals("winter") || (bool)greenhouseBush))
				{
					return true;
				}
				return false;
			}
			if (season.Equals("spring"))
			{
				if (dayOfMonth > 14 && dayOfMonth < 19)
				{
					return true;
				}
			}
			else if (season.Equals("fall") && dayOfMonth > 7 && dayOfMonth < 12)
			{
				return true;
			}
			return false;
		}

		public override bool isActionable()
		{
			return true;
		}

		public override void loadSprite()
		{
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame + (int)tilePosition.X + (int)tilePosition.Y * 777);
			if ((int)size != 4)
			{
				if ((int)size == 1 && (int)tileSheetOffset == 0 && r.NextDouble() < 0.5 && inBloom(Game1.GetSeasonForLocation(currentLocation), Game1.dayOfMonth))
				{
					tileSheetOffset.Value = 1;
				}
				else if (!Game1.GetSeasonForLocation(currentLocation).Equals("summer") && !inBloom(Game1.GetSeasonForLocation(currentLocation), Game1.dayOfMonth))
				{
					tileSheetOffset.Value = 0;
				}
			}
			if ((int)size == 3)
			{
				tileSheetOffset.Value = (inBloom(Game1.GetSeasonForLocation(currentLocation), Game1.dayOfMonth) ? 1 : 0);
			}
			setUpSourceRect();
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			switch ((int)size)
			{
			case 0:
			case 3:
				return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			case 1:
			case 4:
				return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 128, 64);
			case 2:
				return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 192, 64);
			default:
				return Rectangle.Empty;
			}
		}

		public override Rectangle getRenderBounds(Vector2 tileLocation)
		{
			switch ((int)size)
			{
			case 0:
			case 3:
				return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 1f) * 64, 64, 160);
			case 1:
			case 4:
				return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 2f) * 64, 128, 256);
			case 2:
				return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 2f) * 64, 192, 256);
			default:
				return Rectangle.Empty;
			}
		}

		public override bool performUseAction(Vector2 tileLocation, GameLocation location)
		{
			string season = ((int)overrideSeason == -1) ? Game1.GetSeasonForLocation(location) : Utility.getSeasonNameFromNumber(overrideSeason);
			if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
			{
				shakeTimer = 0f;
			}
			if (shakeTimer <= 0f)
			{
				if (maxShake == 0f && ((bool)greenhouseBush || (int)size != 3 || !season.Equals("winter")))
				{
					location.localSound("leafrustle");
				}
				shake(tileLocation, doEvenIfStillShaking: false);
				shakeTimer = 500f;
			}
			return true;
		}

		public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
		{
			if (shakeTimer > 0f)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if ((int)size == 4)
			{
				uniqueSpawnMutex.Update(location);
			}
			alpha = Math.Min(1f, alpha + 0.05f);
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
			return false;
		}

		private void shake(Vector2 tileLocation, bool doEvenIfStillShaking)
		{
			if (!((maxShake == 0f) | doEvenIfStillShaking))
			{
				return;
			}
			shakeLeft = (Game1.player.getTileLocation().X > tileLocation.X || ((Game1.player.getTileLocation().X == tileLocation.X && Game1.random.NextDouble() < 0.5) ? true : false));
			maxShake = (float)Math.PI / 128f;
			if (!townBush && (int)tileSheetOffset == 1 && inBloom(Game1.GetSeasonForLocation(currentLocation), Game1.dayOfMonth))
			{
				string season = ((int)overrideSeason == -1) ? Game1.GetSeasonForLocation(currentLocation) : Utility.getSeasonNameFromNumber(overrideSeason);
				int shakeOff = -1;
				if (!(season == "spring"))
				{
					if (season == "fall")
					{
						shakeOff = 410;
					}
				}
				else
				{
					shakeOff = 296;
				}
				if ((int)size == 3)
				{
					shakeOff = 815;
				}
				if ((int)size == 4)
				{
					shakeOff = 73;
				}
				if (shakeOff == -1)
				{
					return;
				}
				tileSheetOffset.Value = 0;
				setUpSourceRect();
				Random r = new Random((int)tileLocation.X + (int)tileLocation.Y * 5000 + (int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
				if ((int)size == 3 || (int)size == 4)
				{
					int number = 1;
					for (int i = 0; i < number; i++)
					{
						if ((int)size == 4)
						{
							uniqueSpawnMutex.RequestLock(delegate
							{
								Game1.player.team.MarkCollectedNut("Bush_" + currentLocation.Name + "_" + tileLocation.X + "_" + tileLocation.Y);
								Game1.createItemDebris(new Object(shakeOff, 1), new Vector2(getBoundingBox().Center.X, getBoundingBox().Bottom - 2), 0, currentLocation, getBoundingBox().Bottom);
							});
						}
						else
						{
							Game1.createObjectDebris(shakeOff, (int)tileLocation.X, (int)tileLocation.Y);
						}
					}
				}
				else
				{
					int number2 = r.Next(1, 2) + Game1.player.ForagingLevel / 4;
					for (int j = 0; j < number2; j++)
					{
						Game1.createItemDebris(new Object(shakeOff, 1, isRecipe: false, -1, Game1.player.professions.Contains(16) ? 4 : 0), Utility.PointToVector2(getBoundingBox().Center), Game1.random.Next(1, 4));
					}
				}
				if ((int)size != 3)
				{
					DelayedAction.playSoundAfterDelay("leafrustle", 100);
				}
			}
			else if (tileLocation.X == 20f && tileLocation.Y == 8f && Game1.dayOfMonth == 28 && Game1.timeOfDay == 1200 && !Game1.player.mailReceived.Contains("junimoPlush"))
			{
				Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Furniture(1733, Vector2.Zero), junimoPlushCallback);
			}
			else if (tileLocation.X == 28f && tileLocation.Y == 14f && Game1.player.eventsSeen.Contains(520702) && !Game1.player.hasMagnifyingGlass && Game1.currentLocation is Town)
			{
				(Game1.currentLocation as Town).initiateMagnifyingGlassGet();
			}
			else if (tileLocation.X == 47f && tileLocation.Y == 100f && Game1.player.secretNotesSeen.Contains(21) && Game1.timeOfDay == 2440 && Game1.currentLocation is Town && !Game1.player.mailReceived.Contains("secretNote21_done"))
			{
				Game1.player.mailReceived.Add("secretNote21_done");
				(Game1.currentLocation as Town).initiateMarnieLewisBush();
			}
		}

		public void junimoPlushCallback(Item item, Farmer who)
		{
			if (item != null && item is Furniture && (int)(item as Furniture).parentSheetIndex == 1733)
			{
				who?.mailReceived.Add("junimoPlush");
			}
		}

		public override bool isPassable(Character c = null)
		{
			return c is JunimoHarvester;
		}

		public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
		{
			string season = ((int)overrideSeason == -1) ? Game1.GetSeasonForLocation(environment) : Utility.getSeasonNameFromNumber(overrideSeason);
			if ((int)size != 4)
			{
				if ((int)size == 1 && (int)tileSheetOffset == 0 && Game1.random.NextDouble() < 0.2 && inBloom(season, Game1.dayOfMonth))
				{
					tileSheetOffset.Value = 1;
				}
				else if (!season.Equals("summer") && !inBloom(season, Game1.dayOfMonth))
				{
					tileSheetOffset.Value = 0;
				}
				if ((int)size == 3)
				{
					tileSheetOffset.Value = (inBloom(season, Game1.dayOfMonth) ? 1 : 0);
				}
				setUpSourceRect();
				if (tileLocation.X != 6f || tileLocation.Y != 7f || !(environment.Name == "Sunroom"))
				{
					health = 0f;
				}
			}
		}

		public override bool seasonUpdate(bool onLoad)
		{
			if ((int)size == 4)
			{
				return false;
			}
			if (!Game1.IsMultiplayer || Game1.IsServer)
			{
				string season = ((int)overrideSeason == -1) ? Game1.GetSeasonForLocation(currentLocation) : Utility.getSeasonNameFromNumber(overrideSeason);
				if ((int)size == 1 && season.Equals("summer") && Game1.random.NextDouble() < 0.5)
				{
					tileSheetOffset.Value = 1;
				}
				else
				{
					tileSheetOffset.Value = 0;
				}
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
			if ((int)size == 4)
			{
				return false;
			}
			if (explosion > 0)
			{
				shake(tileLocation, doEvenIfStillShaking: true);
				return false;
			}
			if (t != null && t is Axe && isDestroyable(location, tileLocation))
			{
				location.playSound("leafrustle");
				shake(tileLocation, doEvenIfStillShaking: true);
				if ((int)(t as Axe).upgradeLevel >= 1 || (int)size == 3)
				{
					health -= (((int)size == 3) ? 0.5f : ((float)(int)(t as Axe).upgradeLevel / 5f));
					if (health <= -1f)
					{
						location.playSound("treethud");
						DelayedAction.playSoundAfterDelay("leafrustle", 100);
						Color c = Color.Green;
						string season = ((int)overrideSeason == -1) ? Game1.GetSeasonForLocation(location) : Utility.getSeasonNameFromNumber(overrideSeason);
						switch (season)
						{
						case "spring":
							c = Color.Green;
							break;
						case "summer":
							c = Color.ForestGreen;
							break;
						case "fall":
							c = Color.IndianRed;
							break;
						case "winter":
							c = Color.Cyan;
							break;
						}
						if ((bool)greenhouseBush)
						{
							c = Color.Green;
							if (location != null && location.Name.Equals("Sunroom"))
							{
								foreach (NPC character in location.characters)
								{
									character.jump();
									character.doEmote(12);
								}
							}
						}
						for (int j = 0; j <= getEffectiveSize(); j++)
						{
							for (int i = 0; i < 12; i++)
							{
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1200 + (season.Equals("fall") ? 16 : (season.Equals("winter") ? (-16) : 0)), 16, 16), Utility.getRandomPositionInThisRectangle(getBoundingBox(), Game1.random) - new Vector2(0f, Game1.random.Next(64)), flipped: false, 0.01f, c)
								{
									motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -Game1.random.Next(5, 7)),
									acceleration = new Vector2(0f, (float)Game1.random.Next(13, 17) / 100f),
									accelerationChange = new Vector2(0f, -0.001f),
									scale = 4f,
									layerDepth = (tileLocation.Y + 1f) * 64f / 10000f,
									animationLength = 11,
									totalNumberOfLoops = 99,
									interval = Game1.random.Next(20, 90),
									delayBeforeAnimationStart = (j + 1) * i * 20
								});
								if (i % 6 == 0)
								{
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(50, Utility.getRandomPositionInThisRectangle(getBoundingBox(), Game1.random) - new Vector2(32f, Game1.random.Next(32, 64)), c));
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, Utility.getRandomPositionInThisRectangle(getBoundingBox(), Game1.random) - new Vector2(32f, Game1.random.Next(32, 64)), Color.White));
								}
							}
						}
						return true;
					}
					location.playSound("axchop");
				}
			}
			return false;
		}

		public bool isDestroyable(GameLocation location, Vector2 tile)
		{
			if ((int)size == 3)
			{
				return true;
			}
			if (location != null && location is Farm)
			{
				switch (Game1.whichFarm)
				{
				case 2:
					if (tile.X == 13f && tile.Y == 35f)
					{
						return true;
					}
					if (tile.X == 37f && tile.Y == 9f)
					{
						return true;
					}
					return new Rectangle(43, 11, 34, 50).Contains((int)tile.X, (int)tile.Y);
				case 1:
					return new Rectangle(32, 11, 11, 25).Contains((int)tile.X, (int)tile.Y);
				case 3:
					return new Rectangle(24, 56, 10, 8).Contains((int)tile.X, (int)tile.Y);
				case 6:
					return new Rectangle(20, 44, 36, 44).Contains((int)tile.X, (int)tile.Y);
				}
			}
			return false;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			layerDepth += positionOnScreen.X / 100000f;
			spriteBatch.Draw(texture.Value, positionOnScreen + new Vector2(0f, -64f * scale), new Rectangle(32, 96, 16, 32), Color.White, 0f, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
		}

		public override void performPlayerEntryAction(Vector2 tileLocation)
		{
			base.performPlayerEntryAction(tileLocation);
			string season = ((int)overrideSeason == -1) ? Game1.GetSeasonForLocation(currentLocation) : Utility.getSeasonNameFromNumber(overrideSeason);
			if (!season.Equals("winter") && !Game1.IsRainingHere(currentLocation) && Game1.isDarkOut() && Game1.random.NextDouble() < (season.Equals("summer") ? 0.08 : 0.04))
			{
				AmbientLocationSounds.addSound(tileLocation, 3);
				Game1.debugOutput = Game1.debugOutput + "  added cricket at " + tileLocation.ToString();
			}
		}

		private int getEffectiveSize()
		{
			if ((int)size == 3)
			{
				return 0;
			}
			if ((int)size == 4)
			{
				return 1;
			}
			return size;
		}

		public void draw(SpriteBatch spriteBatch, Vector2 tileLocation, float yDrawOffset)
		{
			this.yDrawOffset = yDrawOffset;
			draw(spriteBatch, tileLocation);
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			if ((bool)drawShadow)
			{
				if (getEffectiveSize() > 0)
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((tileLocation.X + ((getEffectiveSize() == 1) ? 0.5f : 1f)) * 64f - 51f, tileLocation.Y * 64f - 16f + yDrawOffset)), shadowSourceRect, Color.White, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
				}
				else
				{
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f - 4f + yDrawOffset)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 1E-06f);
				}
			}
			spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + (float)((getEffectiveSize() + 1) * 64 / 2), (tileLocation.Y + 1f) * 64f - (float)((getEffectiveSize() > 0 && (!townBush || getEffectiveSize() != 1) && (int)size != 4) ? 64 : 0) + yDrawOffset)), sourceRect, Color.White * alpha, shakeRotation, new Vector2((getEffectiveSize() + 1) * 16 / 2, 32f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(getBoundingBox(tileLocation).Center.Y + 48) / 10000f - tileLocation.X / 1000000f);
		}
	}
}
