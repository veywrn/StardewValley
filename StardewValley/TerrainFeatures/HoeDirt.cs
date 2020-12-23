using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	public class HoeDirt : TerrainFeature
	{
		private struct NeighborLoc
		{
			public readonly Vector2 Offset;

			public readonly byte Direction;

			public readonly byte InvDirection;

			public NeighborLoc(Vector2 a, byte b, byte c)
			{
				Offset = a;
				Direction = b;
				InvDirection = c;
			}
		}

		private struct Neighbor
		{
			public readonly HoeDirt feature;

			public readonly byte direction;

			public readonly byte invDirection;

			public Neighbor(HoeDirt a, byte b, byte c)
			{
				feature = a;
				direction = b;
				invDirection = c;
			}
		}

		public const float defaultShakeRate = (float)Math.PI / 80f;

		public const float maximumShake = (float)Math.PI / 8f;

		public const float shakeDecayRate = (float)Math.PI / 300f;

		public const byte N = 1;

		public const byte E = 2;

		public const byte S = 4;

		public const byte W = 8;

		public const byte Cardinals = 15;

		public static readonly Vector2 N_Offset = new Vector2(0f, -1f);

		public static readonly Vector2 E_Offset = new Vector2(1f, 0f);

		public static readonly Vector2 S_Offset = new Vector2(0f, 1f);

		public static readonly Vector2 W_Offset = new Vector2(-1f, 0f);

		public const float paddyGrowBonus = 0.25f;

		public const int dry = 0;

		public const int watered = 1;

		public const int invisible = 2;

		public const int noFertilizer = 0;

		public const int fertilizerLowQuality = 368;

		public const int fertilizerHighQuality = 369;

		public const int waterRetentionSoil = 370;

		public const int waterRetentionSoilQuality = 371;

		public const int speedGro = 465;

		public const int superSpeedGro = 466;

		public const int hyperSpeedGro = 918;

		public const int fertilizerDeluxeQuality = 919;

		public const int waterRetentionSoilDeluxe = 920;

		public static Texture2D lightTexture;

		public static Texture2D darkTexture;

		public static Texture2D snowTexture;

		private readonly NetRef<Crop> netCrop = new NetRef<Crop>();

		public static Dictionary<byte, int> drawGuide;

		[XmlElement("state")]
		public readonly NetInt state = new NetInt();

		[XmlElement("fertilizer")]
		public readonly NetInt fertilizer = new NetInt();

		private bool shakeLeft;

		private float shakeRotation;

		private float maxShake;

		private float shakeRate;

		[XmlElement("c")]
		private readonly NetColor c = new NetColor(Color.White);

		private List<Action<GameLocation, Vector2>> queuedActions = new List<Action<GameLocation, Vector2>>();

		[XmlElement("isGreenhouseDirt")]
		public readonly NetBool isGreenhouseDirt = new NetBool(value: false);

		private byte neighborMask;

		private byte wateredNeighborMask;

		[XmlIgnore]
		public NetInt nearWaterForPaddy = new NetInt(-1);

		private Texture2D texture;

		private static readonly NeighborLoc[] _offsets = new NeighborLoc[4]
		{
			new NeighborLoc(N_Offset, 1, 4),
			new NeighborLoc(S_Offset, 4, 1),
			new NeighborLoc(E_Offset, 2, 8),
			new NeighborLoc(W_Offset, 8, 2)
		};

		private List<Neighbor> _neighbors = new List<Neighbor>();

		public Crop crop
		{
			get
			{
				return netCrop;
			}
			set
			{
				netCrop.Value = value;
			}
		}

		public HoeDirt()
			: base(needsTick: true)
		{
			base.NetFields.AddFields(netCrop, state, fertilizer, c, isGreenhouseDirt, nearWaterForPaddy);
			state.fieldChangeVisibleEvent += delegate
			{
				OnAdded(currentLocation, currentTileLocation);
			};
			netCrop.fieldChangeVisibleEvent += delegate
			{
				nearWaterForPaddy.Value = -1;
				updateNeighbors(currentLocation, currentTileLocation);
				if (netCrop.Value != null)
				{
					netCrop.Value.updateDrawMath(currentTileLocation);
				}
			};
			loadSprite();
			if (drawGuide == null)
			{
				populateDrawGuide();
			}
			initialize(Game1.currentLocation);
			nearWaterForPaddy.Interpolated(interpolate: false, wait: false);
			netCrop.Interpolated(interpolate: false, wait: false);
			netCrop.OnConflictResolve += delegate(Crop rejected, Crop accepted)
			{
				if (Game1.IsMasterGame && rejected != null && rejected.netSeedIndex.Value != -1)
				{
					queuedActions.Add(delegate(GameLocation gLocation, Vector2 tileLocation)
					{
						Vector2 vector = tileLocation * 64f;
						gLocation.debris.Add(new Debris(rejected.netSeedIndex, vector, vector));
					});
					base.NeedsUpdate = true;
				}
			};
		}

		public HoeDirt(int startingState, GameLocation location = null)
			: this()
		{
			state.Value = startingState;
			if (location != null)
			{
				initialize(location);
			}
		}

		public HoeDirt(int startingState, Crop crop)
			: this()
		{
			state.Value = startingState;
			this.crop = crop;
		}

		private void initialize(GameLocation location)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			if (location == null)
			{
				return;
			}
			if (location is MineShaft)
			{
				if ((location as MineShaft).GetAdditionalDifficulty() > 0)
				{
					if ((location as MineShaft).getMineArea() == 0 || (location as MineShaft).getMineArea() == 10)
					{
						c.Value = new Color(80, 100, 140) * 0.5f;
					}
				}
				else if ((location as MineShaft).getMineArea() == 80)
				{
					c.Value = Color.MediumPurple * 0.4f;
				}
			}
			else if (location.GetSeasonForLocation() == "fall" && location.IsOutdoors && !(location is Beach) && !(location is Desert))
			{
				c.Value = new Color(250, 210, 240);
			}
			else if (location is VolcanoDungeon)
			{
				c.Value = Color.MediumPurple * 0.7f;
			}
			isGreenhouseDirt.Value = location.IsGreenhouse;
		}

		public float getShakeRotation()
		{
			return shakeRotation;
		}

		public float getMaxShake()
		{
			return maxShake;
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64);
		}

		public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who, GameLocation location)
		{
			if (crop != null && (int)crop.currentPhase != 0 && speedOfCollision > 0 && maxShake == 0f && positionOfCollider.Intersects(getBoundingBox(tileLocation)) && Utility.isOnScreen(Utility.Vector2ToPoint(tileLocation), 64, location))
			{
				if (Game1.soundBank != null && (who == null || !(who is FarmAnimal)) && !Grass.grassSound.IsPlaying)
				{
					Grass.grassSound = Game1.soundBank.GetCue("grassyStep");
					Grass.grassSound.Play();
				}
				shake((float)Math.PI / 8f / (float)((5 + Game1.player.addedSpeed) / speedOfCollision) - ((speedOfCollision > 2) ? ((float)(int)crop.currentPhase * (float)Math.PI / 64f) : 0f), (float)Math.PI / 80f / (float)((5 + Game1.player.addedSpeed) / speedOfCollision), (float)positionOfCollider.Center.X > tileLocation.X * 64f + 32f);
			}
			if (crop != null && (int)crop.currentPhase != 0 && who is Farmer && (who as Farmer).running)
			{
				(who as Farmer).temporarySpeedBuff = -1f;
			}
		}

		public void shake(float shake, float rate, bool left)
		{
			if (crop != null)
			{
				maxShake = shake * (crop.raisedSeeds ? 0.6f : 1.5f);
				shakeRate = rate * 0.5f;
				shakeRotation = 0f;
				shakeLeft = left;
			}
			base.NeedsUpdate = true;
		}

		public bool needsWatering()
		{
			if (crop != null)
			{
				if (readyForHarvest())
				{
					return (int)crop.regrowAfterHarvest != -1;
				}
				return true;
			}
			return false;
		}

		public static void populateDrawGuide()
		{
			drawGuide = new Dictionary<byte, int>();
			drawGuide.Add(0, 0);
			drawGuide.Add(8, 15);
			drawGuide.Add(2, 13);
			drawGuide.Add(1, 12);
			drawGuide.Add(4, 4);
			drawGuide.Add(9, 11);
			drawGuide.Add(3, 9);
			drawGuide.Add(5, 8);
			drawGuide.Add(6, 1);
			drawGuide.Add(12, 3);
			drawGuide.Add(10, 14);
			drawGuide.Add(7, 5);
			drawGuide.Add(15, 6);
			drawGuide.Add(13, 7);
			drawGuide.Add(11, 10);
			drawGuide.Add(14, 2);
		}

		public override void loadSprite()
		{
			if (lightTexture == null)
			{
				try
				{
					lightTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirt");
				}
				catch (Exception)
				{
				}
			}
			if (darkTexture == null)
			{
				try
				{
					darkTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtDark");
				}
				catch (Exception)
				{
				}
			}
			if (snowTexture == null)
			{
				try
				{
					snowTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtSnow");
				}
				catch (Exception)
				{
				}
			}
			nearWaterForPaddy.Value = -1;
			if (crop != null)
			{
				crop.updateDrawMath(currentTileLocation);
			}
		}

		public override bool isPassable(Character c)
		{
			if (crop != null && (bool)crop.raisedSeeds)
			{
				return c is JunimoHarvester;
			}
			return true;
		}

		public bool readyForHarvest()
		{
			if (crop != null && (!crop.fullyGrown || (int)crop.dayOfCurrentPhase <= 0) && (int)crop.currentPhase >= crop.phaseDays.Count - 1 && !crop.dead)
			{
				if ((bool)crop.forageCrop)
				{
					return (int)crop.whichForageCrop != 2;
				}
				return true;
			}
			return false;
		}

		public override bool performUseAction(Vector2 tileLocation, GameLocation location)
		{
			if (crop != null)
			{
				bool harvestable = (int)crop.currentPhase >= crop.phaseDays.Count - 1 && (!crop.fullyGrown || (int)crop.dayOfCurrentPhase <= 0);
				if ((int)crop.harvestMethod == 0 && crop.harvest((int)tileLocation.X, (int)tileLocation.Y, this))
				{
					if (location != null && location is IslandLocation && Game1.random.NextDouble() < 0.05)
					{
						Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tileLocation.X * 64, (int)tileLocation.Y * 64, 5);
					}
					destroyCrop(tileLocation, showAnimation: false, location);
					return true;
				}
				if ((int)crop.harvestMethod == 1 && readyForHarvest())
				{
					if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon && (Game1.player.CurrentTool as MeleeWeapon).isScythe())
					{
						Game1.player.CanMove = false;
						Game1.player.UsingTool = true;
						Game1.player.canReleaseTool = true;
						Game1.player.Halt();
						try
						{
							Game1.player.CurrentTool.beginUsing(Game1.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, Game1.player);
						}
						catch (Exception)
						{
						}
						((MeleeWeapon)Game1.player.CurrentTool).setFarmerAnimating(Game1.player);
					}
					else
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13915"));
					}
				}
				return harvestable;
			}
			return false;
		}

		public bool plant(int index, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location)
		{
			if (isFertilizer)
			{
				if (crop != null && (int)crop.currentPhase != 0 && (index == 368 || index == 369))
				{
					return false;
				}
				if ((int)fertilizer != 0)
				{
					return false;
				}
				fertilizer.Value = index;
				applySpeedIncreases(who);
				location.playSound("dirtyHit");
				return true;
			}
			Crop c = new Crop(index, tileX, tileY);
			if (c.seasonsToGrowIn.Count == 0)
			{
				return false;
			}
			if (!who.currentLocation.isFarm && !who.currentLocation.IsGreenhouse && !who.currentLocation.CanPlantSeedsHere(index, tileX, tileY) && who.currentLocation.IsOutdoors)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13919"));
				return false;
			}
			if (!who.currentLocation.isOutdoors || who.currentLocation.IsGreenhouse || c.seasonsToGrowIn.Contains(location.GetSeasonForLocation()) || who.currentLocation.SeedsIgnoreSeasonsHere())
			{
				crop = c;
				if ((bool)c.raisedSeeds)
				{
					location.playSound("stoneStep");
				}
				location.playSound("dirtyHit");
				Game1.stats.SeedsSown++;
				applySpeedIncreases(who);
				nearWaterForPaddy.Value = -1;
				if (hasPaddyCrop() && paddyWaterCheck(location, new Vector2(tileX, tileY)))
				{
					state.Value = 1;
					updateNeighbors(location, new Vector2(tileX, tileY));
				}
				return true;
			}
			if (c.seasonsToGrowIn.Count > 0 && !c.seasonsToGrowIn.Contains(location.GetSeasonForLocation()))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924"));
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13925"));
			}
			return false;
		}

		protected void applySpeedIncreases(Farmer who)
		{
			if (crop == null)
			{
				return;
			}
			bool paddy_bonus = false;
			if (currentLocation != null && paddyWaterCheck(currentLocation, currentTileLocation))
			{
				paddy_bonus = true;
			}
			if (!(((int)fertilizer == 465 || (int)fertilizer == 466 || (int)fertilizer == 918 || who.professions.Contains(5)) | paddy_bonus))
			{
				return;
			}
			crop.ResetPhaseDays();
			int totalDaysOfCropGrowth = 0;
			for (int j = 0; j < crop.phaseDays.Count - 1; j++)
			{
				totalDaysOfCropGrowth += crop.phaseDays[j];
			}
			float speedIncrease = 0f;
			if ((int)fertilizer == 465)
			{
				speedIncrease += 0.1f;
			}
			else if ((int)fertilizer == 466)
			{
				speedIncrease += 0.25f;
			}
			else if ((int)fertilizer == 918)
			{
				speedIncrease += 0.33f;
			}
			if (paddy_bonus)
			{
				speedIncrease += 0.25f;
			}
			if (who.professions.Contains(5))
			{
				speedIncrease += 0.1f;
			}
			int daysToRemove = (int)Math.Ceiling((float)totalDaysOfCropGrowth * speedIncrease);
			int tries = 0;
			while (daysToRemove > 0 && tries < 3)
			{
				for (int i = 0; i < crop.phaseDays.Count; i++)
				{
					if ((i > 0 || crop.phaseDays[i] > 1) && crop.phaseDays[i] != 99999)
					{
						crop.phaseDays[i]--;
						daysToRemove--;
					}
					if (daysToRemove <= 0)
					{
						break;
					}
				}
				tries++;
			}
		}

		public void destroyCrop(Vector2 tileLocation, bool showAnimation, GameLocation location)
		{
			if (crop != null && showAnimation && location != null)
			{
				if ((int)crop.currentPhase < 1 && !crop.dead)
				{
					Game1.multiplayer.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite(12, tileLocation * 64f, Color.White));
					location.playSound("dirtyHit");
				}
				else
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(50, tileLocation * 64f, crop.dead ? new Color(207, 193, 43) : Color.ForestGreen));
				}
			}
			crop = null;
			nearWaterForPaddy.Value = -1;
			if (location != null)
			{
				updateNeighbors(location, tileLocation);
			}
		}

		public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
		{
			if (t != null)
			{
				if (t is Hoe)
				{
					if (crop != null && crop.hitWithHoe((int)tileLocation.X, (int)tileLocation.Y, location, this))
					{
						destroyCrop(tileLocation, showAnimation: true, location);
					}
				}
				else
				{
					if (t is Pickaxe && crop == null)
					{
						return true;
					}
					if (t is WateringCan)
					{
						state.Value = 1;
					}
					else if (t is MeleeWeapon && (t as MeleeWeapon).isScythe())
					{
						if (crop != null && (int)crop.harvestMethod == 1)
						{
							if ((int)crop.indexOfHarvest == 771 && (t as MeleeWeapon).hasEnchantmentOfType<HaymakerEnchantment>())
							{
								Game1.createItemDebris(new Object(771, 1), new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), -1);
								Game1.createItemDebris(new Object(771, 1), new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), -1);
							}
							if (crop.harvest((int)tileLocation.X, (int)tileLocation.Y, this))
							{
								destroyCrop(tileLocation, showAnimation: true, location);
							}
						}
						if (crop != null && (bool)crop.dead)
						{
							destroyCrop(tileLocation, showAnimation: true, location);
						}
					}
					else if (t.isHeavyHitter() && !(t is Hoe) && !(t is MeleeWeapon) && crop != null)
					{
						destroyCrop(tileLocation, showAnimation: true, location);
					}
				}
				shake((float)Math.PI / 32f, (float)Math.PI / 40f, tileLocation.X * 64f < Game1.player.Position.X);
			}
			else if (damage > 0 && crop != null)
			{
				if (damage == 50)
				{
					crop.Kill();
				}
				else
				{
					destroyCrop(tileLocation, showAnimation: true, location);
				}
			}
			return false;
		}

		public bool canPlantThisSeedHere(int objectIndex, int tileX, int tileY, bool isFertilizer = false)
		{
			if (isFertilizer)
			{
				if ((int)fertilizer == 0)
				{
					return true;
				}
			}
			else if (crop == null)
			{
				Crop c = new Crop(objectIndex, tileX, tileY);
				if (c.seasonsToGrowIn.Count == 0)
				{
					return false;
				}
				if (!Game1.currentLocation.IsOutdoors || Game1.currentLocation.IsGreenhouse || Game1.currentLocation.SeedsIgnoreSeasonsHere() || c.seasonsToGrowIn.Contains(Game1.currentLocation.GetSeasonForLocation()))
				{
					if ((bool)c.raisedSeeds && Utility.doesRectangleIntersectTile(Game1.player.GetBoundingBox(), tileX, tileY))
					{
						return false;
					}
					return true;
				}
				if (objectIndex == 309 || objectIndex == 310 || objectIndex == 311)
				{
					return true;
				}
				if (Game1.didPlayerJustClickAtAll() && !Game1.doesHUDMessageExist(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924")))
				{
					Game1.playSound("cancel");
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924"));
				}
			}
			return false;
		}

		public override void performPlayerEntryAction(Vector2 tileLocation)
		{
			base.performPlayerEntryAction(tileLocation);
			if (crop != null)
			{
				crop.updateDrawMath(tileLocation);
			}
		}

		public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
		{
			foreach (Action<GameLocation, Vector2> queuedAction in queuedActions)
			{
				queuedAction(location, tileLocation);
			}
			queuedActions.Clear();
			if (maxShake > 0f)
			{
				if (shakeLeft)
				{
					shakeRotation -= shakeRate;
					if (Math.Abs(shakeRotation) >= maxShake)
					{
						shakeLeft = false;
					}
				}
				else
				{
					shakeRotation += shakeRate;
					if (shakeRotation >= maxShake)
					{
						shakeLeft = true;
						shakeRotation -= shakeRate;
					}
				}
				maxShake = Math.Max(0f, maxShake - (float)Math.PI / 300f);
			}
			else
			{
				shakeRotation /= 2f;
				if (shakeRotation <= 0.01f)
				{
					base.NeedsUpdate = false;
					shakeRotation = 0f;
				}
			}
			if ((int)state == 2)
			{
				return crop == null;
			}
			return false;
		}

		public bool hasPaddyCrop()
		{
			if (crop != null && crop.isPaddyCrop())
			{
				return true;
			}
			return false;
		}

		public bool paddyWaterCheck(GameLocation location, Vector2 tile_location)
		{
			if (nearWaterForPaddy.Value >= 0)
			{
				return nearWaterForPaddy.Value == 1;
			}
			if (!hasPaddyCrop())
			{
				nearWaterForPaddy.Value = 0;
				return false;
			}
			if (location.getObjectAtTile((int)tile_location.X, (int)tile_location.Y) is IndoorPot)
			{
				nearWaterForPaddy.Value = 0;
				return false;
			}
			int range = 3;
			for (int x_offset = -range; x_offset <= range; x_offset++)
			{
				for (int y_offset = -range; y_offset <= range; y_offset++)
				{
					if (location.isWaterTile((int)(tile_location.X + (float)x_offset), (int)(tile_location.Y + (float)y_offset)))
					{
						nearWaterForPaddy.Value = 1;
						return true;
					}
				}
			}
			nearWaterForPaddy.Value = 0;
			return false;
		}

		public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
		{
			if (crop != null)
			{
				crop.newDay(state, fertilizer, (int)tileLocation.X, (int)tileLocation.Y, environment);
				if ((bool)environment.isOutdoors && Game1.GetSeasonForLocation(environment).Equals("winter") && crop != null && !crop.isWildSeedCrop() && (int)crop.indexOfHarvest != 771 && !environment.IsGreenhouse && !environment.SeedsIgnoreSeasonsHere())
				{
					destroyCrop(tileLocation, showAnimation: false, environment);
				}
			}
			if ((!hasPaddyCrop() || !paddyWaterCheck(environment, tileLocation)) && ((int)fertilizer != 370 || !(Game1.random.NextDouble() < 0.33)) && ((int)fertilizer != 371 || !(Game1.random.NextDouble() < 0.66)) && (int)fertilizer != 920)
			{
				state.Value = 0;
			}
			if (environment.IsGreenhouse)
			{
				isGreenhouseDirt.Value = true;
				c.Value = Color.White;
			}
		}

		public override bool seasonUpdate(bool onLoad)
		{
			if (!onLoad && !isGreenhouseDirt.Value && !(currentLocation is IslandLocation) && (crop == null || (bool)crop.dead || !crop.seasonsToGrowIn.Contains(Game1.currentLocation.GetSeasonForLocation())))
			{
				fertilizer.Value = 0;
			}
			if (Game1.currentLocation.GetSeasonForLocation() == "fall" && !isGreenhouseDirt.Value)
			{
				c.Value = new Color(250, 210, 240);
			}
			else
			{
				c.Value = Color.White;
			}
			texture = null;
			return false;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			int sourceRectPosition2 = 1;
			byte drawSum = 0;
			Vector2 surroundingLocations = tileLocation;
			surroundingLocations.X += 1f;
			GameLocation farm = Game1.getLocationFromName("Farm");
			if (farm.terrainFeatures.ContainsKey(surroundingLocations) && farm.terrainFeatures[surroundingLocations] is HoeDirt)
			{
				drawSum = (byte)(drawSum + 2);
			}
			surroundingLocations.X -= 2f;
			if (farm.terrainFeatures.ContainsKey(surroundingLocations) && farm.terrainFeatures[surroundingLocations] is HoeDirt)
			{
				drawSum = (byte)(drawSum + 8);
			}
			surroundingLocations.X += 1f;
			surroundingLocations.Y += 1f;
			if (Game1.currentLocation.terrainFeatures.ContainsKey(surroundingLocations) && farm.terrainFeatures[surroundingLocations] is HoeDirt)
			{
				drawSum = (byte)(drawSum + 4);
			}
			surroundingLocations.Y -= 2f;
			if (farm.terrainFeatures.ContainsKey(surroundingLocations) && farm.terrainFeatures[surroundingLocations] is HoeDirt)
			{
				drawSum = (byte)(drawSum + 1);
			}
			sourceRectPosition2 = drawGuide[drawSum];
			spriteBatch.Draw(lightTexture, positionOnScreen, new Rectangle(sourceRectPosition2 % 4 * 64, sourceRectPosition2 / 4 * 64, 64, 64), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth + positionOnScreen.Y / 20000f);
			if (crop != null)
			{
				crop.drawInMenu(spriteBatch, positionOnScreen + new Vector2(64f * scale, 64f * scale), Color.White, 0f, scale, layerDepth + (positionOnScreen.Y + 64f * scale) / 20000f);
			}
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			int state = this.state.Value;
			if (state != 2)
			{
				if (texture == null)
				{
					texture = ((Game1.currentLocation.Name.Equals("Mountain") || Game1.currentLocation.Name.Equals("Mine") || (Game1.currentLocation is MineShaft && (Game1.currentLocation as MineShaft).shouldShowDarkHoeDirt()) || Game1.currentLocation is VolcanoDungeon) ? darkTexture : lightTexture);
					if ((Game1.GetSeasonForLocation(Game1.currentLocation).Equals("winter") && !(Game1.currentLocation is Desert) && !Game1.currentLocation.IsGreenhouse && !Game1.currentLocation.SeedsIgnoreSeasonsHere() && !(Game1.currentLocation is MineShaft)) || (Game1.currentLocation is MineShaft && (Game1.currentLocation as MineShaft).shouldUseSnowTextureHoeDirt()))
					{
						texture = snowTexture;
					}
				}
				byte drawSum = (byte)(neighborMask & 0xF);
				int sourceRectPosition = drawGuide[drawSum];
				int wateredRectPosition = drawGuide[wateredNeighborMask];
				Vector2 drawPos = Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f));
				spriteBatch.Draw(texture, drawPos, new Rectangle(sourceRectPosition % 4 * 16, sourceRectPosition / 4 * 16, 16, 16), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
				if (state == 1)
				{
					spriteBatch.Draw(texture, drawPos, new Rectangle(wateredRectPosition % 4 * 16 + (paddyWaterCheck(Game1.currentLocation, tileLocation) ? 128 : 64), wateredRectPosition / 4 * 16, 16, 16), c, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.2E-08f);
				}
				int fertilizer = this.fertilizer.Value;
				if (fertilizer != 0)
				{
					Rectangle fertilizer_rect = GetFertilizerSourceRect(fertilizer);
					spriteBatch.Draw(Game1.mouseCursors, drawPos, fertilizer_rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.9E-08f);
				}
			}
			if (crop != null)
			{
				crop.draw(spriteBatch, tileLocation, (state == 1 && (int)crop.currentPhase == 0 && crop.shouldDrawDarkWhenWatered()) ? (new Color(180, 100, 200) * 1f) : Color.White, shakeRotation);
			}
		}

		public Rectangle GetFertilizerSourceRect(int fertilizer)
		{
			int fertilizerIndex = 0;
			switch (fertilizer)
			{
			case 369:
				fertilizerIndex = 1;
				break;
			case 370:
				fertilizerIndex = 3;
				break;
			case 371:
				fertilizerIndex = 4;
				break;
			case 920:
				fertilizerIndex = 5;
				break;
			case 465:
				fertilizerIndex = 6;
				break;
			case 466:
				fertilizerIndex = 7;
				break;
			case 918:
				fertilizerIndex = 8;
				break;
			case 919:
				fertilizerIndex = 2;
				break;
			}
			return new Rectangle(173 + fertilizerIndex / 3 * 16, 462 + fertilizerIndex % 3 * 16, 16, 16);
		}

		private List<Neighbor> gatherNeighbors(GameLocation loc, Vector2 tilePos)
		{
			List<Neighbor> results = _neighbors;
			results.Clear();
			TerrainFeature feature = null;
			HoeDirt dirt2 = null;
			NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = loc.terrainFeatures;
			NeighborLoc[] offsets = _offsets;
			for (int j = 0; j < offsets.Length; j++)
			{
				NeighborLoc item = offsets[j];
				Vector2 tile = tilePos + item.Offset;
				if (terrainFeatures.TryGetValue(tile, out feature) && feature != null)
				{
					dirt2 = (feature as HoeDirt);
					if (dirt2 != null && dirt2.state.Value != 2)
					{
						Neighbor i = new Neighbor(dirt2, item.Direction, item.InvDirection);
						results.Add(i);
					}
				}
			}
			return results;
		}

		public void updateNeighbors(GameLocation loc, Vector2 tilePos)
		{
			if (loc != null)
			{
				List<Neighbor> list = gatherNeighbors(loc, tilePos);
				neighborMask = 0;
				wateredNeighborMask = 0;
				foreach (Neighbor i in list)
				{
					neighborMask |= i.direction;
					if ((int)state != 2)
					{
						i.feature.OnNeighborAdded(i.invDirection);
					}
					if ((int)state == 1 && (int)i.feature.state == 1)
					{
						if (i.feature.paddyWaterCheck(i.feature.currentLocation, i.feature.currentTileLocation) == paddyWaterCheck(loc, tilePos))
						{
							wateredNeighborMask |= i.direction;
							i.feature.wateredNeighborMask |= i.invDirection;
						}
						else
						{
							i.feature.wateredNeighborMask = (byte)(i.feature.wateredNeighborMask & ~i.invDirection);
						}
					}
				}
			}
		}

		public void OnAdded(GameLocation loc, Vector2 tilePos)
		{
			updateNeighbors(loc, tilePos);
		}

		public void OnRemoved(GameLocation loc, Vector2 tilePos)
		{
			if (loc != null)
			{
				List<Neighbor> list = gatherNeighbors(loc, tilePos);
				neighborMask = 0;
				wateredNeighborMask = 0;
				foreach (Neighbor i in list)
				{
					i.feature.OnNeighborRemoved(i.invDirection);
					if ((int)state == 1)
					{
						i.feature.wateredNeighborMask = (byte)(i.feature.wateredNeighborMask & ~i.invDirection);
					}
				}
			}
		}

		public void OnNeighborAdded(byte direction)
		{
			neighborMask |= direction;
		}

		public void OnNeighborRemoved(byte direction)
		{
			neighborMask = (byte)(neighborMask & ~direction);
		}
	}
}
