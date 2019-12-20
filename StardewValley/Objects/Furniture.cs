using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class Furniture : Object
	{
		public const int chair = 0;

		public const int bench = 1;

		public const int couch = 2;

		public const int armchair = 3;

		public const int dresser = 4;

		public const int longTable = 5;

		public const int painting = 6;

		public const int lamp = 7;

		public const int decor = 8;

		public const int other = 9;

		public const int bookcase = 10;

		public const int table = 11;

		public const int rug = 12;

		public const int window = 13;

		public const int fireplace = 14;

		public const string furnitureTextureName = "TileSheets\\furniture";

		[XmlIgnore]
		public static Texture2D furnitureTexture;

		[XmlElement("furniture_type")]
		public readonly NetInt furniture_type = new NetInt();

		[XmlElement("rotations")]
		public readonly NetInt rotations = new NetInt();

		[XmlElement("currentRotation")]
		public readonly NetInt currentRotation = new NetInt();

		[XmlElement("sourceIndexOffset")]
		private readonly NetInt sourceIndexOffset = new NetInt();

		[XmlElement("drawPosition")]
		protected readonly NetVector2 drawPosition = new NetVector2();

		[XmlElement("sourceRect")]
		public readonly NetRectangle sourceRect = new NetRectangle();

		[XmlElement("defaultSourceRect")]
		public readonly NetRectangle defaultSourceRect = new NetRectangle();

		[XmlElement("defaultBoundingBox")]
		public readonly NetRectangle defaultBoundingBox = new NetRectangle();

		[XmlIgnore]
		public bool flaggedForPickUp;

		[XmlElement("drawHeldObjectLow")]
		public readonly NetBool drawHeldObjectLow = new NetBool();

		public static bool isDrawingLocationFurniture;

		[XmlIgnore]
		private string _description;

		private const int fireIDBase = 944469;

		private bool lightGlowAdded;

		[XmlIgnore]
		public string description
		{
			get
			{
				if (_description == null)
				{
					_description = loadDescription();
				}
				return _description;
			}
		}

		public override string Name => base.name;

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(furniture_type, rotations, currentRotation, sourceIndexOffset, drawPosition, sourceRect, defaultSourceRect, defaultBoundingBox, drawHeldObjectLow);
		}

		public Furniture()
		{
			updateDrawPosition();
			isOn.Value = false;
		}

		public Furniture(int which, Vector2 tile, int initialRotations)
			: this(which, tile)
		{
			for (int i = 0; i < initialRotations; i++)
			{
				rotate();
			}
			isOn.Value = false;
		}

		public Furniture(int which, Vector2 tile)
		{
			tileLocation.Value = tile;
			isOn.Value = false;
			base.ParentSheetIndex = which;
			string[] data = getData();
			base.name = data[0];
			furniture_type.Value = getTypeNumberFromName(data[1]);
			defaultSourceRect.Value = new Rectangle(which * 16 % furnitureTexture.Width, which * 16 / furnitureTexture.Width * 16, 1, 1);
			drawHeldObjectLow.Value = Name.ToLower().Contains("tea");
			if (data[2].Equals("-1"))
			{
				sourceRect.Value = getDefaultSourceRectForType(which, furniture_type);
				defaultSourceRect.Value = sourceRect.Value;
			}
			else
			{
				defaultSourceRect.Width = Convert.ToInt32(data[2].Split(' ')[0]);
				defaultSourceRect.Height = Convert.ToInt32(data[2].Split(' ')[1]);
				sourceRect.Value = new Rectangle(which * 16 % furnitureTexture.Width, which * 16 / furnitureTexture.Width * 16, defaultSourceRect.Width * 16, defaultSourceRect.Height * 16);
				defaultSourceRect.Value = sourceRect.Value;
			}
			defaultBoundingBox.Value = new Rectangle((int)tileLocation.X, (int)tileLocation.Y, 1, 1);
			if (data[3].Equals("-1"))
			{
				boundingBox.Value = getDefaultBoundingBoxForType(furniture_type);
				defaultBoundingBox.Value = boundingBox;
			}
			else
			{
				defaultBoundingBox.Width = Convert.ToInt32(data[3].Split(' ')[0]);
				defaultBoundingBox.Height = Convert.ToInt32(data[3].Split(' ')[1]);
				boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, defaultBoundingBox.Width * 64, defaultBoundingBox.Height * 64);
				defaultBoundingBox.Value = boundingBox;
			}
			updateDrawPosition();
			rotations.Value = Convert.ToInt32(data[4]);
			price.Value = Convert.ToInt32(data[5]);
		}

		private string[] getData()
		{
			Dictionary<int, string> dataSheet = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
			if (!dataSheet.ContainsKey(parentSheetIndex))
			{
				dataSheet = Game1.content.LoadBase<Dictionary<int, string>>("Data\\Furniture");
			}
			return dataSheet[parentSheetIndex].Split('/');
		}

		protected override string loadDisplayName()
		{
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				string[] data = getData();
				return data[data.Length - 1];
			}
			return base.name;
		}

		protected string loadDescription()
		{
			if ((int)parentSheetIndex == 1308)
			{
				return Game1.parseText(Game1.content.LoadString("Strings\\Objects:CatalogueDescription"), Game1.smallFont, 320);
			}
			if ((int)parentSheetIndex == 1226)
			{
				return Game1.parseText(Game1.content.LoadString("Strings\\Objects:FurnitureCatalogueDescription"), Game1.smallFont, 320);
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12623");
		}

		private void specialVariableChange(bool newValue)
		{
			if ((int)furniture_type == 14 && newValue)
			{
				Game1.playSound("fireball");
			}
		}

		public override string getDescription()
		{
			return Game1.parseText(description, Game1.smallFont, getDescriptionWidth());
		}

		public override bool performDropDownAction(Farmer who)
		{
			resetOnPlayerEntry((who == null) ? Game1.currentLocation : who.currentLocation, dropDown: true);
			return false;
		}

		public override void hoverAction()
		{
			base.hoverAction();
			if (!Game1.player.isInventoryFull())
			{
				Game1.mouseCursor = 2;
			}
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			switch ((int)parentSheetIndex)
			{
			case 1402:
				Game1.activeClickableMenu = new Billboard();
				break;
			case 1308:
				Game1.activeClickableMenu = new ShopMenu(Utility.getAllWallpapersAndFloorsForFree(), 0, null, null, null, "Catalogue");
				break;
			case 1226:
				Game1.activeClickableMenu = new ShopMenu(Utility.getAllFurnituresForFree(), 0, null, null, null, "Furniture Catalogue");
				break;
			case 1309:
				Game1.playSound("openBox");
				shakeTimer = 500;
				if (Game1.getMusicTrackName().Equals("sam_acoustic1"))
				{
					Game1.changeMusicTrack("none", track_interruptable: true);
				}
				else
				{
					Game1.changeMusicTrack("sam_acoustic1");
				}
				break;
			}
			if ((int)furniture_type == 14)
			{
				isOn.Value = !isOn.Value;
				initializeLightSource(tileLocation);
				setFireplace(who.currentLocation, playSound: true, broadcast: true);
				return true;
			}
			return clicked(who);
		}

		public void setFireplace(GameLocation location, bool playSound = true, bool broadcast = false)
		{
			_ = tileLocation.X;
			_ = tileLocation.Y;
			if ((bool)isOn)
			{
				if (base.lightSource == null)
				{
					initializeLightSource(tileLocation);
				}
				if (base.lightSource != null && (bool)isOn && !location.hasLightSource(base.lightSource.Identifier))
				{
					location.sharedLights[base.lightSource.identifier] = base.lightSource.Clone();
				}
				if (playSound)
				{
					location.localSound("fireball");
				}
				AmbientLocationSounds.addSound(new Vector2(tileLocation.X, tileLocation.Y), 1);
			}
			else
			{
				if (playSound)
				{
					location.localSound("fireball");
				}
				base.performRemoveAction(tileLocation, location);
				AmbientLocationSounds.removeSound(new Vector2(tileLocation.X, tileLocation.Y));
			}
		}

		public virtual bool canBeRemoved(Farmer who)
		{
			if (heldObject.Value == null)
			{
				return true;
			}
			return false;
		}

		public override bool clicked(Farmer who)
		{
			Game1.haltAfterCheck = false;
			if ((int)furniture_type == 11 && who.ActiveObject != null && who.ActiveObject != null && heldObject.Value == null)
			{
				return false;
			}
			if (heldObject.Value != null)
			{
				Object item = heldObject.Value;
				heldObject.Value = null;
				if (who.addItemToInventoryBool(item))
				{
					item.performRemoveAction(tileLocation, who.currentLocation);
					Game1.playSound("coin");
					return true;
				}
				heldObject.Value = item;
			}
			return false;
		}

		public override void DayUpdate(GameLocation location)
		{
			base.DayUpdate(location);
			lightGlowAdded = false;
			if (!Game1.isDarkOut() || (Game1.newDay && !Game1.isRaining))
			{
				removeLights(location);
			}
			else
			{
				addLights(location);
			}
		}

		public void resetOnPlayerEntry(GameLocation environment, bool dropDown = false)
		{
			isTemporarilyInvisible = false;
			removeLights(environment);
			if ((int)furniture_type == 14)
			{
				setFireplace(environment, playSound: false);
			}
			if (Game1.isDarkOut())
			{
				addLights(environment);
				Furniture held_furniture;
				if (heldObject.Value != null && (held_furniture = (heldObject.Value as Furniture)) != null)
				{
					held_furniture.addLights(environment);
				}
			}
			if ((int)parentSheetIndex == 1971 && !dropDown)
			{
				environment.instantiateCrittersList();
				environment.addCritter(new Butterfly(environment.getRandomTile()).setStayInbounds(stayInbounds: true));
				while (Game1.random.NextDouble() < 0.5)
				{
					environment.addCritter(new Butterfly(environment.getRandomTile()).setStayInbounds(stayInbounds: true));
				}
			}
		}

		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
		{
			Object dropIn = dropInItem as Object;
			if (dropIn == null)
			{
				return false;
			}
			if (((int)furniture_type == 11 || (int)furniture_type == 5) && heldObject.Value == null && !dropIn.bigCraftable && !(dropIn is Wallpaper) && (!(dropIn is Furniture) || ((dropIn as Furniture).getTilesWide() == 1 && (dropIn as Furniture).getTilesHigh() == 1)))
			{
				heldObject.Value = (Object)dropIn.getOne();
				heldObject.Value.tileLocation.Value = tileLocation;
				heldObject.Value.boundingBox.X = boundingBox.X;
				heldObject.Value.boundingBox.Y = boundingBox.Y;
				heldObject.Value.performDropDownAction(who);
				if (!probe)
				{
					who.currentLocation.playSound("woodyStep");
					who?.reduceActiveItemByOne();
				}
				return true;
			}
			return false;
		}

		private int lightSourceIdentifier()
		{
			return (int)(tileLocation.X * 2000f + tileLocation.Y);
		}

		private void addLights(GameLocation environment)
		{
			if ((int)furniture_type == 7)
			{
				if (sourceIndexOffset.Value == 0)
				{
					sourceRect.Value = defaultSourceRect;
					sourceRect.X += sourceRect.Width;
				}
				sourceIndexOffset.Value = 1;
				if (base.lightSource == null)
				{
					environment.removeLightSource(lightSourceIdentifier());
					base.lightSource = new LightSource(4, new Vector2(boundingBox.X + 32, boundingBox.Y - 64), 2f, Color.Black, lightSourceIdentifier(), LightSource.LightContext.None, 0L);
					environment.sharedLights[base.lightSource.identifier] = base.lightSource.Clone();
				}
			}
			else if ((int)furniture_type == 13)
			{
				if (sourceIndexOffset.Value == 0)
				{
					sourceRect.Value = defaultSourceRect;
					sourceRect.X += sourceRect.Width;
				}
				sourceIndexOffset.Value = 1;
				if (lightGlowAdded)
				{
					environment.lightGlows.Remove(new Vector2(boundingBox.X + 32, boundingBox.Y + 64));
					lightGlowAdded = false;
				}
			}
		}

		private void removeLights(GameLocation environment)
		{
			if ((int)furniture_type == 7)
			{
				if (sourceIndexOffset.Value == 1)
				{
					sourceRect.Value = defaultSourceRect;
				}
				sourceIndexOffset.Value = 0;
				environment.removeLightSource(lightSourceIdentifier());
				base.lightSource = null;
			}
			else
			{
				if ((int)furniture_type != 13)
				{
					return;
				}
				if (sourceIndexOffset.Value == 1)
				{
					sourceRect.Value = defaultSourceRect;
				}
				sourceIndexOffset.Value = 0;
				if (Game1.isRaining)
				{
					sourceRect.Value = defaultSourceRect;
					sourceRect.X += sourceRect.Width;
					sourceIndexOffset.Value = 1;
					return;
				}
				if (!lightGlowAdded && !environment.lightGlows.Contains(new Vector2(boundingBox.X + 32, boundingBox.Y + 64)))
				{
					environment.lightGlows.Add(new Vector2(boundingBox.X + 32, boundingBox.Y + 64));
				}
				lightGlowAdded = true;
			}
		}

		public override bool minutesElapsed(int minutes, GameLocation environment)
		{
			if (Game1.isDarkOut())
			{
				addLights(environment);
				Furniture held_furniture2;
				if (heldObject.Value != null && (held_furniture2 = (heldObject.Value as Furniture)) != null)
				{
					held_furniture2.addLights(environment);
				}
			}
			else
			{
				removeLights(environment);
				Furniture held_furniture;
				if (heldObject.Value != null && (held_furniture = (heldObject.Value as Furniture)) != null)
				{
					held_furniture.removeLights(environment);
				}
			}
			return false;
		}

		public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			removeLights(environment);
			if ((int)furniture_type == 14)
			{
				isOn.Value = false;
				setFireplace(environment, playSound: false);
			}
			if ((int)furniture_type == 13 && lightGlowAdded)
			{
				environment.lightGlows.Remove(new Vector2(boundingBox.X + 32, boundingBox.Y + 64));
				lightGlowAdded = false;
			}
			base.performRemoveAction(tileLocation, environment);
			if ((int)furniture_type == 14)
			{
				base.lightSource = null;
			}
			if ((int)parentSheetIndex == 1309 && Game1.getMusicTrackName().Equals("sam_acoustic1"))
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
		}

		public void rotate()
		{
			if ((int)rotations >= 2)
			{
				_ = (int)currentRotation;
				int rotationAmount = ((int)rotations == 4) ? 1 : 2;
				currentRotation.Value += rotationAmount;
				currentRotation.Value %= 4;
				updateRotation();
			}
		}

		public void updateRotation()
		{
			flipped.Value = false;
			Point specialRotationOffsets = default(Point);
			switch ((int)furniture_type)
			{
			case 2:
				specialRotationOffsets.Y = 1;
				specialRotationOffsets.X = -1;
				break;
			case 5:
				specialRotationOffsets.Y = 0;
				specialRotationOffsets.X = -1;
				break;
			case 3:
				specialRotationOffsets.X = -1;
				specialRotationOffsets.Y = 1;
				break;
			case 12:
				specialRotationOffsets.X = 0;
				specialRotationOffsets.Y = 0;
				break;
			}
			bool differentSizesFor2Rotations = (int)furniture_type == 5 || (int)furniture_type == 12 || (int)parentSheetIndex == 724 || (int)parentSheetIndex == 727;
			bool sourceRectRotate = defaultBoundingBox.Width != defaultBoundingBox.Height;
			if (differentSizesFor2Rotations && (int)currentRotation == 2)
			{
				currentRotation.Value = 1;
			}
			if (sourceRectRotate)
			{
				int oldBoundingBoxHeight = boundingBox.Height;
				switch ((int)currentRotation)
				{
				case 0:
				case 2:
					boundingBox.Height = defaultBoundingBox.Height;
					boundingBox.Width = defaultBoundingBox.Width;
					break;
				case 1:
				case 3:
					boundingBox.Height = boundingBox.Width + specialRotationOffsets.X * 64;
					boundingBox.Width = oldBoundingBoxHeight + specialRotationOffsets.Y * 64;
					break;
				}
			}
			Point specialSpecialSourceRectOffset = default(Point);
			int num = furniture_type;
			if (num == 12)
			{
				specialSpecialSourceRectOffset.X = 1;
				specialSpecialSourceRectOffset.Y = -1;
			}
			if (sourceRectRotate)
			{
				switch ((int)currentRotation)
				{
				case 0:
					sourceRect.Value = defaultSourceRect;
					break;
				case 1:
					sourceRect.Value = new Rectangle(defaultSourceRect.X + defaultSourceRect.Width, defaultSourceRect.Y, defaultSourceRect.Height - 16 + specialRotationOffsets.Y * 16 + specialSpecialSourceRectOffset.X * 16, defaultSourceRect.Width + 16 + specialRotationOffsets.X * 16 + specialSpecialSourceRectOffset.Y * 16);
					break;
				case 2:
					sourceRect.Value = new Rectangle(defaultSourceRect.X + defaultSourceRect.Width + defaultSourceRect.Height - 16 + specialRotationOffsets.Y * 16 + specialSpecialSourceRectOffset.X * 16, defaultSourceRect.Y, defaultSourceRect.Width, defaultSourceRect.Height);
					break;
				case 3:
					sourceRect.Value = new Rectangle(defaultSourceRect.X + defaultSourceRect.Width, defaultSourceRect.Y, defaultSourceRect.Height - 16 + specialRotationOffsets.Y * 16 + specialSpecialSourceRectOffset.X * 16, defaultSourceRect.Width + 16 + specialRotationOffsets.X * 16 + specialSpecialSourceRectOffset.Y * 16);
					flipped.Value = true;
					break;
				}
			}
			else
			{
				flipped.Value = ((int)currentRotation == 3);
				if ((int)rotations == 2)
				{
					sourceRect.Value = new Rectangle(defaultSourceRect.X + (((int)currentRotation == 2) ? 1 : 0) * defaultSourceRect.Width, defaultSourceRect.Y, defaultSourceRect.Width, defaultSourceRect.Height);
				}
				else
				{
					sourceRect.Value = new Rectangle(defaultSourceRect.X + (((int)currentRotation == 3) ? 1 : ((int)currentRotation)) * defaultSourceRect.Width, defaultSourceRect.Y, defaultSourceRect.Width, defaultSourceRect.Height);
				}
			}
			if (differentSizesFor2Rotations && (int)currentRotation == 1)
			{
				currentRotation.Value = 2;
			}
			updateDrawPosition();
		}

		public bool isGroundFurniture()
		{
			if ((int)furniture_type != 13 && (int)furniture_type != 6)
			{
				return (int)furniture_type != 13;
			}
			return false;
		}

		public override bool canBeGivenAsGift()
		{
			return false;
		}

		public int GetModifiedWallTilePosition(GameLocation l, int tile_x, int tile_y)
		{
			if (isGroundFurniture())
			{
				return tile_y;
			}
			DecoratableLocation location = l as DecoratableLocation;
			if (location == null)
			{
				return tile_y;
			}
			foreach (Rectangle wall in location.getWalls())
			{
				if (wall.Contains(new Point(tile_x, tile_y)))
				{
					return wall.Top;
				}
			}
			return tile_y;
		}

		public override bool canBePlacedHere(GameLocation l, Vector2 tile)
		{
			if (!(l is DecoratableLocation))
			{
				return false;
			}
			if (!isGroundFurniture())
			{
				tile.Y = GetModifiedWallTilePosition(l, (int)tile.X, (int)tile.Y);
			}
			for (int x = 0; x < boundingBox.Width / 64; x++)
			{
				for (int y = 0; y < boundingBox.Height / 64; y++)
				{
					Vector2 nonTile = tile * 64f + new Vector2(x, y) * 64f;
					nonTile.X += 32f;
					nonTile.Y += 32f;
					foreach (Furniture f in (l as DecoratableLocation).furniture)
					{
						if ((int)f.furniture_type == 11 && f.getBoundingBox(f.tileLocation).Contains((int)nonTile.X, (int)nonTile.Y) && f.heldObject.Value == null && getTilesWide() == 1 && getTilesHigh() == 1)
						{
							return true;
						}
						if (((int)f.furniture_type != 12 || (int)furniture_type == 12) && f.getBoundingBox(f.tileLocation).Contains((int)nonTile.X, (int)nonTile.Y))
						{
							return false;
						}
					}
					Vector2 currentTile = tile + new Vector2(x, y);
					if (l.Objects.ContainsKey(currentTile))
					{
						return false;
					}
				}
			}
			Rectangle bounding_box = new Rectangle(boundingBox.Value.X, boundingBox.Value.Y, boundingBox.Value.Width, boundingBox.Value.Height);
			bounding_box.X = (int)tile.X * 64;
			bounding_box.Y = (int)tile.Y * 64;
			if (!isPassable())
			{
				foreach (Farmer farmer in l.farmers)
				{
					if (farmer.GetBoundingBox().Intersects(bounding_box))
					{
						return false;
					}
				}
			}
			if (GetAdditionalFurniturePlacementStatus(l, (int)tile.X * 64, (int)tile.Y * 64) != 0)
			{
				return false;
			}
			return base.canBePlacedHere(l, tile);
		}

		public void updateDrawPosition()
		{
			drawPosition.Value = new Vector2(boundingBox.X, boundingBox.Y - (sourceRect.Height * 4 - boundingBox.Height));
		}

		public int getTilesWide()
		{
			return boundingBox.Width / 64;
		}

		public int getTilesHigh()
		{
			return boundingBox.Height / 64;
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			if (!isGroundFurniture())
			{
				y = GetModifiedWallTilePosition(location, x / 64, y / 64) * 64;
			}
			if (GetAdditionalFurniturePlacementStatus(location, x, y, who) != 0)
			{
				return false;
			}
			boundingBox.Value = new Rectangle(x / 64 * 64, y / 64 * 64, boundingBox.Width, boundingBox.Height);
			foreach (Furniture f in (location as DecoratableLocation).furniture)
			{
				if ((int)f.furniture_type == 11 && f.heldObject.Value == null && f.getBoundingBox(f.tileLocation).Intersects(boundingBox))
				{
					f.performObjectDropInAction(this, probe: false, (who == null) ? Game1.player : who);
					return true;
				}
			}
			updateDrawPosition();
			return base.placementAction(location, x, y, who);
		}

		public int GetAdditionalFurniturePlacementStatus(GameLocation location, int x, int y, Farmer who = null)
		{
			if (!(location is DecoratableLocation))
			{
				return 4;
			}
			Point anchor = new Point(x / 64, y / 64);
			List<Rectangle> walls = (location as DecoratableLocation).getWalls();
			tileLocation.Value = new Vector2(anchor.X, anchor.Y);
			bool paintingAtRightPlace = false;
			if ((int)furniture_type == 6 || (int)furniture_type == 13 || (int)parentSheetIndex == 1293)
			{
				int offset = ((int)parentSheetIndex == 1293) ? 3 : 0;
				bool foundWall = false;
				foreach (Rectangle w in walls)
				{
					if (((int)furniture_type == 6 || (int)furniture_type == 13 || offset != 0) && w.Y + offset == anchor.Y && w.Contains(anchor.X, anchor.Y - offset))
					{
						foundWall = true;
						break;
					}
					if (!isGroundFurniture() && w.Y + 1 == anchor.Y && w.Contains(anchor.X, anchor.Y - 1))
					{
						foundWall = true;
						break;
					}
				}
				if (!foundWall)
				{
					return 1;
				}
				paintingAtRightPlace = true;
			}
			for (int furnitureX = anchor.X; furnitureX < anchor.X + getTilesWide(); furnitureX++)
			{
				for (int furnitureY = anchor.Y; furnitureY < anchor.Y + getTilesHigh(); furnitureY++)
				{
					if (location.doesTileHaveProperty(furnitureX, furnitureY, "NoFurniture", "Back") != null)
					{
						return 2;
					}
					if (!paintingAtRightPlace && Utility.pointInRectangles(walls, furnitureX, furnitureY))
					{
						return 3;
					}
					if (location.getTileIndexAt(furnitureX, furnitureY, "Buildings") != -1)
					{
						return -1;
					}
				}
			}
			return 0;
		}

		public override bool isPassable()
		{
			if (furniture_type.Value == 12)
			{
				return true;
			}
			return base.isPassable();
		}

		public override bool isPlaceable()
		{
			return true;
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			if (isTemporarilyInvisible)
			{
				return Rectangle.Empty;
			}
			return boundingBox;
		}

		private Rectangle getDefaultSourceRectForType(int tileIndex, int type)
		{
			int width;
			int height;
			switch (type)
			{
			case 0:
				width = 1;
				height = 2;
				break;
			case 1:
				width = 2;
				height = 2;
				break;
			case 2:
				width = 3;
				height = 2;
				break;
			case 3:
				width = 2;
				height = 2;
				break;
			case 4:
				width = 2;
				height = 2;
				break;
			case 5:
				width = 5;
				height = 3;
				break;
			case 6:
				width = 2;
				height = 2;
				break;
			case 7:
				width = 1;
				height = 3;
				break;
			case 8:
				width = 1;
				height = 2;
				break;
			case 10:
				width = 2;
				height = 3;
				break;
			case 11:
				width = 2;
				height = 3;
				break;
			case 12:
				width = 3;
				height = 2;
				break;
			case 13:
				width = 1;
				height = 2;
				break;
			case 14:
				width = 2;
				height = 5;
				break;
			default:
				width = 1;
				height = 2;
				break;
			}
			return new Rectangle(tileIndex * 16 % furnitureTexture.Width, tileIndex * 16 / furnitureTexture.Width * 16, width * 16, height * 16);
		}

		private Rectangle getDefaultBoundingBoxForType(int type)
		{
			int width;
			int height;
			switch (type)
			{
			case 0:
				width = 1;
				height = 1;
				break;
			case 1:
				width = 2;
				height = 1;
				break;
			case 2:
				width = 3;
				height = 1;
				break;
			case 3:
				width = 2;
				height = 1;
				break;
			case 4:
				width = 2;
				height = 1;
				break;
			case 5:
				width = 5;
				height = 2;
				break;
			case 6:
				width = 2;
				height = 2;
				break;
			case 7:
				width = 1;
				height = 1;
				break;
			case 8:
				width = 1;
				height = 1;
				break;
			case 10:
				width = 2;
				height = 1;
				break;
			case 11:
				width = 2;
				height = 2;
				break;
			case 12:
				width = 3;
				height = 2;
				break;
			case 13:
				width = 1;
				height = 2;
				break;
			case 14:
				width = 2;
				height = 1;
				break;
			default:
				width = 1;
				height = 1;
				break;
			}
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, width * 64, height * 64);
		}

		private int getTypeNumberFromName(string typeName)
		{
			switch (typeName.ToLower())
			{
			case "chair":
				return 0;
			case "bench":
				return 1;
			case "couch":
				return 2;
			case "armchair":
				return 3;
			case "dresser":
				return 4;
			case "long table":
				return 5;
			case "painting":
				return 6;
			case "lamp":
				return 7;
			case "decor":
				return 8;
			case "bookcase":
				return 10;
			case "table":
				return 11;
			case "rug":
				return 12;
			case "window":
				return 13;
			case "fireplace":
				return 14;
			default:
				return 9;
			}
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

		private float getScaleSize()
		{
			int tilesWide = sourceRect.Width / 16;
			int tilesHigh = sourceRect.Height / 16;
			if (tilesWide >= 5)
			{
				return 0.75f;
			}
			if (tilesHigh >= 3)
			{
				return 1f;
			}
			if (tilesWide <= 2)
			{
				return 2f;
			}
			if (tilesWide <= 4)
			{
				return 1f;
			}
			return 0.1f;
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (shakeTimer > 0)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
		}

		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			spriteBatch.Draw(furnitureTexture, location + new Vector2(32f, 32f), defaultSourceRect, color * transparency, 0f, new Vector2(defaultSourceRect.Width / 2, defaultSourceRect.Height / 2), 1f * getScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
			if (((drawStackNumber == StackDrawType.Draw && maximumStackSize() > 1 && Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && Stack != int.MaxValue)
			{
				Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
			}
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (isTemporarilyInvisible)
			{
				return;
			}
			if (isDrawingLocationFurniture)
			{
				spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), sourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)furniture_type == 12) ? 2E-09f : ((float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 13) ? 48 : 8)) / 10000f));
			}
			else
			{
				spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (sourceRect.Height * 4 - boundingBox.Height) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), sourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)furniture_type == 12) ? 2E-09f : ((float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 13) ? 48 : 8)) / 10000f));
			}
			if (heldObject.Value != null)
			{
				if (heldObject.Value is Furniture)
				{
					(heldObject.Value as Furniture).drawAtNonTileSpot(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(boundingBox.Center.X - 32, boundingBox.Center.Y - (heldObject.Value as Furniture).sourceRect.Height * 4 - (drawHeldObjectLow ? (-16) : 16))), (float)(boundingBox.Bottom - 7) / 10000f, alpha);
				}
				else
				{
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(boundingBox.Center.X - 32, boundingBox.Center.Y - (drawHeldObjectLow ? 32 : 85))) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)boundingBox.Bottom / 10000f);
					spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(boundingBox.Center.X - 32, boundingBox.Center.Y - (drawHeldObjectLow ? 32 : 85))), GameLocation.getSourceRectForObject(heldObject.Value.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(boundingBox.Bottom + 1) / 10000f);
				}
			}
			if ((bool)isOn && (int)furniture_type == 14)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(boundingBox.Center.X - 12, boundingBox.Center.Y - 64)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(getBoundingBox(new Vector2(x, y)).Bottom - 2) / 10000f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(boundingBox.Center.X - 32 - 4, boundingBox.Center.Y - 64)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2047) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(getBoundingBox(new Vector2(x, y)).Bottom - 1) / 10000f);
			}
			if (Game1.debugMode)
			{
				spriteBatch.DrawString(Game1.smallFont, string.Concat((object)parentSheetIndex), Game1.GlobalToLocal(Game1.viewport, drawPosition), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}
		}

		public void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
		{
			spriteBatch.Draw(furnitureTexture, location, sourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}

		public override Item getOne()
		{
			Furniture furniture = new Furniture(parentSheetIndex, tileLocation);
			furniture.drawPosition.Value = drawPosition;
			furniture.defaultBoundingBox.Value = defaultBoundingBox;
			furniture.boundingBox.Value = boundingBox;
			furniture.currentRotation.Value = (int)currentRotation - 1;
			furniture.isOn.Value = false;
			furniture.rotations.Value = rotations;
			furniture.rotate();
			return furniture;
		}
	}
}
