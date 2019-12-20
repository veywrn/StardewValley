using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace StardewValley.GameData.Crafting
{
	public class TailorItemRecipe
	{
		[ContentSerializer(Optional = true)]
		public List<string> FirstItemTags;

		[ContentSerializer(Optional = true)]
		public List<string> SecondItemTags;

		[ContentSerializer(Optional = true)]
		public bool SpendRightItem = true;

		[ContentSerializer(Optional = true)]
		public int CraftedItemID;

		[ContentSerializer(Optional = true)]
		public List<string> CraftedItemIDs;

		[ContentSerializer(Optional = true)]
		public string CraftedItemColor = "255 255 255";
	}
}
