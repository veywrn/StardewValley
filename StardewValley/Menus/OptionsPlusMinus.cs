using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class OptionsPlusMinus : OptionsElement
	{
		public const int pixelsWide = 7;

		public List<string> options = new List<string>();

		public List<string> displayOptions = new List<string>();

		public int selected;

		public bool isChecked;

		[InstancedStatic]
		public static bool snapZoomPlus;

		[InstancedStatic]
		public static bool snapZoomMinus;

		public Rectangle minusButton;

		public Rectangle plusButton;

		public static Rectangle minusButtonSource = new Rectangle(177, 345, 7, 8);

		public static Rectangle plusButtonSource = new Rectangle(184, 345, 7, 8);

		public OptionsPlusMinus(string label, int whichOption, List<string> options, List<string> displayOptions, int x = -1, int y = -1)
			: base(label, x, y, 28, 28, whichOption)
		{
			this.options = options;
			this.displayOptions = displayOptions;
			Game1.options.setPlusMinusToProperValue(this);
			if (x == -1)
			{
				x = 32;
			}
			if (y == -1)
			{
				y = 16;
			}
			int txtSize = (int)Game1.dialogueFont.MeasureString(options[0]).X + 28;
			foreach (string displayOption in displayOptions)
			{
				txtSize = Math.Max((int)Game1.dialogueFont.MeasureString(displayOption).X + 28, txtSize);
			}
			bounds = new Rectangle(x, y, 56 + txtSize, 32);
			base.label = label;
			base.whichOption = whichOption;
			minusButton = new Rectangle(x, 16, 28, 32);
			plusButton = new Rectangle(bounds.Right - 32, 16, 28, 32);
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (!greyedOut && options.Count > 0)
			{
				int num = selected;
				if (minusButton.Contains(x, y) && selected != 0)
				{
					selected--;
					snapZoomMinus = true;
					Game1.playSound("drumkit6");
				}
				else if (plusButton.Contains(x, y) && selected != options.Count - 1)
				{
					selected++;
					snapZoomPlus = true;
					Game1.playSound("drumkit6");
				}
				if (selected < 0)
				{
					selected = 0;
				}
				else if (selected >= options.Count)
				{
					selected = options.Count - 1;
				}
				if (num != selected)
				{
					Game1.options.changeDropDownOption(whichOption, options[selected]);
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
				{
					receiveLeftClick(plusButton.Center.X, plusButton.Center.Y);
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
				{
					receiveLeftClick(minusButton.Center.X, minusButton.Center.Y);
				}
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			b.Draw(Game1.mouseCursors, new Vector2(slotX + minusButton.X, slotY + minusButton.Y), minusButtonSource, Color.White * (greyedOut ? 0.33f : 1f) * ((selected == 0) ? 0.5f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
			b.DrawString(Game1.dialogueFont, (selected < displayOptions.Count && selected != -1) ? displayOptions[selected] : "", new Vector2(slotX + minusButton.X + minusButton.Width + 4, slotY + minusButton.Y), Game1.textColor);
			b.Draw(Game1.mouseCursors, new Vector2(slotX + plusButton.X, slotY + plusButton.Y), plusButtonSource, Color.White * (greyedOut ? 0.33f : 1f) * ((selected == displayOptions.Count - 1) ? 0.5f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
			if (!Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				if (snapZoomMinus)
				{
					Game1.setMousePosition(slotX + minusButton.Center.X, slotY + minusButton.Center.Y);
					snapZoomMinus = false;
				}
				else if (snapZoomPlus)
				{
					Game1.setMousePosition(slotX + plusButton.Center.X, slotY + plusButton.Center.Y);
					snapZoomPlus = false;
				}
			}
			base.draw(b, slotX, slotY, context);
		}
	}
}
