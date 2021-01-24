using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class BobberBar : IClickableMenu
	{
		public const int timePerFishSizeReduction = 800;

		public const int bobberTrackHeight = 548;

		public const int bobberBarTrackHeight = 568;

		public const int xOffsetToBobberTrack = 64;

		public const int yOffsetToBobberTrack = 12;

		public const int mixed = 0;

		public const int dart = 1;

		public const int smooth = 2;

		public const int sink = 3;

		public const int floater = 4;

		protected bool handledFishResult;

		private float difficulty;

		private int motionType;

		private int whichFish;

		private float bobberPosition = 548f;

		private float bobberSpeed;

		private float bobberAcceleration;

		private float bobberTargetPosition;

		private float scale;

		private float everythingShakeTimer;

		private float floaterSinkerAcceleration;

		private float treasurePosition;

		private float treasureCatchLevel;

		private float treasureAppearTimer;

		private float treasureScale;

		private bool bobberInBar;

		private bool buttonPressed;

		private bool flipBubble;

		private bool fadeIn;

		private bool fadeOut;

		private bool treasure;

		private bool treasureCaught;

		private bool perfect;

		private bool bossFish;

		private bool beginnersRod;

		private bool fromFishPond;

		private int bobberBarHeight;

		private int fishSize;

		private int fishQuality;

		private int minFishSize;

		private int maxFishSize;

		private int fishSizeReductionTimer;

		private int whichBobber;

		private Vector2 barShake;

		private Vector2 fishShake;

		private Vector2 everythingShake;

		private Vector2 treasureShake;

		private float reelRotation;

		private SparklingText sparkleText;

		private float bobberBarPos;

		private float bobberBarSpeed;

		private float bobberBarAcceleration;

		private float distanceFromCatching = 0.3f;

		public static ICue reelSound;

		public static ICue unReelSound;

		public BobberBar(int whichFish, float fishSize, bool treasure, int bobber)
			: base(0, 0, 96, 636)
		{
			whichBobber = bobber;
			handledFishResult = false;
			this.treasure = treasure;
			treasureAppearTimer = Game1.random.Next(1000, 3000);
			fadeIn = true;
			scale = 0f;
			this.whichFish = whichFish;
			Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			beginnersRod = (Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod && (int)Game1.player.CurrentTool.upgradeLevel == 1);
			bobberBarHeight = 96 + Game1.player.FishingLevel * 8;
			if (Game1.player.FishingLevel < 5 && beginnersRod)
			{
				bobberBarHeight += 40 - Game1.player.FishingLevel * 8;
			}
			bossFish = FishingRod.isFishBossFish(whichFish);
			if (Game1.player.fishCaught != null && Game1.player.fishCaught.Count() == 0)
			{
				distanceFromCatching = 0.1f;
			}
			if (data.ContainsKey(whichFish))
			{
				string[] rawData = data[whichFish].Split('/');
				difficulty = Convert.ToInt32(rawData[1]);
				switch (rawData[2].ToLower())
				{
				case "mixed":
					motionType = 0;
					break;
				case "dart":
					motionType = 1;
					break;
				case "smooth":
					motionType = 2;
					break;
				case "floater":
					motionType = 4;
					break;
				case "sinker":
					motionType = 3;
					break;
				}
				minFishSize = Convert.ToInt32(rawData[3]);
				maxFishSize = Convert.ToInt32(rawData[4]);
				this.fishSize = (int)((float)minFishSize + (float)(maxFishSize - minFishSize) * fishSize);
				this.fishSize++;
				perfect = true;
				fishQuality = ((!((double)fishSize < 0.33)) ? (((double)fishSize < 0.66) ? 1 : 2) : 0);
				fishSizeReductionTimer = 800;
				if (whichBobber == 877)
				{
					fishQuality++;
					if (fishQuality > 2)
					{
						fishQuality = 4;
					}
				}
				if (beginnersRod)
				{
					fishQuality = 0;
					fishSize = minFishSize;
				}
			}
			Reposition();
			if (bobber == 695)
			{
				bobberBarHeight += 24;
			}
			bobberBarPos = 568 - bobberBarHeight;
			bobberPosition = 508f;
			bobberTargetPosition = (100f - difficulty) / 100f * 548f;
			if (Game1.soundBank != null)
			{
				reelSound = Game1.soundBank.GetCue("fastReel");
			}
			Game1.setRichPresence("fishing", Game1.currentLocation.Name);
		}

		public virtual void Reposition()
		{
			switch (Game1.player.FacingDirection)
			{
			case 1:
				xPositionOnScreen = (int)Game1.player.Position.X - 64 - 132;
				yPositionOnScreen = (int)Game1.player.Position.Y - 274;
				break;
			case 3:
				xPositionOnScreen = (int)Game1.player.Position.X + 128;
				yPositionOnScreen = (int)Game1.player.Position.Y - 274;
				flipBubble = true;
				break;
			case 0:
				xPositionOnScreen = (int)Game1.player.Position.X - 64 - 132;
				yPositionOnScreen = (int)Game1.player.Position.Y - 274;
				break;
			case 2:
				xPositionOnScreen = (int)Game1.player.Position.X - 64 - 132;
				yPositionOnScreen = (int)Game1.player.Position.Y - 274;
				break;
			}
			xPositionOnScreen -= Game1.viewport.X;
			yPositionOnScreen -= Game1.viewport.Y + 64;
			if (xPositionOnScreen + 96 > Game1.viewport.Width)
			{
				xPositionOnScreen = Game1.viewport.Width - 96;
			}
			else if (xPositionOnScreen < 0)
			{
				xPositionOnScreen = 0;
			}
			if (yPositionOnScreen < 0)
			{
				yPositionOnScreen = 0;
			}
			else if (yPositionOnScreen + 636 > Game1.viewport.Height)
			{
				yPositionOnScreen = Game1.viewport.Height - 636;
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			Reposition();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void update(GameTime time)
		{
			Reposition();
			if (sparkleText != null && sparkleText.update(time))
			{
				sparkleText = null;
			}
			if (everythingShakeTimer > 0f)
			{
				everythingShakeTimer -= time.ElapsedGameTime.Milliseconds;
				everythingShake = new Vector2((float)Game1.random.Next(-10, 11) / 10f, (float)Game1.random.Next(-10, 11) / 10f);
				if (everythingShakeTimer <= 0f)
				{
					everythingShake = Vector2.Zero;
				}
			}
			if (fadeIn)
			{
				scale += 0.05f;
				if (scale >= 1f)
				{
					scale = 1f;
					fadeIn = false;
				}
			}
			else if (fadeOut)
			{
				if (everythingShakeTimer > 0f || sparkleText != null)
				{
					return;
				}
				scale -= 0.05f;
				if (scale <= 0f)
				{
					scale = 0f;
					fadeOut = false;
					int bait = (Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod && (Game1.player.CurrentTool as FishingRod).attachments[0] != null) ? ((int)(Game1.player.CurrentTool as FishingRod).attachments[0].parentSheetIndex) : (-1);
					bool caughtDouble = !bossFish && bait == 774 && Game1.random.NextDouble() < 0.25 + Game1.player.DailyLuck / 2.0;
					if (distanceFromCatching > 0.9f && Game1.player.CurrentTool is FishingRod)
					{
						(Game1.player.CurrentTool as FishingRod).pullFishFromWater(whichFish, fishSize, fishQuality, (int)difficulty, treasureCaught, perfect, fromFishPond, caughtDouble);
					}
					else
					{
						Game1.player.completelyStopAnimatingOrDoingAction();
						if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod)
						{
							(Game1.player.CurrentTool as FishingRod).doneFishing(Game1.player, consumeBaitAndTackle: true);
						}
					}
					Game1.exitActiveMenu();
					Game1.setRichPresence("location", Game1.currentLocation.Name);
				}
			}
			else
			{
				if (Game1.random.NextDouble() < (double)(difficulty * (float)((motionType != 2) ? 1 : 20) / 4000f) && (motionType != 2 || bobberTargetPosition == -1f))
				{
					float spaceBelow = 548f - bobberPosition;
					float spaceAbove = bobberPosition;
					float percent = Math.Min(99f, difficulty + (float)Game1.random.Next(10, 45)) / 100f;
					bobberTargetPosition = bobberPosition + (float)Game1.random.Next(-(int)spaceAbove, (int)spaceBelow) * percent;
				}
				if (motionType == 4)
				{
					floaterSinkerAcceleration = Math.Max(floaterSinkerAcceleration - 0.01f, -1.5f);
				}
				else if (motionType == 3)
				{
					floaterSinkerAcceleration = Math.Min(floaterSinkerAcceleration + 0.01f, 1.5f);
				}
				if (Math.Abs(bobberPosition - bobberTargetPosition) > 3f && bobberTargetPosition != -1f)
				{
					bobberAcceleration = (bobberTargetPosition - bobberPosition) / ((float)Game1.random.Next(10, 30) + (100f - Math.Min(100f, difficulty)));
					bobberSpeed += (bobberAcceleration - bobberSpeed) / 5f;
				}
				else if (motionType != 2 && Game1.random.NextDouble() < (double)(difficulty / 2000f))
				{
					bobberTargetPosition = bobberPosition + (float)((Game1.random.NextDouble() < 0.5) ? Game1.random.Next(-100, -51) : Game1.random.Next(50, 101));
				}
				else
				{
					bobberTargetPosition = -1f;
				}
				if (motionType == 1 && Game1.random.NextDouble() < (double)(difficulty / 1000f))
				{
					bobberTargetPosition = bobberPosition + (float)((Game1.random.NextDouble() < 0.5) ? Game1.random.Next(-100 - (int)difficulty * 2, -51) : Game1.random.Next(50, 101 + (int)difficulty * 2));
				}
				bobberTargetPosition = Math.Max(-1f, Math.Min(bobberTargetPosition, 548f));
				bobberPosition += bobberSpeed + floaterSinkerAcceleration;
				if (bobberPosition > 532f)
				{
					bobberPosition = 532f;
				}
				else if (bobberPosition < 0f)
				{
					bobberPosition = 0f;
				}
				bobberInBar = (bobberPosition + 12f <= bobberBarPos - 32f + (float)bobberBarHeight && bobberPosition - 16f >= bobberBarPos - 32f);
				if (bobberPosition >= (float)(548 - bobberBarHeight) && bobberBarPos >= (float)(568 - bobberBarHeight - 4))
				{
					bobberInBar = true;
				}
				bool num = buttonPressed;
				buttonPressed = (Game1.oldMouseState.LeftButton == ButtonState.Pressed || Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.X) || Game1.oldPadState.IsButtonDown(Buttons.A))));
				if (!num && buttonPressed)
				{
					Game1.playSound("fishingRodBend");
				}
				float gravity = buttonPressed ? (-0.25f) : 0.25f;
				if (buttonPressed && gravity < 0f && (bobberBarPos == 0f || bobberBarPos == (float)(568 - bobberBarHeight)))
				{
					bobberBarSpeed = 0f;
				}
				if (bobberInBar)
				{
					gravity *= ((whichBobber == 691) ? 0.3f : 0.6f);
					if (whichBobber == 691)
					{
						if (bobberPosition + 16f < bobberBarPos + (float)(bobberBarHeight / 2))
						{
							bobberBarSpeed -= 0.2f;
						}
						else
						{
							bobberBarSpeed += 0.2f;
						}
					}
				}
				float oldPos = bobberBarPos;
				bobberBarSpeed += gravity;
				bobberBarPos += bobberBarSpeed;
				if (bobberBarPos + (float)bobberBarHeight > 568f)
				{
					bobberBarPos = 568 - bobberBarHeight;
					bobberBarSpeed = (0f - bobberBarSpeed) * 2f / 3f * ((whichBobber == 692) ? 0.1f : 1f);
					if (oldPos + (float)bobberBarHeight < 568f)
					{
						Game1.playSound("shiny4");
					}
				}
				else if (bobberBarPos < 0f)
				{
					bobberBarPos = 0f;
					bobberBarSpeed = (0f - bobberBarSpeed) * 2f / 3f;
					if (oldPos > 0f)
					{
						Game1.playSound("shiny4");
					}
				}
				bool treasureInBar = false;
				if (treasure)
				{
					float oldTreasureAppearTimer = treasureAppearTimer;
					treasureAppearTimer -= time.ElapsedGameTime.Milliseconds;
					if (treasureAppearTimer <= 0f)
					{
						if (treasureScale < 1f && !treasureCaught)
						{
							if (oldTreasureAppearTimer > 0f)
							{
								treasurePosition = ((bobberBarPos > 274f) ? Game1.random.Next(8, (int)bobberBarPos - 20) : Game1.random.Next(Math.Min(528, (int)bobberBarPos + bobberBarHeight), 500));
								Game1.playSound("dwop");
							}
							treasureScale = Math.Min(1f, treasureScale + 0.1f);
						}
						treasureInBar = (treasurePosition + 12f <= bobberBarPos - 32f + (float)bobberBarHeight && treasurePosition - 16f >= bobberBarPos - 32f);
						if (treasureInBar && !treasureCaught)
						{
							treasureCatchLevel += 0.0135f;
							treasureShake = new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3));
							if (treasureCatchLevel >= 1f)
							{
								Game1.playSound("newArtifact");
								treasureCaught = true;
							}
						}
						else if (treasureCaught)
						{
							treasureScale = Math.Max(0f, treasureScale - 0.1f);
						}
						else
						{
							treasureShake = Vector2.Zero;
							treasureCatchLevel = Math.Max(0f, treasureCatchLevel - 0.01f);
						}
					}
				}
				if (bobberInBar)
				{
					distanceFromCatching += 0.002f;
					reelRotation += (float)Math.PI / 8f;
					fishShake.X = (float)Game1.random.Next(-10, 11) / 10f;
					fishShake.Y = (float)Game1.random.Next(-10, 11) / 10f;
					barShake = Vector2.Zero;
					Rumble.rumble(0.1f, 1000f);
					if (unReelSound != null)
					{
						unReelSound.Stop(AudioStopOptions.Immediate);
					}
					if (Game1.soundBank != null && (reelSound == null || reelSound.IsStopped || reelSound.IsStopping))
					{
						reelSound = Game1.soundBank.GetCue("fastReel");
					}
					if (reelSound != null && !reelSound.IsPlaying && !reelSound.IsStopping)
					{
						reelSound.Play();
					}
				}
				else if (!treasureInBar || treasureCaught || whichBobber != 693)
				{
					if (!fishShake.Equals(Vector2.Zero))
					{
						Game1.playSound("tinyWhip");
						perfect = false;
						Rumble.stopRumbling();
					}
					fishSizeReductionTimer -= time.ElapsedGameTime.Milliseconds;
					if (fishSizeReductionTimer <= 0)
					{
						fishSize = Math.Max(minFishSize, fishSize - 1);
						fishSizeReductionTimer = 800;
					}
					if ((Game1.player.fishCaught != null && Game1.player.fishCaught.Count() != 0) || Game1.currentMinigame != null)
					{
						distanceFromCatching -= ((whichBobber == 694 || beginnersRod) ? 0.002f : 0.003f);
					}
					float distanceAway = Math.Abs(bobberPosition - (bobberBarPos + (float)(bobberBarHeight / 2)));
					reelRotation -= (float)Math.PI / Math.Max(10f, 200f - distanceAway);
					barShake.X = (float)Game1.random.Next(-10, 11) / 10f;
					barShake.Y = (float)Game1.random.Next(-10, 11) / 10f;
					fishShake = Vector2.Zero;
					if (reelSound != null)
					{
						reelSound.Stop(AudioStopOptions.Immediate);
					}
					if (Game1.soundBank != null && (unReelSound == null || unReelSound.IsStopped))
					{
						unReelSound = Game1.soundBank.GetCue("slowReel");
						unReelSound.SetVariable("Pitch", 600);
					}
					if (unReelSound != null && !unReelSound.IsPlaying && !unReelSound.IsStopping)
					{
						unReelSound.Play();
					}
				}
				distanceFromCatching = Math.Max(0f, Math.Min(1f, distanceFromCatching));
				if (Game1.player.CurrentTool != null)
				{
					Game1.player.CurrentTool.tickUpdate(time, Game1.player);
				}
				if (distanceFromCatching <= 0f)
				{
					fadeOut = true;
					everythingShakeTimer = 500f;
					Game1.playSound("fishEscape");
					handledFishResult = true;
					if (unReelSound != null)
					{
						unReelSound.Stop(AudioStopOptions.Immediate);
					}
					if (reelSound != null)
					{
						reelSound.Stop(AudioStopOptions.Immediate);
					}
				}
				else if (distanceFromCatching >= 1f)
				{
					everythingShakeTimer = 500f;
					Game1.playSound("jingle1");
					fadeOut = true;
					handledFishResult = true;
					if (unReelSound != null)
					{
						unReelSound.Stop(AudioStopOptions.Immediate);
					}
					if (reelSound != null)
					{
						reelSound.Stop(AudioStopOptions.Immediate);
					}
					if (perfect)
					{
						sparkleText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Yellow, Color.White, rainbow: false, 0.1, 1500);
						if (Game1.isFestival())
						{
							Game1.CurrentEvent.perfectFishing();
						}
					}
					else if (fishSize == maxFishSize)
					{
						fishSize--;
					}
				}
			}
			if (bobberPosition < 0f)
			{
				bobberPosition = 0f;
			}
			if (bobberPosition > 548f)
			{
				bobberPosition = 548f;
			}
		}

		public override bool readyToClose()
		{
			return false;
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			if (unReelSound != null)
			{
				unReelSound.Stop(AudioStopOptions.Immediate);
			}
			if (reelSound != null)
			{
				reelSound.Stop(AudioStopOptions.Immediate);
			}
			if (!handledFishResult)
			{
				Game1.playSound("fishEscape");
			}
			fadeOut = true;
			everythingShakeTimer = 500f;
			distanceFromCatching = -1f;
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.menuButton.Contains(new InputButton(key)))
			{
				emergencyShutDown();
			}
		}

		public override void draw(SpriteBatch b)
		{
			Game1.StartWorldDrawInUI(b);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - (flipBubble ? 44 : 20) + 104, yPositionOnScreen - 16 + 314) + everythingShake, new Rectangle(652, 1685, 52, 157), Color.White * 0.6f * scale, 0f, new Vector2(26f, 78.5f) * scale, 4f * scale, flipBubble ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.001f);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 70, yPositionOnScreen + 296) + everythingShake, new Rectangle(644, 1999, 37, 150), Color.White * scale, 0f, new Vector2(18.5f, 74f) * scale, 4f * scale, SpriteEffects.None, 0.01f);
			if (scale == 1f)
			{
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 64, yPositionOnScreen + 12 + (int)bobberBarPos) + barShake + everythingShake, new Rectangle(682, 2078, 9, 2), bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 64, yPositionOnScreen + 12 + (int)bobberBarPos + 8) + barShake + everythingShake, new Rectangle(682, 2081, 9, 1), bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, new Vector2(4f, bobberBarHeight - 16), SpriteEffects.None, 0.89f);
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 64, yPositionOnScreen + 12 + (int)bobberBarPos + bobberBarHeight - 8) + barShake + everythingShake, new Rectangle(682, 2085, 9, 2), bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + 124, yPositionOnScreen + 4 + (int)(580f * (1f - distanceFromCatching)), 16, (int)(580f * distanceFromCatching)), Utility.getRedToGreenLerpColor(distanceFromCatching));
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 18, yPositionOnScreen + 514) + everythingShake, new Rectangle(257, 1990, 5, 10), Color.White, reelRotation, new Vector2(2f, 10f), 4f, SpriteEffects.None, 0.9f);
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 64 + 18, (float)(yPositionOnScreen + 12 + 24) + treasurePosition) + treasureShake + everythingShake, new Rectangle(638, 1865, 20, 24), Color.White, 0f, new Vector2(10f, 10f), 2f * treasureScale, SpriteEffects.None, 0.85f);
				if (treasureCatchLevel > 0f && !treasureCaught)
				{
					b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + 64, yPositionOnScreen + 12 + (int)treasurePosition, 40, 8), Color.DimGray * 0.5f);
					b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + 64, yPositionOnScreen + 12 + (int)treasurePosition, (int)(treasureCatchLevel * 40f), 8), Color.Orange);
				}
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 64 + 18, (float)(yPositionOnScreen + 12 + 24) + bobberPosition) + fishShake + everythingShake, new Rectangle(614 + (bossFish ? 20 : 0), 1840, 20, 20), Color.White, 0f, new Vector2(10f, 10f), 2f, SpriteEffects.None, 0.88f);
				if (sparkleText != null)
				{
					sparkleText.draw(b, new Vector2(xPositionOnScreen - 16, yPositionOnScreen - 64));
				}
			}
			if (Game1.player.fishCaught != null && Game1.player.fishCaught.Count() == 0)
			{
				Vector2 pos = new Vector2(xPositionOnScreen + (flipBubble ? (width + 64 + 8) : (-200)), yPositionOnScreen + 192);
				if (!Game1.options.gamepadControls)
				{
					b.Draw(Game1.mouseCursors, pos, new Rectangle(644, 1330, 48, 69), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
				else
				{
					b.Draw(Game1.controllerMaps, pos, Utility.controllerMapSourceRect(new Rectangle(681, 0, 96, 138)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
				}
			}
			Game1.EndWorldDrawInUI(b);
		}
	}
}
