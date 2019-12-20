using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Minigames
{
	public interface IMinigame
	{
		bool tick(GameTime time);

		bool overrideFreeMouseMovement();

		bool doMainGameUpdates();

		void receiveLeftClick(int x, int y, bool playSound = true);

		void leftClickHeld(int x, int y);

		void receiveRightClick(int x, int y, bool playSound = true);

		void releaseLeftClick(int x, int y);

		void releaseRightClick(int x, int y);

		void receiveKeyPress(Keys k);

		void receiveKeyRelease(Keys k);

		void draw(SpriteBatch b);

		void changeScreenSize();

		void unload();

		void receiveEventPoke(int data);

		string minigameId();

		bool forceQuit();
	}
}
