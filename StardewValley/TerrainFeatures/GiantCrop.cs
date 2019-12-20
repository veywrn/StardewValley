using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Tools;
using System;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	public class GiantCrop : ResourceClump
	{
		public const int cauliflower = 0;

		public const int melon = 1;

		public const int pumpkin = 2;

		[XmlElement("which")]
		public readonly NetInt which = new NetInt();

		[XmlElement("forSale")]
		public readonly NetBool forSale = new NetBool();

		public GiantCrop()
		{
			base.NetFields.AddFields(which, forSale);
		}

		public GiantCrop(int indexOfSmallerVersion, Vector2 tile)
			: this()
		{
			base.tile.Value = tile;
			parentSheetIndex.Value = indexOfSmallerVersion;
			switch (indexOfSmallerVersion)
			{
			case 190:
				which.Value = 0;
				break;
			case 254:
				which.Value = 1;
				break;
			case 276:
				which.Value = 2;
				break;
			}
			width.Value = 3;
			height.Value = 3;
			health.Value = 3f;
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			spriteBatch.Draw(Game1.cropSpriteSheet, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f - new Vector2((shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 2f) : 0f, 64f)), new Rectangle(112 + (int)which * 48, 512, 48, 63), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + 2f) * 64f / 10000f);
		}

		public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
		{
			if (t == null || !(t is Axe))
			{
				return false;
			}
			location.playSound("axchop");
			int power = t.getLastFarmerToUse().toolPower + 1;
			health.Value -= power;
			Game1.createRadialDebris(Game1.currentLocation, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 9), resource: false);
			if (shakeTimer <= 0f)
			{
				shakeTimer = 100f;
				base.NeedsUpdate = true;
			}
			if ((float)health <= 0f)
			{
				t.getLastFarmerToUse().gainExperience(5, 50 * (((int)t.getLastFarmerToUse().luckLevel + 1) / 2));
				if (t.getLastFarmerToUse().hasMagnifyingGlass)
				{
					Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
					if (o != null)
					{
						Game1.createItemDebris(o, tileLocation * 64f, -1, location);
					}
				}
				int numChunks2 = 18;
				Random r;
				if (Game1.IsMultiplayer)
				{
					Game1.recentMultiplayerRandom = new Random((int)tileLocation.X * 1000 + (int)tileLocation.Y);
					r = Game1.recentMultiplayerRandom;
				}
				else
				{
					r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
				}
				numChunks2 = r.Next(15, 22);
				if (Game1.IsMultiplayer)
				{
					Game1.createMultipleObjectDebris(parentSheetIndex, (int)tileLocation.X + 1, (int)tileLocation.Y + 1, numChunks2, t.getLastFarmerToUse().UniqueMultiplayerID, location);
				}
				else
				{
					Game1.createRadialDebris(location, parentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, numChunks2, resource: false, -1, item: true);
				}
				Object tmp = new Object(Vector2.Zero, parentSheetIndex, 1);
				Game1.setRichPresence("giantcrop", tmp.Name);
				Game1.createRadialDebris(Game1.currentLocation, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 9), resource: false);
				location.playSound("stumpCrack");
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, tileLocation * 64f, Color.White));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 0f)) * 64f, Color.White, 8, flipped: false, 110f));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 1f)) * 64f, Color.White, 8, flipped: true, 80f));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(0f, 1f)) * 64f, Color.White, 8, flipped: false, 90f));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, tileLocation * 64f + new Vector2(32f, 32f), Color.White, 8, flipped: false, 70f));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, tileLocation * 64f, Color.White));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(2f, 0f)) * 64f, Color.White, 8, flipped: false, 110f));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(2f, 1f)) * 64f, Color.White, 8, flipped: true, 80f));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(2f, 2f)) * 64f, Color.White, 8, flipped: false, 90f));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, tileLocation * 64f + new Vector2(96f, 96f), Color.White, 8, flipped: false, 70f));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(0f, 2f)) * 64f, Color.White, 8, flipped: false, 110f));
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 2f)) * 64f, Color.White, 8, flipped: true, 80f));
				return true;
			}
			return false;
		}
	}
}
