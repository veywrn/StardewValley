namespace StardewValley
{
	public class GalaxySoulEnchantment : BaseWeaponEnchantment
	{
		public override bool IsSecondaryEnchantment()
		{
			return true;
		}

		public override bool IsForge()
		{
			return false;
		}

		public override int GetMaximumLevel()
		{
			return 3;
		}

		public override bool ShouldBeDisplayed()
		{
			return false;
		}
	}
}
