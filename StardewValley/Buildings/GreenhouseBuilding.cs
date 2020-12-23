using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Buildings
{
	public class GreenhouseBuilding : Building
	{
		protected Farm _farm;

		public GreenhouseBuilding(BluePrint b, Vector2 tileLocation)
			: base(b, tileLocation)
		{
		}

		protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
		{
			return null;
		}

		public GreenhouseBuilding()
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
		}

		public override void drawInMenu(SpriteBatch b, int x, int y)
		{
			Microsoft.Xna.Framework.Rectangle rectangle = getSourceRect();
			y += 336;
			int amount_to_trim = 22;
			rectangle.Height -= amount_to_trim;
			rectangle.Y += amount_to_trim / 2;
			b.Draw(texture.Value, new Vector2(x, y), rectangle, color, 0f, new Vector2(0f, rectangle.Height / 2), 4f, SpriteEffects.None, 0.89f);
		}

		public override Microsoft.Xna.Framework.Rectangle getSourceRect()
		{
			return new Microsoft.Xna.Framework.Rectangle(0, 160, 112, 160);
		}

		public override void Update(GameTime time)
		{
			base.Update(time);
		}

		public override void drawInConstruction(SpriteBatch b)
		{
			float draw_layer = (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f;
			Microsoft.Xna.Framework.Rectangle source_rect = getSourceRect();
			source_rect.Y += source_rect.Height;
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), source_rect, color.Value * alpha, 0f, new Vector2(0f, source_rect.Height), 4f, SpriteEffects.None, draw_layer);
		}

		public override void drawBackground(SpriteBatch b)
		{
			base.drawBackground(b);
			if (!base.isMoving)
			{
				DrawEntranceTiles(b);
				drawShadow(b);
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (base.isMoving)
			{
				return;
			}
			if ((int)daysOfConstructionLeft > 0 || (int)newConstructionTimer > 0)
			{
				drawInConstruction(b);
				return;
			}
			float draw_layer = (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f;
			Microsoft.Xna.Framework.Rectangle source_rect = getSourceRect();
			if (!GetFarm().greenhouseUnlocked.Value)
			{
				source_rect.Y -= source_rect.Height;
			}
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), source_rect, color.Value * alpha, 0f, new Vector2(0f, source_rect.Height), 4f, SpriteEffects.None, draw_layer);
		}

		public Farm GetFarm()
		{
			if (_farm == null)
			{
				_farm = Game1.getFarm();
			}
			return _farm;
		}

		public override string isThereAnythingtoPreventConstruction(GameLocation location, Vector2 tile_position)
		{
			return null;
		}

		public override bool doesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
		{
			if (base.isMoving)
			{
				return false;
			}
			if (tile_x == (int)tileX + humanDoor.X && tile_y == (int)tileY + humanDoor.Y && layer_name == "Buildings" && property_name == "Action")
			{
				property_value = "WarpGreenhouse";
				return true;
			}
			if ((tile_x >= (int)tileX - 1 && tile_x <= (int)tileX + (int)tilesWide - 1 && tile_y <= (int)tileY + (int)tilesHigh && tile_y >= (int)tileY) || (CanDrawEntranceTiles() && tile_x >= (int)tileX + 1 && tile_x <= (int)tileX + (int)tilesWide - 2 && tile_y == (int)tileY + (int)tilesHigh + 1))
			{
				if (CanDrawEntranceTiles() && tile_x >= (int)tileX + humanDoor.X - 1 && tile_x <= (int)tileX + humanDoor.X + 1 && tile_y <= (int)tileY + (int)tilesHigh + 1 && tile_y >= (int)tileY + humanDoor.Y + 1)
				{
					if (property_name == "Type" && layer_name == "Back")
					{
						property_value = "Stone";
						return true;
					}
					if (property_name == "NoSpawn" && layer_name == "Back")
					{
						property_value = "All";
						return true;
					}
					if (property_name == "Buildable" && layer_name == "Back")
					{
						property_value = null;
						return true;
					}
				}
				if (property_name == "Buildable" && layer_name == "Back")
				{
					property_value = "T";
					return true;
				}
				if (property_name == "NoSpawn" && layer_name == "Back")
				{
					property_value = "Tree";
					return true;
				}
				if (property_name == "Diggable" && layer_name == "Back")
				{
					property_value = null;
					return true;
				}
			}
			return base.doesTileHaveProperty(tile_x, tile_y, property_name, layer_name, ref property_value);
		}

		public override int GetAdditionalTilePropertyRadius()
		{
			return 2;
		}

		public virtual bool CanDrawEntranceTiles()
		{
			return true;
		}

		public virtual void DrawEntranceTiles(SpriteBatch b)
		{
			Map map = GetFarm().Map;
			Layer back_layer = map.GetLayer("Back");
			TileSheet tilesheet = map.TileSheets[1];
			if (Game1.whichFarm == 6)
			{
				tilesheet = map.TileSheets[2];
			}
			Vector2 vector_draw_position3 = Vector2.Zero;
			Location draw_location = new Location(0, 0);
			StaticTile tile2 = new StaticTile(back_layer, tilesheet, BlendMode.Alpha, 812);
			if (CanDrawEntranceTiles())
			{
				float draw_layer = 0f;
				vector_draw_position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX + humanDoor.Value.X - 1, (int)tileY + humanDoor.Value.Y + 1) * 64f);
				draw_location.X = (int)vector_draw_position3.X;
				draw_location.Y = (int)vector_draw_position3.Y;
				Game1.mapDisplayDevice.DrawTile(tile2, draw_location, draw_layer);
				draw_location.X += 64;
				Game1.mapDisplayDevice.DrawTile(tile2, draw_location, draw_layer);
				draw_location.X += 64;
				Game1.mapDisplayDevice.DrawTile(tile2, draw_location, draw_layer);
				tile2 = new StaticTile(back_layer, tilesheet, BlendMode.Alpha, 838);
				vector_draw_position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX + humanDoor.Value.X - 1, (int)tileY + humanDoor.Value.Y + 2) * 64f);
				draw_location.X = (int)vector_draw_position3.X;
				draw_location.Y = (int)vector_draw_position3.Y;
				Game1.mapDisplayDevice.DrawTile(tile2, draw_location, draw_layer);
				draw_location.X += 64;
				Game1.mapDisplayDevice.DrawTile(tile2, draw_location, draw_layer);
				draw_location.X += 64;
				Game1.mapDisplayDevice.DrawTile(tile2, draw_location, draw_layer);
			}
		}

		public override void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
		{
			Microsoft.Xna.Framework.Rectangle shadow_rectangle = new Microsoft.Xna.Framework.Rectangle(112, 0, 128, 144);
			if (CanDrawEntranceTiles())
			{
				shadow_rectangle.Y = 144;
			}
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(((int)tileX - 1) * 64, (int)tileY * 64)), shadow_rectangle, Color.White * ((localX == -1) ? ((float)alpha) : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}
	}
}
