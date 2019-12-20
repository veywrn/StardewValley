using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace StardewValley.GameData.Movies
{
	public class MovieCharacterReaction
	{
		public string NPCName;

		[ContentSerializer(Optional = true)]
		public List<MovieReaction> Reactions;
	}
}
