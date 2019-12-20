using Microsoft.Xna.Framework.Input;
using StardewValley.Events;
using System;
using xTile.Dimensions;

namespace StardewValley
{
	public class ModHooks
	{
		public virtual void OnGame1_PerformTenMinuteClockUpdate(Action action)
		{
			action();
		}

		public virtual void OnGame1_NewDayAfterFade(Action action)
		{
			action();
		}

		public virtual void OnGame1_ShowEndOfNightStuff(Action action)
		{
			action();
		}

		public virtual void OnGame1_UpdateControlInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, Action action)
		{
			action();
		}

		public virtual void OnGameLocation_ResetForPlayerEntry(GameLocation location, Action action)
		{
			action();
		}

		public virtual bool OnGameLocation_CheckAction(GameLocation location, Location tileLocation, Rectangle viewport, Farmer who, Func<bool> action)
		{
			return action();
		}

		public virtual FarmEvent OnUtility_PickFarmEvent(Func<FarmEvent> action)
		{
			return action();
		}
	}
}
