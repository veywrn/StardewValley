using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	internal class ImageCreditsBlock : ICreditsBlock
	{
		private ClickableTextureComponent clickableComponent;

		private int animationFrames;

		public ImageCreditsBlock(Texture2D texture, Rectangle sourceRect, int pixelZoom, int animationFrames)
		{
			this.animationFrames = animationFrames;
			clickableComponent = new ClickableTextureComponent(new Rectangle(0, 0, sourceRect.Width * pixelZoom, sourceRect.Height * pixelZoom), texture, sourceRect, pixelZoom);
		}

		public override void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
		{
			b.Draw(clickableComponent.texture, new Rectangle(topLeftX + widthToOccupy / 2 - clickableComponent.bounds.Width / 2, topLeftY, clickableComponent.bounds.Width, clickableComponent.bounds.Height), new Rectangle(clickableComponent.sourceRect.X + clickableComponent.sourceRect.Width * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / (double)(600 / animationFrames)), clickableComponent.sourceRect.Y, clickableComponent.sourceRect.Width, clickableComponent.sourceRect.Height), Color.White);
		}

		public override int getHeight(int maxWidth)
		{
			return clickableComponent.bounds.Height;
		}
	}
}
