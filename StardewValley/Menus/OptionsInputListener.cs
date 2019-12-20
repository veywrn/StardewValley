using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class OptionsInputListener : OptionsElement
	{
		public List<string> buttonNames = new List<string>();

		private string listenerMessage;

		private bool listening;

		private Rectangle setbuttonBounds;

		public static Rectangle setButtonSource = new Rectangle(294, 428, 21, 11);

		public OptionsInputListener(string label, int whichOption, int slotWidth, int x = -1, int y = -1)
			: base(label, x, y, slotWidth - x, 44, whichOption)
		{
			setbuttonBounds = new Rectangle(slotWidth - 112, y + 12, 84, 44);
			if (whichOption != -1)
			{
				Game1.options.setInputListenerToProperValue(this);
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			_ = greyedOut;
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (greyedOut || listening || !setbuttonBounds.Contains(x, y))
			{
				return;
			}
			if (whichOption == -1)
			{
				Game1.options.setControlsToDefault();
				if (Game1.activeClickableMenu is GameMenu && (Game1.activeClickableMenu as GameMenu).GetCurrentPage() is OptionsPage)
				{
					foreach (OptionsElement slot in ((Game1.activeClickableMenu as GameMenu).GetCurrentPage() as OptionsPage).options)
					{
						if (slot is OptionsInputListener)
						{
							Game1.options.setInputListenerToProperValue(slot as OptionsInputListener);
						}
					}
				}
			}
			else
			{
				listening = true;
				Game1.playSound("breathin");
				GameMenu.forcePreventClose = true;
				listenerMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsElement.cs.11225");
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (!greyedOut && listening)
			{
				if (Game1.activeClickableMenu is GameMenu && (Game1.activeClickableMenu as GameMenu).GetCurrentPage() is OptionsPage)
				{
					((Game1.activeClickableMenu as GameMenu).GetCurrentPage() as OptionsPage).lastRebindTick = Game1.ticks;
				}
				if (key == Keys.Escape)
				{
					Game1.playSound("bigDeSelect");
					listening = false;
					GameMenu.forcePreventClose = false;
				}
				else if (!Game1.options.isKeyInUse(key) || new InputButton(key).ToString().Equals(buttonNames.First()))
				{
					Game1.options.changeInputListenerValue(whichOption, key);
					buttonNames[0] = new InputButton(key).ToString();
					Game1.playSound("coin");
					listening = false;
					GameMenu.forcePreventClose = false;
				}
				else
				{
					listenerMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsElement.cs.11228");
				}
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY)
		{
			if (buttonNames.Count > 0 || whichOption == -1)
			{
				if (whichOption == -1)
				{
					Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(bounds.X + slotX, bounds.Y + slotY), Game1.textColor, 1f, 0.15f);
				}
				else
				{
					Utility.drawTextWithShadow(b, label + ": " + buttonNames.Last() + ((buttonNames.Count > 1) ? (", " + buttonNames.First()) : ""), Game1.dialogueFont, new Vector2(bounds.X + slotX, bounds.Y + slotY), Game1.textColor, 1f, 0.15f);
				}
			}
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(setbuttonBounds.X + slotX, setbuttonBounds.Y + slotY), setButtonSource, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.15f);
			if (listening)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Rectangle(0, 0, 1, 1), Color.Black * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, 0.999f);
				b.DrawString(Game1.dialogueFont, listenerMessage, Utility.getTopLeftPositionForCenteringOnScreen(192, 64), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999f);
			}
		}
	}
}
