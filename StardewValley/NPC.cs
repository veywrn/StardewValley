using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	public class NPC : Character, IComparable
	{
		public const int minimum_square_pause = 6000;

		public const int maximum_square_pause = 12000;

		public const int portrait_width = 64;

		public const int portrait_height = 64;

		public const int portrait_neutral_index = 0;

		public const int portrait_happy_index = 1;

		public const int portrait_sad_index = 2;

		public const int portrait_custom_index = 3;

		public const int portrait_blush_index = 4;

		public const int portrait_angry_index = 5;

		public const int startingFriendship = 0;

		public const int defaultSpeed = 2;

		public const int maxGiftsPerWeek = 2;

		public const int friendshipPointsPerHeartLevel = 250;

		public const int maxFriendshipPoints = 2500;

		public const int gift_taste_love = 0;

		public const int gift_taste_like = 2;

		public const int gift_taste_neutral = 8;

		public const int gift_taste_dislike = 4;

		public const int gift_taste_hate = 6;

		public const int textStyle_shake = 0;

		public const int textStyle_fade = 1;

		public const int textStyle_none = 2;

		public const int adult = 0;

		public const int teen = 1;

		public const int child = 2;

		public const int neutral = 0;

		public const int polite = 1;

		public const int rude = 2;

		public const int outgoing = 0;

		public const int shy = 1;

		public const int positive = 0;

		public const int negative = 1;

		public const int male = 0;

		public const int female = 1;

		public const int undefined = 2;

		public const int other = 0;

		public const int desert = 1;

		public const int town = 2;

		public static bool isCheckingSpouseTileOccupancy = false;

		private static List<List<string>> routesFromLocationToLocation;

		private Dictionary<int, SchedulePathDescription> schedule;

		private Dictionary<string, string> dialogue;

		private SchedulePathDescription directionsToNewLocation;

		private int directionIndex;

		private int lengthOfWalkingSquareX;

		private int lengthOfWalkingSquareY;

		private int squarePauseAccumulation;

		private int squarePauseTotal;

		private int squarePauseOffset;

		public Microsoft.Xna.Framework.Rectangle lastCrossroad;

		private Texture2D portrait;

		private Vector2 nextSquarePosition;

		protected int shakeTimer;

		private bool isWalkingInSquare;

		private readonly NetBool isWalkingTowardPlayer = new NetBool();

		protected string textAboveHead;

		protected int textAboveHeadPreTimer;

		protected int textAboveHeadTimer;

		protected int textAboveHeadStyle;

		protected int textAboveHeadColor;

		protected float textAboveHeadAlpha;

		public int daysAfterLastBirth = -1;

		private string extraDialogueMessageToAddThisMorning;

		[XmlElement("birthday_Season")]
		public readonly NetString birthday_Season = new NetString();

		[XmlElement("birthday_Day")]
		public readonly NetInt birthday_Day = new NetInt();

		[XmlElement("age")]
		public readonly NetInt age = new NetInt();

		[XmlElement("manners")]
		public readonly NetInt manners = new NetInt();

		[XmlElement("socialAnxiety")]
		public readonly NetInt socialAnxiety = new NetInt();

		[XmlElement("optimism")]
		public readonly NetInt optimism = new NetInt();

		[XmlElement("gender")]
		public readonly NetInt gender = new NetInt();

		[XmlIgnore]
		public readonly NetBool breather = new NetBool(value: true);

		[XmlIgnore]
		public readonly NetBool isSleeping = new NetBool(value: false);

		[XmlElement("sleptInBed")]
		public readonly NetBool sleptInBed = new NetBool(value: true);

		[XmlIgnore]
		public readonly NetBool hideShadow = new NetBool();

		[XmlElement("isInvisible")]
		public readonly NetBool isInvisible = new NetBool(value: false);

		[XmlElement("lastSeenMovieWeek")]
		public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

		[XmlIgnore]
		public readonly NetString syncedPortraitPath = new NetString();

		public bool datingFarmer;

		public bool divorcedFromFarmer;

		[XmlElement("datable")]
		public readonly NetBool datable = new NetBool();

		[XmlIgnore]
		public bool uniqueSpriteActive;

		[XmlIgnore]
		public bool uniquePortraitActive;

		[XmlIgnore]
		public bool updatedDialogueYet;

		[XmlIgnore]
		public bool immediateSpeak;

		[XmlIgnore]
		public bool ignoreScheduleToday;

		protected int defaultFacingDirection;

		[XmlElement("defaultPosition")]
		private readonly NetVector2 defaultPosition = new NetVector2();

		[XmlElement("defaultMap")]
		public readonly NetString defaultMap = new NetString();

		public string loveInterest;

		protected int idForClones = -1;

		public int id = -1;

		public int homeRegion;

		public int daysUntilNotInvisible;

		public bool followSchedule = true;

		[XmlIgnore]
		public PathFindController temporaryController;

		[XmlElement("moveTowardPlayerThreshold")]
		public readonly NetInt moveTowardPlayerThreshold = new NetInt();

		[XmlIgnore]
		public float rotation;

		[XmlIgnore]
		public float yOffset;

		[XmlIgnore]
		public float swimTimer;

		[XmlIgnore]
		public float timerSinceLastMovement;

		[XmlIgnore]
		public string mapBeforeEvent;

		[XmlIgnore]
		public Vector2 positionBeforeEvent;

		[XmlIgnore]
		public Vector2 lastPosition;

		[XmlIgnore]
		public float currentScheduleDelay;

		[XmlIgnore]
		public float scheduleDelaySeconds;

		[XmlIgnore]
		public bool layingDown;

		[XmlIgnore]
		public Vector2 appliedRouteAnimationOffset = Vector2.Zero;

		[XmlIgnore]
		public string[] routeAnimationMetadata;

		[XmlElement("hasSaidAfternoonDialogue")]
		private NetBool hasSaidAfternoonDialogue = new NetBool(value: false);

		[XmlIgnore]
		public static bool hasSomeoneWateredCrops;

		[XmlIgnore]
		public static bool hasSomeoneFedThePet;

		[XmlIgnore]
		public static bool hasSomeoneFedTheAnimals;

		[XmlIgnore]
		public static bool hasSomeoneRepairedTheFences = false;

		[XmlIgnore]
		protected bool _skipRouteEndIntro;

		[NonInstancedStatic]
		public static HashSet<string> invalidDialogueFiles = new HashSet<string>();

		[XmlIgnore]
		protected bool _hasLoadedMasterScheduleData;

		[XmlIgnore]
		protected Dictionary<string, string> _masterScheduleData;

		[XmlIgnore]
		protected string _lastLoadedScheduleKey;

		[XmlIgnore]
		public readonly NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>> currentMarriageDialogue = new NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>>();

		public readonly NetBool hasBeenKissedToday = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetRef<MarriageDialogueReference> marriageDefaultDialogue = new NetRef<MarriageDialogueReference>(null);

		[XmlIgnore]
		public readonly NetBool shouldSayMarriageDialogue = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetBool exploreFarm = new NetBool(value: false);

		[XmlIgnore]
		public float nextFarmActivityScan;

		[XmlIgnore]
		protected List<FarmActivity> _farmActivities;

		[XmlIgnore]
		protected float _farmActivityWeightTotal;

		[XmlIgnore]
		protected FarmActivity _currentFarmActivity;

		public readonly NetEvent0 removeHenchmanEvent = new NetEvent0();

		private bool isPlayingSleepingAnimation;

		public readonly NetBool shouldPlayRobinHammerAnimation = new NetBool();

		private bool isPlayingRobinHammerAnimation;

		public readonly NetBool shouldPlaySpousePatioAnimation = new NetBool();

		private bool isPlayingSpousePatioAnimation = new NetBool();

		public readonly NetBool shouldWearIslandAttire = new NetBool();

		private bool isWearingIslandAttire;

		public readonly NetBool isMovingOnPathFindPath = new NetBool();

		public List<KeyValuePair<int, SchedulePathDescription>> queuedSchedulePaths = new List<KeyValuePair<int, SchedulePathDescription>>();

		public int lastAttemptedSchedule = -1;

		[XmlIgnore]
		public readonly NetBool doingEndOfRouteAnimation = new NetBool();

		private bool currentlyDoingEndOfRouteAnimation;

		[XmlIgnore]
		public readonly NetBool goingToDoEndOfRouteAnimation = new NetBool();

		[XmlIgnore]
		public readonly NetString endOfRouteMessage = new NetString();

		[XmlElement("dayScheduleName")]
		public readonly NetString dayScheduleName = new NetString();

		[XmlElement("islandScheduleName")]
		public readonly NetString islandScheduleName = new NetString();

		private int[] routeEndIntro;

		private int[] routeEndAnimation;

		private int[] routeEndOutro;

		[XmlIgnore]
		public string nextEndOfRouteMessage;

		private string loadedEndOfRouteBehavior;

		[XmlIgnore]
		protected string _startedEndOfRouteBehavior;

		[XmlIgnore]
		protected string _finishingEndOfRouteBehavior;

		[XmlIgnore]
		protected int _beforeEndOfRouteAnimationFrame;

		public readonly NetString endOfRouteBehaviorName = new NetString();

		private Point previousEndPoint;

		public int squareMovementFacingPreference;

		public const int NO_TRY = 9999999;

		private bool returningToEndPoint;

		private string nameOfTodaysSchedule = "";

		[XmlIgnore]
		public SchedulePathDescription DirectionsToNewLocation
		{
			get
			{
				return directionsToNewLocation;
			}
			set
			{
				directionsToNewLocation = value;
			}
		}

		[XmlIgnore]
		public int DirectionIndex
		{
			get
			{
				return directionIndex;
			}
			set
			{
				directionIndex = value;
			}
		}

		public int DefaultFacingDirection
		{
			get
			{
				return defaultFacingDirection;
			}
			set
			{
				defaultFacingDirection = value;
			}
		}

		[XmlIgnore]
		public Dictionary<string, string> Dialogue
		{
			get
			{
				if (this is Monster)
				{
					return null;
				}
				if (this is Pet)
				{
					return null;
				}
				if (this is Horse)
				{
					return null;
				}
				if (this is Child)
				{
					return null;
				}
				if (dialogue == null)
				{
					string dialogue_file = "Characters\\Dialogue\\" + GetDialogueSheetName();
					if (invalidDialogueFiles.Contains(dialogue_file))
					{
						dialogue = new Dictionary<string, string>();
					}
					try
					{
						dialogue = Game1.content.Load<Dictionary<string, string>>(dialogue_file).Select(delegate(KeyValuePair<string, string> pair)
						{
							string key = pair.Key;
							string text = pair.Value;
							if (text.Contains("¦"))
							{
								text = ((!Game1.player.IsMale) ? text.Substring(text.IndexOf("¦") + 1) : text.Substring(0, text.IndexOf("¦")));
							}
							return new KeyValuePair<string, string>(key, text);
						}).ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
					}
					catch (ContentLoadException)
					{
						invalidDialogueFiles.Add(dialogue_file);
						dialogue = new Dictionary<string, string>();
					}
				}
				return dialogue;
			}
		}

		public string DefaultMap
		{
			get
			{
				return defaultMap.Value;
			}
			set
			{
				defaultMap.Value = value;
			}
		}

		public Vector2 DefaultPosition
		{
			get
			{
				return defaultPosition.Value;
			}
			set
			{
				defaultPosition.Value = value;
			}
		}

		[XmlIgnore]
		public Texture2D Portrait
		{
			get
			{
				if (portrait == null)
				{
					try
					{
						string portraitPath = (!string.IsNullOrEmpty(syncedPortraitPath.Value)) ? ((string)syncedPortraitPath) : ("Portraits\\" + getTextureName());
						if (isWearingIslandAttire)
						{
							try
							{
								portrait = Game1.content.Load<Texture2D>(portraitPath + "_Beach");
							}
							catch (ContentLoadException)
							{
								portrait = null;
							}
						}
						if (portrait == null)
						{
							portrait = Game1.content.Load<Texture2D>(portraitPath);
						}
					}
					catch (ContentLoadException)
					{
						portrait = null;
					}
				}
				return portrait;
			}
			set
			{
				portrait = value;
			}
		}

		[XmlIgnore]
		public Dictionary<int, SchedulePathDescription> Schedule
		{
			get
			{
				return schedule;
			}
			set
			{
				schedule = value;
			}
		}

		public bool IsWalkingInSquare
		{
			get
			{
				return isWalkingInSquare;
			}
			set
			{
				isWalkingInSquare = value;
			}
		}

		public bool IsWalkingTowardPlayer
		{
			get
			{
				return isWalkingTowardPlayer;
			}
			set
			{
				isWalkingTowardPlayer.Value = value;
			}
		}

		[XmlIgnore]
		public Stack<Dialogue> CurrentDialogue
		{
			get
			{
				Stack<Dialogue> currentDialogue = null;
				if (Game1.npcDialogues == null)
				{
					Game1.npcDialogues = new Dictionary<string, Stack<Dialogue>>();
				}
				Game1.npcDialogues.TryGetValue(base.Name, out currentDialogue);
				if (currentDialogue == null)
				{
					Stack<Dialogue> stack2 = Game1.npcDialogues[base.Name] = loadCurrentDialogue();
					currentDialogue = stack2;
				}
				return currentDialogue;
			}
			set
			{
				if (Game1.npcDialogues != null)
				{
					Game1.npcDialogues[base.Name] = value;
				}
			}
		}

		[XmlIgnore]
		public string Birthday_Season
		{
			get
			{
				return birthday_Season;
			}
			set
			{
				birthday_Season.Value = value;
			}
		}

		[XmlIgnore]
		public int Birthday_Day
		{
			get
			{
				return birthday_Day;
			}
			set
			{
				birthday_Day.Value = value;
			}
		}

		[XmlIgnore]
		public int Age
		{
			get
			{
				return age;
			}
			set
			{
				age.Value = value;
			}
		}

		[XmlIgnore]
		public int Manners
		{
			get
			{
				return manners;
			}
			set
			{
				manners.Value = value;
			}
		}

		[XmlIgnore]
		public int SocialAnxiety
		{
			get
			{
				return socialAnxiety;
			}
			set
			{
				socialAnxiety.Value = value;
			}
		}

		[XmlIgnore]
		public int Optimism
		{
			get
			{
				return optimism;
			}
			set
			{
				optimism.Value = value;
			}
		}

		[XmlIgnore]
		public int Gender
		{
			get
			{
				return gender;
			}
			set
			{
				gender.Value = value;
			}
		}

		[XmlIgnore]
		public bool Breather
		{
			get
			{
				return breather;
			}
			set
			{
				breather.Value = value;
			}
		}

		[XmlIgnore]
		public bool HideShadow
		{
			get
			{
				return hideShadow;
			}
			set
			{
				hideShadow.Value = value;
			}
		}

		[XmlIgnore]
		public bool HasPartnerForDance
		{
			get
			{
				foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
				{
					if (onlineFarmer.dancePartner.TryGetVillager() == this)
					{
						return true;
					}
				}
				return false;
			}
		}

		[XmlIgnore]
		public bool IsInvisible
		{
			get
			{
				return isInvisible;
			}
			set
			{
				isInvisible.Value = value;
			}
		}

		public virtual bool CanSocialize
		{
			get
			{
				if (base.Name.Equals("Leo") && !Game1.MasterPlayer.mailReceived.Contains("addedParrotBoy"))
				{
					return false;
				}
				if (base.Name.Equals("Sandy") && !Game1.MasterPlayer.mailReceived.Contains("ccVault"))
				{
					return false;
				}
				if (base.Name.Equals("???") || base.Name.Equals("Bouncer") || base.Name.Equals("Marlon") || base.Name.Equals("Gil") || base.Name.Equals("Gunther") || base.Name.Equals("Henchman") || base.Name.Equals("Birdie") || IsMonster || this is Horse || this is Pet || this is JunimoHarvester)
				{
					return false;
				}
				if (base.Name.Equals("Dwarf") || base.Name.Contains("Qi") || this is Pet || this is Horse || this is Junimo)
				{
					return false;
				}
				if (base.Name.Equals("Krobus"))
				{
					return Game1.player.friendshipData.ContainsKey("Krobus");
				}
				return true;
			}
		}

		public NPC()
		{
		}

		public NPC(AnimatedSprite sprite, Vector2 position, int facingDir, string name, LocalizedContentManager content = null)
			: base(sprite, position, 2, name)
		{
			faceDirection(facingDir);
			defaultPosition.Value = position;
			defaultFacingDirection = facingDir;
			lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
			if (content != null)
			{
				try
				{
					portrait = content.Load<Texture2D>("Portraits\\" + name);
				}
				catch (Exception)
				{
				}
			}
		}

		public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDirection, string name, bool datable, Dictionary<int, int[]> schedule, Texture2D portrait)
			: this(sprite, position, defaultMap, facingDirection, name, schedule, portrait, eventActor: false)
		{
			this.datable.Value = datable;
		}

		public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDir, string name, Dictionary<int, int[]> schedule, Texture2D portrait, bool eventActor, string syncedPortraitPath = null)
			: base(sprite, position, 2, name)
		{
			this.portrait = portrait;
			this.syncedPortraitPath.Value = syncedPortraitPath;
			faceDirection(facingDir);
			if (!eventActor)
			{
				lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
			}
			reloadData();
			defaultPosition.Value = position;
			this.defaultMap.Value = defaultMap;
			base.currentLocation = Game1.getLocationFromName(defaultMap);
			defaultFacingDirection = facingDir;
		}

		public virtual void reloadData()
		{
			try
			{
				Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				if (!(this is Child) && NPCDispositions.ContainsKey(name))
				{
					string[] dataSplit = NPCDispositions[name].Split('/');
					string a = dataSplit[0];
					if (!(a == "teen"))
					{
						if (a == "child")
						{
							Age = 2;
						}
					}
					else
					{
						Age = 1;
					}
					a = dataSplit[1];
					if (!(a == "rude"))
					{
						if (a == "polite")
						{
							Manners = 1;
						}
					}
					else
					{
						Manners = 2;
					}
					a = dataSplit[2];
					if (!(a == "shy"))
					{
						if (a == "outgoing")
						{
							SocialAnxiety = 0;
						}
					}
					else
					{
						SocialAnxiety = 1;
					}
					a = dataSplit[3];
					if (!(a == "positive"))
					{
						if (a == "negative")
						{
							Optimism = 1;
						}
					}
					else
					{
						Optimism = 0;
					}
					a = dataSplit[4];
					if (!(a == "female"))
					{
						if (a == "undefined")
						{
							Gender = 2;
						}
					}
					else
					{
						Gender = 1;
					}
					a = dataSplit[5];
					if (!(a == "datable"))
					{
						if (a == "not-datable")
						{
							datable.Value = false;
						}
					}
					else
					{
						datable.Value = true;
					}
					loveInterest = dataSplit[6];
					switch (dataSplit[7])
					{
					case "Desert":
						homeRegion = 1;
						break;
					case "Other":
						homeRegion = 0;
						break;
					case "Town":
						homeRegion = 2;
						break;
					}
					if (dataSplit.Length > 8 && dataSplit[8].Length > 0)
					{
						Birthday_Season = dataSplit[8].Split(' ')[0];
						Birthday_Day = Convert.ToInt32(dataSplit[8].Split(' ')[1]);
					}
					for (int i = 0; i < NPCDispositions.Count; i++)
					{
						if (NPCDispositions.ElementAt(i).Key.Equals(name))
						{
							id = i;
							break;
						}
					}
					if (!isMarried())
					{
						reloadDefaultLocation();
					}
					base.displayName = dataSplit[11];
				}
			}
			catch (Exception)
			{
			}
		}

		public virtual void reloadDefaultLocation()
		{
			try
			{
				Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				if (NPCDispositions.ContainsKey(name))
				{
					string[] locationParts = NPCDispositions[base.Name].Split('/')[10].Split(' ');
					DefaultMap = locationParts[0];
					DefaultPosition = new Vector2(Convert.ToInt32(locationParts[1]) * 64, Convert.ToInt32(locationParts[2]) * 64);
				}
			}
			catch (Exception)
			{
			}
		}

		public virtual bool canTalk()
		{
			return true;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(birthday_Season, birthday_Day, datable, shouldPlayRobinHammerAnimation, shouldPlaySpousePatioAnimation, isWalkingTowardPlayer, moveTowardPlayerThreshold, age, manners, socialAnxiety, optimism, gender, breather, isSleeping, hideShadow, isInvisible, defaultMap, defaultPosition, removeHenchmanEvent, doingEndOfRouteAnimation, goingToDoEndOfRouteAnimation, endOfRouteMessage, endOfRouteBehaviorName, lastSeenMovieWeek, currentMarriageDialogue, marriageDefaultDialogue, shouldSayMarriageDialogue, hasBeenKissedToday, syncedPortraitPath, hasSaidAfternoonDialogue, dayScheduleName, islandScheduleName, sleptInBed, shouldWearIslandAttire, isMovingOnPathFindPath);
			position.Field.AxisAlignedMovement = true;
			removeHenchmanEvent.onEvent += performRemoveHenchman;
		}

		protected override string translateName(string name)
		{
			switch (name)
			{
			case "Gunther":
				return Game1.content.LoadString("Strings\\NPCNames:Gunther");
			case "Gil":
				return Game1.content.LoadString("Strings\\NPCNames:Gil");
			case "Morris":
				return Game1.content.LoadString("Strings\\NPCNames:Morris");
			case "Marlon":
				return Game1.content.LoadString("Strings\\NPCNames:Marlon");
			case "Kel":
				return Game1.content.LoadString("Strings\\NPCNames:Kel");
			case "Old Mariner":
				return Game1.content.LoadString("Strings\\NPCNames:OldMariner");
			case "Mister Qi":
				return Game1.content.LoadString("Strings\\NPCNames:MisterQi");
			case "Bouncer":
				return Game1.content.LoadString("Strings\\NPCNames:Bouncer");
			case "Henchman":
				return Game1.content.LoadString("Strings\\NPCNames:Henchman");
			case "Welwick":
				return Game1.content.LoadString("Strings\\NPCNames:Welwick");
			case "Governor":
				return Game1.content.LoadString("Strings\\NPCNames:Governor");
			case "Grandpa":
				return Game1.content.LoadString("Strings\\NPCNames:Grandpa");
			case "Bear":
				return Game1.content.LoadString("Strings\\NPCNames:Bear");
			case "Birdie":
				return Game1.content.LoadString("Strings\\NPCNames:Birdie");
			default:
			{
				Dictionary<string, string> dispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				if (dispositions.ContainsKey(name))
				{
					string[] array = dispositions[name].Split('/');
					return array[array.Length - 1];
				}
				return name;
			}
			}
		}

		public string getName()
		{
			if (base.displayName != null && base.displayName.Length > 0)
			{
				return base.displayName;
			}
			return base.Name;
		}

		public string getTextureName()
		{
			return getTextureNameForCharacter(base.Name);
		}

		public static string getTextureNameForCharacter(string character_name)
		{
			string textureName = (character_name == "Old Mariner") ? "Mariner" : ((character_name == "Dwarf King") ? "DwarfKing" : ((character_name == "Mister Qi") ? "MrQi" : ((character_name == "???") ? "Monsters\\Shadow Guy" : ((!(character_name == "Leo")) ? character_name : "ParrotBoy"))));
			if (character_name.Equals(Utility.getOtherFarmerNames()[0]))
			{
				textureName = (Game1.player.IsMale ? "maleRival" : "femaleRival");
			}
			return textureName;
		}

		public virtual bool PathToOnFarm(Point destination, PathFindController.endBehavior on_path_success = null)
		{
			base.controller = null;
			Stack<Point> path = PathFindController.FindPathOnFarm(getTileLocationPoint(), destination, base.currentLocation, 2000);
			if (path != null)
			{
				base.controller = new PathFindController(path, this, base.currentLocation);
				base.controller.nonDestructivePathing = true;
				ignoreScheduleToday = true;
				PathFindController controller = base.controller;
				controller.endBehaviorFunction = (PathFindController.endBehavior)Delegate.Combine(controller.endBehaviorFunction, on_path_success);
				return true;
			}
			return false;
		}

		public virtual void OnFinishPathForActivity(Character c, GameLocation location)
		{
			_currentFarmActivity.BeginActivity();
		}

		public void resetPortrait()
		{
			portrait = null;
		}

		public void resetSeasonalDialogue()
		{
			dialogue = null;
		}

		public virtual void reloadSprite()
		{
			string textureName = getTextureName();
			if (!IsMonster)
			{
				Sprite = new AnimatedSprite("Characters\\" + textureName);
				if (!base.Name.Contains("Dwarf") && !base.Name.Equals("Krobus"))
				{
					Sprite.SpriteHeight = 32;
				}
			}
			else
			{
				Sprite = new AnimatedSprite("Monsters\\" + textureName);
			}
			resetPortrait();
			_ = IsInvisible;
			if (Game1.newDay || Game1.gameMode == 6)
			{
				faceDirection(DefaultFacingDirection);
				previousEndPoint = new Point((int)defaultPosition.X / 64, (int)defaultPosition.Y / 64);
				Schedule = getSchedule(Game1.dayOfMonth);
				faceDirection(defaultFacingDirection);
				resetSeasonalDialogue();
				resetCurrentDialogue();
				if (isMarried() && !getSpouse().divorceTonight && !IsInvisible)
				{
					marriageDuties();
				}
				updateConstructionAnimation();
				try
				{
					base.displayName = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions")[base.Name].Split('/')[11];
				}
				catch (Exception)
				{
				}
			}
		}

		private void updateConstructionAnimation()
		{
			bool isFestivalDay = Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason);
			if (Game1.IsMasterGame && base.Name == "Robin" && !isFestivalDay)
			{
				if ((int)Game1.player.daysUntilHouseUpgrade > 0)
				{
					Farm farm = Game1.getFarm();
					Game1.warpCharacter(this, "Farm", new Vector2(farm.GetMainFarmHouseEntry().X + 4, farm.GetMainFarmHouseEntry().Y - 1));
					isPlayingRobinHammerAnimation = false;
					shouldPlayRobinHammerAnimation.Value = true;
					return;
				}
				if (Game1.getFarm().isThereABuildingUnderConstruction())
				{
					Building b = Game1.getFarm().getBuildingUnderConstruction();
					if ((int)b.daysUntilUpgrade > 0 && b.indoors.Value != null)
					{
						if (base.currentLocation != null)
						{
							base.currentLocation.characters.Remove(this);
						}
						base.currentLocation = b.indoors;
						if (base.currentLocation != null && !base.currentLocation.characters.Contains(this))
						{
							base.currentLocation.addCharacter(this);
						}
						if (b.nameOfIndoorsWithoutUnique.Contains("Shed"))
						{
							setTilePosition(2, 2);
							position.X -= 28f;
						}
						else
						{
							setTilePosition(1, 5);
						}
					}
					else
					{
						Game1.warpCharacter(this, "Farm", new Vector2((int)b.tileX + (int)b.tilesWide / 2, (int)b.tileY + (int)b.tilesHigh / 2));
						position.X += 16f;
						position.Y -= 32f;
					}
					isPlayingRobinHammerAnimation = false;
					shouldPlayRobinHammerAnimation.Value = true;
					return;
				}
				if ((Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value > 0)
				{
					if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						Game1.warpCharacter(this, "Backwoods", new Vector2(41f, 23f));
						isPlayingRobinHammerAnimation = false;
						shouldPlayRobinHammerAnimation.Value = true;
					}
					else
					{
						Game1.warpCharacter(this, "Town", new Vector2(77f, 68f));
						isPlayingRobinHammerAnimation = false;
						shouldPlayRobinHammerAnimation.Value = true;
					}
					return;
				}
			}
			shouldPlayRobinHammerAnimation.Value = false;
		}

		private void doPlayRobinHammerAnimation()
		{
			Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(24, 75),
				new FarmerSprite.AnimationFrame(25, 75),
				new FarmerSprite.AnimationFrame(26, 300, secondaryArm: false, flip: false, robinHammerSound),
				new FarmerSprite.AnimationFrame(27, 1000, secondaryArm: false, flip: false, robinVariablePause)
			});
			ignoreScheduleToday = true;
			bool oneDayLeft = (int)Game1.player.daysUntilHouseUpgrade == 1 || (int)(Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade == 1;
			CurrentDialogue.Clear();
			CurrentDialogue.Push(new Dialogue(oneDayLeft ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3927") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3926"), this));
		}

		public void showTextAboveHead(string Text, int spriteTextColor = -1, int style = 2, int duration = 3000, int preTimer = 0)
		{
			textAboveHeadAlpha = 0f;
			textAboveHead = Text;
			textAboveHeadPreTimer = preTimer;
			textAboveHeadTimer = duration;
			textAboveHeadStyle = style;
			textAboveHeadColor = spriteTextColor;
		}

		public void moveToNewPlaceForEvent(int xTile, int yTile, string oldMap)
		{
			mapBeforeEvent = oldMap;
			positionBeforeEvent = base.Position;
			base.Position = new Vector2(xTile * 64, yTile * 64 - 96);
		}

		public virtual bool hitWithTool(Tool t)
		{
			return false;
		}

		public bool canReceiveThisItemAsGift(Item i)
		{
			if (i is Object || i is Ring || i is Hat || i is Boots || i is MeleeWeapon || i is Clothing)
			{
				return true;
			}
			return false;
		}

		public int getGiftTasteForThisItem(Item item)
		{
			int tasteForItem = 8;
			if (item is Object)
			{
				Object o = item as Object;
				Game1.NPCGiftTastes.TryGetValue(base.Name, out string NPCLikes);
				string[] split = NPCLikes.Split('/');
				int itemNumber = o.ParentSheetIndex;
				int categoryNumber = o.Category;
				string itemNumberString = string.Concat(itemNumber);
				string categoryNumberString = string.Concat(categoryNumber);
				if (Game1.NPCGiftTastes["Universal_Love"].Split(' ').Contains(categoryNumberString))
				{
					tasteForItem = 0;
				}
				else if (Game1.NPCGiftTastes["Universal_Hate"].Split(' ').Contains(categoryNumberString))
				{
					tasteForItem = 6;
				}
				else if (Game1.NPCGiftTastes["Universal_Like"].Split(' ').Contains(categoryNumberString))
				{
					tasteForItem = 2;
				}
				else if (Game1.NPCGiftTastes["Universal_Dislike"].Split(' ').Contains(categoryNumberString))
				{
					tasteForItem = 4;
				}
				if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Love"].Split(' ')))
				{
					tasteForItem = 0;
				}
				else if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Hate"].Split(' ')))
				{
					tasteForItem = 6;
				}
				else if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Like"].Split(' ')))
				{
					tasteForItem = 2;
				}
				else if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Dislike"].Split(' ')))
				{
					tasteForItem = 4;
				}
				bool wasIndividualUniversal = false;
				bool skipDefaultValueRules = false;
				if (Game1.NPCGiftTastes["Universal_Love"].Split(' ').Contains(itemNumberString))
				{
					tasteForItem = 0;
					wasIndividualUniversal = true;
				}
				else if (Game1.NPCGiftTastes["Universal_Hate"].Split(' ').Contains(itemNumberString))
				{
					tasteForItem = 6;
					wasIndividualUniversal = true;
				}
				else if (Game1.NPCGiftTastes["Universal_Like"].Split(' ').Contains(itemNumberString))
				{
					tasteForItem = 2;
					wasIndividualUniversal = true;
				}
				else if (Game1.NPCGiftTastes["Universal_Dislike"].Split(' ').Contains(itemNumberString))
				{
					tasteForItem = 4;
					wasIndividualUniversal = true;
				}
				else if (Game1.NPCGiftTastes["Universal_Neutral"].Split(' ').Contains(itemNumberString))
				{
					tasteForItem = 8;
					wasIndividualUniversal = true;
					skipDefaultValueRules = true;
				}
				if (o.type.Contains("Arch"))
				{
					tasteForItem = 4;
					if (base.Name.Equals("Penny") || name.Equals("Dwarf"))
					{
						tasteForItem = 2;
					}
				}
				if (tasteForItem == 8 && !skipDefaultValueRules)
				{
					if ((int)o.edibility != -300 && (int)o.edibility < 0)
					{
						tasteForItem = 6;
					}
					else if ((int)o.price < 20)
					{
						tasteForItem = 4;
					}
				}
				if (NPCLikes != null)
				{
					List<string[]> items = new List<string[]>();
					for (int j = 0; j < 10; j += 2)
					{
						string[] splitItems = split[j + 1].Split(' ');
						string[] thisItems = new string[splitItems.Length];
						for (int i = 0; i < splitItems.Length; i++)
						{
							if (splitItems[i].Length > 0)
							{
								thisItems[i] = splitItems[i];
							}
						}
						items.Add(thisItems);
					}
					if (items[0].Contains(itemNumberString))
					{
						return 0;
					}
					if (items[3].Contains(itemNumberString))
					{
						return 6;
					}
					if (items[1].Contains(itemNumberString))
					{
						return 2;
					}
					if (items[2].Contains(itemNumberString))
					{
						return 4;
					}
					if (items[4].Contains(itemNumberString))
					{
						return 8;
					}
					if (CheckTasteContextTags(item, items[0]))
					{
						return 0;
					}
					if (CheckTasteContextTags(item, items[3]))
					{
						return 6;
					}
					if (CheckTasteContextTags(item, items[1]))
					{
						return 2;
					}
					if (CheckTasteContextTags(item, items[2]))
					{
						return 4;
					}
					if (CheckTasteContextTags(item, items[4]))
					{
						return 8;
					}
					if (!wasIndividualUniversal)
					{
						if (categoryNumber != 0 && items[0].Contains(categoryNumberString))
						{
							return 0;
						}
						if (categoryNumber != 0 && items[3].Contains(categoryNumberString))
						{
							return 6;
						}
						if (categoryNumber != 0 && items[1].Contains(categoryNumberString))
						{
							return 2;
						}
						if (categoryNumber != 0 && items[2].Contains(categoryNumberString))
						{
							return 4;
						}
						if (categoryNumber != 0 && items[4].Contains(categoryNumberString))
						{
							return 8;
						}
					}
				}
			}
			return tasteForItem;
		}

		public virtual bool CheckTasteContextTags(Item item, string[] list)
		{
			foreach (string entry in list)
			{
				if (entry != null && entry.Length > 0 && !char.IsNumber(entry[0]) && entry[0] != '-' && item.HasContextTag(entry))
				{
					return true;
				}
			}
			return false;
		}

		private void goblinDoorEndBehavior(Character c, GameLocation l)
		{
			l.characters.Remove(this);
			l.playSound("doorClose");
		}

		private void performRemoveHenchman()
		{
			Sprite.CurrentFrame = 4;
			Game1.netWorldState.Value.IsGoblinRemoved = true;
			Game1.player.removeQuest(27);
			Stack<Point> p = new Stack<Point>();
			p.Push(new Point(20, 21));
			p.Push(new Point(20, 22));
			p.Push(new Point(20, 23));
			p.Push(new Point(20, 24));
			p.Push(new Point(20, 25));
			p.Push(new Point(20, 26));
			p.Push(new Point(20, 27));
			p.Push(new Point(20, 28));
			base.addedSpeed = 2;
			controller = new PathFindController(p, this, base.currentLocation);
			controller.endBehaviorFunction = goblinDoorEndBehavior;
			showTextAboveHead(Game1.content.LoadString("Strings\\Characters:Henchman6"));
			Game1.player.mailReceived.Add("henchmanGone");
			base.currentLocation.removeTile(20, 29, "Buildings");
		}

		private void engagementResponse(Farmer who, bool asRoommate = false)
		{
			Game1.changeMusicTrack("none");
			who.spouse = base.Name;
			if (!asRoommate)
			{
				Game1.multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, base.displayName);
			}
			Friendship friendship = who.friendshipData[base.Name];
			friendship.Status = FriendshipStatus.Engaged;
			friendship.RoommateMarriage = asRoommate;
			WorldDate weddingDate = new WorldDate(Game1.Date);
			weddingDate.TotalDays += 3;
			while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
			{
				weddingDate.TotalDays++;
			}
			friendship.WeddingDate = weddingDate;
			CurrentDialogue.Clear();
			CurrentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue")[base.Name + "0"], this));
			string attemptUniqueEngagementString = Game1.content.LoadStringReturnNullIfNotFound("Strings\\StringsFromCSFiles:" + base.Name + "_Engaged");
			if (attemptUniqueEngagementString != null)
			{
				CurrentDialogue.Push(new Dialogue(attemptUniqueEngagementString, this));
			}
			else
			{
				CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3980"), this));
			}
			who.changeFriendship(1, this);
			who.reduceActiveItemByOne();
			who.completelyStopAnimatingOrDoingAction();
			Game1.drawDialogue(this);
		}

		public virtual void tryToReceiveActiveObject(Farmer who)
		{
			who.Halt();
			who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
			if (name.Equals("Henchman") && Game1.currentLocation.name.Equals("WitchSwamp"))
			{
				if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 308)
				{
					if (controller == null)
					{
						who.currentLocation.localSound("coin");
						who.reduceActiveItemByOne();
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman5"), this));
						Game1.drawDialogue(this);
						who.freezePause = 2000;
						removeHenchmanEvent.Fire();
					}
				}
				else if (who.ActiveObject != null)
				{
					if ((int)who.ActiveObject.parentSheetIndex == 684)
					{
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman4"), this));
					}
					else
					{
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman3"), this));
					}
					Game1.drawDialogue(this);
				}
				return;
			}
			if (Game1.player.team.specialOrders != null)
			{
				foreach (SpecialOrder order in Game1.player.team.specialOrders)
				{
					if (order.onItemDelivered != null)
					{
						Delegate[] invocationList = order.onItemDelivered.GetInvocationList();
						for (int i = 0; i < invocationList.Length; i++)
						{
							if (((Func<Farmer, NPC, Item, int>)invocationList[i])(Game1.player, this, who.ActiveObject) > 0)
							{
								if (who.ActiveObject.Stack <= 0)
								{
									who.ActiveObject = null;
									who.showNotCarrying();
								}
								return;
							}
						}
					}
				}
			}
			if (Game1.questOfTheDay != null && (bool)Game1.questOfTheDay.accepted && !Game1.questOfTheDay.completed && Game1.questOfTheDay is ItemDeliveryQuest && ((ItemDeliveryQuest)Game1.questOfTheDay).checkIfComplete(this, -1, -1, who.ActiveObject))
			{
				who.reduceActiveItemByOne();
				who.completelyStopAnimatingOrDoingAction();
				if (Game1.random.NextDouble() < 0.3 && !base.Name.Equals("Wizard"))
				{
					doEmote(32);
				}
			}
			else if (Game1.questOfTheDay != null && Game1.questOfTheDay is FishingQuest && ((FishingQuest)Game1.questOfTheDay).checkIfComplete(this, who.ActiveObject.ParentSheetIndex, 1))
			{
				who.reduceActiveItemByOne();
				who.completelyStopAnimatingOrDoingAction();
				if (Game1.random.NextDouble() < 0.3 && !base.Name.Equals("Wizard"))
				{
					doEmote(32);
				}
			}
			else if (who.ActiveObject != null && Utility.IsNormalObjectAtParentSheetIndex(who.ActiveObject, 897))
			{
				if (base.Name.Equals("Pierre") && !Game1.player.hasOrWillReceiveMail("PierreStocklist"))
				{
					Game1.addMail("PierreStocklist", noLetter: true, sendToEveryone: true);
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					who.currentLocation.localSound("give_gift");
					Game1.player.team.itemsToRemoveOvernight.Add(897);
					setNewDialogue(Game1.content.LoadString("Strings\\Characters:PierreStockListDialogue"), add: true);
					Game1.drawDialogue(this);
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
					{
						Game1.multiplayer.globalChatInfoMessage("StockList");
					});
				}
				else
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", base.displayName)));
				}
			}
			else if (who.ActiveObject != null && !who.ActiveObject.bigCraftable && (int)who.ActiveObject.parentSheetIndex == 71 && base.Name.Equals("Lewis") && who.hasQuest(102))
			{
				if (who.currentLocation != null && who.currentLocation.Name == "IslandSouth")
				{
					Game1.player.activeDialogueEvents["lucky_pants_lewis"] = 28;
				}
				who.completeQuest(102);
				Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				string thankYou = (questData[102].Length > 9) ? questData[102].Split('/')[9] : Game1.content.LoadString("Data\\ExtraDialogue:LostItemQuest_DefaultThankYou");
				setNewDialogue(thankYou);
				Game1.drawDialogue(this);
				Game1.player.changeFriendship(250, this);
				who.ActiveObject = null;
			}
			else if (who.ActiveObject != null && Dialogue.ContainsKey("reject_" + who.ActiveObject.ParentSheetIndex))
			{
				setNewDialogue(Dialogue["reject_" + who.ActiveObject.ParentSheetIndex]);
				Game1.drawDialogue(this);
			}
			else if (who.ActiveObject != null && (bool)who.ActiveObject.questItem)
			{
				if (who.hasQuest(130) && Dialogue.ContainsKey("accept_" + who.ActiveObject.ParentSheetIndex))
				{
					setNewDialogue(Dialogue["accept_" + who.ActiveObject.ParentSheetIndex]);
					Game1.drawDialogue(this);
					CurrentDialogue.Peek().onFinish = delegate
					{
						_ = who.ActiveObject.ParentSheetIndex;
						Object o = new Object(who.ActiveObject.ParentSheetIndex + 1, 1)
						{
							specialItem = true
						};
						o.questItem.Value = true;
						who.reduceActiveItemByOne();
						DelayedAction.playSoundAfterDelay("coin", 200);
						DelayedAction.functionAfterDelay(delegate
						{
							who.addItemByMenuIfNecessary(o);
						}, 200);
						Game1.player.freezePause = 550;
						DelayedAction.functionAfterDelay(delegate
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", o.DisplayName, Lexicon.getProperArticleForWord(o.DisplayName)));
						}, 550);
					};
				}
				else if (!who.checkForQuestComplete(this, -1, -1, who.ActiveObject, "", 9, 3) && (string)name != "Birdie")
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3954"));
				}
			}
			else
			{
				if (who.checkForQuestComplete(this, -1, -1, null, "", 10))
				{
					return;
				}
				if ((int)who.ActiveObject.parentSheetIndex == 809 && !who.ActiveObject.bigCraftable)
				{
					if (!Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", base.displayName)));
						return;
					}
					if (base.Name.Equals("Dwarf") && !who.canUnderstandDwarves)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", base.displayName)));
						return;
					}
					if (base.Name.Equals("Krobus") && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", base.displayName)));
						return;
					}
					if ((!CanSocialize && !base.Name.Equals("Dwarf")) || !isVillager())
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_CantInvite", base.displayName)));
						return;
					}
					if (!who.friendshipData.ContainsKey(base.Name))
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", base.displayName)));
						return;
					}
					if (who.friendshipData[base.Name].IsDivorced())
					{
						if (who == Game1.player)
						{
							Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, base.displayName);
						}
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_gift"), this));
						Game1.drawDialogue(this);
						return;
					}
					if (who.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_FarmerAlreadySeen")));
						return;
					}
					if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Festival")));
						return;
					}
					if (Game1.timeOfDay > 2100)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Closed")));
						return;
					}
					foreach (MovieInvitation invitation2 in who.team.movieInvitations)
					{
						if (invitation2.farmer == who)
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_AlreadyInvitedSomeone", invitation2.invitedNPC.displayName)));
							return;
						}
					}
					faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
					foreach (MovieInvitation invitation in who.team.movieInvitations)
					{
						if (invitation.invitedNPC == this)
						{
							if (who == Game1.player)
							{
								Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, base.displayName);
							}
							CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_InvitedBySomeoneElse", invitation.farmer.displayName), this));
							Game1.drawDialogue(this);
							return;
						}
					}
					if (lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
					{
						if (who == Game1.player)
						{
							Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, base.displayName);
						}
						CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_AlreadySeen"), this));
						Game1.drawDialogue(this);
						return;
					}
					if (MovieTheater.GetResponseForMovie(this) == "reject")
					{
						if (who == Game1.player)
						{
							Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, base.displayName);
						}
						CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_Reject"), this));
						Game1.drawDialogue(this);
						return;
					}
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en && getSpouse() != null && getSpouse().Equals(who) && (string)name != "Krobus")
					{
						CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_Spouse_" + name), this));
					}
					else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en && dialogue != null && dialogue.ContainsKey("MovieInvitation"))
					{
						CurrentDialogue.Push(new Dialogue(dialogue["MovieInvitation"], this));
					}
					else
					{
						CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_Invited"), this));
					}
					Game1.drawDialogue(this);
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					who.currentLocation.localSound("give_gift");
					MovieTheater.Invite(who, this);
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteAccept", Game1.player.displayName, base.displayName);
					}
				}
				else
				{
					if (!Game1.NPCGiftTastes.ContainsKey(base.Name))
					{
						return;
					}
					foreach (string s in who.activeDialogueEvents.Keys)
					{
						if (s.Contains("dumped") && Dialogue.ContainsKey(s))
						{
							doEmote(12);
							return;
						}
					}
					who.completeQuest(25);
					if (who.ActiveObject.ParentSheetIndex == 808 && base.Name.Equals("Krobus"))
					{
						if (who.getFriendshipHeartLevelForNPC(base.Name) >= 10 && (int)who.houseUpgradeLevel >= 1 && !who.isMarried() && !who.isEngaged())
						{
							engagementResponse(who, asRoommate: true);
						}
					}
					else if (who.ActiveObject.ParentSheetIndex == 458)
					{
						if (!datable || (who.spouse != base.Name && isMarriedOrEngaged()))
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", base.displayName));
								return;
							}
							CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3956") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3957"), this));
							Game1.drawDialogue(this);
							return;
						}
						if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDating())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", base.displayName));
							return;
						}
						if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDivorced())
						{
							CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_bouquet"), this));
							Game1.drawDialogue(this);
							return;
						}
						if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].Points < 1000)
						{
							CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3958") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3959"), this));
							Game1.drawDialogue(this);
							return;
						}
						if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].Points < 2000)
						{
							CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3960") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3961"), this));
							Game1.drawDialogue(this);
							return;
						}
						Friendship friendship = who.friendshipData[base.Name];
						if (!friendship.IsDating())
						{
							friendship.Status = FriendshipStatus.Dating;
							Game1.multiplayer.globalChatInfoMessage("Dating", Game1.player.Name, base.displayName);
						}
						CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3962") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3963"), this));
						who.changeFriendship(25, this);
						who.reduceActiveItemByOne();
						who.completelyStopAnimatingOrDoingAction();
						doEmote(20);
						Game1.drawDialogue(this);
					}
					else if (who.ActiveObject.ParentSheetIndex == 277)
					{
						if (!datable || (who.friendshipData.ContainsKey(base.Name) && !who.friendshipData[base.Name].IsDating()) || (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsMarried()))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Meaningless", base.displayName));
						}
						else if (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDating())
						{
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Effect", base.displayName));
							Game1.multiplayer.globalChatInfoMessage("BreakUp", Game1.player.Name, base.displayName);
							who.reduceActiveItemByOne();
							who.friendshipData[base.Name].Status = FriendshipStatus.Friendly;
							who.completelyStopAnimatingOrDoingAction();
							who.friendshipData[base.Name].Points = Math.Min(who.friendshipData[base.Name].Points, 1250);
							switch ((string)name)
							{
							case "Maru":
							case "Haley":
								doEmote(12);
								break;
							default:
								doEmote(28);
								break;
							case "Shane":
							case "Alex":
								break;
							}
							CurrentDialogue.Clear();
							CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Characters\\Dialogue\\" + GetDialogueSheetName() + ":breakUp"), this));
							Game1.drawDialogue(this);
						}
					}
					else if (who.ActiveObject.ParentSheetIndex == 460)
					{
						if (who.isMarried() || who.isEngaged())
						{
							if (who.hasCurrentOrPendingRoommate())
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:TriedToMarryButKrobus"));
							}
							else if (who.isEngaged())
							{
								CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3965") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3966"), this));
								Game1.drawDialogue(this);
							}
							else
							{
								CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3967") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3968"), this));
								Game1.drawDialogue(this);
							}
						}
						else if (!datable || isMarriedOrEngaged() || (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDivorced()) || (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].Points < 1500))
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", base.displayName));
								return;
							}
							CurrentDialogue.Push(new Dialogue((Gender == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3970") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3971"), this));
							Game1.drawDialogue(this);
						}
						else if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].Points < 2500)
						{
							if (!who.friendshipData[base.Name].ProposalRejected)
							{
								CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3973"), this));
								Game1.drawDialogue(this);
								who.changeFriendship(-20, this);
								who.friendshipData[base.Name].ProposalRejected = true;
							}
							else
							{
								CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3974") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3975"), this));
								Game1.drawDialogue(this);
								who.changeFriendship(-50, this);
							}
						}
						else if ((bool)datable && (int)who.houseUpgradeLevel < 1)
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", base.displayName));
								return;
							}
							CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972"), this));
							Game1.drawDialogue(this);
						}
						else
						{
							engagementResponse(who);
						}
					}
					else if ((who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].GiftsThisWeek < 2) || (who.spouse != null && who.spouse.Equals(base.Name)) || this is Child || isBirthday(Game1.currentSeason, Game1.dayOfMonth))
					{
						if (who.friendshipData[base.Name].IsDivorced())
						{
							CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_gift"), this));
							Game1.drawDialogue(this);
							return;
						}
						if (who.friendshipData[base.Name].GiftsToday == 1)
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3981", base.displayName)));
							return;
						}
						receiveGift(who.ActiveObject, who);
						who.reduceActiveItemByOne();
						who.completelyStopAnimatingOrDoingAction();
						faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
						if ((bool)datable && who.spouse != null && !who.spouse.Contains(base.Name) && !who.spouse.Contains("Krobus") && Utility.isMale(who.spouse) == Utility.isMale(base.Name) && Game1.random.NextDouble() < 0.3 - (double)((float)who.LuckLevel / 100f) - who.DailyLuck && !isBirthday(Game1.currentSeason, Game1.dayOfMonth) && who.friendshipData[base.Name].IsDating())
						{
							NPC spouse = Game1.getCharacterFromName(who.spouse);
							who.changeFriendship(-30, spouse);
							spouse.CurrentDialogue.Clear();
							spouse.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3985", base.displayName), spouse));
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3987", base.displayName, 2)));
					}
				}
			}
		}

		public string GetDispositionModifiedString(string path, params object[] substitutions)
		{
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			List<string> disposition_tags = new List<string>();
			string disposition = "";
			disposition_tags.Add(name.Value);
			if (Game1.player.isMarried() && Game1.player.getSpouse() == this)
			{
				disposition_tags.Add("spouse");
			}
			if (dictionary.TryGetValue(base.Name, out disposition))
			{
				string[] dispositions = disposition.Split('/');
				if (dispositions.Length > 4)
				{
					disposition_tags.Add(dispositions[1]);
					disposition_tags.Add(dispositions[2]);
					disposition_tags.Add(dispositions[3]);
					disposition_tags.Add(dispositions[0]);
				}
			}
			foreach (string tag in disposition_tags)
			{
				string current_path = path + "_" + Utility.capitalizeFirstLetter(tag);
				string found_string = Game1.content.LoadString(current_path, substitutions);
				if (!(found_string == current_path))
				{
					return found_string;
				}
			}
			return Game1.content.LoadString(path, substitutions);
		}

		public void haltMe(Farmer who)
		{
			Halt();
		}

		public virtual bool checkAction(Farmer who, GameLocation l)
		{
			if (IsInvisible)
			{
				return false;
			}
			if (isSleeping.Value)
			{
				if (!isEmoting)
				{
					doEmote(24);
				}
				shake(250);
				return false;
			}
			if (!who.CanMove)
			{
				return false;
			}
			if (base.Name.Equals("Henchman") && l.Name.Equals("WitchSwamp"))
			{
				if (!Game1.player.mailReceived.Contains("Henchman1"))
				{
					Game1.player.mailReceived.Add("Henchman1");
					CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman1"), this));
					Game1.drawDialogue(this);
					Game1.player.addQuest(27);
					Game1.player.friendshipData.Add("Henchman", new Friendship());
				}
				else
				{
					if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
					{
						tryToReceiveActiveObject(who);
						return true;
					}
					if (controller == null)
					{
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman2"), this));
						Game1.drawDialogue(this);
					}
				}
				return true;
			}
			bool reacting_to_shorts = false;
			if (who.pantsItem.Value != null && (int)who.pantsItem.Value.parentSheetIndex == 15 && (base.Name.Equals("Lewis") || base.Name.Equals("Marnie")))
			{
				reacting_to_shorts = true;
			}
			if (Game1.NPCGiftTastes.ContainsKey(base.Name) && !Game1.player.friendshipData.ContainsKey(base.Name))
			{
				Game1.player.friendshipData.Add(base.Name, new Friendship(0));
				if (base.Name.Equals("Krobus"))
				{
					CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3990"), this));
					Game1.drawDialogue(this);
					return true;
				}
			}
			if (who.checkForQuestComplete(this, -1, -1, who.ActiveObject, null, -1, 5))
			{
				faceTowardFarmerForPeriod(6000, 3, faceAway: false, who);
				return true;
			}
			if (base.Name.Equals("Krobus") && who.hasQuest(28))
			{
				CurrentDialogue.Push(new Dialogue((l is Sewer) ? Game1.content.LoadString("Strings\\Characters:KrobusDarkTalisman") : Game1.content.LoadString("Strings\\Characters:KrobusDarkTalisman_elsewhere"), this));
				Game1.drawDialogue(this);
				who.removeQuest(28);
				who.mailReceived.Add("krobusUnseal");
				if (l is Sewer)
				{
					DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, flicker: false, flipped: false)
					{
						scale = 4f,
						delayBeforeAnimationStart = 1,
						startSound = "debuffSpell",
						motion = new Vector2(-9f, 1f),
						rotationChange = (float)Math.PI / 64f,
						light = true,
						lightRadius = 1f,
						lightcolor = new Color(150, 0, 50),
						layerDepth = 1f,
						alphaFade = 0.003f
					}, l, 200, waitUntilMenusGone: true);
					DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, flicker: false, flipped: false)
					{
						startSound = "debuffSpell",
						delayBeforeAnimationStart = 1,
						scale = 4f,
						motion = new Vector2(-9f, 1f),
						rotationChange = (float)Math.PI / 64f,
						light = true,
						lightRadius = 1f,
						lightcolor = new Color(150, 0, 50),
						layerDepth = 1f,
						alphaFade = 0.003f
					}, l, 700, waitUntilMenusGone: true);
				}
				return true;
			}
			if (base.Name.Equals(who.spouse) && who.IsLocalPlayer)
			{
				_ = Game1.timeOfDay;
				_ = 2200;
				if (Sprite.CurrentAnimation == null)
				{
					faceDirection(-3);
				}
				if (Sprite.CurrentAnimation == null && who.friendshipData.ContainsKey(name) && who.friendshipData[name].Points >= 3125 && !who.mailReceived.Contains("CF_Spouse"))
				{
					CurrentDialogue.Push(new Dialogue(Game1.content.LoadString(Game1.player.isRoommate(who.spouse) ? "Strings\\StringsFromCSFiles:Krobus_Stardrop" : "Strings\\StringsFromCSFiles:NPC.cs.4001"), this));
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 434, "Cosmic Fruit", canBeSetDown: false, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false));
					shouldSayMarriageDialogue.Value = false;
					currentMarriageDialogue.Clear();
					who.mailReceived.Add("CF_Spouse");
					return true;
				}
				if (Sprite.CurrentAnimation == null && !hasTemporaryMessageAvailable() && currentMarriageDialogue.Count == 0 && CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !isMoving() && who.ActiveObject == null)
				{
					faceGeneralDirection(who.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
					who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
					if (FacingDirection == 3 || FacingDirection == 1)
					{
						int spouseFrame = 28;
						bool facingRight = true;
						switch (base.Name)
						{
						case "Maru":
							spouseFrame = 28;
							facingRight = false;
							break;
						case "Harvey":
							spouseFrame = 31;
							facingRight = false;
							break;
						case "Leah":
							spouseFrame = 25;
							facingRight = true;
							break;
						case "Elliott":
							spouseFrame = 35;
							facingRight = false;
							break;
						case "Sebastian":
							spouseFrame = 40;
							facingRight = false;
							break;
						case "Abigail":
							spouseFrame = 33;
							facingRight = false;
							break;
						case "Penny":
							spouseFrame = 35;
							facingRight = true;
							break;
						case "Alex":
							spouseFrame = 42;
							facingRight = true;
							break;
						case "Sam":
							spouseFrame = 36;
							facingRight = true;
							break;
						case "Shane":
							spouseFrame = 34;
							facingRight = false;
							break;
						case "Emily":
							spouseFrame = 33;
							facingRight = false;
							break;
						case "Krobus":
							spouseFrame = 16;
							facingRight = true;
							break;
						}
						bool flip = (facingRight && FacingDirection == 3) || (!facingRight && FacingDirection == 1);
						if (who.getFriendshipHeartLevelForNPC(base.Name) > 9 && sleptInBed.Value)
						{
							int delay = movementPause = (Game1.IsMultiplayer ? 1000 : 10);
							Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
							{
								new FarmerSprite.AnimationFrame(spouseFrame, delay, secondaryArm: false, flip, haltMe, behaviorAtEndOfFrame: true)
							});
							if (!hasBeenKissedToday.Value)
							{
								who.changeFriendship(10, this);
								if (who.hasCurrentOrPendingRoommate())
								{
									Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\emojis", new Microsoft.Xna.Framework.Rectangle(0, 0, 9, 9), 2000f, 1, 0, new Vector2(getTileX(), getTileY()) * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
									{
										motion = new Vector2(0f, -0.5f),
										alphaFade = 0.01f
									});
								}
								else
								{
									Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, new Vector2(getTileX(), getTileY()) * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
									{
										motion = new Vector2(0f, -0.5f),
										alphaFade = 0.01f
									});
								}
								l.playSound("dwop", NetAudio.SoundContext.NPC);
								who.exhausted.Value = false;
							}
							hasBeenKissedToday.Value = true;
							Sprite.UpdateSourceRect();
						}
						else
						{
							faceDirection((Game1.random.NextDouble() < 0.5) ? 2 : 0);
							doEmote(12);
						}
						int playerFaceDirection = 1;
						if ((facingRight && !flip) || (!facingRight && flip))
						{
							playerFaceDirection = 3;
						}
						who.PerformKiss(playerFaceDirection);
						return true;
					}
				}
			}
			bool newCurrentDialogue = false;
			if (who.friendshipData.ContainsKey(base.Name) || base.Name == "Mister Qi")
			{
				if (getSpouse() == Game1.player && shouldSayMarriageDialogue.Value && currentMarriageDialogue.Count > 0 && currentMarriageDialogue.Count > 0)
				{
					while (currentMarriageDialogue.Count > 0)
					{
						MarriageDialogueReference dialogue_reference = currentMarriageDialogue[currentMarriageDialogue.Count - 1];
						if (dialogue_reference == marriageDefaultDialogue.Value)
						{
							marriageDefaultDialogue.Value = null;
						}
						currentMarriageDialogue.RemoveAt(currentMarriageDialogue.Count - 1);
						CurrentDialogue.Push(dialogue_reference.GetDialogue(this));
					}
					newCurrentDialogue = true;
				}
				if (!newCurrentDialogue)
				{
					newCurrentDialogue = checkForNewCurrentDialogue(who.friendshipData.ContainsKey(base.Name) ? (who.friendshipData[base.Name].Points / 250) : 0);
					if (!newCurrentDialogue)
					{
						newCurrentDialogue = checkForNewCurrentDialogue(who.friendshipData.ContainsKey(base.Name) ? (who.friendshipData[base.Name].Points / 250) : 0, noPreface: true);
					}
				}
			}
			if (who.IsLocalPlayer && who.friendshipData.ContainsKey(base.Name) && (((endOfRouteMessage.Value != null) | newCurrentDialogue) || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))))
			{
				if (!newCurrentDialogue && setTemporaryMessages(who))
				{
					Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
					return false;
				}
				if (Sprite.Texture.Bounds.Height > 32)
				{
					faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
				}
				if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
				{
					tryToReceiveActiveObject(who);
					faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
					return true;
				}
				grantConversationFriendship(who);
				Game1.drawDialogue(this);
				return true;
			}
			if (canTalk() && who.hasClubCard && base.Name.Equals("Bouncer") && who.IsLocalPlayer)
			{
				Response[] responses = new Response[2]
				{
					new Response("Yes.", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4018")),
					new Response("That's", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4020"))
				};
				l.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4021"), responses, "ClubCard");
			}
			else if (canTalk() && CurrentDialogue.Count > 0)
			{
				if (!base.Name.Contains("King") && who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
				{
					if (who.IsLocalPlayer)
					{
						tryToReceiveActiveObject(who);
					}
					else
					{
						faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
					}
					return true;
				}
				if (CurrentDialogue.Count >= 1 || endOfRouteMessage.Value != null || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this)))
				{
					if (setTemporaryMessages(who))
					{
						Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
						return false;
					}
					if (Sprite.Texture.Bounds.Height > 32)
					{
						faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
					}
					if (who.IsLocalPlayer)
					{
						grantConversationFriendship(who);
						if (!reacting_to_shorts)
						{
							Game1.drawDialogue(this);
							return true;
						}
					}
				}
				else if (!doingEndOfRouteAnimation)
				{
					try
					{
						if (who.friendshipData.ContainsKey(base.Name))
						{
							faceTowardFarmerForPeriod(who.friendshipData[base.Name].Points / 125 * 1000 + 1000, 4, faceAway: false, who);
						}
					}
					catch (Exception)
					{
					}
					if (Game1.random.NextDouble() < 0.1)
					{
						doEmote(8);
					}
				}
			}
			else if (canTalk() && !Game1.game1.wasAskedLeoMemory && Game1.CurrentEvent == null && (string)name == "Leo" && base.currentLocation != null && (base.currentLocation.NameOrUniqueName == "LeoTreeHouse" || base.currentLocation.NameOrUniqueName == "Mountain") && Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved") && GetUnseenLeoEvent().HasValue && CanRevisitLeoMemory(GetUnseenLeoEvent()))
			{
				Game1.drawDialogue(this, Game1.content.LoadString("Strings\\Characters:Leo_Memory"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(AskLeoMemoryPrompt));
			}
			else
			{
				if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
				{
					if (base.Name.Equals("Bouncer"))
					{
						return true;
					}
					tryToReceiveActiveObject(who);
					faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
					return true;
				}
				if (base.Name.Equals("Krobus"))
				{
					if (l is Sewer)
					{
						Game1.activeClickableMenu = new ShopMenu((l as Sewer).getShadowShopStock(), 0, "Krobus", (l as Sewer).onShopPurchase);
						return true;
					}
				}
				else if (base.Name.Equals("Dwarf") && who.canUnderstandDwarves && l is Mine)
				{
					Game1.activeClickableMenu = new ShopMenu(Utility.getDwarfShopStock(), 0, "Dwarf");
					return true;
				}
			}
			if (reacting_to_shorts)
			{
				if (yJumpVelocity != 0f || Sprite.CurrentAnimation != null)
				{
					return true;
				}
				if (base.Name.Equals("Lewis"))
				{
					faceTowardFarmerForPeriod(1000, 3, faceAway: false, who);
					jump();
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(26, 1000, secondaryArm: false, flip: false, delegate
						{
							doEmote(12);
						}, behaviorAtEndOfFrame: true)
					});
					Sprite.loop = false;
					shakeTimer = 1000;
					l.playSound("batScreech");
				}
				else if (base.Name.Equals("Marnie"))
				{
					faceTowardFarmerForPeriod(1000, 3, faceAway: false, who);
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(33, 150, secondaryArm: false, flip: false, delegate
						{
							l.playSound("dustMeep");
						}),
						new FarmerSprite.AnimationFrame(34, 180),
						new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
						{
							l.playSound("dustMeep");
						}),
						new FarmerSprite.AnimationFrame(34, 180),
						new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
						{
							l.playSound("dustMeep");
						}),
						new FarmerSprite.AnimationFrame(34, 180),
						new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
						{
							l.playSound("dustMeep");
						}),
						new FarmerSprite.AnimationFrame(34, 180)
					});
					Sprite.loop = false;
				}
				return true;
			}
			if (setTemporaryMessages(who))
			{
				return false;
			}
			if (((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation) && endOfRouteMessage.Value != null)
			{
				Game1.drawDialogue(this);
				return true;
			}
			return false;
		}

		public void grantConversationFriendship(Farmer who, int amount = 20)
		{
			if (!base.Name.Contains("King") && !who.hasPlayerTalkedToNPC(base.Name) && who.friendshipData.ContainsKey(base.Name))
			{
				who.friendshipData[base.Name].TalkedToToday = true;
				Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
				if (!isDivorcedFrom(who))
				{
					who.changeFriendship(amount, this);
				}
			}
		}

		public virtual void AskLeoMemoryPrompt()
		{
			GameLocation i = base.currentLocation;
			Response[] responses = new Response[2]
			{
				new Response("Yes", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_Yes")),
				new Response("No", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_No"))
			};
			string question = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Characters:Leo_Memory_" + GetUnseenLeoEvent().Value.Value);
			if (question == null)
			{
				question = "";
			}
			i.createQuestionDialogue(question, responses, OnLeoMemoryResponse, this);
		}

		public bool CanRevisitLeoMemory(KeyValuePair<string, int>? event_data)
		{
			if (!event_data.HasValue)
			{
				return false;
			}
			string location_name = event_data.Value.Key;
			int event_id = event_data.Value.Value;
			Dictionary<string, string> location_events2 = null;
			try
			{
				location_events2 = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + location_name);
			}
			catch (Exception)
			{
				return false;
			}
			if (location_events2 == null)
			{
				return false;
			}
			foreach (string key in location_events2.Keys)
			{
				if (key.Split('/')[0] == event_id.ToString())
				{
					GameLocation location = Game1.getLocationFromName(location_name);
					string event_key3 = key;
					event_key3 = event_key3.Replace("/e 1039573", "");
					event_key3 = event_key3.Replace("/Hl leoMoved", "");
					if (location != null && location.checkEventPrecondition(event_key3) != -1)
					{
						return true;
					}
				}
			}
			return false;
		}

		public KeyValuePair<string, int>? GetUnseenLeoEvent()
		{
			new List<int>();
			if (!Game1.player.eventsSeen.Contains(6497423))
			{
				return new KeyValuePair<string, int>("IslandWest", 6497423);
			}
			if (!Game1.player.eventsSeen.Contains(6497421))
			{
				return new KeyValuePair<string, int>("IslandNorth", 6497421);
			}
			if (!Game1.player.eventsSeen.Contains(6497428))
			{
				return new KeyValuePair<string, int>("IslandSouth", 6497428);
			}
			return null;
		}

		public void OnLeoMemoryResponse(Farmer who, string whichAnswer)
		{
			if (whichAnswer.ToLower() == "yes")
			{
				KeyValuePair<string, int>? event_data = GetUnseenLeoEvent();
				if (event_data.HasValue)
				{
					string location_name = event_data.Value.Key;
					int event_id = event_data.Value.Value;
					Dictionary<string, string> location_events = null;
					try
					{
						location_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + location_name);
					}
					catch (Exception)
					{
						return;
					}
					if (location_events != null)
					{
						int old_x = Game1.player.getTileX();
						int old_y = Game1.player.getTileY();
						string old_location = Game1.player.currentLocation.NameOrUniqueName;
						int old_direction = Game1.player.FacingDirection;
						foreach (string key in location_events.Keys)
						{
							if (key.Split('/')[0] == event_id.ToString())
							{
								LocationRequest location_request = Game1.getLocationRequest(location_name);
								Game1.warpingForForcedRemoteEvent = true;
								location_request.OnWarp += delegate
								{
									Event @event = new Event(location_events[key], event_id);
									@event.isMemory = true;
									@event.setExitLocation(old_location, old_x, old_y);
									Game1.player.orientationBeforeEvent = old_direction;
									location_request.Location.currentEvent = @event;
									location_request.Location.startEvent(@event);
									Game1.warpingForForcedRemoteEvent = false;
								};
								int x = 8;
								int y = 8;
								Utility.getDefaultWarpLocation(location_request.Name, ref x, ref y);
								Game1.warpFarmer(location_request, x, y, Game1.player.FacingDirection);
							}
						}
					}
				}
			}
			else
			{
				Game1.game1.wasAskedLeoMemory = true;
			}
		}

		public bool isDivorcedFrom(Farmer who)
		{
			if (who == null)
			{
				return false;
			}
			if (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDivorced())
			{
				return true;
			}
			return false;
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (movementPause <= 0)
			{
				faceTowardFarmerTimer = 0;
				base.MovePosition(time, viewport, currentLocation);
			}
		}

		public GameLocation getHome()
		{
			if (isMarried() && getSpouse() != null)
			{
				return Utility.getHomeOfFarmer(getSpouse());
			}
			return Game1.getLocationFromName(defaultMap);
		}

		public override bool canPassThroughActionTiles()
		{
			return true;
		}

		public virtual void behaviorOnFarmerPushing()
		{
		}

		public virtual void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
		{
			if (Sprite != null && Sprite.CurrentAnimation == null && Sprite.SourceRect.Height > 32)
			{
				Sprite.SpriteWidth = 16;
				Sprite.SpriteHeight = 16;
				Sprite.currentFrame = 0;
			}
		}

		public virtual void behaviorOnLocalFarmerLocationEntry(GameLocation location)
		{
			shouldPlayRobinHammerAnimation.CancelInterpolation();
			shouldPlaySpousePatioAnimation.CancelInterpolation();
			shouldWearIslandAttire.CancelInterpolation();
			isSleeping.CancelInterpolation();
			doingEndOfRouteAnimation.CancelInterpolation();
			if (doingEndOfRouteAnimation.Value)
			{
				_skipRouteEndIntro = true;
			}
			else
			{
				_skipRouteEndIntro = false;
			}
			endOfRouteBehaviorName.CancelInterpolation();
			if (isSleeping.Value)
			{
				drawOffset.CancelInterpolation();
				position.Field.CancelInterpolation();
			}
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
			lastPosition = base.Position;
			if (DirectionsToNewLocation != null && !Game1.newDay)
			{
				Point standing = getStandingXY();
				if (standing.X < -64 || standing.X > location.map.DisplayWidth + 64 || standing.Y < -64 || standing.Y > location.map.DisplayHeight + 64)
				{
					IsWalkingInSquare = false;
					Game1.warpCharacter(this, DefaultMap, DefaultPosition);
					location.characters.Remove(this);
				}
				else if (IsWalkingInSquare)
				{
					returnToEndPoint();
					MovePosition(time, Game1.viewport, location);
				}
				else
				{
					if (!followSchedule)
					{
						return;
					}
					MovePosition(time, Game1.viewport, location);
					Warp tmpWarp = location.isCollidingWithWarp(GetBoundingBox(), this);
					PropertyValue door = null;
					location.map.GetLayer("Buildings").PickTile(nextPositionPoint(), Game1.viewport.Size)?.Properties.TryGetValue("Action", out door);
					string[] isDoor = door?.ToString().Split(Utility.CharSpace);
					if (isDoor == null)
					{
						standing = getStandingXY();
						location.map.GetLayer("Buildings").PickTile(new Location(standing.X, standing.Y), Game1.viewport.Size)?.Properties.TryGetValue("Action", out door);
						isDoor = door?.ToString().Split(Utility.CharSpace);
					}
					if (tmpWarp != null)
					{
						if (location is BusStop && tmpWarp.TargetName.Equals("Farm"))
						{
							Point tmp2 = ((isMarried() ? (getHome() as FarmHouse) : Game1.getLocationFromName(getSpouse().homeLocation.Value)) as FarmHouse).getEntryLocation();
							tmpWarp = new Warp(tmpWarp.X, tmpWarp.Y, getSpouse().homeLocation.Value, tmp2.X, tmp2.Y, flipFarmer: false);
						}
						else if (location is FarmHouse && tmpWarp.TargetName.Equals("Farm"))
						{
							tmpWarp = new Warp(tmpWarp.X, tmpWarp.Y, "BusStop", 0, 23, flipFarmer: false);
						}
						Game1.warpCharacter(this, tmpWarp.TargetName, new Vector2(tmpWarp.TargetX * 64, tmpWarp.TargetY * 64 - Sprite.getHeight() / 2 - 16));
						location.characters.Remove(this);
					}
					else if (isDoor != null && isDoor.Length >= 1 && isDoor[0].Contains("Warp"))
					{
						Game1.warpCharacter(this, isDoor[3], new Vector2(Convert.ToInt32(isDoor[1]), Convert.ToInt32(isDoor[2])));
						if (Game1.currentLocation.name.Equals(location.name) && Utility.isOnScreen(getStandingPosition(), 192) && !Game1.eventUp)
						{
							location.playSound("doorClose", NetAudio.SoundContext.NPC);
						}
						location.characters.Remove(this);
					}
					else if (isDoor != null && isDoor.Length >= 1 && isDoor[0].Contains("Door"))
					{
						location.openDoor(new Location(nextPositionPoint().X / 64, nextPositionPoint().Y / 64), Game1.player.currentLocation.Equals(location));
					}
					else
					{
						if (location.map.GetLayer("Paths") == null)
						{
							return;
						}
						standing = getStandingXY();
						Tile tmp = location.map.GetLayer("Paths").PickTile(new Location(standing.X, standing.Y), Game1.viewport.Size);
						Microsoft.Xna.Framework.Rectangle boundingbox = GetBoundingBox();
						boundingbox.Inflate(2, 2);
						if (tmp == null || !new Microsoft.Xna.Framework.Rectangle(standing.X - standing.X % 64, standing.Y - standing.Y % 64, 64, 64).Contains(boundingbox))
						{
							return;
						}
						switch (tmp.TileIndex)
						{
						case 5:
						case 6:
							break;
						case 0:
							if (getDirection() == 3)
							{
								SetMovingOnlyUp();
							}
							else if (getDirection() == 2)
							{
								SetMovingOnlyRight();
							}
							break;
						case 1:
							if (getDirection() == 3)
							{
								SetMovingOnlyDown();
							}
							else if (getDirection() == 0)
							{
								SetMovingOnlyRight();
							}
							break;
						case 2:
							if (getDirection() == 1)
							{
								SetMovingOnlyDown();
							}
							else if (getDirection() == 0)
							{
								SetMovingOnlyLeft();
							}
							break;
						case 3:
							if (getDirection() == 1)
							{
								SetMovingOnlyUp();
							}
							else if (getDirection() == 2)
							{
								SetMovingOnlyLeft();
							}
							break;
						case 4:
							changeSchedulePathDirection();
							moveCharacterOnSchedulePath();
							break;
						case 7:
							ReachedEndPoint();
							break;
						}
					}
				}
			}
			else if (IsWalkingInSquare)
			{
				randomSquareMovement(time);
				MovePosition(time, Game1.viewport, location);
			}
		}

		public void facePlayer(Farmer who)
		{
			if ((int)facingDirectionBeforeSpeakingToPlayer == -1)
			{
				facingDirectionBeforeSpeakingToPlayer.Value = getFacingDirection();
			}
			faceDirection((who.FacingDirection + 2) % 4);
		}

		public void doneFacingPlayer(Farmer who)
		{
		}

		public virtual void UpdateFarmExploration(GameTime time, GameLocation location)
		{
			if (_farmActivities == null)
			{
				InitializeFarmActivities();
			}
			if (_currentFarmActivity != null)
			{
				if (_currentFarmActivity.IsPerformingActivity() && _currentFarmActivity.Update(time))
				{
					_currentFarmActivity.EndActivity();
					_currentFarmActivity = null;
				}
				return;
			}
			nextFarmActivityScan -= (float)time.ElapsedGameTime.TotalSeconds;
			if (nextFarmActivityScan <= 0f)
			{
				bool activity_found = false;
				if (FindFarmActivity())
				{
					activity_found = true;
				}
				if (!activity_found)
				{
					nextFarmActivityScan = 3f;
				}
			}
		}

		public virtual void InitializeFarmActivities()
		{
			_farmActivities = new List<FarmActivity>();
			_farmActivities.Add(new CropWatchActivity().Initialize(this));
			_farmActivities.Add(new FlowerWatchActivity().Initialize(this));
			_farmActivities.Add(new ArtifactSpotWatchActivity().Initialize(this, 0.5f));
			_farmActivities.Add(new TreeActivity().Initialize(this));
			_farmActivities.Add(new ClearingActivity().Initialize(this));
			_farmActivities.Add(new ShrineActivity().Initialize(this, 0.1f));
			_farmActivities.Add(new MailActivity().Initialize(this, 0.1f));
			_farmActivityWeightTotal = 0f;
			foreach (FarmActivity activity in _farmActivities)
			{
				_farmActivityWeightTotal += activity.weight;
			}
		}

		public virtual bool FindFarmActivity()
		{
			if (!(base.currentLocation is Farm))
			{
				return false;
			}
			Farm farm = base.currentLocation as Farm;
			float random = Utility.RandomFloat(0f, _farmActivityWeightTotal);
			FarmActivity found_activity = null;
			foreach (FarmActivity activity in _farmActivities)
			{
				random -= activity.weight;
				if (random <= 0f)
				{
					if (activity.AttemptActivity(farm))
					{
						found_activity = activity;
					}
					break;
				}
			}
			if (found_activity != null)
			{
				if (found_activity.IsTileBlockedFromSight(found_activity.activityPosition))
				{
					return false;
				}
				if (PathToOnFarm(Utility.Vector2ToPoint(found_activity.activityPosition), OnFinishPathForActivity))
				{
					_currentFarmActivity = found_activity;
					return true;
				}
			}
			return false;
		}

		public override void update(GameTime time, GameLocation location)
		{
			if (Game1.IsMasterGame && currentScheduleDelay > 0f)
			{
				currentScheduleDelay -= (float)time.ElapsedGameTime.TotalSeconds;
				if (currentScheduleDelay <= 0f)
				{
					currentScheduleDelay = -1f;
					checkSchedule(Game1.timeOfDay);
					currentScheduleDelay = 0f;
				}
			}
			removeHenchmanEvent.Poll();
			if (Game1.IsMasterGame && (bool)exploreFarm)
			{
				UpdateFarmExploration(time, location);
			}
			if (Game1.IsMasterGame && shouldWearIslandAttire.Value && (base.currentLocation == null || base.currentLocation.GetLocationContext() == GameLocation.LocationContext.Default))
			{
				shouldWearIslandAttire.Value = false;
			}
			if (_startedEndOfRouteBehavior == null && _finishingEndOfRouteBehavior == null && loadedEndOfRouteBehavior != endOfRouteBehaviorName.Value)
			{
				loadEndOfRouteBehavior(endOfRouteBehaviorName);
			}
			if (!currentlyDoingEndOfRouteAnimation && string.Equals(loadedEndOfRouteBehavior, endOfRouteBehaviorName.Value, StringComparison.Ordinal) && (bool)doingEndOfRouteAnimation)
			{
				reallyDoAnimationAtEndOfScheduleRoute();
			}
			else if (currentlyDoingEndOfRouteAnimation && !doingEndOfRouteAnimation)
			{
				finishEndOfRouteAnimation();
			}
			currentlyDoingEndOfRouteAnimation = doingEndOfRouteAnimation;
			if (shouldWearIslandAttire.Value && !isWearingIslandAttire)
			{
				wearIslandAttire();
			}
			else if (!shouldWearIslandAttire.Value && isWearingIslandAttire)
			{
				wearNormalClothes();
			}
			bool isSleeping = this.isSleeping.Value;
			if (isSleeping && !isPlayingSleepingAnimation)
			{
				playSleepingAnimation();
			}
			else if (!isSleeping && isPlayingSleepingAnimation)
			{
				Sprite.StopAnimation();
				isPlayingSleepingAnimation = false;
			}
			bool shouldPlayRobinHammerAnimation = this.shouldPlayRobinHammerAnimation.Value;
			if (shouldPlayRobinHammerAnimation && !isPlayingRobinHammerAnimation)
			{
				doPlayRobinHammerAnimation();
				isPlayingRobinHammerAnimation = true;
			}
			else if (!shouldPlayRobinHammerAnimation && isPlayingRobinHammerAnimation)
			{
				Sprite.StopAnimation();
				isPlayingRobinHammerAnimation = false;
			}
			bool shouldPlaySpousePatioAnimation = this.shouldPlaySpousePatioAnimation.Value;
			if (shouldPlaySpousePatioAnimation && !isPlayingSpousePatioAnimation)
			{
				doPlaySpousePatioAnimation();
				isPlayingSpousePatioAnimation = true;
			}
			else if (!shouldPlaySpousePatioAnimation && isPlayingSpousePatioAnimation)
			{
				Sprite.StopAnimation();
				isPlayingSpousePatioAnimation = false;
			}
			if (returningToEndPoint)
			{
				returnToEndPoint();
				MovePosition(time, Game1.viewport, location);
			}
			else if (temporaryController != null)
			{
				if (temporaryController.update(time))
				{
					bool nPCSchedule = temporaryController.NPCSchedule;
					temporaryController = null;
					if (nPCSchedule)
					{
						currentScheduleDelay = -1f;
						checkSchedule(Game1.timeOfDay);
						currentScheduleDelay = 0f;
					}
				}
				updateEmote(time);
			}
			else
			{
				base.update(time, location);
			}
			if (textAboveHeadTimer > 0)
			{
				if (textAboveHeadPreTimer > 0)
				{
					textAboveHeadPreTimer -= time.ElapsedGameTime.Milliseconds;
				}
				else
				{
					textAboveHeadTimer -= time.ElapsedGameTime.Milliseconds;
					if (textAboveHeadTimer > 500)
					{
						textAboveHeadAlpha = Math.Min(1f, textAboveHeadAlpha + 0.1f);
					}
					else
					{
						textAboveHeadAlpha = Math.Max(0f, textAboveHeadAlpha - 0.04f);
					}
				}
			}
			if (isWalkingInSquare && !returningToEndPoint)
			{
				randomSquareMovement(time);
			}
			if (Sprite != null && Sprite.CurrentAnimation != null && !Game1.eventUp && Game1.IsMasterGame && Sprite.animateOnce(time))
			{
				Sprite.CurrentAnimation = null;
			}
			if (movementPause > 0 && (!Game1.dialogueUp || controller != null))
			{
				freezeMotion = true;
				movementPause -= time.ElapsedGameTime.Milliseconds;
				if (movementPause <= 0)
				{
					freezeMotion = false;
				}
			}
			if (shakeTimer > 0)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (lastPosition.Equals(base.Position))
			{
				timerSinceLastMovement += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				timerSinceLastMovement = 0f;
			}
			if ((bool)swimming)
			{
				yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
				float oldSwimTimer = swimTimer;
				swimTimer -= time.ElapsedGameTime.Milliseconds;
				if (timerSinceLastMovement == 0f)
				{
					if (oldSwimTimer > 400f && swimTimer <= 400f && location.Equals(Game1.currentLocation))
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, getStandingY() - 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
						location.playSound("slosh", NetAudio.SoundContext.NPC);
					}
					if (swimTimer < 0f)
					{
						swimTimer = 800f;
						if (location.Equals(Game1.currentLocation))
						{
							location.playSound("slosh", NetAudio.SoundContext.NPC);
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, getStandingY() - 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
						}
					}
				}
				else if (swimTimer < 0f)
				{
					swimTimer = 100f;
				}
			}
			if (Game1.IsMasterGame)
			{
				isMovingOnPathFindPath.Value = (controller != null && temporaryController != null);
			}
		}

		public virtual void wearIslandAttire()
		{
			try
			{
				Sprite.LoadTexture("Characters\\" + getTextureNameForCharacter(name.Value) + "_Beach");
			}
			catch (ContentLoadException)
			{
				Sprite.LoadTexture("Characters\\" + getTextureNameForCharacter(name.Value));
			}
			isWearingIslandAttire = true;
			resetPortrait();
		}

		public virtual void wearNormalClothes()
		{
			Sprite.LoadTexture("Characters\\" + getTextureNameForCharacter(name.Value));
			isWearingIslandAttire = false;
			resetPortrait();
		}

		public virtual void performTenMinuteUpdate(int timeOfDay, GameLocation l)
		{
			if (Game1.eventUp)
			{
				return;
			}
			if (Game1.random.NextDouble() < 0.1 && Dialogue != null && Dialogue.ContainsKey(l.name + "_Ambient"))
			{
				string[] split = Dialogue[l.name + "_Ambient"].Split('/');
				int extraTime = Game1.random.Next(4) * 1000;
				showTextAboveHead(split[Game1.random.Next(split.Length)], -1, 2, 3000, extraTime);
			}
			else
			{
				if (!isMoving() || !l.isOutdoors || timeOfDay >= 1800 || !(Game1.random.NextDouble() < 0.3 + ((SocialAnxiety == 0) ? 0.25 : ((SocialAnxiety != 1) ? 0.0 : ((Manners == 2) ? (-1.0) : (-0.2))))) || (Age == 1 && (Manners != 1 || SocialAnxiety != 0)) || isMarried())
				{
					return;
				}
				Character c = Utility.isThereAFarmerOrCharacterWithinDistance(getTileLocation(), 4, l);
				if (!c.Name.Equals(base.Name) && !(c is Horse))
				{
					Dictionary<string, string> dispositions = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\NPCDispositions");
					if (dispositions.ContainsKey(base.Name) && !dispositions[base.Name].Split('/')[9].Contains(c.Name) && isFacingToward(c.getTileLocation()))
					{
						sayHiTo(c);
					}
				}
			}
		}

		public void sayHiTo(Character c)
		{
			if (getHi(c.displayName) != null)
			{
				showTextAboveHead(getHi(c.displayName));
				if (c is NPC && Game1.random.NextDouble() < 0.66 && (c as NPC).getHi(base.displayName) != null)
				{
					(c as NPC).showTextAboveHead((c as NPC).getHi(base.displayName), -1, 2, 3000, 1000 + Game1.random.Next(500));
				}
			}
		}

		public string getHi(string nameToGreet)
		{
			if (Age == 2)
			{
				if (SocialAnxiety != 1)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4059");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4058");
			}
			if (SocialAnxiety == 1)
			{
				if (!(Game1.random.NextDouble() < 0.5))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4061");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4060");
			}
			if (SocialAnxiety == 0)
			{
				if (!(Game1.random.NextDouble() < 0.33))
				{
					if (!(Game1.random.NextDouble() < 0.5))
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4068", nameToGreet);
					}
					return ((Game1.timeOfDay < 1200) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4063") : ((Game1.timeOfDay < 1700) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4064") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4065"))) + ", " + Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4066", nameToGreet);
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4062");
			}
			if (!(Game1.random.NextDouble() < 0.33))
			{
				if (!(Game1.random.NextDouble() < 0.5))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4072");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4071", nameToGreet);
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4060");
		}

		public bool isFacingToward(Vector2 tileLocation)
		{
			switch (FacingDirection)
			{
			case 0:
				return (float)getTileY() > tileLocation.Y;
			case 1:
				return (float)getTileX() < tileLocation.X;
			case 2:
				return (float)getTileY() < tileLocation.Y;
			case 3:
				return (float)getTileX() > tileLocation.X;
			default:
				return false;
			}
		}

		public virtual void arriveAt(GameLocation l)
		{
			if (!Game1.eventUp && Game1.random.NextDouble() < 0.5 && Dialogue != null && Dialogue.ContainsKey(l.name + "_Entry"))
			{
				string[] split = Dialogue[l.name + "_Entry"].Split('/');
				showTextAboveHead(split[Game1.random.Next(split.Length)]);
			}
		}

		public override void Halt()
		{
			base.Halt();
			shouldPlaySpousePatioAnimation.Value = false;
			isPlayingSleepingAnimation = false;
			isCharging = false;
			base.speed = 2;
			base.addedSpeed = 0;
			if (isSleeping.Value)
			{
				playSleepingAnimation();
				Sprite.UpdateSourceRect();
			}
		}

		public void addExtraDialogues(string dialogues)
		{
			if (updatedDialogueYet)
			{
				if (dialogues != null)
				{
					CurrentDialogue.Push(new Dialogue(dialogues, this));
				}
			}
			else
			{
				extraDialogueMessageToAddThisMorning = dialogues;
			}
		}

		public void PerformDivorce()
		{
			reloadDefaultLocation();
			Game1.warpCharacter(this, defaultMap, DefaultPosition / 64f);
		}

		public string tryToGetMarriageSpecificDialogueElseReturnDefault(string dialogueKey, string defaultMessage = "")
		{
			Dictionary<string, string> marriageDialogues2 = null;
			try
			{
				marriageDialogues2 = Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + GetDialogueSheetName());
			}
			catch (Exception)
			{
			}
			if (marriageDialogues2 != null && marriageDialogues2.ContainsKey(dialogueKey))
			{
				return marriageDialogues2[dialogueKey];
			}
			marriageDialogues2 = Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue");
			if (marriageDialogues2 != null && marriageDialogues2.ContainsKey(dialogueKey))
			{
				return marriageDialogues2[dialogueKey];
			}
			return defaultMessage;
		}

		public void resetCurrentDialogue()
		{
			CurrentDialogue = null;
			shouldSayMarriageDialogue.Value = false;
			currentMarriageDialogue.Clear();
		}

		private Stack<Dialogue> loadCurrentDialogue()
		{
			updatedDialogueYet = true;
			Friendship friends;
			int heartLevel = Game1.player.friendshipData.TryGetValue(base.Name, out friends) ? (friends.Points / 250) : 0;
			Stack<Dialogue> currentDialogue = new Stack<Dialogue>();
			Random r = new Random((int)(Game1.stats.DaysPlayed * 77) + (int)Game1.uniqueIDForThisGame / 2 + 2 + (int)defaultPosition.X * 77 + (int)defaultPosition.Y * 777);
			if (r.NextDouble() < 0.025 && heartLevel >= 1)
			{
				Dictionary<string, string> npcDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				if (npcDispositions.TryGetValue(base.Name, out string disposition))
				{
					string[] relatives = disposition.Split('/')[9].Split(' ');
					if (relatives.Length > 1)
					{
						int index = r.Next(relatives.Length / 2) * 2;
						string relativeName = relatives[index];
						string relativeDisplayName = relativeName;
						if (LocalizedContentManager.CurrentLanguageCode != 0 && Game1.getCharacterFromName(relativeName) != null)
						{
							relativeDisplayName = Game1.getCharacterFromName(relativeName).displayName;
						}
						string relativeTitle = relatives[index + 1].Replace("'", "").Replace("_", " ");
						string relativeProps;
						bool relativeIsMale = npcDispositions.TryGetValue(relativeName, out relativeProps) && relativeProps.Split('/')[4].Equals("male");
						Dictionary<string, string> npcGiftTastes = Game1.content.Load<Dictionary<string, string>>("Data\\NPCGiftTastes");
						if (npcGiftTastes.ContainsKey(relativeName))
						{
							string itemName = null;
							string nameAndTitle = (relativeTitle.Length <= 2 || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja) ? relativeDisplayName : (relativeIsMale ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4079", relativeTitle) : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4080", relativeTitle));
							string message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4083", nameAndTitle);
							int item;
							if (r.NextDouble() < 0.5)
							{
								string[] loveItems = npcGiftTastes[relativeName].Split('/')[1].Split(' ');
								item = Convert.ToInt32(loveItems[r.Next(loveItems.Length)]);
								if (base.Name == "Penny" && relativeName == "Pam")
								{
									while (item == 303 || item == 346 || item == 348 || item == 459)
									{
										item = Convert.ToInt32(loveItems[r.Next(loveItems.Length)]);
									}
								}
								if (Game1.objectInformation.TryGetValue(item, out string itemDetails))
								{
									itemName = itemDetails.Split('/')[4];
									message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4084", itemName);
									if (Age == 2)
									{
										message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4086", relativeDisplayName, itemName) + (relativeIsMale ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4088") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4089"));
									}
									else
									{
										switch (r.Next(5))
										{
										case 0:
											message = Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4091", nameAndTitle, itemName);
											break;
										case 1:
											message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4094", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4097", nameAndTitle, itemName));
											break;
										case 2:
											message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4100", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4103", nameAndTitle, itemName));
											break;
										case 3:
											message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4106", nameAndTitle, itemName);
											break;
										}
										if (r.NextDouble() < 0.65)
										{
											switch (r.Next(5))
											{
											case 0:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4109") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4111"));
												break;
											case 1:
												message += ((!relativeIsMale) ? ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4115") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4116")) : ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4113") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4114")));
												break;
											case 2:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4118") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4120"));
												break;
											case 3:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4125");
												break;
											case 4:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4126") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128"));
												break;
											}
											if (relativeName.Equals("Abigail") && r.NextDouble() < 0.5)
											{
												message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128", relativeDisplayName, itemName);
											}
										}
									}
								}
							}
							else
							{
								try
								{
									item = Convert.ToInt32(npcGiftTastes[relativeName].Split('/')[7].Split(' ')[r.Next(npcGiftTastes[relativeName].Split('/')[7].Split(' ').Length)]);
								}
								catch (Exception)
								{
									item = Convert.ToInt32(npcGiftTastes["Universal_Hate"].Split(' ')[r.Next(npcGiftTastes["Universal_Hate"].Split(' ').Length)]);
								}
								if (Game1.objectInformation.ContainsKey(item))
								{
									itemName = Game1.objectInformation[item].Split('/')[4];
									message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4135", itemName, Lexicon.getRandomNegativeFoodAdjective()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4138", itemName, Lexicon.getRandomNegativeFoodAdjective()));
									if (Age == 2)
									{
										message = (relativeIsMale ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4141", relativeDisplayName, itemName) : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4144", relativeDisplayName, itemName));
									}
									else
									{
										switch (r.Next(4))
										{
										case 0:
											message = ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4146") : "") + Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4147", nameAndTitle, itemName);
											break;
										case 1:
											message = ((!relativeIsMale) ? ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4153", nameAndTitle, itemName) : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4154", nameAndTitle, itemName)) : ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4149", nameAndTitle, itemName) : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4152", nameAndTitle, itemName)));
											break;
										case 2:
											message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4161", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4164", nameAndTitle, itemName));
											break;
										}
										if (r.NextDouble() < 0.65)
										{
											switch (r.Next(5))
											{
											case 0:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4170");
												break;
											case 1:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4171");
												break;
											case 2:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4172") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4174"));
												break;
											case 3:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4176") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4178"));
												break;
											case 4:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4180");
												break;
											}
											if (base.Name.Equals("Lewis") && r.NextDouble() < 0.5)
											{
												message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4182", relativeDisplayName, itemName);
											}
										}
									}
								}
							}
							if (itemName != null)
							{
								if (Game1.getCharacterFromName(relativeName) != null)
								{
									message = message + "%revealtaste" + relativeName + item;
								}
								currentDialogue.Clear();
								if (message.Length > 0)
								{
									try
									{
										message = message.Substring(0, 1).ToUpper() + message.Substring(1, message.Length - 1);
									}
									catch (Exception)
									{
									}
								}
								currentDialogue.Push(new Dialogue(message, this));
								return currentDialogue;
							}
						}
					}
				}
			}
			if (Dialogue != null && Dialogue.Count != 0)
			{
				string currentDialogueStr = "";
				currentDialogue.Clear();
				if (Game1.player.spouse != null && Game1.player.spouse.Contains(base.Name))
				{
					if (Game1.player.spouse.Equals(base.Name) && Game1.player.isEngaged())
					{
						currentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue")[base.Name + r.Next(2)], this));
					}
					else if (!Game1.newDay && marriageDefaultDialogue.Value != null && !shouldSayMarriageDialogue.Value)
					{
						currentDialogue.Push(marriageDefaultDialogue.Value.GetDialogue(this));
						marriageDefaultDialogue.Value = null;
					}
				}
				else if (idForClones == -1)
				{
					if (Game1.player.friendshipData.ContainsKey(base.Name) && Game1.player.friendshipData[base.Name].IsDivorced())
					{
						try
						{
							currentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + GetDialogueSheetName())["divorced"], this));
							return currentDialogue;
						}
						catch (Exception)
						{
						}
					}
					if (Game1.isRaining && r.NextDouble() < 0.5 && (base.currentLocation == null || base.currentLocation.GetLocationContext() == GameLocation.LocationContext.Default) && (!base.Name.Equals("Krobus") || !(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")) && (!base.Name.Equals("Penny") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")))
					{
						try
						{
							currentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\rainy")[GetDialogueSheetName()], this));
							return currentDialogue;
						}
						catch (Exception)
						{
						}
					}
					Dialogue d = tryToRetrieveDialogue(Game1.currentSeason + "_", heartLevel);
					if (d == null)
					{
						d = tryToRetrieveDialogue("", heartLevel);
					}
					if (d != null)
					{
						currentDialogue.Push(d);
					}
				}
				else
				{
					Dialogue.TryGetValue(string.Concat(idForClones), out currentDialogueStr);
					currentDialogue.Push(new Dialogue(currentDialogueStr, this));
				}
			}
			else if (base.Name.Equals("Bouncer"))
			{
				currentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4192"), this));
			}
			if (extraDialogueMessageToAddThisMorning != null)
			{
				currentDialogue.Push(new Dialogue(extraDialogueMessageToAddThisMorning, this));
			}
			return currentDialogue;
		}

		public bool checkForNewCurrentDialogue(int heartLevel, bool noPreface = false)
		{
			string eventMessageKey2 = "";
			foreach (string s in Game1.player.activeDialogueEvents.Keys)
			{
				if (Dialogue.ContainsKey(s))
				{
					eventMessageKey2 = s;
					if (!eventMessageKey2.Equals("") && !Game1.player.mailReceived.Contains(base.Name + "_" + eventMessageKey2))
					{
						CurrentDialogue.Clear();
						CurrentDialogue.Push(new Dialogue(Dialogue[eventMessageKey2], this));
						if (!s.Contains("dumped"))
						{
							Game1.player.mailReceived.Add(base.Name + "_" + eventMessageKey2);
						}
						return true;
					}
				}
			}
			string preface = (!Game1.currentSeason.Equals("spring") && !noPreface) ? Game1.currentSeason : "";
			if (Dialogue.ContainsKey(preface + (string)Game1.currentLocation.name + "_" + getTileX() + "_" + getTileY()))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[preface + (string)Game1.currentLocation.name + "_" + getTileX() + "_" + getTileY()], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (Dialogue.ContainsKey(preface + Game1.currentLocation.name + "_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[preface + Game1.currentLocation.name + "_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 10 && Dialogue.ContainsKey(preface + Game1.currentLocation.name + "10"))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[preface + Game1.currentLocation.name + "10"], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 8 && Dialogue.ContainsKey(preface + Game1.currentLocation.name + "8"))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[preface + Game1.currentLocation.name + "8"], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 6 && Dialogue.ContainsKey(preface + Game1.currentLocation.name + "6"))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[preface + Game1.currentLocation.name + "6"], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 4 && Dialogue.ContainsKey(preface + Game1.currentLocation.name + "4"))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[preface + Game1.currentLocation.name + "4"], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 2 && Dialogue.ContainsKey(preface + Game1.currentLocation.name + "2"))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[preface + Game1.currentLocation.name + "2"], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (Dialogue.ContainsKey(preface + Game1.currentLocation.name))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[preface + Game1.currentLocation.name], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			return false;
		}

		public Dialogue tryToRetrieveDialogue(string preface, int heartLevel, string appendToEnd = "")
		{
			int year = Game1.year;
			if (Game1.year > 2)
			{
				year = 2;
			}
			if (Game1.player.spouse != null && Game1.player.spouse.Length > 0 && appendToEnd.Equals(""))
			{
				Dialogue s = tryToRetrieveDialogue(preface, heartLevel, "_inlaw_" + Game1.player.spouse);
				if (s != null)
				{
					return s;
				}
			}
			string day_name = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (base.Name == "Pierre" && Game1.isLocationAccessible("CommunityCenter") && day_name == "Wed")
			{
				day_name = "Sat";
			}
			if (Dialogue.ContainsKey(preface + Game1.dayOfMonth + appendToEnd) && year == 1)
			{
				return new Dialogue(Dialogue[preface + Game1.dayOfMonth + appendToEnd], this);
			}
			if (Dialogue.ContainsKey(preface + Game1.dayOfMonth + "_" + year + appendToEnd))
			{
				return new Dialogue(Dialogue[preface + Game1.dayOfMonth + "_" + year + appendToEnd], this);
			}
			if (heartLevel >= 10 && Dialogue.ContainsKey(preface + day_name + "10" + appendToEnd))
			{
				if (!Dialogue.ContainsKey(preface + day_name + "10_" + year + appendToEnd))
				{
					return new Dialogue(Dialogue[preface + day_name + "10" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + day_name + "10_" + year + appendToEnd], this);
			}
			if (heartLevel >= 8 && Dialogue.ContainsKey(preface + day_name + "8" + appendToEnd))
			{
				if (!Dialogue.ContainsKey(preface + day_name + "8_" + year + appendToEnd))
				{
					return new Dialogue(Dialogue[preface + day_name + "8" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + day_name + "8_" + year + appendToEnd], this);
			}
			if (heartLevel >= 6 && Dialogue.ContainsKey(preface + day_name + "6" + appendToEnd))
			{
				if (!Dialogue.ContainsKey(preface + day_name + "6_" + year))
				{
					return new Dialogue(Dialogue[preface + day_name + "6" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + day_name + "6_" + year + appendToEnd], this);
			}
			if (heartLevel >= 4 && Dialogue.ContainsKey(preface + day_name + "4" + appendToEnd))
			{
				if (preface == "fall_" && day_name == "Mon" && base.Name.Equals("Penny") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					if (!Dialogue.ContainsKey(preface + day_name + "_" + year + appendToEnd))
					{
						return new Dialogue(Dialogue["fall_Mon"], this);
					}
					return new Dialogue(Dialogue[preface + day_name + "_" + year + appendToEnd], this);
				}
				if (!Dialogue.ContainsKey(preface + day_name + "4_" + year))
				{
					return new Dialogue(Dialogue[preface + day_name + "4" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + day_name + "4_" + year + appendToEnd], this);
			}
			if (heartLevel >= 2 && Dialogue.ContainsKey(preface + day_name + "2" + appendToEnd))
			{
				if (!Dialogue.ContainsKey(preface + day_name + "2_" + year))
				{
					return new Dialogue(Dialogue[preface + day_name + "2" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + day_name + "2_" + year + appendToEnd], this);
			}
			if (Dialogue.ContainsKey(preface + day_name + appendToEnd))
			{
				if (base.Name.Equals("Caroline") && Game1.isLocationAccessible("CommunityCenter") && preface == "summer_" && day_name == "Mon")
				{
					return new Dialogue(Dialogue["summer_Wed"], this);
				}
				if (!Dialogue.ContainsKey(preface + day_name + "_" + year + appendToEnd))
				{
					return new Dialogue(Dialogue[preface + day_name + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + day_name + "_" + year + appendToEnd], this);
			}
			return null;
		}

		public void clearSchedule()
		{
			schedule = null;
		}

		public void checkSchedule(int timeOfDay)
		{
			if (currentScheduleDelay == 0f && scheduleDelaySeconds > 0f)
			{
				currentScheduleDelay = scheduleDelaySeconds;
			}
			else
			{
				if (returningToEndPoint)
				{
					return;
				}
				updatedDialogueYet = false;
				extraDialogueMessageToAddThisMorning = null;
				if (ignoreScheduleToday || schedule == null)
				{
					return;
				}
				SchedulePathDescription possibleNewDirections = null;
				if (lastAttemptedSchedule < timeOfDay)
				{
					lastAttemptedSchedule = timeOfDay;
					schedule.TryGetValue(timeOfDay, out possibleNewDirections);
					if (possibleNewDirections != null)
					{
						queuedSchedulePaths.Add(new KeyValuePair<int, SchedulePathDescription>(timeOfDay, possibleNewDirections));
					}
					possibleNewDirections = null;
				}
				if (controller != null && controller.pathToEndPoint != null && controller.pathToEndPoint.Count > 0)
				{
					return;
				}
				if (queuedSchedulePaths.Count > 0 && timeOfDay >= queuedSchedulePaths[0].Key)
				{
					possibleNewDirections = queuedSchedulePaths[0].Value;
				}
				if (possibleNewDirections == null)
				{
					return;
				}
				prepareToDisembarkOnNewSchedulePath();
				if (returningToEndPoint || temporaryController != null)
				{
					return;
				}
				directionsToNewLocation = possibleNewDirections;
				if (queuedSchedulePaths.Count > 0)
				{
					queuedSchedulePaths.RemoveAt(0);
				}
				controller = new PathFindController(directionsToNewLocation.route, this, Utility.getGameLocationOfCharacter(this))
				{
					finalFacingDirection = directionsToNewLocation.facingDirection,
					endBehaviorFunction = getRouteEndBehaviorFunction(directionsToNewLocation.endOfRouteBehavior, directionsToNewLocation.endOfRouteMessage)
				};
				if (controller.pathToEndPoint == null || controller.pathToEndPoint.Count == 0)
				{
					if (controller.endBehaviorFunction != null)
					{
						controller.endBehaviorFunction(this, base.currentLocation);
					}
					controller = null;
				}
				if (directionsToNewLocation != null && directionsToNewLocation.route != null)
				{
					previousEndPoint = ((directionsToNewLocation.route.Count > 0) ? directionsToNewLocation.route.Last() : Point.Zero);
				}
			}
		}

		private void finishEndOfRouteAnimation()
		{
			_finishingEndOfRouteBehavior = _startedEndOfRouteBehavior;
			_startedEndOfRouteBehavior = null;
			if (_finishingEndOfRouteBehavior == "change_beach")
			{
				shouldWearIslandAttire.Value = true;
				currentlyDoingEndOfRouteAnimation = false;
			}
			else if (_finishingEndOfRouteBehavior == "change_normal")
			{
				shouldWearIslandAttire.Value = false;
				currentlyDoingEndOfRouteAnimation = false;
			}
			while (CurrentDialogue.Count > 0 && CurrentDialogue.Peek().removeOnNextMove)
			{
				CurrentDialogue.Pop();
			}
			shouldSayMarriageDialogue.Value = false;
			currentMarriageDialogue.Clear();
			nextEndOfRouteMessage = null;
			endOfRouteMessage.Value = null;
			if (currentlyDoingEndOfRouteAnimation && routeEndOutro != null)
			{
				List<FarmerSprite.AnimationFrame> outro = new List<FarmerSprite.AnimationFrame>();
				for (int i = 0; i < routeEndOutro.Length; i++)
				{
					if (i == routeEndOutro.Length - 1)
					{
						outro.Add(new FarmerSprite.AnimationFrame(routeEndOutro[i], 100, 0, secondaryArm: false, flip: false, routeEndAnimationFinished, behaviorAtEndOfFrame: true));
					}
					else
					{
						outro.Add(new FarmerSprite.AnimationFrame(routeEndOutro[i], 100, 0, secondaryArm: false, flip: false));
					}
				}
				if (outro.Count > 0)
				{
					Sprite.setCurrentAnimation(outro);
				}
				else
				{
					routeEndAnimationFinished(null);
				}
				if (_finishingEndOfRouteBehavior != null)
				{
					finishRouteBehavior(_finishingEndOfRouteBehavior);
				}
			}
			else
			{
				routeEndAnimationFinished(null);
			}
		}

		private void prepareToDisembarkOnNewSchedulePath()
		{
			finishEndOfRouteAnimation();
			doingEndOfRouteAnimation.Value = false;
			currentlyDoingEndOfRouteAnimation = false;
			if (!isMarried())
			{
				return;
			}
			if (temporaryController == null && Utility.getGameLocationOfCharacter(this) is FarmHouse)
			{
				temporaryController = new PathFindController(this, getHome(), new Point(getHome().warps[0].X, getHome().warps[0].Y), 2, eraseOldPathController: true)
				{
					NPCSchedule = true
				};
				if (temporaryController.pathToEndPoint == null || temporaryController.pathToEndPoint.Count <= 0)
				{
					temporaryController = null;
					schedule = null;
				}
				else
				{
					followSchedule = true;
				}
			}
			else if (Utility.getGameLocationOfCharacter(this) is Farm)
			{
				temporaryController = null;
				schedule = null;
			}
		}

		public void checkForMarriageDialogue(int timeOfDay, GameLocation location)
		{
			if (base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
			{
				return;
			}
			switch (timeOfDay)
			{
			case 1100:
				setRandomAfternoonMarriageDialogue(1100, location);
				break;
			case 1800:
				if (location is FarmHouse)
				{
					Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + timeOfDay + (int)getSpouse().UniqueMultiplayerID);
					string suffix2 = "";
					int which = random.Next(Game1.isRaining ? 7 : 6) - 1;
					suffix2 = ((which < 0) ? base.Name : string.Concat(which));
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", (Game1.isRaining ? "Rainy" : "Indoor") + "_Night_" + suffix2, false);
				}
				break;
			}
		}

		private void routeEndAnimationFinished(Farmer who)
		{
			doingEndOfRouteAnimation.Value = false;
			freezeMotion = false;
			Sprite.SpriteHeight = 32;
			Sprite.oldFrame = _beforeEndOfRouteAnimationFrame;
			Sprite.StopAnimation();
			endOfRouteMessage.Value = null;
			isCharging = false;
			base.speed = 2;
			base.addedSpeed = 0;
			goingToDoEndOfRouteAnimation.Value = false;
			if (isWalkingInSquare)
			{
				returningToEndPoint = true;
			}
			if (_finishingEndOfRouteBehavior == "penny_dishes")
			{
				drawOffset.Value = new Vector2(0f, 0f);
			}
			if (appliedRouteAnimationOffset != Vector2.Zero)
			{
				drawOffset.Value = new Vector2(0f, 0f);
				appliedRouteAnimationOffset = Vector2.Zero;
			}
			_finishingEndOfRouteBehavior = null;
		}

		public bool isOnSilentTemporaryMessage()
		{
			if (((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation) && endOfRouteMessage.Value != null && endOfRouteMessage.Value.ToLower().Equals("silent"))
			{
				return true;
			}
			return false;
		}

		public bool hasTemporaryMessageAvailable()
		{
			if (isDivorcedFrom(Game1.player))
			{
				return false;
			}
			if (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))
			{
				return true;
			}
			if (endOfRouteMessage.Value != null && ((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation))
			{
				return true;
			}
			return false;
		}

		public bool setTemporaryMessages(Farmer who)
		{
			if (isOnSilentTemporaryMessage())
			{
				return true;
			}
			if (endOfRouteMessage.Value != null && ((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation))
			{
				if (!isDivorcedFrom(Game1.player) && (!endOfRouteMessage.Value.Contains("marriage") || getSpouse() == Game1.player))
				{
					_PushTemporaryDialogue(endOfRouteMessage);
					return false;
				}
			}
			else if (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))
			{
				_PushTemporaryDialogue(base.currentLocation.GetLocationOverrideDialogue(this));
				return false;
			}
			return false;
		}

		protected void _PushTemporaryDialogue(string dialogue_key)
		{
			if (dialogue_key.StartsWith("Resort"))
			{
				string alternate_key = "Resort_Marriage" + dialogue_key.Substring(6);
				if (Game1.content.LoadStringReturnNullIfNotFound(alternate_key) != null)
				{
					dialogue_key = alternate_key;
				}
			}
			if (CurrentDialogue.Count == 0 || CurrentDialogue.Peek().temporaryDialogueKey != dialogue_key)
			{
				Dialogue temporary_dialogue = new Dialogue(Game1.content.LoadString(dialogue_key), this)
				{
					removeOnNextMove = true,
					temporaryDialogueKey = dialogue_key
				};
				CurrentDialogue.Push(temporary_dialogue);
			}
		}

		private void walkInSquareAtEndOfRoute(Character c, GameLocation l)
		{
			startRouteBehavior(endOfRouteBehaviorName);
		}

		private void doAnimationAtEndOfScheduleRoute(Character c, GameLocation l)
		{
			doingEndOfRouteAnimation.Value = true;
			reallyDoAnimationAtEndOfScheduleRoute();
			currentlyDoingEndOfRouteAnimation = true;
		}

		private void reallyDoAnimationAtEndOfScheduleRoute()
		{
			_startedEndOfRouteBehavior = loadedEndOfRouteBehavior;
			bool is_special_route_behavior = false;
			if (_startedEndOfRouteBehavior == "change_beach")
			{
				is_special_route_behavior = true;
			}
			else if (_startedEndOfRouteBehavior == "change_normal")
			{
				is_special_route_behavior = true;
			}
			if (!is_special_route_behavior)
			{
				if (_startedEndOfRouteBehavior == "penny_dishes")
				{
					drawOffset.Value = new Vector2(0f, 16f);
				}
				if (_startedEndOfRouteBehavior.EndsWith("_sleep"))
				{
					layingDown = true;
					HideShadow = true;
				}
				if (routeAnimationMetadata != null)
				{
					for (int j = 0; j < routeAnimationMetadata.Length; j++)
					{
						string[] metadata = routeAnimationMetadata[j].Split(' ');
						if (metadata[0] == "laying_down")
						{
							layingDown = true;
							HideShadow = true;
						}
						else if (metadata[0] == "offset")
						{
							appliedRouteAnimationOffset = new Vector2(int.Parse(metadata[1]), int.Parse(metadata[2]));
						}
					}
				}
				if (appliedRouteAnimationOffset != Vector2.Zero)
				{
					drawOffset.Value = appliedRouteAnimationOffset;
				}
				if (_skipRouteEndIntro)
				{
					doMiddleAnimation(null);
				}
				else
				{
					List<FarmerSprite.AnimationFrame> intro = new List<FarmerSprite.AnimationFrame>();
					for (int i = 0; i < routeEndIntro.Length; i++)
					{
						if (i == routeEndIntro.Length - 1)
						{
							intro.Add(new FarmerSprite.AnimationFrame(routeEndIntro[i], 100, 0, secondaryArm: false, flip: false, doMiddleAnimation, behaviorAtEndOfFrame: true));
						}
						else
						{
							intro.Add(new FarmerSprite.AnimationFrame(routeEndIntro[i], 100, 0, secondaryArm: false, flip: false));
						}
					}
					Sprite.setCurrentAnimation(intro);
				}
			}
			_skipRouteEndIntro = false;
			doingEndOfRouteAnimation.Value = true;
			freezeMotion = true;
			_beforeEndOfRouteAnimationFrame = Sprite.oldFrame;
		}

		private void doMiddleAnimation(Farmer who)
		{
			List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
			for (int i = 0; i < routeEndAnimation.Length; i++)
			{
				anim.Add(new FarmerSprite.AnimationFrame(routeEndAnimation[i], 100, 0, secondaryArm: false, flip: false));
			}
			Sprite.setCurrentAnimation(anim);
			Sprite.loop = true;
			if (_startedEndOfRouteBehavior != null)
			{
				startRouteBehavior(_startedEndOfRouteBehavior);
			}
		}

		private void startRouteBehavior(string behaviorName)
		{
			if (behaviorName.Length > 0 && behaviorName[0] == '"')
			{
				if (Game1.IsMasterGame)
				{
					endOfRouteMessage.Value = behaviorName.Replace("\"", "");
				}
				return;
			}
			if (behaviorName.Contains("square_") && Game1.IsMasterGame)
			{
				lastCrossroad = new Microsoft.Xna.Framework.Rectangle(getTileX() * 64, getTileY() * 64, 64, 64);
				string[] squareSplit = behaviorName.Split('_');
				walkInSquare(Convert.ToInt32(squareSplit[1]), Convert.ToInt32(squareSplit[2]), 6000);
				if (squareSplit.Length > 3)
				{
					squareMovementFacingPreference = Convert.ToInt32(squareSplit[3]);
				}
				else
				{
					squareMovementFacingPreference = -1;
				}
			}
			if (behaviorName.Contains("sleep"))
			{
				isPlayingSleepingAnimation = true;
				playSleepingAnimation();
			}
			if (!(behaviorName == "abigail_videogames"))
			{
				if (!(behaviorName == "dick_fish"))
				{
					if (!(behaviorName == "clint_hammer"))
					{
						if (behaviorName == "birdie_fish")
						{
							extendSourceRect(16, 0);
							Sprite.SpriteWidth = 32;
							Sprite.ignoreSourceRectUpdates = false;
							Sprite.currentFrame = 8;
						}
					}
					else
					{
						extendSourceRect(16, 0);
						Sprite.SpriteWidth = 32;
						Sprite.ignoreSourceRectUpdates = false;
						Sprite.currentFrame = 8;
						Sprite.CurrentAnimation[14] = new FarmerSprite.AnimationFrame(9, 100, 0, secondaryArm: false, flip: false, clintHammerSound);
					}
				}
				else
				{
					extendSourceRect(0, 32);
					Sprite.tempSpriteHeight = 64;
					drawOffset.Value = new Vector2(0f, 96f);
					Sprite.ignoreSourceRectUpdates = false;
					if (Utility.isOnScreen(Utility.Vector2ToPoint(base.Position), 64, base.currentLocation))
					{
						base.currentLocation.playSoundAt("slosh", getTileLocation());
					}
				}
			}
			else if (Game1.IsMasterGame)
			{
				Game1.multiplayer.broadcastSprites(Utility.getGameLocationOfCharacter(this), new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 999999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 688f
				});
				doEmote(52);
			}
		}

		public void playSleepingAnimation()
		{
			isSleeping.Value = true;
			Vector2 draw_offset = new Vector2(0f, name.Equals("Sebastian") ? 12 : (-4));
			if (isMarried())
			{
				draw_offset.X = -12f;
			}
			drawOffset.Value = draw_offset;
			if (!isPlayingSleepingAnimation)
			{
				Dictionary<string, string> animationDescriptions = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
				if (animationDescriptions.ContainsKey(name.Value.ToLower() + "_sleep"))
				{
					int sleep_frame = Convert.ToInt32(animationDescriptions[name.Value.ToLower() + "_sleep"].Split('/')[0]);
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(sleep_frame, 100, secondaryArm: false, flip: false)
					});
					Sprite.loop = true;
				}
				isPlayingSleepingAnimation = true;
			}
		}

		private void finishRouteBehavior(string behaviorName)
		{
			if (!(behaviorName == "abigail_videogames"))
			{
				if (behaviorName == "birdie_fish" || behaviorName == "clint_hammer" || behaviorName == "dick_fish")
				{
					reloadSprite();
					Sprite.SpriteWidth = 16;
					Sprite.SpriteHeight = 32;
					Sprite.UpdateSourceRect();
					drawOffset.Value = Vector2.Zero;
					Halt();
					movementPause = 1;
				}
			}
			else
			{
				Utility.getGameLocationOfCharacter(this).removeTemporarySpritesWithID(688);
			}
			if (layingDown)
			{
				layingDown = false;
				HideShadow = false;
			}
		}

		public bool IsReturningToEndPoint()
		{
			return returningToEndPoint;
		}

		public void StartActivityWalkInSquare(int square_width, int square_height, int pause_offset)
		{
			lastCrossroad = new Microsoft.Xna.Framework.Rectangle(getTileX() * 64, getTileY() * 64, 64, 64);
			walkInSquare(square_height, square_height, pause_offset);
		}

		public void EndActivityRouteEndBehavior()
		{
			finishEndOfRouteAnimation();
		}

		public void StartActivityRouteEndBehavior(string behavior_name, string end_message)
		{
			getRouteEndBehaviorFunction(behavior_name, end_message)?.Invoke(this, base.currentLocation);
		}

		private PathFindController.endBehavior getRouteEndBehaviorFunction(string behaviorName, string endMessage)
		{
			if (endMessage != null || (behaviorName != null && behaviorName.Length > 0 && behaviorName[0] == '"'))
			{
				nextEndOfRouteMessage = endMessage.Replace("\"", "");
			}
			if (behaviorName != null)
			{
				if (behaviorName.Length > 0 && behaviorName.Contains("square_"))
				{
					endOfRouteBehaviorName.Value = behaviorName;
					return walkInSquareAtEndOfRoute;
				}
				Dictionary<string, string> animationDescriptions = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
				if (behaviorName == "change_beach" || behaviorName == "change_normal")
				{
					endOfRouteBehaviorName.Value = behaviorName;
					goingToDoEndOfRouteAnimation.Value = true;
				}
				else
				{
					if (!animationDescriptions.ContainsKey(behaviorName))
					{
						return null;
					}
					endOfRouteBehaviorName.Value = behaviorName;
					loadEndOfRouteBehavior(endOfRouteBehaviorName);
					goingToDoEndOfRouteAnimation.Value = true;
				}
				return doAnimationAtEndOfScheduleRoute;
			}
			return null;
		}

		private void loadEndOfRouteBehavior(string name)
		{
			loadedEndOfRouteBehavior = name;
			if (name.Length > 0 && name.Contains("square_"))
			{
				return;
			}
			Dictionary<string, string> animationDescriptions = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
			if (animationDescriptions.ContainsKey(name))
			{
				string[] rawData = animationDescriptions[name].Split('/');
				routeEndIntro = Utility.parseStringToIntArray(rawData[0]);
				routeEndAnimation = Utility.parseStringToIntArray(rawData[1]);
				routeEndOutro = Utility.parseStringToIntArray(rawData[2]);
				if (rawData.Length > 3 && rawData[3] != "")
				{
					nextEndOfRouteMessage = rawData[3];
				}
				if (rawData.Length > 4)
				{
					routeAnimationMetadata = rawData.Skip(4).ToArray();
				}
				else
				{
					routeAnimationMetadata = null;
				}
			}
		}

		public void warp(bool wasOutdoors)
		{
		}

		public void shake(int duration)
		{
			shakeTimer = duration;
		}

		public void setNewDialogue(string s, bool add = false, bool clearOnMovement = false)
		{
			if (!add)
			{
				CurrentDialogue.Clear();
			}
			CurrentDialogue.Push(new Dialogue(s, this)
			{
				removeOnNextMove = clearOnMovement
			});
		}

		public void setNewDialogue(string dialogueSheetName, string dialogueSheetKey, int numberToAppend = -1, bool add = false, bool clearOnMovement = false)
		{
			if (!add)
			{
				CurrentDialogue.Clear();
			}
			string nameToAppend = (numberToAppend == -1) ? base.Name : "";
			if (dialogueSheetName.Contains("Marriage"))
			{
				if (getSpouse() == Game1.player)
				{
					CurrentDialogue.Push(new Dialogue(tryToGetMarriageSpecificDialogueElseReturnDefault(dialogueSheetKey + ((numberToAppend != -1) ? string.Concat(numberToAppend) : "") + nameToAppend), this)
					{
						removeOnNextMove = clearOnMovement
					});
				}
			}
			else if (Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + dialogueSheetName).ContainsKey(dialogueSheetKey + ((numberToAppend != -1) ? string.Concat(numberToAppend) : "") + nameToAppend))
			{
				CurrentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + dialogueSheetName)[dialogueSheetKey + ((numberToAppend != -1) ? string.Concat(numberToAppend) : "") + nameToAppend], this)
				{
					removeOnNextMove = clearOnMovement
				});
			}
		}

		public string GetDialogueSheetName()
		{
			if (base.Name == "Leo" && DefaultMap != "IslandHut")
			{
				return base.Name + "Mainland";
			}
			return base.Name;
		}

		public void setSpouseRoomMarriageDialogue()
		{
			currentMarriageDialogue.Clear();
			addMarriageDialogue("MarriageDialogue", "spouseRoom_" + base.Name, false);
		}

		public void setRandomAfternoonMarriageDialogue(int time, GameLocation location, bool countAsDailyAfternoon = false)
		{
			if ((base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri") || hasSaidAfternoonDialogue.Value)
			{
				return;
			}
			if (countAsDailyAfternoon)
			{
				hasSaidAfternoonDialogue.Value = true;
			}
			Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + time);
			int hearts = getSpouse().getFriendshipHeartLevelForNPC(base.Name);
			if (location is FarmHouse && r.NextDouble() < 0.5)
			{
				if (hearts < 9)
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", (r.NextDouble() < (double)((float)hearts / 11f)) ? "Neutral_" : ("Bad_" + r.Next(10)), false);
				}
				else if (r.NextDouble() < 0.05)
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + base.Name, false);
				}
				else if ((hearts >= 10 && r.NextDouble() < 0.5) || (hearts >= 11 && r.NextDouble() < 0.75))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Good_" + r.Next(10), false);
				}
				else
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Neutral_" + r.Next(10), false);
				}
			}
			else if (location is Farm)
			{
				if (r.NextDouble() < 0.2)
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + base.Name, false);
				}
				else
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				}
			}
		}

		public bool isBirthday(string season, int day)
		{
			if (Birthday_Season == null)
			{
				return false;
			}
			if (Birthday_Season.Equals(season) && Birthday_Day == day)
			{
				return true;
			}
			return false;
		}

		public Object getFavoriteItem()
		{
			Game1.NPCGiftTastes.TryGetValue(base.Name, out string NPCLikes);
			if (NPCLikes != null)
			{
				return new Object(Convert.ToInt32(NPCLikes.Split('/')[1].Split(' ')[0]), 1);
			}
			return null;
		}

		public void receiveGift(Object o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
		{
			Game1.NPCGiftTastes.TryGetValue(base.Name, out string NPCLikes);
			string[] split = NPCLikes.Split('/');
			float qualityChangeMultipler = 1f;
			switch (o.Quality)
			{
			case 1:
				qualityChangeMultipler = 1.1f;
				break;
			case 2:
				qualityChangeMultipler = 1.25f;
				break;
			case 4:
				qualityChangeMultipler = 1.5f;
				break;
			}
			if (Birthday_Season != null && Game1.currentSeason.Equals(Birthday_Season) && Game1.dayOfMonth == Birthday_Day)
			{
				friendshipChangeMultiplier = 8f;
				string positiveBirthdayMessage = (Manners == 2) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4274") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4275");
				if (Game1.random.NextDouble() < 0.5)
				{
					positiveBirthdayMessage = ((Manners == 2) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4276") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4277"));
				}
				string negativeBirthdayMessage = (Manners == 2) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4278") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4279");
				split[0] = positiveBirthdayMessage;
				split[2] = positiveBirthdayMessage;
				split[4] = negativeBirthdayMessage;
				split[6] = negativeBirthdayMessage;
				split[8] = ((Manners == 2) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4280") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4281"));
			}
			giver?.onGiftGiven(this, o);
			if (getSpouse() != null && getSpouse().Equals(giver))
			{
				friendshipChangeMultiplier /= 2f;
			}
			if (NPCLikes == null)
			{
				return;
			}
			Game1.stats.GiftsGiven++;
			giver.currentLocation.localSound("give_gift");
			if (updateGiftLimitInfo)
			{
				giver.friendshipData[base.Name].GiftsToday++;
				giver.friendshipData[base.Name].GiftsThisWeek++;
				giver.friendshipData[base.Name].LastGiftDate = new WorldDate(Game1.Date);
			}
			int tasteForItem = getGiftTasteForThisItem(o);
			switch (giver.FacingDirection)
			{
			case 0:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(80, 50f);
				break;
			case 1:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(72, 50f);
				break;
			case 2:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(64, 50f);
				break;
			case 3:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(88, 50f);
				break;
			}
			List<string> reactions = new List<string>();
			for (int j = 0; j < 8; j += 2)
			{
				reactions.Add(split[j]);
			}
			if (base.Name.Equals("Krobus") && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
			{
				for (int i = 0; i < reactions.Count; i++)
				{
					reactions[i] = "...";
				}
			}
			switch (tasteForItem)
			{
			case 0:
				if (base.Name.Contains("Dwarf"))
				{
					if (showResponse)
					{
						Game1.drawDialogue(this, giver.canUnderstandDwarves ? reactions[0] : StardewValley.Dialogue.convertToDwarvish(reactions[0]));
					}
				}
				else if (showResponse)
				{
					Game1.drawDialogue(this, reactions[0] + "$h");
				}
				giver.changeFriendship((int)(80f * friendshipChangeMultiplier * qualityChangeMultipler), this);
				doEmote(20);
				faceTowardFarmerForPeriod(15000, 4, faceAway: false, giver);
				return;
			case 6:
				if (base.Name.Contains("Dwarf"))
				{
					if (showResponse)
					{
						Game1.drawDialogue(this, giver.canUnderstandDwarves ? reactions[3] : StardewValley.Dialogue.convertToDwarvish(reactions[3]));
					}
				}
				else if (showResponse)
				{
					Game1.drawDialogue(this, reactions[3] + "$s");
				}
				giver.changeFriendship((int)(-40f * friendshipChangeMultiplier), this);
				faceTowardFarmerForPeriod(15000, 4, faceAway: true, giver);
				doEmote(12);
				return;
			case 2:
				if (base.Name.Contains("Dwarf"))
				{
					if (showResponse)
					{
						Game1.drawDialogue(this, giver.canUnderstandDwarves ? reactions[1] : StardewValley.Dialogue.convertToDwarvish(reactions[1]));
					}
				}
				else if (showResponse)
				{
					Game1.drawDialogue(this, reactions[1] + "$h");
				}
				giver.changeFriendship((int)(45f * friendshipChangeMultiplier * qualityChangeMultipler), this);
				faceTowardFarmerForPeriod(7000, 3, faceAway: true, giver);
				return;
			case 4:
				if (base.Name.Contains("Dwarf"))
				{
					if (showResponse)
					{
						Game1.drawDialogue(this, giver.canUnderstandDwarves ? reactions[2] : StardewValley.Dialogue.convertToDwarvish(reactions[2]));
					}
				}
				else if (showResponse)
				{
					Game1.drawDialogue(this, reactions[2] + "$s");
				}
				giver.changeFriendship((int)(-20f * friendshipChangeMultiplier), this);
				return;
			}
			if (base.Name.Contains("Dwarf"))
			{
				if (showResponse)
				{
					Game1.drawDialogue(this, giver.canUnderstandDwarves ? split[8] : StardewValley.Dialogue.convertToDwarvish(split[8]));
				}
			}
			else if (showResponse)
			{
				Game1.drawDialogue(this, split[8]);
			}
			giver.changeFriendship((int)(20f * friendshipChangeMultiplier), this);
		}

		public override void draw(SpriteBatch b, float alpha = 1f)
		{
			if (Sprite == null || IsInvisible || (!Utility.isOnScreen(base.Position, 128) && (!eventActor || !(base.currentLocation is Summit))))
			{
				return;
			}
			if ((bool)swimming)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 80 + yJumpOffset * 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero) - new Vector2(0f, yOffset), new Microsoft.Xna.Framework.Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, Sprite.SourceRect.Width, Sprite.SourceRect.Height / 2 - (int)(yOffset / 4f)), Color.White, rotation, new Vector2(32f, 96f) / 4f, Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
				Vector2 localPosition = getLocalPosition(Game1.viewport);
				b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)localPosition.X + (int)yOffset + 8, (int)localPosition.Y - 128 + Sprite.SourceRect.Height * 4 + 48 + yJumpOffset * 2 - (int)yOffset, Sprite.SourceRect.Width * 4 - (int)yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, (float)getStandingY() / 10000f + 0.001f);
			}
			else
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(GetSpriteWidthForPositioning() * 4 / 2, GetBoundingBox().Height / 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, Color.White * alpha, rotation, new Vector2(Sprite.SpriteWidth / 2, (float)Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, scale) * 4f, (flip || (Sprite.CurrentAnimation != null && Sprite.CurrentAnimation[Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
			}
			if (Breather && shakeTimer <= 0 && !swimming && Sprite.currentFrame < 16 && !farmerPassesThrough)
			{
				Microsoft.Xna.Framework.Rectangle chestBox = Sprite.SourceRect;
				chestBox.Y += Sprite.SpriteHeight / 2 + Sprite.SpriteHeight / 32;
				chestBox.Height = Sprite.SpriteHeight / 4;
				chestBox.X += Sprite.SpriteWidth / 4;
				chestBox.Width = Sprite.SpriteWidth / 2;
				Vector2 chestPosition = new Vector2(Sprite.SpriteWidth * 4 / 2, 8f);
				if (Age == 2)
				{
					chestBox.Y += Sprite.SpriteHeight / 6 + 1;
					chestBox.Height /= 2;
					chestPosition.Y += Sprite.SpriteHeight / 8 * 4;
					if (this is Child)
					{
						if ((this as Child).Age == 0)
						{
							chestPosition.X -= 12f;
						}
						else if ((this as Child).Age == 1)
						{
							chestPosition.X -= 4f;
						}
					}
				}
				else if (Gender == 1)
				{
					chestBox.Y++;
					chestPosition.Y -= 4f;
					chestBox.Height /= 2;
				}
				float breathScale = Math.Max(0f, (float)Math.Ceiling(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 600.0 + (double)(defaultPosition.X * 20f))) / 4f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + chestPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), chestBox, Color.White * alpha, rotation, new Vector2(chestBox.Width / 2, chestBox.Height / 2 + 1), Math.Max(0.2f, scale) * 4f + breathScale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.992f : ((float)getStandingY() / 10000f + 0.001f)));
			}
			if (isGlowing)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(GetSpriteWidthForPositioning() * 4 / 2, GetBoundingBox().Height / 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, glowingColor * glowingTransparency, rotation, new Vector2(Sprite.SpriteWidth / 2, (float)Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)getStandingY() / 10000f + 0.001f)));
			}
			if (base.IsEmoting && !Game1.eventUp && !(this is Child) && !(this is Pet))
			{
				Vector2 emotePosition = getLocalPosition(Game1.viewport);
				emotePosition.Y -= 32 + Sprite.SpriteHeight * 4;
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)getStandingY() / 10000f);
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (textAboveHeadTimer > 0 && textAboveHead != null)
			{
				Vector2 local = Game1.GlobalToLocal(new Vector2(getStandingX(), getStandingY() - Sprite.SpriteHeight * 4 - 64 + yJumpOffset));
				if (textAboveHeadStyle == 0)
				{
					local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
				}
				if (NeedsBirdieEmoteHack())
				{
					local.X += -GetBoundingBox().Width / 4 + 64;
				}
				if (shouldShadowBeOffset)
				{
					local += drawOffset.Value;
				}
				SpriteText.drawStringWithScrollCenteredAt(b, textAboveHead, (int)local.X, (int)local.Y, "", textAboveHeadAlpha, textAboveHeadColor, 1, (float)(getTileY() * 64) / 10000f + 0.001f + (float)getTileX() / 10000f);
			}
		}

		public bool NeedsBirdieEmoteHack()
		{
			if (Game1.eventUp && Sprite.SpriteWidth == 32 && base.Name == "Birdie")
			{
				return true;
			}
			return false;
		}

		public void warpToPathControllerDestination()
		{
			if (controller != null)
			{
				while (controller.pathToEndPoint.Count > 2)
				{
					controller.pathToEndPoint.Pop();
					controller.handleWarps(new Microsoft.Xna.Framework.Rectangle(controller.pathToEndPoint.Peek().X * 64, controller.pathToEndPoint.Peek().Y * 64, 64, 64));
					base.Position = new Vector2(controller.pathToEndPoint.Peek().X * 64, controller.pathToEndPoint.Peek().Y * 64 + 16);
					Halt();
				}
			}
		}

		public virtual Microsoft.Xna.Framework.Rectangle getMugShotSourceRect()
		{
			return new Microsoft.Xna.Framework.Rectangle(0, (Age == 2) ? 4 : 0, 16, 24);
		}

		public void getHitByPlayer(Farmer who, GameLocation location)
		{
			doEmote(12);
			if (who == null)
			{
				if (Game1.IsMultiplayer)
				{
					return;
				}
				who = Game1.player;
			}
			if (who.friendshipData.ContainsKey(base.Name))
			{
				who.changeFriendship(-30, this);
				if (who.IsLocalPlayer)
				{
					CurrentDialogue.Clear();
					CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4293") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4294"), this));
				}
				location.debris.Add(new Debris(Sprite.textureName, Game1.random.Next(3, 8), new Vector2(GetBoundingBox().Center.X, GetBoundingBox().Center.Y)));
			}
			if (base.Name.Equals("Bouncer"))
			{
				location.localSound("crafting");
			}
			else
			{
				location.localSound("hitEnemy");
			}
		}

		public void walkInSquare(int squareWidth, int squareHeight, int squarePauseOffset)
		{
			isWalkingInSquare = true;
			lengthOfWalkingSquareX = squareWidth;
			lengthOfWalkingSquareY = squareHeight;
			this.squarePauseOffset = squarePauseOffset;
		}

		public void moveTowardPlayer(int threshold)
		{
			isWalkingTowardPlayer.Value = true;
			moveTowardPlayerThreshold.Value = threshold;
		}

		protected virtual Farmer findPlayer()
		{
			return Game1.MasterPlayer;
		}

		public virtual bool withinPlayerThreshold()
		{
			return withinPlayerThreshold(moveTowardPlayerThreshold);
		}

		public virtual bool withinPlayerThreshold(int threshold)
		{
			if (base.currentLocation != null && !base.currentLocation.farmers.Any())
			{
				return false;
			}
			Vector2 tileLocationOfPlayer = findPlayer().getTileLocation();
			Vector2 tileLocationOfMonster = getTileLocation();
			if (Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold)
			{
				return true;
			}
			return false;
		}

		private Stack<Point> addToStackForSchedule(Stack<Point> original, Stack<Point> toAdd)
		{
			if (toAdd == null)
			{
				return original;
			}
			original = new Stack<Point>(original);
			while (original.Count > 0)
			{
				toAdd.Push(original.Pop());
			}
			return toAdd;
		}

		private SchedulePathDescription pathfindToNextScheduleLocation(string startingLocation, int startingX, int startingY, string endingLocation, int endingX, int endingY, int finalFacingDirection, string endBehavior, string endMessage)
		{
			Stack<Point> path = new Stack<Point>();
			Point locationStartPoint = new Point(startingX, startingY);
			List<string> locationsRoute = (!startingLocation.Equals(endingLocation, StringComparison.Ordinal)) ? getLocationRoute(startingLocation, endingLocation) : null;
			if (locationsRoute != null)
			{
				for (int i = 0; i < locationsRoute.Count; i++)
				{
					GameLocation currentLocation = Game1.getLocationFromName(locationsRoute[i]);
					if (currentLocation.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						currentLocation = Game1.getLocationFromName("Trailer_Big");
					}
					if (i < locationsRoute.Count - 1)
					{
						Point target = currentLocation.getWarpPointTo(locationsRoute[i + 1]);
						if (target.Equals(Point.Zero) || locationStartPoint.Equals(Point.Zero))
						{
							throw new Exception("schedule pathing tried to find a warp point that doesn't exist.");
						}
						path = addToStackForSchedule(path, PathFindController.findPathForNPCSchedules(locationStartPoint, target, currentLocation, 30000));
						locationStartPoint = currentLocation.getWarpPointTarget(target, this);
					}
					else
					{
						path = addToStackForSchedule(path, PathFindController.findPathForNPCSchedules(locationStartPoint, new Point(endingX, endingY), currentLocation, 30000));
					}
				}
			}
			else if (startingLocation.Equals(endingLocation, StringComparison.Ordinal))
			{
				GameLocation location = Game1.getLocationFromName(startingLocation);
				if (location.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					location = Game1.getLocationFromName("Trailer_Big");
				}
				path = PathFindController.findPathForNPCSchedules(locationStartPoint, new Point(endingX, endingY), location, 30000);
			}
			return new SchedulePathDescription(path, finalFacingDirection, endBehavior, endMessage);
		}

		private List<string> getLocationRoute(string startingLocation, string endingLocation)
		{
			foreach (List<string> s in routesFromLocationToLocation)
			{
				if (s.First().Equals(startingLocation, StringComparison.Ordinal) && s.Last().Equals(endingLocation, StringComparison.Ordinal) && ((int)gender == 0 || !s.Contains("BathHouse_MensLocker", StringComparer.Ordinal)) && ((int)gender != 0 || !s.Contains("BathHouse_WomensLocker", StringComparer.Ordinal)))
				{
					return s;
				}
			}
			return null;
		}

		private bool changeScheduleForLocationAccessibility(ref string locationName, ref int tileX, ref int tileY, ref int facingDirection)
		{
			switch (locationName)
			{
			case "JojaMart":
			case "Railroad":
				if (!Game1.isLocationAccessible(locationName))
				{
					if (!hasMasterScheduleEntry(locationName + "_Replacement"))
					{
						return true;
					}
					string[] split = getMasterScheduleEntry(locationName + "_Replacement").Split(' ');
					locationName = split[0];
					tileX = Convert.ToInt32(split[1]);
					tileY = Convert.ToInt32(split[2]);
					facingDirection = Convert.ToInt32(split[3]);
				}
				break;
			case "CommunityCenter":
				return !Game1.isLocationAccessible(locationName);
			}
			return false;
		}

		public Dictionary<int, SchedulePathDescription> parseMasterSchedule(string rawData)
		{
			string[] split = rawData.Split('/');
			Dictionary<int, SchedulePathDescription> oneDaySchedule = new Dictionary<int, SchedulePathDescription>();
			int routesToSkip = 0;
			if (split[0].Contains("GOTO"))
			{
				string newKey2 = split[0].Split(' ')[1];
				if (newKey2.ToLower().Equals("season"))
				{
					newKey2 = Game1.currentSeason;
				}
				try
				{
					split = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + base.Name)[newKey2].Split('/');
				}
				catch (Exception)
				{
					return parseMasterSchedule(getMasterScheduleEntry("spring"));
				}
			}
			if (split[0].Contains("NOT"))
			{
				string[] commandSplit = split[0].Split(' ');
				string a = commandSplit[1].ToLower();
				if (a == "friendship")
				{
					int index7 = 2;
					bool conditionMet = false;
					for (; index7 < commandSplit.Length; index7 += 2)
					{
						string who = commandSplit[index7];
						int level = 0;
						if (int.TryParse(commandSplit[index7 + 1], out level))
						{
							foreach (Farmer allFarmer in Game1.getAllFarmers())
							{
								if (allFarmer.getFriendshipHeartLevelForNPC(who) >= level)
								{
									conditionMet = true;
									break;
								}
							}
						}
						if (conditionMet)
						{
							break;
						}
					}
					if (conditionMet)
					{
						return parseMasterSchedule(getMasterScheduleEntry("spring"));
					}
					routesToSkip++;
				}
			}
			else if (split[0].Contains("MAIL"))
			{
				string mailID = split[0].Split(' ')[1];
				routesToSkip = ((!Game1.MasterPlayer.mailReceived.Contains(mailID) && !NetWorldState.checkAnywhereForWorldStateID(mailID)) ? (routesToSkip + 1) : (routesToSkip + 2));
			}
			if (split[routesToSkip].Contains("GOTO"))
			{
				string newKey = split[routesToSkip].Split(' ')[1];
				if (newKey.ToLower().Equals("season"))
				{
					newKey = Game1.currentSeason;
				}
				else if (newKey.ToLower().Equals("no_schedule"))
				{
					followSchedule = false;
					return null;
				}
				return parseMasterSchedule(getMasterScheduleEntry(newKey));
			}
			Point previousPosition = isMarried() ? new Point(0, 23) : new Point((int)defaultPosition.X / 64, (int)defaultPosition.Y / 64);
			string previousGameLocation = isMarried() ? "BusStop" : ((string)defaultMap);
			int previousTime = 610;
			string default_map = DefaultMap;
			int default_x = (int)(defaultPosition.X / 64f);
			int default_y = (int)(defaultPosition.Y / 64f);
			bool default_map_dirty = false;
			for (int i = routesToSkip; i < split.Length; i++)
			{
				if (split.Length <= 1)
				{
					break;
				}
				int index6 = 0;
				string[] newDestinationDescription = split[i].Split(' ');
				int time2 = 0;
				bool time_is_arrival_time = false;
				string time_string = newDestinationDescription[index6];
				if (time_string.Length > 0 && newDestinationDescription[index6][0] == 'a')
				{
					time_is_arrival_time = true;
					time_string = time_string.Substring(1);
				}
				time2 = Convert.ToInt32(time_string);
				index6++;
				string location = newDestinationDescription[index6];
				string endOfRouteAnimation = null;
				string endOfRouteMessage = null;
				int xLocation = 0;
				int yLocation = 0;
				int localFacingDirection = 2;
				if (location == "bed")
				{
					if (isMarried())
					{
						location = "BusStop";
						xLocation = -1;
						yLocation = 23;
						localFacingDirection = 3;
					}
					else
					{
						string default_schedule = null;
						if (hasMasterScheduleEntry("default"))
						{
							default_schedule = getMasterScheduleEntry("default");
						}
						else if (hasMasterScheduleEntry("spring"))
						{
							default_schedule = getMasterScheduleEntry("spring");
						}
						if (default_schedule != null)
						{
							try
							{
								string[] array = default_schedule.Split('/');
								string[] last_schedule_split = array[array.Length - 1].Split(' ');
								location = last_schedule_split[1];
								if (last_schedule_split.Length > 3)
								{
									if (!int.TryParse(last_schedule_split[2], out xLocation) || !int.TryParse(last_schedule_split[3], out yLocation))
									{
										default_schedule = null;
									}
								}
								else
								{
									default_schedule = null;
								}
							}
							catch (Exception)
							{
								default_schedule = null;
							}
						}
						if (default_schedule == null)
						{
							location = default_map;
							xLocation = default_x;
							yLocation = default_y;
						}
					}
					index6++;
					Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
					string sleep_behavior = name.Value.ToLower() + "_sleep";
					if (dictionary.ContainsKey(sleep_behavior))
					{
						endOfRouteAnimation = sleep_behavior;
					}
				}
				else
				{
					if (int.TryParse(location, out int _))
					{
						location = previousGameLocation;
						index6--;
					}
					index6++;
					xLocation = Convert.ToInt32(newDestinationDescription[index6]);
					index6++;
					yLocation = Convert.ToInt32(newDestinationDescription[index6]);
					index6++;
					try
					{
						if (newDestinationDescription.Length > index6)
						{
							if (int.TryParse(newDestinationDescription[index6], out localFacingDirection))
							{
								index6++;
							}
							else
							{
								localFacingDirection = 2;
							}
						}
					}
					catch (Exception)
					{
						localFacingDirection = 2;
					}
				}
				if (changeScheduleForLocationAccessibility(ref location, ref xLocation, ref yLocation, ref localFacingDirection))
				{
					if (Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + base.Name).ContainsKey("default"))
					{
						return parseMasterSchedule(getMasterScheduleEntry("default"));
					}
					return parseMasterSchedule(getMasterScheduleEntry("spring"));
				}
				if (index6 < newDestinationDescription.Length)
				{
					if (newDestinationDescription[index6].Length > 0 && newDestinationDescription[index6][0] == '"')
					{
						endOfRouteMessage = split[i].Substring(split[i].IndexOf('"'));
					}
					else
					{
						endOfRouteAnimation = newDestinationDescription[index6];
						index6++;
						if (index6 < newDestinationDescription.Length && newDestinationDescription[index6].Length > 0 && newDestinationDescription[index6][0] == '"')
						{
							endOfRouteMessage = split[i].Substring(split[i].IndexOf('"')).Replace("\"", "");
						}
					}
				}
				if (time2 == 0)
				{
					default_map_dirty = true;
					default_map = location;
					default_x = xLocation;
					default_y = yLocation;
					previousGameLocation = location;
					previousPosition.X = xLocation;
					previousPosition.Y = yLocation;
					previousEndPoint = new Point(xLocation, yLocation);
					continue;
				}
				SchedulePathDescription path_description = pathfindToNextScheduleLocation(previousGameLocation, previousPosition.X, previousPosition.Y, location, xLocation, yLocation, localFacingDirection, endOfRouteAnimation, endOfRouteMessage);
				if (time_is_arrival_time)
				{
					int distance_traveled = 0;
					Point? last_point = null;
					foreach (Point point in path_description.route)
					{
						if (!last_point.HasValue)
						{
							last_point = point;
						}
						else
						{
							if (Math.Abs(last_point.Value.X - point.X) + Math.Abs(last_point.Value.Y - point.Y) == 1)
							{
								distance_traveled += 64;
							}
							last_point = point;
						}
					}
					int num = distance_traveled / 2;
					int ticks_per_ten_minutes = 420;
					int travel_time = (int)Math.Round((float)num / (float)ticks_per_ten_minutes) * 10;
					time2 = Math.Max(Utility.ConvertMinutesToTime(Utility.ConvertTimeToMinutes(time2) - travel_time), previousTime);
				}
				oneDaySchedule.Add(time2, path_description);
				previousPosition.X = xLocation;
				previousPosition.Y = yLocation;
				previousGameLocation = location;
				previousTime = time2;
			}
			if (Game1.IsMasterGame && default_map_dirty)
			{
				Game1.warpCharacter(this, default_map, new Point(default_x, default_y));
			}
			if (_lastLoadedScheduleKey != null && Game1.IsMasterGame)
			{
				dayScheduleName.Value = _lastLoadedScheduleKey;
			}
			return oneDaySchedule;
		}

		public Dictionary<int, SchedulePathDescription> getSchedule(int dayOfMonth)
		{
			if (!base.Name.Equals("Robin") || Game1.player.currentUpgrade != null)
			{
				IsInvisible = false;
			}
			if ((base.Name.Equals("Willy") && Game1.stats.DaysPlayed < 2) || daysUntilNotInvisible > 0)
			{
				IsInvisible = true;
			}
			else if (Schedule != null)
			{
				followSchedule = true;
			}
			if (getMasterScheduleRawData() == null)
			{
				return null;
			}
			if (islandScheduleName != null && islandScheduleName.Value != null && islandScheduleName.Value != "")
			{
				nameOfTodaysSchedule = islandScheduleName.Value;
				return Schedule;
			}
			if (isMarried())
			{
				if (hasMasterScheduleEntry("marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth))
				{
					nameOfTodaysSchedule = "marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth;
					return parseMasterSchedule(getMasterScheduleEntry("marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth));
				}
				string day = Game1.shortDayNameFromDayOfSeason(dayOfMonth);
				if ((base.Name.Equals("Penny") && (day.Equals("Tue") || day.Equals("Wed") || day.Equals("Fri"))) || (base.Name.Equals("Maru") && (day.Equals("Tue") || day.Equals("Thu"))) || (base.Name.Equals("Harvey") && (day.Equals("Tue") || day.Equals("Thu"))))
				{
					nameOfTodaysSchedule = "marriageJob";
					return parseMasterSchedule(getMasterScheduleEntry("marriageJob"));
				}
				if (!Game1.isRaining && hasMasterScheduleEntry("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
				{
					nameOfTodaysSchedule = "marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
					return parseMasterSchedule(getMasterScheduleEntry("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)));
				}
				followSchedule = false;
				return null;
			}
			if (hasMasterScheduleEntry(Game1.currentSeason + "_" + Game1.dayOfMonth))
			{
				return parseMasterSchedule(getMasterScheduleEntry(Game1.currentSeason + "_" + Game1.dayOfMonth));
			}
			int friendship3 = Utility.GetAllPlayerFriendshipLevel(this);
			if (friendship3 >= 0)
			{
				friendship3 /= 250;
			}
			while (friendship3 > 0)
			{
				if (hasMasterScheduleEntry(Game1.dayOfMonth + "_" + friendship3))
				{
					return parseMasterSchedule(getMasterScheduleEntry(Game1.dayOfMonth + "_" + friendship3));
				}
				friendship3--;
			}
			if (hasMasterScheduleEntry(string.Empty + Game1.dayOfMonth))
			{
				return parseMasterSchedule(getMasterScheduleEntry(string.Empty + Game1.dayOfMonth));
			}
			if (base.Name.Equals("Pam") && Game1.player.mailReceived.Contains("ccVault"))
			{
				return parseMasterSchedule(getMasterScheduleEntry("bus"));
			}
			if (Game1.IsRainingHere(base.currentLocation))
			{
				if (Game1.random.NextDouble() < 0.5 && hasMasterScheduleEntry("rain2"))
				{
					return parseMasterSchedule(getMasterScheduleEntry("rain2"));
				}
				if (hasMasterScheduleEntry("rain"))
				{
					return parseMasterSchedule(getMasterScheduleEntry("rain"));
				}
			}
			List<string> key = new List<string>
			{
				Game1.currentSeason,
				Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)
			};
			friendship3 = Utility.GetAllPlayerFriendshipLevel(this);
			if (friendship3 >= 0)
			{
				friendship3 /= 250;
			}
			while (friendship3 > 0)
			{
				key.Add(string.Empty + friendship3);
				if (hasMasterScheduleEntry(string.Join("_", key)))
				{
					return parseMasterSchedule(getMasterScheduleEntry(string.Join("_", key)));
				}
				friendship3--;
				key.RemoveAt(key.Count - 1);
			}
			if (hasMasterScheduleEntry(string.Join("_", key)))
			{
				return parseMasterSchedule(getMasterScheduleEntry(string.Join("_", key)));
			}
			if (hasMasterScheduleEntry(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
			{
				return parseMasterSchedule(getMasterScheduleEntry(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)));
			}
			if (hasMasterScheduleEntry(Game1.currentSeason))
			{
				return parseMasterSchedule(getMasterScheduleEntry(Game1.currentSeason));
			}
			if (hasMasterScheduleEntry("spring_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
			{
				return parseMasterSchedule(getMasterScheduleEntry("spring_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)));
			}
			key.RemoveAt(key.Count - 1);
			key.Add("spring");
			friendship3 = Utility.GetAllPlayerFriendshipLevel(this);
			if (friendship3 >= 0)
			{
				friendship3 /= 250;
			}
			while (friendship3 > 0)
			{
				key.Add(string.Empty + friendship3);
				if (hasMasterScheduleEntry(string.Join("_", key)))
				{
					return parseMasterSchedule(getMasterScheduleEntry(string.Join("_", key)));
				}
				friendship3--;
				key.RemoveAt(key.Count - 1);
			}
			if (hasMasterScheduleEntry("spring"))
			{
				return parseMasterSchedule(getMasterScheduleEntry("spring"));
			}
			return null;
		}

		public virtual void handleMasterScheduleFileLoadError(Exception e)
		{
		}

		public virtual void InvalidateMasterSchedule()
		{
			_hasLoadedMasterScheduleData = false;
		}

		public Dictionary<string, string> getMasterScheduleRawData()
		{
			if (!_hasLoadedMasterScheduleData)
			{
				_hasLoadedMasterScheduleData = true;
				try
				{
					if (base.Name == "Leo")
					{
						if (DefaultMap == "IslandHut")
						{
							_masterScheduleData = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + base.Name);
						}
						else
						{
							_masterScheduleData = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + base.Name + "Mainland");
						}
					}
					else
					{
						_masterScheduleData = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + base.Name);
					}
				}
				catch (Exception e)
				{
					handleMasterScheduleFileLoadError(e);
				}
			}
			return _masterScheduleData;
		}

		public string getMasterScheduleEntry(string schedule_key)
		{
			if (getMasterScheduleRawData() == null)
			{
				throw new KeyNotFoundException("The schedule file for NPC '" + base.Name + "' could not be loaded...");
			}
			if (_masterScheduleData.TryGetValue(schedule_key, out string data))
			{
				_lastLoadedScheduleKey = schedule_key;
				return data;
			}
			throw new KeyNotFoundException("The schedule file for NPC '" + base.Name + "' has no schedule named '" + schedule_key + "'.");
		}

		public bool hasMasterScheduleEntry(string key)
		{
			if (getMasterScheduleRawData() == null)
			{
				return false;
			}
			return getMasterScheduleRawData().ContainsKey(key);
		}

		public virtual bool isRoommate()
		{
			if (!isVillager())
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.spouse != null && f.spouse.Equals(base.Name) && !f.isEngaged() && f.isRoommate(base.Name))
				{
					return true;
				}
			}
			return false;
		}

		public bool isMarried()
		{
			if (!isVillager())
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.spouse != null && f.spouse.Equals(base.Name) && !f.isEngaged())
				{
					return true;
				}
			}
			return false;
		}

		public bool isMarriedOrEngaged()
		{
			if (!isVillager())
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.spouse != null && f.spouse.Equals(base.Name))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void dayUpdate(int dayOfMonth)
		{
			isMovingOnPathFindPath.Value = false;
			queuedSchedulePaths.Clear();
			lastAttemptedSchedule = -1;
			if (layingDown)
			{
				layingDown = false;
				HideShadow = false;
			}
			exploreFarm.Value = false;
			shouldWearIslandAttire.Value = false;
			if (isWearingIslandAttire)
			{
				wearNormalClothes();
			}
			layingDown = false;
			if (appliedRouteAnimationOffset != Vector2.Zero)
			{
				drawOffset.Value = Vector2.Zero;
				appliedRouteAnimationOffset = Vector2.Zero;
			}
			if (base.currentLocation != null && defaultMap.Value != null)
			{
				Game1.warpCharacter(this, defaultMap, defaultPosition.Value / 64f);
			}
			if (base.Name.Equals("Maru") || base.Name.Equals("Shane"))
			{
				Sprite.LoadTexture("Characters\\" + getTextureNameForCharacter(base.Name));
			}
			if (base.Name.Equals("Willy") || base.Name.Equals("Clint"))
			{
				Sprite.SpriteWidth = 16;
				Sprite.SpriteHeight = 32;
				Sprite.ignoreSourceRectUpdates = false;
				Sprite.UpdateSourceRect();
				IsInvisible = false;
			}
			if (Game1.IsMasterGame && base.Name.Equals("Elliott") && Game1.netWorldState.Value.hasWorldStateID("elliottGone"))
			{
				daysUntilNotInvisible = 7;
				Game1.netWorldState.Value.removeWorldStateID("elliottGone");
				Game1.worldStateIDs.Remove("elliottGone");
			}
			drawOffset.Value = Vector2.Zero;
			if (Game1.IsMasterGame && daysUntilNotInvisible > 0)
			{
				IsInvisible = true;
				daysUntilNotInvisible--;
				if (daysUntilNotInvisible <= 0)
				{
					IsInvisible = false;
				}
			}
			resetForNewDay(dayOfMonth);
			updateConstructionAnimation();
			if (isMarried() && !getSpouse().divorceTonight && !IsInvisible)
			{
				marriageDuties();
			}
		}

		public virtual void resetForNewDay(int dayOfMonth)
		{
			sleptInBed.Value = true;
			if (isMarried() && !isRoommate())
			{
				FarmHouse house = Utility.getHomeOfFarmer(getSpouse());
				if (house != null && house.GetSpouseBed() == null)
				{
					sleptInBed.Value = false;
				}
			}
			doingEndOfRouteAnimation.Value = false;
			Halt();
			hasBeenKissedToday.Value = false;
			currentMarriageDialogue.Clear();
			marriageDefaultDialogue.Value = null;
			shouldSayMarriageDialogue.Value = false;
			isSleeping.Value = false;
			drawOffset.Value = Vector2.Zero;
			faceTowardFarmer = false;
			faceTowardFarmerTimer = 0;
			drawOffset.Value = Vector2.Zero;
			hasSaidAfternoonDialogue.Value = false;
			isPlayingSleepingAnimation = false;
			ignoreScheduleToday = false;
			Halt();
			controller = null;
			temporaryController = null;
			directionsToNewLocation = null;
			faceDirection(DefaultFacingDirection);
			previousEndPoint = new Point((int)defaultPosition.X / 64, (int)defaultPosition.Y / 64);
			isWalkingInSquare = false;
			returningToEndPoint = false;
			lastCrossroad = Microsoft.Xna.Framework.Rectangle.Empty;
			_startedEndOfRouteBehavior = null;
			_finishingEndOfRouteBehavior = null;
			if (isVillager())
			{
				Schedule = getSchedule(dayOfMonth);
			}
			endOfRouteMessage.Value = null;
		}

		public void returnHomeFromFarmPosition(Farm farm)
		{
			Farmer farmer = getSpouse();
			if (farmer == null)
			{
				return;
			}
			FarmHouse farm_house = Utility.getHomeOfFarmer(farmer);
			Point porchPoint = farm_house.getPorchStandingSpot();
			drawOffset.Value = Vector2.Zero;
			if (exploreFarm.Value)
			{
				GameLocation home2 = getHome();
				string nameOfHome2 = home2.Name;
				if (home2.uniqueName.Value != null)
				{
					nameOfHome2 = home2.uniqueName.Value;
				}
				base.willDestroyObjectsUnderfoot = true;
				Point destination2 = farm.getWarpPointTo(nameOfHome2, this);
				Game1.player.getSpouse().PathToOnFarm(destination2);
			}
			else if (getTileLocationPoint().Equals(porchPoint))
			{
				GameLocation home = getHome();
				string nameOfHome = home.Name;
				if (home.uniqueName.Value != null)
				{
					nameOfHome = home.uniqueName.Value;
				}
				base.willDestroyObjectsUnderfoot = true;
				Point destination = farm.getWarpPointTo(nameOfHome, this);
				controller = new PathFindController(this, farm, destination, 0)
				{
					NPCSchedule = true
				};
			}
			else if (!shouldPlaySpousePatioAnimation.Value || !farm.farmers.Any())
			{
				Halt();
				controller = null;
				temporaryController = null;
				ignoreScheduleToday = true;
				Game1.warpCharacter(this, farm_house, Utility.PointToVector2(farm_house.getKitchenStandingSpot()));
			}
		}

		public virtual Vector2 GetSpousePatioPosition()
		{
			return Game1.getFarm().GetSpouseOutdoorAreaCorner() + new Vector2(2f, 3f);
		}

		public void setUpForOutdoorPatioActivity()
		{
			Vector2 patio_location = GetSpousePatioPosition();
			if (!checkTileOccupancyForSpouse(Game1.getFarm(), patio_location))
			{
				Game1.warpCharacter(this, "Farm", patio_location);
				popOffAnyNonEssentialItems();
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", "patio_" + base.Name, false);
				switch ((string)name)
				{
				case "Emily":
					patio_location.X += -1f;
					break;
				case "Shane":
					patio_location.X += -2f;
					break;
				case "Sam":
					patio_location.Y += -1f;
					break;
				case "Elliott":
					patio_location.Y += -1f;
					break;
				case "Harvey":
					patio_location.Y += -1f;
					break;
				case "Alex":
					patio_location.Y += -1f;
					break;
				case "Maru":
					patio_location.X += -1f;
					patio_location.Y += -1f;
					break;
				case "Penny":
					patio_location.Y += -1f;
					break;
				case "Haley":
					patio_location.Y += -1f;
					patio_location.X += -1f;
					break;
				case "Abigail":
					patio_location.Y += -1f;
					break;
				case "Leah":
					patio_location.Y += -1f;
					break;
				}
				setTilePosition((int)patio_location.X, (int)patio_location.Y);
				shouldPlaySpousePatioAnimation.Value = true;
			}
		}

		private void doPlaySpousePatioAnimation()
		{
			switch ((string)name)
			{
			case "Emily":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(54, 4000, 64, secondaryArm: false, flip: false)
				});
				break;
			case "Shane":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(28, 4000, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(29, 800, 64, secondaryArm: false, flip: false)
				});
				break;
			case "Sebastian":
				drawOffset.Value = new Vector2(16f, 40f);
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(32, 500, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(36, 500, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(32, 500, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(36, 500, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(32, 500, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(36, 500, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(32, 500, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(36, 2000, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(33, 100, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(34, 100, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(35, 3000, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(34, 100, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(33, 100, 64, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(32, 1500, 64, secondaryArm: false, flip: false)
				});
				break;
			case "Sam":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(25, 3000),
					new FarmerSprite.AnimationFrame(27, 500),
					new FarmerSprite.AnimationFrame(26, 100),
					new FarmerSprite.AnimationFrame(28, 100),
					new FarmerSprite.AnimationFrame(27, 500),
					new FarmerSprite.AnimationFrame(25, 2000),
					new FarmerSprite.AnimationFrame(27, 500),
					new FarmerSprite.AnimationFrame(26, 100),
					new FarmerSprite.AnimationFrame(29, 100),
					new FarmerSprite.AnimationFrame(30, 100),
					new FarmerSprite.AnimationFrame(32, 500),
					new FarmerSprite.AnimationFrame(31, 1000),
					new FarmerSprite.AnimationFrame(30, 100),
					new FarmerSprite.AnimationFrame(29, 100)
				});
				break;
			case "Elliott":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(33, 3000),
					new FarmerSprite.AnimationFrame(32, 500),
					new FarmerSprite.AnimationFrame(33, 3000),
					new FarmerSprite.AnimationFrame(32, 500),
					new FarmerSprite.AnimationFrame(33, 2000),
					new FarmerSprite.AnimationFrame(34, 1500)
				});
				break;
			case "Harvey":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(42, 6000),
					new FarmerSprite.AnimationFrame(43, 1000),
					new FarmerSprite.AnimationFrame(39, 100),
					new FarmerSprite.AnimationFrame(43, 500),
					new FarmerSprite.AnimationFrame(39, 100),
					new FarmerSprite.AnimationFrame(43, 1000),
					new FarmerSprite.AnimationFrame(42, 5000),
					new FarmerSprite.AnimationFrame(43, 3000)
				});
				break;
			case "Alex":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(34, 4000),
					new FarmerSprite.AnimationFrame(33, 300),
					new FarmerSprite.AnimationFrame(28, 200),
					new FarmerSprite.AnimationFrame(29, 100),
					new FarmerSprite.AnimationFrame(30, 100),
					new FarmerSprite.AnimationFrame(31, 100),
					new FarmerSprite.AnimationFrame(32, 100),
					new FarmerSprite.AnimationFrame(31, 100),
					new FarmerSprite.AnimationFrame(30, 100),
					new FarmerSprite.AnimationFrame(29, 100),
					new FarmerSprite.AnimationFrame(28, 800),
					new FarmerSprite.AnimationFrame(29, 100),
					new FarmerSprite.AnimationFrame(30, 100),
					new FarmerSprite.AnimationFrame(31, 100),
					new FarmerSprite.AnimationFrame(32, 100),
					new FarmerSprite.AnimationFrame(31, 100),
					new FarmerSprite.AnimationFrame(30, 100),
					new FarmerSprite.AnimationFrame(29, 100),
					new FarmerSprite.AnimationFrame(28, 800),
					new FarmerSprite.AnimationFrame(33, 200)
				});
				break;
			case "Maru":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(16, 4000),
					new FarmerSprite.AnimationFrame(17, 200),
					new FarmerSprite.AnimationFrame(18, 200),
					new FarmerSprite.AnimationFrame(19, 200),
					new FarmerSprite.AnimationFrame(20, 200),
					new FarmerSprite.AnimationFrame(21, 200),
					new FarmerSprite.AnimationFrame(22, 200),
					new FarmerSprite.AnimationFrame(23, 200)
				});
				break;
			case "Penny":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(18, 6000),
					new FarmerSprite.AnimationFrame(19, 500)
				});
				break;
			case "Haley":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(30, 2000),
					new FarmerSprite.AnimationFrame(31, 200),
					new FarmerSprite.AnimationFrame(24, 2000),
					new FarmerSprite.AnimationFrame(25, 1000),
					new FarmerSprite.AnimationFrame(32, 200),
					new FarmerSprite.AnimationFrame(33, 2000),
					new FarmerSprite.AnimationFrame(32, 200),
					new FarmerSprite.AnimationFrame(25, 2000),
					new FarmerSprite.AnimationFrame(32, 200),
					new FarmerSprite.AnimationFrame(33, 2000)
				});
				break;
			case "Abigail":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(16, 500),
					new FarmerSprite.AnimationFrame(17, 500),
					new FarmerSprite.AnimationFrame(18, 500),
					new FarmerSprite.AnimationFrame(19, 500)
				});
				break;
			case "Leah":
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(16, 100),
					new FarmerSprite.AnimationFrame(17, 100),
					new FarmerSprite.AnimationFrame(18, 100),
					new FarmerSprite.AnimationFrame(19, 300),
					new FarmerSprite.AnimationFrame(16, 100),
					new FarmerSprite.AnimationFrame(17, 100),
					new FarmerSprite.AnimationFrame(18, 100),
					new FarmerSprite.AnimationFrame(19, 1000),
					new FarmerSprite.AnimationFrame(16, 100),
					new FarmerSprite.AnimationFrame(17, 100),
					new FarmerSprite.AnimationFrame(18, 100),
					new FarmerSprite.AnimationFrame(19, 300),
					new FarmerSprite.AnimationFrame(16, 100),
					new FarmerSprite.AnimationFrame(17, 100),
					new FarmerSprite.AnimationFrame(18, 100),
					new FarmerSprite.AnimationFrame(19, 300),
					new FarmerSprite.AnimationFrame(16, 100),
					new FarmerSprite.AnimationFrame(17, 100),
					new FarmerSprite.AnimationFrame(18, 100),
					new FarmerSprite.AnimationFrame(19, 2000)
				});
				break;
			}
		}

		public bool isGaySpouse()
		{
			Farmer spouse = getSpouse();
			if (spouse != null)
			{
				if (Gender != 0 || !spouse.IsMale)
				{
					if (Gender == 1)
					{
						return !spouse.IsMale;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public bool canGetPregnant()
		{
			if (this is Horse || base.Name.Equals("Krobus") || isRoommate())
			{
				return false;
			}
			Farmer spouse = getSpouse();
			if (spouse == null || (bool)spouse.divorceTonight)
			{
				return false;
			}
			int heartsWithSpouse = spouse.getFriendshipHeartLevelForNPC(base.Name);
			Friendship friendship = spouse.GetSpouseFriendship();
			List<Child> kids = spouse.getChildren();
			defaultMap.Value = spouse.homeLocation.Value;
			FarmHouse farmHouse = Utility.getHomeOfFarmer(spouse);
			if (farmHouse.cribStyle.Value <= 0)
			{
				return false;
			}
			if (farmHouse.upgradeLevel >= 2 && friendship.DaysUntilBirthing < 0 && heartsWithSpouse >= 10 && spouse.GetDaysMarried() >= 7)
			{
				if (kids.Count != 0)
				{
					if (kids.Count < 2)
					{
						return kids[0].Age > 2;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public void marriageDuties()
		{
			if (!Game1.newDay && Game1.gameMode != 6)
			{
				return;
			}
			Farmer spouse = getSpouse();
			if (spouse == null)
			{
				return;
			}
			shouldSayMarriageDialogue.Value = true;
			DefaultMap = spouse.homeLocation.Value;
			FarmHouse farmHouse = Game1.getLocationFromName(spouse.homeLocation.Value) as FarmHouse;
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)spouse.UniqueMultiplayerID);
			int heartsWithSpouse = spouse.getFriendshipHeartLevelForNPC(base.Name);
			if (Game1.IsMasterGame && (base.currentLocation == null || !base.currentLocation.Equals(farmHouse)))
			{
				Game1.warpCharacter(this, spouse.homeLocation.Value, farmHouse.getSpouseBedSpot(base.Name));
			}
			if (Game1.isRaining)
			{
				marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Rainy_Day_" + r.Next(5), false);
			}
			else
			{
				marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Indoor_Day_" + r.Next(5), false);
			}
			currentMarriageDialogue.Add(new MarriageDialogueReference(marriageDefaultDialogue.Value.DialogueFile, marriageDefaultDialogue.Value.DialogueKey, marriageDefaultDialogue.Value.IsGendered, marriageDefaultDialogue.Value.Substitutions));
			if (spouse.GetSpouseFriendship().DaysUntilBirthing == 0)
			{
				setTilePosition(farmHouse.getKitchenStandingSpot());
				currentMarriageDialogue.Clear();
				return;
			}
			if (daysAfterLastBirth >= 0)
			{
				daysAfterLastBirth--;
				switch (getSpouse().getChildrenCount())
				{
				case 1:
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false);
					}
					return;
				case 2:
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false);
					}
					return;
				}
			}
			setTilePosition(farmHouse.getKitchenStandingSpot());
			if (!sleptInBed.Value)
			{
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", "NoBed_" + r.Next(4), false);
				return;
			}
			if (tryToGetMarriageSpecificDialogueElseReturnDefault(Game1.currentSeason + "_" + Game1.dayOfMonth).Length > 0)
			{
				if (spouse != null)
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + Game1.dayOfMonth, false);
				}
				return;
			}
			if (schedule != null)
			{
				if (nameOfTodaysSchedule.Equals("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "funLeave_" + base.Name, false);
				}
				else if (nameOfTodaysSchedule.Equals("marriageJob"))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "jobLeave_" + base.Name, false);
				}
				return;
			}
			if (!Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && spouse == Game1.MasterPlayer && !base.Name.Equals("Krobus"))
			{
				setUpForOutdoorPatioActivity();
				return;
			}
			if (spouse.GetDaysMarried() >= 1 && r.NextDouble() < (double)(1f - (float)Math.Max(1, heartsWithSpouse) / 12f))
			{
				Furniture f2 = farmHouse.getRandomFurniture(r);
				if (f2 != null && f2.isGroundFurniture() && f2.furniture_type.Value != 15 && f2.furniture_type.Value != 12)
				{
					Point p2 = new Point((int)f2.tileLocation.X - 1, (int)f2.tileLocation.Y);
					if (farmHouse.isTileLocationTotallyClearAndPlaceable(p2.X, p2.Y))
					{
						setTilePosition(p2);
						faceDirection(1);
						switch (r.Next(10))
						{
						case 0:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4420", false);
							break;
						case 1:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4421", false);
							break;
						case 2:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4422", true);
							break;
						case 3:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4423", false);
							break;
						case 4:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4424", false);
							break;
						case 5:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4425", false);
							break;
						case 6:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4426", false);
							break;
						case 7:
							if (Gender == 1)
							{
								if (r.NextDouble() < 0.5)
								{
									currentMarriageDialogue.Clear();
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4427", false);
								}
								else
								{
									currentMarriageDialogue.Clear();
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4429", false);
								}
							}
							else
							{
								currentMarriageDialogue.Clear();
								addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4431", false);
							}
							break;
						case 8:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4432", false);
							break;
						case 9:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4433", false);
							break;
						}
						return;
					}
				}
				switch (r.Next(5))
				{
				case 0:
					new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4434", false);
					break;
				case 1:
					new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4435", false);
					break;
				case 2:
					new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4436", false);
					break;
				case 3:
					new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4437", true);
					break;
				case 4:
					new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4438", false);
					break;
				}
				spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse, force: true);
				return;
			}
			Friendship friendship = spouse.GetSpouseFriendship();
			if (friendship.DaysUntilBirthing != -1 && friendship.DaysUntilBirthing <= 7 && r.NextDouble() < 0.5)
			{
				if (isGaySpouse())
				{
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4439", false), farmHouse))
					{
						if (r.NextDouble() < 0.5)
						{
							currentMarriageDialogue.Clear();
						}
						if (r.NextDouble() < 0.5)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4440", false, getSpouse().displayName);
						}
						else
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4441", false, "%endearment");
						}
					}
					return;
				}
				if (Gender == 1)
				{
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck((r.NextDouble() < 0.5) ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4442", false) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4443", false), farmHouse))
					{
						if (r.NextDouble() < 0.5)
						{
							currentMarriageDialogue.Clear();
						}
						currentMarriageDialogue.Add((r.NextDouble() < 0.5) ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4444", false, getSpouse().displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4445", false, "%endearment"));
					}
					return;
				}
				setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4446", true), farmHouse))
				{
					if (r.NextDouble() < 0.5)
					{
						currentMarriageDialogue.Clear();
					}
					currentMarriageDialogue.Add((r.NextDouble() < 0.5) ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4447", true, getSpouse().displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4448", false, "%endearment"));
				}
				return;
			}
			if (r.NextDouble() < 0.07)
			{
				switch (getSpouse().getChildrenCount())
				{
				case 1:
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4449", true), farmHouse))
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false);
					}
					return;
				case 2:
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4452", true), farmHouse))
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false);
					}
					return;
				}
			}
			Farm farm = Game1.getFarm();
			if (currentMarriageDialogue.Count > 0 && currentMarriageDialogue[0].IsItemGrabDialogue(this))
			{
				setTilePosition(farmHouse.getKitchenStandingSpot());
				spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4455", true), farmHouse);
			}
			else if (!Game1.isRaining && r.NextDouble() < 0.4 && !checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())) && !base.Name.Equals("Krobus"))
			{
				bool filledBowl = false;
				if (!farm.petBowlWatered.Value && !hasSomeoneFedThePet)
				{
					filledBowl = true;
					farm.petBowlWatered.Set(newValue: true);
					hasSomeoneFedThePet = true;
				}
				if (r.NextDouble() < 0.6 && !Game1.currentSeason.Equals("winter") && !hasSomeoneWateredCrops)
				{
					Vector2 origin2 = Vector2.Zero;
					int tries2 = 0;
					bool foundWatered = false;
					for (; tries2 < Math.Min(50, farm.terrainFeatures.Count()); tries2++)
					{
						if (!origin2.Equals(Vector2.Zero))
						{
							break;
						}
						int index2 = r.Next(farm.terrainFeatures.Count());
						if (farm.terrainFeatures.Pairs.ElementAt(index2).Value is HoeDirt)
						{
							if ((farm.terrainFeatures.Pairs.ElementAt(index2).Value as HoeDirt).needsWatering())
							{
								origin2 = farm.terrainFeatures.Pairs.ElementAt(index2).Key;
							}
							else if ((farm.terrainFeatures.Pairs.ElementAt(index2).Value as HoeDirt).crop != null)
							{
								foundWatered = true;
							}
						}
					}
					if (!origin2.Equals(Vector2.Zero))
					{
						Microsoft.Xna.Framework.Rectangle wateringArea2 = new Microsoft.Xna.Framework.Rectangle((int)origin2.X - 30, (int)origin2.Y - 30, 60, 60);
						Vector2 currentPosition = default(Vector2);
						for (int x2 = wateringArea2.X; x2 < wateringArea2.Right; x2++)
						{
							for (int y2 = wateringArea2.Y; y2 < wateringArea2.Bottom; y2++)
							{
								currentPosition.X = x2;
								currentPosition.Y = y2;
								if (farm.isTileOnMap(currentPosition) && farm.terrainFeatures.ContainsKey(currentPosition) && farm.terrainFeatures[currentPosition] is HoeDirt && Game1.IsMasterGame && (farm.terrainFeatures[currentPosition] as HoeDirt).needsWatering())
								{
									(farm.terrainFeatures[currentPosition] as HoeDirt).state.Value = 1;
								}
							}
						}
						faceDirection(2);
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
						if (filledBowl)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
						hasSomeoneWateredCrops = true;
					}
					else
					{
						faceDirection(2);
						if (foundWatered)
						{
							currentMarriageDialogue.Clear();
							if (Game1.gameMode == 6)
							{
								if (r.NextDouble() < 0.5)
								{
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4465", false, "%endearment");
								}
								else
								{
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4466", false, "%endearment");
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
									if (filledBowl)
									{
										addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
									}
								}
							}
							else
							{
								currentMarriageDialogue.Clear();
								addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4470", true);
							}
						}
						else
						{
							currentMarriageDialogue.Clear();
							addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
						}
					}
				}
				else if (r.NextDouble() < 0.6 && !hasSomeoneFedTheAnimals)
				{
					bool fedAnything = false;
					foreach (Building b in farm.buildings)
					{
						if ((b is Barn || b is Coop) && (int)b.daysOfConstructionLeft <= 0)
						{
							if (Game1.IsMasterGame)
							{
								(b.indoors.Value as AnimalHouse).feedAllAnimals();
							}
							fedAnything = true;
						}
					}
					faceDirection(2);
					if (fedAnything)
					{
						hasSomeoneFedTheAnimals = true;
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4474", true);
						if (filledBowl)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
					}
					else
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
					}
					if (Game1.IsMasterGame)
					{
						farm.petBowlWatered.Set(newValue: true);
					}
				}
				else if (!hasSomeoneRepairedTheFences)
				{
					int tries = 0;
					faceDirection(2);
					Vector2 origin = Vector2.Zero;
					for (; tries < Math.Min(50, farm.objects.Count()); tries++)
					{
						if (!origin.Equals(Vector2.Zero))
						{
							break;
						}
						int index = r.Next(farm.objects.Count());
						if (farm.objects.Pairs.ElementAt(index).Value is Fence)
						{
							origin = farm.objects.Pairs.ElementAt(index).Key;
						}
					}
					if (!origin.Equals(Vector2.Zero))
					{
						Microsoft.Xna.Framework.Rectangle wateringArea = new Microsoft.Xna.Framework.Rectangle((int)origin.X - 10, (int)origin.Y - 10, 20, 20);
						Vector2 currentPosition2 = default(Vector2);
						for (int x = wateringArea.X; x < wateringArea.Right; x++)
						{
							for (int y = wateringArea.Y; y < wateringArea.Bottom; y++)
							{
								currentPosition2.X = x;
								currentPosition2.Y = y;
								if (farm.isTileOnMap(currentPosition2) && farm.objects.ContainsKey(currentPosition2) && farm.objects[currentPosition2] is Fence && Game1.IsMasterGame)
								{
									(farm.objects[currentPosition2] as Fence).repair();
								}
							}
						}
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4481", true);
						if (filledBowl)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
						hasSomeoneRepairedTheFences = true;
					}
					else
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
					}
				}
				Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
				popOffAnyNonEssentialItems();
				faceDirection(2);
			}
			else if (base.Name.Equals("Krobus") && Game1.isRaining && r.NextDouble() < 0.4 && !checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())))
			{
				addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
				popOffAnyNonEssentialItems();
				faceDirection(2);
			}
			else if (spouse.GetDaysMarried() >= 1 && r.NextDouble() < 0.045)
			{
				if (r.NextDouble() < 0.75)
				{
					Point spot2 = farmHouse.getRandomOpenPointInHouse(r, 1);
					Furniture new_furniture2 = null;
					try
					{
						new_furniture2 = new Furniture(Utility.getRandomSingleTileFurniture(r), new Vector2(spot2.X, spot2.Y));
					}
					catch (Exception)
					{
						new_furniture2 = null;
					}
					if (new_furniture2 != null && spot2.X > 0 && farmHouse.isTileLocationOpen(new Location(spot2.X - 1, spot2.Y)))
					{
						farmHouse.furniture.Add(new_furniture2);
						setTilePosition(spot2.X - 1, spot2.Y);
						faceDirection(1);
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4486", false, "%endearmentlower");
						if (Game1.random.NextDouble() < 0.5)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4488", true);
						}
						else
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4489", false);
						}
					}
					else
					{
						setTilePosition(farmHouse.getKitchenStandingSpot());
						spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4490", false), farmHouse);
					}
					return;
				}
				Point p = farmHouse.getRandomOpenPointInHouse(r);
				if (p.X <= 0)
				{
					return;
				}
				setTilePosition(p.X, p.Y);
				faceDirection(0);
				if (r.NextDouble() < 0.5)
				{
					int wall = farmHouse.getWallForRoomAt(p);
					if (wall != -1)
					{
						int style = r.Next(112);
						List<int> styles = new List<int>();
						switch (base.Name)
						{
						case "Sebastian":
							styles.AddRange(new int[11]
							{
								3,
								4,
								12,
								14,
								30,
								46,
								47,
								56,
								58,
								59,
								107
							});
							break;
						case "Haley":
							styles.AddRange(new int[7]
							{
								1,
								7,
								10,
								35,
								49,
								84,
								99
							});
							break;
						case "Abigail":
							styles.AddRange(new int[10]
							{
								2,
								13,
								23,
								26,
								46,
								45,
								64,
								77,
								106,
								107
							});
							break;
						case "Leah":
							styles.AddRange(new int[7]
							{
								44,
								108,
								43,
								45,
								92,
								37,
								29
							});
							break;
						case "Alex":
							styles.AddRange(new int[1]
							{
								6
							});
							break;
						case "Shane":
							styles.AddRange(new int[3]
							{
								6,
								21,
								60
							});
							break;
						case "Krobus":
							styles.AddRange(new int[2]
							{
								23,
								24
							});
							break;
						}
						if (styles.Count > 0)
						{
							style = styles[r.Next(styles.Count)];
						}
						farmHouse.setWallpaper(style, wall, persist: true);
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4496", false);
					}
				}
				else
				{
					int floor = farmHouse.getFloorAt(p);
					if (floor != -1)
					{
						farmHouse.setFloor(r.Next(40), floor, persist: true);
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4497", false);
					}
				}
			}
			else if (Game1.isRaining && r.NextDouble() < 0.08 && heartsWithSpouse < 11 && base.Name != "Krobus")
			{
				foreach (Furniture f in farmHouse.furniture)
				{
					if ((int)f.furniture_type == 13 && farmHouse.isTileLocationTotallyClearAndPlaceable((int)f.tileLocation.X, (int)f.tileLocation.Y + 1))
					{
						setTilePosition((int)f.tileLocation.X, (int)f.tileLocation.Y + 1);
						faceDirection(0);
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4498", true);
						return;
					}
				}
				spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4499", false), farmHouse, force: true);
			}
			else if (r.NextDouble() < 0.45)
			{
				Vector2 spot = (farmHouse.upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);
				setTilePosition((int)spot.X, (int)spot.Y);
				faceDirection(0);
				setSpouseRoomMarriageDialogue();
				if ((string)name == "Sebastian" && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
				{
					setTilePosition((farmHouse.upgradeLevel == 1) ? 31 : 37, (farmHouse.upgradeLevel == 1) ? 6 : 15);
					faceDirection(2);
				}
			}
			else
			{
				setTilePosition(farmHouse.getKitchenStandingSpot());
				faceDirection(0);
				if (r.NextDouble() < 0.2)
				{
					setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse);
				}
			}
		}

		public virtual void popOffAnyNonEssentialItems()
		{
			if (!Game1.IsMasterGame || base.currentLocation == null)
			{
				return;
			}
			Object tile_object = base.currentLocation.getObjectAtTile(getTileX(), getTileY());
			if (tile_object != null)
			{
				bool pop_off = false;
				if (Utility.IsNormalObjectAtParentSheetIndex(tile_object, 93) || tile_object is Torch)
				{
					pop_off = true;
				}
				if (pop_off)
				{
					Vector2 tile_position = tile_object.TileLocation;
					tile_object.performRemoveAction(tile_position, base.currentLocation);
					base.currentLocation.objects.Remove(tile_position);
					tile_object.dropItem(base.currentLocation, tile_position * 64f, tile_position * 64f);
				}
			}
		}

		public static bool checkTileOccupancyForSpouse(GameLocation location, Vector2 point, string characterToIgnore = "")
		{
			if (location == null)
			{
				return true;
			}
			isCheckingSpouseTileOccupancy = true;
			bool result = location.isTileOccupied(point, characterToIgnore);
			isCheckingSpouseTileOccupancy = false;
			return result;
		}

		public void addMarriageDialogue(string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
		{
			shouldSayMarriageDialogue.Value = true;
			currentMarriageDialogue.Add(new MarriageDialogueReference(dialogue_file, dialogue_key, gendered, substitutions));
		}

		public void clearTextAboveHead()
		{
			textAboveHead = null;
			textAboveHeadPreTimer = -1;
			textAboveHeadTimer = -1;
		}

		public bool isVillager()
		{
			if (!IsMonster && !(this is Child) && !(this is Pet) && !(this is Horse) && !(this is Junimo))
			{
				return !(this is JunimoHarvester);
			}
			return false;
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			if (isMarried() && (Schedule == null || location is FarmHouse))
			{
				return true;
			}
			return base.shouldCollideWithBuildingLayer(location);
		}

		public void arriveAtFarmHouse(FarmHouse farmHouse)
		{
			if (Game1.newDay || !isMarried() || Game1.timeOfDay <= 630 || getTileLocationPoint().Equals(farmHouse.getSpouseBedSpot(name)))
			{
				return;
			}
			setTilePosition(farmHouse.getEntryLocation());
			ignoreScheduleToday = true;
			temporaryController = null;
			controller = null;
			if (Game1.timeOfDay >= 2130)
			{
				Point bed_spot = farmHouse.getSpouseBedSpot(name);
				bool found_bed = farmHouse.GetSpouseBed() != null;
				PathFindController.endBehavior end_behavior = null;
				if (found_bed)
				{
					end_behavior = FarmHouse.spouseSleepEndFunction;
				}
				controller = new PathFindController(this, farmHouse, bed_spot, 0, end_behavior);
				if (controller.pathToEndPoint != null && found_bed)
				{
					foreach (Furniture furniture in farmHouse.furniture)
					{
						if (furniture is BedFurniture && furniture.getBoundingBox(furniture.TileLocation).Intersects(new Microsoft.Xna.Framework.Rectangle(bed_spot.X * 64, bed_spot.Y * 64, 64, 64)))
						{
							(furniture as BedFurniture).ReserveForNPC();
							break;
						}
					}
				}
			}
			else
			{
				controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
			}
			if (controller.pathToEndPoint == null)
			{
				base.willDestroyObjectsUnderfoot = true;
				controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
				setNewDialogue(Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4500"));
			}
			else if (Game1.timeOfDay > 1300)
			{
				if (nameOfTodaysSchedule.Equals("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
				{
					setNewDialogue("MarriageDialogue", "funReturn_", -1, add: false, clearOnMovement: true);
				}
				else if (nameOfTodaysSchedule.Equals("marriageJob"))
				{
					setNewDialogue("MarriageDialogue", "jobReturn_");
				}
				else if (Game1.timeOfDay < 1800)
				{
					setRandomAfternoonMarriageDialogue(Game1.timeOfDay, base.currentLocation, countAsDailyAfternoon: true);
				}
			}
			if (Game1.currentLocation == farmHouse)
			{
				Game1.currentLocation.playSound("doorClose", NetAudio.SoundContext.NPC);
			}
		}

		public Farmer getSpouse()
		{
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.spouse != null && f.spouse.Equals(base.Name))
				{
					return f;
				}
			}
			return null;
		}

		public string getTermOfSpousalEndearment(bool happy = true)
		{
			Farmer spouse = getSpouse();
			if (spouse != null)
			{
				if (isRoommate())
				{
					return spouse.displayName;
				}
				if (spouse.getFriendshipHeartLevelForNPC(base.Name) < 9)
				{
					return spouse.displayName;
				}
				if (happy && Game1.random.NextDouble() < 0.08)
				{
					switch (Game1.random.Next(8))
					{
					case 0:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4507");
					case 1:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4508");
					case 2:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4509");
					case 3:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4510");
					case 4:
						if (!spouse.IsMale)
						{
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4512");
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4511");
					case 5:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4513");
					case 6:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4514");
					case 7:
						if (!spouse.IsMale)
						{
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4516");
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4515");
					}
				}
				if (!happy)
				{
					switch (Game1.random.Next(2))
					{
					case 0:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
					case 1:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518");
					case 2:
						return spouse.displayName;
					}
				}
				switch (Game1.random.Next(5))
				{
				case 0:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4519");
				case 1:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518");
				case 2:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
				case 3:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4522");
				case 4:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4523");
				}
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
		}

		public bool spouseObstacleCheck(MarriageDialogueReference backToBedMessage, GameLocation currentLocation, bool force = false)
		{
			if (force || checkTileOccupancyForSpouse(currentLocation, getTileLocation(), base.Name))
			{
				Game1.warpCharacter(this, defaultMap, (Game1.getLocationFromName(defaultMap) as FarmHouse).getSpouseBedSpot(name));
				faceDirection(1);
				currentMarriageDialogue.Clear();
				currentMarriageDialogue.Add(backToBedMessage);
				shouldSayMarriageDialogue.Value = true;
				return true;
			}
			return false;
		}

		public void setTilePosition(Point p)
		{
			setTilePosition(p.X, p.Y);
		}

		public void setTilePosition(int x, int y)
		{
			base.Position = new Vector2(x * 64, y * 64);
		}

		private void clintHammerSound(Farmer who)
		{
			base.currentLocation.playSoundAt("hammer", getTileLocation());
		}

		private void robinHammerSound(Farmer who)
		{
			if (Game1.currentLocation.Equals(base.currentLocation) && Utility.isOnScreen(base.Position, 256))
			{
				Game1.playSound((Game1.random.NextDouble() < 0.1) ? "clank" : "axchop");
				shakeTimer = 250;
			}
		}

		private void robinVariablePause(Farmer who)
		{
			if (Game1.random.NextDouble() < 0.4)
			{
				Sprite.CurrentAnimation[Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, 300, secondaryArm: false, flip: false, robinVariablePause);
			}
			else if (Game1.random.NextDouble() < 0.25)
			{
				Sprite.CurrentAnimation[Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(23, Game1.random.Next(500, 4000), secondaryArm: false, flip: false, robinVariablePause);
			}
			else
			{
				Sprite.CurrentAnimation[Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, Game1.random.Next(1000, 4000), secondaryArm: false, flip: false, robinVariablePause);
			}
		}

		public void ReachedEndPoint()
		{
		}

		public void changeSchedulePathDirection()
		{
			Microsoft.Xna.Framework.Rectangle boundingbox = GetBoundingBox();
			boundingbox.Inflate(2, 2);
			_ = lastCrossroad;
			if (!lastCrossroad.Intersects(boundingbox))
			{
				isCharging = false;
				base.speed = 2;
				directionIndex++;
				lastCrossroad = new Microsoft.Xna.Framework.Rectangle(getStandingX() - getStandingX() % 64, getStandingY() - getStandingY() % 64, 64, 64);
				moveCharacterOnSchedulePath();
			}
		}

		public void moveCharacterOnSchedulePath()
		{
		}

		public void randomSquareMovement(GameTime time)
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
			boundingBox.Inflate(2, 2);
			Microsoft.Xna.Framework.Rectangle endRect = new Microsoft.Xna.Framework.Rectangle((int)nextSquarePosition.X * 64, (int)nextSquarePosition.Y * 64, 64, 64);
			_ = nextSquarePosition;
			if (nextSquarePosition.Equals(Vector2.Zero))
			{
				squarePauseAccumulation = 0;
				squarePauseTotal = Game1.random.Next(6000 + squarePauseOffset, 12000 + squarePauseOffset);
				nextSquarePosition = new Vector2(lastCrossroad.X / 64 - lengthOfWalkingSquareX / 2 + Game1.random.Next(lengthOfWalkingSquareX), lastCrossroad.Y / 64 - lengthOfWalkingSquareY / 2 + Game1.random.Next(lengthOfWalkingSquareY));
			}
			else if (endRect.Contains(boundingBox))
			{
				Halt();
				if (squareMovementFacingPreference != -1)
				{
					faceDirection(squareMovementFacingPreference);
				}
				isCharging = false;
				base.speed = 2;
			}
			else if (boundingBox.Left <= endRect.Left)
			{
				SetMovingOnlyRight();
			}
			else if (boundingBox.Right >= endRect.Right)
			{
				SetMovingOnlyLeft();
			}
			else if (boundingBox.Top <= endRect.Top)
			{
				SetMovingOnlyDown();
			}
			else if (boundingBox.Bottom >= endRect.Bottom)
			{
				SetMovingOnlyUp();
			}
			squarePauseAccumulation += time.ElapsedGameTime.Milliseconds;
			if (squarePauseAccumulation >= squarePauseTotal && endRect.Contains(boundingBox))
			{
				nextSquarePosition = Vector2.Zero;
				isCharging = false;
				base.speed = 2;
			}
		}

		public void returnToEndPoint()
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
			boundingBox.Inflate(2, 2);
			if (boundingBox.Left <= lastCrossroad.Left)
			{
				SetMovingOnlyRight();
			}
			else if (boundingBox.Right >= lastCrossroad.Right)
			{
				SetMovingOnlyLeft();
			}
			else if (boundingBox.Top <= lastCrossroad.Top)
			{
				SetMovingOnlyDown();
			}
			else if (boundingBox.Bottom >= lastCrossroad.Bottom)
			{
				SetMovingOnlyUp();
			}
			boundingBox.Inflate(-2, -2);
			if (lastCrossroad.Contains(boundingBox))
			{
				isWalkingInSquare = false;
				nextSquarePosition = Vector2.Zero;
				returningToEndPoint = false;
				Halt();
			}
		}

		public void SetMovingOnlyUp()
		{
			moveUp = true;
			moveDown = false;
			moveLeft = false;
			moveRight = false;
		}

		public void SetMovingOnlyRight()
		{
			moveUp = false;
			moveDown = false;
			moveLeft = false;
			moveRight = true;
		}

		public void SetMovingOnlyDown()
		{
			moveUp = false;
			moveDown = true;
			moveLeft = false;
			moveRight = false;
		}

		public void SetMovingOnlyLeft()
		{
			moveUp = false;
			moveDown = false;
			moveLeft = true;
			moveRight = false;
		}

		public static void populateRoutesFromLocationToLocationList()
		{
			routesFromLocationToLocation = new List<List<string>>();
			foreach (GameLocation i in Game1.locations)
			{
				if (!(i is Farm) && !i.name.Equals("Backwoods"))
				{
					List<string> route = new List<string>();
					exploreWarpPoints(i, route);
				}
			}
		}

		private static bool exploreWarpPoints(GameLocation l, List<string> route)
		{
			bool added = false;
			if (l != null && !route.Contains(l.name, StringComparer.Ordinal))
			{
				route.Add(l.name);
				if (route.Count == 1 || !doesRoutesListContain(route))
				{
					if (route.Count > 1)
					{
						routesFromLocationToLocation.Add(route.ToList());
						added = true;
					}
					foreach (Warp warp in l.warps)
					{
						string name2 = warp.TargetName;
						if (name2 == "BoatTunnel")
						{
							name2 = "IslandSouth";
						}
						if (!name2.Equals("Farm", StringComparison.Ordinal) && !name2.Equals("Woods", StringComparison.Ordinal) && !name2.Equals("Backwoods", StringComparison.Ordinal) && !name2.Equals("Tunnel", StringComparison.Ordinal) && !name2.Contains("Volcano"))
						{
							exploreWarpPoints(Game1.getLocationFromName(name2), route);
						}
					}
					foreach (Point p in l.doors.Keys)
					{
						string name = l.doors[p];
						if (name == "BoatTunnel")
						{
							name = "IslandSouth";
						}
						exploreWarpPoints(Game1.getLocationFromName(name), route);
					}
				}
				if (route.Count > 0)
				{
					route.RemoveAt(route.Count - 1);
				}
			}
			return added;
		}

		private static bool doesRoutesListContain(List<string> route)
		{
			foreach (List<string> j in routesFromLocationToLocation)
			{
				if (j.Count == route.Count)
				{
					bool allSame = true;
					for (int i = 0; i < route.Count; i++)
					{
						if (!j[i].Equals(route[i], StringComparison.Ordinal))
						{
							allSame = false;
							break;
						}
					}
					if (allSame)
					{
						return true;
					}
				}
			}
			return false;
		}

		public int CompareTo(object obj)
		{
			if (obj is NPC)
			{
				return (obj as NPC).id - id;
			}
			return 0;
		}

		public virtual void Removed()
		{
		}
	}
}
