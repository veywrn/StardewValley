using Microsoft.Xna.Framework;

namespace StardewValley
{
	public class HaymakerEnchantment : BaseWeaponEnchantment
	{
		public override string GetName()
		{
			return "Haymaker";
		}

		protected override void _OnCutWeed(Vector2 tile_location, GameLocation location, Farmer who)
		{
			base._OnCutWeed(tile_location, location, who);
			if (Game1.random.NextDouble() < 0.5)
			{
				Game1.createItemDebris(new Object(771, 1), new Vector2(tile_location.X * 64f + 32f, tile_location.Y * 64f + 32f), -1);
			}
			if (Game1.random.NextDouble() < 0.33)
			{
				if ((Game1.getLocationFromName("Farm") as Farm).tryToAddHay(1) == 0)
				{
					TemporaryAnimatedSprite tmpSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, who.Position - new Vector2(0f, 128f), flicker: false, flipped: false, who.Position.Y / 10000f, 0.005f, Color.White, 4f, -0.005f, 0f, 0f);
					tmpSprite.motion.Y = -1f;
					tmpSprite.layerDepth = 1f - (float)Game1.random.Next(100) / 10000f;
					tmpSprite.delayBeforeAnimationStart = Game1.random.Next(350);
					Game1.multiplayer.broadcastSprites(location, tmpSprite);
					Game1.addHUDMessage(new HUDMessage("Hay", 1, add: true, Color.LightGoldenrodYellow, new Object(178, 1)));
				}
				else
				{
					Game1.createItemDebris(new Object(178, 1).getOne(), new Vector2(tile_location.X * 64f + 32f, tile_location.Y * 64f + 32f), -1);
				}
			}
		}
	}
}
