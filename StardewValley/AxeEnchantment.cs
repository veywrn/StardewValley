using StardewValley.Tools;

namespace StardewValley
{
	public class AxeEnchantment : BaseEnchantment
	{
		public override bool CanApplyTo(Item item)
		{
			if (item is Axe)
			{
				return true;
			}
			return false;
		}
	}
}
