using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class RubyEnchantment : BaseWeaponEnchantment
	{
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				string[] array = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\weapons")[weapon.InitialParentTileIndex].Split('/');
				int baseMin = Convert.ToInt32(array[2]);
				int baseMax = Convert.ToInt32(array[3]);
				weapon.minDamage.Value += Math.Max(1, (int)((float)baseMin * 0.1f)) * GetLevel();
				weapon.maxDamage.Value += Math.Max(1, (int)((float)baseMax * 0.1f)) * GetLevel();
			}
		}

		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			MeleeWeapon weapon = item as MeleeWeapon;
			if (weapon != null)
			{
				string[] array = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\weapons")[weapon.InitialParentTileIndex].Split('/');
				int baseMin = Convert.ToInt32(array[2]);
				int baseMax = Convert.ToInt32(array[3]);
				weapon.minDamage.Value -= Math.Max(1, (int)((float)baseMin * 0.1f)) * GetLevel();
				weapon.maxDamage.Value -= Math.Max(1, (int)((float)baseMax * 0.1f)) * GetLevel();
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
