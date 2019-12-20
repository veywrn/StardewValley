using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Locations
{
	public class DecoratableLocation : GameLocation
	{
		public readonly DecorationFacade wallPaper = new DecorationFacade();

		public readonly DecorationFacade floor = new DecorationFacade();

		public readonly NetCollection<Furniture> furniture = new NetCollection<Furniture>();

		protected readonly NetMutexQueue<Guid> furnitureToRemove = new NetMutexQueue<Guid>();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(wallPaper.Field, floor.Field, furniture, furnitureToRemove.NetFields);
			wallPaper.OnChange += delegate(int key, int value)
			{
				doSetVisibleWallpaper(key, value);
			};
			floor.OnChange += delegate(int key, int value)
			{
				doSetVisibleFloor(key, value);
			};
			furniture.InterpolationWait = false;
			furnitureToRemove.Processor = removeQueuedFurniture;
		}

		public DecoratableLocation()
		{
		}

		public DecoratableLocation(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			foreach (Furniture f in furniture)
			{
				if ((int)f.furniture_type != 12 && f.getBoundingBox(f.tileLocation).Intersects(position) && (!isFarmer || !f.getBoundingBox(f.tileLocation).Intersects(Game1.player.GetBoundingBox())))
				{
					return true;
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding);
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if (character == null || character.willDestroyObjectsUnderfoot)
			{
				foreach (Furniture f in furniture)
				{
					if ((int)f.furniture_type != 12 && f.getBoundingBox(f.tileLocation).Intersects(position))
					{
						return true;
					}
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override bool isTileLocationTotallyClearAndPlaceable(Vector2 v)
		{
			Vector2 nonTile = v * 64f;
			nonTile.X += 32f;
			nonTile.Y += 32f;
			foreach (Furniture f in furniture)
			{
				if ((int)f.furniture_type != 12 && !f.isPassable() && f.getBoundingBox(f.tileLocation).Contains((int)nonTile.X, (int)nonTile.Y))
				{
					return false;
				}
			}
			return base.isTileLocationTotallyClearAndPlaceable(v);
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			foreach (Furniture item in furniture)
			{
				item.minutesElapsed(10, this);
			}
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			foreach (Furniture item in furniture)
			{
				item.minutesElapsed(Utility.CalculateMinutesUntilMorning(Game1.timeOfDay), this);
				item.DayUpdate(this);
			}
		}

		public override bool LowPriorityLeftClick(int x, int y, Farmer who)
		{
			if (Game1.activeClickableMenu != null)
			{
				return false;
			}
			foreach (Furniture furnitureItem2 in furniture)
			{
				if (!furnitureItem2.isPassable() && furnitureItem2.boundingBox.Value.Contains(x, y) && furnitureItem2.canBeRemoved(who))
				{
					Guid guid3 = furniture.GuidOf(furnitureItem2);
					if (!furnitureToRemove.Contains(guid3))
					{
						furnitureToRemove.Add(guid3);
					}
					return true;
				}
				if (furnitureItem2.boundingBox.Value.Contains(x, y) && furnitureItem2.heldObject.Value != null)
				{
					furnitureItem2.clicked(who);
					return true;
				}
				if (!furnitureItem2.isGroundFurniture() && furnitureItem2.canBeRemoved(who))
				{
					int wall_y = y;
					foreach (Microsoft.Xna.Framework.Rectangle wall in getWalls())
					{
						if (wall.Contains(x / 64, y / 64))
						{
							wall_y = wall.Top * 64;
							break;
						}
					}
					if (furnitureItem2.boundingBox.Value.Contains(x, wall_y))
					{
						Guid guid2 = furniture.GuidOf(furnitureItem2);
						if (!furnitureToRemove.Contains(guid2))
						{
							furnitureToRemove.Add(guid2);
						}
						return true;
					}
				}
			}
			foreach (Furniture furnitureItem in furniture)
			{
				if (furnitureItem.isPassable() && furnitureItem.boundingBox.Value.Contains(x, y) && furnitureItem.canBeRemoved(who))
				{
					Guid guid = furniture.GuidOf(furnitureItem);
					if (!furnitureToRemove.Contains(guid))
					{
						furnitureToRemove.Add(guid);
					}
					return true;
				}
			}
			return base.LowPriorityLeftClick(x, y, who);
		}

		protected virtual void removeQueuedFurniture(Guid guid)
		{
			Farmer who = Game1.player;
			if (!furniture.ContainsGuid(guid))
			{
				return;
			}
			Furniture furnitureItem = furniture[guid];
			if (!who.couldInventoryAcceptThisItem(furnitureItem))
			{
				return;
			}
			furnitureItem.performRemoveAction(furnitureItem.tileLocation, this);
			furniture.Remove(guid);
			bool foundInToolbar = false;
			for (int i = 0; i < 12; i++)
			{
				if (who.items[i] == null)
				{
					who.items[i] = furnitureItem;
					who.CurrentToolIndex = i;
					foundInToolbar = true;
					break;
				}
			}
			if (!foundInToolbar)
			{
				Item item = who.addItemToInventory(furnitureItem, 11);
				who.addItemToInventory(item);
				who.CurrentToolIndex = 11;
			}
			localSound("coin");
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (base.checkAction(tileLocation, viewport, who))
			{
				return true;
			}
			Point vect = new Point(tileLocation.X * 64, tileLocation.Y * 64);
			Point vectOnWall = new Point(tileLocation.X * 64, (tileLocation.Y - 1) * 64);
			bool didRightClick = Game1.didPlayerJustRightClick();
			foreach (Furniture f in furniture)
			{
				if (f.boundingBox.Value.Contains(vect) && (int)f.furniture_type != 12)
				{
					if (didRightClick)
					{
						if (who.ActiveObject != null && f.performObjectDropInAction(who.ActiveObject, probe: false, who))
						{
							return true;
						}
						return f.checkForAction(who);
					}
					return f.clicked(who);
				}
				if ((int)f.furniture_type == 6 && f.boundingBox.Value.Contains(vectOnWall))
				{
					if (didRightClick)
					{
						if (who.ActiveObject != null && f.performObjectDropInAction(who.ActiveObject, probe: false, who))
						{
							return true;
						}
						return f.checkForAction(who);
					}
					return f.clicked(who);
				}
			}
			return false;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (!wasUpdated)
			{
				wallPaper.Update();
				floor.Update();
				furnitureToRemove.Update(this);
				foreach (Furniture item in furniture)
				{
					item.updateWhenCurrentLocation(time, this);
				}
				base.UpdateWhenCurrentLocation(time);
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			furnitureToRemove.Clear();
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!Game1.player.mailReceived.Contains("button_tut_1"))
			{
				Game1.player.mailReceived.Add("button_tut_1");
				Game1.onScreenMenus.Add(new ButtonTutorialMenu(0));
			}
			if (!(this is FarmHouse))
			{
				setWallpapers();
				setFloors();
			}
			if (getTileIndexAt(Game1.player.getTileX(), Game1.player.getTileY(), "Buildings") != -1)
			{
				Game1.player.position.Y += 64f;
			}
			foreach (Furniture item in furniture)
			{
				item.resetOnPlayerEntry(this);
			}
		}

		public override void shiftObjects(int dx, int dy)
		{
			base.shiftObjects(dx, dy);
			foreach (Furniture item in furniture)
			{
				item.tileLocation.X += dx;
				item.tileLocation.Y += dy;
				item.boundingBox.X += dx * 64;
				item.boundingBox.Y += dy * 64;
				item.updateDrawPosition();
			}
			List<KeyValuePair<Vector2, TerrainFeature>> list = new List<KeyValuePair<Vector2, TerrainFeature>>(terrainFeatures.Pairs);
			terrainFeatures.Clear();
			foreach (KeyValuePair<Vector2, TerrainFeature> v in list)
			{
				terrainFeatures.Add(new Vector2(v.Key.X + (float)dx, v.Key.Y + (float)dy), v.Value);
			}
		}

		public override bool isObjectAt(int x, int y)
		{
			foreach (Furniture item in furniture)
			{
				if (item.boundingBox.Value.Contains(x, y))
				{
					return true;
				}
			}
			return base.isObjectAt(x, y);
		}

		public override Object getObjectAt(int x, int y)
		{
			foreach (Furniture f in furniture)
			{
				if (f.boundingBox.Value.Contains(x, y))
				{
					return f;
				}
			}
			return base.getObjectAt(x, y);
		}

		public void moveFurniture(int oldX, int oldY, int newX, int newY)
		{
			Vector2 oldSpot = new Vector2(oldX, oldY);
			foreach (Furniture f in furniture)
			{
				if (f.tileLocation.Equals(oldSpot))
				{
					f.tileLocation.Value = new Vector2(newX, newY);
					f.boundingBox.X = newX * 64;
					f.boundingBox.Y = newY * 64;
					f.updateDrawPosition();
					return;
				}
			}
			if (objects.ContainsKey(oldSpot))
			{
				Object o = objects[oldSpot];
				objects.Remove(oldSpot);
				o.tileLocation.Value = new Vector2(newX, newY);
				objects.Add(new Vector2(newX, newY), o);
			}
		}

		public virtual bool isTileOnWall(int x, int y)
		{
			foreach (Microsoft.Xna.Framework.Rectangle wall in getWalls())
			{
				if (wall.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		public virtual List<Microsoft.Xna.Framework.Rectangle> getWalls()
		{
			return new List<Microsoft.Xna.Framework.Rectangle>
			{
				new Microsoft.Xna.Framework.Rectangle(1, 1, 11, 3)
			};
		}

		public virtual void setFloors()
		{
			for (int i = 0; i < floor.Count; i++)
			{
				setFloor(floor[i], i, persist: true);
				doSetVisibleFloor(i, floor[i]);
			}
		}

		public virtual void setWallpapers()
		{
			for (int i = 0; i < wallPaper.Count; i++)
			{
				setWallpaper(wallPaper[i], i, persist: true);
				doSetVisibleWallpaper(i, wallPaper[i]);
			}
		}

		public void setWallpaper(int which, int whichRoom = -1, bool persist = false)
		{
			List<Microsoft.Xna.Framework.Rectangle> rooms = getWalls();
			if (!persist)
			{
				return;
			}
			wallPaper.SetCountAtLeast(rooms.Count);
			if (whichRoom == -1)
			{
				for (int i = 0; i < wallPaper.Count; i++)
				{
					wallPaper[i] = which;
				}
			}
			else if (whichRoom <= wallPaper.Count - 1)
			{
				wallPaper[whichRoom] = which;
			}
		}

		protected bool IsFloorableTile(int x, int y, string layer_name)
		{
			int tile_index = getTileIndexAt(x, y, "Buildings");
			if (tile_index >= 197 && tile_index <= 199 && getTileSheetIDAt(x, y, "Buildings") == "untitled tile sheet")
			{
				return false;
			}
			return IsFloorableOrWallpaperableTile(x, y, layer_name);
		}

		protected bool IsFloorableOrWallpaperableTile(int x, int y, string layer_name)
		{
			Layer layer = map.GetLayer(layer_name);
			if (layer != null && x < layer.LayerWidth && y < layer.LayerHeight && layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet != null && layer.Tiles[x, y].TileSheet.Id == "walls_and_floors")
			{
				return true;
			}
			return false;
		}

		protected virtual void doSetVisibleWallpaper(int whichRoom, int which)
		{
			updateMap();
			List<Microsoft.Xna.Framework.Rectangle> rooms = getWalls();
			int tileSheetIndex = which % 16 + which / 16 * 48;
			if (whichRoom == -1)
			{
				foreach (Microsoft.Xna.Framework.Rectangle r2 in rooms)
				{
					for (int x2 = r2.X; x2 < r2.Right; x2++)
					{
						if (IsFloorableOrWallpaperableTile(x2, r2.Y, "Back"))
						{
							setMapTileIndex(x2, r2.Y, tileSheetIndex, "Back");
						}
						if (IsFloorableOrWallpaperableTile(x2, r2.Y + 1, "Back"))
						{
							setMapTileIndex(x2, r2.Y + 1, tileSheetIndex + 16, "Back");
						}
						if (r2.Height >= 3)
						{
							if (IsFloorableOrWallpaperableTile(x2, r2.Y + 2, "Buildings"))
							{
								setMapTileIndex(x2, r2.Y + 2, tileSheetIndex + 32, "Buildings");
							}
							if (IsFloorableOrWallpaperableTile(x2, r2.Y + 2, "Back"))
							{
								setMapTileIndex(x2, r2.Y + 2, tileSheetIndex + 32, "Back");
							}
						}
					}
				}
			}
			else
			{
				if (rooms.Count <= whichRoom)
				{
					return;
				}
				Microsoft.Xna.Framework.Rectangle r = rooms[whichRoom];
				for (int x = r.X; x < r.Right; x++)
				{
					if (IsFloorableOrWallpaperableTile(x, r.Y, "Back"))
					{
						setMapTileIndex(x, r.Y, tileSheetIndex, "Back");
					}
					if (IsFloorableOrWallpaperableTile(x, r.Y + 1, "Back"))
					{
						setMapTileIndex(x, r.Y + 1, tileSheetIndex + 16, "Back");
					}
					if (r.Height >= 3)
					{
						if (IsFloorableOrWallpaperableTile(x, r.Y + 2, "Buildings"))
						{
							setMapTileIndex(x, r.Y + 2, tileSheetIndex + 32, "Buildings");
						}
						else if (IsFloorableOrWallpaperableTile(x, r.Y + 2, "Back"))
						{
							setMapTileIndex(x, r.Y + 2, tileSheetIndex + 32, "Back");
						}
					}
				}
			}
		}

		public override bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			return getTileIndexAt((int)p.X, (int)p.Y, "Front") == -1;
		}

		public virtual int getFloorAt(Point p)
		{
			List<Microsoft.Xna.Framework.Rectangle> floors = getFloors();
			for (int i = 0; i < floors.Count; i++)
			{
				if (floors[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public Furniture getRandomFurniture(Random r)
		{
			if (furniture.Count > 0)
			{
				return furniture.ElementAt(r.Next(furniture.Count));
			}
			return null;
		}

		public virtual int getWallForRoomAt(Point p)
		{
			List<Microsoft.Xna.Framework.Rectangle> walls = getWalls();
			for (int y = 0; y < 16; y++)
			{
				for (int i = 0; i < walls.Count; i++)
				{
					if (walls[i].Contains(p))
					{
						return i;
					}
				}
				p.Y--;
			}
			return -1;
		}

		public virtual void setFloor(int which, int whichRoom = -1, bool persist = false)
		{
			List<Microsoft.Xna.Framework.Rectangle> rooms = getFloors();
			if (!persist)
			{
				return;
			}
			floor.SetCountAtLeast(rooms.Count);
			if (whichRoom == -1)
			{
				for (int i = 0; i < floor.Count; i++)
				{
					floor[i] = which;
				}
			}
			else if (whichRoom <= floor.Count - 1)
			{
				floor[whichRoom] = which;
			}
		}

		protected virtual void doSetVisibleFloor(int whichRoom, int which)
		{
			List<Microsoft.Xna.Framework.Rectangle> rooms = getFloors();
			int tileSheetIndex = 336 + which % 8 * 2 + which / 8 * 32;
			if (whichRoom == -1)
			{
				foreach (Microsoft.Xna.Framework.Rectangle r2 in rooms)
				{
					for (int x2 = r2.X; x2 < r2.Right; x2 += 2)
					{
						for (int y2 = r2.Y; y2 < r2.Bottom; y2 += 2)
						{
							if (r2.Contains(x2, y2) && IsFloorableTile(x2, y2, "Back"))
							{
								setMapTileIndex(x2, y2, tileSheetIndex, "Back");
							}
							if (r2.Contains(x2 + 1, y2) && IsFloorableTile(x2 + 1, y2, "Back"))
							{
								setMapTileIndex(x2 + 1, y2, tileSheetIndex + 1, "Back");
							}
							if (r2.Contains(x2, y2 + 1) && IsFloorableTile(x2, y2 + 1, "Back"))
							{
								setMapTileIndex(x2, y2 + 1, tileSheetIndex + 16, "Back");
							}
							if (r2.Contains(x2 + 1, y2 + 1) && IsFloorableTile(x2 + 1, y2 + 1, "Back"))
							{
								setMapTileIndex(x2 + 1, y2 + 1, tileSheetIndex + 17, "Back");
							}
						}
					}
				}
			}
			else
			{
				if (rooms.Count <= whichRoom)
				{
					return;
				}
				Microsoft.Xna.Framework.Rectangle r = rooms[whichRoom];
				for (int x = r.X; x < r.Right; x += 2)
				{
					for (int y = r.Y; y < r.Bottom; y += 2)
					{
						if (r.Contains(x, y) && IsFloorableTile(x, y, "Back"))
						{
							setMapTileIndex(x, y, tileSheetIndex, "Back");
						}
						if (r.Contains(x + 1, y) && IsFloorableTile(x + 1, y, "Back"))
						{
							setMapTileIndex(x + 1, y, tileSheetIndex + 1, "Back");
						}
						if (r.Contains(x, y + 1) && IsFloorableTile(x, y + 1, "Back"))
						{
							setMapTileIndex(x, y + 1, tileSheetIndex + 16, "Back");
						}
						if (r.Contains(x + 1, y + 1) && IsFloorableTile(x + 1, y + 1, "Back"))
						{
							setMapTileIndex(x + 1, y + 1, tileSheetIndex + 17, "Back");
						}
					}
				}
			}
		}

		public virtual List<Microsoft.Xna.Framework.Rectangle> getFloors()
		{
			return new List<Microsoft.Xna.Framework.Rectangle>
			{
				new Microsoft.Xna.Framework.Rectangle(1, 3, 11, 11)
			};
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Furniture.isDrawingLocationFurniture = true;
			foreach (Furniture item in furniture)
			{
				item.draw(b, -1, -1);
			}
			Furniture.isDrawingLocationFurniture = false;
		}
	}
}
