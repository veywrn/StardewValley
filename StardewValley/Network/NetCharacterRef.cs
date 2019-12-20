using Netcode;
using System;

namespace StardewValley.Network
{
	public class NetCharacterRef : INetObject<NetFields>
	{
		private readonly NetNPCRef npc = new NetNPCRef();

		private readonly NetFarmerRef farmer = new NetFarmerRef();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public NetCharacterRef()
		{
			NetFields.AddFields(npc.NetFields, farmer.NetFields);
		}

		public Character Get(GameLocation location)
		{
			NPC npcValue = npc.Get(location);
			if (npcValue != null)
			{
				return npcValue;
			}
			return farmer.Value;
		}

		public void Set(GameLocation location, Character character)
		{
			if (character is NPC)
			{
				npc.Set(location, character as NPC);
				farmer.Value = null;
				return;
			}
			if (character is Farmer)
			{
				npc.Clear();
				farmer.Value = (character as Farmer);
				return;
			}
			throw new ArgumentException();
		}

		public void Clear()
		{
			npc.Clear();
			farmer.Value = null;
		}
	}
}
