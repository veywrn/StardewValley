using System.Collections.Generic;

namespace Netcode
{
	public sealed class NetLongList : NetList<long, NetLong>
	{
		public NetLongList()
		{
		}

		public NetLongList(IEnumerable<long> values)
			: base(values)
		{
		}

		public NetLongList(int capacity)
			: base(capacity)
		{
		}

		public override bool Contains(long item)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == item)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override int IndexOf(long item)
		{
			NetInt count = base.count;
			for (int i = 0; i < (int)count; i++)
			{
				if (array.Value[i] == item)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
