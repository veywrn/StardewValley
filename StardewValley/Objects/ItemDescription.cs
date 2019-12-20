namespace StardewValley.Objects
{
	public struct ItemDescription
	{
		public byte type;

		public int index;

		public int stack;

		public ItemDescription(byte type, int index, int stack)
		{
			this.type = type;
			this.index = index;
			this.stack = stack;
		}
	}
}
