using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class TooManyFarmsMenu : IClickableMenu
	{
		public const int cWidth = 800;

		public const int cHeight = 180;

		public TooManyFarmsMenu()
		{
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(800, 180);
			initialize((int)topLeft.X, (int)topLeft.Y, 800, 180);
		}

		public override bool readyToClose()
		{
			return true;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			exitThisMenu();
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
		{
			b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle(306, 320, 16, 16), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle(275, 313, 1, 6), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle(275, 328, 1, 8), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White);
			b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			drawBox(b, xPositionOnScreen, yPositionOnScreen, width, height);
			int pad = 35;
			string message = Game1.content.LoadString("Strings\\UI:TooManyFarmsMenu_TooManyFarms");
			SpriteText.drawString(b, message, xPositionOnScreen + pad, yPositionOnScreen + pad, 999999, width, height);
			int ypos = 260;
			Rectangle dstRect = new Rectangle(xPositionOnScreen + width - 14 - 52, yPositionOnScreen + height - 14 - 52, 52, 52);
			Rectangle srcRect = new Rectangle(542, ypos, 26, 26);
			if (Game1.options.gamepadControls)
			{
				b.Draw(Game1.controllerMaps, dstRect, srcRect, Color.White);
			}
		}
	}
}
