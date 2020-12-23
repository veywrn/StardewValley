using StardewValley.Tools;

namespace StardewValley
{
	public class EfficientToolEnchantment : BaseEnchantment
	{
		public override string GetName()
		{
			return "Efficient";
		}

		public override bool CanApplyTo(Item item)
		{
			if (item is Tool && !(item is MilkPail) && !(item is MeleeWeapon) && !(item is Shears) && !(item is Pan) && !(item is Wand))
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
				tool.IsEfficient = true;
			}
		}

		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			Tool tool = item as Tool;
			if (tool != null)
			{
				tool.IsEfficient = false;
			}
		}
	}
}
