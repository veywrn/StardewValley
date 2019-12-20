using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.GameData.Movies
{
	public class MovieReaction
	{
		public string Tag;

		[ContentSerializer(Optional = true)]
		public string Response = "like";

		[ContentSerializer(Optional = true)]
		public List<string> Whitelist = new List<string>();

		[ContentSerializer(Optional = true)]
		public SpecialResponses SpecialResponses;

		public string ID = "";

		public bool ShouldApplyToMovie(MovieData movie_data, IEnumerable<string> movie_patrons, params string[] other_valid_tags)
		{
			if (Whitelist != null)
			{
				if (movie_patrons == null)
				{
					return false;
				}
				foreach (string required_character in Whitelist)
				{
					if (!movie_patrons.Contains(required_character))
					{
						return false;
					}
				}
			}
			if (Tag == movie_data.ID)
			{
				return true;
			}
			if (movie_data.Tags.Contains(Tag))
			{
				return true;
			}
			if (Tag == "*")
			{
				return true;
			}
			if (other_valid_tags.Contains(Tag))
			{
				return true;
			}
			return false;
		}
	}
}
