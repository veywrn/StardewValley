using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace StardewValley.Menus
{
	public class ScreenSizeAdjustMenu : IClickableMenu
	{
		public ScreenSizeAdjustMenu()
		{
			Game1.shouldDrawSafeAreaBounds = true;
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B)
			{
				exitThisMenu();
			}
			else
			{
				base.receiveGamePadButton(b);
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.moveUpButton.Contains(new InputButton(key)))
			{
				Game1.AdjustScreenScale(-0.01f);
			}
			else if (Game1.options.moveDownButton.Contains(new InputButton(key)))
			{
				Game1.AdjustScreenScale(0.01f);
			}
			if (key == Keys.Escape)
			{
				exitThisMenu();
			}
		}

		protected override void cleanupBeforeExit()
		{
			Game1.shouldDrawSafeAreaBounds = false;
			base.cleanupBeforeExit();
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black * 0.75f);
			Vector2 origin = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2);
			SpriteFont font = Game1.smallFont;
			string text_string = Game1.content.LoadString("Strings\\UI:DisplayAdjustmentText");
			Vector2 text_bounds = font.MeasureString(text_string);
			text_bounds.X += 32f;
			origin -= text_bounds / 2f;
			int extra_width = 32;
			int box_height = Math.Max((int)text_bounds.Y, 32);
			Game1.DrawBox((int)origin.X - extra_width, (int)origin.Y, (int)text_bounds.X + extra_width * 2, box_height);
			b.DrawString(font, text_string, origin + new Vector2(4f, 4f), Game1.textShadowColor);
			b.DrawString(font, text_string, origin, Game1.textColor);
			string exit_text = Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel");
			origin.Y -= text_bounds.Y / 2f;
			origin.Y += box_height;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				origin.Y += 48f;
			}
			else
			{
				origin.Y += 32f;
			}
			origin.X += text_bounds.X + (float)extra_width;
			origin.X -= font.MeasureString(exit_text).X;
			b.DrawString(font, exit_text, origin, Color.White);
			origin.X -= font.MeasureString("XX").X;
			origin += font.MeasureString("X") / 2f;
			b.Draw(Game1.controllerMaps, origin, Utility.controllerMapSourceRect(new Rectangle(569, 260, 28, 28)), Color.White, 0f, new Vector2(14f, 14f), 1f, SpriteEffects.None, 0.99f);
		}
	}
}
