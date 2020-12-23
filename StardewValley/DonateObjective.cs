using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class DonateObjective : OrderObjective
	{
		[XmlElement("dropBox")]
		public NetString dropBox = new NetString();

		[XmlElement("dropBoxGameLocation")]
		public NetString dropBoxGameLocation = new NetString();

		[XmlElement("dropBoxTileLocation")]
		public NetVector2 dropBoxTileLocation = new NetVector2();

		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		[XmlElement("minimumCapacity")]
		public NetInt minimumCapacity = new NetInt(-1);

		[XmlElement("confirmed")]
		public NetBool confirmed = new NetBool(value: false);

		public virtual string GetDropboxLocationName()
		{
			if (dropBoxGameLocation.Value == "Trailer" && Game1.MasterPlayer.hasOrWillReceiveMail("pamHouseUpgrade"))
			{
				return "Trailer_Big";
			}
			return dropBoxGameLocation.Value;
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"].Trim()));
			}
			if (data.ContainsKey("DropBox"))
			{
				dropBox.Value = order.Parse(data["DropBox"].Trim());
			}
			if (data.ContainsKey("DropBoxGameLocation"))
			{
				dropBoxGameLocation.Value = order.Parse(data["DropBoxGameLocation"].Trim());
			}
			if (data.ContainsKey("DropBoxIndicatorLocation"))
			{
				string coordinates = order.Parse(data["DropBoxIndicatorLocation"]);
				dropBoxTileLocation.Value = new NetVector2(new Vector2((float)Convert.ToDouble(coordinates.Split(' ')[0]), (float)Convert.ToDouble(coordinates.Split(' ')[1])));
			}
			if (data.ContainsKey("MinimumCapacity"))
			{
				minimumCapacity.Value = int.Parse(order.Parse(data["MinimumCapacity"]));
			}
		}

		public int GetAcceptCount(Item item, int stack_count)
		{
			if (IsValidItem(item))
			{
				return Math.Min(GetMaxCount() - GetCount(), stack_count);
			}
			return 0;
		}

		public override bool IsComplete()
		{
			return base.IsComplete();
		}

		public override void OnCompletion()
		{
			base.OnCompletion();
			if (dropBoxGameLocation != null)
			{
				GameLocation i = Game1.getLocationFromName(GetDropboxLocationName());
				if (i != null)
				{
					i.showDropboxIndicator = false;
				}
			}
		}

		public override bool CanComplete()
		{
			return confirmed.Value;
		}

		public virtual void Confirm()
		{
			if (GetCount() >= GetMaxCount())
			{
				confirmed.Value = true;
			}
			else
			{
				confirmed.Value = false;
			}
		}

		public override bool CanUncomplete()
		{
			return true;
		}

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets, dropBox, dropBoxGameLocation, dropBoxTileLocation, minimumCapacity, confirmed);
			confirmed.fieldChangeVisibleEvent += OnConfirmed;
		}

		protected void OnConfirmed(NetBool field, bool oldValue, bool newValue)
		{
			if (!Utility.ShouldIgnoreValueChangeCallback())
			{
				CheckCompletion();
			}
		}

		public virtual bool IsValidItem(Item item)
		{
			if (item == null)
			{
				return false;
			}
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
					return true;
				}
			}
			return false;
		}
	}
}
