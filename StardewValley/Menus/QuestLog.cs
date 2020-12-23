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

		private List<List<IQuest>> pages;

		public List<ClickableComponent> questLogButtons;

		private int currentPage;

		private int questPage = -1;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent rewardBox;

		public ClickableTextureComponent cancelQuestButton;

		protected IQuest _shownQuest;

		protected List<string> _objectiveText;

		protected float _contentHeight;

		protected float _scissorRectHeight;

		public float scrollAmount;

		public ClickableTextureComponent upArrow;

		public ClickableTextureComponent downArrow;

		public ClickableTextureComponent scrollBar;

		private bool scrolling;

		public Rectangle scrollBarBounds;

		private string hoverText = "";

		public QuestLog()
			: base(0, 0, 0, 0, showUpperRightCloseButton: true)
		{
			Game1.dayTimeMoneyBox.DismissQuestPing();
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
			int scrollbar_x = xPositionOnScreen + width + 16;
			upArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, yPositionOnScreen + 96, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
			downArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
			scrollBarBounds = default(Rectangle);
			scrollBarBounds.X = upArrow.bounds.X + 12;
			scrollBarBounds.Width = 24;
			scrollBarBounds.Y = upArrow.bounds.Y + upArrow.bounds.Height + 4;
			scrollBarBounds.Height = downArrow.bounds.Y - 4 - scrollBarBounds.Y;
			scrollBar = new ClickableTextureComponent(new Rectangle(scrollBarBounds.X, scrollBarBounds.Y, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
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
			pages = new List<List<IQuest>>();
			for (int j = Game1.player.team.specialOrders.Count - 1; j >= 0; j--)
			{
				int which = j;
				while (pages.Count <= which / 6)
				{
					pages.Add(new List<IQuest>());
				}
				if (!Game1.player.team.specialOrders[j].IsHidden())
				{
					pages[which / 6].Add(Game1.player.team.specialOrders[j]);
				}
			}
			for (int i = Game1.player.questLog.Count - 1; i >= 0; i--)
			{
				if (Game1.player.questLog[i] == null || (bool)Game1.player.questLog[i].destroy)
				{
					Game1.player.questLog.RemoveAt(i);
				}
				else if (Game1.player.questLog[i] == null || !Game1.player.questLog[i].IsHidden())
				{
					int which2 = Game1.player.visibleQuestCount - 1 - i;
					while (pages.Count <= which2 / 6)
					{
						pages.Add(new List<IQuest>());
					}
					pages[which2 / 6].Add(Game1.player.questLog[i]);
				}
			}
			if (pages.Count == 0)
			{
				pages.Add(new List<IQuest>());
			}
			currentPage = Math.Min(Math.Max(currentPage, 0), pages.Count - 1);
			questPage = -1;
		}

		public bool NeedsScroll()
		{
			if (_shownQuest != null && _shownQuest.ShouldDisplayAsComplete())
			{
				return false;
			}
			if (questPage != -1)
			{
				return _contentHeight > _scissorRectHeight;
			}
			return false;
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (NeedsScroll())
			{
				float new_scroll = scrollAmount - (float)(Math.Sign(direction) * 64 / 2);
				if (new_scroll < 0f)
				{
					new_scroll = 0f;
				}
				if (new_scroll > _contentHeight - _scissorRectHeight)
				{
					new_scroll = _contentHeight - _scissorRectHeight;
				}
				if (scrollAmount != new_scroll)
				{
					scrollAmount = new_scroll;
					Game1.playSound("shiny4");
					SetScrollBarFromAmount();
				}
			}
			base.receiveScrollWheelAction(direction);
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
			else if (_shownQuest.CanBeCancelled() && cancelQuestButton.containsPoint(x, y))
			{
				hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11364");
			}
			forwardButton.tryHover(x, y, 0.2f);
			backButton.tryHover(x, y, 0.2f);
			cancelQuestButton.tryHover(x, y, 0.2f);
			if (NeedsScroll())
			{
				upArrow.tryHover(x, y);
				downArrow.tryHover(x, y);
				scrollBar.tryHover(x, y);
				_ = scrolling;
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.isAnyGamePadButtonBeingPressed() && questPage != -1 && Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				exitQuestPage();
			}
			else
			{
				base.receiveKeyPress(key);
			}
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

		public override void leftClickHeld(int x, int y)
		{
			if (!GameMenu.forcePreventClose)
			{
				base.leftClickHeld(x, y);
				if (scrolling)
				{
					SetScrollFromY(y);
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!GameMenu.forcePreventClose)
			{
				base.releaseLeftClick(x, y);
				scrolling = false;
			}
		}

		public virtual void SetScrollFromY(int y)
		{
			int y2 = scrollBar.bounds.Y;
			float percentage2 = (float)(y - scrollBarBounds.Y) / (float)(scrollBarBounds.Height - scrollBar.bounds.Height);
			percentage2 = Utility.Clamp(percentage2, 0f, 1f);
			scrollAmount = percentage2 * (_contentHeight - _scissorRectHeight);
			SetScrollBarFromAmount();
			if (y2 != scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4");
			}
		}

		public void UpArrowPressed()
		{
			upArrow.scale = upArrow.baseScale;
			scrollAmount -= 64f;
			if (scrollAmount < 0f)
			{
				scrollAmount = 0f;
			}
			SetScrollBarFromAmount();
		}

		public void DownArrowPressed()
		{
			downArrow.scale = downArrow.baseScale;
			scrollAmount += 64f;
			if (scrollAmount > _contentHeight - _scissorRectHeight)
			{
				scrollAmount = _contentHeight - _scissorRectHeight;
			}
			SetScrollBarFromAmount();
		}

		private void SetScrollBarFromAmount()
		{
			if (!NeedsScroll())
			{
				scrollAmount = 0f;
				return;
			}
			if (scrollAmount < 8f)
			{
				scrollAmount = 0f;
			}
			if (scrollAmount > _contentHeight - _scissorRectHeight - 8f)
			{
				scrollAmount = _contentHeight - _scissorRectHeight;
			}
			scrollBar.bounds.Y = (int)((float)scrollBarBounds.Y + (float)(scrollBarBounds.Height - scrollBar.bounds.Height) / Math.Max(1f, _contentHeight - _scissorRectHeight) * scrollAmount);
		}

		public override void applyMovementKey(int direction)
		{
			base.applyMovementKey(direction);
			if (NeedsScroll())
			{
				switch (direction)
				{
				case 0:
					UpArrowPressed();
					break;
				case 2:
					DownArrowPressed();
					break;
				}
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
						_shownQuest = pages[currentPage][i];
						_objectiveText = _shownQuest.GetObjectiveDescriptions();
						_shownQuest.MarkAsViewed();
						scrollAmount = 0f;
						SetScrollBarFromAmount();
						if (Game1.options.SnappyMenus)
						{
							currentlySnappedComponent = getComponentWithID(102);
							currentlySnappedComponent.rightNeighborID = -7777;
							currentlySnappedComponent.downNeighborID = (HasMoneyReward() ? 103 : (_shownQuest.CanBeCancelled() ? 104 : (-1)));
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
				return;
			}
			Quest quest = _shownQuest as Quest;
			if (questPage != -1 && _shownQuest.ShouldDisplayAsComplete() && _shownQuest.HasMoneyReward() && rewardBox.containsPoint(x, y))
			{
				Game1.player.Money += _shownQuest.GetMoneyReward();
				Game1.playSound("purchaseRepeat");
				_shownQuest.OnMoneyRewardClaimed();
			}
			else if (questPage != -1 && quest != null && !quest.completed && (bool)quest.canBeCancelled && cancelQuestButton.containsPoint(x, y))
			{
				quest.accepted.Value = false;
				if (quest.dailyQuest.Value && quest.dayQuestAccepted.Value == Game1.Date.TotalDays)
				{
					Game1.player.acceptedDailyQuest.Set(newValue: false);
				}
				Game1.player.questLog.Remove(quest);
				pages[currentPage].RemoveAt(questPage);
				questPage = -1;
				Game1.playSound("trashcan");
				if (Game1.options.SnappyMenus && currentPage == 0)
				{
					currentlySnappedComponent = getComponentWithID(0);
					snapCursorToCurrentSnappedComponent();
				}
			}
			else if (!NeedsScroll() || backButton.containsPoint(x, y))
			{
				exitQuestPage();
			}
			if (NeedsScroll())
			{
				if (downArrow.containsPoint(x, y) && scrollAmount < _contentHeight - _scissorRectHeight)
				{
					DownArrowPressed();
					Game1.playSound("shwip");
				}
				else if (upArrow.containsPoint(x, y) && scrollAmount > 0f)
				{
					UpArrowPressed();
					Game1.playSound("shwip");
				}
				else if (scrollBar.containsPoint(x, y))
				{
					scrolling = true;
				}
				else if (scrollBarBounds.Contains(x, y))
				{
					scrolling = true;
				}
				else if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
				{
					scrolling = true;
					leftClickHeld(x, y);
					releaseLeftClick(x, y);
				}
			}
		}

		public bool HasReward()
		{
			return _shownQuest.HasReward();
		}

		public bool HasMoneyReward()
		{
			return _shownQuest.HasMoneyReward();
		}

		public void exitQuestPage()
		{
			if (_shownQuest.OnLeaveQuestPage())
			{
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
			if (questPage != -1 && HasReward())
			{
				rewardBox.scale = rewardBox.baseScale + Game1.dialogueButtonScale / 20f;
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11373"), xPositionOnScreen + base.width / 2, yPositionOnScreen - 64);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen, yPositionOnScreen, base.width, height, Color.White, 4f);
			if (questPage == -1)
			{
				for (int k = 0; k < questLogButtons.Count; k++)
				{
					if (pages.Count() > 0 && pages[currentPage].Count() > k)
					{
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), questLogButtons[k].bounds.X, questLogButtons[k].bounds.Y, questLogButtons[k].bounds.Width, questLogButtons[k].bounds.Height, questLogButtons[k].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
						if (pages[currentPage][k].ShouldDisplayAsNew() || pages[currentPage][k].ShouldDisplayAsComplete())
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(questLogButtons[k].bounds.X + 64 + 4, questLogButtons[k].bounds.Y + 44), new Rectangle(pages[currentPage][k].ShouldDisplayAsComplete() ? 341 : 317, 410, 23, 9), Color.White, 0f, new Vector2(11f, 4f), 4f + Game1.dialogueButtonScale * 10f / 250f, flipped: false, 0.99f);
						}
						else
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(questLogButtons[k].bounds.X + 32, questLogButtons[k].bounds.Y + 28), pages[currentPage][k].IsTimedQuest() ? new Rectangle(410, 501, 9, 9) : new Rectangle(395 + (pages[currentPage][k].IsTimedQuest() ? 3 : 0), 497, 3, 8), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f);
						}
						pages[currentPage][k].IsTimedQuest();
						SpriteText.drawString(b, pages[currentPage][k].GetName(), questLogButtons[k].bounds.X + 128 + 4, questLogButtons[k].bounds.Y + 20);
					}
				}
			}
			else
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, _shownQuest.GetName(), xPositionOnScreen + base.width / 2 + ((_shownQuest.IsTimedQuest() && _shownQuest.GetDaysLeft() > 0) ? (Math.Max(32, SpriteText.getWidthOfString(_shownQuest.GetName()) / 3) - 32) : 0), yPositionOnScreen + 32);
				if (_shownQuest.IsTimedQuest() && _shownQuest.GetDaysLeft() > 0)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(xPositionOnScreen + 32, yPositionOnScreen + 48 - 8), new Rectangle(410, 501, 9, 9), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f);
					Utility.drawTextWithShadow(b, Game1.parseText((pages[currentPage][questPage].GetDaysLeft() > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11374", pages[currentPage][questPage].GetDaysLeft()) : (Game1.IsEnglish() ? "Final Day" : Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11375", pages[currentPage][questPage].GetDaysLeft())), Game1.dialogueFont, base.width - 128), Game1.dialogueFont, new Vector2(xPositionOnScreen + 80, yPositionOnScreen + 48 - 8), Game1.textColor);
				}
				string description = Game1.parseText(_shownQuest.GetDescription(), Game1.dialogueFont, base.width - 128);
				Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
				Vector2 description_size = Game1.dialogueFont.MeasureString(description);
				Rectangle scissor_rect = default(Rectangle);
				scissor_rect.X = xPositionOnScreen + 32;
				scissor_rect.Y = yPositionOnScreen + 96;
				scissor_rect.Height = yPositionOnScreen + height - 32 - scissor_rect.Y;
				scissor_rect.Width = base.width - 64;
				_scissorRectHeight = scissor_rect.Height;
				scissor_rect = Utility.ConstrainScissorRectToScreen(scissor_rect);
				b.End();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState
				{
					ScissorTestEnable = true
				});
				Game1.graphics.GraphicsDevice.ScissorRectangle = scissor_rect;
				Utility.drawTextWithShadow(b, description, Game1.dialogueFont, new Vector2(xPositionOnScreen + 64, (float)yPositionOnScreen - scrollAmount + 96f), Game1.textColor);
				float yPos = (float)(yPositionOnScreen + 96) + description_size.Y + 32f - scrollAmount;
				if (_shownQuest.ShouldDisplayAsComplete())
				{
					b.End();
					b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
					SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11376"), xPositionOnScreen + 32 + 4, rewardBox.bounds.Y + 21 + 4);
					rewardBox.draw(b);
					if (HasMoneyReward())
					{
						b.Draw(Game1.mouseCursors, new Vector2(rewardBox.bounds.X + 16, (float)(rewardBox.bounds.Y + 16) - Game1.dialogueButtonScale / 2f), new Rectangle(280, 410, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", _shownQuest.GetMoneyReward()), xPositionOnScreen + 448, rewardBox.bounds.Y + 21 + 4);
					}
				}
				else
				{
					for (int j = 0; j < _objectiveText.Count; j++)
					{
						if (_shownQuest != null)
						{
							_ = (_shownQuest is SpecialOrder);
						}
						string parsed_text = Game1.parseText(_objectiveText[j], width: base.width - 192, whichFont: Game1.dialogueFont);
						bool display_as_complete = false;
						if (_shownQuest != null && _shownQuest is SpecialOrder)
						{
							display_as_complete = (_shownQuest as SpecialOrder).objectives[j].IsComplete();
						}
						if (!display_as_complete)
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(xPositionOnScreen + 96) + 8f * Game1.dialogueButtonScale / 10f, yPos), new Rectangle(412, 495, 5, 4), Color.White, (float)Math.PI / 2f, Vector2.Zero);
						}
						Color text_color = Color.DarkBlue;
						if (display_as_complete)
						{
							text_color = Game1.unselectedOptionColor;
						}
						Utility.drawTextWithShadow(b, parsed_text, Game1.dialogueFont, new Vector2(xPositionOnScreen + 128, yPos - 8f), text_color);
						yPos += Game1.dialogueFont.MeasureString(parsed_text).Y;
						if (_shownQuest != null && _shownQuest is SpecialOrder)
						{
							OrderObjective order_objective = (_shownQuest as SpecialOrder).objectives[j];
							if (order_objective.GetMaxCount() > 1 && order_objective.ShouldShowProgress())
							{
								Color dark_bar_color = Color.DarkRed;
								Color bar_color = Color.Red;
								if (order_objective.GetCount() >= order_objective.GetMaxCount())
								{
									bar_color = Color.LimeGreen;
									dark_bar_color = Color.Green;
								}
								int inset = 64;
								int objective_count_draw_width = 160;
								int notches = 4;
								Rectangle bar_background_source = new Rectangle(0, 224, 47, 12);
								Rectangle bar_notch_source = new Rectangle(47, 224, 1, 12);
								int bar_horizontal_padding = 3;
								int bar_vertical_padding = 3;
								int slice_width = 5;
								string objective_count_text = order_objective.GetCount() + "/" + order_objective.GetMaxCount();
								int max_text_width = (int)Game1.dialogueFont.MeasureString(order_objective.GetMaxCount() + "/" + order_objective.GetMaxCount()).X;
								int count_text_width = (int)Game1.dialogueFont.MeasureString(objective_count_text).X;
								int text_draw_position = xPositionOnScreen + base.width - inset - count_text_width;
								int max_text_draw_position = xPositionOnScreen + base.width - inset - max_text_width;
								Utility.drawTextWithShadow(b, objective_count_text, Game1.dialogueFont, new Vector2(text_draw_position, yPos), Color.DarkBlue);
								Rectangle bar_draw_position = new Rectangle(xPositionOnScreen + inset, (int)yPos, base.width - inset * 2 - objective_count_draw_width, bar_background_source.Height * 4);
								if (bar_draw_position.Right > max_text_draw_position - 16)
								{
									int adjustment = bar_draw_position.Right - (max_text_draw_position - 16);
									bar_draw_position.Width -= adjustment;
								}
								b.Draw(Game1.mouseCursors2, new Rectangle(bar_draw_position.X, bar_draw_position.Y, slice_width * 4, bar_draw_position.Height), new Rectangle(bar_background_source.X, bar_background_source.Y, slice_width, bar_background_source.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
								b.Draw(Game1.mouseCursors2, new Rectangle(bar_draw_position.X + slice_width * 4, bar_draw_position.Y, bar_draw_position.Width - 2 * slice_width * 4, bar_draw_position.Height), new Rectangle(bar_background_source.X + slice_width, bar_background_source.Y, bar_background_source.Width - 2 * slice_width, bar_background_source.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
								b.Draw(Game1.mouseCursors2, new Rectangle(bar_draw_position.Right - slice_width * 4, bar_draw_position.Y, slice_width * 4, bar_draw_position.Height), new Rectangle(bar_background_source.Right - slice_width, bar_background_source.Y, slice_width, bar_background_source.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
								float quest_progress = (float)order_objective.GetCount() / (float)order_objective.GetMaxCount();
								if (order_objective.GetMaxCount() < notches)
								{
									notches = order_objective.GetMaxCount();
								}
								bar_draw_position.X += 4 * bar_horizontal_padding;
								bar_draw_position.Width -= 4 * bar_horizontal_padding * 2;
								for (int i = 1; i < notches; i++)
								{
									b.Draw(Game1.mouseCursors2, new Vector2((float)bar_draw_position.X + (float)bar_draw_position.Width * ((float)i / (float)notches), bar_draw_position.Y), bar_notch_source, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
								}
								bar_draw_position.Y += 4 * bar_vertical_padding;
								bar_draw_position.Height -= 4 * bar_vertical_padding * 2;
								Rectangle rect = new Rectangle(bar_draw_position.X, bar_draw_position.Y, (int)((float)bar_draw_position.Width * quest_progress) - 4, bar_draw_position.Height);
								b.Draw(Game1.staminaRect, rect, null, bar_color, 0f, Vector2.Zero, SpriteEffects.None, (float)rect.Y / 10000f);
								rect.X = rect.Right;
								rect.Width = 4;
								b.Draw(Game1.staminaRect, rect, null, dark_bar_color, 0f, Vector2.Zero, SpriteEffects.None, (float)rect.Y / 10000f);
								yPos += (float)((bar_background_source.Height + 4) * 4);
							}
						}
						_contentHeight = yPos + scrollAmount - (float)scissor_rect.Y;
					}
					b.End();
					b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
					if (_shownQuest.CanBeCancelled())
					{
						cancelQuestButton.draw(b);
					}
					if (NeedsScroll())
					{
						if (scrollAmount > 0f)
						{
							b.Draw(Game1.staminaRect, new Rectangle(scissor_rect.X, scissor_rect.Top, scissor_rect.Width, 4), Color.Black * 0.15f);
						}
						if (scrollAmount < _contentHeight - _scissorRectHeight)
						{
							b.Draw(Game1.staminaRect, new Rectangle(scissor_rect.X, scissor_rect.Bottom - 4, scissor_rect.Width, 4), Color.Black * 0.15f);
						}
					}
				}
			}
			if (NeedsScroll())
			{
				upArrow.draw(b);
				downArrow.draw(b);
				scrollBar.draw(b);
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
