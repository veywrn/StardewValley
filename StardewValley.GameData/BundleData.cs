using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData
{
	public class BundleData
	{
		public string Name;

		public int Index;

		public string Sprite;

		public string Color;

		public string Items;

		[ContentSerializer(Optional = true)]
		public int Pick = -1;

		[ContentSerializer(Optional = true)]
		public int RequiredItems = -1;

		public string Reward;
	}
}
