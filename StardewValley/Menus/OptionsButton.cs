using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Menus
{
	public class OptionsButton : OptionsElement
	{
		private Action action;

		public OptionsButton(string label, Action action)
			: base(label)
		{
			this.action = action;
			int width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
			int height = 80;
			bounds = new Rectangle(32, 0, width, height);
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (bounds.Contains(x, y) && action != null)
			{
				action();
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), slotX + bounds.X, slotY + bounds.Y, bounds.Width, bounds.Height, Color.White, 4f);
			Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(slotX + bounds.Center.X, slotY + bounds.Center.Y + 4) - Game1.dialogueFont.MeasureString(label) / 2f, Game1.textColor, 1f, 1f, -1, -1, 0f);
		}
	}
}
