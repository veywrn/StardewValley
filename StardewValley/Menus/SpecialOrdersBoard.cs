using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class SpecialOrdersBoard : IClickableMenu
	{
		private Texture2D billboardTexture;

		public const int basewidth = 338;

		public const int baseheight = 198;

		public ClickableComponent acceptLeftQuestButton;

		public ClickableComponent acceptRightQuestButton;

		public string boardType = "";

		public SpecialOrder leftOrder;

		public SpecialOrder rightOrder;

		public string[] emojiIndices = new string[38]
		{
			"Abigail",
			"Penny",
			"Maru",
			"Leah",
			"Haley",
			"Emily",
			"Alex",
			"Shane",
			"Sebastian",
			"Sam",
			"Harvey",
			"Elliott",
			"Sandy",
			"Evelyn",
			"Marnie",
			"Caroline",
			"Robin",
			"Pierre",
			"Pam",
			"Jodi",
			"Lewis",
			"Linus",
			"Marlon",
			"Willy",
			"Wizard",
			"Morris",
			"Jas",
			"Vincent",
			"Krobus",
			"Dwarf",
			"Gus",
			"Gunther",
			"George",
			"Demetrius",
			"Clint",
			"Baby",
			"Baby",
			"Bear"
		};

		public SpecialOrdersBoard(string board_type = "")
			: base(0, 0, 0, 0, showUpperRightCloseButton: true)
		{
			SpecialOrder.UpdateAvailableSpecialOrders(force_refresh: false);
			boardType = board_type;
			if (boardType == "Qi")
			{
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SpecialOrdersBoard");
			}
			else
			{
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SpecialOrdersBoard");
			}
			width = 1352;
			height = 792;
			Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
			xPositionOnScreen = (int)center.X;
			yPositionOnScreen = (int)center.Y;
			acceptLeftQuestButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width / 4 - 128, yPositionOnScreen + height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 0,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			acceptRightQuestButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width * 3 / 4 - 128, yPositionOnScreen + height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
			{
				myID = 1,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			leftOrder = Game1.player.team.GetAvailableSpecialOrder(0, GetOrderType());
			rightOrder = Game1.player.team.GetAvailableSpecialOrder(1, GetOrderType());
			upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 20, yPositionOnScreen, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
			Game1.playSound("bigSelect");
			UpdateButtons();
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public virtual void UpdateButtons()
		{
			if (leftOrder == null)
			{
				acceptLeftQuestButton.visible = false;
			}
			if (rightOrder == null)
			{
				acceptRightQuestButton.visible = false;
			}
			if (Game1.player.team.acceptedSpecialOrderTypes.Contains(GetOrderType()))
			{
				acceptLeftQuestButton.visible = false;
				acceptRightQuestButton.visible = false;
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			Game1.activeClickableMenu = new SpecialOrdersBoard(boardType);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			Game1.playSound("bigDeSelect");
			exitThisMenu();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (acceptLeftQuestButton.visible && acceptLeftQuestButton.containsPoint(x, y))
			{
				Game1.playSound("newArtifact");
				if (leftOrder != null)
				{
					Game1.player.team.acceptedSpecialOrderTypes.Add(GetOrderType());
					SpecialOrder order2 = leftOrder;
					Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(order2.questKey.Value, order2.generationSeed));
					Game1.multiplayer.globalChatInfoMessage("AcceptedSpecialOrder", Game1.player.Name, order2.GetName());
					UpdateButtons();
				}
			}
			else if (acceptRightQuestButton.visible && acceptRightQuestButton.containsPoint(x, y))
			{
				Game1.playSound("newArtifact");
				if (rightOrder != null)
				{
					Game1.player.team.acceptedSpecialOrderTypes.Add(GetOrderType());
					SpecialOrder order = rightOrder;
					Game1.player.team.specialOrders.Add(SpecialOrder.GetSpecialOrder(order.questKey.Value, order.generationSeed));
					Game1.multiplayer.globalChatInfoMessage("AcceptedSpecialOrder", Game1.player.Name, order.GetName());
					UpdateButtons();
				}
			}
		}

		public string GetOrderType()
		{
			return boardType;
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (Game1.questOfTheDay != null && !Game1.questOfTheDay.accepted)
			{
				float oldScale2 = acceptLeftQuestButton.scale;
				acceptLeftQuestButton.scale = (acceptLeftQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
				if (acceptLeftQuestButton.scale > oldScale2)
				{
					Game1.playSound("Cowboy_gunshot");
				}
				oldScale2 = acceptRightQuestButton.scale;
				acceptRightQuestButton.scale = (acceptRightQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
				if (acceptRightQuestButton.scale > oldScale2)
				{
					Game1.playSound("Cowboy_gunshot");
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			b.Draw(billboardTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, (boardType == "Qi") ? 198 : 0, 338, 198), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			if (leftOrder != null && leftOrder.IsIslandOrder())
			{
				b.Draw(billboardTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(338, 0, 169, 198), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (rightOrder != null && rightOrder.IsIslandOrder())
			{
				b.Draw(billboardTexture, new Vector2(xPositionOnScreen + 676, yPositionOnScreen), new Rectangle(507, 0, 169, 198), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (!Game1.player.team.acceptedSpecialOrderTypes.Contains(GetOrderType()))
			{
				SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:ChooseOne"), xPositionOnScreen + width / 2, Math.Max(10, yPositionOnScreen - 70), SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\UI:ChooseOne")) + 1);
			}
			if (leftOrder != null)
			{
				SpecialOrder order2 = leftOrder;
				DrawQuestDetails(b, order2, xPositionOnScreen + 64 + 32);
			}
			if (rightOrder != null)
			{
				SpecialOrder order = rightOrder;
				DrawQuestDetails(b, order, xPositionOnScreen + 704 + 32);
			}
			if (acceptLeftQuestButton.visible)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), acceptLeftQuestButton.bounds.X, acceptLeftQuestButton.bounds.Y, acceptLeftQuestButton.bounds.Width, acceptLeftQuestButton.bounds.Height, (acceptLeftQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * acceptLeftQuestButton.scale);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2(acceptLeftQuestButton.bounds.X + 12, acceptLeftQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
			}
			if (acceptRightQuestButton.visible)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), acceptRightQuestButton.bounds.X, acceptRightQuestButton.bounds.Y, acceptRightQuestButton.bounds.Width, acceptRightQuestButton.bounds.Height, (acceptRightQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * acceptRightQuestButton.scale);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2(acceptRightQuestButton.bounds.X + 12, acceptRightQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
			}
			base.draw(b);
			Game1.mouseCursorTransparency = 1f;
			if (!Game1.options.SnappyMenus || acceptLeftQuestButton.visible || acceptRightQuestButton.visible)
			{
				drawMouse(b);
			}
		}

		public KeyValuePair<Texture2D, Rectangle>? GetPortraitForRequester(string requester_name)
		{
			if (requester_name == null)
			{
				return null;
			}
			for (int i = 0; i < emojiIndices.Length; i++)
			{
				if (emojiIndices[i] == requester_name)
				{
					return new KeyValuePair<Texture2D, Rectangle>(ChatBox.emojiTexture, new Rectangle(i % 14 * 9, 99 + i / 14 * 9, 9, 9));
				}
			}
			return null;
		}

		public void DrawQuestDetails(SpriteBatch b, SpecialOrder order, int x)
		{
			bool dehighlight = false;
			bool found_match = false;
			foreach (SpecialOrder active_order in Game1.player.team.specialOrders)
			{
				if (active_order.questState.Value == SpecialOrder.QuestState.InProgress)
				{
					foreach (SpecialOrder available_order in Game1.player.team.availableSpecialOrders)
					{
						if (!(available_order.orderType.Value != GetOrderType()) && active_order.questKey.Value == available_order.questKey.Value)
						{
							if (order.questKey != active_order.questKey)
							{
								dehighlight = true;
							}
							found_match = true;
							break;
						}
					}
					if (found_match)
					{
						break;
					}
				}
			}
			if (!found_match && Game1.player.team.acceptedSpecialOrderTypes.Contains(GetOrderType()))
			{
				dehighlight = true;
			}
			SpriteFont font = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont;
			Color font_color = Game1.textColor;
			float shadow_intensity = 0.5f;
			float graphic_alpha = 1f;
			if (dehighlight)
			{
				font_color = Game1.textColor * 0.25f;
				shadow_intensity = 0f;
				graphic_alpha = 0.25f;
			}
			if (boardType == "Qi")
			{
				font_color = Color.White;
				shadow_intensity = 0f;
				if (dehighlight)
				{
					font_color = Color.White * 0.25f;
					graphic_alpha = 0.25f;
				}
			}
			int header_y = yPositionOnScreen + 128;
			string order_name = order.GetName();
			KeyValuePair<Texture2D, Rectangle>? drawn_portrait = GetPortraitForRequester(order.requester.Value);
			if (drawn_portrait.HasValue)
			{
				Utility.drawWithShadow(b, drawn_portrait.Value.Key, new Vector2(x, header_y), drawn_portrait.Value.Value, Color.White * graphic_alpha, 0f, Vector2.Zero, 4f, flipped: false, -1f, -1, -1, shadow_intensity * 0.6f);
			}
			Utility.drawTextWithShadow(b, order_name, font, new Vector2((float)(x + 256) - font.MeasureString(order_name).X / 2f, header_y), font_color, 1f, -1f, -1, -1, shadow_intensity);
			string raw_description = order.GetDescription();
			string description = Game1.parseText(raw_description, font, 512);
			float height = font.MeasureString(description).Y;
			float scale = 1f;
			float max_height = 400f;
			while (height > max_height && !(scale <= 0.25f))
			{
				scale -= 0.05f;
				description = Game1.parseText(raw_description, font, (int)(512f / scale));
				height = font.MeasureString(description).Y;
			}
			Utility.drawTextWithShadow(b, description, font, new Vector2(x, yPositionOnScreen + 192), font_color, scale, -1f, -1, -1, shadow_intensity);
			if (dehighlight)
			{
				return;
			}
			int days_left = order.GetDaysLeft();
			int due_date_y_position = yPositionOnScreen + 576;
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x, due_date_y_position), new Rectangle(410, 501, 9, 9), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f, -1, -1, shadow_intensity * 0.6f);
			Utility.drawTextWithShadow(b, Game1.parseText((days_left > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11374", days_left) : Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11375", days_left), Game1.dialogueFont, width - 128), Game1.dialogueFont, new Vector2(x + 48, due_date_y_position), font_color, 1f, -1f, -1, -1, shadow_intensity);
			if (boardType == "Qi")
			{
				int reward = -1;
				GemsReward gems = null;
				foreach (OrderReward r in order.rewards)
				{
					if (r is GemsReward)
					{
						gems = (r as GemsReward);
						break;
					}
				}
				if (gems != null)
				{
					reward = gems.amount;
				}
				if (reward != -1)
				{
					Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2((float)x + 512f / scale - Game1.dialogueFont.MeasureString(string.Concat(reward)).X - 12f - 60f, due_date_y_position - 8), new Rectangle(288, 561, 15, 15), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f, -1, -1, shadow_intensity * 0.6f);
					Utility.drawTextWithShadow(b, Game1.parseText(string.Concat(reward), Game1.dialogueFont, width - 128), Game1.dialogueFont, new Vector2((float)x + 512f / scale - Game1.dialogueFont.MeasureString(string.Concat(reward)).X - 4f, due_date_y_position), font_color, 1f, -1f, -1, -1, shadow_intensity);
					Utility.drawTextWithShadow(b, Game1.parseText(Utility.loadStringShort("StringsFromCSFiles", "QuestLog.cs.11376"), Game1.dialogueFont, width - 128), Game1.dialogueFont, new Vector2((float)x + 512f / scale - Game1.dialogueFont.MeasureString(Utility.loadStringShort("StringsFromCSFiles", "QuestLog.cs.11376")).X + 8f, due_date_y_position - 60), font_color * 0.6f, 1f, -1f, -1, -1, shadow_intensity);
				}
			}
		}
	}
}
