using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class AdvancedGameOptions : IClickableMenu
	{
		public const int itemsPerPage = 7;

		private string hoverText = "";

		public List<ClickableComponent> optionSlots = new List<ClickableComponent>();

		public int currentItemIndex;

		private ClickableTextureComponent upArrow;

		private ClickableTextureComponent downArrow;

		private ClickableTextureComponent scrollBar;

		public ClickableTextureComponent okButton;

		public List<Action> applySettingCallbacks = new List<Action>();

		public Dictionary<OptionsElement, string> tooltips = new Dictionary<OptionsElement, string>();

		public int ID_okButton = 10000;

		private bool scrolling;

		public List<OptionsElement> options = new List<OptionsElement>();

		private Rectangle scrollBarBounds;

		protected static int _lastSelectedIndex;

		protected static int _lastCurrentItemIndex;

		protected int _lastHoveredIndex;

		protected int _hoverDuration;

		public const int WINDOW_WIDTH = 800;

		public const int WINDOW_HEIGHT = 500;

		public bool initialMonsterSpawnAtValue;

		private int optionsSlotHeld = -1;

		public AdvancedGameOptions()
			: base(Game1.uiViewport.Width / 2 - 400, Game1.uiViewport.Height / 2 - 250, 800, 500)
		{
			int scrollbar_x = xPositionOnScreen + width + 16;
			upArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, yPositionOnScreen, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
			downArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
			scrollBarBounds = default(Rectangle);
			scrollBarBounds.X = upArrow.bounds.X + 12;
			scrollBarBounds.Width = 24;
			scrollBarBounds.Y = upArrow.bounds.Y + upArrow.bounds.Height + 4;
			scrollBarBounds.Height = downArrow.bounds.Y - 4 - scrollBarBounds.Y;
			scrollBar = new ClickableTextureComponent(new Rectangle(scrollBarBounds.X, scrollBarBounds.Y, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
			for (int i = 0; i < 7; i++)
			{
				optionSlots.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + i * ((height - 16) / 7), width - 16, height / 7), string.Concat(i))
				{
					myID = i,
					downNeighborID = ((i < 6) ? (i + 1) : (-7777)),
					upNeighborID = ((i > 0) ? (i - 1) : (-7777)),
					fullyImmutable = true
				});
			}
			PopulateOptions();
			okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen, yPositionOnScreen + height + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = ID_okButton,
				upNeighborID = -99998
			};
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				setCurrentlySnappedComponentTo(ID_okButton);
				snapCursorToCurrentSnappedComponent();
			}
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			base.customSnapBehavior(direction, oldRegion, oldID);
			if (oldID == 6 && direction == 2)
			{
				if (currentItemIndex < Math.Max(0, options.Count - 7))
				{
					downArrowPressed();
					Game1.playSound("shiny4");
					return;
				}
				currentlySnappedComponent = getComponentWithID(ID_okButton);
				if (currentlySnappedComponent != null)
				{
					currentlySnappedComponent.upNeighborID = Math.Min(options.Count, 7) - 1;
				}
			}
			else if (oldID == 0 && direction == 0)
			{
				if (currentItemIndex > 0)
				{
					upArrowPressed();
					Game1.playSound("shiny4");
				}
				else
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public virtual void PopulateOptions()
		{
			options.Clear();
			tooltips.Clear();
			applySettingCallbacks.Clear();
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:AGO_Label")));
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:AGO_CCB"))
			{
				style = OptionsElement.Style.OptionLabel
			});
			AddDropdown("", Game1.content.LoadString("Strings\\UI:AGO_CCB_Tooltip"), () => Game1.bundleType, delegate(Game1.BundleType val)
			{
				Game1.bundleType = val;
			}, new KeyValuePair<string, Game1.BundleType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Normal"), Game1.BundleType.Default), new KeyValuePair<string, Game1.BundleType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Remixed"), Game1.BundleType.Remixed));
			AddCheckbox(Game1.content.LoadString("Strings\\UI:AGO_Year1Completable"), Game1.content.LoadString("Strings\\UI:AGO_Year1Completable_Tooltip"), () => Game1.game1.GetNewGameOption<bool>("YearOneCompletable"), delegate(bool val)
			{
				Game1.game1.SetNewGameOption("YearOneCompletable", val);
			});
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:AGO_MineTreasureShuffle"))
			{
				style = OptionsElement.Style.OptionLabel
			});
			AddDropdown("", Game1.content.LoadString("Strings\\UI:AGO_MineTreasureShuffle_Tooltip"), () => Game1.game1.GetNewGameOption<Game1.MineChestType>("MineChests"), delegate(Game1.MineChestType val)
			{
				Game1.game1.SetNewGameOption("MineChests", val);
			}, new KeyValuePair<string, Game1.MineChestType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Normal"), Game1.MineChestType.Default), new KeyValuePair<string, Game1.MineChestType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Remixed"), Game1.MineChestType.Remixed));
			AddCheckbox(Game1.content.LoadString("Strings\\UI:AGO_FarmMonsters"), Game1.content.LoadString("Strings\\UI:AGO_FarmMonsters_Tooltip"), delegate
			{
				bool result2 = Game1.spawnMonstersAtNight;
				if (Game1.game1.newGameSetupOptions.ContainsKey("SpawnMonstersAtNight"))
				{
					result2 = Game1.game1.GetNewGameOption<bool>("SpawnMonstersAtNight");
				}
				initialMonsterSpawnAtValue = result2;
				return result2;
			}, delegate(bool val)
			{
				if (initialMonsterSpawnAtValue != val)
				{
					Game1.game1.SetNewGameOption("SpawnMonstersAtNight", val);
				}
			});
			AddDropdown(Game1.content.LoadString("Strings\\UI:Character_Difficulty"), Game1.content.LoadString("Strings\\UI:AGO_ProfitMargin_Tooltip"), () => Game1.player.difficultyModifier, delegate(float val)
			{
				Game1.player.difficultyModifier = val;
			}, new KeyValuePair<string, float>(Game1.content.LoadString("Strings\\UI:Character_Normal"), 1f), new KeyValuePair<string, float>("75%", 0.75f), new KeyValuePair<string, float>("50%", 0.5f), new KeyValuePair<string, float>("25%", 0.25f));
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:AGO_MPOptions_Label")));
			AddDropdown(Game1.content.LoadString("Strings\\UI:Character_StartingCabins"), Game1.content.LoadString("Strings\\UI:AGO_StartingCabins_Tooltip"), () => Game1.startingCabins, delegate(int val)
			{
				Game1.startingCabins = val;
			}, new KeyValuePair<string, int>(Game1.content.LoadString("Strings\\UI:Character_none"), 0), new KeyValuePair<string, int>("1", 1), new KeyValuePair<string, int>("2", 2), new KeyValuePair<string, int>("3", 3));
			AddDropdown(Game1.content.LoadString("Strings\\UI:Character_CabinLayout"), Game1.content.LoadString("Strings\\UI:AGO_CabinLayout_Tooltip"), () => Game1.cabinsSeparate, delegate(bool val)
			{
				Game1.cabinsSeparate = val;
			}, new KeyValuePair<string, bool>(Game1.content.LoadString("Strings\\UI:Character_Close"), value: false), new KeyValuePair<string, bool>(Game1.content.LoadString("Strings\\UI:Character_Separate"), value: true));
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:AGO_OtherOptions_Label")));
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:AGO_RandomSeed"))
			{
				style = OptionsElement.Style.OptionLabel
			});
			OptionsTextEntry optionsTextEntry = AddTextEntry("", Game1.content.LoadString("Strings\\UI:AGO_RandomSeed_Tooltip"), () => (!Game1.startingGameSeed.HasValue) ? "" : Game1.startingGameSeed.Value.ToString(), delegate(string val)
			{
				val.Trim();
				if (string.IsNullOrEmpty(val))
				{
					Game1.startingGameSeed = null;
				}
				else
				{
					ulong result = 0uL;
					while (true)
					{
						if (val.Length <= 0)
						{
							return;
						}
						if (ulong.TryParse(val, out result))
						{
							break;
						}
						val = val.Substring(0, val.Length - 1);
					}
					Game1.startingGameSeed = result;
				}
			});
			optionsTextEntry.textBox.numbersOnly = true;
			optionsTextEntry.textBox.textLimit = 9;
			for (int i = options.Count; i < 7; i++)
			{
				options.Add(new OptionsElement(""));
			}
		}

		public virtual void CloseAndApply()
		{
			foreach (Action applySettingCallback in applySettingCallbacks)
			{
				applySettingCallback();
			}
			applySettingCallbacks.Clear();
			exitThisMenu();
		}

		public virtual OptionsTextEntry AddTextEntry(string label, string tooltip, Func<string> get, Action<string> set)
		{
			OptionsTextEntry option_element = new OptionsTextEntry(label, -999);
			tooltips[option_element] = tooltip;
			option_element.textBox.Text = get();
			applySettingCallbacks.Add(delegate
			{
				set(option_element.textBox.Text);
			});
			options.Add(option_element);
			return option_element;
		}

		public OptionsDropDown AddDropdown<T>(string label, string tooltip, Func<T> get, Action<T> set, params KeyValuePair<string, T>[] dropdown_options)
		{
			OptionsDropDown option_element = new OptionsDropDown(label, -999);
			tooltips[option_element] = tooltip;
			KeyValuePair<string, T>[] array = dropdown_options;
			for (int j = 0; j < array.Length; j++)
			{
				KeyValuePair<string, T> option = array[j];
				option_element.dropDownDisplayOptions.Add(option.Key);
				option_element.dropDownOptions.Add(option.Value.ToString());
			}
			option_element.RecalculateBounds();
			T selected_value = get();
			int selected_option = 0;
			for (int i = 0; i < dropdown_options.Length; i++)
			{
				KeyValuePair<string, T> dropdown_option = dropdown_options[i];
				if ((dropdown_option.Value == null && selected_value == null) || (dropdown_option.Value != null && selected_value != null && dropdown_option.Value.Equals(selected_value)))
				{
					selected_option = i;
					break;
				}
			}
			option_element.selectedOption = selected_option;
			applySettingCallbacks.Add(delegate
			{
				set(dropdown_options[option_element.selectedOption].Value);
			});
			options.Add(option_element);
			return option_element;
		}

		public virtual OptionsCheckbox AddCheckbox(string label, string tooltip, Func<bool> get, Action<bool> set)
		{
			OptionsCheckbox option_element = new OptionsCheckbox(label, -999);
			tooltips[option_element] = tooltip;
			option_element.isChecked = get();
			applySettingCallbacks.Add(delegate
			{
				set(option_element.isChecked);
			});
			options.Add(option_element);
			return option_element;
		}

		public override bool readyToClose()
		{
			return false;
		}

		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			currentlySnappedComponent = getComponentWithID(ID_okButton);
			snapCursorToCurrentSnappedComponent();
		}

		public override void applyMovementKey(int direction)
		{
			if (IsDropdownActive())
			{
				if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count && options[currentItemIndex + optionsSlotHeld] is OptionsDropDown && direction == 2)
				{
				}
			}
			else
			{
				base.applyMovementKey(direction);
			}
		}

		private void setScrollBarToCurrentIndex()
		{
			if (options.Count > 0)
			{
				scrollBar.bounds.Y = scrollBarBounds.Y + scrollBarBounds.Height / Math.Max(1, options.Count - 7) * currentItemIndex;
				if (currentItemIndex == options.Count - 7)
				{
					scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
				}
			}
		}

		public override void snapCursorToCurrentSnappedComponent()
		{
			if (currentlySnappedComponent != null && currentlySnappedComponent.myID < options.Count)
			{
				OptionsDropDown dropdown;
				if ((dropdown = (options[currentlySnappedComponent.myID + currentItemIndex] as OptionsDropDown)) != null)
				{
					Game1.setMousePosition(currentlySnappedComponent.bounds.Left + dropdown.bounds.Right - 32, currentlySnappedComponent.bounds.Center.Y - 4);
				}
				else if (options[currentlySnappedComponent.myID + currentItemIndex] is OptionsPlusMinusButton)
				{
					Game1.setMousePosition(currentlySnappedComponent.bounds.Left + 64, currentlySnappedComponent.bounds.Center.Y + 4);
				}
				else if (options[currentlySnappedComponent.myID + currentItemIndex] is OptionsInputListener)
				{
					Game1.setMousePosition(currentlySnappedComponent.bounds.Right - 48, currentlySnappedComponent.bounds.Center.Y - 12);
				}
				else
				{
					Game1.setMousePosition(currentlySnappedComponent.bounds.Left + 48, currentlySnappedComponent.bounds.Center.Y - 12);
				}
			}
			else if (currentlySnappedComponent != null)
			{
				base.snapCursorToCurrentSnappedComponent();
			}
		}

		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
		}

		public virtual void SetScrollFromY(int y)
		{
			int y2 = scrollBar.bounds.Y;
			float percentage = (float)(y - scrollBarBounds.Y) / (float)scrollBarBounds.Height;
			currentItemIndex = (int)Utility.Lerp(t: Utility.Clamp(percentage, 0f, 1f), a: 0f, b: options.Count - 7);
			setScrollBarToCurrentIndex();
			if (y2 != scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4");
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
				else if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count)
				{
					options[currentItemIndex + optionsSlotHeld].leftClickHeld(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
				}
			}
		}

		public override ClickableComponent getCurrentlySnappedComponent()
		{
			return currentlySnappedComponent;
		}

		public override void setCurrentlySnappedComponentTo(int id)
		{
			currentlySnappedComponent = getComponentWithID(id);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveKeyPress(Keys key)
		{
			if ((optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count) || (Game1.options.snappyMenus && Game1.options.gamepadControls))
			{
				if (currentlySnappedComponent != null && Game1.options.snappyMenus && Game1.options.gamepadControls && options.Count > currentItemIndex + currentlySnappedComponent.myID && currentItemIndex + currentlySnappedComponent.myID >= 0)
				{
					options[currentItemIndex + currentlySnappedComponent.myID].receiveKeyPress(key);
				}
				else if (options.Count > currentItemIndex + optionsSlotHeld && currentItemIndex + optionsSlotHeld >= 0)
				{
					options[currentItemIndex + optionsSlotHeld].receiveKeyPress(key);
				}
			}
			base.receiveKeyPress(key);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (!GameMenu.forcePreventClose && !IsDropdownActive())
			{
				base.receiveScrollWheelAction(direction);
				if (direction > 0 && currentItemIndex > 0)
				{
					upArrowPressed();
					Game1.playSound("shiny4");
				}
				else if (direction < 0 && currentItemIndex < Math.Max(0, options.Count - 7))
				{
					downArrowPressed();
					Game1.playSound("shiny4");
				}
				if (Game1.options.SnappyMenus)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!GameMenu.forcePreventClose)
			{
				base.releaseLeftClick(x, y);
				if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count)
				{
					options[currentItemIndex + optionsSlotHeld].leftClickReleased(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
				}
				optionsSlotHeld = -1;
				scrolling = false;
			}
		}

		public bool IsDropdownActive()
		{
			if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count && options[currentItemIndex + optionsSlotHeld] is OptionsDropDown)
			{
				return true;
			}
			return false;
		}

		private void downArrowPressed()
		{
			if (!IsDropdownActive())
			{
				downArrow.scale = downArrow.baseScale;
				currentItemIndex++;
				UnsubscribeFromSelectedTextbox();
				setScrollBarToCurrentIndex();
			}
		}

		public virtual void UnsubscribeFromSelectedTextbox()
		{
			if (Game1.keyboardDispatcher.Subscriber != null)
			{
				foreach (OptionsElement option in options)
				{
					if (option is OptionsTextEntry && Game1.keyboardDispatcher.Subscriber == (option as OptionsTextEntry).textBox)
					{
						Game1.keyboardDispatcher.Subscriber = null;
						break;
					}
				}
			}
		}

		public void preWindowSizeChange()
		{
			_lastSelectedIndex = ((getCurrentlySnappedComponent() != null) ? getCurrentlySnappedComponent().myID : (-1));
			_lastCurrentItemIndex = currentItemIndex;
		}

		public void postWindowSizeChange()
		{
			if (Game1.options.SnappyMenus)
			{
				Game1.activeClickableMenu.setCurrentlySnappedComponentTo(_lastSelectedIndex);
			}
			currentItemIndex = _lastCurrentItemIndex;
			setScrollBarToCurrentIndex();
		}

		private void upArrowPressed()
		{
			if (!IsDropdownActive())
			{
				upArrow.scale = upArrow.baseScale;
				currentItemIndex--;
				UnsubscribeFromSelectedTextbox();
				setScrollBarToCurrentIndex();
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, options.Count - 7))
			{
				downArrowPressed();
				Game1.playSound("shwip");
			}
			else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
			{
				upArrowPressed();
				Game1.playSound("shwip");
			}
			else if (scrollBar.containsPoint(x, y))
			{
				scrolling = true;
			}
			else if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
			{
				scrolling = true;
				leftClickHeld(x, y);
				releaseLeftClick(x, y);
			}
			currentItemIndex = Math.Max(0, Math.Min(options.Count - 7, currentItemIndex));
			if (okButton.containsPoint(x, y))
			{
				CloseAndApply();
				return;
			}
			UnsubscribeFromSelectedTextbox();
			int i = 0;
			while (true)
			{
				if (i < optionSlots.Count)
				{
					if (optionSlots[i].bounds.Contains(x, y) && currentItemIndex + i < options.Count && options[currentItemIndex + i].bounds.Contains(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y))
					{
						break;
					}
					i++;
					continue;
				}
				return;
			}
			options[currentItemIndex + i].receiveLeftClick(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y);
			optionsSlotHeld = i;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			okButton.tryHover(x, y);
			for (int i = 0; i < optionSlots.Count; i++)
			{
				if (currentItemIndex >= 0 && currentItemIndex + i < options.Count && options[currentItemIndex + i].bounds.Contains(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y))
				{
					Game1.SetFreeCursorDrag();
					break;
				}
			}
			if (scrollBarBounds.Contains(x, y))
			{
				Game1.SetFreeCursorDrag();
			}
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			hoverText = "";
			int hovered_index = -1;
			if (!IsDropdownActive())
			{
				for (int j = 0; j < optionSlots.Count; j++)
				{
					if (optionSlots[j].containsPoint(x, y) && j + currentItemIndex < options.Count && hoverText == "")
					{
						hovered_index = j + currentItemIndex;
					}
				}
			}
			if (_lastHoveredIndex != hovered_index)
			{
				_lastHoveredIndex = hovered_index;
				_hoverDuration = 0;
			}
			else
			{
				_hoverDuration += (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			}
			if (_lastHoveredIndex >= 0 && _hoverDuration >= 500)
			{
				OptionsElement option = options[_lastHoveredIndex];
				if (tooltips.ContainsKey(option))
				{
					hoverText = tooltips[option];
				}
			}
			upArrow.tryHover(x, y);
			downArrow.tryHover(x, y);
			scrollBar.tryHover(x, y);
			_ = scrolling;
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black * 0.75f);
			Game1.DrawBox(xPositionOnScreen, yPositionOnScreen, width, height);
			okButton.draw(b);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			for (int i = 0; i < optionSlots.Count; i++)
			{
				if (currentItemIndex >= 0 && currentItemIndex + i < options.Count)
				{
					options[currentItemIndex + i].draw(b, optionSlots[i].bounds.X, optionSlots[i].bounds.Y, this);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			if (options.Count > 7)
			{
				upArrow.draw(b);
				downArrow.draw(b);
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarBounds.X, scrollBarBounds.Y, scrollBarBounds.Width, scrollBarBounds.Height, Color.White, 4f, drawShadow: false);
				scrollBar.draw(b);
			}
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
			drawMouse(b);
		}
	}
}
