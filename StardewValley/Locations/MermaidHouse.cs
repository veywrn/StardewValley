using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class MermaidHouse : GameLocation
	{
		private Texture2D mermaidSprites;

		private float showTimer;

		private float curtainMovement;

		private float curtainOpenPercent;

		private float blackBGAlpha;

		private float bigMermaidAlpha;

		private float oldStopWatchTime;

		private float finalLeftMermaidAlpha;

		private float finalRightMermaidAlpha;

		private float finalBigMermaidAlpha;

		private float fairyTimer;

		private int[] mermaidFrames;

		private Stopwatch stopWatch;

		private List<Vector2> bubbles;

		private List<TemporaryAnimatedSprite> sparkles;

		private List<TemporaryAnimatedSprite> alwaysFrontTempSprites;

		private List<int> lastFiveClamTones;

		private Farmer pearlRecipient;

		public MermaidHouse()
		{
		}

		public MermaidHouse(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			mermaidSprites = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
			Game1.ambientLight = Color.White;
			Game1.changeMusicTrack("none");
			finalLeftMermaidAlpha = 0f;
			finalRightMermaidAlpha = 0f;
			finalBigMermaidAlpha = 0f;
			blackBGAlpha = 0f;
			bigMermaidAlpha = 0f;
			oldStopWatchTime = 0f;
			showTimer = 0f;
			curtainMovement = 0f;
			curtainOpenPercent = 0f;
			fairyTimer = 0f;
			stopWatch = new Stopwatch();
			bubbles = new List<Vector2>();
			sparkles = new List<TemporaryAnimatedSprite>();
			alwaysFrontTempSprites = new List<TemporaryAnimatedSprite>();
			lastFiveClamTones = new List<int>();
			pearlRecipient = null;
			mermaidFrames = new int[93]
			{
				1,
				0,
				2,
				0,
				1,
				0,
				2,
				0,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				4,
				4,
				3,
				3,
				3,
				3,
				0,
				0,
				0,
				0,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				3,
				4,
				3,
				3,
				4,
				4,
				3,
				3,
				3,
				3,
				0,
				0,
				0,
				0,
				3,
				3,
				3,
				3,
				4,
				4,
				4,
				4,
				3,
				3,
				3,
				3,
				0,
				0,
				5,
				6,
				5,
				6,
				7,
				8,
				8
			};
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				switch (map.GetLayer("Buildings").Tiles[tileLocation].TileIndex)
				{
				case 56:
					playClamTone(0, who);
					return true;
				case 57:
					playClamTone(1, who);
					return true;
				case 58:
					playClamTone(2, who);
					return true;
				case 59:
					playClamTone(3, who);
					return true;
				case 60:
					playClamTone(4, who);
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public void playClamTone(int which)
		{
			playClamTone(which, null);
		}

		public void playClamTone(int which, Farmer who)
		{
			if (!(oldStopWatchTime < 68000f))
			{
				ICue clamTone = Game1.soundBank.GetCue("clam_tone");
				switch (which)
				{
				case 0:
					clamTone.SetVariable("Pitch", 300);
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						color = Color.HotPink,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
						scale = 4f,
						position = new Vector2(35f, 98f) * 4f,
						interval = 1000f,
						animationLength = 1,
						alphaFade = 0.03f,
						layerDepth = 0.0001f
					});
					break;
				case 1:
					clamTone.SetVariable("Pitch", 600);
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						color = Color.Orange,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
						scale = 4f,
						position = new Vector2(51f, 98f) * 4f,
						interval = 1000f,
						animationLength = 1,
						alphaFade = 0.03f,
						layerDepth = 0.0001f
					});
					break;
				case 2:
					clamTone.SetVariable("Pitch", 800);
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						color = Color.Yellow,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
						scale = 4f,
						position = new Vector2(67f, 98f) * 4f,
						interval = 1000f,
						animationLength = 1,
						alphaFade = 0.03f,
						layerDepth = 0.0001f
					});
					break;
				case 3:
					clamTone.SetVariable("Pitch", 1000);
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						color = Color.Cyan,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
						scale = 4f,
						position = new Vector2(83f, 98f) * 4f,
						interval = 1000f,
						animationLength = 1,
						alphaFade = 0.03f,
						layerDepth = 0.0001f
					});
					break;
				case 4:
					clamTone.SetVariable("Pitch", 1200);
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						color = Color.Lime,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
						scale = 4f,
						position = new Vector2(99f, 98f) * 4f,
						interval = 1000f,
						animationLength = 1,
						alphaFade = 0.03f,
						layerDepth = 0.0001f
					});
					break;
				}
				clamTone.Play();
				lastFiveClamTones.Add(which);
				if (lastFiveClamTones.Count > 5)
				{
					lastFiveClamTones.RemoveAt(0);
				}
				if (lastFiveClamTones.Count == 5 && lastFiveClamTones[0] == 0 && lastFiveClamTones[1] == 4 && lastFiveClamTones[2] == 3 && lastFiveClamTones[3] == 1 && lastFiveClamTones[4] == 2 && who != null && !who.mailReceived.Contains("gotPearl"))
				{
					who.freezePause = 4500;
					fairyTimer = 3500f;
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						interval = 1f,
						delayBeforeAnimationStart = 885,
						texture = mermaidSprites,
						endFunction = playClamTone,
						extraInfoForEndBehavior = 0
					});
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						interval = 1f,
						delayBeforeAnimationStart = 1270,
						texture = mermaidSprites,
						endFunction = playClamTone,
						extraInfoForEndBehavior = 4
					});
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						interval = 1f,
						delayBeforeAnimationStart = 1655,
						texture = mermaidSprites,
						endFunction = playClamTone,
						extraInfoForEndBehavior = 3
					});
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						interval = 1f,
						delayBeforeAnimationStart = 2040,
						texture = mermaidSprites,
						endFunction = playClamTone,
						extraInfoForEndBehavior = 1
					});
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						interval = 1f,
						delayBeforeAnimationStart = 2425,
						texture = mermaidSprites,
						endFunction = playClamTone,
						extraInfoForEndBehavior = 2
					});
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						delayBeforeAnimationStart = 885,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
						sourceRectStartingPos = new Vector2(2f, 127f),
						scale = 4f,
						position = new Vector2(28f, 49f) * 4f,
						interval = 96f,
						animationLength = 4,
						totalNumberOfLoops = 121
					});
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						delayBeforeAnimationStart = 1270,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
						sourceRectStartingPos = new Vector2(2f, 127f),
						scale = 4f,
						position = new Vector2(108f, 49f) * 4f,
						interval = 96f,
						animationLength = 4,
						totalNumberOfLoops = 117
					});
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						delayBeforeAnimationStart = 1655,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
						sourceRectStartingPos = new Vector2(2f, 127f),
						scale = 4f,
						position = new Vector2(88f, 39f) * 4f,
						interval = 96f,
						animationLength = 4,
						totalNumberOfLoops = 113
					});
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						delayBeforeAnimationStart = 2040,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
						sourceRectStartingPos = new Vector2(2f, 127f),
						scale = 4f,
						position = new Vector2(48f, 39f) * 4f,
						interval = 96f,
						animationLength = 4,
						totalNumberOfLoops = 19
					});
					temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						delayBeforeAnimationStart = 2425,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
						sourceRectStartingPos = new Vector2(2f, 127f),
						scale = 4f,
						position = new Vector2(68f, 29f) * 4f,
						interval = 96f,
						animationLength = 4,
						totalNumberOfLoops = 15
					});
					pearlRecipient = who;
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (TemporaryAnimatedSprite sparkle in sparkles)
			{
				sparkle.draw(b, localPosition: true);
			}
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(58f, 54f) * 4f), new Microsoft.Xna.Framework.Rectangle(mermaidFrames[Math.Min((int)((float)stopWatch.ElapsedMilliseconds / 769.2308f), mermaidFrames.Length - 1)] * 28, 80, 28, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(27f, 29f) * 4f + new Vector2((float)Math.Sin((float)stopWatch.ElapsedMilliseconds / 1000f) * 4f * 4f, (float)Math.Cos((float)stopWatch.ElapsedMilliseconds / 1000f) * 4f * 4f)), new Microsoft.Xna.Framework.Rectangle(2 + (int)(showTimer % 400f / 100f) * 19, 127, 19, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(97f, 29f) * 4f + new Vector2((float)Math.Cos((float)stopWatch.ElapsedMilliseconds / 1000f + 0.1f) * 4f * 4f, (float)Math.Sin((float)stopWatch.ElapsedMilliseconds / 1000f + 0.1f) * 4f * 4f)), new Microsoft.Xna.Framework.Rectangle(2 + (int)(showTimer % 400f / 100f) * 19, 127, 19, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(16f, 16f) * 4f), new Microsoft.Xna.Framework.Rectangle((int)(144f + 57f * curtainOpenPercent), 119, (int)(57f * (1f - curtainOpenPercent)), 81), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(73f + 57f * curtainOpenPercent, 16f) * 4f), new Microsoft.Xna.Framework.Rectangle(200, 119, (int)(57f * (1f - curtainOpenPercent)), 81), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			b.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * blackBGAlpha);
			int spacing = Game1.graphics.GraphicsDevice.Viewport.Bounds.Height / 4;
			for (int i = -448; i < Game1.graphics.GraphicsDevice.Viewport.Width + 448; i += 448)
			{
				b.Draw(mermaidSprites, new Vector2(i - (int)((float)stopWatch.ElapsedMilliseconds / 6f % 448f), spacing - spacing * 3 / 4), new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48), Color.Lime * blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(mermaidSprites, new Vector2((float)(i + 112) - (float)stopWatch.ElapsedMilliseconds / 6f % 448f, (float)spacing - (float)spacing / 4f + (float)Math.Sin((float)stopWatch.ElapsedMilliseconds / 1000f) * 64f), new Microsoft.Xna.Framework.Rectangle(177, 0, 16, 16), Color.White * blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(mermaidSprites, new Vector2(i + (int)((float)stopWatch.ElapsedMilliseconds / 6f % 448f), spacing * 2 - spacing * 3 / 4), new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48), Color.Cyan * blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(mermaidSprites, new Vector2((float)(i + 112) + (float)stopWatch.ElapsedMilliseconds / 6f % 448f, (float)(spacing * 2) - (float)spacing / 4f + (float)Math.Sin((float)stopWatch.ElapsedMilliseconds / 1000f + 4f) * 64f), new Microsoft.Xna.Framework.Rectangle(161, 0, 16, 16), Color.White * blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
				b.Draw(mermaidSprites, new Vector2(i - (int)((float)stopWatch.ElapsedMilliseconds / 6f % 448f), spacing * 3 - spacing * 3 / 4), new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48), Color.Orange * blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(mermaidSprites, new Vector2((float)(i + 112) - (float)stopWatch.ElapsedMilliseconds / 6f % 448f, (float)(spacing * 3) - (float)spacing / 4f + (float)Math.Sin((float)stopWatch.ElapsedMilliseconds / 1000f + 3f) * 64f), new Microsoft.Xna.Framework.Rectangle(129, 0, 16, 16), Color.White * blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(mermaidSprites, new Vector2(i + (int)((float)stopWatch.ElapsedMilliseconds / 6f % 448f), spacing * 4 - spacing * 3 / 4), new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48), Color.HotPink * blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
				b.Draw(mermaidSprites, new Vector2((float)(i + 112) + (float)stopWatch.ElapsedMilliseconds / 6f % 448f, (float)(spacing * 4) - (float)spacing / 4f + (float)Math.Sin((float)stopWatch.ElapsedMilliseconds / 1000f + 2f) * 64f), new Microsoft.Xna.Framework.Rectangle(145, 0, 16, 16), Color.White * blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
			}
			b.Draw(mermaidSprites, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X - 112) + (float)Math.Sin((float)stopWatch.ElapsedMilliseconds / 1000f) * 64f * 2f, (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y - 140) + (float)Math.Cos((double)((float)stopWatch.ElapsedMilliseconds / 1000f * 2f) + Math.PI / 2.0) * 64f), new Microsoft.Xna.Framework.Rectangle((int)(57 * (stopWatch.ElapsedMilliseconds % 1538 / 769)), 0, 57, 70), Color.White * bigMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			foreach (TemporaryAnimatedSprite alwaysFrontTempSprite in alwaysFrontTempSprites)
			{
				alwaysFrontTempSprite.draw(b, localPosition: true);
			}
			foreach (Vector2 v in bubbles)
			{
				b.Draw(mermaidSprites, v + new Vector2((float)Math.Sin((float)stopWatch.ElapsedMilliseconds / 1000f * 4f + v.X) * 4f * 6f, 0f), new Microsoft.Xna.Framework.Rectangle(132, 20, 8, 8), Color.White * blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			}
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(-20f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(-20f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Orange * finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0011f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(-30f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(-30f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Cyan * finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0011f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(-40f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(-40f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Lime * finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0011f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(150f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(150f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Orange * finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0011f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(160f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(160f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Cyan * finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0011f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(170f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(170f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Lime * finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0011f);
			b.Draw(mermaidSprites, Game1.GlobalToLocal(new Vector2(43f, 180f) * 4f), new Microsoft.Xna.Framework.Rectangle((int)(57 * (stopWatch.ElapsedMilliseconds % 1538 / 769)), 0, 57, 70), Color.White * finalBigMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (stopWatch == null)
			{
				return;
			}
			if (!Game1.shouldTimePass())
			{
				if (stopWatch != null && stopWatch.IsRunning)
				{
					stopWatch.Stop();
				}
				if (Game1.getMusicTrackName().Equals("mermaidSong") && !Game1.currentSong.IsPaused)
				{
					Game1.currentSong.Pause();
				}
			}
			else
			{
				if (stopWatch != null && !stopWatch.IsRunning && Game1.getMusicTrackName().Equals("mermaidSong") && Game1.currentSong.IsPaused)
				{
					stopWatch.Start();
				}
				if (Game1.getMusicTrackName().Equals("mermaidSong") && Game1.currentSong.IsPaused)
				{
					Game1.currentSong.Resume();
				}
			}
			if (Game1.shouldTimePass())
			{
				float num = showTimer;
				showTimer += time.ElapsedGameTime.Milliseconds;
				if (((Game1.currentSong != null && Game1.getMusicTrackName().Equals("mermaidSong") && Game1.currentSong.IsPlaying) || (Game1.options.musicVolumeLevel <= 0f && Game1.options.ambientVolumeLevel <= 0f)) && !stopWatch.IsRunning)
				{
					stopWatch.Start();
				}
				if (curtainMovement != 0f)
				{
					curtainOpenPercent = Math.Max(0f, Math.Min(1f, curtainOpenPercent + curtainMovement * (float)time.ElapsedGameTime.Milliseconds));
				}
				if (num < 3000f && showTimer >= 3000f)
				{
					Game1.changeMusicTrack("mermaidSong");
				}
				if (stopWatch != null && stopWatch.ElapsedMilliseconds > 0 && stopWatch.ElapsedMilliseconds < 1000)
				{
					curtainMovement = 0.0004f;
				}
				for (int j = sparkles.Count - 1; j >= 0; j--)
				{
					if (sparkles[j].update(time))
					{
						sparkles.RemoveAt(j);
					}
				}
				for (int k = alwaysFrontTempSprites.Count - 1; k >= 0; k--)
				{
					if (alwaysFrontTempSprites[k].update(time))
					{
						alwaysFrontTempSprites.RemoveAt(k);
					}
				}
				if (stopWatch.ElapsedMilliseconds >= 30000 && stopWatch.ElapsedMilliseconds < 50000 && (blackBGAlpha < 1f || bigMermaidAlpha < 1f))
				{
					blackBGAlpha += 0.01f;
					bigMermaidAlpha += 0.01f;
				}
				if (stopWatch.ElapsedMilliseconds > 27692 && stopWatch.ElapsedMilliseconds < 55385)
				{
					if (oldStopWatchTime % 769f > (float)(stopWatch.ElapsedMilliseconds % 769))
					{
						bubbles.Add(new Vector2(Game1.random.Next((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel) - 64), (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel));
					}
					for (int l = 0; l < bubbles.Count; l++)
					{
						bubbles[l] = new Vector2(bubbles[l].X, bubbles[l].Y - 0.1f * (float)time.ElapsedGameTime.Milliseconds);
					}
				}
				if (oldStopWatchTime < 36923f && stopWatch.ElapsedMilliseconds >= 36923)
				{
					alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 32f,
						motion = new Vector2(0f, -4f),
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 100,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						delayBeforeAnimationStart = 0,
						initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / 4f, Game1.graphics.GraphicsDevice.Viewport.Height - 1),
						position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
						scale = 4f,
						layerDepth = 1f
					});
				}
				if (oldStopWatchTime < 40000f && stopWatch.ElapsedMilliseconds >= 40000)
				{
					alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 32f,
						motion = new Vector2(0f, -4f),
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 100,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						delayBeforeAnimationStart = 0,
						initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width * 3f / 4f, Game1.graphics.GraphicsDevice.Viewport.Height - 1),
						position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel * 3f / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
						scale = 4f,
						layerDepth = 1f
					});
				}
				if (oldStopWatchTime < 43077f && stopWatch.ElapsedMilliseconds >= 43077)
				{
					alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 32f,
						motion = new Vector2(0f, -4f),
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 100,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						delayBeforeAnimationStart = 0,
						initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / 4f, Game1.graphics.GraphicsDevice.Viewport.Height - 1),
						position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
						scale = 4f,
						layerDepth = 1f
					});
				}
				if (oldStopWatchTime < 46154f && stopWatch.ElapsedMilliseconds >= 46154)
				{
					alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
					{
						texture = mermaidSprites,
						xPeriodic = true,
						xPeriodicLoopTime = 2000f,
						xPeriodicRange = 32f,
						motion = new Vector2(0f, -4f),
						sourceRectStartingPos = new Vector2(67f, 189f),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
						totalNumberOfLoops = 100,
						animationLength = 3,
						pingPong = true,
						interval = 192f,
						delayBeforeAnimationStart = 0,
						initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width * 3f / 4f, Game1.graphics.GraphicsDevice.Viewport.Height - 1),
						position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel * 3f / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
						scale = 4f,
						layerDepth = 1f
					});
				}
				if (stopWatch.ElapsedMilliseconds >= 52308 && (blackBGAlpha > 0f || bigMermaidAlpha > 0f))
				{
					blackBGAlpha -= 0.01f;
					bigMermaidAlpha -= 0.01f;
				}
				if (stopWatch.ElapsedMilliseconds >= 58462 && stopWatch.ElapsedMilliseconds < 60000 && finalLeftMermaidAlpha < 1f)
				{
					finalLeftMermaidAlpha += 0.01f;
				}
				if (stopWatch.ElapsedMilliseconds >= 60000 && stopWatch.ElapsedMilliseconds < 62000 && finalRightMermaidAlpha < 1f)
				{
					finalRightMermaidAlpha += 0.01f;
				}
				if (stopWatch.ElapsedMilliseconds >= 61538 && stopWatch.ElapsedMilliseconds < 63538 && finalBigMermaidAlpha < 1f)
				{
					finalBigMermaidAlpha += 0.01f;
				}
				if (stopWatch.ElapsedMilliseconds >= 64615 && (finalBigMermaidAlpha < 1f || finalRightMermaidAlpha < 1f || finalLeftMermaidAlpha < 1f))
				{
					finalBigMermaidAlpha -= 0.01f;
					finalRightMermaidAlpha -= 0.01f;
					finalLeftMermaidAlpha -= 0.01f;
				}
				if (oldStopWatchTime < 64808f && stopWatch.ElapsedMilliseconds >= 64808)
				{
					for (int i = 0; i < 200; i++)
					{
						sparkles.Add(new TemporaryAnimatedSprite
						{
							texture = mermaidSprites,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 146, 16, 13),
							animationLength = 9,
							interval = 100f,
							delayBeforeAnimationStart = i * 10,
							position = Utility.getRandomPositionOnScreenNotOnMap(),
							scale = 4f
						});
					}
					Utility.addSprinklesToLocation(this, 5, 5, 9, 5, 2000, 100, Color.White);
				}
				if (oldStopWatchTime < 67500f && stopWatch.ElapsedMilliseconds >= 67500)
				{
					curtainMovement = -0.0003f;
				}
				oldStopWatchTime = stopWatch.ElapsedMilliseconds;
			}
			if (fairyTimer > 0f)
			{
				fairyTimer -= time.ElapsedGameTime.Milliseconds;
				if (fairyTimer < 200f && pearlRecipient != null && (int)pearlRecipient.facingDirection == 0)
				{
					pearlRecipient.faceDirection(1);
				}
				if (fairyTimer < 100f && pearlRecipient != null)
				{
					pearlRecipient.faceDirection(2);
				}
				if (fairyTimer <= 0f && pearlRecipient != null)
				{
					foreach (TemporaryAnimatedSprite temporarySprite in temporarySprites)
					{
						temporarySprite.alphaFade = 0.01f;
					}
					pearlRecipient.addItemByMenuIfNecessaryElseHoldUp(new Object(797, 1));
					pearlRecipient.mailReceived.Add("gotPearl");
				}
			}
		}
	}
}
