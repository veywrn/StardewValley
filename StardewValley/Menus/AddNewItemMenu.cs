using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class AddNewItemMenu : IClickableMenu
	{
		private InventoryMenu playerInventory;

		private ClickableComponent garbage;

		public AddNewItemMenu()
			: base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (300 + IClickableMenu.borderWidth * 2) / 2, 800 + IClickableMenu.borderWidth * 2, 300 + IClickableMenu.borderWidth * 2)
		{
			playerInventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, playerInventory: true);
			garbage = new ClickableComponent(new Rectangle(xPositionOnScreen + width + IClickableMenu.spaceToClearSideBorder, yPositionOnScreen + height - 64, 64, 64), "Garbage");
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void draw(SpriteBatch b)
		{
		}
	}
}
