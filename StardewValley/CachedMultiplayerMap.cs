using System.Collections.Generic;
using xTile;

namespace StardewValley
{
	public class CachedMultiplayerMap
	{
		public string mapPath;

		public HashSet<string> appliedMapOverrides;

		public Map map;

		public string loadedMapPath;
	}
}
