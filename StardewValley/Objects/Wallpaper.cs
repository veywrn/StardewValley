using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class Wallpaper : Object
	{
		public static Texture2D wallpaperTexture;

		[XmlElement("sourceRect")]
		public readonly NetRectangle sourceRect = new NetRectangle();

		[XmlElement("isFloor")]
		public readonly NetBool isFloor = new NetBool(value: false);

		private static readonly Rectangle wallpaperContainerRect = new Rectangle(39, 31, 16, 16);

		private static readonly Rectangle floorContainerRect = new Rectangle(55, 31, 16, 16);

		public override string Name => base.name;

		public Wallpaper()
		{
			base.NetFields.AddFields(sourceRect, isFloor);
		}

		public Wallpaper(int which, bool isFloor = false)
			: this()
		{
			if (wallpaperTexture == null)
			{
				wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");
			}
			this.isFloor.Value = isFloor;
			base.ParentSheetIndex = which;
			base.name = (isFloor ? "Flooring" : "Wallpaper");
			sourceRect.Value = (isFloor ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28));
			price.Value = 100;
		}

		protected override string loadDisplayName()
		{
			if (!isFloor)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13204");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13203");
		}

		public override string getDescription()
		{
			if (!isFloor)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13206");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13205");
		}

		public override bool performDropDownAction(Farmer who)
		{
			return true;
		}

		public override bool performObjectDropInAction(Item dropIn, bool probe, Farmer who)
		{
			return false;
		}

		public override bool canBePlacedHere(GameLocation l, Vector2 tile)
		{
			Vector2 nonTile = tile * 64f;
			nonTile.X += 32f;
			nonTile.Y += 32f;
			foreach (Furniture f in l.furniture)
			{
				if ((int)f.furniture_type != 12 && f.getBoundingBox(f.tileLocation).Contains((int)nonTile.X, (int)nonTile.Y))
				{
					return false;
				}
			}
			return true;
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			if (who.currentLocation is DecoratableLocation)
			{
				Point tile = new Point(x / 64, y / 64);
				DecoratableLocation farmHouse = who.currentLocation as DecoratableLocation;
				if ((bool)isFloor)
				{
					List<Rectangle> floors = farmHouse.getFloors();
					for (int j = 0; j < floors.Count; j++)
					{
						if (floors[j].Contains(tile))
						{
							farmHouse.setFloor(parentSheetIndex, j, persist: true);
							location.playSound("coin");
							return true;
						}
					}
				}
				else
				{
					List<Rectangle> walls = farmHouse.getWalls();
					for (int i = 0; i < walls.Count; i++)
					{
						Rectangle wall = walls[i];
						if (wall.Height == 2)
						{
							wall.Height = 3;
						}
						if (wall.Contains(tile))
						{
							farmHouse.setWallpaper(parentSheetIndex, i, persist: true);
							location.playSound("coin");
							return true;
						}
					}
				}
			}
			return false;
		}

		public override bool isPlaceable()
		{
			return true;
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return boundingBox;
		}

		public override int salePrice()
		{
			return price;
		}

		public override int maximumStackSize()
		{
			return 1;
		}

		public override int addToStack(Item stack)
		{
			return 1;
		}

		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			drawInMenu(spriteBatch, objectPosition, 1f);
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if (wallpaperTexture == null)
			{
				wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");
			}
			if ((bool)isFloor)
			{
				spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(32f, 32f), floorContainerRect, color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 30f), sourceRect, color * transparency, 0f, new Vector2(14f, 13f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
			}
			else
			{
				spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(32f, 32f), wallpaperContainerRect, color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 32f), sourceRect, color * transparency, 0f, new Vector2(8f, 14f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
			}
		}

		public override Item getOne()
		{
			Wallpaper wallpaper = new Wallpaper(parentSheetIndex, isFloor);
			wallpaper._GetOneFrom(this);
			return wallpaper;
		}
	}
}
