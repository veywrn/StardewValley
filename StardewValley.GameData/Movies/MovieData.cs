using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace StardewValley.GameData.Movies
{
	public class MovieData
	{
		[ContentSerializer(Optional = true)]
		public string ID;

		public int SheetIndex;

		public string Title;

		public string Description;

		[ContentSerializer(Optional = true)]
		public List<string> Tags;

		public List<MovieScene> Scenes;
	}
}
