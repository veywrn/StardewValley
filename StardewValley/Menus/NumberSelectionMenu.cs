using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace StardewValley.Menus
{
	public class NumberSelectionMenu : IClickableMenu
	{
		public delegate void behaviorOnNumberSelect(int number, int price, Farmer who);

		public const int region_leftButton = 101;

		public const int region_rightButton = 102;

		public const int region_okButton = 103;

		public const int region_cancelButton = 104;

		private string message;

		protected int price;

		protected int minValue;

		protected int maxValue;

		protected int currentValue;

		protected int priceShake;

		protected int heldTimer;

		private behaviorOnNumberSelect behaviorFunction;

		protected TextBox numberSelectedBox;

		public ClickableTextureComponent leftButton;

		public ClickableTextureComponent rightButton;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent cancelButton;

		protected virtual Vector2 centerPosition => new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height / 2);

		public NumberSelectionMenu(string message, behaviorOnNumberSelect behaviorOnSelection, int price = -1, int minValue = 0, int maxValue = 99, int defaultNumber = 0)
		{
			Vector2 vector = Game1.dialogueFont.MeasureString(message);
			int menuWidth = Math.Max((int)vector.X, 600) + IClickableMenu.borderWidth * 2;
			int menuHeight = (int)vector.Y + IClickableMenu.borderWidth * 2 + 160;
			int menuX = (int)centerPosition.X - menuWidth / 2;
			int menuY = (int)centerPosition.Y - menuHeight / 2;
			initialize(menuX, menuY, menuWidth, menuHeight);
			this.message = message;
			this.price = price;
			this.minValue = minValue;
			this.maxValue = maxValue;
			currentValue = defaultNumber;
			behaviorFunction = behaviorOnSelection;
			numberSelectedBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = xPositionOnScreen + IClickableMenu.borderWidth + 56,
				Y = yPositionOnScreen + IClickableMenu.borderWidth + height / 2,
				Text = string.Concat(currentValue),
				numbersOnly = true,
				textLimit = string.Concat(maxValue).Length
			};
			numberSelectedBox.SelectMe();
			leftButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + height / 2, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 101,
				rightNeighborID = 102,
				upNeighborID = -99998
			};
			rightButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth + 64 + numberSelectedBox.Width, yPositionOnScreen + IClickableMenu.borderWidth + height / 2, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 102,
				leftNeighborID = 101,
				rightNeighborID = 103,
				upNeighborID = -99998
			};
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 103,
				leftNeighborID = 102,
				rightNeighborID = 104,
				upNeighborID = -99998
			};
			cancelButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 104,
				leftNeighborID = 103,
				upNeighborID = -99998
			};
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(102);
			snapCursorToCurrentSnappedComponent();
		}

		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
			if (b != Buttons.A || currentlySnappedComponent == null)
			{
				return;
			}
			heldTimer += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			if (heldTimer <= 300)
			{
				return;
			}
			if (currentlySnappedComponent.myID == 102)
			{
				int tempNumber2 = currentValue + 1;
				if (tempNumber2 <= maxValue && (price == -1 || tempNumber2 * price <= Game1.player.Money))
				{
					rightButton.scale = rightButton.baseScale;
					currentValue = tempNumber2;
					numberSelectedBox.Text = string.Concat(currentValue);
				}
			}
			else if (currentlySnappedComponent.myID == 101)
			{
				int tempNumber = currentValue - 1;
				if (tempNumber >= minValue)
				{
					leftButton.scale = leftButton.baseScale;
					currentValue = tempNumber;
					numberSelectedBox.Text = string.Concat(currentValue);
				}
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (leftButton.containsPoint(x, y))
			{
				int tempNumber2 = currentValue - 1;
				if (tempNumber2 >= minValue)
				{
					leftButton.scale = leftButton.baseScale;
					currentValue = tempNumber2;
					numberSelectedBox.Text = string.Concat(currentValue);
					Game1.playSound("smallSelect");
				}
			}
			if (rightButton.containsPoint(x, y))
			{
				int tempNumber = currentValue + 1;
				if (tempNumber <= maxValue && (price == -1 || tempNumber * price <= Game1.player.Money))
				{
					rightButton.scale = rightButton.baseScale;
					currentValue = tempNumber;
					numberSelectedBox.Text = string.Concat(currentValue);
					Game1.playSound("smallSelect");
				}
			}
			if (okButton.containsPoint(x, y))
			{
				if (currentValue > maxValue || currentValue < minValue)
				{
					currentValue = Math.Max(minValue, Math.Min(maxValue, currentValue));
					numberSelectedBox.Text = string.Concat(currentValue);
				}
				else
				{
					behaviorFunction(currentValue, price, Game1.player);
				}
				Game1.playSound("smallSelect");
			}
			if (cancelButton.containsPoint(x, y))
			{
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect");
				Game1.player.canMove = true;
			}
			numberSelectedBox.Update();
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (key == Keys.Enter)
			{
				receiveLeftClick(okButton.bounds.Center.X, okButton.bounds.Center.Y);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			currentValue = 0;
			if (numberSelectedBox.Text != null)
			{
				int.TryParse(numberSelectedBox.Text, out currentValue);
			}
			if (priceShake > 0)
			{
				priceShake -= time.ElapsedGameTime.Milliseconds;
			}
			if (Game1.options.SnappyMenus)
			{
				_ = Game1.oldPadState;
				if (!Game1.oldPadState.IsButtonDown(Buttons.A))
				{
					heldTimer = 0;
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			if (okButton.containsPoint(x, y) && (price == -1 || currentValue > minValue))
			{
				okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.2f);
			}
			else
			{
				okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
			}
			if (cancelButton.containsPoint(x, y))
			{
				cancelButton.scale = Math.Min(cancelButton.scale + 0.02f, cancelButton.baseScale + 0.2f);
			}
			else
			{
				cancelButton.scale = Math.Max(cancelButton.scale - 0.02f, cancelButton.baseScale);
			}
			if (leftButton.containsPoint(x, y))
			{
				leftButton.scale = Math.Min(leftButton.scale + 0.02f, leftButton.baseScale + 0.2f);
			}
			else
			{
				leftButton.scale = Math.Max(leftButton.scale - 0.02f, leftButton.baseScale);
			}
			if (rightButton.containsPoint(x, y))
			{
				rightButton.scale = Math.Min(rightButton.scale + 0.02f, rightButton.baseScale + 0.2f);
			}
			else
			{
				rightButton.scale = Math.Max(rightButton.scale - 0.02f, rightButton.baseScale);
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
			b.DrawString(Game1.dialogueFont, message, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2), Game1.textColor);
			okButton.draw(b);
			cancelButton.draw(b);
			leftButton.draw(b);
			rightButton.draw(b);
			if (price != -1)
			{
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price * currentValue), new Vector2(rightButton.bounds.Right + 32 + ((priceShake > 0) ? Game1.random.Next(-1, 2) : 0), rightButton.bounds.Y + ((priceShake > 0) ? Game1.random.Next(-1, 2) : 0)), (currentValue * price > Game1.player.Money) ? Color.Red : Game1.textColor);
			}
			numberSelectedBox.Draw(b);
			drawMouse(b);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
