using System.Collections.Generic;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	internal class FurniturePlacer
	{
		public static void addAllFurnitureOwnedByFarmer()
		{
			foreach (string item in Game1.player.furnitureOwned)
			{
				addFurniture(item);
			}
		}

		public static void addFurniture(string furnitureName)
		{
			if (furnitureName.Equals("Television"))
			{
				GameLocation farmhouse = Game1.getLocationFromName("FarmHouse");
				farmhouse.Map.GetLayer("Buildings").Tiles[6, 3] = new StaticTile(farmhouse.Map.GetLayer("Buildings"), farmhouse.Map.GetTileSheet("Farmhouse"), BlendMode.Alpha, 12);
				farmhouse.Map.GetLayer("Buildings").Tiles[6, 3].Properties.Add("Action", new PropertyValue("TV"));
				farmhouse.Map.GetLayer("Buildings").Tiles[7, 3] = new StaticTile(farmhouse.Map.GetLayer("Buildings"), farmhouse.Map.GetTileSheet("Farmhouse"), BlendMode.Alpha, 13);
				farmhouse.Map.GetLayer("Buildings").Tiles[7, 3].Properties.Add("Action", new PropertyValue("TV"));
				farmhouse.Map.GetLayer("Buildings").Tiles[6, 2] = new StaticTile(farmhouse.Map.GetLayer("Buildings"), farmhouse.Map.GetTileSheet("Farmhouse"), BlendMode.Alpha, 4);
				farmhouse.Map.GetLayer("Buildings").Tiles[7, 2] = new StaticTile(farmhouse.Map.GetLayer("Buildings"), farmhouse.Map.GetTileSheet("Farmhouse"), BlendMode.Alpha, 5);
			}
			else if (furnitureName.Equals("Incubator"))
			{
				GameLocation coop = Game1.getLocationFromName("Coop");
				coop.map.GetLayer("Buildings").Tiles[1, 3] = new StaticTile(coop.map.GetLayer("Buildings"), coop.map.TileSheets[0], BlendMode.Alpha, 44);
				coop.map.GetLayer("Buildings").Tiles[1, 3].Properties.Add(new KeyValuePair<string, PropertyValue>("Action", new PropertyValue("Incubator")));
				coop.map.GetLayer("Front").Tiles[1, 2] = new StaticTile(coop.map.GetLayer("Front"), coop.map.TileSheets[0], BlendMode.Alpha, 45);
			}
			if (!Game1.player.furnitureOwned.Contains(furnitureName))
			{
				Game1.player.furnitureOwned.Add(furnitureName);
			}
		}
	}
}
