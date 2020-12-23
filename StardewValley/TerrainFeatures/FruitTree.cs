using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	public class FruitTree : TerrainFeature
	{
		public const float shakeRate = (float)Math.PI / 200f;

		public const float shakeDecayRate = 0.00306796166f;

		public const int minWoodDebrisForFallenTree = 12;

		public const int minWoodDebrisForStump = 5;

		public const int startingHealth = 10;

		public const int leafFallRate = 3;

		public const int DaysUntilMaturity = 28;

		public const int maxFruitsOnTrees = 3;

		public const int seedStage = 0;

		public const int sproutStage = 1;

		public const int saplingStage = 2;

		public const int bushStage = 3;

		public const int treeStage = 4;

		public static Texture2D texture;

		[XmlElement("growthStage")]
		public readonly NetInt growthStage = new NetInt();

		[XmlElement("treeType")]
		public readonly NetInt treeType = new NetInt(-1);

		[XmlElement("indexOfFruit")]
		public readonly NetInt indexOfFruit = new NetInt();

		[XmlElement("daysUntilMature")]
		public readonly NetInt daysUntilMature = new NetInt();

		[XmlElement("fruitsOnTree")]
		public readonly NetInt fruitsOnTree = new NetInt();

		[XmlElement("struckByLightningCountdown")]
		public readonly NetInt struckByLightningCountdown = new NetInt();

		[XmlElement("health")]
		public readonly NetFloat health = new NetFloat();

		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		[XmlElement("stump")]
		public readonly NetBool stump = new NetBool();

		[XmlElement("greenHouseTree")]
		public readonly NetBool greenHouseTree = new NetBool();

		[XmlElement("greenHouseTileTree")]
		public readonly NetBool greenHouseTileTree = new NetBool();

		[XmlElement("shakeLeft")]
		public readonly NetBool shakeLeft = new NetBool();

		[XmlElement("falling")]
		private readonly NetBool falling = new NetBool();

		private bool destroy;

		private float shakeRotation;

		private float maxShake;

		private float alpha = 1f;

		private List<Leaf> leaves = new List<Leaf>();

		[XmlElement("lastPlayerToHit")]
		private readonly NetLong lastPlayerToHit = new NetLong();

		[XmlElement("fruitSeason")]
		public readonly NetString fruitSeason = new NetString();

		private float shakeTimer;

		[XmlIgnore]
		public bool GreenHouseTree
		{
			get
			{
				return greenHouseTree;
			}
			set
			{
				greenHouseTree.Value = value;
			}
		}

		[XmlIgnore]
		public bool GreenHouseTileTree
		{
			get
			{
				return greenHouseTileTree;
			}
			set
			{
				greenHouseTileTree.Value = value;
			}
		}

		public FruitTree()
			: base(needsTick: true)
		{
			loadSprite();
			base.NetFields.AddFields(growthStage, treeType, indexOfFruit, daysUntilMature, fruitsOnTree, struckByLightningCountdown, health, flipped, stump, greenHouseTree, greenHouseTileTree, shakeLeft, falling, lastPlayerToHit, fruitSeason);
		}

		public FruitTree(int saplingIndex, int growthStage)
			: this()
		{
			this.growthStage.Value = growthStage;
			flipped.Value = (Game1.random.NextDouble() < 0.5);
			health.Value = 10f;
			loadData(saplingIndex);
			daysUntilMature.Value = 28;
		}

		public FruitTree(int saplingIndex)
			: this()
		{
			flipped.Value = (Game1.random.NextDouble() < 0.5);
			health.Value = 10f;
			loadData(saplingIndex);
			daysUntilMature.Value = 28;
		}

		private void loadData(int saplingIndex)
		{
			Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\fruitTrees");
			if (data.ContainsKey(saplingIndex))
			{
				string[] rawData = data[saplingIndex].Split('/');
				treeType.Value = Convert.ToInt32(rawData[0]);
				fruitSeason.Value = rawData[1];
				indexOfFruit.Value = Convert.ToInt32(rawData[2]);
			}
		}

		public override void loadSprite()
		{
			try
			{
				if (texture == null)
				{
					texture = Game1.content.Load<Texture2D>("TileSheets\\fruitTrees");
				}
			}
			catch (Exception)
			{
			}
		}

		public override bool isActionable()
		{
			return true;
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public override Rectangle getRenderBounds(Vector2 tileLocation)
		{
			if ((bool)stump || (int)growthStage < 4)
			{
				return new Rectangle((int)(tileLocation.X - 0f) * 64, (int)(tileLocation.Y - 1f) * 64, 64, 128);
			}
			return new Rectangle((int)(tileLocation.X - 1f) * 64, (int)(tileLocation.Y - 5f) * 64, 192, 448);
		}

		public override bool performUseAction(Vector2 tileLocation, GameLocation location)
		{
			if (maxShake == 0f && !stump && (int)growthStage >= 3 && (!Game1.GetSeasonForLocation(location).Equals("winter") || location.SeedsIgnoreSeasonsHere()))
			{
				location.playSound("leafrustle");
			}
			shake(tileLocation, doEvenIfStillShaking: false, location);
			return true;
		}

		public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
		{
			if (destroy)
			{
				return true;
			}
			alpha = Math.Min(1f, alpha + 0.05f);
			if (shakeTimer > 0f)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if ((int)growthStage >= 4 && !falling && !stump && Game1.player.GetBoundingBox().Intersects(new Rectangle(64 * ((int)tileLocation.X - 1), 64 * ((int)tileLocation.Y - 4), 192, 224)))
			{
				alpha = Math.Max(0.4f, alpha - 0.09f);
			}
			if (!falling)
			{
				if ((double)Math.Abs(shakeRotation) > Math.PI / 2.0 && leaves.Count <= 0 && (float)health <= 0f)
				{
					return true;
				}
				if (maxShake > 0f)
				{
					if ((bool)shakeLeft)
					{
						shakeRotation -= (((int)growthStage >= 4) ? 0.005235988f : ((float)Math.PI / 200f));
						if (shakeRotation <= 0f - maxShake)
						{
							shakeLeft.Value = false;
						}
					}
					else
					{
						shakeRotation += (((int)growthStage >= 4) ? 0.005235988f : ((float)Math.PI / 200f));
						if (shakeRotation >= maxShake)
						{
							shakeLeft.Value = true;
						}
					}
				}
				if (maxShake > 0f)
				{
					maxShake = Math.Max(0f, maxShake - (((int)growthStage >= 4) ? 0.00102265389f : 0.00306796166f));
				}
				if ((int)struckByLightningCountdown > 0 && Game1.random.NextDouble() < 0.01)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(tileLocation.X * 64f + (float)Game1.random.Next(-64, 96), tileLocation.Y * 64f - 192f + (float)Game1.random.Next(-64, 128)), flipped: false, 0.002f, Color.Gray)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -0.5f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 2f,
						scaleChange = 0.01f
					});
				}
			}
			else
			{
				shakeRotation += (shakeLeft ? (0f - maxShake * maxShake) : (maxShake * maxShake));
				maxShake += 0.00153398083f;
				if (Game1.random.NextDouble() < 0.01 && !Game1.GetSeasonForLocation(location).Equals("winter"))
				{
					location.localSound("leafrustle");
				}
				if ((double)Math.Abs(shakeRotation) > Math.PI / 2.0)
				{
					falling.Value = false;
					maxShake = 0f;
					location.localSound("treethud");
					int leavesToAdd = Game1.random.Next(90, 120);
					for (int j = 0; j < leavesToAdd; j++)
					{
						leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 192f)) + (shakeLeft ? (-320) : 256), tileLocation.Y * 64f - 64f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(10, 40) / 10f));
					}
					Game1.createRadialDebris(location, 12, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit).professions.Contains(12) ? 1.25 : 1.0) * 12.0), resource: true);
					Game1.createRadialDebris(location, 12, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit).professions.Contains(12) ? 1.25 : 1.0) * 12.0), resource: false);
					if (Game1.IsMultiplayer)
					{
						Game1.recentMultiplayerRandom = new Random((int)tileLocation.X * 1000 + (int)tileLocation.Y);
						_ = Game1.recentMultiplayerRandom;
					}
					else
					{
						new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
					}
					if (Game1.IsMultiplayer)
					{
						Game1.createMultipleObjectDebris(92, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, 10, lastPlayerToHit, location);
					}
					else
					{
						Game1.createMultipleObjectDebris(92, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, 10, location);
					}
					if ((float)health <= 0f)
					{
						health.Value = -100f;
					}
				}
			}
			for (int i = leaves.Count - 1; i >= 0; i--)
			{
				leaves.ElementAt(i).position.Y -= leaves.ElementAt(i).yVelocity - 3f;
				leaves.ElementAt(i).yVelocity = Math.Max(0f, leaves.ElementAt(i).yVelocity - 0.01f);
				leaves.ElementAt(i).rotation += leaves.ElementAt(i).rotationRate;
				if (leaves.ElementAt(i).position.Y >= tileLocation.Y * 64f + 64f)
				{
					leaves.RemoveAt(i);
				}
			}
			return false;
		}

		public void shake(Vector2 tileLocation, bool doEvenIfStillShaking, GameLocation location)
		{
			if (((maxShake == 0f) | doEvenIfStillShaking) && (int)growthStage >= 3 && !stump)
			{
				shakeLeft.Value = ((float)Game1.player.getStandingX() > (tileLocation.X + 0.5f) * 64f || ((Game1.player.getTileLocation().X == tileLocation.X && Game1.random.NextDouble() < 0.5) ? true : false));
				maxShake = (float)(((int)growthStage >= 4) ? (Math.PI / 128.0) : (Math.PI / 64.0));
				if ((int)growthStage >= 4)
				{
					if (Game1.random.NextDouble() < 0.66 && Game1.GetSeasonForLocation(location) != "winter")
					{
						int numberOfLeaves2 = Game1.random.Next(1, 6);
						for (int k = 0; k < numberOfLeaves2; k++)
						{
							leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f - 64f), (int)(tileLocation.X * 64f + 128f)), Game1.random.Next((int)(tileLocation.Y * 64f - 256f), (int)(tileLocation.Y * 64f - 192f))), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(5) / 10f));
						}
					}
					int fruitquality = 0;
					if ((int)daysUntilMature <= -112)
					{
						fruitquality = 1;
					}
					if ((int)daysUntilMature <= -224)
					{
						fruitquality = 2;
					}
					if ((int)daysUntilMature <= -336)
					{
						fruitquality = 4;
					}
					if ((int)struckByLightningCountdown > 0)
					{
						fruitquality = 0;
					}
					if (!location.terrainFeatures.ContainsKey(tileLocation) || !location.terrainFeatures[tileLocation].Equals(this))
					{
						return;
					}
					for (int j = 0; j < (int)fruitsOnTree; j++)
					{
						Vector2 offset = new Vector2(0f, 0f);
						switch (j)
						{
						case 0:
							offset.X = -64f;
							break;
						case 1:
							offset.X = 64f;
							offset.Y = -32f;
							break;
						case 2:
							offset.Y = 32f;
							break;
						}
						Debris d = new Debris(((int)struckByLightningCountdown > 0) ? 382 : ((int)indexOfFruit), new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 3f) * 64f + 32f) + offset, new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()))
						{
							itemQuality = fruitquality
						};
						d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
						d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
						location.debris.Add(d);
					}
					fruitsOnTree.Value = 0;
				}
				else if (Game1.random.NextDouble() < 0.66 && Game1.GetSeasonForLocation(location) != "winter")
				{
					int numberOfLeaves = Game1.random.Next(1, 3);
					for (int i = 0; i < numberOfLeaves; i++)
					{
						leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 48f)), tileLocation.Y * 64f - 96f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(30) / 10f));
					}
				}
			}
			else if ((bool)stump)
			{
				shakeTimer = 100f;
			}
		}

		public override bool isPassable(Character c = null)
		{
			if ((float)health <= -99f)
			{
				return true;
			}
			return false;
		}

		public static bool IsGrowthBlocked(Vector2 tileLocation, GameLocation environment)
		{
			Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(tileLocation);
			for (int i = 0; i < surroundingTileLocationsArray.Length; i++)
			{
				Vector2 v = surroundingTileLocationsArray[i];
				bool isClearHoeDirt = environment.terrainFeatures.ContainsKey(v) && environment.terrainFeatures[v] is HoeDirt && (environment.terrainFeatures[v] as HoeDirt).crop == null;
				if (environment.isTileOccupied(v, "", ignoreAllCharacters: true) && !isClearHoeDirt)
				{
					Object o = environment.getObjectAtTile((int)v.X, (int)v.Y);
					if (o == null)
					{
						return true;
					}
					if (!Utility.IsNormalObjectAtParentSheetIndex(o, 590))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
		{
			if ((float)health <= -99f)
			{
				destroy = true;
			}
			if ((int)struckByLightningCountdown > 0)
			{
				struckByLightningCountdown.Value--;
				if ((int)struckByLightningCountdown <= 0)
				{
					fruitsOnTree.Value = 0;
				}
			}
			bool foundSomething = IsGrowthBlocked(tileLocation, environment);
			if (!foundSomething || (int)daysUntilMature <= 0)
			{
				if ((int)daysUntilMature > 28)
				{
					daysUntilMature.Value = 28;
				}
				daysUntilMature.Value--;
				if ((int)daysUntilMature <= 0)
				{
					growthStage.Value = 4;
				}
				else if ((int)daysUntilMature <= 7)
				{
					growthStage.Value = 3;
				}
				else if ((int)daysUntilMature <= 14)
				{
					growthStage.Value = 2;
				}
				else if ((int)daysUntilMature <= 21)
				{
					growthStage.Value = 1;
				}
				else
				{
					growthStage.Value = 0;
				}
			}
			else if (foundSomething && growthStage.Value != 4)
			{
				Game1.multiplayer.broadcastGlobalMessage("Strings\\UI:FruitTree_Warning", true, Game1.objectInformation[indexOfFruit].Split('/')[4]);
			}
			if (!stump && (int)growthStage == 4 && (((int)struckByLightningCountdown > 0 && !Game1.IsWinter) || IsInSeasonHere(environment) || environment.SeedsIgnoreSeasonsHere()))
			{
				fruitsOnTree.Value = Math.Min(3, (int)fruitsOnTree + 1);
				if (environment.IsGreenhouse)
				{
					greenHouseTree.Value = true;
				}
			}
			if ((bool)stump)
			{
				fruitsOnTree.Value = 0;
			}
		}

		public virtual bool IsInSeasonHere(GameLocation location)
		{
			if (fruitSeason.Value == "island")
			{
				if (location.GetLocationContext() == GameLocation.LocationContext.Island)
				{
					return true;
				}
				return Game1.GetSeasonForLocation(location).Equals("summer");
			}
			return Game1.GetSeasonForLocation(location).Equals(fruitSeason);
		}

		public override bool seasonUpdate(bool onLoad)
		{
			if (!IsInSeasonHere(currentLocation) && !onLoad && !greenHouseTree)
			{
				fruitsOnTree.Value = 0;
			}
			return false;
		}

		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location)
		{
			if ((float)health <= -99f)
			{
				return false;
			}
			if (t != null && t is MeleeWeapon)
			{
				return false;
			}
			if ((int)growthStage >= 4)
			{
				if (t != null && t is Axe)
				{
					location.playSound("axchop");
					location.debris.Add(new Debris(12, Game1.random.Next((int)t.upgradeLevel * 2, (int)t.upgradeLevel * 4), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), t.getLastFarmerToUse().Position, 0));
					lastPlayerToHit.Value = t.getLastFarmerToUse().UniqueMultiplayerID;
					int fruitquality = 0;
					if ((int)daysUntilMature <= -112)
					{
						fruitquality = 1;
					}
					if ((int)daysUntilMature <= -224)
					{
						fruitquality = 2;
					}
					if ((int)daysUntilMature <= -336)
					{
						fruitquality = 4;
					}
					if ((int)struckByLightningCountdown > 0)
					{
						fruitquality = 0;
					}
					if (location.terrainFeatures.ContainsKey(tileLocation) && location.terrainFeatures[tileLocation].Equals(this))
					{
						for (int i = 0; i < (int)fruitsOnTree; i++)
						{
							Vector2 offset = new Vector2(0f, 0f);
							switch (i)
							{
							case 0:
								offset.X = -64f;
								break;
							case 1:
								offset.X = 64f;
								offset.Y = -32f;
								break;
							case 2:
								offset.Y = 32f;
								break;
							}
							Debris d2 = new Debris(((int)struckByLightningCountdown > 0) ? 382 : ((int)indexOfFruit), new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 3f) * 64f + 32f) + offset, new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()))
							{
								itemQuality = fruitquality
							};
							d2.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
							d2.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
							location.debris.Add(d2);
						}
						fruitsOnTree.Value = 0;
					}
				}
				else if (explosion <= 0)
				{
					return false;
				}
				shake(tileLocation, doEvenIfStillShaking: true, location);
				float damage3 = 1f;
				if (explosion > 0)
				{
					damage3 = explosion;
				}
				else
				{
					if (t == null)
					{
						return false;
					}
					switch ((int)t.upgradeLevel)
					{
					case 0:
						damage3 = 1f;
						break;
					case 1:
						damage3 = 1.25f;
						break;
					case 2:
						damage3 = 1.67f;
						break;
					case 3:
						damage3 = 2.5f;
						break;
					case 4:
						damage3 = 5f;
						break;
					default:
						damage3 = (int)t.upgradeLevel + 1;
						break;
					}
				}
				health.Value -= damage3;
				if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(damage3 / 5f))
				{
					Debris d = new Debris(388, new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()));
					d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
					d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
					location.debris.Add(d);
				}
				if ((float)health <= 0f)
				{
					if (!stump)
					{
						location.playSound("treecrack");
						stump.Value = true;
						health.Value = 5f;
						falling.Value = true;
						if (t == null || t.getLastFarmerToUse() == null)
						{
							shakeLeft.Value = true;
						}
						else
						{
							shakeLeft.Value = ((float)t.getLastFarmerToUse().getStandingX() > (tileLocation.X + 0.5f) * 64f);
						}
					}
					else
					{
						health.Value = -100f;
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(30, 40), resource: false);
						int whatToDrop = 92;
						if (Game1.IsMultiplayer)
						{
							Game1.recentMultiplayerRandom = new Random((int)tileLocation.X * 2000 + (int)tileLocation.Y);
							_ = Game1.recentMultiplayerRandom;
						}
						else
						{
							new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
						}
						if (t == null || t.getLastFarmerToUse() == null)
						{
							Game1.createMultipleObjectDebris(92, (int)tileLocation.X, (int)tileLocation.Y, 2, location);
						}
						else if (Game1.IsMultiplayer)
						{
							Game1.createMultipleObjectDebris(whatToDrop, (int)tileLocation.X, (int)tileLocation.Y, 1, lastPlayerToHit, location);
							Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.getFarmer(lastPlayerToHit).professions.Contains(12) ? 5 : 4, resource: true);
						}
						else
						{
							Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit).professions.Contains(12) ? 1.25 : 1.0) * 5.0), resource: true);
							Game1.createMultipleObjectDebris(whatToDrop, (int)tileLocation.X, (int)tileLocation.Y, 1, location);
						}
					}
				}
			}
			else if ((int)growthStage >= 3)
			{
				if (t != null && t.BaseName.Contains("Ax"))
				{
					location.playSound("axchop");
					location.playSound("leafrustle");
					location.debris.Add(new Debris(12, Game1.random.Next((int)t.upgradeLevel * 2, (int)t.upgradeLevel * 4), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), new Vector2(t.getLastFarmerToUse().GetBoundingBox().Center.X, t.getLastFarmerToUse().GetBoundingBox().Center.Y), 0));
				}
				else if (explosion <= 0)
				{
					return false;
				}
				shake(tileLocation, doEvenIfStillShaking: true, location);
				float damage = 1f;
				Random debrisRandom = (!Game1.IsMultiplayer) ? new Random((int)((float)(double)Game1.uniqueIDForThisGame + tileLocation.X * 7f + tileLocation.Y * 11f + (float)(double)Game1.stats.DaysPlayed + (float)health)) : Game1.recentMultiplayerRandom;
				if (explosion > 0)
				{
					damage = explosion;
				}
				else
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
				}
				int debris = 0;
				while (t != null && debrisRandom.NextDouble() < (double)damage * 0.08 + (double)((float)t.getLastFarmerToUse().ForagingLevel / 200f))
				{
					debris++;
				}
				health.Value -= damage;
				if (debris > 0)
				{
					Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, debris, location);
				}
				if ((float)health <= 0f)
				{
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(20, 30), resource: false);
					return true;
				}
			}
			else if ((int)growthStage >= 1)
			{
				if (explosion > 0)
				{
					return true;
				}
				if (t != null && t.BaseName.Contains("Axe"))
				{
					location.playSound("axchop");
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
				}
				if (t is Axe || t is Pickaxe || t is Hoe || t is MeleeWeapon)
				{
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
					if (t.BaseName.Contains("Axe") && Game1.recentMultiplayerRandom.NextDouble() < (double)((float)t.getLastFarmerToUse().ForagingLevel / 10f))
					{
						Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, 1, location);
					}
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
					return true;
				}
			}
			else
			{
				if (explosion > 0)
				{
					return true;
				}
				if (t.BaseName.Contains("Axe") || t.BaseName.Contains("Pick") || t.BaseName.Contains("Hoe"))
				{
					location.playSound("woodyHit");
					location.playSound("axchop");
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
					return true;
				}
			}
			return false;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			layerDepth += positionOnScreen.X / 100000f;
			if ((int)growthStage < 4)
			{
				Rectangle sourceRect = Rectangle.Empty;
				switch ((int)growthStage)
				{
				case 0:
					sourceRect = new Rectangle(128, 512, 64, 64);
					break;
				case 1:
					sourceRect = new Rectangle(0, 512, 64, 64);
					break;
				case 2:
					sourceRect = new Rectangle(64, 512, 64, 64);
					break;
				default:
					sourceRect = new Rectangle(0, 384, 64, 128);
					break;
				}
				spriteBatch.Draw(texture, positionOnScreen - new Vector2(0f, (float)sourceRect.Height * scale), sourceRect, Color.White, 0f, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + (float)sourceRect.Height * scale) / 20000f);
			}
			else
			{
				if (!falling)
				{
					spriteBatch.Draw(texture, positionOnScreen + new Vector2(0f, -64f * scale), new Rectangle(128, 384, 64, 128), Color.White, 0f, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
				}
				if (!stump || (bool)falling)
				{
					spriteBatch.Draw(texture, positionOnScreen + new Vector2(-64f * scale, -320f * scale), new Rectangle(0, 0, 192, 384), Color.White, shakeRotation, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale) / 20000f);
				}
			}
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			string season = Game1.GetSeasonForLocation(currentLocation);
			if ((bool)greenHouseTileTree)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(669, 1957, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
			}
			if ((int)growthStage < 4)
			{
				Vector2 positionOffset = new Vector2((float)Math.Max(-8.0, Math.Min(64.0, Math.Sin((double)(tileLocation.X * 200f) / (Math.PI * 2.0)) * -16.0)), (float)Math.Max(-8.0, Math.Min(64.0, Math.Sin((double)(tileLocation.X * 200f) / (Math.PI * 2.0)) * -16.0))) / 2f;
				Rectangle sourceRect = Rectangle.Empty;
				switch ((int)growthStage)
				{
				case 0:
					sourceRect = new Rectangle(0, (int)treeType * 5 * 16, 48, 80);
					break;
				case 1:
					sourceRect = new Rectangle(48, (int)treeType * 5 * 16, 48, 80);
					break;
				case 2:
					sourceRect = new Rectangle(96, (int)treeType * 5 * 16, 48, 80);
					break;
				default:
					sourceRect = new Rectangle(144, (int)treeType * 5 * 16, 48, 80);
					break;
				}
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f + positionOffset.X, tileLocation.Y * 64f - (float)sourceRect.Height + 128f + positionOffset.Y)), sourceRect, Color.White, shakeRotation, new Vector2(24f, 80f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(tileLocation).Bottom / 10000f - tileLocation.X / 1000000f);
			}
			else
			{
				if (!stump || (bool)falling)
				{
					if (!falling)
					{
						spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), new Rectangle((12 + (greenHouseTree ? 1 : Utility.getSeasonNumber(season)) * 3) * 16, (int)treeType * 5 * 16 + 64, 48, 16), ((int)struckByLightningCountdown > 0) ? (Color.Gray * alpha) : (Color.White * alpha), 0f, new Vector2(24f, 16f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-07f);
					}
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), new Rectangle((12 + (greenHouseTree ? 1 : Utility.getSeasonNumber(season)) * 3) * 16, (int)treeType * 5 * 16, 48, 64), ((int)struckByLightningCountdown > 0) ? (Color.Gray * alpha) : (Color.White * alpha), shakeRotation, new Vector2(24f, 80f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(tileLocation).Bottom / 10000f + 0.001f - tileLocation.X / 1000000f);
				}
				if ((float)health >= 1f || (!falling && (float)health > -99f))
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f + ((shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 2f) : 0f), tileLocation.Y * 64f + 64f)), new Rectangle(384, (int)treeType * 5 * 16 + 48, 48, 32), ((int)struckByLightningCountdown > 0) ? (Color.Gray * alpha) : (Color.White * alpha), 0f, new Vector2(24f, 32f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((bool)stump && !falling) ? ((float)getBoundingBox(tileLocation).Bottom / 10000f) : ((float)getBoundingBox(tileLocation).Bottom / 10000f - 0.001f - tileLocation.X / 1000000f));
				}
				for (int j = 0; j < (int)fruitsOnTree; j++)
				{
					switch (j)
					{
					case 0:
						spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 64f + tileLocation.X * 200f % 64f / 2f, tileLocation.Y * 64f - 192f - tileLocation.X % 64f / 3f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, ((int)struckByLightningCountdown > 0) ? 382 : ((int)indexOfFruit), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)getBoundingBox(tileLocation).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f);
						break;
					case 1:
						spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 256f + tileLocation.X * 232f % 64f / 3f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, ((int)struckByLightningCountdown > 0) ? 382 : ((int)indexOfFruit), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)getBoundingBox(tileLocation).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f);
						break;
					case 2:
						spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + tileLocation.X * 200f % 64f / 3f, tileLocation.Y * 64f - 160f + tileLocation.X * 200f % 64f / 3f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, ((int)struckByLightningCountdown > 0) ? 382 : ((int)indexOfFruit), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, (float)getBoundingBox(tileLocation).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f);
						break;
					}
				}
			}
			foreach (Leaf i in leaves)
			{
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, i.position), new Rectangle((24 + Utility.getSeasonNumber(season)) * 16, (int)treeType * 5 * 16, 8, 8), Color.White, i.rotation, Vector2.Zero, 4f, SpriteEffects.None, (float)getBoundingBox(tileLocation).Bottom / 10000f + 0.01f);
			}
		}
	}
}
