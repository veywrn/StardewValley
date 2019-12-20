using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FishPond
{
	public class FishPondReward
	{
		[ContentSerializer(Optional = true)]
		public int RequiredPopulation;

		[ContentSerializer(Optional = true)]
		public float Chance = 1f;

		public int ItemID;

		[ContentSerializer(Optional = true)]
		public int MinQuantity = 1;

		[ContentSerializer(Optional = true)]
		public int MaxQuantity = 1;
	}
}
