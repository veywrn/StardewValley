using Microsoft.Xna.Framework;
using StardewValley.Locations;
using System.Collections.Generic;
using xTile.Dimensions;

namespace StardewValley.Tools
{
	public class Hoe : Tool
	{
		public Hoe()
			: base("Hoe", 0, 21, 47, stackable: false)
		{
			base.UpgradeLevel = 0;
		}

		public override Item getOne()
		{
			return new Hoe
			{
				UpgradeLevel = base.UpgradeLevel
			};
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Hoe.cs.14101");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Hoe.cs.14102");
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			if (location.Name.StartsWith("UndergroundMine"))
			{
				power = 1;
			}
			who.Stamina -= (float)(2 * power) - (float)who.FarmingLevel * 0.1f;
			power = who.toolPower;
			who.stopJittering();
			location.playSound("woodyHit");
			Vector2 initialTile = new Vector2(x / 64, y / 64);
			List<Vector2> tileLocations = tilesAffected(initialTile, power, who);
			foreach (Vector2 tileLocation in tileLocations)
			{
				tileLocation.Equals(initialTile);
				if (location.terrainFeatures.ContainsKey(tileLocation))
				{
					if (location.terrainFeatures[tileLocation].performToolAction(this, 0, tileLocation, location))
					{
						location.terrainFeatures.Remove(tileLocation);
					}
				}
				else
				{
					if (location.objects.ContainsKey(tileLocation) && location.Objects[tileLocation].performToolAction(this, location))
					{
						if (location.Objects[tileLocation].type.Equals("Crafting") && (int)location.Objects[tileLocation].fragility != 2)
						{
							location.debris.Add(new Debris(location.Objects[tileLocation].bigCraftable ? (-location.Objects[tileLocation].ParentSheetIndex) : location.Objects[tileLocation].ParentSheetIndex, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y)));
						}
						location.Objects[tileLocation].performRemoveAction(tileLocation, location);
						location.Objects.Remove(tileLocation);
					}
					if (location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") != null)
					{
						if (location.Name.StartsWith("UndergroundMine") && !location.isTileOccupied(tileLocation))
						{
							if ((location as MineShaft).getMineArea() != 77377)
							{
								location.makeHoeDirt(tileLocation);
								location.playSound("hoeHit");
								Game1.removeSquareDebrisFromTile((int)tileLocation.X, (int)tileLocation.Y);
								location.checkForBuriedItem((int)tileLocation.X, (int)tileLocation.Y, explosion: false, detectOnly: false, who);
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
								if (tileLocations.Count > 2)
								{
									Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(6, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(initialTile, tileLocation) * 30f));
								}
							}
						}
						else if (!location.isTileOccupied(tileLocation) && location.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport))
						{
							location.makeHoeDirt(tileLocation);
							location.playSound("hoeHit");
							Game1.removeSquareDebrisFromTile((int)tileLocation.X, (int)tileLocation.Y);
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
							if (tileLocations.Count > 2)
							{
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(6, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(initialTile, tileLocation) * 30f));
							}
							location.checkForBuriedItem((int)tileLocation.X, (int)tileLocation.Y, explosion: false, detectOnly: false, who);
						}
						Game1.stats.DirtHoed++;
					}
				}
			}
		}
	}
}
