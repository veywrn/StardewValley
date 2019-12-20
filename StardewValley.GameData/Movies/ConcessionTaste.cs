using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace StardewValley.GameData.Movies
{
	public class ConcessionTaste
	{
		public string Name;

		[ContentSerializer(Optional = true)]
		public List<string> LovedTags;

		[ContentSerializer(Optional = true)]
		public List<string> LikedTags;

		[ContentSerializer(Optional = true)]
		public List<string> DislikedTags;
	}
}
