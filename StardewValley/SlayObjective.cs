using Netcode;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class SlayObjective : OrderObjective
	{
		[XmlElement("targetNames")]
		public NetStringList targetNames = new NetStringList();

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(targetNames);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			base.Load(order, data);
			if (data.ContainsKey("TargetName"))
			{
				string[] array = order.Parse(data["TargetName"]).Split(',');
				foreach (string target in array)
				{
					targetNames.Add(target.Trim());
				}
			}
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onMonsterSlain = (Action<Farmer, Monster>)Delegate.Combine(order.onMonsterSlain, new Action<Farmer, Monster>(OnMonsterSlain));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onMonsterSlain = (Action<Farmer, Monster>)Delegate.Remove(order.onMonsterSlain, new Action<Farmer, Monster>(OnMonsterSlain));
		}

		public virtual void OnMonsterSlain(Farmer farmer, Monster monster)
		{
			foreach (string target in targetNames)
			{
				if (monster.Name.Contains(target))
				{
					IncrementCount(1);
					break;
				}
			}
		}
	}
}
