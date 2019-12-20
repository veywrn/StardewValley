using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Locations
{
	public class Summit : GameLocation
	{
		public Summit()
		{
		}

		public Summit(string map, string name)
			: base(map, name)
		{
		}

		public override void checkForMusic(GameTime time)
		{
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (temporarySprites.Count != 0 || !(Game1.random.NextDouble() < ((Game1.timeOfDay < 1800) ? 0.0006 : ((Game1.currentSeason.Equals("summer") && Game1.dayOfMonth == 20) ? 1.0 : 0.001))))
			{
				return;
			}
			Rectangle sourceRect = Rectangle.Empty;
			Vector2 startingPosition = new Vector2(Game1.viewport.Width, Game1.random.Next(0, 200));
			float speed = -4f;
			int loops = 100;
			float animationSpeed = 100f;
			if (Game1.timeOfDay < 1800)
			{
				if (Game1.currentSeason.Equals("spring") || Game1.currentSeason.Equals("fall"))
				{
					sourceRect = new Rectangle(640, 736, 16, 16);
					int rows = Game1.random.Next(1, 4);
					speed = -1f;
					for (int j = 0; j < rows; j++)
					{
						TemporaryAnimatedSprite bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), 4, 100, startingPosition + new Vector2((j + 1) * Game1.random.Next(15, 18), (j + 1) * -20), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true);
						bird2.motion = new Vector2(-1f, 0f);
						temporarySprites.Add(bird2);
						bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), 4, 100, startingPosition + new Vector2((j + 1) * Game1.random.Next(15, 18), (j + 1) * 20), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true);
						bird2.motion = new Vector2(-1f, 0f);
						temporarySprites.Add(bird2);
					}
				}
				else if (Game1.currentSeason.Equals("summer"))
				{
					sourceRect = new Rectangle(640, 752 + ((Game1.random.NextDouble() < 0.5) ? 16 : 0), 16, 16);
					speed = -0.5f;
					animationSpeed = 150f;
				}
			}
			else if (Game1.timeOfDay >= 1900)
			{
				sourceRect = new Rectangle(640, 816, 16, 16);
				speed = -2f;
				loops = 0;
				startingPosition.X -= Game1.random.Next(64, Game1.viewport.Width);
				if (Game1.currentSeason.Equals("summer") && Game1.dayOfMonth == 20)
				{
					int numExtra = Game1.random.Next(3);
					for (int i = 0; i < numExtra; i++)
					{
						TemporaryAnimatedSprite t2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), Game1.currentSeason.Equals("winter") ? 2 : 4, loops, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true);
						t2.motion = new Vector2(speed, 0f);
						temporarySprites.Add(t2);
						startingPosition.X -= Game1.random.Next(64, Game1.viewport.Width);
						startingPosition.Y = Game1.random.Next(0, 200);
					}
				}
				else if (Game1.currentSeason.Equals("winter") && Game1.timeOfDay >= 1700 && Game1.random.NextDouble() < 0.1)
				{
					sourceRect = new Rectangle(640, 800, 32, 16);
					loops = 1000;
					startingPosition.X = Game1.viewport.Width;
				}
				else if (Game1.currentSeason.Equals("winter"))
				{
					sourceRect = Rectangle.Empty;
				}
			}
			if (Game1.timeOfDay >= 2200 && !Game1.currentSeason.Equals("winter") && Game1.currentSeason.Equals("summer") && Game1.dayOfMonth == 20 && Game1.random.NextDouble() < 0.05)
			{
				sourceRect = new Rectangle(640, 784, 16, 16);
				loops = 100;
				startingPosition.X = Game1.viewport.Width;
				speed = -3f;
			}
			if (!sourceRect.Equals(Rectangle.Empty))
			{
				TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, animationSpeed, Game1.currentSeason.Equals("winter") ? 2 : 4, loops, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true);
				t.motion = new Vector2(speed, 0f);
				temporarySprites.Add(t);
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			Game1.background = null;
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			Game1.background = new Background();
			temporarySprites.Clear();
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}
