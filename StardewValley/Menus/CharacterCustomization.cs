using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Minigames;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class CharacterCustomization : IClickableMenu
	{
		public enum Source
		{
			NewGame,
			NewFarmhand,
			Wizard,
			HostNewFarm,
			Dresser,
			ClothesDye,
			DyePots
		}

		public const int region_okbutton = 505;

		public const int region_skipIntroButton = 506;

		public const int region_randomButton = 507;

		public const int region_male = 508;

		public const int region_female = 509;

		public const int region_dog = 510;

		public const int region_cat = 511;

		public const int region_shirtLeft = 512;

		public const int region_shirtRight = 513;

		public const int region_hairLeft = 514;

		public const int region_hairRight = 515;

		public const int region_accLeft = 516;

		public const int region_accRight = 517;

		public const int region_skinLeft = 518;

		public const int region_skinRight = 519;

		public const int region_directionLeft = 520;

		public const int region_directionRight = 521;

		public const int region_cabinsLeft = 621;

		public const int region_cabinsRight = 622;

		public const int region_cabinsClose = 623;

		public const int region_cabinsSeparate = 624;

		public const int region_coopHelp = 625;

		public const int region_coopHelpOK = 626;

		public const int region_difficultyLeft = 627;

		public const int region_difficultyRight = 628;

		public const int region_petLeft = 627;

		public const int region_petRight = 628;

		public const int region_pantsLeft = 629;

		public const int region_pantsRight = 630;

		public const int region_walletsLeft = 631;

		public const int region_walletsRight = 632;

		public const int region_coopHelpRight = 633;

		public const int region_coopHelpLeft = 634;

		public const int region_coopHelpButtons = 635;

		public const int region_advancedOptions = 636;

		public const int region_colorPicker1 = 522;

		public const int region_colorPicker2 = 523;

		public const int region_colorPicker3 = 524;

		public const int region_colorPicker4 = 525;

		public const int region_colorPicker5 = 526;

		public const int region_colorPicker6 = 527;

		public const int region_colorPicker7 = 528;

		public const int region_colorPicker8 = 529;

		public const int region_colorPicker9 = 530;

		public const int region_farmSelection1 = 531;

		public const int region_farmSelection2 = 532;

		public const int region_farmSelection3 = 533;

		public const int region_farmSelection4 = 534;

		public const int region_farmSelection5 = 535;

		public const int region_farmSelection6 = 545;

		public const int region_farmSelection7 = 546;

		public const int region_nameBox = 536;

		public const int region_farmNameBox = 537;

		public const int region_favThingBox = 538;

		public const int colorPickerTimerDelay = 100;

		public const int widthOfMultiplayerArea = 256;

		private List<int> shirtOptions;

		private List<int> hairStyleOptions;

		private List<int> accessoryOptions;

		private int currentShirt;

		private int currentHair;

		private int currentAccessory;

		private int colorPickerTimer;

		private int currentPet;

		public ColorPicker pantsColorPicker;

		public ColorPicker hairColorPicker;

		public ColorPicker eyeColorPicker;

		public List<ClickableComponent> labels = new List<ClickableComponent>();

		public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();

		public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();

		public List<ClickableComponent> genderButtons = new List<ClickableComponent>();

		public List<ClickableComponent> petButtons = new List<ClickableComponent>();

		public List<ClickableTextureComponent> farmTypeButtons = new List<ClickableTextureComponent>();

		public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();

		public List<ClickableTextureComponent> cabinLayoutButtons = new List<ClickableTextureComponent>();

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent skipIntroButton;

		public ClickableTextureComponent randomButton;

		public ClickableTextureComponent coopHelpButton;

		public ClickableTextureComponent coopHelpOkButton;

		public ClickableTextureComponent coopHelpRightButton;

		public ClickableTextureComponent coopHelpLeftButton;

		public ClickableTextureComponent advancedOptionsButton;

		private TextBox nameBox;

		private TextBox farmnameBox;

		private TextBox favThingBox;

		private bool skipIntro;

		public bool isModifyingExistingPet;

		public bool showingCoopHelp;

		public int coopHelpScreen;

		public Source source;

		private Vector2 helpStringSize;

		private string hoverText;

		private string hoverTitle;

		private string coopHelpString;

		private string noneString;

		private string normalDiffString;

		private string toughDiffString;

		private string hardDiffString;

		private string superDiffString;

		private string sharedWalletString;

		private string separateWalletString;

		public ClickableComponent nameBoxCC;

		public ClickableComponent farmnameBoxCC;

		public ClickableComponent favThingBoxCC;

		public ClickableComponent backButton;

		private ClickableComponent nameLabel;

		private ClickableComponent farmLabel;

		private ClickableComponent favoriteLabel;

		private ClickableComponent shirtLabel;

		private ClickableComponent skinLabel;

		private ClickableComponent hairLabel;

		private ClickableComponent accLabel;

		private ClickableComponent pantsStyleLabel;

		private ClickableComponent startingCabinsLabel;

		private ClickableComponent cabinLayoutLabel;

		private ClickableComponent separateWalletLabel;

		private ClickableComponent difficultyModifierLabel;

		private ColorPicker _sliderOpTarget;

		private Action _sliderAction;

		private readonly Action _recolorEyesAction;

		private readonly Action _recolorPantsAction;

		private readonly Action _recolorHairAction;

		protected Clothing _itemToDye;

		protected bool _shouldShowBackButton = true;

		protected bool _isDyeMenu;

		protected Farmer _displayFarmer;

		public Rectangle portraitBox;

		public Rectangle? petPortraitBox;

		public string oldName = "";

		private float advancedCCHighlightTimer;

		private ColorPicker lastHeldColorPicker;

		private int timesRandom;

		public CharacterCustomization(Clothing item)
			: this(Source.ClothesDye)
		{
			_itemToDye = item;
			setUpPositions();
			if (source == Source.NewGame || source == Source.HostNewFarm)
			{
				Game1.spawnMonstersAtNight = false;
			}
			_recolorPantsAction = delegate
			{
				DyeItem(pantsColorPicker.getSelectedColor());
			};
			if (_itemToDye.clothesType.Value == 0)
			{
				_displayFarmer.shirtItem.Set(_itemToDye);
			}
			else if (_itemToDye.clothesType.Value == 1)
			{
				_displayFarmer.pantsItem.Set(_itemToDye);
			}
			_displayFarmer.UpdateClothing();
		}

		public void DyeItem(Color color)
		{
			if (_itemToDye != null)
			{
				_itemToDye.Dye(color, 1f);
				_displayFarmer.FarmerRenderer.MarkSpriteDirty();
			}
		}

		public CharacterCustomization(Source source)
			: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (648 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 648 + IClickableMenu.borderWidth * 2 + 64)
		{
			oldName = Game1.player.Name;
			int items_to_dye = 0;
			if (source == Source.ClothesDye || source == Source.DyePots)
			{
				_isDyeMenu = true;
				switch (source)
				{
				case Source.ClothesDye:
					items_to_dye = 1;
					break;
				case Source.DyePots:
					if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
					{
						items_to_dye++;
					}
					if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
					{
						items_to_dye++;
					}
					break;
				}
				height = 308 + IClickableMenu.borderWidth * 2 + 64 + 72 * items_to_dye - 4;
				xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
				yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2 - 64;
			}
			shirtOptions = new List<int>
			{
				0,
				1,
				2,
				3,
				4,
				5
			};
			hairStyleOptions = new List<int>
			{
				0,
				1,
				2,
				3,
				4,
				5
			};
			accessoryOptions = new List<int>
			{
				0,
				1,
				2,
				3,
				4,
				5
			};
			this.source = source;
			setUpPositions();
			_recolorEyesAction = delegate
			{
				Game1.player.changeEyeColor(eyeColorPicker.getSelectedColor());
			};
			_recolorPantsAction = delegate
			{
				Game1.player.changePants(pantsColorPicker.getSelectedColor());
			};
			_recolorHairAction = delegate
			{
				Game1.player.changeHairColor(hairColorPicker.getSelectedColor());
			};
			if (source == Source.DyePots)
			{
				_recolorHairAction = delegate
				{
					if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
					{
						Game1.player.shirtItem.Value.clothesColor.Value = hairColorPicker.getSelectedColor();
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						_displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				};
				_recolorPantsAction = delegate
				{
					if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
					{
						Game1.player.pantsItem.Value.clothesColor.Value = pantsColorPicker.getSelectedColor();
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						_displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				};
				favThingBoxCC.visible = false;
				nameBoxCC.visible = false;
				farmnameBoxCC.visible = false;
				favoriteLabel.visible = false;
				nameLabel.visible = false;
				farmLabel.visible = false;
			}
			_displayFarmer = GetOrCreateDisplayFarmer();
		}

		public Farmer GetOrCreateDisplayFarmer()
		{
			if (_displayFarmer == null)
			{
				if (source == Source.ClothesDye || source == Source.DyePots)
				{
					_displayFarmer = Game1.player.CreateFakeEventFarmer();
				}
				else
				{
					_displayFarmer = Game1.player;
				}
				if (source == Source.NewFarmhand)
				{
					if (_displayFarmer.pants.Value == -1)
					{
						_displayFarmer.pants.Value = _displayFarmer.GetPantsIndex();
					}
					if (_displayFarmer.shirt.Value == -1)
					{
						_displayFarmer.shirt.Value = _displayFarmer.GetShirtIndex();
					}
				}
				_displayFarmer.faceDirection(2);
				_displayFarmer.FarmerSprite.StopAnimation();
			}
			return _displayFarmer;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			if (_isDyeMenu)
			{
				xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
				yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2 - 64;
			}
			else
			{
				xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
				yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
			}
			setUpPositions();
		}

		public void showAdvancedCharacterCreationHighlight()
		{
			advancedCCHighlightTimer = 4000f;
		}

		private void setUpPositions()
		{
			colorPickerCCs.Clear();
			if (source == Source.ClothesDye && _itemToDye == null)
			{
				return;
			}
			bool allow_accessory_changes = true;
			bool allow_clothing_changes = true;
			if (source == Source.Wizard || source == Source.ClothesDye || source == Source.DyePots)
			{
				allow_clothing_changes = false;
			}
			if (source == Source.ClothesDye || source == Source.DyePots)
			{
				allow_accessory_changes = false;
			}
			labels.Clear();
			petButtons.Clear();
			genderButtons.Clear();
			cabinLayoutButtons.Clear();
			leftSelectionButtons.Clear();
			rightSelectionButtons.Clear();
			farmTypeButtons.Clear();
			if (source == Source.NewGame || source == Source.HostNewFarm)
			{
				advancedOptionsButton = new ClickableTextureComponent("Advanced", new Rectangle(xPositionOnScreen - 80, yPositionOnScreen + height - 80 - 16, 80, 80), null, null, Game1.mouseCursors2, new Rectangle(154, 154, 20, 20), 4f)
				{
					myID = 636,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
			}
			else
			{
				advancedOptionsButton = null;
			}
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 505,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -198 - 48, Game1.uiViewport.Height - 81 - 24, 198, 81), "")
			{
				myID = 81114,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			nameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
				Y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16,
				Text = Game1.player.Name
			};
			nameBoxCC = new ClickableComponent(new Rectangle(xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 192, 48), "")
			{
				myID = 536,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			int textBoxLabelsXOffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-4) : 0;
			labels.Add(nameLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Name")));
			farmnameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
				Y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64,
				Text = Game1.MasterPlayer.farmName
			};
			farmnameBoxCC = new ClickableComponent(new Rectangle(xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 192, 48), "")
			{
				myID = 537,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			int farmLabelXOffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? (-16) : 0;
			labels.Add(farmLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + textBoxLabelsXOffset * 3 + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4 + farmLabelXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Farm")));
			int favThingBoxXoffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 48 : 0;
			favThingBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256 + favThingBoxXoffset,
				Y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128,
				Text = Game1.player.favoriteThing
			};
			favThingBoxCC = new ClickableComponent(new Rectangle(xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 192, 48), "")
			{
				myID = 538,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			labels.Add(favoriteLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 1, 1), Game1.content.LoadString("Strings\\UI:Character_FavoriteThing")));
			randomButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 48, yPositionOnScreen + 64 + 56, 40, 40), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f)
			{
				myID = 507,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			if (source == Source.DyePots || source == Source.ClothesDye)
			{
				randomButton.visible = false;
			}
			portraitBox = new Rectangle(xPositionOnScreen + 64 + 42 - 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 128, 192);
			if (_isDyeMenu)
			{
				portraitBox.X = xPositionOnScreen + (width - portraitBox.Width) / 2;
				randomButton.bounds.X = portraitBox.X - 56;
			}
			int yOffset4 = 128;
			leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(portraitBox.X - 32, portraitBox.Y + 144, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 520,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(portraitBox.Right - 32, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 521,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			int leftSelectionXOffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-20) : 0;
			isModifyingExistingPet = false;
			if (source == Source.NewGame || source == Source.HostNewFarm)
			{
				petPortraitBox = new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 448 - 16 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? 60 : 0), yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192 - 16, 64, 64);
				labels.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8 + textBoxLabelsXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8 + 192, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Animal")));
			}
			if (source == Source.NewGame || source == Source.HostNewFarm || source == Source.NewFarmhand || source == Source.Wizard)
			{
				genderButtons.Add(new ClickableTextureComponent("Male", new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 8, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Male", Game1.mouseCursors, new Rectangle(128, 192, 16, 16), 4f)
				{
					myID = 508,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				genderButtons.Add(new ClickableTextureComponent("Female", new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 64 + 24, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), 4f)
				{
					myID = 509,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				if (source == Source.Wizard && genderButtons != null && genderButtons.Count > 0)
				{
					int start_x = xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 320 + 16;
					int start_y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 64 + 48;
					for (int i = 0; i < genderButtons.Count; i++)
					{
						genderButtons[i].bounds.X = start_x + 80 * i;
						genderButtons[i].bounds.Y = start_y;
					}
				}
				yOffset4 = 256;
				if (source == Source.Wizard)
				{
					yOffset4 = 192;
				}
				leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? (-20) : 0);
				leftSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + leftSelectionXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 518,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				labels.Add(skinLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Skin")));
				rightSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 128, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 519,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			if (source == Source.NewGame || source == Source.HostNewFarm)
			{
				Game1.startingCabins = 0;
				if (source == Source.HostNewFarm)
				{
					Game1.startingCabins = 1;
				}
				Game1.player.difficultyModifier = 1f;
				Game1.player.team.useSeparateWallets.Value = false;
				Point baseFarmButton = new Point(xPositionOnScreen + width + 4 + 8, yPositionOnScreen + IClickableMenu.borderWidth);
				farmTypeButtons.Add(new ClickableTextureComponent("Standard", new Rectangle(baseFarmButton.X, baseFarmButton.Y + 88, 88, 80), null, Game1.content.LoadString("Strings\\UI:Character_FarmStandard"), Game1.mouseCursors, new Rectangle(0, 324, 22, 20), 4f)
				{
					myID = 531,
					downNeighborID = 532,
					leftNeighborID = 537
				});
				farmTypeButtons.Add(new ClickableTextureComponent("Riverland", new Rectangle(baseFarmButton.X, baseFarmButton.Y + 176, 88, 80), null, Game1.content.LoadString("Strings\\UI:Character_FarmFishing"), Game1.mouseCursors, new Rectangle(22, 324, 22, 20), 4f)
				{
					myID = 532,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				farmTypeButtons.Add(new ClickableTextureComponent("Forest", new Rectangle(baseFarmButton.X, baseFarmButton.Y + 264, 88, 80), null, Game1.content.LoadString("Strings\\UI:Character_FarmForaging"), Game1.mouseCursors, new Rectangle(44, 324, 22, 20), 4f)
				{
					myID = 533,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				farmTypeButtons.Add(new ClickableTextureComponent("Hills", new Rectangle(baseFarmButton.X, baseFarmButton.Y + 352, 88, 80), null, Game1.content.LoadString("Strings\\UI:Character_FarmMining"), Game1.mouseCursors, new Rectangle(66, 324, 22, 20), 4f)
				{
					myID = 534,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				farmTypeButtons.Add(new ClickableTextureComponent("Wilderness", new Rectangle(baseFarmButton.X, baseFarmButton.Y + 440, 88, 80), null, Game1.content.LoadString("Strings\\UI:Character_FarmCombat"), Game1.mouseCursors, new Rectangle(88, 324, 22, 20), 4f)
				{
					myID = 535,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				farmTypeButtons.Add(new ClickableTextureComponent("Four Corners", new Rectangle(baseFarmButton.X, baseFarmButton.Y + 528, 88, 80), null, Game1.content.LoadString("Strings\\UI:Character_FarmFourCorners"), Game1.mouseCursors, new Rectangle(0, 345, 22, 20), 4f)
				{
					myID = 545,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				farmTypeButtons.Add(new ClickableTextureComponent("Beach", new Rectangle(baseFarmButton.X, baseFarmButton.Y + 616, 88, 80), null, Game1.content.LoadString("Strings\\UI:Character_FarmBeach"), Game1.mouseCursors, new Rectangle(22, 345, 22, 20), 4f)
				{
					myID = 546,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			if (source == Source.HostNewFarm)
			{
				labels.Add(startingCabinsLabel = new ClickableComponent(new Rectangle(xPositionOnScreen - 21 - 128, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 84, 1, 1), Game1.content.LoadString("Strings\\UI:Character_StartingCabins")));
				leftSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 + 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 621,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				rightSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 622,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				labels.Add(cabinLayoutLabel = new ClickableComponent(new Rectangle(xPositionOnScreen - 128 - (int)(Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Character_CabinLayout")).X / 2f), yPositionOnScreen + IClickableMenu.borderWidth * 2 + 120 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_CabinLayout")));
				cabinLayoutButtons.Add(new ClickableTextureComponent("Close", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Close"), Game1.mouseCursors, new Rectangle(208, 192, 16, 16), 4f)
				{
					myID = 623,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				cabinLayoutButtons.Add(new ClickableTextureComponent("Separate", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Separate"), Game1.mouseCursors, new Rectangle(224, 192, 16, 16), 4f)
				{
					myID = 624,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				labels.Add(difficultyModifierLabel = new ClickableComponent(new Rectangle(xPositionOnScreen - 21 - 128, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 56, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Difficulty")));
				leftSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 627,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				rightSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 628,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				int walletY = yPositionOnScreen + IClickableMenu.borderWidth * 2 + 320 + 100;
				labels.Add(separateWalletLabel = new ClickableComponent(new Rectangle(xPositionOnScreen - 21 - 128, walletY - 24, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Wallets")));
				leftSelectionButtons.Add(new ClickableTextureComponent("Wallets", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, walletY, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 631,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				rightSelectionButtons.Add(new ClickableTextureComponent("Wallets", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, walletY, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 632,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				coopHelpButton = new ClickableTextureComponent("CoopHelp", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 448 + 40, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_CoopHelp"), Game1.mouseCursors, new Rectangle(240, 192, 16, 16), 4f)
				{
					myID = 625,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
				coopHelpOkButton = new ClickableTextureComponent("CoopHelpOK", new Rectangle(xPositionOnScreen - 256 - 12, yPositionOnScreen + height - 64, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
				{
					myID = 626,
					region = 635,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
				noneString = Game1.content.LoadString("Strings\\UI:Character_none");
				normalDiffString = Game1.content.LoadString("Strings\\UI:Character_Normal");
				toughDiffString = Game1.content.LoadString("Strings\\UI:Character_Tough");
				hardDiffString = Game1.content.LoadString("Strings\\UI:Character_Hard");
				superDiffString = Game1.content.LoadString("Strings\\UI:Character_Super");
				separateWalletString = Game1.content.LoadString("Strings\\UI:Character_SeparateWallet");
				sharedWalletString = Game1.content.LoadString("Strings\\UI:Character_SharedWallet");
				coopHelpRightButton = new ClickableTextureComponent("CoopHelpRight", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 633,
					region = 635,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
				coopHelpLeftButton = new ClickableTextureComponent("CoopHelpLeft", new Rectangle(xPositionOnScreen, yPositionOnScreen + height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 634,
					region = 635,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
			}
			Point top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4);
			int label_position = xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8;
			if (_isDyeMenu)
			{
				label_position = xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth;
			}
			if (source == Source.NewGame || source == Source.HostNewFarm || source == Source.NewFarmhand || source == Source.Wizard)
			{
				labels.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_EyeColor")));
				eyeColorPicker = new ColorPicker("Eyes", top.X, top.Y);
				eyeColorPicker.setColor(Game1.player.newEyeColor);
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 522,
					downNeighborID = -99998,
					upNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 523,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 524,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				yOffset4 += 68;
				leftSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + leftSelectionXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 514,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				labels.Add(hairLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Hair")));
				rightSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 515,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4);
			if (source == Source.NewGame || source == Source.HostNewFarm || source == Source.NewFarmhand || source == Source.Wizard)
			{
				labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_HairColor")));
				hairColorPicker = new ColorPicker("Hair", top.X, top.Y);
				hairColorPicker.setColor(Game1.player.hairstyleColor);
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 525,
					downNeighborID = -99998,
					upNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 526,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 527,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
			}
			if (source == Source.DyePots)
			{
				yOffset4 += 68;
				if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
				{
					top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4);
					top.X = xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
					labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_ShirtColor")));
					hairColorPicker = new ColorPicker("Hair", top.X, top.Y);
					hairColorPicker.setColor(Game1.player.GetShirtColor());
					colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
					{
						myID = 525,
						downNeighborID = -99998,
						upNeighborID = -99998,
						leftNeighborImmutable = true,
						rightNeighborImmutable = true
					});
					colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
					{
						myID = 526,
						upNeighborID = -99998,
						downNeighborID = -99998,
						leftNeighborImmutable = true,
						rightNeighborImmutable = true
					});
					colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
					{
						myID = 527,
						upNeighborID = -99998,
						downNeighborID = -99998,
						leftNeighborImmutable = true,
						rightNeighborImmutable = true
					});
					yOffset4 += 64;
				}
				if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
				{
					top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4);
					top.X = xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
					int pantsColorLabelYOffset2 = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? (-16) : 0;
					labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16 + pantsColorLabelYOffset2, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
					pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
					pantsColorPicker.setColor(Game1.player.GetPantsColor());
					colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
					{
						myID = 528,
						downNeighborID = -99998,
						upNeighborID = -99998,
						rightNeighborImmutable = true,
						leftNeighborImmutable = true
					});
					colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
					{
						myID = 529,
						downNeighborID = -99998,
						upNeighborID = -99998,
						rightNeighborImmutable = true,
						leftNeighborImmutable = true
					});
					colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
					{
						myID = 530,
						downNeighborID = -99998,
						upNeighborID = -99998,
						rightNeighborImmutable = true,
						leftNeighborImmutable = true
					});
				}
			}
			else if (allow_clothing_changes)
			{
				yOffset4 += 68;
				int shirtArrowsExtraWidth = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? 8 : 0;
				leftSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset - shirtArrowsExtraWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 512,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				labels.Add(shirtLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Shirt")));
				rightSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + shirtArrowsExtraWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 513,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				int pantsColorLabelYOffset = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? (-16) : 0;
				labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16 + pantsColorLabelYOffset, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
				top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4);
				pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
				pantsColorPicker.setColor(Game1.player.pantsColor);
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 528,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 529,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 530,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
			}
			else if (source == Source.ClothesDye)
			{
				yOffset4 += 60;
				top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4);
				top.X = xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
				labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_DyeColor")));
				pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
				pantsColorPicker.setColor(_itemToDye.clothesColor);
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 528,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 529,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 530,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
			}
			skipIntroButton = new ClickableTextureComponent("Skip Intro", new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 - 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 80, 36, 36), null, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 4f)
			{
				myID = 506,
				upNeighborID = 530,
				leftNeighborID = 517,
				rightNeighborID = 505
			};
			if (allow_clothing_changes)
			{
				yOffset4 += 68;
				leftSelectionButtons.Add(new ClickableTextureComponent("Pants Style", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 629,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				labels.Add(pantsStyleLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Pants")));
				rightSelectionButtons.Add(new ClickableTextureComponent("Pants Style", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 517,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			yOffset4 += 68;
			if (allow_accessory_changes)
			{
				int accessoryArrowsExtraWidth = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? 32 : 0;
				leftSelectionButtons.Add(new ClickableTextureComponent("Acc", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset - accessoryArrowsExtraWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 516,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				labels.Add(accLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Accessory")));
				rightSelectionButtons.Add(new ClickableTextureComponent("Acc", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + accessoryArrowsExtraWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 517,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			if (Game1.gameMode == 3 && Game1.locations != null && source == Source.Wizard)
			{
				Pet pet = Game1.getCharacterFromName<Pet>(Game1.player.getPetName(), mustBeVillager: false);
				if (pet != null)
				{
					Game1.player.whichPetBreed = pet.whichBreed;
					Game1.player.catPerson = (pet is Cat);
					isModifyingExistingPet = true;
					yOffset4 += 60;
					labels.Add(new ClickableComponent(new Rectangle((int)((float)(xPositionOnScreen + width / 2) - Game1.smallFont.MeasureString(pet.name).X / 2f), yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4 + 16, 1, 1), pet.Name));
					top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4);
					top.X = xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 128;
					yOffset4 += 42;
					top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4);
					top.X = xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 128;
					petPortraitBox = new Rectangle(xPositionOnScreen + width / 2 - 32, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset4, 64, 64);
				}
			}
			if (petPortraitBox.HasValue)
			{
				leftSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(petPortraitBox.Value.Left - 64, petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 511,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				rightSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(petPortraitBox.Value.Left + 64, petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 510,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				if (colorPickerCCs != null && colorPickerCCs.Count > 0)
				{
					colorPickerCCs[0].upNeighborID = 511;
					colorPickerCCs[0].upNeighborImmutable = true;
				}
			}
			_shouldShowBackButton = true;
			if (source == Source.Dresser || source == Source.Wizard || source == Source.ClothesDye)
			{
				_shouldShowBackButton = false;
			}
			if (source == Source.Dresser || source == Source.Wizard || _isDyeMenu)
			{
				nameBoxCC.visible = false;
				farmnameBoxCC.visible = false;
				favThingBoxCC.visible = false;
				farmLabel.visible = false;
				nameLabel.visible = false;
				favoriteLabel.visible = false;
			}
			if (source == Source.Wizard)
			{
				nameLabel.visible = true;
				nameBoxCC.visible = true;
				favThingBoxCC.visible = true;
				favoriteLabel.visible = true;
				favThingBoxCC.bounds.Y = farmnameBoxCC.bounds.Y;
				favoriteLabel.bounds.Y = farmLabel.bounds.Y;
				favThingBox.Y = farmnameBox.Y;
			}
			if (source == Source.NewGame || source == Source.HostNewFarm)
			{
				skipIntroButton.visible = true;
			}
			else
			{
				skipIntroButton.visible = false;
			}
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (showingCoopHelp)
			{
				currentlySnappedComponent = getComponentWithID(626);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID(521);
			}
			snapCursorToCurrentSnappedComponent();
		}

		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
			if (currentlySnappedComponent == null)
			{
				return;
			}
			switch (b)
			{
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				switch (currentlySnappedComponent.myID)
				{
				case 522:
					eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
					eyeColorPicker.changeHue(1);
					eyeColorPicker.Dirty = true;
					_sliderOpTarget = eyeColorPicker;
					_sliderAction = _recolorEyesAction;
					break;
				case 523:
					eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
					eyeColorPicker.changeSaturation(1);
					eyeColorPicker.Dirty = true;
					_sliderOpTarget = eyeColorPicker;
					_sliderAction = _recolorEyesAction;
					break;
				case 524:
					eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
					eyeColorPicker.changeValue(1);
					eyeColorPicker.Dirty = true;
					_sliderOpTarget = eyeColorPicker;
					_sliderAction = _recolorEyesAction;
					break;
				case 525:
					hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
					hairColorPicker.changeHue(1);
					hairColorPicker.Dirty = true;
					_sliderOpTarget = hairColorPicker;
					_sliderAction = _recolorHairAction;
					break;
				case 526:
					hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
					hairColorPicker.changeSaturation(1);
					hairColorPicker.Dirty = true;
					_sliderOpTarget = hairColorPicker;
					_sliderAction = _recolorHairAction;
					break;
				case 527:
					hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
					hairColorPicker.changeValue(1);
					hairColorPicker.Dirty = true;
					_sliderOpTarget = hairColorPicker;
					_sliderAction = _recolorHairAction;
					break;
				case 528:
					pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
					pantsColorPicker.changeHue(1);
					pantsColorPicker.Dirty = true;
					_sliderOpTarget = pantsColorPicker;
					_sliderAction = _recolorPantsAction;
					break;
				case 529:
					pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
					pantsColorPicker.changeSaturation(1);
					pantsColorPicker.Dirty = true;
					_sliderOpTarget = pantsColorPicker;
					_sliderAction = _recolorPantsAction;
					break;
				case 530:
					pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
					pantsColorPicker.changeValue(1);
					pantsColorPicker.Dirty = true;
					_sliderOpTarget = pantsColorPicker;
					_sliderAction = _recolorPantsAction;
					break;
				}
				break;
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				switch (currentlySnappedComponent.myID)
				{
				case 522:
					eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
					eyeColorPicker.changeHue(-1);
					eyeColorPicker.Dirty = true;
					_sliderOpTarget = eyeColorPicker;
					_sliderAction = _recolorEyesAction;
					break;
				case 523:
					eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
					eyeColorPicker.changeSaturation(-1);
					eyeColorPicker.Dirty = true;
					_sliderOpTarget = eyeColorPicker;
					_sliderAction = _recolorEyesAction;
					break;
				case 524:
					eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
					eyeColorPicker.changeValue(-1);
					eyeColorPicker.Dirty = true;
					_sliderOpTarget = eyeColorPicker;
					_sliderAction = _recolorEyesAction;
					break;
				case 525:
					hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
					hairColorPicker.changeHue(-1);
					hairColorPicker.Dirty = true;
					_sliderOpTarget = hairColorPicker;
					_sliderAction = _recolorHairAction;
					break;
				case 526:
					hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
					hairColorPicker.changeSaturation(-1);
					hairColorPicker.Dirty = true;
					_sliderOpTarget = hairColorPicker;
					_sliderAction = _recolorHairAction;
					break;
				case 527:
					hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
					hairColorPicker.changeValue(-1);
					hairColorPicker.Dirty = true;
					_sliderOpTarget = hairColorPicker;
					_sliderAction = _recolorHairAction;
					break;
				case 528:
					pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
					pantsColorPicker.changeHue(-1);
					pantsColorPicker.Dirty = true;
					_sliderOpTarget = pantsColorPicker;
					_sliderAction = _recolorPantsAction;
					break;
				case 529:
					pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
					pantsColorPicker.changeSaturation(-1);
					pantsColorPicker.Dirty = true;
					_sliderOpTarget = pantsColorPicker;
					_sliderAction = _recolorPantsAction;
					break;
				case 530:
					pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
					pantsColorPicker.changeValue(-1);
					pantsColorPicker.Dirty = true;
					_sliderOpTarget = pantsColorPicker;
					_sliderAction = _recolorPantsAction;
					break;
				}
				break;
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (currentlySnappedComponent == null)
			{
				return;
			}
			switch (b)
			{
			case Buttons.RightTrigger:
			{
				int myID = currentlySnappedComponent.myID;
				if ((uint)(myID - 512) <= 9u)
				{
					selectionClick(currentlySnappedComponent.name, 1);
				}
				break;
			}
			case Buttons.LeftTrigger:
			{
				int myID = currentlySnappedComponent.myID;
				if ((uint)(myID - 512) <= 9u)
				{
					selectionClick(currentlySnappedComponent.name, -1);
				}
				break;
			}
			case Buttons.B:
				if (showingCoopHelp)
				{
					receiveLeftClick(coopHelpOkButton.bounds.Center.X, coopHelpOkButton.bounds.Center.Y);
				}
				break;
			}
		}

		private void optionButtonClick(string name)
		{
			switch (name)
			{
			case "Standard":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 0;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Riverland":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 1;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Forest":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 2;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Hills":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 3;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Wilderness":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 4;
					Game1.spawnMonstersAtNight = true;
				}
				break;
			case "Four Corners":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 5;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Beach":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 6;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Male":
				Game1.player.changeGender(male: true);
				if (source != Source.Wizard)
				{
					Game1.player.changeHairStyle(0);
				}
				break;
			case "Close":
				Game1.cabinsSeparate = false;
				break;
			case "Separate":
				Game1.cabinsSeparate = true;
				break;
			case "Female":
				Game1.player.changeGender(male: false);
				if (source != Source.Wizard)
				{
					Game1.player.changeHairStyle(16);
				}
				break;
			case "Cat":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.player.catPerson = true;
				}
				break;
			case "Dog":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.player.catPerson = false;
				}
				break;
			case "OK":
			{
				if (!canLeaveMenu())
				{
					return;
				}
				if (_itemToDye != null)
				{
					if (!Game1.player.IsEquippedItem(_itemToDye))
					{
						Utility.CollectOrDrop(_itemToDye);
					}
					_itemToDye = null;
				}
				if (source == Source.ClothesDye)
				{
					Game1.exitActiveMenu();
					break;
				}
				Game1.player.Name = nameBox.Text.Trim();
				Game1.player.displayName = Game1.player.Name;
				Game1.player.favoriteThing.Value = favThingBox.Text.Trim();
				Game1.player.isCustomized.Value = true;
				Game1.player.ConvertClothingOverrideToClothesItems();
				if (source == Source.HostNewFarm)
				{
					Game1.multiplayerMode = 2;
				}
				try
				{
					if (Game1.player.Name != oldName && Game1.player.Name.IndexOf("[") != -1 && Game1.player.Name.IndexOf("]") != -1)
					{
						int start = Game1.player.Name.IndexOf("[");
						int end = Game1.player.Name.IndexOf("]");
						if (end > start)
						{
							string s = Game1.player.Name.Substring(start + 1, end - start - 1);
							int item_index = -1;
							if (int.TryParse(s, out item_index))
							{
								string itemName = Game1.objectInformation[item_index].Split('/')[0];
								switch (Game1.random.Next(5))
								{
								case 0:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg1"), new Color(104, 214, 255));
									break;
								case 1:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg2", Lexicon.makePlural(itemName)), new Color(100, 50, 255));
									break;
								case 2:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg3", Lexicon.makePlural(itemName)), new Color(0, 220, 40));
									break;
								case 3:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg4"), new Color(0, 220, 40));
									DelayedAction.functionAfterDelay(delegate
									{
										Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg5"), new Color(104, 214, 255));
									}, 12000);
									break;
								case 4:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg6", Lexicon.getProperArticleForWord(itemName), itemName), new Color(100, 120, 255));
									break;
								}
							}
						}
					}
				}
				catch (Exception)
				{
				}
				string changed_pet_name = null;
				if (petPortraitBox.HasValue && Game1.gameMode == 3 && Game1.locations != null)
				{
					Pet pet = Game1.getCharacterFromName<Pet>(Game1.player.getPetName(), mustBeVillager: false);
					if (pet != null && petHasChanges(pet))
					{
						pet.whichBreed.Value = Game1.player.whichPetBreed;
						changed_pet_name = pet.getName();
					}
				}
				if (Game1.activeClickableMenu is TitleMenu)
				{
					(Game1.activeClickableMenu as TitleMenu).createdNewCharacter(skipIntro);
					break;
				}
				Game1.exitActiveMenu();
				if (Game1.currentMinigame != null && Game1.currentMinigame is Intro)
				{
					(Game1.currentMinigame as Intro).doneCreatingCharacter();
				}
				else if (source == Source.Wizard)
				{
					if (changed_pet_name != null)
					{
						Game1.multiplayer.globalChatInfoMessage("Makeover_Pet", Game1.player.Name, changed_pet_name);
					}
					else
					{
						Game1.multiplayer.globalChatInfoMessage("Makeover", Game1.player.Name);
					}
					Game1.flashAlpha = 1f;
					Game1.playSound("yoba");
				}
				else if (source == Source.ClothesDye)
				{
					Game1.playSound("yoba");
				}
				break;
			}
			}
			Game1.playSound("coin");
		}

		public bool petHasChanges(Pet pet)
		{
			if (Game1.player.catPerson && pet == null)
			{
				return true;
			}
			if (Game1.player.whichPetBreed != pet.whichBreed.Value)
			{
				return true;
			}
			return false;
		}

		private void selectionClick(string name, int change)
		{
			switch (name)
			{
			case "Skin":
				Game1.player.changeSkinColor((int)Game1.player.skin + change);
				Game1.playSound("skeletonStep");
				break;
			case "Hair":
			{
				List<int> all_hairs = Farmer.GetAllHairstyleIndices();
				int current_index2 = all_hairs.IndexOf(Game1.player.hair);
				current_index2 += change;
				if (current_index2 >= all_hairs.Count)
				{
					current_index2 = 0;
				}
				else if (current_index2 < 0)
				{
					current_index2 = all_hairs.Count() - 1;
				}
				Game1.player.changeHairStyle(all_hairs[current_index2]);
				Game1.playSound("grassyStep");
				break;
			}
			case "Shirt":
				Game1.player.changeShirt((int)Game1.player.shirt + change, is_customization_screen: true);
				Game1.playSound("coin");
				break;
			case "Pants Style":
				Game1.player.changePantStyle((int)Game1.player.pants + change, is_customization_screen: true);
				Game1.playSound("coin");
				break;
			case "Acc":
				Game1.player.changeAccessory((int)Game1.player.accessory + change);
				Game1.playSound("purchase");
				break;
			case "Direction":
				_displayFarmer.faceDirection((_displayFarmer.FacingDirection - change + 4) % 4);
				_displayFarmer.FarmerSprite.StopAnimation();
				_displayFarmer.completelyStopAnimatingOrDoingAction();
				Game1.playSound("pickUpItem");
				break;
			case "Cabins":
				if ((Game1.startingCabins != 0 || change >= 0) && (Game1.startingCabins != 3 || change <= 0))
				{
					Game1.playSound("axchop");
				}
				Game1.startingCabins += change;
				Game1.startingCabins = Math.Max(0, Math.Min(3, Game1.startingCabins));
				break;
			case "Difficulty":
				if (Game1.player.difficultyModifier < 1f && change < 0)
				{
					Game1.playSound("breathout");
					Game1.player.difficultyModifier += 0.25f;
				}
				else if (Game1.player.difficultyModifier > 0.25f && change > 0)
				{
					Game1.playSound("batFlap");
					Game1.player.difficultyModifier -= 0.25f;
				}
				break;
			case "Wallets":
				if ((bool)Game1.player.team.useSeparateWallets)
				{
					Game1.playSound("coin");
					Game1.player.team.useSeparateWallets.Value = false;
				}
				else
				{
					Game1.playSound("coin");
					Game1.player.team.useSeparateWallets.Value = true;
				}
				break;
			case "Pet":
				Game1.player.whichPetBreed += change;
				if (Game1.player.whichPetBreed >= 3)
				{
					Game1.player.whichPetBreed = 0;
					if (!isModifyingExistingPet)
					{
						Game1.player.catPerson = !Game1.player.catPerson;
					}
				}
				else if (Game1.player.whichPetBreed < 0)
				{
					Game1.player.whichPetBreed = 2;
					if (!isModifyingExistingPet)
					{
						Game1.player.catPerson = !Game1.player.catPerson;
					}
				}
				Game1.playSound("coin");
				break;
			}
		}

		public void ShowAdvancedOptions()
		{
			AddDependency();
			(TitleMenu.subMenu = new AdvancedGameOptions()).exitFunction = delegate
			{
				TitleMenu.subMenu = this;
				RemoveDependency();
				populateClickableComponentList();
				if (Game1.options.SnappyMenus)
				{
					setCurrentlySnappedComponentTo(636);
					snapCursorToCurrentSnappedComponent();
				}
			};
		}

		public override bool readyToClose()
		{
			if (showingCoopHelp)
			{
				return false;
			}
			if (Game1.lastCursorMotionWasMouse)
			{
				foreach (ClickableTextureComponent farmTypeButton in farmTypeButtons)
				{
					if (farmTypeButton.containsPoint(Game1.getMouseX(ui_scale: true), Game1.getMouseY(ui_scale: true)))
					{
						return false;
					}
				}
			}
			return base.readyToClose();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (showingCoopHelp)
			{
				if (coopHelpOkButton != null && coopHelpOkButton.containsPoint(x, y))
				{
					showingCoopHelp = false;
					Game1.playSound("bigDeSelect");
					if (Game1.options.SnappyMenus)
					{
						currentlySnappedComponent = coopHelpButton;
						snapCursorToCurrentSnappedComponent();
					}
				}
				if (coopHelpScreen == 0 && coopHelpRightButton != null && coopHelpRightButton.containsPoint(x, y))
				{
					coopHelpScreen++;
					coopHelpString = Game1.parseText(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString2").Replace("^", Environment.NewLine), Game1.dialogueFont, width + 384 - IClickableMenu.borderWidth * 2);
					Game1.playSound("shwip");
				}
				if (coopHelpScreen == 1 && coopHelpLeftButton != null && coopHelpLeftButton.containsPoint(x, y))
				{
					coopHelpScreen--;
					coopHelpString = Game1.parseText(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString").Replace("^", Environment.NewLine), Game1.dialogueFont, width + 384 - IClickableMenu.borderWidth * 2);
					Game1.playSound("shwip");
				}
				return;
			}
			if (genderButtons.Count > 0)
			{
				foreach (ClickableComponent c6 in genderButtons)
				{
					if (c6.containsPoint(x, y))
					{
						optionButtonClick(c6.name);
						c6.scale -= 0.5f;
						c6.scale = Math.Max(3.5f, c6.scale);
					}
				}
			}
			if (farmTypeButtons.Count > 0)
			{
				foreach (ClickableTextureComponent c5 in farmTypeButtons)
				{
					if (c5.containsPoint(x, y) && !c5.name.Contains("Gray"))
					{
						optionButtonClick(c5.name);
						c5.scale -= 0.5f;
						c5.scale = Math.Max(3.5f, c5.scale);
					}
				}
			}
			if (petButtons.Count > 0)
			{
				foreach (ClickableComponent c4 in petButtons)
				{
					if (c4.containsPoint(x, y))
					{
						optionButtonClick(c4.name);
						c4.scale -= 0.5f;
						c4.scale = Math.Max(3.5f, c4.scale);
					}
				}
			}
			if (cabinLayoutButtons.Count > 0)
			{
				foreach (ClickableTextureComponent c3 in cabinLayoutButtons)
				{
					if (Game1.startingCabins > 0 && c3.containsPoint(x, y))
					{
						optionButtonClick(c3.name);
						c3.scale -= 0.5f;
						c3.scale = Math.Max(3.5f, c3.scale);
					}
				}
			}
			if (leftSelectionButtons.Count > 0)
			{
				foreach (ClickableComponent c2 in leftSelectionButtons)
				{
					if (c2.containsPoint(x, y))
					{
						selectionClick(c2.name, -1);
						if (c2.scale != 0f)
						{
							c2.scale -= 0.25f;
							c2.scale = Math.Max(0.75f, c2.scale);
						}
					}
				}
			}
			if (rightSelectionButtons.Count > 0)
			{
				foreach (ClickableComponent c in rightSelectionButtons)
				{
					if (c.containsPoint(x, y))
					{
						selectionClick(c.name, 1);
						if (c.scale != 0f)
						{
							c.scale -= 0.25f;
							c.scale = Math.Max(0.75f, c.scale);
						}
					}
				}
			}
			if (okButton.containsPoint(x, y) && canLeaveMenu())
			{
				optionButtonClick(okButton.name);
				okButton.scale -= 0.25f;
				okButton.scale = Math.Max(0.75f, okButton.scale);
			}
			if (hairColorPicker != null && hairColorPicker.containsPoint(x, y))
			{
				Color color2 = hairColorPicker.click(x, y);
				if (source == Source.DyePots)
				{
					if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
					{
						Game1.player.shirtItem.Value.clothesColor.Value = color2;
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						_displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				}
				else
				{
					Game1.player.changeHairColor(color2);
				}
				lastHeldColorPicker = hairColorPicker;
			}
			else if (pantsColorPicker != null && pantsColorPicker.containsPoint(x, y))
			{
				Color color = pantsColorPicker.click(x, y);
				if (source == Source.DyePots)
				{
					if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
					{
						Game1.player.pantsItem.Value.clothesColor.Value = color;
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						_displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				}
				else if (source == Source.ClothesDye)
				{
					DyeItem(color);
				}
				else
				{
					Game1.player.changePants(color);
				}
				lastHeldColorPicker = pantsColorPicker;
			}
			else if (eyeColorPicker != null && eyeColorPicker.containsPoint(x, y))
			{
				Game1.player.changeEyeColor(eyeColorPicker.click(x, y));
				lastHeldColorPicker = eyeColorPicker;
			}
			if (source != Source.Dresser && source != Source.ClothesDye && source != Source.DyePots)
			{
				nameBox.Update();
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					farmnameBox.Update();
				}
				else
				{
					farmnameBox.Text = Game1.MasterPlayer.farmName.Value;
				}
				favThingBox.Update();
				if ((source == Source.NewGame || source == Source.HostNewFarm) && skipIntroButton.containsPoint(x, y))
				{
					Game1.playSound("drumkit6");
					skipIntroButton.sourceRect.X = ((skipIntroButton.sourceRect.X == 227) ? 236 : 227);
					skipIntro = !skipIntro;
				}
			}
			if (coopHelpButton != null && coopHelpButton.containsPoint(x, y))
			{
				if (Game1.options.SnappyMenus)
				{
					currentlySnappedComponent = coopHelpOkButton;
					snapCursorToCurrentSnappedComponent();
				}
				Game1.playSound("bigSelect");
				showingCoopHelp = true;
				coopHelpScreen = 0;
				coopHelpString = Game1.parseText(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString").Replace("^", Environment.NewLine), Game1.dialogueFont, width + 384 - IClickableMenu.borderWidth * 2);
				helpStringSize = Game1.dialogueFont.MeasureString(coopHelpString);
				coopHelpRightButton.bounds.Y = yPositionOnScreen + (int)helpStringSize.Y + IClickableMenu.borderWidth * 2 - 4;
				coopHelpRightButton.bounds.X = xPositionOnScreen + (int)helpStringSize.X - IClickableMenu.borderWidth * 5;
				coopHelpLeftButton.bounds.Y = yPositionOnScreen + (int)helpStringSize.Y + IClickableMenu.borderWidth * 2 - 4;
				coopHelpLeftButton.bounds.X = xPositionOnScreen - IClickableMenu.borderWidth * 4;
			}
			if (advancedOptionsButton != null && advancedOptionsButton.containsPoint(x, y))
			{
				Game1.playSound("drumkit6");
				ShowAdvancedOptions();
			}
			if (!randomButton.containsPoint(x, y))
			{
				return;
			}
			string sound = "drumkit6";
			if (timesRandom > 0)
			{
				switch (Game1.random.Next(15))
				{
				case 0:
					sound = "drumkit1";
					break;
				case 1:
					sound = "dirtyHit";
					break;
				case 2:
					sound = "axchop";
					break;
				case 3:
					sound = "hoeHit";
					break;
				case 4:
					sound = "fishSlap";
					break;
				case 5:
					sound = "drumkit6";
					break;
				case 6:
					sound = "drumkit5";
					break;
				case 7:
					sound = "drumkit6";
					break;
				case 8:
					sound = "junimoMeep1";
					break;
				case 9:
					sound = "coin";
					break;
				case 10:
					sound = "axe";
					break;
				case 11:
					sound = "hammer";
					break;
				case 12:
					sound = "drumkit2";
					break;
				case 13:
					sound = "drumkit4";
					break;
				case 14:
					sound = "drumkit3";
					break;
				}
			}
			Game1.playSound(sound);
			timesRandom++;
			if (accLabel != null && accLabel.visible)
			{
				if (Game1.random.NextDouble() < 0.33)
				{
					if (Game1.player.IsMale)
					{
						Game1.player.changeAccessory(Game1.random.Next(19));
					}
					else
					{
						Game1.player.changeAccessory(Game1.random.Next(6, 19));
					}
				}
				else
				{
					Game1.player.changeAccessory(-1);
				}
			}
			if (hairLabel != null && hairLabel.visible)
			{
				if (Game1.player.IsMale)
				{
					Game1.player.changeHairStyle(Game1.random.Next(16));
				}
				else
				{
					Game1.player.changeHairStyle(Game1.random.Next(16, 32));
				}
				Color hairColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
				if (Game1.random.NextDouble() < 0.5)
				{
					hairColor.R /= 2;
					hairColor.G /= 2;
					hairColor.B /= 2;
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					hairColor.R = (byte)Game1.random.Next(15, 50);
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					hairColor.G = (byte)Game1.random.Next(15, 50);
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					hairColor.B = (byte)Game1.random.Next(15, 50);
				}
				Game1.player.changeHairColor(hairColor);
				hairColorPicker.setColor(hairColor);
			}
			if (shirtLabel != null && shirtLabel.visible)
			{
				Game1.player.changeShirt(Game1.random.Next(112));
			}
			if (skinLabel != null && skinLabel.visible)
			{
				Game1.player.changeSkinColor(Game1.random.Next(6));
				if (Game1.random.NextDouble() < 0.25)
				{
					Game1.player.changeSkinColor(Game1.random.Next(24));
				}
			}
			if (pantsStyleLabel != null && pantsStyleLabel.visible)
			{
				Color pantsColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
				if (Game1.random.NextDouble() < 0.5)
				{
					pantsColor.R /= 2;
					pantsColor.G /= 2;
					pantsColor.B /= 2;
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					pantsColor.R = (byte)Game1.random.Next(15, 50);
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					pantsColor.G = (byte)Game1.random.Next(15, 50);
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					pantsColor.B = (byte)Game1.random.Next(15, 50);
				}
				Game1.player.changePants(pantsColor);
				pantsColorPicker.setColor(Game1.player.pantsColor);
			}
			if (eyeColorPicker != null)
			{
				Color eyeColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
				eyeColor.R /= 2;
				eyeColor.G /= 2;
				eyeColor.B /= 2;
				if (Game1.random.NextDouble() < 0.5)
				{
					eyeColor.R = (byte)Game1.random.Next(15, 50);
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					eyeColor.G = (byte)Game1.random.Next(15, 50);
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					eyeColor.B = (byte)Game1.random.Next(15, 50);
				}
				Game1.player.changeEyeColor(eyeColor);
				eyeColorPicker.setColor(Game1.player.newEyeColor);
			}
			randomButton.scale = 3.5f;
		}

		public override void leftClickHeld(int x, int y)
		{
			colorPickerTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			if (colorPickerTimer > 0)
			{
				return;
			}
			if (lastHeldColorPicker != null && !Game1.options.SnappyMenus)
			{
				if (lastHeldColorPicker.Equals(hairColorPicker))
				{
					Color color2 = hairColorPicker.clickHeld(x, y);
					if (source == Source.DyePots)
					{
						if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
						{
							Game1.player.shirtItem.Value.clothesColor.Value = color2;
							Game1.player.FarmerRenderer.MarkSpriteDirty();
							_displayFarmer.FarmerRenderer.MarkSpriteDirty();
						}
					}
					else
					{
						Game1.player.changeHairColor(color2);
					}
				}
				if (lastHeldColorPicker.Equals(pantsColorPicker))
				{
					Color color = pantsColorPicker.clickHeld(x, y);
					if (source == Source.DyePots)
					{
						if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
						{
							Game1.player.pantsItem.Value.clothesColor.Value = color;
							Game1.player.FarmerRenderer.MarkSpriteDirty();
							_displayFarmer.FarmerRenderer.MarkSpriteDirty();
						}
					}
					else if (source == Source.ClothesDye)
					{
						DyeItem(color);
					}
					else
					{
						Game1.player.changePants(color);
					}
				}
				if (lastHeldColorPicker.Equals(eyeColorPicker))
				{
					Game1.player.changeEyeColor(eyeColorPicker.clickHeld(x, y));
				}
			}
			colorPickerTimer = 100;
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (hairColorPicker != null)
			{
				hairColorPicker.releaseClick();
			}
			if (pantsColorPicker != null)
			{
				pantsColorPicker.releaseClick();
			}
			if (eyeColorPicker != null)
			{
				eyeColorPicker.releaseClick();
			}
			lastHeldColorPicker = null;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Tab)
			{
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					if (nameBox.Selected)
					{
						farmnameBox.SelectMe();
						nameBox.Selected = false;
					}
					else if (farmnameBox.Selected)
					{
						farmnameBox.Selected = false;
						favThingBox.SelectMe();
					}
					else
					{
						favThingBox.Selected = false;
						nameBox.SelectMe();
					}
				}
				else if (source == Source.NewFarmhand)
				{
					if (nameBox.Selected)
					{
						favThingBox.SelectMe();
						nameBox.Selected = false;
					}
					else
					{
						favThingBox.Selected = false;
						nameBox.SelectMe();
					}
				}
			}
			if (Game1.options.SnappyMenus && !Game1.options.doesInputListContain(Game1.options.menuButton, key) && Game1.GetKeyboardState().GetPressedKeys().Count() == 0)
			{
				base.receiveKeyPress(key);
			}
		}

		public override void performHoverAction(int x, int y)
		{
			hoverText = "";
			hoverTitle = "";
			foreach (ClickableTextureComponent c6 in leftSelectionButtons)
			{
				if (c6.containsPoint(x, y))
				{
					c6.scale = Math.Min(c6.scale + 0.02f, c6.baseScale + 0.1f);
				}
				else
				{
					c6.scale = Math.Max(c6.scale - 0.02f, c6.baseScale);
				}
				if (c6.name.Equals("Cabins") && Game1.startingCabins == 0)
				{
					c6.scale = 0f;
				}
			}
			foreach (ClickableTextureComponent c5 in rightSelectionButtons)
			{
				if (c5.containsPoint(x, y))
				{
					c5.scale = Math.Min(c5.scale + 0.02f, c5.baseScale + 0.1f);
				}
				else
				{
					c5.scale = Math.Max(c5.scale - 0.02f, c5.baseScale);
				}
				if (c5.name.Equals("Cabins") && Game1.startingCabins == 3)
				{
					c5.scale = 0f;
				}
			}
			if (source == Source.NewGame || source == Source.HostNewFarm)
			{
				foreach (ClickableTextureComponent c4 in farmTypeButtons)
				{
					if (c4.containsPoint(x, y) && !c4.name.Contains("Gray"))
					{
						c4.scale = Math.Min(c4.scale + 0.02f, c4.baseScale + 0.1f);
						hoverTitle = c4.hoverText.Split('_')[0];
						hoverText = c4.hoverText.Split('_')[1];
					}
					else
					{
						c4.scale = Math.Max(c4.scale - 0.02f, c4.baseScale);
						if (c4.name.Contains("Gray") && c4.containsPoint(x, y))
						{
							hoverText = "Reach level 10 " + Game1.content.LoadString("Strings\\UI:Character_" + c4.name.Split('_')[1]) + " to unlock.";
						}
					}
				}
			}
			foreach (ClickableTextureComponent c3 in genderButtons)
			{
				if (c3.containsPoint(x, y))
				{
					c3.scale = Math.Min(c3.scale + 0.05f, c3.baseScale + 0.5f);
				}
				else
				{
					c3.scale = Math.Max(c3.scale - 0.05f, c3.baseScale);
				}
			}
			if (source == Source.NewGame || source == Source.HostNewFarm)
			{
				foreach (ClickableTextureComponent c2 in petButtons)
				{
					if (c2.containsPoint(x, y))
					{
						c2.scale = Math.Min(c2.scale + 0.05f, c2.baseScale + 0.5f);
					}
					else
					{
						c2.scale = Math.Max(c2.scale - 0.05f, c2.baseScale);
					}
				}
				foreach (ClickableTextureComponent c in cabinLayoutButtons)
				{
					if (Game1.startingCabins > 0 && c.containsPoint(x, y))
					{
						c.scale = Math.Min(c.scale + 0.05f, c.baseScale + 0.5f);
						hoverText = c.hoverText;
					}
					else
					{
						c.scale = Math.Max(c.scale - 0.05f, c.baseScale);
					}
				}
			}
			if (okButton.containsPoint(x, y) && canLeaveMenu())
			{
				okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.1f);
			}
			else
			{
				okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
			}
			if (coopHelpButton != null)
			{
				if (coopHelpButton.containsPoint(x, y))
				{
					coopHelpButton.scale = Math.Min(coopHelpButton.scale + 0.05f, coopHelpButton.baseScale + 0.5f);
					hoverText = coopHelpButton.hoverText;
				}
				else
				{
					coopHelpButton.scale = Math.Max(coopHelpButton.scale - 0.05f, coopHelpButton.baseScale);
				}
			}
			if (coopHelpOkButton != null)
			{
				if (coopHelpOkButton.containsPoint(x, y))
				{
					coopHelpOkButton.scale = Math.Min(coopHelpOkButton.scale + 0.025f, coopHelpOkButton.baseScale + 0.2f);
				}
				else
				{
					coopHelpOkButton.scale = Math.Max(coopHelpOkButton.scale - 0.025f, coopHelpOkButton.baseScale);
				}
			}
			if (coopHelpRightButton != null)
			{
				if (coopHelpRightButton.containsPoint(x, y))
				{
					coopHelpRightButton.scale = Math.Min(coopHelpRightButton.scale + 0.025f, coopHelpRightButton.baseScale + 0.2f);
				}
				else
				{
					coopHelpRightButton.scale = Math.Max(coopHelpRightButton.scale - 0.025f, coopHelpRightButton.baseScale);
				}
			}
			if (coopHelpLeftButton != null)
			{
				if (coopHelpLeftButton.containsPoint(x, y))
				{
					coopHelpLeftButton.scale = Math.Min(coopHelpLeftButton.scale + 0.025f, coopHelpLeftButton.baseScale + 0.2f);
				}
				else
				{
					coopHelpLeftButton.scale = Math.Max(coopHelpLeftButton.scale - 0.025f, coopHelpLeftButton.baseScale);
				}
			}
			if (advancedOptionsButton != null)
			{
				advancedOptionsButton.tryHover(x, y);
			}
			randomButton.tryHover(x, y, 0.25f);
			randomButton.tryHover(x, y, 0.25f);
			if ((hairColorPicker != null && hairColorPicker.containsPoint(x, y)) || (pantsColorPicker != null && pantsColorPicker.containsPoint(x, y)) || (eyeColorPicker != null && eyeColorPicker.containsPoint(x, y)))
			{
				Game1.SetFreeCursorDrag();
			}
			nameBox.Hover(x, y);
			farmnameBox.Hover(x, y);
			favThingBox.Hover(x, y);
			skipIntroButton.tryHover(x, y);
		}

		public bool canLeaveMenu()
		{
			if (source != Source.ClothesDye && source != Source.DyePots)
			{
				if (Game1.player.Name.Length > 0 && Game1.player.farmName.Length > 0)
				{
					return Game1.player.favoriteThing.Length > 0;
				}
				return false;
			}
			return true;
		}

		private string getNameOfDifficulty()
		{
			if (Game1.player.difficultyModifier < 0.5f)
			{
				return superDiffString;
			}
			if (Game1.player.difficultyModifier < 0.75f)
			{
				return hardDiffString;
			}
			if (Game1.player.difficultyModifier < 1f)
			{
				return toughDiffString;
			}
			return normalDiffString;
		}

		public override void draw(SpriteBatch b)
		{
			bool ignoreTitleSafe2 = false;
			ignoreTitleSafe2 = true;
			if (showingCoopHelp)
			{
				IClickableMenu.drawTextureBox(b, xPositionOnScreen - 192, yPositionOnScreen + 64, (int)helpStringSize.X + IClickableMenu.borderWidth * 2, (int)helpStringSize.Y + IClickableMenu.borderWidth * 2, Color.White);
				Utility.drawTextWithShadow(b, coopHelpString, Game1.dialogueFont, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth - 192, yPositionOnScreen + IClickableMenu.borderWidth + 64), Game1.textColor);
				if (coopHelpOkButton != null)
				{
					coopHelpOkButton.draw(b, Color.White, 0.95f);
				}
				if (coopHelpRightButton != null)
				{
					coopHelpRightButton.draw(b, Color.White, 0.95f);
				}
				if (coopHelpLeftButton != null)
				{
					coopHelpLeftButton.draw(b, Color.White, 0.95f);
				}
				drawMouse(b);
				return;
			}
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe2);
			if (source == Source.HostNewFarm)
			{
				IClickableMenu.drawTextureBox(b, xPositionOnScreen - 256 + 4 - ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 25 : 0), yPositionOnScreen + IClickableMenu.borderWidth * 2 + 68, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 320 : 256, 512, Color.White);
				foreach (ClickableTextureComponent c4 in cabinLayoutButtons)
				{
					c4.draw(b, Color.White * ((Game1.startingCabins > 0) ? 1f : 0.5f), 0.9f);
					if (Game1.startingCabins > 0 && ((c4.name.Equals("Close") && !Game1.cabinsSeparate) || (c4.name.Equals("Separate") && Game1.cabinsSeparate)))
					{
						b.Draw(Game1.mouseCursors, c4.bounds, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34), Color.White);
					}
				}
			}
			b.Draw(Game1.daybg, new Vector2(portraitBox.X, portraitBox.Y), Color.White);
			foreach (ClickableTextureComponent c3 in genderButtons)
			{
				if (c3.visible)
				{
					c3.draw(b);
					if ((c3.name.Equals("Male") && Game1.player.IsMale) || (c3.name.Equals("Female") && !Game1.player.IsMale))
					{
						b.Draw(Game1.mouseCursors, c3.bounds, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34), Color.White);
					}
				}
			}
			foreach (ClickableTextureComponent c2 in petButtons)
			{
				if (c2.visible)
				{
					c2.draw(b);
					if ((c2.name.Equals("Cat") && Game1.player.catPerson) || (c2.name.Equals("Dog") && !Game1.player.catPerson))
					{
						b.Draw(Game1.mouseCursors, c2.bounds, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34), Color.White);
					}
				}
			}
			if (nameBoxCC.visible)
			{
				Game1.player.Name = nameBox.Text;
			}
			if (favThingBoxCC.visible)
			{
				Game1.player.favoriteThing.Value = favThingBox.Text;
			}
			if (farmnameBoxCC.visible)
			{
				Game1.player.farmName.Value = farmnameBox.Text;
			}
			if (source == Source.NewFarmhand)
			{
				Game1.player.farmName.Value = Game1.MasterPlayer.farmName.Value;
			}
			foreach (ClickableTextureComponent leftSelectionButton in leftSelectionButtons)
			{
				leftSelectionButton.draw(b);
			}
			foreach (ClickableComponent c in labels)
			{
				if (c.visible)
				{
					string sub = "";
					float offset = 0f;
					float subYOffset = 0f;
					Color color = Game1.textColor;
					if (c == nameLabel)
					{
						color = ((Game1.player.Name != null && Game1.player.Name.Length < 1) ? Color.Red : Game1.textColor);
					}
					else if (c == farmLabel)
					{
						color = ((Game1.player.farmName.Value != null && Game1.player.farmName.Length < 1) ? Color.Red : Game1.textColor);
					}
					else if (c == favoriteLabel)
					{
						color = ((Game1.player.favoriteThing.Value != null && Game1.player.favoriteThing.Length < 1) ? Color.Red : Game1.textColor);
					}
					else if (c == shirtLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
						sub = string.Concat((int)Game1.player.shirt + 1);
					}
					else if (c == skinLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
						sub = string.Concat((int)Game1.player.skin + 1);
					}
					else if (c == hairLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
						if (!c.name.Contains("Color"))
						{
							sub = string.Concat(Farmer.GetAllHairstyleIndices().IndexOf(Game1.player.hair) + 1);
						}
					}
					else if (c == accLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
						sub = string.Concat((int)Game1.player.accessory + 2);
					}
					else if (c == pantsStyleLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
						sub = string.Concat((int)Game1.player.pants + 1);
					}
					else if (c == startingCabinsLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
						sub = ((Game1.startingCabins == 0 && noneString != null) ? noneString : string.Concat(Game1.startingCabins));
						subYOffset = 4f;
					}
					else if (c == difficultyModifierLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
						subYOffset = 4f;
						sub = getNameOfDifficulty();
					}
					else if (c == separateWalletLabel)
					{
						offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
						subYOffset = 4f;
						sub = (Game1.player.team.useSeparateWallets ? separateWalletString : sharedWalletString);
					}
					else
					{
						color = Game1.textColor;
					}
					Utility.drawTextWithShadow(b, c.name, Game1.smallFont, new Vector2((float)c.bounds.X + offset, c.bounds.Y), color);
					if (sub.Length > 0)
					{
						Utility.drawTextWithShadow(b, sub, Game1.smallFont, new Vector2((float)(c.bounds.X + 21) - Game1.smallFont.MeasureString(sub).X / 2f, (float)(c.bounds.Y + 32) + subYOffset), color);
					}
				}
			}
			foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
			{
				rightSelectionButton.draw(b);
			}
			if (farmTypeButtons.Count > 0)
			{
				IClickableMenu.drawTextureBox(b, farmTypeButtons[0].bounds.X - 16, farmTypeButtons[0].bounds.Y - 20, 120, 652, Color.White);
				for (int i = 0; i < farmTypeButtons.Count; i++)
				{
					farmTypeButtons[i].draw(b, farmTypeButtons[i].name.Contains("Gray") ? (Color.Black * 0.5f) : Color.White, 0.88f);
					if (farmTypeButtons[i].name.Contains("Gray"))
					{
						b.Draw(Game1.mouseCursors, new Vector2(farmTypeButtons[i].bounds.Center.X - 12, farmTypeButtons[i].bounds.Center.Y - 8), new Rectangle(107, 442, 7, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
					}
					if (i == Game1.whichFarm)
					{
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), farmTypeButtons[i].bounds.X, farmTypeButtons[i].bounds.Y - 4, farmTypeButtons[i].bounds.Width, farmTypeButtons[i].bounds.Height + 8, Color.White, 4f, drawShadow: false);
					}
				}
			}
			if (petPortraitBox.HasValue)
			{
				b.Draw(Game1.mouseCursors, petPortraitBox.Value, new Rectangle(160 + ((!Game1.player.catPerson) ? 48 : 0) + Game1.player.whichPetBreed * 16, 208, 16, 16), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.89f);
			}
			if (advancedOptionsButton != null)
			{
				advancedOptionsButton.draw(b);
			}
			if (canLeaveMenu())
			{
				okButton.draw(b, Color.White, 0.75f);
			}
			else
			{
				okButton.draw(b, Color.White, 0.75f);
				okButton.draw(b, Color.Black * 0.5f, 0.751f);
			}
			if (coopHelpButton != null)
			{
				coopHelpButton.draw(b, Color.White, 0.75f);
			}
			if (hairColorPicker != null)
			{
				hairColorPicker.draw(b);
			}
			if (pantsColorPicker != null)
			{
				pantsColorPicker.draw(b);
			}
			if (eyeColorPicker != null)
			{
				eyeColorPicker.draw(b);
			}
			if (source != Source.Dresser && source != Source.DyePots && source != Source.ClothesDye)
			{
				nameBox.Draw(b);
				favThingBox.Draw(b);
			}
			if (farmnameBoxCC.visible)
			{
				farmnameBox.Draw(b);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Character_FarmNameSuffix"), Game1.smallFont, new Vector2(farmnameBox.X + farmnameBox.Width + 8, farmnameBox.Y + 12), Game1.textColor);
			}
			if (skipIntroButton != null && skipIntroButton.visible)
			{
				skipIntroButton.draw(b);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.smallFont, new Vector2(skipIntroButton.bounds.X + skipIntroButton.bounds.Width + 8, skipIntroButton.bounds.Y + 8), Game1.textColor);
			}
			if (advancedCCHighlightTimer > 0f)
			{
				b.Draw(Game1.mouseCursors, advancedOptionsButton.getVector2() + new Vector2(4f, 84f), new Rectangle(128 + ((advancedCCHighlightTimer % 500f < 250f) ? 16 : 0), 208, 16, 16), Color.White * Math.Min(1f, advancedCCHighlightTimer / 500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
			}
			randomButton.draw(b);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			_displayFarmer.FarmerRenderer.draw(b, _displayFarmer.FarmerSprite.CurrentAnimationFrame, _displayFarmer.FarmerSprite.CurrentFrame, _displayFarmer.FarmerSprite.SourceRect, new Vector2(portraitBox.Center.X - 32, portraitBox.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0f, 1f, _displayFarmer);
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			if (hoverText != null && hoverTitle != null && hoverText.Count() > 0)
			{
				IClickableMenu.drawHoverText(b, Game1.parseText(hoverText, Game1.smallFont, 256), Game1.smallFont, 0, 0, -1, hoverTitle);
			}
			drawMouse(b);
		}

		public override void emergencyShutDown()
		{
			if (_itemToDye != null)
			{
				if (!Game1.player.IsEquippedItem(_itemToDye))
				{
					Utility.CollectOrDrop(_itemToDye);
				}
				_itemToDye = null;
			}
			base.emergencyShutDown();
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (a.region != b.region)
			{
				return false;
			}
			if (advancedOptionsButton != null && backButton != null && a == advancedOptionsButton && b == backButton)
			{
				return false;
			}
			if (source == Source.Wizard)
			{
				if (a == favThingBoxCC && b.myID >= 522 && b.myID <= 530)
				{
					return false;
				}
				if (b == favThingBoxCC && a.myID >= 522 && a.myID <= 530)
				{
					return false;
				}
			}
			if (source == Source.Wizard)
			{
				if (a.name == "Direction" && b.name == "Pet")
				{
					return false;
				}
				if (b.name == "Direction" && a.name == "Pet")
				{
					return false;
				}
			}
			if (randomButton != null)
			{
				switch (direction)
				{
				case 3:
					if (b == randomButton && a.name == "Direction")
					{
						return false;
					}
					break;
				default:
					if (a == randomButton && b.name != "Direction")
					{
						return false;
					}
					if (b == randomButton && a.name != "Direction")
					{
						return false;
					}
					break;
				case 0:
					break;
				}
				if (a.myID == 622 && direction == 1 && (b == nameBoxCC || b == favThingBoxCC || b == farmnameBoxCC))
				{
					return false;
				}
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (showingCoopHelp)
			{
				backButton.visible = false;
				if (coopHelpScreen == 0)
				{
					coopHelpRightButton.visible = true;
					coopHelpLeftButton.visible = false;
				}
				else if (coopHelpScreen == 1)
				{
					coopHelpRightButton.visible = false;
					coopHelpLeftButton.visible = true;
				}
			}
			else
			{
				backButton.visible = _shouldShowBackButton;
			}
			if (_sliderOpTarget != null)
			{
				Color col = _sliderOpTarget.getSelectedColor();
				if (_sliderOpTarget.Dirty && _sliderOpTarget.LastColor == col)
				{
					_sliderAction();
					_sliderOpTarget.LastColor = _sliderOpTarget.getSelectedColor();
					_sliderOpTarget.Dirty = false;
					_sliderOpTarget = null;
				}
				else
				{
					_sliderOpTarget.LastColor = col;
				}
			}
			if (advancedCCHighlightTimer > 0f)
			{
				advancedCCHighlightTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			return true;
		}
	}
}
