using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class LetterViewerMenu : IClickableMenu
	{
		public const int region_backButton = 101;

		public const int region_forwardButton = 102;

		public const int region_acceptQuestButton = 103;

		public const int region_itemGrabButton = 104;

		public const int letterWidth = 320;

		public const int letterHeight = 180;

		public Texture2D letterTexture;

		public Texture2D secretNoteImageTexture;

		private int moneyIncluded;

		private int questID = -1;

		private int secretNoteImage = -1;

		private int whichBG;

		private string learnedRecipe = "";

		private string cookingOrCrafting = "";

		private string mailTitle;

		private List<string> mailMessage = new List<string>();

		private int page;

		public List<ClickableComponent> itemsToGrab = new List<ClickableComponent>();

		private float scale;

		private bool isMail;

		private bool isFromCollection;

		public new bool destroy;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		public ClickableComponent acceptQuestButton;

		public const float scaleChange = 0.003f;

		public LetterViewerMenu(string text)
			: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
		{
			Game1.playSound("shwip");
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 101,
				rightNeighborID = 102
			};
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				leftNeighborID = 101
			};
			letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
			mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(text, width - 64, height - 128);
		}

		public LetterViewerMenu(int secretNoteIndex)
			: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
		{
			Game1.playSound("shwip");
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 101,
				rightNeighborID = 102
			};
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				leftNeighborID = 101
			};
			letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
			string data = Game1.content.Load<Dictionary<int, string>>("Data\\SecretNotes")[secretNoteIndex];
			if (data[0] == '!')
			{
				secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\SecretNotesImages");
				secretNoteImage = Convert.ToInt32(data.Split(' ')[1]);
			}
			else
			{
				mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(Utility.ParseGiftReveals(data.Replace("@", Game1.player.name)), width - 64, height - 128);
				whichBG = 1;
			}
		}

		public LetterViewerMenu(string mail, string mailTitle, bool fromCollection = false)
			: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
		{
			isFromCollection = fromCollection;
			mail = mail.Split(new string[1]
			{
				"[#]"
			}, StringSplitOptions.None)[0];
			mail = mail.Replace("@", Game1.player.Name);
			if (mail.Contains("%update"))
			{
				mail = mail.Replace("%update", Utility.getStardewHeroStandingsString());
			}
			isMail = true;
			Game1.playSound("shwip");
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 101,
				rightNeighborID = 102
			};
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				leftNeighborID = 101
			};
			acceptQuestButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 128, yPositionOnScreen + height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 103,
				rightNeighborID = 102,
				leftNeighborID = 101
			};
			this.mailTitle = mailTitle;
			letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
			if (mail.Contains("¦"))
			{
				mail = (Game1.player.IsMale ? mail.Substring(0, mail.IndexOf("¦")) : mail.Substring(mail.IndexOf("¦") + 1));
			}
			if (mail.Contains("%item"))
			{
				string itemDescription = mail.Substring(mail.IndexOf("%item"), mail.IndexOf("%%") + 2 - mail.IndexOf("%item"));
				string[] split = itemDescription.Split(' ');
				mail = mail.Replace(itemDescription, "");
				if (!isFromCollection)
				{
					if (split[1].Equals("object"))
					{
						int maxNum2 = split.Length - 1;
						int which3 = Game1.random.Next(2, maxNum2);
						which3 -= which3 % 2;
						Object o2 = new Object(Vector2.Zero, Convert.ToInt32(split[which3]), Convert.ToInt32(split[which3 + 1]));
						itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), o2)
						{
							myID = 104,
							leftNeighborID = 101,
							rightNeighborID = 102
						});
						backButton.rightNeighborID = 104;
						forwardButton.leftNeighborID = 104;
					}
					else if (split[1].Equals("tools"))
					{
						for (int j = 2; j < split.Length; j++)
						{
							Item tool = null;
							switch (split[j])
							{
							case "Axe":
								tool = new Axe();
								break;
							case "Hoe":
								tool = new Hoe();
								break;
							case "Can":
								tool = new WateringCan();
								break;
							case "Scythe":
								tool = new MeleeWeapon(47);
								break;
							case "Pickaxe":
								tool = new Pickaxe();
								break;
							}
							if (tool != null)
							{
								itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), tool));
							}
						}
					}
					else if (split[1].Equals("bigobject"))
					{
						int maxNum = split.Length - 1;
						int which = Game1.random.Next(2, maxNum);
						Object o = new Object(Vector2.Zero, Convert.ToInt32(split[which]));
						itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), o)
						{
							myID = 104,
							leftNeighborID = 101,
							rightNeighborID = 102
						});
						backButton.rightNeighborID = 104;
						forwardButton.leftNeighborID = 104;
					}
					else if (split[1].Equals("money"))
					{
						int moneyToAdd2 = (split.Length > 4) ? Game1.random.Next(Convert.ToInt32(split[2]), Convert.ToInt32(split[3])) : Convert.ToInt32(split[2]);
						moneyToAdd2 -= moneyToAdd2 % 10;
						Game1.player.Money += moneyToAdd2;
						moneyIncluded = moneyToAdd2;
					}
					else if (split[1].Equals("conversationTopic"))
					{
						string topic = split[2];
						int numDays = Convert.ToInt32(split[3].Replace("%%", ""));
						Game1.player.activeDialogueEvents.Add(topic, numDays);
						if (topic.Equals("ElliottGone3"))
						{
							Utility.getHomeOfFarmer(Game1.player).fridge.Value.addItem(new Object(732, 1));
						}
					}
					else if (split[1].Equals("cookingRecipe"))
					{
						Dictionary<string, string> cookingRecipes = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
						int lowest_required_heart_level = 1000;
						string recipe_string = "";
						foreach (string s in cookingRecipes.Keys)
						{
							string[] cookingSplit = cookingRecipes[s].Split('/');
							string[] getConditions = cookingSplit[3].Split(' ');
							if (getConditions[0].Equals("f") && getConditions[1].Equals(mailTitle.Replace("Cooking", "")) && !Game1.player.cookingRecipes.ContainsKey(s))
							{
								int required_heart_level = Convert.ToInt32(getConditions[2]);
								if (required_heart_level <= lowest_required_heart_level)
								{
									lowest_required_heart_level = required_heart_level;
									recipe_string = s;
									learnedRecipe = s;
									if (LocalizedContentManager.CurrentLanguageCode != 0)
									{
										learnedRecipe = cookingSplit[cookingSplit.Length - 1];
									}
								}
							}
						}
						if (recipe_string != "")
						{
							if (!Game1.player.cookingRecipes.ContainsKey(recipe_string))
							{
								Game1.player.cookingRecipes.Add(recipe_string, 0);
							}
							cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_cooking");
						}
					}
					else if (split[1].Equals("craftingRecipe"))
					{
						learnedRecipe = split[2].Replace('_', ' ');
						if (!Game1.player.craftingRecipes.ContainsKey(learnedRecipe))
						{
							Game1.player.craftingRecipes.Add(learnedRecipe, 0);
						}
						cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_crafting");
						if (LocalizedContentManager.CurrentLanguageCode != 0)
						{
							Dictionary<string, string> craftingRecipes = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
							if (craftingRecipes.ContainsKey(learnedRecipe))
							{
								string[] craftingSplit = craftingRecipes[learnedRecipe].Split('/');
								learnedRecipe = craftingSplit[craftingSplit.Length - 1];
							}
						}
					}
					else if (split[1].Equals("itemRecovery"))
					{
						if (Game1.player.recoveredItem != null)
						{
							Item item = Game1.player.recoveredItem;
							Game1.player.recoveredItem = null;
							itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), item)
							{
								myID = 104,
								leftNeighborID = 101,
								rightNeighborID = 102
							});
							backButton.rightNeighborID = 104;
							forwardButton.leftNeighborID = 104;
						}
					}
					else if (split[1].Equals("quest"))
					{
						questID = Convert.ToInt32(split[2].Replace("%%", ""));
						if (split.Length > 4)
						{
							if (!Game1.player.mailReceived.Contains("NOQUEST_" + questID))
							{
								Game1.player.addQuest(questID);
							}
							questID = -1;
						}
						backButton.rightNeighborID = 103;
						forwardButton.leftNeighborID = 103;
					}
				}
			}
			if (mailTitle == "ccBulletinThankYou" && !Game1.player.hasOrWillReceiveMail("ccBulletinThankYouReceived"))
			{
				foreach (NPC i in Utility.getAllCharacters())
				{
					if (!i.datable && i.isVillager())
					{
						Game1.player.changeFriendship(500, i);
					}
				}
				Game1.addMailForTomorrow("ccBulletinThankYouReceived", noLetter: true);
			}
			Random r = new Random((int)(Game1.uniqueIDForThisGame / 2uL) ^ Game1.year ^ (int)Game1.player.UniqueMultiplayerID);
			bool hide_secret_santa = fromCollection;
			if (Game1.currentSeason == "winter" && Game1.dayOfMonth >= 18 && Game1.dayOfMonth <= 25)
			{
				hide_secret_santa = false;
			}
			mail = mail.Replace("%secretsanta", hide_secret_santa ? "???" : Utility.getRandomTownNPC(r).displayName);
			mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(mail, width - 64, height - 128);
			if (mailTitle.Equals("winter_5_2") || mailTitle.Equals("winter_12_1") || mailTitle.ToLower().Contains("wizard"))
			{
				whichBG = 2;
			}
			else if (mailTitle.Equals("Sandy"))
			{
				whichBG = 1;
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
				if (mailMessage != null && mailMessage.Count <= 1)
				{
					backButton.myID = -100;
					forwardButton.myID = -100;
				}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (questID != -1)
			{
				currentlySnappedComponent = getComponentWithID(103);
			}
			else if (itemsToGrab != null && itemsToGrab.Count > 0)
			{
				currentlySnappedComponent = getComponentWithID(104);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID(102);
			}
			snapCursorToCurrentSnappedComponent();
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X;
			yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y;
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 101,
				rightNeighborID = 102
			};
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				leftNeighborID = 101
			};
			acceptQuestButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 128, yPositionOnScreen + height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 103,
				rightNeighborID = 102,
				leftNeighborID = 101
			};
			foreach (ClickableComponent item in itemsToGrab)
			{
				item.bounds = new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96);
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (key != 0)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
				{
					exitThisMenu(ShouldPlayExitSound());
				}
				else
				{
					base.receiveKeyPress(key);
				}
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (b == Buttons.LeftTrigger && page > 0)
			{
				page--;
				Game1.playSound("shwip");
			}
			else if (b == Buttons.RightTrigger && page < mailMessage.Count - 1)
			{
				page++;
				Game1.playSound("shwip");
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (scale < 1f)
			{
				return;
			}
			if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
			{
				if (playSound)
				{
					Game1.playSound("bigDeSelect");
				}
				if (!isFromCollection)
				{
					exitThisMenu(ShouldPlayExitSound());
				}
				else
				{
					destroy = true;
				}
			}
			if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
			{
				unload();
				return;
			}
			foreach (ClickableComponent c in itemsToGrab)
			{
				if (c.containsPoint(x, y) && c.item != null)
				{
					Game1.playSound("coin");
					Game1.player.addItemByMenuIfNecessary(c.item);
					c.item = null;
					return;
				}
			}
			if (backButton.containsPoint(x, y) && page > 0)
			{
				page--;
				Game1.playSound("shwip");
			}
			else if (forwardButton.containsPoint(x, y) && page < mailMessage.Count - 1)
			{
				page++;
				Game1.playSound("shwip");
			}
			else if (acceptQuestButton != null && acceptQuestButton.containsPoint(x, y))
			{
				AcceptQuest();
			}
			else if (isWithinBounds(x, y))
			{
				if (page < mailMessage.Count - 1)
				{
					page++;
					Game1.playSound("shwip");
				}
				else if (!isMail)
				{
					exitThisMenuNoSound();
					Game1.playSound("shwip");
				}
				else if (isFromCollection)
				{
					destroy = true;
				}
			}
			else if (!itemsLeftToGrab())
			{
				if (!isFromCollection)
				{
					exitThisMenuNoSound();
					Game1.playSound("shwip");
				}
				else
				{
					destroy = true;
				}
			}
		}

		public virtual bool ShouldPlayExitSound()
		{
			if (questID != -1)
			{
				return false;
			}
			return true;
		}

		public bool itemsLeftToGrab()
		{
			if (itemsToGrab == null)
			{
				return false;
			}
			foreach (ClickableComponent item in itemsToGrab)
			{
				if (item.item != null)
				{
					return true;
				}
			}
			return false;
		}

		public void AcceptQuest()
		{
			if (questID != -1)
			{
				Game1.player.addQuest(questID);
				if (questID == 20)
				{
					MineShaft.CheckForQiChallengeCompletion();
				}
				questID = -1;
				Game1.playSound("newArtifact");
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableComponent c in itemsToGrab)
			{
				if (c.containsPoint(x, y))
				{
					c.scale = Math.Min(c.scale + 0.03f, 1.1f);
				}
				else
				{
					c.scale = Math.Max(1f, c.scale - 0.03f);
				}
			}
			backButton.tryHover(x, y, 0.6f);
			forwardButton.tryHover(x, y, 0.6f);
			if (questID != -1)
			{
				float oldScale = acceptQuestButton.scale;
				acceptQuestButton.scale = (acceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
				if (acceptQuestButton.scale > oldScale)
				{
					Game1.playSound("Cowboy_gunshot");
				}
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (scale < 1f)
			{
				scale += (float)time.ElapsedGameTime.Milliseconds * 0.003f;
				if (scale >= 1f)
				{
					scale = 1f;
				}
			}
			if (page < mailMessage.Count - 1 && !forwardButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				forwardButton.scale = 4f + (float)Math.Sin((double)(float)time.TotalGameTime.Milliseconds / (Math.PI * 64.0)) / 1.5f;
			}
		}

		private int getTextColor()
		{
			switch (whichBG)
			{
			case 1:
				return 8;
			case 2:
				return 7;
			default:
				return -1;
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			b.Draw(letterTexture, new Vector2(xPositionOnScreen + width / 2, yPositionOnScreen + height / 2), new Rectangle(whichBG * 320, 0, 320, 180), Color.White, 0f, new Vector2(160f, 90f), 4f * scale, SpriteEffects.None, 0.86f);
			if (scale == 1f)
			{
				if (secretNoteImage != -1)
				{
					b.Draw(secretNoteImageTexture, new Vector2(xPositionOnScreen + width / 2 - 128 - 4, yPositionOnScreen + height / 2 - 128 + 8), new Rectangle(secretNoteImage * 64 % secretNoteImageTexture.Width, secretNoteImage * 64 / secretNoteImageTexture.Width * 64, 64, 64), Color.Black * 0.4f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(secretNoteImageTexture, new Vector2(xPositionOnScreen + width / 2 - 128, yPositionOnScreen + height / 2 - 128), new Rectangle(secretNoteImage * 64 % secretNoteImageTexture.Width, secretNoteImage * 64 / secretNoteImageTexture.Width * 64, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(secretNoteImageTexture, new Vector2(xPositionOnScreen + width / 2 - 40, yPositionOnScreen + height / 2 - 192), new Rectangle(193, 65, 14, 21), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.867f);
				}
				else
				{
					SpriteText.drawString(b, mailMessage[page], xPositionOnScreen + 32, yPositionOnScreen + 32, 999999, width - 64, 999999, 0.75f, 0.865f, junimoText: false, -1, "", getTextColor());
				}
				foreach (ClickableComponent c in itemsToGrab)
				{
					b.Draw(letterTexture, c.bounds, new Rectangle(whichBG * 24, 180, 24, 24), Color.White);
					if (c.item != null)
					{
						c.item.drawInMenu(b, new Vector2(c.bounds.X + 16, c.bounds.Y + 16), c.scale);
					}
				}
				if (moneyIncluded > 0)
				{
					string moneyText = Game1.content.LoadString("Strings\\UI:LetterViewer_MoneyIncluded", moneyIncluded);
					SpriteText.drawString(b, moneyText, xPositionOnScreen + width / 2 - SpriteText.getWidthOfString(moneyText) / 2, yPositionOnScreen + height - 96, 999999, -1, 9999, 0.75f, 0.865f);
				}
				else if (learnedRecipe != null && learnedRecipe.Length > 0)
				{
					string recipeText = Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipe", cookingOrCrafting);
					SpriteText.drawStringHorizontallyCenteredAt(b, recipeText, xPositionOnScreen + width / 2, yPositionOnScreen + height - 32 - SpriteText.getHeightOfString(recipeText) * 2, 999999, width - 64, 9999, 0.65f, 0.865f);
					SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipeName", learnedRecipe), xPositionOnScreen + width / 2, yPositionOnScreen + height - 32 - SpriteText.getHeightOfString("t"), 999999, width - 64, 9999, 0.9f, 0.865f);
				}
				base.draw(b);
				if (page < mailMessage.Count - 1)
				{
					forwardButton.draw(b);
				}
				if (page > 0)
				{
					backButton.draw(b);
				}
				if (questID != -1)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), acceptQuestButton.bounds.X, acceptQuestButton.bounds.Y, acceptQuestButton.bounds.Width, acceptQuestButton.bounds.Height, (acceptQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * acceptQuestButton.scale);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2(acceptQuestButton.bounds.X + 12, acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
				}
			}
			if (Game1.activeClickableMenu == this && !Game1.options.hardwareCursor)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
		}

		public void unload()
		{
		}

		protected override void cleanupBeforeExit()
		{
			if (questID != -1)
			{
				AcceptQuest();
			}
			if (itemsLeftToGrab())
			{
				List<Item> items = new List<Item>();
				foreach (ClickableComponent c in itemsToGrab)
				{
					if (c.item != null)
					{
						items.Add(c.item);
						c.item = null;
					}
				}
				if (items.Count > 0)
				{
					Game1.playSound("coin");
					Game1.player.addItemsByMenuIfNecessary(items);
				}
			}
			base.cleanupBeforeExit();
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (isFromCollection)
			{
				destroy = true;
			}
			else
			{
				receiveLeftClick(x, y, playSound);
			}
		}
	}
}
