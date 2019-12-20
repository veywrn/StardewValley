using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Menus
{
	public class ClickableAnimatedComponent : ClickableComponent
	{
		public TemporaryAnimatedSprite sprite;

		public Rectangle sourceRect;

		public float baseScale;

		public string hoverText = "";

		private bool drawLabel;

		public ClickableAnimatedComponent(Rectangle bounds, string name, string hoverText, TemporaryAnimatedSprite sprite, bool drawLabel)
			: base(bounds, name)
		{
			this.sprite = sprite;
			this.sprite.position = new Vector2(bounds.X, bounds.Y);
			baseScale = sprite.scale;
			this.hoverText = hoverText;
			this.drawLabel = drawLabel;
		}

		public ClickableAnimatedComponent(Rectangle bounds, string name, string hoverText, TemporaryAnimatedSprite sprite)
			: this(bounds, name, hoverText, sprite, drawLabel: true)
		{
		}

		public void update(GameTime time)
		{
			sprite.update(time);
		}

		public string tryHover(int x, int y)
		{
			if (bounds.Contains(x, y))
			{
				sprite.scale = Math.Min(sprite.scale + 0.02f, baseScale + 0.1f);
				return hoverText;
			}
			sprite.scale = Math.Max(sprite.scale - 0.02f, baseScale);
			return null;
		}

		public void draw(SpriteBatch b)
		{
			sprite.draw(b, localPosition: true);
		}
	}
}
