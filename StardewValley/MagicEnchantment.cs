using Microsoft.Xna.Framework;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;

namespace StardewValley
{
	public class MagicEnchantment : BaseWeaponEnchantment
	{
		protected override void _OnSwing(MeleeWeapon weapon, Farmer farmer)
		{
			base._OnSwing(weapon, farmer);
			Vector2 shot_velocity = default(Vector2);
			Vector2 shot_origin = farmer.getStandingPosition() - new Vector2(32f, 32f);
			float rotation_velocity2 = 0f;
			switch (farmer.facingDirection.Value)
			{
			case 0:
				shot_velocity.Y = -1f;
				break;
			case 1:
				shot_velocity.X = 1f;
				break;
			case 3:
				shot_velocity.X = -1f;
				break;
			case 2:
				shot_velocity.Y = 1f;
				break;
			}
			rotation_velocity2 = 32f;
			shot_velocity *= 10f;
			BasicProjectile projectile = new BasicProjectile((int)Math.Ceiling((float)weapon.minDamage.Value / 4f), 11, 0, 1, rotation_velocity2 * ((float)Math.PI / 180f), shot_velocity.X, shot_velocity.Y, shot_origin, "", "", explode: false, damagesMonsters: true, farmer.currentLocation, farmer);
			projectile.ignoreTravelGracePeriod.Value = true;
			projectile.ignoreMeleeAttacks.Value = true;
			projectile.maxTravelDistance.Value = 256;
			projectile.height.Value = 32f;
			farmer.currentLocation.projectiles.Add(projectile);
		}

		public override string GetName()
		{
			return "Starburst";
		}
	}
}
