using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Network;
using System;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class Beach : GameLocation
	{
		private NPC oldMariner;

		[XmlElement("bridgeFixed")]
		public readonly NetBool bridgeFixed = new NetBool();

		public Beach()
		{
		}

		public Beach(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(bridgeFixed);
			bridgeFixed.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && mapPath.Value != null)
				{
					fixBridge(this);
				}
			};
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (wasUpdated)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			if (oldMariner != null)
			{
				oldMariner.update(time, this);
			}
			if (Game1.eventUp || !(Game1.random.NextDouble() < 1E-06))
			{
				return;
			}
			Vector2 position = new Vector2(Game1.random.Next(15, 47) * 64, Game1.random.Next(29, 42) * 64);
			bool draw = true;
			for (float i = position.Y / 64f; i < (float)map.GetLayer("Back").LayerHeight; i += 1f)
			{
				if (doesTileHaveProperty((int)position.X / 64, (int)i, "Water", "Back") == null || doesTileHaveProperty((int)position.X / 64 - 1, (int)i, "Water", "Back") == null || doesTileHaveProperty((int)position.X / 64 + 1, (int)i, "Water", "Back") == null)
				{
					draw = false;
					break;
				}
			}
			if (draw)
			{
				temporarySprites.Add(new SeaMonsterTemporarySprite(250f, 4, Game1.random.Next(7), position));
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (!Game1.isRaining && !Game1.isFestival())
			{
				Game1.changeMusicTrack("none");
			}
			oldMariner = null;
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if (Game1.currentSeason.Equals("summer") && who.getTileX() >= 82 && who.FishingLevel >= 5 && !who.fishCaught.ContainsKey(159) && waterDepth >= 3 && Game1.random.NextDouble() < 0.18)
			{
				return new Object(159, 1);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			Microsoft.Xna.Framework.Rectangle tidePools = new Microsoft.Xna.Framework.Rectangle(65, 11, 25, 12);
			float chance3 = 1f;
			while (Game1.random.NextDouble() < (double)chance3)
			{
				int index = 393;
				if (Game1.random.NextDouble() < 0.2)
				{
					index = 397;
				}
				Vector2 position = new Vector2(Game1.random.Next(tidePools.X, tidePools.Right), Game1.random.Next(tidePools.Y, tidePools.Bottom));
				if (isTileLocationTotallyClearAndPlaceable(position))
				{
					dropObject(new Object(index, 1), position * 64f, Game1.viewport, initialPlacement: true);
				}
				chance3 /= 2f;
			}
			Microsoft.Xna.Framework.Rectangle seaweedShore = new Microsoft.Xna.Framework.Rectangle(66, 24, 19, 1);
			chance3 = 0.25f;
			while (Game1.random.NextDouble() < (double)chance3)
			{
				if (Game1.random.NextDouble() < 0.1)
				{
					Vector2 position2 = new Vector2(Game1.random.Next(seaweedShore.X, seaweedShore.Right), Game1.random.Next(seaweedShore.Y, seaweedShore.Bottom));
					if (isTileLocationTotallyClearAndPlaceable(position2))
					{
						dropObject(new Object(152, 1), position2 * 64f, Game1.viewport, initialPlacement: true);
					}
				}
				chance3 /= 2f;
			}
			if (!Game1.currentSeason.Equals("summer") || Game1.dayOfMonth < 12 || Game1.dayOfMonth > 14)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				spawnObjects();
			}
			chance3 = 1.5f;
			while (Game1.random.NextDouble() < (double)chance3)
			{
				int index2 = 393;
				if (Game1.random.NextDouble() < 0.2)
				{
					index2 = 397;
				}
				Vector2 position3 = getRandomTile();
				position3.Y /= 2f;
				string prop = doesTileHaveProperty((int)position3.X, (int)position3.Y, "Type", "Back");
				if (isTileLocationTotallyClearAndPlaceable(position3) && (prop == null || !prop.Equals("Wood")))
				{
					dropObject(new Object(index2, 1), position3 * 64f, Game1.viewport, initialPlacement: true);
				}
				chance3 /= 1.1f;
			}
		}

		public void doneWithBridgeFix()
		{
			Game1.globalFadeToClear();
			Game1.viewportFreeze = false;
		}

		public void fadedForBridgeFix()
		{
			DelayedAction.playSoundAfterDelay("crafting", 1000);
			DelayedAction.playSoundAfterDelay("crafting", 1500);
			DelayedAction.playSoundAfterDelay("crafting", 2000);
			DelayedAction.playSoundAfterDelay("crafting", 2500);
			DelayedAction.playSoundAfterDelay("axchop", 3000);
			DelayedAction.playSoundAfterDelay("Ship", 3200);
			Game1.viewportFreeze = true;
			Game1.viewport.X = -10000;
			bridgeFixed.Value = true;
			Game1.pauseThenDoFunction(4000, doneWithBridgeFix);
			fixBridge(this);
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer != null && questionAndAnswer.Equals("BeachBridge_Yes"))
			{
				Game1.globalFadeToBlack(fadedForBridgeFix);
				Game1.player.removeItemsFromInventory(388, 300);
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			switch ((map.GetLayer("Buildings").Tiles[tileLocation] != null) ? map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : (-1))
			{
			case 284:
				if (who.hasItemInInventory(388, 300))
				{
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Question"), createYesNoResponses(), "BeachBridge");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Hint"));
				}
				break;
			case 496:
				if (!Game1.MasterPlayer.mailReceived.Contains("spring_2_1"))
				{
					Game1.drawLetterMessage(Game1.content.LoadString("Strings\\Locations:Beach_GoneFishingMessage").Replace('\n', '^'));
					return false;
				}
				break;
			}
			if (oldMariner != null && oldMariner.getTileX() == tileLocation.X && oldMariner.getTileY() == tileLocation.Y)
			{
				string playerTerm = Game1.content.LoadString("Strings\\Locations:Beach_Mariner_Player_" + (who.IsMale ? "Male" : "Female"));
				if (!who.isMarried() && who.specialItems.Contains(460) && !Utility.doesItemWithThisIndexExistAnywhere(460))
				{
					for (int i = who.specialItems.Count - 1; i >= 0; i--)
					{
						if (who.specialItems[i] == 460)
						{
							who.specialItems.RemoveAt(i);
						}
					}
				}
				if (who.isMarried())
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerMarried", playerTerm)));
				}
				else if (who.specialItems.Contains(460))
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerHasItem", playerTerm)));
				}
				else if (who.hasAFriendWithHeartLevel(10, datablesOnly: true) && (int)who.houseUpgradeLevel == 0)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNotUpgradedHouse", playerTerm)));
				}
				else if (who.hasAFriendWithHeartLevel(10, datablesOnly: true))
				{
					Response[] answers = new Response[2]
					{
						new Response("Buy", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerYes")),
						new Response("Not", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerNo"))
					};
					createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_Question", playerTerm)), answers, "mariner");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNoRelationship", playerTerm)));
				}
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if (oldMariner != null && position.Intersects(oldMariner.GetBoundingBox()))
			{
				return true;
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override void checkForMusic(GameTime time)
		{
			if (Game1.random.NextDouble() < 0.003 && Game1.timeOfDay < 1900)
			{
				localSound("seagulls");
			}
			base.checkForMusic(time);
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (Game1.currentSeason.Equals("summer") && Game1.dayOfMonth >= 12 && Game1.dayOfMonth <= 14)
			{
				waterColor.Value = new Color(0, 255, 0) * 0.4f;
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!Game1.isRaining && !Game1.isFestival())
			{
				Game1.changeMusicTrack("ocean");
			}
			int numSeagulls = Game1.random.Next(6);
			foreach (Vector2 tile in Utility.getPositionsInClusterAroundThisTile(new Vector2(Game1.random.Next(map.DisplayWidth / 64), Game1.random.Next(12, map.DisplayHeight / 64)), numSeagulls))
			{
				if (isTileOnMap(tile) && (isTileLocationTotallyClearAndPlaceable(tile) || doesTileHaveProperty((int)tile.X, (int)tile.Y, "Water", "Back") != null))
				{
					int state = 3;
					if (doesTileHaveProperty((int)tile.X, (int)tile.Y, "Water", "Back") != null)
					{
						state = 2;
						if (Game1.random.NextDouble() < 0.5)
						{
							continue;
						}
					}
					critters.Add(new Seagull(tile * 64f + new Vector2(32f, 32f), state));
				}
			}
			if (Game1.isRaining && Game1.timeOfDay < 1900)
			{
				oldMariner = new NPC(new AnimatedSprite("Characters\\Mariner", 0, 16, 32), new Vector2(80f, 5f) * 64f, 2, "Old Mariner");
			}
			if ((bool)bridgeFixed)
			{
				fixBridge(this);
			}
		}

		public static void fixBridge(GameLocation location)
		{
			if (!NetWorldState.checkAnywhereForWorldStateID("beachBridgeFixed"))
			{
				NetWorldState.addWorldStateIDEverywhere("beachBridgeFixed");
			}
			location.updateMap();
			int whichTileSheet = (!location.name.Value.Contains("Market")) ? 1 : 2;
			location.setMapTile(58, 13, 301, "Buildings", null, whichTileSheet);
			location.setMapTile(59, 13, 301, "Buildings", null, whichTileSheet);
			location.setMapTile(60, 13, 301, "Buildings", null, whichTileSheet);
			location.setMapTile(61, 13, 301, "Buildings", null, whichTileSheet);
			location.setMapTile(58, 14, 336, "Back", null, whichTileSheet);
			location.setMapTile(59, 14, 336, "Back", null, whichTileSheet);
			location.setMapTile(60, 14, 336, "Back", null, whichTileSheet);
			location.setMapTile(61, 14, 336, "Back", null, whichTileSheet);
		}

		public override void draw(SpriteBatch b)
		{
			if (oldMariner != null)
			{
				oldMariner.draw(b);
			}
			base.draw(b);
			if (!bridgeFixed)
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3704f, 720f + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.095401f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3744f, 760f + yOffset)), new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.09541f);
			}
		}
	}
}
