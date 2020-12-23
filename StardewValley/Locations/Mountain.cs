using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Events;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System.Xml.Serialization;
using xTile;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class Mountain : GameLocation
	{
		public const int daysBeforeLandslide = 31;

		private TemporaryAnimatedSprite minecartSteam;

		private bool bridgeRestored;

		[XmlIgnore]
		public bool treehouseBuilt;

		[XmlIgnore]
		public bool treehouseDoorDirty;

		private readonly NetBool oreBoulderPresent = new NetBool();

		private readonly NetBool railroadAreaBlocked = new NetBool(Game1.stats.DaysPlayed < 31);

		private readonly NetBool landslide = new NetBool(Game1.stats.DaysPlayed < 5);

		private Microsoft.Xna.Framework.Rectangle landSlideRect = new Microsoft.Xna.Framework.Rectangle(3200, 256, 192, 320);

		private Microsoft.Xna.Framework.Rectangle railroadBlockRect = new Microsoft.Xna.Framework.Rectangle(512, 0, 256, 320);

		private int oldTime;

		private Microsoft.Xna.Framework.Rectangle boulderSourceRect = new Microsoft.Xna.Framework.Rectangle(439, 1385, 39, 48);

		private Microsoft.Xna.Framework.Rectangle raildroadBlocksourceRect = new Microsoft.Xna.Framework.Rectangle(640, 2176, 64, 80);

		private Microsoft.Xna.Framework.Rectangle landSlideSourceRect = new Microsoft.Xna.Framework.Rectangle(646, 1218, 48, 80);

		private Vector2 boulderPosition = new Vector2(47f, 3f) * 64f - new Vector2(4f, 3f) * 4f;

		public Mountain()
		{
		}

		public Mountain(string map, string name)
			: base(map, name)
		{
			for (int i = 0; i < 10; i++)
			{
				quarryDayUpdate();
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(oreBoulderPresent, railroadAreaBlocked, landslide);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				switch (map.GetLayer("Buildings").Tiles[tileLocation].TileIndex)
				{
				case 1136:
					if (!who.mailReceived.Contains("guildMember") && !who.hasQuest(16))
					{
						Game1.drawLetterMessage(Game1.content.LoadString("Strings\\Locations:Mountain_AdventurersGuildNote").Replace('\n', '^'));
						return true;
					}
					break;
				case 958:
				case 1080:
				case 1081:
					if (Game1.player.mount != null)
					{
						return true;
					}
					if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
					{
						if (Game1.player.isRidingHorse() && Game1.player.mount != null)
						{
							Game1.player.mount.checkAction(Game1.player, this);
							break;
						}
						Response[] destinations = new Response[4]
						{
							new Response("Bus", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_BusStop")),
							new Response("Mines", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Mines")),
							new Response("Town", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Town")),
							new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
						};
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), destinations, "Minecart");
						break;
					}
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public void ApplyTreehouseIfNecessary()
		{
			if (((Game1.farmEvent != null && Game1.farmEvent is WorldChangeEvent && (int)(Game1.farmEvent as WorldChangeEvent).whichEvent == 14) || Game1.MasterPlayer.mailReceived.Contains("leoMoved") || Game1.MasterPlayer.mailReceived.Contains("leoMoved%&NL&%")) && !treehouseBuilt)
			{
				TileSheet tilesheet = map.GetTileSheet("untitled tile sheet2");
				map.GetLayer("Buildings").Tiles[16, 6] = new StaticTile(map.GetLayer("Buildings"), tilesheet, BlendMode.Alpha, 197);
				map.GetLayer("Buildings").Tiles[16, 7] = new StaticTile(map.GetLayer("Buildings"), tilesheet, BlendMode.Alpha, 213);
				map.GetLayer("Back").Tiles[16, 8] = new StaticTile(map.GetLayer("Back"), tilesheet, BlendMode.Alpha, 229);
				map.GetLayer("Buildings").Tiles[16, 7].Properties["Action"] = new PropertyValue("LockedDoorWarp 3 8 LeoTreeHouse 600 2300");
				treehouseBuilt = true;
				if (Game1.IsMasterGame)
				{
					updateDoors();
					treehouseDoorDirty = true;
				}
			}
		}

		private void restoreBridge()
		{
			LocalizedContentManager temp2 = Game1.content.CreateTemporary();
			Map i = temp2.Load<Map>("Maps\\Mountain-BridgeFixed");
			int xOffset = 92;
			int yOffset = 24;
			for (int x = 0; x < i.GetLayer("Back").LayerWidth; x++)
			{
				for (int y = 0; y < i.GetLayer("Back").LayerHeight; y++)
				{
					map.GetLayer("Back").Tiles[x + xOffset, y + yOffset] = ((i.GetLayer("Back").Tiles[x, y] == null) ? null : new StaticTile(map.GetLayer("Back"), map.TileSheets[0], BlendMode.Alpha, i.GetLayer("Back").Tiles[x, y].TileIndex));
					map.GetLayer("Buildings").Tiles[x + xOffset, y + yOffset] = ((i.GetLayer("Buildings").Tiles[x, y] == null) ? null : new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, i.GetLayer("Buildings").Tiles[x, y].TileIndex));
					map.GetLayer("Front").Tiles[x + xOffset, y + yOffset] = ((i.GetLayer("Front").Tiles[x, y] == null) ? null : new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, i.GetLayer("Front").Tiles[x, y].TileIndex));
				}
			}
			bridgeRestored = true;
			temp2.Unload();
			temp2 = null;
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			oreBoulderPresent.Value = (!Game1.MasterPlayer.mailReceived.Contains("ccFishTank") || Game1.farmEvent != null);
			if (!objects.ContainsKey(new Vector2(29f, 9f)))
			{
				Vector2 tile = new Vector2(29f, 9f);
				objects.Add(tile, new Torch(tile, 146, bigCraftable: true)
				{
					IsOn = false,
					Fragility = 2
				});
				objects[tile].checkForAction(null);
			}
			if (Game1.stats.DaysPlayed >= 5)
			{
				landslide.Value = false;
			}
			if (Game1.stats.DaysPlayed >= 31)
			{
				railroadAreaBlocked.Value = false;
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
			{
				minecartSteam = new TemporaryAnimatedSprite(27, new Vector2(8072f, 656f), Color.White)
				{
					totalNumberOfLoops = 999999,
					interval = 60f,
					flipped = true
				};
			}
			if (!bridgeRestored && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccCraftsRoom"))
			{
				restoreBridge();
			}
			boulderSourceRect = new Microsoft.Xna.Framework.Rectangle(439 + (Game1.currentSeason.Equals("winter") ? 39 : 0), 1385, 39, 48);
			if (Game1.IsSpring)
			{
				raildroadBlocksourceRect = new Microsoft.Xna.Framework.Rectangle(640, 2176, 64, 80);
			}
			else
			{
				raildroadBlocksourceRect = new Microsoft.Xna.Framework.Rectangle(640, 1453, 64, 80);
			}
			addFrog();
			if (!(Game1.farmEvent is WorldChangeEvent) || (Game1.farmEvent as WorldChangeEvent).whichEvent.Value != 14)
			{
				ApplyTreehouseIfNecessary();
			}
			if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				ApplyMapOverride("Mountain_Shortcuts");
				waterTiles[81, 37] = false;
				waterTiles[82, 37] = false;
				waterTiles[83, 37] = false;
				waterTiles[84, 37] = false;
				waterTiles[85, 37] = false;
				waterTiles[85, 38] = false;
				waterTiles[85, 39] = false;
				waterTiles[85, 40] = false;
			}
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			quarryDayUpdate();
			if (Game1.stats.DaysPlayed >= 31)
			{
				railroadAreaBlocked.Value = false;
			}
			if (Game1.stats.DaysPlayed >= 5)
			{
				landslide.Value = false;
				if (!Game1.player.hasOrWillReceiveMail("landslideDone"))
				{
					Game1.mailbox.Add("landslideDone");
				}
			}
		}

		private void quarryDayUpdate()
		{
			Microsoft.Xna.Framework.Rectangle quarryBounds = new Microsoft.Xna.Framework.Rectangle(106, 13, 21, 21);
			int numberOfAdditionsToTry = 5;
			for (int i = 0; i < numberOfAdditionsToTry; i++)
			{
				Vector2 position = Utility.getRandomPositionInThisRectangle(quarryBounds, Game1.random);
				if (!isTileOpenForQuarryStone((int)position.X, (int)position.Y))
				{
					continue;
				}
				if (Game1.random.NextDouble() < 0.06)
				{
					terrainFeatures.Add(position, new Tree(1 + Game1.random.Next(2), 1));
				}
				else if (Game1.random.NextDouble() < 0.02)
				{
					if (Game1.random.NextDouble() < 0.1)
					{
						objects.Add(position, new Object(position, 46, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 12
						});
					}
					else
					{
						objects.Add(position, new Object(position, (Game1.random.Next(7) + 1) * 2, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 5
						});
					}
				}
				else if (Game1.random.NextDouble() < 0.1)
				{
					if (Game1.random.NextDouble() < 0.001)
					{
						objects.Add(position, new Object(position, 765, 1)
						{
							MinutesUntilReady = 16
						});
					}
					else if (Game1.random.NextDouble() < 0.1)
					{
						objects.Add(position, new Object(position, 764, 1)
						{
							MinutesUntilReady = 8
						});
					}
					else if (Game1.random.NextDouble() < 0.33)
					{
						objects.Add(position, new Object(position, 290, 1)
						{
							MinutesUntilReady = 5
						});
					}
					else
					{
						objects.Add(position, new Object(position, 751, 1)
						{
							MinutesUntilReady = 3
						});
					}
				}
				else
				{
					Object obj = new Object(position, (Game1.random.NextDouble() < 0.25) ? 32 : ((Game1.random.NextDouble() < 0.33) ? 38 : ((Game1.random.NextDouble() < 0.5) ? 40 : 42)), 1);
					obj.minutesUntilReady.Value = 2;
					obj.Name = "Stone";
					objects.Add(position, obj);
				}
			}
		}

		private bool isTileOpenForQuarryStone(int tileX, int tileY)
		{
			if (doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null)
			{
				return isTileLocationTotallyClearAndPlaceable(new Vector2(tileX, tileY));
			}
			return false;
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			minecartSteam = null;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (minecartSteam != null)
			{
				minecartSteam.update(time);
			}
			if ((bool)landslide && (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds - 400.0) / 1600.0) % 2 != 0 && Utility.isOnScreen(new Point(landSlideRect.X / 64, landSlideRect.Y / 64), 128))
			{
				if (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 < (double)(oldTime % 400))
				{
					localSound("hammer");
				}
				oldTime = (int)time.TotalGameTime.TotalMilliseconds;
			}
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			bool using_magic_bait = IsUsingMagicBait(who);
			float bobberAddition = 0f;
			if (who != null && who.CurrentTool is FishingRod && (who.CurrentTool as FishingRod).getBobberAttachmentIndex() == 856)
			{
				bobberAddition += 0.1f;
			}
			if (((Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY") || Game1.isRaining) | using_magic_bait) && who.FishingLevel >= 10 && waterDepth >= 4 && Game1.random.NextDouble() < 0.1 + (double)bobberAddition)
			{
				if (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY"))
				{
					return new Object(900, 1);
				}
				if (!who.fishCaught.ContainsKey(163) && (Game1.currentSeason.Equals("spring") | using_magic_bait))
				{
					return new Object(163, 1);
				}
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if ((bool)landslide && position.Intersects(landSlideRect))
			{
				return true;
			}
			if ((bool)railroadAreaBlocked && position.Intersects(railroadBlockRect))
			{
				return true;
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override void draw(SpriteBatch spriteBatch)
		{
			base.draw(spriteBatch);
			if (minecartSteam != null)
			{
				minecartSteam.draw(spriteBatch);
			}
			if ((bool)oreBoulderPresent)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, boulderPosition), boulderSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
			}
			if ((bool)railroadAreaBlocked)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, railroadBlockRect), raildroadBlocksourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.0193f);
			}
			if ((bool)landslide)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, landSlideRect), landSlideSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.0192f);
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(new Vector2(landSlideRect.X + 192 - 20, landSlideRect.Y + 192 + 20) + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0224f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(landSlideRect.X + 192 - 20, landSlideRect.Y + 128)), new Microsoft.Xna.Framework.Rectangle(288 + (((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1600.0 % 2.0) != 0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 19) : 0), 1349, 19, 28), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0256f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(landSlideRect.X + 256 - 20, landSlideRect.Y + 128)), new Microsoft.Xna.Framework.Rectangle(335, 1410, 21, 21), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0128f);
			}
		}
	}
}
