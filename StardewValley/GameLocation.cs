using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	[XmlInclude(typeof(Farm))]
	[XmlInclude(typeof(Beach))]
	[XmlInclude(typeof(AnimalHouse))]
	[XmlInclude(typeof(SlimeHutch))]
	[XmlInclude(typeof(Shed))]
	[XmlInclude(typeof(LibraryMuseum))]
	[XmlInclude(typeof(AdventureGuild))]
	[XmlInclude(typeof(Woods))]
	[XmlInclude(typeof(Railroad))]
	[XmlInclude(typeof(Summit))]
	[XmlInclude(typeof(Forest))]
	[XmlInclude(typeof(ShopLocation))]
	[XmlInclude(typeof(SeedShop))]
	[XmlInclude(typeof(FishShop))]
	[XmlInclude(typeof(BathHousePool))]
	[XmlInclude(typeof(FarmHouse))]
	[XmlInclude(typeof(Cabin))]
	[XmlInclude(typeof(Club))]
	[XmlInclude(typeof(BusStop))]
	[XmlInclude(typeof(CommunityCenter))]
	[XmlInclude(typeof(Desert))]
	[XmlInclude(typeof(FarmCave))]
	[XmlInclude(typeof(JojaMart))]
	[XmlInclude(typeof(MineShaft))]
	[XmlInclude(typeof(Mountain))]
	[XmlInclude(typeof(Sewer))]
	[XmlInclude(typeof(WizardHouse))]
	[XmlInclude(typeof(Town))]
	[XmlInclude(typeof(Cellar))]
	[XmlInclude(typeof(Submarine))]
	[XmlInclude(typeof(MermaidHouse))]
	[XmlInclude(typeof(BeachNightMarket))]
	[XmlInclude(typeof(MovieTheater))]
	[XmlInclude(typeof(ManorHouse))]
	[XmlInclude(typeof(AbandonedJojaMart))]
	[XmlInclude(typeof(Mine))]
	[XmlInclude(typeof(BoatTunnel))]
	[XmlInclude(typeof(IslandWest))]
	[XmlInclude(typeof(IslandEast))]
	[XmlInclude(typeof(IslandSouth))]
	[XmlInclude(typeof(IslandNorth))]
	[XmlInclude(typeof(IslandSouthEast))]
	[XmlInclude(typeof(IslandSouthEastCave))]
	[XmlInclude(typeof(IslandFarmCave))]
	[XmlInclude(typeof(IslandWestCave1))]
	[XmlInclude(typeof(IslandFieldOffice))]
	[XmlInclude(typeof(IslandHut))]
	[XmlInclude(typeof(IslandFarmHouse))]
	[XmlInclude(typeof(IslandSecret))]
	[XmlInclude(typeof(IslandShrine))]
	[XmlInclude(typeof(IslandLocation))]
	[XmlInclude(typeof(IslandForestLocation))]
	[XmlInclude(typeof(Caldera))]
	[XmlInclude(typeof(BugLand))]
	[InstanceStatics]
	public class GameLocation : INetObject<NetFields>, IEquatable<GameLocation>
	{
		public enum LocationContext
		{
			Default,
			Island,
			MAX
		}

		public delegate void afterQuestionBehavior(Farmer who, string whichAnswer);

		private struct DamagePlayersEventArg : NetEventArg
		{
			public Microsoft.Xna.Framework.Rectangle Area;

			public int Damage;

			public void Read(BinaryReader reader)
			{
				Area = reader.ReadRectangle();
				Damage = reader.ReadInt32();
			}

			public void Write(BinaryWriter writer)
			{
				writer.WriteRectangle(Area);
				writer.Write(Damage);
			}
		}

		public const int minDailyWeeds = 5;

		public const int maxDailyWeeds = 12;

		public const int minDailyObjectSpawn = 1;

		public const int maxDailyObjectSpawn = 4;

		public const int maxSpawnedObjectsAtOnce = 6;

		public const int maxTriesForDebrisPlacement = 3;

		public const int maxTriesForObjectSpawn = 6;

		public const double chanceForStumpOrBoulderRespawn = 0.2;

		public const double chanceForClay = 0.03;

		public const string OVERRIDE_MAP_TILESHEET_PREFIX = "zzzzz";

		public const string PHONE_DIAL_SOUND = "telephone_buttonPush";

		public const int PHONE_RING_DURATION = 4950;

		public const string PHONE_PICKUP_SOUND = "bigSelect";

		public const string PHONE_HANGUP_SOUND = "openBox";

		public const int forageDataIndex = 0;

		public const int fishDataIndex = 4;

		public const int diggablesDataIndex = 8;

		[XmlIgnore]
		public string seasonOverride;

		[XmlIgnore]
		public LocationContext locationContext = (LocationContext)(-1);

		[XmlIgnore]
		public afterQuestionBehavior afterQuestion;

		[XmlIgnore]
		public Map map;

		[XmlIgnore]
		public readonly NetString mapPath = new NetString().Interpolated(interpolate: false, wait: false);

		[XmlIgnore]
		protected string loadedMapPath;

		public readonly NetCollection<NPC> characters = new NetCollection<NPC>();

		[XmlIgnore]
		public readonly NetVector2Dictionary<Object, NetRef<Object>> netObjects = new NetVector2Dictionary<Object, NetRef<Object>>();

		[XmlIgnore]
		public readonly Dictionary<Vector2, Object> overlayObjects = new Dictionary<Vector2, Object>(tilePositionComparer);

		[XmlElement("objects")]
		public readonly OverlaidDictionary objects;

		private readonly List<Object> tempObjects = new List<Object>();

		[XmlIgnore]
		public NetList<MapSeat, NetRef<MapSeat>> mapSeats = new NetList<MapSeat, NetRef<MapSeat>>();

		[XmlIgnore]
		private List<KeyValuePair<Vector2, Object>> _objectUpdateList = new List<KeyValuePair<Vector2, Object>>();

		[XmlIgnore]
		public List<TemporaryAnimatedSprite> temporarySprites = new List<TemporaryAnimatedSprite>();

		[XmlIgnore]
		public List<Action> postFarmEventOvernightActions = new List<Action>();

		[XmlIgnore]
		public readonly NetObjectList<Warp> warps = new NetObjectList<Warp>();

		[XmlIgnore]
		public readonly NetPointDictionary<string, NetString> doors = new NetPointDictionary<string, NetString>();

		[XmlIgnore]
		public readonly InteriorDoorDictionary interiorDoors;

		[XmlIgnore]
		public readonly FarmerCollection farmers;

		[XmlIgnore]
		public readonly NetCollection<Projectile> projectiles = new NetCollection<Projectile>();

		public readonly NetCollection<ResourceClump> resourceClumps = new NetCollection<ResourceClump>();

		public readonly NetCollection<LargeTerrainFeature> largeTerrainFeatures = new NetCollection<LargeTerrainFeature>();

		[XmlIgnore]
		public List<TerrainFeature> _activeTerrainFeatures = new List<TerrainFeature>();

		[XmlIgnore]
		public List<Critter> critters;

		[XmlElement("terrainFeatures")]
		public readonly NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = new NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>();

		[XmlIgnore]
		public readonly NetCollection<Debris> debris = new NetCollection<Debris>();

		[XmlIgnore]
		public readonly NetPoint fishSplashPoint = new NetPoint(Point.Zero);

		[XmlIgnore]
		public readonly NetPoint orePanPoint = new NetPoint(Point.Zero);

		[XmlIgnore]
		public TemporaryAnimatedSprite fishSplashAnimation;

		[XmlIgnore]
		public TemporaryAnimatedSprite orePanAnimation;

		[XmlIgnore]
		public bool[,] waterTiles;

		[XmlIgnore]
		protected HashSet<string> _appliedMapOverrides;

		[XmlElement("uniqueName")]
		public readonly NetString uniqueName = new NetString();

		[XmlElement("name")]
		public readonly NetString name = new NetString();

		[XmlElement("waterColor")]
		public readonly NetColor waterColor = new NetColor(Color.White * 0.33f);

		[XmlIgnore]
		public string lastQuestionKey;

		[XmlIgnore]
		public Vector2 lastTouchActionLocation = Vector2.Zero;

		[XmlElement("lightLevel")]
		protected readonly NetFloat lightLevel = new NetFloat(0f);

		[XmlElement("isFarm")]
		public readonly NetBool isFarm = new NetBool();

		[XmlElement("isOutdoors")]
		public readonly NetBool isOutdoors = new NetBool();

		[XmlIgnore]
		public readonly NetBool isGreenhouse = new NetBool();

		[XmlElement("isStructure")]
		public readonly NetBool isStructure = new NetBool();

		[XmlElement("ignoreDebrisWeather")]
		public readonly NetBool ignoreDebrisWeather = new NetBool();

		[XmlElement("ignoreOutdoorLighting")]
		public readonly NetBool ignoreOutdoorLighting = new NetBool();

		[XmlElement("ignoreLights")]
		public readonly NetBool ignoreLights = new NetBool();

		[XmlElement("treatAsOutdoors")]
		public readonly NetBool treatAsOutdoors = new NetBool();

		[XmlIgnore]
		public bool wasUpdated;

		private List<Vector2> terrainFeaturesToRemoveList = new List<Vector2>();

		public int numberOfSpawnedObjectsOnMap;

		[XmlIgnore]
		public bool showDropboxIndicator;

		[XmlIgnore]
		public Vector2 dropBoxIndicatorLocation;

		[XmlElement("miniJukeboxCount")]
		public readonly NetInt miniJukeboxCount = new NetInt();

		[XmlElement("miniJukeboxTrack")]
		public readonly NetString miniJukeboxTrack = new NetString("");

		[XmlIgnore]
		public readonly NetString randomMiniJukeboxTrack = new NetString();

		[XmlIgnore]
		public Event currentEvent;

		[XmlIgnore]
		public Object actionObjectForQuestionDialogue;

		[XmlIgnore]
		public int waterAnimationIndex;

		[XmlIgnore]
		public int waterAnimationTimer;

		[XmlIgnore]
		public bool waterTileFlip;

		[XmlIgnore]
		public bool forceViewportPlayerFollow;

		[XmlIgnore]
		public bool forceLoadPathLayerLights;

		[XmlIgnore]
		public float waterPosition;

		[XmlIgnore]
		public readonly NetAudio netAudio;

		[XmlIgnore]
		public readonly NetIntDictionary<LightSource, NetRef<LightSource>> sharedLights = new NetIntDictionary<LightSource, NetRef<LightSource>>();

		private readonly NetEvent1Field<float, NetFloat> removeTemporarySpritesWithIDEvent = new NetEvent1Field<float, NetFloat>();

		private readonly NetEvent1Field<int, NetInt> rumbleAndFadeEvent = new NetEvent1Field<int, NetInt>();

		private readonly NetEvent1<DamagePlayersEventArg> damagePlayersEvent = new NetEvent1<DamagePlayersEventArg>();

		[XmlIgnore]
		public NetList<Vector2, NetVector2> lightGlows = new NetList<Vector2, NetVector2>();

		public static readonly int JOURNAL_INDEX = 1000;

		public static readonly float FIRST_SECRET_NOTE_CHANCE = 0.8f;

		public static readonly float LAST_SECRET_NOTE_CHANCE = 0.12f;

		public static readonly int NECKLACE_SECRET_NOTE_INDEX = 25;

		public static readonly int CAROLINES_NECKLACE_ITEM = 191;

		public static readonly string CAROLINES_NECKLACE_MAIL = "carolinesNecklace";

		public static TilePositionComparer tilePositionComparer = new TilePositionComparer();

		[XmlIgnore]
		public bool wasInhabited;

		[XmlIgnore]
		protected bool _madeMapModifications;

		[XmlIgnore]
		public ModDataDictionary modData = new ModDataDictionary();

		public readonly NetCollection<Furniture> furniture = new NetCollection<Furniture>();

		protected readonly NetMutexQueue<Guid> furnitureToRemove = new NetMutexQueue<Guid>();

		protected bool ignoreWarps;

		private Vector2 snowPos;

		private const int fireIDBase = 944468;

		private static HashSet<int> secretNotesSeen = new HashSet<int>();

		private static List<int> unseenSecretNotes = new List<int>();

		private static HashSet<int> journalsSeen = new HashSet<int>();

		private static List<int> unseenJournals = new List<int>();

		private static readonly char[] ForwardSlash = new char[1]
		{
			'/'
		};

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		[XmlIgnore]
		public NetRoot<GameLocation> Root => NetFields.Root as NetRoot<GameLocation>;

		[XmlIgnore]
		public string NameOrUniqueName
		{
			get
			{
				if (uniqueName.Value != null)
				{
					return uniqueName.Value;
				}
				return name.Value;
			}
		}

		[XmlIgnore]
		public float LightLevel
		{
			get
			{
				return lightLevel;
			}
			set
			{
				lightLevel.Value = value;
			}
		}

		[XmlIgnore]
		public Map Map
		{
			get
			{
				updateMap();
				return map;
			}
			set
			{
				map = value;
			}
		}

		[XmlIgnore]
		public OverlaidDictionary Objects => objects;

		[XmlIgnore]
		public List<TemporaryAnimatedSprite> TemporarySprites => temporarySprites;

		public string Name => name;

		[XmlIgnore]
		public bool IsFarm
		{
			get
			{
				return isFarm;
			}
			set
			{
				isFarm.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsOutdoors
		{
			get
			{
				return isOutdoors;
			}
			set
			{
				isOutdoors.Value = value;
			}
		}

		public bool IsGreenhouse
		{
			get
			{
				return isGreenhouse;
			}
			set
			{
				isGreenhouse.Value = value;
			}
		}

		[XmlElement("modData")]
		public ModDataDictionary modDataForSerialization
		{
			get
			{
				return modData.GetForSerialization();
			}
			set
			{
				modData.SetFromSerialization(value);
			}
		}

		public virtual bool SeedsIgnoreSeasonsHere()
		{
			return IsGreenhouse;
		}

		public virtual bool CanPlantSeedsHere(int crop_index, int tile_x, int tile_y)
		{
			return IsGreenhouse;
		}

		public virtual bool CanPlantTreesHere(int sapling_index, int tile_x, int tile_y)
		{
			if (Object.isWildTreeSeed(sapling_index) && IsOutdoors && doesTileHavePropertyNoNull(tile_x, tile_y, "Type", "Back") == "Dirt")
			{
				return true;
			}
			return IsGreenhouse;
		}

		protected virtual void initNetFields()
		{
			NetFields.AddFields(mapPath, uniqueName, name, lightLevel, sharedLights, isFarm, isOutdoors, isStructure, ignoreDebrisWeather, ignoreOutdoorLighting, ignoreLights, treatAsOutdoors, warps, doors, interiorDoors, waterColor, netObjects, projectiles, largeTerrainFeatures, terrainFeatures, characters, debris, netAudio.NetFields, removeTemporarySpritesWithIDEvent, rumbleAndFadeEvent, damagePlayersEvent, lightGlows, fishSplashPoint, orePanPoint, isGreenhouse, miniJukeboxCount, miniJukeboxTrack, randomMiniJukeboxTrack, resourceClumps);
			NetFields.AddFields(furniture, furnitureToRemove.NetFields);
			NetFields.AddField(mapSeats);
			NetFields.AddField(modData);
			sharedLights.OnValueAdded += delegate(int identifier, LightSource light)
			{
				if (Game1.currentLocation == this)
				{
					Game1.currentLightSources.Add(light);
				}
			};
			sharedLights.OnValueRemoved += delegate(int identifier, LightSource light)
			{
				if (Game1.currentLocation == this)
				{
					Game1.currentLightSources.Remove(light);
				}
			};
			netObjects.OnConflictResolve += delegate(Vector2 pos, NetRef<Object> rejected, NetRef<Object> accepted)
			{
				if (Game1.IsMasterGame)
				{
					Object value = rejected.Value;
					if (value != null)
					{
						value.NetFields.Parent = null;
						value.dropItem(this, pos * 64f, pos * 64f);
					}
				}
			};
			removeTemporarySpritesWithIDEvent.onEvent += removeTemporarySpritesWithIDLocal;
			rumbleAndFadeEvent.onEvent += performRumbleAndFade;
			damagePlayersEvent.onEvent += performDamagePlayers;
			fishSplashPoint.fieldChangeVisibleEvent += delegate
			{
				updateFishSplashAnimation();
			};
			orePanPoint.fieldChangeVisibleEvent += delegate
			{
				updateOrePanAnimation();
			};
			characters.OnValueRemoved += delegate(NPC npc)
			{
				npc.Removed();
			};
			terrainFeatures.OnValueAdded += delegate(Vector2 pos, TerrainFeature tf)
			{
				if (tf is Flooring)
				{
					(tf as Flooring).OnAdded(this, pos);
				}
				else if (tf is HoeDirt)
				{
					(tf as HoeDirt).OnAdded(this, pos);
				}
				OnTerrainFeatureAdded(tf, pos);
			};
			terrainFeatures.OnValueRemoved += delegate(Vector2 pos, TerrainFeature tf)
			{
				if (tf is Flooring)
				{
					(tf as Flooring).OnRemoved(this, pos);
				}
				else if (tf is HoeDirt)
				{
					(tf as HoeDirt).OnRemoved(this, pos);
				}
				OnTerrainFeatureRemoved(tf);
			};
			largeTerrainFeatures.OnValueAdded += delegate(LargeTerrainFeature tf)
			{
				OnTerrainFeatureAdded(tf, tf.currentTileLocation);
			};
			largeTerrainFeatures.OnValueRemoved += delegate(LargeTerrainFeature tf)
			{
				OnTerrainFeatureRemoved(tf);
			};
			furniture.InterpolationWait = false;
			furniture.OnValueAdded += delegate(Furniture f)
			{
				f.OnAdded(this, f.TileLocation);
			};
			furnitureToRemove.Processor = removeQueuedFurniture;
		}

		public virtual void InvalidateCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data)
		{
			if (!Game1.IsMasterGame && cached_data.ContainsKey(NameOrUniqueName))
			{
				cached_data.Remove(NameOrUniqueName);
			}
		}

		public virtual void MakeMapModifications(bool force = false)
		{
			if (force)
			{
				_appliedMapOverrides.Clear();
			}
			interiorDoors.MakeMapModifications();
			switch ((string)name)
			{
			case "Sunroom":
			{
				string imageSource = map.TileSheets[1].ImageSource;
				Path.GetFileName(imageSource);
				string imageDir = Path.GetDirectoryName(imageSource);
				if (string.IsNullOrWhiteSpace(imageDir))
				{
					imageDir = "Maps";
				}
				map.TileSheets[1].ImageSource = Path.Combine(imageDir, "CarolineGreenhouseTiles" + ((Game1.IsRainingHere(this) || Game1.timeOfDay > Game1.getTrulyDarkTime()) ? "_rainy" : ""));
				map.DisposeTileSheets(Game1.mapDisplayDevice);
				map.LoadTileSheets(Game1.mapDisplayDevice);
				break;
			}
			case "AbandonedJojaMart":
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					StaticTile[] tileFrames = CommunityCenter.getJunimoNoteTileFrames(0, map);
					string layer = "Buildings";
					Point position = new Point(8, 8);
					map.GetLayer(layer).Tiles[position.X, position.Y] = new AnimatedTile(map.GetLayer(layer), tileFrames, 70L);
				}
				else
				{
					removeTile(8, 8, "Buildings");
				}
				break;
			case "WitchSwamp":
				if (Game1.MasterPlayer.mailReceived.Contains("henchmanGone"))
				{
					removeTile(20, 29, "Buildings");
				}
				else
				{
					setMapTileIndex(20, 29, 10, "Buildings");
				}
				break;
			case "WitchHut":
				if (Game1.player.mailReceived.Contains("hasPickedUpMagicInk"))
				{
					setMapTileIndex(4, 11, 113, "Buildings");
					map.GetLayer("Buildings").Tiles[4, 11].Properties.Remove("Action");
				}
				break;
			case "HaleyHouse":
				if (Game1.player.eventsSeen.Contains(463391) && (Game1.player.spouse == null || !Game1.player.spouse.Equals("Emily")))
				{
					setMapTileIndex(14, 4, 2173, "Buildings");
					setMapTileIndex(14, 3, 2141, "Buildings");
					setMapTileIndex(14, 3, 219, "Back");
				}
				break;
			case "Saloon":
				if (NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom"))
				{
					refurbishMapPortion(new Microsoft.Xna.Framework.Rectangle(32, 1, 7, 9), "RefurbishedSaloonRoom", Point.Zero);
					Game1.currentLightSources.Add(new LightSource(1, new Vector2(33f, 7f) * 64f, 4f, LightSource.LightContext.None, 0L));
					Game1.currentLightSources.Add(new LightSource(1, new Vector2(36f, 7f) * 64f, 4f, LightSource.LightContext.None, 0L));
					Game1.currentLightSources.Add(new LightSource(1, new Vector2(34f, 5f) * 64f, 4f, LightSource.LightContext.None, 0L));
				}
				break;
			case "Backwoods":
				if (Game1.netWorldState.Value.hasWorldStateID("golemGrave"))
				{
					ApplyMapOverride("Backwoods_GraveSite");
				}
				if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts") && !_appliedMapOverrides.Contains("Backwoods_Staircase"))
				{
					ApplyMapOverride("Backwoods_Staircase");
					LargeTerrainFeature blockingBush = null;
					foreach (LargeTerrainFeature t in largeTerrainFeatures)
					{
						if (t.tilePosition.Equals(new Vector2(37f, 16f)))
						{
							blockingBush = t;
							break;
						}
					}
					if (blockingBush != null)
					{
						largeTerrainFeatures.Remove(blockingBush);
					}
				}
				break;
			}
		}

		public virtual bool ApplyCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data, string requested_map_path)
		{
			if (Game1.IsMasterGame)
			{
				return false;
			}
			if (cached_data.ContainsKey(NameOrUniqueName))
			{
				CachedMultiplayerMap data = cached_data[NameOrUniqueName];
				if (data.mapPath == requested_map_path)
				{
					_appliedMapOverrides = data.appliedMapOverrides;
					map = data.map;
					loadedMapPath = data.loadedMapPath;
					return true;
				}
				cached_data.Remove(NameOrUniqueName);
				return false;
			}
			return false;
		}

		public virtual void StoreCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data)
		{
			if (!Game1.IsMasterGame && !(this is VolcanoDungeon) && !(this is MineShaft))
			{
				CachedMultiplayerMap data = new CachedMultiplayerMap();
				data.map = map;
				data.appliedMapOverrides = _appliedMapOverrides;
				data.mapPath = mapPath;
				data.loadedMapPath = loadedMapPath;
				cached_data[NameOrUniqueName] = data;
			}
		}

		public virtual void TransferDataFromSavedLocation(GameLocation l)
		{
			modData.Clear();
			if (l.modData != null)
			{
				foreach (string key in l.modData.Keys)
				{
					modData[key] = l.modData[key];
				}
			}
			SelectRandomMiniJukeboxTrack();
		}

		public virtual void OnTerrainFeatureAdded(TerrainFeature feature, Vector2 location)
		{
			if (feature != null)
			{
				feature.currentLocation = this;
				feature.currentTileLocation = location;
				feature.OnAddedToLocation(this, location);
				UpdateTerrainFeatureUpdateSubscription(feature);
			}
		}

		public virtual void OnTerrainFeatureRemoved(TerrainFeature feature)
		{
			if (feature != null)
			{
				if (feature.NeedsUpdate)
				{
					_activeTerrainFeatures.Remove(feature);
				}
				feature.currentLocation = null;
			}
		}

		public virtual void UpdateTerrainFeatureUpdateSubscription(TerrainFeature feature)
		{
			if (feature.NeedsUpdate)
			{
				_activeTerrainFeatures.Add(feature);
			}
			else
			{
				_activeTerrainFeatures.Remove(feature);
			}
		}

		public string GetSeasonForLocation()
		{
			if (seasonOverride == null)
			{
				if (map == null && mapPath.Value != null)
				{
					reloadMap();
				}
				if (map != null)
				{
					seasonOverride = null;
					PropertyValue value = null;
					if (map.Properties.TryGetValue("SeasonOverride", out value))
					{
						seasonOverride = value.ToString();
					}
					else
					{
						seasonOverride = "";
					}
				}
			}
			if (seasonOverride != null && seasonOverride.Length == 0)
			{
				return Game1.currentSeason;
			}
			return seasonOverride;
		}

		public bool isTemp()
		{
			if (!Name.StartsWith("Temp") && !Name.Equals("fishingGame"))
			{
				return Name.Equals("tent");
			}
			return true;
		}

		private void updateFishSplashAnimation()
		{
			if (fishSplashPoint.Value == Point.Zero)
			{
				fishSplashAnimation = null;
			}
			else
			{
				fishSplashAnimation = new TemporaryAnimatedSprite(51, new Vector2(fishSplashPoint.X * 64, fishSplashPoint.Y * 64), Color.White, 10, flipped: false, 80f, 999999);
			}
		}

		private void updateOrePanAnimation()
		{
			if (orePanPoint.Value == Point.Zero)
			{
				orePanAnimation = null;
			}
			else
			{
				orePanAnimation = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), new Vector2(orePanPoint.X * 64 + 32, orePanPoint.Y * 64 + 32), flipped: false, 0f, Color.White)
				{
					totalNumberOfLoops = 9999999,
					interval = 100f,
					scale = 3f,
					animationLength = 6
				};
			}
		}

		public GameLocation()
		{
			farmers = new FarmerCollection(this);
			interiorDoors = new InteriorDoorDictionary(this);
			netAudio = new NetAudio(this);
			objects = new OverlaidDictionary(netObjects, overlayObjects);
			_appliedMapOverrides = new HashSet<string>();
			terrainFeatures.SetEqualityComparer(tilePositionComparer);
			netObjects.SetEqualityComparer(tilePositionComparer);
			objects.SetEqualityComparer(tilePositionComparer, ref netObjects, ref overlayObjects);
			initNetFields();
		}

		public GameLocation(string mapPath, string name)
			: this()
		{
			this.mapPath.Set(mapPath);
			this.name.Value = name;
			if (name.Contains("Farm") || name.Contains("Coop") || name.Contains("Barn") || name.Equals("SlimeHutch"))
			{
				isFarm.Value = true;
			}
			if (name == "Greenhouse")
			{
				IsGreenhouse = true;
			}
			reloadMap();
			loadObjects();
		}

		public void playSound(string audioName, NetAudio.SoundContext soundContext = NetAudio.SoundContext.Default)
		{
			netAudio.Play(audioName, soundContext);
		}

		public void playSoundPitched(string audioName, int pitch, NetAudio.SoundContext soundContext = NetAudio.SoundContext.Default)
		{
			netAudio.PlayPitched(audioName, Vector2.Zero, pitch, soundContext);
		}

		public void playSoundAt(string audioName, Vector2 position, NetAudio.SoundContext soundContext = NetAudio.SoundContext.Default)
		{
			netAudio.PlayAt(audioName, position, soundContext);
		}

		public void localSound(string audioName)
		{
			netAudio.PlayLocal(audioName);
		}

		public void localSoundAt(string audioName, Vector2 position)
		{
			netAudio.PlayLocalAt(audioName, position);
		}

		private bool doorHasStateOpen(Point door)
		{
			bool isOpen;
			return interiorDoors.TryGetValue(door, out isOpen) && isOpen;
		}

		protected virtual LocalizedContentManager getMapLoader()
		{
			return Game1.game1.xTileContent;
		}

		public void ApplyMapOverride(Map override_map, string override_key, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? dest_rect = null)
		{
			if (_appliedMapOverrides.Contains(override_key))
			{
				return;
			}
			_appliedMapOverrides.Add(override_key);
			updateSeasonalTileSheets(override_map);
			Dictionary<TileSheet, TileSheet> tilesheet_lookup = new Dictionary<TileSheet, TileSheet>();
			foreach (TileSheet override_tile_sheet in override_map.TileSheets)
			{
				TileSheet map_tilesheet = map.GetTileSheet(override_tile_sheet.Id);
				string source_image_source = "";
				string dest_image_source = "";
				if (map_tilesheet != null)
				{
					source_image_source = map_tilesheet.ImageSource;
				}
				if (dest_image_source != null)
				{
					dest_image_source = override_tile_sheet.ImageSource;
				}
				if (map_tilesheet == null || dest_image_source != source_image_source)
				{
					map_tilesheet = new TileSheet("zzzzz_" + override_key + "_" + override_tile_sheet.Id, map, override_tile_sheet.ImageSource, override_tile_sheet.SheetSize, override_tile_sheet.TileSize);
					for (int j = 0; j < override_tile_sheet.TileCount; j++)
					{
						map_tilesheet.TileIndexProperties[j].CopyFrom(override_tile_sheet.TileIndexProperties[j]);
					}
					map.AddTileSheet(map_tilesheet);
				}
				else if (map_tilesheet.TileCount < override_tile_sheet.TileCount)
				{
					int tileCount = map_tilesheet.TileCount;
					map_tilesheet.SheetWidth = override_tile_sheet.SheetWidth;
					map_tilesheet.SheetHeight = override_tile_sheet.SheetHeight;
					for (int k = tileCount; k < override_tile_sheet.TileCount; k++)
					{
						map_tilesheet.TileIndexProperties[k].CopyFrom(override_tile_sheet.TileIndexProperties[k]);
					}
				}
				tilesheet_lookup[override_tile_sheet] = map_tilesheet;
			}
			Dictionary<Layer, Layer> layer_lookup = new Dictionary<Layer, Layer>();
			int map_width2 = 0;
			int map_height2 = 0;
			for (int layer_index3 = 0; layer_index3 < override_map.Layers.Count; layer_index3++)
			{
				layer_lookup[override_map.Layers[layer_index3]] = map.GetLayer(override_map.Layers[layer_index3].Id);
				map_width2 = Math.Max(map_width2, override_map.Layers[layer_index3].LayerWidth);
				map_height2 = Math.Max(map_height2, override_map.Layers[layer_index3].LayerHeight);
			}
			if (!source_rect.HasValue)
			{
				source_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, map_width2, map_height2);
			}
			map_width2 = 0;
			map_height2 = 0;
			for (int layer_index2 = 0; layer_index2 < map.Layers.Count; layer_index2++)
			{
				map_width2 = Math.Max(map_width2, map.Layers[layer_index2].LayerWidth);
				map_height2 = Math.Max(map_height2, map.Layers[layer_index2].LayerHeight);
			}
			if (!dest_rect.HasValue)
			{
				dest_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, map_width2, map_height2);
			}
			int source_rect_x = source_rect.Value.X;
			int source_rect_y = source_rect.Value.Y;
			int dest_rect_x = dest_rect.Value.X;
			int dest_rect_y = dest_rect.Value.Y;
			for (int x = 0; x < source_rect.Value.Width; x++)
			{
				for (int y = 0; y < source_rect.Value.Height; y++)
				{
					Point source_tile_pos = new Point(source_rect_x + x, source_rect_y + y);
					Point dest_tile_pos = new Point(dest_rect_x + x, dest_rect_y + y);
					bool lower_layer_overridden = false;
					for (int layer_index = 0; layer_index < override_map.Layers.Count; layer_index++)
					{
						Layer override_layer = override_map.Layers[layer_index];
						Layer target_layer = layer_lookup[override_layer];
						if (target_layer == null || dest_tile_pos.X >= target_layer.LayerWidth || dest_tile_pos.Y >= target_layer.LayerHeight || (!lower_layer_overridden && override_map.Layers[layer_index].Tiles[source_tile_pos.X, source_tile_pos.Y] == null))
						{
							continue;
						}
						lower_layer_overridden = true;
						if (source_tile_pos.X >= override_layer.LayerWidth || source_tile_pos.Y >= override_layer.LayerHeight)
						{
							continue;
						}
						if (override_layer.Tiles[source_tile_pos.X, source_tile_pos.Y] == null)
						{
							target_layer.Tiles[dest_tile_pos.X, dest_tile_pos.Y] = null;
							continue;
						}
						Tile override_tile = override_layer.Tiles[source_tile_pos.X, source_tile_pos.Y];
						Tile new_tile = null;
						if (override_tile is StaticTile)
						{
							new_tile = new StaticTile(target_layer, tilesheet_lookup[override_tile.TileSheet], override_tile.BlendMode, override_tile.TileIndex);
						}
						else if (override_tile is AnimatedTile)
						{
							AnimatedTile override_animated_tile = override_tile as AnimatedTile;
							StaticTile[] tiles = new StaticTile[override_animated_tile.TileFrames.Length];
							for (int i = 0; i < override_animated_tile.TileFrames.Length; i++)
							{
								StaticTile frame_tile = override_animated_tile.TileFrames[i];
								tiles[i] = new StaticTile(target_layer, tilesheet_lookup[frame_tile.TileSheet], frame_tile.BlendMode, frame_tile.TileIndex);
							}
							new_tile = new AnimatedTile(target_layer, tiles, override_animated_tile.FrameInterval);
						}
						new_tile?.Properties.CopyFrom(override_tile.Properties);
						target_layer.Tiles[dest_tile_pos.X, dest_tile_pos.Y] = new_tile;
					}
				}
			}
			map.LoadTileSheets(Game1.mapDisplayDevice);
			if (Game1.IsMasterGame)
			{
				UpdateMapSeats();
			}
		}

		public virtual bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
		{
			return true;
		}

		public void ApplyMapOverride(string map_name, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? destination_rect = null)
		{
			if (!_appliedMapOverrides.Contains(map_name))
			{
				Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_name);
				ApplyMapOverride(override_map, map_name, source_rect, destination_rect);
			}
		}

		public void ApplyMapOverride(string map_name, string override_key_name, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? destination_rect = null)
		{
			if (!_appliedMapOverrides.Contains(override_key_name))
			{
				Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_name);
				ApplyMapOverride(override_map, override_key_name, source_rect, destination_rect);
			}
		}

		public virtual void UpdateMapSeats()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Dictionary<string, string> base_tilesheet_paths = new Dictionary<string, string>();
			Dictionary<string, string> chair_tile_data = Game1.content.Load<Dictionary<string, string>>("Data\\ChairTiles");
			mapSeats.Clear();
			Layer buildings_layer = map.GetLayer("Buildings");
			if (buildings_layer == null)
			{
				return;
			}
			for (int x = 0; x < buildings_layer.LayerWidth; x++)
			{
				for (int y = 0; y < buildings_layer.LayerHeight; y++)
				{
					Tile tile = buildings_layer.Tiles[x, y];
					if (tile == null)
					{
						continue;
					}
					string path = Path.GetFileNameWithoutExtension(tile.TileSheet.ImageSource);
					if (base_tilesheet_paths.ContainsKey(path))
					{
						path = base_tilesheet_paths[path];
					}
					else
					{
						if (path.StartsWith("summer_") || path.StartsWith("winter_") || path.StartsWith("fall_"))
						{
							path = "spring_" + path.Substring(path.IndexOf('_') + 1);
						}
						base_tilesheet_paths[path] = path;
					}
					int tiles_per_row = tile.TileSheet.SheetWidth;
					int tile_x = tile.TileIndex % tiles_per_row;
					int tile_y = tile.TileIndex / tiles_per_row;
					string key = path + "/" + tile_x + "/" + tile_y;
					if (chair_tile_data.ContainsKey(key))
					{
						MapSeat seat = MapSeat.FromData(chair_tile_data[key], x, y);
						if (seat != null)
						{
							mapSeats.Add(seat);
						}
					}
				}
			}
		}

		public void loadMap(string mapPath, bool force_reload = false)
		{
			if (force_reload)
			{
				LocalizedContentManager loader = Program.gamePtr.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
				map = loader.Load<Map>(mapPath);
				loader.Unload();
				InvalidateCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
			}
			else if (!ApplyCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps, mapPath))
			{
				map = getMapLoader().Load<Map>(mapPath);
			}
			updateSeasonalTileSheets(map);
			map.LoadTileSheets(Game1.mapDisplayDevice);
			UpdateMapSeats();
			map.Properties.TryGetValue("Outdoors", out PropertyValue isOutdoorsValue);
			if (isOutdoorsValue != null)
			{
				isOutdoors.Value = true;
			}
			map.Properties.TryGetValue("forceLoadPathLayerLights", out PropertyValue pathLayerLightsValue);
			if (pathLayerLightsValue != null)
			{
				forceLoadPathLayerLights = true;
			}
			map.Properties.TryGetValue("TreatAsOutdoors", out PropertyValue treatAsOutdoorsValue);
			if (treatAsOutdoorsValue != null)
			{
				treatAsOutdoors.Value = true;
			}
			bool indoorWater = false;
			map.Properties.TryGetValue("indoorWater", out PropertyValue indoorWaterValue);
			if (indoorWaterValue != null)
			{
				indoorWater = true;
			}
			if ((((bool)isOutdoors | indoorWater) || this is Sewer || this is Submarine) && !(this is Desert))
			{
				waterTiles = new bool[map.Layers[0].LayerWidth, map.Layers[0].LayerHeight];
				bool foundAnyWater = false;
				for (int x = 0; x < map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < map.Layers[0].LayerHeight; y++)
					{
						if (doesTileHaveProperty(x, y, "Water", "Back") != null)
						{
							foundAnyWater = true;
							waterTiles[x, y] = true;
						}
					}
				}
				if (!foundAnyWater)
				{
					waterTiles = null;
				}
			}
			if ((bool)isOutdoors)
			{
				critters = new List<Critter>();
			}
			loadLights();
		}

		public void reloadMap()
		{
			if (mapPath != null)
			{
				loadMap(mapPath);
			}
			else
			{
				map = null;
			}
			loadedMapPath = mapPath;
		}

		public virtual bool canSlimeMateHere()
		{
			return true;
		}

		public virtual bool canSlimeHatchHere()
		{
			return true;
		}

		public void addCharacter(NPC character)
		{
			characters.Add(character);
		}

		public NetCollection<NPC> getCharacters()
		{
			return characters;
		}

		public static Microsoft.Xna.Framework.Rectangle getSourceRectForObject(int tileIndex)
		{
			return new Microsoft.Xna.Framework.Rectangle(tileIndex * 16 % Game1.objectSpriteSheet.Width, tileIndex * 16 / Game1.objectSpriteSheet.Width * 16, 16, 16);
		}

		public Warp isCollidingWithWarp(Microsoft.Xna.Framework.Rectangle position, Character character)
		{
			if (ignoreWarps)
			{
				return null;
			}
			foreach (Warp w in warps)
			{
				if ((character is NPC || !w.npcOnly.Value) && (w.X == (int)Math.Floor((double)position.Left / 64.0) || w.X == (int)Math.Floor((double)position.Right / 64.0)) && (w.Y == (int)Math.Floor((double)position.Top / 64.0) || w.Y == (int)Math.Floor((double)position.Bottom / 64.0)))
				{
					if (w.TargetName == "BoatTunnel" && character is NPC)
					{
						return new Warp(w.X, w.Y, "IslandSouth", 17, 43, flipFarmer: false);
					}
					if (w.TargetName == "VolcanoEntrance")
					{
						return new Warp(w.X, w.Y, "VolcanoDungeon0", w.TargetX, w.TargetY, flipFarmer: false);
					}
					return w;
				}
			}
			return null;
		}

		public Warp isCollidingWithWarpOrDoor(Microsoft.Xna.Framework.Rectangle position, Character character = null)
		{
			Warp w = isCollidingWithWarp(position, character);
			if (w == null)
			{
				w = isCollidingWithDoors(position, character);
			}
			return w;
		}

		public virtual Warp isCollidingWithDoors(Microsoft.Xna.Framework.Rectangle position, Character character = null)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 v = Utility.getCornersOfThisRectangle(ref position, i);
				Point rectangleCorner = new Point((int)v.X / 64, (int)v.Y / 64);
				foreach (KeyValuePair<Point, string> pair in doors.Pairs)
				{
					Point door = pair.Key;
					if (rectangleCorner.Equals(door))
					{
						return getWarpFromDoor(door, character);
					}
				}
			}
			return null;
		}

		public virtual Warp getWarpFromDoor(Point door, Character character = null)
		{
			string[] split = doesTileHaveProperty(door.X, door.Y, "Action", "Buildings").Split(' ');
			if (split[0].Equals("WarpCommunityCenter"))
			{
				return new Warp(door.X, door.Y, "CommunityCenter", 32, 23, flipFarmer: false);
			}
			if (split[0].Equals("Warp_Sunroom_Door"))
			{
				return new Warp(door.X, door.Y, "Sunroom", 5, 13, flipFarmer: false);
			}
			if ((split[0].Equals("WarpBoatTunnel") || (split.Length > 3 && split[3].Equals("BoatTunnel"))) && character is NPC)
			{
				return new Warp(door.X, door.Y, "IslandSouth", 17, 43, flipFarmer: false);
			}
			return new Warp(door.X, door.Y, split[3], Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), flipFarmer: false);
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

		public virtual bool canFishHere()
		{
			return true;
		}

		public virtual bool CanRefillWateringCanOnTile(int tileX, int tileY)
		{
			if (doesTileHaveProperty(tileX, tileY, "Water", "Back") == null && doesTileHaveProperty(tileX, tileY, "WaterSource", "Back") == null)
			{
				if (!isOutdoors && doesTileHavePropertyNoNull(tileX, tileY, "Action", "Buildings").Equals("kitchen"))
				{
					if (getTileIndexAt(tileX, tileY, "Buildings") != 172)
					{
						return getTileIndexAt(tileX, tileY, "Buildings") == 257;
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public virtual bool isTileBuildingFishable(int tileX, int tileY)
		{
			return false;
		}

		public virtual bool isTileFishable(int tileX, int tileY)
		{
			if (isTileBuildingFishable(tileX, tileY))
			{
				return true;
			}
			if (doesTileHaveProperty(tileX, tileY, "Water", "Back") == null || doesTileHaveProperty(tileX, tileY, "NoFishing", "Back") != null || getTileIndexAt(tileX, tileY, "Buildings") != -1)
			{
				return doesTileHaveProperty(tileX, tileY, "Water", "Buildings") != null;
			}
			return true;
		}

		public bool isFarmerCollidingWithAnyCharacter()
		{
			foreach (NPC i in characters)
			{
				if (i != null && Game1.player.GetBoundingBox().Intersects(i.GetBoundingBox()))
				{
					return true;
				}
			}
			return false;
		}

		public bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer)
		{
			return isCollidingPosition(position, viewport, isFarmer, 0, glider: false, null, pathfinding: false);
		}

		public bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, Character character)
		{
			return isCollidingPosition(position, viewport, isFarmer: false, 0, glider: false, character, pathfinding: false);
		}

		public bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider)
		{
			return isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, null, pathfinding: false);
		}

		protected bool _TestCornersWorld(int top, int bottom, int left, int right, Func<int, int, bool> action)
		{
			if (action(right, top))
			{
				return true;
			}
			if (action(right, bottom))
			{
				return true;
			}
			if (action(left, top))
			{
				return true;
			}
			if (action(left, bottom))
			{
				return true;
			}
			return false;
		}

		protected bool _TestCornersWorld(int top, int bottom, int left, int right, int center, bool bigger_than_tile, Func<int, int, bool> action)
		{
			if (_TestCornersWorld(top, bottom, left, right, action))
			{
				return true;
			}
			if (bigger_than_tile)
			{
				if (action(center, top))
				{
					return true;
				}
				if (action(center, bottom))
				{
					return true;
				}
			}
			return false;
		}

		protected bool _TestCornersTiles(Vector2 top_right, Vector2 top_left, Vector2 bottom_right, Vector2 bottom_left, Vector2 top_mid, Vector2 bottom_mid, Vector2 player_top_right, Vector2 player_top_left, Vector2 player_bottom_right, Vector2 player_bottom_left, Vector2 player_top_mid, Vector2 player_bottom_mid, bool bigger_than_tile, Func<Vector2, Vector2, bool> action)
		{
			if (action(top_right, player_top_right))
			{
				return true;
			}
			if (action(top_left, player_top_left))
			{
				return true;
			}
			if (action(bottom_left, player_bottom_left))
			{
				return true;
			}
			if (action(bottom_right, player_bottom_right))
			{
				return true;
			}
			if (bigger_than_tile)
			{
				if (action(top_mid, player_top_mid))
				{
					return true;
				}
				if (action(bottom_mid, player_bottom_mid))
				{
					return true;
				}
			}
			return false;
		}

		public Furniture GetFurnitureAt(Vector2 tile_position)
		{
			Point position = default(Point);
			position.X = (int)((float)(int)tile_position.X + 0.5f) * 64;
			position.Y = (int)((float)(int)tile_position.Y + 0.5f) * 64;
			foreach (Furniture f in furniture)
			{
				if (f.getBoundingBox(f.TileLocation).Contains(position))
				{
					return f;
				}
			}
			return null;
		}

		public virtual bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if (!Game1.eventUp && (character == null || character.willDestroyObjectsUnderfoot))
			{
				foreach (Furniture f in furniture)
				{
					if ((int)f.furniture_type != 12 && f.IntersectsForCollision(position) && (!isFarmer || !(character as Farmer).TemporaryPassableTiles.Intersects(position)))
					{
						return true;
					}
				}
			}
			return isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding: false);
		}

		public virtual bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			foreach (ResourceClump r in resourceClumps)
			{
				if (!glider && r.getBoundingBox(r.tile).Intersects(position))
				{
					return true;
				}
			}
			bool is_event_up = Game1.eventUp;
			if (is_event_up && Game1.CurrentEvent != null && !Game1.CurrentEvent.ignoreObjectCollisions)
			{
				is_event_up = false;
			}
			updateMap();
			if (position.Right < 0 || position.X > map.Layers[0].DisplayWidth || position.Bottom < 0 || position.Top > map.Layers[0].DisplayHeight)
			{
				return false;
			}
			if (character == null && !ignoreCharacterRequirement)
			{
				return true;
			}
			Vector2 topRight = new Vector2(position.Right / 64, position.Top / 64);
			Vector2 topLeft = new Vector2(position.Left / 64, position.Top / 64);
			Vector2 BottomRight = new Vector2(position.Right / 64, position.Bottom / 64);
			Vector2 BottomLeft = new Vector2(position.Left / 64, position.Bottom / 64);
			bool biggerThanTile = position.Width > 64;
			Vector2 BottomMid = new Vector2(position.Center.X / 64, position.Bottom / 64);
			Vector2 TopMid = new Vector2(position.Center.X / 64, position.Top / 64);
			Microsoft.Xna.Framework.Rectangle playerBox = Game1.player.GetBoundingBox();
			Vector2 playerTopRight = new Vector2((playerBox.Right - 1) / 64, playerBox.Top / 64);
			Vector2 playerTopLeft = new Vector2(playerBox.Left / 64, playerBox.Top / 64);
			Vector2 playerBottomRight = new Vector2((playerBox.Right - 1) / 64, (playerBox.Bottom - 1) / 64);
			Vector2 playerBottomLeft = new Vector2(playerBox.Left / 64, (playerBox.Bottom - 1) / 64);
			Vector2 playerBottomMid = new Vector2(playerBox.Center.X / 64, (playerBox.Bottom - 1) / 64);
			Vector2 playerTopMid = new Vector2(playerBox.Center.X / 64, playerBox.Top / 64);
			if (((character != null) & isFarmer) && character is Farmer)
			{
				Farmer farmer2 = character as Farmer;
				if (farmer2.bridge != null && farmer2.onBridge.Value && position.Right >= farmer2.bridge.bridgeBounds.X && position.Left <= farmer2.bridge.bridgeBounds.Right)
				{
					if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, (int x, int y) => (y > farmer2.bridge.bridgeBounds.Bottom || y < farmer2.bridge.bridgeBounds.Top) ? true : false))
					{
						return true;
					}
					return false;
				}
			}
			if (!glider && (!is_event_up || (character != null && !isFarmer && (!pathfinding || !character.willDestroyObjectsUnderfoot))) && _TestCornersTiles(topRight, topLeft, BottomRight, BottomLeft, TopMid, BottomMid, playerTopRight, playerTopLeft, playerBottomRight, playerBottomLeft, playerTopMid, playerBottomMid, biggerThanTile, delegate(Vector2 corner, Vector2 player_corner)
			{
				objects.TryGetValue(corner, out Object value2);
				return (value2 != null && !value2.IsHoeDirt && !value2.isPassable() && !Game1.player.TemporaryPassableTiles.Intersects(value2.getBoundingBox(corner)) && value2.getBoundingBox(corner).Intersects(position) && character != null && (!(character is FarmAnimal) || !value2.isAnimalProduct()) && character.collideWith(value2) && (!isFarmer || corner != player_corner)) ? true : false;
			}))
			{
				return true;
			}
			if (!glider && !is_event_up)
			{
				foreach (Furniture f in furniture)
				{
					if ((int)f.furniture_type != 12 && f.IntersectsForCollision(position) && (!isFarmer || !f.IntersectsForCollision(Game1.player.GetBoundingBox())))
					{
						return true;
					}
				}
			}
			if (largeTerrainFeatures != null && !glider)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(position))
					{
						return true;
					}
				}
			}
			if (!is_event_up && !glider && _TestCornersTiles(topRight, topLeft, BottomRight, BottomLeft, TopMid, BottomMid, playerTopRight, playerTopLeft, playerBottomRight, playerBottomLeft, playerTopMid, playerBottomMid, biggerThanTile, delegate(Vector2 corner, Vector2 player_corner)
			{
				if (terrainFeatures.ContainsKey(corner) && terrainFeatures[corner].getBoundingBox(corner).Intersects(position))
				{
					if (!pathfinding)
					{
						terrainFeatures[corner].doCollisionAction(position, Game1.player.speed + Game1.player.addedSpeed, corner, character, this);
					}
					if (terrainFeatures.ContainsKey(corner) && !terrainFeatures[corner].isPassable(character) && (!isFarmer || corner != player_corner))
					{
						return true;
					}
				}
				return false;
			}))
			{
				return true;
			}
			if (character != null && character.hasSpecialCollisionRules() && (character.isColliding(this, topRight) || character.isColliding(this, topLeft) || character.isColliding(this, BottomRight) || character.isColliding(this, BottomLeft)))
			{
				return true;
			}
			if (((isFarmer && (currentEvent == null || currentEvent.playerControlSequence)) || (character != null && (bool)character.collidesWithOtherCharacters)) && !pathfinding)
			{
				for (int i = characters.Count - 1; i >= 0; i--)
				{
					NPC other = characters[i];
					if (other != null && (character == null || !character.Equals(other)))
					{
						Microsoft.Xna.Framework.Rectangle bounding_box = other.GetBoundingBox();
						if (other.layingDown)
						{
							bounding_box.Y -= 64;
							bounding_box.Height += 64;
						}
						if (bounding_box.Intersects(position) && !Game1.player.temporarilyInvincible)
						{
							other.behaviorOnFarmerPushing();
						}
						if (isFarmer && !is_event_up && !other.farmerPassesThrough && bounding_box.Intersects(position) && !Game1.player.temporarilyInvincible && Game1.player.TemporaryPassableTiles.IsEmpty() && (!other.IsMonster || (!((Monster)other).isGlider && !Game1.player.GetBoundingBox().Intersects(other.GetBoundingBox()))) && !other.IsInvisible && !Game1.player.GetBoundingBox().Intersects(bounding_box))
						{
							return true;
						}
						if (!isFarmer && bounding_box.Intersects(position))
						{
							return true;
						}
					}
				}
			}
			Layer back_layer = map.GetLayer("Back");
			Layer buildings_layer = map.GetLayer("Buildings");
			if (isFarmer)
			{
				if (currentEvent != null && currentEvent.checkForCollision(position, (character != null) ? (character as Farmer) : Game1.player))
				{
					return true;
				}
				if (Game1.player.currentUpgrade != null && Game1.player.currentUpgrade.daysLeftTillUpgradeDone <= 3 && name.Equals("Farm") && position.Intersects(new Microsoft.Xna.Framework.Rectangle((int)Game1.player.currentUpgrade.positionOfCarpenter.X, (int)Game1.player.currentUpgrade.positionOfCarpenter.Y + 64, 64, 32)))
				{
					return true;
				}
			}
			else
			{
				if (!pathfinding && !(character is Monster) && damagesFarmer == 0 && !glider)
				{
					foreach (Farmer farmer in farmers)
					{
						if (position.Intersects(farmer.GetBoundingBox()))
						{
							return true;
						}
					}
				}
				if (((bool)isFarm || name.Value.StartsWith("UndergroundMine") || this is IslandLocation) && character != null && !character.Name.Contains("NPC") && !character.eventActor && !glider)
				{
					PropertyValue barrier2 = null;
					Tile t3;
					if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, delegate(int x, int y)
					{
						t3 = back_layer.PickTile(new Location(x, y), viewport.Size);
						if (t3 != null)
						{
							t3.Properties.TryGetValue("NPCBarrier", out barrier2);
							if (barrier2 != null)
							{
								return true;
							}
						}
						return false;
					}))
					{
						return true;
					}
				}
				if (glider && !projectile)
				{
					return false;
				}
			}
			if (!isFarmer || !Game1.player.isRafting)
			{
				PropertyValue barrier = null;
				Tile t2;
				if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, delegate(int x, int y)
				{
					t2 = back_layer.PickTile(new Location(x, y), viewport.Size);
					if (t2 != null)
					{
						t2.Properties.TryGetValue("TemporaryBarrier", out barrier);
						if (barrier != null)
						{
							return true;
						}
					}
					return false;
				}))
				{
					return true;
				}
			}
			if (!isFarmer || !Game1.player.isRafting)
			{
				PropertyValue passable = null;
				if ((!(character is FarmAnimal) || !(character as FarmAnimal).IsActuallySwimming()) && _TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, position.Center.X, biggerThanTile, delegate(int x, int y)
				{
					Tile tile2 = back_layer.PickTile(new Location(x, y), viewport.Size);
					if (tile2 != null)
					{
						tile2.TileIndexProperties.TryGetValue("Passable", out passable);
						if (passable == null)
						{
							tile2.Properties.TryGetValue("Passable", out passable);
						}
						if (passable != null && (!isFarmer || !Game1.player.TemporaryPassableTiles.Contains(x, y)))
						{
							return true;
						}
					}
					return false;
				}))
				{
					return true;
				}
				Tile tmp;
				if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, position.Center.X, biggerThanTile, delegate(int x, int y)
				{
					tmp = buildings_layer.PickTile(new Location(x, y), viewport.Size);
					if (tmp != null)
					{
						if (projectile && this is VolcanoDungeon)
						{
							Tile tile = back_layer.PickTile(new Location(x, y), viewport.Size);
							if (tile != null)
							{
								PropertyValue value = null;
								if (value == null)
								{
									tile.TileIndexProperties.TryGetValue("Water", out value);
								}
								if (value == null)
								{
									tile.Properties.TryGetValue("Water", out value);
								}
								if (value != null)
								{
									return false;
								}
							}
						}
						tmp.TileIndexProperties.TryGetValue("Shadow", out passable);
						if (passable == null)
						{
							tmp.TileIndexProperties.TryGetValue("Passable", out passable);
						}
						if (passable == null)
						{
							tmp.Properties.TryGetValue("Passable", out passable);
						}
						if (projectile)
						{
							if (passable == null)
							{
								tmp.TileIndexProperties.TryGetValue("ProjectilePassable", out passable);
							}
							if (passable == null)
							{
								tmp.Properties.TryGetValue("ProjectilePassable", out passable);
							}
						}
						if (passable == null && !isFarmer)
						{
							tmp.TileIndexProperties.TryGetValue("NPCPassable", out passable);
						}
						if (passable == null && !isFarmer)
						{
							tmp.Properties.TryGetValue("NPCPassable", out passable);
						}
						if (passable == null && !isFarmer && character != null && character.canPassThroughActionTiles())
						{
							tmp.Properties.TryGetValue("Action", out passable);
						}
						if ((passable == null || passable.ToString().Length == 0) && (!isFarmer || !Game1.player.TemporaryPassableTiles.Contains(x, y)))
						{
							if (character == null)
							{
								return true;
							}
							return character.shouldCollideWithBuildingLayer(this);
						}
					}
					return false;
				}))
				{
					return true;
				}
				if (!isFarmer && passable != null && (passable.ToString().StartsWith("Door ") || passable.ToString() == "Door"))
				{
					openDoor(new Location(position.Center.X / 64, position.Bottom / 64), playSound: false);
					openDoor(new Location(position.Center.X / 64, position.Top / 64), Game1.currentLocation.Equals(this));
				}
				return false;
			}
			PropertyValue passable2 = null;
			Tile t;
			if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, delegate(int x, int y)
			{
				t = back_layer.PickTile(new Location(x, y), viewport.Size);
				if (t != null)
				{
					t.TileIndexProperties.TryGetValue("Water", out passable2);
				}
				if (passable2 == null)
				{
					if (isTileLocationOpen(new Location(x / 64, y / 64)) && !isTileOccupiedForPlacement(new Vector2(x / 64, y / 64)))
					{
						Game1.player.isRafting = false;
						Game1.player.Position = new Vector2(x / 64 * 64, y / 64 * 64 - 32);
						Game1.player.setTrajectory(0, 0);
					}
					return true;
				}
				return false;
			}))
			{
				return true;
			}
			return false;
		}

		public bool isTilePassable(Location tileLocation, xTile.Dimensions.Rectangle viewport)
		{
			PropertyValue passable = null;
			Tile tmp = map.GetLayer("Back").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
			tmp?.TileIndexProperties.TryGetValue("Passable", out passable);
			Tile tile = map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
			if (passable == null && tile == null)
			{
				return tmp != null;
			}
			return false;
		}

		public bool isPointPassable(Location location, xTile.Dimensions.Rectangle viewport)
		{
			PropertyValue passable = null;
			PropertyValue shadow = null;
			map.GetLayer("Back").PickTile(new Location(location.X, location.Y), viewport.Size)?.TileIndexProperties.TryGetValue("Passable", out passable);
			Tile tile = map.GetLayer("Buildings").PickTile(new Location(location.X, location.Y), viewport.Size);
			tile?.TileIndexProperties.TryGetValue("Shadow", out shadow);
			if (passable == null)
			{
				if (tile != null)
				{
					return shadow != null;
				}
				return true;
			}
			return false;
		}

		public bool isTilePassable(Microsoft.Xna.Framework.Rectangle nextPosition, xTile.Dimensions.Rectangle viewport)
		{
			if (isPointPassable(new Location(nextPosition.Left, nextPosition.Top), viewport) && isPointPassable(new Location(nextPosition.Left, nextPosition.Bottom), viewport) && isPointPassable(new Location(nextPosition.Right, nextPosition.Top), viewport))
			{
				return isPointPassable(new Location(nextPosition.Right, nextPosition.Bottom), viewport);
			}
			return false;
		}

		public bool isTileOnMap(Vector2 position)
		{
			if (position.X >= 0f && position.X < (float)map.Layers[0].LayerWidth && position.Y >= 0f)
			{
				return position.Y < (float)map.Layers[0].LayerHeight;
			}
			return false;
		}

		public bool isTileOnMap(int x, int y)
		{
			if (x >= 0 && x < map.Layers[0].LayerWidth && y >= 0)
			{
				return y < map.Layers[0].LayerHeight;
			}
			return false;
		}

		public void busLeave()
		{
			NPC pam = null;
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				if (characters[i].Name.Equals("Pam"))
				{
					pam = characters[i];
					characters.RemoveAt(i);
					break;
				}
			}
			if (pam == null)
			{
				return;
			}
			Game1.changeMusicTrack("none");
			localSound("openBox");
			if (name.Equals("BusStop"))
			{
				Game1.warpFarmer("Desert", 32, 27, flip: true);
				pam.followSchedule = false;
				pam.Position = new Vector2(1984f, 1752f);
				pam.faceDirection(2);
				pam.CurrentDialogue.Peek().temporaryDialogue = Game1.parseText(Game1.content.LoadString("Strings\\Locations:Desert_BusArrived"));
				Game1.getLocationFromName("Desert").characters.Add(pam);
				return;
			}
			pam.CurrentDialogue.Peek().temporaryDialogue = null;
			Game1.warpFarmer("BusStop", 9, 9, flip: true);
			if (Game1.timeOfDay >= 2300)
			{
				pam.Position = new Vector2(1152f, 408f);
				pam.faceDirection(2);
				Game1.getLocationFromName("Trailer").characters.Add(pam);
			}
			else if (Game1.timeOfDay >= 1700)
			{
				pam.Position = new Vector2(448f, 1112f);
				pam.faceDirection(1);
				Game1.getLocationFromName("Saloon").characters.Add(pam);
			}
			else
			{
				pam.Position = new Vector2(512f, 600f);
				pam.faceDirection(2);
				Game1.getLocationFromName("BusStop").characters.Add(pam);
				pam.Sprite.currentFrame = 0;
			}
			pam.DirectionsToNewLocation = null;
			pam.followSchedule = true;
		}

		public int numberOfObjectsWithName(string name)
		{
			int number = 0;
			foreach (Object value in objects.Values)
			{
				if (value.Name.Equals(name))
				{
					number++;
				}
			}
			return number;
		}

		public virtual Point getWarpPointTo(string location, Character character = null)
		{
			foreach (Warp w in warps)
			{
				if (w.TargetName.Equals(location))
				{
					return new Point(w.X, w.Y);
				}
				if (w.TargetName.Equals("BoatTunnel") && location == "IslandSouth")
				{
					return new Point(w.X, w.Y);
				}
			}
			foreach (KeyValuePair<Point, string> v in doors.Pairs)
			{
				if (v.Value.Equals("BoatTunnel") && location == "IslandSouth")
				{
					return v.Key;
				}
				if (v.Value.Equals(location))
				{
					return v.Key;
				}
			}
			return Point.Zero;
		}

		public Point getWarpPointTarget(Point warpPointLocation, Character character = null)
		{
			foreach (Warp w in warps)
			{
				if (w.X == warpPointLocation.X && w.Y == warpPointLocation.Y)
				{
					return new Point(w.TargetX, w.TargetY);
				}
			}
			foreach (KeyValuePair<Point, string> pair in doors.Pairs)
			{
				if (pair.Key.Equals(warpPointLocation))
				{
					string action = doesTileHaveProperty(warpPointLocation.X, warpPointLocation.Y, "Action", "Buildings");
					if (action != null && action.Contains("Warp"))
					{
						string[] split = action.Split(' ');
						if (split[0].Equals("WarpCommunityCenter"))
						{
							return new Point(32, 23);
						}
						if (split[0].Equals("Warp_Sunroom_Door"))
						{
							return new Point(5, 13);
						}
						if (character is NPC && (split[0].Equals("WarpBoatTunnel") || (split.Length > 3 && split[3].Equals("BoatTunnel"))))
						{
							return new Point(17, 43);
						}
						if (split[3].Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
						{
							return new Point(13, 24);
						}
						return new Point(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]));
					}
				}
			}
			return Point.Zero;
		}

		public virtual bool HasLocationOverrideDialogue(NPC character)
		{
			return false;
		}

		public virtual string GetLocationOverrideDialogue(NPC character)
		{
			if (!HasLocationOverrideDialogue(character))
			{
				return null;
			}
			return "";
		}

		public void boardBus(Vector2 playerTileLocation)
		{
			if (Game1.player.hasBusTicket || name.Equals("Desert"))
			{
				bool isPamOnDuty = false;
				for (int i = characters.Count - 1; i >= 0; i--)
				{
				}
				if (isPamOnDuty)
				{
					Game1.player.hasBusTicket = false;
					Game1.player.CanMove = false;
					Game1.viewportFreeze = true;
					Game1.player.position.X = -99999f;
					Game1.boardingBus = true;
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Bus_NoDriver"));
				}
			}
		}

		public NPC doesPositionCollideWithCharacter(float x, float y)
		{
			foreach (NPC i in characters)
			{
				if (i.GetBoundingBox().Contains((int)x, (int)y))
				{
					return i;
				}
			}
			return null;
		}

		public NPC doesPositionCollideWithCharacter(Microsoft.Xna.Framework.Rectangle r, bool ignoreMonsters = false)
		{
			foreach (NPC i in characters)
			{
				if (i.GetBoundingBox().Intersects(r) && (!i.IsMonster || !ignoreMonsters))
				{
					return i;
				}
			}
			return null;
		}

		public void switchOutNightTiles()
		{
			try
			{
				map.Properties.TryGetValue("NightTiles", out PropertyValue nightTiles);
				if (nightTiles != null)
				{
					string[] split = nightTiles.ToString().Split(' ');
					for (int i = 0; i < split.Length; i += 4)
					{
						if ((!split[i + 3].Equals("726") && !split[i + 3].Equals("720")) || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
						{
							map.GetLayer(split[i]).Tiles[Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])].TileIndex = Convert.ToInt32(split[i + 3]);
						}
					}
				}
			}
			catch (Exception)
			{
			}
			if (!(this is MineShaft) && !(this is Woods))
			{
				lightGlows.Clear();
			}
		}

		public virtual void checkForMusic(GameTime time)
		{
			if (!Game1.startedJukeboxMusic && Game1.getMusicTrackName() == "IslandMusic" && Game1.currentLocation != null && Game1.currentLocation.GetLocationContext() != LocationContext.Island)
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (Game1.getMusicTrackName() == "rain" && !Game1.IsRainingHere())
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (Utility.IsDesertLocation(this))
			{
				return;
			}
			if (Game1.getMusicTrackName() == "sam_acoustic1" && Game1.isMusicContextActiveButNotPlaying())
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (!(this is MineShaft) && Game1.getMusicTrackName().Contains("Ambient") && !Game1.getMusicTrackName().Contains(Game1.currentSeason) && getMapProperty("Music") != Game1.getMusicTrackName())
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (IsOutdoors && Game1.isMusicContextActiveButNotPlaying() && !Game1.IsRainingHere(this) && !Game1.eventUp)
			{
				if (!Game1.isDarkOut())
				{
					switch (Game1.currentSeason)
					{
					case "spring":
						Game1.changeMusicTrack("spring_day_ambient", track_interruptable: true);
						break;
					case "summer":
						Game1.changeMusicTrack("summer_day_ambient", track_interruptable: true);
						break;
					case "fall":
						Game1.changeMusicTrack("fall_day_ambient", track_interruptable: true);
						break;
					case "winter":
						Game1.changeMusicTrack("winter_day_ambient", track_interruptable: true);
						break;
					}
				}
				else if (Game1.isDarkOut() && Game1.timeOfDay < 2500)
				{
					switch (Game1.currentSeason)
					{
					case "spring":
						Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
						break;
					case "summer":
						Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
						break;
					case "fall":
						Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
						break;
					case "winter":
						Game1.changeMusicTrack("none", track_interruptable: true);
						break;
					}
				}
			}
			else if (Game1.isMusicContextActiveButNotPlaying() && Game1.IsRainingHere(this) && !Game1.showingEndOfNightStuff)
			{
				Game1.changeMusicTrack("rain", track_interruptable: true);
			}
			if (!Game1.isRaining && (!Game1.currentSeason.Equals("fall") || !Game1.isDebrisWeather) && !Game1.currentSeason.Equals("winter") && !Game1.eventUp && Game1.timeOfDay < 1800 && name.Equals("Town") && (Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("ambient")))
			{
				Game1.changeMusicTrack("springtown");
			}
			else if ((name.Equals("AnimalShop") || name.Equals("ScienceHouse")) && (Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("ambient")) && currentEvent == null)
			{
				Game1.changeMusicTrack("marnieShop");
			}
		}

		public NPC isCollidingWithCharacter(Microsoft.Xna.Framework.Rectangle box)
		{
			if (Game1.isFestival() && currentEvent != null)
			{
				foreach (NPC j in currentEvent.actors)
				{
					if (j.GetBoundingBox().Intersects(box))
					{
						return j;
					}
				}
			}
			foreach (NPC i in characters)
			{
				if (i.GetBoundingBox().Intersects(box))
				{
					return i;
				}
			}
			return null;
		}

		public virtual void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (critters != null && Game1.farmEvent == null)
			{
				for (int i = 0; i < critters.Count; i++)
				{
					critters[i].drawAboveFrontLayer(b);
				}
			}
			foreach (NPC character in characters)
			{
				character.drawAboveAlwaysFrontLayer(b);
			}
			foreach (TemporaryAnimatedSprite s in TemporarySprites)
			{
				if (s.drawAboveAlwaysFront)
				{
					s.draw(b);
				}
			}
			foreach (Projectile projectile in projectiles)
			{
				projectile.draw(b);
			}
		}

		public bool moveObject(int oldX, int oldY, int newX, int newY)
		{
			Vector2 oldObjectLocation = new Vector2(oldX, oldY);
			Vector2 newObjectLocation = new Vector2(newX, newY);
			if (objects.ContainsKey(oldObjectLocation) && !objects.ContainsKey(newObjectLocation))
			{
				Object o = objects[oldObjectLocation];
				o.tileLocation.Value = newObjectLocation;
				objects.Remove(oldObjectLocation);
				objects.Add(newObjectLocation, o);
				return true;
			}
			return false;
		}

		private void getGalaxySword()
		{
			Game1.flashAlpha = 1f;
			Game1.player.holdUpItemThenMessage(new MeleeWeapon(4));
			Game1.player.reduceActiveItemByOne();
			if (!Game1.player.addItemToInventoryBool(new MeleeWeapon(4)))
			{
				Game1.createItemDebris(new MeleeWeapon(4), Game1.player.getStandingPosition(), 1);
			}
			Game1.player.mailReceived.Add("galaxySword");
			Game1.player.jitterStrength = 0f;
			Game1.screenGlowHold = false;
			Game1.multiplayer.globalChatInfoMessage("GalaxySword", Game1.player.Name);
		}

		public virtual void performTouchAction(string fullActionString, Vector2 playerStandingPosition)
		{
			if (!Game1.eventUp)
			{
				try
				{
					switch (fullActionString.Split(' ')[0])
					{
					case "MagicalSeal":
						if (!Game1.player.mailReceived.Contains("krobusUnseal"))
						{
							Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
							Game1.player.yVelocity = 0f;
							Game1.player.Halt();
							Game1.player.TemporaryPassableTiles.Clear();
							if (Game1.player.getTileLocation().Equals(lastTouchActionLocation))
							{
								if (Game1.player.position.Y > lastTouchActionLocation.Y * 64f + 32f)
								{
									Game1.player.position.Y += 4f;
								}
								else
								{
									Game1.player.position.Y -= 4f;
								}
								lastTouchActionLocation = Vector2.Zero;
							}
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_MagicSeal"));
							for (int i = 0; i < 40; i++)
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 19f) * 64f + new Vector2(-8 + i % 4 * 16, -(i / 4) * 64 / 4), flicker: false, flipped: false)
								{
									layerDepth = 0.1152f + (float)i / 10000f,
									color = new Color(100 + i * 4, i * 5, 120 + i * 4),
									pingPong = true,
									delayBeforeAnimationStart = i * 10,
									scale = 4f,
									alphaFade = 0.01f
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 17f) * 64f + new Vector2(-8 + i % 4 * 16, i / 4 * 64 / 4), flicker: false, flipped: false)
								{
									layerDepth = 0.1152f + (float)i / 10000f,
									color = new Color(232 - i * 4, 192 - i * 6, 255 - i * 4),
									pingPong = true,
									delayBeforeAnimationStart = 320 + i * 10,
									scale = 4f,
									alphaFade = 0.01f
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 19f) * 64f + new Vector2(-8 + i % 4 * 16, -(i / 4) * 64 / 4), flicker: false, flipped: false)
								{
									layerDepth = 0.1152f + (float)i / 10000f,
									color = new Color(100 + i * 4, i * 6, 120 + i * 4),
									pingPong = true,
									delayBeforeAnimationStart = 640 + i * 10,
									scale = 4f,
									alphaFade = 0.01f
								});
							}
							Game1.player.jitterStrength = 2f;
							Game1.player.freezePause = 500;
							playSound("debuffHit");
						}
						break;
					case "MagicWarp":
					{
						string locationToWarp = fullActionString.Split(' ')[1];
						int locationX = Convert.ToInt32(fullActionString.Split(' ')[2]);
						int locationY = Convert.ToInt32(fullActionString.Split(' ')[3]);
						string mailRequired = (fullActionString.Split(' ').Length > 4) ? fullActionString.Split(' ')[4] : null;
						if (mailRequired == null || Game1.player.mailReceived.Contains(mailRequired))
						{
							for (int k = 0; k < 12; k++)
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)Game1.player.position.X - 256, (int)Game1.player.position.X + 192), Game1.random.Next((int)Game1.player.position.Y - 256, (int)Game1.player.position.Y + 192)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
							}
							playSound("wand");
							Game1.freezeControls = true;
							Game1.displayFarmer = false;
							Game1.player.CanMove = false;
							Game1.flashAlpha = 1f;
							DelayedAction.fadeAfterDelay(delegate
							{
								Game1.warpFarmer(locationToWarp, locationX, locationY, flip: false);
								Game1.fadeToBlackAlpha = 0.99f;
								Game1.screenGlow = false;
								Game1.displayFarmer = true;
								Game1.player.CanMove = true;
								Game1.freezeControls = false;
							}, 1000);
							new Microsoft.Xna.Framework.Rectangle(Game1.player.GetBoundingBox().X, Game1.player.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
							int j = 0;
							for (int x = Game1.player.getTileX() + 8; x >= Game1.player.getTileX() - 8; x--)
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(6, new Vector2(x, Game1.player.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
								{
									layerDepth = 1f,
									delayBeforeAnimationStart = j * 25,
									motion = new Vector2(-0.25f, 0f)
								});
								j++;
							}
						}
						break;
					}
					case "Door":
					{
						int l = 1;
						while (true)
						{
							if (l >= fullActionString.Split(' ').Length)
							{
								return;
							}
							if (Game1.player.getFriendshipHeartLevelForNPC(fullActionString.Split(' ')[l]) < 2 && l == fullActionString.Split(' ').Length - 1)
							{
								Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
								Game1.player.yVelocity = 0f;
								Game1.player.Halt();
								Game1.player.TemporaryPassableTiles.Clear();
								if (Game1.player.getTileLocation().Equals(lastTouchActionLocation))
								{
									if (Game1.player.Position.Y > lastTouchActionLocation.Y * 64f + 32f)
									{
										Game1.player.position.Y += 4f;
									}
									else
									{
										Game1.player.position.Y -= 4f;
									}
									lastTouchActionLocation = Vector2.Zero;
								}
								if ((!Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[1]) || (fullActionString.Split(' ').Length != 2 && !Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[2]))) && (fullActionString.Split(' ').Length != 3 || !Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[2])))
								{
									if (fullActionString.Split(' ').Length == 2)
									{
										NPC character4 = Game1.getCharacterFromName(fullActionString.Split(' ')[1]);
										string gender = (character4.Gender == 0) ? "Male" : "Female";
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_" + gender, character4.displayName));
									}
									else
									{
										NPC character3 = Game1.getCharacterFromName(fullActionString.Split(' ')[1]);
										NPC character2 = Game1.getCharacterFromName(fullActionString.Split(' ')[2]);
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_Couple", character3.displayName, character2.displayName));
									}
								}
								return;
							}
							if (l != fullActionString.Split(' ').Length - 1 && Game1.player.getFriendshipHeartLevelForNPC(fullActionString.Split(' ')[l]) >= 2)
							{
								if (!Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[l]))
								{
									Game1.player.mailReceived.Add("doorUnlock" + fullActionString.Split(' ')[l]);
								}
								return;
							}
							if (l == fullActionString.Split(' ').Length - 1 && Game1.player.getFriendshipHeartLevelForNPC(fullActionString.Split(' ')[l]) >= 2)
							{
								break;
							}
							l++;
						}
						if (!Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[l]))
						{
							Game1.player.mailReceived.Add("doorUnlock" + fullActionString.Split(' ')[l]);
						}
						break;
					}
					case "Sleep":
						if (!Game1.newDay && Game1.shouldTimePass() && Game1.player.hasMoved && !Game1.player.passedOut)
						{
							createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), createYesNoResponses(), "Sleep", null);
						}
						break;
					case "Bus":
						boardBus(playerStandingPosition);
						break;
					case "FaceDirection":
						if (getCharacterFromName(fullActionString.Split(' ')[1]) != null)
						{
							getCharacterFromName(fullActionString.Split(' ')[1]).faceDirection(Convert.ToInt32(fullActionString.Split(' ')[2]));
						}
						break;
					case "Emote":
						getCharacterFromName(fullActionString.Split(' ')[1]).doEmote(Convert.ToInt32(fullActionString.Split(' ')[2]));
						break;
					case "legendarySword":
						if (Game1.player.ActiveObject != null && Utility.IsNormalObjectAtParentSheetIndex(Game1.player.ActiveObject, 74) && !Game1.player.mailReceived.Contains("galaxySword"))
						{
							Game1.player.Halt();
							Game1.player.faceDirection(2);
							Game1.player.showCarrying();
							Game1.player.jitterStrength = 1f;
							Game1.pauseThenDoFunction(7000, getGalaxySword);
							Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.Event);
							playSound("crit");
							Game1.screenGlowOnce(new Color(30, 0, 150), hold: true, 0.01f, 0.999f);
							DelayedAction.playSoundAfterDelay("stardrop", 1500);
							Game1.screenOverlayTempSprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), 500, Color.White, 10, 2000));
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
							{
								Game1.stopMusicTrack(Game1.MusicContext.Event);
							});
						}
						else if (!Game1.player.mailReceived.Contains("galaxySword"))
						{
							localSound("SpringBirds");
						}
						break;
					case "MensLocker":
						if (!Game1.player.IsMale)
						{
							Game1.player.position.Y += (Game1.player.Speed + Game1.player.addedSpeed) * 2;
							Game1.player.Halt();
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MensLocker_WrongGender"));
						}
						break;
					case "WomensLocker":
						if (Game1.player.IsMale)
						{
							Game1.player.position.Y += (Game1.player.Speed + Game1.player.addedSpeed) * 2;
							Game1.player.Halt();
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WomensLocker_WrongGender"));
						}
						break;
					case "PoolEntrance":
						if (!Game1.player.swimming)
						{
							Game1.player.swimTimer = 800;
							Game1.player.swimming.Value = true;
							Game1.player.position.Y += 16f;
							Game1.player.yVelocity = -8f;
							playSound("pullItemFromWater");
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(27, 100f, 4, 0, new Vector2(Game1.player.Position.X, Game1.player.getStandingY() - 40), flicker: false, flipped: false)
							{
								layerDepth = 1f,
								motion = new Vector2(0f, 2f)
							});
						}
						else
						{
							Game1.player.jump();
							Game1.player.swimTimer = 800;
							Game1.player.position.X = playerStandingPosition.X * 64f;
							playSound("pullItemFromWater");
							Game1.player.yVelocity = 8f;
							Game1.player.swimming.Value = false;
						}
						Game1.player.noMovementPause = 500;
						break;
					case "ChangeIntoSwimsuit":
						Game1.player.changeIntoSwimsuit();
						break;
					case "ChangeOutOfSwimsuit":
						Game1.player.changeOutOfSwimSuit();
						break;
					}
				}
				catch (Exception)
				{
				}
			}
		}

		public virtual void updateMap()
		{
			if (!object.Equals(mapPath.Value, loadedMapPath))
			{
				reloadMap();
			}
		}

		public LargeTerrainFeature getLargeTerrainFeatureAt(int tileX, int tileY)
		{
			foreach (LargeTerrainFeature ltf in largeTerrainFeatures)
			{
				if (ltf.getBoundingBox().Contains(tileX * 64 + 32, tileY * 64 + 32))
				{
					return ltf;
				}
			}
			return null;
		}

		public virtual void UpdateWhenCurrentLocation(GameTime time)
		{
			updateMap();
			if (wasUpdated)
			{
				return;
			}
			wasUpdated = true;
			furnitureToRemove.Update(this);
			foreach (Furniture item in furniture)
			{
				item.updateWhenCurrentLocation(time, this);
			}
			AmbientLocationSounds.update(time);
			if (critters != null)
			{
				for (int l = critters.Count - 1; l >= 0; l--)
				{
					if (critters[l].update(time, this))
					{
						critters.RemoveAt(l);
					}
				}
			}
			if (fishSplashAnimation != null)
			{
				fishSplashAnimation.update(time);
				if (Game1.random.NextDouble() < 0.02)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite(0, fishSplashAnimation.position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), Color.White * 0.3f));
				}
			}
			if (orePanAnimation != null)
			{
				orePanAnimation.update(time);
				if (Game1.random.NextDouble() < 0.05)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), orePanAnimation.position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flipped: false, 0.02f, Color.White * 0.8f)
					{
						scale = 2f,
						animationLength = 6,
						interval = 100f
					});
				}
			}
			interiorDoors.Update(time);
			updateWater(time);
			Map.Update(time.ElapsedGameTime.Milliseconds);
			int k = 0;
			while (k < debris.Count)
			{
				if (debris[k].updateChunks(time, this))
				{
					debris.RemoveAt(k);
				}
				else
				{
					k++;
				}
			}
			if (Game1.shouldTimePass() || Game1.isFestival())
			{
				int j = 0;
				while (j < projectiles.Count)
				{
					if (projectiles[j].update(time, this))
					{
						projectiles.RemoveAt(j);
					}
					else
					{
						j++;
					}
				}
			}
			if (true)
			{
				for (int i = 0; i < _activeTerrainFeatures.Count; i++)
				{
					TerrainFeature feature = _activeTerrainFeatures[i];
					if (feature.tickUpdate(time, feature.currentTileLocation, this))
					{
						terrainFeaturesToRemoveList.Add(feature.currentTileLocation);
					}
				}
			}
			else
			{
				foreach (KeyValuePair<Vector2, TerrainFeature> v4 in terrainFeatures.Pairs)
				{
					if (v4.Value.tickUpdate(time, v4.Key, this))
					{
						terrainFeaturesToRemoveList.Add(v4.Key);
					}
				}
			}
			foreach (Vector2 v3 in terrainFeaturesToRemoveList)
			{
				terrainFeatures.Remove(v3);
			}
			terrainFeaturesToRemoveList.Clear();
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					largeTerrainFeature.tickUpdate(time, this);
				}
			}
			foreach (ResourceClump r in resourceClumps)
			{
				r.tickUpdate(time, r.tile, this);
			}
			if (Game1.timeOfDay >= 2000 && (float)lightLevel > 0f && name.Equals("FarmHouse"))
			{
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(64f, 448f), 2f, LightSource.LightContext.None, 0L));
			}
			if (currentEvent != null)
			{
				bool continue_execution2 = false;
				do
				{
					int last_command_index = currentEvent.CurrentCommand;
					currentEvent.checkForNextCommand(this, time);
					if (currentEvent != null)
					{
						continue_execution2 = currentEvent.simultaneousCommand;
						if (last_command_index == currentEvent.CurrentCommand)
						{
							continue_execution2 = false;
						}
					}
					else
					{
						continue_execution2 = false;
					}
				}
				while (continue_execution2);
			}
			foreach (Object v2 in objects.Values)
			{
				tempObjects.Add(v2);
			}
			foreach (Object tempObject in tempObjects)
			{
				tempObject.updateWhenCurrentLocation(time, this);
			}
			tempObjects.Clear();
			if (Game1.gameMode != 3 || this != Game1.currentLocation)
			{
				return;
			}
			if (!Utility.IsDesertLocation(Game1.currentLocation))
			{
				if ((bool)isOutdoors && !Game1.IsRainingHere(this) && Game1.random.NextDouble() < 0.002 && Game1.isMusicContextActiveButNotPlaying() && Game1.timeOfDay < 2000 && !Game1.currentSeason.Equals("winter") && !name.Equals("Desert"))
				{
					localSound("SpringBirds");
				}
				else if (!Game1.IsRainingHere(this) && (bool)isOutdoors && Game1.timeOfDay > 2100 && Game1.currentSeason.Equals("summer") && Game1.random.NextDouble() < 0.0005 && !(this is Beach) && !name.Equals("temp"))
				{
					localSound("crickets");
				}
				else if (Game1.IsRainingHere(this) && (bool)isOutdoors && !name.Equals("Town") && !Game1.eventUp && Game1.options.musicVolumeLevel > 0f && Game1.random.NextDouble() < 0.00015)
				{
					localSound("rainsound");
				}
			}
			Vector2 playerStandingPosition = new Vector2(Game1.player.getStandingX() / 64, Game1.player.getStandingY() / 64);
			if (lastTouchActionLocation.Equals(Vector2.Zero))
			{
				string touchActionProperty = doesTileHaveProperty((int)playerStandingPosition.X, (int)playerStandingPosition.Y, "TouchAction", "Back");
				lastTouchActionLocation = new Vector2(Game1.player.getStandingX() / 64, Game1.player.getStandingY() / 64);
				if (touchActionProperty != null)
				{
					performTouchAction(touchActionProperty, playerStandingPosition);
				}
			}
			else if (!lastTouchActionLocation.Equals(playerStandingPosition))
			{
				lastTouchActionLocation = Vector2.Zero;
			}
			foreach (Farmer farmer in farmers)
			{
				Vector2 playerPos = farmer.getTileLocation();
				Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
				foreach (Vector2 offset in adjacentTilesOffsets)
				{
					Vector2 v = playerPos + offset;
					if (objects.TryGetValue(v, out Object obj))
					{
						obj.farmerAdjacentAction(this);
					}
				}
			}
			if (Game1.boardingBus)
			{
				NPC pam = getCharacterFromName("Pam");
				if (pam != null && doesTileHaveProperty(pam.getStandingX() / 64, pam.getStandingY() / 64, "TouchAction", "Back") != null)
				{
					busLeave();
				}
			}
		}

		public void updateWater(GameTime time)
		{
			waterAnimationTimer -= time.ElapsedGameTime.Milliseconds;
			if (waterAnimationTimer <= 0)
			{
				waterAnimationIndex = (waterAnimationIndex + 1) % 10;
				waterAnimationTimer = 200;
			}
			if (!isFarm)
			{
				waterPosition += (float)((Math.Sin((float)time.TotalGameTime.Milliseconds / 1000f) + 1.0) * 0.15000000596046448);
			}
			else
			{
				waterPosition += 0.1f;
			}
			if (waterPosition >= 64f)
			{
				waterPosition -= 64f;
				waterTileFlip = !waterTileFlip;
			}
		}

		public NPC getCharacterFromName(string name)
		{
			NPC character = null;
			foreach (NPC i in characters)
			{
				if (i.Name.Equals(name))
				{
					return i;
				}
			}
			return character;
		}

		protected virtual void updateCharacters(GameTime time)
		{
			bool shouldTimePass = Game1.shouldTimePass();
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				NPC character = characters[i];
				if (character != null && (shouldTimePass || character is Horse || character.forceUpdateTimer > 0))
				{
					character.currentLocation = this;
					character.update(time, this);
					if (i < characters.Count && character is Monster && ((Monster)character).Health <= 0)
					{
						characters.RemoveAt(i);
					}
				}
				else if (character != null)
				{
					if (character.hasJustStartedFacingPlayer)
					{
						character.updateFaceTowardsFarmer(time, this);
					}
					character.updateEmote(time);
				}
			}
		}

		public virtual void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			netAudio.Update();
			removeTemporarySpritesWithIDEvent.Poll();
			rumbleAndFadeEvent.Poll();
			damagePlayersEvent.Poll();
			if (!ignoreWasUpdatedFlush)
			{
				wasUpdated = false;
			}
			updateCharacters(time);
			for (int i = temporarySprites.Count - 1; i >= 0; i--)
			{
				TemporaryAnimatedSprite sprite = (i < temporarySprites.Count) ? temporarySprites[i] : null;
				if (i < temporarySprites.Count && sprite != null && sprite.update(time) && i < temporarySprites.Count)
				{
					temporarySprites.RemoveAt(i);
				}
			}
		}

		public Response[] createYesNoResponses()
		{
			return new Response[2]
			{
				new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y),
				new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
			};
		}

		public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey)
		{
			lastQuestionKey = dialogKey;
			Game1.drawObjectQuestionDialogue(question, answerChoices.ToList());
		}

		public void createQuestionDialogueWithCustomWidth(string question, Response[] answerChoices, string dialogKey)
		{
			int width = SpriteText.getWidthOfString(question) + 64;
			lastQuestionKey = dialogKey;
			Game1.drawObjectQuestionDialogue(question, answerChoices.ToList(), width);
		}

		public void createQuestionDialogue(string question, Response[] answerChoices, afterQuestionBehavior afterDialogueBehavior, NPC speaker = null)
		{
			afterQuestion = afterDialogueBehavior;
			Game1.drawObjectQuestionDialogue(question, answerChoices.ToList());
			if (speaker != null)
			{
				Game1.objectDialoguePortraitPerson = speaker;
			}
		}

		public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey, Object actionObject)
		{
			lastQuestionKey = dialogKey;
			Game1.drawObjectQuestionDialogue(question, answerChoices.ToList());
			actionObjectForQuestionDialogue = actionObject;
		}

		public virtual Point GetMapPropertyPosition(string key, int default_x, int default_y)
		{
			string property_value = getMapProperty(key);
			if (property_value != "")
			{
				int x = -1;
				int y = -1;
				string[] split = property_value.Split(' ');
				if (split.Length >= 2 && int.TryParse(split[0], out x) && int.TryParse(split[1], out y))
				{
					return new Point(x, y);
				}
			}
			return new Point(default_x, default_y);
		}

		public virtual void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			_ = (int)monster.coinsToDrop;
			IList<int> objects = monster.objectsToDrop;
			Vector2 playerPosition = new Vector2(Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y);
			List<Item> extraDrops2 = monster.getExtraDropItems();
			if (Game1.player.isWearingRing(526))
			{
				string result = "";
				Game1.content.Load<Dictionary<string, string>>("Data\\Monsters").TryGetValue(monster.Name, out result);
				if (result != null && result.Length > 0)
				{
					string[] objectsSplit = result.Split('/')[6].Split(' ');
					for (int l = 0; l < objectsSplit.Length; l += 2)
					{
						if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[l + 1]))
						{
							objects.Add(Convert.ToInt32(objectsSplit[l]));
						}
					}
				}
			}
			for (int k = 0; k < objects.Count; k++)
			{
				int objectToAdd = objects[k];
				if (objectToAdd < 0)
				{
					debris.Add(monster.ModifyMonsterLoot(new Debris(Math.Abs(objectToAdd), Game1.random.Next(1, 4), new Vector2(x, y), playerPosition)));
				}
				else
				{
					debris.Add(monster.ModifyMonsterLoot(new Debris(objectToAdd, new Vector2(x, y), playerPosition)));
				}
			}
			for (int j = 0; j < extraDrops2.Count; j++)
			{
				debris.Add(monster.ModifyMonsterLoot(new Debris(extraDrops2[j], new Vector2(x, y), playerPosition)));
			}
			if (Game1.player.isWearingRing(526))
			{
				extraDrops2 = monster.getExtraDropItems();
				for (int i = 0; i < extraDrops2.Count; i++)
				{
					Item tmp = extraDrops2[i].getOne();
					tmp.Stack = extraDrops2[i].Stack;
					tmp.HasBeenInInventory = false;
					debris.Add(monster.ModifyMonsterLoot(new Debris(tmp, new Vector2(x, y), playerPosition)));
				}
			}
			if (HasUnlockedAreaSecretNotes(Game1.player) && Game1.random.NextDouble() < 0.033)
			{
				Object o = tryToCreateUnseenSecretNote(Game1.player);
				if (o != null)
				{
					monster.ModifyMonsterLoot(Game1.createItemDebris(o, new Vector2(x, y), -1, this));
				}
			}
			if (this is Woods && Game1.random.NextDouble() < 0.1)
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(new Object(292, 1), new Vector2(x, y), -1, this));
			}
			if ((bool)monster.isHardModeMonster && Game1.stats.getStat("hardModeMonstersKilled") > 50 && Game1.random.NextDouble() < 0.001 + (double)((float)who.LuckLevel * 0.0002f))
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(new Object(896, 1), new Vector2(x, y), -1, this));
			}
			else if ((bool)monster.isHardModeMonster && Game1.random.NextDouble() < 0.008 + (double)((float)who.LuckLevel * 0.002f))
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(new Object(858, 1), new Vector2(x, y), -1, this));
			}
		}

		public virtual bool HasUnlockedAreaSecretNotes(Farmer who)
		{
			if (GetLocationContext() == LocationContext.Island)
			{
				return true;
			}
			return who.hasMagnifyingGlass;
		}

		public bool damageMonster(Microsoft.Xna.Framework.Rectangle areaOfEffect, int minDamage, int maxDamage, bool isBomb, Farmer who)
		{
			return damageMonster(areaOfEffect, minDamage, maxDamage, isBomb, 1f, 0, 0f, 1f, triggerMonsterInvincibleTimer: false, who);
		}

		private bool isMonsterDamageApplicable(Farmer who, Monster monster, bool horizontalBias = true)
		{
			if (!monster.isGlider && !(who.CurrentTool is Slingshot) && !monster.ignoreDamageLOS.Value)
			{
				Point farmerStandingPoint = who.getTileLocationPoint();
				Point monsterStandingPoint = monster.getTileLocationPoint();
				if (Math.Abs(farmerStandingPoint.X - monsterStandingPoint.X) + Math.Abs(farmerStandingPoint.Y - monsterStandingPoint.Y) > 1)
				{
					int xDif = monsterStandingPoint.X - farmerStandingPoint.X;
					int yDif = monsterStandingPoint.Y - farmerStandingPoint.Y;
					Vector2 pointInQuestion = new Vector2(farmerStandingPoint.X, farmerStandingPoint.Y);
					while (xDif != 0 || yDif != 0)
					{
						if (horizontalBias)
						{
							if (Math.Abs(xDif) >= Math.Abs(yDif))
							{
								pointInQuestion.X += Math.Sign(xDif);
								xDif -= Math.Sign(xDif);
							}
							else
							{
								pointInQuestion.Y += Math.Sign(yDif);
								yDif -= Math.Sign(yDif);
							}
						}
						else if (Math.Abs(yDif) >= Math.Abs(xDif))
						{
							pointInQuestion.Y += Math.Sign(yDif);
							yDif -= Math.Sign(yDif);
						}
						else
						{
							pointInQuestion.X += Math.Sign(xDif);
							xDif -= Math.Sign(xDif);
						}
						if ((objects.ContainsKey(pointInQuestion) && !objects[pointInQuestion].isPassable()) || BlocksDamageLOS((int)pointInQuestion.X, (int)pointInQuestion.Y))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public virtual bool BlocksDamageLOS(int x, int y)
		{
			if (getTileIndexAt(x, y, "Buildings") != -1 && doesTileHaveProperty(x, y, "Passable", "Buildings") == null)
			{
				return true;
			}
			return false;
		}

		public bool damageMonster(Microsoft.Xna.Framework.Rectangle areaOfEffect, int minDamage, int maxDamage, bool isBomb, float knockBackModifier, int addedPrecision, float critChance, float critMultiplier, bool triggerMonsterInvincibleTimer, Farmer who)
		{
			bool didAnyDamage = false;
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				Monster monster;
				if (i < characters.Count && (monster = (characters[i] as Monster)) != null && monster.IsMonster && monster.Health > 0 && monster.TakesDamageFromHitbox(areaOfEffect))
				{
					if (monster.currentLocation == null)
					{
						monster.currentLocation = this;
					}
					if (!monster.IsInvisible && !monster.isInvincible() && (isBomb || isMonsterDamageApplicable(who, monster) || isMonsterDamageApplicable(who, monster, horizontalBias: false)))
					{
						bool isDagger = !isBomb && who != null && who.CurrentTool != null && who.CurrentTool is MeleeWeapon && (int)(who.CurrentTool as MeleeWeapon).type == 1;
						bool isDaggerSpecial = false;
						if (isDagger && MeleeWeapon.daggerHitsLeft > 1)
						{
							isDaggerSpecial = true;
						}
						if (isDaggerSpecial)
						{
							triggerMonsterInvincibleTimer = false;
						}
						didAnyDamage = true;
						if (Game1.currentLocation == this)
						{
							Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 8.0), 200 + Game1.random.Next(-50, 50));
						}
						Microsoft.Xna.Framework.Rectangle monsterBox = monster.GetBoundingBox();
						Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(monsterBox, who);
						if (!(knockBackModifier > 0f))
						{
							trajectory = new Vector2(monster.xVelocity, monster.yVelocity);
						}
						else
						{
							trajectory *= knockBackModifier;
						}
						if (monster.Slipperiness == -1)
						{
							trajectory = Vector2.Zero;
						}
						bool crit = false;
						int damageAmount5 = 0;
						if (who != null && who.CurrentTool != null && monster.hitWithTool(who.CurrentTool))
						{
							return false;
						}
						if (who.professions.Contains(25))
						{
							critChance += critChance * 0.5f;
						}
						if (maxDamage >= 0)
						{
							damageAmount5 = Game1.random.Next(minDamage, maxDamage + 1);
							if (who != null && Game1.random.NextDouble() < (double)(critChance + (float)who.LuckLevel * (critChance / 40f)))
							{
								crit = true;
								playSound("crit");
							}
							damageAmount5 = (crit ? ((int)((float)damageAmount5 * critMultiplier)) : damageAmount5);
							damageAmount5 = Math.Max(1, damageAmount5 + ((who != null) ? (who.attack * 3) : 0));
							if (who != null && who.professions.Contains(24))
							{
								damageAmount5 = (int)Math.Ceiling((float)damageAmount5 * 1.1f);
							}
							if (who != null && who.professions.Contains(26))
							{
								damageAmount5 = (int)Math.Ceiling((float)damageAmount5 * 1.15f);
							}
							if (who != null && crit && who.professions.Contains(29))
							{
								damageAmount5 = (int)((float)damageAmount5 * 2f);
							}
							if (who != null)
							{
								foreach (BaseEnchantment enchantment in who.enchantments)
								{
									enchantment.OnCalculateDamage(monster, this, who, ref damageAmount5);
								}
							}
							damageAmount5 = monster.takeDamage(damageAmount5, (int)trajectory.X, (int)trajectory.Y, isBomb, (double)addedPrecision / 10.0, who);
							if (isDaggerSpecial)
							{
								monster.stunTime = 50;
							}
							else
							{
								monster.stunTime = 0;
							}
							if (damageAmount5 == -1)
							{
								debris.Add(new Debris("Miss", 1, new Vector2(monsterBox.Center.X, monsterBox.Center.Y), Color.LightGray, 1f, 0f));
							}
							else
							{
								removeDamageDebris(monster);
								debris.Add(new Debris(damageAmount5, new Vector2(monsterBox.Center.X + 16, monsterBox.Center.Y), crit ? Color.Yellow : new Color(255, 130, 0), crit ? (1f + (float)damageAmount5 / 300f) : 1f, monster));
								if (who != null)
								{
									foreach (BaseEnchantment enchantment2 in who.enchantments)
									{
										enchantment2.OnDealDamage(monster, this, who, ref damageAmount5);
									}
								}
							}
							if (triggerMonsterInvincibleTimer)
							{
								monster.setInvincibleCountdown(450 / (isDagger ? 3 : 2));
							}
						}
						else
						{
							damageAmount5 = -2;
							monster.setTrajectory(trajectory);
							if (monster.Slipperiness > 10)
							{
								monster.xVelocity /= 2f;
								monster.yVelocity /= 2f;
							}
						}
						if (who != null && who.CurrentTool != null && who.CurrentTool.Name.Equals("Galaxy Sword"))
						{
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(50, 120), 6, 1, new Vector2(monsterBox.Center.X - 32, monsterBox.Center.Y - 32), flicker: false, flipped: false));
						}
						if (monster.Health <= 0)
						{
							if (!isFarm)
							{
								who.checkForQuestComplete(null, 1, 1, null, monster.Name, 4);
							}
							if (!isFarm && Game1.player.team.specialOrders != null)
							{
								foreach (SpecialOrder order in Game1.player.team.specialOrders)
								{
									if (order.onMonsterSlain != null)
									{
										order.onMonsterSlain(Game1.player, monster);
									}
								}
							}
							if (who != null)
							{
								foreach (BaseEnchantment enchantment3 in who.enchantments)
								{
									enchantment3.OnMonsterSlay(monster, this, who);
								}
							}
							if (who != null && who.leftRing.Value != null)
							{
								who.leftRing.Value.onMonsterSlay(monster, this, who);
							}
							if (who != null && who.rightRing.Value != null)
							{
								who.rightRing.Value.onMonsterSlay(monster, this, who);
							}
							if (who != null && !isFarm && (!(monster is GreenSlime) || (bool)(monster as GreenSlime).firstGeneration))
							{
								if (who.IsLocalPlayer)
								{
									Game1.stats.monsterKilled(monster.Name);
								}
								else if (Game1.IsMasterGame)
								{
									who.queueMessage(25, Game1.player, monster.Name);
								}
							}
							monsterDrop(monster, monsterBox.Center.X, monsterBox.Center.Y, who);
							if (who != null && !isFarm)
							{
								who.gainExperience(4, monster.ExperienceGained);
							}
							if ((bool)monster.isHardModeMonster)
							{
								Game1.stats.incrementStat("hardModeMonstersKilled", 1);
							}
							characters.Remove(monster);
							Game1.stats.MonstersKilled++;
						}
						else if (damageAmount5 > 0)
						{
							monster.shedChunks(Game1.random.Next(1, 3));
							if (crit)
							{
								Vector2 standPos = monster.getStandingPosition();
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32f, 32f), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
								{
									scale = 0.75f,
									alpha = (crit ? 0.75f : 0.5f)
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
								{
									scale = 0.5f,
									delayBeforeAnimationStart = 50,
									alpha = (crit ? 0.75f : 0.5f)
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
								{
									scale = 0.5f,
									delayBeforeAnimationStart = 100,
									alpha = (crit ? 0.75f : 0.5f)
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
								{
									scale = 0.5f,
									delayBeforeAnimationStart = 150,
									alpha = (crit ? 0.75f : 0.5f)
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
								{
									scale = 0.5f,
									delayBeforeAnimationStart = 200,
									alpha = (crit ? 0.75f : 0.5f)
								});
							}
						}
					}
				}
			}
			return didAnyDamage;
		}

		public void moveCharacters(GameTime time)
		{
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				NPC character = characters[i];
				if (!character.IsInvisible)
				{
					character.updateMovement(this, time);
				}
			}
		}

		public void growWeedGrass(int iterations)
		{
			for (int i = 0; i < iterations; i++)
			{
				for (int j = terrainFeatures.Count() - 1; j >= 0; j--)
				{
					KeyValuePair<Vector2, TerrainFeature> kvp = terrainFeatures.Pairs.ElementAt(j);
					if (kvp.Value is Grass && Game1.random.NextDouble() < 0.65)
					{
						if ((int)((Grass)kvp.Value).numberOfWeeds < 4)
						{
							((Grass)kvp.Value).numberOfWeeds.Value = Math.Max(0, Math.Min(4, (int)((Grass)kvp.Value).numberOfWeeds + Game1.random.Next(3)));
						}
						else if ((int)((Grass)kvp.Value).numberOfWeeds >= 4)
						{
							int xCoord = (int)kvp.Key.X;
							int yCoord = (int)kvp.Key.Y;
							if (isTileOnMap(xCoord, yCoord) && !isTileOccupied(kvp.Key + new Vector2(-1f, 0f)) && isTileLocationOpenIgnoreFrontLayers(new Location(xCoord - 1, yCoord)) && doesTileHaveProperty(xCoord - 1, yCoord, "Diggable", "Back") != null && doesTileHaveProperty(xCoord - 1, yCoord, "NoSpawn", "Back") == null && Game1.random.NextDouble() < 0.25)
							{
								terrainFeatures.Add(kvp.Key + new Vector2(-1f, 0f), new Grass((byte)((Grass)kvp.Value).grassType, Game1.random.Next(1, 3)));
							}
							if (isTileOnMap(xCoord, yCoord) && !isTileOccupied(kvp.Key + new Vector2(1f, 0f)) && isTileLocationOpenIgnoreFrontLayers(new Location(xCoord + 1, yCoord)) && doesTileHaveProperty(xCoord + 1, yCoord, "Diggable", "Back") != null && doesTileHaveProperty(xCoord + 1, yCoord, "NoSpawn", "Back") == null && Game1.random.NextDouble() < 0.25)
							{
								terrainFeatures.Add(kvp.Key + new Vector2(1f, 0f), new Grass((byte)((Grass)kvp.Value).grassType, Game1.random.Next(1, 3)));
							}
							if (isTileOnMap(xCoord, yCoord) && !isTileOccupied(kvp.Key + new Vector2(0f, 1f)) && isTileLocationOpenIgnoreFrontLayers(new Location(xCoord, yCoord + 1)) && doesTileHaveProperty(xCoord, yCoord + 1, "Diggable", "Back") != null && doesTileHaveProperty(xCoord, yCoord + 1, "NoSpawn", "Back") == null && Game1.random.NextDouble() < 0.25)
							{
								terrainFeatures.Add(kvp.Key + new Vector2(0f, 1f), new Grass((byte)((Grass)kvp.Value).grassType, Game1.random.Next(1, 3)));
							}
							if (isTileOnMap(xCoord, yCoord) && !isTileOccupied(kvp.Key + new Vector2(0f, -1f)) && isTileLocationOpenIgnoreFrontLayers(new Location(xCoord, yCoord - 1)) && doesTileHaveProperty(xCoord, yCoord - 1, "Diggable", "Back") != null && doesTileHaveProperty(xCoord, yCoord - 1, "NoSpawn", "Back") == null && Game1.random.NextDouble() < 0.25)
							{
								terrainFeatures.Add(kvp.Key + new Vector2(0f, -1f), new Grass((byte)((Grass)kvp.Value).grassType, Game1.random.Next(1, 3)));
							}
						}
					}
				}
			}
		}

		public void removeDamageDebris(Monster monster)
		{
			debris.Filter((Debris d) => d.toHover == null || !d.toHover.Equals(monster) || d.nonSpriteChunkColor.Equals(Color.Yellow) || !(d.timeSinceDoneBouncing > 900f));
		}

		public void spawnWeeds(bool weedsOnly)
		{
			int numberOfNewWeeds = Game1.random.Next(isFarm ? 5 : 2, isFarm ? 12 : 6);
			if (Game1.dayOfMonth == 1 && Game1.currentSeason.Equals("spring"))
			{
				numberOfNewWeeds *= 15;
			}
			if (name.Equals("Desert"))
			{
				numberOfNewWeeds = ((Game1.random.NextDouble() < 0.1) ? 1 : 0);
			}
			for (int i = 0; i < numberOfNewWeeds; i++)
			{
				int numberOfTries = 0;
				while (numberOfTries < 3)
				{
					int xCoord = Game1.random.Next(map.DisplayWidth / 64);
					int yCoord = Game1.random.Next(map.DisplayHeight / 64);
					Vector2 location = new Vector2(xCoord, yCoord);
					objects.TryGetValue(location, out Object o);
					int grass = -1;
					int tree = -1;
					if (name.Equals("Desert"))
					{
						if (!(Game1.random.NextDouble() < 0.5))
						{
						}
					}
					else if (Game1.random.NextDouble() < 0.15 + (weedsOnly ? 0.05 : 0.0))
					{
						grass = 1;
					}
					else if (!weedsOnly && Game1.random.NextDouble() < 0.35)
					{
						tree = 1;
					}
					else if (!weedsOnly && !isFarm && Game1.random.NextDouble() < 0.35)
					{
						tree = 2;
					}
					if (tree != -1)
					{
						if (this is Farm && Game1.random.NextDouble() < 0.25)
						{
							return;
						}
					}
					else if (o == null && doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null && isTileLocationOpen(new Location(xCoord, yCoord)) && !isTileOccupied(location) && doesTileHaveProperty(xCoord, yCoord, "Water", "Back") == null)
					{
						string noSpawn = doesTileHaveProperty(xCoord, yCoord, "NoSpawn", "Back");
						if (noSpawn != null && (noSpawn.Equals("Grass") || noSpawn.Equals("All") || noSpawn.Equals("True")))
						{
							continue;
						}
						if (grass != -1 && !GetSeasonForLocation().Equals("winter") && name.Equals("Farm"))
						{
							int numberOfWeeds = Game1.random.Next(1, 3);
							terrainFeatures.Add(location, new Grass(grass, numberOfWeeds));
						}
					}
					numberOfTries++;
				}
			}
		}

		public bool addCharacterAtRandomLocation(NPC n)
		{
			Vector2 tileLocationAttempt = new Vector2(Game1.random.Next(0, map.GetLayer("Back").LayerWidth), Game1.random.Next(0, map.GetLayer("Back").LayerHeight));
			int attempts;
			for (attempts = 0; attempts < 6; attempts++)
			{
				if (!isTileOccupied(tileLocationAttempt) && isTilePassable(new Location((int)tileLocationAttempt.X, (int)tileLocationAttempt.Y), Game1.viewport) && map.GetLayer("Back").Tiles[(int)tileLocationAttempt.X, (int)tileLocationAttempt.Y] != null && !map.GetLayer("Back").Tiles[(int)tileLocationAttempt.X, (int)tileLocationAttempt.Y].Properties.ContainsKey("NPCBarrier"))
				{
					break;
				}
				tileLocationAttempt = new Vector2(Game1.random.Next(0, map.GetLayer("Back").LayerWidth), Game1.random.Next(0, map.GetLayer("Back").LayerHeight));
			}
			if (attempts < 6)
			{
				n.Position = tileLocationAttempt * new Vector2(64f, 64f) - new Vector2(0f, n.Sprite.SpriteHeight - 64);
				addCharacter(n);
				return true;
			}
			return false;
		}

		public virtual void OnMiniJukeboxAdded()
		{
			miniJukeboxCount.Value += 1;
			UpdateMiniJukebox();
		}

		public virtual void OnMiniJukeboxRemoved()
		{
			miniJukeboxCount.Value -= 1;
			UpdateMiniJukebox();
		}

		public virtual void UpdateMiniJukebox()
		{
			if (miniJukeboxCount.Value <= 0)
			{
				miniJukeboxCount.Set(0);
				miniJukeboxTrack.Set("");
			}
		}

		public virtual bool IsMiniJukeboxPlaying()
		{
			if (miniJukeboxCount.Value > 0 && miniJukeboxTrack.Value != "")
			{
				if (IsOutdoors)
				{
					return !Game1.IsRainingHere(this);
				}
				return true;
			}
			return false;
		}

		public virtual void DayUpdate(int dayOfMonth)
		{
			netAudio.StopPlaying("fuse");
			SelectRandomMiniJukeboxTrack();
			if (critters != null)
			{
				critters.Clear();
			}
			int i = 0;
			while (i < debris.Count)
			{
				Debris d2 = debris[i];
				if (d2.isEssentialItem() && Game1.IsMasterGame)
				{
					if (Utility.IsNormalObjectAtParentSheetIndex(d2.item, 73))
					{
						d2.collect(Game1.player);
					}
					else
					{
						Item item = d2.item;
						d2.item = null;
						Game1.player.team.returnedDonations.Add(item);
						Game1.player.team.newLostAndFoundItems.Value = true;
					}
					debris.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
			updateMap();
			new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			temporarySprites.Clear();
			KeyValuePair<Vector2, TerrainFeature>[] map_features = terrainFeatures.Pairs.ToArray();
			KeyValuePair<Vector2, TerrainFeature>[] array = map_features;
			for (int n = 0; n < array.Length; n++)
			{
				KeyValuePair<Vector2, TerrainFeature> pair = array[n];
				if (!isTileOnMap(pair.Key))
				{
					terrainFeatures.Remove(pair.Key);
				}
				else
				{
					pair.Value.dayUpdate(this, pair.Key);
				}
			}
			array = map_features;
			for (int n = 0; n < array.Length; n++)
			{
				KeyValuePair<Vector2, TerrainFeature> pair2 = array[n];
				HoeDirt hoe_dirt;
				if ((hoe_dirt = (pair2.Value as HoeDirt)) != null)
				{
					hoe_dirt.updateNeighbors(this, pair2.Key);
				}
			}
			if (largeTerrainFeatures != null)
			{
				LargeTerrainFeature[] array2 = largeTerrainFeatures.ToArray();
				for (int n = 0; n < array2.Length; n++)
				{
					array2[n].dayUpdate(this);
				}
			}
			List<KeyValuePair<Vector2, Object>> objects_to_remove = new List<KeyValuePair<Vector2, Object>>();
			for (int l = objects.Count() - 1; l >= 0; l--)
			{
				objects.Pairs.ElementAt(l).Value.DayUpdate(this);
				if (objects.Pairs.ElementAt(l).Value.destroyOvernight)
				{
					objects_to_remove.Add(objects.Pairs.ElementAt(l));
				}
			}
			foreach (KeyValuePair<Vector2, Object> kvp in objects_to_remove)
			{
				if (objects.ContainsKey(kvp.Key) && objects[kvp.Key] == kvp.Value)
				{
					if (kvp.Value != null)
					{
						kvp.Value.performRemoveAction(kvp.Key, this);
					}
					objects.Remove(kvp.Key);
				}
			}
			if (!(this is FarmHouse))
			{
				debris.Filter((Debris d) => d.item != null);
			}
			if (((bool)isOutdoors || map.Properties.ContainsKey("ForceSpawnForageables")) && !map.Properties.ContainsKey("skipWeedGrowth"))
			{
				if (Game1.dayOfMonth % 7 == 0 && !(this is Farm))
				{
					Microsoft.Xna.Framework.Rectangle ignoreRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0);
					if (this is IslandWest)
					{
						ignoreRectangle = new Microsoft.Xna.Framework.Rectangle(31, 3, 77, 66);
					}
					for (int m = objects.Count() - 1; m >= 0; m--)
					{
						if ((bool)objects.Pairs.ElementAt(m).Value.isSpawnedObject && !ignoreRectangle.Contains(Utility.Vector2ToPoint(objects.Pairs.ElementAt(m).Key)))
						{
							objects.Remove(objects.Pairs.ElementAt(m).Key);
						}
					}
					numberOfSpawnedObjectsOnMap = 0;
					spawnObjects();
					spawnObjects();
				}
				spawnObjects();
				if (Game1.dayOfMonth == 1)
				{
					spawnObjects();
				}
				if (Game1.stats.DaysPlayed < 4)
				{
					spawnObjects();
				}
				bool hasPathsLayer = false;
				foreach (Layer layer in map.Layers)
				{
					if (layer.Id.Equals("Paths"))
					{
						hasPathsLayer = true;
						break;
					}
				}
				if (hasPathsLayer && !(this is Farm))
				{
					for (int x = 0; x < map.Layers[0].LayerWidth; x++)
					{
						for (int y = 0; y < map.Layers[0].LayerHeight; y++)
						{
							if (map.GetLayer("Paths").Tiles[x, y] == null || !(Game1.random.NextDouble() < 0.5))
							{
								continue;
							}
							Vector2 tile = new Vector2(x, y);
							int treeType = -1;
							switch (map.GetLayer("Paths").Tiles[x, y].TileIndex)
							{
							case 9:
								treeType = 1;
								if (Game1.currentSeason.Equals("winter"))
								{
									treeType += 3;
								}
								break;
							case 10:
								treeType = 2;
								if (Game1.currentSeason.Equals("winter"))
								{
									treeType += 3;
								}
								break;
							case 11:
								treeType = 3;
								break;
							case 12:
								treeType = 6;
								break;
							case 31:
								treeType = 9;
								break;
							case 32:
								treeType = 8;
								break;
							}
							if (treeType != -1 && GetFurnitureAt(tile) == null && !terrainFeatures.ContainsKey(tile) && !objects.ContainsKey(tile))
							{
								terrainFeatures.Add(tile, new Tree(treeType, 2));
							}
						}
					}
				}
			}
			if (!isFarm && !SeedsIgnoreSeasonsHere())
			{
				ICollection<Vector2> objKeys = new List<Vector2>(terrainFeatures.Keys);
				for (int k = objKeys.Count - 1; k >= 0; k--)
				{
					if (terrainFeatures[objKeys.ElementAt(k)] is HoeDirt && ((terrainFeatures[objKeys.ElementAt(k)] as HoeDirt).crop == null || (bool)(terrainFeatures[objKeys.ElementAt(k)] as HoeDirt).crop.forageCrop))
					{
						terrainFeatures.Remove(objKeys.ElementAt(k));
					}
				}
			}
			for (int j = characters.Count - 1; j >= 0; j--)
			{
			}
			lightLevel.Value = 0f;
			name.Equals("BugLand");
			foreach (Furniture item2 in furniture)
			{
				item2.minutesElapsed(Utility.CalculateMinutesUntilMorning(Game1.timeOfDay), this);
				item2.DayUpdate(this);
			}
			addLightGlows();
		}

		public void addLightGlows()
		{
			int night_tiles_time = Game1.getTrulyDarkTime();
			if ((bool)isOutdoors || (Game1.timeOfDay >= night_tiles_time && !Game1.newDay))
			{
				return;
			}
			lightGlows.Clear();
			map.Properties.TryGetValue("DayTiles", out PropertyValue dayTiles);
			if (dayTiles == null)
			{
				return;
			}
			string[] split = dayTiles.ToString().Trim().Split(' ');
			for (int i = 0; i < split.Length; i += 4)
			{
				if (map.GetLayer(split[i]).PickTile(new Location(Convert.ToInt32(split[i + 1]) * 64, Convert.ToInt32(split[i + 2]) * 64), new Size(Game1.viewport.Width, Game1.viewport.Height)) != null)
				{
					map.GetLayer(split[i]).Tiles[Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])].TileIndex = Convert.ToInt32(split[i + 3]);
					switch (Convert.ToInt32(split[i + 3]))
					{
					case 257:
						lightGlows.Add(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])) * 64f + new Vector2(32f, -4f));
						break;
					case 256:
						lightGlows.Add(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])) * 64f + new Vector2(32f, 64f));
						break;
					case 405:
						lightGlows.Add(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])) * 64f + new Vector2(32f, 32f));
						lightGlows.Add(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])) * 64f + new Vector2(96f, 32f));
						break;
					case 469:
						lightGlows.Add(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])) * 64f + new Vector2(32f, 36f));
						break;
					case 1224:
						lightGlows.Add(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])) * 64f + new Vector2(32f, 32f));
						break;
					}
				}
			}
		}

		public NPC isCharacterAtTile(Vector2 tileLocation)
		{
			NPC c = null;
			tileLocation.X = (int)tileLocation.X;
			tileLocation.Y = (int)tileLocation.Y;
			if (currentEvent == null)
			{
				foreach (NPC j in characters)
				{
					if (j.getTileLocation().Equals(tileLocation))
					{
						return j;
					}
				}
				return c;
			}
			foreach (NPC i in currentEvent.actors)
			{
				if (i.getTileLocation().Equals(tileLocation))
				{
					return i;
				}
			}
			return c;
		}

		public void ResetCharacterDialogues()
		{
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				characters[i].resetCurrentDialogue();
			}
		}

		public string getMapProperty(string propertyName)
		{
			PropertyValue value = null;
			Map.Properties.TryGetValue(propertyName, out value);
			if (value == null)
			{
				return "";
			}
			return value.ToString();
		}

		public virtual void tryToAddCritters(bool onlyIfOnScreen = false)
		{
			if (Game1.CurrentEvent != null)
			{
				return;
			}
			double mapArea = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight;
			double butterflyChance;
			double birdieChance;
			double num = butterflyChance = (birdieChance = Math.Max(0.15, Math.Min(0.5, mapArea / 15000.0)));
			double bunnyChance = num / 2.0;
			double squirrelChance = num / 2.0;
			double woodPeckerChance = num / 8.0;
			double cloudChange = num * 2.0;
			if (Game1.IsRainingHere(this))
			{
				return;
			}
			addClouds(cloudChange / (double)(onlyIfOnScreen ? 2f : 1f), onlyIfOnScreen);
			if (!(this is Beach) && critters != null && critters.Count <= (Game1.currentSeason.Equals("summer") ? 20 : 10))
			{
				addBirdies(birdieChance, onlyIfOnScreen);
				addButterflies(butterflyChance, onlyIfOnScreen);
				addBunnies(bunnyChance, onlyIfOnScreen);
				addSquirrels(squirrelChance, onlyIfOnScreen);
				addWoodpecker(woodPeckerChance, onlyIfOnScreen);
				if (Game1.isDarkOut() && Game1.random.NextDouble() < 0.01)
				{
					addOwl();
				}
			}
		}

		public void addClouds(double chance, bool onlyIfOnScreen = false)
		{
			if (!Game1.currentSeason.Equals("summer") || Game1.IsRainingHere(this) || Game1.weatherIcon == 4 || Game1.timeOfDay >= Game1.getStartingToGetDarkTime() - 100)
			{
				return;
			}
			while (Game1.random.NextDouble() < Math.Min(0.9, chance))
			{
				Vector2 v = getRandomTile();
				if (onlyIfOnScreen)
				{
					v = ((Game1.random.NextDouble() < 0.5) ? new Vector2(map.Layers[0].LayerWidth, Game1.random.Next(map.Layers[0].LayerHeight)) : new Vector2(Game1.random.Next(map.Layers[0].LayerWidth), map.Layers[0].LayerHeight));
				}
				if (onlyIfOnScreen || !Utility.isOnScreen(v * 64f, 1280))
				{
					Cloud cloud = new Cloud(v);
					bool freeToAdd = true;
					if (critters != null)
					{
						foreach (Critter c in critters)
						{
							if (c is Cloud && c.getBoundingBox(0, 0).Intersects(cloud.getBoundingBox(0, 0)))
							{
								freeToAdd = false;
								break;
							}
						}
					}
					if (freeToAdd)
					{
						addCritter(cloud);
					}
				}
			}
		}

		public void addOwl()
		{
			critters.Add(new Owl(new Vector2(Game1.random.Next(64, map.Layers[0].LayerWidth * 64 - 64), -128f)));
		}

		public void setFireplace(bool on, int tileLocationX, int tileLocationY, bool playSound = true)
		{
			int fireid = 944468 + tileLocationX * 1000 + tileLocationY;
			if (on)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(tileLocationX, tileLocationY) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = fireid,
					id = fireid,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = ((float)tileLocationY + 1.1f) * 64f / 10000f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(tileLocationX + 1, tileLocationY) * 64f + new Vector2(-16f, -32f), flipped: false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = fireid,
					id = fireid,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = ((float)tileLocationY + 1.1f) * 64f / 10000f
				});
				if (playSound && Game1.gameMode != 6)
				{
					localSound("fireball");
				}
				AmbientLocationSounds.addSound(new Vector2(tileLocationX, tileLocationY), 1);
			}
			else
			{
				removeTemporarySpritesWithID(fireid);
				Utility.removeLightSource(fireid);
				if (playSound)
				{
					localSound("fireball");
				}
				AmbientLocationSounds.removeSound(new Vector2(tileLocationX, tileLocationY));
			}
		}

		public void addWoodpecker(double chance, bool onlyIfOnScreen = false)
		{
			if (Game1.isStartingToGetDarkOut() || onlyIfOnScreen || this is Town || this is Desert || !(Game1.random.NextDouble() < chance))
			{
				return;
			}
			int i = 0;
			int index;
			while (true)
			{
				if (i < 3)
				{
					index = Game1.random.Next(terrainFeatures.Count());
					if (terrainFeatures.Count() > 0 && terrainFeatures.Pairs.ElementAt(index).Value is Tree && (int)(terrainFeatures.Pairs.ElementAt(index).Value as Tree).treeType != 2 && (int)(terrainFeatures.Pairs.ElementAt(index).Value as Tree).growthStage >= 5)
					{
						break;
					}
					i++;
					continue;
				}
				return;
			}
			critters.Add(new Woodpecker(terrainFeatures.Pairs.ElementAt(index).Value as Tree, terrainFeatures.Pairs.ElementAt(index).Key));
		}

		public void addSquirrels(double chance, bool onlyIfOnScreen = false)
		{
			if (Game1.isStartingToGetDarkOut() || onlyIfOnScreen || this is Farm || this is Town || this is Desert || !(Game1.random.NextDouble() < chance))
			{
				return;
			}
			int j = 0;
			Vector2 pos;
			bool flip;
			while (true)
			{
				if (j >= 3)
				{
					return;
				}
				int index = Game1.random.Next(terrainFeatures.Count());
				if (terrainFeatures.Count() > 0 && terrainFeatures.Pairs.ElementAt(index).Value is Tree && (int)(terrainFeatures.Pairs.ElementAt(index).Value as Tree).growthStage >= 5 && !(terrainFeatures.Pairs.ElementAt(index).Value as Tree).stump)
				{
					pos = terrainFeatures.Pairs.ElementAt(index).Key;
					int distance = Game1.random.Next(4, 7);
					flip = (Game1.random.NextDouble() < 0.5);
					bool success = true;
					for (int i = 0; i < distance; i++)
					{
						pos.X += (flip ? 1 : (-1));
						if (!isTileLocationTotallyClearAndPlaceable(pos))
						{
							success = false;
							break;
						}
					}
					if (success)
					{
						break;
					}
				}
				j++;
			}
			critters.Add(new Squirrel(pos, flip));
		}

		public void addBunnies(double chance, bool onlyIfOnScreen = false)
		{
			if (onlyIfOnScreen || this is Farm || this is Desert || !(Game1.random.NextDouble() < chance) || largeTerrainFeatures == null)
			{
				return;
			}
			int i = 0;
			Vector2 pos;
			bool flip;
			while (true)
			{
				if (i >= 3)
				{
					return;
				}
				int index = Game1.random.Next(largeTerrainFeatures.Count);
				if (largeTerrainFeatures.Count > 0 && largeTerrainFeatures[index] is Bush)
				{
					pos = largeTerrainFeatures[index].tilePosition;
					int distance = Game1.random.Next(5, 12);
					flip = (Game1.random.NextDouble() < 0.5);
					bool success = true;
					for (int j = 0; j < distance; j++)
					{
						pos.X += (flip ? 1 : (-1));
						if (!largeTerrainFeatures[index].getBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)pos.X * 64, (int)pos.Y * 64, 64, 64)) && !isTileLocationTotallyClearAndPlaceable(pos))
						{
							success = false;
							break;
						}
					}
					if (success)
					{
						break;
					}
				}
				i++;
			}
			critters.Add(new Rabbit(pos, flip));
		}

		public void instantiateCrittersList()
		{
			if (critters == null)
			{
				critters = new List<Critter>();
			}
		}

		public void addCritter(Critter c)
		{
			if (critters != null)
			{
				critters.Add(c);
			}
		}

		public void addButterflies(double chance, bool onlyIfOnScreen = false)
		{
			bool island_location = GetLocationContext() == LocationContext.Island;
			bool firefly = (Game1.currentSeason.Equals("summer") | island_location) && Game1.isDarkOut();
			if ((Game1.timeOfDay >= 1500 && !firefly) || !((Game1.currentSeason.Equals("spring") || Game1.currentSeason.Equals("summer")) | island_location))
			{
				return;
			}
			chance = Math.Min(0.8, chance * 1.5);
			while (Game1.random.NextDouble() < chance)
			{
				Vector2 v = getRandomTile();
				if (onlyIfOnScreen && Utility.isOnScreen(v * 64f, 64))
				{
					continue;
				}
				if (firefly)
				{
					critters.Add(new Firefly(v));
				}
				else
				{
					critters.Add(new Butterfly(v, island_location));
				}
				while (Game1.random.NextDouble() < 0.4)
				{
					if (firefly)
					{
						critters.Add(new Firefly(v + new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3))));
					}
					else
					{
						critters.Add(new Butterfly(v + new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3)), island_location));
					}
				}
			}
		}

		public void addBirdies(double chance, bool onlyIfOnScreen = false)
		{
			if (Game1.timeOfDay >= 1500 || this is Desert || this is Railroad || this is Farm || Game1.currentSeason.Equals("summer"))
			{
				return;
			}
			while (Game1.random.NextDouble() < chance)
			{
				int birdiesToAdd = Game1.random.Next(1, 4);
				bool success = false;
				int tries = 0;
				while (!success && tries < 5)
				{
					Vector2 randomTile = getRandomTile();
					if (!onlyIfOnScreen || !Utility.isOnScreen(randomTile * 64f, 64))
					{
						Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle((int)randomTile.X - 2, (int)randomTile.Y - 2, 5, 5);
						if (isAreaClear(area))
						{
							List<Critter> crittersToAdd = new List<Critter>();
							int whichBird = Game1.currentSeason.Equals("fall") ? 45 : 25;
							if (Game1.random.NextDouble() < 0.5 && Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
							{
								whichBird = (Game1.currentSeason.Equals("fall") ? 135 : 125);
							}
							for (int i = 0; i < birdiesToAdd; i++)
							{
								crittersToAdd.Add(new Birdie(-100, -100, whichBird));
							}
							addCrittersStartingAtTile(randomTile, crittersToAdd);
							success = true;
						}
					}
					tries++;
				}
			}
		}

		public void addJumperFrog(Vector2 tileLocation)
		{
			if (critters != null)
			{
				critters.Add(new Frog(tileLocation));
			}
		}

		public void addFrog()
		{
			if (!Game1.IsRainingHere(this) || Game1.currentSeason.Equals("winter"))
			{
				return;
			}
			for (int j = 0; j < 3; j++)
			{
				Vector2 v = getRandomTile();
				if (doesTileHaveProperty((int)v.X, (int)v.Y, "Water", "Back") == null || doesTileHaveProperty((int)v.X, (int)v.Y - 1, "Water", "Back") == null || doesTileHaveProperty((int)v.X, (int)v.Y, "Passable", "Buildings") != null)
				{
					continue;
				}
				int distanceToCheck = 10;
				bool flip = Game1.random.NextDouble() < 0.5;
				for (int i = 0; i < distanceToCheck; i++)
				{
					v.X += (flip ? 1 : (-1));
					if (isTileOnMap((int)v.X, (int)v.Y) && doesTileHaveProperty((int)v.X, (int)v.Y, "Water", "Back") == null)
					{
						critters.Add(new Frog(v, waterLeaper: true, flip));
						return;
					}
				}
			}
		}

		public void checkForSpecialCharacterIconAtThisTile(Vector2 tileLocation)
		{
			if (currentEvent != null)
			{
				currentEvent.checkForSpecialCharacterIconAtThisTile(tileLocation);
			}
		}

		private void addCrittersStartingAtTile(Vector2 tile, List<Critter> crittersToAdd)
		{
			if (crittersToAdd == null)
			{
				return;
			}
			int tries = 0;
			HashSet<Vector2> tried_tiles = new HashSet<Vector2>();
			while (crittersToAdd.Count > 0 && tries < 20)
			{
				if (tried_tiles.Contains(tile))
				{
					tile = Utility.getTranslatedVector2(tile, Game1.random.Next(4), 1f);
				}
				else
				{
					if (isTileLocationTotallyClearAndPlaceable(tile))
					{
						crittersToAdd.Last().position = tile * 64f;
						crittersToAdd.Last().startingPosition = tile * 64f;
						critters.Add(crittersToAdd.Last());
						crittersToAdd.RemoveAt(crittersToAdd.Count - 1);
					}
					tile = Utility.getTranslatedVector2(tile, Game1.random.Next(4), 1f);
					tried_tiles.Add(tile);
				}
				tries++;
			}
		}

		public bool isAreaClear(Microsoft.Xna.Framework.Rectangle area)
		{
			for (int x = area.Left; x < area.Right; x++)
			{
				for (int y = area.Top; y < area.Bottom; y++)
				{
					if (!isTileLocationTotallyClearAndPlaceable(new Vector2(x, y)))
					{
						return false;
					}
				}
			}
			return true;
		}

		public void refurbishMapPortion(Microsoft.Xna.Framework.Rectangle areaToRefurbish, string refurbishedMapName, Point mapReaderStartPoint)
		{
			Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\" + refurbishedMapName);
			Point mapReader = mapReaderStartPoint;
			map.Properties.Remove("DayTiles");
			map.Properties.Remove("NightTiles");
			for (int x = 0; x < areaToRefurbish.Width; x++)
			{
				for (int y = 0; y < areaToRefurbish.Height; y++)
				{
					if (refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(map.GetLayer("Back"), map.TileSheets[0], BlendMode.Alpha, refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
						foreach (string v2 in refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y].Properties.Keys)
						{
							map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y].Properties.Add(v2, refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y].Properties[v2]);
						}
					}
					if (refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
						adjustMapLightPropertiesForLamp(refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings");
						foreach (string v in refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].Properties.Keys)
						{
							map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y].Properties.Add(v, refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y].Properties[v]);
						}
					}
					else
					{
						map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
					}
					if (y < areaToRefurbish.Height - 1 && refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
						adjustMapLightPropertiesForLamp(refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front");
					}
					else if (y < areaToRefurbish.Height - 1)
					{
						map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
					}
				}
			}
		}

		public Vector2 getRandomTile()
		{
			return new Vector2(Game1.random.Next(Map.Layers[0].LayerWidth), Game1.random.Next(Map.Layers[0].LayerHeight));
		}

		public void setUpLocationSpecificFlair()
		{
			if (!isOutdoors && !(this is FarmHouse) && !(this is IslandFarmHouse))
			{
				map.Properties.TryGetValue("AmbientLight", out PropertyValue ambientLight);
				if (ambientLight == null)
				{
					Game1.ambientLight = new Color(100, 120, 30);
				}
			}
			switch ((string)name)
			{
			case "Summit":
				Game1.ambientLight = Color.Black;
				break;
			case "QiNutRoom":
				Game1.changeMusicTrack("clubloop", track_interruptable: false, Game1.MusicContext.SubLocation);
				Game1.ambientLight = new Color(100, 120, 30);
				break;
			case "LeoTreeHouse":
				temporarySprites.Add(new EmilysParrot(new Vector2(88f, 224f))
				{
					layerDepth = 1f,
					id = 5858585f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(71, 334, 12, 11), new Vector2(304f, 32f), flipped: false, 0f, Color.White)
				{
					layerDepth = 0.001f,
					interval = 700f,
					animationLength = 3,
					totalNumberOfLoops = 999999,
					scale = 4f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(47, 334, 12, 11), new Vector2(112f, -25.6f), flipped: true, 0f, Color.White)
				{
					layerDepth = 0.001f,
					interval = 300f,
					animationLength = 3,
					totalNumberOfLoops = 999999,
					scale = 4f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(71, 334, 12, 11), new Vector2(224f, -25.6f), flipped: false, 0f, Color.White)
				{
					layerDepth = 0.001f,
					interval = 800f,
					animationLength = 3,
					totalNumberOfLoops = 999999,
					scale = 4f
				});
				Game1.changeMusicTrack("sad_kid", track_interruptable: true, Game1.MusicContext.SubLocation);
				break;
			case "Sunroom":
				Game1.ambientLight = new Color(0, 0, 0);
				AmbientLocationSounds.addSound(new Vector2(3f, 4f), 0);
				if (largeTerrainFeatures.Count == 0)
				{
					Bush b = new Bush(new Vector2(6f, 7f), 3, this, -999);
					b.greenhouseBush.Value = true;
					b.loadSprite();
					b.health = 99f;
					largeTerrainFeatures.Add(b);
				}
				if (!Game1.IsRainingHere(this))
				{
					Game1.changeMusicTrack("SunRoom", track_interruptable: false, Game1.MusicContext.SubLocation);
					critters = new List<Critter>();
					critters.Add(new Butterfly(getRandomTile()).setStayInbounds(stayInbounds: true));
					while (Game1.random.NextDouble() < 0.5)
					{
						critters.Add(new Butterfly(getRandomTile()).setStayInbounds(stayInbounds: true));
					}
				}
				break;
			case "AbandonedJojaMart":
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					Point position = new Point(8, 8);
					Game1.currentLightSources.Add(new LightSource(4, new Vector2(position.X * 64, position.Y * 64), 1f, LightSource.LightContext.None, 0L));
					temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64, position.Y * 64), Color.White)
					{
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f)
					});
					temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64 - 12, position.Y * 64 - 12), Color.White)
					{
						scale = 0.75f,
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = 50
					});
					temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64 - 12, position.Y * 64 + 12), Color.White)
					{
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = 100
					});
					temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64, position.Y * 64), Color.White)
					{
						layerDepth = 1f,
						scale = 0.75f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = 150
					});
					if (characters.Count == 0)
					{
						characters.Add(new Junimo(new Vector2(8f, 7f) * 64f, 6));
					}
				}
				break;
			case "WitchHut":
				if (Game1.player.mailReceived.Contains("cursed_doll") && !farmers.Any())
				{
					characters.Clear();
					addCharacter(new Bat(new Vector2(7f, 6f) * 64f, -666));
					if (Game1.stats.getStat("childrenTurnedToDoves") > 1)
					{
						addCharacter(new Bat(new Vector2(4f, 7f) * 64f, -666));
					}
					if (Game1.stats.getStat("childrenTurnedToDoves") > 2)
					{
						addCharacter(new Bat(new Vector2(10f, 7f) * 64f, -666));
					}
					for (int i = 4; i <= Game1.stats.getStat("childrenTurnedToDoves"); i++)
					{
						addCharacter(new Bat(Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(1, 4, 13, 4), Game1.random) * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), -666));
					}
				}
				break;
			case "BugLand":
				if (!Game1.player.hasDarkTalisman && isTileLocationTotallyClearAndPlaceable(31, 5))
				{
					overlayObjects.Add(new Vector2(31f, 5f), new Chest(0, new List<Item>
					{
						new SpecialItem(6)
					}, new Vector2(31f, 5f))
					{
						Tint = Color.Gray
					});
				}
				foreach (NPC j in characters)
				{
					if (j is Grub)
					{
						(j as Grub).setHard();
					}
					else if (j is Fly)
					{
						(j as Fly).setHard();
					}
				}
				break;
			case "HaleyHouse":
				if (Game1.player.eventsSeen.Contains(463391) && (Game1.player.spouse == null || !Game1.player.spouse.Equals("Emily")))
				{
					temporarySprites.Add(new EmilysParrot(new Vector2(912f, 160f)));
				}
				break;
			case "ScienceHouse":
				if (Game1.random.NextDouble() < 0.5 && Game1.player.currentLocation != null && (bool)Game1.player.currentLocation.isOutdoors)
				{
					NPC p = Game1.getCharacterFromName("Robin");
					if (p != null && p.getTileY() == 18)
					{
						string toSay = "";
						switch (Game1.random.Next(4))
						{
						case 0:
							toSay = (Game1.isRaining ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Raining1" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotRaining1");
							break;
						case 1:
							toSay = (Game1.isSnowing ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Snowing" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotSnowing");
							break;
						case 2:
							toSay = ((Game1.player.getFriendshipHeartLevelForNPC("Robin") > 4) ? "Strings\\SpeechBubbles:ScienceHouse_Robin_CloseFriends" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotCloseFriends");
							break;
						case 3:
							toSay = (Game1.isRaining ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Raining2" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotRaining2");
							break;
						case 4:
							toSay = "Strings\\SpeechBubbles:ScienceHouse_Robin_Greeting";
							break;
						}
						if (Game1.random.NextDouble() < 0.001)
						{
							toSay = "Strings\\SpeechBubbles:ScienceHouse_Robin_RareGreeting";
						}
						p.showTextAboveHead(Game1.content.LoadString(toSay, Game1.player.Name));
					}
				}
				if (getCharacterFromName("Robin") == null && Game1.IsVisitingIslandToday("Robin"))
				{
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(129f, 210f),
						interval = 50000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(7f, 18f) * 64f + new Vector2(3f, 0f) * 4f,
						scale = 4f,
						layerDepth = 0.1281f,
						id = 777f
					});
				}
				break;
			case "CommunityCenter":
				if (this is CommunityCenter && (Game1.isLocationAccessible("CommunityCenter") || (currentEvent != null && currentEvent.id == 191393)))
				{
					setFireplace(on: true, 31, 8, playSound: false);
					setFireplace(on: true, 32, 8, playSound: false);
					setFireplace(on: true, 33, 8, playSound: false);
				}
				break;
			case "AnimalShop":
				setFireplace(on: true, 3, 14, playSound: false);
				if (Game1.random.NextDouble() < 0.5)
				{
					NPC p2 = Game1.getCharacterFromName("Marnie");
					if (p2 != null && p2.getTileY() == 14)
					{
						string toSay2 = "";
						switch (Game1.random.Next(4))
						{
						case 0:
							toSay2 = "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting1";
							break;
						case 1:
							toSay2 = "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting2";
							break;
						case 2:
							toSay2 = ((Game1.player.getFriendshipHeartLevelForNPC("Marnie") > 4) ? "Strings\\SpeechBubbles:AnimalShop_Marnie_CloseFriends" : "Strings\\SpeechBubbles:AnimalShop_Marnie_NotCloseFriends");
							break;
						case 3:
							toSay2 = (Game1.isRaining ? "Strings\\SpeechBubbles:AnimalShop_Marnie_Raining" : "Strings\\SpeechBubbles:AnimalShop_Marnie_NotRaining");
							break;
						case 4:
							toSay2 = "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting3";
							break;
						}
						if (Game1.random.NextDouble() < 0.001)
						{
							toSay2 = "Strings\\SpeechBubbles:AnimalShop_Marnie_RareGreeting";
						}
						p2.showTextAboveHead(Game1.content.LoadString(toSay2, Game1.player.Name, Game1.player.farmName));
					}
				}
				if (getCharacterFromName("Marnie") == null && Game1.IsVisitingIslandToday("Marnie"))
				{
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(129f, 210f),
						interval = 50000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(13f, 14f) * 64f + new Vector2(3f, 0f) * 4f,
						scale = 4f,
						layerDepth = 0.1025f,
						id = 777f
					});
				}
				if (Game1.netWorldState.Value.hasWorldStateID("m_painting0"))
				{
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(25, 1925, 25, 23),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(25f, 1925f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
						scale = 4f,
						layerDepth = 0.1f,
						id = 777f
					});
				}
				else if (Game1.netWorldState.Value.hasWorldStateID("m_painting1"))
				{
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 1925, 25, 23),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(0f, 1925f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
						scale = 4f,
						layerDepth = 0.1f,
						id = 777f
					});
				}
				else if (Game1.netWorldState.Value.hasWorldStateID("m_painting2"))
				{
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 1948, 25, 24),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(0f, 1948f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
						scale = 4f,
						layerDepth = 0.1f,
						id = 777f
					});
				}
				break;
			case "AdventureGuild":
			{
				setFireplace(on: true, 9, 11, playSound: false);
				if (!(Game1.random.NextDouble() < 0.5))
				{
					break;
				}
				NPC p3 = Game1.getCharacterFromName("Marlon");
				if (p3 != null)
				{
					string toSay3 = "";
					switch (Game1.random.Next(5))
					{
					case 0:
						toSay3 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting_" + (Game1.player.IsMale ? "Male" : "Female");
						break;
					case 1:
						toSay3 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting1";
						break;
					case 2:
						toSay3 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting2";
						break;
					case 3:
						toSay3 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting3";
						break;
					case 4:
						toSay3 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting4";
						break;
					}
					p3.showTextAboveHead(Game1.content.LoadString(toSay3));
				}
				break;
			}
			case "Blacksmith":
				AmbientLocationSounds.addSound(new Vector2(9f, 10f), 2);
				AmbientLocationSounds.changeSpecificVariable("Frequency", 2f, 2);
				Game1.changeMusicTrack("none");
				break;
			case "Hospital":
			{
				if (!Game1.isRaining)
				{
					Game1.changeMusicTrack("distantBanjo");
				}
				Game1.ambientLight = new Color(100, 100, 60);
				if (!(Game1.random.NextDouble() < 0.5))
				{
					break;
				}
				NPC p4 = Game1.getCharacterFromName("Maru");
				if (p4 != null && p4.currentLocation == this && !p4.isDivorcedFrom(Game1.player))
				{
					string toSay5 = "";
					switch (Game1.random.Next(5))
					{
					case 0:
						toSay5 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting1";
						break;
					case 1:
						toSay5 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting2";
						break;
					case 2:
						toSay5 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting3";
						break;
					case 3:
						toSay5 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting4";
						break;
					case 4:
						toSay5 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting5";
						break;
					}
					if (Game1.player.spouse != null && Game1.player.spouse.Equals("Maru"))
					{
						toSay5 = "Strings\\SpeechBubbles:Hospital_Maru_Spouse";
						p4.showTextAboveHead(Game1.content.LoadString(toSay5), 2);
					}
					else
					{
						p4.showTextAboveHead(Game1.content.LoadString(toSay5));
					}
				}
				break;
			}
			case "LeahHouse":
			{
				Game1.changeMusicTrack("distantBanjo");
				NPC k = Game1.getCharacterFromName("Leah");
				if (Game1.IsFall || Game1.IsWinter || Game1.isRaining)
				{
					setFireplace(on: true, 11, 4, playSound: false);
				}
				if (k != null && k.currentLocation == this && !k.isDivorcedFrom(Game1.player))
				{
					string toSay6 = "";
					switch (Game1.random.Next(3))
					{
					case 0:
						toSay6 = "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting1";
						break;
					case 1:
						toSay6 = "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting2";
						break;
					case 2:
						toSay6 = "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting3";
						break;
					}
					k.faceTowardFarmerForPeriod(3000, 15, faceAway: false, Game1.player);
					k.showTextAboveHead(Game1.content.LoadString(toSay6, Game1.player.Name));
				}
				break;
			}
			case "ElliottHouse":
			{
				Game1.changeMusicTrack("communityCenter");
				NPC e = Game1.getCharacterFromName("Elliott");
				if (e != null && e.currentLocation == this && !e.isDivorcedFrom(Game1.player))
				{
					string toSay7 = "";
					switch (Game1.random.Next(3))
					{
					case 0:
						toSay7 = "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting1";
						break;
					case 1:
						toSay7 = "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting2";
						break;
					case 2:
						toSay7 = "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting3";
						break;
					}
					e.faceTowardFarmerForPeriod(3000, 15, faceAway: false, Game1.player);
					e.showTextAboveHead(Game1.content.LoadString(toSay7, Game1.player.Name));
				}
				break;
			}
			case "JojaMart":
				Game1.changeMusicTrack("Hospital_Ambient");
				Game1.ambientLight = new Color(0, 0, 0);
				if (Game1.random.NextDouble() < 0.5)
				{
					NPC p5 = Game1.getCharacterFromName("Morris");
					if (p5 != null && p5.currentLocation == this)
					{
						string toSay8 = "Strings\\SpeechBubbles:JojaMart_Morris_Greeting";
						p5.showTextAboveHead(Game1.content.LoadString(toSay8));
					}
				}
				break;
			case "SandyHouse":
			{
				Game1.changeMusicTrack("distantBanjo");
				Game1.ambientLight = new Color(0, 0, 0);
				if (!(Game1.random.NextDouble() < 0.5))
				{
					break;
				}
				NPC p6 = Game1.getCharacterFromName("Sandy");
				if (p6 != null && p6.currentLocation == this)
				{
					string toSay9 = "";
					switch (Game1.random.Next(5))
					{
					case 0:
						toSay9 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting1";
						break;
					case 1:
						toSay9 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting2";
						break;
					case 2:
						toSay9 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting3";
						break;
					case 3:
						toSay9 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting4";
						break;
					case 4:
						toSay9 = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting5";
						break;
					}
					p6.showTextAboveHead(Game1.content.LoadString(toSay9));
				}
				break;
			}
			case "ManorHouse":
			{
				Game1.ambientLight = new Color(150, 120, 50);
				NPC le = Game1.getCharacterFromName("Lewis");
				if (le != null && le.currentLocation == this)
				{
					string toSay11 = "";
					toSay11 = ((Game1.timeOfDay < 1200) ? "Morning" : ((Game1.timeOfDay < 1700) ? "Afternoon" : "Evening"));
					le.faceTowardFarmerForPeriod(3000, 15, faceAway: false, Game1.player);
					le.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:ManorHouse_Lewis_" + toSay11));
				}
				break;
			}
			case "Saloon":
				if (Game1.timeOfDay >= 1700)
				{
					setFireplace(on: true, 22, 17, playSound: false);
					Game1.changeMusicTrack("Saloon1");
				}
				if (Game1.random.NextDouble() < 0.25)
				{
					NPC p7 = Game1.getCharacterFromName("Gus");
					if (p7 != null && p7.getTileY() == 18 && p7.currentLocation == this)
					{
						string toSay12 = "";
						switch (Game1.random.Next(5))
						{
						case 0:
							toSay12 = "Greeting";
							break;
						case 1:
							toSay12 = (Game1.IsSummer ? "Summer" : "NotSummer");
							break;
						case 2:
							toSay12 = (Game1.isSnowing ? "Snowing1" : "NotSnowing1");
							break;
						case 3:
							toSay12 = (Game1.isRaining ? "Raining" : "NotRaining");
							break;
						case 4:
							toSay12 = (Game1.isSnowing ? "Snowing2" : "NotSnowing2");
							break;
						}
						if (Game1.random.NextDouble() < 0.001)
						{
							toSay12 = "RareGreeting";
						}
						p7.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:Saloon_Gus_" + toSay12));
					}
				}
				if (getCharacterFromName("Gus") == null && Game1.IsVisitingIslandToday("Gus"))
				{
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(129f, 210f),
						interval = 50000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(11f, 18f) * 64f + new Vector2(3f, 0f) * 4f,
						scale = 4f,
						layerDepth = 0.1281f,
						id = 777f
					});
				}
				if (Game1.dayOfMonth % 7 == 0 && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom") && Game1.timeOfDay < 1500)
				{
					Texture2D tempTxture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 336, 19, 14),
						animationLength = 7,
						sourceRectStartingPos = new Vector2(368f, 336f),
						interval = 5000f,
						totalNumberOfLoops = 99999,
						position = new Vector2(34f, 3f) * 64f + new Vector2(7f, 13f) * 4f,
						scale = 4f,
						layerDepth = 0.0401f,
						id = 2400f
					});
				}
				break;
			case "ArchaeologyHouse":
			{
				setFireplace(on: true, 43, 4, playSound: false);
				if (!(Game1.random.NextDouble() < 0.5) || !Game1.player.hasOrWillReceiveMail("artifactFound"))
				{
					break;
				}
				NPC g = Game1.getCharacterFromName("Gunther");
				if (g != null && g.currentLocation == this)
				{
					string toSay13 = "";
					switch (Game1.random.Next(5))
					{
					case 0:
						toSay13 = "Greeting1";
						break;
					case 1:
						toSay13 = "Greeting2";
						break;
					case 2:
						toSay13 = "Greeting3";
						break;
					case 3:
						toSay13 = "Greeting4";
						break;
					case 4:
						toSay13 = "Greeting5";
						break;
					}
					if (Game1.random.NextDouble() < 0.001)
					{
						toSay13 = "RareGreeting";
					}
					g.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:ArchaeologyHouse_Gunther_" + toSay13));
				}
				break;
			}
			case "Greenhouse":
				if (Game1.isDarkOut())
				{
					Game1.ambientLight = Game1.outdoorLight;
				}
				break;
			case "SeedShop":
				setFireplace(on: true, 25, 13, playSound: false);
				if (Game1.random.NextDouble() < 0.5 && Game1.player.getTileY() > 10)
				{
					NPC p8 = Game1.getCharacterFromName("Pierre");
					if (p8 != null && p8.getTileY() == 17 && p8.currentLocation == this)
					{
						string toSay14 = "";
						switch (Game1.random.Next(5))
						{
						case 0:
							toSay14 = (Game1.IsWinter ? "Winter" : "NotWinter");
							break;
						case 1:
							toSay14 = (Game1.IsSummer ? "Summer" : "NotSummer");
							break;
						case 2:
							toSay14 = "Greeting1";
							break;
						case 3:
							toSay14 = "Greeting2";
							break;
						case 4:
							toSay14 = (Game1.isRaining ? "Raining" : "NotRaining");
							break;
						}
						if (Game1.random.NextDouble() < 0.001)
						{
							toSay14 = "RareGreeting";
						}
						string dialogue = Game1.content.LoadString("Strings\\SpeechBubbles:SeedShop_Pierre_" + toSay14);
						if (dialogue.Contains('^'))
						{
							dialogue = ((!Game1.player.IsMale) ? dialogue.Split('^')[1] : dialogue.Split('^')[0]);
						}
						p8.showTextAboveHead(string.Format(dialogue, Game1.player.Name));
					}
				}
				if (getCharacterFromName("Pierre") == null && Game1.IsVisitingIslandToday("Pierre"))
				{
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(129f, 210f),
						interval = 50000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(5f, 17f) * 64f + new Vector2(3f, 0f) * 4f,
						scale = 4f,
						layerDepth = 0.1217f,
						id = 777f
					});
				}
				break;
			}
		}

		public virtual void hostSetup()
		{
			if (Game1.IsMasterGame && !farmers.Any() && !HasFarmerWatchingBroadcastEventReturningHere())
			{
				interiorDoors.ResetSharedState();
			}
		}

		public virtual bool HasFarmerWatchingBroadcastEventReturningHere()
		{
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.locationBeforeForcedEvent.Value != null && farmer.locationBeforeForcedEvent.Value == NameOrUniqueName)
				{
					return true;
				}
			}
			return false;
		}

		public void resetForPlayerEntry()
		{
			Game1.updateWeatherIcon();
			Game1.hooks.OnGameLocation_ResetForPlayerEntry(this, delegate
			{
				_madeMapModifications = false;
				if (!farmers.Any() && !HasFarmerWatchingBroadcastEventReturningHere())
				{
					resetSharedState();
				}
				resetLocalState();
				if (!_madeMapModifications)
				{
					_madeMapModifications = true;
					MakeMapModifications();
				}
			});
			Microsoft.Xna.Framework.Rectangle player_bounds = Game1.player.GetBoundingBox();
			foreach (Furniture f in furniture)
			{
				if (f.getBoundingBox(f.TileLocation).Intersects(player_bounds) && f.IntersectsForCollision(player_bounds) && !f.isPassable())
				{
					Game1.player.TemporaryPassableTiles.Add(f.getBoundingBox(f.TileLocation));
				}
			}
		}

		protected virtual void resetLocalState()
		{
			Game1.elliottPiano = 0;
			Game1.crabPotOverlayTiles.Clear();
			Utility.killAllStaticLoopingSoundCues();
			Game1.player.bridge = null;
			Game1.player.SetOnBridge(val: false);
			if (Game1.CurrentEvent == null && !Name.ToLower().Contains("bath"))
			{
				Game1.player.canOnlyWalk = false;
			}
			if (!(this is Farm))
			{
				for (int i2 = temporarySprites.Count - 1; i2 >= 0; i2--)
				{
					if (temporarySprites[i2].clearOnAreaEntry())
					{
						temporarySprites.RemoveAt(i2);
					}
				}
			}
			if (Game1.options != null)
			{
				if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton))
				{
					Game1.player.setRunning(!Game1.options.autoRun, force: true);
				}
				else
				{
					Game1.player.setRunning(Game1.options.autoRun, force: true);
				}
			}
			Game1.UpdateViewPort(overrideFreeze: false, new Point(Game1.player.getStandingX(), Game1.player.getStandingY()));
			Game1.previousViewportPosition = new Vector2(Game1.viewport.X, Game1.viewport.Y);
			Game1.PushUIMode();
			foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
			{
				onScreenMenu.gameWindowSizeChanged(new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height), new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height));
			}
			Game1.PopUIMode();
			ignoreWarps = false;
			if (Game1.newDaySync == null || Game1.newDaySync.hasFinished())
			{
				if (Game1.player.rightRing.Value != null)
				{
					Game1.player.rightRing.Value.onNewLocation(Game1.player, this);
				}
				if (Game1.player.leftRing.Value != null)
				{
					Game1.player.leftRing.Value.onNewLocation(Game1.player, this);
				}
			}
			forceViewportPlayerFollow = Map.Properties.ContainsKey("ViewportFollowPlayer");
			lastTouchActionLocation = Game1.player.getTileLocation();
			for (int n = Game1.player.questLog.Count - 1; n >= 0; n--)
			{
				Game1.player.questLog[n].adjustGameLocation(this);
			}
			if (!isOutdoors)
			{
				Game1.player.FarmerSprite.currentStep = "thudStep";
			}
			_updateAmbientLighting();
			setUpLocationSpecificFlair();
			map.Properties.TryGetValue("UniquePortrait", out PropertyValue uniquePortraits);
			if (uniquePortraits != null)
			{
				string[] array = uniquePortraits.ToString().Split(' ');
				for (int num = 0; num < array.Length; num++)
				{
					NPC m = Game1.getCharacterFromName(array[num]);
					if (characters.Contains(m))
					{
						try
						{
							m.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + m.Name + "_" + name);
							m.uniquePortraitActive = true;
						}
						catch (Exception)
						{
						}
					}
				}
			}
			map.Properties.TryGetValue("Light", out PropertyValue lights2);
			if (lights2 != null && !ignoreLights)
			{
				string[] split5 = lights2.ToString().Split(' ');
				for (int l = 0; l < split5.Length; l += 3)
				{
					Game1.currentLightSources.Add(new LightSource(Convert.ToInt32(split5[l + 2]), new Vector2(Convert.ToInt32(split5[l]) * 64 + 32, Convert.ToInt32(split5[l + 1]) * 64 + 32), 1f, LightSource.LightContext.MapLight, 0L));
				}
			}
			lights2 = null;
			map.Properties.TryGetValue("WindowLight", out lights2);
			if (lights2 != null && !ignoreLights)
			{
				string[] split4 = lights2.ToString().Split(' ');
				for (int k = 0; k < split4.Length; k += 3)
				{
					Game1.currentLightSources.Add(new LightSource(Convert.ToInt32(split4[k + 2]), new Vector2(Convert.ToInt32(split4[k]) * 64 + 32, Convert.ToInt32(split4[k + 1]) * 64 + 32), 1f, LightSource.LightContext.WindowLight, 0L));
				}
			}
			if ((bool)isOutdoors || (bool)treatAsOutdoors)
			{
				map.Properties.TryGetValue("BrookSounds", out PropertyValue brookSounds);
				if (brookSounds != null)
				{
					string[] split3 = brookSounds.ToString().Split(' ');
					for (int j = 0; j < split3.Length; j += 3)
					{
						AmbientLocationSounds.addSound(new Vector2(Convert.ToInt32(split3[j]), Convert.ToInt32(split3[j + 1])), Convert.ToInt32(split3[j + 2]));
					}
				}
				Game1.randomizeRainPositions();
				Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
			}
			foreach (KeyValuePair<Vector2, TerrainFeature> kvp in terrainFeatures.Pairs)
			{
				kvp.Value.performPlayerEntryAction(kvp.Key);
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					largeTerrainFeature.performPlayerEntryAction(largeTerrainFeature.tilePosition);
				}
			}
			foreach (KeyValuePair<Vector2, Object> pair in objects.Pairs)
			{
				pair.Value.actionOnPlayerEntry();
			}
			if ((bool)isOutdoors && Game1.shouldPlayMorningSong())
			{
				Game1.playMorningSong();
			}
			PropertyValue musicValue = null;
			map.Properties.TryGetValue("Music", out musicValue);
			if (musicValue != null)
			{
				string[] split2 = musicValue.ToString().Split(' ');
				if (split2.Length > 1)
				{
					if (Game1.timeOfDay >= Convert.ToInt32(split2[0]) && Game1.timeOfDay < Convert.ToInt32(split2[1]) && !split2[2].Equals(Game1.getMusicTrackName()))
					{
						Game1.changeMusicTrack(split2[2]);
					}
				}
				else if (Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || !split2[0].Equals(Game1.getMusicTrackName()))
				{
					Game1.changeMusicTrack(split2[0]);
				}
			}
			if ((bool)isOutdoors)
			{
				((FarmerSprite)Game1.player.Sprite).currentStep = "sandyStep";
				tryToAddCritters();
			}
			interiorDoors.ResetLocalState();
			int night_tiles_time = Game1.getTrulyDarkTime();
			if (Game1.timeOfDay < night_tiles_time && (!Game1.IsRainingHere(this) || name.Equals("SandyHouse")))
			{
				map.Properties.TryGetValue("DayTiles", out PropertyValue dayTiles);
				if (dayTiles != null)
				{
					string[] split = dayTiles.ToString().Trim().Split(' ');
					for (int i = 0; i < split.Length; i += 4)
					{
						if ((!split[i + 3].Equals("720") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")) && map.GetLayer(split[i]).Tiles[Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])] != null)
						{
							map.GetLayer(split[i]).Tiles[Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])].TileIndex = Convert.ToInt32(split[i + 3]);
						}
					}
				}
			}
			else if (Game1.timeOfDay >= night_tiles_time || (Game1.IsRainingHere(this) && !name.Equals("SandyHouse")))
			{
				switchOutNightTiles();
			}
			if (name.Equals("Coop"))
			{
				string[] feedLocation2 = getMapProperty("Feed").Split(' ');
				if (Game1.MasterPlayer.Feed <= 0)
				{
					map.GetLayer("Buildings").Tiles[Convert.ToInt32(feedLocation2[0]), Convert.ToInt32(feedLocation2[1])].TileIndex = 35;
				}
				else
				{
					map.GetLayer("Buildings").Tiles[Convert.ToInt32(feedLocation2[0]), Convert.ToInt32(feedLocation2[1])].TileIndex = 31;
				}
			}
			else if (name.Equals("Barn"))
			{
				string[] feedLocation = getMapProperty("Feed").Split(' ');
				if (Game1.MasterPlayer.Feed <= 0)
				{
					map.GetLayer("Buildings").Tiles[Convert.ToInt32(feedLocation[0]), Convert.ToInt32(feedLocation[1])].TileIndex = 35;
				}
				else
				{
					map.GetLayer("Buildings").Tiles[Convert.ToInt32(feedLocation[0]), Convert.ToInt32(feedLocation[1])].TileIndex = 31;
				}
			}
			if (name.Equals("Club"))
			{
				Game1.changeMusicTrack("clubloop");
			}
			else if (Game1.getMusicTrackName().Equals("clubloop"))
			{
				Game1.changeMusicTrack("none");
			}
			if (Game1.killScreen && Game1.activeClickableMenu != null && !Game1.dialogueUp)
			{
				Game1.activeClickableMenu.emergencyShutDown();
				Game1.exitActiveMenu();
			}
			if (Game1.activeClickableMenu == null && !Game1.warpingForForcedRemoteEvent)
			{
				checkForEvents();
			}
			Game1.currentLightSources.UnionWith(sharedLights.Values);
			foreach (NPC character in characters)
			{
				character.behaviorOnLocalFarmerLocationEntry(this);
			}
			foreach (Furniture item in furniture)
			{
				item.resetOnPlayerEntry(this);
			}
			updateFishSplashAnimation();
			updateOrePanAnimation();
			showDropboxIndicator = false;
			foreach (SpecialOrder s in Game1.player.team.specialOrders)
			{
				if (!s.ShouldDisplayAsComplete())
				{
					foreach (OrderObjective o in s.objectives)
					{
						if (o is DonateObjective)
						{
							DonateObjective donateObjective = o as DonateObjective;
							if (donateObjective.dropBoxGameLocation != null && donateObjective.GetDropboxLocationName() == Name)
							{
								showDropboxIndicator = true;
								dropBoxIndicatorLocation = donateObjective.dropBoxTileLocation.Value * 64f + new Vector2(7f, 0f) * 4f;
							}
						}
					}
				}
			}
		}

		protected void _updateAmbientLighting()
		{
			if (!isOutdoors || (bool)ignoreOutdoorLighting)
			{
				map.Properties.TryGetValue("AmbientLight", out PropertyValue ambientLight);
				if (ambientLight != null)
				{
					string[] colorSplit = ambientLight.ToString().Split(' ');
					Game1.ambientLight = new Color(Convert.ToInt32(colorSplit[0]), Convert.ToInt32(colorSplit[1]), Convert.ToInt32(colorSplit[2]));
				}
				else if (Game1.isDarkOut() || (float)lightLevel > 0f)
				{
					Game1.ambientLight = new Color(180, 180, 0);
				}
				else
				{
					Game1.ambientLight = Color.White;
				}
				if (Game1.getMusicTrackName().Contains("ambient"))
				{
					Game1.changeMusicTrack("none", Game1.currentTrackOverrideable);
				}
			}
			else if (!(this is Desert))
			{
				Game1.ambientLight = (Game1.IsRainingHere(this) ? new Color(255, 200, 80) : Color.White);
			}
		}

		public void SelectRandomMiniJukeboxTrack()
		{
			if (!(miniJukeboxTrack.Value != "random"))
			{
				Farmer farmer = Game1.player;
				if (this is FarmHouse && (this as FarmHouse).owner != null)
				{
					farmer = (this as FarmHouse).owner;
				}
				List<string> list = farmer.songsHeard.Distinct().ToList();
				ChooseFromListMenu.FilterJukeboxTracks(list);
				string song = Utility.GetRandom(list);
				randomMiniJukeboxTrack.Value = song;
			}
		}

		protected virtual void resetSharedState()
		{
			SelectRandomMiniJukeboxTrack();
			for (int j = characters.Count - 1; j >= 0; j--)
			{
				characters[j].behaviorOnFarmerLocationEntry(this, Game1.player);
			}
			Map.Properties.TryGetValue("UniqueSprite", out PropertyValue uniqueSprites);
			if (uniqueSprites != null)
			{
				string[] array = uniqueSprites.ToString().Split(' ');
				for (int k = 0; k < array.Length; k++)
				{
					NPC i = Game1.getCharacterFromName(array[k]);
					if (characters.Contains(i))
					{
						try
						{
							i.Sprite.LoadTexture("Characters\\" + NPC.getTextureNameForCharacter(i.Name) + "_" + name);
							i.uniqueSpriteActive = true;
						}
						catch (Exception)
						{
						}
					}
				}
			}
			if (!(this is MineShaft))
			{
				switch (Game1.currentSeason.ToLower())
				{
				case "spring":
					waterColor.Value = new Color(120, 200, 255) * 0.5f;
					break;
				case "summer":
					waterColor.Value = new Color(60, 240, 255) * 0.5f;
					break;
				case "fall":
					waterColor.Value = new Color(255, 130, 200) * 0.5f;
					break;
				case "winter":
					waterColor.Value = new Color(130, 80, 255) * 0.5f;
					break;
				}
			}
			if (name.Equals("Mountain") && (Game1.timeOfDay < 2000 || !Game1.currentSeason.Equals("summer") || !(Game1.random.NextDouble() < 0.3)) && Game1.IsRainingHere(this) && !Game1.currentSeason.Equals("winter"))
			{
				Game1.random.NextDouble();
				_ = 0.2;
			}
		}

		public LightSource getLightSource(int identifier)
		{
			sharedLights.TryGetValue(identifier, out LightSource i);
			return i;
		}

		public bool hasLightSource(int identifier)
		{
			return sharedLights.ContainsKey(identifier);
		}

		public void removeLightSource(int identifier)
		{
			sharedLights.Remove(identifier);
		}

		public void repositionLightSource(int identifier, Vector2 position)
		{
			sharedLights.TryGetValue(identifier, out LightSource i);
			if (i != null)
			{
				i.position.Value = position;
			}
		}

		public virtual bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.occupiesTile((int)tileLocation.X, (int)tileLocation.Y))
				{
					return true;
				}
			}
			objects.TryGetValue(tileLocation, out Object o);
			Microsoft.Xna.Framework.Rectangle tileLocationRect = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i] != null && characters[i].GetBoundingBox().Intersects(tileLocationRect))
				{
					return true;
				}
			}
			if (isTileOccupiedByFarmer(tileLocation) != null && (toPlace == null || !toPlace.isPassable()))
			{
				return true;
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(tileLocationRect))
					{
						return true;
					}
				}
			}
			if (toPlace != null && toPlace.Category == -19)
			{
				if (toPlace.Category == -19 && terrainFeatures.ContainsKey(tileLocation) && terrainFeatures[tileLocation] is HoeDirt)
				{
					HoeDirt hoe_dirt = terrainFeatures[tileLocation] as HoeDirt;
					if ((int)(terrainFeatures[tileLocation] as HoeDirt).fertilizer != 0)
					{
						return true;
					}
					if (((int)toPlace.parentSheetIndex == 368 || (int)toPlace.parentSheetIndex == 368) && hoe_dirt.crop != null && (int)hoe_dirt.crop.currentPhase != 0)
					{
						return true;
					}
				}
			}
			else if (terrainFeatures.ContainsKey(tileLocation) && tileLocationRect.Intersects(terrainFeatures[tileLocation].getBoundingBox(tileLocation)) && (!terrainFeatures[tileLocation].isPassable() || (terrainFeatures[tileLocation] is HoeDirt && ((HoeDirt)terrainFeatures[tileLocation]).crop != null) || (toPlace != null && toPlace.isSapling())))
			{
				return true;
			}
			if ((toPlace == null || !(toPlace is BedFurniture) || isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) || !isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y + 1), Game1.viewport)) && !isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) && (toPlace == null || !(toPlace is Wallpaper)))
			{
				return true;
			}
			if (toPlace != null && (toPlace.Category == -74 || toPlace.Category == -19) && o != null && o is IndoorPot)
			{
				if ((int)toPlace.parentSheetIndex == 251)
				{
					if ((o as IndoorPot).bush.Value == null && (o as IndoorPot).hoeDirt.Value.crop == null)
					{
						return false;
					}
				}
				else if ((o as IndoorPot).hoeDirt.Value.canPlantThisSeedHere(toPlace.parentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, toPlace.Category == -19) && (o as IndoorPot).bush.Value == null)
				{
					return false;
				}
			}
			return o != null;
		}

		public Farmer isTileOccupiedByFarmer(Vector2 tileLocation)
		{
			foreach (Farmer f in farmers)
			{
				if (f.getTileLocation().Equals(tileLocation))
				{
					return f;
				}
			}
			return null;
		}

		public virtual bool isTileOccupied(Vector2 tileLocation, string characterToIgnore = "", bool ignoreAllCharacters = false)
		{
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.occupiesTile((int)tileLocation.X, (int)tileLocation.Y))
				{
					return true;
				}
			}
			objects.TryGetValue(tileLocation, out Object o);
			Microsoft.Xna.Framework.Rectangle tileLocationRect = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64 + 1, (int)tileLocation.Y * 64 + 1, 62, 62);
			if (!ignoreAllCharacters)
			{
				for (int i = 0; i < characters.Count; i++)
				{
					if (characters[i] != null && !characters[i].Name.Equals(characterToIgnore) && characters[i].GetBoundingBox().Intersects(tileLocationRect))
					{
						return true;
					}
				}
			}
			if (terrainFeatures.ContainsKey(tileLocation) && tileLocationRect.Intersects(terrainFeatures[tileLocation].getBoundingBox(tileLocation)) && (!NPC.isCheckingSpouseTileOccupancy || !terrainFeatures[tileLocation].isPassable()))
			{
				return true;
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(tileLocationRect))
					{
						return true;
					}
				}
			}
			Furniture f = GetFurnitureAt(tileLocation);
			if (f != null && !f.isPassable())
			{
				return true;
			}
			if (NPC.isCheckingSpouseTileOccupancy && o != null && o.isPassable())
			{
				return false;
			}
			return o != null;
		}

		public virtual bool isTileOccupiedIgnoreFloors(Vector2 tileLocation, string characterToIgnore = "")
		{
			objects.TryGetValue(tileLocation, out Object o);
			Microsoft.Xna.Framework.Rectangle tileLocationRect = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64 + 1, (int)tileLocation.Y * 64 + 1, 62, 62);
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i] != null && !characters[i].name.Equals(characterToIgnore) && characters[i].GetBoundingBox().Intersects(tileLocationRect))
				{
					return true;
				}
			}
			if (terrainFeatures.ContainsKey(tileLocation) && tileLocationRect.Intersects(terrainFeatures[tileLocation].getBoundingBox(tileLocation)) && !terrainFeatures[tileLocation].isPassable())
			{
				return true;
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(tileLocationRect))
					{
						return true;
					}
				}
			}
			Furniture f = GetFurnitureAt(tileLocation);
			if (f != null && !f.isPassable())
			{
				return true;
			}
			return o != null;
		}

		public bool isTileHoeDirt(Vector2 tileLocation)
		{
			if (terrainFeatures.ContainsKey(tileLocation) && terrainFeatures[tileLocation] is HoeDirt)
			{
				return true;
			}
			if (objects.ContainsKey(tileLocation) && objects[tileLocation] is IndoorPot)
			{
				return true;
			}
			return false;
		}

		public void playTerrainSound(Vector2 tileLocation, Character who = null, bool showTerrainDisturbAnimation = true)
		{
			string currentStep = "thudStep";
			if (Game1.currentLocation.IsOutdoors || Game1.currentLocation.Name.ToLower().Contains("mine"))
			{
				switch (Game1.currentLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back"))
				{
				case "Dirt":
					currentStep = "sandyStep";
					break;
				case "Stone":
					currentStep = "stoneStep";
					break;
				case "Grass":
					currentStep = (GetSeasonForLocation().Equals("winter") ? "snowyStep" : "grassyStep");
					break;
				case "Wood":
					currentStep = "woodyStep";
					break;
				case null:
				{
					string stepType = Game1.currentLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Water", "Back");
					if (stepType != null)
					{
						currentStep = "waterSlosh";
					}
					break;
				}
				}
			}
			else
			{
				currentStep = "thudStep";
			}
			if (Game1.currentLocation.terrainFeatures.ContainsKey(tileLocation) && Game1.currentLocation.terrainFeatures[tileLocation] is Flooring)
			{
				currentStep = ((Flooring)Game1.currentLocation.terrainFeatures[tileLocation]).getFootstepSound();
			}
			if (who != null && showTerrainDisturbAnimation && currentStep.Equals("sandyStep"))
			{
				Vector2 offset2 = Vector2.Zero;
				if (who.shouldShadowBeOffset)
				{
					offset2 = who.drawOffset.Value;
				}
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 64, 64, 64), 50f, 4, 1, new Vector2(who.Position.X + (float)Game1.random.Next(-8, 8), who.Position.Y + (float)Game1.random.Next(-16, 0)) + offset2, flicker: false, Game1.random.NextDouble() < 0.5, 0.0001f, 0f, Color.White, 1f, 0.01f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 128f));
			}
			else if (who != null && showTerrainDisturbAnimation && GetSeasonForLocation().Equals("winter") && currentStep.Equals("grassyStep"))
			{
				Vector2 offset = Vector2.Zero;
				if (who.shouldShadowBeOffset)
				{
					offset = who.drawOffset.Value;
				}
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(who.Position.X, who.Position.Y) + offset, flicker: false, flipped: false, 0.0001f, 0.001f, Color.White, 1f, 0.01f, 0f, 0f));
			}
			if (currentStep.Length > 0)
			{
				localSound(currentStep);
			}
		}

		public bool checkTileIndexAction(int tileIndex)
		{
			if ((tileIndex == 1799 || (uint)(tileIndex - 1824) <= 9u) && Name.Equals("AbandonedJojaMart"))
			{
				(Game1.getLocationFromName("AbandonedJojaMart") as AbandonedJojaMart).checkBundle();
				return true;
			}
			return false;
		}

		public virtual bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			return Game1.hooks.OnGameLocation_CheckAction(this, tileLocation, viewport, who, delegate
			{
				if (who.IsSitting())
				{
					who.StopSitting();
					return true;
				}
				Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
				foreach (Farmer current in farmers)
				{
					if (current != Game1.player && current.GetBoundingBox().Intersects(value) && current.checkAction(who, this))
					{
						return true;
					}
				}
				if (currentEvent != null && currentEvent.isFestival)
				{
					return currentEvent.checkAction(tileLocation, viewport, who);
				}
				foreach (NPC current2 in characters)
				{
					if (current2 != null && !current2.IsMonster && (!who.isRidingHorse() || !(current2 is Horse)) && current2.GetBoundingBox().Intersects(value) && current2.checkAction(who, this))
					{
						if (who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, carrying: false) || who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, carrying: true))
						{
							who.faceGeneralDirection(current2.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
						}
						return true;
					}
				}
				if (who.IsLocalPlayer && who.currentUpgrade != null && name.Equals("Farm") && tileLocation.Equals(new Location((int)(who.currentUpgrade.positionOfCarpenter.X + 32f) / 64, (int)(who.currentUpgrade.positionOfCarpenter.Y + 32f) / 64)))
				{
					if (who.currentUpgrade.daysLeftTillUpgradeDone == 1)
					{
						Game1.drawDialogue(Game1.getCharacterFromName("Robin"), Game1.content.LoadString("Data\\ExtraDialogue:Farm_RobinWorking_ReadyTomorrow"));
					}
					else
					{
						Game1.drawDialogue(Game1.getCharacterFromName("Robin"), Game1.content.LoadString("Data\\ExtraDialogue:Farm_RobinWorking" + (Game1.random.Next(2) + 1)));
					}
				}
				foreach (ResourceClump current3 in resourceClumps)
				{
					if (current3.getBoundingBox(current3.tile).Intersects(value) && current3.performUseAction(new Vector2(tileLocation.X, tileLocation.Y), this))
					{
						return true;
					}
				}
				Vector2 vector = new Vector2(tileLocation.X, tileLocation.Y);
				if (objects.ContainsKey(vector) && objects[vector].Type != null)
				{
					if (who.isRidingHorse() && !(objects[vector] is Fence))
					{
						return false;
					}
					if (vector.Equals(who.getTileLocation()) && !objects[vector].isPassable())
					{
						Tool tool = new Pickaxe();
						tool.DoFunction(Game1.currentLocation, -1, -1, 0, who);
						if (objects[vector].performToolAction(tool, this))
						{
							objects[vector].performRemoveAction(objects[vector].tileLocation, Game1.currentLocation);
							objects[vector].dropItem(this, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y));
							Game1.currentLocation.Objects.Remove(vector);
							return true;
						}
						tool = new Axe();
						tool.DoFunction(Game1.currentLocation, -1, -1, 0, who);
						if (objects.ContainsKey(vector) && objects[vector].performToolAction(tool, this))
						{
							objects[vector].performRemoveAction(objects[vector].tileLocation, Game1.currentLocation);
							objects[vector].dropItem(this, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y));
							Game1.currentLocation.Objects.Remove(vector);
							return true;
						}
						if (!objects.ContainsKey(vector))
						{
							return true;
						}
					}
					if (objects.ContainsKey(vector) && (objects[vector].Type.Equals("Crafting") || objects[vector].Type.Equals("interactive")))
					{
						if (who.ActiveObject == null && objects[vector].checkForAction(who))
						{
							return true;
						}
						if (objects.ContainsKey(vector))
						{
							if (who.CurrentItem != null)
							{
								Object value2 = objects[vector].heldObject.Value;
								objects[vector].heldObject.Value = null;
								bool flag = objects[vector].performObjectDropInAction(who.CurrentItem, probe: true, who);
								objects[vector].heldObject.Value = value2;
								bool flag2 = objects[vector].performObjectDropInAction(who.CurrentItem, probe: false, who);
								if ((flag | flag2) && who.isMoving())
								{
									Game1.haltAfterCheck = false;
								}
								if (flag2)
								{
									who.reduceActiveItemByOne();
									return true;
								}
								return objects[vector].checkForAction(who) | flag;
							}
							return objects[vector].checkForAction(who);
						}
					}
					else if (objects.ContainsKey(vector) && (bool)objects[vector].isSpawnedObject)
					{
						int quality = objects[vector].quality;
						Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)vector.X + (int)vector.Y * 777);
						if (who.professions.Contains(16) && objects[vector].isForage(this))
						{
							objects[vector].Quality = 4;
						}
						else if (objects[vector].isForage(this))
						{
							if (random.NextDouble() < (double)((float)who.ForagingLevel / 30f))
							{
								objects[vector].Quality = 2;
							}
							else if (random.NextDouble() < (double)((float)who.ForagingLevel / 15f))
							{
								objects[vector].Quality = 1;
							}
						}
						if ((bool)objects[vector].questItem && objects[vector].questId.Value != 0 && !who.hasQuest(objects[vector].questId))
						{
							return false;
						}
						if (who.couldInventoryAcceptThisItem(objects[vector]))
						{
							if (who.IsLocalPlayer)
							{
								localSound("pickUpItem");
								DelayedAction.playSoundAfterDelay("coin", 300);
							}
							who.animateOnce(279 + who.FacingDirection);
							if (!isFarmBuildingInterior())
							{
								if (objects[vector].isForage(this))
								{
									who.gainExperience(2, 7);
								}
							}
							else
							{
								who.gainExperience(0, 5);
							}
							who.addItemToInventoryBool(objects[vector].getOne());
							Game1.stats.ItemsForaged++;
							if (who.professions.Contains(13) && random.NextDouble() < 0.2 && !objects[vector].questItem && who.couldInventoryAcceptThisItem(objects[vector]) && !isFarmBuildingInterior())
							{
								who.addItemToInventoryBool(objects[vector].getOne());
								who.gainExperience(2, 7);
							}
							objects.Remove(vector);
							return true;
						}
						objects[vector].Quality = quality;
					}
				}
				if (who.isRidingHorse())
				{
					who.mount.checkAction(who, this);
					return true;
				}
				foreach (MapSeat current4 in mapSeats)
				{
					if (current4.OccupiesTile(tileLocation.X, tileLocation.Y) && !current4.IsBlocked(this))
					{
						who.BeginSitting(current4);
						return true;
					}
				}
				foreach (KeyValuePair<Vector2, TerrainFeature> current5 in terrainFeatures.Pairs)
				{
					if (current5.Value.getBoundingBox(current5.Key).Intersects(value) && current5.Value.performUseAction(current5.Key, this))
					{
						Game1.haltAfterCheck = false;
						return true;
					}
				}
				if (largeTerrainFeatures != null)
				{
					foreach (LargeTerrainFeature current6 in largeTerrainFeatures)
					{
						if (current6.getBoundingBox().Intersects(value) && current6.performUseAction(current6.tilePosition, this))
						{
							Game1.haltAfterCheck = false;
							return true;
						}
					}
				}
				string text = null;
				Tile tile = map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
				if (tile != null)
				{
					tile.Properties.TryGetValue("Action", out PropertyValue value3);
					if (value3 != null)
					{
						text = value3.ToString();
					}
				}
				if (text == null)
				{
					text = doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
				}
				NPC nPC = isCharacterAtTile(vector + new Vector2(0f, 1f));
				if (text != null)
				{
					if (currentEvent == null && nPC != null && !nPC.IsInvisible && !nPC.IsMonster && (!who.isRidingHorse() || !(nPC is Horse)) && Utility.withinRadiusOfPlayer(nPC.getStandingX(), nPC.getStandingY(), 1, who) && nPC.checkAction(who, this))
					{
						if (who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, who.IsCarrying()))
						{
							who.faceGeneralDirection(nPC.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
						}
						return true;
					}
					return performAction(text, who, tileLocation);
				}
				if (tile != null && checkTileIndexAction(tile.TileIndex))
				{
					return true;
				}
				Point value4 = new Point(tileLocation.X * 64, (tileLocation.Y - 1) * 64);
				bool flag3 = Game1.didPlayerJustRightClick();
				foreach (Furniture current7 in furniture)
				{
					if (current7.boundingBox.Value.Contains((int)(vector.X * 64f), (int)(vector.Y * 64f)) && (int)current7.furniture_type != 12)
					{
						if (flag3)
						{
							if (who.ActiveObject != null && current7.performObjectDropInAction(who.ActiveObject, probe: false, who))
							{
								return true;
							}
							return current7.checkForAction(who);
						}
						return current7.clicked(who);
					}
					if ((int)current7.furniture_type == 6 && current7.boundingBox.Value.Contains(value4))
					{
						if (flag3)
						{
							if (who.ActiveObject != null && current7.performObjectDropInAction(who.ActiveObject, probe: false, who))
							{
								return true;
							}
							return current7.checkForAction(who);
						}
						return current7.clicked(who);
					}
				}
				return false;
			});
		}

		public virtual bool CanFreePlaceFurniture()
		{
			return false;
		}

		public virtual bool LowPriorityLeftClick(int x, int y, Farmer who)
		{
			if (Game1.activeClickableMenu != null)
			{
				return false;
			}
			for (int i = furniture.Count - 1; i >= 0; i--)
			{
				Furniture furnitureItem2 = furniture[i];
				if (CanFreePlaceFurniture() || furnitureItem2.IsCloseEnoughToFarmer(who))
				{
					if (!furnitureItem2.isPassable() && furnitureItem2.boundingBox.Value.Contains(x, y) && furnitureItem2.canBeRemoved(who))
					{
						furnitureItem2.AttemptRemoval(delegate(Furniture f)
						{
							Guid job3 = furniture.GuidOf(f);
							if (!furnitureToRemove.Contains(job3))
							{
								furnitureToRemove.Add(job3);
							}
						});
						return true;
					}
					if (furnitureItem2.boundingBox.Value.Contains(x, y) && furnitureItem2.heldObject.Value != null)
					{
						furnitureItem2.clicked(who);
						return true;
					}
					if (!furnitureItem2.isGroundFurniture() && furnitureItem2.canBeRemoved(who))
					{
						int wall_y = y;
						foreach (Microsoft.Xna.Framework.Rectangle wall in getWalls())
						{
							if (wall.Contains(x / 64, y / 64))
							{
								wall_y = wall.Top * 64;
								break;
							}
						}
						if (furnitureItem2.boundingBox.Value.Contains(x, wall_y))
						{
							furnitureItem2.AttemptRemoval(delegate(Furniture f)
							{
								Guid job2 = furniture.GuidOf(f);
								if (!furnitureToRemove.Contains(job2))
								{
									furnitureToRemove.Add(job2);
								}
							});
							return true;
						}
					}
				}
			}
			for (int j = furniture.Count - 1; j >= 0; j--)
			{
				Furniture furnitureItem = furniture[j];
				if ((CanFreePlaceFurniture() || furnitureItem.IsCloseEnoughToFarmer(who)) && furnitureItem.isPassable() && furnitureItem.boundingBox.Value.Contains(x, y) && furnitureItem.canBeRemoved(who))
				{
					furnitureItem.AttemptRemoval(delegate(Furniture f)
					{
						Guid job = furniture.GuidOf(f);
						if (!furnitureToRemove.Contains(job))
						{
							furnitureToRemove.Add(job);
						}
					});
					return true;
				}
			}
			return false;
		}

		public virtual bool CanPlaceThisFurnitureHere(Furniture furniture)
		{
			if (furniture == null)
			{
				return false;
			}
			if (furniture.furniture_type.Value == 15 && !(this is FarmHouse) && !(this is IslandFarmHouse))
			{
				return false;
			}
			int furniture_restriction = furniture.placementRestriction;
			if ((furniture_restriction == 0 || furniture_restriction == 2) && (this is DecoratableLocation || !IsOutdoors))
			{
				return true;
			}
			if ((furniture_restriction == 1 || furniture_restriction == 2) && !(this is DecoratableLocation) && IsOutdoors)
			{
				return true;
			}
			return false;
		}

		public virtual List<Microsoft.Xna.Framework.Rectangle> getWalls()
		{
			return new List<Microsoft.Xna.Framework.Rectangle>();
		}

		protected virtual void removeQueuedFurniture(Guid guid)
		{
			Farmer who = Game1.player;
			if (!furniture.ContainsGuid(guid))
			{
				return;
			}
			Furniture furnitureItem = furniture[guid];
			if (!who.couldInventoryAcceptThisItem(furnitureItem))
			{
				return;
			}
			furnitureItem.performRemoveAction(furnitureItem.tileLocation, this);
			furniture.Remove(guid);
			bool foundInToolbar = false;
			for (int i = 0; i < 12; i++)
			{
				if (who.items[i] == null)
				{
					who.items[i] = furnitureItem;
					who.CurrentToolIndex = i;
					foundInToolbar = true;
					break;
				}
			}
			if (!foundInToolbar)
			{
				Item item = who.addItemToInventory(furnitureItem, 11);
				who.addItemToInventory(item);
				who.CurrentToolIndex = 11;
			}
			localSound("coin");
		}

		public virtual bool leftClick(int x, int y, Farmer who)
		{
			Vector2 clickTile = new Vector2(x / 64, y / 64);
			if (objects.ContainsKey(clickTile) && objects[clickTile].clicked(who))
			{
				objects.Remove(clickTile);
				return true;
			}
			return false;
		}

		public virtual int getExtraMillisecondsPerInGameMinuteForThisLocation()
		{
			return 0;
		}

		public bool isTileLocationTotallyClearAndPlaceable(int x, int y)
		{
			return isTileLocationTotallyClearAndPlaceable(new Vector2(x, y));
		}

		public virtual bool isTileLocationTotallyClearAndPlaceableIgnoreFloors(Vector2 v)
		{
			if (isTileOnMap(v) && !isTileOccupiedIgnoreFloors(v) && isTilePassable(new Location((int)v.X, (int)v.Y), Game1.viewport))
			{
				return isTilePlaceable(v);
			}
			return false;
		}

		public void ActivateKitchen(NetRef<Chest> fridge)
		{
			List<NetMutex> muticies = new List<NetMutex>();
			List<Chest> mini_fridges = new List<Chest>();
			foreach (Object item in objects.Values)
			{
				if (item != null && (bool)item.bigCraftable && item is Chest && item.ParentSheetIndex == 216)
				{
					mini_fridges.Add(item as Chest);
					muticies.Add((item as Chest).mutex);
				}
			}
			if (fridge != null && fridge.Value.mutex.IsLocked())
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
				return;
			}
			MultipleMutexRequest multiple_mutex_request = null;
			multiple_mutex_request = new MultipleMutexRequest(muticies, delegate
			{
				fridge.Value.mutex.RequestLock(delegate
				{
					List<Chest> list = new List<Chest>();
					if (fridge != null)
					{
						list.Add(fridge);
					}
					list.AddRange(mini_fridges);
					Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
					Game1.activeClickableMenu = new CraftingPage((int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, cooking: true, standalone_menu: true, list);
					Game1.activeClickableMenu.exitFunction = delegate
					{
						fridge.Value.mutex.ReleaseLock();
						multiple_mutex_request.ReleaseLocks();
					};
				}, delegate
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
					multiple_mutex_request.ReleaseLocks();
				});
			}, delegate
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
			});
		}

		public virtual bool isTileLocationTotallyClearAndPlaceable(Vector2 v)
		{
			Vector2 nonTile = v * 64f;
			nonTile.X += 32f;
			nonTile.Y += 32f;
			foreach (Furniture f in furniture)
			{
				if ((int)f.furniture_type != 12 && !f.isPassable() && f.getBoundingBox(f.tileLocation).Contains((int)nonTile.X, (int)nonTile.Y) && !f.AllowPlacementOnThisTile((int)v.X, (int)v.Y))
				{
					return false;
				}
			}
			if (isTileOnMap(v) && !isTileOccupied(v) && isTilePassable(new Location((int)v.X, (int)v.Y), Game1.viewport))
			{
				return isTilePlaceable(v);
			}
			return false;
		}

		public virtual bool isTilePlaceable(Vector2 v, Item item = null)
		{
			if (doesTileHaveProperty((int)v.X, (int)v.Y, "NoFurniture", "Back") == null || (item != null && item is Object && (item as Object).isPassable() && Game1.currentLocation.IsOutdoors && !doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "NoFurniture", "Back").Equals("total")))
			{
				if (doesTileHaveProperty((int)v.X, (int)v.Y, "Water", "Back") != null)
				{
					return item?.canBePlacedInWater() ?? false;
				}
				return true;
			}
			return false;
		}

		public virtual bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			if (doesTileHaveProperty((int)p.X, (int)p.Y, "Passable", "Buildings") != null)
			{
				return true;
			}
			if (terrainFeatures.TryGetValue(p, out TerrainFeature feat) && feat is HoeDirt)
			{
				return true;
			}
			if (doesTileHaveProperty((int)p.X, (int)p.Y, "Water", "Back") != null && (!(getTileSheetIDAt((int)p.X, (int)p.Y, "Buildings") == "Town") || getTileIndexAt((int)p.X, (int)p.Y, "Buildings") < 1004 || getTileIndexAt((int)p.X, (int)p.Y, "Buildings") > 1013))
			{
				return true;
			}
			return false;
		}

		public void openDoor(Location tileLocation, bool playSound)
		{
			try
			{
				int tileIndex = getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings");
				Point point = new Point(tileLocation.X, tileLocation.Y);
				if (interiorDoors.ContainsKey(point))
				{
					interiorDoors[point] = true;
					if (playSound)
					{
						Vector2 pos = new Vector2(tileLocation.X, tileLocation.Y);
						if (tileIndex == 120)
						{
							playSoundAt("doorOpen", pos);
						}
						else
						{
							playSoundAt("doorCreak", pos);
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public void doStarpoint(string which)
		{
			if (!(which == "3"))
			{
				if (which == "4" && Game1.player.ActiveObject != null && Game1.player.ActiveObject != null && !Game1.player.ActiveObject.bigCraftable && (int)Game1.player.ActiveObject.parentSheetIndex == 203)
				{
					Object reward2 = new Object(Vector2.Zero, 162);
					if (!Game1.player.couldInventoryAcceptThisItem(reward2) && (int)Game1.player.ActiveObject.stack > 1)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
						return;
					}
					Game1.player.reduceActiveItemByOne();
					Game1.player.makeThisTheActiveObject(reward2);
					localSound("croak");
					Game1.flashAlpha = 1f;
				}
			}
			else if (Game1.player.ActiveObject != null && Game1.player.ActiveObject != null && !Game1.player.ActiveObject.bigCraftable && (int)Game1.player.ActiveObject.parentSheetIndex == 307)
			{
				Object reward = new Object(Vector2.Zero, 161);
				if (!Game1.player.couldInventoryAcceptThisItem(reward) && (int)Game1.player.ActiveObject.stack > 1)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					return;
				}
				Game1.player.reduceActiveItemByOne();
				Game1.player.makeThisTheActiveObject(reward);
				localSound("discoverMineral");
				Game1.flashAlpha = 1f;
			}
		}

		public virtual string FormatCompletionLine(Func<Farmer, float> check)
		{
			KeyValuePair<Farmer, float> kvp = Utility.GetFarmCompletion(check);
			if (kvp.Key == Game1.player)
			{
				return kvp.Value.ToString();
			}
			return "(" + kvp.Key.Name + ") " + kvp.Value.ToString();
		}

		public virtual string FormatCompletionLine(Func<Farmer, bool> check, string true_value, string false_value)
		{
			KeyValuePair<Farmer, bool> kvp = Utility.GetFarmCompletion(check);
			if (kvp.Key == Game1.player)
			{
				if (!kvp.Value)
				{
					return false_value;
				}
				return true_value;
			}
			return "(" + kvp.Key.Name + ") " + (kvp.Value ? true_value : false_value);
		}

		public virtual void ShowQiCat()
		{
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.MasterPlayer.mailReceived.Contains("GotPerfectionStatue"))
			{
				Game1.MasterPlayer.mailReceived.Add("GotPerfectionStatue");
				Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Object(Vector2.Zero, 280));
				return;
			}
			Game1.playSound("qi_shop");
			StringBuilder sb = new StringBuilder();
			bool smallWindow = Game1.viewport.Height < 850;
			string[] brokenUp = new string[2];
			sb.AppendLine(Utility.loadStringShort("UI", "PT_Title") + "^");
			sb.AppendLine("----------------^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_Shipped") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getFarmerItemsShippedPercent(farmer) * 100f)) + "%^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_Obelisks") + ": " + Math.Min(Utility.numObelisksOnFarm(), 4) + "/4^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_GoldClock") + ": " + (Game1.getFarm().isBuildingConstructed("Gold Clock") ? Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes") : Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_MonsterSlayer") + ": " + FormatCompletionLine((Farmer farmer) => farmer.hasCompletedAllMonsterSlayerQuests.Value, Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_GreatFriends") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getMaxedFriendshipPercent(farmer) * 100f)) + "%^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_FarmerLevel") + ": " + FormatCompletionLine((Farmer farmer) => Math.Min(farmer.Level, 25)) + "/25^");
			if (smallWindow)
			{
				sb.AppendLine("...");
				brokenUp[0] = sb.ToString();
				sb.Clear();
			}
			sb.AppendLine(Utility.loadStringShort("UI", "PT_Stardrops") + ": " + FormatCompletionLine((Farmer farmer) => Utility.foundAllStardrops(farmer), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_Cooking") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getCookedRecipesPercent(farmer) * 100f)) + "%^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_Crafting") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getCraftedRecipesPercent(farmer) * 100f)) + "%^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_Fish") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getFishCaughtPercent(farmer) * 100f)) + "%^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_GoldenWalnut") + ": " + Math.Min(Game1.netWorldState.Value.GoldenWalnutsFound, 130) + "/" + 130 + "^");
			sb.AppendLine("----------------^");
			sb.AppendLine(Utility.loadStringShort("UI", "PT_Total") + ": " + Math.Floor(Utility.percentGameComplete() * 100f) + "%^");
			if (smallWindow)
			{
				brokenUp[1] = sb.ToString();
				Game1.multipleDialogues(brokenUp);
			}
			else
			{
				Game1.drawDialogueNoTyping(sb.ToString());
			}
		}

		public virtual bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string[] actionParams = action.Split(' ');
				switch (actionParams[0])
				{
				case "SummitBoulder":
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SummitBoulder"));
					break;
				case "QiCat":
					ShowQiCat();
					break;
				case "QiGemShop":
					Game1.playSound("qi_shop");
					Game1.activeClickableMenu = new ShopMenu(Utility.GetQiChallengeRewardStock(Game1.player), 4, null, null, null, "QiGemShop")
					{
						purchaseSound = "qi_shop_purchase"
					};
					return true;
				case "QiChallengeBoard":
					Game1.player.team.qiChallengeBoardMutex.RequestLock(delegate
					{
						Game1.activeClickableMenu = new SpecialOrdersBoard("Qi")
						{
							behaviorBeforeCleanup = delegate
							{
								Game1.player.team.qiChallengeBoardMutex.ReleaseLock();
							}
						};
					});
					break;
				case "SpecialOrders":
					Game1.player.team.ordersBoardMutex.RequestLock(delegate
					{
						Game1.activeClickableMenu = new SpecialOrdersBoard
						{
							behaviorBeforeCleanup = delegate
							{
								Game1.player.team.ordersBoardMutex.ReleaseLock();
							}
						};
					});
					break;
				case "MonsterGrave":
					if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en && who.eventsSeen.Contains(6963327))
					{
						Game1.multipleDialogues(new string[2]
						{
							"Abigail took a life to save mine...",
							"I'll never forget that."
						});
					}
					break;
				case "Warp_Sunroom_Door":
					if (who.getFriendshipHeartLevelForNPC("Caroline") >= 2)
					{
						playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
						Game1.warpFarmer("Sunroom", 5, 13, flip: false);
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Caroline_Sunroom_Door"));
					}
					break;
				case "DogStatue":
				{
					if (canRespec(0) || canRespec(3) || canRespec(2) || canRespec(4) || canRespec(1))
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue"), createYesNoResponses(), "dogStatue");
						break;
					}
					string displayed_text2 = Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue");
					displayed_text2 = displayed_text2.Substring(0, displayed_text2.LastIndexOf('^'));
					Game1.drawObjectDialogue(displayed_text2);
					break;
				}
				case "WizardBook":
					if (who.mailReceived.Contains("hasPickedUpMagicInk") || who.hasMagicInk)
					{
						Game1.activeClickableMenu = new CarpenterMenu(magicalConstruction: true);
					}
					break;
				case "EvilShrineLeft":
					if (who.getChildrenCount() == 0)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineLeftInactive"));
					}
					else
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineLeft"), createYesNoResponses(), "evilShrineLeft");
					}
					break;
				case "EvilShrineCenter":
					if (who.isDivorced())
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineCenter"), createYesNoResponses(), "evilShrineCenter");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineCenterInactive"));
					}
					break;
				case "EvilShrineRight":
					if (Game1.spawnMonstersAtNight)
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineRightDeActivate"), createYesNoResponses(), "evilShrineRightDeActivate");
					}
					else
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineRightActivate"), createYesNoResponses(), "evilShrineRightActivate");
					}
					break;
				case "Tailoring":
					if (who.eventsSeen.Contains(992559))
					{
						Game1.activeClickableMenu = new TailoringMenu();
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_SewingMachine"));
					}
					break;
				case "DyePot":
					if (who.eventsSeen.Contains(992559))
					{
						if (!DyeMenu.IsWearingDyeable())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:DyePot_NoDyeable"));
						}
						else
						{
							Game1.activeClickableMenu = new DyeMenu();
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_DyePot"));
					}
					break;
				case "MagicInk":
					if (!who.mailReceived.Contains("hasPickedUpMagicInk"))
					{
						who.mailReceived.Add("hasPickedUpMagicInk");
						who.hasMagicInk = true;
						setMapTileIndex(4, 11, 113, "Buildings");
						who.addItemByMenuIfNecessaryElseHoldUp(new SpecialItem(7));
					}
					break;
				case "LeoParrot":
				{
					TemporaryAnimatedSprite tmp = getTemporarySpriteByID(5858585);
					if (tmp != null && tmp is EmilysParrot)
					{
						(tmp as EmilysParrot).doAction();
					}
					break;
				}
				case "EmilyRoomObject":
					if (Game1.player.eventsSeen.Contains(463391) && (Game1.player.spouse == null || !Game1.player.spouse.Equals("Emily")))
					{
						TemporaryAnimatedSprite t = getTemporarySpriteByID(5858585);
						if (t != null && t is EmilysParrot)
						{
							(t as EmilysParrot).doAction();
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_EmilyRoomObject"));
					}
					break;
				case "Starpoint":
					try
					{
						doStarpoint(actionParams[1]);
					}
					catch (Exception)
					{
					}
					break;
				case "JojaShop":
					Game1.activeClickableMenu = new ShopMenu(Utility.getJojaStock());
					break;
				case "ColaMachine":
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_ColaMachine_Question"), createYesNoResponses(), "buyJojaCola");
					break;
				case "IceCreamStand":
					if (isCharacterAtTile(new Vector2(tileLocation.X, tileLocation.Y - 2)) != null || isCharacterAtTile(new Vector2(tileLocation.X, tileLocation.Y - 1)) != null || isCharacterAtTile(new Vector2(tileLocation.X, tileLocation.Y - 3)) != null)
					{
						Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
						stock.Add(new Object(233, 1), new int[2]
						{
							250,
							2147483647
						});
						Game1.activeClickableMenu = new ShopMenu(stock);
					}
					else if (Game1.currentSeason.Equals("summer"))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:IceCreamStand_ComeBackLater"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:IceCreamStand_NotSummer"));
					}
					break;
				case "WizardShrine":
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WizardTower_WizardShrine").Replace('\n', '^'), createYesNoResponses(), "WizardShrine");
					break;
				case "HMTGF":
					if (who.ActiveObject != null && who.ActiveObject != null && !who.ActiveObject.bigCraftable && (int)who.ActiveObject.parentSheetIndex == 155)
					{
						Object reward = new Object(Vector2.Zero, 155);
						if (!Game1.player.couldInventoryAcceptThisItem(reward) && (int)Game1.player.ActiveObject.stack > 1)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
							break;
						}
						Game1.player.reduceActiveItemByOne();
						Game1.player.makeThisTheActiveObject(reward);
						localSound("discoverMineral");
						Game1.flashAlpha = 1f;
					}
					break;
				case "HospitalShop":
					if (isCharacterAtTile(who.getTileLocation() + new Vector2(0f, -2f)) != null || isCharacterAtTile(who.getTileLocation() + new Vector2(-1f, -2f)) != null)
					{
						Game1.activeClickableMenu = new ShopMenu(Utility.getHospitalStock());
					}
					break;
				case "BuyBackpack":
				{
					Response purchase10001 = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response2000"));
					Response purchase10000 = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response10000"));
					Response notNow = new Response("Not", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"));
					if ((int)Game1.player.maxItems == 12)
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question24"), new Response[2]
						{
							purchase10001,
							notNow
						}, "Backpack");
					}
					else if ((int)Game1.player.maxItems < 36)
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question36"), new Response[2]
						{
							purchase10000,
							notNow
						}, "Backpack");
					}
					break;
				}
				case "BuyQiCoins":
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_Buy100Coins"), createYesNoResponses(), "BuyQiCoins");
					break;
				case "LumberPile":
					if (!who.hasOrWillReceiveMail("TH_LumberPile") && who.hasOrWillReceiveMail("TH_SandDragon"))
					{
						Game1.player.hasClubCard = true;
						Game1.player.CanMove = false;
						Game1.player.mailReceived.Add("TH_LumberPile");
						Game1.player.addItemByMenuIfNecessaryElseHoldUp(new SpecialItem(2));
						Game1.player.removeQuest(5);
					}
					break;
				case "SandDragon":
					if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 768 && !who.hasOrWillReceiveMail("TH_SandDragon") && who.hasOrWillReceiveMail("TH_MayorFridge"))
					{
						who.reduceActiveItemByOne();
						Game1.player.CanMove = false;
						localSound("eat");
						Game1.player.mailReceived.Add("TH_SandDragon");
						Game1.multipleDialogues(new string[2]
						{
							Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_ConsumeEssence"),
							Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_MrQiNote")
						});
						Game1.player.removeQuest(4);
						Game1.player.addQuest(5);
					}
					else if (who.hasOrWillReceiveMail("TH_SandDragon"))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_MrQiNote"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_Initial"));
					}
					break;
				case "RailroadBox":
					if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 394 && !who.hasOrWillReceiveMail("TH_Railroad") && who.hasOrWillReceiveMail("TH_Tunnel"))
					{
						who.reduceActiveItemByOne();
						Game1.player.CanMove = false;
						localSound("Ship");
						Game1.player.mailReceived.Add("TH_Railroad");
						Game1.multipleDialogues(new string[2]
						{
							Game1.content.LoadString("Strings\\Locations:Railroad_Box_ConsumeShell"),
							Game1.content.LoadString("Strings\\Locations:Railroad_Box_MrQiNote")
						});
						Game1.player.removeQuest(2);
						Game1.player.addQuest(3);
					}
					else if (who.hasOrWillReceiveMail("TH_Railroad"))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Railroad_Box_MrQiNote"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Railroad_Box_Initial"));
					}
					break;
				case "TunnelSafe":
					if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 787 && !who.hasOrWillReceiveMail("TH_Tunnel"))
					{
						who.reduceActiveItemByOne();
						Game1.player.CanMove = false;
						playSound("openBox");
						DelayedAction.playSoundAfterDelay("doorCreakReverse", 500);
						Game1.player.mailReceived.Add("TH_Tunnel");
						Game1.multipleDialogues(new string[2]
						{
							Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_ConsumeBattery"),
							Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_MrQiNote")
						});
						Game1.player.addQuest(2);
					}
					else if (who.hasOrWillReceiveMail("TH_Tunnel"))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_MrQiNote"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_Initial"));
					}
					break;
				case "SkullDoor":
					if (who.hasSkullKey)
					{
						if (!who.hasUnlockedSkullDoor)
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:SkullCave_SkullDoor_Unlock")));
							DelayedAction.playSoundAfterDelay("openBox", 500);
							DelayedAction.playSoundAfterDelay("openBox", 700);
							Game1.addMailForTomorrow("skullCave");
							who.hasUnlockedSkullDoor = true;
							who.completeQuest(19);
						}
						else
						{
							who.completelyStopAnimatingOrDoingAction();
							playSound("doorClose");
							DelayedAction.playSoundAfterDelay("stairsdown", 500, this);
							Game1.enterMine(121);
							MineShaft.numberOfCraftedStairsUsedThisRun = 0;
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SkullCave_SkullDoor_Locked"));
					}
					break;
				case "Crib":
					foreach (NPC i in characters)
					{
						if (i is Child)
						{
							if ((i as Child).Age == 1)
							{
								(i as Child).toss(who);
								return true;
							}
							if ((i as Child).Age == 0)
							{
								Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FarmHouse_Crib_NewbornSleeping", i.displayName)));
								return true;
							}
							if ((i as Child).isInCrib() && (i as Child).Age == 2)
							{
								return i.checkAction(who, this);
							}
						}
					}
					return false;
				case "WarpGreenhouse":
					if (Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
					{
						who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
						playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
						GameLocation greenhouse = Game1.getLocationFromName("Greenhouse");
						int destination_x = 10;
						int destination_y = 23;
						if (greenhouse != null)
						{
							foreach (Warp warp in greenhouse.warps)
							{
								if (warp.TargetName == "Farm")
								{
									destination_x = warp.X;
									destination_y = warp.Y - 1;
									break;
								}
							}
						}
						Game1.warpFarmer("Greenhouse", destination_x, destination_y, flip: false);
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GreenhouseRuins"));
					}
					break;
				case "Arcade_Prairie":
					showPrairieKingMenu();
					break;
				case "Arcade_Minecart":
					if (who.hasSkullKey)
					{
						Response[] junimoKartOptions = new Response[3]
						{
							new Response("Progress", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_ProgressMode")),
							new Response("Endless", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_EndlessMode")),
							new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"))
						};
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Menu"), junimoKartOptions, "MinecartGame");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Inactive"));
					}
					break;
				case "WarpCommunityCenter":
					if (Game1.MasterPlayer.mailReceived.Contains("ccDoorUnlock") || Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
					{
						playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
						Game1.warpFarmer("CommunityCenter", 32, 23, flip: false);
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8175"));
					}
					break;
				case "AdventureShop":
					adventureShop();
					break;
				case "Warp":
					who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
					Rumble.rumble(0.15f, 200f);
					if (actionParams.Length < 5)
					{
						playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
					}
					Game1.warpFarmer(actionParams[3], Convert.ToInt32(actionParams[1]), Convert.ToInt32(actionParams[2]), flip: false);
					break;
				case "WarpWomensLocker":
					if (who.IsMale)
					{
						if (who.IsLocalPlayer)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WomensLocker_WrongGender"));
						}
						return true;
					}
					who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
					if (actionParams.Length < 5)
					{
						playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
					}
					Game1.warpFarmer(actionParams[3], Convert.ToInt32(actionParams[1]), Convert.ToInt32(actionParams[2]), flip: false);
					break;
				case "WarpMensLocker":
					if (!who.IsMale)
					{
						if (who.IsLocalPlayer)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MensLocker_WrongGender"));
						}
						return true;
					}
					who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
					if (actionParams.Length < 5)
					{
						playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
					}
					Game1.warpFarmer(actionParams[3], Convert.ToInt32(actionParams[1]), Convert.ToInt32(actionParams[2]), flip: false);
					break;
				case "LockedDoorWarp":
					who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
					lockedDoorWarp(actionParams);
					break;
				case "Door":
					if (actionParams.Length > 1 && !Game1.eventUp)
					{
						for (int j = 1; j < actionParams.Length; j++)
						{
							if (who.getFriendshipHeartLevelForNPC(actionParams[j]) >= 2 || Game1.player.mailReceived.Contains("doorUnlock" + actionParams[j]))
							{
								Rumble.rumble(0.1f, 100f);
								if (!Game1.player.mailReceived.Contains("doorUnlock" + actionParams[j]))
								{
									Game1.player.mailReceived.Add("doorUnlock" + actionParams[j]);
								}
								openDoor(tileLocation, playSound: true);
								return true;
							}
						}
						if (actionParams.Length == 2 && Game1.getCharacterFromName(actionParams[1]) != null)
						{
							NPC character4 = Game1.getCharacterFromName(actionParams[1]);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_" + ((character4.Gender == 0) ? "Male" : "Female"), character4.displayName));
						}
						else if (Game1.getCharacterFromName(actionParams[1]) != null && Game1.getCharacterFromName(actionParams[2]) != null)
						{
							NPC character3 = Game1.getCharacterFromName(actionParams[1]);
							NPC character2 = Game1.getCharacterFromName(actionParams[2]);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_Couple", character3.displayName, character2.displayName));
						}
						else if (Game1.getCharacterFromName(actionParams[1]) != null)
						{
							NPC character = Game1.getCharacterFromName(actionParams[1]);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_" + ((character.Gender == 0) ? "Male" : "Female"), character.displayName));
						}
						break;
					}
					openDoor(tileLocation, playSound: true);
					return true;
				case "Tutorial":
					Game1.activeClickableMenu = new TutorialMenu();
					break;
				case "MessageSpeech":
				case "Message":
					Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:" + actionParams[1].Replace("\"", "")));
					break;
				case "Dialogue":
					Game1.drawDialogueNoTyping(actionParamsToString(actionParams));
					break;
				case "NPCSpeechMessageNoRadius":
				{
					NPC npc2 = Game1.getCharacterFromName(actionParams[1]);
					if (npc2 != null)
					{
						try
						{
							npc2.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:" + actionParams[2]), add: true);
							Game1.drawDialogue(npc2);
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					try
					{
						NPC nPC = new NPC(null, Vector2.Zero, "", 0, actionParams[1], datable: false, null, Game1.temporaryContent.Load<Texture2D>("Portraits\\" + actionParams[1]));
						nPC.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:" + actionParams[2]), add: true);
						Game1.drawDialogue(nPC);
						return true;
					}
					catch (Exception)
					{
						return false;
					}
				}
				case "NPCMessage":
				{
					NPC npc3 = Game1.getCharacterFromName(actionParams[1]);
					if (npc3 != null && npc3.currentLocation == who.currentLocation && Utility.tileWithinRadiusOfPlayer(npc3.getTileX(), npc3.getTileY(), 14, who))
					{
						try
						{
							string str_name = action.Substring(action.IndexOf('"') + 1).Split('/')[0];
							string str_name_no_filePath = str_name.Substring(str_name.IndexOf(':') + 1);
							npc3.setNewDialogue(Game1.content.LoadString(str_name), add: true);
							Game1.drawDialogue(npc3);
							switch (str_name_no_filePath)
							{
							case "AnimalShop_Marnie_Trash":
							case "JoshHouse_Alex_Trash":
							case "SamHouse_Sam_Trash":
							case "SeedShop_Abigail_Drawers":
								if (who != null)
								{
									Game1.multiplayer.globalChatInfoMessage("Caught_Snooping", who.name, npc3.displayName);
								}
								break;
							}
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					try
					{
						Game1.drawDialogueNoTyping(Game1.content.LoadString(action.Substring(action.IndexOf('"')).Split('/')[1].Replace("\"", "")));
						return false;
					}
					catch (Exception)
					{
						return false;
					}
				}
				case "ElliottPiano":
					playElliottPiano(int.Parse(actionParams[1]));
					break;
				case "DropBox":
				{
					string box_id = actionParams[1];
					int minimum_capacity = 0;
					foreach (SpecialOrder order in Game1.player.team.specialOrders)
					{
						if (order.UsesDropBox(box_id))
						{
							minimum_capacity = Math.Max(minimum_capacity, order.GetMinimumDropBoxCapacity(box_id));
						}
					}
					foreach (SpecialOrder order2 in Game1.player.team.specialOrders)
					{
						if (order2.UsesDropBox(box_id))
						{
							order2.donateMutex.RequestLock(delegate
							{
								while (order2.donatedItems.Count < minimum_capacity)
								{
									order2.donatedItems.Add(null);
								}
								Game1.activeClickableMenu = new QuestContainerMenu(order2.donatedItems, 3, order2.HighlightAcceptableItems, order2.GetAcceptCount, order2.UpdateDonationCounts, order2.ConfirmCompleteDonations);
							});
							return true;
						}
					}
					return false;
				}
				case "playSound":
					localSound(actionParams[1]);
					break;
				case "Letter":
					Game1.drawLetterMessage(Game1.content.LoadString("Strings\\StringsFromMaps:" + actionParams[1].Replace("\"", "")));
					break;
				case "MessageOnce":
					if (!who.eventsSeen.Contains(Convert.ToInt32(actionParams[1])))
					{
						who.eventsSeen.Add(Convert.ToInt32(actionParams[1]));
						Game1.drawObjectDialogue(Game1.parseText(actionParamsToString(actionParams).Substring(actionParamsToString(actionParams).IndexOf(' '))));
					}
					break;
				case "Material":
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8205", who.WoodPieces, who.StonePieces).Replace("\n", "^"));
					break;
				case "Lamp":
					if ((float)lightLevel == 0f)
					{
						lightLevel.Value = 0.6f;
					}
					else
					{
						lightLevel.Value = 0f;
					}
					playSound("openBox");
					break;
				case "Billboard":
					Game1.activeClickableMenu = new Billboard(actionParams[1].Equals("3"));
					break;
				case "GetLumber":
					GetLumber();
					break;
				case "MinecartTransport":
					if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
					{
						createQuestionDialogue(answerChoices: (!Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom")) ? new Response[3]
						{
							new Response("Town", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Town")),
							new Response("Bus", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_BusStop")),
							new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
						} : new Response[4]
						{
							new Response("Town", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Town")),
							new Response("Bus", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_BusStop")),
							new Response("Quarry", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Quarry")),
							new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
						}, question: Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), dialogKey: "Minecart");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
					}
					break;
				case "MineElevator":
					if (MineShaft.lowestLevelReached < 5)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_MineElevator_NotWorking")));
					}
					else
					{
						Game1.activeClickableMenu = new MineElevatorMenu();
					}
					break;
				case "NextMineLevel":
				case "Mine":
					playSound("stairsdown");
					Game1.enterMine((actionParams.Length == 1) ? 1 : Convert.ToInt32(actionParams[1]));
					break;
				case "ExitMine":
				{
					Response[] responses = new Response[3]
					{
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:Mines_LeaveMine")),
						new Response("Go", Game1.content.LoadString("Strings\\Locations:Mines_GoUp")),
						new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing"))
					};
					createQuestionDialogue(" ", responses, "ExitMine");
					break;
				}
				case "GoldenScythe":
					if (!Game1.player.mailReceived.Contains("gotGoldenScythe"))
					{
						if (!Game1.player.isInventoryFull())
						{
							Game1.playSound("parry");
							Game1.player.mailReceived.Add("gotGoldenScythe");
							setMapTileIndex(29, 4, 245, "Front");
							setMapTileIndex(30, 4, 246, "Front");
							setMapTileIndex(29, 5, 261, "Front");
							setMapTileIndex(30, 5, 262, "Front");
							setMapTileIndex(29, 6, 277, "Buildings");
							setMapTileIndex(30, 56, 278, "Buildings");
							Game1.player.addItemByMenuIfNecessaryElseHoldUp(new MeleeWeapon(53));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
						}
					}
					else
					{
						Game1.changeMusicTrack("none");
						performTouchAction("MagicWarp Mine 67 10", Game1.player.getStandingPosition());
					}
					break;
				case "RemoveChest":
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:RemoveChest"));
					map.GetLayer("Buildings").Tiles[tileLocation.X, tileLocation.Y] = null;
					break;
				case "Saloon":
					if (who.getTileY() > tileLocation.Y)
					{
						return saloon(tileLocation);
					}
					return false;
				case "Carpenter":
					if (who.getTileY() > tileLocation.Y)
					{
						return carpenters(tileLocation);
					}
					return false;
				case "AnimalShop":
					if (who.getTileY() > tileLocation.Y)
					{
						return animalShop(tileLocation);
					}
					return false;
				case "Blacksmith":
					if (who.getTileY() > tileLocation.Y)
					{
						return blacksmith(tileLocation);
					}
					return false;
				case "Jukebox":
					Game1.activeClickableMenu = new ChooseFromListMenu(Game1.player.songsHeard.Distinct().ToList(), ChooseFromListMenu.playSongAction, isJukebox: true);
					break;
				case "Buy":
					if (who.getTileY() >= tileLocation.Y)
					{
						return openShopMenu(actionParams[1]);
					}
					return false;
				case "StorageBox":
					openStorageBox(actionParamsToString(actionParams));
					break;
				case "Craft":
					openCraftingMenu(actionParamsToString(actionParams));
					break;
				case "MineSign":
					Game1.drawObjectDialogue(Game1.parseText(actionParamsToString(actionParams)));
					break;
				case "MineWallDecor":
					getWallDecorItem(tileLocation);
					break;
				case "Minecart":
					openChest(tileLocation, 4, Game1.random.Next(3, 7));
					break;
				case "ItemChest":
					openItemChest(tileLocation, Convert.ToInt32(actionParams[1]));
					break;
				case "Incubator":
					(this as AnimalHouse).incubator();
					break;
				case "ClubSlots":
					Game1.currentMinigame = new Slots();
					break;
				case "ClubShop":
					Game1.activeClickableMenu = new ShopMenu(Utility.getQiShopStock(), 2);
					break;
				case "ClubCards":
				case "BlackJack":
					if (actionParams.Length > 1 && actionParams[1].Equals("1000"))
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_HS"), new Response[2]
						{
							new Response("Play", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Play")),
							new Response("Leave", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Leave"))
						}, "CalicoJackHS");
					}
					else
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack"), new Response[3]
						{
							new Response("Play", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Play")),
							new Response("Leave", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Leave")),
							new Response("Rules", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules"))
						}, "CalicoJack");
					}
					break;
				case "QiCoins":
					if (who.clubCoins > 0)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_QiCoins", who.clubCoins));
					}
					else
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_QiCoins_BuyStarter"), createYesNoResponses(), "BuyClubCoins");
					}
					break;
				case "ClubComputer":
				case "FarmerFile":
					farmerFile();
					break;
				case "ClubSeller":
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_ClubSeller"), new Response[2]
					{
						new Response("I'll", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_Yes")),
						new Response("No", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_No"))
					}, "ClubSeller");
					break;
				case "Mailbox":
					if (this is Farm)
					{
						Point mailbox_position = Game1.player.getMailboxPosition();
						if (tileLocation.X != mailbox_position.X || tileLocation.Y != mailbox_position.Y)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_OtherPlayerMailbox"));
							break;
						}
					}
					mailbox();
					break;
				case "Notes":
					readNote(Convert.ToInt32(actionParams[1]));
					break;
				case "SpiritAltar":
					if (who.ActiveObject != null && (double)Game1.player.team.sharedDailyLuck != -0.12 && (double)Game1.player.team.sharedDailyLuck != 0.12)
					{
						if (who.ActiveObject.Price >= 60)
						{
							temporarySprites.Add(new TemporaryAnimatedSprite(352, 70f, 2, 2, new Vector2(tileLocation.X * 64, tileLocation.Y * 64), flicker: false, flipped: false));
							Game1.player.team.sharedDailyLuck.Value = 0.12;
							playSound("money");
						}
						else
						{
							temporarySprites.Add(new TemporaryAnimatedSprite(362, 50f, 6, 1, new Vector2(tileLocation.X * 64, tileLocation.Y * 64), flicker: false, flipped: false));
							Game1.player.team.sharedDailyLuck.Value = -0.12;
							playSound("thunder");
						}
						who.ActiveObject = null;
						who.showNotCarrying();
					}
					break;
				case "WizardHatch":
					if (who.friendshipData.ContainsKey("Wizard") && who.friendshipData["Wizard"].Points >= 1000)
					{
						playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
						Game1.warpFarmer("WizardHouseBasement", 4, 4, flip: true);
					}
					else
					{
						NPC wizard = characters[0];
						wizard.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Data\\ExtraDialogue:Wizard_Hatch"), wizard));
						Game1.drawDialogue(wizard);
					}
					break;
				case "EnterSewer":
					if (who.hasRustyKey && !who.mailReceived.Contains("OpenedSewer"))
					{
						playSound("openBox");
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Forest_OpenedSewer")));
						who.mailReceived.Add("OpenedSewer");
					}
					else if (who.mailReceived.Contains("OpenedSewer"))
					{
						playSoundAt("stairsdown", new Vector2(tileLocation.X, tileLocation.Y));
						Game1.warpFarmer("Sewer", 16, 11, 2);
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
					}
					break;
				case "DwarfGrave":
					if (who.canUnderstandDwarves)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_DwarfGrave_Translated").Replace('\n', '^'));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8214"));
					}
					break;
				case "Yoba":
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_Yoba"));
					break;
				case "ElliottBook":
					if (who.eventsSeen.Contains(41))
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ElliottHouse_ElliottBook_Filled", Game1.elliottBookName, who.displayName)));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ElliottHouse_ElliottBook_Blank"));
					}
					break;
				case "Theater_Poster":
					if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
					{
						MovieData current_movie = MovieTheater.GetMovieForDate(Game1.Date);
						if (current_movie != null)
						{
							Game1.multipleDialogues(new string[2]
							{
								Game1.content.LoadString("Strings\\Locations:Theater_Poster_0", current_movie.Title),
								Game1.content.LoadString("Strings\\Locations:Theater_Poster_1", current_movie.Description)
							});
						}
					}
					break;
				case "Theater_PosterComingSoon":
					if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
					{
						WorldDate worldDate = new WorldDate(Game1.Date);
						worldDate.TotalDays += 28;
						MovieData current_movie2 = MovieTheater.GetMovieForDate(worldDate);
						if (current_movie2 != null)
						{
							Game1.multipleDialogues(new string[1]
							{
								Game1.content.LoadString("Strings\\Locations:Theater_Poster_Coming_Soon", current_movie2.Title)
							});
						}
					}
					break;
				case "Theater_Entrance":
				{
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
					{
						break;
					}
					if (Game1.player.team.movieMutex.IsLocked())
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_CurrentlyShowing")));
						break;
					}
					if (Game1.isFestival())
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_ClosedFestival")));
						break;
					}
					if (Game1.timeOfDay > 2100 || Game1.timeOfDay < 900)
					{
						string openTime = Game1.getTimeOfDayString(900).Replace(" ", "");
						string closeTime = Game1.getTimeOfDayString(2100).Replace(" ", "");
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", openTime, closeTime));
						break;
					}
					if ((int)Game1.player.lastSeenMovieWeek >= Game1.Date.TotalWeeks)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AlreadySeen"));
						break;
					}
					NPC invited_npc = null;
					foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
					{
						if (invitation.farmer == Game1.player && !invitation.fulfilled && MovieTheater.GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
						{
							invited_npc = invitation.invitedNPC;
							break;
						}
					}
					if (invited_npc != null && Game1.player.hasItemInInventory(809, 1))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchWithFriendPrompt", invited_npc.displayName), Game1.currentLocation.createYesNoResponses(), "EnterTheaterSpendTicket");
					}
					else if (Game1.player.hasItemInInventory(809, 1))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchAlonePrompt"), Game1.currentLocation.createYesNoResponses(), "EnterTheaterSpendTicket");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_NoTicket")));
					}
					break;
				}
				case "Theater_BoxOffice":
					if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
					{
						if (Game1.isFestival())
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_ClosedFestival")));
						}
						else if (Game1.timeOfDay > 2100)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_BoxOfficeClosed"));
						}
						else if (MovieTheater.GetMovieForDate(Game1.Date) != null)
						{
							Dictionary<ISalable, int[]> items = new Dictionary<ISalable, int[]>();
							Object obj = new Object(809, 1);
							items.Add(obj, new int[2]
							{
								obj.salePrice(),
								2147483647
							});
							Game1.activeClickableMenu = new ShopMenu(items, 0, "boxOffice");
						}
					}
					break;
				default:
					return false;
				}
				return true;
			}
			if (action != null && !who.IsLocalPlayer)
			{
				switch (action.ToString().Split(' ')[0])
				{
				case "Minecart":
					openChest(tileLocation, 4, Game1.random.Next(3, 7));
					break;
				case "RemoveChest":
					map.GetLayer("Buildings").Tiles[tileLocation.X, tileLocation.Y] = null;
					break;
				case "Door":
					openDoor(tileLocation, playSound: true);
					break;
				case "TV":
					Game1.tvStation = Game1.random.Next(2);
					break;
				}
			}
			return false;
		}

		public void showPrairieKingMenu()
		{
			if (Game1.player.jotpkProgress.Value == null)
			{
				Game1.currentMinigame = new AbigailGame();
				return;
			}
			Response[] junimoKartOptions = new Response[3]
			{
				new Response("Continue", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Continue")),
				new Response("NewGame", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_NewGame")),
				new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"))
			};
			createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Menu"), junimoKartOptions, "CowboyGame");
		}

		public Vector2 findNearestObject(Vector2 startingPoint, int objectIndex, bool bigCraftable)
		{
			int attempts = 0;
			Queue<Vector2> openList3 = new Queue<Vector2>();
			openList3.Enqueue(startingPoint);
			HashSet<Vector2> closedList3 = new HashSet<Vector2>();
			List<Vector2> adjacent2 = new List<Vector2>();
			while (attempts < 1000)
			{
				if (objects.ContainsKey(startingPoint) && (int)objects[startingPoint].parentSheetIndex == objectIndex && (bool)objects[startingPoint].bigCraftable == bigCraftable)
				{
					openList3 = null;
					closedList3 = null;
					return startingPoint;
				}
				attempts++;
				closedList3.Add(startingPoint);
				adjacent2 = Utility.getAdjacentTileLocations(startingPoint);
				for (int i = 0; i < adjacent2.Count; i++)
				{
					if (!closedList3.Contains(adjacent2[i]))
					{
						openList3.Enqueue(adjacent2[i]);
					}
				}
				startingPoint = openList3.Dequeue();
			}
			openList3 = null;
			closedList3 = null;
			return Vector2.Zero;
		}

		public void lockedDoorWarp(string[] actionParams)
		{
			bool town_key_applies = Game1.player.HasTownKey;
			if (AreStoresClosedForFestival() && Name != "Desert" && GetLocationContext() == LocationContext.Default)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FestivalDay_DoorLocked")));
				return;
			}
			if (actionParams[3].Equals("SeedShop") && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed") && !Utility.HasAnyPlayerSeenEvent(191393) && !town_key_applies)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:SeedShop_LockedWed")));
				return;
			}
			int openTime = Convert.ToInt32(actionParams[4]);
			if (actionParams[3] == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
			{
				openTime = 800;
			}
			if (town_key_applies)
			{
				if (town_key_applies && Name == "Desert")
				{
					town_key_applies = false;
				}
				if (town_key_applies && GetLocationContext() != 0)
				{
					town_key_applies = false;
				}
				if (town_key_applies && this is BeachNightMarket && actionParams[3] != "FishShop")
				{
					town_key_applies = false;
				}
			}
			if ((town_key_applies || (Game1.timeOfDay >= openTime && Game1.timeOfDay < Convert.ToInt32(actionParams[5]))) && (actionParams.Length < 7 || Game1.currentSeason.Equals("winter") || (Game1.player.friendshipData.ContainsKey(actionParams[6]) && Game1.player.friendshipData[actionParams[6]].Points >= Convert.ToInt32(actionParams[7]))))
			{
				Rumble.rumble(0.15f, 200f);
				Game1.player.completelyStopAnimatingOrDoingAction();
				playSoundAt("doorClose", Game1.player.getTileLocation());
				Game1.warpFarmer(actionParams[3], Convert.ToInt32(actionParams[1]), Convert.ToInt32(actionParams[2]), flip: false);
			}
			else if (actionParams.Length < 7)
			{
				string openTimeString = Game1.getTimeOfDayString(Convert.ToInt32(actionParams[4])).Replace(" ", "");
				if (actionParams[3] == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
				{
					openTimeString = Game1.getTimeOfDayString(800).Replace(" ", "");
				}
				string closeTime = Game1.getTimeOfDayString(Convert.ToInt32(actionParams[5])).Replace(" ", "");
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", openTimeString, closeTime));
			}
			else if (Game1.timeOfDay < Convert.ToInt32(actionParams[4]) || Game1.timeOfDay >= Convert.ToInt32(actionParams[5]))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
			}
			else
			{
				NPC character = Game1.getCharacterFromName(actionParams[6]);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_FriendsOnly", character.displayName));
			}
		}

		public void playElliottPiano(int key)
		{
			if (Game1.IsMultiplayer && (long)Game1.player.uniqueMultiplayerID % 111 == 0L)
			{
				switch (key)
				{
				case 1:
					playSoundPitched("toyPiano", 500);
					break;
				case 2:
					playSoundPitched("toyPiano", 1200);
					break;
				case 3:
					playSoundPitched("toyPiano", 1400);
					break;
				case 4:
					playSoundPitched("toyPiano", 2000);
					break;
				}
				return;
			}
			switch (key)
			{
			case 1:
				playSoundPitched("toyPiano", 1100);
				break;
			case 2:
				playSoundPitched("toyPiano", 1500);
				break;
			case 3:
				playSoundPitched("toyPiano", 1600);
				break;
			case 4:
				playSoundPitched("toyPiano", 1800);
				break;
			}
			if (Game1.elliottPiano == 0)
			{
				if (key == 2)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 1)
			{
				if (key == 4)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 2)
			{
				if (key == 3)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 3)
			{
				if (key == 2)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 4)
			{
				if (key == 3)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 5)
			{
				if (key == 4)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 6)
			{
				if (key == 2)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else
			{
				if (Game1.elliottPiano != 7)
				{
					return;
				}
				if (key == 1)
				{
					Game1.elliottPiano = 0;
					NPC elliott = getCharacterFromName("Elliott");
					if (!Game1.eventUp && elliott != null && !elliott.isMoving())
					{
						elliott.faceTowardFarmerForPeriod(1000, 100, faceAway: false, Game1.player);
						elliott.doEmote(20);
					}
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
		}

		public void readNote(int which)
		{
			if ((int)Game1.netWorldState.Value.LostBooksFound >= which)
			{
				string message = Game1.content.LoadString("Strings\\Notes:" + which).Replace('\n', '^');
				if (!Game1.player.mailReceived.Contains("lb_" + which))
				{
					Game1.player.mailReceived.Add("lb_" + which);
				}
				removeTemporarySpritesWithIDLocal(which);
				Game1.drawLetterMessage(message);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Notes:Missing")));
			}
		}

		public void mailbox()
		{
			if (Game1.mailbox.Count > 0)
			{
				if (!Game1.player.mailReceived.Contains(Game1.mailbox.First()) && !Game1.mailbox.First().Contains("passedOut") && !Game1.mailbox.First().Contains("Cooking"))
				{
					Game1.player.mailReceived.Add(Game1.mailbox.First());
				}
				string mailTitle = Game1.mailbox.First();
				Game1.mailbox.RemoveAt(0);
				Dictionary<string, string> mails = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
				string mail = mails.ContainsKey(mailTitle) ? mails[mailTitle] : "";
				string learnedRecipe = "";
				string cookingOrCrafting = "";
				if (mailTitle.StartsWith("passedOut "))
				{
					string[] split2 = mailTitle.Split(' ');
					int moneyTaken2 = (split2.Length > 1) ? Convert.ToInt32(split2[1]) : 0;
					switch (new Random(moneyTaken2).Next((Game1.player.getSpouse() != null && Game1.player.getSpouse().Name.Equals("Harvey")) ? 2 : 3))
					{
					case 0:
						mail = ((!Game1.MasterPlayer.hasCompletedCommunityCenter() || Game1.MasterPlayer.mailReceived.Contains("JojaMember")) ? string.Format(mails["passedOut1_" + ((moneyTaken2 > 0) ? "Billed" : "NotBilled") + "_" + (Game1.player.IsMale ? "Male" : "Female")], moneyTaken2) : string.Format(mails["passedOut4"], moneyTaken2));
						break;
					case 1:
						mail = string.Format(mails["passedOut2"], moneyTaken2);
						break;
					case 2:
						mail = string.Format(mails["passedOut3_" + ((moneyTaken2 > 0) ? "Billed" : "NotBilled")], moneyTaken2);
						break;
					}
				}
				else if (mailTitle.StartsWith("passedOut"))
				{
					string[] split = mailTitle.Split(' ');
					if (split.Length > 1)
					{
						int moneyTaken = Convert.ToInt32(split[1]);
						mail = string.Format(mails[split[0]], moneyTaken);
					}
				}
				if (mail.Length != 0)
				{
					Game1.activeClickableMenu = new LetterViewerMenu(mail, mailTitle);
				}
			}
			else if (Game1.mailbox.Count == 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8429"));
			}
		}

		public void farmerFile()
		{
			Game1.multipleDialogues(new string[2]
			{
				Game1.content.LoadString("Strings\\UI:FarmerFile_1", Game1.player.Name, Game1.stats.StepsTaken, Game1.stats.GiftsGiven, Game1.stats.DaysPlayed, Game1.stats.DirtHoed, Game1.stats.ItemsCrafted, Game1.stats.ItemsCooked, Game1.stats.PiecesOfTrashRecycled).Replace('\n', '^'),
				Game1.content.LoadString("Strings\\UI:FarmerFile_2", Game1.stats.MonstersKilled, Game1.stats.FishCaught, Game1.stats.TimesFished, Game1.stats.SeedsSown, Game1.stats.ItemsShipped).Replace('\n', '^')
			});
		}

		public void openItemChest(Location location, int whichObject)
		{
			playSound("openBox");
			if (Game1.player.ActiveObject == null)
			{
				if (whichObject == 434)
				{
					Game1.player.ActiveObject = new Object(Vector2.Zero, 434, "Cosmic Fruit", canBeSetDown: false, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
					Game1.player.eatHeldObject();
				}
				else
				{
					debris.Add(new Debris(whichObject, new Vector2(location.X * 64, location.Y * 64), Game1.player.Position));
				}
				map.GetLayer("Buildings").Tiles[location.X, location.Y].TileIndex++;
				map.GetLayer("Buildings").Tiles[location].Properties["Action"] = new PropertyValue("RemoveChest");
			}
		}

		public void getWallDecorItem(Location location)
		{
		}

		public static string getFavoriteItemName(string character)
		{
			string favoriteItem = "???";
			if (Game1.NPCGiftTastes.ContainsKey(character))
			{
				string[] favoriteItems = Game1.NPCGiftTastes[character].Split('/')[1].Split(' ');
				favoriteItem = Game1.objectInformation[Convert.ToInt32(favoriteItems[Game1.random.Next(favoriteItems.Length)])].Split('/')[0];
			}
			return favoriteItem;
		}

		public static void openCraftingMenu(string nameOfCraftingDevice)
		{
			Game1.activeClickableMenu = new GameMenu(4);
		}

		private void openStorageBox(string which)
		{
		}

		public virtual bool openShopMenu(string which)
		{
			if (which.Equals("Fish"))
			{
				if (getCharacterFromName("Willy") != null && getCharacterFromName("Willy").getTileLocation().Y < (float)Game1.player.getTileY())
				{
					Game1.activeClickableMenu = new ShopMenu(Utility.getFishShopStock(Game1.player), 0, "Willy");
					return true;
				}
				return false;
			}
			if (this is SeedShop)
			{
				if (getCharacterFromName("Pierre") != null && getCharacterFromName("Pierre").getTileLocation().Equals(new Vector2(4f, 17f)) && Game1.player.getTileY() > getCharacterFromName("Pierre").getTileY())
				{
					Game1.activeClickableMenu = new ShopMenu((this as SeedShop).shopStock(), 0, "Pierre");
				}
				else if (getCharacterFromName("Pierre") == null && Game1.IsVisitingIslandToday("Pierre"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_MoneyBox"));
					Game1.afterDialogues = delegate
					{
						Game1.activeClickableMenu = new ShopMenu((this as SeedShop).shopStock());
					};
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8525"));
				}
				return true;
			}
			if (name.Equals("SandyHouse"))
			{
				NPC sandy = getCharacterFromName("Sandy");
				if (sandy != null && sandy.currentLocation == this)
				{
					Game1.activeClickableMenu = new ShopMenu(sandyShopStock(), 0, "Sandy", onSandyShopPurchase);
				}
				return true;
			}
			return false;
		}

		public virtual bool isObjectAt(int x, int y)
		{
			Vector2 v = new Vector2(x / 64, y / 64);
			foreach (Furniture item in furniture)
			{
				if (item.boundingBox.Value.Contains(x, y))
				{
					return true;
				}
			}
			if (objects.ContainsKey(v))
			{
				return true;
			}
			return false;
		}

		public virtual bool isObjectAtTile(int tileX, int tileY)
		{
			Vector2 v = new Vector2(tileX, tileY);
			foreach (Furniture item in furniture)
			{
				if (item.boundingBox.Value.Contains(tileX * 64, tileY * 64))
				{
					return true;
				}
			}
			if (objects.ContainsKey(v))
			{
				return true;
			}
			return false;
		}

		public virtual Object getObjectAt(int x, int y)
		{
			Vector2 v = new Vector2(x / 64, y / 64);
			foreach (Furniture f in furniture)
			{
				if (f.boundingBox.Value.Contains(x, y))
				{
					return f;
				}
			}
			if (objects.ContainsKey(v))
			{
				return objects[v];
			}
			return null;
		}

		public Object getObjectAtTile(int x, int y)
		{
			return getObjectAt(x * 64, y * 64);
		}

		private bool onSandyShopPurchase(ISalable item, Farmer who, int amount)
		{
			Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Sandy, item, amount);
			return false;
		}

		private Dictionary<ISalable, int[]> sandyShopStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			Utility.AddStock(stock, new Object(802, int.MaxValue), (int)(75f * Game1.MasterPlayer.difficultyModifier));
			Utility.AddStock(stock, new Object(478, int.MaxValue));
			Utility.AddStock(stock, new Object(486, int.MaxValue));
			Utility.AddStock(stock, new Object(494, int.MaxValue));
			Utility.AddStock(stock, new Object(Vector2.Zero, 196)
			{
				Stack = int.MaxValue
			});
			switch (Game1.dayOfMonth % 7)
			{
			case 0:
				Utility.AddStock(stock, new Object(233, int.MaxValue));
				break;
			case 1:
				Utility.AddStock(stock, new Object(88, 1), 200, 10);
				break;
			case 2:
				Utility.AddStock(stock, new Object(90, int.MaxValue));
				break;
			case 3:
				Utility.AddStock(stock, new Object(749, 1), 500, 3);
				break;
			case 4:
				Utility.AddStock(stock, new Object(466, int.MaxValue));
				break;
			case 5:
				Utility.AddStock(stock, new Object(340, int.MaxValue));
				break;
			case 6:
				Utility.AddStock(stock, new Object(371, int.MaxValue), 100);
				break;
			}
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			Clothing shirt = new Clothing(1000 + r.Next(127));
			stock.Add(shirt, new int[2]
			{
				1000,
				2147483647
			});
			stock.Add(new Furniture(2655, Vector2.Zero), new int[2]
			{
				700,
				2147483647
			});
			switch (Game1.dayOfMonth % 7)
			{
			case 0:
				stock.Add(new Furniture(2720, Vector2.Zero), new int[2]
				{
					3000,
					2147483647
				});
				break;
			case 1:
				stock.Add(new Furniture(2802, Vector2.Zero), new int[2]
				{
					2000,
					2147483647
				});
				break;
			case 2:
				stock.Add(new Furniture(2734 + r.Next(4) * 2, Vector2.Zero), new int[2]
				{
					500,
					2147483647
				});
				break;
			case 3:
				stock.Add(new Furniture(2584, Vector2.Zero), new int[2]
				{
					5000,
					2147483647
				});
				break;
			case 4:
				stock.Add(new Furniture(2794, Vector2.Zero), new int[2]
				{
					2500,
					2147483647
				});
				break;
			case 5:
				stock.Add(new Furniture(2784, Vector2.Zero), new int[2]
				{
					2500,
					2147483647
				});
				break;
			case 6:
				stock.Add(new Furniture(2748, Vector2.Zero), new int[2]
				{
					500,
					2147483647
				});
				stock.Add(new Furniture(2812, Vector2.Zero), new int[2]
				{
					500,
					2147483647
				});
				break;
			}
			Game1.player.team.synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(SynchronizedShopStock.SynchedShop.Sandy, stock);
			return stock;
		}

		public virtual bool saloon(Location tileLocation)
		{
			foreach (NPC i in characters)
			{
				if (i.Name.Equals("Gus"))
				{
					if (i.getTileY() != Game1.player.getTileY() - 1 && i.getTileY() != Game1.player.getTileY() - 2)
					{
						return false;
					}
					i.facePlayer(Game1.player);
					Game1.activeClickableMenu = new ShopMenu(Utility.getSaloonStock(), 0, "Gus", delegate(ISalable item, Farmer farmer, int amount)
					{
						Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Saloon, item, amount);
						return false;
					});
					return true;
				}
			}
			if (getCharacterFromName("Gus") == null && Game1.IsVisitingIslandToday("Gus"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_MoneyBox"));
				Game1.afterDialogues = delegate
				{
					Game1.activeClickableMenu = new ShopMenu(Utility.getSaloonStock(), 0, null, delegate(ISalable item, Farmer farmer, int amount)
					{
						Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Saloon, item, amount);
						return false;
					});
				};
				return true;
			}
			return false;
		}

		private void adventureShop()
		{
			if (Game1.player.itemsLostLastDeath.Count > 0)
			{
				List<Response> options = new List<Response>();
				options.Add(new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")));
				options.Add(new Response("Recovery", Game1.content.LoadString("Strings\\Locations:AdventureGuild_ItemRecovery")));
				options.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave")));
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:AdventureGuild_Greeting"), options.ToArray(), "adventureGuild");
			}
			else
			{
				Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureShopStock(), 0, "Marlon");
			}
		}

		public virtual bool carpenters(Location tileLocation)
		{
			if (Game1.player.currentUpgrade == null)
			{
				foreach (NPC i in characters)
				{
					if (i.Name.Equals("Robin"))
					{
						if (Vector2.Distance(i.getTileLocation(), new Vector2(tileLocation.X, tileLocation.Y)) > 3f)
						{
							return false;
						}
						i.faceDirection(2);
						if ((int)Game1.player.daysUntilHouseUpgrade < 0 && !Game1.getFarm().isThereABuildingUnderConstruction())
						{
							List<Response> options = new List<Response>();
							options.Add(new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")));
							if (Game1.IsMasterGame)
							{
								if ((int)Game1.player.houseUpgradeLevel < 3)
								{
									options.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeHouse")));
								}
								else if ((Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.mailReceived.Contains("JojaMember") || Game1.MasterPlayer.hasCompletedCommunityCenter()) && (int)(Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade <= 0)
								{
									if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
									{
										options.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
									}
									else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
									{
										options.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
									}
								}
							}
							else if ((int)Game1.player.houseUpgradeLevel < 3)
							{
								options.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeCabin")));
							}
							if ((int)Game1.player.houseUpgradeLevel >= 2)
							{
								if (Game1.IsMasterGame)
								{
									options.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateHouse")));
								}
								else
								{
									options.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateCabin")));
								}
							}
							options.Add(new Response("Construct", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct")));
							options.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave")));
							createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu"), options.ToArray(), "carpenter");
						}
						else
						{
							Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
						}
						return true;
					}
				}
				if (getCharacterFromName("Robin") == null && Game1.IsVisitingIslandToday("Robin"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_MoneyBox"));
					Game1.afterDialogues = delegate
					{
						Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock());
					};
					return true;
				}
				if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_RobinAbsent").Replace('\n', '^'));
					return true;
				}
				return false;
			}
			return false;
		}

		public virtual bool blacksmith(Location tileLocation)
		{
			foreach (NPC i in characters)
			{
				if (i.Name.Equals("Clint"))
				{
					if (!i.getTileLocation().Equals(new Vector2(tileLocation.X, tileLocation.Y - 1)))
					{
						i.getTileLocation().Equals(new Vector2(tileLocation.X - 1, tileLocation.Y - 1));
					}
					i.faceDirection(2);
					if (Game1.player.toolBeingUpgraded.Value == null)
					{
						Response[] responses = (!Game1.player.hasItemInInventory(535, 1) && !Game1.player.hasItemInInventory(536, 1) && !Game1.player.hasItemInInventory(537, 1) && !Game1.player.hasItemInInventory(749, 1) && !Game1.player.hasItemInInventory(275, 1) && !Game1.player.hasItemInInventory(791, 1)) ? new Response[3]
						{
							new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
							new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
							new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
						} : new Response[4]
						{
							new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
							new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
							new Response("Process", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Geodes")),
							new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
						};
						createQuestionDialogue("", responses, "Blacksmith");
					}
					else if ((int)Game1.player.daysLeftForToolUpgrade <= 0)
					{
						if (Game1.player.freeSpotsInInventory() > 0 || Game1.player.toolBeingUpgraded.Value is GenericTool)
						{
							Tool tool = Game1.player.toolBeingUpgraded.Value;
							Game1.player.toolBeingUpgraded.Value = null;
							Game1.player.hasReceivedToolUpgradeMessageYet = false;
							Game1.player.holdUpItemThenMessage(tool);
							if (tool is GenericTool)
							{
								tool.actionWhenClaimed();
							}
							else
							{
								Game1.player.addItemToInventoryBool(tool);
							}
							if (Game1.player.team.useSeparateWallets.Value && tool.UpgradeLevel == 4)
							{
								Game1.multiplayer.globalChatInfoMessage("IridiumToolUpgrade", Game1.player.Name, tool.DisplayName);
							}
						}
						else
						{
							Game1.drawDialogue(i, Game1.content.LoadString("Data\\ExtraDialogue:Clint_NoInventorySpace"));
						}
					}
					else
					{
						Game1.drawDialogue(i, Game1.content.LoadString("Data\\ExtraDialogue:Clint_StillWorking", Game1.player.toolBeingUpgraded.Value.DisplayName));
					}
					return true;
				}
			}
			return false;
		}

		public virtual bool animalShop(Location tileLocation)
		{
			foreach (NPC i in characters)
			{
				if (i.Name.Equals("Marnie"))
				{
					if (!i.getTileLocation().Equals(new Vector2(tileLocation.X, tileLocation.Y - 1)))
					{
						return false;
					}
					i.faceDirection(2);
					Response[] options = new Response[3]
					{
						new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
						new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
					};
					createQuestionDialogue("", options, "Marnie");
					return true;
				}
			}
			if (getCharacterFromName("Marnie") == null && Game1.IsVisitingIslandToday("Marnie"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:AnimalShop_MoneyBox"));
				Game1.afterDialogues = delegate
				{
					Game1.activeClickableMenu = new ShopMenu(Utility.getAnimalShopStock());
				};
				return true;
			}
			if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Absent").Replace('\n', '^'));
				return true;
			}
			return false;
		}

		private void gunther()
		{
			if ((this as LibraryMuseum).doesFarmerHaveAnythingToDonate(Game1.player))
			{
				Response[] choice = ((this as LibraryMuseum).getRewardsForPlayer(Game1.player).Count <= 0) ? new Response[2]
				{
					new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				} : new Response[3]
				{
					new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
					new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				};
				createQuestionDialogue("", choice, "Museum");
			}
			else if ((this as LibraryMuseum).getRewardsForPlayer(Game1.player).Count > 0)
			{
				createQuestionDialogue("", new Response[2]
				{
					new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				}, "Museum");
			}
			else if (Game1.player.achievements.Contains(5))
			{
				Game1.drawDialogue(Game1.getCharacterFromName("Gunther"), Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_MuseumComplete")));
			}
			else
			{
				Game1.drawDialogue(Game1.getCharacterFromName("Gunther"), Game1.player.mailReceived.Contains("artifactFound") ? Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NothingToDonate")) : Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NoArtifactsFound"));
			}
		}

		public void openChest(Location location, int debrisType, int numberOfChunks)
		{
			int[] debris = new int[1]
			{
				debrisType
			};
			openChest(location, debris, numberOfChunks);
		}

		public void openChest(Location location, int[] debrisType, int numberOfChunks)
		{
			playSound("openBox");
			map.GetLayer("Buildings").Tiles[location.X, location.Y].TileIndex++;
			for (int i = 0; i < debrisType.Length; i++)
			{
				Game1.createDebris(debrisType[i], location.X, location.Y, numberOfChunks);
			}
			map.GetLayer("Buildings").Tiles[location].Properties["Action"] = new PropertyValue("RemoveChest");
		}

		public string actionParamsToString(string[] actionparams)
		{
			string str = actionparams[1];
			for (int i = 2; i < actionparams.Length; i++)
			{
				str = str + " " + actionparams[i];
			}
			return str;
		}

		private void GetLumber()
		{
			if (Game1.player.ActiveObject == null && Game1.player.WoodPieces > 0)
			{
				Game1.player.grabObject(new Object(Vector2.Zero, 30, "Lumber", canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false));
				Game1.player.WoodPieces--;
			}
			else if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.Name.Equals("Lumber"))
			{
				Game1.player.CanMove = false;
				switch (Game1.player.FacingDirection)
				{
				case 2:
					((FarmerSprite)Game1.player.Sprite).animateBackwardsOnce(64, 75f);
					break;
				case 1:
					((FarmerSprite)Game1.player.Sprite).animateBackwardsOnce(72, 75f);
					break;
				case 0:
					((FarmerSprite)Game1.player.Sprite).animateBackwardsOnce(80, 75f);
					break;
				case 3:
					((FarmerSprite)Game1.player.Sprite).animateBackwardsOnce(88, 75f);
					break;
				}
				Game1.player.ActiveObject = null;
				Game1.player.WoodPieces++;
			}
		}

		public void removeTile(Location tileLocation, string layer)
		{
			Map.GetLayer(layer).Tiles[tileLocation.X, tileLocation.Y] = null;
		}

		public void removeTile(int x, int y, string layer)
		{
			Map.GetLayer(layer).Tiles[x, y] = null;
		}

		public void characterTrampleTile(Vector2 tile)
		{
			if (!(this is FarmHouse) && !(this is IslandFarmHouse) && !(this is Farm))
			{
				terrainFeatures.TryGetValue(tile, out TerrainFeature tf);
				if (tf != null && tf is Tree && (int)(tf as Tree).growthStage < 1 && (tf as Tree).instantDestroy(tile, this))
				{
					terrainFeatures.Remove(tile);
				}
			}
		}

		public bool characterDestroyObjectWithinRectangle(Microsoft.Xna.Framework.Rectangle rect, bool showDestroyedObject)
		{
			if (this is FarmHouse || this is IslandFarmHouse)
			{
				return false;
			}
			foreach (Farmer farmer in farmers)
			{
				if (rect.Intersects(farmer.GetBoundingBox()))
				{
					return false;
				}
			}
			Vector2 tilePositionToTry = new Vector2(rect.X / 64, rect.Y / 64);
			objects.TryGetValue(tilePositionToTry, out Object o);
			if (checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
			{
				return true;
			}
			terrainFeatures.TryGetValue(tilePositionToTry, out TerrainFeature tf);
			if (checkDestroyTerrainFeature(tf, tilePositionToTry))
			{
				return true;
			}
			tilePositionToTry.X = rect.Right / 64;
			objects.TryGetValue(tilePositionToTry, out o);
			if (checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
			{
				return true;
			}
			terrainFeatures.TryGetValue(tilePositionToTry, out tf);
			if (checkDestroyTerrainFeature(tf, tilePositionToTry))
			{
				return true;
			}
			tilePositionToTry.X = rect.X / 64;
			tilePositionToTry.Y = rect.Bottom / 64;
			objects.TryGetValue(tilePositionToTry, out o);
			if (checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
			{
				return true;
			}
			terrainFeatures.TryGetValue(tilePositionToTry, out tf);
			if (checkDestroyTerrainFeature(tf, tilePositionToTry))
			{
				return true;
			}
			tilePositionToTry.X = rect.Right / 64;
			objects.TryGetValue(tilePositionToTry, out o);
			if (checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
			{
				return true;
			}
			terrainFeatures.TryGetValue(tilePositionToTry, out tf);
			if (checkDestroyTerrainFeature(tf, tilePositionToTry))
			{
				return true;
			}
			return false;
		}

		private bool checkDestroyTerrainFeature(TerrainFeature tf, Vector2 tilePositionToTry)
		{
			if (tf != null && tf is Tree && (tf as Tree).instantDestroy(tilePositionToTry, this))
			{
				terrainFeatures.Remove(tilePositionToTry);
			}
			return false;
		}

		private bool checkDestroyItem(Object o, Vector2 tilePositionToTry, bool showDestroyedObject)
		{
			if (o != null && !o.IsHoeDirt && !o.isPassable() && !map.GetLayer("Back").Tiles[(int)tilePositionToTry.X, (int)tilePositionToTry.Y].Properties.ContainsKey("NPCBarrier"))
			{
				if (!o.IsHoeDirt)
				{
					if (o.IsSpawnedObject)
					{
						numberOfSpawnedObjectsOnMap--;
					}
					if (showDestroyedObject && !o.bigCraftable)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(o.ParentSheetIndex, 150f, 1, 3, new Vector2(tilePositionToTry.X * 64f, tilePositionToTry.Y * 64f), flicker: false, o.flipped)
						{
							alphaFade = 0.01f
						});
					}
					o.performToolAction(null, this);
					if (objects.ContainsKey(tilePositionToTry))
					{
						if (o is Chest)
						{
							Chest chest = o as Chest;
							if ((chest.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin || chest.SpecialChestType == Chest.SpecialChestTypes.JunimoChest) && chest.MoveToSafePosition(this, tilePositionToTry))
							{
								return true;
							}
							chest.destroyAndDropContents(tilePositionToTry * 64f, this);
						}
						objects.Remove(tilePositionToTry);
					}
				}
				return true;
			}
			return false;
		}

		public Object removeObject(Vector2 location, bool showDestroyedObject)
		{
			objects.TryGetValue(location, out Object o);
			if (o != null && (o.CanBeGrabbed | showDestroyedObject))
			{
				if (o.IsSpawnedObject)
				{
					numberOfSpawnedObjectsOnMap--;
				}
				Object tmp = objects[location];
				objects.Remove(location);
				if (showDestroyedObject)
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(tmp.Type.Equals("Crafting") ? tmp.ParentSheetIndex : (tmp.ParentSheetIndex + 1), 150f, 1, 3, new Vector2(location.X * 64f, location.Y * 64f), flicker: true, tmp.bigCraftable, tmp.flipped));
				}
				if (o.Name.Contains("Weed"))
				{
					Game1.stats.WeedsEliminated++;
				}
				return tmp;
			}
			return null;
		}

		public void removeTileProperty(int tileX, int tileY, string layer, string key)
		{
			try
			{
				if (map != null)
				{
					Layer i = map.GetLayer(layer);
					if (i != null)
					{
						Tile tile = i.Tiles[tileX, tileY];
						if (tile != null)
						{
							IPropertyCollection prop = tile.Properties;
							if (prop.ContainsKey(key))
							{
								prop.Remove(key);
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public void setTileProperty(int tileX, int tileY, string layer, string key, string value)
		{
			try
			{
				if (map != null)
				{
					Layer i = map.GetLayer(layer);
					if (i != null)
					{
						Tile tile = i.Tiles[tileX, tileY];
						if (tile != null)
						{
							IPropertyCollection prop = tile.Properties;
							if (!prop.ContainsKey(key))
							{
								prop.Add(key, new PropertyValue(value));
							}
							else
							{
								prop[key] = value;
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		private void removeDirt(Vector2 location)
		{
			objects.TryGetValue(location, out Object o);
			if (o != null && o.IsHoeDirt)
			{
				objects.Remove(location);
			}
		}

		public void removeBatch(List<Vector2> locations)
		{
			foreach (Vector2 v in locations)
			{
				objects.Remove(v);
			}
		}

		public void setObjectAt(float x, float y, Object o)
		{
			Vector2 v = new Vector2(x, y);
			if (objects.ContainsKey(v))
			{
				objects[v] = o;
			}
			else
			{
				objects.Add(v, o);
			}
		}

		public virtual void cleanupBeforeSave()
		{
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				if (characters[i] is Junimo)
				{
					characters.RemoveAt(i);
				}
			}
			if (name.Equals("WitchHut"))
			{
				characters.Clear();
			}
		}

		public virtual void cleanupForVacancy()
		{
			int i = 0;
			while (i < debris.Count)
			{
				Debris d = debris[i];
				if (d.isEssentialItem() && Game1.IsMasterGame && d.collect(Game1.player))
				{
					debris.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}

		public virtual void cleanupBeforePlayerExit()
		{
			int l = 0;
			while (l < debris.Count)
			{
				Debris d = debris[l];
				if (d.isEssentialItem() && d.player.Value != null && d.player.Value == Game1.player && d.collect(d.player))
				{
					debris.RemoveAt(l);
				}
				else
				{
					l++;
				}
			}
			Game1.currentLightSources.Clear();
			if (critters != null)
			{
				critters.Clear();
			}
			for (int k = Game1.onScreenMenus.Count - 1; k >= 0; k--)
			{
				IClickableMenu menu = Game1.onScreenMenus[k];
				if (menu.destroy)
				{
					Game1.onScreenMenus.RemoveAt(k);
					if (menu is IDisposable)
					{
						(menu as IDisposable).Dispose();
					}
				}
			}
			if (Game1.getMusicTrackName() == "sam_acoustic1")
			{
				Game1.changeMusicTrack("none");
			}
			bool nextLocationOutdoors = Game1.locationRequest == null || Game1.locationRequest.Location == null || Game1.locationRequest.Location.IsOutdoors;
			if (((!Game1.getMusicTrackName().Contains(Game1.currentSeason) && !Game1.getMusicTrackName().Contains("night_ambient") && !Game1.getMusicTrackName().Contains("day_ambient") && !Game1.getMusicTrackName().Equals("rain") && !Game1.eventUp && (bool)isOutdoors) & nextLocationOutdoors) && GetLocationContext() == LocationContext.Default)
			{
				Game1.changeMusicTrack("none");
			}
			AmbientLocationSounds.onLocationLeave();
			if (((name.Equals("AnimalShop") || name.Equals("ScienceHouse")) && Game1.getMusicTrackName().Equals("marnieShop")) & nextLocationOutdoors)
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("Saloon") && (Game1.getMusicTrackName().Contains("Saloon") || Game1.startedJukeboxMusic))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("LeahHouse"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("ElliottHouse"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandSouthEastCave"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandFarmCave"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandNorthCave1"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandWestCave1"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandFieldOffice"))
			{
				Game1.changeMusicTrack("none");
			}
			if (this is LibraryMuseum || this is JojaMart)
			{
				Game1.changeMusicTrack("none");
			}
			Game1.startedJukeboxMusic = false;
			if ((name.Equals("Hospital") && Game1.getMusicTrackName().Equals("distantBanjo")) & nextLocationOutdoors)
			{
				Game1.changeMusicTrack("none");
			}
			if (Game1.player.rightRing.Value != null)
			{
				Game1.player.rightRing.Value.onLeaveLocation(Game1.player, this);
			}
			if (Game1.player.leftRing.Value != null)
			{
				Game1.player.leftRing.Value.onLeaveLocation(Game1.player, this);
			}
			if (Game1.locationRequest == null || (string)name != Game1.locationRequest.Name)
			{
				foreach (NPC j in characters)
				{
					if (j.uniqueSpriteActive)
					{
						j.Sprite.LoadTexture("Characters\\" + NPC.getTextureNameForCharacter(j.Name));
						j.uniqueSpriteActive = false;
					}
					if (j.uniquePortraitActive)
					{
						j.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + NPC.getTextureNameForCharacter(j.Name));
						j.uniquePortraitActive = false;
					}
				}
			}
			if (name.Equals("AbandonedJojaMart"))
			{
				if (farmers.Count <= 1)
				{
					for (int i = characters.Count - 1; i >= 0; i--)
					{
						if (characters[i] is Junimo)
						{
							characters.RemoveAt(i);
						}
					}
				}
				Game1.changeMusicTrack("none");
			}
			furnitureToRemove.Clear();
			Game1.stopMusicTrack(Game1.MusicContext.SubLocation);
			interiorDoors.CleanUpLocalState();
			Game1.temporaryContent.Unload();
			Utility.CollectGarbage();
		}

		public static int getWeedForSeason(Random r, string season)
		{
			if (!(season == "spring"))
			{
				if (!(season == " summer"))
				{
					if (!(season == "fall"))
					{
						if (!(season == "winter"))
						{
						}
						return 674;
					}
					if (!(r.NextDouble() < 0.33))
					{
						if (!(r.NextDouble() < 0.5))
						{
							return 679;
						}
						return 678;
					}
					return 786;
				}
				if (!(r.NextDouble() < 0.33))
				{
					if (!(r.NextDouble() < 0.5))
					{
						return 677;
					}
					return 676;
				}
				return 785;
			}
			if (!(r.NextDouble() < 0.33))
			{
				if (!(r.NextDouble() < 0.5))
				{
					return 675;
				}
				return 674;
			}
			return 784;
		}

		private void startSleep()
		{
			Game1.player.timeWentToBed.Value = Game1.timeOfDay;
			if (Game1.IsMultiplayer)
			{
				Game1.player.team.SetLocalReady("sleep", ready: true);
				Game1.dialogueUp = false;
				Game1.activeClickableMenu = new ReadyCheckDialog("sleep", allowCancel: true, delegate
				{
					doSleep();
				}, delegate(Farmer who)
				{
					if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog)
					{
						(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(who);
					}
					who.timeWentToBed.Value = 0;
				});
			}
			else
			{
				doSleep();
			}
			if (Game1.player.team.announcedSleepingFarmers.Contains(Game1.player))
			{
				return;
			}
			Game1.player.team.announcedSleepingFarmers.Add(Game1.player);
			if (!Game1.IsMultiplayer || ((FarmerTeam.SleepAnnounceModes)Game1.player.team.sleepAnnounceMode != 0 && ((FarmerTeam.SleepAnnounceModes)Game1.player.team.sleepAnnounceMode != FarmerTeam.SleepAnnounceModes.First || Game1.player.team.announcedSleepingFarmers.Count() != 1)))
			{
				return;
			}
			string key = "GoneToBed";
			if (Game1.random.NextDouble() < 0.75)
			{
				if (Game1.timeOfDay < 1800)
				{
					key += "Early";
				}
				else if (Game1.timeOfDay > 2530)
				{
					key += "Late";
				}
			}
			int key_index = 0;
			for (int i = 0; i < 2; i++)
			{
				if (Game1.random.NextDouble() < 0.25)
				{
					key_index++;
				}
			}
			Game1.multiplayer.globalChatInfoMessage(key + key_index, Game1.player.displayName);
		}

		private void doSleep()
		{
			if ((float)lightLevel == 0f && Game1.timeOfDay < 2000)
			{
				lightLevel.Value = 0.6f;
				localSound("openBox");
				if (Game1.IsMasterGame)
				{
					Game1.NewDay(600f);
				}
			}
			else if ((float)lightLevel > 0f && Game1.timeOfDay >= 2000)
			{
				lightLevel.Value = 0f;
				localSound("openBox");
				if (Game1.IsMasterGame)
				{
					Game1.NewDay(600f);
				}
			}
			else if (Game1.IsMasterGame)
			{
				Game1.NewDay(0f);
			}
			Game1.player.lastSleepLocation.Value = Game1.currentLocation.NameOrUniqueName;
			Game1.player.lastSleepPoint.Value = Game1.player.getTileLocationPoint();
			Game1.player.mostRecentBed = Game1.player.Position;
			Game1.player.doEmote(24);
			Game1.player.freezePause = 2000;
		}

		public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			switch (questionAndAnswer)
			{
			case "EnterTheaterSpendTicket_Yes":
				Game1.player.removeItemsFromInventory(809, 1);
				Rumble.rumble(0.15f, 200f);
				Game1.player.completelyStopAnimatingOrDoingAction();
				playSoundAt("doorClose", Game1.player.getTileLocation());
				Game1.warpFarmer("MovieTheater", 13, 15, 0);
				break;
			case "EnterTheater_Yes":
				Rumble.rumble(0.15f, 200f);
				Game1.player.completelyStopAnimatingOrDoingAction();
				playSoundAt("doorClose", Game1.player.getTileLocation());
				Game1.warpFarmer("MovieTheater", 13, 15, 0);
				break;
			case "dogStatue_Yes":
			{
				if (Game1.player.Money < 10000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
					break;
				}
				List<Response> skill_responses = new List<Response>();
				if (canRespec(0))
				{
					skill_responses.Add(new Response("farming", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604")));
				}
				if (canRespec(3))
				{
					skill_responses.Add(new Response("mining", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605")));
				}
				if (canRespec(2))
				{
					skill_responses.Add(new Response("foraging", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606")));
				}
				if (canRespec(1))
				{
					skill_responses.Add(new Response("fishing", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607")));
				}
				if (canRespec(4))
				{
					skill_responses.Add(new Response("combat", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608")));
				}
				skill_responses.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueCancel")));
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueQuestion"), skill_responses.ToArray(), "professionForget");
				break;
			}
			case "professionForget_farming":
			{
				if (Game1.player.newLevels.Contains(new Point(0, 5)) || Game1.player.newLevels.Contains(new Point(0, 10)))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
					break;
				}
				Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
				RemoveProfession(0);
				RemoveProfession(1);
				RemoveProfession(3);
				RemoveProfession(5);
				RemoveProfession(2);
				RemoveProfession(4);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
				int num4 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[0]);
				if (num4 >= 5)
				{
					Game1.player.newLevels.Add(new Point(0, 5));
				}
				if (num4 >= 10)
				{
					Game1.player.newLevels.Add(new Point(0, 10));
				}
				DelayedAction.playSoundAfterDelay("dog_bark", 300);
				DelayedAction.playSoundAfterDelay("dog_bark", 900);
				break;
			}
			case "professionForget_mining":
			{
				if (Game1.player.newLevels.Contains(new Point(3, 5)) || Game1.player.newLevels.Contains(new Point(3, 10)))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
					break;
				}
				Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
				RemoveProfession(23);
				RemoveProfession(21);
				RemoveProfession(18);
				RemoveProfession(19);
				RemoveProfession(22);
				RemoveProfession(20);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
				int num6 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[3]);
				if (num6 >= 5)
				{
					Game1.player.newLevels.Add(new Point(3, 5));
				}
				if (num6 >= 10)
				{
					Game1.player.newLevels.Add(new Point(3, 10));
				}
				DelayedAction.playSoundAfterDelay("dog_bark", 300);
				DelayedAction.playSoundAfterDelay("dog_bark", 900);
				break;
			}
			case "professionForget_foraging":
			{
				if (Game1.player.newLevels.Contains(new Point(2, 5)) || Game1.player.newLevels.Contains(new Point(2, 10)))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
					break;
				}
				Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
				RemoveProfession(16);
				RemoveProfession(14);
				RemoveProfession(17);
				RemoveProfession(12);
				RemoveProfession(13);
				RemoveProfession(15);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
				int num5 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[2]);
				if (num5 >= 5)
				{
					Game1.player.newLevels.Add(new Point(2, 5));
				}
				if (num5 >= 10)
				{
					Game1.player.newLevels.Add(new Point(2, 10));
				}
				DelayedAction.playSoundAfterDelay("dog_bark", 300);
				DelayedAction.playSoundAfterDelay("dog_bark", 900);
				break;
			}
			case "professionForget_fishing":
			{
				if (Game1.player.newLevels.Contains(new Point(1, 5)) || Game1.player.newLevels.Contains(new Point(1, 10)))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
					break;
				}
				Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
				RemoveProfession(8);
				RemoveProfession(11);
				RemoveProfession(10);
				RemoveProfession(6);
				RemoveProfession(9);
				RemoveProfession(7);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
				int num3 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[1]);
				if (num3 >= 5)
				{
					Game1.player.newLevels.Add(new Point(1, 5));
				}
				if (num3 >= 10)
				{
					Game1.player.newLevels.Add(new Point(1, 10));
				}
				DelayedAction.playSoundAfterDelay("dog_bark", 300);
				DelayedAction.playSoundAfterDelay("dog_bark", 900);
				break;
			}
			case "professionForget_combat":
			{
				if (Game1.player.newLevels.Contains(new Point(4, 5)) || Game1.player.newLevels.Contains(new Point(4, 10)))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
					break;
				}
				Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
				RemoveProfession(26);
				RemoveProfession(27);
				RemoveProfession(29);
				RemoveProfession(25);
				RemoveProfession(28);
				RemoveProfession(24);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
				int num = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[4]);
				if (num >= 5)
				{
					Game1.player.newLevels.Add(new Point(4, 5));
				}
				if (num >= 10)
				{
					Game1.player.newLevels.Add(new Point(4, 10));
				}
				DelayedAction.playSoundAfterDelay("dog_bark", 300);
				DelayedAction.playSoundAfterDelay("dog_bark", 900);
				break;
			}
			case "specialCharmQuestion_Yes":
				if (Game1.player.hasItemInInventory(446, 1))
				{
					Game1.player.holdUpItemThenMessage(new SpecialItem(3));
					Game1.player.removeFirstOfThisItemFromInventory(446);
					Game1.player.hasSpecialCharm = true;
					Game1.player.mailReceived.Add("SecretNote20_done");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_specialCharmNoFoot"));
				}
				break;
			case "evilShrineLeft_Yes":
				if (Game1.player.removeItemsFromInventory(74, 1))
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(156f, 388f), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						layerDepth = 0.0385000035f,
						scale = 4f
					});
					for (int j = 0; j < 20; j++)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.LightGray)
						{
							alpha = 0.75f,
							motion = new Vector2(1f, -0.5f),
							acceleration = new Vector2(-0.002f, 0f),
							interval = 99999f,
							layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
							scale = 3f,
							scaleChange = 0.01f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = j * 25
						});
					}
					playSound("fireball");
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(4f, -2f)
					});
					if (Game1.player.getChildrenCount() > 1)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(4f, -1.5f),
							delayBeforeAnimationStart = 50
						});
					}
					string message = "";
					foreach (Child i in Game1.player.getChildren())
					{
						message += Game1.content.LoadString("Strings\\Locations:WitchHut_Goodbye", i.getName());
					}
					Game1.showGlobalMessage(message);
					Game1.player.getRidOfChildren();
					Game1.multiplayer.globalChatInfoMessage("EvilShrine", Game1.player.name);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
				}
				break;
			case "evilShrineCenter_Yes":
				if (Game1.player.Money >= 30000)
				{
					Game1.player.Money -= 30000;
					Game1.player.wipeExMemories();
					Game1.multiplayer.globalChatInfoMessage("EvilShrine", Game1.player.name);
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(468f, 328f), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						layerDepth = 0.0385000035f,
						scale = 4f
					});
					playSound("fireball");
					DelayedAction.playSoundAfterDelay("debuffHit", 500, this);
					int count = 0;
					Game1.player.faceDirection(2);
					Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
					{
						new FarmerSprite.AnimationFrame(94, 1500),
						new FarmerSprite.AnimationFrame(0, 1)
					});
					Game1.player.freezePause = 1500;
					Game1.player.jitterStrength = 1f;
					for (int l = 0; l < 20; l++)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(7f, 5f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.SlateGray)
						{
							alpha = 0.75f,
							motion = new Vector2(0f, -0.5f),
							acceleration = new Vector2(-0.002f, 0f),
							interval = 99999f,
							layerDepth = 0.032f + (float)Game1.random.Next(100) / 10000f,
							scale = 3f,
							scaleChange = 0.01f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = l * 25
						});
					}
					for (int k = 0; k < 16; k++)
					{
						foreach (Vector2 v in Utility.getBorderOfThisRectangle(Utility.getRectangleCenteredAt(new Vector2(7f, 5f), 2 + k * 2)))
						{
							if (count % 2 == 0)
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(692, 1853, 4, 4), 25f, 1, 16, v * 64f + new Vector2(32f, 32f), flicker: false, flipped: false)
								{
									layerDepth = 1f,
									delayBeforeAnimationStart = k * 50,
									scale = 4f,
									scaleChange = 1f,
									color = new Color(255 - Utility.getRedToGreenLerpColor(1f / (float)(k + 1)).R, 255 - Utility.getRedToGreenLerpColor(1f / (float)(k + 1)).G, 255 - Utility.getRedToGreenLerpColor(1f / (float)(k + 1)).B),
									acceleration = new Vector2(-0.1f, 0f)
								});
							}
							count++;
						}
					}
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
				}
				break;
			case "evilShrineRightActivate_Yes":
				if (Game1.player.removeItemsFromInventory(203, 1))
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(780f, 388f), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						layerDepth = 0.0385000035f,
						scale = 4f
					});
					playSound("fireball");
					DelayedAction.playSoundAfterDelay("batScreech", 500, this);
					for (int n = 0; n < 20; n++)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(12f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.DarkSlateBlue)
						{
							alpha = 0.75f,
							motion = new Vector2(-0.1f, -0.5f),
							acceleration = new Vector2(-0.002f, 0f),
							interval = 99999f,
							layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
							scale = 3f,
							scaleChange = 0.01f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = n * 60
						});
					}
					Game1.player.freezePause = 1501;
					for (int m = 0; m < 28; m++)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 347, 13, 13), 50f, 4, 9999, new Vector2(12f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = 500 + m * 25,
							motion = new Vector2(Game1.random.Next(1, 5) * ((!(Game1.random.NextDouble() < 0.5)) ? 1 : (-1)), Game1.random.Next(1, 5) * ((!(Game1.random.NextDouble() < 0.5)) ? 1 : (-1)))
						});
					}
					Game1.spawnMonstersAtNight = true;
					Game1.multiplayer.globalChatInfoMessage("MonstersActivated", Game1.player.name);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
				}
				break;
			case "evilShrineRightDeActivate_Yes":
				if (Game1.player.removeItemsFromInventory(203, 1))
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(780f, 388f), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						layerDepth = 0.0385000035f,
						scale = 4f
					});
					playSound("fireball");
					for (int i2 = 0; i2 < 20; i2++)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(12f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.DarkSlateBlue)
						{
							alpha = 0.75f,
							motion = new Vector2(0f, -0.5f),
							acceleration = new Vector2(-0.002f, 0f),
							interval = 99999f,
							layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
							scale = 3f,
							scaleChange = 0.01f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = i2 * 25
						});
					}
					Game1.spawnMonstersAtNight = false;
					Game1.multiplayer.globalChatInfoMessage("MonstersDeActivated", Game1.player.name);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
				}
				break;
			case "buyJojaCola_Yes":
				if (Game1.player.Money >= 75)
				{
					Game1.player.Money -= 75;
					Game1.player.addItemByMenuIfNecessary(new Object(167, 1));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
				}
				break;
			case "WizardShrine_Yes":
				if (Game1.player.Money >= 500)
				{
					Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);
					Game1.player.Money -= 500;
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney2"));
				}
				break;
			case "Backpack_Purchase":
				if ((int)Game1.player.maxItems == 12 && Game1.player.Money >= 2000)
				{
					Game1.player.Money -= 2000;
					Game1.player.maxItems.Value += 12;
					for (int i4 = 0; i4 < (int)Game1.player.maxItems; i4++)
					{
						if (Game1.player.items.Count <= i4)
						{
							Game1.player.items.Add(null);
						}
					}
					Game1.player.holdUpItemThenMessage(new SpecialItem(99, Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708")));
					Game1.multiplayer.globalChatInfoMessage("BackpackLarge", Game1.player.Name);
				}
				else if ((int)Game1.player.maxItems < 36 && Game1.player.Money >= 10000)
				{
					Game1.player.Money -= 10000;
					Game1.player.maxItems.Value += 12;
					Game1.player.holdUpItemThenMessage(new SpecialItem(99, Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709")));
					for (int i3 = 0; i3 < (int)Game1.player.maxItems; i3++)
					{
						if (Game1.player.items.Count <= i3)
						{
							Game1.player.items.Add(null);
						}
					}
					Game1.multiplayer.globalChatInfoMessage("BackpackDeluxe", Game1.player.Name);
				}
				else if ((int)Game1.player.maxItems != 36)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney2"));
				}
				break;
			case "ClubSeller_I'll":
				if (Game1.player.Money >= 1000000)
				{
					Game1.player.Money -= 1000000;
					Game1.exitActiveMenu();
					Game1.player.forceCanMove();
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Object(Vector2.Zero, 127));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_NotEnoughMoney"));
				}
				break;
			case "BuyQiCoins_Yes":
				if (Game1.player.Money >= 1000)
				{
					Game1.player.Money -= 1000;
					localSound("Pickup_Coin15");
					Game1.player.clubCoins += 100;
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8715"));
				}
				break;
			case "Shaft_Jump":
				if (this is MineShaft)
				{
					(this as MineShaft).enterMineShaft();
				}
				break;
			case "mariner_Buy":
				if (Game1.player.Money >= 5000)
				{
					Game1.player.Money -= 5000;
					Game1.player.addItemByMenuIfNecessary(new Object(460, 1)
					{
						specialItem = true
					});
					if (Game1.activeClickableMenu == null)
					{
						Game1.player.holdUpItemThenMessage(new Object(460, 1));
					}
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
				}
				break;
			case "upgrade_Yes":
				houseUpgradeAccept();
				break;
			case "communityUpgrade_Yes":
				communityUpgradeAccept();
				break;
			case "adventureGuild_Shop":
				Game1.player.forceCanMove();
				Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureShopStock(), 0, "Marlon");
				break;
			case "adventureGuild_Recovery":
				Game1.player.forceCanMove();
				Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureRecoveryStock(), 0, "Marlon_Recovery");
				break;
			case "carpenter_Shop":
				Game1.player.forceCanMove();
				Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
				break;
			case "carpenter_Upgrade":
				houseUpgradeOffer();
				break;
			case "carpenter_Renovate":
				Game1.player.forceCanMove();
				HouseRenovation.ShowRenovationMenu();
				break;
			case "carpenter_CommunityUpgrade":
				communityUpgradeOffer();
				break;
			case "carpenter_Construct":
				Game1.activeClickableMenu = new CarpenterMenu();
				break;
			case "Eat_Yes":
				Game1.player.isEating = false;
				Game1.player.eatHeldObject();
				break;
			case "Eat_No":
				Game1.player.isEating = false;
				Game1.player.completelyStopAnimatingOrDoingAction();
				break;
			case "Marnie_Supplies":
				Game1.activeClickableMenu = new ShopMenu(Utility.getAnimalShopStock(), 0, "Marnie");
				break;
			case "Marnie_Purchase":
				Game1.player.forceCanMove();
				Game1.activeClickableMenu = new PurchaseAnimalsMenu(Utility.getPurchaseAnimalStock());
				break;
			case "Blacksmith_Shop":
				Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithStock(), 0, "Clint");
				break;
			case "Blacksmith_Upgrade":
				Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithUpgradeStock(Game1.player), 0, "ClintUpgrade");
				break;
			case "Blacksmith_Process":
				Game1.activeClickableMenu = new GeodeMenu();
				break;
			case "Dungeon_Go":
				Game1.enterMine(Game1.CurrentMineLevel + 1);
				break;
			case "Mine_Return":
				Game1.enterMine(Game1.player.deepestMineLevel);
				break;
			case "Mine_Enter":
				Game1.enterMine(1);
				break;
			case "Sleep_Yes":
				startSleep();
				break;
			case "Mine_Yes":
				if (Game1.CurrentMineLevel > 120)
				{
					Game1.warpFarmer("SkullCave", 3, 4, 2);
				}
				else
				{
					Game1.warpFarmer("UndergroundMine", 16, 16, flip: false);
				}
				break;
			case "Mine_No":
			{
				Response[] noYesResponses = new Response[2]
				{
					new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")),
					new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"))
				};
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_ResetMine")), noYesResponses, "ResetMine");
				break;
			}
			case "ExitMine_Yes":
			case "ExitMine_Leave":
				if (Game1.CurrentMineLevel == 77377)
				{
					Game1.warpFarmer("Mine", 67, 10, flip: true);
				}
				else if (Game1.CurrentMineLevel > 120)
				{
					Game1.warpFarmer("SkullCave", 3, 4, 2);
				}
				else
				{
					Game1.warpFarmer("Mine", 23, 8, flip: false);
				}
				Game1.changeMusicTrack("none");
				break;
			case "ExitMine_Go":
				Game1.enterMine(Game1.CurrentMineLevel - 1);
				break;
			case "Minecart_Mines":
				Game1.player.Halt();
				Game1.player.freezePause = 700;
				Game1.warpFarmer("Mine", 13, 9, 1);
				if (Game1.getMusicTrackName() == "springtown")
				{
					Game1.changeMusicTrack("none");
				}
				break;
			case "Minecart_Town":
				Game1.player.Halt();
				Game1.player.freezePause = 700;
				Game1.warpFarmer("Town", 105, 80, 1);
				break;
			case "Minecart_Quarry":
				Game1.player.Halt();
				Game1.player.freezePause = 700;
				Game1.warpFarmer("Mountain", 124, 12, 2);
				break;
			case "Minecart_Bus":
				Game1.player.Halt();
				Game1.player.freezePause = 700;
				Game1.warpFarmer("BusStop", 4, 4, 2);
				break;
			case "Backpack_Yes":
				tryToBuyNewBackpack();
				break;
			case "Quest_Yes":
				Game1.questOfTheDay.dailyQuest.Value = true;
				Game1.questOfTheDay.accept();
				Game1.currentBillboard = 0;
				Game1.player.questLog.Add(Game1.questOfTheDay);
				break;
			case "Quest_No":
				Game1.currentBillboard = 0;
				break;
			case "MinecartGame_Endless":
				Game1.currentMinigame = new MineCart(0, 2);
				break;
			case "MinecartGame_Progress":
				Game1.currentMinigame = new MineCart(0, 3);
				break;
			case "CowboyGame_NewGame":
				Game1.player.jotpkProgress.Value = null;
				Game1.currentMinigame = new AbigailGame();
				break;
			case "CowboyGame_Continue":
				Game1.currentMinigame = new AbigailGame();
				break;
			case "Smelt_Copper":
				Game1.player.CopperPieces -= 10;
				smeltBar(new Object(Vector2.Zero, 334, "Copper Bar", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), 60);
				break;
			case "Smelt_Iron":
				Game1.player.IronPieces -= 10;
				smeltBar(new Object(Vector2.Zero, 335, "Iron Bar", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), 120);
				break;
			case "Smelt_Gold":
				Game1.player.GoldPieces -= 10;
				smeltBar(new Object(Vector2.Zero, 336, "Gold Bar", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), 300);
				break;
			case "Smelt_Iridium":
				Game1.player.IridiumPieces -= 10;
				smeltBar(new Object(Vector2.Zero, 337, "Iridium Bar", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), 1440);
				break;
			case "jukebox_Yes":
				Game1.drawItemNumberSelection("jukebox", -1);
				Game1.jukeboxPlaying = true;
				break;
			case "RemoveIncubatingEgg_Yes":
				Game1.player.ActiveObject = new Object(Vector2.Zero, (this as AnimalHouse).incubatingEgg.Y, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
				Game1.player.showCarrying();
				(this as AnimalHouse).incubatingEgg.Y = -1;
				map.GetLayer("Front").Tiles[1, 2].TileIndex = 45;
				break;
			case "ClubCard_Yes.":
			case "ClubCard_That's":
				playSound("explosion");
				Game1.flashAlpha = 5f;
				characters.Remove(getCharacterFromName("Bouncer"));
				if (characters.Count > 0)
				{
					characters[0].faceDirection(1);
					characters[0].setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Sandy_PlayerClubMember"));
					characters[0].doEmote(16);
				}
				Game1.pauseThenMessage(500, Game1.content.LoadString("Strings\\Locations:Club_Bouncer_PlayerClubMember"), showProgressBar: false);
				Game1.player.Halt();
				Game1.getCharacterFromName("Mister Qi").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:MisterQi_PlayerClubMember"));
				break;
			case "CalicoJack_Rules":
				Game1.multipleDialogues(new string[2]
				{
					Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules1"),
					Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules2")
				});
				break;
			case "CalicoJackHS_Play":
				if (Game1.player.clubCoins >= 1000)
				{
					Game1.currentMinigame = new CalicoJack(-1, highStakes: true);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJackHS_NotEnoughCoins"));
				}
				break;
			case "CalicoJack_Play":
				if (Game1.player.clubCoins >= 100)
				{
					Game1.currentMinigame = new CalicoJack();
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_NotEnoughCoins"));
				}
				break;
			case "BuyClubCoins_Yes":
				if (Game1.player.Money >= 1000)
				{
					Game1.player.Money -= 1000;
					Game1.player.clubCoins += 10;
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
				}
				break;
			case "Bouquet_Yes":
				if (Game1.player.Money >= 500)
				{
					if (Game1.player.ActiveObject == null)
					{
						Game1.player.Money -= 500;
						Game1.player.grabObject(new Object(Vector2.Zero, 458, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false));
						return true;
					}
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
				}
				break;
			case "Mariner_Buy":
				if (Game1.player.Money >= 5000)
				{
					Game1.player.Money -= 5000;
					Game1.player.grabObject(new Object(Vector2.Zero, 460, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false));
					return true;
				}
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
				break;
			case "Flute_Change":
				Game1.drawItemNumberSelection("flutePitch", -1);
				break;
			case "Drum_Change":
				Game1.drawItemNumberSelection("drumTone", -1);
				break;
			case "ClearHouse_Yes":
			{
				Vector2 playerPos = Game1.player.getTileLocation();
				Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
				foreach (Vector2 offset in adjacentTilesOffsets)
				{
					Vector2 v2 = playerPos + offset;
					if (objects.ContainsKey(v2))
					{
						objects.Remove(v2);
					}
				}
				break;
			}
			case "ExitToTitle_Yes":
				Game1.fadeScreenToBlack();
				Game1.exitToTitle = true;
				break;
			case "taxvote_For":
				Game1.shippingTax = true;
				Game1.addMailForTomorrow("taxPassed");
				currentEvent.currentCommand++;
				break;
			case "taxvote_Against":
				Game1.addMailForTomorrow("taxRejected");
				currentEvent.currentCommand++;
				break;
			case "telephone_Saloon":
				playShopPhoneNumberSounds(questionAndAnswer);
				Game1.player.freezePause = 4950;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.playSound("bigSelect");
					NPC characterFromName2 = Game1.getCharacterFromName("Gus");
					if (AreStoresClosedForFestival())
					{
						Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					else if (Game1.timeOfDay >= 1200 && Game1.timeOfDay < 2400 && (characterFromName2.dayScheduleName.Value != "fall_4" || Game1.timeOfDay >= 1700))
					{
						if (Game1.dishOfTheDay != null)
						{
							Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""), Game1.dishOfTheDay.DisplayName));
						}
						else
						{
							Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Open_NoDishOfTheDay"));
						}
					}
					else if (Game1.dishOfTheDay != null && Game1.timeOfDay < 2400)
					{
						Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Closed", Game1.dishOfTheDay.DisplayName), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					else
					{
						Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Closed_NoDishOfTheDay"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					answerDialogueAction("HangUp", new string[0]);
				}, 4950);
				break;
			case "telephone_SeedShop":
				playShopPhoneNumberSounds(questionAndAnswer);
				Game1.player.freezePause = 4950;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.playSound("bigSelect");
					NPC characterFromName3 = Game1.getCharacterFromName("Pierre");
					string a = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
					if (AreStoresClosedForFestival())
					{
						Game1.drawDialogue(characterFromName3, Game1.content.LoadString("Strings\\Characters:Phone_Pierre_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					else if ((Game1.isLocationAccessible("CommunityCenter") || a != "Wed") && Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
					{
						Game1.drawDialogue(characterFromName3, Game1.content.LoadString("Strings\\Characters:Phone_Pierre_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : "")));
					}
					else
					{
						Game1.drawDialogue(characterFromName3, Game1.content.LoadString("Strings\\Characters:Phone_Pierre_Closed"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
					{
						List<Response> list = new List<Response>
						{
							new Response("SeedShop_CheckSeedStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock")),
							new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
						};
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), list.ToArray(), "telephone");
					});
				}, 4950);
				break;
			case "telephone_AnimalShop":
				playShopPhoneNumberSounds(questionAndAnswer);
				Game1.player.freezePause = 4950;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.playSound("bigSelect");
					NPC characterFromName4 = Game1.getCharacterFromName("Marnie");
					Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
					if (AreStoresClosedForFestival())
					{
						Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Marnie_ClosedDay"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					else if (characterFromName4.dayScheduleName.Value == "fall_18" || characterFromName4.dayScheduleName.Value == "winter_18" || characterFromName4.dayScheduleName.Value == "Tue" || characterFromName4.dayScheduleName.Value == "Mon")
					{
						Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Marnie_ClosedDay"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
					{
						Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Marnie_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : "")));
					}
					else
					{
						Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Marnie_Closed"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
					{
						List<Response> list2 = new List<Response>
						{
							new Response("AnimalShop_CheckAnimalPrices", Game1.content.LoadString("Strings\\Characters:Phone_CheckAnimalPrices")),
							new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
						};
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), list2.ToArray(), "telephone");
					});
				}, 4950);
				break;
			case "telephone_Blacksmith":
				playShopPhoneNumberSounds(questionAndAnswer);
				Game1.player.freezePause = 4950;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.playSound("bigSelect");
					NPC characterFromName5 = Game1.getCharacterFromName("Clint");
					if (AreStoresClosedForFestival())
					{
						Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					else if (Game1.player.daysLeftForToolUpgrade.Value > 0)
					{
						int value = Game1.player.daysLeftForToolUpgrade.Value;
						if (value == 1)
						{
							Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Working_OneDay"));
						}
						else
						{
							Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Working", value));
						}
					}
					else
					{
						string value2 = characterFromName5.dayScheduleName.Value;
						if (!(value2 == "winter_16"))
						{
							if (value2 == "Fri")
							{
								Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
							}
							else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
							{
								Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : "")));
							}
							else
							{
								Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Closed"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
							}
						}
						else
						{
							Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
						}
					}
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
					{
						List<Response> list3 = new List<Response>
						{
							new Response("Blacksmith_UpgradeCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckToolCost")),
							new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
						};
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), list3.ToArray(), "telephone");
					});
				}, 4950);
				break;
			case "telephone_AdventureGuild":
				playShopPhoneNumberSounds(questionAndAnswer);
				Game1.player.freezePause = 4950;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.playSound("bigSelect");
					NPC character = Game1.getCharacterFromName("Marlon");
					Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
					if (Game1.player.mailForTomorrow.Contains("MarlonRecovery"))
					{
						Game1.drawDialogue(character, Game1.content.LoadString("Strings\\Characters:Phone_Marlon_AlreadyRecovering"));
					}
					else
					{
						Game1.drawDialogue(character, Game1.content.LoadString("Strings\\Characters:Phone_Marlon_Open"));
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							if (Game1.player.itemsLostLastDeath.Count > 0)
							{
								Game1.player.forceCanMove();
								Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureRecoveryStock(), 0, "Marlon_Recovery");
							}
							else
							{
								Game1.drawDialogue(character, Game1.content.LoadString("Strings\\Characters:Phone_Marlon_NoDeathItems"));
							}
						});
					}
				}, 4950);
				break;
			case "telephone_Carpenter":
				playShopPhoneNumberSounds(questionAndAnswer);
				Game1.player.freezePause = 4950;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.playSound("bigSelect");
					NPC characterFromName6 = Game1.getCharacterFromName("Robin");
					Town town;
					if (AreStoresClosedForFestival())
					{
						Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
					}
					else if ((town = (Game1.getLocationFromName("Town") as Town)) != null && town.daysUntilCommunityUpgrade.Value > 0)
					{
						int value3 = town.daysUntilCommunityUpgrade.Value;
						if (value3 == 1)
						{
							Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Working_OneDay"));
						}
						else
						{
							Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Working", value3));
						}
					}
					else if (Game1.getFarm().isThereABuildingUnderConstruction())
					{
						Building buildingUnderConstruction = Game1.getFarm().getBuildingUnderConstruction();
						int num7 = 0;
						if (buildingUnderConstruction != null)
						{
							num7 = buildingUnderConstruction.daysUntilUpgrade;
						}
						if (num7 == 1)
						{
							Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Working_OneDay"));
						}
						else
						{
							Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Working", num7));
						}
					}
					else
					{
						string value4 = characterFromName6.dayScheduleName.Value;
						if (!(value4 == "summer_18"))
						{
							if (value4 == "Tue")
							{
								Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Workout"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
							}
							else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
							{
								Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : "")));
							}
							else
							{
								Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Closed"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
							}
						}
						else
						{
							Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
						}
					}
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
					{
						List<Response> list4 = new List<Response>
						{
							new Response("Carpenter_ShopStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock"))
						};
						if ((int)Game1.player.houseUpgradeLevel < 3)
						{
							list4.Add(new Response("Carpenter_HouseCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckHouseCost")));
						}
						list4.Add(new Response("Carpenter_BuildingCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckBuildingCost")));
						list4.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp")));
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), list4.ToArray(), "telephone");
					});
				}, 4950);
				break;
			case "telephone_HangUp":
				playSound("openBox");
				break;
			case "telephone_Carpenter_HouseCost":
			{
				NPC characterFromName = Game1.getCharacterFromName("Robin");
				string upgrade_text = Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse" + ((int)Game1.player.houseUpgradeLevel + 1));
				if (upgrade_text.Contains('.'))
				{
					upgrade_text = upgrade_text.Substring(0, upgrade_text.LastIndexOf('.') + 1);
				}
				else if (upgrade_text.Contains('。'))
				{
					upgrade_text = upgrade_text.Substring(0, upgrade_text.LastIndexOf('。') + 1);
				}
				Game1.drawDialogue(characterFromName, upgrade_text, Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					answerDialogueAction("HangUp", new string[0]);
				});
				break;
			}
			case "telephone_Carpenter_BuildingCost":
			{
				answerDialogueAction("carpenter_Construct", new string[0]);
				CarpenterMenu menu;
				if ((menu = (Game1.activeClickableMenu as CarpenterMenu)) != null)
				{
					menu.readOnly = true;
					menu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(menu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
					{
						answerDialogueAction("HangUp", new string[0]);
					});
				}
				break;
			}
			case "telephone_Carpenter_ShopStock":
				Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock());
				if (Game1.activeClickableMenu is ShopMenu)
				{
					ShopMenu obj = Game1.activeClickableMenu as ShopMenu;
					obj.readOnly = true;
					obj.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(obj.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
					{
						answerDialogueAction("HangUp", new string[0]);
					});
				}
				break;
			case "telephone_Blacksmith_UpgradeCost":
			{
				answerDialogueAction("Blacksmith_Upgrade", new string[0]);
				ShopMenu menu2;
				if ((menu2 = (Game1.activeClickableMenu as ShopMenu)) != null)
				{
					menu2.readOnly = true;
					menu2.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(menu2.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
					{
						answerDialogueAction("HangUp", new string[0]);
					});
				}
				break;
			}
			case "telephone_SeedShop_CheckSeedStock":
			{
				SeedShop seed_shop;
				if ((seed_shop = (Game1.getLocationFromName("SeedShop") as SeedShop)) != null)
				{
					Game1.activeClickableMenu = new ShopMenu(seed_shop.shopStock(), 0, "Pierre");
					ShopMenu menu3;
					if ((menu3 = (Game1.activeClickableMenu as ShopMenu)) != null)
					{
						menu3.readOnly = true;
						menu3.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(menu3.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
						{
							answerDialogueAction("HangUp", new string[0]);
						});
					}
				}
				else
				{
					answerDialogueAction("HangUp", new string[0]);
				}
				break;
			}
			case "telephone_AnimalShop_CheckAnimalPrices":
			{
				Game1.activeClickableMenu = new PurchaseAnimalsMenu(Utility.getPurchaseAnimalStock());
				PurchaseAnimalsMenu menu4;
				if ((menu4 = (Game1.activeClickableMenu as PurchaseAnimalsMenu)) != null)
				{
					menu4.readOnly = true;
					menu4.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(menu4.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
					{
						answerDialogueAction("HangUp", new string[0]);
					});
				}
				break;
			}
			case "ShrineOfChallenge_Yes":
				Game1.player.team.toggleMineShrineOvernight.Value = true;
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_Activated"));
				Game1.multiplayer.globalChatInfoMessage((!Game1.player.team.mineShrineActivated.Value) ? "HardModeMinesActivated" : "HardModeMinesDeactivated", Game1.player.Name);
				DelayedAction.functionAfterDelay(delegate
				{
					if (!Game1.player.team.mineShrineActivated.Value)
					{
						Game1.playSound("fireball");
						temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(8.75f, 5.8f) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
						{
							interval = 50f,
							totalNumberOfLoops = 99999,
							animationLength = 4,
							light = true,
							lightID = 888,
							id = 888f,
							lightRadius = 2f,
							scale = 4f,
							yPeriodic = true,
							lightcolor = new Color(100, 0, 0),
							yPeriodicLoopTime = 1000f,
							yPeriodicRange = 4f,
							layerDepth = 0.04544f
						});
						temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(10.75f, 5.8f) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
						{
							interval = 50f,
							totalNumberOfLoops = 99999,
							animationLength = 4,
							light = true,
							lightID = 889,
							id = 889f,
							lightRadius = 2f,
							scale = 4f,
							lightcolor = new Color(100, 0, 0),
							yPeriodic = true,
							yPeriodicLoopTime = 1100f,
							yPeriodicRange = 4f,
							layerDepth = 0.04544f
						});
					}
					else
					{
						removeTemporarySpritesWithID(888);
						removeTemporarySpritesWithID(889);
						Game1.playSound("fireball");
					}
				}, 500);
				break;
			default:
				return false;
			}
			return true;
		}

		private void playShopPhoneNumberSounds(string whichShop)
		{
			Random r = new Random(whichShop.GetHashCode());
			DelayedAction.playSoundAfterDelay("telephone_dialtone", 495, null, 1200);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1200, null, 1200 + r.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1370, null, 1200 + r.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1600, null, 1200 + r.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1850, null, 1200 + r.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2030, null, 1200 + r.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2250, null, 1200 + r.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2410, null, 1200 + r.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_ringingInEar", 3150);
		}

		public virtual bool answerDialogue(Response answer)
		{
			string[] questionParams = (lastQuestionKey != null) ? lastQuestionKey.Split(' ') : null;
			string questionAndAnswer = (questionParams != null) ? (questionParams[0] + "_" + answer.responseKey) : null;
			if (answer.responseKey.Equals("Move"))
			{
				Game1.player.grabObject(actionObjectForQuestionDialogue);
				removeObject(actionObjectForQuestionDialogue.TileLocation, showDestroyedObject: false);
				actionObjectForQuestionDialogue = null;
				return true;
			}
			if (afterQuestion != null)
			{
				afterQuestion(Game1.player, answer.responseKey);
				afterQuestion = null;
				Game1.objectDialoguePortraitPerson = null;
				return true;
			}
			if (questionAndAnswer == null)
			{
				return false;
			}
			return answerDialogueAction(questionAndAnswer, questionParams);
		}

		public static bool AreStoresClosedForFestival()
		{
			if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
			{
				return Utility.getStartTimeOfFestival() < 1900;
			}
			return false;
		}

		public static void RemoveProfession(int profession)
		{
			if (Game1.player.professions.Contains(profession))
			{
				LevelUpMenu.removeImmediateProfessionPerk(profession);
				Game1.player.professions.Remove(profession);
			}
		}

		public static bool canRespec(int skill_index)
		{
			if (Game1.player.GetUnmodifiedSkillLevel(skill_index) < 5)
			{
				return false;
			}
			if (Game1.player.newLevels.Contains(new Point(skill_index, 5)) || Game1.player.newLevels.Contains(new Point(skill_index, 10)))
			{
				return false;
			}
			return true;
		}

		public void setObject(Vector2 v, Object o)
		{
			if (objects.ContainsKey(v))
			{
				objects[v] = o;
			}
			else
			{
				objects.Add(v, o);
			}
		}

		private void houseUpgradeOffer()
		{
			switch ((int)Game1.player.houseUpgradeLevel)
			{
			case 0:
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse1")), createYesNoResponses(), "upgrade");
				break;
			case 1:
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse2")), createYesNoResponses(), "upgrade");
				break;
			case 2:
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse3")), createYesNoResponses(), "upgrade");
				break;
			}
		}

		private void communityUpgradeOffer()
		{
			if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_CommunityUpgrade1")), createYesNoResponses(), "communityUpgrade");
				if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgradeAsked"))
				{
					Game1.MasterPlayer.mailReceived.Add("pamHouseUpgradeAsked");
				}
			}
			else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_CommunityUpgrade2")), createYesNoResponses(), "communityUpgrade");
			}
		}

		public virtual bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			return false;
		}

		public virtual float getExtraTrashChanceForCrabPot(int x, int y)
		{
			return 0f;
		}

		private void communityUpgradeAccept()
		{
			if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				if (Game1.player.Money >= 500000 && Game1.player.hasItemInInventory(388, 950))
				{
					Game1.player.Money -= 500000;
					Game1.player.removeItemsFromInventory(388, 950);
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_PamUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					(Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value = 3;
					Game1.multiplayer.globalChatInfoMessage("CommunityUpgrade", Game1.player.Name);
				}
				else if (Game1.player.Money < 500000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood3"));
				}
			}
			else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				if (Game1.player.Money >= 300000)
				{
					Game1.player.Money -= 300000;
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					(Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value = 3;
					Game1.multiplayer.globalChatInfoMessage("CommunityUpgrade", Game1.player.Name);
				}
				else if (Game1.player.Money < 300000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
			}
		}

		private void houseUpgradeAccept()
		{
			switch ((int)Game1.player.houseUpgradeLevel)
			{
			case 0:
				if (Game1.player.Money >= 10000 && Game1.player.hasItemInInventory(388, 450))
				{
					Game1.player.daysUntilHouseUpgrade.Value = 3;
					Game1.player.Money -= 10000;
					Game1.player.removeItemsFromInventory(388, 450);
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getPossessivePronoun(Game1.player.isMale));
				}
				else if (Game1.player.Money < 10000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood1"));
				}
				break;
			case 1:
				if (Game1.player.Money >= 50000 && Game1.player.hasItemInInventory(709, 150))
				{
					Game1.player.daysUntilHouseUpgrade.Value = 3;
					Game1.player.Money -= 50000;
					Game1.player.removeItemsFromInventory(709, 150);
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getPossessivePronoun(Game1.player.isMale));
				}
				else if (Game1.player.Money < 50000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood2"));
				}
				break;
			case 2:
				if (Game1.player.Money >= 100000)
				{
					Game1.player.daysUntilHouseUpgrade.Value = 3;
					Game1.player.Money -= 100000;
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getPossessivePronoun(Game1.player.isMale));
				}
				else if (Game1.player.Money < 100000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
				break;
			}
		}

		private void smeltBar(Object bar, int minutesUntilReady)
		{
			Game1.player.CoalPieces--;
			actionObjectForQuestionDialogue.heldObject.Value = bar;
			actionObjectForQuestionDialogue.minutesUntilReady.Value = minutesUntilReady;
			actionObjectForQuestionDialogue.showNextIndex.Value = true;
			actionObjectForQuestionDialogue = null;
			playSound("openBox");
			playSound("furnace");
			Game1.stats.BarsSmelted++;
		}

		public void tryToBuyNewBackpack()
		{
			int cost = 0;
			switch (Game1.player.MaxItems)
			{
			case 4:
				cost = 3500;
				break;
			case 9:
				cost = 7500;
				break;
			case 14:
				cost = 15000;
				break;
			}
			if (Game1.player.Money >= cost)
			{
				Game1.player.increaseBackpackSize(5);
				Game1.player.Money -= cost;
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:Backpack_Bought", Game1.player.MaxItems));
				checkForMapChanges();
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			}
		}

		public void checkForMapChanges()
		{
			if (name.Equals("SeedShop") && Game1.player.MaxItems == 19)
			{
				map.GetLayer("Front").Tiles[10, 21] = new StaticTile(map.GetLayer("Front"), map.GetTileSheet("TownHouseIndoors"), BlendMode.Alpha, 203);
			}
		}

		public void removeStumpOrBoulder(int tileX, int tileY, Object o)
		{
			List<Vector2> boulderBatch = new List<Vector2>();
			switch (o.Name)
			{
			case "Stump1/4":
			case "Boulder1/4":
				boulderBatch.Add(new Vector2(tileX, tileY));
				boulderBatch.Add(new Vector2(tileX + 1, tileY));
				boulderBatch.Add(new Vector2(tileX, tileY + 1));
				boulderBatch.Add(new Vector2(tileX + 1, tileY + 1));
				break;
			case "Stump2/4":
			case "Boulder2/4":
				boulderBatch.Add(new Vector2(tileX, tileY));
				boulderBatch.Add(new Vector2(tileX - 1, tileY));
				boulderBatch.Add(new Vector2(tileX, tileY + 1));
				boulderBatch.Add(new Vector2(tileX - 1, tileY + 1));
				break;
			case "Stump3/4":
			case "Boulder3/4":
				boulderBatch.Add(new Vector2(tileX, tileY));
				boulderBatch.Add(new Vector2(tileX + 1, tileY));
				boulderBatch.Add(new Vector2(tileX, tileY - 1));
				boulderBatch.Add(new Vector2(tileX + 1, tileY - 1));
				break;
			case "Stump4/4":
			case "Boulder4/4":
				boulderBatch.Add(new Vector2(tileX, tileY));
				boulderBatch.Add(new Vector2(tileX - 1, tileY));
				boulderBatch.Add(new Vector2(tileX, tileY - 1));
				boulderBatch.Add(new Vector2(tileX - 1, tileY - 1));
				break;
			}
			int whichDebris = o.Name.Contains("Stump") ? 5 : 3;
			if (Game1.currentSeason.Equals("winter"))
			{
				whichDebris += 376;
			}
			for (int i = 0; i < boulderBatch.Count; i++)
			{
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(whichDebris, Game1.random.Next(150, 400), 1, 3, new Vector2(boulderBatch[i].X * 64f, boulderBatch[i].Y * 64f), flicker: true, o.flipped));
			}
			removeBatch(boulderBatch);
		}

		public void destroyObject(Vector2 tileLocation, Farmer who)
		{
			destroyObject(tileLocation, hardDestroy: false, who);
		}

		public void destroyObject(Vector2 tileLocation, bool hardDestroy, Farmer who)
		{
			if (objects.ContainsKey(tileLocation) && !objects[tileLocation].IsHoeDirt && (int)objects[tileLocation].fragility != 2 && !(objects[tileLocation] is Chest) && (int)objects[tileLocation].parentSheetIndex != 165)
			{
				Object obj = objects[tileLocation];
				bool remove = false;
				if (obj.Type != null && (obj.Type.Equals("Fish") || obj.Type.Equals("Cooking") || obj.Type.Equals("Crafting")))
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(obj.ParentSheetIndex, 150f, 1, 3, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), flicker: true, obj.bigCraftable, obj.flipped));
					remove = true;
				}
				else if (obj.Name.Contains("Stump") || obj.Name.Contains("Boulder"))
				{
					remove = true;
					removeStumpOrBoulder((int)tileLocation.X, (int)tileLocation.Y, obj);
				}
				else if (obj.CanBeGrabbed | hardDestroy)
				{
					remove = true;
				}
				if (this is MineShaft && !obj.Name.Contains("Lumber"))
				{
					remove = true;
				}
				if (obj.Name.Contains("Stone") && !obj.bigCraftable && !(obj is Fence))
				{
					remove = true;
					OnStoneDestroyed(obj.parentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, who);
				}
				if (remove)
				{
					objects.Remove(tileLocation);
				}
			}
		}

		public LocationContext GetLocationContext()
		{
			if (locationContext == (LocationContext)(-1))
			{
				if (map == null)
				{
					reloadMap();
				}
				locationContext = LocationContext.Default;
				string location_context2 = null;
				PropertyValue value = null;
				if (map == null)
				{
					return LocationContext.Default;
				}
				location_context2 = ((!map.Properties.TryGetValue("LocationContext", out value)) ? "" : value.ToString());
				if (location_context2 != "" && !Enum.TryParse(location_context2, out locationContext))
				{
					locationContext = LocationContext.Default;
				}
			}
			return locationContext;
		}

		public virtual bool sinkDebris(Debris debris, Vector2 chunkTile, Vector2 chunkPosition)
		{
			if (debris.isEssentialItem())
			{
				return false;
			}
			if ((Debris.DebrisType)debris.debrisType == Debris.DebrisType.OBJECT && (int)debris.chunkType == 74)
			{
				return false;
			}
			if ((Debris.DebrisType)debris.debrisType == Debris.DebrisType.CHUNKS)
			{
				localSound("quickSlosh");
				TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 3, 0, chunkPosition, flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.02f, Color.White, 0.75f, 0.003f, 0f, 0f));
			}
			else
			{
				TemporarySprites.Add(new TemporaryAnimatedSprite(28, 300f, 2, 1, chunkPosition, flicker: false, flipped: false));
				localSound("dropItemInWater");
			}
			return true;
		}

		public virtual bool doesTileSinkDebris(int xTile, int yTile, Debris.DebrisType type)
		{
			if (type == Debris.DebrisType.CHUNKS)
			{
				if (doesTileHaveProperty(xTile, yTile, "Water", "Back") != null)
				{
					return getTileIndexAt(xTile, yTile, "Buildings") == -1;
				}
				return false;
			}
			if (doesTileHaveProperty(xTile, yTile, "Water", "Back") != null && !isTileUpperWaterBorder(getTileIndexAt(xTile, yTile, "Buildings")))
			{
				return doesTileHaveProperty(xTile, yTile, "Passable", "Buildings") == null;
			}
			return false;
		}

		private bool isTileUpperWaterBorder(int index)
		{
			switch (index)
			{
			case 183:
			case 184:
			case 185:
			case 211:
			case 1182:
			case 1183:
			case 1184:
			case 1210:
				return true;
			default:
				return false;
			}
		}

		public virtual bool doesEitherTileOrTileIndexPropertyEqual(int xTile, int yTile, string propertyName, string layerName, string propertyValue)
		{
			PropertyValue property = null;
			if (map != null && map.GetLayer(layerName) != null)
			{
				Tile tmp = map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
				if (tmp != null && tmp.TileIndexProperties.TryGetValue(propertyName, out property) && property.ToString() == propertyValue)
				{
					return true;
				}
				if (tmp != null && map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size).Properties.TryGetValue(propertyName, out property) && property.ToString() == propertyValue)
				{
					return true;
				}
			}
			return propertyValue == null;
		}

		public virtual string doesTileHaveProperty(int xTile, int yTile, string propertyName, string layerName)
		{
			foreach (Furniture f in furniture)
			{
				if ((float)xTile >= f.tileLocation.X - (float)f.GetAdditionalTilePropertyRadius() && (float)xTile < f.tileLocation.X + (float)f.getTilesWide() + (float)f.GetAdditionalTilePropertyRadius() && (float)yTile >= f.tileLocation.Y - (float)f.GetAdditionalTilePropertyRadius() && (float)yTile < f.tileLocation.Y + (float)f.getTilesHigh() + (float)f.GetAdditionalTilePropertyRadius())
				{
					string tile_property = null;
					if (f.DoesTileHaveProperty(xTile, yTile, propertyName, layerName, ref tile_property))
					{
						return tile_property;
					}
				}
			}
			PropertyValue property = null;
			if (map != null && map.GetLayer(layerName) != null)
			{
				Tile tmp = map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
				tmp?.TileIndexProperties.TryGetValue(propertyName, out property);
				if (property == null && tmp != null)
				{
					map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size).Properties.TryGetValue(propertyName, out property);
				}
			}
			return property?.ToString();
		}

		public virtual string doesTileHavePropertyNoNull(int xTile, int yTile, string propertyName, string layerName)
		{
			foreach (Furniture f in furniture)
			{
				if ((float)xTile >= f.tileLocation.X && (float)xTile < f.tileLocation.X + (float)f.getTilesWide() && (float)yTile >= f.tileLocation.Y && (float)yTile < f.tileLocation.Y + (float)f.getTilesHigh())
				{
					string tile_property = null;
					if (f.DoesTileHaveProperty(xTile, yTile, propertyName, layerName, ref tile_property))
					{
						if (tile_property == null)
						{
							return "";
						}
						return tile_property;
					}
				}
			}
			PropertyValue property = null;
			PropertyValue propertyTile = null;
			if (map != null && map.GetLayer(layerName) != null)
			{
				Tile tmp = map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
				tmp?.TileIndexProperties.TryGetValue(propertyName, out property);
				if (tmp != null)
				{
					map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size).Properties.TryGetValue(propertyName, out propertyTile);
				}
				if (propertyTile != null)
				{
					property = propertyTile;
				}
			}
			if (property != null)
			{
				return property.ToString();
			}
			return "";
		}

		public bool isWaterTile(int xTile, int yTile)
		{
			return doesTileHaveProperty(xTile, yTile, "Water", "Back") != null;
		}

		public bool isOpenWater(int xTile, int yTile)
		{
			if (isWaterTile(xTile, yTile) && getTileIndexAt(xTile, yTile, "Buildings") == -1)
			{
				return !objects.ContainsKey(new Vector2(xTile, yTile));
			}
			return false;
		}

		public bool isCropAtTile(int tileX, int tileY)
		{
			Vector2 v = new Vector2(tileX, tileY);
			if (terrainFeatures.ContainsKey(v) && terrainFeatures[v] is HoeDirt)
			{
				return ((HoeDirt)terrainFeatures[v]).crop != null;
			}
			return false;
		}

		public virtual bool dropObject(Object obj, Vector2 dropLocation, xTile.Dimensions.Rectangle viewport, bool initialPlacement, Farmer who = null)
		{
			obj.isSpawnedObject.Value = true;
			Vector2 tileLocation = new Vector2((int)dropLocation.X / 64, (int)dropLocation.Y / 64);
			if (!isTileOnMap(tileLocation) || map.GetLayer("Back").PickTile(new Location((int)dropLocation.X, (int)dropLocation.Y), Game1.viewport.Size) == null || map.GetLayer("Back").Tiles[(int)tileLocation.X, (int)tileLocation.Y].TileIndexProperties.ContainsKey("Unplaceable"))
			{
				return false;
			}
			if ((bool)obj.bigCraftable)
			{
				obj.tileLocation.Value = tileLocation;
				if (!isFarm)
				{
					return false;
				}
				if (!obj.setOutdoors && (bool)isOutdoors)
				{
					return false;
				}
				if (!obj.setIndoors && !isOutdoors)
				{
					return false;
				}
				if (obj.performDropDownAction(who))
				{
					return false;
				}
			}
			else if (obj.Type != null && obj.Type.Equals("Crafting") && obj.performDropDownAction(who))
			{
				obj.CanBeSetDown = false;
			}
			bool tilePassable = isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), viewport) && !isTileOccupiedForPlacement(tileLocation);
			if ((obj.CanBeSetDown | initialPlacement) && tilePassable && !isTileHoeDirt(tileLocation))
			{
				obj.TileLocation = tileLocation;
				if (objects.ContainsKey(tileLocation))
				{
					return false;
				}
				objects.Add(tileLocation, obj);
			}
			else if (doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Water", "Back") != null)
			{
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(28, 300f, 2, 1, dropLocation, flicker: false, obj.flipped));
				playSound("dropItemInWater");
			}
			else
			{
				if (obj.CanBeSetDown && !tilePassable)
				{
					return false;
				}
				if (obj.ParentSheetIndex >= 0 && obj.Type != null)
				{
					if (obj.Type.Equals("Fish") || obj.Type.Equals("Cooking") || obj.Type.Equals("Crafting"))
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(obj.ParentSheetIndex, 150f, 1, 3, dropLocation, flicker: true, obj.flipped));
					}
					else
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(obj.ParentSheetIndex + 1, 150f, 1, 3, dropLocation, flicker: true, obj.flipped));
					}
				}
			}
			return true;
		}

		private void rumbleAndFade(int milliseconds)
		{
			rumbleAndFadeEvent.Fire(milliseconds);
		}

		private void performRumbleAndFade(int milliseconds)
		{
			if (Game1.currentLocation == this)
			{
				Rumble.rumbleAndFade(1f, milliseconds);
			}
		}

		private void damagePlayers(Microsoft.Xna.Framework.Rectangle area, int damage)
		{
			damagePlayersEvent.Fire(new DamagePlayersEventArg
			{
				Area = area,
				Damage = damage
			});
		}

		private void performDamagePlayers(DamagePlayersEventArg arg)
		{
			if (Game1.player.currentLocation == this && Game1.player.GetBoundingBox().Intersects(arg.Area) && !Game1.player.onBridge.Value)
			{
				Game1.player.takeDamage(arg.Damage, overrideParry: true, null);
			}
		}

		public void explode(Vector2 tileLocation, int radius, Farmer who, bool damageFarmers = true, int damage_amount = -1)
		{
			bool insideCircle = false;
			updateMap();
			Vector2 currentTile = new Vector2(Math.Min(map.Layers[0].LayerWidth - 1, Math.Max(0f, tileLocation.X - (float)radius)), Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, tileLocation.Y - (float)radius)));
			bool[,] circleOutline2 = Game1.getCircleOutlineGrid(radius);
			Microsoft.Xna.Framework.Rectangle areaOfEffect = new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - (float)radius - 1f) * 64, (int)(tileLocation.Y - (float)radius - 1f) * 64, (radius * 2 + 1) * 64, (radius * 2 + 1) * 64);
			if (damage_amount > 0)
			{
				damageMonster(areaOfEffect, damage_amount, damage_amount, isBomb: true, who);
			}
			else
			{
				damageMonster(areaOfEffect, radius * 6, radius * 8, isBomb: true, who);
			}
			List<TemporaryAnimatedSprite> sprites = new List<TemporaryAnimatedSprite>();
			sprites.Add(new TemporaryAnimatedSprite(23, 9999f, 6, 1, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
			{
				light = true,
				lightRadius = radius,
				lightcolor = Color.Black,
				alphaFade = 0.03f - (float)radius * 0.003f,
				Parent = this
			});
			rumbleAndFade(300 + radius * 100);
			if (damageFarmers)
			{
				if (damage_amount > 0)
				{
					damagePlayers(areaOfEffect, damage_amount);
				}
				else
				{
					damagePlayers(areaOfEffect, radius * 3);
				}
			}
			for (int n = terrainFeatures.Count() - 1; n >= 0; n--)
			{
				KeyValuePair<Vector2, TerrainFeature> i = terrainFeatures.Pairs.ElementAt(n);
				if (i.Value.getBoundingBox(i.Key).Intersects(areaOfEffect) && i.Value.performToolAction(null, radius / 2, i.Key, this))
				{
					terrainFeatures.Remove(i.Key);
				}
			}
			for (int m = 0; m < radius * 2 + 1; m++)
			{
				for (int j = 0; j < radius * 2 + 1; j++)
				{
					if (m == 0 || j == 0 || m == radius * 2 || j == radius * 2)
					{
						insideCircle = circleOutline2[m, j];
					}
					else if (circleOutline2[m, j])
					{
						insideCircle = !insideCircle;
						if (!insideCircle)
						{
							if (objects.ContainsKey(currentTile) && objects[currentTile].onExplosion(who, this))
							{
								destroyObject(currentTile, who);
							}
							if (Game1.random.NextDouble() < 0.45)
							{
								if (Game1.random.NextDouble() < 0.5)
								{
									sprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
									{
										delayBeforeAnimationStart = Game1.random.Next(700)
									});
								}
								else
								{
									sprites.Add(new TemporaryAnimatedSprite(5, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), Color.White, 8, flipped: false, 50f)
									{
										delayBeforeAnimationStart = Game1.random.Next(200),
										scale = (float)Game1.random.Next(5, 15) / 10f
									});
								}
							}
						}
					}
					if (insideCircle)
					{
						explosionAt(currentTile.X, currentTile.Y);
						if (objects.ContainsKey(currentTile) && objects[currentTile].onExplosion(who, this))
						{
							destroyObject(currentTile, who);
						}
						if (Game1.random.NextDouble() < 0.45)
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								sprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
								{
									delayBeforeAnimationStart = Game1.random.Next(700)
								});
							}
							else
							{
								sprites.Add(new TemporaryAnimatedSprite(5, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), Color.White, 8, flipped: false, 50f)
								{
									delayBeforeAnimationStart = Game1.random.Next(200),
									scale = (float)Game1.random.Next(5, 15) / 10f
								});
							}
						}
						sprites.Add(new TemporaryAnimatedSprite(6, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(currentTile, tileLocation) * 20f));
					}
					currentTile.Y += 1f;
					currentTile.Y = Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, currentTile.Y));
				}
				currentTile.X += 1f;
				currentTile.Y = Math.Min(map.Layers[0].LayerWidth - 1, Math.Max(0f, currentTile.X));
				currentTile.Y = tileLocation.Y - (float)radius;
				currentTile.Y = Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, currentTile.Y));
			}
			Game1.multiplayer.broadcastSprites(this, sprites);
			radius /= 2;
			circleOutline2 = Game1.getCircleOutlineGrid(radius);
			currentTile = new Vector2((int)(tileLocation.X - (float)radius), (int)(tileLocation.Y - (float)radius));
			for (int l = 0; l < radius * 2 + 1; l++)
			{
				for (int k = 0; k < radius * 2 + 1; k++)
				{
					if (l == 0 || k == 0 || l == radius * 2 || k == radius * 2)
					{
						insideCircle = circleOutline2[l, k];
					}
					else if (circleOutline2[l, k])
					{
						insideCircle = !insideCircle;
						if (!insideCircle && !objects.ContainsKey(currentTile) && Game1.random.NextDouble() < 0.9 && doesTileHaveProperty((int)currentTile.X, (int)currentTile.Y, "Diggable", "Back") != null && !isTileHoeDirt(currentTile))
						{
							checkForBuriedItem((int)currentTile.X, (int)currentTile.Y, explosion: true, detectOnly: false, who);
							makeHoeDirt(currentTile);
						}
					}
					if (insideCircle && !objects.ContainsKey(currentTile) && Game1.random.NextDouble() < 0.9 && doesTileHaveProperty((int)currentTile.X, (int)currentTile.Y, "Diggable", "Back") != null && !isTileHoeDirt(currentTile))
					{
						checkForBuriedItem((int)currentTile.X, (int)currentTile.Y, explosion: true, detectOnly: false, who);
						makeHoeDirt(currentTile);
					}
					currentTile.Y += 1f;
					currentTile.Y = Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, currentTile.Y));
				}
				currentTile.X += 1f;
				currentTile.Y = Math.Min(map.Layers[0].LayerWidth - 1, Math.Max(0f, currentTile.X));
				currentTile.Y = tileLocation.Y - (float)radius;
				currentTile.Y = Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, currentTile.Y));
			}
		}

		public virtual void explosionAt(float x, float y)
		{
		}

		public void removeTemporarySpritesWithID(int id)
		{
			removeTemporarySpritesWithID((float)id);
		}

		public void removeTemporarySpritesWithID(float id)
		{
			removeTemporarySpritesWithIDEvent.Fire(id);
		}

		public void removeTemporarySpritesWithIDLocal(float id)
		{
			for (int i = temporarySprites.Count - 1; i >= 0; i--)
			{
				if (temporarySprites[i].id == id)
				{
					if (temporarySprites[i].hasLit)
					{
						Utility.removeLightSource(temporarySprites[i].lightID);
					}
					temporarySprites.RemoveAt(i);
				}
			}
		}

		public void makeHoeDirt(Vector2 tileLocation, bool ignoreChecks = false)
		{
			if ((ignoreChecks || (doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") != null && !isTileOccupied(tileLocation) && isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport))) && (!(this is MineShaft) || (this as MineShaft).getMineArea() != 77377))
			{
				terrainFeatures.Add(tileLocation, new HoeDirt((Game1.IsRainingHere(this) && (bool)isOutdoors && !Name.Equals("Desert")) ? 1 : 0, this));
			}
		}

		public int numberOfObjectsOfType(int index, bool bigCraftable)
		{
			int number = 0;
			foreach (KeyValuePair<Vector2, Object> v in Objects.Pairs)
			{
				if ((int)v.Value.parentSheetIndex == index && (bool)v.Value.bigCraftable == bigCraftable)
				{
					number++;
				}
			}
			return number;
		}

		public void passTimeForObjects(int timeElapsed)
		{
			lock (_objectUpdateList)
			{
				_objectUpdateList.Clear();
				foreach (KeyValuePair<Vector2, Object> pair2 in objects.Pairs)
				{
					_objectUpdateList.Add(pair2);
				}
				for (int i = _objectUpdateList.Count - 1; i >= 0; i--)
				{
					KeyValuePair<Vector2, Object> pair = _objectUpdateList[i];
					if (pair.Value.minutesElapsed(timeElapsed, this))
					{
						Vector2 key = pair.Key;
						objects.Remove(key);
					}
				}
				_objectUpdateList.Clear();
			}
		}

		public virtual void performTenMinuteUpdate(int timeOfDay)
		{
			foreach (Furniture item in furniture)
			{
				item.minutesElapsed(10, this);
			}
			for (int k = 0; k < characters.Count; k++)
			{
				NPC character = characters[k];
				if (!character.IsInvisible)
				{
					character.checkSchedule(timeOfDay);
					character.performTenMinuteUpdate(timeOfDay, this);
				}
			}
			passTimeForObjects(10);
			if ((bool)isOutdoors)
			{
				Random r = new Random(timeOfDay + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
				if (Equals(Game1.currentLocation))
				{
					tryToAddCritters(onlyIfOnScreen: true);
				}
				if (Game1.IsMasterGame)
				{
					if (fishSplashPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.5 && (!(this is Farm) || Game1.whichFarm == 1))
					{
						for (int tries = 0; tries < 2; tries++)
						{
							Point p = new Point(r.Next(0, map.GetLayer("Back").LayerWidth), r.Next(0, map.GetLayer("Back").LayerHeight));
							if (!isOpenWater(p.X, p.Y) || doesTileHaveProperty(p.X, p.Y, "NoFishing", "Back") != null)
							{
								continue;
							}
							int toLand = FishingRod.distanceToLand(p.X, p.Y, this);
							if (toLand > 1 && toLand < 5)
							{
								if (Game1.player.currentLocation.Equals(this))
								{
									playSound("waterSlosh");
								}
								fishSplashPoint.Value = p;
								break;
							}
						}
					}
					else if (!fishSplashPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.1)
					{
						fishSplashPoint.Value = Point.Zero;
					}
					performOrePanTenMinuteUpdate(r);
				}
			}
			if (Game1.dayOfMonth % 7 == 0 && name.Equals("Saloon") && Game1.timeOfDay >= 1200 && Game1.timeOfDay <= 1500 && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom"))
			{
				if (Game1.timeOfDay == 1500)
				{
					removeTemporarySpritesWithID(2400);
				}
				else
				{
					bool goodEvent = Game1.random.NextDouble() < 0.25;
					bool badEvent = Game1.random.NextDouble() < 0.25;
					List<NPC> sportsBoys = new List<NPC>();
					foreach (NPC j in characters)
					{
						if (j.getTileY() < 12 && j.getTileX() > 26 && Game1.random.NextDouble() < ((goodEvent | badEvent) ? 0.66 : 0.25))
						{
							sportsBoys.Add(j);
						}
					}
					foreach (NPC i in sportsBoys)
					{
						i.showTextAboveHead(Game1.content.LoadString("Strings\\Characters:Saloon_" + (goodEvent ? "goodEvent" : (badEvent ? "badEvent" : "neutralEvent")) + "_" + Game1.random.Next(5)));
						if (goodEvent && Game1.random.NextDouble() < 0.55)
						{
							i.jump();
						}
					}
				}
			}
			if (name.Equals("BugLand") && Game1.random.NextDouble() <= 0.2 && Game1.currentLocation.Equals(this))
			{
				characters.Add(new Fly(getRandomTile() * 64f, hard: true));
			}
		}

		public virtual void performOrePanTenMinuteUpdate(Random r)
		{
			if (Game1.MasterPlayer.mailReceived.Contains("ccFishTank") && !(this is Beach) && orePanPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.5)
			{
				int tries = 0;
				Point p;
				while (true)
				{
					if (tries >= 6)
					{
						return;
					}
					p = new Point(r.Next(0, Map.GetLayer("Back").LayerWidth), r.Next(0, Map.GetLayer("Back").LayerHeight));
					if (isOpenWater(p.X, p.Y) && FishingRod.distanceToLand(p.X, p.Y, this) <= 1 && getTileIndexAt(p, "Buildings") == -1)
					{
						break;
					}
					tries++;
				}
				if (Game1.player.currentLocation.Equals(this))
				{
					playSound("slosh");
				}
				orePanPoint.Value = p;
			}
			else if (!orePanPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.1)
			{
				orePanPoint.Value = Point.Zero;
			}
		}

		public bool dropObject(Object obj)
		{
			return dropObject(obj, obj.TileLocation, Game1.viewport, initialPlacement: false);
		}

		public virtual int getFishingLocation(Vector2 tile)
		{
			return -1;
		}

		public virtual bool IsUsingMagicBait(Farmer who)
		{
			if (who != null && who.CurrentTool != null && who.CurrentTool is FishingRod)
			{
				return (who.CurrentTool as FishingRod).getBaitAttachmentIndex() == 908;
			}
			return false;
		}

		public virtual Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			Object caught2 = null;
			int whichFish = -1;
			Dictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			bool favBait = false;
			bool using_magic_bait = IsUsingMagicBait(who);
			string nameToUse = (locationName == null) ? ((string)name) : locationName;
			if (nameToUse == "BeachNightMarket")
			{
				nameToUse = "Beach";
			}
			if (name.Equals("WitchSwamp") && !Game1.MasterPlayer.mailReceived.Contains("henchmanGone") && Game1.random.NextDouble() < 0.25 && !Game1.player.hasItemInInventory(308, 1))
			{
				return new Object(308, 1);
			}
			if (locationData.ContainsKey(nameToUse))
			{
				string[] rawFishData = locationData[nameToUse].Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Split(' ');
				if (using_magic_bait)
				{
					List<string> all_season_fish = new List<string>();
					for (int l = 0; l < 4; l++)
					{
						if (locationData[nameToUse].Split('/')[4 + l].Split(' ').Length > 1)
						{
							all_season_fish.AddRange(locationData[nameToUse].Split('/')[4 + l].Split(' '));
						}
					}
					rawFishData = all_season_fish.ToArray();
				}
				Dictionary<string, string> rawFishDataWithLocation = new Dictionary<string, string>();
				if (rawFishData.Length > 1)
				{
					for (int k = 0; k < rawFishData.Length; k += 2)
					{
						rawFishDataWithLocation[rawFishData[k]] = rawFishData[k + 1];
					}
				}
				string[] keys = rawFishDataWithLocation.Keys.ToArray();
				Dictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
				Utility.Shuffle(Game1.random, keys);
				for (int j = 0; j < keys.Length; j++)
				{
					bool fail = true;
					string[] specificFishData = fishData[Convert.ToInt32(keys[j])].Split('/');
					string[] timeSpans = specificFishData[5].Split(' ');
					int location = Convert.ToInt32(rawFishDataWithLocation[keys[j]]);
					if (location == -1 || getFishingLocation(who.getTileLocation()) == location)
					{
						for (int i = 0; i < timeSpans.Length; i += 2)
						{
							if (Game1.timeOfDay >= Convert.ToInt32(timeSpans[i]) && Game1.timeOfDay < Convert.ToInt32(timeSpans[i + 1]))
							{
								fail = false;
								break;
							}
						}
					}
					if (!specificFishData[7].Equals("both"))
					{
						if (specificFishData[7].Equals("rainy") && !Game1.IsRainingHere(this))
						{
							fail = true;
						}
						else if (specificFishData[7].Equals("sunny") && Game1.IsRainingHere(this))
						{
							fail = true;
						}
					}
					if (using_magic_bait)
					{
						fail = false;
					}
					bool beginnersRod = who != null && who.CurrentTool != null && who.CurrentTool is FishingRod && (int)who.CurrentTool.upgradeLevel == 1;
					if (Convert.ToInt32(specificFishData[1]) >= 50 && beginnersRod)
					{
						fail = true;
					}
					if (who.FishingLevel < Convert.ToInt32(specificFishData[12]))
					{
						fail = true;
					}
					if (!fail)
					{
						double chance4 = Convert.ToDouble(specificFishData[10]);
						double dropOffAmount = Convert.ToDouble(specificFishData[11]) * chance4;
						chance4 -= (double)Math.Max(0, Convert.ToInt32(specificFishData[9]) - waterDepth) * dropOffAmount;
						chance4 += (double)((float)who.FishingLevel / 50f);
						if (beginnersRod)
						{
							chance4 *= 1.1;
						}
						chance4 = Math.Min(chance4, 0.89999997615814209);
						if (chance4 < 0.25 && who != null && who.CurrentTool != null && who.CurrentTool is FishingRod && (who.CurrentTool as FishingRod).getBobberAttachmentIndex() == 856)
						{
							float max = 0.25f;
							float min = 0.08f;
							chance4 = (double)((max - min) / max) * chance4 + (double)((max - min) / 2f);
						}
						if (Game1.random.NextDouble() <= chance4)
						{
							whichFish = Convert.ToInt32(keys[j]);
							break;
						}
					}
				}
			}
			bool wasTrash = false;
			if (whichFish == -1)
			{
				whichFish = Game1.random.Next(167, 173);
				wasTrash = true;
			}
			if ((who.fishCaught == null || who.fishCaught.Count() == 0) && whichFish >= 152)
			{
				whichFish = 145;
			}
			if (!Game1.isFestival() && Game1.random.NextDouble() <= 0.15 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				whichFish = 890;
			}
			if (who.currentLocation.HasUnlockedAreaSecretNotes(who) && wasTrash && Game1.random.NextDouble() < 0.08)
			{
				Object o = tryToCreateUnseenSecretNote(who);
				if (o != null)
				{
					return o;
				}
			}
			caught2 = new Object(whichFish, 1);
			if (favBait)
			{
				caught2.scale.X = 1f;
			}
			return caught2;
		}

		public virtual bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			bool isActionable = false;
			string action_property = doesTileHaveProperty(xTile, yTile, "Action", "Buildings");
			if (action_property != null)
			{
				isActionable = true;
				if (action_property.StartsWith("DropBox"))
				{
					isActionable = false;
					string[] split = action_property.Split(' ');
					if (split.Length >= 2 && Game1.player.team.specialOrders != null)
					{
						foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
						{
							if (specialOrder.UsesDropBox(split[1]))
							{
								isActionable = true;
								break;
							}
						}
					}
				}
				if (action_property.Contains("Message"))
				{
					if (action_property.Contains("Speech"))
					{
						Game1.isSpeechAtCurrentCursorTile = true;
					}
					else
					{
						Game1.isInspectionAtCurrentCursorTile = true;
					}
				}
			}
			if (objects.ContainsKey(new Vector2(xTile, yTile)) && objects[new Vector2(xTile, yTile)].isActionable(who))
			{
				isActionable = true;
			}
			if (!Game1.isFestival() && terrainFeatures.ContainsKey(new Vector2(xTile, yTile)) && terrainFeatures[new Vector2(xTile, yTile)].isActionable())
			{
				isActionable = true;
			}
			if (isActionable && !Utility.tileWithinRadiusOfPlayer(xTile, yTile, 1, who))
			{
				Game1.mouseCursorTransparency = 0.5f;
			}
			return isActionable;
		}

		public virtual void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
		{
			Random r = new Random(xLocation * 2000 + yLocation + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
			int toDigUp2 = -1;
			bool archaeologyEnchant = who != null && who.CurrentTool != null && who.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<ArchaeologistEnchantment>();
			string[] split2 = null;
			foreach (KeyValuePair<int, string> v in Game1.objectInformation)
			{
				split2 = v.Value.Split('/');
				if (split2[3].Contains("Arch"))
				{
					string[] archSplit = split2[6].Split(' ');
					for (int j = 0; j < archSplit.Length; j += 2)
					{
						if (archSplit[j].Equals(name) && r.NextDouble() < (double)((!archaeologyEnchant) ? 1 : 2) * Convert.ToDouble(archSplit[j + 1], CultureInfo.InvariantCulture))
						{
							toDigUp2 = v.Key;
							break;
						}
					}
				}
				if (toDigUp2 != -1)
				{
					break;
				}
			}
			if (r.NextDouble() < 0.2 && !(this is Farm))
			{
				toDigUp2 = 102;
			}
			if (toDigUp2 == 102 && (int)Game1.netWorldState.Value.LostBooksFound >= 21)
			{
				toDigUp2 = 770;
			}
			if (toDigUp2 != -1)
			{
				Game1.createObjectDebris(toDigUp2, xLocation, yLocation, who.UniqueMultiplayerID);
				who.gainExperience(5, 25);
				return;
			}
			bool generousEnchant = who != null && who.CurrentTool != null && who.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
			float generousChance = 0.5f;
			if (Game1.GetSeasonForLocation(this).Equals("winter") && r.NextDouble() < 0.5 && !(this is Desert))
			{
				if (r.NextDouble() < 0.4)
				{
					Game1.createObjectDebris(416, xLocation, yLocation, who.UniqueMultiplayerID);
					if (generousEnchant && r.NextDouble() < (double)generousChance)
					{
						Game1.createObjectDebris(416, xLocation, yLocation, who.UniqueMultiplayerID);
					}
				}
				else
				{
					Game1.createObjectDebris(412, xLocation, yLocation, who.UniqueMultiplayerID);
					if (generousEnchant && r.NextDouble() < (double)generousChance)
					{
						Game1.createObjectDebris(412, xLocation, yLocation, who.UniqueMultiplayerID);
					}
				}
				return;
			}
			if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				Game1.createMultipleObjectDebris(890, xLocation, yLocation, r.Next(2, 6), who.UniqueMultiplayerID, this);
			}
			if (Game1.GetSeasonForLocation(this).Equals("spring") && r.NextDouble() < 0.0625 && !(this is Desert) && !(this is Beach))
			{
				Game1.createMultipleObjectDebris(273, xLocation, yLocation, r.Next(2, 6), who.UniqueMultiplayerID, this);
				if (generousEnchant && r.NextDouble() < (double)generousChance)
				{
					Game1.createMultipleObjectDebris(273, xLocation, yLocation, r.Next(2, 6), who.UniqueMultiplayerID, this);
				}
				return;
			}
			if (Game1.random.NextDouble() <= 0.2 && (Game1.MasterPlayer.mailReceived.Contains("guntherBones") || (Game1.player.team.specialOrders.Where((SpecialOrder x) => (string)x.questKey == "Gunther") != null && Game1.player.team.specialOrders.Where((SpecialOrder x) => (string)x.questKey == "Gunther").Count() > 0)))
			{
				Game1.createMultipleObjectDebris(881, xLocation, yLocation, r.Next(2, 6), who.UniqueMultiplayerID, this);
			}
			Dictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			if (!locationData.ContainsKey(name))
			{
				return;
			}
			string[] rawData = locationData[name].Split('/')[8].Split(' ');
			if (rawData.Length == 0 || rawData[0].Equals("-1"))
			{
				return;
			}
			int i = 0;
			while (true)
			{
				if (i < rawData.Length)
				{
					if (r.NextDouble() <= Convert.ToDouble(rawData[i + 1]))
					{
						break;
					}
					i += 2;
					continue;
				}
				return;
			}
			toDigUp2 = Convert.ToInt32(rawData[i]);
			if (Game1.objectInformation.ContainsKey(toDigUp2) && (Game1.objectInformation[toDigUp2].Split('/')[3].Contains("Arch") || toDigUp2 == 102))
			{
				if (toDigUp2 == 102 && (int)Game1.netWorldState.Value.LostBooksFound >= 21)
				{
					toDigUp2 = 770;
				}
				Game1.createObjectDebris(toDigUp2, xLocation, yLocation, who.UniqueMultiplayerID);
				return;
			}
			if (toDigUp2 == 330 && HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.11)
			{
				Object o = tryToCreateUnseenSecretNote(who);
				if (o != null)
				{
					Game1.createItemDebris(o, new Vector2((float)xLocation + 0.5f, (float)yLocation + 0.5f) * 64f, -1, this);
					return;
				}
			}
			else if (toDigUp2 == 330 && Game1.stats.DaysPlayed > 28 && Game1.random.NextDouble() < 0.1)
			{
				Game1.createMultipleObjectDebris(688 + Game1.random.Next(3), xLocation, yLocation, 1, who.UniqueMultiplayerID);
			}
			Game1.createMultipleObjectDebris(toDigUp2, xLocation, yLocation, r.Next(1, 4), who.UniqueMultiplayerID);
			if (generousEnchant && r.NextDouble() < (double)generousChance)
			{
				Game1.createMultipleObjectDebris(toDigUp2, xLocation, yLocation, r.Next(1, 4), who.UniqueMultiplayerID);
			}
		}

		public virtual string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			Random r = new Random(xLocation * 2000 + yLocation * 77 + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)Game1.stats.DirtHoed);
			string item = doesTileHaveProperty(xLocation, yLocation, "Treasure", "Back");
			if (item != null)
			{
				string[] treasureDescription = item.Split(' ');
				if (detectOnly)
				{
					return treasureDescription[0];
				}
				switch (treasureDescription[0])
				{
				case "Coins":
					Game1.createObjectDebris(330, xLocation, yLocation);
					break;
				case "Copper":
					Game1.createDebris(0, xLocation, yLocation, Convert.ToInt32(treasureDescription[1]));
					break;
				case "Coal":
					Game1.createDebris(4, xLocation, yLocation, Convert.ToInt32(treasureDescription[1]));
					break;
				case "Iron":
					Game1.createDebris(2, xLocation, yLocation, Convert.ToInt32(treasureDescription[1]));
					break;
				case "Gold":
					Game1.createDebris(6, xLocation, yLocation, Convert.ToInt32(treasureDescription[1]));
					break;
				case "Iridium":
					Game1.createDebris(10, xLocation, yLocation, Convert.ToInt32(treasureDescription[1]));
					break;
				case "CaveCarrot":
					Game1.createObjectDebris(78, xLocation, yLocation);
					break;
				case "Arch":
					Game1.createObjectDebris(Convert.ToInt32(treasureDescription[1]), xLocation, yLocation);
					break;
				case "Object":
					Game1.createObjectDebris(Convert.ToInt32(treasureDescription[1]), xLocation, yLocation);
					if (Convert.ToInt32(treasureDescription[1]) == 78)
					{
						Game1.stats.CaveCarrotsFound++;
					}
					break;
				}
				map.GetLayer("Back").Tiles[xLocation, yLocation].Properties["Treasure"] = null;
			}
			else
			{
				bool generousEnchant = who != null && who.CurrentTool != null && who.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
				float generousChance = 0.5f;
				if (!isFarm && (bool)isOutdoors && Game1.GetSeasonForLocation(this).Equals("winter") && r.NextDouble() < 0.08 && !explosion && !detectOnly && !(this is Desert))
				{
					Game1.createObjectDebris((r.NextDouble() < 0.5) ? 412 : 416, xLocation, yLocation);
					if (generousEnchant && r.NextDouble() < (double)generousChance)
					{
						Game1.createObjectDebris((r.NextDouble() < 0.5) ? 412 : 416, xLocation, yLocation);
					}
					return "";
				}
				if ((bool)isOutdoors && r.NextDouble() < 0.03 && !explosion)
				{
					if (detectOnly)
					{
						map.GetLayer("Back").Tiles[xLocation, yLocation].Properties.Add("Treasure", new PropertyValue("Object " + 330));
						return "Object";
					}
					Game1.createObjectDebris(330, xLocation, yLocation);
					if (generousEnchant && r.NextDouble() < (double)generousChance)
					{
						Game1.createObjectDebris(330, xLocation, yLocation);
					}
					return "";
				}
			}
			return "";
		}

		public void setAnimatedMapTile(int tileX, int tileY, int[] animationTileIndexes, long interval, string layer, string action, int whichTileSheet = 0)
		{
			StaticTile[] tiles = new StaticTile[animationTileIndexes.Count()];
			for (int i = 0; i < animationTileIndexes.Count(); i++)
			{
				tiles[i] = new StaticTile(map.GetLayer(layer), map.TileSheets[whichTileSheet], BlendMode.Alpha, animationTileIndexes[i]);
			}
			map.GetLayer(layer).Tiles[tileX, tileY] = new AnimatedTile(map.GetLayer(layer), tiles, interval);
			if (action != null && layer != null && layer.Equals("Buildings"))
			{
				map.GetLayer("Buildings").Tiles[tileX, tileY].Properties.Add("Action", new PropertyValue(action));
			}
		}

		public virtual bool AllowMapModificationsInResetState()
		{
			return false;
		}

		public void setMapTile(int tileX, int tileY, int index, string layer, string action, int whichTileSheet = 0)
		{
			map.GetLayer(layer).Tiles[tileX, tileY] = new StaticTile(map.GetLayer(layer), map.TileSheets[whichTileSheet], BlendMode.Alpha, index);
			if (action != null && layer != null && layer.Equals("Buildings"))
			{
				map.GetLayer("Buildings").Tiles[tileX, tileY].Properties.Add("Action", new PropertyValue(action));
			}
		}

		public void setMapTileIndex(int tileX, int tileY, int index, string layer, int whichTileSheet = 0)
		{
			if (map != null)
			{
				try
				{
					if (map.GetLayer(layer).Tiles[tileX, tileY] != null)
					{
						if (index == -1)
						{
							map.GetLayer(layer).Tiles[tileX, tileY] = null;
						}
						else
						{
							map.GetLayer(layer).Tiles[tileX, tileY].TileIndex = index;
						}
					}
					else
					{
						map.GetLayer(layer).Tiles[tileX, tileY] = new StaticTile(map.GetLayer(layer), map.TileSheets[whichTileSheet], BlendMode.Alpha, index);
					}
				}
				catch (Exception)
				{
				}
			}
		}

		public virtual void shiftObjects(int dx, int dy)
		{
			List<KeyValuePair<Vector2, Object>> list = new List<KeyValuePair<Vector2, Object>>(objects.Pairs);
			objects.Clear();
			foreach (KeyValuePair<Vector2, Object> v in list)
			{
				if (v.Value.lightSource != null)
				{
					removeLightSource(v.Value.lightSource.identifier);
				}
				v.Value.tileLocation.Value = new Vector2(v.Key.X + (float)dx, v.Key.Y + (float)dy);
				objects.Add(v.Value.tileLocation, v.Value);
				v.Value.initializeLightSource(v.Value.tileLocation);
			}
		}

		public int getTileIndexAt(Point p, string layer)
		{
			return map.GetLayer(layer).Tiles[p.X, p.Y]?.TileIndex ?? (-1);
		}

		public int getTileIndexAt(int x, int y, string layer)
		{
			Layer tileLayer = map.GetLayer(layer);
			if (tileLayer != null)
			{
				return tileLayer.Tiles[x, y]?.TileIndex ?? (-1);
			}
			return -1;
		}

		public string getTileSheetIDAt(int x, int y, string layer)
		{
			if (map.GetLayer(layer) != null)
			{
				Tile tmp = map.GetLayer(layer).Tiles[x, y];
				if (tmp != null)
				{
					return tmp.TileSheet.Id;
				}
				return "";
			}
			return "";
		}

		public void OnStoneDestroyed(int indexOfStone, int x, int y, Farmer who)
		{
			if (who != null && Game1.random.NextDouble() <= 0.02 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				Game1.createMultipleObjectDebris(890, x, y, 1, who.UniqueMultiplayerID, this);
			}
			if (!Name.StartsWith("UndergroundMine"))
			{
				if (indexOfStone == 343 || indexOfStone == 450)
				{
					Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + x * 2000 + y);
					if (r.NextDouble() < 0.035 && Game1.stats.DaysPlayed > 1)
					{
						Game1.createObjectDebris(535 + ((Game1.stats.DaysPlayed > 60 && r.NextDouble() < 0.2) ? 1 : ((Game1.stats.DaysPlayed > 120 && r.NextDouble() < 0.2) ? 2 : 0)), x, y, who.UniqueMultiplayerID, this);
					}
					if (r.NextDouble() < 0.035 * (double)((!who.professions.Contains(21)) ? 1 : 2) && Game1.stats.DaysPlayed > 1)
					{
						Game1.createObjectDebris(382, x, y, who.UniqueMultiplayerID, this);
					}
					if (r.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 1)
					{
						Game1.createObjectDebris(390, x, y, who.UniqueMultiplayerID, this);
					}
				}
				breakStone(indexOfStone, x, y, who, new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + x * 4000 + y));
			}
			else
			{
				(this as MineShaft).checkStoneForItems(indexOfStone, x, y, who);
			}
		}

		protected virtual bool breakStone(int indexOfStone, int x, int y, Farmer who, Random r)
		{
			int experience2 = 0;
			int addedOres = who.professions.Contains(18) ? 1 : 0;
			if (indexOfStone == 44)
			{
				indexOfStone = Game1.random.Next(1, 8) * 2;
			}
			switch (indexOfStone)
			{
			case 95:
				Game1.createMultipleObjectDebris(909, x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 200f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				experience2 = 18;
				break;
			case 843:
			case 844:
				Game1.createMultipleObjectDebris(848, x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 200f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				experience2 = 12;
				break;
			case 25:
				Game1.createMultipleObjectDebris(719, x, y, r.Next(2, 5), who.uniqueMultiplayerID, this);
				experience2 = 5;
				if (this is IslandLocation && r.NextDouble() < 0.1)
				{
					Game1.player.team.RequestLimitedNutDrops("MusselStone", this, x * 64, y * 64, 5);
				}
				break;
			case 75:
				Game1.createObjectDebris(535, x, y, who.uniqueMultiplayerID, this);
				experience2 = 8;
				break;
			case 76:
				Game1.createObjectDebris(536, x, y, who.uniqueMultiplayerID, this);
				experience2 = 16;
				break;
			case 77:
				Game1.createObjectDebris(537, x, y, who.uniqueMultiplayerID, this);
				experience2 = 32;
				break;
			case 816:
			case 817:
				if (r.NextDouble() < 0.1)
				{
					Game1.createObjectDebris(823, x, y, who.uniqueMultiplayerID, this);
				}
				else if (r.NextDouble() < 0.015)
				{
					Game1.createObjectDebris(824, x, y, who.uniqueMultiplayerID, this);
				}
				else if (r.NextDouble() < 0.1)
				{
					Game1.createObjectDebris(579 + r.Next(11), x, y, who.uniqueMultiplayerID, this);
				}
				Game1.createMultipleObjectDebris(881, x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				experience2 = 6;
				break;
			case 818:
				Game1.createMultipleObjectDebris(330, x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				experience2 = 6;
				break;
			case 819:
				Game1.createObjectDebris(749, x, y, who.uniqueMultiplayerID, this);
				experience2 = 64;
				break;
			case 8:
				Game1.createObjectDebris(66, x, y, who.uniqueMultiplayerID, this);
				experience2 = 16;
				break;
			case 10:
				Game1.createObjectDebris(68, x, y, who.uniqueMultiplayerID, this);
				experience2 = 16;
				break;
			case 12:
				Game1.createObjectDebris(60, x, y, who.uniqueMultiplayerID, this);
				experience2 = 80;
				break;
			case 14:
				Game1.createObjectDebris(62, x, y, who.uniqueMultiplayerID, this);
				experience2 = 40;
				break;
			case 6:
				Game1.createObjectDebris(70, x, y, who.uniqueMultiplayerID, this);
				experience2 = 40;
				break;
			case 4:
				Game1.createObjectDebris(64, x, y, who.uniqueMultiplayerID, this);
				experience2 = 80;
				break;
			case 2:
				Game1.createObjectDebris(72, x, y, who.uniqueMultiplayerID, this);
				experience2 = 150;
				break;
			case 668:
			case 670:
			case 845:
			case 846:
			case 847:
				Game1.createMultipleObjectDebris(390, x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				experience2 = 3;
				if (r.NextDouble() < 0.08)
				{
					Game1.createMultipleObjectDebris(382, x, y, 1 + addedOres, who.uniqueMultiplayerID, this);
					experience2 = 4;
				}
				break;
			case 751:
			case 849:
				Game1.createMultipleObjectDebris(378, x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				experience2 = 5;
				Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Orange * 0.5f, 175, 100));
				break;
			case 290:
			case 850:
				Game1.createMultipleObjectDebris(380, x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				experience2 = 12;
				Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.White * 0.5f, 175, 100));
				break;
			case 764:
				Game1.createMultipleObjectDebris(384, x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				experience2 = 18;
				Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Yellow * 0.5f, 175, 100));
				break;
			case 765:
				Game1.createMultipleObjectDebris(386, x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				experience2 = 50;
				Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 6, Color.BlueViolet * 0.5f, 175, 100));
				if (r.NextDouble() < 0.04)
				{
					Game1.createMultipleObjectDebris(74, x, y, 1);
				}
				experience2 = 50;
				break;
			}
			if (who.professions.Contains(19) && r.NextDouble() < 0.5)
			{
				switch (indexOfStone)
				{
				case 8:
					Game1.createObjectDebris(66, x, y, who.uniqueMultiplayerID, this);
					experience2 = 8;
					break;
				case 10:
					Game1.createObjectDebris(68, x, y, who.uniqueMultiplayerID, this);
					experience2 = 8;
					break;
				case 12:
					Game1.createObjectDebris(60, x, y, who.uniqueMultiplayerID, this);
					experience2 = 50;
					break;
				case 14:
					Game1.createObjectDebris(62, x, y, who.uniqueMultiplayerID, this);
					experience2 = 20;
					break;
				case 6:
					Game1.createObjectDebris(70, x, y, who.uniqueMultiplayerID, this);
					experience2 = 20;
					break;
				case 4:
					Game1.createObjectDebris(64, x, y, who.uniqueMultiplayerID, this);
					experience2 = 50;
					break;
				case 2:
					Game1.createObjectDebris(72, x, y, who.uniqueMultiplayerID, this);
					experience2 = 100;
					break;
				}
			}
			if (indexOfStone == 46)
			{
				Game1.createDebris(10, x, y, r.Next(1, 4), this);
				Game1.createDebris(6, x, y, r.Next(1, 5), this);
				if (r.NextDouble() < 0.25)
				{
					Game1.createMultipleObjectDebris(74, x, y, 1, who.uniqueMultiplayerID, this);
				}
				experience2 = 50;
				Game1.stats.MysticStonesCrushed++;
			}
			if (((bool)isOutdoors || (bool)treatAsOutdoors) && experience2 == 0)
			{
				double chanceModifier = who.DailyLuck / 2.0 + (double)who.MiningLevel * 0.005 + (double)who.LuckLevel * 0.001;
				Random ran = new Random(x * 1000 + y + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
				Game1.createDebris(14, x, y, 1, this);
				who.gainExperience(3, 1);
				if (who.professions.Contains(21) && ran.NextDouble() < 0.05 * (1.0 + chanceModifier))
				{
					Game1.createObjectDebris(382, x, y, who.UniqueMultiplayerID, this);
				}
				if (ran.NextDouble() < 0.05 * (1.0 + chanceModifier))
				{
					ran.Next(1, 3);
					ran.NextDouble();
					_ = 0.1 * (1.0 + chanceModifier);
					Game1.createObjectDebris(382, x, y, who.UniqueMultiplayerID, this);
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, Game1.random.NextDouble() < 0.5, 80f, 0, -1, -1f, 128));
					who.gainExperience(3, 5);
				}
			}
			if (HasUnlockedAreaSecretNotes(who) && r.NextDouble() < 0.0075)
			{
				Object o = tryToCreateUnseenSecretNote(who);
				if (o != null)
				{
					Game1.createItemDebris(o, new Vector2((float)x + 0.5f, (float)y + 0.75f) * 64f, Game1.player.facingDirection, this);
				}
			}
			who.gainExperience(3, experience2);
			return experience2 > 0;
		}

		public bool isBehindBush(Vector2 Tile)
		{
			if (largeTerrainFeatures != null)
			{
				Microsoft.Xna.Framework.Rectangle down = new Microsoft.Xna.Framework.Rectangle((int)Tile.X * 64, (int)(Tile.Y + 1f) * 64, 64, 128);
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(down))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool isBehindTree(Vector2 Tile)
		{
			if (terrainFeatures != null)
			{
				Microsoft.Xna.Framework.Rectangle down = new Microsoft.Xna.Framework.Rectangle((int)(Tile.X - 1f) * 64, (int)Tile.Y * 64, 192, 256);
				foreach (KeyValuePair<Vector2, TerrainFeature> i in terrainFeatures.Pairs)
				{
					if (i.Value is Tree && i.Value.getBoundingBox(i.Key).Intersects(down))
					{
						return true;
					}
				}
			}
			return false;
		}

		public virtual void spawnObjects()
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
			Dictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			if (locationData.ContainsKey(name))
			{
				string rawData = locationData[name].Split('/')[Utility.getSeasonNumber(GetSeasonForLocation())];
				if (!rawData.Equals("-1") && numberOfSpawnedObjectsOnMap < 6)
				{
					string[] split = rawData.Split(' ');
					int numberToSpawn = r.Next(1, Math.Min(5, 7 - numberOfSpawnedObjectsOnMap));
					for (int k = 0; k < numberToSpawn; k++)
					{
						for (int j = 0; j < 11; j++)
						{
							int xCoord2 = r.Next(map.DisplayWidth / 64);
							int yCoord2 = r.Next(map.DisplayHeight / 64);
							Vector2 location2 = new Vector2(xCoord2, yCoord2);
							objects.TryGetValue(location2, out Object o);
							int whichObject = r.Next(split.Length / 2) * 2;
							if (o == null && doesTileHaveProperty(xCoord2, yCoord2, "Spawnable", "Back") != null && !doesEitherTileOrTileIndexPropertyEqual(xCoord2, yCoord2, "Spawnable", "Back", "F") && r.NextDouble() < Convert.ToDouble(split[whichObject + 1], CultureInfo.InvariantCulture) && isTileLocationTotallyClearAndPlaceable(xCoord2, yCoord2) && getTileIndexAt(xCoord2, yCoord2, "AlwaysFront") == -1 && getTileIndexAt(xCoord2, yCoord2, "Front") == -1 && !isBehindBush(location2) && (Game1.random.NextDouble() < 0.1 || !isBehindTree(location2)) && dropObject(new Object(location2, Convert.ToInt32(split[whichObject]), null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: true), new Vector2(xCoord2 * 64, yCoord2 * 64), Game1.viewport, initialPlacement: true))
							{
								numberOfSpawnedObjectsOnMap++;
								break;
							}
						}
					}
				}
			}
			List<Vector2> positionOfArtifactSpots = new List<Vector2>();
			foreach (KeyValuePair<Vector2, Object> v in objects.Pairs)
			{
				if ((int)v.Value.parentSheetIndex == 590)
				{
					positionOfArtifactSpots.Add(v.Key);
				}
			}
			if (!(this is Farm) && !(this is IslandWest))
			{
				spawnWeedsAndStones();
			}
			for (int i = positionOfArtifactSpots.Count - 1; i >= 0; i--)
			{
				if ((!(this is IslandNorth) || !(positionOfArtifactSpots[i].X < 26f)) && Game1.random.NextDouble() < 0.15)
				{
					objects.Remove(positionOfArtifactSpots.ElementAt(i));
					positionOfArtifactSpots.RemoveAt(i);
				}
			}
			if (positionOfArtifactSpots.Count > ((!(this is Farm)) ? 1 : 0) && (!GetSeasonForLocation().Equals("winter") || positionOfArtifactSpots.Count > 4))
			{
				return;
			}
			double chanceForNewArtifactAttempt = 1.0;
			while (r.NextDouble() < chanceForNewArtifactAttempt)
			{
				int xCoord = r.Next(map.DisplayWidth / 64);
				int yCoord = r.Next(map.DisplayHeight / 64);
				Vector2 location = new Vector2(xCoord, yCoord);
				if (isTileLocationTotallyClearAndPlaceable(location) && getTileIndexAt(xCoord, yCoord, "AlwaysFront") == -1 && getTileIndexAt(xCoord, yCoord, "Front") == -1 && !isBehindBush(location) && (doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null || (GetSeasonForLocation().Equals("winter") && doesTileHaveProperty(xCoord, yCoord, "Type", "Back") != null && doesTileHaveProperty(xCoord, yCoord, "Type", "Back").Equals("Grass"))))
				{
					if (name.Equals("Forest") && xCoord >= 93 && yCoord <= 22)
					{
						continue;
					}
					objects.Add(location, new Object(location, 590, 1));
				}
				chanceForNewArtifactAttempt *= 0.75;
				if (GetSeasonForLocation().Equals("winter"))
				{
					chanceForNewArtifactAttempt += 0.10000000149011612;
				}
			}
		}

		public bool isTileLocationOpen(Location location)
		{
			if (map.GetLayer("Buildings").Tiles[location.X, location.Y] == null && doesTileHaveProperty(location.X, location.Y, "Water", "Back") == null && map.GetLayer("Front").Tiles[location.X, location.Y] == null)
			{
				if (map.GetLayer("AlwaysFront") != null)
				{
					return map.GetLayer("AlwaysFront").Tiles[location.X, location.Y] == null;
				}
				return true;
			}
			return false;
		}

		public bool isTileLocationOpenIgnoreFrontLayers(Location location)
		{
			if (map.GetLayer("Buildings").Tiles[location.X, location.Y] == null)
			{
				return doesTileHaveProperty(location.X, location.Y, "Water", "Back") == null;
			}
			return false;
		}

		public void spawnWeedsAndStones(int numDebris = -1, bool weedsOnly = false, bool spawnFromOldWeeds = true)
		{
			if ((this is Farm || this is IslandWest) && Game1.getFarm().isBuildingConstructed("Gold Clock"))
			{
				return;
			}
			bool notified_destruction = false;
			if (this is Beach || GetSeasonForLocation().Equals("winter") || this is Desert)
			{
				return;
			}
			int numWeedsAndStones = (numDebris != -1) ? numDebris : ((Game1.random.NextDouble() < 0.95) ? ((Game1.random.NextDouble() < 0.25) ? Game1.random.Next(10, 21) : Game1.random.Next(5, 11)) : 0);
			if (Game1.IsRainingHere(this))
			{
				numWeedsAndStones *= 2;
			}
			if (Game1.dayOfMonth == 1)
			{
				numWeedsAndStones *= 5;
			}
			if (objects.Count() <= 0 && spawnFromOldWeeds)
			{
				return;
			}
			if (!(this is Farm))
			{
				numWeedsAndStones /= 2;
			}
			for (int i = 0; i < numWeedsAndStones; i++)
			{
				Vector2 v = spawnFromOldWeeds ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : new Vector2(Game1.random.Next(map.Layers[0].LayerWidth), Game1.random.Next(map.Layers[0].LayerHeight));
				if (!spawnFromOldWeeds && this is IslandWest)
				{
					v = new Vector2(Game1.random.Next(57, 97), Game1.random.Next(44, 68));
				}
				while (spawnFromOldWeeds && v.Equals(Vector2.Zero))
				{
					v = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
				}
				KeyValuePair<Vector2, Object> o = new KeyValuePair<Vector2, Object>(Vector2.Zero, null);
				if (spawnFromOldWeeds)
				{
					o = objects.Pairs.ElementAt(Game1.random.Next(objects.Count()));
				}
				Vector2 baseVect = spawnFromOldWeeds ? o.Key : Vector2.Zero;
				if ((this is Mountain && v.X + baseVect.X > 100f) || this is IslandNorth)
				{
					continue;
				}
				bool num = this is Farm || this is IslandWest;
				int checked_tile_x = (int)(v.X + baseVect.X);
				int checked_tile_y = (int)(v.Y + baseVect.Y);
				Vector2 checked_tile = v + baseVect;
				bool is_valid_tile = false;
				bool tile_is_diggable = doesTileHaveProperty(checked_tile_x, checked_tile_y, "Diggable", "Back") != null;
				if (num == tile_is_diggable && doesTileHaveProperty(checked_tile_x, checked_tile_y, "NoSpawn", "Back") == null && doesTileHaveProperty(checked_tile_x, checked_tile_y, "Type", "Back") != "Wood")
				{
					bool is_tile_clear = false;
					if (isTileLocationTotallyClearAndPlaceable(checked_tile))
					{
						is_tile_clear = true;
					}
					else if (spawnFromOldWeeds)
					{
						if (objects.ContainsKey(checked_tile))
						{
							Object tile_object = objects[checked_tile];
							if (!tile_object.bigCraftable.Value || (tile_object.ParentSheetIndex != 105 && tile_object.ParentSheetIndex != 264))
							{
								is_tile_clear = true;
							}
						}
						if (!is_tile_clear && terrainFeatures.ContainsKey(checked_tile))
						{
							TerrainFeature terrain_feature = terrainFeatures[checked_tile];
							if (terrain_feature is HoeDirt || terrain_feature is Flooring)
							{
								is_tile_clear = true;
							}
						}
					}
					if (is_tile_clear)
					{
						if (spawnFromOldWeeds)
						{
							is_valid_tile = true;
						}
						else if (!objects.ContainsKey(checked_tile))
						{
							is_valid_tile = true;
						}
					}
				}
				if (!is_valid_tile)
				{
					continue;
				}
				int whatToAdd = -1;
				if (this is Desert)
				{
					whatToAdd = 750;
				}
				else
				{
					if (Game1.random.NextDouble() < 0.5 && !weedsOnly && (!spawnFromOldWeeds || o.Value.Name.Equals("Stone") || o.Value.Name.Contains("Twig")))
					{
						whatToAdd = ((!(Game1.random.NextDouble() < 0.5)) ? ((Game1.random.NextDouble() < 0.5) ? 343 : 450) : ((Game1.random.NextDouble() < 0.5) ? 294 : 295));
					}
					else if (!spawnFromOldWeeds || o.Value.Name.Contains("Weed"))
					{
						whatToAdd = getWeedForSeason(Game1.random, GetSeasonForLocation());
					}
					if (this is Farm && !spawnFromOldWeeds && Game1.random.NextDouble() < 0.05)
					{
						terrainFeatures.Add(checked_tile, new Tree(Game1.random.Next(3) + 1, Game1.random.Next(3)));
						continue;
					}
				}
				if (whatToAdd == -1)
				{
					continue;
				}
				bool destroyed = false;
				if (objects.ContainsKey(v + baseVect))
				{
					Object removed = objects[v + baseVect];
					if (removed is Fence || removed is Chest)
					{
						continue;
					}
					if (removed.name != null && !removed.Name.Contains("Weed") && !removed.Name.Equals("Stone") && !removed.name.Contains("Twig") && removed.name.Length > 0)
					{
						destroyed = true;
						Game1.debugOutput = removed.Name + " was destroyed";
					}
					objects.Remove(v + baseVect);
				}
				if (terrainFeatures.ContainsKey(v + baseVect))
				{
					try
					{
						destroyed = (terrainFeatures[v + baseVect] is HoeDirt || terrainFeatures[v + baseVect] is Flooring);
					}
					catch (Exception)
					{
					}
					if (!destroyed)
					{
						break;
					}
					terrainFeatures.Remove(v + baseVect);
				}
				if (destroyed && this is Farm && Game1.stats.DaysPlayed > 1 && !notified_destruction)
				{
					notified_destruction = true;
					Game1.multiplayer.broadcastGlobalMessage("Strings\\Locations:Farm_WeedsDestruction", false);
				}
				objects.Add(v + baseVect, new Object(v + baseVect, whatToAdd, 1));
			}
		}

		public virtual void removeEverythingExceptCharactersFromThisTile(int x, int y)
		{
			Vector2 v = new Vector2(x, y);
			if (terrainFeatures.ContainsKey(v))
			{
				terrainFeatures.Remove(v);
			}
			if (objects.ContainsKey(v))
			{
				objects.Remove(v);
			}
		}

		public virtual string getFootstepSoundReplacement(string footstep)
		{
			return footstep;
		}

		public virtual void removeEverythingFromThisTile(int x, int y)
		{
			for (int i = resourceClumps.Count - 1; i >= 0; i--)
			{
				if (resourceClumps[i].tile.X == (float)x && resourceClumps[i].tile.Y == (float)y)
				{
					resourceClumps.RemoveAt(i);
				}
			}
			Vector2 v = new Vector2(x, y);
			if (terrainFeatures.ContainsKey(v))
			{
				terrainFeatures.Remove(v);
			}
			if (objects.ContainsKey(v))
			{
				objects.Remove(v);
			}
			for (int j = characters.Count - 1; j >= 0; j--)
			{
				if (characters[j].getTileLocation().Equals(v) && characters[j] is Monster)
				{
					characters.RemoveAt(j);
				}
			}
		}

		public virtual Dictionary<string, string> GetLocationEvents()
		{
			string nameToTry = name;
			if (uniqueName != null && uniqueName.Value != null && uniqueName.Value.Equals(Game1.player.homeLocation.Value))
			{
				nameToTry = "FarmHouse";
			}
			Dictionary<string, string> events2 = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + nameToTry);
			if (Name == "IslandFarmHouse")
			{
				events2 = new Dictionary<string, string>(events2);
				Dictionary<string, string> farmhouse_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\FarmHouse");
				{
					foreach (string farmhouse_event_key in farmhouse_events.Keys)
					{
						string event_string5 = farmhouse_events[farmhouse_event_key];
						if (farmhouse_event_key.StartsWith("558291/") || farmhouse_event_key.StartsWith("558292/"))
						{
							events2[farmhouse_event_key] = event_string5;
						}
					}
					return events2;
				}
			}
			if (Name == "Trailer_Big")
			{
				events2 = new Dictionary<string, string>(events2);
				Dictionary<string, string> trailer_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\Trailer");
				if (trailer_events != null)
				{
					foreach (string trailer_event_key in trailer_events.Keys)
					{
						string event_string4 = trailer_events[trailer_event_key];
						if (!(nameToTry == "Trailer_Big") || !events2.ContainsKey(trailer_event_key))
						{
							if (trailer_event_key.StartsWith("36/"))
							{
								event_string4 = event_string4.Replace("/farmer -30 30 0", "/farmer 12 19 0");
								event_string4 = event_string4.Replace("/playSound doorClose/warp farmer 12 9", "/move farmer 0 -10 0");
							}
							else if (trailer_event_key.StartsWith("35/"))
							{
								event_string4 = event_string4.Replace("/farmer -30 30 0", "/farmer 12 19 0");
								event_string4 = event_string4.Replace("/warp farmer 12 9/playSound doorClose", "/move farmer 0 -10 0");
								event_string4 = event_string4.Replace("/warp farmer -40 -40/playSound doorClose", "/move farmer 0 10 0/warp farmer -40 -40");
							}
							events2[trailer_event_key] = event_string4;
						}
					}
					return events2;
				}
			}
			return events2;
		}

		public virtual void checkForEvents()
		{
			if (Game1.killScreen && !Game1.eventUp)
			{
				if (name.Equals("Mine"))
				{
					string rescuer3 = "Linus";
					string uniquemessage3 = "Data\\ExtraDialogue:Mines_PlayerKilled_Linus";
					switch (Game1.random.Next(7))
					{
					case 0:
						rescuer3 = "Robin";
						uniquemessage3 = "Data\\ExtraDialogue:Mines_PlayerKilled_Robin";
						break;
					case 1:
						rescuer3 = "Clint";
						uniquemessage3 = "Data\\ExtraDialogue:Mines_PlayerKilled_Clint";
						break;
					case 2:
						rescuer3 = "Maru";
						uniquemessage3 = ((Game1.player.spouse != null && Game1.player.spouse.Equals("Maru")) ? "Data\\ExtraDialogue:Mines_PlayerKilled_Maru_Spouse" : "Data\\ExtraDialogue:Mines_PlayerKilled_Maru_NotSpouse");
						break;
					default:
						rescuer3 = "Linus";
						uniquemessage3 = "Data\\ExtraDialogue:Mines_PlayerKilled_Linus";
						break;
					}
					if (Game1.random.NextDouble() < 0.1 && Game1.player.spouse != null && !Game1.player.isEngaged() && Game1.player.spouse.Length > 1)
					{
						rescuer3 = Game1.player.spouse;
						uniquemessage3 = (Game1.player.IsMale ? "Data\\ExtraDialogue:Mines_PlayerKilled_Spouse_PlayerMale" : "Data\\ExtraDialogue:Mines_PlayerKilled_Spouse_PlayerFemale");
					}
					currentEvent = new Event(Game1.content.LoadString("Data\\Events\\Mine:PlayerKilled", rescuer3, uniquemessage3, Game1.player.Name));
				}
				else if (this is IslandLocation)
				{
					string rescuer = "Willy";
					string uniquemessage = "Data\\ExtraDialogue:Island_willy_rescue";
					if (Game1.player.friendshipData.ContainsKey("Leo") && Game1.random.NextDouble() < 0.5)
					{
						rescuer = "Leo";
						uniquemessage = "Data\\ExtraDialogue:Island_leo_rescue";
					}
					currentEvent = new Event(Game1.content.LoadString("Data\\Events\\IslandSouth:PlayerKilled", rescuer, uniquemessage, Game1.player.Name));
				}
				else if (name.Equals("Hospital"))
				{
					currentEvent = new Event(Game1.content.LoadString("Data\\Events\\Hospital:PlayerKilled", Game1.player.Name));
				}
				Game1.eventUp = true;
				Game1.killScreen = false;
				Game1.player.health = 10;
			}
			else if (!Game1.eventUp && Game1.weddingsToday.Count > 0 && (Game1.CurrentEvent == null || Game1.CurrentEvent.id != -2) && Game1.currentLocation != null && Game1.currentLocation.Name != "Temp")
			{
				currentEvent = Game1.getAvailableWeddingEvent();
				if (currentEvent != null)
				{
					startEvent(currentEvent);
				}
			}
			else
			{
				if (Game1.eventUp || Game1.farmEvent != null)
				{
					return;
				}
				string key = Game1.currentSeason + Game1.dayOfMonth;
				try
				{
					Event festival = new Event();
					if (festival.tryToLoadFestival(key))
					{
						currentEvent = festival;
					}
				}
				catch (Exception)
				{
				}
				if (!Game1.eventUp && currentEvent == null && Game1.farmEvent == null)
				{
					Dictionary<string, string> events2 = null;
					try
					{
						_ = (string)name;
						events2 = GetLocationEvents();
					}
					catch (Exception)
					{
						return;
					}
					if (events2 != null)
					{
						string[] keys = events2.Keys.ToArray();
						for (int j = 0; j < keys.Length; j++)
						{
							int precondition = checkEventPrecondition(keys[j]);
							if (precondition != -1)
							{
								currentEvent = new Event(events2[keys[j]], precondition);
								break;
							}
						}
						if (currentEvent == null && name.Equals("Farm") && Game1.IsMasterGame && !Game1.player.mailReceived.Contains("rejectedPet") && Game1.stats.DaysPlayed >= 20 && !Game1.player.hasPet())
						{
							for (int i = 0; i < keys.Length; i++)
							{
								if ((keys[i].Contains("dog") && !Game1.player.catPerson) || (keys[i].Contains("cat") && Game1.player.catPerson))
								{
									currentEvent = new Event(events2[keys[i]]);
									Game1.player.eventsSeen.Add(Convert.ToInt32(keys[i].Split('/')[0]));
									break;
								}
							}
						}
					}
				}
				if (currentEvent != null)
				{
					startEvent(currentEvent);
				}
			}
		}

		public Event findEventById(int id, Farmer farmerActor = null)
		{
			if (id == -2)
			{
				long? spouseFarmer = Game1.player.team.GetSpouse(farmerActor.UniqueMultiplayerID);
				if (farmerActor == null || !spouseFarmer.HasValue)
				{
					return Utility.getWeddingEvent(farmerActor);
				}
				if (Game1.otherFarmers.ContainsKey(spouseFarmer.Value))
				{
					Farmer spouse = Game1.otherFarmers[spouseFarmer.Value];
					return Utility.getPlayerWeddingEvent(farmerActor, spouse);
				}
			}
			Dictionary<string, string> events2 = null;
			try
			{
				events2 = GetLocationEvents();
			}
			catch (Exception)
			{
				return null;
			}
			foreach (KeyValuePair<string, string> pair in events2)
			{
				if (pair.Key.Split('/')[0] == id.ToString())
				{
					return new Event(pair.Value, id, farmerActor);
				}
			}
			return null;
		}

		public virtual void startEvent(Event evt)
		{
			if (!Game1.eventUp && !Game1.eventOver)
			{
				currentEvent = evt;
				if (evt.exitLocation == null)
				{
					evt.exitLocation = Game1.getLocationRequest(isStructure ? uniqueName.Value : Name, isStructure);
				}
				if (Game1.player.mount != null)
				{
					Game1.player.mount.currentLocation = this;
					Game1.player.mount.dismount();
				}
				foreach (NPC character in characters)
				{
					character.clearTextAboveHead();
				}
				Game1.eventUp = true;
				Game1.displayHUD = false;
				Game1.player.CanMove = false;
				Game1.player.showNotCarrying();
				if (critters != null)
				{
					critters.Clear();
				}
			}
		}

		public virtual void drawBackground(SpriteBatch b)
		{
		}

		public virtual void drawWater(SpriteBatch b)
		{
			if (currentEvent != null)
			{
				currentEvent.drawUnderWater(b);
			}
			if (waterTiles == null)
			{
				return;
			}
			for (int y = Math.Max(0, Game1.viewport.Y / 64 - 1); y < Math.Min(map.Layers[0].LayerHeight, (Game1.viewport.Y + Game1.viewport.Height) / 64 + 2); y++)
			{
				for (int x = Math.Max(0, Game1.viewport.X / 64 - 1); x < Math.Min(map.Layers[0].LayerWidth, (Game1.viewport.X + Game1.viewport.Width) / 64 + 1); x++)
				{
					if (waterTiles[x, y])
					{
						drawWaterTile(b, x, y);
					}
				}
			}
		}

		public virtual void drawWaterTile(SpriteBatch b, int x, int y)
		{
			drawWaterTile(b, x, y, waterColor);
		}

		public void drawWaterTile(SpriteBatch b, int x, int y, Color color)
		{
			bool num = y == map.Layers[0].LayerHeight - 1 || !waterTiles[x, y + 1];
			bool topY = y == 0 || !waterTiles[x, y - 1];
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (int)((!topY) ? waterPosition : 0f))), new Microsoft.Xna.Framework.Rectangle(waterAnimationIndex * 64, 2064 + (((x + y) % 2 != 0) ? ((!waterTileFlip) ? 128 : 0) : (waterTileFlip ? 128 : 0)) + (topY ? ((int)waterPosition) : 0), 64, 64 + (topY ? ((int)(0f - waterPosition)) : 0)), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
			if (num)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int)waterPosition)), new Microsoft.Xna.Framework.Rectangle(waterAnimationIndex * 64, 2064 + (((x + (y + 1)) % 2 != 0) ? ((!waterTileFlip) ? 128 : 0) : (waterTileFlip ? 128 : 0)), 64, 64 - (int)(64f - waterPosition) - 1), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
			}
		}

		public virtual void drawFloorDecorations(SpriteBatch b)
		{
			if (!Game1.isFestival())
			{
				Vector2 tile = default(Vector2);
				for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
				{
					for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
					{
						tile.X = x;
						tile.Y = y;
						if (terrainFeatures.TryGetValue(tile, out TerrainFeature feat) && feat is Flooring)
						{
							feat.draw(b, tile);
						}
					}
				}
			}
			if (!Game1.eventUp || this is Farm || this is FarmHouse)
			{
				Furniture.isDrawingLocationFurniture = true;
				foreach (Furniture f in furniture)
				{
					if (f.furniture_type.Value == 12)
					{
						f.draw(b, -1, -1);
					}
				}
				Furniture.isDrawingLocationFurniture = false;
			}
		}

		public TemporaryAnimatedSprite getTemporarySpriteByID(int id)
		{
			for (int i = 0; i < temporarySprites.Count; i++)
			{
				if (temporarySprites[i].id == (float)id)
				{
					return temporarySprites[i];
				}
			}
			return null;
		}

		protected void drawDebris(SpriteBatch b)
		{
			int counter = 0;
			foreach (Debris d in debris)
			{
				counter++;
				if (d.item != null)
				{
					if (d.item is Object && (bool)(d.item as Object).bigCraftable)
					{
						d.item.drawInMenu(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[0].position + new Vector2(32f, 32f))), 1.6f, 1f, ((float)(d.chunkFinalYLevel + 64 + 8) + d.Chunks[0].position.X / 10000f) / 10000f, StackDrawType.Hide, Color.White, drawShadow: true);
					}
					else
					{
						d.item.drawInMenu(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[0].position + new Vector2(32f, 32f))), 0.8f + (float)d.itemQuality * 0.1f, 1f, ((float)(d.chunkFinalYLevel + 64 + 8) + d.Chunks[0].position.X / 10000f) / 10000f, StackDrawType.Hide, Color.White, drawShadow: true);
					}
				}
				else if ((Debris.DebrisType)d.debrisType == Debris.DebrisType.LETTERS)
				{
					Game1.drawWithBorder(d.debrisMessage, Color.Black, d.nonSpriteChunkColor, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[0].position)), d.Chunks[0].rotation, d.Chunks[0].scale, (d.Chunks[0].position.Y + 64f) / 10000f);
				}
				else if ((Debris.DebrisType)d.debrisType == Debris.DebrisType.NUMBERS)
				{
					NumberSprite.draw(d.chunkType, b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(new Vector2(d.Chunks[0].position.X, (float)d.chunkFinalYLevel - ((float)d.chunkFinalYLevel - d.Chunks[0].position.Y)))), d.nonSpriteChunkColor, d.Chunks[0].scale * 0.75f, 0.98f + 0.0001f * (float)counter, d.Chunks[0].alpha, -1 * (int)((float)d.chunkFinalYLevel - d.Chunks[0].position.Y) / 2);
				}
				else if ((Debris.DebrisType)d.debrisType == Debris.DebrisType.SPRITECHUNKS)
				{
					for (int j = 0; j < d.Chunks.Count; j++)
					{
						b.Draw(d.spriteChunkSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[j].position)), new Microsoft.Xna.Framework.Rectangle(d.Chunks[j].xSpriteSheet, d.Chunks[j].ySpriteSheet, Math.Min(d.sizeOfSourceRectSquares, d.spriteChunkSheet.Bounds.Width), Math.Min(d.sizeOfSourceRectSquares, d.spriteChunkSheet.Bounds.Height)), d.nonSpriteChunkColor.Value * d.Chunks[j].alpha, d.Chunks[j].rotation, new Vector2((int)d.sizeOfSourceRectSquares / 2, (int)d.sizeOfSourceRectSquares / 2), d.Chunks[j].scale, SpriteEffects.None, ((float)(d.chunkFinalYLevel + 16) + d.Chunks[j].position.X / 10000f) / 10000f);
					}
				}
				else if ((Debris.DebrisType)d.debrisType == Debris.DebrisType.SQUARES)
				{
					for (int l = 0; l < d.Chunks.Count; l++)
					{
						b.Draw(Game1.littleEffect, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[l].position)), new Microsoft.Xna.Framework.Rectangle(0, 0, 4, 4), d.nonSpriteChunkColor, 0f, Vector2.Zero, 1f + (float)d.Chunks[l].yVelocity / 2f, SpriteEffects.None, (d.Chunks[l].position.Y + 64f) / 10000f);
					}
				}
				else if ((Debris.DebrisType)d.debrisType != 0)
				{
					for (int k = 0; k < d.Chunks.Count; k++)
					{
						if (d.Chunks[k].debrisType <= 0)
						{
							b.Draw(Game1.bigCraftableSpriteSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[k].position + new Vector2(32f, 64f))), Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32, -d.Chunks[k].debrisType), Color.White, 0f, new Vector2(8f, 32f), 3.2f, SpriteEffects.None, ((float)(d.chunkFinalYLevel + 48) + d.Chunks[k].position.X / 10000f) / 10000f);
							b.Draw(Game1.shadowTexture, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, new Vector2(d.Chunks[k].position.X + 25.6f, (d.chunksMoveTowardPlayer ? (d.Chunks[k].position.Y + 8f) : ((float)d.chunkFinalYLevel)) + 32f))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), Math.Min(3f, 3f - (d.chunksMoveTowardPlayer ? 0f : (((float)d.chunkFinalYLevel - d.Chunks[k].position.Y) / 128f))), SpriteEffects.None, (float)d.chunkFinalYLevel / 10000f);
						}
						else
						{
							b.Draw(Game1.objectSpriteSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[k].position)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, d.Chunks[k].debrisType, 16, 16), Color.White, 0f, Vector2.Zero, ((Debris.DebrisType)d.debrisType == Debris.DebrisType.RESOURCE || (bool)d.floppingFish) ? 4f : (4f * (0.8f + (float)d.itemQuality * 0.1f)), ((bool)d.floppingFish && d.Chunks[k].bounces % 2 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((float)(d.chunkFinalYLevel + 32) + d.Chunks[k].position.X / 10000f) / 10000f);
							b.Draw(Game1.shadowTexture, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, new Vector2(d.Chunks[k].position.X + 25.6f, (d.chunksMoveTowardPlayer ? (d.Chunks[k].position.Y + 8f) : ((float)d.chunkFinalYLevel)) + 32f + (float)(12 * d.itemQuality)))), Game1.shadowTexture.Bounds, Color.White * 0.75f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), Math.Min(3f, 3f - (d.chunksMoveTowardPlayer ? 0f : (((float)d.chunkFinalYLevel - d.Chunks[k].position.Y) / 96f))), SpriteEffects.None, (float)d.chunkFinalYLevel / 10000f);
						}
					}
				}
				else
				{
					for (int i = 0; i < d.Chunks.Count; i++)
					{
						b.Draw(Game1.debrisSpriteSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[i].position)), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, d.Chunks[i].debrisType, 16, 16), d.chunksColor, 0f, Vector2.Zero, 4f * (float)d.scale, SpriteEffects.None, (d.Chunks[i].position.Y + 128f + d.Chunks[i].position.X / 10000f) / 10000f);
					}
				}
			}
		}

		public virtual bool shouldHideCharacters()
		{
			return false;
		}

		protected virtual void drawCharacters(SpriteBatch b)
		{
			if (shouldHideCharacters() || (Game1.eventUp && (Game1.CurrentEvent == null || !Game1.CurrentEvent.showWorldCharacters)))
			{
				return;
			}
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i] != null)
				{
					characters[i].draw(b);
				}
			}
		}

		protected virtual void drawFarmers(SpriteBatch b)
		{
			if (!shouldHideCharacters() && Game1.currentMinigame == null)
			{
				if (currentEvent == null || currentEvent.isFestival || currentEvent.farmerActors.Count == 0)
				{
					foreach (Farmer farmer in farmers)
					{
						if (!Game1.multiplayer.isDisconnecting(farmer.UniqueMultiplayerID))
						{
							farmer.draw(b);
						}
					}
				}
				else
				{
					currentEvent.drawFarmers(b);
				}
			}
		}

		public virtual void DrawFarmerUsernames(SpriteBatch b)
		{
			if (!shouldHideCharacters() && Game1.currentMinigame == null && (currentEvent == null || currentEvent.isFestival || currentEvent.farmerActors.Count == 0))
			{
				foreach (Farmer farmer in farmers)
				{
					if (!Game1.multiplayer.isDisconnecting(farmer.UniqueMultiplayerID))
					{
						farmer.DrawUsername(b);
					}
				}
			}
		}

		public virtual void draw(SpriteBatch b)
		{
			foreach (MapSeat mapSeat in mapSeats)
			{
				mapSeat.Draw(b);
			}
			foreach (ResourceClump r in resourceClumps)
			{
				r.draw(b, r.tile);
			}
			List<Farmer> current_location_farmers = new List<Farmer>();
			foreach (Farmer farmer2 in farmers)
			{
				farmer2.drawLayerDisambiguator = 0f;
				current_location_farmers.Add(farmer2);
			}
			if (current_location_farmers.Contains(Game1.player))
			{
				current_location_farmers.Remove(Game1.player);
				current_location_farmers.Insert(0, Game1.player);
			}
			float disambiguator_amount = 0.0001f;
			for (int k = 0; k < current_location_farmers.Count; k++)
			{
				for (int i = k + 1; i < current_location_farmers.Count; i++)
				{
					Farmer farmer = current_location_farmers[k];
					Farmer other_farmer = current_location_farmers[i];
					if (!other_farmer.IsSitting() && Math.Abs(farmer.getDrawLayer() - other_farmer.getDrawLayer()) < disambiguator_amount && Math.Abs(farmer.position.X - other_farmer.position.X) < 64f)
					{
						other_farmer.drawLayerDisambiguator += farmer.getDrawLayer() - disambiguator_amount - other_farmer.getDrawLayer();
					}
				}
			}
			drawCharacters(b);
			drawFarmers(b);
			if (critters != null && Game1.farmEvent == null)
			{
				for (int j = 0; j < critters.Count; j++)
				{
					critters[j].draw(b);
				}
			}
			drawDebris(b);
			if (!Game1.eventUp || (currentEvent != null && currentEvent.showGroundObjects))
			{
				Vector2 tile = default(Vector2);
				for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 3; y++)
				{
					for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; x++)
					{
						tile.X = x;
						tile.Y = y;
						if (objects.TryGetValue(tile, out Object o))
						{
							o.draw(b, (int)tile.X, (int)tile.Y);
						}
					}
				}
			}
			foreach (TemporaryAnimatedSprite s in TemporarySprites)
			{
				if (!s.drawAboveAlwaysFront)
				{
					s.draw(b);
				}
			}
			interiorDoors.Draw(b);
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					largeTerrainFeature.draw(b);
				}
			}
			if (fishSplashAnimation != null)
			{
				fishSplashAnimation.draw(b);
			}
			if (orePanAnimation != null)
			{
				orePanAnimation.draw(b);
			}
			if (!Game1.eventUp || this is Farm || this is FarmHouse)
			{
				Furniture.isDrawingLocationFurniture = true;
				foreach (Furniture f in furniture)
				{
					if (f.furniture_type.Value != 12)
					{
						f.draw(b, -1, -1);
					}
				}
				Furniture.isDrawingLocationFurniture = false;
			}
			if (showDropboxIndicator && !Game1.eventUp)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(dropBoxIndicatorLocation.X, dropBoxIndicatorLocation.Y + yOffset)), new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
			}
		}

		public virtual void drawAboveFrontLayer(SpriteBatch b)
		{
			if (!Game1.isFestival())
			{
				Vector2 tile = default(Vector2);
				for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
				{
					for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
					{
						tile.X = x;
						tile.Y = y;
						if (terrainFeatures.TryGetValue(tile, out TerrainFeature feat) && !(feat is Flooring))
						{
							feat.draw(b, tile);
						}
					}
				}
			}
			if (!(this is MineShaft))
			{
				foreach (NPC character in characters)
				{
					(character as Monster)?.drawAboveAllLayers(b);
				}
			}
			if (lightGlows.Count > 0)
			{
				drawLightGlows(b);
			}
			DrawFarmerUsernames(b);
		}

		public virtual void drawLightGlows(SpriteBatch b)
		{
			foreach (Vector2 v in lightGlows)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, v), new Microsoft.Xna.Framework.Rectangle(21, 1695, 41, 67), Color.White, 0f, new Vector2(19f, 22f), 4f, SpriteEffects.None, 1f);
			}
		}

		public Object tryToCreateUnseenSecretNote(Farmer who)
		{
			bool journal = GetLocationContext() == LocationContext.Island;
			if (who != null && ((who.hasMagnifyingGlass && !journal) | journal))
			{
				Dictionary<int, string> secretNoteData = Game1.content.Load<Dictionary<int, string>>("Data\\SecretNotes");
				int totalNotes = 0;
				foreach (int note2 in secretNoteData.Keys)
				{
					if (journal)
					{
						if (note2 >= JOURNAL_INDEX)
						{
							totalNotes++;
						}
					}
					else if (note2 < JOURNAL_INDEX)
					{
						totalNotes++;
					}
				}
				secretNotesSeen.Clear();
				foreach (int note in who.secretNotesSeen)
				{
					if (note < JOURNAL_INDEX)
					{
						secretNotesSeen.Add(note);
					}
					else
					{
						journalsSeen.Add(note);
					}
				}
				int totalNotesSeen2 = 0;
				totalNotesSeen2 = ((!journal) ? secretNotesSeen.Count : journalsSeen.Count);
				if (totalNotesSeen2 == totalNotes)
				{
					return null;
				}
				float fractionOfNotesRemaining = (float)(totalNotes - 1 - totalNotesSeen2) / (float)Math.Max(1, totalNotes - 1);
				float chanceForNewNote = LAST_SECRET_NOTE_CHANCE + (FIRST_SECRET_NOTE_CHANCE - LAST_SECRET_NOTE_CHANCE) * fractionOfNotesRemaining;
				if (Game1.random.NextDouble() < (double)chanceForNewNote)
				{
					unseenJournals.Clear();
					unseenSecretNotes.Clear();
					foreach (int data in secretNoteData.Keys)
					{
						if (data < JOURNAL_INDEX && !secretNotesSeen.Contains(data) && !who.hasItemInInventoryNamed("Secret Note #" + data) && (data != 10 || who.mailReceived.Contains("QiChallengeComplete")))
						{
							unseenSecretNotes.Add(data);
						}
						else if (data >= JOURNAL_INDEX && !journalsSeen.Contains(data) && !who.hasItemInInventoryNamed("Journal Scrap #" + (data - JOURNAL_INDEX)))
						{
							unseenJournals.Add(data);
						}
					}
					if (!journal && unseenSecretNotes.Count > 0)
					{
						int whichNote2 = unseenSecretNotes[Game1.random.Next(unseenSecretNotes.Count)];
						Object @object = new Object(79, 1);
						@object.name = @object.name + " #" + whichNote2;
						return @object;
					}
					if (journal && unseenJournals.Count > 0)
					{
						int whichNote = unseenJournals.First();
						Object object2 = new Object(842, 1);
						object2.name = object2.name + " #" + (whichNote - JOURNAL_INDEX);
						return object2;
					}
				}
			}
			return null;
		}

		public virtual bool performToolAction(Tool t, int tileX, int tileY)
		{
			for (int i = resourceClumps.Count - 1; i >= 0; i--)
			{
				if (resourceClumps[i] != null && resourceClumps[i].getBoundingBox(resourceClumps[i].tile).Contains(tileX * 64, tileY * 64) && resourceClumps[i].performToolAction(t, 1, resourceClumps[i].tile, this))
				{
					resourceClumps.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public virtual void seasonUpdate(string season, bool onLoad = false)
		{
			for (int i = terrainFeatures.Count() - 1; i >= 0; i--)
			{
				if (terrainFeatures.Values.ElementAt(i).seasonUpdate(onLoad))
				{
					terrainFeatures.Remove(terrainFeatures.Keys.ElementAt(i));
				}
			}
			if (largeTerrainFeatures != null)
			{
				for (int j = largeTerrainFeatures.Count - 1; j >= 0; j--)
				{
					if (largeTerrainFeatures.ElementAt(j).seasonUpdate(onLoad))
					{
						largeTerrainFeatures.Remove(largeTerrainFeatures.ElementAt(j));
					}
				}
			}
			foreach (NPC k in getCharacters())
			{
				if (!k.IsMonster)
				{
					k.resetSeasonalDialogue();
				}
			}
			if (IsOutdoors && !onLoad)
			{
				for (int l = objects.Count() - 1; l >= 0; l--)
				{
					if (objects.Pairs.ElementAt(l).Value.IsSpawnedObject && !objects.Pairs.ElementAt(l).Value.Name.Equals("Stone"))
					{
						objects.Remove(objects.Pairs.ElementAt(l).Key);
					}
					else if ((int)objects.Pairs.ElementAt(l).Value.parentSheetIndex == 590 && doesTileHavePropertyNoNull((int)objects.Pairs.ElementAt(l).Key.X, (int)objects.Pairs.ElementAt(l).Key.Y, "Diggable", "Back") == "")
					{
						objects.Remove(objects.Pairs.ElementAt(l).Key);
					}
				}
				numberOfSpawnedObjectsOnMap = 0;
			}
			switch (season.ToLower())
			{
			case "spring":
				waterColor.Value = new Color(120, 200, 255) * 0.5f;
				break;
			case "summer":
				waterColor.Value = new Color(60, 240, 255) * 0.5f;
				break;
			case "fall":
				waterColor.Value = new Color(255, 130, 200) * 0.5f;
				break;
			case "winter":
				waterColor.Value = new Color(130, 80, 255) * 0.5f;
				break;
			}
			if (!onLoad && season == "spring" && Game1.stats.daysPlayed > 1 && !(this is Farm))
			{
				loadWeeds();
			}
		}

		public virtual void updateSeasonalTileSheets(Map map = null)
		{
			if (map == null)
			{
				map = Map;
			}
			if (this is Summit || (IsOutdoors && !Name.Equals("Desert")))
			{
				map.DisposeTileSheets(Game1.mapDisplayDevice);
				for (int i = 0; i < map.TileSheets.Count; i++)
				{
					map.TileSheets[i].ImageSource = GetSeasonalTilesheetName(map.TileSheets[i].ImageSource, GetSeasonForLocation());
				}
				map.LoadTileSheets(Game1.mapDisplayDevice);
			}
		}

		public static string GetSeasonalTilesheetName(string sheet_path, string current_season)
		{
			string file_name = Path.GetFileName(sheet_path);
			if (file_name.StartsWith("spring_") || file_name.StartsWith("summer_") || file_name.StartsWith("fall_") || file_name.StartsWith("winter_"))
			{
				sheet_path = Path.Combine(Path.GetDirectoryName(sheet_path), current_season + file_name.Substring(file_name.IndexOf('_')));
			}
			return sheet_path;
		}

		public virtual int checkEventPrecondition(string precondition)
		{
			string[] split = precondition.Split(ForwardSlash);
			if (!int.TryParse(split[0], out int eventId))
			{
				return -1;
			}
			if (Game1.player.eventsSeen.Contains(eventId))
			{
				return -1;
			}
			for (int l = 1; l < split.Length; l++)
			{
				if (split[l][0] == 'e')
				{
					if (checkEventsSeenPreconditions(split[l].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'h')
				{
					if (Game1.player.hasPet())
					{
						return -1;
					}
					if ((Game1.player.catPerson && !split[l].Split(' ')[1].ToString().ToLower().Equals("cat")) || (!Game1.player.catPerson && !split[l].Split(' ')[1].ToString().ToLower().Equals("dog")))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'H')
				{
					string[] hostSplit = split[l].Split(' ');
					if (hostSplit[0] == "H")
					{
						if (!Game1.IsMasterGame)
						{
							return -1;
						}
					}
					else if (hostSplit[0] == "Hn")
					{
						if (!Game1.MasterPlayer.mailReceived.Contains(hostSplit[1]))
						{
							return -1;
						}
					}
					else if (hostSplit[0] == "Hl" && Game1.MasterPlayer.mailReceived.Contains(hostSplit[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == '*')
				{
					string[] starSplit = split[l].Split(' ');
					if (starSplit[0] == "*")
					{
						if (!NetWorldState.checkAnywhereForWorldStateID(starSplit[1]))
						{
							return -1;
						}
					}
					else if (starSplit[0] == "*n")
					{
						if (!Game1.MasterPlayer.mailReceived.Contains(starSplit[1]) && !Game1.player.mailReceived.Contains(starSplit[1]))
						{
							return -1;
						}
					}
					else if (starSplit[0] == "*l" && (Game1.MasterPlayer.mailReceived.Contains(starSplit[1]) || Game1.player.mailReceived.Contains(starSplit[1])))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'm')
				{
					string[] moneySplit2 = split[l].Split(' ');
					if (Game1.player.totalMoneyEarned < Convert.ToInt32(moneySplit2[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'M')
				{
					string[] moneySplit = split[l].Split(' ');
					if (Game1.player.Money < Convert.ToInt32(moneySplit[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'c')
				{
					if (Game1.player.freeSpotsInInventory() < Convert.ToInt32(split[l].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'C')
				{
					if (!Game1.MasterPlayer.eventsSeen.Contains(191393) && !Game1.MasterPlayer.eventsSeen.Contains(502261) && !Game1.MasterPlayer.hasCompletedCommunityCenter())
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'X')
				{
					if (Game1.MasterPlayer.eventsSeen.Contains(191393) || Game1.MasterPlayer.eventsSeen.Contains(502261) || Game1.MasterPlayer.hasCompletedCommunityCenter())
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'D')
				{
					string npcName = split[l].Split(' ')[1];
					if (!Game1.player.friendshipData.ContainsKey(npcName))
					{
						return -1;
					}
					if (!Game1.player.friendshipData[npcName].IsDating())
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'j')
				{
					if (Game1.stats.DaysPlayed <= Convert.ToInt32(split[l].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'J')
				{
					if (!checkJojaCompletePrerequisite())
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'f')
				{
					if (!checkFriendshipPrecondition(split[l].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'F')
				{
					if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'r')
				{
					string[] rollSplit = split[l].Split(' ');
					if (Game1.random.NextDouble() > Convert.ToDouble(rollSplit[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 's')
				{
					if (!checkItemsPrecondition(split[l].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'S')
				{
					if (!Game1.player.secretNotesSeen.Contains(Convert.ToInt32(split[l].Split(' ')[1])))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'q')
				{
					if (!checkDialoguePrecondition(split[l].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'n')
				{
					if (!Game1.player.mailReceived.Contains(split[l].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'N')
				{
					if (Game1.netWorldState.Value.GoldenWalnutsFound.Value < Convert.ToInt32(split[l].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'l')
				{
					if (Game1.player.mailReceived.Contains(split[l].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'L')
				{
					if (!(this is FarmHouse) || (this as FarmHouse).upgradeLevel < 2)
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 't')
				{
					string[] timeSplit = split[l].Split(' ');
					if (Game1.timeOfDay < Convert.ToInt32(timeSplit[1]) || Game1.timeOfDay > Convert.ToInt32(timeSplit[2]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'w')
				{
					string[] weatherSplit = split[l].Split(' ');
					if ((weatherSplit[1].Equals("rainy") && !Game1.IsRainingHere(this)) || (weatherSplit[1].Equals("sunny") && Game1.IsRainingHere(this)))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'd')
				{
					if (split[l].Split(' ').Contains(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'o')
				{
					if (Game1.player.spouse != null && Game1.player.spouse.Equals(split[l].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'O')
				{
					if (Game1.player.spouse == null || !Game1.player.spouse.Equals(split[l].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'v')
				{
					if (Game1.getCharacterFromName(split[l].Split(' ')[1]).IsInvisible)
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'p')
				{
					string[] presentSplit = split[l].Split(' ');
					if (!isCharacterHere(presentSplit[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'z')
				{
					string[] seasonSplit = split[l].Split(' ');
					if (Game1.currentSeason.Equals(seasonSplit[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'B')
				{
					if (Utility.getHomeOfFarmer(Game1.player).GetSpouseBed() == null)
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'b')
				{
					string[] mineSplit = split[l].Split(' ');
					if (Game1.player.timesReachedMineBottom < Convert.ToInt32(mineSplit[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'y')
				{
					if (Game1.year < Convert.ToInt32(split[l].Split(' ')[1]) || (Convert.ToInt32(split[l].Split(' ')[1]) == 1 && Game1.year != 1))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'g')
				{
					if (!(Game1.player.IsMale ? "male" : "female").Equals(split[l].Split(' ')[1].ToLower()))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'i')
				{
					if (!Game1.player.hasItemInInventory(Convert.ToInt32(split[l].Split(' ')[1]), 1) && (Game1.player.ActiveObject == null || Game1.player.ActiveObject.ParentSheetIndex != Convert.ToInt32(split[l].Split(' ')[1])))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'k')
				{
					if (!checkEventsSeenPreconditions(split[l].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'a')
				{
					bool foundValidTile = false;
					string[] args = split[l].Split(' ');
					for (int k = 1; k < args.Length - 1; k += 2)
					{
						if (Game1.xLocationAfterWarp == Convert.ToInt32(args[k]) && Game1.yLocationAfterWarp == Convert.ToInt32(args[k + 1]))
						{
							foundValidTile = true;
						}
					}
					if (!foundValidTile)
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'A')
				{
					if (Game1.player.activeDialogueEvents.ContainsKey(split[l].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'x')
				{
					string[] dayssplit2 = split[l].Split(' ');
					if (dayssplit2.Length == 2)
					{
						Game1.addMailForTomorrow(dayssplit2[1]);
					}
					else
					{
						Game1.player.mailbox.Add(dayssplit2[1]);
					}
					Game1.player.eventsSeen.Add(Convert.ToInt32(split[0]));
					return -1;
				}
				if (split[l][0] == 'u')
				{
					bool foundDay = false;
					string[] dayssplit = split[l].Split(' ');
					for (int j = 1; j < dayssplit.Length; j++)
					{
						if (Game1.dayOfMonth == Convert.ToInt32(dayssplit[j]))
						{
							foundDay = true;
							break;
						}
					}
					if (!foundDay)
					{
						return -1;
					}
					continue;
				}
				if (split[l][0] == 'U')
				{
					int numDays = Convert.ToInt32(split[l].Split(' ')[1]);
					string season = Game1.currentSeason;
					int day = Game1.dayOfMonth;
					for (int i = 0; i < numDays; i++)
					{
						if (Utility.isFestivalDay(day, season))
						{
							return -1;
						}
						day++;
						if (day > 28)
						{
							day = 1;
							season = Utility.getSeasonNameFromNumber((Utility.getSeasonNumber(season) + 1) % 4).ToLower();
						}
					}
					continue;
				}
				return -1;
			}
			return eventId;
		}

		private bool isCharacterHere(string name)
		{
			foreach (NPC i in characters)
			{
				if (i.Name.Equals(name) && !i.IsInvisible)
				{
					return true;
				}
			}
			return false;
		}

		private bool checkJojaCompletePrerequisite()
		{
			return Utility.hasFinishedJojaRoute();
		}

		private bool checkEventsSeenPreconditions(string[] eventIDs)
		{
			for (int i = 1; i < eventIDs.Length; i++)
			{
				if (int.TryParse(eventIDs[i], out int _) && Game1.player.eventsSeen.Contains(Convert.ToInt32(eventIDs[i])))
				{
					return false;
				}
			}
			return true;
		}

		private bool checkFriendshipPrecondition(string[] friendshipString)
		{
			for (int i = 1; i < friendshipString.Length; i += 2)
			{
				if (!Game1.player.friendshipData.ContainsKey(friendshipString[i]) || Game1.player.friendshipData[friendshipString[i]].Points < Convert.ToInt32(friendshipString[i + 1]))
				{
					return false;
				}
			}
			return true;
		}

		private bool checkItemsPrecondition(string[] itemString)
		{
			for (int i = 1; i < itemString.Length; i += 2)
			{
				if (!Game1.player.basicShipped.ContainsKey(Convert.ToInt32(itemString[i])) || Game1.player.basicShipped[Convert.ToInt32(itemString[i])] < Convert.ToInt32(itemString[i + 1]))
				{
					return false;
				}
			}
			return true;
		}

		private bool checkDialoguePrecondition(string[] dialogueString)
		{
			for (int i = 1; i < dialogueString.Length; i += 2)
			{
				if (!Game1.player.DialogueQuestionsAnswered.Contains(Convert.ToInt32(dialogueString[i])))
				{
					return false;
				}
			}
			return true;
		}

		public virtual void updateWarps()
		{
			warps.Clear();
			map.Properties.TryGetValue("NPCWarp", out PropertyValue warpsUnparsed2);
			if (warpsUnparsed2 != null)
			{
				string[] warpInfo2 = warpsUnparsed2.ToString().Split(' ');
				for (int j = 0; j < warpInfo2.Length; j += 5)
				{
					warps.Add(new Warp(Convert.ToInt32(warpInfo2[j]), Convert.ToInt32(warpInfo2[j + 1]), warpInfo2[j + 2], Convert.ToInt32(warpInfo2[j + 3]), Convert.ToInt32(warpInfo2[j + 4]), flipFarmer: false, npcOnly: true));
				}
			}
			warpsUnparsed2 = null;
			map.Properties.TryGetValue("Warp", out warpsUnparsed2);
			if (warpsUnparsed2 != null)
			{
				string[] warpInfo = warpsUnparsed2.ToString().Split(' ');
				for (int i = 0; i < warpInfo.Length; i += 5)
				{
					warps.Add(new Warp(Convert.ToInt32(warpInfo[i]), Convert.ToInt32(warpInfo[i + 1]), warpInfo[i + 2], Convert.ToInt32(warpInfo[i + 3]), Convert.ToInt32(warpInfo[i + 4]), flipFarmer: false));
				}
			}
		}

		public void loadWeeds()
		{
			if (map == null)
			{
				return;
			}
			bool hasPathsLayer = false;
			foreach (Layer layer in map.Layers)
			{
				if (layer.Id.Equals("Paths"))
				{
					hasPathsLayer = true;
					break;
				}
			}
			if (!(((bool)isOutdoors || (bool)treatAsOutdoors) & hasPathsLayer))
			{
				return;
			}
			for (int x = 0; x < map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < map.Layers[0].LayerHeight; y++)
				{
					Tile t = map.GetLayer("Paths").Tiles[x, y];
					if (t == null)
					{
						continue;
					}
					Vector2 tile = new Vector2(x, y);
					switch (t.TileIndex)
					{
					case 13:
					case 14:
					case 15:
						if (CanLoadPathObjectHere(tile))
						{
							objects.Add(tile, new Object(tile, getWeedForSeason(Game1.random, GetSeasonForLocation()), 1));
						}
						break;
					case 16:
						if (CanLoadPathObjectHere(tile))
						{
							objects.Add(tile, new Object(tile, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 17:
						if (CanLoadPathObjectHere(tile))
						{
							objects.Add(tile, new Object(tile, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 18:
						if (CanLoadPathObjectHere(tile))
						{
							objects.Add(tile, new Object(tile, (Game1.random.NextDouble() < 0.5) ? 294 : 295, 1));
						}
						break;
					}
				}
			}
		}

		public bool CanLoadPathObjectHere(Vector2 tile)
		{
			if (objects.ContainsKey(tile))
			{
				return false;
			}
			if (terrainFeatures.ContainsKey(tile))
			{
				return false;
			}
			if (getLargeTerrainFeatureAt((int)tile.X, (int)tile.Y) != null)
			{
				return false;
			}
			Vector2 tile_center = tile * 64f;
			tile_center.X += 32f;
			tile_center.Y += 32f;
			foreach (Furniture f in furniture)
			{
				if ((int)f.furniture_type != 12 && !f.isPassable() && f.getBoundingBox(f.tileLocation).Contains((int)tile_center.X, (int)tile_center.Y) && !f.AllowPlacementOnThisTile((int)tile.X, (int)tile.Y))
				{
					return false;
				}
			}
			return true;
		}

		public void loadObjects()
		{
			if (map == null)
			{
				return;
			}
			updateWarps();
			map.Properties.TryGetValue(Game1.currentSeason.Substring(0, 1).ToUpper() + Game1.currentSeason.Substring(1) + "_Objects", out PropertyValue springObjects);
			bool hasPathsLayer = false;
			foreach (Layer layer in map.Layers)
			{
				if (layer.Id.Equals("Paths"))
				{
					hasPathsLayer = true;
					break;
				}
			}
			map.Properties.TryGetValue("Trees", out PropertyValue trees);
			if (trees != null)
			{
				string[] rawTreeString = trees.ToString().Split(' ');
				for (int j = 0; j < rawTreeString.Length; j += 3)
				{
					int x2 = Convert.ToInt32(rawTreeString[j]);
					int y2 = Convert.ToInt32(rawTreeString[j + 1]);
					int treeType2 = Convert.ToInt32(rawTreeString[j + 2]) + 1;
					terrainFeatures.Add(new Vector2(x2, y2), new Tree(treeType2, 5));
				}
			}
			if (this is FishShop || this is SeedShop)
			{
				updateDoors();
			}
			if (!(((bool)isOutdoors || name.Equals("BathHouse_Entry") || (bool)treatAsOutdoors || map.Properties.ContainsKey("forceLoadObjects")) & hasPathsLayer))
			{
				return;
			}
			List<Vector2> startingCabins = new List<Vector2>();
			for (int x = 0; x < map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < map.Layers[0].LayerHeight; y++)
				{
					Tile t = map.GetLayer("Paths").Tiles[x, y];
					if (t == null)
					{
						continue;
					}
					Vector2 tile = new Vector2(x, y);
					int treeType = -1;
					switch (t.TileIndex)
					{
					case 9:
						treeType = 1;
						if (Game1.currentSeason.Equals("winter"))
						{
							treeType += 3;
						}
						break;
					case 10:
						treeType = 2;
						if (Game1.currentSeason.Equals("winter"))
						{
							treeType += 3;
						}
						break;
					case 11:
						treeType = 3;
						break;
					case 12:
						treeType = 6;
						break;
					case 31:
						treeType = 9;
						break;
					case 32:
						treeType = 8;
						break;
					}
					if (treeType != -1)
					{
						if (GetFurnitureAt(tile) == null && !terrainFeatures.ContainsKey(tile) && !objects.ContainsKey(tile))
						{
							terrainFeatures.Add(tile, new Tree(treeType, 5));
						}
						continue;
					}
					switch (t.TileIndex)
					{
					case 13:
					case 14:
					case 15:
						if (!objects.ContainsKey(tile))
						{
							objects.Add(tile, new Object(tile, getWeedForSeason(Game1.random, GetSeasonForLocation()), 1));
						}
						break;
					case 16:
						if (!objects.ContainsKey(tile))
						{
							objects.Add(tile, new Object(tile, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 17:
						if (!objects.ContainsKey(tile))
						{
							objects.Add(tile, new Object(tile, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 18:
						if (!objects.ContainsKey(tile))
						{
							objects.Add(tile, new Object(tile, (Game1.random.NextDouble() < 0.5) ? 294 : 295, 1));
						}
						break;
					case 19:
						addResourceClumpAndRemoveUnderlyingTerrain(602, 2, 2, tile);
						break;
					case 20:
						addResourceClumpAndRemoveUnderlyingTerrain(672, 2, 2, tile);
						break;
					case 21:
						addResourceClumpAndRemoveUnderlyingTerrain(600, 2, 2, tile);
						break;
					case 22:
						if (!terrainFeatures.ContainsKey(tile))
						{
							terrainFeatures.Add(tile, new Grass(1, 3));
						}
						break;
					case 23:
						if (!terrainFeatures.ContainsKey(tile))
						{
							terrainFeatures.Add(tile, new Tree(Game1.random.Next(1, 4), Game1.random.Next(2, 4)));
						}
						break;
					case 24:
						if (!terrainFeatures.ContainsKey(tile))
						{
							largeTerrainFeatures.Add(new Bush(tile, 2, this));
						}
						break;
					case 25:
						if (!terrainFeatures.ContainsKey(tile))
						{
							largeTerrainFeatures.Add(new Bush(tile, 1, this));
						}
						break;
					case 26:
						if (!terrainFeatures.ContainsKey(tile))
						{
							largeTerrainFeatures.Add(new Bush(tile, 0, this));
						}
						break;
					case 33:
						if (!terrainFeatures.ContainsKey(tile))
						{
							largeTerrainFeatures.Add(new Bush(tile, 4, this));
						}
						break;
					case 27:
						changeMapProperties("BrookSounds", tile.X + " " + tile.Y + " 0");
						break;
					case 28:
					{
						string a = name;
						if (a == "BugLand")
						{
						}
						break;
					}
					case 29:
					case 30:
						if (Game1.startingCabins > 0)
						{
							PropertyValue pv = null;
							t.Properties.TryGetValue("Order", out pv);
							if (pv != null && int.Parse(pv.ToString()) <= Game1.startingCabins && ((t.TileIndex == 29 && !Game1.cabinsSeparate) || (t.TileIndex == 30 && Game1.cabinsSeparate)))
							{
								startingCabins.Add(tile);
							}
						}
						break;
					}
				}
			}
			if (springObjects != null && !Game1.eventUp)
			{
				spawnObjects();
			}
			if (startingCabins.Count > 0)
			{
				List<string> cabinStyleOrder = new List<string>();
				switch (Game1.whichFarm)
				{
				case 3:
				case 4:
					cabinStyleOrder.Add("Stone Cabin");
					cabinStyleOrder.Add("Log Cabin");
					cabinStyleOrder.Add("Plank Cabin");
					break;
				case 1:
					cabinStyleOrder.Add("Plank Cabin");
					cabinStyleOrder.Add("Log Cabin");
					cabinStyleOrder.Add("Stone Cabin");
					break;
				default:
				{
					bool logFirst = Game1.random.NextDouble() < 0.5;
					cabinStyleOrder.Add(logFirst ? "Log Cabin" : "Plank Cabin");
					cabinStyleOrder.Add("Stone Cabin");
					cabinStyleOrder.Add(logFirst ? "Plank Cabin" : "Log Cabin");
					break;
				}
				}
				for (int i = 0; i < startingCabins.Count; i++)
				{
					if (this is BuildableGameLocation)
					{
						clearArea((int)startingCabins[i].X, (int)startingCabins[i].Y, 5, 3);
						clearArea((int)startingCabins[i].X + 2, (int)startingCabins[i].Y + 3, 1, 1);
						setTileProperty((int)startingCabins[i].X + 2, (int)startingCabins[i].Y + 3, "Back", "NoSpawn", "All");
						Building b = new Building(new BluePrint(cabinStyleOrder[i])
						{
							magical = true
						}, startingCabins[i]);
						b.daysOfConstructionLeft.Value = 0;
						b.load();
						(this as BuildableGameLocation).buildStructure(b, startingCabins[i], Game1.player, skipSafetyChecks: true);
						b.removeOverlappingBushes(this);
					}
				}
			}
			updateDoors();
		}

		public void updateDoors()
		{
			doors.Clear();
			Layer building_layer = map.GetLayer("Buildings");
			for (int x = 0; x < building_layer.LayerWidth; x++)
			{
				for (int y = 0; y < building_layer.LayerHeight; y++)
				{
					if (building_layer.Tiles[x, y] == null)
					{
						continue;
					}
					PropertyValue door = null;
					building_layer.Tiles[x, y].Properties.TryGetValue("Action", out door);
					if (door != null && door.ToString().Contains("Warp"))
					{
						string[] split = door.ToString().Split(' ');
						if (split[0].Equals("WarpBoatTunnel"))
						{
							doors.Add(new Point(x, y), new NetString("BoatTunnel"));
						}
						if (split[0].Equals("WarpCommunityCenter"))
						{
							doors.Add(new Point(x, y), new NetString("CommunityCenter"));
						}
						else if (split[0].Equals("Warp_Sunroom_Door"))
						{
							doors.Add(new Point(x, y), new NetString("Sunroom"));
						}
						else if ((!name.Equals("Mountain") || x != 8 || y != 20) && split.Length > 3)
						{
							doors.Add(new Point(x, y), new NetString(split[3]));
						}
					}
				}
			}
		}

		private void clearArea(int startingX, int startingY, int width, int height)
		{
			for (int x = startingX; x < startingX + width; x++)
			{
				for (int y = startingY; y < startingY + height; y++)
				{
					removeEverythingExceptCharactersFromThisTile(x, y);
				}
			}
		}

		public bool isTerrainFeatureAt(int x, int y)
		{
			Vector2 v = new Vector2(x, y);
			if (terrainFeatures.ContainsKey(v) && !terrainFeatures[v].isPassable())
			{
				return true;
			}
			if (largeTerrainFeatures != null)
			{
				Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(tileRect))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void loadLights()
		{
			if (((bool)isOutdoors && !Game1.isFestival() && !forceLoadPathLayerLights) || this is FarmHouse || this is IslandFarmHouse)
			{
				return;
			}
			bool hasPathsLayer = false;
			foreach (Layer layer in map.Layers)
			{
				if (layer.Id.Equals("Paths"))
				{
					hasPathsLayer = true;
					break;
				}
			}
			for (int x = 0; x < map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < map.Layers[0].LayerHeight; y++)
				{
					if (!isOutdoors && !map.Properties.ContainsKey("IgnoreLightingTiles"))
					{
						Tile t3 = map.GetLayer("Front").Tiles[x, y];
						if (t3 != null)
						{
							adjustMapLightPropertiesForLamp(t3.TileIndex, x, y, "Front");
						}
						t3 = map.GetLayer("Buildings").Tiles[x, y];
						if (t3 != null)
						{
							adjustMapLightPropertiesForLamp(t3.TileIndex, x, y, "Buildings");
						}
					}
					if (hasPathsLayer)
					{
						Tile t3 = map.GetLayer("Paths").Tiles[x, y];
						if (t3 != null)
						{
							adjustMapLightPropertiesForLamp(t3.TileIndex, x, y, "Paths");
						}
					}
				}
			}
		}

		public bool isFarmBuildingInterior()
		{
			if (this is AnimalHouse)
			{
				return true;
			}
			return false;
		}

		public virtual bool CanBeRemotedlyViewed()
		{
			return false;
		}

		protected void adjustMapLightPropertiesForLamp(int tile, int x, int y, string layer)
		{
			if (isFarmBuildingInterior())
			{
				switch (tile)
				{
				case 24:
					changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 26);
					changeMapProperties("WindowLight", x + " " + (y + 1) + " 4");
					changeMapProperties("WindowLight", x + " " + (y + 3) + " 4");
					break;
				case 25:
					changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 12);
					break;
				case 46:
					changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 53);
					break;
				}
				return;
			}
			switch (tile)
			{
			case 8:
				changeMapProperties("Light", x + " " + y + " 4");
				break;
			case 1346:
				changeMapProperties("DayTiles", "Front " + x + " " + y + " " + tile);
				changeMapProperties("NightTiles", "Front " + x + " " + y + " " + 1347);
				changeMapProperties("DayTiles", "Buildings " + x + " " + (y + 1) + " " + 452);
				changeMapProperties("NightTiles", "Buildings " + x + " " + (y + 1) + " " + 453);
				changeMapProperties("Light", x + " " + y + " 4");
				break;
			case 480:
				changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
				changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 809);
				changeMapProperties("Light", x + " " + y + " 4");
				break;
			case 826:
				changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
				changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 827);
				changeMapProperties("Light", x + " " + y + " 4");
				break;
			case 1344:
				changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
				changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 1345);
				changeMapProperties("Light", x + " " + y + " 4");
				break;
			case 256:
				changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
				changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 1253);
				changeMapProperties("DayTiles", layer + " " + x + " " + (y + 1) + " " + 288);
				changeMapProperties("NightTiles", layer + " " + x + " " + (y + 1) + " " + 1285);
				changeMapProperties("WindowLight", x + " " + y + " 4");
				changeMapProperties("WindowLight", x + " " + (y + 1) + " 4");
				break;
			case 225:
				if (!name.Contains("BathHouse") && !name.Contains("Club") && (!name.Equals("SeedShop") || (x != 36 && x != 37)))
				{
					changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 1222);
					changeMapProperties("DayTiles", layer + " " + x + " " + (y + 1) + " " + 257);
					changeMapProperties("NightTiles", layer + " " + x + " " + (y + 1) + " " + 1254);
					changeMapProperties("WindowLight", x + " " + y + " 4");
					changeMapProperties("WindowLight", x + " " + (y + 1) + " 4");
				}
				break;
			}
		}

		private void changeMapProperties(string propertyName, string toAdd)
		{
			try
			{
				bool addSpaceToFront = true;
				if (!map.Properties.ContainsKey(propertyName))
				{
					map.Properties.Add(propertyName, new PropertyValue(string.Empty));
					addSpaceToFront = false;
				}
				if (!map.Properties[propertyName].ToString().Contains(toAdd))
				{
					StringBuilder b = new StringBuilder(map.Properties[propertyName].ToString());
					if (addSpaceToFront)
					{
						b.Append(" ");
					}
					b.Append(toAdd);
					map.Properties[propertyName] = new PropertyValue(b.ToString());
				}
			}
			catch (Exception)
			{
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is GameLocation)
			{
				return Equals(obj as GameLocation);
			}
			return false;
		}

		public bool Equals(GameLocation other)
		{
			if (isStructure.Get() == other.isStructure.Get())
			{
				return string.Equals(NameOrUniqueName, other.NameOrUniqueName, StringComparison.Ordinal);
			}
			return false;
		}
	}
}
