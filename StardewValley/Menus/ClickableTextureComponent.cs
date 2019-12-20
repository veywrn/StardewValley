using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Menus
{
	public class ClickableTextureComponent : ClickableComponent
	{
		public Texture2D texture;

		public Rectangle sourceRect;

		public float baseScale;

		public string hoverText = "";

		public bool drawShadow;

		public bool drawLabelWithShadow;

		public ClickableTextureComponent(string name, Rectangle bounds, string label, string hoverText, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false)
			: base(bounds, name, label)
		{
			this.texture = texture;
			if (sourceRect.Equals(Rectangle.Empty) && texture != null)
			{
				this.sourceRect = texture.Bounds;
			}
			else
			{
				this.sourceRect = sourceRect;
			}
			base.scale = scale;
			baseScale = scale;
			this.hoverText = hoverText;
			this.drawShadow = drawShadow;
			base.label = label;
		}

		public ClickableTextureComponent(Rectangle bounds, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false)
			: this("", bounds, "", "", texture, sourceRect, scale, drawShadow)
		{
		}

		public Vector2 getVector2()
		{
			return new Vector2(bounds.X, bounds.Y);
		}

		public void tryHover(int x, int y, float maxScaleIncrease = 0.1f)
		{
			if (bounds.Contains(x, y))
			{
				scale = Math.Min(scale + 0.04f, baseScale + maxScaleIncrease);
				Game1.SetFreeCursorDrag();
			}
			else
			{
				scale = Math.Max(scale - 0.04f, baseScale);
			}
		}

		public void draw(SpriteBatch b)
		{
			if (visible)
			{
				draw(b, Color.White, 0.86f + (float)bounds.Y / 20000f);
			}
		}

		public void draw(SpriteBatch b, Color c, float layerDepth, int frameOffset = 0)
		{
			if (!visible)
			{
				return;
			}
			if (texture != null)
			{
				Rectangle r = sourceRect;
				if (frameOffset != 0)
				{
					r = new Rectangle(sourceRect.X + sourceRect.Width * frameOffset, sourceRect.Y, sourceRect.Width, sourceRect.Height);
				}
				if (drawShadow)
				{
					Utility.drawWithShadow(b, texture, new Vector2((float)bounds.X + (float)(sourceRect.Width / 2) * baseScale, (float)bounds.Y + (float)(sourceRect.Height / 2) * baseScale), r, c, 0f, new Vector2(sourceRect.Width / 2, sourceRect.Height / 2), scale, flipped: false, layerDepth);
				}
				else
				{
					b.Draw(texture, new Vector2((float)bounds.X + (float)(sourceRect.Width / 2) * baseScale, (float)bounds.Y + (float)(sourceRect.Height / 2) * baseScale), r, c, 0f, new Vector2(sourceRect.Width / 2, sourceRect.Height / 2), scale, SpriteEffects.None, layerDepth);
				}
			}
			if (!string.IsNullOrEmpty(label))
			{
				if (drawLabelWithShadow)
				{
					Utility.drawTextWithShadow(b, label, Game1.smallFont, new Vector2(bounds.X + bounds.Width, (float)bounds.Y + ((float)(bounds.Height / 2) - Game1.smallFont.MeasureString(label).Y / 2f)), Game1.textColor);
				}
				else
				{
					b.DrawString(Game1.smallFont, label, new Vector2(bounds.X + bounds.Width, (float)bounds.Y + ((float)(bounds.Height / 2) - Game1.smallFont.MeasureString(label).Y / 2f)), Game1.textColor);
				}
			}
		}

		public void drawItem(SpriteBatch b, int xOffset = 0, int yOffset = 0)
		{
			if (item != null && visible)
			{
				item.drawInMenu(b, new Vector2(bounds.X + xOffset, bounds.Y + yOffset), scale / 4f);
			}
		}
	}
}
