using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class BedFurniture : Furniture
	{
		public enum BedType
		{
			Any = -1,
			Single,
			Double,
			Child
		}

		public static int DEFAULT_BED_INDEX = 2048;

		public static int DOUBLE_BED_INDEX = 2052;

		public static int CHILD_BED_INDEX = 2076;

		[XmlIgnore]
		public int bedTileOffset;

		[XmlIgnore]
		protected bool _alreadyAttempingRemoval;

		[XmlIgnore]
		public static bool ignoreContextualBedSpotOffset = false;

		[XmlIgnore]
		protected NetEnum<BedType> _bedType = new NetEnum<BedType>(BedType.Any);

		[XmlIgnore]
		public NetMutex mutex = new NetMutex();

		[XmlElement("bedType")]
		public BedType bedType
		{
			get
			{
				if (_bedType.Value == BedType.Any)
				{
					BedType bed_type = BedType.Single;
					string[] data = getData();
					if (data.Length > 1)
					{
						string[] tokens = data[1].Split(' ');
						if (tokens.Length > 1)
						{
							if (tokens[1] == "double")
							{
								bed_type = BedType.Double;
							}
							else if (tokens[1] == "child")
							{
								bed_type = BedType.Child;
							}
						}
					}
					_bedType.Value = bed_type;
				}
				return _bedType.Value;
			}
			set
			{
				_bedType.Value = value;
			}
		}

		public BedFurniture()
		{
		}

		public BedFurniture(int which, Vector2 tile, int initialRotations)
			: base(which, tile, initialRotations)
		{
		}

		public BedFurniture(int which, Vector2 tile)
			: base(which, tile)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(_bedType, mutex.NetFields);
		}

		public virtual bool IsBeingSleptIn(GameLocation location)
		{
			if (mutex.IsLocked())
			{
				return true;
			}
			foreach (Farmer farmer in location.farmers)
			{
				if (farmer.GetBoundingBox().Intersects(getBoundingBox(tileLocation)))
				{
					return true;
				}
			}
			return false;
		}

		public override void DayUpdate(GameLocation location)
		{
			base.DayUpdate(location);
			mutex.ReleaseLock();
		}

		public virtual void ReserveForNPC()
		{
			mutex.RequestLock();
		}

		public override void AttemptRemoval(Action<Furniture> removal_action)
		{
			if (_alreadyAttempingRemoval)
			{
				_alreadyAttempingRemoval = false;
				return;
			}
			_alreadyAttempingRemoval = true;
			mutex.RequestLock(delegate
			{
				_alreadyAttempingRemoval = false;
				if (removal_action != null)
				{
					removal_action(this);
					mutex.ReleaseLock();
				}
			}, delegate
			{
				_alreadyAttempingRemoval = false;
			});
		}

		public static BedFurniture GetBedAtTile(GameLocation location, int x, int y)
		{
			if (location == null)
			{
				return null;
			}
			foreach (Furniture furniture in location.furniture)
			{
				if (Utility.doesRectangleIntersectTile(furniture.getBoundingBox(furniture.tileLocation), x, y) && furniture is BedFurniture)
				{
					return furniture as BedFurniture;
				}
			}
			return null;
		}

		public static void ApplyWakeUpPosition(Farmer who)
		{
			if (who.lastSleepLocation.Value != null && Game1.isLocationAccessible(who.lastSleepLocation) && Game1.getLocationFromName(who.lastSleepLocation) != null && (IsBedHere(Game1.getLocationFromName(who.lastSleepLocation), who.lastSleepPoint.Value.X, who.lastSleepPoint.Value.Y) || who.sleptInTemporaryBed.Value || Game1.getLocationFromName(who.lastSleepLocation) is IslandFarmHouse))
			{
				GameLocation start_location = Game1.getLocationFromName(who.lastSleepLocation);
				who.Position = Utility.PointToVector2(who.lastSleepPoint.Value) * 64f;
				who.currentLocation = start_location;
				ShiftPositionForBed(who);
			}
			else
			{
				GameLocation home = who.currentLocation = Game1.getLocationFromName(who.homeLocation.Value);
				who.Position = Utility.PointToVector2((home as FarmHouse).GetPlayerBedSpot()) * 64f;
				ShiftPositionForBed(who);
			}
			if (who == Game1.player)
			{
				Game1.currentLocation = who.currentLocation;
			}
		}

		public static void ShiftPositionForBed(Farmer who)
		{
			BedFurniture bed = GetBedAtTile(who.currentLocation, (int)(who.position.X / 64f), (int)(who.position.Y / 64f));
			if (bed != null)
			{
				who.Position = Utility.PointToVector2(bed.GetBedSpot()) * 64f;
				if (bed.bedType != BedType.Double)
				{
					if (who.currentLocation.map == null)
					{
						who.currentLocation.reloadMap();
					}
					if (!who.currentLocation.isTileLocationTotallyClearAndPlaceable(new Vector2(bed.TileLocation.X - 1f, bed.TileLocation.Y + 1f)))
					{
						who.faceDirection(3);
					}
					else
					{
						who.position.X -= 64f;
						who.faceDirection(1);
					}
				}
				else
				{
					bool should_wake_up_in_spouse_spot = false;
					if (who.currentLocation is FarmHouse)
					{
						FarmHouse farmhouse = who.currentLocation as FarmHouse;
						if (farmhouse.owner != null)
						{
							if (farmhouse.owner.team.GetSpouse(farmhouse.owner.UniqueMultiplayerID) == who.UniqueMultiplayerID)
							{
								should_wake_up_in_spouse_spot = true;
							}
							else if (farmhouse.owner != who && !farmhouse.owner.isMarried())
							{
								should_wake_up_in_spouse_spot = true;
							}
						}
					}
					if (should_wake_up_in_spouse_spot)
					{
						who.position.X += 64f;
						who.faceDirection(3);
					}
					else
					{
						who.position.X -= 64f;
						who.faceDirection(1);
					}
				}
			}
			who.position.Y += 32f;
			if (who.NetFields.Root != null)
			{
				(who.NetFields.Root as NetRoot<Farmer>).CancelInterpolation();
			}
		}

		public virtual bool CanModifyBed(GameLocation location, Farmer who)
		{
			if (location is FarmHouse)
			{
				FarmHouse farmhouse = location as FarmHouse;
				if (farmhouse.owner != who && farmhouse.owner.team.GetSpouse(farmhouse.owner.UniqueMultiplayerID) != who.UniqueMultiplayerID)
				{
					return false;
				}
			}
			return true;
		}

		public override int GetAdditionalFurniturePlacementStatus(GameLocation location, int x, int y, Farmer who = null)
		{
			if (bedType == BedType.Double)
			{
				if (!location.isTileLocationTotallyClearAndPlaceable(new Vector2(x / 64 - 1, y / 64 + 1)))
				{
					return -1;
				}
			}
			else if (!location.isTileLocationTotallyClearAndPlaceable(new Vector2(x / 64 - 1, y / 64 + 1)) && !location.isTileLocationTotallyClearAndPlaceable(new Vector2(x / 64 + getTilesWide(), y / 64 + 1)))
			{
				return -1;
			}
			return base.GetAdditionalFurniturePlacementStatus(location, x, y, who);
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			_alreadyAttempingRemoval = false;
			if (!CanModifyBed(location, who))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_CantMoveOthersBeds"));
				return false;
			}
			if (location is FarmHouse && ((bedType == BedType.Child && (location as FarmHouse).upgradeLevel < 2) || (bedType == BedType.Double && (location as FarmHouse).upgradeLevel < 1)))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_NeedsUpgrade"));
				return false;
			}
			return base.placementAction(location, x, y, who);
		}

		public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			_alreadyAttempingRemoval = false;
			base.performRemoveAction(tileLocation, environment);
		}

		public override void hoverAction()
		{
			if (!Game1.player.GetBoundingBox().Intersects(getBoundingBox(tileLocation)))
			{
				base.hoverAction();
			}
		}

		public override bool canBeRemoved(Farmer who)
		{
			if (!CanModifyBed(who.currentLocation, who))
			{
				if (!Game1.player.GetBoundingBox().Intersects(getBoundingBox(tileLocation)))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_CantMoveOthersBeds"));
				}
				return false;
			}
			if (IsBeingSleptIn(who.currentLocation))
			{
				if (!Game1.player.GetBoundingBox().Intersects(getBoundingBox(tileLocation)))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_InUse"));
				}
				return false;
			}
			return true;
		}

		public override Item getOne()
		{
			BedFurniture bedFurniture = new BedFurniture(parentSheetIndex, tileLocation);
			bedFurniture.drawPosition.Value = drawPosition;
			bedFurniture.defaultBoundingBox.Value = defaultBoundingBox;
			bedFurniture.boundingBox.Value = boundingBox;
			bedFurniture.currentRotation.Value = (int)currentRotation - 1;
			bedFurniture.isOn.Value = false;
			bedFurniture.rotations.Value = rotations;
			bedFurniture.bedType = bedType;
			bedFurniture.rotate();
			bedFurniture._GetOneFrom(this);
			return bedFurniture;
		}

		public Point GetBedSpot()
		{
			return new Point((int)tileLocation.X + 1, (int)tileLocation.Y + 1);
		}

		public override void resetOnPlayerEntry(GameLocation environment, bool dropDown = false)
		{
			UpdateBedTile(check_bounds: false);
		}

		public virtual void UpdateBedTile(bool check_bounds)
		{
			Rectangle bounding_box = getBoundingBox(tileLocation);
			if (bedType == BedType.Double)
			{
				bedTileOffset = 1;
			}
			else if (!check_bounds || !bounding_box.Intersects(Game1.player.GetBoundingBox()))
			{
				if (Game1.player.Position.X > (float)bounding_box.Center.X)
				{
					bedTileOffset = 0;
				}
				else
				{
					bedTileOffset = 1;
				}
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			mutex.Update(Game1.getOnlineFarmers());
			UpdateBedTile(check_bounds: true);
			base.updateWhenCurrentLocation(time, environment);
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (!isTemporarilyInvisible)
			{
				if (Furniture.isDrawingLocationFurniture)
				{
					Rectangle drawn_rect = sourceRect.Value;
					spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(boundingBox.Value.Top + 1) / 10000f);
					drawn_rect.X += drawn_rect.Width;
					spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(boundingBox.Value.Bottom - 1) / 10000f);
				}
				else
				{
					base.draw(spriteBatch, x, y, alpha);
				}
			}
		}

		public override bool AllowPlacementOnThisTile(int x, int y)
		{
			if (bedType == BedType.Child && (float)y == base.TileLocation.Y + 1f)
			{
				return true;
			}
			return base.AllowPlacementOnThisTile(x, y);
		}

		public override bool IntersectsForCollision(Rectangle rect)
		{
			Rectangle bounds = getBoundingBox(tileLocation);
			Rectangle current_rect = bounds;
			current_rect.Height = 64;
			if (current_rect.Intersects(rect))
			{
				return true;
			}
			current_rect = bounds;
			current_rect.Y += 128;
			current_rect.Height -= 128;
			if (current_rect.Intersects(rect))
			{
				return true;
			}
			return false;
		}

		public override int GetAdditionalTilePropertyRadius()
		{
			return 1;
		}

		public static bool IsBedHere(GameLocation location, int x, int y)
		{
			if (location == null)
			{
				return false;
			}
			ignoreContextualBedSpotOffset = true;
			if (location.doesTileHaveProperty(x, y, "Bed", "Back") != null)
			{
				ignoreContextualBedSpotOffset = false;
				return true;
			}
			ignoreContextualBedSpotOffset = false;
			return false;
		}

		public override bool DoesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
		{
			if (bedType == BedType.Double && (float)tile_x == tileLocation.X - 1f && (float)tile_y == tileLocation.Y + 1f && layer_name == "Back" && property_name == "NoFurniture")
			{
				property_value = "t";
				return true;
			}
			if ((float)tile_x >= tileLocation.X && (float)tile_x < tileLocation.X + (float)getTilesWide() && (float)tile_y == tileLocation.Y + 1f && layer_name == "Back")
			{
				if (property_name == "Bed")
				{
					property_value = "t";
					return true;
				}
				if (bedType != BedType.Child)
				{
					int bed_spot_x = (int)tileLocation.X + bedTileOffset;
					if (ignoreContextualBedSpotOffset)
					{
						bed_spot_x = (int)tileLocation.X + 1;
					}
					if (tile_x == bed_spot_x && property_name == "TouchAction")
					{
						property_value = "Sleep";
						return true;
					}
				}
			}
			return false;
		}
	}
}
