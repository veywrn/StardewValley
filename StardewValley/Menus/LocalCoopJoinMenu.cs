using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace StardewValley.Menus
{
	public class LocalCoopJoinMenu : IClickableMenu
	{
		public override void update(GameTime time)
		{
			base.update(time);
			int max_players = GameRunner.instance.GetMaxSimultaneousPlayers();
			if (GameRunner.instance.gameInstances.Count >= max_players)
			{
				return;
			}
			for (PlayerIndex i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
			{
				if (GameRunner.instance.gameInstances.Count >= max_players)
				{
					break;
				}
				if (!GameRunner.instance.IsStartDown(i))
				{
					continue;
				}
				bool fail = false;
				foreach (Game1 instances in GameRunner.instance.gameInstances)
				{
					if (instances.instancePlayerOneIndex == i && !instances.IsMainInstance)
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					if (i == PlayerIndex.One)
					{
						GameRunner.instance.gameInstances[0].instancePlayerOneIndex = (PlayerIndex)(-1);
					}
					GameRunner.instance.AddGameInstance(i);
				}
			}
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
			if (key == Keys.Escape)
			{
				exitThisMenu();
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black * 0.75f);
			Vector2 origin = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2);
			SpriteFont font = Game1.smallFont;
			string[] text_split = Game1.content.LoadString("Strings\\UI:LocalJoinPrompt").Split('*');
			Vector2 text_bounds = font.MeasureString(text_split[0]);
			text_bounds.X += 32f;
			int first_part_length = (int)text_bounds.X;
			text_bounds.X += font.MeasureString(text_split[1]).X;
			text_bounds.Y = Math.Max(text_bounds.Y, font.MeasureString(text_split[1]).Y);
			origin -= text_bounds / 2f;
			int extra_width = 32;
			int box_height = Math.Max((int)text_bounds.Y, 32);
			Game1.DrawBox((int)origin.X - extra_width, (int)origin.Y, (int)text_bounds.X + extra_width * 2, box_height);
			b.DrawString(font, text_split[0], origin + new Vector2(4f, 4f), Game1.textShadowColor);
			b.DrawString(font, text_split[1], origin + new Vector2(first_part_length, 0f) + new Vector2(4f, 4f), Game1.textShadowColor);
			Vector2 button_draw_position = origin + new Vector2(first_part_length - 16, 0f);
			button_draw_position.Y += font.MeasureString("XX").X / 2f;
			b.Draw(Game1.controllerMaps, button_draw_position + new Vector2(4f, 4f), Utility.controllerMapSourceRect(new Rectangle(653, 260, 28, 28)), Color.Black * 0.25f, 0f, new Vector2(14f, 14f), 1f, SpriteEffects.None, 0.99f);
			b.Draw(Game1.controllerMaps, button_draw_position, Utility.controllerMapSourceRect(new Rectangle(653, 260, 28, 28)), Color.White, 0f, new Vector2(14f, 14f), 1f, SpriteEffects.None, 0.99f);
			b.DrawString(font, text_split[0], origin, Game1.textColor);
			b.DrawString(font, text_split[1], origin + new Vector2(first_part_length, 0f), Game1.textColor);
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
			if (!Game1.options.SnappyMenus)
			{
				drawMouse(b);
			}
		}
	}
}
