namespace StardewValley
{
	public class PreservingEnchantment : FishingRodEnchantment
	{
		public override string GetName()
		{
			return "Preserving";
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
