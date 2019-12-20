using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	[XmlInclude(typeof(SocializeQuest))]
	[XmlInclude(typeof(SlayMonsterQuest))]
	[XmlInclude(typeof(ResourceCollectionQuest))]
	[XmlInclude(typeof(ItemDeliveryQuest))]
	[XmlInclude(typeof(ItemHarvestQuest))]
	[XmlInclude(typeof(CraftingQuest))]
	[XmlInclude(typeof(FishingQuest))]
	[XmlInclude(typeof(GoSomewhereQuest))]
	[XmlInclude(typeof(LostItemQuest))]
	[XmlInclude(typeof(DescriptionElement))]
	[XmlInclude(typeof(SecretLostItemQuest))]
	public class Quest : INetObject<NetFields>
	{
		public const int type_basic = 1;

		public const int type_crafting = 2;

		public const int type_itemDelivery = 3;

		public const int type_monster = 4;

		public const int type_socialize = 5;

		public const int type_location = 6;

		public const int type_fishing = 7;

		public const int type_building = 8;

		public const int type_harvest = 9;

		public const int type_resource = 10;

		public const int type_weeding = 11;

		public string _currentObjective = "";

		public string _questDescription = "";

		public string _questTitle = "";

		[XmlElement("rewardDescription")]
		public readonly NetString rewardDescription = new NetString();

		[XmlElement("completionString")]
		public readonly NetString completionString = new NetString();

		protected Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);

		[XmlElement("accepted")]
		public readonly NetBool accepted = new NetBool();

		[XmlElement("completed")]
		public readonly NetBool completed = new NetBool();

		[XmlElement("dailyQuest")]
		public readonly NetBool dailyQuest = new NetBool();

		[XmlElement("showNew")]
		public readonly NetBool showNew = new NetBool();

		[XmlElement("canBeCancelled")]
		public readonly NetBool canBeCancelled = new NetBool();

		[XmlElement("destroy")]
		public readonly NetBool destroy = new NetBool();

		[XmlElement("id")]
		public readonly NetInt id = new NetInt();

		[XmlElement("moneyReward")]
		public readonly NetInt moneyReward = new NetInt();

		[XmlElement("questType")]
		public readonly NetInt questType = new NetInt();

		[XmlElement("daysLeft")]
		public readonly NetInt daysLeft = new NetInt();

		[XmlElement("dayQuestAccepted")]
		public readonly NetInt dayQuestAccepted = new NetInt(-1);

		public readonly NetIntList nextQuests = new NetIntList();

		private bool _loadedDescription;

		private bool _loadedTitle;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public string questTitle
		{
			get
			{
				if (!_loadedTitle)
				{
					switch (questType.Value)
					{
					case 3:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13285");
						break;
					case 4:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13696");
						break;
					case 5:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SocializeQuest.cs.13785");
						break;
					case 7:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingQuest.cs.13227");
						break;
					case 10:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13640");
						break;
					}
					Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
					if (questData != null && questData.ContainsKey(id))
					{
						string[] rawData = questData[id].Split('/');
						_questTitle = rawData[1];
					}
					_loadedTitle = true;
				}
				if (_questTitle == null)
				{
					_questTitle = "";
				}
				return _questTitle;
			}
			set
			{
				_questTitle = value;
			}
		}

		[XmlIgnore]
		public string questDescription
		{
			get
			{
				if (!_loadedDescription)
				{
					reloadDescription();
					Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
					if (questData != null && questData.ContainsKey(id))
					{
						string[] rawData = questData[id].Split('/');
						_questDescription = rawData[2];
					}
					_loadedDescription = true;
				}
				if (_questDescription == null)
				{
					_questDescription = "";
				}
				return _questDescription;
			}
			set
			{
				_questDescription = value;
			}
		}

		[XmlIgnore]
		public string currentObjective
		{
			get
			{
				Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				if (questData != null && questData.ContainsKey(id))
				{
					string[] rawData = questData[id].Split('/');
					if (rawData[3].Length > 1)
					{
						_currentObjective = rawData[3];
					}
				}
				reloadObjective();
				if (_currentObjective == null)
				{
					_currentObjective = "";
				}
				return _currentObjective;
			}
			set
			{
				_currentObjective = value;
			}
		}

		public Quest()
		{
			initNetFields();
		}

		protected virtual void initNetFields()
		{
			NetFields.AddFields(rewardDescription, completionString, accepted, completed, dailyQuest, showNew, canBeCancelled, destroy, id, moneyReward, questType, daysLeft, nextQuests, dayQuestAccepted);
		}

		public static Quest getQuestFromId(int id)
		{
			Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
			if (questData != null && questData.ContainsKey(id))
			{
				string[] rawData = questData[id].Split('/');
				string questType = rawData[0];
				Quest q = null;
				string[] conditionsSplit = rawData[4].Split(' ');
				switch (questType)
				{
				case "Crafting":
					q = new CraftingQuest(Convert.ToInt32(conditionsSplit[0]), conditionsSplit[1].ToLower().Equals("true"));
					q.questType.Value = 2;
					break;
				case "Location":
					q = new GoSomewhereQuest(conditionsSplit[0]);
					q.questType.Value = 6;
					break;
				case "Building":
					q = new Quest();
					q.questType.Value = 8;
					q.completionString.Value = conditionsSplit[0];
					break;
				case "ItemDelivery":
					q = new ItemDeliveryQuest();
					(q as ItemDeliveryQuest).target.Value = conditionsSplit[0];
					(q as ItemDeliveryQuest).item.Value = Convert.ToInt32(conditionsSplit[1]);
					(q as ItemDeliveryQuest).targetMessage = rawData[9];
					if (conditionsSplit.Length > 2)
					{
						(q as ItemDeliveryQuest).number.Value = Convert.ToInt32(conditionsSplit[2]);
					}
					q.questType.Value = 3;
					break;
				case "Monster":
					q = new SlayMonsterQuest();
					(q as SlayMonsterQuest).loadQuestInfo();
					(q as SlayMonsterQuest).monster.Value.Name = conditionsSplit[0].Replace('_', ' ');
					(q as SlayMonsterQuest).monsterName.Value = (q as SlayMonsterQuest).monster.Value.Name;
					(q as SlayMonsterQuest).numberToKill.Value = Convert.ToInt32(conditionsSplit[1]);
					if (conditionsSplit.Length > 2)
					{
						(q as SlayMonsterQuest).target.Value = conditionsSplit[2];
					}
					else
					{
						(q as SlayMonsterQuest).target.Value = "null";
					}
					q.questType.Value = 4;
					break;
				case "Basic":
					q = new Quest();
					q.questType.Value = 1;
					break;
				case "Social":
					q = new SocializeQuest();
					(q as SocializeQuest).loadQuestInfo();
					break;
				case "ItemHarvest":
					q = new ItemHarvestQuest(Convert.ToInt32(conditionsSplit[0]), (conditionsSplit.Length <= 1) ? 1 : Convert.ToInt32(conditionsSplit[1]));
					break;
				case "LostItem":
					q = new LostItemQuest(conditionsSplit[0], conditionsSplit[2], Convert.ToInt32(conditionsSplit[1]), Convert.ToInt32(conditionsSplit[3]), Convert.ToInt32(conditionsSplit[4]));
					break;
				case "SecretLostItem":
					q = new SecretLostItemQuest(conditionsSplit[0], Convert.ToInt32(conditionsSplit[1]), Convert.ToInt32(conditionsSplit[2]), Convert.ToInt32(conditionsSplit[3]));
					break;
				}
				q.id.Value = id;
				q.questTitle = rawData[1];
				q.questDescription = rawData[2];
				if (rawData[3].Length > 1)
				{
					q.currentObjective = rawData[3];
				}
				string[] nextQuestsSplit = rawData[5].Split(' ');
				for (int i = 0; i < nextQuestsSplit.Length; i++)
				{
					string nextQuest = nextQuestsSplit[i];
					if (nextQuest.StartsWith("h"))
					{
						if (!Game1.IsMasterGame)
						{
							continue;
						}
						nextQuest = nextQuest.Substring(1);
					}
					q.nextQuests.Add(Convert.ToInt32(nextQuest));
				}
				q.showNew.Value = true;
				q.moneyReward.Value = Convert.ToInt32(rawData[6]);
				q.rewardDescription.Value = (rawData[6].Equals("-1") ? null : rawData[7]);
				if (rawData.Length > 8)
				{
					q.canBeCancelled.Value = rawData[8].Equals("true");
				}
				return q;
			}
			return null;
		}

		public virtual void reloadObjective()
		{
		}

		public virtual void reloadDescription()
		{
		}

		public virtual void adjustGameLocation(GameLocation location)
		{
		}

		public virtual void accept()
		{
			accepted.Value = true;
		}

		public virtual bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -2, Item item = null, string str = null)
		{
			if (completionString.Value != null && str != null && str.Equals(completionString.Value))
			{
				questComplete();
				return true;
			}
			return false;
		}

		public bool hasReward()
		{
			if ((int)moneyReward <= 0)
			{
				if (rewardDescription.Value != null)
				{
					return rewardDescription.Value.Length > 2;
				}
				return false;
			}
			return true;
		}

		public virtual bool isSecretQuest()
		{
			return false;
		}

		public virtual void questComplete()
		{
			if (!completed)
			{
				if ((bool)dailyQuest || (int)questType == 7)
				{
					Game1.stats.QuestsCompleted++;
				}
				completed.Value = true;
				if (nextQuests.Count > 0)
				{
					foreach (int i in nextQuests)
					{
						if (i > 0)
						{
							Game1.player.questLog.Add(getQuestFromId(i));
						}
					}
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
				}
				if ((int)moneyReward <= 0 && (rewardDescription.Value == null || rewardDescription.Value.Length <= 2))
				{
					Game1.player.questLog.Remove(this);
				}
				else
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
				}
				Game1.playSound("questcomplete");
				if (id.Value == 126)
				{
					Game1.player.mailReceived.Add("emilyFiber");
					Game1.player.activeDialogueEvents.Add("emilyFiber", 2);
				}
			}
		}
	}
}
