using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;

namespace StardewValley.Locations
{
	public class DecoratableLocation : GameLocation
	{
		public readonly DecorationFacade wallPaper = new DecorationFacade();

		public readonly DecorationFacade floor = new DecorationFacade();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(wallPaper.Field, floor.Field);
			wallPaper.OnChange += delegate(int key, int value)
			{
				doSetVisibleWallpaper(key, value);
			};
			floor.OnChange += delegate(int key, int value)
			{
				doSetVisibleFloor(key, value);
			};
		}

		public DecoratableLocation()
		{
		}

		public DecoratableLocation(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (!wasUpdated)
			{
				wallPaper.Update();
				floor.Update();
				base.UpdateWhenCurrentLocation(time);
			}
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
		}

		public override void shiftObjects(int dx, int dy)
		{
			base.shiftObjects(dx, dy);
			foreach (Furniture v2 in furniture)
			{
				v2.removeLights(this);
				v2.tileLocation.X += dx;
				v2.tileLocation.Y += dy;
				v2.boundingBox.X += dx * 64;
				v2.boundingBox.Y += dy * 64;
				v2.updateDrawPosition();
				if (Game1.isDarkOut())
				{
					v2.addLights(this);
				}
			}
			List<KeyValuePair<Vector2, TerrainFeature>> list = new List<KeyValuePair<Vector2, TerrainFeature>>(terrainFeatures.Pairs);
			terrainFeatures.Clear();
			foreach (KeyValuePair<Vector2, TerrainFeature> v in list)
			{
				terrainFeatures.Add(new Vector2(v.Key.X + (float)dx, v.Key.Y + (float)dy), v.Value);
			}
		}

		public void moveFurniture(int oldX, int oldY, int newX, int newY)
		{
			Vector2 oldSpot = new Vector2(oldX, oldY);
			foreach (Furniture f in furniture)
			{
				if (f.tileLocation.Equals(oldSpot))
				{
					f.removeLights(this);
					f.tileLocation.Value = new Vector2(newX, newY);
					f.boundingBox.X = newX * 64;
					f.boundingBox.Y = newY * 64;
					f.updateDrawPosition();
					if (Game1.isDarkOut())
					{
						f.addLights(this);
					}
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

		public override bool CanFreePlaceFurniture()
		{
			return true;
		}

		public virtual bool isTileOnWall(int x, int y)
		{
			foreach (Rectangle wall in getWalls())
			{
				if (wall.Contains(x, y))
				{
					return true;
				}
			}
			return false;
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
			List<Rectangle> rooms = getWalls();
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
			List<Rectangle> rooms = getWalls();
			int tileSheetIndex = which % 16 + which / 16 * 48;
			if (whichRoom == -1)
			{
				foreach (Rectangle r2 in rooms)
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
				Rectangle r = rooms[whichRoom];
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

		public override void drawFloorDecorations(SpriteBatch b)
		{
			base.drawFloorDecorations(b);
		}

		public virtual int getFloorAt(Point p)
		{
			List<Rectangle> floors = getFloors();
			for (int i = 0; i < floors.Count; i++)
			{
				if (floors[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			DecoratableLocation decoratable_location = l as DecoratableLocation;
			if (decoratable_location != null)
			{
				wallPaper.Set(decoratable_location.wallPaper);
				floor.Set(decoratable_location.floor);
			}
			setWallpapers();
			setFloors();
			base.TransferDataFromSavedLocation(l);
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
			List<Rectangle> walls = getWalls();
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
			List<Rectangle> rooms = getFloors();
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

		public virtual int GetFlooringIndex(int base_tile_sheet, int tile_x, int tile_y)
		{
			int replaced_tile_index2 = getTileIndexAt(tile_x, tile_y, "Back");
			if (replaced_tile_index2 < 0)
			{
				return 0;
			}
			replaced_tile_index2 -= 336;
			int x_offset = replaced_tile_index2 % 2;
			int y_offset = replaced_tile_index2 % 32 / 16;
			return base_tile_sheet + x_offset + 16 * y_offset;
		}

		protected virtual void doSetVisibleFloor(int whichRoom, int which)
		{
			List<Rectangle> rooms = getFloors();
			int tileSheetIndex = 336 + which % 8 * 2 + which / 8 * 32;
			if (whichRoom == -1)
			{
				foreach (Rectangle r2 in rooms)
				{
					for (int x2 = r2.X; x2 < r2.Right; x2 += 2)
					{
						for (int y2 = r2.Y; y2 < r2.Bottom; y2 += 2)
						{
							if (r2.Contains(x2, y2) && IsFloorableTile(x2, y2, "Back"))
							{
								setMapTileIndex(x2, y2, GetFlooringIndex(tileSheetIndex, x2, y2), "Back");
							}
							if (r2.Contains(x2 + 1, y2) && IsFloorableTile(x2 + 1, y2, "Back"))
							{
								setMapTileIndex(x2 + 1, y2, GetFlooringIndex(tileSheetIndex, x2 + 1, y2), "Back");
							}
							if (r2.Contains(x2, y2 + 1) && IsFloorableTile(x2, y2 + 1, "Back"))
							{
								setMapTileIndex(x2, y2 + 1, GetFlooringIndex(tileSheetIndex, x2, y2 + 1), "Back");
							}
							if (r2.Contains(x2 + 1, y2 + 1) && IsFloorableTile(x2 + 1, y2 + 1, "Back"))
							{
								setMapTileIndex(x2 + 1, y2 + 1, GetFlooringIndex(tileSheetIndex, x2 + 1, y2 + 1), "Back");
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
				Rectangle r = rooms[whichRoom];
				for (int x = r.X; x < r.Right; x += 2)
				{
					for (int y = r.Y; y < r.Bottom; y += 2)
					{
						if (r.Contains(x, y) && IsFloorableTile(x, y, "Back"))
						{
							setMapTileIndex(x, y, GetFlooringIndex(tileSheetIndex, x, y), "Back");
						}
						if (r.Contains(x + 1, y) && IsFloorableTile(x + 1, y, "Back"))
						{
							setMapTileIndex(x + 1, y, GetFlooringIndex(tileSheetIndex, x + 1, y), "Back");
						}
						if (r.Contains(x, y + 1) && IsFloorableTile(x, y + 1, "Back"))
						{
							setMapTileIndex(x, y + 1, GetFlooringIndex(tileSheetIndex, x, y + 1), "Back");
						}
						if (r.Contains(x + 1, y + 1) && IsFloorableTile(x + 1, y + 1, "Back"))
						{
							setMapTileIndex(x + 1, y + 1, GetFlooringIndex(tileSheetIndex, x + 1, y + 1), "Back");
						}
					}
				}
			}
		}

		public virtual List<Rectangle> getFloors()
		{
			return new List<Rectangle>();
		}
	}
}
