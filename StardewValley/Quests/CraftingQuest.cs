using Netcode;
using StardewValley.Objects;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class CraftingQuest : Quest
	{
		[XmlElement("isBigCraftable")]
		public readonly NetBool isBigCraftable = new NetBool();

		[XmlElement("indexToCraft")]
		public readonly NetInt indexToCraft = new NetInt();

		public CraftingQuest()
		{
		}

		public CraftingQuest(int indexToCraft, bool bigCraftable)
		{
			this.indexToCraft.Value = indexToCraft;
			isBigCraftable.Value = bigCraftable;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(isBigCraftable, indexToCraft);
		}

		public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -2, Item item = null, string str = null)
		{
			if (item is Clothing)
			{
				return false;
			}
			if (item != null && item is Object && (item as Object).bigCraftable.Value == isBigCraftable.Value && (item as Object).parentSheetIndex.Value == indexToCraft.Value)
			{
				questComplete();
				return true;
			}
			return false;
		}
	}
}
