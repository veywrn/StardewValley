using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class MineElevatorMenu : IClickableMenu
	{
		public List<ClickableComponent> elevators = new List<ClickableComponent>();

		public MineElevatorMenu()
			: base(0, 0, 0, 0, showUpperRightCloseButton: true)
		{
			int numElevators = Math.Min(MineShaft.lowestLevelReached, 120) / 5;
			width = ((numElevators > 50) ? (484 + IClickableMenu.borderWidth * 2) : Math.Min(220 + IClickableMenu.borderWidth * 2, numElevators * 44 + IClickableMenu.borderWidth * 2));
			height = Math.Max(64 + IClickableMenu.borderWidth * 3, numElevators * 44 / (width - IClickableMenu.borderWidth) * 44 + 64 + IClickableMenu.borderWidth * 3);
			xPositionOnScreen = Game1.viewport.Width / 2 - width / 2;
			yPositionOnScreen = Game1.viewport.Height / 2 - height / 2;
			Game1.playSound("crystal");
			int buttonsPerRow = width / 44 - 1;
			int x2 = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
			int y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.borderWidth / 3;
			elevators.Add(new ClickableComponent(new Rectangle(x2, y, 44, 44), string.Concat(0))
			{
				myID = 0,
				rightNeighborID = 1,
				downNeighborID = buttonsPerRow
			});
			x2 = x2 + 64 - 20;
			if (x2 > xPositionOnScreen + width - IClickableMenu.borderWidth)
			{
				x2 = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
				y += 44;
			}
			for (int i = 1; i <= numElevators; i++)
			{
				elevators.Add(new ClickableComponent(new Rectangle(x2, y, 44, 44), string.Concat(i * 5))
				{
					myID = i,
					rightNeighborID = ((i % buttonsPerRow == buttonsPerRow - 1) ? (-1) : (i + 1)),
					leftNeighborID = ((i % buttonsPerRow == 0) ? (-1) : (i - 1)),
					downNeighborID = i + buttonsPerRow,
					upNeighborID = i - buttonsPerRow
				});
				x2 = x2 + 64 - 20;
				if (x2 > xPositionOnScreen + width - IClickableMenu.borderWidth)
				{
					x2 = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
					y += 44;
				}
			}
			initializeUpperRightCloseButton();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (isWithinBounds(x, y))
			{
				foreach (ClickableComponent c in elevators)
				{
					if (c.containsPoint(x, y))
					{
						Game1.playSound("smallSelect");
						if (Convert.ToInt32(c.name) == 0)
						{
							if (!(Game1.currentLocation is MineShaft))
							{
								return;
							}
							Game1.warpFarmer("Mine", 17, 4, flip: true);
							Game1.exitActiveMenu();
							Game1.changeMusicTrack("none");
						}
						else
						{
							if (Convert.ToInt32(c.name) == Game1.CurrentMineLevel)
							{
								return;
							}
							Game1.player.ridingMineElevator = true;
							Game1.enterMine(Convert.ToInt32(c.name));
							Game1.exitActiveMenu();
						}
					}
				}
				base.receiveLeftClick(x, y);
			}
			else
			{
				Game1.exitActiveMenu();
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableComponent c in elevators)
			{
				if (c.containsPoint(x, y))
				{
					c.scale = 2f;
				}
				else
				{
					c.scale = 1f;
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen - 64 + 8, width + 21, height + 64, speaker: false, drawOnlyBox: true);
			foreach (ClickableComponent c in elevators)
			{
				b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X - 4, c.bounds.Y + 4), new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10), Color.Black * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X, c.bounds.Y), new Rectangle((c.scale > 1f) ? 267 : 256, 256, 10, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
				NumberSprite.draw(position: new Vector2(c.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(c.name)) * 6, c.bounds.Y + 24 - NumberSprite.getHeight() / 4), number: Convert.ToInt32(c.name), b: b, c: (Game1.CurrentMineLevel == Convert.ToInt32(c.name)) ? (Color.Gray * 0.75f) : Color.Gold, scale: 0.5f, layerDepth: 0.86f, alpha: 1f, secondDigitOffset: 0);
			}
			drawMouse(b);
			base.draw(b);
		}
	}
}
