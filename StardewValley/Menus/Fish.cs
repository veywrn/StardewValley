using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class Fish
	{
		public const int widthOfTrack = 1020;

		public const int msPerFrame = 65;

		public const int fishingFieldWidth = 1028;

		public const int fishingFieldHeight = 612;

		public int whichFish;

		public int indexOfAnimation;

		public int animationTimer = 65;

		public float chanceToDart;

		public float dartingRandomness;

		public float dartingIntensity;

		public float dartingDuration;

		public float dartingTimer;

		public float dartingExtraSpeed;

		public float turnFrequency;

		public float turnSpeed;

		public float turnIntensity;

		public float minSpeed;

		public float maxSpeed;

		public float speedChangeFrequency;

		public float currentSpeed;

		public float targetSpeed;

		public float positionOnTrack = 510f;

		public Vector2 position;

		public float rotation;

		public float targetRotation;

		public bool isDarting;

		public Rectangle fishingField;

		private string fishName;

		public int bobberDifficulty;

		public Fish(int whichFish)
		{
			this.whichFish = whichFish;
			fishingField = new Rectangle(0, 0, 1028, 612);
			Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			if (data.ContainsKey(whichFish))
			{
				string[] rawData = data[whichFish].Split('/');
				fishName = rawData[0];
				chanceToDart = Convert.ToInt32(rawData[1]);
				dartingRandomness = Convert.ToInt32(rawData[2]);
				dartingIntensity = Convert.ToInt32(rawData[3]);
				dartingDuration = Convert.ToInt32(rawData[4]);
				turnFrequency = Convert.ToInt32(rawData[5]);
				turnSpeed = Convert.ToInt32(rawData[6]);
				turnIntensity = Convert.ToInt32(rawData[7]);
				minSpeed = Convert.ToInt32(rawData[8]);
				maxSpeed = Convert.ToInt32(rawData[9]);
				speedChangeFrequency = Convert.ToInt32(rawData[10]);
				bobberDifficulty = Convert.ToInt32(rawData[11]);
			}
			position = new Vector2(514f, 306f);
			targetSpeed = minSpeed / 50f;
		}

		public bool isWithinRectangle(Rectangle r, int xPositionOfFishingField, int yPositionOfFishingField)
		{
			if (r.Contains((int)position.X + xPositionOfFishingField, (int)position.Y + yPositionOfFishingField))
			{
				return true;
			}
			return false;
		}

		public void Update(GameTime time)
		{
			animationTimer -= time.ElapsedGameTime.Milliseconds;
			if (animationTimer <= 0)
			{
				animationTimer = 65 - (int)(currentSpeed * 10f);
				indexOfAnimation = (indexOfAnimation + 1) % 8;
			}
			if (!isDarting && Game1.random.NextDouble() < (double)(chanceToDart / 10000f))
			{
				rotation += (float)((double)Game1.random.Next(-(int)dartingRandomness, (int)dartingRandomness) * Math.PI / 100.0);
				targetSpeed = rotation;
				dartingExtraSpeed = dartingIntensity / 20f;
				dartingExtraSpeed *= 1f + (float)Game1.random.Next(-10, 10) / 100f;
				dartingTimer = dartingDuration * 10f + (float)Game1.random.Next(-(int)dartingDuration, (int)dartingDuration) * 0.1f;
				isDarting = true;
			}
			if (dartingTimer > 0f)
			{
				dartingTimer -= time.ElapsedGameTime.Milliseconds;
				if (dartingTimer <= 0f && isDarting)
				{
					isDarting = false;
					dartingTimer = dartingDuration * 10f + (float)Game1.random.Next(-(int)dartingDuration, (int)dartingDuration) * 0.1f;
				}
				if (!isDarting)
				{
					dartingExtraSpeed -= dartingExtraSpeed * 0.0005f * (float)time.ElapsedGameTime.Milliseconds;
				}
			}
			if (Game1.random.NextDouble() < (double)(turnFrequency / 10000f))
			{
				targetRotation = (float)((double)((float)Game1.random.Next((int)(0f - turnIntensity), (int)turnIntensity) / 100f) * Math.PI);
			}
			if (Game1.random.NextDouble() < (double)(speedChangeFrequency / 10000f))
			{
				targetSpeed = (int)((float)Game1.random.Next((int)minSpeed, (int)maxSpeed) / 20f);
			}
			if (Math.Abs(rotation - targetRotation) > Math.Abs(targetRotation / (100f - turnSpeed)))
			{
				rotation += targetRotation / (100f - turnSpeed);
			}
			rotation %= (float)Math.PI * 2f;
			currentSpeed += (targetSpeed - currentSpeed) / 10f;
			currentSpeed = Math.Min(maxSpeed / 20f, currentSpeed);
			currentSpeed = Math.Max(minSpeed / 20f, currentSpeed);
			position.X += (float)((double)currentSpeed * Math.Cos(rotation));
			int wallsHit = 0;
			if (!fishingField.Contains(new Rectangle((int)position.X - 32, (int)position.Y - 32, 64, 64)))
			{
				Vector2 cartesian = new Vector2(currentSpeed * (float)Math.Cos(rotation), currentSpeed * (float)Math.Sin(rotation));
				cartesian.X = 0f - cartesian.X;
				rotation = (float)Math.Atan(cartesian.Y / cartesian.X);
				if (cartesian.X < 0f)
				{
					rotation += (float)Math.PI;
				}
				else if (cartesian.Y < 0f)
				{
					rotation += (float)Math.PI / 2f;
				}
				position.X += (float)((double)currentSpeed * Math.Cos(rotation));
				wallsHit++;
			}
			position.Y += (float)((double)currentSpeed * Math.Sin(rotation));
			if (!fishingField.Contains(new Rectangle((int)position.X - 32, (int)position.Y - 32, 64, 64)))
			{
				Vector2 cartesian2 = new Vector2(currentSpeed * (float)Math.Cos(rotation), currentSpeed * (float)Math.Sin(rotation));
				cartesian2.Y = 0f - cartesian2.Y;
				rotation = (float)Math.Atan(cartesian2.Y / cartesian2.X);
				if (cartesian2.X < 0f)
				{
					rotation += (float)Math.PI;
				}
				else if (cartesian2.Y > 0f)
				{
					rotation += (float)Math.PI / 2f;
				}
				position.Y += (float)((double)currentSpeed * Math.Sin(rotation));
				wallsHit++;
			}
			if (wallsHit >= 2)
			{
				Vector2 targetLocation = Utility.getVelocityTowardPoint(new Point((int)position.X, (int)position.Y), new Vector2(514f, 306f), currentSpeed);
				rotation = (float)Math.Atan(targetLocation.Y / targetLocation.X);
				if (targetLocation.X < 0f)
				{
					rotation += (float)Math.PI;
				}
				else if (targetLocation.Y < 0f)
				{
					rotation += (float)Math.PI / 2f;
				}
				position.X += (float)((double)currentSpeed * Math.Cos(rotation));
				position.Y += (float)((double)currentSpeed * Math.Sin(rotation));
			}
			else if (wallsHit == 1)
			{
				targetRotation = rotation;
			}
		}

		public void draw(SpriteBatch b, Vector2 positionOfFishingField)
		{
			b.Draw(Game1.mouseCursors, position + positionOfFishingField, new Rectangle(561, 1846 + indexOfAnimation * 16, 16, 16), Color.White, rotation + (float)Math.PI / 2f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 0.5f);
		}
	}
}
