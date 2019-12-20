using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Util;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class FarmerTeam : INetObject<NetFields>
	{
		public enum RemoteBuildingPermissions
		{
			Off,
			OwnedBuildings,
			On
		}

		public enum SleepAnnounceModes
		{
			All,
			First,
			Off
		}

		public readonly NetIntDelta money = new NetIntDelta(500);

		public readonly NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>> individualMoney = new NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>>();

		public readonly NetIntDelta totalMoneyEarned = new NetIntDelta(0);

		public readonly NetBool hasRustyKey = new NetBool();

		public readonly NetBool hasSkullKey = new NetBool();

		public readonly NetBool canUnderstandDwarves = new NetBool();

		public readonly NetBool useSeparateWallets = new NetBool();

		public NetIntDictionary<long, NetLong> cellarAssignments = new NetIntDictionary<long, NetLong>();

		public NetStringList broadcastedMail = new NetStringList();

		public readonly NetFarmerCollection announcedSleepingFarmers = new NetFarmerCollection();

		public readonly NetEnum<SleepAnnounceModes> sleepAnnounceMode = new NetEnum<SleepAnnounceModes>(SleepAnnounceModes.All);

		public readonly NetEnum<RemoteBuildingPermissions> farmhandsCanMoveBuildings = new NetEnum<RemoteBuildingPermissions>(RemoteBuildingPermissions.Off);

		private readonly NetStringDictionary<ReadyCheck, NetRef<ReadyCheck>> readyChecks = new NetStringDictionary<ReadyCheck, NetRef<ReadyCheck>>();

		private readonly NetLongDictionary<Proposal, NetRef<Proposal>> proposals = new NetLongDictionary<Proposal, NetRef<Proposal>>();

		public readonly NetList<MovieInvitation, NetRef<MovieInvitation>> movieInvitations = new NetList<MovieInvitation, NetRef<MovieInvitation>>();

		public readonly NetCollection<Item> luauIngredients = new NetCollection<Item>();

		public readonly NetCollection<Item> grangeDisplay = new NetCollection<Item>();

		public readonly NetMutex grangeMutex = new NetMutex();

		private readonly NetEvent1Field<Rectangle, NetRectangle> festivalPropRemovalEvent = new NetEvent1Field<Rectangle, NetRectangle>();

		public readonly NetEvent1Field<long, NetLong> requestSpouseSleepEvent = new NetEvent1Field<long, NetLong>();

		public readonly NetEvent1Field<long, NetLong> requestPetWarpHomeEvent = new NetEvent1Field<long, NetLong>();

		public readonly NetEvent1Field<long, NetLong> requestMovieEndEvent = new NetEvent1Field<long, NetLong>();

		public readonly NetEvent1Field<long, NetLong> endMovieEvent = new NetEvent1Field<long, NetLong>();

		public readonly NetEvent1Field<Guid, NetGuid> demolishStableEvent = new NetEvent1Field<Guid, NetGuid>();

		public readonly NetFarmerPairDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetFarmerPairDictionary<Friendship, NetRef<Friendship>>();

		public readonly NetWitnessedLock demolishLock = new NetWitnessedLock();

		public readonly NetMutex buildLock = new NetMutex();

		public readonly NetMutex movieMutex = new NetMutex();

		public readonly SynchronizedShopStock synchronizedShopStock = new SynchronizedShopStock();

		public readonly NetLong theaterBuildDate = new NetLong(-1L);

		public readonly NetInt lastDayQueenOfSauceRerunUpdated = new NetInt(0);

		public readonly NetInt queenOfSauceRerunWeek = new NetInt(1);

		public readonly NetDouble sharedDailyLuck = new NetDouble(0.0010000000474974513);

		public readonly NetBool spawnMonstersAtNight = new NetBool(value: false);

		public readonly NetLeaderboards junimoKartScores = new NetLeaderboards();

		public PlayerStatusList junimoKartStatus = new PlayerStatusList();

		public PlayerStatusList endOfNightStatus = new PlayerStatusList();

		public PlayerStatusList festivalScoreStatus = new PlayerStatusList();

		public PlayerStatusList sleepStatus = new PlayerStatusList();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public FarmerTeam()
		{
			NetFields.AddFields(money, totalMoneyEarned, hasRustyKey, hasSkullKey, canUnderstandDwarves, readyChecks, proposals, luauIngredients, grangeDisplay, grangeMutex.NetFields, festivalPropRemovalEvent, friendshipData, demolishLock.NetFields, buildLock.NetFields, movieInvitations, movieMutex.NetFields, requestMovieEndEvent, endMovieEvent, requestSpouseSleepEvent, useSeparateWallets, individualMoney, announcedSleepingFarmers.NetFields, sleepAnnounceMode, theaterBuildDate, demolishStableEvent, queenOfSauceRerunWeek, lastDayQueenOfSauceRerunUpdated, broadcastedMail, sharedDailyLuck, spawnMonstersAtNight, junimoKartScores.NetFields, cellarAssignments, synchronizedShopStock.NetFields, junimoKartStatus.NetFields, endOfNightStatus.NetFields, festivalScoreStatus.NetFields, sleepStatus.NetFields, farmhandsCanMoveBuildings, requestPetWarpHomeEvent);
			junimoKartStatus.sortMode = PlayerStatusList.SortMode.NumberSortDescending;
			festivalScoreStatus.sortMode = PlayerStatusList.SortMode.NumberSortDescending;
			endOfNightStatus.displayMode = PlayerStatusList.DisplayMode.Icons;
			endOfNightStatus.AddSpriteDefinition("sleep", "LooseSprites\\PlayerStatusList", 0, 0, 16, 16);
			endOfNightStatus.AddSpriteDefinition("level", "LooseSprites\\PlayerStatusList", 16, 0, 16, 16);
			endOfNightStatus.AddSpriteDefinition("shipment", "LooseSprites\\PlayerStatusList", 32, 0, 16, 16);
			endOfNightStatus.AddSpriteDefinition("ready", "LooseSprites\\PlayerStatusList", 48, 0, 16, 16);
			endOfNightStatus.iconAnimationFrames = 4;
			money.Minimum = 0;
			festivalPropRemovalEvent.onEvent += delegate(Rectangle rect)
			{
				if (Game1.CurrentEvent != null)
				{
					Game1.CurrentEvent.removeFestivalProps(rect);
				}
			};
			requestSpouseSleepEvent.onEvent += OnRequestSpouseSleepEvent;
			requestPetWarpHomeEvent.onEvent += OnRequestPetWarpHomeEvent;
			requestMovieEndEvent.onEvent += OnRequestMovieEndEvent;
			endMovieEvent.onEvent += OnEndMovieEvent;
			demolishStableEvent.onEvent += OnDemolishStableEvent;
		}

		public int GetIndividualMoney(Farmer who)
		{
			return GetMoney(who).Value;
		}

		public void AddIndividualMoney(Farmer who, int value)
		{
			GetMoney(who).Value += value;
		}

		public void SetIndividualMoney(Farmer who, int value)
		{
			GetMoney(who).Value = value;
		}

		public NetIntDelta GetMoney(Farmer who)
		{
			if ((bool)useSeparateWallets)
			{
				if (!individualMoney.ContainsKey(who.UniqueMultiplayerID))
				{
					individualMoney[who.uniqueMultiplayerID] = new NetIntDelta(500);
					individualMoney[who.uniqueMultiplayerID].Minimum = 0;
				}
				return individualMoney[who.uniqueMultiplayerID];
			}
			return money;
		}

		public void OnRequestMovieEndEvent(long uid)
		{
			if (Game1.IsMasterGame)
			{
				(Game1.getLocationFromName("MovieTheater") as MovieTheater).RequestEndMovie(uid);
			}
		}

		public void OnRequestPetWarpHomeEvent(long uid)
		{
			if (Game1.IsMasterGame)
			{
				Farmer farmer = Game1.getFarmerMaybeOffline(uid);
				if (farmer == null)
				{
					farmer = Game1.MasterPlayer;
				}
				Pet pet = Game1.getCharacterFromName<Pet>(farmer.getPetName(), mustBeVillager: false);
				if (pet == null || !(pet.currentLocation is FarmHouse))
				{
					pet?.warpToFarmHouse(farmer);
				}
			}
		}

		public void OnRequestSpouseSleepEvent(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farmer farmer = Game1.getFarmerMaybeOffline(uid);
			if (farmer != null)
			{
				NPC spouse = Game1.getCharacterFromName(farmer.spouse);
				if (spouse != null && !spouse.isSleeping.Value)
				{
					FarmHouse farm_house = Utility.getHomeOfFarmer(farmer);
					Game1.warpCharacter(spouse, farm_house, new Vector2(farm_house.getSpouseBedSpot(farmer.spouse).X, farm_house.getSpouseBedSpot(farmer.spouse).Y));
					spouse.Halt();
					spouse.faceDirection(0);
					spouse.controller = null;
					spouse.temporaryController = null;
					spouse.ignoreScheduleToday = true;
					FarmHouse.spouseSleepEndFunction(spouse, farm_house);
				}
			}
		}

		public void OnEndMovieEvent(long uid)
		{
			if (Game1.player.UniqueMultiplayerID == uid)
			{
				Game1.player.lastSeenMovieWeek.Set(Game1.Date.TotalWeeks);
				if (Game1.CurrentEvent != null)
				{
					Event currentEvent = Game1.CurrentEvent;
					currentEvent.onEventFinished = (Action)Delegate.Combine(currentEvent.onEventFinished, (Action)delegate
					{
						LocationRequest locationRequest = Game1.getLocationRequest("MovieTheater");
						locationRequest.OnWarp += delegate
						{
						};
						Game1.warpFarmer(locationRequest, 13, 4, 2);
						Game1.fadeToBlackAlpha = 1f;
					});
					Game1.CurrentEvent.endBehaviors(new string[1]
					{
						"end"
					}, Game1.currentLocation);
				}
			}
		}

		public void OnDemolishStableEvent(Guid stable_guid)
		{
			if (Game1.player.mount != null && Game1.player.mount.HorseId == stable_guid)
			{
				Game1.player.mount.dismount(from_demolish: true);
			}
		}

		public void DeleteFarmhand(Farmer farmhand)
		{
			friendshipData.Filter((KeyValuePair<FarmerPair, Friendship> pair) => !pair.Key.Contains(farmhand.UniqueMultiplayerID));
		}

		public Friendship GetFriendship(long farmer1, long farmer2)
		{
			FarmerPair pair = FarmerPair.MakePair(farmer1, farmer2);
			if (!friendshipData.ContainsKey(pair))
			{
				friendshipData.Add(pair, new Friendship());
			}
			return friendshipData[pair];
		}

		public void AddAnyBroadcastedMail()
		{
			foreach (string broadcast_mail_key in broadcastedMail)
			{
				Multiplayer.PartyWideMessageQueue mail_queue = Multiplayer.PartyWideMessageQueue.SeenMail;
				string mail_key = broadcast_mail_key;
				if (mail_key.StartsWith("%&SM&%"))
				{
					mail_key = mail_key.Substring("%&SM&%".Length);
					mail_queue = Multiplayer.PartyWideMessageQueue.SeenMail;
				}
				else if (mail_key.StartsWith("%&MFT&%"))
				{
					mail_key = mail_key.Substring("%&MFT&%".Length);
					mail_queue = Multiplayer.PartyWideMessageQueue.MailForTomorrow;
				}
				if (mail_queue == Multiplayer.PartyWideMessageQueue.SeenMail)
				{
					if (mail_key.Contains("%&NL&%"))
					{
						mail_key = mail_key.Replace("%&NL&%", "");
					}
					if (!Game1.player.mailReceived.Contains(mail_key))
					{
						Game1.player.mailReceived.Add(mail_key);
					}
				}
				else if (!Game1.MasterPlayer.mailForTomorrow.Contains(broadcast_mail_key))
				{
					if (mail_key.Contains("%&NL&%"))
					{
						string stripped = mail_key.Replace("%&NL&%", "");
						if (!Game1.player.mailReceived.Contains(stripped))
						{
							Game1.player.mailReceived.Add(stripped);
						}
					}
					else if (!Game1.player.mailbox.Contains(mail_key))
					{
						Game1.player.mailbox.Add(mail_key);
					}
				}
				else
				{
					Game1.player.mailForTomorrow.Add(mail_key);
				}
			}
		}

		public bool IsMarried(long farmer)
		{
			foreach (KeyValuePair<FarmerPair, Friendship> kvpair in friendshipData.Pairs)
			{
				if (kvpair.Key.Contains(farmer) && kvpair.Value.IsMarried())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsEngaged(long farmer)
		{
			foreach (KeyValuePair<FarmerPair, Friendship> kvpair in friendshipData.Pairs)
			{
				if (kvpair.Key.Contains(farmer) && kvpair.Value.IsEngaged())
				{
					return true;
				}
			}
			return false;
		}

		public long? GetSpouse(long farmer)
		{
			foreach (KeyValuePair<FarmerPair, Friendship> kvpair in friendshipData.Pairs)
			{
				if (kvpair.Key.Contains(farmer) && (kvpair.Value.IsEngaged() || kvpair.Value.IsMarried()))
				{
					return kvpair.Key.GetOther(farmer);
				}
			}
			return null;
		}

		public void FestivalPropsRemoved(Rectangle rect)
		{
			festivalPropRemovalEvent.Fire(rect);
		}

		public void SendProposal(Farmer receiver, ProposalType proposalType, Item gift = null)
		{
			Proposal proposal = new Proposal();
			proposal.sender.Value = Game1.player;
			proposal.receiver.Value = receiver;
			proposal.proposalType.Value = proposalType;
			proposal.gift.Value = gift;
			proposals[Game1.player.UniqueMultiplayerID] = proposal;
		}

		public Proposal GetOutgoingProposal()
		{
			if (proposals.TryGetValue(Game1.player.UniqueMultiplayerID, out Proposal proposal))
			{
				return proposal;
			}
			return null;
		}

		public void RemoveOutgoingProposal()
		{
			proposals.Remove(Game1.player.UniqueMultiplayerID);
		}

		public Proposal GetIncomingProposal()
		{
			foreach (Proposal proposal in proposals.Values)
			{
				if (proposal.receiver.Value == Game1.player && proposal.response.Value == ProposalResponse.None)
				{
					return proposal;
				}
			}
			return null;
		}

		private bool locationsMatch(GameLocation location1, GameLocation location2)
		{
			if (location1 == null || location2 == null)
			{
				return false;
			}
			if (location1.Name == location2.Name)
			{
				return true;
			}
			if ((location1 is Mine || (location1 is MineShaft && Convert.ToInt32(location1.Name.Substring("UndergroundMine".Length)) < 121)) && (location2 is Mine || (location2 is MineShaft && Convert.ToInt32(location2.Name.Substring("UndergroundMine".Length)) < 121)))
			{
				return true;
			}
			if ((location1.Name.Equals("SkullCave") || (location1 is MineShaft && Convert.ToInt32(location1.Name.Substring("UndergroundMine".Length)) >= 121)) && (location2.Name.Equals("SkullCave") || (location2 is MineShaft && Convert.ToInt32(location2.Name.Substring("UndergroundMine".Length)) >= 121)))
			{
				return true;
			}
			return false;
		}

		public double AverageDailyLuck(GameLocation inThisLocation = null)
		{
			double sum = 0.0;
			int count = 0;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (inThisLocation == null || locationsMatch(inThisLocation, farmer.currentLocation))
				{
					sum += farmer.DailyLuck;
					count++;
				}
			}
			return sum / (double)Math.Max(count, 1);
		}

		public double AverageLuckLevel(GameLocation inThisLocation = null)
		{
			double sum = 0.0;
			int count = 0;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (inThisLocation == null || locationsMatch(inThisLocation, farmer.currentLocation))
				{
					sum += (double)farmer.LuckLevel;
					count++;
				}
			}
			return sum / (double)Math.Max(count, 1);
		}

		public double AverageSkillLevel(int skillIndex, GameLocation inThisLocation = null)
		{
			double sum = 0.0;
			int count = 0;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (inThisLocation == null || locationsMatch(inThisLocation, farmer.currentLocation))
				{
					sum += (double)farmer.GetSkillLevel(skillIndex);
					count++;
				}
			}
			return sum / (double)Math.Max(count, 1);
		}

		public void Update()
		{
			requestMovieEndEvent.Poll();
			endMovieEvent.Poll();
			festivalPropRemovalEvent.Poll();
			demolishStableEvent.Poll();
			requestSpouseSleepEvent.Poll();
			requestPetWarpHomeEvent.Poll();
			grangeMutex.Update(Game1.getOnlineFarmers());
			demolishLock.Update();
			buildLock.Update(Game1.getOnlineFarmers());
			movieMutex.Update(Game1.getOnlineFarmers());
			if (grangeMutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				grangeMutex.ReleaseLock();
			}
			foreach (ReadyCheck value in readyChecks.Values)
			{
				value.Update();
			}
			if (Game1.IsMasterGame && proposals.Count() > 0)
			{
				proposals.Filter((KeyValuePair<long, Proposal> pair) => playerIsOnline(pair.Key) && playerIsOnline(pair.Value.receiver.UID));
			}
			Proposal proposal = GetIncomingProposal();
			if (proposal != null && proposal.canceled.Value)
			{
				proposal.cancelConfirmed.Value = true;
			}
			if (Game1.dialogueUp)
			{
				return;
			}
			if (proposal != null)
			{
				if (!handleIncomingProposal(proposal))
				{
					proposal.responseMessageKey.Value = genderedKey("Strings\\UI:Proposal_PlayerBusy", Game1.player);
					proposal.response.Value = ProposalResponse.Rejected;
				}
			}
			else if (Game1.activeClickableMenu == null && GetOutgoingProposal() != null)
			{
				Game1.activeClickableMenu = new PendingProposalDialog();
			}
		}

		private string genderedKey(string baseKey, Farmer farmer)
		{
			return baseKey + (farmer.IsMale ? "_Male" : "_Female");
		}

		private bool handleIncomingProposal(Proposal proposal)
		{
			if (Game1.gameMode != 3 || Game1.activeClickableMenu != null || Game1.currentMinigame != null)
			{
				return (ProposalType)proposal.proposalType == ProposalType.Baby;
			}
			if (Game1.currentLocation == null)
			{
				return false;
			}
			if (proposal.proposalType.Value != ProposalType.Dance && Game1.CurrentEvent != null)
			{
				return false;
			}
			string additionalVar = "";
			string responseYes = null;
			string responseNo = null;
			string questionKey2;
			if ((ProposalType)proposal.proposalType == ProposalType.Dance)
			{
				if (Game1.CurrentEvent == null || !Game1.CurrentEvent.isSpecificFestival("spring24"))
				{
					return false;
				}
				questionKey2 = "Strings\\UI:AskedToDance";
				responseYes = "Strings\\UI:AskedToDance_Accepted";
				responseNo = "Strings\\UI:AskedToDance_Rejected";
				if (Game1.player.dancePartner.Value != null)
				{
					return false;
				}
			}
			else if ((ProposalType)proposal.proposalType == ProposalType.Marriage)
			{
				if (Game1.player.isMarried() || Game1.player.isEngaged())
				{
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = genderedKey("Strings\\UI:AskedToMarry_NotSingle", Game1.player);
					return true;
				}
				questionKey2 = "Strings\\UI:AskedToMarry";
				responseYes = "Strings\\UI:AskedToMarry_Accepted";
				responseNo = "Strings\\UI:AskedToMarry_Rejected";
			}
			else if ((ProposalType)proposal.proposalType == ProposalType.Gift && proposal.gift != null)
			{
				if (!Game1.player.couldInventoryAcceptThisItem(proposal.gift))
				{
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = genderedKey("Strings\\UI:GiftPlayerItem_NoInventorySpace", Game1.player);
					return true;
				}
				questionKey2 = "Strings\\UI:GivenGift";
				additionalVar = proposal.gift.Value.DisplayName;
			}
			else
			{
				if ((ProposalType)proposal.proposalType != ProposalType.Baby)
				{
					return false;
				}
				if (proposal.sender.Value.IsMale != Game1.player.IsMale)
				{
					questionKey2 = "Strings\\UI:AskedToHaveBaby";
					responseYes = "Strings\\UI:AskedToHaveBaby_Accepted";
					responseNo = "Strings\\UI:AskedToHaveBaby_Rejected";
				}
				else
				{
					questionKey2 = "Strings\\UI:AskedToAdoptBaby";
					responseYes = "Strings\\UI:AskedToAdoptBaby_Accepted";
					responseNo = "Strings\\UI:AskedToAdoptBaby_Rejected";
				}
			}
			questionKey2 = genderedKey(questionKey2, proposal.sender);
			if (responseYes != null)
			{
				responseYes = genderedKey(responseYes, Game1.player);
			}
			if (responseNo != null)
			{
				responseNo = genderedKey(responseNo, Game1.player);
			}
			string question = Game1.content.LoadString(questionKey2, proposal.sender.Value.Name, additionalVar);
			Game1.currentLocation.createQuestionDialogue(question, Game1.currentLocation.createYesNoResponses(), delegate(Farmer _, string answer)
			{
				if (proposal.canceled.Value)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:ProposalWithdrawn", proposal.sender.Value.Name));
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = responseNo;
				}
				else if (answer == "Yes")
				{
					proposal.response.Value = ProposalResponse.Accepted;
					proposal.responseMessageKey.Value = responseYes;
					if (proposal.proposalType.Value == ProposalType.Gift || proposal.proposalType.Value == ProposalType.Marriage)
					{
						Item value = proposal.gift.Value;
						proposal.gift.Value = null;
						value = Game1.player.addItemToInventory(value);
						if (value != null)
						{
							Game1.currentLocation.debris.Add(new Debris(value, Game1.player.Position));
						}
					}
					if (proposal.proposalType.Value == ProposalType.Dance)
					{
						Game1.player.dancePartner.Value = proposal.sender.Value;
					}
					if (proposal.proposalType.Value == ProposalType.Marriage)
					{
						Friendship friendship = GetFriendship(proposal.sender.Value.UniqueMultiplayerID, Game1.player.UniqueMultiplayerID);
						friendship.Status = FriendshipStatus.Engaged;
						friendship.Proposer = proposal.sender.Value.UniqueMultiplayerID;
						WorldDate worldDate = new WorldDate(Game1.Date);
						worldDate.TotalDays += 3;
						while (!Game1.canHaveWeddingOnDay(worldDate.DayOfMonth, worldDate.Season))
						{
							worldDate.TotalDays++;
						}
						friendship.WeddingDate = worldDate;
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:PlayerWeddingArranged"));
						Game1.multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, proposal.sender.Value.Name);
					}
					if (proposal.proposalType.Value == ProposalType.Baby)
					{
						Friendship friendship2 = GetFriendship(proposal.sender.Value.UniqueMultiplayerID, Game1.player.UniqueMultiplayerID);
						WorldDate worldDate2 = new WorldDate(Game1.Date);
						worldDate2.TotalDays += 14;
						friendship2.NextBirthingDate = worldDate2;
					}
					Game1.player.doEmote(20);
				}
				else
				{
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = responseNo;
				}
			});
			return true;
		}

		private bool playerIsOnline(long uid)
		{
			if (Game1.serverHost != null && Game1.serverHost.Value.UniqueMultiplayerID == uid)
			{
				return true;
			}
			if (Game1.otherFarmers.ContainsKey(uid))
			{
				return !Game1.multiplayer.isDisconnecting(uid);
			}
			return false;
		}

		public void SetLocalRequiredFarmers(string checkName, IEnumerable<Farmer> required_farmers)
		{
			if (!readyChecks.ContainsKey(checkName))
			{
				readyChecks.Add(checkName, new ReadyCheck(checkName));
			}
			readyChecks[checkName].SetRequiredFarmers(required_farmers);
		}

		public void SetLocalReady(string checkName, bool ready)
		{
			if (!readyChecks.ContainsKey(checkName))
			{
				readyChecks.Add(checkName, new ReadyCheck(checkName));
			}
			readyChecks[checkName].SetLocalReady(ready);
		}

		public bool IsReady(string checkName)
		{
			if (readyChecks.TryGetValue(checkName, out ReadyCheck check))
			{
				return check.IsReady();
			}
			return false;
		}

		public bool IsReadyCheckCancelable(string checkName)
		{
			if (readyChecks.TryGetValue(checkName, out ReadyCheck check))
			{
				return check.IsCancelable();
			}
			return false;
		}

		public bool IsOtherFarmerReady(string checkName, Farmer farmer)
		{
			if (readyChecks.TryGetValue(checkName, out ReadyCheck check))
			{
				return check.IsOtherFarmerReady(farmer);
			}
			return false;
		}

		public int GetNumberReady(string checkName)
		{
			if (!readyChecks.TryGetValue(checkName, out ReadyCheck check))
			{
				return 0;
			}
			return check.GetNumberReady();
		}

		public int GetNumberRequired(string checkName)
		{
			if (!readyChecks.TryGetValue(checkName, out ReadyCheck check))
			{
				return 0;
			}
			return check.GetNumberRequired();
		}

		public void NewDay()
		{
			readyChecks.Clear();
			luauIngredients.Clear();
			grangeDisplay.Clear();
			movieInvitations.Clear();
		}
	}
}
