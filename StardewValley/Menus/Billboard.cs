using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class Billboard : IClickableMenu
	{
		private Texture2D billboardTexture;

		public const int basewidth = 338;

		public const int baseWidth_calendar = 301;

		public const int baseheight = 198;

		private bool dailyQuestBoard;

		public ClickableComponent acceptQuestButton;

		public List<ClickableTextureComponent> calendarDays;

		private string hoverText = "";

		private string nightMarketLocalized;

		private string wizardBirthdayLocalized;

		protected Dictionary<ClickableTextureComponent, List<string>> _upcomingWeddings;

		public Billboard(bool dailyQuest = false)
			: base(0, 0, 0, 0, showUpperRightCloseButton: true)
		{
			_upcomingWeddings = new Dictionary<ClickableTextureComponent, List<string>>();
			if (!Game1.player.hasOrWillReceiveMail("checkedBulletinOnce"))
			{
				Game1.player.mailReceived.Add("checkedBulletinOnce");
				(Game1.getLocationFromName("Town") as Town).checkedBoard();
			}
			dailyQuestBoard = dailyQuest;
			billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Billboard");
			width = (dailyQuest ? 338 : 301) * 4;
			height = 792;
			Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
			xPositionOnScreen = (int)center.X;
			yPositionOnScreen = (int)center.Y;
			acceptQuestButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 128, yPositionOnScreen + height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 0
			};
			UpdateDailyQuestButton();
			upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 20, yPositionOnScreen, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
			Game1.playSound("bigSelect");
			if (!dailyQuest)
			{
				calendarDays = new List<ClickableTextureComponent>();
				Dictionary<int, NPC> birthdays = new Dictionary<int, NPC>();
				foreach (NPC k in Utility.getAllCharacters())
				{
					if (k.isVillager() && k.Birthday_Season != null && k.Birthday_Season.Equals(Game1.currentSeason) && !birthdays.ContainsKey(k.Birthday_Day) && (Game1.player.friendshipData.ContainsKey(k.Name) || (!k.Name.Equals("Dwarf") && !k.Name.Equals("Sandy") && !k.Name.Equals("Krobus"))))
					{
						birthdays.Add(k.Birthday_Day, k);
					}
				}
				nightMarketLocalized = Game1.content.LoadString("Strings\\UI:Billboard_NightMarket");
				wizardBirthdayLocalized = Game1.content.LoadString("Strings\\UI:Billboard_Birthday", Game1.getCharacterFromName("Wizard").displayName);
				for (int j = 1; j <= 28; j++)
				{
					string festival = "";
					string birthday = "";
					NPC i = birthdays.ContainsKey(j) ? birthdays[j] : null;
					if (Utility.isFestivalDay(j, Game1.currentSeason))
					{
						festival = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + j)["name"];
					}
					else
					{
						if (i != null)
						{
							birthday = ((i.displayName.Last() != 's' && (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.de || (i.displayName.Last() != 'x' && i.displayName.Last() != 'ß' && i.displayName.Last() != 'z'))) ? Game1.content.LoadString("Strings\\UI:Billboard_Birthday", i.displayName) : Game1.content.LoadString("Strings\\UI:Billboard_SBirthday", i.displayName));
						}
						if (Game1.currentSeason.Equals("winter") && j >= 15 && j <= 17)
						{
							festival = nightMarketLocalized;
						}
					}
					Texture2D character_texture = null;
					if (i != null)
					{
						try
						{
							character_texture = Game1.content.Load<Texture2D>("Characters\\" + i.getTextureName());
						}
						catch (Exception)
						{
							character_texture = i.Sprite.Texture;
						}
					}
					ClickableTextureComponent calendar_day = new ClickableTextureComponent(festival, new Rectangle(xPositionOnScreen + 152 + (j - 1) % 7 * 32 * 4, yPositionOnScreen + 200 + (j - 1) / 7 * 32 * 4, 124, 124), festival, birthday, character_texture, (i != null) ? new Rectangle(0, 0, 16, 24) : Rectangle.Empty, 1f)
					{
						myID = j,
						rightNeighborID = ((j % 7 != 0) ? (j + 1) : (-1)),
						leftNeighborID = ((j % 7 != 1) ? (j - 1) : (-1)),
						downNeighborID = j + 7,
						upNeighborID = ((j > 7) ? (j - 7) : (-1))
					};
					HashSet<Farmer> traversed_farmers = new HashSet<Farmer>();
					foreach (Farmer farmer in Game1.getOnlineFarmers())
					{
						if (!traversed_farmers.Contains(farmer) && farmer.isEngaged() && !farmer.hasCurrentOrPendingRoommate())
						{
							string spouse_name = null;
							WorldDate wedding_date = null;
							if (Game1.getCharacterFromName(farmer.spouse) != null)
							{
								wedding_date = farmer.friendshipData[farmer.spouse].WeddingDate;
								spouse_name = Game1.getCharacterFromName(farmer.spouse).displayName;
							}
							else
							{
								long? spouse = farmer.team.GetSpouse(farmer.uniqueMultiplayerID);
								if (spouse.HasValue)
								{
									Farmer spouse_farmer = Game1.getFarmerMaybeOffline(spouse.Value);
									if (spouse_farmer != null && Game1.getOnlineFarmers().Contains(spouse_farmer))
									{
										wedding_date = farmer.team.GetFriendship(farmer.uniqueMultiplayerID, spouse.Value).WeddingDate;
										traversed_farmers.Add(spouse_farmer);
										spouse_name = spouse_farmer.Name;
									}
								}
							}
							if (!(wedding_date == null))
							{
								if (wedding_date.TotalDays < Game1.Date.TotalDays)
								{
									wedding_date = new WorldDate(Game1.Date);
									wedding_date.TotalDays++;
								}
								if (wedding_date != null && wedding_date.TotalDays >= Game1.Date.TotalDays && Utility.getSeasonNumber(Game1.currentSeason) == wedding_date.SeasonIndex && j == wedding_date.DayOfMonth)
								{
									if (!_upcomingWeddings.ContainsKey(calendar_day))
									{
										_upcomingWeddings[calendar_day] = new List<string>();
									}
									traversed_farmers.Add(farmer);
									_upcomingWeddings[calendar_day].Add(farmer.Name);
									_upcomingWeddings[calendar_day].Add(spouse_name);
								}
							}
						}
					}
					calendarDays.Add(calendar_day);
				}
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID((!dailyQuestBoard) ? 1 : 0);
			snapCursorToCurrentSnappedComponent();
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			Game1.activeClickableMenu = new Billboard(dailyQuestBoard);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			Game1.playSound("bigDeSelect");
			exitThisMenu();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (acceptQuestButton.visible && acceptQuestButton.containsPoint(x, y))
			{
				Game1.playSound("newArtifact");
				Game1.questOfTheDay.dailyQuest.Value = true;
				Game1.questOfTheDay.dayQuestAccepted.Value = Game1.Date.TotalDays;
				Game1.questOfTheDay.accepted.Value = true;
				Game1.questOfTheDay.canBeCancelled.Value = true;
				Game1.questOfTheDay.daysLeft.Value = 2;
				Game1.player.questLog.Add(Game1.questOfTheDay);
				Game1.player.acceptedDailyQuest.Set(newValue: true);
				UpdateDailyQuestButton();
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			hoverText = "";
			if (dailyQuestBoard && Game1.questOfTheDay != null && !Game1.questOfTheDay.accepted)
			{
				float oldScale = acceptQuestButton.scale;
				acceptQuestButton.scale = (acceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
				if (acceptQuestButton.scale > oldScale)
				{
					Game1.playSound("Cowboy_gunshot");
				}
			}
			if (calendarDays != null)
			{
				foreach (ClickableTextureComponent c in calendarDays)
				{
					if (c.bounds.Contains(x, y))
					{
						if (c.hoverText.Length > 0)
						{
							hoverText = c.hoverText;
						}
						else
						{
							hoverText = c.label;
						}
						if (c.hoverText.Equals(wizardBirthdayLocalized))
						{
							c.hoverText = c.hoverText + Environment.NewLine + nightMarketLocalized;
						}
						if (_upcomingWeddings.ContainsKey(c))
						{
							for (int i = 0; i < _upcomingWeddings[c].Count / 2; i++)
							{
								hoverText = hoverText + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Calendar_Wedding", _upcomingWeddings[c][i * 2], _upcomingWeddings[c][i * 2 + 1]);
							}
						}
						hoverText = hoverText.Trim();
					}
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			bool hide_mouse = false;
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			b.Draw(billboardTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), dailyQuestBoard ? new Rectangle(0, 0, 338, 198) : new Rectangle(0, 198, 301, 198), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			if (!dailyQuestBoard)
			{
				b.DrawString(Game1.dialogueFont, Utility.getSeasonNameFromNumber(Utility.getSeasonNumber(Game1.currentSeason)), new Vector2(xPositionOnScreen + 160, yPositionOnScreen + 80), Game1.textColor);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:Billboard_Year", Game1.year), new Vector2(xPositionOnScreen + 448, yPositionOnScreen + 80), Game1.textColor);
				for (int i = 0; i < calendarDays.Count; i++)
				{
					if (calendarDays[i].name.Length > 0)
					{
						if (calendarDays[i].name.Equals(nightMarketLocalized))
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(calendarDays[i].bounds.X + 12, (float)(calendarDays[i].bounds.Y + 60) - Game1.dialogueButtonScale / 2f), new Rectangle(346, 392, 8, 8), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
						}
						else
						{
							Utility.drawWithShadow(b, billboardTexture, new Vector2(calendarDays[i].bounds.X + 40, (float)(calendarDays[i].bounds.Y + 56) - Game1.dialogueButtonScale / 2f), new Rectangle(1 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 14, 398, 14, 12), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
						}
					}
					if (calendarDays[i].hoverText.Length > 0)
					{
						b.Draw(calendarDays[i].texture, new Vector2(calendarDays[i].bounds.X + 48, calendarDays[i].bounds.Y + 28), calendarDays[i].sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
					if (_upcomingWeddings.ContainsKey(calendarDays[i]))
					{
						foreach (string item in _upcomingWeddings[calendarDays[i]])
						{
							_ = item;
							b.Draw(Game1.mouseCursors2, new Vector2(calendarDays[i].bounds.Right - 56, calendarDays[i].bounds.Top - 12), new Rectangle(112, 32, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						}
					}
					if (Game1.dayOfMonth > i + 1)
					{
						b.Draw(Game1.staminaRect, calendarDays[i].bounds, Color.Gray * 0.25f);
					}
					else if (Game1.dayOfMonth == i + 1)
					{
						int offset = (int)(4f * Game1.dialogueButtonScale / 8f);
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), calendarDays[i].bounds.X - offset, calendarDays[i].bounds.Y - offset, calendarDays[i].bounds.Width + offset * 2, calendarDays[i].bounds.Height + offset * 2, Color.Blue, 4f, drawShadow: false);
					}
				}
			}
			else
			{
				if (Game1.options.SnappyMenus)
				{
					hide_mouse = true;
				}
				if (Game1.questOfTheDay == null || Game1.questOfTheDay.currentObjective == null || Game1.questOfTheDay.currentObjective.Length == 0)
				{
					b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:Billboard_NothingPosted"), new Vector2(xPositionOnScreen + 384, yPositionOnScreen + 320), Game1.textColor);
				}
				else
				{
					SpriteFont font = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont;
					string description = Game1.parseText(Game1.questOfTheDay.questDescription, font, 640);
					Utility.drawTextWithShadow(b, description, font, new Vector2(xPositionOnScreen + 320 + 32, yPositionOnScreen + 256), Game1.textColor, 1f, -1f, -1, -1, 0.5f);
					if (acceptQuestButton.visible)
					{
						hide_mouse = false;
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), acceptQuestButton.bounds.X, acceptQuestButton.bounds.Y, acceptQuestButton.bounds.Width, acceptQuestButton.bounds.Height, (acceptQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * acceptQuestButton.scale);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2(acceptQuestButton.bounds.X + 12, acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
					}
				}
			}
			base.draw(b);
			if (!hide_mouse)
			{
				Game1.mouseCursorTransparency = 1f;
				drawMouse(b);
				if (hoverText.Length > 0)
				{
					IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
				}
			}
		}

		public void UpdateDailyQuestButton()
		{
			if (acceptQuestButton != null)
			{
				if (!dailyQuestBoard)
				{
					acceptQuestButton.visible = false;
				}
				else
				{
					acceptQuestButton.visible = Game1.CanAcceptDailyQuest();
				}
			}
		}
	}
}
