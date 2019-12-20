using Microsoft.Xna.Framework;

namespace StardewValley
{
	public struct RainDrop
	{
		public int frame;

		public int accumulator;

		public Vector2 position;

		public RainDrop(int x, int y, int frame, int accumulator)
		{
			position = new Vector2(x, y);
			this.frame = frame;
			this.accumulator = accumulator;
		}
	}
}
