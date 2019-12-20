using Microsoft.Xna.Framework;
using System;

namespace StardewValley.Locations
{
	public class WizardHouse : GameLocation
	{
		private int cauldronTimer = 250;

		public WizardHouse()
		{
		}

		public WizardHouse(string m, string name)
			: base(m, name)
		{
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (!wasUpdated)
			{
				base.UpdateWhenCurrentLocation(time);
				cauldronTimer -= time.ElapsedGameTime.Milliseconds;
				if (cauldronTimer <= 0)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(3f, 20f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.Lime)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -0.5f),
						acceleration = new Vector2(-0.002f, 0f),
						interval = 99999f,
						layerDepth = 0.144f - (float)Game1.random.Next(100) / 10000f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
					});
					cauldronTimer = 100;
				}
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			Game1.changeMusicTrack("none");
			base.cleanupBeforePlayerExit();
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(276, 1985, 12, 11), new Vector2(10f, 12f) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				light = true,
				lightRadius = 2f,
				scale = 4f
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(276, 1985, 12, 11), new Vector2(2f, 21f) * 64f + new Vector2(51f, 32f), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				light = true,
				lightRadius = 1f,
				scale = 2f
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(276, 1985, 12, 11), new Vector2(3f, 21f) * 64f + new Vector2(16f, 32f), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				light = true,
				lightRadius = 1f,
				scale = 3f
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(276, 1985, 12, 11), new Vector2(4f, 21f) * 64f + new Vector2(-16f, 32f), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				light = true,
				lightRadius = 1f,
				scale = 2f
			});
			if (Game1.player.eventsSeen.Contains(418172))
			{
				setMapTileIndex(2, 12, 2143, "Front", 1);
			}
		}
	}
}
