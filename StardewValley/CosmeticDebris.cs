using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace StardewValley
{
	public class CosmeticDebris : TemporaryAnimatedSprite
	{
		public const float gravity = 0.3f;

		public const float bounciness = 0.45f;

		private new Vector2 position;

		private new float rotation;

		private float rotationSpeed;

		private float xVelocity;

		private float yVelocity;

		private new Rectangle sourceRect;

		private int groundYLevel;

		private int disappearTimer;

		private int lightTailLength;

		private int timeToDisappearAfterReachingGround;

		private new int id;

		private new Color color;

		private ICue tapSound;

		private new LightSource light;

		private Queue<Vector2> lightTail;

		private new Texture2D texture;

		public CosmeticDebris(Texture2D texture, Vector2 startingPosition, float rotationSpeed, float xVelocity, float yVelocity, int groundYLevel, Rectangle sourceRect, Color color, ICue tapSound, LightSource light, int lightTailLength, int disappearTime)
		{
			timeToDisappearAfterReachingGround = disappearTime;
			disappearTimer = timeToDisappearAfterReachingGround;
			this.texture = texture;
			position = startingPosition;
			this.rotationSpeed = rotationSpeed;
			this.xVelocity = xVelocity;
			this.yVelocity = yVelocity;
			this.sourceRect = sourceRect;
			this.groundYLevel = groundYLevel;
			this.color = color;
			this.tapSound = tapSound;
			this.light = light;
			id = Game1.random.Next();
			if (light != null)
			{
				light.Identifier = id;
				Game1.currentLightSources.Add(light);
			}
			if (lightTailLength > 0)
			{
				lightTail = new Queue<Vector2>();
				this.lightTailLength = lightTailLength;
			}
		}

		public override bool update(GameTime time)
		{
			if (light != null)
			{
				Utility.repositionLightSource(id, position);
			}
			yVelocity += 0.3f;
			position += new Vector2(xVelocity, yVelocity);
			rotation += rotationSpeed;
			if (position.Y >= (float)groundYLevel)
			{
				position.Y = groundYLevel - 1;
				yVelocity = 0f - yVelocity;
				yVelocity *= 0.45f;
				xVelocity *= 0.45f;
				rotationSpeed *= 0.225f;
				if (Game1.soundBank != null && !tapSound.IsPlaying)
				{
					tapSound = Game1.soundBank.GetCue(tapSound.Name);
					tapSound.Play();
				}
				disappearTimer--;
			}
			if (disappearTimer < timeToDisappearAfterReachingGround)
			{
				disappearTimer -= time.ElapsedGameTime.Milliseconds;
				if (disappearTimer <= 0)
				{
					Utility.removeLightSource(id);
					return true;
				}
			}
			return false;
		}

		public override void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
		{
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, position), sourceRect, color, rotation, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)(groundYLevel + 1) / 10000f);
		}
	}
}
