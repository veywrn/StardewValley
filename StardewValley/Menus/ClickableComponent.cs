using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	public class ClickableComponent
	{
		public const int ID_ignore = -500;

		public const int CUSTOM_SNAP_BEHAVIOR = -7777;

		public const int SNAP_AUTOMATIC = -99998;

		public const int SNAP_TO_DEFAULT = -99999;

		public Rectangle bounds;

		public string name;

		public string label;

		public float scale = 1f;

		public Item item;

		public bool visible = true;

		public bool leftNeighborImmutable;

		public bool rightNeighborImmutable;

		public bool upNeighborImmutable;

		public bool downNeighborImmutable;

		public bool tryDefaultIfNoDownNeighborExists;

		public bool tryDefaultIfNoRightNeighborExists;

		public bool fullyImmutable;

		public int myID = -500;

		public int leftNeighborID = -1;

		public int rightNeighborID = -1;

		public int upNeighborID = -1;

		public int downNeighborID = -1;

		public int myAlternateID = -500;

		public int region;

		public ClickableComponent(Rectangle bounds, string name)
		{
			this.bounds = bounds;
			this.name = name;
		}

		public ClickableComponent(Rectangle bounds, string name, string label)
		{
			this.bounds = bounds;
			this.name = name;
			this.label = label;
		}

		public ClickableComponent(Rectangle bounds, Item item)
		{
			this.bounds = bounds;
			this.item = item;
		}

		public virtual bool containsPoint(int x, int y)
		{
			if (!visible)
			{
				return false;
			}
			if (bounds.Contains(x, y))
			{
				Game1.SetFreeCursorDrag();
				return true;
			}
			return false;
		}

		public virtual void snapMouseCursor()
		{
			Game1.setMousePosition(bounds.Right - bounds.Width / 8, bounds.Bottom - bounds.Height / 8);
		}

		public void snapMouseCursorToCenter()
		{
			Game1.setMousePosition(bounds.Center.X, bounds.Center.Y);
		}
	}
}
