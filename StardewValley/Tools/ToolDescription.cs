namespace StardewValley.Tools
{
	public struct ToolDescription
	{
		public byte index;

		public byte upgradeLevel;

		public ToolDescription(byte index, byte upgradeLevel)
		{
			this.index = index;
			this.upgradeLevel = upgradeLevel;
		}
	}
}
