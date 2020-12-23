using StardewValley.Tools;

namespace StardewValley
{
	public class MilkPailEnchantment : BaseEnchantment
	{
		public override bool CanApplyTo(Item item)
		{
			if (item is MilkPail)
			{
				return true;
			}
			return false;
		}
	}
}
