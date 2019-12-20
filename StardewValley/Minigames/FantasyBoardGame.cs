using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace StardewValley.Minigames
{
	public class FantasyBoardGame : IMinigame
	{
		public int borderSourceWidth = 138;

		public int borderSourceHeight = 74;

		public int slideSourceWidth = 128;

		public int slideSourceHeight = 64;

		private LocalizedContentManager content;

		private Texture2D slides;

		private Texture2D border;

		public int whichSlide;

		public int shakeTimer;

		public int endTimer;

		private string grade = "";

		public FantasyBoardGame()
		{
			content = Game1.content.CreateTemporary();
			slides = content.Load<Texture2D>("LooseSprites\\boardGame");
			border = content.Load<Texture2D>("LooseSprites\\boardGameBorder");
			Game1.globalFadeToClear();
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool tick(GameTime time)
		{
			if (shakeTimer > 0)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			Game1.currentLocation.currentEvent.checkForNextCommand(Game1.currentLocation, time);
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.update(time);
			}
			if (endTimer > 0)
			{
				endTimer -= time.ElapsedGameTime.Milliseconds;
				if (endTimer <= 0 && whichSlide == -1)
				{
					Game1.globalFadeToBlack(end);
				}
			}
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.performHoverAction(Game1.getOldMouseX(), Game1.getOldMouseY());
			}
			return false;
		}

		public void end()
		{
			unload();
			Game1.currentLocation.currentEvent.CurrentCommand++;
			Game1.currentMinigame = null;
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.receiveLeftClick(x, y);
			}
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
			Game1.pressActionButton(Game1.GetKeyboardState(), Game1.input.GetMouseState(), Game1.input.GetGamePadState());
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.receiveRightClick(x, y);
			}
		}

		public void releaseLeftClick(int x, int y)
		{
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void receiveKeyPress(Keys k)
		{
			if (Game1.isQuestion)
			{
				if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
				{
					Game1.currentQuestionChoice = Math.Max(Game1.currentQuestionChoice - 1, 0);
					Game1.playSound("toolSwap");
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
				{
					Game1.currentQuestionChoice = Math.Min(Game1.currentQuestionChoice + 1, Game1.questionChoices.Count - 1);
					Game1.playSound("toolSwap");
				}
			}
			else if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.receiveKeyPress(k);
			}
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			if (whichSlide >= 0)
			{
				Vector2 offset = default(Vector2);
				if (shakeTimer > 0)
				{
					offset = new Vector2(Game1.random.Next(-2, 2), Game1.random.Next(-2, 2));
				}
				b.Draw(border, offset + new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - borderSourceWidth * 4 / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - borderSourceHeight * 4 / 2 - 128), new Rectangle(0, 0, borderSourceWidth, borderSourceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				b.Draw(slides, offset + new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - slideSourceWidth * 4 / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - slideSourceHeight * 4 / 2 - 128), new Rectangle(whichSlide % 2 * slideSourceWidth, whichSlide / 2 * slideSourceHeight, slideSourceWidth, slideSourceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
			}
			else
			{
				string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:FantasyBoardGame.cs.11980", grade);
				float yOffset = (float)Math.Sin(endTimer / 1000) * 8f;
				Game1.drawWithBorder(s, Game1.textColor, Color.Purple, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f, yOffset + (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2)));
			}
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.draw(b);
			}
			b.End();
		}

		public void changeScreenSize()
		{
		}

		public void unload()
		{
			content.Unload();
		}

		public void afterFade()
		{
			whichSlide = -1;
			int score = 0;
			if (Game1.player.mailReceived.Contains("savedFriends"))
			{
				score++;
			}
			if (Game1.player.mailReceived.Contains("destroyedPods"))
			{
				score++;
			}
			if (Game1.player.mailReceived.Contains("killedSkeleton"))
			{
				score++;
			}
			switch (score)
			{
			case 0:
				grade = "D";
				break;
			case 1:
				grade = "C";
				break;
			case 2:
				grade = "B";
				break;
			case 3:
				grade = "A";
				break;
			}
			Game1.playSound("newArtifact");
			endTimer = 5500;
		}

		public void receiveEventPoke(int data)
		{
			switch (data)
			{
			case -1:
				shakeTimer = 1000;
				break;
			case -2:
				Game1.globalFadeToBlack(afterFade);
				break;
			default:
				whichSlide = data;
				break;
			}
		}

		public string minigameId()
		{
			return "FantasyBoardGame";
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
