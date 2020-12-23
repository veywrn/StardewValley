using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class CollectionsPage : IClickableMenu
	{
		public const int region_sideTabShipped = 7001;

		public const int region_sideTabFish = 7002;

		public const int region_sideTabArtifacts = 7003;

		public const int region_sideTabMinerals = 7004;

		public const int region_sideTabCooking = 7005;

		public const int region_sideTabAchivements = 7006;

		public const int region_sideTabSecretNotes = 7007;

		public const int region_sideTabLetters = 7008;

		public const int region_forwardButton = 707;

		public const int region_backButton = 706;

		public static int widthToMoveActiveTab = 8;

		public const int organicsTab = 0;

		public const int fishTab = 1;

		public const int archaeologyTab = 2;

		public const int mineralsTab = 3;

		public const int cookingTab = 4;

		public const int achievementsTab = 5;

		public const int secretNotesTab = 6;

		public const int lettersTab = 7;

		public const int distanceFromMenuBottomBeforeNewPage = 128;

		private string descriptionText = "";

		private string hoverText = "";

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		public Dictionary<int, ClickableTextureComponent> sideTabs = new Dictionary<int, ClickableTextureComponent>();

		public int currentTab;

		public int currentPage;

		public int secretNoteImage = -1;

		public Dictionary<int, List<List<ClickableTextureComponent>>> collections = new Dictionary<int, List<List<ClickableTextureComponent>>>();

		public Dictionary<int, string> secretNotesData;

		public Texture2D secretNoteImageTexture;

		public LetterViewerMenu letterviewerSubMenu;

		private Item hoverItem;

		private CraftingRecipe hoverCraftingRecipe;

		private int value;

		public CollectionsPage(int x, int y, int width, int height)
			: base(x, y, width, height)
		{
			sideTabs.Add(0, new ClickableTextureComponent(string.Concat(0), new Rectangle(xPositionOnScreen - 48 + widthToMoveActiveTab, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Shipped"), Game1.mouseCursors, new Rectangle(640, 80, 16, 16), 4f)
			{
				myID = 7001,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			collections.Add(0, new List<List<ClickableTextureComponent>>());
			sideTabs.Add(1, new ClickableTextureComponent(string.Concat(1), new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Fish"), Game1.mouseCursors, new Rectangle(640, 64, 16, 16), 4f)
			{
				myID = 7002,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			collections.Add(1, new List<List<ClickableTextureComponent>>());
			sideTabs.Add(2, new ClickableTextureComponent(string.Concat(2), new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Artifacts"), Game1.mouseCursors, new Rectangle(656, 64, 16, 16), 4f)
			{
				myID = 7003,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			collections.Add(2, new List<List<ClickableTextureComponent>>());
			sideTabs.Add(3, new ClickableTextureComponent(string.Concat(3), new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Minerals"), Game1.mouseCursors, new Rectangle(672, 64, 16, 16), 4f)
			{
				myID = 7004,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			collections.Add(3, new List<List<ClickableTextureComponent>>());
			sideTabs.Add(4, new ClickableTextureComponent(string.Concat(4), new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Cooking"), Game1.mouseCursors, new Rectangle(688, 64, 16, 16), 4f)
			{
				myID = 7005,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			collections.Add(4, new List<List<ClickableTextureComponent>>());
			sideTabs.Add(5, new ClickableTextureComponent(string.Concat(5), new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Achievements"), Game1.mouseCursors, new Rectangle(656, 80, 16, 16), 4f)
			{
				myID = 7006,
				upNeighborID = 7005,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			collections.Add(5, new List<List<ClickableTextureComponent>>());
			sideTabs.Add(7, new ClickableTextureComponent(string.Concat(7), new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Letters"), Game1.mouseCursors, new Rectangle(688, 80, 16, 16), 4f)
			{
				myID = 7008,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 0
			});
			collections.Add(7, new List<List<ClickableTextureComponent>>());
			if (Game1.player.secretNotesSeen.Count > 0)
			{
				sideTabs.Add(6, new ClickableTextureComponent(string.Concat(6), new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_SecretNotes"), Game1.mouseCursors, new Rectangle(672, 80, 16, 16), 4f)
				{
					myID = 7007,
					upNeighborID = -99998,
					rightNeighborID = 0
				});
				collections.Add(6, new List<List<ClickableTextureComponent>>());
			}
			sideTabs[0].upNeighborID = -1;
			sideTabs[0].upNeighborImmutable = true;
			int last_tab = 0;
			int last_y = 0;
			foreach (int key in sideTabs.Keys)
			{
				if (sideTabs[key].bounds.Y > last_y)
				{
					last_y = sideTabs[key].bounds.Y;
					last_tab = key;
				}
			}
			sideTabs[last_tab].downNeighborID = -1;
			sideTabs[last_tab].downNeighborImmutable = true;
			widthToMoveActiveTab = 8;
			backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 48, yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 706,
				rightNeighborID = -7777
			};
			forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 60, yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 707,
				leftNeighborID = -7777
			};
			int[] widthUsed = new int[8];
			int baseX = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
			int baseY = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
			int collectionWidth = 10;
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>(Game1.objectInformation);
			list.Sort((KeyValuePair<int, string> a, KeyValuePair<int, string> b) => a.Key.CompareTo(b.Key));
			foreach (KeyValuePair<int, string> kvp2 in list)
			{
				string typeString = kvp2.Value.Split('/')[3];
				int whichCollection2 = 0;
				bool farmerHas2 = false;
				bool farmerHasButNotMade = false;
				if (typeString.Contains("Arch"))
				{
					whichCollection2 = 2;
					if (Game1.player.archaeologyFound.ContainsKey(kvp2.Key))
					{
						farmerHas2 = true;
					}
				}
				else if (typeString.Contains("Fish"))
				{
					if ((kvp2.Key >= 167 && kvp2.Key <= 172) || (kvp2.Key >= 898 && kvp2.Key <= 902))
					{
						continue;
					}
					whichCollection2 = 1;
					if (Game1.player.fishCaught.ContainsKey(kvp2.Key))
					{
						farmerHas2 = true;
					}
				}
				else if (typeString.Contains("Mineral") || typeString.Substring(typeString.Length - 3).Equals("-2"))
				{
					whichCollection2 = 3;
					if (Game1.player.mineralsFound.ContainsKey(kvp2.Key))
					{
						farmerHas2 = true;
					}
				}
				else if (typeString.Contains("Cooking") || typeString.Substring(typeString.Length - 3).Equals("-7"))
				{
					whichCollection2 = 4;
					string last_minute_1_5_hack_name = kvp2.Value.Split('/')[0];
					switch (last_minute_1_5_hack_name)
					{
					case "Cheese Cauli.":
						last_minute_1_5_hack_name = "Cheese Cauliflower";
						break;
					case "Vegetable Medley":
						last_minute_1_5_hack_name = "Vegetable Stew";
						break;
					case "Cookie":
						last_minute_1_5_hack_name = "Cookies";
						break;
					case "Eggplant Parmesan":
						last_minute_1_5_hack_name = "Eggplant Parm.";
						break;
					case "Cranberry Sauce":
						last_minute_1_5_hack_name = "Cran. Sauce";
						break;
					case "Dish O' The Sea":
						last_minute_1_5_hack_name = "Dish o' The Sea";
						break;
					}
					if (Game1.player.recipesCooked.ContainsKey(kvp2.Key))
					{
						farmerHas2 = true;
					}
					else if (Game1.player.cookingRecipes.ContainsKey(last_minute_1_5_hack_name))
					{
						farmerHasButNotMade = true;
					}
					if (kvp2.Key == 217 || kvp2.Key == 772 || kvp2.Key == 773 || kvp2.Key == 279 || kvp2.Key == 873)
					{
						continue;
					}
				}
				else
				{
					if (!Object.isPotentialBasicShippedCategory(kvp2.Key, typeString.Substring(typeString.Length - 3)))
					{
						continue;
					}
					whichCollection2 = 0;
					if (Game1.player.basicShipped.ContainsKey(kvp2.Key))
					{
						farmerHas2 = true;
					}
				}
				int xPos4 = baseX + widthUsed[whichCollection2] % collectionWidth * 68;
				int yPos4 = baseY + widthUsed[whichCollection2] / collectionWidth * 68;
				if (yPos4 > yPositionOnScreen + height - 128)
				{
					collections[whichCollection2].Add(new List<ClickableTextureComponent>());
					widthUsed[whichCollection2] = 0;
					xPos4 = baseX;
					yPos4 = baseY;
				}
				if (collections[whichCollection2].Count == 0)
				{
					collections[whichCollection2].Add(new List<ClickableTextureComponent>());
				}
				collections[whichCollection2].Last().Add(new ClickableTextureComponent(kvp2.Key + " " + farmerHas2.ToString() + " " + farmerHasButNotMade.ToString(), new Rectangle(xPos4, yPos4, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, kvp2.Key, 16, 16), 4f, farmerHas2)
				{
					myID = collections[whichCollection2].Last().Count,
					rightNeighborID = (((collections[whichCollection2].Last().Count + 1) % collectionWidth == 0) ? (-1) : (collections[whichCollection2].Last().Count + 1)),
					leftNeighborID = ((collections[whichCollection2].Last().Count % collectionWidth == 0) ? 7001 : (collections[whichCollection2].Last().Count - 1)),
					downNeighborID = ((yPos4 + 68 > yPositionOnScreen + height - 128) ? (-7777) : (collections[whichCollection2].Last().Count + collectionWidth)),
					upNeighborID = ((collections[whichCollection2].Last().Count < collectionWidth) ? 12345 : (collections[whichCollection2].Last().Count - collectionWidth)),
					fullyImmutable = true
				});
				widthUsed[whichCollection2]++;
			}
			if (collections[5].Count == 0)
			{
				collections[5].Add(new List<ClickableTextureComponent>());
			}
			foreach (KeyValuePair<int, string> kvp in Game1.achievements)
			{
				bool farmerHas = Game1.player.achievements.Contains(kvp.Key);
				string[] split2 = kvp.Value.Split('^');
				if (farmerHas || (split2[2].Equals("true") && (split2[3].Equals("-1") || farmerHasAchievements(split2[3]))))
				{
					int xPos3 = baseX + widthUsed[5] % collectionWidth * 68;
					int yPos3 = baseY + widthUsed[5] / collectionWidth * 68;
					collections[5][0].Add(new ClickableTextureComponent(kvp.Key + " " + farmerHas.ToString(), new Rectangle(xPos3, yPos3, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), 1f));
					widthUsed[5]++;
				}
			}
			if (Game1.player.secretNotesSeen.Count > 0)
			{
				if (collections[6].Count == 0)
				{
					collections[6].Add(new List<ClickableTextureComponent>());
				}
				secretNotesData = Game1.content.Load<Dictionary<int, string>>("Data\\SecretNotes");
				secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\SecretNotesImages");
				bool show_journals = Game1.player.secretNotesSeen.Contains(GameLocation.JOURNAL_INDEX + 1);
				foreach (int i in secretNotesData.Keys)
				{
					if (i >= GameLocation.JOURNAL_INDEX)
					{
						if (!show_journals)
						{
							continue;
						}
					}
					else if (!Game1.player.hasMagnifyingGlass)
					{
						continue;
					}
					int xPos2 = baseX + widthUsed[6] % collectionWidth * 68;
					int yPos2 = baseY + widthUsed[6] / collectionWidth * 68;
					if (i >= GameLocation.JOURNAL_INDEX)
					{
						collections[6][0].Add(new ClickableTextureComponent(i + " " + Game1.player.secretNotesSeen.Contains(i).ToString(), new Rectangle(xPos2, yPos2, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 842, 16, 16), 4f, Game1.player.secretNotesSeen.Contains(i)));
					}
					else
					{
						collections[6][0].Add(new ClickableTextureComponent(i + " " + Game1.player.secretNotesSeen.Contains(i).ToString(), new Rectangle(xPos2, yPos2, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 79, 16, 16), 4f, Game1.player.secretNotesSeen.Contains(i)));
					}
					widthUsed[6]++;
				}
			}
			if (collections[7].Count == 0)
			{
				collections[7].Add(new List<ClickableTextureComponent>());
			}
			Dictionary<string, string> mail = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
			foreach (string s in Game1.player.mailReceived)
			{
				if (mail.ContainsKey(s))
				{
					int xPos = baseX + widthUsed[7] % collectionWidth * 68;
					int yPos = baseY + widthUsed[7] / collectionWidth * 68;
					string[] split = mail[s].Split(new string[1]
					{
						"[#]"
					}, StringSplitOptions.None);
					if (yPos > yPositionOnScreen + height - 128)
					{
						collections[7].Add(new List<ClickableTextureComponent>());
						widthUsed[7] = 0;
						xPos = baseX;
						yPos = baseY;
					}
					collections[7].Last().Add(new ClickableTextureComponent(s + " true " + ((split.Count() > 1) ? split[1] : "???"), new Rectangle(xPos, yPos, 64, 64), null, "", Game1.mouseCursors, new Rectangle(190, 423, 14, 11), 4f, drawShadow: true)
					{
						myID = collections[7].Last().Count,
						rightNeighborID = (((collections[7].Last().Count + 1) % collectionWidth == 0) ? (-1) : (collections[7].Last().Count + 1)),
						leftNeighborID = ((collections[7].Last().Count % collectionWidth == 0) ? 7008 : (collections[7].Last().Count - 1)),
						downNeighborID = ((yPos + 68 > yPositionOnScreen + height - 128) ? (-7777) : (collections[7].Last().Count + collectionWidth)),
						upNeighborID = ((collections[7].Last().Count < collectionWidth) ? 12345 : (collections[7].Last().Count - collectionWidth)),
						fullyImmutable = true
					});
					widthUsed[7]++;
				}
			}
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			base.customSnapBehavior(direction, oldRegion, oldID);
			switch (direction)
			{
			case 2:
				if (currentPage > 0)
				{
					currentlySnappedComponent = getComponentWithID(706);
				}
				else if (currentPage == 0 && collections[currentTab].Count > 1)
				{
					currentlySnappedComponent = getComponentWithID(707);
				}
				backButton.upNeighborID = oldID;
				forwardButton.upNeighborID = oldID;
				break;
			case 3:
				if (oldID == 707 && currentPage > 0)
				{
					currentlySnappedComponent = getComponentWithID(706);
				}
				break;
			case 1:
				if (oldID == 706 && collections[currentTab].Count > currentPage + 1)
				{
					currentlySnappedComponent = getComponentWithID(707);
				}
				break;
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		private bool farmerHasAchievements(string listOfAchievementNumbers)
		{
			string[] array = listOfAchievementNumbers.Split(' ');
			foreach (string s in array)
			{
				if (!Game1.player.achievements.Contains(Convert.ToInt32(s)))
				{
					return false;
				}
			}
			return true;
		}

		public override bool readyToClose()
		{
			if (letterviewerSubMenu != null)
			{
				return false;
			}
			return base.readyToClose();
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (letterviewerSubMenu == null)
			{
				return;
			}
			letterviewerSubMenu.update(time);
			if (letterviewerSubMenu.destroy)
			{
				letterviewerSubMenu = null;
				if (Game1.options.SnappyMenus)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.receiveKeyPress(key);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.receiveLeftClick(x, y);
				return;
			}
			foreach (KeyValuePair<int, ClickableTextureComponent> v in sideTabs)
			{
				if (v.Value.containsPoint(x, y) && currentTab != v.Key)
				{
					Game1.playSound("smallSelect");
					sideTabs[currentTab].bounds.X -= widthToMoveActiveTab;
					currentTab = Convert.ToInt32(v.Value.name);
					currentPage = 0;
					v.Value.bounds.X += widthToMoveActiveTab;
				}
			}
			if (currentPage > 0 && backButton.containsPoint(x, y))
			{
				currentPage--;
				Game1.playSound("shwip");
				backButton.scale = backButton.baseScale;
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && currentPage == 0)
				{
					currentlySnappedComponent = forwardButton;
					Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
				}
			}
			if (currentPage < collections[currentTab].Count - 1 && forwardButton.containsPoint(x, y))
			{
				currentPage++;
				Game1.playSound("shwip");
				forwardButton.scale = forwardButton.baseScale;
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && currentPage == collections[currentTab].Count - 1)
				{
					currentlySnappedComponent = backButton;
					Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
				}
			}
			if (currentTab == 7)
			{
				Dictionary<string, string> mail = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
				foreach (ClickableTextureComponent c2 in collections[currentTab][currentPage])
				{
					if (c2.containsPoint(x, y))
					{
						letterviewerSubMenu = new LetterViewerMenu(mail[c2.name.Split(' ')[0]], c2.name.Split(' ')[0], fromCollection: true);
					}
				}
			}
			else if (currentTab == 6)
			{
				foreach (ClickableTextureComponent c in collections[currentTab][currentPage])
				{
					if (c.containsPoint(x, y))
					{
						int index = -1;
						string[] split = c.name.Split(' ');
						if (split[1] == "True" && int.TryParse(split[0], out index))
						{
							letterviewerSubMenu = new LetterViewerMenu(index);
							letterviewerSubMenu.isFromCollection = true;
							break;
						}
					}
				}
			}
		}

		public override bool shouldDrawCloseButton()
		{
			return letterviewerSubMenu == null;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.receiveRightClick(x, y);
			}
		}

		public override void applyMovementKey(int direction)
		{
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.applyMovementKey(direction);
			}
			else
			{
				base.applyMovementKey(direction);
			}
		}

		public override void gamePadButtonHeld(Buttons b)
		{
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.gamePadButtonHeld(b);
			}
			else
			{
				base.gamePadButtonHeld(b);
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.receiveGamePadButton(b);
			}
			else
			{
				base.receiveGamePadButton(b);
			}
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			hoverText = "";
			value = -1;
			secretNoteImage = -1;
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.performHoverAction(x, y);
				return;
			}
			foreach (ClickableTextureComponent c2 in sideTabs.Values)
			{
				if (c2.containsPoint(x, y))
				{
					hoverText = c2.hoverText;
					return;
				}
			}
			bool hoveredAny = false;
			foreach (ClickableTextureComponent c in collections[currentTab][currentPage])
			{
				if (c.containsPoint(x, y))
				{
					c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
					string[] data_split = c.name.Split(' ');
					if (currentTab == 5 || (data_split.Length > 1 && Convert.ToBoolean(data_split[1])) || (data_split.Length > 2 && Convert.ToBoolean(data_split[2])))
					{
						if (currentTab == 7)
						{
							hoverText = Game1.parseText(c.name.Substring(c.name.IndexOf(' ', c.name.IndexOf(' ') + 1) + 1), Game1.smallFont, 256);
						}
						else
						{
							hoverText = createDescription(Convert.ToInt32(data_split[0]));
						}
					}
					else
					{
						if (hoverText != "???")
						{
							hoverItem = null;
						}
						hoverText = "???";
					}
					hoveredAny = true;
				}
				else
				{
					c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
				}
			}
			if (!hoveredAny)
			{
				hoverItem = null;
			}
			forwardButton.tryHover(x, y, 0.5f);
			backButton.tryHover(x, y, 0.5f);
		}

		public string createDescription(int index)
		{
			string description3 = "";
			if (currentTab == 5)
			{
				string[] split3 = Game1.achievements[index].Split('^');
				description3 = description3 + split3[0] + Environment.NewLine + Environment.NewLine;
				description3 += split3[1];
			}
			else if (currentTab == 6)
			{
				if (secretNotesData != null)
				{
					description3 = ((index >= GameLocation.JOURNAL_INDEX) ? (description3 + Game1.content.LoadString("Strings\\Locations:Journal_Name") + " #" + (index - GameLocation.JOURNAL_INDEX)) : (description3 + Game1.content.LoadString("Strings\\Locations:Secret_Note_Name") + " #" + index));
					if (secretNotesData[index][0] == '!')
					{
						secretNoteImage = Convert.ToInt32(secretNotesData[index].Split(' ')[1]);
					}
					else
					{
						string letter_text = Game1.parseText(Utility.ParseGiftReveals(secretNotesData[index]).TrimStart(' ', '^').Replace("^", Environment.NewLine)
							.Replace("@", Game1.player.name), Game1.smallFont, 512);
						string[] split2 = letter_text.Split(new string[1]
						{
							Environment.NewLine
						}, StringSplitOptions.None);
						int max_lines = 15;
						if (split2.Length > max_lines)
						{
							string[] new_split = new string[max_lines];
							for (int i = 0; i < max_lines; i++)
							{
								new_split[i] = split2[i];
							}
							letter_text = string.Join(Environment.NewLine, new_split).Trim() + Environment.NewLine + "(...)";
						}
						description3 = description3 + Environment.NewLine + Environment.NewLine + letter_text;
					}
				}
			}
			else
			{
				string[] split = Game1.objectInformation[index].Split('/');
				string displayName = split[4];
				description3 = description3 + displayName + Environment.NewLine + Environment.NewLine + Game1.parseText(split[5], Game1.smallFont, 256) + Environment.NewLine + Environment.NewLine;
				if (split[3].Contains("Arch"))
				{
					description3 += (Game1.player.archaeologyFound.ContainsKey(index) ? Game1.content.LoadString("Strings\\UI:Collections_Description_ArtifactsFound", Game1.player.archaeologyFound[index][0]) : "");
				}
				else if (split[3].Contains("Cooking"))
				{
					description3 += (Game1.player.recipesCooked.ContainsKey(index) ? Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", Game1.player.recipesCooked[index]) : "");
					if (hoverItem == null || hoverItem.ParentSheetIndex != index)
					{
						hoverItem = new Object(index, 1);
						string last_minute_1_5_hack_name = hoverItem.Name;
						switch (last_minute_1_5_hack_name)
						{
						case "Cheese Cauli.":
							last_minute_1_5_hack_name = "Cheese Cauliflower";
							break;
						case "Vegetable Medley":
							last_minute_1_5_hack_name = "Vegetable Stew";
							break;
						case "Cookie":
							last_minute_1_5_hack_name = "Cookies";
							break;
						case "Eggplant Parmesan":
							last_minute_1_5_hack_name = "Eggplant Parm.";
							break;
						case "Cranberry Sauce":
							last_minute_1_5_hack_name = "Cran. Sauce";
							break;
						case "Dish O' The Sea":
							last_minute_1_5_hack_name = "Dish o' The Sea";
							break;
						}
						hoverCraftingRecipe = new CraftingRecipe(last_minute_1_5_hack_name, isCookingRecipe: true);
					}
				}
				else if (!split[3].Contains("Fish"))
				{
					description3 = ((!split[3].Contains("Minerals") && !split[3].Substring(split[3].Length - 3).Equals("-2")) ? (description3 + Game1.content.LoadString("Strings\\UI:Collections_Description_NumberShipped", Game1.player.basicShipped.ContainsKey(index) ? Game1.player.basicShipped[index] : 0)) : (description3 + Game1.content.LoadString("Strings\\UI:Collections_Description_MineralsFound", Game1.player.mineralsFound.ContainsKey(index) ? Game1.player.mineralsFound[index] : 0)));
				}
				else
				{
					description3 += Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", Game1.player.fishCaught.ContainsKey(index) ? Game1.player.fishCaught[index][0] : 0);
					if (Game1.player.fishCaught.ContainsKey(index) && Game1.player.fishCaught[index][1] > 0)
					{
						description3 = description3 + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Collections_Description_BiggestCatch", Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round((double)Game1.player.fishCaught[index][1] * 2.54) : ((double)Game1.player.fishCaught[index][1])));
					}
				}
				value = Convert.ToInt32(split[1]);
			}
			return description3;
		}

		public override void draw(SpriteBatch b)
		{
			foreach (ClickableTextureComponent value2 in sideTabs.Values)
			{
				value2.draw(b);
			}
			if (currentPage > 0)
			{
				backButton.draw(b);
			}
			if (currentPage < collections[currentTab].Count - 1)
			{
				forwardButton.draw(b);
			}
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			foreach (ClickableTextureComponent c in collections[currentTab][currentPage])
			{
				bool drawColor = Convert.ToBoolean(c.name.Split(' ')[1]);
				bool drawColorFaded = currentTab == 4 && Convert.ToBoolean(c.name.Split(' ')[2]);
				c.draw(b, drawColorFaded ? (Color.DimGray * 0.4f) : (drawColor ? Color.White : (Color.Black * 0.2f)), 0.86f);
				if (currentTab == 5 && drawColor)
				{
					int StarPos = new Random(Convert.ToInt32(c.name.Split(' ')[0])).Next(12);
					b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X + 16 + 16, c.bounds.Y + 20 + 16), new Rectangle(256 + StarPos % 6 * 64 / 2, 128 + StarPos / 6 * 64 / 2, 32, 32), Color.White, 0f, new Vector2(16f, 16f), c.scale, SpriteEffects.None, 0.88f);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			if (hoverItem != null)
			{
				IClickableMenu.drawToolTip(b, hoverItem.getDescription(), hoverItem.DisplayName, hoverItem, heldItem: false, -1, 0, -1, -1, hoverCraftingRecipe);
			}
			else if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, 0, 0, value);
				if (secretNoteImage != -1)
				{
					IClickableMenu.drawTextureBox(b, Game1.getOldMouseX(), Game1.getOldMouseY() + 64 + 32, 288, 288, Color.White);
					b.Draw(secretNoteImageTexture, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 64 + 32 + 16), new Rectangle(secretNoteImage * 64 % secretNoteImageTexture.Width, secretNoteImage * 64 / secretNoteImageTexture.Width * 64, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				}
			}
			if (letterviewerSubMenu != null)
			{
				letterviewerSubMenu.draw(b);
			}
		}
	}
}
