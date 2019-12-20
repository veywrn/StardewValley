namespace StardewValley.SDKs
{
	public interface SDKHelper
	{
		string Name
		{
			get;
		}

		SDKNetHelper Networking
		{
			get;
		}

		bool ConnectionFinished
		{
			get;
		}

		int ConnectionProgress
		{
			get;
		}

		void EarlyInitialize();

		void Initialize();

		void Update();

		void Shutdown();

		void DebugInfo();

		void GetAchievement(string achieve);

		void ResetAchievements();

		string FilterDirtyWords(string words);
	}
}
