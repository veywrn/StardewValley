using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class PondQueryMenu : IClickableMenu
	{
		public const int region_okButton = 101;

		public const int region_emptyButton = 103;

		public const int region_noButton = 105;

		public const int region_nettingButton = 106;

		public new static int width = 384;

		public new static int height = 512;

		public const int unresolved_needs_extra_height = 116;

		protected FishPond _pond;

		protected Object _fishItem;

		protected string _statusText = "";

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent emptyButton;

		public ClickableTextureComponent yesButton;

		public ClickableTextureComponent noButton;

		public ClickableTextureComponent changeNettingButton;

		private bool confirmingEmpty;

		protected Rectangle _confirmationBoxRectangle;

		protected string _confirmationText;

		protected float _age;

		private string hoverText = "";

		public PondQueryMenu(FishPond fish_pond)
			: base(Game1.uiViewport.Width / 2 - width / 2, Game1.uiViewport.Height / 2 - height / 2, width, height)
		{
			Game1.player.Halt();
			width = 384;
			height = 512;
			_pond = fish_pond;
			_fishItem = new Object(_pond.fishType.Value, 1);
			okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 101,
				upNeighborID = -99998
			};
			emptyButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 256 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Rectangle(32, 384, 16, 16), 4f)
			{
				myID = 103,
				downNeighborID = -99998
			};
			changeNettingButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Rectangle(48, 384, 16, 16), 4f)
			{
				myID = 106,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
			UpdateState();
			yPositionOnScreen = Game1.uiViewport.Height / 2 - measureTotalHeight() / 2;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(101);
			snapCursorToCurrentSnappedComponent();
		}

		public void textBoxEnter(TextBox sender)
		{
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (Game1.options.menuButton.Contains(new InputButton(key)))
			{
				Game1.playSound("smallSelect");
				if (readyToClose())
				{
					Game1.exitActiveMenu();
				}
			}
			else if (Game1.options.SnappyMenus && !Game1.options.menuButton.Contains(new InputButton(key)))
			{
				base.receiveKeyPress(key);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			_age += (float)time.ElapsedGameTime.TotalSeconds;
		}

		public void finishedPlacingAnimal()
		{
			Game1.exitActiveMenu();
			Game1.currentLocation = Game1.player.currentLocation;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear();
			Game1.displayHUD = true;
			Game1.viewportFreeze = false;
			Game1.displayFarmer = true;
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_HomeChanged"), Color.LimeGreen, 3500f));
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (confirmingEmpty)
			{
				if (yesButton.containsPoint(x, y))
				{
					Game1.playSound("fishSlap");
					_pond.ClearPond();
					exitThisMenu();
				}
				else if (noButton.containsPoint(x, y))
				{
					confirmingEmpty = false;
					Game1.playSound("smallSelect");
					if (Game1.options.SnappyMenus)
					{
						currentlySnappedComponent = getComponentWithID(103);
						snapCursorToCurrentSnappedComponent();
					}
				}
				return;
			}
			if (okButton != null && okButton.containsPoint(x, y) && readyToClose())
			{
				Game1.exitActiveMenu();
				Game1.playSound("smallSelect");
			}
			if (changeNettingButton.containsPoint(x, y))
			{
				Game1.playSound("drumkit6");
				_pond.nettingStyle.Value++;
				_pond.nettingStyle.Value %= 4;
			}
			else if (emptyButton.containsPoint(x, y))
			{
				_confirmationBoxRectangle = new Rectangle(0, 0, 400, 100);
				_confirmationBoxRectangle.X = Game1.uiViewport.Width / 2 - _confirmationBoxRectangle.Width / 2;
				_confirmationText = Game1.content.LoadString("Strings\\UI:PondQuery_ConfirmEmpty");
				_confirmationText = Game1.parseText(_confirmationText, Game1.smallFont, _confirmationBoxRectangle.Width);
				Vector2 text_size = Game1.smallFont.MeasureString(_confirmationText);
				_confirmationBoxRectangle.Height = (int)text_size.Y;
				_confirmationBoxRectangle.Y = Game1.uiViewport.Height / 2 - _confirmationBoxRectangle.Height / 2;
				confirmingEmpty = true;
				yesButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 - 64 - 4, _confirmationBoxRectangle.Bottom + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
				{
					myID = 111,
					rightNeighborID = 105
				};
				noButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 + 4, _confirmationBoxRectangle.Bottom + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
				{
					myID = 105,
					leftNeighborID = 111
				};
				Game1.playSound("smallSelect");
				if (Game1.options.SnappyMenus)
				{
					populateClickableComponentList();
					currentlySnappedComponent = noButton;
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override bool readyToClose()
		{
			if (base.readyToClose())
			{
				return !Game1.globalFade;
			}
			return false;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!Game1.globalFade && readyToClose())
			{
				Game1.exitActiveMenu();
				Game1.playSound("smallSelect");
			}
		}

		public override void performHoverAction(int x, int y)
		{
			hoverText = "";
			if (okButton != null)
			{
				if (okButton.containsPoint(x, y))
				{
					okButton.scale = Math.Min(1.1f, okButton.scale + 0.05f);
				}
				else
				{
					okButton.scale = Math.Max(1f, okButton.scale - 0.05f);
				}
			}
			if (emptyButton != null)
			{
				if (emptyButton.containsPoint(x, y))
				{
					emptyButton.scale = Math.Min(4.1f, emptyButton.scale + 0.05f);
					hoverText = Game1.content.LoadString("Strings\\UI:PondQuery_EmptyPond", 10);
				}
				else
				{
					emptyButton.scale = Math.Max(4f, emptyButton.scale - 0.05f);
				}
			}
			if (changeNettingButton != null)
			{
				if (changeNettingButton.containsPoint(x, y))
				{
					changeNettingButton.scale = Math.Min(4.1f, changeNettingButton.scale + 0.05f);
					hoverText = Game1.content.LoadString("Strings\\UI:PondQuery_ChangeNetting", 10);
				}
				else
				{
					changeNettingButton.scale = Math.Max(4f, emptyButton.scale - 0.05f);
				}
			}
			if (yesButton != null)
			{
				if (yesButton.containsPoint(x, y))
				{
					yesButton.scale = Math.Min(1.1f, yesButton.scale + 0.05f);
				}
				else
				{
					yesButton.scale = Math.Max(1f, yesButton.scale - 0.05f);
				}
			}
			if (noButton != null)
			{
				if (noButton.containsPoint(x, y))
				{
					noButton.scale = Math.Min(1.1f, noButton.scale + 0.05f);
				}
				else
				{
					noButton.scale = Math.Max(1f, noButton.scale - 0.05f);
				}
			}
		}

		public static string GetFishTalkSuffix(Object fishItem)
		{
			List<string> tags = fishItem.GetContextTagList();
			for (int j = 0; j < tags.Count; j++)
			{
				string tag = tags[j];
				if (!tag.StartsWith("fish_talk_"))
				{
					continue;
				}
				if (tag == "fish_talk_rude")
				{
					return "_Rude";
				}
				if (tag == "fish_talk_stiff")
				{
					return "_Stiff";
				}
				if (tag == "fish_talk_demanding")
				{
					return "_Demanding";
				}
				string talk_type2 = tag.Substring("fish_talk_".Length);
				talk_type2 = "_" + talk_type2;
				char[] array = talk_type2.ToCharArray();
				bool capitalize_next = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == '_')
					{
						capitalize_next = true;
					}
					else if (capitalize_next)
					{
						array[i] = char.ToUpper(array[i]);
						capitalize_next = false;
					}
				}
				return new string(array);
			}
			if (tags.Contains("fish_carnivorous"))
			{
				return "_Carnivore";
			}
			return "";
		}

		public static string getCompletedRequestString(FishPond pond, Object fishItem, Random r)
		{
			if (fishItem != null)
			{
				string talk_suffix = GetFishTalkSuffix(fishItem);
				if (talk_suffix != "")
				{
					return Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete" + talk_suffix + r.Next(3), pond.neededItem.Value.DisplayName));
				}
			}
			return Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete" + r.Next(7), pond.neededItem.Value.DisplayName);
		}

		public void UpdateState()
		{
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)_pond.seedOffset);
			if (_pond.currentOccupants.Value <= 0)
			{
				_statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusNoFish");
				return;
			}
			if (_pond.neededItem.Value != null)
			{
				if ((bool)_pond.hasCompletedRequest)
				{
					_statusText = getCompletedRequestString(_pond, _fishItem, r);
					return;
				}
				if (_pond.HasUnresolvedNeeds())
				{
					string item_count_string = string.Concat(_pond.neededItemCount.Value);
					if (_pond.neededItemCount.Value <= 1)
					{
						item_count_string = Lexicon.getProperArticleForWord(_pond.neededItem.Value.DisplayName);
						if (item_count_string == "")
						{
							item_count_string = Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestOneCount");
						}
					}
					if (_fishItem != null)
					{
						if (_fishItem.GetContextTagList().Contains("fish_talk_rude"))
						{
							_statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending_Rude" + r.Next(3) + "_" + (Game1.player.isMale ? "Male" : "Female"), Lexicon.makePlural(_pond.neededItem.Value.DisplayName, _pond.neededItemCount.Value == 1), item_count_string, _pond.neededItem.Value.DisplayName));
							return;
						}
						string talk_suffix = GetFishTalkSuffix(_fishItem);
						if (talk_suffix != "")
						{
							_statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending" + talk_suffix + r.Next(3), Lexicon.makePlural(_pond.neededItem.Value.DisplayName, _pond.neededItemCount.Value == 1), item_count_string, _pond.neededItem.Value.DisplayName));
							return;
						}
					}
					_statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending" + r.Next(7), Lexicon.makePlural(_pond.neededItem.Value.DisplayName, _pond.neededItemCount.Value == 1), item_count_string, _pond.neededItem.Value.DisplayName));
					return;
				}
			}
			if (_fishItem != null && ((int)_fishItem.parentSheetIndex == 397 || (int)_fishItem.parentSheetIndex == 393))
			{
				_statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusOk_Coral", _fishItem.DisplayName);
			}
			else
			{
				_statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusOk" + r.Next(7));
			}
		}

		private int measureTotalHeight()
		{
			return 644 + measureExtraTextHeight(getDisplayedText());
		}

		private int measureExtraTextHeight(string displayed_text)
		{
			return Math.Max(0, (int)Game1.smallFont.MeasureString(displayed_text).Y - 90) + 4;
		}

		private string getDisplayedText()
		{
			return Game1.parseText(_statusText, Game1.smallFont, width - IClickableMenu.spaceToClearSideBorder * 2 - 64);
		}

		public override void draw(SpriteBatch b)
		{
			if (!Game1.globalFade)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				bool has_unresolved_needs = _pond.neededItem.Value != null && _pond.HasUnresolvedNeeds() && !_pond.hasCompletedRequest;
				string pond_name_text = Game1.content.LoadString("Strings\\UI:PondQuery_Name", _fishItem.DisplayName);
				Vector2 text_size4 = Game1.smallFont.MeasureString(pond_name_text);
				Game1.DrawBox((int)((float)(Game1.uiViewport.Width / 2) - (text_size4.X + 64f) * 0.5f), yPositionOnScreen - 4 + 128, (int)(text_size4.X + 64f), 64);
				Utility.drawTextWithShadow(b, pond_name_text, Game1.smallFont, new Vector2((float)(Game1.uiViewport.Width / 2) - text_size4.X * 0.5f, (float)(yPositionOnScreen - 4) + 160f - text_size4.Y * 0.5f), Color.Black);
				string displayed_text = getDisplayedText();
				int extraHeight = 0;
				if (has_unresolved_needs)
				{
					extraHeight += 116;
				}
				int extraTextHeight = measureExtraTextHeight(displayed_text);
				Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen + 128, width, height - 128 + extraHeight + extraTextHeight, speaker: false, drawOnlyBox: true);
				string population_text = Game1.content.LoadString("Strings\\UI:PondQuery_Population", string.Concat(_pond.FishCount), _pond.maxOccupants);
				text_size4 = Game1.smallFont.MeasureString(population_text);
				Utility.drawTextWithShadow(b, population_text, Game1.smallFont, new Vector2((float)(xPositionOnScreen + width / 2) - text_size4.X * 0.5f, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 + 128), Game1.textColor);
				int slots_to_draw = _pond.maxOccupants;
				float slot_spacing = 13f;
				int x = 0;
				int y = 0;
				for (int i = 0; i < slots_to_draw; i++)
				{
					float y_offset = (float)Math.Sin(_age * 1f + (float)x * 0.75f + (float)y * 0.25f) * 2f;
					if (i < _pond.FishCount)
					{
						_fishItem.drawInMenu(b, new Vector2((float)(xPositionOnScreen + width / 2) - slot_spacing * (float)Math.Min(slots_to_draw, 5) * 4f * 0.5f + slot_spacing * 4f * (float)x - 12f, (float)(yPositionOnScreen + (int)(y_offset * 4f)) + (float)(y * 4) * slot_spacing + 275.2f), 0.75f, 1f, 0f, StackDrawType.Hide, Color.White, drawShadow: false);
					}
					else
					{
						_fishItem.drawInMenu(b, new Vector2((float)(xPositionOnScreen + width / 2) - slot_spacing * (float)Math.Min(slots_to_draw, 5) * 4f * 0.5f + slot_spacing * 4f * (float)x - 12f, (float)(yPositionOnScreen + (int)(y_offset * 4f)) + (float)(y * 4) * slot_spacing + 275.2f), 0.75f, 0.35f, 0f, StackDrawType.Hide, Color.Black, drawShadow: false);
					}
					x++;
					if (x == 5)
					{
						x = 0;
						y++;
					}
				}
				text_size4 = Game1.smallFont.MeasureString(displayed_text);
				Utility.drawTextWithShadow(b, displayed_text, Game1.smallFont, new Vector2((float)(xPositionOnScreen + width / 2) - text_size4.X * 0.5f, (float)(yPositionOnScreen + height + extraTextHeight - (has_unresolved_needs ? 32 : 48)) - text_size4.Y), Game1.textColor);
				if (has_unresolved_needs)
				{
					drawHorizontalPartition(b, (int)((float)(yPositionOnScreen + height + extraTextHeight) - 48f));
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(xPositionOnScreen + 60) + 8f * Game1.dialogueButtonScale / 10f, yPositionOnScreen + height + extraTextHeight + 28), new Rectangle(412, 495, 5, 4), Color.White, (float)Math.PI / 2f, Vector2.Zero);
					string bring_text = Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequest_Bring");
					text_size4 = Game1.smallFont.MeasureString(bring_text);
					int left_x = xPositionOnScreen + 88;
					float text_x = left_x;
					float icon_x = text_x + text_size4.X + 4f;
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr)
					{
						icon_x = left_x - 8;
						text_x = left_x + 76;
					}
					Utility.drawTextWithShadow(b, bring_text, Game1.smallFont, new Vector2(text_x, yPositionOnScreen + height + extraTextHeight + 24), Game1.textColor);
					b.Draw(Game1.objectSpriteSheet, new Vector2(icon_x, yPositionOnScreen + height + extraTextHeight + 4), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, _pond.neededItem.Value.parentSheetIndex, 16, 16), Color.Black * 0.4f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					b.Draw(Game1.objectSpriteSheet, new Vector2(icon_x + 4f, yPositionOnScreen + height + extraTextHeight), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, _pond.neededItem.Value.parentSheetIndex, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					if ((int)_pond.neededItemCount > 1)
					{
						Utility.drawTinyDigits(_pond.neededItemCount, b, new Vector2(icon_x + 48f, yPositionOnScreen + height + extraTextHeight + 48), 3f, 1f, Color.White);
					}
				}
				okButton.draw(b);
				emptyButton.draw(b);
				changeNettingButton.draw(b);
				if (confirmingEmpty)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
					int padding = 16;
					_confirmationBoxRectangle.Width += padding;
					_confirmationBoxRectangle.Height += padding;
					_confirmationBoxRectangle.X -= padding / 2;
					_confirmationBoxRectangle.Y -= padding / 2;
					Game1.DrawBox(_confirmationBoxRectangle.X, _confirmationBoxRectangle.Y, _confirmationBoxRectangle.Width, _confirmationBoxRectangle.Height);
					_confirmationBoxRectangle.Width -= padding;
					_confirmationBoxRectangle.Height -= padding;
					_confirmationBoxRectangle.X += padding / 2;
					_confirmationBoxRectangle.Y += padding / 2;
					b.DrawString(Game1.smallFont, _confirmationText, new Vector2(_confirmationBoxRectangle.X, _confirmationBoxRectangle.Y), Game1.textColor);
					yesButton.draw(b);
					noButton.draw(b);
				}
				else if (hoverText != null && hoverText.Length > 0)
				{
					IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
				}
			}
			drawMouse(b);
		}
	}
}
