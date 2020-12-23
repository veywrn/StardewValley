using Netcode;
using System.Collections.Generic;

namespace StardewValley.Network
{
	public interface IWorldState : INetObject<NetFields>
	{
		WorldDate Date
		{
			get;
		}

		bool IsTimePaused
		{
			get;
			set;
		}

		bool IsPaused
		{
			get;
			set;
		}

		bool IsGoblinRemoved
		{
			get;
			set;
		}

		bool IsSubmarineLocked
		{
			get;
			set;
		}

		int MinesDifficulty
		{
			get;
			set;
		}

		int SkullCavesDifficulty
		{
			get;
			set;
		}

		int LowestMineLevelForOrder
		{
			get;
			set;
		}

		int LowestMineLevel
		{
			get;
			set;
		}

		int WeatherForTomorrow
		{
			get;
			set;
		}

		Dictionary<string, string> BundleData
		{
			get;
		}

		NetBundles Bundles
		{
			get;
		}

		NetIntDictionary<bool, NetBool> BundleRewards
		{
			get;
		}

		NetVector2Dictionary<int, NetInt> MuseumPieces
		{
			get;
		}

		NetIntDelta LostBooksFound
		{
			get;
		}

		NetIntDelta GoldenWalnuts
		{
			get;
		}

		NetIntDelta GoldenWalnutsFound
		{
			get;
		}

		NetIntDelta MiniShippingBinsObtained
		{
			get;
		}

		NetBool GoldenCoconutCracked
		{
			get;
		}

		NetBool ParrotPlatformsUnlocked
		{
			get;
		}

		NetStringDictionary<bool, NetBool> FoundBuriedNuts
		{
			get;
		}

		NetStringDictionary<bool, NetBool> IslandVisitors
		{
			get;
		}

		NetIntDictionary<LocationWeather, NetRef<LocationWeather>> LocationWeather
		{
			get;
		}

		int VisitsUntilY1Guarantee
		{
			get;
			set;
		}

		Game1.MineChestType ShuffleMineChests
		{
			get;
			set;
		}

		NetInt HighestPlayerLimit
		{
			get;
		}

		NetInt CurrentPlayerLimit
		{
			get;
		}

		NetRef<Object> DishOfTheDay
		{
			get;
		}

		void RegisterSpecialCurrencies();

		LocationWeather GetWeatherForLocation(GameLocation.LocationContext location_context);

		Dictionary<string, string> GetUnlocalizedBundleData();

		void SetBundleData(Dictionary<string, string> data);

		bool hasWorldStateID(string id);

		void addWorldStateID(string id);

		void removeWorldStateID(string id);

		void UpdateFromGame1();

		void WriteToGame1();
	}
}
