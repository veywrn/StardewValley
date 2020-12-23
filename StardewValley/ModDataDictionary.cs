using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	public class ModDataDictionary : NetStringDictionary<string, NetString>
	{
		public ModDataDictionary()
		{
			InterpolationWait = false;
		}

		public virtual void SetFromSerialization(ModDataDictionary source)
		{
			Clear();
			if (source != null)
			{
				foreach (string key in source.Keys)
				{
					base[key] = source[key];
				}
			}
		}

		public ModDataDictionary GetForSerialization()
		{
			if (Game1.game1 != null && Game1.game1.IsSaving && Count() == 0)
			{
				return null;
			}
			return this;
		}
	}
}
