using StardewValley.Tools;

namespace StardewValley
{
	public class ReachingToolEnchantment : BaseEnchantment
	{
		public override string GetName()
		{
			return "Expansive";
		}

		public override bool CanApplyTo(Item item)
		{
			if (item is Tool && (item is WateringCan || item is Hoe))
			{
				return (item as Tool).UpgradeLevel == 4;
			}
			return false;
		}
	}
}
