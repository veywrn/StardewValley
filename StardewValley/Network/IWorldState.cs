using Netcode;

namespace StardewValley.Network
{
	public interface IWorldState : INetObject<NetFields>
	{
		WorldDate Date
		{
			get;
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

		NetInt HighestPlayerLimit
		{
			get;
		}

		NetInt CurrentPlayerLimit
		{
			get;
		}

		bool hasWorldStateID(string id);

		void addWorldStateID(string id);

		void removeWorldStateID(string id);

		void UpdateFromGame1();

		void WriteToGame1();
	}
}
