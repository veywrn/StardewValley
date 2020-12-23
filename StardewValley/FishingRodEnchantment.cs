using StardewValley.Tools;
using System.Xml.Serialization;

namespace StardewValley
{
	[XmlInclude(typeof(FishingRodEnchantment))]
	public class FishingRodEnchantment : BaseEnchantment
	{
		public override bool CanApplyTo(Item item)
		{
			if (item is FishingRod)
			{
				return true;
			}
			return false;
		}
	}
}
