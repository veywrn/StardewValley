using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class ProfileItem
	{
		protected ProfileMenu _context;

		public string itemName = "";

		protected Vector2 _nameDrawPosition;

		public ProfileItem(ProfileMenu context, string name)
		{
			_context = context;
			itemName = name;
		}

		public virtual void Unload()
		{
		}

		public virtual string GetName()
		{
			return itemName;
		}

		public virtual void performHover(int x, int y)
		{
		}

		public virtual float HandleLayout(float draw_y, Rectangle content_rectangle, int index)
		{
			if (index > 0)
			{
				draw_y += Game1.smallFont.MeasureString(GetName()).Y;
			}
			_nameDrawPosition = new Vector2(content_rectangle.Left, draw_y);
			draw_y += Game1.smallFont.MeasureString(GetName()).Y;
			return draw_y;
		}

		public virtual void DrawItemName(SpriteBatch b)
		{
			b.DrawString(Game1.smallFont, GetName(), _nameDrawPosition, Game1.textColor);
		}

		public virtual void Draw(SpriteBatch b)
		{
			DrawItemName(b);
			DrawItem(b);
		}

		public virtual void DrawItem(SpriteBatch b)
		{
		}

		public virtual bool ShouldDraw()
		{
			return true;
		}
	}
}
