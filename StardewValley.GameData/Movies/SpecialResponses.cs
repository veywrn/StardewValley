using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies
{
	public class SpecialResponses
	{
		[ContentSerializer(Optional = true)]
		public CharacterResponse BeforeMovie;

		[ContentSerializer(Optional = true)]
		public CharacterResponse DuringMovie;

		[ContentSerializer(Optional = true)]
		public CharacterResponse AfterMovie;
	}
}
