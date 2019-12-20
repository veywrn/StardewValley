using Microsoft.Xna.Framework;

namespace StardewValley.TerrainFeatures
{
	public class Leaf
	{
		public Vector2 position;

		public float rotation;

		public float rotationRate;

		public float yVelocity;

		public int type;

		public Leaf(Vector2 position, float rotationRate, int type, float yVelocity)
		{
			this.position = position;
			this.rotationRate = rotationRate;
			this.type = type;
			this.yVelocity = yVelocity;
		}
	}
}
