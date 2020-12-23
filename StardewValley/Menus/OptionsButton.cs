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
			int height = 68;
			bounds = new Rectangle(32, 0, width, height);
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (!greyedOut && bounds.Contains(x, y) && action != null)
			{
				action();
			}
			base.receiveLeftClick(x, y);
		}

		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			float draw_layer = 0.8f - (float)(slotY + bounds.Y) * 1E-06f;
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), slotX + bounds.X, slotY + bounds.Y, bounds.Width, bounds.Height, Color.White * (greyedOut ? 0.33f : 1f), 4f, drawShadow: true, draw_layer);
			Vector2 string_center = Game1.dialogueFont.MeasureString(label) / 2f;
			string_center.X = (int)(string_center.X / 4f) * 4;
			string_center.Y = (int)(string_center.Y / 4f) * 4;
			Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(slotX + bounds.Center.X, slotY + bounds.Center.Y) - string_center, Game1.textColor * (greyedOut ? 0.33f : 1f), 1f, draw_layer + 1E-06f, -1, -1, 0f);
		}
	}
}
