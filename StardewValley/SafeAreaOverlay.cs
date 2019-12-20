using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	public class SafeAreaOverlay : DrawableGameComponent
	{
		private SpriteBatch spriteBatch;

		private Texture2D dummyTexture;

		public SafeAreaOverlay(Game game)
			: base(game)
		{
			base.DrawOrder = 1000;
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
			dummyTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
			dummyTexture.SetData(new Color[1]
			{
				Color.White
			});
		}

		public override void Draw(GameTime gameTime)
		{
			Viewport viewport = Game1.graphics.GraphicsDevice.Viewport;
			Rectangle safeArea = viewport.GetTitleSafeArea();
			int viewportRight = viewport.X + viewport.Width;
			int viewportBottom = viewport.Y + viewport.Height;
			Rectangle leftBorder = new Rectangle(viewport.X, viewport.Y, safeArea.X - viewport.X, viewport.Height);
			Rectangle rightBorder = new Rectangle(safeArea.Right, viewport.Y, viewportRight - safeArea.Right, viewport.Height);
			Rectangle topBorder = new Rectangle(safeArea.Left, viewport.Y, safeArea.Width, safeArea.Top - viewport.Y);
			Rectangle bottomBorder = new Rectangle(safeArea.Left, safeArea.Bottom, safeArea.Width, viewportBottom - safeArea.Bottom);
			Color translucentRed = Color.Red;
			spriteBatch.Begin();
			spriteBatch.Draw(dummyTexture, leftBorder, translucentRed);
			spriteBatch.Draw(dummyTexture, rightBorder, translucentRed);
			spriteBatch.Draw(dummyTexture, topBorder, translucentRed);
			spriteBatch.Draw(dummyTexture, bottomBorder, translucentRed);
			spriteBatch.End();
		}
	}
}
