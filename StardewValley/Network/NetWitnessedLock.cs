using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley.Network
{
	public class NetWitnessedLock : INetObject<NetFields>
	{
		private readonly NetBool requested = new NetBool().Interpolated(interpolate: false, wait: false);

		private readonly NetFarmerCollection witnesses = new NetFarmerCollection();

		private Action acquired;

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public NetWitnessedLock()
		{
			NetFields.AddFields(requested, witnesses.NetFields);
		}

		public void RequestLock(Action acquired, Action failed)
		{
			if (!Game1.IsMasterGame)
			{
				throw new InvalidOperationException();
			}
			if (acquired == null)
			{
				throw new ArgumentException();
			}
			if ((bool)requested)
			{
				failed();
				return;
			}
			requested.Value = true;
			this.acquired = acquired;
		}

		public bool IsLocked()
		{
			return requested;
		}

		public void Update()
		{
			witnesses.RetainOnlinePlayers();
			if ((bool)requested)
			{
				if (!witnesses.Contains(Game1.player))
				{
					witnesses.Add(Game1.player);
				}
				if (Game1.IsMasterGame)
				{
					foreach (Farmer f in Game1.otherFarmers.Values)
					{
						if (!witnesses.Contains(f))
						{
							return;
						}
					}
					acquired();
					acquired = null;
					requested.Value = false;
					witnesses.Clear();
				}
			}
		}
	}
}
