using Microsoft.Xna.Framework;
using System;

namespace StardewValley.BellsAndWhistles
{
	public class EmilysParrot : TemporaryAnimatedSprite
	{
		public const int flappingPhase = 1;

		public const int hoppingPhase = 0;

		public const int lookingSidewaysPhase = 2;

		public const int nappingPhase = 3;

		public const int headBobbingPhase = 4;

		private int currentFrame;

		private int currentFrameTimer;

		private int currentPhaseTimer;

		private int currentPhase;

		private int shakeTimer;

		public EmilysParrot(Vector2 location)
		{
			texture = Game1.mouseCursors;
			sourceRect = new Rectangle(92, 148, 9, 16);
			sourceRectStartingPos = new Vector2(92f, 149f);
			position = location;
			initialPosition = position;
			scale = 4f;
			id = 5858585f;
		}

		public void doAction()
		{
			Game1.playSound("parrot");
			shakeTimer = 800;
		}

		public override bool update(GameTime time)
		{
			currentPhaseTimer -= time.ElapsedGameTime.Milliseconds;
			if (currentPhaseTimer <= 0)
			{
				currentPhase = Game1.random.Next(5);
				currentPhaseTimer = Game1.random.Next(4000, 16000);
				if (currentPhase == 1)
				{
					currentPhaseTimer /= 2;
					updateFlappingPhase();
				}
				else
				{
					position = initialPosition;
				}
			}
			if (shakeTimer > 0)
			{
				shakeIntensity = 1f;
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				shakeIntensity = 0f;
			}
			currentFrameTimer -= time.ElapsedGameTime.Milliseconds;
			if (currentFrameTimer <= 0)
			{
				switch (currentPhase)
				{
				case 0:
					if (currentFrame == 7)
					{
						currentFrame = 0;
						currentFrameTimer = 600;
					}
					else if (Game1.random.NextDouble() < 0.5)
					{
						currentFrame = 7;
						currentFrameTimer = 300;
					}
					break;
				case 4:
					if (currentFrame == 1 && Game1.random.NextDouble() < 0.1)
					{
						currentFrame = 2;
					}
					else if (currentFrame == 2)
					{
						currentFrame = 1;
					}
					else
					{
						currentFrame = Game1.random.Next(2);
					}
					currentFrameTimer = 500;
					break;
				case 3:
					if (currentFrame == 5)
					{
						currentFrame = 6;
					}
					else
					{
						currentFrame = 5;
					}
					currentFrameTimer = 1000;
					break;
				case 2:
					currentFrame = Game1.random.Next(3, 5);
					currentFrameTimer = 1000;
					break;
				case 1:
					updateFlappingPhase();
					currentFrameTimer = 0;
					break;
				}
			}
			if (currentPhase == 1 && currentFrame != 0)
			{
				sourceRect.X = 38 + currentFrame * 13;
				sourceRect.Width = 13;
			}
			else
			{
				sourceRect.X = 92 + currentFrame * 9;
				sourceRect.Width = 9;
			}
			return false;
		}

		private void updateFlappingPhase()
		{
			currentFrame = 6 - currentPhaseTimer % 1000 / 166;
			currentFrame = 3 - Math.Abs(currentFrame - 3);
			position.Y = initialPosition.Y - (float)(4 * (3 - currentFrame));
			if (currentFrame == 0)
			{
				position.X = initialPosition.X;
			}
			else
			{
				position.X = initialPosition.X - 8f;
			}
		}
	}
}
