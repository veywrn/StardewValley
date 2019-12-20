using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class MineShaft : GameLocation
	{
		public const int mineFrostLevel = 40;

		public const int mineLavaLevel = 80;

		public const int upperArea = 0;

		public const int jungleArea = 10;

		public const int frostArea = 40;

		public const int lavaArea = 80;

		public const int desertArea = 121;

		public const int bottomOfMineLevel = 120;

		public const int quarryMineShaft = 77377;

		public const int numberOfLevelsPerArea = 40;

		public const int mineFeature_barrels = 0;

		public const int mineFeature_chests = 1;

		public const int mineFeature_coalCart = 2;

		public const int mineFeature_elevator = 3;

		public const double chanceForColoredGemstone = 0.008;

		public const double chanceForDiamond = 0.0005;

		public const double chanceForPrismaticShard = 0.0005;

		public const int monsterLimit = 30;

		public static SerializableDictionary<int, MineInfo> permanentMineChanges = new SerializableDictionary<int, MineInfo>();

		public static int numberOfCraftedStairsUsedThisRun;

		private Random mineRandom = new Random();

		private LocalizedContentManager mineLoader = Game1.content.CreateTemporary();

		private int timeUntilElevatorLightUp;

		[XmlIgnore]
		public int loadedMapNumber;

		private int fogTime;

		private NetBool isFogUp = new NetBool();

		public static int timeSinceLastMusic = 200000;

		private bool ladderHasSpawned;

		private bool ghostAdded;

		private bool loadedDarkArea;

		private bool isFallingDownShaft;

		private Vector2 fogPos;

		private readonly NetBool elevatorShouldDing = new NetBool();

		public readonly NetString mapImageSource = new NetString();

		public readonly NetObjectList<ResourceClump> resourceClumps = new NetObjectList<ResourceClump>();

		private readonly NetInt netMineLevel = new NetInt();

		private readonly NetIntDelta netStonesLeftOnThisLevel = new NetIntDelta();

		private readonly NetVector2 netTileBeneathLadder = new NetVector2();

		private readonly NetVector2 netTileBeneathElevator = new NetVector2();

		private readonly NetPoint netElevatorLightSpot = new NetPoint();

		private readonly NetBool netIsSlimeArea = new NetBool();

		private readonly NetBool netIsMonsterArea = new NetBool();

		private readonly NetBool netIsTreasureRoom = new NetBool();

		private readonly NetBool netIsDinoArea = new NetBool();

		private readonly NetBool netIsQuarryArea = new NetBool();

		private readonly NetBool netAmbientFog = new NetBool();

		private readonly NetColor netLighting = new NetColor(Color.White);

		private readonly NetColor netFogColor = new NetColor();

		private readonly NetVector2Dictionary<bool, NetBool> createLadderAtEvent = new NetVector2Dictionary<bool, NetBool>();

		private readonly NetPointDictionary<bool, NetBool> createLadderDownEvent = new NetPointDictionary<bool, NetBool>();

		private float fogAlpha;

		[XmlIgnore]
		public static ICue bugLevelLoop;

		private readonly NetBool rainbowLights = new NetBool(value: false);

		private readonly NetBool isLightingDark = new NetBool(value: false);

		private LocalizedContentManager mapContent;

		public static List<MineShaft> activeMines = new List<MineShaft>();

		public static HashSet<int> mushroomLevelsGeneratedToday = new HashSet<int>();

		private int lastLevelsDownFallen;

		private Microsoft.Xna.Framework.Rectangle fogSource = new Microsoft.Xna.Framework.Rectangle(640, 0, 64, 64);

		private List<Vector2> brownSpots = new List<Vector2>();

		private int lifespan;

		public static int lowestLevelReached
		{
			get
			{
				return Game1.netWorldState.Value.LowestMineLevel;
			}
			set
			{
				Game1.netWorldState.Value.LowestMineLevel = value;
			}
		}

		public int mineLevel
		{
			get
			{
				return netMineLevel;
			}
			set
			{
				netMineLevel.Value = value;
			}
		}

		private int stonesLeftOnThisLevel
		{
			get
			{
				return netStonesLeftOnThisLevel;
			}
			set
			{
				netStonesLeftOnThisLevel.Value = value;
			}
		}

		private Vector2 tileBeneathLadder
		{
			get
			{
				return netTileBeneathLadder;
			}
			set
			{
				netTileBeneathLadder.Value = value;
			}
		}

		private Vector2 tileBeneathElevator
		{
			get
			{
				return netTileBeneathElevator;
			}
			set
			{
				netTileBeneathElevator.Value = value;
			}
		}

		private Point ElevatorLightSpot
		{
			get
			{
				return netElevatorLightSpot;
			}
			set
			{
				netElevatorLightSpot.Value = value;
			}
		}

		private bool isSlimeArea
		{
			get
			{
				return netIsSlimeArea;
			}
			set
			{
				netIsSlimeArea.Value = value;
			}
		}

		private bool isDinoArea
		{
			get
			{
				return netIsDinoArea;
			}
			set
			{
				netIsDinoArea.Value = value;
			}
		}

		private bool isMonsterArea
		{
			get
			{
				return netIsMonsterArea;
			}
			set
			{
				netIsMonsterArea.Value = value;
			}
		}

		private bool isQuarryArea
		{
			get
			{
				return netIsQuarryArea;
			}
			set
			{
				netIsQuarryArea.Value = value;
			}
		}

		private bool ambientFog
		{
			get
			{
				return netAmbientFog;
			}
			set
			{
				netAmbientFog.Value = value;
			}
		}

		private Color lighting
		{
			get
			{
				return netLighting;
			}
			set
			{
				netLighting.Value = value;
			}
		}

		private Color fogColor
		{
			get
			{
				return netFogColor;
			}
			set
			{
				netFogColor.Value = value;
			}
		}

		public int EnemyCount => characters.OfType<Monster>().Count();

		public MineShaft()
		{
			name.Value = "UndergroundMine" + mineLevel;
			mapContent = Game1.game1.xTileContent.CreateTemporary();
		}

		public MineShaft(int level)
			: this()
		{
			mineLevel = level;
			name.Value = "UndergroundMine" + level;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(netMineLevel, netStonesLeftOnThisLevel, netTileBeneathLadder, netTileBeneathElevator, netElevatorLightSpot, netIsSlimeArea, netIsMonsterArea, netIsTreasureRoom, netIsDinoArea, netIsQuarryArea, netAmbientFog, netLighting, netFogColor, resourceClumps, createLadderAtEvent, createLadderDownEvent, mapImageSource, rainbowLights, isLightingDark, elevatorShouldDing, isFogUp);
			isFogUp.fieldChangeEvent += delegate(NetBool field, bool oldValue, bool newValue)
			{
				if (!oldValue && newValue)
				{
					if (Game1.currentLocation == this)
					{
						Game1.changeMusicTrack("none");
					}
					if (Game1.IsClient)
					{
						fogTime = 35000;
					}
				}
				else if (!newValue)
				{
					fogTime = 0;
				}
			};
			createLadderAtEvent.OnValueAdded += delegate(Vector2 v, bool b)
			{
				doCreateLadderAt(v);
			};
			createLadderDownEvent.OnValueAdded += delegate(Point p, bool b)
			{
				doCreateLadderDown(p, b);
			};
			mapImageSource.fieldChangeEvent += delegate(NetString field, string oldValue, string newValue)
			{
				if (newValue != null && newValue != oldValue)
				{
					base.Map.TileSheets[0].ImageSource = newValue;
					base.Map.LoadTileSheets(Game1.mapDisplayDevice);
				}
			};
		}

		protected override LocalizedContentManager getMapLoader()
		{
			return mapContent;
		}

		private void setElevatorLit()
		{
			setMapTileIndex(ElevatorLightSpot.X, ElevatorLightSpot.Y, 48, "Buildings");
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(ElevatorLightSpot.X, ElevatorLightSpot.Y) * 64f, 2f, Color.Black, ElevatorLightSpot.X + ElevatorLightSpot.Y * 1000, LightSource.LightContext.None, 0L));
			elevatorShouldDing.Value = false;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			bool isCurrentLocation = Game1.currentLocation == this;
			foreach (ResourceClump r in resourceClumps)
			{
				r.tickUpdate(time, r.tile, this);
			}
			if ((Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("Ambient")) && Game1.random.NextDouble() < 0.00195)
			{
				localSound("cavedrip");
			}
			if (timeUntilElevatorLightUp > 0)
			{
				timeUntilElevatorLightUp -= time.ElapsedGameTime.Milliseconds;
				if (timeUntilElevatorLightUp <= 0)
				{
					localSound("crystal");
					setElevatorLit();
				}
			}
			if (isCurrentLocation)
			{
				if ((bool)isFogUp && Game1.shouldTimePass())
				{
					if (Game1.soundBank != null && (bugLevelLoop == null || bugLevelLoop.IsStopped))
					{
						bugLevelLoop = Game1.soundBank.GetCue("bugLevelLoop");
						bugLevelLoop.Play();
					}
					if (fogAlpha < 1f)
					{
						if (Game1.shouldTimePass())
						{
							fogAlpha += 0.01f;
						}
						if (bugLevelLoop != null && Game1.soundBank != null)
						{
							bugLevelLoop.SetVariable("Volume", fogAlpha * 100f);
							bugLevelLoop.SetVariable("Frequency", fogAlpha * 25f);
						}
					}
					else if (bugLevelLoop != null && Game1.soundBank != null)
					{
						float f = (float)Math.Max(0.0, Math.Min(100.0, Math.Sin((double)((float)fogTime / 10000f) % (Math.PI * 200.0))));
						bugLevelLoop.SetVariable("Frequency", Math.Max(0f, Math.Min(100f, fogAlpha * 25f + f * 10f)));
					}
					fogPos = Game1.updateFloatingObjectPositionForMovement(current: new Vector2(Game1.viewport.X, Game1.viewport.Y), w: fogPos, previous: Game1.previousViewportPosition, speed: -1f);
					fogPos.X = (fogPos.X + 0.5f) % 256f;
					fogPos.Y = (fogPos.Y + 0.5f) % 256f;
				}
				else if (fogAlpha > 0f)
				{
					if (Game1.shouldTimePass())
					{
						fogAlpha -= 0.01f;
					}
					if (bugLevelLoop != null)
					{
						bugLevelLoop.SetVariable("Volume", fogAlpha * 100f);
						bugLevelLoop.SetVariable("Frequency", Math.Max(0f, bugLevelLoop.GetVariable("Frequency") - 0.01f));
						if (fogAlpha <= 0f)
						{
							bugLevelLoop.Stop(AudioStopOptions.Immediate);
							bugLevelLoop = null;
						}
					}
				}
				else if (ambientFog)
				{
					fogPos = Game1.updateFloatingObjectPositionForMovement(current: new Vector2(Game1.viewport.X, Game1.viewport.Y), w: fogPos, previous: Game1.previousViewportPosition, speed: -1f);
					fogPos.X = (fogPos.X + 0.5f) % 256f;
					fogPos.Y = (fogPos.Y + 0.5f) % 256f;
				}
			}
			base.UpdateWhenCurrentLocation(time);
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (bugLevelLoop != null)
			{
				bugLevelLoop.Stop(AudioStopOptions.Immediate);
				bugLevelLoop = null;
			}
			if (!Game1.IsMultiplayer && mineLevel == 20)
			{
				Game1.changeMusicTrack("none");
			}
		}

		public override int getExtraMillisecondsPerInGameMinuteForThisLocation()
		{
			if (Game1.IsMultiplayer)
			{
				return base.getExtraMillisecondsPerInGameMinuteForThisLocation();
			}
			if (getMineArea() != 121)
			{
				return 0;
			}
			return 2000;
		}

		public Vector2 mineEntrancePosition(Farmer who)
		{
			if (!who.ridingMineElevator || tileBeneathElevator.Equals(Vector2.Zero))
			{
				return tileBeneathLadder;
			}
			return tileBeneathElevator;
		}

		private void generateContents()
		{
			ladderHasSpawned = false;
			loadLevel(mineLevel);
			chooseLevelType();
			findLadder();
			populateLevel();
		}

		public void chooseLevelType()
		{
			fogTime = 0;
			if (bugLevelLoop != null)
			{
				bugLevelLoop.Stop(AudioStopOptions.Immediate);
				bugLevelLoop = null;
			}
			ambientFog = false;
			rainbowLights.Value = false;
			isLightingDark.Value = false;
			Random r = new Random((int)Game1.stats.DaysPlayed * mineLevel + 4 * mineLevel + (int)Game1.uniqueIDForThisGame / 2);
			lighting = new Color(80, 80, 40);
			if (getMineArea() == 80)
			{
				lighting = new Color(100, 100, 50);
			}
			if (r.NextDouble() < 0.3 && mineLevel > 2)
			{
				isLightingDark.Value = true;
				lighting = new Color(120, 120, 60);
				if (r.NextDouble() < 0.3)
				{
					lighting = new Color(150, 150, 120);
				}
			}
			if (r.NextDouble() < 0.15 && mineLevel > 5 && mineLevel != 120)
			{
				isLightingDark.Value = true;
				switch (getMineArea())
				{
				case 0:
				case 10:
					lighting = new Color(110, 110, 70);
					break;
				case 40:
					lighting = Color.Black;
					break;
				case 80:
					lighting = new Color(90, 130, 70);
					break;
				}
			}
			if (r.NextDouble() < 0.035 && getMineArea() == 80 && mineLevel % 5 != 0 && !mushroomLevelsGeneratedToday.Contains(mineLevel))
			{
				rainbowLights.Value = true;
				mushroomLevelsGeneratedToday.Add(mineLevel);
			}
			if (isDarkArea() && mineLevel < 120)
			{
				isLightingDark.Value = true;
				lighting = ((getMineArea() == 80) ? new Color(70, 100, 100) : new Color(150, 150, 120));
				if (getMineArea() == 0)
				{
					ambientFog = true;
					fogColor = Color.Black;
				}
			}
			if (mineLevel == 100)
			{
				lighting = new Color(140, 140, 80);
			}
			if (getMineArea() == 121)
			{
				lighting = new Color(110, 110, 40);
				if (r.NextDouble() < 0.05)
				{
					lighting = ((r.NextDouble() < 0.5) ? new Color(30, 30, 0) : new Color(150, 150, 50));
				}
			}
			if (getMineArea() == 77377)
			{
				isLightingDark.Value = false;
				rainbowLights.Value = false;
				ambientFog = true;
				fogColor = Color.White * 0.4f;
				lighting = new Color(80, 80, 30);
			}
		}

		private bool canAdd(int typeOfFeature, int numberSoFar)
		{
			if (permanentMineChanges.ContainsKey(mineLevel))
			{
				switch (typeOfFeature)
				{
				case 0:
					return permanentMineChanges[mineLevel].platformContainersLeft > numberSoFar;
				case 1:
					return permanentMineChanges[mineLevel].chestsLeft > numberSoFar;
				case 2:
					return permanentMineChanges[mineLevel].coalCartsLeft > numberSoFar;
				case 3:
					return permanentMineChanges[mineLevel].elevator == 0;
				}
			}
			return true;
		}

		public void updateMineLevelData(int feature, int amount = 1)
		{
			if (!permanentMineChanges.ContainsKey(mineLevel))
			{
				permanentMineChanges.Add(mineLevel, new MineInfo());
			}
			switch (feature)
			{
			case 0:
				permanentMineChanges[mineLevel].platformContainersLeft += amount;
				break;
			case 1:
				permanentMineChanges[mineLevel].chestsLeft += amount;
				break;
			case 2:
				permanentMineChanges[mineLevel].coalCartsLeft += amount;
				break;
			case 3:
				permanentMineChanges[mineLevel].elevator += amount;
				break;
			}
		}

		public void chestConsumed()
		{
			Game1.player.chestConsumedMineLevels[mineLevel] = true;
		}

		public bool isLevelSlimeArea()
		{
			return isSlimeArea;
		}

		public void checkForMapAlterations(int x, int y)
		{
			Tile buildingsTile = map.GetLayer("Buildings").Tiles[x, y];
			if (buildingsTile != null)
			{
				int tileIndex = buildingsTile.TileIndex;
				if (tileIndex == 194 && !canAdd(2, 0))
				{
					setMapTileIndex(x, y, 195, "Buildings");
					setMapTileIndex(x, y - 1, 179, "Front");
				}
			}
		}

		public void findLadder()
		{
			int found = 0;
			int currentTileIndex = -1;
			tileBeneathElevator = Vector2.Zero;
			bool lookForWater = mineLevel % 20 == 0;
			lightGlows.Clear();
			for (int y = 0; y < map.GetLayer("Buildings").LayerHeight; y++)
			{
				for (int x = 0; x < map.GetLayer("Buildings").LayerWidth; x++)
				{
					if (map.GetLayer("Buildings").Tiles[x, y] != null)
					{
						currentTileIndex = map.GetLayer("Buildings").Tiles[x, y].TileIndex;
						switch (currentTileIndex)
						{
						case 115:
							tileBeneathLadder = new Vector2(x, y + 1);
							sharedLights[x + y * 999] = new LightSource(4, new Vector2(x, y - 2) * 64f + new Vector2(32f, 0f), 0.25f, new Color(0, 20, 50), x + y * 999, LightSource.LightContext.None, 0L);
							sharedLights[x + y * 998] = new LightSource(4, new Vector2(x, y - 1) * 64f + new Vector2(32f, 0f), 0.5f, new Color(0, 20, 50), x + y * 998, LightSource.LightContext.None, 0L);
							sharedLights[x + y * 997] = new LightSource(4, new Vector2(x, y) * 64f + new Vector2(32f, 0f), 0.75f, new Color(0, 20, 50), x + y * 997, LightSource.LightContext.None, 0L);
							sharedLights[x + y * 1000] = new LightSource(4, new Vector2(x, y + 1) * 64f + new Vector2(32f, 0f), 1f, new Color(0, 20, 50), x + y * 1000, LightSource.LightContext.None, 0L);
							found++;
							break;
						case 112:
							tileBeneathElevator = new Vector2(x, y + 1);
							found++;
							break;
						}
						if (lighting.Equals(Color.White) && found == 2 && !lookForWater)
						{
							return;
						}
						if (!lighting.Equals(Color.White) && (currentTileIndex == 97 || currentTileIndex == 113 || currentTileIndex == 65 || currentTileIndex == 66 || currentTileIndex == 81 || currentTileIndex == 82 || currentTileIndex == 48))
						{
							sharedLights[x + y * 1000] = new LightSource(4, new Vector2(x, y) * 64f, 2.5f, new Color(0, 50, 100), x + y * 1000, LightSource.LightContext.None, 0L);
							switch (currentTileIndex)
							{
							case 66:
								lightGlows.Add(new Vector2(x, y) * 64f + new Vector2(0f, 64f));
								break;
							case 97:
							case 113:
								lightGlows.Add(new Vector2(x, y) * 64f + new Vector2(32f, 32f));
								break;
							}
						}
					}
					if (Game1.IsMasterGame && doesTileHaveProperty(x, y, "Water", "Back") != null && getMineArea() == 80 && Game1.random.NextDouble() < 0.1)
					{
						sharedLights[x + y * 1000] = new LightSource(4, new Vector2(x, y) * 64f, 2f, new Color(0, 220, 220), x + y * 1000, LightSource.LightContext.None, 0L);
					}
				}
			}
			if (isFallingDownShaft)
			{
				Vector2 p = default(Vector2);
				while (!isTileClearForMineObjects(p))
				{
					p.X = Game1.random.Next(1, map.Layers[0].LayerWidth);
					p.Y = Game1.random.Next(1, map.Layers[0].LayerHeight);
				}
				tileBeneathLadder = p;
				Game1.player.showFrame(5);
			}
			isFallingDownShaft = false;
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.occupiesTile((int)tileLocation.X, (int)tileLocation.Y))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (mustKillAllMonstersToAdvance() && EnemyCount == 0)
			{
				Vector2 p = new Vector2((int)tileBeneathLadder.X, (int)tileBeneathLadder.Y);
				if (getTileIndexAt(Utility.Vector2ToPoint(p), "Buildings") == -1)
				{
					createLadderAt(p, "newArtifact");
					if (mustKillAllMonstersToAdvance() && Game1.player.currentLocation == this)
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineShaft.cs.9484"));
					}
				}
			}
			if ((bool)isFogUp || map == null || mineLevel % 5 == 0 || !(Game1.random.NextDouble() < 0.1) || Game1.player.hasBuff(23))
			{
				return;
			}
			if (mineLevel > 10 && !mustKillAllMonstersToAdvance() && Game1.random.NextDouble() < 0.11 && getMineArea() != 77377)
			{
				isFogUp.Value = true;
				fogTime = 35000 + Game1.random.Next(-5, 6) * 1000;
				switch (getMineArea())
				{
				case 121:
					fogColor = Color.BlueViolet * 1f;
					break;
				case 0:
				case 10:
					fogColor = (isDarkArea() ? Color.Khaki : (Color.Green * 0.75f));
					break;
				case 40:
					fogColor = Color.Blue * 0.75f;
					break;
				case 80:
					fogColor = Color.Red * 0.5f;
					break;
				}
			}
			else
			{
				spawnFlyingMonsterOffScreen();
			}
		}

		public void spawnFlyingMonsterOffScreen()
		{
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
				spawnLocation.X -= Game1.viewport.Width / 64;
			}
			switch (getMineArea())
			{
			case 0:
				if (mineLevel > 10 && isDarkArea())
				{
					characters.Add(new Bat(spawnLocation * 64f, mineLevel)
					{
						focusedOnFarmers = true
					});
					playSound("batScreech");
				}
				break;
			case 10:
				characters.Add(new Fly(spawnLocation * 64f)
				{
					focusedOnFarmers = true
				});
				break;
			case 40:
				characters.Add(new Bat(spawnLocation * 64f, mineLevel)
				{
					focusedOnFarmers = true
				});
				playSound("batScreech");
				break;
			case 80:
				characters.Add(new Bat(spawnLocation * 64f, mineLevel)
				{
					focusedOnFarmers = true
				});
				playSound("batScreech");
				break;
			case 121:
				if (mineLevel < 171 || Game1.random.NextDouble() < 0.5)
				{
					characters.Add(new Serpent(spawnLocation * 64f)
					{
						focusedOnFarmers = true
					});
					playSound("serpentDie");
				}
				else
				{
					characters.Add(new Bat(spawnLocation * 64f, mineLevel)
					{
						focusedOnFarmers = true
					});
					playSound("batScreech");
				}
				break;
			case 77377:
				characters.Add(new Bat(spawnLocation * 64f, 77377)
				{
					focusedOnFarmers = true
				});
				playSound("rockGolemHit");
				break;
			}
		}

		public override void drawLightGlows(SpriteBatch b)
		{
			Color c;
			switch (getMineArea())
			{
			case 0:
				c = (isDarkArea() ? (Color.PaleGoldenrod * 0.5f) : (Color.PaleGoldenrod * 0.33f));
				break;
			case 80:
				c = (isDarkArea() ? (Color.Pink * 0.4f) : (Color.Red * 0.33f));
				break;
			case 40:
				c = Color.White * 0.65f;
				break;
			case 121:
				c = Color.White * 0.8f;
				if (isDinoArea)
				{
					c = Color.Orange * 0.5f;
				}
				break;
			default:
				c = Color.PaleGoldenrod * 0.33f;
				break;
			}
			foreach (Vector2 v in lightGlows)
			{
				if ((bool)rainbowLights)
				{
					switch ((int)(v.X / 64f + v.Y / 64f) % 4)
					{
					case 0:
						c = Color.Red * 0.5f;
						break;
					case 1:
						c = Color.Yellow * 0.5f;
						break;
					case 2:
						c = Color.Cyan * 0.33f;
						break;
					case 3:
						c = Color.Lime * 0.45f;
						break;
					}
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, v), new Microsoft.Xna.Framework.Rectangle(88, 1779, 30, 30), c, 0f, new Vector2(15f, 15f), 8f + (float)(96.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(v.X * 777f) + (double)(v.Y * 9746f)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, 1f);
			}
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			int fish = -1;
			double chanceMultiplier6 = 1.0;
			chanceMultiplier6 += 0.4 * (double)who.FishingLevel;
			chanceMultiplier6 += (double)waterDepth * 0.1;
			switch (getMineArea())
			{
			case 0:
			case 10:
				chanceMultiplier6 += (double)((bait == 689) ? 3 : 0);
				if (Game1.random.NextDouble() < 0.02 + 0.01 * chanceMultiplier6)
				{
					fish = 158;
				}
				break;
			case 40:
				chanceMultiplier6 += (double)((bait == 682) ? 3 : 0);
				if (Game1.random.NextDouble() < 0.015 + 0.009 * chanceMultiplier6)
				{
					fish = 161;
				}
				break;
			case 80:
				chanceMultiplier6 += (double)((bait == 684) ? 3 : 0);
				if (Game1.random.NextDouble() < 0.01 + 0.008 * chanceMultiplier6)
				{
					fish = 162;
				}
				break;
			}
			int quality = 0;
			if (Game1.random.NextDouble() < (double)((float)who.FishingLevel / 10f))
			{
				quality = 1;
			}
			if (Game1.random.NextDouble() < (double)((float)who.FishingLevel / 50f + (float)who.LuckLevel / 100f))
			{
				quality = 2;
			}
			if (fish != -1)
			{
				return new Object(fish, 1, isRecipe: false, -1, quality);
			}
			if (getMineArea() == 80)
			{
				return new Object(Game1.random.Next(167, 173), 1);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "UndergroundMine");
		}

		private void adjustLevelChances(ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
		{
			if (mineLevel == 1)
			{
				monsterChance = 0.0;
				itemChance = 0.0;
				gemStoneChance = 0.0;
			}
			else if (mineLevel % 5 == 0 && getMineArea() != 121)
			{
				itemChance = 0.0;
				gemStoneChance = 0.0;
				if (mineLevel % 10 == 0)
				{
					monsterChance = 0.0;
				}
			}
			if (mustKillAllMonstersToAdvance())
			{
				monsterChance = 0.025;
				itemChance = 0.001;
				stoneChance = 0.0;
				gemStoneChance = 0.0;
				if (isDinoArea)
				{
					itemChance *= 4.0;
				}
			}
			if (Game1.player.hasBuff(23) && getMineArea() != 121)
			{
				monsterChance = 0.0;
			}
			gemStoneChance /= 2.0;
			if (isQuarryArea || getMineArea() == 77377)
			{
				gemStoneChance = 0.001;
				itemChance = 0.0001;
				stoneChance *= 2.0;
				monsterChance = 0.02;
			}
		}

		private void populateLevel()
		{
			objects.Clear();
			terrainFeatures.Clear();
			resourceClumps.Clear();
			debris.Clear();
			characters.Clear();
			ghostAdded = false;
			stonesLeftOnThisLevel = 0;
			double stoneChance = (double)mineRandom.Next(10, 30) / 100.0;
			double monsterChance = 0.002 + (double)mineRandom.Next(200) / 10000.0;
			double itemChance = 0.0025;
			double gemStoneChance = 0.003;
			adjustLevelChances(ref stoneChance, ref monsterChance, ref itemChance, ref gemStoneChance);
			int barrelsAdded = 0;
			bool firstTime = !permanentMineChanges.ContainsKey(mineLevel);
			if (mineLevel > 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.5 && !mustKillAllMonstersToAdvance())
			{
				int numBarrels = mineRandom.Next(5) + (int)(Game1.player.team.AverageDailyLuck(Game1.currentLocation) * 20.0);
				for (int l = 0; l < numBarrels; l++)
				{
					Point p;
					Point motion;
					if (mineRandom.NextDouble() < 0.33)
					{
						p = new Point(mineRandom.Next(map.GetLayer("Back").LayerWidth), 0);
						motion = new Point(0, 1);
					}
					else if (!(mineRandom.NextDouble() < 0.5))
					{
						p = new Point(map.GetLayer("Back").LayerWidth - 1, mineRandom.Next(map.GetLayer("Back").LayerHeight));
						motion = new Point(-1, 0);
					}
					else
					{
						p = new Point(0, mineRandom.Next(map.GetLayer("Back").LayerHeight));
						motion = new Point(1, 0);
					}
					while (isTileOnMap(p.X, p.Y))
					{
						p.X += motion.X;
						p.Y += motion.Y;
						if (isTileClearForMineObjects(p.X, p.Y))
						{
							Vector2 objectPos5 = new Vector2(p.X, p.Y);
							objects.Add(objectPos5, new BreakableContainer(objectPos5, 118, this));
							break;
						}
					}
				}
			}
			if (mineLevel % 10 != 0 || (getMineArea() == 121 && mineLevel != 220 && !netIsTreasureRoom.Value))
			{
				for (int k = 0; k < map.GetLayer("Back").LayerWidth; k++)
				{
					for (int i = 0; i < map.GetLayer("Back").LayerHeight; i++)
					{
						checkForMapAlterations(k, i);
						if (isTileClearForMineObjects(k, i))
						{
							if (mineRandom.NextDouble() <= stoneChance)
							{
								Vector2 objectPos4 = new Vector2(k, i);
								if (base.Objects.ContainsKey(objectPos4))
								{
									continue;
								}
								if (getMineArea() == 40 && mineRandom.NextDouble() < 0.15)
								{
									base.Objects.Add(objectPos4, new Object(objectPos4, mineRandom.Next(319, 322), "Weeds", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
									{
										Fragility = 2,
										CanBeGrabbed = true
									});
								}
								else if ((bool)rainbowLights && mineRandom.NextDouble() < 0.55)
								{
									if (mineRandom.NextDouble() < 0.25)
									{
										int which = 404;
										switch (mineRandom.Next(5))
										{
										case 0:
											which = 422;
											break;
										case 1:
											which = 420;
											break;
										case 2:
											which = 420;
											break;
										case 3:
											which = 420;
											break;
										case 4:
											which = 420;
											break;
										}
										base.Objects.Add(objectPos4, new Object(which, 1)
										{
											IsSpawnedObject = true
										});
									}
								}
								else
								{
									Object stone = chooseStoneType(0.001, 5E-05, gemStoneChance, objectPos4);
									if (stone != null)
									{
										base.Objects.Add(objectPos4, stone);
										stonesLeftOnThisLevel++;
									}
								}
							}
							else if (mineRandom.NextDouble() <= monsterChance && getDistanceFromStart(k, i) > 5f)
							{
								Monster monsterToAdd2 = getMonsterForThisLevel(mineLevel, k, i);
								if (monsterToAdd2 is Grub)
								{
									if (mineRandom.NextDouble() < 0.4)
									{
										tryToAddMonster(new Grub(Vector2.Zero), k - 1, i);
									}
									if (mineRandom.NextDouble() < 0.4)
									{
										tryToAddMonster(new Grub(Vector2.Zero), k + 1, i);
									}
									if (mineRandom.NextDouble() < 0.4)
									{
										tryToAddMonster(new Grub(Vector2.Zero), k, i - 1);
									}
									if (mineRandom.NextDouble() < 0.4)
									{
										tryToAddMonster(new Grub(Vector2.Zero), k, i + 1);
									}
								}
								else if (monsterToAdd2 is DustSpirit)
								{
									if (mineRandom.NextDouble() < 0.6)
									{
										tryToAddMonster(new DustSpirit(Vector2.Zero), k - 1, i);
									}
									if (mineRandom.NextDouble() < 0.6)
									{
										tryToAddMonster(new DustSpirit(Vector2.Zero), k + 1, i);
									}
									if (mineRandom.NextDouble() < 0.6)
									{
										tryToAddMonster(new DustSpirit(Vector2.Zero), k, i - 1);
									}
									if (mineRandom.NextDouble() < 0.6)
									{
										tryToAddMonster(new DustSpirit(Vector2.Zero), k, i + 1);
									}
								}
								if (mineRandom.NextDouble() < 0.00175)
								{
									monsterToAdd2.hasSpecialItem.Value = true;
								}
								if (monsterToAdd2.GetBoundingBox().Width <= 64 || isTileClearForMineObjects(k + 1, i))
								{
									characters.Add(monsterToAdd2);
								}
							}
							else if (mineRandom.NextDouble() <= itemChance)
							{
								Vector2 objectPos3 = new Vector2(k, i);
								base.Objects.Add(objectPos3, getRandomItemForThisLevel(mineLevel));
							}
							else if (mineRandom.NextDouble() <= 0.005 && !isDarkArea() && !mustKillAllMonstersToAdvance() && isTileClearForMineObjects(k + 1, i) && isTileClearForMineObjects(k, i + 1) && isTileClearForMineObjects(k + 1, i + 1))
							{
								Vector2 objectPos2 = new Vector2(k, i);
								int whichClump = (mineRandom.NextDouble() < 0.5) ? 752 : 754;
								int mineArea = getMineArea();
								if (mineArea == 40)
								{
									whichClump = ((mineRandom.NextDouble() < 0.5) ? 756 : 758);
								}
								resourceClumps.Add(new ResourceClump(whichClump, 2, 2, objectPos2));
							}
						}
						else if (isContainerPlatform(k, i) && isTileLocationTotallyClearAndPlaceable(k, i) && mineRandom.NextDouble() < 0.4 && (firstTime || canAdd(0, barrelsAdded)))
						{
							Vector2 objectPos = new Vector2(k, i);
							objects.Add(objectPos, new BreakableContainer(objectPos, 118, this));
							barrelsAdded++;
							if (firstTime)
							{
								updateMineLevelData(0);
							}
						}
						else if (mineRandom.NextDouble() <= monsterChance && isTileLocationTotallyClearAndPlaceable(k, i) && isTileOnClearAndSolidGround(k, i) && getDistanceFromStart(k, i) > 5f && (!Game1.player.hasBuff(23) || getMineArea() == 121))
						{
							Monster monsterToAdd = getMonsterForThisLevel(mineLevel, k, i);
							if (mineRandom.NextDouble() < 0.01)
							{
								monsterToAdd.hasSpecialItem.Value = true;
							}
							characters.Add(monsterToAdd);
						}
					}
				}
				if (stonesLeftOnThisLevel > 35)
				{
					int tries = stonesLeftOnThisLevel / 35;
					for (int j = 0; j < tries; j++)
					{
						Vector2 stone2 = objects.Keys.ElementAt(mineRandom.Next(objects.Count()));
						if (!objects[stone2].name.Equals("Stone"))
						{
							continue;
						}
						int radius = mineRandom.Next(3, 8);
						bool monsterSpot = mineRandom.NextDouble() < 0.1;
						for (int x = (int)stone2.X - radius / 2; (float)x < stone2.X + (float)(radius / 2); x++)
						{
							for (int y = (int)stone2.Y - radius / 2; (float)y < stone2.Y + (float)(radius / 2); y++)
							{
								Vector2 tile = new Vector2(x, y);
								if (objects.ContainsKey(tile) && objects[tile].name.Equals("Stone"))
								{
									objects.Remove(tile);
									stonesLeftOnThisLevel--;
									if (monsterSpot && mineRandom.NextDouble() < 0.12)
									{
										characters.Add(getMonsterForThisLevel(mineLevel, x, y));
									}
								}
							}
						}
					}
				}
				tryToAddAreaUniques();
				if (mineRandom.NextDouble() < 0.95 && !mustKillAllMonstersToAdvance() && mineLevel > 1 && mineLevel % 5 != 0 && shouldCreateLadderOnThisLevel())
				{
					Vector2 possibleSpot = new Vector2(mineRandom.Next(map.GetLayer("Back").LayerWidth), mineRandom.Next(map.GetLayer("Back").LayerHeight));
					if (isTileClearForMineObjects(possibleSpot))
					{
						createLadderDown((int)possibleSpot.X, (int)possibleSpot.Y);
					}
				}
				if (mustKillAllMonstersToAdvance() && EnemyCount <= 1)
				{
					characters.Add(new Bat(tileBeneathLadder * 64f + new Vector2(256f, 256f)));
				}
			}
			if ((!mustKillAllMonstersToAdvance() || isDinoArea) && mineLevel % 5 != 0 && mineLevel > 2 && mineLevel != 220 && !netIsTreasureRoom.Value)
			{
				tryToAddOreClumps();
				if ((bool)isLightingDark)
				{
					tryToAddOldMinerPath();
				}
			}
		}

		public void placeAppropriateOreAt(Vector2 tile)
		{
			if (isTileLocationTotallyClearAndPlaceable(tile))
			{
				objects.Add(tile, getAppropriateOre(tile));
			}
		}

		public Object getAppropriateOre(Vector2 tile)
		{
			Object ore = new Object(tile, 751, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
			ore.minutesUntilReady.Value = 3;
			switch (getMineArea())
			{
			case 40:
				if (mineRandom.NextDouble() < 0.8)
				{
					ore = new Object(tile, 290, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
					ore.minutesUntilReady.Value = 4;
				}
				break;
			case 80:
				if (mineRandom.NextDouble() < 0.8)
				{
					ore = new Object(tile, 764, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
					ore.minutesUntilReady.Value = 8;
				}
				break;
			case 121:
				ore = new Object(tile, 764, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
				ore.minutesUntilReady.Value = 8;
				if (mineRandom.NextDouble() < 0.02)
				{
					ore = new Object(tile, 765, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
					ore.minutesUntilReady.Value = 16;
				}
				break;
			}
			if (mineRandom.NextDouble() < 0.25 && getMineArea() != 40)
			{
				ore = new Object(tile, (mineRandom.NextDouble() < 0.5) ? 668 : 670, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
				ore.minutesUntilReady.Value = 2;
			}
			return ore;
		}

		public void tryToAddOreClumps()
		{
			if (!(mineRandom.NextDouble() < 0.55 + Game1.player.team.AverageDailyLuck(Game1.currentLocation)))
			{
				return;
			}
			Vector2 endPoint = getRandomTile();
			for (int tries = 0; tries < 1 || mineRandom.NextDouble() < 0.25 + Game1.player.team.AverageDailyLuck(Game1.currentLocation); tries++)
			{
				if (isTileLocationTotallyClearAndPlaceable(endPoint) && isTileOnClearAndSolidGround(endPoint) && doesTileHaveProperty((int)endPoint.X, (int)endPoint.Y, "Diggable", "Back") == null)
				{
					Object ore = getAppropriateOre(endPoint);
					if ((int)ore.parentSheetIndex == 670)
					{
						ore.ParentSheetIndex = 668;
					}
					Utility.recursiveObjectPlacement(ore, (int)endPoint.X, (int)endPoint.Y, 0.949999988079071, 0.30000001192092896, this, "Dirt", ((int)ore.parentSheetIndex == 668) ? 1 : 0, 0.05000000074505806, ((int)ore.parentSheetIndex != 668) ? 1 : 2);
				}
				endPoint = getRandomTile();
			}
		}

		public void tryToAddOldMinerPath()
		{
			Vector2 endPoint = getRandomTile();
			int tries = 0;
			while (!isTileOnClearAndSolidGround(endPoint) && tries < 8)
			{
				endPoint = getRandomTile();
				tries++;
			}
			if (!isTileOnClearAndSolidGround(endPoint))
			{
				return;
			}
			Stack<Point> path = PathFindController.findPath(Utility.Vector2ToPoint(tileBeneathLadder), Utility.Vector2ToPoint(endPoint), PathFindController.isAtEndPoint, this, Game1.player, 500);
			if (path == null)
			{
				return;
			}
			while (path.Count > 0)
			{
				Point p = path.Pop();
				removeEverythingExceptCharactersFromThisTile(p.X, p.Y);
				if (path.Count <= 0 || !(mineRandom.NextDouble() < 0.2))
				{
					continue;
				}
				Vector2 torchPosition = Vector2.Zero;
				torchPosition = ((path.Peek().X == p.X) ? new Vector2(p.X + ((!(mineRandom.NextDouble() < 0.5)) ? 1 : (-1)), p.Y) : new Vector2(p.X, p.Y + ((!(mineRandom.NextDouble() < 0.5)) ? 1 : (-1))));
				if (!torchPosition.Equals(Vector2.Zero) && isTileLocationTotallyClearAndPlaceable(torchPosition) && isTileOnClearAndSolidGround(torchPosition))
				{
					if (mineRandom.NextDouble() < 0.5)
					{
						new Torch(torchPosition, 1).placementAction(this, (int)torchPosition.X * 64, (int)torchPosition.Y * 64, null);
					}
					else
					{
						placeAppropriateOreAt(torchPosition);
					}
				}
			}
		}

		public void tryToAddAreaUniques()
		{
			if ((getMineArea() == 10 || getMineArea() == 80 || (getMineArea() == 40 && mineRandom.NextDouble() < 0.1)) && !isDarkArea() && !mustKillAllMonstersToAdvance())
			{
				int tries = mineRandom.Next(7, 24);
				int baseWeedIndex = (getMineArea() == 80) ? 316 : ((getMineArea() == 40) ? 319 : 313);
				for (int i = 0; i < tries; i++)
				{
					Vector2 tile = new Vector2(mineRandom.Next(map.GetLayer("Back").LayerWidth), mineRandom.Next(map.GetLayer("Back").LayerHeight));
					Utility.recursiveObjectPlacement(new Object(tile, baseWeedIndex, "Weeds", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						Fragility = 2,
						CanBeGrabbed = true
					}, (int)tile.X, (int)tile.Y, 1.0, (float)mineRandom.Next(10, 40) / 100f, this, "Dirt", 2, 0.29);
				}
			}
		}

		public void tryToAddMonster(Monster m, int tileX, int tileY)
		{
			if (isTileClearForMineObjects(tileX, tileY) && !isTileOccupied(new Vector2(tileX, tileY)))
			{
				m.setTilePosition(tileX, tileY);
				characters.Add(m);
			}
		}

		public bool isContainerPlatform(int x, int y)
		{
			if (map.GetLayer("Back").Tiles[x, y] != null && map.GetLayer("Back").Tiles[x, y].TileIndex == 257)
			{
				return true;
			}
			return false;
		}

		public bool mustKillAllMonstersToAdvance()
		{
			if (!isSlimeArea && !isMonsterArea)
			{
				return isDinoArea;
			}
			return true;
		}

		public void createLadderAt(Vector2 p, string sound = "hoeHit")
		{
			if (shouldCreateLadderOnThisLevel())
			{
				playSound(sound);
				createLadderAtEvent[p] = true;
			}
		}

		public bool shouldCreateLadderOnThisLevel()
		{
			if (mineLevel != 77377)
			{
				return mineLevel != 120;
			}
			return false;
		}

		private void doCreateLadderAt(Vector2 p)
		{
			string startSound = (Game1.currentLocation == this) ? "sandyStep" : null;
			updateMap();
			setMapTileIndex((int)p.X, (int)p.Y, 173, "Buildings");
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f, Color.White * 0.5f)
			{
				interval = 80f
			});
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(16f, 16f), Color.White * 0.5f)
			{
				delayBeforeAnimationStart = 150,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f + new Vector2(32f, 16f), Color.White * 0.5f)
			{
				delayBeforeAnimationStart = 300,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(32f, -16f), Color.White * 0.5f)
			{
				delayBeforeAnimationStart = 450,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(-16f, 16f), Color.White * 0.5f)
			{
				delayBeforeAnimationStart = 600,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			if (Game1.player.currentLocation == this)
			{
				Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle((int)p.X * 64, (int)p.Y * 64, 64, 64));
			}
		}

		public bool recursiveTryToCreateLadderDown(Vector2 centerTile, string sound = "hoeHit", int maxIterations = 16)
		{
			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(centerTile);
			List<Vector2> closedList = new List<Vector2>();
			for (; iterations < maxIterations; iterations++)
			{
				if (positionsToCheck.Count <= 0)
				{
					break;
				}
				Vector2 currentPoint = positionsToCheck.Dequeue();
				closedList.Add(currentPoint);
				if (!isTileOccupied(currentPoint, "ignoreMe") && isTileOnClearAndSolidGround(currentPoint) && isTileOccupiedByFarmer(currentPoint) == null && doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back") != null && doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back").Equals("Stone"))
				{
					createLadderAt(currentPoint);
					return true;
				}
				Vector2[] directionsTileVectors = Utility.DirectionsTileVectors;
				foreach (Vector2 v in directionsTileVectors)
				{
					if (!closedList.Contains(currentPoint + v))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
			}
			return false;
		}

		public override void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			if ((bool)monster.hasSpecialItem)
			{
				Game1.createItemDebris(getSpecialItemForThisMineLevel(mineLevel, x / 64, y / 64), monster.Position, Game1.random.Next(4), monster.currentLocation);
			}
			else if (mineLevel > 121 && who != null && who.getFriendshipHeartLevelForNPC("Krobus") >= 10 && (int)who.houseUpgradeLevel >= 1 && !who.isMarried() && !who.isEngaged() && Game1.random.NextDouble() < 0.001)
			{
				Game1.createItemDebris(new Object(808, 1), monster.Position, Game1.random.Next(4), monster.currentLocation);
			}
			else
			{
				base.monsterDrop(monster, x, y, who);
			}
			if ((mustKillAllMonstersToAdvance() || !(Game1.random.NextDouble() < 0.15)) && (!mustKillAllMonstersToAdvance() || EnemyCount > 1))
			{
				return;
			}
			Vector2 p = new Vector2(x, y) / 64f;
			p.X = (int)p.X;
			p.Y = (int)p.Y;
			monster.Name = "ignoreMe";
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)p.X * 64, (int)p.Y * 64, 64, 64);
			if (!isTileOccupied(p, "ignoreMe") && isTileOnClearAndSolidGround(p) && !Game1.player.GetBoundingBox().Intersects(tileRect) && doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back") != null && doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back").Equals("Stone"))
			{
				createLadderAt(p);
			}
			else if (mustKillAllMonstersToAdvance() && EnemyCount <= 1)
			{
				p = new Vector2((int)tileBeneathLadder.X, (int)tileBeneathLadder.Y);
				createLadderAt(p, "newArtifact");
				if (mustKillAllMonstersToAdvance() && Game1.player.currentLocation == this)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineShaft.cs.9484"));
				}
			}
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			for (int i = resourceClumps.Count - 1; i >= 0; i--)
			{
				if (resourceClumps[i] != null && resourceClumps[i].getBoundingBox(resourceClumps[i].tile).Contains(tileX * 64, tileY * 64))
				{
					if (resourceClumps[i].performToolAction(t, 1, resourceClumps[i].tile, this))
					{
						resourceClumps.RemoveAt(i);
					}
					return true;
				}
			}
			return base.performToolAction(t, tileX, tileY);
		}

		private void addLevelChests()
		{
			List<Item> chestItem = new List<Item>();
			Vector2 chestSpot = new Vector2(9f, 9f);
			Color tint = Color.White;
			if (mineLevel % 20 == 0 && mineLevel % 40 != 0)
			{
				chestSpot.Y += 4f;
			}
			switch (mineLevel)
			{
			case 5:
				Game1.player.completeQuest(14);
				if (!Game1.player.hasOrWillReceiveMail("guildQuest"))
				{
					Game1.addMailForTomorrow("guildQuest");
				}
				break;
			case 10:
				chestItem.Add(new Boots(506));
				break;
			case 20:
				chestItem.Add(new MeleeWeapon(11));
				break;
			case 40:
				Game1.player.completeQuest(17);
				chestItem.Add(new Slingshot());
				break;
			case 50:
				chestItem.Add(new Boots(509));
				break;
			case 60:
				chestItem.Add(new MeleeWeapon(21));
				break;
			case 70:
				chestItem.Add(new Slingshot(33));
				break;
			case 80:
				chestItem.Add(new Boots(512));
				break;
			case 90:
				chestItem.Add(new MeleeWeapon(8));
				break;
			case 100:
				chestItem.Add(new Object(434, 1));
				break;
			case 110:
				chestItem.Add(new Boots(514));
				break;
			case 120:
				Game1.player.completeQuest(18);
				Game1.getSteamAchievement("Achievement_TheBottom");
				if (!Game1.player.hasSkullKey)
				{
					chestItem.Add(new SpecialItem(4));
				}
				tint = Color.Pink;
				break;
			case 220:
				if (Game1.player.secretNotesSeen.Contains(10) && !Game1.player.mailReceived.Contains("qiCave"))
				{
					Game1.eventUp = true;
					Game1.displayHUD = false;
					Game1.player.CanMove = false;
					Game1.player.showNotCarrying();
					currentEvent = new Event(Game1.content.LoadString((numberOfCraftedStairsUsedThisRun <= 10) ? "Data\\ExtraDialogue:SkullCavern_100_event_honorable" : "Data\\ExtraDialogue:SkullCavern_100_event"));
					currentEvent.exitLocation = new LocationRequest(base.Name, isStructure: false, this);
				}
				break;
			}
			if (netIsTreasureRoom.Value)
			{
				chestItem.Add(getTreasureRoomItem());
			}
			if (chestItem.Count > 0 && !Game1.player.chestConsumedMineLevels.ContainsKey(mineLevel))
			{
				overlayObjects[chestSpot] = new Chest(0, chestItem, chestSpot)
				{
					Tint = tint
				};
			}
		}

		public static Item getTreasureRoomItem()
		{
			switch (Game1.random.Next(24))
			{
			case 0:
				return new Object(288, 5);
			case 1:
				return new Object(287, 10);
			case 2:
				return new Object(802, 15);
			case 3:
				return new Object(773, Game1.random.Next(2, 5));
			case 4:
				return new Object(749, 5);
			case 5:
				return new Object(688, 5);
			case 6:
				return new Object(681, Game1.random.Next(1, 4));
			case 7:
				return new Object(Game1.random.Next(628, 634), 1);
			case 8:
				return new Object(645, Game1.random.Next(1, 3));
			case 9:
				return new Object(621, 4);
			case 10:
				return new Object(Game1.random.Next(472, 499), Game1.random.Next(1, 5) * 5);
			case 11:
				return new Object(286, 15);
			case 12:
				return new Object(437, 1);
			case 13:
				return new Object(439, 1);
			case 14:
				return new Object(349, Game1.random.Next(2, 5));
			case 15:
				return new Object(337, Game1.random.Next(2, 4));
			case 16:
				return new Object(Game1.random.Next(235, 245), 5);
			case 17:
				return new Object(74, 1);
			case 18:
				return new Object(Vector2.Zero, 21);
			case 19:
				return new Object(Vector2.Zero, 25);
			case 20:
				return new Object(Vector2.Zero, 165);
			case 21:
				return new Hat(37);
			case 22:
				return new Hat(38);
			case 23:
				return new Hat(65);
			default:
				return new Object(288, 5);
			}
		}

		public static Item getSpecialItemForThisMineLevel(int level, int x, int y)
		{
			Random r = new Random(level + (int)Game1.stats.DaysPlayed + x + y * 10000);
			if (level < 20)
			{
				switch (r.Next(6))
				{
				case 0:
					return new MeleeWeapon(16);
				case 1:
					return new MeleeWeapon(24);
				case 2:
					return new Boots(504);
				case 3:
					return new Boots(505);
				case 4:
					return new Ring(516);
				case 5:
					return new Ring(518);
				}
			}
			else if (level < 40)
			{
				switch (r.Next(7))
				{
				case 0:
					return new MeleeWeapon(22);
				case 1:
					return new MeleeWeapon(24);
				case 2:
					return new Boots(504);
				case 3:
					return new Boots(505);
				case 4:
					return new Ring(516);
				case 5:
					return new Ring(518);
				case 6:
					return new MeleeWeapon(15);
				}
			}
			else if (level < 60)
			{
				switch (r.Next(7))
				{
				case 0:
					return new MeleeWeapon(6);
				case 1:
					return new MeleeWeapon(26);
				case 2:
					return new MeleeWeapon(15);
				case 3:
					return new Boots(510);
				case 4:
					return new Ring(517);
				case 5:
					return new Ring(519);
				case 6:
					return new MeleeWeapon(27);
				}
			}
			else if (level < 160)
			{
				switch (r.Next(7))
				{
				case 0:
					return new MeleeWeapon(26);
				case 1:
					return new MeleeWeapon(26);
				case 2:
					return new Boots(508);
				case 3:
					return new Boots(510);
				case 4:
					return new Ring(517);
				case 5:
					return new Ring(519);
				case 6:
					return new MeleeWeapon(26);
				}
			}
			else if (level < 100)
			{
				switch (r.Next(7))
				{
				case 0:
					return new MeleeWeapon(48);
				case 1:
					return new MeleeWeapon(48);
				case 2:
					return new Boots(511);
				case 3:
					return new Boots(513);
				case 4:
					return new MeleeWeapon(18);
				case 5:
					return new MeleeWeapon(28);
				case 6:
					return new MeleeWeapon(52);
				}
			}
			else if (level < 120)
			{
				switch (r.Next(6))
				{
				case 0:
					return new MeleeWeapon(19);
				case 1:
					return new MeleeWeapon(50);
				case 2:
					return new Boots(511);
				case 3:
					return new Boots(513);
				case 4:
					return new MeleeWeapon(18);
				case 5:
					return new MeleeWeapon(46);
				}
			}
			else
			{
				switch (r.Next(8))
				{
				case 0:
					return new MeleeWeapon(45);
				case 1:
					return new MeleeWeapon(50);
				case 2:
					return new Boots(511);
				case 3:
					return new Boots(513);
				case 4:
					return new MeleeWeapon(18);
				case 5:
					return new MeleeWeapon(28);
				case 6:
					return new MeleeWeapon(52);
				case 7:
					return new Object(787, 1);
				}
			}
			return new Object(78, 1);
		}

		public override bool isTileOccupied(Vector2 tileLocation, string characterToIgnore = "", bool ignoreAllCharacters = false)
		{
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.occupiesTile((int)tileLocation.X, (int)tileLocation.Y))
				{
					return true;
				}
			}
			if (tileBeneathLadder.Equals(tileLocation))
			{
				return true;
			}
			if (tileBeneathElevator != Vector2.Zero && tileBeneathElevator.Equals(tileLocation))
			{
				return true;
			}
			return base.isTileOccupied(tileLocation, characterToIgnore, ignoreAllCharacters);
		}

		public bool isDarkArea()
		{
			if (loadedDarkArea || mineLevel % 40 > 30)
			{
				return getMineArea() != 40;
			}
			return false;
		}

		public bool isTileClearForMineObjects(Vector2 v)
		{
			if (tileBeneathLadder.Equals(v) || tileBeneathElevator.Equals(v))
			{
				return false;
			}
			if (!isTileLocationTotallyClearAndPlaceable(v))
			{
				return false;
			}
			string s = doesTileHaveProperty((int)v.X, (int)v.Y, "Type", "Back");
			if (s == null || !s.Equals("Stone"))
			{
				return false;
			}
			if (!isTileOnClearAndSolidGround(v))
			{
				return false;
			}
			if (objects.ContainsKey(v))
			{
				return false;
			}
			return true;
		}

		public bool isTileOnClearAndSolidGround(Vector2 v)
		{
			if (map.GetLayer("Back").Tiles[(int)v.X, (int)v.Y] == null)
			{
				return false;
			}
			if (map.GetLayer("Front").Tiles[(int)v.X, (int)v.Y] != null || map.GetLayer("Buildings").Tiles[(int)v.X, (int)v.Y] != null)
			{
				return false;
			}
			if (getTileIndexAt((int)v.X, (int)v.Y, "Back") == 77)
			{
				return false;
			}
			return true;
		}

		public bool isTileOnClearAndSolidGround(int x, int y)
		{
			if (map.GetLayer("Back").Tiles[x, y] == null)
			{
				return false;
			}
			if (map.GetLayer("Front").Tiles[x, y] != null)
			{
				return false;
			}
			if (getTileIndexAt(x, y, "Back") == 77)
			{
				return false;
			}
			return true;
		}

		public bool isTileClearForMineObjects(int x, int y)
		{
			return isTileClearForMineObjects(new Vector2(x, y));
		}

		public void loadLevel(int level)
		{
			isMonsterArea = false;
			isSlimeArea = false;
			loadedDarkArea = false;
			isQuarryArea = false;
			isDinoArea = false;
			mineLoader.Unload();
			mineLoader.Dispose();
			mineLoader = Game1.content.CreateTemporary();
			int mapNumberToLoad2 = (level % 40 % 20 == 0 && level % 40 != 0) ? 20 : ((level % 10 == 0) ? 10 : level);
			mapNumberToLoad2 %= 40;
			if (level == 120)
			{
				mapNumberToLoad2 = 120;
			}
			if (getMineArea(level) == 121)
			{
				MineShaft last_level = null;
				foreach (MineShaft mine in activeMines)
				{
					if (mine != null && mine.mineLevel > 120 && mine.mineLevel < level && (last_level == null || mine.mineLevel > last_level.mineLevel))
					{
						last_level = mine;
					}
				}
				mapNumberToLoad2 = mineRandom.Next(40);
				while (last_level != null && mapNumberToLoad2 == last_level.loadedMapNumber)
				{
					mapNumberToLoad2 = mineRandom.Next(40);
				}
				while (mapNumberToLoad2 % 5 == 0)
				{
					mapNumberToLoad2 = mineRandom.Next(40);
				}
				if (level == 220)
				{
					mapNumberToLoad2 = 10;
				}
				else if (level >= 130)
				{
					double chance2 = 0.01;
					chance2 += Game1.player.team.AverageDailyLuck(Game1.currentLocation) / 10.0 + Game1.player.team.AverageLuckLevel(Game1.currentLocation) / 100.0;
					if (Game1.random.NextDouble() < chance2)
					{
						netIsTreasureRoom.Value = true;
						mapNumberToLoad2 = 10;
					}
				}
			}
			else if (getMineArea() == 77377 && mineLevel == 77377)
			{
				mapNumberToLoad2 = 77377;
			}
			mapPath.Value = "Maps\\Mines\\" + mapNumberToLoad2;
			loadedMapNumber = mapNumberToLoad2;
			updateMap();
			Random r = new Random((int)Game1.stats.DaysPlayed + level * 100 + (int)Game1.uniqueIDForThisGame / 2);
			if ((!Game1.player.hasBuff(23) || getMineArea() == 121) && r.NextDouble() < 0.044 && mapNumberToLoad2 % 5 != 0 && mapNumberToLoad2 % 40 > 5 && mapNumberToLoad2 % 40 < 30 && mapNumberToLoad2 % 40 != 19)
			{
				if (r.NextDouble() < 0.5)
				{
					isMonsterArea = true;
				}
				else
				{
					isSlimeArea = true;
				}
				if (getMineArea() == 121 && mineLevel > 126 && r.NextDouble() < 0.5)
				{
					isDinoArea = true;
					isSlimeArea = false;
					isMonsterArea = false;
				}
			}
			else if (mineLevel < 121 && r.NextDouble() < 0.044 && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccCraftsRoom") && mapNumberToLoad2 % 40 > 1 && mapNumberToLoad2 % 5 != 0)
			{
				isQuarryArea = true;
				if (r.NextDouble() < 0.25)
				{
					isMonsterArea = true;
				}
			}
			if (isQuarryArea || getMineArea(level) == 77377)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_quarryshaft";
				int numBrownSpots = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight / 100;
				isQuarryArea = true;
				isSlimeArea = false;
				isDinoArea = false;
				for (int i = 0; i < numBrownSpots; i++)
				{
					brownSpots.Add(new Vector2(mineRandom.Next(0, map.Layers[0].LayerWidth), mineRandom.Next(0, map.Layers[0].LayerHeight)));
				}
			}
			else if (isDinoArea)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_dino";
			}
			else if (isSlimeArea)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_slime";
			}
			else if (getMineArea() == 0 || getMineArea() == 10 || (getMineArea(level) != 0 && getMineArea(level) != 10))
			{
				if (getMineArea(level) == 40)
				{
					mapImageSource.Value = "Maps\\Mines\\mine_frost";
					if (level >= 70)
					{
						mapImageSource.Value += "_dark";
						loadedDarkArea = true;
					}
				}
				else if (getMineArea(level) == 80)
				{
					mapImageSource.Value = "Maps\\Mines\\mine_lava";
					if (level >= 110 && level != 120)
					{
						mapImageSource.Value += "_dark";
						loadedDarkArea = true;
					}
				}
				else if (getMineArea(level) == 121)
				{
					mapImageSource.Value = "Maps\\Mines\\mine_desert";
					if (mapNumberToLoad2 % 40 >= 30)
					{
						mapImageSource.Value += "_dark";
						loadedDarkArea = true;
					}
				}
			}
			if (!map.TileSheets[0].TileIndexProperties[165].ContainsKey("Diggable"))
			{
				map.TileSheets[0].TileIndexProperties[165].Add("Diggable", new PropertyValue("true"));
			}
			if (!map.TileSheets[0].TileIndexProperties[181].ContainsKey("Diggable"))
			{
				map.TileSheets[0].TileIndexProperties[181].Add("Diggable", new PropertyValue("true"));
			}
			if (!map.TileSheets[0].TileIndexProperties[183].ContainsKey("Diggable"))
			{
				map.TileSheets[0].TileIndexProperties[183].Add("Diggable", new PropertyValue("true"));
			}
			if (!isSideBranch())
			{
				lowestLevelReached = Math.Max(lowestLevelReached, level);
				if (mineLevel % 5 == 0 && getMineArea() != 121)
				{
					prepareElevator();
				}
			}
			Utility.CollectGarbage();
		}

		public static void CheckForQiChallengeCompletion()
		{
			if (Game1.player.deepestMineLevel >= 145 && Game1.player.hasQuest(20) && !Game1.player.hasOrWillReceiveMail("QiChallengeComplete"))
			{
				Game1.player.completeQuest(20);
				Game1.addMailForTomorrow("QiChallengeComplete");
			}
		}

		private void prepareElevator()
		{
			Point elevatorSpot = ElevatorLightSpot = Utility.findTile(this, 80, "Buildings");
			if (elevatorSpot.X >= 0)
			{
				if (canAdd(3, 0))
				{
					elevatorShouldDing.Value = true;
					updateMineLevelData(3);
				}
				else
				{
					setMapTileIndex(elevatorSpot.X, elevatorSpot.Y, 48, "Buildings");
				}
			}
		}

		public void enterMineShaft()
		{
			DelayedAction.playSoundAfterDelay("fallDown", 1200);
			DelayedAction.playSoundAfterDelay("clubSmash", 2200);
			Random random = new Random(mineLevel + (int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
			int levelsDown = random.Next(3, 9);
			if (random.NextDouble() < 0.1)
			{
				levelsDown = levelsDown * 2 - 1;
			}
			if (mineLevel < 220 && mineLevel + levelsDown > 220)
			{
				levelsDown = 220 - mineLevel;
			}
			lastLevelsDownFallen = levelsDown;
			Game1.player.health = Math.Max(1, Game1.player.health - levelsDown * 3);
			isFallingDownShaft = true;
			Game1.globalFadeToBlack(afterFall, 0.045f);
			Game1.player.CanMove = false;
			Game1.player.jump();
		}

		private void afterFall()
		{
			Game1.drawObjectDialogue(Game1.content.LoadString((lastLevelsDownFallen > 7) ? "Strings\\Locations:Mines_FallenFar" : "Strings\\Locations:Mines_Fallen", lastLevelsDownFallen));
			Game1.messagePause = true;
			Game1.enterMine(mineLevel + lastLevelsDownFallen);
			Game1.fadeToBlackAlpha = 1f;
			Game1.player.faceDirection(2);
			Game1.player.showFrame(5);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Tile tile = map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
			if (tile != null && who.IsLocalPlayer)
			{
				switch (tile.TileIndex)
				{
				case 112:
					if (mineLevel <= 120)
					{
						Game1.activeClickableMenu = new MineElevatorMenu();
						return true;
					}
					break;
				case 115:
				{
					Response[] options2 = new Response[2]
					{
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:Mines_LeaveMine")),
						new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing"))
					};
					createQuestionDialogue(" ", options2, "ExitMine");
					return true;
				}
				case 173:
					Game1.enterMine(mineLevel + 1);
					playSound("stairsdown");
					return true;
				case 174:
				{
					Response[] options = new Response[2]
					{
						new Response("Jump", Game1.content.LoadString("Strings\\Locations:Mines_ShaftJumpIn")),
						new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing"))
					};
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Mines_Shaft"), options, "Shaft");
					return true;
				}
				case 194:
					playSound("openBox");
					playSound("Ship");
					map.GetLayer("Buildings").Tiles[tileLocation].TileIndex++;
					map.GetLayer("Front").Tiles[tileLocation.X, tileLocation.Y - 1].TileIndex++;
					Game1.createRadialDebris(this, 382, tileLocation.X, tileLocation.Y, 6, resource: false, -1, item: true);
					updateMineLevelData(2, -1);
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (Game1.random.NextDouble() < 0.15)
			{
				int objectIndex = 330;
				if (Game1.random.NextDouble() < 0.07)
				{
					if (Game1.random.NextDouble() < 0.75)
					{
						switch (Game1.random.Next(5))
						{
						case 0:
							objectIndex = 96;
							break;
						case 1:
							objectIndex = ((!who.hasOrWillReceiveMail("lostBookFound")) ? 770 : (((int)Game1.netWorldState.Value.LostBooksFound < 21) ? 102 : 770));
							break;
						case 2:
							objectIndex = 110;
							break;
						case 3:
							objectIndex = 112;
							break;
						case 4:
							objectIndex = 585;
							break;
						}
					}
					else if (Game1.random.NextDouble() < 0.75)
					{
						switch (getMineArea())
						{
						case 0:
						case 10:
							objectIndex = ((Game1.random.NextDouble() < 0.5) ? 121 : 97);
							break;
						case 40:
							objectIndex = ((Game1.random.NextDouble() < 0.5) ? 122 : 336);
							break;
						case 80:
							objectIndex = 99;
							break;
						}
					}
					else
					{
						objectIndex = ((Game1.random.NextDouble() < 0.5) ? 126 : 127);
					}
				}
				else if (Game1.random.NextDouble() < 0.19)
				{
					objectIndex = ((Game1.random.NextDouble() < 0.5) ? 390 : getOreIndexForLevel(mineLevel, Game1.random));
				}
				else
				{
					if (Game1.random.NextDouble() < 0.08)
					{
						Game1.createRadialDebris(this, 8, xLocation, yLocation, Game1.random.Next(1, 5), resource: true);
						return "";
					}
					if (Game1.random.NextDouble() < 0.45)
					{
						objectIndex = 330;
					}
					else if (Game1.random.NextDouble() < 0.12)
					{
						if (Game1.random.NextDouble() < 0.25)
						{
							objectIndex = 749;
						}
						else
						{
							switch (getMineArea())
							{
							case 0:
							case 10:
								objectIndex = 535;
								break;
							case 40:
								objectIndex = 536;
								break;
							case 80:
								objectIndex = 537;
								break;
							}
						}
					}
					else
					{
						objectIndex = 78;
					}
				}
				Game1.createObjectDebris(objectIndex, xLocation, yLocation, who.UniqueMultiplayerID, this);
				return "";
			}
			return "";
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			foreach (ResourceClump r in resourceClumps)
			{
				if (!glider && r.getBoundingBox(r.tile).Intersects(position))
				{
					return true;
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			foreach (NPC i in characters)
			{
				if (i is Monster)
				{
					(i as Monster).drawAboveAllLayers(b);
				}
			}
			if (fogAlpha > 0f || ambientFog)
			{
				Vector2 v = default(Vector2);
				for (float x = -256 + (int)(fogPos.X % 256f); x < (float)Game1.graphics.GraphicsDevice.Viewport.Width; x += 256f)
				{
					for (float y = -256 + (int)(fogPos.Y % 256f); y < (float)Game1.graphics.GraphicsDevice.Viewport.Height; y += 256f)
					{
						v.X = (int)x;
						v.Y = (int)y;
						b.Draw(Game1.mouseCursors, v, fogSource, (fogAlpha > 0f) ? (fogColor * fogAlpha) : fogColor, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
					}
				}
			}
			if (!Game1.game1.takingMapScreenshot && !isSideBranch())
			{
				if (mustKillAllMonstersToAdvance())
				{
					b.Draw(Game1.mouseCursors, new Vector2(16f, 16f), new Microsoft.Xna.Framework.Rectangle(193, 324, 7, 10), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 25f, SpriteEffects.None, 1f);
					return;
				}
				int col = (getMineArea() == 0 || (isDarkArea() && getMineArea() != 121)) ? 4 : ((getMineArea() == 10) ? 6 : ((getMineArea() == 40) ? 7 : ((getMineArea() == 80) ? 2 : 3)));
				string txt = string.Concat(mineLevel + ((getMineArea() == 121) ? (-120) : 0));
				Microsoft.Xna.Framework.Rectangle tsarea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
				SpriteText.drawString(b, txt, tsarea.Left + 16, tsarea.Top + 16, 999999, -1, 999999, 1f, 1f, junimoText: false, 2, "", col);
			}
		}

		public override void checkForMusic(GameTime time)
		{
			if (Game1.player.freezePause <= 0 && !isFogUp && mineLevel != 120)
			{
				string trackName = "";
				switch (getMineArea())
				{
				case 0:
				case 10:
				case 121:
				case 77377:
					trackName = "Upper";
					break;
				case 40:
					trackName = "Frost";
					break;
				case 80:
					trackName = "Lava";
					break;
				}
				trackName += "_Ambient";
				if (Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || (Game1.getMusicTrackName().EndsWith("_Ambient") && Game1.getMusicTrackName() != trackName))
				{
					Game1.changeMusicTrack(trackName);
				}
				timeSinceLastMusic = Math.Min(335000, timeSinceLastMusic + time.ElapsedGameTime.Milliseconds);
			}
		}

		public string getMineSong()
		{
			if (mineLevel < 40)
			{
				return "EarthMine";
			}
			if (mineLevel < 80)
			{
				return "FrostMine";
			}
			return "LavaMine";
		}

		public bool isPlayingSongFromDifferentArea()
		{
			if (Game1.getMusicTrackName() != getMineSong())
			{
				return Game1.getMusicTrackName().EndsWith("Mine");
			}
			return false;
		}

		public void playMineSong()
		{
			string track_for_area = getMineSong();
			if ((Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("Ambient")) && !isDarkArea() && mineLevel != 77377)
			{
				Game1.changeMusicTrack(track_for_area);
				timeSinceLastMusic = 0;
			}
		}

		protected override void resetLocalState()
		{
			addLevelChests();
			base.resetLocalState();
			if ((bool)elevatorShouldDing)
			{
				timeUntilElevatorLightUp = 1500;
			}
			else if (mineLevel % 5 == 0 && getMineArea() != 121)
			{
				setElevatorLit();
			}
			if (!isSideBranch(mineLevel))
			{
				Game1.player.deepestMineLevel = Math.Max(Game1.player.deepestMineLevel, mineLevel);
			}
			CheckForQiChallengeCompletion();
			if (mineLevel == 120)
			{
				int num = ++Game1.player.timesReachedMineBottom;
			}
			Vector2 vector = mineEntrancePosition(Game1.player);
			Game1.xLocationAfterWarp = (int)vector.X;
			Game1.yLocationAfterWarp = (int)vector.Y;
			if (Game1.IsClient)
			{
				Game1.player.Position = new Vector2(Game1.xLocationAfterWarp * 64, Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16);
				if (!map.TileSheets[0].TileIndexProperties[165].ContainsKey("Diggable"))
				{
					map.TileSheets[0].TileIndexProperties[165].Add("Diggable", new PropertyValue("true"));
				}
				if (!map.TileSheets[0].TileIndexProperties[181].ContainsKey("Diggable"))
				{
					map.TileSheets[0].TileIndexProperties[181].Add("Diggable", new PropertyValue("true"));
				}
				if (!map.TileSheets[0].TileIndexProperties[183].ContainsKey("Diggable"))
				{
					map.TileSheets[0].TileIndexProperties[183].Add("Diggable", new PropertyValue("true"));
				}
			}
			forceViewportPlayerFollow = true;
			if (mineLevel == 20 && !Game1.IsMultiplayer && Game1.isRaining && Game1.player.eventsSeen.Contains(901756) && !Game1.IsMultiplayer)
			{
				characters.Clear();
				NPC nPC = new NPC(new AnimatedSprite("Characters\\Abigail", 0, 16, 32), new Vector2(896f, 644f), "SeedShop", 3, "AbigailMine", datable: true, null, Game1.content.Load<Texture2D>("Portraits\\Abigail"));
				nPC.displayName = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions")["Abigail"].Split('/')[11];
				NPC a = nPC;
				Random r2 = new Random((int)Game1.stats.DaysPlayed);
				if (!Game1.player.mailReceived.Contains("AbigailInMineFirst"))
				{
					a.setNewDialogue(Game1.content.LoadString("Strings\\Characters:AbigailInMineFirst"));
					a.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(0, 300),
						new FarmerSprite.AnimationFrame(1, 300),
						new FarmerSprite.AnimationFrame(2, 300),
						new FarmerSprite.AnimationFrame(3, 300)
					});
					Game1.player.mailReceived.Add("AbigailInMineFirst");
				}
				else if (r2.NextDouble() < 0.15)
				{
					a.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(16, 500),
						new FarmerSprite.AnimationFrame(17, 500),
						new FarmerSprite.AnimationFrame(18, 500),
						new FarmerSprite.AnimationFrame(19, 500)
					});
					a.setNewDialogue(Game1.content.LoadString("Strings\\Characters:AbigailInMineFlute"));
					Game1.changeMusicTrack("AbigailFlute");
				}
				else
				{
					a.setNewDialogue(Game1.content.LoadString("Strings\\Characters:AbigailInMine" + r2.Next(5)));
					a.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(0, 300),
						new FarmerSprite.AnimationFrame(1, 300),
						new FarmerSprite.AnimationFrame(2, 300),
						new FarmerSprite.AnimationFrame(3, 300)
					});
				}
				characters.Add(a);
			}
			if (isMonsterArea || isSlimeArea)
			{
				Random r = new Random((int)Game1.stats.DaysPlayed);
				Game1.showGlobalMessage(Game1.content.LoadString((r.NextDouble() < 0.5) ? "Strings\\Locations:Mines_Infested" : "Strings\\Locations:Mines_Overrun"));
			}
			bool num2 = mineLevel % 20 == 0;
			bool foundAnyWater = false;
			if (num2)
			{
				waterTiles = new bool[map.Layers[0].LayerWidth, map.Layers[0].LayerHeight];
				waterColor.Value = ((getMineArea() == 80) ? (Color.Red * 0.8f) : (new Color(50, 100, 200) * 0.5f));
				for (int y = 0; y < map.GetLayer("Buildings").LayerHeight; y++)
				{
					for (int x = 0; x < map.GetLayer("Buildings").LayerWidth; x++)
					{
						if (doesTileHaveProperty(x, y, "Water", "Back") != null)
						{
							foundAnyWater = true;
							waterTiles[x, y] = true;
							if (getMineArea() == 80 && Game1.random.NextDouble() < 0.1)
							{
								sharedLights[x + y * 1000] = new LightSource(4, new Vector2(x, y) * 64f, 2f, new Color(0, 220, 220), x + y * 1000, LightSource.LightContext.None, 0L);
							}
						}
					}
				}
			}
			if (!foundAnyWater)
			{
				waterTiles = null;
			}
			if (getMineArea(mineLevel) != getMineArea(mineLevel - 1) || mineLevel == 120 || isPlayingSongFromDifferentArea())
			{
				Game1.changeMusicTrack("none");
			}
			if (mineLevel == 77377 && Game1.player.mailReceived.Contains("gotGoldenScythe"))
			{
				setMapTileIndex(29, 4, 245, "Front");
				setMapTileIndex(30, 4, 246, "Front");
				setMapTileIndex(29, 5, 261, "Front");
				setMapTileIndex(30, 5, 262, "Front");
				setMapTileIndex(29, 6, 277, "Buildings");
				setMapTileIndex(30, 56, 278, "Buildings");
			}
			if (mineLevel > 1 && (mineLevel == 2 || (mineLevel % 5 != 0 && timeSinceLastMusic > 150000 && Game1.random.NextDouble() < 0.5)))
			{
				playMineSong();
			}
		}

		public void createLadderDown(int x, int y, bool forceShaft = false)
		{
			createLadderDownEvent[new Point(x, y)] = (forceShaft || (getMineArea() == 121 && !mustKillAllMonstersToAdvance() && mineRandom.NextDouble() < 0.2));
		}

		private void doCreateLadderDown(Point point, bool shaft)
		{
			updateMap();
			int x = point.X;
			int y = point.Y;
			if (shaft)
			{
				map.GetLayer("Buildings").Tiles[x, y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 174);
			}
			else
			{
				ladderHasSpawned = true;
				map.GetLayer("Buildings").Tiles[x, y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 173);
			}
			if (Game1.player.currentLocation == this)
			{
				Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64));
			}
		}

		public void checkStoneForItems(int tileIndexOfStone, int x, int y, Farmer who)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			double chanceModifier = who.DailyLuck / 2.0 + (double)who.MiningLevel * 0.005 + (double)who.LuckLevel * 0.001;
			Random r = new Random(x * 1000 + y + mineLevel + (int)Game1.uniqueIDForThisGame / 2);
			r.NextDouble();
			double oreModifier = (tileIndexOfStone == 40 || tileIndexOfStone == 42) ? 1.2 : 0.8;
			if (tileIndexOfStone != 34 && tileIndexOfStone != 36 && tileIndexOfStone != 50)
			{
				_ = 52;
			}
			stonesLeftOnThisLevel--;
			double chanceForLadderDown = 0.02 + 1.0 / (double)Math.Max(1, stonesLeftOnThisLevel) + (double)who.LuckLevel / 100.0 + Game1.player.DailyLuck / 5.0;
			if (EnemyCount == 0)
			{
				chanceForLadderDown += 0.04;
			}
			if (!ladderHasSpawned && !mustKillAllMonstersToAdvance() && (stonesLeftOnThisLevel == 0 || r.NextDouble() < chanceForLadderDown) && shouldCreateLadderOnThisLevel())
			{
				createLadderDown(x, y);
			}
			if (breakStone(tileIndexOfStone, x, y, who, r))
			{
				return;
			}
			if (tileIndexOfStone == 44)
			{
				int whichGem2 = r.Next(59, 70);
				whichGem2 += whichGem2 % 2;
				if (who.timesReachedMineBottom == 0)
				{
					if (mineLevel < 40 && whichGem2 != 66 && whichGem2 != 68)
					{
						whichGem2 = ((r.NextDouble() < 0.5) ? 66 : 68);
					}
					else if (mineLevel < 80 && (whichGem2 == 64 || whichGem2 == 60))
					{
						whichGem2 = ((!(r.NextDouble() < 0.5)) ? ((r.NextDouble() < 0.5) ? 68 : 62) : ((r.NextDouble() < 0.5) ? 66 : 70));
					}
				}
				Game1.createObjectDebris(whichGem2, x, y, who.uniqueMultiplayerID, this);
				Game1.stats.OtherPreciousGemsFound++;
				return;
			}
			if (r.NextDouble() < 0.022 * (1.0 + chanceModifier) * (double)((!who.professions.Contains(22)) ? 1 : 2))
			{
				int index = 535 + ((getMineArea() == 40) ? 1 : ((getMineArea() == 80) ? 2 : 0));
				if (getMineArea() == 121)
				{
					index = 749;
				}
				if (who.professions.Contains(19) && r.NextDouble() < 0.5)
				{
					Game1.createObjectDebris(index, x, y, who.UniqueMultiplayerID, this);
				}
				Game1.createObjectDebris(index, x, y, who.UniqueMultiplayerID, this);
				who.gainExperience(5, 20 * getMineArea());
			}
			if (mineLevel > 20 && r.NextDouble() < 0.005 * (1.0 + chanceModifier) * (double)((!who.professions.Contains(22)) ? 1 : 2))
			{
				if (who.professions.Contains(19) && r.NextDouble() < 0.5)
				{
					Game1.createObjectDebris(749, x, y, who.UniqueMultiplayerID, this);
				}
				Game1.createObjectDebris(749, x, y, who.UniqueMultiplayerID, this);
				who.gainExperience(5, 40 * getMineArea());
			}
			if (r.NextDouble() < 0.05 * (1.0 + chanceModifier) * oreModifier)
			{
				r.Next(1, 3);
				r.NextDouble();
				_ = 0.1 * (1.0 + chanceModifier);
				if (r.NextDouble() < 0.25 * (double)((!who.professions.Contains(21)) ? 1 : 2))
				{
					Game1.createObjectDebris(382, x, y, who.UniqueMultiplayerID, this);
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, Game1.random.NextDouble() < 0.5, 80f, 0, -1, -1f, 128));
				}
				Game1.createObjectDebris(getOreIndexForLevel(mineLevel, r), x, y, who.UniqueMultiplayerID, this);
				who.gainExperience(3, 5);
			}
			else if (r.NextDouble() < 0.5)
			{
				Game1.createDebris(14, x, y, 1, this);
			}
		}

		public int getOreIndexForLevel(int mineLevel, Random r)
		{
			if (getMineArea(mineLevel) == 77377)
			{
				return 380;
			}
			if (mineLevel < 40)
			{
				if (mineLevel >= 20 && r.NextDouble() < 0.1)
				{
					return 380;
				}
				return 378;
			}
			if (mineLevel < 80)
			{
				if (mineLevel >= 60 && r.NextDouble() < 0.1)
				{
					return 384;
				}
				if (!(r.NextDouble() < 0.75))
				{
					return 378;
				}
				return 380;
			}
			if (mineLevel < 120)
			{
				if (!(r.NextDouble() < 0.75))
				{
					if (!(r.NextDouble() < 0.75))
					{
						return 378;
					}
					return 380;
				}
				return 384;
			}
			if (r.NextDouble() < 0.01 + (double)((float)(mineLevel - 120) / 2000f))
			{
				return 386;
			}
			if (!(r.NextDouble() < 0.75))
			{
				if (!(r.NextDouble() < 0.75))
				{
					return 378;
				}
				return 380;
			}
			return 384;
		}

		public int getMineArea(int level = -1)
		{
			if (level == -1)
			{
				level = mineLevel;
			}
			if (!isQuarryArea)
			{
				switch (level)
				{
				case 77377:
					break;
				case 80:
				case 81:
				case 82:
				case 83:
				case 84:
				case 85:
				case 86:
				case 87:
				case 88:
				case 89:
				case 90:
				case 91:
				case 92:
				case 93:
				case 94:
				case 95:
				case 96:
				case 97:
				case 98:
				case 99:
				case 100:
				case 101:
				case 102:
				case 103:
				case 104:
				case 105:
				case 106:
				case 107:
				case 108:
				case 109:
				case 110:
				case 111:
				case 112:
				case 113:
				case 114:
				case 115:
				case 116:
				case 117:
				case 118:
				case 119:
				case 120:
					return 80;
				default:
					if (level > 120)
					{
						return 121;
					}
					if (level >= 40)
					{
						return 40;
					}
					if (level > 10 && mineLevel < 30)
					{
						return 10;
					}
					return 0;
				}
			}
			return 77377;
		}

		public bool isSideBranch(int level = -1)
		{
			if (level == -1)
			{
				level = mineLevel;
			}
			return level == 77377;
		}

		public byte getWallAt(int x, int y)
		{
			return byte.MaxValue;
		}

		public Color getLightingColor(GameTime time)
		{
			return lighting;
		}

		public Object getRandomItemForThisLevel(int level)
		{
			int index = 80;
			if (mineRandom.NextDouble() < 0.05 && level > 80)
			{
				index = 422;
			}
			else if (mineRandom.NextDouble() < 0.1 && level > 20 && getMineArea() != 40)
			{
				index = 420;
			}
			else if (mineRandom.NextDouble() < 0.25)
			{
				switch (getMineArea())
				{
				case 0:
				case 10:
					index = 86;
					break;
				case 40:
					index = 84;
					break;
				case 80:
					index = 82;
					break;
				case 121:
					index = ((mineRandom.NextDouble() < 0.3) ? 86 : ((mineRandom.NextDouble() < 0.3) ? 84 : 82));
					break;
				}
			}
			else
			{
				index = 80;
			}
			if (isDinoArea)
			{
				index = 259;
				if (mineRandom.NextDouble() < 0.06)
				{
					index = 107;
				}
			}
			return new Object(index, 1)
			{
				IsSpawnedObject = true
			};
		}

		public bool shouldShowDarkHoeDirt()
		{
			if (getMineArea() == 121 && !isDinoArea)
			{
				return false;
			}
			return true;
		}

		public int getRandomGemRichStoneForThisLevel(int level)
		{
			int whichGem2 = mineRandom.Next(59, 70);
			whichGem2 += whichGem2 % 2;
			if (Game1.player.timesReachedMineBottom == 0)
			{
				if (level < 40 && whichGem2 != 66 && whichGem2 != 68)
				{
					whichGem2 = ((mineRandom.NextDouble() < 0.5) ? 66 : 68);
				}
				else if (level < 80 && (whichGem2 == 64 || whichGem2 == 60))
				{
					whichGem2 = ((!(mineRandom.NextDouble() < 0.5)) ? ((mineRandom.NextDouble() < 0.5) ? 68 : 62) : ((mineRandom.NextDouble() < 0.5) ? 66 : 70));
				}
			}
			switch (whichGem2)
			{
			case 66:
				return 8;
			case 68:
				return 10;
			case 60:
				return 12;
			case 70:
				return 6;
			case 64:
				return 4;
			case 62:
				return 14;
			default:
				return 40;
			}
		}

		public float getDistanceFromStart(int xTile, int yTile)
		{
			float distance = Utility.distance(xTile, tileBeneathLadder.X, yTile, tileBeneathLadder.Y);
			if (tileBeneathElevator != Vector2.Zero)
			{
				distance = Math.Min(distance, Utility.distance(xTile, tileBeneathElevator.X, yTile, tileBeneathElevator.Y));
			}
			return distance;
		}

		public Monster getMonsterForThisLevel(int level, int xTile, int yTile)
		{
			Vector2 position = new Vector2(xTile, yTile) * 64f;
			float distanceFromLadder = getDistanceFromStart(xTile, yTile);
			if (isSlimeArea)
			{
				if (mineRandom.NextDouble() < 0.2)
				{
					return new BigSlime(position, getMineArea());
				}
				return new GreenSlime(position, mineLevel);
			}
			if (isDinoArea)
			{
				if (mineRandom.NextDouble() < 0.1)
				{
					return new Bat(position, 999);
				}
				if (mineRandom.NextDouble() < 0.1)
				{
					return new Fly(position, hard: true);
				}
				return new DinoMonster(position);
			}
			if (getMineArea() == 0 || getMineArea() == 10)
			{
				if (mineRandom.NextDouble() < 0.25 && !mustKillAllMonstersToAdvance())
				{
					return new Bug(position, mineRandom.Next(4), this);
				}
				if (level < 15)
				{
					if (doesTileHaveProperty(xTile, yTile, "Diggable", "Back") != null)
					{
						return new Duggy(position);
					}
					if (mineRandom.NextDouble() < 0.15)
					{
						return new RockCrab(position);
					}
					return new GreenSlime(position, level);
				}
				if (level <= 30)
				{
					if (doesTileHaveProperty(xTile, yTile, "Diggable", "Back") != null)
					{
						return new Duggy(position);
					}
					if (mineRandom.NextDouble() < 0.15)
					{
						return new RockCrab(position);
					}
					if (mineRandom.NextDouble() < 0.05 && distanceFromLadder > 10f)
					{
						return new Fly(position);
					}
					if (mineRandom.NextDouble() < 0.45)
					{
						return new GreenSlime(position, level);
					}
					return new Grub(position);
				}
				if (level <= 40)
				{
					if (mineRandom.NextDouble() < 0.1 && distanceFromLadder > 10f)
					{
						return new Bat(position, level);
					}
					return new RockGolem(position, this);
				}
			}
			else if (getMineArea() == 40)
			{
				if (mineLevel >= 70 && mineRandom.NextDouble() < 0.75)
				{
					return new Skeleton(position);
				}
				if (mineRandom.NextDouble() < 0.3)
				{
					return new DustSpirit(position, mineRandom.NextDouble() < 0.8);
				}
				if (mineRandom.NextDouble() < 0.3 && distanceFromLadder > 10f)
				{
					return new Bat(position, mineLevel);
				}
				if (!ghostAdded && mineLevel > 50 && mineRandom.NextDouble() < 0.3 && distanceFromLadder > 10f)
				{
					ghostAdded = true;
					return new Ghost(position);
				}
			}
			else if (getMineArea() == 80)
			{
				if (isDarkArea() && mineRandom.NextDouble() < 0.25)
				{
					return new Bat(position, mineLevel);
				}
				if (mineRandom.NextDouble() < 0.15)
				{
					return new GreenSlime(position, getMineArea());
				}
				if (mineRandom.NextDouble() < 0.15)
				{
					return new MetalHead(position, getMineArea());
				}
				if (mineRandom.NextDouble() < 0.25)
				{
					return new ShadowBrute(position);
				}
				if (mineRandom.NextDouble() < 0.25)
				{
					return new ShadowShaman(position);
				}
				if (mineRandom.NextDouble() < 0.25)
				{
					return new RockCrab(position, "Lava Crab");
				}
				if (mineRandom.NextDouble() < 0.2 && distanceFromLadder > 8f && mineLevel >= 90)
				{
					return new SquidKid(position);
				}
			}
			else
			{
				if (getMineArea() == 121)
				{
					if (loadedDarkArea)
					{
						if (mineRandom.NextDouble() < 0.18 && distanceFromLadder > 8f)
						{
							return new Ghost(position, "Carbon Ghost");
						}
						return new Mummy(position);
					}
					if (mineLevel % 20 == 0 && distanceFromLadder > 10f)
					{
						return new Bat(position, mineLevel);
					}
					if (mineLevel % 16 == 0 && !mustKillAllMonstersToAdvance())
					{
						return new Bug(position, mineRandom.Next(4), this);
					}
					if (mineRandom.NextDouble() < 0.33 && distanceFromLadder > 10f)
					{
						return new Serpent(position);
					}
					if (mineRandom.NextDouble() < 0.33 && distanceFromLadder > 10f && mineLevel >= 171)
					{
						return new Bat(position, mineLevel);
					}
					if (mineLevel >= 126 && distanceFromLadder > 10f && mineRandom.NextDouble() < 0.04 && !mustKillAllMonstersToAdvance())
					{
						return new DinoMonster(position);
					}
					if (mineRandom.NextDouble() < 0.33 && !mustKillAllMonstersToAdvance())
					{
						return new Bug(position, mineRandom.Next(4), this);
					}
					if (mineRandom.NextDouble() < 0.25)
					{
						return new GreenSlime(position, level);
					}
					if (mineLevel >= 146 && mineRandom.NextDouble() < 0.25)
					{
						return new RockCrab(position, "Iridium Crab");
					}
					return new BigSlime(position, this);
				}
				if (getMineArea() == 77377)
				{
					if ((mineLevel == 77377 && yTile > 59) || (mineLevel != 77377 && mineLevel % 2 == 0))
					{
						GreenSlime slime = new GreenSlime(position, 77377);
						Vector2 tile = new Vector2(xTile, yTile);
						bool brown = false;
						for (int i = 0; i < brownSpots.Count; i++)
						{
							if (Vector2.Distance(tile, brownSpots[i]) < 4f)
							{
								brown = true;
								break;
							}
						}
						if (brown)
						{
							int red = Game1.random.Next(120, 200);
							slime.color.Value = new Color(red, red / 2, red / 4);
							while (Game1.random.NextDouble() < 0.33)
							{
								slime.objectsToDrop.Add(378);
							}
							slime.Health = (int)((float)slime.Health * 0.5f);
							slime.Speed += 2;
						}
						else
						{
							int colorBase = Game1.random.Next(120, 200);
							slime.color.Value = new Color(colorBase, colorBase, colorBase);
							while (Game1.random.NextDouble() < 0.33)
							{
								slime.objectsToDrop.Add(380);
							}
							slime.Speed = 1;
						}
						return slime;
					}
					if (yTile < 51 || mineLevel != 77377)
					{
						return new Bat(position, 77377);
					}
					return new Bat(position, 77377)
					{
						focusedOnFarmers = true
					};
				}
			}
			return new GreenSlime(position, level);
		}

		public Color getCrystalColorForThisLevel()
		{
			Random levelRandom = new Random(mineLevel + Game1.player.timesReachedMineBottom);
			if (levelRandom.NextDouble() < 0.04 && mineLevel < 80)
			{
				Color c = new Color(mineRandom.Next(256), mineRandom.Next(256), mineRandom.Next(256));
				while (c.R + c.G + c.B < 500)
				{
					c.R = (byte)Math.Min(255, c.R + 10);
					c.G = (byte)Math.Min(255, c.G + 10);
					c.B = (byte)Math.Min(255, c.B + 10);
				}
				return c;
			}
			if (levelRandom.NextDouble() < 0.07)
			{
				return new Color(255 - mineRandom.Next(20), 255 - mineRandom.Next(20), 255 - mineRandom.Next(20));
			}
			if (mineLevel < 40)
			{
				switch (mineRandom.Next(2))
				{
				case 0:
					return new Color(58, 145, 72);
				case 1:
					return new Color(255, 255, 255);
				}
			}
			else if (mineLevel < 80)
			{
				switch (mineRandom.Next(4))
				{
				case 0:
					return new Color(120, 0, 210);
				case 1:
					return new Color(0, 100, 170);
				case 2:
					return new Color(0, 220, 255);
				case 3:
					return new Color(0, 255, 220);
				}
			}
			else
			{
				switch (mineRandom.Next(2))
				{
				case 0:
					return new Color(200, 100, 0);
				case 1:
					return new Color(220, 60, 0);
				}
			}
			return Color.White;
		}

		private Object chooseStoneType(double chanceForPurpleStone, double chanceForMysticStone, double gemStoneChance, Vector2 tile)
		{
			int whichStone4 = 32;
			int stoneHealth2 = 1;
			if (getMineArea() == 0 || getMineArea() == 10)
			{
				whichStone4 = mineRandom.Next(31, 42);
				if (mineLevel % 40 < 30 && whichStone4 >= 33 && whichStone4 < 38)
				{
					whichStone4 = ((mineRandom.NextDouble() < 0.5) ? 32 : 38);
				}
				else if (mineLevel % 40 >= 30)
				{
					whichStone4 = ((mineRandom.NextDouble() < 0.5) ? 34 : 36);
				}
				if (mineLevel != 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
				{
					return new Object(tile, 751, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 3
					};
				}
			}
			else if (getMineArea() == 40)
			{
				whichStone4 = mineRandom.Next(47, 54);
				stoneHealth2 = 3;
				if (mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
				{
					return new Object(tile, 290, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 4
					};
				}
			}
			else if (getMineArea() == 80)
			{
				stoneHealth2 = 4;
				whichStone4 = ((mineRandom.NextDouble() < 0.3 && !isDarkArea()) ? ((!(mineRandom.NextDouble() < 0.5)) ? 32 : 38) : ((mineRandom.NextDouble() < 0.3) ? mineRandom.Next(55, 58) : ((!(mineRandom.NextDouble() < 0.5)) ? 762 : 760)));
				if (mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
				{
					return new Object(tile, 764, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 8
					};
				}
			}
			else
			{
				if (getMineArea() == 77377)
				{
					stoneHealth2 = 5;
					bool foundSomething = false;
					foreach (Vector2 v in Utility.getAdjacentTileLocations(tile))
					{
						if (objects.ContainsKey(v))
						{
							foundSomething = true;
							break;
						}
					}
					if (!foundSomething && mineRandom.NextDouble() < 0.45)
					{
						return null;
					}
					bool brownSpot = false;
					for (int i = 0; i < brownSpots.Count; i++)
					{
						if (Vector2.Distance(tile, brownSpots[i]) < 4f)
						{
							brownSpot = true;
							break;
						}
						if (Vector2.Distance(tile, brownSpots[i]) < 6f)
						{
							return null;
						}
					}
					if (brownSpot)
					{
						whichStone4 = ((mineRandom.NextDouble() < 0.5) ? 32 : 38);
						if (mineRandom.NextDouble() < 0.01)
						{
							return new Object(tile, 751, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
							{
								MinutesUntilReady = 3
							};
						}
					}
					else
					{
						whichStone4 = ((mineRandom.NextDouble() < 0.5) ? 34 : 36);
						if (mineRandom.NextDouble() < 0.01)
						{
							return new Object(tile, 290, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
							{
								MinutesUntilReady = 3
							};
						}
					}
					return new Object(tile, whichStone4, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = stoneHealth2
					};
				}
				stoneHealth2 = 5;
				whichStone4 = ((mineRandom.NextDouble() < 0.5) ? ((!(mineRandom.NextDouble() < 0.5)) ? 32 : 38) : ((!(mineRandom.NextDouble() < 0.5)) ? 42 : 40));
				int skullCavernMineLevel = mineLevel - 120;
				double chanceForOre = 0.02 + (double)skullCavernMineLevel * 0.0005;
				if (mineLevel >= 130)
				{
					chanceForOre += 0.01 * (double)((float)(Math.Min(100, skullCavernMineLevel) - 10) / 10f);
				}
				double iridiumBoost2 = 0.0;
				if (mineLevel >= 130)
				{
					iridiumBoost2 += 0.001 * (double)((float)(skullCavernMineLevel - 10) / 10f);
				}
				iridiumBoost2 = Math.Min(iridiumBoost2, 0.004);
				if (skullCavernMineLevel > 100)
				{
					iridiumBoost2 += (double)skullCavernMineLevel / 1000000.0;
				}
				if (!netIsTreasureRoom.Value && mineRandom.NextDouble() < chanceForOre)
				{
					double chanceForIridium = (double)Math.Min(100, skullCavernMineLevel) * (0.0003 + iridiumBoost2);
					double chanceForGold = 0.01 + (double)(mineLevel - Math.Min(150, skullCavernMineLevel)) * 0.0005;
					double chanceForIron = Math.Min(0.5, 0.1 + (double)(mineLevel - Math.Min(200, skullCavernMineLevel)) * 0.005);
					if (mineRandom.NextDouble() < chanceForIridium)
					{
						return new Object(tile, 765, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 16
						};
					}
					if (mineRandom.NextDouble() < chanceForGold)
					{
						return new Object(tile, 764, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 8
						};
					}
					if (mineRandom.NextDouble() < chanceForIron)
					{
						return new Object(tile, 290, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 4
						};
					}
					return new Object(tile, 751, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 2
					};
				}
			}
			double averageDailyLuck = Game1.player.team.AverageDailyLuck(Game1.currentLocation);
			double averageMiningLevel = Game1.player.team.AverageSkillLevel(3, Game1.currentLocation);
			double chanceModifier = averageDailyLuck + averageMiningLevel * 0.005;
			if (mineLevel > 50 && mineRandom.NextDouble() < 0.00025 + (double)mineLevel / 120000.0 + 0.0005 * chanceModifier / 2.0)
			{
				whichStone4 = 2;
				stoneHealth2 = 10;
			}
			else if (gemStoneChance != 0.0 && mineRandom.NextDouble() < gemStoneChance + gemStoneChance * chanceModifier + (double)mineLevel / 24000.0)
			{
				return new Object(tile, getRandomGemRichStoneForThisLevel(mineLevel), "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
				{
					MinutesUntilReady = 5
				};
			}
			if (mineRandom.NextDouble() < chanceForPurpleStone / 2.0 + chanceForPurpleStone * averageMiningLevel * 0.008 + chanceForPurpleStone * (averageDailyLuck / 2.0))
			{
				whichStone4 = 44;
			}
			if (mineLevel > 100 && mineRandom.NextDouble() < chanceForMysticStone + chanceForMysticStone * averageMiningLevel * 0.008 + chanceForMysticStone * (averageDailyLuck / 2.0))
			{
				whichStone4 = 46;
			}
			whichStone4 += whichStone4 % 2;
			if (mineRandom.NextDouble() < 0.1 && getMineArea() != 40)
			{
				return new Object(tile, (mineRandom.NextDouble() < 0.5) ? 668 : 670, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
				{
					MinutesUntilReady = 2,
					Flipped = (mineRandom.NextDouble() < 0.5)
				};
			}
			return new Object(tile, whichStone4, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
			{
				MinutesUntilReady = stoneHealth2
			};
		}

		public override void draw(SpriteBatch b)
		{
			foreach (ResourceClump r in resourceClumps)
			{
				r.draw(b, r.tile);
			}
			base.draw(b);
		}

		public static void OnLeftMines()
		{
			if (!Game1.IsClient && !Game1.IsMultiplayer)
			{
				clearInactiveMines(keepUntickedLevels: false);
			}
		}

		public static void clearActiveMines()
		{
			activeMines.RemoveAll(delegate(MineShaft mine)
			{
				mine.mapContent.Dispose();
				return true;
			});
		}

		private static void clearInactiveMines(bool keepUntickedLevels = true)
		{
			int maxMineLevel = -1;
			int maxSkullLevel = -1;
			foreach (MineShaft mine2 in activeMines)
			{
				if (mine2.farmers.Count() > 0)
				{
					if (mine2.mineLevel > 120)
					{
						if (mine2.mineLevel < 77377)
						{
							maxSkullLevel = Math.Max(maxSkullLevel, mine2.mineLevel);
						}
					}
					else
					{
						maxMineLevel = Math.Max(maxMineLevel, mine2.mineLevel);
					}
				}
			}
			activeMines.RemoveAll(delegate(MineShaft mine)
			{
				if (mine.mineLevel == 77377)
				{
					return false;
				}
				if (mine.mineLevel > 120)
				{
					if (mine.mineLevel <= maxSkullLevel)
					{
						return false;
					}
				}
				else if (mine.mineLevel <= maxMineLevel)
				{
					return false;
				}
				if ((mine.lifespan == 0) & keepUntickedLevels)
				{
					return false;
				}
				mine.mapContent.Dispose();
				return true;
			});
		}

		public static void UpdateMines10Minutes(int timeOfDay)
		{
			clearInactiveMines();
			if (!Game1.IsClient)
			{
				foreach (MineShaft mine in activeMines)
				{
					if (mine.farmers.Count() > 0)
					{
						mine.performTenMinuteUpdate(timeOfDay);
					}
					mine.lifespan++;
				}
			}
		}

		protected override void updateCharacters(GameTime time)
		{
			if (farmers.Count() != 0)
			{
				base.updateCharacters(time);
			}
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			if (!Game1.shouldTimePass() || !isFogUp)
			{
				return;
			}
			int oldTime = fogTime;
			fogTime -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (Game1.IsMasterGame)
			{
				if (fogTime > 5000 && oldTime % 4000 < fogTime % 4000)
				{
					spawnFlyingMonsterOffScreen();
				}
				if (fogTime <= 0)
				{
					isFogUp.Value = false;
				}
			}
		}

		public static void UpdateMines(GameTime time)
		{
			foreach (MineShaft mine in activeMines)
			{
				if (mine.farmers.Count() > 0)
				{
					mine.UpdateWhenCurrentLocation(time);
				}
				mine.updateEvenIfFarmerIsntHere(time);
			}
		}

		public static MineShaft GetMine(string name)
		{
			foreach (MineShaft mine in activeMines)
			{
				if (mine.Name.Equals(name))
				{
					return mine;
				}
			}
			MineShaft newMine = new MineShaft(Convert.ToInt32(name.Substring("UndergroundMine".Length)));
			activeMines.Add(newMine);
			newMine.generateContents();
			return newMine;
		}

		public static void ForEach(Action<MineShaft> action)
		{
			foreach (MineShaft mine in activeMines)
			{
				action(mine);
			}
		}
	}
}
