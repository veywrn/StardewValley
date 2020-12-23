using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class Furniture : Object, ISittable
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

		public const int bed = 15;

		public const int torch = 16;

		public const int sconce = 17;

		public const string furnitureTextureName = "TileSheets\\furniture";

		public const string furnitureFrontTextureName = "TileSheets\\furnitureFront";

		[XmlIgnore]
		public static Texture2D furnitureTexture;

		[XmlIgnore]
		public static Texture2D furnitureFrontTexture;

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

		[XmlIgnore]
		public NetLongDictionary<int, NetInt> sittingFarmers = new NetLongDictionary<int, NetInt>();

		[XmlIgnore]
		public Vector2? lightGlowPosition;

		public static bool isDrawingLocationFurniture;

		[XmlIgnore]
		private int _placementRestriction = -1;

		[XmlIgnore]
		private string _description;

		private const int fireIDBase = 944469;

		[XmlIgnore]
		public int placementRestriction
		{
			get
			{
				if (_placementRestriction < 0)
				{
					bool use_default = true;
					string[] data = getData();
					if (data.Length > 6 && int.TryParse(data[6], out _placementRestriction))
					{
						use_default = false;
					}
					if (use_default)
					{
						if (base.name.Contains("TV"))
						{
							_placementRestriction = 0;
						}
						else if (furniture_type.Value == 11 || furniture_type.Value == 1 || furniture_type.Value == 0 || furniture_type.Value == 5 || furniture_type.Value == 8 || furniture_type.Value == 16)
						{
							_placementRestriction = 2;
						}
						else
						{
							_placementRestriction = 0;
						}
					}
				}
				return _placementRestriction;
			}
		}

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
			base.NetFields.AddFields(furniture_type, rotations, currentRotation, sourceIndexOffset, drawPosition, sourceRect, defaultSourceRect, defaultBoundingBox, drawHeldObjectLow, sittingFarmers);
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

		public void OnAdded(GameLocation loc, Vector2 tilePos)
		{
			if (IntersectsForCollision(Game1.player.GetBoundingBox()))
			{
				Game1.player.TemporaryPassableTiles.Add(getBoundingBox(tilePos));
			}
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

		protected string[] getData()
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
			if (placementRestriction == 1)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Outdoors_Description");
			}
			if (placementRestriction == 2)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Decoration_Description");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12623");
		}

		private void specialVariableChange(bool newValue)
		{
			if (((int)furniture_type == 14 || (int)furniture_type == 16) && newValue)
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
				return true;
			case 1308:
				Game1.activeClickableMenu = new ShopMenu(Utility.getAllWallpapersAndFloorsForFree(), 0, null, null, null, "Catalogue");
				return true;
			case 1226:
				Game1.activeClickableMenu = new ShopMenu(Utility.getAllFurnituresForFree(), 0, null, null, null, "Furniture Catalogue");
				return true;
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
				return true;
			default:
				if ((int)furniture_type == 14 || (int)furniture_type == 16)
				{
					isOn.Value = !isOn.Value;
					initializeLightSource(tileLocation);
					setFireplace(who.currentLocation, playSound: true, broadcast: true);
					return true;
				}
				if (GetSeatCapacity() > 0)
				{
					who.BeginSitting(this);
					return true;
				}
				return clicked(who);
			}
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

		public virtual void AttemptRemoval(Action<Furniture> removal_action)
		{
			removal_action?.Invoke(this);
		}

		public virtual bool canBeRemoved(Farmer who)
		{
			if (HasSittingFarmers())
			{
				return false;
			}
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

		public virtual int GetSeatCapacity()
		{
			if ((int)furniture_type == 0)
			{
				return 1;
			}
			if ((int)furniture_type == 1)
			{
				return 2;
			}
			if ((int)furniture_type == 2)
			{
				return defaultBoundingBox.Width / 64 - 1;
			}
			if ((int)furniture_type == 3)
			{
				return 1;
			}
			return 0;
		}

		public bool IsSeatHere(GameLocation location)
		{
			return location.furniture.Contains(this);
		}

		public bool IsSittingHere(Farmer who)
		{
			if (sittingFarmers.ContainsKey(who.UniqueMultiplayerID))
			{
				return true;
			}
			return false;
		}

		public Vector2? GetSittingPosition(Farmer who, bool ignore_offsets = false)
		{
			if (sittingFarmers.ContainsKey(who.UniqueMultiplayerID))
			{
				return GetSeatPositions(ignore_offsets)[sittingFarmers[who.UniqueMultiplayerID]];
			}
			return null;
		}

		public bool HasSittingFarmers()
		{
			return sittingFarmers.Count() > 0;
		}

		public virtual void RemoveSittingFarmer(Farmer farmer)
		{
			sittingFarmers.Remove(farmer.UniqueMultiplayerID);
		}

		public virtual int GetSittingFarmerCount()
		{
			return sittingFarmers.Count();
		}

		public virtual Rectangle GetSeatBounds()
		{
			Rectangle bounds = getBoundingBox(base.TileLocation);
			bounds.X /= 64;
			bounds.Y /= 64;
			bounds.Width /= 64;
			bounds.Height /= 64;
			return bounds;
		}

		public int GetSittingDirection()
		{
			if (Name.Contains("Stool"))
			{
				return Game1.player.FacingDirection;
			}
			if (currentRotation.Value == 0)
			{
				return 2;
			}
			if (currentRotation.Value == 1)
			{
				return 1;
			}
			if (currentRotation.Value == 2)
			{
				return 0;
			}
			if (currentRotation.Value == 3)
			{
				return 3;
			}
			return 2;
		}

		public virtual Vector2? AddSittingFarmer(Farmer who)
		{
			List<Vector2> seat_positions = GetSeatPositions();
			int seat_index = -1;
			Vector2? sit_position = null;
			float distance = 96f;
			for (int i = 0; i < seat_positions.Count; i++)
			{
				if (!sittingFarmers.Values.Contains(i))
				{
					float curr_distance = ((seat_positions[i] + new Vector2(0.5f, 0.5f)) * 64f - who.getStandingPosition()).Length();
					if (curr_distance < distance)
					{
						distance = curr_distance;
						sit_position = seat_positions[i];
						seat_index = i;
					}
				}
			}
			if (sit_position.HasValue)
			{
				sittingFarmers[who.UniqueMultiplayerID] = seat_index;
			}
			return sit_position;
		}

		public virtual List<Vector2> GetSeatPositions(bool ignore_offsets = false)
		{
			List<Vector2> seat_positions = new List<Vector2>();
			if ((int)furniture_type == 0)
			{
				seat_positions.Add(base.TileLocation);
			}
			if ((int)furniture_type == 1)
			{
				for (int x = 0; x < getTilesWide(); x++)
				{
					for (int y = 0; y < getTilesHigh(); y++)
					{
						seat_positions.Add(base.TileLocation + new Vector2(x, y));
					}
				}
			}
			if ((int)furniture_type == 2)
			{
				int width = defaultBoundingBox.Width / 64 - 1;
				if ((int)currentRotation == 0 || (int)currentRotation == 2)
				{
					seat_positions.Add(base.TileLocation + new Vector2(0.5f, 0f));
					for (int i = 1; i < width - 1; i++)
					{
						seat_positions.Add(base.TileLocation + new Vector2((float)i + 0.5f, 0f));
					}
					seat_positions.Add(base.TileLocation + new Vector2((float)(width - 1) + 0.5f, 0f));
				}
				else if ((int)currentRotation == 1)
				{
					for (int k = 0; k < width; k++)
					{
						seat_positions.Add(base.TileLocation + new Vector2(1f, k));
					}
				}
				else
				{
					for (int j = 0; j < width; j++)
					{
						seat_positions.Add(base.TileLocation + new Vector2(0f, j));
					}
				}
			}
			if ((int)furniture_type == 3)
			{
				if ((int)currentRotation == 0 || (int)currentRotation == 2)
				{
					seat_positions.Add(base.TileLocation + new Vector2(0.5f, 0f));
				}
				else if ((int)currentRotation == 1)
				{
					seat_positions.Add(base.TileLocation + new Vector2(1f, 0f));
				}
				else
				{
					seat_positions.Add(base.TileLocation + new Vector2(0f, 0f));
				}
			}
			return seat_positions;
		}

		public override void DayUpdate(GameLocation location)
		{
			base.DayUpdate(location);
			sittingFarmers.Clear();
			if (!Game1.isDarkOut() || (Game1.newDay && !Game1.IsRainingHere(location)))
			{
				removeLights(location);
			}
			else
			{
				addLights(location);
			}
			RemoveLightGlow(location);
		}

		public virtual void AddLightGlow(GameLocation location)
		{
			if (!lightGlowPosition.HasValue)
			{
				Vector2 light_glow_position = new Vector2(boundingBox.X + 32, boundingBox.Y + 64);
				if (!location.lightGlows.Contains(light_glow_position))
				{
					lightGlowPosition = light_glow_position;
					location.lightGlows.Add(light_glow_position);
				}
			}
		}

		public virtual void RemoveLightGlow(GameLocation location)
		{
			if (lightGlowPosition.HasValue && location.lightGlows.Contains(lightGlowPosition.Value))
			{
				location.lightGlows.Remove(lightGlowPosition.Value);
			}
			lightGlowPosition = null;
		}

		public virtual void resetOnPlayerEntry(GameLocation environment, bool dropDown = false)
		{
			isTemporarilyInvisible = false;
			RemoveLightGlow(environment);
			removeLights(environment);
			if ((int)furniture_type == 14 || (int)furniture_type == 16)
			{
				setFireplace(environment, playSound: false);
			}
			if (Game1.isDarkOut())
			{
				addLights(environment);
				if (heldObject.Value != null && heldObject.Value is Furniture)
				{
					(heldObject.Value as Furniture).addLights(environment);
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

		public void addLights(GameLocation environment)
		{
			if ((int)furniture_type == 7 || (int)furniture_type == 17)
			{
				sourceRect.Value = defaultSourceRect.Value;
				sourceIndexOffset.Value = 1;
				if (base.lightSource == null)
				{
					environment.removeLightSource(lightSourceIdentifier());
					base.lightSource = new LightSource(4, new Vector2(boundingBox.X + 32, boundingBox.Y + (((int)furniture_type == 7) ? (-64) : 64)), ((int)furniture_type == 17) ? 1f : 2f, Color.Black, lightSourceIdentifier(), LightSource.LightContext.None, 0L);
					environment.sharedLights[base.lightSource.identifier] = base.lightSource.Clone();
				}
			}
			else if ((int)furniture_type == 13)
			{
				sourceRect.Value = defaultSourceRect.Value;
				sourceIndexOffset.Value = 1;
				RemoveLightGlow(environment);
			}
			else if (this is FishTankFurniture && base.lightSource == null)
			{
				int identifier = lightSourceIdentifier();
				Vector2 light_position = new Vector2(tileLocation.X * 64f + 32f + 2f, tileLocation.Y * 64f + 12f);
				for (int i = 0; i < getTilesWide(); i++)
				{
					environment.removeLightSource(identifier);
					base.lightSource = new LightSource(8, light_position, 2f, Color.Black, identifier, LightSource.LightContext.None, 0L);
					environment.sharedLights[identifier] = base.lightSource.Clone();
					light_position.X += 64f;
					identifier += 2000;
				}
			}
		}

		public void removeLights(GameLocation environment)
		{
			if ((int)furniture_type == 7 || (int)furniture_type == 17)
			{
				sourceRect.Value = defaultSourceRect.Value;
				sourceIndexOffset.Value = 0;
				environment.removeLightSource(lightSourceIdentifier());
				base.lightSource = null;
			}
			else if ((int)furniture_type == 13)
			{
				if (Game1.IsRainingHere(environment))
				{
					sourceRect.Value = defaultSourceRect.Value;
					sourceIndexOffset.Value = 1;
				}
				else
				{
					sourceRect.Value = defaultSourceRect.Value;
					sourceIndexOffset.Value = 0;
					AddLightGlow(environment);
				}
			}
			else if (this is FishTankFurniture)
			{
				int identifier = lightSourceIdentifier();
				for (int i = 0; i < getTilesWide(); i++)
				{
					environment.removeLightSource(identifier);
					identifier += 2000;
				}
				base.lightSource = null;
			}
		}

		public override bool minutesElapsed(int minutes, GameLocation environment)
		{
			if (Game1.isDarkOut())
			{
				addLights(environment);
				if (heldObject.Value != null && heldObject.Value is Furniture)
				{
					(heldObject.Value as Furniture).addLights(environment);
				}
			}
			else
			{
				removeLights(environment);
				if (heldObject.Value != null && heldObject.Value is Furniture)
				{
					(heldObject.Value as Furniture).removeLights(environment);
				}
			}
			return false;
		}

		public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			removeLights(environment);
			if ((int)furniture_type == 14 || (int)furniture_type == 16)
			{
				isOn.Value = false;
				setFireplace(environment, playSound: false);
			}
			RemoveLightGlow(environment);
			base.performRemoveAction(tileLocation, environment);
			if ((int)furniture_type == 14 || (int)furniture_type == 16)
			{
				base.lightSource = null;
			}
			if ((int)parentSheetIndex == 1309 && Game1.getMusicTrackName().Equals("sam_acoustic1"))
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			sittingFarmers.Clear();
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
			if ((int)furniture_type != 13 && (int)furniture_type != 6 && (int)furniture_type != 17)
			{
				return (int)furniture_type != 13;
			}
			return false;
		}

		public override bool canBeGivenAsGift()
		{
			return false;
		}

		public static Furniture GetFurnitureInstance(int index, Vector2? position = null)
		{
			if (!position.HasValue)
			{
				position = Vector2.Zero;
			}
			if (index == 1466 || index == 1468 || index == 1680 || index == 2326)
			{
				return new TV(index, position.Value);
			}
			Dictionary<int, string> data_sheet = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
			if (data_sheet.ContainsKey(index))
			{
				string furniture_type = data_sheet[index].Split('/')[1];
				if (furniture_type == "fishtank")
				{
					return new FishTankFurniture(index, position.Value);
				}
				if (furniture_type.StartsWith("bed"))
				{
					return new BedFurniture(index, position.Value);
				}
				if (furniture_type == "dresser")
				{
					return new StorageFurniture(index, position.Value);
				}
			}
			return new Furniture(index, position.Value);
		}

		public virtual bool IsCloseEnoughToFarmer(Farmer f, int? override_tile_x = null, int? override_tile_y = null)
		{
			Rectangle furniture_rect = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, getTilesWide() * 64, getTilesHigh() * 64);
			if (override_tile_x.HasValue)
			{
				furniture_rect.X = override_tile_x.Value * 64;
			}
			if (override_tile_y.HasValue)
			{
				furniture_rect.Y = override_tile_y.Value * 64;
			}
			furniture_rect.Inflate(96, 96);
			if (furniture_rect.Contains(Utility.Vector2ToPoint(Game1.player.getStandingPosition())))
			{
				return true;
			}
			return false;
		}

		public int GetModifiedWallTilePosition(GameLocation l, int tile_x, int tile_y)
		{
			if (isGroundFurniture())
			{
				return tile_y;
			}
			if (l == null)
			{
				return tile_y;
			}
			foreach (Rectangle wall in l.getWalls())
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
			if (!l.CanPlaceThisFurnitureHere(this))
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
					foreach (Furniture f in l.furniture)
					{
						if ((int)f.furniture_type == 11 && f.getBoundingBox(f.tileLocation).Contains((int)nonTile.X, (int)nonTile.Y) && f.heldObject.Value == null && getTilesWide() == 1 && getTilesHigh() == 1)
						{
							return true;
						}
						if (((int)f.furniture_type != 12 || (int)furniture_type == 12) && f.getBoundingBox(f.tileLocation).Contains((int)nonTile.X, (int)nonTile.Y) && !f.AllowPlacementOnThisTile((int)tile.X + x, (int)tile.Y + y))
						{
							return false;
						}
					}
					Vector2 currentTile = tile + new Vector2(x, y);
					if (l.Objects.ContainsKey(currentTile))
					{
						return false;
					}
					if (l.getLargeTerrainFeatureAt((int)currentTile.X, (int)currentTile.Y) != null)
					{
						return false;
					}
					if (l.terrainFeatures.ContainsKey(currentTile) && l.terrainFeatures[currentTile] is Tree)
					{
						return false;
					}
					if (l.isTerrainFeatureAt((int)currentTile.X, (int)currentTile.Y))
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
				foreach (NPC character in l.characters)
				{
					if (character.GetBoundingBox().Intersects(bounding_box))
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
			foreach (Furniture f in location.furniture)
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

		public virtual int GetAdditionalFurniturePlacementStatus(GameLocation location, int x, int y, Farmer who = null)
		{
			if (!location.CanPlaceThisFurnitureHere(this))
			{
				return 4;
			}
			Point anchor = new Point(x / 64, y / 64);
			List<Rectangle> walls = location.getWalls();
			tileLocation.Value = new Vector2(anchor.X, anchor.Y);
			bool paintingAtRightPlace = false;
			if ((int)furniture_type == 6 || (int)furniture_type == 17 || (int)furniture_type == 13 || (int)parentSheetIndex == 1293)
			{
				int offset = ((int)parentSheetIndex == 1293) ? 3 : 0;
				bool foundWall = false;
				foreach (Rectangle w in walls)
				{
					if (((int)furniture_type == 6 || (int)furniture_type == 17 || (int)furniture_type == 13 || offset != 0) && w.Y + offset == anchor.Y && w.Contains(anchor.X, anchor.Y - offset))
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
			int tiles_high_to_check = getTilesHigh();
			if ((int)furniture_type == 6 && tiles_high_to_check > 2)
			{
				tiles_high_to_check = 2;
			}
			for (int furnitureX = anchor.X; furnitureX < anchor.X + getTilesWide(); furnitureX++)
			{
				for (int furnitureY = anchor.Y; furnitureY < anchor.Y + tiles_high_to_check; furnitureY++)
				{
					if (location.doesTileHaveProperty(furnitureX, furnitureY, "NoFurniture", "Back") != null)
					{
						return 2;
					}
					if (!paintingAtRightPlace && Utility.pointInRectangles(walls, furnitureX, furnitureY))
					{
						if (!(this is BedFurniture) || furnitureY != anchor.Y)
						{
							return 3;
						}
						continue;
					}
					int buildings_index = location.getTileIndexAt(furnitureX, furnitureY, "Buildings");
					if (buildings_index != -1 && (!(location is IslandFarmHouse) || buildings_index < 192 || buildings_index > 194 || !(location.getTileSheetIDAt(furnitureX, furnitureY, "Buildings") == "untitled tile sheet")))
					{
						return -1;
					}
					if (location is BuildableGameLocation && (location as BuildableGameLocation).isTileOccupiedForPlacement(new Vector2(furnitureX, furnitureY), this))
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

		public virtual bool AllowPlacementOnThisTile(int tile_x, int tile_y)
		{
			return false;
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
			case 17:
				width = 1;
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
			case 16:
				width = 1;
				height = 2;
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
			case 17:
				width = 1;
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
			case 16:
				width = 1;
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
			if (typeName.ToLower().StartsWith("bed"))
			{
				return 15;
			}
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
			case "torch":
				return 16;
			case "sconce":
				return 17;
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
			int tilesWide = defaultSourceRect.Width / 16;
			int tilesHigh = defaultSourceRect.Height / 16;
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
			if (Game1.IsMasterGame && sittingFarmers.Count() > 0)
			{
				List<long> ids_to_remove = new List<long>();
				foreach (long uid2 in sittingFarmers.Keys)
				{
					if (!Game1.player.team.playerIsOnline(uid2))
					{
						ids_to_remove.Add(uid2);
					}
				}
				foreach (long uid in ids_to_remove)
				{
					sittingFarmers.Remove(uid);
				}
			}
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
			Rectangle drawn_source_rect = sourceRect.Value;
			drawn_source_rect.X += drawn_source_rect.Width * sourceIndexOffset.Value;
			if (isDrawingLocationFurniture)
			{
				if (HasSittingFarmers() && sourceRect.Right <= furnitureFrontTexture.Width && sourceRect.Bottom <= furnitureFrontTexture.Height)
				{
					spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_source_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(boundingBox.Value.Top + 16) / 10000f);
					spriteBatch.Draw(furnitureFrontTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_source_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(boundingBox.Value.Bottom - 8) / 10000f);
				}
				else
				{
					spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_source_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)furniture_type == 12) ? (2E-09f + tileLocation.Y / 100000f) : ((float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 17 || (int)furniture_type == 13) ? 48 : 8)) / 10000f));
				}
			}
			else
			{
				spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (sourceRect.Height * 4 - boundingBox.Height) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), sourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)furniture_type == 12) ? (2E-09f + tileLocation.Y / 100000f) : ((float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 17 || (int)furniture_type == 13) ? 48 : 8)) / 10000f));
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
			else if ((bool)isOn && (int)furniture_type == 16)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(boundingBox.Center.X - 20, (float)boundingBox.Center.Y - 105.6f)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(getBoundingBox(new Vector2(x, y)).Bottom - 2) / 10000f);
			}
			if (Game1.debugMode)
			{
				spriteBatch.DrawString(Game1.smallFont, string.Concat((object)parentSheetIndex), Game1.GlobalToLocal(Game1.viewport, drawPosition), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}
		}

		public void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
		{
			Rectangle drawn_source_rect = sourceRect.Value;
			drawn_source_rect.X += drawn_source_rect.Width * (int)sourceIndexOffset;
			spriteBatch.Draw(furnitureTexture, location, drawn_source_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}

		public virtual int GetAdditionalTilePropertyRadius()
		{
			return 0;
		}

		public virtual bool DoesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
		{
			return false;
		}

		public virtual bool IntersectsForCollision(Rectangle rect)
		{
			return getBoundingBox(tileLocation).Intersects(rect);
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
			furniture._GetOneFrom(this);
			return furniture;
		}
	}
}
