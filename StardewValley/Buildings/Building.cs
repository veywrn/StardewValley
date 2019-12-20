using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Buildings
{
	[XmlInclude(typeof(Coop))]
	[XmlInclude(typeof(Barn))]
	[XmlInclude(typeof(Stable))]
	[XmlInclude(typeof(Mill))]
	[XmlInclude(typeof(JunimoHut))]
	[XmlInclude(typeof(ShippingBin))]
	[XmlInclude(typeof(FishPond))]
	public class Building : INetObject<NetFields>
	{
		[XmlIgnore]
		public Lazy<Texture2D> texture;

		[XmlElement("indoors")]
		public readonly NetRef<GameLocation> indoors = new NetRef<GameLocation>();

		[XmlElement("tileX")]
		public readonly NetInt tileX = new NetInt();

		[XmlElement("tileY")]
		public readonly NetInt tileY = new NetInt();

		[XmlElement("tilesWide")]
		public readonly NetInt tilesWide = new NetInt();

		[XmlElement("tilesHigh")]
		public readonly NetInt tilesHigh = new NetInt();

		[XmlElement("maxOccupants")]
		public readonly NetInt maxOccupants = new NetInt();

		[XmlElement("currentOccupants")]
		public readonly NetInt currentOccupants = new NetInt();

		[XmlElement("daysOfConstructionLeft")]
		public readonly NetInt daysOfConstructionLeft = new NetInt();

		[XmlElement("daysUntilUpgrade")]
		public readonly NetInt daysUntilUpgrade = new NetInt();

		[XmlElement("buildingType")]
		public readonly NetString buildingType = new NetString();

		protected int _isCabin = -1;

		[Obsolete]
		public string baseNameOfIndoors;

		[XmlElement("humanDoor")]
		public readonly NetPoint humanDoor = new NetPoint();

		[XmlElement("animalDoor")]
		public readonly NetPoint animalDoor = new NetPoint();

		[XmlElement("color")]
		public readonly NetColor color = new NetColor(Color.White);

		[XmlElement("animalDoorOpen")]
		public readonly NetBool animalDoorOpen = new NetBool();

		[XmlElement("magical")]
		public readonly NetBool magical = new NetBool();

		[XmlElement("fadeWhenPlayerIsBehind")]
		public readonly NetBool fadeWhenPlayerIsBehind = new NetBool(value: true);

		[XmlElement("owner")]
		public readonly NetLong owner = new NetLong();

		[XmlElement("newConstructionTimer")]
		protected readonly NetInt newConstructionTimer = new NetInt();

		[XmlElement("alpha")]
		protected readonly NetFloat alpha = new NetFloat();

		[XmlIgnore]
		protected bool _isMoving;

		public static Rectangle leftShadow = new Rectangle(656, 394, 16, 16);

		public static Rectangle middleShadow = new Rectangle(672, 394, 16, 16);

		public static Rectangle rightShadow = new Rectangle(688, 394, 16, 16);

		public bool isCabin
		{
			get
			{
				if (_isCabin == -1)
				{
					if (indoors.Value != null && indoors.Value is Cabin)
					{
						_isCabin = 1;
					}
					else
					{
						_isCabin = 0;
					}
				}
				return _isCabin == 1;
			}
		}

		public string nameOfIndoors
		{
			get
			{
				GameLocation indoors = this.indoors.Get();
				if (indoors == null)
				{
					return "null";
				}
				return indoors.uniqueName;
			}
		}

		public string nameOfIndoorsWithoutUnique
		{
			get
			{
				if (indoors.Value == null)
				{
					return "null";
				}
				return getBuildingMapFileName(indoors.Value.Name);
			}
		}

		public bool isMoving
		{
			get
			{
				return _isMoving;
			}
			set
			{
				if (_isMoving != value)
				{
					_isMoving = value;
					if (_isMoving)
					{
						OnStartMove();
					}
					if (!_isMoving)
					{
						OnEndMove();
					}
				}
			}
		}

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public Building()
		{
			resetTexture();
			initNetFields();
		}

		public Building(BluePrint blueprint, Vector2 tileLocation)
			: this()
		{
			tileX.Value = (int)tileLocation.X;
			tileY.Value = (int)tileLocation.Y;
			tilesWide.Value = blueprint.tilesWidth;
			tilesHigh.Value = blueprint.tilesHeight;
			buildingType.Value = blueprint.name;
			humanDoor.Value = blueprint.humanDoor;
			animalDoor.Value = blueprint.animalDoor;
			indoors.Value = getIndoors(getBuildingMapFileName(blueprint.mapToWarpTo));
			maxOccupants.Value = blueprint.maxOccupants;
			daysOfConstructionLeft.Value = blueprint.daysToConstruct;
			magical.Value = blueprint.magical;
			alpha.Value = 1f;
		}

		public virtual bool hasCarpenterPermissions()
		{
			if (Game1.IsMasterGame)
			{
				return true;
			}
			if (owner.Value == Game1.player.UniqueMultiplayerID)
			{
				return true;
			}
			if (isCabin && indoors.Value is Cabin && (indoors.Value as Cabin).owner == Game1.player)
			{
				return true;
			}
			return false;
		}

		protected virtual string getBuildingMapFileName(string name)
		{
			if (name == "Slime Hutch")
			{
				return "SlimeHutch";
			}
			if (name == "Big Coop")
			{
				return "Coop2";
			}
			if (name == "Deluxe Coop")
			{
				return "Coop3";
			}
			if (name == "Big Barn")
			{
				return "Barn2";
			}
			if (name == "Deluxe Barn")
			{
				return "Barn3";
			}
			if (name == "Big Shed")
			{
				return "Shed2";
			}
			return name;
		}

		protected virtual void initNetFields()
		{
			NetFields.AddFields(indoors, tileX, tileY, tilesWide, tilesHigh, maxOccupants, currentOccupants, daysOfConstructionLeft, daysUntilUpgrade, buildingType, humanDoor, animalDoor, magical, animalDoorOpen, owner, newConstructionTimer);
			buildingType.fieldChangeVisibleEvent += delegate
			{
				resetTexture();
			};
		}

		public virtual string textureName()
		{
			return "Buildings\\" + buildingType;
		}

		public virtual void resetTexture()
		{
			texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>(textureName()));
		}

		public int getTileSheetIndexForStructurePlacementTile(int x, int y)
		{
			if (x == humanDoor.X && y == humanDoor.Y)
			{
				return 2;
			}
			if (x == animalDoor.X && y == animalDoor.Y)
			{
				return 4;
			}
			return 0;
		}

		public virtual void performTenMinuteAction(int timeElapsed)
		{
		}

		public virtual void resetLocalState()
		{
			color.Value = Color.White;
			isMoving = false;
		}

		public virtual bool leftClicked()
		{
			return false;
		}

		public virtual bool doAction(Vector2 tileLocation, Farmer who)
		{
			if (who.IsLocalPlayer && tileLocation.X >= (float)(int)tileX && tileLocation.X < (float)((int)tileX + (int)tilesWide) && tileLocation.Y >= (float)(int)tileY && tileLocation.Y < (float)((int)tileY + (int)tilesHigh) && (int)daysOfConstructionLeft > 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:UnderConstruction"));
			}
			else
			{
				if (who.IsLocalPlayer && tileLocation.X == (float)(humanDoor.X + (int)tileX) && tileLocation.Y == (float)(humanDoor.Y + (int)tileY) && indoors.Value != null)
				{
					if (who.mount != null)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:DismountBeforeEntering"));
						return false;
					}
					if (who.team.demolishLock.IsLocked())
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantEnter"));
						return false;
					}
					indoors.Value.isStructure.Value = true;
					who.currentLocation.playSoundAt("doorClose", tileLocation);
					Game1.warpFarmer(indoors.Value.uniqueName.Value, indoors.Value.warps[0].X, indoors.Value.warps[0].Y - 1, Game1.player.FacingDirection, isStructure: true);
					return true;
				}
				if (who.IsLocalPlayer && buildingType.Equals("Silo") && !isTilePassable(tileLocation))
				{
					if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 178)
					{
						if (who.ActiveObject.Stack == 0)
						{
							who.ActiveObject.stack.Value = 1;
						}
						int old = who.ActiveObject.Stack;
						int leftOver = (Game1.getLocationFromName("Farm") as Farm).tryToAddHay(who.ActiveObject.Stack);
						who.ActiveObject.stack.Value = leftOver;
						if ((int)who.ActiveObject.stack < old)
						{
							Game1.playSound("Ship");
							DelayedAction.playSoundAfterDelay("grassyStep", 100);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:AddedHay", old - who.ActiveObject.Stack));
						}
						if (who.ActiveObject.Stack <= 0)
						{
							who.removeItemFromInventory(who.ActiveObject);
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:PiecesOfHay", (Game1.getLocationFromName("Farm") as Farm).piecesOfHay, Utility.numSilos() * 240));
					}
				}
				else
				{
					if (who.IsLocalPlayer && buildingType.Value.Contains("Obelisk") && !isTilePassable(tileLocation))
					{
						if (buildingType.Value == "Desert Obelisk" && Game1.player.isRidingHorse() && Game1.player.mount != null)
						{
							Game1.player.mount.checkAction(Game1.player, Game1.player.currentLocation);
						}
						else
						{
							for (int j = 0; j < 12; j++)
							{
								who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
							}
							who.currentLocation.playSound("wand");
							Game1.displayFarmer = false;
							Game1.player.temporarilyInvincible = true;
							Game1.player.temporaryInvincibilityTimer = -2000;
							Game1.player.freezePause = 1000;
							Game1.flashAlpha = 1f;
							DelayedAction.fadeAfterDelay(obeliskWarpForReal, 1000);
							new Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
							int i = 0;
							for (int x = who.getTileX() + 8; x >= who.getTileX() - 8; x--)
							{
								who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x, who.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
								{
									layerDepth = 1f,
									delayBeforeAnimationStart = i * 25,
									motion = new Vector2(-0.25f, 0f)
								});
								i++;
							}
						}
						return true;
					}
					if (who.IsLocalPlayer && who.ActiveObject != null && !isTilePassable(tileLocation))
					{
						return performActiveObjectDropInAction(who, probe: false);
					}
				}
			}
			return false;
		}

		private void obeliskWarpForReal()
		{
			switch ((string)buildingType)
			{
			case "Earth Obelisk":
				Game1.warpFarmer("Mountain", 31, 20, flip: false);
				break;
			case "Water Obelisk":
				Game1.warpFarmer("Beach", 20, 4, flip: false);
				break;
			case "Desert Obelisk":
				Game1.warpFarmer("Desert", 35, 43, flip: false);
				break;
			}
			Game1.fadeToBlackAlpha = 0.99f;
			Game1.screenGlow = false;
			Game1.player.temporarilyInvincible = false;
			Game1.player.temporaryInvincibilityTimer = 0;
			Game1.displayFarmer = true;
		}

		public virtual bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			if (humanDoor.X >= 0 && xTile == (int)tileX + humanDoor.X && yTile == (int)tileY + humanDoor.Y)
			{
				return true;
			}
			if (animalDoor.X >= 0 && xTile == (int)tileX + animalDoor.X && yTile == (int)tileY + animalDoor.Y)
			{
				return true;
			}
			return false;
		}

		public virtual void performActionOnBuildingPlacement()
		{
			Farm farm = Game1.getLocationFromName("Farm") as Farm;
			if (farm == null)
			{
				return;
			}
			for (int y = 0; y < (int)tilesHigh; y++)
			{
				for (int x = 0; x < (int)tilesWide; x++)
				{
					Vector2 currentGlobalTilePosition = new Vector2((int)tileX + x, (int)tileY + y);
					farm.terrainFeatures.Remove(currentGlobalTilePosition);
				}
			}
		}

		public virtual void performActionOnConstruction(GameLocation location)
		{
			load();
			location.playSound("axchop");
			newConstructionTimer.Value = (((bool)magical || (int)daysOfConstructionLeft <= 0) ? 2000 : 1000);
			if (!magical)
			{
				location.playSound("axchop");
				for (int x2 = tileX; x2 < (int)tileX + (int)tilesWide; x2++)
				{
					for (int y2 = tileY; y2 < (int)tileY + (int)tilesHigh; y2++)
					{
						for (int j = 0; j < 5; j++)
						{
							location.temporarySprites.Add(new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 46 : 12, new Vector2(x2, y2) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextDouble() < 0.5)
							{
								delayBeforeAnimationStart = Math.Max(0, Game1.random.Next(-200, 400)),
								motion = new Vector2(0f, -1f),
								interval = Game1.random.Next(50, 80)
							});
						}
						location.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2(x2, y2) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextDouble() < 0.5));
					}
				}
				for (int k = 0; k < 8; k++)
				{
					DelayedAction.playSoundAfterDelay("dirtyHit", 250 + k * 150);
				}
				return;
			}
			for (int i = 0; i < 8; i++)
			{
				DelayedAction.playSoundAfterDelay("dirtyHit", 100 + i * 210);
			}
			Game1.flashAlpha = 2f;
			location.playSound("wand");
			for (int x = 0; x < getSourceRectForMenu().Width / 16 * 2; x++)
			{
				for (int y = getSourceRect().Height / 16 * 2; y >= 0; y--)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(666, 1851, 8, 8), 40f, 4, 2, new Vector2((int)tileX, (int)tileY) * 64f + new Vector2(x * 64 / 2, y * 64 / 2 - getSourceRect().Height * 4 + (int)tilesHigh * 64) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flicker: false, flipped: false)
					{
						layerDepth = (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + (float)x / 10000f,
						pingPong = true,
						delayBeforeAnimationStart = (getSourceRect().Height / 16 * 2 - y) * 100,
						scale = 4f,
						alphaFade = 0.01f,
						color = Color.AliceBlue
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(666, 1851, 8, 8), 40f, 4, 2, new Vector2((int)tileX, (int)tileY) * 64f + new Vector2(x * 64 / 2, y * 64 / 2 - getSourceRect().Height * 4 + (int)tilesHigh * 64) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flicker: false, flipped: false)
					{
						layerDepth = (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + (float)x / 10000f + 0.0001f,
						pingPong = true,
						delayBeforeAnimationStart = (getSourceRect().Height / 16 * 2 - y) * 100,
						scale = 4f,
						alphaFade = 0.01f,
						color = Color.AliceBlue
					});
				}
			}
		}

		public virtual void performActionOnDemolition(GameLocation location)
		{
			if (indoors.Value != null)
			{
				Game1.multiplayer.broadcastRemoveLocationFromLookup(indoors.Value);
			}
			indoors.Value = null;
		}

		public virtual void performActionOnUpgrade(GameLocation location)
		{
		}

		public virtual string isThereAnythingtoPreventConstruction(GameLocation location)
		{
			return null;
		}

		public virtual bool performActiveObjectDropInAction(Farmer who, bool probe)
		{
			return false;
		}

		public virtual void performToolAction(Tool t, int tileX, int tileY)
		{
		}

		public virtual void updateWhenFarmNotCurrentLocation(GameTime time)
		{
			if ((int)newConstructionTimer > 0)
			{
				newConstructionTimer.Value -= time.ElapsedGameTime.Milliseconds;
				if ((int)newConstructionTimer <= 0 && (bool)magical)
				{
					daysOfConstructionLeft.Value = 0;
				}
			}
		}

		public virtual void Update(GameTime time)
		{
			alpha.Value = Math.Min(1f, alpha.Value + 0.05f);
			int tilesHigh = this.tilesHigh.Get();
			if (fadeWhenPlayerIsBehind.Value && Game1.player.GetBoundingBox().Intersects(new Rectangle(64 * (int)tileX, 64 * ((int)tileY + (-(getSourceRectForMenu().Height / 16) + tilesHigh)), (int)tilesWide * 64, (getSourceRectForMenu().Height / 16 - tilesHigh) * 64 + 32)))
			{
				alpha.Value = Math.Max(0.4f, alpha.Value - 0.09f);
			}
		}

		public virtual void showUpgradeAnimation(GameLocation location)
		{
			color.Value = Color.White;
			location.temporarySprites.Add(new TemporaryAnimatedSprite(46, getUpgradeSignLocation() + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), Color.Beige, 10, Game1.random.NextDouble() < 0.5, 75f)
			{
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2(-0.02f, 0.01f),
				delayBeforeAnimationStart = Game1.random.Next(100),
				layerDepth = 0.89f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite(46, getUpgradeSignLocation() + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), Color.Beige, 10, Game1.random.NextDouble() < 0.5, 75f)
			{
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2(-0.02f, 0.01f),
				delayBeforeAnimationStart = Game1.random.Next(40),
				layerDepth = 0.89f
			});
		}

		public virtual Vector2 getUpgradeSignLocation()
		{
			if (indoors.Value != null && indoors.Value is Shed)
			{
				return new Vector2((int)tileX + 5, (int)tileY + 1) * 64f + new Vector2(-12f, -16f);
			}
			return new Vector2((int)tileX * 64 + 32, (int)tileY * 64 - 32);
		}

		public virtual string getNameOfNextUpgrade()
		{
			switch (buildingType.Value.ToLower())
			{
			case "coop":
				return "Big Coop";
			case "big coop":
				return "Deluxe Coop";
			case "barn":
				return "Big Barn";
			case "big barn":
				return "Deluxe Barn";
			case "shed":
				return "Big Shed";
			default:
				return "well";
			}
		}

		public virtual void showDestroyedAnimation(GameLocation location)
		{
			for (int x = tileX; x < (int)tileX + (int)tilesWide; x++)
			{
				for (int y = tileY; y < (int)tileY + (int)tilesHigh; y++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x * 64, y * 64) + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
					{
						delayBeforeAnimationStart = Game1.random.Next(300)
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x * 64, y * 64) + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
					{
						delayBeforeAnimationStart = 250 + Game1.random.Next(300)
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(276, 1985, 12, 11), new Vector2(x, y) * 64f + new Vector2(32f, -32f) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 16)), flipped: false, 0f, Color.White)
					{
						interval = 30f,
						totalNumberOfLoops = 99999,
						animationLength = 4,
						scale = 4f,
						alphaFade = 0.01f
					});
				}
			}
		}

		public virtual void dayUpdate(int dayOfMonth)
		{
			if ((int)daysOfConstructionLeft > 0 && !Utility.isFestivalDay(dayOfMonth, Game1.currentSeason))
			{
				daysOfConstructionLeft.Value--;
				if ((int)daysOfConstructionLeft > 0)
				{
					return;
				}
				Game1.player.checkForQuestComplete(null, -1, -1, null, buildingType, 8);
				if (buildingType.Equals("Slime Hutch") && indoors.Value != null)
				{
					indoors.Value.objects.Add(new Vector2(1f, 4f), new Object(new Vector2(1f, 4f), 156)
					{
						Fragility = 2
					});
					if (!Game1.player.mailReceived.Contains("slimeHutchBuilt"))
					{
						Game1.player.mailReceived.Add("slimeHutchBuilt");
					}
				}
				return;
			}
			if ((int)daysUntilUpgrade > 0 && !Utility.isFestivalDay(dayOfMonth, Game1.currentSeason))
			{
				daysUntilUpgrade.Value--;
				if ((int)daysUntilUpgrade <= 0)
				{
					Game1.player.checkForQuestComplete(null, -1, -1, null, getNameOfNextUpgrade(), 8);
					BluePrint CurrentBlueprint = new BluePrint(getNameOfNextUpgrade());
					indoors.Value.mapPath.Value = "Maps\\" + CurrentBlueprint.mapToWarpTo;
					indoors.Value.name.Value = CurrentBlueprint.mapToWarpTo;
					buildingType.Value = CurrentBlueprint.name;
					if (indoors.Value is AnimalHouse)
					{
						((AnimalHouse)(GameLocation)indoors).resetPositionsOfAllAnimals();
						((AnimalHouse)(GameLocation)indoors).animalLimit.Value += 4;
						((AnimalHouse)(GameLocation)indoors).loadLights();
					}
					upgrade();
					resetTexture();
				}
			}
			if (indoors.Value != null)
			{
				indoors.Value.DayUpdate(dayOfMonth);
			}
			if (buildingType.Value.Contains("Deluxe"))
			{
				(indoors.Value as AnimalHouse).feedAllAnimals();
			}
		}

		public virtual void upgrade()
		{
			if (buildingType.Equals("Big Shed"))
			{
				(indoors.Value as Shed).setUpgradeLevel(1);
				updateInteriorWarps(indoors.Value);
			}
		}

		public Rectangle getSourceRect()
		{
			if (buildingType.Value.Contains("Cabin"))
			{
				int upgrade = 0;
				Cabin cabin = indoors.Value as Cabin;
				if (cabin != null)
				{
					upgrade = Math.Min(cabin.upgradeLevel, 2);
				}
				return new Rectangle(upgrade * 80, 0, 80, 112);
			}
			return texture.Value.Bounds;
		}

		public virtual Rectangle getSourceRectForMenu()
		{
			return getSourceRect();
		}

		public virtual void updateInteriorWarps(GameLocation interior = null)
		{
			if (interior == null)
			{
				interior = indoors.Value;
			}
			if (interior != null)
			{
				foreach (Warp warp in interior.warps)
				{
					warp.TargetX = humanDoor.X + (int)tileX;
					warp.TargetY = humanDoor.Y + (int)tileY + 1;
				}
			}
		}

		protected virtual GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
		{
			GameLocation lcl_indoors = null;
			if (buildingType.Value.Equals("Slime Hutch"))
			{
				lcl_indoors = new SlimeHutch("Maps\\" + nameOfIndoorsWithoutUnique, buildingType);
			}
			else if (buildingType.Value.Equals("Shed"))
			{
				lcl_indoors = new Shed("Maps\\" + nameOfIndoorsWithoutUnique, buildingType);
			}
			else if (buildingType.Value.Equals("Big Shed"))
			{
				lcl_indoors = new Shed("Maps\\" + nameOfIndoorsWithoutUnique, buildingType);
			}
			else if (buildingType.Value.Contains("Cabin"))
			{
				lcl_indoors = new Cabin("Maps\\Cabin");
			}
			else if (nameOfIndoorsWithoutUnique != null && nameOfIndoorsWithoutUnique.Length > 0 && !nameOfIndoorsWithoutUnique.Equals("null"))
			{
				lcl_indoors = new GameLocation("Maps\\" + nameOfIndoorsWithoutUnique, buildingType);
			}
			if (lcl_indoors != null)
			{
				lcl_indoors.uniqueName.Value = nameOfIndoorsWithoutUnique + StardewValley.Util.GuidHelper.NewGuid().ToString();
				lcl_indoors.IsFarm = true;
				lcl_indoors.isStructure.Value = true;
				updateInteriorWarps(lcl_indoors);
			}
			return lcl_indoors;
		}

		public virtual Point getPointForHumanDoor()
		{
			return new Point((int)tileX + humanDoor.Value.X, (int)tileY + humanDoor.Value.Y);
		}

		public virtual Rectangle getRectForHumanDoor()
		{
			return new Rectangle(getPointForHumanDoor().X * 64, getPointForHumanDoor().Y * 64, 64, 64);
		}

		public virtual Rectangle getRectForAnimalDoor()
		{
			return new Rectangle((animalDoor.X + (int)tileX) * 64, ((int)tileY + animalDoor.Y) * 64, 64, 64);
		}

		public virtual void load()
		{
			GameLocation baseLocation = getIndoors(nameOfIndoorsWithoutUnique);
			if (baseLocation != null)
			{
				baseLocation.characters.Set(indoors.Value.characters);
				baseLocation.netObjects.MoveFrom(indoors.Value.netObjects);
				baseLocation.terrainFeatures.MoveFrom(indoors.Value.terrainFeatures);
				baseLocation.IsFarm = true;
				baseLocation.IsOutdoors = false;
				baseLocation.isStructure.Value = true;
				baseLocation.miniJukeboxCount.Set(indoors.Value.miniJukeboxCount.Value);
				baseLocation.miniJukeboxTrack.Set(indoors.Value.miniJukeboxTrack.Value);
				baseLocation.uniqueName.Value = indoors.Value.uniqueName;
				if (baseLocation.uniqueName.Value == null)
				{
					baseLocation.uniqueName.Value = nameOfIndoorsWithoutUnique + ((int)tileX * 2000 + (int)tileY);
				}
				baseLocation.numberOfSpawnedObjectsOnMap = indoors.Value.numberOfSpawnedObjectsOnMap;
				if (indoors.Value is Shed)
				{
					((Shed)baseLocation).upgradeLevel.Set(((Shed)indoors.Value).upgradeLevel.Value);
				}
				if (indoors.Value is AnimalHouse)
				{
					((AnimalHouse)baseLocation).animals.MoveFrom(((AnimalHouse)indoors.Value).animals);
					((AnimalHouse)baseLocation).animalsThatLiveHere.Set(((AnimalHouse)indoors.Value).animalsThatLiveHere);
					foreach (KeyValuePair<long, FarmAnimal> pair in ((AnimalHouse)baseLocation).animals.Pairs)
					{
						pair.Value.reload(this);
					}
				}
				if (indoors.Value is DecoratableLocation)
				{
					((DecoratableLocation)baseLocation).furniture.Set(((DecoratableLocation)indoors.Value).furniture);
					foreach (Furniture item in ((DecoratableLocation)baseLocation).furniture)
					{
						item.updateDrawPosition();
					}
					((DecoratableLocation)baseLocation).wallPaper.Set(((DecoratableLocation)indoors.Value).wallPaper);
					((DecoratableLocation)baseLocation).floor.Set(((DecoratableLocation)indoors.Value).floor);
				}
				if (indoors.Value is Cabin)
				{
					Cabin cabin = baseLocation as Cabin;
					cabin.fridge.Value = (indoors.Value as Cabin).fridge.Value;
					cabin.fireplaceOn.Value = (indoors.Value as Cabin).fireplaceOn.Value;
					cabin.farmhand.Set((indoors.Value as Cabin).farmhand);
					if (cabin.farmhand.Value != null)
					{
						SaveGame.loadDataToFarmer(cabin.farmhand);
						cabin.resetFarmhandState();
					}
				}
				indoors.Value = baseLocation;
				baseLocation = null;
				updateInteriorWarps();
				for (int i = indoors.Value.characters.Count - 1; i >= 0; i--)
				{
					SaveGame.initializeCharacter(indoors.Value.characters[i], indoors.Value);
				}
				foreach (TerrainFeature value in indoors.Value.terrainFeatures.Values)
				{
					value.loadSprite();
				}
				foreach (KeyValuePair<Vector2, Object> v in indoors.Value.objects.Pairs)
				{
					v.Value.initializeLightSource(v.Key);
					v.Value.reloadSprite();
				}
				if (indoors.Value is AnimalHouse)
				{
					AnimalHouse a = indoors.Value as AnimalHouse;
					string a2 = buildingType.Value.Split(' ')[0];
					if (!(a2 == "Big"))
					{
						if (a2 == "Deluxe")
						{
							a.animalLimit.Value = 12;
						}
						else
						{
							a.animalLimit.Value = 4;
						}
					}
					else
					{
						a.animalLimit.Value = 8;
					}
				}
			}
			BluePrint blueprint = new BluePrint(buildingType.Value);
			if (blueprint != null)
			{
				humanDoor.X = blueprint.humanDoor.X;
				humanDoor.Y = blueprint.humanDoor.Y;
			}
		}

		public bool isUnderConstruction()
		{
			return (int)daysOfConstructionLeft > 0;
		}

		public bool occupiesTile(Vector2 tile)
		{
			if (tile.X >= (float)(int)tileX && tile.X < (float)((int)tileX + (int)tilesWide) && tile.Y >= (float)(int)tileY)
			{
				return tile.Y < (float)((int)tileY + (int)tilesHigh);
			}
			return false;
		}

		public virtual bool isTilePassable(Vector2 tile)
		{
			if (isCabin && occupiesTile(tile) && (int)tile.Y == (int)tileY + (int)tilesHigh - 1)
			{
				return true;
			}
			return !occupiesTile(tile);
		}

		public virtual bool isTileOccupiedForPlacement(Vector2 tile, Object to_place)
		{
			if (!isTilePassable(tile))
			{
				if (isCabin && to_place != null && (int)tile.Y == (int)tileY + (int)tilesHigh - 1)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public virtual bool isTileFishable(Vector2 tile)
		{
			return false;
		}

		public virtual bool CanRefillWateringCan()
		{
			if ((int)daysOfConstructionLeft <= 0 && buildingType.Equals("Well"))
			{
				return true;
			}
			return false;
		}

		public virtual bool intersects(Rectangle boundingBox)
		{
			if (isCabin && (int)daysOfConstructionLeft <= 0)
			{
				if (new Rectangle(((int)tileX + 4) * 64, ((int)tileY + (int)tilesHigh - 1) * 64, 64, 64).Intersects(boundingBox))
				{
					return true;
				}
				return new Rectangle((int)tileX * 64, (int)tileY * 64, (int)tilesWide * 64, ((int)tilesHigh - 1) * 64).Intersects(boundingBox);
			}
			return new Rectangle((int)tileX * 64, (int)tileY * 64, (int)tilesWide * 64, (int)tilesHigh * 64).Intersects(boundingBox);
		}

		public virtual void drawInMenu(SpriteBatch b, int x, int y)
		{
			if ((int)tilesWide <= 8)
			{
				drawShadow(b, x, y);
				b.Draw(texture.Value, new Vector2(x, y), getSourceRect(), color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
			}
			else
			{
				int xOffset = 108;
				int yOffset = 28;
				b.Draw(texture.Value, new Vector2(x + xOffset, y + yOffset), new Rectangle(getSourceRect().Width / 2 - 64, getSourceRect().Height - 136 - 2, 122, 138), color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
			}
		}

		public virtual void draw(SpriteBatch b)
		{
			if (isMoving)
			{
				return;
			}
			if ((int)daysOfConstructionLeft > 0 || (int)newConstructionTimer > 0)
			{
				drawInConstruction(b);
				return;
			}
			drawShadow(b);
			float draw_layer = (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f;
			if (isCabin)
			{
				draw_layer = (float)(((int)tileY + ((int)tilesHigh - 1)) * 64) / 10000f;
			}
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), getSourceRect(), color.Value * alpha, 0f, new Vector2(0f, getSourceRect().Height), 4f, SpriteEffects.None, draw_layer);
			if ((bool)magical && buildingType.Value.Equals("Gold Clock"))
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 92, (int)tileY * 64 - 40)), Town.hourHandSource, Color.White * alpha, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1200) / 1200f) + (double)((float)Game1.gameTimeInterval / 7000f / 23f)), new Vector2(2.5f, 8f), 3f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 92, (int)tileY * 64 - 40)), Town.minuteHandSource, Color.White * alpha, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1000 % 100 % 60) / 60f) + (double)((float)Game1.gameTimeInterval / 7000f * 1.02f)), new Vector2(2.5f, 12f), 3f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.00011f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 92, (int)tileY * 64 - 40)), Town.clockNub, Color.White * alpha, 0f, new Vector2(2f, 2f), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.00012f);
			}
			if ((int)daysUntilUpgrade > 0 && indoors.Value != null && indoors.Value is Shed)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, getUpgradeSignLocation()), new Rectangle(367, 309, 16, 15), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
			}
		}

		public virtual void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
		{
			Vector2 basePosition = (localX == -1) ? Game1.GlobalToLocal(new Vector2((int)tileX * 64, ((int)tileY + (int)tilesHigh) * 64)) : new Vector2(localX, localY + getSourceRectForMenu().Height * 4);
			b.Draw(Game1.mouseCursors, basePosition, leftShadow, Color.White * ((localX == -1) ? ((float)alpha) : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			for (int x = 1; x < (int)tilesWide - 1; x++)
			{
				b.Draw(Game1.mouseCursors, basePosition + new Vector2(x * 64, 0f), middleShadow, Color.White * ((localX == -1) ? ((float)alpha) : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			}
			b.Draw(Game1.mouseCursors, basePosition + new Vector2(((int)tilesWide - 1) * 64, 0f), rightShadow, Color.White * ((localX == -1) ? ((float)alpha) : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
		}

		public virtual void OnStartMove()
		{
		}

		public virtual void OnEndMove()
		{
		}

		public Point getPorchStandingSpot()
		{
			if (isCabin)
			{
				return new Point((int)tileX + 1, (int)tileY + (int)tilesHigh - 1);
			}
			return new Point(0, 0);
		}

		public virtual bool doesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
		{
			if (isCabin)
			{
				if (tile_x == getMailboxPosition().X && tile_y == getMailboxPosition().Y && property_name == "Action" && layer_name == "Buildings")
				{
					property_value = "Mailbox";
					return true;
				}
				if (tile_x == getPointForHumanDoor().X && tile_y == getPointForHumanDoor().Y && property_name == "Action" && layer_name == "Buildings")
				{
					property_value = "Warp  3 11" + indoors.Value.uniqueName.Value;
					return true;
				}
				if (tile_y == (int)tileY + (int)tilesHigh - 1)
				{
					if (property_name == "NoSpawn")
					{
						property_value = "All";
						return true;
					}
					if (property_name == "Buildable")
					{
						property_value = "f";
						return true;
					}
					if (property_name == "NoFurniture")
					{
						if (tile_x == (int)tileX + 1)
						{
							property_value = "T";
							return true;
						}
						if (tile_x == (int)tileX + 2)
						{
							property_value = "T";
							return true;
						}
						if (tile_x == (int)tileX + 4)
						{
							property_value = "T";
							return true;
						}
					}
					if (property_name == "Diggable" && layer_name == "Back")
					{
						property_value = null;
						return true;
					}
					if (property_name == "Type" && layer_name == "Back")
					{
						property_value = "Wood";
						return true;
					}
				}
			}
			return false;
		}

		public Point getMailboxPosition()
		{
			if (isCabin)
			{
				return new Point((int)tileX + (int)tilesWide - 1, (int)tileY + (int)tilesHigh - 1);
			}
			return new Point(68, 16);
		}

		public void removeOverlappingBushes(GameLocation location)
		{
			for (int x = tileX; x < (int)tileX + (int)tilesWide; x++)
			{
				for (int y = tileY; y < (int)tileY + (int)tilesHigh; y++)
				{
					if (location.isTerrainFeatureAt(x, y))
					{
						LargeTerrainFeature large_feature = location.getLargeTerrainFeatureAt(x, y);
						if (large_feature != null && large_feature is Bush)
						{
							location.largeTerrainFeatures.Remove(large_feature);
						}
					}
				}
			}
		}

		public virtual void drawInConstruction(SpriteBatch b)
		{
			int drawPercentage = Math.Min(16, Math.Max(0, (int)(16f - (float)(int)newConstructionTimer / 1000f * 16f)));
			float drawPercentageReal = (float)(2000 - (int)newConstructionTimer) / 2000f;
			if ((bool)magical || (int)daysOfConstructionLeft <= 0)
			{
				drawShadow(b);
				int yPos = (int)((float)(getSourceRect().Height * 4) * (1f - drawPercentageReal));
				b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64 + yPos + 4 - yPos % 4)), new Rectangle(0, getSourceRect().Bottom - (int)(drawPercentageReal * (float)getSourceRect().Height), getSourceRectForMenu().Width, (int)((float)getSourceRect().Height * drawPercentageReal)), color.Value * alpha, 0f, new Vector2(0f, getSourceRect().Height), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f);
				if ((bool)magical)
				{
					for (int j = 0; j < (int)tilesWide * 4; j++)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + j * 16, (float)((int)tileY * 64 - getSourceRect().Height * 4 + (int)tilesHigh * 64) + (float)(getSourceRect().Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) - ((j % 2 == 0) ? 32 : 8)), new Rectangle(536 + ((int)newConstructionTimer + j * 4) % 56 / 8 * 8, 1945, 8, 8), (j % 2 == 1) ? (Color.Pink * alpha) : (Color.LightPink * alpha), 0f, new Vector2(0f, 0f), 4f + (float)Game1.random.Next(100) / 100f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
						if (j % 2 == 0)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + j * 16, (float)((int)tileY * 64 - getSourceRect().Height * 4 + (int)tilesHigh * 64) + (float)(getSourceRect().Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) + ((j % 2 == 0) ? 32 : 8)), new Rectangle(536 + ((int)newConstructionTimer + j * 4) % 56 / 8 * 8, 1945, 8, 8), Color.White * alpha, 0f, new Vector2(0f, 0f), 4f + (float)Game1.random.Next(100) / 100f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
						}
					}
					return;
				}
				for (int i = 0; i < (int)tilesWide * 4; i++)
				{
					b.Draw(Game1.animations, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 - 16 + i * 16, (float)((int)tileY * 64 - getSourceRect().Height * 4 + (int)tilesHigh * 64) + (float)(getSourceRect().Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) - ((i % 2 == 0) ? 32 : 8)), new Rectangle(((int)newConstructionTimer + i * 20) % 304 / 38 * 64, 768, 64, 64), Color.White * alpha * ((float)(int)newConstructionTimer / 500f), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
					if (i % 2 == 0)
					{
						b.Draw(Game1.animations, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 - 16 + i * 16, (float)((int)tileY * 64 - getSourceRect().Height * 4 + (int)tilesHigh * 64) + (float)(getSourceRect().Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) - ((i % 2 == 0) ? 32 : 8)), new Rectangle(((int)newConstructionTimer + i * 20) % 400 / 50 * 64, 2944, 64, 64), Color.White * alpha * ((float)(int)newConstructionTimer / 500f), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
					}
				}
				return;
			}
			bool drawFloor = (int)daysOfConstructionLeft == 1;
			for (int x = tileX; x < (int)tileX + (int)tilesWide; x++)
			{
				for (int y = tileY; y < (int)tileY + (int)tilesHigh; y++)
				{
					if (x == (int)tileX + (int)tilesWide / 2 && y == (int)tileY + (int)tilesHigh - 1)
					{
						if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16 - 4), new Rectangle(367, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(367, 309, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
					}
					else if (x == (int)tileX && y == (int)tileY)
					{
						if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Rectangle(351, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(351, 293, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
					}
					else if (x == (int)tileX + (int)tilesWide - 1 && y == (int)tileY)
					{
						if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Rectangle(383, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(383, 293, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
					}
					else if (x == (int)tileX + (int)tilesWide - 1 && y == (int)tileY + (int)tilesHigh - 1)
					{
						if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Rectangle(383, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(383, 325, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
					}
					else if (x == (int)tileX && y == (int)tileY + (int)tilesHigh - 1)
					{
						if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Rectangle(351, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(351, 325, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
					}
					else if (x == (int)tileX + (int)tilesWide - 1)
					{
						if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Rectangle(383, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(383, 309, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
					}
					else if (y == (int)tileY + (int)tilesHigh - 1)
					{
						if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Rectangle(367, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(367, 325, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
					}
					else if (x == (int)tileX)
					{
						if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Rectangle(351, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(351, 309, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
					}
					else if (y == (int)tileY)
					{
						if (drawFloor)
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Rectangle(367, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(367, 293, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
					}
					else if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Rectangle(367, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
				}
			}
		}
	}
}
