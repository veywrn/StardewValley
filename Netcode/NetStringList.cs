using System.Collections.Generic;

namespace Netcode
{
	public sealed class NetStringList : NetList<string, NetString>
	{
		public NetStringList()
		{
		}

		public NetStringList(IEnumerable<string> values)
			: base(values)
		{
		}

		public NetStringList(int capacity)
			: base(capacity)
		{
		}
	}
}
