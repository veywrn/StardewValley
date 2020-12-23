using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using System;
using System.Text;

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

		private StringBuilder _hoverText = new StringBuilder();

		private StringBuilder _timeText = new StringBuilder();

		private StringBuilder _dateText = new StringBuilder();

		private StringBuilder _hours = new StringBuilder();

		private StringBuilder _padZeros = new StringBuilder();

		private StringBuilder _temp = new StringBuilder();

		private int _lastDayOfMonth = -1;

		private string _lastDayOfMonthString;

		private string _amString;

		private string _pmString;

		private LocalizedContentManager.LanguageCode _languageCode = (LocalizedContentManager.LanguageCode)(-1);

		public bool questsDirty;

		public int questPingTimer;

		public DayTimeMoneyBox()
			: base(Game1.uiViewport.Width - 300 + 32, 8, 300, 284)
		{
			position = new Vector2(xPositionOnScreen, yPositionOnScreen);
			sourceRect = new Rectangle(333, 431, 71, 43);
			questButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 220, yPositionOnScreen + 240, 44, 46), Game1.mouseCursors, new Rectangle(383, 493, 11, 14), 4f);
			zoomOutButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 92, yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(177, 345, 7, 8), 4f);
			zoomInButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 124, yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(184, 345, 7, 8), 4f);
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
			if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y) && Game1.player.CanMove && !Game1.dialogueUp && !Game1.eventUp && Game1.farmEvent == null)
			{
				Game1.activeClickableMenu = new QuestLog();
			}
			if (Game1.options.zoomButtons)
			{
				if (zoomInButton.containsPoint(x, y) && Game1.options.desiredBaseZoomLevel < 2f)
				{
					int zoom6 = (int)Math.Round(Game1.options.desiredBaseZoomLevel * 100f);
					zoom6 -= zoom6 % 5;
					zoom6 += 5;
					Game1.options.desiredBaseZoomLevel = Math.Min(2f, (float)zoom6 / 100f);
					Game1.forceSnapOnNextViewportUpdate = true;
					Game1.playSound("drumkit6");
				}
				else if (zoomOutButton.containsPoint(x, y) && Game1.options.desiredBaseZoomLevel > 0.75f)
				{
					int zoom3 = (int)Math.Round(Game1.options.desiredBaseZoomLevel * 100f);
					zoom3 -= zoom3 % 5;
					zoom3 -= 5;
					Game1.options.desiredBaseZoomLevel = Math.Max(0.75f, (float)zoom3 / 100f);
					Game1.forceSnapOnNextViewportUpdate = true;
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
			if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y))
			{
				_hoverText.Clear();
				_hoverText.Append(Game1.content.LoadString("Strings\\UI:QuestButton_Hover", Game1.options.journalButton[0].ToString()));
			}
			if (Game1.options.zoomButtons)
			{
				if (zoomInButton.containsPoint(x, y))
				{
					_hoverText.Clear();
					_hoverText.Append(Game1.content.LoadString("Strings\\UI:ZoomInButton_Hover"));
				}
				else if (zoomOutButton.containsPoint(x, y))
				{
					_hoverText.Clear();
					_hoverText.Append(Game1.content.LoadString("Strings\\UI:ZoomOutButton_Hover"));
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

		public override void update(GameTime time)
		{
			base.update(time);
			if (_languageCode != LocalizedContentManager.CurrentLanguageCode)
			{
				_languageCode = LocalizedContentManager.CurrentLanguageCode;
				_amString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370");
				_pmString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371");
			}
			if (questPingTimer > 0)
			{
				questPingTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (questPingTimer < 0)
			{
				questPingTimer = 0;
			}
			if (questsDirty)
			{
				if (Game1.player.hasPendingCompletedQuests)
				{
					PingQuestLog();
				}
				questsDirty = false;
			}
		}

		public virtual void PingQuestLog()
		{
			questPingTimer = 6000;
		}

		public virtual void DismissQuestPing()
		{
			questPingTimer = 0;
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
			if (Game1.dayOfMonth != _lastDayOfMonth)
			{
				_lastDayOfMonth = Game1.dayOfMonth;
				_lastDayOfMonthString = Game1.shortDayDisplayNameFromDayOfSeason(_lastDayOfMonth);
			}
			_dateText.Clear();
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				_dateText.AppendEx(Game1.dayOfMonth);
				_dateText.Append("日 (");
				_dateText.Append(_lastDayOfMonthString);
				_dateText.Append(")");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				_dateText.Append(_lastDayOfMonthString);
				_dateText.Append(" ");
				_dateText.AppendEx(Game1.dayOfMonth);
				_dateText.Append("日");
			}
			else
			{
				_dateText.Append(_lastDayOfMonthString);
				_dateText.Append(". ");
				_dateText.AppendEx(Game1.dayOfMonth);
			}
			Vector2 daySize = font.MeasureString(_dateText);
			Vector2 dayPosition = new Vector2((float)sourceRect.X * 0.55f - daySize.X / 2f, (float)sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.1f : 0.1f) - daySize.Y / 2f);
			Utility.drawTextWithShadow(b, _dateText, font, position + dayPosition, Game1.textColor);
			b.Draw(Game1.mouseCursors, position + new Vector2(212f, 68f), new Rectangle(406, 441 + Utility.getSeasonNumber(Game1.currentSeason) * 8, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			b.Draw(Game1.mouseCursors, position + new Vector2(116f, 68f), new Rectangle(317 + 12 * Game1.weatherIcon, 421, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			_padZeros.Clear();
			if (Game1.timeOfDay % 100 == 0)
			{
				_padZeros.Append("0");
			}
			_hours.Clear();
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.zh:
				if (Game1.timeOfDay / 100 % 24 == 0)
				{
					_hours.Append("00");
				}
				else if (Game1.timeOfDay / 100 % 12 == 0)
				{
					_hours.Append("12");
				}
				else
				{
					_hours.AppendEx(Game1.timeOfDay / 100 % 12);
				}
				break;
			case LocalizedContentManager.LanguageCode.ru:
			case LocalizedContentManager.LanguageCode.pt:
			case LocalizedContentManager.LanguageCode.es:
			case LocalizedContentManager.LanguageCode.de:
			case LocalizedContentManager.LanguageCode.th:
			case LocalizedContentManager.LanguageCode.fr:
			case LocalizedContentManager.LanguageCode.tr:
			case LocalizedContentManager.LanguageCode.hu:
				_temp.Clear();
				_temp.AppendEx(Game1.timeOfDay / 100 % 24);
				if (Game1.timeOfDay / 100 % 24 <= 9)
				{
					_hours.Append("0");
				}
				_hours.AppendEx(_temp);
				break;
			default:
				if (Game1.timeOfDay / 100 % 12 == 0)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
					{
						_hours.Append("0");
					}
					else
					{
						_hours.Append("12");
					}
				}
				else
				{
					_hours.AppendEx(Game1.timeOfDay / 100 % 12);
				}
				break;
			}
			_timeText.Clear();
			_timeText.AppendEx(_hours);
			_timeText.Append(":");
			_timeText.AppendEx(Game1.timeOfDay % 100);
			_timeText.AppendEx(_padZeros);
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it)
			{
				_timeText.Append(" ");
				if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
				{
					_timeText.Append(_amString);
				}
				else
				{
					_timeText.Append(_pmString);
				}
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
				{
					_timeText.Append(_amString);
				}
				else
				{
					_timeText.Append(_pmString);
				}
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				_temp.Clear();
				_temp.AppendEx(_timeText);
				_timeText.Clear();
				if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
				{
					_timeText.Append(_amString);
					_timeText.Append(" ");
					_timeText.AppendEx(_temp);
				}
				else
				{
					_timeText.Append(_pmString);
					_timeText.Append(" ");
					_timeText.AppendEx(_temp);
				}
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				_temp.Clear();
				_temp.AppendEx(_timeText);
				_timeText.Clear();
				if (Game1.timeOfDay < 600 || Game1.timeOfDay >= 2400)
				{
					_timeText.Append("凌晨 ");
					_timeText.AppendEx(_temp);
				}
				else if (Game1.timeOfDay < 1200)
				{
					_timeText.Append(_amString);
					_timeText.Append(" ");
					_timeText.AppendEx(_temp);
				}
				else if (Game1.timeOfDay < 1300)
				{
					_timeText.Append("中午  ");
					_timeText.AppendEx(_temp);
				}
				else if (Game1.timeOfDay < 1900)
				{
					_timeText.Append(_pmString);
					_timeText.Append(" ");
					_timeText.AppendEx(_temp);
				}
				else
				{
					_timeText.Append("晚上  ");
					_timeText.AppendEx(_temp);
				}
			}
			Vector2 txtSize = font.MeasureString(_timeText);
			Vector2 timePosition = new Vector2((float)sourceRect.X * 0.55f - txtSize.X / 2f + (float)((timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0), (float)sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.31f : 0.31f) - txtSize.Y / 2f + (float)((timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0));
			bool nofade = Game1.shouldTimePass() || Game1.fadeToBlack || Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0 > 1000.0;
			Utility.drawTextWithShadow(b, _timeText, font, position + timePosition, (Game1.timeOfDay >= 2400) ? Color.Red : (Game1.textColor * (nofade ? 1f : 0.5f)));
			int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
			if (Game1.player.visibleQuestCount > 0)
			{
				questButton.draw(b);
				if (questPulseTimer > 0)
				{
					float scaleMult = 1f / (Math.Max(300f, Math.Abs(questPulseTimer % 1000 - 500)) / 500f);
					b.Draw(Game1.mouseCursors, new Vector2(questButton.bounds.X + 24, questButton.bounds.Y + 32) + ((scaleMult > 1f) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(2f, 4f), 4f * scaleMult, SpriteEffects.None, 0.99f);
				}
				if (questPingTimer > 0)
				{
					b.Draw(Game1.mouseCursors, new Vector2(Game1.dayTimeMoneyBox.questButton.bounds.Left - 16, Game1.dayTimeMoneyBox.questButton.bounds.Bottom + 8), new Rectangle(128 + ((questPingTimer / 200 % 2 != 0) ? 16 : 0), 208, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
				}
			}
			if (Game1.options.zoomButtons)
			{
				zoomInButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel >= 2f) ? 0.5f : 1f), 1f);
				zoomOutButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel <= 0.75f) ? 0.5f : 1f), 1f);
			}
			drawMoneyBox(b);
			if (_hoverText.Length > 0 && isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				IClickableMenu.drawHoverText(b, _hoverText, Game1.dialogueFont);
			}
			b.Draw(Game1.mouseCursors, position + new Vector2(88f, 88f), new Rectangle(324, 477, 7, 19), Color.White, (float)(Math.PI + Math.Min(Math.PI, (double)(((float)adjustedTime + (float)Game1.gameTimeInterval / 7000f * 16.6f - 600f) / 2000f) * Math.PI)), new Vector2(3f, 17f), 4f, SpriteEffects.None, 0.9f);
		}

		private void updatePosition()
		{
			position = new Vector2(Game1.uiViewport.Width - 300, 8f);
			if (Game1.isOutdoorMapSmallerThanViewport())
			{
				position = new Vector2(Math.Min(position.X, -Game1.uiViewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 300), 8f);
			}
			Utility.makeSafe(ref position, 300, 284);
			xPositionOnScreen = (int)position.X;
			yPositionOnScreen = (int)position.Y;
			questButton.bounds = new Rectangle(xPositionOnScreen + 212, yPositionOnScreen + 240, 44, 46);
			zoomOutButton.bounds = new Rectangle(xPositionOnScreen + 92, yPositionOnScreen + 244, 28, 32);
			zoomInButton.bounds = new Rectangle(xPositionOnScreen + 124, yPositionOnScreen + 244, 28, 32);
		}
	}
}
