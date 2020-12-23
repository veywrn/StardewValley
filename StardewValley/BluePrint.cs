using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class BluePrint
	{
		public string name;

		public int woodRequired;

		public int stoneRequired;

		public int copperRequired;

		public int IronRequired;

		public int GoldRequired;

		public int IridiumRequired;

		public int tilesWidth;

		public int tilesHeight;

		public int maxOccupants;

		public int moneyRequired;

		public int daysToConstruct;

		public Point humanDoor;

		public Point animalDoor;

		public string mapToWarpTo;

		public string description;

		public string blueprintType;

		public string nameOfBuildingToUpgrade;

		public string actionBehavior;

		public string displayName;

		public readonly string textureName;

		public readonly Texture2D texture;

		public List<string> namesOfOkayBuildingLocations = new List<string>();

		public Rectangle sourceRectForMenuView;

		public Dictionary<int, int> itemsRequired = new Dictionary<int, int>();

		public bool canBuildOnCurrentMap;

		public bool magical;

		public List<Point> additionalPlacementTiles = new List<Point>();

		public BluePrint(string name)
		{
			this.name = name;
			if (name.Equals("Info Tool"))
			{
				textureName = "LooseSprites\\Cursors";
				displayName = name;
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:BluePrint.cs.1");
				sourceRectForMenuView = new Rectangle(576, 0, 64, 64);
			}
			else
			{
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Blueprints");
				string rawData = null;
				dictionary.TryGetValue(name, out rawData);
				if (rawData != null)
				{
					string[] split = rawData.Split('/');
					if (split[0].Equals("animal"))
					{
						try
						{
							textureName = "Animals\\" + (name.Equals("Chicken") ? "White Chicken" : name);
						}
						catch (Exception)
						{
							Game1.debugOutput = "Blueprint loaded with no texture!";
						}
						moneyRequired = Convert.ToInt32(split[1]);
						sourceRectForMenuView = new Rectangle(0, 0, Convert.ToInt32(split[2]), Convert.ToInt32(split[3]));
						blueprintType = "Animals";
						tilesWidth = 1;
						tilesHeight = 1;
						displayName = split[4];
						description = split[5];
						humanDoor = new Point(-1, -1);
						animalDoor = new Point(-1, -1);
					}
					else
					{
						textureName = "Buildings\\" + name;
						string[] recipeSplit = split[0].Split(' ');
						for (int j = 0; j < recipeSplit.Length; j += 2)
						{
							if (!recipeSplit[j].Equals(""))
							{
								itemsRequired.Add(Convert.ToInt32(recipeSplit[j]), Convert.ToInt32(recipeSplit[j + 1]));
							}
						}
						tilesWidth = Convert.ToInt32(split[1]);
						tilesHeight = Convert.ToInt32(split[2]);
						humanDoor = new Point(Convert.ToInt32(split[3]), Convert.ToInt32(split[4]));
						animalDoor = new Point(Convert.ToInt32(split[5]), Convert.ToInt32(split[6]));
						mapToWarpTo = split[7];
						displayName = split[8];
						description = split[9];
						blueprintType = split[10];
						if (blueprintType.Equals("Upgrades"))
						{
							nameOfBuildingToUpgrade = split[11];
						}
						sourceRectForMenuView = new Rectangle(0, 0, Convert.ToInt32(split[12]), Convert.ToInt32(split[13]));
						maxOccupants = Convert.ToInt32(split[14]);
						actionBehavior = split[15];
						string[] array = split[16].Split(' ');
						foreach (string s in array)
						{
							namesOfOkayBuildingLocations.Add(s);
						}
						int count = 17;
						if (split.Length > count)
						{
							moneyRequired = Convert.ToInt32(split[17]);
						}
						if (split.Length > count + 1)
						{
							magical = Convert.ToBoolean(split[18]);
						}
						if (split.Length > count + 2)
						{
							daysToConstruct = Convert.ToInt32(split[19]);
						}
						else
						{
							daysToConstruct = 2;
						}
						if (split.Length > count + 3)
						{
							string obj = split[20];
							additionalPlacementTiles.Clear();
							string[] additional_placement_coordinates = obj.Split(' ');
							for (int i = 0; i < additional_placement_coordinates.Length / 2; i++)
							{
								int x = Convert.ToInt32(additional_placement_coordinates[i * 2]);
								int y = Convert.ToInt32(additional_placement_coordinates[i * 2 + 1]);
								additionalPlacementTiles.Add(new Point(x, y));
							}
						}
					}
				}
			}
			try
			{
				texture = Game1.content.Load<Texture2D>(textureName);
			}
			catch (Exception)
			{
			}
		}

		public void consumeResources()
		{
			foreach (KeyValuePair<int, int> kvp in itemsRequired)
			{
				Game1.player.consumeObject(kvp.Key, kvp.Value);
			}
			Game1.player.Money -= moneyRequired;
		}

		public int getTileSheetIndexForStructurePlacementTile(int x, int y)
		{
			if (x == humanDoor.X && y == humanDoor.Y)
			{
				return 2;
			}
			if (x == animalDoor.X && y == animalDoor.Y)
			{
				return 4;
			}
			return 0;
		}

		public bool isUpgrade()
		{
			if (nameOfBuildingToUpgrade != null)
			{
				return nameOfBuildingToUpgrade.Length > 0;
			}
			return false;
		}

		public bool doesFarmerHaveEnoughResourcesToBuild()
		{
			if (moneyRequired < 0)
			{
				return false;
			}
			foreach (KeyValuePair<int, int> kvp in itemsRequired)
			{
				if (!Game1.player.hasItemInInventory(kvp.Key, kvp.Value))
				{
					return false;
				}
			}
			if (Game1.player.Money < moneyRequired)
			{
				return false;
			}
			return true;
		}

		public void drawDescription(SpriteBatch b, int x, int y, int width)
		{
			b.DrawString(Game1.smallFont, name, new Vector2(x, y), Game1.textColor);
			string descriptionString = Game1.parseText(description, Game1.smallFont, width);
			b.DrawString(Game1.smallFont, descriptionString, new Vector2(x, (float)y + Game1.smallFont.MeasureString(name).Y), Game1.textColor * 0.75f);
			int yPosition2 = (int)((float)y + Game1.smallFont.MeasureString(name).Y + Game1.smallFont.MeasureString(descriptionString).Y);
			foreach (KeyValuePair<int, int> kvp in itemsRequired)
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2(x + 8, yPosition2), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, kvp.Key, 16, 16), Color.White, 0f, new Vector2(6f, 3f), 2f, SpriteEffects.None, 0.999f);
				Color colorToDrawResource2 = Game1.player.hasItemInInventory(kvp.Key, kvp.Value) ? Color.DarkGreen : Color.DarkRed;
				Utility.drawTinyDigits(kvp.Value, b, new Vector2((float)(x + 32) - Game1.tinyFont.MeasureString(string.Concat(kvp.Value)).X, (float)(yPosition2 + 32) - Game1.tinyFont.MeasureString(string.Concat(kvp.Value)).Y), 1f, 0.9f, Color.AntiqueWhite);
				b.DrawString(Game1.smallFont, Game1.objectInformation[kvp.Key].Split('/')[4], new Vector2(x + 32 + 16, yPosition2), colorToDrawResource2);
				yPosition2 += (int)Game1.smallFont.MeasureString("P").Y;
			}
			if (moneyRequired > 0)
			{
				b.Draw(Game1.debrisSpriteSheet, new Vector2(x, yPosition2), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8), Color.White, 0f, new Vector2(24f, 11f), 0.5f, SpriteEffects.None, 0.999f);
				Color colorToDrawResource = (Game1.player.Money >= moneyRequired) ? Color.DarkGreen : Color.DarkRed;
				b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", moneyRequired), new Vector2(x + 16 + 8, yPosition2), colorToDrawResource);
				yPosition2 += (int)Game1.smallFont.MeasureString(string.Concat(moneyRequired)).Y;
			}
		}
	}
}
