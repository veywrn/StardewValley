using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class AnimatedSprite : INetObject<NetFields>
	{
		public delegate void endOfAnimationBehavior(Farmer who);

		public Texture2D spriteTexture;

		public string loadedTexture;

		public readonly NetString textureName = new NetString();

		public float timer;

		public float interval = 175f;

		public int framesPerAnimation = 4;

		public int currentFrame;

		public readonly NetInt spriteWidth = new NetInt(16);

		public readonly NetInt spriteHeight = new NetInt(24);

		public int tempSpriteHeight = -1;

		public Rectangle sourceRect;

		public bool loop = true;

		public bool ignoreStopAnimation;

		public bool textureUsesFlippedRightForLeft;

		public endOfAnimationBehavior endOfAnimationFunction;

		public readonly List<FarmerSprite.AnimationFrame> currentAnimation = new List<FarmerSprite.AnimationFrame>(12);

		public int oldFrame;

		public int currentAnimationIndex;

		protected ContentManager contentManager;

		public bool ignoreSourceRectUpdates;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public Texture2D Texture
		{
			get
			{
				loadTexture();
				return spriteTexture;
			}
		}

		protected int textureWidth
		{
			get
			{
				if (Texture != null)
				{
					return Texture.Width;
				}
				return 96;
			}
		}

		protected int textureHeight
		{
			get
			{
				if (Texture != null)
				{
					return Texture.Height;
				}
				return 128;
			}
		}

		public int SpriteWidth
		{
			get
			{
				return spriteWidth.Get();
			}
			set
			{
				spriteWidth.Value = value;
			}
		}

		public int SpriteHeight
		{
			get
			{
				if (tempSpriteHeight != -1)
				{
					return tempSpriteHeight;
				}
				return spriteHeight.Get();
			}
			set
			{
				spriteHeight.Value = value;
				tempSpriteHeight = -1;
			}
		}

		public virtual int CurrentFrame
		{
			get
			{
				return currentFrame;
			}
			set
			{
				currentFrame = value;
				UpdateSourceRect();
			}
		}

		public List<FarmerSprite.AnimationFrame> CurrentAnimation
		{
			get
			{
				if (currentAnimation.Count == 0)
				{
					return null;
				}
				return currentAnimation;
			}
			set
			{
				currentAnimation.Clear();
				if (value != null)
				{
					currentAnimation.AddRange(value);
				}
			}
		}

		public Rectangle SourceRect
		{
			get
			{
				return sourceRect;
			}
			set
			{
				sourceRect = value;
			}
		}

		public AnimatedSprite()
		{
			initNetFields();
			contentManager = Game1.content;
		}

		public AnimatedSprite(ContentManager contentManager, string textureName, int currentFrame, int spriteWidth, int spriteHeight)
			: this()
		{
			this.contentManager = contentManager;
			this.currentFrame = currentFrame;
			SpriteWidth = spriteWidth;
			SpriteHeight = spriteHeight;
			LoadTexture(textureName);
		}

		public AnimatedSprite(ContentManager contentManager, string textureName)
			: this()
		{
			this.contentManager = contentManager;
			LoadTexture(textureName);
		}

		public AnimatedSprite(string textureName, int currentFrame, int spriteWidth, int spriteHeight)
			: this(Game1.content, textureName, currentFrame, spriteWidth, spriteHeight)
		{
		}

		public AnimatedSprite(string textureName)
			: this(Game1.content, textureName)
		{
		}

		protected virtual void initNetFields()
		{
			NetFields.AddFields(textureName, spriteWidth, spriteHeight);
		}

		public void LoadTexture(string textureName)
		{
			this.textureName.Value = textureName;
			loadTexture();
		}

		private void loadTexture()
		{
			if (!(loadedTexture == textureName.Value))
			{
				if (textureName.Value == null)
				{
					spriteTexture = null;
				}
				else
				{
					spriteTexture = contentManager.Load<Texture2D>(textureName.Value);
				}
				loadedTexture = textureName.Value;
				if (spriteTexture != null)
				{
					UpdateSourceRect();
				}
			}
		}

		public int getHeight()
		{
			return SpriteHeight;
		}

		public int getWidth()
		{
			return SpriteWidth;
		}

		public virtual void StopAnimation()
		{
			if (ignoreStopAnimation)
			{
				return;
			}
			if (CurrentAnimation != null)
			{
				CurrentAnimation = null;
				currentFrame = oldFrame;
				UpdateSourceRect();
				return;
			}
			if (this is FarmerSprite && currentFrame >= 232)
			{
				currentFrame -= 8;
			}
			if (currentFrame >= 64 && currentFrame <= 155)
			{
				currentFrame = (currentFrame - currentFrame % (textureWidth / SpriteWidth)) % 32 + 96;
			}
			else if (textureUsesFlippedRightForLeft && currentFrame >= textureWidth / SpriteWidth * 3)
			{
				if (currentFrame == 14 && textureWidth / SpriteWidth == 4)
				{
					currentFrame = 4;
				}
			}
			else
			{
				currentFrame = (currentFrame - currentFrame % (textureWidth / SpriteWidth)) % 32;
			}
			UpdateSourceRect();
		}

		public virtual void standAndFaceDirection(int direction)
		{
			switch (direction)
			{
			case 0:
				currentFrame = 12;
				break;
			case 1:
				currentFrame = 6;
				break;
			case 2:
				currentFrame = 0;
				break;
			case 3:
				currentFrame = 6;
				break;
			}
			UpdateSourceRect();
		}

		public void faceDirectionStandard(int direction)
		{
			switch (direction)
			{
			case 0:
				direction = 2;
				break;
			case 2:
				direction = 0;
				break;
			}
			currentFrame = direction * 4;
			UpdateSourceRect();
		}

		public virtual void faceDirection(int direction)
		{
			if (!ignoreStopAnimation && CurrentAnimation == null)
			{
				try
				{
					switch (direction)
					{
					case 0:
						currentFrame = textureWidth / SpriteWidth * 2 + currentFrame % (textureWidth / SpriteWidth);
						break;
					case 1:
						currentFrame = textureWidth / SpriteWidth + currentFrame % (textureWidth / SpriteWidth);
						break;
					case 2:
						currentFrame %= textureWidth / SpriteWidth;
						break;
					case 3:
						if (textureUsesFlippedRightForLeft)
						{
							currentFrame = textureWidth / SpriteWidth + currentFrame % (textureWidth / SpriteWidth);
						}
						else
						{
							currentFrame = textureWidth / SpriteWidth * 3 + currentFrame % (textureWidth / SpriteWidth);
						}
						break;
					}
				}
				catch (Exception)
				{
				}
				UpdateSourceRect();
			}
		}

		public void AnimateRight(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
		{
			if (currentFrame >= framesPerAnimation * 2 || currentFrame < framesPerAnimation)
			{
				currentFrame = framesPerAnimation + currentFrame % framesPerAnimation;
			}
			timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (timer > interval + (float)intervalOffset)
			{
				currentFrame++;
				timer = 0f;
				if (currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
				{
					Game1.playSound(soundForFootstep);
				}
				if (currentFrame >= framesPerAnimation * 2 && loop)
				{
					currentFrame = framesPerAnimation;
				}
			}
			UpdateSourceRect();
		}

		public void AnimateUp(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
		{
			if (currentFrame >= framesPerAnimation * 3 || currentFrame < framesPerAnimation * 2)
			{
				currentFrame = framesPerAnimation * 2 + currentFrame % framesPerAnimation;
			}
			timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (timer > interval + (float)intervalOffset)
			{
				currentFrame++;
				timer = 0f;
				if (currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
				{
					Game1.playSound(soundForFootstep);
				}
				if (currentFrame >= framesPerAnimation * 3 && loop)
				{
					currentFrame = framesPerAnimation * 2;
				}
			}
			UpdateSourceRect();
		}

		public void AnimateDown(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
		{
			if (currentFrame >= framesPerAnimation || currentFrame < 0)
			{
				currentFrame %= framesPerAnimation;
			}
			timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (timer > interval + (float)intervalOffset)
			{
				currentFrame++;
				timer = 0f;
				if (currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
				{
					Game1.playSound(soundForFootstep);
				}
				if (currentFrame >= framesPerAnimation && loop)
				{
					currentFrame = 0;
				}
			}
			UpdateSourceRect();
		}

		public void AnimateLeft(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
		{
			if (currentFrame >= framesPerAnimation * 4 || currentFrame < framesPerAnimation * 3)
			{
				currentFrame = framesPerAnimation * 3 + currentFrame % framesPerAnimation;
			}
			timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (timer > interval + (float)intervalOffset)
			{
				currentFrame++;
				timer = 0f;
				if (currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
				{
					Game1.playSound(soundForFootstep);
				}
				if (currentFrame >= framesPerAnimation * 4 && loop)
				{
					currentFrame = framesPerAnimation * 3;
				}
			}
			UpdateSourceRect();
		}

		public bool Animate(GameTime gameTime, int startFrame, int numberOfFrames, float interval)
		{
			if (currentFrame >= startFrame + numberOfFrames || currentFrame < startFrame)
			{
				currentFrame = startFrame + currentFrame % numberOfFrames;
			}
			timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (timer > interval)
			{
				currentFrame++;
				timer = 0f;
				if (currentFrame >= startFrame + numberOfFrames)
				{
					if (loop)
					{
						currentFrame = startFrame;
					}
					UpdateSourceRect();
					return true;
				}
			}
			UpdateSourceRect();
			return false;
		}

		public bool AnimateBackwards(GameTime gameTime, int startFrame, int numberOfFrames, float interval)
		{
			if (currentFrame >= startFrame + numberOfFrames || currentFrame < startFrame)
			{
				currentFrame = startFrame + currentFrame % numberOfFrames;
			}
			timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (timer > interval)
			{
				currentFrame--;
				timer = 0f;
				if (currentFrame <= startFrame - numberOfFrames)
				{
					if (loop)
					{
						currentFrame = startFrame;
					}
					UpdateSourceRect();
					return true;
				}
			}
			UpdateSourceRect();
			return false;
		}

		public virtual void setCurrentAnimation(List<FarmerSprite.AnimationFrame> animation)
		{
			currentAnimation.Clear();
			currentAnimation.AddRange(animation);
			oldFrame = currentFrame;
			currentAnimationIndex = 0;
			if (CurrentAnimation.Count > 0)
			{
				timer = CurrentAnimation[0].milliseconds;
				currentFrame = CurrentAnimation[0].frame;
			}
		}

		public bool animateOnce(GameTime time)
		{
			if (CurrentAnimation != null)
			{
				timer -= time.ElapsedGameTime.Milliseconds;
				if (timer <= 0f)
				{
					if (CurrentAnimation[currentAnimationIndex].frameEndBehavior != null)
					{
						CurrentAnimation[currentAnimationIndex].frameEndBehavior(null);
						if (CurrentAnimation == null)
						{
							currentFrame = oldFrame;
							CurrentAnimation = null;
							UpdateSourceRect();
							return true;
						}
					}
					currentAnimationIndex++;
					if (currentAnimationIndex >= CurrentAnimation.Count)
					{
						if (!loop)
						{
							currentFrame = oldFrame;
							CurrentAnimation = null;
							UpdateSourceRect();
							return true;
						}
						currentAnimationIndex = 0;
					}
					if (CurrentAnimation[currentAnimationIndex].frameStartBehavior != null)
					{
						CurrentAnimation[currentAnimationIndex].frameStartBehavior(null);
					}
					if (CurrentAnimation != null)
					{
						timer = CurrentAnimation[currentAnimationIndex].milliseconds;
						currentFrame = CurrentAnimation[currentAnimationIndex].frame;
					}
				}
				UpdateSourceRect();
				return false;
			}
			UpdateSourceRect();
			return true;
		}

		public virtual void UpdateSourceRect()
		{
			if (!ignoreSourceRectUpdates)
			{
				int SpriteWidth = this.SpriteWidth;
				int SpriteHeight = this.SpriteHeight;
				SourceRect = new Rectangle(currentFrame * SpriteWidth % Texture.Width, currentFrame * SpriteWidth / Texture.Width * SpriteHeight, SpriteWidth, SpriteHeight);
			}
		}

		public void draw(SpriteBatch b, Vector2 screenPosition, float layerDepth)
		{
			if (Texture != null)
			{
				b.Draw(Texture, screenPosition, sourceRect, Color.White, 0f, Vector2.Zero, 4f, (CurrentAnimation != null && CurrentAnimation[currentAnimationIndex].flip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
			}
		}

		public void draw(SpriteBatch b, Vector2 screenPosition, float layerDepth, int xOffset, int yOffset, Color c, bool flip = false, float scale = 1f, float rotation = 0f, bool characterSourceRectOffset = false)
		{
			if (Texture != null)
			{
				b.Draw(Texture, screenPosition, new Rectangle(sourceRect.X + xOffset, sourceRect.Y + yOffset, sourceRect.Width, sourceRect.Height), c, rotation, characterSourceRectOffset ? new Vector2(SpriteWidth / 2, (float)SpriteHeight * 3f / 4f) : Vector2.Zero, scale, (flip || (CurrentAnimation != null && CurrentAnimation[currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
			}
		}

		public void drawShadow(SpriteBatch b, Vector2 screenPosition, float scale = 4f, float alpha = 1f)
		{
			b.Draw(Game1.shadowTexture, screenPosition + new Vector2((float)(SpriteWidth / 2 * 4) - scale, (float)(SpriteHeight * 4) - scale), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, Utility.PointToVector2(Game1.shadowTexture.Bounds.Center), scale, SpriteEffects.None, 1E-05f);
		}

		public void drawShadow(SpriteBatch b, Vector2 screenPosition, float scale = 4f)
		{
			drawShadow(b, screenPosition, scale, 1f);
		}

		public AnimatedSprite Clone()
		{
			AnimatedSprite animatedSprite = new AnimatedSprite();
			animatedSprite.spriteWidth.Set(spriteWidth.Value);
			animatedSprite.spriteHeight.Set(spriteHeight.Value);
			animatedSprite.spriteTexture = spriteTexture;
			animatedSprite.loadedTexture = loadedTexture;
			animatedSprite.textureName.Set(textureName.Value);
			animatedSprite.timer = timer;
			animatedSprite.interval = interval;
			animatedSprite.framesPerAnimation = framesPerAnimation;
			animatedSprite.currentFrame = currentFrame;
			animatedSprite.tempSpriteHeight = tempSpriteHeight;
			animatedSprite.sourceRect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
			animatedSprite.loop = loop;
			animatedSprite.ignoreStopAnimation = ignoreStopAnimation;
			animatedSprite.textureUsesFlippedRightForLeft = textureUsesFlippedRightForLeft;
			animatedSprite.CurrentAnimation = CurrentAnimation;
			animatedSprite.oldFrame = oldFrame;
			animatedSprite.currentAnimationIndex = currentAnimationIndex;
			animatedSprite.contentManager = contentManager;
			animatedSprite.UpdateSourceRect();
			return animatedSprite;
		}
	}
}
