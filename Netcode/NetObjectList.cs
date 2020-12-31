using System.Collections.Generic;

namespace Netcode
{
	public sealed class NetObjectList<T> : NetList<T, NetRef<T>> where T : class, INetObject<INetSerializable>
	{
		public NetObjectList()
		{
		}

		public NetObjectList(IEnumerable<T> values)
			: base(values)
		{
		}

		public NetObjectList(int capacity)
			: base(capacity)
		{
		}
	}
}
