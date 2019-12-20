using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace StardewValley.Util
{
	public class BoundingBoxGroup
	{
		private List<Rectangle> rectangles = new List<Rectangle>();

		public bool Intersects(Rectangle rect)
		{
			foreach (Rectangle rectangle in rectangles)
			{
				if (rectangle.Intersects(rect))
				{
					return true;
				}
			}
			return false;
		}

		public bool Contains(int x, int y)
		{
			foreach (Rectangle rectangle in rectangles)
			{
				if (rectangle.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		public void Add(Rectangle rect)
		{
			if (!rectangles.Contains(rect))
			{
				rectangles.Add(rect);
			}
		}

		public void ClearNonIntersecting(Rectangle rect)
		{
			for (int i = rectangles.Count - 1; i >= 0; i--)
			{
				if (!rectangles[i].Intersects(rect))
				{
					rectangles.RemoveAt(i);
				}
			}
		}

		public void Clear()
		{
			rectangles.Clear();
		}

		public void Draw(SpriteBatch b)
		{
			foreach (Rectangle r in rectangles)
			{
				r.Offset(-Game1.viewport.X, -Game1.viewport.Y);
				b.Draw(Game1.fadeToBlackRect, r, Color.Green * 0.5f);
			}
		}

		public bool IsEmpty()
		{
			return rectangles.Count == 0;
		}
	}
}
