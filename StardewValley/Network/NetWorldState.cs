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

		private readonly NetBool isRaining = new NetBool();

		private readonly NetBool isSnowing = new NetBool();

		private readonly NetBool isLightning = new NetBool();

		private readonly NetBool bloomDay = new NetBool();

		private readonly NetBool isDebrisWeather = new NetBool();

		private readonly NetBool isPaused = new NetBool();

		public readonly NetBool goblinRemoved = new NetBool();

		public readonly NetBool submarineLocked = new NetBool();

		public readonly NetInt weatherForTomorrow = new NetInt();

		public readonly NetInt lowestMineLevel = new NetInt();

		public readonly NetInt currentSongIndex = new NetInt();

		private readonly NetBundles bundles = new NetBundles();

		private readonly NetIntDictionary<bool, NetBool> bundleRewards = new NetIntDictionary<bool, NetBool>();

		private readonly NetVector2Dictionary<int, NetInt> museumPieces = new NetVector2Dictionary<int, NetInt>();

		private readonly NetIntDelta lostBooksFound = new NetIntDelta();

		private readonly NetStringList worldStateIDs = new NetStringList();

		private readonly NetLong uniqueIDForThisGame = new NetLong();

		public readonly NetInt highestPlayerLimit = new NetInt(-1);

		public readonly NetInt currentPlayerLimit = new NetInt(-1);

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public WorldDate Date => new WorldDate(year, currentSeason, dayOfMonth);

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

		public NetBundles Bundles => bundles;

		public NetIntDictionary<bool, NetBool> BundleRewards => bundleRewards;

		public NetVector2Dictionary<int, NetInt> MuseumPieces => museumPieces;

		public NetIntDelta LostBooksFound => lostBooksFound;

		public NetInt CurrentPlayerLimit => currentPlayerLimit;

		public NetInt HighestPlayerLimit => highestPlayerLimit;

		public NetWorldState()
		{
			lostBooksFound.Minimum = 0;
			lostBooksFound.Maximum = 21;
			NetFields.AddFields(year, currentSeason, dayOfMonth, timeOfDay, whichFarm, daysPlayed, weatherForTomorrow, isRaining, isSnowing, isLightning, bloomDay, isDebrisWeather, isPaused, goblinRemoved, submarineLocked, lowestMineLevel, bundles, bundleRewards, museumPieces, worldStateIDs, uniqueIDForThisGame, currentSongIndex, lostBooksFound, highestPlayerLimit, currentPlayerLimit);
			foreach (KeyValuePair<string, string> v in Game1.content.Load<Dictionary<string, string>>("Data\\Bundles"))
			{
				bundles.Add(Convert.ToInt32(v.Key.Split('/')[1]), new NetArray<bool, NetBool>(v.Value.Split('/')[2].Split(' ').Length));
				bundleRewards.Add(Convert.ToInt32(v.Key.Split('/')[1]), new NetBool(value: false));
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
			isRaining.Value = Game1.isRaining;
			isSnowing.Value = Game1.isSnowing;
			isLightning.Value = Game1.isLightning;
			bloomDay.Value = Game1.bloomDay;
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

		public void WriteToGame1()
		{
			if (Game1.farmEvent != null)
			{
				return;
			}
			Game1.isRaining = isRaining.Value;
			Game1.isSnowing = isSnowing.Value;
			Game1.isLightning = isLightning.Value;
			Game1.bloomDay = bloomDay.Value;
			Game1.isDebrisWeather = isDebrisWeather.Value;
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
