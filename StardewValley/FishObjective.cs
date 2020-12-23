using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class FishObjective : OrderObjective
	{
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"]));
			}
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onFishCaught = (Action<Farmer, Item>)Delegate.Combine(order.onFishCaught, new Action<Farmer, Item>(OnFishCaught));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onFishCaught = (Action<Farmer, Item>)Delegate.Remove(order.onFishCaught, new Action<Farmer, Item>(OnFishCaught));
		}

		public virtual void OnFishCaught(Farmer farmer, Item fish_item)
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
						if (fish_item.HasContextTag(acceptable_tag.Trim()))
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
					IncrementCount(fish_item.Stack);
					break;
				}
			}
		}
	}
}
