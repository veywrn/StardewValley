using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace StardewValley.Menus
{
	public class OptionsPage : IClickableMenu
	{
		public const int itemsPerPage = 7;

		private string descriptionText = "";

		private string hoverText = "";

		public List<ClickableComponent> optionSlots = new List<ClickableComponent>();

		public int currentItemIndex;

		private ClickableTextureComponent upArrow;

		private ClickableTextureComponent downArrow;

		private ClickableTextureComponent scrollBar;

		private bool scrolling;

		public List<OptionsElement> options = new List<OptionsElement>();

		private Rectangle scrollBarRunner;

		protected static int _lastSelectedIndex;

		protected static int _lastCurrentItemIndex;

		public int lastRebindTick = -1;

		private int optionsSlotHeld = -1;

		public OptionsPage(int x, int y, int width, int height)
			: base(x, y, width, height)
		{
			upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
			downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
			scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
			scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 128 - upArrow.bounds.Height - 8);
			for (int i = 0; i < 7; i++)
			{
				optionSlots.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 80 + 4 + i * ((height - 128) / 7), width - 32, (height - 128) / 7 + 4), string.Concat(i))
				{
					myID = i,
					downNeighborID = ((i < 6) ? (i + 1) : (-7777)),
					upNeighborID = ((i > 0) ? (i - 1) : (-7777)),
					fullyImmutable = true
				});
			}
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11233")));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11234"), 0));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11235"), 7));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11236"), 8));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11237"), 11));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11238"), 12));
			options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:Options_GamepadMode"), 38));
			options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:Options_StowingMode"), 28));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11239"), 27));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11240"), 14));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_GamepadStyleMenus"), 29));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_ShowAdvancedCraftingInformation"), 34));
			if (Game1.multiplayerMode == 2)
			{
				options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:OptionsPage_MultiplayerSection")));
				options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode"), 31));
				options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:OptionsPage_IPConnections"), 30));
				options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:OptionsPage_FarmhandCreation"), 32));
				if (Program.sdk.Networking != null)
				{
					options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:GameMenu_ServerInvite"), delegate
					{
						offerInvite();
					}));
					if (Program.sdk.Networking.SupportsInviteCodes())
					{
						options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_ShowInviteCode"), delegate
						{
							showInviteCode();
						}));
					}
				}
			}
			if (Game1.IsMultiplayer)
			{
				options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:OptionsPage_ShowReadyStatus"), 35));
			}
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11241")));
			options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11242"), 1));
			options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11243"), 2));
			options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11244"), 20));
			options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11245"), 21));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11246"), 3));
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11247")));
			if (!Game1.conventionMode)
			{
				options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11248"), 13));
				options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11251"), 6));
			}
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_Vsync"), 37));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11252"), 9));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11253"), 15));
			options.Add(new OptionsPlusMinus(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11254"), 18, new List<string>
			{
				"75%",
				"80%",
				"85%",
				"90%",
				"95%",
				"100%",
				"105%",
				"110%",
				"115%",
				"120%",
				"125%"
			}, new List<string>
			{
				"75%",
				"80%",
				"85%",
				"90%",
				"95%",
				"100%",
				"105%",
				"110%",
				"115%",
				"120%",
				"125%"
			}));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11266"), 19));
			options.Add(new OptionsPlusMinus(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11267"), 25, new List<string>
			{
				"Low",
				"Med.",
				"High"
			}, new List<string>
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11268"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11269"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11270")
			}));
			options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11271"), 23));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11272"), 24));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11273"), 26));
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11274")));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11275"), 16));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11276"), 22));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11277"), -1, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11278"), 7, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11279"), 10, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11280"), 15, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11281"), 18, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11282"), 19, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11283"), 11, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11284"), 14, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11285"), 13, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11286"), 12, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11287"), 17, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\UI:Input_EmoteButton"), 33, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11288"), 16, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.toolbarSwap"), 32, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11289"), 20, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11290"), 21, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11291"), 22, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11292"), 23, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11293"), 24, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11294"), 25, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11295"), 26, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11296"), 27, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11297"), 28, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11298"), 29, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11299"), 30, optionSlots[0].bounds.Width));
			options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11300"), 31, optionSlots[0].bounds.Width));
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:OptionsPage_ScreenshotHeader")));
			options.Add(new OptionsPlusMinusButton(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11254"), 36, new List<string>
			{
				"25%",
				"50%",
				"75%",
				"100%"
			}, new List<string>
			{
				"25%",
				"50%",
				"75%",
				"100%"
			}, Game1.mouseCursors2, new Rectangle(72, 31, 18, 16), delegate(string selection)
			{
				Game1.flashAlpha = 1f;
				selection = selection.Substring(0, selection.Length - 1);
				int result = 25;
				if (!int.TryParse(selection, out result))
				{
					result = 25;
				}
				string text = Game1.game1.takeMapScreenshot((float)result / 100f);
				if (text != null)
				{
					Game1.addHUDMessage(new HUDMessage(text, 6));
				}
				Game1.playSound("cameraNoise");
			}));
			string screenshot_folder = "Screenshots";
			Environment.SpecialFolder screenshot_folder_root = (Environment.OSVersion.Platform != PlatformID.Unix) ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.LocalApplicationData;
			string main_folder = Path.Combine(Path.Combine(Environment.GetFolderPath(screenshot_folder_root), "StardewValley"), screenshot_folder);
			if (Directory.Exists(main_folder))
			{
				options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_OpenFolder"), delegate
				{
					try
					{
						Process.Start(new ProcessStartInfo
						{
							FileName = main_folder,
							UseShellExecute = true,
							Verb = "open"
						});
					}
					catch (Exception)
					{
					}
				}));
			}
		}

		public override bool readyToClose()
		{
			if (lastRebindTick == Game1.ticks)
			{
				return false;
			}
			return base.readyToClose();
		}

		private void waitForServerConnection(Action onConnection)
		{
			if (Game1.server != null)
			{
				if (Game1.server.connected())
				{
					onConnection();
					return;
				}
				IClickableMenu thisMenu = Game1.activeClickableMenu;
				ConfirmationDialog.behavior onClose = delegate
				{
					Game1.activeClickableMenu = thisMenu;
					thisMenu.snapCursorToCurrentSnappedComponent();
				};
				Game1.activeClickableMenu = new ServerConnectionDialog(delegate(Farmer who)
				{
					onClose(who);
					onConnection();
				}, onClose);
			}
		}

		private void offerInvite()
		{
			waitForServerConnection(delegate
			{
				Game1.server.offerInvite();
			});
		}

		private void showInviteCode()
		{
			IClickableMenu thisMenu = Game1.activeClickableMenu;
			waitForServerConnection(delegate
			{
				ConfirmationDialog.behavior onClose = delegate
				{
					Game1.activeClickableMenu = thisMenu;
					thisMenu.snapCursorToCurrentSnappedComponent();
				};
				Game1.activeClickableMenu = new InviteCodeDialog(Game1.server.getInviteCode(), onClose);
			});
		}

		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			currentlySnappedComponent = getComponentWithID(1);
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

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			base.customSnapBehavior(direction, oldRegion, oldID);
			if (oldID == 6 && direction == 2 && currentItemIndex < Math.Max(0, options.Count - 7))
			{
				downArrowPressed();
				Game1.playSound("shiny4");
			}
			else
			{
				if (oldID != 0 || direction != 0)
				{
					return;
				}
				if (currentItemIndex > 0)
				{
					upArrowPressed();
					Game1.playSound("shiny4");
					return;
				}
				currentlySnappedComponent = getComponentWithID(12346);
				if (currentlySnappedComponent != null)
				{
					currentlySnappedComponent.downNeighborID = 0;
				}
				snapCursorToCurrentSnappedComponent();
			}
		}

		private void setScrollBarToCurrentIndex()
		{
			if (options.Count > 0)
			{
				scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, options.Count - 7 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
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

		public override void leftClickHeld(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.leftClickHeld(x, y);
			if (scrolling)
			{
				int y2 = scrollBar.bounds.Y;
				scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
				float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
				currentItemIndex = Math.Min(options.Count - 7, Math.Max(0, (int)((float)options.Count * percentage)));
				setScrollBarToCurrentIndex();
				if (y2 != scrollBar.bounds.Y)
				{
					Game1.playSound("shiny4");
				}
			}
			else if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count)
			{
				options[currentItemIndex + optionsSlotHeld].leftClickHeld(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
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
				setScrollBarToCurrentIndex();
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
			for (int i = 0; i < optionSlots.Count; i++)
			{
				if (currentItemIndex >= 0 && currentItemIndex + i < options.Count && options[currentItemIndex + i].bounds.Contains(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y))
				{
					Game1.SetFreeCursorDrag();
					break;
				}
			}
			if (scrollBarRunner.Contains(x, y))
			{
				Game1.SetFreeCursorDrag();
			}
			if (!GameMenu.forcePreventClose)
			{
				descriptionText = "";
				hoverText = "";
				upArrow.tryHover(x, y);
				downArrow.tryHover(x, y);
				scrollBar.tryHover(x, y);
				_ = scrolling;
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
			for (int i = 0; i < optionSlots.Count; i++)
			{
				if (currentItemIndex >= 0 && currentItemIndex + i < options.Count)
				{
					options[currentItemIndex + i].draw(b, optionSlots[i].bounds.X, optionSlots[i].bounds.Y);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			if (!GameMenu.forcePreventClose)
			{
				upArrow.draw(b);
				downArrow.draw(b);
				if (options.Count > 7)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
					scrollBar.draw(b);
				}
			}
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
		}
	}
}
