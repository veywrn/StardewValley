using StardewValley.Tools;

namespace StardewValley
{
	public class SwiftToolEnchantment : BaseEnchantment
	{
		public override string GetName()
		{
			return "Swift";
		}

		public override bool CanApplyTo(Item item)
		{
			if (item is Tool && !(item is MilkPail) && !(item is MeleeWeapon) && !(item is Shears) && !(item is FishingRod) && !(item is Pan) && !(item is WateringCan) && !(item is Wand))
			{
				return !(item is Slingshot);
			}
			return false;
		}

		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			Tool tool = item as Tool;
			if (tool != null)
			{
				tool.AnimationSpeedModifier = 0.66f;
			}
		}

		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			Tool tool = item as Tool;
			if (tool != null)
			{
				tool.AnimationSpeedModifier = 1f;
			}
		}
	}
}
