using Microsoft.Xna.Framework.Input;
using System;

namespace StardewValley
{
	public struct ButtonCollection
	{
		public struct ButtonEnumerator
		{
			private readonly Buttons _pressed;

			private int _current;

			public Buttons Current
			{
				get
				{
					if (_current < 0 || _current > 32)
					{
						throw new InvalidOperationException();
					}
					return (Buttons)(1 << _current);
				}
			}

			public ButtonEnumerator(Buttons pressed)
			{
				_pressed = pressed;
				_current = -1;
			}

			public bool MoveNext()
			{
				if (_pressed == (Buttons)0)
				{
					return false;
				}
				while (_current < 31)
				{
					_current++;
					if (((int)_pressed & (1 << _current)) != 0)
					{
						return true;
					}
				}
				return false;
			}

			public void Reset()
			{
				_current = -1;
			}
		}

		private readonly Buttons _pressed;

		private readonly int _count;

		public int Count => _count;

		public ButtonCollection(ref GamePadState padState, ref GamePadState oldPadState)
		{
			_count = 0;
			_pressed = (Buttons)0;
			if (padState.IsButtonDown(Buttons.A) && !oldPadState.IsButtonDown(Buttons.A))
			{
				_pressed |= Buttons.A;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.B) && !oldPadState.IsButtonDown(Buttons.B))
			{
				_pressed |= Buttons.B;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.X) && !oldPadState.IsButtonDown(Buttons.X))
			{
				_pressed |= Buttons.X;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.Y) && !oldPadState.IsButtonDown(Buttons.Y))
			{
				_pressed |= Buttons.Y;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.Start) && !oldPadState.IsButtonDown(Buttons.Start))
			{
				_pressed |= Buttons.Start;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.Back) && !oldPadState.IsButtonDown(Buttons.Back))
			{
				_pressed |= Buttons.Back;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.RightTrigger) && !oldPadState.IsButtonDown(Buttons.RightTrigger))
			{
				_pressed |= Buttons.RightTrigger;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.LeftTrigger) && !oldPadState.IsButtonDown(Buttons.LeftTrigger))
			{
				_pressed |= Buttons.LeftTrigger;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.RightShoulder) && !oldPadState.IsButtonDown(Buttons.RightShoulder))
			{
				_pressed |= Buttons.RightShoulder;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.LeftShoulder) && !oldPadState.IsButtonDown(Buttons.LeftShoulder))
			{
				_pressed |= Buttons.LeftShoulder;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.DPadUp) && !oldPadState.IsButtonDown(Buttons.DPadUp))
			{
				_pressed |= Buttons.DPadUp;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.DPadRight) && !oldPadState.IsButtonDown(Buttons.DPadRight))
			{
				_pressed |= Buttons.DPadRight;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.DPadDown) && !oldPadState.IsButtonDown(Buttons.DPadDown))
			{
				_pressed |= Buttons.DPadDown;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.DPadLeft) && !oldPadState.IsButtonDown(Buttons.DPadLeft))
			{
				_pressed |= Buttons.DPadLeft;
				_count++;
			}
			if ((double)padState.ThumbSticks.Left.Y > 0.2 && (double)oldPadState.ThumbSticks.Left.Y <= 0.2 && Utility.thumbstickIsInDirection(0, padState))
			{
				_pressed |= Buttons.LeftThumbstickUp;
				_count++;
			}
			if ((double)padState.ThumbSticks.Left.X > 0.2 && (double)oldPadState.ThumbSticks.Left.X <= 0.2 && Utility.thumbstickIsInDirection(1, padState))
			{
				_pressed |= Buttons.LeftThumbstickRight;
				_count++;
			}
			if ((double)padState.ThumbSticks.Left.Y < -0.2 && (double)oldPadState.ThumbSticks.Left.Y >= -0.2 && Utility.thumbstickIsInDirection(2, padState))
			{
				_pressed |= Buttons.LeftThumbstickDown;
				_count++;
			}
			if ((double)padState.ThumbSticks.Left.X < -0.2 && (double)oldPadState.ThumbSticks.Left.X >= -0.2 && Utility.thumbstickIsInDirection(3, padState))
			{
				_pressed |= Buttons.LeftThumbstickLeft;
				_count++;
			}
		}

		public ButtonCollection(ref GamePadState padState)
		{
			_count = 0;
			_pressed = (Buttons)0;
			if (padState.IsButtonDown(Buttons.A))
			{
				_pressed |= Buttons.A;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.B))
			{
				_pressed |= Buttons.B;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.X))
			{
				_pressed |= Buttons.X;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.Y))
			{
				_pressed |= Buttons.Y;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.Start))
			{
				_pressed |= Buttons.Start;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.Back))
			{
				_pressed |= Buttons.Back;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.RightTrigger))
			{
				_pressed |= Buttons.RightTrigger;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.LeftTrigger))
			{
				_pressed |= Buttons.LeftTrigger;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.RightShoulder))
			{
				_pressed |= Buttons.RightShoulder;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.LeftShoulder))
			{
				_pressed |= Buttons.LeftShoulder;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.DPadUp))
			{
				_pressed |= Buttons.DPadUp;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.DPadRight))
			{
				_pressed |= Buttons.DPadRight;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.DPadDown))
			{
				_pressed |= Buttons.DPadDown;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.DPadLeft))
			{
				_pressed |= Buttons.DPadLeft;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.LeftThumbstickUp))
			{
				_pressed |= Buttons.LeftThumbstickUp;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.LeftThumbstickRight))
			{
				_pressed |= Buttons.LeftThumbstickRight;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.LeftThumbstickDown))
			{
				_pressed |= Buttons.LeftThumbstickDown;
				_count++;
			}
			if (padState.IsButtonDown(Buttons.LeftThumbstickLeft))
			{
				_pressed |= Buttons.LeftThumbstickLeft;
				_count++;
			}
		}

		public ButtonEnumerator GetEnumerator()
		{
			return new ButtonEnumerator(_pressed);
		}
	}
}
