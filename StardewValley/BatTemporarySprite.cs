using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	public class BatTemporarySprite : TemporaryAnimatedSprite
	{
		public new Texture2D texture;

		private bool moveLeft;

		private int horizontalSpeed;

		private float verticalSpeed;

		public BatTemporarySprite(Vector2 position)
			: base(-666, 100f, 4, 99999, position, flicker: false, flipped: false)
		{
			texture = Game1.content.Load<Texture2D>("LooseSprites\\Bat");
			currentParentTileIndex = 0;
			if (position.X > (float)(Game1.currentLocation.Map.DisplayWidth / 2))
			{
				moveLeft = true;
			}
			horizontalSpeed = Game1.random.Next(1, 8);
			verticalSpeed = Game1.random.Next(3, 7);
			interval = 160f - ((float)horizontalSpeed + verticalSpeed) * 10f;
		}

		public override void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
		{
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, base.Position), new Rectangle(currentParentTileIndex * 64, 0, 64, 64), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (base.Position.Y + 32f) / 10000f);
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
					currentParentTileIndex = 0;
				}
			}
			if (moveLeft)
			{
				position.X -= horizontalSpeed;
			}
			else
			{
				position.X += horizontalSpeed;
			}
			position.Y += verticalSpeed;
			verticalSpeed -= 0.1f;
			if (position.Y >= (float)Game1.currentLocation.Map.DisplayHeight || position.Y < 0f || position.X < 0f || position.X >= (float)Game1.currentLocation.Map.DisplayWidth)
			{
				return true;
			}
			return false;
		}
	}
}
