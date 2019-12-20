using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies
{
	public class CharacterResponse
	{
		[ContentSerializer(Optional = true)]
		public string ResponsePoint;

		[ContentSerializer(Optional = true)]
		public string Script = "";

		[ContentSerializer(Optional = true)]
		public string Text = "";
	}
}
