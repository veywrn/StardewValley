using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class OptionsTextEntry : OptionsElement
	{
		public const int pixelsHigh = 11;

		public TextBox textBox;

		public OptionsTextEntry(string label, int whichOption, int x = -1, int y = -1)
			: base(label, x, y, (int)Game1.smallFont.MeasureString("Windowed Borderless Mode   ").X + 48, 44, whichOption)
		{
			textBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Color.Black);
			textBox.Width = bounds.Width;
		}

		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			textBox.X = slotX + bounds.Left - 8;
			textBox.Y = slotY + bounds.Top;
			textBox.Draw(b);
			base.draw(b, slotX, slotY, context);
		}

		public override void receiveLeftClick(int x, int y)
		{
			textBox.SelectMe();
			textBox.Update();
		}
	}
}
