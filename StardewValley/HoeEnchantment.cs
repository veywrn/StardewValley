using StardewValley.Tools;

namespace StardewValley
{
	public class HoeEnchantment : BaseEnchantment
	{
		public override bool CanApplyTo(Item item)
		{
			if (item is Hoe)
			{
				return true;
			}
			return false;
		}
	}
}
