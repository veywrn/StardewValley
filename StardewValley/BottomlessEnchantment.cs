using StardewValley.Tools;

namespace StardewValley
{
	public class BottomlessEnchantment : WateringCanEnchantment
	{
		public override string GetName()
		{
			return "Bottomless";
		}

		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			WateringCan tool = item as WateringCan;
			if (tool != null)
			{
				tool.IsBottomless = true;
				tool.WaterLeft = tool.waterCanMax;
			}
		}

		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			WateringCan tool = item as WateringCan;
			if (tool != null)
			{
				tool.IsBottomless = false;
			}
		}
	}
}
