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
	public class Slots : IMinigame
	{
		public const float slotTurnRate = 0.008f;

		public const int numberOfIcons = 8;

		public const int defaultBet = 10;

		private string coinBuffer;

		private List<float> slots;

		private List<float> slotResults;

		private ClickableComponent spinButton10;

		private ClickableComponent spinButton100;

		private ClickableComponent doneButton;

		private bool spinning;

		private bool showResult;

		private float payoutModifier;

		private int currentBet;

		private int spinsCount;

		private int slotsFinished;

		private int endTimer;

		public ClickableComponent currentlySnappedComponent;

		public Slots(int toBet = -1, bool highStakes = false)
		{
			coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? "     " : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  "));
			currentBet = toBet;
			if (currentBet == -1)
			{
				currentBet = 10;
			}
			slots = new List<float>();
			slots.Add(0f);
			slots.Add(0f);
			slots.Add(0f);
			slotResults = new List<float>();
			slotResults.Add(0f);
			slotResults.Add(0f);
			slotResults.Add(0f);
			Game1.playSound("newArtifact");
			setSlotResults(slots);
			Vector2 pos3 = Utility.getTopLeftPositionForCenteringOnScreen(104, 52, -16, 32);
			spinButton10 = new ClickableComponent(new Rectangle((int)pos3.X, (int)pos3.Y, 104, 52), Game1.content.LoadString("Strings\\StringsFromCSFiles:Slots.cs.12117"));
			pos3 = Utility.getTopLeftPositionForCenteringOnScreen(124, 52, -16, 96);
			spinButton100 = new ClickableComponent(new Rectangle((int)pos3.X, (int)pos3.Y, 124, 52), Game1.content.LoadString("Strings\\StringsFromCSFiles:Slots.cs.12118"));
			pos3 = Utility.getTopLeftPositionForCenteringOnScreen(96, 52, -16, 160);
			doneButton = new ClickableComponent(new Rectangle((int)pos3.X, (int)pos3.Y, 96, 52), Game1.content.LoadString("Strings\\StringsFromCSFiles:NameSelect.cs.3864"));
			if (Game1.isAnyGamePadButtonBeingPressed())
			{
				Game1.setMousePosition(spinButton10.bounds.Center);
				if (Game1.options.SnappyMenus)
				{
					currentlySnappedComponent = spinButton10;
				}
			}
		}

		public void setSlotResults(List<float> toSet)
		{
			double d = Game1.random.NextDouble();
			double modifier = 1.0 + Game1.player.DailyLuck * 2.0 + (double)Game1.player.LuckLevel * 0.08;
			if (d < 0.001 * modifier)
			{
				set(toSet, 5);
				payoutModifier = 2500f;
				return;
			}
			if (d < 0.0016 * modifier)
			{
				set(toSet, 6);
				payoutModifier = 1000f;
				return;
			}
			if (d < 0.0025 * modifier)
			{
				set(toSet, 7);
				payoutModifier = 500f;
				return;
			}
			if (d < 0.005 * modifier)
			{
				set(toSet, 4);
				payoutModifier = 200f;
				return;
			}
			if (d < 0.007 * modifier)
			{
				set(toSet, 3);
				payoutModifier = 120f;
				return;
			}
			if (d < 0.01 * modifier)
			{
				set(toSet, 2);
				payoutModifier = 80f;
				return;
			}
			if (d < 0.02 * modifier)
			{
				set(toSet, 1);
				payoutModifier = 30f;
				return;
			}
			if (d < 0.12 * modifier)
			{
				int whereToPutNonStar = Game1.random.Next(3);
				for (int k = 0; k < 3; k++)
				{
					toSet[k] = ((k == whereToPutNonStar) ? Game1.random.Next(7) : 7);
				}
				payoutModifier = 3f;
				return;
			}
			if (d < 0.2 * modifier)
			{
				set(toSet, 0);
				payoutModifier = 5f;
				return;
			}
			if (d < 0.4 * modifier)
			{
				int whereToPutStar = Game1.random.Next(3);
				for (int j = 0; j < 3; j++)
				{
					toSet[j] = ((j == whereToPutStar) ? 7 : Game1.random.Next(7));
				}
				payoutModifier = 2f;
				return;
			}
			payoutModifier = 0f;
			int[] used = new int[8];
			for (int i = 0; i < 3; i++)
			{
				int next = Game1.random.Next(6);
				while (used[next] > 1)
				{
					next = Game1.random.Next(6);
				}
				toSet[i] = next;
				used[next]++;
			}
		}

		private void set(List<float> toSet, int number)
		{
			toSet[0] = number;
			toSet[1] = number;
			toSet[2] = number;
		}

		public bool tick(GameTime time)
		{
			if (spinning && endTimer <= 0)
			{
				for (int i = slotsFinished; i < slots.Count; i++)
				{
					float old = slots[i];
					slots[i] += (float)time.ElapsedGameTime.Milliseconds * 0.008f * (1f - (float)i * 0.05f);
					slots[i] %= 8f;
					if (i == 2)
					{
						if (old % (0.25f + (float)slotsFinished * 0.5f) > slots[i] % (0.25f + (float)slotsFinished * 0.5f))
						{
							Game1.playSound("shiny4");
						}
						if (old > slots[i])
						{
							spinsCount++;
						}
					}
					if (spinsCount > 0 && i == slotsFinished && Math.Abs(slots[i] - slotResults[i]) <= (float)time.ElapsedGameTime.Milliseconds * 0.008f)
					{
						slots[i] = slotResults[i];
						slotsFinished++;
						spinsCount--;
						Game1.playSound("Cowboy_gunshot");
					}
				}
				if (slotsFinished >= 3)
				{
					endTimer = ((payoutModifier == 0f) ? 600 : 1000);
				}
			}
			if (endTimer > 0)
			{
				endTimer -= time.ElapsedGameTime.Milliseconds;
				if (endTimer <= 0)
				{
					spinning = false;
					spinsCount = 0;
					slotsFinished = 0;
					if (payoutModifier > 0f)
					{
						showResult = true;
						Game1.playSound((!(payoutModifier >= 5f)) ? "newArtifact" : ((payoutModifier >= 10f) ? "reward" : "money"));
					}
					else
					{
						Game1.playSound("breathout");
					}
					Game1.player.clubCoins += (int)((float)currentBet * payoutModifier);
					if (payoutModifier == 2500f)
					{
						Game1.multiplayer.globalChatInfoMessage("Jackpot", Game1.player.Name);
					}
				}
			}
			spinButton10.scale = ((!spinning && spinButton10.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
			spinButton100.scale = ((!spinning && spinButton100.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
			doneButton.scale = ((!spinning && doneButton.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
			return false;
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!spinning && Game1.player.clubCoins >= 10 && spinButton10.bounds.Contains(x, y))
			{
				Club.timesPlayedSlots++;
				setSlotResults(slotResults);
				spinning = true;
				Game1.playSound("bigSelect");
				currentBet = 10;
				slotsFinished = 0;
				spinsCount = 0;
				showResult = false;
				Game1.player.clubCoins -= 10;
			}
			if (!spinning && Game1.player.clubCoins >= 100 && spinButton100.bounds.Contains(x, y))
			{
				Club.timesPlayedSlots++;
				setSlotResults(slotResults);
				Game1.playSound("bigSelect");
				spinning = true;
				slotsFinished = 0;
				spinsCount = 0;
				showResult = false;
				currentBet = 100;
				Game1.player.clubCoins -= 100;
			}
			if (!spinning && doneButton.bounds.Contains(x, y))
			{
				Game1.playSound("bigDeSelect");
				Game1.currentMinigame = null;
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
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public void receiveKeyPress(Keys k)
		{
			if (!spinning && (k.Equals(Keys.Escape) || Game1.options.doesInputListContain(Game1.options.menuButton, k)))
			{
				unload();
				Game1.playSound("bigDeSelect");
				Game1.currentMinigame = null;
			}
			else
			{
				if (spinning || currentlySnappedComponent == null)
				{
					return;
				}
				if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
				{
					if (currentlySnappedComponent.Equals(spinButton10))
					{
						currentlySnappedComponent = spinButton100;
						Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
					}
					else if (currentlySnappedComponent.Equals(spinButton100))
					{
						currentlySnappedComponent = doneButton;
						Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
					}
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
				{
					if (currentlySnappedComponent.Equals(doneButton))
					{
						currentlySnappedComponent = spinButton100;
						Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
					}
					else if (currentlySnappedComponent.Equals(spinButton100))
					{
						currentlySnappedComponent = spinButton10;
						Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
					}
				}
			}
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public int getIconIndex(int index)
		{
			switch (index)
			{
			case 0:
				return 24;
			case 1:
				return 186;
			case 2:
				return 138;
			case 3:
				return 392;
			case 4:
				return 254;
			case 5:
				return 434;
			case 6:
				return 72;
			case 7:
				return 638;
			default:
				return 24;
			}
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), new Color(38, 0, 7));
			b.Draw(Game1.mouseCursors, Utility.getTopLeftPositionForCenteringOnScreen(228, 52, 0, -256), new Rectangle(441, 424, 57, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			for (int i = 0; i < 3; i++)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 112 + i * 26 * 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 128), new Rectangle(306, 320, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				float faceValue = (slots[i] + 1f) % 8f;
				int previous = getIconIndex(((int)faceValue + 8 - 1) % 8);
				int current = getIconIndex((previous + 1) % 8);
				b.Draw(Game1.objectSpriteSheet, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 112 + i * 26 * 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 128) - new Vector2(0f, -64f * (faceValue % 1f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, previous, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.objectSpriteSheet, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 112 + i * 26 * 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 128) - new Vector2(0f, 64f - 64f * (faceValue % 1f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, current, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 132 + i * 26 * 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 192), new Rectangle(415, 385, 26, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			}
			if (showResult)
			{
				SpriteText.drawString(b, "+" + payoutModifier * (float)currentBet, Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 372, spinButton10.bounds.Y - 64 + 8, 9999, -1, 9999, 1f, 1f, junimoText: false, -1, "", 4);
			}
			b.Draw(Game1.mouseCursors, new Vector2(spinButton10.bounds.X, spinButton10.bounds.Y), new Rectangle(441, 385, 26, 13), Color.White * ((!spinning && Game1.player.clubCoins >= 10) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * spinButton10.scale, SpriteEffects.None, 0.99f);
			b.Draw(Game1.mouseCursors, new Vector2(spinButton100.bounds.X, spinButton100.bounds.Y), new Rectangle(441, 398, 31, 13), Color.White * ((!spinning && Game1.player.clubCoins >= 100) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * spinButton100.scale, SpriteEffects.None, 0.99f);
			b.Draw(Game1.mouseCursors, new Vector2(doneButton.bounds.X, doneButton.bounds.Y), new Rectangle(441, 411, 24, 13), Color.White * ((!spinning) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * doneButton.scale, SpriteEffects.None, 0.99f);
			SpriteText.drawStringWithScrollBackground(b, coinBuffer + Game1.player.clubCoins, Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 376, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 120);
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 376 + 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 120 + 4), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Vector2 basePos = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 + 200, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 352);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), (int)basePos.X, (int)basePos.Y, 384, 704, Color.White, 4f);
			b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(8f, 8f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(7), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			SpriteText.drawString(b, "x2", (int)basePos.X + 192 + 16, (int)basePos.Y + 24, 9999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", 4);
			b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(8f, 76f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(7), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(76f, 76f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(7), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			SpriteText.drawString(b, "x3", (int)basePos.X + 192 + 16, (int)basePos.Y + 68 + 24, 9999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", 4);
			for (int j = 0; j < 8; j++)
			{
				int which = j;
				switch (j)
				{
				case 5:
					which = 7;
					break;
				case 7:
					which = 5;
					break;
				}
				b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(8f, 8 + (j + 2) * 68), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(which), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(76f, 8 + (j + 2) * 68), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(which), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(144f, 8 + (j + 2) * 68), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIconIndex(which), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				int payout = 0;
				switch (j)
				{
				case 0:
					payout = 5;
					break;
				case 1:
					payout = 30;
					break;
				case 2:
					payout = 80;
					break;
				case 3:
					payout = 120;
					break;
				case 4:
					payout = 200;
					break;
				case 5:
					payout = 500;
					break;
				case 6:
					payout = 1000;
					break;
				case 7:
					payout = 2500;
					break;
				}
				SpriteText.drawString(b, "x" + payout, (int)basePos.X + 192 + 16, (int)basePos.Y + (j + 2) * 68 + 24, 9999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", 4);
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), (int)basePos.X - 640, (int)basePos.Y, 1024, 704, Color.Red, 4f, drawShadow: false);
			for (int k = 1; k < 8; k++)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), (int)basePos.X - 640 - 4 * k, (int)basePos.Y - 4 * k, 1024 + 8 * k, 704 + 8 * k, Color.Red * (1f - (float)k * 0.15f), 4f, drawShadow: false);
			}
			for (int l = 0; l < 17; l++)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(147, 472, 3, 3), (int)basePos.X - 640 + 8, (int)basePos.Y + l * 4 * 3 + 12, (int)(608f - (float)(l * 64) * 1.2f + (float)(l * l * 4) * 0.7f), 4, new Color(l * 25, (l > 8) ? (l * 10) : 0, 255 - l * 25), 4f, drawShadow: false);
			}
			if (!Game1.options.hardwareCursor)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			b.End();
		}

		public void changeScreenSize()
		{
		}

		public void unload()
		{
		}

		public void receiveEventPoke(int data)
		{
		}

		public string minigameId()
		{
			return "Slots";
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public bool forceQuit()
		{
			if (spinning)
			{
				Game1.player.clubCoins += currentBet;
			}
			unload();
			return true;
		}
	}
}
