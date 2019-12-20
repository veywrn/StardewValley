using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class FarmHouse : DecoratableLocation
	{
		public int farmerNumberOfOwner;

		[XmlElement("fireplaceOn")]
		public readonly NetBool fireplaceOn = new NetBool();

		[XmlElement("fridge")]
		public readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(playerChest: true));

		public Point fridgePosition;

		private LocalizedContentManager mapLoader;

		public List<Warp> cellarWarps;

		private int currentlyDisplayedUpgradeLevel;

		private bool displayingSpouseRoom;

		[XmlIgnore]
		public virtual Farmer owner => Game1.MasterPlayer;

		[XmlIgnore]
		public virtual int upgradeLevel
		{
			get
			{
				return owner.houseUpgradeLevel;
			}
			set
			{
				owner.houseUpgradeLevel.Value = value;
			}
		}

		public FarmHouse()
		{
		}

		public FarmHouse(int ownerNumber = 1)
		{
			farmerNumberOfOwner = ownerNumber;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(fireplaceOn, fridge);
			fireplaceOn.fieldChangeVisibleEvent += delegate(NetBool field, bool oldValue, bool newValue)
			{
				Point fireplacePoint = getFireplacePoint();
				setFireplace(newValue, fireplacePoint.X, fireplacePoint.Y);
			};
		}

		public List<Child> getChildren()
		{
			return (from n in characters
				where n is Child
				select n as Child).ToList();
		}

		public int getChildrenCount()
		{
			int count = 0;
			foreach (NPC character in characters)
			{
				if (character is Child)
				{
					count++;
				}
			}
			return count;
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			foreach (Furniture f in furniture)
			{
				if ((int)f.furniture_type != 12 && f.getBoundingBox(f.tileLocation).Intersects(position))
				{
					return true;
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile: false, ignoreCharacterRequirement: false);
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
				if ((int)f.furniture_type != 12 && f.getBoundingBox(f.tileLocation).Contains((int)nonTile.X, (int)nonTile.Y))
				{
					return false;
				}
			}
			return base.isTileLocationTotallyClearAndPlaceable(v);
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			foreach (NPC c in characters)
			{
				if (c.isMarried())
				{
					if (c.getSpouse() == Game1.player)
					{
						c.checkForMarriageDialogue(timeOfDay, this);
					}
					if (Game1.IsMasterGame && Game1.timeOfDay >= 2200 && Game1.IsMasterGame && c.getTileLocationPoint() != getSpouseBedSpot(c.Name) && (timeOfDay == 2200 || (c.controller == null && timeOfDay % 100 % 30 == 0)))
					{
						c.controller = null;
						c.controller = new PathFindController(c, this, getSpouseBedSpot(c.Name), 0, spouseSleepEndFunction);
						if (c.controller.pathToEndPoint == null || !isTileOnMap(c.controller.pathToEndPoint.Last().X, c.controller.pathToEndPoint.Last().Y))
						{
							c.controller = null;
						}
					}
				}
				if (c is Child)
				{
					(c as Child).tenMinuteUpdate();
				}
			}
		}

		public static void spouseSleepEndFunction(Character c, GameLocation location)
		{
			if (c != null && c is NPC && Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions").ContainsKey(c.name.Value.ToLower() + "_sleep"))
			{
				(c as NPC).playSleepingAnimation();
			}
		}

		public virtual Point getPorchStandingSpot()
		{
			int num = farmerNumberOfOwner;
			if ((uint)num <= 1u)
			{
				return new Point(66, 15);
			}
			return new Point(-1000, -1000);
		}

		public Point getKitchenStandingSpot()
		{
			switch (upgradeLevel)
			{
			case 1:
				return new Point(4, 5);
			case 2:
			case 3:
				return new Point(7, 14);
			default:
				return new Point(-1000, -1000);
			}
		}

		public Point getSpouseBedSpot(string spouseName)
		{
			switch (upgradeLevel)
			{
			case 1:
				if (!SocialPage.isRoommateOfAnyone(spouseName))
				{
					return new Point(23, 4);
				}
				return new Point(32, 5);
			case 2:
			case 3:
				if (!SocialPage.isRoommateOfAnyone(spouseName))
				{
					return new Point(29, 13);
				}
				return new Point(38, 14);
			default:
				return new Point(-1000, -1000);
			}
		}

		public Point getBedSpot()
		{
			switch (upgradeLevel)
			{
			case 0:
				return new Point(10, 9);
			case 1:
				return new Point(22, 4);
			case 2:
			case 3:
				return new Point(28, 13);
			default:
				return new Point(-1000, -1000);
			}
		}

		public Point getEntryLocation()
		{
			switch (upgradeLevel)
			{
			case 0:
				return new Point(3, 11);
			case 1:
				return new Point(9, 11);
			case 2:
			case 3:
				return new Point(12, 20);
			default:
				return new Point(-1000, -1000);
			}
		}

		public Point getChildBed(int gender)
		{
			switch (gender)
			{
			case 0:
				return new Point(23, 5);
			case 1:
				return new Point(27, 5);
			default:
				return Point.Zero;
			}
		}

		public Point getRandomOpenPointInHouse(Random r, int buffer = 0, int tries = 30)
		{
			Point point2 = Point.Zero;
			for (int numTries = 0; numTries < tries; numTries++)
			{
				point2 = new Point(r.Next(map.Layers[0].LayerWidth), r.Next(map.Layers[0].LayerHeight));
				Microsoft.Xna.Framework.Rectangle zone = new Microsoft.Xna.Framework.Rectangle(point2.X - buffer, point2.Y - buffer, 1 + buffer * 2, 1 + buffer * 2);
				bool obstacleFound = false;
				for (int x = zone.X; x < zone.Right; x++)
				{
					for (int y = zone.Y; y < zone.Bottom; y++)
					{
						obstacleFound = (getTileIndexAt(x, y, "Back") == -1 || !isTileLocationTotallyClearAndPlaceable(x, y) || Utility.pointInRectangles(getWalls(), x, y));
						if (obstacleFound)
						{
							break;
						}
					}
					if (obstacleFound)
					{
						break;
					}
				}
				if (!obstacleFound)
				{
					return point2;
				}
			}
			return Point.Zero;
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string[] array = action.Split(' ');
				string a = array[0];
				if (a == "kitchen")
				{
					List<NetMutex> muticies = new List<NetMutex>();
					List<Chest> mini_fridges = new List<Chest>();
					foreach (Object item in objects.Values)
					{
						if (item != null && (bool)item.bigCraftable && item is Chest && item.ParentSheetIndex == 216)
						{
							mini_fridges.Add(item as Chest);
							muticies.Add((item as Chest).mutex);
						}
					}
					if (fridge.Value.mutex.IsLocked())
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
					}
					else
					{
						MultipleMutexRequest multiple_mutex_request = null;
						multiple_mutex_request = new MultipleMutexRequest(muticies, delegate
						{
							fridge.Value.mutex.RequestLock(delegate
							{
								List<Chest> list = new List<Chest>();
								list.Add(fridge);
								list.AddRange(mini_fridges);
								Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
								Game1.activeClickableMenu = new CraftingPage((int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, cooking: true, standalone_menu: true, list);
								Game1.activeClickableMenu.exitFunction = delegate
								{
									fridge.Value.mutex.ReleaseLock();
									multiple_mutex_request.ReleaseLocks();
								};
							}, delegate
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
								multiple_mutex_request.ReleaseLocks();
							});
						}, delegate
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
						});
					}
					return true;
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				switch (map.GetLayer("Buildings").Tiles[tileLocation].TileIndex)
				{
				case 173:
					fridge.Value.fridge.Value = true;
					fridge.Value.checkForAction(who);
					return true;
				case 794:
				case 795:
				case 796:
				case 797:
					fireplaceOn.Value = !fireplaceOn;
					return true;
				case 2173:
					if (Game1.player.eventsSeen.Contains(463391) && Game1.player.spouse != null && Game1.player.spouse.Equals("Emily"))
					{
						TemporaryAnimatedSprite t = getTemporarySpriteByID(5858585);
						if (t != null && t is EmilysParrot)
						{
							(t as EmilysParrot).doAction();
						}
					}
					return true;
				}
			}
			if (base.checkAction(tileLocation, viewport, who))
			{
				return true;
			}
			return false;
		}

		public FarmHouse(string m, string name)
			: base(m, name)
		{
			switch (Game1.whichFarm)
			{
			case 0:
				furniture.Add(new Furniture(1120, new Vector2(5f, 4f)));
				furniture.Last().heldObject.Value = new Furniture(1364, new Vector2(5f, 4f));
				furniture.Add(new Furniture(1376, new Vector2(1f, 10f)));
				furniture.Add(new Furniture(0, new Vector2(4f, 4f)));
				furniture.Add(new TV(1466, new Vector2(1f, 4f)));
				furniture.Add(new Furniture(1614, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1618, new Vector2(6f, 8f)));
				furniture.Add(new Furniture(1602, new Vector2(5f, 1f)));
				if (!(this is Cabin))
				{
					furniture.Add(new Furniture(1792, Utility.PointToVector2(getFireplacePoint())));
				}
				objects.Add(new Vector2(3f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(3f, 7f), giftbox: true));
				break;
			case 1:
				setWallpaper(11, -1, persist: true);
				setFloor(1, -1, persist: true);
				furniture.Add(new Furniture(1122, new Vector2(1f, 6f)));
				furniture.Last().heldObject.Value = new Furniture(1367, new Vector2(1f, 6f));
				furniture.Add(new Furniture(3, new Vector2(1f, 5f)));
				furniture.Add(new TV(1680, new Vector2(5f, 4f)));
				furniture.Add(new Furniture(1673, new Vector2(1f, 1f)));
				furniture.Add(new Furniture(1673, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1676, new Vector2(5f, 1f)));
				furniture.Add(new Furniture(1737, new Vector2(6f, 8f)));
				furniture.Add(new Furniture(1742, new Vector2(5f, 5f)));
				if (!(this is Cabin))
				{
					furniture.Add(new Furniture(1792, Utility.PointToVector2(getFireplacePoint())));
				}
				furniture.Add(new Furniture(1675, new Vector2(10f, 1f)));
				objects.Add(new Vector2(4f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(4f, 7f), giftbox: true));
				break;
			case 2:
				setWallpaper(92, -1, persist: true);
				setFloor(34, -1, persist: true);
				furniture.Add(new Furniture(1134, new Vector2(1f, 7f)));
				furniture.Last().heldObject.Value = new Furniture(1748, new Vector2(1f, 7f));
				furniture.Add(new Furniture(3, new Vector2(1f, 6f)));
				furniture.Add(new TV(1680, new Vector2(6f, 4f)));
				furniture.Add(new Furniture(1296, new Vector2(1f, 4f)));
				furniture.Add(new Furniture(1682, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1777, new Vector2(6f, 5f)));
				furniture.Add(new Furniture(1745, new Vector2(6f, 1f)));
				if (!(this is Cabin))
				{
					furniture.Add(new Furniture(1792, Utility.PointToVector2(getFireplacePoint())));
				}
				furniture.Add(new Furniture(1747, new Vector2(5f, 4f)));
				furniture.Add(new Furniture(1296, new Vector2(10f, 4f)));
				objects.Add(new Vector2(4f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(4f, 7f), giftbox: true));
				break;
			case 3:
				setWallpaper(12, -1, persist: true);
				setFloor(18, -1, persist: true);
				furniture.Add(new Furniture(1218, new Vector2(1f, 6f)));
				furniture.Last().heldObject.Value = new Furniture(1368, new Vector2(1f, 6f));
				furniture.Add(new Furniture(1755, new Vector2(1f, 5f)));
				furniture.Add(new Furniture(1755, new Vector2(3f, 6f), 1));
				furniture.Add(new TV(1680, new Vector2(5f, 4f)));
				furniture.Add(new Furniture(1751, new Vector2(5f, 10f)));
				furniture.Add(new Furniture(1749, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1753, new Vector2(5f, 1f)));
				furniture.Add(new Furniture(1742, new Vector2(5f, 5f)));
				objects.Add(new Vector2(2f, 9f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(2f, 9f), giftbox: true));
				if (!(this is Cabin))
				{
					furniture.Add(new Furniture(1794, Utility.PointToVector2(getFireplacePoint())));
				}
				break;
			case 4:
				setWallpaper(95, -1, persist: true);
				setFloor(4, -1, persist: true);
				furniture.Add(new TV(1680, new Vector2(1f, 4f)));
				furniture.Add(new Furniture(1628, new Vector2(1f, 5f)));
				furniture.Add(new Furniture(1393, new Vector2(3f, 4f)));
				furniture.Last().heldObject.Value = new Furniture(1369, new Vector2(3f, 4f));
				furniture.Add(new Furniture(1678, new Vector2(10f, 1f)));
				furniture.Add(new Furniture(1812, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1630, new Vector2(1f, 1f)));
				if (!(this is Cabin))
				{
					furniture.Add(new Furniture(1794, Utility.PointToVector2(getFireplacePoint())));
				}
				furniture.Add(new Furniture(1811, new Vector2(6f, 1f)));
				furniture.Add(new Furniture(1389, new Vector2(10f, 4f)));
				objects.Add(new Vector2(4f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(4f, 7f), giftbox: true));
				furniture.Add(new Furniture(1758, new Vector2(1f, 10f)));
				break;
			case 5:
				setWallpaper(65, -1, persist: true);
				setFloor(5, -1, persist: true);
				furniture.Add(new TV(1466, new Vector2(1f, 4f)));
				if (!(this is Cabin))
				{
					furniture.Add(new Furniture(1792, Utility.PointToVector2(getFireplacePoint())));
				}
				furniture.Add(new Furniture(1614, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1614, new Vector2(6f, 1f)));
				furniture.Add(new Furniture(1601, new Vector2(10f, 1f)));
				furniture.Add(new Furniture(202, new Vector2(3f, 4f), 1));
				furniture.Add(new Furniture(1124, new Vector2(4f, 4f), 1));
				furniture.Last().heldObject.Value = new Furniture(1379, new Vector2(5f, 4f));
				furniture.Add(new Furniture(202, new Vector2(6f, 4f), 3));
				furniture.Add(new Furniture(1378, new Vector2(10f, 4f)));
				furniture.Add(new Furniture(1377, new Vector2(1f, 9f)));
				furniture.Add(new Furniture(1445, new Vector2(1f, 10f)));
				furniture.Add(new Furniture(1618, new Vector2(2f, 9f)));
				objects.Add(new Vector2(3f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(3f, 7f), giftbox: true));
				break;
			}
		}

		public bool hasActiveFireplace()
		{
			for (int i = 0; i < furniture.Count(); i++)
			{
				if ((int)furniture[i].furniture_type == 14 && (bool)furniture[i].isOn)
				{
					return true;
				}
			}
			return false;
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			if (Game1.IsMasterGame)
			{
				foreach (NPC npc in characters)
				{
					Farmer spouse_farmer = npc.getSpouse();
					if (spouse_farmer != null && spouse_farmer == owner)
					{
						NPC spouse = npc;
						if (spouse != null && Game1.timeOfDay < 1500 && Game1.random.NextDouble() < 0.0006 && spouse.controller == null && spouse.Schedule == null && !spouse.getTileLocation().Equals(Utility.PointToVector2(getSpouseBedSpot(Game1.player.spouse))) && furniture.Count > 0)
						{
							Microsoft.Xna.Framework.Rectangle b = furniture[Game1.random.Next(furniture.Count)].boundingBox;
							Vector2 possibleLocation = new Vector2(b.X / 64, b.Y / 64);
							int tries = 0;
							int facingDirection = -3;
							for (; tries < 3; tries++)
							{
								int xMove = Game1.random.Next(-1, 2);
								int yMove = Game1.random.Next(-1, 2);
								possibleLocation.X += xMove;
								if (xMove == 0)
								{
									possibleLocation.Y += yMove;
								}
								switch (xMove)
								{
								case -1:
									facingDirection = 1;
									break;
								case 1:
									facingDirection = 3;
									break;
								default:
									switch (yMove)
									{
									case -1:
										facingDirection = 2;
										break;
									case 1:
										facingDirection = 0;
										break;
									}
									break;
								}
								if (isTileLocationTotallyClearAndPlaceable(possibleLocation))
								{
									break;
								}
							}
							if (tries < 3)
							{
								spouse.controller = new PathFindController(spouse, this, new Point((int)possibleLocation.X, (int)possibleLocation.Y), facingDirection, eraseOldPathController: false, clearMarriageDialogues: false);
							}
						}
					}
				}
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (wasUpdated)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			fridge.Value.updateWhenCurrentLocation(time, this);
			if (!Game1.player.isMarried() || Game1.player.spouse == null)
			{
				return;
			}
			NPC spouse = getCharacterFromName(Game1.player.spouse);
			if (spouse == null || spouse.isEmoting)
			{
				return;
			}
			Vector2 spousePos = spouse.getTileLocation();
			Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
			foreach (Vector2 offset in adjacentTilesOffsets)
			{
				Vector2 v = spousePos + offset;
				NPC i = isCharacterAtTile(v);
				if (i != null && i.IsMonster && !i.Name.Equals("Cat"))
				{
					spouse.faceGeneralDirection(v * new Vector2(64f, 64f));
					Game1.showSwordswipeAnimation(spouse.FacingDirection, spouse.Position, 60f, flip: false);
					localSound("swordswipe");
					spouse.shake(500);
					spouse.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:FarmHouse_SpouseAttacked" + (Game1.random.Next(12) + 1)));
					((Monster)i).takeDamage(50, (int)Utility.getAwayFromPositionTrajectory(i.GetBoundingBox(), spouse.Position).X, (int)Utility.getAwayFromPositionTrajectory(i.GetBoundingBox(), spouse.Position).Y, isBomb: false, 1.0, Game1.player);
					if (((Monster)i).Health <= 0)
					{
						debris.Add(new Debris(i.Sprite.textureName, Game1.random.Next(6, 16), new Vector2(i.getStandingX(), i.getStandingY())));
						monsterDrop((Monster)i, i.getStandingX(), i.getStandingY(), owner);
						characters.Remove(i);
						Game1.stats.MonstersKilled++;
						Game1.player.changeFriendship(-10, spouse);
					}
					else
					{
						((Monster)i).shedChunks(4);
					}
					spouse.CurrentDialogue.Clear();
					spouse.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Data\\ExtraDialogue:Spouse_MonstersInHouse"), spouse));
				}
			}
		}

		public Point getFireplacePoint()
		{
			switch (upgradeLevel)
			{
			case 0:
				return new Point(8, 4);
			case 1:
				return new Point(26, 4);
			case 2:
			case 3:
				return new Point(2, 13);
			default:
				return new Point(-50, -50);
			}
		}

		public bool shouldShowSpouseRoom()
		{
			return owner.isMarried();
		}

		public virtual void showSpouseRoom()
		{
			bool showSpouse = owner.isMarried() && owner.spouse != null;
			bool num = displayingSpouseRoom;
			displayingSpouseRoom = showSpouse;
			updateMap();
			if (num && !displayingSpouseRoom)
			{
				Microsoft.Xna.Framework.Rectangle spouseRoomBounds = default(Microsoft.Xna.Framework.Rectangle);
				switch (upgradeLevel)
				{
				case 1:
					spouseRoomBounds = new Microsoft.Xna.Framework.Rectangle(29, 4, 6, 6);
					break;
				case 2:
				case 3:
					spouseRoomBounds = new Microsoft.Xna.Framework.Rectangle(35, 13, 6, 6);
					break;
				}
				for (int x = spouseRoomBounds.X - 1; x <= spouseRoomBounds.Right; x++)
				{
					for (int y = spouseRoomBounds.Y; y <= spouseRoomBounds.Bottom; y++)
					{
						Vector2 tile = new Vector2(x, y);
						for (int i = furniture.Count - 1; i >= 0; i--)
						{
							if (furniture[i].tileLocation.Equals(tile))
							{
								Game1.createItemDebris(furniture[i], new Vector2(spouseRoomBounds.X, spouseRoomBounds.Center.Y) * 64f, 3);
								furniture.RemoveAt(i);
							}
						}
					}
				}
			}
			loadObjects();
			if (upgradeLevel == 3)
			{
				setMapTileIndex(3, 22, 162, "Front");
				removeTile(4, 22, "Front");
				removeTile(5, 22, "Front");
				setMapTileIndex(6, 22, 163, "Front");
				setMapTileIndex(3, 23, 64, "Buildings");
				setMapTileIndex(3, 24, 96, "Buildings");
				setMapTileIndex(4, 24, 165, "Front");
				setMapTileIndex(5, 24, 165, "Front");
				removeTile(4, 23, "Back");
				removeTile(5, 23, "Back");
				setMapTileIndex(4, 23, 1043, "Back");
				setMapTileIndex(5, 23, 1043, "Back");
				setMapTileIndex(4, 24, 1075, "Back");
				setMapTileIndex(5, 24, 1075, "Back");
				setMapTileIndex(6, 23, 68, "Buildings");
				setMapTileIndex(6, 24, 130, "Buildings");
				setMapTileIndex(4, 25, 0, "Front");
				setMapTileIndex(5, 25, 0, "Front");
				removeTile(4, 23, "Buildings");
				removeTile(5, 23, "Buildings");
				createCellarWarps();
				if (!Game1.player.craftingRecipes.ContainsKey("Cask"))
				{
					Game1.player.craftingRecipes.Add("Cask", 0);
				}
			}
			if (showSpouse)
			{
				loadSpouseRoom();
			}
		}

		public string GetCellarName()
		{
			int cellar_number = -1;
			if (owner != null)
			{
				foreach (int i in Game1.player.team.cellarAssignments.Keys)
				{
					if (Game1.player.team.cellarAssignments[i] == owner.UniqueMultiplayerID)
					{
						cellar_number = i;
					}
				}
			}
			if (cellar_number >= 0 && cellar_number <= 1)
			{
				return "Cellar";
			}
			return "Cellar" + cellar_number;
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (Game1.timeOfDay >= 2200 && owner.spouse != null && getCharacterFromName(owner.spouse) != null && !owner.isEngaged())
			{
				Game1.player.team.requestSpouseSleepEvent.Fire(owner.UniqueMultiplayerID);
			}
			if (Game1.timeOfDay >= 2000 && owner.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID && Game1.getFarm().farmers.Count <= 1)
			{
				Game1.player.team.requestPetWarpHomeEvent.Fire(owner.UniqueMultiplayerID);
			}
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farm farm = Game1.getFarm();
			for (int l = characters.Count - 1; l >= 0; l--)
			{
				if (characters[l] is Pet && (!isTileOnMap(characters[l].getTileX(), characters[l].getTileY()) || getTileIndexAt(characters[l].getTileLocationPoint(), "Buildings") != -1 || getTileIndexAt(characters[l].getTileX() + 1, characters[l].getTileY(), "Buildings") != -1))
				{
					characters[l].faceDirection(2);
					Game1.warpCharacter(characters[l], "Farm", farm.GetPetStartLocation());
					break;
				}
			}
			for (int k = characters.Count - 1; k >= 0; k--)
			{
				for (int j = k - 1; j >= 0; j--)
				{
					if (k < characters.Count && j < characters.Count && (characters[j].Equals(characters[k]) || (characters[j].Name.Equals(characters[k].Name) && characters[j].isVillager() && characters[k].isVillager())) && j != k)
					{
						characters.RemoveAt(j);
					}
				}
				for (int i = farm.characters.Count - 1; i >= 0; i--)
				{
					if (k < characters.Count && i < characters.Count && farm.characters[i].Equals(characters[k]))
					{
						farm.characters.RemoveAt(i);
					}
				}
			}
		}

		public void updateFarmLayout()
		{
			if (currentlyDisplayedUpgradeLevel != upgradeLevel)
			{
				setMapForUpgradeLevel(upgradeLevel);
			}
			if ((!displayingSpouseRoom && shouldShowSpouseRoom()) || (displayingSpouseRoom && !shouldShowSpouseRoom()))
			{
				showSpouseRoom();
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (owner.isMarried() && owner.spouse != null && owner.spouse.Equals("Emily") && Game1.player.eventsSeen.Contains(463391))
			{
				Vector2 parrotSpot = new Vector2(2064f, 160f);
				int upgradeLevel = this.upgradeLevel;
				if ((uint)(upgradeLevel - 2) <= 1u)
				{
					parrotSpot = new Vector2(2448f, 736f);
				}
				temporarySprites.Add(new EmilysParrot(parrotSpot));
			}
			updateFarmLayout();
			setWallpapers();
			setFloors();
			if (Game1.player.currentLocation == null || (!Game1.player.currentLocation.Equals(this) && !Game1.player.currentLocation.name.Value.StartsWith("Cellar")))
			{
				switch (this.upgradeLevel)
				{
				case 1:
					Game1.player.Position = new Vector2(9f, 11f) * 64f;
					break;
				case 2:
				case 3:
					Game1.player.Position = new Vector2(12f, 20f) * 64f;
					break;
				}
				Game1.xLocationAfterWarp = Game1.player.getTileX();
				Game1.yLocationAfterWarp = Game1.player.getTileY();
				Game1.player.currentLocation = this;
			}
			foreach (NPC i in characters)
			{
				if (i is Child)
				{
					(i as Child).resetForPlayerEntry(this);
				}
				if (Game1.IsMasterGame && Game1.timeOfDay >= 2000 && !(i is Pet))
				{
					i.controller = null;
					i.Halt();
				}
			}
			if (owner == Game1.player && Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).HasValue && Game1.player.team.IsMarried(Game1.player.UniqueMultiplayerID) && !Game1.player.mailReceived.Contains("CF_Spouse"))
			{
				Vector2 chestPosition = Utility.PointToVector2(getBedSpot()) + new Vector2(-3f, 0f);
				if (this.upgradeLevel == 1)
				{
					chestPosition = Utility.PointToVector2(getBedSpot()) + new Vector2(3f, 0f);
				}
				Chest chest = new Chest(0, new List<Item>
				{
					new Object(434, 1)
				}, chestPosition, giftbox: true, 1);
				overlayObjects[chestPosition] = chest;
			}
			if (owner != null && !owner.activeDialogueEvents.ContainsKey("pennyRedecorating"))
			{
				int whichQuilt2 = -1;
				if (owner.mailReceived.Contains("pennyQuilt0"))
				{
					whichQuilt2 = 0;
				}
				else if (owner.mailReceived.Contains("pennyQuilt1"))
				{
					whichQuilt2 = 1;
				}
				else if (owner.mailReceived.Contains("pennyQuilt2"))
				{
					whichQuilt2 = 2;
				}
				if (whichQuilt2 != -1)
				{
					Point startTile = Point.Zero;
					if (this.upgradeLevel >= 2)
					{
						startTile = new Point(27, 12);
					}
					else if (this.upgradeLevel == 1)
					{
						startTile = new Point(21, 3);
					}
					if (!startTile.Equals(Point.Zero))
					{
						int startIndex = 61 + whichQuilt2 * 3;
						setMapTileIndex(startTile.X, startTile.Y, startIndex, "Front", 1);
						setMapTileIndex(startTile.X + 1, startTile.Y, startIndex + 1, "Front", 1);
						setMapTileIndex(startTile.X + 2, startTile.Y, startIndex + 2, "Front", 1);
						setMapTileIndex(startTile.X, startTile.Y + 1, startIndex + 12, "Front", 1);
						setMapTileIndex(startTile.X + 1, startTile.Y + 1, startIndex + 13, "Front", 1);
						setMapTileIndex(startTile.X + 2, startTile.Y + 1, startIndex + 14, "Front", 1);
					}
				}
			}
			if (owner.Equals(Game1.player) && !Game1.player.activeDialogueEvents.ContainsKey("pennyRedecorating"))
			{
				int whichQuilt = -1;
				if (Game1.player.mailReceived.Contains("pennyQuilt0"))
				{
					whichQuilt = 0;
				}
				else if (Game1.player.mailReceived.Contains("pennyQuilt1"))
				{
					whichQuilt = 1;
				}
				else if (Game1.player.mailReceived.Contains("pennyQuilt2"))
				{
					whichQuilt = 2;
				}
				if (whichQuilt != -1 && !Game1.player.mailReceived.Contains("pennyRefurbished"))
				{
					Game1.player.mailReceived.Add("pennyRefurbished");
					Microsoft.Xna.Framework.Rectangle roomToRedecorate2 = Microsoft.Xna.Framework.Rectangle.Empty;
					roomToRedecorate2 = ((this.upgradeLevel >= 2) ? new Microsoft.Xna.Framework.Rectangle(23, 10, 11, 13) : new Microsoft.Xna.Framework.Rectangle(20, 1, 8, 10));
					List<Object> objectsPickedUp = new List<Object>();
					for (int x = roomToRedecorate2.X; x <= roomToRedecorate2.Right; x++)
					{
						for (int y = roomToRedecorate2.Y; y <= roomToRedecorate2.Bottom; y++)
						{
							if (getObjectAtTile(x, y) == null)
							{
								continue;
							}
							Object o2 = null;
							o2 = getObjectAtTile(x, y);
							if (o2 != null && !(o2 is Chest) && !(o2 is StorageFurniture))
							{
								if (o2.Name != null && o2.Name.Contains("Table") && o2.heldObject.Value != null)
								{
									Object held_object = o2.heldObject.Value;
									o2.heldObject.Value = null;
									objectsPickedUp.Add(held_object);
								}
								o2.performRemoveAction(new Vector2(x, y), this);
								objectsPickedUp.Add(getObjectAtTile(x, y));
								objects.Remove(new Vector2(x, y));
								if (o2 is Furniture)
								{
									furniture.Remove(o2 as Furniture);
								}
							}
						}
					}
					decoratePennyRoom(whichQuilt, objectsPickedUp);
				}
			}
			if (owner.getSpouse() == null || !owner.getSpouse().name.Equals("Sebastian") || !Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
			{
				return;
			}
			Vector2 spot = new Vector2((this.upgradeLevel == 1) ? 30 : 36, (this.upgradeLevel == 1) ? 7 : 16);
			removeTile((int)spot.X, (int)spot.Y - 1, "Front");
			removeTile((int)spot.X + 1, (int)spot.Y - 1, "Front");
			removeTile((int)spot.X + 2, (int)spot.Y - 1, "Front");
			temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.mouseCursors,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
				animationLength = 1,
				sourceRectStartingPos = new Vector2(641f, 1534f),
				interval = 5000f,
				totalNumberOfLoops = 9999,
				position = spot * 64f + new Vector2(0f, -5f) * 4f,
				scale = 4f,
				layerDepth = (spot.Y + 2f + 0.1f) * 64f / 10000f
			});
			if (Game1.random.NextDouble() < 0.85)
			{
				Texture2D crittersText3 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
				base.TemporarySprites.Add(new SebsFrogs
				{
					texture = crittersText3,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 224, 16, 16),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(64f, 224f),
					interval = 100f,
					totalNumberOfLoops = 9999,
					position = spot * 64f + new Vector2((Game1.random.NextDouble() < 0.5) ? 22 : 25, (!(Game1.random.NextDouble() < 0.5)) ? 1 : 2) * 4f,
					scale = 4f,
					flipped = (Game1.random.NextDouble() < 0.5),
					layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
					Parent = this
				});
			}
			if (!Game1.player.activeDialogueEvents.ContainsKey("sebastianFrog2") && Game1.random.NextDouble() < 0.5)
			{
				Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
				base.TemporarySprites.Add(new SebsFrogs
				{
					texture = crittersText2,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 240, 16, 16),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(64f, 240f),
					interval = 150f,
					totalNumberOfLoops = 9999,
					position = spot * 64f + new Vector2(8f, 3f) * 4f,
					scale = 4f,
					layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
					flipped = (Game1.random.NextDouble() < 0.5),
					pingPong = false,
					Parent = this
				});
				if (Game1.random.NextDouble() < 0.1 && Game1.timeOfDay > 610)
				{
					DelayedAction.playSoundAfterDelay("croak", 1000);
				}
			}
		}

		private void addFurnitureIfSpaceIsFreePenny(List<Object> objectsToStoreInChests, Furniture f, Furniture heldObject = null)
		{
			bool fail = false;
			foreach (Furniture furniture in base.furniture)
			{
				if (f.getBoundingBox(f.tileLocation).Intersects(furniture.getBoundingBox(furniture.tileLocation)))
				{
					fail = true;
					break;
				}
			}
			if (objects.ContainsKey(f.TileLocation))
			{
				fail = true;
			}
			if (!fail)
			{
				base.furniture.Add(f);
				if (heldObject != null)
				{
					base.furniture.Last().heldObject.Value = heldObject;
				}
			}
			else
			{
				objectsToStoreInChests.Add(f);
				if (heldObject != null)
				{
					objectsToStoreInChests.Add(heldObject);
				}
			}
		}

		private void addFurnitureIfSpaceIsFree(Furniture f, Furniture heldObject = null)
		{
			if (!objects.ContainsKey(f.TileLocation))
			{
				furniture.Add(f);
				if (heldObject != null)
				{
					furniture.Last().heldObject.Value = heldObject;
				}
			}
		}

		private void decoratePennyRoom(int whichStyle, List<Object> objectsToStoreInChests)
		{
			List<Chest> chests = new List<Chest>();
			List<Vector2> chest_positions = new List<Vector2>();
			Color chest_color = default(Color);
			switch (whichStyle)
			{
			case 0:
				if (upgradeLevel == 1)
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(20f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1914, new Vector2(21f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1915, new Vector2(22f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1914, new Vector2(23f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(24f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1682, new Vector2(26f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1747, new Vector2(25f, 4f)));
					if (!(this is Cabin))
					{
						addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1395, new Vector2(26f, 4f)), new Furniture(1363, Vector2.Zero));
						addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1443, new Vector2(27f, 4f)));
					}
					else
					{
						objectsToStoreInChests.Add(new Furniture(1395, new Vector2(26f, 4f)));
						objectsToStoreInChests.Add(new Furniture(1363, Vector2.Zero));
						objectsToStoreInChests.Add(new Furniture(1443, new Vector2(27f, 4f)));
					}
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1664, new Vector2(27f, 5f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1978, new Vector2(21f, 6f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1124, new Vector2(26f, 9f)), new Furniture(1368, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(6, new Vector2(25f, 10f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1296, new Vector2(28f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1747, new Vector2(24f, 10f)));
					setWallpaper(107, 2, persist: true);
					setFloor(2, 3, persist: true);
					chest_color = new Color(85, 85, 255);
					chest_positions.Add(new Vector2(21f, 10f));
					chest_positions.Add(new Vector2(22f, 10f));
				}
				else
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(23f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1914, new Vector2(24f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1604, new Vector2(26f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1915, new Vector2(28f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(30f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1914, new Vector2(32f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(33f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1443, new Vector2(23f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1747, new Vector2(24f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1395, new Vector2(25f, 13f)), new Furniture(1363, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(714, new Vector2(31f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1443, new Vector2(33f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1978, new Vector2(27f, 15f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1664, new Vector2(32f, 15f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1664, new Vector2(23f, 17f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1124, new Vector2(31f, 21f)), new Furniture(1368, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(416, new Vector2(25f, 22f), 2));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1296, new Vector2(23f, 22f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(6, new Vector2(30f, 22f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1296, new Vector2(33f, 22f)));
					setWallpaper(107, 6, persist: true);
					setFloor(2, 6, persist: true);
					chest_color = new Color(85, 85, 255);
					chest_positions.Add(new Vector2(23f, 14f));
					chest_positions.Add(new Vector2(24f, 14f));
				}
				break;
			case 1:
				if (upgradeLevel == 1)
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1678, new Vector2(20f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(21f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(22f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(23f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1907, new Vector2(24f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1400, new Vector2(25f, 4f)), new Furniture(1365, Vector2.Zero));
					if (!(this is Cabin))
					{
						addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1866, new Vector2(26f, 4f)));
					}
					else
					{
						objectsToStoreInChests.Add(new Furniture(1866, new Vector2(26f, 4f)));
					}
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1909, new Vector2(27f, 6f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1451, new Vector2(21f, 6f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1138, new Vector2(27f, 9f)), new Furniture(1378, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(12, new Vector2(26f, 10f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1758, new Vector2(24f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1618, new Vector2(21f, 9f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1390, new Vector2(22f, 10f)));
					setWallpaper(84, 2, persist: true);
					setFloor(35, 3, persist: true);
					chest_color = new Color(255, 85, 85);
					chest_positions.Add(new Vector2(21f, 10f));
					chest_positions.Add(new Vector2(23f, 10f));
				}
				else
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1678, new Vector2(24f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1907, new Vector2(25f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(27f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(28f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(29f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1907, new Vector2(30f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(33f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1758, new Vector2(23f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1400, new Vector2(25f, 13f)), new Furniture(1365, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1390, new Vector2(31f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1866, new Vector2(32f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1387, new Vector2(23f, 14f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1909, new Vector2(32f, 14f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(719, new Vector2(23f, 15f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1451, new Vector2(27f, 15f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1909, new Vector2(23f, 17f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1389, new Vector2(32f, 19f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1377, new Vector2(33f, 19f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1758, new Vector2(26f, 20f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(424, new Vector2(27f, 20f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1618, new Vector2(29f, 20f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(536, new Vector2(32f, 20f), 3));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1138, new Vector2(23f, 21f)), new Furniture(1378, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1383, new Vector2(26f, 21f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1449, new Vector2(33f, 22f)));
					setWallpaper(84, 6, persist: true);
					setFloor(35, 6, persist: true);
					chest_color = new Color(255, 85, 85);
					chest_positions.Add(new Vector2(24f, 13f));
					chest_positions.Add(new Vector2(28f, 15f));
				}
				break;
			case 2:
				if (upgradeLevel == 1)
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1673, new Vector2(20f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1547, new Vector2(21f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1675, new Vector2(24f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1900, new Vector2(25f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1393, new Vector2(25f, 4f)), new Furniture(1367, Vector2.Zero));
					if (!(this is Cabin))
					{
						addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1798, new Vector2(26f, 4f)));
					}
					else
					{
						objectsToStoreInChests.Add(new Furniture(1798, new Vector2(26f, 4f)));
					}
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1902, new Vector2(25f, 5f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1751, new Vector2(22f, 6f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1122, new Vector2(26f, 9f)), new Furniture(1378, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(197, new Vector2(28f, 9f), 3));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(3, new Vector2(25f, 10f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1294, new Vector2(20f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1294, new Vector2(24f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1964, new Vector2(21f, 8f)));
					setWallpaper(95, 2, persist: true);
					setFloor(1, 3, persist: true);
					chest_color = new Color(85, 85, 85);
					chest_positions.Add(new Vector2(22f, 10f));
					chest_positions.Add(new Vector2(23f, 10f));
				}
				else
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1673, new Vector2(23f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1675, new Vector2(25f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1547, new Vector2(27f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1900, new Vector2(30f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1751, new Vector2(23f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1393, new Vector2(25f, 13f)), new Furniture(1367, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1798, new Vector2(32f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1902, new Vector2(31f, 14f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1964, new Vector2(27f, 15f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1294, new Vector2(23f, 16f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(3, new Vector2(31f, 19f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1294, new Vector2(23f, 20f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1122, new Vector2(31f, 20f)), new Furniture(1369, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(197, new Vector2(33f, 20f), 3));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(709, new Vector2(23f, 21f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(3, new Vector2(32f, 22f), 2));
					setWallpaper(95, 6, persist: true);
					setFloor(1, 6, persist: true);
					chest_color = new Color(85, 85, 85);
					chest_positions.Add(new Vector2(24f, 13f));
					chest_positions.Add(new Vector2(31f, 13f));
				}
				break;
			}
			if (objectsToStoreInChests != null)
			{
				foreach (Object o in objectsToStoreInChests)
				{
					if (chests.Count == 0)
					{
						chests.Add(new Chest(playerChest: true));
					}
					bool found_chest_to_stash_in = false;
					foreach (Chest item in chests)
					{
						if (item.addItem(o) == null)
						{
							found_chest_to_stash_in = true;
						}
					}
					if (!found_chest_to_stash_in)
					{
						Chest new_chest = new Chest(playerChest: true);
						chests.Add(new_chest);
						new_chest.addItem(o);
					}
				}
			}
			for (int i = 0; i < chests.Count; i++)
			{
				Chest chest = chests[i];
				chest.playerChoiceColor.Value = chest_color;
				Vector2 chest_position = chest_positions[Math.Min(i, chest_positions.Count - 1)];
				PlaceInNearbySpace(chest_position, chest);
			}
		}

		public void PlaceInNearbySpace(Vector2 tileLocation, Object o)
		{
			if (o == null || tileLocation.Equals(Vector2.Zero))
			{
				return;
			}
			int attempts = 0;
			Queue<Vector2> open_list = new Queue<Vector2>();
			HashSet<Vector2> closed_list = new HashSet<Vector2>();
			open_list.Enqueue(tileLocation);
			Vector2 current = Vector2.Zero;
			for (; attempts < 100; attempts++)
			{
				current = open_list.Dequeue();
				if (!isTileOccupiedForPlacement(current) && isTileLocationTotallyClearAndPlaceable(current) && !isOpenWater((int)current.X, (int)current.Y))
				{
					break;
				}
				closed_list.Add(current);
				foreach (Vector2 v in Utility.getAdjacentTileLocations(current))
				{
					if (!closed_list.Contains(v))
					{
						open_list.Enqueue(v);
					}
				}
			}
			if (!current.Equals(Vector2.Zero) && !isTileOccupiedForPlacement(current) && !isOpenWater((int)current.X, (int)current.Y) && isTileLocationTotallyClearAndPlaceable(current))
			{
				o.tileLocation.Value = current;
				objects.Add(current, o);
			}
		}

		public void moveObjectsForHouseUpgrade(int whichUpgrade)
		{
			overlayObjects.Clear();
			switch (whichUpgrade)
			{
			case 0:
				if (upgradeLevel == 1)
				{
					shiftObjects(-6, 0);
				}
				break;
			case 1:
				if (upgradeLevel == 0)
				{
					shiftObjects(6, 0);
				}
				if (upgradeLevel == 2)
				{
					shiftObjects(-3, 0);
				}
				break;
			case 2:
			case 3:
				if (upgradeLevel == 1)
				{
					shiftObjects(3, 9);
					foreach (Furniture v in furniture)
					{
						if (v.tileLocation.X >= 10f && v.tileLocation.X <= 13f && v.tileLocation.Y >= 10f && v.tileLocation.Y <= 11f)
						{
							v.tileLocation.X -= 3f;
							v.boundingBox.X -= 192;
							v.tileLocation.Y -= 9f;
							v.boundingBox.Y -= 576;
							v.updateDrawPosition();
						}
					}
					moveFurniture(27, 13, 1, 4);
					moveFurniture(28, 13, 2, 4);
					moveFurniture(29, 13, 3, 4);
					moveFurniture(28, 14, 7, 4);
					moveFurniture(29, 14, 8, 4);
					moveFurniture(27, 14, 4, 4);
					moveFurniture(28, 15, 5, 4);
					moveFurniture(29, 16, 6, 4);
				}
				if (upgradeLevel == 0)
				{
					shiftObjects(9, 9);
				}
				break;
			}
		}

		protected override LocalizedContentManager getMapLoader()
		{
			if (mapLoader == null)
			{
				mapLoader = Game1.game1.xTileContent.CreateTemporary();
			}
			return mapLoader;
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			base.drawAboveFrontLayer(b);
			if (fridge.Value.mutex.IsLocked())
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(fridgePosition.X, fridgePosition.Y - 1) * 64f), new Microsoft.Xna.Framework.Rectangle(0, 192, 16, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((fridgePosition.Y + 1) * 64 + 1) / 10000f);
			}
		}

		public override void updateMap()
		{
			bool showSpouse = owner.spouse != null && owner.isMarried();
			mapPath.Value = "Maps\\" + base.Name + ((upgradeLevel == 0) ? "" : ((upgradeLevel == 3) ? "2" : string.Concat(upgradeLevel))) + (showSpouse ? "_marriage" : "");
			base.updateMap();
		}

		public virtual void setMapForUpgradeLevel(int level)
		{
			upgradeLevel = level;
			currentlyDisplayedUpgradeLevel = level;
			bool showSpouse = owner.isMarried() && owner.spouse != null;
			if (displayingSpouseRoom && !showSpouse)
			{
				displayingSpouseRoom = false;
			}
			updateMap();
			if (showSpouse)
			{
				showSpouseRoom();
			}
			loadObjects();
			if (level == 3)
			{
				setMapTileIndex(3, 22, 162, "Front");
				removeTile(4, 22, "Front");
				removeTile(5, 22, "Front");
				setMapTileIndex(6, 22, 163, "Front");
				setMapTileIndex(3, 23, 64, "Buildings");
				setMapTileIndex(3, 24, 96, "Buildings");
				setMapTileIndex(4, 24, 165, "Front");
				setMapTileIndex(5, 24, 165, "Front");
				removeTile(4, 23, "Back");
				removeTile(5, 23, "Back");
				setMapTileIndex(4, 23, 1043, "Back");
				setMapTileIndex(5, 23, 1043, "Back");
				setTileProperty(4, 23, "Back", "NoFurniture", "t");
				setTileProperty(5, 23, "Back", "NoFurniture", "t");
				setTileProperty(4, 23, "Back", "NPCBarrier", "t");
				setTileProperty(5, 23, "Back", "NPCBarrier", "t");
				setMapTileIndex(4, 24, 1075, "Back");
				setMapTileIndex(5, 24, 1075, "Back");
				setTileProperty(4, 24, "Back", "NoFurniture", "t");
				setTileProperty(5, 24, "Back", "NoFurniture", "t");
				setMapTileIndex(6, 23, 68, "Buildings");
				setMapTileIndex(6, 24, 130, "Buildings");
				setMapTileIndex(4, 25, 0, "Front");
				setMapTileIndex(5, 25, 0, "Front");
				removeTile(4, 23, "Buildings");
				removeTile(5, 23, "Buildings");
				createCellarWarps();
				if (!Game1.player.craftingRecipes.ContainsKey("Cask"))
				{
					Game1.player.craftingRecipes.Add("Cask", 0);
				}
			}
			if (wallPaper.Count > 0 && floor.Count > 0)
			{
				if (upgradeLevel >= 1 && floor.Count <= 1)
				{
					setFloor(floor[0], 1, persist: true);
					setFloor(floor[0], 2, persist: true);
					setFloor(floor[0], 3, persist: true);
					setFloor(22, 0, persist: true);
				}
				if (upgradeLevel >= 2 && wallPaper.Count <= 3)
				{
					setWallpaper(wallPaper[0], 4, persist: true);
					setWallpaper(wallPaper[2], 6, persist: true);
					setWallpaper(wallPaper[1], 5, persist: true);
					setWallpaper(11, 0, persist: true);
					setWallpaper(61, 1, persist: true);
					setWallpaper(61, 2, persist: true);
				}
				if (upgradeLevel >= 2 && floor.Count <= 4)
				{
					int bedroomFloor = floor[3];
					setFloor(floor[2], 5, persist: true);
					setFloor(floor[0], 3, persist: true);
					setFloor(floor[1], 4, persist: true);
					setFloor(bedroomFloor, 6, persist: true);
					setFloor(1, 0, persist: true);
					setFloor(31, 1, persist: true);
					setFloor(31, 2, persist: true);
				}
			}
			lightGlows.Clear();
			fridgePosition = default(Point);
			bool found_fridge = false;
			for (int x = 0; x < map.GetLayer("Buildings").LayerWidth; x++)
			{
				for (int y = 0; y < map.GetLayer("Buildings").LayerHeight; y++)
				{
					if (map.GetLayer("Buildings").Tiles[x, y] != null && map.GetLayer("Buildings").Tiles[x, y].TileIndex == 173)
					{
						fridgePosition = new Point(x, y);
						found_fridge = true;
						break;
					}
				}
				if (found_fridge)
				{
					break;
				}
			}
		}

		public void createCellarWarps()
		{
			cellarWarps = new List<Warp>();
			cellarWarps.Add(new Warp(4, 25, GetCellarName(), 3, 2, flipFarmer: false));
			cellarWarps.Add(new Warp(5, 25, GetCellarName(), 4, 2, flipFarmer: false));
			updateCellarWarps();
		}

		public void updateCellarWarps()
		{
			if (cellarWarps != null)
			{
				foreach (Warp warp in cellarWarps)
				{
					if (!warps.Contains(warp))
					{
						warps.Add(warp);
					}
					warp.TargetName = GetCellarName();
				}
			}
		}

		public virtual void loadSpouseRoom()
		{
			NPC spouse = owner.getSpouse();
			if (spouse == null)
			{
				return;
			}
			int indexInSpouseMapSheet = -1;
			switch (spouse.Name)
			{
			case "Abigail":
				indexInSpouseMapSheet = 0;
				break;
			case "Penny":
				indexInSpouseMapSheet = 1;
				break;
			case "Leah":
				indexInSpouseMapSheet = 2;
				break;
			case "Haley":
				indexInSpouseMapSheet = 3;
				break;
			case "Maru":
				indexInSpouseMapSheet = 4;
				break;
			case "Sebastian":
				indexInSpouseMapSheet = 5;
				break;
			case "Alex":
				indexInSpouseMapSheet = 6;
				break;
			case "Harvey":
				indexInSpouseMapSheet = 7;
				break;
			case "Elliott":
				indexInSpouseMapSheet = 8;
				break;
			case "Sam":
				indexInSpouseMapSheet = 9;
				break;
			case "Shane":
				indexInSpouseMapSheet = 10;
				break;
			case "Emily":
				indexInSpouseMapSheet = 11;
				break;
			case "Krobus":
				indexInSpouseMapSheet = 12;
				break;
			}
			Microsoft.Xna.Framework.Rectangle areaToRefurbish = (upgradeLevel == 1) ? new Microsoft.Xna.Framework.Rectangle(29, 1, 6, 9) : new Microsoft.Xna.Framework.Rectangle(35, 10, 6, 9);
			Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\spouseRooms");
			Point mapReader = new Point(indexInSpouseMapSheet % 5 * 6, indexInSpouseMapSheet / 5 * 9);
			map.Properties.Remove("DayTiles");
			map.Properties.Remove("NightTiles");
			for (int x = 0; x < areaToRefurbish.Width; x++)
			{
				for (int y = 0; y < areaToRefurbish.Height; y++)
				{
					if (refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(map.GetLayer("Back"), map.GetTileSheet(refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
					}
					if (refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(map.GetLayer("Buildings"), map.GetTileSheet(refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
						adjustMapLightPropertiesForLamp(refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings");
					}
					else
					{
						map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
					}
					if (y < areaToRefurbish.Height - 1 && refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(map.GetLayer("Front"), map.GetTileSheet(refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
						adjustMapLightPropertiesForLamp(refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front");
					}
					else if (y < areaToRefurbish.Height - 1)
					{
						map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
					}
					if (x == 4 && y == 4)
					{
						map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y].Properties["NoFurniture"] = new PropertyValue("T");
					}
				}
			}
		}

		public void playerDivorced()
		{
			displayingSpouseRoom = false;
		}

		public virtual List<Microsoft.Xna.Framework.Rectangle> getForbiddenPetWarpTiles()
		{
			List<Microsoft.Xna.Framework.Rectangle> forbidden_tiles = new List<Microsoft.Xna.Framework.Rectangle>();
			switch (upgradeLevel)
			{
			case 0:
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(2, 8, 3, 4));
				break;
			case 1:
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(8, 8, 3, 4));
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(17, 8, 4, 3));
				break;
			case 2:
			case 3:
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(11, 17, 3, 4));
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(20, 17, 4, 3));
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(12, 5, 4, 3));
				forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(11, 7, 2, 6));
				break;
			}
			return forbidden_tiles;
		}

		public bool canPetWarpHere(Vector2 tile_position)
		{
			foreach (Microsoft.Xna.Framework.Rectangle forbiddenPetWarpTile in getForbiddenPetWarpTiles())
			{
				if (forbiddenPetWarpTile.Contains((int)tile_position.X, (int)tile_position.Y))
				{
					return false;
				}
			}
			return true;
		}

		public override List<Microsoft.Xna.Framework.Rectangle> getWalls()
		{
			List<Microsoft.Xna.Framework.Rectangle> walls = new List<Microsoft.Xna.Framework.Rectangle>();
			switch (upgradeLevel)
			{
			case 0:
				walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 10, 3));
				break;
			case 1:
				walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 17, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(18, 6, 2, 2));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(20, 1, 9, 3));
				break;
			case 2:
			case 3:
				walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 12, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(15, 1, 13, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(13, 3, 2, 2));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 10, 10, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(13, 10, 8, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 15, 2, 2));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(23, 10, 11, 3));
				break;
			}
			return walls;
		}

		public override List<Microsoft.Xna.Framework.Rectangle> getFloors()
		{
			List<Microsoft.Xna.Framework.Rectangle> floors = new List<Microsoft.Xna.Framework.Rectangle>();
			switch (upgradeLevel)
			{
			case 0:
				floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 10, 9));
				break;
			case 1:
				floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 6, 9));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(7, 3, 11, 9));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(18, 8, 2, 2));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(20, 3, 9, 8));
				break;
			case 2:
			case 3:
				floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 12, 6));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(15, 3, 13, 6));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(13, 5, 2, 2));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 12, 10, 11));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(10, 12, 11, 9));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 17, 2, 2));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(23, 12, 12, 11));
				break;
			}
			return floors;
		}
	}
}
