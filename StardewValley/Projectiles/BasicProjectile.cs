using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace StardewValley.Projectiles
{
	public class BasicProjectile : Projectile
	{
		public delegate void onCollisionBehavior(GameLocation location, int xPosition, int yPosition, Character who);

		public readonly NetInt damageToFarmer = new NetInt();

		private readonly NetString collisionSound = new NetString();

		private readonly NetBool explode = new NetBool();

		private onCollisionBehavior collisionBehavior;

		public BasicProjectile()
		{
			base.NetFields.AddFields(damageToFarmer, collisionSound, explode);
		}

		public BasicProjectile(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, Character firer = null, bool spriteFromObjectSheet = false, onCollisionBehavior collisionBehavior = null)
			: this()
		{
			this.damageToFarmer.Value = damageToFarmer;
			currentTileSheetIndex.Value = parentSheetIndex;
			bouncesLeft.Value = bouncesTillDestruct;
			base.tailLength.Value = tailLength;
			base.rotationVelocity.Value = rotationVelocity;
			base.xVelocity.Value = xVelocity;
			base.yVelocity.Value = yVelocity;
			position.Value = startingPosition;
			if (firingSound != null && !firingSound.Equals(""))
			{
				location?.playSound(firingSound);
			}
			this.explode.Value = explode;
			this.collisionSound.Value = collisionSound;
			base.damagesMonsters.Value = damagesMonsters;
			theOneWhoFiredMe.Set(location, firer);
			base.spriteFromObjectSheet.Value = spriteFromObjectSheet;
			this.collisionBehavior = collisionBehavior;
		}

		public BasicProjectile(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition)
			: this(damageToFarmer, parentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, "flameSpellHit", "flameSpell", explode: true)
		{
		}

		public override void updatePosition(GameTime time)
		{
			position.X += xVelocity;
			position.Y += yVelocity;
		}

		public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
		{
			if (!damagesMonsters)
			{
				player.takeDamage(damageToFarmer, overrideParry: false, null);
				explosionAnimation(location);
			}
		}

		public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
		{
			t.performUseAction(tileLocation, location);
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

		public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
		{
			if (!damagesMonsters)
			{
				return;
			}
			explosionAnimation(location);
			if (n is Monster)
			{
				location.damageMonster(n.GetBoundingBox(), damageToFarmer, (int)damageToFarmer + 1, isBomb: false, (theOneWhoFiredMe.Get(location) is Farmer) ? (theOneWhoFiredMe.Get(location) as Farmer) : Game1.player);
				return;
			}
			n.getHitByPlayer((theOneWhoFiredMe.Get(location) == null || !(theOneWhoFiredMe.Get(location) is Farmer)) ? Game1.player : (theOneWhoFiredMe.Get(location) as Farmer), location);
			string projectileName = "";
			if (Game1.objectInformation.ContainsKey(currentTileSheetIndex.Value))
			{
				projectileName = Game1.objectInformation[currentTileSheetIndex.Value].Split('/')[4];
			}
			else if (Game1.objectInformation.ContainsKey(currentTileSheetIndex.Value - 1))
			{
				projectileName = Game1.objectInformation[currentTileSheetIndex.Value - 1].Split('/')[4];
			}
			Game1.multiplayer.globalChatInfoMessage("Slingshot_Hit", ((theOneWhoFiredMe.Get(location) == null || !(theOneWhoFiredMe.Get(location) is Farmer)) ? Game1.player : (theOneWhoFiredMe.Get(location) as Farmer)).Name, (n.Name == null) ? "???" : n.Name, Lexicon.prependArticle(projectileName));
		}

		private void explosionAnimation(GameLocation location)
		{
			Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(spriteFromObjectSheet ? Game1.objectSpriteSheet : Projectile.projectileSheet, currentTileSheetIndex);
			sourceRect.X += 28;
			sourceRect.Y += 28;
			sourceRect.Width = 8;
			sourceRect.Height = 8;
			int whichDebris = 12;
			switch (currentTileSheetIndex.Value)
			{
			case 390:
				whichDebris = 14;
				break;
			case 378:
				whichDebris = 0;
				break;
			case 380:
				whichDebris = 2;
				break;
			case 384:
				whichDebris = 6;
				break;
			case 386:
				whichDebris = 10;
				break;
			case 382:
				whichDebris = 4;
				break;
			}
			if ((bool)spriteFromObjectSheet)
			{
				Game1.createRadialDebris(location, whichDebris, (int)(position.X + 32f) / 64, (int)(position.Y + 32f) / 64, 6, resource: false);
			}
			else
			{
				Game1.createRadialDebris(location, "TileSheets\\Projectiles", sourceRect, 4, (int)position.X + 32, (int)position.Y + 32, 12, (int)(position.Y / 64f) + 1);
			}
			if (collisionSound.Value != null && !collisionSound.Value.Equals(""))
			{
				location.playSound(collisionSound);
			}
			if ((bool)explode)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, position, flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
			}
			if (collisionBehavior != null)
			{
				collisionBehavior(location, getBoundingBox().Center.X, getBoundingBox().Center.Y, theOneWhoFiredMe.Get(location));
			}
			destroyMe = true;
		}

		public static void explodeOnImpact(GameLocation location, int x, int y, Character who)
		{
			location.explode(new Vector2(x / 64, y / 64), 2, (who is Farmer) ? ((Farmer)who) : null);
		}
	}
}
