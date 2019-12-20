using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
using StardewValley.Tools;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	public class Farmer : Character, IComparable
	{
		public class EmoteType
		{
			public string emoteString = "";

			public int emoteIconIndex = -1;

			public FarmerSprite.AnimationFrame[] animationFrames;

			public bool hidden;

			public int facingDirection = 2;

			public string displayNameKey;

			public string displayName => Game1.content.LoadString(displayNameKey);

			public EmoteType(string emote_string = "", string display_name_key = "", int icon_index = -1, FarmerSprite.AnimationFrame[] frames = null, int facing_direction = 2, bool is_hidden = false)
			{
				emoteString = emote_string;
				emoteIconIndex = icon_index;
				animationFrames = frames;
				facingDirection = facing_direction;
				hidden = is_hidden;
				displayNameKey = "Strings\\UI:" + display_name_key;
			}
		}

		public const int millisecondsPerSpeedUnit = 64;

		public const byte halt = 64;

		public const byte up = 1;

		public const byte right = 2;

		public const byte down = 4;

		public const byte left = 8;

		public const byte run = 16;

		public const byte release = 32;

		public const int farmingSkill = 0;

		public const int miningSkill = 3;

		public const int fishingSkill = 1;

		public const int foragingSkill = 2;

		public const int combatSkill = 4;

		public const int luckSkill = 5;

		public const float interpolationConstant = 0.5f;

		public const int runningSpeed = 5;

		public const int walkingSpeed = 2;

		public const int caveNothing = 0;

		public const int caveBats = 1;

		public const int caveMushrooms = 2;

		public const int millisecondsInvincibleAfterDamage = 1200;

		public const int millisecondsPerFlickerWhenInvincible = 50;

		public const int startingStamina = 270;

		public const int totalLevels = 35;

		public static int tileSlideThreshold = 32;

		public const int maxInventorySpace = 36;

		public const int hotbarSize = 12;

		public const int eyesOpen = 0;

		public const int eyesHalfShut = 4;

		public const int eyesClosed = 1;

		public const int eyesRight = 2;

		public const int eyesLeft = 3;

		public const int eyesWide = 5;

		public const int rancher = 0;

		public const int tiller = 1;

		public const int butcher = 2;

		public const int shepherd = 3;

		public const int artisan = 4;

		public const int agriculturist = 5;

		public const int fisher = 6;

		public const int trapper = 7;

		public const int angler = 8;

		public const int pirate = 9;

		public const int baitmaster = 10;

		public const int mariner = 11;

		public const int forester = 12;

		public const int gatherer = 13;

		public const int lumberjack = 14;

		public const int tapper = 15;

		public const int botanist = 16;

		public const int tracker = 17;

		public const int miner = 18;

		public const int geologist = 19;

		public const int blacksmith = 20;

		public const int burrower = 21;

		public const int excavator = 22;

		public const int gemologist = 23;

		public const int fighter = 24;

		public const int scout = 25;

		public const int brute = 26;

		public const int defender = 27;

		public const int acrobat = 28;

		public const int desperado = 29;

		public readonly NetObjectList<Quest> questLog = new NetObjectList<Quest>();

		public readonly NetIntList professions = new NetIntList();

		public readonly NetList<Point, NetPoint> newLevels = new NetList<Point, NetPoint>();

		private Queue<int> newLevelSparklingTexts = new Queue<int>();

		private SparklingText sparklingText;

		public readonly NetArray<int, NetInt> experiencePoints = new NetArray<int, NetInt>(6);

		public readonly NetObjectList<Item> items = new NetObjectList<Item>();

		public readonly NetIntList dialogueQuestionsAnswered = new NetIntList();

		public List<string> furnitureOwned = new List<string>();

		[XmlElement("cookingRecipes")]
		public readonly NetStringDictionary<int, NetInt> cookingRecipes = new NetStringDictionary<int, NetInt>();

		[XmlElement("craftingRecipes")]
		public readonly NetStringDictionary<int, NetInt> craftingRecipes = new NetStringDictionary<int, NetInt>();

		[XmlElement("activeDialogueEvents")]
		public readonly NetStringDictionary<int, NetInt> activeDialogueEvents = new NetStringDictionary<int, NetInt>();

		public readonly NetIntList eventsSeen = new NetIntList();

		public readonly NetIntList secretNotesSeen = new NetIntList();

		public List<string> songsHeard = new List<string>();

		public readonly NetIntList achievements = new NetIntList();

		public readonly NetIntList specialItems = new NetIntList();

		public readonly NetIntList specialBigCraftables = new NetIntList();

		public readonly NetStringList mailReceived = new NetStringList();

		public readonly NetStringList mailForTomorrow = new NetStringList();

		public readonly NetStringList mailbox = new NetStringList();

		public readonly NetInt timeWentToBed = new NetInt();

		public Stats stats = new Stats();

		[XmlIgnore]
		public readonly NetCollection<Item> personalShippingBin = new NetCollection<Item>();

		[XmlIgnore]
		public IList<Item> displayedShippedItems = new List<Item>();

		public List<string> blueprints = new List<string>();

		[XmlIgnore]
		protected NetRef<Item> _recoveredItem = new NetRef<Item>();

		public NetObjectList<Item> itemsLostLastDeath = new NetObjectList<Item>();

		public List<int> movementDirections = new List<int>();

		[XmlElement("farmName")]
		public readonly NetString farmName = new NetString("");

		[XmlElement("favoriteThing")]
		public readonly NetString favoriteThing = new NetString();

		[XmlElement("horseName")]
		public readonly NetString horseName = new NetString();

		public string slotName;

		public bool slotCanHost;

		[XmlIgnore]
		public bool hasReceivedToolUpgradeMessageYet;

		[XmlIgnore]
		private readonly NetArray<int, NetInt> appliedBuffs = new NetArray<int, NetInt>(12);

		[XmlIgnore]
		public IList<OutgoingMessage> messageQueue = new List<OutgoingMessage>();

		[XmlIgnore]
		public readonly NetLong uniqueMultiplayerID = new NetLong(Utility.RandomLong());

		[XmlElement("userID")]
		public readonly NetString userID = new NetString("");

		[XmlIgnore]
		public string previousLocationName = "";

		[XmlIgnore]
		public readonly NetBool hasMenuOpen = new NetBool(value: false);

		[XmlIgnore]
		public readonly Color DEFAULT_SHIRT_COLOR = Color.White;

		[XmlIgnore]
		public readonly Color DEFAULT_PANTS_COLOR = new Color(46, 85, 183);

		public string defaultChatColor;

		public bool catPerson = true;

		public int whichPetBreed;

		[XmlIgnore]
		public bool isAnimatingMount;

		[XmlElement("acceptedDailyQuest")]
		public readonly NetBool acceptedDailyQuest = new NetBool(value: false);

		[XmlIgnore]
		public Item mostRecentlyGrabbedItem;

		[XmlIgnore]
		public Item itemToEat;

		[XmlElement("farmerRenderer")]
		private readonly NetRef<FarmerRenderer> farmerRenderer = new NetRef<FarmerRenderer>();

		[XmlIgnore]
		public int toolPower;

		[XmlIgnore]
		public int toolHold;

		public Vector2 mostRecentBed;

		[XmlElement("emoteFavorites")]
		public readonly NetStringList emoteFavorites = new NetStringList();

		[XmlElement("performedEmotes")]
		public readonly NetStringDictionary<bool, NetBool> performedEmotes = new NetStringDictionary<bool, NetBool>();

		[XmlElement("shirt")]
		public readonly NetInt shirt = new NetInt(0);

		[XmlElement("hair")]
		public readonly NetInt hair = new NetInt(0);

		[XmlElement("skin")]
		public readonly NetInt skin = new NetInt(0);

		[XmlElement("shoes")]
		public readonly NetInt shoes = new NetInt(2);

		[XmlElement("accessory")]
		public readonly NetInt accessory = new NetInt(-1);

		[XmlElement("facialHair")]
		public readonly NetInt facialHair = new NetInt(-1);

		[XmlElement("pants")]
		public readonly NetInt pants = new NetInt(0);

		[XmlIgnore]
		public int currentEyes;

		[XmlIgnore]
		public int blinkTimer;

		[XmlIgnore]
		public readonly NetInt netFestivalScore = new NetInt();

		[XmlIgnore]
		public float temporarySpeedBuff;

		[XmlElement("hairstyleColor")]
		public readonly NetColor hairstyleColor = new NetColor(new Color(193, 90, 50));

		[XmlElement("pantsColor")]
		public readonly NetColor pantsColor = new NetColor(new Color(46, 85, 183));

		[XmlElement("newEyeColor")]
		public readonly NetColor newEyeColor = new NetColor(new Color(122, 68, 52));

		[XmlElement("hat")]
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		[XmlElement("boots")]
		public readonly NetRef<Boots> boots = new NetRef<Boots>();

		[XmlElement("leftRing")]
		public readonly NetRef<Ring> leftRing = new NetRef<Ring>();

		[XmlElement("rightRing")]
		public readonly NetRef<Ring> rightRing = new NetRef<Ring>();

		[XmlElement("shirtItem")]
		public readonly NetRef<Clothing> shirtItem = new NetRef<Clothing>();

		[XmlElement("pantsItem")]
		public readonly NetRef<Clothing> pantsItem = new NetRef<Clothing>();

		[XmlIgnore]
		public readonly NetDancePartner dancePartner = new NetDancePartner();

		[XmlIgnore]
		public bool ridingMineElevator;

		[XmlIgnore]
		public bool mineMovementDirectionWasUp;

		[XmlIgnore]
		public bool cameFromDungeon;

		[XmlIgnore]
		public readonly NetBool exhausted = new NetBool();

		[XmlElement("divorceTonight")]
		public readonly NetBool divorceTonight = new NetBool();

		[XmlElement("changeWalletTypeTonight")]
		public readonly NetBool changeWalletTypeTonight = new NetBool();

		[XmlIgnore]
		public AnimatedSprite.endOfAnimationBehavior toolOverrideFunction;

		private readonly NetInt netDeepestMineLevel = new NetInt();

		[XmlElement("currentToolIndex")]
		private readonly NetInt currentToolIndex = new NetInt(0);

		[XmlIgnore]
		private readonly NetRef<Item> temporaryItem = new NetRef<Item>();

		[XmlIgnore]
		private readonly NetRef<Item> cursorSlotItem = new NetRef<Item>();

		[XmlIgnore]
		public readonly NetBool netItemStowed = new NetBool(value: false);

		protected bool _itemStowed;

		public int woodPieces;

		public int stonePieces;

		public int copperPieces;

		public int ironPieces;

		public int coalPieces;

		public int goldPieces;

		public int iridiumPieces;

		public int quartzPieces;

		public string gameVersion = "-1";

		[XmlIgnore]
		public bool isFakeEventActor;

		[XmlElement("caveChoice")]
		public readonly NetInt caveChoice = new NetInt();

		public int feed;

		[XmlElement("farmingLevel")]
		public readonly NetInt farmingLevel = new NetInt();

		[XmlElement("miningLevel")]
		public readonly NetInt miningLevel = new NetInt();

		[XmlElement("combatLevel")]
		public readonly NetInt combatLevel = new NetInt();

		[XmlElement("foragingLevel")]
		public readonly NetInt foragingLevel = new NetInt();

		[XmlElement("fishingLevel")]
		public readonly NetInt fishingLevel = new NetInt();

		[XmlElement("luckLevel")]
		public readonly NetInt luckLevel = new NetInt();

		[XmlElement("newSkillPointsToSpend")]
		public readonly NetInt newSkillPointsToSpend = new NetInt();

		[XmlElement("addedFarmingLevel")]
		public readonly NetInt addedFarmingLevel = new NetInt();

		[XmlElement("addedMiningLevel")]
		public readonly NetInt addedMiningLevel = new NetInt();

		[XmlElement("addedCombatLevel")]
		public readonly NetInt addedCombatLevel = new NetInt();

		[XmlElement("addedForagingLevel")]
		public readonly NetInt addedForagingLevel = new NetInt();

		[XmlElement("addedFishingLevel")]
		public readonly NetInt addedFishingLevel = new NetInt();

		[XmlElement("addedLuckLevel")]
		public readonly NetInt addedLuckLevel = new NetInt();

		[XmlElement("maxStamina")]
		public readonly NetInt maxStamina = new NetInt(270);

		[XmlElement("maxItems")]
		public readonly NetInt maxItems = new NetInt(12);

		[XmlElement("lastSeenMovieWeek")]
		public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

		[XmlIgnore]
		public readonly NetString viewingLocation = new NetString(null);

		private readonly NetFloat netStamina = new NetFloat(270f);

		public int resilience;

		public int attack;

		public int immunity;

		public float attackIncreaseModifier;

		public float knockbackModifier;

		public float weaponSpeedModifier;

		public float critChanceModifier;

		public float critPowerModifier;

		public float weaponPrecisionModifier;

		[XmlIgnore]
		public NetRoot<FarmerTeam> teamRoot = new NetRoot<FarmerTeam>(new FarmerTeam());

		public int clubCoins;

		public int trashCanLevel;

		private NetLong netMillisecondsPlayed = new NetLong();

		[XmlElement("toolBeingUpgraded")]
		public readonly NetRef<Tool> toolBeingUpgraded = new NetRef<Tool>();

		[XmlElement("daysLeftForToolUpgrade")]
		public readonly NetInt daysLeftForToolUpgrade = new NetInt();

		[XmlIgnore]
		private float timeOfLastPositionPacket;

		private int numUpdatesSinceLastDraw;

		[XmlElement("houseUpgradeLevel")]
		public readonly NetInt houseUpgradeLevel = new NetInt(0);

		[XmlElement("daysUntilHouseUpgrade")]
		public readonly NetInt daysUntilHouseUpgrade = new NetInt(-1);

		public int coopUpgradeLevel;

		public int barnUpgradeLevel;

		public bool hasGreenhouse;

		public bool hasUnlockedSkullDoor;

		public bool hasDarkTalisman;

		public bool hasMagicInk;

		public bool showChestColorPicker = true;

		public bool hasMagnifyingGlass;

		public bool hasWateringCanEnchantment;

		[XmlElement("magneticRadius")]
		public readonly NetInt magneticRadius = new NetInt(128);

		public int temporaryInvincibilityTimer;

		[XmlIgnore]
		public float rotation;

		private int craftingTime = 1000;

		private int raftPuddleCounter = 250;

		private int raftBobCounter = 1000;

		public int health = 100;

		public int maxHealth = 100;

		private readonly NetInt netTimesReachedMineBottom = new NetInt(0);

		public float difficultyModifier = 1f;

		[XmlIgnore]
		public Vector2 jitter = Vector2.Zero;

		[XmlIgnore]
		public Vector2 lastPosition;

		[XmlIgnore]
		public Vector2 lastGrabTile = Vector2.Zero;

		[XmlIgnore]
		public float jitterStrength;

		[XmlIgnore]
		public float xOffset;

		[XmlElement("isMale")]
		public readonly NetBool isMale = new NetBool(value: true);

		[XmlIgnore]
		public bool canMove = true;

		[XmlIgnore]
		public bool running;

		[XmlIgnore]
		public bool ignoreCollisions;

		[XmlIgnore]
		public readonly NetBool usingTool = new NetBool(value: false);

		[XmlIgnore]
		public bool isEating;

		[XmlIgnore]
		public readonly NetBool isInBed = new NetBool(value: false);

		[XmlIgnore]
		public bool forceTimePass;

		[XmlIgnore]
		public bool isRafting;

		[XmlIgnore]
		public bool usingSlingshot;

		[XmlIgnore]
		public readonly NetBool bathingClothes = new NetBool(value: false);

		[XmlIgnore]
		public bool canOnlyWalk;

		[XmlIgnore]
		public bool temporarilyInvincible;

		public bool hasBusTicket;

		public bool stardewHero;

		public bool hasClubCard;

		public bool hasSpecialCharm;

		[XmlIgnore]
		public bool canReleaseTool;

		[XmlIgnore]
		public bool isCrafting;

		[XmlIgnore]
		public bool isEmoteAnimating;

		[XmlIgnore]
		public bool passedOut;

		[XmlIgnore]
		protected int _emoteGracePeriod;

		[XmlIgnore]
		private BoundingBoxGroup temporaryPassableTiles = new BoundingBoxGroup();

		[XmlIgnore]
		public readonly NetBool hidden = new NetBool();

		[XmlElement("basicShipped")]
		public readonly NetIntDictionary<int, NetInt> basicShipped = new NetIntDictionary<int, NetInt>();

		[XmlElement("mineralsFound")]
		public readonly NetIntDictionary<int, NetInt> mineralsFound = new NetIntDictionary<int, NetInt>();

		[XmlElement("recipesCooked")]
		public readonly NetIntDictionary<int, NetInt> recipesCooked = new NetIntDictionary<int, NetInt>();

		[XmlElement("fishCaught")]
		public readonly NetIntIntArrayDictionary fishCaught = new NetIntIntArrayDictionary();

		[XmlElement("archaeologyFound")]
		public readonly NetIntIntArrayDictionary archaeologyFound = new NetIntIntArrayDictionary();

		public SerializableDictionary<string, SerializableDictionary<int, int>> giftedItems;

		[XmlElement("tailoredItems")]
		public readonly NetStringDictionary<int, NetInt> tailoredItems = new NetStringDictionary<int, NetInt>();

		public SerializableDictionary<string, int[]> friendships;

		[XmlElement("friendshipData")]
		public readonly NetStringDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetStringDictionary<Friendship, NetRef<Friendship>>();

		[XmlIgnore]
		public Vector2 positionBeforeEvent;

		[XmlIgnore]
		public int orientationBeforeEvent;

		[XmlIgnore]
		public int swimTimer;

		[XmlIgnore]
		public int regenTimer;

		[XmlIgnore]
		public int timerSinceLastMovement;

		[XmlIgnore]
		public int noMovementPause;

		[XmlIgnore]
		public int freezePause;

		[XmlIgnore]
		public float yOffset;

		public BuildingUpgrade currentUpgrade;

		[XmlElement("spouse")]
		protected readonly NetString netSpouse = new NetString();

		public string dateStringForSaveGame;

		public int? dayOfMonthForSaveGame;

		public int? seasonForSaveGame;

		public int? yearForSaveGame;

		public int overallsColor;

		public int shirtColor;

		public int skinColor;

		public int hairColor;

		public int eyeColor;

		[XmlIgnore]
		public Vector2 armOffset;

		public string bobber = "";

		private readonly NetRef<Horse> netMount = new NetRef<Horse>();

		[XmlIgnore]
		protected Item _lastSelectedItem;

		private readonly NetEvent0 fireToolEvent = new NetEvent0(interpolate: true);

		private readonly NetEvent0 beginUsingToolEvent = new NetEvent0(interpolate: true);

		private readonly NetEvent0 endUsingToolEvent = new NetEvent0(interpolate: true);

		private readonly NetEvent0 sickAnimationEvent = new NetEvent0();

		private readonly NetEvent0 passOutEvent = new NetEvent0();

		private readonly NetEvent0 haltAnimationEvent = new NetEvent0();

		private readonly NetEvent1Field<Object, NetRef<Object>> drinkAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

		private readonly NetEvent1Field<Object, NetRef<Object>> eatAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

		private readonly NetEvent1Field<string, NetString> doEmoteEvent = new NetEvent1Field<string, NetString>();

		private readonly NetEvent1Field<long, NetLong> kissFarmerEvent = new NetEvent1Field<long, NetLong>();

		private readonly NetEvent1Field<float, NetFloat> synchronizedJumpEvent = new NetEvent1Field<float, NetFloat>();

		[XmlElement("chestConsumedLevels")]
		public readonly NetIntDictionary<bool, NetBool> chestConsumedMineLevels = new NetIntDictionary<bool, NetBool>();

		public int saveTime;

		[XmlIgnore]
		public float drawLayerDisambiguator;

		[XmlElement("isCustomized")]
		public readonly NetBool isCustomized = new NetBool(value: false);

		[XmlElement("homeLocation")]
		public readonly NetString homeLocation = new NetString("FarmHouse");

		public static readonly EmoteType[] EMOTES = new EmoteType[22]
		{
			new EmoteType("happy", "Emote_Happy", 32),
			new EmoteType("sad", "Emote_Sad", 28),
			new EmoteType("heart", "Emote_Heart", 20),
			new EmoteType("exclamation", "Emote_Exclamation", 16),
			new EmoteType("note", "Emote_Note", 56),
			new EmoteType("sleep", "Emote_Sleep", 24),
			new EmoteType("game", "Emote_Game", 52),
			new EmoteType("question", "Emote_Question", 8),
			new EmoteType("x", "Emote_X", 36),
			new EmoteType("pause", "Emote_Pause", 40),
			new EmoteType("blush", "Emote_Blush", 60, null, 2, is_hidden: true),
			new EmoteType("angry", "Emote_Angry", 12),
			new EmoteType("yes", "Emote_Yes", 56, new FarmerSprite.AnimationFrame[7]
			{
				new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("jingle1");
				}),
				new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false)
			}),
			new EmoteType("no", "Emote_No", 36, new FarmerSprite.AnimationFrame[5]
			{
				new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("cancel");
				}),
				new FarmerSprite.AnimationFrame(27, 250, secondaryArm: true, flip: false),
				new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(27, 250, secondaryArm: true, flip: false),
				new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false)
			}),
			new EmoteType("sick", "Emote_Sick", 12, new FarmerSprite.AnimationFrame[8]
			{
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("croak");
				}),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false)
			}),
			new EmoteType("laugh", "Emote_Laugh", 56, new FarmerSprite.AnimationFrame[8]
			{
				new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("dustMeep");
				}),
				new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("dustMeep");
				}),
				new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("dustMeep");
				}),
				new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("dustMeep");
				}),
				new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false)
			}),
			new EmoteType("surprised", "Emote_Surprised", 16, new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(94, 1500, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("batScreech");
					who.jumpWithoutSound(4f);
					who.jitterStrength = 1f;
				})
			}),
			new EmoteType("hi", "Emote_Hi", 56, new FarmerSprite.AnimationFrame[4]
			{
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("give_gift");
				}),
				new FarmerSprite.AnimationFrame(85, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(85, 250, secondaryArm: false, flip: false)
			}),
			new EmoteType("taunt", "Emote_Taunt", 12, new FarmerSprite.AnimationFrame[10]
			{
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("hitEnemy");
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("hitEnemy");
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("hitEnemy");
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(3, 500, secondaryArm: false, flip: false)
			}, 2, is_hidden: true),
			new EmoteType("uh", "Emote_Uh", 40, new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(10, 1500, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("clam_tone");
				})
			}),
			new EmoteType("music", "Emote_Music", 56, new FarmerSprite.AnimationFrame[9]
			{
				new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.playHarpEmoteSound();
				}),
				new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false)
			}, 2, is_hidden: true),
			new EmoteType("jar", "Emote_Jar", -1, new FarmerSprite.AnimationFrame[6]
			{
				new FarmerSprite.AnimationFrame(111, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(111, 300, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("fishingRodBend");
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(111, 500, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(111, 300, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("fishingRodBend");
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(111, 500, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(112, 1000, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.currentLocation.localSound("coin");
					who.jumpWithoutSound(4f);
				})
			}, 1, is_hidden: true)
		};

		public int daysMarried;

		private int toolPitchAccumulator;

		private int charactercollisionTimer;

		private NPC collisionNPC;

		public float movementMultiplier = 0.01f;

		public int visibleQuestCount
		{
			get
			{
				int count = 0;
				foreach (Quest item in questLog)
				{
					if (!item.isSecretQuest())
					{
						count++;
					}
				}
				return count;
			}
		}

		public Item recoveredItem
		{
			get
			{
				return _recoveredItem.Value;
			}
			set
			{
				_recoveredItem.Value = value;
			}
		}

		[XmlElement("theaterBuildDate")]
		public long theaterBuildDate
		{
			get
			{
				return teamRoot.Value.theaterBuildDate;
			}
			set
			{
				teamRoot.Value.theaterBuildDate.Value = value;
			}
		}

		[XmlIgnore]
		public int festivalScore
		{
			get
			{
				return netFestivalScore;
			}
			set
			{
				if (Game1.player != null && Game1.player.team != null && Game1.player.team.festivalScoreStatus != null)
				{
					Game1.player.team.festivalScoreStatus.UpdateState(string.Concat(Game1.player.festivalScore));
				}
				netFestivalScore.Value = value;
			}
		}

		public int deepestMineLevel
		{
			get
			{
				return netDeepestMineLevel;
			}
			set
			{
				netDeepestMineLevel.Value = value;
			}
		}

		public float stamina
		{
			get
			{
				return netStamina;
			}
			set
			{
				netStamina.Value = value;
			}
		}

		[XmlIgnore]
		public FarmerTeam team
		{
			get
			{
				if (Game1.player != null && this != Game1.player)
				{
					return Game1.player.team;
				}
				return teamRoot.Value;
			}
		}

		public uint totalMoneyEarned
		{
			get
			{
				return (uint)teamRoot.Value.totalMoneyEarned.Value;
			}
			set
			{
				if (teamRoot.Value.totalMoneyEarned.Value != 0)
				{
					if (value >= 15000 && teamRoot.Value.totalMoneyEarned.Value < 15000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned15k", farmName);
					}
					if (value >= 50000 && teamRoot.Value.totalMoneyEarned.Value < 50000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned50k", farmName);
					}
					if (value >= 250000 && teamRoot.Value.totalMoneyEarned.Value < 250000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned250k", farmName);
					}
					if (value >= 1000000 && teamRoot.Value.totalMoneyEarned.Value < 1000000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned1m", farmName);
					}
					if (value >= 10000000 && teamRoot.Value.totalMoneyEarned.Value < 10000000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned10m", farmName);
					}
					if (value >= 100000000 && teamRoot.Value.totalMoneyEarned.Value < 100000000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned100m", farmName);
					}
				}
				teamRoot.Value.totalMoneyEarned.Value = (int)value;
			}
		}

		public ulong millisecondsPlayed
		{
			get
			{
				return (ulong)netMillisecondsPlayed.Value;
			}
			set
			{
				netMillisecondsPlayed.Value = (long)value;
			}
		}

		public bool hasRustyKey
		{
			get
			{
				return teamRoot.Value.hasRustyKey;
			}
			set
			{
				teamRoot.Value.hasRustyKey.Value = value;
			}
		}

		public bool hasSkullKey
		{
			get
			{
				return teamRoot.Value.hasSkullKey;
			}
			set
			{
				teamRoot.Value.hasSkullKey.Value = value;
			}
		}

		public bool canUnderstandDwarves
		{
			get
			{
				return teamRoot.Value.canUnderstandDwarves;
			}
			set
			{
				teamRoot.Value.canUnderstandDwarves.Value = value;
			}
		}

		[XmlElement("useSeparateWallets")]
		public bool useSeparateWallets
		{
			get
			{
				return teamRoot.Value.useSeparateWallets;
			}
			set
			{
				teamRoot.Value.useSeparateWallets.Value = value;
			}
		}

		public int timesReachedMineBottom
		{
			get
			{
				return netTimesReachedMineBottom;
			}
			set
			{
				netTimesReachedMineBottom.Value = value;
			}
		}

		public string spouse
		{
			get
			{
				if (netSpouse.Value != null && netSpouse.Value.Length != 0)
				{
					return netSpouse.Value;
				}
				return null;
			}
			set
			{
				if (value == null)
				{
					netSpouse.Value = "";
				}
				else
				{
					netSpouse.Value = value;
				}
			}
		}

		[XmlIgnore]
		public bool isUnclaimedFarmhand
		{
			get
			{
				if (!IsMainPlayer)
				{
					return !isCustomized;
				}
				return false;
			}
		}

		[XmlIgnore]
		public Horse mount
		{
			get
			{
				return netMount.Value;
			}
			set
			{
				setMount(value);
			}
		}

		[XmlIgnore]
		public int MaxItems
		{
			get
			{
				return maxItems;
			}
			set
			{
				maxItems.Value = value;
			}
		}

		[XmlIgnore]
		public int Level => ((int)farmingLevel + (int)fishingLevel + (int)foragingLevel + (int)combatLevel + (int)miningLevel + (int)luckLevel) / 2;

		[XmlIgnore]
		public int CraftingTime
		{
			get
			{
				return craftingTime;
			}
			set
			{
				craftingTime = value;
			}
		}

		[XmlIgnore]
		public int NewSkillPointsToSpend
		{
			get
			{
				return newSkillPointsToSpend;
			}
			set
			{
				newSkillPointsToSpend.Value = value;
			}
		}

		[XmlIgnore]
		public int FarmingLevel
		{
			get
			{
				return (int)farmingLevel + (int)addedFarmingLevel;
			}
			set
			{
				farmingLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int MiningLevel
		{
			get
			{
				return (int)miningLevel + (int)addedMiningLevel;
			}
			set
			{
				miningLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int CombatLevel
		{
			get
			{
				return (int)combatLevel + (int)addedCombatLevel;
			}
			set
			{
				combatLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int ForagingLevel
		{
			get
			{
				return (int)foragingLevel + (int)addedForagingLevel;
			}
			set
			{
				foragingLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int FishingLevel
		{
			get
			{
				return (int)fishingLevel + (int)addedFishingLevel;
			}
			set
			{
				fishingLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int LuckLevel
		{
			get
			{
				return (int)luckLevel + (int)addedLuckLevel;
			}
			set
			{
				luckLevel.Value = value;
			}
		}

		[XmlIgnore]
		public double DailyLuck => team.sharedDailyLuck.Value + (double)(hasSpecialCharm ? 0.025f : 0f);

		[XmlIgnore]
		public int HouseUpgradeLevel
		{
			get
			{
				return houseUpgradeLevel;
			}
			set
			{
				houseUpgradeLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int CoopUpgradeLevel
		{
			get
			{
				return coopUpgradeLevel;
			}
			set
			{
				coopUpgradeLevel = value;
			}
		}

		[XmlIgnore]
		public int BarnUpgradeLevel
		{
			get
			{
				return barnUpgradeLevel;
			}
			set
			{
				barnUpgradeLevel = value;
			}
		}

		[XmlIgnore]
		public BoundingBoxGroup TemporaryPassableTiles
		{
			get
			{
				return temporaryPassableTiles;
			}
			set
			{
				temporaryPassableTiles = value;
			}
		}

		[XmlIgnore]
		public IList<Item> Items
		{
			get
			{
				return items;
			}
			set
			{
				items.CopyFrom(value);
			}
		}

		[XmlIgnore]
		public int MagneticRadius
		{
			get
			{
				return magneticRadius;
			}
			set
			{
				magneticRadius.Value = value;
			}
		}

		[XmlIgnore]
		public Object ActiveObject
		{
			get
			{
				if (TemporaryItem != null)
				{
					if (TemporaryItem is Object)
					{
						return (Object)TemporaryItem;
					}
					return null;
				}
				if (_itemStowed)
				{
					return null;
				}
				if ((int)currentToolIndex < items.Count && items[currentToolIndex] != null && items[currentToolIndex] is Object)
				{
					return (Object)items[currentToolIndex];
				}
				return null;
			}
			set
			{
				netItemStowed.Set(newValue: false);
				if (value == null)
				{
					removeItemFromInventory(ActiveObject);
				}
				else
				{
					addItemToInventory(value, CurrentToolIndex);
				}
			}
		}

		[XmlIgnore]
		public bool IsMale
		{
			get
			{
				return isMale;
			}
			set
			{
				isMale.Set(value);
			}
		}

		[XmlIgnore]
		public IList<int> DialogueQuestionsAnswered => dialogueQuestionsAnswered;

		[XmlIgnore]
		public int WoodPieces
		{
			get
			{
				return woodPieces;
			}
			set
			{
				woodPieces = value;
			}
		}

		[XmlIgnore]
		public int StonePieces
		{
			get
			{
				return stonePieces;
			}
			set
			{
				stonePieces = value;
			}
		}

		[XmlIgnore]
		public int CopperPieces
		{
			get
			{
				return copperPieces;
			}
			set
			{
				copperPieces = value;
			}
		}

		[XmlIgnore]
		public int IronPieces
		{
			get
			{
				return ironPieces;
			}
			set
			{
				ironPieces = value;
			}
		}

		[XmlIgnore]
		public int CoalPieces
		{
			get
			{
				return coalPieces;
			}
			set
			{
				coalPieces = value;
			}
		}

		[XmlIgnore]
		public int GoldPieces
		{
			get
			{
				return goldPieces;
			}
			set
			{
				goldPieces = value;
			}
		}

		[XmlIgnore]
		public int IridiumPieces
		{
			get
			{
				return iridiumPieces;
			}
			set
			{
				iridiumPieces = value;
			}
		}

		[XmlIgnore]
		public int QuartzPieces
		{
			get
			{
				return quartzPieces;
			}
			set
			{
				quartzPieces = value;
			}
		}

		[XmlIgnore]
		public int Feed
		{
			get
			{
				return feed;
			}
			set
			{
				feed = value;
			}
		}

		[XmlIgnore]
		public bool CanMove
		{
			get
			{
				return canMove;
			}
			set
			{
				canMove = value;
			}
		}

		[XmlIgnore]
		public bool UsingTool
		{
			get
			{
				return usingTool;
			}
			set
			{
				usingTool.Set(value);
			}
		}

		[XmlIgnore]
		public Tool CurrentTool
		{
			get
			{
				if (CurrentItem != null && CurrentItem is Tool)
				{
					return (Tool)CurrentItem;
				}
				return null;
			}
			set
			{
				while (CurrentToolIndex >= items.Count)
				{
					items.Add(null);
				}
				items[CurrentToolIndex] = value;
			}
		}

		[XmlIgnore]
		public Item TemporaryItem
		{
			get
			{
				return temporaryItem.Value;
			}
			set
			{
				temporaryItem.Value = value;
			}
		}

		public Item CursorSlotItem
		{
			get
			{
				return cursorSlotItem.Value;
			}
			set
			{
				cursorSlotItem.Value = value;
			}
		}

		[XmlIgnore]
		public Item CurrentItem
		{
			get
			{
				if (TemporaryItem != null)
				{
					return TemporaryItem;
				}
				if (_itemStowed)
				{
					return null;
				}
				if ((int)currentToolIndex >= items.Count)
				{
					return null;
				}
				return items[currentToolIndex];
			}
		}

		[XmlIgnore]
		public int CurrentToolIndex
		{
			get
			{
				return currentToolIndex;
			}
			set
			{
				netItemStowed.Set(newValue: false);
				if ((int)currentToolIndex >= 0 && CurrentItem != null && value != (int)currentToolIndex)
				{
					CurrentItem.actionWhenStopBeingHeld(this);
				}
				currentToolIndex.Set(value);
			}
		}

		[XmlIgnore]
		public float Stamina
		{
			get
			{
				return stamina;
			}
			set
			{
				stamina = Math.Min((int)maxStamina, Math.Max(value, -16f));
			}
		}

		[XmlIgnore]
		public int MaxStamina
		{
			get
			{
				return maxStamina;
			}
			set
			{
				maxStamina.Value = value;
			}
		}

		public long UniqueMultiplayerID
		{
			get
			{
				return uniqueMultiplayerID;
			}
			set
			{
				uniqueMultiplayerID.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsLocalPlayer
		{
			get
			{
				if (UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
				{
					if (Game1.CurrentEvent != null)
					{
						return Game1.CurrentEvent.farmer == this;
					}
					return false;
				}
				return true;
			}
		}

		[XmlIgnore]
		public bool IsMainPlayer
		{
			get
			{
				if (!(Game1.serverHost == null) || !IsLocalPlayer)
				{
					if (Game1.serverHost != null)
					{
						return UniqueMultiplayerID == Game1.serverHost.Value.UniqueMultiplayerID;
					}
					return false;
				}
				return true;
			}
		}

		[XmlIgnore]
		public override AnimatedSprite Sprite
		{
			get
			{
				return base.Sprite;
			}
			set
			{
				base.Sprite = value;
				if (base.Sprite != null)
				{
					(base.Sprite as FarmerSprite).setOwner(this);
				}
			}
		}

		[XmlIgnore]
		public FarmerSprite FarmerSprite
		{
			get
			{
				return (FarmerSprite)Sprite;
			}
			set
			{
				Sprite = value;
			}
		}

		[XmlIgnore]
		public FarmerRenderer FarmerRenderer
		{
			get
			{
				return farmerRenderer;
			}
			set
			{
				farmerRenderer.Set(value);
			}
		}

		[XmlElement("money")]
		public int _money
		{
			get
			{
				return teamRoot.Value.GetMoney(this);
			}
			set
			{
				teamRoot.Value.GetMoney(this).Value = value;
			}
		}

		[XmlIgnore]
		public int Money
		{
			get
			{
				return _money;
			}
			set
			{
				if (Game1.player != this)
				{
					throw new Exception("Cannot change another farmer's money. Use Game1.player.team.SetIndividualMoney");
				}
				int previousMoney = _money;
				_money = value;
				if (value > previousMoney)
				{
					uint earned = (uint)(value - previousMoney);
					totalMoneyEarned += earned;
					if (Game1.player.useSeparateWallets)
					{
						stats.IndividualMoneyEarned += earned;
					}
					Game1.stats.checkForMoneyAchievements();
				}
			}
		}

		public void addUnearnedMoney(int money)
		{
			_money += money;
		}

		public NetStringList GetEmoteFavorites()
		{
			if (emoteFavorites.Count == 0)
			{
				emoteFavorites.Add("question");
				emoteFavorites.Add("heart");
				emoteFavorites.Add("yes");
				emoteFavorites.Add("happy");
				emoteFavorites.Add("pause");
				emoteFavorites.Add("sad");
				emoteFavorites.Add("no");
				emoteFavorites.Add("angry");
			}
			return emoteFavorites;
		}

		public Farmer()
		{
			farmerInit();
			Sprite = new FarmerSprite(null);
		}

		public Farmer(FarmerSprite sprite, Vector2 position, int speed, string name, List<Item> initialTools, bool isMale)
			: base(sprite, position, speed, name)
		{
			farmerInit();
			base.Name = name;
			base.displayName = name;
			IsMale = isMale;
			stamina = (int)maxStamina;
			items.CopyFrom(initialTools);
			for (int i = items.Count; i < (int)maxItems; i++)
			{
				items.Add(null);
			}
			activeDialogueEvents.Add("Introduction", 6);
			if (base.currentLocation != null)
			{
				mostRecentBed = Utility.PointToVector2((base.currentLocation as FarmHouse).getBedSpot()) * 64f;
			}
			else
			{
				mostRecentBed = new Vector2(9f, 9f) * 64f;
			}
		}

		private void farmerInit()
		{
			base.NetFields.AddFields(uniqueMultiplayerID, userID, farmerRenderer, isMale, bathingClothes, shirt, pants, hair, skin, shoes, accessory, facialHair, hairstyleColor, pantsColor, newEyeColor, items, currentToolIndex, temporaryItem, cursorSlotItem, fireToolEvent, beginUsingToolEvent, endUsingToolEvent, hat, boots, leftRing, rightRing, hidden, isInBed, caveChoice, houseUpgradeLevel, daysUntilHouseUpgrade, magneticRadius, netSpouse, mailReceived, mailForTomorrow, mailbox, eventsSeen, secretNotesSeen, netMount.NetFields, dancePartner.NetFields, divorceTonight, isCustomized, homeLocation, farmName, favoriteThing, horseName, netMillisecondsPlayed, netFestivalScore, friendshipData, drinkAnimationEvent, eatAnimationEvent, sickAnimationEvent, passOutEvent, doEmoteEvent, questLog, professions, newLevels, experiencePoints, dialogueQuestionsAnswered, cookingRecipes, craftingRecipes, activeDialogueEvents, achievements, specialItems, specialBigCraftables, farmingLevel, miningLevel, combatLevel, foragingLevel, fishingLevel, luckLevel, newSkillPointsToSpend, addedFarmingLevel, addedMiningLevel, addedCombatLevel, addedForagingLevel, addedFishingLevel, addedLuckLevel, maxStamina, netStamina, maxItems, chestConsumedMineLevels, toolBeingUpgraded, daysLeftForToolUpgrade, exhausted, appliedBuffs, netDeepestMineLevel, netTimesReachedMineBottom, netItemStowed, acceptedDailyQuest, lastSeenMovieWeek, shirtItem, pantsItem, personalShippingBin, viewingLocation, kissFarmerEvent, haltAnimationEvent, synchronizedJumpEvent, tailoredItems, basicShipped, mineralsFound, recipesCooked, archaeologyFound, fishCaught, _recoveredItem, itemsLostLastDeath);
			if (Sprite != null)
			{
				FarmerSprite.setOwner(this);
			}
			netMillisecondsPlayed.DeltaAggregateTicks = 60;
			fireToolEvent.onEvent += performFireTool;
			beginUsingToolEvent.onEvent += performBeginUsingTool;
			endUsingToolEvent.onEvent += performEndUsingTool;
			drinkAnimationEvent.onEvent += performDrinkAnimation;
			eatAnimationEvent.onEvent += performEatAnimation;
			sickAnimationEvent.onEvent += performSickAnimation;
			passOutEvent.onEvent += performPassOut;
			doEmoteEvent.onEvent += performPlayerEmote;
			kissFarmerEvent.onEvent += performKissFarmer;
			haltAnimationEvent.onEvent += performHaltAnimation;
			synchronizedJumpEvent.onEvent += performSynchronizedJump;
			FarmerRenderer = new FarmerRenderer("Characters\\Farmer\\farmer_" + (IsMale ? "" : "girl_") + "base", this);
			base.currentLocation = Game1.getLocationFromName(homeLocation);
			items.Clear();
			giftedItems = new SerializableDictionary<string, SerializableDictionary<int, int>>();
			craftingRecipes.Add("Chest", 0);
			craftingRecipes.Add("Wood Fence", 0);
			craftingRecipes.Add("Gate", 0);
			craftingRecipes.Add("Torch", 0);
			craftingRecipes.Add("Campfire", 0);
			craftingRecipes.Add("Wood Path", 0);
			craftingRecipes.Add("Cobblestone Path", 0);
			craftingRecipes.Add("Gravel Path", 0);
			craftingRecipes.Add("Wood Sign", 0);
			craftingRecipes.Add("Stone Sign", 0);
			cookingRecipes.Add("Fried Egg", 0);
			songsHeard.Add("title_day");
			songsHeard.Add("title_night");
			changeShirt(0);
			changeSkinColor(0);
			changeShoeColor(2);
			shirtItem.fieldChangeVisibleEvent += delegate
			{
				UpdateClothing();
			};
			pantsItem.fieldChangeVisibleEvent += delegate
			{
				UpdateClothing();
			};
		}

		public bool CanEmote()
		{
			if (Game1.farmEvent != null)
			{
				return false;
			}
			if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence && IsLocalPlayer)
			{
				return false;
			}
			if (usingSlingshot)
			{
				return false;
			}
			if (isEating)
			{
				return false;
			}
			if (UsingTool)
			{
				return false;
			}
			if (!CanMove)
			{
				return false;
			}
			if (isRidingHorse())
			{
				return false;
			}
			if (bathingClothes.Value)
			{
				return false;
			}
			return true;
		}

		public void performPlayerEmote(string emote_string)
		{
			for (int i = 0; i < EMOTES.Length; i++)
			{
				EmoteType emote_type = EMOTES[i];
				if (!(emote_type.emoteString == emote_string))
				{
					continue;
				}
				performedEmotes[emote_string] = true;
				if (emote_type.animationFrames != null)
				{
					if (!CanEmote())
					{
						break;
					}
					if (isEmoteAnimating)
					{
						EndEmoteAnimation();
					}
					else if (FarmerSprite.PauseForSingleAnimation)
					{
						break;
					}
					isEmoteAnimating = true;
					_emoteGracePeriod = 200;
					if (this == Game1.player)
					{
						Game1.player.noMovementPause = Math.Max(Game1.player.noMovementPause, 200);
					}
					base.FacingDirection = emote_type.facingDirection;
					FarmerSprite.animateOnce(emote_type.animationFrames, OnEmoteAnimationEnd);
				}
				if (emote_type.emoteIconIndex >= 0)
				{
					isEmoting = false;
					doEmote(emote_type.emoteIconIndex, nextEventCommand: false);
				}
			}
		}

		public static List<Item> initialTools()
		{
			return new List<Item>
			{
				new Axe(),
				new Hoe(),
				new WateringCan(),
				new Pickaxe(),
				new MeleeWeapon(47)
			};
		}

		private void playHarpEmoteSound()
		{
			int[] notes2 = new int[4]
			{
				1200,
				1600,
				1900,
				2400
			};
			switch (Game1.random.Next(5))
			{
			case 0:
				notes2 = new int[4]
				{
					1200,
					1600,
					1900,
					2400
				};
				break;
			case 1:
				notes2 = new int[4]
				{
					1200,
					1700,
					2100,
					2400
				};
				break;
			case 2:
				notes2 = new int[4]
				{
					1100,
					1400,
					1900,
					2300
				};
				break;
			case 3:
				notes2 = new int[3]
				{
					1600,
					1900,
					2400
				};
				break;
			case 4:
				notes2 = new int[3]
				{
					700,
					1200,
					1900
				};
				break;
			}
			if (!IsLocalPlayer)
			{
				return;
			}
			if (Game1.IsMultiplayer && (long)uniqueMultiplayerID % 111 == 0L)
			{
				notes2 = new int[4]
				{
					800 + Game1.random.Next(4) * 100,
					1200 + Game1.random.Next(4) * 100,
					1600 + Game1.random.Next(4) * 100,
					2000 + Game1.random.Next(4) * 100
				};
				for (int j = 0; j < notes2.Count(); j++)
				{
					DelayedAction.playSoundAfterDelay("miniharp_note", Game1.random.Next(60, 150) * j, base.currentLocation, notes2[j]);
					if (j > 1 && Game1.random.NextDouble() < 0.25)
					{
						break;
					}
				}
			}
			else
			{
				for (int i = 0; i < notes2.Count(); i++)
				{
					DelayedAction.playSoundAfterDelay("miniharp_note", (i > 0) ? (150 + Game1.random.Next(35, 51) * i) : 0, base.currentLocation, notes2[i]);
				}
			}
		}

		private static void removeLowestUpgradeLevelTool(List<Item> items, Type toolType)
		{
			Tool lowestItem = null;
			foreach (Item item in items)
			{
				if (item is Tool && item.GetType() == toolType && (lowestItem == null || (int)(item as Tool).upgradeLevel < (int)lowestItem.upgradeLevel))
				{
					lowestItem = (item as Tool);
				}
			}
			if (lowestItem != null)
			{
				items.Remove(lowestItem);
			}
		}

		public static void removeInitialTools(List<Item> items)
		{
			removeLowestUpgradeLevelTool(items, typeof(Axe));
			removeLowestUpgradeLevelTool(items, typeof(Hoe));
			removeLowestUpgradeLevelTool(items, typeof(WateringCan));
			removeLowestUpgradeLevelTool(items, typeof(Pickaxe));
			Item scythe = items.FirstOrDefault((Item item) => item is MeleeWeapon && (item as Tool).InitialParentTileIndex == 47);
			if (scythe != null)
			{
				items.Remove(scythe);
			}
		}

		public Point getMailboxPosition()
		{
			foreach (Building b in Game1.getFarm().buildings)
			{
				if (b.isCabin && b.nameOfIndoors == (string)homeLocation)
				{
					return b.getMailboxPosition();
				}
			}
			return new Point(68, 16);
		}

		public void ClearBuffs()
		{
			Game1.buffsDisplay.clearAllBuffs();
			stopGlowing();
			addedCombatLevel.Value = 0;
			addedFarmingLevel.Value = 0;
			addedFishingLevel.Value = 0;
			addedForagingLevel.Value = 0;
			addedLuckLevel.Value = 0;
			addedMiningLevel.Value = 0;
			base.addedSpeed = 0;
			attack = 0;
		}

		public void addBuffAttributes(int[] buffAttributes)
		{
			for (int i = 0; i < appliedBuffs.Length; i++)
			{
				appliedBuffs[i] += buffAttributes[i];
			}
			addedFarmingLevel.Value += buffAttributes[0];
			addedFishingLevel.Value += buffAttributes[1];
			addedMiningLevel.Value += buffAttributes[2];
			addedLuckLevel.Value += buffAttributes[4];
			addedForagingLevel.Value += buffAttributes[5];
			CraftingTime -= buffAttributes[6];
			MaxStamina += buffAttributes[7];
			MagneticRadius += buffAttributes[8];
			resilience += buffAttributes[10];
			attack += buffAttributes[11];
			base.addedSpeed += buffAttributes[9];
		}

		public void removeBuffAttributes(int[] buffAttributes)
		{
			for (int i = 0; i < appliedBuffs.Length; i++)
			{
				appliedBuffs[i] -= buffAttributes[i];
			}
			if (buffAttributes[0] != 0)
			{
				addedFarmingLevel.Value = Math.Max(0, (int)addedFarmingLevel - buffAttributes[0]);
			}
			if (buffAttributes[1] != 0)
			{
				addedFishingLevel.Value = Math.Max(0, (int)addedFishingLevel - buffAttributes[1]);
			}
			if (buffAttributes[2] != 0)
			{
				addedMiningLevel.Value = Math.Max(0, (int)addedMiningLevel - buffAttributes[2]);
			}
			if (buffAttributes[4] != 0)
			{
				addedLuckLevel.Value = Math.Max(0, (int)addedLuckLevel - buffAttributes[4]);
			}
			if (buffAttributes[5] != 0)
			{
				addedForagingLevel.Value = Math.Max(0, (int)addedForagingLevel - buffAttributes[5]);
			}
			if (buffAttributes[6] != 0)
			{
				CraftingTime = Math.Max(0, CraftingTime - buffAttributes[6]);
			}
			if (buffAttributes[7] != 0)
			{
				MaxStamina = Math.Max(0, MaxStamina - buffAttributes[7]);
				stamina = Math.Min(stamina, MaxStamina);
			}
			if (buffAttributes[8] != 0)
			{
				MagneticRadius = Math.Max(0, MagneticRadius - buffAttributes[8]);
			}
			if (buffAttributes[10] != 0)
			{
				resilience = Math.Max(0, resilience - buffAttributes[10]);
			}
			if (buffAttributes[9] != 0)
			{
				if (buffAttributes[9] < 0)
				{
					base.addedSpeed += Math.Abs(buffAttributes[9]);
				}
				else
				{
					base.addedSpeed -= buffAttributes[9];
				}
			}
			if (buffAttributes[11] != 0)
			{
				if (buffAttributes[11] < 0)
				{
					attack += Math.Abs(buffAttributes[11]);
				}
				else
				{
					attack -= buffAttributes[11];
				}
			}
		}

		public void removeBuffAttributes()
		{
			removeBuffAttributes(appliedBuffs.ToArray());
		}

		public bool isActive()
		{
			if (this != Game1.player)
			{
				return Game1.otherFarmers.ContainsKey(UniqueMultiplayerID);
			}
			return true;
		}

		public string getTexture()
		{
			return "Characters\\Farmer\\farmer_" + (IsMale ? "" : "girl_") + "base" + (isBald() ? "_bald" : "");
		}

		public void checkForLevelTenStatus()
		{
		}

		public void unload()
		{
			if (FarmerRenderer != null)
			{
				FarmerRenderer.unload();
			}
		}

		public void setInventory(List<Item> newInventory)
		{
			items.CopyFrom(newInventory);
			for (int i = items.Count; i < (int)maxItems; i++)
			{
				items.Add(null);
			}
		}

		public void makeThisTheActiveObject(Object o)
		{
			if (freeSpotsInInventory() > 0)
			{
				Item i = CurrentItem;
				ActiveObject = o;
				addItemToInventory(i);
			}
		}

		public int getNumberOfChildren()
		{
			return getChildrenCount();
		}

		private void setMount(Horse mount)
		{
			if (mount != null)
			{
				netMount.Value = mount;
				xOffset = -11f;
				base.Position = Utility.PointToVector2(mount.GetBoundingBox().Location);
				position.Y -= 16f;
				position.X -= 8f;
				base.speed = 2;
				showNotCarrying();
				return;
			}
			netMount.Value = null;
			collisionNPC = null;
			running = false;
			base.speed = ((Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton) && !Game1.options.autoRun) ? 5 : 2);
			bool isRunning = running = (base.speed == 5);
			if (running)
			{
				base.speed = 5;
			}
			else
			{
				base.speed = 2;
			}
			completelyStopAnimatingOrDoingAction();
			xOffset = 0f;
		}

		public bool isRidingHorse()
		{
			if (mount != null)
			{
				return !Game1.eventUp;
			}
			return false;
		}

		public List<Child> getChildren()
		{
			return Utility.getHomeOfFarmer(this).getChildren();
		}

		public int getChildrenCount()
		{
			return Utility.getHomeOfFarmer(this).getChildrenCount();
		}

		public Tool getToolFromName(string name)
		{
			foreach (Item t in items)
			{
				if (t != null && t is Tool && t.Name.Contains(name))
				{
					return (Tool)t;
				}
			}
			return null;
		}

		public override void SetMovingDown(bool b)
		{
			setMoving((byte)(4 + ((!b) ? 32 : 0)));
		}

		public override void SetMovingRight(bool b)
		{
			setMoving((byte)(2 + ((!b) ? 32 : 0)));
		}

		public override void SetMovingUp(bool b)
		{
			setMoving((byte)(1 + ((!b) ? 32 : 0)));
		}

		public override void SetMovingLeft(bool b)
		{
			setMoving((byte)(8 + ((!b) ? 32 : 0)));
		}

		public int? tryGetFriendshipLevelForNPC(string name)
		{
			if (friendshipData.TryGetValue(name, out Friendship friendship))
			{
				return friendship.Points;
			}
			return null;
		}

		public int getFriendshipLevelForNPC(string name)
		{
			if (friendshipData.TryGetValue(name, out Friendship friendship))
			{
				return friendship.Points;
			}
			return 0;
		}

		public int getFriendshipHeartLevelForNPC(string name)
		{
			return getFriendshipLevelForNPC(name) / 250;
		}

		public bool isRoommate(string name)
		{
			if (name == null)
			{
				return false;
			}
			if (friendshipData.TryGetValue(name, out Friendship friendship))
			{
				return friendship.IsRoommate();
			}
			return false;
		}

		public bool hasCurrentOrPendingRoommate()
		{
			if (spouse == null)
			{
				return false;
			}
			if (friendshipData.TryGetValue(spouse, out Friendship friendship))
			{
				return friendship.RoommateMarriage;
			}
			return false;
		}

		public bool hasRoommate()
		{
			if (spouse != null && isRoommate(spouse))
			{
				return true;
			}
			return false;
		}

		public bool hasAFriendWithHeartLevel(int heartLevel, bool datablesOnly)
		{
			foreach (NPC i in Utility.getAllCharacters())
			{
				if ((!datablesOnly || (bool)i.datable) && getFriendshipHeartLevelForNPC(i.Name) >= heartLevel)
				{
					return true;
				}
			}
			return false;
		}

		public int getTallyOfObject(int index, bool bigCraftable)
		{
			int tally = 0;
			foreach (Item i in items)
			{
				if (i is Object && (i as Object).ParentSheetIndex == index && (bool)(i as Object).bigCraftable == bigCraftable)
				{
					tally += i.Stack;
				}
			}
			return tally;
		}

		public bool areAllItemsNull()
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null)
				{
					return false;
				}
			}
			return true;
		}

		public void shippedBasic(int index, int number)
		{
			if (basicShipped.ContainsKey(index))
			{
				basicShipped[index] += number;
			}
			else
			{
				basicShipped.Add(index, number);
			}
		}

		public void shiftToolbar(bool right)
		{
			if (items == null || items.Count < 12 || UsingTool || Game1.dialogueUp || (!Game1.pickingTool && !Game1.player.CanMove) || areAllItemsNull() || Game1.eventUp || Game1.farmEvent != null)
			{
				return;
			}
			Game1.playSound("shwip");
			if (CurrentItem != null)
			{
				CurrentItem.actionWhenStopBeingHeld(this);
			}
			if (right)
			{
				List<Item> toMove2 = items.GetRange(0, 12);
				items.RemoveRange(0, 12);
				items.AddRange(toMove2);
			}
			else
			{
				List<Item> toMove = items.GetRange(items.Count - 12, 12);
				for (int j = 0; j < items.Count - 12; j++)
				{
					toMove.Add(items[j]);
				}
				items.Set(toMove);
			}
			netItemStowed.Set(newValue: false);
			if (CurrentItem != null)
			{
				CurrentItem.actionWhenBeingHeld(this);
			}
			int i = 0;
			while (true)
			{
				if (i < Game1.onScreenMenus.Count)
				{
					if (Game1.onScreenMenus[i] is Toolbar)
					{
						break;
					}
					i++;
					continue;
				}
				return;
			}
			(Game1.onScreenMenus[i] as Toolbar).shifted(right);
		}

		public void foundArtifact(int index, int number)
		{
			bool shouldHoldUpArtifact = false;
			if (index == 102)
			{
				if (!hasOrWillReceiveMail("lostBookFound"))
				{
					Game1.addMailForTomorrow("lostBookFound", noLetter: true);
					shouldHoldUpArtifact = true;
				}
				else
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14100"));
				}
				Game1.playSound("newRecipe");
				Game1.netWorldState.Value.LostBooksFound.Value++;
				Game1.multiplayer.globalChatInfoMessage("LostBook", base.displayName);
			}
			if (archaeologyFound.ContainsKey(index))
			{
				int[] artifact_entry = archaeologyFound[index];
				artifact_entry[0] += number;
				artifact_entry[1] += number;
				archaeologyFound[index] = artifact_entry;
			}
			else
			{
				if (archaeologyFound.Count() == 0)
				{
					if (!eventsSeen.Contains(0) && index != 102)
					{
						addQuest(23);
					}
					mailReceived.Add("artifactFound");
					shouldHoldUpArtifact = true;
				}
				archaeologyFound.Add(index, new int[2]
				{
					number,
					number
				});
			}
			if (shouldHoldUpArtifact)
			{
				holdUpItemThenMessage(new Object(index, 1));
			}
		}

		public void cookedRecipe(int index)
		{
			if (recipesCooked.ContainsKey(index))
			{
				recipesCooked[index]++;
			}
			else
			{
				recipesCooked.Add(index, 1);
			}
		}

		public bool caughtFish(int index, int size, bool from_fish_pond = false, int numberCaught = 1)
		{
			if (index >= 167 && index < 173)
			{
				return false;
			}
			bool sizeRecord = false;
			if (!from_fish_pond)
			{
				if (fishCaught.ContainsKey(index))
				{
					int[] fish_entry = fishCaught[index];
					fish_entry[0] += numberCaught;
					Game1.stats.checkForFishingAchievements();
					if (size > fishCaught[index][1])
					{
						fish_entry[1] = size;
						sizeRecord = true;
					}
					fishCaught[index] = fish_entry;
				}
				else
				{
					fishCaught.Add(index, new int[2]
					{
						numberCaught,
						size
					});
					Game1.stats.checkForFishingAchievements();
				}
				checkForQuestComplete(null, index, numberCaught, null, null, 7);
			}
			return sizeRecord;
		}

		public void gainExperience(int which, int howMuch)
		{
			if (which == 5 || howMuch <= 0)
			{
				return;
			}
			if (!IsLocalPlayer)
			{
				queueMessage(17, Game1.player, which, howMuch);
				return;
			}
			int newLevel = checkForLevelGain(experiencePoints[which], experiencePoints[which] + howMuch);
			experiencePoints[which] += howMuch;
			int oldLevel = -1;
			if (newLevel != -1)
			{
				switch (which)
				{
				case 0:
					oldLevel = farmingLevel;
					farmingLevel.Value = newLevel;
					break;
				case 3:
					oldLevel = miningLevel;
					miningLevel.Value = newLevel;
					break;
				case 1:
					oldLevel = fishingLevel;
					fishingLevel.Value = newLevel;
					break;
				case 2:
					oldLevel = foragingLevel;
					foragingLevel.Value = newLevel;
					break;
				case 5:
					oldLevel = luckLevel;
					luckLevel.Value = newLevel;
					break;
				case 4:
					oldLevel = combatLevel;
					combatLevel.Value = newLevel;
					break;
				}
			}
			if (newLevel > oldLevel)
			{
				for (int i = oldLevel + 1; i <= newLevel; i++)
				{
					newLevels.Add(new Point(which, i));
					_ = newLevels.Count;
					_ = 1;
				}
			}
		}

		public int getEffectiveSkillLevel(int whichSkill)
		{
			if (whichSkill < 0 || whichSkill > 5)
			{
				return -1;
			}
			int[] effectiveSkillLevels = new int[6]
			{
				farmingLevel,
				fishingLevel,
				foragingLevel,
				miningLevel,
				combatLevel,
				luckLevel
			};
			for (int i = 0; i < newLevels.Count; i++)
			{
				effectiveSkillLevels[newLevels[i].X]--;
			}
			return effectiveSkillLevels[whichSkill];
		}

		public static int checkForLevelGain(int oldXP, int newXP)
		{
			int highestLevel = -1;
			if (oldXP < 100 && newXP >= 100)
			{
				highestLevel = 1;
			}
			if (oldXP < 380 && newXP >= 380)
			{
				highestLevel = 2;
			}
			if (oldXP < 770 && newXP >= 770)
			{
				highestLevel = 3;
			}
			if (oldXP < 1300 && newXP >= 1300)
			{
				highestLevel = 4;
			}
			if (oldXP < 2150 && newXP >= 2150)
			{
				highestLevel = 5;
			}
			if (oldXP < 3300 && newXP >= 3300)
			{
				highestLevel = 6;
			}
			if (oldXP < 4800 && newXP >= 4800)
			{
				highestLevel = 7;
			}
			if (oldXP < 6900 && newXP >= 6900)
			{
				highestLevel = 8;
			}
			if (oldXP < 10000 && newXP >= 10000)
			{
				highestLevel = 9;
			}
			if (oldXP < 15000 && newXP >= 15000)
			{
				highestLevel = 10;
			}
			return highestLevel;
		}

		public void revealGiftTaste(NPC npc, int parent_sheet_index)
		{
			if (!giftedItems.ContainsKey(npc.name))
			{
				giftedItems[npc.name] = new SerializableDictionary<int, int>();
			}
			if (!giftedItems[npc.name].ContainsKey(parent_sheet_index))
			{
				giftedItems[npc.name][parent_sheet_index] = 0;
			}
		}

		public void revealGiftTaste(NPC npc, Object item)
		{
			if (!item.bigCraftable)
			{
				revealGiftTaste(npc, item.ParentSheetIndex);
			}
		}

		public void onGiftGiven(NPC npc, Object item)
		{
			if (!item.bigCraftable)
			{
				if (!giftedItems.ContainsKey(npc.name))
				{
					giftedItems[npc.name] = new SerializableDictionary<int, int>();
				}
				if (!giftedItems[npc.name].ContainsKey(item.ParentSheetIndex))
				{
					giftedItems[npc.name][item.ParentSheetIndex] = 0;
				}
				giftedItems[npc.name][item.ParentSheetIndex] = giftedItems[npc.name][item.ParentSheetIndex] + 1;
			}
		}

		public bool hasGiftTasteBeenRevealed(NPC npc, int item_index)
		{
			if (hasItemBeenGifted(npc, item_index))
			{
				return true;
			}
			if (!giftedItems.ContainsKey(npc.name))
			{
				return false;
			}
			if (!giftedItems[npc.name].ContainsKey(item_index))
			{
				return false;
			}
			return true;
		}

		public bool hasItemBeenGifted(NPC npc, int item_index)
		{
			if (!giftedItems.ContainsKey(npc.name))
			{
				return false;
			}
			if (!giftedItems[npc.name].ContainsKey(item_index))
			{
				return false;
			}
			return giftedItems[npc.name][item_index] > 0;
		}

		public void MarkItemAsTailored(Item item)
		{
			if (item != null)
			{
				string item_key = Utility.getStandardDescriptionFromItem(item, 1);
				if (!tailoredItems.ContainsKey(item_key))
				{
					tailoredItems[item_key] = 0;
				}
				tailoredItems[item_key]++;
			}
		}

		public bool HasTailoredThisItem(Item item)
		{
			if (item == null)
			{
				return false;
			}
			string item_key = Utility.getStandardDescriptionFromItem(item, 1);
			if (tailoredItems.ContainsKey(item_key))
			{
				return true;
			}
			return false;
		}

		public void foundMineral(int index)
		{
			if (mineralsFound.ContainsKey(index))
			{
				mineralsFound[index]++;
			}
			else
			{
				mineralsFound.Add(index, 1);
			}
			if (!hasOrWillReceiveMail("artifactFound"))
			{
				mailReceived.Add("artifactFound");
			}
		}

		public void increaseBackpackSize(int howMuch)
		{
			MaxItems += howMuch;
			for (int i = 0; i < howMuch; i++)
			{
				items.Add(null);
			}
		}

		public void consumeObject(int index, int quantity)
		{
			for (int i = items.Count - 1; i >= 0; i--)
			{
				if (items[i] != null && items[i] is Object && (int)((Object)items[i]).parentSheetIndex == index)
				{
					int toRemove = quantity;
					quantity -= items[i].Stack;
					items[i].Stack -= toRemove;
					if (items[i].Stack <= 0)
					{
						items[i] = null;
					}
					if (quantity <= 0)
					{
						break;
					}
				}
			}
		}

		public int getItemCount(int item_index, int min_price = 0)
		{
			return getItemCountInList(items, item_index, min_price);
		}

		public bool hasItemInInventory(int itemIndex, int quantity, int minPrice = 0)
		{
			if (getItemCount(itemIndex, minPrice) >= quantity)
			{
				return true;
			}
			return false;
		}

		public bool hasItemInInventoryNamed(string name)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null && items[i].Name != null && items[i].Name.Equals(name))
				{
					return true;
				}
			}
			return false;
		}

		public int getItemCountInList(IList<Item> list, int item_index, int min_price = 0)
		{
			int number_found = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && list[i] is Object && !(list[i] is Furniture) && !(list[i] is Wallpaper) && !(list[i] as Object).bigCraftable && (((Object)list[i]).ParentSheetIndex == item_index || (list[i] is Object && ((Object)list[i]).Category == item_index) || CraftingRecipe.isThereSpecialIngredientRule((Object)list[i], item_index)))
				{
					number_found += list[i].Stack;
				}
			}
			return number_found;
		}

		public bool hasItemInList(IList<Item> list, int itemIndex, int quantity, int minPrice = 0)
		{
			if (getItemCountInList(list, itemIndex, minPrice) >= quantity)
			{
				return true;
			}
			return false;
		}

		public void addItemByMenuIfNecessaryElseHoldUp(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null)
		{
			mostRecentlyGrabbedItem = item;
			addItemsByMenuIfNecessary(new List<Item>
			{
				item
			}, itemSelectedCallback);
			if (Game1.activeClickableMenu == null && (int)mostRecentlyGrabbedItem.parentSheetIndex != 434)
			{
				holdUpItemThenMessage(item);
			}
		}

		public void addItemByMenuIfNecessary(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null)
		{
			addItemsByMenuIfNecessary(new List<Item>
			{
				item
			}, itemSelectedCallback);
		}

		public void addItemsByMenuIfNecessary(List<Item> itemsToAdd, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null)
		{
			if (itemsToAdd == null || !IsLocalPlayer)
			{
				return;
			}
			if (itemsToAdd.Count > 0 && itemsToAdd[0] is Object && (int)(itemsToAdd[0] as Object).parentSheetIndex == 434)
			{
				eatObject(itemsToAdd[0] as Object, overrideFullness: true);
				if (Game1.activeClickableMenu != null)
				{
					Game1.activeClickableMenu.exitThisMenu(playSound: false);
				}
				return;
			}
			for (int i = itemsToAdd.Count - 1; i >= 0; i--)
			{
				if (addItemToInventoryBool(itemsToAdd[i]))
				{
					itemSelectedCallback?.Invoke(itemsToAdd[i], this);
					itemsToAdd.Remove(itemsToAdd[i]);
				}
			}
			if (itemsToAdd.Count > 0)
			{
				Game1.activeClickableMenu = new ItemGrabMenu(itemsToAdd).setEssential(essential: true);
				(Game1.activeClickableMenu as ItemGrabMenu).inventory.showGrayedOutSlots = true;
				(Game1.activeClickableMenu as ItemGrabMenu).inventory.onAddItem = itemSelectedCallback;
				(Game1.activeClickableMenu as ItemGrabMenu).source = 2;
			}
		}

		public void showRiding()
		{
			if (!isRidingHorse())
			{
				return;
			}
			xOffset = -6f;
			switch (base.FacingDirection)
			{
			case 0:
				FarmerSprite.setCurrentSingleFrame(113, 32000);
				break;
			case 1:
				FarmerSprite.setCurrentSingleFrame(106, 32000);
				xOffset += 2f;
				break;
			case 3:
				FarmerSprite.setCurrentSingleFrame(106, 32000, secondaryArm: false, flip: true);
				xOffset = -12f;
				break;
			case 2:
				FarmerSprite.setCurrentSingleFrame(107, 32000);
				break;
			}
			if (isMoving())
			{
				switch (mount.Sprite.currentAnimationIndex)
				{
				case 0:
					yOffset = 0f;
					break;
				case 1:
					yOffset = -4f;
					break;
				case 2:
					yOffset = -4f;
					break;
				case 3:
					yOffset = 0f;
					break;
				case 4:
					yOffset = 4f;
					break;
				case 5:
					yOffset = 4f;
					break;
				}
			}
			else
			{
				yOffset = 0f;
			}
		}

		public void showCarrying()
		{
			if (Game1.eventUp || isRidingHorse() || Game1.killScreen)
			{
				return;
			}
			if ((bool)bathingClothes)
			{
				showNotCarrying();
				return;
			}
			if (!FarmerSprite.PauseForSingleAnimation && !isMoving())
			{
				switch (base.FacingDirection)
				{
				case 0:
					FarmerSprite.setCurrentFrame(144);
					break;
				case 1:
					FarmerSprite.setCurrentFrame(136);
					break;
				case 2:
					FarmerSprite.setCurrentFrame(128);
					break;
				case 3:
					FarmerSprite.setCurrentFrame(152);
					break;
				}
			}
			if (ActiveObject != null)
			{
				mostRecentlyGrabbedItem = ActiveObject;
			}
			if (IsLocalPlayer && mostRecentlyGrabbedItem != null && mostRecentlyGrabbedItem is Object && (mostRecentlyGrabbedItem as Object).ParentSheetIndex == 434)
			{
				eatHeldObject();
			}
		}

		public void showNotCarrying()
		{
			if (!FarmerSprite.PauseForSingleAnimation && !isMoving())
			{
				bool canOnlyWalk = this.canOnlyWalk || (bool)bathingClothes;
				switch (base.FacingDirection)
				{
				case 0:
					FarmerSprite.setCurrentFrame(canOnlyWalk ? 16 : 48, canOnlyWalk ? 1 : 0);
					break;
				case 1:
					FarmerSprite.setCurrentFrame(canOnlyWalk ? 8 : 40, canOnlyWalk ? 1 : 0);
					break;
				case 2:
					FarmerSprite.setCurrentFrame((!canOnlyWalk) ? 32 : 0, canOnlyWalk ? 1 : 0);
					break;
				case 3:
					FarmerSprite.setCurrentFrame(canOnlyWalk ? 24 : 56, canOnlyWalk ? 1 : 0);
					break;
				}
			}
		}

		public int GetDaysMarried()
		{
			if (spouse == null || spouse == "")
			{
				return 0;
			}
			return friendshipData[spouse].DaysMarried;
		}

		public Friendship GetSpouseFriendship()
		{
			if (Game1.player.team.GetSpouse(UniqueMultiplayerID).HasValue)
			{
				long spouseID = Game1.player.team.GetSpouse(UniqueMultiplayerID).Value;
				return Game1.player.team.GetFriendship(UniqueMultiplayerID, spouseID);
			}
			if (spouse == null || spouse == "")
			{
				return null;
			}
			return friendshipData[spouse];
		}

		public bool hasDailyQuest()
		{
			for (int i = questLog.Count - 1; i >= 0; i--)
			{
				if ((bool)questLog[i].dailyQuest)
				{
					return true;
				}
			}
			return false;
		}

		public void showToolUpgradeAvailability()
		{
			int day = Game1.dayOfMonth;
			if (Game1.newDay)
			{
				day++;
			}
			if (toolBeingUpgraded != null && (int)daysLeftForToolUpgrade <= 0 && toolBeingUpgraded.Value != null && !Utility.isFestivalDay(day, Game1.currentSeason) && (Game1.shortDayNameFromDayOfSeason(day) != "Fri" || !hasCompletedCommunityCenter() || Game1.isRaining) && !hasReceivedToolUpgradeMessageYet)
			{
				if (Game1.newDay)
				{
					Game1.morningQueue.Enqueue(delegate
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", toolBeingUpgraded.Value.DisplayName));
					});
				}
				else
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", toolBeingUpgraded.Value.DisplayName));
				}
				hasReceivedToolUpgradeMessageYet = true;
			}
		}

		public void dayupdate()
		{
			resetFriendshipsForNewDay();
			acceptedDailyQuest.Set(newValue: false);
			dancePartner.Value = null;
			festivalScore = 0;
			forceTimePass = false;
			if ((int)daysLeftForToolUpgrade > 0)
			{
				daysLeftForToolUpgrade.Value--;
			}
			showToolUpgradeAvailability();
			if ((int)daysUntilHouseUpgrade > 0)
			{
				daysUntilHouseUpgrade.Value--;
				if ((int)daysUntilHouseUpgrade <= 0)
				{
					FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(this);
					homeOfFarmer.moveObjectsForHouseUpgrade((int)houseUpgradeLevel + 1);
					houseUpgradeLevel.Value++;
					daysUntilHouseUpgrade.Value = -1;
					homeOfFarmer.setMapForUpgradeLevel(houseUpgradeLevel);
					Game1.stats.checkForBuildingUpgradeAchievements();
				}
			}
			for (int i = questLog.Count - 1; i >= 0; i--)
			{
				if ((bool)questLog[i].dailyQuest)
				{
					questLog[i].daysLeft.Value--;
					if ((int)questLog[i].daysLeft <= 0 && !questLog[i].completed)
					{
						questLog.RemoveAt(i);
					}
				}
			}
			ClearBuffs();
			bobber = "";
			float oldStamina = Stamina;
			Stamina = MaxStamina;
			if ((bool)exhausted)
			{
				exhausted.Value = false;
				Stamina = MaxStamina / 2 + 1;
			}
			int bedTime = ((int)timeWentToBed == 0) ? Game1.timeOfDay : ((int)timeWentToBed);
			if (bedTime > 2400)
			{
				float staminaRestorationReduction = (1f - (float)(2600 - Math.Min(2600, bedTime)) / 200f) * (float)(MaxStamina / 2);
				Stamina -= staminaRestorationReduction;
				if (Game1.timeOfDay > 2700)
				{
					Stamina /= 2f;
				}
			}
			if (Game1.timeOfDay < 2700 && oldStamina > Stamina && !exhausted)
			{
				Stamina = oldStamina;
			}
			health = maxHealth;
			List<string> toRemove = new List<string>();
			foreach (string s2 in activeDialogueEvents.Keys.ToList())
			{
				activeDialogueEvents[s2]--;
				if (activeDialogueEvents[s2] < 0)
				{
					toRemove.Add(s2);
				}
			}
			foreach (string s in toRemove)
			{
				activeDialogueEvents.Remove(s);
			}
			hasMoved = false;
			if (Game1.random.NextDouble() < 0.905 && !hasOrWillReceiveMail("RarecrowSociety") && Utility.doesItemWithThisIndexExistAnywhere(136, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(137, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(138, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(139, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(140, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(126, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(110, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(113, bigCraftable: true))
			{
				mailbox.Add("RarecrowSociety");
			}
			timeWentToBed.Value = 0;
		}

		public void doDivorce()
		{
			divorceTonight.Value = false;
			if (!isMarried())
			{
				return;
			}
			if (spouse != null)
			{
				NPC currentSpouse = getSpouse();
				if (currentSpouse == null)
				{
					return;
				}
				spouse = null;
				for (int i = specialItems.Count - 1; i >= 0; i--)
				{
					if (specialItems[i] == 460)
					{
						specialItems.RemoveAt(i);
					}
				}
				if (friendshipData.ContainsKey(currentSpouse.name))
				{
					friendshipData[currentSpouse.name].Points = 0;
					friendshipData[currentSpouse.name].RoommateMarriage = false;
					friendshipData[currentSpouse.name].Status = FriendshipStatus.Divorced;
				}
				Utility.getHomeOfFarmer(this).showSpouseRoom();
				Game1.getFarm().addSpouseOutdoorArea("");
				removeQuest(126);
			}
			else if (team.GetSpouse(UniqueMultiplayerID).HasValue)
			{
				long spouseID = team.GetSpouse(UniqueMultiplayerID).Value;
				Friendship friendship = team.GetFriendship(UniqueMultiplayerID, spouseID);
				friendship.Points = 0;
				friendship.RoommateMarriage = false;
				friendship.Status = FriendshipStatus.Divorced;
			}
		}

		public static void showReceiveNewItemMessage(Farmer who)
		{
			string possibleSpecialMessage = who.mostRecentlyGrabbedItem.checkForSpecialItemHoldUpMeessage();
			if (possibleSpecialMessage != null)
			{
				Game1.drawObjectDialogue(possibleSpecialMessage);
			}
			else if ((int)who.mostRecentlyGrabbedItem.parentSheetIndex == 472 && who.mostRecentlyGrabbedItem.Stack == 15)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1918"));
			}
			else
			{
				Game1.drawObjectDialogue((who.mostRecentlyGrabbedItem.Stack > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1922", who.mostRecentlyGrabbedItem.Stack, who.mostRecentlyGrabbedItem.DisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", who.mostRecentlyGrabbedItem.DisplayName, Lexicon.getProperArticleForWord(who.mostRecentlyGrabbedItem.DisplayName)));
			}
			who.completelyStopAnimatingOrDoingAction();
		}

		public static void showEatingItem(Farmer who)
		{
			TemporaryAnimatedSprite tempSprite2 = null;
			if (who.itemToEat == null)
			{
				return;
			}
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				tempSprite2 = ((!who.IsLocalPlayer || who.itemToEat == null || !(who.itemToEat is Object) || (who.itemToEat as Object).ParentSheetIndex != 434) ? new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (who.itemToEat as Object).parentSheetIndex, 16, 16), 254f, 1, 0, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f) : new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 62.75f, 8, 2, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case 2:
				if (who.IsLocalPlayer && who.itemToEat != null && who.itemToEat is Object && (who.itemToEat as Object).ParentSheetIndex == 434)
				{
					tempSprite2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 81.25f, 8, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, -0.01f, 0f, 0f)
					{
						motion = new Vector2(0.8f, -11f),
						acceleration = new Vector2(0f, 0.5f)
					};
					break;
				}
				if (Game1.currentLocation == who.currentLocation)
				{
					Game1.playSound("dwop");
				}
				tempSprite2 = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (who.itemToEat as Object).parentSheetIndex, 16, 16), 650f, 1, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, -0.01f, 0f, 0f)
				{
					motion = new Vector2(0.8f, -11f),
					acceleration = new Vector2(0f, 0.5f)
				};
				break;
			case 3:
				who.yJumpVelocity = 6f;
				who.yJumpOffset = 1;
				break;
			case 4:
			{
				if (Game1.currentLocation == who.currentLocation)
				{
					Game1.playSound("eat");
				}
				for (int i = 0; i < 8; i++)
				{
					Microsoft.Xna.Framework.Rectangle r = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (who.itemToEat as Object).parentSheetIndex, 16, 16);
					r.X += 8;
					r.Y += 8;
					r.Width = 4;
					r.Height = 4;
					tempSprite2 = new TemporaryAnimatedSprite("Maps\\springobjects", r, 400f, 1, 0, who.Position + new Vector2(24f, -48f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-6, -3)),
						acceleration = new Vector2(0f, 0.5f)
					};
					who.currentLocation.temporarySprites.Add(tempSprite2);
				}
				return;
			}
			default:
				who.freezePause = 0;
				break;
			}
			if (tempSprite2 != null)
			{
				who.currentLocation.temporarySprites.Add(tempSprite2);
			}
		}

		public static void eatItem(Farmer who)
		{
		}

		public bool hasBuff(int whichBuff)
		{
			foreach (Buff otherBuff in Game1.buffsDisplay.otherBuffs)
			{
				if (otherBuff.which == whichBuff)
				{
					return true;
				}
			}
			return false;
		}

		public bool hasOrWillReceiveMail(string id)
		{
			if (!mailReceived.Contains(id) && !mailForTomorrow.Contains(id) && !Game1.mailbox.Contains(id))
			{
				return mailForTomorrow.Contains(id + "%&NL&%");
			}
			return true;
		}

		public static void showHoldingItem(Farmer who)
		{
			if (who.mostRecentlyGrabbedItem is SpecialItem)
			{
				TemporaryAnimatedSprite t = (who.mostRecentlyGrabbedItem as SpecialItem).getTemporarySpriteForHoldingUp(who.Position + new Vector2(0f, -124f));
				t.motion = new Vector2(0f, -0.1f);
				t.scale = 4f;
				t.interval = 2500f;
				t.totalNumberOfLoops = 0;
				t.animationLength = 1;
				Game1.currentLocation.temporarySprites.Add(t);
			}
			else if (who.mostRecentlyGrabbedItem is Slingshot)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\weapons", Game1.getSquareSourceRectForNonStandardTileSheet(Tool.weaponsTexture, 16, 16, (who.mostRecentlyGrabbedItem as Slingshot).IndexOfMenuItemView), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
			else if (who.mostRecentlyGrabbedItem is MeleeWeapon)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\weapons", Game1.getSquareSourceRectForNonStandardTileSheet(Tool.weaponsTexture, 16, 16, (who.mostRecentlyGrabbedItem as MeleeWeapon).IndexOfMenuItemView), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
			else if (who.mostRecentlyGrabbedItem is Boots)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSquareSourceRectForNonStandardTileSheet(Game1.objectSpriteSheet, 16, 16, (who.mostRecentlyGrabbedItem as Boots).indexInTileSheet), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
			else if (who.mostRecentlyGrabbedItem is Tool)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\tools", Game1.getSquareSourceRectForNonStandardTileSheet(Game1.toolSpriteSheet, 16, 16, (who.mostRecentlyGrabbedItem as Tool).IndexOfMenuItemView), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
			else if (who.mostRecentlyGrabbedItem is Furniture)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\furniture", (who.mostRecentlyGrabbedItem as Furniture).sourceRect, 2500f, 1, 0, who.Position + new Vector2(32 - (who.mostRecentlyGrabbedItem as Furniture).sourceRect.Width / 2 * 4, -188f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, -0.1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			else if (who.mostRecentlyGrabbedItem is Object && !(who.mostRecentlyGrabbedItem as Object).bigCraftable)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, who.mostRecentlyGrabbedItem.parentSheetIndex, 16, 16), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, -0.1f),
					scale = 4f,
					layerDepth = 1f
				});
				if (who.IsLocalPlayer && (int)who.mostRecentlyGrabbedItem.parentSheetIndex == 434)
				{
					who.eatHeldObject();
				}
			}
			else if (who.mostRecentlyGrabbedItem is Object)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\Craftables", Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, who.mostRecentlyGrabbedItem.parentSheetIndex, 16, 32), 2500f, 1, 0, who.Position + new Vector2(0f, -188f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, -0.1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			if (who.mostRecentlyGrabbedItem == null)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(420, 489, 25, 18), 2500f, 1, 0, who.Position + new Vector2(-20f, -152f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, -0.1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			else
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(10, who.Position + new Vector2(32f, -96f), Color.White)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
		}

		public void holdUpItemThenMessage(Item item, bool showMessage = true)
		{
			completelyStopAnimatingOrDoingAction();
			if (showMessage)
			{
				DelayedAction.playSoundAfterDelay("getNewSpecialItem", 750);
			}
			faceDirection(2);
			freezePause = 4000;
			FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[3]
			{
				new FarmerSprite.AnimationFrame(57, 0),
				new FarmerSprite.AnimationFrame(57, 2500, secondaryArm: false, flip: false, showHoldingItem),
				showMessage ? new FarmerSprite.AnimationFrame((short)FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false, showReceiveNewItemMessage, behaviorAtEndOfFrame: true) : new FarmerSprite.AnimationFrame((short)FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false)
			});
			mostRecentlyGrabbedItem = item;
			canMove = false;
		}

		private void checkForLevelUp()
		{
			int xpAtLevel = 600;
			int lastLevel = 0;
			int level = Level;
			for (int i = 0; i <= 35; i++)
			{
				if (level <= i && totalMoneyEarned >= xpAtLevel)
				{
					NewSkillPointsToSpend += 2;
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1925"), Color.Violet, 3500f));
				}
				else if (totalMoneyEarned < xpAtLevel)
				{
					break;
				}
				int num = xpAtLevel;
				xpAtLevel += (int)((double)(xpAtLevel - lastLevel) * 1.2);
				lastLevel = num;
			}
		}

		public void resetState()
		{
			mount = null;
			removeBuffAttributes();
			TemporaryItem = null;
			swimming.Value = false;
			bathingClothes.Value = false;
			ignoreCollisions = false;
			resetItemStates();
			fireToolEvent.Clear();
			beginUsingToolEvent.Clear();
			endUsingToolEvent.Clear();
			sickAnimationEvent.Clear();
			passOutEvent.Clear();
			drinkAnimationEvent.Clear();
			eatAnimationEvent.Clear();
		}

		public void resetItemStates()
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null)
				{
					items[i].resetState();
				}
			}
		}

		public void clearBackpack()
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i] = null;
			}
		}

		public int numberOfItemsInInventory()
		{
			int num = 0;
			foreach (Item o in items)
			{
				if (o != null && o is Object)
				{
					num++;
				}
			}
			return num;
		}

		public void resetFriendshipsForNewDay()
		{
			foreach (string name in friendshipData.Keys)
			{
				bool single = false;
				NPC i = Game1.getCharacterFromName(name);
				if (i == null)
				{
					i = Game1.getCharacterFromName<Child>(name, mustBeVillager: false);
				}
				if (i != null)
				{
					if (i != null && (bool)i.datable && !friendshipData[name].IsDating() && !i.isMarried())
					{
						single = true;
					}
					if (spouse != null && name.Equals(spouse) && !hasPlayerTalkedToNPC(name))
					{
						changeFriendship(-20, i);
					}
					else if (i != null && friendshipData[name].IsDating() && !hasPlayerTalkedToNPC(name) && friendshipData[name].Points < 2500)
					{
						changeFriendship(-8, i);
					}
					if (hasPlayerTalkedToNPC(name))
					{
						friendshipData[name].TalkedToToday = false;
					}
					else if ((!single && friendshipData[name].Points < 2500) || (single && friendshipData[name].Points < 2000))
					{
						changeFriendship(-2, i);
					}
				}
			}
			WorldDate tomorrow = new WorldDate(Game1.Date);
			int num = ++tomorrow.TotalDays;
			updateFriendshipGifts(tomorrow);
		}

		public void updateFriendshipGifts(WorldDate date)
		{
			foreach (string name in friendshipData.Keys)
			{
				if (friendshipData[name].LastGiftDate == null || date.TotalDays != friendshipData[name].LastGiftDate.TotalDays)
				{
					friendshipData[name].GiftsToday = 0;
				}
				if (friendshipData[name].LastGiftDate == null || date.TotalSundayWeeks != friendshipData[name].LastGiftDate.TotalSundayWeeks)
				{
					if (friendshipData[name].GiftsThisWeek == 2)
					{
						changeFriendship(10, Game1.getCharacterFromName(name));
					}
					friendshipData[name].GiftsThisWeek = 0;
				}
			}
		}

		public bool hasPlayerTalkedToNPC(string name)
		{
			if (!friendshipData.ContainsKey(name) && Game1.NPCGiftTastes.ContainsKey(name))
			{
				friendshipData.Add(name, new Friendship());
			}
			if (friendshipData.ContainsKey(name) && friendshipData[name].TalkedToToday)
			{
				return true;
			}
			return false;
		}

		public void fuelLantern(int units)
		{
			Tool lantern = getToolFromName("Lantern");
			if (lantern != null)
			{
				((Lantern)lantern).fuelLeft = Math.Min(100, ((Lantern)lantern).fuelLeft + units);
			}
		}

		public bool tryToCraftItem(List<int[]> ingredients, double successRate, int itemToCraft, bool bigCraftable, string craftingOrCooking)
		{
			List<int[]> locationOfIngredients = new List<int[]>();
			foreach (int[] ingredient in ingredients)
			{
				if (ingredient[0] <= -100)
				{
					int farmerStock = 0;
					switch (ingredient[0])
					{
					case -100:
						farmerStock = WoodPieces;
						break;
					case -101:
						farmerStock = stonePieces;
						break;
					case -102:
						farmerStock = CopperPieces;
						break;
					case -103:
						farmerStock = IronPieces;
						break;
					case -104:
						farmerStock = CoalPieces;
						break;
					case -105:
						farmerStock = GoldPieces;
						break;
					case -106:
						farmerStock = IridiumPieces;
						break;
					}
					if (farmerStock < ingredient[1])
					{
						return false;
					}
					locationOfIngredients.Add(ingredient);
				}
				else
				{
					for (int l = 0; l < ingredient[1]; l++)
					{
						int[] cheapestIndex = new int[2]
						{
							99999,
							-1
						};
						for (int k = 0; k < items.Count; k++)
						{
							if (items[k] != null && items[k] is Object && ((Object)items[k]).ParentSheetIndex == ingredient[0] && !containsIndex(locationOfIngredients, k))
							{
								locationOfIngredients.Add(new int[2]
								{
									k,
									1
								});
								break;
							}
							if (items[k] != null && items[k] is Object && ((Object)items[k]).Category == ingredient[0] && !containsIndex(locationOfIngredients, k) && ((Object)items[k]).Price < cheapestIndex[0])
							{
								cheapestIndex[0] = ((Object)items[k]).Price;
								cheapestIndex[1] = k;
							}
							if (k == items.Count - 1)
							{
								if (cheapestIndex[1] != -1)
								{
									locationOfIngredients.Add(new int[2]
									{
										cheapestIndex[1],
										ingredient[1]
									});
									break;
								}
								return false;
							}
						}
					}
				}
			}
			string fishType = "";
			switch (itemToCraft)
			{
			case 291:
				fishType = ((Object)items[locationOfIngredients[0][0]]).Name;
				break;
			case 216:
				if (Game1.random.NextDouble() < 0.5)
				{
					itemToCraft++;
				}
				break;
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1927", craftingOrCooking));
			isCrafting = true;
			Game1.playSound("crafting");
			int locationToPlace = -1;
			string message2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1930");
			if (bigCraftable)
			{
				Game1.player.ActiveObject = new Object(Vector2.Zero, itemToCraft);
				Game1.player.showCarrying();
			}
			else if (itemToCraft < 0)
			{
				if (1 == 0)
				{
					message2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1935");
				}
			}
			else
			{
				locationToPlace = locationOfIngredients[0][0];
				if (locationOfIngredients[0][0] < 0)
				{
					for (int j = 0; j < items.Count; j++)
					{
						if (items[j] == null)
						{
							locationToPlace = j;
							break;
						}
						if (j == (int)maxItems - 1)
						{
							Game1.pauseThenMessage(craftingTime + ingredients.Count() * 500, Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1936"), showProgressBar: true);
							return false;
						}
					}
				}
				if (!fishType.Equals(""))
				{
					items[locationToPlace] = new Object(Vector2.Zero, itemToCraft, fishType + " Bobber", canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
				}
				else
				{
					items[locationToPlace] = new Object(Vector2.Zero, itemToCraft, null, canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
				}
			}
			Game1.pauseThenMessage(craftingTime + ingredients.Count * 500, message2, showProgressBar: true);
			string a = craftingOrCooking.ToLower();
			if (!(a == "crafting"))
			{
				if (a == "cooking")
				{
					Game1.stats.ItemsCooked++;
				}
			}
			else
			{
				Game1.stats.ItemsCrafted++;
			}
			foreach (int[] i in locationOfIngredients)
			{
				if (i[0] <= -100)
				{
					switch (i[0])
					{
					case -100:
						WoodPieces -= i[1];
						break;
					case -101:
						stonePieces -= i[1];
						break;
					case -102:
						CopperPieces -= i[1];
						break;
					case -103:
						IronPieces -= i[1];
						break;
					case -104:
						CoalPieces -= i[1];
						break;
					case -105:
						GoldPieces -= i[1];
						break;
					case -106:
						IridiumPieces -= i[1];
						break;
					}
				}
				else if (i[0] != locationToPlace)
				{
					items[i[0]] = null;
				}
			}
			return true;
		}

		private static bool containsIndex(List<int[]> locationOfIngredients, int index)
		{
			for (int i = 0; i < locationOfIngredients.Count; i++)
			{
				if (locationOfIngredients[i][0] == index)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsEquippedItem(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (item == Game1.player.hat.Value)
			{
				return true;
			}
			if (item == Game1.player.shirtItem.Value)
			{
				return true;
			}
			if (item == Game1.player.pantsItem.Value)
			{
				return true;
			}
			if (item == Game1.player.leftRing.Value)
			{
				return true;
			}
			if (item == Game1.player.rightRing.Value)
			{
				return true;
			}
			if (item == Game1.player.boots.Value)
			{
				return true;
			}
			return false;
		}

		public override bool collideWith(Object o)
		{
			base.collideWith(o);
			if (isRidingHorse() && o is Fence)
			{
				mount.squeezeForGate();
				switch (base.FacingDirection)
				{
				case 3:
					if (o.tileLocation.X > (float)getTileX())
					{
						return false;
					}
					break;
				case 1:
					if (o.tileLocation.X < (float)getTileX())
					{
						return false;
					}
					break;
				}
			}
			return true;
		}

		public void changeIntoSwimsuit()
		{
			bathingClothes.Value = true;
			Halt();
			setRunning(isRunning: false);
			canOnlyWalk = true;
		}

		public void changeOutOfSwimSuit()
		{
			bathingClothes.Value = false;
			canOnlyWalk = false;
			Halt();
			FarmerSprite.StopAnimation();
			if (Game1.options.autoRun)
			{
				setRunning(isRunning: true);
			}
		}

		public bool ownsFurniture(string name)
		{
			foreach (string item in furnitureOwned)
			{
				if (item.Equals(name))
				{
					return true;
				}
			}
			return false;
		}

		public void showFrame(int frame, bool flip = false)
		{
			List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>();
			animationFrames.Add(new FarmerSprite.AnimationFrame(Convert.ToInt32(frame), 100, secondaryArm: false, flip));
			FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
			FarmerSprite.loop = true;
			FarmerSprite.PauseForSingleAnimation = true;
			Sprite.currentFrame = Convert.ToInt32(frame);
		}

		public void stopShowingFrame()
		{
			FarmerSprite.loop = false;
			FarmerSprite.PauseForSingleAnimation = false;
			completelyStopAnimatingOrDoingAction();
		}

		public Item addItemToInventory(Item item)
		{
			if (item == null)
			{
				return null;
			}
			if (item is SpecialItem)
			{
				return item;
			}
			for (int j = 0; j < (int)maxItems; j++)
			{
				if (j < items.Count && items[j] != null && items[j].maximumStackSize() != -1 && items[j].Stack < items[j].maximumStackSize() && items[j].Name.Equals(item.Name) && (!(item is Object) || !(items[j] is Object) || ((item as Object).quality.Value == (items[j] as Object).quality.Value && (item as Object).parentSheetIndex.Value == (items[j] as Object).parentSheetIndex.Value)) && item.canStackWith(items[j]))
				{
					int stackLeft = items[j].addToStack(item);
					if (stackLeft <= 0)
					{
						return null;
					}
					item.Stack = stackLeft;
				}
			}
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (items.Count > i && items[i] == null)
				{
					items[i] = item;
					return null;
				}
			}
			return item;
		}

		public bool isInventoryFull()
		{
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (items.Count > i && items[i] == null)
				{
					return false;
				}
			}
			return true;
		}

		public bool couldInventoryAcceptThisItem(Item item)
		{
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 102))
			{
				return true;
			}
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (items.Count > i && (items[i] == null || (item is Object && items[i] is Object && items[i].Stack + item.Stack <= items[i].maximumStackSize() && (items[i] as Object).canStackWith(item))))
				{
					return true;
				}
			}
			if (IsLocalPlayer && isInventoryFull() && Game1.hudMessages.Count() == 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			return false;
		}

		public bool couldInventoryAcceptThisObject(int index, int stack, int quality = 0)
		{
			if (index == 102)
			{
				return true;
			}
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (items.Count > i && (items[i] == null || (items[i] is Object && items[i].Stack + stack <= items[i].maximumStackSize() && (items[i] as Object).ParentSheetIndex == index && (int)(items[i] as Object).quality == quality)))
				{
					return true;
				}
			}
			if (IsLocalPlayer && isInventoryFull() && Game1.hudMessages.Count() == 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			return false;
		}

		public bool hasItemOfType(string type)
		{
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (items.Count > i && items[i] is Object && (items[i] as Object).type.Equals(type))
				{
					return true;
				}
			}
			return false;
		}

		public NPC getSpouse()
		{
			if (isMarried() && spouse != null)
			{
				return Game1.getCharacterFromName(spouse);
			}
			return null;
		}

		public int freeSpotsInInventory()
		{
			int number = 0;
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (i < items.Count && items[i] == null)
				{
					number++;
				}
			}
			return number;
		}

		public Item hasItemWithNameThatContains(string name)
		{
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (i < items.Count && items[i] != null && items[i].netName != null && items[i].Name.Contains(name))
				{
					return items[i];
				}
			}
			return null;
		}

		public bool addItemToInventoryBool(Item item, bool makeActiveObject = false)
		{
			if (item == null)
			{
				return false;
			}
			_ = item.Stack;
			Item tmp = IsLocalPlayer ? addItemToInventory(item) : null;
			bool success = tmp == null || tmp.Stack != item.Stack || item is SpecialItem;
			if (item is Object)
			{
				(item as Object).reloadSprite();
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 102))
			{
				success = true;
			}
			if (success && IsLocalPlayer)
			{
				if (item != null)
				{
					if (IsLocalPlayer && !item.HasBeenInInventory)
					{
						if (item is SpecialItem)
						{
							(item as SpecialItem).actionWhenReceived(this);
							return true;
						}
						if (item is Object && (item as Object).specialItem)
						{
							if ((bool)(item as Object).bigCraftable || item is Furniture)
							{
								if (!specialBigCraftables.Contains((item as Object).parentSheetIndex))
								{
									specialBigCraftables.Add((item as Object).parentSheetIndex);
								}
							}
							else if (!specialItems.Contains((item as Object).parentSheetIndex))
							{
								specialItems.Add((item as Object).parentSheetIndex);
							}
						}
						if (item is Object && (item as Object).Category == -2 && !(item as Object).hasBeenPickedUpByFarmer)
						{
							foundMineral((item as Object).parentSheetIndex);
						}
						else if (!(item is Furniture) && item is Object && (item as Object).type != null && (item as Object).type.Contains("Arch") && !(item as Object).hasBeenPickedUpByFarmer)
						{
							foundArtifact((item as Object).parentSheetIndex, 1);
						}
						if (Utility.IsNormalObjectAtParentSheetIndex(item, 102))
						{
							foundArtifact((item as Object).parentSheetIndex, 1);
							removeItemFromInventory(item);
						}
						else
						{
							switch ((int)item.parentSheetIndex)
							{
							case 384:
								Game1.stats.GoldFound += (uint)item.Stack;
								break;
							case 378:
								Game1.stats.CopperFound += (uint)item.Stack;
								break;
							case 380:
								Game1.stats.IronFound += (uint)item.Stack;
								break;
							case 386:
								Game1.stats.IridiumFound += (uint)item.Stack;
								break;
							}
						}
					}
					if (item is Object && !item.HasBeenInInventory)
					{
						Utility.checkItemFirstInventoryAdd(item);
						if ((bool)(item as Object).questItem)
						{
							return true;
						}
					}
					Color fontColor = Color.WhiteSmoke;
					string name = item.DisplayName;
					if (item is Object)
					{
						switch ((string)(item as Object).type)
						{
						case "Arch":
							fontColor = Color.Tan;
							name += Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1954");
							break;
						case "Fish":
							fontColor = Color.SkyBlue;
							break;
						case "Mineral":
							fontColor = Color.PaleVioletRed;
							break;
						case "Vegetable":
							fontColor = Color.PaleGreen;
							break;
						case "Fruit":
							fontColor = Color.Pink;
							break;
						}
					}
					if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ItemGrabMenu))
					{
						Game1.addHUDMessage(new HUDMessage(name, Math.Max(1, item.Stack), add: true, fontColor, item));
					}
					if (freezePause <= 0)
					{
						mostRecentlyGrabbedItem = item;
					}
					if (tmp != null && makeActiveObject && item.Stack <= 1)
					{
						int newItemPosition = getIndexOfInventoryItem(item);
						Item i = items[currentToolIndex];
						items[currentToolIndex] = items[newItemPosition];
						items[newItemPosition] = i;
					}
				}
				return success;
			}
			return false;
		}

		public int getIndexOfInventoryItem(Item item)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] == item || (items[i] != null && item != null && item.canStackWith(items[i])))
				{
					return i;
				}
			}
			return -1;
		}

		public void reduceActiveItemByOne()
		{
			if (CurrentItem != null && --CurrentItem.Stack <= 0)
			{
				removeItemFromInventory(CurrentItem);
				showNotCarrying();
			}
		}

		public bool removeItemsFromInventory(int index, int stack)
		{
			if (hasItemInInventory(index, stack))
			{
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i] != null && items[i] is Object && (int)(items[i] as Object).parentSheetIndex == index)
					{
						if (items[i].Stack > stack)
						{
							items[i].Stack -= stack;
							return true;
						}
						stack -= items[i].Stack;
						items[i] = null;
					}
					if (stack <= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public Item addItemToInventory(Item item, int position)
		{
			if (item != null && item is Object && (item as Object).specialItem)
			{
				if ((bool)(item as Object).bigCraftable)
				{
					if (!specialBigCraftables.Contains((item as Object).parentSheetIndex))
					{
						specialBigCraftables.Add((item as Object).parentSheetIndex);
					}
				}
				else if (!specialItems.Contains((item as Object).parentSheetIndex))
				{
					specialItems.Add((item as Object).parentSheetIndex);
				}
			}
			if (position >= 0 && position < items.Count)
			{
				if (items[position] == null)
				{
					items[position] = item;
					return null;
				}
				if (item != null && items[position].maximumStackSize() != -1 && items[position].Name.Equals(item.Name) && items[position].ParentSheetIndex == item.ParentSheetIndex && (!(item is Object) || !(items[position] is Object) || (item as Object).quality == (items[position] as Object).quality))
				{
					int stackLeft = items[position].addToStack(item);
					if (stackLeft <= 0)
					{
						return null;
					}
					item.Stack = stackLeft;
					return item;
				}
				Item result = items[position];
				items[position] = item;
				return result;
			}
			return item;
		}

		public void removeItemFromInventory(Item which)
		{
			int i = items.IndexOf(which);
			if (i >= 0 && i < items.Count)
			{
				items[i].actionWhenStopBeingHeld(this);
				items[i] = null;
			}
		}

		public Item removeItemFromInventory(int whichItemIndex)
		{
			if (whichItemIndex >= 0 && whichItemIndex < items.Count && items[whichItemIndex] != null)
			{
				Item item = items[whichItemIndex];
				items[whichItemIndex] = null;
				item.actionWhenStopBeingHeld(this);
				return item;
			}
			return null;
		}

		public bool isMarried()
		{
			if (team.IsMarried(UniqueMultiplayerID))
			{
				return true;
			}
			if (spouse != null && friendshipData.ContainsKey(spouse))
			{
				return friendshipData[spouse].IsMarried();
			}
			return false;
		}

		public bool isEngaged()
		{
			if (team.IsEngaged(UniqueMultiplayerID))
			{
				return true;
			}
			if (spouse != null && friendshipData.ContainsKey(spouse))
			{
				return friendshipData[spouse].IsEngaged();
			}
			return false;
		}

		public void removeFirstOfThisItemFromInventory(int parentSheetIndexOfItem)
		{
			if (ActiveObject != null && ActiveObject.ParentSheetIndex == parentSheetIndexOfItem)
			{
				ActiveObject.Stack--;
				if (ActiveObject.Stack <= 0)
				{
					ActiveObject = null;
					showNotCarrying();
				}
				return;
			}
			int i = 0;
			while (true)
			{
				if (i < items.Count)
				{
					if (items[i] != null && items[i] is Object && ((Object)items[i]).ParentSheetIndex == parentSheetIndexOfItem)
					{
						break;
					}
					i++;
					continue;
				}
				return;
			}
			items[i].Stack--;
			if (items[i].Stack <= 0)
			{
				items[i] = null;
			}
		}

		public void changeShirt(int whichShirt, bool is_customization_screen = false)
		{
			if (is_customization_screen)
			{
				int direction = whichShirt - (int)shirt;
				if (whichShirt < 0)
				{
					whichShirt = 111;
				}
				else if (whichShirt > 111)
				{
					whichShirt = 0;
				}
				if (whichShirt == 127 && !eventsSeen.Contains(3917601))
				{
					whichShirt = 127 + direction;
					if (whichShirt > FarmerRenderer.shirtsTexture.Height / 32 * 16 - 1)
					{
						whichShirt = 0;
					}
				}
			}
			shirt.Set(whichShirt);
			FarmerRenderer.changeShirt(whichShirt);
		}

		public void changePantStyle(int whichPants, bool is_customization_screen = false)
		{
			if (is_customization_screen)
			{
				_ = (int)pants;
				if (whichPants < 0)
				{
					whichPants = 3;
				}
				else if (whichPants > 3)
				{
					whichPants = 0;
				}
			}
			pants.Set(whichPants);
			FarmerRenderer.changePants(whichPants);
		}

		public void ConvertClothingOverrideToClothesItems()
		{
			if (pants.Value >= 0)
			{
				Clothing clothes2 = new Clothing(pants.Value);
				clothes2.clothesColor.Set(pantsColor);
				pantsItem.Value = clothes2;
				pants.Value = -1;
			}
			if (shirt.Value >= 0)
			{
				Clothing clothes = new Clothing(shirt.Value + 1000);
				shirtItem.Value = clothes;
				shirt.Value = -1;
			}
		}

		public void changeHairStyle(int whichHair)
		{
			bool wasBald = isBald();
			if (whichHair < 0)
			{
				whichHair = FarmerRenderer.hairStylesTexture.Height / 96 * 8 - 1;
			}
			else if (whichHair > FarmerRenderer.hairStylesTexture.Height / 96 * 8 - 1)
			{
				whichHair = 0;
			}
			hair.Set(whichHair);
			if ((uint)(whichHair - 49) <= 6u)
			{
				FarmerRenderer.textureName.Set(getTexture());
			}
			if (wasBald && !isBald())
			{
				FarmerRenderer.textureName.Set(getTexture());
			}
		}

		private bool isBald()
		{
			int num = getHair();
			if ((uint)(num - 49) <= 6u)
			{
				return true;
			}
			return false;
		}

		public void changeShoeColor(int which)
		{
			FarmerRenderer.recolorShoes(which);
			shoes.Set(which);
		}

		public void changeHairColor(Color c)
		{
			hairstyleColor.Set(c);
		}

		public void changePants(Color color)
		{
			pantsColor.Set(color);
		}

		public void changeHat(int newHat)
		{
			if (newHat < 0)
			{
				hat.Value = null;
			}
			else
			{
				hat.Value = new Hat(newHat);
			}
		}

		public void changeAccessory(int which)
		{
			if (which < -1)
			{
				which = 18;
			}
			if (which >= -1)
			{
				if (which >= 19)
				{
					which = -1;
				}
				accessory.Set(which);
			}
		}

		public void changeSkinColor(int which, bool force = false)
		{
			if (which < 0)
			{
				which = 23;
			}
			else if (which >= 24)
			{
				which = 0;
			}
			skin.Set(FarmerRenderer.recolorSkin(which, force));
		}

		public bool hasDarkSkin()
		{
			if (((int)skin >= 4 && (int)skin <= 8) || (int)skin == 14)
			{
				return true;
			}
			return false;
		}

		public void changeEyeColor(Color c)
		{
			newEyeColor.Set(c);
			FarmerRenderer.recolorEyes(c);
		}

		public int getHair(bool ignore_hat = false)
		{
			if (hat.Value != null && !bathingClothes && !ignore_hat)
			{
				switch (hat.Value.hairDrawType.Value)
				{
				case 2:
					return -1;
				case 1:
					switch ((int)hair)
					{
					case 50:
					case 51:
					case 52:
					case 53:
					case 54:
					case 55:
						return hair;
					case 48:
						return 6;
					case 49:
						return 52;
					case 3:
						return 11;
					case 1:
					case 5:
					case 6:
					case 9:
					case 11:
					case 17:
					case 20:
					case 23:
					case 24:
					case 25:
					case 27:
					case 28:
					case 29:
					case 30:
					case 32:
					case 33:
					case 34:
					case 36:
					case 39:
					case 41:
					case 43:
					case 44:
					case 45:
					case 46:
					case 47:
						return hair;
					case 18:
					case 19:
					case 21:
					case 31:
						return 23;
					case 42:
						return 46;
					default:
						if ((int)hair >= 16)
						{
							return 30;
						}
						return 7;
					}
				}
			}
			return hair;
		}

		public void changeGender(bool male)
		{
			if (male)
			{
				IsMale = true;
				FarmerRenderer.textureName.Set(getTexture());
				FarmerRenderer.heightOffset.Set(0);
			}
			else
			{
				IsMale = false;
				FarmerRenderer.heightOffset.Set(4);
				FarmerRenderer.textureName.Set(getTexture());
			}
			changeShirt(shirt);
		}

		public void changeFriendship(int amount, NPC n)
		{
			if (n == null || (!(n is Child) && !n.isVillager()) || (amount > 0 && n.Name.Equals("Dwarf") && !canUnderstandDwarves))
			{
				return;
			}
			if (friendshipData.ContainsKey(n.Name))
			{
				if (n.isDivorcedFrom(this) && amount > 0)
				{
					return;
				}
				friendshipData[n.Name].Points = Math.Max(0, Math.Min(friendshipData[n.Name].Points + amount, (Utility.GetMaximumHeartsForCharacter(n) + 1) * 250 - 1));
				if ((bool)n.datable && friendshipData[n.Name].Points >= 2000 && !hasOrWillReceiveMail("Bouquet"))
				{
					Game1.addMailForTomorrow("Bouquet");
				}
				if ((bool)n.datable && friendshipData[n.Name].Points >= 2500 && !hasOrWillReceiveMail("SeaAmulet"))
				{
					Game1.addMailForTomorrow("SeaAmulet");
				}
				if (friendshipData[n.Name].Points < 0)
				{
					friendshipData[n.Name].Points = 0;
				}
			}
			else
			{
				Game1.debugOutput = "Tried to change friendship for a friend that wasn't there.";
			}
			Game1.stats.checkForFriendshipAchievements();
		}

		public bool knowsRecipe(string name)
		{
			if (!craftingRecipes.Keys.Contains(name.Replace(" Recipe", "")))
			{
				return cookingRecipes.Keys.Contains(name.Replace(" Recipe", ""));
			}
			return true;
		}

		public Vector2 getUniformPositionAwayFromBox(int direction, int distance)
		{
			switch (base.FacingDirection)
			{
			case 0:
				return new Vector2(GetBoundingBox().Center.X, GetBoundingBox().Y - distance);
			case 1:
				return new Vector2(GetBoundingBox().Right + distance, GetBoundingBox().Center.Y);
			case 2:
				return new Vector2(GetBoundingBox().Center.X, GetBoundingBox().Bottom + distance);
			case 3:
				return new Vector2(GetBoundingBox().X - distance, GetBoundingBox().Center.Y);
			default:
				return Vector2.Zero;
			}
		}

		public bool hasTalkedToFriendToday(string npcName)
		{
			if (friendshipData.ContainsKey(npcName))
			{
				return friendshipData[npcName].TalkedToToday;
			}
			return false;
		}

		public void talkToFriend(NPC n, int friendshipPointChange = 20)
		{
			if (friendshipData.ContainsKey(n.Name) && !friendshipData[n.Name].TalkedToToday)
			{
				changeFriendship(friendshipPointChange, n);
				friendshipData[n.Name].TalkedToToday = true;
			}
		}

		public void moveRaft(GameLocation currentLocation, GameTime time)
		{
			float raftInertia = 0.2f;
			if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
			{
				yVelocity = Math.Max(yVelocity - raftInertia, -3f + Math.Abs(xVelocity) / 2f);
				faceDirection(0);
			}
			if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
			{
				xVelocity = Math.Min(xVelocity + raftInertia, 3f - Math.Abs(yVelocity) / 2f);
				faceDirection(1);
			}
			if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
			{
				yVelocity = Math.Min(yVelocity + raftInertia, 3f - Math.Abs(xVelocity) / 2f);
				faceDirection(2);
			}
			if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
			{
				xVelocity = Math.Max(xVelocity - raftInertia, -3f + Math.Abs(yVelocity) / 2f);
				faceDirection(3);
			}
			Microsoft.Xna.Framework.Rectangle collidingBox = new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)(base.Position.Y + 64f + 16f), 64, 64);
			collidingBox.X += (int)Math.Ceiling(xVelocity);
			if (!currentLocation.isCollidingPosition(collidingBox, Game1.viewport, isFarmer: true))
			{
				position.X += xVelocity;
			}
			collidingBox.X -= (int)Math.Ceiling(xVelocity);
			collidingBox.Y += (int)Math.Floor(yVelocity);
			if (!currentLocation.isCollidingPosition(collidingBox, Game1.viewport, isFarmer: true))
			{
				position.Y += yVelocity;
			}
			if (xVelocity != 0f || yVelocity != 0f)
			{
				raftPuddleCounter -= time.ElapsedGameTime.Milliseconds;
				if (raftPuddleCounter <= 0)
				{
					raftPuddleCounter = 250;
					currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(collidingBox.X, collidingBox.Y - 64), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					if (Game1.random.NextDouble() < 0.6)
					{
						Game1.playSound("wateringCan");
					}
					if (Game1.random.NextDouble() < 0.6)
					{
						raftBobCounter /= 2;
					}
				}
			}
			raftBobCounter -= time.ElapsedGameTime.Milliseconds;
			if (raftBobCounter <= 0)
			{
				raftBobCounter = Game1.random.Next(15, 28) * 100;
				if (yOffset <= 0f)
				{
					yOffset = 4f;
					currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(collidingBox.X, collidingBox.Y - 64), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
				}
				else
				{
					yOffset = 0f;
				}
			}
			if (xVelocity > 0f)
			{
				xVelocity = Math.Max(0f, xVelocity - raftInertia / 2f);
			}
			else if (xVelocity < 0f)
			{
				xVelocity = Math.Min(0f, xVelocity + raftInertia / 2f);
			}
			if (yVelocity > 0f)
			{
				yVelocity = Math.Max(0f, yVelocity - raftInertia / 2f);
			}
			else if (yVelocity < 0f)
			{
				yVelocity = Math.Min(0f, yVelocity + raftInertia / 2f);
			}
		}

		public void warpFarmer(Warp w)
		{
			if (w != null && !Game1.eventUp)
			{
				Halt();
				Game1.warpFarmer(w.TargetName, w.TargetX, w.TargetY, w.flipFarmer);
			}
		}

		public void startToPassOut()
		{
			passOutEvent.Fire();
		}

		private void performPassOut()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (!swimming.Value && bathingClothes.Value)
			{
				bathingClothes.Value = false;
			}
			if (!passedOut && !FarmerSprite.isPassingOut())
			{
				faceDirection(2);
				completelyStopAnimatingOrDoingAction();
				animateOnce(293);
			}
		}

		public static void passOutFromTired(Farmer who)
		{
			if (!who.IsLocalPlayer)
			{
				return;
			}
			if (who.isRidingHorse())
			{
				who.mount.dismount();
			}
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.emergencyShutDown();
				Game1.exitActiveMenu();
			}
			who.completelyStopAnimatingOrDoingAction();
			if ((bool)who.bathingClothes)
			{
				who.changeOutOfSwimSuit();
			}
			who.swimming.Value = false;
			who.CanMove = false;
			if (who == Game1.player && (FarmerTeam.SleepAnnounceModes)Game1.player.team.sleepAnnounceMode != FarmerTeam.SleepAnnounceModes.Off)
			{
				string key = "PassedOut";
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
			GameLocation passOutLocation = Game1.currentLocation;
			Vector2 bed = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).getBedSpot()) * 64f;
			bed.X -= 64f;
			LocationRequest.Callback continuePassOut = delegate
			{
				who.Position = bed;
				who.currentLocation.lastTouchActionLocation = bed;
				if (!Game1.IsMultiplayer || Game1.timeOfDay >= 2600)
				{
					Game1.PassOutNewDay();
				}
				Game1.changeMusicTrack("none");
				if (!(passOutLocation is FarmHouse) && !(passOutLocation is Cellar))
				{
					int num = Math.Min(1000, who.Money / 10);
					string text = "";
					Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)Game1.player.UniqueMultiplayerID);
					if (random.Next(0, 3) == 0 && Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
					{
						text = "passedOut4";
						num = 0;
					}
					else
					{
						Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
						List<int> list = new List<int>(new int[3]
						{
							1,
							2,
							3
						});
						if (Game1.player.getSpouse() != null && Game1.player.getSpouse().Name.Equals("Harvey"))
						{
							list.Remove(3);
						}
						if (Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
						{
							list.Remove(1);
						}
						int random2 = Utility.GetRandom(list, random);
						text = (dictionary.ContainsKey("passedOut" + random2 + "_" + ((num > 0) ? "Billed" : "NotBilled") + "_" + (Game1.player.IsMale ? "Male" : "Female")) ? ("passedOut" + random2 + "_" + ((num > 0) ? "Billed" : "NotBilled") + "_" + (Game1.player.IsMale ? "Male" : "Female") + " " + num) : (dictionary.ContainsKey("passedOut" + random2 + "_" + ((num > 0) ? "Billed" : "NotBilled")) ? ("passedOut" + random2 + "_" + ((num > 0) ? "Billed" : "NotBilled") + " " + num) : ((!dictionary.ContainsKey("passedOut" + random2)) ? ("passedOut2 " + num) : ("passedOut" + random2 + " " + num))));
					}
					if (num > 0)
					{
						who.Money -= num;
					}
					who.mailForTomorrow.Add(text);
				}
			};
			if (!who.isInBed || (who.currentLocation != null && !who.currentLocation.Equals(who.homeLocation.Value)))
			{
				LocationRequest locationRequest = Game1.getLocationRequest(who.homeLocation.Value);
				Game1.warpFarmer(locationRequest, (int)bed.X / 64, (int)bed.Y / 64, 2);
				locationRequest.OnWarp += continuePassOut;
				who.FarmerSprite.setCurrentSingleFrame(5, 3000);
				who.FarmerSprite.PauseForSingleAnimation = true;
			}
			else
			{
				continuePassOut();
			}
		}

		public static void doSleepEmote(Farmer who)
		{
			who.doEmote(24);
			who.yJumpVelocity = -2f;
		}

		public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			if (mount != null && !mount.dismounting)
			{
				return mount.GetBoundingBox();
			}
			return new Microsoft.Xna.Framework.Rectangle((int)base.Position.X + 8, (int)base.Position.Y + Sprite.getHeight() - 32, 48, 32);
		}

		public string getPetName()
		{
			foreach (NPC j in Game1.getFarm().characters)
			{
				if (j is Pet)
				{
					return j.Name;
				}
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (NPC i in Utility.getHomeOfFarmer(allFarmer).characters)
				{
					if (i is Pet)
					{
						return i.Name;
					}
				}
			}
			return "the Farm";
		}

		public Pet getPet()
		{
			foreach (NPC j in Game1.getFarm().characters)
			{
				if (j is Pet)
				{
					return j as Pet;
				}
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (NPC i in Utility.getHomeOfFarmer(allFarmer).characters)
				{
					if (i is Pet)
					{
						return i as Pet;
					}
				}
			}
			return null;
		}

		public string getPetDisplayName()
		{
			foreach (NPC j in Game1.getFarm().characters)
			{
				if (j is Pet)
				{
					return j.displayName;
				}
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (NPC i in Utility.getHomeOfFarmer(allFarmer).characters)
				{
					if (i is Pet)
					{
						return i.displayName;
					}
				}
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1972");
		}

		public bool hasPet()
		{
			foreach (NPC character in Game1.getFarm().characters)
			{
				if (character is Pet)
				{
					return true;
				}
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (NPC character2 in Utility.getHomeOfFarmer(allFarmer).characters)
				{
					if (character2 is Pet)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void UpdateClothing()
		{
			FarmerRenderer.MarkSpriteDirty();
		}

		public int GetPantsIndex()
		{
			if (pants.Value >= 0)
			{
				return pants.Value;
			}
			if (pantsItem.Value != null)
			{
				if ((bool)isMale || (int)pantsItem.Value.indexInTileSheetFemale < 0)
				{
					return pantsItem.Value.indexInTileSheetMale;
				}
				return pantsItem.Value.indexInTileSheetFemale;
			}
			return 14;
		}

		public int GetShirtIndex()
		{
			if (shirt.Value >= 0)
			{
				return shirt.Value;
			}
			if (shirtItem.Value != null)
			{
				if ((bool)isMale || (int)shirtItem.Value.indexInTileSheetFemale < 0)
				{
					return shirtItem.Value.indexInTileSheetMale;
				}
				return shirtItem.Value.indexInTileSheetFemale;
			}
			if (IsMale)
			{
				return 209;
			}
			return 41;
		}

		public List<string> GetShirtExtraData()
		{
			if (shirt.Value > 0)
			{
				return new Clothing(shirt.Value + 1000).GetOtherData();
			}
			if (shirtItem.Value != null)
			{
				return shirtItem.Value.GetOtherData();
			}
			return new List<string>();
		}

		public Color GetShirtColor()
		{
			if (shirt.Value >= 0)
			{
				return Color.White;
			}
			if (shirtItem.Value != null)
			{
				if ((bool)shirtItem.Value.isPrismatic)
				{
					return Utility.GetPrismaticColor();
				}
				return shirtItem.Value.clothesColor.Value;
			}
			return DEFAULT_SHIRT_COLOR;
		}

		public Color GetPantsColor()
		{
			if (pants.Value >= 0)
			{
				return pantsColor.Value;
			}
			if (pantsItem.Value != null)
			{
				if ((bool)pantsItem.Value.isPrismatic)
				{
					return Utility.GetPrismaticColor();
				}
				return pantsItem.Value.clothesColor.Value;
			}
			return Color.White;
		}

		public bool movedDuringLastTick()
		{
			return !base.Position.Equals(lastPosition);
		}

		public int CompareTo(object obj)
		{
			return ((Farmer)obj).saveTime - saveTime;
		}

		public float getDrawLayer()
		{
			return (float)getStandingY() / 10000f + drawLayerDisambiguator;
		}

		public override void draw(SpriteBatch b)
		{
			if (base.currentLocation == null || (!base.currentLocation.Equals(Game1.currentLocation) && !IsLocalPlayer && !Game1.currentLocation.Name.Equals("Temp") && !isFakeEventActor) || ((bool)hidden && (base.currentLocation.currentEvent == null || this != base.currentLocation.currentEvent.farmer) && (!IsLocalPlayer || Game1.locationRequest == null)))
			{
				return;
			}
			if (isRidingHorse())
			{
				mount.SyncPositionToRider();
				mount.draw(b);
			}
			float draw_layer = getDrawLayer();
			Vector2 origin = new Vector2(xOffset, (yOffset + 128f - (float)(GetBoundingBox().Height / 2)) / 4f + 4f);
			numUpdatesSinceLastDraw = 0;
			PropertyValue shadow = null;
			Tile shadowTile = Game1.currentLocation.Map.GetLayer("Buildings").PickTile(new Location(getStandingX(), getStandingY()), Game1.viewport.Size);
			if (isGlowing && coloredBorder)
			{
				b.Draw(Sprite.Texture, new Vector2(getLocalPosition(Game1.viewport).X - 4f, getLocalPosition(Game1.viewport).Y - 4f), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, Vector2.Zero, 1.1f, SpriteEffects.None, Math.Max(0f, draw_layer - 0.001f));
			}
			else if (isGlowing && !coloredBorder)
			{
				FarmerRenderer.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport) + jitter + new Vector2(0f, yJumpOffset), origin, Math.Max(0f, draw_layer + 0.00011f), glowingColor * glowingTransparency, rotation, this);
			}
			shadowTile?.TileIndexProperties.TryGetValue("Shadow", out shadow);
			if (shadow == null)
			{
				if (!temporarilyInvincible || temporaryInvincibilityTimer % 100 < 50)
				{
					farmerRenderer.Value.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport) + jitter + new Vector2(0f, yJumpOffset), origin, Math.Max(0f, draw_layer + 0.0001f), Color.White, rotation, this);
				}
			}
			else
			{
				farmerRenderer.Value.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport), origin, Math.Max(0f, draw_layer + 0.0001f), Color.White, rotation, this);
				farmerRenderer.Value.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport), origin, Math.Max(0f, draw_layer + 0.0002f), Color.Black * 0.25f, rotation, this);
			}
			if (isRafting)
			{
				b.Draw(Game1.toolSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(0f, yOffset), Game1.getSourceRectForStandardTileSheet(Game1.toolSpriteSheet, 1), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, draw_layer - 0.001f);
			}
			if (Game1.activeClickableMenu == null && !Game1.eventUp && IsLocalPlayer && CurrentTool != null && (Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.options.alwaysShowToolHitLocation) && CurrentTool.doesShowTileLocationMarker() && (!Game1.options.hideToolHitLocationWhenInMotion || !isMoving()))
			{
				Vector2 drawLocation = Game1.GlobalToLocal(Game1.viewport, (Utility.withinRadiusOfPlayer(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y, 1, this) && !Game1.options.gamepadControls) ? (new Vector2((Game1.getOldMouseX() + Game1.viewport.X) / 64, (Game1.getOldMouseY() + Game1.viewport.Y) / 64) * 64f) : Utility.clampToTile(GetToolLocation()));
				if (!Game1.wasMouseVisibleThisFrame || Game1.isAnyGamePadButtonBeingPressed())
				{
					drawLocation = Game1.GlobalToLocal(Game1.viewport, Utility.clampToTile(GetToolLocation()));
				}
				b.Draw(Game1.mouseCursors, drawLocation, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (GetToolLocation().Y + 64f) / 10000f);
			}
			if (base.IsEmoting)
			{
				Vector2 emotePosition = getLocalPosition(Game1.viewport);
				emotePosition.Y -= 160f;
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer);
			}
			if (ActiveObject != null && IsCarrying())
			{
				Game1.drawPlayerHeldObject(this);
			}
			if (sparklingText != null)
			{
				sparklingText.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(32f - sparklingText.textWidth / 2f, -128f)));
			}
			bool pickingTool = IsLocalPlayer && Game1.pickingTool;
			if ((UsingTool | pickingTool) && CurrentTool != null && (!CurrentTool.Name.Equals("Seeds") || pickingTool))
			{
				Game1.drawTool(this);
			}
		}

		public static void drinkGlug(Farmer who)
		{
			Color c = Color.LightBlue;
			if (who.itemToEat != null)
			{
				switch (who.itemToEat.Name.Split(' ').Last())
				{
				case "Tonic":
					c = Color.Red;
					break;
				case "Remedy":
					c = Color.LimeGreen;
					break;
				case "Cola":
				case "Espresso":
				case "Coffee":
					c = new Color(46, 20, 0);
					break;
				case "Wine":
					c = Color.Purple;
					break;
				case "Beer":
					c = Color.Orange;
					break;
				case "Milk":
					c = Color.White;
					break;
				case "Tea":
				case "Juice":
					c = Color.LightGreen;
					break;
				}
			}
			if (Game1.currentLocation == who.currentLocation)
			{
				Game1.playSound("gulp");
			}
			who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(653, 858, 1, 1), 9999f, 1, 1, who.Position + new Vector2(32 + Game1.random.Next(-2, 3) * 4, -48f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.001f, 0.04f, c, 5f, 0f, 0f, 0f)
			{
				acceleration = new Vector2(0f, 0.5f)
			});
		}

		public void handleDisconnect()
		{
			if (base.currentLocation != null)
			{
				if (rightRing.Value != null)
				{
					rightRing.Value.onLeaveLocation(this, base.currentLocation);
				}
				if (leftRing.Value != null)
				{
					leftRing.Value.onLeaveLocation(this, base.currentLocation);
				}
			}
		}

		public bool isDivorced()
		{
			foreach (Friendship value in friendshipData.Values)
			{
				if (value.IsDivorced())
				{
					return true;
				}
			}
			return false;
		}

		public void wipeExMemories()
		{
			foreach (string npcName in friendshipData.Keys)
			{
				Friendship friendship = friendshipData[npcName];
				if (friendship.IsDivorced())
				{
					friendship.Clear();
					NPC i = Game1.getCharacterFromName(npcName);
					if (i != null)
					{
						i.CurrentDialogue.Clear();
						i.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:WipedMemory"), i));
						Game1.stats.incrementStat("exMemoriesWiped", 1);
					}
				}
			}
		}

		public void getRidOfChildren()
		{
			for (int i = Utility.getHomeOfFarmer(this).characters.Count() - 1; i >= 0; i--)
			{
				if (Utility.getHomeOfFarmer(this).characters[i] is Child)
				{
					Utility.getHomeOfFarmer(this).characters.RemoveAt(i);
					Game1.stats.incrementStat("childrenTurnedToDoves", 1);
				}
			}
		}

		public void animateOnce(int whichAnimation)
		{
			FarmerSprite.animateOnce(whichAnimation, 100f, 6);
			CanMove = false;
		}

		public static void showItemIntake(Farmer who)
		{
			TemporaryAnimatedSprite tempSprite = null;
			Object toShow = (who.mostRecentlyGrabbedItem != null && who.mostRecentlyGrabbedItem is Object) ? ((Object)who.mostRecentlyGrabbedItem) : ((who.ActiveObject == null) ? null : who.ActiveObject);
			if (toShow == null)
			{
				return;
			}
			switch (who.FacingDirection)
			{
			case 2:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -32f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 2:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -43f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 3:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 4:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 5:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
					break;
				}
				break;
			case 1:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(28f, -64f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 2:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(24f, -72f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 3:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(4f, -128f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 4:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 5:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
					break;
				}
				break;
			case 0:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -32f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 2:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -43f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 3:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 4:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 5:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
					break;
				}
				break;
			case 3:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(-32f, -64f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 2:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(-28f, -76f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 3:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(-16f, -128f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 4:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 5:
					tempSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, toShow.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
					break;
				}
				break;
			}
			if ((toShow.Equals(who.ActiveObject) || (who.ActiveObject != null && toShow != null && toShow.ParentSheetIndex == (int)who.ActiveObject.parentSheetIndex)) && who.FarmerSprite.currentAnimationIndex == 5)
			{
				tempSprite = null;
			}
			if (tempSprite != null)
			{
				who.currentLocation.temporarySprites.Add(tempSprite);
			}
			if (who.mostRecentlyGrabbedItem is ColoredObject && tempSprite != null)
			{
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)toShow.parentSheetIndex + 1, 16, 16), tempSprite.interval, 1, 0, tempSprite.Position, flicker: false, flipped: false, tempSprite.layerDepth + 0.0001f, tempSprite.alphaFade, (who.mostRecentlyGrabbedItem as ColoredObject).color, 4f, tempSprite.scaleChange, 0f, 0f));
			}
			if (who.FarmerSprite.currentAnimationIndex == 5)
			{
				who.Halt();
				who.FarmerSprite.CurrentAnimation = null;
			}
		}

		public static void showSwordSwipe(Farmer who)
		{
			TemporaryAnimatedSprite tempSprite = null;
			bool dagger = who.CurrentTool != null && who.CurrentTool is MeleeWeapon && (int)(who.CurrentTool as MeleeWeapon).type == 1;
			Vector2 actionTile = who.GetToolLocation(ignoreClick: true);
			if (who.CurrentTool != null && who.CurrentTool is MeleeWeapon)
			{
				(who.CurrentTool as MeleeWeapon).DoDamage(who.currentLocation, (int)actionTile.X, (int)actionTile.Y, who.FacingDirection, 1, who);
			}
			switch (who.FacingDirection)
			{
			case 2:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 0:
					if (dagger)
					{
						who.yVelocity = -0.6f;
					}
					break;
				case 1:
					who.yVelocity = (dagger ? 0.5f : (-0.5f));
					break;
				case 5:
					who.yVelocity = 0.3f;
					tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(503, 256, 42, 17), who.Position + new Vector2(-16f, -2f) * 4f, flipped: false, 0.07f, Color.White)
					{
						scale = 4f,
						animationLength = 1,
						interval = who.FarmerSprite.CurrentAnimationFrame.milliseconds,
						alpha = 0.5f,
						layerDepth = (who.Position.Y + 64f) / 10000f
					};
					break;
				}
				break;
			case 1:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 0:
					if (dagger)
					{
						who.xVelocity = 0.6f;
					}
					break;
				case 1:
					who.xVelocity = (dagger ? (-0.5f) : 0.5f);
					break;
				case 5:
					who.xVelocity = -0.3f;
					tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(4f, -12f) * 4f, flipped: false, 0.07f, Color.White)
					{
						scale = 4f,
						animationLength = 1,
						interval = who.FarmerSprite.CurrentAnimationFrame.milliseconds,
						alpha = 0.5f
					};
					break;
				}
				break;
			case 3:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 0:
					if (dagger)
					{
						who.xVelocity = -0.6f;
					}
					break;
				case 1:
					who.xVelocity = (dagger ? 0.5f : (-0.5f));
					break;
				case 5:
					who.xVelocity = 0.3f;
					tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(-15f, -12f) * 4f, flipped: false, 0.07f, Color.White)
					{
						scale = 4f,
						animationLength = 1,
						interval = who.FarmerSprite.CurrentAnimationFrame.milliseconds,
						flipped = true,
						alpha = 0.5f
					};
					break;
				}
				break;
			case 0:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 0:
					if (dagger)
					{
						who.yVelocity = 0.6f;
					}
					break;
				case 1:
					who.yVelocity = (dagger ? (-0.5f) : 0.5f);
					break;
				case 5:
					who.yVelocity = -0.3f;
					tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(0f, -32f) * 4f, flipped: false, 0.07f, Color.White)
					{
						scale = 4f,
						animationLength = 1,
						interval = who.FarmerSprite.CurrentAnimationFrame.milliseconds,
						alpha = 0.5f,
						rotation = 3.926991f
					};
					break;
				}
				break;
			}
			if (tempSprite != null)
			{
				if (who.CurrentTool != null && who.CurrentTool is MeleeWeapon && who.CurrentTool.InitialParentTileIndex == 4)
				{
					tempSprite.color = Color.HotPink;
				}
				who.currentLocation.temporarySprites.Add(tempSprite);
			}
		}

		public static void showToolSwipeEffect(Farmer who)
		{
			if (who.CurrentTool != null && who.CurrentTool is WateringCan)
			{
				_ = who.FacingDirection;
				return;
			}
			switch (who.FacingDirection)
			{
			case 1:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(20f, -132f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 3:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(-92f, -132f), Color.White, 4, flipped: true, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 2:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(19, who.Position + new Vector2(-4f, -128f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 0:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(18, who.Position + new Vector2(0f, -132f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 100f : 50f, 0, 64, 1f, 64)
				{
					layerDepth = (float)(who.getStandingY() - 9) / 10000f
				});
				break;
			}
		}

		public static void canMoveNow(Farmer who)
		{
			who.CanMove = true;
			who.UsingTool = false;
			who.usingSlingshot = false;
			who.FarmerSprite.PauseForSingleAnimation = false;
			who.yVelocity = 0f;
			who.xVelocity = 0f;
		}

		public void FireTool()
		{
			fireToolEvent.Fire();
		}

		public void synchronizedJump(float velocity)
		{
			if (IsLocalPlayer)
			{
				synchronizedJumpEvent.Fire(velocity);
				synchronizedJumpEvent.Poll();
			}
		}

		protected void performSynchronizedJump(float velocity)
		{
			yJumpVelocity = velocity;
			yJumpOffset = -1;
		}

		private void performFireTool()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (CurrentTool != null)
			{
				CurrentTool.leftClick(this);
			}
		}

		public static void useTool(Farmer who)
		{
			if (who.toolOverrideFunction != null)
			{
				who.toolOverrideFunction(who);
			}
			else if (who.CurrentTool != null)
			{
				float oldStamina = who.stamina;
				if (who.IsLocalPlayer)
				{
					who.CurrentTool.DoFunction(who.currentLocation, (int)who.GetToolLocation().X, (int)who.GetToolLocation().Y, 1, who);
				}
				who.lastClick = Vector2.Zero;
				who.checkForExhaustion(oldStamina);
				Game1.toolHold = 0f;
			}
		}

		public void BeginUsingTool()
		{
			beginUsingToolEvent.Fire();
		}

		private void performBeginUsingTool()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (CurrentTool != null)
			{
				FarmerSprite.setOwner(this);
				CanMove = false;
				UsingTool = true;
				canReleaseTool = true;
				CurrentTool.beginUsing(base.currentLocation, (int)lastClick.X, (int)lastClick.Y, this);
			}
		}

		public void EndUsingTool()
		{
			if (this == Game1.player)
			{
				endUsingToolEvent.Fire();
			}
			else
			{
				performEndUsingTool();
			}
		}

		private void performEndUsingTool()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (CurrentTool != null)
			{
				CurrentTool.endUsing(base.currentLocation, this);
			}
		}

		public void checkForExhaustion(float oldStamina)
		{
			if (stamina <= 0f && oldStamina > 0f)
			{
				if (!exhausted && IsLocalPlayer)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1986"));
				}
				setRunning(isRunning: false);
				doEmote(36);
			}
			else if (stamina <= 15f && oldStamina > 15f && IsLocalPlayer)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1987"));
			}
			if (stamina <= 0f)
			{
				exhausted.Value = true;
			}
		}

		public void setMoving(byte command)
		{
			if (movementDirections.Count < 2)
			{
				if (command == 1 && !movementDirections.Contains(0) && !movementDirections.Contains(2))
				{
					movementDirections.Insert(0, 0);
				}
				if (command == 2 && !movementDirections.Contains(1) && !movementDirections.Contains(3))
				{
					movementDirections.Insert(0, 1);
				}
				if (command == 4 && !movementDirections.Contains(2) && !movementDirections.Contains(0))
				{
					movementDirections.Insert(0, 2);
				}
				if (command == 8 && !movementDirections.Contains(3) && !movementDirections.Contains(1))
				{
					movementDirections.Insert(0, 3);
				}
			}
			if (command == 33)
			{
				movementDirections.Remove(0);
			}
			if (command == 34)
			{
				movementDirections.Remove(1);
			}
			if (command == 36)
			{
				movementDirections.Remove(2);
			}
			if (command == 40)
			{
				movementDirections.Remove(3);
			}
			switch (command)
			{
			case 16:
				setRunning(isRunning: true);
				break;
			case 48:
				setRunning(isRunning: false);
				break;
			}
			if ((command & 0x40) == 64)
			{
				Halt();
				running = false;
			}
		}

		public void toolPowerIncrease()
		{
			if (toolPower == 0)
			{
				toolPitchAccumulator = 0;
			}
			toolPower++;
			if (CurrentTool is Pickaxe && toolPower == 1)
			{
				toolPower += 2;
			}
			Color powerUpColor = Color.White;
			int frameOffset = (base.FacingDirection == 0) ? 4 : ((base.FacingDirection == 2) ? 2 : 0);
			switch (toolPower)
			{
			case 1:
				powerUpColor = Color.Orange;
				if (!(CurrentTool is WateringCan))
				{
					FarmerSprite.CurrentFrame = 72 + frameOffset;
				}
				jitterStrength = 0.25f;
				break;
			case 2:
				powerUpColor = Color.LightSteelBlue;
				if (!(CurrentTool is WateringCan))
				{
					FarmerSprite.CurrentFrame++;
				}
				jitterStrength = 0.5f;
				break;
			case 3:
				powerUpColor = Color.Gold;
				jitterStrength = 1f;
				break;
			case 4:
				powerUpColor = Color.Violet;
				jitterStrength = 2f;
				break;
			}
			int xAnimation = (base.FacingDirection == 1) ? 64 : ((base.FacingDirection == 3) ? (-64) : ((base.FacingDirection == 2) ? 32 : 0));
			int yAnimation = 192;
			if (CurrentTool is WateringCan)
			{
				xAnimation = -xAnimation;
				yAnimation = 128;
			}
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(21, base.Position - new Vector2(xAnimation, yAnimation), powerUpColor, 8, flipped: false, 70f, 0, 64, (float)getStandingY() / 10000f + 0.005f, 128));
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, 64, 64), 50f, 4, 0, base.Position - new Vector2((base.FacingDirection != 1) ? (-64) : 0, 128f), flicker: false, base.FacingDirection == 1, (float)getStandingY() / 10000f, 0.01f, Color.White, 1f, 0f, 0f, 0f));
			if (Game1.soundBank != null)
			{
				ICue cue = Game1.soundBank.GetCue("toolCharge");
				Random r = new Random(Game1.dayOfMonth + (int)base.Position.X * 1000 + (int)base.Position.Y);
				cue.SetVariable("Pitch", r.Next(12, 16) * 100 + toolPower * 100);
				cue.Play();
			}
		}

		public void UpdateIfOtherPlayer(GameTime time)
		{
			if (base.currentLocation == null)
			{
				return;
			}
			position.UpdateExtrapolation(getMovementSpeed());
			position.Field.InterpolationEnabled = !currentLocationRef.IsChanging();
			if (_lastSelectedItem != CurrentItem)
			{
				if (_lastSelectedItem != null)
				{
					_lastSelectedItem.actionWhenStopBeingHeld(this);
				}
				_lastSelectedItem = CurrentItem;
			}
			FarmerSprite.setOwner(this);
			fireToolEvent.Poll();
			beginUsingToolEvent.Poll();
			endUsingToolEvent.Poll();
			drinkAnimationEvent.Poll();
			eatAnimationEvent.Poll();
			sickAnimationEvent.Poll();
			passOutEvent.Poll();
			doEmoteEvent.Poll();
			kissFarmerEvent.Poll();
			haltAnimationEvent.Poll();
			synchronizedJumpEvent.Poll();
			FarmerSprite.checkForSingleAnimation(time);
			updateCommon(time, base.currentLocation);
		}

		public void forceCanMove()
		{
			forceTimePass = false;
			movementDirections.Clear();
			isEating = false;
			CanMove = true;
			Game1.freezeControls = false;
			freezePause = 0;
			UsingTool = false;
			usingSlingshot = false;
			FarmerSprite.PauseForSingleAnimation = false;
			if (CurrentTool is FishingRod)
			{
				(CurrentTool as FishingRod).isFishing = false;
			}
		}

		public void dropItem(Item i)
		{
			if (i != null && i.canBeDropped())
			{
				Game1.createItemDebris(i.getOne(), getStandingPosition(), base.FacingDirection);
			}
		}

		public bool addEvent(string eventName, int daysActive)
		{
			if (!activeDialogueEvents.ContainsKey(eventName))
			{
				activeDialogueEvents.Add(eventName, daysActive);
				return true;
			}
			return false;
		}

		public void dropObjectFromInventory(int parentSheetIndex, int quantity)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] == null || !(items[i] is Object) || (int)(items[i] as Object).parentSheetIndex != parentSheetIndex)
				{
					continue;
				}
				while (quantity > 0)
				{
					dropItem(items[i].getOne());
					items[i].Stack--;
					quantity--;
					if (items[i].Stack <= 0)
					{
						items[i] = null;
						break;
					}
				}
				if (quantity <= 0)
				{
					break;
				}
			}
		}

		public Vector2 getMostRecentMovementVector()
		{
			return new Vector2(base.Position.X - lastPosition.X, base.Position.Y - lastPosition.Y);
		}

		public void dropActiveItem()
		{
			if (CurrentItem != null && CurrentItem.canBeDropped())
			{
				Game1.createItemDebris(CurrentItem.getOne(), getStandingPosition(), base.FacingDirection);
				reduceActiveItemByOne();
			}
		}

		public int GetSkillLevel(int index)
		{
			switch (index)
			{
			case 0:
				return FarmingLevel;
			case 3:
				return MiningLevel;
			case 1:
				return FishingLevel;
			case 2:
				return ForagingLevel;
			case 5:
				return LuckLevel;
			case 4:
				return CombatLevel;
			default:
				return 0;
			}
		}

		public int GetUnmodifiedSkillLevel(int index)
		{
			switch (index)
			{
			case 0:
				return farmingLevel.Value;
			case 3:
				return miningLevel.Value;
			case 1:
				return fishingLevel.Value;
			case 2:
				return foragingLevel.Value;
			case 5:
				return luckLevel.Value;
			case 4:
				return combatLevel.Value;
			default:
				return 0;
			}
		}

		public static string getSkillNameFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return "Farming";
			case 3:
				return "Mining";
			case 1:
				return "Fishing";
			case 2:
				return "Foraging";
			case 5:
				return "Luck";
			case 4:
				return "Combat";
			default:
				return "";
			}
		}

		public static string getSkillDisplayNameFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1991");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1992");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1993");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1994");
			case 5:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1995");
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1996");
			default:
				return "";
			}
		}

		public bool hasCompletedCommunityCenter()
		{
			if (mailReceived.Contains("ccBoilerRoom") && mailReceived.Contains("ccCraftsRoom") && mailReceived.Contains("ccPantry") && mailReceived.Contains("ccFishTank") && mailReceived.Contains("ccVault"))
			{
				return mailReceived.Contains("ccBulletin");
			}
			return false;
		}

		private bool localBusMoving()
		{
			if (base.currentLocation is Desert)
			{
				Desert desert = base.currentLocation as Desert;
				if (!desert.drivingOff)
				{
					return desert.drivingBack;
				}
				return true;
			}
			if (base.currentLocation is BusStop)
			{
				BusStop busStop = base.currentLocation as BusStop;
				if (!busStop.drivingOff)
				{
					return busStop.drivingBack;
				}
				return true;
			}
			return false;
		}

		public void takeDamage(int damage, bool overrideParry, Monster damager)
		{
			if (Game1.eventUp || FarmerSprite.isPassingOut())
			{
				return;
			}
			bool num = damager != null && !damager.isInvincible() && !overrideParry;
			bool monsterDamageCapable = (damager == null || !damager.isInvincible()) && (damager == null || (!(damager is GreenSlime) && !(damager is BigSlime)) || !isWearingRing(520));
			bool playerParryable = CurrentTool != null && CurrentTool is MeleeWeapon && ((MeleeWeapon)CurrentTool).isOnSpecial && (int)((MeleeWeapon)CurrentTool).type == 3;
			bool playerDamageable = !temporarilyInvincible && !Game1.player.isEating && !Game1.fadeToBlack && !Game1.buffsDisplay.hasBuff(21);
			if (num & playerParryable)
			{
				Rumble.rumble(0.75f, 150f);
				base.currentLocation.playSound("parry");
				damager.parried(damage, this);
			}
			else if (monsterDamageCapable && playerDamageable)
			{
				damager?.onDealContactDamage(this);
				if (isWearingRing(524) && !Game1.buffsDisplay.hasBuff(21) && Game1.random.NextDouble() < (0.9 - (double)((float)health / 100f)) / (double)(3 - LuckLevel / 10) + ((health <= 15) ? 0.2 : 0.0))
				{
					base.currentLocation.playSound("yoba");
					Game1.buffsDisplay.addOtherBuff(new Buff(21));
					return;
				}
				Rumble.rumble(0.75f, 150f);
				damage += Game1.random.Next(Math.Min(-1, -damage / 8), Math.Max(1, damage / 8));
				damage = Math.Max(1, damage - resilience);
				health = Math.Max(0, health - damage);
				temporarilyInvincible = true;
				base.currentLocation.debris.Add(new Debris(damage, new Vector2(getStandingX() + 8, getStandingY()), Color.Red, 1f, this));
				base.currentLocation.playSound("ow");
				Game1.hitShakeTimer = 100 * damage;
			}
		}

		private void checkDamage(GameLocation location)
		{
			if (!Game1.eventUp)
			{
				foreach (NPC character in location.characters)
				{
					if (character is Monster)
					{
						Monster monster = character as Monster;
						if (monster.GetBoundingBox().Intersects(GetBoundingBox()))
						{
							monster.currentLocation = location;
							monster.collisionWithFarmerBehavior();
							if (monster.DamageToFarmer > 0)
							{
								if (CurrentTool != null && CurrentTool is MeleeWeapon && ((MeleeWeapon)CurrentTool).isOnSpecial && (int)((MeleeWeapon)CurrentTool).type == 3)
								{
									takeDamage(monster.DamageToFarmer, overrideParry: false, character as Monster);
								}
								else
								{
									takeDamage(Math.Max(1, monster.DamageToFarmer + Game1.random.Next(-monster.DamageToFarmer / 4, monster.DamageToFarmer / 4)), overrideParry: false, character as Monster);
								}
							}
						}
					}
				}
			}
		}

		public bool checkAction(Farmer who, GameLocation location)
		{
			if (who.isRidingHorse())
			{
				who.Halt();
			}
			if ((bool)hidden)
			{
				return false;
			}
			if (Game1.CurrentEvent != null)
			{
				if (Game1.CurrentEvent.isSpecificFestival("spring24") && who.dancePartner.Value == null)
				{
					who.Halt();
					who.faceGeneralDirection(getStandingPosition());
					string question3 = Game1.content.LoadString("Strings\\UI:AskToDance_" + (IsMale ? "Male" : "Female"), base.Name);
					location.createQuestionDialogue(question3, location.createYesNoResponses(), delegate(Farmer _, string answer)
					{
						if (answer == "Yes")
						{
							who.team.SendProposal(this, ProposalType.Dance);
							Game1.activeClickableMenu = new PendingProposalDialog();
						}
					});
					return true;
				}
				return false;
			}
			if (who.CurrentItem != null && (int)who.CurrentItem.parentSheetIndex == 801 && !isMarried() && !isEngaged() && !who.isMarried() && !who.isEngaged())
			{
				who.Halt();
				who.faceGeneralDirection(getStandingPosition());
				string question2 = Game1.content.LoadString("Strings\\UI:AskToMarry_" + (IsMale ? "Male" : "Female"), base.Name);
				location.createQuestionDialogue(question2, location.createYesNoResponses(), delegate(Farmer _, string answer)
				{
					if (answer == "Yes")
					{
						who.team.SendProposal(this, ProposalType.Marriage, who.CurrentItem.getOne());
						Game1.activeClickableMenu = new PendingProposalDialog();
					}
				});
				return true;
			}
			if (who.CanMove && who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.ActiveObject.questItem)
			{
				who.Halt();
				who.faceGeneralDirection(getStandingPosition());
				string question = Game1.content.LoadString("Strings\\UI:GiftPlayerItem_" + (IsMale ? "Male" : "Female"), who.ActiveObject.DisplayName, base.Name);
				location.createQuestionDialogue(question, location.createYesNoResponses(), delegate(Farmer _, string answer)
				{
					if (answer == "Yes")
					{
						who.team.SendProposal(this, ProposalType.Gift, who.ActiveObject.getOne());
						Game1.activeClickableMenu = new PendingProposalDialog();
					}
				});
				return true;
			}
			long? playerSpouseID = team.GetSpouse(UniqueMultiplayerID);
			if ((playerSpouseID.HasValue & (who.UniqueMultiplayerID == playerSpouseID)) && who.CanMove && !who.isMoving() && !isMoving() && Utility.IsHorizontalDirection(getGeneralDirectionTowards(who.getStandingPosition(), -10, opposite: false, useTileCalculations: false)))
			{
				who.Halt();
				who.faceGeneralDirection(getStandingPosition());
				who.kissFarmerEvent.Fire(UniqueMultiplayerID);
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, new Vector2(getTileX(), getTileY()) * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.5f),
					alphaFade = 0.01f
				});
				base.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
				return true;
			}
			return false;
		}

		public void Update(GameTime time, GameLocation location)
		{
			FarmerSprite.setOwner(this);
			position.UpdateExtrapolation(getMovementSpeed());
			fireToolEvent.Poll();
			beginUsingToolEvent.Poll();
			endUsingToolEvent.Poll();
			drinkAnimationEvent.Poll();
			eatAnimationEvent.Poll();
			sickAnimationEvent.Poll();
			passOutEvent.Poll();
			doEmoteEvent.Poll();
			kissFarmerEvent.Poll();
			synchronizedJumpEvent.Poll();
			if (IsLocalPlayer)
			{
				if (base.currentLocation == null)
				{
					return;
				}
				hidden.Value = (localBusMoving() || (location.currentEvent != null && !location.currentEvent.isFestival) || (location.currentEvent != null && location.currentEvent.doingSecretSanta) || Game1.locationRequest != null || !Game1.displayFarmer);
				isInBed.Value = (base.currentLocation.doesTileHaveProperty((int)getTileLocation().X, (int)getTileLocation().Y, "Bed", "Back") != null);
				if (!Game1.options.allowStowing)
				{
					netItemStowed.Value = false;
				}
				hasMenuOpen.Value = (Game1.activeClickableMenu != null);
			}
			if (Game1.CurrentEvent == null && !bathingClothes)
			{
				canOnlyWalk = false;
			}
			if (noMovementPause > 0)
			{
				CanMove = false;
				noMovementPause -= time.ElapsedGameTime.Milliseconds;
				if (noMovementPause <= 0)
				{
					CanMove = true;
				}
			}
			if (freezePause > 0)
			{
				CanMove = false;
				freezePause -= time.ElapsedGameTime.Milliseconds;
				if (freezePause <= 0)
				{
					CanMove = true;
				}
			}
			if (sparklingText != null && sparklingText.update(time))
			{
				sparklingText = null;
			}
			if (newLevelSparklingTexts.Count > 0 && sparklingText == null && !UsingTool && CanMove && Game1.activeClickableMenu == null)
			{
				sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2003", getSkillDisplayNameFromIndex(newLevelSparklingTexts.Peek())), Color.White, Color.White, rainbow: true);
				newLevelSparklingTexts.Dequeue();
			}
			if ((bool)isInBed && Game1.IsMultiplayer)
			{
				regenTimer -= time.ElapsedGameTime.Milliseconds;
				if (regenTimer < 0)
				{
					regenTimer = 500;
					if (this.stamina < (float)(int)maxStamina)
					{
						float stamina = this.stamina;
						this.stamina = stamina + 1f;
					}
					if (health < maxHealth)
					{
						health++;
					}
				}
			}
			FarmerSprite.setOwner(this);
			FarmerSprite.checkForSingleAnimation(time);
			if (CanMove)
			{
				rotation = 0f;
				if (health <= 0 && !Game1.killScreen && Game1.timeOfDay < 2600)
				{
					CanMove = false;
					Game1.screenGlowOnce(Color.Red, hold: true);
					Game1.killScreen = true;
					faceDirection(2);
					FarmerSprite.setCurrentFrame(5);
					jitterStrength = 1f;
					Game1.pauseTime = 3000f;
					Rumble.rumbleAndFade(0.75f, 1500f);
					freezePause = 8000;
					if (Game1.currentSong != null && Game1.currentSong.IsPlaying)
					{
						Game1.currentSong.Stop(AudioStopOptions.Immediate);
					}
					base.currentLocation.playSound("death");
					Game1.dialogueUp = false;
					Game1.stats.TimesUnconscious++;
					if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is GameMenu)
					{
						Game1.activeClickableMenu.emergencyShutDown();
						Game1.activeClickableMenu = null;
					}
				}
				switch (getDirection())
				{
				case 0:
					location.isCollidingWithWarp(nextPosition(0));
					break;
				case 1:
					location.isCollidingWithWarp(nextPosition(1));
					break;
				case 2:
					location.isCollidingWithWarp(nextPosition(2));
					break;
				case 3:
					location.isCollidingWithWarp(nextPosition(3));
					break;
				}
				if (collisionNPC != null)
				{
					collisionNPC.farmerPassesThrough = true;
				}
				if (movementDirections.Count > 0 && !isRidingHorse() && location.isCollidingWithCharacter(nextPosition(base.FacingDirection)) != null)
				{
					charactercollisionTimer += time.ElapsedGameTime.Milliseconds;
					if (charactercollisionTimer > 400)
					{
						location.isCollidingWithCharacter(nextPosition(base.FacingDirection)).shake(50);
					}
					if (charactercollisionTimer >= 1500 && collisionNPC == null)
					{
						collisionNPC = location.isCollidingWithCharacter(nextPosition(base.FacingDirection));
						if (collisionNPC.Name.Equals("Bouncer") && base.currentLocation != null && base.currentLocation.name.Equals("SandyHouse"))
						{
							collisionNPC.showTextAboveHead(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2010"));
							collisionNPC = null;
							charactercollisionTimer = 0;
						}
						else if (collisionNPC.name.Equals("Henchman") && base.currentLocation != null && base.currentLocation.name.Equals("WitchSwamp"))
						{
							collisionNPC = null;
							charactercollisionTimer = 0;
						}
					}
				}
				else
				{
					charactercollisionTimer = 0;
					if (collisionNPC != null && location.isCollidingWithCharacter(nextPosition(base.FacingDirection)) == null)
					{
						collisionNPC.farmerPassesThrough = false;
						collisionNPC = null;
					}
				}
			}
			if (Game1.shouldTimePass())
			{
				MeleeWeapon.weaponsTypeUpdate(time);
			}
			if (!Game1.eventUp || movementDirections.Count <= 0 || base.currentLocation.currentEvent == null || base.currentLocation.currentEvent.playerControlSequence)
			{
				lastPosition = base.Position;
				if (controller != null)
				{
					if (controller.update(time))
					{
						controller = null;
					}
				}
				else if (controller == null)
				{
					MovePosition(time, Game1.viewport, location);
				}
			}
			updateCommon(time, location);
			position.Paused = (FarmerSprite.PauseForSingleAnimation || UsingTool || isEating);
			checkDamage(location);
		}

		private void updateCommon(GameTime time, GameLocation location)
		{
			if (jitterStrength > 0f)
			{
				jitter = new Vector2((float)Game1.random.Next(-(int)(jitterStrength * 100f), (int)((jitterStrength + 1f) * 100f)) / 100f, (float)Game1.random.Next(-(int)(jitterStrength * 100f), (int)((jitterStrength + 1f) * 100f)) / 100f);
			}
			if (yJumpOffset != 0)
			{
				yJumpVelocity -= 0.5f;
				yJumpOffset -= (int)yJumpVelocity;
				if (yJumpOffset >= 0)
				{
					yJumpOffset = 0;
					yJumpVelocity = 0f;
				}
			}
			updateMovementAnimation(time);
			updateEmote(time);
			updateGlow();
			currentLocationRef.Update();
			if ((bool)exhausted && this.stamina <= 1f)
			{
				currentEyes = 4;
				blinkTimer = -1000;
			}
			blinkTimer += time.ElapsedGameTime.Milliseconds;
			if (blinkTimer > 2200 && Game1.random.NextDouble() < 0.01)
			{
				blinkTimer = -150;
				currentEyes = 4;
			}
			else if (blinkTimer > -100)
			{
				if (blinkTimer < -50)
				{
					currentEyes = 1;
				}
				else if (blinkTimer < 0)
				{
					currentEyes = 4;
				}
				else
				{
					currentEyes = 0;
				}
			}
			if (isCustomized.Value && isInBed.Value && ((timerSinceLastMovement >= 3000 && Game1.timeOfDay >= 630) || timeWentToBed.Value != 0))
			{
				currentEyes = 1;
				blinkTimer = -10;
			}
			UpdateItemStow();
			if ((bool)swimming)
			{
				yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
				int oldSwimTimer = swimTimer;
				swimTimer -= time.ElapsedGameTime.Milliseconds;
				if (timerSinceLastMovement == 0)
				{
					if (oldSwimTimer > 400 && swimTimer <= 400 && IsLocalPlayer)
					{
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, getStandingY() - 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					}
					if (swimTimer < 0)
					{
						swimTimer = 800;
						if (IsLocalPlayer)
						{
							base.currentLocation.playSound("slosh");
							Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, getStandingY() - 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
						}
					}
				}
				else if (!Game1.eventUp && (Game1.activeClickableMenu == null || Game1.IsMultiplayer) && !Game1.paused)
				{
					if (timerSinceLastMovement > 800)
					{
						currentEyes = 1;
					}
					else if (timerSinceLastMovement > 700)
					{
						currentEyes = 4;
					}
					if (swimTimer < 0)
					{
						swimTimer = 100;
						if (this.stamina < (float)(int)maxStamina)
						{
							float stamina = this.stamina;
							this.stamina = stamina + 1f;
						}
						if (health < maxHealth)
						{
							health++;
						}
					}
				}
			}
			if (!isMoving())
			{
				timerSinceLastMovement += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				timerSinceLastMovement = 0;
			}
			for (int i = items.Count - 1; i >= 0; i--)
			{
				if (items[i] != null && items[i] is Tool)
				{
					((Tool)items[i]).tickUpdate(time, this);
				}
			}
			if (TemporaryItem is Tool)
			{
				(TemporaryItem as Tool).tickUpdate(time, this);
			}
			if (rightRing.Value != null)
			{
				rightRing.Value.update(time, location, this);
			}
			if (leftRing.Value != null)
			{
				leftRing.Value.update(time, location, this);
			}
			if (mount != null)
			{
				mount.update(time, location);
				if (mount != null)
				{
					mount.SyncPositionToRider();
				}
			}
		}

		public void UpdateItemStow()
		{
			if (_itemStowed != netItemStowed.Value)
			{
				if (netItemStowed.Value && ActiveObject != null)
				{
					ActiveObject.actionWhenStopBeingHeld(this);
				}
				_itemStowed = netItemStowed.Value;
				if (!netItemStowed.Value && ActiveObject != null)
				{
					ActiveObject.actionWhenBeingHeld(this);
				}
			}
		}

		public void addQuest(int questID)
		{
			if (hasQuest(questID))
			{
				return;
			}
			Quest quest = Quest.getQuestFromId(questID);
			if (quest != null)
			{
				questLog.Add(quest);
				if (!quest.isSecretQuest())
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2011"), 2));
				}
			}
		}

		public void removeQuest(int questID)
		{
			for (int i = questLog.Count - 1; i >= 0; i--)
			{
				if ((int)questLog[i].id == questID)
				{
					questLog.RemoveAt(i);
				}
			}
		}

		public void completeQuest(int questID)
		{
			for (int i = questLog.Count - 1; i >= 0; i--)
			{
				if ((int)questLog[i].id == questID)
				{
					questLog[i].questComplete();
				}
			}
		}

		public bool hasQuest(int id)
		{
			for (int i = questLog.Count - 1; i >= 0; i--)
			{
				if ((int)questLog[i].id == id)
				{
					return true;
				}
			}
			return false;
		}

		public bool hasNewQuestActivity()
		{
			foreach (Quest q in questLog)
			{
				if (!q.isSecretQuest() && ((bool)q.showNew || ((bool)q.completed && !q.destroy)))
				{
					return true;
				}
			}
			return false;
		}

		public float getMovementSpeed()
		{
			float movementSpeed = 1f;
			if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
			{
				movementMultiplier = 0.066f;
				movementSpeed = Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : ((float)base.addedSpeed + (isRidingHorse() ? 4.6f : temporarySpeedBuff)))) * movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds);
				if (movementDirections.Count > 1)
				{
					movementSpeed = 0.7f * movementSpeed;
				}
			}
			else
			{
				movementSpeed = Math.Max(1f, (float)base.speed + (Game1.eventUp ? ((float)Math.Max(0, Game1.CurrentEvent.farmerAddedSpeed - 2)) : ((float)base.addedSpeed + (isRidingHorse() ? 5f : temporarySpeedBuff))));
				if (movementDirections.Count > 1)
				{
					movementSpeed = Math.Max(1, (int)Math.Sqrt(2f * (movementSpeed * movementSpeed)) / 2);
				}
			}
			return movementSpeed;
		}

		public bool isWearingRing(int ringIndex)
		{
			if (rightRing.Value == null || (int)rightRing.Value.indexInTileSheet != ringIndex)
			{
				if (leftRing.Value != null)
				{
					return (int)leftRing.Value.indexInTileSheet == ringIndex;
				}
				return false;
			}
			return true;
		}

		public override void Halt()
		{
			if (!FarmerSprite.PauseForSingleAnimation && !isRidingHorse() && !UsingTool)
			{
				base.Halt();
			}
			movementDirections.Clear();
			if (!isEmoteAnimating && !UsingTool)
			{
				stopJittering();
			}
			armOffset = Vector2.Zero;
			if (isRidingHorse())
			{
				mount.Halt();
				mount.Sprite.CurrentAnimation = null;
			}
		}

		public void stopJittering()
		{
			jitterStrength = 0f;
			jitter = Vector2.Zero;
		}

		public override Microsoft.Xna.Framework.Rectangle nextPosition(int direction)
		{
			Microsoft.Xna.Framework.Rectangle nextPosition = GetBoundingBox();
			switch (direction)
			{
			case 0:
				nextPosition.Y -= (int)Math.Ceiling(getMovementSpeed());
				break;
			case 1:
				nextPosition.X += (int)Math.Ceiling(getMovementSpeed());
				break;
			case 2:
				nextPosition.Y += (int)Math.Ceiling(getMovementSpeed());
				break;
			case 3:
				nextPosition.X -= (int)Math.Ceiling(getMovementSpeed());
				break;
			}
			return nextPosition;
		}

		public Microsoft.Xna.Framework.Rectangle nextPositionHalf(int direction)
		{
			Microsoft.Xna.Framework.Rectangle nextPosition = GetBoundingBox();
			switch (direction)
			{
			case 0:
				nextPosition.Y -= (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
				break;
			case 1:
				nextPosition.X += (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
				break;
			case 2:
				nextPosition.Y += (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
				break;
			case 3:
				nextPosition.X -= (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
				break;
			}
			return nextPosition;
		}

		public int getProfessionForSkill(int skillType, int skillLevel)
		{
			switch (skillLevel)
			{
			case 5:
				switch (skillType)
				{
				case 0:
					if (professions.Contains(0))
					{
						return 0;
					}
					if (professions.Contains(1))
					{
						return 1;
					}
					break;
				case 1:
					if (professions.Contains(6))
					{
						return 6;
					}
					if (professions.Contains(7))
					{
						return 7;
					}
					break;
				case 2:
					if (professions.Contains(12))
					{
						return 12;
					}
					if (professions.Contains(13))
					{
						return 13;
					}
					break;
				case 3:
					if (professions.Contains(18))
					{
						return 18;
					}
					if (professions.Contains(19))
					{
						return 19;
					}
					break;
				case 4:
					if (professions.Contains(24))
					{
						return 24;
					}
					if (professions.Contains(25))
					{
						return 25;
					}
					break;
				}
				break;
			case 10:
				switch (skillType)
				{
				case 0:
					if (professions.Contains(1))
					{
						if (professions.Contains(4))
						{
							return 4;
						}
						if (professions.Contains(5))
						{
							return 5;
						}
					}
					else
					{
						if (professions.Contains(2))
						{
							return 2;
						}
						if (professions.Contains(3))
						{
							return 3;
						}
					}
					break;
				case 1:
					if (professions.Contains(6))
					{
						if (professions.Contains(8))
						{
							return 8;
						}
						if (professions.Contains(9))
						{
							return 9;
						}
					}
					else
					{
						if (professions.Contains(10))
						{
							return 10;
						}
						if (professions.Contains(11))
						{
							return 11;
						}
					}
					break;
				case 2:
					if (professions.Contains(12))
					{
						if (professions.Contains(14))
						{
							return 14;
						}
						if (professions.Contains(15))
						{
							return 15;
						}
					}
					else
					{
						if (professions.Contains(16))
						{
							return 16;
						}
						if (professions.Contains(17))
						{
							return 17;
						}
					}
					break;
				case 3:
					if (professions.Contains(18))
					{
						if (professions.Contains(20))
						{
							return 20;
						}
						if (professions.Contains(21))
						{
							return 21;
						}
					}
					else
					{
						if (professions.Contains(23))
						{
							return 23;
						}
						if (professions.Contains(22))
						{
							return 22;
						}
					}
					break;
				case 4:
					if (professions.Contains(24))
					{
						if (professions.Contains(26))
						{
							return 26;
						}
						if (professions.Contains(27))
						{
							return 27;
						}
					}
					else
					{
						if (professions.Contains(28))
						{
							return 28;
						}
						if (professions.Contains(29))
						{
							return 29;
						}
					}
					break;
				}
				break;
			}
			return -1;
		}

		public void behaviorOnMovement(int direction)
		{
		}

		public void OnEmoteAnimationEnd(Farmer farmer)
		{
			if (farmer == this && isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
		}

		public void EndEmoteAnimation()
		{
			if (isEmoteAnimating)
			{
				if (jitterStrength > 0f)
				{
					stopJittering();
				}
				if (yJumpOffset != 0)
				{
					yJumpOffset = 0;
					yJumpVelocity = 0f;
				}
				FarmerSprite.PauseForSingleAnimation = false;
				FarmerSprite.StopAnimation();
				isEmoteAnimating = false;
			}
		}

		private void broadcastHaltAnimation(Farmer who)
		{
			if (IsLocalPlayer)
			{
				haltAnimationEvent.Fire();
			}
			else
			{
				completelyStopAnimating(who);
			}
		}

		private void performHaltAnimation()
		{
			completelyStopAnimatingOrDoingAction();
		}

		public void performKissFarmer(long otherPlayerID)
		{
			Farmer spouse = Game1.getFarmer(otherPlayerID);
			if (spouse != null)
			{
				bool localPlayerOnLeft = getStandingX() < spouse.getStandingX();
				PerformKiss(localPlayerOnLeft ? 1 : 3);
				spouse.PerformKiss((!localPlayerOnLeft) ? 1 : 3);
			}
		}

		public void PerformKiss(int facingDirection)
		{
			if (!Game1.eventUp && !UsingTool && (!IsLocalPlayer || Game1.activeClickableMenu == null) && !isRidingHorse() && !base.IsEmoting && CanMove)
			{
				CanMove = false;
				FarmerSprite.PauseForSingleAnimation = false;
				faceDirection(facingDirection);
				FarmerSprite.animateOnce(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(101, 1000, 0, secondaryArm: false, base.FacingDirection == 3),
					new FarmerSprite.AnimationFrame(6, 1, secondaryArm: false, base.FacingDirection == 3, broadcastHaltAnimation)
				}.ToArray());
			}
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
			{
				if (Game1.shouldTimePass() && temporarilyInvincible)
				{
					temporaryInvincibilityTimer += time.ElapsedGameTime.Milliseconds;
					if (temporaryInvincibilityTimer > 1200)
					{
						temporarilyInvincible = false;
						temporaryInvincibilityTimer = 0;
					}
				}
			}
			else if (temporarilyInvincible)
			{
				temporarilyInvincible = false;
				temporaryInvincibilityTimer = 0;
			}
			if (Game1.activeClickableMenu != null && (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence))
			{
				return;
			}
			if (isRafting)
			{
				moveRaft(currentLocation, time);
				return;
			}
			if (xVelocity != 0f || yVelocity != 0f)
			{
				if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity))
				{
					xVelocity = 0f;
					yVelocity = 0f;
				}
				Microsoft.Xna.Framework.Rectangle nextPositionFloor = GetBoundingBox();
				nextPositionFloor.X += (int)Math.Floor(xVelocity);
				nextPositionFloor.Y -= (int)Math.Floor(yVelocity);
				Microsoft.Xna.Framework.Rectangle nextPositionCeil = GetBoundingBox();
				nextPositionCeil.X += (int)Math.Ceiling(xVelocity);
				nextPositionCeil.Y -= (int)Math.Ceiling(yVelocity);
				Microsoft.Xna.Framework.Rectangle nextPosition = Microsoft.Xna.Framework.Rectangle.Union(nextPositionFloor, nextPositionCeil);
				if (!currentLocation.isCollidingPosition(nextPosition, viewport, isFarmer: true, -1, glider: false, this))
				{
					position.X += xVelocity;
					position.Y -= yVelocity;
					xVelocity -= xVelocity / 16f;
					yVelocity -= yVelocity / 16f;
					if (Math.Abs(xVelocity) <= 0.05f)
					{
						xVelocity = 0f;
					}
					if (Math.Abs(yVelocity) <= 0.05f)
					{
						yVelocity = 0f;
					}
				}
				else
				{
					xVelocity -= xVelocity / 16f;
					yVelocity -= yVelocity / 16f;
					if (Math.Abs(xVelocity) <= 0.05f)
					{
						xVelocity = 0f;
					}
					if (Math.Abs(yVelocity) <= 0.05f)
					{
						yVelocity = 0f;
					}
				}
			}
			if (CanMove || Game1.eventUp || controller != null)
			{
				temporaryPassableTiles.ClearNonIntersecting(GetBoundingBox());
				float movementSpeed = getMovementSpeed();
				temporarySpeedBuff = 0f;
				if (movementDirections.Contains(0))
				{
					Warp warp = Game1.currentLocation.isCollidingWithWarp(this.nextPosition(0));
					if (warp != null && IsLocalPlayer)
					{
						warpFarmer(warp);
						return;
					}
					if (!currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: true, 0, glider: false, this) || ignoreCollisions)
					{
						position.Y -= movementSpeed;
						behaviorOnMovement(0);
					}
					else if (!currentLocation.isCollidingPosition(nextPositionHalf(0), viewport, isFarmer: true, 0, glider: false, this))
					{
						position.Y -= movementSpeed / 2f;
						behaviorOnMovement(0);
					}
					else if (movementDirections.Count == 1)
					{
						Microsoft.Xna.Framework.Rectangle tmp = this.nextPosition(0);
						tmp.Width /= 4;
						bool leftCorner = currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
						tmp.X += tmp.Width * 3;
						bool rightCorner = currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
						if (leftCorner && !rightCorner && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
						else if (rightCorner && !leftCorner && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
					}
				}
				if (movementDirections.Contains(2))
				{
					Warp warp4 = Game1.currentLocation.isCollidingWithWarp(this.nextPosition(2));
					if (warp4 != null && IsLocalPlayer)
					{
						warpFarmer(warp4);
						return;
					}
					if (!currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: true, 0, glider: false, this) || ignoreCollisions)
					{
						position.Y += movementSpeed;
						behaviorOnMovement(2);
					}
					else if (!currentLocation.isCollidingPosition(nextPositionHalf(2), viewport, isFarmer: true, 0, glider: false, this))
					{
						position.Y += movementSpeed / 2f;
						behaviorOnMovement(2);
					}
					else if (movementDirections.Count == 1)
					{
						Microsoft.Xna.Framework.Rectangle tmp2 = this.nextPosition(2);
						tmp2.Width /= 4;
						bool leftCorner2 = currentLocation.isCollidingPosition(tmp2, viewport, isFarmer: true, 0, glider: false, this);
						tmp2.X += tmp2.Width * 3;
						bool rightCorner2 = currentLocation.isCollidingPosition(tmp2, viewport, isFarmer: true, 0, glider: false, this);
						if (leftCorner2 && !rightCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
						else if (rightCorner2 && !leftCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
					}
				}
				if (movementDirections.Contains(1))
				{
					Warp warp3 = Game1.currentLocation.isCollidingWithWarp(this.nextPosition(1));
					if (warp3 != null && IsLocalPlayer)
					{
						warpFarmer(warp3);
						return;
					}
					if (!currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: true, 0, glider: false, this) || ignoreCollisions)
					{
						position.X += movementSpeed;
						behaviorOnMovement(1);
					}
					else if (!currentLocation.isCollidingPosition(nextPositionHalf(1), viewport, isFarmer: true, 0, glider: false, this))
					{
						position.X += movementSpeed / 2f;
						behaviorOnMovement(1);
					}
					else if (movementDirections.Count == 1)
					{
						Microsoft.Xna.Framework.Rectangle tmp3 = this.nextPosition(1);
						tmp3.Height /= 4;
						bool topCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, isFarmer: true, 0, glider: false, this);
						tmp3.Y += tmp3.Height * 3;
						bool bottomCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, isFarmer: true, 0, glider: false, this);
						if (topCorner2 && !bottomCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
						else if (bottomCorner2 && !topCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
					}
				}
				if (movementDirections.Contains(3))
				{
					Warp warp2 = Game1.currentLocation.isCollidingWithWarp(this.nextPosition(3));
					if (warp2 != null && IsLocalPlayer)
					{
						warpFarmer(warp2);
						return;
					}
					if (!currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: true, 0, glider: false, this) || ignoreCollisions)
					{
						position.X -= movementSpeed;
						behaviorOnMovement(3);
					}
					else if (!currentLocation.isCollidingPosition(nextPositionHalf(3), viewport, isFarmer: true, 0, glider: false, this))
					{
						position.X -= movementSpeed / 2f;
						behaviorOnMovement(3);
					}
					else if (movementDirections.Count == 1)
					{
						Microsoft.Xna.Framework.Rectangle tmp4 = this.nextPosition(3);
						tmp4.Height /= 4;
						bool topCorner = currentLocation.isCollidingPosition(tmp4, viewport, isFarmer: true, 0, glider: false, this);
						tmp4.Y += tmp4.Height * 3;
						bool bottomCorner = currentLocation.isCollidingPosition(tmp4, viewport, isFarmer: true, 0, glider: false, this);
						if (topCorner && !bottomCorner && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
						else if (bottomCorner && !topCorner && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
					}
				}
			}
			if (movementDirections.Count > 0 && !UsingTool)
			{
				FarmerSprite.intervalModifier = 1f - (running ? 0.03f : 0.025f) * (Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : ((float)base.addedSpeed + (isRidingHorse() ? 4.6f : 0f)))) * movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds) * 1.25f);
			}
			else
			{
				FarmerSprite.intervalModifier = 1f;
			}
			if (currentLocation != null && currentLocation.isFarmerCollidingWithAnyCharacter())
			{
				temporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle((int)getTileLocation().X * 64, (int)getTileLocation().Y * 64, 64, 64));
			}
		}

		public void updateMovementAnimation(GameTime time)
		{
			if (_emoteGracePeriod > 0)
			{
				_emoteGracePeriod -= time.ElapsedGameTime.Milliseconds;
			}
			if (isEmoteAnimating)
			{
				bool moving2 = false;
				moving2 = ((!IsLocalPlayer) ? position.Field.IsInterpolating() : (movementDirections.Count > 0));
				if ((moving2 && _emoteGracePeriod <= 0) || !FarmerSprite.PauseForSingleAnimation)
				{
					EndEmoteAnimation();
				}
			}
			bool carrying = IsCarrying();
			if (!isRidingHorse())
			{
				xOffset = 0f;
			}
			if (CurrentTool is FishingRod)
			{
				FishingRod rod = CurrentTool as FishingRod;
				if (rod.isTimingCast || rod.isCasting)
				{
					rod.setTimingCastAnimation(this);
					return;
				}
			}
			if (FarmerSprite.PauseForSingleAnimation || UsingTool)
			{
				return;
			}
			if (IsLocalPlayer && !CanMove && !Game1.eventUp)
			{
				if (isRidingHorse() && mount != null && !isAnimatingMount)
				{
					showRiding();
				}
				else if (carrying)
				{
					showCarrying();
				}
				return;
			}
			if (IsLocalPlayer || isFakeEventActor)
			{
				moveUp = movementDirections.Contains(0);
				moveRight = movementDirections.Contains(1);
				moveDown = movementDirections.Contains(2);
				moveLeft = movementDirections.Contains(3);
				if (moveUp || moveRight || moveDown)
				{
					_ = 1;
				}
				else
					_ = moveLeft;
				if (moveLeft)
				{
					base.FacingDirection = 3;
				}
				else if (moveRight)
				{
					base.FacingDirection = 1;
				}
				else if (moveUp)
				{
					base.FacingDirection = 0;
				}
				else if (moveDown)
				{
					base.FacingDirection = 2;
				}
				if (isRidingHorse() && !mount.dismounting)
				{
					base.speed = 2;
				}
			}
			else
			{
				moveLeft = (position.Field.IsInterpolating() && base.FacingDirection == 3);
				moveRight = (position.Field.IsInterpolating() && base.FacingDirection == 1);
				moveUp = (position.Field.IsInterpolating() && base.FacingDirection == 0);
				moveDown = (position.Field.IsInterpolating() && base.FacingDirection == 2);
				bool num = moveUp || moveRight || moveDown || moveLeft;
				float speed = position.CurrentInterpolationSpeed() / ((float)Game1.currentGameTime.ElapsedGameTime.Milliseconds * 0.066f);
				running = (Math.Abs(speed - 5f) < Math.Abs(speed - 2f) && !bathingClothes);
				if (!num)
				{
					FarmerSprite.StopAnimation();
				}
			}
			if (!FarmerSprite.PauseForSingleAnimation && !UsingTool)
			{
				if (isRidingHorse() && !mount.dismounting)
				{
					showRiding();
				}
				else if (moveLeft && running && !carrying)
				{
					FarmerSprite.animate(56, time);
				}
				else if (moveRight && running && !carrying)
				{
					FarmerSprite.animate(40, time);
				}
				else if (moveUp && running && !carrying)
				{
					FarmerSprite.animate(48, time);
				}
				else if (moveDown && running && !carrying)
				{
					FarmerSprite.animate(32, time);
				}
				else if (moveLeft && running)
				{
					FarmerSprite.animate(152, time);
				}
				else if (moveRight && running)
				{
					FarmerSprite.animate(136, time);
				}
				else if (moveUp && running)
				{
					FarmerSprite.animate(144, time);
				}
				else if (moveDown && running)
				{
					FarmerSprite.animate(128, time);
				}
				else if (moveLeft && !carrying)
				{
					FarmerSprite.animate(24, time);
				}
				else if (moveRight && !carrying)
				{
					FarmerSprite.animate(8, time);
				}
				else if (moveUp && !carrying)
				{
					FarmerSprite.animate(16, time);
				}
				else if (moveDown && !carrying)
				{
					FarmerSprite.animate(0, time);
				}
				else if (moveLeft)
				{
					FarmerSprite.animate(120, time);
				}
				else if (moveRight)
				{
					FarmerSprite.animate(104, time);
				}
				else if (moveUp)
				{
					FarmerSprite.animate(112, time);
				}
				else if (moveDown)
				{
					FarmerSprite.animate(96, time);
				}
				else if (carrying)
				{
					showCarrying();
				}
				else
				{
					showNotCarrying();
				}
			}
		}

		public bool IsCarrying()
		{
			if (mount != null || isAnimatingMount)
			{
				return false;
			}
			if (ActiveObject == null || Game1.eventUp || Game1.killScreen)
			{
				return false;
			}
			if (ActiveObject is Furniture)
			{
				return false;
			}
			return true;
		}

		public void doneEating()
		{
			isEating = false;
			completelyStopAnimatingOrDoingAction();
			forceCanMove();
			if (mostRecentlyGrabbedItem == null)
			{
				return;
			}
			Object consumed = itemToEat as Object;
			if (IsLocalPlayer && consumed.ParentSheetIndex == 434)
			{
				if (Utility.foundAllStardrops())
				{
					Game1.getSteamAchievement("Achievement_Stardrop");
				}
				yOffset = 0f;
				yJumpOffset = 0;
				Game1.changeMusicTrack("none");
				Game1.playSound("stardrop");
				string mid = (Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3094") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3095");
				DelayedAction.showDialogueAfterDelay(string.Concat(str1: favoriteThing.Contains("Stardew") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3097") : ((!favoriteThing.Equals("ConcernedApe")) ? (mid + favoriteThing) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3099")), str0: Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3100"), str2: Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3101")), 6000);
				MaxStamina += 34;
				Stamina = MaxStamina;
				FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
				{
					new FarmerSprite.AnimationFrame(57, 6000)
				});
				startGlowing(new Color(200, 0, 255), border: false, 0.1f);
				jitterStrength = 1f;
				Game1.staminaShakeTimer = 12000;
				Game1.screenGlowOnce(new Color(200, 0, 255), hold: true);
				CanMove = false;
				freezePause = 8000;
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 60f, 8, 40, base.Position + new Vector2(-8f, -128f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0.0075f, 0f, 0f)
				{
					alpha = 0.75f,
					alphaFade = 0.0025f,
					motion = new Vector2(0f, -0.25f)
				});
				if (Game1.displayHUD && !Game1.eventUp)
				{
					for (int i = 0; i < 40; i++)
					{
						Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite(Game1.random.Next(10, 12), new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right / Game1.options.zoomLevel - 48f - 8f - (float)Game1.random.Next(64), (float)Game1.random.Next(-64, 64) + (float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom / Game1.options.zoomLevel - 224f - 16f - (float)(int)((double)(Game1.player.MaxStamina - 270) * 0.715)), (Game1.random.NextDouble() < 0.5) ? Color.White : Color.Lime, 8, flipped: false, 50f)
						{
							layerDepth = 1f,
							delayBeforeAnimationStart = 200 * i,
							interval = 100f,
							local = true
						});
					}
				}
				Utility.addSprinklesToLocation(base.currentLocation, getTileX(), getTileY(), 9, 9, 6000, 100, new Color(200, 0, 255), null, motionTowardCenter: true);
				DelayedAction.stopFarmerGlowing(6000);
				Utility.addSprinklesToLocation(base.currentLocation, getTileX(), getTileY(), 9, 9, 6000, 300, Color.Cyan, null, motionTowardCenter: true);
				mostRecentlyGrabbedItem = null;
			}
			else if (IsLocalPlayer)
			{
				string[] objectDescription = Game1.objectInformation[consumed.ParentSheetIndex].Split('/');
				if (Convert.ToInt32(objectDescription[2]) > 0)
				{
					string[] whatToBuff = (objectDescription.Length > 7) ? objectDescription[7].Split(' ') : new string[12]
					{
						"0",
						"0",
						"0",
						"0",
						"0",
						"0",
						"0",
						"0",
						"0",
						"0",
						"0",
						"0"
					};
					if (objectDescription.Length > 6 && objectDescription[6].Equals("drink"))
					{
						if (!Game1.buffsDisplay.tryToAddDrinkBuff(new Buff(Convert.ToInt32(whatToBuff[0]), Convert.ToInt32(whatToBuff[1]), Convert.ToInt32(whatToBuff[2]), Convert.ToInt32(whatToBuff[3]), Convert.ToInt32(whatToBuff[4]), Convert.ToInt32(whatToBuff[5]), Convert.ToInt32(whatToBuff[6]), Convert.ToInt32(whatToBuff[7]), Convert.ToInt32(whatToBuff[8]), Convert.ToInt32(whatToBuff[9]), Convert.ToInt32(whatToBuff[10]), (whatToBuff.Length > 10) ? Convert.ToInt32(whatToBuff[10]) : 0, (objectDescription.Length > 8) ? Convert.ToInt32(objectDescription[8]) : (-1), objectDescription[0], objectDescription[4])))
						{
						}
					}
					else if (Convert.ToInt32(objectDescription[2]) > 0)
					{
						Game1.buffsDisplay.tryToAddFoodBuff(new Buff(Convert.ToInt32(whatToBuff[0]), Convert.ToInt32(whatToBuff[1]), Convert.ToInt32(whatToBuff[2]), Convert.ToInt32(whatToBuff[3]), Convert.ToInt32(whatToBuff[4]), Convert.ToInt32(whatToBuff[5]), Convert.ToInt32(whatToBuff[6]), Convert.ToInt32(whatToBuff[7]), Convert.ToInt32(whatToBuff[8]), Convert.ToInt32(whatToBuff[9]), Convert.ToInt32(whatToBuff[10]), (whatToBuff.Length > 11) ? Convert.ToInt32(whatToBuff[11]) : 0, (objectDescription.Length > 8) ? Convert.ToInt32(objectDescription[8]) : (-1), objectDescription[0], objectDescription[4]), Math.Min(120000, (int)((float)Convert.ToInt32(objectDescription[2]) / 20f * 30000f)));
					}
				}
				float oldStam = Stamina;
				int oldHealth = health;
				int staminaToHeal = consumed.staminaRecoveredOnConsumption();
				Stamina = Math.Min(MaxStamina, Stamina + (float)staminaToHeal);
				health = Math.Min(maxHealth, health + consumed.healthRecoveredOnConsumption());
				if (oldStam < Stamina)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3116", (int)(Stamina - oldStam)), 4));
				}
				if (oldHealth < health)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3118", health - oldHealth), 5));
				}
			}
			if (consumed != null && consumed.Edibility < 0 && IsLocalPlayer)
			{
				CanMove = false;
				sickAnimationEvent.Fire();
			}
		}

		public bool checkForQuestComplete(NPC n, int number1, int number2, Item item, string str, int questType = -1, int questTypeToIgnore = -1)
		{
			bool worked = false;
			for (int i = questLog.Count - 1; i >= 0; i--)
			{
				if (questLog[i] != null && (questType == -1 || (int)questLog[i].questType == questType) && (questTypeToIgnore == -1 || (int)questLog[i].questType != questTypeToIgnore) && questLog[i].checkIfComplete(n, number1, number2, item, str))
				{
					worked = true;
				}
			}
			return worked;
		}

		public static void completelyStopAnimating(Farmer who)
		{
			who.completelyStopAnimatingOrDoingAction();
		}

		public void completelyStopAnimatingOrDoingAction()
		{
			CanMove = !Game1.eventUp;
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (UsingTool)
			{
				EndUsingTool();
				if (CurrentTool is FishingRod)
				{
					(CurrentTool as FishingRod).resetState();
				}
			}
			if (usingSlingshot && CurrentTool is Slingshot)
			{
				(CurrentTool as Slingshot).finish();
			}
			UsingTool = false;
			isEating = false;
			FarmerSprite.PauseForSingleAnimation = false;
			usingSlingshot = false;
			canReleaseTool = false;
			Halt();
			Sprite.StopAnimation();
			if (CurrentTool is MeleeWeapon)
			{
				(CurrentTool as MeleeWeapon).isOnSpecial = false;
			}
			stopJittering();
		}

		public void doEmote(int whichEmote)
		{
			if (!isEmoting)
			{
				isEmoting = true;
				currentEmote = whichEmote;
				currentEmoteFrame = 0;
				emoteInterval = 0f;
			}
		}

		public void performTenMinuteUpdate()
		{
			if (base.addedSpeed > 0 && Game1.buffsDisplay.otherBuffs.Count == 0 && Game1.buffsDisplay.food == null && Game1.buffsDisplay.drink == null)
			{
				base.addedSpeed = 0;
			}
		}

		public void setRunning(bool isRunning, bool force = false)
		{
			if (canOnlyWalk || ((bool)bathingClothes && !running) || (Game1.CurrentEvent != null && isRunning && !Game1.CurrentEvent.isFestival && !Game1.CurrentEvent.playerControlSequence))
			{
				return;
			}
			if (isRidingHorse())
			{
				running = true;
			}
			else if (stamina <= 0f)
			{
				base.speed = 2;
				if (running)
				{
					Halt();
				}
				running = false;
			}
			else if (force || (CanMove && !isEating && (Game1.currentLocation.currentEvent == null || Game1.currentLocation.currentEvent.playerControlSequence) && (isRunning || !UsingTool) && (isRunning || !Game1.pickingTool) && (Sprite == null || !((FarmerSprite)Sprite).PauseForSingleAnimation)))
			{
				running = isRunning;
				if (running)
				{
					base.speed = 5;
				}
				else
				{
					base.speed = 2;
				}
			}
			else if (UsingTool)
			{
				running = isRunning;
				if (running)
				{
					base.speed = 5;
				}
				else
				{
					base.speed = 2;
				}
			}
		}

		public void addSeenResponse(int id)
		{
			dialogueQuestionsAnswered.Add(id);
		}

		public void eatObject(Object o, bool overrideFullness = false)
		{
			if (o.ParentSheetIndex == 434)
			{
				Game1.changeMusicTrack("none");
				Game1.multiplayer.globalChatInfoMessage("Stardrop", base.Name);
			}
			if (getFacingDirection() != 2)
			{
				faceDirection(2);
			}
			itemToEat = o;
			mostRecentlyGrabbedItem = o;
			string[] objectDescription = Game1.objectInformation[o.ParentSheetIndex].Split('/');
			forceCanMove();
			completelyStopAnimatingOrDoingAction();
			if (objectDescription.Length > 6 && objectDescription[6].Equals("drink"))
			{
				if (IsLocalPlayer && Game1.buffsDisplay.hasBuff(7) && !overrideFullness)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2898"), Color.OrangeRed, 3500f));
					return;
				}
				drinkAnimationEvent.Fire(o.getOne() as Object);
			}
			else if (Convert.ToInt32(objectDescription[2]) != -300)
			{
				if (Game1.buffsDisplay.hasBuff(6) && !overrideFullness)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2899"), Color.OrangeRed, 3500f));
					return;
				}
				eatAnimationEvent.Fire(o.getOne() as Object);
			}
			freezePause = 20000;
			CanMove = false;
			isEating = true;
		}

		private void performDrinkAnimation(Object item)
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (!IsLocalPlayer)
			{
				itemToEat = item;
			}
			FarmerSprite.animateOnce(294, 80f, 8);
			isEating = true;
		}

		public Farmer CreateFakeEventFarmer()
		{
			Farmer farmer = new Farmer(new FarmerSprite(FarmerSprite.textureName.Value), new Vector2(192f, 192f), 1, "", new List<Item>(), IsMale);
			farmer.Name = base.Name;
			farmer.displayName = base.displayName;
			farmer.isFakeEventActor = true;
			farmer.changeGender(IsMale);
			farmer.changeHairStyle(hair);
			farmer.UniqueMultiplayerID = UniqueMultiplayerID;
			farmer.shirtItem.Set(shirtItem);
			farmer.pantsItem.Set(pantsItem);
			farmer.shirt.Set(shirt.Value);
			farmer.pants.Set(pants.Value);
			farmer.changeShoeColor(shoes.Value);
			farmer.boots.Set(boots.Value);
			farmer.leftRing.Set(leftRing.Value);
			farmer.rightRing.Set(rightRing.Value);
			farmer.hat.Set(hat.Value);
			farmer.shirtColor = shirtColor;
			farmer.pantsColor.Set(pantsColor.Value);
			farmer.changeHairColor(hairstyleColor);
			farmer.changeSkinColor(skin.Value);
			farmer.accessory.Set(accessory.Value);
			farmer.changeEyeColor(newEyeColor.Value);
			farmer.UpdateClothing();
			return farmer;
		}

		private void performEatAnimation(Object item)
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (!IsLocalPlayer)
			{
				itemToEat = item;
			}
			FarmerSprite.animateOnce(216, 80f, 8);
			isEating = true;
		}

		public void netDoEmote(string emote_type)
		{
			doEmoteEvent.Fire(emote_type);
		}

		private void performSickAnimation()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			isEating = false;
			FarmerSprite.animateOnce(224, 350f, 4);
			doEmote(12);
		}

		public void eatHeldObject()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (!Game1.fadeToBlack)
			{
				if (ActiveObject == null)
				{
					ActiveObject = (Object)mostRecentlyGrabbedItem;
				}
				eatObject(ActiveObject);
				if (isEating)
				{
					reduceActiveItemByOne();
					CanMove = false;
				}
			}
		}

		public void grabObject(Object obj)
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (obj != null)
			{
				CanMove = false;
				switch (base.FacingDirection)
				{
				case 2:
					((FarmerSprite)Sprite).animateOnce(64, 50f, 8);
					break;
				case 1:
					((FarmerSprite)Sprite).animateOnce(72, 50f, 8);
					break;
				case 0:
					((FarmerSprite)Sprite).animateOnce(80, 50f, 8);
					break;
				case 3:
					((FarmerSprite)Sprite).animateOnce(88, 50f, 8);
					break;
				}
				Game1.playSound("pickUpItem");
			}
		}

		public string getTitle()
		{
			int level = Level;
			if (level >= 30)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2016");
			}
			if (level > 28)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2017");
			}
			if (level > 26)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2018");
			}
			if (level > 24)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2019");
			}
			if (level > 22)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2020");
			}
			if (level > 20)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2021");
			}
			if (level > 18)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2022");
			}
			if (level > 16)
			{
				if (!IsMale)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2024");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2023");
			}
			if (level > 14)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2025");
			}
			if (level > 12)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2026");
			}
			if (level > 10)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2027");
			}
			if (level > 8)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2028");
			}
			if (level > 6)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2029");
			}
			if (level > 4)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2030");
			}
			if (level > 2)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2031");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2032");
		}

		public void queueMessage(byte messageType, Farmer sourceFarmer, params object[] data)
		{
			queueMessage(new OutgoingMessage(messageType, sourceFarmer, data));
		}

		public void queueMessage(OutgoingMessage message)
		{
			messageQueue.Add(message);
		}
	}
}
