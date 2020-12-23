using Netcode;
using System.Collections.Generic;

namespace StardewValley
{
	public class MoneyReward : OrderReward
	{
		public NetInt amount = new NetInt(0);

		public NetFloat multiplier = new NetFloat(1f);

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(amount, multiplier);
		}

		public virtual int GetRewardMoneyAmount()
		{
			return (int)((float)amount.Value * multiplier.Value);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			amount.Value = int.Parse(order.Parse(data["Amount"]));
			if (data.ContainsKey("Multiplier"))
			{
				multiplier.Value = float.Parse(order.Parse(data["Multiplier"]));
			}
		}

		public override void Grant()
		{
			base.Grant();
		}
	}
}
