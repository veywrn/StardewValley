using Microsoft.Xna.Framework;

namespace StardewValley.BellsAndWhistles
{
	public class SebsFrogs : TemporaryAnimatedSprite
	{
		private float yOriginal;

		private bool flipJump;

		public override bool update(GameTime time)
		{
			base.update(time);
			if (!pingPong && motion.Equals(Vector2.Zero) && Game1.random.NextDouble() < 0.003)
			{
				if (Game1.random.NextDouble() < 0.4)
				{
					animationLength = 3;
					pingPong = true;
				}
				else
				{
					flipJump = !flipJump;
					yOriginal = position.Y;
					motion = new Vector2((!flipJump) ? 1 : (-1), -3f);
					acceleration = new Vector2(0f, 0.2f);
					sourceRect.X = 0;
					interval = Game1.random.Next(110, 150);
					animationLength = 5;
					flipped = flipJump;
					if (base.Parent != null && base.Parent == Game1.currentLocation && Game1.random.NextDouble() < 0.03)
					{
						Game1.playSound("croak");
					}
				}
			}
			else if (pingPong && Game1.random.NextDouble() < 0.02 && sourceRect.X == 64)
			{
				animationLength = 1;
				pingPong = false;
				sourceRect.X = (int)sourceRectStartingPos.X;
			}
			if (!motion.Equals(Vector2.Zero) && position.Y > yOriginal)
			{
				motion = Vector2.Zero;
				acceleration = Vector2.Zero;
				sourceRect.X = 64;
				animationLength = 1;
				position.Y = yOriginal;
			}
			return false;
		}
	}
}
