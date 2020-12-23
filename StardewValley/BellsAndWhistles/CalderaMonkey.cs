using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class CalderaMonkey : Critter
	{
		private const int phase_tailBOB = 0;

		private const int phase_footPaddle = 1;

		private const int phase_relaxing = 2;

		private const int phase_scream = 3;

		public Rectangle movementRectangle;

		private int currentPhase;

		private int currentFrame;

		private float nextFrameTimer;

		private float nextPhaseTimer;

		private float currentFrameDelay;

		protected Rectangle _baseSourceRectangle = new Rectangle(0, 309, 20, 24);

		protected Vector2 movementDirection = Vector2.Zero;

		private List<Vector2> buddies = new List<Vector2>();

		private Texture2D texture;

		private Texture2D swimShadow;

		public CalderaMonkey()
		{
			sprite = new AnimatedSprite(Critter.critterTexture, 0, 18, 18);
			sprite.SourceRect = _baseSourceRectangle;
			sprite.ignoreSourceRectUpdates = true;
			texture = Game1.temporaryContent.Load<Texture2D>(Critter.critterTexture);
			swimShadow = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\swimShadow");
		}

		public CalderaMonkey(Vector2 start_position)
			: this()
		{
			position = start_position;
			sprite = new AnimatedSprite(Critter.critterTexture, 0, 18, 18);
			sprite.SourceRect = _baseSourceRectangle;
			sprite.ignoreSourceRectUpdates = true;
			if (Game1.random.NextDouble() < 0.5)
			{
				buddies.Add(new Vector2(-96f, 76.8f) + position);
			}
			if (Game1.random.NextDouble() < 0.5)
			{
				buddies.Add(new Vector2(32f, 134.4f) + position);
			}
			if (Game1.random.NextDouble() < 0.5)
			{
				buddies.Add(new Vector2(128f, 44.8f) + position);
			}
			texture = Game1.temporaryContent.Load<Texture2D>(Critter.critterTexture);
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			nextFrameTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (nextPhaseTimer >= 0f)
			{
				nextPhaseTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
				if (nextPhaseTimer <= 0f)
				{
					if (currentPhase != 3 || !(Game1.random.NextDouble() < 0.2))
					{
						currentPhase = Game1.random.Next(4);
					}
					nextFrameTimer = 0f;
					switch (currentPhase)
					{
					case 0:
						currentFrameDelay = Game1.random.Next(400, 500);
						nextPhaseTimer = Game1.random.Next(3000, 8000);
						break;
					case 1:
						currentFrameDelay = Game1.random.Next(300, 1200);
						nextPhaseTimer = Game1.random.Next(3000, 6000);
						break;
					case 3:
						nextPhaseTimer = Game1.random.Next(700, 3000);
						nextFrameTimer = 400f;
						if (Game1.activeClickableMenu == null)
						{
							environment.playSound("monkey1");
						}
						setFrame(5);
						break;
					case 2:
						nextPhaseTimer = Game1.random.Next(3000, 8000);
						break;
					}
				}
			}
			switch (currentPhase)
			{
			case 0:
				if (nextFrameTimer <= 0f)
				{
					if (currentFrame == 0)
					{
						setFrame(1);
					}
					else
					{
						setFrame(0);
					}
					if (Game1.random.NextDouble() < 0.2)
					{
						setFrame(6);
						nextFrameTimer = 200f;
					}
					else
					{
						nextFrameTimer = currentFrameDelay;
					}
				}
				break;
			case 1:
				if (nextFrameTimer <= 0f)
				{
					if (currentFrame == 2)
					{
						setFrame(3);
					}
					else
					{
						setFrame(2);
					}
					nextFrameTimer = currentFrameDelay;
					if (Game1.activeClickableMenu == null)
					{
						environment.playSound("slosh");
					}
					environment.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 3, 0, position + new Vector2((currentFrame == 2) ? 32 : (-8), 48f), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.02f, Color.White, 0.75f, 0.003f, 0f, 0f));
				}
				break;
			case 2:
				setFrame(4);
				break;
			case 3:
				if (nextFrameTimer <= 0f)
				{
					setFrame(0);
				}
				break;
			}
			return base.update(time, environment);
		}

		private void setFrame(int frame)
		{
			sprite.sourceRect.X = frame * 20;
			currentFrame = frame;
		}

		public override void draw(SpriteBatch b)
		{
			sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(base.position + new Vector2(0f, -20f + yJumpOffset + base.yOffset))), (base.position.Y + 64f - 32f) / 10000f, 0, 0, Color.White, flip, 4f);
			for (int i = 0; i < buddies.Count; i++)
			{
				float yOffset = (float)Math.Sin((double)(float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 500.0 + (double)(i * 100)) * 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, buddies[i]);
				b.Draw(texture, position + new Vector2(0f, yOffset), new Rectangle(14 * i, 333, 14, 12 - (int)yOffset / 2), Color.White, 0f, Vector2.Zero, 4f, (position.X > 1408f) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, buddies[i].Y / 10000f);
				b.Draw(Game1.staminaRect, new Rectangle((int)position.X + (int)yOffset + 8, (int)position.Y + 44 + 2, 56 - (int)yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, new Color(255, 255, 150) * 0.55f, 0f, Vector2.Zero, SpriteEffects.None, buddies[i].Y / 10000f + 0.001f);
				b.Draw(swimShadow, position + new Vector2(-4f, 48f), new Rectangle((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 700 / 70 * 16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, buddies[i].Y / 10000f - 0.001f);
			}
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
		}
	}
}
