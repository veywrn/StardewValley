using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class DeliverObjective : OrderObjective
	{
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		[XmlElement("targetName")]
		public NetString targetName = new NetString();

		[XmlElement("message")]
		public NetString message = new NetString();

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"]));
			}
			if (data.ContainsKey("TargetName"))
			{
				targetName.Value = order.Parse(data["TargetName"]);
			}
			else
			{
				targetName.Value = _order.requester.Value;
			}
			if (data.ContainsKey("Message"))
			{
				message.Value = order.Parse(data["Message"]);
			}
			else
			{
				message.Value = "";
			}
		}

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets, targetName, message);
		}

		public override bool ShouldShowProgress()
		{
			return false;
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onItemDelivered = (Func<Farmer, NPC, Item, int>)Delegate.Combine(order.onItemDelivered, new Func<Farmer, NPC, Item, int>(OnItemDelivered));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onItemDelivered = (Func<Farmer, NPC, Item, int>)Delegate.Remove(order.onItemDelivered, new Func<Farmer, NPC, Item, int>(OnItemDelivered));
		}

		public virtual int OnItemDelivered(Farmer farmer, NPC npc, Item item)
		{
			if (IsComplete())
			{
				return 0;
			}
			if (npc.Name != targetName.Value)
			{
				return 0;
			}
			bool is_valid_delivery = true;
			foreach (string acceptableContextTagSet in acceptableContextTagSets)
			{
				is_valid_delivery = false;
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
					is_valid_delivery = true;
					break;
				}
			}
			if (!is_valid_delivery)
			{
				return 0;
			}
			int required_amount = GetMaxCount() - GetCount();
			int donated_amount = Math.Min(item.Stack, required_amount);
			if (donated_amount < required_amount)
			{
				return 0;
			}
			Item donated_item = item.getOne();
			donated_item.Stack = donated_amount;
			_order.donatedItems.Add(donated_item);
			item.Stack -= donated_amount;
			IncrementCount(donated_amount);
			if (!string.IsNullOrEmpty(message.Value))
			{
				npc.CurrentDialogue.Push(new Dialogue(message.Value, npc));
				Game1.drawDialogue(npc);
			}
			return donated_amount;
		}
	}
}
