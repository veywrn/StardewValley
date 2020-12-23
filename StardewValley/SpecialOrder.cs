using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	[XmlInclude(typeof(SpecialOrder))]
	[XmlInclude(typeof(OrderObjective))]
	[XmlInclude(typeof(ShipObjective))]
	[XmlInclude(typeof(SlayObjective))]
	[XmlInclude(typeof(DeliverObjective))]
	[XmlInclude(typeof(FishObjective))]
	[XmlInclude(typeof(GiftObjective))]
	[XmlInclude(typeof(JKScoreObjective))]
	[XmlInclude(typeof(ReachMineFloorObjective))]
	[XmlInclude(typeof(CollectObjective))]
	[XmlInclude(typeof(DonateObjective))]
	[XmlInclude(typeof(MailReward))]
	[XmlInclude(typeof(MoneyReward))]
	[XmlInclude(typeof(GemsReward))]
	[XmlInclude(typeof(ResetEventReward))]
	[XmlInclude(typeof(OrderReward))]
	[XmlInclude(typeof(FriendshipReward))]
	public class SpecialOrder : INetObject<NetFields>, IQuest
	{
		public enum QuestState
		{
			InProgress,
			Complete,
			Failed
		}

		public enum QuestDuration
		{
			Week,
			Month,
			TwoWeeks,
			TwoDays,
			ThreeDays
		}

		[XmlIgnore]
		public Action<Farmer, Item, int> onItemShipped;

		[XmlIgnore]
		public Action<Farmer, Monster> onMonsterSlain;

		[XmlIgnore]
		public Action<Farmer, Item> onFishCaught;

		[XmlIgnore]
		public Action<Farmer, NPC, Item> onGiftGiven;

		[XmlIgnore]
		public Func<Farmer, NPC, Item, int> onItemDelivered;

		[XmlIgnore]
		public Action<Farmer, Item> onItemCollected;

		[XmlIgnore]
		public Action<Farmer, int> onMineFloorReached;

		[XmlIgnore]
		public Action<Farmer, int> onJKScoreAchieved;

		[XmlIgnore]
		protected bool _objectiveRegistrationDirty;

		[XmlElement("preSelectedItems")]
		public NetStringDictionary<int, NetInt> preSelectedItems = new NetStringDictionary<int, NetInt>();

		[XmlElement("selectedRandomElements")]
		public NetStringDictionary<int, NetInt> selectedRandomElements = new NetStringDictionary<int, NetInt>();

		[XmlElement("objectives")]
		public NetList<OrderObjective, NetRef<OrderObjective>> objectives = new NetList<OrderObjective, NetRef<OrderObjective>>();

		[XmlElement("generationSeed")]
		public NetInt generationSeed = new NetInt();

		[XmlElement("seenParticipantsIDs")]
		public NetLongDictionary<bool, NetBool> seenParticipants = new NetLongDictionary<bool, NetBool>();

		[XmlElement("participantsIDs")]
		public NetLongDictionary<bool, NetBool> participants = new NetLongDictionary<bool, NetBool>();

		[XmlElement("unclaimedRewardsIDs")]
		public NetLongDictionary<bool, NetBool> unclaimedRewards = new NetLongDictionary<bool, NetBool>();

		[XmlElement("donatedItems")]
		public readonly NetCollection<Item> donatedItems = new NetCollection<Item>();

		[XmlElement("appliedSpecialRules")]
		public bool appliedSpecialRules;

		[XmlIgnore]
		public readonly NetMutex donateMutex = new NetMutex();

		[XmlIgnore]
		protected int _isIslandOrder = -1;

		[XmlElement("rewards")]
		public NetList<OrderReward, NetRef<OrderReward>> rewards = new NetList<OrderReward, NetRef<OrderReward>>();

		[XmlIgnore]
		protected int _moneyReward = -1;

		[XmlElement("questKey")]
		public NetString questKey = new NetString();

		[XmlElement("questName")]
		public NetString questName = new NetString("Strings\\SpecialOrders:PlaceholderName");

		[XmlElement("questDescription")]
		public NetString questDescription = new NetString("Strings\\SpecialOrders:PlaceholderDescription");

		[XmlElement("requester")]
		public NetString requester = new NetString();

		[XmlElement("orderType")]
		public NetString orderType = new NetString("");

		[XmlElement("specialRule")]
		public NetString specialRule = new NetString("");

		[XmlElement("readyForRemoval")]
		public NetBool readyForRemoval = new NetBool(value: false);

		[XmlElement("itemToRemoveOnEnd")]
		public NetInt itemToRemoveOnEnd = new NetInt(-1);

		[XmlElement("mailToRemoveOnEnd")]
		public NetString mailToRemoveOnEnd = new NetString(null);

		[XmlIgnore]
		protected string _localizedName;

		[XmlIgnore]
		protected string _localizedDescription;

		[XmlElement("dueDate")]
		public NetInt dueDate = new NetInt();

		[XmlElement("duration")]
		public NetEnum<QuestDuration> questDuration = new NetEnum<QuestDuration>();

		[XmlIgnore]
		protected List<OrderObjective> _registeredObjectives = new List<OrderObjective>();

		[XmlIgnore]
		protected Dictionary<Item, bool> _highlightLookup;

		[XmlIgnore]
		protected SpecialOrderData _orderData;

		[XmlElement("questState")]
		public NetEnum<QuestState> questState = new NetEnum<QuestState>(QuestState.InProgress);

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public SpecialOrder()
		{
			InitializeNetFields();
		}

		public virtual void SetDuration(QuestDuration duration)
		{
			questDuration.Value = duration;
			WorldDate date = new WorldDate();
			switch (duration)
			{
			case QuestDuration.Week:
				date = new WorldDate(Game1.year, Game1.currentSeason, (Game1.dayOfMonth - 1) / 7 * 7);
				date.TotalDays++;
				date.TotalDays += 7;
				break;
			case QuestDuration.TwoWeeks:
				date = new WorldDate(Game1.year, Game1.currentSeason, (Game1.dayOfMonth - 1) / 7 * 7);
				date.TotalDays++;
				date.TotalDays += 14;
				break;
			case QuestDuration.Month:
				date = new WorldDate(Game1.year, Game1.currentSeason, 0);
				date.TotalDays++;
				date.TotalDays += 28;
				break;
			case QuestDuration.TwoDays:
				date = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
				date.TotalDays += 2;
				break;
			case QuestDuration.ThreeDays:
				date = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
				date.TotalDays += 3;
				break;
			}
			dueDate.Value = date.TotalDays;
		}

		public virtual void OnFail()
		{
			foreach (OrderObjective objective in objectives)
			{
				objective.OnFail();
			}
			for (int i = 0; i < donatedItems.Count; i++)
			{
				Item item = donatedItems[i];
				donatedItems[i] = null;
				if (item != null)
				{
					Game1.player.team.returnedDonations.Add(item);
					Game1.player.team.newLostAndFoundItems.Value = true;
				}
			}
			if (Game1.IsMasterGame)
			{
				HostHandleQuestEnd();
			}
			questState.Value = QuestState.Failed;
			_RemoveSpecialRuleIfNecessary();
		}

		public virtual int GetCompleteObjectivesCount()
		{
			int count = 0;
			foreach (OrderObjective objective in objectives)
			{
				if (objective.IsComplete())
				{
					count++;
				}
			}
			return count;
		}

		public virtual void ConfirmCompleteDonations()
		{
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective)
				{
					(objective as DonateObjective).Confirm();
				}
			}
		}

		public virtual void UpdateDonationCounts()
		{
			_highlightLookup = null;
			int old_completed_objectives_count = 0;
			int new_completed_objectives_count = 0;
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective)
				{
					DonateObjective donate_objective = objective as DonateObjective;
					int count = 0;
					if (donate_objective.GetCount() >= donate_objective.GetMaxCount())
					{
						old_completed_objectives_count++;
					}
					foreach (Item item in donatedItems)
					{
						if (donate_objective.IsValidItem(item))
						{
							count += item.Stack;
						}
					}
					donate_objective.SetCount(count);
					if (donate_objective.GetCount() >= donate_objective.GetMaxCount())
					{
						new_completed_objectives_count++;
					}
				}
			}
			if (new_completed_objectives_count > old_completed_objectives_count)
			{
				Game1.playSound("newArtifact");
			}
		}

		public bool HighlightAcceptableItems(Item item)
		{
			if (_highlightLookup != null && _highlightLookup.ContainsKey(item))
			{
				return _highlightLookup[item];
			}
			if (_highlightLookup == null)
			{
				_highlightLookup = new Dictionary<Item, bool>();
			}
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective && (objective as DonateObjective).GetAcceptCount(item, 1) > 0)
				{
					_highlightLookup[item] = true;
					return true;
				}
			}
			_highlightLookup[item] = false;
			return false;
		}

		public virtual int GetAcceptCount(Item item)
		{
			int total_accepted_count = 0;
			int total_stacks = item.Stack;
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective)
				{
					int accepted_count = (objective as DonateObjective).GetAcceptCount(item, total_stacks);
					total_stacks -= accepted_count;
					total_accepted_count += accepted_count;
				}
			}
			return total_accepted_count;
		}

		public static bool CheckTags(string tag_list)
		{
			if (tag_list == null)
			{
				return true;
			}
			List<string> tags = new List<string>();
			string[] array = tag_list.Split(',');
			foreach (string tag in array)
			{
				tags.Add(tag.Trim());
			}
			foreach (string item in tags)
			{
				string current_tag = item;
				if (current_tag.Length != 0)
				{
					bool match = true;
					if (current_tag[0] == '!')
					{
						match = false;
						current_tag = current_tag.Substring(1);
					}
					if (CheckTag(current_tag) != match)
					{
						return false;
					}
				}
			}
			return true;
		}

		protected static bool CheckTag(string tag)
		{
			if (tag == "NOT_IMPLEMENTED")
			{
				return false;
			}
			if (tag.StartsWith("dropbox_"))
			{
				string value7 = tag.Substring("dropbox_".Length);
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					if (specialOrder.UsesDropBox(value7))
					{
						return true;
					}
				}
			}
			if (tag.StartsWith("rule_"))
			{
				string value6 = tag.Substring("rule_".Length);
				if (Game1.player.team.SpecialOrderRuleActive(value6))
				{
					return true;
				}
			}
			if (tag.StartsWith("completed_"))
			{
				string value5 = tag.Substring("season_".Length);
				if (Game1.player.team.completedSpecialOrders.ContainsKey(value5))
				{
					return true;
				}
			}
			if (tag.StartsWith("season_"))
			{
				string value4 = tag.Substring("season_".Length);
				if (Game1.currentSeason == value4)
				{
					return true;
				}
			}
			else if (tag.StartsWith("mail_"))
			{
				string value3 = tag.Substring("mail_".Length);
				if (Game1.MasterPlayer.hasOrWillReceiveMail(value3))
				{
					return true;
				}
			}
			else if (tag.StartsWith("event_"))
			{
				int value2 = Convert.ToInt32(tag.Substring("event_".Length));
				if (Game1.MasterPlayer.eventsSeen.Contains(value2))
				{
					return true;
				}
			}
			else
			{
				if (tag == "island")
				{
					if (Utility.doesAnyFarmerHaveOrWillReceiveMail("seenBoatJourney"))
					{
						return true;
					}
					return false;
				}
				if (tag.StartsWith("knows_"))
				{
					string value = tag.Substring("knows_".Length);
					foreach (Farmer allFarmer in Game1.getAllFarmers())
					{
						if (allFarmer.friendshipData.ContainsKey(value))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool IsIslandOrder()
		{
			if (_isIslandOrder == -1)
			{
				Dictionary<string, SpecialOrderData> order_data = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
				if (order_data.ContainsKey(questKey.Value))
				{
					if (order_data[questKey.Value].RequiredTags.Contains("island"))
					{
						_isIslandOrder = 1;
					}
					else
					{
						_isIslandOrder = 0;
					}
				}
			}
			return _isIslandOrder == 1;
		}

		public static bool IsSpecialOrdersBoardUnlocked()
		{
			return Game1.stats.DaysPlayed >= 58;
		}

		public static void UpdateAvailableSpecialOrders(bool force_refresh)
		{
			if (Game1.player.team.availableSpecialOrders != null)
			{
				foreach (SpecialOrder order in Game1.player.team.availableSpecialOrders)
				{
					if ((order.questDuration.Value == QuestDuration.TwoDays || order.questDuration.Value == QuestDuration.ThreeDays) && !Game1.player.team.acceptedSpecialOrderTypes.Contains(order.orderType.Value))
					{
						order.SetDuration(order.questDuration);
					}
				}
			}
			if (Game1.player.team.availableSpecialOrders.Count > 0 && !force_refresh)
			{
				return;
			}
			Game1.player.team.availableSpecialOrders.Clear();
			Game1.player.team.acceptedSpecialOrderTypes.Clear();
			Dictionary<string, SpecialOrderData> order_data = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
			List<string> keys = new List<string>(order_data.Keys);
			for (int k = 0; k < keys.Count; k++)
			{
				string key = keys[k];
				bool invalid = false;
				if (!invalid && order_data[key].Repeatable != "True" && Game1.MasterPlayer.team.completedSpecialOrders.ContainsKey(key))
				{
					invalid = true;
				}
				if (Game1.dayOfMonth >= 16 && order_data[key].Duration == "Month")
				{
					invalid = true;
				}
				if (!invalid && !CheckTags(order_data[key].RequiredTags))
				{
					invalid = true;
				}
				if (!invalid)
				{
					foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
					{
						if ((string)specialOrder.questKey == key)
						{
							invalid = true;
							break;
						}
					}
				}
				if (invalid)
				{
					keys.RemoveAt(k);
					k--;
				}
			}
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)((float)(double)Game1.stats.DaysPlayed * 1.3f));
			Game1.player.team.availableSpecialOrders.Clear();
			string[] array = new string[2]
			{
				"",
				"Qi"
			};
			foreach (string type_to_find in array)
			{
				List<string> typed_keys = new List<string>();
				foreach (string key3 in keys)
				{
					if (order_data[key3].OrderType == type_to_find)
					{
						typed_keys.Add(key3);
					}
				}
				List<string> all_keys = new List<string>(typed_keys);
				if (type_to_find != "Qi")
				{
					for (int j = 0; j < typed_keys.Count; j++)
					{
						if (Game1.player.team.completedSpecialOrders.ContainsKey(typed_keys[j]))
						{
							typed_keys.RemoveAt(j);
							j--;
						}
					}
				}
				for (int i = 0; i < 2; i++)
				{
					if (typed_keys.Count == 0)
					{
						if (all_keys.Count == 0)
						{
							break;
						}
						typed_keys = new List<string>(all_keys);
					}
					int index = r.Next(typed_keys.Count);
					string key2 = typed_keys[index];
					Game1.player.team.availableSpecialOrders.Add(GetSpecialOrder(key2, r.Next()));
					typed_keys.Remove(key2);
					all_keys.Remove(key2);
				}
			}
		}

		public static SpecialOrder GetSpecialOrder(string key, int? generation_seed)
		{
			Dictionary<string, SpecialOrderData> order_data = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
			if (!generation_seed.HasValue)
			{
				generation_seed = Game1.random.Next();
			}
			if (order_data.ContainsKey(key))
			{
				Random r = new Random(generation_seed.Value);
				SpecialOrderData data = order_data[key];
				SpecialOrder order = new SpecialOrder();
				order.generationSeed.Value = generation_seed.Value;
				order._orderData = data;
				order.questKey.Value = key;
				order.questName.Value = data.Name;
				order.requester.Value = data.Requester;
				order.orderType.Value = data.OrderType.Trim();
				order.specialRule.Value = data.SpecialRule.Trim();
				if (data.ItemToRemoveOnEnd != null)
				{
					int item_to_remove = -1;
					if (int.TryParse(data.ItemToRemoveOnEnd, out item_to_remove))
					{
						order.itemToRemoveOnEnd.Value = item_to_remove;
					}
				}
				if (data.MailToRemoveOnEnd != null)
				{
					order.mailToRemoveOnEnd.Value = data.MailToRemoveOnEnd;
				}
				order.selectedRandomElements.Clear();
				if (data.RandomizedElements != null)
				{
					foreach (RandomizedElement randomized_element in data.RandomizedElements)
					{
						List<int> valid_indices = new List<int>();
						for (int i = 0; i < randomized_element.Values.Count; i++)
						{
							if (CheckTags(randomized_element.Values[i].RequiredTags))
							{
								valid_indices.Add(i);
							}
						}
						int selected_index = Utility.GetRandom(valid_indices, r);
						order.selectedRandomElements[randomized_element.Name] = selected_index;
						string value2 = randomized_element.Values[selected_index].Value;
						if (value2.StartsWith("PICK_ITEM"))
						{
							value2 = value2.Substring("PICK_ITEM".Length);
							string[] array = value2.Split(',');
							List<int> valid_item_ids = new List<int>();
							string[] array2 = array;
							for (int j = 0; j < array2.Length; j++)
							{
								string valid_item_name = array2[j].Trim();
								if (valid_item_name.Length != 0)
								{
									if (char.IsDigit(valid_item_name[0]))
									{
										int item_id = -1;
										if (int.TryParse(valid_item_name, out item_id))
										{
											valid_item_ids.Add(item_id);
										}
									}
									else
									{
										Item item = Utility.fuzzyItemSearch(valid_item_name);
										if (Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex))
										{
											valid_item_ids.Add(item.ParentSheetIndex);
										}
									}
								}
							}
							order.preSelectedItems[randomized_element.Name] = Utility.GetRandom(valid_item_ids, r);
						}
					}
				}
				if (data.Duration == "Month")
				{
					order.SetDuration(QuestDuration.Month);
				}
				else if (data.Duration == "TwoWeeks")
				{
					order.SetDuration(QuestDuration.TwoWeeks);
				}
				else if (data.Duration == "TwoDays")
				{
					order.SetDuration(QuestDuration.TwoDays);
				}
				else if (data.Duration == "ThreeDays")
				{
					order.SetDuration(QuestDuration.ThreeDays);
				}
				else
				{
					order.SetDuration(QuestDuration.Week);
				}
				order.questDescription.Value = data.Text;
				foreach (SpecialOrderObjectiveData objective_data in data.Objectives)
				{
					OrderObjective objective2 = null;
					Type objective_type = Type.GetType("StardewValley." + objective_data.Type.Trim() + "Objective");
					if (!(objective_type == null) && objective_type.IsSubclassOf(typeof(OrderObjective)))
					{
						objective2 = (OrderObjective)Activator.CreateInstance(objective_type);
						if (objective2 != null)
						{
							objective2.description.Value = order.Parse(objective_data.Text);
							objective2.maxCount.Value = int.Parse(order.Parse(objective_data.RequiredCount));
							objective2.Load(order, objective_data.Data);
							order.objectives.Add(objective2);
						}
					}
				}
				{
					foreach (SpecialOrderRewardData reward_data in data.Rewards)
					{
						OrderReward reward2 = null;
						Type reward_type = Type.GetType("StardewValley." + reward_data.Type.Trim() + "Reward");
						if (!(reward_type == null) && reward_type.IsSubclassOf(typeof(OrderReward)))
						{
							reward2 = (OrderReward)Activator.CreateInstance(reward_type);
							if (reward2 != null)
							{
								reward2.Load(order, reward_data.Data);
								order.rewards.Add(reward2);
							}
						}
					}
					return order;
				}
			}
			return null;
		}

		public virtual string MakeLocalizationReplacements(string data)
		{
			data = data.Trim();
			int open_index2 = 0;
			do
			{
				open_index2 = data.LastIndexOf('[');
				if (open_index2 >= 0)
				{
					int close_index = data.IndexOf(']', open_index2);
					if (close_index == -1)
					{
						return data;
					}
					string inner = data.Substring(open_index2 + 1, close_index - open_index2 - 1);
					string value = Game1.content.LoadString("Strings\\SpecialOrderStrings:" + inner);
					data = data.Remove(open_index2, close_index - open_index2 + 1);
					data = data.Insert(open_index2, value);
				}
			}
			while (open_index2 >= 0);
			return data;
		}

		public virtual string Parse(string data)
		{
			data = data.Trim();
			GetData();
			data = MakeLocalizationReplacements(data);
			int open_index = 0;
			do
			{
				open_index = data.LastIndexOf('{');
				if (open_index < 0)
				{
					continue;
				}
				int close_index = data.IndexOf('}', open_index);
				if (close_index == -1)
				{
					return data;
				}
				string inner = data.Substring(open_index + 1, close_index - open_index - 1);
				string value = inner;
				string key = inner;
				string subkey = null;
				if (inner.Contains(":"))
				{
					string[] split2 = inner.Split(':');
					key = split2[0];
					if (split2.Length > 1)
					{
						subkey = split2[1];
					}
				}
				if (_orderData.RandomizedElements != null)
				{
					if (preSelectedItems.ContainsKey(key))
					{
						Object requested_item = new Object(Vector2.Zero, preSelectedItems[key], 0);
						if (subkey == "Text")
						{
							value = requested_item.DisplayName;
						}
						else if (subkey == "TextPlural")
						{
							value = Lexicon.makePlural(requested_item.DisplayName);
						}
						else if (subkey == "TextPluralCapitalized")
						{
							value = Utility.capitalizeFirstLetter(Lexicon.makePlural(requested_item.DisplayName));
						}
						else if (subkey == "Tags")
						{
							string alternate_id2 = "id_" + Utility.getStandardDescriptionFromItem(requested_item, 0, '_');
							alternate_id2 = alternate_id2.Substring(0, alternate_id2.Length - 2).ToLower();
							value = alternate_id2;
						}
						else if (subkey == "Price")
						{
							value = string.Concat(requested_item.sellToStorePrice(-1L));
						}
					}
					else if (selectedRandomElements.ContainsKey(key))
					{
						foreach (RandomizedElement randomized_element in _orderData.RandomizedElements)
						{
							if (randomized_element.Name == key)
							{
								value = MakeLocalizationReplacements(randomized_element.Values[selectedRandomElements[key]].Value);
								break;
							}
						}
					}
				}
				if (subkey != null)
				{
					string[] split = value.Split('|');
					for (int i = 0; i < split.Length; i += 2)
					{
						if (i + 1 <= split.Length && split[i] == subkey)
						{
							value = split[i + 1];
							break;
						}
					}
				}
				data = data.Remove(open_index, close_index - open_index + 1);
				data = data.Insert(open_index, value);
			}
			while (open_index >= 0);
			return data;
		}

		public virtual SpecialOrderData GetData()
		{
			if (_orderData == null)
			{
				Dictionary<string, SpecialOrderData> order_data = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
				if (order_data.ContainsKey(questKey.Value))
				{
					_orderData = order_data[questKey.Value];
				}
			}
			return _orderData;
		}

		public virtual void InitializeNetFields()
		{
			NetFields.AddFields(questName, questDescription, dueDate, objectives, rewards, questState, donatedItems, questKey, requester, generationSeed, selectedRandomElements, preSelectedItems, orderType, specialRule, participants, seenParticipants, unclaimedRewards, donateMutex.NetFields, itemToRemoveOnEnd, mailToRemoveOnEnd, questDuration, readyForRemoval);
			objectives.OnArrayReplaced += delegate
			{
				_objectiveRegistrationDirty = true;
			};
			objectives.OnElementChanged += delegate
			{
				_objectiveRegistrationDirty = true;
			};
		}

		protected virtual void _UpdateObjectiveRegistration()
		{
			for (int i = 0; i < _registeredObjectives.Count; i++)
			{
				OrderObjective objective = _registeredObjectives[i];
				if (!objectives.Contains(objective))
				{
					objective.Unregister();
				}
			}
			foreach (OrderObjective objective2 in objectives)
			{
				if (!_registeredObjectives.Contains(objective2))
				{
					objective2.Register(this);
					_registeredObjectives.Add(objective2);
				}
			}
		}

		public bool UsesDropBox(string box_id)
		{
			if (questState.Value != 0)
			{
				return false;
			}
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective && (objective as DonateObjective).dropBox.Value == box_id)
				{
					return true;
				}
			}
			return false;
		}

		public int GetMinimumDropBoxCapacity(string box_id)
		{
			int minimum_capacity = 9;
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective && (objective as DonateObjective).dropBox.Value == box_id && (objective as DonateObjective).minimumCapacity.Value > 0)
				{
					minimum_capacity = Math.Max(minimum_capacity, (objective as DonateObjective).minimumCapacity);
				}
			}
			return minimum_capacity;
		}

		public virtual void Update()
		{
			_AddSpecialRulesIfNecessary();
			if (_objectiveRegistrationDirty)
			{
				_objectiveRegistrationDirty = false;
				_UpdateObjectiveRegistration();
			}
			if (!readyForRemoval.Value)
			{
				if (questState.Value == QuestState.InProgress && !participants.ContainsKey(Game1.player.UniqueMultiplayerID))
				{
					participants[Game1.player.UniqueMultiplayerID] = true;
				}
				else if (questState.Value == QuestState.Complete)
				{
					if (unclaimedRewards.ContainsKey(Game1.player.UniqueMultiplayerID))
					{
						unclaimedRewards.Remove(Game1.player.UniqueMultiplayerID);
						Game1.stats.QuestsCompleted++;
						Game1.playSound("questcomplete");
						Game1.dayTimeMoneyBox.questsDirty = true;
						foreach (OrderReward reward in rewards)
						{
							reward.Grant();
						}
					}
					if (participants.ContainsKey(Game1.player.UniqueMultiplayerID) && GetMoneyReward() <= 0)
					{
						RemoveFromParticipants();
					}
				}
			}
			donateMutex.Update(Game1.getOnlineFarmers());
			if (donateMutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				donateMutex.ReleaseLock();
			}
			if (Game1.activeClickableMenu == null)
			{
				_highlightLookup = null;
			}
			if (Game1.IsMasterGame && questState.Value != 0)
			{
				MarkForRemovalIfEmpty();
				if (readyForRemoval.Value)
				{
					_RemoveSpecialRuleIfNecessary();
					Game1.player.team.specialOrders.Remove(this);
				}
			}
		}

		public virtual void RemoveFromParticipants()
		{
			participants.Remove(Game1.player.UniqueMultiplayerID);
			MarkForRemovalIfEmpty();
		}

		public virtual void MarkForRemovalIfEmpty()
		{
			if (participants.Count() == 0)
			{
				readyForRemoval.Value = true;
			}
		}

		public virtual void HostHandleQuestEnd()
		{
			if (Game1.IsMasterGame)
			{
				if (itemToRemoveOnEnd.Value >= 0 && !Game1.player.team.itemsToRemoveOvernight.Contains(itemToRemoveOnEnd.Value))
				{
					Game1.player.team.itemsToRemoveOvernight.Add(itemToRemoveOnEnd.Value);
				}
				if (mailToRemoveOnEnd.Value != null && !Game1.player.team.mailToRemoveOvernight.Contains(mailToRemoveOnEnd.Value))
				{
					Game1.player.team.mailToRemoveOvernight.Add(mailToRemoveOnEnd.Value);
				}
			}
		}

		protected void _AddSpecialRulesIfNecessary()
		{
			if (!Game1.IsMasterGame || appliedSpecialRules || questState.Value != 0)
			{
				return;
			}
			appliedSpecialRules = true;
			string[] array = specialRule.Value.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string formatted_rule = array[i].Trim();
				if (!Game1.player.team.SpecialOrderRuleActive(formatted_rule, this))
				{
					AddSpecialRule(formatted_rule);
					if (Game1.player.team.specialRulesRemovedToday.Contains(formatted_rule))
					{
						Game1.player.team.specialRulesRemovedToday.Remove(formatted_rule);
					}
				}
			}
		}

		protected void _RemoveSpecialRuleIfNecessary()
		{
			if (!Game1.IsMasterGame || !appliedSpecialRules)
			{
				return;
			}
			appliedSpecialRules = false;
			string[] array = specialRule.Value.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string formatted_rule = array[i].Trim();
				if (!Game1.player.team.SpecialOrderRuleActive(formatted_rule, this))
				{
					RemoveSpecialRule(formatted_rule);
					if (!Game1.player.team.specialRulesRemovedToday.Contains(formatted_rule))
					{
						Game1.player.team.specialRulesRemovedToday.Add(formatted_rule);
					}
				}
			}
		}

		public virtual void AddSpecialRule(string rule)
		{
			if (rule == "MINE_HARD")
			{
				Game1.netWorldState.Value.MinesDifficulty++;
				Game1.player.team.kickOutOfMinesEvent.Fire();
				Game1.netWorldState.Value.LowestMineLevelForOrder = 0;
			}
			else if (rule == "SC_HARD")
			{
				Game1.netWorldState.Value.SkullCavesDifficulty++;
				Game1.player.team.kickOutOfMinesEvent.Fire();
			}
		}

		public static void RemoveSpecialRuleAtEndOfDay(string rule)
		{
			if (rule == "MINE_HARD")
			{
				if (Game1.netWorldState.Value.MinesDifficulty > 0)
				{
					Game1.netWorldState.Value.MinesDifficulty--;
				}
				Game1.netWorldState.Value.LowestMineLevelForOrder = -1;
			}
			else if (rule == "SC_HARD")
			{
				if (Game1.netWorldState.Value.SkullCavesDifficulty > 0)
				{
					Game1.netWorldState.Value.SkullCavesDifficulty--;
				}
			}
			else if (rule == "QI_COOKING")
			{
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is Object && (item as Object).orderData.Value == "QI_COOKING")
					{
						(item as Object).orderData.Value = null;
						item.MarkContextTagsDirty();
					}
				});
			}
		}

		public virtual void RemoveSpecialRule(string rule)
		{
			if (rule == "QI_BEANS")
			{
				Game1.player.team.itemsToRemoveOvernight.Add(890);
				Game1.player.team.itemsToRemoveOvernight.Add(889);
			}
		}

		public virtual bool HasMoneyReward()
		{
			if (questState.Value == QuestState.Complete && GetMoneyReward() > 0)
			{
				return participants.ContainsKey(Game1.player.UniqueMultiplayerID);
			}
			return false;
		}

		public virtual void Fail()
		{
		}

		public virtual void AddObjective(OrderObjective objective)
		{
			objectives.Add(objective);
		}

		public void CheckCompletion()
		{
			if (questState.Value == QuestState.InProgress)
			{
				foreach (OrderObjective objective2 in objectives)
				{
					if ((bool)objective2.failOnCompletion && objective2.IsComplete())
					{
						OnFail();
						return;
					}
				}
				foreach (OrderObjective objective in objectives)
				{
					if (!objective.failOnCompletion && !objective.IsComplete())
					{
						return;
					}
				}
				if (Game1.IsMasterGame)
				{
					foreach (long farmer_id in participants.Keys)
					{
						if (!unclaimedRewards.ContainsKey(farmer_id))
						{
							unclaimedRewards[farmer_id] = true;
						}
					}
					Game1.multiplayer.globalChatInfoMessage("CompletedSpecialOrder", GetName());
					HostHandleQuestEnd();
					Game1.player.team.completedSpecialOrders[questKey.Value] = true;
					questState.Value = QuestState.Complete;
					_RemoveSpecialRuleIfNecessary();
				}
			}
		}

		public override string ToString()
		{
			string temp = "";
			foreach (OrderObjective objective in objectives)
			{
				temp += objective.description;
				if (objective.GetMaxCount() > 1)
				{
					temp = temp + " (" + objective.GetCount() + "/" + objective.GetMaxCount() + ")";
				}
				temp += "\n";
			}
			return temp.Trim();
		}

		public string GetName()
		{
			if (_localizedName == null)
			{
				_localizedName = MakeLocalizationReplacements(questName.Value);
			}
			return _localizedName;
		}

		public string GetDescription()
		{
			if (_localizedDescription == null)
			{
				_localizedDescription = Parse(questDescription.Value).Trim();
			}
			return _localizedDescription;
		}

		public List<string> GetObjectiveDescriptions()
		{
			List<string> objective_descriptions = new List<string>();
			foreach (OrderObjective objective in objectives)
			{
				objective_descriptions.Add(Parse(objective.GetDescription()));
			}
			return objective_descriptions;
		}

		public bool CanBeCancelled()
		{
			return false;
		}

		public void MarkAsViewed()
		{
			if (!seenParticipants.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				seenParticipants[Game1.player.UniqueMultiplayerID] = true;
			}
		}

		public bool IsHidden()
		{
			return !participants.ContainsKey(Game1.player.UniqueMultiplayerID);
		}

		public bool ShouldDisplayAsNew()
		{
			return !seenParticipants.ContainsKey(Game1.player.UniqueMultiplayerID);
		}

		public bool HasReward()
		{
			return HasMoneyReward();
		}

		public int GetMoneyReward()
		{
			if (_moneyReward == -1)
			{
				_moneyReward = 0;
				foreach (OrderReward reward in rewards)
				{
					if (reward is MoneyReward)
					{
						_moneyReward += (reward as MoneyReward).GetRewardMoneyAmount();
					}
				}
			}
			return _moneyReward;
		}

		public bool ShouldDisplayAsComplete()
		{
			return questState.Value != QuestState.InProgress;
		}

		public bool IsTimedQuest()
		{
			return true;
		}

		public int GetDaysLeft()
		{
			if (questState.Value != 0)
			{
				return 0;
			}
			return (int)dueDate - Game1.Date.TotalDays;
		}

		public void OnMoneyRewardClaimed()
		{
			participants.Remove(Game1.player.UniqueMultiplayerID);
			MarkForRemovalIfEmpty();
		}

		public bool OnLeaveQuestPage()
		{
			if (!participants.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				MarkForRemovalIfEmpty();
				return true;
			}
			return false;
		}
	}
}
