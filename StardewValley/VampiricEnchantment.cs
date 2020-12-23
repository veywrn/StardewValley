using Microsoft.Xna.Framework;
using StardewValley.Monsters;
using System;

namespace StardewValley
{
	public class VampiricEnchantment : BaseWeaponEnchantment
	{
		protected override void _OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
		{
		}

		protected override void _OnMonsterSlay(Monster m, GameLocation location, Farmer who)
		{
			if (Game1.random.NextDouble() < 0.090000003576278687)
			{
				int amount = Math.Max(1, (int)((float)(m.MaxHealth + Game1.random.Next(-m.MaxHealth / 10, m.MaxHealth / 15 + 1)) * 0.1f));
				who.health = Math.Min(who.maxHealth, Game1.player.health + amount);
				location.debris.Add(new Debris(amount, new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()), Color.Lime, 1f, who));
				Game1.playSound("healSound");
			}
		}

		public override string GetName()
		{
			return "Vampiric";
		}
	}
}
