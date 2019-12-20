using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using System;

namespace StardewValley.Menus
{
	public class DayTimeMoneyBox : IClickableMenu
	{
		public new const int width = 300;

		public new const int height = 284;

		public Vector2 position;

		private Rectangle sourceRect;

		public MoneyDial moneyDial = new MoneyDial(8);

		public int timeShakeTimer;

		public int moneyShakeTimer;

		public int questPulseTimer;

		public int whenToPulseTimer;

		public ClickableTextureComponent questButton;

		public ClickableTextureComponent zoomOutButton;

		public ClickableTextureComponent zoomInButton;

		private string hoverText = "";

		public DayTimeMoneyBox()
			: base(Game1.viewport.Width - 300 + 32, 8, 300, 284)
		{
			position = new Vector2(xPositionOnScreen, yPositionOnScreen);
			sourceRect = new Rectangle(333, 431, 71, 43);
			questButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 220, yPositionOnScreen + 240, 44, 46), Game1.mouseCursors, new Rectangle(383, 493, 11, 14), 4f);
			zoomOutButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 92, yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(177, 345, 7, 8), 4f);
			zoomInButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 124, yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(184, 345, 7, 8), 4f);
		}

		public override bool isWithinBounds(int x, int y)
		{
			if (Game1.options.zoomButtons && (zoomInButton.containsPoint(x, y) || zoomOutButton.containsPoint(x, y)))
			{
				return true;
			}
			if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y))
			{
				return true;
			}
			return false;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			updatePosition();
			if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y))
			{
				Game1.activeClickableMenu = new QuestLog();
			}
			if (Game1.options.zoomButtons)
			{
				if (zoomInButton.containsPoint(x, y) && Game1.options.zoomLevel < 1.25f)
				{
					int zoom6 = (int)Math.Round(Game1.options.zoomLevel * 100f);
					zoom6 -= zoom6 % 5;
					zoom6 += 5;
					Game1.options.zoomLevel = Math.Min(1.25f, (float)zoom6 / 100f);
					Program.gamePtr.refreshWindowSettings();
					Game1.playSound("drumkit6");
				}
				else if (zoomOutButton.containsPoint(x, y) && Game1.options.zoomLevel > 0.75f)
				{
					int zoom3 = (int)Math.Round(Game1.options.zoomLevel * 100f);
					zoom3 -= zoom3 % 5;
					zoom3 -= 5;
					Game1.options.zoomLevel = Math.Max(0.75f, (float)zoom3 / 100f);
					Program.gamePtr.refreshWindowSettings();
					Game1.playSound("drumkit6");
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			updatePosition();
		}

		public void questIconPulse()
		{
			questPulseTimer = 2000;
		}

		public override void performHoverAction(int x, int y)
		{
			updatePosition();
			hoverText = "";
			if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y))
			{
				hoverText = Game1.content.LoadString("Strings\\UI:QuestButton_Hover", Game1.options.journalButton[0].ToString());
			}
			if (Game1.options.zoomButtons)
			{
				if (zoomInButton.containsPoint(x, y))
				{
					hoverText = Game1.content.LoadString("Strings\\UI:ZoomInButton_Hover");
				}
				if (zoomOutButton.containsPoint(x, y))
				{
					hoverText = Game1.content.LoadString("Strings\\UI:ZoomOutButton_Hover");
				}
			}
		}

		public void drawMoneyBox(SpriteBatch b, int overrideX = -1, int overrideY = -1)
		{
			updatePosition();
			b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? position.X : ((float)overrideX), overrideY - 172) : position) + new Vector2(28 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 172 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), new Rectangle(340, 472, 65, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			moneyDial.draw(b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? position.X : ((float)overrideX), overrideY - 172) : position) + new Vector2(68 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 196 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), Game1.player.Money);
			if (moneyShakeTimer > 0)
			{
				moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
		}

		public override void draw(SpriteBatch b)
		{
			SpriteFont font = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont;
			updatePosition();
			if (timeShakeTimer > 0)
			{
				timeShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (questPulseTimer > 0)
			{
				questPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (whenToPulseTimer >= 0)
			{
				whenToPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				if (whenToPulseTimer <= 0)
				{
					whenToPulseTimer = 3000;
					if (Game1.player.hasNewQuestActivity())
					{
						questPulseTimer = 1000;
					}
				}
			}
			b.Draw(Game1.mouseCursors, position, sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			string dateText = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja) ? (Game1.dayOfMonth + "日 (" + Game1.shortDayDisplayNameFromDayOfSeason(Game1.dayOfMonth) + ")") : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? (Game1.shortDayDisplayNameFromDayOfSeason(Game1.dayOfMonth) + " " + Game1.dayOfMonth + "日") : (Game1.shortDayDisplayNameFromDayOfSeason(Game1.dayOfMonth) + ". " + Game1.dayOfMonth));
			Vector2 daySize = font.MeasureString(dateText);
			Vector2 dayPosition = new Vector2((float)sourceRect.X * 0.55f - daySize.X / 2f, (float)sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.1f : 0.1f) - daySize.Y / 2f);
			Utility.drawTextWithShadow(b, dateText, font, position + dayPosition, Game1.textColor);
			b.Draw(Game1.mouseCursors, position + new Vector2(212f, 68f), new Rectangle(406, 441 + Utility.getSeasonNumber(Game1.currentSeason) * 8, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			b.Draw(Game1.mouseCursors, position + new Vector2(116f, 68f), new Rectangle(317 + 12 * Game1.weatherIcon, 421, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			string zeroPad = (Game1.timeOfDay % 100 == 0) ? "0" : "";
			string hours2 = (Game1.timeOfDay / 100 % 12 == 0) ? "12" : string.Concat(Game1.timeOfDay / 100 % 12);
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.en:
			case LocalizedContentManager.LanguageCode.ko:
			case LocalizedContentManager.LanguageCode.it:
				hours2 = ((Game1.timeOfDay / 100 % 12 == 0) ? "12" : string.Concat(Game1.timeOfDay / 100 % 12));
				break;
			case LocalizedContentManager.LanguageCode.ja:
				hours2 = ((Game1.timeOfDay / 100 % 12 == 0) ? "0" : string.Concat(Game1.timeOfDay / 100 % 12));
				break;
			case LocalizedContentManager.LanguageCode.zh:
				hours2 = ((Game1.timeOfDay / 100 % 24 == 0) ? "00" : ((Game1.timeOfDay / 100 % 12 == 0) ? "12" : string.Concat(Game1.timeOfDay / 100 % 12)));
				break;
			case LocalizedContentManager.LanguageCode.ru:
			case LocalizedContentManager.LanguageCode.pt:
			case LocalizedContentManager.LanguageCode.es:
			case LocalizedContentManager.LanguageCode.de:
			case LocalizedContentManager.LanguageCode.th:
			case LocalizedContentManager.LanguageCode.fr:
			case LocalizedContentManager.LanguageCode.tr:
			case LocalizedContentManager.LanguageCode.hu:
				hours2 = string.Concat(Game1.timeOfDay / 100 % 24);
				hours2 = ((Game1.timeOfDay / 100 % 24 <= 9) ? ("0" + hours2) : hours2);
				break;
			}
			string timeText = hours2 + ":" + Game1.timeOfDay % 100 + zeroPad;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it)
			{
				timeText = timeText + " " + ((Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371"));
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				timeText += ((Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371"));
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				timeText = ((Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") + " " + timeText) : (Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371") + " " + timeText));
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				timeText = ((Game1.timeOfDay < 600 || Game1.timeOfDay >= 2400) ? ("凌晨 " + timeText) : ((Game1.timeOfDay < 1200) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") + " " + timeText) : ((Game1.timeOfDay < 1300) ? ("中午  " + timeText) : ((Game1.timeOfDay < 1900) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371") + " " + timeText) : ("晚上  " + timeText)))));
			}
			Vector2 txtSize = font.MeasureString(timeText);
			Vector2 timePosition = new Vector2((float)sourceRect.X * 0.55f - txtSize.X / 2f + (float)((timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0), (float)sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.31f : 0.31f) - txtSize.Y / 2f + (float)((timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0));
			bool nofade = Game1.shouldTimePass() || Game1.fadeToBlack || Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0 > 1000.0;
			Utility.drawTextWithShadow(b, timeText, font, position + timePosition, (Game1.timeOfDay >= 2400) ? Color.Red : (Game1.textColor * (nofade ? 1f : 0.5f)));
			int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
			if (Game1.player.visibleQuestCount > 0)
			{
				questButton.draw(b);
				if (questPulseTimer > 0)
				{
					float scaleMult = 1f / (Math.Max(300f, Math.Abs(questPulseTimer % 1000 - 500)) / 500f);
					b.Draw(Game1.mouseCursors, new Vector2(questButton.bounds.X + 24, questButton.bounds.Y + 32) + ((scaleMult > 1f) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(2f, 4f), 4f * scaleMult, SpriteEffects.None, 0.99f);
				}
			}
			if (Game1.options.zoomButtons)
			{
				zoomInButton.draw(b, Color.White * ((Game1.options.zoomLevel >= 1.25f) ? 0.5f : 1f), 1f);
				zoomOutButton.draw(b, Color.White * ((Game1.options.zoomLevel <= 0.75f) ? 0.5f : 1f), 1f);
			}
			drawMoneyBox(b);
			if (!hoverText.Equals("") && isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
			}
			b.Draw(Game1.mouseCursors, position + new Vector2(88f, 88f), new Rectangle(324, 477, 7, 19), Color.White, (float)(Math.PI + Math.Min(Math.PI, (double)(((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f - 600f) / 2000f) * Math.PI)), new Vector2(3f, 17f), 4f, SpriteEffects.None, 0.9f);
		}

		private void updatePosition()
		{
			position = new Vector2(Game1.viewport.Width - 300, 8f);
			if (Game1.isOutdoorMapSmallerThanViewport())
			{
				position = new Vector2(Math.Min(position.X, -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 300), 8f);
			}
			Utility.makeSafe(ref position, 300, 284);
			xPositionOnScreen = (int)position.X;
			yPositionOnScreen = (int)position.Y;
			questButton.bounds = new Rectangle(xPositionOnScreen + 212, yPositionOnScreen + 240, 44, 46);
			zoomOutButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 92, yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(177, 345, 7, 8), 4f);
			zoomInButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 124, yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(184, 345, 7, 8), 4f);
		}
	}
}
