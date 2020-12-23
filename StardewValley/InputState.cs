using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class InputState
	{
		protected Point _simulatedMousePosition = Point.Zero;

		protected List<Keys> _ignoredKeys = new List<Keys>();

		protected List<Keys> _pressedKeys = new List<Keys>();

		protected KeyboardState? _keyState;

		protected int _lastKeyStateTick = -1;

		protected KeyboardState _currentKeyboardState;

		protected MouseState _currentMouseState;

		protected GamePadState _currentGamepadState;

		public virtual void UpdateStates()
		{
			_currentKeyboardState = Keyboard.GetState();
			_currentMouseState = Mouse.GetState();
			if (Game1.playerOneIndex >= PlayerIndex.One)
			{
				_currentGamepadState = GamePad.GetState(Game1.playerOneIndex);
			}
			else
			{
				_currentGamepadState = default(GamePadState);
			}
		}

		public virtual void Update()
		{
		}

		public virtual void IgnoreKeys(Keys[] keys)
		{
			if (keys.Length != 0)
			{
				_ignoredKeys.AddRange(keys);
				string keys_to_ignore = "";
				for (int i = 0; i < keys.Length; i++)
				{
					Keys key = keys[i];
					keys_to_ignore = keys_to_ignore + key.ToString() + " ";
				}
				Console.WriteLine("Ignoring keys: " + keys_to_ignore.Trim());
			}
		}

		public virtual KeyboardState GetKeyboardState()
		{
			if (!Game1.game1.IsMainInstance || !Game1.game1.HasKeyboardFocus())
			{
				return default(KeyboardState);
			}
			if (_lastKeyStateTick != Game1.ticks || !_keyState.HasValue)
			{
				if (_ignoredKeys.Count == 0)
				{
					_keyState = _currentKeyboardState;
				}
				else
				{
					_pressedKeys.Clear();
					_pressedKeys.AddRange(_currentKeyboardState.GetPressedKeys());
					for (int j = 0; j < _ignoredKeys.Count; j++)
					{
						Keys key = _ignoredKeys[j];
						if (!_pressedKeys.Contains(key))
						{
							_ignoredKeys.RemoveAt(j);
							j--;
						}
					}
					for (int i = 0; i < _pressedKeys.Count; i++)
					{
						Keys key2 = _pressedKeys[i];
						if (_ignoredKeys.Contains(key2))
						{
							_pressedKeys.RemoveAt(i);
							i--;
						}
					}
					_keyState = new KeyboardState(_pressedKeys.ToArray());
				}
				_lastKeyStateTick = Game1.ticks;
			}
			return _keyState.Value;
		}

		public virtual GamePadState GetGamePadState()
		{
			if (Game1.options.gamepadMode == Options.GamepadModes.ForceOff || Game1.playerOneIndex == (PlayerIndex)(-1))
			{
				return default(GamePadState);
			}
			return _currentGamepadState;
		}

		public virtual MouseState GetMouseState()
		{
			if (!Game1.game1.IsMainInstance)
			{
				return new MouseState(_simulatedMousePosition.X, _simulatedMousePosition.Y, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
			}
			return _currentMouseState;
		}

		public virtual void SetMousePosition(int x, int y)
		{
			if (!Game1.game1.IsMainInstance)
			{
				_simulatedMousePosition.X = x;
				_simulatedMousePosition.Y = y;
			}
			else
			{
				Mouse.SetPosition(x, y);
				_currentMouseState = new MouseState(x, y, _currentMouseState.ScrollWheelValue, _currentMouseState.LeftButton, _currentMouseState.MiddleButton, _currentMouseState.RightButton, _currentMouseState.XButton1, _currentMouseState.XButton2);
			}
		}
	}
}
