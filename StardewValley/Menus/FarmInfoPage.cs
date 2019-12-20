using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class FarmInfoPage : IClickableMenu
	{
		private string descriptionText = "";

		private string hoverText = "";

		private ClickableTextureComponent moneyIcon;

		private ClickableTextureComponent farmMap;

		private ClickableTextureComponent mapFarmer;

		private ClickableTextureComponent farmHouse;

		private List<ClickableTextureComponent> animals = new List<ClickableTextureComponent>();

		private List<ClickableTextureComponent> mapBuildings = new List<ClickableTextureComponent>();

		private List<MiniatureTerrainFeature> mapFeatures = new List<MiniatureTerrainFeature>();

		private Farm farm;

		private int mapX;

		private int mapY;

		public FarmInfoPage(int x, int y, int width, int height)
			: base(x, y, width, height)
		{
			moneyIcon = new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 32, (Game1.player.Money > 9999) ? 18 : 20, 16), Game1.player.Money + "g", "", Game1.debrisSpriteSheet, new Rectangle(88, 280, 16, 16), 1f);
			mapX = x + IClickableMenu.spaceToClearSideBorder + 128 + 32 + 16;
			mapY = y + IClickableMenu.spaceToClearTopBorder + 21 - 4;
			farmMap = new ClickableTextureComponent(new Rectangle(mapX, mapY, 20, 20), Game1.content.Load<Texture2D>("LooseSprites\\farmMap"), Rectangle.Empty, 1f);
			int numChickens = 0;
			int numDucks = 0;
			int numRabbits = 0;
			int numOther = 0;
			int numCows = 0;
			int numSheep = 0;
			int numGoats = 0;
			int numPigs = 0;
			int chickenHeart = 0;
			int rabbitHeart = 0;
			int duckHeart = 0;
			int otherHeart = 0;
			int cowHeart = 0;
			int goatHeart = 0;
			int sheepHeart = 0;
			int pigHeart = 0;
			farm = (Farm)Game1.getLocationFromName("Farm");
			farmHouse = new ClickableTextureComponent("FarmHouse", new Rectangle(mapX + 443, mapY + 43, 80, 72), "FarmHouse", "", Game1.content.Load<Texture2D>("Buildings\\houses"), new Rectangle(0, 0, 160, 144), 0.5f);
			foreach (FarmAnimal a in farm.getAllFarmAnimals())
			{
				if (a.type.Value.Contains("Chicken"))
				{
					numChickens++;
					chickenHeart += (int)a.friendshipTowardFarmer;
				}
				else
				{
					switch (a.type.Value)
					{
					case "Cow":
						numCows++;
						cowHeart += (int)a.friendshipTowardFarmer;
						break;
					case "Duck":
						numDucks++;
						duckHeart += (int)a.friendshipTowardFarmer;
						break;
					case "Rabbit":
						numRabbits++;
						rabbitHeart += (int)a.friendshipTowardFarmer;
						break;
					case "Sheep":
						numSheep++;
						sheepHeart += (int)a.friendshipTowardFarmer;
						break;
					case "Goat":
						numGoats++;
						goatHeart += (int)a.friendshipTowardFarmer;
						break;
					case "Pig":
						numPigs++;
						pigHeart += (int)a.friendshipTowardFarmer;
						break;
					default:
						numOther++;
						otherHeart += (int)a.friendshipTowardFarmer;
						break;
					}
				}
			}
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64, 40, 32), string.Concat(numChickens), "Chickens" + ((numChickens > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", chickenHeart / numChickens)) : ""), Game1.mouseCursors, new Rectangle(256, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 36, 40, 32), string.Concat(numDucks), "Ducks" + ((numDucks > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", duckHeart / numDucks)) : ""), Game1.mouseCursors, new Rectangle(288, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 72, 40, 32), string.Concat(numRabbits), "Rabbits" + ((numRabbits > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", rabbitHeart / numRabbits)) : ""), Game1.mouseCursors, new Rectangle(256, 96, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 108, 40, 32), string.Concat(numCows), "Cows" + ((numCows > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", cowHeart / numCows)) : ""), Game1.mouseCursors, new Rectangle(320, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 144, 40, 32), string.Concat(numGoats), "Goats" + ((numGoats > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", goatHeart / numGoats)) : ""), Game1.mouseCursors, new Rectangle(352, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 180, 40, 32), string.Concat(numSheep), "Sheep" + ((numSheep > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", sheepHeart / numSheep)) : ""), Game1.mouseCursors, new Rectangle(352, 96, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 216, 40, 32), string.Concat(numPigs), "Pigs" + ((numPigs > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", pigHeart / numPigs)) : ""), Game1.mouseCursors, new Rectangle(320, 96, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 252, 40, 32), string.Concat(numOther), "???" + ((numOther > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", otherHeart / numOther)) : ""), Game1.mouseCursors, new Rectangle(288, 96, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 288, 40, 32), string.Concat(Game1.stats.CropsShipped), Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10440"), Game1.mouseCursors, new Rectangle(480, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 324, 40, 32), string.Concat(farm.buildings.Count()), Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10441"), Game1.mouseCursors, new Rectangle(448, 64, 32, 32), 1f));
			int mapTileSize = 8;
			foreach (Building b in farm.buildings)
			{
				mapBuildings.Add(new ClickableTextureComponent("", new Rectangle(mapX + (int)b.tileX * mapTileSize, mapY + (int)b.tileY * mapTileSize + ((int)b.tilesHigh + 1) * mapTileSize - (int)((float)b.texture.Value.Height / 8f), (int)b.tilesWide * mapTileSize, (int)((float)b.texture.Value.Height / 8f)), "", b.buildingType, b.texture.Value, b.getSourceRectForMenu(), 0.125f));
			}
			foreach (KeyValuePair<Vector2, TerrainFeature> kvp in farm.terrainFeatures.Pairs)
			{
				mapFeatures.Add(new MiniatureTerrainFeature(kvp.Value, new Vector2(kvp.Key.X * (float)mapTileSize + (float)mapX, kvp.Key.Y * (float)mapTileSize + (float)mapY), kvp.Key, 0.125f));
			}
			if (Game1.currentLocation is Farm)
			{
				mapFarmer = new ClickableTextureComponent("", new Rectangle(mapX + (int)(Game1.player.Position.X / 8f), mapY + (int)(Game1.player.Position.Y / 8f), 8, 12), "", Game1.player.Name, null, new Rectangle(0, 0, 64, 96), 0.125f);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			hoverText = "";
			foreach (ClickableTextureComponent c in animals)
			{
				if (c.containsPoint(x, y))
				{
					hoverText = c.hoverText;
					return;
				}
			}
			foreach (ClickableTextureComponent b in mapBuildings)
			{
				if (b.containsPoint(x, y))
				{
					hoverText = b.hoverText;
					return;
				}
			}
			if (mapFarmer != null && mapFarmer.containsPoint(x, y))
			{
				hoverText = mapFarmer.hoverText;
			}
		}

		public override void draw(SpriteBatch b)
		{
			drawVerticalPartition(b, xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 128);
			moneyIcon.draw(b);
			foreach (ClickableTextureComponent animal in animals)
			{
				animal.draw(b);
			}
			farmMap.draw(b);
			foreach (ClickableTextureComponent mapBuilding in mapBuildings)
			{
				mapBuilding.draw(b);
			}
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			farmMap.draw(b);
			foreach (ClickableTextureComponent mapBuilding2 in mapBuildings)
			{
				mapBuilding2.draw(b);
			}
			foreach (MiniatureTerrainFeature mapFeature in mapFeatures)
			{
				mapFeature.draw(b);
			}
			farmHouse.draw(b);
			if (mapFarmer != null)
			{
				Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2(mapFarmer.bounds.X - 16, mapFarmer.bounds.Y - 16), 0.99f, 2f, 2, Game1.player);
			}
			foreach (KeyValuePair<long, FarmAnimal> kvp2 in farm.animals.Pairs)
			{
				b.Draw(kvp2.Value.Sprite.Texture, new Vector2(mapX + (int)(kvp2.Value.Position.X / 8f), mapY + (int)(kvp2.Value.Position.Y / 8f)), kvp2.Value.Sprite.SourceRect, Color.White, 0f, Vector2.Zero, 0.125f, SpriteEffects.None, 0.86f + kvp2.Value.Position.Y / 8f / 20000f + 0.0125f);
			}
			foreach (KeyValuePair<Vector2, Object> kvp in farm.objects.Pairs)
			{
				kvp.Value.drawInMenu(b, new Vector2((float)mapX + kvp.Key.X * 8f, (float)mapY + kvp.Key.Y * 8f), 0.125f, 1f, 0.86f + ((float)mapY + kvp.Key.Y * 8f - 25f) / 20000f);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
		}
	}
}
