using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class QuestLog : IClickableMenu
	{
		public const int questsPerPage = 6;

		public const int region_forwardButton = 101;

		public const int region_backButton = 102;

		public const int region_rewardBox = 103;

		public const int region_cancelQuestButton = 104;

		private List<List<Quest>> pages;

		public List<ClickableComponent> questLogButtons;

		private int currentPage;

		private int questPage = -1;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent rewardBox;

		public ClickableTextureComponent cancelQuestButton;

		private string hoverText = "";

		public QuestLog()
			: base(0, 0, 0, 0, showUpperRightCloseButton: true)
		{
			Game1.playSound("bigSelect");
			paginateQuests();
			width = 832;
			height = 576;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
			{
				height += 64;
			}
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
			xPositionOnScreen = (int)topLeft.X;
			yPositionOnScreen = (int)topLeft.Y + 32;
			questLogButtons = new List<ClickableComponent>();
			for (int i = 0; i < 6; i++)
			{
				questLogButtons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16 + i * ((height - 32) / 6), width - 32, (height - 32) / 6 + 4), string.Concat(i))
				{
					myID = i,
					downNeighborID = -7777,
					upNeighborID = ((i > 0) ? (i - 1) : (-1)),
					rightNeighborID = -7777,
					leftNeighborID = -7777,
					fullyImmutable = true
				});
			}
			upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 20, yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen - 64, yPositionOnScreen + 8, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 102,
				rightNeighborID = -7777
			};
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 64 - 48, yPositionOnScreen + height - 48, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 101
			};
			rewardBox = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 - 80, yPositionOnScreen + height - 32 - 96, 96, 96), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), 4f, drawShadow: true)
			{
				myID = 103
			};
			cancelQuestButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 4, yPositionOnScreen + height + 4, 48, 48), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 4f, drawShadow: true)
			{
				myID = 104
			};
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID >= 0 && oldID < 6 && questPage == -1)
			{
				switch (direction)
				{
				case 2:
					if (oldID < 5 && pages[currentPage].Count - 1 > oldID)
					{
						currentlySnappedComponent = getComponentWithID(oldID + 1);
					}
					break;
				case 1:
					if (currentPage < pages.Count - 1)
					{
						currentlySnappedComponent = getComponentWithID(101);
						currentlySnappedComponent.leftNeighborID = oldID;
					}
					break;
				case 3:
					if (currentPage > 0)
					{
						currentlySnappedComponent = getComponentWithID(102);
						currentlySnappedComponent.rightNeighborID = oldID;
					}
					break;
				}
			}
			else if (oldID == 102)
			{
				if (questPage != -1)
				{
					return;
				}
				currentlySnappedComponent = getComponentWithID(0);
			}
			snapCursorToCurrentSnappedComponent();
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.RightTrigger && questPage == -1 && currentPage < pages.Count - 1)
			{
				nonQuestPageForwardButton();
			}
			else if (b == Buttons.LeftTrigger && questPage == -1 && currentPage > 0)
			{
				nonQuestPageBackButton();
			}
		}

		private void paginateQuests()
		{
			pages = new List<List<Quest>>();
			for (int i = Game1.player.questLog.Count - 1; i >= 0; i--)
			{
				if (Game1.player.questLog[i] == null || (bool)Game1.player.questLog[i].destroy)
				{
					Game1.player.questLog.RemoveAt(i);
				}
				else if (Game1.player.questLog[i] == null || !Game1.player.questLog[i].isSecretQuest())
				{
					int which = Game1.player.visibleQuestCount - 1 - i;
					if (pages.Count <= which / 6)
					{
						pages.Add(new List<Quest>());
					}
					pages[which / 6].Add(Game1.player.questLog[i]);
				}
			}
			if (pages.Count == 0)
			{
				pages.Add(new List<Quest>());
			}
			currentPage = Math.Min(Math.Max(currentPage, 0), pages.Count - 1);
			questPage = -1;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			hoverText = "";
			base.performHoverAction(x, y);
			if (questPage == -1)
			{
				for (int i = 0; i < questLogButtons.Count; i++)
				{
					if (pages.Count > 0 && pages[0].Count > i && questLogButtons[i].containsPoint(x, y) && !questLogButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
					{
						Game1.playSound("Cowboy_gunshot");
					}
				}
			}
			else if ((bool)pages[currentPage][questPage].canBeCancelled && cancelQuestButton.containsPoint(x, y))
			{
				hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11364");
			}
			forwardButton.tryHover(x, y, 0.2f);
			backButton.tryHover(x, y, 0.2f);
			cancelQuestButton.tryHover(x, y, 0.2f);
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.doesInputListContain(Game1.options.journalButton, key) && readyToClose())
			{
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect");
			}
		}

		private void nonQuestPageForwardButton()
		{
			currentPage++;
			Game1.playSound("shwip");
			if (Game1.options.SnappyMenus && currentPage == pages.Count - 1)
			{
				currentlySnappedComponent = getComponentWithID(0);
				snapCursorToCurrentSnappedComponent();
			}
		}

		private void nonQuestPageBackButton()
		{
			currentPage--;
			Game1.playSound("shwip");
			if (Game1.options.SnappyMenus && currentPage == 0)
			{
				currentlySnappedComponent = getComponentWithID(0);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (Game1.activeClickableMenu == null)
			{
				return;
			}
			if (questPage == -1)
			{
				for (int i = 0; i < questLogButtons.Count; i++)
				{
					if (pages.Count > 0 && pages[currentPage].Count > i && questLogButtons[i].containsPoint(x, y))
					{
						Game1.playSound("smallSelect");
						questPage = i;
						pages[currentPage][i].showNew.Value = false;
						if (Game1.options.SnappyMenus)
						{
							currentlySnappedComponent = getComponentWithID(102);
							currentlySnappedComponent.rightNeighborID = -7777;
							currentlySnappedComponent.downNeighborID = (((bool)pages[currentPage][questPage].completed && (int)pages[currentPage][questPage].moneyReward > 0) ? 103 : (pages[currentPage][questPage].canBeCancelled ? 104 : (-1)));
							snapCursorToCurrentSnappedComponent();
						}
						return;
					}
				}
				if (currentPage < pages.Count - 1 && forwardButton.containsPoint(x, y))
				{
					nonQuestPageForwardButton();
					return;
				}
				if (currentPage > 0 && backButton.containsPoint(x, y))
				{
					nonQuestPageBackButton();
					return;
				}
				Game1.playSound("bigDeSelect");
				exitThisMenu();
			}
			else if (questPage != -1 && (bool)pages[currentPage][questPage].completed && (int)pages[currentPage][questPage].moneyReward > 0 && rewardBox.containsPoint(x, y))
			{
				Game1.player.Money += pages[currentPage][questPage].moneyReward;
				Game1.playSound("purchaseRepeat");
				pages[currentPage][questPage].moneyReward.Value = 0;
				pages[currentPage][questPage].destroy.Value = true;
			}
			else if (questPage != -1 && !pages[currentPage][questPage].completed && (bool)pages[currentPage][questPage].canBeCancelled && cancelQuestButton.containsPoint(x, y))
			{
				pages[currentPage][questPage].accepted.Value = false;
				if (pages[currentPage][questPage].dailyQuest.Value && pages[currentPage][questPage].dayQuestAccepted.Value == Game1.Date.TotalDays)
				{
					Game1.player.acceptedDailyQuest.Set(newValue: false);
				}
				Game1.player.questLog.Remove(pages[currentPage][questPage]);
				pages[currentPage].RemoveAt(questPage);
				questPage = -1;
				Game1.playSound("trashcan");
				if (Game1.options.SnappyMenus && currentPage == 0)
				{
					currentlySnappedComponent = getComponentWithID(0);
					snapCursorToCurrentSnappedComponent();
				}
			}
			else
			{
				exitQuestPage();
			}
		}

		public void exitQuestPage()
		{
			if ((bool)pages[currentPage][questPage].completed && (int)pages[currentPage][questPage].moneyReward <= 0)
			{
				pages[currentPage][questPage].destroy.Value = true;
			}
			if ((bool)pages[currentPage][questPage].destroy)
			{
				Game1.player.questLog.Remove(pages[currentPage][questPage]);
				pages[currentPage].RemoveAt(questPage);
			}
			questPage = -1;
			paginateQuests();
			Game1.playSound("shwip");
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (questPage != -1 && pages[currentPage][questPage].hasReward())
			{
				rewardBox.scale = rewardBox.baseScale + Game1.dialogueButtonScale / 20f;
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11373"), xPositionOnScreen + width / 2, yPositionOnScreen - 64);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f);
			if (questPage == -1)
			{
				for (int i = 0; i < questLogButtons.Count; i++)
				{
					if (pages.Count() > 0 && pages[currentPage].Count() > i)
					{
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), questLogButtons[i].bounds.X, questLogButtons[i].bounds.Y, questLogButtons[i].bounds.Width, questLogButtons[i].bounds.Height, questLogButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
						if ((bool)pages[currentPage][i].showNew || (bool)pages[currentPage][i].completed)
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(questLogButtons[i].bounds.X + 64 + 4, questLogButtons[i].bounds.Y + 44), new Rectangle(pages[currentPage][i].completed ? 341 : 317, 410, 23, 9), Color.White, 0f, new Vector2(11f, 4f), 4f + Game1.dialogueButtonScale * 10f / 250f, flipped: false, 0.99f);
						}
						else
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(questLogButtons[i].bounds.X + 32, questLogButtons[i].bounds.Y + 28), pages[currentPage][i].dailyQuest ? new Rectangle(410, 501, 9, 9) : new Rectangle(395 + (pages[currentPage][i].dailyQuest ? 3 : 0), 497, 3, 8), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f);
						}
						_ = (bool)pages[currentPage][i].dailyQuest;
						SpriteText.drawString(b, pages[currentPage][i].questTitle, questLogButtons[i].bounds.X + 128 + 4, questLogButtons[i].bounds.Y + 20);
					}
				}
			}
			else
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, pages[currentPage][questPage].questTitle, xPositionOnScreen + width / 2 + (((bool)pages[currentPage][questPage].dailyQuest && (int)pages[currentPage][questPage].daysLeft > 0) ? (Math.Max(32, SpriteText.getWidthOfString(pages[currentPage][questPage].questTitle) / 3) - 32) : 0), yPositionOnScreen + 32);
				if ((bool)pages[currentPage][questPage].dailyQuest && (int)pages[currentPage][questPage].daysLeft > 0)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(xPositionOnScreen + 32, yPositionOnScreen + 48 - 8), new Rectangle(410, 501, 9, 9), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f);
					Utility.drawTextWithShadow(b, Game1.parseText(((int)pages[currentPage][questPage].daysLeft > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11374", pages[currentPage][questPage].daysLeft) : Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11375", pages[currentPage][questPage].daysLeft), Game1.dialogueFont, width - 128), Game1.dialogueFont, new Vector2(xPositionOnScreen + 80, yPositionOnScreen + 48 - 8), Game1.textColor);
				}
				Utility.drawTextWithShadow(b, Game1.parseText(pages[currentPage][questPage].questDescription, Game1.dialogueFont, width - 128), Game1.dialogueFont, new Vector2(xPositionOnScreen + 64, yPositionOnScreen + 96), Game1.textColor);
				float yPos = (float)(yPositionOnScreen + 96) + Game1.dialogueFont.MeasureString(Game1.parseText(pages[currentPage][questPage].questDescription, Game1.dialogueFont, width - 128)).Y + 32f;
				if ((bool)pages[currentPage][questPage].completed)
				{
					SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11376"), xPositionOnScreen + 32 + 4, rewardBox.bounds.Y + 21 + 4);
					rewardBox.draw(b);
					if ((int)pages[currentPage][questPage].moneyReward > 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2(rewardBox.bounds.X + 16, (float)(rewardBox.bounds.Y + 16) - Game1.dialogueButtonScale / 2f), new Rectangle(280, 410, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", pages[currentPage][questPage].moneyReward), xPositionOnScreen + 448, rewardBox.bounds.Y + 21 + 4);
					}
				}
				else
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(xPositionOnScreen + 96) + 8f * Game1.dialogueButtonScale / 10f, yPos), new Rectangle(412, 495, 5, 4), Color.White, (float)Math.PI / 2f, Vector2.Zero);
					Utility.drawTextWithShadow(b, Game1.parseText(pages[currentPage][questPage].currentObjective, Game1.dialogueFont, width - 256), Game1.dialogueFont, new Vector2(xPositionOnScreen + 128, yPos - 8f), Color.DarkBlue);
					if ((bool)pages[currentPage][questPage].canBeCancelled)
					{
						cancelQuestButton.draw(b);
					}
				}
			}
			if (currentPage < pages.Count - 1 && questPage == -1)
			{
				forwardButton.draw(b);
			}
			if (currentPage > 0 || questPage != -1)
			{
				backButton.draw(b);
			}
			base.draw(b);
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
			if (hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
			}
		}
	}
}
