using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace StardewValley.Minigames
{
	public class GrandpaStory : IMinigame
	{
		public const int sceneWidth = 1294;

		public const int sceneHeight = 730;

		public const int scene_beforeGrandpa = 0;

		public const int scene_grandpaSpeech = 1;

		public const int scene_fadeOutFromGrandpa = 2;

		public const int scene_timePass = 3;

		public const int scene_jojaCorpOverhead = 4;

		public const int scene_jojaCorpPan = 5;

		public const int scene_desk = 6;

		private LocalizedContentManager content;

		private Texture2D texture;

		private float foregroundFade;

		private float backgroundFade;

		private float foregroundFadeChange;

		private float backgroundFadeChange;

		private float panX;

		private float letterScale = 0.5f;

		private float letterDy;

		private float letterDyDy;

		private int scene;

		private int totalMilliseconds;

		private int grandpaSpeechTimer;

		private int parallaxPan;

		private int letterOpenTimer;

		private bool drawGrandpa;

		private bool letterReceived;

		private bool mouseActive;

		private bool clickedLetter;

		private bool quit;

		private bool fadingToQuit;

		private Queue<string> grandpaSpeech;

		private Vector2 letterPosition = new Vector2(477f, 345f);

		private LetterViewerMenu letterView;

		public GrandpaStory()
		{
			Game1.changeMusicTrack("none");
			content = Game1.content.CreateTemporary();
			texture = content.Load<Texture2D>("Minigames\\jojacorps");
			backgroundFadeChange = 0.0003f;
			grandpaSpeech = new Queue<string>();
			grandpaSpeech.Enqueue(Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12026") : Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12028"));
			grandpaSpeech.Enqueue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12029"));
			grandpaSpeech.Enqueue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12030"));
			grandpaSpeech.Enqueue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12031"));
			grandpaSpeech.Enqueue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12034"));
			grandpaSpeech.Enqueue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12035"));
			grandpaSpeech.Enqueue(Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12036") : Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12038"));
			grandpaSpeech.Enqueue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12040"));
			Game1.player.Position = new Vector2(panX, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 360) + new Vector2(3000f, 376f);
			Game1.viewport.X = 0;
			Game1.viewport.Y = 0;
			Game1.currentLocation = new FarmHouse("Maps\\FarmHouse", "FarmHouse");
			Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
			Game1.player.currentLocation = Game1.currentLocation;
		}

		public bool tick(GameTime time)
		{
			if (quit)
			{
				unload();
				Game1.currentMinigame = new Intro();
				return false;
			}
			if (letterView != null)
			{
				letterView.update(time);
			}
			totalMilliseconds += time.ElapsedGameTime.Milliseconds;
			totalMilliseconds %= 9000000;
			backgroundFade += backgroundFadeChange * (float)time.ElapsedGameTime.Milliseconds;
			backgroundFade = Math.Max(0f, Math.Min(1f, backgroundFade));
			foregroundFade += foregroundFadeChange * (float)time.ElapsedGameTime.Milliseconds;
			foregroundFade = Math.Max(0f, Math.Min(1f, foregroundFade));
			int old = grandpaSpeechTimer;
			if (foregroundFade >= 1f && fadingToQuit)
			{
				unload();
				Game1.currentMinigame = new Intro();
				return false;
			}
			switch (scene)
			{
			case 0:
				if (backgroundFade == 1f)
				{
					if (!drawGrandpa)
					{
						foregroundFade = 1f;
						foregroundFadeChange = -0.0005f;
						drawGrandpa = true;
					}
					if (foregroundFade == 0f)
					{
						scene = 1;
						Game1.changeMusicTrack("grandpas_theme");
					}
				}
				break;
			case 1:
				grandpaSpeechTimer += time.ElapsedGameTime.Milliseconds;
				if (grandpaSpeechTimer >= 60000)
				{
					foregroundFadeChange = 0.0005f;
				}
				if (foregroundFade >= 1f)
				{
					drawGrandpa = false;
					scene = 3;
					grandpaSpeechTimer = 0;
					foregroundFade = 0f;
					foregroundFadeChange = 0f;
				}
				if (old % 10000 > grandpaSpeechTimer % 10000 && grandpaSpeech.Count > 0)
				{
					grandpaSpeech.Dequeue();
				}
				if (old < 25000 && grandpaSpeechTimer > 25000 && grandpaSpeech.Count > 0)
				{
					grandpaSpeech.Dequeue();
				}
				if (old < 17000 && grandpaSpeechTimer >= 17000)
				{
					Game1.playSound("newRecipe");
					letterReceived = true;
					letterDy = -0.6f;
					letterDyDy = 0.001f;
				}
				if (letterReceived && letterPosition.Y <= (float)Game1.viewport.Height)
				{
					letterDy += letterDyDy * (float)time.ElapsedGameTime.Milliseconds;
					letterPosition.Y += letterDy * (float)time.ElapsedGameTime.Milliseconds;
					letterPosition.X += 0.01f * (float)time.ElapsedGameTime.Milliseconds;
					letterScale += 0.00125f * (float)time.ElapsedGameTime.Milliseconds;
					if (letterPosition.Y > (float)Game1.viewport.Height)
					{
						Game1.playSound("coin");
					}
				}
				break;
			case 3:
				grandpaSpeechTimer += time.ElapsedGameTime.Milliseconds;
				if (grandpaSpeechTimer > 2600 && old <= 2600)
				{
					Game1.changeMusicTrack("jojaOfficeSoundscape");
				}
				else if (grandpaSpeechTimer > 4000)
				{
					grandpaSpeechTimer = 0;
					scene = 4;
				}
				break;
			case 4:
				grandpaSpeechTimer += time.ElapsedGameTime.Milliseconds;
				if (grandpaSpeechTimer >= 9000)
				{
					grandpaSpeechTimer = 0;
					scene = 5;
					Game1.player.faceDirection(1);
					Game1.player.currentEyes = 1;
				}
				if (grandpaSpeechTimer >= 7000)
				{
					Game1.viewport.X = 0;
					Game1.viewport.Y = 0;
					panX -= 0.2f * (float)time.ElapsedGameTime.Milliseconds;
					Game1.player.Position = new Vector2(panX, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 360) + new Vector2(3612f, 572f);
				}
				break;
			case 5:
				if (panX > (float)(-4800 + Math.Max(1600, Game1.viewport.Width)))
				{
					Game1.viewport.X = 0;
					Game1.viewport.Y = 0;
					panX -= 0.2f * (float)time.ElapsedGameTime.Milliseconds;
					Game1.player.Position = new Vector2(panX, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel / 2f - 360f) + new Vector2(3612f, 572f);
					break;
				}
				grandpaSpeechTimer += time.ElapsedGameTime.Milliseconds;
				if (old < 2000 && grandpaSpeechTimer >= 2000)
				{
					Game1.player.currentEyes = 4;
				}
				if (old < 3000 && grandpaSpeechTimer >= 3000)
				{
					Game1.player.currentEyes = 1;
					Game1.player.jitterStrength = 1f;
				}
				if (old < 3500 && grandpaSpeechTimer >= 3500)
				{
					Game1.player.stopJittering();
				}
				if (old < 4000 && grandpaSpeechTimer >= 4000)
				{
					Game1.player.currentEyes = 1;
					Game1.player.jitterStrength = 1f;
				}
				if (old < 4500 && grandpaSpeechTimer >= 4500)
				{
					Game1.player.stopJittering();
					Game1.player.doEmote(28);
				}
				if (old < 7000 && grandpaSpeechTimer >= 7000)
				{
					Game1.player.currentEyes = 4;
				}
				if (old < 8000 && grandpaSpeechTimer >= 8000)
				{
					Game1.player.showFrame(33);
				}
				if (grandpaSpeechTimer >= 10000)
				{
					scene = 6;
					grandpaSpeechTimer = 0;
				}
				Game1.player.Position = new Vector2(panX, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel / 2f - 360f) + new Vector2(3612f, 572f);
				break;
			case 6:
				grandpaSpeechTimer += time.ElapsedGameTime.Milliseconds;
				if (grandpaSpeechTimer >= 2000)
				{
					parallaxPan += (int)Math.Ceiling(0.1 * (double)time.ElapsedGameTime.Milliseconds);
					if (parallaxPan >= 107)
					{
						parallaxPan = 107;
					}
				}
				if (old < 3500 && grandpaSpeechTimer >= 3500)
				{
					Game1.changeMusicTrack("none");
				}
				if (old < 5000 && grandpaSpeechTimer >= 5000)
				{
					Game1.playSound("doorCreak");
				}
				if (old < 6000 && grandpaSpeechTimer >= 6000)
				{
					mouseActive = true;
					Point pos = clickableGrandpaLetterRect().Center;
					Game1.setMousePositionRaw((int)((float)pos.X * Game1.options.zoomLevel), (int)((float)pos.Y * Game1.options.zoomLevel));
				}
				if (clickedLetter)
				{
					letterOpenTimer += time.ElapsedGameTime.Milliseconds;
				}
				break;
			}
			Game1.player.updateEmote(time);
			if (Game1.player.jitterStrength > 0f)
			{
				Game1.player.jitter = new Vector2((float)Game1.random.Next(-(int)(Game1.player.jitterStrength * 100f), (int)((Game1.player.jitterStrength + 1f) * 100f)) / 100f, (float)Game1.random.Next(-(int)(Game1.player.jitterStrength * 100f), (int)((Game1.player.jitterStrength + 1f) * 100f)) / 100f);
			}
			return false;
		}

		public void afterFade()
		{
		}

		private Rectangle clickableGrandpaLetterRect()
		{
			return new Rectangle((int)Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730).X + (286 - parallaxPan) * 4, (int)Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730).Y + 218 + Math.Max(0, Math.Min(60, (grandpaSpeechTimer - 5000) / 8)), 524, 344);
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!clickedLetter && mouseActive && (clickableGrandpaLetterRect().Contains(x, y) || Game1.options.SnappyMenus))
			{
				clickedLetter = true;
				Game1.playSound("newRecipe");
				Game1.changeMusicTrack("musicboxsong");
				letterView = new LetterViewerMenu(Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12051", Game1.player.Name, Game1.player.farmName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12055", Game1.player.Name, Game1.player.farmName));
				letterView.exitFunction = onLetterExit;
			}
			if (letterView != null)
			{
				letterView.receiveLeftClick(x, y);
			}
		}

		public void onLetterExit()
		{
			mouseActive = false;
			foregroundFadeChange = 0.0003f;
			fadingToQuit = true;
			if (letterView != null)
			{
				letterView.unload();
				letterView = null;
			}
			Game1.playSound("newRecipe");
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void releaseLeftClick(int x, int y)
		{
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void receiveKeyPress(Keys k)
		{
			if (k == Keys.Escape || Game1.options.doesInputListContain(Game1.options.menuButton, k))
			{
				if (!quit && !fadingToQuit)
				{
					Game1.playSound("bigDeSelect");
				}
				if (letterView != null)
				{
					letterView.unload();
					letterView = null;
				}
				quit = true;
			}
			else if (letterView != null)
			{
				letterView.receiveKeyPress(k);
				if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightTrigger) && !Game1.oldPadState.IsButtonDown(Buttons.RightTrigger))
				{
					letterView.receiveGamePadButton(Buttons.RightTrigger);
				}
				if (Game1.input.GetGamePadState().IsButtonDown(Buttons.LeftTrigger) && Game1.oldPadState.IsButtonUp(Buttons.LeftTrigger))
				{
					letterView.receiveGamePadButton(Buttons.LeftTrigger);
				}
			}
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(64, 136, 248));
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * backgroundFade);
			if (drawGrandpa)
			{
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730), new Rectangle(427, (totalMilliseconds % 300 < 150) ? 240 : 0, 427, 240), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(317f, 74f) * 3f, new Rectangle(427 + 74 * (totalMilliseconds % 400 / 100), 480, 74, 42), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(320f, 75f) * 3f, new Rectangle(427, 522, 70, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				if (grandpaSpeechTimer > 8000 && grandpaSpeechTimer % 10000 < 5000)
				{
					b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(189f, 69f) * 3f, new Rectangle(497 + 18 * (totalMilliseconds % 400 / 200), 523, 18, 18), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				}
				if (grandpaSpeech.Count > 0 && grandpaSpeechTimer > 3000)
				{
					float textScale = 1f;
					string text = grandpaSpeech.Peek();
					Vector2 textSize = Game1.dialogueFont.MeasureString(text);
					textSize *= textScale;
					float shadowOffsetX = 3f * textScale;
					Vector2 textPos = new Vector2((float)(Game1.viewport.Width / 2) - textSize.X / 2f, (float)((int)Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730).Y + 669) + 3f);
					textPos.X -= shadowOffsetX;
					b.DrawString(Game1.dialogueFont, text, textPos, Color.White * 0.25f, 0f, Vector2.Zero, textScale, SpriteEffects.None, 1f);
					textPos.X += shadowOffsetX;
					b.DrawString(Game1.dialogueFont, text, textPos, Color.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, 1f);
				}
				if (letterReceived)
				{
					b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(157f, 113f) * 3f, new Rectangle(463, 556, 37, 17), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					if (grandpaSpeechTimer > 8000 && grandpaSpeechTimer % 10000 > 7000 && grandpaSpeechTimer % 10000 < 9000 && totalMilliseconds % 600 < 300)
					{
						b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(157f, 113f) * 3f, new Rectangle(500, 556, 37, 17), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					}
					b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + letterPosition, new Rectangle(729, 524, 131, 63), Color.White, 0f, Vector2.Zero, letterScale, SpriteEffects.None, 1f);
				}
			}
			else if (scene == 3)
			{
				SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:GrandpaStory.cs.12059"), (int)Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 0, 0, -200).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 0, 0, 0, -50).Y, 999, -1, 999, 1f, 1f, junimoText: false, -1, "", 4);
			}
			else if (scene == 4)
			{
				float alpha = 1f - ((float)grandpaSpeechTimer - 7000f) / 2000f;
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730), new Rectangle(0, 0, 427, 240), Color.White * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(22f, 211f) * 3f, new Rectangle(264 + totalMilliseconds % 500 / 250 * 19, 581, 19, 17), Color.White * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(332f, 215f) * 3f, new Rectangle(305 + totalMilliseconds % 600 / 200 * 12, 581, 12, 12), Color.White * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(414f, 211f) * 3f, new Rectangle(460 + totalMilliseconds % 400 / 200 * 13, 581, 13, 17), Color.White * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(189f, 81f) * 3f, new Rectangle(426 + totalMilliseconds % 800 / 400 * 16, 581, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
			}
			if ((scene == 4 && grandpaSpeechTimer >= 5000) || scene == 5)
			{
				b.Draw(texture, new Vector2(panX, Game1.viewport.Height / 2 - 360), new Rectangle(0, 600, 1200, 180), Color.White * ((scene == 5) ? 1f : (((float)grandpaSpeechTimer - 7000f) / 2000f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(texture, new Vector2(panX, Game1.viewport.Height / 2 - 360) + new Vector2(1080f, 524f), new Rectangle(350 + totalMilliseconds % 800 / 400 * 14, 581, 14, 9), Color.White * ((scene == 5) ? 1f : (((float)grandpaSpeechTimer - 7000f) / 2000f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(texture, new Vector2(panX, Game1.viewport.Height / 2 - 360) + new Vector2(1564f, 520f), new Rectangle(383 + totalMilliseconds % 400 / 200 * 9, 581, 9, 7), Color.White * ((scene == 5) ? 1f : (((float)grandpaSpeechTimer - 7000f) / 2000f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(texture, new Vector2(panX, Game1.viewport.Height / 2 - 360) + new Vector2(2632f, 520f), new Rectangle(403 + totalMilliseconds % 600 / 300 * 8, 582, 8, 8), Color.White * ((scene == 5) ? 1f : (((float)grandpaSpeechTimer - 7000f) / 2000f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(texture, new Vector2(panX, Game1.viewport.Height / 2 - 360) + new Vector2(2604f, 504f), new Rectangle(364 + totalMilliseconds % 1100 / 100 * 5, 594, 5, 3), Color.White * ((scene == 5) ? 1f : (((float)grandpaSpeechTimer - 7000f) / 2000f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(texture, new Vector2(panX, Game1.viewport.Height / 2 - 360) + new Vector2(3116f, 492f), new Rectangle(343 + totalMilliseconds % 3000 / 1000 * 6, 593, 6, 5), Color.White * ((scene == 5) ? 1f : (((float)grandpaSpeechTimer - 7000f) / 2000f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				if (scene == 5)
				{
					Game1.player.draw(b);
				}
				b.Draw(texture, new Vector2(panX, Game1.viewport.Height / 2 - 360) + new Vector2(3580f, 540f), new Rectangle(895, 735, 29, 36), Color.White * ((scene == 5) ? 1f : (((float)grandpaSpeechTimer - 7000f) / 2000f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (scene == 6)
			{
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(261 - parallaxPan, 145f) * 4f, new Rectangle(550, 540, 56 + parallaxPan, 35), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(261 - parallaxPan, 4f + (float)Math.Max(0, Math.Min(60, (grandpaSpeechTimer - 5000) / 8))) * 4f, new Rectangle(264, 434, 56 + parallaxPan, 141), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				if (grandpaSpeechTimer > 3000)
				{
					b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(286 - parallaxPan, 32f + (float)Math.Max(0, Math.Min(60, (grandpaSpeechTimer - 5000) / 8)) + Math.Min(30f, (float)letterOpenTimer / 4f)) * 4f, new Rectangle(729 + Math.Min(2, letterOpenTimer / 200) * 131, 508, 131, 79), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				}
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730), new Rectangle(parallaxPan, 240, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(texture, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 1294, 730) + new Vector2(187f - (float)parallaxPan * 2.5f, 8f) * 4f, new Rectangle(20, 428, 232, 172), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			b.End();
			Game1.PushUIMode();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			if (letterView != null)
			{
				letterView.draw(b);
			}
			if (mouseActive)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			b.End();
			Game1.PopUIMode();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), fadingToQuit ? (new Color(64, 136, 248) * foregroundFade) : (Color.Black * foregroundFade));
			b.End();
		}

		public void changeScreenSize()
		{
			Game1.viewport.X = 0;
			Game1.viewport.Y = 0;
		}

		public void unload()
		{
			content.Unload();
			content = null;
		}

		public void receiveEventPoke(int data)
		{
		}

		public string minigameId()
		{
			return null;
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public bool forceQuit()
		{
			return false;
		}
	}
}
