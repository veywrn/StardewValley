using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class OptionsPlusMinusButton : OptionsPlusMinus
	{
		protected Rectangle _buttonBounds;

		protected Rectangle _buttonRect;

		protected Texture2D _buttonTexture;

		protected Action<string> _buttonAction;

		public OptionsPlusMinusButton(string label, int whichOptions, List<string> options, List<string> displayOptions, Texture2D buttonTexture, Rectangle buttonRect, Action<string> buttonAction, int x = -1, int y = -1)
			: base(label, whichOptions, options, displayOptions, x, y)
		{
			_buttonRect = buttonRect;
			_buttonBounds = new Rectangle(bounds.Left, 4 - _buttonRect.Height / 2 + 8, _buttonRect.Width * 4, _buttonRect.Height * 4);
			_buttonTexture = buttonTexture;
			_buttonAction = buttonAction;
			int offset = 8;
			plusButton.X += _buttonBounds.Width + offset * 4;
			minusButton.X += _buttonBounds.Width + offset * 4;
			bounds.Width += _buttonBounds.Width + offset * 4;
			int height_adjustment = _buttonBounds.Height - bounds.Height;
			if (height_adjustment > 0)
			{
				bounds.Y -= height_adjustment / 2;
				bounds.Height += height_adjustment;
				labelOffset.Y += height_adjustment / 2;
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY)
		{
			b.Draw(_buttonTexture, new Vector2(slotX + _buttonBounds.X, slotY + _buttonBounds.Y), _buttonRect, Color.White * (greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
			base.draw(b, slotX, slotY);
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (!greyedOut && _buttonBounds.Contains(x, y))
			{
				if (_buttonAction != null)
				{
					string selection = "";
					if (selected >= 0 && selected < options.Count)
					{
						selection = options[selected];
					}
					_buttonAction(selection);
				}
			}
			else
			{
				base.receiveLeftClick(x, y);
			}
		}
	}
}
