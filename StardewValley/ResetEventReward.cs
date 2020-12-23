using Netcode;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class ResetEventReward : OrderReward
	{
		public NetIntList resetEvents = new NetIntList();

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(resetEvents);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string[] array = order.Parse(data["ResetEvents"]).Split(' ');
			foreach (string s in array)
			{
				resetEvents.Add(Convert.ToInt32(s));
			}
		}

		public override void Grant()
		{
			foreach (int event_index in resetEvents)
			{
				Game1.player.eventsSeen.Remove(event_index);
			}
		}
	}
}
