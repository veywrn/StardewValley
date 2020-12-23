using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class MenuHUDButtons : IClickableMenu
	{
		public new const int width = 70;

		public new const int height = 21;

		private List<ClickableComponent> buttons = new List<ClickableComponent>();

		private string hoverText = "";

		private Vector2 position;

		private Rectangle sourceRect;

		public MenuHUDButtons()
			: base(Game1.uiViewport.Width / 2 + 384 + 64, Game1.uiViewport.Height - 84 - 16, 280, 84)
		{
			for (int i = 0; i < 7; i++)
			{
				buttons.Add(new ClickableComponent(new Rectangle(Game1.uiViewport.Width / 2 + 384 + 16 + i * 9 * 4, yPositionOnScreen + 20, 36, 44), string.Concat(i)));
			}
			position = new Vector2(xPositionOnScreen, yPositionOnScreen);
			sourceRect = new Rectangle(221, 362, 70, 21);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!Game1.player.UsingTool)
			{
				foreach (ClickableComponent c in buttons)
				{
					if (c.containsPoint(x, y))
					{
						Game1.activeClickableMenu = new GameMenu(Convert.ToInt32(c.name));
						Game1.playSound("bigSelect");
						break;
					}
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			hoverText = "";
			foreach (ClickableComponent c in buttons)
			{
				if (c.containsPoint(x, y))
				{
					int slotNumber = Convert.ToInt32(c.name);
					hoverText = GameMenu.getLabelOfTabFromIndex(slotNumber);
				}
			}
		}

		public override void update(GameTime time)
		{
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			xPositionOnScreen = Game1.uiViewport.Width / 2 + 384 + 64;
			yPositionOnScreen = Game1.uiViewport.Height - 84 - 16;
			for (int i = 0; i < 7; i++)
			{
				buttons[i].bounds = new Rectangle(Game1.uiViewport.Width / 2 + 384 + 16 + i * 9 * 4, yPositionOnScreen + 20, 36, 44);
			}
			position = new Vector2(xPositionOnScreen, yPositionOnScreen);
		}

		public override bool isWithinBounds(int x, int y)
		{
			return new Rectangle(buttons.First().bounds.X, buttons.First().bounds.Y, buttons.Last().bounds.X - buttons.First().bounds.X + 64, 64).Contains(x, y);
		}

		public override void draw(SpriteBatch b)
		{
			if (Game1.activeClickableMenu == null)
			{
				b.Draw(Game1.mouseCursors, position, sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				if (!hoverText.Equals("") && isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
				{
					IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
				}
			}
		}
	}
}
