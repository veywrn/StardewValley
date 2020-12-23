using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Tools;
using System;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	public class CosmeticPlant : Grass
	{
		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		[XmlElement("scale")]
		public readonly NetFloat scale = new NetFloat(1f);

		[XmlElement("xOffset")]
		private readonly NetInt xOffset = new NetInt();

		[XmlElement("yOffset")]
		private readonly NetInt yOffset = new NetInt();

		public CosmeticPlant()
		{
			initFields();
		}

		public CosmeticPlant(int which)
			: base(which, 1)
		{
			initFields();
			flipped.Value = (Game1.random.NextDouble() < 0.5);
		}

		private void initFields()
		{
			base.NetFields.AddFields(flipped, scale, xOffset, yOffset);
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return new Rectangle((int)(tileLocation.X * 64f + 16f), (int)((tileLocation.Y + 1f) * 64f - 8f - 4f), 8, 8);
		}

		public override bool seasonUpdate(bool onLoad)
		{
			return false;
		}

		public override string textureName()
		{
			return "TerrainFeatures\\upperCavePlants";
		}

		public override void loadSprite()
		{
			xOffset.Value = Game1.random.Next(-2, 3) * 4;
			yOffset.Value = Game1.random.Next(-2, 1) * 4;
		}

		public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location = null)
		{
			if ((t != null && t is MeleeWeapon && (int)((MeleeWeapon)t).type != 2) || explosion > 0)
			{
				shake((float)Math.PI * 3f / 32f, (float)Math.PI / 40f, Game1.random.NextDouble() < 0.5);
				int numberOfWeedsToDestroy2 = 0;
				numberOfWeedsToDestroy2 = ((explosion <= 0) ? (((int)t.upgradeLevel == 3) ? 3 : ((int)t.upgradeLevel + 1)) : Math.Max(1, explosion + 2 - Game1.random.Next(2)));
				Game1.createRadialDebris(location, textureName(), new Rectangle((byte)grassType * 16, 6, 7, 6), (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 14));
				numberOfWeeds.Value = (int)numberOfWeeds - numberOfWeedsToDestroy2;
				if ((int)numberOfWeeds <= 0)
				{
					Random grassRandom = new Random((int)((float)(double)Game1.uniqueIDForThisGame + tileLocation.X * 7f + tileLocation.Y * 11f + (float)Game1.CurrentMineLevel + (float)Game1.player.timesReachedMineBottom));
					if (grassRandom.NextDouble() < 0.005)
					{
						Game1.createObjectDebris(114, (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, location);
					}
					else if (grassRandom.NextDouble() < 0.01)
					{
						Game1.createDebris((grassRandom.NextDouble() < 0.5) ? 4 : 8, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(1, 2), location);
					}
					else if (grassRandom.NextDouble() < 0.02)
					{
						Game1.createDebris(92, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(2, 4), location);
					}
					return true;
				}
			}
			return false;
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f) + new Vector2(32 + (int)xOffset, 60 + (int)yOffset)), new Rectangle((byte)grassType * 16, 0, 16, 24), Color.White, shakeRotation, new Vector2(8f, 23f), 4f * (float)scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((float)(getBoundingBox(tileLocation).Y - 4) + tileLocation.X / 900f + (float)scale / 100f) / 10000f);
		}
	}
}
