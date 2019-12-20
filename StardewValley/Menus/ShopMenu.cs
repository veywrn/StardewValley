using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class ShopMenu : IClickableMenu
	{
		public const int region_shopButtonModifier = 3546;

		public const int region_upArrow = 97865;

		public const int region_downArrow = 97866;

		public const int region_tabStartIndex = 99999;

		public const int howManyRecipesFitOnPage = 28;

		public const int infiniteStock = int.MaxValue;

		public const int salePriceIndex = 0;

		public const int stockIndex = 1;

		public const int extraTradeItemIndex = 2;

		public const int extraTradeItemCountIndex = 3;

		public const int itemsPerPage = 4;

		public const int numberRequiredForExtraItemTrade = 5;

		private string descriptionText = "";

		private string hoverText = "";

		private string boldTitleText = "";

		public string purchaseSound = "purchaseClick";

		public string purchaseRepeatSound = "purchaseRepeat";

		public string storeContext = "";

		public InventoryMenu inventory;

		public ISalable heldItem;

		public ISalable hoveredItem;

		private Texture2D wallpapers;

		private Texture2D floors;

		private int lastWallpaperFloorPrice;

		private TemporaryAnimatedSprite poof;

		private Rectangle scrollBarRunner;

		public List<ISalable> forSale = new List<ISalable>();

		public List<ClickableComponent> forSaleButtons = new List<ClickableComponent>();

		public List<int> categoriesToSellHere = new List<int>();

		public Dictionary<ISalable, int[]> itemPriceAndStock = new Dictionary<ISalable, int[]>();

		private float sellPercentage = 1f;

		private List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();

		public int hoverPrice = -1;

		public int currency;

		public int currentItemIndex;

		public ClickableTextureComponent upArrow;

		public ClickableTextureComponent downArrow;

		public ClickableTextureComponent scrollBar;

		public NPC portraitPerson;

		public string potraitPersonDialogue;

		public object source;

		private bool scrolling;

		public Func<ISalable, Farmer, int, bool> onPurchase;

		public Func<ISalable, bool> onSell;

		public Func<int, bool> canPurchaseCheck;

		public List<ClickableTextureComponent> tabButtons = new List<ClickableTextureComponent>();

		protected int currentTab;

		protected bool _isStorageShop;

		public ShopMenu(Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
			: this(itemPriceAndStock.Keys.ToList(), currency, who, on_purchase, on_sell, context)
		{
			this.itemPriceAndStock = itemPriceAndStock;
			if (potraitPersonDialogue == null)
			{
				setUpShopOwner(who);
			}
		}

		public ShopMenu(List<ISalable> itemsForSale, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
			: base(Game1.viewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
		{
			foreach (ISalable j in itemsForSale)
			{
				if (j is Object && (bool)(j as Object).isRecipe)
				{
					if (Game1.player.knowsRecipe(j.Name))
					{
						continue;
					}
					j.Stack = 1;
				}
				forSale.Add(j);
				itemPriceAndStock.Add(j, new int[2]
				{
					j.salePrice(),
					j.Stack
				});
			}
			if (itemPriceAndStock.Count >= 2)
			{
				setUpShopOwner(who);
			}
			updatePosition();
			this.currency = currency;
			onPurchase = on_purchase;
			onSell = on_sell;
			Game1.player.forceCanMove();
			Game1.playSound("dwop");
			inventory = new InventoryMenu(xPositionOnScreen + width, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, playerInventory: false, null, highlightItemToSell)
			{
				showGrayedOutSlots = true
			};
			inventory.movePosition(-inventory.width - 32, 0);
			this.currency = currency;
			upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
			{
				myID = 97865,
				downNeighborID = 106,
				leftNeighborID = 3546
			};
			downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
			{
				myID = 106,
				upNeighborID = 97865,
				leftNeighborID = 3546
			};
			scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
			scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 64 - upArrow.bounds.Height - 28);
			for (int i = 0; i < 4; i++)
			{
				forSaleButtons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16 + i * ((height - 256) / 4), width - 32, (height - 256) / 4 + 4), string.Concat(i))
				{
					myID = i + 3546,
					rightNeighborID = 97865,
					fullyImmutable = true
				});
			}
			updateSaleButtonNeighbors();
			if (context == null)
			{
				context = Game1.currentLocation.name;
			}
			storeContext = context;
			setUpStoreForContext();
			if (tabButtons.Count > 0)
			{
				foreach (ClickableComponent forSaleButton in forSaleButtons)
				{
					forSaleButton.leftNeighborID = -99998;
				}
			}
			applyTab();
			foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Top))
			{
				item.upNeighborID = -99998;
			}
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public void updateSaleButtonNeighbors()
		{
			ClickableComponent last_valid_button = forSaleButtons[0];
			for (int i = 0; i < forSaleButtons.Count; i++)
			{
				ClickableComponent button = forSaleButtons[i];
				button.upNeighborImmutable = true;
				button.downNeighborImmutable = true;
				button.upNeighborID = ((i > 0) ? (i + 3546 - 1) : (-7777));
				button.downNeighborID = ((i < 3 && i < forSale.Count - 1) ? (i + 3546 + 1) : (-7777));
				if (i >= forSale.Count)
				{
					if (button == currentlySnappedComponent)
					{
						currentlySnappedComponent = last_valid_button;
						if (Game1.options.SnappyMenus)
						{
							snapCursorToCurrentSnappedComponent();
						}
					}
				}
				else
				{
					last_valid_button = button;
				}
			}
		}

		public virtual void setUpStoreForContext()
		{
			tabButtons = new List<ClickableTextureComponent>();
			switch (storeContext)
			{
			case "SeedShop":
				categoriesToSellHere.AddRange(new int[14]
				{
					-81,
					-75,
					-79,
					-80,
					-74,
					-17,
					-18,
					-6,
					-26,
					-5,
					-14,
					-19,
					-7,
					-25
				});
				break;
			case "Blacksmith":
				categoriesToSellHere.AddRange(new int[3]
				{
					-12,
					-2,
					-15
				});
				break;
			case "ScienceHouse":
				categoriesToSellHere.AddRange(new int[1]
				{
					-16
				});
				break;
			case "AnimalShop":
				categoriesToSellHere.AddRange(new int[4]
				{
					-18,
					-6,
					-5,
					-14
				});
				break;
			case "FishShop":
				categoriesToSellHere.AddRange(new int[4]
				{
					-4,
					-23,
					-21,
					-22
				});
				break;
			case "AdventureGuild":
				categoriesToSellHere.AddRange(new int[4]
				{
					-28,
					-98,
					-97,
					-96
				});
				break;
			case "Furniture Catalogue":
			{
				ClickableTextureComponent tab7 = null;
				tab7 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab7);
				tab7 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 48, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab7);
				tab7 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 48, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab7);
				tab7 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 64, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab7);
				tab7 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 64, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab7);
				tab7 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 64, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab7);
				break;
			}
			case "Catalogue":
			{
				ClickableTextureComponent tab11 = null;
				tab11 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab11);
				tab11 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 64, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab11);
				tab11 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 64, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab11);
				break;
			}
			case "Dresser":
			{
				categoriesToSellHere.AddRange(new int[4]
				{
					-95,
					-100,
					-97,
					-96
				});
				_isStorageShop = true;
				ClickableTextureComponent tab18 = null;
				tab18 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 48, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab18);
				tab18 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 48, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab18);
				tab18 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 48, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab18);
				tab18 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 48, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab18);
				tab18 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 64, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab18);
				tab18 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 64, 16, 16), 4f)
				{
					myID = 99999 + tabButtons.Count,
					upNeighborID = -99998,
					downNeighborID = -99998,
					rightNeighborID = 3546
				};
				tabButtons.Add(tab18);
				break;
			}
			}
			repositionTabs();
			if (_isStorageShop)
			{
				purchaseSound = null;
				purchaseRepeatSound = null;
			}
		}

		public void repositionTabs()
		{
			for (int i = 0; i < tabButtons.Count; i++)
			{
				if (i == currentTab)
				{
					tabButtons[i].bounds.X = xPositionOnScreen - 56;
				}
				else
				{
					tabButtons[i].bounds.X = xPositionOnScreen - 64;
				}
				tabButtons[i].bounds.Y = yPositionOnScreen + i * 16 * 4 + 16;
			}
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			switch (direction)
			{
			case 2:
			{
				if (currentItemIndex < Math.Max(0, forSale.Count - 4))
				{
					downArrowPressed();
					break;
				}
				int emptySlot = -1;
				for (int i = 0; i < 12; i++)
				{
					inventory.inventory[i].upNeighborID = oldID;
					if (emptySlot == -1 && heldItem != null && inventory.actualInventory != null && inventory.actualInventory.Count > i && inventory.actualInventory[i] == null)
					{
						emptySlot = i;
					}
				}
				currentlySnappedComponent = getComponentWithID((emptySlot != -1) ? emptySlot : 0);
				snapCursorToCurrentSnappedComponent();
				break;
			}
			case 0:
				if (currentItemIndex > 0)
				{
					upArrowPressed();
					currentlySnappedComponent = getComponentWithID(3546);
					snapCursorToCurrentSnappedComponent();
				}
				break;
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(3546);
			snapCursorToCurrentSnappedComponent();
		}

		public void setUpShopOwner(string who)
		{
			if (who == null)
			{
				return;
			}
			Random r = new Random((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
			string ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11457");
			switch (who)
			{
			case "DesertTrade":
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:DesertTrader" + (r.Next(2) + 1));
				if (r.NextDouble() < 0.2)
				{
					int which = r.Next(2) + 3;
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:DesertTrader" + which + ((which == 4) ? ("_" + (Game1.player.isMale ? "male" : "female")) : ""));
				}
				break;
			case "boxOffice":
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheaterBoxOffice");
				break;
			case "Concessions":
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheaterConcessions" + Game1.random.Next(5));
				break;
			case "Robin":
				portraitPerson = Game1.getCharacterFromName("Robin");
				switch (Game1.random.Next(5))
				{
				case 0:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11460");
					break;
				case 1:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11461");
					break;
				case 2:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11462");
					break;
				case 3:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11463");
					break;
				case 4:
				{
					string suggestedItem = itemPriceAndStock.ElementAt(Game1.random.Next(2, itemPriceAndStock.Count)).Key.DisplayName;
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11464", suggestedItem, Lexicon.getRandomPositiveAdjectiveForEventOrPerson(), Lexicon.getProperArticleForWord(suggestedItem));
					break;
				}
				}
				break;
			case "Clint":
				portraitPerson = Game1.getCharacterFromName("Clint");
				switch (Game1.random.Next(3))
				{
				case 0:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11469");
					break;
				case 1:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11470");
					break;
				case 2:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11471");
					break;
				}
				break;
			case "ClintUpgrade":
				portraitPerson = Game1.getCharacterFromName("Clint");
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11474");
				break;
			case "Willy":
				portraitPerson = Game1.getCharacterFromName("Willy");
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11477");
				if (Game1.random.NextDouble() < 0.05)
				{
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11478");
				}
				break;
			case "Pierre":
				portraitPerson = Game1.getCharacterFromName("Pierre");
				switch (Game1.dayOfMonth % 7)
				{
				case 1:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11481");
					break;
				case 2:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11482");
					break;
				case 3:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11483");
					break;
				case 4:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11484");
					break;
				case 5:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11485");
					break;
				case 6:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11486");
					break;
				case 0:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11487");
					break;
				}
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11488") + ppDialogue;
				if (Game1.dayOfMonth == 28)
				{
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11489");
				}
				break;
			case "Dwarf":
				portraitPerson = Game1.getCharacterFromName("Dwarf");
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11492");
				break;
			case "HatMouse":
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11494");
				break;
			case "BlueBoat":
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.blueboat");
				break;
			case "magicBoatShop":
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.magicBoat");
				break;
			case "KrobusGone":
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:KrobusShopGone");
				break;
			case "Krobus":
				portraitPerson = Game1.getCharacterFromName("Krobus");
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11497");
				break;
			case "TravelerNightMarket":
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.travelernightmarket");
				break;
			case "Traveler":
				switch (r.Next(5))
				{
				case 0:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11499");
					break;
				case 1:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11500");
					break;
				case 2:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11501");
					break;
				case 3:
					ppDialogue = ((itemPriceAndStock.Count <= 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11504") : Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11502", itemPriceAndStock.ElementAt(r.Next(itemPriceAndStock.Count)).Key.DisplayName));
					break;
				case 4:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11504");
					break;
				}
				break;
			case "Marnie":
				portraitPerson = Game1.getCharacterFromName("Marnie");
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11507");
				if (r.NextDouble() < 0.0001)
				{
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11508");
				}
				break;
			case "Gus":
				portraitPerson = Game1.getCharacterFromName("Gus");
				switch (Game1.random.Next(4))
				{
				case 0:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11511");
					break;
				case 1:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11512", itemPriceAndStock.ElementAt(r.Next(itemPriceAndStock.Count)).Key.DisplayName);
					break;
				case 2:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11513");
					break;
				case 3:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11514");
					break;
				}
				break;
			case "Marlon_Recovery":
				portraitPerson = Game1.getCharacterFromName("Marlon");
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemRecovery_Description");
				break;
			case "Marlon":
				portraitPerson = Game1.getCharacterFromName("Marlon");
				switch (r.Next(4))
				{
				case 0:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11517");
					break;
				case 1:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11518");
					break;
				case 2:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11519");
					break;
				case 3:
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11520");
					break;
				}
				if (r.NextDouble() < 0.001)
				{
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11521");
				}
				break;
			case "Sandy":
				portraitPerson = Game1.getCharacterFromName("Sandy");
				ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11524");
				if (r.NextDouble() < 0.0001)
				{
					ppDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11525");
				}
				break;
			}
			potraitPersonDialogue = Game1.parseText(ppDialogue, Game1.dialogueFont, 304);
		}

		public bool highlightItemToSell(Item i)
		{
			if (categoriesToSellHere.Contains(i.Category))
			{
				return true;
			}
			return false;
		}

		public static int getPlayerCurrencyAmount(Farmer who, int currencyType)
		{
			switch (currencyType)
			{
			case 0:
				return who.Money;
			case 1:
				return who.festivalScore;
			case 2:
				return who.clubCoins;
			default:
				return 0;
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (scrolling)
			{
				int y2 = scrollBar.bounds.Y;
				scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
				float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
				currentItemIndex = Math.Min(forSale.Count - 4, Math.Max(0, (int)((float)forSale.Count * percentage)));
				setScrollBarToCurrentIndex();
				updateSaleButtonNeighbors();
				if (y2 != scrollBar.bounds.Y)
				{
					Game1.playSound("shiny4");
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			scrolling = false;
		}

		private void setScrollBarToCurrentIndex()
		{
			if (forSale.Count > 0)
			{
				scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, forSale.Count - 4 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
				if (currentItemIndex == forSale.Count - 4)
				{
					scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
				}
			}
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && currentItemIndex > 0)
			{
				upArrowPressed();
				Game1.playSound("shiny4");
			}
			else if (direction < 0 && currentItemIndex < Math.Max(0, forSale.Count - 4))
			{
				downArrowPressed();
				Game1.playSound("shiny4");
			}
		}

		private void downArrowPressed()
		{
			downArrow.scale = downArrow.baseScale;
			currentItemIndex++;
			setScrollBarToCurrentIndex();
			updateSaleButtonNeighbors();
		}

		private void upArrowPressed()
		{
			upArrow.scale = upArrow.baseScale;
			currentItemIndex--;
			setScrollBarToCurrentIndex();
			updateSaleButtonNeighbors();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y);
			if (Game1.activeClickableMenu == null)
			{
				return;
			}
			Vector2 snappedPosition = inventory.snapToClickableComponent(x, y);
			if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, forSale.Count - 4))
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
			for (int k = 0; k < tabButtons.Count; k++)
			{
				if (tabButtons[k].containsPoint(x, y))
				{
					switchTab(k);
				}
			}
			currentItemIndex = Math.Max(0, Math.Min(forSale.Count - 4, currentItemIndex));
			if (heldItem == null)
			{
				Item toSell3 = inventory.leftClick(x, y, null, playSound: false);
				if (toSell3 != null)
				{
					if (onSell != null)
					{
						if (onSell(toSell3))
						{
							toSell3 = null;
						}
					}
					else
					{
						chargePlayer(Game1.player, currency, -((int)((toSell3 is Object) ? ((float)(toSell3 as Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(toSell3.salePrice() / 2) * sellPercentage)) * toSell3.Stack));
						int coins = toSell3.Stack / 8 + 2;
						for (int j = 0; j < coins; j++)
						{
							animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), flicker: false, flipped: false)
							{
								alphaFade = 0.025f,
								motion = new Vector2(Game1.random.Next(-3, 4), -4f),
								acceleration = new Vector2(0f, 0.5f),
								delayBeforeAnimationStart = j * 25,
								scale = 2f
							});
							animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), flicker: false, flipped: false)
							{
								scale = 4f,
								alphaFade = 0.025f,
								delayBeforeAnimationStart = j * 50,
								motion = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2(xPositionOnScreen - 36, yPositionOnScreen + height - inventory.height - 16), 8f),
								acceleration = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2(xPositionOnScreen - 36, yPositionOnScreen + height - inventory.height - 16), 0.5f)
							});
						}
						if (toSell3 is Object && (int)(toSell3 as Object).edibility != -300)
						{
							Item stackClone = toSell3.getOne();
							stackClone.Stack = toSell3.Stack;
							if (Game1.currentLocation is ShopLocation)
							{
								(Game1.currentLocation as ShopLocation).itemsToStartSellingTomorrow.Add(stackClone);
							}
						}
						toSell3 = null;
						Game1.playSound("sell");
						Game1.playSound("purchase");
						if (inventory.getItemAt(x, y) == null)
						{
							animations.Add(new TemporaryAnimatedSprite(5, snappedPosition + new Vector2(32f, 32f), Color.White)
							{
								motion = new Vector2(0f, -0.5f)
							});
						}
					}
					updateSaleButtonNeighbors();
				}
			}
			else
			{
				heldItem = inventory.leftClick(x, y, heldItem as Item);
			}
			for (int i = 0; i < forSaleButtons.Count; i++)
			{
				if (currentItemIndex + i >= forSale.Count || !forSaleButtons[i].containsPoint(x, y))
				{
					continue;
				}
				int index = currentItemIndex + i;
				if (forSale[index] != null)
				{
					int toBuy2 = (!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : Math.Min(Math.Min(5, getPlayerCurrencyAmount(Game1.player, currency) / Math.Max(1, itemPriceAndStock[forSale[index]][0])), Math.Max(1, itemPriceAndStock[forSale[index]][1]));
					toBuy2 = Math.Min(toBuy2, forSale[index].maximumStackSize());
					if (toBuy2 == -1)
					{
						toBuy2 = 1;
					}
					if (canPurchaseCheck != null && !canPurchaseCheck(index))
					{
						return;
					}
					if (toBuy2 > 0 && tryToPurchaseItem(forSale[index], heldItem, toBuy2, x, y, index))
					{
						itemPriceAndStock.Remove(forSale[index]);
						forSale.RemoveAt(index);
					}
					else if (toBuy2 <= 0)
					{
						Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
						Game1.playSound("cancel");
					}
					if (heldItem != null && (_isStorageShop || Game1.options.SnappyMenus || (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && heldItem.maximumStackSize() == 1)) && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(heldItem as Item))
					{
						heldItem = null;
						DelayedAction.playSoundAfterDelay("coin", 100);
					}
				}
				currentItemIndex = Math.Max(0, Math.Min(forSale.Count - 4, currentItemIndex));
				updateSaleButtonNeighbors();
				setScrollBarToCurrentIndex();
				return;
			}
			if (readyToClose() && (x < xPositionOnScreen - 64 || y < yPositionOnScreen - 64 || x > xPositionOnScreen + width + 128 || y > yPositionOnScreen + height + 64))
			{
				exitThisMenu();
			}
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (direction == 1 && tabButtons.Contains(a) && tabButtons.Contains(b))
			{
				return false;
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		public virtual void switchTab(int new_tab)
		{
			currentTab = new_tab;
			Game1.playSound("shwip");
			applyTab();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				snapCursorToCurrentSnappedComponent();
			}
		}

		public virtual void applyTab()
		{
			if (storeContext == "Dresser")
			{
				if (currentTab == 0)
				{
					forSale = itemPriceAndStock.Keys.ToList();
				}
				else
				{
					forSale.Clear();
					foreach (ISalable key in itemPriceAndStock.Keys)
					{
						Item item3 = key as Item;
						if (item3 != null)
						{
							if (currentTab == 1)
							{
								if (item3.Category == -95)
								{
									forSale.Add(item3);
								}
							}
							else if (currentTab == 2)
							{
								if (item3 is Clothing && (item3 as Clothing).clothesType.Value == 0)
								{
									forSale.Add(item3);
								}
							}
							else if (currentTab == 3)
							{
								if (item3 is Clothing && (item3 as Clothing).clothesType.Value == 1)
								{
									forSale.Add(item3);
								}
							}
							else if (currentTab == 4)
							{
								if (item3.Category == -97)
								{
									forSale.Add(item3);
								}
							}
							else if (currentTab == 5 && item3.Category == -96)
							{
								forSale.Add(item3);
							}
						}
					}
				}
			}
			else if (storeContext == "Catalogue")
			{
				if (currentTab == 0)
				{
					forSale = itemPriceAndStock.Keys.ToList();
				}
				else
				{
					forSale.Clear();
					foreach (ISalable key2 in itemPriceAndStock.Keys)
					{
						Item item2 = key2 as Item;
						if (item2 != null)
						{
							if (currentTab == 1)
							{
								if (item2 is Wallpaper && (item2 as Wallpaper).isFloor.Value)
								{
									forSale.Add(item2);
								}
							}
							else if (currentTab == 2 && item2 is Wallpaper && !(item2 as Wallpaper).isFloor.Value)
							{
								forSale.Add(item2);
							}
						}
					}
				}
			}
			else if (storeContext == "Furniture Catalogue")
			{
				if (currentTab == 0)
				{
					forSale = itemPriceAndStock.Keys.ToList();
				}
				else
				{
					forSale.Clear();
					foreach (ISalable key3 in itemPriceAndStock.Keys)
					{
						Item item = key3 as Item;
						if (item != null)
						{
							if (currentTab == 1)
							{
								if (item is Furniture && ((item as Furniture).furniture_type.Value == 5 || (item as Furniture).furniture_type.Value == 4 || (item as Furniture).furniture_type.Value == 11))
								{
									forSale.Add(item);
								}
							}
							else if (currentTab == 2)
							{
								if (item is Furniture && ((item as Furniture).furniture_type.Value == 0 || (item as Furniture).furniture_type.Value == 1 || (item as Furniture).furniture_type.Value == 2 || (item as Furniture).furniture_type.Value == 3))
								{
									forSale.Add(item);
								}
							}
							else if (currentTab == 3)
							{
								if (item is Furniture && ((item as Furniture).furniture_type.Value == 6 || (item as Furniture).furniture_type.Value == 13))
								{
									forSale.Add(item);
								}
							}
							else if (currentTab == 4)
							{
								if (item is Furniture && (item as Furniture).furniture_type.Value == 12)
								{
									forSale.Add(item);
								}
							}
							else if (currentTab == 5 && item is Furniture && ((item as Furniture).furniture_type.Value == 7 || (item as Furniture).furniture_type.Value == 10 || (item as Furniture).furniture_type.Value == 8 || (item as Furniture).furniture_type.Value == 9 || (item as Furniture).furniture_type.Value == 14))
							{
								forSale.Add(item);
							}
						}
					}
				}
			}
			currentItemIndex = 0;
			setScrollBarToCurrentIndex();
			updateSaleButtonNeighbors();
		}

		public override bool readyToClose()
		{
			if (heldItem == null)
			{
				return animations.Count == 0;
			}
			return false;
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			if (heldItem != null)
			{
				Game1.player.addItemToInventoryBool(heldItem as Item);
				Game1.playSound("coin");
			}
		}

		public static void chargePlayer(Farmer who, int currencyType, int amount)
		{
			switch (currencyType)
			{
			case 0:
				who.Money -= amount;
				break;
			case 1:
				who.festivalScore -= amount;
				break;
			case 2:
				who.clubCoins -= amount;
				break;
			}
		}

		private bool tryToPurchaseItem(ISalable item, ISalable held_item, int numberToBuy, int x, int y, int indexInForSaleList)
		{
			if (held_item == null)
			{
				if (itemPriceAndStock[item][1] == 0)
				{
					hoveredItem = null;
					return true;
				}
				if (item.GetSalableInstance().maximumStackSize() < numberToBuy)
				{
					numberToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
				}
				int price2 = itemPriceAndStock[item][0] * numberToBuy;
				int extraTradeItem2 = -1;
				int extraTradeItemCount2 = 5;
				if (itemPriceAndStock[item].Length > 2)
				{
					extraTradeItem2 = itemPriceAndStock[item][2];
					if (itemPriceAndStock[item].Length > 3)
					{
						extraTradeItemCount2 = itemPriceAndStock[item][3];
					}
					extraTradeItemCount2 *= numberToBuy;
				}
				if (getPlayerCurrencyAmount(Game1.player, currency) >= price2 && (extraTradeItem2 == -1 || Game1.player.hasItemInInventory(extraTradeItem2, extraTradeItemCount2)))
				{
					heldItem = item.GetSalableInstance();
					heldItem.Stack = numberToBuy;
					if (itemPriceAndStock[item][1] == int.MaxValue && item.Stack != int.MaxValue)
					{
						heldItem.Stack *= item.Stack;
					}
					if (!heldItem.CanBuyItem(Game1.player) && !item.IsInfiniteStock() && (!(heldItem is Object) || !(heldItem as Object).isRecipe))
					{
						Game1.playSound("smallSelect");
						heldItem = null;
						return false;
					}
					if (itemPriceAndStock[item][1] != int.MaxValue && !item.IsInfiniteStock())
					{
						itemPriceAndStock[item][1] -= numberToBuy;
						forSale[indexInForSaleList].Stack -= numberToBuy;
					}
					chargePlayer(Game1.player, currency, price2);
					if (extraTradeItem2 != -1)
					{
						Game1.player.removeItemsFromInventory(extraTradeItem2, extraTradeItemCount2);
					}
					if (item.actionWhenPurchased())
					{
						if (heldItem is Object && (bool)(heldItem as Object).isRecipe)
						{
							string recipeName = heldItem.Name.Substring(0, heldItem.Name.IndexOf("Recipe") - 1);
							try
							{
								if ((heldItem as Object).Category == -7)
								{
									Game1.player.cookingRecipes.Add(recipeName, 0);
								}
								else
								{
									Game1.player.craftingRecipes.Add(recipeName, 0);
								}
								Game1.playSound("newRecipe");
							}
							catch (Exception)
							{
							}
						}
						held_item = null;
						heldItem = null;
					}
					else if (Game1.mouseClickPolling > 300)
					{
						if (purchaseRepeatSound != null)
						{
							Game1.playSound(purchaseRepeatSound);
						}
					}
					else if (purchaseSound != null)
					{
						Game1.playSound(purchaseSound);
					}
					if (onPurchase != null && onPurchase(item, Game1.player, numberToBuy))
					{
						exitThisMenu();
					}
				}
				else
				{
					Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
					Game1.playSound("cancel");
				}
			}
			else if (held_item.canStackWith(item))
			{
				numberToBuy = Math.Min(numberToBuy, held_item.maximumStackSize() - held_item.Stack);
				if (numberToBuy > 0)
				{
					int price = itemPriceAndStock[item][0] * numberToBuy;
					int extraTradeItem = -1;
					int extraTradeItemCount = 5;
					if (itemPriceAndStock[item].Length > 2)
					{
						extraTradeItem = itemPriceAndStock[item][2];
						if (itemPriceAndStock[item].Length > 3)
						{
							extraTradeItemCount = itemPriceAndStock[item][3];
						}
						extraTradeItemCount *= numberToBuy;
					}
					int tmp = item.Stack;
					item.Stack = numberToBuy + heldItem.Stack;
					if (!item.CanBuyItem(Game1.player))
					{
						item.Stack = tmp;
						Game1.playSound("cancel");
						return false;
					}
					item.Stack = tmp;
					if (getPlayerCurrencyAmount(Game1.player, currency) >= price && (extraTradeItem == -1 || Game1.player.hasItemInInventory(extraTradeItem, extraTradeItemCount)))
					{
						int amountAddedToStack = numberToBuy;
						if (itemPriceAndStock[item][1] == int.MaxValue && item.Stack != int.MaxValue)
						{
							amountAddedToStack *= item.Stack;
						}
						heldItem.Stack += amountAddedToStack;
						if (itemPriceAndStock[item][1] != int.MaxValue && !item.IsInfiniteStock())
						{
							itemPriceAndStock[item][1] -= numberToBuy;
							forSale[indexInForSaleList].Stack -= numberToBuy;
						}
						chargePlayer(Game1.player, currency, price);
						if (Game1.mouseClickPolling > 300)
						{
							if (purchaseRepeatSound != null)
							{
								Game1.playSound(purchaseRepeatSound);
							}
						}
						else if (purchaseSound != null)
						{
							Game1.playSound(purchaseSound);
						}
						if (extraTradeItem != -1)
						{
							Game1.player.removeItemsFromInventory(extraTradeItem, extraTradeItemCount);
						}
						if (item.actionWhenPurchased())
						{
							heldItem = null;
						}
						if (onPurchase != null && onPurchase(item, Game1.player, numberToBuy))
						{
							exitThisMenu();
						}
					}
					else
					{
						Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
						Game1.playSound("cancel");
					}
				}
			}
			if (itemPriceAndStock[item][1] <= 0)
			{
				hoveredItem = null;
				return true;
			}
			return false;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			Vector2 snappedPosition = inventory.snapToClickableComponent(x, y);
			if (heldItem == null)
			{
				ISalable toSell3 = inventory.rightClick(x, y, null, playSound: false);
				if (toSell3 != null)
				{
					if (onSell != null)
					{
						if (onSell(toSell3))
						{
							toSell3 = null;
						}
					}
					else
					{
						chargePlayer(Game1.player, currency, -((int)((toSell3 is Object) ? ((float)(toSell3 as Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(toSell3.salePrice() / 2) * sellPercentage)) * toSell3.Stack));
						toSell3 = null;
						if (Game1.mouseClickPolling > 300)
						{
							if (purchaseRepeatSound != null)
							{
								Game1.playSound(purchaseRepeatSound);
							}
						}
						else if (purchaseSound != null)
						{
							Game1.playSound(purchaseSound);
						}
						int coins = 2;
						for (int j = 0; j < coins; j++)
						{
							animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), flicker: false, flipped: false)
							{
								alphaFade = 0.025f,
								motion = new Vector2(Game1.random.Next(-3, 4), -4f),
								acceleration = new Vector2(0f, 0.5f),
								delayBeforeAnimationStart = j * 25,
								scale = 2f
							});
							animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), flicker: false, flipped: false)
							{
								scale = 4f,
								alphaFade = 0.025f,
								delayBeforeAnimationStart = j * 50,
								motion = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2(xPositionOnScreen - 36, yPositionOnScreen + height - inventory.height - 16), 8f),
								acceleration = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2(xPositionOnScreen - 36, yPositionOnScreen + height - inventory.height - 16), 0.5f)
							});
						}
						if (toSell3 is Object && (int)(toSell3 as Object).edibility != -300 && Game1.random.NextDouble() < 0.039999999105930328 && Game1.currentLocation is ShopLocation)
						{
							(Game1.currentLocation as ShopLocation).itemsToStartSellingTomorrow.Add((Item)toSell3.GetSalableInstance());
						}
						if (inventory.getItemAt(x, y) == null)
						{
							Game1.playSound("sell");
							animations.Add(new TemporaryAnimatedSprite(5, snappedPosition + new Vector2(32f, 32f), Color.White)
							{
								motion = new Vector2(0f, -0.5f)
							});
						}
					}
				}
			}
			else
			{
				heldItem = inventory.rightClick(x, y, heldItem as Item);
			}
			int i = 0;
			while (true)
			{
				if (i < forSaleButtons.Count)
				{
					if (currentItemIndex + i < forSale.Count && forSaleButtons[i].containsPoint(x, y))
					{
						break;
					}
					i++;
					continue;
				}
				return;
			}
			int index = currentItemIndex + i;
			if (forSale[index] == null)
			{
				return;
			}
			int toBuy = 1;
			if (itemPriceAndStock[forSale[index]][0] > 0)
			{
				toBuy = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : Math.Min(Math.Min(5, getPlayerCurrencyAmount(Game1.player, currency) / itemPriceAndStock[forSale[index]][0]), itemPriceAndStock[forSale[index]][1]));
			}
			if (canPurchaseCheck == null || canPurchaseCheck(index))
			{
				if (toBuy > 0 && tryToPurchaseItem(forSale[index], heldItem, toBuy, x, y, index))
				{
					itemPriceAndStock.Remove(forSale[index]);
					forSale.RemoveAt(index);
				}
				if (heldItem != null && (_isStorageShop || Game1.options.SnappyMenus) && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(heldItem as Item))
				{
					heldItem = null;
					DelayedAction.playSoundAfterDelay("coin", 100);
				}
				setScrollBarToCurrentIndex();
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			descriptionText = "";
			hoverText = "";
			hoveredItem = null;
			hoverPrice = -1;
			boldTitleText = "";
			upArrow.tryHover(x, y);
			downArrow.tryHover(x, y);
			scrollBar.tryHover(x, y);
			if (scrolling)
			{
				return;
			}
			for (int j = 0; j < forSaleButtons.Count; j++)
			{
				if (currentItemIndex + j < forSale.Count && forSaleButtons[j].containsPoint(x, y))
				{
					ISalable item = forSale[currentItemIndex + j];
					if (canPurchaseCheck == null || canPurchaseCheck(currentItemIndex + j))
					{
						hoverText = item.getDescription();
						boldTitleText = item.DisplayName;
						if (!_isStorageShop)
						{
							hoverPrice = ((itemPriceAndStock != null && itemPriceAndStock.ContainsKey(item)) ? itemPriceAndStock[item][0] : item.salePrice());
						}
						hoveredItem = item;
						forSaleButtons[j].scale = Math.Min(forSaleButtons[j].scale + 0.03f, 1.1f);
					}
				}
				else
				{
					forSaleButtons[j].scale = Math.Max(1f, forSaleButtons[j].scale - 0.03f);
				}
			}
			if (heldItem == null)
			{
				foreach (ClickableComponent c in inventory.inventory)
				{
					if (c.containsPoint(x, y))
					{
						Item i = inventory.getItemFromClickableComponent(c);
						if (i != null && (inventory.highlightMethod == null || inventory.highlightMethod(i)))
						{
							if (_isStorageShop)
							{
								hoverText = i.getDescription();
								boldTitleText = i.DisplayName;
								hoveredItem = i;
							}
							else
							{
								hoverText = i.DisplayName + " x" + i.Stack;
								Object hovered_object;
								if ((hovered_object = (i as Object)) != null && hovered_object.needsToBeDonated())
								{
									hoverText = hoverText + "\n\n" + i.getDescription() + "\n";
								}
								hoverPrice = (int)((i is Object) ? ((float)(i as Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(i.salePrice() / 2) * sellPercentage)) * i.Stack;
							}
						}
					}
				}
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (poof != null && poof.update(time))
			{
				poof = null;
			}
			repositionTabs();
		}

		public void drawCurrency(SpriteBatch b)
		{
			if (!_isStorageShop)
			{
				if (currency != 0)
				{
					_ = 1;
				}
				else
				{
					Game1.dayTimeMoneyBox.drawMoneyBox(b, xPositionOnScreen - 36, yPositionOnScreen + height - inventory.height - 12);
				}
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (b != Buttons.RightTrigger && b != Buttons.LeftTrigger)
			{
				return;
			}
			if (currentlySnappedComponent != null && currentlySnappedComponent.myID >= 3546)
			{
				int emptySlot = -1;
				for (int i = 0; i < 12; i++)
				{
					inventory.inventory[i].upNeighborID = 3546 + forSaleButtons.Count - 1;
					if (emptySlot == -1 && heldItem != null && inventory.actualInventory != null && inventory.actualInventory.Count > i && inventory.actualInventory[i] == null)
					{
						emptySlot = i;
					}
				}
				currentlySnappedComponent = getComponentWithID((emptySlot != -1) ? emptySlot : 0);
				snapCursorToCurrentSnappedComponent();
			}
			else
			{
				snapToDefaultClickableComponent();
			}
			Game1.playSound("shiny4");
		}

		private int getHoveredItemExtraItemIndex()
		{
			if (itemPriceAndStock != null && hoveredItem != null && itemPriceAndStock.ContainsKey(hoveredItem) && itemPriceAndStock[hoveredItem].Length > 2)
			{
				return itemPriceAndStock[hoveredItem][2];
			}
			return -1;
		}

		private int getHoveredItemExtraItemAmount()
		{
			if (itemPriceAndStock != null && hoveredItem != null && itemPriceAndStock.ContainsKey(hoveredItem) && itemPriceAndStock[hoveredItem].Length > 3)
			{
				return itemPriceAndStock[hoveredItem][3];
			}
			return 5;
		}

		public void updatePosition()
		{
			width = 1000 + IClickableMenu.borderWidth * 2;
			height = 600 + IClickableMenu.borderWidth * 2;
			xPositionOnScreen = Game1.viewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2;
			yPositionOnScreen = Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;
			int num = xPositionOnScreen - 320;
			bool has_portrait_to_draw = false;
			if (portraitPerson != null)
			{
				has_portrait_to_draw = true;
			}
			if (potraitPersonDialogue != null && potraitPersonDialogue != "")
			{
				has_portrait_to_draw = true;
			}
			if (!((num > 0 && Game1.options.showMerchantPortraits) & has_portrait_to_draw))
			{
				xPositionOnScreen = Game1.viewport.Width / 2 - (1000 + IClickableMenu.borderWidth * 2) / 2;
				yPositionOnScreen = Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			updatePosition();
			initializeUpperRightCloseButton();
			Game1.player.forceCanMove();
			inventory = new InventoryMenu(xPositionOnScreen + width, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, playerInventory: false, null, highlightItemToSell)
			{
				showGrayedOutSlots = true
			};
			inventory.movePosition(-inventory.width - 32, 0);
			upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
			downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
			scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
			scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 64 - upArrow.bounds.Height - 28);
			forSaleButtons.Clear();
			for (int i = 0; i < 4; i++)
			{
				forSaleButtons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16 + i * ((height - 256) / 4), width - 32, (height - 256) / 4 + 4), string.Concat(i)));
			}
			if (tabButtons.Count > 0)
			{
				foreach (ClickableComponent forSaleButton in forSaleButtons)
				{
					forSaleButton.leftNeighborID = -99998;
				}
			}
			repositionTabs();
			foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Top))
			{
				item.upNeighborID = -99998;
			}
		}

		public void setItemPriceAndStock(Dictionary<ISalable, int[]> new_stock)
		{
			itemPriceAndStock = new_stock;
			forSale = itemPriceAndStock.Keys.ToList();
			applyTab();
		}

		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showMenuBackground)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen + width - inventory.width - 32 - 24, yPositionOnScreen + height - 256 + 40, inventory.width + 56, height - 448 + 20, Color.White, 4f);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen, yPositionOnScreen, width, height - 256 + 32 + 4, Color.White, 4f);
			drawCurrency(b);
			for (int k = 0; k < forSaleButtons.Count; k++)
			{
				if (currentItemIndex + k >= forSale.Count)
				{
					continue;
				}
				bool failedCanPurchaseCheck = false;
				if (canPurchaseCheck != null && !canPurchaseCheck(currentItemIndex + k))
				{
					failedCanPurchaseCheck = true;
				}
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), forSaleButtons[k].bounds.X, forSaleButtons[k].bounds.Y, forSaleButtons[k].bounds.Width, forSaleButtons[k].bounds.Height, (forSaleButtons[k].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !scrolling) ? Color.Wheat : Color.White, 4f, drawShadow: false);
				b.Draw(Game1.mouseCursors, new Vector2(forSaleButtons[k].bounds.X + 32 - 12, forSaleButtons[k].bounds.Y + 24 - 4), new Rectangle(296, 363, 18, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				ISalable item = forSale[currentItemIndex + k];
				bool num = item.Stack > 1 && item.Stack != int.MaxValue && itemPriceAndStock[item][1] == int.MaxValue;
				StackDrawType stackDrawType;
				if (itemPriceAndStock[item][1] == int.MaxValue)
				{
					stackDrawType = StackDrawType.Hide;
				}
				else
				{
					stackDrawType = StackDrawType.Draw_OneInclusive;
					if (_isStorageShop)
					{
						stackDrawType = StackDrawType.Draw;
					}
				}
				forSale[currentItemIndex + k].drawInMenu(b, new Vector2(forSaleButtons[k].bounds.X + 32 - 8, forSaleButtons[k].bounds.Y + 24), 1f, 1f, 0.9f, stackDrawType, Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f), drawShadow: true);
				string displayName = item.DisplayName;
				if (num)
				{
					displayName = displayName + " x" + item.Stack;
				}
				SpriteText.drawString(b, displayName, forSaleButtons[k].bounds.X + 96 + 8, forSaleButtons[k].bounds.Y + 28, 999999, -1, 999999, failedCanPurchaseCheck ? 0.5f : 1f);
				if (itemPriceAndStock[forSale[currentItemIndex + k]][0] > 0)
				{
					SpriteText.drawString(b, itemPriceAndStock[forSale[currentItemIndex + k]][0] + " ", forSaleButtons[k].bounds.Right - SpriteText.getWidthOfString(itemPriceAndStock[forSale[currentItemIndex + k]][0] + " ") - 60, forSaleButtons[k].bounds.Y + 28, 999999, -1, 999999, (getPlayerCurrencyAmount(Game1.player, currency) >= itemPriceAndStock[forSale[currentItemIndex + k]][0] && !failedCanPurchaseCheck) ? 1f : 0.5f);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(forSaleButtons[k].bounds.Right - 52, forSaleButtons[k].bounds.Y + 40 - 4), new Rectangle(193 + currency * 9, 373, 9, 10), Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f), 0f, Vector2.Zero, 4f, flipped: false, 1f, -1, -1, (!failedCanPurchaseCheck) ? 0.35f : 0f);
				}
				else if (itemPriceAndStock[forSale[currentItemIndex + k]].Length > 2)
				{
					int required_item_count = 5;
					int requiredItem = itemPriceAndStock[forSale[currentItemIndex + k]][2];
					if (itemPriceAndStock[forSale[currentItemIndex + k]].Length > 3)
					{
						required_item_count = itemPriceAndStock[forSale[currentItemIndex + k]][3];
					}
					bool hasEnoughToTrade = Game1.player.hasItemInInventory(requiredItem, required_item_count);
					if (canPurchaseCheck != null && !canPurchaseCheck(currentItemIndex + k))
					{
						hasEnoughToTrade = false;
					}
					float textWidth = SpriteText.getWidthOfString("x" + required_item_count);
					Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2((float)(forSaleButtons[k].bounds.Right - 88) - textWidth, forSaleButtons[k].bounds.Y + 28 - 4), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, requiredItem, 16, 16), Color.White * (hasEnoughToTrade ? 1f : 0.25f), 0f, Vector2.Zero, -1f, flipped: false, -1f, -1, -1, hasEnoughToTrade ? 0.35f : 0f);
					SpriteText.drawString(b, "x" + required_item_count, forSaleButtons[k].bounds.Right - (int)textWidth - 16, forSaleButtons[k].bounds.Y + 44, 999999, -1, 999999, hasEnoughToTrade ? 1f : 0.5f);
				}
			}
			if (forSale.Count == 0 && !_isStorageShop)
			{
				SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), xPositionOnScreen + width / 2 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583")) / 2, yPositionOnScreen + height / 2 - 128);
			}
			inventory.draw(b);
			for (int j = animations.Count - 1; j >= 0; j--)
			{
				if (animations[j].update(Game1.currentGameTime))
				{
					animations.RemoveAt(j);
				}
				else
				{
					animations[j].draw(b, localPosition: true);
				}
			}
			if (poof != null)
			{
				poof.draw(b);
			}
			upArrow.draw(b);
			downArrow.draw(b);
			for (int i = 0; i < tabButtons.Count; i++)
			{
				tabButtons[i].draw(b);
			}
			if (forSale.Count > 4)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
				scrollBar.draw(b);
			}
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawToolTip(b, hoverText, boldTitleText, hoveredItem as Item, heldItem != null, -1, currency, getHoveredItemExtraItemIndex(), getHoveredItemExtraItemAmount(), null, (hoverPrice > 0) ? hoverPrice : (-1));
			}
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);
			}
			base.draw(b);
			int portrait_draw_position2 = xPositionOnScreen - 320;
			if (portrait_draw_position2 > 0 && Game1.options.showMerchantPortraits)
			{
				if (portraitPerson != null)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(portrait_draw_position2, yPositionOnScreen), new Rectangle(603, 414, 74, 74), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.91f);
					if (portraitPerson.Portrait != null)
					{
						b.Draw(portraitPerson.Portrait, new Vector2(portrait_draw_position2 + 20, yPositionOnScreen + 20), new Rectangle(0, 0, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.92f);
					}
				}
				if (potraitPersonDialogue != null)
				{
					portrait_draw_position2 = xPositionOnScreen - (int)Game1.dialogueFont.MeasureString(potraitPersonDialogue).X - 64;
					if (portrait_draw_position2 > 0)
					{
						IClickableMenu.drawHoverText(b, potraitPersonDialogue, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, portrait_draw_position2, yPositionOnScreen + ((portraitPerson != null) ? 312 : 0));
					}
				}
			}
			drawMouse(b);
		}
	}
}
