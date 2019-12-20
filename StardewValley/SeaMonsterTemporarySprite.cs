using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	public class SeaMonsterTemporarySprite : TemporaryAnimatedSprite
	{
		public new Texture2D texture;

		public SeaMonsterTemporarySprite(float animationInterval, int animationLength, int numberOfLoops, Vector2 position)
			: base(-666, animationInterval, animationLength, numberOfLoops, position, flicker: false, flipped: false)
		{
			texture = Game1.content.Load<Texture2D>("LooseSprites\\SeaMonster");
			Game1.playSound("pullItemFromWater");
			currentParentTileIndex = 0;
		}

		public override void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
		{
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, base.Position), new Rectangle(currentParentTileIndex * 16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 32f) / 10000f);
		}

		public override bool update(GameTime time)
		{
			timer += time.ElapsedGameTime.Milliseconds;
			if (timer > interval)
			{
				currentParentTileIndex++;
				timer = 0f;
				if (currentParentTileIndex >= animationLength)
				{
					currentNumberOfLoops++;
					currentParentTileIndex = 2;
				}
			}
			if (currentNumberOfLoops >= totalNumberOfLoops)
			{
				position.Y += 2f;
				if (position.Y >= (float)Game1.currentLocation.Map.DisplayHeight)
				{
					return true;
				}
			}
			return false;
		}
	}
}
