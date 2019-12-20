using Netcode;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network
{
	public class NetFarmerRef : INetObject<NetFields>, IEnumerable<Farmer>, IEnumerable
	{
		private readonly NetBool defined = new NetBool();

		private readonly NetLong uid = new NetLong();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public long UID
		{
			get
			{
				if (!defined)
				{
					return 0L;
				}
				return uid.Value;
			}
			set
			{
				uid.Value = value;
				defined.Value = true;
			}
		}

		public Farmer Value
		{
			get
			{
				if (!defined)
				{
					return null;
				}
				return getFarmer(uid);
			}
			set
			{
				defined.Value = (value != null);
				uid.Value = (value?.UniqueMultiplayerID ?? 0);
			}
		}

		public NetFarmerRef()
		{
			NetFields.AddFields(defined, uid);
		}

		private Farmer getFarmer(long uid)
		{
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (farmer.UniqueMultiplayerID == uid)
				{
					return farmer;
				}
			}
			return null;
		}

		public NetFarmerRef Delayed(bool interpolationWait)
		{
			defined.Interpolated(interpolate: false, interpolationWait);
			uid.Interpolated(interpolate: false, interpolationWait);
			return this;
		}

		public IEnumerator<Farmer> GetEnumerator()
		{
			if ((bool)defined)
			{
				yield return Value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public static implicit operator Farmer(NetFarmerRef farmerRef)
		{
			return farmerRef.Value;
		}
	}
}
