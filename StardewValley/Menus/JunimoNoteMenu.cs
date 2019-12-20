using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class JunimoNoteMenu : IClickableMenu
	{
		public const int region_ingredientSlotModifier = 250;

		public const int region_ingredientListModifier = 1000;

		public const int region_bundleModifier = 5000;

		public const int region_areaNextButton = 101;

		public const int region_areaBackButton = 102;

		public const int region_backButton = 103;

		public const int region_purchaseButton = 104;

		public const int region_presentButton = 105;

		public const string noteTextureName = "LooseSprites\\JunimoNote";

		public Texture2D noteTexture;

		private bool specificBundlePage;

		public const int baseWidth = 320;

		public const int baseHeight = 180;

		public InventoryMenu inventory;

		private Item heldItem;

		private Item hoveredItem;

		public static bool canClick = true;

		private int whichArea;

		public bool bundlesChanged;

		public static ScreenSwipe screenSwipe;

		public static string hoverText = "";

		public List<Bundle> bundles = new List<Bundle>();

		public static List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();

		public List<ClickableTextureComponent> ingredientSlots = new List<ClickableTextureComponent>();

		public List<ClickableTextureComponent> ingredientList = new List<ClickableTextureComponent>();

		public List<ClickableTextureComponent> otherClickableComponents = new List<ClickableTextureComponent>();

		public bool fromGameMenu;

		public bool fromThisMenu;

		public bool scrambledText;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent purchaseButton;

		public ClickableTextureComponent areaNextButton;

		public ClickableTextureComponent areaBackButton;

		public ClickableAnimatedComponent presentButton;

		private Bundle currentPageBundle;

		public JunimoNoteMenu(bool fromGameMenu, int area = 1, bool fromThisMenu = false)
			: base(Game1.viewport.Width / 2 - 640, Game1.viewport.Height / 2 - 360, 1280, 720, showUpperRightCloseButton: true)
		{
			CommunityCenter cc = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
			if (fromGameMenu && !fromThisMenu)
			{
				for (int j = 0; j < cc.areasComplete.Count; j++)
				{
					if (cc.shouldNoteAppearInArea(j) && !cc.areasComplete[j])
					{
						area = j;
						whichArea = area;
						break;
					}
				}
				if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					area = 6;
				}
			}
			setUpMenu(area, cc.bundlesDict());
			Game1.player.forceCanMove();
			areaNextButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				visible = false,
				myID = 101,
				leftNeighborID = 102,
				leftNeighborImmutable = true,
				downNeighborID = -99998
			};
			areaBackButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				visible = false,
				myID = 102,
				rightNeighborID = 101,
				rightNeighborImmutable = true,
				downNeighborID = -99998
			};
			int area_count = 6;
			for (int i = 0; i < area_count; i++)
			{
				if (i != area && cc.shouldNoteAppearInArea(i))
				{
					areaNextButton.visible = true;
					areaBackButton.visible = true;
					break;
				}
			}
			this.fromGameMenu = fromGameMenu;
			this.fromThisMenu = fromThisMenu;
			foreach (Bundle bundle in bundles)
			{
				bundle.depositsAllowed = false;
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public JunimoNoteMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
			: base(Game1.viewport.Width / 2 - 640, Game1.viewport.Height / 2 - 360, 1280, 720, showUpperRightCloseButton: true)
		{
			setUpMenu(whichArea, bundlesComplete);
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (specificBundlePage)
			{
				currentlySnappedComponent = getComponentWithID(0);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID(5000);
			}
			snapCursorToCurrentSnappedComponent();
		}

		protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			if (specificBundlePage)
			{
				return false;
			}
			return true;
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID - 5000 < 0 || oldID - 5000 >= 10 || currentlySnappedComponent == null)
			{
				return;
			}
			int lowestScoreBundle = -1;
			int lowestScore = 999999;
			Point startingPosition = currentlySnappedComponent.bounds.Center;
			for (int i = 0; i < bundles.Count; i++)
			{
				if (bundles[i].myID == oldID)
				{
					continue;
				}
				int score = 999999;
				Point bundlePosition = bundles[i].bounds.Center;
				switch (direction)
				{
				case 3:
					if (bundlePosition.X < startingPosition.X)
					{
						score = startingPosition.X - bundlePosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
					}
					break;
				case 0:
					if (bundlePosition.Y < startingPosition.Y)
					{
						score = startingPosition.Y - bundlePosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
					}
					break;
				case 1:
					if (bundlePosition.X > startingPosition.X)
					{
						score = bundlePosition.X - startingPosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
					}
					break;
				case 2:
					if (bundlePosition.Y > startingPosition.Y)
					{
						score = bundlePosition.Y - startingPosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
					}
					break;
				}
				if (score < 10000 && score < lowestScore)
				{
					lowestScore = score;
					lowestScoreBundle = i;
				}
			}
			if (lowestScoreBundle != -1)
			{
				currentlySnappedComponent = getComponentWithID(lowestScoreBundle + 5000);
				snapCursorToCurrentSnappedComponent();
				return;
			}
			switch (direction)
			{
			case 2:
				if (presentButton != null)
				{
					currentlySnappedComponent = presentButton;
					snapCursorToCurrentSnappedComponent();
					presentButton.upNeighborID = oldID;
				}
				break;
			case 3:
				if (areaBackButton != null && areaBackButton.visible)
				{
					currentlySnappedComponent = areaBackButton;
					snapCursorToCurrentSnappedComponent();
					areaBackButton.rightNeighborID = oldID;
				}
				break;
			case 1:
				if (areaNextButton != null && areaNextButton.visible)
				{
					currentlySnappedComponent = areaNextButton;
					snapCursorToCurrentSnappedComponent();
					areaNextButton.leftNeighborID = oldID;
				}
				break;
			}
		}

		public void setUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
		{
			noteTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNote");
			if (!Game1.player.hasOrWillReceiveMail("seenJunimoNote"))
			{
				Game1.player.removeQuest(26);
				Game1.player.mailReceived.Add("seenJunimoNote");
			}
			if (!Game1.player.hasOrWillReceiveMail("wizardJunimoNote"))
			{
				Game1.addMailForTomorrow("wizardJunimoNote");
			}
			if (!Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && whichArea == 6)
			{
				Game1.player.mailReceived.Add("hasSeenAbandonedJunimoNote");
			}
			scrambledText = !Game1.player.hasOrWillReceiveMail("canReadJunimoText");
			tempSprites.Clear();
			this.whichArea = whichArea;
			inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, playerInventory: true, null, Utility.highlightSmallObjects, 36, 6, 8, 8, drawSlots: false)
			{
				capacity = 36
			};
			for (int j = 0; j < inventory.inventory.Count; j++)
			{
				if (j >= inventory.actualInventory.Count)
				{
					inventory.inventory[j].visible = false;
				}
			}
			foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
			{
				item.downNeighborID = -99998;
			}
			foreach (ClickableComponent item2 in inventory.GetBorder(InventoryMenu.BorderSide.Right))
			{
				item2.rightNeighborID = -99998;
			}
			inventory.dropItemInvisibleButton.visible = false;
			Dictionary<string, string> bundlesInfo = Game1.content.Load<Dictionary<string, string>>("Data\\Bundles");
			string areaName = CommunityCenter.getAreaNameFromNumber(whichArea);
			int bundlesAdded = 0;
			foreach (string i in bundlesInfo.Keys)
			{
				if (i.Contains(areaName))
				{
					int bundleIndex = Convert.ToInt32(i.Split('/')[1]);
					bundles.Add(new Bundle(bundleIndex, bundlesInfo[i], bundlesComplete[bundleIndex], getBundleLocationFromNumber(bundlesAdded), "LooseSprites\\JunimoNote", this)
					{
						myID = bundlesAdded + 5000,
						rightNeighborID = -7777,
						leftNeighborID = -7777,
						upNeighborID = -7777,
						downNeighborID = -7777,
						fullyImmutable = true
					});
					bundlesAdded++;
				}
			}
			backButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth * 2 + 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 103
			};
			checkForRewards();
			canClick = true;
			Game1.playSound("shwip");
			bool isOneIncomplete = false;
			foreach (Bundle b in bundles)
			{
				if (!b.complete && !b.Equals(currentPageBundle))
				{
					isOneIncomplete = true;
					break;
				}
			}
			if (!isOneIncomplete)
			{
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).markAreaAsComplete(whichArea);
				exitFunction = restoreAreaOnExit;
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
			}
		}

		public override bool readyToClose()
		{
			if (!specificBundlePage)
			{
				return isReadyToCloseMenuOrBundle();
			}
			return false;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!canClick)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			if (scrambledText)
			{
				return;
			}
			if (specificBundlePage)
			{
				if (!currentPageBundle.complete && currentPageBundle.completionTimer <= 0)
				{
					heldItem = inventory.leftClick(x, y, heldItem);
				}
				if (backButton.containsPoint(x, y) && heldItem == null)
				{
					takeDownBundleSpecificPage(currentPageBundle);
					Game1.playSound("shwip");
				}
				if (heldItem != null)
				{
					if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
					{
						for (int i = 0; i < ingredientSlots.Count; i++)
						{
							if (ingredientSlots[i].item == null)
							{
								heldItem = currentPageBundle.tryToDepositThisItem(heldItem, ingredientSlots[i], "LooseSprites\\JunimoNote");
								checkIfBundleIsComplete();
								return;
							}
						}
					}
					for (int j = 0; j < ingredientSlots.Count; j++)
					{
						if (ingredientSlots[j].containsPoint(x, y))
						{
							heldItem = currentPageBundle.tryToDepositThisItem(heldItem, ingredientSlots[j], "LooseSprites\\JunimoNote");
							checkIfBundleIsComplete();
						}
					}
				}
				if (purchaseButton != null && purchaseButton.containsPoint(x, y))
				{
					int moneyRequired = currentPageBundle.ingredients.Last().stack;
					if (Game1.player.Money >= moneyRequired)
					{
						Game1.player.Money -= moneyRequired;
						Game1.playSound("select");
						currentPageBundle.completionAnimation(this);
						if (purchaseButton != null)
						{
							purchaseButton.scale = purchaseButton.baseScale * 0.75f;
						}
						((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[currentPageBundle.bundleIndex] = true;
						(Game1.getLocationFromName("CommunityCenter") as CommunityCenter).bundles.FieldDict[currentPageBundle.bundleIndex][0] = true;
						checkForRewards();
						bool isOneIncomplete = false;
						foreach (Bundle b2 in bundles)
						{
							if (!b2.complete && !b2.Equals(currentPageBundle))
							{
								isOneIncomplete = true;
								break;
							}
						}
						if (!isOneIncomplete)
						{
							((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).markAreaAsComplete(whichArea);
							exitFunction = restoreAreaOnExit;
							((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
						}
						else
						{
							((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).getJunimoForArea(whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.bundleColor), Game1.getLocationFromName("CommunityCenter"));
						}
						Game1.multiplayer.globalChatInfoMessage("Bundle");
					}
					else
					{
						Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
					}
				}
				if (upperRightCloseButton != null && isReadyToCloseMenuOrBundle() && upperRightCloseButton.containsPoint(x, y))
				{
					closeBundlePage();
				}
			}
			else
			{
				foreach (Bundle b in bundles)
				{
					if (b.canBeClicked() && b.containsPoint(x, y))
					{
						setUpBundleSpecificPage(b);
						Game1.playSound("shwip");
						return;
					}
				}
				if (presentButton != null && presentButton.containsPoint(x, y) && !fromGameMenu && !fromThisMenu)
				{
					openRewardsMenu();
				}
				if (fromGameMenu)
				{
					Game1.getLocationFromName("CommunityCenter");
					if (areaNextButton.containsPoint(x, y))
					{
						SwapPage(1);
					}
					else if (areaBackButton.containsPoint(x, y))
					{
						SwapPage(-1);
					}
				}
			}
			if (heldItem != null && !isWithinBounds(x, y) && heldItem.canBeTrashed())
			{
				Game1.playSound("throwDownITem");
				Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				heldItem = null;
			}
		}

		public bool isReadyToCloseMenuOrBundle()
		{
			if (specificBundlePage && currentPageBundle != null && currentPageBundle.completionTimer > 0)
			{
				return false;
			}
			if (heldItem != null)
			{
				return false;
			}
			return true;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (fromGameMenu && !specificBundlePage)
			{
				Game1.getLocationFromName("CommunityCenter");
				switch (b)
				{
				case Buttons.RightTrigger:
					SwapPage(1);
					break;
				case Buttons.LeftTrigger:
					SwapPage(-1);
					break;
				}
			}
		}

		public void SwapPage(int direction)
		{
			if ((direction > 0 && !areaNextButton.visible) || (direction < 0 && !areaBackButton.visible))
			{
				return;
			}
			CommunityCenter cc = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
			int area = whichArea;
			int area_count = 6;
			int i = 0;
			while (true)
			{
				if (i < area_count)
				{
					area += direction;
					if (area < 0)
					{
						area += area_count;
					}
					if (area >= area_count)
					{
						area -= area_count;
					}
					if (cc.shouldNoteAppearInArea(area))
					{
						break;
					}
					i++;
					continue;
				}
				return;
			}
			int selected_id = -1;
			if (currentlySnappedComponent != null && (currentlySnappedComponent.myID >= 5000 || currentlySnappedComponent.myID == 101 || currentlySnappedComponent.myID == 102))
			{
				selected_id = currentlySnappedComponent.myID;
			}
			JunimoNoteMenu new_menu = (JunimoNoteMenu)(Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true, area, fromThisMenu: true));
			if (selected_id >= 0)
			{
				new_menu.currentlySnappedComponent = new_menu.getComponentWithID(currentlySnappedComponent.myID);
				new_menu.snapCursorToCurrentSnappedComponent();
			}
			if (new_menu.getComponentWithID(areaNextButton.leftNeighborID) != null)
			{
				new_menu.areaNextButton.leftNeighborID = areaNextButton.leftNeighborID;
			}
			else
			{
				new_menu.areaNextButton.leftNeighborID = new_menu.areaBackButton.myID;
			}
			new_menu.areaNextButton.rightNeighborID = areaNextButton.rightNeighborID;
			new_menu.areaNextButton.upNeighborID = areaNextButton.upNeighborID;
			new_menu.areaNextButton.downNeighborID = areaNextButton.downNeighborID;
			if (new_menu.getComponentWithID(areaBackButton.rightNeighborID) != null)
			{
				new_menu.areaBackButton.leftNeighborID = areaBackButton.leftNeighborID;
			}
			else
			{
				new_menu.areaBackButton.leftNeighborID = new_menu.areaNextButton.myID;
			}
			new_menu.areaBackButton.rightNeighborID = areaBackButton.rightNeighborID;
			new_menu.areaBackButton.upNeighborID = areaBackButton.upNeighborID;
			new_menu.areaBackButton.downNeighborID = areaBackButton.downNeighborID;
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (key.Equals(Keys.Delete) && heldItem != null && heldItem.canBeTrashed())
			{
				Utility.trashItem(heldItem);
				heldItem = null;
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && isReadyToCloseMenuOrBundle())
			{
				closeBundlePage();
			}
		}

		private void closeBundlePage()
		{
			if (specificBundlePage)
			{
				hoveredItem = null;
				inventory.descriptionText = "";
				if (heldItem == null)
				{
					takeDownBundleSpecificPage(currentPageBundle);
					Game1.playSound("shwip");
				}
				else
				{
					heldItem = inventory.tryToAddItem(heldItem);
				}
			}
		}

		private void reOpenThisMenu()
		{
			bool num = specificBundlePage;
			JunimoNoteMenu newMenu = (!fromGameMenu && !fromThisMenu) ? new JunimoNoteMenu(whichArea, ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundlesDict()) : new JunimoNoteMenu(fromGameMenu, whichArea, fromThisMenu);
			if (num)
			{
				foreach (Bundle bundle in newMenu.bundles)
				{
					if (bundle.bundleIndex == currentPageBundle.bundleIndex)
					{
						newMenu.setUpBundleSpecificPage(bundle);
						break;
					}
				}
			}
			Game1.activeClickableMenu = newMenu;
		}

		private void updateIngredientSlots()
		{
			int slotNumber = 0;
			for (int i = 0; i < currentPageBundle.ingredients.Count; i++)
			{
				if (currentPageBundle.ingredients[i].completed && slotNumber < ingredientSlots.Count)
				{
					ingredientSlots[slotNumber].item = new Object(currentPageBundle.ingredients[i].index, currentPageBundle.ingredients[i].stack, isRecipe: false, -1, currentPageBundle.ingredients[i].quality);
					currentPageBundle.ingredientDepositAnimation(ingredientSlots[slotNumber], "LooseSprites\\JunimoNote", skipAnimation: true);
					slotNumber++;
				}
			}
		}

		public static void GetBundleRewards(int area, List<Item> rewards)
		{
			Dictionary<string, string> bundlesInfo = Game1.content.Load<Dictionary<string, string>>("Data\\Bundles");
			foreach (string j in bundlesInfo.Keys)
			{
				if (j.Contains(CommunityCenter.getAreaNameFromNumber(area)))
				{
					int bundleIndex = Convert.ToInt32(j.Split('/')[1]);
					if (((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[bundleIndex])
					{
						Item i = Utility.getItemFromStandardTextDescription(bundlesInfo[j].Split('/')[1], Game1.player);
						i.SpecialVariable = bundleIndex;
						rewards.Add(i);
					}
				}
			}
		}

		private void openRewardsMenu()
		{
			Game1.playSound("smallSelect");
			List<Item> rewards = new List<Item>();
			GetBundleRewards(whichArea, rewards);
			Game1.activeClickableMenu = new ItemGrabMenu(rewards, reverseGrab: false, showReceivingMenu: true, null, null, null, rewardGrabbed, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
			Game1.activeClickableMenu.exitFunction = ((exitFunction != null) ? exitFunction : new onExit(reOpenThisMenu));
		}

		private void rewardGrabbed(Item item, Farmer who)
		{
			((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[item.SpecialVariable] = false;
		}

		private void checkIfBundleIsComplete()
		{
			if (!specificBundlePage || currentPageBundle == null)
			{
				return;
			}
			int numberOfFilledSlots = 0;
			foreach (ClickableTextureComponent ingredientSlot in ingredientSlots)
			{
				if (ingredientSlot.item != null)
				{
					numberOfFilledSlots++;
				}
			}
			if (numberOfFilledSlots < currentPageBundle.numberOfIngredientSlots)
			{
				return;
			}
			if (heldItem != null)
			{
				Game1.player.addItemToInventory(heldItem);
				heldItem = null;
			}
			for (int i = 0; i < ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundles[currentPageBundle.bundleIndex].Length; i++)
			{
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundles.FieldDict[currentPageBundle.bundleIndex][i] = true;
			}
			((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).checkForNewJunimoNotes();
			screenSwipe = new ScreenSwipe(0);
			currentPageBundle.completionAnimation(this, playSound: true, 400);
			canClick = false;
			((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[currentPageBundle.bundleIndex] = true;
			Game1.multiplayer.globalChatInfoMessage("Bundle");
			bool isOneIncomplete = false;
			foreach (Bundle b in bundles)
			{
				if (!b.complete && !b.Equals(currentPageBundle))
				{
					isOneIncomplete = true;
					break;
				}
			}
			if (!isOneIncomplete)
			{
				if (whichArea == 6)
				{
					exitFunction = restoreaAreaOnExit_AbandonedJojaMart;
				}
				else
				{
					((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).markAreaAsComplete(whichArea);
					exitFunction = restoreAreaOnExit;
					((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
				}
			}
			else
			{
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).getJunimoForArea(whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.bundleColor), Game1.getLocationFromName("CommunityCenter"));
			}
			checkForRewards();
		}

		private void restoreaAreaOnExit_AbandonedJojaMart()
		{
			((AbandonedJojaMart)Game1.getLocationFromName("AbandonedJojaMart")).restoreAreaCutscene();
		}

		private void restoreAreaOnExit()
		{
			if (!fromGameMenu)
			{
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).restoreAreaCutscene(whichArea);
			}
		}

		public void checkForRewards()
		{
			Dictionary<string, string> bundlesInfo = Game1.content.Load<Dictionary<string, string>>("Data\\Bundles");
			foreach (string i in bundlesInfo.Keys)
			{
				if (i.Contains(CommunityCenter.getAreaNameFromNumber(whichArea)) && bundlesInfo[i].Split('/')[1].Length > 1)
				{
					int bundleIndex = Convert.ToInt32(i.Split('/')[1]);
					if (((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[bundleIndex])
					{
						presentButton = new ClickableAnimatedComponent(new Rectangle(xPositionOnScreen + 592, yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite("LooseSprites\\JunimoNote", new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), flicker: false, flipped: false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true));
						break;
					}
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (canClick)
			{
				if (specificBundlePage)
				{
					heldItem = inventory.rightClick(x, y, heldItem);
				}
				if (!specificBundlePage && isReadyToCloseMenuOrBundle())
				{
					exitThisMenu();
				}
			}
		}

		public override void update(GameTime time)
		{
			if (specificBundlePage && currentPageBundle != null && currentPageBundle.completionTimer <= 0 && isReadyToCloseMenuOrBundle() && currentPageBundle.complete)
			{
				takeDownBundleSpecificPage(currentPageBundle);
			}
			foreach (Bundle bundle in bundles)
			{
				bundle.update(time);
			}
			for (int i = tempSprites.Count - 1; i >= 0; i--)
			{
				if (tempSprites[i].update(time))
				{
					tempSprites.RemoveAt(i);
				}
			}
			if (presentButton != null)
			{
				presentButton.update(time);
			}
			if (screenSwipe != null)
			{
				canClick = false;
				if (screenSwipe.update(time))
				{
					screenSwipe = null;
					canClick = true;
				}
			}
			if (bundlesChanged && fromGameMenu)
			{
				reOpenThisMenu();
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (scrambledText)
			{
				return;
			}
			hoverText = "";
			if (specificBundlePage)
			{
				backButton.tryHover(x, y);
				if (!currentPageBundle.complete && currentPageBundle.completionTimer <= 0)
				{
					hoveredItem = inventory.hover(x, y, heldItem);
				}
				else
				{
					hoveredItem = null;
				}
				foreach (ClickableTextureComponent c2 in ingredientList)
				{
					if (c2.bounds.Contains(x, y))
					{
						hoverText = c2.hoverText;
						break;
					}
				}
				if (heldItem != null)
				{
					foreach (ClickableTextureComponent c in ingredientSlots)
					{
						if (c.bounds.Contains(x, y) && currentPageBundle.canAcceptThisItem(heldItem, c))
						{
							c.sourceRect.X = 530;
							c.sourceRect.Y = 262;
						}
						else
						{
							c.sourceRect.X = 512;
							c.sourceRect.Y = 244;
						}
					}
				}
				if (purchaseButton != null)
				{
					purchaseButton.tryHover(x, y);
				}
			}
			else
			{
				if (presentButton != null)
				{
					hoverText = presentButton.tryHover(x, y);
				}
				foreach (Bundle bundle in bundles)
				{
					bundle.tryHoverAction(x, y);
				}
				if (fromGameMenu)
				{
					Game1.getLocationFromName("CommunityCenter");
					areaNextButton.tryHover(x, y);
					areaBackButton.tryHover(x, y);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (Game1.options.showMenuBackground)
			{
				base.drawBackground(b);
			}
			else
			{
				b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);
			}
			if (!specificBundlePage)
			{
				b.Draw(noteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				SpriteText.drawStringHorizontallyCenteredAt(b, scrambledText ? CommunityCenter.getAreaEnglishDisplayNameFromNumber(whichArea) : CommunityCenter.getAreaDisplayNameFromNumber(whichArea), xPositionOnScreen + width / 2 + 16, yPositionOnScreen + 12, 999999, -1, 99999, 0.88f, 0.88f, scrambledText);
				if (scrambledText)
				{
					SpriteText.drawString(b, LocalizedContentManager.CurrentLanguageLatin ? Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786") : Game1.content.LoadBaseString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786"), xPositionOnScreen + 96, yPositionOnScreen + 96, 999999, width - 192, 99999, 0.88f, 0.88f, junimoText: true);
					base.draw(b);
					if (canClick)
					{
						drawMouse(b);
					}
					return;
				}
				foreach (Bundle bundle in bundles)
				{
					bundle.draw(b);
				}
				if (presentButton != null)
				{
					presentButton.draw(b);
				}
				foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
				{
					tempSprite.draw(b, localPosition: true);
				}
				if (fromGameMenu)
				{
					if (areaNextButton.visible)
					{
						areaNextButton.draw(b);
					}
					if (areaBackButton.visible)
					{
						areaBackButton.draw(b);
					}
				}
			}
			else
			{
				b.Draw(noteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(320, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				if (currentPageBundle != null)
				{
					b.Draw(noteTexture, new Vector2(xPositionOnScreen + 872, yPositionOnScreen + 88), new Rectangle(currentPageBundle.bundleIndex * 16 * 2 % noteTexture.Width, 180 + 32 * (currentPageBundle.bundleIndex * 16 * 2 / noteTexture.Width), 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.15f);
					float textX = Game1.dialogueFont.MeasureString((!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label)).X;
					b.Draw(noteTexture, new Vector2(xPositionOnScreen + 936 - (int)textX / 2 - 16, yPositionOnScreen + 228), new Rectangle(517, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					b.Draw(noteTexture, new Rectangle(xPositionOnScreen + 936 - (int)textX / 2, yPositionOnScreen + 228, (int)textX, 68), new Rectangle(520, 266, 1, 17), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
					b.Draw(noteTexture, new Vector2(xPositionOnScreen + 936 + (int)textX / 2, yPositionOnScreen + 228), new Rectangle(524, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(xPositionOnScreen + 936) - textX / 2f, yPositionOnScreen + 236) + new Vector2(2f, 2f), Game1.textShadowColor);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(xPositionOnScreen + 936) - textX / 2f, yPositionOnScreen + 236) + new Vector2(0f, 2f), Game1.textShadowColor);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(xPositionOnScreen + 936) - textX / 2f, yPositionOnScreen + 236) + new Vector2(2f, 0f), Game1.textShadowColor);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(xPositionOnScreen + 936) - textX / 2f, yPositionOnScreen + 236), Game1.textColor * 0.9f);
				}
				backButton.draw(b);
				if (purchaseButton != null)
				{
					purchaseButton.draw(b);
					Game1.dayTimeMoneyBox.drawMoneyBox(b);
				}
				foreach (TemporaryAnimatedSprite tempSprite2 in tempSprites)
				{
					tempSprite2.draw(b, localPosition: true);
				}
				foreach (ClickableTextureComponent c2 in ingredientSlots)
				{
					if (c2.item == null)
					{
						c2.draw(b, fromGameMenu ? (Color.LightGray * 0.5f) : Color.White, 0.89f);
					}
					c2.drawItem(b, 4, 4);
				}
				for (int i = 0; i < ingredientList.Count; i++)
				{
					ClickableTextureComponent c = ingredientList[i];
					bool completed = false;
					_ = currentPageBundle.bundleColor;
					if (currentPageBundle != null && currentPageBundle.ingredients != null && i < currentPageBundle.ingredients.Count && currentPageBundle.ingredients[i].completed)
					{
						completed = true;
					}
					if (!completed)
					{
						b.Draw(Game1.shadowTexture, new Vector2(c.bounds.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4, c.bounds.Center.Y + 4), Game1.shadowTexture.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					}
					if (c.item != null && c.visible)
					{
						c.item.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale / 4f, 1f, 0.9f, StackDrawType.Draw, Color.White * (completed ? 0.25f : 1f), drawShadow: false);
					}
				}
				inventory.draw(b);
			}
			SpriteText.drawStringWithScrollCenteredAt(b, getRewardNameForArea(whichArea), xPositionOnScreen + width / 2, Math.Min(yPositionOnScreen + height + 20, Game1.viewport.Height - 64 - 8));
			base.draw(b);
			Game1.mouseCursorTransparency = 1f;
			if (canClick)
			{
				drawMouse(b);
			}
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
			}
			if (inventory.descriptionText.Length > 0)
			{
				if (hoveredItem != null)
				{
					IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
				}
			}
			else
			{
				IClickableMenu.drawHoverText(b, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") && hoverText.Length > 0) ? "???" : hoverText, Game1.dialogueFont);
			}
			if (screenSwipe != null)
			{
				screenSwipe.draw(b);
			}
		}

		public string getRewardNameForArea(int whichArea)
		{
			switch (whichArea)
			{
			case 3:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBoiler");
			case 5:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBulletin");
			case 1:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardCrafts");
			case 0:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardPantry");
			case 4:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardVault");
			case 2:
				return Game1.content.LoadString("Strings\\UI:JunimoNote_RewardFishTank");
			default:
				return "???";
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			xPositionOnScreen = Game1.viewport.Width / 2 - 640;
			yPositionOnScreen = Game1.viewport.Height / 2 - 360;
			backButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth * 2 + 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
			if (fromGameMenu)
			{
				areaNextButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
				{
					visible = false
				};
				areaBackButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
				{
					visible = false
				};
			}
			inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, playerInventory: true, null, Utility.highlightSmallObjects, Game1.player.maxItems, 6, 8, 8, drawSlots: false);
			for (int l = 0; l < inventory.inventory.Count; l++)
			{
				if (l >= inventory.actualInventory.Count)
				{
					inventory.inventory[l].visible = false;
				}
			}
			for (int k = 0; k < bundles.Count; k++)
			{
				Point p = getBundleLocationFromNumber(k);
				bundles[k].bounds.X = p.X;
				bundles[k].bounds.Y = p.Y;
				bundles[k].sprite.position = new Vector2(p.X, p.Y);
			}
			if (!specificBundlePage)
			{
				return;
			}
			int numberOfIngredientSlots = currentPageBundle.numberOfIngredientSlots;
			List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
			addRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
			ingredientSlots.Clear();
			for (int j = 0; j < ingredientSlotRectangles.Count; j++)
			{
				ingredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[j], noteTexture, new Rectangle(512, 244, 18, 18), 4f));
			}
			List<Rectangle> ingredientListRectangles = new List<Rectangle>();
			ingredientList.Clear();
			addRectangleRowsToList(ingredientListRectangles, currentPageBundle.ingredients.Count, 932, 364);
			for (int i = 0; i < ingredientListRectangles.Count; i++)
			{
				if (Game1.objectInformation.ContainsKey(currentPageBundle.ingredients[i].index))
				{
					ingredientList.Add(new ClickableTextureComponent("", ingredientListRectangles[i], "", Game1.objectInformation[currentPageBundle.ingredients[i].index].Split('/')[0], Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, currentPageBundle.ingredients[i].index, 16, 16), 4f)
					{
						myID = i + 1000,
						item = new Object(currentPageBundle.ingredients[i].index, currentPageBundle.ingredients[i].stack, isRecipe: false, -1, currentPageBundle.ingredients[i].quality),
						upNeighborID = -99998,
						rightNeighborID = -99998,
						leftNeighborID = -99998,
						downNeighborID = -99998
					});
				}
			}
			updateIngredientSlots();
		}

		private void setUpBundleSpecificPage(Bundle b)
		{
			tempSprites.Clear();
			currentPageBundle = b;
			specificBundlePage = true;
			if (whichArea == 4)
			{
				if (!fromGameMenu)
				{
					purchaseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 800, yPositionOnScreen + 504, 260, 72), noteTexture, new Rectangle(517, 286, 65, 20), 4f)
					{
						myID = 797,
						leftNeighborID = 103
					};
					if (Game1.options.SnappyMenus)
					{
						currentlySnappedComponent = purchaseButton;
						snapCursorToCurrentSnappedComponent();
					}
				}
				return;
			}
			int numberOfIngredientSlots = b.numberOfIngredientSlots;
			List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
			addRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
			for (int k = 0; k < ingredientSlotRectangles.Count; k++)
			{
				ingredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[k], noteTexture, new Rectangle(512, 244, 18, 18), 4f)
				{
					myID = k + 250,
					upNeighborID = -99998,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998
				});
			}
			List<Rectangle> ingredientListRectangles = new List<Rectangle>();
			addRectangleRowsToList(ingredientListRectangles, b.ingredients.Count, 932, 364);
			for (int j = 0; j < ingredientListRectangles.Count; j++)
			{
				if (Game1.objectInformation.ContainsKey(b.ingredients[j].index))
				{
					string displayName = Game1.objectInformation[b.ingredients[j].index].Split('/')[4];
					ingredientList.Add(new ClickableTextureComponent("ingredient_list_slot", ingredientListRectangles[j], "", displayName, Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, b.ingredients[j].index, 16, 16), 4f)
					{
						myID = j + 1000,
						item = new Object(b.ingredients[j].index, b.ingredients[j].stack, isRecipe: false, -1, b.ingredients[j].quality),
						upNeighborID = -99998,
						rightNeighborID = -99998,
						leftNeighborID = -99998,
						downNeighborID = -99998
					});
				}
			}
			updateIngredientSlots();
			if (!Game1.options.SnappyMenus)
			{
				return;
			}
			populateClickableComponentList();
			if (inventory != null && inventory.inventory != null)
			{
				for (int i = 0; i < inventory.inventory.Count; i++)
				{
					if (inventory.inventory[i] != null)
					{
						if (inventory.inventory[i].downNeighborID == 101)
						{
							inventory.inventory[i].downNeighborID = -1;
						}
						if (inventory.inventory[i].leftNeighborID == -1)
						{
							inventory.inventory[i].leftNeighborID = 103;
						}
						if (inventory.inventory[i].upNeighborID >= 1000)
						{
							inventory.inventory[i].upNeighborID = 103;
						}
					}
				}
			}
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return (a.myID >= 5000 || a.myID == 101 || a.myID == 102) == (b.myID >= 5000 || b.myID == 101 || b.myID == 102);
		}

		private void addRectangleRowsToList(List<Rectangle> toAddTo, int numberOfItems, int centerX, int centerY)
		{
			switch (numberOfItems)
			{
			case 1:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 1, 72, 72, 12));
				break;
			case 2:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 2, 72, 72, 12));
				break;
			case 3:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 3, 72, 72, 12));
				break;
			case 4:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 4, 72, 72, 12));
				break;
			case 5:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
				break;
			case 6:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
				break;
			case 7:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
				break;
			case 8:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
				break;
			case 9:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
				break;
			case 10:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
				break;
			case 11:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
				break;
			case 12:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
				break;
			}
		}

		private List<Rectangle> createRowOfBoxesCenteredAt(int xStart, int yStart, int numBoxes, int boxWidth, int boxHeight, int horizontalGap)
		{
			List<Rectangle> rectangles = new List<Rectangle>();
			int actualXStart = xStart - numBoxes * (boxWidth + horizontalGap) / 2;
			int actualYStart = yStart - boxHeight / 2;
			for (int i = 0; i < numBoxes; i++)
			{
				rectangles.Add(new Rectangle(actualXStart + i * (boxWidth + horizontalGap), actualYStart, boxWidth, boxHeight));
			}
			return rectangles;
		}

		public void takeDownBundleSpecificPage(Bundle b = null)
		{
			if (!isReadyToCloseMenuOrBundle())
			{
				return;
			}
			hoveredItem = null;
			if (!specificBundlePage)
			{
				return;
			}
			if (b == null)
			{
				b = currentPageBundle;
			}
			specificBundlePage = false;
			ingredientSlots.Clear();
			ingredientList.Clear();
			tempSprites.Clear();
			purchaseButton = null;
			if (Game1.options.SnappyMenus)
			{
				if (currentPageBundle != null)
				{
					currentlySnappedComponent = currentPageBundle;
					snapCursorToCurrentSnappedComponent();
				}
				else
				{
					snapToDefaultClickableComponent();
				}
			}
		}

		private Point getBundleLocationFromNumber(int whichBundle)
		{
			Point location = new Point(xPositionOnScreen, yPositionOnScreen);
			switch (whichBundle)
			{
			case 0:
				location.X += 592;
				location.Y += 136;
				break;
			case 1:
				location.X += 392;
				location.Y += 384;
				break;
			case 2:
				location.X += 784;
				location.Y += 388;
				break;
			case 5:
				location.X += 588;
				location.Y += 276;
				break;
			case 6:
				location.X += 588;
				location.Y += 380;
				break;
			case 3:
				location.X += 304;
				location.Y += 252;
				break;
			case 4:
				location.X += 892;
				location.Y += 252;
				break;
			case 7:
				location.X += 440;
				location.Y += 164;
				break;
			case 8:
				location.X += 776;
				location.Y += 164;
				break;
			}
			return location;
		}
	}
}
