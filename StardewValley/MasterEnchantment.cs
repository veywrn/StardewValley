namespace StardewValley
{
	public class MasterEnchantment : FishingRodEnchantment
	{
		public override string GetName()
		{
			return "Master";
		}

		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
		}

		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
		}
	}
}
