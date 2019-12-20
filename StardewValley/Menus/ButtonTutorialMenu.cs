using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class ButtonTutorialMenu : IClickableMenu
	{
		public const int move_run_check = 0;

		public const int useTool_menu = 1;

		public const float movementSpeed = 0.2f;

		public new const int width = 42;

		public new const int height = 109;

		private int timerToclose = 15000;

		private int which;

		private static int current;

		private int myID;

		public ButtonTutorialMenu(int which)
			: base(-168, Game1.viewport.Height / 2 - 218, 168, 436)
		{
			this.which = which;
			current++;
			myID = current;
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (myID != current)
			{
				destroy = true;
			}
			if (xPositionOnScreen < 0 && timerToclose > 0)
			{
				xPositionOnScreen += (int)((float)time.ElapsedGameTime.Milliseconds * 0.2f);
				if (xPositionOnScreen >= 0)
				{
					xPositionOnScreen = 0;
				}
				return;
			}
			timerToclose -= time.ElapsedGameTime.Milliseconds;
			if (timerToclose <= 0)
			{
				if (xPositionOnScreen >= -232)
				{
					xPositionOnScreen -= (int)((float)time.ElapsedGameTime.Milliseconds * 0.2f);
				}
				else
				{
					destroy = true;
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!destroy)
			{
				if (!Game1.options.gamepadControls)
				{
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(275 + which * 42, 0, 42, 109), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.82f);
				}
				else
				{
					b.Draw(Game1.controllerMaps, new Vector2(xPositionOnScreen, yPositionOnScreen), Utility.controllerMapSourceRect(new Rectangle(512 + which * 42 * 2, 0, 84, 218)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.82f);
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
