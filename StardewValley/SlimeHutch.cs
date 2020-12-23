using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Monsters;
using StardewValley.Tools;
using System;
using System.Xml.Serialization;

namespace StardewValley
{
	public class SlimeHutch : GameLocation
	{
		[XmlElement("slimeMatingsLeft")]
		public readonly NetInt slimeMatingsLeft = new NetInt();

		public readonly NetArray<bool, NetBool> waterSpots = new NetArray<bool, NetBool>(4);

		public SlimeHutch()
		{
		}

		public SlimeHutch(string m, string name)
			: base(m, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(slimeMatingsLeft, waterSpots);
		}

		public void updateWhenNotCurrentLocation(Building parentBuilding, GameTime time)
		{
		}

		public bool isFull()
		{
			return characters.Count >= 20;
		}

		public Building getBuilding()
		{
			foreach (Building b in Game1.getFarm().buildings)
			{
				if (b.indoors.Value != null && b.indoors.Value.Equals(this))
				{
					return b;
				}
			}
			return null;
		}

		public override bool canSlimeMateHere()
		{
			int matesLeft = slimeMatingsLeft;
			slimeMatingsLeft.Value--;
			if (!isFull())
			{
				return matesLeft > 0;
			}
			return false;
		}

		public override bool canSlimeHatchHere()
		{
			return !isFull();
		}

		public override void DayUpdate(int dayOfMonth)
		{
			int waters = 0;
			int startIndex = Game1.random.Next(waterSpots.Length);
			for (int i = 0; i < waterSpots.Length; i++)
			{
				if (waterSpots[(i + startIndex) % waterSpots.Length] && waters * 5 < characters.Count)
				{
					waters++;
					waterSpots[(i + startIndex) % waterSpots.Length] = false;
				}
			}
			for (int j = objects.Count() - 1; j >= 0; j--)
			{
				if (objects.Pairs.ElementAt(j).Value.IsSprinkler())
				{
					foreach (Vector2 v in objects.Pairs.ElementAt(j).Value.GetSprinklerTiles())
					{
						if (v.X == 16f && v.Y >= 6f && v.Y <= 9f)
						{
							waterSpots[(int)v.Y - 6] = true;
						}
					}
				}
			}
			for (int numSlimeBalls = Math.Min(characters.Count / 5, waters); numSlimeBalls > 0; numSlimeBalls--)
			{
				int tries = 50;
				Vector2 tile = getRandomTile();
				while ((!isTileLocationTotallyClearAndPlaceable(tile) || doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 12f) && tries > 0)
				{
					tile = getRandomTile();
					tries--;
				}
				if (tries > 0)
				{
					objects.Add(tile, new Object(tile, 56));
				}
			}
			while ((int)slimeMatingsLeft > 0)
			{
				if (characters.Count > 1 && !isFull())
				{
					NPC mate4 = characters[Game1.random.Next(characters.Count)];
					if (mate4 is GreenSlime)
					{
						GreenSlime mate3 = mate4 as GreenSlime;
						if ((int)mate3.ageUntilFullGrown <= 0)
						{
							for (int distance = 1; distance < 10; distance++)
							{
								GreenSlime mate2 = (GreenSlime)Utility.checkForCharacterWithinArea(mate3.GetType(), mate4.Position, this, new Rectangle((int)mate3.Position.X - 64 * distance, (int)mate3.Position.Y - 64 * distance, 64 * (distance * 2 + 1), 64 * (distance * 2 + 1)));
								if (mate2 != null && mate2.cute != mate3.cute && (int)mate2.ageUntilFullGrown <= 0)
								{
									mate3.mateWith(mate2, this);
									break;
								}
							}
						}
					}
				}
				slimeMatingsLeft.Value--;
			}
			slimeMatingsLeft.Value = characters.Count / 5 + 1;
			base.DayUpdate(dayOfMonth);
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (l is SlimeHutch)
			{
				for (int i = 0; i < waterSpots.Length; i++)
				{
					if (i < (l as SlimeHutch).waterSpots.Count)
					{
						waterSpots[i] = (l as SlimeHutch).waterSpots[i];
					}
				}
			}
			base.TransferDataFromSavedLocation(l);
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (t is WateringCan && tileX == 16 && tileY >= 6 && tileY <= 9)
			{
				waterSpots[tileY - 6] = true;
			}
			return false;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			for (int i = 0; i < waterSpots.Length; i++)
			{
				if (waterSpots[i])
				{
					setMapTileIndex(16, 6 + i, 2135, "Buildings");
				}
				else
				{
					setMapTileIndex(16, 6 + i, 2134, "Buildings");
				}
			}
		}
	}
}
