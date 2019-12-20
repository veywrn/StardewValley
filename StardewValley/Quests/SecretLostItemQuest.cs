using Netcode;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class SecretLostItemQuest : Quest
	{
		[XmlElement("npcName")]
		public readonly NetString npcName = new NetString();

		[XmlElement("friendshipReward")]
		public readonly NetInt friendshipReward = new NetInt();

		[XmlElement("exclusiveQuestId")]
		public readonly NetInt exclusiveQuestId = new NetInt(0);

		[XmlElement("itemIndex")]
		public readonly NetInt itemIndex = new NetInt();

		[XmlElement("itemFound")]
		public readonly NetBool itemFound = new NetBool();

		public SecretLostItemQuest()
		{
		}

		public SecretLostItemQuest(string npcName, int itemIndex, int friendshipReward, int exclusiveQuestId)
		{
			this.npcName.Value = npcName;
			this.itemIndex.Value = itemIndex;
			this.friendshipReward.Value = friendshipReward;
			this.exclusiveQuestId.Value = exclusiveQuestId;
			questType.Value = 9;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(npcName, friendshipReward, exclusiveQuestId, itemFound);
		}

		public override bool isSecretQuest()
		{
			return true;
		}

		public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -2, Item item = null, string str = null)
		{
			if ((bool)completed)
			{
				return false;
			}
			if (item != null && item is Object && (item as Object).parentSheetIndex.Value == itemIndex.Value && !itemFound)
			{
				itemFound.Value = true;
				Game1.playSound("jingle1");
			}
			else if (n != null && n.Name.Equals(npcName.Value) && n.isVillager() && (bool)itemFound && Game1.player.hasItemInInventory(itemIndex, 1))
			{
				questComplete();
				Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				string thankYou = (questData[id].Length > 9) ? questData[id].Split('/')[9] : Game1.content.LoadString("Data\\ExtraDialogue:LostItemQuest_DefaultThankYou");
				n.setNewDialogue(thankYou);
				Game1.drawDialogue(n);
				Game1.player.changeFriendship(friendshipReward.Value, n);
				Game1.player.removeFirstOfThisItemFromInventory(itemIndex);
				return true;
			}
			return false;
		}

		public override void questComplete()
		{
			if (!completed)
			{
				completed.Value = true;
				Game1.player.questLog.Remove(this);
				foreach (Quest q in Game1.player.questLog)
				{
					if (q != null && (int)q.id == exclusiveQuestId.Value)
					{
						q.destroy.Value = true;
					}
				}
				Game1.playSound("questcomplete");
			}
		}
	}
}
