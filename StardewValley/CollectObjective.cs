using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class CollectObjective : OrderObjective
	{
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"]));
			}
		}

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets);
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onItemCollected = (Action<Farmer, Item>)Delegate.Combine(order.onItemCollected, new Action<Farmer, Item>(OnItemShipped));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onItemCollected = (Action<Farmer, Item>)Delegate.Remove(order.onItemCollected, new Action<Farmer, Item>(OnItemShipped));
		}

		public virtual void OnItemShipped(Farmer farmer, Item item)
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
					IncrementCount(item.Stack);
					break;
				}
			}
		}
	}
}
