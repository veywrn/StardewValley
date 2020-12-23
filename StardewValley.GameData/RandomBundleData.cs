using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace StardewValley.GameData
{
	public class RandomBundleData
	{
		public string AreaName = "";

		public string Keys = "";

		[ContentSerializer(Optional = true)]
		public List<BundleSetData> BundleSets = new List<BundleSetData>();

		[ContentSerializer(Optional = true)]
		public List<BundleData> Bundles = new List<BundleData>();
	}
}
