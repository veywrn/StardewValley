namespace StardewValley.Tools
{
	public abstract class Stackable : Tool
	{
		private int numberInStack;

		public int NumberInStack
		{
			get
			{
				return numberInStack;
			}
			set
			{
				numberInStack = value;
			}
		}

		public Stackable()
		{
		}

		public Stackable(string name, int upgradeLevel, int initialParentTileIndex, int indexOfMenuItemView, bool stackable)
			: base(name, upgradeLevel, initialParentTileIndex, indexOfMenuItemView, stackable)
		{
		}
	}
}
