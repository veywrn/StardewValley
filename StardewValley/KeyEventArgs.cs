using Microsoft.Xna.Framework.Input;
using System;

namespace StardewValley
{
	public class KeyEventArgs : EventArgs
	{
		private Keys keyCode;

		public Keys KeyCode => keyCode;

		public KeyEventArgs(Keys keyCode)
		{
			this.keyCode = keyCode;
		}
	}
}
