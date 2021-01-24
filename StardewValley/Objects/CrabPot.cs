using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class CrabPot : Object
	{
		public const int lidFlapTimerInterval = 60;

		private float yBob;

		[XmlElement("directionOffset")]
		public readonly NetVector2 directionOffset = new NetVector2();

		[XmlElement("bait")]
		public readonly NetRef<Object> bait = new NetRef<Object>();

		public int tileIndexToShow;

		private bool lidFlapping;

		private bool lidClosing;

		private float lidFlapTimer;

		private new float shakeTimer;

		private Vector2 shake;

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(directionOffset, bait);
		}

		public CrabPot()
		{
		}

		public List<Vector2> getOverlayTiles(GameLocation location)
		{
			List<Vector2> tiles = new List<Vector2>();
			if (directionOffset.Y < 0f)
			{
				addOverlayTilesIfNecessary(location, (int)base.TileLocation.X, (int)tileLocation.Y, tiles);
			}
			addOverlayTilesIfNecessary(location, (int)base.TileLocation.X, (int)tileLocation.Y + 1, tiles);
			if (directionOffset.X < 0f)
			{
				addOverlayTilesIfNecessary(location, (int)base.TileLocation.X - 1, (int)tileLocation.Y + 1, tiles);
			}
			if (directionOffset.X > 0f)
			{
				addOverlayTilesIfNecessary(location, (int)base.TileLocation.X + 1, (int)tileLocation.Y + 1, tiles);
			}
			return tiles;
		}

		protected void addOverlayTilesIfNecessary(GameLocation location, int tile_x, int tile_y, List<Vector2> tiles)
		{
			if (location == Game1.currentLocation && location.getTileIndexAt(tile_x, tile_y, "Buildings") >= 0 && location.doesTileHaveProperty(tile_x, tile_y + 1, "Back", "Water") == null)
			{
				tiles.Add(new Vector2(tile_x, tile_y));
			}
		}

		public void addOverlayTiles(GameLocation location)
		{
			if (location == Game1.currentLocation)
			{
				foreach (Vector2 tile in getOverlayTiles(location))
				{
					if (!Game1.crabPotOverlayTiles.ContainsKey(tile))
					{
						Game1.crabPotOverlayTiles[tile] = 0;
					}
					Game1.crabPotOverlayTiles[tile]++;
				}
			}
		}

		public void removeOverlayTiles(GameLocation location)
		{
			if (location == Game1.currentLocation)
			{
				foreach (Vector2 tile in getOverlayTiles(location))
				{
					if (Game1.crabPotOverlayTiles.ContainsKey(tile))
					{
						Game1.crabPotOverlayTiles[tile]--;
						if (Game1.crabPotOverlayTiles[tile] <= 0)
						{
							Game1.crabPotOverlayTiles.Remove(tile);
						}
					}
				}
			}
		}

		public CrabPot(Vector2 tileLocation, int stack = 1)
			: base(tileLocation, 710, "Crab Pot", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
		{
			type.Value = "interactive";
			tileIndexToShow = parentSheetIndex;
		}

		public static bool IsValidCrabPotLocationTile(GameLocation location, int x, int y)
		{
			if (location is Caldera)
			{
				return false;
			}
			Vector2 placement_tile = new Vector2(x, y);
			bool neighbor_check = (location.doesTileHaveProperty(x + 1, y, "Water", "Back") != null && location.doesTileHaveProperty(x - 1, y, "Water", "Back") != null) || (location.doesTileHaveProperty(x, y + 1, "Water", "Back") != null && location.doesTileHaveProperty(x, y - 1, "Water", "Back") != null);
			if (location.objects.ContainsKey(placement_tile) || !neighbor_check || location.doesTileHaveProperty((int)placement_tile.X, (int)placement_tile.Y, "Water", "Back") == null || location.doesTileHaveProperty((int)placement_tile.X, (int)placement_tile.Y, "Passable", "Buildings") != null)
			{
				return false;
			}
			return true;
		}

		public override void actionOnPlayerEntry()
		{
			updateOffset(Game1.currentLocation);
			addOverlayTiles(Game1.currentLocation);
			base.actionOnPlayerEntry();
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			Vector2 placementTile = new Vector2(x / 64, y / 64);
			if (who != null)
			{
				owner.Value = who.UniqueMultiplayerID;
			}
			if (!IsValidCrabPotLocationTile(location, (int)placementTile.X, (int)placementTile.Y))
			{
				return false;
			}
			tileLocation.Value = new Vector2(x / 64, y / 64);
			location.objects.Add(tileLocation, this);
			location.playSound("waterSlosh");
			DelayedAction.playSoundAfterDelay("slosh", 150);
			updateOffset(location);
			addOverlayTiles(location);
			return true;
		}

		public void updateOffset(GameLocation location)
		{
			Vector2 offset = Vector2.Zero;
			if (checkLocation(location, tileLocation.X - 1f, tileLocation.Y))
			{
				offset += new Vector2(32f, 0f);
			}
			if (checkLocation(location, tileLocation.X + 1f, tileLocation.Y))
			{
				offset += new Vector2(-32f, 0f);
			}
			if (offset.X != 0f && checkLocation(location, tileLocation.X + (float)Math.Sign(offset.X), tileLocation.Y + 1f))
			{
				offset += new Vector2(0f, -42f);
			}
			if (checkLocation(location, tileLocation.X, tileLocation.Y - 1f))
			{
				offset += new Vector2(0f, 32f);
			}
			if (checkLocation(location, tileLocation.X, tileLocation.Y + 1f))
			{
				offset += new Vector2(0f, -42f);
			}
			directionOffset.Value = offset;
		}

		protected bool checkLocation(GameLocation location, float tile_x, float tile_y)
		{
			if (location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Water", "Back") == null || location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Passable", "Buildings") != null)
			{
				return true;
			}
			return false;
		}

		public override bool canBePlacedInWater()
		{
			return true;
		}

		public override Item getOne()
		{
			Object @object = new Object(parentSheetIndex, 1);
			@object._GetOneFrom(this);
			return @object;
		}

		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
		{
			Object dropIn = dropInItem as Object;
			if (dropIn == null)
			{
				return false;
			}
			Farmer owner_farmer = Game1.getFarmer(owner);
			if (dropIn.Category == -21 && bait.Value == null && (owner_farmer == null || !owner_farmer.professions.Contains(11)))
			{
				if (!probe)
				{
					if (who != null)
					{
						owner.Value = who.UniqueMultiplayerID;
					}
					bait.Value = (dropIn.getOne() as Object);
					who.currentLocation.playSound("Ship");
					lidFlapping = true;
					lidFlapTimer = 60f;
				}
				return true;
			}
			return false;
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (tileIndexToShow == 714)
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				Object item = heldObject.Value;
				heldObject.Value = null;
				if (who.IsLocalPlayer && !who.addItemToInventoryBool(item))
				{
					heldObject.Value = item;
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					return false;
				}
				Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
				if (data.ContainsKey(item.parentSheetIndex))
				{
					string[] rawData = data[item.parentSheetIndex].Split('/');
					int minFishSize = (rawData.Length <= 5) ? 1 : Convert.ToInt32(rawData[5]);
					int maxFishSize = (rawData.Length > 5) ? Convert.ToInt32(rawData[6]) : 10;
					who.caughtFish(item.parentSheetIndex, Game1.random.Next(minFishSize, maxFishSize + 1));
				}
				readyForHarvest.Value = false;
				tileIndexToShow = 710;
				lidFlapping = true;
				lidFlapTimer = 60f;
				bait.Value = null;
				who.animateOnce(279 + who.FacingDirection);
				who.currentLocation.playSound("fishingRodBend");
				DelayedAction.playSoundAfterDelay("coin", 500);
				who.gainExperience(1, 5);
				shake = Vector2.Zero;
				shakeTimer = 0f;
				return true;
			}
			if (bait.Value == null)
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
				{
					if (Game1.player.addItemToInventoryBool(getOne()))
					{
						if (who.isMoving())
						{
							Game1.haltAfterCheck = false;
						}
						Game1.playSound("coin");
						Game1.currentLocation.objects.Remove(tileLocation);
						return true;
					}
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
				}
			}
			return false;
		}

		public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			removeOverlayTiles(environment);
			base.performRemoveAction(tileLocation, environment);
		}

		public override void DayUpdate(GameLocation location)
		{
			bool isLuremaster = Game1.getFarmer(owner) != null && Game1.getFarmer(owner).professions.Contains(11);
			bool isMariner = Game1.getFarmer(owner) != null && Game1.getFarmer(owner).professions.Contains(10);
			if ((long)owner == 0L && Game1.player.professions.Contains(11))
			{
				isMariner = true;
			}
			if (!((bait.Value != null) | isLuremaster) || heldObject.Value != null)
			{
				return;
			}
			tileIndexToShow = 714;
			readyForHarvest.Value = true;
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)tileLocation.X * 1000 + (int)tileLocation.Y);
			Dictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			List<int> marinerList = new List<int>();
			double chanceForJunk = isMariner ? 0.0 : 0.2;
			if (!isMariner)
			{
				chanceForJunk += (double)location.getExtraTrashChanceForCrabPot((int)tileLocation.X, (int)tileLocation.Y);
			}
			if (r.NextDouble() > chanceForJunk)
			{
				foreach (KeyValuePair<int, string> v in fishData)
				{
					if (v.Value.Contains("trap"))
					{
						bool should_catch_ocean_fish = location is Beach || location.catchOceanCrabPotFishFromThisSpot((int)tileLocation.X, (int)tileLocation.Y);
						string[] rawSplit = v.Value.Split('/');
						if ((!rawSplit[4].Equals("ocean") || should_catch_ocean_fish) && (!rawSplit[4].Equals("freshwater") || !should_catch_ocean_fish))
						{
							if (isMariner)
							{
								marinerList.Add(v.Key);
							}
							else
							{
								double chanceForCatch = Convert.ToDouble(rawSplit[2]);
								if (r.NextDouble() < chanceForCatch)
								{
									heldObject.Value = new Object(v.Key, 1);
									break;
								}
							}
						}
					}
				}
			}
			if (heldObject.Value == null)
			{
				if (isMariner && marinerList.Count > 0)
				{
					heldObject.Value = new Object(marinerList[r.Next(marinerList.Count)], 1);
				}
				else
				{
					heldObject.Value = new Object(r.Next(168, 173), 1);
				}
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (lidFlapping)
			{
				lidFlapTimer -= time.ElapsedGameTime.Milliseconds;
				if (lidFlapTimer <= 0f)
				{
					tileIndexToShow += ((!lidClosing) ? 1 : (-1));
					if (tileIndexToShow >= 713 && !lidClosing)
					{
						lidClosing = true;
						tileIndexToShow--;
					}
					else if (tileIndexToShow <= 709 && lidClosing)
					{
						lidClosing = false;
						tileIndexToShow++;
						lidFlapping = false;
						if (bait.Value != null)
						{
							tileIndexToShow = 713;
						}
					}
					lidFlapTimer = 60f;
				}
			}
			if ((bool)readyForHarvest && heldObject.Value != null)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (shakeTimer < 0f)
				{
					shakeTimer = Game1.random.Next(2800, 3200);
				}
			}
			if (shakeTimer > 2000f)
			{
				shake.X = Game1.random.Next(-1, 2);
			}
			else
			{
				shake.X = 0f;
			}
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (heldObject.Value != null)
			{
				tileIndexToShow = 714;
			}
			else if (tileIndexToShow == 0)
			{
				tileIndexToShow = parentSheetIndex;
			}
			yBob = (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 500.0 + (double)(x * 64)) * 8.0 + 8.0);
			if (yBob <= 0.001f)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, directionOffset + new Vector2(x * 64 + 4, y * 64 + 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f));
			}
			_ = Game1.currentLocation.Map.GetLayer("Buildings").Tiles[x, y];
			spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, directionOffset + new Vector2(x * 64, y * 64 + (int)yBob)) + shake, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, tileIndexToShow, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, ((float)(y * 64) + directionOffset.Y + (float)(x % 4)) / 10000f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, directionOffset + new Vector2(x * 64 + 4, y * 64 + 48)) + shake, new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2112 + (((x + y) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)), 56, 16 + (int)yBob), Game1.currentLocation.waterColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, ((float)(y * 64) + directionOffset.Y + (float)(x % 4)) / 9999f);
			if ((bool)readyForHarvest && heldObject.Value != null)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, directionOffset + new Vector2(x * 64 - 8, (float)(y * 64 - 96 - 16) + yOffset)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-06f + tileLocation.X / 10000f);
				spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, directionOffset + new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, heldObject.Value.parentSheetIndex, 16, 16), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + tileLocation.X / 10000f);
			}
		}
	}
}
