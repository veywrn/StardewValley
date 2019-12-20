using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class BlueprintsMenu : IClickableMenu
	{
		public static int heightOfDescriptionBox = 384;

		public static int blueprintButtonMargin = 32;

		public new static int tabYPositionRelativeToMenuY = -48;

		public const int buildingsTab = 0;

		public const int upgradesTab = 1;

		public const int decorationsTab = 2;

		public const int demolishTab = 3;

		public const int animalsTab = 4;

		public const int numberOfTabs = 5;

		private bool placingStructure;

		private bool demolishing;

		private bool upgrading;

		private bool queryingAnimals;

		private int currentTab;

		private Vector2 positionOfAnimalWhenClicked;

		private string hoverText = "";

		private List<Dictionary<ClickableComponent, BluePrint>> blueprintButtons = new List<Dictionary<ClickableComponent, BluePrint>>();

		private List<ClickableComponent> tabs = new List<ClickableComponent>();

		private BluePrint hoveredItem;

		private BluePrint structureForPlacement;

		private FarmAnimal currentAnimal;

		private Texture2D buildingPlacementTiles;

		public BlueprintsMenu(int x, int y)
			: base(x, y, Game1.viewport.Width / 2 + 96, 0)
		{
			tabYPositionRelativeToMenuY = -48;
			blueprintButtonMargin = 32;
			heightOfDescriptionBox = 384;
			for (int k = 0; k < 5; k++)
			{
				blueprintButtons.Add(new Dictionary<ClickableComponent, BluePrint>());
			}
			xPositionOnScreen = x;
			yPositionOnScreen = y;
			int[] rowWidthTally = new int[5];
			for (int j = 0; j < Game1.player.blueprints.Count; j++)
			{
				BluePrint print = new BluePrint(Game1.player.blueprints[j]);
				int tabNumber = getTabNumberFromName(print.blueprintType);
				if (print.blueprintType != null)
				{
					int printWidth = (int)((float)Math.Max(print.tilesWidth, 4) / 4f * 64f) + blueprintButtonMargin;
					if (rowWidthTally[tabNumber] % (width - IClickableMenu.borderWidth * 2) + printWidth > width - IClickableMenu.borderWidth * 2)
					{
						rowWidthTally[tabNumber] += width - IClickableMenu.borderWidth * 2 - rowWidthTally[tabNumber] % (width - IClickableMenu.borderWidth * 2);
					}
					blueprintButtons[Math.Min(4, tabNumber)].Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + rowWidthTally[tabNumber] % (width - IClickableMenu.borderWidth * 2), y + IClickableMenu.borderWidth + rowWidthTally[tabNumber] / (width - IClickableMenu.borderWidth * 2) * 64 * 2 + 64, printWidth, 128), print.name), print);
					rowWidthTally[tabNumber] += printWidth;
				}
			}
			blueprintButtons[4].Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + rowWidthTally[4] % (width - IClickableMenu.borderWidth * 2), y + IClickableMenu.borderWidth + rowWidthTally[4] / (width - IClickableMenu.borderWidth * 2) * 64 * 2 + 64, 64 + blueprintButtonMargin, 128), "Info Tool"), new BluePrint("Info Tool"));
			int tallestTab = 0;
			for (int i = 0; i < rowWidthTally.Length; i++)
			{
				if (rowWidthTally[i] > tallestTab)
				{
					tallestTab = rowWidthTally[i];
				}
			}
			height = 128 + tallestTab / (width - IClickableMenu.borderWidth * 2) * 64 * 2 + IClickableMenu.borderWidth * 4 + heightOfDescriptionBox;
			buildingPlacementTiles = Game1.content.Load<Texture2D>("LooseSprites\\buildingPlacementTiles");
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Buildings"));
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64 + 64 + 4, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Upgrades"));
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64 + 136, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Decorations"));
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64 + 204, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Demolish"));
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64 + 272, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Animals"));
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
			case "Decorations":
				whichTab = 2;
				break;
			case "Demolish":
				whichTab = 3;
				break;
			case "Animals":
				whichTab = 4;
				break;
			}
			return whichTab;
		}

		public void changePosition(int x, int y)
		{
			int translateX = xPositionOnScreen - x;
			int translateY = yPositionOnScreen - y;
			xPositionOnScreen = x;
			yPositionOnScreen = y;
			foreach (Dictionary<ClickableComponent, BluePrint> blueprintButton in blueprintButtons)
			{
				foreach (ClickableComponent key in blueprintButton.Keys)
				{
					key.bounds.X += translateX;
					key.bounds.Y -= translateY;
				}
			}
			foreach (ClickableComponent tab in tabs)
			{
				tab.bounds.X += translateX;
				tab.bounds.Y -= translateY;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (currentAnimal != null)
			{
				currentAnimal = null;
				placingStructure = true;
				queryingAnimals = true;
			}
			if (!placingStructure)
			{
				Microsoft.Xna.Framework.Rectangle menuBounds = new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen, yPositionOnScreen, width, height);
				foreach (ClickableComponent c in blueprintButtons[currentTab].Keys)
				{
					if (c.containsPoint(x, y))
					{
						if (c.name.Equals("Info Tool"))
						{
							placingStructure = true;
							queryingAnimals = true;
							Game1.playSound("smallSelect");
						}
						else if (blueprintButtons[currentTab][c].doesFarmerHaveEnoughResourcesToBuild())
						{
							structureForPlacement = blueprintButtons[currentTab][c];
							placingStructure = true;
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
						return;
					}
				}
				foreach (ClickableComponent c2 in tabs)
				{
					if (c2.containsPoint(x, y))
					{
						currentTab = getTabNumberFromName(c2.name);
						Game1.playSound("smallSelect");
						if (currentTab == 3)
						{
							placingStructure = true;
							demolishing = true;
						}
						return;
					}
				}
				if (!menuBounds.Contains(x, y))
				{
					Game1.exitActiveMenu();
				}
			}
			else if (demolishing)
			{
				Building destroyed = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				if (destroyed != null && ((Farm)Game1.getLocationFromName("Farm")).destroyStructure(destroyed))
				{
					int groundYTile = (int)destroyed.tileY + (int)destroyed.tilesHigh;
					for (int i = 0; i < destroyed.texture.Value.Bounds.Height / 64; i++)
					{
						Game1.createRadialDebris(Game1.currentLocation, destroyed.textureName(), new Microsoft.Xna.Framework.Rectangle(destroyed.texture.Value.Bounds.Center.X, destroyed.texture.Value.Bounds.Center.Y, 4, 4), (int)destroyed.tileX + Game1.random.Next(destroyed.tilesWide), (int)destroyed.tileY + (int)destroyed.tilesHigh - i, Game1.random.Next(20, 45), groundYTile);
					}
					Game1.playSound("explosion");
					Utility.spreadAnimalsAround(destroyed, (Farm)Game1.getLocationFromName("Farm"));
				}
				else
				{
					Game1.exitActiveMenu();
				}
			}
			else if (upgrading && Game1.currentLocation is Farm)
			{
				Building toUpgrade = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				if (toUpgrade != null && structureForPlacement.name != null && toUpgrade.buildingType.Equals(structureForPlacement.nameOfBuildingToUpgrade))
				{
					toUpgrade.indoors.Value.map = Game1.game1.xTileContent.Load<Map>("Maps\\" + structureForPlacement.mapToWarpTo);
					toUpgrade.indoors.Value.name.Value = structureForPlacement.mapToWarpTo;
					toUpgrade.buildingType.Value = structureForPlacement.name;
					toUpgrade.resetTexture();
					if (toUpgrade.indoors.Value is AnimalHouse)
					{
						((AnimalHouse)(GameLocation)toUpgrade.indoors).resetPositionsOfAllAnimals();
					}
					Game1.playSound("axe");
					structureForPlacement.consumeResources();
					toUpgrade.color.Value = Color.White;
					Game1.exitActiveMenu();
				}
				else if (toUpgrade != null)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10011"), Color.Red, 3500f));
				}
				else
				{
					Game1.exitActiveMenu();
				}
			}
			else if (queryingAnimals)
			{
				if (Game1.currentLocation is Farm || Game1.currentLocation is AnimalHouse)
				{
					foreach (FarmAnimal animal in (Game1.currentLocation is Farm) ? ((Farm)Game1.currentLocation).animals.Values.ToList() : ((AnimalHouse)Game1.currentLocation).animals.Values.ToList())
					{
						if (new Microsoft.Xna.Framework.Rectangle((int)animal.Position.X, (int)animal.Position.Y, animal.Sprite.SourceRect.Width, animal.Sprite.SourceRect.Height).Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()))
						{
							positionOfAnimalWhenClicked = Game1.GlobalToLocal(Game1.viewport, animal.Position);
							currentAnimal = animal;
							queryingAnimals = false;
							placingStructure = false;
							if (animal.sound.Value != null && !animal.sound.Value.Equals(""))
							{
								Game1.playSound(animal.sound);
							}
							break;
						}
					}
				}
			}
			else if (!(Game1.currentLocation is Farm))
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10012"), Color.Red, 3500f));
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
			else
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10016"), Color.Red, 3500f));
			}
		}

		public bool tryToBuild()
		{
			if (structureForPlacement.blueprintType.Equals("Animals"))
			{
				return ((Farm)Game1.getLocationFromName("Farm")).placeAnimal(structureForPlacement, new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64), serverCommand: false, Game1.player.UniqueMultiplayerID);
			}
			return ((Farm)Game1.getLocationFromName("Farm")).buildStructure(structureForPlacement, new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64), Game1.player);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (currentAnimal != null)
			{
				currentAnimal = null;
				queryingAnimals = true;
				placingStructure = true;
			}
			else if (placingStructure)
			{
				placingStructure = false;
				queryingAnimals = false;
				upgrading = false;
				demolishing = false;
			}
			else
			{
				Game1.exitActiveMenu();
			}
		}

		public override void performHoverAction(int x, int y)
		{
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
			else if (!placingStructure)
			{
				foreach (ClickableComponent c2 in tabs)
				{
					if (c2.containsPoint(x, y))
					{
						hoverText = c2.name;
						return;
					}
				}
				hoverText = "";
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
				if (!overAnyButton)
				{
					hoveredItem = null;
				}
			}
		}

		public int getTileSheetIndexForStructurePlacementTile(int x, int y)
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

		public override void draw(SpriteBatch b)
		{
			if (currentAnimal != null)
			{
				int x = (int)Math.Max(0f, Math.Min(positionOfAnimalWhenClicked.X - 256f + 32f, Game1.viewport.Width - 512));
				int y2 = (int)Math.Max(0f, Math.Min(Game1.viewport.Height - 256 - currentAnimal.frontBackSourceRect.Height, positionOfAnimalWhenClicked.Y - 256f - (float)currentAnimal.frontBackSourceRect.Height));
				Game1.drawDialogueBox(x, y2, 512, 352, speaker: false, drawOnlyBox: true);
				b.Draw(currentAnimal.Sprite.Texture, new Vector2(x + IClickableMenu.borderWidth + 96 - currentAnimal.frontBackSourceRect.Width / 2, y2 + IClickableMenu.borderWidth + 96), new Microsoft.Xna.Framework.Rectangle(0, 0, currentAnimal.frontBackSourceRect.Width, currentAnimal.frontBackSourceRect.Height), Color.White);
				float fullness = (float)(int)(byte)currentAnimal.fullness / 255f;
				float happiness = (float)(int)(byte)currentAnimal.happiness / 255f;
				string fullnessStr = Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10026");
				string happyStr = Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10027");
				b.DrawString(Game1.dialogueFont, currentAnimal.displayName, new Vector2((float)(x + IClickableMenu.borderWidth + 96) - Game1.dialogueFont.MeasureString(currentAnimal.displayName).X / 2f, y2 + IClickableMenu.borderWidth + 96 + currentAnimal.frontBackSourceRect.Height + 8), Game1.textColor);
				b.DrawString(Game1.dialogueFont, fullnessStr, new Vector2(x + IClickableMenu.borderWidth + 192, y2 + IClickableMenu.borderWidth + 96), Game1.textColor);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + 192, y2 + IClickableMenu.borderWidth + 96 + (int)Game1.dialogueFont.MeasureString(fullnessStr).Y + 8, 192, 16), Color.Gray);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + 192, y2 + IClickableMenu.borderWidth + 96 + (int)Game1.dialogueFont.MeasureString(fullnessStr).Y + 8, (int)(192f * fullness), 16), (!((double)fullness > 0.33)) ? Color.Red : (((double)fullness > 0.66) ? Color.Green : Color.Goldenrod));
				b.DrawString(Game1.dialogueFont, happyStr, new Vector2(x + IClickableMenu.borderWidth + 192, (float)(y2 + IClickableMenu.borderWidth + 96) + Game1.dialogueFont.MeasureString(fullnessStr).Y + 32f), Game1.textColor);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + 192, y2 + IClickableMenu.borderWidth + 96 + (int)Game1.dialogueFont.MeasureString(fullnessStr).Y + (int)Game1.dialogueFont.MeasureString(happyStr).Y + 32, 192, 16), Color.Gray);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + 192, y2 + IClickableMenu.borderWidth + 96 + (int)Game1.dialogueFont.MeasureString(fullnessStr).Y + (int)Game1.dialogueFont.MeasureString(happyStr).Y + 32, (int)(192f * happiness), 16), (!((double)happiness > 0.33)) ? Color.Red : (((double)happiness > 0.66) ? Color.Green : Color.Goldenrod));
			}
			else if (!placingStructure)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
				Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height - heightOfDescriptionBox, speaker: false, drawOnlyBox: true);
				foreach (ClickableComponent c2 in tabs)
				{
					int sheetIndex2 = 0;
					switch (c2.name)
					{
					case "Buildings":
						sheetIndex2 = 4;
						break;
					case "Upgrades":
						sheetIndex2 = 5;
						break;
					case "Decorations":
						sheetIndex2 = 7;
						break;
					case "Demolish":
						sheetIndex2 = 6;
						break;
					case "Animals":
						sheetIndex2 = 8;
						break;
					}
					b.Draw(Game1.mouseCursors, new Vector2(c2.bounds.X, c2.bounds.Y + ((currentTab == getTabNumberFromName(c2.name)) ? 8 : 0)), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, sheetIndex2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0001f);
				}
				foreach (ClickableComponent c in blueprintButtons[currentTab].Keys)
				{
					Texture2D structureTexture = blueprintButtons[currentTab][c].texture;
					Vector2 origin = c.name.Equals("Info Tool") ? new Vector2(32f, 32f) : new Vector2(blueprintButtons[currentTab][c].sourceRectForMenuView.Center.X, blueprintButtons[currentTab][c].sourceRectForMenuView.Center.Y);
					b.Draw(structureTexture, new Vector2(c.bounds.Center.X, c.bounds.Center.Y), blueprintButtons[currentTab][c].sourceRectForMenuView, Color.White, 0f, origin, 0.25f * c.scale + ((currentTab == 4) ? 0.75f : 0f), SpriteEffects.None, 0.9f);
				}
				Game1.drawWithBorder(hoverText, Color.Black, Color.White, new Vector2(Game1.getOldMouseX() + 64, Game1.getOldMouseY() + 64), 0f, 1f, 1f);
				Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen + (height - heightOfDescriptionBox) - IClickableMenu.borderWidth * 2, width, heightOfDescriptionBox, speaker: false, drawOnlyBox: true);
				if (hoveredItem == null)
				{
				}
			}
			else if (!demolishing && !upgrading && !queryingAnimals)
			{
				Vector2 mousePositionTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
				for (int y = 0; y < structureForPlacement.tilesHeight; y++)
				{
					for (int x2 = 0; x2 < structureForPlacement.tilesWidth; x2++)
					{
						int sheetIndex = getTileSheetIndexForStructurePlacementTile(x2, y);
						Vector2 currentGlobalTilePosition = new Vector2(mousePositionTile.X + (float)x2, mousePositionTile.Y + (float)y);
						if (Game1.player.getTileLocation().Equals(currentGlobalTilePosition) || Game1.currentLocation.isTileOccupied(currentGlobalTilePosition) || !Game1.currentLocation.isTilePassable(new Location((int)currentGlobalTilePosition.X, (int)currentGlobalTilePosition.Y), Game1.viewport))
						{
							sheetIndex++;
						}
						b.Draw(buildingPlacementTiles, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition * 64f), Game1.getSourceRectForStandardTileSheet(buildingPlacementTiles, sheetIndex), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.999f);
					}
				}
			}
			b.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, (queryingAnimals || currentAnimal != null) ? 9 : 0), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
		}
	}
}
