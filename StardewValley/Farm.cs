using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley
{
	public class Farm : BuildableGameLocation, IAnimalLocation
	{
		public class LightningStrikeEvent : NetEventArg
		{
			public Vector2 boltPosition;

			public bool createBolt;

			public bool bigFlash;

			public bool smallFlash;

			public bool destroyedTerrainFeature;

			public void Read(BinaryReader reader)
			{
				createBolt = reader.ReadBoolean();
				bigFlash = reader.ReadBoolean();
				smallFlash = reader.ReadBoolean();
				destroyedTerrainFeature = reader.ReadBoolean();
				boltPosition.X = reader.ReadInt32();
				boltPosition.Y = reader.ReadInt32();
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write(createBolt);
				writer.Write(bigFlash);
				writer.Write(smallFlash);
				writer.Write(destroyedTerrainFeature);
				writer.Write((int)boltPosition.X);
				writer.Write((int)boltPosition.Y);
			}
		}

		[XmlIgnore]
		public static readonly Texture2D houseTextures = Game1.content.Load<Texture2D>("Buildings\\houses");

		[XmlIgnore]
		public Texture2D paintedHouseTexture;

		public Color? frameHouseColor;

		public NetRef<BuildingPaintColor> housePaintColor = new NetRef<BuildingPaintColor>();

		public const int default_layout = 0;

		public const int riverlands_layout = 1;

		public const int forest_layout = 2;

		public const int mountains_layout = 3;

		public const int combat_layout = 4;

		public const int fourCorners_layout = 5;

		public const int beach_layout = 6;

		public const int layout_max = 6;

		[XmlElement("animals")]
		public readonly NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = new NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>();

		[XmlElement("piecesOfHay")]
		public readonly NetInt piecesOfHay = new NetInt(0);

		[XmlElement("grandpaScore")]
		public NetInt grandpaScore = new NetInt(0);

		[XmlElement("farmCaveReady")]
		public NetBool farmCaveReady = new NetBool(value: false);

		private TemporaryAnimatedSprite shippingBinLid;

		private Microsoft.Xna.Framework.Rectangle shippingBinLidOpenArea = new Microsoft.Xna.Framework.Rectangle(4480, 832, 256, 192);

		[XmlIgnore]
		private readonly NetCollection<Item> sharedShippingBin = new NetCollection<Item>();

		[XmlIgnore]
		public Item lastItemShipped;

		public bool hasSeenGrandpaNote;

		[XmlElement("houseSource")]
		public readonly NetRectangle houseSource = new NetRectangle();

		[XmlElement("greenhouseUnlocked")]
		public readonly NetBool greenhouseUnlocked = new NetBool();

		[XmlElement("greenhouseMoved")]
		public readonly NetBool greenhouseMoved = new NetBool();

		private readonly NetEvent1Field<Vector2, NetVector2> spawnCrowEvent = new NetEvent1Field<Vector2, NetVector2>();

		public readonly NetEvent1<LightningStrikeEvent> lightningStrikeEvent = new NetEvent1<LightningStrikeEvent>();

		private readonly List<KeyValuePair<long, FarmAnimal>> _tempAnimals = new List<KeyValuePair<long, FarmAnimal>>();

		public readonly NetBool petBowlWatered = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetPoint petBowlPosition = new NetPoint();

		[XmlIgnore]
		public Point? mapGrandpaShrinePosition;

		[XmlIgnore]
		public Point? mapMainMailboxPosition;

		[XmlIgnore]
		public Point? mainFarmhouseEntry;

		[XmlIgnore]
		public Vector2? mapSpouseAreaCorner;

		[XmlIgnore]
		public Vector2? mapShippingBinPosition;

		private int chimneyTimer = 500;

		public const int numCropsForCrow = 16;

		public NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals => animals;

		public Farm()
		{
		}

		public Farm(string mapPath, string name)
			: base(mapPath, name)
		{
			if (Game1.IsMasterGame)
			{
				Layer building_layer = map.GetLayer("Buildings");
				for (int x = 0; x < building_layer.LayerWidth; x++)
				{
					for (int y = 0; y < building_layer.LayerHeight; y++)
					{
						if (building_layer.Tiles[x, y] != null && building_layer.Tiles[x, y].TileIndex == 1938)
						{
							petBowlPosition.Set(x, y);
						}
					}
				}
			}
			AddModularShippingBin();
		}

		public virtual void AddModularShippingBin()
		{
			Building building2 = new ShippingBin(new BluePrint("Shipping Bin"), GetStarterShippingBinLocation());
			buildings.Add(building2);
			building2.load();
			building2 = new GreenhouseBuilding(new BluePrint("Greenhouse"), GetGreenhouseStartLocation());
			buildings.Add(building2);
			building2.load();
		}

		public virtual Microsoft.Xna.Framework.Rectangle GetHouseRect()
		{
			Point house_entry = GetMainFarmHouseEntry();
			return new Microsoft.Xna.Framework.Rectangle(house_entry.X - 5, house_entry.Y - 4, 9, 6);
		}

		public virtual Vector2 GetStarterShippingBinLocation()
		{
			if (!mapShippingBinPosition.HasValue)
			{
				mapShippingBinPosition = Utility.PointToVector2(GetMapPropertyPosition("ShippingBinLocation", 71, 14));
			}
			return mapShippingBinPosition.Value;
		}

		public virtual Vector2 GetGreenhouseStartLocation()
		{
			if (map.Properties.ContainsKey("GreenhouseLocation"))
			{
				int x = -1;
				int y = -1;
				string[] split = map.Properties["GreenhouseLocation"].ToString().Split(' ');
				if (split.Length >= 2 && int.TryParse(split[0], out x) && int.TryParse(split[1], out y))
				{
					return new Vector2(x, y);
				}
			}
			if (Game1.whichFarm == 5)
			{
				return new Vector2(36f, 29f);
			}
			if (Game1.whichFarm == 6)
			{
				return new Vector2(14f, 14f);
			}
			return new Vector2(25f, 10f);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(animals, piecesOfHay, sharedShippingBin, houseSource, spawnCrowEvent, petBowlWatered, petBowlPosition, lightningStrikeEvent, grandpaScore, greenhouseUnlocked, greenhouseMoved, housePaintColor, farmCaveReady);
			spawnCrowEvent.onEvent += doSpawnCrow;
			lightningStrikeEvent.onEvent += doLightningStrike;
			greenhouseMoved.fieldChangeVisibleEvent += delegate
			{
				ClearGreenhouseGrassTiles();
			};
			petBowlWatered.fieldChangeVisibleEvent += delegate
			{
				_UpdateWaterBowl();
			};
			if (housePaintColor.Value == null)
			{
				housePaintColor.Value = new BuildingPaintColor();
			}
		}

		public virtual void ClearGreenhouseGrassTiles()
		{
			if (map != null && Game1.gameMode != 6 && greenhouseMoved.Value)
			{
				if (Game1.whichFarm == 0 || Game1.whichFarm == 4 || Game1.whichFarm == 3)
				{
					ApplyMapOverride("Farm_Greenhouse_Dirt", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle((int)GetGreenhouseStartLocation().X, (int)GetGreenhouseStartLocation().Y, 9, 6));
				}
				else if (Game1.whichFarm == 5)
				{
					ApplyMapOverride("Farm_Greenhouse_Dirt_FourCorners", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle((int)GetGreenhouseStartLocation().X, (int)GetGreenhouseStartLocation().Y, 9, 6));
				}
			}
		}

		protected void _UpdateWaterBowl()
		{
			if (petBowlWatered.Value)
			{
				setMapTileIndex(petBowlPosition.X, petBowlPosition.Y, 1939, "Buildings");
			}
			else
			{
				setMapTileIndex(petBowlPosition.X, petBowlPosition.Y, 1938, "Buildings");
			}
		}

		public static string getMapNameFromTypeInt(int type)
		{
			switch (type)
			{
			case 0:
				return "Farm";
			case 1:
				return "Farm_Fishing";
			case 2:
				return "Farm_Foraging";
			case 3:
				return "Farm_Mining";
			case 4:
				return "Farm_Combat";
			case 5:
				return "Farm_FourCorners";
			case 6:
				return "Farm_Island";
			default:
				return "Farm";
			}
		}

		public Point GetPetStartLocation()
		{
			return new Point(petBowlPosition.X - 1, petBowlPosition.Y + 1);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			for (int i = animals.Count() - 1; i >= 0; i--)
			{
				animals.Pairs.ElementAt(i).Value.dayUpdate(this);
			}
			base.DayUpdate(dayOfMonth);
			for (int j = characters.Count - 1; j >= 0; j--)
			{
				if (characters[j] is Pet && (getTileIndexAt(characters[j].getTileLocationPoint(), "Buildings") != -1 || getTileIndexAt(characters[j].getTileX() + 1, characters[j].getTileY(), "Buildings") != -1 || !isTileLocationTotallyClearAndPlaceable(characters[j].getTileLocation()) || !isTileLocationTotallyClearAndPlaceable(new Vector2(characters[j].getTileX() + 1, characters[j].getTileY()))))
				{
					characters[j].setTilePosition(GetPetStartLocation());
				}
			}
			lastItemShipped = null;
			for (int k = characters.Count - 1; k >= 0; k--)
			{
				if (characters[k] is JunimoHarvester)
				{
					characters.RemoveAt(k);
				}
			}
			for (int l = characters.Count - 1; l >= 0; l--)
			{
				if (characters[l] is Monster && (characters[l] as Monster).wildernessFarmMonster)
				{
					characters.RemoveAt(l);
				}
			}
			if (characters.Count > 5)
			{
				int slimesEscaped = 0;
				for (int m = characters.Count - 1; m >= 0; m--)
				{
					if (characters[m] is GreenSlime && Game1.random.NextDouble() < 0.035)
					{
						characters.RemoveAt(m);
						slimesEscaped++;
					}
				}
				if (slimesEscaped > 0)
				{
					Game1.multiplayer.broadcastGlobalMessage((slimesEscaped == 1) ? "Strings\\Locations:Farm_1SlimeEscaped" : "Strings\\Locations:Farm_NSlimesEscaped", false, string.Concat(slimesEscaped));
				}
			}
			if (Game1.whichFarm == 5)
			{
				if (isTileLocationTotallyClearAndPlaceable(5, 32) && isTileLocationTotallyClearAndPlaceable(6, 32) && isTileLocationTotallyClearAndPlaceable(6, 33) && isTileLocationTotallyClearAndPlaceable(5, 33))
				{
					resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2(5f, 32f)));
				}
				if (objects.Count() > 0)
				{
					for (int i3 = 0; i3 < 6; i3++)
					{
						Object o2 = objects.Pairs.ElementAt(Game1.random.Next(objects.Count())).Value;
						if (o2.name.Equals("Weeds") && o2.tileLocation.X < 36f && o2.tileLocation.Y < 34f)
						{
							o2.ParentSheetIndex = 792 + Utility.getSeasonNumber(Game1.currentSeason);
						}
					}
				}
			}
			else if (Game1.whichFarm == 6)
			{
				while (Game1.random.NextDouble() < 0.9)
				{
					Vector2 v2 = getRandomTile();
					if (!isTileLocationTotallyClearAndPlaceable(v2) || getTileIndexAt((int)v2.X, (int)v2.Y, "AlwaysFront") != -1)
					{
						continue;
					}
					int whichItem2 = -1;
					if (doesTileHavePropertyNoNull((int)v2.X, (int)v2.Y, "BeachSpawn", "Back") != "")
					{
						whichItem2 = 372;
						Game1.stats.incrementStat("beachFarmSpawns", 1);
						switch (Game1.random.Next(6))
						{
						case 0:
							whichItem2 = 393;
							break;
						case 1:
							whichItem2 = 719;
							break;
						case 2:
							whichItem2 = 718;
							break;
						case 3:
							whichItem2 = 723;
							break;
						case 4:
						case 5:
							whichItem2 = 152;
							break;
						}
						if (Game1.stats.DaysPlayed > 1)
						{
							if (Game1.random.NextDouble() < 0.15 || Game1.stats.getStat("beachFarmSpawns") % 4u == 0)
							{
								whichItem2 = Game1.random.Next(922, 925);
								objects.Add(v2, new Object(v2, whichItem2, 1)
								{
									Fragility = 2,
									MinutesUntilReady = 3
								});
								whichItem2 = -1;
							}
							else if (Game1.random.NextDouble() < 0.1)
							{
								whichItem2 = 397;
							}
							else if (Game1.random.NextDouble() < 0.05)
							{
								whichItem2 = 392;
							}
							else if (Game1.random.NextDouble() < 0.02)
							{
								whichItem2 = 394;
							}
						}
					}
					else if (Game1.currentSeason != "winter" && new Microsoft.Xna.Framework.Rectangle(20, 66, 33, 18).Contains((int)v2.X, (int)v2.Y) && doesTileHavePropertyNoNull((int)v2.X, (int)v2.Y, "Type", "Back") == "Grass")
					{
						whichItem2 = Utility.getRandomBasicSeasonalForageItem(Game1.currentSeason, (int)Game1.stats.DaysPlayed);
					}
					if (whichItem2 != -1)
					{
						dropObject(new Object(v2, whichItem2, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: true), v2 * 64f, Game1.viewport, initialPlacement: true);
					}
				}
			}
			if (Game1.whichFarm == 2)
			{
				for (int x = 0; x < 20; x++)
				{
					for (int y = 0; y < map.Layers[0].LayerHeight; y++)
					{
						if (map.GetLayer("Paths").Tiles[x, y] != null && map.GetLayer("Paths").Tiles[x, y].TileIndex == 21 && isTileLocationTotallyClearAndPlaceable(x, y) && isTileLocationTotallyClearAndPlaceable(x + 1, y) && isTileLocationTotallyClearAndPlaceable(x + 1, y + 1) && isTileLocationTotallyClearAndPlaceable(x, y + 1))
						{
							resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2(x, y)));
						}
					}
				}
				if (!Game1.IsWinter)
				{
					while (Game1.random.NextDouble() < 0.75)
					{
						Vector2 v = new Vector2(Game1.random.Next(18), Game1.random.Next(map.Layers[0].LayerHeight));
						if (Game1.random.NextDouble() < 0.5)
						{
							v = getRandomTile();
						}
						if (!isTileLocationTotallyClearAndPlaceable(v) || getTileIndexAt((int)v.X, (int)v.Y, "AlwaysFront") != -1 || (!(v.X < 18f) && !doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back").Equals("Grass")))
						{
							continue;
						}
						int whichItem = 792;
						switch (Game1.currentSeason)
						{
						case "spring":
							switch (Game1.random.Next(4))
							{
							case 0:
								whichItem = 16;
								break;
							case 1:
								whichItem = 22;
								break;
							case 2:
								whichItem = 20;
								break;
							case 3:
								whichItem = 257;
								break;
							}
							break;
						case "summer":
							switch (Game1.random.Next(4))
							{
							case 0:
								whichItem = 402;
								break;
							case 1:
								whichItem = 396;
								break;
							case 2:
								whichItem = 398;
								break;
							case 3:
								whichItem = 404;
								break;
							}
							break;
						case "fall":
							switch (Game1.random.Next(4))
							{
							case 0:
								whichItem = 281;
								break;
							case 1:
								whichItem = 420;
								break;
							case 2:
								whichItem = 422;
								break;
							case 3:
								whichItem = 404;
								break;
							}
							break;
						}
						dropObject(new Object(v, whichItem, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: true), v * 64f, Game1.viewport, initialPlacement: true);
					}
					if (objects.Count() > 0)
					{
						for (int i5 = 0; i5 < 6; i5++)
						{
							Object o3 = objects.Pairs.ElementAt(Game1.random.Next(objects.Count())).Value;
							if (o3.name.Equals("Weeds"))
							{
								o3.ParentSheetIndex = 792 + Utility.getSeasonNumber(Game1.currentSeason);
							}
						}
					}
				}
			}
			if (Game1.whichFarm == 3 || Game1.whichFarm == 5)
			{
				doDailyMountainFarmUpdate();
			}
			ICollection<Vector2> objKeys = new List<Vector2>(terrainFeatures.Keys);
			for (int i4 = objKeys.Count - 1; i4 >= 0; i4--)
			{
				if (terrainFeatures[objKeys.ElementAt(i4)] is HoeDirt && (terrainFeatures[objKeys.ElementAt(i4)] as HoeDirt).crop == null && Game1.random.NextDouble() <= 0.1)
				{
					terrainFeatures.Remove(objKeys.ElementAt(i4));
				}
			}
			if (terrainFeatures.Count() > 0 && Game1.currentSeason.Equals("fall") && Game1.dayOfMonth > 1 && Game1.random.NextDouble() < 0.05)
			{
				for (int tries = 0; tries < 10; tries++)
				{
					TerrainFeature t = terrainFeatures.Pairs.ElementAt(Game1.random.Next(terrainFeatures.Count())).Value;
					if (t is Tree && (int)(t as Tree).growthStage >= 5 && !(t as Tree).tapped)
					{
						(t as Tree).treeType.Value = 7;
						(t as Tree).loadSprite();
						break;
					}
				}
			}
			addCrows();
			if (!Game1.currentSeason.Equals("winter"))
			{
				spawnWeedsAndStones(Game1.currentSeason.Equals("summer") ? 30 : 20);
			}
			spawnWeeds(weedsOnly: false);
			if (dayOfMonth == 1)
			{
				for (int i2 = terrainFeatures.Count() - 1; i2 >= 0; i2--)
				{
					if (terrainFeatures.Pairs.ElementAt(i2).Value is HoeDirt && (terrainFeatures.Pairs.ElementAt(i2).Value as HoeDirt).crop == null && Game1.random.NextDouble() < 0.8)
					{
						terrainFeatures.Remove(terrainFeatures.Pairs.ElementAt(i2).Key);
					}
				}
				spawnWeedsAndStones(20, weedsOnly: false, spawnFromOldWeeds: false);
				if (Game1.currentSeason.Equals("spring") && Game1.stats.DaysPlayed > 1)
				{
					spawnWeedsAndStones(40, weedsOnly: false, spawnFromOldWeeds: false);
					spawnWeedsAndStones(40, weedsOnly: true, spawnFromOldWeeds: false);
					for (int n = 0; n < 15; n++)
					{
						int xCoord = Game1.random.Next(map.DisplayWidth / 64);
						int yCoord = Game1.random.Next(map.DisplayHeight / 64);
						Vector2 location = new Vector2(xCoord, yCoord);
						objects.TryGetValue(location, out Object o);
						if (o == null && doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null && doesTileHaveProperty(xCoord, yCoord, "NoSpawn", "Back") == null && isTileLocationOpen(new Location(xCoord, yCoord)) && !isTileOccupied(location) && doesTileHaveProperty(xCoord, yCoord, "Water", "Back") == null)
						{
							terrainFeatures.Add(location, new Grass(1, 4));
						}
					}
					growWeedGrass(40);
				}
			}
			growWeedGrass(1);
		}

		public void doDailyMountainFarmUpdate()
		{
			double chance = 1.0;
			while (Game1.random.NextDouble() < chance)
			{
				Vector2 v = (Game1.whichFarm == 5) ? Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(51, 67, 11, 3), Game1.random) : Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(5, 37, 22, 8), Game1.random);
				if (doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back").Equals("Dirt") && isTileLocationTotallyClearAndPlaceable(v))
				{
					int whichStone = 668;
					int health = 2;
					if (Game1.random.NextDouble() < 0.15)
					{
						objects.Add(v, new Object(v, 590, 1));
						continue;
					}
					if (Game1.random.NextDouble() < 0.5)
					{
						whichStone = 670;
					}
					if (Game1.random.NextDouble() < 0.1)
					{
						if (Game1.player.MiningLevel >= 8 && Game1.random.NextDouble() < 0.33)
						{
							whichStone = 77;
							health = 7;
						}
						else if (Game1.player.MiningLevel >= 5 && Game1.random.NextDouble() < 0.5)
						{
							whichStone = 76;
							health = 5;
						}
						else
						{
							whichStone = 75;
							health = 3;
						}
					}
					if (Game1.random.NextDouble() < 0.21)
					{
						whichStone = 751;
						health = 3;
					}
					if (Game1.player.MiningLevel >= 4 && Game1.random.NextDouble() < 0.15)
					{
						whichStone = 290;
						health = 4;
					}
					if (Game1.player.MiningLevel >= 7 && Game1.random.NextDouble() < 0.1)
					{
						whichStone = 764;
						health = 8;
					}
					if (Game1.player.MiningLevel >= 10 && Game1.random.NextDouble() < 0.01)
					{
						whichStone = 765;
						health = 16;
					}
					objects.Add(v, new Object(v, whichStone, 10)
					{
						MinutesUntilReady = health
					});
				}
				chance *= 0.75;
			}
		}

		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			if (Game1.whichFarm == 6)
			{
				if (x > 28 && x < 57 && y > 46 && y < 82)
				{
					return false;
				}
				return true;
			}
			return base.catchOceanCrabPotFishFromThisSpot(x, y);
		}

		public override float getExtraTrashChanceForCrabPot(int x, int y)
		{
			if (Game1.whichFarm == 6)
			{
				if (x > 28 && x < 57 && y > 46 && y < 82)
				{
					return 0.25f;
				}
				return 0f;
			}
			return base.getExtraTrashChanceForCrabPot(x, y);
		}

		public void addCrows()
		{
			int numCrops = 0;
			foreach (KeyValuePair<Vector2, TerrainFeature> v2 in terrainFeatures.Pairs)
			{
				if (v2.Value is HoeDirt && (v2.Value as HoeDirt).crop != null)
				{
					numCrops++;
				}
			}
			List<Vector2> scarecrowPositions = new List<Vector2>();
			foreach (KeyValuePair<Vector2, Object> v3 in objects.Pairs)
			{
				if ((bool)v3.Value.bigCraftable && v3.Value.Name.Contains("arecrow"))
				{
					scarecrowPositions.Add(v3.Key);
				}
			}
			int potentialCrows = Math.Min(4, numCrops / 16);
			for (int i = 0; i < potentialCrows; i++)
			{
				if (!(Game1.random.NextDouble() < 0.3))
				{
					continue;
				}
				for (int attempts = 0; attempts < 10; attempts++)
				{
					Vector2 v = terrainFeatures.Pairs.ElementAt(Game1.random.Next(terrainFeatures.Count())).Key;
					if (terrainFeatures[v] is HoeDirt && (terrainFeatures[v] as HoeDirt).crop != null && (int)(terrainFeatures[v] as HoeDirt).crop.currentPhase > 1)
					{
						bool scarecrow = false;
						foreach (Vector2 s in scarecrowPositions)
						{
							int radius = objects[s].Name.Contains("Deluxe") ? 17 : 9;
							if (Vector2.Distance(s, v) < (float)radius)
							{
								scarecrow = true;
								objects[s].SpecialVariable++;
								break;
							}
						}
						if (!scarecrow)
						{
							(terrainFeatures[v] as HoeDirt).destroyCrop(v, showAnimation: false, this);
							spawnCrowEvent.Fire(v);
						}
						break;
					}
				}
			}
		}

		private void doSpawnCrow(Vector2 v)
		{
			if (critters == null && (bool)isOutdoors)
			{
				critters = new List<Critter>();
			}
			critters.Add(new Crow((int)v.X, (int)v.Y));
		}

		public static Point getFrontDoorPositionForFarmer(Farmer who)
		{
			Point entry_point = Game1.getFarm().GetMainFarmHouseEntry();
			entry_point.Y--;
			return entry_point;
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (timeOfDay >= 1300 && Game1.IsMasterGame)
			{
				foreach (NPC i in new List<Character>(characters))
				{
					if (i.isMarried())
					{
						i.returnHomeFromFarmPosition(this);
					}
				}
			}
			foreach (NPC c in characters)
			{
				if (c.getSpouse() == Game1.player)
				{
					c.checkForMarriageDialogue(timeOfDay, this);
				}
				if (c is Child)
				{
					(c as Child).tenMinuteUpdate();
				}
			}
			if (!Game1.spawnMonstersAtNight || Game1.farmEvent != null || Game1.timeOfDay < 1900 || !(Game1.random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() / 2.0))
			{
				return;
			}
			if (Game1.random.NextDouble() < 0.25)
			{
				if (Equals(Game1.currentLocation))
				{
					spawnFlyingMonstersOffScreen();
				}
			}
			else
			{
				spawnGroundMonsterOffScreen();
			}
		}

		public void spawnGroundMonsterOffScreen()
		{
			bool success2 = false;
			int i = 0;
			Vector2 spawnLocation;
			while (true)
			{
				if (i < 15)
				{
					spawnLocation = Vector2.Zero;
					spawnLocation = getRandomTile();
					if (Utility.isOnScreen(Utility.Vector2ToPoint(spawnLocation), 64, this))
					{
						spawnLocation.X -= Game1.viewport.Width / 64;
					}
					if (isTileLocationTotallyClearAndPlaceable(spawnLocation))
					{
						break;
					}
					i++;
					continue;
				}
				return;
			}
			if (Game1.player.CombatLevel >= 8 && Game1.random.NextDouble() < 0.15)
			{
				characters.Add(new ShadowBrute(spawnLocation * 64f)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success2 = true;
			}
			else if (Game1.random.NextDouble() < ((Game1.whichFarm == 4) ? 0.66 : 0.33) && isTileLocationTotallyClearAndPlaceable(spawnLocation))
			{
				characters.Add(new RockGolem(spawnLocation * 64f, Game1.player.CombatLevel)
				{
					wildernessFarmMonster = true
				});
				success2 = true;
			}
			else
			{
				int virtualMineLevel = 1;
				if (Game1.player.CombatLevel >= 10)
				{
					virtualMineLevel = 140;
				}
				else if (Game1.player.CombatLevel >= 8)
				{
					virtualMineLevel = 100;
				}
				else if (Game1.player.CombatLevel >= 4)
				{
					virtualMineLevel = 41;
				}
				characters.Add(new GreenSlime(spawnLocation * 64f, virtualMineLevel)
				{
					wildernessFarmMonster = true
				});
				success2 = true;
			}
			if (success2 && Game1.currentLocation.Equals(this))
			{
				foreach (KeyValuePair<Vector2, Object> v in objects.Pairs)
				{
					if (v.Value != null && (bool)v.Value.bigCraftable && (int)v.Value.parentSheetIndex == 83)
					{
						v.Value.shakeTimer = 1000;
						v.Value.showNextIndex.Value = true;
						Game1.currentLightSources.Add(new LightSource(4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, (int)(v.Key.X * 797f + v.Key.Y * 13f + 666f), LightSource.LightContext.None, 0L));
					}
				}
			}
		}

		public void spawnFlyingMonstersOffScreen()
		{
			bool success2 = false;
			Vector2 spawnLocation = Vector2.Zero;
			switch (Game1.random.Next(4))
			{
			case 0:
				spawnLocation.X = Game1.random.Next(map.Layers[0].LayerWidth);
				break;
			case 3:
				spawnLocation.Y = Game1.random.Next(map.Layers[0].LayerHeight);
				break;
			case 1:
				spawnLocation.X = map.Layers[0].LayerWidth - 1;
				spawnLocation.Y = Game1.random.Next(map.Layers[0].LayerHeight);
				break;
			case 2:
				spawnLocation.Y = map.Layers[0].LayerHeight - 1;
				spawnLocation.X = Game1.random.Next(map.Layers[0].LayerWidth);
				break;
			}
			if (Utility.isOnScreen(spawnLocation * 64f, 64))
			{
				spawnLocation.X -= Game1.viewport.Width;
			}
			if (Game1.player.CombatLevel >= 10 && Game1.random.NextDouble() < 0.01 && Game1.player.hasItemInInventoryNamed("Galaxy Sword"))
			{
				characters.Add(new Bat(spawnLocation * 64f, 9999)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success2 = true;
			}
			else if (Game1.player.CombatLevel >= 10 && Game1.random.NextDouble() < 0.25)
			{
				characters.Add(new Bat(spawnLocation * 64f, 172)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success2 = true;
			}
			else if (Game1.player.CombatLevel >= 10 && Game1.random.NextDouble() < 0.25)
			{
				characters.Add(new Serpent(spawnLocation * 64f)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success2 = true;
			}
			else if (Game1.player.CombatLevel >= 8 && Game1.random.NextDouble() < 0.5)
			{
				characters.Add(new Bat(spawnLocation * 64f, 81)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success2 = true;
			}
			else if (Game1.player.CombatLevel >= 5 && Game1.random.NextDouble() < 0.5)
			{
				characters.Add(new Bat(spawnLocation * 64f, 41)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success2 = true;
			}
			else
			{
				characters.Add(new Bat(spawnLocation * 64f, 1)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success2 = true;
			}
			if (success2 && Game1.currentLocation.Equals(this))
			{
				foreach (KeyValuePair<Vector2, Object> v in objects.Pairs)
				{
					if (v.Value != null && (bool)v.Value.bigCraftable && (int)v.Value.parentSheetIndex == 83)
					{
						v.Value.shakeTimer = 1000;
						v.Value.showNextIndex.Value = true;
						Game1.currentLightSources.Add(new LightSource(4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, (int)(v.Key.X * 797f + v.Key.Y * 13f + 666f), LightSource.LightContext.None, 0L));
					}
				}
			}
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			new Point(tileX * 64 + 32, tileY * 64 + 32);
			if (t is MeleeWeapon)
			{
				foreach (FarmAnimal a in animals.Values)
				{
					if (a.GetBoundingBox().Intersects((t as MeleeWeapon).mostRecentArea))
					{
						a.hitWithWeapon(t as MeleeWeapon);
					}
				}
			}
			if (t is WateringCan && (t as WateringCan).WaterLeft > 0 && getTileIndexAt(tileX, tileY, "Buildings") == 1938 && !petBowlWatered.Value)
			{
				petBowlWatered.Set(newValue: true);
				_UpdateWaterBowl();
			}
			return base.performToolAction(t, tileX, tileY);
		}

		public override void timeUpdate(int timeElapsed)
		{
			base.timeUpdate(timeElapsed);
			if (Game1.IsMasterGame)
			{
				foreach (FarmAnimal value in animals.Values)
				{
					value.updatePerTenMinutes(Game1.timeOfDay, this);
				}
			}
			foreach (Building b in buildings)
			{
				if ((int)b.daysOfConstructionLeft <= 0)
				{
					b.performTenMinuteAction(timeElapsed);
					if (b.indoors.Value != null && !Game1.locations.Contains(b.indoors.Value) && timeElapsed >= 10)
					{
						b.indoors.Value.performTenMinuteUpdate(Game1.timeOfDay);
						if (timeElapsed > 10)
						{
							b.indoors.Value.passTimeForObjects(timeElapsed - 10);
						}
					}
				}
			}
		}

		public bool placeAnimal(BluePrint blueprint, Vector2 tileLocation, bool serverCommand, long ownerID)
		{
			for (int y = 0; y < blueprint.tilesHeight; y++)
			{
				for (int x = 0; x < blueprint.tilesWidth; x++)
				{
					Vector2 currentGlobalTilePosition = new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y);
					if (Game1.player.getTileLocation().Equals(currentGlobalTilePosition) || isTileOccupied(currentGlobalTilePosition) || !isTilePassable(new Location((int)currentGlobalTilePosition.X, (int)currentGlobalTilePosition.Y), Game1.viewport))
					{
						return false;
					}
				}
			}
			long id = Game1.multiplayer.getNewID();
			FarmAnimal animal = new FarmAnimal(blueprint.name, id, ownerID);
			animal.Position = new Vector2(tileLocation.X * 64f + 4f, tileLocation.Y * 64f + 64f - (float)animal.Sprite.getHeight() - 4f);
			animals.Add(id, animal);
			if (animal.sound.Value != null && !animal.sound.Value.Equals(""))
			{
				localSound(animal.sound);
			}
			return true;
		}

		public int tryToAddHay(int num)
		{
			int piecesToAdd = Math.Min(Utility.numSilos() * 240 - (int)piecesOfHay, num);
			piecesOfHay.Value += piecesToAdd;
			_ = 0;
			return num - piecesToAdd;
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			if (!glider)
			{
				if (resourceClumps.Count > 0)
				{
					Microsoft.Xna.Framework.Rectangle characterBox = character?.GetBoundingBox() ?? Microsoft.Xna.Framework.Rectangle.Empty;
					foreach (ResourceClump resourceClump in resourceClumps)
					{
						Microsoft.Xna.Framework.Rectangle stumpBox = resourceClump.getBoundingBox(resourceClump.tile);
						if (stumpBox.Intersects(position) && (!isFarmer || character == null || !stumpBox.Intersects(characterBox)))
						{
							return true;
						}
					}
				}
				if (character != null && !(character is FarmAnimal))
				{
					Microsoft.Xna.Framework.Rectangle playerBox = Game1.player.GetBoundingBox();
					Farmer farmer = isFarmer ? (character as Farmer) : null;
					foreach (FarmAnimal animal in animals.Values)
					{
						if (position.Intersects(animal.GetBoundingBox()) && (!isFarmer || !playerBox.Intersects(animal.GetBoundingBox())))
						{
							if (farmer != null && farmer.TemporaryPassableTiles.Intersects(position))
							{
								break;
							}
							animal.farmerPushing();
							return true;
						}
					}
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
		}

		public bool CheckPetAnimal(Vector2 position, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if (!kvp.Value.wasPet && kvp.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					kvp.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckPetAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if (!kvp.Value.wasPet && kvp.Value.GetBoundingBox().Intersects(rect))
				{
					kvp.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckInspectAnimal(Vector2 position, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if ((bool)kvp.Value.wasPet && kvp.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					kvp.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckInspectAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if ((bool)kvp.Value.wasPet && kvp.Value.GetBoundingBox().Intersects(rect))
				{
					kvp.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public virtual void requestGrandpaReevaluation()
		{
			grandpaScore.Value = 0;
			if (Game1.IsMasterGame)
			{
				Game1.player.eventsSeen.Remove(558292);
				Game1.player.eventsSeen.Add(321777);
			}
			removeTemporarySpritesWithID(6666);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			if (!objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)) && CheckPetAnimal(tileRect, who))
			{
				return true;
			}
			int tileIndexOfCheckLocation = (map.GetLayer("Buildings").Tiles[tileLocation] != null) ? map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : (-1);
			if ((uint)(tileIndexOfCheckLocation - 1956) <= 2u)
			{
				if (!hasSeenGrandpaNote)
				{
					Game1.addMail("hasSeenGrandpaNote", noLetter: true);
					hasSeenGrandpaNote = true;
					Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaNote", Game1.player.Name).Replace('\n', '^'));
					return true;
				}
				if (Game1.year >= 3 && (int)grandpaScore > 0 && (int)grandpaScore < 4)
				{
					if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 72 && (int)grandpaScore < 4)
					{
						who.reduceActiveItemByOne();
						playSound("stoneStep");
						playSound("fireball");
						DelayedAction.playSoundAfterDelay("yoba", 800, this);
						DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_PlaceDiamond"), 1200);
						Game1.multiplayer.broadcastGrandpaReevaluation();
						Game1.player.freezePause = 1200;
						return true;
					}
					if (who.ActiveObject == null || (int)who.ActiveObject.parentSheetIndex != 72)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_DiamondSlot"));
						return true;
					}
				}
				else
				{
					if ((int)grandpaScore >= 4 && !Utility.doesItemWithThisIndexExistAnywhere(160, bigCraftable: true))
					{
						who.addItemByMenuIfNecessaryElseHoldUp(new Object(Vector2.Zero, 160), grandpaStatueCallback);
						return true;
					}
					if ((int)grandpaScore == 0 && Game1.year >= 3)
					{
						Game1.player.eventsSeen.Remove(558292);
						if (!Game1.player.eventsSeen.Contains(321777))
						{
							Game1.player.eventsSeen.Add(321777);
						}
					}
				}
			}
			if (base.checkAction(tileLocation, viewport, who))
			{
				return true;
			}
			if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true) && CheckInspectAnimal(tileRect, who))
			{
				return true;
			}
			return false;
		}

		public void grandpaStatueCallback(Item item, Farmer who)
		{
			if (item != null && item is Object && (bool)(item as Object).bigCraftable && (int)(item as Object).parentSheetIndex == 160)
			{
				who?.mailReceived.Add("grandpaPerfect");
			}
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			housePaintColor.Value = (l as Farm).housePaintColor.Value;
			farmCaveReady.Value = (l as Farm).farmCaveReady.Value;
			if ((l as Farm).hasSeenGrandpaNote)
			{
				Game1.addMail("hasSeenGrandpaNote", noLetter: true);
			}
		}

		public NetCollection<Item> getShippingBin(Farmer who)
		{
			if ((bool)Game1.player.team.useSeparateWallets)
			{
				return who.personalShippingBin;
			}
			return sharedShippingBin;
		}

		public void shipItem(Item i, Farmer who)
		{
			if (i != null)
			{
				who.removeItemFromInventory(i);
				getShippingBin(who).Add(i);
				if (i is Object)
				{
					showShipment(i as Object, playThrowSound: false);
				}
				lastItemShipped = i;
				if (Game1.player.ActiveObject == null)
				{
					Game1.player.showNotCarrying();
					Game1.player.Halt();
				}
			}
		}

		public override bool leftClick(int x, int y, Farmer who)
		{
			return base.leftClick(x, y, who);
		}

		public void showShipment(Object o, bool playThrowSound = true)
		{
			if (playThrowSound)
			{
				localSound("backpackIN");
			}
			DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0);
			int temp = Game1.random.Next();
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 218, 34, 22), new Vector2(71f, 13f) * 64f + new Vector2(0f, 5f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.09601f,
				id = temp,
				extraInfoForEndBehavior = temp,
				endFunction = base.removeTemporarySpritesWithID
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 230, 34, 10), new Vector2(71f, 13f) * 64f + new Vector2(0f, 17f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.0963f,
				id = temp,
				extraInfoForEndBehavior = temp
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, o.parentSheetIndex, 16, 16), new Vector2(71f, 13f) * 64f + new Vector2(8 + Game1.random.Next(6), 2f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 9999f,
				scale = 4f,
				alphaFade = 0.045f,
				layerDepth = 0.096225f,
				motion = new Vector2(0f, 0.3f),
				acceleration = new Vector2(0f, 0.2f),
				scaleChange = -0.05f
			});
		}

		public override int getFishingLocation(Vector2 tile)
		{
			switch (Game1.whichFarm)
			{
			case 3:
				return 0;
			case 1:
			case 2:
			case 5:
				return 1;
			default:
				return -1;
			}
		}

		public override bool doesTileSinkDebris(int tileX, int tileY, Debris.DebrisType type)
		{
			if (isTileBuildingFishable(tileX, tileY))
			{
				return true;
			}
			return base.doesTileSinkDebris(tileX, tileY, type);
		}

		public override bool CanRefillWateringCanOnTile(int tileX, int tileY)
		{
			Vector2 tile = new Vector2(tileX, tileY);
			Building bldg = getBuildingAt(tile);
			if (bldg != null && bldg.CanRefillWateringCan())
			{
				return true;
			}
			return base.CanRefillWateringCanOnTile(tileX, tileY);
		}

		public override bool isTileBuildingFishable(int tileX, int tileY)
		{
			Vector2 tile = new Vector2(tileX, tileY);
			foreach (Building building in buildings)
			{
				if (building.isTileFishable(tile))
				{
					return true;
				}
			}
			return base.isTileBuildingFishable(tileX, tileY);
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string location = null)
		{
			if (location != null && location != base.Name)
			{
				return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, location);
			}
			if (bobberTile != Vector2.Zero)
			{
				foreach (Building b in buildings)
				{
					if (b is FishPond && b.isTileFishable(bobberTile))
					{
						return (b as FishPond).CatchFish();
					}
				}
			}
			if (Game1.whichFarm == 1)
			{
				if (Game1.random.NextDouble() < 0.3)
				{
					return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Forest");
				}
				return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Town");
			}
			if (Game1.whichFarm == 6)
			{
				if (who != null && who.getTileLocation().Equals(new Vector2(23f, 98f)) && !who.mailReceived.Contains("gotBoatPainting"))
				{
					who.mailReceived.Add("gotBoatPainting");
					return new Furniture(2421, Vector2.Zero);
				}
				if (!new Microsoft.Xna.Framework.Rectangle(26, 45, 31, 39).Contains((int)bobberTile.X, (int)bobberTile.Y))
				{
					if (Game1.random.NextDouble() < 0.15)
					{
						return new Object(152, 1);
					}
					if (Game1.random.NextDouble() < 0.06)
					{
						int whichItem = -1;
						switch (Game1.random.Next(4))
						{
						case 0:
							whichItem = 723;
							break;
						case 1:
							whichItem = 393;
							break;
						case 2:
							whichItem = 719;
							break;
						case 3:
							whichItem = 718;
							break;
						}
						return new Object(whichItem, 1);
					}
					if (Game1.random.NextDouble() < 0.66)
					{
						return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Beach");
					}
				}
			}
			else if (Game1.whichFarm == 5)
			{
				if (who != null && who.getTileX() < 40 && who.getTileY() > 54 && Game1.random.NextDouble() <= 0.5)
				{
					if (who.mailReceived.Contains("cursed_doll") && !who.mailReceived.Contains("eric's_prank_1"))
					{
						who.mailReceived.Add("eric's_prank_1");
						return new Object(103, 1);
					}
					return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Forest");
				}
			}
			else if (Game1.whichFarm == 3)
			{
				if (Game1.random.NextDouble() < 0.5)
				{
					return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Forest");
				}
			}
			else if (Game1.whichFarm == 2)
			{
				if (Game1.random.NextDouble() < 0.05 + Game1.player.DailyLuck)
				{
					return new Object(734, 1);
				}
				if (Game1.random.NextDouble() < 0.45)
				{
					return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Forest");
				}
			}
			else if (Game1.whichFarm == 4 && Game1.random.NextDouble() <= 0.35)
			{
				return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Mountain");
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
		}

		public List<FarmAnimal> getAllFarmAnimals()
		{
			List<FarmAnimal> farmAnimals = animals.Values.ToList();
			foreach (Building b in buildings)
			{
				if (b.indoors.Value != null && b.indoors.Value is AnimalHouse)
				{
					farmAnimals.AddRange(((AnimalHouse)(GameLocation)b.indoors).animals.Values.ToList());
				}
			}
			return farmAnimals;
		}

		public override bool isTileOccupied(Vector2 tileLocation, string characterToIgnore = "", bool ignoreAllCharacters = false)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (pair.Value.getTileLocation().Equals(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupied(tileLocation, characterToIgnore, ignoreAllCharacters);
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (!greenhouseUnlocked.Value && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccPantry"))
			{
				greenhouseUnlocked.Value = true;
			}
			ClearGreenhouseGrassTiles();
			houseSource.Value = new Microsoft.Xna.Framework.Rectangle(0, 144 * (((int)Game1.MasterPlayer.houseUpgradeLevel == 3) ? 2 : ((int)Game1.MasterPlayer.houseUpgradeLevel)), 160, 144);
			int i = characters.Count - 1;
			while (true)
			{
				if (i >= 0)
				{
					if (Game1.timeOfDay >= 1300 && characters[i].isMarried() && characters[i].controller == null)
					{
						break;
					}
					i--;
					continue;
				}
				return;
			}
			characters[i].Halt();
			characters[i].drawOffset.Value = Vector2.Zero;
			characters[i].Sprite.StopAnimation();
			FarmHouse farmHouse = Game1.getLocationFromName(characters[i].getSpouse().homeLocation.Value) as FarmHouse;
			Game1.warpCharacter(characters[i], characters[i].getSpouse().homeLocation.Value, farmHouse.getKitchenStandingSpot());
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			hasSeenGrandpaNote = Game1.player.hasOrWillReceiveMail("hasSeenGrandpaNote");
			frameHouseColor = null;
			_UpdateWaterBowl();
			if (!Game1.player.mailReceived.Contains("button_tut_2"))
			{
				Game1.player.mailReceived.Add("button_tut_2");
				Game1.onScreenMenus.Add(new ButtonTutorialMenu(1));
			}
			if (Game1.MasterPlayer.isMarried() && Game1.MasterPlayer.spouse != null)
			{
				addSpouseOutdoorArea(Game1.MasterPlayer.spouse);
			}
			for (int k = characters.Count - 1; k >= 0; k--)
			{
				if (characters[k] is Child)
				{
					(characters[k] as Child).resetForPlayerEntry(this);
				}
				if (characters[k].isVillager() && characters[k].name.Equals(Game1.player.spouse))
				{
					petBowlWatered.Set(newValue: true);
				}
			}
			if (Game1.timeOfDay >= 1830)
			{
				for (int l = animals.Count() - 1; l >= 0; l--)
				{
					animals.Pairs.ElementAt(l).Value.warpHome(this, animals.Pairs.ElementAt(l).Value);
				}
			}
			if (isThereABuildingUnderConstruction() && (int)getBuildingUnderConstruction().daysOfConstructionLeft > 0 && Game1.getCharacterFromName("Robin").currentLocation.Equals(this))
			{
				Building b = getBuildingUnderConstruction();
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(399, 262, ((int)b.daysOfConstructionLeft == 1) ? 29 : 9, 43), new Vector2((int)b.tileX + (int)b.tilesWide / 2, (int)b.tileY + (int)b.tilesHigh / 2) * 64f + new Vector2(-16f, -144f), flipped: false, 0f, Color.White)
				{
					id = 16846f,
					scale = 4f,
					interval = 999999f,
					animationLength = 1,
					totalNumberOfLoops = 99999,
					layerDepth = (float)(((int)b.tileY + (int)b.tilesHigh / 2) * 64 + 32) / 10000f
				});
			}
			else
			{
				removeTemporarySpritesWithIDLocal(16846f);
			}
			addGrandpaCandles();
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("Farm_Eternal_Parrots") && !Game1.IsRainingHere(this))
			{
				for (int j = 0; j < 20; j++)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(49, 24 * Game1.random.Next(4), 24, 24), new Vector2(Game1.viewport.MaxCorner.X, Game1.viewport.Location.Y + Game1.random.Next(64, Game1.viewport.Height / 2)), flipped: false, 0f, Color.White)
					{
						scale = 4f,
						motion = new Vector2(-5f + (float)Game1.random.Next(-10, 11) / 10f, 4f + (float)Game1.random.Next(-10, 11) / 10f),
						acceleration = new Vector2(0f, -0.02f),
						animationLength = 3,
						interval = 100f,
						pingPong = true,
						totalNumberOfLoops = 999,
						delayBeforeAnimationStart = j * 250,
						drawAboveAlwaysFront = true,
						startSound = "batFlap"
					});
				}
				DelayedAction.playSoundAfterDelay("parrot_squawk", 1000);
				DelayedAction.playSoundAfterDelay("parrot_squawk", 4000);
				DelayedAction.playSoundAfterDelay("parrot", 3000);
				DelayedAction.playSoundAfterDelay("parrot", 5500);
				DelayedAction.playSoundAfterDelay("parrot_squawk", 7000);
				for (int i = 0; i < 20; i++)
				{
					DelayedAction.playSoundAfterDelay("batFlap", 5000 + i * 250);
				}
				Game1.player.mailReceived.Add("Farm_Eternal_Parrots");
			}
		}

		public virtual Vector2 GetSpouseOutdoorAreaCorner()
		{
			if (!mapSpouseAreaCorner.HasValue)
			{
				int default_x = 69;
				int default_y = 6;
				if (Game1.whichFarm == 6)
				{
					default_x = 79;
					default_y = 2;
				}
				Point point = GetMapPropertyPosition("SpouseAreaLocation", default_x, default_y);
				mapSpouseAreaCorner = Utility.PointToVector2(point);
			}
			return mapSpouseAreaCorner.Value;
		}

		public virtual int GetSpouseOutdoorAreaSpritesheetIndex()
		{
			if (Game1.whichFarm == 6)
			{
				return 2;
			}
			return 1;
		}

		public void addSpouseOutdoorArea(string spouseName)
		{
			Point patio_corner = Utility.Vector2ToPoint(GetSpouseOutdoorAreaCorner());
			int spritesheet_index = GetSpouseOutdoorAreaSpritesheetIndex();
			string above_always_layer = "AlwaysFront";
			removeTile(patio_corner.X + 1, patio_corner.Y + 3, "Buildings");
			removeTile(patio_corner.X + 2, patio_corner.Y + 3, "Buildings");
			removeTile(patio_corner.X + 3, patio_corner.Y + 3, "Buildings");
			removeTile(patio_corner.X, patio_corner.Y + 3, "Buildings");
			removeTile(patio_corner.X + 1, patio_corner.Y + 2, "Buildings");
			removeTile(patio_corner.X + 2, patio_corner.Y + 2, "Buildings");
			removeTile(patio_corner.X + 3, patio_corner.Y + 2, "Buildings");
			removeTile(patio_corner.X, patio_corner.Y + 2, "Buildings");
			removeTile(patio_corner.X + 1, patio_corner.Y + 1, "Front");
			removeTile(patio_corner.X + 2, patio_corner.Y + 1, "Front");
			removeTile(patio_corner.X + 3, patio_corner.Y + 1, "Front");
			removeTile(patio_corner.X, patio_corner.Y + 1, "Front");
			removeTile(patio_corner.X + 1, patio_corner.Y, above_always_layer);
			removeTile(patio_corner.X + 2, patio_corner.Y, above_always_layer);
			removeTile(patio_corner.X + 3, patio_corner.Y, above_always_layer);
			removeTile(patio_corner.X, patio_corner.Y, above_always_layer);
			switch (spouseName)
			{
			case "Emily":
				setMapTileIndex(patio_corner.X, patio_corner.Y + 2, 1867, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 2, 1867, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X, patio_corner.Y + 1, 1842, "Front", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 1, 1842, "Front", spritesheet_index);
				setMapTileIndex(patio_corner.X, patio_corner.Y + 3, 1866, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 2, patio_corner.Y + 2, 1866, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 3, 1967, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 2, 1967, "Buildings", spritesheet_index);
				break;
			case "Shane":
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 3, 1940, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 2, patio_corner.Y + 3, 1941, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 3, 1942, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 2, 1915, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 2, patio_corner.Y + 2, 1916, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 2, 1917, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 1, 1772, "Front", spritesheet_index);
				setMapTileIndex(patio_corner.X + 2, patio_corner.Y + 1, 1773, "Front", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 1, 1774, "Front", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y, 1747, above_always_layer, spritesheet_index);
				setMapTileIndex(patio_corner.X + 2, patio_corner.Y, 1748, above_always_layer, spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y, 1749, above_always_layer, spritesheet_index);
				break;
			case "Sebastian":
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 2, 1927, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 2, patio_corner.Y + 2, 1928, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 2, 1929, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 1, 1902, "Front", spritesheet_index);
				setMapTileIndex(patio_corner.X + 2, patio_corner.Y + 1, 1903, "Front", spritesheet_index);
				break;
			case "Sam":
				setMapTileIndex(patio_corner.X, patio_corner.Y + 2, 1173, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 2, 1174, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 2, 1198, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 2, patio_corner.Y + 2, 1199, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X, patio_corner.Y + 1, 1148, "Front", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 1, 1149, "Front", spritesheet_index);
				break;
			case "Elliott":
				setMapTileIndex(patio_corner.X, patio_corner.Y + 2, 1098, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 2, 1123, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 2, 1098, "Buildings", spritesheet_index);
				break;
			case "Harvey":
				setMapTileIndex(patio_corner.X, patio_corner.Y + 2, 1098, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 2, 1123, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 2, 1098, "Buildings", spritesheet_index);
				break;
			case "Alex":
				setMapTileIndex(patio_corner.X, patio_corner.Y + 2, 1099, "Buildings", spritesheet_index);
				break;
			case "Maru":
				setMapTileIndex(patio_corner.X + 2, patio_corner.Y + 2, 1124, "Buildings", spritesheet_index);
				break;
			case "Penny":
				setMapTileIndex(patio_corner.X, patio_corner.Y + 2, 1098, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 2, 1123, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 2, 1098, "Buildings", spritesheet_index);
				break;
			case "Haley":
				setMapTileIndex(patio_corner.X, patio_corner.Y + 2, 1074, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X, patio_corner.Y + 1, 1049, "Front", spritesheet_index);
				setMapTileIndex(patio_corner.X, patio_corner.Y, 1024, above_always_layer, spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 2, 1074, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 1, 1049, "Front", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y, 1024, above_always_layer, spritesheet_index);
				break;
			case "Abigail":
				setMapTileIndex(patio_corner.X, patio_corner.Y + 2, 1098, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 2, 1123, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 3, patio_corner.Y + 2, 1098, "Buildings", spritesheet_index);
				break;
			case "Leah":
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 2, 1122, "Buildings", spritesheet_index);
				setMapTileIndex(patio_corner.X + 1, patio_corner.Y + 1, 1097, "Front", spritesheet_index);
				break;
			}
		}

		public void addGrandpaCandles()
		{
			Point grandpa_shrine_location = GetGrandpaShrinePosition();
			if ((int)grandpaScore > 0)
			{
				Microsoft.Xna.Framework.Rectangle candleSource = new Microsoft.Xna.Framework.Rectangle(577, 1985, 2, 5);
				removeTemporarySpritesWithIDLocal(6666f);
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X - 1) * 64 + 20, (grandpa_shrine_location.Y - 1) * 64 + 20), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X - 1) * 64 + 12, (grandpa_shrine_location.Y - 1) * 64 - 4), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					light = true,
					id = 6666f,
					lightRadius = 1f,
					scale = 3f,
					layerDepth = 0.0385000035f,
					delayBeforeAnimationStart = 0
				});
				if ((int)grandpaScore > 1)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X - 1) * 64 + 40, (grandpa_shrine_location.Y - 2) * 64 + 24), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X - 1) * 64 + 36, (grandpa_shrine_location.Y - 2) * 64), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						light = true,
						id = 6666f,
						lightRadius = 1f,
						scale = 3f,
						layerDepth = 0.0385000035f,
						delayBeforeAnimationStart = 50
					});
				}
				if ((int)grandpaScore > 2)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X + 1) * 64 + 20, (grandpa_shrine_location.Y - 2) * 64 + 24), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X + 1) * 64 + 16, (grandpa_shrine_location.Y - 2) * 64), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						light = true,
						id = 6666f,
						lightRadius = 1f,
						scale = 3f,
						layerDepth = 0.0385000035f,
						delayBeforeAnimationStart = 100
					});
				}
				if ((int)grandpaScore > 3)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X + 1) * 64 + 40, (grandpa_shrine_location.Y - 1) * 64 + 20), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X + 1) * 64 + 36, (grandpa_shrine_location.Y - 1) * 64 - 4), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						light = true,
						id = 6666f,
						lightRadius = 1f,
						scale = 3f,
						layerDepth = 0.0385000035f,
						delayBeforeAnimationStart = 150
					});
				}
			}
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(176, 157, 15, 16), 99999f, 1, 9999, new Vector2(grandpa_shrine_location.X * 64 + 4, (grandpa_shrine_location.Y - 2) * 64 - 24), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
			}
		}

		private void openShippingBinLid()
		{
			if (shippingBinLid != null)
			{
				if (shippingBinLid.pingPongMotion != 1 && Game1.currentLocation == this)
				{
					localSound("doorCreak");
				}
				shippingBinLid.pingPongMotion = 1;
				shippingBinLid.paused = false;
			}
		}

		private void closeShippingBinLid()
		{
			if (shippingBinLid != null && shippingBinLid.currentParentTileIndex > 0)
			{
				if (shippingBinLid.pingPongMotion != -1 && Game1.currentLocation == this)
				{
					localSound("doorCreakReverse");
				}
				shippingBinLid.pingPongMotion = -1;
				shippingBinLid.paused = false;
			}
		}

		private void updateShippingBinLid(GameTime time)
		{
			if (isShippingBinLidOpen(requiredToBeFullyOpen: true) && shippingBinLid.pingPongMotion == 1)
			{
				shippingBinLid.paused = true;
			}
			else if (shippingBinLid.currentParentTileIndex == 0 && shippingBinLid.pingPongMotion == -1)
			{
				if (!shippingBinLid.paused && Game1.currentLocation == this)
				{
					localSound("woodyStep");
				}
				shippingBinLid.paused = true;
			}
			shippingBinLid.update(time);
		}

		private bool isShippingBinLidOpen(bool requiredToBeFullyOpen = false)
		{
			if (shippingBinLid != null && shippingBinLid.currentParentTileIndex >= ((!requiredToBeFullyOpen) ? 1 : (shippingBinLid.animationLength - 1)))
			{
				return true;
			}
			return false;
		}

		public override void pokeTileForConstruction(Vector2 tile)
		{
			base.pokeTileForConstruction(tile);
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if (kvp.Value.getTileLocation().Equals(tile))
				{
					kvp.Value.Poke();
				}
			}
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (pair.Value.getTileLocation().Equals(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			if (doesTileHaveProperty((int)p.X, (int)p.Y, "NoSpawn", "Back") == "All" && doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back") == "Wood")
			{
				return true;
			}
			foreach (Building building in buildings)
			{
				if (building.occupiesTile(p) && building.isTilePassable(p))
				{
					return true;
				}
			}
			return base.shouldShadowBeDrawnAboveBuildingsLayer(p);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				pair.Value.draw(b);
			}
			Point entry_position_tile = GetMainFarmHouseEntry();
			Vector2 entry_position_world = Utility.PointToVector2(entry_position_tile) * 64f;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(entry_position_tile.X - 5, entry_position_tile.Y + 2) * 64f), Building.leftShadow, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			for (int x = 1; x < 8; x++)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(entry_position_tile.X - 5 + x, entry_position_tile.Y + 2) * 64f), Building.middleShadow, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(entry_position_tile.X + 3, entry_position_tile.Y + 2) * 64f), Building.rightShadow, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			Texture2D house_texture = houseTextures;
			if (paintedHouseTexture != null)
			{
				house_texture = paintedHouseTexture;
			}
			Color house_draw_color = Color.White;
			if (frameHouseColor.HasValue)
			{
				house_draw_color = frameHouseColor.Value;
				frameHouseColor = null;
			}
			Vector2 house_draw_position = new Vector2(entry_position_world.X - 384f, entry_position_world.Y - 440f);
			b.Draw(house_texture, Game1.GlobalToLocal(Game1.viewport, house_draw_position), houseSource, house_draw_color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (house_draw_position.Y + 230f) / 10000f);
			if (Game1.mailbox.Count > 0)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				Point mailbox_position = Game1.player.getMailboxPosition();
				float draw_layer = (float)((mailbox_position.X + 1) * 64) / 10000f + (float)(mailbox_position.Y * 64) / 10000f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailbox_position.X * 64, (float)(mailbox_position.Y * 64 - 96 - 48) + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-06f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailbox_position.X * 64 + 32 + 4, (float)(mailbox_position.Y * 64 - 64 - 24 - 8) + yOffset)), new Microsoft.Xna.Framework.Rectangle(189, 423, 15, 13), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
			}
			if (shippingBinLid != null)
			{
				shippingBinLid.draw(b);
			}
			if (!hasSeenGrandpaNote)
			{
				Point grandpa_shrine = GetGrandpaShrinePosition();
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((grandpa_shrine.X + 1) * 64, grandpa_shrine.Y * 64)), new Microsoft.Xna.Framework.Rectangle(575, 1972, 11, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0448009968f);
			}
		}

		public virtual Point GetMainMailboxPosition()
		{
			if (!mapMainMailboxPosition.HasValue)
			{
				mapMainMailboxPosition = GetMapPropertyPosition("MailboxLocation", 68, 16);
			}
			return mapMainMailboxPosition.Value;
		}

		public virtual Point GetGrandpaShrinePosition()
		{
			if (!mapGrandpaShrinePosition.HasValue)
			{
				mapGrandpaShrinePosition = GetMapPropertyPosition("GrandpaShrineLocation", 8, 7);
			}
			return mapGrandpaShrinePosition.Value;
		}

		public virtual Point GetMainFarmHouseEntry()
		{
			if (!mainFarmhouseEntry.HasValue)
			{
				mainFarmhouseEntry = GetMapPropertyPosition("FarmHouseEntry", 64, 15);
			}
			return mainFarmhouseEntry.Value;
		}

		public override void startEvent(Event evt)
		{
			if (evt.id != -2)
			{
				Point mainFarmHouseEntry = GetMainFarmHouseEntry();
				int offset_x = mainFarmHouseEntry.X - 64;
				int offset_y = mainFarmHouseEntry.Y - 15;
				evt.eventPositionTileOffset = new Vector2(offset_x, offset_y);
			}
			base.startEvent(evt);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
		}

		public virtual void ApplyHousePaint()
		{
			if (paintedHouseTexture != null)
			{
				paintedHouseTexture.Dispose();
				paintedHouseTexture = null;
			}
			paintedHouseTexture = BuildingPainter.Apply(houseTextures, "Buildings\\houses_PaintMask", housePaintColor);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			spawnCrowEvent.Poll();
			lightningStrikeEvent.Poll();
			housePaintColor.Value.Poll(ApplyHousePaint);
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			if (!Game1.currentLocation.Equals(this))
			{
				NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.PairsCollection pairs = animals.Pairs;
				for (int i = pairs.Count() - 1; i >= 0; i--)
				{
					pairs.ElementAt(i).Value.updateWhenNotCurrentLocation(null, time, this);
				}
			}
		}

		public bool isTileOpenBesidesTerrainFeatures(Vector2 tile)
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
			foreach (Building building in buildings)
			{
				if (building.intersects(boundingBox))
				{
					return false;
				}
			}
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.getBoundingBox(resourceClump.tile).Intersects(boundingBox))
				{
					return false;
				}
			}
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (pair.Value.getTileLocation().Equals(tile))
				{
					return true;
				}
			}
			if (!objects.ContainsKey(tile))
			{
				return isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
			}
			return false;
		}

		private void doLightningStrike(LightningStrikeEvent lightning)
		{
			if (lightning.smallFlash)
			{
				if (Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && !Game1.newDay && Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext()).isLightning.Value)
				{
					Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
					if (Game1.random.NextDouble() < 0.5)
					{
						DelayedAction.screenFlashAfterDelay((float)(0.3 + Game1.random.NextDouble()), Game1.random.Next(500, 1000));
					}
					DelayedAction.playSoundAfterDelay("thunder_small", Game1.random.Next(500, 1500));
				}
			}
			else if (lightning.bigFlash && Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext()).isLightning.Value && !Game1.newDay)
			{
				Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
				Game1.playSound("thunder");
			}
			if (lightning.createBolt && Game1.currentLocation.name.Equals("Farm"))
			{
				if (lightning.destroyedTerrainFeature)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite(362, 75f, 6, 1, lightning.boltPosition, flicker: false, flipped: false));
				}
				Utility.drawLightningBolt(lightning.boltPosition, this);
			}
		}

		public override bool CanBeRemotedlyViewed()
		{
			return true;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (wasUpdated && Game1.gameMode != 0)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			chimneyTimer -= time.ElapsedGameTime.Milliseconds;
			if (chimneyTimer <= 0)
			{
				FarmHouse farmHouse = Utility.getHomeOfFarmer(Game1.MasterPlayer);
				if (farmHouse != null && farmHouse.hasActiveFireplace())
				{
					Point p = farmHouse.getPorchStandingSpot();
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(p.X * 64 + 4 * (((int)Game1.MasterPlayer.houseUpgradeLevel >= 2) ? 9 : (-5)), p.Y * 64 - 420), flipped: false, 0.002f, Color.Gray)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -0.5f),
						acceleration = new Vector2(0.002f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 2f,
						scaleChange = 0.02f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
					});
				}
				for (int i = 0; i < buildings.Count; i++)
				{
					if (buildings[i].indoors.Value is Cabin && (buildings[i].indoors.Value as Cabin).hasActiveFireplace())
					{
						temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(((int)buildings[i].tileX + 4) * 64 + -20, ((int)buildings[i].tileY + 3) * 64 - 420), flipped: false, 0.002f, Color.Gray)
						{
							alpha = 0.75f,
							motion = new Vector2(0f, -0.5f),
							acceleration = new Vector2(0.002f, 0f),
							interval = 99999f,
							layerDepth = 1f,
							scale = 2f,
							scaleChange = 0.02f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
						});
					}
				}
				chimneyTimer = 500;
			}
			foreach (KeyValuePair<long, FarmAnimal> kvp2 in animals.Pairs)
			{
				_tempAnimals.Add(kvp2);
			}
			foreach (KeyValuePair<long, FarmAnimal> kvp in _tempAnimals)
			{
				if (kvp.Value.updateWhenCurrentLocation(time, this))
				{
					animals.Remove(kvp.Key);
				}
			}
			_tempAnimals.Clear();
			if (shippingBinLid != null)
			{
				bool opening = false;
				foreach (Farmer farmer in farmers)
				{
					if (farmer.GetBoundingBox().Intersects(shippingBinLidOpenArea))
					{
						openShippingBinLid();
						opening = true;
					}
				}
				if (!opening)
				{
					closeShippingBinLid();
				}
				updateShippingBinLid(time);
			}
		}

		public int getTotalCrops()
		{
			int amount = 0;
			foreach (TerrainFeature t in terrainFeatures.Values)
			{
				if (t is HoeDirt && (t as HoeDirt).crop != null && !(t as HoeDirt).crop.dead)
				{
					amount++;
				}
			}
			return amount;
		}

		public int getTotalCropsReadyForHarvest()
		{
			int amount = 0;
			foreach (TerrainFeature t in terrainFeatures.Values)
			{
				if (t is HoeDirt && (t as HoeDirt).readyForHarvest())
				{
					amount++;
				}
			}
			return amount;
		}

		public int getTotalUnwateredCrops()
		{
			int amount = 0;
			foreach (TerrainFeature t in terrainFeatures.Values)
			{
				if (t is HoeDirt && (t as HoeDirt).crop != null && (t as HoeDirt).needsWatering() && (int)(t as HoeDirt).state != 1)
				{
					amount++;
				}
			}
			return amount;
		}

		public int getTotalGreenhouseCropsReadyForHarvest()
		{
			if (Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
			{
				int amount = 0;
				{
					foreach (TerrainFeature t in Game1.getLocationFromName("Greenhouse").terrainFeatures.Values)
					{
						if (t is HoeDirt && (t as HoeDirt).readyForHarvest())
						{
							amount++;
						}
					}
					return amount;
				}
			}
			return -1;
		}

		private GreenhouseBuilding getGreenhouseBuilding()
		{
			foreach (Building b in buildings)
			{
				if (b is GreenhouseBuilding)
				{
					return b as GreenhouseBuilding;
				}
			}
			return null;
		}

		public int getTotalOpenHoeDirt()
		{
			int amount = 0;
			foreach (TerrainFeature t in terrainFeatures.Values)
			{
				if (t is HoeDirt && (t as HoeDirt).crop == null && !objects.ContainsKey(t.currentTileLocation))
				{
					amount++;
				}
			}
			return amount;
		}

		public int getTotalForageItems()
		{
			int amount = 0;
			foreach (Object value in objects.Values)
			{
				if ((bool)value.isSpawnedObject)
				{
					amount++;
				}
			}
			return amount;
		}

		public int getNumberOfMachinesReadyForHarvest()
		{
			int num = 0;
			foreach (Object value in objects.Values)
			{
				if (value.IsConsideredReadyMachineForComputer())
				{
					num++;
				}
			}
			foreach (Object value2 in Game1.getLocationFromName("FarmHouse").objects.Values)
			{
				if (value2.IsConsideredReadyMachineForComputer())
				{
					num++;
				}
			}
			foreach (Building b in buildings)
			{
				if (b.indoors.Value != null)
				{
					foreach (Object value3 in b.indoors.Value.objects.Values)
					{
						if (value3.IsConsideredReadyMachineForComputer())
						{
							num++;
						}
					}
				}
			}
			return num;
		}

		public bool doesFarmCaveNeedHarvesting()
		{
			return farmCaveReady.Value;
		}
	}
}
