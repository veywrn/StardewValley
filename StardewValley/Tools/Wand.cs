using Microsoft.Xna.Framework;

namespace StardewValley.Tools
{
	public class Wand : Tool
	{
		public bool charged;

		public Wand()
			: base("Return Scepter", 0, 2, 2, stackable: false)
		{
			base.UpgradeLevel = 0;
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
			base.InstantUse = true;
		}

		public override Item getOne()
		{
			return new Wand();
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wand.cs.14318");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wand.cs.14319");
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			if (!who.bathingClothes && who.IsLocalPlayer)
			{
				indexOfMenuItemView.Value = 2;
				base.CurrentParentTileIndex = 2;
				for (int j = 0; j < 12; j++)
				{
					Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.position.X - 256, (int)who.position.X + 192), Game1.random.Next((int)who.position.Y - 256, (int)who.position.Y + 192)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
				}
				location.playSound("wand");
				Game1.displayFarmer = false;
				who.temporarilyInvincible = true;
				who.temporaryInvincibilityTimer = -2000;
				who.Halt();
				who.faceDirection(2);
				who.freezePause = 1000;
				Game1.flashAlpha = 1f;
				DelayedAction.fadeAfterDelay(wandWarpForReal, 1000);
				new Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
				int i = 0;
				for (int xTile = who.getTileX() + 8; xTile >= who.getTileX() - 8; xTile--)
				{
					Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(6, new Vector2(xTile, who.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = i * 25,
						motion = new Vector2(-0.25f, 0f)
					});
					i++;
				}
				base.CurrentParentTileIndex = base.IndexOfMenuItemView;
			}
		}

		public override bool actionWhenPurchased()
		{
			Game1.player.mailReceived.Add("ReturnScepter");
			return base.actionWhenPurchased();
		}

		private void wandWarpForReal()
		{
			Game1.warpFarmer("Farm", 64, 15, flip: false);
			if (!Game1.isStartingToGetDarkOut() && !Game1.isRaining)
			{
				Game1.playMorningSong();
			}
			else
			{
				Game1.changeMusicTrack("none");
			}
			Game1.fadeToBlackAlpha = 0.99f;
			Game1.screenGlow = false;
			lastUser.temporarilyInvincible = false;
			lastUser.temporaryInvincibilityTimer = 0;
			Game1.displayFarmer = true;
		}
	}
}
