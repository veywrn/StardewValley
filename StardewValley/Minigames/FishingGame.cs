using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using xTile.Dimensions;

namespace StardewValley.Minigames
{
	public class FishingGame : IMinigame
	{
		private GameLocation location;

		private LocalizedContentManager content;

		private int timerToStart = 1000;

		private int gameEndTimer;

		private int showResultsTimer;

		public bool exit;

		public bool gameDone;

		public int score;

		public int fishCaught;

		public int starTokensWon;

		public int perfections;

		public int perfectionBonus;

		public GameLocation originalLocation;

		public FishingGame()
		{
			content = Game1.content.CreateTemporary();
			location = new GameLocation("Maps\\FishingGame", "fishingGame");
			location.isStructure.Value = true;
			location.uniqueName.Value = "fishingGame" + Game1.player.UniqueMultiplayerID;
			location.currentEvent = Game1.currentLocation.currentEvent;
			Game1.player.CurrentToolIndex = 0;
			Game1.player.TemporaryItem = new FishingRod();
			(Game1.player.CurrentTool as FishingRod).attachments[0] = new Object(690, 99);
			(Game1.player.CurrentTool as FishingRod).attachments[1] = new Object(687, 1);
			Game1.player.UsingTool = false;
			Game1.player.CurrentToolIndex = 0;
			Game1.globalFadeToClear(null, 0.01f);
			location.Map.LoadTileSheets(Game1.mapDisplayDevice);
			Game1.player.Position = new Vector2(14f, 7f) * 64f;
			Game1.player.currentLocation = location;
			originalLocation = Game1.currentLocation;
			Game1.currentLocation = location;
			changeScreenSize();
			gameEndTimer = 100000;
			showResultsTimer = -1;
			Game1.player.faceDirection(3);
			Game1.player.Halt();
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool tick(GameTime time)
		{
			Rumble.update(time.ElapsedGameTime.Milliseconds);
			Game1.player.Stamina = Game1.player.MaxStamina;
			for (int i = Game1.screenOverlayTempSprites.Count - 1; i >= 0; i--)
			{
				if (Game1.screenOverlayTempSprites[i].update(time))
				{
					Game1.screenOverlayTempSprites.RemoveAt(i);
				}
			}
			if (Game1.activeClickableMenu != null)
			{
				Game1.updateActiveMenu(time);
			}
			if (timerToStart > 0)
			{
				Game1.player.faceDirection(3);
				timerToStart -= time.ElapsedGameTime.Milliseconds;
				if (timerToStart <= 0)
				{
					Game1.playSound("whistle");
				}
			}
			else if (showResultsTimer >= 0)
			{
				int num = showResultsTimer;
				showResultsTimer -= time.ElapsedGameTime.Milliseconds;
				if (num > 11000 && showResultsTimer <= 11000)
				{
					Game1.playSound("smallSelect");
				}
				if (num > 9000 && showResultsTimer <= 9000)
				{
					Game1.playSound("smallSelect");
				}
				if (num > 7000 && showResultsTimer <= 7000)
				{
					if (perfections > 0)
					{
						score += perfections * 10;
						perfectionBonus = perfections * 10;
						if (fishCaught >= 3 && perfections >= 3)
						{
							perfectionBonus += score;
							score *= 2;
						}
						Game1.playSound("newArtifact");
					}
					else
					{
						Game1.playSound("smallSelect");
					}
				}
				if (num > 5000 && showResultsTimer <= 5000)
				{
					if (score >= 10)
					{
						Game1.playSound("reward");
						starTokensWon = (score + 5) / 10 * 6;
						starTokensWon *= 2;
						Game1.player.festivalScore += starTokensWon;
					}
					else
					{
						Game1.playSound("fishEscape");
					}
				}
				if (showResultsTimer <= 0)
				{
					Game1.globalFadeToClear();
					return true;
				}
			}
			else if (!gameDone)
			{
				gameEndTimer -= time.ElapsedGameTime.Milliseconds;
				if (gameEndTimer <= 0 && Game1.activeClickableMenu == null && (!Game1.player.UsingTool || (Game1.player.CurrentTool as FishingRod).isFishing))
				{
					(Game1.player.CurrentTool as FishingRod).doneFishing(Game1.player);
					(Game1.player.CurrentTool as FishingRod).tickUpdate(time, Game1.player);
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.playSound("whistle");
					gameEndTimer = 1000;
					gameDone = true;
				}
			}
			else if (gameDone && gameEndTimer > 0)
			{
				gameEndTimer -= time.ElapsedGameTime.Milliseconds;
				if (gameEndTimer <= 0)
				{
					Game1.globalFadeToBlack(gameDoneAfterFade, 0.01f);
					Game1.exitActiveMenu();
					Game1.player.forceCanMove();
				}
			}
			return exit;
		}

		public void gameDoneAfterFade()
		{
			showResultsTimer = 11100;
			Game1.player.canMove = false;
			Game1.player.Position = new Vector2(24f, 71f) * 64f;
			Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(1536, 4544, 64, 64));
			Game1.player.currentLocation = originalLocation;
			Game1.currentLocation = originalLocation;
			Game1.player.faceDirection(2);
			Utility.killAllStaticLoopingSoundCues();
			if (FishingRod.reelSound != null && FishingRod.reelSound.IsPlaying)
			{
				FishingRod.reelSound.Stop(AudioStopOptions.Immediate);
			}
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!Game1.isAnyGamePadButtonBeingPressed())
			{
				handleCastInput();
			}
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void releaseLeftClick(int x, int y)
		{
			handleCastInputReleased();
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void receiveKeyPress(Keys k)
		{
			if (!gameDone)
			{
				if (Game1.player.movementDirections.Count < 2 && !Game1.player.UsingTool && timerToStart <= 0)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
					{
						Game1.player.setMoving(1);
					}
					if (Game1.options.doesInputListContain(Game1.options.moveRightButton, k))
					{
						Game1.player.setMoving(2);
					}
					if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
					{
						Game1.player.setMoving(4);
					}
					if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, k))
					{
						Game1.player.setMoving(8);
					}
				}
				if (Game1.options.doesInputListContain(Game1.options.useToolButton, k))
				{
					handleCastInput();
				}
				if (k == Keys.Escape)
				{
					if (gameEndTimer <= 0 && !gameDone)
					{
						EmergencyCancel();
					}
					else if (Game1.activeClickableMenu == null)
					{
						gameEndTimer = 1;
					}
					else if (Game1.activeClickableMenu is BobberBar)
					{
						(Game1.activeClickableMenu as BobberBar).receiveKeyPress(k);
					}
				}
			}
			if (Game1.options.doesInputListContain(Game1.options.runButton, k) || Game1.isGamePadThumbstickInMotion())
			{
				Game1.player.setRunning(isRunning: true);
			}
		}

		public void receiveKeyRelease(Keys k)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
			{
				Game1.player.setMoving(33);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveRightButton, k))
			{
				Game1.player.setMoving(34);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
			{
				Game1.player.setMoving(36);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, k))
			{
				Game1.player.setMoving(40);
			}
			if (Game1.options.doesInputListContain(Game1.options.runButton, k))
			{
				Game1.player.setRunning(isRunning: false);
			}
			if (Game1.player.movementDirections.Count == 0 && !Game1.player.UsingTool)
			{
				Game1.player.Halt();
			}
			if (Game1.options.doesInputListContain(Game1.options.useToolButton, k))
			{
				handleCastInputReleased();
			}
		}

		public virtual void EmergencyCancel()
		{
			Game1.player.Halt();
			Game1.player.isEating = false;
			Game1.player.CanMove = true;
			Game1.player.UsingTool = false;
			Game1.player.usingSlingshot = false;
			Game1.player.FarmerSprite.PauseForSingleAnimation = false;
			if (Game1.player.CurrentTool is FishingRod)
			{
				(Game1.player.CurrentTool as FishingRod).resetState();
			}
		}

		private void handleCastInput()
		{
			if (timerToStart <= 0 && showResultsTimer < 0 && !gameDone && Game1.activeClickableMenu == null && !(Game1.player.CurrentTool as FishingRod).hit && !(Game1.player.CurrentTool as FishingRod).pullingOutOfWater && !(Game1.player.CurrentTool as FishingRod).isCasting && !(Game1.player.CurrentTool as FishingRod).fishCaught && !(Game1.player.CurrentTool as FishingRod).castedButBobberStillInAir)
			{
				Game1.player.lastClick = Vector2.Zero;
				Game1.player.Halt();
				Game1.pressUseToolButton();
			}
			else if (showResultsTimer > 11000)
			{
				showResultsTimer = 11001;
			}
			else if (showResultsTimer > 9000)
			{
				showResultsTimer = 9001;
			}
			else if (showResultsTimer > 7000)
			{
				showResultsTimer = 7001;
			}
			else if (showResultsTimer > 5000)
			{
				showResultsTimer = 5001;
			}
			else if (showResultsTimer < 5000 && showResultsTimer > 1000)
			{
				showResultsTimer = 1500;
				Game1.playSound("smallSelect");
			}
		}

		private void handleCastInputReleased()
		{
			if (showResultsTimer < 0 && Game1.player.CurrentTool != null && !(Game1.player.CurrentTool as FishingRod).isCasting && Game1.activeClickableMenu == null && Game1.player.CurrentTool.onRelease(location, 0, 0, Game1.player))
			{
				Game1.player.Halt();
			}
		}

		public void draw(SpriteBatch b)
		{
			if (showResultsTimer < 0)
			{
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
				Game1.mapDisplayDevice.BeginScene(b);
				location.Map.GetLayer("Back").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
				location.drawWater(b);
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, Game1.player.Position + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f - ((Game1.player.running || Game1.player.UsingTool) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[Game1.player.FarmerSprite.CurrentFrame]) * 0.8f) : 0f), SpriteEffects.None, Math.Max(0f, (float)Game1.player.getStandingY() / 10000f + 0.00011f) - 1E-07f);
				location.Map.GetLayer("Buildings").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
				location.draw(b);
				Game1.player.draw(b);
				location.Map.GetLayer("Front").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
				if (Game1.activeClickableMenu != null)
				{
					Game1.activeClickableMenu.draw(b);
				}
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1514", Utility.getMinutesSecondsStringFromMilliseconds(Math.Max(0, gameEndTimer))), new Vector2(16f, 64f), Color.White);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", score), new Vector2(16f, 32f), Color.White);
				foreach (TemporaryAnimatedSprite screenOverlayTempSprite in Game1.screenOverlayTempSprites)
				{
					screenOverlayTempSprite.draw(b);
				}
				b.End();
				return;
			}
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			Vector2 position = new Vector2(Game1.viewport.Width / 2 - 128, Game1.viewport.Height / 2 - 64);
			if (showResultsTimer <= 11000)
			{
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", score), Game1.textColor, (showResultsTimer <= 7000 && perfectionBonus > 0) ? Color.Lime : Color.White, position);
			}
			if (showResultsTimer <= 9000)
			{
				position.Y += 48f;
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12010", fishCaught), Game1.textColor, Color.White, position);
			}
			if (showResultsTimer <= 7000)
			{
				position.Y += 48f;
				if (perfectionBonus > 1)
				{
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12011", perfectionBonus), Game1.textColor, Color.Yellow, position);
				}
				else
				{
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12012"), Game1.textColor, Color.Red, position);
				}
			}
			if (showResultsTimer <= 5000)
			{
				position.Y += 64f;
				if (starTokensWon > 0)
				{
					float fade = Math.Min(1f, (float)(showResultsTimer - 2000) / 4000f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", starTokensWon), Game1.textColor * 0.2f * fade, Color.SkyBlue * 0.3f * fade, position + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) * 4f * 2f, 0f, 1f, 1f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", starTokensWon), Game1.textColor * 0.2f * fade, Color.SkyBlue * 0.3f * fade, position + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) * 4f * 2f, 0f, 1f, 1f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", starTokensWon), Game1.textColor * 0.2f * fade, Color.SkyBlue * 0.3f * fade, position + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) * 4f * 2f, 0f, 1f, 1f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", starTokensWon), Game1.textColor, Color.SkyBlue, position, 0f, 1f, 1f);
				}
				else
				{
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12021"), Game1.textColor, Color.Red, position);
				}
			}
			if (showResultsTimer <= 1000)
			{
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * (1f - (float)showResultsTimer / 1000f));
			}
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 64), Color.Black * 0.75f);
			b.Draw(Game1.mouseCursors, new Vector2(32f, 32f), new Microsoft.Xna.Framework.Rectangle(338, 400, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Game1.drawWithBorder(string.Concat(Game1.player.festivalScore), Color.Black, Color.White, new Vector2(72f, 29f), 0f, 1f, 1f, tiny: false);
			b.End();
		}

		public static void startMe()
		{
			Game1.currentMinigame = new FishingGame();
		}

		public void changeScreenSize()
		{
			Game1.viewport.X = location.Map.Layers[0].LayerWidth * 64 / 2 - (int)((float)(Game1.game1.localMultiplayerWindow.Width / 2) / Game1.options.zoomLevel);
			Game1.viewport.Y = location.Map.Layers[0].LayerHeight * 64 / 2 - (int)((float)(Game1.game1.localMultiplayerWindow.Height / 2) / Game1.options.zoomLevel);
		}

		public void unload()
		{
			(Game1.player.CurrentTool as FishingRod).castingEndFunction(-1);
			(Game1.player.CurrentTool as FishingRod).doneFishing(Game1.player);
			Game1.player.TemporaryItem = null;
			Game1.player.currentLocation = Game1.currentLocation;
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.player.forceCanMove();
			Game1.player.faceDirection(2);
			content.Unload();
			content.Dispose();
			content = null;
		}

		public void receiveEventPoke(int data)
		{
		}

		public string minigameId()
		{
			return "FishingGame";
		}

		public bool doMainGameUpdates()
		{
			return true;
		}

		public bool forceQuit()
		{
			return false;
		}
	}
}
