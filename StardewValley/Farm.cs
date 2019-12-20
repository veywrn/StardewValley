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

		public const int default_layout = 0;

		public const int riverlands_layout = 1;

		public const int forest_layout = 2;

		public const int mountains_layout = 3;

		public const int combat_layout = 4;

		public const int fourCorners_layout = 5;

		public const int layout_max = 5;

		[XmlElement("animals")]
		public readonly NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = new NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>();

		public readonly NetCollection<ResourceClump> resourceClumps = new NetCollection<ResourceClump>();

		[XmlElement("piecesOfHay")]
		public readonly NetInt piecesOfHay = new NetInt(0);

		[XmlElement("grandpaScore")]
		public NetInt grandpaScore = new NetInt(0);

		private TemporaryAnimatedSprite shippingBinLid;

		private Microsoft.Xna.Framework.Rectangle shippingBinLidOpenArea = new Microsoft.Xna.Framework.Rectangle(4480, 832, 256, 192);

		[XmlIgnore]
		private readonly NetCollection<Item> sharedShippingBin = new NetCollection<Item>();

		[XmlIgnore]
		public Item lastItemShipped;

		public bool hasSeenGrandpaNote;

		[XmlElement("houseSource")]
		private readonly NetRectangle houseSource = new NetRectangle();

		[XmlElement("greenhouseSource")]
		private readonly NetRectangle greenhouseSource = new NetRectangle();

		private readonly NetEvent1Field<Vector2, NetVector2> spawnCrowEvent = new NetEvent1Field<Vector2, NetVector2>();

		public readonly NetEvent1<LightningStrikeEvent> lightningStrikeEvent = new NetEvent1<LightningStrikeEvent>();

		private readonly List<KeyValuePair<long, FarmAnimal>> _tempAnimals = new List<KeyValuePair<long, FarmAnimal>>();

		public readonly NetBool petBowlWatered = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetPoint petBowlPosition = new NetPoint();

		private int chimneyTimer = 500;

		public const int numCropsForCrow = 16;

		public NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals => animals;

		public Farm()
		{
		}

		public Farm(string mapPath, string name)
			: base(mapPath, name)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
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

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(animals, resourceClumps, piecesOfHay, sharedShippingBin, houseSource, greenhouseSource, spawnCrowEvent, petBowlWatered, petBowlPosition, lightningStrikeEvent, grandpaScore);
			spawnCrowEvent.onEvent += doSpawnCrow;
			lightningStrikeEvent.onEvent += doLightningStrike;
			petBowlWatered.fieldChangeVisibleEvent += delegate
			{
				_UpdateWaterBowl();
			};
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
			if (Game1.whichFarm == 4 && !Game1.player.mailReceived.Contains("henchmanGone"))
			{
				Game1.spawnMonstersAtNight = true;
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
					for (int n = 0; n < 6; n++)
					{
						Object o2 = objects.Pairs.ElementAt(Game1.random.Next(objects.Count())).Value;
						if (o2.name.Equals("Weeds") && o2.tileLocation.X < 36f && o2.tileLocation.Y < 34f)
						{
							o2.ParentSheetIndex = 792 + Utility.getSeasonNumber(Game1.currentSeason);
						}
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
				for (int i3 = terrainFeatures.Count() - 1; i3 >= 0; i3--)
				{
					if (terrainFeatures.Pairs.ElementAt(i3).Value is HoeDirt && (terrainFeatures.Pairs.ElementAt(i3).Value as HoeDirt).crop == null && Game1.random.NextDouble() < 0.8)
					{
						terrainFeatures.Remove(terrainFeatures.Pairs.ElementAt(i3).Key);
					}
				}
				spawnWeedsAndStones(20, weedsOnly: false, spawnFromOldWeeds: false);
				if (Game1.currentSeason.Equals("spring") && Game1.stats.DaysPlayed > 1)
				{
					spawnWeedsAndStones(40, weedsOnly: false, spawnFromOldWeeds: false);
					spawnWeedsAndStones(40, weedsOnly: true, spawnFromOldWeeds: false);
					for (int i2 = 0; i2 < 15; i2++)
					{
						int xCoord = Game1.random.Next(map.DisplayWidth / 64);
						int yCoord = Game1.random.Next(map.DisplayHeight / 64);
						Vector2 location = new Vector2(xCoord, yCoord);
						objects.TryGetValue(location, out Object o);
						if (o == null && doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null && isTileLocationOpen(new Location(xCoord * 64, yCoord * 64)) && !isTileOccupied(location) && doesTileHaveProperty(xCoord, yCoord, "Water", "Back") == null)
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
			return new Point(64, 14);
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
			else if (Game1.random.NextDouble() < 0.65 && isTileLocationTotallyClearAndPlaceable(spawnLocation))
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
			Point p = new Point(tileX * 64 + 32, tileY * 64 + 32);
			resourceClumps.Filter((ResourceClump clump) => !clump.getBoundingBox(clump.tile).Contains(p) || !clump.performToolAction(t, 1, clump.tile, this));
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
				foreach (ResourceClump stump in resourceClumps)
				{
					if (stump.getBoundingBox(stump.tile).Intersects(position) && (!isFarmer || character == null || !stump.getBoundingBox(stump.tile).Intersects(character.GetBoundingBox())))
					{
						return true;
					}
				}
				foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
				{
					if (character != null && !character.Equals(kvp.Value) && !(character is FarmAnimal) && position.Intersects(kvp.Value.GetBoundingBox()) && (!isFarmer || !Game1.player.GetBoundingBox().Intersects(kvp.Value.GetBoundingBox())))
					{
						if (isFarmer && character is Farmer && (character as Farmer).TemporaryPassableTiles.Intersects(position))
						{
							break;
						}
						kvp.Value.farmerPushing();
						return true;
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
			foreach (ResourceClump stump in resourceClumps)
			{
				if (stump.getBoundingBox(stump.tile).Intersects(tileRect))
				{
					stump.performUseAction(new Vector2(tileLocation.X, tileLocation.Y), this);
					return true;
				}
			}
			if (tileLocation.X >= 71 && tileLocation.X <= 72 && tileLocation.Y >= 13 && tileLocation.Y <= 14)
			{
				ItemGrabMenu itemGrabMenu = new ItemGrabMenu(null, reverseGrab: true, showReceivingMenu: false, Utility.highlightShippableObjects, shipItem, "", null, snapToBottom: true, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
				itemGrabMenu.initializeUpperRightCloseButton();
				itemGrabMenu.setBackgroundTransparency(b: false);
				itemGrabMenu.setDestroyItemOnClick(b: true);
				itemGrabMenu.initializeShippingBin();
				Game1.activeClickableMenu = itemGrabMenu;
				playSound("shwip");
				if (Game1.player.FacingDirection == 1)
				{
					Game1.player.Halt();
				}
				Game1.player.showCarrying();
				return true;
			}
			int tileIndexOfCheckLocation = (map.GetLayer("Buildings").Tiles[tileLocation] != null) ? map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : (-1);
			if ((uint)(tileIndexOfCheckLocation - 1956) <= 2u)
			{
				if (!hasSeenGrandpaNote)
				{
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

		public NetCollection<Item> getShippingBin(Farmer who)
		{
			if ((bool)Game1.player.team.useSeparateWallets)
			{
				return who.personalShippingBin;
			}
			return sharedShippingBin;
		}

		private void shipItem(Item i, Farmer who)
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
			if (who.ActiveObject != null && x / 64 >= 71 && x / 64 <= 72 && y / 64 >= 13 && y / 64 <= 14 && who.ActiveObject.canBeShipped() && Vector2.Distance(who.getTileLocation(), new Vector2(71.5f, 14f)) <= 2f)
			{
				getShippingBin(who).Add(who.ActiveObject);
				lastItemShipped = who.ActiveObject;
				who.showNotCarrying();
				showShipment(who.ActiveObject);
				who.ActiveObject = null;
				return true;
			}
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
			if (Game1.whichFarm == 5)
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
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.getBoundingBox(resourceClump.tile).Intersects(r))
				{
					return true;
				}
			}
			return base.isTileOccupied(tileLocation, characterToIgnore, ignoreAllCharacters);
		}

		public override void removeEverythingExceptCharactersFromThisTile(int x, int y)
		{
			base.removeEverythingFromThisTile(x, y);
			for (int i = resourceClumps.Count - 1; i >= 0; i--)
			{
				if (resourceClumps[i].tile.X == (float)x && resourceClumps[i].tile.Y == (float)y)
				{
					resourceClumps.RemoveAt(i);
				}
			}
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			houseSource.Value = new Microsoft.Xna.Framework.Rectangle(0, 144 * (((int)Game1.MasterPlayer.houseUpgradeLevel == 3) ? 2 : ((int)Game1.MasterPlayer.houseUpgradeLevel)), 160, 144);
			greenhouseSource.Value = new Microsoft.Xna.Framework.Rectangle(160, 160 * (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccPantry") ? 1 : 0), 112, 160);
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
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				if (characters[i] is Child)
				{
					(characters[i] as Child).resetForPlayerEntry(this);
				}
				if (characters[i].isVillager() && characters[i].name.Equals(Game1.player.spouse))
				{
					petBowlWatered.Set(newValue: true);
				}
			}
			if (Game1.timeOfDay >= 1830)
			{
				for (int j = animals.Count() - 1; j >= 0; j--)
				{
					animals.Pairs.ElementAt(j).Value.warpHome(this, animals.Pairs.ElementAt(j).Value);
				}
			}
			shippingBinLid = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(134, 226, 30, 25), new Vector2(71f, 13f) * 64f + new Vector2(2f, -7f) * 4f, flipped: false, 0f, Color.White)
			{
				holdLastFrame = true,
				destroyable = false,
				interval = 20f,
				animationLength = 13,
				paused = true,
				scale = 4f,
				layerDepth = 0.0961f,
				pingPong = true,
				pingPongMotion = 0
			};
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
		}

		public void addSpouseOutdoorArea(string spouseName)
		{
			removeTile(70, 9, "Buildings");
			removeTile(71, 9, "Buildings");
			removeTile(72, 9, "Buildings");
			removeTile(69, 9, "Buildings");
			removeTile(70, 8, "Buildings");
			removeTile(71, 8, "Buildings");
			removeTile(72, 8, "Buildings");
			removeTile(69, 8, "Buildings");
			removeTile(70, 7, "Front");
			removeTile(71, 7, "Front");
			removeTile(72, 7, "Front");
			removeTile(69, 7, "Front");
			removeTile(70, 6, "AlwaysFront");
			removeTile(71, 6, "AlwaysFront");
			removeTile(72, 6, "AlwaysFront");
			removeTile(69, 6, "AlwaysFront");
			switch (spouseName)
			{
			case "Emily":
				setMapTileIndex(69, 8, 1867, "Buildings", 1);
				setMapTileIndex(72, 8, 1867, "Buildings", 1);
				setMapTileIndex(69, 7, 1842, "Front", 1);
				setMapTileIndex(72, 7, 1842, "Front", 1);
				setMapTileIndex(69, 9, 1866, "Buildings", 1);
				setMapTileIndex(71, 8, 1866, "Buildings", 1);
				setMapTileIndex(72, 9, 1967, "Buildings", 1);
				setMapTileIndex(70, 8, 1967, "Buildings", 1);
				break;
			case "Shane":
				setMapTileIndex(70, 9, 1940, "Buildings", 1);
				setMapTileIndex(71, 9, 1941, "Buildings", 1);
				setMapTileIndex(72, 9, 1942, "Buildings", 1);
				setMapTileIndex(70, 8, 1915, "Buildings", 1);
				setMapTileIndex(71, 8, 1916, "Buildings", 1);
				setMapTileIndex(72, 8, 1917, "Buildings", 1);
				setMapTileIndex(70, 7, 1772, "Front", 1);
				setMapTileIndex(71, 7, 1773, "Front", 1);
				setMapTileIndex(72, 7, 1774, "Front", 1);
				setMapTileIndex(70, 6, 1747, "AlwaysFront", 1);
				setMapTileIndex(71, 6, 1748, "AlwaysFront", 1);
				setMapTileIndex(72, 6, 1749, "AlwaysFront", 1);
				break;
			case "Sebastian":
				setMapTileIndex(70, 8, 1927, "Buildings", 1);
				setMapTileIndex(71, 8, 1928, "Buildings", 1);
				setMapTileIndex(72, 8, 1929, "Buildings", 1);
				setMapTileIndex(70, 7, 1902, "Front", 1);
				setMapTileIndex(71, 7, 1903, "Front", 1);
				break;
			case "Sam":
				setMapTileIndex(69, 8, 1173, "Buildings", 1);
				setMapTileIndex(72, 8, 1174, "Buildings", 1);
				setMapTileIndex(70, 8, 1198, "Buildings", 1);
				setMapTileIndex(71, 8, 1199, "Buildings", 1);
				setMapTileIndex(69, 7, 1148, "Front", 1);
				setMapTileIndex(72, 7, 1149, "Front", 1);
				break;
			case "Elliott":
				setMapTileIndex(69, 8, 1098, "Buildings", 1);
				setMapTileIndex(70, 8, 1123, "Buildings", 1);
				setMapTileIndex(72, 8, 1098, "Buildings", 1);
				break;
			case "Harvey":
				setMapTileIndex(69, 8, 1098, "Buildings", 1);
				setMapTileIndex(70, 8, 1123, "Buildings", 1);
				setMapTileIndex(72, 8, 1098, "Buildings", 1);
				break;
			case "Alex":
				setMapTileIndex(69, 8, 1099, "Buildings", 1);
				break;
			case "Maru":
				setMapTileIndex(71, 8, 1124, "Buildings", 1);
				break;
			case "Penny":
				setMapTileIndex(69, 8, 1098, "Buildings", 1);
				setMapTileIndex(70, 8, 1123, "Buildings", 1);
				setMapTileIndex(72, 8, 1098, "Buildings", 1);
				break;
			case "Haley":
				setMapTileIndex(69, 8, 1074, "Buildings", 1);
				setMapTileIndex(69, 7, 1049, "Front", 1);
				setMapTileIndex(69, 6, 1024, "AlwaysFront", 1);
				setMapTileIndex(72, 8, 1074, "Buildings", 1);
				setMapTileIndex(72, 7, 1049, "Front", 1);
				setMapTileIndex(72, 6, 1024, "AlwaysFront", 1);
				break;
			case "Abigail":
				setMapTileIndex(69, 8, 1098, "Buildings", 1);
				setMapTileIndex(70, 8, 1123, "Buildings", 1);
				setMapTileIndex(72, 8, 1098, "Buildings", 1);
				break;
			case "Leah":
				setMapTileIndex(70, 8, 1122, "Buildings", 1);
				setMapTileIndex(70, 7, 1097, "Front", 1);
				break;
			}
		}

		public void addGrandpaCandles()
		{
			if ((int)grandpaScore > 0)
			{
				Microsoft.Xna.Framework.Rectangle candleSource = new Microsoft.Xna.Framework.Rectangle(577, 1985, 2, 5);
				removeTemporarySpritesWithIDLocal(6666f);
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2(468f, 404f), flicker: false, flipped: false, 0.0384f, 0f, Color.White, 4f, 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(460f, 380f), flipped: false, 0f, Color.White)
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
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2(488f, 344f), flicker: false, flipped: false, 0.0384f, 0f, Color.White, 4f, 0f, 0f, 0f));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(484f, 320f), flipped: false, 0f, Color.White)
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
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2(596f, 344f), flicker: false, flipped: false, 0.0384f, 0f, Color.White, 4f, 0f, 0f, 0f));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(592f, 320f), flipped: false, 0f, Color.White)
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
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2(616f, 404f), flicker: false, flipped: false, 0.0384f, 0f, Color.White, 4f, 0f, 0f, 0f));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(612f, 380f), flipped: false, 0f, Color.White)
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

		public override bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			if (p.X >= 71f && p.X <= 72f && p.Y == 13f)
			{
				return false;
			}
			if (doesTileHavePropertyNoNull((int)p.X, (int)p.Y, "Type", "Back").Length > 0)
			{
				return true;
			}
			return base.shouldShadowBeDrawnAboveBuildingsLayer(p);
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
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.occupiesTile((int)tileLocation.X, (int)tileLocation.Y))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (ResourceClump stump in resourceClumps)
			{
				stump.draw(b, stump.tile);
			}
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				pair.Value.draw(b);
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3776f, 1088f)), Building.leftShadow, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			for (int x = 1; x < 8; x++)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3776 + x * 64, 1088f)), Building.middleShadow, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(4288f, 1088f)), Building.rightShadow, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			b.Draw(houseTextures, Game1.GlobalToLocal(Game1.viewport, new Vector2(3712f, 520f)), houseSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.075f);
			b.Draw(houseTextures, Game1.GlobalToLocal(Game1.viewport, (Game1.whichFarm == 5) ? new Vector2(2304f, 1600f) : new Vector2(1600f, 384f)), greenhouseSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0704f);
			if (Game1.mailbox.Count > 0)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2);
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
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(576f, 448f)), new Microsoft.Xna.Framework.Rectangle(575, 1972, 11, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0448009968f);
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			spawnCrowEvent.Poll();
			lightningStrikeEvent.Poll();
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

		public void addResourceClumpAndRemoveUnderlyingTerrain(int resourceClumpIndex, int width, int height, Vector2 tile)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					removeEverythingExceptCharactersFromThisTile((int)tile.X + x, (int)tile.Y + y);
				}
			}
			resourceClumps.Add(new ResourceClump(resourceClumpIndex, width, height, tile));
		}

		private void doLightningStrike(LightningStrikeEvent lightning)
		{
			if (lightning.smallFlash)
			{
				if (Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && !Game1.newDay)
				{
					Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
					if (Game1.random.NextDouble() < 0.5)
					{
						DelayedAction.screenFlashAfterDelay((float)(0.3 + Game1.random.NextDouble()), Game1.random.Next(500, 1000));
					}
					DelayedAction.playSoundAfterDelay("thunder_small", Game1.random.Next(500, 1500));
				}
			}
			else if (lightning.bigFlash && Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && !Game1.newDay)
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
			foreach (ResourceClump stump in resourceClumps)
			{
				stump.tickUpdate(time, stump.tile, this);
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
	}
}
