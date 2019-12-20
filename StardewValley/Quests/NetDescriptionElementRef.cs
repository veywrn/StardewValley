using Netcode;

namespace StardewValley.Quests
{
	public class NetDescriptionElementRef : NetExtendableRef<DescriptionElement, NetDescriptionElementRef>
	{
		public NetDescriptionElementRef()
		{
			Serializer = DescriptionElement.serializer;
		}

		public NetDescriptionElementRef(DescriptionElement value)
			: base(value)
		{
			Serializer = DescriptionElement.serializer;
		}
	}
}
