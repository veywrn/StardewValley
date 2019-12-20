using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace StardewValley.GameData.Movies
{
	public class ConcessionItemData
	{
		public int ID;

		public string Name;

		public string DisplayName;

		public string Description;

		public int Price;

		[ContentSerializer(Optional = true)]
		public List<string> ItemTags;
	}
}
