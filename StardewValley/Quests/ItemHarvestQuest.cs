using Netcode;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class ItemHarvestQuest : Quest
	{
		[XmlElement("itemIndex")]
		public readonly NetInt itemIndex = new NetInt();

		[XmlElement("number")]
		public readonly NetInt number = new NetInt();

		public ItemHarvestQuest()
		{
		}

		public ItemHarvestQuest(int index, int number = 1)
		{
			itemIndex.Value = index;
			this.number.Value = number;
			questType.Value = 9;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(itemIndex, number);
		}

		public override bool checkIfComplete(NPC n = null, int itemIndex = -1, int numberHarvested = 1, Item item = null, string str = null)
		{
			if (!completed && itemIndex != -1 && itemIndex == this.itemIndex.Value)
			{
				number.Value -= numberHarvested;
				if ((int)number <= 0)
				{
					questComplete();
					return true;
				}
			}
			return false;
		}
	}
}
