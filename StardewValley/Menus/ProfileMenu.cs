using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class ProfileMenu : IClickableMenu
	{
		public class ProfileItemCategory
		{
			public string categoryName;

			public int[] validCategories;

			public ProfileItemCategory(string name, int[] valid_categories)
			{
				categoryName = name;
				validCategories = valid_categories;
			}
		}

		public const int region_characterSelectors = 500;

		public const int region_categorySelector = 501;

		public const int region_itemButtons = 502;

		public const int region_backButton = 101;

		public const int region_forwardButton = 102;

		public const int region_upArrow = 105;

		public const int region_downArrow = 106;

		public const int region_scrollButtons = 107;

		public const int letterWidth = 320;

		public const int letterHeight = 180;

		public Texture2D letterTexture;

		public Texture2D secretNoteImageTexture;

		protected string hoverText = "";

		protected List<ProfileItem> _profileItems;

		protected Character _target;

		public Item hoveredItem;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent nextCharacterButton;

		public ClickableTextureComponent previousCharacterButton;

		protected Rectangle characterSpriteBox;

		protected int _currentCategory;

		protected AnimatedSprite _animatedSprite;

		protected float _directionChangeTimer;

		protected float _hiddenEmoteTimer = -1f;

		protected int _currentDirection;

		protected int _hideTooltipTime;

		protected SocialPage _socialPage;

		protected string _status = "";

		protected string _printedName = "";

		protected Vector2 _characterEntrancePosition = new Vector2(0f, 0f);

		public ClickableTextureComponent upArrow;

		public ClickableTextureComponent downArrow;

		protected ClickableTextureComponent scrollBar;

		protected Rectangle scrollBarRunner;

		public List<ClickableComponent> clickableProfileItems;

		protected List<Character> _charactersList;

		protected Friendship _friendship;

		protected Vector2 _characterNamePosition;

		protected Vector2 _heartDisplayPosition;

		protected Vector2 _birthdayHeadingDisplayPosition;

		protected Vector2 _birthdayDisplayPosition;

		protected Vector2 _statusHeadingDisplayPosition;

		protected Vector2 _statusDisplayPosition;

		protected Vector2 _giftLogHeadingDisplayPosition;

		protected Vector2 _giftLogCategoryDisplayPosition;

		protected Vector2 _errorMessagePosition;

		protected Vector2 _characterSpriteDrawPosition;

		protected Rectangle _characterStatusDisplayBox;

		protected List<ClickableTextureComponent> _clickableTextureComponents;

		public Rectangle _itemDisplayRect;

		protected int scrollPosition;

		protected int scrollStep = 36;

		protected int scrollSize;

		public static ProfileItemCategory[] itemCategories = new ProfileItemCategory[10]
		{
			new ProfileItemCategory("Profile_Gift_Category_LikedGifts", null),
			new ProfileItemCategory("Profile_Gift_Category_FruitsAndVegetables", new int[2]
			{
				-75,
				-79
			}),
			new ProfileItemCategory("Profile_Gift_Category_AnimalProduce", new int[4]
			{
				-6,
				-5,
				-14,
				-18
			}),
			new ProfileItemCategory("Profile_Gift_Category_ArtisanItems", new int[1]
			{
				-26
			}),
			new ProfileItemCategory("Profile_Gift_Category_CookedItems", new int[1]
			{
				-7
			}),
			new ProfileItemCategory("Profile_Gift_Category_ForagedItems", new int[4]
			{
				-80,
				-81,
				-23,
				-17
			}),
			new ProfileItemCategory("Profile_Gift_Category_Fish", new int[1]
			{
				-4
			}),
			new ProfileItemCategory("Profile_Gift_Category_Ingredients", new int[2]
			{
				-27,
				-25
			}),
			new ProfileItemCategory("Profile_Gift_Category_MineralsAndGems", new int[3]
			{
				-15,
				-12,
				-2
			}),
			new ProfileItemCategory("Profile_Gift_Category_Misc", null)
		};

		protected Dictionary<int, List<Item>> _sortedItems;

		public bool scrolling;

		private int _characterSpriteRandomInt;

		public ProfileMenu(Character character)
			: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
		{
			_charactersList = new List<Character>();
			_socialPage = new SocialPage(0, 0, 0, 0);
			_printedName = "";
			_characterEntrancePosition = new Vector2(0f, 4f);
			foreach (object name in _socialPage.names)
			{
				if (!(name is long) && name is string)
				{
					NPC npc = Game1.getCharacterFromName((string)name);
					if (npc != null && Game1.player.friendshipData.ContainsKey(npc.name))
					{
						_charactersList.Add(npc);
					}
				}
			}
			_profileItems = new List<ProfileItem>();
			clickableProfileItems = new List<ClickableComponent>();
			UpdateButtons();
			letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
			_SetCharacter(character);
		}

		public Character GetCharacter()
		{
			return _target;
		}

		public NPC GetTemporaryCharacter(Character character)
		{
			Texture2D portrait2 = null;
			try
			{
				portrait2 = Game1.content.Load<Texture2D>("Portraits\\" + (character as NPC).getTextureName());
			}
			catch (Exception)
			{
				return null;
			}
			int height = (character.name.Contains("Dwarf") || character.name.Equals("Krobus")) ? 96 : 128;
			return new NPC(new AnimatedSprite("Characters\\" + (character as NPC).getTextureName(), 0, 16, height / 4), new Vector2(0f, 0f), character.Name, character.facingDirection, character.name, null, portrait2, eventActor: true);
		}

		protected void _SetCharacter(Character character)
		{
			_target = character;
			_sortedItems = new Dictionary<int, List<Item>>();
			if (_target is NPC)
			{
				_friendship = _socialPage.getFriendship(_target.name);
				NPC npc = GetTemporaryCharacter(_target);
				_animatedSprite = npc.Sprite.Clone();
				_animatedSprite.tempSpriteHeight = -1;
				_animatedSprite.faceDirection(2);
				foreach (int item_index in Game1.objectInformation.Keys)
				{
					Object item = new Object(item_index, 1);
					if (Game1.player.hasGiftTasteBeenRevealed(npc, item_index) && (!(item.Name == "Stone") || item_index == 390))
					{
						for (int j = 0; j < itemCategories.Length; j++)
						{
							if (itemCategories[j].categoryName == "Profile_Gift_Category_LikedGifts")
							{
								int gift_taste = npc.getGiftTasteForThisItem(item);
								if (gift_taste == 2 || gift_taste == 0)
								{
									if (!_sortedItems.ContainsKey(j))
									{
										_sortedItems[j] = new List<Item>();
									}
									_sortedItems[j].Add(item);
								}
							}
							else if (itemCategories[j].categoryName == "Profile_Gift_Category_Misc")
							{
								bool is_accounted_for = false;
								for (int i = 0; i < itemCategories.Length; i++)
								{
									if (itemCategories[i].validCategories != null && itemCategories[i].validCategories.Contains(item.Category))
									{
										is_accounted_for = true;
										break;
									}
								}
								if (!is_accounted_for)
								{
									npc.getGiftTasteForThisItem(item);
									if (!_sortedItems.ContainsKey(j))
									{
										_sortedItems[j] = new List<Item>();
									}
									_sortedItems[j].Add(item);
								}
							}
							else if (itemCategories[j].validCategories.Contains(item.Category))
							{
								if (!_sortedItems.ContainsKey(j))
								{
									_sortedItems[j] = new List<Item>();
								}
								_sortedItems[j].Add(item);
							}
						}
					}
				}
				bool num = SocialPage.isDatable(_target.name);
				bool spouse = _friendship.IsMarried();
				bool housemate = spouse && SocialPage.isRoommateOfAnyone(_target.name);
				_status = "";
				if (num | housemate)
				{
					string text2 = (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.pt) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').First() : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last());
					if (housemate)
					{
						text2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Housemate");
					}
					else if (spouse)
					{
						text2 = ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
					}
					else if (_socialPage.isMarriedToAnyone(_target.name))
					{
						text2 = ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
					}
					else if (!Game1.player.isMarried() && _friendship.IsDating())
					{
						text2 = ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
					}
					else if (_socialPage.getFriendship(_target.name).IsDivorced())
					{
						text2 = ((_socialPage.getGender(_target.name) == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
					}
					text2 = Game1.parseText(text2, Game1.smallFont, width);
					string status2 = text2.Replace("(", "").Replace(")", "").Replace("（", "")
						.Replace("）", "");
					status2 = (_status = Utility.capitalizeFirstLetter(status2));
				}
				_UpdateList();
			}
			_directionChangeTimer = 2000f;
			_currentDirection = 2;
			_hiddenEmoteTimer = -1f;
		}

		public void ChangeCharacter(int offset)
		{
			int index2 = _charactersList.IndexOf(_target);
			if (index2 == -1)
			{
				if (_charactersList.Count > 0)
				{
					_SetCharacter(_charactersList[0]);
				}
				return;
			}
			for (index2 += offset; index2 < 0; index2 += _charactersList.Count)
			{
			}
			while (index2 >= _charactersList.Count)
			{
				index2 -= _charactersList.Count;
			}
			_SetCharacter(_charactersList[index2]);
			Game1.playSound("smallSelect");
			_printedName = "";
			_characterEntrancePosition = new Vector2(Math.Sign(offset) * -4, 0f);
			if (Game1.options.SnappyMenus && (currentlySnappedComponent == null || !currentlySnappedComponent.visible))
			{
				snapToDefaultClickableComponent();
			}
		}

		protected void _UpdateList()
		{
			for (int i = 0; i < _profileItems.Count; i++)
			{
				_profileItems[i].Unload();
			}
			_profileItems.Clear();
			if (_target is NPC)
			{
				NPC npc = _target as NPC;
				List<Item> loved_items = new List<Item>();
				List<Item> liked_items = new List<Item>();
				List<Item> neutral_items = new List<Item>();
				List<Item> disliked_items = new List<Item>();
				List<Item> hated_items = new List<Item>();
				if (_sortedItems.ContainsKey(_currentCategory))
				{
					foreach (Item item in _sortedItems[_currentCategory])
					{
						switch (npc.getGiftTasteForThisItem(item))
						{
						case 0:
							loved_items.Add(item);
							break;
						case 2:
							liked_items.Add(item);
							break;
						case 8:
							neutral_items.Add(item);
							break;
						case 4:
							disliked_items.Add(item);
							break;
						case 6:
							hated_items.Add(item);
							break;
						}
					}
				}
				PI_ItemList item_display5 = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Loved"), loved_items);
				_profileItems.Add(item_display5);
				item_display5 = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Liked"), liked_items);
				_profileItems.Add(item_display5);
				item_display5 = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Neutral"), neutral_items);
				_profileItems.Add(item_display5);
				item_display5 = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Disliked"), disliked_items);
				_profileItems.Add(item_display5);
				item_display5 = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Hated"), hated_items);
				_profileItems.Add(item_display5);
				SetupLayout();
				populateClickableComponentList();
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && (currentlySnappedComponent == null || !allClickableComponents.Contains(currentlySnappedComponent)))
				{
					snapToDefaultClickableComponent();
				}
			}
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (direction == 2 && a.region == 501 && b.region == 500)
			{
				return false;
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		public override void snapToDefaultClickableComponent()
		{
			if (clickableProfileItems.Count > 0)
			{
				currentlySnappedComponent = clickableProfileItems[0];
			}
			else
			{
				currentlySnappedComponent = backButton;
			}
			snapCursorToCurrentSnappedComponent();
		}

		public void UpdateButtons()
		{
			_clickableTextureComponents = new List<ClickableTextureComponent>();
			upArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
			{
				myID = 105,
				upNeighborID = 102,
				upNeighborImmutable = true,
				downNeighborID = 106,
				downNeighborImmutable = true,
				leftNeighborID = -99998,
				leftNeighborImmutable = true
			};
			downArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
			{
				myID = 106,
				upNeighborID = 105,
				upNeighborImmutable = true,
				leftNeighborID = -99998,
				leftNeighborImmutable = true
			};
			scrollBar = new ClickableTextureComponent(new Rectangle(0, 0, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 101,
				name = "Back Button",
				upNeighborID = -99998,
				downNeighborID = -99998,
				downNeighborImmutable = true,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 501
			};
			_clickableTextureComponents.Add(backButton);
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				name = "Forward Button",
				upNeighborID = -99998,
				downNeighborID = -99998,
				downNeighborImmutable = true,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 501
			};
			_clickableTextureComponents.Add(forwardButton);
			previousCharacterButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 0,
				name = "Previous Char",
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 500
			};
			_clickableTextureComponents.Add(previousCharacterButton);
			nextCharacterButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 0,
				name = "Next Char",
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 500
			};
			_clickableTextureComponents.Add(nextCharacterButton);
			_clickableTextureComponents.Add(upArrow);
			_clickableTextureComponents.Add(downArrow);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0)
			{
				Scroll(-scrollStep);
			}
			else if (direction < 0)
			{
				Scroll(scrollStep);
			}
		}

		public void ChangePage(int offset)
		{
			scrollPosition = 0;
			_currentCategory += offset;
			while (_currentCategory < 0)
			{
				_currentCategory += itemCategories.Length;
			}
			while (_currentCategory >= itemCategories.Length)
			{
				_currentCategory -= itemCategories.Length;
			}
			Game1.playSound("shwip");
			_UpdateList();
			if (Game1.options.SnappyMenus && (currentlySnappedComponent == null || !currentlySnappedComponent.visible))
			{
				snapToDefaultClickableComponent();
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X;
			yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y;
			UpdateButtons();
			SetupLayout();
			initializeUpperRightCloseButton();
			populateClickableComponentList();
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			switch (b)
			{
			case Buttons.LeftTrigger:
				ChangePage(-1);
				break;
			case Buttons.RightTrigger:
				ChangePage(1);
				break;
			case Buttons.RightShoulder:
				ChangeCharacter(1);
				break;
			case Buttons.LeftShoulder:
				ChangeCharacter(-1);
				break;
			case Buttons.Back:
				PlayHiddenEmote();
				break;
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (key != 0)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
				{
					exitThisMenu();
				}
				else if (Game1.options.snappyMenus && Game1.options.gamepadControls && !overrideSnappyMenuCursorMovementBan())
				{
					applyMovementKey(key);
				}
			}
		}

		public override void applyMovementKey(int direction)
		{
			base.applyMovementKey(direction);
			ConstrainSelectionToView();
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			scrolling = false;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (scrollBar.containsPoint(x, y))
			{
				scrolling = true;
			}
			else if (scrollBarRunner.Contains(x, y))
			{
				scrolling = true;
				leftClickHeld(x, y);
				releaseLeftClick(x, y);
			}
			if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
			{
				exitThisMenu();
				return;
			}
			if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
			{
				unload();
				return;
			}
			if (backButton.containsPoint(x, y))
			{
				ChangePage(-1);
				return;
			}
			if (forwardButton.containsPoint(x, y))
			{
				ChangePage(1);
				return;
			}
			if (previousCharacterButton.containsPoint(x, y))
			{
				ChangeCharacter(-1);
				return;
			}
			if (nextCharacterButton.containsPoint(x, y))
			{
				ChangeCharacter(1);
				return;
			}
			if (downArrow.containsPoint(x, y))
			{
				Scroll(scrollStep);
			}
			if (upArrow.containsPoint(x, y))
			{
				Scroll(-scrollStep);
			}
			if (characterSpriteBox.Contains(x, y))
			{
				PlayHiddenEmote();
			}
		}

		public void PlayHiddenEmote()
		{
			if (GetCharacter() == null)
			{
				return;
			}
			string name = GetCharacter().name;
			if (Game1.player.getFriendshipHeartLevelForNPC(GetCharacter().name) >= 4)
			{
				_currentDirection = 2;
				_characterSpriteRandomInt = Game1.random.Next(4);
				if (name == "Leo")
				{
					if (_hiddenEmoteTimer <= 0f)
					{
						Game1.playSound("parrot_squawk");
						_hiddenEmoteTimer = 300f;
					}
				}
				else
				{
					Game1.playSound("drumkit6");
					_hiddenEmoteTimer = 4000f;
				}
			}
			else
			{
				_currentDirection = 2;
				_directionChangeTimer = 5000f;
				Game1.playSound("Cowboy_Footstep");
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			hoveredItem = null;
			if (_itemDisplayRect.Contains(x, y))
			{
				foreach (ProfileItem profileItem in _profileItems)
				{
					profileItem.performHover(x, y);
				}
			}
			upArrow.tryHover(x, y);
			downArrow.tryHover(x, y);
			backButton.tryHover(x, y, 0.6f);
			forwardButton.tryHover(x, y, 0.6f);
			nextCharacterButton.tryHover(x, y, 0.6f);
			previousCharacterButton.tryHover(x, y, 0.6f);
		}

		public void ConstrainSelectionToView()
		{
			if (!Game1.options.snappyMenus)
			{
				return;
			}
			if (currentlySnappedComponent != null && currentlySnappedComponent.region == 502 && !_itemDisplayRect.Contains(currentlySnappedComponent.bounds))
			{
				if (currentlySnappedComponent.bounds.Bottom > _itemDisplayRect.Bottom)
				{
					int scroll2 = (int)Math.Ceiling(((double)currentlySnappedComponent.bounds.Bottom - (double)_itemDisplayRect.Bottom) / (double)scrollStep) * scrollStep;
					Scroll(scroll2);
				}
				else if (currentlySnappedComponent.bounds.Top < _itemDisplayRect.Top)
				{
					int scroll = (int)Math.Floor(((double)currentlySnappedComponent.bounds.Top - (double)_itemDisplayRect.Top) / (double)scrollStep) * scrollStep;
					Scroll(scroll);
				}
			}
			if (scrollPosition <= scrollStep)
			{
				scrollPosition = 0;
				UpdateScroll();
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (_target != null && _target.displayName != null && _printedName.Length < _target.displayName.Length)
			{
				_printedName += _target.displayName[_printedName.Length].ToString();
			}
			if (_hideTooltipTime > 0)
			{
				_hideTooltipTime -= time.ElapsedGameTime.Milliseconds;
				if (_hideTooltipTime < 0)
				{
					_hideTooltipTime = 0;
				}
			}
			if (_characterEntrancePosition.X != 0f)
			{
				_characterEntrancePosition.X -= (float)Math.Sign(_characterEntrancePosition.X) * 0.25f;
			}
			if (_characterEntrancePosition.Y != 0f)
			{
				_characterEntrancePosition.Y -= (float)Math.Sign(_characterEntrancePosition.Y) * 0.25f;
			}
			if (_animatedSprite == null)
			{
				return;
			}
			if (_hiddenEmoteTimer > 0f)
			{
				_hiddenEmoteTimer -= time.ElapsedGameTime.Milliseconds;
				if (_hiddenEmoteTimer <= 0f)
				{
					_hiddenEmoteTimer = -1f;
					_currentDirection = 2;
					_directionChangeTimer = 2000f;
					if (GetCharacter() != null && (string)GetCharacter().name == "Leo")
					{
						GetCharacter().Sprite.AnimateDown(time);
					}
				}
			}
			else if (_directionChangeTimer > 0f)
			{
				_directionChangeTimer -= time.ElapsedGameTime.Milliseconds;
				if (_directionChangeTimer <= 0f)
				{
					_directionChangeTimer = 2000f;
					_currentDirection = (_currentDirection + 1) % 4;
				}
			}
			if (_characterEntrancePosition != Vector2.Zero)
			{
				if (_characterEntrancePosition.X < 0f)
				{
					_animatedSprite.AnimateRight(time, 2);
				}
				else if (_characterEntrancePosition.X > 0f)
				{
					_animatedSprite.AnimateLeft(time, 2);
				}
				else if (_characterEntrancePosition.Y > 0f)
				{
					_animatedSprite.AnimateUp(time, 2);
				}
				else if (_characterEntrancePosition.Y < 0f)
				{
					_animatedSprite.AnimateDown(time, 2);
				}
			}
			else if (_hiddenEmoteTimer > 0f)
			{
				switch ((string)GetCharacter().name)
				{
				case "Abigail":
					_animatedSprite.Animate(time, 16, 4, 200f);
					break;
				case "Penny":
					_animatedSprite.Animate(time, 18, 2, 1000f);
					break;
				case "Maru":
					_animatedSprite.Animate(time, 16, 8, 150f);
					break;
				case "Leah":
					_animatedSprite.Animate(time, 16, 4, 200f);
					break;
				case "Haley":
					_animatedSprite.Animate(time, 26, 1, 200f);
					break;
				case "Emily":
					_animatedSprite.Animate(time, 16 + _characterSpriteRandomInt * 2, 2, 300f);
					break;
				case "Sam":
					_animatedSprite.Animate(time, 20, 2, 300f);
					break;
				case "Sebastian":
					_animatedSprite.Animate(time, 16, 8, 180f);
					break;
				case "Shane":
					_animatedSprite.Animate(time, 28, 2, 500f);
					break;
				case "Elliott":
					_animatedSprite.Animate(time, 33, 2, 800f);
					break;
				case "Harvey":
					_animatedSprite.Animate(time, 20, 2, 800f);
					break;
				case "Alex":
					_animatedSprite.Animate(time, 16, 8, 170f);
					break;
				case "Lewis":
					_animatedSprite.Animate(time, 24, 1, 170f);
					break;
				case "Wizard":
					_animatedSprite.Animate(time, 16, 1, 170f);
					break;
				case "Willy":
					_animatedSprite.Animate(time, 28, 4, 200f);
					break;
				case "Vincent":
					_animatedSprite.Animate(time, 18, 2, 600f);
					break;
				case "Robin":
					_animatedSprite.Animate(time, 32, 2, 120f);
					break;
				case "Marnie":
					_animatedSprite.Animate(time, 28, 4, 120f);
					break;
				case "Linus":
					_animatedSprite.Animate(time, 22, 1, 200f);
					break;
				case "Kent":
					_animatedSprite.Animate(time, 16, 1, 200f);
					break;
				case "Jodi":
					_animatedSprite.Animate(time, 16, 2, 200f);
					break;
				case "Jas":
					_animatedSprite.Animate(time, 16, 4, 100f);
					break;
				case "Gus":
					_animatedSprite.Animate(time, 18, 3, 200f);
					break;
				case "George":
					_animatedSprite.Animate(time, 16, 4, 400f);
					break;
				case "Demetrius":
					_animatedSprite.Animate(time, 30, 2, 200f);
					break;
				case "Caroline":
					_animatedSprite.Animate(time, 19, 1, 200f);
					break;
				case "Pierre":
					_animatedSprite.Animate(time, 23, 1, 200f);
					break;
				case "Krobus":
					_animatedSprite.Animate(time, 20, 4, 200f);
					break;
				case "Evelyn":
					_animatedSprite.Animate(time, 20, 1, 200f);
					break;
				case "Clint":
					_animatedSprite.Animate(time, 39, 1, 200f);
					break;
				case "Dwarf":
					_animatedSprite.Animate(time, 16, 4, 100f);
					break;
				case "Pam":
					_animatedSprite.Animate(time, 28, 2, 200f);
					break;
				case "Sandy":
					_animatedSprite.Animate(time, 16, 2, 200f);
					break;
				case "Leo":
					_animatedSprite.Animate(time, 17, 1, 200f);
					break;
				default:
					_animatedSprite.AnimateDown(time, 2);
					break;
				}
			}
			else
			{
				switch (_currentDirection)
				{
				case 0:
					_animatedSprite.AnimateUp(time, 2);
					break;
				case 2:
					_animatedSprite.AnimateDown(time, 2);
					break;
				case 3:
					_animatedSprite.AnimateLeft(time, 2);
					break;
				case 1:
					_animatedSprite.AnimateRight(time, 2);
					break;
				}
			}
		}

		public void SetupLayout()
		{
			int x = xPositionOnScreen + 64 - 12;
			int y = yPositionOnScreen + IClickableMenu.borderWidth;
			Rectangle left_pane_rectangle = new Rectangle(x, y, 400, 720 - IClickableMenu.borderWidth * 2);
			Rectangle content_rectangle = new Rectangle(x, y, 1204, 720 - IClickableMenu.borderWidth * 2);
			content_rectangle.X += left_pane_rectangle.Width;
			content_rectangle.Width -= left_pane_rectangle.Width;
			_characterStatusDisplayBox = new Rectangle(left_pane_rectangle.X, left_pane_rectangle.Y, left_pane_rectangle.Width, left_pane_rectangle.Height);
			left_pane_rectangle.Y += 32;
			left_pane_rectangle.Height -= 32;
			_characterSpriteDrawPosition = new Vector2(left_pane_rectangle.X + (left_pane_rectangle.Width - Game1.nightbg.Width) / 2, left_pane_rectangle.Y);
			characterSpriteBox = new Rectangle(xPositionOnScreen + 64 - 12 + (400 - Game1.nightbg.Width) / 2, yPositionOnScreen + IClickableMenu.borderWidth, Game1.nightbg.Width, Game1.nightbg.Height);
			previousCharacterButton.bounds.X = (int)_characterSpriteDrawPosition.X - 64 - previousCharacterButton.bounds.Width / 2;
			previousCharacterButton.bounds.Y = (int)_characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - previousCharacterButton.bounds.Height / 2;
			nextCharacterButton.bounds.X = (int)_characterSpriteDrawPosition.X + Game1.nightbg.Width + 64 - nextCharacterButton.bounds.Width / 2;
			nextCharacterButton.bounds.Y = (int)_characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - nextCharacterButton.bounds.Height / 2;
			left_pane_rectangle.Y += Game1.daybg.Height + 32;
			left_pane_rectangle.Height -= Game1.daybg.Height + 32;
			_characterNamePosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
			left_pane_rectangle.Y += 96;
			left_pane_rectangle.Height -= 96;
			_heartDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
			if (_target is NPC)
			{
				left_pane_rectangle.Y += 56;
				left_pane_rectangle.Height -= 48;
				_birthdayHeadingDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
				if ((_target as NPC).birthday_Season.Value != null)
				{
					int season_number = Utility.getSeasonNumber((_target as NPC).birthday_Season);
					if (season_number >= 0)
					{
						left_pane_rectangle.Y += 48;
						left_pane_rectangle.Height -= 48;
						_birthdayDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
						_ = (_target as NPC).Birthday_Day + " " + Utility.getSeasonNameFromNumber(season_number);
						left_pane_rectangle.Y += 64;
						left_pane_rectangle.Height -= 64;
					}
				}
				if (_status != "")
				{
					_statusHeadingDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
					left_pane_rectangle.Y += 48;
					left_pane_rectangle.Height -= 48;
					_statusDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
					left_pane_rectangle.Y += 64;
					left_pane_rectangle.Height -= 64;
				}
			}
			content_rectangle.Height -= 96;
			content_rectangle.Y -= 8;
			_giftLogHeadingDisplayPosition = new Vector2(content_rectangle.Center.X, content_rectangle.Top);
			content_rectangle.Y += 80;
			content_rectangle.Height -= 70;
			backButton.bounds.X = content_rectangle.Left + 64 - forwardButton.bounds.Width / 2;
			backButton.bounds.Y = content_rectangle.Top;
			forwardButton.bounds.X = content_rectangle.Right - 64 - forwardButton.bounds.Width / 2;
			forwardButton.bounds.Y = content_rectangle.Top;
			content_rectangle.Width -= 250;
			content_rectangle.X += 125;
			_giftLogCategoryDisplayPosition = new Vector2(content_rectangle.Center.X, content_rectangle.Top);
			content_rectangle.Y += 64;
			content_rectangle.Y += 32;
			content_rectangle.Height -= 32;
			_itemDisplayRect = content_rectangle;
			int scroll_inset = 64;
			scrollBarRunner = new Rectangle(content_rectangle.Right + 48, content_rectangle.Top + scroll_inset, scrollBar.bounds.Width, content_rectangle.Height - scroll_inset * 2);
			downArrow.bounds.Y = scrollBarRunner.Bottom + 16;
			downArrow.bounds.X = scrollBarRunner.Center.X - downArrow.bounds.Width / 2;
			upArrow.bounds.Y = scrollBarRunner.Top - 16 - upArrow.bounds.Height;
			upArrow.bounds.X = scrollBarRunner.Center.X - upArrow.bounds.Width / 2;
			float draw_y = 0f;
			if (_profileItems.Count > 0)
			{
				int drawn_index = 0;
				for (int i = 0; i < _profileItems.Count; i++)
				{
					ProfileItem profile_item = _profileItems[i];
					if (profile_item.ShouldDraw())
					{
						draw_y = profile_item.HandleLayout(draw_y, _itemDisplayRect, drawn_index);
						drawn_index++;
					}
				}
			}
			scrollSize = (int)draw_y - _itemDisplayRect.Height;
			if (NeedsScrollBar())
			{
				upArrow.visible = true;
				downArrow.visible = true;
			}
			else
			{
				upArrow.visible = false;
				downArrow.visible = false;
			}
			UpdateScroll();
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
				int num = scrollPosition;
				Console.WriteLine((float)(y - scrollBarRunner.Top) / (float)scrollBarRunner.Height);
				scrollPosition = (int)Math.Round((float)(y - scrollBarRunner.Top) / (float)scrollBarRunner.Height * (float)scrollSize / (float)scrollStep) * scrollStep;
				UpdateScroll();
				if (num != scrollPosition)
				{
					Game1.playSound("shiny4");
				}
			}
		}

		public bool NeedsScrollBar()
		{
			return scrollSize > 0;
		}

		public void Scroll(int offset)
		{
			if (NeedsScrollBar())
			{
				int num = scrollPosition;
				scrollPosition += offset;
				UpdateScroll();
				if (num != scrollPosition)
				{
					Game1.playSound("shwip");
				}
			}
		}

		public virtual void UpdateScroll()
		{
			scrollPosition = Utility.Clamp(scrollPosition, 0, scrollSize);
			float draw_y = _itemDisplayRect.Top - scrollPosition;
			_errorMessagePosition = new Vector2(_itemDisplayRect.Center.X, _itemDisplayRect.Center.Y);
			if (_profileItems.Count > 0)
			{
				int drawn_index = 0;
				for (int i = 0; i < _profileItems.Count; i++)
				{
					ProfileItem profile_item = _profileItems[i];
					if (profile_item.ShouldDraw())
					{
						draw_y = profile_item.HandleLayout(draw_y, _itemDisplayRect, drawn_index);
						drawn_index++;
					}
				}
			}
			if (scrollSize > 0)
			{
				scrollBar.bounds.X = scrollBarRunner.Center.X - scrollBar.bounds.Width / 2;
				scrollBar.bounds.Y = (int)Utility.Lerp(scrollBarRunner.Top, scrollBarRunner.Bottom - scrollBar.bounds.Height, (float)scrollPosition / (float)scrollSize);
				if (Game1.options.SnappyMenus)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			b.Draw(letterTexture, new Vector2(xPositionOnScreen + width / 2, yPositionOnScreen + height / 2), new Rectangle(0, 0, 320, 180), Color.White, 0f, new Vector2(160f, 90f), 4f, SpriteEffects.None, 0.86f);
			Game1.DrawBox(_characterStatusDisplayBox.X, _characterStatusDisplayBox.Y, _characterStatusDisplayBox.Width, _characterStatusDisplayBox.Height);
			b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, _characterSpriteDrawPosition, Color.White);
			Vector2 character_position_offset = new Vector2(0f, (32 - _animatedSprite.SpriteHeight) * 4);
			character_position_offset += _characterEntrancePosition * 4f;
			if (!(_target is Farmer) && _target is NPC)
			{
				_animatedSprite.draw(b, new Vector2(_characterSpriteDrawPosition.X + 32f + character_position_offset.X, _characterSpriteDrawPosition.Y + 32f + character_position_offset.Y), 0.8f);
				int heartLevel = Game1.player.getFriendshipHeartLevelForNPC(_target.name);
				bool datable = SocialPage.isDatable(_target.name);
				bool spouse = _friendship.IsMarried();
				if (spouse)
				{
					SocialPage.isRoommateOfAnyone(_target.name);
				}
				else
					_ = 0;
				int drawn_hearts = Math.Max(10, Utility.GetMaximumHeartsForCharacter(_target));
				float heart_draw_start_x = _heartDisplayPosition.X - (float)(Math.Min(10, drawn_hearts) * 32 / 2);
				float heart_draw_offset_y = (drawn_hearts > 10) ? (-16f) : 0f;
				for (int hearts = 0; hearts < drawn_hearts; hearts++)
				{
					int xSource = (hearts < heartLevel) ? 211 : 218;
					if (datable && !_friendship.IsDating() && !spouse && hearts >= 8)
					{
						xSource = 211;
					}
					if (hearts < 10)
					{
						b.Draw(Game1.mouseCursors, new Vector2(heart_draw_start_x + (float)(hearts * 32), _heartDisplayPosition.Y + heart_draw_offset_y), new Rectangle(xSource, 428, 7, 6), (datable && !_friendship.IsDating() && !spouse && hearts >= 8) ? (Color.Black * 0.35f) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, new Vector2(heart_draw_start_x + (float)((hearts - 10) * 32), _heartDisplayPosition.Y + heart_draw_offset_y + 32f), new Rectangle(xSource, 428, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
					}
				}
			}
			if (_printedName.Length < _target.displayName.Length)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, "", (int)_characterNamePosition.X, (int)_characterNamePosition.Y, _printedName);
			}
			else
			{
				SpriteText.drawStringWithScrollCenteredAt(b, _target.displayName, (int)_characterNamePosition.X, (int)_characterNamePosition.Y);
			}
			if ((_target as NPC).birthday_Season.Value != null)
			{
				int season_number = Utility.getSeasonNumber((_target as NPC).birthday_Season);
				if (season_number >= 0)
				{
					SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_Birthday"), (int)_birthdayHeadingDisplayPosition.X, (int)_birthdayHeadingDisplayPosition.Y);
					string birthday = (_target as NPC).Birthday_Day + " " + Utility.getSeasonNameFromNumber(season_number);
					b.DrawString(Game1.dialogueFont, birthday, new Vector2((0f - Game1.dialogueFont.MeasureString(birthday).X) / 2f + _birthdayDisplayPosition.X, _birthdayDisplayPosition.Y), Game1.textColor);
				}
				if (_status != "")
				{
					SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_Status"), (int)_statusHeadingDisplayPosition.X, (int)_statusHeadingDisplayPosition.Y);
					b.DrawString(Game1.dialogueFont, _status, new Vector2((0f - Game1.dialogueFont.MeasureString(_status).X) / 2f + _statusDisplayPosition.X, _statusDisplayPosition.Y), Game1.textColor);
				}
			}
			SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_GiftLog"), (int)_giftLogHeadingDisplayPosition.X, (int)_giftLogHeadingDisplayPosition.Y);
			SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:" + itemCategories[_currentCategory].categoryName, _target.displayName), (int)_giftLogCategoryDisplayPosition.X, (int)_giftLogCategoryDisplayPosition.Y);
			bool drew_items = false;
			b.End();
			Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
			b.GraphicsDevice.ScissorRectangle = _itemDisplayRect;
			if (_profileItems.Count > 0)
			{
				for (int i = 0; i < _profileItems.Count; i++)
				{
					ProfileItem profile_item = _profileItems[i];
					if (profile_item.ShouldDraw())
					{
						drew_items = true;
						profile_item.Draw(b);
					}
				}
			}
			b.End();
			b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			if (NeedsScrollBar())
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
				scrollBar.draw(b);
			}
			if (!drew_items)
			{
				string error_string = Game1.content.LoadString("Strings\\UI:Profile_GiftLog_NoGiftsGiven");
				b.DrawString(Game1.smallFont, error_string, new Vector2((0f - Game1.smallFont.MeasureString(error_string).X) / 2f + _errorMessagePosition.X, _errorMessagePosition.Y), Game1.textColor);
			}
			foreach (ClickableTextureComponent clickableTextureComponent in _clickableTextureComponents)
			{
				clickableTextureComponent.draw(b);
			}
			base.draw(b);
			drawMouse(b, ignore_transparency: true);
			if (hoveredItem != null)
			{
				bool draw_tooltip = true;
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse && _hideTooltipTime > 0)
				{
					draw_tooltip = false;
				}
				if (draw_tooltip)
				{
					IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
				}
			}
		}

		public void unload()
		{
			_socialPage = null;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			receiveLeftClick(x, y, playSound);
		}

		public void RegisterClickable(ClickableComponent clickable)
		{
			clickableProfileItems.Add(clickable);
		}

		public void UnregisterClickable(ClickableComponent clickable)
		{
			clickableProfileItems.Remove(clickable);
		}
	}
}
