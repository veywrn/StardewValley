using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.TerrainFeatures
{
	public class Tree : TerrainFeature
	{
		public const float chanceForDailySeed = 0.05f;

		public const float shakeRate = (float)Math.PI / 200f;

		public const float shakeDecayRate = 0.00306796166f;

		public const int minWoodDebrisForFallenTree = 12;

		public const int minWoodDebrisForStump = 5;

		public const int startingHealth = 10;

		public const int leafFallRate = 3;

		public const int bushyTree = 1;

		public const int leafyTree = 2;

		public const int pineTree = 3;

		public const int winterTree1 = 4;

		public const int winterTree2 = 5;

		public const int palmTree = 6;

		public const int mushroomTree = 7;

		public const int mahoganyTree = 8;

		public const int palmTree2 = 9;

		public const int seedStage = 0;

		public const int sproutStage = 1;

		public const int saplingStage = 2;

		public const int bushStage = 3;

		public const int treeStage = 5;

		public Lazy<Texture2D> texture;

		private string season;

		[XmlElement("growthStage")]
		public readonly NetInt growthStage = new NetInt();

		[XmlElement("treeType")]
		public readonly NetInt treeType = new NetInt();

		[XmlElement("health")]
		public readonly NetFloat health = new NetFloat();

		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		[XmlElement("stump")]
		public readonly NetBool stump = new NetBool();

		[XmlElement("tapped")]
		public readonly NetBool tapped = new NetBool();

		[XmlElement("hasSeed")]
		public readonly NetBool hasSeed = new NetBool();

		[XmlElement("fertilized")]
		public readonly NetBool fertilized = new NetBool();

		[XmlElement("shakeLeft")]
		public readonly NetBool shakeLeft = new NetBool().Interpolated(interpolate: false, wait: false);

		[XmlElement("falling")]
		private readonly NetBool falling = new NetBool();

		[XmlElement("destroy")]
		private readonly NetBool destroy = new NetBool();

		private float shakeRotation;

		private float maxShake;

		private float alpha = 1f;

		private List<Leaf> leaves = new List<Leaf>();

		[XmlElement("lastPlayerToHit")]
		private readonly NetLong lastPlayerToHit = new NetLong();

		private float shakeTimer;

		public Microsoft.Xna.Framework.Rectangle treeTopSourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 96);

		public static Microsoft.Xna.Framework.Rectangle stumpSourceRect = new Microsoft.Xna.Framework.Rectangle(32, 96, 16, 32);

		public static Microsoft.Xna.Framework.Rectangle shadowSourceRect = new Microsoft.Xna.Framework.Rectangle(663, 1011, 41, 30);

		public Tree()
			: base(needsTick: true)
		{
			resetTexture();
			base.NetFields.AddFields(growthStage, treeType, health, flipped, stump, tapped, hasSeed, fertilized, shakeLeft, falling, destroy, lastPlayerToHit);
			treeType.fieldChangeVisibleEvent += delegate
			{
				resetTexture();
			};
		}

		public Tree(int which, int growthStage)
			: this()
		{
			this.growthStage.Value = growthStage;
			treeType.Value = which;
			if ((int)treeType == 4)
			{
				treeType.Value = 1;
			}
			if ((int)treeType == 5)
			{
				treeType.Value = 2;
			}
			flipped.Value = (Game1.random.NextDouble() < 0.5);
			health.Value = 10f;
		}

		public Tree(int which)
			: this()
		{
			treeType.Value = which;
			if ((int)treeType == 4)
			{
				treeType.Value = 1;
			}
			if ((int)treeType == 5)
			{
				treeType.Value = 2;
			}
			flipped.Value = (Game1.random.NextDouble() < 0.5);
			health.Value = 10f;
		}

		protected void resetTexture()
		{
			texture = new Lazy<Texture2D>(loadTexture);
		}

		protected Texture2D loadTexture()
		{
			if ((int)treeType == 7)
			{
				return Game1.content.Load<Texture2D>("TerrainFeatures\\mushroom_tree");
			}
			if ((int)treeType == 9)
			{
				return Game1.content.Load<Texture2D>("TerrainFeatures\\tree_palm2");
			}
			if ((int)treeType == 6)
			{
				return Game1.content.Load<Texture2D>("TerrainFeatures\\tree_palm");
			}
			string season = Game1.GetSeasonForLocation(currentLocation);
			if ((int)treeType == 3 && season.Equals("summer"))
			{
				season = "spring";
			}
			if (Game1.currentLocation != null && (Game1.currentLocation.Name.Equals("Desert") || Game1.currentLocation is MineShaft))
			{
				season = "spring";
			}
			return Game1.content.Load<Texture2D>("TerrainFeatures\\tree" + Math.Max(1, treeType) + "_" + season);
		}

		public override Microsoft.Xna.Framework.Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public override Microsoft.Xna.Framework.Rectangle getRenderBounds(Vector2 tileLocation)
		{
			if ((bool)stump || (int)growthStage < 5)
			{
				return new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - 0f) * 64, (int)(tileLocation.Y - 1f) * 64, 64, 128);
			}
			return new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - 1f) * 64, (int)(tileLocation.Y - 5f) * 64, 192, 448);
		}

		public override bool performUseAction(Vector2 tileLocation, GameLocation location)
		{
			if (!tapped)
			{
				if (maxShake == 0f && !stump && (int)growthStage >= 3 && (!Game1.GetSeasonForLocation(currentLocation).Equals("winter") || location.Name.Equals("Desert") || (int)treeType == 3))
				{
					location.localSound("leafrustle");
				}
				shake(tileLocation, doEvenIfStillShaking: false, location);
			}
			if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.canBePlacedHere(location, tileLocation))
			{
				return false;
			}
			return true;
		}

		private int extraWoodCalculator(Vector2 tileLocation)
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
			int extraWood = 0;
			if (random.NextDouble() < Game1.player.DailyLuck)
			{
				extraWood++;
			}
			if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
			{
				extraWood++;
			}
			if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
			{
				extraWood++;
			}
			if (random.NextDouble() < (double)Game1.player.LuckLevel / 25.0)
			{
				extraWood++;
			}
			return extraWood;
		}

		public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
		{
			if (season != Game1.GetSeasonForLocation(currentLocation))
			{
				resetTexture();
				season = Game1.GetSeasonForLocation(currentLocation);
			}
			if (shakeTimer > 0f)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if ((bool)destroy)
			{
				return true;
			}
			alpha = Math.Min(1f, alpha + 0.05f);
			if ((int)growthStage >= 5 && !falling && !stump && Game1.player.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(64 * ((int)tileLocation.X - 1), 64 * ((int)tileLocation.Y - 5), 192, 288)))
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
						shakeRotation -= (((int)growthStage >= 5) ? 0.005235988f : ((float)Math.PI / 200f));
						if (shakeRotation <= 0f - maxShake)
						{
							shakeLeft.Value = false;
						}
					}
					else
					{
						shakeRotation += (((int)growthStage >= 5) ? 0.005235988f : ((float)Math.PI / 200f));
						if (shakeRotation >= maxShake)
						{
							shakeLeft.Value = true;
						}
					}
				}
				if (maxShake > 0f)
				{
					maxShake = Math.Max(0f, maxShake - (((int)growthStage >= 5) ? 0.00102265389f : 0.00306796166f));
				}
			}
			else
			{
				shakeRotation += (shakeLeft ? (0f - maxShake * maxShake) : (maxShake * maxShake));
				maxShake += 0.00153398083f;
				if (Game1.random.NextDouble() < 0.01 && (int)treeType != 7)
				{
					location.localSound("leafrustle");
				}
				if ((double)Math.Abs(shakeRotation) > Math.PI / 2.0)
				{
					falling.Value = false;
					maxShake = 0f;
					location.localSound("treethud");
					int leavesToAdd = Game1.random.Next(90, 120);
					if (location.Objects.ContainsKey(tileLocation))
					{
						location.Objects.Remove(tileLocation);
					}
					for (int j = 0; j < leavesToAdd; j++)
					{
						leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 192f)) + (shakeLeft ? (-320) : 256), tileLocation.Y * 64f - 64f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(10, 40) / 10f));
					}
					if ((int)treeType != 7)
					{
						if ((int)treeType != 8)
						{
							Game1.createRadialDebris(location, 12, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit).professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + extraWoodCalculator(tileLocation))), resource: true);
							Game1.createRadialDebris(location, 12, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit).professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + extraWoodCalculator(tileLocation))), resource: false);
						}
						Random r;
						if (Game1.IsMultiplayer)
						{
							Game1.recentMultiplayerRandom = new Random((int)tileLocation.X * 1000 + (int)tileLocation.Y);
							r = Game1.recentMultiplayerRandom;
						}
						else
						{
							r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
						}
						if (Game1.IsMultiplayer)
						{
							if ((int)treeType != 8)
							{
								Game1.createMultipleObjectDebris(92, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, 5, lastPlayerToHit, location);
							}
							int numHardwood2 = 0;
							if (Game1.getFarmer(lastPlayerToHit) != null)
							{
								while (Game1.getFarmer(lastPlayerToHit).professions.Contains(14) && r.NextDouble() < 0.5)
								{
									numHardwood2++;
								}
							}
							if ((int)treeType == 8)
							{
								numHardwood2 += r.Next(7, 12);
								if (Game1.getFarmer(lastPlayerToHit).professions.Contains(14))
								{
									numHardwood2 += (int)((float)numHardwood2 * 0.25f + 0.9f);
								}
							}
							if (numHardwood2 > 0)
							{
								Game1.createMultipleObjectDebris(709, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, numHardwood2, lastPlayerToHit, location);
							}
							if (Game1.getFarmer(lastPlayerToHit).getEffectiveSkillLevel(2) >= 1 && r.NextDouble() < 0.75)
							{
								if ((int)treeType < 4)
								{
									Game1.createMultipleObjectDebris(308 + (int)treeType, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, r.Next(1, 3), lastPlayerToHit, location);
								}
								else if ((int)treeType == 8 && r.NextDouble() < 0.75)
								{
									Game1.createMultipleObjectDebris(292, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, r.Next(1, 3), lastPlayerToHit, location);
								}
							}
						}
						else
						{
							if ((int)treeType != 8)
							{
								Game1.createMultipleObjectDebris(92, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, 5, location);
							}
							int numHardwood = 0;
							if (Game1.getFarmer(lastPlayerToHit) != null)
							{
								while (Game1.getFarmer(lastPlayerToHit).professions.Contains(14) && r.NextDouble() < 0.5)
								{
									numHardwood++;
								}
							}
							if ((int)treeType == 8)
							{
								numHardwood += r.Next(7, 12);
								if (Game1.getFarmer(lastPlayerToHit).professions.Contains(14))
								{
									numHardwood += (int)((float)numHardwood * 0.25f + 0.9f);
								}
							}
							if (numHardwood > 0)
							{
								Game1.createMultipleObjectDebris(709, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, numHardwood, location);
							}
							if ((long)lastPlayerToHit != 0L && Game1.getFarmer(lastPlayerToHit).getEffectiveSkillLevel(2) >= 1 && r.NextDouble() < 0.75)
							{
								if ((int)treeType < 4)
								{
									Game1.createMultipleObjectDebris(308 + (int)treeType, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, r.Next(1, 3), location);
								}
								else if ((int)treeType == 8 && r.NextDouble() < 0.75)
								{
									Game1.createMultipleObjectDebris(292, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, r.Next(1, 3), location);
								}
							}
						}
					}
					else
					{
						Game1.createMultipleObjectDebris(420, (int)tileLocation.X + (shakeLeft ? (-4) : 4), (int)tileLocation.Y, 5, location);
						if (Game1.random.NextDouble() < 0.01)
						{
							location.debris.Add(new Debris(new Hat(42), tileLocation * 64f + new Vector2(32f, 32f)));
						}
					}
					if ((float)health == -100f)
					{
						return true;
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

		private void shake(Vector2 tileLocation, bool doEvenIfStillShaking, GameLocation location)
		{
			if (((maxShake == 0f) | doEvenIfStillShaking) && (int)growthStage >= 3 && !stump)
			{
				shakeLeft.Value = ((float)Game1.player.getStandingX() > (tileLocation.X + 0.5f) * 64f || ((Game1.player.getTileLocation().X == tileLocation.X && Game1.random.NextDouble() < 0.5) ? true : false));
				maxShake = (float)(((int)growthStage >= 5) ? (Math.PI / 128.0) : (Math.PI / 64.0));
				if ((int)growthStage >= 5)
				{
					if (Game1.random.NextDouble() < 0.66)
					{
						int numberOfLeaves2 = Game1.random.Next(1, 6);
						for (int j = 0; j < numberOfLeaves2; j++)
						{
							leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f - 64f), (int)(tileLocation.X * 64f + 128f)), Game1.random.Next((int)(tileLocation.Y * 64f - 256f), (int)(tileLocation.Y * 64f - 192f))), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(5) / 10f));
						}
					}
					if (Game1.random.NextDouble() < 0.01 && (Game1.GetSeasonForLocation(currentLocation).Equals("spring") || Game1.GetSeasonForLocation(currentLocation).Equals("summer") || currentLocation.GetLocationContext() == GameLocation.LocationContext.Island))
					{
						while (Game1.random.NextDouble() < 0.8)
						{
							location.addCritter(new Butterfly(new Vector2(tileLocation.X + (float)Game1.random.Next(1, 3), tileLocation.Y - 2f + (float)Game1.random.Next(-1, 2)), currentLocation.GetLocationContext() == GameLocation.LocationContext.Island));
						}
					}
					if ((bool)hasSeed && (Game1.IsMultiplayer || Game1.player.ForagingLevel >= 1))
					{
						int seedIndex = -1;
						switch ((int)treeType)
						{
						case 3:
							seedIndex = 311;
							break;
						case 1:
							seedIndex = 309;
							break;
						case 8:
							seedIndex = 292;
							break;
						case 2:
							seedIndex = 310;
							break;
						case 6:
						case 9:
							seedIndex = 88;
							break;
						}
						if (Game1.GetSeasonForLocation(currentLocation).Equals("fall") && (int)treeType == 2 && Game1.dayOfMonth >= 14)
						{
							seedIndex = 408;
						}
						if (seedIndex != -1)
						{
							Game1.createObjectDebris(seedIndex, (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
						}
						if (seedIndex == 88 && new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 13 + (int)tileLocation.Y * 54).NextDouble() < 0.1)
						{
							Game1.createObjectDebris(791, (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
						}
						if (Game1.random.NextDouble() <= 0.5 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
						{
							Game1.createObjectDebris(890, (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
						}
						hasSeed.Value = false;
					}
				}
				else if (Game1.random.NextDouble() < 0.66)
				{
					int numberOfLeaves = Game1.random.Next(1, 3);
					for (int i = 0; i < numberOfLeaves; i++)
					{
						leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 48f)), tileLocation.Y * 64f - 32f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(30) / 10f));
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
			if ((float)health <= -99f || (int)growthStage == 0)
			{
				return true;
			}
			return false;
		}

		public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
		{
			Microsoft.Xna.Framework.Rectangle growthRect = new Microsoft.Xna.Framework.Rectangle((int)((tileLocation.X - 1f) * 64f), (int)((tileLocation.Y - 1f) * 64f), 192, 192);
			if ((float)health <= -100f)
			{
				destroy.Value = true;
			}
			if (tapped.Value)
			{
				Object tile_object = environment.getObjectAtTile((int)tileLocation.X, (int)tileLocation.Y);
				if (tile_object == null || !tile_object.bigCraftable || (int)tile_object.parentSheetIndex != 105)
				{
					tapped.Value = false;
				}
			}
			if (!Game1.GetSeasonForLocation(currentLocation).Equals("winter") || (int)treeType == 6 || (int)treeType == 9 || environment.CanPlantTreesHere(-1, (int)tileLocation.X, (int)tileLocation.Y) || fertilized.Value)
			{
				string s = environment.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "NoSpawn", "Back");
				if (s != null && (s.Equals("All") || s.Equals("Tree") || s.Equals("True")))
				{
					return;
				}
				if ((int)growthStage == 4)
				{
					foreach (KeyValuePair<Vector2, TerrainFeature> t in environment.terrainFeatures.Pairs)
					{
						if (t.Value is Tree && !t.Value.Equals(this) && (int)((Tree)t.Value).growthStage >= 5 && t.Value.getBoundingBox(t.Key).Intersects(growthRect))
						{
							return;
						}
					}
				}
				else if ((int)growthStage == 0 && environment.objects.ContainsKey(tileLocation))
				{
					return;
				}
				if ((int)treeType == 8)
				{
					if (Game1.random.NextDouble() < 0.15 || (fertilized.Value && Game1.random.NextDouble() < 0.6))
					{
						growthStage.Value++;
					}
				}
				else if (Game1.random.NextDouble() < 0.2 || fertilized.Value)
				{
					growthStage.Value++;
				}
			}
			if (Game1.GetSeasonForLocation(currentLocation).Equals("winter") && (int)treeType == 7)
			{
				stump.Value = true;
			}
			else if ((int)treeType == 7 && Game1.dayOfMonth <= 1 && Game1.currentSeason.Equals("spring"))
			{
				stump.Value = false;
				health.Value = 10f;
				shakeRotation = 0f;
			}
			if ((int)growthStage >= 5 && environment is Farm && Game1.random.NextDouble() < 0.15)
			{
				int xCoord = Game1.random.Next(-3, 4) + (int)tileLocation.X;
				int yCoord = Game1.random.Next(-3, 4) + (int)tileLocation.Y;
				Vector2 location = new Vector2(xCoord, yCoord);
				string noSpawn = environment.doesTileHaveProperty(xCoord, yCoord, "NoSpawn", "Back");
				if ((noSpawn == null || (!noSpawn.Equals("Tree") && !noSpawn.Equals("All") && !noSpawn.Equals("True"))) && environment.isTileLocationOpen(new Location(xCoord, yCoord)) && !environment.isTileOccupied(location) && environment.doesTileHaveProperty(xCoord, yCoord, "Water", "Back") == null && environment.isTileOnMap(location))
				{
					environment.terrainFeatures.Add(location, new Tree(treeType, 0));
				}
			}
			hasSeed.Value = false;
			float seedChance = 0.05f;
			if ((int)treeType == 9)
			{
				seedChance *= 3f;
			}
			if ((int)growthStage >= 5 && Game1.random.NextDouble() < (double)seedChance)
			{
				hasSeed.Value = true;
			}
		}

		public override bool seasonUpdate(bool onLoad)
		{
			loadSprite();
			return false;
		}

		public override bool isActionable()
		{
			if (!tapped)
			{
				return (int)growthStage >= 3;
			}
			return false;
		}

		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			if (explosion > 0)
			{
				tapped.Value = false;
			}
			if ((bool)tapped)
			{
				return false;
			}
			if ((float)health <= -99f)
			{
				return false;
			}
			if ((int)growthStage >= 5)
			{
				if (t != null && t is Axe)
				{
					location.playSound("axchop");
					lastPlayerToHit.Value = t.getLastFarmerToUse().UniqueMultiplayerID;
					location.debris.Add(new Debris(12, Game1.random.Next(1, 3), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), t.getLastFarmerToUse().Position, 0, ((int)treeType == 7) ? 10000 : (-1)));
					if (!stump && t.getLastFarmerToUse() != null && location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.005)
					{
						Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
						if (o != null)
						{
							Game1.createItemDebris(o, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.getStandingY() - 32);
						}
					}
				}
				else if (explosion <= 0)
				{
					return false;
				}
				shake(tileLocation, doEvenIfStillShaking: true, location);
				float damage2 = 1f;
				if (explosion > 0)
				{
					damage2 = explosion;
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
						damage2 = 1f;
						break;
					case 1:
						damage2 = 1.25f;
						break;
					case 2:
						damage2 = 1.67f;
						break;
					case 3:
						damage2 = 2.5f;
						break;
					case 4:
						damage2 = 5f;
						break;
					}
					if ((int)t.upgradeLevel > 4)
					{
						damage2 = (int)t.upgradeLevel + 1;
					}
				}
				if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(damage2 / 5f))
				{
					Debris d = new Debris(388, new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()));
					d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
					d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
					location.debris.Add(d);
				}
				health.Value -= damage2;
				if ((float)health <= 0f && performTreeFall(t, explosion, tileLocation, location))
				{
					return true;
				}
			}
			else if ((int)growthStage >= 3)
			{
				if (t != null && t.BaseName.Contains("Ax"))
				{
					location.playSound("axchop");
					if ((int)treeType != 7)
					{
						location.playSound("leafrustle");
					}
					location.debris.Add(new Debris(12, Game1.random.Next((int)t.upgradeLevel * 2, (int)t.upgradeLevel * 4), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), new Vector2(t.getLastFarmerToUse().GetBoundingBox().Center.X, t.getLastFarmerToUse().GetBoundingBox().Center.Y), 0));
				}
				else if (explosion <= 0)
				{
					return false;
				}
				shake(tileLocation, doEvenIfStillShaking: true, location);
				float damage = 1f;
				if (Game1.IsMultiplayer)
				{
					_ = Game1.recentMultiplayerRandom;
				}
				else
				{
					new Random((int)((float)(double)Game1.uniqueIDForThisGame + tileLocation.X * 7f + tileLocation.Y * 11f + (float)(double)Game1.stats.DaysPlayed + (float)health));
				}
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
					if ((int)t.upgradeLevel > 4)
					{
						damage = 10 + ((int)t.upgradeLevel - 4);
					}
				}
				health.Value -= damage;
				if ((float)health <= 0f)
				{
					performBushDestroy(tileLocation, location);
					return true;
				}
			}
			else if ((int)growthStage >= 1)
			{
				if (explosion > 0)
				{
					location.playSound("cut");
					return true;
				}
				if (t != null && t.BaseName.Contains("Axe"))
				{
					location.playSound("axchop");
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
				}
				if (t is Axe || t is Pickaxe || t is Hoe || t is MeleeWeapon)
				{
					location.playSound("cut");
					performSproutDestroy(t, tileLocation, location);
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
					performSeedDestroy(t, tileLocation, location);
					return true;
				}
			}
			return false;
		}

		public bool fertilize(GameLocation location)
		{
			if ((int)growthStage >= 5)
			{
				Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:TreeFertilizer1");
				location.playSound("cancel");
				return false;
			}
			if (fertilized.Value)
			{
				Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:TreeFertilizer2");
				location.playSound("cancel");
				return false;
			}
			fertilized.Value = true;
			location.playSound("dirtyHit");
			return true;
		}

		public bool instantDestroy(Vector2 tileLocation, GameLocation location)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			if ((int)growthStage >= 5)
			{
				return performTreeFall(null, 0, tileLocation, location);
			}
			if ((int)growthStage >= 3)
			{
				performBushDestroy(tileLocation, location);
				return true;
			}
			if ((int)growthStage >= 1)
			{
				performSproutDestroy(null, tileLocation, location);
				return true;
			}
			performSeedDestroy(null, tileLocation, location);
			return true;
		}

		private void performSeedDestroy(Tool t, Vector2 tileLocation, GameLocation location)
		{
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
			if ((long)lastPlayerToHit != 0L && Game1.getFarmer(lastPlayerToHit).getEffectiveSkillLevel(2) >= 1)
			{
				Game1.createMultipleObjectDebris(308 + (int)treeType, (int)tileLocation.X, (int)tileLocation.Y, 1, t.getLastFarmerToUse().uniqueMultiplayerID, location);
			}
			else if (Game1.player.getEffectiveSkillLevel(2) >= 1 && (int)treeType <= 3)
			{
				Game1.createMultipleObjectDebris(308 + (int)treeType, (int)tileLocation.X, (int)tileLocation.Y, 1, (t == null) ? Game1.player.uniqueMultiplayerID : t.getLastFarmerToUse().uniqueMultiplayerID, location);
			}
			else if (Game1.player.getEffectiveSkillLevel(2) >= 1 && (int)treeType == 8)
			{
				Game1.createMultipleObjectDebris(292, (int)tileLocation.X, (int)tileLocation.Y, 1, (t == null) ? Game1.player.uniqueMultiplayerID : t.getLastFarmerToUse().uniqueMultiplayerID, location);
			}
		}

		public void UpdateTapperProduct(Object tapper_instance, Object previous_object = null)
		{
			float time_multiplier = 1f;
			if (tapper_instance != null && (int)tapper_instance.parentSheetIndex == 264)
			{
				time_multiplier = 0.5f;
			}
			switch ((int)treeType)
			{
			case 4:
			case 5:
			case 6:
				break;
			case 8:
			{
				Random r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + 73137);
				tapper_instance.heldObject.Value = new Object(92, r.Next(3, 8));
				tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, (int)Math.Max(1.0, Math.Floor(1f * time_multiplier)));
				break;
			}
			case 2:
				tapper_instance.heldObject.Value = new Object(724, 1);
				tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, (int)Math.Max(1.0, Math.Floor(9f * time_multiplier)));
				break;
			case 1:
				tapper_instance.heldObject.Value = new Object(725, 1);
				tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, (int)Math.Max(1.0, Math.Floor(7f * time_multiplier)));
				break;
			case 3:
				tapper_instance.heldObject.Value = new Object(726, 1);
				tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, (int)Math.Max(1.0, Math.Floor(5f * time_multiplier)));
				break;
			case 7:
				if (previous_object == null)
				{
					tapper_instance.heldObject.Value = new Object(420, 1);
					tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
					if (!Game1.GetSeasonForLocation(currentLocation).Equals("fall"))
					{
						tapper_instance.heldObject.Value = new Object(404, 1);
						tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 2);
					}
					break;
				}
				switch (previous_object.ParentSheetIndex)
				{
				case 422:
					tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
					tapper_instance.heldObject.Value = new Object(420, 1);
					break;
				case 404:
				case 420:
					tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
					tapper_instance.heldObject.Value = new Object(previous_object.ParentSheetIndex, 1);
					if (!Game1.GetSeasonForLocation(currentLocation).Equals("fall"))
					{
						tapper_instance.heldObject.Value = new Object(404, 1);
						tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 2);
					}
					if (Game1.dayOfMonth % 10 == 0)
					{
						tapper_instance.heldObject.Value = new Object(422, 1);
					}
					if (Game1.GetSeasonForLocation(currentLocation).Equals("winter"))
					{
						int daysTilSpring = new WorldDate(Game1.year + 1, "spring", 1).TotalDays - Game1.Date.TotalDays;
						tapper_instance.minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, daysTilSpring);
					}
					break;
				}
				break;
			}
		}

		private void performSproutDestroy(Tool t, Vector2 tileLocation, GameLocation location)
		{
			Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
			if (t != null && t.BaseName.Contains("Axe") && Game1.recentMultiplayerRandom.NextDouble() < (double)((float)t.getLastFarmerToUse().ForagingLevel / 10f))
			{
				Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, 1);
			}
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
		}

		private void performBushDestroy(Vector2 tileLocation, GameLocation location)
		{
			if ((int)treeType == 7)
			{
				Game1.createMultipleObjectDebris(420, (int)tileLocation.X, (int)tileLocation.Y, 1, location);
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(20, 30), resource: false, -1, item: false, 10000);
			}
			else
			{
				Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit).professions.Contains(12) ? 1.25 : 1.0) * 4.0), location);
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(20, 30), resource: false);
			}
		}

		private bool performTreeFall(Tool t, int explosion, Vector2 tileLocation, GameLocation location)
		{
			if (!stump)
			{
				if (t != null || explosion > 0)
				{
					location.playSound("treecrack");
				}
				stump.Value = true;
				health.Value = 5f;
				falling.Value = true;
				if (t != null && t.getLastFarmerToUse().IsLocalPlayer)
				{
					t?.getLastFarmerToUse().gainExperience(2, 12);
					if (t == null || t.getLastFarmerToUse() == null)
					{
						shakeLeft.Value = true;
					}
					else
					{
						shakeLeft.Value = ((float)t.getLastFarmerToUse().getStandingX() > (tileLocation.X + 0.5f) * 64f);
					}
				}
			}
			else
			{
				if (t != null && (float)health != -100f && t.getLastFarmerToUse().IsLocalPlayer)
				{
					t?.getLastFarmerToUse().gainExperience(2, 1);
				}
				health.Value = -100f;
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(30, 40), resource: false, -1, item: false, ((int)treeType == 7) ? 10000 : (-1));
				int whatToDrop = ((int)treeType == 7 && tileLocation.X % 7f == 0f) ? 422 : (((int)treeType == 7) ? 420 : (((int)treeType == 8) ? 709 : 92));
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
					if (location.Equals(Game1.currentLocation))
					{
						Game1.createMultipleObjectDebris(92, (int)tileLocation.X, (int)tileLocation.Y, 2, location);
					}
					else
					{
						Game1.createItemDebris(new Object(92, 1), tileLocation * 64f, 2, location);
						Game1.createItemDebris(new Object(92, 1), tileLocation * 64f, 2, location);
					}
				}
				else if (Game1.IsMultiplayer)
				{
					Game1.createMultipleObjectDebris(whatToDrop, (int)tileLocation.X, (int)tileLocation.Y, 1, lastPlayerToHit, location);
					if ((int)treeType != 7 && (int)treeType != 8)
					{
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit).professions.Contains(12) ? 1.25 : 1.0) * 4.0), resource: true);
					}
				}
				else
				{
					if ((int)treeType != 7 && (int)treeType != 8)
					{
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((Game1.getFarmer(lastPlayerToHit).professions.Contains(12) ? 1.25 : 1.0) * (double)(5 + extraWoodCalculator(tileLocation))), resource: true);
					}
					Game1.createMultipleObjectDebris(whatToDrop, (int)tileLocation.X, (int)tileLocation.Y, 1, location);
				}
				if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
				{
					Game1.createObjectDebris(890, (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
				}
				location.playSound("treethud");
				if (!falling)
				{
					return true;
				}
			}
			return false;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			layerDepth += positionOnScreen.X / 100000f;
			if ((int)growthStage < 5)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect = Microsoft.Xna.Framework.Rectangle.Empty;
				switch ((int)growthStage)
				{
				case 0:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 16);
					break;
				case 1:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 128, 16, 16);
					break;
				case 2:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16);
					break;
				default:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 32);
					break;
				}
				spriteBatch.Draw(texture.Value, positionOnScreen - new Vector2(0f, (float)sourceRect.Height * scale), sourceRect, Color.White, 0f, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + (float)sourceRect.Height * scale) / 20000f);
			}
			else
			{
				if (!falling)
				{
					spriteBatch.Draw(texture.Value, positionOnScreen + new Vector2(0f, -64f * scale), new Microsoft.Xna.Framework.Rectangle(32, 96, 16, 32), Color.White, 0f, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
				}
				if (!stump || (bool)falling)
				{
					spriteBatch.Draw(texture.Value, positionOnScreen + new Vector2(-64f * scale, -320f * scale), new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 96), Color.White, shakeRotation, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale) / 20000f);
				}
			}
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			if (isTemporarilyInvisible)
			{
				return;
			}
			if ((int)growthStage < 5)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect = Microsoft.Xna.Framework.Rectangle.Empty;
				switch ((int)growthStage)
				{
				case 0:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 16);
					break;
				case 1:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 128, 16, 16);
					break;
				case 2:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16);
					break;
				default:
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 32);
					break;
				}
				spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - (float)(sourceRect.Height * 4 - 64) + (float)(((int)growthStage >= 3) ? 128 : 64))), sourceRect, fertilized ? Color.HotPink : Color.White, shakeRotation, new Vector2(8f, ((int)growthStage >= 3) ? 32 : 16), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)growthStage == 0) ? 0.0001f : ((float)getBoundingBox(tileLocation).Bottom / 10000f));
			}
			else
			{
				if (!stump || (bool)falling)
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 51f, tileLocation.Y * 64f - 16f)), shadowSourceRect, Color.White * ((float)Math.PI / 2f - Math.Abs(shakeRotation)), 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
					Microsoft.Xna.Framework.Rectangle source_rect = treeTopSourceRect;
					if (treeType.Value == 9)
					{
						if (hasSeed.Value)
						{
							source_rect.X = 48;
						}
						else
						{
							source_rect.X = 0;
						}
					}
					spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), source_rect, Color.White * alpha, shakeRotation, new Vector2(24f, 96f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(getBoundingBox(tileLocation).Bottom + 2) / 10000f - tileLocation.X / 1000000f);
				}
				if ((float)health >= 1f || (!falling && (float)health > -99f))
				{
					spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 3f) : 0f), tileLocation.Y * 64f - 64f)), stumpSourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(tileLocation).Bottom / 10000f);
				}
				if ((bool)stump && (float)health < 4f && (float)health > -99f)
				{
					spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 3f) : 0f), tileLocation.Y * 64f)), new Microsoft.Xna.Framework.Rectangle(Math.Min(2, (int)(3f - (float)health)) * 16, 144, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(getBoundingBox(tileLocation).Bottom + 1) / 10000f);
				}
			}
			foreach (Leaf i in leaves)
			{
				spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, i.position), new Microsoft.Xna.Framework.Rectangle(16 + i.type % 2 * 8, 112 + i.type / 2 * 8, 8, 8), Color.White, i.rotation, Vector2.Zero, 4f, SpriteEffects.None, (float)getBoundingBox(tileLocation).Bottom / 10000f + 0.01f);
			}
		}
	}
}
