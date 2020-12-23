using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.TerrainFeatures;
using System;

namespace StardewValley.Projectiles
{
	public class DebuffingProjectile : Projectile
	{
		private readonly NetInt debuff = new NetInt();

		public DebuffingProjectile()
		{
			base.NetFields.AddField(debuff);
		}

		public DebuffingProjectile(int debuff, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, GameLocation location = null, Character owner = null)
			: this()
		{
			theOneWhoFiredMe.Set(location, owner);
			this.debuff.Value = debuff;
			currentTileSheetIndex.Value = parentSheetIndex;
			bouncesLeft.Value = bouncesTillDestruct;
			base.tailLength.Value = tailLength;
			base.rotationVelocity.Value = rotationVelocity;
			base.xVelocity.Value = xVelocity;
			base.yVelocity.Value = yVelocity;
			position.Value = startingPosition;
			if (location == null)
			{
				Game1.playSound("debuffSpell");
			}
			else
			{
				location.playSound("debuffSpell");
			}
		}

		public override void updatePosition(GameTime time)
		{
			position.X += xVelocity;
			position.Y += yVelocity;
			position.X += (float)Math.Sin((double)time.TotalGameTime.Milliseconds * Math.PI / 128.0) * 8f;
			position.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 128.0) * 8f;
		}

		public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
		{
			if (Game1.random.Next(11) >= player.immunity && !player.hasBuff(28))
			{
				if (Game1.player == player)
				{
					Game1.buffsDisplay.addOtherBuff(new Buff(debuff));
				}
				explosionAnimation(location);
				if ((int)debuff == 19)
				{
					location.playSound("frozen");
				}
				else
				{
					location.playSound("debuffHit");
				}
			}
		}

		public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
		{
			explosionAnimation(location);
		}

		public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
		{
			explosionAnimation(Game1.mine);
		}

		public override void behaviorOnCollisionWithOther(GameLocation location)
		{
			explosionAnimation(location);
		}

		private void explosionAnimation(GameLocation location)
		{
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(352, Game1.random.Next(100, 150), 2, 1, position, flicker: false, flipped: false));
		}

		public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
		{
		}
	}
}
