using Microsoft.Xna.Framework;

namespace StardewValley
{
	public class LivestockSprite : AnimatedSprite
	{
		public LivestockSprite(string texture, int currentFrame)
			: base(texture, 0, 64, 96)
		{
		}

		public override void faceDirection(int direction)
		{
			switch (direction)
			{
			case 0:
				currentFrame = base.Texture.Width / 64 * 2 + currentFrame % (base.Texture.Width / 64);
				break;
			case 1:
				currentFrame = base.Texture.Width / 128 + currentFrame % (base.Texture.Width / 128);
				break;
			case 2:
				currentFrame %= base.Texture.Width / 64;
				break;
			case 3:
				currentFrame = base.Texture.Width / 128 * 3 + currentFrame % (base.Texture.Width / 128);
				break;
			}
			UpdateSourceRect();
		}

		public override void UpdateSourceRect()
		{
			switch (currentFrame)
			{
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
			case 21:
			case 22:
			case 23:
				break;
			case 0:
			case 1:
			case 2:
			case 3:
				base.SourceRect = new Rectangle(currentFrame * 64, 0, 64, 96);
				break;
			case 4:
			case 5:
			case 6:
			case 7:
				base.SourceRect = new Rectangle(currentFrame % 4 * 64 * 2, 96, 128, 96);
				break;
			case 8:
			case 9:
			case 10:
			case 11:
				base.SourceRect = new Rectangle(currentFrame % 4 * 64, 192, 64, 96);
				break;
			case 12:
			case 13:
			case 14:
			case 15:
				base.SourceRect = new Rectangle(currentFrame % 4 * 64 * 2, 288, 128, 96);
				break;
			case 24:
			case 25:
			case 26:
			case 27:
				base.SourceRect = new Rectangle((currentFrame - 20) * 64, 192, 64, 96);
				break;
			}
		}
	}
}
