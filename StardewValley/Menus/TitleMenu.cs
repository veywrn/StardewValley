using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StardewValley.Menus
{
	public class TitleMenu : IClickableMenu, IDisposable
	{
		public const int region_muteMusic = 81111;

		public const int region_windowedButton = 81112;

		public const int region_aboutButton = 81113;

		public const int region_backButton = 81114;

		public const int region_newButton = 81115;

		public const int region_loadButton = 81116;

		public const int region_coopButton = 81119;

		public const int region_exitButton = 81117;

		public const int region_languagesButton = 81118;

		public const int fadeFromWhiteDuration = 2000;

		public const int viewportFinalPosition = -1000;

		public const int logoSwipeDuration = 1000;

		public const int numberOfButtons = 4;

		public const int spaceBetweenButtons = 8;

		public const float bigCloudDX = 0.1f;

		public const float mediumCloudDX = 0.2f;

		public const float smallCloudDX = 0.3f;

		public const float bgmountainsParallaxSpeed = 0.66f;

		public const float mountainsParallaxSpeed = 1f;

		public const float foregroundJungleParallaxSpeed = 2f;

		public const float cloudsParallaxSpeed = 0.5f;

		public const int pixelZoom = 3;

		public const string titleButtonsTextureName = "Minigames\\TitleButtons";

		public LocalizedContentManager menuContent = Game1.content.CreateTemporary();

		public Texture2D cloudsTexture;

		public Texture2D titleButtonsTexture;

		private List<float> bigClouds = new List<float>();

		private List<float> smallClouds = new List<float>();

		private List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();

		public List<ClickableTextureComponent> buttons = new List<ClickableTextureComponent>();

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent muteMusicButton;

		public ClickableTextureComponent aboutButton;

		public ClickableTextureComponent languageButton;

		public ClickableTextureComponent windowedButton;

		public ClickableComponent skipButton;

		protected bool _movedCursor;

		public List<TemporaryAnimatedSprite> birds = new List<TemporaryAnimatedSprite>();

		private Rectangle eRect;

		private Rectangle screwRect;

		private List<Rectangle> leafRects;

		private static IClickableMenu _subMenu;

		private readonly StartupPreferences startupPreferences;

		private int globalXOffset;

		private float viewportY;

		private float viewportDY;

		private float logoSwipeTimer;

		private float globalCloudAlpha = 1f;

		private int numFarmsSaved = -1;

		private int fadeFromWhiteTimer;

		private int pauseBeforeViewportRiseTimer;

		private int buttonsToShow;

		private int showButtonsTimer;

		private int logoFadeTimer;

		private int logoSurprisedTimer;

		private int clicksOnE;

		private int clicksOnLeaf;

		private int clicksOnScrew;

		private int buttonsDX;

		private int chuckleFishTimer;

		private bool titleInPosition;

		private bool isTransitioningButtons;

		private bool shades;

		private bool transitioningCharacterCreationMenu;

		private static int windowNumber = 3;

		public string startupMessage = "";

		public Color startupMessageColor = Color.DeepSkyBlue;

		private int bCount;

		private string whichSubMenu = "";

		private int quitTimer;

		private bool transitioningFromLoadScreen;

		private bool disposedValue;

		public static IClickableMenu subMenu
		{
			get
			{
				return _subMenu;
			}
			set
			{
				if (_subMenu != null)
				{
					if (_subMenu.exitFunction != null)
					{
						_subMenu.exitFunction = null;
					}
					if (_subMenu is IDisposable)
					{
						(_subMenu as IDisposable).Dispose();
					}
				}
				_subMenu = value;
				if (_subMenu != null)
				{
					if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is TitleMenu)
					{
						IClickableMenu subMenu = _subMenu;
						subMenu.exitFunction = (onExit)Delegate.Combine(subMenu.exitFunction, new onExit((Game1.activeClickableMenu as TitleMenu).CloseSubMenu));
					}
					if (Game1.options.snappyMenus && Game1.options.gamepadControls)
					{
						_subMenu.snapToDefaultClickableComponent();
					}
				}
			}
		}

		private bool HasActiveUser => true;

		public TitleMenu()
			: base(0, 0, Game1.viewport.Width, Game1.viewport.Height)
		{
			LocalizedContentManager.OnLanguageChange += OnLanguageChange;
			cloudsTexture = menuContent.Load<Texture2D>(Path.Combine("Minigames", "Clouds"));
			titleButtonsTexture = menuContent.Load<Texture2D>("Minigames\\TitleButtons");
			viewportY = 0f;
			fadeFromWhiteTimer = 4000;
			logoFadeTimer = 5000;
			bigClouds.Add(-750f);
			bigClouds.Add(width * 3 / 4);
			shades = (Game1.random.NextDouble() < 0.5);
			smallClouds.Add(width - 1);
			smallClouds.Add(width - 1 + 690);
			smallClouds.Add(width * 2 / 3);
			smallClouds.Add(width / 8);
			smallClouds.Add(width - 1 + 1290);
			smallClouds.Add(width * 3 / 4);
			smallClouds.Add(1f);
			smallClouds.Add(width / 2 + 450);
			smallClouds.Add(width - 1 + 1890);
			smallClouds.Add(width - 1 + 390);
			smallClouds.Add(width / 3 + 570);
			smallClouds.Add(301f);
			smallClouds.Add(width / 2 + 2490);
			smallClouds.Add(width * 2 / 3 + 360);
			smallClouds.Add(width * 3 / 4 + 510);
			smallClouds.Add(width / 4 + 660);
			for (int i = 0; i < smallClouds.Count; i++)
			{
				smallClouds[i] += Game1.random.Next(400);
			}
			birds.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 227, 26, 21), new Vector2(width - 210, height - 390), flipped: false, 0f, Color.White)
			{
				scale = 3f,
				pingPong = true,
				animationLength = 4,
				interval = 100f,
				totalNumberOfLoops = 9999,
				local = true,
				motion = new Vector2(-1f, 0f),
				layerDepth = 0.25f
			});
			birds.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 227, 26, 21), new Vector2(width - 120, height - 360), flipped: false, 0f, Color.White)
			{
				scale = 3f,
				pingPong = true,
				animationLength = 4,
				interval = 100f,
				totalNumberOfLoops = 9999,
				local = true,
				delayBeforeAnimationStart = 100,
				motion = new Vector2(-1f, 0f),
				layerDepth = 0.25f
			});
			setUpIcons();
			muteMusicButton = new ClickableTextureComponent(new Rectangle(16, 16, 36, 36), Game1.mouseCursors, new Rectangle(128, 384, 9, 9), 4f)
			{
				myID = 81111,
				downNeighborID = 81115,
				rightNeighborID = 81112
			};
			windowedButton = new ClickableTextureComponent(new Rectangle(Game1.viewport.Width - 36 - 16, 16, 36, 36), Game1.mouseCursors, new Rectangle((Game1.options != null && !Game1.options.isCurrentlyWindowed()) ? 155 : 146, 384, 9, 9), 4f)
			{
				myID = 81112,
				leftNeighborID = 81111,
				downNeighborID = 81113
			};
			startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false);
			applyPreferences();
			switch (startupPreferences.timesPlayed)
			{
			case 3:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11718");
				break;
			case 5:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11720");
				break;
			case 7:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11722");
				break;
			case 2:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11717");
				break;
			case 4:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11719");
				break;
			case 6:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11721");
				break;
			case 8:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11723");
				break;
			case 9:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11724");
				break;
			case 10:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11725");
				break;
			case 15:
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
				{
					string noun3 = Dialogue.getRandomNoun();
					string noun2 = Dialogue.getRandomNoun();
					startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11726") + Environment.NewLine + "The " + Dialogue.getRandomAdjective() + " " + noun3 + " " + Dialogue.getRandomVerb() + " " + Dialogue.getRandomPositional() + " the " + (noun3.Equals(noun2) ? ("other " + noun2) : noun2);
				}
				else
				{
					int randSentence = new Random().Next(1, 15);
					startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:RandomSentence." + randSentence);
				}
				break;
			case 20:
				startupMessage = "<";
				break;
			case 30:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11731");
				break;
			case 100:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11732");
				break;
			case 1000:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11733");
				break;
			case 10000:
				startupMessage = menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11734");
				break;
			}
			startupPreferences.savePreferences(async: false);
			Game1.setRichPresence("menus");
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		private bool alternativeTitleGraphic()
		{
			return LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh;
		}

		public void applyPreferences()
		{
			if (startupPreferences.playerLimit > 0)
			{
				Game1.multiplayer.playerLimit = startupPreferences.playerLimit;
			}
			if (startupPreferences.startMuted)
			{
				if (Utility.toggleMuteMusic())
				{
					muteMusicButton.sourceRect.X = 137;
				}
				else
				{
					muteMusicButton.sourceRect.X = 128;
				}
			}
			if (startupPreferences.skipWindowPreparation && windowNumber == 3)
			{
				windowNumber = -1;
			}
			if (startupPreferences.windowMode == 2 && startupPreferences.fullscreenResolutionX != 0 && startupPreferences.fullscreenResolutionY != 0)
			{
				Game1.options.preferredResolutionX = startupPreferences.fullscreenResolutionX;
				Game1.options.preferredResolutionY = startupPreferences.fullscreenResolutionY;
			}
			Game1.options.gamepadMode = startupPreferences.gamepadMode;
			Game1.game1.CheckGamepadMode();
			if (Game1.options.gamepadControls && Game1.options.snappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		private void OnLanguageChange(LocalizedContentManager.LanguageCode code)
		{
			titleButtonsTexture = menuContent.Load<Texture2D>(Path.Combine("Minigames", "TitleButtons"));
			setUpIcons();
			tempSprites.Clear();
			startupPreferences.OnLanguageChange(code);
		}

		public void skipToTitleButtons()
		{
			logoFadeTimer = 0;
			logoSwipeTimer = 0f;
			titleInPosition = false;
			pauseBeforeViewportRiseTimer = 0;
			fadeFromWhiteTimer = 0;
			viewportY = -999f;
			viewportDY = -0.01f;
			birds.Clear();
			logoSwipeTimer = 1f;
			chuckleFishTimer = 0;
			Game1.changeMusicTrack("MainTheme");
			if (Game1.options.SnappyMenus && Game1.options.gamepadControls)
			{
				snapToDefaultClickableComponent();
			}
		}

		public void setUpIcons()
		{
			buttons.Clear();
			int buttonWidth = 74;
			int mainButtonSetWidth2 = buttonWidth * 4 * 3;
			mainButtonSetWidth2 += 72;
			int curx4 = width / 2 - mainButtonSetWidth2 / 2;
			buttons.Add(new ClickableTextureComponent("New", new Rectangle(curx4, height - 174 - 24, buttonWidth * 3, 174), null, "", titleButtonsTexture, new Rectangle(0, 187, 74, 58), 3f)
			{
				myID = 81115,
				rightNeighborID = 81116,
				upNeighborID = 81111
			});
			curx4 += (buttonWidth + 8) * 3;
			buttons.Add(new ClickableTextureComponent("Load", new Rectangle(curx4, height - 174 - 24, 222, 174), null, "", titleButtonsTexture, new Rectangle(74, 187, 74, 58), 3f)
			{
				myID = 81116,
				leftNeighborID = 81115,
				rightNeighborID = -7777,
				upNeighborID = 81111
			});
			curx4 += (buttonWidth + 8) * 3;
			buttons.Add(new ClickableTextureComponent("Co-op", new Rectangle(curx4, height - 174 - 24, 222, 174), null, "", titleButtonsTexture, new Rectangle(148, 187, 74, 58), 3f)
			{
				myID = 81119,
				leftNeighborID = 81116,
				rightNeighborID = 81117
			});
			curx4 += (buttonWidth + 8) * 3;
			buttons.Add(new ClickableTextureComponent("Exit", new Rectangle(curx4, height - 174 - 24, 222, 174), null, "", titleButtonsTexture, new Rectangle(222, 187, 74, 58), 3f)
			{
				myID = 81117,
				leftNeighborID = 81119,
				rightNeighborID = 81118,
				upNeighborID = 81111
			});
			int zoom = (height < 800) ? 2 : 3;
			eRect = new Rectangle(width / 2 - 200 * zoom + 251 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 26 * zoom, 42 * zoom, 68 * zoom);
			screwRect = new Rectangle(width / 2 + 150 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 80 * zoom, 5 * zoom, 5 * zoom);
			populateLeafRects();
			backButton = new ClickableTextureComponent(menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11739"), new Rectangle(width + -198 - 48, height - 81 - 24, 198, 81), null, "", titleButtonsTexture, new Rectangle(296, 252, 66, 27), 3f)
			{
				myID = 81114
			};
			aboutButton = new ClickableTextureComponent(menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11740"), new Rectangle(width + -66 - 48, height - 75 - 24, 66, 75), null, "", titleButtonsTexture, new Rectangle(8, 458, 22, 25), 3f)
			{
				myID = 81113,
				upNeighborID = 81118,
				leftNeighborID = -7777
			};
			languageButton = new ClickableTextureComponent(menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11740"), new Rectangle(width + -66 - 48, height - 150 - 48, 81, 75), null, "", titleButtonsTexture, new Rectangle(52, 458, 27, 25), 3f)
			{
				myID = 81118,
				downNeighborID = 81113,
				leftNeighborID = -7777,
				upNeighborID = 81112
			};
			skipButton = new ClickableComponent(new Rectangle(width / 2 - 261, height / 2 - 102, 249, 201), menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11741"));
			if (Game1.options.gamepadControls && Game1.options.snappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (subMenu != null)
			{
				subMenu.snapToDefaultClickableComponent();
				return;
			}
			currentlySnappedComponent = getComponentWithID((startupPreferences != null && startupPreferences.timesPlayed > 0) ? 81116 : 81115);
			snapCursorToCurrentSnappedComponent();
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID == 81116 && direction == 1)
			{
				if (getComponentWithID(81119) != null)
				{
					setCurrentlySnappedComponentTo(81119);
					snapCursorToCurrentSnappedComponent();
				}
				else if (getComponentWithID(81117) != null)
				{
					setCurrentlySnappedComponentTo(81117);
					snapCursorToCurrentSnappedComponent();
				}
				else
				{
					setCurrentlySnappedComponentTo(81118);
					snapCursorToCurrentSnappedComponent();
				}
			}
			else if ((oldID == 81118 || oldID == 81113) && direction == 3)
			{
				if (getComponentWithID(81117) != null)
				{
					setCurrentlySnappedComponentTo(81117);
					snapCursorToCurrentSnappedComponent();
				}
				else
				{
					setCurrentlySnappedComponentTo(81116);
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public void populateLeafRects()
		{
			int zoom = (height < 800) ? 2 : 3;
			leafRects = new List<Rectangle>();
			leafRects.Add(new Rectangle(width / 2 - 200 * zoom + 251 * zoom - 196 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 26 * zoom + 109 * zoom, 17 * zoom, 30 * zoom));
			leafRects.Add(new Rectangle(width / 2 - 200 * zoom + 251 * zoom + 91 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 26 * zoom - 26 * zoom, 17 * zoom, 31 * zoom));
			leafRects.Add(new Rectangle(width / 2 - 200 * zoom + 251 * zoom + 79 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 26 * zoom + 83 * zoom, 25 * zoom, 17 * zoom));
			leafRects.Add(new Rectangle(width / 2 - 200 * zoom + 251 * zoom - 213 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 26 * zoom - 24 * zoom, 14 * zoom, 23 * zoom));
			leafRects.Add(new Rectangle(width / 2 - 200 * zoom + 251 * zoom - 234 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 26 * zoom - 11 * zoom, 18 * zoom, 12 * zoom));
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (ShouldAllowInteraction() && !transitioningCharacterCreationMenu && subMenu != null)
			{
				subMenu.receiveRightClick(x, y);
			}
		}

		public override bool readyToClose()
		{
			return false;
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return !titleInPosition;
		}

		public override void leftClickHeld(int x, int y)
		{
			if (!transitioningCharacterCreationMenu)
			{
				base.leftClickHeld(x, y);
				if (subMenu != null)
				{
					subMenu.leftClickHeld(x, y);
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!transitioningCharacterCreationMenu)
			{
				base.releaseLeftClick(x, y);
				if (subMenu != null)
				{
					subMenu.releaseLeftClick(x, y);
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (transitioningCharacterCreationMenu)
			{
				return;
			}
			if (!Program.releaseBuild && key == Keys.N && Game1.oldKBState.IsKeyDown(Keys.RightShift) && Game1.oldKBState.IsKeyDown(Keys.LeftControl))
			{
				string season = "spring";
				if (Game1.oldKBState.IsKeyDown(Keys.C))
				{
					Game1.whichFarm = Game1.random.Next(6);
					season = (Game1.currentSeason = Utility.getSeasonNameFromNumber(Game1.random.Next(4)).ToLower());
				}
				Game1.loadForNewGame();
				Game1.saveOnNewDay = false;
				Game1.player.eventsSeen.Add(60367);
				Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
				Game1.player.Position = new Vector2(9f, 9f) * 64f;
				Game1.player.FarmerSprite.setOwner(Game1.player);
				Game1.player.isInBed.Value = true;
				if (Game1.oldKBState.IsKeyDown(Keys.C))
				{
					Game1.currentSeason = season;
					Game1.setGraphicsForSeason();
				}
				Game1.NewDay(0f);
				Game1.exitActiveMenu();
				Game1.setGameMode(3);
				return;
			}
			if (logoFadeTimer > 0 && (key == Keys.B || key == Keys.Escape))
			{
				bCount++;
				if (key == Keys.Escape)
				{
					bCount += 3;
				}
				if (bCount >= 3)
				{
					Game1.playSound("bigDeSelect");
					logoFadeTimer = 0;
					fadeFromWhiteTimer = 0;
					Game1.delayedActions.Clear();
					Game1.morningSongPlayAction = null;
					pauseBeforeViewportRiseTimer = 0;
					fadeFromWhiteTimer = 0;
					viewportY = -999f;
					viewportDY = -0.01f;
					birds.Clear();
					logoSwipeTimer = 1f;
					chuckleFishTimer = 0;
					Game1.changeMusicTrack("MainTheme");
				}
			}
			if (!Game1.options.doesInputListContain(Game1.options.menuButton, key) && ShouldAllowInteraction())
			{
				if (subMenu != null)
				{
					subMenu.receiveKeyPress(key);
				}
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && subMenu == null)
				{
					base.receiveKeyPress(key);
				}
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			bool passThrough = true;
			if (subMenu != null)
			{
				if (subMenu is LoadGameMenu && (subMenu as LoadGameMenu).deleteConfirmationScreen)
				{
					passThrough = false;
				}
				if (subMenu is CharacterCustomization && (subMenu as CharacterCustomization).showingCoopHelp)
				{
					passThrough = false;
				}
				subMenu.receiveGamePadButton(b);
			}
			if (passThrough && b == Buttons.B && logoFadeTimer <= 0 && fadeFromWhiteTimer <= 0 && titleInPosition)
			{
				backButtonPressed();
			}
		}

		public override void gamePadButtonHeld(Buttons b)
		{
			if (!Game1.lastCursorMotionWasMouse)
			{
				_movedCursor = true;
			}
			if (subMenu != null)
			{
				subMenu.gamePadButtonHeld(b);
			}
		}

		public void backButtonPressed()
		{
			if (subMenu == null || !subMenu.readyToClose())
			{
				return;
			}
			Game1.playSound("bigDeSelect");
			buttonsDX = -1;
			if (subMenu is AboutMenu)
			{
				subMenu = null;
				buttonsDX = 0;
				if (Game1.options.SnappyMenus)
				{
					setCurrentlySnappedComponentTo(81113);
					snapCursorToCurrentSnappedComponent();
				}
			}
			else
			{
				isTransitioningButtons = true;
				if (subMenu is LoadGameMenu)
				{
					transitioningFromLoadScreen = true;
				}
				subMenu = null;
				Game1.changeMusicTrack("spring_day_ambient");
			}
		}

		protected void CloseSubMenu()
		{
			if (!subMenu.readyToClose())
			{
				return;
			}
			buttonsDX = -1;
			if (subMenu is AboutMenu || subMenu is LanguageSelectionMenu)
			{
				subMenu = null;
				buttonsDX = 0;
				return;
			}
			isTransitioningButtons = true;
			if (subMenu is LoadGameMenu)
			{
				transitioningFromLoadScreen = true;
			}
			subMenu = null;
			Game1.changeMusicTrack("spring_day_ambient");
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (HasActiveUser && muteMusicButton.containsPoint(x, y))
			{
				startupPreferences.startMuted = Utility.toggleMuteMusic();
				if (muteMusicButton.sourceRect.X == 128)
				{
					muteMusicButton.sourceRect.X = 137;
				}
				else
				{
					muteMusicButton.sourceRect.X = 128;
				}
				Game1.playSound("drumkit6");
				startupPreferences.savePreferences(async: false);
				return;
			}
			if (HasActiveUser && windowedButton.containsPoint(x, y))
			{
				if (!Game1.options.isCurrentlyWindowed())
				{
					Game1.options.setWindowedOption("Windowed");
					windowedButton.sourceRect.X = 146;
					startupPreferences.windowMode = 1;
				}
				else
				{
					Game1.options.setWindowedOption("Windowed Borderless");
					windowedButton.sourceRect.X = 155;
					startupPreferences.windowMode = 0;
				}
				startupPreferences.savePreferences(async: false);
				Game1.playSound("drumkit6");
				return;
			}
			if (logoFadeTimer > 0 && skipButton.containsPoint(x, y) && chuckleFishTimer <= 0)
			{
				if (logoSurprisedTimer <= 0)
				{
					logoSurprisedTimer = 1500;
					string soundtoPlay = "fishSlap";
					Game1.changeMusicTrack("none");
					switch (Game1.random.Next(2))
					{
					case 0:
						soundtoPlay = "Duck";
						break;
					case 1:
						soundtoPlay = "fishSlap";
						break;
					}
					Game1.playSound(soundtoPlay);
				}
				else if (logoSurprisedTimer > 1)
				{
					logoSurprisedTimer = Math.Max(1, logoSurprisedTimer - 500);
				}
			}
			if (chuckleFishTimer > 500)
			{
				chuckleFishTimer = 500;
			}
			if (logoFadeTimer > 0 || fadeFromWhiteTimer > 0 || transitioningCharacterCreationMenu)
			{
				return;
			}
			if (subMenu != null)
			{
				if (subMenu.readyToClose() && backButton.containsPoint(x, y))
				{
					subMenu.exitThisMenu();
				}
				else if (!isTransitioningButtons)
				{
					subMenu.receiveLeftClick(x, y);
				}
				return;
			}
			if (logoFadeTimer <= 0 && !titleInPosition && logoSwipeTimer == 0f)
			{
				pauseBeforeViewportRiseTimer = 0;
				fadeFromWhiteTimer = 0;
				viewportY = -999f;
				viewportDY = -0.01f;
				birds.Clear();
				logoSwipeTimer = 1f;
				return;
			}
			if (!alternativeTitleGraphic())
			{
				if (clicksOnLeaf >= 10 && Game1.random.NextDouble() < 0.001)
				{
					Game1.playSound("junimoMeep1");
				}
				if (titleInPosition && eRect.Contains(x, y) && clicksOnE < 10)
				{
					clicksOnE++;
					Game1.playSound("woodyStep");
					if (clicksOnE == 10)
					{
						int zoom3 = (height < 800) ? 2 : 3;
						Game1.playSound("openChest");
						tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(0, 491, 42, 68), new Vector2(width / 2 - 200 * zoom3 + 251 * zoom3, -300 * zoom3 - (int)(viewportY / 3f) * zoom3 + 26 * zoom3), flipped: false, 0f, Color.White)
						{
							scale = zoom3,
							animationLength = 9,
							interval = 200f,
							local = true,
							holdLastFrame = true
						});
					}
				}
				else if (titleInPosition)
				{
					bool clicked = false;
					foreach (Rectangle leafRect in leafRects)
					{
						if (leafRect.Contains(x, y))
						{
							clicked = true;
							break;
						}
					}
					if (screwRect.Contains(x, y) && clicksOnScrew < 10)
					{
						Game1.playSound("cowboy_monsterhit");
						clicksOnScrew++;
						if (clicksOnScrew == 10)
						{
							showButterflies();
						}
					}
					if (clicked)
					{
						clicksOnLeaf++;
						if (clicksOnLeaf == 10)
						{
							int zoom2 = (height < 800) ? 2 : 3;
							Game1.playSound("discoverMineral");
							tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(264, 464, 16, 16), new Vector2(width / 2 - 200 * zoom2 + 80 * zoom2, -300 * zoom2 - (int)(viewportY / 3f) * zoom2 + 10 * zoom2 + 2), flipped: false, 0f, Color.White)
							{
								scale = zoom2,
								animationLength = 8,
								interval = 80f,
								totalNumberOfLoops = 999999,
								local = true,
								holdLastFrame = false,
								delayBeforeAnimationStart = 200
							});
							tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 448, 16, 16), new Vector2(width / 2 - 200 * zoom2 + 80 * zoom2, -300 * zoom2 - (int)(viewportY / 3f) * zoom2 + 10 * zoom2), flipped: false, 0f, Color.White)
							{
								scale = zoom2,
								animationLength = 8,
								interval = 50f,
								local = true,
								holdLastFrame = false
							});
							tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(200, 464, 16, 16), new Vector2(width / 2 - 200 * zoom2 + 178 * zoom2, -300 * zoom2 - (int)(viewportY / 3f) * zoom2 + 141 * zoom2 + 2), flipped: false, 0f, Color.White)
							{
								scale = zoom2,
								animationLength = 4,
								interval = 150f,
								totalNumberOfLoops = 999999,
								local = true,
								holdLastFrame = false,
								delayBeforeAnimationStart = 400
							});
							tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 448, 16, 16), new Vector2(width / 2 - 200 * zoom2 + 178 * zoom2, -300 * zoom2 - (int)(viewportY / 3f) * zoom2 + 141 * zoom2), flipped: false, 0f, Color.White)
							{
								scale = zoom2,
								animationLength = 8,
								interval = 50f,
								local = true,
								holdLastFrame = false,
								delayBeforeAnimationStart = 200
							});
							tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 464, 16, 16), new Vector2(width / 2 - 200 * zoom2 + 294 * zoom2, -300 * zoom2 - (int)(viewportY / 3f) * zoom2 + 89 * zoom2 + 2), flipped: false, 0f, Color.White)
							{
								scale = zoom2,
								animationLength = 4,
								interval = 150f,
								totalNumberOfLoops = 999999,
								local = true,
								holdLastFrame = false,
								delayBeforeAnimationStart = 600
							});
							tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 448, 16, 16), new Vector2(width / 2 - 200 * zoom2 + 294 * zoom2, -300 * zoom2 - (int)(viewportY / 3f) * zoom2 + 89 * zoom2), flipped: false, 0f, Color.White)
							{
								scale = zoom2,
								animationLength = 8,
								interval = 50f,
								local = true,
								holdLastFrame = false,
								delayBeforeAnimationStart = 400
							});
						}
						else
						{
							Game1.playSound("leafrustle");
							int zoom = (height < 800) ? 2 : 3;
							for (int i = 0; i < 2; i++)
							{
								tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1199 + Game1.random.Next(-1, 2) * 16, 16, 16), new Vector2(x + Game1.random.Next(-8, 9), y + Game1.random.Next(-8, 9)), Game1.random.NextDouble() < 0.5, 0f, Color.White)
								{
									scale = zoom,
									animationLength = 11,
									interval = 50 + Game1.random.Next(50),
									totalNumberOfLoops = 999,
									motion = new Vector2((float)Game1.random.Next(-100, 101) / 100f, 1f + (float)Game1.random.Next(-100, 100) / 500f),
									xPeriodic = (Game1.random.NextDouble() < 0.5),
									xPeriodicLoopTime = Game1.random.Next(6000, 16000),
									xPeriodicRange = Game1.random.Next(64, 192),
									alphaFade = 0.001f,
									local = true,
									holdLastFrame = false,
									delayBeforeAnimationStart = i * 20
								});
							}
						}
					}
				}
			}
			if (ShouldAllowInteraction() && HasActiveUser && (subMenu == null || subMenu.readyToClose()) && !isTransitioningButtons)
			{
				foreach (ClickableTextureComponent c in buttons)
				{
					if (c.containsPoint(x, y))
					{
						performButtonAction(c.name);
					}
				}
				if (aboutButton.containsPoint(x, y))
				{
					subMenu = new AboutMenu();
					Game1.playSound("newArtifact");
				}
				if (languageButton.visible && languageButton.containsPoint(x, y))
				{
					subMenu = new LanguageSelectionMenu();
					Game1.playSound("newArtifact");
				}
			}
		}

		public void performButtonAction(string which)
		{
			whichSubMenu = which;
			switch (which)
			{
			default:
				if (which == "Exit")
				{
					Game1.playSound("bigDeSelect");
					Game1.changeMusicTrack("none");
					quitTimer = 500;
				}
				break;
			case "New":
				buttonsDX = 1;
				isTransitioningButtons = true;
				Game1.playSound("select");
				foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
				{
					tempSprite.pingPong = false;
				}
				lock (this)
				{
					numFarmsSaved = -1;
				}
				Game1.GetNumFarmsSavedAsync(delegate(int num)
				{
					lock (this)
					{
						numFarmsSaved = num;
					}
				});
				break;
			case "Load":
			case "Co-op":
			case "Invite":
				buttonsDX = 1;
				isTransitioningButtons = true;
				Game1.playSound("select");
				break;
			}
		}

		private void addRightLeafGust()
		{
			if (!isTransitioningButtons && tempSprites.Count() <= 0 && !alternativeTitleGraphic())
			{
				int zoom = (height < 800) ? 2 : 3;
				tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 187, 27, 21), new Vector2(width / 2 - 200 * zoom + 327 * zoom, (float)(-300 * zoom) - viewportY / 3f * (float)zoom + (float)(107 * zoom)), flipped: false, 0f, Color.White)
				{
					scale = zoom,
					pingPong = true,
					animationLength = 3,
					interval = 100f,
					totalNumberOfLoops = 3,
					local = true
				});
			}
		}

		private void addLeftLeafGust()
		{
			if (!isTransitioningButtons && tempSprites.Count() <= 0 && !alternativeTitleGraphic())
			{
				int zoom = (height < 800) ? 2 : 3;
				tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 208, 22, 18), new Vector2(width / 2 - 200 * zoom + 16 * zoom, (float)(-300 * zoom) - viewportY / 3f * (float)zoom + (float)(16 * zoom)), flipped: false, 0f, Color.White)
				{
					scale = zoom,
					pingPong = true,
					animationLength = 3,
					interval = 100f,
					totalNumberOfLoops = 3,
					local = true
				});
			}
		}

		public void createdNewCharacter(bool skipIntro)
		{
			Game1.playSound("smallSelect");
			subMenu = null;
			transitioningCharacterCreationMenu = true;
			if (skipIntro)
			{
				Game1.loadForNewGame();
				Game1.saveOnNewDay = true;
				Game1.player.eventsSeen.Add(60367);
				Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
				Game1.player.Position = new Vector2(9f, 9f) * 64f;
				Game1.player.isInBed.Value = true;
				Game1.NewDay(0f);
				Game1.exitActiveMenu();
				Game1.setGameMode(3);
			}
		}

		public override void update(GameTime time)
		{
			if (windowNumber > ((startupPreferences.windowMode != 1) ? 1 : 0))
			{
				if (windowNumber % 2 == 0)
				{
					Game1.options.setWindowedOption("Windowed Borderless");
				}
				else
				{
					Game1.options.setWindowedOption("Windowed");
				}
				windowNumber--;
				if (windowNumber == ((startupPreferences.windowMode != 1) ? 1 : 0))
				{
					Game1.options.setWindowedOption(startupPreferences.windowMode);
				}
			}
			base.update(time);
			if (subMenu != null)
			{
				subMenu.update(time);
			}
			if (transitioningCharacterCreationMenu)
			{
				globalCloudAlpha -= (float)time.ElapsedGameTime.Milliseconds * 0.001f;
				if (globalCloudAlpha <= 0f)
				{
					transitioningCharacterCreationMenu = false;
					globalCloudAlpha = 0f;
					subMenu = null;
					Game1.currentMinigame = new GrandpaStory();
					Game1.exitActiveMenu();
					Game1.setGameMode(3);
				}
			}
			if (quitTimer > 0)
			{
				quitTimer -= time.ElapsedGameTime.Milliseconds;
				if (quitTimer <= 0)
				{
					Game1.quit = true;
					Game1.exitActiveMenu();
				}
			}
			if (chuckleFishTimer > 0)
			{
				chuckleFishTimer -= time.ElapsedGameTime.Milliseconds;
			}
			else if (logoFadeTimer > 0)
			{
				if (logoSurprisedTimer > 0)
				{
					logoSurprisedTimer -= time.ElapsedGameTime.Milliseconds;
					if (logoSurprisedTimer <= 0)
					{
						logoFadeTimer = 1;
					}
				}
				else
				{
					int old = logoFadeTimer;
					logoFadeTimer -= time.ElapsedGameTime.Milliseconds;
					if (logoFadeTimer < 4000 && old >= 4000)
					{
						Game1.playSound("mouseClick");
					}
					if (logoFadeTimer < 2500 && old >= 2500)
					{
						Game1.playSound("mouseClick");
					}
					if (logoFadeTimer < 2000 && old >= 2000)
					{
						Game1.playSound("mouseClick");
					}
					if (logoFadeTimer <= 0)
					{
						Game1.changeMusicTrack("MainTheme");
					}
				}
			}
			else if (fadeFromWhiteTimer > 0)
			{
				fadeFromWhiteTimer -= time.ElapsedGameTime.Milliseconds;
				if (fadeFromWhiteTimer <= 0)
				{
					pauseBeforeViewportRiseTimer = 3500;
				}
			}
			else if (pauseBeforeViewportRiseTimer > 0)
			{
				pauseBeforeViewportRiseTimer -= time.ElapsedGameTime.Milliseconds;
				if (pauseBeforeViewportRiseTimer <= 0)
				{
					viewportDY = -0.05f;
				}
			}
			viewportY += viewportDY;
			if (viewportDY < 0f)
			{
				viewportDY -= 0.006f;
			}
			if (viewportY <= -1000f)
			{
				if (viewportDY != 0f)
				{
					logoSwipeTimer = 1000f;
					showButtonsTimer = 200;
				}
				viewportDY = 0f;
			}
			if (logoSwipeTimer > 0f)
			{
				logoSwipeTimer -= time.ElapsedGameTime.Milliseconds;
				if (logoSwipeTimer <= 0f)
				{
					addLeftLeafGust();
					addRightLeafGust();
					titleInPosition = true;
					int zoom = (height < 800) ? 2 : 3;
					eRect = new Rectangle(width / 2 - 200 * zoom + 251 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 26 * zoom, 42 * zoom, 68 * zoom);
					screwRect = new Rectangle(width / 2 + 150 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 80 * zoom, 5 * zoom, 5 * zoom);
					populateLeafRects();
				}
			}
			if (showButtonsTimer > 0 && HasActiveUser && subMenu == null)
			{
				showButtonsTimer -= time.ElapsedGameTime.Milliseconds;
				if (showButtonsTimer <= 0)
				{
					if (buttonsToShow < 4)
					{
						buttonsToShow++;
						Game1.playSound("Cowboy_gunshot");
						showButtonsTimer = 200;
					}
					else if (Game1.options.gamepadControls && Game1.options.snappyMenus)
					{
						populateClickableComponentList();
						snapToDefaultClickableComponent();
					}
				}
			}
			if (titleInPosition && !isTransitioningButtons && globalXOffset == 0 && Game1.random.NextDouble() < 0.005)
			{
				if (Game1.random.NextDouble() < 0.5)
				{
					addLeftLeafGust();
				}
				else
				{
					addRightLeafGust();
				}
			}
			if (titleInPosition && isTransitioningButtons)
			{
				int dx = buttonsDX * (int)time.ElapsedGameTime.TotalMilliseconds;
				int offsetx = globalXOffset + dx;
				int over = offsetx - width;
				if (over > 0)
				{
					offsetx -= over;
					dx -= over;
				}
				globalXOffset = offsetx;
				moveFeatures(dx, 0);
				if (buttonsDX > 0 && globalXOffset >= width)
				{
					if (subMenu != null)
					{
						if (subMenu.readyToClose())
						{
							isTransitioningButtons = false;
							buttonsDX = 0;
						}
					}
					else if (whichSubMenu.Equals("Load"))
					{
						subMenu = new LoadGameMenu();
						Game1.changeMusicTrack("title_night");
						buttonsDX = 0;
						isTransitioningButtons = false;
					}
					else if (whichSubMenu.Equals("Co-op"))
					{
						subMenu = new CoopMenu();
						Game1.changeMusicTrack("title_night");
						buttonsDX = 0;
						isTransitioningButtons = false;
					}
					else if (whichSubMenu.Equals("Invite"))
					{
						subMenu = new FarmhandMenu();
						Game1.changeMusicTrack("title_night");
						buttonsDX = 0;
						isTransitioningButtons = false;
					}
					else if (whichSubMenu.Equals("New") && numFarmsSaved != -1)
					{
						if (numFarmsSaved >= Game1.GetMaxNumFarmsSaved())
						{
							subMenu = new TooManyFarmsMenu();
							Game1.playSound("newArtifact");
							buttonsDX = 0;
							isTransitioningButtons = false;
						}
						else
						{
							Game1.resetPlayer();
							subMenu = new CharacterCustomization(CharacterCustomization.Source.NewGame);
							Game1.playSound("select");
							Game1.changeMusicTrack("CloudCountry");
							Game1.player.favoriteThing.Value = "";
							buttonsDX = 0;
							isTransitioningButtons = false;
						}
					}
					if (!isTransitioningButtons)
					{
						whichSubMenu = "";
					}
				}
				else if (buttonsDX < 0 && globalXOffset <= 0)
				{
					globalXOffset = 0;
					isTransitioningButtons = false;
					buttonsDX = 0;
					setUpIcons();
					whichSubMenu = "";
					transitioningFromLoadScreen = false;
				}
			}
			for (int i = bigClouds.Count - 1; i >= 0; i--)
			{
				bigClouds[i] -= 0.1f;
				bigClouds[i] += buttonsDX * time.ElapsedGameTime.Milliseconds / 2;
				if (bigClouds[i] < -1536f)
				{
					bigClouds[i] = width;
				}
			}
			for (int j = smallClouds.Count - 1; j >= 0; j--)
			{
				smallClouds[j] -= 0.3f;
				smallClouds[j] += buttonsDX * time.ElapsedGameTime.Milliseconds / 2;
				if (smallClouds[j] < -447f)
				{
					smallClouds[j] = width;
				}
			}
			for (int k = tempSprites.Count - 1; k >= 0; k--)
			{
				if (tempSprites[k].update(time))
				{
					tempSprites.RemoveAt(k);
				}
			}
			for (int l = birds.Count - 1; l >= 0; l--)
			{
				birds[l].position.Y -= viewportDY * 2f;
				if (birds[l].update(time))
				{
					birds.RemoveAt(l);
				}
			}
		}

		private void moveFeatures(int dx, int dy)
		{
			foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
			{
				tempSprite.position.X += dx;
				tempSprite.position.Y += dy;
			}
			foreach (ClickableTextureComponent button in buttons)
			{
				button.bounds.X += dx;
				button.bounds.Y += dy;
			}
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (ShouldAllowInteraction())
			{
				base.receiveScrollWheelAction(direction);
				if (subMenu != null)
				{
					subMenu.receiveScrollWheelAction(direction);
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			if (!ShouldAllowInteraction())
			{
				x = int.MinValue;
				y = int.MinValue;
			}
			base.performHoverAction(x, y);
			muteMusicButton.tryHover(x, y);
			if (subMenu != null)
			{
				subMenu.performHoverAction(x, y);
				if (backButton == null)
				{
					return;
				}
				if (backButton.containsPoint(x, y))
				{
					if (backButton.sourceRect.Y == 252)
					{
						Game1.playSound("Cowboy_Footstep");
					}
					backButton.sourceRect.Y = 279;
				}
				else
				{
					backButton.sourceRect.Y = 252;
				}
				backButton.tryHover(x, y, 0.25f);
			}
			else
			{
				if (!titleInPosition || !HasActiveUser)
				{
					return;
				}
				foreach (ClickableTextureComponent c in buttons)
				{
					if (c.containsPoint(x, y))
					{
						if (c.sourceRect.Y == 187)
						{
							Game1.playSound("Cowboy_Footstep");
						}
						c.sourceRect.Y = 245;
					}
					else
					{
						c.sourceRect.Y = 187;
					}
					c.tryHover(x, y, 0.25f);
				}
				aboutButton.tryHover(x, y, 0.25f);
				if (aboutButton.containsPoint(x, y))
				{
					if (aboutButton.sourceRect.X == 8)
					{
						Game1.playSound("Cowboy_Footstep");
					}
					aboutButton.sourceRect.X = 30;
				}
				else
				{
					aboutButton.sourceRect.X = 8;
				}
				if (!languageButton.visible)
				{
					return;
				}
				languageButton.tryHover(x, y, 0.25f);
				if (languageButton.containsPoint(x, y))
				{
					if (languageButton.sourceRect.X == 52)
					{
						Game1.playSound("Cowboy_Footstep");
					}
					languageButton.sourceRect.X = 79;
				}
				else
				{
					languageButton.sourceRect.X = 52;
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, width, height), new Color(64, 136, 248));
			b.Draw(Game1.mouseCursors, new Rectangle(0, (int)(-900f - viewportY * 0.66f), width, 900 + height - 360), new Rectangle(703, 1912, 1, 264), Color.White);
			if (!whichSubMenu.Equals("Load"))
			{
				b.Draw(Game1.mouseCursors, new Vector2(-30f, -1080f - viewportY * 0.66f), new Rectangle(0, 1453, 638, 195), Color.White * (1f - (float)globalXOffset / 1200f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			}
			foreach (float f in bigClouds)
			{
				b.Draw(cloudsTexture, new Vector2(f, (float)(height - 750) - viewportY * 0.5f), new Rectangle(0, 0, 512, 337), Color.White * globalCloudAlpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.01f);
			}
			b.Draw(Game1.mouseCursors, new Vector2(-90f, (float)(height - 474) - viewportY * 0.66f), new Rectangle(0, 886, 639, 148), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.08f);
			b.Draw(Game1.mouseCursors, new Vector2(1827f, (float)(height - 474) - viewportY * 0.66f), new Rectangle(0, 886, 640, 148), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.08f);
			for (int j = 0; j < smallClouds.Count; j++)
			{
				b.Draw(cloudsTexture, new Vector2(smallClouds[j], (float)(height - 900 - j * 12 * 3) - viewportY * 0.5f), (j % 3 == 0) ? new Rectangle(152, 447, 123, 55) : ((j % 3 == 1) ? new Rectangle(0, 471, 149, 66) : new Rectangle(410, 467, 63, 37)), Color.White * globalCloudAlpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.01f);
			}
			b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(height - 444) - viewportY * 1f), new Rectangle(0, 737, 639, 148), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.1f);
			b.Draw(Game1.mouseCursors, new Vector2(1917f, (float)(height - 444) - viewportY * 1f), new Rectangle(0, 737, 640, 148), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.1f);
			foreach (TemporaryAnimatedSprite bird in birds)
			{
				bird.draw(b);
			}
			b.Draw(cloudsTexture, new Vector2(0f, (float)(height - 426) - viewportY * 2f), new Rectangle(0, 554, 165, 142), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.2f);
			b.Draw(cloudsTexture, new Vector2(width - 366, (float)(height - 459) - viewportY * 2f), new Rectangle(390, 543, 122, 153), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.2f);
			int zoom = (height < 800) ? 2 : 3;
			if (whichSubMenu.Equals("Load") || whichSubMenu.Equals("Co-op") || (subMenu != null && subMenu is LoadGameMenu) || (subMenu != null && subMenu is CharacterCustomization && (subMenu as CharacterCustomization).source == CharacterCustomization.Source.HostNewFarm) || transitioningFromLoadScreen)
			{
				b.Draw(destinationRectangle: new Rectangle(0, 0, width, height), sourceRectangle: new Rectangle(702, 1912, 1, 264), texture: Game1.mouseCursors, color: Color.White * ((float)globalXOffset / 1200f));
				b.Draw(Game1.mouseCursors, Vector2.Zero, new Rectangle(0, 1453, 638, 195), Color.White * ((float)globalXOffset / 1200f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
				b.Draw(Game1.mouseCursors, new Vector2(0f, 780f), new Rectangle(0, 1453, 638, 195), Color.White * ((float)globalXOffset / 1200f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.8f);
			}
			b.Draw(titleButtonsTexture, new Vector2(globalXOffset + width / 2 - 200 * zoom, (float)(-300 * zoom) - viewportY / 3f * (float)zoom), new Rectangle(0, 0, 400, 187), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.2f);
			if (logoSwipeTimer > 0f)
			{
				b.Draw(titleButtonsTexture, new Vector2(globalXOffset + width / 2, (float)(-300 * zoom) - viewportY / 3f * (float)zoom + (float)(93 * zoom)), new Rectangle(0, 0, 400, 187), Color.White, 0f, new Vector2(200f, 93f), (float)zoom + (0.5f - Math.Abs(logoSwipeTimer / 1000f - 0.5f)) * 0.1f, SpriteEffects.None, 0.2f);
			}
			for (int i = 0; i < buttonsToShow; i++)
			{
				if (buttons.Count > i)
				{
					buttons[i].draw(b, (subMenu == null || (!(subMenu is AboutMenu) && !(subMenu is LanguageSelectionMenu))) ? Color.White : (Color.LightGray * 0.8f), 1f);
				}
			}
			if (subMenu == null)
			{
				foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
				{
					tempSprite.draw(b);
				}
			}
			if (subMenu != null && !isTransitioningButtons)
			{
				if (backButton != null && subMenu.readyToClose())
				{
					backButton.draw(b);
				}
				subMenu.draw(b);
				if (backButton != null && !(subMenu is CharacterCustomization) && subMenu.readyToClose())
				{
					backButton.draw(b);
				}
			}
			else if (subMenu == null && isTransitioningButtons && (whichSubMenu.Equals("Load") || whichSubMenu.Equals("New")))
			{
				int x = 84;
				int y = Game1.viewport.Height - 64;
				int w = 0;
				int h = 64;
				Utility.makeSafe(ref x, ref y, w, h);
				SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3689"), x, y);
			}
			else if (subMenu == null && !isTransitioningButtons && titleInPosition && !transitioningCharacterCreationMenu && HasActiveUser)
			{
				aboutButton.draw(b);
				languageButton.draw(b);
			}
			if (chuckleFishTimer > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, width, height), Color.White);
				b.Draw(titleButtonsTexture, new Vector2(width / 2 - 264, height / 2 - 192), new Rectangle(chuckleFishTimer % 200 / 100 * 132, 559, 132, 96), Color.White * Math.Min(1f, (float)chuckleFishTimer / 500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
			}
			else if (logoFadeTimer > 0 || fadeFromWhiteTimer > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, width, height), Color.White * ((float)fadeFromWhiteTimer / 2000f));
				b.Draw(titleButtonsTexture, new Vector2(width / 2, height / 2 - 90), new Rectangle(171 + ((logoFadeTimer / 100 % 2 == 0 && logoSurprisedTimer <= 0) ? 111 : 0), 311, 111, 60), Color.White * ((logoFadeTimer < 500) ? ((float)logoFadeTimer / 500f) : ((logoFadeTimer > 4500) ? (1f - (float)(logoFadeTimer - 4500) / 500f) : 1f)), 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.2f);
				if (logoSurprisedTimer <= 0)
				{
					b.Draw(titleButtonsTexture, new Vector2(width / 2 - 261, height / 2 - 102), new Rectangle((logoFadeTimer / 100 % 2 == 0) ? 85 : 0, 306 + (shades ? 69 : 0), 85, 69), Color.White * ((logoFadeTimer < 500) ? ((float)logoFadeTimer / 500f) : ((logoFadeTimer > 4500) ? (1f - (float)(logoFadeTimer - 4500) / 500f) : 1f)), 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.2f);
				}
				if (logoSurprisedTimer > 0)
				{
					b.Draw(titleButtonsTexture, new Vector2(width / 2 - 261, height / 2 - 102), new Rectangle((logoSurprisedTimer > 800 || logoSurprisedTimer < 400) ? 176 : 260, 375, 85, 69), Color.White * ((logoSurprisedTimer < 200) ? ((float)logoSurprisedTimer / 200f) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.22f);
				}
				if (startupMessage.Length > 0 && logoFadeTimer > 0)
				{
					b.DrawString(Game1.smallFont, Game1.parseText(startupMessage, Game1.smallFont, 640), new Vector2(8f, (float)Game1.viewport.Height - Game1.smallFont.MeasureString(Game1.parseText(startupMessage, Game1.smallFont, 640)).Y - 4f), startupMessageColor * ((logoFadeTimer < 500) ? ((float)logoFadeTimer / 500f) : ((logoFadeTimer > 4500) ? (1f - (float)(logoFadeTimer - 4500) / 500f) : 1f)));
				}
			}
			if (logoFadeTimer > 0)
			{
				_ = logoSurprisedTimer;
				_ = 0;
			}
			if (quitTimer > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, width, height), Color.Black * (1f - (float)quitTimer / 500f));
			}
			if (HasActiveUser)
			{
				muteMusicButton.draw(b);
				windowedButton.draw(b);
			}
			if (ShouldDrawCursor())
			{
				drawMouse(b);
			}
		}

		protected bool ShouldAllowInteraction()
		{
			if (quitTimer > 0)
			{
				return false;
			}
			if (isTransitioningButtons)
			{
				return false;
			}
			if (showButtonsTimer > 0)
			{
				return false;
			}
			if (subMenu != null)
			{
				if (subMenu is LoadGameMenu && (subMenu as LoadGameMenu).IsDoingTask())
				{
					return false;
				}
			}
			else if (!titleInPosition)
			{
				return false;
			}
			return true;
		}

		protected bool ShouldDrawCursor()
		{
			if (!Game1.options.gamepadControls || !Game1.options.snappyMenus)
			{
				return true;
			}
			if (chuckleFishTimer > 0)
			{
				return false;
			}
			if (pauseBeforeViewportRiseTimer > 0)
			{
				return false;
			}
			if (logoSwipeTimer > 0f)
			{
				return false;
			}
			if (logoFadeTimer > 0)
			{
				if (_movedCursor)
				{
					return true;
				}
				return false;
			}
			if (fadeFromWhiteTimer > 0)
			{
				return false;
			}
			if (!titleInPosition)
			{
				return false;
			}
			if (viewportDY != 0f)
			{
				return false;
			}
			if (_subMenu is TooManyFarmsMenu)
			{
				return false;
			}
			if (!ShouldAllowInteraction())
			{
				return false;
			}
			return true;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			width = Game1.viewport.Width;
			height = Game1.viewport.Height;
			if (!isTransitioningButtons && !(subMenu is LoadGameMenu) && !(subMenu is CharacterCustomization))
			{
				setUpIcons();
			}
			if (subMenu != null)
			{
				subMenu.gameWindowSizeChanged(oldBounds, newBounds);
			}
			backButton = new ClickableTextureComponent(menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11739"), new Rectangle(width + -198 - 48, height - 81 - 24, 198, 81), null, "", titleButtonsTexture, new Rectangle(296, 252, 66, 27), 3f)
			{
				myID = 81114
			};
			tempSprites.Clear();
			if (birds.Count > 0 && !titleInPosition)
			{
				for (int i = 0; i < birds.Count; i++)
				{
					birds[i].position = ((i % 2 == 0) ? new Vector2(width - 210, height - 360) : new Vector2(width - 120, height - 330));
				}
			}
			windowedButton = new ClickableTextureComponent(new Rectangle(Game1.viewport.Width - 36 - 16, 16, 36, 36), Game1.mouseCursors, new Rectangle((Game1.options != null && !Game1.options.isCurrentlyWindowed()) ? 155 : 146, 384, 9, 9), 4f)
			{
				myID = 81112,
				leftNeighborID = 81111,
				downNeighborID = 81113
			};
			if (Game1.options.SnappyMenus)
			{
				int id = (currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 81115;
				populateClickableComponentList();
				currentlySnappedComponent = getComponentWithID(id);
				if (_subMenu != null)
				{
					_subMenu.snapCursorToCurrentSnappedComponent();
				}
				else
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		private void showButterflies()
		{
			Game1.playSound("yoba");
			int zoom = (height < 800) ? 2 : 3;
			tempSprites.Add(new TemporaryAnimatedSprite("Tilesheets\\Critters", new Rectangle(128, 96, 16, 16), new Vector2(width / 2 - 240 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 86 * zoom), flipped: false, 0f, Color.White)
			{
				scale = zoom,
				animationLength = 4,
				totalNumberOfLoops = 999999,
				pingPong = true,
				interval = 75f,
				local = true,
				yPeriodic = true,
				yPeriodicLoopTime = 3200f,
				yPeriodicRange = 16f,
				xPeriodic = true,
				xPeriodicLoopTime = 5000f,
				xPeriodicRange = 21f,
				alpha = 0.001f,
				alphaFade = -0.03f
			});
			List<TemporaryAnimatedSprite> l = Utility.sparkleWithinArea(new Rectangle(width / 2 - 240 * zoom - 8 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 86 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f);
			foreach (TemporaryAnimatedSprite item in l)
			{
				item.local = true;
				item.scale = (float)zoom / 4f;
			}
			tempSprites.AddRange(l);
			tempSprites.Add(new TemporaryAnimatedSprite("Tilesheets\\Critters", new Rectangle(192, 96, 16, 16), new Vector2(width / 2 + 220 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 15 * zoom), flipped: false, 0f, Color.White)
			{
				scale = zoom,
				animationLength = 4,
				totalNumberOfLoops = 999999,
				pingPong = true,
				delayBeforeAnimationStart = 10,
				interval = 70f,
				local = true,
				yPeriodic = true,
				yPeriodicLoopTime = 2800f,
				yPeriodicRange = 12f,
				xPeriodic = true,
				xPeriodicLoopTime = 4000f,
				xPeriodicRange = 16f,
				alpha = 0.001f,
				alphaFade = -0.03f
			});
			l = Utility.sparkleWithinArea(new Rectangle(width / 2 + 220 * zoom - 8 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 15 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f);
			foreach (TemporaryAnimatedSprite item2 in l)
			{
				item2.local = true;
				item2.scale = (float)zoom / 4f;
			}
			tempSprites.AddRange(l);
			tempSprites.Add(new TemporaryAnimatedSprite("Tilesheets\\Critters", new Rectangle(256, 96, 16, 16), new Vector2(width / 2 - 250 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 35 * zoom), flipped: false, 0f, Color.White)
			{
				scale = zoom,
				animationLength = 4,
				totalNumberOfLoops = 999999,
				pingPong = true,
				delayBeforeAnimationStart = 20,
				interval = 65f,
				local = true,
				yPeriodic = true,
				yPeriodicLoopTime = 3500f,
				yPeriodicRange = 16f,
				xPeriodic = true,
				xPeriodicLoopTime = 3000f,
				xPeriodicRange = 10f,
				alpha = 0.001f,
				alphaFade = -0.03f
			});
			l = Utility.sparkleWithinArea(new Rectangle(width / 2 - 250 * zoom - 8 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 35 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f);
			foreach (TemporaryAnimatedSprite item3 in l)
			{
				item3.local = true;
				item3.scale = (float)zoom / 4f;
			}
			tempSprites.AddRange(l);
			tempSprites.Add(new TemporaryAnimatedSprite("Tilesheets\\Critters", new Rectangle(256, 112, 16, 16), new Vector2(width / 2 + 250 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 60 * zoom), flipped: false, 0f, Color.White)
			{
				scale = zoom,
				animationLength = 4,
				totalNumberOfLoops = 999999,
				yPeriodic = true,
				yPeriodicLoopTime = 3000f,
				yPeriodicRange = 16f,
				pingPong = true,
				delayBeforeAnimationStart = 30,
				interval = 85f,
				local = true,
				xPeriodic = true,
				xPeriodicLoopTime = 5000f,
				xPeriodicRange = 16f,
				alpha = 0.001f,
				alphaFade = -0.03f
			});
			l = Utility.sparkleWithinArea(new Rectangle(width / 2 + 250 * zoom - 8 * zoom, -300 * zoom - (int)(viewportY / 3f) * zoom + 60 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f);
			foreach (TemporaryAnimatedSprite item4 in l)
			{
				item4.local = true;
				item4.scale = (float)zoom / 4f;
			}
			tempSprites.AddRange(l);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposedValue)
			{
				return;
			}
			if (disposing)
			{
				if (tempSprites != null)
				{
					tempSprites.Clear();
				}
				if (menuContent != null)
				{
					menuContent.Dispose();
					menuContent = null;
				}
				LocalizedContentManager.OnLanguageChange -= OnLanguageChange;
				subMenu = null;
			}
			disposedValue = true;
		}

		~TitleMenu()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
