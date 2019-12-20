using Netcode;
using System;

namespace StardewValley.Network
{
	public class NetNPCRef : INetObject<NetFields>
	{
		private readonly NetGuid guid = new NetGuid();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public NetNPCRef()
		{
			NetFields.AddFields(guid);
		}

		public NPC Get(GameLocation location)
		{
			if (guid.Value == Guid.Empty || location == null)
			{
				return null;
			}
			if (!location.characters.ContainsGuid(guid.Value))
			{
				return null;
			}
			return location.characters[guid.Value];
		}

		public void Set(GameLocation location, NPC npc)
		{
			if (npc == null)
			{
				guid.Value = Guid.Empty;
				return;
			}
			Guid newGuid = location.characters.GuidOf(npc);
			if (newGuid == Guid.Empty)
			{
				throw new ArgumentException();
			}
			guid.Value = newGuid;
		}

		public void Clear()
		{
			guid.Value = Guid.Empty;
		}
	}
}
