using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public abstract class ICreditsBlock
	{
		public virtual void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
		{
		}

		public virtual int getHeight(int maxWidth)
		{
			return 0;
		}

		public virtual void hovered()
		{
		}

		public virtual void clicked()
		{
		}
	}
}
