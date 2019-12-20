using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Events
{
	public interface FarmEvent : INetObject<NetFields>
	{
		bool setUp();

		bool tickUpdate(GameTime time);

		void draw(SpriteBatch b);

		void drawAboveEverything(SpriteBatch b);

		void makeChangesToLocation();
	}
}
