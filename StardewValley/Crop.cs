using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class Crop : INetObject<NetFields>
	{
		public const int mixedSeedIndex = 770;

		public const int seedPhase = 0;

		public const int grabHarvest = 0;

		public const int sickleHarvest = 1;

		public const int rowOfWildSeeds = 23;

		public const int finalPhaseLength = 99999;

		public const int forageCrop_springOnion = 1;

		public const int forageCrop_ginger = 2;

		public readonly NetIntList phaseDays = new NetIntList();

		[XmlElement("rowInSpriteSheet")]
		public readonly NetInt rowInSpriteSheet = new NetInt();

		[XmlElement("phaseToShow")]
		public readonly NetInt phaseToShow = new NetInt(-1);

		[XmlElement("currentPhase")]
		public readonly NetInt currentPhase = new NetInt();

		[XmlElement("harvestMethod")]
		public readonly NetInt harvestMethod = new NetInt();

		[XmlElement("indexOfHarvest")]
		public readonly NetInt indexOfHarvest = new NetInt();

		[XmlElement("regrowAfterHarvest")]
		public readonly NetInt regrowAfterHarvest = new NetInt();

		[XmlElement("dayOfCurrentPhase")]
		public readonly NetInt dayOfCurrentPhase = new NetInt();

		[XmlElement("minHarvest")]
		public readonly NetInt minHarvest = new NetInt();

		[XmlElement("maxHarvest")]
		public readonly NetInt maxHarvest = new NetInt();

		[XmlElement("maxHarvestIncreasePerFarmingLevel")]
		public readonly NetInt maxHarvestIncreasePerFarmingLevel = new NetInt();

		[XmlElement("daysOfUnclutteredGrowth")]
		public readonly NetInt daysOfUnclutteredGrowth = new NetInt();

		[XmlElement("whichForageCrop")]
		public readonly NetInt whichForageCrop = new NetInt();

		public readonly NetStringList seasonsToGrowIn = new NetStringList();

		[XmlElement("tintColor")]
		public readonly NetColor tintColor = new NetColor();

		[XmlElement("flip")]
		public readonly NetBool flip = new NetBool();

		[XmlElement("fullGrown")]
		public readonly NetBool fullyGrown = new NetBool();

		[XmlElement("raisedSeeds")]
		public readonly NetBool raisedSeeds = new NetBool();

		[XmlElement("programColored")]
		public readonly NetBool programColored = new NetBool();

		[XmlElement("dead")]
		public readonly NetBool dead = new NetBool();

		[XmlElement("forageCrop")]
		public readonly NetBool forageCrop = new NetBool();

		[XmlElement("chanceForExtraCrops")]
		public readonly NetDouble chanceForExtraCrops = new NetDouble(0.0);

		[XmlElement("seedIndex")]
		public readonly NetInt netSeedIndex = new NetInt(-1);

		private Vector2 drawPosition;

		private Vector2 tilePosition;

		private float layerDepth;

		private float coloredLayerDepth;

		private Rectangle sourceRect;

		private Rectangle coloredSourceRect;

		private static Vector2 origin = new Vector2(8f, 24f);

		private static Vector2 smallestTileSizeOrigin = new Vector2(8f, 8f);

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public Crop()
		{
			NetFields.AddFields(phaseDays, rowInSpriteSheet, phaseToShow, currentPhase, harvestMethod, indexOfHarvest, regrowAfterHarvest, dayOfCurrentPhase, minHarvest, maxHarvest, maxHarvestIncreasePerFarmingLevel, daysOfUnclutteredGrowth, whichForageCrop, seasonsToGrowIn, tintColor, flip, fullyGrown, raisedSeeds, programColored, dead, forageCrop, chanceForExtraCrops, netSeedIndex);
			dayOfCurrentPhase.fieldChangeVisibleEvent += delegate
			{
				updateDrawMath(tilePosition);
			};
			fullyGrown.fieldChangeVisibleEvent += delegate
			{
				updateDrawMath(tilePosition);
			};
		}

		public Crop(bool forageCrop, int which, int tileX, int tileY)
			: this()
		{
			this.forageCrop.Value = forageCrop;
			whichForageCrop.Value = which;
			fullyGrown.Value = true;
			currentPhase.Value = 5;
			updateDrawMath(new Vector2(tileX, tileY));
		}

		public Crop(int seedIndex, int tileX, int tileY)
			: this()
		{
			Dictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
			if (seedIndex == 770)
			{
				seedIndex = getRandomLowGradeCropForThisSeason(Game1.currentSeason);
				if (seedIndex == 473)
				{
					seedIndex--;
				}
				if (Game1.currentLocation is IslandLocation)
				{
					switch (Game1.random.Next(4))
					{
					case 0:
						seedIndex = 479;
						break;
					case 1:
						seedIndex = 833;
						break;
					case 2:
						seedIndex = 481;
						break;
					case 3:
						seedIndex = 478;
						break;
					}
				}
			}
			if (cropData.ContainsKey(seedIndex))
			{
				string[] split = cropData[seedIndex].Split('/');
				string[] phaseSplit = split[0].Split(' ');
				for (int k = 0; k < phaseSplit.Length; k++)
				{
					phaseDays.Add(Convert.ToInt32(phaseSplit[k]));
				}
				phaseDays.Add(99999);
				string[] seasonSplit = split[1].Split(' ');
				for (int j = 0; j < seasonSplit.Length; j++)
				{
					seasonsToGrowIn.Add(seasonSplit[j]);
				}
				rowInSpriteSheet.Value = Convert.ToInt32(split[2]);
				if ((int)rowInSpriteSheet == 23)
				{
					whichForageCrop.Value = seedIndex;
				}
				else
				{
					netSeedIndex.Value = seedIndex;
				}
				indexOfHarvest.Value = Convert.ToInt32(split[3]);
				regrowAfterHarvest.Value = Convert.ToInt32(split[4]);
				harvestMethod.Value = Convert.ToInt32(split[5]);
				ResetCropYield();
				raisedSeeds.Value = Convert.ToBoolean(split[7]);
				string[] programColors = split[8].Split(' ');
				if (programColors.Length != 0 && programColors[0].Equals("true"))
				{
					List<Color> colors = new List<Color>();
					for (int i = 1; i < programColors.Length; i += 3)
					{
						colors.Add(new Color(Convert.ToByte(programColors[i]), Convert.ToByte(programColors[i + 1]), Convert.ToByte(programColors[i + 2])));
					}
					Random r = new Random(tileX * 1000 + tileY + Game1.dayOfMonth);
					tintColor.Value = colors[r.Next(colors.Count)];
					programColored.Value = true;
				}
				flip.Value = (Game1.random.NextDouble() < 0.5);
			}
			updateDrawMath(new Vector2(tileX, tileY));
		}

		public void InferSeedIndex()
		{
			Dictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
			foreach (int key in cropData.Keys)
			{
				if (Convert.ToInt32(cropData[key].Split('/')[3]) == indexOfHarvest.Value)
				{
					netSeedIndex.Value = key;
					break;
				}
			}
		}

		public void ResetCropYield()
		{
			Dictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
			int seedIndex2 = -1;
			if ((int)rowInSpriteSheet != 23 && netSeedIndex.Value == -1)
			{
				InferSeedIndex();
			}
			seedIndex2 = (((int)rowInSpriteSheet != 23) ? netSeedIndex.Value : whichForageCrop.Value);
			if (cropData.ContainsKey(seedIndex2))
			{
				string[] cropYieldSplit = cropData[seedIndex2].Split('/')[6].Split(' ');
				if (cropYieldSplit.Length != 0 && cropYieldSplit[0].Equals("true"))
				{
					minHarvest.Value = Convert.ToInt32(cropYieldSplit[1]);
					maxHarvest.Value = Convert.ToInt32(cropYieldSplit[2]);
					maxHarvestIncreasePerFarmingLevel.Value = Convert.ToInt32(cropYieldSplit[3]);
					chanceForExtraCrops.Value = Convert.ToDouble(cropYieldSplit[4]);
				}
				else
				{
					minHarvest.Value = 1;
					maxHarvest.Value = 1;
					maxHarvestIncreasePerFarmingLevel.Value = 0;
					chanceForExtraCrops.Value = 0.0;
				}
			}
		}

		public void ResetPhaseDays()
		{
			Dictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
			int seedIndex2 = -1;
			if ((int)rowInSpriteSheet != 23 && netSeedIndex.Value == -1)
			{
				InferSeedIndex();
			}
			seedIndex2 = (((int)rowInSpriteSheet != 23) ? netSeedIndex.Value : whichForageCrop.Value);
			if (cropData.ContainsKey(seedIndex2))
			{
				string[] phaseSplit = cropData[seedIndex2].Split('/')[0].Split(' ');
				phaseDays.Clear();
				for (int i = 0; i < phaseSplit.Length; i++)
				{
					phaseDays.Add(Convert.ToInt32(phaseSplit[i]));
				}
				phaseDays.Add(99999);
			}
		}

		public static int getRandomLowGradeCropForThisSeason(string season)
		{
			if (season.Equals("winter"))
			{
				season = ((Game1.random.NextDouble() < 0.33) ? "spring" : ((Game1.random.NextDouble() < 0.5) ? "summer" : "fall"));
			}
			if (!(season == "spring"))
			{
				if (!(season == "summer"))
				{
					if (season == "fall")
					{
						return Game1.random.Next(487, 491);
					}
				}
				else
				{
					switch (Game1.random.Next(4))
					{
					case 0:
						return 487;
					case 1:
						return 483;
					case 2:
						return 482;
					case 3:
						return 484;
					}
				}
				return -1;
			}
			return Game1.random.Next(472, 476);
		}

		public void growCompletely()
		{
			currentPhase.Value = phaseDays.Count - 1;
			dayOfCurrentPhase.Value = 0;
			if ((int)regrowAfterHarvest != -1)
			{
				fullyGrown.Value = true;
			}
			updateDrawMath(tilePosition);
		}

		public bool hitWithHoe(int xTile, int yTile, GameLocation location, HoeDirt dirt)
		{
			if ((bool)forageCrop && (int)whichForageCrop == 2)
			{
				dirt.state.Value = (Game1.IsRainingHere(location) ? 1 : 0);
				Object harvestedItem = new Object(829, 1);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(xTile * 64, yTile * 64), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
				location.playSound("dirtyHit");
				Game1.createItemDebris(harvestedItem.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
				return true;
			}
			return false;
		}

		public bool harvest(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester = null)
		{
			if ((bool)dead)
			{
				if (junimoHarvester != null)
				{
					return true;
				}
				return false;
			}
			bool success = false;
			if ((bool)forageCrop)
			{
				Object o = null;
				int experience2 = 3;
				Random r2 = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + xTile * 1000 + yTile * 2000);
				switch ((int)whichForageCrop)
				{
				case 1:
					o = new Object(399, 1);
					break;
				case 2:
					soil.shake((float)Math.PI / 48f, (float)Math.PI / 40f, (float)(xTile * 64) < Game1.player.Position.X);
					return false;
				}
				if (Game1.player.professions.Contains(16))
				{
					o.Quality = 4;
				}
				else if (r2.NextDouble() < (double)((float)Game1.player.ForagingLevel / 30f))
				{
					o.Quality = 2;
				}
				else if (r2.NextDouble() < (double)((float)Game1.player.ForagingLevel / 15f))
				{
					o.Quality = 1;
				}
				Game1.stats.ItemsForaged += (uint)o.Stack;
				if (junimoHarvester != null)
				{
					junimoHarvester.tryToAddItemToHut(o);
					return true;
				}
				if (Game1.player.addItemToInventoryBool(o))
				{
					Vector2 initialTile2 = new Vector2(xTile, yTile);
					Game1.player.animateOnce(279 + Game1.player.FacingDirection);
					Game1.player.canMove = false;
					Game1.player.currentLocation.playSound("harvest");
					DelayedAction.playSoundAfterDelay("coin", 260);
					if ((int)regrowAfterHarvest == -1)
					{
						Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(17, new Vector2(initialTile2.X * 64f, initialTile2.Y * 64f), Color.White, 7, r2.NextDouble() < 0.5, 125f));
						Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(14, new Vector2(initialTile2.X * 64f, initialTile2.Y * 64f), Color.White, 7, r2.NextDouble() < 0.5, 50f));
					}
					Game1.player.gainExperience(2, experience2);
					return true;
				}
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			else if ((int)currentPhase >= phaseDays.Count - 1 && (!fullyGrown || (int)dayOfCurrentPhase <= 0))
			{
				int numToHarvest = 1;
				int cropQuality = 0;
				int fertilizerQualityLevel = 0;
				if ((int)indexOfHarvest == 0)
				{
					return true;
				}
				Random r = new Random(xTile * 7 + yTile * 11 + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame);
				switch ((int)soil.fertilizer)
				{
				case 368:
					fertilizerQualityLevel = 1;
					break;
				case 369:
					fertilizerQualityLevel = 2;
					break;
				case 919:
					fertilizerQualityLevel = 3;
					break;
				}
				double chanceForGoldQuality = 0.2 * ((double)Game1.player.FarmingLevel / 10.0) + 0.2 * (double)fertilizerQualityLevel * (((double)Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
				double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);
				if (fertilizerQualityLevel >= 3 && r.NextDouble() < chanceForGoldQuality / 2.0)
				{
					cropQuality = 4;
				}
				else if (r.NextDouble() < chanceForGoldQuality)
				{
					cropQuality = 2;
				}
				else if (r.NextDouble() < chanceForSilverQuality || fertilizerQualityLevel >= 3)
				{
					cropQuality = 1;
				}
				if ((int)minHarvest > 1 || (int)maxHarvest > 1)
				{
					int max_harvest_increase = 0;
					if (maxHarvestIncreasePerFarmingLevel.Value > 0)
					{
						max_harvest_increase = Game1.player.FarmingLevel / (int)maxHarvestIncreasePerFarmingLevel;
					}
					numToHarvest = r.Next(minHarvest, Math.Max((int)minHarvest + 1, (int)maxHarvest + 1 + max_harvest_increase));
				}
				if ((double)chanceForExtraCrops > 0.0)
				{
					while (r.NextDouble() < Math.Min(0.9, chanceForExtraCrops))
					{
						numToHarvest++;
					}
				}
				if ((int)indexOfHarvest == 771 || (int)indexOfHarvest == 889)
				{
					cropQuality = 0;
				}
				Object harvestedItem2 = programColored ? new ColoredObject(indexOfHarvest, 1, tintColor)
				{
					Quality = cropQuality
				} : new Object(indexOfHarvest, 1, isRecipe: false, -1, cropQuality);
				if ((int)harvestMethod == 1)
				{
					if (junimoHarvester != null)
					{
						DelayedAction.playSoundAfterDelay("daggerswipe", 150, junimoHarvester.currentLocation);
					}
					if (junimoHarvester != null && Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
					{
						junimoHarvester.currentLocation.playSound("harvest");
					}
					if (junimoHarvester != null && Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
					{
						DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
					}
					if (junimoHarvester != null)
					{
						junimoHarvester.tryToAddItemToHut(harvestedItem2.getOne());
					}
					else
					{
						Game1.createItemDebris(harvestedItem2.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
					}
					success = true;
				}
				else if (junimoHarvester != null || Game1.player.addItemToInventoryBool(harvestedItem2.getOne()))
				{
					Vector2 initialTile = new Vector2(xTile, yTile);
					if (junimoHarvester == null)
					{
						Game1.player.animateOnce(279 + Game1.player.FacingDirection);
						Game1.player.canMove = false;
					}
					else
					{
						junimoHarvester.tryToAddItemToHut(harvestedItem2.getOne());
					}
					if (r.NextDouble() < Game1.player.team.AverageLuckLevel() / 1500.0 + Game1.player.team.AverageDailyLuck() / 1200.0 + 9.9999997473787516E-05)
					{
						numToHarvest *= 2;
						if (junimoHarvester == null)
						{
							Game1.player.currentLocation.playSound("dwoop");
						}
						else if (Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
						{
							junimoHarvester.currentLocation.playSound("dwoop");
						}
					}
					else if ((int)harvestMethod == 0)
					{
						if (junimoHarvester == null)
						{
							Game1.player.currentLocation.playSound("harvest");
						}
						else if (Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
						{
							junimoHarvester.currentLocation.playSound("harvest");
						}
						if (junimoHarvester == null)
						{
							DelayedAction.playSoundAfterDelay("coin", 260, Game1.player.currentLocation);
						}
						else if (Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
						{
							DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
						}
						if ((int)regrowAfterHarvest == -1 && (junimoHarvester == null || junimoHarvester.currentLocation.Equals(Game1.currentLocation)))
						{
							Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(17, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 7, Game1.random.NextDouble() < 0.5, 125f));
							Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(14, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 7, Game1.random.NextDouble() < 0.5, 50f));
						}
					}
					success = true;
				}
				else
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
				}
				if (success)
				{
					if ((int)indexOfHarvest == 421)
					{
						indexOfHarvest.Value = 431;
						numToHarvest = r.Next(1, 4);
					}
					int price = Convert.ToInt32(Game1.objectInformation[indexOfHarvest].Split('/')[1]);
					harvestedItem2 = (programColored ? new ColoredObject(indexOfHarvest, 1, tintColor) : new Object(indexOfHarvest, 1));
					float experience = (float)(16.0 * Math.Log(0.018 * (double)price + 1.0, Math.E));
					if (junimoHarvester == null)
					{
						Game1.player.gainExperience(0, (int)Math.Round(experience));
					}
					for (int i = 0; i < numToHarvest - 1; i++)
					{
						if (junimoHarvester == null)
						{
							Game1.createItemDebris(harvestedItem2.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
						}
						else
						{
							junimoHarvester.tryToAddItemToHut(harvestedItem2.getOne());
						}
					}
					if ((int)indexOfHarvest == 262 && r.NextDouble() < 0.4)
					{
						Object hay_item = new Object(178, 1);
						if (junimoHarvester == null)
						{
							Game1.createItemDebris(hay_item.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
						}
						else
						{
							junimoHarvester.tryToAddItemToHut(hay_item.getOne());
						}
					}
					else if ((int)indexOfHarvest == 771)
					{
						if (soil != null && soil.currentLocation != null)
						{
							soil.currentLocation.playSound("cut");
						}
						if (r.NextDouble() < 0.1)
						{
							Object mixedSeeds_item = new Object(770, 1);
							if (junimoHarvester == null)
							{
								Game1.createItemDebris(mixedSeeds_item.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
							}
							else
							{
								junimoHarvester.tryToAddItemToHut(mixedSeeds_item.getOne());
							}
						}
					}
					if ((int)regrowAfterHarvest == -1)
					{
						return true;
					}
					fullyGrown.Value = true;
					if (dayOfCurrentPhase.Value == (int)regrowAfterHarvest)
					{
						updateDrawMath(tilePosition);
					}
					dayOfCurrentPhase.Value = regrowAfterHarvest;
				}
			}
			return false;
		}

		public int getRandomWildCropForSeason(string season)
		{
			if (!(season == "spring"))
			{
				if (!(season == "summer"))
				{
					if (!(season == "fall"))
					{
						if (season == "winter")
						{
							return 412 + Game1.random.Next(4) * 2;
						}
						return 22;
					}
					return 404 + Game1.random.Next(4) * 2;
				}
				if (!(Game1.random.NextDouble() < 0.33))
				{
					if (!(Game1.random.NextDouble() < 0.5))
					{
						return 402;
					}
					return 398;
				}
				return 396;
			}
			return 16 + Game1.random.Next(4) * 2;
		}

		private Rectangle getSourceRect(int number)
		{
			if ((bool)dead)
			{
				return new Rectangle(192 + number % 4 * 16, 384, 16, 32);
			}
			int effectiveRow = rowInSpriteSheet;
			if ((int)indexOfHarvest == 771)
			{
				if (Game1.currentSeason == "fall")
				{
					effectiveRow = (int)rowInSpriteSheet + 1;
				}
				else if (Game1.currentSeason == "winter")
				{
					effectiveRow = (int)rowInSpriteSheet + 2;
				}
			}
			return new Rectangle(Math.Min(240, ((!fullyGrown) ? ((int)(((int)phaseToShow != -1) ? phaseToShow : currentPhase) + (((int)(((int)phaseToShow != -1) ? phaseToShow : currentPhase) == 0 && number % 2 == 0) ? (-1) : 0) + 1) : (((int)dayOfCurrentPhase <= 0) ? 6 : 7)) * 16 + ((effectiveRow % 2 != 0) ? 128 : 0)), effectiveRow / 2 * 16 * 2, 16, 32);
		}

		public void Kill()
		{
			dead.Value = true;
			raisedSeeds.Value = false;
		}

		public void newDay(int state, int fertilizer, int xTile, int yTile, GameLocation environment)
		{
			if ((bool)environment.isOutdoors && ((bool)dead || (!environment.SeedsIgnoreSeasonsHere() && !seasonsToGrowIn.Contains(environment.GetSeasonForLocation())) || (!environment.SeedsIgnoreSeasonsHere() && (int)indexOfHarvest == 90)))
			{
				Kill();
				return;
			}
			if (state == 1 || (int)indexOfHarvest == 771)
			{
				if (!fullyGrown)
				{
					dayOfCurrentPhase.Value = Math.Min((int)dayOfCurrentPhase + 1, (phaseDays.Count > 0) ? phaseDays[Math.Min(phaseDays.Count - 1, currentPhase)] : 0);
				}
				else
				{
					dayOfCurrentPhase.Value--;
				}
				if ((int)dayOfCurrentPhase >= ((phaseDays.Count > 0) ? phaseDays[Math.Min(phaseDays.Count - 1, currentPhase)] : 0) && (int)currentPhase < phaseDays.Count - 1)
				{
					currentPhase.Value++;
					dayOfCurrentPhase.Value = 0;
				}
				while ((int)currentPhase < phaseDays.Count - 1 && phaseDays.Count > 0 && phaseDays[currentPhase] <= 0)
				{
					currentPhase.Value++;
				}
				if ((int)rowInSpriteSheet == 23 && (int)phaseToShow == -1 && (int)currentPhase > 0)
				{
					phaseToShow.Value = Game1.random.Next(1, 7);
				}
				if (environment is Farm && (int)currentPhase == phaseDays.Count - 1 && ((int)indexOfHarvest == 276 || (int)indexOfHarvest == 190 || (int)indexOfHarvest == 254) && OneTimeRandom.GetDouble(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (ulong)xTile, (ulong)yTile) < 0.01)
				{
					for (int x2 = xTile - 1; x2 <= xTile + 1; x2++)
					{
						for (int y = yTile - 1; y <= yTile + 1; y++)
						{
							Vector2 v2 = new Vector2(x2, y);
							if (!environment.terrainFeatures.ContainsKey(v2) || !(environment.terrainFeatures[v2] is HoeDirt) || (environment.terrainFeatures[v2] as HoeDirt).crop == null || (environment.terrainFeatures[v2] as HoeDirt).crop.indexOfHarvest != indexOfHarvest)
							{
								return;
							}
						}
					}
					for (int x = xTile - 1; x <= xTile + 1; x++)
					{
						for (int y2 = yTile - 1; y2 <= yTile + 1; y2++)
						{
							Vector2 v3 = new Vector2(x, y2);
							(environment.terrainFeatures[v3] as HoeDirt).crop = null;
						}
					}
					(environment as Farm).resourceClumps.Add(new GiantCrop(indexOfHarvest, new Vector2(xTile - 1, yTile - 1)));
				}
			}
			if ((!fullyGrown || (int)dayOfCurrentPhase <= 0) && (int)currentPhase >= phaseDays.Count - 1 && (int)rowInSpriteSheet == 23)
			{
				Vector2 v = new Vector2(xTile, yTile);
				string season = Game1.currentSeason;
				switch ((int)whichForageCrop)
				{
				case 495:
					season = "spring";
					break;
				case 496:
					season = "summer";
					break;
				case 497:
					season = "fall";
					break;
				case 498:
					season = "winter";
					break;
				}
				if (environment.objects.ContainsKey(v))
				{
					if (environment.objects[v] is IndoorPot)
					{
						(environment.objects[v] as IndoorPot).heldObject.Value = new Object(v, getRandomWildCropForSeason(season), 1);
						(environment.objects[v] as IndoorPot).hoeDirt.Value.crop = null;
					}
					else
					{
						environment.objects.Remove(v);
					}
				}
				if (!environment.objects.ContainsKey(v))
				{
					environment.objects.Add(v, new Object(v, getRandomWildCropForSeason(season), 1)
					{
						IsSpawnedObject = true,
						CanBeGrabbed = true
					});
				}
				if (environment.terrainFeatures.ContainsKey(v) && environment.terrainFeatures[v] != null && environment.terrainFeatures[v] is HoeDirt)
				{
					(environment.terrainFeatures[v] as HoeDirt).crop = null;
				}
			}
			updateDrawMath(new Vector2(xTile, yTile));
		}

		public virtual bool isPaddyCrop()
		{
			if (indexOfHarvest.Value == 271 || indexOfHarvest.Value == 830)
			{
				return true;
			}
			return false;
		}

		public virtual bool shouldDrawDarkWhenWatered()
		{
			if (isPaddyCrop())
			{
				return false;
			}
			return !raisedSeeds.Value;
		}

		public bool isWildSeedCrop()
		{
			return (int)rowInSpriteSheet == 23;
		}

		public void updateDrawMath(Vector2 tileLocation)
		{
			if (!tileLocation.Equals(Vector2.Zero))
			{
				if ((bool)forageCrop)
				{
					drawPosition = new Vector2(tileLocation.X * 64f + ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f) + 32f, tileLocation.Y * 64f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f) + 32f);
					layerDepth = (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f;
					sourceRect = new Rectangle((int)(tileLocation.X * 51f + tileLocation.Y * 77f) % 3 * 16, 128 + (int)whichForageCrop * 16, 16, 16);
				}
				else
				{
					drawPosition = new Vector2(tileLocation.X * 64f + ((!shouldDrawDarkWhenWatered() || (int)currentPhase >= phaseDays.Count - 1) ? 0f : ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f)) + 32f, tileLocation.Y * 64f + (((bool)raisedSeeds || (int)currentPhase >= phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) + 32f);
					layerDepth = (tileLocation.Y * 64f + 32f + ((!shouldDrawDarkWhenWatered() || (int)currentPhase >= phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f))) / 10000f / (((int)currentPhase == 0 && shouldDrawDarkWhenWatered()) ? 2f : 1f);
					sourceRect = getSourceRect((int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
					coloredSourceRect = new Rectangle(((!fullyGrown) ? ((int)currentPhase + 1 + 1) : (((int)dayOfCurrentPhase <= 0) ? 6 : 7)) * 16 + (((int)rowInSpriteSheet % 2 != 0) ? 128 : 0), (int)rowInSpriteSheet / 2 * 16 * 2, 16, 32);
					coloredLayerDepth = (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f / (float)(((int)currentPhase != 0 || !shouldDrawDarkWhenWatered()) ? 1 : 2);
				}
				tilePosition = tileLocation;
			}
		}

		public void draw(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
		{
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, drawPosition);
			if ((bool)forageCrop)
			{
				if ((int)whichForageCrop == 2)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f) + 32f, tileLocation.Y * 64f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f) + 64f)), new Rectangle(128 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(tileLocation.X * 111f + tileLocation.Y * 77f)) % 800.0 / 200.0) * 16, 128, 16, 16), Color.White, rotation, new Vector2(8f, 16f), 4f, SpriteEffects.None, (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, position, sourceRect, Color.White, 0f, smallestTileSizeOrigin, 4f, SpriteEffects.None, layerDepth);
				}
				return;
			}
			SpriteEffects effect = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			b.Draw(Game1.cropSpriteSheet, position, sourceRect, toTint, rotation, origin, 4f, effect, layerDepth);
			Color tintColor = this.tintColor.Value;
			if (!tintColor.Equals(Color.White) && (int)currentPhase == phaseDays.Count - 1 && !dead)
			{
				b.Draw(Game1.cropSpriteSheet, position, coloredSourceRect, tintColor, rotation, origin, 4f, effect, coloredLayerDepth);
			}
		}

		public void drawInMenu(SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth)
		{
			b.Draw(Game1.cropSpriteSheet, screenPosition, getSourceRect(0), toTint, rotation, new Vector2(32f, 96f), scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}

		public void drawWithOffset(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, Vector2 offset)
		{
			if ((bool)forageCrop)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), sourceRect, Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
				return;
			}
			b.Draw(Game1.cropSpriteSheet, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), sourceRect, toTint, rotation, new Vector2(8f, 24f), 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
			if (!tintColor.Equals(Color.White) && (int)currentPhase == phaseDays.Count - 1 && !dead)
			{
				b.Draw(Game1.cropSpriteSheet, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), coloredSourceRect, tintColor, rotation, new Vector2(8f, 24f), 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.67f) * 64f / 10000f + tileLocation.X * 1E-05f);
			}
		}
	}
}
