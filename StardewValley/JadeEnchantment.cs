using StardewValley.Tools;

namespace StardewValley
{
	public class JadeEnchantment : BaseWeaponEnchantment
	{
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.critMultiplier.Value += 0.1f * (float)GetLevel();
			}
		}

		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				weapon.critMultiplier.Value -= 0.1f * (float)GetLevel();
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
