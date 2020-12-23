using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
			string text_string = Game1.content.LoadString("Strings\\UI:LocalJoinPrompt");
			Vector2 text_bounds = font.MeasureString(text_string);
			origin -= text_bounds / 2f;
			int extra_width = 32;
			Game1.DrawBox((int)origin.X - extra_width, (int)origin.Y, (int)text_bounds.X + extra_width * 2, (int)text_bounds.Y);
			b.DrawString(font, text_string, origin + new Vector2(4f, 4f), Game1.textShadowColor);
			b.DrawString(font, text_string, origin, Game1.textColor);
			if (!Game1.options.SnappyMenus)
			{
				drawMouse(b);
			}
		}
	}
}
