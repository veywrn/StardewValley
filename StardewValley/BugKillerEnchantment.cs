using StardewValley.Monsters;

namespace StardewValley
{
	public class BugKillerEnchantment : BaseWeaponEnchantment
	{
		protected override void _OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
		{
			if (monster is Grub || monster is Fly || monster is Bug || monster is Leaper || monster is LavaCrab || monster is RockCrab)
			{
				amount = (int)((float)amount * 2f);
			}
		}

		public override string GetName()
		{
			return "Bug Killer";
		}
	}
}
