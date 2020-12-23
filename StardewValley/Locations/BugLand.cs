using Microsoft.Xna.Framework;
using StardewValley.Monsters;
using System.Xml.Serialization;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class BugLand : GameLocation
	{
		[XmlElement("hasSpawnedBugsToday")]
		public bool hasSpawnedBugsToday;

		public BugLand()
		{
		}

		public BugLand(string map, string name)
			: base(map, name)
		{
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (l is BugLand)
			{
				BugLand location = l as BugLand;
				hasSpawnedBugsToday = location.hasSpawnedBugsToday;
			}
			base.TransferDataFromSavedLocation(l);
		}

		public override void hostSetup()
		{
			base.hostSetup();
			if (Game1.IsMasterGame && !hasSpawnedBugsToday)
			{
				InitializeBugLand();
			}
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i] is Grub || characters[i] is Fly)
				{
					characters.RemoveAt(i);
					i--;
				}
			}
			hasSpawnedBugsToday = false;
		}

		public virtual void InitializeBugLand()
		{
			if (hasSpawnedBugsToday)
			{
				return;
			}
			hasSpawnedBugsToday = true;
			for (int x = 0; x < map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < map.Layers[0].LayerHeight; y++)
				{
					if (!(Game1.random.NextDouble() < 0.33))
					{
						continue;
					}
					Tile t = map.GetLayer("Paths").Tiles[x, y];
					if (t == null)
					{
						continue;
					}
					Vector2 tile = new Vector2(x, y);
					switch (t.TileIndex)
					{
					case 13:
					case 14:
					case 15:
						if (!objects.ContainsKey(tile))
						{
							objects.Add(tile, new Object(tile, GameLocation.getWeedForSeason(Game1.random, "spring"), 1));
						}
						break;
					case 16:
						if (!objects.ContainsKey(tile))
						{
							objects.Add(tile, new Object(tile, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 17:
						if (!objects.ContainsKey(tile))
						{
							objects.Add(tile, new Object(tile, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 18:
						if (!objects.ContainsKey(tile))
						{
							objects.Add(tile, new Object(tile, (Game1.random.NextDouble() < 0.5) ? 294 : 295, 1));
						}
						break;
					case 28:
						if (isTileLocationTotallyClearAndPlaceable(tile) && characters.Count < 50)
						{
							characters.Add(new Grub(new Vector2(tile.X * 64f, tile.Y * 64f), hard: true));
						}
						break;
					}
				}
			}
		}
	}
}
