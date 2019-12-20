using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using System;

namespace StardewValley.Menus
{
	public class NamingMenu : IClickableMenu
	{
		public delegate void doneNamingBehavior(string s);

		public const int region_okButton = 101;

		public const int region_doneNamingButton = 102;

		public const int region_randomButton = 103;

		public const int region_namingBox = 104;

		public ClickableTextureComponent doneNamingButton;

		public ClickableTextureComponent randomButton;

		protected TextBox textBox;

		public ClickableComponent textBoxCC;

		private TextBoxEvent e;

		private doneNamingBehavior doneNaming;

		private string title;

		protected int minLength = 1;

		public NamingMenu(doneNamingBehavior b, string title, string defaultName = null)
		{
			doneNaming = b;
			xPositionOnScreen = 0;
			yPositionOnScreen = 0;
			width = Game1.viewport.Width;
			height = Game1.viewport.Height;
			this.title = title;
			randomButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 51 + 64, Game1.viewport.Height / 2, 64, 64), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f);
			textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
			textBox.X = Game1.viewport.Width / 2 - 192;
			textBox.Y = Game1.viewport.Height / 2;
			textBox.Width = 256;
			textBox.Height = 192;
			e = textBoxEnter;
			textBox.OnEnterPressed += e;
			Game1.keyboardDispatcher.Subscriber = textBox;
			textBox.Text = ((defaultName != null) ? defaultName : Dialogue.randomName());
			textBox.Selected = true;
			randomButton = new ClickableTextureComponent(new Rectangle(textBox.X + textBox.Width + 64 + 48 - 8, Game1.viewport.Height / 2 + 4, 64, 64), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f)
			{
				myID = 103,
				leftNeighborID = 102
			};
			doneNamingButton = new ClickableTextureComponent(new Rectangle(textBox.X + textBox.Width + 32 + 4, Game1.viewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 102,
				rightNeighborID = 103,
				leftNeighborID = 104
			};
			textBoxCC = new ClickableComponent(new Rectangle(textBox.X, textBox.Y, 192, 48), "")
			{
				myID = 104,
				rightNeighborID = 102
			};
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(104);
			snapCursorToCurrentSnappedComponent();
		}

		public void textBoxEnter(TextBox sender)
		{
			if (sender.Text.Length >= minLength)
			{
				if (doneNaming != null)
				{
					doneNaming(sender.Text);
					textBox.Selected = false;
				}
				else
				{
					Game1.exitActiveMenu();
				}
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (textBox.Selected)
			{
				switch (b)
				{
				case Buttons.DPadUp:
				case Buttons.DPadDown:
				case Buttons.DPadLeft:
				case Buttons.DPadRight:
				case Buttons.LeftThumbstickLeft:
				case Buttons.LeftThumbstickUp:
				case Buttons.LeftThumbstickDown:
				case Buttons.LeftThumbstickRight:
					textBox.Selected = false;
					break;
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (!textBox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				base.receiveKeyPress(key);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (doneNamingButton != null)
			{
				if (doneNamingButton.containsPoint(x, y))
				{
					doneNamingButton.scale = Math.Min(1.1f, doneNamingButton.scale + 0.05f);
				}
				else
				{
					doneNamingButton.scale = Math.Max(1f, doneNamingButton.scale - 0.05f);
				}
			}
			randomButton.tryHover(x, y, 0.5f);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			textBox.Update();
			if (doneNamingButton.containsPoint(x, y))
			{
				textBoxEnter(textBox);
				Game1.playSound("smallSelect");
			}
			else if (randomButton.containsPoint(x, y))
			{
				textBox.Text = Dialogue.randomName();
				randomButton.scale = randomButton.baseScale;
				Game1.playSound("drumkit6");
			}
			textBox.Update();
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			SpriteText.drawStringWithScrollCenteredAt(b, title, Game1.viewport.Width / 2, Game1.viewport.Height / 2 - 128, title);
			textBox.Draw(b);
			doneNamingButton.draw(b);
			randomButton.draw(b);
			drawMouse(b);
		}
	}
}
