using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class Rabbit : Critter
	{
		private int characterCheckTimer = 200;

		private bool running;

		public Rabbit(Vector2 position, bool flip)
		{
			base.position = position * 64f;
			position.Y += 48f;
			base.flip = flip;
			baseFrame = (Game1.currentSeason.Equals("winter") ? 74 : 54);
			sprite = new AnimatedSprite(Critter.critterTexture, Game1.currentSeason.Equals("winter") ? 69 : 68, 32, 32);
			sprite.loop = true;
			startingPosition = position;
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (characterCheckTimer <= 0 && !running)
			{
				if (Utility.isOnScreen(position, -32))
				{
					running = true;
					sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(baseFrame, 40),
						new FarmerSprite.AnimationFrame(baseFrame + 1, 40),
						new FarmerSprite.AnimationFrame(baseFrame + 2, 40),
						new FarmerSprite.AnimationFrame(baseFrame + 3, 100),
						new FarmerSprite.AnimationFrame(baseFrame + 5, 70),
						new FarmerSprite.AnimationFrame(baseFrame + 5, 40)
					});
					sprite.loop = true;
				}
				characterCheckTimer = 200;
			}
			if (running)
			{
				position.X += (flip ? (-6) : 6);
			}
			if (running && characterCheckTimer <= 0)
			{
				characterCheckTimer = 200;
				if (environment.largeTerrainFeatures != null)
				{
					Rectangle tileRect = new Rectangle((int)position.X + 32, (int)position.Y - 32, 4, 192);
					foreach (LargeTerrainFeature f in environment.largeTerrainFeatures)
					{
						if (f is Bush && f.getBoundingBox().Intersects(tileRect))
						{
							(f as Bush).performUseAction(f.tilePosition, environment);
							return true;
						}
					}
				}
			}
			return base.update(time, environment);
		}
	}
}
