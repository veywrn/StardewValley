using StardewValley.Tools;

namespace StardewValley
{
	public class EmeraldEnchantment : BaseWeaponEnchantment
	{
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.speed.Value += 5 * GetLevel();
			}
		}

		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.speed.Value -= 5 * GetLevel();
			}
		}

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
