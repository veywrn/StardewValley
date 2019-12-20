using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	public interface ICustomEventScript
	{
		bool update(GameTime time, Event e);

		void draw(SpriteBatch b);

		void drawAboveAlwaysFront(SpriteBatch b);
	}
}
