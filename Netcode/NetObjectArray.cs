using System.Collections.Generic;

namespace Netcode
{
	public class NetObjectArray<T> : NetArray<T, NetRef<T>> where T : class, INetObject<INetSerializable>
	{
		public NetObjectArray()
		{
		}

		public NetObjectArray(IEnumerable<T> values)
			: base(values)
		{
		}

		public NetObjectArray(int size)
			: base(size)
		{
		}
	}
}
