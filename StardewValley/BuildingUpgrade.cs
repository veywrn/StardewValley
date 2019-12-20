using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;

namespace StardewValley
{
	public class BuildingUpgrade
	{
		public int daysLeftTillUpgradeDone = 4;

		public string whichBuilding;

		public Vector2 positionOfCarpenter;

		[XmlIgnore]
		public Texture2D workerTexture;

		private int currentFrame = 2;

		private int numberOfHammers;

		private int currentHammer;

		private int pauseTime = 1000;

		private int hammerPause = 500;

		private float timeAccumulator;

		public BuildingUpgrade(string whichBuilding, Vector2 positionOfCarpenter)
		{
			this.whichBuilding = whichBuilding;
			this.positionOfCarpenter = positionOfCarpenter;
			this.positionOfCarpenter.X *= 64f;
			this.positionOfCarpenter.X -= 6f;
			this.positionOfCarpenter.Y *= 64f;
			this.positionOfCarpenter.Y -= 32f;
			workerTexture = Game1.content.Load<Texture2D>("LooseSprites\\robinAtWork");
		}

		public BuildingUpgrade()
		{
			workerTexture = Game1.content.Load<Texture2D>("LooseSprites\\robinAtWork");
		}

		public Rectangle getSourceRectangle()
		{
			return new Rectangle(currentFrame * 64, 0, 64, 96);
		}

		public void update(float milliseconds)
		{
			timeAccumulator += milliseconds;
			if (numberOfHammers > 0 && (timeAccumulator > (float)hammerPause || (timeAccumulator > (float)(hammerPause / 3) && currentFrame == 3)))
			{
				timeAccumulator = 0f;
				switch (currentFrame)
				{
				case 0:
					currentFrame = 1;
					Game1.playSound("woodyStep");
					break;
				case 1:
				case 2:
				case 3:
				case 4:
					currentFrame = 0;
					break;
				}
				currentHammer++;
				if (currentHammer >= numberOfHammers && currentFrame == 0)
				{
					currentHammer = 0;
					numberOfHammers = 0;
					pauseTime = Game1.random.Next(800, 3000);
					currentFrame = ((Game1.random.NextDouble() < 0.2) ? 4 : 2);
				}
			}
			else if (timeAccumulator > (float)pauseTime)
			{
				timeAccumulator = 0f;
				numberOfHammers = Game1.random.Next(2, 14);
				currentFrame = 3;
				hammerPause = Game1.random.Next(400, 700);
			}
		}
	}
}
