using Netcode;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class ReachMineFloorObjective : OrderObjective
	{
		public NetBool skullCave = new NetBool(value: false);

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(skullCave);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			base.Load(order, data);
			if (data.ContainsKey("SkullCave") && data["SkullCave"].ToLowerInvariant() == "true")
			{
				skullCave.Value = true;
			}
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onMineFloorReached = (Action<Farmer, int>)Delegate.Combine(order.onMineFloorReached, new Action<Farmer, int>(OnNewValue));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onMineFloorReached = (Action<Farmer, int>)Delegate.Remove(order.onMineFloorReached, new Action<Farmer, int>(OnNewValue));
		}

		public virtual void OnNewValue(Farmer who, int new_value)
		{
			if (skullCave.Value)
			{
				new_value -= 120;
			}
			else if (new_value > 120)
			{
				return;
			}
			if (new_value > 0)
			{
				SetCount(Math.Min(Math.Max(new_value, currentCount.Value), GetMaxCount()));
			}
		}
	}
}
