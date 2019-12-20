using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class LibraryMuseum : GameLocation
	{
		public const int dwarvenGuide = 0;

		public const int totalArtifacts = 95;

		public const int totalNotes = 21;

		[Obsolete]
		[XmlIgnore]
		private Dictionary<int, Vector2> lostBooksLocations = new Dictionary<int, Vector2>();

		private readonly NetMutex mutex = new NetMutex();

		[XmlElement("museumPieces")]
		public NetVector2Dictionary<int, NetInt> museumPieces => Game1.netWorldState.Value.MuseumPieces;

		public LibraryMuseum()
		{
		}

		public LibraryMuseum(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(mutex.NetFields);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			mutex.Update(this);
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
		}

		public bool museumAlreadyHasArtifact(int index)
		{
			foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
			{
				if (pair.Value == index)
				{
					return true;
				}
			}
			return false;
		}

		public bool isItemSuitableForDonation(Item i)
		{
			if (i is Object && (i as Object).type != null && ((i as Object).type.Equals("Arch") || (i as Object).type.Equals("Minerals")))
			{
				int index = (i as Object).parentSheetIndex;
				bool museumHasItem = false;
				foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
				{
					if (pair.Value == index)
					{
						museumHasItem = true;
						break;
					}
				}
				if (!museumHasItem)
				{
					return true;
				}
			}
			return false;
		}

		public bool doesFarmerHaveAnythingToDonate(Farmer who)
		{
			for (int i = 0; i < (int)who.maxItems; i++)
			{
				if (i < who.items.Count && who.items[i] is Object && (who.items[i] as Object).type != null && ((who.items[i] as Object).type.Equals("Arch") || (who.items[i] as Object).type.Equals("Minerals")))
				{
					int index = (who.items[i] as Object).parentSheetIndex;
					bool museumHasItem = false;
					foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
					{
						if (pair.Value == index)
						{
							museumHasItem = true;
							break;
						}
					}
					if (!museumHasItem)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool museumContainsTheseItems(int[] items, HashSet<int> museumItems)
		{
			for (int i = 0; i < items.Length; i++)
			{
				if (!museumItems.Contains(items[i]))
				{
					return false;
				}
			}
			return true;
		}

		private int numberOfMuseumItemsOfType(string type)
		{
			int num = 0;
			foreach (KeyValuePair<Vector2, int> v in museumPieces.Pairs)
			{
				if (Game1.objectInformation[v.Value].Split('/')[3].Contains(type))
				{
					num++;
				}
			}
			return num;
		}

		private Dictionary<int, Vector2> getLostBooksLocations()
		{
			Dictionary<int, Vector2> lostBooksLocations = new Dictionary<int, Vector2>();
			for (int x = 0; x < map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < map.Layers[0].LayerHeight; y++)
				{
					if (doesTileHaveProperty(x, y, "Action", "Buildings") != null && doesTileHaveProperty(x, y, "Action", "Buildings").Contains("Notes"))
					{
						lostBooksLocations.Add(Convert.ToInt32(doesTileHaveProperty(x, y, "Action", "Buildings").Split(' ')[1]), new Vector2(x, y));
					}
				}
			}
			return lostBooksLocations;
		}

		protected override void resetLocalState()
		{
			if (!Game1.player.eventsSeen.Contains(0) && doesFarmerHaveAnythingToDonate(Game1.player) && !Game1.player.mailReceived.Contains("somethingToDonate"))
			{
				Game1.player.mailReceived.Add("somethingToDonate");
			}
			if (museumPieces.Count() > 0 && !Game1.player.mailReceived.Contains("somethingWasDonated"))
			{
				Game1.player.mailReceived.Add("somethingWasDonated");
			}
			base.resetLocalState();
			if (!Game1.isRaining)
			{
				Game1.changeMusicTrack("libraryTheme");
			}
			int booksFound = Game1.netWorldState.Value.LostBooksFound;
			Dictionary<int, Vector2> lostBooksLocations = getLostBooksLocations();
			for (int i = 0; i < lostBooksLocations.Count; i++)
			{
				if (lostBooksLocations.ElementAt(i).Key <= booksFound && !Game1.player.mailReceived.Contains("lb_" + lostBooksLocations.ElementAt(i).Key))
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 447, 15, 15), new Vector2(lostBooksLocations.ElementAt(i).Value.X * 64f, lostBooksLocations.ElementAt(i).Value.Y * 64f - 96f - 16f), flipped: false, 0f, Color.White)
					{
						interval = 99999f,
						animationLength = 1,
						totalNumberOfLoops = 9999,
						yPeriodic = true,
						yPeriodicLoopTime = 4000f,
						yPeriodicRange = 16f,
						layerDepth = 1f,
						scale = 4f,
						id = lostBooksLocations.ElementAt(i).Key
					});
				}
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (!Game1.isRaining)
			{
				Game1.changeMusicTrack("none");
			}
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "Museum_Collect"))
			{
				if (!(questionAndAnswer == "Museum_Donate"))
				{
					if (questionAndAnswer == "Museum_Rearrange_Yes" && !mutex.IsLocked())
					{
						mutex.RequestLock(delegate
						{
							Game1.activeClickableMenu = new MuseumMenu(InventoryMenu.highlightNoItems)
							{
								exitFunction = delegate
								{
									mutex.ReleaseLock();
								}
							};
						});
					}
				}
				else
				{
					mutex.RequestLock(delegate
					{
						Game1.activeClickableMenu = new MuseumMenu(isItemSuitableForDonation)
						{
							exitFunction = delegate
							{
								mutex.ReleaseLock();
							}
						};
					});
				}
			}
			else
			{
				Game1.activeClickableMenu = new ItemGrabMenu(getRewardsForPlayer(Game1.player), reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", collectedReward, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, this);
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		public string getRewardItemKey(Item item)
		{
			return "museumCollectedReward" + Utility.getStandardDescriptionFromItem(item, 1, '_');
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string a = action.Split(' ')[0];
				if (a == "Gunther")
				{
					gunther();
					return true;
				}
				if (a == "Rearrange" && !doesFarmerHaveAnythingToDonate(Game1.player))
				{
					rearrange();
					return true;
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		public void rearrange()
		{
			if (museumPieces.Count() > 0)
			{
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Rearrange"), createYesNoResponses(), "Museum_Rearrange");
			}
		}

		public List<Item> getRewardsForPlayer(Farmer who)
		{
			List<Item> rewards = new List<Item>();
			HashSet<int> museumItems = new HashSet<int>(museumPieces.Values);
			int archItems = numberOfMuseumItemsOfType("Arch");
			int mineralItems = numberOfMuseumItemsOfType("Minerals");
			int total = archItems + mineralItems;
			if (!who.canUnderstandDwarves && museumItems.Contains(96) && museumItems.Contains(97) && museumItems.Contains(98) && museumItems.Contains(99))
			{
				AddRewardIfUncollected(who, rewards, new Object(326, 1));
			}
			if (!who.specialBigCraftables.Contains(1305) && museumItems.Contains(113) && archItems > 4)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1305, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1304) && archItems >= 15)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1304, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(139) && archItems >= 20)
			{
				AddRewardIfUncollected(who, rewards, new Object(Vector2.Zero, 139));
			}
			if (!who.specialBigCraftables.Contains(1545) && museumContainsTheseItems(new int[2]
			{
				108,
				122
			}, museumItems) && archItems > 10)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1545, Vector2.Zero));
			}
			if (!who.specialItems.Contains(464) && museumItems.Contains(119) && archItems > 2)
			{
				AddRewardIfUncollected(who, rewards, new Object(464, 1));
			}
			if (!who.specialItems.Contains(463) && museumItems.Contains(123) && archItems > 2)
			{
				AddRewardIfUncollected(who, rewards, new Object(463, 1));
			}
			if (!who.specialItems.Contains(499) && museumItems.Contains(114))
			{
				AddRewardIfUncollected(who, rewards, new Object(499, 1));
				AddRewardIfUncollected(who, rewards, new Object(499, 1, isRecipe: true));
			}
			if (!who.specialBigCraftables.Contains(1301) && museumContainsTheseItems(new int[3]
			{
				579,
				581,
				582
			}, museumItems))
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1301, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1302) && museumContainsTheseItems(new int[2]
			{
				583,
				584
			}, museumItems))
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1302, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1303) && museumContainsTheseItems(new int[2]
			{
				580,
				585
			}, museumItems))
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1303, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1298) && mineralItems > 10)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1298, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1299) && mineralItems > 30)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1299, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(94) && mineralItems > 20)
			{
				AddRewardIfUncollected(who, rewards, new Object(Vector2.Zero, 94));
			}
			if (!who.specialBigCraftables.Contains(21) && mineralItems >= 50)
			{
				AddRewardIfUncollected(who, rewards, new Object(Vector2.Zero, 21));
			}
			if (!who.specialBigCraftables.Contains(131) && mineralItems > 40)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(131, Vector2.Zero));
			}
			foreach (Item item in rewards)
			{
				item.specialItem = true;
			}
			if (!who.mailReceived.Contains("museum5") && total >= 5)
			{
				AddRewardIfUncollected(who, rewards, new Object(474, 9));
			}
			if (!who.mailReceived.Contains("museum10") && total >= 10)
			{
				AddRewardIfUncollected(who, rewards, new Object(479, 9));
			}
			if (!who.mailReceived.Contains("museum15") && total >= 15)
			{
				AddRewardIfUncollected(who, rewards, new Object(486, 1));
			}
			if (!who.mailReceived.Contains("museum20") && total >= 20)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1541, Vector2.Zero));
			}
			if (!who.mailReceived.Contains("museum25") && total >= 25)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1554, Vector2.Zero));
			}
			if (!who.mailReceived.Contains("museum30") && total >= 30)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1669, Vector2.Zero));
			}
			if (!who.mailReceived.Contains("museum35") && total >= 35)
			{
				AddRewardIfUncollected(who, rewards, new Object(490, 9));
			}
			if (!who.mailReceived.Contains("museum40") && total >= 40)
			{
				AddRewardIfUncollected(who, rewards, new Object(Vector2.Zero, 140));
			}
			if (!who.mailReceived.Contains("museum50") && total >= 50)
			{
				AddRewardIfUncollected(who, rewards, new Furniture(1671, Vector2.Zero));
			}
			if (!who.mailReceived.Contains("museum70") && total >= 70)
			{
				AddRewardIfUncollected(who, rewards, new Object(253, 3));
			}
			if (!who.mailReceived.Contains("museum80") && total >= 80)
			{
				AddRewardIfUncollected(who, rewards, new Object(688, 5));
			}
			if (!who.mailReceived.Contains("museum90") && total >= 90)
			{
				AddRewardIfUncollected(who, rewards, new Object(279, 1));
			}
			if (!who.mailReceived.Contains("museumComplete") && total >= 95)
			{
				AddRewardIfUncollected(who, rewards, new Object(434, 1));
			}
			if (total >= 60)
			{
				if (!Game1.player.eventsSeen.Contains(295672))
				{
					Game1.player.eventsSeen.Add(295672);
				}
				else if (!Game1.player.hasRustyKey)
				{
					Game1.player.eventsSeen.Remove(66);
				}
			}
			return rewards;
		}

		public void AddRewardIfUncollected(Farmer farmer, List<Item> rewards, Item reward_item)
		{
			if (!farmer.mailReceived.Contains(getRewardItemKey(reward_item)))
			{
				rewards.Add(reward_item);
			}
		}

		public void collectedReward(Item item, Farmer who)
		{
			if (item == null)
			{
				return;
			}
			if (item is Object)
			{
				(item as Object).specialItem = true;
				switch ((item as Object).ParentSheetIndex)
				{
				case 434:
					who.mailReceived.Add("museumComplete");
					break;
				case 474:
					who.mailReceived.Add("museum5");
					break;
				case 479:
					who.mailReceived.Add("museum10");
					break;
				case 486:
					who.mailReceived.Add("museum15");
					break;
				case 1541:
					who.mailReceived.Add("museum20");
					break;
				case 1554:
					who.mailReceived.Add("museum25");
					break;
				case 1669:
					who.mailReceived.Add("museum30");
					break;
				case 490:
					who.mailReceived.Add("museum35");
					break;
				case 140:
					who.mailReceived.Add("museum40");
					break;
				case 1671:
					who.mailReceived.Add("museum50");
					break;
				case 253:
					who.mailReceived.Add("museum70");
					break;
				case 688:
					who.mailReceived.Add("museum80");
					break;
				case 279:
					who.mailReceived.Add("museum90");
					break;
				}
			}
			if (!who.hasOrWillReceiveMail(getRewardItemKey(item)))
			{
				who.mailReceived.Add(getRewardItemKey(item));
			}
		}

		private void gunther()
		{
			if (doesFarmerHaveAnythingToDonate(Game1.player) && !mutex.IsLocked())
			{
				Response[] choice = (getRewardsForPlayer(Game1.player).Count <= 0) ? new Response[2]
				{
					new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				} : new Response[3]
				{
					new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
					new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				};
				createQuestionDialogue("", choice, "Museum");
			}
			else if (getRewardsForPlayer(Game1.player).Count > 0)
			{
				createQuestionDialogue("", new Response[2]
				{
					new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				}, "Museum");
			}
			else if (doesFarmerHaveAnythingToDonate(Game1.player) && mutex.IsLocked())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NPC_Busy", Game1.getCharacterFromName("Gunther").displayName));
			}
			else if (Game1.player.achievements.Contains(5))
			{
				Game1.drawDialogue(Game1.getCharacterFromName("Gunther"), Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_MuseumComplete")));
			}
			else
			{
				Game1.drawDialogue(Game1.getCharacterFromName("Gunther"), Game1.player.mailReceived.Contains("artifactFound") ? Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NothingToDonate")) : Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NoArtifactsFound"));
			}
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			foreach (KeyValuePair<Vector2, int> v in museumPieces.Pairs)
			{
				if (v.Key.X == (float)tileLocation.X && (v.Key.Y == (float)tileLocation.Y || v.Key.Y == (float)(tileLocation.Y - 1)))
				{
					string displayText = Game1.objectInformation[v.Value].Split('/')[4];
					Game1.drawObjectDialogue(Game1.parseText(" - " + displayText + " - " + Environment.NewLine + Game1.objectInformation[v.Value].Split('/')[5]));
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public bool isTileSuitableForMuseumPiece(int x, int y)
		{
			Vector2 p = new Vector2(x, y);
			if (!museumPieces.ContainsKey(p))
			{
				int indexOfBuildingsLayer = getTileIndexAt(new Point(x, y), "Buildings");
				if (indexOfBuildingsLayer == 1073 || indexOfBuildingsLayer == 1074 || indexOfBuildingsLayer == 1072 || indexOfBuildingsLayer == 1237 || indexOfBuildingsLayer == 1238)
				{
					return true;
				}
			}
			return false;
		}

		public Microsoft.Xna.Framework.Rectangle getMuseumDonationBounds()
		{
			return new Microsoft.Xna.Framework.Rectangle(26, 5, 22, 13);
		}

		public Vector2 getFreeDonationSpot()
		{
			Microsoft.Xna.Framework.Rectangle bounds = getMuseumDonationBounds();
			for (int x = bounds.X; x <= bounds.Right; x++)
			{
				for (int y = bounds.Y; y <= bounds.Bottom; y++)
				{
					if (isTileSuitableForMuseumPiece(x, y))
					{
						return new Vector2(x, y);
					}
				}
			}
			return new Vector2(26f, 5f);
		}

		public Vector2 findMuseumPieceLocationInDirection(Vector2 startingPoint, int direction, int distanceToCheck = 8, bool ignoreExistingItems = true)
		{
			Vector2 checkTile = startingPoint;
			Vector2 offset = Vector2.Zero;
			switch (direction)
			{
			case 0:
				offset = new Vector2(0f, -1f);
				break;
			case 1:
				offset = new Vector2(1f, 0f);
				break;
			case 2:
				offset = new Vector2(0f, 1f);
				break;
			case 3:
				offset = new Vector2(-1f, 0f);
				break;
			}
			for (int j = 0; j < distanceToCheck; j++)
			{
				for (int i = 0; i < distanceToCheck; i++)
				{
					checkTile += offset;
					if (isTileSuitableForMuseumPiece((int)checkTile.X, (int)checkTile.Y) || (!ignoreExistingItems && museumPieces.ContainsKey(checkTile)))
					{
						return checkTile;
					}
				}
				checkTile = startingPoint;
				int sign = (j % 2 != 0) ? 1 : (-1);
				switch (direction)
				{
				case 0:
				case 2:
					checkTile.X += sign * (j / 2 + 1);
					break;
				case 1:
				case 3:
					checkTile.Y += sign * (j / 2 + 1);
					break;
				}
			}
			return startingPoint;
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			foreach (TemporaryAnimatedSprite t in temporarySprites)
			{
				if (t.layerDepth >= 1f)
				{
					t.draw(b);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (KeyValuePair<Vector2, int> v in museumPieces.Pairs)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, v.Key * 64f + new Vector2(32f, 52f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (v.Key.Y * 64f - 2f) / 10000f);
				b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, v.Key * 64f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, v.Value, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, v.Key.Y * 64f / 10000f);
			}
		}
	}
}
