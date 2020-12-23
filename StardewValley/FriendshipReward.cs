using Netcode;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class FriendshipReward : OrderReward
	{
		[XmlElement("targetName")]
		public NetString targetName = new NetString();

		[XmlElement("amount")]
		public NetInt amount = new NetInt();

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(targetName, amount);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string target_name2 = order.requester;
			if (data.ContainsKey("TargetName"))
			{
				target_name2 = data["TargetName"];
			}
			target_name2 = order.Parse(target_name2);
			targetName.Value = target_name2;
			string amount_string2 = "250";
			if (data.ContainsKey("Amount"))
			{
				amount_string2 = data["Amount"];
			}
			amount_string2 = order.Parse(amount_string2);
			amount.Value = int.Parse(amount_string2);
		}

		public override void Grant()
		{
			NPC i = Game1.getCharacterFromName(targetName.Value);
			if (i != null)
			{
				Game1.player.changeFriendship(amount.Value, i);
			}
		}
	}
}
