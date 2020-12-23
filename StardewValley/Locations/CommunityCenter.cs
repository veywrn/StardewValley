using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class CommunityCenter : GameLocation
	{
		public const int AREA_Pantry = 0;

		public const int AREA_FishTank = 2;

		public const int AREA_CraftsRoom = 1;

		public const int AREA_BoilerRoom = 3;

		public const int AREA_Vault = 4;

		public const int AREA_Bulletin = 5;

		public const int AREA_AbandonedJojaMart = 6;

		public const int AREA_Bulletin2 = 7;

		public const int AREA_JunimoHut = 8;

		[XmlElement("warehouse")]
		private readonly NetBool warehouse = new NetBool();

		[XmlIgnore]
		public List<NetMutex> bundleMutexes = new List<NetMutex>();

		public readonly NetArray<bool, NetBool> areasComplete = new NetArray<bool, NetBool>(6);

		[XmlElement("numberOfStarsOnPlaque")]
		public readonly NetInt numberOfStarsOnPlaque = new NetInt();

		[XmlIgnore]
		private readonly NetEvent0 newJunimoNoteCheckEvent = new NetEvent0();

		[XmlIgnore]
		private readonly NetEvent1Field<int, NetInt> restoreAreaCutsceneEvent = new NetEvent1Field<int, NetInt>();

		[XmlIgnore]
		private readonly NetEvent1Field<int, NetInt> areaCompleteRewardEvent = new NetEvent1Field<int, NetInt>();

		private float messageAlpha;

		private List<int> junimoNotesViewportTargets;

		private Dictionary<int, List<int>> areaToBundleDictionary;

		private Dictionary<int, int> bundleToAreaDictionary;

		private Dictionary<int, List<List<int>>> bundlesIngredientsInfo;

		private bool _isWatchingJunimoGoodbye;

		private Vector2 missedRewardsChestTile = new Vector2(22f, 10f);

		private int missedRewardsTileSheet = 1;

		[XmlIgnore]
		public readonly NetRef<Chest> missedRewardsChest = new NetRef<Chest>(new Chest(playerChest: true));

		[XmlIgnore]
		public readonly NetBool missedRewardsChestVisible = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetEvent1Field<bool, NetBool> showMissedRewardsChestEvent = new NetEvent1Field<bool, NetBool>();

		public const int PHASE_firstPause = 0;

		public const int PHASE_junimoAppear = 1;

		public const int PHASE_junimoDance = 2;

		public const int PHASE_restore = 3;

		private int restoreAreaTimer;

		private int restoreAreaPhase;

		private int restoreAreaIndex;

		private ICue buildUpSound;

		[XmlElement("bundles")]
		public NetBundles bundles => Game1.netWorldState.Value.Bundles;

		[XmlElement("bundleRewards")]
		public NetIntDictionary<bool, NetBool> bundleRewards => Game1.netWorldState.Value.BundleRewards;

		public CommunityCenter()
		{
			initAreaBundleConversions();
			refreshBundlesIngredientsInfo();
		}

		public CommunityCenter(string name)
			: base("Maps\\CommunityCenter_Ruins", name)
		{
			initAreaBundleConversions();
			refreshBundlesIngredientsInfo();
		}

		public void refreshBundlesIngredientsInfo()
		{
			bundlesIngredientsInfo = new Dictionary<int, List<List<int>>>();
			Dictionary<string, string> bundleData = Game1.netWorldState.Value.BundleData;
			Dictionary<int, bool[]> bundlesD = bundlesDict();
			foreach (KeyValuePair<string, string> v in bundleData)
			{
				int bundleIndex = Convert.ToInt32(v.Key.Split('/')[1]);
				string areaName = v.Key.Split('/')[0];
				new List<int>();
				string[] ingredientSplit = v.Value.Split('/')[2].Split(' ');
				if (shouldNoteAppearInArea(getAreaNumberFromName(areaName)))
				{
					for (int i = 0; i < ingredientSplit.Length; i += 3)
					{
						if (bundlesD.ContainsKey(bundleIndex) && !bundlesD[bundleIndex][i / 3])
						{
							int itemIndex = Convert.ToInt32(ingredientSplit[i]);
							int itemStack = Convert.ToInt32(ingredientSplit[i + 1]);
							int itemQuality = Convert.ToInt32(ingredientSplit[i + 2]);
							if (!bundlesIngredientsInfo.ContainsKey(itemIndex))
							{
								bundlesIngredientsInfo.Add(itemIndex, new List<List<int>>());
							}
							bundlesIngredientsInfo[itemIndex].Add(new List<int>
							{
								bundleIndex,
								itemStack,
								itemQuality
							});
						}
					}
				}
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(warehouse, areasComplete, numberOfStarsOnPlaque, newJunimoNoteCheckEvent, restoreAreaCutsceneEvent, areaCompleteRewardEvent, missedRewardsChest, showMissedRewardsChestEvent, missedRewardsChestVisible);
			newJunimoNoteCheckEvent.onEvent += doCheckForNewJunimoNotes;
			restoreAreaCutsceneEvent.onEvent += doRestoreAreaCutscene;
			areaCompleteRewardEvent.onEvent += doAreaCompleteReward;
			showMissedRewardsChestEvent.onEvent += doShowMissedRewardsChest;
		}

		private void initAreaBundleConversions()
		{
			areaToBundleDictionary = new Dictionary<int, List<int>>();
			bundleToAreaDictionary = new Dictionary<int, int>();
			for (int j = 0; j < 7; j++)
			{
				areaToBundleDictionary.Add(j, new List<int>());
				NetMutex i = new NetMutex();
				bundleMutexes.Add(i);
				base.NetFields.AddField(i.NetFields);
			}
			foreach (KeyValuePair<string, string> v in Game1.netWorldState.Value.BundleData)
			{
				int bundleIndex = Convert.ToInt32(v.Key.Split('/')[1]);
				areaToBundleDictionary[getAreaNumberFromName(v.Key.Split('/')[0])].Add(bundleIndex);
				bundleToAreaDictionary.Add(bundleIndex, getAreaNumberFromName(v.Key.Split('/')[0]));
			}
		}

		public static int getAreaNumberFromName(string name)
		{
			switch (name)
			{
			case "Pantry":
				return 0;
			case "Crafts Room":
			case "CraftsRoom":
				return 1;
			case "Fish Tank":
			case "FishTank":
				return 2;
			case "Boiler Room":
			case "BoilerRoom":
				return 3;
			case "Vault":
				return 4;
			case "BulletinBoard":
			case "Bulletin Board":
			case "Bulletin":
				return 5;
			case "Abandoned Joja Mart":
				return 6;
			default:
				return -1;
			}
		}

		private Point getNotePosition(int area)
		{
			switch (area)
			{
			case 0:
				return new Point(14, 5);
			case 2:
				return new Point(40, 10);
			case 1:
				return new Point(14, 23);
			case 3:
				return new Point(63, 14);
			case 4:
				return new Point(55, 6);
			case 5:
				return new Point(46, 11);
			default:
				return Point.Zero;
			}
		}

		public void addJunimoNote(int area)
		{
			Point position = getNotePosition(area);
			if (!position.Equals(Vector2.Zero))
			{
				StaticTile[] tileFrames = getJunimoNoteTileFrames(area, map);
				string layer = (area == 5) ? "Front" : "Buildings";
				map.GetLayer(layer).Tiles[position.X, position.Y] = new AnimatedTile(map.GetLayer(layer), tileFrames, 70L);
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
			}
		}

		public int numberOfCompleteBundles()
		{
			int number = 0;
			foreach (KeyValuePair<int, bool[]> v in bundles.Pairs)
			{
				number++;
				for (int i = 0; i < v.Value.Length; i++)
				{
					if (!v.Value[i])
					{
						number--;
						break;
					}
				}
			}
			return number;
		}

		public void addStarToPlaque()
		{
			numberOfStarsOnPlaque.Value++;
		}

		private string getMessageForAreaCompletion()
		{
			int areasComplete = getNumberOfAreasComplete();
			if (areasComplete >= 1 && areasComplete <= 6)
			{
				return Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaCompletion" + areasComplete, Game1.player.Name);
			}
			return "";
		}

		private int getNumberOfAreasComplete()
		{
			int complete = 0;
			for (int i = 0; i < areasComplete.Count; i++)
			{
				if (areasComplete[i])
				{
					complete++;
				}
			}
			return complete;
		}

		public Dictionary<int, bool[]> bundlesDict()
		{
			return bundles.Pairs.Select((KeyValuePair<int, bool[]> kvp) => new KeyValuePair<int, bool[]>(kvp.Key, kvp.Value.ToArray())).ToDictionary((KeyValuePair<int, bool[]> x) => x.Key, (KeyValuePair<int, bool[]> y) => y.Value);
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string a = action.Split(' ')[0];
				if (a == "MissedRewards")
				{
					missedRewardsChest.Value.mutex.RequestLock(delegate
					{
						new List<Chest>().Add(missedRewardsChest);
						Game1.activeClickableMenu = new ItemGrabMenu(missedRewardsChest.Value.items, reverseGrab: false, showReceivingMenu: true, null, null, null, rewardGrabbed, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
						Game1.activeClickableMenu.exitFunction = delegate
						{
							missedRewardsChest.Value.mutex.ReleaseLock();
							checkForMissedRewards();
						};
					});
					return true;
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		private void rewardGrabbed(Item item, Farmer who)
		{
			bundleRewards[item.SpecialVariable] = false;
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			switch ((map.GetLayer("Buildings").Tiles[tileLocation] != null) ? map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : (-1))
			{
			case 1799:
				if (numberOfCompleteBundles() > 2)
				{
					checkBundle(5);
				}
				break;
			case 1824:
			case 1825:
			case 1826:
			case 1827:
			case 1828:
			case 1829:
			case 1830:
			case 1831:
			case 1832:
			case 1833:
				checkBundle(getAreaNumberFromLocation(who.getTileLocation()));
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public void checkBundle(int area)
		{
			bundleMutexes[area].RequestLock(delegate
			{
				Game1.activeClickableMenu = new JunimoNoteMenu(area, bundlesDict());
			});
		}

		public void addJunimoNoteViewportTarget(int area)
		{
			if (junimoNotesViewportTargets == null)
			{
				junimoNotesViewportTargets = new List<int>();
			}
			if (!junimoNotesViewportTargets.Contains(area))
			{
				junimoNotesViewportTargets.Add(area);
			}
		}

		public void checkForNewJunimoNotes()
		{
			newJunimoNoteCheckEvent.Fire();
		}

		private void doCheckForNewJunimoNotes()
		{
			if (Game1.currentLocation != this)
			{
				return;
			}
			for (int i = 0; i < areasComplete.Count; i++)
			{
				if (!isJunimoNoteAtArea(i) && shouldNoteAppearInArea(i))
				{
					addJunimoNoteViewportTarget(i);
				}
			}
		}

		public void removeJunimoNote(int area)
		{
			Point p = getNotePosition(area);
			if (area == 5)
			{
				map.GetLayer("Front").Tiles[p.X, p.Y] = null;
			}
			else
			{
				map.GetLayer("Buildings").Tiles[p.X, p.Y] = null;
			}
		}

		public bool isJunimoNoteAtArea(int area)
		{
			Point p = getNotePosition(area);
			if (area == 5)
			{
				return map.GetLayer("Front").Tiles[p.X, p.Y] != null;
			}
			return map.GetLayer("Buildings").Tiles[p.X, p.Y] != null;
		}

		public bool shouldNoteAppearInArea(int area)
		{
			bool isAreaComplete = true;
			for (int j = 0; j < areaToBundleDictionary[area].Count; j++)
			{
				foreach (int bundleIndex in areaToBundleDictionary[area])
				{
					if (bundles.ContainsKey(bundleIndex))
					{
						int bundleLength = bundles[bundleIndex].Length / 3;
						for (int i = 0; i < bundleLength; i++)
						{
							if (!bundles[bundleIndex][i])
							{
								isAreaComplete = false;
								break;
							}
						}
					}
					if (!isAreaComplete)
					{
						break;
					}
				}
			}
			if (area >= 0 && !isAreaComplete)
			{
				switch (area)
				{
				case 1:
					return true;
				case 0:
				case 2:
					if (numberOfCompleteBundles() > 0)
					{
						return true;
					}
					break;
				case 3:
					if (numberOfCompleteBundles() > 1)
					{
						return true;
					}
					break;
				case 5:
					if (numberOfCompleteBundles() > 2)
					{
						return true;
					}
					break;
				case 4:
					if (numberOfCompleteBundles() > 3)
					{
						return true;
					}
					break;
				case 6:
					if (Utility.HasAnyPlayerSeenEvent(191393))
					{
						return true;
					}
					break;
				}
			}
			return false;
		}

		public override void updateMap()
		{
			if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
			{
				warehouse.Value = true;
				mapPath.Value = "Maps\\CommunityCenter_Joja";
			}
			base.updateMap();
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (areAllAreasComplete())
			{
				mapPath.Value = "Maps\\CommunityCenter_Refurbished";
				updateMap();
			}
			base.TransferDataFromSavedLocation(l);
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (areAllAreasComplete())
			{
				mapPath.Value = "Maps\\CommunityCenter_Refurbished";
			}
			_isWatchingJunimoGoodbye = false;
			if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember") && !areAllAreasComplete())
			{
				for (int i = 0; i < areasComplete.Count; i++)
				{
					if (shouldNoteAppearInArea(i))
					{
						characters.Add(new Junimo(new Vector2(getNotePosition(i).X, getNotePosition(i).Y + 2) * 64f, i));
					}
				}
			}
			numberOfStarsOnPlaque.Value = 0;
			for (int j = 0; j < areasComplete.Count; j++)
			{
				if (areasComplete[j])
				{
					numberOfStarsOnPlaque.Value++;
				}
			}
			checkForMissedRewards();
		}

		private void doShowMissedRewardsChest(bool isVisible)
		{
			int tileX = (int)missedRewardsChestTile.X;
			int tileY = (int)missedRewardsChestTile.Y;
			if (isVisible)
			{
				setMapTileIndex(tileX, tileY, -1, "Buildings");
				setMapTileIndex(tileX, tileY, 5, "Buildings", missedRewardsTileSheet);
				setTileProperty(tileX, tileY, "Buildings", "Action", "MissedRewards");
			}
			else
			{
				setMapTileIndex(tileX, tileY, -1, "Buildings");
				removeTileProperty(tileX, tileY, "Buildings", "Action");
			}
		}

		private void checkForMissedRewards()
		{
			HashSet<int> visited_areas = new HashSet<int>();
			bool hasUnclaimedRewards = false;
			missedRewardsChest.Value.items.Clear();
			List<Item> rewards = new List<Item>();
			foreach (int key in bundleRewards.Keys)
			{
				int area = bundleToAreaDictionary[key];
				if (bundleRewards[key] && areasComplete.Count() > area && areasComplete[area] && !visited_areas.Contains(area))
				{
					visited_areas.Add(area);
					hasUnclaimedRewards = true;
					rewards.Clear();
					JunimoNoteMenu.GetBundleRewards(area, rewards);
					foreach (Item item in rewards)
					{
						missedRewardsChest.Value.addItem(item);
					}
				}
			}
			if (hasUnclaimedRewards && !missedRewardsChestVisible)
			{
				showMissedRewardsChestEvent.Fire(arg: true);
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 5 : 46, missedRewardsChestTile * 64f + new Vector2(16f, 16f), Color.White)
				{
					layerDepth = 1f
				});
			}
			else if (!hasUnclaimedRewards && (bool)missedRewardsChestVisible)
			{
				showMissedRewardsChestEvent.Fire(arg: false);
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 5 : 46, missedRewardsChestTile * 64f + new Vector2(16f, 16f), Color.White)
				{
					layerDepth = 1f
				});
			}
			missedRewardsChestVisible.Value = hasUnclaimedRewards;
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!Game1.MasterPlayer.mailReceived.Contains("JojaMember") && !areAllAreasComplete())
			{
				for (int i = 0; i < areasComplete.Count; i++)
				{
					if (shouldNoteAppearInArea(i))
					{
						addJunimoNote(i);
					}
					else if (areasComplete[i])
					{
						loadArea(i, showEffects: false);
					}
				}
			}
			if (!Game1.eventUp && !areAllAreasComplete())
			{
				Game1.changeMusicTrack("communityCenter");
			}
			doShowMissedRewardsChest(missedRewardsChestVisible);
		}

		private int getAreaNumberFromLocation(Vector2 tileLocation)
		{
			for (int i = 0; i < areasComplete.Count; i++)
			{
				if (getAreaBounds(i).Contains((int)tileLocation.X, (int)tileLocation.Y))
				{
					return i;
				}
			}
			return -1;
		}

		private Microsoft.Xna.Framework.Rectangle getAreaBounds(int area)
		{
			switch (area)
			{
			case 1:
				return new Microsoft.Xna.Framework.Rectangle(0, 12, 21, 17);
			case 0:
				return new Microsoft.Xna.Framework.Rectangle(0, 0, 22, 11);
			case 2:
				return new Microsoft.Xna.Framework.Rectangle(35, 4, 9, 9);
			case 3:
				return new Microsoft.Xna.Framework.Rectangle(52, 9, 16, 12);
			case 5:
				return new Microsoft.Xna.Framework.Rectangle(22, 13, 28, 9);
			case 4:
				return new Microsoft.Xna.Framework.Rectangle(45, 0, 15, 9);
			case 7:
				return new Microsoft.Xna.Framework.Rectangle(44, 10, 6, 3);
			case 8:
				return new Microsoft.Xna.Framework.Rectangle(22, 4, 13, 9);
			default:
				return Microsoft.Xna.Framework.Rectangle.Empty;
			}
		}

		protected void removeJunimo()
		{
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				if (characters[i] is Junimo)
				{
					characters.RemoveAt(i);
				}
			}
		}

		public override void cleanupBeforeSave()
		{
			removeJunimo();
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (farmers.Count <= 1)
			{
				removeJunimo();
			}
			Game1.changeMusicTrack("none");
		}

		public bool isBundleComplete(int bundleIndex)
		{
			for (int i = 0; i < bundles[bundleIndex].Length; i++)
			{
				if (!bundles[bundleIndex][i])
				{
					return false;
				}
			}
			return true;
		}

		public bool couldThisIngredienteBeUsedInABundle(Object o)
		{
			if (!o.bigCraftable)
			{
				if (bundlesIngredientsInfo.ContainsKey(o.parentSheetIndex))
				{
					foreach (List<int> j in bundlesIngredientsInfo[o.parentSheetIndex])
					{
						if (o.Quality >= j[2])
						{
							return true;
						}
					}
				}
				if (o.Category < 0 && bundlesIngredientsInfo.ContainsKey(o.Category))
				{
					foreach (List<int> i in bundlesIngredientsInfo[o.Category])
					{
						if (o.Quality >= i[2])
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public void areaCompleteReward(int whichArea)
		{
			areaCompleteRewardEvent.Fire(whichArea);
		}

		private void doAreaCompleteReward(int whichArea)
		{
			string mailReceivedID = "";
			switch (whichArea)
			{
			case 3:
				mailReceivedID = "ccBoilerRoom";
				break;
			case 0:
				mailReceivedID = "ccPantry";
				break;
			case 2:
				mailReceivedID = "ccFishTank";
				break;
			case 4:
				mailReceivedID = "ccVault";
				break;
			case 5:
				mailReceivedID = "ccBulletin";
				Game1.addMailForTomorrow("ccBulletinThankYou");
				break;
			case 1:
				mailReceivedID = "ccCraftsRoom";
				break;
			}
			if (mailReceivedID.Length > 0 && !Game1.player.mailReceived.Contains(mailReceivedID))
			{
				Game1.player.mailForTomorrow.Add(mailReceivedID + "%&NL&%");
			}
		}

		public void loadArea(int area, bool showEffects = true)
		{
			Microsoft.Xna.Framework.Rectangle areaToRefurbish = getAreaBounds(area);
			Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\CommunityCenter_Refurbished");
			for (int x = areaToRefurbish.X; x < areaToRefurbish.Right; x++)
			{
				for (int y = areaToRefurbish.Y; y < areaToRefurbish.Bottom; y++)
				{
					if (refurbishedMap.GetLayer("Back").Tiles[x, y] != null)
					{
						map.GetLayer("Back").Tiles[x, y].TileIndex = refurbishedMap.GetLayer("Back").Tiles[x, y].TileIndex;
					}
					if (refurbishedMap.GetLayer("Buildings").Tiles[x, y] != null)
					{
						map.GetLayer("Buildings").Tiles[x, y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, refurbishedMap.GetLayer("Buildings").Tiles[x, y].TileIndex);
						adjustMapLightPropertiesForLamp(refurbishedMap.GetLayer("Buildings").Tiles[x, y].TileIndex, x, y, "Buildings");
						if (Game1.player.getTileX() == x && Game1.player.getTileY() == y)
						{
							Game1.player.Position = new Vector2(2080f, 576f);
						}
					}
					else
					{
						map.GetLayer("Buildings").Tiles[x, y] = null;
					}
					if (refurbishedMap.GetLayer("Front").Tiles[x, y] != null)
					{
						map.GetLayer("Front").Tiles[x, y] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, refurbishedMap.GetLayer("Front").Tiles[x, y].TileIndex);
						adjustMapLightPropertiesForLamp(refurbishedMap.GetLayer("Front").Tiles[x, y].TileIndex, x, y, "Front");
					}
					else
					{
						map.GetLayer("Front").Tiles[x, y] = null;
					}
					if (refurbishedMap.GetLayer("Paths").Tiles[x, y] != null && refurbishedMap.GetLayer("Paths").Tiles[x, y].TileIndex == 8)
					{
						Game1.currentLightSources.Add(new LightSource(4, new Vector2(x * 64, y * 64), 2f, LightSource.LightContext.None, 0L));
					}
					if (showEffects && Game1.random.NextDouble() < 0.58 && refurbishedMap.GetLayer("Buildings").Tiles[x, y] == null)
					{
						temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x * 64, y * 64), Color.White)
						{
							layerDepth = 1f,
							interval = 50f,
							motion = new Vector2((float)Game1.random.Next(17) / 10f, 0f),
							acceleration = new Vector2(-0.005f, 0f),
							delayBeforeAnimationStart = Game1.random.Next(500)
						});
					}
				}
			}
			if ((area == 5 || area == 8) && (bool)missedRewardsChestVisible)
			{
				doShowMissedRewardsChest(isVisible: true);
			}
			if (area == 5)
			{
				loadArea(7);
			}
			addLightGlows();
		}

		public void restoreAreaCutscene(int whichArea)
		{
			restoreAreaCutsceneEvent.Fire(whichArea);
		}

		public void markAreaAsComplete(int area)
		{
			if (Game1.currentLocation == this)
			{
				areasComplete[area] = true;
			}
			if (areAllAreasComplete() && Game1.currentLocation == this)
			{
				_isWatchingJunimoGoodbye = true;
			}
		}

		private void doRestoreAreaCutscene(int whichArea)
		{
			markAreaAsComplete(whichArea);
			restoreAreaIndex = whichArea;
			restoreAreaPhase = 0;
			restoreAreaTimer = 1000;
			if (Game1.player.currentLocation == this)
			{
				Game1.freezeControls = true;
				Game1.changeMusicTrack("none");
			}
			checkForMissedRewards();
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			restoreAreaCutsceneEvent.Poll();
			newJunimoNoteCheckEvent.Poll();
			areaCompleteRewardEvent.Poll();
			showMissedRewardsChestEvent.Poll();
			foreach (NetMutex i in bundleMutexes)
			{
				i.Update(this);
				if (i.IsLockHeld() && Game1.activeClickableMenu == null)
				{
					i.ReleaseLock();
				}
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			missedRewardsChest.Value.updateWhenCurrentLocation(time, this);
			if (restoreAreaTimer > 0)
			{
				int old = restoreAreaTimer;
				restoreAreaTimer -= time.ElapsedGameTime.Milliseconds;
				switch (restoreAreaPhase)
				{
				case 0:
					if (restoreAreaTimer <= 0)
					{
						restoreAreaTimer = 3000;
						restoreAreaPhase = 1;
						if (Game1.player.currentLocation == this)
						{
							Game1.player.faceDirection(2);
							Game1.player.jump();
							Game1.player.jitterStrength = 1f;
							Game1.player.showFrame(94);
						}
					}
					break;
				case 1:
					if (Game1.IsMasterGame && Game1.random.NextDouble() < 0.4)
					{
						Vector2 v = Utility.getRandomPositionInThisRectangle(getAreaBounds(restoreAreaIndex), Game1.random);
						Junimo i = new Junimo(v * 64f, restoreAreaIndex, temporary: true);
						if (!isCollidingPosition(i.GetBoundingBox(), Game1.viewport, i))
						{
							characters.Add(i);
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 5 : 46, v * 64f + new Vector2(16f, 16f), Color.White)
							{
								layerDepth = 1f
							});
							localSound("tinyWhip");
						}
					}
					if (restoreAreaTimer <= 0)
					{
						restoreAreaTimer = 999999;
						restoreAreaPhase = 2;
						if (Game1.player.currentLocation != this)
						{
							break;
						}
						Game1.screenGlowOnce(Color.White, hold: true, 0.005f, 1f);
						if (Game1.soundBank != null)
						{
							buildUpSound = Game1.soundBank.GetCue("wind");
							buildUpSound.SetVariable("Volume", 0f);
							buildUpSound.SetVariable("Frequency", 0f);
							buildUpSound.Play();
						}
						Game1.player.jitterStrength = 2f;
						Game1.player.stopShowingFrame();
					}
					Game1.drawLighting = false;
					break;
				case 2:
					if (buildUpSound != null)
					{
						buildUpSound.SetVariable("Volume", Game1.screenGlowAlpha * 150f);
						buildUpSound.SetVariable("Frequency", Game1.screenGlowAlpha * 150f);
					}
					if (Game1.screenGlowAlpha >= Game1.screenGlowMax)
					{
						messageAlpha += 0.008f;
						messageAlpha = Math.Min(messageAlpha, 1f);
					}
					if ((Game1.screenGlowAlpha == Game1.screenGlowMax || Game1.currentLocation != this) && restoreAreaTimer > 5200)
					{
						restoreAreaTimer = 5200;
					}
					if (restoreAreaTimer < 5200 && Game1.random.NextDouble() < (double)((float)(5200 - restoreAreaTimer) / 10000f))
					{
						localSound((Game1.random.NextDouble() < 0.5) ? "dustMeep" : "junimoMeep1");
					}
					if (restoreAreaTimer > 0)
					{
						break;
					}
					restoreAreaTimer = 2000;
					messageAlpha = 0f;
					restoreAreaPhase = 3;
					if (Game1.IsMasterGame)
					{
						for (int j = characters.Count - 1; j >= 0; j--)
						{
							if (characters[j] is Junimo && (bool)(characters[j] as Junimo).temporaryJunimo)
							{
								characters.RemoveAt(j);
							}
						}
					}
					if (Game1.player.currentLocation != this)
					{
						if (Game1.IsMasterGame)
						{
							loadArea(restoreAreaIndex);
							UpdateMapSeats();
						}
						break;
					}
					Game1.screenGlowHold = false;
					loadArea(restoreAreaIndex);
					if (Game1.IsMasterGame)
					{
						UpdateMapSeats();
					}
					if (buildUpSound != null)
					{
						buildUpSound.Stop(AudioStopOptions.Immediate);
					}
					localSound("wand");
					Game1.changeMusicTrack("junimoStarSong");
					localSound("woodyHit");
					Game1.flashAlpha = 1f;
					Game1.player.stopJittering();
					Game1.drawLighting = true;
					break;
				case 3:
					if (old > 1000 && restoreAreaTimer <= 1000)
					{
						Junimo k = getJunimoForArea(restoreAreaIndex);
						if (k != null && Game1.IsMasterGame)
						{
							if (!k.holdingBundle)
							{
								k.Position = Utility.getRandomAdjacentOpenTile(Utility.PointToVector2(getNotePosition(restoreAreaIndex)), this) * 64f;
								int iter = 0;
								while (isCollidingPosition(k.GetBoundingBox(), Game1.viewport, k) && iter < 20)
								{
									Microsoft.Xna.Framework.Rectangle area_bounds = getAreaBounds(restoreAreaIndex);
									if (restoreAreaIndex == 5)
									{
										area_bounds = new Microsoft.Xna.Framework.Rectangle(44, 13, 6, 2);
									}
									k.Position = Utility.getRandomPositionInThisRectangle(area_bounds, Game1.random) * 64f;
									iter++;
								}
								if (iter < 20)
								{
									k.fadeBack();
								}
							}
							k.returnToJunimoHutToFetchStar(this);
						}
					}
					if (restoreAreaTimer <= 0 && !_isWatchingJunimoGoodbye)
					{
						Game1.freezeControls = false;
					}
					break;
				}
			}
			else if (Game1.activeClickableMenu == null && junimoNotesViewportTargets != null && junimoNotesViewportTargets.Count > 0 && !Game1.isViewportOnCustomPath())
			{
				setViewportToNextJunimoNoteTarget();
			}
		}

		private void setViewportToNextJunimoNoteTarget()
		{
			if (junimoNotesViewportTargets.Count > 0)
			{
				Game1.freezeControls = true;
				int area = junimoNotesViewportTargets[0];
				Point p = getNotePosition(area);
				Game1.moveViewportTo(new Vector2(p.X, p.Y) * 64f, 5f, 2000, afterViewportGetsToJunimoNotePosition, setViewportToNextJunimoNoteTarget);
			}
			else
			{
				Game1.viewportFreeze = true;
				Game1.viewportHold = 10000;
				Game1.globalFadeToBlack(Game1.afterFadeReturnViewportToPlayer);
				Game1.freezeControls = false;
				Game1.afterViewport = null;
			}
		}

		private void afterViewportGetsToJunimoNotePosition()
		{
			int area = junimoNotesViewportTargets[0];
			junimoNotesViewportTargets.RemoveAt(0);
			addJunimoNote(area);
			localSound("reward");
		}

		public Junimo getJunimoForArea(int whichArea)
		{
			foreach (NPC c in characters)
			{
				if (c is Junimo && (int)(c as Junimo).whichArea == whichArea)
				{
					return c as Junimo;
				}
			}
			Junimo i = new Junimo(Vector2.Zero, whichArea);
			addCharacter(i);
			return i;
		}

		public bool areAllAreasComplete()
		{
			foreach (bool item in areasComplete)
			{
				if (!item)
				{
					return false;
				}
			}
			return true;
		}

		public void junimoGoodbyeDance()
		{
			getJunimoForArea(0).Position = new Vector2(23f, 11f) * 64f;
			getJunimoForArea(1).Position = new Vector2(27f, 11f) * 64f;
			getJunimoForArea(2).Position = new Vector2(24f, 12f) * 64f;
			getJunimoForArea(4).Position = new Vector2(26f, 12f) * 64f;
			getJunimoForArea(3).Position = new Vector2(28f, 12f) * 64f;
			getJunimoForArea(5).Position = new Vector2(25f, 11f) * 64f;
			for (int i = 0; i < areasComplete.Count; i++)
			{
				getJunimoForArea(i).stayStill();
				getJunimoForArea(i).faceDirection(1);
				getJunimoForArea(i).fadeBack();
				getJunimoForArea(i).IsInvisible = false;
				getJunimoForArea(i).setAlpha(1f);
			}
			Game1.moveViewportTo(new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()), 2f, 5000, startGoodbyeDance, endGoodbyeDance);
			Game1.viewportFreeze = false;
			Game1.freezeControls = true;
		}

		public void prepareForJunimoDance()
		{
			for (int j = 0; j < areasComplete.Count; j++)
			{
				Junimo junimoForArea = getJunimoForArea(j);
				junimoForArea.holdingBundle.Value = false;
				junimoForArea.holdingStar.Value = false;
				junimoForArea.controller = null;
				junimoForArea.Halt();
				junimoForArea.IsInvisible = true;
			}
			numberOfStarsOnPlaque.Value = 0;
			for (int i = 0; i < areasComplete.Count; i++)
			{
				if (areasComplete[i])
				{
					numberOfStarsOnPlaque.Value++;
				}
			}
		}

		private void startGoodbyeDance()
		{
			Game1.freezeControls = true;
			getJunimoForArea(0).Position = new Vector2(23f, 11f) * 64f;
			getJunimoForArea(1).Position = new Vector2(27f, 11f) * 64f;
			getJunimoForArea(2).Position = new Vector2(24f, 12f) * 64f;
			getJunimoForArea(4).Position = new Vector2(26f, 12f) * 64f;
			getJunimoForArea(3).Position = new Vector2(28f, 12f) * 64f;
			getJunimoForArea(5).Position = new Vector2(25f, 11f) * 64f;
			for (int i = 0; i < areasComplete.Count; i++)
			{
				getJunimoForArea(i).stayStill();
				getJunimoForArea(i).faceDirection(1);
				getJunimoForArea(i).fadeBack();
				getJunimoForArea(i).IsInvisible = false;
				getJunimoForArea(i).setAlpha(1f);
				getJunimoForArea(i).sayGoodbye();
			}
		}

		private void endGoodbyeDance()
		{
			for (int i = 0; i < areasComplete.Count; i++)
			{
				getJunimoForArea(i).fadeAway();
			}
			Game1.pauseThenDoFunction(3600, loadJunimoHut);
			Game1.freezeControls = true;
		}

		private void loadJunimoHut()
		{
			for (int i = 0; i < areasComplete.Count; i++)
			{
				getJunimoForArea(i).clearTextAboveHead();
			}
			loadArea(8);
			Game1.flashAlpha = 1f;
			localSound("wand");
			Game1.freezeControls = false;
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:CommunityCenter_JunimosReturned"));
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			for (int i = 0; i < (int)numberOfStarsOnPlaque; i++)
			{
				switch (i)
				{
				case 0:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2136f, 324f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 1:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2136f, 364f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 2:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2096f, 384f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 3:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2056f, 364f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 4:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2056f, 324f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				case 5:
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2096f, 308f)), new Microsoft.Xna.Framework.Rectangle(354, 401, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
					break;
				}
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			if (messageAlpha > 0f)
			{
				Junimo i = getJunimoForArea(0);
				if (i != null)
				{
					b.Draw(i.Sprite.Texture, new Vector2(Game1.viewport.Width / 2 - 32, (float)(Game1.viewport.Height * 2) / 3f - 64f), new Microsoft.Xna.Framework.Rectangle((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 800.0) / 100 * 16, 0, 16, 16), Color.Lime * messageAlpha, 0f, new Vector2(i.Sprite.SpriteWidth * 4 / 2, (float)(i.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, 1f) * 4f, i.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f);
				}
				b.DrawString(Game1.dialogueFont, "\"" + Game1.parseText(getMessageForAreaCompletion() + "\"", Game1.dialogueFont, 640), new Vector2(Game1.viewport.Width / 2 - 320, (float)(Game1.viewport.Height * 2) / 3f), Game1.textColor * messageAlpha * 0.6f);
			}
		}

		public static string getAreaNameFromNumber(int areaNumber)
		{
			switch (areaNumber)
			{
			case 3:
				return "Boiler Room";
			case 5:
				return "Bulletin Board";
			case 1:
				return "Crafts Room";
			case 2:
				return "Fish Tank";
			case 0:
				return "Pantry";
			case 4:
				return "Vault";
			case 6:
				return "Abandoned Joja Mart";
			default:
				return "";
			}
		}

		public static string getAreaEnglishDisplayNameFromNumber(int areaNumber)
		{
			return Game1.content.LoadBaseString("Strings\\Locations:CommunityCenter_AreaName_" + getAreaNameFromNumber(areaNumber).Replace(" ", ""));
		}

		public static string getAreaDisplayNameFromNumber(int areaNumber)
		{
			return Game1.content.LoadString("Strings\\Locations:CommunityCenter_AreaName_" + getAreaNameFromNumber(areaNumber).Replace(" ", ""));
		}

		public static StaticTile[] getJunimoNoteTileFrames(int area, Map map)
		{
			if (area != 5)
			{
				return new StaticTile[20]
				{
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1832),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1824),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1825),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1826),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1827),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1828),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1829),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1830),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1831),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1832),
					new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 1833)
				};
			}
			return new StaticTile[13]
			{
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1741),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1741),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1741),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1741),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1741),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1741),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1741),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1741),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1741),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1773),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1805),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1805),
				new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 1773)
			};
		}
	}
}
