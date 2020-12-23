using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	internal class FarmerBoxButton : ClickableComponent
	{
		public bool Selected;

		public FarmerBoxButton(string name)
			: base(Rectangle.Empty, name)
		{
		}
	}
}
