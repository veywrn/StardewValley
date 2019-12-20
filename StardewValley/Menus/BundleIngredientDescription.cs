namespace StardewValley.Menus
{
	public struct BundleIngredientDescription
	{
		public int index;

		public int stack;

		public int quality;

		public bool completed;

		public BundleIngredientDescription(int index, int stack, int quality, bool completed)
		{
			this.completed = completed;
			this.index = index;
			this.stack = stack;
			this.quality = quality;
		}
	}
}
