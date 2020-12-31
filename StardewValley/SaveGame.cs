using Ionic.Zlib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace StardewValley
{
	public class SaveGame
	{
		public enum SaveFixes
		{
			NONE,
			StoredBigCraftablesStackFix,
			PorchedCabinBushesFix,
			ChangeObeliskFootprintHeight,
			CreateStorageDressers,
			InferPreserves,
			TransferHatSkipHairFlag,
			RevealSecretNoteItemTastes,
			TransferHoneyTypeToPreserves,
			TransferNoteBlockScale,
			FixCropHarvestAmountsAndInferSeedIndex,
			Level9PuddingFishingRecipe,
			Level9PuddingFishingRecipe2,
			quarryMineBushes,
			MissingQisChallenge,
			BedsToFurniture,
			ChildBedsToFurniture,
			ModularizeFarmStructures,
			FixFlooringFlags,
			AddBugSteakRecipe,
			AddBirdie,
			AddTownBush,
			AddNewRingRecipes1_5,
			ResetForges,
			AddSquidInkRavioli,
			MakeDarkSwordVampiric,
			FixRingSheetIndex,
			FixBeachFarmBushes,
			AddCampfireKit,
			Level9PuddingFishingRecipe3,
			OstrichIncubatorFragility,
			FixBotchedBundleData,
			LeoChildrenFix,
			MAX
		}

		public static XmlSerializer serializer = new XmlSerializer(typeof(SaveGame), new Type[25]
		{
			typeof(Tool),
			typeof(GameLocation),
			typeof(Duggy),
			typeof(Bug),
			typeof(BigSlime),
			typeof(Ghost),
			typeof(Child),
			typeof(Pet),
			typeof(Dog),
			typeof(Cat),
			typeof(Horse),
			typeof(GreenSlime),
			typeof(LavaCrab),
			typeof(RockCrab),
			typeof(ShadowGuy),
			typeof(SquidKid),
			typeof(Grub),
			typeof(Fly),
			typeof(DustSpirit),
			typeof(Quest),
			typeof(MetalHead),
			typeof(ShadowGirl),
			typeof(Monster),
			typeof(JunimoHarvester),
			typeof(TerrainFeature)
		});

		public static XmlSerializer farmerSerializer = new XmlSerializer(typeof(Farmer), new Type[1]
		{
			typeof(Tool)
		});

		public static XmlSerializer locationSerializer = new XmlSerializer(typeof(GameLocation), new Type[24]
		{
			typeof(Tool),
			typeof(Duggy),
			typeof(Ghost),
			typeof(GreenSlime),
			typeof(LavaCrab),
			typeof(RockCrab),
			typeof(ShadowGuy),
			typeof(Child),
			typeof(Pet),
			typeof(Dog),
			typeof(Cat),
			typeof(Horse),
			typeof(SquidKid),
			typeof(Grub),
			typeof(Fly),
			typeof(DustSpirit),
			typeof(Bug),
			typeof(BigSlime),
			typeof(BreakableContainer),
			typeof(MetalHead),
			typeof(ShadowGirl),
			typeof(Monster),
			typeof(JunimoHarvester),
			typeof(TerrainFeature)
		});

		[InstancedStatic]
		public static bool IsProcessing;

		[InstancedStatic]
		public static bool CancelToTitle;

		public Farmer player;

		public List<GameLocation> locations;

		public string currentSeason;

		public string samBandName;

		public string elliottBookName;

		public List<string> mailbox;

		public List<string> broadcastedMail;

		public List<string> worldStateIDs;

		public int lostBooksFound = -1;

		public int goldenWalnuts = -1;

		public int goldenWalnutsFound;

		public int miniShippingBinsObtained;

		public bool mineShrineActivated;

		public bool goldenCoconutCracked;

		public bool parrotPlatformsUnlocked;

		public bool farmPerfect;

		public List<string> foundBuriedNuts = new List<string>();

		public int visitsUntilY1Guarantee = -1;

		public Game1.MineChestType shuffleMineChests;

		public int dayOfMonth;

		public int year;

		public int farmerWallpaper;

		public int FarmerFloor;

		public int currentWallpaper;

		public int currentFloor;

		public int currentSongIndex;

		public int? countdownToWedding;

		public Point incubatingEgg;

		public double chanceToRainTomorrow;

		public double dailyLuck;

		public ulong uniqueIDForThisGame;

		public bool weddingToday;

		public bool isRaining;

		public bool isDebrisWeather;

		public bool shippingTax;

		public bool bloomDay;

		public bool isLightning;

		public bool isSnowing;

		public bool shouldSpawnMonsters;

		public bool hasApplied1_3_UpdateChanges;

		public bool hasApplied1_4_UpdateChanges;

		public Stats stats;

		[InstancedStatic]
		public static SaveGame loaded;

		public float musicVolume;

		public float soundVolume;

		public int[] cropsOfTheWeek;

		public Object dishOfTheDay;

		public int highestPlayerLimit = -1;

		public int moveBuildingPermissionMode;

		public SerializableDictionary<GameLocation.LocationContext, LocationWeather> locationWeather;

		public SerializableDictionary<string, string> bannedUsers = new SerializableDictionary<string, string>();

		public SerializableDictionary<string, string> bundleData = new SerializableDictionary<string, string>();

		public SerializableDictionary<string, int> limitedNutDrops = new SerializableDictionary<string, int>();

		public long latestID;

		public Options options;

		public SerializableDictionary<long, Options> splitscreenOptions = new SerializableDictionary<long, Options>();

		public SerializableDictionary<string, string> CustomData = new SerializableDictionary<string, string>();

		public SerializableDictionary<int, MineInfo> mine_permanentMineChanges;

		[Obsolete]
		[XmlIgnore]
		public List<ResourceClump> mine_resourceClumps = new List<ResourceClump>();

		[Obsolete]
		[XmlIgnore]
		public int mine_mineLevel;

		[Obsolete]
		[XmlIgnore]
		public int mine_nextLevel;

		public int mine_lowestLevelReached;

		public int minecartHighScore;

		public int weatherForTomorrow;

		public int whichFarm;

		public int mine_lowestLevelReachedForOrder = -1;

		public int skullCavesDifficulty;

		public int minesDifficulty;

		public int currentGemBirdIndex;

		public NetLeaderboards junimoKartLeaderboards;

		public List<SpecialOrder> specialOrders;

		public List<SpecialOrder> availableSpecialOrders;

		public List<string> completedSpecialOrders;

		public List<string> acceptedSpecialOrderTypes = new List<string>();

		public List<Item> returnedDonations;

		public List<Item> junimoChest;

		public List<string> collectedNutTracker = new List<string>();

		public SerializableDictionary<FarmerPair, Friendship> farmerFriendships = new SerializableDictionary<FarmerPair, Friendship>();

		public SerializableDictionary<int, long> cellarAssignments = new SerializableDictionary<int, long>();

		public int lastAppliedSaveFix;

		public string gameVersion = Game1.version;

		public static XmlSerializer GetSerializer(Type type)
		{
			return new XmlSerializer(type);
		}

		public static IEnumerator<int> Save()
		{
			IsProcessing = true;
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				IEnumerator<int> save = getSaveEnumerator();
				while (save.MoveNext())
				{
					yield return save.Current;
				}
				yield return 100;
				yield break;
			}
			Console.WriteLine("SaveGame.Save() called.");
			yield return 1;
			IEnumerator<int> loader = getSaveEnumerator();
			Task saveTask = new Task(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				if (loader != null)
				{
					while (loader.MoveNext() && loader.Current < 100)
					{
					}
				}
			});
			saveTask.Start();
			while (!saveTask.IsCanceled && !saveTask.IsCompleted && !saveTask.IsFaulted)
			{
				yield return 1;
			}
			IsProcessing = false;
			if (saveTask.IsFaulted)
			{
				Exception e = saveTask.Exception.GetBaseException();
				Console.WriteLine("saveTask failed with an exception");
				Console.WriteLine(e);
				if (!(e is TaskCanceledException))
				{
					throw e;
				}
				Game1.ExitToTitle();
			}
			else
			{
				Console.WriteLine("SaveGame.Save() completed without exceptions.");
				yield return 100;
			}
		}

		public static string FilterFileName(string fileName)
		{
			string text = fileName;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (!char.IsLetterOrDigit(c))
				{
					fileName = fileName.Replace(c.ToString() ?? "", "");
				}
			}
			return fileName;
		}

		public static IEnumerator<int> getSaveEnumerator()
		{
			if (CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 1;
			SaveGame saveData = new SaveGame();
			saveData.player = Game1.player;
			saveData.player.gameVersion = Game1.version;
			saveData.locations = new List<GameLocation>();
			saveData.locations.AddRange(Game1.locations);
			foreach (GameLocation location in Game1.locations)
			{
				location.cleanupBeforeSave();
			}
			saveData.currentSeason = Game1.currentSeason;
			saveData.samBandName = Game1.samBandName;
			saveData.broadcastedMail = new List<string>();
			saveData.bannedUsers = Game1.bannedUsers;
			foreach (string mail_key in Game1.player.team.broadcastedMail)
			{
				saveData.broadcastedMail.Add(mail_key);
			}
			saveData.skullCavesDifficulty = Game1.netWorldState.Value.SkullCavesDifficulty;
			saveData.minesDifficulty = Game1.netWorldState.Value.MinesDifficulty;
			saveData.visitsUntilY1Guarantee = Game1.netWorldState.Value.VisitsUntilY1Guarantee;
			saveData.shuffleMineChests = Game1.netWorldState.Value.ShuffleMineChests;
			saveData.elliottBookName = Game1.elliottBookName;
			saveData.dayOfMonth = Game1.dayOfMonth;
			saveData.year = Game1.year;
			saveData.farmerWallpaper = Game1.farmerWallpaper;
			saveData.FarmerFloor = Game1.FarmerFloor;
			saveData.chanceToRainTomorrow = Game1.chanceToRainTomorrow;
			saveData.dailyLuck = Game1.player.team.sharedDailyLuck.Value;
			saveData.isRaining = Game1.isRaining;
			saveData.isLightning = Game1.isLightning;
			saveData.isSnowing = Game1.isSnowing;
			saveData.isDebrisWeather = Game1.isDebrisWeather;
			saveData.shouldSpawnMonsters = Game1.spawnMonstersAtNight;
			saveData.specialOrders = Game1.player.team.specialOrders.ToList();
			saveData.availableSpecialOrders = Game1.player.team.availableSpecialOrders.ToList();
			saveData.completedSpecialOrders = Game1.player.team.completedSpecialOrders.Keys.ToList();
			saveData.collectedNutTracker = new List<string>(Game1.player.team.collectedNutTracker.Keys);
			saveData.acceptedSpecialOrderTypes = Game1.player.team.acceptedSpecialOrderTypes.ToList();
			saveData.returnedDonations = Game1.player.team.returnedDonations.ToList();
			saveData.junimoChest = Game1.player.team.junimoChest.ToList();
			saveData.weddingToday = Game1.weddingToday;
			saveData.whichFarm = Game1.whichFarm;
			saveData.minecartHighScore = Game1.minecartHighScore;
			saveData.junimoKartLeaderboards = Game1.player.team.junimoKartScores;
			saveData.lastAppliedSaveFix = (int)Game1.lastAppliedSaveFix;
			saveData.locationWeather = new SerializableDictionary<GameLocation.LocationContext, LocationWeather>();
			foreach (int i in Game1.netWorldState.Value.LocationWeather.Keys)
			{
				LocationWeather weather = Game1.netWorldState.Value.LocationWeather[i];
				saveData.locationWeather[(GameLocation.LocationContext)i] = weather;
			}
			saveData.cellarAssignments = new SerializableDictionary<int, long>();
			foreach (int key4 in Game1.player.team.cellarAssignments.Keys)
			{
				saveData.cellarAssignments[key4] = Game1.player.team.cellarAssignments[key4];
			}
			saveData.uniqueIDForThisGame = Game1.uniqueIDForThisGame;
			saveData.musicVolume = Game1.options.musicVolumeLevel;
			saveData.soundVolume = Game1.options.soundVolumeLevel;
			saveData.shippingTax = Game1.shippingTax;
			saveData.cropsOfTheWeek = Game1.cropsOfTheWeek;
			saveData.mine_lowestLevelReached = Game1.netWorldState.Value.LowestMineLevel;
			saveData.mine_lowestLevelReachedForOrder = Game1.netWorldState.Value.LowestMineLevelForOrder;
			saveData.currentGemBirdIndex = Game1.currentGemBirdIndex;
			saveData.mine_permanentMineChanges = MineShaft.permanentMineChanges;
			saveData.currentFloor = Game1.currentFloor;
			saveData.currentWallpaper = Game1.currentWallpaper;
			saveData.bloomDay = Game1.bloomDay;
			saveData.dishOfTheDay = Game1.dishOfTheDay;
			saveData.latestID = (long)Game1.multiplayer.latestID;
			saveData.highestPlayerLimit = Game1.netWorldState.Value.HighestPlayerLimit;
			saveData.options = Game1.options;
			saveData.splitscreenOptions = Game1.splitscreenOptions;
			saveData.CustomData = Game1.CustomData;
			saveData.worldStateIDs = Game1.worldStateIDs;
			saveData.currentSongIndex = Game1.currentSongIndex;
			saveData.weatherForTomorrow = Game1.weatherForTomorrow;
			saveData.goldenWalnuts = Game1.netWorldState.Value.GoldenWalnuts;
			saveData.goldenWalnutsFound = Game1.netWorldState.Value.GoldenWalnutsFound;
			saveData.miniShippingBinsObtained = Game1.netWorldState.Value.MiniShippingBinsObtained;
			saveData.goldenCoconutCracked = Game1.netWorldState.Value.GoldenCoconutCracked.Value;
			saveData.parrotPlatformsUnlocked = Game1.netWorldState.Value.ParrotPlatformsUnlocked.Value;
			saveData.farmPerfect = Game1.player.team.farmPerfect.Value;
			saveData.lostBooksFound = Game1.netWorldState.Value.LostBooksFound;
			saveData.foundBuriedNuts = new List<string>(Game1.netWorldState.Value.FoundBuriedNuts.Keys);
			saveData.mineShrineActivated = Game1.player.team.mineShrineActivated;
			saveData.gameVersion = Game1.version;
			saveData.limitedNutDrops = new SerializableDictionary<string, int>();
			foreach (string key3 in Game1.player.team.limitedNutDrops.Keys)
			{
				saveData.limitedNutDrops[key3] = Game1.player.team.limitedNutDrops[key3];
			}
			saveData.bundleData = new SerializableDictionary<string, string>();
			Dictionary<string, string> unlocalized_bundles = Game1.netWorldState.Value.GetUnlocalizedBundleData();
			foreach (string key2 in unlocalized_bundles.Keys)
			{
				saveData.bundleData[key2] = unlocalized_bundles[key2];
			}
			saveData.moveBuildingPermissionMode = (int)Game1.player.team.farmhandsCanMoveBuildings.Value;
			saveData.hasApplied1_3_UpdateChanges = Game1.hasApplied1_3_UpdateChanges;
			saveData.hasApplied1_4_UpdateChanges = Game1.hasApplied1_4_UpdateChanges;
			foreach (FarmerPair key in Game1.player.team.friendshipData.Keys)
			{
				saveData.farmerFriendships[key] = Game1.player.team.friendshipData[key];
			}
			string tmpString = "_STARDEWVALLEYSAVETMP";
			bool save_backups_and_metadata = true;
			string friendlyName = FilterFileName(Game1.GetSaveGameName());
			string filenameNoTmpString = friendlyName + "_" + Game1.uniqueIDForThisGame;
			string filenameWithTmpString = friendlyName + "_" + Game1.uniqueIDForThisGame + tmpString;
			string save_directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves", filenameNoTmpString + Path.DirectorySeparatorChar.ToString());
			if (Game1.savePathOverride != "")
			{
				save_directory = Game1.savePathOverride;
				if (Game1.savePathOverride != "")
				{
					save_backups_and_metadata = false;
				}
			}
			string fullFilePath3 = Path.Combine(save_directory, filenameWithTmpString);
			ensureFolderStructureExists();
			string justFarmerFilePath3 = Path.Combine(save_directory, "SaveGameInfo" + tmpString);
			if (File.Exists(fullFilePath3))
			{
				File.Delete(fullFilePath3);
			}
			if (save_backups_and_metadata && File.Exists(justFarmerFilePath3))
			{
				File.Delete(justFarmerFilePath3);
			}
			Stream fstream = null;
			try
			{
				fstream = File.Create(fullFilePath3);
			}
			catch (IOException ex)
			{
				if (fstream != null)
				{
					fstream.Close();
					fstream.Dispose();
				}
				Game1.gameMode = 9;
				Game1.debugOutput = Game1.parseText(ex.Message);
				yield break;
			}
			MemoryStream mstream3 = new MemoryStream(1024);
			MemoryStream mstream2 = new MemoryStream(1024);
			if (CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 2;
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.CloseOutput = false;
			Console.WriteLine("Saving without compression...");
			_ = mstream2;
			XmlWriter writer2 = XmlWriter.Create(mstream3, settings);
			writer2.WriteStartDocument();
			serializer.Serialize(writer2, saveData);
			writer2.WriteEndDocument();
			writer2.Flush();
			writer2.Close();
			mstream3.Close();
			byte[] buffer = mstream3.ToArray();
			if (CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 2;
			fstream.Write(buffer, 0, buffer.Length);
			fstream.Close();
			if (save_backups_and_metadata)
			{
				Game1.player.saveTime = (int)(DateTime.UtcNow - new DateTime(2012, 6, 22)).TotalMinutes;
				try
				{
					fstream = File.Create(justFarmerFilePath3);
				}
				catch (IOException ex2)
				{
					fstream?.Close();
					Game1.gameMode = 9;
					Game1.debugOutput = Game1.parseText(ex2.Message);
					yield break;
				}
				writer2 = XmlWriter.Create(fstream, new XmlWriterSettings
				{
					CloseOutput = false
				});
				writer2.WriteStartDocument();
				farmerSerializer.Serialize(writer2, Game1.player);
				writer2.WriteEndDocument();
				writer2.Flush();
				fstream.Close();
			}
			if (CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 2;
			fullFilePath3 = Path.Combine(save_directory, filenameNoTmpString);
			justFarmerFilePath3 = Path.Combine(save_directory, "SaveGameInfo");
			if (save_backups_and_metadata)
			{
				string fullFilePathOld = Path.Combine(save_directory, filenameNoTmpString + "_old");
				string justFarmerFilePathOld = Path.Combine(save_directory, "SaveGameInfo_old");
				if (File.Exists(fullFilePathOld))
				{
					File.Delete(fullFilePathOld);
				}
				if (File.Exists(justFarmerFilePathOld))
				{
					File.Delete(justFarmerFilePathOld);
				}
				try
				{
					File.Move(fullFilePath3, fullFilePathOld);
					File.Move(justFarmerFilePath3, justFarmerFilePathOld);
				}
				catch (Exception)
				{
				}
			}
			if (File.Exists(fullFilePath3))
			{
				File.Delete(fullFilePath3);
			}
			if (save_backups_and_metadata && File.Exists(justFarmerFilePath3))
			{
				File.Delete(justFarmerFilePath3);
			}
			fullFilePath3 = Path.Combine(save_directory, filenameWithTmpString);
			if (File.Exists(fullFilePath3))
			{
				File.Move(fullFilePath3, fullFilePath3.Replace(tmpString, ""));
			}
			if (save_backups_and_metadata)
			{
				justFarmerFilePath3 = Path.Combine(save_directory, "SaveGameInfo" + tmpString);
				if (File.Exists(justFarmerFilePath3))
				{
					File.Move(justFarmerFilePath3, justFarmerFilePath3.Replace(tmpString, ""));
				}
			}
			if (CancelToTitle)
			{
				throw new TaskCanceledException();
			}
			yield return 100;
		}

		public static bool IsNewGameSaveNameCollision(string save_name)
		{
			string friendlyName = save_name;
			string text = friendlyName;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (!char.IsLetterOrDigit(c))
				{
					friendlyName = friendlyName.Replace(c.ToString() ?? "", "");
				}
			}
			if (!friendlyName.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				friendlyName += Path.DirectorySeparatorChar.ToString();
			}
			string filename = friendlyName + "_" + Game1.uniqueIDForThisGame;
			return new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), filename)).Directory.Exists;
		}

		public static void ensureFolderStructureExists(string tmpString = "")
		{
			string friendlyName = Game1.GetSaveGameName();
			string text = friendlyName;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (!char.IsLetterOrDigit(c))
				{
					friendlyName = friendlyName.Replace(c.ToString() ?? "", "");
				}
			}
			string filename = friendlyName + "_" + Game1.uniqueIDForThisGame + tmpString;
			FileInfo info3 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), filename));
			if (!info3.Directory.Exists)
			{
				info3.Directory.Create();
			}
			info3 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), filename), "dummy"));
			if (!info3.Directory.Exists)
			{
				info3.Directory.Create();
			}
			info3 = null;
		}

		public static void Load(string filename)
		{
			Game1.gameMode = 6;
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4690");
			Game1.currentLoader = getLoadEnumerator(filename);
		}

		public static IEnumerator<int> getLoadEnumerator(string file)
		{
			Game1.SetSaveName(Path.GetFileNameWithoutExtension(file).Split('_').FirstOrDefault());
			Console.WriteLine("getLoadEnumerator('{0}')", file);
			Stopwatch stopwatch = Stopwatch.StartNew();
			Game1.loadingMessage = "Accessing save...";
			SaveGame saveData = new SaveGame();
			IsProcessing = true;
			if (CancelToTitle)
			{
				Game1.ExitToTitle();
			}
			yield return 1;
			Stream stream = null;
			string fullFilePath = file;
			Game1.savePathOverride = Path.GetDirectoryName(file);
			if (Game1.savePathOverride == "")
			{
				fullFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves", file, file);
			}
			if (!File.Exists(fullFilePath))
			{
				fullFilePath += ".xml";
				if (!File.Exists(fullFilePath))
				{
					Game1.gameMode = 9;
					Game1.debugOutput = "File does not exist (-_-)";
					yield break;
				}
			}
			yield return 5;
			try
			{
				byte[] buffer = File.ReadAllBytes(fullFilePath);
				stream = new MemoryStream(buffer, writable: false);
			}
			catch (IOException ex)
			{
				Game1.gameMode = 9;
				Game1.debugOutput = Game1.parseText(ex.Message);
				if (stream != null)
				{
					stream.Close();
				}
				yield break;
			}
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4696");
			yield return 7;
			byte num = (byte)stream.ReadByte();
			stream.Position--;
			if (num == 120)
			{
				Console.WriteLine("zlib stream detected...");
				stream = new ZlibStream(stream, CompressionMode.Decompress);
			}
			else
			{
				Console.WriteLine("regular stream detected...");
			}
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				loaded = (SaveGame)serializer.Deserialize(stream);
			}
			else
			{
				SaveGame pendingSaveGame = null;
				Task deserializeTask4 = new Task(delegate
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					pendingSaveGame = (SaveGame)serializer.Deserialize(stream);
				});
				deserializeTask4.Start();
				while (!deserializeTask4.IsCanceled && !deserializeTask4.IsCompleted && !deserializeTask4.IsFaulted)
				{
					yield return 20;
				}
				if (deserializeTask4.IsFaulted)
				{
					Exception baseException = deserializeTask4.Exception.GetBaseException();
					Console.WriteLine("deserializeTask failed with an exception");
					Console.WriteLine(baseException);
					throw baseException;
				}
				loaded = pendingSaveGame;
			}
			stream.Dispose();
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4697");
			if (CancelToTitle)
			{
				Game1.ExitToTitle();
			}
			yield return 20;
			Game1.whichFarm = loaded.whichFarm;
			Game1.year = loaded.year;
			Game1.netWorldState.Value.CurrentPlayerLimit.Set(Game1.multiplayer.playerLimit);
			if (loaded.highestPlayerLimit >= 0)
			{
				Game1.netWorldState.Value.HighestPlayerLimit.Set(loaded.highestPlayerLimit);
			}
			else
			{
				Game1.netWorldState.Value.HighestPlayerLimit.Set(Math.Max(Game1.netWorldState.Value.HighestPlayerLimit.Value, Game1.multiplayer.MaxPlayers));
			}
			Game1.uniqueIDForThisGame = loaded.uniqueIDForThisGame;
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				Game1.loadForNewGame(loadedGame: true);
			}
			else
			{
				Task deserializeTask4 = new Task(delegate
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					Game1.loadForNewGame(loadedGame: true);
				});
				deserializeTask4.Start();
				while (!deserializeTask4.IsCanceled && !deserializeTask4.IsCompleted && !deserializeTask4.IsFaulted)
				{
					yield return 24;
				}
				if (deserializeTask4.IsFaulted)
				{
					Exception baseException2 = deserializeTask4.Exception.GetBaseException();
					Console.WriteLine("loadNewGameTask failed with an exception");
					Console.WriteLine(baseException2);
					throw baseException2;
				}
				if (CancelToTitle)
				{
					Game1.ExitToTitle();
				}
				yield return 25;
			}
			Game1.weatherForTomorrow = loaded.weatherForTomorrow;
			Game1.dayOfMonth = loaded.dayOfMonth;
			Game1.year = loaded.year;
			Game1.currentSeason = loaded.currentSeason;
			Game1.worldStateIDs = loaded.worldStateIDs;
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4698");
			if (loaded.mine_permanentMineChanges != null)
			{
				MineShaft.permanentMineChanges = loaded.mine_permanentMineChanges;
				Game1.netWorldState.Value.LowestMineLevel = loaded.mine_lowestLevelReached;
				Game1.netWorldState.Value.LowestMineLevelForOrder = loaded.mine_lowestLevelReachedForOrder;
			}
			Game1.currentGemBirdIndex = loaded.currentGemBirdIndex;
			if (loaded.bundleData.Count > 0)
			{
				Game1.netWorldState.Value.SetBundleData(loaded.bundleData);
				foreach (string key7 in Game1.netWorldState.Value.BundleData.Keys)
				{
					saveData.bundleData[key7] = Game1.netWorldState.Value.BundleData[key7];
				}
			}
			if (CancelToTitle)
			{
				Game1.ExitToTitle();
			}
			yield return 26;
			Game1.isRaining = loaded.isRaining;
			Game1.isLightning = loaded.isLightning;
			Game1.isSnowing = loaded.isSnowing;
			Game1.lastAppliedSaveFix = (SaveFixes)loaded.lastAppliedSaveFix;
			if (Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.UpdateFromGame1();
			}
			if (loaded.locationWeather != null)
			{
				foreach (GameLocation.LocationContext context in loaded.locationWeather.Keys)
				{
					Game1.netWorldState.Value.GetWeatherForLocation(context).CopyFrom(loaded.locationWeather[context]);
				}
			}
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				loadDataToFarmer(loaded.player);
			}
			else
			{
				Task deserializeTask4 = new Task(delegate
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					loadDataToFarmer(loaded.player);
				});
				deserializeTask4.Start();
				while (!deserializeTask4.IsCanceled && !deserializeTask4.IsCompleted && !deserializeTask4.IsFaulted)
				{
					yield return 1;
				}
				if (deserializeTask4.IsFaulted)
				{
					Exception baseException3 = deserializeTask4.Exception.GetBaseException();
					Console.WriteLine("loadFarmerTask failed with an exception");
					Console.WriteLine(baseException3);
					throw baseException3;
				}
			}
			Game1.player = loaded.player;
			if (Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved"))
			{
				Mountain mountain = Game1.getLocationFromName("Mountain") as Mountain;
				if (mountain != null)
				{
					mountain.reloadMap();
					mountain.ApplyTreehouseIfNecessary();
					if (mountain.treehouseDoorDirty)
					{
						mountain.treehouseDoorDirty = false;
						NPC.populateRoutesFromLocationToLocationList();
					}
				}
			}
			Game1.addParrotBoyIfNecessary();
			foreach (FarmerPair key6 in loaded.farmerFriendships.Keys)
			{
				Game1.player.team.friendshipData[key6] = loaded.farmerFriendships[key6];
			}
			Game1.spawnMonstersAtNight = loaded.shouldSpawnMonsters;
			Game1.player.team.limitedNutDrops.Clear();
			if (Game1.netWorldState != null && Game1.netWorldState.Value != null)
			{
				Game1.netWorldState.Value.RegisterSpecialCurrencies();
			}
			if (loaded.limitedNutDrops != null)
			{
				foreach (string key5 in loaded.limitedNutDrops.Keys)
				{
					if (loaded.limitedNutDrops[key5] > 0)
					{
						Game1.player.team.limitedNutDrops[key5] = loaded.limitedNutDrops[key5];
					}
				}
			}
			Game1.player.team.completedSpecialOrders.Clear();
			foreach (string key4 in loaded.completedSpecialOrders)
			{
				Game1.player.team.completedSpecialOrders[key4] = true;
			}
			Game1.player.team.specialOrders.Clear();
			foreach (SpecialOrder order2 in loaded.specialOrders)
			{
				Game1.player.team.specialOrders.Add(order2);
			}
			Game1.player.team.availableSpecialOrders.Clear();
			foreach (SpecialOrder order in loaded.availableSpecialOrders)
			{
				Game1.player.team.availableSpecialOrders.Add(order);
			}
			Game1.player.team.acceptedSpecialOrderTypes.Clear();
			Game1.player.team.acceptedSpecialOrderTypes.AddRange(loaded.acceptedSpecialOrderTypes);
			Game1.player.team.collectedNutTracker.Clear();
			foreach (string key3 in loaded.collectedNutTracker)
			{
				Game1.player.team.collectedNutTracker[key3] = true;
			}
			Game1.player.team.junimoChest.Clear();
			foreach (Item donated_item2 in loaded.junimoChest)
			{
				Game1.player.team.junimoChest.Add(donated_item2);
			}
			Game1.player.team.returnedDonations.Clear();
			foreach (Item donated_item in loaded.returnedDonations)
			{
				Game1.player.team.returnedDonations.Add(donated_item);
			}
			if (loaded.stats != null)
			{
				Game1.player.stats = loaded.stats;
			}
			if (loaded.mailbox != null && !Game1.player.mailbox.Any())
			{
				Game1.player.mailbox.Clear();
				Game1.player.mailbox.AddRange(loaded.mailbox);
			}
			Game1.random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 1);
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4699");
			if (CancelToTitle)
			{
				Game1.ExitToTitle();
			}
			yield return 36;
			if (loaded.cellarAssignments != null)
			{
				foreach (int key2 in loaded.cellarAssignments.Keys)
				{
					Game1.player.team.cellarAssignments[key2] = loaded.cellarAssignments[key2];
				}
			}
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				loadDataToLocations(loaded.locations);
			}
			else
			{
				Task deserializeTask4 = new Task(delegate
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					loadDataToLocations(loaded.locations);
				});
				deserializeTask4.Start();
				while (!deserializeTask4.IsCanceled && !deserializeTask4.IsCompleted && !deserializeTask4.IsFaulted)
				{
					yield return 1;
				}
				if (deserializeTask4.IsFaulted)
				{
					Exception baseException4 = deserializeTask4.Exception.GetBaseException();
					Console.WriteLine("loadLocationsTask failed with an exception");
					Console.WriteLine(baseException4);
					throw deserializeTask4.Exception.GetBaseException();
				}
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				int farmerMoney = farmer.Money;
				if (!Game1.player.team.individualMoney.ContainsKey(farmer.uniqueMultiplayerID))
				{
					Game1.player.team.individualMoney.Add(farmer.uniqueMultiplayerID, new NetIntDelta(farmerMoney));
				}
				Game1.player.team.individualMoney[farmer.uniqueMultiplayerID].Value = farmerMoney;
			}
			Game1.updateCellarAssignments();
			foreach (GameLocation location in Game1.locations)
			{
				if (location is BuildableGameLocation)
				{
					foreach (Building building2 in (location as BuildableGameLocation).buildings)
					{
						if (building2.indoors.Value is FarmHouse)
						{
							(building2.indoors.Value as FarmHouse).updateCellarWarps();
						}
					}
				}
				if (location is FarmHouse)
				{
					(location as FarmHouse).updateCellarWarps();
				}
			}
			if (CancelToTitle)
			{
				Game1.ExitToTitle();
			}
			yield return 50;
			yield return 51;
			Game1.isDebrisWeather = loaded.isDebrisWeather;
			if (Game1.isDebrisWeather)
			{
				Game1.populateDebrisWeatherArray();
			}
			else
			{
				Game1.debrisWeather.Clear();
			}
			yield return 53;
			Game1.player.team.sharedDailyLuck.Value = loaded.dailyLuck;
			yield return 54;
			yield return 55;
			Game1.bloomDay = loaded.bloomDay;
			Game1.setGraphicsForSeason();
			yield return 56;
			Game1.samBandName = loaded.samBandName;
			Game1.elliottBookName = loaded.elliottBookName;
			Game1.shippingTax = loaded.shippingTax;
			Game1.cropsOfTheWeek = loaded.cropsOfTheWeek;
			yield return 60;
			FurniturePlacer.addAllFurnitureOwnedByFarmer();
			yield return 63;
			Game1.weddingToday = loaded.weddingToday;
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4700");
			yield return 64;
			Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4701");
			yield return 73;
			Game1.farmerWallpaper = loaded.farmerWallpaper;
			yield return 75;
			Game1.updateWallpaperInFarmHouse(Game1.farmerWallpaper);
			yield return 77;
			Game1.FarmerFloor = loaded.FarmerFloor;
			if (CancelToTitle)
			{
				Game1.ExitToTitle();
			}
			yield return 79;
			Game1.updateFloorInFarmHouse(Game1.FarmerFloor);
			Game1.options.musicVolumeLevel = loaded.musicVolume;
			Game1.options.soundVolumeLevel = loaded.soundVolume;
			yield return 83;
			if (loaded.countdownToWedding.HasValue && loaded.countdownToWedding.Value != 0 && loaded.player.spouse != null && loaded.player.spouse != "")
			{
				WorldDate weddingDate = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
				weddingDate.TotalDays += loaded.countdownToWedding.Value;
				Friendship friendship = loaded.player.friendshipData[loaded.player.spouse];
				friendship.Status = FriendshipStatus.Engaged;
				friendship.WeddingDate = weddingDate;
			}
			yield return 85;
			yield return 87;
			Game1.chanceToRainTomorrow = loaded.chanceToRainTomorrow;
			yield return 88;
			yield return 95;
			Game1.currentSongIndex = loaded.currentSongIndex;
			Game1.fadeToBlack = true;
			Game1.fadeIn = false;
			Game1.fadeToBlackAlpha = 0.99f;
			_ = Game1.player.mostRecentBed;
			if (Game1.player.mostRecentBed.X <= 0f)
			{
				Game1.player.Position = new Vector2(192f, 384f);
			}
			Game1.removeFrontLayerForFarmBuildings();
			Game1.addNewFarmBuildingMaps();
			GameLocation last_sleep_location = null;
			if (Game1.player.lastSleepLocation.Value != null && Game1.isLocationAccessible(Game1.player.lastSleepLocation.Value))
			{
				last_sleep_location = Game1.getLocationFromName(Game1.player.lastSleepLocation);
			}
			bool apply_default_bed_position = true;
			if (last_sleep_location != null && (Game1.player.sleptInTemporaryBed.Value || last_sleep_location.GetFurnitureAt(Utility.PointToVector2(Game1.player.lastSleepPoint)) is BedFurniture))
			{
				Game1.currentLocation = last_sleep_location;
				Game1.player.currentLocation = Game1.currentLocation;
				Game1.player.Position = Utility.PointToVector2(Game1.player.lastSleepPoint) * 64f;
				apply_default_bed_position = false;
			}
			if (apply_default_bed_position)
			{
				Game1.currentLocation = Game1.getLocationFromName("FarmHouse");
			}
			Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
			Game1.player.CanMove = true;
			Game1.player.ReequipEnchantments();
			if (loaded.junimoKartLeaderboards != null)
			{
				Game1.player.team.junimoKartScores.LoadScores(loaded.junimoKartLeaderboards.GetScores());
			}
			Game1.minecartHighScore = loaded.minecartHighScore;
			Game1.currentWallpaper = loaded.currentWallpaper;
			Game1.currentFloor = loaded.currentFloor;
			Game1.options = loaded.options;
			Game1.splitscreenOptions = loaded.splitscreenOptions;
			Game1.CustomData = loaded.CustomData;
			Game1.hasApplied1_3_UpdateChanges = loaded.hasApplied1_3_UpdateChanges;
			Game1.hasApplied1_4_UpdateChanges = loaded.hasApplied1_4_UpdateChanges;
			Game1.RefreshQuestOfTheDay();
			Game1.player.team.broadcastedMail.Clear();
			if (loaded.broadcastedMail != null)
			{
				foreach (string mail_key in loaded.broadcastedMail)
				{
					Game1.player.team.broadcastedMail.Add(mail_key);
				}
			}
			if (Game1.options == null)
			{
				Game1.options = new Options();
			}
			else
			{
				Game1.options.platformClampValues();
			}
			try
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(async: false, applyLanguage: false);
				Game1.options.gamepadMode = startupPreferences.gamepadMode;
			}
			catch (Exception)
			{
			}
			if (Game1.soundBank != null)
			{
				Game1.initializeVolumeLevels();
			}
			Game1.multiplayer.latestID = (ulong)loaded.latestID;
			Game1.netWorldState.Value.SkullCavesDifficulty = loaded.skullCavesDifficulty;
			Game1.netWorldState.Value.MinesDifficulty = loaded.minesDifficulty;
			Game1.netWorldState.Value.VisitsUntilY1Guarantee = loaded.visitsUntilY1Guarantee;
			Game1.netWorldState.Value.ShuffleMineChests = loaded.shuffleMineChests;
			Game1.netWorldState.Value.DishOfTheDay.Value = loaded.dishOfTheDay;
			if (Game1.IsRainingHere())
			{
				Game1.changeMusicTrack("rain", track_interruptable: true);
			}
			Game1.updateWeatherIcon();
			Game1.netWorldState.Value.MiniShippingBinsObtained.Set(loaded.miniShippingBinsObtained);
			Game1.netWorldState.Value.LostBooksFound.Set(loaded.lostBooksFound);
			Game1.netWorldState.Value.GoldenWalnuts.Set(loaded.goldenWalnuts);
			Game1.netWorldState.Value.GoldenWalnutsFound.Set(loaded.goldenWalnutsFound);
			Game1.netWorldState.Value.GoldenCoconutCracked.Value = loaded.goldenCoconutCracked;
			Game1.netWorldState.Value.FoundBuriedNuts.Clear();
			foreach (string key in loaded.foundBuriedNuts)
			{
				Game1.netWorldState.Value.FoundBuriedNuts[key] = true;
			}
			IslandSouth.SetupIslandSchedules();
			Game1.player.team.farmhandsCanMoveBuildings.Value = (FarmerTeam.RemoteBuildingPermissions)loaded.moveBuildingPermissionMode;
			Game1.player.team.mineShrineActivated.Value = loaded.mineShrineActivated;
			if (Game1.multiplayerMode == 2)
			{
				if (Program.sdk.Networking != null && Game1.options.serverPrivacy == ServerPrivacy.InviteOnly)
				{
					Game1.options.setServerMode("invite");
				}
				else if (Program.sdk.Networking != null && Game1.options.serverPrivacy == ServerPrivacy.FriendsOnly)
				{
					Game1.options.setServerMode("friends");
				}
				else
				{
					Game1.options.serverPrivacy = ServerPrivacy.Public;
					Game1.options.setServerMode("online");
				}
			}
			Game1.bannedUsers = loaded.bannedUsers;
			bool need_lost_book_recount = false;
			if (loaded.lostBooksFound < 0)
			{
				need_lost_book_recount = true;
			}
			loaded = null;
			Game1.currentLocation.lastTouchActionLocation = Game1.player.getTileLocation();
			if (Game1.player.horseName.Value == null)
			{
				Horse horse = Utility.findHorse(Guid.Empty);
				if (horse != null && horse.displayName != "")
				{
					Game1.player.horseName.Value = horse.displayName;
					horse.ownerId.Value = Game1.player.UniqueMultiplayerID;
				}
			}
			Game1.UpdateHorseOwnership();
			foreach (Item i in Game1.player.items)
			{
				if (i != null && i is Object)
				{
					(i as Object).reloadSprite();
				}
			}
			Game1.gameMode = 3;
			Game1.AddModNPCs();
			try
			{
				Game1.fixProblems();
			}
			catch (Exception)
			{
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				LevelUpMenu.AddMissedProfessionChoices(allFarmer);
				LevelUpMenu.AddMissedLevelRecipes(allFarmer);
				LevelUpMenu.RevalidateHealth(allFarmer);
			}
			updateWedding();
			foreach (Building building in Game1.getFarm().buildings)
			{
				if ((int)building.daysOfConstructionLeft <= 0 && building.indoors.Value is Cabin)
				{
					(building.indoors.Value as Cabin).updateFarmLayout();
				}
				if ((int)building.daysOfConstructionLeft <= 0 && building.indoors.Value is Shed)
				{
					Shed shed = building.indoors.Value as Shed;
					shed.updateLayout();
					building.updateInteriorWarps(shed);
				}
			}
			if (!Game1.hasApplied1_3_UpdateChanges)
			{
				Game1.apply1_3_UpdateChanges();
			}
			if (!Game1.hasApplied1_4_UpdateChanges)
			{
				Game1.apply1_4_UpdateChanges();
			}
			else
			{
				if (need_lost_book_recount)
				{
					Game1.recalculateLostBookCount();
				}
				Game1.UpdateFarmPerfection();
				Game1.doMorningStuff();
			}
			int current_save_fix = (int)Game1.lastAppliedSaveFix;
			while (current_save_fix < 32)
			{
				if (Enum.IsDefined(typeof(SaveFixes), current_save_fix))
				{
					current_save_fix++;
					Console.WriteLine("Applying save fix: " + (SaveFixes)current_save_fix);
					Game1.applySaveFix((SaveFixes)current_save_fix);
					Game1.lastAppliedSaveFix = (SaveFixes)current_save_fix;
				}
			}
			if (apply_default_bed_position && Game1.player.currentLocation is FarmHouse)
			{
				Game1.player.Position = Utility.PointToVector2((Game1.player.currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f;
			}
			BedFurniture.ShiftPositionForBed(Game1.player);
			Game1.stats.checkForAchievements();
			if (Game1.stats.stat_dictionary.ContainsKey("walnutsFound"))
			{
				Game1.netWorldState.Value.GoldenWalnutsFound.Value += (int)Game1.stats.stat_dictionary["walnutsFound"];
				Game1.stats.stat_dictionary.Remove("walnutsFound");
			}
			if (Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.UpdateFromGame1();
			}
			Console.WriteLine("getLoadEnumerator() exited, elapsed = '{0}'", stopwatch.Elapsed);
			if (CancelToTitle)
			{
				Game1.ExitToTitle();
			}
			IsProcessing = false;
			Game1.player.currentLocation.lastTouchActionLocation = Game1.player.getTileLocation();
			Game1.player.currentLocation.resetForPlayerEntry();
			Game1.player.showToolUpgradeAvailability();
			Game1.dayTimeMoneyBox.questsDirty = true;
			yield return 100;
		}

		private static void updateWedding()
		{
		}

		public static void loadDataToFarmer(Farmer target)
		{
			target.gameVersion = target.gameVersion;
			target.items.CopyFrom(target.items);
			target.canMove = true;
			target.Sprite = new FarmerSprite(null);
			target.FarmerSprite.setOwner(target);
			if (target.cookingRecipes == null || target.cookingRecipes.Count() == 0)
			{
				target.cookingRecipes.Add("Fried Egg", 0);
			}
			if (target.craftingRecipes == null || target.craftingRecipes.Count() == 0)
			{
				target.craftingRecipes.Add("Lumber", 0);
			}
			if (!target.songsHeard.Contains("title_day"))
			{
				target.songsHeard.Add("title_day");
			}
			if (!target.songsHeard.Contains("title_night"))
			{
				target.songsHeard.Add("title_night");
			}
			if (target.addedSpeed > 0)
			{
				target.addedSpeed = 0;
			}
			target.maxItems.Value = target.maxItems;
			for (int i = 0; i < (int)target.maxItems; i++)
			{
				if (target.items.Count <= i)
				{
					target.items.Add(null);
				}
			}
			if (target.FarmerRenderer == null)
			{
				target.FarmerRenderer = new FarmerRenderer(target.getTexture(), target);
			}
			target.changeGender(target.IsMale);
			target.changeAccessory(target.accessory);
			target.changeShirt(target.shirt);
			target.changePants(target.pantsColor);
			target.changeSkinColor(target.skin);
			target.changeHairColor(target.hairstyleColor);
			target.changeHairStyle(target.hair);
			target.changeShoeColor(target.shoes);
			target.changeEyeColor(target.newEyeColor);
			target.Stamina = target.Stamina;
			target.health = target.health;
			target.MaxStamina = target.MaxStamina;
			target.mostRecentBed = target.mostRecentBed;
			target.Position = target.mostRecentBed;
			target.position.X -= 64f;
			if (!target.craftingRecipes.ContainsKey("Wood Path"))
			{
				target.craftingRecipes.Add("Wood Path", 1);
			}
			if (!target.craftingRecipes.ContainsKey("Gravel Path"))
			{
				target.craftingRecipes.Add("Gravel Path", 1);
			}
			if (!target.craftingRecipes.ContainsKey("Cobblestone Path"))
			{
				target.craftingRecipes.Add("Cobblestone Path", 1);
			}
			if (target.friendships != null && target.friendshipData.Count() == 0)
			{
				foreach (KeyValuePair<string, int[]> friend in target.friendships)
				{
					Friendship friendship2 = new Friendship(friend.Value[0]);
					friendship2.GiftsThisWeek = friend.Value[1];
					friendship2.TalkedToToday = (friend.Value[2] != 0);
					friendship2.GiftsToday = friend.Value[3];
					friendship2.ProposalRejected = (friend.Value[4] != 0);
					target.friendshipData[friend.Key] = friendship2;
				}
				target.friendships = null;
			}
			if (target.spouse != null && target.spouse != "")
			{
				bool engaged = target.spouse.Contains("engaged");
				string spouseName = target.spouse.Replace("engaged", "");
				Friendship friendship = target.friendshipData[spouseName];
				if ((friendship.Status == FriendshipStatus.Friendly || friendship.Status == FriendshipStatus.Dating) | engaged)
				{
					if (engaged)
					{
						friendship.Status = FriendshipStatus.Engaged;
					}
					else
					{
						friendship.Status = FriendshipStatus.Married;
					}
					target.spouse = spouseName;
					if (!engaged)
					{
						friendship.WeddingDate = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
						friendship.WeddingDate.TotalDays -= target.daysMarried;
						target.daysMarried = 0;
					}
				}
			}
			target.questLog.Filter((Quest x) => x != null);
			target.songsHeard = target.songsHeard.Distinct().ToList();
			target.ConvertClothingOverrideToClothesItems();
			target.UpdateClothing();
		}

		public static void loadDataToLocations(List<GameLocation> gamelocations)
		{
			foreach (GameLocation l2 in gamelocations)
			{
				if (l2 is Cellar && Game1.getLocationFromName(l2.name) == null)
				{
					Game1.locations.Add(new Cellar("Maps\\Cellar", l2.name));
				}
				if (l2 is FarmHouse)
				{
					GameLocation realLocation2 = Game1.getLocationFromName(l2.name);
					(realLocation2 as FarmHouse).setMapForUpgradeLevel((realLocation2 as FarmHouse).upgradeLevel);
					(realLocation2 as FarmHouse).fireplaceOn.Value = (l2 as FarmHouse).fireplaceOn;
					(realLocation2 as FarmHouse).fridge.Value = (l2 as FarmHouse).fridge;
					(realLocation2 as FarmHouse).farmerNumberOfOwner = (l2 as FarmHouse).farmerNumberOfOwner;
				}
				if (l2.name.Equals("Farm"))
				{
					GameLocation realLocation3 = Game1.getLocationFromName(l2.name);
					foreach (Building building in ((Farm)l2).buildings)
					{
						building.load();
					}
					((Farm)realLocation3).buildings.Set(((Farm)l2).buildings);
					foreach (FarmAnimal value in ((Farm)l2).animals.Values)
					{
						value.reload(null);
					}
					((Farm)realLocation3).greenhouseUnlocked.Value = ((Farm)l2).greenhouseUnlocked.Value;
					((Farm)realLocation3).greenhouseMoved.Value = ((Farm)l2).greenhouseMoved.Value;
				}
				if (l2.name.Equals("MovieTheater"))
				{
					(Game1.getLocationFromName(l2.name) as MovieTheater).dayFirstEntered.Set((l2 as MovieTheater).dayFirstEntered);
				}
			}
			Game1.netWorldState.Value.ParrotPlatformsUnlocked.Set(loaded.parrotPlatformsUnlocked);
			Game1.player.team.farmPerfect.Value = loaded.farmPerfect;
			foreach (GameLocation l3 in gamelocations)
			{
				GameLocation realLocation = Game1.getLocationFromName(l3.name);
				if (realLocation != null)
				{
					realLocation.miniJukeboxCount.Value = l3.miniJukeboxCount.Value;
					realLocation.miniJukeboxTrack.Value = l3.miniJukeboxTrack.Value;
					realLocation.furniture.Set(l3.furniture);
					foreach (Furniture item in realLocation.furniture)
					{
						item.updateDrawPosition();
					}
					for (int i2 = l3.characters.Count - 1; i2 >= 0; i2--)
					{
						initializeCharacter(l3.characters[i2], realLocation);
					}
					foreach (LargeTerrainFeature largeTerrainFeature in l3.largeTerrainFeatures)
					{
						largeTerrainFeature.currentLocation = realLocation;
						largeTerrainFeature.loadSprite();
					}
					foreach (TerrainFeature value2 in l3.terrainFeatures.Values)
					{
						value2.currentLocation = realLocation;
						value2.loadSprite();
					}
					foreach (KeyValuePair<Vector2, Object> v in l3.objects.Pairs)
					{
						v.Value.initializeLightSource(v.Key);
						v.Value.reloadSprite();
					}
					if (l3.name.Equals("Farm"))
					{
						((Farm)realLocation).buildings.Set(((Farm)l3).buildings);
						foreach (FarmAnimal value3 in ((Farm)l3).animals.Values)
						{
							value3.reload(null);
						}
						foreach (Building building2 in ((Farm)realLocation).buildings)
						{
							building2.load();
						}
					}
					if (realLocation != null)
					{
						realLocation.characters.Set(l3.characters);
						realLocation.netObjects.Set(l3.netObjects.Pairs);
						realLocation.numberOfSpawnedObjectsOnMap = l3.numberOfSpawnedObjectsOnMap;
						realLocation.terrainFeatures.Set(l3.terrainFeatures.Pairs);
						realLocation.largeTerrainFeatures.Set(l3.largeTerrainFeatures);
						if (realLocation.name.Equals("Farm"))
						{
							((Farm)realLocation).animals.MoveFrom(((Farm)l3).animals);
							(realLocation as Farm).piecesOfHay.Value = (l3 as Farm).piecesOfHay;
							List<ResourceClump> clumps = new List<ResourceClump>((l3 as Farm).resourceClumps);
							(l3 as Farm).resourceClumps.Clear();
							(realLocation as Farm).resourceClumps.Set(clumps);
							(realLocation as Farm).hasSeenGrandpaNote = (l3 as Farm).hasSeenGrandpaNote;
							(realLocation as Farm).grandpaScore = (l3 as Farm).grandpaScore;
							(realLocation as Farm).petBowlWatered.Set((l3 as Farm).petBowlWatered.Value);
						}
						if (realLocation.name.Equals("Town"))
						{
							(realLocation as Town).daysUntilCommunityUpgrade.Value = (l3 as Town).daysUntilCommunityUpgrade;
						}
						if (realLocation is Beach)
						{
							(realLocation as Beach).bridgeFixed.Value = (l3 as Beach).bridgeFixed;
						}
						if (realLocation is Woods)
						{
							(realLocation as Woods).stumps.MoveFrom((l3 as Woods).stumps);
							(realLocation as Woods).hasUnlockedStatue.Value = (l3 as Woods).hasUnlockedStatue.Value;
						}
						if (realLocation is CommunityCenter)
						{
							(realLocation as CommunityCenter).areasComplete.Set((l3 as CommunityCenter).areasComplete);
						}
						if (realLocation is ShopLocation && l3 is ShopLocation)
						{
							(realLocation as ShopLocation).itemsFromPlayerToSell.MoveFrom((l3 as ShopLocation).itemsFromPlayerToSell);
							(realLocation as ShopLocation).itemsToStartSellingTomorrow.MoveFrom((l3 as ShopLocation).itemsToStartSellingTomorrow);
						}
						if (realLocation is Forest)
						{
							if (Game1.dayOfMonth % 7 % 5 == 0)
							{
								(realLocation as Forest).travelingMerchantDay = true;
								(realLocation as Forest).travelingMerchantBounds.Clear();
								(realLocation as Forest).travelingMerchantBounds.Add(new Rectangle(1472, 640, 492, 112));
								(realLocation as Forest).travelingMerchantBounds.Add(new Rectangle(1652, 744, 76, 48));
								(realLocation as Forest).travelingMerchantBounds.Add(new Rectangle(1812, 744, 104, 48));
								foreach (Rectangle travelingMerchantBound in (realLocation as Forest).travelingMerchantBounds)
								{
									Utility.clearObjectsInArea(travelingMerchantBound, realLocation);
								}
							}
							(realLocation as Forest).log = (l3 as Forest).log;
						}
						realLocation.TransferDataFromSavedLocation(l3);
						if (realLocation is IslandLocation)
						{
							(realLocation as IslandLocation).AddAdditionalWalnutBushes();
						}
					}
				}
			}
			List<NPC> characters = new List<NPC>();
			foreach (GameLocation n in Game1.locations)
			{
				for (int m = 0; m < n.characters.Count; m++)
				{
					characters.Add(n.characters[m]);
				}
			}
			for (int l = 0; l < characters.Count; l++)
			{
				characters[l].reloadSprite();
			}
			foreach (GameLocation k in Game1.locations)
			{
				if (k is BuildableGameLocation)
				{
					foreach (Building building3 in (k as BuildableGameLocation).buildings)
					{
						GameLocation interior = building3.indoors.Value;
						if (interior != null)
						{
							for (int j = interior.characters.Count - 1; j >= 0; j--)
							{
								if (j < interior.characters.Count)
								{
									interior.characters[j].reloadSprite();
								}
							}
						}
					}
				}
			}
			foreach (GameLocation i in Game1.locations)
			{
				if (i.name.Equals("Farm"))
				{
					Game1.getLocationFromName(i.name);
					foreach (Building b in ((Farm)i).buildings)
					{
						if (b is Stable && (int)b.daysOfConstructionLeft <= 0)
						{
							(b as Stable).grabHorse();
						}
					}
				}
			}
			(Game1.getLocationFromName("FarmCave") as FarmCave)?.UpdateReadyFlag();
			Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
		}

		public static void initializeCharacter(NPC c, GameLocation location)
		{
			string default_map = c.DefaultMap;
			Vector2 default_position = c.DefaultPosition;
			c.reloadData();
			if (c.DefaultMap != default_map)
			{
				c.DefaultMap = default_map;
				c.DefaultPosition = default_position;
			}
			if (!c.DefaultPosition.Equals(Vector2.Zero))
			{
				c.Position = c.DefaultPosition;
			}
			c.currentLocation = location;
			if (c.datingFarmer)
			{
				Friendship friendship2 = Game1.player.friendshipData[c.Name];
				if (!friendship2.IsDating())
				{
					friendship2.Status = FriendshipStatus.Dating;
				}
				c.datingFarmer = false;
			}
			else if (c.divorcedFromFarmer)
			{
				Friendship friendship = Game1.player.friendshipData[c.Name];
				if (!friendship.IsDivorced())
				{
					friendship.RoommateMarriage = false;
					friendship.Status = FriendshipStatus.Divorced;
				}
				c.divorcedFromFarmer = false;
			}
		}
	}
}
