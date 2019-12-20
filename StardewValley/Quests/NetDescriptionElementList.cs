using Netcode;
using System.Collections.Generic;

namespace StardewValley.Quests
{
	public class NetDescriptionElementList : NetList<DescriptionElement, NetDescriptionElementRef>
	{
		public NetDescriptionElementList()
		{
		}

		public NetDescriptionElementList(IEnumerable<DescriptionElement> values)
			: base(values)
		{
		}

		public NetDescriptionElementList(int capacity)
			: base(capacity)
		{
		}
	}
}
