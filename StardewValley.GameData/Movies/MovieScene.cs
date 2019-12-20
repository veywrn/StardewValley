using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies
{
	public class MovieScene
	{
		[ContentSerializer(Optional = true)]
		public int Image = -1;

		[ContentSerializer(Optional = true)]
		public string Music = "";

		[ContentSerializer(Optional = true)]
		public string Sound = "";

		[ContentSerializer(Optional = true)]
		public int MessageDelay = 500;

		[ContentSerializer(Optional = true)]
		public string Script = "";

		[ContentSerializer(Optional = true)]
		public string Text = "";

		[ContentSerializer(Optional = true)]
		public bool Shake;

		[ContentSerializer(Optional = true)]
		public string ResponsePoint;

		public string ID = "";
	}
}
