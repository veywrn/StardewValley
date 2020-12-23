using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class ShipObjective : OrderObjective
	{
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		[XmlElement("useShipmentValue")]
		public NetBool useShipmentValue = new NetBool();

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"]));
			}
			if (data.ContainsKey("UseShipmentValue") && data["UseShipmentValue"].ToLowerInvariant().Trim() == "true")
			{
				useShipmentValue.Value = true;
			}
		}

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets, useShipmentValue);
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onItemShipped = (Action<Farmer, Item, int>)Delegate.Combine(order.onItemShipped, new Action<Farmer, Item, int>(OnItemShipped));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onItemShipped = (Action<Farmer, Item, int>)Delegate.Remove(order.onItemShipped, new Action<Farmer, Item, int>(OnItemShipped));
		}

		public virtual void OnItemShipped(Farmer farmer, Item item, int shipped_price)
		{
			foreach (string acceptableContextTagSet in acceptableContextTagSets)
			{
				bool fail = false;
				string[] array = acceptableContextTagSet.Split(',');
				foreach (string obj in array)
				{
					bool found_match = false;
					string[] array2 = obj.Split('/');
					foreach (string acceptable_tag in array2)
					{
						if (item.HasContextTag(acceptable_tag.Trim()))
						{
							found_match = true;
							break;
						}
					}
					if (!found_match)
					{
						fail = true;
					}
				}
				if (!fail)
				{
					if (useShipmentValue.Value)
					{
						IncrementCount(shipped_price);
					}
					else
					{
						IncrementCount(item.Stack);
					}
					break;
				}
			}
		}
	}
}
