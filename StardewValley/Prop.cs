using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	public class Prop
	{
		private Texture2D texture;

		private Rectangle sourceRect;

		private Rectangle drawRect;

		private Rectangle boundingRect;

		private bool solid;

		public Prop(Texture2D texture, int index, int tilesWideSolid, int tilesHighSolid, int tilesHighDraw, int tileX, int tileY, bool solid = true)
		{
			this.texture = texture;
			sourceRect = Game1.getSourceRectForStandardTileSheet(texture, index, 16, 16);
			sourceRect.Width = tilesWideSolid * 16;
			sourceRect.Height = tilesHighDraw * 16;
			drawRect = new Rectangle(tileX * 64, tileY * 64 + (tilesHighSolid - tilesHighDraw) * 64, tilesWideSolid * 64, tilesHighDraw * 64);
			boundingRect = new Rectangle(tileX * 64, tileY * 64, tilesWideSolid * 64, tilesHighSolid * 64);
			this.solid = solid;
		}

		public bool isColliding(Rectangle r)
		{
			if (solid)
			{
				return r.Intersects(boundingRect);
			}
			return false;
		}

		public void draw(SpriteBatch b)
		{
			drawRect.X = boundingRect.X - Game1.viewport.X;
			drawRect.Y = boundingRect.Y + (boundingRect.Height - drawRect.Height) - Game1.viewport.Y;
			b.Draw(texture, drawRect, sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, solid ? ((float)boundingRect.Y / 10000f) : 0f);
		}
	}
}
