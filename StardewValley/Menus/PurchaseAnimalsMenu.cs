using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class PurchaseAnimalsMenu : IClickableMenu
	{
		public const int region_okButton = 101;

		public const int region_doneNamingButton = 102;

		public const int region_randomButton = 103;

		public const int region_namingBox = 104;

		public static int menuHeight = 320;

		public static int menuWidth = 448;

		public List<ClickableTextureComponent> animalsToPurchase = new List<ClickableTextureComponent>();

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent doneNamingButton;

		public ClickableTextureComponent randomButton;

		public ClickableTextureComponent hovered;

		public ClickableComponent textBoxCC;

		private bool onFarm;

		private bool namingAnimal;

		private bool freeze;

		private FarmAnimal animalBeingPurchased;

		private TextBox textBox;

		private TextBoxEvent e;

		private Building newAnimalHome;

		private int priceOfAnimal;

		public bool readOnly;

		public PurchaseAnimalsMenu(List<Object> stock)
			: base(Game1.uiViewport.Width / 2 - menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - menuHeight - IClickableMenu.borderWidth * 2) / 4, menuWidth + IClickableMenu.borderWidth * 2, menuHeight + IClickableMenu.borderWidth)
		{
			height += 64;
			for (int i = 0; i < stock.Count; i++)
			{
				animalsToPurchase.Add(new ClickableTextureComponent(string.Concat(stock[i].salePrice()), new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + IClickableMenu.borderWidth + i % 3 * 64 * 2, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 + i / 3 * 85, 128, 64), null, stock[i].Name, (i >= 9) ? Game1.mouseCursors2 : Game1.mouseCursors, (i >= 9) ? new Microsoft.Xna.Framework.Rectangle(128 + i % 3 * 16 * 2, i / 3 * 16, 32, 16) : new Microsoft.Xna.Framework.Rectangle(i % 3 * 16 * 2, 448 + i / 3 * 16, 32, 16), 4f, stock[i].Type == null)
				{
					item = stock[i],
					myID = i,
					rightNeighborID = ((i % 3 == 2) ? (-1) : (i + 1)),
					leftNeighborID = ((i % 3 == 0) ? (-1) : (i - 1)),
					downNeighborID = i + 3,
					upNeighborID = i - 3
				});
			}
			okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 101,
				upNeighborID = 103,
				leftNeighborID = 103
			};
			randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width + 51 + 64, Game1.uiViewport.Height / 2, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f)
			{
				myID = 103,
				downNeighborID = 101,
				rightNeighborID = 101
			};
			menuHeight = 320;
			menuWidth = 448;
			textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
			textBox.X = Game1.uiViewport.Width / 2 - 192;
			textBox.Y = Game1.uiViewport.Height / 2;
			textBox.Width = 256;
			textBox.Height = 192;
			e = textBoxEnter;
			textBoxCC = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X, textBox.Y, 192, 48), "")
			{
				myID = 104,
				rightNeighborID = 102,
				downNeighborID = 101
			};
			randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X + textBox.Width + 64 + 48 - 8, Game1.uiViewport.Height / 2 + 4, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f)
			{
				myID = 103,
				leftNeighborID = 102,
				downNeighborID = 101,
				rightNeighborID = 101
			};
			doneNamingButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X + textBox.Width + 32 + 4, Game1.uiViewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 102,
				rightNeighborID = 103,
				leftNeighborID = 104,
				downNeighborID = 101
			};
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override bool shouldClampGamePadCursor()
		{
			return onFarm;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public void textBoxEnter(TextBox sender)
		{
			if (!namingAnimal)
			{
				return;
			}
			if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is PurchaseAnimalsMenu))
			{
				textBox.OnEnterPressed -= e;
			}
			else if (sender.Text.Length >= 1)
			{
				if (Utility.areThereAnyOtherAnimalsWithThisName(sender.Text))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11308"));
					return;
				}
				textBox.OnEnterPressed -= e;
				animalBeingPurchased.Name = sender.Text;
				animalBeingPurchased.displayName = sender.Text;
				animalBeingPurchased.home = newAnimalHome;
				animalBeingPurchased.homeLocation.Value = new Vector2((int)newAnimalHome.tileX, (int)newAnimalHome.tileY);
				animalBeingPurchased.setRandomPosition(animalBeingPurchased.home.indoors);
				(newAnimalHome.indoors.Value as AnimalHouse).animals.Add(animalBeingPurchased.myID, animalBeingPurchased);
				(newAnimalHome.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(animalBeingPurchased.myID);
				newAnimalHome = null;
				namingAnimal = false;
				Game1.player.Money -= priceOfAnimal;
				setUpForReturnAfterPurchasingAnimal();
			}
		}

		public void setUpForReturnAfterPurchasingAnimal()
		{
			LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop");
			locationRequest.OnWarp += delegate
			{
				onFarm = false;
				Game1.player.viewingLocation.Value = null;
				okButton.bounds.X = xPositionOnScreen + width + 4;
				Game1.displayHUD = true;
				Game1.displayFarmer = true;
				freeze = false;
				textBox.OnEnterPressed -= e;
				textBox.Selected = false;
				Game1.viewportFreeze = false;
				marnieAnimalPurchaseMessage();
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
		}

		public void marnieAnimalPurchaseMessage()
		{
			exitThisMenu();
			Game1.player.forceCanMove();
			freeze = false;
			Game1.drawDialogue(Game1.getCharacterFromName("Marnie"), animalBeingPurchased.isMale() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11311", animalBeingPurchased.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11314", animalBeingPurchased.displayName));
		}

		public void setUpForAnimalPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.displayFarmer = false;
			Game1.currentLocation = Game1.getLocationFromName("Farm");
			Game1.player.viewingLocation.Value = "Farm";
			Game1.currentLocation.resetForPlayerEntry();
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.globalFadeToClear();
			onFarm = true;
			freeze = false;
			okButton.bounds.X = Game1.uiViewport.Width - 128;
			okButton.bounds.Y = Game1.uiViewport.Height - 128;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.viewport.Location = new Location(3136, 320);
			Game1.panScreen(0, 0);
		}

		public void setUpForReturnToShopMenu()
		{
			freeze = false;
			Game1.displayFarmer = true;
			LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop");
			locationRequest.OnWarp += delegate
			{
				onFarm = false;
				Game1.player.viewingLocation.Value = null;
				okButton.bounds.X = xPositionOnScreen + width + 4;
				okButton.bounds.Y = yPositionOnScreen + height - 64 - IClickableMenu.borderWidth;
				Game1.displayHUD = true;
				Game1.viewportFreeze = false;
				namingAnimal = false;
				textBox.OnEnterPressed -= e;
				textBox.Selected = false;
				if (Game1.options.SnappyMenus)
				{
					snapToDefaultClickableComponent();
				}
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.IsFading() || freeze)
			{
				return;
			}
			if (okButton != null && okButton.containsPoint(x, y) && readyToClose())
			{
				if (onFarm)
				{
					setUpForReturnToShopMenu();
					Game1.playSound("smallSelect");
				}
				else
				{
					Game1.exitActiveMenu();
					Game1.playSound("bigDeSelect");
				}
			}
			if (onFarm)
			{
				Vector2 clickTile = new Vector2((int)((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f), (int)((Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f));
				Building selection = (Game1.getLocationFromName("Farm") as Farm).getBuildingAt(clickTile);
				if (selection != null && !namingAnimal)
				{
					if (selection.buildingType.Value.Contains(animalBeingPurchased.buildingTypeILiveIn.Value))
					{
						if ((selection.indoors.Value as AnimalHouse).isFull())
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11321"));
						}
						else if ((byte)animalBeingPurchased.harvestType != 2)
						{
							namingAnimal = true;
							newAnimalHome = selection;
							if (animalBeingPurchased.sound.Value != null && Game1.soundBank != null)
							{
								ICue cue = Game1.soundBank.GetCue(animalBeingPurchased.sound.Value);
								cue.SetVariable("Pitch", 1200 + Game1.random.Next(-200, 201));
								cue.Play();
							}
							textBox.OnEnterPressed += e;
							textBox.Text = animalBeingPurchased.displayName;
							Game1.keyboardDispatcher.Subscriber = textBox;
							if (Game1.options.SnappyMenus)
							{
								currentlySnappedComponent = getComponentWithID(104);
								snapCursorToCurrentSnappedComponent();
							}
						}
						else if (Game1.player.Money >= priceOfAnimal)
						{
							newAnimalHome = selection;
							animalBeingPurchased.home = newAnimalHome;
							animalBeingPurchased.homeLocation.Value = new Vector2((int)newAnimalHome.tileX, (int)newAnimalHome.tileY);
							animalBeingPurchased.setRandomPosition(animalBeingPurchased.home.indoors);
							(newAnimalHome.indoors.Value as AnimalHouse).animals.Add(animalBeingPurchased.myID, animalBeingPurchased);
							(newAnimalHome.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(animalBeingPurchased.myID);
							newAnimalHome = null;
							namingAnimal = false;
							if (animalBeingPurchased.sound.Value != null && Game1.soundBank != null)
							{
								ICue cue2 = Game1.soundBank.GetCue(animalBeingPurchased.sound.Value);
								cue2.SetVariable("Pitch", 1200 + Game1.random.Next(-200, 201));
								cue2.Play();
							}
							Game1.player.Money -= priceOfAnimal;
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11324", animalBeingPurchased.displayType), Color.LimeGreen, 3500f));
							animalBeingPurchased = new FarmAnimal(animalBeingPurchased.type, Game1.multiplayer.getNewID(), Game1.player.uniqueMultiplayerID);
						}
						else if (Game1.player.Money < priceOfAnimal)
						{
							Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
						}
					}
					else
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11326", animalBeingPurchased.displayType));
					}
				}
				if (namingAnimal)
				{
					if (doneNamingButton.containsPoint(x, y))
					{
						textBoxEnter(textBox);
						Game1.playSound("smallSelect");
					}
					else if (namingAnimal && randomButton.containsPoint(x, y))
					{
						animalBeingPurchased.Name = Dialogue.randomName();
						animalBeingPurchased.displayName = animalBeingPurchased.Name;
						textBox.Text = animalBeingPurchased.displayName;
						randomButton.scale = randomButton.baseScale;
						Game1.playSound("drumkit6");
					}
					textBox.Update();
				}
			}
			else
			{
				foreach (ClickableTextureComponent c in animalsToPurchase)
				{
					if (!readOnly && c.containsPoint(x, y) && (c.item as Object).Type == null)
					{
						int price = c.item.salePrice();
						if (Game1.player.Money >= price)
						{
							Game1.globalFadeToBlack(setUpForAnimalPlacement);
							Game1.playSound("smallSelect");
							onFarm = true;
							animalBeingPurchased = new FarmAnimal(c.hoverText, Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
							priceOfAnimal = price;
						}
						else
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"), Color.Red, 3500f));
						}
					}
				}
			}
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			if (onFarm)
			{
				return !namingAnimal;
			}
			return false;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (b == Buttons.B && !Game1.globalFade && onFarm && namingAnimal)
			{
				setUpForReturnToShopMenu();
				Game1.playSound("smallSelect");
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade || freeze)
			{
				return;
			}
			if (!Game1.globalFade && onFarm)
			{
				if (!namingAnimal)
				{
					if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose() && !Game1.IsFading())
					{
						setUpForReturnToShopMenu();
					}
					else if (!Game1.options.SnappyMenus)
					{
						if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
						{
							Game1.panScreen(0, 4);
						}
						else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
						{
							Game1.panScreen(4, 0);
						}
						else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
						{
							Game1.panScreen(0, -4);
						}
						else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
						{
							Game1.panScreen(-4, 0);
						}
					}
				}
				else if (Game1.options.SnappyMenus)
				{
					if (!textBox.Selected && Game1.options.doesInputListContain(Game1.options.menuButton, key))
					{
						setUpForReturnToShopMenu();
						Game1.playSound("smallSelect");
					}
					else if (!textBox.Selected || !Game1.options.doesInputListContain(Game1.options.menuButton, key))
					{
						base.receiveKeyPress(key);
					}
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.IsFading())
			{
				if (readyToClose())
				{
					Game1.player.forceCanMove();
					Game1.exitActiveMenu();
					Game1.playSound("bigDeSelect");
				}
			}
			else if (Game1.options.SnappyMenus)
			{
				base.receiveKeyPress(key);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (onFarm && !namingAnimal)
			{
				int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
				int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
				if (mouseX - Game1.viewport.X < 64)
				{
					Game1.panScreen(-8, 0);
				}
				else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -64)
				{
					Game1.panScreen(8, 0);
				}
				if (mouseY - Game1.viewport.Y < 64)
				{
					Game1.panScreen(0, -8);
				}
				else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
				{
					Game1.panScreen(0, 8);
				}
				Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
				foreach (Keys key in pressedKeys)
				{
					receiveKeyPress(key);
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			hovered = null;
			if (Game1.IsFading() || freeze)
			{
				return;
			}
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
			if (onFarm)
			{
				if (!namingAnimal)
				{
					Vector2 clickTile = new Vector2((int)((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f), (int)((Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f));
					Farm f = Game1.getLocationFromName("Farm") as Farm;
					foreach (Building building in f.buildings)
					{
						building.color.Value = Color.White;
					}
					Building selection = f.getBuildingAt(clickTile);
					if (selection != null)
					{
						if (selection.buildingType.Value.Contains(animalBeingPurchased.buildingTypeILiveIn.Value) && !(selection.indoors.Value as AnimalHouse).isFull())
						{
							selection.color.Value = Color.LightGreen * 0.8f;
						}
						else
						{
							selection.color.Value = Color.Red * 0.8f;
						}
					}
				}
				if (doneNamingButton != null)
				{
					if (doneNamingButton.containsPoint(x, y))
					{
						doneNamingButton.scale = Math.Min(1.1f, doneNamingButton.scale + 0.05f);
					}
					else
					{
						doneNamingButton.scale = Math.Max(1f, doneNamingButton.scale - 0.05f);
					}
				}
				randomButton.tryHover(x, y, 0.5f);
			}
			else
			{
				foreach (ClickableTextureComponent c in animalsToPurchase)
				{
					if (c.containsPoint(x, y))
					{
						c.scale = Math.Min(c.scale + 0.05f, 4.1f);
						hovered = c;
					}
					else
					{
						c.scale = Math.Max(4f, c.scale - 0.025f);
					}
				}
			}
		}

		public static string getAnimalTitle(string name)
		{
			switch (name)
			{
			case "Chicken":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5922");
			case "Duck":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5937");
			case "Rabbit":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5945");
			case "Dairy Cow":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5927");
			case "Pig":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5948");
			case "Goat":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5933");
			case "Sheep":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5942");
			case "Ostrich":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Ostrich_Name");
			default:
				return "";
			}
		}

		public static string getAnimalDescription(string name)
		{
			switch (name)
			{
			case "Chicken":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11334") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
			case "Duck":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11337") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
			case "Rabbit":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11340") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
			case "Dairy Cow":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11343") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
			case "Pig":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11346") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
			case "Goat":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11349") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
			case "Sheep":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11352") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
			case "Ostrich":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Ostrich_Description") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
			default:
				return "";
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!onFarm && !Game1.dialogueUp && !Game1.IsFading())
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11354"), xPositionOnScreen + 96, yPositionOnScreen);
				Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
				Game1.dayTimeMoneyBox.drawMoneyBox(b);
				foreach (ClickableTextureComponent c in animalsToPurchase)
				{
					c.draw(b, ((c.item as Object).Type != null) ? (Color.Black * 0.4f) : Color.White, 0.87f);
				}
			}
			else if (!Game1.IsFading() && onFarm)
			{
				string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11355", animalBeingPurchased.displayHouse, animalBeingPurchased.displayType);
				SpriteText.drawStringWithScrollBackground(b, s, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, 16);
				if (namingAnimal)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
					Game1.drawDialogueBox(Game1.uiViewport.Width / 2 - 256, Game1.uiViewport.Height / 2 - 192 - 32, 512, 192, speaker: false, drawOnlyBox: true);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11357"), Game1.dialogueFont, new Vector2(Game1.uiViewport.Width / 2 - 256 + 32 + 8, Game1.uiViewport.Height / 2 - 128 + 8), Game1.textColor);
					textBox.Draw(b);
					doneNamingButton.draw(b);
					randomButton.draw(b);
				}
			}
			if (!Game1.IsFading() && okButton != null)
			{
				okButton.draw(b);
			}
			if (hovered != null)
			{
				if ((hovered.item as Object).Type != null)
				{
					IClickableMenu.drawHoverText(b, Game1.parseText((hovered.item as Object).Type, Game1.dialogueFont, 320), Game1.dialogueFont);
				}
				else
				{
					string displayName = getAnimalTitle(hovered.hoverText);
					SpriteText.drawStringWithScrollBackground(b, displayName, xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 64, yPositionOnScreen + height + -32 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "Truffle Pig");
					SpriteText.drawStringWithScrollBackground(b, "$" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", hovered.item.salePrice()), xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 128, yPositionOnScreen + height + 64 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "$99999999g", (Game1.player.Money >= hovered.item.salePrice()) ? 1f : 0.5f);
					string description = getAnimalDescription(hovered.hoverText);
					IClickableMenu.drawHoverText(b, Game1.parseText(description, Game1.smallFont, 320), Game1.smallFont, 0, 0, -1, displayName);
				}
			}
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
		}
	}
}
