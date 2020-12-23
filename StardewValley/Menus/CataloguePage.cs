using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class CataloguePage : IClickableMenu
	{
		public static int widthToMoveActiveTab = 8;

		public static int blueprintButtonMargin = 32;

		public const int buildingsTab = 0;

		public const int upgradesTab = 1;

		public const int animalsTab = 2;

		public const int demolishTab = 3;

		public const int numberOfTabs = 4;

		private string descriptionText = "";

		private string hoverText = "";

		private InventoryMenu inventory;

		private Item heldItem;

		private int currentTab;

		private BluePrint hoveredItem;

		private List<ClickableTextureComponent> sideTabs = new List<ClickableTextureComponent>();

		private List<Dictionary<ClickableComponent, BluePrint>> blueprintButtons = new List<Dictionary<ClickableComponent, BluePrint>>();

		private bool demolishing;

		private bool upgrading;

		private bool placingStructure;

		private BluePrint structureForPlacement;

		private GameMenu parent;

		private Texture2D buildingPlacementTiles;

		public CataloguePage(int x, int y, int width, int height, GameMenu parent)
			: base(x, y, width, height)
		{
			this.parent = parent;
			buildingPlacementTiles = Game1.content.Load<Texture2D>("LooseSprites\\buildingPlacementTiles");
			widthToMoveActiveTab = 8;
			blueprintButtonMargin = 32;
			inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 - 16, playerInventory: false);
			sideTabs.Add(new ClickableTextureComponent("", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 48 + widthToMoveActiveTab, yPositionOnScreen + 128, 64, 64), "", "Buildings", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 4), 1f));
			sideTabs.Add(new ClickableTextureComponent("", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 192, 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10138"), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 5), 1f));
			sideTabs.Add(new ClickableTextureComponent("", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 256, 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10139"), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 8), 1f));
			sideTabs.Add(new ClickableTextureComponent("", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 320, 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10140"), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 6), 1f));
			for (int j = 0; j < 4; j++)
			{
				blueprintButtons.Add(new Dictionary<ClickableComponent, BluePrint>());
			}
			int widthOfBlueprintSpace = 512;
			int[] rowWidthTally = new int[4];
			for (int i = 0; i < Game1.player.blueprints.Count; i++)
			{
				BluePrint print = new BluePrint(Game1.player.blueprints[i]);
				if (canPlaceThisBuildingOnTheCurrentMap(print, Game1.currentLocation))
				{
					print.canBuildOnCurrentMap = true;
				}
				int tabNumber = getTabNumberFromName(print.blueprintType);
				if (print.blueprintType != null)
				{
					int printWidth = (int)((float)Math.Max(print.tilesWidth, 4) / 4f * 64f) + blueprintButtonMargin;
					if (rowWidthTally[tabNumber] % (widthOfBlueprintSpace - IClickableMenu.borderWidth * 2) + printWidth > widthOfBlueprintSpace - IClickableMenu.borderWidth * 2)
					{
						rowWidthTally[tabNumber] += widthOfBlueprintSpace - IClickableMenu.borderWidth * 2 - rowWidthTally[tabNumber] % (widthOfBlueprintSpace - IClickableMenu.borderWidth * 2);
					}
					blueprintButtons[Math.Min(3, tabNumber)].Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + rowWidthTally[tabNumber] % (widthOfBlueprintSpace - IClickableMenu.borderWidth * 2), y + IClickableMenu.borderWidth + rowWidthTally[tabNumber] / (widthOfBlueprintSpace - IClickableMenu.borderWidth * 2) * 64 * 2 + 64, printWidth, 128), print.name), print);
					rowWidthTally[tabNumber] += printWidth;
				}
			}
		}

		public int getTabNumberFromName(string name)
		{
			int whichTab = -1;
			switch (name)
			{
			case "Buildings":
				whichTab = 0;
				break;
			case "Upgrades":
				whichTab = 1;
				break;
			case "Demolish":
				whichTab = 3;
				break;
			case "Animals":
				whichTab = 2;
				break;
			}
			return whichTab;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!placingStructure)
			{
				heldItem = inventory.leftClick(x, y, heldItem);
				for (int i = 0; i < sideTabs.Count; i++)
				{
					if (sideTabs[i].containsPoint(x, y) && currentTab != i)
					{
						Game1.playSound("smallSelect");
						if (i == 3)
						{
							placingStructure = true;
							demolishing = true;
							parent.invisible = true;
						}
						else
						{
							sideTabs[currentTab].bounds.X -= widthToMoveActiveTab;
							currentTab = i;
							sideTabs[i].bounds.X += widthToMoveActiveTab;
						}
					}
				}
				foreach (ClickableComponent c in blueprintButtons[currentTab].Keys)
				{
					if (c.containsPoint(x, y))
					{
						if (blueprintButtons[currentTab][c].doesFarmerHaveEnoughResourcesToBuild())
						{
							structureForPlacement = blueprintButtons[currentTab][c];
							placingStructure = true;
							parent.invisible = true;
							if (currentTab == 1)
							{
								upgrading = true;
							}
							Game1.playSound("smallSelect");
						}
						else
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10002"), Color.Red, 3500f));
						}
						break;
					}
				}
			}
			else if (demolishing)
			{
				if (!(Game1.currentLocation is Farm))
				{
					return;
				}
				if (Game1.IsClient)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10148"), Color.Red, 3500f));
					return;
				}
				Vector2 tileLocation = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
				Building destroyed = ((Farm)Game1.currentLocation).getBuildingAt(tileLocation);
				if (Game1.IsMultiplayer && destroyed != null && destroyed.indoors.Value.farmers.Any())
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10149"), Color.Red, 3500f));
				}
				else if (destroyed != null && ((Farm)Game1.currentLocation).destroyStructure(destroyed))
				{
					int groundYTile = (int)destroyed.tileY + (int)destroyed.tilesHigh;
					for (int j = 0; j < destroyed.texture.Value.Bounds.Height / 64; j++)
					{
						Game1.createRadialDebris(Game1.currentLocation, destroyed.textureName(), new Microsoft.Xna.Framework.Rectangle(destroyed.texture.Value.Bounds.Center.X, destroyed.texture.Value.Bounds.Center.Y, 4, 4), (int)destroyed.tileX + Game1.random.Next(destroyed.tilesWide), (int)destroyed.tileY + (int)destroyed.tilesHigh - j, Game1.random.Next(20, 45), groundYTile);
					}
					Game1.playSound("explosion");
					Utility.spreadAnimalsAround(destroyed, (Farm)Game1.currentLocation);
				}
				else
				{
					parent.invisible = false;
					placingStructure = false;
					demolishing = false;
				}
			}
			else if (upgrading && Game1.currentLocation is Farm)
			{
				(Game1.currentLocation as Farm).tryToUpgrade(((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64)), structureForPlacement);
			}
			else if (!canPlaceThisBuildingOnTheCurrentMap(structureForPlacement, Game1.currentLocation))
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10152"), Color.Red, 3500f));
			}
			else if (!structureForPlacement.doesFarmerHaveEnoughResourcesToBuild())
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10002"), Color.Red, 3500f));
			}
			else if (tryToBuild())
			{
				structureForPlacement.consumeResources();
				if (!structureForPlacement.blueprintType.Equals("Animals"))
				{
					Game1.playSound("axe");
				}
			}
			else if (!Game1.IsClient)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10016"), Color.Red, 3500f));
			}
		}

		public static bool canPlaceThisBuildingOnTheCurrentMap(BluePrint structureToPlace, GameLocation map)
		{
			return true;
		}

		private bool tryToBuild()
		{
			if (structureForPlacement.blueprintType.Equals("Animals"))
			{
				return ((Farm)Game1.getLocationFromName("Farm")).placeAnimal(structureForPlacement, new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64), serverCommand: false, Game1.player.UniqueMultiplayerID);
			}
			return (Game1.currentLocation as BuildableGameLocation).buildStructure(structureForPlacement, new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64), Game1.player);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (placingStructure)
			{
				placingStructure = false;
				upgrading = false;
				demolishing = false;
				parent.invisible = false;
			}
			else
			{
				heldItem = inventory.rightClick(x, y, heldItem);
			}
		}

		public override bool readyToClose()
		{
			if (heldItem == null)
			{
				return !placingStructure;
			}
			return false;
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			hoverText = "";
			foreach (ClickableTextureComponent c2 in sideTabs)
			{
				if (c2.containsPoint(x, y))
				{
					hoverText = c2.hoverText;
					return;
				}
			}
			bool overAnyButton = false;
			foreach (ClickableComponent c in blueprintButtons[currentTab].Keys)
			{
				if (c.containsPoint(x, y))
				{
					c.scale = Math.Min(c.scale + 0.01f, 1.1f);
					hoveredItem = blueprintButtons[currentTab][c];
					overAnyButton = true;
				}
				else
				{
					c.scale = Math.Max(c.scale - 0.01f, 1f);
				}
			}
			if (demolishing)
			{
				foreach (Building building in ((Farm)Game1.getLocationFromName("Farm")).buildings)
				{
					building.color.Value = Color.White;
				}
				Building b2 = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				if (b2 != null)
				{
					b2.color.Value = Color.Red * 0.8f;
				}
			}
			else if (upgrading)
			{
				foreach (Building building2 in ((Farm)Game1.getLocationFromName("Farm")).buildings)
				{
					building2.color.Value = Color.White;
				}
				Building b = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				if (b != null && structureForPlacement.nameOfBuildingToUpgrade != null && structureForPlacement.nameOfBuildingToUpgrade.Equals(b.buildingType))
				{
					b.color.Value = Color.Green * 0.8f;
				}
				else if (b != null)
				{
					b.color.Value = Color.Red * 0.8f;
				}
			}
			if (!overAnyButton)
			{
				hoveredItem = null;
			}
		}

		private int getTileSheetIndexForStructurePlacementTile(int x, int y)
		{
			if (x == structureForPlacement.humanDoor.X && y == structureForPlacement.humanDoor.Y)
			{
				return 2;
			}
			if (x == structureForPlacement.animalDoor.X && y == structureForPlacement.animalDoor.Y)
			{
				return 4;
			}
			return 0;
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && placingStructure)
			{
				placingStructure = false;
				upgrading = false;
				demolishing = false;
				parent.invisible = false;
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!placingStructure)
			{
				foreach (ClickableTextureComponent sideTab in sideTabs)
				{
					sideTab.draw(b);
				}
				drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256);
				drawVerticalUpperIntersectingPartition(b, xPositionOnScreen + 576, 328);
				inventory.draw(b);
				foreach (ClickableComponent c in blueprintButtons[currentTab].Keys)
				{
					Texture2D structureTexture = blueprintButtons[currentTab][c].texture;
					b.Draw(origin: new Vector2(blueprintButtons[currentTab][c].sourceRectForMenuView.Center.X, blueprintButtons[currentTab][c].sourceRectForMenuView.Center.Y), texture: structureTexture, position: new Vector2(c.bounds.Center.X, c.bounds.Center.Y), sourceRectangle: blueprintButtons[currentTab][c].sourceRectForMenuView, color: blueprintButtons[currentTab][c].canBuildOnCurrentMap ? Color.White : (Color.Gray * 0.8f), rotation: 0f, scale: 1f * c.scale + ((currentTab == 2) ? 0.75f : 0f), effects: SpriteEffects.None, layerDepth: 0.9f);
				}
				if (hoveredItem != null)
				{
					hoveredItem.drawDescription(b, xPositionOnScreen + 576 + 42, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 32, 224);
				}
				if (heldItem != null)
				{
					heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
				}
				if (!hoverText.Equals(""))
				{
					IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
				}
			}
			else
			{
				if (demolishing || upgrading)
				{
					return;
				}
				Vector2 mousePositionTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
				for (int y = 0; y < structureForPlacement.tilesHeight; y++)
				{
					for (int x = 0; x < structureForPlacement.tilesWidth; x++)
					{
						int sheetIndex = getTileSheetIndexForStructurePlacementTile(x, y);
						Vector2 currentGlobalTilePosition = new Vector2(mousePositionTile.X + (float)x, mousePositionTile.Y + (float)y);
						if (Game1.player.getTileLocation().Equals(currentGlobalTilePosition) || Game1.currentLocation.isTileOccupied(currentGlobalTilePosition) || !Game1.currentLocation.isTilePassable(new Location((int)currentGlobalTilePosition.X, (int)currentGlobalTilePosition.Y), Game1.viewport))
						{
							sheetIndex++;
						}
						b.Draw(buildingPlacementTiles, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition * 64f), Game1.getSourceRectForStandardTileSheet(buildingPlacementTiles, sheetIndex), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.999f);
					}
				}
			}
		}
	}
}
