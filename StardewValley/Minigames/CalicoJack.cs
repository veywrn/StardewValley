using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Minigames
{
	public class CalicoJack : IMinigame
	{
		public const int cardState_flipped = -1;

		public const int cardState_up = 0;

		public const int cardState_transitioning = 400;

		public const int bet = 100;

		public const int cardWidth = 96;

		public const int dealTime = 1000;

		public const int playingTo = 21;

		public const int passNumber = 18;

		public const int dealerTurnDelay = 1000;

		public List<int[]> playerCards;

		public List<int[]> dealerCards;

		private Random r;

		private int currentBet;

		private int startTimer;

		private int dealerTurnTimer = -1;

		private int bustTimer;

		private ClickableComponent hit;

		private ClickableComponent stand;

		private ClickableComponent doubleOrNothing;

		private ClickableComponent playAgain;

		private ClickableComponent quit;

		private ClickableComponent currentlySnappedComponent;

		private bool showingResultsScreen;

		private bool playerWon;

		private bool highStakes;

		private string endMessage = "";

		private string endTitle = "";

		private string coinBuffer;

		public CalicoJack(int toBet = -1, bool highStakes = false)
		{
			coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? "     " : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  "));
			this.highStakes = highStakes;
			startTimer = 1000;
			playerCards = new List<int[]>();
			dealerCards = new List<int[]>();
			if (toBet == -1)
			{
				currentBet = (highStakes ? 1000 : 100);
			}
			else
			{
				currentBet = toBet;
			}
			Club.timesPlayedCalicoJack++;
			r = new Random(Club.timesPlayedCalicoJack + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame);
			hit = new ClickableComponent(new Rectangle((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924"))), Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 64, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924") + "  "), 64), "", " " + Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924") + " ");
			stand = new ClickableComponent(new Rectangle((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927"))), Game1.graphics.GraphicsDevice.Viewport.Height / 2 + 32, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927") + "  "), 64), "", " " + Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927") + " ");
			doubleOrNothing = new ClickableComponent(new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930")) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel), SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930")) + 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930"));
			playAgain = new ClickableComponent(new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933")) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel) + 64 + 16, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933")) + 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933"));
			quit = new ClickableComponent(new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936")) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel) + 64 + 96, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936")) + 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936"));
			RepositionButtons();
			if (Game1.options.SnappyMenus)
			{
				currentlySnappedComponent = hit;
				currentlySnappedComponent.snapMouseCursorToCenter();
			}
		}

		public void RepositionButtons()
		{
			hit.bounds = new Rectangle((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924"))), Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 64, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924") + "  "), 64);
			stand.bounds = new Rectangle((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927"))), Game1.graphics.GraphicsDevice.Viewport.Height / 2 + 32, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927") + "  "), 64);
			doubleOrNothing.bounds = new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - (SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930")) + 64) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel), SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930")) + 64, 64);
			playAgain.bounds = new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - (SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933")) + 64) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel) + 64 + 16, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933")) + 64, 64);
			quit.bounds = new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - (SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936")) + 64) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel) + 64 + 96, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936")) + 64, 64);
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool playButtonsActive()
		{
			if (startTimer <= 0 && dealerTurnTimer < 0)
			{
				return !showingResultsScreen;
			}
			return false;
		}

		public bool tick(GameTime time)
		{
			for (int l = 0; l < playerCards.Count; l++)
			{
				if (playerCards[l][1] > 0)
				{
					playerCards[l][1] -= time.ElapsedGameTime.Milliseconds;
					if (playerCards[l][1] <= 0)
					{
						playerCards[l][1] = 0;
					}
				}
			}
			for (int k = 0; k < dealerCards.Count; k++)
			{
				if (dealerCards[k][1] > 0)
				{
					dealerCards[k][1] -= time.ElapsedGameTime.Milliseconds;
					if (dealerCards[k][1] <= 0)
					{
						dealerCards[k][1] = 0;
					}
				}
			}
			if (startTimer > 0)
			{
				int oldTimer = startTimer;
				startTimer -= time.ElapsedGameTime.Milliseconds;
				if (oldTimer % 250 < startTimer % 250)
				{
					switch (oldTimer / 250)
					{
					case 4:
						dealerCards.Add(new int[2]
						{
							r.Next(1, 12),
							-1
						});
						break;
					case 3:
						dealerCards.Add(new int[2]
						{
							r.Next(1, 10),
							400
						});
						break;
					case 2:
						playerCards.Add(new int[2]
						{
							r.Next(1, 12),
							400
						});
						break;
					case 1:
						playerCards.Add(new int[2]
						{
							r.Next(1, 10),
							400
						});
						break;
					}
					Game1.playSound("shwip");
				}
			}
			else if (bustTimer > 0)
			{
				bustTimer -= time.ElapsedGameTime.Milliseconds;
				if (bustTimer <= 0)
				{
					endGame();
				}
			}
			else if (dealerTurnTimer > 0 && !showingResultsScreen)
			{
				dealerTurnTimer -= time.ElapsedGameTime.Milliseconds;
				if (dealerTurnTimer <= 0)
				{
					int dealerTotal2 = 0;
					foreach (int[] j in dealerCards)
					{
						dealerTotal2 += j[0];
					}
					int playertotal = 0;
					foreach (int[] i in playerCards)
					{
						playertotal += i[0];
					}
					if (dealerCards[0][1] == -1)
					{
						dealerCards[0][1] = 400;
						Game1.playSound("shwip");
					}
					else if (dealerTotal2 < 18 || (dealerTotal2 < playertotal && playertotal <= 21))
					{
						int nextCard = r.Next(1, 10);
						int dealerDistance = 21 - dealerTotal2;
						if (playertotal == 20 && r.NextDouble() < 0.5)
						{
							nextCard = dealerDistance + r.Next(1, 4);
						}
						else if (playertotal == 19 && r.NextDouble() < 0.25)
						{
							nextCard = dealerDistance + r.Next(1, 4);
						}
						else if (playertotal == 18 && r.NextDouble() < 0.1)
						{
							nextCard = dealerDistance + r.Next(1, 4);
						}
						dealerCards.Add(new int[2]
						{
							nextCard,
							400
						});
						dealerTotal2 += dealerCards.Last()[0];
						Game1.playSound("shwip");
						if (dealerTotal2 > 21)
						{
							bustTimer = 2000;
						}
					}
					else
					{
						bustTimer = 50;
					}
					dealerTurnTimer = 1000;
				}
			}
			if (playButtonsActive())
			{
				hit.scale = (hit.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
				stand.scale = (stand.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
			}
			else if (showingResultsScreen)
			{
				doubleOrNothing.scale = (doubleOrNothing.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
				playAgain.scale = (playAgain.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
				quit.scale = (quit.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
			}
			return false;
		}

		public void endGame()
		{
			if (Game1.options.SnappyMenus)
			{
				currentlySnappedComponent = quit;
				currentlySnappedComponent.snapMouseCursorToCenter();
			}
			showingResultsScreen = true;
			int playertotal = 0;
			foreach (int[] j in playerCards)
			{
				playertotal += j[0];
			}
			if (playertotal == 21)
			{
				Game1.playSound("reward");
				playerWon = true;
				endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
				endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11944");
				Game1.player.clubCoins += currentBet;
				return;
			}
			if (playertotal > 21)
			{
				Game1.playSound("fishEscape");
				endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11946");
				endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11947");
				Game1.player.clubCoins -= currentBet;
				return;
			}
			int dealerTotal = 0;
			foreach (int[] i in dealerCards)
			{
				dealerTotal += i[0];
			}
			if (dealerTotal > 21)
			{
				Game1.playSound("reward");
				playerWon = true;
				endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
				endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11950");
				Game1.player.clubCoins += currentBet;
			}
			else if (playertotal == dealerTotal)
			{
				endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11951");
				endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11952");
			}
			else if (playertotal > dealerTotal)
			{
				Game1.playSound("reward");
				endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
				endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11955", 21);
				Game1.player.clubCoins += currentBet;
				playerWon = true;
			}
			else
			{
				Game1.playSound("fishEscape");
				endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11946");
				endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11958", 21);
				Game1.player.clubCoins -= currentBet;
			}
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (playButtonsActive() && bustTimer <= 0)
			{
				if (hit.bounds.Contains(x, y))
				{
					int playertotal = 0;
					foreach (int[] j in playerCards)
					{
						playertotal += j[0];
					}
					int nextCard = r.Next(1, 10);
					int distance = 21 - playertotal;
					if (distance > 1 && distance < 6 && r.NextDouble() < (double)(1f / (float)distance))
					{
						nextCard = ((r.NextDouble() < 0.5) ? distance : (distance - 1));
					}
					playerCards.Add(new int[2]
					{
						nextCard,
						400
					});
					Game1.playSound("shwip");
					int total = 0;
					foreach (int[] i in playerCards)
					{
						total += i[0];
					}
					if (total == 21)
					{
						bustTimer = 1000;
					}
					else if (total > 21)
					{
						bustTimer = 1000;
					}
				}
				if (stand.bounds.Contains(x, y))
				{
					dealerTurnTimer = 1000;
					Game1.playSound("coin");
				}
			}
			else if (showingResultsScreen)
			{
				if (playerWon && doubleOrNothing.containsPoint(x, y))
				{
					Game1.currentMinigame = new CalicoJack(currentBet * 2, highStakes);
					Game1.playSound("bigSelect");
				}
				if (Game1.player.clubCoins >= currentBet && playAgain.containsPoint(x, y))
				{
					Game1.currentMinigame = new CalicoJack(-1, highStakes);
					Game1.playSound("smallSelect");
				}
				if (quit.containsPoint(x, y))
				{
					Game1.currentMinigame = null;
					Game1.playSound("bigDeSelect");
				}
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

		public void receiveKeyPress(Keys k)
		{
			if (!Game1.options.SnappyMenus)
			{
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
			{
				if (currentlySnappedComponent.Equals(stand))
				{
					currentlySnappedComponent = hit;
				}
				else if (currentlySnappedComponent.Equals(playAgain) && playerWon)
				{
					currentlySnappedComponent = doubleOrNothing;
				}
				else if (currentlySnappedComponent.Equals(quit) && Game1.player.clubCoins >= currentBet)
				{
					currentlySnappedComponent = playAgain;
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
			{
				if (currentlySnappedComponent.Equals(hit))
				{
					currentlySnappedComponent = stand;
				}
				else if (currentlySnappedComponent.Equals(doubleOrNothing))
				{
					currentlySnappedComponent = playAgain;
				}
				else if (currentlySnappedComponent.Equals(playAgain))
				{
					currentlySnappedComponent = quit;
				}
			}
			if (currentlySnappedComponent != null)
			{
				currentlySnappedComponent.snapMouseCursorToCenter();
			}
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), highStakes ? new Color(130, 0, 82) : Color.DarkGreen);
			Vector2 coin_draw_pos = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width - 192, 32f);
			SpriteText.drawStringWithScrollBackground(b, coinBuffer + Game1.player.clubCoins, (int)coin_draw_pos.X, (int)coin_draw_pos.Y);
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(coin_draw_pos.X + 4f, coin_draw_pos.Y + 4f), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			if (showingResultsScreen)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, endMessage, Game1.graphics.GraphicsDevice.Viewport.Width / 2, 48);
				SpriteText.drawStringWithScrollCenteredAt(b, endTitle, Game1.graphics.GraphicsDevice.Viewport.Width / 2, 128);
				if (!endTitle.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11951")))
				{
					SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11965", (playerWon ? "" : "-") + currentBet + "   "), Game1.graphics.GraphicsDevice.Viewport.Width / 2, 256);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 32 + SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11965", (playerWon ? "" : "-") + currentBet + "   ")) / 2, 260f) + new Vector2(8f, 0f), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				}
				if (playerWon)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), doubleOrNothing.bounds.X, doubleOrNothing.bounds.Y, doubleOrNothing.bounds.Width, doubleOrNothing.bounds.Height, Color.White, 4f * doubleOrNothing.scale);
					SpriteText.drawString(b, doubleOrNothing.label, doubleOrNothing.bounds.X + 32, doubleOrNothing.bounds.Y + 8);
				}
				if (Game1.player.clubCoins >= currentBet)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), playAgain.bounds.X, playAgain.bounds.Y, playAgain.bounds.Width, playAgain.bounds.Height, Color.White, 4f * playAgain.scale);
					SpriteText.drawString(b, playAgain.label, playAgain.bounds.X + 32, playAgain.bounds.Y + 8);
				}
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), quit.bounds.X, quit.bounds.Y, quit.bounds.Width, quit.bounds.Height, Color.White, 4f * quit.scale);
				SpriteText.drawString(b, quit.label, quit.bounds.X + 32, quit.bounds.Y + 8);
			}
			else
			{
				Vector2 start = new Vector2(128f, Game1.graphics.GraphicsDevice.Viewport.Height - 320);
				int total = 0;
				foreach (int[] j in playerCards)
				{
					int cardHeight2 = 144;
					if (j[1] > 0)
					{
						cardHeight2 = (int)(Math.Abs((float)j[1] - 200f) / 200f * 144f);
					}
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, (j[1] > 200 || j[1] == -1) ? new Rectangle(399, 396, 15, 15) : new Rectangle(384, 396, 15, 15), (int)start.X, (int)start.Y + 72 - cardHeight2 / 2, 96, cardHeight2, Color.White, 4f);
					if (j[1] == 0)
					{
						SpriteText.drawStringHorizontallyCenteredAt(b, string.Concat(j[0]), (int)start.X + 48 - 8 + 4, (int)start.Y + 72 - 16);
					}
					start.X += 112f;
					if (j[1] == 0)
					{
						total += j[0];
					}
				}
				SpriteText.drawStringWithScrollBackground(b, Game1.player.Name + ": " + total, 160, (int)start.Y + 144 + 32);
				start.X = 128f;
				start.Y = 128f;
				total = 0;
				foreach (int[] i in dealerCards)
				{
					int cardHeight = 144;
					if (i[1] > 0)
					{
						cardHeight = (int)(Math.Abs((float)i[1] - 200f) / 200f * 144f);
					}
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, (i[1] > 200 || i[1] == -1) ? new Rectangle(399, 396, 15, 15) : new Rectangle(384, 396, 15, 15), (int)start.X, (int)start.Y + 72 - cardHeight / 2, 96, cardHeight, Color.White, 4f);
					if (i[1] == 0)
					{
						SpriteText.drawStringHorizontallyCenteredAt(b, string.Concat(i[0]), (int)start.X + 48 - 8 + 4, (int)start.Y + 72 - 16);
					}
					start.X += 112f;
					if (i[1] == 0)
					{
						total += i[0];
					}
					else if (i[1] == -1)
					{
						total = -99999;
					}
				}
				SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11970", (total > 0) ? string.Concat(total) : "?"), 160, 32);
				SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11972", currentBet + coinBuffer), 160, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 48);
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(172 + SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11972", currentBet)), Game1.graphics.GraphicsDevice.Viewport.Height / 2 + 4 - 48), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				if (playButtonsActive())
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), hit.bounds.X, hit.bounds.Y, hit.bounds.Width, hit.bounds.Height, Color.White, 4f * hit.scale);
					SpriteText.drawString(b, hit.label, hit.bounds.X + 8, hit.bounds.Y + 8);
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), stand.bounds.X, stand.bounds.Y, stand.bounds.Width, stand.bounds.Height, Color.White, 4f * stand.scale);
					SpriteText.drawString(b, stand.label, stand.bounds.X + 8, stand.bounds.Y + 8);
				}
			}
			if (!Game1.options.hardwareCursor)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			b.End();
		}

		public void changeScreenSize()
		{
			RepositionButtons();
		}

		public void unload()
		{
		}

		public void receiveEventPoke(int data)
		{
		}

		public string minigameId()
		{
			return "CalicoJack";
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public bool forceQuit()
		{
			return true;
		}
	}
}
