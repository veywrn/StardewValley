using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System.IO;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	public class InteriorDoor : NetField<bool, InteriorDoor>
	{
		public GameLocation Location;

		public Point Position;

		public TemporaryAnimatedSprite Sprite;

		public Tile Tile;

		public InteriorDoor()
		{
		}

		public InteriorDoor(GameLocation location, Point position)
			: this()
		{
			Location = location;
			Position = position;
		}

		public override void Set(bool newValue)
		{
			if (newValue != value)
			{
				cleanSet(newValue);
				MarkDirty();
			}
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			bool newValue = reader.ReadBoolean();
			if (version.IsPriorityOver(ChangeVersion))
			{
				setInterpolationTarget(newValue);
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(targetValue);
		}

		public void ResetLocalState()
		{
			int x = Position.X;
			int y = Position.Y;
			Location doorLocation = new Location(x, y);
			Layer buildingsLayer = Location.Map.GetLayer("Buildings");
			Layer backLayer = Location.Map.GetLayer("Back");
			if (Tile == null)
			{
				Tile = buildingsLayer.Tiles[doorLocation];
			}
			if (Tile == null)
			{
				return;
			}
			if (Tile.Properties.TryGetValue("Action", out PropertyValue doorAction) && doorAction != null && doorAction.ToString().Contains("Door") && doorAction.ToString().Split(' ').Length > 1 && backLayer.Tiles[doorLocation] != null && !backLayer.Tiles[doorLocation].Properties.ContainsKey("TouchAction"))
			{
				backLayer.Tiles[doorLocation].Properties.Add("TouchAction", new PropertyValue("Door " + doorAction.ToString().Substring(doorAction.ToString().IndexOf(' ') + 1)));
			}
			Microsoft.Xna.Framework.Rectangle sourceRect = default(Microsoft.Xna.Framework.Rectangle);
			bool flip = false;
			switch (Tile.TileIndex)
			{
			case 824:
				sourceRect = new Microsoft.Xna.Framework.Rectangle(640, 144, 16, 48);
				break;
			case 825:
				sourceRect = new Microsoft.Xna.Framework.Rectangle(640, 144, 16, 48);
				flip = true;
				break;
			case 838:
				sourceRect = new Microsoft.Xna.Framework.Rectangle(576, 144, 16, 48);
				if (x == 10 && y == 5)
				{
					flip = true;
				}
				break;
			case 120:
				sourceRect = new Microsoft.Xna.Framework.Rectangle(512, 144, 16, 48);
				break;
			}
			Sprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 100f, 4, 1, new Vector2(x, y - 2) * 64f, flicker: false, flip, (float)((y + 1) * 64 - 12) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				holdLastFrame = true,
				paused = true
			};
			if (base.Value)
			{
				Sprite.paused = false;
				Sprite.resetEnd();
			}
		}

		public virtual void ApplyMapModifications()
		{
			if (base.Value)
			{
				openDoorTiles();
			}
			else
			{
				closeDoorTiles();
			}
		}

		public void CleanUpLocalState()
		{
			closeDoorTiles();
		}

		private void closeDoorSprite()
		{
			Sprite.reset();
			Sprite.paused = true;
		}

		private void openDoorSprite()
		{
			Sprite.paused = false;
		}

		private void openDoorTiles()
		{
			Location.setTileProperty(Position.X, Position.Y, "Back", "TemporaryBarrier", "T");
			Location.removeTile(Position.X, Position.Y, "Buildings");
			DelayedAction.functionAfterDelay(delegate
			{
				Location.removeTileProperty(Position.X, Position.Y, "Back", "TemporaryBarrier");
			}, 400);
			Location.removeTile(Position.X, Position.Y - 1, "Front");
			Location.removeTile(Position.X, Position.Y - 2, "Front");
		}

		private void closeDoorTiles()
		{
			Location doorLocation = new Location(Position.X, Position.Y);
			Map map = Location.Map;
			if (map != null && Tile != null)
			{
				map.GetLayer("Buildings").Tiles[doorLocation] = Tile;
				Location.removeTileProperty(Position.X, Position.Y, "Back", "TemporaryBarrier");
				doorLocation.Y--;
				map.GetLayer("Front").Tiles[doorLocation] = new StaticTile(map.GetLayer("Front"), Tile.TileSheet, BlendMode.Alpha, Tile.TileIndex - Tile.TileSheet.SheetWidth);
				doorLocation.Y--;
				map.GetLayer("Front").Tiles[doorLocation] = new StaticTile(map.GetLayer("Front"), Tile.TileSheet, BlendMode.Alpha, Tile.TileIndex - Tile.TileSheet.SheetWidth * 2);
			}
		}

		public void Update(GameTime time)
		{
			if (Sprite != null)
			{
				if (base.Value && Sprite.paused)
				{
					openDoorSprite();
					openDoorTiles();
				}
				else if (!base.Value && !Sprite.paused)
				{
					closeDoorSprite();
					closeDoorTiles();
				}
				Sprite.update(time);
			}
		}

		public void Draw(SpriteBatch b)
		{
			if (Sprite != null)
			{
				Sprite.draw(b);
			}
		}
	}
}
