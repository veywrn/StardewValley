using StardewValley.Monsters;

namespace StardewValley
{
	public class CrusaderEnchantment : BaseWeaponEnchantment
	{
		protected override void _OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
		{
			if (monster is Ghost || monster is Skeleton || monster is Mummy || monster is ShadowBrute || monster is ShadowShaman || monster is ShadowGirl || monster is ShadowGuy || monster is Shooter)
			{
				amount = (int)((float)amount * 1.5f);
			}
		}

		public override string GetName()
		{
			return "Crusader";
		}
	}
}
