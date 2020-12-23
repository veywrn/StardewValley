using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class Forest : GameLocation
	{
		[XmlIgnore]
		public readonly NetObjectList<FarmAnimal> marniesLivestock = new NetObjectList<FarmAnimal>();

		[XmlIgnore]
		public readonly NetList<Microsoft.Xna.Framework.Rectangle, NetRectangle> travelingMerchantBounds = new NetList<Microsoft.Xna.Framework.Rectangle, NetRectangle>();

		[XmlIgnore]
		public readonly NetBool netTravelingMerchantDay = new NetBool(value: false);

		[XmlElement("log")]
		public readonly NetRef<ResourceClump> netLog = new NetRef<ResourceClump>();

		private int chimneyTimer = 500;

		private bool hasShownCCUpgrade;

		private Microsoft.Xna.Framework.Rectangle hatterSource = new Microsoft.Xna.Framework.Rectangle(600, 1957, 64, 32);

		private Vector2 hatterPos = new Vector2(2056f, 6016f);

		[XmlIgnore]
		public bool travelingMerchantDay
		{
			get
			{
				return netTravelingMerchantDay.Value;
			}
			set
			{
				netTravelingMerchantDay.Value = value;
			}
		}

		[XmlIgnore]
		public ResourceClump log
		{
			get
			{
				return netLog.Value;
			}
			set
			{
				netLog.Value = value;
			}
		}

		public Forest()
		{
		}

		public Forest(string map, string name)
			: base(map, name)
		{
			marniesLivestock.Add(new FarmAnimal("Dairy Cow", Game1.multiplayer.getNewID(), -1L));
			marniesLivestock.Add(new FarmAnimal("Dairy Cow", Game1.multiplayer.getNewID(), -1L));
			marniesLivestock[0].Position = new Vector2(6272f, 1280f);
			marniesLivestock[1].Position = new Vector2(6464f, 1280f);
			log = new ResourceClump(602, 2, 2, new Vector2(1f, 6f));
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(marniesLivestock, travelingMerchantBounds, netTravelingMerchantDay, netLog);
		}

		public void removeSewerTrash()
		{
			ApplyMapOverride("Forest-SewerClean", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(83, 97, 24, 12));
			setMapTileIndex(43, 106, -1, "Buildings");
			setMapTileIndex(17, 106, -1, "Buildings");
			setMapTileIndex(13, 105, -1, "Buildings");
			setMapTileIndex(4, 85, -1, "Buildings");
			setMapTileIndex(2, 85, -1, "Buildings");
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			addFrog();
			if (Game1.year > 2 && getCharacterFromName("TrashBear") != null && NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
			{
				characters.Remove(getCharacterFromName("TrashBear"));
			}
			if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
			{
				removeSewerTrash();
			}
			if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				showCommunityUpgradeShortcuts();
			}
		}

		private void showCommunityUpgradeShortcuts()
		{
			if (!hasShownCCUpgrade)
			{
				removeTile(119, 36, "Buildings");
				LargeTerrainFeature blockingBush = null;
				foreach (LargeTerrainFeature t in largeTerrainFeatures)
				{
					if (t.tilePosition.Equals(new Vector2(119f, 35f)))
					{
						blockingBush = t;
						break;
					}
				}
				if (blockingBush != null)
				{
					largeTerrainFeatures.Remove(blockingBush);
				}
				hasShownCCUpgrade = true;
				warps.Add(new Warp(120, 35, "Beach", 0, 6, flipFarmer: false));
				warps.Add(new Warp(120, 36, "Beach", 0, 6, flipFarmer: false));
			}
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (Game1.year > 2 && !Game1.isRaining && !Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) && getCharacterFromName("TrashBear") == null && !NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
			{
				characters.Add(new TrashBear());
			}
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (log != null && log.getBoundingBox(log.tile).Contains(tileX * 64, tileY * 64))
			{
				if (log.performToolAction(t, 1, log.tile, this))
				{
					log = null;
				}
				return true;
			}
			return base.performToolAction(t, tileX, tileY);
		}

		protected virtual bool isWizardHouseUnlocked()
		{
			if (Game1.player.mailReceived.Contains("wizardJunimoNote"))
			{
				return true;
			}
			if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
			{
				return true;
			}
			bool num = Game1.MasterPlayer.mailReceived.Contains("ccFishTank");
			bool ccBulletin = Game1.MasterPlayer.mailReceived.Contains("ccBulletin");
			bool ccPantry = Game1.MasterPlayer.mailReceived.Contains("ccPantry");
			bool ccVault = Game1.MasterPlayer.mailReceived.Contains("ccVault");
			bool ccBoilerRoom = Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom");
			bool ccCraftsRoom = Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom");
			return num & ccBulletin & ccPantry & ccVault & ccBoilerRoom & ccCraftsRoom;
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexOfCheckLocation = (map.GetLayer("Buildings").Tiles[tileLocation] != null) ? map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : (-1);
			if (tileIndexOfCheckLocation == 901 && !isWizardHouseUnlocked())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Forest_WizardTower_Locked"));
				return false;
			}
			if (base.checkAction(tileLocation, viewport, who))
			{
				return true;
			}
			switch (tileIndexOfCheckLocation)
			{
			case 1394:
				if (who.hasRustyKey && !who.mailReceived.Contains("OpenedSewer"))
				{
					playSound("openBox");
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Forest_OpenedSewer")));
					who.mailReceived.Add("OpenedSewer");
				}
				else if (who.mailReceived.Contains("OpenedSewer"))
				{
					Game1.warpFarmer("Sewer", 3, 48, 0);
					playSound("openChest");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
				}
				break;
			case 1972:
				if (who.achievements.Count > 0)
				{
					Game1.activeClickableMenu = new ShopMenu(Utility.getHatStock(), 0, "HatMouse");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Forest_HatMouseStore_Abandoned"));
				}
				break;
			}
			if (travelingMerchantDay && Game1.timeOfDay < 2000)
			{
				if (tileLocation.X == 27 && tileLocation.Y == 11)
				{
					Game1.activeClickableMenu = new ShopMenu(Utility.getTravelingMerchantStock((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed)), 0, "Traveler", Utility.onTravelingMerchantShopPurchase);
					return true;
				}
				if (tileLocation.X == 23 && tileLocation.Y == 11)
				{
					playSound("pig");
					return true;
				}
			}
			Microsoft.Xna.Framework.Rectangle boundingBox = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			if (log != null && log.getBoundingBox(log.tile).Intersects(boundingBox))
			{
				log.performUseAction(new Vector2(tileLocation.X, tileLocation.Y), this);
				return true;
			}
			return false;
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			if (log != null && log.getBoundingBox(log.tile).Intersects(position))
			{
				return true;
			}
			if (travelingMerchantBounds != null)
			{
				foreach (Microsoft.Xna.Framework.Rectangle r in travelingMerchantBounds)
				{
					if (position.Intersects(r))
					{
						return true;
					}
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			if (dayOfMonth % 7 % 5 == 0)
			{
				travelingMerchantDay = true;
				travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(1472, 640, 492, 116));
				travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(1652, 744, 76, 48));
				travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(1812, 744, 104, 48));
				foreach (Microsoft.Xna.Framework.Rectangle travelingMerchantBound in travelingMerchantBounds)
				{
					Utility.clearObjectsInArea(travelingMerchantBound, this);
				}
				if (Game1.IsMasterGame && Game1.netWorldState.Value.VisitsUntilY1Guarantee >= 0)
				{
					Game1.netWorldState.Value.VisitsUntilY1Guarantee--;
				}
			}
			else
			{
				travelingMerchantBounds.Clear();
				travelingMerchantDay = false;
			}
			if (Game1.currentSeason.Equals("spring"))
			{
				for (int i = 0; i < 7; i++)
				{
					Vector2 origin = new Vector2(Game1.random.Next(70, map.Layers[0].LayerWidth - 10), Game1.random.Next(68, map.Layers[0].LayerHeight - 15));
					if (origin.Y > 30f)
					{
						foreach (Vector2 v in Utility.recursiveFindOpenTiles(this, origin, 16))
						{
							string s = doesTileHaveProperty((int)v.X, (int)v.Y, "Diggable", "Back");
							if (!terrainFeatures.ContainsKey(v) && s != null && Game1.random.NextDouble() < (double)(1f - Vector2.Distance(origin, v) * 0.15f))
							{
								terrainFeatures.Add(v, new HoeDirt(0, new Crop(forageCrop: true, 1, (int)v.X, (int)v.Y)));
							}
						}
					}
				}
			}
			if (Game1.year > 2 && getCharacterFromName("TrashBear") != null)
			{
				characters.Remove(getCharacterFromName("TrashBear"));
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			foreach (FarmAnimal item in marniesLivestock)
			{
				item.updateWhenCurrentLocation(time, this);
			}
			if (log != null)
			{
				log.tickUpdate(time, log.tile, this);
			}
			if (Game1.timeOfDay >= 2000)
			{
				return;
			}
			if (travelingMerchantDay)
			{
				if (Game1.random.NextDouble() < 0.001)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(99, 1423, 13, 19), new Vector2(1472f, 668f), flipped: false, 0f, Color.White)
					{
						interval = Game1.random.Next(500, 1500),
						layerDepth = 0.07682f,
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(51, 1444, 5, 5), new Vector2(1500f, 744f), flipped: false, 0f, Color.White)
					{
						interval = 500f,
						animationLength = 1,
						layerDepth = 0.07682f,
						scale = 4f
					});
				}
				if (Game1.random.NextDouble() < 0.003)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(89, 1445, 6, 3), new Vector2(1764f, 664f), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						animationLength = 3,
						pingPong = true,
						totalNumberOfLoops = 1,
						layerDepth = 0.07682f,
						scale = 4f
					});
				}
			}
			chimneyTimer -= time.ElapsedGameTime.Milliseconds;
			if (chimneyTimer <= 0)
			{
				chimneyTimer = (travelingMerchantDay ? 500 : Game1.random.Next(200, 2000));
				Vector2 smokeSpot = travelingMerchantDay ? new Vector2(1868f, 524f) : new Vector2(5592f, 608f);
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), smokeSpot, flipped: false, 0.002f, Color.Gray)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -0.5f),
					acceleration = new Vector2(0.002f, 0f),
					interval = 99999f,
					layerDepth = 1f,
					scale = 3f,
					scaleChange = 0.01f,
					rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
				});
				if (travelingMerchantDay)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(225, 1388, 7, 5), new Vector2(1868f, 536f), flipped: false, 0f, Color.White)
					{
						interval = chimneyTimer - chimneyTimer / 5,
						animationLength = 1,
						layerDepth = 0.99f,
						scale = 4.3f,
						scaleChange = -0.015f
					});
				}
			}
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (travelingMerchantDay && Game1.random.NextDouble() < 0.4)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(57, 1430, 4, 12), new Vector2(1792f, 656f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					animationLength = 10,
					pingPong = true,
					totalNumberOfLoops = 1,
					layerDepth = 0.07682f,
					scale = 4f
				});
				if (Game1.random.NextDouble() < 0.66)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(89, 1445, 6, 3), new Vector2(1764f, 664f), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						animationLength = 3,
						pingPong = true,
						totalNumberOfLoops = 1,
						layerDepth = 0.07683001f,
						scale = 4f
					});
				}
			}
		}

		public override int getFishingLocation(Vector2 tile)
		{
			if (tile.X < 53f && tile.Y < 43f)
			{
				return 1;
			}
			return 0;
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			bool using_magic_bait = IsUsingMagicBait(who);
			if (who.getTileX() == 58 && who.getTileY() == 87 && who.FishingLevel >= 6 && waterDepth >= 3 && Game1.random.NextDouble() < 0.5)
			{
				if (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY"))
				{
					return new Object(902, 1);
				}
				if (!who.fishCaught.ContainsKey(775) && (Game1.currentSeason.Equals("winter") | using_magic_bait))
				{
					return new Object(775, 1);
				}
			}
			if (bobberTile.Y > 108f && !Game1.player.mailReceived.Contains("caughtIridiumKrobus"))
			{
				Game1.player.mailReceived.Add("caughtIridiumKrobus");
				return new Furniture(2428, Vector2.Zero);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override void draw(SpriteBatch spriteBatch)
		{
			base.draw(spriteBatch);
			foreach (FarmAnimal item in marniesLivestock)
			{
				item.draw(spriteBatch);
			}
			if (log != null)
			{
				log.draw(spriteBatch, log.tile);
			}
			if (travelingMerchantDay)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(1536f, 512f)), new Microsoft.Xna.Framework.Rectangle(142, 1382, 109, 70), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0768f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(1472f, 672f)), new Microsoft.Xna.Framework.Rectangle(112, 1424, 30, 24), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07681f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(1536f, 728f)), new Microsoft.Xna.Framework.Rectangle(142, 1424, 16, 3), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07682f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(1544f, 600f)), new Microsoft.Xna.Framework.Rectangle(71, 1966, 18, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07678001f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(1472f, 608f)), new Microsoft.Xna.Framework.Rectangle(167, 1966, 18, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.07678001f);
				if (Game1.timeOfDay >= 2000)
				{
					spriteBatch.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Microsoft.Xna.Framework.Rectangle(1744, 640, 64, 64)), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.0768400058f);
				}
			}
			if (Game1.player.achievements.Count > 0)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(hatterPos), hatterSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6016f);
			}
		}
	}
}
