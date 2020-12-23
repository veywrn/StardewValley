namespace StardewValley.SDKs
{
	public class NullSDKHelper : SDKHelper
	{
		public bool IsEnterButtonAssignmentFlipped => false;

		public bool IsJapaneseRegionRelease => false;

		public virtual string Name
		{
			get;
		} = "?";


		public SDKNetHelper Networking
		{
			get;
		}

		public bool ConnectionFinished
		{
			get;
		} = true;


		public int ConnectionProgress
		{
			get;
		}

		public bool HasOverlay => false;

		public void EarlyInitialize()
		{
		}

		public void Initialize()
		{
		}

		public void GetAchievement(string achieve)
		{
		}

		public void ResetAchievements()
		{
		}

		public void Update()
		{
		}

		public void Shutdown()
		{
		}

		public void DebugInfo()
		{
		}

		public string FilterDirtyWords(string words)
		{
			return words;
		}
	}
}
