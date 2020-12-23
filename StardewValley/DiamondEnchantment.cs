namespace StardewValley
{
	public class DiamondEnchantment : BaseWeaponEnchantment
	{
		public override bool ShouldBeDisplayed()
		{
			return false;
		}

		public override bool IsForge()
		{
			return true;
		}
	}
}
