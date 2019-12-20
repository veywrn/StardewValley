using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley
{
	public class BloomComponent : DrawableGameComponent
	{
		public enum IntermediateBuffer
		{
			PreBloom,
			BlurredHorizontally,
			BlurredBothWays,
			FinalResult
		}

		private SpriteBatch spriteBatch;

		private Effect bloomExtractEffect;

		private Effect brightWhiteEffect;

		private Effect bloomCombineEffect;

		private Effect gaussianBlurEffect;

		private RenderTarget2D sceneRenderTarget;

		private RenderTarget2D renderTarget1;

		private RenderTarget2D renderTarget2;

		public float hueShiftR;

		public float hueShiftG;

		public float hueShiftB;

		public float timeLeftForShifting;

		public float totalTime;

		public float shiftRate;

		public float offsetShift;

		public float shiftFade;

		public float blurLevel;

		public float saturationLevel;

		public float contrastLevel;

		public float bloomLevel;

		public float brightnessLevel;

		public float globalIntensity;

		public float globalIntensityMax;

		public float rabbitHoleTimer;

		private bool cyclingShift;

		private BloomSettings settings = BloomSettings.PresetSettings[5];

		private BloomSettings targetSettings = BloomSettings.PresetSettings[5];

		private BloomSettings oldSetting = BloomSettings.PresetSettings[5];

		private IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;

		public BloomSettings Settings
		{
			get
			{
				return settings;
			}
			set
			{
				settings = value;
			}
		}

		public IntermediateBuffer ShowBuffer
		{
			get
			{
				return showBuffer;
			}
			set
			{
				showBuffer = value;
			}
		}

		public BloomComponent(Game game)
			: base(game)
		{
			if (game == null)
			{
				throw new ArgumentNullException("game");
			}
		}

		public void startShifting(float howLongMilliseconds, float shiftRate, float shiftFade, float globalIntensityMax, float blurShiftLevel, float saturationShiftLevel, float contrastShiftLevel, float bloomIntensityShift, float brightnessShift, float globalIntensityStart = 1f, float offsetShift = 3000f, bool cyclingShift = true)
		{
			timeLeftForShifting = howLongMilliseconds;
			totalTime = howLongMilliseconds;
			this.shiftRate = shiftRate;
			blurLevel = blurShiftLevel;
			saturationLevel = saturationShiftLevel;
			contrastLevel = contrastShiftLevel;
			bloomLevel = bloomIntensityShift;
			brightnessLevel = brightnessShift;
			base.Visible = true;
			oldSetting = new BloomSettings("old", settings.BloomThreshold, settings.BlurAmount, settings.BloomIntensity, settings.BaseIntensity, settings.BloomSaturation, settings.BaseSaturation);
			targetSettings = new BloomSettings("old", settings.BloomThreshold, settings.BlurAmount, settings.BloomIntensity, settings.BaseIntensity, settings.BloomSaturation, settings.BaseSaturation);
			this.cyclingShift = cyclingShift;
			this.shiftFade = shiftFade;
			globalIntensity = globalIntensityStart;
			this.globalIntensityMax = globalIntensityMax / 2f;
			this.offsetShift = offsetShift;
			Game1.debugOutput = howLongMilliseconds + " " + shiftRate + " " + shiftFade + " " + globalIntensityMax + " " + blurShiftLevel + " " + saturationShiftLevel + " " + contrastShiftLevel + " " + bloomIntensityShift + " " + brightnessShift + " " + globalIntensityStart + " " + offsetShift;
			hueShiftR = 0f;
			hueShiftB = 0f;
			hueShiftG = 0f;
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(base.GraphicsDevice);
			bloomExtractEffect = Game1.content.Load<Effect>("Effects\\BloomExtract");
			bloomCombineEffect = Game1.content.Load<Effect>("Effects\\BloomCombine");
			gaussianBlurEffect = Game1.content.Load<Effect>("Effects\\GaussianBlur");
			brightWhiteEffect = Game1.content.Load<Effect>("Effects\\BrightWhite");
			PresentationParameters pp = base.GraphicsDevice.PresentationParameters;
			int width2 = pp.BackBufferWidth;
			int height2 = pp.BackBufferHeight;
			SurfaceFormat format = pp.BackBufferFormat;
			sceneRenderTarget = new RenderTarget2D(base.GraphicsDevice, width2, height2, mipMap: false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			width2 /= 2;
			height2 /= 2;
			renderTarget1 = new RenderTarget2D(base.GraphicsDevice, width2, height2, mipMap: false, format, DepthFormat.None);
			renderTarget2 = new RenderTarget2D(base.GraphicsDevice, width2, height2, mipMap: false, format, DepthFormat.None);
		}

		public void reload()
		{
			PresentationParameters pp = base.GraphicsDevice.PresentationParameters;
			int width2 = pp.BackBufferWidth;
			int height2 = pp.BackBufferHeight;
			SurfaceFormat format = pp.BackBufferFormat;
			sceneRenderTarget = new RenderTarget2D(base.GraphicsDevice, width2, height2, mipMap: false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			width2 /= 2;
			height2 /= 2;
			renderTarget1 = new RenderTarget2D(base.GraphicsDevice, width2, height2, mipMap: false, format, DepthFormat.None);
			renderTarget2 = new RenderTarget2D(base.GraphicsDevice, width2, height2, mipMap: false, format, DepthFormat.None);
		}

		protected override void UnloadContent()
		{
			sceneRenderTarget.Dispose();
			renderTarget1.Dispose();
			renderTarget2.Dispose();
		}

		public void BeginDraw()
		{
			if (base.Visible)
			{
				base.GraphicsDevice.SetRenderTarget(sceneRenderTarget);
			}
		}

		public void tick(GameTime time)
		{
			if (!(timeLeftForShifting > 0f))
			{
				return;
			}
			base.Visible = true;
			timeLeftForShifting -= time.ElapsedGameTime.Milliseconds;
			shiftRate = Math.Max(0.0001f, shiftRate + shiftFade * (float)time.ElapsedGameTime.Milliseconds);
			if (cyclingShift)
			{
				offsetShift += (float)time.ElapsedGameTime.Milliseconds / 10f;
				globalIntensity = globalIntensityMax * (float)Math.Cos(((double)timeLeftForShifting - (double)totalTime * Math.PI * 4.0) * (Math.PI * 2.0 / (double)totalTime)) + globalIntensityMax;
				float offset = offsetShift * (float)Math.Sin((double)timeLeftForShifting * (Math.PI * 2.0) / (double)totalTime);
				targetSettings.BaseSaturation = Math.Max(1f, 0.25f * globalIntensity * (saturationLevel * (float)Math.Sin((double)(timeLeftForShifting - offset / 2f) * (Math.PI * 2.0) / (double)shiftRate) + (0.25f * globalIntensity + saturationLevel)));
				targetSettings.BloomIntensity = Math.Max(0f, 0.5f * globalIntensity * (bloomLevel / 2f * (float)Math.Sin((double)(timeLeftForShifting - offset * 2f) * (Math.PI * 2.0) / (double)shiftRate) + (0.5f * globalIntensity + bloomLevel / 2f)));
				targetSettings.BlurAmount = Math.Max(0f, 1f * globalIntensity * (blurLevel * (float)Math.Sin((double)timeLeftForShifting * (Math.PI * 2.0) / (double)(shiftRate / 2f))) + (1f * globalIntensity + blurLevel));
				settings.BaseSaturation += (targetSettings.BaseSaturation - settings.BaseSaturation) / 10f;
				settings.BloomIntensity += (targetSettings.BloomIntensity - settings.BloomIntensity) / 10f;
				settings.BaseIntensity += (targetSettings.BaseIntensity - settings.BaseIntensity) / 10f;
				settings.BlurAmount += (targetSettings.BaseSaturation - settings.BlurAmount) / 10f;
				hueShiftR = globalIntensity / 2f * (float)(Math.Cos((double)(timeLeftForShifting - offset / 2f) * (Math.PI * 2.0) / (double)(shiftRate / 2f)) + 1.0) / 4f;
				hueShiftG = globalIntensity / 2f * (float)(Math.Sin((double)(timeLeftForShifting - offset / 2f) * (Math.PI * 2.0) / (double)(shiftRate / 2f)) + 1.0) / 4f;
				hueShiftB = globalIntensity / 2f * (float)(Math.Cos((double)(timeLeftForShifting - offset / 2f - totalTime / 2f) * (Math.PI * 2.0) / (double)shiftRate) + 1.0) / 4f;
				rabbitHoleTimer -= time.ElapsedGameTime.Milliseconds;
				if (rabbitHoleTimer <= 0f)
				{
					rabbitHoleTimer = 1000f;
					Console.WriteLine("timeLeft: " + timeLeftForShifting + " shiftRate: " + shiftRate + " globalIntensity: " + globalIntensity + " settings.BloomThreshold: " + settings.BloomThreshold + " settings.BaseSaturation: " + settings.BaseSaturation + " settings.BloomIntensity: " + settings.BloomIntensity + " settings.BaseIntensity: " + settings.BaseIntensity + " settings.BlurAmount: " + settings.BlurAmount + " hueShift: " + hueShiftR + "," + hueShiftG + "," + hueShiftB + " x,y: ");
				}
			}
			if (timeLeftForShifting <= 0f)
			{
				hueShiftR = 0f;
				hueShiftG = 0f;
				hueShiftB = 0f;
				settings = oldSetting;
				if (Game1.bloomDay && (bool)Game1.currentLocation.isOutdoors)
				{
					base.Visible = true;
				}
				else
				{
					base.Visible = false;
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			if (settings != null)
			{
				base.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
				if (settings.brightWhiteOnly)
				{
					DrawFullscreenQuad(sceneRenderTarget, renderTarget1, brightWhiteEffect, IntermediateBuffer.PreBloom);
				}
				else
				{
					bloomExtractEffect.Parameters["BloomThreshold"].SetValue(Settings.BloomThreshold);
					DrawFullscreenQuad(sceneRenderTarget, renderTarget1, bloomExtractEffect, IntermediateBuffer.PreBloom);
				}
				SetBlurEffectParameters(1f / (float)renderTarget1.Width, 0f);
				DrawFullscreenQuad(renderTarget1, renderTarget2, gaussianBlurEffect, IntermediateBuffer.BlurredHorizontally);
				SetBlurEffectParameters(0f, 1f / (float)renderTarget1.Height);
				DrawFullscreenQuad(renderTarget2, renderTarget1, gaussianBlurEffect, IntermediateBuffer.BlurredBothWays);
				base.GraphicsDevice.SetRenderTarget(null);
				EffectParameterCollection parameters = bloomCombineEffect.Parameters;
				parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
				parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
				parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
				parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);
				parameters["HueR"].SetValue((float)Math.Round(hueShiftR, 2));
				parameters["HueG"].SetValue((float)Math.Round(hueShiftG, 2));
				parameters["HueB"].SetValue((float)Math.Round(hueShiftB, 2));
				base.GraphicsDevice.Textures[1] = sceneRenderTarget;
				Viewport viewport = base.GraphicsDevice.Viewport;
				DrawFullscreenQuad(renderTarget1, viewport.Width, viewport.Height, bloomCombineEffect, IntermediateBuffer.FinalResult);
			}
		}

		private void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget, Effect effect, IntermediateBuffer currentBuffer)
		{
			base.GraphicsDevice.SetRenderTarget(renderTarget);
			DrawFullscreenQuad(texture, renderTarget.Width, renderTarget.Height, effect, currentBuffer);
		}

		private void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect, IntermediateBuffer currentBuffer)
		{
			if (showBuffer < currentBuffer)
			{
				effect = null;
			}
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, effect);
			spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
			spriteBatch.End();
		}

		private void SetBlurEffectParameters(float dx, float dy)
		{
			EffectParameter weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
			EffectParameter offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];
			int sampleCount = weightsParameter.Elements.Count;
			float[] sampleWeights = new float[sampleCount];
			Vector2[] sampleOffsets = new Vector2[sampleCount];
			sampleWeights[0] = ComputeGaussian(0f);
			sampleOffsets[0] = new Vector2(0f);
			float totalWeights = sampleWeights[0];
			for (int j = 0; j < sampleCount / 2; j++)
			{
				totalWeights += (sampleWeights[j * 2 + 2] = (sampleWeights[j * 2 + 1] = ComputeGaussian(j + 1))) * 2f;
				float sampleOffset = (float)(j * 2) + 1.5f;
				sampleOffsets[j * 2 + 2] = -(sampleOffsets[j * 2 + 1] = new Vector2(dx, dy) * sampleOffset);
			}
			for (int i = 0; i < sampleWeights.Length; i++)
			{
				sampleWeights[i] /= totalWeights;
			}
			weightsParameter.SetValue(sampleWeights);
			offsetsParameter.SetValue(sampleOffsets);
		}

		private float ComputeGaussian(float n)
		{
			float theta = Settings.BlurAmount;
			return (float)(1.0 / Math.Sqrt(Math.PI * 2.0 * (double)theta) * Math.Exp((0f - n * n) / (2f * theta * theta)));
		}
	}
}
