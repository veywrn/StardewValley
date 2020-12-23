using StardewValley.Tools;

namespace StardewValley
{
	public class PanEnchantment : BaseEnchantment
	{
		public override bool CanApplyTo(Item item)
		{
			if (item is Pan)
			{
				return true;
			}
			return false;
		}
	}
}
