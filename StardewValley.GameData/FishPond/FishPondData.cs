using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace StardewValley.GameData.FishPond
{
	public class FishPondData
	{
		public List<string> RequiredTags;

		[ContentSerializer(Optional = true)]
		public int SpawnTime = -1;

		public List<FishPondReward> ProducedItems;

		[ContentSerializer(Optional = true)]
		public Dictionary<int, List<string>> PopulationGates;
	}
}
