namespace StardewValley
{
	internal struct ShippedItem
	{
		public int index;

		public int price;

		public string name;

		public ShippedItem(int index, int price, string name)
		{
			this.index = index;
			this.price = price;
			this.name = name;
		}
	}
}
