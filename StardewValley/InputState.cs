using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	public class InputState
	{
		public virtual void Update()
		{
		}

		public virtual KeyboardState GetKeyboardState()
		{
			return Keyboard.GetState();
		}

		public virtual GamePadState GetGamePadState()
		{
			if (Game1.options.gamepadMode == Options.GamepadModes.ForceOff)
			{
				return default(GamePadState);
			}
			return GamePad.GetState(PlayerIndex.One);
		}

		public virtual MouseState GetMouseState()
		{
			return Mouse.GetState();
		}
	}
}
