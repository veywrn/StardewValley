using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class BuildableGameLocation : GameLocation
	{
		public readonly NetCollection<Building> buildings = new NetCollection<Building>();

		private Microsoft.Xna.Framework.Rectangle caveNoBuildRect = new Microsoft.Xna.Framework.Rectangle(32, 8, 5, 3);

		private Microsoft.Xna.Framework.Rectangle shippingAreaNoBuildRect = new Microsoft.Xna.Framework.Rectangle(69, 10, 4, 4);

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(buildings);
			buildings.InterpolationWait = false;
		}

		public BuildableGameLocation()
		{
		}

		public BuildableGameLocation(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			foreach (Building building in buildings)
			{
				building.dayUpdate(dayOfMonth);
			}
		}

		public override void cleanupBeforeSave()
		{
			foreach (Building building in buildings)
			{
				if (building.indoors.Value != null)
				{
					building.indoors.Value.cleanupBeforeSave();
				}
			}
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			foreach (Building b in buildings)
			{
				if (b.occupiesTile(new Vector2(tileX, tileY)))
				{
					b.performToolAction(t, tileX, tileY);
				}
			}
			return base.performToolAction(t, tileX, tileY);
		}

		public virtual void timeUpdate(int timeElapsed)
		{
			foreach (Building b in buildings)
			{
				if (b.indoors.Value != null && b.indoors.Value is AnimalHouse)
				{
					foreach (KeyValuePair<long, FarmAnimal> pair in ((AnimalHouse)(GameLocation)b.indoors).animals.Pairs)
					{
						pair.Value.updatePerTenMinutes(Game1.timeOfDay, b.indoors);
					}
				}
			}
		}

		public Building getBuildingAt(Vector2 tile)
		{
			foreach (Building building in buildings)
			{
				if (!building.isTilePassable(tile))
				{
					return building;
				}
			}
			return null;
		}

		public Building getBuildingByName(string name)
		{
			foreach (Building building in buildings)
			{
				if (string.Equals(building.nameOfIndoors, name, StringComparison.Ordinal))
				{
					return building;
				}
			}
			return null;
		}

		public override bool leftClick(int x, int y, Farmer who)
		{
			foreach (Building building in buildings)
			{
				if (building.CanLeftClick(x, y))
				{
					building.leftClicked();
				}
			}
			return base.leftClick(x, y, who);
		}

		public bool destroyStructure(Vector2 tile)
		{
			Building building = getBuildingAt(tile);
			if (building != null)
			{
				building.performActionOnDemolition(this);
				buildings.Remove(building);
				return true;
			}
			return false;
		}

		public bool destroyStructure(Building b)
		{
			b.performActionOnDemolition(this);
			return buildings.Remove(b);
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			if (!glider && buildings.Count > 0)
			{
				Microsoft.Xna.Framework.Rectangle playerBox = Game1.player.GetBoundingBox();
				FarmAnimal animal = character as FarmAnimal;
				bool isJunimo = character is JunimoHarvester;
				bool isNPC = character is NPC;
				foreach (Building b in buildings)
				{
					if (b.intersects(position) && (!isFarmer || !b.intersects(playerBox)))
					{
						if (animal != null)
						{
							Microsoft.Xna.Framework.Rectangle door = b.getRectForAnimalDoor();
							door.Height += 64;
							if (door.Contains(position) && b.buildingType.Value.Contains(animal.buildingTypeILiveIn.Value))
							{
								continue;
							}
						}
						else if (isJunimo)
						{
							Microsoft.Xna.Framework.Rectangle door2 = b.getRectForAnimalDoor();
							door2.Height += 64;
							if (door2.Contains(position))
							{
								continue;
							}
						}
						else if (isNPC)
						{
							Microsoft.Xna.Framework.Rectangle door3 = b.getRectForHumanDoor();
							door3.Height += 64;
							if (door3.Contains(position))
							{
								continue;
							}
						}
						return true;
					}
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
		}

		public override bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			foreach (Building building in buildings)
			{
				if (building.isActionableTile(xTile, yTile, who))
				{
					return true;
				}
			}
			return base.isActionableTile(xTile, yTile, who);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			foreach (Building building in buildings)
			{
				if (building.doAction(new Vector2(tileLocation.X, tileLocation.Y), who))
				{
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override bool isTileOccupied(Vector2 tileLocation, string characterToIngore = "", bool ignoreAllCharacters = false)
		{
			foreach (Building building in buildings)
			{
				if (!building.isTilePassable(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupied(tileLocation, characterToIngore, ignoreAllCharacters);
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (Building building in buildings)
			{
				if (building.isTileOccupiedForPlacement(tileLocation, toPlace))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			foreach (Building building in buildings)
			{
				building.updateWhenFarmNotCurrentLocation(time);
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (!wasUpdated || Game1.gameMode == 0)
			{
				base.UpdateWhenCurrentLocation(time);
				foreach (Building building in buildings)
				{
					building.Update(time);
				}
			}
		}

		public override void drawFloorDecorations(SpriteBatch b)
		{
			int border_buffer = 1;
			Microsoft.Xna.Framework.Rectangle viewport_rect = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X / 64 - border_buffer, Game1.viewport.Y / 64 - border_buffer, (int)Math.Ceiling((float)Game1.viewport.Width / 64f) + 2 * border_buffer, (int)Math.Ceiling((float)Game1.viewport.Height / 64f) + 3 + 2 * border_buffer);
			Microsoft.Xna.Framework.Rectangle object_rectangle = default(Microsoft.Xna.Framework.Rectangle);
			foreach (Building building in buildings)
			{
				int additional_radius = building.GetAdditionalTilePropertyRadius();
				object_rectangle.X = (int)building.tileX - additional_radius;
				object_rectangle.Width = (int)building.tilesWide + additional_radius * 2;
				int bottom_y = (int)building.tileY + (int)building.tilesHigh + additional_radius;
				object_rectangle.Height = bottom_y - (object_rectangle.Y = bottom_y - (int)Math.Ceiling((float)building.getSourceRect().Height * 4f / 64f) - additional_radius);
				if (object_rectangle.Intersects(viewport_rect))
				{
					building.drawBackground(b);
				}
			}
			base.drawFloorDecorations(b);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			int border_buffer = 1;
			Microsoft.Xna.Framework.Rectangle viewport_rect = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X / 64 - border_buffer, Game1.viewport.Y / 64 - border_buffer, (int)Math.Ceiling((float)Game1.viewport.Width / 64f) + 2 * border_buffer, (int)Math.Ceiling((float)Game1.viewport.Height / 64f) + 3 + 2 * border_buffer);
			Microsoft.Xna.Framework.Rectangle object_rectangle = default(Microsoft.Xna.Framework.Rectangle);
			foreach (Building building in buildings)
			{
				int additional_radius = building.GetAdditionalTilePropertyRadius();
				object_rectangle.X = (int)building.tileX - additional_radius;
				object_rectangle.Width = (int)building.tilesWide + additional_radius * 2;
				int bottom_y = (int)building.tileY + (int)building.tilesHigh + additional_radius;
				object_rectangle.Height = bottom_y - (object_rectangle.Y = bottom_y - (int)Math.Ceiling((float)building.getSourceRect().Height * 4f / 64f) - additional_radius);
				if (object_rectangle.Intersects(viewport_rect))
				{
					building.draw(b);
				}
			}
		}

		public void tryToUpgrade(Building toUpgrade, BluePrint blueprint)
		{
			if (toUpgrade != null && blueprint.name != null && toUpgrade.buildingType.Equals(blueprint.nameOfBuildingToUpgrade))
			{
				if (toUpgrade.indoors.Value.farmers.Any())
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\Locations:BuildableLocation_CantUpgrade_SomeoneInside"), Color.Red, 3500f));
					return;
				}
				toUpgrade.indoors.Value.map = Game1.game1.xTileContent.Load<Map>("Maps\\" + blueprint.mapToWarpTo);
				toUpgrade.indoors.Value.name.Value = blueprint.mapToWarpTo;
				toUpgrade.indoors.Value.isStructure.Value = true;
				toUpgrade.buildingType.Value = blueprint.name;
				toUpgrade.resetTexture();
				if (toUpgrade.indoors.Value is AnimalHouse)
				{
					((AnimalHouse)(GameLocation)toUpgrade.indoors).resetPositionsOfAllAnimals();
				}
				playSound("axe");
				blueprint.consumeResources();
				toUpgrade.performActionOnUpgrade(this);
				toUpgrade.color.Value = Color.White;
				Game1.exitActiveMenu();
				Game1.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, Utility.AOrAn(blueprint.displayName), blueprint.displayName, Game1.player.farmName);
			}
			else if (toUpgrade != null)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\Locations:BuildableLocation_CantUpgrade_IncorrectBuildingType"), Color.Red, 3500f));
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			foreach (Building building in buildings)
			{
				building.resetLocalState();
			}
			if (Game1.whichFarm == 5)
			{
				caveNoBuildRect = new Microsoft.Xna.Framework.Rectangle(29, 38, 3, 1);
			}
		}

		public bool isBuildingConstructed(string name)
		{
			foreach (Building b in buildings)
			{
				if (b.buildingType.Value.Equals(name) && (int)b.daysOfConstructionLeft <= 0)
				{
					return true;
				}
			}
			return false;
		}

		public int getNumberBuildingsConstructed(string name)
		{
			int count = 0;
			foreach (Building b in buildings)
			{
				if (b.buildingType.Value.Contains(name) && (int)b.daysOfConstructionLeft <= 0 && (int)b.daysUntilUpgrade <= 0)
				{
					count++;
				}
			}
			return count;
		}

		public bool isThereABuildingUnderConstruction()
		{
			foreach (Building b in buildings)
			{
				if ((int)b.daysOfConstructionLeft > 0 || (int)b.daysUntilUpgrade > 0)
				{
					return true;
				}
			}
			return false;
		}

		public Building getBuildingUnderConstruction()
		{
			foreach (Building b in buildings)
			{
				if ((int)b.daysOfConstructionLeft > 0 || (int)b.daysUntilUpgrade > 0)
				{
					return b;
				}
			}
			return null;
		}

		public bool buildStructure(Building b, Vector2 tileLocation, Farmer who, bool skipSafetyChecks = false)
		{
			if (!skipSafetyChecks)
			{
				for (int y = 0; y < (int)b.tilesHigh; y++)
				{
					for (int x = 0; x < (int)b.tilesWide; x++)
					{
						pokeTileForConstruction(new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y));
					}
				}
				foreach (Point additionalPlacementTile in b.additionalPlacementTiles)
				{
					int x4 = additionalPlacementTile.X;
					int y4 = additionalPlacementTile.Y;
					pokeTileForConstruction(new Vector2(tileLocation.X + (float)x4, tileLocation.Y + (float)y4));
				}
				for (int y3 = 0; y3 < (int)b.tilesHigh; y3++)
				{
					for (int x2 = 0; x2 < (int)b.tilesWide; x2++)
					{
						Vector2 currentGlobalTilePosition = new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y3);
						if (!buildings.Contains(b) || !b.occupiesTile(currentGlobalTilePosition))
						{
							if (!isBuildable(currentGlobalTilePosition))
							{
								return false;
							}
							foreach (Farmer farmer in farmers)
							{
								if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x2 * 64, y3 * 64, 64, 64)))
								{
									return false;
								}
							}
						}
					}
				}
				foreach (Point additionalPlacementTile2 in b.additionalPlacementTiles)
				{
					int x3 = additionalPlacementTile2.X;
					int y2 = additionalPlacementTile2.Y;
					Vector2 currentGlobalTilePosition2 = new Vector2(tileLocation.X + (float)x3, tileLocation.Y + (float)y2);
					if (!buildings.Contains(b) || !b.occupiesTile(currentGlobalTilePosition2))
					{
						if (!isBuildable(currentGlobalTilePosition2))
						{
							return false;
						}
						foreach (Farmer farmer2 in farmers)
						{
							if (farmer2.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x3 * 64, y2 * 64, 64, 64)))
							{
								return false;
							}
						}
					}
				}
				if (b.humanDoor.Value != new Point(-1, -1))
				{
					Vector2 doorPos = tileLocation + new Vector2(b.humanDoor.X, b.humanDoor.Y + 1);
					if ((!buildings.Contains(b) || !b.occupiesTile(doorPos)) && !isBuildable(doorPos) && !isPath(doorPos))
					{
						return false;
					}
				}
				string finalCheckResult = b.isThereAnythingtoPreventConstruction(this, tileLocation);
				if (finalCheckResult != null)
				{
					Game1.addHUDMessage(new HUDMessage(finalCheckResult, Color.Red, 3500f));
					return false;
				}
			}
			b.tileX.Value = (int)tileLocation.X;
			b.tileY.Value = (int)tileLocation.Y;
			if (b.indoors.Value != null && b.indoors.Value is AnimalHouse)
			{
				foreach (long a in (b.indoors.Value as AnimalHouse).animalsThatLiveHere)
				{
					FarmAnimal animal2 = Utility.getAnimal(a);
					if (animal2 != null)
					{
						animal2.homeLocation.Value = tileLocation;
						animal2.home = b;
					}
					else if (animal2 == null && (b.indoors.Value as AnimalHouse).animals.ContainsKey(a))
					{
						animal2 = (b.indoors.Value as AnimalHouse).animals[a];
						animal2.homeLocation.Value = tileLocation;
						animal2.home = b;
					}
				}
			}
			if (b.indoors.Value != null)
			{
				foreach (Warp warp in b.indoors.Value.warps)
				{
					warp.TargetX = b.humanDoor.X + (int)b.tileX;
					warp.TargetY = b.humanDoor.Y + (int)b.tileY + 1;
				}
			}
			if (!buildings.Contains(b))
			{
				buildings.Add(b);
			}
			return true;
		}

		public override string doesTileHaveProperty(int xTile, int yTile, string propertyName, string layerName)
		{
			foreach (Building building in buildings)
			{
				int additional_radius = building.GetAdditionalTilePropertyRadius();
				if (xTile >= (int)building.tileX - additional_radius && xTile < (int)building.tileX + (int)building.tilesWide + additional_radius && yTile >= (int)building.tileY - additional_radius && yTile < (int)building.tileY + (int)building.tilesHigh + additional_radius)
				{
					string tile_property = null;
					if (building.doesTileHaveProperty(xTile, yTile, propertyName, layerName, ref tile_property))
					{
						return tile_property;
					}
				}
			}
			return base.doesTileHaveProperty(xTile, yTile, propertyName, layerName);
		}

		public override string doesTileHavePropertyNoNull(int xTile, int yTile, string propertyName, string layerName)
		{
			foreach (Building building in buildings)
			{
				int additional_radius = building.GetAdditionalTilePropertyRadius();
				if (xTile >= (int)building.tileX - additional_radius && xTile < (int)building.tileX + (int)building.tilesWide + additional_radius && yTile >= (int)building.tileY - additional_radius && yTile < (int)building.tileY + (int)building.tilesHigh + additional_radius)
				{
					string tile_property = null;
					if (building.doesTileHaveProperty(xTile, yTile, propertyName, layerName, ref tile_property))
					{
						if (tile_property == null)
						{
							return "";
						}
						return tile_property;
					}
				}
			}
			return base.doesTileHavePropertyNoNull(xTile, yTile, propertyName, layerName);
		}

		public virtual void pokeTileForConstruction(Vector2 tile)
		{
		}

		public bool isBuildable(Vector2 tileLocation)
		{
			if ((!Game1.player.getTileLocation().Equals(tileLocation) || !Game1.player.currentLocation.Equals(this)) && !isTileOccupiedForPlacement(tileLocation) && GetFurnitureAt(tileLocation) == null && isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) && doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "NoFurniture", "Back") == null && !caveNoBuildRect.Contains(Utility.Vector2ToPoint(tileLocation)) && !shippingAreaNoBuildRect.Contains(Utility.Vector2ToPoint(tileLocation)))
			{
				if (Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").ToLower().Equals("t") || Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").ToLower().Equals("true"))
				{
					return true;
				}
				if (Game1.currentLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") != null && !Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").ToLower().Equals("f"))
				{
					return true;
				}
			}
			return false;
		}

		public bool isPath(Vector2 tileLocation)
		{
			Object obj = null;
			TerrainFeature tf = null;
			objects.TryGetValue(tileLocation, out obj);
			terrainFeatures.TryGetValue(tileLocation, out tf);
			if (tf != null && tf.isPassable())
			{
				return obj?.isPassable() ?? true;
			}
			return false;
		}

		public override Point getWarpPointTo(string location, Character character = null)
		{
			foreach (Building building in buildings)
			{
				if (building.indoors.Value != null && (building.indoors.Value.Name == location || (building.indoors.Value.uniqueName.Value != null && building.indoors.Value.uniqueName.Value == location)))
				{
					return building.getPointForHumanDoor();
				}
			}
			return base.getWarpPointTo(location, character);
		}

		public override Warp isCollidingWithDoors(Microsoft.Xna.Framework.Rectangle position, Character character = null)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 v = Utility.getCornersOfThisRectangle(ref position, i);
				Point rectangleCorner = new Point((int)v.X / 64, (int)v.Y / 64);
				foreach (Building building in buildings)
				{
					_ = (Point)building.humanDoor;
					if (building.indoors.Value != null && rectangleCorner.Equals(building.getPointForHumanDoor()))
					{
						return getWarpFromDoor(building.getPointForHumanDoor(), character);
					}
				}
			}
			return base.isCollidingWithDoors(position, character);
		}

		public override Warp getWarpFromDoor(Point door, Character character = null)
		{
			foreach (Building building in buildings)
			{
				_ = (Point)building.humanDoor;
				if (building.indoors.Value != null && door == building.getPointForHumanDoor())
				{
					return new Warp(door.X, door.Y, building.indoors.Value.uniqueName, building.indoors.Value.warps[0].X, building.indoors.Value.warps[0].Y - 1, flipFarmer: false);
				}
			}
			return base.getWarpFromDoor(door, character);
		}

		public bool buildStructure(BluePrint structureForPlacement, Vector2 tileLocation, Farmer who, bool magicalConstruction = false, bool skipSafetyChecks = false)
		{
			if (!skipSafetyChecks)
			{
				for (int y5 = 0; y5 < structureForPlacement.tilesHeight; y5++)
				{
					for (int x2 = 0; x2 < structureForPlacement.tilesWidth; x2++)
					{
						pokeTileForConstruction(new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y5));
					}
				}
				foreach (Point additionalPlacementTile in structureForPlacement.additionalPlacementTiles)
				{
					int x5 = additionalPlacementTile.X;
					int y4 = additionalPlacementTile.Y;
					pokeTileForConstruction(new Vector2(tileLocation.X + (float)x5, tileLocation.Y + (float)y4));
				}
				for (int y3 = 0; y3 < structureForPlacement.tilesHeight; y3++)
				{
					for (int x3 = 0; x3 < structureForPlacement.tilesWidth; x3++)
					{
						Vector2 currentGlobalTilePosition2 = new Vector2(tileLocation.X + (float)x3, tileLocation.Y + (float)y3);
						if (!isBuildable(currentGlobalTilePosition2))
						{
							return false;
						}
						foreach (Farmer farmer in farmers)
						{
							if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x3 * 64, y3 * 64, 64, 64)))
							{
								return false;
							}
						}
					}
				}
				foreach (Point additionalPlacementTile2 in structureForPlacement.additionalPlacementTiles)
				{
					int x4 = additionalPlacementTile2.X;
					int y2 = additionalPlacementTile2.Y;
					Vector2 currentGlobalTilePosition3 = new Vector2(tileLocation.X + (float)x4, tileLocation.Y + (float)y2);
					if (!isBuildable(currentGlobalTilePosition3))
					{
						return false;
					}
					foreach (Farmer farmer2 in farmers)
					{
						if (farmer2.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x4 * 64, y2 * 64, 64, 64)))
						{
							return false;
						}
					}
				}
				if (structureForPlacement.humanDoor != new Point(-1, -1))
				{
					Vector2 doorPos = tileLocation + new Vector2(structureForPlacement.humanDoor.X, structureForPlacement.humanDoor.Y + 1);
					if (!isBuildable(doorPos) && !isPath(doorPos))
					{
						return false;
					}
				}
			}
			Building b;
			switch (structureForPlacement.name)
			{
			case "Stable":
				b = new Stable(StardewValley.Util.GuidHelper.NewGuid(), structureForPlacement, tileLocation);
				break;
			case "Coop":
			case "Big Coop":
			case "Deluxe Coop":
				b = new Coop(structureForPlacement, tileLocation);
				break;
			case "Barn":
			case "Big Barn":
			case "Deluxe Barn":
				b = new Barn(structureForPlacement, tileLocation);
				break;
			case "Mill":
				b = new Mill(structureForPlacement, tileLocation);
				break;
			case "Junimo Hut":
				b = new JunimoHut(structureForPlacement, tileLocation);
				break;
			case "Shipping Bin":
				b = new ShippingBin(structureForPlacement, tileLocation);
				break;
			case "Fish Pond":
				b = new FishPond(structureForPlacement, tileLocation);
				break;
			case "Greenhouse":
				b = new GreenhouseBuilding(structureForPlacement, tileLocation);
				break;
			default:
				b = new Building(structureForPlacement, tileLocation);
				break;
			}
			b.owner.Value = who.UniqueMultiplayerID;
			if (!skipSafetyChecks)
			{
				string finalCheckResult = b.isThereAnythingtoPreventConstruction(this, tileLocation);
				if (finalCheckResult != null)
				{
					Game1.addHUDMessage(new HUDMessage(finalCheckResult, Color.Red, 3500f));
					return false;
				}
			}
			for (int y = 0; y < structureForPlacement.tilesHeight; y++)
			{
				for (int x = 0; x < structureForPlacement.tilesWidth; x++)
				{
					Vector2 currentGlobalTilePosition = new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y);
					terrainFeatures.Remove(currentGlobalTilePosition);
				}
			}
			buildings.Add(b);
			b.performActionOnConstruction(this);
			if (magicalConstruction)
			{
				Game1.multiplayer.globalChatInfoMessage("BuildingMagicBuild", Game1.player.Name, Utility.AOrAn(structureForPlacement.displayName), structureForPlacement.displayName, Game1.player.farmName);
			}
			else
			{
				Game1.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, Utility.AOrAn(structureForPlacement.displayName), structureForPlacement.displayName, Game1.player.farmName);
			}
			return true;
		}
	}
}
