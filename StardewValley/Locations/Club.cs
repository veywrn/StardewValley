using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Locations
{
	public class Club : GameLocation
	{
		public static int timesPlayedCalicoJack;

		public static int timesPlayedSlots;

		private string coinBuffer;

		public Club()
		{
		}

		public Club(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			lightGlows.Clear();
			coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? "     " : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  "));
		}

		public override void checkForMusic(GameTime time)
		{
			if (Game1.random.NextDouble() < 0.002)
			{
				localSound("boop");
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			Game1.changeMusicTrack("none");
			base.cleanupBeforePlayerExit();
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			SpriteText.drawStringWithScrollBackground(b, coinBuffer + Game1.player.clubCoins, 64, 16);
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(68f, 20f), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
		}
	}
}
