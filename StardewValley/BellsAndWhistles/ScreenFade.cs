using Microsoft.Xna.Framework;
using System;

namespace StardewValley.BellsAndWhistles
{
	public class ScreenFade
	{
		public bool globalFade;

		public bool fadeIn = true;

		public bool fadeToBlack;

		public bool nonWarpFade;

		public float fadeToBlackAlpha;

		public float globalFadeSpeed;

		private const float fadeToFudge = 0.1f;

		private Game1.afterFadeFunction afterFade;

		private Func<bool> onFadeToBlackComplete;

		private Action onFadedBackInComplete;

		public ScreenFade(Func<bool> onFadeToBlack, Action onFadeIn)
		{
			onFadeToBlackComplete = onFadeToBlack;
			onFadedBackInComplete = onFadeIn;
		}

		public bool UpdateFade(GameTime time)
		{
			if (fadeToBlack && (Game1.pauseTime == 0f || Game1.eventUp))
			{
				if (fadeToBlackAlpha > 1.1f && !Game1.messagePause)
				{
					fadeToBlackAlpha = 1f;
					if (onFadeToBlackComplete())
					{
						return true;
					}
					nonWarpFade = false;
					fadeIn = false;
					if (afterFade != null)
					{
						Game1.afterFadeFunction afterFadeFunction = afterFade;
						afterFade = null;
						afterFadeFunction();
					}
					globalFade = false;
				}
				if (fadeToBlackAlpha < -0.1f)
				{
					fadeToBlackAlpha = 0f;
					fadeToBlack = false;
					onFadedBackInComplete();
				}
				UpdateFadeAlpha(time);
			}
			return false;
		}

		public void UpdateFadeAlpha(GameTime time)
		{
			if (fadeIn)
			{
				fadeToBlackAlpha += ((Game1.eventUp || Game1.farmEvent != null) ? 0.0008f : 0.0019f) * (float)time.ElapsedGameTime.Milliseconds;
			}
			else if (!Game1.menuUp && !Game1.messagePause && !Game1.dialogueUp)
			{
				fadeToBlackAlpha -= ((Game1.eventUp || Game1.farmEvent != null) ? 0.0008f : 0.0019f) * (float)time.ElapsedGameTime.Milliseconds;
			}
		}

		public void FadeScreenToBlack(float startAlpha = 0f, bool stopMovement = true)
		{
			globalFade = false;
			fadeToBlack = true;
			fadeIn = true;
			fadeToBlackAlpha = startAlpha;
			if (stopMovement)
			{
				Game1.player.CanMove = false;
			}
		}

		public void FadeClear(float startAlpha = 1f)
		{
			globalFade = false;
			fadeIn = false;
			fadeToBlack = true;
			fadeToBlackAlpha = startAlpha;
		}

		public void GlobalFadeToBlack(Game1.afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
		{
			if (fadeToBlack && !fadeIn)
			{
				onFadedBackInComplete();
			}
			fadeToBlack = false;
			globalFade = true;
			fadeIn = false;
			this.afterFade = afterFade;
			globalFadeSpeed = fadeSpeed;
			fadeToBlackAlpha = 0f;
		}

		public void GlobalFadeToClear(Game1.afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
		{
			if (fadeToBlack && fadeIn)
			{
				onFadeToBlackComplete();
			}
			fadeToBlack = false;
			globalFade = true;
			fadeIn = true;
			this.afterFade = afterFade;
			globalFadeSpeed = fadeSpeed;
			fadeToBlackAlpha = 1f;
		}

		public void UpdateGlobalFade()
		{
			if (fadeIn)
			{
				if (fadeToBlackAlpha <= 0f)
				{
					globalFade = false;
					if (afterFade != null)
					{
						Game1.afterFadeFunction tmp2 = afterFade;
						afterFade();
						if (afterFade != null && afterFade.Equals(tmp2))
						{
							afterFade = null;
						}
						if (Game1.nonWarpFade)
						{
							fadeToBlack = false;
						}
					}
				}
				fadeToBlackAlpha = Math.Max(0f, fadeToBlackAlpha - globalFadeSpeed);
				return;
			}
			if (fadeToBlackAlpha >= 1f)
			{
				globalFade = false;
				if (afterFade != null)
				{
					Game1.afterFadeFunction tmp = afterFade;
					afterFade();
					if (afterFade != null && afterFade.Equals(tmp))
					{
						afterFade = null;
					}
					if (Game1.nonWarpFade)
					{
						fadeToBlack = false;
					}
				}
			}
			fadeToBlackAlpha = Math.Min(1f, fadeToBlackAlpha + globalFadeSpeed);
		}
	}
}
