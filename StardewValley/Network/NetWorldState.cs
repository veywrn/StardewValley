using Netcode;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Network
{
	public class NetWorldState : IWorldState, INetObject<NetFields>
	{
		private readonly NetInt year = new NetInt(1);

		private readonly NetString currentSeason = new NetString("spring");

		private readonly NetInt dayOfMonth = new NetInt(0);

		private readonly NetInt timeOfDay = new NetInt();

		private readonly NetInt whichFarm = new NetInt();

		private readonly NetInt daysPlayed = new NetInt();

		private readonly NetIntDictionary<LocationWeather, NetRef<LocationWeather>> locationWeather = new NetIntDictionary<LocationWeather, NetRef<LocationWeather>>();

		public readonly NetInt visitsUntilY1Guarantee = new NetInt(-1);

		public readonly NetEnum<Game1.MineChestType> shuffleMineChests = new NetEnum<Game1.MineChestType>(Game1.MineChestType.Default);

		private readonly NetBool isRaining = new NetBool();

		private readonly NetBool isSnowing = new NetBool();

		private readonly NetBool isLightning = new NetBool();

		private readonly NetBool bloomDay = new NetBool();

		private readonly NetBool isDebrisWeather = new NetBool();

		private readonly NetBool isPaused = new NetBool();

		private readonly NetBool isTimePaused = new NetBool();

		public readonly NetBool parrotPlatformsUnlocked = new NetBool();

		public readonly NetBool goblinRemoved = new NetBool();

		public readonly NetBool submarineLocked = new NetBool();

		public readonly NetInt weatherForTomorrow = new NetInt();

		public readonly NetInt minesDifficulty = new NetInt();

		public readonly NetInt skullCavesDifficulty = new NetInt();

		public readonly NetInt lowestMineLevel = new NetInt();

		public readonly NetInt lowestMineLevelForOrder = new NetInt(-1);

		public readonly NetInt currentSongIndex = new NetInt();

		private readonly NetBundles bundles = new NetBundles();

		private readonly NetIntDictionary<bool, NetBool> bundleRewards = new NetIntDictionary<bool, NetBool>();

		private readonly NetVector2Dictionary<int, NetInt> museumPieces = new NetVector2Dictionary<int, NetInt>();

		private readonly NetIntDelta lostBooksFound = new NetIntDelta();

		private readonly NetIntDelta goldenWalnuts = new NetIntDelta();

		private readonly NetIntDelta goldenWalnutsFound = new NetIntDelta();

		private readonly NetBool goldenCoconutCracked = new NetBool();

		private readonly NetStringDictionary<bool, NetBool> foundBuriedNuts = new NetStringDictionary<bool, NetBool>();

		private readonly NetStringList worldStateIDs = new NetStringList();

		private readonly NetIntDelta miniShippingBinsObtained = new NetIntDelta();

		private readonly NetStringDictionary<bool, NetBool> islandVisitors = new NetStringDictionary<bool, NetBool>();

		private readonly NetLong uniqueIDForThisGame = new NetLong();

		private readonly NetStringDictionary<string, NetString> netBundleData = new NetStringDictionary<string, NetString>();

		private Dictionary<string, string> _bundleData;

		private bool _bundleDataDirty = true;

		public readonly NetInt highestPlayerLimit = new NetInt(-1);

		public readonly NetInt currentPlayerLimit = new NetInt(-1);

		public readonly NetRef<Object> dishOfTheDay = new NetRef<Object>();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public WorldDate Date => new WorldDate(year, currentSeason, dayOfMonth);

		public bool IsTimePaused
		{
			get
			{
				return isTimePaused;
			}
			set
			{
				isTimePaused.Value = value;
			}
		}

		public bool IsPaused
		{
			get
			{
				return isPaused;
			}
			set
			{
				isPaused.Value = value;
			}
		}

		public bool IsGoblinRemoved
		{
			get
			{
				return goblinRemoved;
			}
			set
			{
				goblinRemoved.Value = value;
			}
		}

		public bool IsSubmarineLocked
		{
			get
			{
				return submarineLocked;
			}
			set
			{
				submarineLocked.Value = value;
			}
		}

		public int SkullCavesDifficulty
		{
			get
			{
				return skullCavesDifficulty;
			}
			set
			{
				skullCavesDifficulty.Value = value;
			}
		}

		public int MinesDifficulty
		{
			get
			{
				return minesDifficulty;
			}
			set
			{
				minesDifficulty.Value = value;
			}
		}

		public int LowestMineLevel
		{
			get
			{
				return lowestMineLevel;
			}
			set
			{
				lowestMineLevel.Value = value;
			}
		}

		public int LowestMineLevelForOrder
		{
			get
			{
				return lowestMineLevelForOrder;
			}
			set
			{
				lowestMineLevelForOrder.Value = value;
			}
		}

		public int WeatherForTomorrow
		{
			get
			{
				return weatherForTomorrow;
			}
			set
			{
				weatherForTomorrow.Value = value;
			}
		}

		public Game1.MineChestType ShuffleMineChests
		{
			get
			{
				return shuffleMineChests.Value;
			}
			set
			{
				shuffleMineChests.Value = value;
			}
		}

		public int VisitsUntilY1Guarantee
		{
			get
			{
				return visitsUntilY1Guarantee.Value;
			}
			set
			{
				visitsUntilY1Guarantee.Value = value;
			}
		}

		public NetBundles Bundles => bundles;

		public NetIntDictionary<bool, NetBool> BundleRewards => bundleRewards;

		public NetVector2Dictionary<int, NetInt> MuseumPieces => museumPieces;

		public NetStringDictionary<bool, NetBool> FoundBuriedNuts => foundBuriedNuts;

		public NetStringDictionary<bool, NetBool> IslandVisitors => islandVisitors;

		public NetIntDictionary<LocationWeather, NetRef<LocationWeather>> LocationWeather => locationWeather;

		public NetIntDelta MiniShippingBinsObtained => miniShippingBinsObtained;

		public NetIntDelta GoldenWalnutsFound => goldenWalnutsFound;

		public NetIntDelta GoldenWalnuts => goldenWalnuts;

		public NetBool GoldenCoconutCracked => goldenCoconutCracked;

		public NetBool ParrotPlatformsUnlocked => parrotPlatformsUnlocked;

		public Dictionary<string, string> BundleData
		{
			get
			{
				if (_bundleDataDirty)
				{
					_bundleDataDirty = false;
					_bundleData = new Dictionary<string, string>();
					foreach (string key in netBundleData.Keys)
					{
						_bundleData[key] = netBundleData[key];
					}
					if (LocalizedContentManager.CurrentLanguageCode != 0)
					{
						AddLocalizedBundleNames();
					}
				}
				return _bundleData;
			}
		}

		public NetIntDelta LostBooksFound => lostBooksFound;

		public NetInt CurrentPlayerLimit => currentPlayerLimit;

		public NetInt HighestPlayerLimit => highestPlayerLimit;

		public NetRef<Object> DishOfTheDay => dishOfTheDay;

		public NetWorldState()
		{
			if (Game1.specialCurrencyDisplay != null)
			{
				RegisterSpecialCurrencies();
			}
			miniShippingBinsObtained.Minimum = 0;
			goldenWalnuts.Minimum = 0;
			goldenWalnutsFound.Minimum = 0;
			lostBooksFound.Minimum = 0;
			lostBooksFound.Maximum = 21;
			NetFields.AddFields(year, currentSeason, dayOfMonth, timeOfDay, whichFarm, daysPlayed, weatherForTomorrow, isRaining, isSnowing, isLightning, bloomDay, isDebrisWeather, isPaused, goblinRemoved, submarineLocked, lowestMineLevel, bundles, bundleRewards, museumPieces, worldStateIDs, uniqueIDForThisGame, currentSongIndex, lostBooksFound, highestPlayerLimit, currentPlayerLimit, goldenWalnuts, goldenCoconutCracked, locationWeather, parrotPlatformsUnlocked, netBundleData, visitsUntilY1Guarantee, isTimePaused, dishOfTheDay, shuffleMineChests, miniShippingBinsObtained, foundBuriedNuts, goldenWalnutsFound, minesDifficulty, skullCavesDifficulty, lowestMineLevelForOrder, islandVisitors);
			SetBundleData(Game1.content.LoadBase<Dictionary<string, string>>("Data\\Bundles"));
			isTimePaused.InterpolationWait = false;
			netBundleData.OnConflictResolve += delegate
			{
				_bundleDataDirty = true;
			};
			netBundleData.OnValueAdded += delegate
			{
				_bundleDataDirty = true;
			};
			netBundleData.OnValueRemoved += delegate
			{
				_bundleDataDirty = true;
			};
		}

		public virtual void RegisterSpecialCurrencies()
		{
			if (Game1.specialCurrencyDisplay != null)
			{
				Game1.specialCurrencyDisplay.Register("walnuts", goldenWalnuts);
				Game1.specialCurrencyDisplay.Register("qiGems", Game1.player.netQiGems);
			}
		}

		public void SetBundleData(Dictionary<string, string> data)
		{
			_bundleDataDirty = true;
			netBundleData.CopyFrom(data);
			foreach (string key in netBundleData.Keys)
			{
				string value = netBundleData[key];
				int index = Convert.ToInt32(key.Split('/')[1]);
				int count = value.Split('/')[2].Split(' ').Length;
				if (!bundles.ContainsKey(index))
				{
					bundles.Add(index, new NetArray<bool, NetBool>(count));
				}
				else if (bundles[index].Length < count)
				{
					NetArray<bool, NetBool> new_array = new NetArray<bool, NetBool>(count);
					for (int i = 0; i < Math.Min(bundles[index].Length, count); i++)
					{
						new_array[i] = bundles[index][i];
					}
					bundles.Remove(index);
					bundles.Add(index, new_array);
				}
				if (!bundleRewards.ContainsKey(index))
				{
					bundleRewards.Add(index, new NetBool(value: false));
				}
			}
		}

		public static bool checkAnywhereForWorldStateID(string id)
		{
			if (!Game1.worldStateIDs.Contains(id))
			{
				return Game1.netWorldState.Value.hasWorldStateID(id);
			}
			return true;
		}

		public static void addWorldStateIDEverywhere(string id)
		{
			Game1.netWorldState.Value.addWorldStateID(id);
			if (!Game1.worldStateIDs.Contains(id))
			{
				Game1.worldStateIDs.Add(id);
			}
		}

		public Dictionary<string, string> GetUnlocalizedBundleData()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (string key in netBundleData.Keys)
			{
				dictionary[key] = netBundleData[key];
			}
			return dictionary;
		}

		public virtual void AddLocalizedBundleNames()
		{
			List<string> list = new List<string>(_bundleData.Keys);
			Dictionary<string, string> localized_bundle_data = Game1.content.Load<Dictionary<string, string>>("Data\\Bundles");
			foreach (string key in list)
			{
				string bundle_name = _bundleData[key].Split('/')[0];
				string localized_name = bundle_name;
				bool localization_found = false;
				foreach (string localized_key in localized_bundle_data.Keys)
				{
					string[] localized_split = localized_bundle_data[localized_key].Split('/');
					if (localized_split[0] == bundle_name)
					{
						localized_name = localized_split.Last();
						localization_found = true;
						break;
					}
				}
				if (!localization_found)
				{
					localized_name = Game1.content.LoadString("Strings\\BundleNames:" + bundle_name);
				}
				_bundleData[key] = _bundleData[key] + "/" + localized_name;
			}
		}

		public bool hasWorldStateID(string id)
		{
			return worldStateIDs.Contains(id);
		}

		public void addWorldStateID(string id)
		{
			if (!hasWorldStateID(id))
			{
				worldStateIDs.Add(id);
			}
		}

		public void removeWorldStateID(string id)
		{
			worldStateIDs.Remove(id);
		}

		public void UpdateFromGame1()
		{
			year.Value = Game1.year;
			currentSeason.Value = Game1.currentSeason;
			dayOfMonth.Value = Game1.dayOfMonth;
			timeOfDay.Value = Game1.timeOfDay;
			GetWeatherForLocation(GameLocation.LocationContext.Default).weatherForTomorrow.Value = Game1.weatherForTomorrow;
			GetWeatherForLocation(GameLocation.LocationContext.Default).isRaining.Value = Game1.isRaining;
			GetWeatherForLocation(GameLocation.LocationContext.Default).isSnowing.Value = Game1.isSnowing;
			GetWeatherForLocation(GameLocation.LocationContext.Default).bloomDay.Value = Game1.bloomDay;
			GetWeatherForLocation(GameLocation.LocationContext.Default).isDebrisWeather.Value = Game1.isDebrisWeather;
			isDebrisWeather.Value = Game1.isDebrisWeather;
			whichFarm.Value = Game1.whichFarm;
			weatherForTomorrow.Value = Game1.weatherForTomorrow;
			daysPlayed.Value = (int)Game1.stats.daysPlayed;
			currentSongIndex.Value = Game1.currentSongIndex;
			uniqueIDForThisGame.Value = (long)Game1.uniqueIDForThisGame;
			currentPlayerLimit.Value = Game1.multiplayer.playerLimit;
			highestPlayerLimit.Value = Math.Max(highestPlayerLimit.Value, Game1.multiplayer.playerLimit);
			worldStateIDs.CopyFrom(Game1.worldStateIDs);
		}

		public LocationWeather GetWeatherForLocation(int location_context)
		{
			if (!locationWeather.ContainsKey(location_context))
			{
				locationWeather[location_context] = new LocationWeather();
			}
			return locationWeather[location_context];
		}

		public LocationWeather GetWeatherForLocation(GameLocation.LocationContext location_context)
		{
			return GetWeatherForLocation((int)location_context);
		}

		public void WriteToGame1()
		{
			if (Game1.farmEvent != null)
			{
				return;
			}
			Game1.weatherForTomorrow = GetWeatherForLocation(GameLocation.LocationContext.Default).weatherForTomorrow.Value;
			Game1.isRaining = GetWeatherForLocation(GameLocation.LocationContext.Default).isRaining.Value;
			Game1.isSnowing = GetWeatherForLocation(GameLocation.LocationContext.Default).isSnowing.Value;
			Game1.isLightning = GetWeatherForLocation(GameLocation.LocationContext.Default).isLightning.Value;
			Game1.bloomDay = GetWeatherForLocation(GameLocation.LocationContext.Default).bloomDay.Value;
			Game1.isDebrisWeather = GetWeatherForLocation(GameLocation.LocationContext.Default).isDebrisWeather.Value;
			Game1.weatherForTomorrow = weatherForTomorrow.Value;
			Game1.worldStateIDs = worldStateIDs.ToList();
			if (!Game1.IsServer)
			{
				bool num = Game1.currentSeason != currentSeason.Value;
				Game1.year = year.Value;
				Game1.currentSeason = currentSeason.Value;
				Game1.dayOfMonth = dayOfMonth.Value;
				Game1.timeOfDay = timeOfDay.Value;
				Game1.whichFarm = whichFarm.Value;
				Game1.stats.daysPlayed = (uint)daysPlayed.Value;
				Game1.uniqueIDForThisGame = (ulong)uniqueIDForThisGame.Value;
				if (num)
				{
					Game1.setGraphicsForSeason();
				}
			}
			Game1.currentSongIndex = currentSongIndex.Value;
			Game1.updateWeatherIcon();
			if (IsGoblinRemoved)
			{
				Game1.player.removeQuest(27);
			}
		}
	}
}
