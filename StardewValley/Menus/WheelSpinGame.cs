using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using System;

namespace StardewValley.Menus
{
	public class WheelSpinGame : IClickableMenu
	{
		public new const int width = 640;

		public new const int height = 448;

		public double arrowRotation;

		public double arrowRotationVelocity;

		public double arrowRotationDeceleration;

		private int timerBeforeStart;

		private int wager;

		private SparklingText resultText;

		private bool doneSpinning;

		public WheelSpinGame(int wager)
			: base(Game1.uiViewport.Width / 2 - 320, Game1.uiViewport.Height / 2 - 224, 640, 448)
		{
			timerBeforeStart = 1000;
			arrowRotationVelocity = Math.PI / 16.0;
			arrowRotationVelocity += (double)Game1.random.Next(0, 15) * Math.PI / 256.0;
			arrowRotationDeceleration = -0.00062831853071795862;
			if (Game1.random.NextDouble() < 0.5)
			{
				arrowRotationVelocity += Math.PI / 64.0;
			}
			this.wager = wager;
			Game1.player.Halt();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (timerBeforeStart <= 0)
			{
				double oldVelocity = arrowRotationVelocity;
				arrowRotationVelocity += arrowRotationDeceleration;
				if (arrowRotationVelocity <= Math.PI / 80.0 && oldVelocity > Math.PI / 80.0)
				{
					bool colorChoiceGreen2 = Game1.currentLocation.currentEvent.specialEventVariable2;
					if (arrowRotation > Math.PI / 2.0 && arrowRotation <= 4.3196898986859651 && Game1.random.NextDouble() < (double)((float)Game1.player.LuckLevel / 15f))
					{
						if (colorChoiceGreen2)
						{
							arrowRotationVelocity = Math.PI / 48.0;
							Game1.playSound("dwop");
						}
					}
					else if ((arrowRotation + Math.PI) % (Math.PI * 2.0) <= 4.3196898986859651 && !colorChoiceGreen2 && Game1.random.NextDouble() < (double)((float)Game1.player.LuckLevel / 20f))
					{
						arrowRotationVelocity = Math.PI / 48.0;
						Game1.playSound("dwop");
					}
				}
				if (arrowRotationVelocity <= 0.0 && !doneSpinning)
				{
					doneSpinning = true;
					arrowRotationDeceleration = 0.0;
					arrowRotationVelocity = 0.0;
					bool colorChoiceGreen = Game1.currentLocation.currentEvent.specialEventVariable2;
					bool won = false;
					if (arrowRotation > Math.PI / 2.0 && arrowRotation <= 4.71238898038469)
					{
						if (!colorChoiceGreen)
						{
							won = true;
						}
					}
					else if (colorChoiceGreen)
					{
						won = true;
					}
					if (won)
					{
						Game1.playSound("reward");
						resultText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:WheelSpinGame.cs.11829"), Color.Lime, Color.White);
						Game1.player.festivalScore += wager;
					}
					else
					{
						resultText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:WheelSpinGame.cs.11830"), Color.Red, Color.Transparent);
						Game1.playSound("fishEscape");
						Game1.player.festivalScore -= wager;
					}
				}
				double num = arrowRotation;
				arrowRotation += arrowRotationVelocity;
				if (num % (Math.PI / 2.0) > arrowRotation % (Math.PI / 2.0))
				{
					Game1.playSound("Cowboy_gunshot");
				}
				arrowRotation %= Math.PI * 2.0;
			}
			else
			{
				timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
				if (timerBeforeStart <= 0)
				{
					Game1.playSound("cowboy_monsterhit");
				}
			}
			if (resultText != null && resultText.update(time))
			{
				resultText = null;
			}
			if (doneSpinning && resultText == null)
			{
				Game1.exitActiveMenu();
				Game1.player.canMove = true;
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void receiveKeyPress(Keys key)
		{
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(128, 1184, 160, 112), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.95f);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 320, yPositionOnScreen + 224 + 4), new Rectangle(120, 1234, 8, 16), Color.White, (float)arrowRotation, new Vector2(4f, 15f), 4f, SpriteEffects.None, 0.96f);
			if (resultText != null)
			{
				resultText.draw(b, new Vector2((float)(xPositionOnScreen + 320) - resultText.textWidth, yPositionOnScreen - 64));
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
