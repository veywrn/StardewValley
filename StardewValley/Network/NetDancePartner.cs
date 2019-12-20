using Netcode;
using System;

namespace StardewValley.Network
{
	public class NetDancePartner : INetObject<NetFields>
	{
		private readonly NetFarmerRef farmer = new NetFarmerRef();

		private readonly NetString villager = new NetString();

		public Character Value
		{
			get
			{
				return GetCharacter();
			}
			set
			{
				SetCharacter(value);
			}
		}

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public NetDancePartner()
		{
			NetFields.AddFields(farmer.NetFields, villager);
		}

		public NetDancePartner(Farmer farmer)
		{
			this.farmer.Value = farmer;
		}

		public NetDancePartner(string villagerName)
		{
			villager.Value = villagerName;
		}

		public Character GetCharacter()
		{
			if (farmer.Value != null)
			{
				return farmer.Value;
			}
			if (Game1.CurrentEvent != null && villager.Value != null)
			{
				return Game1.CurrentEvent.getActorByName(villager.Value);
			}
			return null;
		}

		public void SetCharacter(Character value)
		{
			if (value == null)
			{
				farmer.Value = null;
				villager.Value = null;
				return;
			}
			if (value is Farmer)
			{
				farmer.Value = (value as Farmer);
				villager.Value = null;
				return;
			}
			if (value is NPC && (value as NPC).isVillager())
			{
				farmer.Value = null;
				villager.Value = (value as NPC).Name;
				return;
			}
			throw new ArgumentException(value.ToString());
		}

		public NPC TryGetVillager()
		{
			if (farmer.Value != null)
			{
				return null;
			}
			if (Game1.CurrentEvent != null && villager.Value != null)
			{
				return Game1.CurrentEvent.getActorByName(villager.Value);
			}
			return null;
		}

		public Farmer TryGetFarmer()
		{
			return farmer.Value;
		}

		public bool IsFarmer()
		{
			return TryGetFarmer() != null;
		}

		public bool IsVillager()
		{
			return TryGetVillager() != null;
		}

		public int GetGender()
		{
			if (IsFarmer())
			{
				if (!TryGetFarmer().IsMale)
				{
					return 1;
				}
				return 0;
			}
			if (IsVillager())
			{
				return TryGetVillager().Gender;
			}
			return 2;
		}
	}
}
