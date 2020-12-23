using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class CarpenterMenu : IClickableMenu
	{
		public const int region_backButton = 101;

		public const int region_forwardButton = 102;

		public const int region_upgradeIcon = 103;

		public const int region_demolishButton = 104;

		public const int region_moveBuitton = 105;

		public const int region_okButton = 106;

		public const int region_cancelButton = 107;

		public const int region_paintButton = 108;

		public int maxWidthOfBuildingViewer = 448;

		public int maxHeightOfBuildingViewer = 512;

		public int maxWidthOfDescription = 416;

		private List<BluePrint> blueprints;

		private int currentBlueprintIndex;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent cancelButton;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent upgradeIcon;

		public ClickableTextureComponent demolishButton;

		public ClickableTextureComponent moveButton;

		public ClickableTextureComponent paintButton;

		private Building currentBuilding;

		private Building buildingToMove;

		private string buildingDescription;

		private string buildingName;

		private List<Item> ingredients = new List<Item>();

		private int price;

		private bool onFarm;

		private bool drawBG = true;

		private bool freeze;

		private bool upgrading;

		private bool demolishing;

		private bool moving;

		private bool magicalConstruction;

		private bool painting;

		protected BluePrint _demolishCheckBlueprint;

		private string hoverText = "";

		public bool readOnly
		{
			set
			{
				if (value)
				{
					upgradeIcon.visible = false;
					demolishButton.visible = false;
					moveButton.visible = false;
					okButton.visible = false;
					paintButton.visible = false;
					cancelButton.leftNeighborID = 102;
				}
			}
		}

		public BluePrint CurrentBlueprint => blueprints[currentBlueprintIndex];

		public CarpenterMenu(bool magicalConstruction = false)
		{
			this.magicalConstruction = magicalConstruction;
			Game1.player.forceCanMove();
			resetBounds();
			blueprints = new List<BluePrint>();
			if (magicalConstruction)
			{
				blueprints.Add(new BluePrint("Junimo Hut"));
				blueprints.Add(new BluePrint("Earth Obelisk"));
				blueprints.Add(new BluePrint("Water Obelisk"));
				blueprints.Add(new BluePrint("Desert Obelisk"));
				if (Game1.stats.getStat("boatRidesToIsland") >= 1)
				{
					blueprints.Add(new BluePrint("Island Obelisk"));
				}
				blueprints.Add(new BluePrint("Gold Clock"));
			}
			else
			{
				blueprints.Add(new BluePrint("Coop"));
				blueprints.Add(new BluePrint("Barn"));
				blueprints.Add(new BluePrint("Well"));
				blueprints.Add(new BluePrint("Silo"));
				blueprints.Add(new BluePrint("Mill"));
				blueprints.Add(new BluePrint("Shed"));
				blueprints.Add(new BluePrint("Fish Pond"));
				int numCabins = Game1.getFarm().getNumberBuildingsConstructed("Cabin");
				if (Game1.IsMasterGame && numCabins < Game1.CurrentPlayerLimit - 1)
				{
					blueprints.Add(new BluePrint("Stone Cabin"));
					blueprints.Add(new BluePrint("Plank Cabin"));
					blueprints.Add(new BluePrint("Log Cabin"));
				}
				if (Game1.getFarm().getNumberBuildingsConstructed("Stable") < numCabins + 1)
				{
					blueprints.Add(new BluePrint("Stable"));
				}
				blueprints.Add(new BluePrint("Slime Hutch"));
				if (Game1.getFarm().isBuildingConstructed("Coop"))
				{
					blueprints.Add(new BluePrint("Big Coop"));
				}
				if (Game1.getFarm().isBuildingConstructed("Big Coop"))
				{
					blueprints.Add(new BluePrint("Deluxe Coop"));
				}
				if (Game1.getFarm().isBuildingConstructed("Barn"))
				{
					blueprints.Add(new BluePrint("Big Barn"));
				}
				if (Game1.getFarm().isBuildingConstructed("Big Barn"))
				{
					blueprints.Add(new BluePrint("Deluxe Barn"));
				}
				if (Game1.getFarm().isBuildingConstructed("Shed"))
				{
					blueprints.Add(new BluePrint("Big Shed"));
				}
				blueprints.Add(new BluePrint("Shipping Bin"));
			}
			setNewActiveBlueprint();
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
			currentlySnappedComponent = getComponentWithID(107);
			snapCursorToCurrentSnappedComponent();
		}

		private void resetBounds()
		{
			xPositionOnScreen = Game1.uiViewport.Width / 2 - maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - maxHeightOfBuildingViewer / 2 - IClickableMenu.spaceToClearTopBorder + 32;
			width = maxWidthOfBuildingViewer + maxWidthOfDescription + IClickableMenu.spaceToClearSideBorder * 2 + 64;
			height = maxHeightOfBuildingViewer + IClickableMenu.spaceToClearTopBorder;
			initialize(xPositionOnScreen, yPositionOnScreen, width, height, showUpperRightCloseButton: true);
			okButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 192 - 12, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 373, 16, 16), 4f)
			{
				myID = 106,
				rightNeighborID = 104,
				leftNeighborID = 105
			};
			cancelButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 107,
				leftNeighborID = 104
			};
			backButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 48, 44), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 101,
				rightNeighborID = 102
			};
			forwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + maxWidthOfBuildingViewer - 256 + 16, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 48, 44), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				leftNeighborID = 101,
				rightNeighborID = -99998
			};
			demolishButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_Demolish"), new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128 - 8, yPositionOnScreen + maxHeightOfBuildingViewer + 64 - 4, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(348, 372, 17, 17), 4f)
			{
				myID = 104,
				rightNeighborID = 107,
				leftNeighborID = 106
			};
			upgradeIcon = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + maxWidthOfBuildingViewer - 128 + 32, yPositionOnScreen + 8, 36, 52), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(402, 328, 9, 13), 4f)
			{
				myID = 103,
				rightNeighborID = 104,
				leftNeighborID = 105
			};
			moveButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings"), new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 256 - 20, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(257, 284, 16, 16), 4f)
			{
				myID = 105,
				rightNeighborID = 106,
				leftNeighborID = -99998
			};
			paintButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_PaintBuildings"), new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 320 - 20, yPositionOnScreen + maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors2, new Microsoft.Xna.Framework.Rectangle(80, 208, 16, 16), 4f)
			{
				myID = 105,
				rightNeighborID = -99998,
				leftNeighborID = -99998
			};
			bool has_owned_buildings = false;
			bool has_paintable_buildings = CanPaintHouse() && HasPermissionsToPaint(null);
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.hasCarpenterPermissions())
				{
					has_owned_buildings = true;
				}
				if (building.CanBePainted() && HasPermissionsToPaint(building))
				{
					has_paintable_buildings = true;
				}
			}
			demolishButton.visible = Game1.IsMasterGame;
			moveButton.visible = (Game1.IsMasterGame || Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.On || (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.OwnedBuildings && has_owned_buildings));
			paintButton.visible = has_paintable_buildings;
			if (magicalConstruction)
			{
				paintButton.visible = false;
			}
			if (!demolishButton.visible)
			{
				upgradeIcon.rightNeighborID = demolishButton.rightNeighborID;
				okButton.rightNeighborID = demolishButton.rightNeighborID;
				cancelButton.leftNeighborID = demolishButton.leftNeighborID;
			}
			if (!moveButton.visible)
			{
				upgradeIcon.leftNeighborID = moveButton.leftNeighborID;
				forwardButton.rightNeighborID = -99998;
				okButton.leftNeighborID = moveButton.leftNeighborID;
			}
		}

		public void setNewActiveBlueprint()
		{
			if (blueprints[currentBlueprintIndex].name.Contains("Coop"))
			{
				currentBuilding = new Coop(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Barn"))
			{
				currentBuilding = new Barn(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Mill"))
			{
				currentBuilding = new Mill(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Junimo Hut"))
			{
				currentBuilding = new JunimoHut(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Shipping Bin"))
			{
				currentBuilding = new ShippingBin(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Fish Pond"))
			{
				currentBuilding = new FishPond(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Greenhouse"))
			{
				currentBuilding = new GreenhouseBuilding(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else
			{
				currentBuilding = new Building(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			price = blueprints[currentBlueprintIndex].moneyRequired;
			ingredients.Clear();
			foreach (KeyValuePair<int, int> v in blueprints[currentBlueprintIndex].itemsRequired)
			{
				ingredients.Add(new Object(v.Key, v.Value));
			}
			buildingDescription = blueprints[currentBlueprintIndex].description;
			buildingName = blueprints[currentBlueprintIndex].displayName;
		}

		public override void performHoverAction(int x, int y)
		{
			cancelButton.tryHover(x, y);
			base.performHoverAction(x, y);
			if (!onFarm)
			{
				backButton.tryHover(x, y, 1f);
				forwardButton.tryHover(x, y, 1f);
				okButton.tryHover(x, y);
				demolishButton.tryHover(x, y);
				moveButton.tryHover(x, y);
				paintButton.tryHover(x, y);
				if (CurrentBlueprint.isUpgrade() && upgradeIcon.containsPoint(x, y))
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Upgrade", new BluePrint(CurrentBlueprint.nameOfBuildingToUpgrade).displayName);
				}
				else if (demolishButton.containsPoint(x, y) && CanDemolishThis(CurrentBlueprint))
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Demolish");
				}
				else if (moveButton.containsPoint(x, y))
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings");
				}
				else if (okButton.containsPoint(x, y) && CurrentBlueprint.doesFarmerHaveEnoughResourcesToBuild())
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Build");
				}
				else if (paintButton.containsPoint(x, y))
				{
					hoverText = paintButton.name;
				}
				else
				{
					hoverText = "";
				}
			}
			else
			{
				if ((!upgrading && !demolishing && !moving && !painting) || freeze)
				{
					return;
				}
				Farm farm = Game1.getFarm();
				Vector2 tile_pos = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
				if (painting && farm.GetHouseRect().Contains(Utility.Vector2ToPoint(tile_pos)) && HasPermissionsToPaint(null) && CanPaintHouse())
				{
					farm.frameHouseColor = Color.Lime;
				}
				foreach (Building building in ((Farm)Game1.getLocationFromName("Farm")).buildings)
				{
					building.color.Value = Color.White;
				}
				Building b = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(tile_pos);
				if (b == null)
				{
					b = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false) + 128) / 64));
					if (b == null)
					{
						b = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false) + 192) / 64));
					}
				}
				if (upgrading)
				{
					if (b != null && CurrentBlueprint.nameOfBuildingToUpgrade != null && CurrentBlueprint.nameOfBuildingToUpgrade.Equals(b.buildingType))
					{
						b.color.Value = Color.Lime * 0.8f;
					}
					else if (b != null)
					{
						b.color.Value = Color.Red * 0.8f;
					}
				}
				else if (demolishing)
				{
					if (b != null && hasPermissionsToDemolish(b) && CanDemolishThis(b))
					{
						b.color.Value = Color.Red * 0.8f;
					}
				}
				else if (moving)
				{
					if (b != null && hasPermissionsToMove(b))
					{
						b.color.Value = Color.Lime * 0.8f;
					}
				}
				else if (painting && b != null && b.CanBePainted() && HasPermissionsToPaint(b))
				{
					b.color.Value = Color.Lime * 0.8f;
				}
			}
		}

		public bool hasPermissionsToDemolish(Building b)
		{
			if (Game1.IsMasterGame)
			{
				return CanDemolishThis(b);
			}
			return false;
		}

		public bool CanPaintHouse()
		{
			return Game1.MasterPlayer.HouseUpgradeLevel >= 2;
		}

		public bool HasPermissionsToPaint(Building b)
		{
			if (b == null)
			{
				if (Game1.player.UniqueMultiplayerID == Game1.MasterPlayer.UniqueMultiplayerID)
				{
					return true;
				}
				if (Game1.player.spouse == Game1.MasterPlayer.UniqueMultiplayerID.ToString())
				{
					return true;
				}
				return false;
			}
			if (b.isCabin && b.indoors.Value is Cabin)
			{
				Farmer cabin_owner = (b.indoors.Value as Cabin).owner;
				if (Game1.player.UniqueMultiplayerID == cabin_owner.UniqueMultiplayerID)
				{
					return true;
				}
				if (Game1.player.spouse == cabin_owner.UniqueMultiplayerID.ToString())
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public bool hasPermissionsToMove(Building b)
		{
			if (!Game1.getFarm().greenhouseUnlocked.Value && b is GreenhouseBuilding)
			{
				return false;
			}
			if (Game1.IsMasterGame)
			{
				return true;
			}
			if (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.On)
			{
				return true;
			}
			if (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.OwnedBuildings && b.hasCarpenterPermissions())
			{
				return true;
			}
			return false;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (!onFarm && b == Buttons.LeftTrigger)
			{
				currentBlueprintIndex--;
				if (currentBlueprintIndex < 0)
				{
					currentBlueprintIndex = blueprints.Count - 1;
				}
				setNewActiveBlueprint();
				Game1.playSound("shwip");
			}
			if (!onFarm && b == Buttons.RightTrigger)
			{
				currentBlueprintIndex = (currentBlueprintIndex + 1) % blueprints.Count;
				setNewActiveBlueprint();
				Game1.playSound("shwip");
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (freeze)
			{
				return;
			}
			if (!onFarm)
			{
				base.receiveKeyPress(key);
			}
			if (Game1.IsFading() || !onFarm)
			{
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose() && Game1.locationRequest == null)
			{
				returnToCarpentryMenu();
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

		public override void update(GameTime time)
		{
			base.update(time);
			if (onFarm && !Game1.IsFading())
			{
				int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
				int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
				if (mouseX - Game1.viewport.X < 64)
				{
					Game1.panScreen(-8, 0);
				}
				else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -128)
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
				if (!Game1.IsMultiplayer)
				{
					Farm farm = Game1.getFarm();
					foreach (FarmAnimal value in farm.animals.Values)
					{
						value.MovePosition(Game1.currentGameTime, Game1.viewport, farm);
					}
				}
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (freeze)
			{
				return;
			}
			if (!onFarm)
			{
				base.receiveLeftClick(x, y, playSound);
			}
			if (cancelButton.containsPoint(x, y))
			{
				if (onFarm)
				{
					if (moving && buildingToMove != null)
					{
						Game1.playSound("cancel");
						return;
					}
					returnToCarpentryMenu();
					Game1.playSound("smallSelect");
					return;
				}
				exitThisMenu();
				Game1.player.forceCanMove();
				Game1.playSound("bigDeSelect");
			}
			if (!onFarm && backButton.containsPoint(x, y))
			{
				currentBlueprintIndex--;
				if (currentBlueprintIndex < 0)
				{
					currentBlueprintIndex = blueprints.Count - 1;
				}
				setNewActiveBlueprint();
				Game1.playSound("shwip");
				backButton.scale = backButton.baseScale;
			}
			if (!onFarm && forwardButton.containsPoint(x, y))
			{
				currentBlueprintIndex = (currentBlueprintIndex + 1) % blueprints.Count;
				setNewActiveBlueprint();
				backButton.scale = backButton.baseScale;
				Game1.playSound("shwip");
			}
			if (!onFarm && demolishButton.containsPoint(x, y) && demolishButton.visible && CanDemolishThis(blueprints[currentBlueprintIndex]))
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
				demolishing = true;
			}
			if (!onFarm && moveButton.containsPoint(x, y) && moveButton.visible)
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
				moving = true;
			}
			if (!onFarm && paintButton.containsPoint(x, y) && paintButton.visible)
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
				painting = true;
			}
			if (okButton.containsPoint(x, y) && !onFarm && price >= 0 && Game1.player.Money >= price && blueprints[currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild())
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
			}
			if (!onFarm || freeze || Game1.IsFading())
			{
				return;
			}
			if (demolishing)
			{
				Farm farm = Game1.getLocationFromName("Farm") as Farm;
				Building destroyed = farm.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64));
				Action buildingLockFailed = delegate
				{
					if (demolishing)
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), Color.Red, 3500f));
					}
				};
				Action continueDemolish = delegate
				{
					if (demolishing && destroyed != null && farm.buildings.Contains(destroyed))
					{
						if ((int)destroyed.daysOfConstructionLeft > 0 || (int)destroyed.daysUntilUpgrade > 0)
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_DuringConstruction"), Color.Red, 3500f));
						}
						else if (destroyed.indoors.Value != null && destroyed.indoors.Value is AnimalHouse && (destroyed.indoors.Value as AnimalHouse).animalsThatLiveHere.Count > 0)
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_AnimalsHere"), Color.Red, 3500f));
						}
						else if (destroyed.indoors.Value != null && destroyed.indoors.Value.farmers.Any())
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), Color.Red, 3500f));
						}
						else
						{
							if (destroyed.indoors.Value != null && destroyed.indoors.Value is Cabin)
							{
								foreach (Farmer allFarmer in Game1.getAllFarmers())
								{
									if (allFarmer.currentLocation.Name == (destroyed.indoors.Value as Cabin).GetCellarName())
									{
										Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), Color.Red, 3500f));
										return;
									}
								}
							}
							if (destroyed.indoors.Value is Cabin && (destroyed.indoors.Value as Cabin).farmhand.Value.isActive())
							{
								Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_FarmhandOnline"), Color.Red, 3500f));
							}
							else
							{
								destroyed.BeforeDemolish();
								Chest chest = null;
								if (destroyed.indoors.Value is Cabin)
								{
									List<Item> list = (destroyed.indoors.Value as Cabin).demolish();
									if (list.Count > 0)
									{
										chest = new Chest(playerChest: true);
										chest.fixLidFrame();
										chest.items.Set(list);
									}
								}
								if (farm.destroyStructure(destroyed))
								{
									_ = (int)destroyed.tileY;
									_ = (int)destroyed.tilesHigh;
									Game1.flashAlpha = 1f;
									destroyed.showDestroyedAnimation(Game1.getFarm());
									Game1.playSound("explosion");
									Utility.spreadAnimalsAround(destroyed, farm);
									DelayedAction.functionAfterDelay(returnToCarpentryMenu, 1500);
									freeze = true;
									if (chest != null)
									{
										farm.objects[new Vector2((int)destroyed.tileX + (int)destroyed.tilesWide / 2, (int)destroyed.tileY + (int)destroyed.tilesHigh / 2)] = chest;
									}
								}
							}
						}
					}
				};
				if (destroyed != null)
				{
					if (destroyed.indoors.Value != null && destroyed.indoors.Value is Cabin && !Game1.IsMasterGame)
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), Color.Red, 3500f));
						destroyed = null;
						return;
					}
					if (!CanDemolishThis(destroyed))
					{
						destroyed = null;
						return;
					}
					if (!Game1.IsMasterGame && !hasPermissionsToDemolish(destroyed))
					{
						destroyed = null;
						return;
					}
				}
				if (destroyed != null && destroyed.indoors.Value is Cabin)
				{
					Cabin cabin = destroyed.indoors.Value as Cabin;
					if (cabin.farmhand.Value != null && (bool)cabin.farmhand.Value.isCustomized)
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:Carpenter_DemolishCabinConfirm", cabin.farmhand.Value.Name), Game1.currentLocation.createYesNoResponses(), delegate(Farmer f, string answer)
						{
							if (answer == "Yes")
							{
								Game1.activeClickableMenu = this;
								Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
							}
							else
							{
								DelayedAction.functionAfterDelay(returnToCarpentryMenu, 500);
							}
						});
						return;
					}
				}
				if (destroyed != null)
				{
					Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
				}
			}
			else if (upgrading)
			{
				Building toUpgrade = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64));
				if (toUpgrade != null && CurrentBlueprint.name != null && toUpgrade.buildingType.Equals(CurrentBlueprint.nameOfBuildingToUpgrade))
				{
					CurrentBlueprint.consumeResources();
					toUpgrade.daysUntilUpgrade.Value = 2;
					toUpgrade.showUpgradeAnimation(Game1.getFarm());
					Game1.playSound("axe");
					DelayedAction.functionAfterDelay(returnToCarpentryMenuAfterSuccessfulBuild, 1500);
					freeze = true;
					Game1.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, Utility.AOrAn(CurrentBlueprint.displayName), CurrentBlueprint.displayName, Game1.player.farmName);
				}
				else if (toUpgrade != null)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantUpgrade_BuildingType"), Color.Red, 3500f));
				}
			}
			else if (painting)
			{
				Farm farm_location = Game1.getFarm();
				Vector2 tile_position = new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64);
				Building paint_building = farm_location.getBuildingAt(tile_position);
				if (paint_building != null)
				{
					if (!paint_building.CanBePainted())
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint"), Color.Red, 3500f));
						return;
					}
					if (!HasPermissionsToPaint(paint_building))
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint_Permission"), Color.Red, 3500f));
						return;
					}
					paint_building.color.Value = Color.White;
					SetChildMenu(new BuildingPaintMenu(paint_building));
				}
				else if (farm_location.GetHouseRect().Contains(Utility.Vector2ToPoint(tile_position)))
				{
					if (!CanPaintHouse())
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint"), Color.Red, 3500f));
					}
					else if (!HasPermissionsToPaint(null))
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint_Permission"), Color.Red, 3500f));
					}
					else
					{
						SetChildMenu(new BuildingPaintMenu("House", () => (farm_location.paintedHouseTexture != null) ? farm_location.paintedHouseTexture : Farm.houseTextures, farm_location.houseSource.Value, farm_location.housePaintColor.Value));
					}
				}
			}
			else if (moving)
			{
				if (buildingToMove == null)
				{
					buildingToMove = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64));
					if (buildingToMove != null)
					{
						if ((int)buildingToMove.daysOfConstructionLeft > 0)
						{
							buildingToMove = null;
							return;
						}
						if (!hasPermissionsToMove(buildingToMove))
						{
							buildingToMove = null;
							return;
						}
						buildingToMove.isMoving = true;
						Game1.playSound("axchop");
					}
				}
				else if (((Farm)Game1.getLocationFromName("Farm")).buildStructure(buildingToMove, new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64), Game1.player))
				{
					buildingToMove.isMoving = false;
					if (buildingToMove is ShippingBin)
					{
						(buildingToMove as ShippingBin).initLid();
					}
					if (buildingToMove is GreenhouseBuilding)
					{
						Game1.getFarm().greenhouseMoved.Value = true;
					}
					buildingToMove.performActionOnBuildingPlacement();
					buildingToMove = null;
					Game1.playSound("axchop");
					DelayedAction.playSoundAfterDelay("dirtyHit", 50);
					DelayedAction.playSoundAfterDelay("dirtyHit", 150);
				}
				else
				{
					Game1.playSound("cancel");
				}
			}
			else
			{
				Game1.player.team.buildLock.RequestLock(delegate
				{
					if (onFarm && Game1.locationRequest == null)
					{
						if (tryToBuild())
						{
							CurrentBlueprint.consumeResources();
							DelayedAction.functionAfterDelay(returnToCarpentryMenuAfterSuccessfulBuild, 2000);
							freeze = true;
						}
						else
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), Color.Red, 3500f));
						}
					}
					Game1.player.team.buildLock.ReleaseLock();
				});
			}
		}

		public bool tryToBuild()
		{
			return ((Farm)Game1.getLocationFromName("Farm")).buildStructure(CurrentBlueprint, new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64), Game1.player, magicalConstruction);
		}

		public void returnToCarpentryMenu()
		{
			LocationRequest locationRequest = Game1.getLocationRequest(magicalConstruction ? "WizardHouse" : "ScienceHouse");
			locationRequest.OnWarp += delegate
			{
				onFarm = false;
				Game1.player.viewingLocation.Value = null;
				resetBounds();
				upgrading = false;
				moving = false;
				painting = false;
				buildingToMove = null;
				freeze = false;
				Game1.displayHUD = true;
				Game1.viewportFreeze = false;
				Game1.viewport.Location = new Location(320, 1536);
				drawBG = true;
				demolishing = false;
				Game1.displayFarmer = true;
				if (Game1.options.SnappyMenus)
				{
					populateClickableComponentList();
					snapToDefaultClickableComponent();
				}
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
		}

		public void returnToCarpentryMenuAfterSuccessfulBuild()
		{
			LocationRequest locationRequest = Game1.getLocationRequest(magicalConstruction ? "WizardHouse" : "ScienceHouse");
			locationRequest.OnWarp += delegate
			{
				Game1.displayHUD = true;
				Game1.player.viewingLocation.Value = null;
				Game1.viewportFreeze = false;
				Game1.viewport.Location = new Location(320, 1536);
				freeze = true;
				Game1.displayFarmer = true;
				robinConstructionMessage();
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
		}

		public void robinConstructionMessage()
		{
			exitThisMenu();
			Game1.player.forceCanMove();
			if (!magicalConstruction)
			{
				string dialoguePath = "Data\\ExtraDialogue:Robin_" + (upgrading ? "Upgrade" : "New") + "Construction";
				if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
				{
					dialoguePath += "_Festival";
				}
				if (CurrentBlueprint.daysToConstruct <= 0)
				{
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"), Game1.content.LoadString("Data\\ExtraDialogue:Robin_Instant", (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? CurrentBlueprint.displayName : CurrentBlueprint.displayName.ToLower()));
				}
				else
				{
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"), Game1.content.LoadString(dialoguePath, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? CurrentBlueprint.displayName : CurrentBlueprint.displayName.ToLower(), (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? CurrentBlueprint.displayName.Split(' ').Last().Split('-')
						.Last() : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? CurrentBlueprint.displayName.ToLower().Split(' ').First() : CurrentBlueprint.displayName.ToLower().Split(' ').Last())));
				}
			}
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return onFarm;
		}

		public void setUpForBuildingPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			hoverText = "";
			Game1.currentLocation = Game1.getLocationFromName("Farm");
			Game1.player.viewingLocation.Value = "Farm";
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear();
			onFarm = true;
			cancelButton.bounds.X = Game1.uiViewport.Width - 128;
			cancelButton.bounds.Y = Game1.uiViewport.Height - 128;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.viewport.Location = new Location(3136, 320);
			Game1.panScreen(0, 0);
			drawBG = false;
			freeze = false;
			Game1.displayFarmer = false;
			if (!demolishing && CurrentBlueprint.nameOfBuildingToUpgrade != null && CurrentBlueprint.nameOfBuildingToUpgrade.Length > 0 && !moving && !painting)
			{
				upgrading = true;
			}
		}

		public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			resetBounds();
		}

		public virtual bool CanDemolishThis(Building building)
		{
			if (building == null)
			{
				return false;
			}
			if (_demolishCheckBlueprint == null || _demolishCheckBlueprint.name != building.buildingType.Value)
			{
				_demolishCheckBlueprint = new BluePrint(building.buildingType);
			}
			if (_demolishCheckBlueprint != null)
			{
				return CanDemolishThis(_demolishCheckBlueprint);
			}
			return true;
		}

		public virtual bool CanDemolishThis(BluePrint blueprint)
		{
			if (blueprint.moneyRequired < 0)
			{
				return false;
			}
			if (blueprint.name == "Shipping Bin")
			{
				int bins = 0;
				foreach (Building building in Game1.getFarm().buildings)
				{
					if (building is ShippingBin)
					{
						bins++;
					}
					if (bins > 1)
					{
						break;
					}
				}
				if (bins <= 1)
				{
					return false;
				}
			}
			return true;
		}

		public override void draw(SpriteBatch b)
		{
			if (drawBG)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
			}
			if (Game1.IsFading() || freeze)
			{
				return;
			}
			if (!onFarm)
			{
				base.draw(b);
				IClickableMenu.drawTextureBox(b, xPositionOnScreen - 96, yPositionOnScreen - 16, maxWidthOfBuildingViewer + 64, maxHeightOfBuildingViewer + 64, magicalConstruction ? Color.RoyalBlue : Color.White);
				currentBuilding.drawInMenu(b, xPositionOnScreen + maxWidthOfBuildingViewer / 2 - (int)currentBuilding.tilesWide * 64 / 2 - 64, yPositionOnScreen + maxHeightOfBuildingViewer / 2 - currentBuilding.getSourceRectForMenu().Height * 4 / 2);
				if (CurrentBlueprint.isUpgrade())
				{
					upgradeIcon.draw(b);
				}
				string placeholder = " Deluxe  Barn   ";
				if (SpriteText.getWidthOfString(buildingName) >= SpriteText.getWidthOfString(placeholder))
				{
					placeholder = buildingName + " ";
				}
				SpriteText.drawStringWithScrollCenteredAt(b, buildingName, xPositionOnScreen + maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder - 16 + 64 + (width - (maxWidthOfBuildingViewer + 128)) / 2, yPositionOnScreen, SpriteText.getWidthOfString(placeholder));
				int descriptionWidth;
				switch (LocalizedContentManager.CurrentLanguageCode)
				{
				case LocalizedContentManager.LanguageCode.es:
					descriptionWidth = maxWidthOfDescription + 64 + ((CurrentBlueprint?.name == "Deluxe Barn") ? 96 : 0);
					break;
				case LocalizedContentManager.LanguageCode.it:
					descriptionWidth = maxWidthOfDescription + 96;
					break;
				case LocalizedContentManager.LanguageCode.fr:
					descriptionWidth = maxWidthOfDescription + 96 + ((CurrentBlueprint?.name == "Slime Hutch" || CurrentBlueprint?.name == "Deluxe Coop" || CurrentBlueprint?.name == "Deluxe Barn") ? 72 : 0);
					break;
				case LocalizedContentManager.LanguageCode.ko:
					descriptionWidth = maxWidthOfDescription + 96 + ((CurrentBlueprint?.name == "Slime Hutch") ? 64 : ((CurrentBlueprint?.name == "Deluxe Coop") ? 96 : ((CurrentBlueprint?.name == "Deluxe Barn") ? 112 : ((CurrentBlueprint?.name == "Big Barn") ? 64 : 0))));
					break;
				default:
					descriptionWidth = maxWidthOfDescription + 64;
					break;
				}
				IClickableMenu.drawTextureBox(b, xPositionOnScreen + maxWidthOfBuildingViewer - 16, yPositionOnScreen + 80, descriptionWidth, maxHeightOfBuildingViewer - 32, magicalConstruction ? Color.RoyalBlue : Color.White);
				if (magicalConstruction)
				{
					Utility.drawTextWithShadow(b, Game1.parseText(buildingDescription, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer - 4, yPositionOnScreen + 80 + 16 + 4), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f);
					Utility.drawTextWithShadow(b, Game1.parseText(buildingDescription, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer - 1, yPositionOnScreen + 80 + 16 + 4), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f);
				}
				Utility.drawTextWithShadow(b, Game1.parseText(buildingDescription, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer, yPositionOnScreen + 80 + 16), magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.75f);
				Vector2 ingredientsPosition = new Vector2(xPositionOnScreen + maxWidthOfBuildingViewer + 16, yPositionOnScreen + 256 + 32);
				if (ingredients.Count < 3 && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt))
				{
					ingredientsPosition.Y += 64f;
				}
				if (price >= 0)
				{
					SpriteText.drawString(b, "$", (int)ingredientsPosition.X, (int)ingredientsPosition.Y);
					if (magicalConstruction)
					{
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f, ingredientsPosition.Y + 8f), Game1.textColor * 0.5f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f - 1f, ingredientsPosition.Y + 8f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
					}
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f, ingredientsPosition.Y + 4f), (Game1.player.Money < price) ? Color.Red : (magicalConstruction ? Color.PaleGoldenrod : Game1.textColor), 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
				}
				ingredientsPosition.X -= 16f;
				ingredientsPosition.Y -= 21f;
				foreach (Item i in ingredients)
				{
					ingredientsPosition.Y += 68f;
					i.drawInMenu(b, ingredientsPosition, 1f);
					bool hasItem = (!(i is Object) || Game1.player.hasItemInInventory((i as Object).parentSheetIndex, i.Stack)) ? true : false;
					if (magicalConstruction)
					{
						Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 12f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
						Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f - 1f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
					}
					Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f, ingredientsPosition.Y + 20f), hasItem ? (magicalConstruction ? Color.PaleGoldenrod : Game1.textColor) : Color.Red, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
				}
				backButton.draw(b);
				forwardButton.draw(b);
				okButton.draw(b, blueprints[currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild() ? Color.White : (Color.Gray * 0.8f), 0.88f);
				demolishButton.draw(b, CanDemolishThis(blueprints[currentBlueprintIndex]) ? Color.White : (Color.Gray * 0.8f), 0.88f);
				moveButton.draw(b);
				paintButton.draw(b);
			}
			else
			{
				string message2 = "";
				message2 = (upgrading ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Upgrade", new BluePrint(CurrentBlueprint.nameOfBuildingToUpgrade).displayName) : (demolishing ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Demolish") : ((!painting) ? Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation") : Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Paint"))));
				SpriteText.drawStringWithScrollBackground(b, message2, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(message2) / 2, 16);
				Game1.StartWorldDrawInUI(b);
				if (!upgrading && !demolishing && !moving && !painting)
				{
					Vector2 mousePositionTile2 = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
					for (int y4 = 0; y4 < CurrentBlueprint.tilesHeight; y4++)
					{
						for (int x3 = 0; x3 < CurrentBlueprint.tilesWidth; x3++)
						{
							int sheetIndex3 = CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x3, y4);
							Vector2 currentGlobalTilePosition3 = new Vector2(mousePositionTile2.X + (float)x3, mousePositionTile2.Y + (float)y4);
							if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(currentGlobalTilePosition3))
							{
								sheetIndex3++;
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition3 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex3 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
						}
					}
					foreach (Point additionalPlacementTile in CurrentBlueprint.additionalPlacementTiles)
					{
						int x4 = additionalPlacementTile.X;
						int y3 = additionalPlacementTile.Y;
						int sheetIndex4 = CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x4, y3);
						Vector2 currentGlobalTilePosition4 = new Vector2(mousePositionTile2.X + (float)x4, mousePositionTile2.Y + (float)y3);
						if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(currentGlobalTilePosition4))
						{
							sheetIndex4++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition4 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex4 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
				}
				else if (!painting && moving && buildingToMove != null)
				{
					Vector2 mousePositionTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
					BuildableGameLocation bl = Game1.currentLocation as BuildableGameLocation;
					for (int y2 = 0; y2 < (int)buildingToMove.tilesHigh; y2++)
					{
						for (int x = 0; x < (int)buildingToMove.tilesWide; x++)
						{
							int sheetIndex = buildingToMove.getTileSheetIndexForStructurePlacementTile(x, y2);
							Vector2 currentGlobalTilePosition = new Vector2(mousePositionTile.X + (float)x, mousePositionTile.Y + (float)y2);
							bool occupiedByBuilding = bl.buildings.Contains(buildingToMove) && buildingToMove.occupiesTile(currentGlobalTilePosition);
							if (!bl.isBuildable(currentGlobalTilePosition) && !occupiedByBuilding)
							{
								sheetIndex++;
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
						}
					}
					foreach (Point additionalPlacementTile2 in buildingToMove.additionalPlacementTiles)
					{
						int x2 = additionalPlacementTile2.X;
						int y = additionalPlacementTile2.Y;
						int sheetIndex2 = buildingToMove.getTileSheetIndexForStructurePlacementTile(x2, y);
						Vector2 currentGlobalTilePosition2 = new Vector2(mousePositionTile.X + (float)x2, mousePositionTile.Y + (float)y);
						bool occupiedByBuilding2 = bl.buildings.Contains(buildingToMove) && buildingToMove.occupiesTile(currentGlobalTilePosition2);
						if (!bl.isBuildable(currentGlobalTilePosition2) && !occupiedByBuilding2)
						{
							sheetIndex2++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition2 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex2 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
				}
				Game1.EndWorldDrawInUI(b);
			}
			cancelButton.draw(b);
			drawMouse(b);
			if (hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
