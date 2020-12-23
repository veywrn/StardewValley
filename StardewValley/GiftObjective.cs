using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class GiftObjective : OrderObjective
	{
		public enum LikeLevels
		{
			None,
			Hated,
			Disliked,
			Neutral,
			Liked,
			Loved
		}

		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		[XmlElement("minimumLikeLevel")]
		public NetEnum<LikeLevels> minimumLikeLevel = new NetEnum<LikeLevels>(LikeLevels.None);

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"]));
			}
			if (data.ContainsKey("MinimumLikeLevel"))
			{
				minimumLikeLevel.Value = (LikeLevels)Enum.Parse(typeof(LikeLevels), data["MinimumLikeLevel"]);
			}
		}

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets, minimumLikeLevel);
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onGiftGiven = (Action<Farmer, NPC, Item>)Delegate.Combine(order.onGiftGiven, new Action<Farmer, NPC, Item>(OnGiftGiven));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onGiftGiven = (Action<Farmer, NPC, Item>)Delegate.Remove(order.onGiftGiven, new Action<Farmer, NPC, Item>(OnGiftGiven));
		}

		public virtual void OnGiftGiven(Farmer farmer, NPC npc, Item item)
		{
			bool is_valid_gift = true;
			foreach (string acceptableContextTagSet in acceptableContextTagSets)
			{
				is_valid_gift = false;
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
					is_valid_gift = true;
					break;
				}
			}
			if (!is_valid_gift)
			{
				return;
			}
			if (minimumLikeLevel.Value > LikeLevels.None)
			{
				switch (npc.getGiftTasteForThisItem(item))
				{
				case 6:
					if (minimumLikeLevel.Value < LikeLevels.Hated)
					{
						return;
					}
					break;
				case 4:
					if (minimumLikeLevel.Value < LikeLevels.Disliked)
					{
						return;
					}
					break;
				case 8:
					if (minimumLikeLevel.Value < LikeLevels.Neutral)
					{
						return;
					}
					break;
				case 2:
					if (minimumLikeLevel.Value < LikeLevels.Liked)
					{
						return;
					}
					break;
				case 0:
					if (minimumLikeLevel.Value < LikeLevels.Loved)
					{
						return;
					}
					break;
				}
			}
			IncrementCount(1);
		}
	}
}
